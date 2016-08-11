# this files contains functions that are only provided to provide legacy support to
# scripts or datatrack data that was created with previous versions

# These are wrappers for WriteDataobject etc, and are only for legacy purposes
# They must be updated if the new functions ever change parameter list

#' Legacy Wrapper for WriteDataobject
#'
#' @param x mixed
#' @param name character
#' @param params list
#' @param dependencies list
#' @param check.before.overwrite boolean
#' @return int the version of the name that was written
WriteOutput <- function (x, name, params = list(), dependencies = list(), check.before.overwrite = TRUE) {
    return(WriteDataobject(x, name, params, dependencies, check.before.overwrite))
}

#' Legacy Wrapper for ReadDataobject
#'
#' @param name character
#' @param purpose character
#' @param include.meta boolean
#' @param params list
#' @param dependencies list
#' @param false.if.missing boolean
#' @param optional boolean
#' @param use.last.accessed boolean
#' @param version int
#' @return list
ReadOutput <- function (name = NULL,
                        purpose = NA,
                        include.meta = TRUE,
                        params = NULL,
                        dependencies = NULL,
                        false.if.missing = FALSE,
                        optional = FALSE,
                        use.last.accessed = TRUE,
                        version = NULL) {
    return(ReadDataobject(name = name, purpose = purpose,  include.meta = include.meta,
                          params = params,  dependencies = dependencies, false.if.missing = false.if.missing,
                          optional = optional, use.last.accessed = use.last.accessed, version = version))
}

#' Legacy Wrapper for WriteStructuredDataobject
#'
#' @param x list
#' @param check.before.overwrite boolean
#' @return int the version of the name that was written
WriteStructuredOutput <- function (x, check.before.overwrite = TRUE) {
    return(WriteStructuredDataobject(x, check.before.overwrite))
}


#' Updates meta csv column
#'
#' @details
#' In previous versions of datatrack, whether to save as csv was specified in the config file
#' There was no 'csv' column in the metadata. Although this saved a column, it required more configuration
#' This function adds the 'csv' column and populates the rows according to the json config
AddCsvColToMeta <- function () {
    meta <- ReadMeta()
    if ('as.object' %in% names(pkg.env$config)) {
        if (!('csv' %in% colnames(meta))) {
            meta$csv <- as.numeric(!(meta$name %in% pkg.env$config$as.object))
            .WriteMeta(meta)
        }
    }


}



