[CmdletBinding()]
param(
    [switch]$Check,
    [switch]$WrapLongLines,
    [string[]]$Include = @()
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$layoutProject = 'tools/LS.SourceFormatting/LS.SourceFormatting.csproj'
$sourceRoots = @('src', 'tests', 'tools')
$defaultScope = @('src/Backend', 'src/Frontend/Shared', 'src/Frontend/Web', 'src/Shared', 'tests', 'tools')
$diagnostics = @(
    'IDE0011', 'IDE0022', 'IDE2000', 'IDE2003',
    'RCS0010', 'RCS0021', 'RCS0023', 'RCS0030', 'RCS0033',
    'RCS0053', 'RCS0054', 'RCS0060'
)

function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($args[0]) failed with exit code $LASTEXITCODE. Review the output above."
    }
}

Push-Location $repoRoot
try {
    $paths = if ($Include.Count) { $Include } else { $defaultScope }
    $selected = @(foreach ($path in $paths) {
        $item = Get-Item -LiteralPath $path
        $relative = [IO.Path]::GetRelativePath($repoRoot, $item.FullName).Replace('\', '/')
        if ($relative.Split('/')[0] -notin $sourceRoots) {
            throw "Select a path under src, tests, or tools: $path"
        }
        if (-not $item.PSIsContainer -and $item.Extension -notin @('.cs', '.razor')) {
            throw "Select a C# or Razor source file: $path"
        }
        $relative
    })

    [xml]$solution = Get-Content -LiteralPath 'LlanSacco.slnx' -Raw
    foreach ($project in @($solution.SelectNodes('//Project[@Path]'))) {
        $projectPath = $project.GetAttribute('Path')
        $directory = [IO.Path]::GetDirectoryName($projectPath).Replace('\', '/')
        $ownsSelection = $selected | Where-Object {
            $_ -eq $directory -or $_.StartsWith("$directory/", [StringComparison]::OrdinalIgnoreCase) -or
            $directory.StartsWith("$_/", [StringComparison]::OrdinalIgnoreCase)
        }
        if (!$ownsSelection) { $null = $project.ParentNode.RemoveChild($project) }
    }
    if ($solution.SelectNodes('//Project[@Path]').Count -eq 0) {
        throw 'The selected paths do not belong to a LlanSacco project.'
    }
    $temporarySolution = Join-Path $repoRoot ('.format-' + [guid]::CreateVersion7().ToString('N') + '.slnx')
    $solution.Save($temporarySolution)

    $migrationRoots = Get-ChildItem 'src/Backend/Persistence' -Directory -Recurse -Filter Migrations |
        ForEach-Object { [IO.Path]::GetRelativePath($repoRoot, $_.FullName).Replace('\', '/') }
    $nativeScope = @('--include') + $selected + @('--exclude') + @($migrationRoots)
    $layoutScope = @('--include') + $selected
    if ($Check) { $nativeScope = @('--verify-no-changes') + $nativeScope }
    if ($WrapLongLines) { $diagnostics += 'RCS0056' }

    Invoke-DotNet restore $temporarySolution --configfile nuget.config
    Invoke-DotNet restore $layoutProject --configfile nuget.config
    Invoke-DotNet build $layoutProject --configuration Release --no-restore --verbosity quiet '-p:UseSharedCompilation=false'
    Invoke-DotNet run --project $layoutProject --configuration Release --no-build --no-restore -- --self-test

    if (-not $Check) {
        Invoke-DotNet run --project $layoutProject --configuration Release --no-build --no-restore -- --fix @layoutScope
    }

    Invoke-DotNet format whitespace $temporarySolution --no-restore @nativeScope
    Invoke-DotNet format $temporarySolution --no-restore --severity info --diagnostics @diagnostics @nativeScope

    if (-not $Check) {
        Invoke-DotNet format whitespace $temporarySolution --no-restore @nativeScope
        Invoke-DotNet run --project $layoutProject --configuration Release --no-build --no-restore -- --fix @layoutScope
    }
    Invoke-DotNet run --project $layoutProject --configuration Release --no-build --no-restore -- --check @layoutScope
}
finally {
    if ($temporarySolution -and (Test-Path -LiteralPath $temporarySolution)) {
        Remove-Item -LiteralPath $temporarySolution
    }
    Pop-Location
}
