param([String] $XMLFile)

[XML]$nugetRoot = Get-Content $XMLfile
 
Write-Host $nugetRoot.package.metadata.version