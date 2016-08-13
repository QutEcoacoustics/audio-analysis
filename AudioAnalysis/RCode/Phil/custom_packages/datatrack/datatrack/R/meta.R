#Contains functions to do with manipulating the metadata file


#' Compute a Checksum of the Metadata
#'
#' To quickly compare if the metadata of 2 datatrack projects are the same
#' We can computer a checksum which ignores the date column.
#'
#' @details
#' After removing the date column of the metadata, performs a hash of the dataframe and returns is
#' @export
GetChecksum <- function (ignore.cols = c('date', 'callstack'), algo = 'sha1') {
    if (!requireNamespace("digest", quietly = TRUE)) {
        stop("Package \"digest\" needed to generate checksum. Please install it",
             call. = FALSE)
    }
    meta <- ReadMeta()
    meta <- meta[,-which(names(meta) %in% ignore.cols)]
    return(digest::digest(meta, algo = algo))
}


#' Reads the csv of metadata from disk
#' @details
#' If the file doesn't exist, creates an empty one with the correct columns
#' @export
ReadMeta <- function () {
    .LoadConfig()
    path <- file.path(pkg.env$meta.dir, 'meta.csv')
    if (file.exists(path)) {
        meta <- read.csv(path, stringsAsFactors=FALSE)
    } else {
        return(.CreateEmptyMeta())
    }
    meta <- FixMeta(meta)
    meta <- .VerifyMeta(meta)
    return(meta)
}

#' Fix the structure of the metadata dataframe
#'
#' Restores the structure of the metadata dataframe, which might be
#' necessary if, during a change to datatrack e.g. a new version of the package
#' extra columns are added, removed or reordered
#' @details
#' An empty correct meta dataframe is created, then each of the existing
#' columns are attempted to be matched to a correct column. If no match for an existing
#' column is found, it is discarded with a warning. If a correct column has not had a match found
#' for it, it is left blank.
#'
#' In specific cases of upgrades, transformations might need to be made to the existing columns. If this
#' happens this function should be added to appropriately. For example, if csv column is removed an a 'class' column
#' is addeed instead, then specific tranformation from existing csv = 1 rows to class = 'csv' needs to be coded.
#' @export
FixMeta <- function (meta) {

    .LoadConfig()

    correct.meta <- .CreateEmptyMeta()
    correct.names <- colnames(correct.meta)
    existing.names <- colnames(meta)
    if (length(correct.names) == length(existing.names) && all(correct.names == existing.names)) {
        return(meta)
    }

    warning('Existing meta dataframe is invalid. Attempting to fix it ...')

    .Report('Correct meta columns: ', paste(correct.names, collapse = ","), '. Existing meta columns: ',  paste(existing.names, collapse = ","))
    .Report('Missing columns: ', paste(setdiff(correct.names, existing.names), collapse = ","))
    .Report('Extra columns: ', paste(setdiff(existing.names, correct.names), collapse = ","))

    matching.names <- intersect(existing.names,correct.names)
    corrected.meta <- as.data.frame(matrix(NA, ncol = length(correct.names), nrow = nrow(meta)))
    colnames(corrected.meta) <- correct.names
    corrected.meta[,matching.names] <- meta[,matching.names]

    # if there are some columns from the existing meta that we couldn't find a place for in the
    # correct meta, save the existing meta with a different file name as a backup
    if (length(matching.names) < length(existing.names)) {
        .ArchiveMeta(meta, fn.note = "before_fixMeta")
        warning('Because there were some columns in the existing meta data.frame that can not be put in the correct meta dataframe, the exising meta data frame will be saved to a backup')
    }

    return(corrected.meta)
}


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
        meta <- ReadMeta()
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
    meta <- ReadMeta()
    meta <- meta[meta$name == name & meta$version == version,]
    meta <- as.list(meta)
    meta$params <- rjson::fromJSON(meta$params)
    meta$dependencies <- rjson::fromJSON(meta$dependencies)
    return(meta)
}


#' TODO: every time the meta is written, make a copy of the old meta (1 per day)
#'
#' @param meta data.frame
#' @param fn.note character if supplied will insert the string into the filename
#' @details
#' The archive version of the metadata will be saved in as in the archive dir within
#' the meta directory. The filename will have the form:
#' meta.bak + fn.note + datetime + csv
.ArchiveMeta <- function (meta, fn.note = '') {

    if (fn.note != '') {
        fn.note = paste0(fn.note, '.')
    }
    fn <- paste0('meta.bak.', fn.note, format(Sys.time(), "%Y-%m-%d_%H-%M-%S"), '.csv')
    archive.dir <- file.path(pkg.env$meta.dir, 'archive')
    if (!file.exists(archive.dir)) {
        dir.create(archive.dir)
    }
    path <- file.path(archive.dir, fn)
    write.csv(meta, path, row.names = FALSE)
    .Report('Meta archived to file: ', fn)
    return(fn)

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
#' @param csv int whether it is saved as a csv or an R object
#' @param annotations mixed list or character
#' @return data.frame
.MakeMetaRow <- function (name,
                          v.num,
                          params = list(),
                          dependencies = list(),
                          date = NA,
                          col.names = NULL,
                          callstack = NULL,
                          csv = 0,
                          annotations = NULL) {

    if (is.na(date)) {
        date <- .DateTime()
    }

    sysinfo <- rjson::toJSON(.SessionInfo())

    row <- data.frame(name = name,
                      version = v.num,
                      params = .toCsvValue(params),
                      dependencies = .toCsvValue(dependencies),
                      date = date, file.exists = NA,
                      col.names = .toCsvValue(col.names),
                      callstack = .toCsvValue(callstack),
                      csv = csv,
                      annotations = .toCsvValue(dependencies),
                      system = sysinfo)
    return(row)
}
