[CmdletBinding()]
param(
    # SQL Server connection string for an empty or existing database the check may migrate.
    [Parameter(Mandatory)][string]$SqlConnectionString,
    [string]$RabbitMqHost = 'localhost',
    [int]$RabbitMqPort = 5672,
    [Parameter(Mandatory)][string]$RabbitMqUser,
    [Parameter(Mandatory)][string]$RabbitMqPassword,
    [Parameter(Mandatory)][string]$RedisConnectionString,
    [int]$Port = 8199,
    [string]$Configuration = 'Release',
    [int]$TimeoutSeconds = 180,
    [switch]$SkipMigrations
)

# Starts the real API host with messaging and background jobs enabled and requires it to become
# healthy. This catches startup-only failures (dependency injection, messaging licence/bus
# configuration, migrations) that unit and integration tests do not exercise.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$api = 'src/Backend/Api/LS.Api/LS.Api.csproj'

# Every context-specific connection name points at the target database, so developer
# user-secrets cannot redirect a migration or the host to another server.
$connectionNames = @('DefaultConnection', 'DefaultSqlConnection', 'AccountingConnection', 'BankingConnection',
    'ControlPlaneConnection', 'HrConnection', 'IamConnection', 'MembershipConnection', 'SharedConnection')
foreach ($name in $connectionNames) { Set-Item "env:ConnectionStrings__$name" $SqlConnectionString }
$persistence = 'src/Backend/Persistence/LS.Persistence/LS.Persistence.csproj'

dotnet build $api --configuration $Configuration -p:UseSharedCompilation=false
if ($LASTEXITCODE -ne 0) { throw 'API build failed.' }

if (-not $SkipMigrations) {
    dotnet tool restore --configfile nuget.config
    if ($LASTEXITCODE -ne 0) { throw 'Tool restore failed.' }

    # Some machines restore local tools into a global packages folder that `dotnet ef` does not
    # probe. Fall back to the restored tool assembly at the pinned manifest version.
    $efCommand = @('ef')
    dotnet ef --version *> $null
    if ($LASTEXITCODE -ne 0) {
        $efVersion = (Get-Content '.config/dotnet-tools.json' -Raw | ConvertFrom-Json).tools.'dotnet-ef'.version
        $packages = ((dotnet nuget locals global-packages --list) -replace '^global-packages:\s*', '').Trim()
        $efDll = Get-ChildItem (Join-Path $packages "dotnet-ef/$efVersion/tools") -Recurse -Filter 'dotnet-ef.dll' | Select-Object -First 1
        if (-not $efDll) { throw "dotnet-ef $efVersion is not available after tool restore." }
        $efCommand = @($efDll.FullName)
    }

    $contexts = Get-Content 'ops/migrations/contexts.txt' | ForEach-Object { $_.Trim() } | Where-Object { $_ -and -not $_.StartsWith('#') }
    foreach ($context in $contexts) {
        Write-Host "Migrating ${context}SqlServerDBContext"
        dotnet @efCommand database update --no-build --configuration $Configuration --project $persistence --startup-project $api --context "${context}SqlServerDBContext"
        if ($LASTEXITCODE -ne 0) { throw "Migration failed for $context." }
    }
}

$environment = @{
    ASPNETCORE_ENVIRONMENT = 'Development'
    ASPNETCORE_URLS = "http://localhost:$Port"
    JwtSettings__SecretKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
    IamProvisioning__TemporaryPassword = "Startup!$([Guid]::NewGuid().ToString('N').Substring(0, 12))Aa1"
    DevelopmentSeed__AdminPassword = "Startup!$([Guid]::NewGuid().ToString('N').Substring(0, 12))Aa1"
    BackgroundJobs__Enabled = 'true'
    Messaging__Enabled = 'true'
    Messaging__Transport = 'RabbitMq'
    Messaging__RabbitMq__Host = $RabbitMqHost
    Messaging__RabbitMq__Port = "$RabbitMqPort"
    Messaging__RabbitMq__VirtualHost = '/'
    Messaging__RabbitMq__Username = $RabbitMqUser
    Messaging__RabbitMq__Password = $RabbitMqPassword
    CacheSettings__Provider = 'Redis'
    CacheSettings__Redis__ConnectionString = $RedisConnectionString
    ProfileImageStorage__Provider = 'Local'
    # The check must not depend on developer user-secrets or external providers.
    EmailSettings__Provider = 'NoOp'
}

$dll = "src/Backend/Api/LS.Api/bin/$Configuration/net10.0/LS.Api.dll"
# The API writes straight to this console (no redirection), so its full startup log, including any
# fatal exception, appears in the CI job output in order.
$startInfo = [Diagnostics.ProcessStartInfo]::new('dotnet', "`"$(Join-Path $repoRoot $dll)`"")
$startInfo.WorkingDirectory = Join-Path $repoRoot 'src/Backend/Api/LS.Api'
$startInfo.UseShellExecute = $false
foreach ($key in $environment.Keys) { $startInfo.Environment[$key] = $environment[$key] }
# Hide developer user-secrets so a local run sees only committed configuration, exactly like CI.
$isolatedProfile = Join-Path ([IO.Path]::GetTempPath()) "llansacco-startup-profile-$Port"
New-Item -ItemType Directory -Force $isolatedProfile | Out-Null
$startInfo.Environment['APPDATA'] = $isolatedProfile
$startInfo.Environment['HOME'] = $isolatedProfile
foreach ($name in $connectionNames) { $startInfo.Environment["ConnectionStrings__$name"] = $SqlConnectionString }

$process = [Diagnostics.Process]::Start($startInfo)

$healthy = $false
$lastReport = 'no health response received'
$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
try {
    while ([DateTime]::UtcNow -lt $deadline) {
        if ($process.HasExited) { break }
        try {
            $live = Invoke-WebRequest "http://localhost:$Port/health" -UseBasicParsing -TimeoutSec 5 -SkipHttpErrorCheck
            $ready = Invoke-WebRequest "http://localhost:$Port/health/ready" -UseBasicParsing -TimeoutSec 5 -SkipHttpErrorCheck
            $lastReport = "/health $($live.StatusCode): $($live.Content)  /health/ready $($ready.StatusCode)"
            if ($live.StatusCode -eq 200 -and $ready.StatusCode -eq 200) {
                # Stay up long enough for hosted services (bus, outbox workers, jobs) to start.
                Start-Sleep -Seconds 10
                if (-not $process.HasExited) { $healthy = $true }
                break
            }
        }
        catch { }
        Start-Sleep -Seconds 2
    }
}
finally {
    $exitedEarly = $process.HasExited
    $exitCode = if ($exitedEarly) { $process.ExitCode } else { $null }
    if (-not $process.HasExited) { $process.Kill($true); $process.WaitForExit(10000) | Out-Null }
}

if (-not $healthy) {
    $reason = if ($exitedEarly) { "the API process exited with code $exitCode (see its log above)" } else { "it did not report healthy within $TimeoutSeconds seconds" }
    throw "API did not start healthy with messaging enabled: $reason. Last health report: $lastReport"
}

Write-Host 'PASS: API started healthy with RabbitMQ messaging and background jobs enabled.'
