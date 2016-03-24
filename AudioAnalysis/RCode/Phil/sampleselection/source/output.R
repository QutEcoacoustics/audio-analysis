g.output.parent.dir <- "/Users/n8933464/Documents/sample_selection_output"
g.output.master.dir <- file.path(g.output.parent.dir, 'master')
g.hash.dir <- file.path(g.output.parent.dir, 'hash')
g.output.meta.dir <- file.path(g.output.parent.dir, 'meta')


#g.cachepath <- c(
#    '/Volumes/files/qut_data/cache',
#    '/Users/n8933464/Documents/sample_selection_output/cache'
#    )


# TODO: allow file of meta data to be saved along with a dataframe, to be read and returned
# e.g. explanation of column names
# file should be arbitrary, maybe json?
# probably best to save as separate file rather than in the meta file


require('rjson')

# the way the output works is that each output file will have parameters and dependent output
# eg clustering will have feature weights as parameters and a particular features file as dependency
# when reading output for a later stage, the user will need to select the version of the output they want
# they will do this by selecting the parameters of the output and the parameters of the dependencies


g.access.log <- list()





ReadOutput <- function (name = NULL, 
                        purpose = NA, 
                        include.meta = TRUE, 
                        params = NULL, 
                        dependencies = NULL, 
                        false.if.missing = FALSE, 
                        optional = FALSE, 
                        use.last.accessed = TRUE, 
                        version = NULL) {
    # reads the output for type 'name' 
    #
    # Args:
    #   name: string; Optional. the output to read, eg "clusters", "features" etc.  If ommited, will first ask the user which type of output they want
    #   purpose: string; just to display to the user
    #   include.meta: bool; if true, will wrap the data to return in a list that also contains the metadata
    #   params: list; Optional. If supplied, will only consider returing output with the matching params
    #   dependencies: list; optional. If supplied, will only consider returning output with the matching 
    #   false.if.missing: bool; if true, will return false if the file is missing
    #   optional: boolean; if true, an option is added to select none and return false
    #   use.last.accessed: boolean; if true (default) will look for the version that was last written or read in this session
    #   version: int; If the required version is known, then it can be supplied. This version will be used if it exists
    #
    # Value:
    #   if include.meta, will return a list that has a 'data' key, that contains the data read in
    #   if include.meta is false, will return the data read in, eg data frame if it's a csv
    #
    # Details:
    #    1) if name is not supplied, will ask user, 
    #    2) then if version is supplied, find the that version of the output type. If it doesn't exist, will return false or stop
    #    3) looks for the last accessed version if it matches the version OR  params, dependencies
    #    4) if no matching last accessed verion is found, it will ask the user to select a version
    #       This means that for a particular set of params and dependencies, only one version of a particular output type can be used within the same run
    #    5) if the file is found, then it will set the 'last accessed' flag 
    #       on the chosen output and it dependencies
    #    6) Output data-type is different depending on the name of the output, for example, binary object for clustering
    #       and and CSV for features. This is defined in the GetType function
    
    if (is.null(name)) {
        # if name is ommited from function call, get user input
        # this should only happen when calling directly from the commandline
        choices = GetOutputTypes()
        choice = GetUserChoice(choices, choosing.what = "choose a type of output")
        name = choices[choice]
    }
    
    if (!is.na(purpose)) {
        Report(1, 'Reading output for:', purpose)     
    }
    
    if (use.last.accessed) {
        meta.row <- GetLastAccessed(name, params, dependencies, version) 
    } else {
        meta.row <- FALSE
    }
    
    if (!is.data.frame(meta.row) || optional) {
        meta.row <- ChooseOutputVersion(name, params = params, dependencies = dependencies, false.if.missing = false.if.missing, optional = optional, version = version)
        if (!is.data.frame(meta.row)) {
            return(FALSE)
        }
    }
    
    SetLastAccessed.recursive(meta.row$name, meta.row$version)
    val <- ReadOutputFile(meta.row$name, meta.row$version)
    if (include.meta) {
        meta <- ExpandMeta(meta.row)
        meta$indirect.dependencies <- GetIndirectDependenciesStack(meta.row$name, meta.row$version)
        meta$data <- val
        return(meta)
    } else {
        return(val)
    }
}





ClearAccessLog <- function () {
    g.access.log <<- list()  
}
SetLastAccessed <- function (name, meta.row) {
    g.access.log[[name]] <<- list(meta = meta.row, date.accessed = DateTime())  
}
GetLastAccessed <- function (names, params = NULL, dependencies = NULL, version = NULL) {
    # checks the last accessed log for the output type of 'name' and 
    # returns it if the params, dependencies and version match, 
    # otherwise returns false
    #
    # Args 
    #    name: string vector
    #    params: list
    #    dependencies: list
    #    version: int
    #
    
    last.accessed.date <- FALSE
    last.accessed.meta <- FALSE
    
    # for in list of names, check if the name is in the access log
    # if it is, check if the supplied params, dependencies and version all match
    # if not supplied, treat as matched
    # if they all match, compare to see if the access date of that is the newest out of all 
    # of the names provided. If so, record it
    
    for (name in names) {
        if (!is.null(g.access.log[[name]])) {     
            
            params.match <- is.null(params) || g.access.log[[name]]$meta$params == toJSON(params)
            dependencies.match <- is.null(dependencies) || g.access.log[[name]]$meta$dependencies != toJSON(dependencies)
            version.match <- is.null(version) || g.access.log[[name]]$meta$version != version
            
            if (params.match && dependencies.match && version.match) {
                if (last.accessed.date == FALSE || g.access.log[[name]]$date.accessed > last.accessed.date) {
                    last.accessed.date <- g.access.log[[name]]$date.accessed
                    last.accessed.meta <- g.access.log[[name]]$meta
                    last.accessed.meta$name <- name
                }   
            }
            
        } 
    }
    
    return(last.accessed.meta)
    
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


GetIndirectDependenciesDFStack <- function (name, version, meta = NA) {
    # given a name and a version, will return a dataframe listing of all of the dependencies and their versions
    # if an output type is a dependency of more than one dependency, then it will only appear once. 
    # if this occurs, it should have the same version. If there are different versions of the same dependency 
    # in the dependency tree, this is wrong, and it will cause an error. 
    #
    # Args:
    #     name: string;
    #     version: int;
    #     meta: data.frame; optional. The dataframe of all outputs. If ommited will read from disk
    # 
    # Value: 
    #     data.frame; with columns name, version
    
    
    
  
    if (!is.data.frame(meta)) {
        meta <- ReadMeta()
    }    
    meta.row <- meta[meta$name == name & meta$version == version, ]
    if (nrow(meta.row) != 1) {
        return(FALSE)
    }
    d <- DependenciesToDf(meta.row$dependencies)
   

    if (nrow(d) > 0) {
        for (i in 1:nrow(d)) {  
            cur.d.name <- as.character(d$name[i])
            cur.d.version <- as.character(d$version[i])
            cur.d.dependencies <- GetIndirectDependenciesDFStack(cur.d.name, cur.d.version, meta)
            d <- rbind(d, cur.d.dependencies)
        }
    }
    return(d)
}

GetIndirectDependenciesStack <- function (name, version, meta = NA) {
    # given a name and a version, will return a list of all of the dependencies and their versions
    # if an output type is a dependency of more than one dependency, then it will only appear once. 
    # if this occurs, it should have the same version. If there are different versions of the same dependency 
    # in the dependency tree, this is wrong, and it will cause an error. 
    #
    # Args:
    #     name: string;
    #     version: int;
    #     meta: data.frame; optional. The dataframe of all outputs. If ommited will read from disk
    # 
    # Value: 
    #     list; Each dependency type is the name of and the version is the value
    
 
    if (!is.data.frame(meta)) {
        meta <- ReadMeta()
    }    
    meta.row <- meta[meta$name == name & meta$version == version, ]
    if (nrow(meta.row) != 1) {
        return(FALSE)
    }
    d <- DependenciesToDf(meta.row$dependencies)
    d.list <- list()
    
    if (nrow(d) > 0) {
        for (i in 1:nrow(d)) {  
            cur.d.name <- as.character(d$name[i])
            cur.d.version <- as.integer(d$version[i])
            d.list <- AddToDependencyStack(d.list, cur.d.name, cur.d.version) 
            cur.d.dependencies <- GetIndirectDependenciesStack(cur.d.name, cur.d.version, meta)
            for(j in names(cur.d.dependencies)) {
                d.list <- AddToDependencyStack(d.list, j, cur.d.dependencies[[j]])   
            }
        }
    }
    return(d.list)
}


AddToDependencyStack <- function (list, name, value) {
    # adds a value to a list only if it doesn't exist
    # if it does exist with the same value, it is ignored, 
    # if it does exist with a different value, it causes an error
    
    if (name %in% names(list) && list[[name]] != value) {
        stop('multiple versions of same output type in dependency tree')
    } else {
        list[[name]] <- value
        return(list)
    }
}




GetIndirectDependenciesTree <- function (name, version, meta = NA) {
    # given a name and version, will return a tree of all the dependencies and their dependencies etc
    # 
    # Args:
    #     name: string;
    #     version: int;
    #     meta: data.frame; optional. The dataframe of all outputs. If ommited will read from disk
    #
    # Value:
    #     list; in the form 
    #list(dependency.1 = list(version = 12, 
    #                         dependencies = list(dependency.1.1.name = list(version = 3, 
    #                                                                        dependencies = list())
    #                                             dependency.1.2.name = list(version = 2, 
    #                                                                        dependencies = list())))
    #     dependency.2 = list(version = 12, 
    #                         dependencies = list(dependency.2.1.name = list(version = 3, 
    #                                                                        dependencies = list())
    #                                             dependency.2.2.name = list(version = 2, 
    #                                                                        dependencies = list()))))
    
    
    
    require('rjson')   
    if (!is.data.frame(meta)) {
        meta <- ReadMeta()
    }    
    meta.row <- meta[meta$name == name & meta$version == version, ]
    if (nrow(meta.row) != 1) {
        return(FALSE)
    }
    d <- fromJSON(meta.row$dependencies)
    d.names <- names(d)
    if (length(d) > 0) {
        for (i in 1:length(d)) {  
            cur.d.name <- d.names[i]
            cur.d.version <- d[[d.names[i]]]
            cur.d.dependencies <- GetIndirectDependencies.recursive(cur.d.name, cur.d.version, meta)
            d[[cur.d.name]] <- list(version = cur.d.version, dependencies <- cur.d.dependencies)
        }
    }
    return(d)
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
    # some types of output are data.frames and some are not. 
    # Those which can be represented as a data frame are saved as a csv\
    # those which can't must be saved as a binary object
    # this function returns which forms for the given type of output
    #
    # clustering is the object returned by clustering method
    # ranked samples is a 3d array with ranking-method, num-clusters and min-num as the dimensions
    # species.in.each.min and optimal.samples are lists
    
    
    if (name %in% c('clustering', 'clustering.HA','clustering.kmeans', 'ranked.samples', 'species.in.each.min', 'optimal.samples','silence.model')) {
        return('object')
    } else {
        return('csv')
    }
}





GetOutputTypes <- function () {
    # reads the meta file and returns all the types of output that exist
    meta <- ReadMeta()
    output.types <- meta$name[meta$file.exists == 1]
    output.types <- unique(output.types)
    return(output.types)
    
}




WriteStructuredOutput <- function (x, check.before.overwrite = TRUE) {
    # writes output where the meta values for the output are included in the list
    # rather than passed as separate parameters
    # Simply takes the parameters from the list and uses them for WriteOutput
    
    required.params <- c('data', 'name', 'params', 'dependencies')
    missing.items <- required.params[!required.params %in% names(x)]
    if (length(missing.items) > 0) {
        stop(paste('supplied output list is missing some items:', paste(missing.items, sep = ", ")))
    }
    
    v.num = WriteOutput(x = x$data, name = x$name, params = x$params, dependencies = x$dependencies, check.before.overwrite = check.before.overwrite)
    
    
    return(v.num)
    
    
    
}


WriteOutput <- function (x, name, params = list(), dependencies = list(), check.before.overwrite = TRUE) {
    # writes output to a file
    # Args
    #   x: data.frame (normally); The data to write
    #   name: string; what to call the output. This is used in: the filename and the meta file to keep track of versions and dependencies
    #   params: list;  The parameters used when generating this output. Saved in the meta file.
    #   dependencies: list; The output from previous steps that was used as input data for the process that created this output. List of name/version pairs
    #
    # Value
    #   the version that it gets saved as
    #
    # Details
    #   output will be saved with the filename like: name.version.csv  The version is detected automatically. 
    #   If output for this name with the same parameters and dependencies is already saved, then it will be overwritten (after user confirmation).
    #   If params or dependencies are different, then a new version is saved. The version number is created automatically. 
    #
    #   The version for the dependency can be ommited (by putting a value < 1). If this happens, then it will be assumed to be the last accessed version
    #   of the dependency output. So, the process which generated the output will have accesed some other output it depends on. Then, when it saves its output
    #   it need only pass the name of the input data without the version, and the WriteOutput function will know which version it was.

    # read the meta for all outputs
    meta <- ReadMeta()
    params <- toJSON(params)
    
    # if a dependency version is not supplied, then it tries 
    # to use the version that was last accessed 
    dependency.names <- names(dependencies)
    missing <- c()
    for (d.name in dependency.names) {
        if (!dependencies[[d.name]] > 0) {
            last.accessed <- GetLastAccessed(d.name)
            if (class(last.accessed) == 'data.frame') {
                dependencies[[d.name]] <- last.accessed$version
            }
        }
    }
    if (any(dependencies < 1)) {
        # if any dependency versions are less than 1 (i.e. 0), stop and give the error message
        # This will happen if the dependency verion was not supplied AND the last accesed verion is not stored
        stop(paste('dependency versions must be supplied for ', paste(names(dependencies[dependencies < 1]), collapse = ",")))
    }
    
    dependencies <- toJSON(dependencies)
    
    matching.name <- meta$name == name
    matching.p.and.d<- matching.name & meta$params == params & meta$dependencies == dependencies
    
    # search for a previous version with the same params and same dependencies
    # if found, confirm overwrite and update meta with new date
    # if not found, create a new meta row

    if (any(matching.p.and.d)) {
        # todo: check if this file is the dependency of other files. If so, maybe not safe to overwrite?
        if (check.before.overwrite) {
            msg <- paste0("Overwrite output for version ", meta$version[matching.p.and.d], " of ", name," (", meta$date[matching.p.and.d], ") ?")
            overwrite <- Confirm(msg) 
        } else {
            overwrite <- TRUE
        }

        if (!overwrite) {
            return(FALSE)
        } else {
            meta$date[matching.p.and.d] <- DateTime()
            new.v.num <- meta$version[matching.p.and.d]
            meta.row <- meta[matching.p.and.d, ]
        }
    } else {       
        if (any(matching.name)) {
            new.v.num <- max(meta$version[matching.name]) + 1  
        } else {
            new.v.num <- 1
        }
        
        meta.row <- MakeMetaRow(name, new.v.num, params, dependencies) 
        meta <- rbind(meta, meta.row)
        
    }
    
    SetLastAccessed(name, meta.row)
    
    WriteMeta(meta)
    WriteOutputFile(x, name, new.v.num)
    
    return(new.v.num)
    
    
    
}

ReadOutputFile <- function (name, version) {
  # given a name and version, figures out if it should be saved as csv or R object
  # then calls the relevant function
  type <- GetType(name)
  path <- OutputPath(name, version, unzip = TRUE)
  if (type == 'object') {   
    val <- ReadObject(path)
  } else {
    val <- ReadCsv(path)
  }
  return(val)
}
WriteOutputFile <- function (x, name, v.num) {
  # given some data, the name (type of output) and version
  # figures out whether to save as csv or object
  # then calls the relevant function
    type <- GetType(name)
    path <- OutputPath(name, v.num)
    if (type == 'object') {    
        WriteObject(x, path)
    } else {
        WriteCsv(x, path)
    }
    if (file.exists(path) && file.exists(ZipPath(path))) {
      # confirm that the save has worked (path exists) then
      # delete any zipped version
      file.remove(ZipPath(path))
    }
    
}


ReadObject <- function (path) {
  if (file.exists(path)) {  
    load(path)
    return(x) # this is the name of the variable used when saving
  } 
  return(FALSE) 
}
WriteObject <- function (x, path) {
  f <- save(x, file = path)
}


# read/write wrappers for csv with correct options set
ReadCsv <- function (path) {
    return(read.csv(path, header = TRUE, stringsAsFactors=FALSE))
}
WriteCsv <- function (x, path) {
  write.csv(x, path, row.names = FALSE)
}


OutputPath <- function (name, version, ext = NA, unzip.file = FALSE) {
  # returns the path to the file for the name and version
  # 
  # Args:
  #   name: string
  #   version: int
  #   ext: string; the file extension. If ommitted, will check the correc type for the name of the output
  #   unzip.file: boolean; if true, and the file has been zipped, will first unzip the file
  
    if (!is.character(ext)) {
        ext <- GetType(name)    
    }
    fn <- paste(as.character(name), sprintf("%03d", as.integer(version)), ext, sep = '.')
    
    path <- file.path(g.output.master.dir, fn)
    if (!file.exists(path) && unzip.file) {
      zipped.path <- ZipPath(path)
      success <- unzip(zipped.path, junkpaths = TRUE, exdir = g.output.master.dir)
      if (success < 2 & file.exists(path)) {
        file.remove(zipped.path)
      }
    }
    
    return(path)
}
ZipPath <- function (path) {
  return(paste0(path, '.zip'))
}

ReadMeta <- function () {

    path <- file.path(g.output.meta.dir, 'meta.csv')
    if (file.exists(path)) {
        meta <- read.csv(path, stringsAsFactors=FALSE)  
    } else {   
        return(EmptyMeta())
    }
    meta <- VerifyMeta(meta)
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

GetMeta <- function (name, version) {
    meta <- ReadMeta()
    meta <- meta[meta$name == name & meta$version == version,]
    meta <- as.list(meta)
    meta$params <- fromJSON(meta$params)
    meta$dependencies <- fromJSON(meta$dependencies)
    return(meta)
}

DateTime <- function () {
    return(format(Sys.time(), "%Y-%m-%d %H:%M:%S"))
}

WriteMeta <- function (meta) {
    path <- file.path(g.output.meta.dir, 'meta.csv')
    meta <- meta[order(meta$date, decreasing = TRUE), ]
    write.csv(meta, path, row.names = FALSE)  
}




ChooseOutputVersion <- function (names, params, dependencies, false.if.missing = FALSE, optional = FALSE, version = NULL) {
    # gets user input to choose a version of output to read
    #
    # Args:
    #   name: string; Name of the output to read
    #   params: list; Optional. Parameters that the output must have to appear in the list
    #   dependencies: list; Optional. Dependencies that the output must have to appear in the list
    #   false.if.missing: Boolean. If FALSE will stop if there is no output. 
    #                              If TRUE, will return false if there is no output
    #   optional: boolean; if TRUE, adds a user choice to return false
    #
    # Value:
    #   
    # Details:
    #   When presenting the choices, it will find the parameters of the output choice as well as each of their dependencies parameters. 
    #   So that the user can choose based on the unique set of input parameters that have generated the output. 
    
    meta <- ReadMeta()
    
    name.meta <- meta[meta$name %in% names & meta$file.exists == 1, ]
    if (nrow(name.meta) == 0) {
        if (false.if.missing) {
            return(FALSE)
        }
        stop(paste("Missing output file:", name))
    }
    filter <- rep(TRUE, nrow(name.meta))
    if (is.list(params)) {
        params <- toJSON(params)
        filter[name.meta$params != params] <- FALSE
    }
    if (is.list(dependencies)) {
        dependencies <- toJSON(dependencies)
        filter[name.meta$dependencies != dependencies] <- FALSE
    }
    if (!is.null(version)) {
        filter[name.meta$version != version] <- FALSE
    }
    
    name.meta <- name.meta[filter, ] 
    
    if (nrow(name.meta) == 0) {
        if (false.if.missing) {
            return(FALSE)
        }
        stop(paste("Missing output file with specified params:", name, params))
    } 
    choices <- apply(name.meta, 1, function (x) {
        params <- GetParams.recursive(x['name'], x['version'], meta)
        return(MultiParamsToString(params))
    })
    if (nrow(name.meta) == 1) {
        # we only have one thing to choose from, so choose it for them
        # but show them which one is being chosen
        Report(4, 'only one file to choose from, returing it:')
        ReportAnimated(5, choices[1], duration = 3)
        which.version <- 1
    } else {
        which.version <- GetUserChoice(choices, optional = optional)   
    }
    

    if (which.version == 0 && optional) {
        return(FALSE)
    } else {
        return(name.meta[which.version, ])  
    }

}



GetParams.recursive <- function (name, v.num, meta = NA) {
    # get metadata for all versions of the output name (eg 'features')
    if (!is.data.frame(meta)) {
        meta <- ReadMeta()    
    }

    # find the metadata for this version of the output
    row <- meta[meta$name == name & meta$version == as.numeric(v.num), ]
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
    #  given a list of lists of params, versions and dates of multiple files
    #  converts it all to a readable string
    #  inner list is in the form
    #  list(name = 'clustering', date = '2014-10-10', params = "{ ... JSON ... }" )
    #  outer list names are the names of outout, and values are the inner list
    data <- sapply(list, function (x) {
        return(paste0('v', as.character(x$version), " ", as.character(x$date), " ", as.character(x$params)))
    })
    str <- paste(names(list), data, sep = ":", collapse = "\n     ")
    return(str)
    
}

VerifyMeta <- function (meta = NULL) {
    # updates the "file.exists" column of the meta csv
    # i.e. checkes if the file exists
    # files may get deleted, however this should not necessarily 
    # mean the meta row should be deleted, since it can still be 
    # used to show information about dependencies.
    if (is.null(meta)) {
        meta <- ReadMeta()    
    }
    files.exist <- apply(meta, 1, function (row) {
        path <- OutputPath(row['name'], row['version'])
        return(file.exists(path) || file.exists(paste0(path, '.zip')))
    })
    meta$file.exists <- as.integer(files.exist)
    WriteMeta(meta)
    return(meta)
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
    meta.list$name <- meta.df$name
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
    cache.dir <- Path('cache')
    return(file.path(cache.dir, cache.id))
}

ReadCache <- function (cache.id) {
    path <- CachePath(cache.id)
    if (file.exists(path)) {
        load(path)
#         result <- tryCatch({
#             load(path)
#         }, warning = function (w) {
#             print('warning: corrupt cache file')
#             print(w)
#         }, error = function (e) {
#             print('error: corrupt cache file')
#             print(e)
#         } , finally =  {
#            
#         })
        if (exists('x')) {
            Report(6, 'successfully read file from cache')
            return(x)  # this is the name of the variable used when saving   
        }
    } 
    return(FALSE) 
}

WriteCache <- function (x, cache.id) {
    # TODO: set cache limit and cleanup
    path <- CachePath(cache.id)
    f <- save(x, file = path)
}


ZipOldFiles <- function () {
  #
  
  meta <- ReadMeta()
  
  for (r in 1:nrow(meta)) {
    
    meta.time <- as.POSIXlt(meta$date[r], tz = Sys.timezone())
    now <- Sys.time()
    diff <- difftime(now, meta.time, tz = Sys.timezone(), units = 'days')
    
    if (diff > 30) {
      
      file.path <- OutputPath(meta$name[r], meta$version[r])
      file.path.zip <- paste0(file.path, '.zip')
      
      if (file.exists(file.path) && !file.exists(file.path.zip)) {
        
        # zip the file
        # flags here http://linux.die.net/man/1/zip
        # recursive, compression level 9 (max/slowest), exclude file info, junk paths
        success <- zip(file.path.zip, file.path, flags = '-r9Xj')
        print(success)
        
        if (success < 2 && file.exists(file.path.zip)) {
          # error codes: http://www.info-zip.org/FAQ.html#error-codes
          # remove the non-zipped file
          Report(3, r, "of", nrow(meta), " files zipped:", file.path)
          file.remove(file.path)
        } else {
          Report(1, 'something went wrong, zip file not there')
        }
      }
    }
  }
}


D3Inspector <- function () {
    
    data <- DataVis()
    html.file <- 'inspect.data.html'
    HtmlInspector(NULL, template.file = 'output.inspector.html', output.fn =  html.file, singles = list(data = data))
    
}


DataVis <- function () {
    # Converts data to json for use with D3 and SVG
    #
    # Details:
    #   Final format will be something like this:
    #
    #   {'name':{'id':1,
    #            'format':'csv',
    #            'versions':[{"v":1,'params':{},'links':{'name':1},'cols':['a','b']}],
    #           }
    #   } 
    #   
    
    m <- ReadMeta()
    group.names <- unique(m$name)
    
    # for some stupid reason this returns a named character vector with no way to remove names
    # which then gets encoded in json wrong if you select with single square brackets
    group.format <- sapply(m$name, GetType)
    data <- list()
    
    for (g in 1:length(group.names)) {
        cur <-group.names[g]
        data[[g]] <- list(name = cur,
                          format = group.format[[g]],
                          versions = list())
        
        versions <- m[m$name == cur,]
        for (v in 1:nrow(versions)) {
            data[[g]][['versions']][[v]] <- list(v = versions$version[v],
                                                 params = fromJSON(versions$params[v]),
                                                 links = fromJSON(versions$dependencies[v]),
                                                 date = versions$date[v])
        }
    }

    data <- toJSON(data)
    
    return(data)
    
    
    
    
    
}

DataVisFlat <- function () {
    # Converts data to json for use with D3 and SVG
    #
    # Details:
    #   Final format will be something like this:
    #      
    #   {
    #      "groups":{
    #          'names':['name1','name2','name3'], // the different types of data
    #          'format':['csv','object','csv']  // the format of the data
    #          'count':[1,2,10] // how many of each group there is
    #          },
    #      "group_links":{
    #          "from":[from1, from2,from3], // this is redundant, and will be based on the version links
    #          "to":[to1,to2,to3]   
    #          },
    #      "versions":{
    #          'group':[0,1,1], // index of the group
    #          'params':[{'name':val},{'name':val},{'name':val}],
    #          'version':[1,1,1,],
    #          'cols':[['a','b','c'],['a','b','c'],['a','b','c']] // the column names of the csv (if applicable)
    #      },
    #      "version_links":{
    #          "from":[], // the index within the versions list
    #          "to":[]
    #      }
    #   }
    #
    
    m <- ReadMeta()
    group.names <- unique(m$name)
    group.format <- sapply(m$name, GetType)
    group.count <- tabulate(as.factor(m$name)) #todo: check that this is in the same order as unique
    
    version.links <- GetLinks(m, TRUE)

    
    # map to index of groups list, not meta rows
    group.map <- function (m.row) {
        return(which(group.names == m$name[m.row]))
    }
    group.links <- data.frame(from = sapply(version.links$from, group.map),
                        to = sapply(version.links$to, group.map))
    group.links <- unique(group.links)
    
    group.links <- as.list(group.links)
    version.links <- as.list(version.links)
    
    versions <- list(group = sapply(1:nrow(m), group.map))
    versions$params <- unname(sapply(m$params, fromJSON))
    versions$version <- m$version
    
    # TODO: cols
    
    groups <- list(names = group.names,
                   format = group.format,
                   count = group.count)
    
    all.data <- list(groups = groups, 
                     group_links = group.links,
                     versions = versions,
                     version_links = version.links)
    
    all.data <- toJSON(all.data)
    
    return(all.data)
    


    
    
}



MakeDataGraph <- function (include.versions = FALSE) {
    
    # converts the meta table to json directed graph
    #
    # Args: 
    #     as.json: boolean; whether it should be returned as a list(false) or json(true)
    
    m <- ReadMeta()
    
    if (!include.versions) {
        m <- m[m$version == 1,]
    }
    
    if (include.versions) {
        label <- title <- paste(m$name, m$version, sep = ":")
    } else {
        label <- title <- m$name
    }


    
    nodes <- data.frame(id = 1:nrow(m), 
                        label = label,
                        title = title,
                        shape = "square",
                        color = "green",
                        size = 15)
    
    if (include.versions) {
        nodes$group <- m$name
    }
    
    edges <- GetLinks(m, include.versions = include.versions)
    edges$arrows <- "middle"
    
    
    visNetwork(nodes, edges, width = "100%")
}

MakeDataGraph.cola <- function (format = 'df') {
    # converts the meta table to json directed graph
    #
    # Args: 
    #     as.json: boolean; whether it should be returned as a list(false) or json(true)
    
    m <- ReadMeta()
    
    if (format %in% c('list','json')) {
        nodes <- list()
        for (r in 1:nrow(m)) {
            nodes[[r]] <- list(
                'name'= paste(m[r,'name'],':',m[r,'version'])
            )
        }   
    } else if (format =='df') {
        
        nodes <- data.frame(id = 1:nrow(m), 
                            title = paste(m$name, m$version, sep = ":"),
                            shape = "square",
                            color = "green",
                            size = 5)
        
        
    }
    

    
    edges <- GetLinks(m)

    
    #graph.json <- toJSON(graph)
    # save json
    #fileConn<-file(file.path(g.output.meta.dir, "data_graph.json"))
    #writeLines(graph.json, fileConn)
    #close(fileConn)

    visNetwork(nodes, edges) %>% visOptions(highlightNearest = TRUE, nodesIdSelection = TRUE)
    
    
    
}

GetLinks <- function (m, include.versions, as.list = FALSE, index.from = 1) {
    # produces a list of dependency links by row number of meta
    # for output to directed-graph visualization
    #
    # Args: 
    #   as.list: boolean; Whether the format should be as a list (e.g. for conversion to json)
    #                     or a dataframe (e.g. for use with htmlwidgets in R)
    #   index.from: int;  the source/destination pointer to the rows of the meta should start counting rows from 0 or 1?
    #
    # Details:
    #   https://cran.r-project.org/web/packages/visNetwork/vignettes/Introduction-to-visNetwork.html
    #   http://www.htmlwidgets.org/showcase_visNetwork.html
    #   http://visjs.org/docs/network/
    
    
    # max length for links will be half square of number of nodes
    # changing length of list is slow, so initialise to max then trim at the end
    max.links <- (nrow(m) * nrow(m)) / 2
    
    link.list <-  list()
    length(link.list) <- max.links
    link.df <- data.frame(from = numeric(max.links), to = numeric(max.links))


    
    # store current index of dependencies separately for efficiency
    cur.d <- 0 
    
    for (r in 1:nrow(m)) {
        dependencies <- DependenciesToDf(m[r,'dependencies'])
        for (local.d in seq_len(nrow(dependencies))) {
            # add link
            
            cur.d <- cur.d + 1
            
            # source row is the index (starting from 0) of the node of the dependency
            # which is the same as the meta row of the dependency - 1
            if (include.versions) {
                link.source <- which(m$name == dependencies$name[local.d] & m$version == dependencies$version[local.d])
            } else {
                link.source <- which(m$name == dependencies$name[local.d])
            }

            link.source <- link.source + index.from - 1
            if (length(link.source) > 0) {
                if (length(link.source) > 1) {
                    msg <- paste('corrupted meta: ', 
                                 dependencies$name[local.d], 
                                 'version', dependencies$version[local.d], 
                                 'exists', length(link.source), 'times, on rows', paste(link.source,sep=","))
                    stop(msg)
                }
            } else {
                stop('missing dependency')
            }
            
            
            link.target <- r + index.from - 1
            link.list[[cur.d]] <- list(source = link.source, target = link.target)
            link.df[cur.d,] <- c(link.source, link.target)
        }
    }
    
    # trim the list 
    length(link.list) <- cur.d
    # trim the df
    link.df <- link.df[1:cur.d,]
 
    
    if (as.list) {
        return(link.list)
    } else {
        return(link.df)
    }
    
    
    
}



    
