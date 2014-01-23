

ReadEvents <- function () {
    return(read.csv(OutputPath('events'), header = TRUE))
}

CreateMinuteList <- function () {
    # creates a list of random minutes between the target start date/time and end date/time
    # the number of minutes to create is set in config 
    #
    # Details:
    #   The list of minutes will be the target for the survey. The reason is to reduce the total
    #   processing needed while still keeping a representative subset of the target period. 
    #   results are written to the output path. 
    #
    #  TODO: update the way that output is saved
    #   
    start.date.time <- as.POSIXlt(g.start.date, tz = 'GMT') + 60 * (g.start.min - 1)
    end.date.time<- as.POSIXlt(g.end.date, tz = 'GMT') + 60 * g.end.min
    diff <- (as.double(end.date.time) - as.double(start.date.time)) / 60;   
    cols <- c('site', 'date', 'min');
    # the total number of mins in the target
    total.num.mins <- diff * length(g.sites);
    # the number of mins we want to include in the target
    num.desired.mins <- floor(total.num.mins * g.percent.of.target / 100)
    # random mins is the number of minutes to add to the start.date.time,
    # thats why it goes from zero
    random.mins <- sort(sample(0:(total.num.mins-1), num.desired.mins, replace = FALSE));
    site.num <- floor(random.mins / diff)
    min <- random.mins %% diff
    day <- floor(min / 1440)
    min <- min %% 1440
    site <- g.sites[site.num + 1]
    day <- format(start.date.time + 1440 * 60 * day, '%Y-%m-%d')
    min.list <- as.data.frame(cbind(site, day,  min), rownames = FALSE)
    colnames(min.list) <- c('site', 'date', 'min')
    
    # create a new output directory if there is less than 100 % of the target
    # being used, because the random minutes will be different
    if (g.percent.of.target == 100) {
        new = FALSE
    } else {
        new = TRUE
    }
    write.csv(min.list, OutputPath('minlist', new = new), row.names = FALSE)
    
    
}

MergeEvents <- function () {
    # for each target minute (generated in minute list) 
    # looks for the events and adds them to a single csv file
    # including the site, date, start second in day and minute num in day 
    # columns, as well as the frequency bounds and duration
    
    files <- list.files(path = g.events.source.dir, full.names = FALSE)


    output.path <- OutputPath('events')
    min.list.path <- OutputPath('minlist')
    min.list <- as.data.frame(read.csv(min.list.path))
    for(i in 1:nrow(min.list)) {
        site <- min.list[i, 1]
        date <- min.list[i, 2]
        min <- min.list[i, 3]
        event.file <- EventFile(site, date, min)
        fn <- file.path(g.events.source.dir, event.file$fn)
        if (!file.exists(fn)) {
            stop(paste('missing event file',fn))
        }
        events <- as.data.frame(read.csv(fn, header = FALSE))
        events <- AddMinCol(events, as.numeric(event.file$start.min))
        #subset the events
        events <- events[events$min == min, ]
        # add the date and site columns
        date.col <- rep(date, nrow(events))
        site.col <- rep(site, nrow(events))
        events <- cbind(site.col, date.col, events)
        if (!exists('selected.events')) {
            selected.events <- events
        } else {
            selected.events <- rbind(selected.events, events)
        }
    }
    
    colnames(selected.events) <- c('site', 'date', 'start.sec', 'start.sec.in.file', 'duration', 'bottom.frequency', 'top.frequency',  'min')
    
    # TODO: check that it is sorted correctly (by site, date and start second)
    
    write.csv(selected.events, output.path, row.names = FALSE)
}

MergeEvents_old <- function () {
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

EventFile <- function (site, date, min, ext = 'wav.txt') {
    # original event files are for 10 mins of audio
    min <- floor(min/10) * 10
    min <- sprintf('%03d',min)
    #convert to underscorem separated date
    date <- format(as.Date(date), '%Y_%m_%d')
    fn <- paste(site, date, min, 10, ext, sep = '.')
    return(list(fn = fn, start.min = min))
}

AddMinCol <- function (events, start.min) {
    # given a list of events where column 1 is start second in file
    # adds a column which is the minute that the event occurs in
    # and replaces the value for the start second in file with the 
    # start second in the day. 
    #
    # Args:
    #   events: data.frame or matrix
    #   start.min: the minute of the day that the event list starts at
    #     i.e. the start.sec.in.file is relative to this minute. 
    #     midnight = 0
    start.sec.in.day <- events[,1] + start.min * 60
    min <- floor(start.sec.in.day / 60)
    events <- cbind(start.sec.in.day, events, min)
    return(events)
}


