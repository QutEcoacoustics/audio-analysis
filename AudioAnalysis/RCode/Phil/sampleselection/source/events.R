



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
    start.date.time <- as.POSIXlt(g.start.date, tz = 'GMT') + 60 * (g.start.min)
    end.date.time<- as.POSIXlt(g.end.date, tz = 'GMT') + 60 * (g.end.min + 1)
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
    min <- (random.mins %% diff)  + g.start.min
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
    

    
    WriteOutput(min.list, 'minlist', new = new)

    return(min.list)
    
    
}


MergeEvents <- function () {
    # merge all events into 1 file, adding extra columns
    files <- list.files(path = g.events.source.dir, full.names = FALSE)
    Report(3, "Merging events")
    

    
    orig.col.names <- c('start.sec.in.file', 'duration', 'bottom.f', 'top.f')
     
    
    for(f in 1:length(files)) {

        filepath <- file.path(g.events.source.dir, files[f])
        if (file.info(filepath)$size == 0) {
            next()
        }
        events <- as.data.frame(read.csv(filepath, header = FALSE, col.names = orig.col.names));
        filename <- substr(files[f], 1, (nchar(files[f]) - 8));
        filename.parts <- unlist(strsplit(filename, '.', fixed=TRUE));
        file.site <- filename.parts[1];
        file.date <- FixDate(filename.parts[2]);
        file.startmin <- as.numeric(filename.parts[3]);
        file.duration <- as.numeric(filename.parts[4]);
        date <- rep(file.date, nrow(events))
        site <- rep(file.site, nrow(events))
        start.sec <- events$start.sec.in.file %% 60
        min <- ((events$start.sec.in.file - start.sec) / 60) + file.startmin
        filename <- rep(filename, nrow(events))
        
        events <- cbind(filename, date, site, start.sec, min, events)
        
        if (f == 1) {
            all.events <- events
            
        } else {
            
            all.events <- rbind(all.events, events)
        }
        
        
        
    }
    
    # sort by site then chronological. Should be already, except that I stuffed up the naming
    all.events <- all.events[with(all.events, order(site, date, min, start.sec)), ]
    
    event.id <- 1:nrow(all.events)
    all.events <- cbind(event.id, all.events)
    
    WriteAllEvents(all.events);
    
}

CreateEventList <- function () {
    
    min.list <- ReadOutput('minlist')
    
    all.events <- ReadAllEvents()
 
    
    
    for(m in 1:nrow(min.list)) {
        Dot()
        #subset
        condition <- all.events$site == min.list$site[m] & 
        all.events$date == min.list$date[m] &
        all.events$min == min.list$min[m]
    
        this.mins.events <- all.events[condition, ]
    
        min.id <- rep(m, nrow(this.mins.events))
        this.mins.events <- cbind(this.mins.events, min.id)
    
        if (m == 1) {
            all.selected.events <- this.mins.events
        } else {
            all.selected.events <- rbind(all.selected.events, this.mins.events)
        }
        
    }
    
    WriteOutput(all.selected.events, 'events')
    
    
    
}


MergeEventsOld <- function () {
    # for each target minute (generated in minute list) 
    # looks for the events and adds them to a single csv file
    # including the site, date, start second in day and minute num in day 
    # columns, as well as the frequency bounds and duration
    #
    # TODO:
    #   - handle missing event file (currently causes error, stops)
    
    files <- list.files(path = g.events.source.dir, full.names = FALSE)
 
    min.list <- ReadOutput('minlist')
    Report(3, "Merging events for each minute")
    prev.fn <- ''
    for(i in 1:nrow(min.list)) {
        Dot()
        site <- min.list[i, 1]
        date <- min.list[i, 2]
        min <- min.list[i, 3]
        min.id <- i
        event.file <- EventFile(site, date, min)
        
        fn <- file.path(g.events.source.dir, event.file$fn)
        if (fn != prev.fn) {
            if (!file.exists(fn)) {
                stop(paste('missing event file',fn))
            }
            #todo: make more efficient by not reading the same csv file over and over again
            if (file.info(fn)$size == 0) {
                next()
            }
            events.1 <- as.data.frame(read.csv(fn, header = FALSE))
            events.1 <- AddMinCol(events.1, as.numeric(event.file$start.min))
            prev.fn <- fn
        }
        #subset the events
        events <- events.1[events.1$min == min, ]
        # add the date and site columns
        min.id.col <- rep(min.id, nrow(events))
        date.col <- rep(date, nrow(events))
        site.col <- rep(site, nrow(events))
        events <- cbind(min.id.col, site.col, date.col, events)
        if (!exists('selected.events')) {
            selected.events <- events
        } else {
            selected.events <- rbind(selected.events, events)
        }
    }
    
    colnames(selected.events) <- c('min.id', 'site', 'date', 'start.sec', 'start.sec.in.file', 'duration', 'bottom.frequency', 'top.frequency',  'min')
    
    # TODO: check that it is sorted correctly (by site, date and start second)
    
    WriteOutput(selected.events, 'events')

}





EventLabels <- function (events) {
    
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


FromFn <- function (fn) {
    
    
    
    
    
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



WriteAllEvents <- function (events) {
    write.csv(events, file = AllEventsPath(), row.names = FALSE)
}
ReadAllEvents <- function () {
    return(read.csv(AllEventsPath(), header = TRUE, stringsAsFactors=FALSE))
}

AllEventsPath <- function () {
    return(file.path(g.output.parent.dir, 'events', paste(g.all.events.version, 'csv', sep = '.')))  
}

