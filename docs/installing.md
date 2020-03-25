# Installing

## Beginner Tutorial

If you're new to using _AP.exe_ we recommend following the isntructions
in the <https://research.ecosounds.org/tutorials/ap/practical> practical
to setup and install _AP.exe_

## Supported Platforms

- Any Windows computer with v4.6.2 of the .NET Framework installed.
- Mac OS X 10.9 and later.
- Linux
    - Ubuntu 16.04 (used in production)
    - Debian 9 (out Docker image is based on Debian)
    - Raspian (not tested)
- Docker: <https://hub.docker.com/r/qutecoacoustics/audio-analysis/>
- Singularity (coming soon)


## Prerequisites

### Windows

- The .NET Framework v4.6.2 (Installed by default on most Windows computers)

### MacOSX

The following additional dependencies are required for MaxOSX machines:

- [Optional] [Powershell](https://docs.microsoft.com/en-us/powershell/scripting/setup/installing-powershell-core-on-macos?view=powershell-6)



```
$ AnalysisPrograms CheckEnvironment
```


### Unix

The following additional dependencies are required for Unix machines:

- ffmpeg
- wavpack
- libsox-fmt-all
- sox
- shntool
- mp3splt
- libav-tools (on some distros only, not needed in Ubuntu 18)
- [Optional] [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/setup/installing-powershell-core-on-linux?view=powershell-6)


```
$ AnalysisPrograms CheckEnvironment
```

## Automatic install

The prerequisites must be installed first.

### Windows

Run the following command in an elevated (_Run as Administrator_) prompt:

```
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "$t = \"$env:Temp\download_ap.ps1\"; (New-Object System.Net.WebClient).DownloadFile('https://raw.githubusercontent.com/QutEcoacoustics/audio-analysis/master/build/download_ap.ps1', $t); & $t; rm $t"
```

### MacOSX and Linux

Only supported if you installed PowerShell, otherwise see the _Manual Install_ 
instructions below.  

Run the following in _Terminal_:

```
    sudo pwsh -NoProfile -ExecutionPolicy Bypass -Command "\$t = \"\$env:Temp/download_ap.ps1\"; (New-Object System.Net.WebClient).DownloadFile('https://raw.githubusercontent.com/QutEcoacoustics/audio-analysis/master/build/download_ap.ps1', \$t); & \$t; rm \$t"
```

**NOTE**: Please inspect
https://github.com/QutEcoacoustics/audio-analysis/blob/master/build/download_ap.ps1
prior to running ensure safety. We already know it's safe, but you should verify
the security and contents of any script from the internet you are not familiar
with. All of these scripts download a remote PowerShell script and execute it on
your machine.

## Packages

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

## Manual Install

The prerequisites must be installed first.

1. Download the _Release_ Zip file from one of these links:
    - [Stable](https://github.com/QutEcoacoustics/audio-analysis/releases/latest)
    - [Weekly](https://github.com/QutEcoacoustics/audio-analysis/releases)
    - [Continuous](https://ci.appveyor.com/project/QUTEcoacousticsResearchGroup/audio-analysis/build/artifacts)
1. Extract the contents to a directory on your computer
    - It can be installed in any directory
    - We typically extract to a directory named `C:\AP` or `/AP` on Linux
1. [Optionally] Add the directory to your system's `PATH` environment variable. 
This makes it easier to type _AP.exe_ commands
1. Check the install by running:

    ```
    C:\AP\AnalysisPrograms.exe CheckEnvironment
    ```
    Or on MacOSX and Linux

    ```
    mono /AP/AnalysisPrograms.exe CheckEnvironment
    ```







