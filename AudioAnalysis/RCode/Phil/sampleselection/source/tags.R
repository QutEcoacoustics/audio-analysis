

GetTags <- function (target.only = TRUE) {
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
    tags <- ReadTagsFromDb(tag.fields, target.only)
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



ReadTagsFromDb <- function (fields = c('start_date', 
                                'start_time', 
                                'site', 
                                'species_id'), target = TRUE) {
    # selects the tags for the appropriate sites, between the 
    # appropriate date times from the local MySql database
    # target: mixed: if true will use the target in config
    #                if false will get all tags from the study
    #                TODO: if a list, will use the values in the list as the target
    
    require('RMySQL')
    
    if (class(target) == 'list') {
    
    } else if (target == TRUE) {
        start.min <- g.start.min
        end.min <- g.end.min
        start.date <- g.start.date
        end.date <- g.end.date
        start.min <- g.start.min
        end.min <- g.end.min
        sites <- g.sites
    } else {
        start.min <- g.study.start.min
        end.min <- g.study.end.min
        start.date <- g.study.start.date
        end.date <- g.study.end.date
        start.min <- g.study.start.min
        end.min <- g.study.end.min
        sites <- g.study.sites 
    }
    
    start.time <- MinToTime(start.min)
    end.time <- MinToTime(end.min + 1) # target is inclusive of end min
    
    # need to round start time down to the nearest 10 mins 
    # and end time up to the nearest 10 mins. 
    
    start.date.time <- paste0("'",start.date," ",start.time,"'")
    end.date.time <- paste0("'",end.date," ",end.time,"'")
    
    # convert vector of sites to comma separated list of 
    # quoted strings between parentheses
    sites <- paste0("('",paste0(sites, collapse="','"),"')")
    
    # construct SQL statement
    sql.statement <- paste(
        "SELECT",
        paste(fields, collapse = ", "),
        "From tags",
        "WHERE species_id > 0",
        "AND duplicate = 0",
        "AND start_date_time >=",
        start.date.time,
        "AND end_date_time <=",
        end.date.time,
        "AND site in",
        sites
    );
    Report(5, sql.statement)
    
    con <- ConnectToDb()
    res <- dbSendQuery(con, statement = sql.statement)
    data <- fetch(res, n = - 1)
    mysqlCloseConnection(con)
    Report(5, 'query complete')
    return(data)
}


ConnectToDb <- function () {
    
    con <- dbConnect(dbDriver("MySQL"), 
                     user = g.tags.db.user, 
                     password = g.tags.db.password, 
                     dbname = g.tags.db.dbname, 
                     unix.socket = g.tags.db.unix.socket) 
    return(con)
    
}
