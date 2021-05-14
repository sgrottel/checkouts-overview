param(
	[Parameter(Mandatory=$true)][string]$version
	)

dotnet publish Checkouts-Overview.sln -c release --no-build --no-restore /p:CopyOutputSymbolsToPublishDirectory=false

# write-host $version
rm "${PSScriptRoot}\bin\Release\*.zip" -ErrorAction Ignore
rm "${PSScriptRoot}\bin\Release\Checkouts-Overview" -recurse -force -ErrorAction Ignore
mkdir "${PSScriptRoot}\bin\Release\Checkouts-Overview"
copy "${PSScriptRoot}\bin\Release\net5.0-windows\publish\*" "${PSScriptRoot}\bin\Release\Checkouts-Overview" -recurse -force
Compress-Archive -Path "${PSScriptRoot}\bin\Release\Checkouts-Overview" -DestinationPath ("${PSScriptRoot}\bin\Release\Checkouts-Overview-" + $version + ".zip") -force
