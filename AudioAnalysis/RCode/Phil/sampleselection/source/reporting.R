########
# 
# outputting to screen functions
#
########


# if set to a string, will write output to a file of that name
# useful for parallel when output to console doesn't work
g.report.file = FALSE
g.report.socket = FALSE
g.report.console = TRUE


SetReportMode <- function (console = TRUE, file = FALSE, socket = FALSE, filname = 'log.txt', clear.file = TRUE) {
    # sets the report mode for future report calls until this function is called again
    # 
    # Args
    #   console: boolean
    #   file: boolean
    #   socket: boolean
    #   filename: string; if file == TRUE, where to log to
    #   clear.file: boolean; if file == TRUE, clear any existing logged?
    
    
    
    
    if (file) {
        # todo: stuff for writing to file
        g.report.file <<- filname
        if (clear.file) {
            writeLines(c(""), g.report.file)
        }
    } else {
        g.report.file <<- FALSE
    }
    
    if (class(g.report.socket) == 'socket') {
        close.socket(g.report.socket)
    }
    g.report.socket <<- FALSE
    
    if (socket) {
        while (!class(g.report.socket) == 'socket') {
            
            print("in termial type nc -l [port]");
            socket.port <- ReadInt()
            
            g.report.socket <<-  tryCatch({
                 make.socket(port = socket.port)   
            }, warning = function(w) {
                g.report.socket <<- FALSE
                print("hey, You must start listening on port before typing there port here");
                return(FALSE)
            }, error = function(e) {
                g.report.socket <<-  FALSE
                print("You must start listening on port before typing there port here");
                return(FALSE)
            })

            
        }
        
         print(paste("outputting to socket on port", g.report.socket$port))
    }
    

}


WriteMsg <- function (msg) {
    # writes a message to the correct destination
    # which might be the console
    # or for parallel processing might be a socket or log file
    
    if (is.character(g.report.file)) {
        sink(g.report.file, append=TRUE)
    } 
    if (class(g.report.socket) == 'socket') {
        write.socket(g.report.socket, msg)
    }
    if (g.report.console) {
        cat(msg)   
    }

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
            WriteMsg("\n")
        }
        WriteMsg(paste(c(paste(as.vector(list(...)),  collapse = " ")), collapse = ""))
        if (nl.after) {
            WriteMsg("\n")
        }
    }
}

ReportAnimated <- function (level, ..., nl.before = FALSE, nl.after = TRUE, duration = NULL, after = 4) {
    # prints to screen, but does it one character at a time
    if (level <= g.report.level) {
        if (nl.before) {
            WriteMsg("\n")
        }
        str <- paste(c(paste(as.vector(list(...)),  collapse = " ")), collapse = "")
        str <- strsplit(str, '')[[1]]
        str <- c(str, rep(".", round(after*length(str))))
        if (is.null(duration)) {
            sleep.for <- 0.1
        } else {
            sleep.for <- duration / length(str)  
        }
        
        for (char in str) {
            WriteMsg(char)
            Sys.sleep(sleep.for)
        }
        
        if (nl.after) {
            WriteMsg("\n")
        }
    }
}

Dot <- function(level = 5) {
    #outputs a dot, used for feedback during long loops
    if (level <= g.report.level) {
        WriteMsg(".")
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
