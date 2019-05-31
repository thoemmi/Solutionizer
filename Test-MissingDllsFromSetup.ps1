
#[xml]$wxs = Get-Content C:\priv\Solutionizer\Solutionizer.wxs

<#
$wxs = New-Object System.Xml.XmlDocument
$wxs.PreserveWhitespace = $true
$wxs.Load("C:\priv\Solutionizer\Solutionizer.wxs")
#>

$wxsPath = Join-Path $PSScriptRoot Solutionizer.wxs
$wxs = ( Select-Xml -Path $wxsPath -XPath / ).Node

$ns = @{wi = 'http://schemas.microsoft.com/wix/2006/wi'}
$existingComponents = Select-Xml "//wi:Directory[@Id='INSTALLLOCATION']/wi:Component/@Id" $wxs -Namespace $ns | % {
  $_.Node.Value
}
$existingComponentRefs = Select-Xml "//wi:Feature[@Id='Complete']/wi:ComponentRef/@Id" $wxs -Namespace $ns | % {
  $_.Node.Value
}

$dllsToInstall = Get-ChildItem (Join-Path $PSScriptRoot "Solutionizer\bin\Debug") -Filter *.dll | % { $_.Name }

$dllsToInstall | ? { $existingComponents -notcontains $_ } | % {
    Write-Host @"
          <Component Id="$_" Guid="$([System.Guid]::NewGuid().ToString("B"))">
            <File Id="$_" Name="$_" Source="Solutionizer/bin/Debug/$_"/>
            <RegistryValue Root="HKCU" Key="SOFTWARE\ThomasFreudenberg\Solutionizer" Name="$_" Type="integer" Value="1" KeyPath="yes"/>
          </Component>
"@
}

$dllsToInstall | ? { $existingComponentRefs -notcontains $_ } | % {
    Write-Host @"
      <ComponentRef Id="$_"/>
"@
}


