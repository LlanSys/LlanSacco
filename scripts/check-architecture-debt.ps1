param([Parameter(Mandatory)][string]$BaseRef)
$ErrorActionPreference = 'Stop'
$register = 'docs/architecture/architecture-debt.json'
$baseTree = git ls-tree --name-only $BaseRef -- $register
if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect the PR base.' }
if (-not $baseTree) { Write-Host 'Initial debt register: review every entry in this bootstrap PR.'; exit 0 }
$previousJson = git show "${BaseRef}:$register"
if ($LASTEXITCODE -ne 0) { throw 'Cannot read the base debt register.' }
$previous = ($previousJson -join "`n") | ConvertFrom-Json
$current = Get-Content $register -Raw | ConvertFrom-Json
$identities = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($entry in $previous) { [void]$identities.Add("$($entry.Rule)|$($entry.Path)|$($entry.Symbol)|$($entry.Detail)|$($entry.Phase)") }
$added = @($current | Where-Object { -not $identities.Contains("$($_.Rule)|$($_.Path)|$($_.Symbol)|$($_.Detail)|$($_.Phase)") })
if ($added.Count) { $added | Select-Object Rule,Path,Symbol,Phase | Format-Table; throw 'Debt cannot grow or be deferred to another phase. Fix new violations; do not expand the baseline.' }
Write-Host 'Architecture debt does not grow against the PR base.'
