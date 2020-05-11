# A loop to generate indices and zooming images, with support for remote file downloading
# and retry semantics.
# Anthony Truskinger 2018
#
# For each audio file found,
# - It copies the file off a remote server
# - it generates high reoslution indices
# - It generates zooming tiles for each file.
#
# Assumptions:
# - AnalysisPrograms.exe is on PATH (or in current directory)
# - You're working in Windows (though only trivial changes are required to work in Unix)
# - Users will customize the below variables before running the script

$working_dir = "D:/Temp/zooming"
$csv = "$working_dir/all_oxley_creek_recordings.csv"
$output = $working_dir

# find the path to analysis programs
$ap_path = (Get-Command AnalysisPrograms.exe).Path
$default_configs = Resolve-Path "$ap_path/../ConfigFiles"

$indices_config_file = "$default_configs/Towsey.Acoustic.Zooming.yml"
$zooming_config_file = "$default_configs/SpectrogramZoomingConfig.yml"

$remote_user = "ubuntnu"
$remote_server = "server.example.com"
$remote_path = "/home/ubuntu/data/original_audio"

# helper functions



# just checks whether a previous run was successful
function HasAlreadyRun($results_dir) {
    $log = Join-Path $results_dir "log.txt"
    if (Test-Path $log) {
        $match = Get-Content -Tail 1 $log | Select-String "ERRORLEVEL: (\d+)"
        return $match.Matches.Groups[1].Value -eq 0
    }

    return $false
}

# a trivial utility to indent output from analysis programs
# (makes it easier to separate script logging from AP.exe logging)
function IndentOutput {
    Process {
        "`t" + $_
    }
}

# import the csv data
$recordings = Import-Csv $csv

# for each file
$results = @()
foreach ($recording in $recordings) {
    Write-Output "Starting new recording $($recording.uuid)"

    # create a results object to store results
    $result = New-Object "pscustomobject" | Select-Object Download, Indices, Images

    # extract all needed meta data to create a path to the remote file
    # constructs a path that looks like: ".../data/b2/b24460cf-e25e-44c9-9034-af9b0a1ddcbe_20121019-140000Z.wav
    $uuid = $recording.uuid
    $prefix = $uuid.Substring(0, 2)
    $date = (Get-Date $recording.recorded_date).ToString("yyyyMMdd-HHmmssZ")
    $name = "$uuid`_$date.wav"
    $remote_path = "$remote_path/$prefix/$name"
    $local_dir = "$output/$prefix"
    $local_path = "$local_dir/$name"

    mkdir.exe -p $local_dir
    Set-Location $local_dir

    # download the file via ssh
    sftp "$remote_user@$remote_server`:$remote_path" $name
    $result.Download = $LASTEXITCODE
    if ($result.Download -ne 0) {
        $results += $result
        continue;
    }

    $instance_output = $local_path + "_results"
    # generate indices

    if (HasAlreadyRun $instance_output) {
        $result.Indices = "0*"
        Write-Output "Skipping indices generation for $uuid - already completed"
    }
    else {
        AnalysisPrograms.exe audio2csv $local_path $indices_config_file $instance_output -p --when-exit-copy-log -n | IndentOutput
        $result.Indices = $LASTEXITCODE
        if ($result.Indices -ne 0) {
            $results += $result
            continue;
        }
    }

    # generate zooming tiles
    $indices_dir = Join-Path $instance_output "Towsey.Acoustic"
    AnalysisPrograms.exe DrawZoomingSpectrograms $indices_dir $zooming_config_file $instance_output -o "sqlite3" -z "Tile" -n | IndentOutput
    $result.Images = $LASTEXITCODE
    $results += $result

    # copy log
    Copy-Item (Join-Path $ap_path "../Logs/log.txt") (Join-Path $instance_output "zooming_log.txt")
}

Write-Output "Analysis complete"
$results | Export-Csv $output/results.csv -NoTypeInformation