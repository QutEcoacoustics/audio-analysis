README.txt

HOW TO USE THE CONCATENATION SOFTWARE

This document describes the command line arguments required to concatenate csv data files output by the index calculation software that works on files shorter than 24 hours.
This software concatenates csv files into blocks of 24 hours only.

The command line arguments are:

First argument MUST be the word "concatenateIndexFiles"

Arguments then follow in this order:

InputDataDirectories: string: the directory containing all the files of interest - the required files can be in subdirectories of any depth

OutputDirectory: string: the output directory. If it does not exist it will be created. If it exists contents will be overwritten.

DirectoryFilter: string: Typically the recording siteName is used as the filter pattern to select directories. It can also be used for naming the output files

FileStemName: string: the stem name given to the output files

StartDate: A .Net DateTimeOffset object: start dateTime must be in format readable into a DateTimeOffset object

EndDate:   A .Net DateTimeOffset object:  end  dateTime must be in format readable into a DateTimeOffset object

DrawImages: boolean: typically will set this true because want to obtain the false-colour spectrograms

IndexPropertiesConfig: string: full path to the IndexPropertiesConfig file - required only if DrawImages = true

ConcatenateEverythingYouCanLayYourHandsOn: bool: NEVER CHANGE THIS - SHOULD be FALSE by default.

TimeSpanOffsetHint: A .Net TimeSpan object: should be set to +1000 hours for Queensland. = TimeSpan.FromHours(10),

Verbose: bool: set = true when want to debug all the info.
