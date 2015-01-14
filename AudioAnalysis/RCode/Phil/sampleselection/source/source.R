# takes care of constructing paths to source file
# such as events csvs and audio files



GetAedPath <- function (site, date, min = NULL) {
    # constructs the correct path to the appropriate AED csv and returns the path
    # 
    # Details:
    #   set source.path to the parent directory for the output. All directories within this
    #   should be named by the AED process and therefore should be consistent. 
    
    # this is where we moved the AED output to. The rest of the 
    source.path <- "/Volumes/My Passport/Phil#61/2015Jan05-093858 - AED_3.5_400, SERF 20, For Phil, #61, Part1"    
    path <- file.path(source.path, 'TaggedRecordings', site)
    # within path, there should be filders named after the files. 
    # for our recordings, the files are 24 hour recordings, so only 1 file per day
    
    # files have a UID which we don't need to worry about, followed by date, followed by start time which is always 0000
    # so, to not have to worry about the UID, we search through the directory for the file with the right date
    dirs <- list.dirs(path, recursive = FALSE)
    correct.dir <- NULL
    for (i in 1:length(dirs)) {
        if (grepl(date, dirs[i])) {
            correct.dir <- dirs[i]
            break()
        }   
    }
    if (is.null(correct.dir)) {
        stop('no matching AED for this date found')
    }
    path <- file.path(correct.dir, 'Ecosounds.AED')
    # search for the events csv containing all events for the day
    # this should be near the end alphabetically, so this shouldn't
    # be too inefficient if we reverse the vector first
    files <- rev(list.files(path, recursive = FALSE))
    correct.file <- NULL
    for (i in 1:length(files)) {
        if (grepl("Ecosounds.AED.Events.csv", files[i])) {
            correct.file <- files[i]
            break()
        }   
    }
    if (is.null(correct.file)) {
        stop('no matching AED for this date found')
    }
    return(file.path(path, correct.file))
}

RemoveUnwantedColsFromDir <- function (dirpath) {
    # removes unwanted columns from all csvs within a directory (not subdirectories)
    # see RemoveUnwantedCols function
    
    files <- list.files(dirpath, 
                        all.files = FALSE,
                        full.names = TRUE, 
                        recursive = FALSE,
                        include.dirs = FALSE)
    
    for (i in 1:length(files)) {    
        Report(5, 'removing unwanted cols from ', files[i])
        RemoveUnwantedCols(files[i])   
    }
    
}

RemoveUnwantedCols <- function (path) {
    # Removes unecessary columns from a CSV and saves it
    # files produced by AED are huge, because they have a bunch of unnecessary columns
    
    # this shows all the columns, with the ones to removed commented out
    to.keep <- c(
        'EventStartSeconds',
        'EventEndSeconds',
        'Duration',
        'MinHz',
        'MaxHz',
        #'FreqBinCount',
        #'FreqBinWidth',
        #'FrameDuration',
        #'FrameCount',
        #'SegmentStartOffset',
        #'Score',
        #'EventCount',
        #'FileName',
        #'StartOffset',
        #'SegmentDuration',
        'StartOffsetMinute'  # removed comma
        #'Bottom',
        #'Top',
        #'Left',
        #'Right',
        #'HitElements'
        )
    
    csv <- read.csv(path)
    csv2 <- csv[,to.keep]
    write.csv(csv2, path, row.names = FALSE)
    
}

