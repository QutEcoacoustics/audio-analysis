#' Contains functions for outputting feedback

#' prints output to the screen if the level is above the configuration output level
#' @param ... strings concatenated to form the message
#' @param level how important is the message
#' @param nl.before boolean add a new line before the message?
#' @param nl.after boolean add a new line after the message?
.Report <- function (..., level = 3, nl.before = FALSE, nl.after = TRUE) {

    if (level >= pkg.env$config$report.level) {
        if (nl.before) cat("\n")
        cat(paste(c(paste(as.vector(list(...)),  collapse = " ")), collapse = ""))
        if (nl.after) cat("\n")
    }
}
