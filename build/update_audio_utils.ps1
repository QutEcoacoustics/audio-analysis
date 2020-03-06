# When adding a new binary check that our supported platforms documentation matches
# what the binary is capable of running on.
#
# # FFMPEG notes
#
# Download from here:
# https://ffmpeg.zeranoe.com/builds/
#
# Choose a stable version, 64-bit builds only, and choose "Shared" linking.
# The shared linking option saves on space, otherwise all resources are embedded into all
# executables.

$output_directory = "$PSScriptRoot\..\lib\audio-utils"
$ffmpeg_version = "4.2.2"
#$sox_version = "14.4.2"

$tools = @{
    "ffmpeg" = @{
        "win-x64" = "https://ffmpeg.zeranoe.com/builds/win64/shared/ffmpeg-$ffmpeg_version-win64-shared.zip";
        # likely broken - not exact RID match
        "win-arm64" = "https://ffmpeg.zeranoe.com/builds/win64/shared/ffmpeg-$ffmpeg_version-win64-shared.zip";
        "linux-x64" = "https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz";
        # likely broken - not exact RID match
        "linux-musl-x64" = "https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz";
        "linux-arm" = "https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-armhf-static.tar.xz";
        "osx-x64" = "https://ffmpeg.zeranoe.com/builds/macos64/shared/ffmpeg-$ffmpeg_version-macos64-shared.zip";

    };
    # only supports ffmpeg currently
    # "sox" = @{
    #     "win-x64" = "";
    #     "win-arm64" = "";
    #     "linux-x64" = "";
    #     "linux-musl-x64" = "";
    #     "linux-arm" = "";
    #     "osx-x64" = "";
    # }
}

Push-Location

foreach ($tool in $tools.Keys) {
    foreach ($rid in $tools[$tool].Keys) {
        Write-Output "Downloading $tool for $rid"
        $url = $tools[$tool][$rid]

        if ([string]::IsNullOrEmpty($url)) {
            Write-Warning "Skipping $tool for $rid because url is empty"
            continue;
        }

        $tool_output_directory = Get-AbsolutePath $output_directory "$rid/$tool"
        $extension = if ($url -ilike "*.tar.xz") { ".tar.xz" } else { ".zip" }
        $download_file =  Join-Path $tool_output_directory "download$extension"
        New-Item -Path $tool_output_directory -ItemType Directory -Force | Out-Null

        Set-Location $tool_output_directory

        Invoke-WebRequest $url -OutFile $download_file

        Write-Output "Extracting $tool for $rid"
        if ($url -ilike "*.tar.xz*") {
        $command = "tar --force-local -xf '$download_file'"
        }
        else {
            $command = "unzip -o '$download_file'"
        }
        Invoke-Expression $command

        Write-Output "Deleting download $tool for $rid"
        Remove-Item $download_file

        if ($tool -eq "ffmpeg") {


            if ($rid -ilike "linux*") {
                $bin = Get-ChildItem $tool_output_directory -Recurse -File -Include ffmpeg
                Copy-Item "$($bin.Directory)/*" $bin.Directory.Parent
                Remove-Item $bin.Directory -Force -Recurse
            }
            elseif ($rid -ilike "osx*") {
                $bin = Get-ChildItem $tool_output_directory -Recurse -File -Include ffmpeg
                Copy-Item "$($bin.Directory)/*" $tool_output_directory
                Copy-Item $bin/../../*.txt $tool_output_directory
                Remove-Item $bin.Directory.Parent -Force -Recurse
            }
            else {
                $bin = Get-ChildItem $tool_output_directory -Recurse -File -Include ffmpeg.exe
                Copy-Item "$($bin.Directory)/*" $tool_output_directory
                Copy-Item $bin/../../*.txt $tool_output_directory
                Remove-Item $bin.Directory.Parent -Force -Recurse
            }
        }
    }
}

Write-Output "finished"

Pop-Location