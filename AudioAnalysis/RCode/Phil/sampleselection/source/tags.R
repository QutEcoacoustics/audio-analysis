

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
                                                     'duration'), target = events)
    
    # remove date keep time only in the form hh:mm:ss.nnn
    time <- str_extract(str_extract(tags$start_date_time_char, " [0-9:.]+"), "[0-9:.]+")
    
    # split by colon to hour, min, sec (seconds have 3 decimal places)
    # cast from list of vectors of length 3 to a 3 column dataframe
    time <- as.data.frame(matrix(as.numeric(unlist(strsplit(time, ":"))), ncol = 3, byrow = TRUE))
    colnames(time) = c('hour', 'min1', 'sec')
    # add millisecond column
    time$millisec <- as.integer(str_extract(time$sec, "[0-9]+$"))
    # replace sec column to not have milliseconds
    time$sec <- as.integer(str_extract(time$sec, "^[0-9]+"))
    # add min in day column
    time$min <- (time$hour*60) + time$min1
    
    tags <- cbind(tags, time)
    
    # any annotation that starts very close to the end of a second, round the start up to the next second
    threshold <- 100
    
    # boolean vector of which tags start close to the end of the segment
    is.close <- tags$millisec > 1000-threshold & tags$duration > threshold
    
    # for that subset:
    
    
    # function to round the start millisecond up to the start of the next second
    bump.up.to.next.sec <- function (tags) {
        
        # add one to the sec column (move to next second)
        tags$sec <- tags$sec + 1
        # reduce the duration by the difference
        tags$duration <- tags$duration - (1000 - tags$millisec)
        # change the millisec to 0
        tags$millisec <- 0
        
        # bump up the relevant minutes where rounding up pushed it into the next minute
        tags$min[tags$sec == 60] = tags$min[tags$sec == 60] + 1
        
        return(tags)
    }
    
    tags[is.close,] <- bump.up.to.next.sec(tags[is.close,])
    
    # filter out very long annotations, because they are probably rubbish
    max.length <- 8000
    tags <- tags[tags$duration <= max.length, ]
    

    
    # probably should bump up the hours the same way, but not going to make much difference
    
    
    # if duration is > 1000 - start millisecond + threshold, it also includes the next second
    
    add.overlapping <- function(tags) {
        
        overlaps.if.duration.over <- 1000 + threshold - tags$millisec
        
        subset <- tags$duration > overlaps.if.duration.over
        
        if (sum(subset) > 1) {
        
            overlaps.to.next.sec <- tags[subset,]
            overlaps.to.next.sec <- bump.up.to.next.sec(overlaps.to.next.sec)
        
            # cutoff the ones we duplicated to the end of the second
            tags$duration[subset] <- overlaps.if.duration.over[subset] - 1
            
            overlaps.to.next.sec <- add.overlapping(overlaps.to.next.sec)
            
            tags <- rbind(tags, overlaps.to.next.sec)
            
        }
        
        return(tags)
        
        
    }
    
    tags.extended <- add.overlapping(tags)

    
    return(tags.extended)
    
    
}




