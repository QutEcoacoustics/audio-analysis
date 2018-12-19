#!/usr/bin/pwsh

#Requires -Version 5

# .SYNOPSIS
# Downloads and "installs" (or updates) AP.exe
# .DESCRIPTION
# Downloads and installs AnalysisPrograms.exe. It can retieve releases from GitHub or AppVeyor.
# It defaults to installing in the root drive.
# For Windows machines it will add the install directory to the PATH envivronment variable.
# For Linux and MacOS it will symlink the binary into /usr/bin.
# AP.exe does not automatically update itself and currently is not integrated with any system package managers.
# .PARAMETER package
# Downloads one of the pre-defined versions, either Stable, Weekly, or Continuous
# .PARAMETER version
# The AP.exe version to download from GitHub. Must be a specific version (e.g. "18.8.1.2").
# .PARAMETER ci_build_number
# The AP.exe build to download from AppVeyor. Must be the CI build number (e.g. "314").
# .PARAMETER destination
# Where to "install" AP.exe to. Defaults to "/AP" ("C:\AP" on Windows)
# .EXAMPLE
# C:\> ./download_ap.ps1
# .EXAMPLE
# C:\> ./download_ap.ps1 -package Stable
# .EXAMPLE
# C:\> ./download_ap.ps1 -package Weekly
# .EXAMPLE
# C:\> ./download_ap.ps1 -package Continuous
# .LINK
# https://github.com/QutEcoacoustics/audio-analysis/blob/master/docs/installing.md
[CmdletBinding(
    DefaultParameterSetName = "Default",
    HelpURI = "https://github.com/QutEcoacoustics/audio-analysis/blob/master/docs/installing.md",
    SupportsShouldProcess = $true
)]
param(
    [Parameter(ParameterSetName = "Default")]
    [ValidateSet('Stable', 'Weekly', 'Continuous')]
    [string]$package = "Stable",

    [Parameter(ParameterSetName = "GitHub", Mandatory = $true)]
    [string]$version,

    [Parameter(ParameterSetName = "AppVeyor", Mandatory = $true)]
    [string]$ci_build_number,

    [Parameter()]
    [string]$destination = "/AP"
)
$ErrorActionPreference = "Stop"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Import-Module "Microsoft.PowerShell.Archive" -Force

$build = "Release"
$type = $PsCmdlet.ParameterSetName
$exact_version = $null
switch ($type) {
    "Default" {
        switch ($package) {
            "Stable" {
                $source = "github"
            }
            "Weekly" {
                $source = "github"
            }
            "Continuous" {
                $source = "appveyor"

            }
            Default { throw "Unkown package type"}
        }
    }
    "GitHub" {
        if ($version -notmatch '\d{2}\.\d{1,2}\.\d{1,2}\.\d{1,2}') {
            Write-Error "The version argument '$version' did not look like a version (w.x.y.z)"
            exit 1
        }
        $source = "github"
        $exact_version = $version
    }
    "AppVeyor" {
        if ($ci_build_number -notmatch '\d{1,4}') {
            Write-Error "The ci_build_number argument '$ci_build_number' is not a valid number"
            exit 1
        }
        $source = "appveyor"
        $exact_version = $ci_build_number
    }
}

# resolve metadata for asset
# Begin non-volitile block (this section always runs despite -whatif but it also
# should have no side effects)
$script:oldWhatIfPreference = $WhatIfPreference
try {
    $WhatIfPreference = $false
    if ($source -eq "github") {
        $github_url = "https://api.github.com/repos/QutEcoacoustics/audio-analysis/releases"
        if ($null -eq $exact_version) {
            if ($package -eq "Stable") {
                $github_url += "/latest"
            }
            else {
                # Weekly release i.e. pre-release
                # Nothing to do here, we return a list of releases and filter for the
                # one we want
            }
        }
        else {
            # strip the leading v if it is present
            $exact_version = $exact_version -replace "^v", ""
            $github_url += "/tags/v$exact_version"
        }

        $response = Invoke-RestMethod -Method Get -Uri $github_url
        $response = $response | Select-Object -First 1
        $asset_url = $response.assets `
            | Where-Object { $_.name -like "$build*" } `
            | ForEach-Object browser_download_url
        $exact_version = $response.tag_name
        Write-Output "Downloading release $($response.tag_name) from GitHub"
    }
    elseif ($source -eq "appveyor") {
        $appveyor_api = "https://ci.appveyor.com/api"
        $appveyor_project_url = "$appveyor_api/projects/QUTEcoacousticsResearchGroup/audio-analysis"

        # get the last 50 builds
        $response = Invoke-RestMethod -Method Get -Uri "$appveyor_project_url/history?recordsNumber=50"

        # filter builds for master branch and build success
        $ci_builds = $response.builds `
            | Where-Object { $_.status -eq "success" -and $_.branch -eq "master" } `
            | Sort-Object finished -Descending
        if ($null -eq $exact_version) {
            # "Continuous" package
            $ci_build = $ci_builds[0]
        }
        else {
            $ci_build = $ci_builds | Where-Object { $_.version -eq $exact_version }
        }

        if ($null -eq $ci_build) {
            throw "could not find version '$exact_version' in last 50 AppVeyor builds"
        }

        # now get the build (we need to do this again because the job sub-object)
        # is not included in the build objects when the history is retrieved
        $ci_build = (Invoke-RestMethod -Method Get -Uri "$appveyor_project_url/build/$($ci_build.version)").build
        $artifacts_url = "$appveyor_api/buildJobs/$($ci_build.jobs[0].jobId)/artifacts"
        $artifacts = Invoke-RestMethod -Method Get -Uri $artifacts_url

        $file_name = ($artifacts | Where-Object { $_.fileName -like "*$build*" }).fileName

        $asset_url = "$artifacts_url/$file_name"
        $exact_version = $ci_build.version
        Write-Output "Downloading version $($ci_build.version) from AppVeyor"
    }
    else {
        throw "unknown source '$source'"
    }
}
finally {
    $WhatIfPreference = $script:oldWhatIfPreference
}

#
# Begin volitile actions
#

# remove directory if it already exists
$destination = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($destination)
if (Test-Path $destination) {
    Write-Warning "Deleting old installation at '$destination' (but keeping log files)"
    if ($PsCmdlet.ShouldProcess($destination, "Deleting old install")) {
        $script_name = split-path $PSCommandPath -Leaf
        Remove-Item -Path "$destination/*" -Recurse -ErrorAction Stop -Exclude "$script_name", "*log*"
        Start-Sleep 1
    }
}
else {
    New-Item $destination -ItemType Directory -ErrorAction Stop
}


# download asset 
Write-Output "Downloading asset $asset_url"
$downloaded_zip = "$destination/AP.zip"
if ($PsCmdlet.ShouldProcess($asset_url, "Downloading asset")) {
    # use curl if available (faster)
    $curl = Get-Command curl* -CommandType Application -TotalCount 1
    if ($curl) {
        & $curl -L -o "$downloaded_zip" "$asset_url"
    }
    else {
        $asset_response = Invoke-WebRequest $asset_url -OutFile $downloaded_zip -PassThru
        if ($asset_response.StatusCode -ne 200) {
            throw "failed downloading $asset_url"
        }
    }
}

# extract asset
if ($PsCmdlet.ShouldProcess($downloaded_zip, "Extracting AP.exe zip")) {
    Import-Module "Microsoft.PowerShell.Archive" -Force
    Microsoft.PowerShell.Archive\Expand-Archive -LiteralPath $downloaded_zip -DestinationPath $destination -Force
    Remove-Item $downloaded_zip
}

Write-Output "Download complete, installed to $destination"


$IsWin = $IsWindows -or ($PSVersionTable.PSVersion.Major -lt 6)
if ($IsWin) {
    $paths = [Environment]::GetEnvironmentVariables("User").Path -split [IO.Path]::PathSeparator
    if ($paths -notcontains $destination) {
        Write-Output "Adding AP.exe to PATH"
        $paths = $paths | Where-Object { $_ }
        $paths += $destination
        $user_path = ($paths -join [IO.Path]::PathSeparator)
        if ($PsCmdlet.ShouldProcess("PATH", "Adding $destionation to PATH")) {
            [Environment]::SetEnvironmentVariable("Path", $user_path, "User")
            # this change to so that the process env vars have access
            $env:Path += ([IO.Path]::PathSeparator + $destination)
        }
    }
}
# TODO Unix/MacOs symlinking

$check_environment = $null
if ($PsCmdlet.ShouldProcess("AnalysisPrograms.exe", "Checking the install")) {
    if ($IsWin) {
        . "$destination/AnalysisPrograms.exe" CheckEnvironment
        $check_environment = $LASTEXITCODE
    }
    else {
        mono "$destination/AnalysisPrograms.exe" CheckEnvironment
        $check_environment = $LASTEXITCODE
    }

    if ($check_environment -ne 0) {
        throw "Unable to run AP.exe. There is some problem with your setup."
    }
}

return [PSCustomObject]@{
    Type             = $type
    Source           = $source
    ResolvedVersion  = $exact_version
    Destination      = $destination
    AssetUrl         = $asset_url
    EnvironmentCheck = $check_environment
}