# this file contains functions that handle the configuration, mainly regarding paths where the data files are saved

pkg.env <- new.env()
pkg.env$access.log <- list()
pkg.env$config.file.name <- 'datatrack.config.json'

#' saves the relevant global configuration variables to disk in the working directory
#' @details
#' This is called if the configuration file was not present and is created interactively
.SaveConfig = function () {
    config.json <- toJSON(pkg.env$config)
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
    pkg.env$config <- list()
    if (file.exists(pkg.env$config.file.name)) {
        config.json <- readChar(pkg.env$config.file.name, file.info(pkg.env$config.file.name)$size)
        pkg.env$config <- fromJSON(config.json)
    } else {
        create.config <- userinput::Confirm("No datatrack configuration file found in working directory, would you like to create one?")
        if (create.config) {
            .CreateConfig()
        }
    }

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
