param(
    [ValidateSet('All', 'Api', 'Blazor', 'Unit', 'Architecture', 'Integration', 'Guardrails')]
    [string]$Check = 'All',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$BaseRef,
    [switch]$NoRestore
)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$projects = [ordered]@{
    Api = 'src/Backend/Api/LS.Api/LS.Api.csproj'
    Blazor = 'src/Frontend/Web/LS.UI.Blazor/LS.UI.Blazor.csproj'
    Unit = 'tests/LS.Tests.Unit/LS.Tests.Unit.csproj'
    Architecture = 'tests/LS.Tests.Architecture/LS.Tests.Architecture.csproj'
    Integration = 'tests/LS.Tests.Integration/LS.Tests.Integration.csproj'
}
$failed = [System.Collections.Generic.List[string]]::new()
Push-Location $repoRoot
try {
    if ($Check -in @('All', 'Guardrails')) {
        if ($BaseRef) {
            & ./scripts/check-architecture-debt.ps1 -BaseRef $BaseRef
        }

        & ./scripts/check-package-policy.ps1
        & ./scripts/test-package-policy.ps1

        if (-not $NoRestore) {
            dotnet tool restore --configfile nuget.config
            if ($LASTEXITCODE -ne 0) { throw 'Local tool restore failed.' }
        }

        $formatArguments = @('run', '--project', 'tools/LS.SourceFormatting/LS.SourceFormatting.csproj',
            '--configuration', $Configuration, '-p:UseSharedCompilation=false')
        if ($NoRestore) { $formatArguments += '--no-restore' }
        & dotnet @formatArguments -- --self-test
        if ($LASTEXITCODE -ne 0) { throw 'Formatter fixtures failed.' }
    }
    elseif ($BaseRef) {
        throw 'Select All or Guardrails when comparing the debt register to a base ref.'
    }

    foreach ($entry in $projects.GetEnumerator()) {
        if ($Check -ne 'All' -and $entry.Key -ne $Check) { continue }
        $verb = if ($entry.Key -in @('Api', 'Blazor')) { 'build' } else { 'test' }
        $arguments = @($verb, $entry.Value, '--configuration', $Configuration, '-p:UseSharedCompilation=false', '--verbosity', 'minimal')
        if ($NoRestore) { $arguments += '--no-restore' }
        if ($verb -eq 'test') {
            $arguments += @('--logger', "trx;LogFileName=$($entry.Key).trx", '--results-directory', 'artifacts/baseline')
        }
        Write-Host "Running $($entry.Key) baseline..."
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) { $failed.Add($entry.Key) }
    }
} finally { Pop-Location }
if ($failed.Count -gt 0) { throw "Baseline failed: $($failed -join ', ')." }
Write-Host 'Selected baseline checks passed.'
