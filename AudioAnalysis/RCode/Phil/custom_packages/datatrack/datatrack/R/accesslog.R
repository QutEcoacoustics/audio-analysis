#' Clear the datatrack access log
#'
#' Clear the access log used to save the most recently accessed version of each name
ClearAccessLog <- function () {
    pkg.env$access.log <- list()
}


#' Records the last accessed version of a given name
#'
#' @details
#' This is used so that data being read can be returned without promting for user selection.
#' This allows the last accessed version to be automatically read without prompting for user selection
.SetLastAccessed <- function (name, meta.row) {
    pkg.env$access.log[[name]] <- list(meta = meta.row, date.accessed = .DateTime())
}

#' checks the last accessed log for the dataobjects with any the given names
#' returns it if the params, dependencies and version match,
#' otherwise returns false
#'
#' @param names vector of strings
#' @param params list
#' @param dependencies list
#' @param version int
.GetLastAccessed <- function (names, params = NULL, dependencies = NULL, version = NULL) {

    last.accessed.date <- FALSE
    last.accessed.meta <- FALSE

    # for each of the list of names, check if the name is in the access log
    # if it is, check if the supplied params, dependencies and version all match
    # if not supplied, treat as matched
    # if they all match, compare to see if the access date is the newest out of all
    # of the names provided. If so, record it

    for (name in names) {
        if (!is.null(pkg.env$access.log[[name]])) {

            params.match <- is.null(params) || pkg.env$access.log[[name]]$meta$params == rjson::toJSON(params)
            dependencies.match <- is.null(dependencies) || pkg.env$access.log[[name]]$meta$dependencies != rjson::toJSON(dependencies)
            version.match <- is.null(version) || pkg.env$access.log[[name]]$meta$version != version

            if (params.match && dependencies.match && version.match) {
                if (last.accessed.date == FALSE || pkg.env$access.log[[name]]$date.accessed > last.accessed.date) {
                    last.accessed.date <- pkg.env$access.log[[name]]$date.accessed
                    last.accessed.meta <- pkg.env$access.log[[name]]$meta
                    last.accessed.meta$name <- name
                }
            }
        }
    }

    return(last.accessed.meta)
}

#' given a row of meta data. Looks at the name and version of each dependency
#' and sets the last accessed for those.
#'
#' @param name string
#' @param version int
#' @param meta data.frame optional. The entire dataframe of metadata. If ommitted will be read from disk.
.SetLastAccessed.recursive <- function (name, version, meta = NA) {

    if (!is.data.frame(meta)) {
        meta <- ReadMeta()
    }

    meta.row <- meta[meta$name == name & meta$version == version, ]
    if (nrow(meta.row) != 1) {
        return(FALSE)
    }

    .SetLastAccessed(meta.row$name, meta.row)

    dependencies <- .DependenciesToDf(meta.row$dependencies)
    if (nrow(dependencies) > 0) {
        for (i in 1:nrow(dependencies)) {
            .SetLastAccessed.recursive(dependencies$name[i], dependencies$version[i])
        }
    }
}
