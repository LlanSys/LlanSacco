param(
    [switch]$SkipBuild,
    [switch]$SkipArchitecture,
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'
$verify = Join-Path $PSScriptRoot 'verify-baseline.ps1'

if (-not $SkipBuild) {
    & $verify -Check Api -Configuration Debug -NoRestore:$SkipRestore
}

if (-not $SkipArchitecture) {
    & $verify -Check Architecture -Configuration Debug -NoRestore:$SkipRestore
}

Write-Host 'Local pre-push checks passed.'
