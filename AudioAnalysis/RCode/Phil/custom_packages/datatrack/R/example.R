# this file is for testing and learning to use datatrack.
# It allows the user to generate and save example data in a datatrack project


# for this example, we will generate 6 data objects with a few versions each




#' Creates a few example data objects with some dependencies between them
Example1 <- function () {


    # save a version of two data objects called "one" and "two" respectively

    # returns the version number
    data1.v1 <- .SaveExampleData("one")
    data2.v1 <- .SaveExampleData("two")

    # save a version of "three" that depends on both of these previous
    data3.v1.dependencies <- list('one' = data1.v1, 'two' = data2.v1)
    data3.v1 <- .SaveExampleData("three", data3.v1.dependencies)

    # save another version of "three" with different parameters but the same dependencies
    data3.v2 <- .SaveExampleData("three", data3.v1.dependencies)



    # save a version of "four" that depends on "three" version 2 and "two" version 1
    data4.v1.dependencies <- list('three' = data3.v2, 'two' = data2.v1)
    data4.v1 <- .SaveExampleData("four", data4.v1.dependencies)

    # save another version of "two" with different parameters which is the only
    # dependency of a new data object type called "five"
    data2.v2 <- .SaveExampleData("two")
    data5.v1.dependencies <- list('two' = data2.v2)
    .SaveExampleData("five", data5.v1.dependencies)

    ClearAccessLog()

}

#' Example showing attempting some incompatible dependencies
#' @details
#' This should be done after setting up the initial data objects with Example1
#' Here, we try to save a data object that has indirect dependencies on different versions of the same
#' type. In this case, versions 1 and 2 of two
Example2 <- function () {

    # "three" version 1 depends on "two" version 1, and "five" version 1 depends on "two" version 2
    # this is not allowed, although currently is will succeed. However, it will fail when trying to read
    # TODO: check for this on writing data and stop with error
    data4.v2.dependencies <- list('three' = 1, 'five' = 1)
    data4.v2 <- .SaveExampleData("four", data4.v2.dependencies)


}


.SaveExampleData <- function (name, dependencies = list()) {

    data <- .RandomDataFrame()
    params <- .RandomParams()
    name <- name
    version <- WriteDataobject(data, name = name, params = params, dependencies = dependencies)
    return(version)

}



#' generates a random data frame
#' @value data.frame
.RandomDataFrame <- function () {
    ncols.range = 4:8
    nrows.range = 15:25
    ncols <- sample(ncols.range, 1)
    nrows <- sample(nrows.range,1)
    df <- as.data.frame(matrix(sample.int(ncols*nrows), ncol = ncols))
    colnames(df) <- .Wordlist(ncols)
    return(df)
}

#' Generates a list of random parameters
#' @param num.params
#' @value list
.RandomParams <- function (num.params = 3) {

    params <- as.list(sample.int(100, num.params))
    names(params) <- .Wordlist(num.params)
    return(params)

}


#' returns a random list of words
#' @param how many words to return
#' @value character vector
#' @details
#' If the number of words requested is > than the number in our list,
#' it will sample with replacement, otherwise not
#' This is only used for generating example data
.Wordlist <- function (num.words) {

    words <- c("people","history","way","art","world","information","map","two","family",
               "government","health","system","computer","meat","year","thanks","music",
               "person","reading","method","data","food","understanding","theory","law",
               "bird","literature","problem","software","control","knowledge","power",
               "ability","economics","love","internet","television","science","library",
               "nature","fact","product","idea","temperature","investment","area","society",
               "activity","story","industry","media","thing","oven","community","definition",
               "safety","quality","development","language","management","player","variety",
               "video","week","security","country","exam","movie","organization","equipment",
               "physics","analysis","policy","series")

    replace <- num.words > length(words)
    return(sample(words, num.words, replace = replace))

}


CreateEmptyMeta <- .CreateEmptyMeta
