# audio-analysis

The source code for the QUT Ecoacoustics _AnalysisPrograms.exe_ program.

## Quick links

 - See the **[docs](./docs/README.md)** for instructions on
   - Downloading AnalysisPrograms.exe
   - Running the program
   - Understanding concepts
- See the **[Issues list](https://github.com/QutEcoacoustics/audio-analysis/issues)** for
  - reporting bugs
  - requesting new features
- See the **[Contributing guidelines](./CONTRIBUTING.md)** if you want to
  - Compile the code yourself
  - Make a contribution

## Description

*QUT Ecoacoustics Analysis Programs* is a software package that can perform a  suite of analyses on audio recordings of
the environment. Although the analyses are intended for long-duration recordings (1 – 24 hours), in fact they
can be performed on any audio file in a format supported by the software. Analysis Programs can:

- calculate of summary and spectral acoustic indices at variable resolutions
- produce long-duration, false-colour, multi-index spectrograms
- calculate critical statistics of annotations downloaded from an Acoustic Workbench
- run various acoustic event recognizers

All the analyses are performed by a single executable file, _AnalysisPrograms.exe_. 

## Citation

Use this citation in all publications that use data or concepts from this code base is required:

> Towsey, M., Truskinger, A., & Roe, P. (2017) Audio Analysis Software (Version 17.04.3813.0) \[Computer software\].
> Brisbane: QUT Ecoacoustics Research Group, https://github.com/QutEcoacoustics/audio-analysis

Additionally, depending on the analysis that was run, extra work may be required to be cited. Any such additional
citations will printed in the console and in the log file.

## Build status

[![Build status](https://ci.appveyor.com/api/projects/status/ntf6vtuy5wnlww37/branch/master?svg=true)](https://ci.appveyor.com/project/QUTEcoacousticsResearchGroup/audio-analysis/branch/master)

Weekly versions of AnalysisPrograms.exe are now built automatically at midnight Monday night.
You can get copies from the [Releases](https://github.com/QutBioacoustics/audio-analysis/releases) page.

Per-commit (the very latest) builds can be found from the AppVeyor
[artifacts](https://ci.appveyor.com/project/QUTEcoacousticsResearchGroup/audio-analysis/build/artifacts)
page.

# License

This project is very old. We've released the full history for the sake of maintainability and transparency.
Unfortunately this means all code before our open source release is not open sourced.

In practice this should never be a problem. We never use our old code, except for historical purposes, and you never
should need to either.

## Newer code - All commits after [a275d0bc5744ba43096b43de2ef2aee32dc14c18](https://github.com/QutEcoacoustics/audio-analysis/commit/a275d0bc5744ba43096b43de2ef2aee32dc14c18)

All code after a275d0bc5744ba43096b43de2ef2aee32dc14c18 (<time>2018-01-30T06:38:46Z</time>) are licensed under the 
[Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/)

## Older code

All commits before a275d0bc5744ba43096b43de2ef2aee32dc14c18 (<time>2018-01-30T06:38:46Z</time>) are not licensed under
an open source license. All rights and copyright are retained, however, the public has permission to view, link to, cite
the code on GitHub.
