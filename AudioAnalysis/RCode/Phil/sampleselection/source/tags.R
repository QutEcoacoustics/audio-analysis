GetTags <- function (fields = c('start_date', 
                                'start_time', 
                                'site', 
                                'species_id')) {
    # selects the tags for the appropriate sites, between the 
    # appropriate date times from the local MySql database
    
    require('RMySQL')
    
    start.time <- MinToTime(g.start.min)
    end.time <- MinToTime(g.end.min)
    
    # need to round start time down to the nearest 10 mins 
    # and end time up to the nearest 10 mins. 
    
    start.date.time <- paste0("'",g.start.date," ",start.time,"'")
    end.date.time <- paste0("'",g.end.date," ",end.time,"'")
    
    # convert vector of sites to comma separated list of 
    # quoted strings between parentheses
    sites <- paste0("('",paste0(g.sites, collapse="','"),"')")
    
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
    con <- ConnectToDb()
    res <- dbSendQuery(con, statement = sql.statement)
    data <- fetch(res, n = - 1)
    mysqlCloseConnection(con)
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
