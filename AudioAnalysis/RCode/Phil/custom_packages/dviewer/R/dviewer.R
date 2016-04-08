#' Create a html widget for the data viewer
#' @param data json string or list
#' @import htmlwidgets
#' @import rjson
#' @export
dataGraph <- function(data, trim = FALSE,
                  width = NULL, height = NULL) {

    if (is.list(data)) {
        data <- toJSON(data)
    }

    # create a list that contains the settings
    settings <- list(
        trim = trim
    )

    # pass the data and settings using 'x'
    x <- list(
        data = data,
        settings = settings
    )

    # create the widget
    htmlwidgets::createWidget("dviewer", x, width = width, height = height)
}

#' Standard functions to make the html widget work with shiny
#' @export
sigmaOutput <- function(outputId, width = "100%", height = "400px") {
    shinyWidgetOutput(outputId, "sigma", width, height, package = "sigma")
}
#' @export
renderSigma <- function(expr, env = parent.frame(), quoted = FALSE) {
    if (!quoted) { expr <- substitute(expr) } # force quoted
    shinyRenderWidget(expr, sigmaOutput, env, quoted = TRUE)
}

#' test the dviewer using the test data
#' @export
testDviewer <- function () {

    dataGraph(test_data)

}

#' set up demo with data
#' @export
demoData <- function () {

    fileConn<-file('inst/htmlwidgets/lib/dviewer-0.0.1/demo/data/demo_data.js')
    demo_data <- paste("var demo_data = JSON.parse('", test_data, "');")
    writeLines(demo_data, fileConn)
    close(fileConn)

}





