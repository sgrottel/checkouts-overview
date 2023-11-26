#
# Inject the build number into the version number into many files
#

param(
	[string]$buildNumber,
	[string]$commitHash
	)

#
# Checkouts-Overview.csproj
#
$csproj = New-Object System.XML.XMLDocument
$csprojPath = Join-Path $PSScriptRoot "Checkouts-Overview.csproj"
$csproj.Load($csprojPath)

$version = New-Object System.Version $csproj.Project.PropertyGroup.Version
$version = New-Object System.Version @( $version.Major, $version.Minor, ([System.Math]::Max(0, $version.Build)), $buildNumber )

Write-Host "Version number: $version"
Write-Output "FULL_VERSION_NUMBER=$version" >> $env:GITHUB_ENV

$csproj.Project.PropertyGroup.Version = $version.ToString()
$csproj.Save($csprojPath)


#
# ComponentSource.json
#
$csjson = join-path $PSScriptRoot "ComponentSource.json"
$json = Get-Content $csjson -raw | ConvertFrom-Json -AsHashtable
$json.components[0].source[0].version = $version.ToString()
$json.components[0].source[0].hash = $commitHash
$json | ConvertTo-Json -Depth 10 | Set-Content $csjson


#
# README.md
#
$lines = [System.Collections.ArrayList](Get-Content (Join-Path ${PSScriptRoot} ".\..\README.md"))
$lines.Insert(2, ("Version " + $version));
$newLines = @()
$inc = $true
$lines | foreach { if ($_ -match "\s*<!---\s+START\s+STRIP\s+-->\s*") { $inc = $false; } elseif ($_ -match "\s*<!---\s+END\s+STRIP\s+-->\s*") { $inc = $true } elseif ($inc) { $newLines += $_ } }
Set-Content (Join-Path ${PSScriptRoot} "README.md") $newLines

