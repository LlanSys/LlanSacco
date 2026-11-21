[CmdletBinding()]
param([switch]$SkipStart)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$envFile = Join-Path $repoRoot 'ops/local/.env'
$composeFile = Join-Path $repoRoot 'ops/local/docker-compose.yml'
$testProject = Join-Path $repoRoot 'tests/LS.Tests.Integration/LS.Tests.Integration.csproj'

if (-not (Test-Path -LiteralPath $envFile)) {
    throw 'Local platform is not configured. Run ./scripts/setup-local-platform.ps1 first.'
}

$environment = @{}
foreach ($line in Get-Content -LiteralPath $envFile) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') {
        $environment[$matches[1].Trim()] = $matches[2].Trim()
    }
}

foreach ($required in @('RABBITMQ_USER', 'RABBITMQ_PASSWORD')) {
    if ([string]::IsNullOrWhiteSpace($environment[$required])) {
        throw "Local RabbitMQ setting '$required' is missing from $envFile."
    }
}

if (-not $SkipStart) {
    & (Join-Path $PSScriptRoot 'check-local-endpoints.ps1') -ComposeFiles $composeFile
    & docker compose --env-file $envFile -f $composeFile up -d --wait rabbitmq
    if ($LASTEXITCODE -ne 0) {
        throw "RabbitMQ startup failed with exit code $LASTEXITCODE."
    }
}

$endpoint = docker compose --env-file $envFile -f $composeFile port rabbitmq 5672
if ($LASTEXITCODE -ne 0 -or $endpoint -notmatch ':(\d+)$') {
    throw 'The LlanSacco RabbitMQ port is unavailable.'
}
$env:BT_RABBITMQ_PORT = $matches[1]
$env:BT_RUN_RABBITMQ_TESTS = 'true'
$env:BT_RABBITMQ_HOST = 'localhost'
$env:BT_RABBITMQ_VIRTUAL_HOST = '/'
$env:BT_RABBITMQ_USERNAME = $environment.RABBITMQ_USER
$env:BT_RABBITMQ_PASSWORD = $environment.RABBITMQ_PASSWORD

try {
    dotnet test $testProject --filter 'Category=ExternalRabbitMq' -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) {
        throw "RabbitMQ transport certification failed with exit code $LASTEXITCODE."
    }
}
finally {
    Remove-Item Env:BT_RUN_RABBITMQ_TESTS -ErrorAction SilentlyContinue
    Remove-Item Env:BT_RABBITMQ_PORT -ErrorAction SilentlyContinue
    Remove-Item Env:BT_RABBITMQ_HOST -ErrorAction SilentlyContinue
    Remove-Item Env:BT_RABBITMQ_VIRTUAL_HOST -ErrorAction SilentlyContinue
    Remove-Item Env:BT_RABBITMQ_USERNAME -ErrorAction SilentlyContinue
    Remove-Item Env:BT_RABBITMQ_PASSWORD -ErrorAction SilentlyContinue
}
