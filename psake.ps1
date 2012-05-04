# Helper script for those who want to run psake without importing the module.
# Example:
# .\psake.ps1 "default.ps1" "BuildHelloWord" "4.0" 

# Must match parameter definitions for psake.psm1/invoke-psake 
# otherwise named parameter binding fails
param(
  [Parameter(Position=0,Mandatory=0)]
  [string]$buildFile = 'default.ps1',
  [Parameter(Position=1,Mandatory=0)]
  [string[]]$taskList = @(),
  [Parameter(Position=2,Mandatory=0)]
  [string]$framework = '4.0',
  [Parameter(Position=3,Mandatory=0)]
  [switch]$docs = $false,
  [Parameter(Position=4,Mandatory=0)]
  [System.Collections.Hashtable]$parameters = @{},
  [Parameter(Position=5, Mandatory=0)]
  [System.Collections.Hashtable]$properties = @{}
)

$solution_path = Split-Path -parent $MyInvocation.MyCommand.path
$nuget_dir = Join-Path $solution_path .nuget
$nuget_exe = Join-Path $nuget_dir nuget.exe
$nuget_packages_config = Join-Path $nuget_dir packages.config
$packages_dir = Join-Path $solution_path packages

function Get-NuGetPackageFolder {
    param(
        [string] $package_name
    )

    $xml = [xml](gc $nuget_packages_config)
    $package = $xml.packages.package | where { $_.id -eq $package_name }
    Join-Path $packages_dir ($package_name + "." + $package.version)
}

try {
    #check, if psake is up-to-date
    & $nuget_exe install $nuget_packages_config -o $packages_dir

    # get psake directory
    $psake_dir = Get-NuGetPackageFolder "psake"

    # invoke psake
    #. (Join-Path $psake_dir "tools\psake.ps1") .\default.ps1

    import-module (join-path $psake_dir "tools\psake.psm1")

    invoke-psake $buildFile $taskList $framework $docs $parameters $properties
} finally {
    remove-module psake -ea 'SilentlyContinue'
}