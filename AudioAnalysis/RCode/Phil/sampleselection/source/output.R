# Takes care of things like
# - temp output directories for visualisations
# - caching 


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
    
    
    parent.temp.dir <- TempDirPath()
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
    parent.temp.dir <- TempDirPath()
    # seems dangerous
    # unlink(parent.temp.dir, recursive = TRUE)
    Report("please delete the files in ", TempDirPath())
}

TempDirPath <- function () {
    return(file.path(Path('output'), 'temp'))
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
# deprecated ... ?
HashPath <- function (name) {
    hash.path <- file.path(Path('output'), 'hash', paste0(name, '.txt'))
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


# general caching, e.g. for spectrograms

CachePath <- function (cache.id) {
    path <- Path('cache')
    return(file.path(path, cache.id))
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



