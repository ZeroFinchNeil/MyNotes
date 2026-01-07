param([switch]$RunAfterBuild = $false, [string]$SolutionDir, [string]$Configuration = "Debug", [string]$Platform = "x64")

$projectPath = "."
$objDebugPath = ".\obj\x64\Debug"
$binDebugPath = ".\bin\x64\Debug"
$appxPath = "$binDebugPath\AppX"

Push-Location
. "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\Launch-VsDevShell.ps1"
Pop-Location

if($RunAfterBuild) {
  dotnet build $SolutionDir -c $Configuration -p:Platform=$Platform
  if ($LASTEXITCODE -ne 0) {
    Write-Error "빌드 실패"
    exit $LASTEXITCODE
  }
}

# XBF
$sourceXbfPath = "$binDebugPath\MyNotes.Templates\Themes\Generic.xbf"
$appxXbfFolderPath = "$appxPath\MyNotes.Templates\Themes"
Copy-Item -Path "$SolutionDir\MyNotes.Templates\bin\x64\Debug\net10.0-windows10.0.26100.0\MyNotes.Templates\Themes\Generic.xbf" -Destination "$SolutionDir\MyNotes\bin\x64\Debug\net10.0-windows10.0.26100.0\MyNotes.Templates\Themes" -Force -ErrorAction SilentlyContinue
Copy-Item -Path $sourceXbfPath -Destination $appxXbfFolderPath -Force -ErrorAction SilentlyContinue

# PRI
$priConfigPath = "$objDebugPath\priconfig.xml"
$outputPriPath = "$binDebugPath\resources.pri"
$appxPriPath = "$appxPath\resources.pri"

Remove-Item $outputPriPath -ErrorAction SilentlyContinue
Remove-Item $appxPriPath -ErrorAction SilentlyContinue
makepri new /ProjectRoot $projectPath /ConfigXml $priConfigPath /OutputFile $outputPriPath /IndexName "ZeroFinchNeil.MyNotesbyZeroFinchNeil" /Verbose /Overwrite

if ($LASTEXITCODE -eq 0) {
  Write-Host "makepri 성공"
} else {
  Write-Host "makepri 실패 (코드: $LASTEXITCODE)"
  pause
}

# Get-ChildItem $appxPath -Recurse -Filter "*.xbf" -File | Remove-Item -ErrorAction SilentlyContinue