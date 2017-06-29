

GetTags <- function (target.only = TRUE, study.only = TRUE, no.duplicates = TRUE) {
    # Calls a function to get the tags from the db, 
    # then adds the minute number for each tag and modifies the 
    # column names
    #
    # Value:
    #   data.frame
    #     site, date, time, species.id, min
    Report(2, 'Checking mySql database of labeled minutes (tags)');
    tag.fields <- c('site',
                    'start_date', 
                    'start_time', 
                    'species_id')
    tags <- ReadTargetTagsFromDb(tag.fields, target.only, study.only, no.duplicates)
    date.col <- match('start_date', colnames(tags))
    time.col <- match('start_time', colnames(tags))
    # add a column which is the minute number in the day for each tag (eg 1000 = 4:40pm)
    min.nums <- apply(tags, 1, 
                      function (tag, date.col, time.col) {
                          sdt <- strptime(paste(tag[date.col], tag[time.col]), 
                                          format = '%Y-%m-%d %H:%M:%S')
                          hour <- format(sdt, format = '%H')
                          min <- format(sdt, format = '%M')
                          minnum <- as.numeric(hour) * 60 + as.numeric(min) 
                          return(minnum)
                      }, date.col, time.col)
    tags <- as.data.frame(cbind(tags, min.nums))
    colnames(tags) <- c('site', 'date', 'time', 'species.id','min')
    return(tags);    
}


GetTargetRange <- function (tmids = NULL) {
    # gets the smallest range to search the database
    # while only specifying start and end for date and minute
    
    range <- list();
    
    if (!is.data.frame(tmids)) {
        tmids <- ReadOutput('target.min.ids')$data   
    }
    
      
        range$sites <-  unique(tmids$site) 
        range$start.date <- min(tmids$date)
        range$end.date <- max(tmids$date)
        range$start.min <- min(tmids$min)
        range$end.min <- max(tmids$min)
        range$target.min.ids <- tmids       

    return(range)
    
}


ReadTargetTagsFromDb <- function (fields = c('start_date', 
                                'start_time', 
                                'site', 
                                'species_id'), target = TRUE, study.only = TRUE, no.duplicates = TRUE) {
    # selects the tags for the appropriate sites, between the 
    # appropriate date times from the local MySql database
    # target: mixed: if true will use the target in config
    #                if false will get all tags from the study
    #                TODO: if a list, will use the values in the list as the target
    
    
    
    if (class(target) == 'list') {
        # get tags for a specific list on minute ids
    } else if (target == TRUE) {
        # TODO: multi part of day minute selection
        # currently selects from the start of the first part 
        # until the end of the last partQ
        # TODO: this is broken!
        range <- GetTargetRange()
    } else if (is.data.frame(target)) {
        range <- GetTargetRange(target)
    } else if (study.only) {
        range <- list()
        range$start.min <- g.study.start.min
        range$end.min <- g.study.end.min
        range$start.date <- g.study.start.date
        range$end.date <- g.study.end.date
        range$sites <- g.study.sites 
    } else {
        range <- FALSE
    }
    
    if (is.list(range)) {
        
        start.time <- MinToTime(range$start.min)
        end.time <- MinToTime(range$end.min + 1) # target is inclusive of end min
        
        # need to round start time down to the nearest 10 mins 
        # and end time up to the nearest 10 mins. 
        
        start.date.time <- DbDateTime(range$start.date, start.time) 
        end.date.time <- DbDateTime(range$end.date, end.time) 
        
        sites <- range$sites
        
    } else {
        
        start.date.time <- NULL
        end.date.time <- NULL
        sites <- NULL
    }
    
    # TODO
    # we have searched for all tags between the first and last row of the target
    # but we need to remove any mins that are not present in the target
    # for now it doesn't matter, because we are only looking at one full day   

    data <- ReadTagsFromDb(fields, sites, start.date.time, end.date.time, no.duplicates)
    
    
    return(data)
}



ReadTagsFromDb <- function (fields, sites = NULL, start.date.time = NULL, end.date.time = NULL, no.duplicates = TRUE, reference.tags = FALSE, species.id = NULL) {
    # does a database query with the supplied constraints
    require('RMySQL')
    sites <- MapSites(sites)    
    where.statement <- WhereStatement(sites, start.date.time, end.date.time, no.duplicates, reference.tags, species.id)
    # construct SQL statement
    sql.statement <- paste(
        "SELECT",
        paste(fields, collapse = ", "),
        "From tags",
        where.statement,
        "ORDER BY site, start_date, start_time"
    )
    Report(5, sql.statement)
    con <- ConnectToDb()
    res <- dbSendQuery(con, statement = sql.statement)
    data <- fetch(res, n = - 1)
    dbClearResult(res)
    dbDisconnect(con)
    Report(5, 'query complete')
    data$site <- MapSites(data$site, FALSE) 
    return(data) 
    
}

WhereStatement <- function (sites = NULL, start.date.time = NULL, end.date.time = NULL, no.duplicates = TRUE, reference.tags = FALSE, species.id = NULL) {
    
    where.statement <- "WHERE species_id > 0"
    quote = "'"
    
    if (!is.null(sites)) {
        # convert vector of sites to comma separated list of 
        # quoted strings between parentheses
        sites <- paste0("('",paste0(sites, collapse="','"),"')")    
        where.statement <- c(where.statement, paste0("site in ", sites))
    }
    
    if (!is.null(start.date.time)) {
        if (!substr(start.date.time, 1, 1) == "'" || substr(start.date.time, 1, 1) == "'") {
            start.date.time <- paste0(quote, start.date.time, quote)
        }
        where.statement <- c(where.statement, paste0("start_date_time >= ", start.date.time))
    }
    
    if (!is.null(end.date.time)) {
        if (!substr(end.date.time, 1, 1) == "'" || substr(end.date.time, 1, 1) == "'") {
            end.date.time <- paste0(quote, end.date.time, quote)
        }
        where.statement <- c(where.statement, paste0("start_date_time <= ", end.date.time))
    }  
    
    if (no.duplicates) {
        where.statement <- c(where.statement, paste0("duplicate = 0"))
    }
    
    if (reference.tags) {
        where.statement <- c(where.statement, paste0("reference_tag > 0"))
    }
    
    if (!is.null(species.id)) {
        where.statement <- c(where.statement, paste0('species_id = ', species.id))
    }
    
    
    where.statement <- paste(where.statement, collapse = " AND ")
    
    return(where.statement)
    
}



FindTag <- function (site, date, start.time, end.time, bottom.f, top.f) {
    # looks for a tag which contains the frequency and time bounds given
    
    start.date.time <- DbDateTime(date, start.time) 
    end.date.time <- DbDateTime(date, end.time) 
    site <- MapSites(site)
    fields <- c('species_id', 'start_frequency', 'end_frequency')
    tags <- ReadTagsFromDb(fields, site, start.time, end.time)
    
    tags <- tags[tags$start_frequency < bottom.f & tags$end_frequency > top.f,]
    
    return(tags)
    
    
}

MapSites <- function (sites, to.db = TRUE) {
    # site names in the database are sometimes different from how we label them
    # in this system. This function converts between the names before and after
    # making database queries
    
    if (is.null(sites)) {
        return(NULL)
    }
    
    r.vals <- c('NE', 'NW', 'SE', 'SW')
    db.vals <- c('NE', 'NW', 'SE', 'SW Backup')
    
    if (to.db) {
        from <- r.vals
        to <- db.vals
    } else {
        from <- db.vals
        to <- r.vals
    }
    
    mapped <- to[match(sites, from)]
    if (any(is.na(mapped))) {
        stop('invalid site for database query')
    }
    return(mapped)
    
}


ConnectToDb <- function () {
    
    con <- dbConnect(dbDriver("MySQL"), 
                     user = g.tags.db.user, 
                     password = g.tags.db.password, 
                     dbname = g.tags.db.dbname, 
                     unix.socket = g.tags.db.unix.socket) 
    return(con)
    
}

InspectTags <- function () {
    tags <- GetTags(FALSE, FALSE,  TRUE)
    plot(tags$min, col = rgb(0,0,0,0.2))
}

DayByDaySummary <- function () {
    
    days <- c('NE', '2010-10-13',
              'NE', '2010-10-14',
              'NE', '2010-10-17',
              'NW', '2010-10-13',
              'NW', '2010-10-14',
              'SE', '2010-10-13',
              'SE', '2010-10-17',
              'SW', '2010-10-16'
              )
    
    days <- as.data.frame(matrix(days, ncol = 2, byrow = TRUE))
    days <- cbind(days, rep(NA, nrow(days)))
    colnames(days) <- c('site', 'date', 'num.species')
    
    tags <- GetTags(target.only = FALSE)
    
    for (d in 1:nrow(days)) {  
        days$num.species[d] <- length(unique(tags$species.id[tags$site == as.character(days$site[d]) & tags$date == as.character(days$date[d])]))  
    }
    
    total <- length(unique(tags$species.id))
    
    days <- rbind(days, data.frame(site = 'total', date = '', num.species = total))
    
    return(days)
    
}

DbDateTime <- function (date, time, quote = "'") {
    return(paste0(date," ",time))
}




ReadTagsFromDb.test <- function () {
    
    fields <- c('start_date', 'start_time', 'site', 'species_id')
    sites <- c('NE', 'NW')
    start.date.time <- "'2010-10-13 16:16:16'"
    end.date.time <- "'2010-10-13 17:17:17'"
    
    result <- ReadTagsFromDb(fields = fields, sites = sites, start.date.time = start.date.time, end.date.time = end.date.time)
    
    View(result)
    
    
}

GetSpeciesList <- function () {
    # Gets a list of species ids and species names
    
    require('RMySQL')
    
    con <- ConnectToDb()
    
    # all species
    #sql.statement <- "SELECT id, common_name FROM species"
    
    # conditions on tags
    sql.statement <- paste('SELECT tags.species_id as id, species.common_name as name FROM tags',
                           'JOIN species',
                           'ON species.id = tags.species_id',
                           'WHERE tags.species_id IS NOT NULL',
                           'GROUP BY tags.species_id')
    
    
    
    res <- dbSendQuery(con, statement = sql.statement)
    data <- fetch(res, n = - 1)
    dbClearResult(res)
    dbDisconnect(con)
    Report(5, 'query complete')
    return(data)   
        

    
}

GetSpeciesMinutesMatrix <- function () {
    

    # gets a list of tags
    tags <- GetTags(FALSE, TRUE, TRUE)

    mins <- tags$mins
    species.ids <- tags$species.id
    
    # hack to make a sparse matrix of species vs minutes, including minutes that have no species at all
    # add 1 of the first species to each minute of the day, then after calculating the sparse matrix, subtract 1 for that column
    sp1 <- tags$species.id[1]
    mins <- c(mins, 1:1440)
    species.ids <- c(species.ids, rep(sp1, 1440))
    
    m <- xtabs(~ mins + species.ids, sparse = TRUE)
    m <- as.matrix(m)
    
    # remove the extra count for each minute added to 
    m[,sp1] <- m[,sp1] - 1
    
    
    
}



AttachSpeciesToEvents <- function (events) {
    # adds the species to each segment 
    #
    # Args:
    #   events is a data frame containing
    #      - site, date, min
    #      - optionally max frequency and min frequency
    
    tags <- ReadTargetTagsFromDb(fields  = c('start_date', 
                                             'start_time', 
                                             'site', 
                                             'species_id',
                                             'start_date_time_char',
                                             'duration',
                                             'id'), target = events)
    
    # remove date keep time only in the form hh:mm:ss.nnn
    time <- str_extract(str_extract(tags$start_date_time_char, " [0-9:.]+"), "[0-9:.]+")
    
    # split by colon to hour, min, sec (seconds have 3 decimal places)
    # cast from list of vectors of length 3 to a 3 column dataframe
    time <- as.data.frame(matrix(as.numeric(unlist(strsplit(time, ":"))), ncol = 3, byrow = TRUE))
    colnames(time) = c('hour', 'min1', 'sec')
    
    # add millisecond column: match decimal number at the end of the as.character of the numeric version of the string
    # if it milliseconds is exactly 0, there will be no decimal and this will result in NA
    time$millisec <- as.numeric(str_extract(time$sec, "\\.[0-9]+$")) * 1000
    time$millisec[is.na(time$millisec)] <- 0
    # replace sec column to not have milliseconds 0-9 at start of string
    time$sec <- as.integer(str_extract(time$sec, "^[0-9]+"))
    # add min in day column
    time$min <- (time$hour*60) + time$min1
    
    tags <- cbind(tags, time)
    
    
    # filter out very long annotations, because they are probably rubbish
    # they probably contain very sparse repetitions
    max.length <- 8000
    tags <- tags[tags$duration <= max.length, ]
    
    
    # tighten the start and end of the annotation
    # increase the start time and decrease the duration. 
    # this is because most of the tags start a little bit early and end a little bit late
    
    # shorten each annotation by 50 ms plus 5%
    # short duration tags are more likely to be more accurate
    reduce.by <- 60 + tags$duration * 0.05
    tags$duration <- round(tags$duration - reduce.by)
    tags$millisec <- round(tags$millisec + reduce.by / 2)
    

    
    # fix milliseconds that are > 1000 and minutes that are > 60
    fix.start.time <- function (tags) {
        
        add.secs <- floor(tags$millisec / 1000)
        tags$sec <- tags$sec + add.secs
        tags$millisec <- tags$millisec - (1000 * add.secs)
        
        add.mins <- floor(tags$sec / 60)
        tags$min <- tags$min + add.mins
        tags$min1 <- tags$min1 + add.mins
        tags$sec <- tags$sec - (60 * add.mins)
        
        add.hours <- floor(tags$min1 / 60)
        tags$hour <- tags$hour + add.hours
        tags$min1 <- tags$min1 - (60 * add.hours)
        
        return(tags)
    
    }
    
    # some start times will now be more than 1000 millisec, i.e. they are now in the next second
    tags <- fix.start.time(tags)
    
    # function to round the start millisecond up to the start of the next second
    bump.up.to.next.sec <- function (tags) {
        
        # add one to the sec column (move to next second)
        #tags$sec <- tags$sec + 1
        
        # reduce the duration by the difference between the start millisecond and the end of the second
        diff <- 1000 - tags$millisec
        tags$duration <- tags$duration - diff
        # change the millisec to 0
        tags$millisec <- 1000
        
        tags <- fix.start.time(tags)
        
        # bump up the relevant minutes where rounding up pushed it into the next minute
        # tags$min[tags$sec == 60] = tags$min[tags$sec == 60] + 1
        
        # should do this for the hour as well, probably
        
        return(tags)
    }
    
    # if the annotation spans multiple segments, it should be split into a separate annotation for each segment
    # so that there are more annotations, but none spand multiple seconds
    add.overlapping <- function(tags) {
        overlaps.if.duration.over <- 1000 - tags$millisec
        subset <- tags$duration > overlaps.if.duration.over
        if (sum(subset) > 1) {
            
            # new df with the overlapping tags
            overlaps.to.next.sec <- tags[subset,]
            
            overlaps.to.next.sec <- bump.up.to.next.sec(overlaps.to.next.sec)
            
            # cutoff the ones we just duplicated to the end of the second
            tags$duration[subset] <- overlaps.if.duration.over[subset] - 1
            
            
            overlaps.to.next.sec <- add.overlapping(overlaps.to.next.sec)
            tags <- rbind(tags, overlaps.to.next.sec)
        }
        return(tags)
    }
    
    tags <- add.overlapping(tags)
    
    
    
    # threshold is the number of milliseconds
    # that the start of the annotation should be from the start of the second
    # and the end of the annotation should be from the end of the second
    threshold <- 200
    
    # any annotation that starts very close to the end of a second, round the start up to the next second
    
    #annotations that start too early
    too.early <- tags$millisec < threshold & tags$duration < threshold - tags$millisec
    
    # annotations that start too late. All durations should
    # be cropped to the end of the second, so we just need to check start time
    too.late <- tags$millisec > 1000-threshold 
    
    tags <- tags[!too.early & !too.late,]


    

    

    
    # remove seconds with more than one species annotated (there shouldn't be too many)
    # it is likely that there are many the seconds that have a single annotation which actually 
    # contain one or more other species that have not been annotated
    
    # find rows that where the date, site, min and sec are not unique
    
    # do this twice and get the OR of them. If fromLast == FALSE, it doesn't include the first duplicate, 
    # otherwise it doesn't include the last duplicate. We don't want to include any that appear more than once. 
    # those seconds with multiple annotations are not wanted. 
    are.duplicates1 <- duplicated(tags[,c('start_date','site','sec','min')])
    are.duplicates2 <- duplicated(tags[,c('start_date','site','sec','min')], fromLast = TRUE)
    are.duplicates <- are.duplicates1 | are.duplicates2
    
    duplicates <- tags[are.duplicates,]
    tags <- tags[!are.duplicates,]
    
    # now each second set of events either one tag in the list of tags or zero
    # attach the specis ID to the events 
    
    events$species.id <- events$annotation.id <- NA
    
    # species.id <- mapply(function (site, date, min, sec) {
    #     
    #     #mating site, date, min and sec
    #     matching <- tags$site == site & tags$start_date == date & tags$min == min & tags$sec == sec
    #     
    #     if (sum(matching) == 1) {
    #         return(tags$species_id[matching])
    # 
    #     }
    #     
    #     return(NA)
    #     
    # }, site=events$site, date=events$date, min=events$min, sec=events$start.sec, SIMPLIFY = TRUE)
    #
    #    events$species.id <- species.id
    
    
    # create a 'token' which is the combination of columns that identify the absoulte second
    # this makes it easier to vectorise the comparison
    events$token <- paste(events$site, events$date, events$min, events$start.sec, sep=".")
    tags$token <- paste(tags$site, tags$start_date, tags$min, tags$sec, sep=".")
    
    # this is not chronological order, because token is string and min,site are not fixed length
    # events <- events[order(events$token),]
    # tags <- tags[order(tags$token),]
    
    # put things in chronological order (should be already)
    events <- events[order(events$site, events$date, events$min, events$start.sec),]
    tags <- tags[order(tags$site, tags$start_date, tags$min, tags$sec),]
    
    
    #double check there are no duplicate tokens in tags (i.e. no seconds with 2 species)
    if (anyDuplicated(tags$token)) {
        stop('duplicate tag tokens')
    }
    
    # there may be some tag tokens that don't exist in the events 
    # this is because the database query just looks at the start and end event and gets all tags between those
    # there may be some seconds of the day missing due to filtering. 
    # remove any tags that are not in the events
    tags <- tags[tags$token %in% events$token,]
    
    matching.events.subset <- events$token %in% tags$token
    
    #sanity check
    if (!isTRUE(all.equal(events$token[matching.events.subset], tags$token))) {
        stop("events and tags don't line up")
    }
    
    events$species.id[matching.events.subset] <- tags$species_id
    events$annotation.id[matching.events.subset] <- tags$id
    

    

    
    return(events)
    
    
    
}




