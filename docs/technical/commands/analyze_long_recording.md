---
title: Analyze Long Recordings
uid: command-analyze-long-recording
---

# Analyze Long Recordings (audio2csv)

- **Command**: audio2csv
- **Config file**: \<any compatible config file\>

This command applies an analysis to a long recording. It works well with recordings
that are up to 1440 minutes (i.e. 24-hours) long.

This command takes a single long-duration audio recording as input, splits it
into segments and calculates results and writes the output to several .csv files.
This analysis process supports **parallel** processing.

Analyze long recordings can run many types of analyses. The provided config file
determines what kind of analysis is run.

To see a list of all analyses that can be used by _audio2csv_, execute:

```shell
AnalysisPrograms.exe analysesavailable
```

As the command name implies, the output of this analysis is a set of CSV files
that can contain:

- recognized acoustic events
- summary indices
- spectral indices

## Usage

This section describes the command line arguments required to, for example,
calculate acoustic indices derived from a single audio file.

To run the command, type:

```shell
$ AnalysisPrograms.exe audio2csv [arguments] [options]
```

Here is an example of a command line with abbreviated path names:

```shell
$ AnalysisPrograms.exe audio2csv "audioPath\fileName.wav" "configPath\fileName.yml" "outputPath\directoryName"
```

The above three paths are required arguments in that order. The program will fail if they are not found on the command line. 

>Reminder: All path names should be double quoted, BUT make sure you use plain
>double quotes (`".."`) and NOT so called _smart quotes_ (`“..”`).

> Reminder: The paths to directories should *never* end in a forward-slash or 
>back-slash.

## Options

Typically, analyses will also include optional arguments that take default
values if they are not found on the command line. 

Here is more detail about the command line options:

- `Source`: The path to the audio file to process
- `Output`: The output will be placed the directory provided
- `Config`:
 - **IMPORTANT**: The name of the config file chosen will determine the kind of analysis that is run.
 - The config file contains parameters that are unique for the chosen analysis. If the config file cannot be found, _AP.exe_ will try and use a default. 
 - Typically, if a required parameter is not found in the config file, 
   a default value will be used or in certain cases, a fatal error may result.
- `--channels`: The default value is nothing (not specified) which means that all channels
   are used.  _AP.exe_ can process files with up to 8 channels.
 - [TODO: CHECK THIS] Channels are numbered, 0, 1, 2, … In the usual case of 
   stereo, left channel = 0 and right channel = 1. Note that, although the
   software should be able to accept recordings where channel number is greater
   than two, this use has not been debugged and results are undefined.
- `--mix-down-to-mono`: The default value is `true`. Typically, indices are
  calculated on the mixed down waveform.
- `--parallel`: If you have access to a multi-core CPU you can set this option
  to true. Otherwise, the segments will be cut and analysed in sequence.

Use the analysis-identifier option (`-a` or `--analysis-identifier`) followed by the `<analysis type>` to choose the
analysis to run. If you do this _AP_ will not have to guess the name of your config file and this your config file
can be named anyway you like.