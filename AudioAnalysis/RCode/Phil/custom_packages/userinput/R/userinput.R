pkg.env <- new.env()
pkg.env$preset.input = character()



#' Prompts the user for confirmation in the console
#'
#' @param msg string the thing that you are want a yes/no answer to
#' @param default mixed optional. If supplied and y or yes or similar, will default to true.
#'                                If supplied and not similar to yes will default to no.
#'                                If not supplied, will not have a default and the user must choose an option
#' @return boolean
#' @details
#' Presents the user with a yes/no question and returns true or false depending on their answer
#' @export
Confirm <- function (msg, default = NULL) {
    options <- c('Yes', 'No')
    if (!is.null(default)) {
        if (default %in% c('y', 'Y', 'yes', 1, TRUE)) {
            default <- 1
        } else {
            default <- 2
        }
        options[default] <- paste(options[default], "(default)")
    }
    # output yes / no
    cat(paste0(1:length(options), ") ", options, collapse = '\n'))
    choice <- .GetValidatedInt(msg, max = length(options), default = default, parse.range = FALSE, equivalents = list('y' = 1, 'n' = 2))
    if (choice == 1) {
        return(TRUE)
    } else {
        return(FALSE)
    }
}

#' Prompts the user to choose one of the given set of choices
#'
#' @param choices vector of strings
#' @param choosing.what string; used for presenting the instructions to the user
#' @param default int if the user just hits enter, this will be chosen
#' @param allow.range boolean if TRUE, the user can enter something like 2-4 which will return c(2,3,4)
#' @param optional boolean if TRUE, user can select 0 to return false (i.e. no choice)
#' @return int the index of the choice selected by the user
#' @export
GetUserChoice <- function (choices, choosing.what = "one of the following", default = 1, allow.range = FALSE, optional = FALSE) {

    #todo recursive validation like http://www.rexamples.com/4/Reading%20user%20input
    cat(paste0("choose ", choosing.what, ":\n"))
    if (optional) {
        cat(paste0(0:length(choices), ") ", c("none", choices), collapse = '\n'))
        min <- 0
    } else {
        cat(paste0(1:length(choices), ") ", choices, collapse = '\n'))
        min <- 1
    }

    if (default %in% 1:length(choices)) {
        cat(paste('\ndefault: ', default))
    } else {
        default = NA
    }
    msg <- paste0("enter int 1 to ",length(choices),": ")
    choice <- .GetValidatedInt(msg, min = min, max = length(choices), default = default, parse.range = allow.range)
    return(choice)
}

#' allows the user to select 1 or more of the choices,
#'
#' @param options string vector; list of choices
#' @param choosing.what string; instrucitons for user
#' @param default int or string "all"; which options should be selected if the just hits clicks 'enter'
#' @param all boolean; should there be an extra option at the end to choose all the options in the list?
#' @return int vector of the choice numbers
#' @export
GetMultiUserchoice <- function (options, choosing.what = 'one of the following', default = 1, all = FALSE) {

    if (length(options) == 1 && (default == 1 || default == 'all')) {
        # if there was only 1 option and the default is 1 or 'all',
        # then just return that option without getting user input
        return(c(1))
    }

    if (default == 'all') {
        all <- TRUE
    }

    if (all) {
        options <- c(options, 'all')
        all.choice <- length(options)
        if (default == 'all') {
            default <- all.choice
        }
    } else {
        all.choice <- -99  # can't choose all
    }

    options <- c(options, 'exit')
    exit.choice <- length(options)
    last.choice <- -1;
    chosen <- c()
    while(TRUE) {
        if (max(last.choice) > 0) {
            # if something has been chosen, change the default to exit
            default = exit.choice
        }
        last.choice <- GetUserChoice(options, choosing.what, default = default, allow.range = TRUE)
        should.exit <- exit.choice %in% last.choice
        should.use.all <- all.choice %in% last.choice
        if (should.use.all) {
            chosen <- 1:length(options)
            break()
        }
        if (should.exit) {
            break()
        } else  {
            chosen <- union(chosen, last.choice)
        }
    }
    # setdiff also returns unique
    chosen <- setdiff(chosen, c(exit.choice, all.choice))
    return(chosen)
}



#' Prompts the user to enter an integer
#'
#' @param msg string; the message to display. eg, choose a number between 1 and 10, or choose from the following options
#' @param max int; optional. the highest allowed integer
#' @param min int; optional. the lowest allowed integer
#' @param default int; optional. The integer which will be returned if nothing is inputted (i.e. user hits return)
#' @param num.attempts int; The number of attempts attempted so far. The method recurses on itself to give the user another chance if
#'                    the input doesn't validate. This is only used by the recusive function call.
#' @param parse.range boolean; if TRUE, validates a range of int in the form "from-to", eg 2-4, and returns a vector containing that range
#' @param equivalents list; Allows the user to enter any of the values in the list which will be interpreted as the corresponding name in the list
#' @param quit string; If the input equals this, the program will quit. Allows the user to quit during a request for input
.GetValidatedInt <- function (msg,
                             max = NA,
                             min = 1,
                             default = NULL,
                             num.attempts = 0,
                             parse.range = FALSE,
                             equivalents = list(),
                             quit =
                                 "Q") {


    max.attempts <- 8
    choice <- .ReadLine(paste(msg, " : "))

    if (choice == quit) {
        stop('quitting')
    }

    if (!is.null(equivalents[[choice]])) {
        choice <- equivalents[[choice]]
    }

    if (choice == '' && !is.null(default)) {
        choice <- as.integer(default)
    } else if (grepl("^[0-9]+$",choice)) {
        choice <- as.integer(choice)
    } else if (parse.range && grepl("^[0-9]+[ ]*[:-][ ]*[0-9]+$",choice)) {
        # split by hyphen and parse range
        values <- regmatches(choice, gregexpr("[0-9]+", choice))
        choice <- as.integer(values[[1]][1]):as.integer(values[[1]][2])
    }

    if (num.attempts > max.attempts) {
        stop("you kept entering an invalid choice, idiot")
    } else if (class(choice) != 'integer' || (!is.na(max) && max(choice) > max) || (!is.na(min) && min(choice) < min)) {
        if (num.attempts == 0) {
            msg <- paste("Invalid choice.", msg)
        }
        .GetValidatedInt(msg, max = max, min = min, default = default, num.attempts = num.attempts + 1, parse.range = parse.range, equivalents = equivalents)
    } else {
        return(choice)
    }
}

#' Prompts the user to enter an float
#'
#' @param msg string the message to display. eg, choose a number between 1 and 10, or choose from the following options
#' @param max float optional. the highest allowed number
#' @param min float optional. the lowest allowed number
#' @param default float optional. The integer which will be returned if nothing is inputted (i.e. user hits return)
#' @param num.attempts int; The number of attempts attempted so far. The method recurses on itself to give the user another chance if
#'                    the input doesn't validate. This is only used by the recusive function call.
#' @param parse.range boolean; if TRUE, validates a range of int in the form "from-to", eg 2-4, and returns a vector containing that range
#' @param equivalents list; Allows the user to enter any of the values in the list which will be interpreted as the corresponding name in the list
#' @param quit string; If the input equals this, the program will quit. Allows the user to quit during a request for input
#' TODO: refactor this to be more general. e.g. a list of validation rules as functions
.GetValidatedFloat <- function (msg = 'Enter a number', max = NA, min = 0, default = NA, num.attempts = 0, quit = "Q") {
    max.attempts <- 8
    val <- .ReadLine(paste(msg, " : "))
    if (val == quit) {
        stop('quitting')
    }
    if (val == '' && !is.na(default)) {
        val <- as.numeric(default)
    } else if (grepl("^-?[0-9]+.?[0-9]*$",val)) {
        val <- as.numeric(val)
    }
    if (num.attempts > max.attempts) {
        stop("you kept entering an invalid choice, idiot")
    } else if (class(val) != 'numeric' || (!is.na(max) && max(val) > max) || (!is.na(min) && min(val) < min)) {
        if (num.attempts == 0) {
            msg <- paste("Invalid choice.", msg)
        }
        .GetValidatedFloat(msg, max = max, min = min, default = default, num.attempts = num.attempts + 1)
    } else {
        return(val)
    }
}


#' Reads an int input from the user and re-prompts if they didn't enter an int
#' @param msg character
#' @param min int
#' @param max int
#' @param default int optional
#' @export
ReadInt <- function (msg = "Enter an integer", min = 1, max = NA, default = NULL) {
    extra <- c();
    if (!is.na(min)) {
        extra <- c(extra, paste('min', min))
    }
    if (!is.na(max)) {
        extra <- c(extra, paste('max', max))
    }

    if (!is.null(default)) {
        extra <- c(extra, paste('default', default))
    }

    if (length(extra) > 0) {
        msg <- paste0(msg, " (", paste(extra, collapse = ", "), ")")
    }
    val <- .GetValidatedInt(min = min, max = max, default = default, msg = msg)
    return(val)
}


#' prompts the user for a directory
#'
#' @param msg the prompt to show to the user
#' @param create.if.missing boolean whether to create the directory if it is missing or prompt
#' @details
#' after the user enters in a directory, it will check if the directory exists.
#' If it doesn't, it will prompt the user if they want to create it, or if create.if.missing is TRUE
#' will create it without asking. It will only create the directory itself, not parent directories.
#' e.g. if the user enters /a/b/c and /a/b doesn't exist, it will not create it. But if /a/b exists and
#'  /a/b/c doesn't exist, it will prompt to create c
#'  @export
GetDirectory = function (msg = 'please enter a path to the directory', create.if.missing = FALSE) {

    msg <- paste(msg, 'Enter . (dot) for the working directory. Enter blank string to cancel')

    while (is.character(msg)) {
        dir.path <- .ReadLine(paste(msg, " : "))
        if (dir.path == "") {
            return(FALSE)
        } else if (!file.exists(dirname(dir.path))) {
            msg <- paste("Sorry, ", dirname(dir.path), "doesn't exist. Please try again")
        } else if (file.exists(dir.path) && !file.info(dir.path)$isdir) {
            msg <- "Sorry, that path already exists as a file. Please try again"
        } else {
            msg <- FALSE
        }
    }

    if(!file.exists(dir.path)) {
        dir.missing.msg <- "The directory you entered doesn't exist. Would you like to create it?"
        if (create.if.missing || Confirm(dir.missing.msg)) {
            dir.create(dir.path)
        } else {
            return(FALSE)
        }
    }

    return(dir.path)


}

#' Wrapper for .ReadLine which first check if any preset input exists
#' and will return it if it does exist or .ReadLine if it doesn't
#' @param prompt character
#' @return character
#' @details
#' By allowing the presetting of user input, unit tests and examples can be run without
#' pausing to wait for user input.
.ReadLine <- function (prompt) {

    if (length(pkg.env$preset.input) > 0) {
        auto.input <- pkg.env$preset.input[1]
        cat(paste(prompt, auto.input, '(preset)'))
        pkg.env$preset.input <- pkg.env$preset.input[-1]
        return(auto.input)
    } else {
        return(readline(prompt))
    }

}

#' sets the preset input global variable, which if not empty will be used instead
#' of .ReadLine.
#'
#' Allows tests to preset the userinput without interrupting the test with .ReadLine.
#' @param user.input.strings character
#' @details
#' This should probably not be used except for its designed purpose of unit
#' tests on scripts that use userinput. It could cause unexpected behaviour if
#' by mistake something is left in the preset.input variable. Use on.exit(Preset())
#' @export
Preset <- function (user.input.strings = character()) {
    pkg.env$preset.input <- user.input.strings
}

#' Returns the preset input
#'
#' @return character
#' @export
GetPresets <- function () {
    return(pkg.env$preset.input)
}



