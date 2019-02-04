# Concatenate Index Files

- **Command**: ConcatenateIndexFiles
- **Config file**: SpectrogramFalseColourConfig.yml
- **Config file2**: IndexPropertiesConfig.yml

This command also produces false-colour spectrograms. However instead of taking 
the spectral indices from a single audio file, it takes the total output from
multiple runs of the *audio2csv* command, and concatenates them to produce one
or more concatenated false-colour index spectrograms.

Typically, this command is used to produce a sequence of one or more 24-hour
false-colour spectrograms, where the original recordings can be anything from 30 minutes to 24 hours duration. 24-hour false-colour spectrograms are much easier to
interpret because sound-marks, such as the morning chorus, evening chorus, and
insect tracks, are easier to identify. False-colour index spectrograms
shorter than about 3 hours are difficult to interpret due to the lack of
soundscape context.

There is also an option with this command to concatenate the false-colour
spectrograms of every audio recording that can be found in a specified directory
into one large data-set and image. Due to memory constraints however, one would not
usually attempt to concatenate more than about 48 hours of recordings.

It is **strongly** recommended you only run this command on files from a single
acoustic sensor deployment - don't mix recordings from different sites or
deployments!

## Usage

This section describes the command line arguments required to concatenate the
output from multiple runs of the audio2csv command on shorter duration audio
recordings, that is less than 24-hours. Typically, the output is a sequence of
one or more 24-hour FC spectrograms.

To run the command, type:

```
$ AnalysisPrograms.exe concatenateIndexfiles [options]
```

## Options

Some of the option are obligatory (the program will return fatal error if they are not found on the command line) and some are optional. 

-   `-inputdatadirectories`: An array of one or more directories where the original csv files are located. The required files can be in subdirectories to any depth
-   `-inputdatadirectory`: A single directory where *all* the original csv files are located. This option exists (in addition to the above) as a hack to get around commas in paths conflicting with PowerArgs' array parsing feature.
-   `-outputdirectory`: The directory where the all the output is to go.
    - If it does not exist it will be created. If it exists contents will be overwritten.
-   `-directoryfilter`: Used as a pattern matcher to collect the required data CSV files, which are assumed to be in a matching directory or subdirectory. We often place the output in directories with same name as the recording file (including the extension).
    - Typically the recording siteName is used as the filter pattern to select directories. It can also be used for naming the output files
-   `-fileStemName`: User defined file stem name for the output files.
-   `-startdate`: A date at which concatenation is to begin. If null, then start with earliest available file. Can parse an ISO8601 date.
-   `-enddate`: A date at which concatenation ends. If null, then will be set equal to today's date available file. Can parse an ISO8601 date.
-   `-timeSpanOffsetHint`: A TimeSpan offset hint required if file names do not contain time zone offset info. NO DEFAULT IS SET.
-   `-indexpropertiesconfig`: User specified file as for the colourSpectrogram command.
-   `-falsecolourspectrogramconfig`: Config file for drawing the false colour spectrograms.
-   `-concatenateeverythingyoucanlayyourhandson`: Set true only when concatenating more than 24-hours of data into one image

## Other notes

-   StartDate: A .Net DateTimeOffset object, start dateTime must be in format readable into a DateTimeOffset object, e.g. 2015-10-25T00:00:00+10:00

-   EndDate: A .Net DateTimeOffset object, end dateTime must be in format readable into a DateTimeOffset object 2015-10-25T00:00:00+10:00

-   TimeSpanOffsetHint (timespan): should be set to +1000 hours for Queensland. = TimeSpan.FromHours(10), e.g. 10:00:00
