---
title: Manual Install
uid: basics-manual-install
---


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
7. [Optional] Add the install directory to your `PATH` environment variable
    - Instructions in the [Path](./path.md) document.
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

You should use the **Stable** release unless there is a recent
feature implemented or bug fix in the prerelease version that you need.
