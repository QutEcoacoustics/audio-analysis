

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
    } else if (study.only) {
        range <- list()
        range$start.min <- g.study.start.min
        range$end.min <- g.study.end.min
        range$start.date <- g.study.start.date
        range$end.date <- g.study.end.date
        range$sites <- g.study.sites 
    }
    
    if (target || study.only) {
        
        start.time <- MinToTime(range$start.min)
        end.time <- MinToTime(range$end.min + 1) # target is inclusive of end min
        
        # need to round start time down to the nearest 10 mins 
        # and end time up to the nearest 10 mins. 
        
        start.date.time <- DbDateTime(range$start.date, start.time) 
        end.date.time <- DbDateTime(range$end.date, end.time) 
        
    } else {
        
        start.date.time <- NULL
        end.date.time <- NULL
    }
    
    
    if (target) {
        # TODO
        # we have searched for all tags between the first and last row of the target
        # but we need to remove any mins that are not present in the target
        # for now it doesn't matter, because we are only looking at one full day 
    }

    data <- ReadTagsFromDb(fields, sites, start.date.time, end.date.time, no.duplicates)
    
    
    return(data)
}



ReadTagsFromDb <- function (fields, sites = NULL, start.date.time = NULL, end.date.time = NULL, no.duplicates = TRUE) {
    # does a database query with the supplied constraints
    require('RMySQL')
    sites <- MapSites(sites)    
    where.statement <- WhereStatement(sites, start.date.time, end.date.time, no.duplicates)
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

WhereStatement <- function (sites = NULL, start.date.time = NULL, end.date.time = NULL, no.duplicates = TRUE) {
    
    where.statement <- "WHERE species_id > 0"
    quote = "'"
    if (!substr(start.date.time, 1, 1) == "'" || substr(start.date.time, 1, 1) == "'") {
        start.date.time <- paste0(quote, start.date.time, quote)
    }
    if (!substr(end.date.time, 1, 1) == "'" || substr(end.date.time, 1, 1) == "'") {
        end.date.time <- paste0(quote, end.date.time, quote)
    }
    
    if (!is.null(sites)) {
        # convert vector of sites to comma separated list of 
        # quoted strings between parentheses
        sites <- paste0("('",paste0(sites, collapse="','"),"')")    
        where.statement <- c(where.statement, paste0("site in ", sites))
    }
    
    if (!is.null(start.date.time)) {
        where.statement <- c(where.statement, paste0("start_date_time >= ", start.date.time))
    }
    
    if (!is.null(end.date.time)) {
        where.statement <- c(where.statement, paste0("start_date_time <= ", end.date.time))
    }  
    
    if (no.duplicates) {
        where.statement <- c(where.statement, paste0("duplicate = 0"))
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
    return(paste0(quote,date," ",time))
}




ReadTagsFromDb.test <- function () {
    
    fields <- c('start_date', 'start_time', 'site', 'species_id')
    sites <- c('NE', 'NW')
    start.date.time <- "'2010-10-13 16:16:16'"
    end.date.time <- "'2010-10-13 17:17:17'"
    
    result <- ReadTagsFromDb(fields = fields, sites = sites, start.date.time = start.date.time, end.date.time = end.date.time)
    
    View(result)
    
    
}

