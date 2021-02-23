#!/usr/bin/pwsh -l

#Requires -Version 7

<#
.SYNOPSIS
Downloads and "installs" (or updates), checks the installation, or uninstalls AP.exe
.DESCRIPTION
This script will install, check or uninstall QUT Ecoacoustic's AnalysisPrograms.exe. The defaults for each OS are different:
  - Windows: will install to HOMEDIR and add the directory to the PATH environment variable.
  - Linux will
    - install to $HOME/.local/share and symlink to $HOME/.local/bin
    - install sox if necessary (requires sudo)
  - MacOS: will install to $HOME/.local/share and symlink to $HOME/.local/bin

AP.exe does not automatically update itself and currently is not integrated with any system package managers.
.PARAMETER Install
Indicates an install should occur. This is implied if none of Install, Check, or Uninstall are supplied.
.PARAMETER Check
Indicates an that the installation of AP should be checked (implied when -Install used)
.PARAMETER Uninstall
Indicates an uninstall should occur
.PARAMETER Prerelease
Get the latest pre-release version instead of the latest stable version
.PARAMETER Version
The AP.exe version to download from GitHub. Must be a specific version (e.g. "18.8.1.2"). If not supplied the latest version is installed.
.PARAMETER Destination
Where to "install" AP.exe to.
.PARAMETER DontAddToPath
By default, the installer will place a shortcut AP.exe in a place where other programs can easily locate it. On Linux it will symlink to from a bin path to the install location.
To prevent this, use the DontAddToPath switch.
.PARAMETER GithubApiToken
An optional token used to avoid rate limiting by the GitHub API.

If not provided and an environment variable named "GITHUB_AUTH_TOKEN" exists, then the value of GITHUB_AUTH_TOKEN will be used.
.PARAMETER Force
Force overwrite an existing installation
.PARAMETER Quiet
Supress informational messages (overrides $InformationalPreference)
.PARAMETER PassThru
If used will return an object with information about the install result.
.INPUTS
None
.OUTPUTS
An object describing the install if -PassThru specified, otherwise nothing.
.EXAMPLE
./download_ap.ps1
Install the latest stable version
.EXAMPLE
./download_ap.ps1 -Prerelease
Install the latest prerelease version
.EXAMPLE
./download_ap.ps1 -Version "v20.11.2.0"
Install a specific version
.EXAMPLE
./download_ap.ps1 -Install
Specify the install action explicitly
.EXAMPLE
./download_ap.ps1 -Quiet
Disable status messages
.EXAMPLE
./download_ap.ps1 -Check
Check AP's installation
.EXAMPLE
./download_ap.ps1 -Uninstall
Uninstall AP
.LINK
https://ap.qut.ecoacoustics.info/basics/installing.html
#>

[CmdletBinding(
    DefaultParameterSetName = "Install-Latest",
    HelpURI = "https://ap.qut.ecoacoustics.info/basics/installing.html",
    SupportsShouldProcess = $true
)]
param(
    [Parameter(ParameterSetName = "Install-Latest")]
    [Parameter(ParameterSetName = "Install-Version")]
    [switch]$Install,
    [Parameter(ParameterSetName = "Check")]
    [switch]$Check,
    [Parameter(ParameterSetName = "Uninstall")]
    [switch]$Uninstall,

    [Parameter(ParameterSetName = "Install-Version")]
    [string]$Version = $null,

    [Parameter(ParameterSetName = "Install-Latest")]
    [switch]$Prerelease = $false,

    [Parameter(ParameterSetName = "Install-Latest")]
    [Parameter(ParameterSetName = "Install-Version")]
    [switch]$FxDependent = $false,

    [Parameter(ParameterSetName = "Install-Latest")]
    [Parameter(ParameterSetName = "Install-Version")]
    [switch]$DontAddToPath,

    [Parameter(ParameterSetName = "Check")]
    [switch]$DontCheckPath,

    [Parameter()]
    [string]$Destination,

    # GitHub API token used to bypass rate limiting
    [Parameter()]
    [string]
    $GithubApiToken,

    [Switch]$Force,

    [switch]$Quiet,

    [switch]$PassThru
)
$ErrorActionPreference = "Stop"
# https://docs.microsoft.com/en-us/powershell/scripting/learn/deep-dives/everything-about-shouldprocess
if ($Force){
    $ConfirmPreference = 'None'
}
if ($Quiet) {
    $InformationPreference = 'SilentlyContinue'
} else {
        $InformationPreference = 'Continue'
}
# When pwsh is run without a profile the output encoding reverts to the system code page on Windows
$OutputEncoding = [console]::InputEncoding = [console]::OutputEncoding = New-Object System.Text.UTF8Encoding
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
# alias $PsCmdlet. When we have sub functions $PsCmdlet is redefined in them and we need access to the parent value
# we were using the `script:` scope modifier but that doesn't work when this script is evaluated as a dynamic function
# - like when its downloaded and run from the internet
$cmdlet = $PsCmdlet

function PickForOS($winValue, $linuxValue, $macValue) {
    if($IsWindows) {
        return $winValue
    } elseif ($IsLinux) {
        return $linuxValue
    } elseif ($IsMacOS) {
        return $macValue
    } else {
        throw "Unknown operating system"
    }
}

if (!$Destination) {
    $Destination = PickForOS "${home}\AP" "${home}/.local/share/AP" "${home}/.local/share/AP"
}

$headers = if ($GithubApiToken) {
        Write-Debug "Using github auth token from script argument"
        @{"Authorization" = "token $GithubApiToken"}
    } elseif ($env:GITHUB_AUTH_TOKEN) {
        Write-Debug "Using github auth token from environment variable GITHUB_AUTH_TOKEN"
        @{"Authorization" = "token $env:GITHUB_AUTH_TOKEN"}
    } else {
        Write-Debug "Not using a github auth token"
        @{}
    }

$base_indent = "- "
$indent = "  - "
$indent2 = "    - "


$path_path = PickForOS $Destination "${home}/.local/bin" "${home}/.local/bin"
$path_delimiter = [System.IO.Path]::PathSeparator
$dir_seperator = [System.IO.Path]::DirectorySeparatorChar
$symlink_path = PickForOS $null "${home}/.local/bin/AnalysisPrograms" "${home}/.local/bin/AnalysisPrograms"
$alias_path = PickForOS "${Destination}${dir_seperator}AP.exe" "${home}/.local/bin/AP" "${home}/.local/bin/AP"
$binary_name = PickForOS "AnalysisPrograms.exe"  "AnalysisPrograms"  "AnalysisPrograms"
$ap_path = "$Destination$dir_seperator$binary_name"
$version_regex = '\d{2}\.\d{1,2}\.\d{1,2}\.\d{1,2}'

if ($Version) {

    # strip the leading v if it is present
    $Version = $Version -replace "^v", ""

    if ($Version -notmatch $version_regex ) {
        Write-Error "The version argument '$Version' did not look like a version (w.x.y.z)"
        exit 1
    }
}

# resolve metadata for asset
function Get-AssetUrl($prerelease ) {
    # Begin non-volitile block (this section always runs despite -whatif but it also
    # should have no side effects)
    $oldWhatIfPreference = $WhatIfPreference
    try {
        $WhatIfPreference = $false
        $github_url = "https://api.github.com/repos/QutEcoacoustics/audio-analysis/releases"
        if (!$Version) {
            if  (!$prerelease) {
                $github_url += "/latest"
            }
        }
        else {
            $github_url += "/tags/v$Version"
        }

        # not supported
        #AP_win-x64_Debug_v20.11.2.0.zip
        #AP_linux-x64_Debug_v20.11.2.0.tar.xz
        #AP_osx-x64_Debug_v20.11.2.0.tar.xz

        $x64 = [Environment]::Is64BitOperatingSystem
        $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
        $is_musl = $IsLinux -and ((ldd --version *>&1 | join-string) -match "musl")

        $build = switch ($arch) {
            #AP_any_v20.11.2.0.tar.xz
            { $FxDependent } { "any_v" }
            #AP_linux-musl-x64_v20.11.2.0.tar.xz
            {$x64 -and $IsLinux -and $is_musl } { "linux-musl-x64_v" }
            #AP_win-arm64_v20.11.2.0.zip
            { $_ -eq "Arm64" -and $IsWindows } { "win-arm64_v" }
            #AP_linux-arm64_v20.11.2.0.tar.xz
            { $_ -eq "Arm64" -and $IsLinux } { "linux-arm64_v" }
            #AP_linux-arm_v20.11.2.0.tar.xz
            { $_ -eq "Arm" -and $IsLinux } { "linux-arm_v" }
            #AP_linux-x64_v20.11.2.0.tar.xz
            { $_ -eq "X64" -and $IsLinux } { "linux-x64_v" }
            #AP_win-x64_v20.11.2.0.zip
            { $_ -eq "X64" -and $IsWindows } { "win-x64_v" }
            #AP_osx-x64_v20.11.2.0.tar.xz
            { $_ -eq "X64" -and $IsMacOS } { "osx-x64_v" }
            default { $null }
        }

        Write-Debug "${indent}Build $build chosen, x64: $x64, arch: $arch, is musl: $is_musl"
        if ($null -eq $build) {
            throw "Unknown platform. We could not detect your platform or we do not have a build for your platform."
        }
        Write-Debug "${indent}Querying GitHub with $github_url"
        $response = Invoke-RestMethod -Method Get -Uri $github_url -Headers $headers
        $response = $response | Select-Object -First 1
        $asset_url = $response.assets `
            | Where-Object { $_.name -like "*$build*" } `
            | ForEach-Object browser_download_url

        if($asset_url.Count -gt 1)  {
            throw "More than one asset matched criteria, need one url to continue. Urls: $asset_url"
        }

        $final_version = $response.tag_name -replace "^v",""
        $is_prerelease = $response.prerelease
        Write-Information "${indent}Found release $($response.tag_name) from GitHub"
        return [pscustomobject]@{Version = $final_version ; Url =  $asset_url ; IsPrerelease = $is_prerelease}
    }
    finally {
        $WhatIfPreference = $oldWhatIfPreference
    }
}

function New-InstallDirectory {
    # remove directory if it already exists
    if (Test-Path $Destination) {
        $message = "${indent}There was an existing install of AP. Continuing`n${indent2}will delete config files`n${indent2}will NOT delete log files`n${indent2}will delete everything else"
        if($Force -or $cmdlet.ShouldContinue("Deleting existing folder ``$Destination``?", $message)) {
            # hardcoding because this script can be executed anonymously - in that case it doesn't have a name we can use
            $script_name = "download_ap.ps1"
            Get-ChildItem -Path "$Destination" -Exclude "$script_name","Logs" | Remove-Item -Recurse -ErrorAction Stop
            Start-Sleep 1
        }
        else {
            Write-Error "User cancelled installation"
            return $false
        }
    }
    else {
        New-Item $Destination -ItemType Directory -ErrorAction Stop -Force | Write-Debug
        return $true
    }
}

function Get-Asset($asset_url) {
    if(!$asset_url) { throw "`$asset_url null or empty" }
    # download asset
    Write-Information "${indent}Downloading $final_version"
    $extension = switch ($asset_url) {
        { $_.EndsWith(".zip") } { ".zip" }
        { $_.EndsWith(".tar.xz") } { ".tar.xz" }
        Default { "Unspported extension for $asset_url"}
    }
    $downloaded_zip = "$Destination${dir_seperator}AP$extension"
    Write-Debug "${indent}Downloading $asset_url to $downloaded_zip"
    if ($cmdlet.ShouldProcess($asset_url, "Downloading asset")) {
        $curl = Get-Command curl, curl.exe -CommandType Application  -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($curl) {
            $curl_headers = @($headers.GetEnumerator() | ForEach-Object { "-H", "'$($_.Key): $($_.Value)'" })
            & $curl  @curl_headers -L -o "$downloaded_zip" "$asset_url" | ForEach-Object { "${index}$_" } | Write-Information
            if ($LASTEXITCODE -ne 0) {
                throw "Failed dowloading $asset_url using $curl"
            }
        }
        else {
            $ProgressPreference = 'SilentlyContinue'
            Invoke-RestMethod $asset_url -OutFile $downloaded_zip -TimeoutSec 60 -MaximumRetryCount 3 -Headers $headers
        }
    }

    # extract asset
    if ($cmdlet.ShouldProcess($downloaded_zip, "Extracting AP.exe")) {
        if ($extension -eq ".tar.xz") {
            # we should be only mac/linux at this branch
            tar --extract --xz --file $downloaded_zip --directory $Destination
            if ($LASTEXITCODE -ne 0) {
                throw "Failed extracting $downloaded_zip using tar"
            }
        }
        elseif (Get-Command unzip -CommandType Application -ErrorAction SilentlyContinue) {
            unzip -q -o $downloaded_zip -d $Destination

            if ($LASTEXITCODE -ne 0) {
                throw "Failed extracting $downloaded_zip using unzip"
            }
        }
        elseif (Get-Command tar -CommandType Application -ErrorAction SilentlyContinue) {
            tar --extract --xz --file $downloaded_zip --directory $Destination
            if ($LASTEXITCODE -ne 0) {
                throw "Failed extracting $downloaded_zip using tar"
            }
        }
        else {
            # the archive module has given us lots of problems - we try to avoid it if possible
            Import-Module "Microsoft.PowerShell.Archive" -Force
            Microsoft.PowerShell.Archive\Expand-Archive -LiteralPath $downloaded_zip -DestinationPath $Destination -Force

        }
        Remove-Item $downloaded_zip
    }

    Write-Information "${indent}Download complete, installed to $Destination"
}

function Remove-InstallDirectory {
    if (Test-Path $Destination) {
        if($Force -or $cmdlet.ShouldContinue("Are you sure you want to delete the folder ``$Destination``?", "${indent}Continuing will delete everything in $Destination")) {

            Remove-Item $Destination -Recurse -Force
            Start-Sleep 1
            return $true
        }
        else {
            Write-Error "User cancelled uninstall"
            return $false
        }
    }
    else {
        Write-Information "${indent}$Destination does not exist. Nothing to do."
        return $null
    }
}

function Test-Sox {
    !!(Get-Command "sox" -CommandType Application -ErrorAction SilentlyContinue)
}
function Install-SoxOnLinux {
    if (!$IsLinux) {
        throw "Install SoX only works on Linux"
    }
    if($Force -or $cmdlet.ShouldContinue("Install SoX?", "${indent}SoX not detected. We will attempt to install it")) {
        $package_manager = "apt-get", "yum" | Get-Command  -CommandType Application -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name -First 1
        $package_manager += switch ($package_manager) {
            "apt-get" { " -o=Dpkg::Use-Pty=0 -qq -y" } # suppress progress bar
            "yum" { " -q -y" }
            Default {
                throw "Could not detect package manager: $package_managage"
            }
        }

        # wild west here
        Invoke-Expression "sudo $package_manager install sox" *>&1 | Write-Information -OutBuffer 1000

        if ($LASTEXITCODE -ne 0) {
            throw "Failed installing sox with $package_manager"
        }

        return $true
    }
    else {
        Write-Error "User cancelled installation"
    }
}

function Test-ApDirInPath {
    # test path in PATH, make sure path matches wholly and not a subset of a path
    # by splitting the path on it's delimiter
    ($env:PATH -split $path_delimiter) -contains $path_path
}


# ensure updates to these variables in nested scopes propagate back to this scope
"installed_version", "install_info", "update_info" | Foreach-Object { Set-Variable -Name $_ -Value $null -Option AllScope }

$actions =  [ordered]@{
    "Check User Directory" = @{
        "Install" = $null
        "Check" = ("User home directory is found at $home", { Test-Path $home } )
        "Uninstall" = $null
    }
    "Resolve asset" = @{
        "Install" = ("Querying GitHub for releases", {
            $install_info = Get-AssetUrl $Prerelease
            return $install_info.Url.Length -gt 0

        } )
        "Check" =  ("Querying GitHub for updates", {
            $update_info = Get-AssetUrl $true
            return $true
        } )
        "Uninstall" = $null
    }
    "Install Directory" = @{
        "Install" = ({"Installing to $Destination"}, { New-InstallDirectory } )
        "Check" = ({"AP directory $Destination exists"}, { Test-Path $Destination } )
        "Uninstall" = ({"Deleting all files in $Destination"}, { Remove-InstallDirectory } )
    }
    "Place AP" = @{
        "Install" = ({"Downloading to $Destination"}, {
            Get-Asset $install_info.Url
            return $true
         } )
        "Check" =  ({"AP executable should be in $Destination"}, { Test-Path -PathType Leaf $ap_path } )
        "Uninstall" = $null
    }
    "Check permissions" = @{
        "Install" = ("Allow AP to have execute permission", {
            if($IsWindows) { return $null }  chmod +x $ap_path
            if ($LASTEXITCODE -ne 0) { return $false } else { return $true }
        } )
        "Check" =  ("AP executable should have execute permission", { $(test -x $ap_path ; 0 -eq $LASTEXITCODE) } )
        "Uninstall" = $null
    }
    "Check AP runs" = @{
        "Install" = $null
        "Check" =  (
            "AP should be able to run",
            {
                $installed_version = & "$ap_path" --version | Select-Object -Last 1
                return $LASTEXITCODE -eq 0
            }
        )
        "Uninstall" = $null
    }
    "Check version" = @{
        "Install" = $null
        "Check" =  (
            { "AP version $installed_version should match desired version $Version" },
            {
                if ($Version) {
                    return $installed_version -eq $Version
                } else {
                    return $installed_version -match $version_regex
                }
            }
        )
        "Uninstall" = $null
    }
    "Is SoX available" = @{
        "Install" = ("Install SoX", {
            if (!$IsLinux) { return $null }
            if (Test-Sox) { return $true }
            return Install-SoxOnLinux
        } )
        "Check" =  ({ "Check SoX is available" }, { if (!$IsLinux) { return $null } else { return Test-Sox } })
        "Uninstall" = $null
    }
    "AP checks its environment" = @{
        "Install" = $null
        "Check" =  ("AP checking its environment", { $output = & "$ap_path" CheckEnvironment ; if ($LASTEXITCODE -ne 0) { $output | Write-Error ; return $false } else { return $true } }  )
        "Uninstall" = $null
    }
    "Modify Path" = @{
        "Install" = ({"Add $path_path to `$PATH"}, {
            if ($DontAddToPath) { return $null }
            if (Test-ApDirInPath) { return $true }
            $to_add = $path_delimiter + $path_path
            if ($IsWindows) {
                $new_path = [System.Environment]::GetEnvironmentVariable("Path","User") + $to_add
                [System.Environment]::SetEnvironmentVariable("Path", $new_path, "User")
            } else {
                # bash syntax
                "PATH=`$PATH$to_add`n" | Out-File "$HOME/.profile" -Encoding utf8NoBOM -Append
            }
            # update current process path as well
            $env:PATH += $to_add
            return $true
        } )
        "Check" = ({"The `$PATH variable contains the path $path_path"}, {
            if ($DontCheckPath) { return $null }
            Test-ApDirInPath
        } )
        "Uninstall" = ("Removing $path_path from `$PATH variable", {
            if(!$IsWindows) {
                # the path added to PATH for mac/linux is a general system bin path
                # - we shouldn't mess with it or remove it, even if we added it
                return $null
            }
            if (Test-ApDirInPath) {
                $to_remove = "(^|$path_delimiter)" + [Regex]::Escape($path_path)
                if ($IsWindows) {
                    $new_path = [System.Environment]::GetEnvironmentVariable("Path","User") -replace $to_remove,""
                    [System.Environment]::SetEnvironmentVariable("Path", $new_path, "User")
                } else {
                    # bash syntax
                    $target = "PATH=`$PATH:$path_path`n"
                    (Get-Content "$HOME/.profile" -Raw) -replace $target,"`n" | Out-File "$HOME/.profile" -Encoding utf8NoBOM -Append
                }
                # update current process path as well
                $env:PATH =  $env:PATH -replace $to_remove,""
                return $true
            } else {
                return $null
            }
        } )
    }
    "Symlink for AP" = @{
        "Install" = ({"Symlink $symlink_path to $ap_path"}, { if($null -eq $symlink_path) { return $null } New-Item -Type SymbolicLink -Force -Path $symlink_path -Target $ap_path | Out-Null ;  return $true  } )
        "Check" = ({"$symlink_path exists and points to $ap_path"},  {
            if($null -eq $symlink_path) { return $null }
            return (Test-Path $symlink_path) `
              -and ((Get-Item $symlink_path).LinkType -eq 'SymbolicLink') `
              -and ((Get-Item $symlink_path).Target -eq $ap_path)
        } )
        "Uninstall" = ("Removing symlink", {
            if($null -eq $symlink_path) { return $null }
            if(Test-Path $symlink_path) {
                Remove-Item $symlink_path
                return  $true
            }
            else {
                Write-Debug "symlink path $symlink_path does not exist, skipping"
                return $true
            }
        } )
    }
    "Symlink alias for AP" = @{
        "Install" = ({"Symlink $alias_path to $ap_path"}, {
            if($null -eq $alias_path) { return $null }

            try {
                New-Item -Type SymbolicLink -Force -Path $alias_path -Target $ap_path | Out-Null ;
            }
            catch {
                Write-Warning "Failed to symlink AP. Are you running with administrator privileges?"
                $_ | Out-String | Write-Debug
            }
            return  $true
        } )
        "Check" = ("$alias_path exists and points to $ap_path",  {
            return (Test-Path $alias_path) `
              -and ((Get-Item $alias_path).LinkType -eq 'SymbolicLink') `
              -and ((Get-Item $alias_path).Target -eq $ap_path)
        } )
        "Uninstall" = ("Removing alias symlink", {
            if(Test-Path $alias_path) {
                Remove-Item $alias_path
                return  $true
            }
            else {
                Write-Debug "alias path $alias_path does not exist, skipping"
                return $null
            }
        } )
    }
}

$success_symbol = "✅"
$skip_symbol = "➖"
$fail_symbol = "❌"

if ($IsWindows -and !$env:WT_SESSION) {
    # we're on windows and not in Windows Terminal (but rather conhost.exe)
    # conhost does not support emoji so change success indicators
    $success_symbol = "+"
    $skip_symbol = "~"
    $fail_symbol = "!"
}

function Invoke-APInstallTask($current, $key) {
    if ($null -eq $current) {
        Write-Debug "${indent} no task for $key, skipped"
        return
    }

    $message, $task = $current
    $err = $null
    $result = $null
    try {
        # using & operator to evaluate over Invoke-Command, because it avoids a
        # layer of indirection and makes tracking down errors easier
        $result = & $task
    }
    catch {
        $err = $_
    }

    if ($message  -is [scriptblock]) {
        $message = & $message
    }

    switch ($result) {
        { $err } {
             Write-Error "${indent}$fail_symbol [error] AP's installation has a problem: $message`n Error:$($err | Out-String)"
             exit 1
        }
        $false {
             Write-Error "${indent}$fail_symbol [error] AP's installation has a problem: $message failed"
             exit 1
        }
        $null { Write-Information "${indent}$skip_symbol [skipped] $message" }
        $true { Write-Information "${indent}$success_symbol [success] $message" }
        default {
            throw "unexpected result: $result"
        }
    }
}


function Invoke-APInstallTasks($actions, $key) {
    if (!$key) {
        throw "`$key was null or empty: $key"
    }

    foreach($step in $actions.GetEnumerator()) {
        $step_name, $tasks = $step.Key, $step.Value
        $current = $tasks[$key]

        # if installing then automatically run the check task after each install task
        $next =  if ("Install" -eq $key) {  $tasks["Check"] } else { $null }

        if ($current -or $next) {
            Write-Information ($base_indent + $step_name)
        }

        Invoke-APInstallTask $current $key

        if ($next) {
            Invoke-APInstallTask $next "Check"
        }
    }
}

function Write-Metadata() {
    if ($PassThru) {

        $resolved_version = $final_version
        $resolved_version = $resolved_version -replace "^v", ""

        return [PSCustomObject]@{
            InstallMetadata = $install_info
            UpdateInfo = $update_info
            UpdateAvailable = if($update_info -and $installed_version) { [version]"$($update_info.Version)" -gt [version]"$installed_version" } else { $null }
            InstalledVersion = $installed_version
            InstalledDestination = $Destination
        }
    }
}


switch -wildcard ($PsCmdlet.ParameterSetName) {
    "Install-*" {
        Invoke-APInstallTasks $actions "Install"
        Write-Information "$success_symbol Installed AP $installed_version"
     }
    "Check" {
        Invoke-APInstallTasks $actions "Check"
        Write-Information "$success_symbol Check complete for AP version $installed_version"
    }
    "Uninstall" {
        Invoke-APInstallTasks $actions "Uninstall"
        Write-Information "$success_symbol Uninstalled AP"
    }
    default {
        Write-Output "No action selected, no action taken, exiting"
        exit 1
    }
}

return Write-Metadata
