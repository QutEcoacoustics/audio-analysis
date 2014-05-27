Confirm <- function (msg, default = FALSE) {
    options <- c('Yes', 'No')
    choice <- GetUserChoice(options, msg, default)
    if (choice == 1) {
        return(TRUE)
    } else {
        return(FALSE)
    }  
}


GetUserChoice <- function (choices, choosing.what = "one of the following", default = 1, allow.range = FALSE) {
    #todo recursive validation like http://www.rexamples.com/4/Reading%20user%20input
    cat(paste0("choose ", choosing.what, ":\n"))
    cat(paste0(1:length(choices), ") ", choices, collapse = '\n'))
    if (default %in% 1:length(choices)) {
        cat(paste('\ndefault: ', default))
    } else {
        default = NA
    }
    msg <- paste0("enter int 1 to ",length(choices),": ")
    choice <- GetValidatedInt(msg, max = length(choices), default = default, parse.range = allow.range)  
    return(choice)
}

GetMultiUserchoice <- function (options, choosing.what = 'one of the following', default = 1, all = FALSE) {
    # allows the user to select 1 or more of the choices, returning a vector 
    # of the choice numbers
    
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

GetValidatedInt <- function (msg, max = NA, min = 1, default = NA, num.attempts = 0, parse.range = FALSE) { 
    max.attempts <- 8
    choice <- readline(paste(msg, " : "))
    if (choice == '' && !is.na(default)) {
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
        GetValidatedInt(msg, max = max, min = min, default = default, num.attempts = num.attempts + 1, parse.range = parse.range)
    } else {
        return(choice)
    }
}

GetValidatedFloat <- function (msg = 'Enter a number', max = NA, min = 0, default = NA, num.attempts = 0) {
    max.attempts <- 8 
    val <- readline(paste(msg, " : "))
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
        GetValidatedFloat(msg, max = max, min = min, default = default, num.attempts = num.attempts + 1)  
    } else {
        return(val)
    }
}



ReadInt <- function (msg = "Enter an integer", min = 1, max = NA, default = NA) { 
    extra <- c();
    if (!is.na(min)) {
        extra <- c(extra, paste('min', min))
    }
    if (!is.na(max)) {
        extra <- c(extra, paste('max', max))
    }
    if (length(extra) > 0) {
        msg <- paste0(msg, " (", paste(extra, collapse = ", "), ")")
    }
    val <- GetValidatedInt(min = min, max = max, default = default, msg = msg)
    return(val)    
}





Report <- function (level, ..., nl.before = FALSE, nl.after = TRUE) {
    # prints output to the screen if the level is above the 
    # global output level. 
    #
    # Args:
    #   level: int; how important is this? 1 = most important
    #   ... : strings;  concatenated to form the message
    #   nl: boolean; whether to start at a new line
    if (level <= g.report.level) {
        if (nl.before) {
            cat("\n")
        }
        cat(paste(c(paste(as.vector(list(...)),  collapse = " ")), collapse = ""))
        if (nl.after) {
            cat("\n")
        }
    }
}

Dot <- function(level = 5) {
    #outputs a dot, used for feedback during long loops
    if (level <= g.report.level) {
        cat(".")
    }
}

Timer <- function(prev = NULL, what = 'processing', num = NULL, per = "each") {
    # used for reporting on the execution time of parts of the code
    if(is.null(prev)) {
        return(proc.time())
    } else {
        t <- (proc.time() - prev)[3]
        Report(3, 'finished', what, 'in', round(t, 2), ' sec')
        if (is.numeric(num) && num > 0) {
            time.per.each <- round(t / num, 3)
            Report(3, time.per.each, "per", per)
        } 
    }
}

