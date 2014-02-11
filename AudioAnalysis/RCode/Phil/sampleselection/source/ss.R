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
cat('clearing the workspace \n')
rm(list = ls())


source('config.R')
source('events.R')
source('features.R')
source('cluster.R')
source('samples.R')
source('tags.R')
source('util.R')
source('spectrogram.R')
source('audio.R')
source('output.R')



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
    
    all.steps <- c('minute.list',
                       'events',
                       'feature.extraction',
                       'clustering',
                       'sample.selection',
                       'inspect.samples')
    steps <- unlist(list(...));
    
    if (length(steps) == 0) {
        steps <- all.steps;
    } else {
        diff <- setdiff(steps, all.steps)
        if (length(diff) > 0) {
            # if an invalid step is passed, give an error
            stop('invalid step listed: ', paste(diff, collapse = ", "), '. Valid steps include: ', paste(all.steps, collapse = ", "))
        }
    }
    
    
    # Step 0:
    # Audio Event Detection
    # This is done by another program, currently birgit's matlab code
    # Events are detected. Frequency bounds, start time and duration
    # are written to a csv file for each audio file. 
    
    
    if (!is.na(match('minute.list', steps))) {
        
        # Step 1: 
        # generate a list of minutes to use in as the target
        CreateMinuteList()
    }
    
    
    if (!is.na(match('events', steps))) {
        
        # Step 2: 
        # merge events from several files (one for each audio file)
        # into a single csv file
        CreateEventList()
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
        samples <- RankSamples()
        EvaluateSamples(samples) 
        
    }
    
    if (!is.na(match('inspect.samples', steps))) {
        # Step 6:
        # output a series of spectrograms of the samples
        # with the events colorcoded by cluster
        InspectSamples()
        
    }
    

    
    CleanupTempDir();
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




