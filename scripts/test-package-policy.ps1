[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$checker = Join-Path $PSScriptRoot 'check-package-policy.ps1'
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixture = Join-Path $tempRoot ('ls-package-policy-' + [guid]::CreateVersion7().ToString('N'))
[IO.Directory]::CreateDirectory($fixture) | Out-Null
try {
    foreach ($file in 'Directory.Packages.props', 'Directory.Build.props', 'nuget.config',
        'ops/docker/api.Dockerfile', 'ops/docker/blazor.Dockerfile') {
        $target = Join-Path $fixture $file
        [IO.Directory]::CreateDirectory((Split-Path -Parent $target)) | Out-Null
        Copy-Item -LiteralPath (Join-Path $repo $file) -Destination $target
    }
    $project = 'src/Fixture/Fixture.csproj'
    $projectFile = Join-Path $fixture $project
    [IO.Directory]::CreateDirectory((Split-Path -Parent $projectFile)) | Out-Null
    [IO.File]::WriteAllText($projectFile, '<Project><ItemGroup><PackageReference Include="FluentValidation" /></ItemGroup></Project>')
    & $checker -RepoRoot $fixture

    $cases = @(
        @{ File = $project; From = 'Include="FluentValidation"'; To = 'Include="FluentValidation" Version="12.2.0"'; Error = 'per-project version or override' }
        @{ File = $project; From = 'Include="FluentValidation"'; To = 'Include="Unregistered.Package"'; Error = 'without a central version' }
        @{ File = 'Directory.Packages.props'; From = 'Include="MassTransit.RabbitMQ" Version="[$(MassTransitVersion)]"'; To = 'Include="MassTransit.RabbitMQ" Version="9.2.4"'; Error = 'exact shared MassTransit pin' }
        @{ File = 'nuget.config'; From = 'https://api.nuget.org/v3/index.json'; To = 'https://nuget.pkg.github.com/example/index.json'; Error = 'sole source' }
        @{ File = 'Directory.Build.props'; From = '<NuGetAudit>true</NuGetAudit>'; To = '<NuGetAudit>false</NuGetAudit>'; Error = 'bypasses the central package or NuGet audit policy' }
        @{ File = 'ops/docker/api.Dockerfile'; From = 'COPY ["Directory.Packages.props", "."]'; To = '# Missing central manifest'; Error = 'must copy the central manifest before restore' }
    )
    foreach ($case in $cases) {
        $file = Join-Path $fixture $case.File
        $original = [IO.File]::ReadAllText($file)
        if (!$original.Contains($case.From)) {
            throw "The fixture no longer contains the expected text in $($case.File)."
        }
        [IO.File]::WriteAllText($file, $original.Replace($case.From, $case.To))
        $errorMessage = $null
        try { & $checker -RepoRoot $fixture } catch { $errorMessage = $_.Exception.Message }
        finally { [IO.File]::WriteAllText($file, $original) }
        if (!$errorMessage -or !$errorMessage.Contains($case.Error)) {
            throw "Expected rejection '$($case.Error)' for $($case.File), got '$errorMessage'."
        }
    }
    Write-Output "Package policy accepted the baseline and rejected $($cases.Count) invalid configurations."
}
finally {
    $resolved = (Resolve-Path -LiteralPath $fixture).Path
    if ((Split-Path -Parent $resolved).TrimEnd([IO.Path]::DirectorySeparatorChar) -ne
        $tempRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) -or
        (Split-Path -Leaf $resolved) -notlike 'ls-package-policy-*' -or
        (Get-ChildItem -LiteralPath $resolved -Recurse -Force -Attributes ReparsePoint)) {
        throw 'Unexpected fixture cleanup path.'
    }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
