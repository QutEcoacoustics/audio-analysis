#Audio Analysis

This directory contains the bulk of the editable code in the repository.

##C\# / F\# 

The following directories contain .NET code and related dependencies:


	- AED
	- Analysis.Test
	- AnalysisBase
	- AnalysisConfigFiles
	- AnalysisPrograms
	- AnalysisRunner
	- AudioAnalysisTools
	- AudioBrowser
	- Dong.Felt
	- FELT
	- QutBioacosutics.Xie
	- SammonProjection
	- TowseyLibrary

###AnalysisPrograms
`AnalysisPrograms` is the main project for the `AudioAnalysis2012.sln` solution. 

Most dependencies should be installed via `Nuget` (automatically restored when the solution is built) or checked in to the `..\ExtraAssemblies` folder.

The build produces `AnalysisPrograms\bin\[Debug|Release]\AnalysisPrograms.exe` which can run all analysis related functionality. Part of the build process copies all required dependencies into the `Debug` and `Release` folders making the build artefact portable. Dependencies copied include the `AnalysisConfigFiles\*` folder and the required executable for processing audio.

###Analysis Programs Architecture
####CLI
The `AnalysisPrograms.exe` (abbreviated here on in to `AP`) contains many sub programs. 
Usage is obtained by

    $ AP help


A list of sub-programs can be obtained by running:

    $ AP list

The list of programs is defined in `AnalysisPrograms\Production\Actions.cs`. All sub-programs have `Execute` or `Main` methods within the `AnalysisPrograms` project. If necessary these entry points simply redirect to main methods within other projects.

Command-line argument parsing is automatically done by a custom build of the `PowerArgs` library. This library has been earmarked for replacement with an alternative.

####Sub-program types
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

Most development actions correlate to an implementation of `IAnalyser` or a custom algorithm. For details on implementing the `IAnalyser` work-flow refer to [AboutIAnalyser.md](AboutIAnalyser.md)

####Logging
Every execution of `AP` produces a log file that is saved relative the executables' location in the `LogFiles` directory. Log files:
 
- Are automatically rolled over on every execution
- The latest log file will always be named `Log.txt`
- Up to 50 log files will be kept
- All StandardOutput should be directed to the log provider
	- **DO NOT USE THE `System.Console.Write*` methods**
	- Instead use the proper logger or the `LoggedConsole` helper class
- A new instance of the logger can be created (per class) with the following snippet:
    `private static read-only ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);`
- Logging level, output format, and destinations are controlled in `AnalysisPrograms\log4net.config`

####Production Code
Some code is used often enough to be depended on. Any code that matches the following criteria may be used but __should not be modified__ unless you know what you are doing. Code that falls under these guidelines includes

- All files in `AnalysisBase`
- All files in `..\Acoustics.Tools`
- All files in `..\ExtraAssemblies`
- `AnalysisPrograms\Main.cs`
- `AnalysisPrograms\AnalyseLongRecording.cs`
- `AnalysisPrograms\Production\**`
- All well used Action declarations (`AnalysisPrograms\Production\Actions.cs`)
	- Including: 
		- names (the names listed by usage)
		- arguments (properties/argument availability or format)


#Matlab
The following directory contains all Matlab related code:

    - Matlab

#RCode
The following directory contains all R Scripts:

    - RCode