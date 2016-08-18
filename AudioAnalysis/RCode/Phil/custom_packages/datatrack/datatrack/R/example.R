# this file is for testing and learning to use datatrack.
# It allows the user to generate and save example data in a datatrack project


# for this example, we will generate 6 data objects with a few versions each


#' @export
ExampleB <- function () {
    
    set.seed(2)
    .InitialiseExamples(new = TRUE)
    
    data1.v1 <- WriteDataobject(.RandomDataFrame(), name = 'audio', params = list('days' = 1:4), annotations = 'Audio data from John Smith')
    
    data2.v1 <- WriteDataobject(.RandomDataFrame(), name = 'weather', params = list('days' = 1:4))
    
    data3.v1 <- WriteDataobject(.RandomDataFrame(), name = 'radar.wthr', params = list('days' = 1:4, 'threshold' = 0.5))
    data3.v2 <- WriteDataobject(.RandomDataFrame(), name = 'radar.wthr', params = list('days' = 1:4, 'threshold' = 0.8))
    
    
    # csv called 'events' with different parameters depending on the day
    d4.dependencies <- list(audio = data1.v1)
    data4.v1 <- WriteDataobject(.RandomDataFrame(), name = 'events', params = list('day' = 1), dependencies = d4.dependencies)
    data4.v2 <- WriteDataobject(.RandomDataFrame(), name = 'events', params = list('day' = 2), dependencies = d4.dependencies)
    data4.v3 <- WriteDataobject(.RandomDataFrame(), name = 'events', params = list('day' = 3), dependencies = d4.dependencies)
    data4.v4 <- WriteDataobject(.RandomDataFrame(), name = 'events', params = list('day' = 4), dependencies = d4.dependencies)
    
    # csv called 'event.features.1' for each day of events, except day 4
    # done twice each with a different parameters
    features.1.params.1 <- list('features' = c(1,2,5), 'sigma' = 0.4, 'entropy.threshold' = 4)
    data5.v1 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.1', params = features.1.params.1, dependencies = list('events' = data4.v1))  
    data5.v2 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.1', params = features.1.params.1, dependencies = list('events' = data4.v2))  
    data5.v3 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.1', params = features.1.params.1, dependencies = list('events' = data4.v3))  
    
    features.1.params.2 <- list('features' = c(1,2,5), 'sigma' = 0.6, 'entropy.threshold' = 7)
    data5.v6 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.1', params = features.1.params.2, dependencies = list('events' = data4.v1))  
    data5.v7 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.1', params = features.1.params.2, dependencies = list('events' = data4.v2))  
    data5.v8 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.1', params = features.1.params.2, dependencies = list('events' = data4.v3))  
    
    # csv called 'event.features.2' for each day of events, except day 4
    # done twice each with a different parameters
    features.2.params.1 <- list('features' = c(3,4,6), 'overlap' = 0.4, 'envelope.level' = 445)
    data7.v1 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.2', params = features.2.params.1, dependencies = list('events' = data4.v1))  
    data7.v2 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.2', params = features.2.params.1, dependencies = list('events' = data4.v2))  
    data7.v3 <- WriteDataobject(.RandomDataFrame('f'), name = 'event.features.2', params = features.2.params.1, dependencies = list('events' = data4.v3))  
    
    # csv called "clustering" which depends on event features 1 or event features 2
    data6.v1 <- WriteDataobject(.RandomBinaryData('kmeans'), name = 'clustering.1', params = list('k' = 50), dependencies = list('event.features.1' = data5.v1))  
    data6.v2 <- WriteDataobject(.RandomBinaryData('kmeans'), name = 'clustering.1', params = list('k' = 50), dependencies = list('event.features.1' = data5.v2))  
    data6.v3 <- WriteDataobject(.RandomBinaryData('kmeans'), name = 'clustering.1', params = list('k' = 60), dependencies = list('event.features.1' = data5.v1))  
    data6.v4 <- WriteDataobject(.RandomBinaryData('kmeans'), name = 'clustering.1', params = list('k' = 60), dependencies = list('event.features.1' = data5.v2)) 

    data6.v5 <- WriteDataobject(.RandomBinaryData('kmeans'), name = 'clustering.1', params = list('k' = 50), dependencies = list('event.features.2' = data7.v1))  
    data6.v6 <- WriteDataobject(.RandomBinaryData('kmeans'), name = 'clustering.1', params = list('k' = 50), dependencies = list('event.features.2' = data7.v2))  
    data6.v7 <- WriteDataobject(.RandomBinaryData('kmeans'), name = 'clustering.1', params = list('k' = 60), dependencies = list('event.features.2' = data7.v1))  
    data6.v8 <- WriteDataobject(.RandomBinaryData('kmeans'), name = 'clustering.1', params = list('k' = 60), dependencies = list('event.features.2' = data7.v2)) 
       
    # ranking, which uses clustering plus the weather and radar data
    data8.v1 <- WriteDataobject(.RandomDataFrame(), name = 'ranking', params = list('weights' = list('weather' = 0.2, 'radar' = 0.2), 'clusters' = 0.6), dependencies = list('clustering.1' = data6.v3, 'weather' = data1.v1, 'radar.wthr' = data2.v1))  
    data8.v2 <- WriteDataobject(.RandomDataFrame(), name = 'ranking', params = list('weights' = list('weather' = 0.2, 'radar' = 0.2), 'clusters' = 0.6), dependencies = list('clustering.1' = data6.v4, 'weather' = data1.v1, 'radar.wthr' = data2.v1))  
    data8.v3 <- WriteDataobject(.RandomDataFrame(), name = 'ranking', params = list('weights' = list('weather' = 0.2, 'radar' = 0.2), 'clusters' = 0.6), dependencies = list('clustering.1' = data6.v5, 'weather' = data1.v1, 'radar.wthr' = data2.v1))  
    data8.v4 <- WriteDataobject(.RandomDataFrame(), name = 'ranking', params = list('weights' = list('weather' = 0.2, 'radar' = 0.2), 'clusters' = 0.6), dependencies = list('clustering.1' = data6.v6, 'weather' = data1.v1, 'radar.wthr' = data2.v1))  
    
    
    
    
    
    
}






#' Runs all examples
#' This should produce consistent set of dataobjects and metadata
#' @export
RunAllExamples <- function () {

    # this ensures that all random data and parameters will be identical on each run
    set.seed(2)
    .InitialiseExamples(new = TRUE)
    Example1()
    #Example2()
    Example3()
    Example4()
    Example5()
    Example6()

    # I would like to ensure that, if starting with a blank datatrack project, running the examples would produce identical files
    # (i.e. nothing in the diff)
    # however, the date time is recorded, which means this is not possible.
    # therefore we use the checksum function, which removes these variable columns

    return(GetChecksum())

}


#' Creates a few example data objects with some dependencies between them
#' @export
Example1 <- function () {

    .InitialiseExamples()

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
    # dependency of a new data object named "five"
    data2.v2 <- .SaveExampleData("two")
    data5.v1.dependencies <- list('two' = data2.v2)
    .SaveExampleData("five", data5.v1.dependencies)

    ClearAccessLog()

}


#' Example showing attempting some incompatible dependencies
#' @details
#' This should be done after setting up the initial data objects with Example1
#' Here, we try to save a data object that has indirect dependencies on different versions of the same
#' name. This should not be allowed, but currently it is.
#' @export
Example2 <- function () {
    .InitialiseExamples()
    # "three" version 1 depends on "two" version 1, and "five" version 1 depends on "two" version 2
    # this is not allowed, although currently is will succeed. However, it will fail when trying to read
    # TODO: check for this on writing data and stop with error
    data4.v2.dependencies <- list('three' = 1, 'five' = 1)
    data4.v2 <- .SaveExampleData("four", data4.v2.dependencies)
}

#' Example showing reading data without specifying name
#' @export
Example3 <- function () {
    .InitialiseExamples()
    userinput::Preset(c('1'))
    on.exit(userinput::Preset())
    obj <- ReadDataobject(purpose = 'example 3')

}

#' Example showing reading data with specifying a name
#' @export
Example4 <- function () {
    .InitialiseExamples()
    userinput::Preset(c('1'))
    on.exit(userinput::Preset())
    obj <- ReadDataobject(name = 'two', purpose = 'example 5')

}

#' Example showing reading data specifying 2 names
#' @details
#' When 2 or more names are specified, it will return the last accessed version of any of those names
#' @export
Example5 <- function () {
    .InitialiseExamples()
    obj <- ReadDataobject(name = c('two', 'three'), purpose = 'example 5')
    return(obj)

}

#' Example showing saving non-csv data
#' @export
Example6 <- function () {
    .InitialiseExamples()
    # create a linear model to save
    one <- c(1,2,3,4,5,6,7,8,9)
    two <- c(2,1,3,2,4,3,5,4,6)
    three <- c(10,12,13,14,16,16,17,19,22)
    data = lm( one ~ two + three)
    params <- .RandomParams()
    dependencies <- list(three = 1)
    data6.v1 <- WriteDataobject(data, 'six', params = params, dependencies = dependencies)
}


#' Example showing saving with annotations
#' @details
#' Annotations can either be a list or just a string. This example saves one of each
#' @export
Example7 <- function () {
    .InitialiseExamples()
    data <- .RandomDataFrame()
    params <- .RandomParams()
    dependencies <- list('four', 2)
    annotations <- 'annotation for seven'
    data7.v1 <- WriteDataobject(data, 'six', params = params, dependencies = dependencies, annotations = annotations)
    # new params mean it will be saved as a different version
    params <- .RandomParams()
    annotations <- list(author = 'me', context = "lorem ipsom dolor sit amet")
    data7.v2 <- WriteDataobject(data, 'six', params = params, dependencies = dependencies, annotations = annotations)
}



#' Initialise the Examples
#' 
#' Creates a directory in the working directory to do all examples in, which will be deleted later
#' Then specifies this directory to use for config rather than the working directory
#' @param example.path character where to put the examples
#' @details
#' example.path should be manually deleted by the user. 
.InitialiseExamples <- function (example.path = file.path('.','datatrack_examples'), new = FALSE) {
    example.path <- .GetNumberedDir(example.path, new = new)
    dir.create(example.path, showWarnings = FALSE)
    SetConfigFile(file.path(example.path, 'datatrack.config.json'))
    SetConfig(datatrack.directory = file.path(example.path,'datatrack_data'))
    .LoadConfig()
}




#' Saves some example data
#' @param name character
#' @param dependencies list
#' @param seed int optional. This function generates random csv and params. In order to test we need deterministic results
#' Therefore seed can be set to ensure that the same results will come each time
.SaveExampleData <- function (name, dependencies = list()) {
    data <- .RandomDataFrame()
    params <- .RandomParams()
    name <- name
    version <- WriteDataobject(data, name = name, params = params, dependencies = dependencies)
    return(version)
}



#' generates a random data frame
#' @return data.frame
.RandomDataFrame <- function (col.heading = NULL) {
    ncols.range = 4:8
    nrows.range = 15:25
    ncols <- sample(ncols.range, 1)
    nrows <- sample(nrows.range,1)
    df <- as.data.frame(matrix(sample.int(ncols*nrows), ncol = ncols))
    if (is.null(col.heading)) {
        colnames(df) <- .Wordlist(ncols)
    } else {
        colnames(df) <- paste0(col.heading, 1:ncol(df)) 
    }

    return(df)
}

#' Generates random data not data.frame
#' @return mixed
.RandomBinaryData <- function (class.name) {
    data <- as.list(.RandomDataFrame())
    class(data) <- class.name
    return(data)
}

#' Generates a list of random parameters
#' @param num.params int
#' @return list
.RandomParams <- function (num.params = 3) {

    params <- as.list(sample.int(100, num.params))
    names(params) <- .Wordlist(num.params)
    return(params)

}


#' returns a random list of words
#' @param how many words to return
#' @return character vector
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



