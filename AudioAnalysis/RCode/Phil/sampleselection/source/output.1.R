
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