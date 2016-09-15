# Create release .tar.gz files for AnalysisPrograms.exe

# depends on 7-zip (7za.exe) being on path (`choco install 7zip`)
# depends on hub being on path (`choco install hub`)
# depends on MSBuild.exe being on path

param([bool]$pre_release = $true)

cd $PSScriptRoot

function Start-BuildCommand($target, $configuration) {
    # Use $target="" for a actually building, otherwise append an additional target
    if ($target.Length -gt 0) {
        $target = ":" + $target
    }

    iex "MSBuild.exe `".\AudioAnalysis\AudioAnalysis2012.sln`" /verbosity:quiet /clp:`"NoSummary;NoItemAndPropertyList;ErrorsOnly`" /p:warn=option /p:WarningLevel=0 /p:RunCodeAnalysis=false /t:AnalysisPrograms$target /p:Configuration=$configuration"

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "MSBuild.exe `".\AudioAnalysis\AudioAnalysis2012.sln`" /p:RunCodeAnalysis=false /t:AnalysisPrograms$target /p:Configuration=$configuration"
        throw "Build failed for $target and $configuration, exit code: $LASTEXITCODE"
    }
}

echo "Cleaning projects"
(Start-BuildCommand "Clean" "Release")
(Start-BuildCommand "Clean" "Debug")

echo "Building Release"
(Start-BuildCommand "" "Release")
echo "Building Debug"
(Start-BuildCommand "" "Debug")

echo ("Build Complete")

cd "AudioAnalysis/AnalysisPrograms/bin"

echo "Removing old tarballs"

rm *.tar*

# extract the built version
$version =  (.\Release\AnalysisPrograms.exe | Select-String '\d{2}\.\d{2}\.\d{4}\.\d+').Matches[0].Value

echo "Packging files for version $version"

Pause

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
