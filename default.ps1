Framework "4.0"

Properties {
    $build_dir = Split-Path $psake.build_script_file
    $build_artifacts_dir = "$build_dir\build\"
    $solution_file = "$build_dir\solutionizer.sln"
}

task Default -depends Clean, Compile

task CreateAssemblyInfo {
    $gittag = & git describe --tags --long
    $gittag

    if (!($gittag -match '^v(?<major>\d+)\.(?<minor>\d+)(\.(?<patch>\d+))?-(?<revision>\d+)-(?<commit>[a-z0-9]+)$')){
        throw "$gittag is not recognized"
    }
    $majorVersion = $matches['major']
    $minorVersion = $matches['minor']
    $patchVersion = $matches['patch']
    $revisionCount = $matches['revision']
    $commitVersion = $matches['commit']

    Write-Host "Current version: $majorVersion.$minorVersion.$patchVersion.$revisionCount ($commitVersion)"

    $version = "$majorVersion.$minorVersion.$patchVersion.$revisionCount"
    $fileversion = "$majorVersion.$minorVersion.$patchVersion.$revisionCount"
    $asmInfo = "using System.Reflection;

[assembly: AssemblyVersion(""$majorVersion.$minorVersion.0"")]
[assembly: AssemblyInformationalVersion(""$majorVersion.$minorVersion.$patchVersion.$revisionCount ($commitVersion)"")]
[assembly: AssemblyFileVersion(""$majorVersion.$minorVersion.$patchVersion.$revisionCount"")]"

    $file = Join-Path $build_dir "CommonAssemblyInfo.cs"
    Write-Host "Generating assembly info file $file"
    Write-Output $asmInfo > $file
}

task Compile -depends CreateAssemblyInfo {
    Write-Host "Building $solution_file" -ForegroundColor Green
    Exec { msbuild "$solution_file" /v:minimal /p:OutDir=$build_artifacts_dir }
}

Task Clean {
    Write-Host "Creating BuildArtifacts directory" -ForegroundColor Green
    if (Test-Path $build_artifacts_dir) {   
        rd $build_artifacts_dir -rec -force | out-null
    }
    
    mkdir $build_artifacts_dir | out-null
    
    Write-Host "Cleaning $solution_file" -ForegroundColor Green
    Exec { msbuild $solution_file /t:Clean /p:Configuration=Release /v:minimal } 
}