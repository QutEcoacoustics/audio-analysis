# this file contains functions that handle the configuration, mainly regarding paths where the data files are saved

pkg.env <- new.env()
pkg.env$config <- list()
pkg.env$access.log <- list()
pkg.env$config.file.name <- 'datatrack.config.json'

#' Sets the path to the config json file
#' @param path string
#' @details
#' By Default, datatrack will look for a file called 'datatrack.config.json' in the working directory
#' This can be overridden by this function. This is not recommended during normal use of datatrack,
#' since it needs to be called after datatrack is loaded but before it is used, leaving it prone to error.
#' It is designed for use for unit testing so that datatrack projects can be created and destroyed
SetConfigFile <- function (path) {
    pkg.env$config.file.name <- path
}

#' sets the config values and saves the config file
#' @param ... name value pairs as arguments or a list of name value pairs
SetConfig <- function (...) {
    args <- list(...)
    if(is.null(names(args)) && is.list(args[[1]])) {
        config.values <- args[[1]]
    } else {
        config.values <- args
    }
    pkg.env$config <- .MergeLists(config.values, pkg.env$config)
    .SaveConfig()
}


#' Sets configuration options to the default values if they have not been included in the config file
.SetDefaults <- function () {
    defaults <- list()
    # after this many days of not being accessed, data objects will be zipped to save space
    defaults$zip.after.days <- 30
    # The higher the number, the more reporting
    defaults$report.level <- 2

    pkg.env$config <- .MergeLists(pkg.env$config, defaults)
}


#' saves the relevant global configuration variables to disk in the working directory
#' @details
#' This is called if the configuration file was not present and is created interactively
.SaveConfig = function () {
    config.json <- rjson::toJSON(pkg.env$config)
    .SaveText(config.json, pkg.env$config.file.name)
}

#' loads configuration variables to the global package environment
#'
#' @details
#' For each project, configuration is saved in a file in the working directory
#' The most important configuration is the path to the datatrack data and metadata
#' Different datatrack projects can have different paths as long as the working directory is different
#' This function attempts to load the configuration file. If it is not there, then
#' the user can create one with the necessary config options interactively
.LoadConfig = function () {

    if (file.exists(pkg.env$config.file.name)) {
        config.json <- readChar(pkg.env$config.file.name, file.info(pkg.env$config.file.name)$size)
        pkg.env$config <- rjson::fromJSON(config.json)
    } else {
        create.config <- userinput::Confirm("No datatrack configuration file found in working directory, would you like to create one?")
        if (create.config) {
            .CreateConfig()
        }
    }

    .SetDefaults()

    # secondary config options that can be calculated based on the saved config options
    if ("datatrack.directory" %in% names(pkg.env$config) && is.character(pkg.env$config$datatrack.directory)) {
        pkg.env$data.dir <- file.path(pkg.env$config$datatrack.directory, 'dataobjects')
        pkg.env$meta.dir <- file.path(pkg.env$config$datatrack.directory, 'meta')
        .CheckPaths()
        return(TRUE)
    } else {
        # no config file was found, and user chose not to create one
        # therefore the configuration variables are not available and data track can't continue
        stop('No datatrack configuration available.')
        return(FALSE)
    }

}

#' prompts the user for values for config options then saves the file in the working directory
.CreateConfig <- function () {
    pkg.env$config <- list()
    pkg.env$config$datatrack.directory <- userinput::GetDirectory('Please enter the path to the datatrack directory')
    .SaveConfig()
}

#' Saves a text file to a given path
#'
#' @param str string
#' @param path string
.SaveText = function (str, path) {
    fileConn<-file(path)
    writeLines(str, fileConn)
    close(fileConn)
}

#' checks all global paths to make sure they exist
.CheckPaths <- function () {
    create.if.absent <- c(
        pkg.env$config$datatrack.directory,
        pkg.env$data.dir,
        pkg.env$meta.dir
    )
    lapply(create.if.absent, function (f) {
        if (!file.exists(f)) {
            dir.create(f)
        }
    })
}
