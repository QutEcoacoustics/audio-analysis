# A simple loop to generate indices for a folder of files.
# Karlina Indraswari 2017
#
# For each audio file found,
# - It runs audio2csv to generate indices
#
# Assumptions:
# - AnalysisPrograms.exe is in current directory
# - You're working in Windows (though only trivial changes are required to work in Unix)
# - Users will customize the below variables before running the script


# Select the directory containing the files
$directory = "C:\temp\Emerald River Audio Snippets\20131227\"
# The directory to store the results
$base_output_directory = "C:\temp\indices_output"

# Get a list of audio files inside the directory
# (Get-ChildItem is just like ls, or dir)
$files = Get-ChildItem "$directory\*" -Include "*.mp3", "*.wav"

# iterate through each file
foreach($file in $files) {
    Write-Output ("Processing " + $file.FullName)

    # get just the name of the file
    $file_name = $file.Name

    # make a folder for results
    $output_directory = "$base_output_directory\$file_name"
    mkdir $output_directory

    # prepare command
    $command = ".\AnalysisPrograms.exe audio2csv -source `"$file`" -config `".\configFiles\Towsey.Acoustic.30.yml`" -output `"$output_directory`" -n"
    
    # finally, execute the command
    Invoke-Expression $command
}