##
#  Entry point for sample selection
#  - Sets some global variables for the paths to the input and output data
#  - Function which calls each of the steps
#  - The steps are:
#
#  1. Merge Events.  Takes the events detected using the matlab AED script,
#     which are spread over many files, and merges them into a single csv,
#     adding extra columns to record the audio file, date, site and time.
#     (this info is contained in the filename of the input files
#
#  2. Feature extraction.  For each event in the file created in step 1, extacts
#     features. Outputs a table of features to a new file. This file is parallel
#     to the events file. I.e. each row of the events file corresponds to the
#     same row in the features table.
#
#  3. Clustering. clusters the events in the events table using the features
#     in the features table. Outputs a new table identical to the events table,
#     except with an extra column containing the
#     cluster number (aka group number)
#
#  4. Sample selection. Using the clusters table, selects a list of minutes.
#     Searches the tag data from SERF to output the number of species found
#     in those minutes.
#
##

# clear the workspace
# rm(list = ls())



source('config.R')
source('events.R')
source('features.R')
source('cluster.R')
source('samples.R')
source('tags.R')
source('util.R')
source('spectrogram.R')

test <- function (...) {
    
    
    return(...)
}

SS <- function (...) {
    # Main entry point whcih runs the specified steps.
    #
    # Args:
    #   events: Boolean; Whether to run events step
    #   feature.extraction: Boolean; Whether to run feature extraction
    #   clustering: Boolean; Whether to run clustering step
    #   sample.selection: Boolean; Whether to run sample selection step
    #   all: Boolean; Whether to run all steps. Overrides the above params

    
    CheckPaths()
    
    default.steps <- c('events',
                       'feature.extraction',
                       'clustering',
                       'sample.selection')
    steps <- unlist(list(...));
    
    if (length(steps) == 0) {
        steps <- default.steps;
    }
    
    
    # Step 1:
    # Audio Event Detection
    # This is done by another program, currently birgit's matlab code
    # Events are detected. Frequency bounds, start time and duration
    # are written to a csv file for each audio file. 
    
    
    if (!is.na(match('events', steps))) {
        
        # Step 2: 
        # merge events from several files (one for each audio file)
        # into a single csv file
        MergeEvents()
    }
    

    if (!is.na(match('feature.extraction', steps))) {
        # Step 3: 
        # feature extraction 
        # creates a new output file parallel to the events file
        DoFeatureExtraction() 
    }
    
    if (!is.na(match('clustering', steps))) {
        # Step 4: 
        # creates a new output csv file, identical to the events file,
        # except for the addition of a "group" column.
        ClusterEvents()  
    }
    
    if (!is.na(match('sample.selection', steps))) {
        # Step 5:
        # chooses samples based on cluster groups
        # outputs a list of minute samples to a csv file
        samples <- SelectSamples()
        EvaluateSamples(samples) 
    }
    
    
}

CheckPaths <- function () {
    # checks all global paths to make sure they exist
    # returns: 
    #   null
    #
    # if one of them doesn't exist, execution will stop
    
    to.test <- c(g.output.parent.dir, 
                 g.source.dir, 
                 g.audio.dir, 
                 g.events.source.dir)
    for (t in 1:length(to.test)) {
        if (!file.exists(to.test[t])) {
            stop(paste(to.test[t], 'path does not exist'))
        }
    }
    
}


OutputPath <- function (fn, new = FALSE, ext = 'csv') {
    # Returns the correct path for output of a particular type
    #
    # Args:
    #   fn: String; the name of the output file, eg "features"
    #   new: Boolean; whether to create a new output folder or 
    #     overwrite/use any values in the latest already created folder
    #
    # Returns: 
    #   String; Something like:
    #     "output/2010-10-13.80.2010-10-13.120.NW.SWBackup/1/events.csv"
    #  
    # Details:
    #   Used both by functions writing the output and reading previous output
    #   First creates a directory based on the start and end time and dates 
    #   and dates to be processed (so changing any of these will cause the  
    #   system to read and write to a different directory). Within this 
    #   directory output will be sent to a numbered folder, so that the user  
    #   has the option of keeping different versions of output with the same  
    #   start/end time/dates and sites. 
    
    # first create the output directory 
    sites <- paste(g.sites, collapse = ".")
    dir.name <- paste(g.start.date, g.start.min, g.end.date,
                      g.end.min, sites, sep='.')
    dir.name <- gsub(" ","", dir.name)
    output.dir <- file.path(g.output.parent.dir,dir.name)
    if (!file.exists(g.output.parent.dir)) {
        dir.create(g.output.parent.dir)
    }
    if (!file.exists(output.dir)) {
        dir.create(output.dir)
        dir.create(file.path(output.dir, "1"))
    }
    dirs <- list.files(path = output.dir, full.names = FALSE, 
                       recursive = FALSE)
    if (length(dirs) > 0) {
        v <- as.numeric(dirs[length(dirs)])
        if (is.na(v)) {
            warning('bad folder name')
        }
    } else {
        v <- 1   
    }
    if (new) {
        v <- v + 1
    }
    path <- file.path(output.dir,v)
    if (!file.exists(path)) {
        dir.create(path)
    }
    op <- file.path(path, paste(fn,ext, sep='.'))
    return(op)
}

TempDirectory <- function () {
    #creates a temporary folder within the temp directory
    #
    # Returns
    #    String. The path to the new temp directory
    #
    # Details
    #    generates the name of the directory based on the time
    #    to guarantee a unique directory name. 
    #    name is a concatenation of 
    #    - years since 1900, 
    #    - days since the start of the year
    #    - hundredths of a second since the start of the day
    
    
    parent.temp.dir <- file.path(g.output.parent.dir, 'temp')
    if (!file.exists(parent.temp.dir)) {
        dir.create(parent.temp.dir)
    }
    op <- options(digits.secs = 6)
    t <- as.POSIXlt(Sys.time())
    options(op)  
    s <- t$sec + t$min * 60 + t$hour * 100 * 60 * 60
    hs <- round(s * 100)
    print(t$year)
    temp.dir.name <- paste0(t$year, t$yday, hs)
    temp.dir.path <- file.path(parent.temp.dir, temp.dir.name)
    dir.create(temp.dir.path)
    return(temp.dir.path)   
}

IsWithinTargetTimes <- function (date, min, site) {
    # determines whether the given date, startmin and site are within
    # the start and end date and min and list of sites to process
    #
    #  Args:
    #    date: String; 
    #    min: Int;
    #    site: String
    #
    #  Returns:
    #    Boolean
    # 
    # Details:
    #   first tests for site, then
    #   constructs date-time strings and compares the strings
    require('stringr')
    
    
    if (!site %in% g.sites) {
        return(FALSE)
    }
    date <- FixDate(date)
    start.date <- FixDate(g.start.date)
    end.date <- FixDate(g.end.date)
    start.date.time <- paste(start.date, MinToTime(g.start.min))
    end.date.time <- paste(end.date, MinToTime(g.end.min))
    date.time <- paste(date, MinToTime(min))
    if (date.time >=  start.date.time && date.time <= end.date.time) {
        return(TRUE)
    } else  {
        return(FALSE)
    }
}

Report <- function (level, ...) {
    # prints output to the screen if the level is above the 
    # global output level. 
    if (level < g.report.level) {
        cat(paste(c(as.vector(list(...)), "\n"), collapse=" "))
    }
}


