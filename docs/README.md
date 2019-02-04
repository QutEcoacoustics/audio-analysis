# QUT Ecoacoustics: AnalysisPrograms.exe manual


*QUT Ecoacoustics Analysis Programs* is a software package that can perform a 
suite of analyses on audio recordings of the environment. Although the analyses
are intended for long-duration recordings (1 – 24 hours approx), in fact they
can be performed on any audio file in a format supported by the software.
Typically, you would not use the software to analyse recordings shorter than one
 minute. Currently the software performs:

- calculation of summary and spectral acoustic indices at variable resolutions
- produces long-duration, false-colour, multi-index spectrograms
- can calculate critical statistics of annotations downloaded from an Acoustic
  Workbench
- is capable of running various acoustic event recognizers

All the analyses are performed by a single executable file, `AnalysisPrograms.exe`
(henceforth abbreviated to _AP.exe_), with command-line arguments determining
what analyses are to be done and on which files.

## Table of contents

- [Introduction](./introduction.md)
- [FAQ](./faq.md)
- [Downloading and Installing](./installing.md) AnalysisPrograms.exe
- [Introduction to running commands](./cli.md)
- [Commands](./commands.md)
- [Config Files](./config_files.md)
- [Supported Audio Formats](./formats.md)
- [List of Analyses](./analyses/)
- [Workflows](./workflows.md)
- [TODO: Citing the software]

Supplementary:

- [Version Numbers](./versioning.md)
- [Log files](./logs.md)
- [Code paths](./code_paths.md) that explain how code is executed
