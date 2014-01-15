

ReadEvents <- function () {
    return(read.csv(OutputPath('events'), header = TRUE, 
                    col.names = g.events.col.names))
}


MergeEvents <- function () {
    # takes the separate list of events and merges them into one big long list
    
    Report(3, "merging events ...")
    Report(2, 'target times (set in config):');
    Report(2, 'From: ', g.start.date, ' minute number', g.start.min);
    Report(2, 'To: ', g.end.date, ' minute number', g.end.min);
    Report(2, 'Sites: ', g.sites);
    
    
    # set this to how many files to process, 
    # false to do them all
    limit <- FALSE 
    
    #read events.directory
    files <- list.files(path = g.events.source.dir, full.names = FALSE)
    if (limit == FALSE || limit > length(files)) {
        limit <- length(files)
    }
    # columns not existing in the source event files
    new.col.names <- g.events.col.names[1:4]
    
    # columns that exist in the source event files
    old.col.names <- g.events.col.names[5:8]
    
    output.path <- OutputPath('events')
    file.count <- 0
    
    file.statuses <- rep(NA, limit)
    
    
    for (f in 1:limit) {

        filename <- substr(files[f], 1, (nchar(files[f]) - 8))
        filename.parts <- unlist(strsplit(filename, '.', fixed=TRUE))
        site <- filename.parts[1]
        date <- FixDate(filename.parts[2])
        file.startmin <- as.numeric(filename.parts[3])
        file.duration <- as.numeric(filename.parts[4])
        file.start.is.within <- IsWithinTargetTimes(date, file.startmin, site)
        file.end.min <- file.startmin + file.duration - 1
        file.end.is.within <- IsWithinTargetTimes(date, file.end.min, site) 
       
        
        if (!file.start.is.within && !file.end.is.within) {
            Report(5, 'Skipping file', filename)
            file.statuses[f] <- 'skipped'
            next()
        } else if (!file.start.is.within || !file.end.is.within) {
            # this file spans the start or end times that we are looking at 
            file.statuses[f] <- 'partial'
            check.each.event <- TRUE 
        } else {
            # this file is comepletely within times that we are looking at 
            check.each.event <- FALSE  
            file.statuses[f] <- 'all'
        }

        events <- as.data.frame(
            read.csv(file.path(g.events.source.dir, files[f])))
        new.cols <- c()
        for (ev in 1:nrow(events)) {
            start.second <- file.startmin * 60 + events[ev, 1]
            if (check.each.event) {
                event.min <- floor(start.second / 60)
                if (event.min < g.start.min || event.min > g.end.min) {
                    #this event should not be included
                    next()
                }
            }
            new.cols <- c(new.cols, filename, site, 
                          date, start.second, events[ev,])
            
        }
        
        if (length(new.cols) > 0) { 
            file.count <- file.count + 1
            num.cols <- length(new.col.names) + length(old.col.names)
            events.complete <- matrix(data = new.cols, 
                                      ncol = num.cols, 
                                      byrow = TRUE)
            colnames(events.complete) <- c(new.col.names, old.col.names)
            is.first <- (file.count == 1)
            Report(4, 'Merging events from', filename, 
                   '(', nrow(events), ' events) is.first = ', is.first)
            write.table(events.complete, file = output.path, 
                        append = (!is.first), col.names = is.first, 
                        sep = ",", quote = FALSE, row.names = FALSE) 
        }

    }
    
    Report(4, paste('of', limit, 'files,', 
                    length(file.statuses[file.statuses == 'skipped']), 
                    'were not within the target,',
                    length(file.statuses[file.statuses == 'partial']), 
                    'were partly in within the target, and', 
                    length(file.statuses[file.statuses == 'all']), 
                    'were completely within the target'))
    
}



DrawEvent <- function (event.num) { 
    events <- read.csv(g.clusters.path, stringsAsFactors=FALSE)
    event <- events[event.num, ]
    g.wav.path <- paste(c(audio.dir, event$filename, '.wav'), collapse = '')
    spectro.path <- paste(c(spectros.dir, event$filename, '.csv'), 
                          collapse = '')
    if (file.exists(spectro.path)) {
        spectro <- ReadSpectro(spectro.path)
    } 
}

EventLables <- function (events) {
    
    cns <- colnames(events);
    
    labels <- apply(events, 1, 
                                   function (r) {
                                       time <- SecToTime(r[4])
                                       # todo: if start/end date are the same don't show date
                                       # if all from same site, don't show site
                                       return(time)
                                       #return(paste(r[2], r[3], time))
                                   })
    
    return(labels)
    
}

