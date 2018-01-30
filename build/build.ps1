# Builds (runs MSBuild) for AnalysisPrograms.exe
# depends on MSBuild.exe version 15 being on path
#
# This script is not used by our CI and is for local builds


cd $PSScriptRoot

if ((gcm MSBuild.exe*) -eq $null) {
    throw "Cannot find MSBuild.exe on PATH"
}

$version = (MSBuild.exe /version) | Select-String "^\d+\.\d+\."
if ($version -notlike "15.*") {
    throw "MSBuild version 15 is required - found $version instead"
}

function Start-BuildCommand($target, $configuration, $platform='Any CPU') {
    # Use $target="" for a actually building, otherwise append an additional target
    if ($target.Length -gt 0) {
        $target = "/t:" + $target
        # /t:AnalysisPrograms$target
    }

    $command = @"
    MSBuild.exe ".\AudioAnalysis\AudioAnalysis2012.sln"
    /verbosity:minimal /m
    /p:WarningLevel=0 /p:RunCodeAnalysis=false
    $target /p:Configuration=$configuration /property:Platform="$platform"
"@

    $command = $command -replace '\s+',' '

    iex $command

    if ($LASTEXITCODE -ne 0) {
        Write-Warning ("Build command failed`nCommand:`n" + $command)
        throw "Build failed for $target and $configuration and $platform, exit code: $LASTEXITCODE"
    }
}

echo "Cleaning projects"
(Start-BuildCommand "AnalysisPrograms:Clean" "Release")
(Start-BuildCommand "AnalysisPrograms:Clean" "Debug")

echo "Building Release"
(Start-BuildCommand "" "Release")
echo "Building Debug"
(Start-BuildCommand "" "Debug")

echo ("Build Complete")

