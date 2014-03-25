





GetMinuteList <- function () {
    # creates a list of minutes for the total of the entire study, 
    # giving each minute an id. 
    # for 5 days * 4 sites processing time = 0.55 seconds. 
    start.date.time <- as.POSIXlt(g.study.start.date, tz = 'GMT') + 60 * (g.study.start.min)
    end.date.time<- as.POSIXlt(g.study.end.date, tz = 'GMT') + 60 * (g.study.end.min + 1)
    diff <- (as.double(end.date.time) - as.double(start.date.time)) / 60;  
    cols <- c('site', 'date', 'min');
    total.num.mins <- diff * length(g.study.sites);
    min.ids <- 0:(total.num.mins-1)
    site.num <- floor(min.ids / diff)
    min <- (min.ids %% diff)  + g.study.start.min
    day <- floor(min / 1440)
    min <- min %% 1440
    site <- g.study.sites[site.num + 1]
    day <- format(start.date.time + 1440 * 60 * day, '%Y-%m-%d')
    min.list <- as.data.frame(cbind(site, day,  min), rownames = FALSE, stringsAsFactors = FALSE)
    colnames(min.list) <- cols
    min.list$min.id <- 1:nrow(min.list)
    return(min.list)
    
}


CreateTargetMinutes <- function () { 
    study.min.list <- GetMinuteList()
    target.mins <- TargetSubset(study.min.list)
    if (g.percent.of.target < 100) { 
        random.mins <- sample(1:nrow(target.mins), floor(nrow(target.mins)*g.percent.of.target/100))
        target.min.ids <- target.mins$min.id[random.mins] 
        new = TRUE
    } else {
        new = FALSE
    }
    
    # create a new output directory if there is less than 100 % of the target
    # being used, because the random minutes will be different

    WriteOutput(target.mins, 'target.min.ids', new = new)
}

TargetSubset <- function (df) {
    # returns a subset of the dataframe, includes only rows that 
    # belong within the outer target sites/times. 
    #
    # Args:
    #   df: data.frame; must have the columns site, date, min
    # 
    # Value
    #   data.frame
    
    rows <- df$site %in% g.sites & 
        as.character(df$date) >= g.start.date & 
        as.character(df$date) <= g.end.date & 
        as.numeric(df$min) >= g.start.min & 
        as.numeric(df$min) <= g.end.min

    return(df[rows, ])
    
    
}



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
    all.events <- ReadMasterOutput('events')
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
    
    #WriteOutput(all.selected.events, 'events')
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
    min <- sprintf('%03d',min)
    #convert to underscorem separated date
    date <- format(as.Date(date), '%Y_%m_%d')
    fn <- paste(site, date, min, 10, sep = '.')
    if (!is.na(ext)) {
        fn <- paste(fn, ext, sep='.')
    }
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


ExpandMinId <- function (min.ids = NA) {
    # given a dataframe with a min.id column
    # add columns for site, date, min (in day)
    
    
    if (class(min.ids) %in% c('numeric', 'integer')) {
        min.ids <- data.frame(min.id = min.ids)  
    } else if (class(min.ids) != 'data.frame') {
        min.ids <- ReadOutput('target.min.ids')
    }
    row.names <- rownames(min.ids)
    full.min.list <- GetMinuteList()
    sub.min.list <- full.min.list[full.min.list$min.id %in% min.ids$min.id, c('site', 'date', 'min')]
    new.df <- cbind(min.ids, sub.min.list)
    # this should not be necessary: according to the doc
    # it should take the rownames of the first argument. But it isn't
    row.names(new.df) <- row.names
    return(new.df)
}

