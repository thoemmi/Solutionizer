$SolutionDir = split-path -parent $PSCommandPath
$wixdir = Join-Path (Join-Path $SolutionDir "assets") "wix.3.7"
$TargetDir = Join-Path $SolutionDir "publish"
$WxsFile = Join-Path $SolutionDir "Solutionizer.wxs"

$solutionizerExe = Join-Path $SolutionDir "Solutionizer/bin/Debug/Solutionizer.exe"
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($solutionizerExe).ProductVersion

if (!(Test-Path $TargetDir)) {
    New-Item -ItemType Directory -Path $TargetDir
}

Push-Location $SolutionDir
Invoke-Expression "$(Join-Path $wixdir 'candle.exe') -out '$TargetDir\Solutionizer.wixobj' '$WxsFile'"
Invoke-Expression "$(Join-Path $wixdir 'light.exe') -ext WixNetFxExtension -ext WixUIExtension -ext WixUtilExtension -out '$TargetDir\Solutionizer-$Version.msi' '$TargetDir\Solutionizer.wixobj'"
Pop-Location