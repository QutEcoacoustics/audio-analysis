# Builds (runs MSBuild) for AnalysisPrograms.exe
# depends on MSBuild.exe being on path

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
