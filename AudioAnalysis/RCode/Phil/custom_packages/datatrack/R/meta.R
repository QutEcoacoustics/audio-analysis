#Contains functions to do with manipulating the metadata file


#' given a single row of meta data as a data.frame
#' converts it to a list and then converts the values that are json to lists
#' @param meta.df data.frame
#' @return list
.ExpandMeta <- function (meta.df) {
    meta.list <- list()
    meta.list$version <- meta.df$version
    meta.list$params <- rjson::fromJSON(as.character(meta.df$params))
    meta.list$dependencies <- rjson::fromJSON(as.character(meta.df$dependencies))
    meta.list$date <- meta.df$date
    meta.list$name <- meta.df$name
    return(meta.list)
}


#' updates the "file.exists" column of the meta csv
#'
#' @param meta data.frame optional if ommitted will read from disk
#' @details
#' i.e. checkes if the file exists
#' files may get deleted, however this should not necessarily
#' mean the meta row should be deleted, since it can still be
#' used to show information about dependencies.
.VerifyMeta <- function (meta = NULL) {

    if (is.null(meta)) {
        meta <- .ReadMeta()
    }

    file.paths <- .DataobjectPath(meta$name, meta$version, meta$csv)
    files.exist <- file.exists(file.paths) || file.exists(.ZipPath(file.paths))
    meta$file.exists <- as.integer(files.exist)

    if (!"col.names" %in% colnames(meta)) {
        meta$col.names = '';
    }
    if (!"callstack" %in% colnames(meta)) {
        meta$callstack = '';
    }

    .WriteMeta(meta)
    return(meta)
}

#' saves the metadata csv
#' @param meta data.frame
.WriteMeta <- function (meta) {
    path <- file.path(pkg.env$meta.dir, 'meta.csv')
    meta <- meta[order(meta$date, decreasing = TRUE), ]
    write.csv(meta, path, row.names = FALSE)
}

#' Returns the metadata row for the given name/version pair
#'
#' @param name string
#' @param version int
#' @return list
#'
#' @details
#' Reads the metadata, filters to the correct row, converts to list,
#' then converts json encoded values to lists
.GetMeta <- function (name, version) {
    meta <- .ReadMeta()
    meta <- meta[meta$name == name & meta$version == version,]
    meta <- as.list(meta)
    meta$params <- rjson::fromJSON(meta$params)
    meta$dependencies <- rjson::fromJSON(meta$dependencies)
    return(meta)
}

#' Reads the csv of metadata from disk
#' @details
#' If the file doesn't exist, creates an empty one with the correct columns
.ReadMeta <- function () {
    path <- file.path(pkg.env$meta.dir, 'meta.csv')
    if (file.exists(path)) {
        meta <- read.csv(path, stringsAsFactors=FALSE)
    } else {
        return(.CreateEmptyMeta())
    }
    meta <- .VerifyMeta(meta)
    return(meta)
}

#'TODO: every time the meta is written, make a copy of the old meta (1 per day)
.ArchiveMeta <- function () {

}

#' creates an empty dataframe with the correct columns
#' @return data.frame
.CreateEmptyMeta <- function () {

    # create a dummy meta row to get all the right columns
    # then remove the row to get an empty data frame
    dummy.meta <- .MakeMetaRow("", 0)
    return(dummy.meta[c(),])
}

#' Makes a 1-row data frame for the metdata of a saved dataobject
#' @param name string
#' @param v.num int
#' @param params list,
#' @param dependencies list
#' @param date string
#' @param col.names character vector
#' @param callstack character vector
#' @return data.frame
.MakeMetaRow <- function (name,
                          v.num,
                          params = list(),
                          dependencies = list(),
                          date = NA,
                          col.names = NULL,
                          callstack = NULL,
                          csv = 0) {
    if (is.list(params)) {
        params <- rjson::toJSON(params)
    }
    if (is.list(dependencies)) {
        dependencies <- rjson::toJSON(dependencies)
    }
    if (is.na(date)) {
        date <- .DateTime()
    }
    if (is.character(col.names)) {
        col.names = rjson::toJSON(col.names)
    } else {
        col.names = ''
    }
    if (is.character(callstack)) {
        callstack = rjson::toJSON(callstack)
    } else {
        callstack = ""
    }
    row <- data.frame(name = name, version = v.num, params = params, dependencies = dependencies, date = date, file.exists = NA, col.names = col.names, callstack = callstack, csv = csv)
    return(row)
}

