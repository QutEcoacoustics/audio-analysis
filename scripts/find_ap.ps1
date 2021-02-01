param()

function Test-ApDir($path) {
    $command = Get-Command $path -ErrorAction SilentlyContinue | Select-Object -First 1

    if ($null -ne $command) {
        Write-Output $command.Path;
        exit 0;
    }

}

Test-ApDir "AnalysisPrograms*"

$old = switch ($null) {
    { $IsWindows } { "C:\AP\AnalysisPrograms.exe"  }
    { $IsLinux } {  "/AP/AnalysisPrograms.exe" }
    { $IsMacOS } {  "/AP/AnalysisPrograms.exe" }
    Default { throw "Unknown OS"}
}

Test-ApDir $old

$new = switch ($null) {
    { $IsWindows } { "$home\AP\AnalysisPrograms.exe"  }
    { $IsLinux } {  "$HOME/.local/share/AP/AnalysisPrograms.exe" }
    { $IsMacOS } {  "$HOME/.local/share/AP/AnalysisPrograms.exe" }
    Default { throw "Unknown OS"}
}

Test-ApDir $new

Write-Error @"
Can't find AnalysisPrograms.exe.
Searched on PATH, in $old, and in $new.
Please install it using the instructions from:
https://github.com/QutEcoacoustics/audio-analysis/blob/master/docs/installing.md
"@

exit 1;