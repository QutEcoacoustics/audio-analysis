





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
    WriteOutput(data.frame(min.id = target.min.ids), 'target.min.ids', new = new)
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

CreateMinuteList.old <- function () {
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
    # this is not done again after changing target. It only needs to 
    # be called if the original event detection is redone
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
    
    min.ids <- GetMinuteList();
    #event.min.ids <- min.ids$min.id[min.ids$site %in% all.events$site & min.ids$date %in% all.events$date & min.ids$min %in% all.events$min]
    
    all.events.with.min.id <- merge(all.events, min.ids, c('site', 'date', 'min'))
    all.events.with.min.id <- all.events.with.min.id[order(all.events.with.min.id$min.id),]
    

    WriteAllEvents(all.events.with.min.id);
}

GetOuterTargetEvents <- function () {
    # gets a subset of the target events according to the 
    # config targets, but ignoring the percent of target param
    all.events <- ReadAllEvents()
    selected.events <- TargetSubset(all.events)
    return(selected.events)   
}


GetInnerTargetEvents <- function () {
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
    return(file.path(g.output.master.dir, 'events', paste(g.all.events.version, 'csv', sep = '.')))  
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

