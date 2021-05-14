param(
	[Parameter(Mandatory=$true)][string]$version
	)

dotnet publish -c release /p:CopyOutputSymbolsToPublishDirectory=false

# write-host $version
rm ".\bin\Release\*.zip" -ErrorAction Ignore
rm ".\bin\Release\Checkouts-Overview" -recurse -force -ErrorAction Ignore
mkdir ".\bin\Release\Checkouts-Overview"
copy ".\bin\Release\net5.0-windows\publish\*" ".\bin\Release\Checkouts-Overview" -recurse -force
Compress-Archive -Path ".\bin\Release\Checkouts-Overview" -DestinationPath (".\bin\Release\Checkouts-Overview-" + $version + ".zip") -force
