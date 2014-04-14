MergeEvents <- function () {
    # merge all events into 1 file, adding extra columns
    # this is not done again after changing target. It only needs to 
    # be called if the original event detection is redone
    files <- list.files(path = g.events.source.dir, full.names = FALSE)
    Report(3, "Merging events")
    orig.col.names <- c('start.sec.in.file', 'duration', 'bottom.f', 'top.f')

    for(f in 1:length(files)) {
        filepath <- file.path(g.events.source.dir, files[f])
        if (!file.exists(filepath)) {
           stop('Check g.all.events.version in config') 
        }
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
    
    min.ids <- GetMinuteList();
    #event.min.ids <- min.ids$min.id[min.ids$site %in% all.events$site & min.ids$date %in% all.events$date & min.ids$min %in% all.events$min]
    
    all.events.with.min.id <- merge(all.events, min.ids, c('site', 'date', 'min'))
    all.events.with.min.id <- all.events.with.min.id[order(all.events.with.min.id$min.id),]
    

    WriteMasterOutput(all.events.with.min.id, 'events');
}



MergeEventsF2 <- function () {
    # Reads events produced from the F# AED and converts the format
    # of the csv to be compatible with this system. 
    # this is not done again after changing target. It only needs to 
    # be called if the original event detection is redone
    files <- list.files(path = g.events.source.dir, full.names = FALSE)
    Report(3, "Merging events")
    orig.col.names <- c('start.sec.in.file', 'duration', 'bottom.f', 'top.f')
    
    for(f in 1:length(files)) {
        filepath <- file.path(g.events.source.dir, files[f])
        if (!file.exists(filepath)) {
            stop('Check g.all.events.version in config') 
        }
        if (file.info(filepath)$size == 0) {
            next()
        }
        events <- as.data.frame(read.csv(filepath, header = TRUE));
        file.info <- ParseEventFileName(files[f])

        date <- rep(file.info$date, nrow(events))
        site <- rep(file.info$site, nrow(events))
        
        start.sec <- events$EvStartSec  # seconds from teh start of the minute the event is in
        min <- as.integer(events$EvStartMin)  # minute of the day (midnight is minute number 0)
        
        #filename and start.sec.in.file are needed when finding the audio which 
        # matches the events. Because these events were detected from audio which was segmented differently,
        # we don't know these yet
        NAs <- rep(NA, nrow(events))
        
        events <- data.frame(site = site, 
                             date = date, 
                             min = min, 
                             filename = NAs, 
                             start.sec = start.sec, 
                             start.sec.in.file = NAs,   
                             duration = events$EvDuration,
                             bottom.f = events$MIN_HZ,
                             top.f = events$MAX_HZ)
        if (f == 1) {
            all.events <- events
        } else {
            all.events <- rbind(all.events, events)
        }  
    }
    # sort by site then chronological (should be already but to be sure)
    all.events <- all.events[with(all.events, order(site, date, min, start.sec)), ]
    event.id <- 1:nrow(all.events)
    all.events <- cbind(event.id, all.events)
    
    min.ids <- GetMinuteList();
    #event.min.ids <- min.ids$min.id[min.ids$site %in% all.events$site & min.ids$date %in% all.events$date & min.ids$min %in% all.events$min]
    
    all.events.with.min.id <- merge(all.events, min.ids, c('site', 'date', 'min'))
    all.events.with.min.id <- all.events.with.min.id[order(all.events.with.min.id$min.id),]
    
    all.events.with.min.id <- AddFileInfo(all.events.with.min.id)
    
    WriteMasterOutput(all.events.with.min.id, 'events');
}

AddFileInfo <- function (events) {
    # given a dataframe of events which is missing the filename and start second in file
    # determines which audio file each event is in, and adds these columns
    # assumes audio files are segmented every 10 minutes
    

    finfo  <- EventFile(events$site, events$date, events$min, ext = NA)
    events$filename <- finfo$fn
    events$start.sec.in.file <-  60 * (events$min - finfo$start.min) + events$start.sec
    
    return(events)
    
}


MergeAnnotationsAsEvents <- function () {
    # to test everything after event detection using known "good" events
    # create an event list which uses the manually created annotations as events
    
    fields <- c('site', 'start_date', 'start_date_time_char', 'end_date_time_char', 'start_frequency', 'end_frequency')
    
    all.tags <- ReadTagsFromDb(fields = fields, target = FALSE)
    
    
    # temp small subset for debug
    # all.tags <- all.tags[1:30,]
    
    #convert start_date_time_char and end_date_time_char to second in day
    
    # returns matrix with a COLUMN for each event, and date attributes in each row
    start <- sapply(all.tags$start_date_time_char, ExplodeDatetime)
    end <- sapply(all.tags$end_date_time_char, ExplodeDatetime)
    
    filename <- rep(NA, nrow(all.tags))
    
    for (i in 1:nrow(all.tags)) {
        fn <- EventFile(site = all.tags$site[i], 
                                 date = as.character(start['date', i]), 
                                 min = as.integer(start['min.in.day', i]), 
                                 ext = NA)
        filename[i] <- fn$fn
        
    }
    
    start.sec.in.file <- sapply(as.numeric(start['sec.in.day', ]), function (s) {
        file.start.sec.in.day <- floor(s/600) * 600
        return(s - file.start.sec.in.day)
    })
    
    
    #TODO: fix for events which cross over dates
    
    events <- data.frame(site = all.tags$site, 
                         date = as.character(start['date', ]),
                         min = as.integer(start['min.in.day', ]),
                         event.id = 1:nrow(all.tags),
                         filename = filename,
                         start.sec = as.numeric(start['sec', ]),
                         start.sec.in.file = start.sec.in.file,
                         duration = as.numeric(end['sec.in.day', ]) - as.numeric(start['sec.in.day', ]),
                         bottom.f = all.tags$start_frequency,
                         top.f = all.tags$end_frequency
                         )
    
    
    
    
    
    min.ids <- GetMinuteList();
    all.events.with.min.id <- merge(events, min.ids, c('site', 'date', 'min'))
    all.events.with.min.id <- all.events.with.min.id[order(all.events.with.min.id$min.id),]
    all.events.with.min.id$event.id <- 1:nrow(all.tags) # put event.id in the correct order too
    

    WriteMasterOutput(all.events.with.min.id, 'events');
    
}

GetOuterTargetEvents <- function () {
    # gets a subset of the target events according to the 
    # config targets, but ignoring the percent of target param
    all.events <- ReadMasterOutput('events', false.if.missing = FALSE)
    selected.events <- TargetSubset(all.events)
    return(selected.events)   
}

GetInnerTargetEvents <- function () {
    min.list <- ReadOutput('minlist')
    all.events <- ReadMasterOutput('events')
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
    

    Return(all.selected.events)
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
    min.char <- sprintf('%03d',min)
    #convert to underscorem separated date
    date <- format(as.Date(date), '%Y_%m_%d')
    fn <- paste(site, date, min.char, 10, sep = '.')
    if (!is.na(ext)) {
        fn <- paste(fn, ext, sep='.')
    }
    return(list(fn = fn, start.min = min))
}

ParseEventFileName <- function (fn) {
    # given an filename named according to the convention for output of AED
    # will return the meta data encoded in the filename as a list
    # site, date, start min, duraion

    filename.parts <- unlist(strsplit(fn, '.', fixed=TRUE));
    file.info <- list()
    file.info$site <- filename.parts[1];
    file.info$date <- FixDate(filename.parts[2]);
    file.info$startmin <- as.numeric(filename.parts[3]);
    file.info$duration <- as.numeric(filename.parts[4]);
    return(file.info)
    
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

FilterEvents <- function (min.ids = FALSE) {
        events <- ReadMasterOutput('events')
        features <- ReadMasterOutput('rating.features')
        
        if (is.numeric(min.ids)) {        
            events <- events[events$min.id %in% min.ids, ]
            features <- features[features$event.id %in% events$event.id, ]       
        }
        
        # remove the "event id" from features and add a column of 1s to the start
        # also, perform feature scaling
        f2 <- cbind(rep(1, nrow(features)), scale(features[,1:(length(features)-1)]))

        #decision boundary
        #db <- c(1, 1, 1, 1, 1, 1, 1, -1)
        db <- c(1, 0, 0, 0, 0, 0, 0, -1)
        
        # todo, train this classifier with simoid cost function or something like that
        events$is.good <- as.vector(crossprod(db, t(f2))) >= 0
        WriteOutput(events, 'events', level = 1)
}


FilterEvents1 <- function (min.ids = FALSE, events = NA, rating.features = NA) {
    
    if (!is.data.frame(events) || !is.data.frame(rating.features)) {
        # must supply both or both will be read from master
        events <- ReadMasterOutput('events')
        features <- ReadMasterOutput('rating.features')   
    }

    
    if (is.numeric(min.ids)) {        
        events <- events[events$min.id %in% min.ids, ]
        rating.features <- rating.features[features$event.id %in% events$event.id, ]       
    }
    is.good <- rep(TRUE, nrow(events))
        
    # remove very long events
    is.good[rating.features$duration > 6] <- FALSE
    
    # remove very short
    is.good[rating.features$duration < 0.25] <- FALSE
    
    return(is.good)

}



# bug: sometimes returns events and features with different number of rows!
GetEventsAndFeatures <- function (reextract = FALSE) {
    
    SetOutputPath(level = 1) # for reading
    
    if (reextract || !OutputExists('events', level = 1) || !OutputExists('features', level = 1) || !OutputExists('rating.features', level = 1)) {
        
        limit <- ReadInt('limit the number of events') 
        
        Report(4, 'Retrieving target events and features. Copying from master')   
        target.min.ids <- ReadOutput('target.min.ids', level = 0)
        all.events <- ReadMasterOutput('events')
        all.feature.rows <- ReadOutputCsv(MasterOutputPath('features'))
        all.rating.feature.rows <- ReadOutputCsv(MasterOutputPath('rating.features'))
        events <- all.events[all.events$min.id %in% target.min.ids$min.id, ]
        event.features <- all.feature.rows[all.feature.rows$event.id %in% events$event.id, ]
        rating.features <- all.rating.feature.rows[all.rating.feature.rows$event.id %in% events$event.id, ]
        #ensure that both are sorted by event id
        events <- events[with(events, order(event.id)) ,]
        event.features <- event.features[with(event.features, order(event.id)) ,]
        rating.features <- rating.features[with(rating.features, order(event.id)) ,]
        
        event.filter <- FilterEvents1(events = events, rating.features = rating.features)
        events <- events[event.filter, ]
        event.features <- event.features[event.filter, ]
        rating.features <- rating.features[event.filter, ]
        
        
        # limit the number
        if (limit < nrow(events)) {
            Report(4, 'Number of target events (', nrow(events), ") is greater than limit (", limit ,"). Not all the events will be included. ")           
            include <- GetIncluded(nrow(events), limit)
            events <- events[include, ]
            event.features <- event.features[include, ]
            rating.features <- rating.features[include, ]
        }      
        
        WriteOutput(events, 'events', level = 1)
        WriteOutput(event.features, 'features', level = 1)
        WriteOutput(rating.features, 'rating.features', level = 1)
        
        
    } else {
        Report(4, 'Retrieving target events and features')
        events <- ReadOutput('events', level = 1)
        event.features <- ReadOutput('features', level = 1)
        rating.features <- ReadOutput('rating.features', level = 1)  
    }
    
    # remove event.id.column from features table
    drop.cols <- names(event.features) %in% c('event.id')
    event.features <- event.features[!drop.cols]
    drop.cols <- names(rating.features) %in% c('event.id')
    rating.features <- rating.features[!drop.cols]
    return (list(events = events, event.features = event.features, rating.features = rating.features))
}



