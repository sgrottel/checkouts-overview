param(
	[Parameter(Mandatory=$true)][string]$version
	)

dotnet publish Checkouts-Overview.sln -c release --no-build --no-restore /p:CopyOutputSymbolsToPublishDirectory=false

# write-host $version
rm "${PSScriptRoot}\bin\Release\*.zip" -ErrorAction Ignore
rm "${PSScriptRoot}\bin\Release\Checkouts-Overview" -recurse -force -ErrorAction Ignore
mkdir "${PSScriptRoot}\bin\Release\Checkouts-Overview"
$src = ( gci "app\bin\Release\*\publish" | select -expand fullname )
copy "${src}\*" "${PSScriptRoot}\bin\Release\Checkouts-Overview" -recurse -force
copy "${PSScriptRoot}\bin\Release\ComponentSource.json" "${PSScriptRoot}\bin\Release\Checkouts-Overview" -force

copy "${PSScriptRoot}\..\LICENSE" "${PSScriptRoot}\bin\Release\Checkouts-Overview" -force

$lines = [System.Collections.ArrayList](get-content "${PSScriptRoot}\..\README.md")
$lines.Insert(2, ("Version " + $version));
$newLines = @()
$inc = $true
$lines | foreach { if ($_ -match "\s*<!---\s+START\s+STRIP\s+-->\s*") { $inc = $false; } elseif ($_ -match "\s*<!---\s+END\s+STRIP\s+-->\s*") { $inc = $true } elseif ($inc) { $newLines += $_ } }
set-content "${PSScriptRoot}\bin\Release\Checkouts-Overview\README.md" $newLines

Compress-Archive -Path "${PSScriptRoot}\bin\Release\Checkouts-Overview" -DestinationPath ("${PSScriptRoot}\bin\Release\Checkouts-Overview-" + $version + ".zip") -force
