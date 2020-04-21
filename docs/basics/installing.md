---
uid: basics-installing
---
# Installing

## Beginner Tutorial

If you're new to using _AP.exe_ we recommend following the instructions
in the <xref:tutorial-01> practical
to setup and install _AP.exe_

## Supported Platforms

- Any of the following platforms:
    - Windows 7 or newer
    - Mac OSX 10.9 or later
    - Any Debian based linux
    - Any Debian based linux container

- As well as the following CPu architectures
    - Intel x86 (64-bit only)
    - ARM (32 bit and 64-bit)

## Automatic install

The automatic install will download AP.exe and may install required perquisites.

1. Perquisite: install Powershell 6+
    - Go to <https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7>
2. Run the following command in a PowerShell prompt:

Automatic install command:

# [Windows](#tab/windows-automatic)

Run the following command in an elevated (_Run as Administrator_) prompt:

```
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "$t = \"$env:Temp\download_ap.ps1\"; (New-Object System.Net.WebClient).DownloadFile('https://raw.githubusercontent.com/QutEcoacoustics/audio-analysis/master/build/download_ap.ps1', $t); & $t; rm $t"
```

# [Linux](#tab/linux-automatic)

Run the following command:

```
sudo pwsh -NoProfile -ExecutionPolicy Bypass -Command "\$t = \"\$env:Temp/download_ap.ps1\"; (New-Object System.Net.WebClient).DownloadFile('https://raw.githubusercontent.com/QutEcoacoustics/audio-analysis/master/build/download_ap.ps1', \$t); & \$t; rm \$t"
```

# [MacOSX](#tab/osx-automatic)

Run the following command in _Terminal_:

```
sudo pwsh -NoProfile -ExecutionPolicy Bypass -Command "\$t = \"\$env:Temp/download_ap.ps1\"; (New-Object System.Net.WebClient).DownloadFile('https://raw.githubusercontent.com/QutEcoacoustics/audio-analysis/master/build/download_ap.ps1', \$t); & \$t; rm \$t"
```

***

> [!NOTE]
> Please inspect
> https://github.com/QutEcoacoustics/audio-analysis/blob/master/build/download_ap.ps1
> prior to running ensure safety. We already know it's safe, but you should verify
> the security and contents of any script from the internet you are not familiar
> with. All of these scripts download a remote PowerShell script and execute it on
> your machine.

## Manual Install

1. Go to our [releases](https://github.com/QutEcoacoustics/audio-analysis/releases) page
2. Select the version you want to download
    - Choose the _Latest release_ unless you specifically need features that are 
      available in a _Pre-release_
3. Scroll down to the assets section for your chosen release
4. Download the version of AnalysisPrograms suitable for your computer (see [Choosing the asset](#choosing-the-asset))
5. Extract the folder
    - It can be installed in any directory
    - We typically extract to a directory named `C:\AP` or `/AP` on Linux
7. Make sure any [Prerequisites](#prerequisites) are installed
6. Finally, check the install by running:

# [Windows](#tab/windows-automatic)
```bash
C:\AP\AnalysisPrograms.exe CheckEnvironment
```
# [Linux](#tab/linux-automatic)
```bash
/AP/AnalysisPrograms CheckEnvironment
```
# [MacOSX](#tab/osx-automatic)
```bash
/AP/AnalysisPrograms CheckEnvironment
```
***


### Choosing the asset

[!include[<Asset chooser>](<./assetChooser.html>)]


## Prerequisites

### Windows

None. Self contained download.

### MacOSX


None. Self contained download.

### Linux/Unix

The following additional dependencies may be required for Linux/Unix machines:

- **MAYBE**: ffmpeg 
    - a packaged version with AP.exe should work for all platforms except ARM and ARM64
- **MAYBE**:  wavpack
- libsox-fmt-all, sox
- libav-tools (on some distros only, not needed in Ubuntu 18)



## Build Packages

There are three packages AP.exe:

1. The **Stable** release is well tested used by QUT Ecoacoustics on our servers
    and is usually a few months old
2. The **Weekly** release is automatically built every Monday. It has more
    features and bug fixes than the stable release but it also could have more
    bugs.
3. The **Continuous** package is created every time there is a change to our
    code. It is the bleeding edge:  always up to date but conversely the most
    likely to have bugs.

You should use the **Stable** release unless there is a specific feature or bug
you need.







