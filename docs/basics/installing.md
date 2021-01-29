---
uid: basics-installing
---
# Installing

If you're new to using _AP.exe_ we recommend following the instructions
in the <xref:tutorial-01> practical.

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

1. Prerequisite: install Powershell 7+
    - Go to <https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell>
      to find instructions for installing PowerShell
2. Then:

<!-- https://git.io/JtOo3 created with git.io and should point to-->
<!-- https://raw.githubusercontent.com/QutEcoacoustics/audio-analysis/master/build/download_ap.ps1 -->

### [Windows install](#tab/windows)

Run the following command in an elevated (_Run as Administrator_) prompt:

```powershell
'$function:i=irm "https://git.io/JtOo3";i' | pwsh -nop -ex B -c - 
```

Or, to install the prerelease version:

```powershell
'$function:i=irm "https://git.io/JtOo3";i -Pre' | pwsh -nop -ex B -c - 
```

### [Linux install](#tab/linux)

Run the following command:

```bash
echo '$function:i=irm "https://git.io/JtOo3";i' | pwsh -nop -c - 
```

Or, to install the prerelease version:

```bash
echo '$function:i=irm "https://git.io/JtOo3";i -Pre' | pwsh -nop -c - 
```

### [MacOSX install](#tab/osx)

Run the following command in _Terminal_:

```bash
echo '$function:i=irm "https://git.io/JtOo3";i' | sudo pwsh -nop -c - 
```

Or, to install the prerelease version:

```bash
echo '$function:i=irm "https://git.io/JtOo3";i -Pre' | sudo pwsh -nop -c - 
```

***

> [!NOTE]
> Please inspect <https://git.io/JtOo3"> which should point to
> <https://github.com/QutEcoacoustics/audio-analysis/blob/master/build/download_ap.ps1>
> prior to running these commands.
>
> We already know the script is safe, but you should verify
> the security and contents of any script from the internet you are not familiar
> with. The above command downloads a remote PowerShell script and executes it on
> your machine.

> [!WARN]
> The installer script is brand new. There may be bugs. No warranties provided.

## Uninstall

If you used our automatic install you can use the same script to uninstall:

### [Windows uninstall](#tab/windows)

Run the following command in an elevated (_Run as Administrator_) prompt:

```powershell
'$function:i=irm "https://git.io/JtOo3";i -Un' | pwsh -nop -ex B -c - 
```

### [Linux uninstall](#tab/linux)

Run the following command:

```bash
echo '$function:i=irm "https://git.io/JtOo3";i  -Un' | pwsh -nop -c - 
```

### [MacOSX uninstall](#tab/osx)

Run the following command in _Terminal_:

```bash
echo '$function:i=irm "https://git.io/JtOo3";i  -Un' | sudo pwsh -nop -c - 
```

## Manual Install

1. Go to our [releases](https://github.com/QutEcoacoustics/audio-analysis/releases) page
2. Select the version you want to download
    - Choose the _Latest release_ unless you specifically need features that are 
      available in a _Pre-release_
3. Scroll down to the assets section for your chosen release
4. Download the version of AnalysisPrograms suitable for your computer (see [Choosing the asset](#choosing-the-asset))
5. Extract the folder
    - It can be installed in any directory
    - We typically extract to a directory named `~\AP` or `~/.local/share/AP` on Linux
6. Make sure any [Prerequisites](#prerequisites) are installed
7. [Optional] Add the install directory to your PATH environment variable
8. Finally, check the install by running:

Run the following command:

### [Windows Check](#tab/windows)

```powershell
C:\AP\AnalysisPrograms.exe CheckEnvironment
```

### [Linux Check](#tab/linux)

```bash
/AP/AnalysisPrograms CheckEnvironment
```

### [MacOSX Check](#tab/osx)

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

There are two variants of AP.exe:

1. The **Stable** release is well tested used by QUT Ecoacoustics on our servers
    and is usually a few months old
2. The **Prerelease** release is automatically built weekly, every Monday. It has more
    features and bug fixes than the stable release but it also could have more
    bugs.

You should use the **Stable** release unless there is a specific feature or bug
you need.
