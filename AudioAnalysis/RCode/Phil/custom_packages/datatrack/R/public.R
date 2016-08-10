# This file contains the public functions of the package (i.e. those that can be
# called from a script using this package)
# other files contain private functions that should not be called from outside the
# package and should only be called by these public functions or by each other

# Most of these functions should call LoadConfig at the start or they won't work


#' reads the dataobject of a given name
#'
#' @param name String; Optional. The dataobject to read, eg "clusters", "features" etc.  If ommited, will prompt the user with a choice of available names
#' @param purpose string; Displayed to the user when prompting
#' @param include.meta bool; if true, will wrap the data to return in a list that also contains the metadata
#' @param params list; Optional. If supplied, will only consider returning a dataobject with the matching params
#' @param dependencies list; optional. If supplied, will only consider returning a dataobject with the matching
#' @param false.if.missing bool; if true, will return false if the file is missing
#' @param optional boolean; if true, an option is added to select none and return false
#' @param use.last.accessed boolean; if true (default) will look for the version that was last written or read in this session
#' @param version int; If the required version is known, then it can be supplied. This version will be used if it exists
#'
#' @return if include.meta, will return a list that has a 'data' key, that contains the data read in if include.meta is false, will return the data read in, eg data frame if it's a csv
#'
#' @details
#'    1) if name is not supplied, will ask user,
#'    2) then if version is supplied, find the that version of the dataobject name. If it doesn't exist, will return false or stop (depending on false.if.missing)
#'    3) looks for the last accessed version if it matches the version OR  params, dependencies
#'    4) if no matching last accessed verion is found, it will ask the user to select a version
#'       This means that for a particular set of params and dependencies, only one version of a particular dataobject name can be used within the same run
#'    5) if the file is found, then it will set the 'last accessed' flag
#'       on the chosen dataobject and it dependencies
#'
#' @export
ReadDataobject <- function (name = NULL,
                        purpose = NA,
                        include.meta = TRUE,
                        params = NULL,
                        dependencies = NULL,
                        false.if.missing = FALSE,
                        optional = FALSE,
                        use.last.accessed = TRUE,
                        version = NULL) {

    .LoadConfig()

    if (is.null(name)) {
        # if name is ommited from function call, get user input
        # this should only happen when calling directly from the commandline
        choices = .GetDataobjectNames()
        choice = GetUserChoice(choices, choosing.what = "choose a type of dataobject")
        name = choices[choice]
    }

    if (!is.na(purpose)) {
        .Report('Reading dataobject for:', purpose)
    }

    if (use.last.accessed) {
        meta.row <- .GetLastAccessed(name, params, dependencies, version)
    } else {
        meta.row <- FALSE
    }

    if (!is.data.frame(meta.row) || optional) {
        meta.row <- .ChooseDataobjectVersion(name, params = params, dependencies = dependencies, false.if.missing = false.if.missing, optional = optional, version = version)
        if (!is.data.frame(meta.row)) {
            return(FALSE)
        }
    }

    .SetLastAccessed.recursive(meta.row$name, meta.row$version)
    val <- .ReadDataobjectFile(meta.row)
    if (include.meta) {
        meta <- .ExpandMeta(meta.row)
        meta$indirect.dependencies <- .GetIndirectDependenciesStack(meta.row$name, meta.row$version)
        meta$data <- val
        return(meta)
    } else {
        return(val)
    }
}

#' writes a dataobject where the meta values for the dataobject are included in the list
#' rather than passed as separate parameters
#'
#' @param x mixed the data to write
#' @param check.before.overwrite boolean whether to prompt for user confirmation before overwriting an existing version
#' @details
#' Simply takes the parameters from the list and uses them for WriteDataobject
WriteStructuredDataobject <- function (x, check.before.overwrite = TRUE) {

    .LoadConfig()

    required.params <- c('data', 'name', 'params', 'dependencies')
    missing.items <- required.params[!required.params %in% names(x)]
    if (length(missing.items) > 0) {
        stop(paste('supplied dataobject list is missing some items:', paste(missing.items, sep = ", ")))
    }
    v.num = WriteDataobject(x = x$data, name = x$name, params = x$params, dependencies = x$dependencies, check.before.overwrite = check.before.overwrite)
    return(v.num)
}

#' writes dataobject to a file
#'
#' @param x data.frame or Object
#' @param name string what to call the dataobject. This is used in: the filename and the meta file to keep track of versions and dependencies
#' @param params list The parameters used when generating this dataobject. Saved in the meta file.
#' @param dependencies list The dataobjects created from previous steps that were used as input data for the process that created this dataobject List of name/version pairs
#' If the dependency version is given as 0, then datatrack will attempt to look up the last accessed version of that name to use. In other words, use 0 to
#' specify that datatrack should use the last accessed version as the dependency
#'
#' @return the version that it gets saved as
#'
#' @details
#' dataobject will be saved with the filename like: name.version.csv  The version is detected automatically.
#' If dataobject for this name with the same parameters and dependencies is already saved, then it will be overwritten (after user confirmation).
#' If params or dependencies are different, then a new version is saved. The version number is created automatically.
#'
#' The version for the dependency can be ommited (by putting a value < 1). If this happens, then it will be assumed to be the last accessed version
#' of the dependency dataobject. So, the process which generated the dataobject will have accesed some other dataobject it depends on. Then, when it saves its dataobject
#' it need only pass the name of the input dataobject without the version, and the function will know which version it was.
WriteDataobject <- function (x, name, params = list(), dependencies = list(), check.before.overwrite = TRUE) {

    .LoadConfig()

    # read the meta for all dataobjects
    meta <- .ReadMeta()
    params <- rjson::toJSON(params)

    # if a dependency version is not supplied, then it tries
    # to use the version that was last accessed
    dependency.names <- names(dependencies)
    missing <- c()
    for (d.name in dependency.names) {
        if (!dependencies[[d.name]] > 0) {
            last.accessed <- .GetLastAccessed(d.name)
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

    dependencies <- rjson::toJSON(dependencies)

    matching.name <- meta$name == name
    matching.p.and.d<- matching.name & meta$params == params & meta$dependencies == dependencies

    # search for a previous version with the same params and same dependencies
    # if found, confirm overwrite and update meta with new date
    # if not found, create a new meta row

    callstack <- as.character(sys.calls())
    # remove this function from call stack
    length(callstack) <- length(callstack) - 1

    if (is.data.frame(x)) {
        col.names = colnames(x)
    } else {
        col.names = NULL
    }


    new.meta.row <- .MakeMetaRow(name, 0, params, dependencies, col.names = col.names, callstack = callstack, csv = .UseCsv(x))

    if (any(matching.p.and.d)) {
        # todo: check if this file is the dependency of other files. If so, maybe not safe to overwrite?
        if (check.before.overwrite) {
            msg <- paste0("Overwrite dataobject for version ", meta$version[matching.p.and.d], " of ", name," (", meta$date[matching.p.and.d], ") ?")
            overwrite <- Confirm(msg)
        } else {
            overwrite <- TRUE
        }

        if (!overwrite) {
            return(FALSE)
        } else {
            new.meta.row$version <- new.v.num <- meta$version[matching.p.and.d]
            meta[matching.p.and.d, ] <- new.meta.row
        }
    } else {
        if (any(matching.name)) {
            new.v.num <- max(meta$version[matching.name]) + 1
        } else {
            new.v.num <- 1
        }

        meta <- rbind(meta, new.meta.row)
    }

    .SetLastAccessed(name, new.meta.row)
    .WriteMeta(meta)
    .WriteDataobjectFile(x, name, new.v.num)
    return(new.v.num)

}

#' Clear the access log used to save the most recently accessed version of each name
ClearAccessLog <- function () {
    pkg.env$access.log <- list()
}

#' Main function to generate the viewer
#'
#' @param group string; name to filter nodes by
#' @details
#' Gets the metadata in the appropriate json format then passes it to the dataGraph function of the dviewer package
D3Inspector <- function (group = FALSE) {

    .LoadConfig()
    data <- .DataVis()
    #uncommet this to save a the metadata into the dviewer package
    #SaveDemoVisData(data)
    print(dviewer::dataGraph(data, group));
}





