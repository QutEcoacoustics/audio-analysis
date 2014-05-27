g.output.parent.dir <- "/Users/n8933464/Documents/sample_selection_output"
g.output.master.dir <- file.path(g.output.parent.dir, 'master')
g.hash.dir <- file.path(g.output.parent.dir, 'hash')
g.output.meta.dir <- file.path(g.output.parent.dir, 'meta')


# the way the output works is that each output file will have parameters and dependent output
# eg clustering will have feature weights as parameters and a particular features file as dependency
# when reading output for a later stage, the user will need to select the version of the output they want
# they will do this by selecting the parameters of the output and the parameters of the dependencies


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



GetType <- function (name) { 
    if (name %in% c('clustering')) {
        return('object')
    } else {
        return('csv')
    }
}


ReadOutput <- function (name, purpose = NA, include.meta = FALSE, params = NA) {
    v <- ChooseOutputVersion(name, params)
    if (!is.data.frame(v)) {
        return(FALSE)
    }
    type <- GetType(name)
    if (type == 'object') {   
        val <- (ReadObject(name, v$version))
    } else {
        val <- (ReadCsv(name, v$version))
    }
    if (include.meta) {
        meta <- ExpandMeta(v)
        meta$val <- val
        return(meta)
    } else {
        return(val)
    }
}


WriteOutput <- function (x, name, params, dependencies = list()) {
    require('rjson')
    versions <- ReadVersions(name)
    params <- toJSON(params)
    dependencies <- toJSON(dependencies)
    
    # search for a previous version with the same params and same dependencies
    if (is.data.frame(versions)) {
        matching <- versions$params == params & versions$dependencies == dependencies       
        if (any(matching)) {
            # todo: check if this file is the dependency of other files. If so, maybe not safe to overwrite?
            msg <- paste("Overwrite output for version ", versions$version[matching], " (", versions$date[matching], ")?")
            overwrite <- Confirm(msg)
            if (!overwrite) {
                return(FALSE)
            } else {
                versions$date[matching] <- date()
                new.v.num <- versions$version[matching]
            }
        } else {       
            new.v.num <- max(versions$version) + 1
            new.version.meta <- VersionRow(new.v.num, params, dependencies) 
            versions <- rbind(versions, new.version.meta)

        }
    } else {
        new.v.num <- 1
        versions <- VersionRow(new.v.num, params, dependencies)
    }
    WriteVersions(name, versions)
    WriteOutputFile(x, name, new.v.num)
    
    
    
}


WriteOutputFile <- function (x, name, v.num) {
    type <- GetType(name)
    if (type == 'object') {    
        return(WriteObject(x, name, v.num))
    } else {
        return(WriteCsv(x, name, v.num))
    }
    
}

ReadObject <- function (name, version) {
    path <- OutputPath(name, version, 'object')
    if (file.exists(path)) {  
        load(path)
        return(x) # this is the name of the variable used when saving
    } 
    return(FALSE) 
}
WriteObject <- function (x, name, version) {
    path <- OutputPath(name, version, 'object')
    f <- save(x, file = path) 
}
# read/write wrappers for csv with correct options set
ReadCsv <- function (name, v.num) {
    path <- OutputPath(name, v.num, 'csv')
    return(read.csv(path, header = TRUE, stringsAsFactors=FALSE))
}
WriteCsv <- function (x, name, v.num) {
    path <- OutputPath(name, v.num, 'csv')
    write.csv(x, path, row.names = FALSE)
}


OutputPath <- function (name, version, ext) {  
    fn <- paste(name, sprintf("%03d", version), ext, sep = '.')
    return(file.path(g.output.master.dir, fn))
}


ReadVersions <- function (name) {
    path <- file.path(g.output.meta.dir, paste0(name, '.csv'))
    if (file.exists(path)) {
        versions <- read.csv(path)  
    } else {   
        return(FALSE)
    }
    return(versions)
}

WriteVersions <- function (name, x) {
    path <- file.path(g.output.meta.dir, paste0(name, '.csv'))
    write.csv(x, path, row.names = FALSE)  
}

VersionRow <- function (v.num, params, dependencies, date = NA) {
    if (is.list(params)) {
        params <- toJSON(params)
    }
    if (is.list(dependencies)) {
        dependencies <- toJSON(dependencies)
    }
    if (is.na(date)) {
        date <- date()
    }
    row <- data.frame(version = v.num, params = params, dependencies = dependencies, date = date)
    return(row)
}


ChooseOutputVersion <- function (name, params) {
    versions <- ReadVersions(name)
    if (!is.data.frame(versions)) {
        return(FALSE)
    }
    if (is.list(params)) {
        params <- toJSON(params)
    }
    if (is.character(params)) {
        versions <- versions[versions$params == params] 
    }
    if (nrow(versions) == 0) {
        return(false)
    }
    choices <- sapply(versions$version, function (v.num) {
        params <- GetParams.recursive(name, v.num)
        return(MultiParamsToString(params))
    })
    which.version <- GetUserChoice(choices)
    return(versions[which.version, ])  
}

GetParams.recursive <- function (name, v.num) {
    # get metadata for all versions of the output name (eg 'features')
    v <- ReadVersions(name)
    # find the metadata for this version of the output
    v <- v[v$version == v.num, ]
    # get its params from the param col, as a readable string
    params.line <- list()
    params.line[[name]] <- list(version = v.num, date = v$date, params = v$params)
    # get the names and versions of its dependencies
    dependencies <- DependenciesToDf(as.character(v$dependencies))
    # for each dependency, get the params, and params of its dependencies,
    # then append them to the params list ()
    if (nrow(dependencies) > 0) {
        for (i in nrow(dependencies)) {
            d.params <- GetParams.recursive(as.character(dependencies$name[i]), as.integer(dependencies$version[i]))
            params.line[[as.character(dependencies$name[i])]] <- d.params[[as.character(dependencies$name[i])]]
        }  
    }
    return(params.line)  
}

MultiParamsToString <- function (list) {
    #  given a list of params, versions and dates of multiple files
    #  converts it all to a readable string
    #  list is in the form
    #  list(name = 'clustering', date = '2014-10-10', params = "{ ... JSON ... }" )
    data <- sapply(list, function (x) {
        return(paste0('v', as.character(x$version), " ", as.character(x$date), " ", as.character(x$params)))
    })  
    params <- paste(names(list), data, sep = ":", collapse = " , ")
    return(params)
}

VerifyMeta <- function () {
    
    meta.files <- list.files(g.output.meta.dir)
    if (length(meta.files) > 1) {
        for (i in 1:length(meta.files)) {
            name <- RemoveFileExtension(meta.files[i])
            meta <- ReadVersions(name)
            ext <- GetType(name)
            file.exists <- sapply(meta$version, 2, function (v.num) {
                output.file <- OutputPath(name, v.num, ext)
                return(file.exists(output.file))       
            })
            # delete rows where the output is missing
        }
    }
    
    
    
}

DependenciesToDf <- function (str) {
    require('rjson')
    d <- fromJSON(str)
    return(data.frame(name = names(d), version = as.integer(d)))
}

ExpandMeta <- function (meta.df) {
    require('rjson')
    meta.list <- list()
    meta.list$version <- meta.df$version
    meta.list$params <- fromJSON(as.character(meta.df$params))
    meta.list$dependencies <- fromJSON(as.character(meta.df$dependencies))
    meta.list$date <- meta.df$date
    return(meta.list) 
}


#############
#
#  Temp Directory Stuff
#
#############


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


#############
#
#  Caching Stuff
#
#############



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
    require('digest')
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


  
    
