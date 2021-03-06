# audio-analysis

The source code for the QUT Ecoacoustics _AnalysisPrograms.exe_ (_AP.exe_) program.
Documentation (_in progress_) can be found at <https://ap.qut.ecoacoustics.info/>.

## Quick links

 - Ask questions or start a discussion with us in the [discussions](https://github.com/QutEcoacoustics/audio-analysis/discussions) 🙂  
 - See the **[docs](https://ap.qut.ecoacoustics.info/)** for instructions on
   - Downloading AnalysisPrograms.exe
   - Running the program
   - Understanding concepts
- See the **[Issues list](https://github.com/QutEcoacoustics/audio-analysis/issues)** for
  - reporting bugs
  - requesting new features
- See the **[Contributing guidelines](./CONTRIBUTING.md)** if you want to
  - Compile the code yourself
  - Make a contribution
- Our documentation can be found at <https://ap.qut.ecoacoustics.info/>
    - The source files are in the `docs` folder and instructions for editing them are in [`docs/README.md`](./docs/)
- [Let us know](https://github.com/QutEcoacoustics/audio-analysis/wiki/Projects-and-people-using-AP.exe) if you're using AP.exe 

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

[![DOI](https://zenodo.org/badge/DOI/10.5281/zenodo.4274299.svg)](https://doi.org/10.5281/zenodo.4274299)

This citation should be used in all publications that use data, concepts, or results generated by AnalysisPrograms.exe or from this code base:

> Michael Towsey, Anthony Truskinger, Mark Cottman-Fields, & Paul Roe. (2020, November 15). QutEcoacoustics/audio-analysis: Ecoacoustics Audio Analysis Software v20.11.2.0 (Version v20.11.2.0). Zenodo. http://doi.org/10.5281/zenodo.4274299

Additionally, depending on the analysis that was run, extra work may be required to be cited. Any such additional
citations will printed in the console and in the log file.

## Build status

[![build](https://github.com/QutEcoacoustics/audio-analysis/actions/workflows/build.yml/badge.svg)](https://github.com/QutEcoacoustics/audio-analysis/actions/workflows/build.yml)
[![docs](https://github.com/QutEcoacoustics/audio-analysis/actions/workflows/docs.yml/badge.svg)](https://github.com/QutEcoacoustics/audio-analysis/actions/workflows/docs.yml)
[![docs](https://github.com/QutEcoacoustics/audio-analysis/actions/workflows/docs.yml/badge.svg)](https://github.com/QutEcoacoustics/audio-analysis/actions/workflows/docs.yml)
[![release](https://github.com/QutEcoacoustics/audio-analysis/actions/workflows/release.yml/badge.svg)](https://github.com/QutEcoacoustics/audio-analysis/actions/workflows/release.yml)

Weekly versions of AnalysisPrograms.exe are now built automatically at midnight Monday night.
You can get copies from the [Releases](https://github.com/QutBioacoustics/audio-analysis/releases) page.

Per-commit (the very latest) builds can be found from the [Actions](https://github.com/QutEcoacoustics/audio-analysis/actions) page.

# License

This project is very old. We've released the full history for the sake of maintainability and transparency.
Unfortunately this means all code before our open source release is not open sourced.

In practice this should never be a problem. We never use our old code, except for historical purposes, and you never
should need to either. If you do need to use any of this code, just raise an issue and we'll make an exeption
for your use case.

## Newer code - All commits after 2018-01-30

All code after [a275d0bc5744ba43096b43de2ef2aee32dc14c18](https://github.com/QutEcoacoustics/audio-analysis/commit/a275d0bc5744ba43096b43de2ef2aee32dc14c18) (<time>2018-01-30T06:38:46Z</time>) are licensed under the 
[Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/)

## Older code - All commits before 2018-01-30

All commits before [a275d0bc5744ba43096b43de2ef2aee32dc14c18](https://github.com/QutEcoacoustics/audio-analysis/commit/a275d0bc5744ba43096b43de2ef2aee32dc14c18) (<time>2018-01-30T06:38:46Z</time>) are not licensed under
an open source license. All rights and copyright are retained, however, the public has permission to view, link to, and cite
the code on GitHub.
