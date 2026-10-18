param([string]$Dotnet = 'dotnet', [string]$LogPath)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$source = Join-Path $repo 'src/CivicConnect.Web/Lifecycle/RequestState.cs'
$saved = [System.IO.File]::ReadAllBytes($source)
$log = if ($LogPath) { $LogPath } else { Join-Path $repo 'docs/evidence/m4-cameron-20261018/gate-negative-control.txt' }
New-Item -ItemType Directory -Path (Split-Path $log) -Force | Out-Null
try {
    $text = [System.Text.Encoding]::UTF8.GetString($saved)
    $original = '[AssignedState.Instance, RejectedState.Instance];'
    if (-not $text.Contains($original)) { throw 'Expected State rule was not found; no probe performed.' }
    [System.IO.File]::WriteAllText($source, $text.Replace($original, '[AssignedState.Instance, RejectedState.Instance, ClosedState.Instance];'))
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
& $Dotnet run --project (Join-Path $repo 'tests/CivicConnect.M3Tests') --configuration Release --no-build 2>&1 | Tee-Object -FilePath $log -Append
if ($LASTEXITCODE -ne 0) { throw 'Restored component checks did not pass.' }
Write-Output 'PASS gate probe detected forbidden transition; original source restored and rebuilt.'
