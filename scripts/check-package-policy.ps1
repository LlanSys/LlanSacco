[CmdletBinding()]
param([string]$RepoRoot = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
$issues = [Collections.Generic.List[string]]::new()

function Read-PolicyXml([string]$path) {
    [xml](Get-Content -LiteralPath (Join-Path $RepoRoot $path) -Raw)
}

$central = Read-PolicyXml 'Directory.Packages.props'
foreach ($setting in @{
    ManagePackageVersionsCentrally = 'true'
    CentralPackageVersionOverrideEnabled = 'false'
    CentralPackageTransitivePinningEnabled = 'false'
}.GetEnumerator()) {
    $node = $central.SelectSingleNode("/Project/PropertyGroup/$($setting.Key)")
    if ($null -eq $node -or $node.InnerText -ne $setting.Value) {
        $issues.Add("Set $($setting.Key)=$($setting.Value) in Directory.Packages.props.")
    }
}

$massTransit = $central.SelectSingleNode('/Project/PropertyGroup/MassTransitVersion').InnerText
if ($massTransit -notmatch '^8\.\d+\.\d+$') {
    $issues.Add('MassTransit must retain its approved Apache-2.0 8.x pin; v9 is commercially licensed.')
}
$versions = @{}
foreach ($version in $central.SelectNodes('/Project/ItemGroup/PackageVersion')) {
    $name = $version.GetAttribute('Include')
    $value = $version.GetAttribute('Version')
    if (!$name -or $versions.ContainsKey($name)) {
        $issues.Add("Missing or duplicate central package identity $name.")
        continue
    }
    $versions[$name] = $value
    if ($name -match '^MassTransit($|\.)') {
        if ($value -cne '[$(MassTransitVersion)]') { $issues.Add("$name must use the exact shared MassTransit pin.") }
    }
    elseif ($value -notmatch '^(\d+\.\d+\.\d+(\.\d+)?|\[\d+\.\d+\.\d+\])$') {
        $issues.Add("$name must use an explicit stable version.")
    }
}

$nuget = Read-PolicyXml 'nuget.config'
$sources = @($nuget.SelectNodes('/configuration/packageSources/*'))
$mappings = @($nuget.SelectNodes('/configuration/packageSourceMapping/*'))
if ($sources.Count -ne 2 -or $sources[0].Name -ne 'clear' -or $sources[1].Name -ne 'add' -or
    $sources[1].GetAttribute('key') -ne 'nuget.org' -or
    $sources[1].GetAttribute('value') -ne 'https://api.nuget.org/v3/index.json' -or
    $mappings.Count -ne 2 -or $mappings[0].Name -ne 'clear' -or $mappings[1].Name -ne 'packageSource' -or
    $mappings[1].GetAttribute('key') -ne 'nuget.org' -or $mappings[1].ChildNodes.Count -ne 1 -or
    $null -eq $mappings[1].SelectSingleNode('package[@pattern="*"]') -or
    $null -ne $nuget.SelectSingleNode('/configuration/packageSourceCredentials')) {
    $issues.Add('Configure NuGet.org as the sole source and clear inherited sources and mappings.')
}

$sourceRoots = @('src', 'tests', 'tools') | ForEach-Object { Join-Path $RepoRoot $_ }
$activeRoots = $sourceRoots | Where-Object { Test-Path -LiteralPath $_ }
$projects = @(Get-ChildItem -LiteralPath $activeRoots -Recurse -File -Filter '*.csproj' |
    Where-Object FullName -NotMatch '[/\\](bin|obj|artifacts|TestResults)[/\\]')
$ignored = '[/\\](bin|obj|artifacts|TestResults)[/\\]'
$nested = @(Get-ChildItem -LiteralPath $activeRoots -Recurse -File -Filter 'Directory.Packages.props' |
    Where-Object FullName -NotMatch $ignored)
foreach ($file in $nested) {
    $issues.Add("Use the repository's central manifest instead of $([IO.Path]::GetRelativePath($RepoRoot, $file.FullName)).")
}
$build = Read-PolicyXml 'Directory.Build.props'
if ($null -eq $build.SelectSingleNode('/Project/PropertyGroup/NuGetAudit[.="true"]') -or
    $null -eq $build.SelectSingleNode('/Project/PropertyGroup/NuGetAuditMode[.="all"]')) {
    $issues.Add('Directory.Build.props must audit both direct and transitive NuGet packages.')
}

$props = Get-Item -LiteralPath (Join-Path $RepoRoot 'Directory.Build.props')
foreach ($file in @($props) + $projects) {
    $relative = [IO.Path]::GetRelativePath($RepoRoot, $file.FullName)
    [xml]$document = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($reference in $document.SelectNodes('//PackageReference')) {
        $name = $reference.GetAttribute('Include')
        if (!$name) { $name = $reference.GetAttribute('Update') }
        if ($reference.HasAttribute('Version') -or $reference.HasAttribute('VersionOverride') -or
            $null -ne $reference.SelectSingleNode('Version | VersionOverride')) {
            $issues.Add("$relative has a per-project version or override for $name.")
        }
        if ($name -and !$versions.ContainsKey($name)) {
            $issues.Add("$relative references $name without a central version.")
        }
    }
    foreach ($download in $document.SelectNodes('//PackageDownload')) {
        if ($download.GetAttribute('Include') -ne 'PinguApps.RazorStyle.Cli' -or
            $download.GetAttribute('Version') -cne '[$(RazorStyleVersion)]') {
            $issues.Add("$relative has a package download outside the reviewed formatter pin.")
        }
    }
    if ($null -ne $document.SelectSingleNode('//ManagePackageVersionsCentrally[.="false"] | //CentralPackageVersionOverrideEnabled[.="true"] | //NuGetAudit[.="false"] | //NuGetAuditSuppress')) {
        $issues.Add("$relative bypasses the central package or NuGet audit policy.")
    }
}

foreach ($dockerfile in 'ops/docker/api.Dockerfile', 'ops/docker/blazor.Dockerfile') {
    $content = Get-Content -LiteralPath (Join-Path $RepoRoot $dockerfile) -Raw
    $manifest = $content.IndexOf('COPY ["Directory.Packages.props", "."]', [StringComparison]::Ordinal)
    $restore = $content.IndexOf('RUN dotnet restore', [StringComparison]::Ordinal)
    if ($manifest -lt 0 -or $restore -lt $manifest) {
        $issues.Add("$dockerfile must copy the central manifest before restore.")
    }
}

if ($issues.Count) {
    throw ("Package policy failed:`n - " + ($issues -join "`n - "))
}
Write-Output "Package policy passed for $($projects.Count) projects and $($versions.Count) central versions."
