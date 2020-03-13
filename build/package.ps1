# Create package a build into a .zip file for AnalysisPrograms.exe
#
# depends on 7-zip (7za.exe) being on path (`choco install 7zip`)
# 
# This script has been modified to work with our CI server

param($configuration = $null)
$ErrorActionPreference = "Continue"

function script:exec {
    [CmdletBinding()]

	param(
		[Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
		[Parameter(Position=1,Mandatory=0)][string]$errorMessage = ("Error executing command: {0}" -f $cmd)
	)
	& $cmd
	if ($lastexitcode -ne 0)
	{
		throw $errorMessage
	}
}

function Check-Command($cmdname)
{
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}


if (!(Check-Command 7z)) {
	throw "Cannot find needed executable dependencies";
}

if ($configuration -eq $null) {
    throw "Configuration argument must be provided"
}


cd $PSScriptRoot
try {

cd "$PSScriptRoot/../src/AnalysisPrograms/bin"


echo "Removing old zips (this should have no effect on CI server)"
rm "$configuration*.zip" -Verbose

# extract the built version
$version =  (. ".\$configuration\AnalysisPrograms.exe" -n | Select-String '\d+\.\d+\.\d+\.\d+').Matches[0].Value
$env:ApVersion = $version

echo "Packging files for version $version"

$ApName = "$configuration.$version.zip"
Set-Item "env:AppName$Configuration" $ApName

# create tar.gz for $environment
exec { 7z a -tzip $ApName "./$configuration/*" -xr0!*log.txt* }

echo "Packing complete"

Set-Item "env:ApPackage$Configuration" (Join-Path "src\AnalysisPrograms\bin" $ApName)

}
finally {
cd "$PSScriptRoot/.."
}
