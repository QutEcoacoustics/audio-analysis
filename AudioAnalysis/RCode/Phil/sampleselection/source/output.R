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

WriteOutput <- function (x, fn, new = FALSE, ext = 'csv', level = 1) {
    # writes output files in a consistent way, 
    # with reporting
    Report(4, 'Writing output',level,':', fn)
    path <- OutputPath(fn, new = new, ext = ext, level = level, allow.new = TRUE)
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
    path <- OutputPath(fn, ext = ext, level = level, allow.new = FALSE)
    if (false.if.missing && !file.exists(path)) {
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

SaveObject <- function (x, fn, level = 1) {
    Report(4, 'Saving object level',level,':', fn)
    path <- OutputPath(fn, ext = 'object', level = level, allow.new = TRUE)
    f <- save(x, file = path)
    
}
ReadObject <- function (fn, level = 1) {
    Report(4, 'Reading object level',level,':', fn)
    path <- OutputPath(fn, ext = 'object', level = level)
    if (file.exists(path)) {  
        load(path)
        return(x) # this is the name of the variable used when saving
    } 
    return(FALSE) 
}

OutputExists <- function (fn, ext = 'csv') {
    path <- OutputPath(fn, ext = ext)
    return(file.exists(path))
}

# output is heirachincal, like a tree, because for each step there are some choices and output can be different
# depending on the choice. Output on later steps depends on output of earlier steps. 
# So, output can be of different nested levels


OutputPath <- function (fn = FALSE, new = FALSE, ext = 'csv', level = 1, allow.new = FALSE) {
    path <- OutputPathL1(new, ext)  
    if (level > 1) {
        for (i in 2:level) {   
            dirs <- list.dirs(path, full.names = TRUE, recursive = FALSE)  # full.names is broken (bug) ATM
            options <- dirs
            if (allow.new && level == i) {
                new.dir <- file.path(path, as.character(length(dirs) + 1))
                dirs <- c(dirs, new.dir)
                options <- c(options, paste(new.dir, "(new)"))
            }   
            if (length(dirs) == 0) {
                stop(paste("Maybe you are trying to access level", i, "output which doesn't exist"))
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
    if (fn != FALSE) {
        op <- OutputFile(path, fn, ext)
    } else {
        op <- path
    }
    return(op)
}

OutputFile <- function (path, fn, ext = 'csv') {
    op <- file.path(path, paste(fn,ext, sep='.'))
}



OutputPathL1 <- function (new = FALSE, ext = 'csv') {
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


    return(path)
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

Confirm <- function (msg, default = FALSE) {
    options <- c('Yes', 'No')
    choice <- GetUserChoice(options, msg, default)
    if (choice == 1) {
        return(TRUE)
    } else {
        return(FALSE)
    }  
}


GetUserChoice <- function (choices, choosing.what = "one of the following", default = 1, allow.range = FALSE) {
    #todo recursive validation like http://www.rexamples.com/4/Reading%20user%20input
    cat(paste0("choose ", choosing.what, ":\n"))
    cat(paste0(1:length(choices), ") ", choices, collapse = '\n'))
    if (default %in% 1:length(choices)) {
        cat(paste('\ndefault: ', default))
    } else {
        default = NA
    }
    msg <- paste0("enter int 1 to ",length(choices),": ")
    choice <- GetValidatedUserChoice(msg, length(choices), default, parse.range = allow.range)  
    return(choice)
}

GetMultiUserchoice <- function (options, choosing.what = 'one of the following', default = 1, all = FALSE) {
    # allows the user to select 1 or more of the choices, returning a vector 
    # of the choice numbers
    
    if (default == 'all') {
        all <- TRUE
    }
    
    if (all) {
        options <- c(options, 'all')
        all.choice <- length(options)
        if (default == 'all') {
            default <- all.choice
        }
    } else {
        all.choice <- -99  # can't choose all
    }

    options <- c(options, 'exit')
    exit.choice <- length(options)
    last.choice <- -1;
    chosen <- c()
    while(TRUE) {
        if (max(last.choice) > 0) {
            # if something has been chosen, change the default to exit
            default = exit.choice
        }
        last.choice <- GetUserChoice(options, choosing.what, default = default, allow.range = TRUE)
        should.exit <- exit.choice %in% last.choice
        should.use.all <- all.choice %in% last.choice
        if (should.use.all) {
            chosen <- 1:length(options)
            break()
        }
        if (should.exit) {
            break()
        } else  {
            chosen <- union(chosen, last.choice)    
        }
    }
    last.choice <- setdiff(chosen, c(exit.choice, all.choice))
    return(unique(chosen))
}

GetValidatedUserChoice <- function (msg, num.choices, default = 1, num.attempts = 0, parse.range = FALSE) { 
    max <- 8
    choice <- readline(msg)
    if (choice == '' && !is.na(default)) {
        choice <- as.integer(default)
    } else if (grepl("^[0-9]+$",choice)) {
        choice <- as.integer(choice)
    } else if (parse.range && grepl("^[0-9]+[ ]*[:-][ ]*[0-9]+$",choice)) {
        # split by hyphen and parse range
        values <- regmatches(choice, gregexpr("[0-9]+", choice))
        choice <- as.integer(values[[1]][1]):as.integer(values[[1]][2])
    }
    if (num.attempts > max) {
        stop("you kept entering an invalid choice, idiot")
    } else if (class(choice) != 'integer' || max(choice) > num.choices || min(choice) <= 0) {
        if (num.attempts == 0) {
            msg <- paste("Invalid choice.", msg)
        }
        GetValidatedUserChoice(msg, num.choices, default = default, num.attempts = num.attempts + 1, parse.range = parse.range)
    } else {
        return(choice)
    }
}


CheckPaths()