param(
    [ValidateSet('EditMode', 'PlayMode', 'All')][string]$Platform = 'All',
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Close Unity before batch tests. Do not open the same project twice.' }
if (-not (Test-Path -LiteralPath $UnityPath)) { throw "Unity not found: $UnityPath" }
if (-not (Test-Path -LiteralPath (Join-Path $root 'Assets/MaseiKivotos/Scenes/TurnSandbox.unity'))) { throw 'Create the sandbox scene first using the Makibo editor menu.' }
$null = New-Item -ItemType Directory -Path (Join-Path $root 'Logs') -Force
$platforms = if ($Platform -eq 'All') { @('EditMode', 'PlayMode') } else { @($Platform) }
foreach ($mode in $platforms) {
    # Unique output paths prevent a stale passing XML from being reused.
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
    $result = Join-Path $root "Logs/makibo-$mode-$stamp.xml"
    $log = Join-Path $root "Logs/makibo-$mode-$stamp.log"
    $arguments = @('-batchmode', '-projectPath', ('"' + $root + '"'), '-runTests', '-testPlatform', $mode,
        '-assemblyNames', "MaseiKivotos.${mode}Tests", '-testResults', ('"' + $result + '"'), '-logFile', ('"' + $log + '"'))
    # PlayMode uses a real graphics device to capture the actual uGUI canvas.
    if ($mode -eq 'EditMode') { $arguments += '-nographics' }
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
    Write-Output "$mode Unity PID $($process.Id); log: $log"
    $process.WaitForExit()
    if (-not (Test-Path -LiteralPath $result)) { throw "Unity returned $($process.ExitCode), but no test XML was produced. See $log" }
    [xml]$xml = Get-Content -Raw -LiteralPath $result
    $run = $xml.'test-run'
    Write-Output "$mode result=$($run.result) total=$($run.total) passed=$($run.passed) failed=$($run.failed) exit=$($process.ExitCode)"
    if ($process.ExitCode -ne 0 -or [int]$run.total -eq 0 -or $run.result -ne 'Passed' -or [int]$run.failed -ne 0) {
        throw "Test run did not pass. See $result and $log"
    }
}
