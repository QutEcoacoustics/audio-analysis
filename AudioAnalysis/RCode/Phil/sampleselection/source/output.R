# all functions to do with outputting and reporting

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

WriteOutput <- function (x, fn, new = FALSE, ext = 'csv') {
    # writes output files in a consistent way, 
    # with reporting
    Report(4, 'Writing output:', fn)
    path <- OutputPath(fn, new = new, ext = ext)
    write.csv(x, path, row.names = FALSE)
}

ReadOutput <- function (fn, ext = 'csv', false.on.missing = FALSE) { 
    # reads output files in a consistent way, 
    # with reporting
    #
    # Args:
    #   fn: string
    #   ext: string
    #   false.on.fail: boolean. If true, will check if exists and 
    #     return false if it is missing. If true, will cause an error
    #     if missing
    Report(4, 'reading', fn)
    path <- OutputPath(fn, ext = ext)
    if (false.on.missing && !file.exists(path)) {
        return(FALSE)
    }
    data <- read.csv(path, header = TRUE, stringsAsFactors=FALSE)
    return(data)
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
                      g.end.min, sites, g.percent.of.target, sep='.')
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
    temp.dir.name <- paste0(t$year, t$yday, hs)
    temp.dir.path <- file.path(parent.temp.dir, temp.dir.name)
    dir.create(temp.dir.path)
    return(temp.dir.path)   
}

CleanupTempDir <- function () {
    parent.temp.dir <- file.path(g.output.parent.dir, 'temp')
    unlink(parent.temp.dir, recursive = TRUE)
}



Report <- function (level, ..., nl = TRUE) {
    # prints output to the screen if the level is above the 
    # global output level. 
    #
    # Args:
    #   level: int; how important is this? 1 = most important
    #   ... : strings;  concatenated to form the message
    #   nl: boolean; whether to start at a new line
    if (level <= g.report.level) {
        if (nl) {
            nl <- "\n"
        } else {
            nl <- ""
        }
        cat(paste(c(nl, paste(as.vector(list(...)), collapse = " ")), collapse = ""))
    }
}

Dot <- function(level = 5) {
    #outputs a dot, used for feedback during long loops
    Report(level, " .", nl = FALSE)
}

Timer <- function(prev = NULL, what = 'processing', num = NULL, per = "each") {
    # used for reporting on the execution time of parts of the code
    if(is.null(prev)) {
        return(proc.time())
    } else {
        t <- (proc.time() - prev)[3]
        Report(3, 'finished', what, ':', round(t, 2), ' sec')
        if (!is.null(num)) {
            time.per.each <- round(t / num, 3)
            Report(3, " ", time.per.each, "per", per, nl = FALSE)
        } 
    }
}


