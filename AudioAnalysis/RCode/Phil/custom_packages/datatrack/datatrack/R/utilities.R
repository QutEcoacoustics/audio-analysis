#' returns true if all numbers in the vector are the same
.AllSame <- function (x) {
    return(diff(range(x)) < .Machine$double.eps ^ 0.5)
}


#' returns the current date time in the correct format to save in the metadata
#' @return string
.DateTime <- function () {
    return(format(Sys.time(), "%Y-%m-%d %H:%M:%S"))
}

#' Merges two lists
#' @param x list the list to add values to
#' @param y list the list to add values from
#' @return list
#' @details
#' adds names to x that are in y by not in x. If the name already exists
.MergeLists <- function (x, y) {
    not.set <- ! names(y) %in% names(x)
    x[names(y)[not.set]] <- y[not.set]
    return(x)
}

#' returns the indicies of a vector for iterating over the vector by index
#'
#' Useful for iterating over a vector by index taking into account that the
#' vector might be empty
#'
#' @param vector vector anything that has a length
#' @return numeric vector
.Indices <- function (vector) {
    if (length(vector) == 0) {
        return(numeric())
    } else {
        return(1:length(vector))
    }
}

#' Format value as string
#'
#' Formats the value as a string in different ways depending on the type, so that it can be saved concisely in the meta data csv
#' @param mixed val
#' @details
#' if list length > 0: toJSON
#' if numeric or character with length > 1 : toJSON
#' if numeric or character with length == 1: as is, or as character, depending on the value of preserve.numeric
#' if NULL or NA or anything with length zero: empty string ""
.toCsvValue <- function (val) {
    if (length(val) == 0 || is.na(val)) return("")
    if (is.list(val) || length(val) > 1) return(rjson::toJSON(val))
    if ((is.numeric(val) || is.character(val)) && length(val) == 1) return(val)
    # any other datatype, make it json
    return(rjson::toJSON(val))
}

#' Generate a path to a new directory by checking for 
#' existence of the given directory and appending a number
#' @param path character
#' @param new boolean, whether to get the highest existing number or the first non-existing number
#' @param create boolean if new = TRUE, should the new folder be created or just the name of the non-existing folder returned
#' @export
.GetNumberedDir <- function (path, new = TRUE, create = FALSE) {
    
    parent <- dirname(path)
    base <- basename(path)
    
    files <- list.files(path = parent, pattern = paste0(base, "[0-9]*$"), all.files = FALSE,
               full.names = FALSE, recursive = FALSE,
               ignore.case = TRUE, include.dirs = TRUE)
    
    if (length(files) == 0) {
        fn <- file.path(parent, paste0(base, '01'))
        if (create) {
            dir.create(fn)  
        }
    } else {
        last.file <- tail(files, n=1)
        if (!new) {
            fn <- file.path(parent, last.file)
        } else {
            cur.max = as.integer(substring(last.file, nchar(base) + 1))
            fn <- file.path(parent, paste0(base, sprintf("%02d", cur.max + 1)))
            if (create) {
                dir.create(fn)  
            }
        }
    }
    return(fn)  
}



