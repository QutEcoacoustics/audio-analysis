# This file contains functions needed for launching the dependency graph in the R studio viewer

#' Display the visual datatrack inspector
#'
#' Generate the visualisation of saved data objects, their parameters and dependencies
#'
#' @param group string; name of the datatrack data object to filter nodes by.
#' Will only show nodes that are dependent on, or a dependency of an version of this name
#' @details
#' Gets the metadata in the appropriate json format then passes it to the dataGraph function of the dviewer package
#' @export
Inspector <- function (group = FALSE) {

    .LoadConfig()
    data <- .DataVis()
    #uncommet this to save a the metadata into the dviewer package
    .SaveDemoVisData(data)
    print(dviewer::dataGraph(data, group));
}


#' Helper function for development only: saves data from datatrack to the dviewer data source
#' so that it can be tested directly from the dviewer package without needing to use datatrack
#'
#' @details
#' For dviewer to be tested during development, it needs some test data. In order to get realistic test data
#' in the appropriate json format, use this function to save data generated from datatrack.
#' This function is not intended to be used other than for development of the dviewer package
.SaveDemoVisData <- function (test_data, path.to.package.source = file.path('..','..','custom_packages','dviewer','data')) {
    save(test_data, file = file.path(path.to.package.source, 'test_data.rda'))
}


#' Converts data to json for use with the dviewer package
#'
#' @details
#' Final format will be something like this:
#'
#' {'name':{'id':1,
#'          'format':'csv',
#'          'versions':[{"v":1,'params':{},'links':{'name':1},'cols':['a','b']}],
#'         }
#' }
.DataVis <- function () {

    m <- ReadMeta()
    group.names <- unique(m$name)

    # TODO: test this (changed during refactoring)
    group.format <- rep('object', nrow(m))
    group.format[m$csv == 1] <- 'csv'


    data <- list()

    for (g in 1:length(group.names)) {
        cur <-group.names[g]
        data[[g]] <- list(name = cur,
                          format = group.format[[g]],
                          versions = list())

        versions <- m[m$name == cur,]
        for (v in 1:nrow(versions)) {
            .Report('adding: ', group.names[g], versions[v], level = 2)
            data[[g]][['versions']][[v]] <- list(v = versions$version[v],
                                                 params = rjson::fromJSON(versions$params[v]),
                                                 links = rjson::fromJSON(versions$dependencies[v]),
                                                 date = versions$date[v],
                                                 exists = versions$file.exists[v])

            # add colnames if they are there
            if (is.character(versions$col.names[v]) && !is.na(versions$col.names[v]) && nzchar(versions$col.names[v])) {
                data[[g]][['versions']][[v]]$colnames = as.list(rjson::fromJSON(versions$col.names[v]))
            }
        }
    }

    data <- rjson::toJSON(data)
    return(data)
}



