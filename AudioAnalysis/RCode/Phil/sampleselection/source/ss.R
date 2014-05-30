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
# flush output device. the stack can get stuck on an output file that 
# doesn't exist sometimes if execution is interrupted before dev.off is called
cat('clearing the workspace \n flushing dev \v')
rm(list = ls())
dev.flush()
#options(error = NULL)
options(error = traceback)
#options(error = utils::recover)


source('config.R')  #must be first
source('minutes.R')
source('events.R')
source('features.R')
source('cluster.R')
source('ranking.R')
source('evaluation.R')
source('random.sampling.R')
source('inspection.R')
source('tags.R')
source('util.R')
source('spectrogram.R')
source('audio.R')
source('output.R')
source('user.input.R')
source('indices.R')
source('lines.R')


SS <- function (from.step = NA, to.step = NA, use.lines = FALSE) {
    # Main entry point whcih runs the specified steps.
    #
    # Args: 
    #   from.step: string or int. The step name or number to start running from
    #              if ommitted, will run from the start
    #   to.step: string or int. The step name or number to finish at
    #              if ommited, will only run from.step. If both are ommitted, 
    #              will run all steps. If "end" will run until the end
    
    
    CheckPaths()
    ClearAccessLog()
    options(warn = 1)  #print warnings as they occur
    
    
    if (use.lines) {
        all.steps <- c('minute.list', 
                       '',
                       'subset',
                       'clustering',
                       'internal.distance',
                       'ranking',
                       'evaluation',
                       'inspect.samples')
    } else {
        all.steps <- c('minute.list',
                       'feature.extraction',
                       'subset',
                       'clustering',
                       'internal.distance',
                       'ranking',
                       'evaluation',
                       'inspect.samples')
    }
    

    

    
    
    if (is.na(from.step)) {
        from.step.num <- 1
    } else if (class(from.step) == 'character') {
        from.step.num <- match(from.step, all.steps)
    } else {
        # make sure the from step num is between 1 and the total number of steps
        from.step.num <- min(c(from.step, length(all.steps)))
        from.step.num <- max(1, from.step.num)
    }
    
    
    if (is.na(to.step) && !is.na(from.step)) {
        to.step.num <- from.step.num
    } else if (is.na(to.step) || to.step == 'end') {
        to.step.num <- length(all.steps)
    } else if (class(to.step) == 'character') {
        to.step.num <- match(to.step, all.steps)
    } else {
        #make sure the to step num is not less than the from step or more than the length
        to.step.num <- min(c(to.step, length(all.steps)))
        to.step.num <- max(to.step.num, from.step.num)
    }

    
    invalid <- c()
    if (is.na(to.step.num)) {
        invalid <- c(invalid, to.step)
    }
    if (is.na(from.step.num)) {
        invalid <- c(invalid, from.step)
    }
    
    
    if (length(invalid) > 0) {
        stop('invalid step listed: ', paste(invalid, collapse = ", "), '. Valid steps include: ', paste(all.steps, collapse = ", ")) 
    }
    
    Report(1, 'Executing steps', paste(all.steps[from.step.num:to.step.num], collapse = ', '))
    

    
    # Step 0:
    # Audio Event Detection
    # This is done by another program, currently birgit's matlab code
    # Events are detected. Frequency bounds, start time and duration
    # are written to a csv file for each audio file. 

    steps.aed <- list(
        function () {
            # Step 1: 
            # generate a list of minutes to use in as the target
            CreateTargetMinutes()
        },
        function () {
            # Step 2: 
            # feature extraction 
            # creates a new output file parallel to the events file
            DoFeatureExtraction() 
        },
        function () {
            # 
            CreateEventAndFeaturesSubset()
        },
        function () {
            # Step 3: 
            # creates a new output csv file, identical to the events file,
            # except for the addition of a "group" column.
            ClusterEvents() 
        },
        function () {
            # Step 4:
            # calculates the sum of distances between all pairs of 
            # events in each minute
            InternalMinuteDistances()
        },
        function () {
            # Step 5:
            # chooses samples based on cluster groups
            # outputs a list of minute samples to a csv file
            RankSamples(use.lines = FALSE)
        },
        function () {
            # Step 6:
            # Evaluates the Richness survey from ranked samples
            EvaluateSamples() 
        },
        function () {
            # Step 7:
            # output a series of spectrograms of the samples
            # with the events colorcoded by cluster
            InspectSamples()
        }
    )
    steps.lines <- list(
        function () {
            # Step 1: 
            # generate a list of minutes to use in as the target
            CreateTargetMinutes()
        },
        function () {
            # Step 2: 
            # with lines, feature extraction is done prior to this entry point
            # step 2 is empty
            
        },
        function () {
            # Step 2: 
            # subset
            CreateLinesSubset()
        },
        function () {
            # Step 3: 
            # creates a new output csv file, identical to the events file,
            # except for the addition of a "group" column.
            ClusterLines() 
        },
        function () {
            # Step 4:
            # calculates the sum of distances between all pairs of 
            # events in each minute
            InternalMinuteDistances.lines()
        },
        function () {
            # Step 5:
            # chooses samples based on cluster groups
            # outputs a list of minute samples to a csv file
            RankSamples(use.lines = TRUE)
        },
        function () {
            # Step 6:
            # Evaluates the Richness survey from ranked samples
            EvaluateSamples() 
        },
        function () {
            # Step 7:
            # output a series of spectrograms of the samples
            # with the events colorcoded by cluster
            InspectSamples()
        }
    )
    
    if (use.lines)  {
        for (s in from.step.num:to.step.num) {
            steps.lines[[s]]()
        }   
    } else {
        for (s in from.step.num:to.step.num) {
            steps.aed[[s]]()
        }
    }
    

    
    CleanupTempDir();
}






