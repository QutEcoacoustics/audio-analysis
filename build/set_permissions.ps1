#!/usr/bin/pwsh

#Requires -Version 6

$tools = @(
    "ffmpeg",
    "ffprobe",
    "sox",
    "soxi",
    "wvunpack"
)

foreach ($tool in $tools) {
    $files = Get-ChildItem "$PSScriptRoot/../lib/audio-utils" -Include "$tool*" -Recurse -File
        | Where-Object { $_.Name -in $tools -or $_.Extension -eq ".exe" }

    foreach ($file in $files) {
        if (!$IsWindows) {
            chmod a+x $file
        }

        git update-index --chmod=+x $file

    }
}