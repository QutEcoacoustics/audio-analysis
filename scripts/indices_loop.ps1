<#
.SYNOPSIS
  # A simple loop to generate indices for a folder of files.
.DESCRIPTION
  Generates acoustic indices for multiple audio files.
  Each recording will have it's indices save in a directory named after the input audio recording.
.PARAMETER $source_directory
    A directory of audio files to process.
.PARAMETER $output_directory
.INPUTS
  Optionally: input directories
.OUTPUTS
  Files stored in $output_directory
.NOTES
  Version:        2.0
  Author:         Karlina Indraswari & Anthony Truskinger
  Creation Date:  2017
  Purpose/Change: Updated docs links, add ap finder script

.EXAMPLE
  ./indices_loop.ps1 D:/Stud -time_zone_offset "10:00" -output_directory ./output
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

$ap = . "$PSScriptRoot/find_ap.ps1"
$default_configs = Resolve-Path "$ap_path/../ConfigFiles"

# Get a list of audio files inside the directory
# (Get-ChildItem is just like ls, or dir)
$files = Get-ChildItem "$source_directory\*" -Include "*.mp3", "*.wav"

# iterate through each file
foreach ($file in $files) {
    Write-Output ("Processing " + $file.FullName)

    # get just the name of the file
    $file_name = $file.Name

    # make a folder for results
    $instance_output_directory = Join-Path $output_directory $file_name
    New-Item $instance_output_directory -ItemType Directory

    # prepare command
    # for more information on how this command works, please see:
    # https://ap.qut.ecoacoustics.info/technical/commands/analyze_long_recording.html
    $command = "$ap audio2csv `"$file`" `"$default_configs\Towsey.Acoustic.yml`" `"$instance_output_directory`" -n --parallel"

    # finally, execute the command
    Invoke-Expression $command
}
