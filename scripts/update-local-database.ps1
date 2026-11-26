<#
.SYNOPSIS
    Reports, and with -Apply runs, pending EF Core migrations for every bounded context against the local
    PostgreSQL database. The API does not migrate on startup, so run this after pulling changes that add migrations.

.EXAMPLE
    ./scripts/update-local-database.ps1                        # status, using the API's user secrets / environment
.EXAMPLE
    ./scripts/update-local-database.ps1 -LocalCompose -Apply   # upgrade the ops/local Docker stack's database
#>
[CmdletBinding()]
param(
    [switch]$Apply,
    # Target the ops/local Docker stack's PostgreSQL, reading its credentials from ops/local/.env.
    [switch]$LocalCompose
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$operation = if ($Apply) { 'local-upgrade' } else { 'local-status' }

Push-Location $repoRoot
$previousConnection = $env:LS_DATABASE_CONNECTION
try {
    if ($LocalCompose) {
        $envFile = Join-Path $repoRoot 'ops/local/.env'
        if (-not (Test-Path $envFile)) { throw 'ops/local/.env was not found. Create it from the local platform setup first.' }

        $values = @{}
        foreach ($line in Get-Content $envFile) {
            if ($line -match '^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)$') { $values[$Matches[1]] = $Matches[2].Trim().Trim('"') }
        }
        foreach ($required in 'POSTGRES_USER', 'POSTGRES_PASSWORD') {
            if (-not $values[$required]) { throw "ops/local/.env does not set $required." }
        }

        # ops/local/docker-compose.yml publishes PostgreSQL on 5436 with database 'llansacco'.
        $database = if ($values['POSTGRES_DB']) { $values['POSTGRES_DB'] } else { 'llansacco' }
        $env:LS_DATABASE_CONNECTION = "Host=localhost;Port=5436;Database=$database;Username=$($values['POSTGRES_USER']);Password=$($values['POSTGRES_PASSWORD'])"
    }

    & dotnet run --project tools/LS.DatabaseTools -- $operation
    if ($LASTEXITCODE -ne 0) {
        throw 'The local database workflow failed. Review the diagnostics above.'
    }
}
finally {
    $env:LS_DATABASE_CONNECTION = $previousConnection
    Pop-Location
}
