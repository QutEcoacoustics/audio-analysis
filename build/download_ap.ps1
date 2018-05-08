#!/usr/bin/pwsh

# Downloads bianry assets for AP.exe
param(
    # the source to get the binary from either 'github' or 'appveyor'   
    [Parameter(Position=0)]
    [ValidateSet('github','appveyor')]
    [string]$source,

    # The version to download. Use 'latest' to get the latest version.
    # Use an actual version number ('18.03.4.1') to get a speicific github version.
    # Use the AppVeyor build ID ('314') to get a specific appveyor build.
    [Parameter()]
    [string]$version = "latest",

    # Which build to get, either 'Debug' or 'Release', defaults to 'Release'
    [Parameter()]
    [ValidateSet('Release','Debug')]
    [string]$build = "Release",


    # Directory to extract binary to. Defaults to "/AP" ("C:\AP" on Windows)
    [Parameter()]
    [string]$destination = "/AP"
)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# resolve metadata for asset
if ($source -eq "github") {
    $github_url = "https://api.github.com/repos/QutEcoacoustics/audio-analysis/releases"
    if ($version -eq "latest") {
        $github_url += "/latest"
    }
    else {
        # strip the leading v if it is present
        $version = $version -replace "^v",""
        $github_url += "/tags/v$version"
    }

    $response = Invoke-RestMethod -Method Get -Uri $github_url
    $asset_url = $response.assets `
                 | Where-Object { $_.name -like "$build*" } `
                 | ForEach-Object browser_download_url
    Write-Output "Downloading release $($response.tag_name) from GitHub"
}
elseif ($source -eq "appveyor") {
    $appveyor_api = "https://ci.appveyor.com/api"
    $appveyor_project_url = "$appveyor_api/projects/QUTEcoacousticsResearchGroup/audio-analysis"

    # get the last 50 builds
    $response = Invoke-RestMethod -Method Get -Uri "$appveyor_project_url/history?recordsNumber=50"

    # filter builds for master branch and build success
    $ci_builds = $response.builds `
              | Where-Object { $_.status -eq "success"  -and $_.branch -eq "master" } `
              | Sort-Object finished -Descending
    if ($version -eq "latest") {
        $ci_build = $ci_builds[0]
    }
    else {
        $ci_build = $ci_builds | Where-Object { $_.version -eq $version }
    }

    if ($null -eq $ci_build) {
        throw "could not find version '$version' in last 50 AppVeyor builds"
    }

    # now get the build (we need to do this again because the job sub-object)
    # is not included in the build objects when the history is retrieved
    $ci_build = (Invoke-RestMethod -Method Get -Uri "$appveyor_project_url/build/$($ci_build.version)").build
    $artifacts_url = "$appveyor_api/buildJobs/$($ci_build.jobs[0].jobId)/artifacts"
    $artifacts = Invoke-RestMethod -Method Get -Uri $artifacts_url

    $file_name =  ($artifacts | Where-Object { $_.fileName -like "*$build*" }).fileName

    $asset_url = "$artifacts_url/$file_name"
    Write-Output "Downloading version $($ci_build.version) from AppVeyor"
}
else {
    throw "unknown source '$source'"
}


# remove directory if it already exists
if (Test-Path $destination) {
    Write-Warning "Deleting old installation at '$destination'"
    Remove-Item $destination -Recurse -ErrorAction Stop
    Start-Sleep 1
}
New-Item $destination -ItemType Directory -ErrorAction Stop

try {
    Push-Location $destination

    # download asset using system native curl
    Write-Output "Downloading asset $asset_url"
    $curl = Get-Command curl* -CommandType Application
    & $curl -L -O "$asset_url"
    if ($LASTEXITCODE -ne 0) {
        throw "failed downloading $asset_url"
    }

    $downloaded_zip = Get-ChildItem "$build*.zip"

    unzip -o $downloaded_zip
    if ($LASTEXITCODE -ne 0) {
        throw "failed extracting zip $downloaded_zip"
    }

    Remove-Item $downloaded_zip

    Write-Output "Download complete, installed to $destination"

}
finally {
    Pop-Location
}
