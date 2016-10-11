# Create release .tar.gz files for AnalysisPrograms.exe
#
# depends on 7-zip (7za.exe) being on path (`choco install 7zip`)
# depends on hub being on path (`choco install hub`)
# depends on MSBuild.exe being on path

param([bool]$pre_release = $true)
$ErrorActionPreference = "Stop"

function Check-Command($cmdname)
{
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}
if (!(Check-Command 7za) -or !(Check-Command hub) -or !(Check-Command MSBuild)) {
	throw "Cannot find needed executable dependencies";
}


cd $PSScriptRoot

echo "Running build"

. .\build.ps1

Pause

cd "AudioAnalysis/AnalysisPrograms/bin"

echo "Removing old tarballs"

rm *.tar*

# extract the built version
$version =  (.\Release\AnalysisPrograms.exe | Select-String '\d{2}\.\d{2}\.\d{4}\.\d+').Matches[0].Value

echo "Packging files for version $version"



# FYI pipelining is slow because each line is allocated a System.String object.
# We're just going to write temporary files instead.

# create tar.gz for Release
7za.exe a -ttar Release.$version.tar Release/* -xr0!*log.txt* ; 7za.exe a Release.$version.tar.gz Release.$version.tar 

# create tar.gz for Debug
7za.exe a -ttar Debug.$version.tar Debug/* xr0!*log.txt* ; 7za.exe a Debug.$version.tar.gz Debug.$version.tar

# create and upload a github release
$tag_name = "v$version"
echo "creating tag '$tag_name'"
git tag -a -m "Version $tag_name" $tag_name
echo "pushing tags"
git push --follow-tags
echo "creating github release"
hub release create "$(if($pre_release){"-p"})" -a .\Release.$version.tar.gz -a .\Debug.$version.tar.gz -m "Version $tag_name`nRELEASE and DEBUG builds" $tag_name

Write-Host "Release created!"
