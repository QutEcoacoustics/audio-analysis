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
