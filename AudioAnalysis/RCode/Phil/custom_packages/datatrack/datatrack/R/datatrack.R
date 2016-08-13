
# TODO: add more examples and also add them to documentation

# TODO: test and document dviewer

# TODO: write technical report

# the way it works is that each dataobject file will have parameters and dependent dataobjects
# eg clustering will have feature weights as parameters and a particular features file as dependency
# when reading dataobject for a later stage, the user will need to select the version of the dataobject they want
# they will do this by selecting the parameters of the dataobject and the parameters of the dependencies


#' given a name and a version, will return a dataframe listing of all of the dependencies and their versions
#'
#' @param name string;
#' @param version int;
#' @param meta: data.frame; optional. The entire dataframe of metadata. If ommitted will be read from disk.
#'
#' @return list with dependency name as the name of and the version as the value
#'
#' @details
#' a dataobject of a given name will only appear once in the stack, even if it appears in multiple places in the dependency tree
#' (i.e. if two dataobjects in the stack both have a direct dependency of the same name)
#' if this occurs, it should have the same version. If there are different versions of the same dependency
#' in the dependency tree, something is wrong.
.GetIndirectDependenciesStack <- function (name, version, meta = NA) {

    if (!is.data.frame(meta)) {
        meta <- ReadMeta()
    }
    meta.row <- meta[meta$name == name & meta$version == version, ]
    if (nrow(meta.row) != 1) {
        return(FALSE)
    }
    d <- .DependenciesToDf(meta.row$dependencies)
    d.list <- list()

    if (nrow(d) > 0) {
        for (i in 1:nrow(d)) {
            cur.d.name <- as.character(d$name[i])
            cur.d.version <- as.integer(d$version[i])
            d.list <- .AddToDependencyStack(d.list, cur.d.name, cur.d.version)
            cur.d.dependencies <- .GetIndirectDependenciesStack(cur.d.name, cur.d.version, meta)
            for(j in names(cur.d.dependencies)) {
                d.list <- .AddToDependencyStack(d.list, j, cur.d.dependencies[[j]])
            }
        }
    }
    return(d.list)
}

#' Adds a dependency to the list of indirect dependencies, ensuring that the same name is not added twice
#'
#' @param list list the dependency stack
#' @param name string the name to add to the list
#' @param value the value to add to the list
#'
#' @details
#' When generating a list of indirect dependencies, by traversing up the tree, some basic checking is done to ensure that
#' dependencies are logincal. If data object A depends on data objects B and C, and data objects B and C both depend on D,
#' that is fine if B and C both depend on the same version of D, but not okay if they depend on different versions of D.
.AddToDependencyStack <- function (list, name, value) {
    # adds a value to a list only if it doesn't exist
    # if it does exist with the same value, it is ignored,
    # if it does exist with a different value, it causes an error

    if (name %in% names(list) && list[[name]] != value) {
        warning('multiple versions of same dataobject name in dependency tree, only displaying one')
    } else {
        list[[name]] <- value
        return(list)
    }
}


#' reads the meta file and returns all the names of data files that exist
.GetDataobjectNames <- function () {
    meta <- ReadMeta()
    return(unique(meta$name[meta$file.exists == 1]))
}

#' Determines whether the file was saved as a csv or an R object, then calls the
#' appropriate function to read it and return it
#'
#' @param name string
#' @param version int
.ReadDataobjectFile <- function (meta.row) {
  # given a name and version, figures out if it was as csv or R object
  # then calls the relevant function
  path <- .DataobjectPath(meta.row$name, meta.row$version, csv = meta.row$csv, unzip = TRUE)
  if (meta.row$csv) {
     val <- .ReadCsv(path)
  } else {
     val <- .ReadObject(path)
  }
  return(val)
}

#' Determines whether the file should be saved as a csv or an R object,
#' then calls the appropriate function to write it
#'
#' @param x the value to save
#' @param name string
#' @param version int
.WriteDataobjectFile <- function (x, name, v.num) {

    as.csv <- .UseCsv(x)
    path <- .DataobjectPath(name, v.num, csv = as.csv)
    if (!as.csv) {
        .WriteObject(x, path)
    } else {
        .WriteCsv(x, path)
    }
    if (file.exists(path) && file.exists(.ZipPath(path))) {
      # confirm that the save has worked (path exists) then
      # delete any zipped version
      file.remove(.ZipPath(path))
    }
}

#' checks the datatype of a data object and returns
#' whether it can be used as a csv
#' @param x the dataobject to check
#'
#' TODO: maybe add table or even matrix to this
#'       Adding these types to the condition would work for writing, but it would get read as a data.frame
#'       Best approach would be to change the csv column to 'class', then record the class for all data object
#'       using csv when appropriate and converting back to the correct format
.UseCsv <- function (x) {
    if (is.data.frame(x)) return(1)
    return(0)
}

#' reads an R object from disk
#'
#' @param path string
.ReadObject <- function (path) {
  x <- NULL  # required to initialise to pass tests
  if (file.exists(path)) {
    load(path)
    return(x) # this is the name of the variable used when saving
  }
  return(FALSE)
}

#' writes an R object to disk
#'
#' @param x the value to save
#' @param path string
.WriteObject <- function (x, path) {
  f <- save(x, file = path)
}

#' reads a csv from disk
#'
#' @param path string
#' @details
#' Just a wrapper for the native R read.csv function with the right values set
.ReadCsv <- function (path) {
    return(read.csv(path, header = TRUE, stringsAsFactors=FALSE))
}

#' writes a csv to disk
#'
#' @param x the value to save
#' @param path string
#' @details
#' Just a wrapper for the native R read.csv function with the right values set
.WriteCsv <- function (x, path) {
  write.csv(x, path, row.names = FALSE)
}


#' returns the path to the file for the name and version
#'
#' @param name character vector
#' @param version int vector
#' @param csv int or boolean; whether the dataobject is saved as a csv or an object
#' @param unzip boolean; if true, and the file has been zipped, will first unzip the file
#' @return string
 .DataobjectPath <- function (name, version, csv, unzip = FALSE) {

    if (!.AllSame(c(length(name), length(version), length(csv)))) {
        stop('arguments are not of same length')
    }

    ext <- rep('object', length(csv))
    ext[csv] <- 'csv'

    fn <- paste(as.character(name), sprintf("%03d", as.integer(version)), ext, sep = '.')
    path <- file.path(pkg.env$data.dir, fn)

    # if unzip is true, filter only the paths that are missing but which have a zipped file
    # then unzip those files, and delete
    if (unzip) {
        .RestoreFromZip(path)
    }

    return(path)
}



#' gets user input to choose a version of dataobject to read
#'
#' TODO: test and document for multiple names
#'
#' @param names: string; Name or names of the dataobject to read
#' @param params: list; Optional. Parameters that the dataobject must have to appear in the list
#' @param dependencies: list; Optional. Dependencies that the dataobject must have to appear in the list
#' @param false.if.missing: Boolean. If FALSE will stop if there is no dataobject.
#'                              If TRUE, will return false if there is no dataobject
#' @param optional: boolean; if TRUE, adds a user choice to return false
#'
#' @return data.frame one row of the metadata data frame
#'
#' @details
#' When presenting the choices, it will find the parameters of the dataobject choice as well as each of their dependencies parameters.
#' So that the user can choose based on the unique set of input parameters that have generated the dataobject.
.ChooseDataobjectVersion <- function (names, params, dependencies, false.if.missing = FALSE, optional = FALSE, version = NULL) {

    meta <- ReadMeta()

    name.meta <- meta[meta$name %in% names & meta$file.exists == 1, ]
    if (nrow(name.meta) == 0) {
        if (false.if.missing) {
            return(FALSE)
        }
        stop(paste("Missing dataobject file:", paste(names, collapse = ",")))
    }
    filter <- rep(TRUE, nrow(name.meta))
    if (is.list(params)) {
        params <- rjson::toJSON(params)
        filter[name.meta$params != params] <- FALSE
    }
    if (is.list(dependencies)) {
        dependencies <- rjson::toJSON(dependencies)
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
        stop(paste("Missing dataobject file with specified params:", paste(names, collapse = ","), params))
    }
    choices <- apply(name.meta, 1, function (x) {
        params <- .GetParams.recursive(x['name'], x['version'], meta)
        return(.MultiParamsToString(params))
    })
    if (nrow(name.meta) == 1) {
        # we only have one thing to choose from, so choose it for them
        # but show them which one is being chosen
        .Report('only one file to choose from; returing it')


        .Report(choices[1], level = 1)
        which.version <- 1
    } else {
        Inspector(names[1])

        which.version <- userinput::GetUserChoice(choices, optional = optional)
    }

    if (which.version == 0 && optional) {
        return(FALSE)
    } else {
        return(name.meta[which.version, ])
    }

}

#' Returns a list of parameters used by a given data object (name/version)
#' as well as all its direct and indirect dependencies
#' @param name string
#' @param v.num int
#' @param meta data.frame optional if ommited will read from disk.
#' @return list of parameters (name value pairs)
.GetParams.recursive <- function (name, v.num, meta = NA) {
    # get metadata for all versions of the dataobject name (eg 'features')
    if (!is.data.frame(meta)) {
        meta <- ReadMeta()
    }

    # find the metadata for this version of the dataobject
    row <- meta[meta$name == name & meta$version == as.numeric(v.num), ]
    # get its params from the param col, as a readable string
    params.line <- list()
    params.line[[name]] <- list(version = v.num, date = row$date, params = row$params)
    # get the names and versions of its dependencies
    dependencies <- .DependenciesToDf(as.character(row$dependencies))
    # for each dependency, get the params, and params of its dependencies,
    # then append them to the params list ()
    if (nrow(dependencies) > 0) {
        for (i in 1:nrow(dependencies)) {
            dependency.name <- as.character(dependencies$name[i])
            dependency.version <- as.integer(dependencies$version[i])
            dependency.params <- .GetParams.recursive(dependency.name, dependency.version, meta)
            params.line <- c(params.line, dependency.params)
        }
    }
    return(params.line)
}

#'  given a list of lists of params, versions and dates of multiple files
#'
#'  @param list list
#'  @details
#'  converts it all to a readable string
#'  inner list is in the form
#'  list(name = 'clustering', date = '2014-10-10', params = "{ ... JSON ... }" )
#'  outer list names are the names of outout, and values are the inner list
.MultiParamsToString <- function (list) {

    data <- sapply(list, function (x) {
        return(paste0('v', as.character(x$version), " ", as.character(x$date), " ", as.character(x$params)))
    })
    str <- paste(names(list), data, sep = ":", collapse = "\n     ")
    return(str)

}

#' converts a json string of dependencies to a data frame
#'
#' @param str the json string of dependencies
#' @return data.frame with the columns name, version
.DependenciesToDf <- function (str) {
    d <- rjson::fromJSON(str)
    return(data.frame(name = names(d), version = as.integer(d)))
}


#' Lists relevant session info
#'
#' Extracts the relevant parts of sessionInfo to save for provenance information
#' @return character json of list of session info
#' @details
#' Retains the "R.version"  "platform"   "locale" "running"    "basePkgs" members of sessionInfo
#' But of otherPkgs and loadedOnly extracts just the name and the version of the packages
#' @export
.SessionInfo <- function () {

    si <- sessionInfo()
    mysi <- list()

    for (name in names(si)) {
        mysi[[name]] <- si[[name]]
    }

    packageVersions <- function (packages) {
        getPackageVersion <- function (pkg) {
            return(pkg$Version)
        }
        versions <- sapply(packages, getPackageVersion)
        return(paste(names(versions), versions))
    }

    # replace each otherPkgs value with just the version
    # and strip out all but the R version string
    mysi$R.version <- si$R.version$version.string
    mysi$otherPkgs <- packageVersions(mysi$otherPkgs)
    mysi$loadedOnly <- packageVersions(mysi$loadedOnly)

    # remove locale, since it's not that important and is quite a long string
    mysi$locale <- NULL

    return(mysi)

}

