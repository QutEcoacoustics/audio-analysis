# Analyze Long Recordings (audio2csv)

Action      | audio2csv
Config file | \<any compatible config file\>

This action applies an analysis to a long recording. It works well with recordings
that are up 1440 minutes long.

This action takes a single long-duration audio recording as input, splits it into segments and calculates results and writes the output to several .csv files.
This analysis process supports **parallel** processing.

Analyze long recordings can run many types of analyses. The provided config file
determines what analyses is run.

To see a list of all analyses that can be used by _audio2csv_, execute:

```
AnalysisPrograms.exe analysesavailable
```

As the action name implies, the output of this analysis is a set of CSV files that can contain:

- recognized acoustic events
- summary indices
- spectral indices

## Usage

This section describes the command line arguments required to, for example, calculate acoustic indices derived from a single audio file.

To run the action, type:

```
$ AnalysisPrograms.exe audio2csv <options...>
```

Here is an example of a command line with abbreviated path names:

```
$ AnalysisPrograms.exe audio2csv -input "audioPath\\fileName.wav"  -output "outputPath\\directoryName" -config "configPath\\fileName.yml"
```

The above three paths are required arguments (the program will return a fatal error if they are not found on the command line). 

A REMINDER: All path names should be double quoted, BUT make sure you use plain double quotes (`".."`) and NOT so called _smart quotes_ (`“..”`). The paths to directories should *never* end in a forward-slash or back-slash.

## Options

Typically, analyses will also include optional arguments that take default values if they are not found on the command line. 

Here is more detail about the command line options:

-   `-input`: The path to the audio file to process
-   `-output`: The output will be placed the directory provided
-   `-config`:
    - **IMPORTANT**: The config file chosen will determine the analysis that is run
    - The config file contain parameters that unique for the chosen analysis
    - If the config file cannot be found, a fatal error will result
    - Typically, if a required parameter is not found in the config file, a default value will be used or in certain cases, a fatal error may result.
-   `-Channels`:
    - The default value is `null` (not specified) which means that all channels are used.
    - _AP.exe_ can process files with up to 8 channels.
    - [TODO: CHECK THIS] Channels are numbered, 0, 1, 2, … In the usual case of stereo, left channel = 0 and right channel = 1. Note that, although the software should be able to accept recordings where channel number is greater than two, this use has not been debugged and results are undefined.
-   `-MixDownToMono`: The default value is `true`. Typically, indices are calculated on the mixed down waveform.
-   `-Parallel`: If you have access to a multi-core CPU you can set this option to true. Otherwise, the segments will be cut and analysed in sequence.