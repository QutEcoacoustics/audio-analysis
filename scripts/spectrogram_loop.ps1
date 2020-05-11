# Generate standard-scale spectrograms from multiple one-minute recordings.
<#
.SYNOPSIS
  Generates standard spectrograms for each minute for multiple audio files.
.DESCRIPTION
  For every audio file found in a directory (searches recursively) this command
  will run AP.exe to generate a set of standard spectrograms for each minute of
  data.
  NOTE: all spectrograms from all recordings will be saved to the same
  directory!
.PARAMETER $source_directory
    A directory of audio files to process.
.PARAMETER $output_directory
.INPUTS
  Optionally: input directories
.OUTPUTS
  Files stored in $output_directory
.NOTES
  Version:        2.0
  Author:         Anthony Truskinger & Michael Towsey
  Creation Date:  2020-01-30
  Purpose/Change: Updated docs links, add ap finder script

.EXAMPLE
  ./spectrogrtam_loop.ps1 D:/Stud -output_directory ./output
#>

param(
    [Parameter(
        Position = 0,
        Mandatory = $true)]
    $source_directory,
    [Parameter(
        Position = 1,
        Mandatory = $true)]
    $output_directory
)

Write-Output "Draw standard-scale spectrograms of multiple one-minute recordings"


$ap = . "$PSScriptRoot/find_ap.ps1"
$default_configs = Resolve-Path "$ap_path/../ConfigFiles"

$workshop_config = "$default_configs/Towsey.SpectrogramGenerator.yml"

Write-Output "Using config File = $workshop_config"
Write-Output "Source Directory = $source_directory"
Write-Output "Output Directory = $output_directory"

# Get a list of audio files inside the directory
# (Get-ChildItem is just like ls, or dir)
$recordingFiles = Get-ChildItem "$source_directory\*" -Include "*.wav"

Write-Output "Found $($recordingFiles.Count) files"


# for each file found...
foreach ($file in $recordingFiles) {
    Write-Output (">>>Processing: " + $file.Name)

    # Run the spectrogram generation command
    # And also tell AP.exe to talk less (with --quiet)!
    # for more information on how this command works, please see:
    # https://ap.qut.ecoacoustics.info/technical/commands/analyze_long_recording.html
    . $ap audio2sonogram $file $workshop_config $output_directory --quiet
}

Write-Output "Finished!"
