[CmdletBinding()]
param(
    [switch]$IncludeSqlServer,
    [switch]$IncludeSeq,
    [switch]$SkipPull
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $repoRoot 'ops/local/docker-compose.yml'
$envFile = Join-Path $repoRoot 'ops/local/.env'
$apiProject = Join-Path $repoRoot 'src/Backend/Api/LS.Api/LS.Api.csproj'

function New-LocalSecret {
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $bytes = New-Object byte[] 24
        $rng.GetBytes($bytes)
        return [BitConverter]::ToString($bytes).Replace("-", "")
    } finally {
        $rng.Dispose()
    }
}

function New-LocalBase64Secret {
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $bytes = New-Object byte[] 32
        $rng.GetBytes($bytes)
        return [Convert]::ToBase64String($bytes)
    } finally {
        $rng.Dispose()
    }
}

function Read-EnvironmentFile([string]$path) {
    $values = @{}
    foreach ($line in Get-Content -LiteralPath $path) {
        if ($line -match '^\s*([^#][^=]*)=(.*)$') {
            $values[$matches[1].Trim()] = $matches[2].Trim()
        }
    }

    return $values
}

if (-not (Test-Path -LiteralPath $envFile)) {
    $rabbitPassword = New-LocalSecret
    $redisPassword = New-LocalSecret
    $sqlPassword = "Ls!$(New-LocalSecret)"

    @(
        'RABBITMQ_USER=lsdev'
        "RABBITMQ_PASSWORD=$rabbitPassword"
        "REDIS_PASSWORD=$redisPassword"
        'REDIS_HOST_PORT=6381'
        "MSSQL_SA_PASSWORD=$sqlPassword"
        'AZURITE_ACCOUNT_NAME=lsdevstorage'
        "AZURITE_ACCOUNT_KEY=$(New-LocalBase64Secret)"
    ) | Set-Content -LiteralPath $envFile -Encoding ascii
}

$environment = Read-EnvironmentFile $envFile
if (-not $environment.ContainsKey('REDIS_HOST_PORT')) {
    Add-Content -LiteralPath $envFile -Value 'REDIS_HOST_PORT=6381'
    $environment = Read-EnvironmentFile $envFile
}

if (-not $environment.ContainsKey('AZURITE_ACCOUNT_NAME')) {
    Add-Content -LiteralPath $envFile -Value 'AZURITE_ACCOUNT_NAME=btdevstorage'
}
if (-not $environment.ContainsKey('AZURITE_ACCOUNT_KEY')) {
    Add-Content -LiteralPath $envFile -Value "AZURITE_ACCOUNT_KEY=$(New-LocalBase64Secret)"
}
$environment = Read-EnvironmentFile $envFile

$requiredKeys = @(
    'RABBITMQ_USER',
    'RABBITMQ_PASSWORD',
    'REDIS_PASSWORD',
    'REDIS_HOST_PORT',
    'MSSQL_SA_PASSWORD',
    'AZURITE_ACCOUNT_NAME',
    'AZURITE_ACCOUNT_KEY'
)
foreach ($key in $requiredKeys) {
    if ([string]::IsNullOrWhiteSpace($environment[$key])) {
        throw "Local platform setting '$key' is missing from $envFile."
    }
}

$volumeNames = @(
    'llansacco_rabbitmq-data',
    'llansacco_redis-data',
    'llansacco_seq-data',
    'llansacco_azurite-data',
    'llansacco_sqlserver-data'
)
foreach ($volumeName in $volumeNames) {
    & docker volume inspect $volumeName *> $null
    if ($LASTEXITCODE -ne 0) {
        & docker volume create $volumeName *> $null
        if ($LASTEXITCODE -ne 0) {
            throw "Could not create Docker volume '$volumeName'."
        }
    }
}

dotnet user-secrets set 'Messaging:Enabled' 'true' --project $apiProject
dotnet user-secrets set 'Messaging:Transport' 'RabbitMq' --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:Host' 'localhost' --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:Port' '5673' --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:VirtualHost' '/' --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:Username' $environment.RABBITMQ_USER --project $apiProject
dotnet user-secrets set 'Messaging:RabbitMq:Password' $environment.RABBITMQ_PASSWORD --project $apiProject
dotnet user-secrets set 'CacheSettings:Provider' 'Redis' --project $apiProject
dotnet user-secrets set 'CacheSettings:Redis:ConnectionString' "localhost:$($environment.REDIS_HOST_PORT),password=$($environment.REDIS_PASSWORD),abortConnect=false" --project $apiProject
dotnet user-secrets set 'EmailSettings:Provider' 'LocalMailpit' --project $apiProject
dotnet user-secrets set 'EmailSettings:LocalMailpit:Host' 'localhost' --project $apiProject
dotnet user-secrets set 'EmailSettings:LocalMailpit:Port' '1026' --project $apiProject
dotnet user-secrets remove 'EmailSettings:Host' --project $apiProject *> $null
dotnet user-secrets remove 'EmailSettings:Port' --project $apiProject *> $null
dotnet user-secrets remove 'EmailSettings:Username' --project $apiProject *> $null
dotnet user-secrets remove 'EmailSettings:Password' --project $apiProject *> $null
dotnet user-secrets remove 'EmailSettings:EnableSsl' --project $apiProject *> $null
dotnet user-secrets remove 'EmailSettings:UseAuthentication' --project $apiProject *> $null
dotnet user-secrets remove 'SmtpSettings:Password' --project $apiProject *> $null
dotnet user-secrets set 'ProfileImageStorage:Provider' 'Azurite' --project $apiProject
dotnet user-secrets set 'ProfileImageStorage:Azurite:ConnectionString' 'DefaultEndpointsProtocol=http;AccountName=lsdevstorage;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10010/lsdevstorage;QueueEndpoint=http://127.0.0.1:10011/lsdevstorage;TableEndpoint=http://127.0.0.1:10012/lsdevstorage;' --project $apiProject
dotnet user-secrets set 'ProfileImageStorage:Azurite:ContainerName' 'profile-images' --project $apiProject

$profiles = @()
if ($IncludeSqlServer) {
    $profiles += @('--profile', 'database')
    $sqlConnection = "Server=localhost,14334;Database=LS;User Id=sa;Password=$($environment.MSSQL_SA_PASSWORD);Encrypt=True;TrustServerCertificate=True;"
    dotnet user-secrets set 'ConnectionStrings:DefaultConnection' $sqlConnection --project $apiProject
}
if ($IncludeSeq) {
    $profiles += @('--profile', 'observability')
}

$composeArguments = @('compose', '--env-file', $envFile, '-f', $composeFile)
$composeArguments += $profiles
if (-not $SkipPull) {
    & docker @composeArguments pull
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose pull failed with exit code $LASTEXITCODE."
    }
}

& docker @composeArguments up -d --wait
if ($LASTEXITCODE -ne 0) {
    throw "Docker Compose startup failed with exit code $LASTEXITCODE."
}

Write-Host ''
Write-Host 'Local platform is ready:'
Write-Host '  RabbitMQ:  amqp://localhost:5673'
Write-Host '  Management: http://localhost:15673'
Write-Host "  Redis:      localhost:$($environment.REDIS_HOST_PORT)"
Write-Host '  Mailpit:    http://localhost:8026'
if ($IncludeSeq) { Write-Host '  Seq:        http://localhost:5342' }
Write-Host '  Azurite:    http://localhost:10010'
if ($IncludeSqlServer) { Write-Host '  SQL Server: localhost,14334' }
