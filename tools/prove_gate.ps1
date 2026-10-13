param([string]$Dotnet = 'dotnet')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$source = Join-Path $repo 'src/CivicConnect.Web/Application/StatusTransitionService.cs'
$saved = [System.IO.File]::ReadAllBytes($source)
$log = Join-Path $repo 'docs/evidence/m3/gate-negative-control.txt'
try {
    $text = [System.Text.Encoding]::UTF8.GetString($saved)
    $original = 'target is "Assigned" or "Rejected"'
    if (-not $text.Contains($original)) { throw 'Expected State rule was not found; no probe performed.' }
    [System.IO.File]::WriteAllText($source, $text.Replace($original, 'target is "Assigned" or "Rejected" or "Closed"'))
    & $Dotnet build (Join-Path $repo 'tests/CivicConnect.M3Tests') --configuration Release --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Probe build failed before test execution.' }
    & $Dotnet run --project (Join-Path $repo 'tests/CivicConnect.M3Tests') --configuration Release --no-build 2>&1 | Tee-Object -FilePath $log
    $probeExit = $LASTEXITCODE
    Add-Content -LiteralPath $log -Value "NEGATIVE CONTROL: deliberately allowed New -> Closed; suite exit code $probeExit."
    if ($probeExit -eq 0) { throw 'Gate failed to detect the injected lifecycle defect.' }
} finally {
    [System.IO.File]::WriteAllBytes($source, $saved)
    & $Dotnet build (Join-Path $repo 'tests/CivicConnect.M3Tests') --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Restored source did not build.' }
}
Write-Output 'PASS gate probe detected forbidden transition; original source restored and rebuilt.'
