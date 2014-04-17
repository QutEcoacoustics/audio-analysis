# all functions to do with outputting and reporting

g.output.master.dir <- file.path(g.output.parent.dir, 'master')
g.hash.dir <- file.path(g.output.parent.dir, 'hash')

g.state <- list()
g.state$cur.output.paths <- rep(NA, 3)

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

WriteOutput <- function (x, fn, ext = 'csv', level = 1) {
    # writes output files in a consistent way, 
    # with reporting
    Report(4, 'Writing output',level,':', fn)
    path <- OutputFile(GetOutputPath(level, read = FALSE), fn, ext)

    WriteOutputCsv(x, path)
}

ReadOutput <- function (fn, ext = 'csv', false.if.missing = FALSE, level = 1) { 
    # reads output files in a consistent way, 
    # with reporting
    #
    # Args:
    #   fn: string
    #   ext: string
    #   false.on.fail: boolean. If true, will check if exists and 
    #     return false if it is missing. If true, will cause an error
    #     if missing
    Report(4, 'Reading output',level,':', fn)
    path <- OutputFile(GetOutputPath(level, read = TRUE), fn, ext)

    if (false.if.missing && !file.exists(path)) {
        return(FALSE)
    }
    data <- ReadOutputCsv(path)
    return(data)
}

SaveObject <- function (x, fn, level = 1) {
    Report(4, 'Saving object level',level,':', fn)
    path <- OutputFile(GetOutputPath(level, read = FALSE), fn, ext = 'object')
    f <- save(x, file = path)
    
}
ReadObject <- function (fn, level = 1) {
    Report(4, 'Reading object level',level,':', fn)
    path <- OutputFile(GetOutputPath(level, read = TRUE), fn, ext = 'object')
    if (file.exists(path)) {  
        load(path)
        return(x) # this is the name of the variable used when saving
    } 
    return(FALSE) 
}

# read/write wrappers for csv with correct options set
ReadOutputCsv <- function (path) {
    return(read.csv(path, header = TRUE, stringsAsFactors=FALSE))
}
WriteOutputCsv <- function (x, path) {
    write.csv(x, path, row.names = FALSE)
}



OutputExists <- function (fn, ext = 'csv', level = 1) {
    path <- OutputFilePath(fn, ext = ext, read = TRUE, level = level)
    if (is.na(path)) {
        return(FALSE)
    } else {
        return(file.exists(path))   
    }
}

OutputFilePath <- function (fn, ext, level = 1, read = TRUE) {
    
    dir <- GetOutputPath(level, read = read)
    if (is.na(dir)) {
        return(NA)
    } else {
        return(OutputFile(dir, fn, ext)) 
    }
}

SetOutputPath <- function (level = 1, allow.new = TRUE) {
    path <- OutputPath(level = level, allow.new = allow.new)
    g.state$cur.output.paths[level] <<- path
}


GetOutputPath <- function (level = 1, read = TRUE, save = TRUE) {
    # gets the output path for the level, either using the one already saved
    # or getting them to choose a new one
    path <- NA
    if (level > 0 && !is.na(g.state$cur.output.paths[level])) {
        if (file.exists(g.state$cur.output.paths[level])) {
            path <- g.state$cur.output.paths[level]
        } else {
            g.state$cur.output.paths[level] <<- NA
        }
    }
    
    if (is.na(path)) {
        if (read) {
            allow.new = FALSE
        } else {
            allow.new = TRUE
        } 
        path <- OutputPath(level = level, allow.new = allow.new)   
        if (save && level > 0) {
            g.state$cur.output.paths[level] <<- path
        } 
    }
    return(path)
    
}

# output is heirachincal, like a tree, because for each step there are some choices and output can be different
# depending on the choice. Output on later steps depends on output of earlier steps. 
# So, output can be of different nested levels
OutputPath <- function (level = 1, allow.new = FALSE) {
    path <- OutputPathL0()  
    if (level > 0) {
        for (i in 1:level) {   
            dirs <- list.dirs(path, full.names = TRUE, recursive = FALSE)  # full.names is broken (bug) ATM
            options <- dirs
            if (allow.new && level == i) {
                new.dir <- file.path(path, as.character(length(dirs) + 1))
                dirs <- c(dirs, new.dir)
                options <- c(options, paste(new.dir, "(new)"))
            }   
            if (length(dirs) == 0) {
                #stop(paste("Maybe you are trying to access level", i, "output which doesn't exist"))
                return(NA)
            } else if (length(dirs) > 1) {
                choice <- GetUserChoice(options, paste('level',i,'output directory'), default = length(dirs))  
            } else {
                choice <- 1
            }
            path <- dirs[choice]
            if (!file.exists(path)) {
                dir.create(path) 
            }          
        }
    } 
    return(path)
}

OutputFile <- function (path, fn, ext = 'csv') {
    op <- file.path(path, paste(fn,ext, sep='.'))
}



OutputPathL0 <- function () {
    # Returns the correct level zero path for output for the config settings
    #
    #
    # Returns: 
    #   String; Something like:
    #     "output/2010-10-13.80.2010-10-13.120.NW.SWBackup/
    #  
    # Details:
    #   Used both by functions writing the output and reading previous output
    #   First creates a directory based on the start and end time and dates 
    #   and dates to be processed (so changing any of these will cause the  
    #   system to read and write to a different directory). 
    
    # first create the output directory 
    sites <- paste(g.sites, collapse = ".")
    dir.name <- paste(g.all.events.version, g.start.date, g.start.min, g.end.date,
                      g.end.min, sites, g.percent.of.target, sep='.')
    dir.name <- gsub(" ","", dir.name)
    output.dir <- file.path(g.output.parent.dir,dir.name)

    if (!file.exists(output.dir)) {
        dir.create(output.dir)
    }
   
    return(output.dir)
}


WriteMasterOutput <- function (x, fn, ext = 'csv') {
    WriteOutputCsv(x, MasterOutputPath(fn, ext))
}

ReadMasterOutput <- function (fn, ext = 'csv', false.if.missing = FALSE) {
    p <- MasterOutputPath(fn, ext);
    if (file.exists(p)) {
        return(ReadOutputCsv(p))
    } else if (false.if.missing) {
        return(FALSE)
    } else {
        stop(paste("file doesn't exist: ", p))
    }
}

MasterOutputPath <- function (fn, ext = 'csv') {
    return(file.path(g.output.master.dir, g.all.events.version, paste(fn,ext, sep='.')))
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



Report <- function (level, ..., nl.before = FALSE, nl.after = TRUE) {
    # prints output to the screen if the level is above the 
    # global output level. 
    #
    # Args:
    #   level: int; how important is this? 1 = most important
    #   ... : strings;  concatenated to form the message
    #   nl: boolean; whether to start at a new line
    if (level <= g.report.level) {
        if (nl.before) {
            cat("\n")
        }
        cat(paste(c(paste(as.vector(list(...)),  collapse = " ")), collapse = ""))
        if (nl.after) {
            cat("\n")
        }
    }
}

Dot <- function(level = 5) {
    #outputs a dot, used for feedback during long loops
    if (level <= g.report.level) {
        cat(".")
    }
}

Timer <- function(prev = NULL, what = 'processing', num = NULL, per = "each") {
    # used for reporting on the execution time of parts of the code
    if(is.null(prev)) {
        return(proc.time())
    } else {
        t <- (proc.time() - prev)[3]
        Report(3, 'finished', what, 'in', round(t, 2), ' sec')
        if (is.numeric(num) && num > 0) {
            time.per.each <- round(t / num, 3)
            Report(3, time.per.each, "per", per)
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
        should.delete <- Confirm(paste("the following files have changed",
                                 not.if.changed,
                                 "Replace existing master output",
                                 output.name,
                                 "?"))
        if (should.delete) {
            p <- MasterOutputPath(output.name)
            if (file.exists(p)) {
                file.remove(p)
            }
            return(FALSE)
        }

    } 
    return(ReadMasterOutput(output.name, false.if.missing = TRUE))
      
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




CheckPaths()