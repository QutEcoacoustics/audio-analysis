param()

$command = Get-Command AnalysisPrograms* -ErrorAction SilentlyContinue

if ($null -ne $command) {
    return $command.Path;
    exit 0;
}

if ($IsWindows) {
    
    $command = Get-Command C:\AP\AnalysisPrograms.exe -ErrorAction SilentlyContinue

    if ($null -ne $command) {
        return $command.Path;
        exit 0;
    }
}

if ($IsLinux) {
    $command = Get-Command /AP/AnalysisPrograms.exe -ErrorAction SilentlyContinue

    if ($null -ne $command) {
        return $command.Path;
        exit 0;
    }
}

Write-Error @"
Can't find AnalysisPrograms.exe. 
Please install it using the instructions from: 
https://github.com/QutEcoacoustics/audio-analysis/blob/master/docs/installing.md
"@

exit 1;