# all functions to do with outputting and reporting

g.output.master.dir <- file.path(g.output.parent.dir, 'master')
g.hash.dir <- file.path(g.output.parent.dir, 'hash')

# events and features are stored in "master"
# clustering and ranking based on clusters are stored in folders for the different subsets. 
# a copy of the subset of the master features and events is moved to the folder
# a copy of the parameters for that subset is kept with it. 




CheckPaths <- function () {
    # checks all global paths to make sure they exist
    # missing output paths are created
    # missing source paths cause error
    # called at the end of this file
    
    warn.if.absent <- c( 
                 g.source.dir, 
                 g.audio.dir)
    create.if.absent <- c(
        g.output.parent.dir,
        g.output.master.dir,
        file.path(g.output.master.dir, g.all.events.version),
        g.hash.dir
        )
    lapply(warn.if.absent, function (f) {
        if (!file.exists(f)) {
            stop(paste(f, 'path does not exist'))
        }  
    })

    lapply(create.if.absent, function (f) {
        if (!file.exists(f)) {
            dir.create(f)
        }  
    })
    
}

WriteOutput <- function (x, fn, new = FALSE, ext = 'csv') {
    # writes output files in a consistent way, 
    # with reporting
    Report(4, 'Writing output:', fn)
    path <- OutputPath(fn, new = new, ext = ext)
    WriteOutputCsv(x, path)
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
    Report(4, 'Reading:', fn)
    path <- OutputPath(fn, ext = ext)
    if (false.on.missing && !file.exists(path)) {
        return(FALSE)
    }
    data <- ReadOutputCsv(path)
    return(data)
}

# read/write wrappers for csv with correct options set
ReadOutputCsv <- function (path) {
    return(read.csv(path, header = TRUE, stringsAsFactors=FALSE))
}
WriteOutputCsv <- function (x, path) {
    write.csv(x, path, row.names = FALSE)
}

SaveObject <- function (x, fn) {
    path <- OutputPath(fn, ext = 'object')
    f <- save(x, file = path)
    
}
ReadObject <- function (fn) {
    path <- OutputPath(fn, ext = 'object')
    if (file.exists(path)) {  
        load(path)
        return(x) # this is the name of the variable used when saving
    } 
    return(FALSE) 
}

WriteMasterOutput <- function (x, fn, ext = 'csv') {
    WriteOutputCsv(x, MasterOutputPath(fn, ext))
}

ReadMasterOutput <- function (fn, ext = 'csv', false.on.missing = FALSE) {
    p <- MasterOutputPath(fn, ext);
    if (file.exists(p)) {
        return(ReadOutputCsv(p))
    } else if (false.on.missing) {
        return(FALSE)
    } else {
        stop(paste("file doesn't exist: ", p))
    }
}

MasterOutputPath <- function (fn, ext = 'csv') {
    return(file.path(g.output.master.dir, g.all.events.version, paste(fn,ext, sep='.')))
}


OutputPath <- function (fn = FALSE, new = FALSE, ext = 'csv') {
    # Returns the correct path for output of a particular type
    #
    # Args:
    #   fn: String; the name of the output file, eg "features"
    #   new: Boolean; whether to create a new output folder or 
    #     overwrite/use any values in the latest already created folder
    #   ext: the extension of the file
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
    #   to get the current output path, leave fn empth
    #   to create a new folder for output without a particular file, just pass new = TRUE
    
    # first create the output directory 
    sites <- paste(g.sites, collapse = ".")
    dir.name <- paste(g.all.events.version, g.start.date, g.start.min, g.end.date,
                      g.end.min, sites, g.percent.of.target, sep='.')
    dir.name <- gsub(" ","", dir.name)
    output.dir <- file.path(g.output.parent.dir,dir.name)

    if (!file.exists(output.dir)) {
        dir.create(output.dir)
        dir.create(file.path(output.dir, "1"))
    }
    dirs <- list.files(path = output.dir, full.names = FALSE, 
                       recursive = FALSE)
    
    v <-suppressWarnings(max(round(as.numeric(dirs)), na.rm=TRUE))
     
    
    if (v < 1) {
        v <- 1
    } else if (new) {
        v <- v + 1
    }

    path <- file.path(output.dir,v)
    if (!file.exists(path)) {
        dir.create(path)
    }
    if (fn != FALSE) {
        op <- file.path(path, paste(fn,ext, sep='.'))
    } else {
        op <- path
    }

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

GetExistingMasterOutput <- function (output.name, not.if.changed) {
    # checks if any of the not.if.changed files have changed since this was last retrieved
    # and if not, returns the master output
    # if so, deletes the master output and returns FALSE
    require('digest')
    hash.name <- output.name
    new.content.hash <- HashFileContents(not.if.changed)
    old.content.hash <-  ReadHash(hash.name)
    if (old.content.hash != new.content.hash) {
        WriteHash(hash.name, new.content.hash)
        p <- MasterOutputPath(output.name)
        if (file.exists(p)) {
            file.remove(p)
        }
        return(FALSE)
    } else {
        return(ReadMasterOutput(output.name))
    }  
}

# hashes used to track changes are stored in a specific output directory  
HashPath <- function (name) {
    hash.path <- file.path(g.output.parent.dir, 'hash', paste0(name, '.txt'))
    return(hash.path)
}
ReadHash <- function (name) {
    hash.path <- HashPath(name)
    if (file.exists(hash.path)) {
        return(readChar(hash.path, file.info(hash.path)$size))
    } else {
        return("")
    }
}
WriteHash <- function (name, val) {
    hash.path <- HashPath(name)
    writeChar(val, hash.path)
}
HashFileContents <- function (filepaths) {
    hash <- digest(paste(sapply(filepaths, function (path) {
        readChar(path, file.info(path)$size)
    })));
    return(hash)
}

CachePath <- function (cache.id) {
    return(file.path(g.output.parent.dir, 'cache', cache.id))
}

ReadCache <- function (cache.id) {
    path <- CachePath(cache.id)
    if (file.exists(path)) {  
            load(path)
            return(x)  # this is the name of the variable used when saving
    } 
    return(FALSE) 
}

WriteCache <- function (x, cache.id) {
    # TODO: set cache limit and cleanup
    path <- CachePath(cache.id)
    f <- save(x, file = path)
}



GetUserChoice <- function (choices, choosing.what = "one of the following") {
    #todo recursive validation like http://www.rexamples.com/4/Reading%20user%20input
    
    cat(paste0("choose ", choosing.what, ":\n"))
    cat(paste0(1:length(choices), ") ", choices, collapse = '\n'))
    
    msg <- paste0("enter int 1 to ",length(choices),": ")

    choice <- GetValidatedUserChoice(msg, length(choices))
    
    return(choice)
}

GetValidatedUserChoice <- function (msg, num.choices, num.attempts = 0) { 
    max <- 8
    choice <- readline(msg)
    if (grepl("^[0-9]+$",choice)) {
        choice <- as.integer(choice)
    }
    if (num.attempts > max) {
        stop("you kept entering an invalid choice, idiot")
    } else if (class(choice) != 'integer' || choice > num.choices || choice <= 0) {
        if (num.attempts == 0) {
            msg <- paste("Invalid choice.", msg)
        }
        GetValidatedUserChoice(msg,num.choices, num.attempts + 1)
    } else {
        return(choice)
    }
}


CheckPaths()