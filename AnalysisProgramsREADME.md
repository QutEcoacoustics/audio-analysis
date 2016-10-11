# AnalysisPrograms.exe

The `AnalysisPrograms.exe` (abbreviated here on in to `AP`) contains many sub programs. 
Usage is obtained by running the program without arguments:

    $ AnalysisPrograms.exe

Which will produce output like:
```
QUT Bioacoustic Analysis Program - version 16.10.3596.0 (DEBUG build, 11/10/2016 11:07 AM)
Git branch-version: master-e2a88694390d39216bfab3a88a77d21f96be2f4a
Copyright QUT 2016
An action is required to run the program, here are some suggestions:
Usage: AnalysisPrograms <action> options

Environment variables:
    AP_PLAIN_LOGGING  [true|false]       Enable simpler logging - the default is value is `false`
Global options:
    -d    -debug      [switch]        *  Do not show the debug prompt AND automatically attach a debugger. Has no effect in RELEASE builds
    -n    -nodebug    [switch]        *  Do not show the debug prompt or attach a debugger. Has no effect in RELEASE builds
    -l    -loglevel   [logverbosity]  *  Set the logging. Valid values: None = 0, Error = 1, Warn = 2, Info = 3, Debug = 4, Trace = 5, Verbose = 6, All = 7
    -v    -verbose    [switch]        *  Set the logging to be verbose. Equivalent to LogLevel = Debug = 4
    -vv   -vverbose   [switch]        *  Set the logging to very verbose. Equivalent to LogLevel = Trace = 4
    -vvv  -vvverbose  [switch]        *  Set the logging to very very verbose. Equivalent to LogLevel = ALL = 7
Actions:
  list - Prints the available program actions
    << no arguments >>
  help - Prints the full help for the program and all actions
    EXAMPLE: help spt
           will print help for the spt action

    -a  -actionname  [string]  1
```


A list of sub-programs can be obtained by running:

    $ AnalysisPrograms.exe list

And help for a specific action:

    $ AnalysisPrograms.exe help audio2csv

## Gotchas 

 - **Never** finish a double quoted argument with a backslash (`\`). The parsing rules for such 
cases are complicated and outside of our control. See 
[here](https://msdn.microsoft.com/en-us/library/system.environment.getcommandlineargs.aspx) for 
details.
 - You can test arguments with the `echoargs.EXE` program
 - If an input argument is an array (e.g. `directoryinfo[]`) any commas in the argument will delimit
the values. For example `"Y:\Results\abc, 123, doo-dah-dee"` will be parsed as
`"Y:\Results\abc"`, `" 123"`, `" doo-dah-dee"`. 

## Version numbering

Our program uses an automatic version numbering system. A version number such as `16.06.3430.0` can be deciphered as:

```
<2-digit-year>.<2-digit-month>.<number-of-repository-commits>.0
```

It is important that before building releases that the local repository is clean and up-to-date - otherwise the 
build number won't increment.

The version information is also printed when the program first starts:

```
QUT Bioacoustic Analysis Program - version 16.06.3430.0 (RELEASE build, 23/06/2016 11:29)
Git branch-version: master-16a11bad5e3c2423bb92386ec83773c700eb4be0
```

In the above output you can see the:
```
<program-name> - version <version-number> (<build-type> build, <assembly-creation-date>)
Git branch-version: <git-branch-when-built>-<lastest-commit-hash-when-built>
```
  

## Sub-program types
There are, in broad terms, these types of sub-programs:

 - Main actions
	 - Process large amounts of information (like `audio2csv`)
 - Development / small scale actions
	 - Small data / development entry points 
 - Utility actions
	 - DummyAnalyser
	 - audiocutter
 - Meta actions
	 - CLI usage
	 - `analysesavailable`

Most development actions correlate to an implementation of `IAnalyser` or a custom algorithm. For details on implementing the `IAnalyser` work-flow refer to [AboutIAnalyser.md](AudioAnalysis/AboutIAnalyser.md)

## Documentation for specific sub-programs

### Concatenate Index Files

This section describes the command line arguments required to concatenate csv data files output by 
the index calculation software that works on files shorter than 24 hours.
This software concatenates csv files into blocks of 24 hours only.



To get help:

    $ AnalysisPrograms.exe help concatenateindexfiles

Output:

```
...
Usage: AnalysisPrograms <action> options

Global options:
    -d   -debug     [switch]        *
    -n   -nodebug   [switch]        *
    -l   -loglevel  [logverbosity]  *
    -v   -verbose   [switch]        *
    -vv  -vverbose  [switch]        *
Action:
  concatenateindexfiles - Calls ConcatenateIndexFiles.Execute():  Concatenates multiple consecutive index.csv files.
    -input  -inputdatadirectories                       [directoryinfo[]]  One or more directories where the original csv files are located.
    -outp   -outputdirectory                            [directory]        Directory where the output is to go.
    -di     -directoryfilter                            [string]           Filter string used to search for the required csv files - assumed to be in directory path.
    -f      -filestemname                               [string]           File stem name for output files.
    -sta    -startdate                                  [nullable`1]       DateTime at which concatenation begins. If null, then start with earliest available file.
    -en     -enddate                                    [nullable`1]       DateTime at which concatenation ends. If missing|null, then will be set = today's date or last available file.
    -ti     -timespanoffsethint                         [nullable`1]       TimeSpan offset hint required if file names do not contain time zone info. NO DEFAULT IS SET
    -in     -indexpropertiesconfig                      [file]             User specified file containing a list of indices and their properties.
    -con    -concatenateeverythingyoucanlayyourhandson  [switch]           Set this true when want to concatenate longer than 24-hour recordings as in case of PNG data.
```

An example command: 

    $ AnalysisPrograms.exe concatenateindexfiles -input "Y:\Results\2016Apr14-135123 - Yvonne, Towsey.Acoustic, #93" -output "C:\Temp\concatOutput" -directoryfilter "GympieNP" -filestemname "GympieNP" -timespanoffsethint "10:00:00"

Other notes:
- `InputDataDirectories` (path): the directory containing all the files of interest - the required files can be in subdirectories of any depth
- `OutputDirectory` (path): the output directory. If it does not exist it will be created. If it exists contents will be overwritten.
- `DirectoryFilter` (string): Typically the recording siteName is used as the filter pattern to select directories. It can also be used for naming the output files
- `FileStemName` (string): the stem name given to the output files
- `StartDate`: A .Net DateTimeOffset object, start dateTime must be in format readable into a DateTimeOffset object, e.g. `2015-10-25T00:00:00+10:00`
- `EndDate`: A .Net DateTimeOffset object, end  dateTime must be in format readable into a DateTimeOffset object `2015-10-25T00:00:00+10:00`
- `DrawImages` (boolean): **HARDCODED TO TRUE CURRENTLY** typically will set this true because want to obtain the false-colour spectrograms
- `IndexPropertiesConfig` (path): full path to the IndexPropertiesConfig file - required only if DrawImages = true
- `ConcatenateEverythingYouCanLayYourHandsOn` (bool): **NEVER CHANGE THIS - SHOULD be FALSE by default.**
- `TimeSpanOffsetHint` (timespan): should be set to +1000 hours for Queensland. = TimeSpan.FromHours(10), e.g. `10:00:00`

