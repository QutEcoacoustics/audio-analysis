# datatrack allows files that have not been accessed for a certain lenth of time to be zipped
# if they are read, the unzipping is done automatically.
# this is to save space, especially will csv files.


#' Zips files that have not been accessed for a certain length of time
#'
#' @details
#' for row of metadata (for each file of data) checks the last accessed time,
#' If it is older than a certain amount of time, will zip the file, delete the
.ZipOldFiles <- function () {

    meta <- ReadMeta()

    for (r in 1:nrow(meta)) {

        meta.time <- as.POSIXlt(meta$date[r], tz = Sys.timezone())
        now <- Sys.time()
        diff <- difftime(now, meta.time, tz = Sys.timezone(), units = 'days')

        if (diff > pkg.env$config$zip.after.days) {

            file.path <- .DataobjectPath(meta$name[r], meta$version[r], meta$csv[r])
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
                    .Report(r, "of", nrow(meta), " files zipped:", file.path, level =  2)
                    file.remove(file.path)
                } else {
                    .Report('something went wrong, zip file not there')
                }
            }
        }
    }
}

#' Returns the path to the zipped version of a file
#'
#' @param the path to the non-zipped version of the file
.ZipPath <- function (path) {
    return(paste0(path, '.zip'))
}


#' Given a list of paths to data objects
#' Check which don't have files but have zip files
#' Then unzips the zip file and delete the zip file
#' @param path character
#'
#' TODO: test
.RestoreFromZip <- function (paths) {

    # list of paths that don't have files
    paths.of.missing <- paths[!file.exists(paths)]
    # path to zip of paths that don't have files
    zipped.paths.of.missing <- .ZipPath(paths.of.missing)
    # whether the zip file exists
    zipped.exists <- file.exists(zipped.paths.of.missing)
    # paths to zip of paths that are missing and have an existing zip file
    zipped.paths.of.missing <- zipped.paths.of.missing[zipped.exists]
    # paths to files that are missing and have an existing zip file
    paths.of.missing.having.zip <- paths.of.missing[zipped.exists]
    for (i in .Indices(zipped.paths.of.missing)) {
        zipped.file <- zipped.paths.of.missing[i]
        unzipped.file <- paths.of.missing.having.zip[i]
        success <- unzip(zipped.file, junkpaths = TRUE, exdir = pkg.env$data.dir)
        if (success < 2 & file.exists(unzipped.file)) {
            file.remove(zipped.file)
        }
    }

}


