g.output.parent.dir <- "/Users/n8933464/Documents/sample_selection_output"
g.output.master.dir <- file.path(g.output.parent.dir, 'master')
g.hash.dir <- file.path(g.output.parent.dir, 'hash')
g.output.meta.dir <- file.path(g.output.parent.dir, 'meta')
require('rjson')

# the way the output works is that each output file will have parameters and dependent output
# eg clustering will have feature weights as parameters and a particular features file as dependency
# when reading output for a later stage, the user will need to select the version of the output they want
# they will do this by selecting the parameters of the output and the parameters of the dependencies


g.access.log <- list()

ClearAccessLog <- function () {
    g.access.log <<- list()  
}
SetLastAccessed <- function (name, meta.row) {
    g.access.log[[name]] <<- meta.row  
}
GetLastAccessed <- function (name) {
    if (!is.null(g.access.log[[name]])) {
        return(g.access.log[[name]])
    } else {
        return(FALSE)
    }
}

SetLastAccessed.recursive <- function (name, version, meta = NA) {
    # given a row of meta data. Looks at the name and version of each dependency
    # and sets the last accessed for those. 
    
    if (!is.data.frame(meta)) {
        meta <- ReadMeta()
    }
    
    meta.row <- meta[meta$name == name & meta$version == version, ]
    if (nrow(meta.row) != 1) {
        return(FALSE)
    }
    
    SetLastAccessed(meta.row$name, meta.row)
    
    dependencies <- DependenciesToDf(meta.row$dependencies)
    if (nrow(dependencies) > 0) {
        for (i in 1:nrow(dependencies)) {
            SetLastAccessed.recursive(dependencies$name[i], dependencies$version[i])
        }
    }
    
    
}


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
    if (name %in% c('clustering', 'ranked.samples')) {
        return('object')
    } else {
        return('csv')
    }
}


ReadOutput <- function (name, purpose = NA, include.meta = TRUE, params = NA, false.if.missing = FALSE) {
    
    meta.row <- GetLastAccessed(name)
    if (!is.data.frame(meta.row)) {
        meta.row <- ChooseOutputVersion(name, params, false.if.missing = false.if.missing)
        if (!is.data.frame(meta.row)) {
            return(FALSE)
        }
    }
    
    SetLastAccessed.recursive(meta.row$name, meta.row$version)
    

    type <- GetType(name)
    if (type == 'object') {   
        val <- (ReadObject(name, meta.row$version))
    } else {
        val <- (ReadCsv(name, meta.row$version))
    }
    if (include.meta) {
        meta <- ExpandMeta(meta.row)
        meta$data <- val
        return(meta)
    } else {
        return(val)
    }
}


WriteOutput <- function (x, name, params, dependencies = list()) {

    meta <- ReadMeta()
    params <- toJSON(params)
    dependencies <- toJSON(dependencies)
    
    matching.name <- meta$name == name
    matching.p.and.d<- matching.name & meta$params == params & meta$dependencies == dependencies
    
    # search for a previous version with the same params and same dependencies
    # if found, confirm overwrite and update meta with new date
    # if not found, create a new meta row

    if (any(matching.p.and.d)) {
        # todo: check if this file is the dependency of other files. If so, maybe not safe to overwrite?
        msg <- paste("Overwrite output for version ", meta$version[matching.p.and.d], " (", meta$date[matching.p.and.d], ")?")
        overwrite <- Confirm(msg)
        if (!overwrite) {
            return(FALSE)
        } else {
            meta$date[matching.p.and.d] <- DateTime()
            new.v.num <- meta$version[matching.p.and.d]
        }
    } else {       
        if (any(matching.name)) {
            new.v.num <- max(meta$version[matching.name]) + 1  
        } else {
            new.v.num <- 1
        }
        
        new.meta.row <- MakeMetaRow(name, new.v.num, params, dependencies) 
        meta <- rbind(meta, new.meta.row)
        
    }    
    WriteMeta(meta)
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


OutputPath <- function (name, version, ext = NA) {
    if (!is.character(ext)) {
        ext <- GetType(name)    
    }
    fn <- paste(as.character(name), sprintf("%03d", as.integer(version)), ext, sep = '.')
    return(file.path(g.output.master.dir, fn))
}


ReadMeta <- function () {
    path <- file.path(g.output.meta.dir, 'meta.csv')
    if (file.exists(path)) {
        meta <- read.csv(path, stringsAsFactors=FALSE)  
    } else {   
        return(EmptyMeta())
    }
    return(meta)
}

EmptyMeta <- function () {
    return(data.frame(name = character(), version = integer(), params = character(), dependencies = character(), date = character()))
}

MakeMetaRow <- function (name, v.num, params = list(), dependencies = list(), date = NA) {
    if (is.list(params)) {
        params <- toJSON(params)
    }
    if (is.list(dependencies)) {
        dependencies <- toJSON(dependencies)
    }
    if (is.na(date)) {
        date <- DateTime()
    }
    row <- data.frame(name = name, version = v.num, params = params, dependencies = dependencies, date = date, file.exists = NA)
    return(row)
}

DateTime <- function () {
    return(format(Sys.time(), "%Y-%m-%d %H:%M:%S"))
}

WriteMeta <- function (meta) {
    path <- file.path(g.output.meta.dir, 'meta.csv')
    meta <- meta[order(meta$date), ]
    write.csv(meta, path, row.names = FALSE)  
}




ChooseOutputVersion <- function (name, params, false.if.missing = FALSE) {
    VerifyMeta()
    meta <- ReadMeta()
    name.meta <- meta[meta$name == name & meta$file.exists == 1, ]
    if (nrow(name.meta) == 0) {
        if (false.if.missing) {
            return(FALSE)
        }
        stop(paste("Missing output file:", name))
    }
    if (is.list(params)) {
        params <- toJSON(params)
    }
    if (is.character(params)) {
        name.meta <- name.meta[name.meta$params == params, ] 
    }
    if (nrow(name.meta) == 0) {
        if (false.if.missing) {
            return(FALSE)
        }
        stop(paste("Missing output file with specified params:", name, params))
    }
    choices <- sapply(name.meta$version, function (v.num) {
        params <- GetParams.recursive(name, v.num, meta)
        return(MultiParamsToString(params))
    })
    which.version <- GetUserChoice(choices)
    return(name.meta[which.version, ])  
}



GetParams.recursive <- function (name, v.num, meta = NA) {
    # get metadata for all versions of the output name (eg 'features')
    if (!is.data.frame(meta)) {
        meta <- ReadMeta()    
    }

    # find the metadata for this version of the output
    row <- meta[meta$name == name & meta$version == v.num, ]
    # get its params from the param col, as a readable string
    params.line <- list()
    params.line[[name]] <- list(version = v.num, date = row$date, params = row$params)
    # get the names and versions of its dependencies
    dependencies <- DependenciesToDf(as.character(row$dependencies))
    # for each dependency, get the params, and params of its dependencies,
    # then append them to the params list ()
    if (nrow(dependencies) > 0) {
        for (i in 1:nrow(dependencies)) {
            dependency.name <- as.character(dependencies$name[i])
            dependency.version <- as.integer(dependencies$version[i])
            dependency.params <- GetParams.recursive(dependency.name, dependency.version, meta)
            params.line <- c(params.line, dependency.params) 
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
    meta <- ReadMeta()
    files.exist <- apply(meta, 1, function (row) {
        path <- OutputPath(row['name'], row['version'])
        return(file.exists(path))
    })
    meta$file.exists <- as.integer(files.exist)
    WriteMeta(meta)
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


  
    
