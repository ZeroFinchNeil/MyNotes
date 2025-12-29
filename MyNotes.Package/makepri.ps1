param([switch]$RunAfterBuild = $false, [string]$SolutionDir, [string]$Configuration = "Debug", [string]$Platform = "x64")

$projectPath = "."
$debugPath = ".\obj\x64\Debug"
$configPath = ".\obj\x64\Debug\priconfig.xml"
$outputPath = ".\bin\x64\Debug\resources.pri"
$bidDebugPath = ".\bin\x64\Debug"
$appxPath = ".\bin\x64\Debug\AppX"

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

Remove-Item $outputPath -ErrorAction SilentlyContinue
Remove-Item "$appxPath\resources.pri" -ErrorAction SilentlyContinue
Copy-Item "$bidDebugPath\MyNotes.Templates\Themes\Generic.xbf" -Destination "$appxPath\MyNotes.Templates\Themes" -Force -ErrorAction SilentlyContinue
#Get-ChildItem $appxPath -Recurse -Filter "*.pri" -File | Remove-Item -ErrorAction SilentlyContinue
#Get-ChildItem $appxPath -Recurse -Filter "*.xbf" -File | Remove-Item -ErrorAction SilentlyContinue
makepri new /pr $projectPath /cf $configPath /of $outputPath /overwrite

if ($LASTEXITCODE -eq 0) {
  Write-Host "makepri 성공"
} else {
  Write-Host "makepri 실패 (코드: $LASTEXITCODE)"
}