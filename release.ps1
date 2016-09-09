# Create release .tar.gz files for AnalysisPrograms.exe

# depends on 7-zip (7za.exe) being on path (`choco install 7zip`)
# depends on hub being on path (`choco install hub`)

param([bool]$pre_release = $true)

cd $PSScriptRoot

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
hub release create "$(if($pre_release){"-p"})" -a .\Release.16.09.3549.0.tar -a .\Debug.16.09.3549.0.tar -m "Version $tag_name`nRLEASE and DEBUG builds" $tag_name

Write-Host "Release created!"