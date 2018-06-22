# Installing

[![Build status](https://ci.appveyor.com/api/projects/status/ntf6vtuy5wnlww37/branch/master?svg=true)](https://ci.appveyor.com/project/QUTEcoacousticsResearchGroup/audio-analysis/branch/master)


## General

1. Choose what version of _AP.exe_ that you want
    - Stable: Well tested code, used on production servers, usually months old
    - Weekly: Automatic build based off the master branch.
        - Has many more features and many more bug fixes
        - Has many more bugs
    - Continuous: Builds that are done by the continuous integration system
        - Bleeding edge, always up to date
        - Bleeding edge, very likely will break
1. Download a package
    - Click on the appropriate link
        - [Stable](https://github.com/QutEcoacoustics/audio-analysis/releases/latest)
        - [Weekly](https://github.com/QutEcoacoustics/audio-analysis/releases)
        - [Continuous](https://ci.appveyor.com/project/QUTEcoacousticsResearchGroup/audio-analysis/build/artifacts)
    - Download the _Release_ version - it will be a ZIP file that starts with _Release_
1. Extract the contents to a directory on your computer
    - It can be installed in any directory
    - We typically extract to a directory named `C:\Temp\AP`
1. [Optionally] Add the directory to your system's `PATH` environment variable
    - This makes it easier to type _AP.exe_ commands

When you've downloaded and extracted the program you can check the install by
running:

```
AnalysisPrograms.exe CheckEnvironment
```

## MacOSX

The following additional dependencies are required for MaxOSX machines:

- mono (version 5.5 or greater)

## Unix

The following additional dependencies are required for Unix machines:

- mono (version 5.5 or greater)
- ffmpeg
- wavpack
- libsox-fmt-all
- sox
- shntool
- mp3splt
- libav-tools


## Coming soon: Docker

We have a `Dockerfile` that theoretically works (BETA warning). See the 
`build` directory to find it.