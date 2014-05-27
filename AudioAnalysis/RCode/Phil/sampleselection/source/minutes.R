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
    min.list <- as.data.frame(cbind(site, day), rownames = FALSE, stringsAsFactors = FALSE)
    min.list$min <- min
    colnames(min.list) <- cols
    min.list$min.id <- 1:nrow(min.list)
    return(min.list)
    
}
CreateTargetMinutes <- function () { 
    study.min.list <- GetMinuteList()
    target.mins <- TargetSubset(study.min.list)
    if (g.percent.of.target < 100) { 
        num.to.include <- floor(nrow(target.mins)*g.percent.of.target/100)
        to.include <- GetIncluded(total.num = nrow(target.mins), num.included = num.to.include, offset = 0)
        target.min.ids <- target.mins$min.id[to.include]
        # create a new folder inside the output path
        # because target minutes are not deterministic if percent < 100
    }
    
    # create a new output directory if there is less than 100 % of the target
    # being used, because the random minutes will be different
    
    WriteOutput(target.mins, 'target.min.ids', list(minute.ranges = g.minute.ranges, start.date = g.start.date, end.date = g.end.date, sites = g.sites))
}

CreateTargetMinutesRandom <- function () {
    # randomly selects a subset of the target minutes
    # abandoned because it is non-deterministic and was making 
    # it difficult to save output based on minute selection
    # now use deterministic funciton CreateTargetMinutes
    study.min.list <- GetMinuteList()
    target.mins <- TargetSubset(study.min.list)
    if (g.percent.of.target < 100) { 
        random.mins <- sample(1:nrow(target.mins), floor(nrow(target.mins)*g.percent.of.target/100))
        target.min.ids <- target.mins$min.id[random.mins]
        # create a new folder inside the output path
        # because target minutes are not deterministic if percent < 100
        OutputPathL1(new = TRUE)
    }
    
    # create a new output directory if there is less than 100 % of the target
    # being used, because the random minutes will be different
    
    WriteOutput(target.mins, 'target.min.ids')
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
        rows.site.date <- df$site %in% g.sites & 
            as.character(df$date) >= g.start.date & 
            as.character(df$date) <= g.end.date
        if (length(g.minute.ranges) %% 2 > 0) {
            stop('g.minute.ranges must have an even number of entries')
        }
        start.mins <- g.minute.ranges[seq(1, length(g.minute.ranges) - 1, 2)]
        end.mins <- g.minute.ranges[seq(2, length(g.minute.ranges), 2)]
        rows.min <- rep(FALSE, nrow(df))
        for (i in 1:length(start.mins)) {
            rows.min <- rows.min | (df$min >= start.mins[i] & df$min <= end.mins[i])     
        }
        rows <- rows.site.date & rows.min
  
    return(df[rows, ])
    
}



ExpandMinId <- function (min.ids = NA) {
    # given a dataframe with a min.id column, or a vector of min ids
    # add columns for site, date, min (in day)
    
    
    if (class(min.ids) %in% c('numeric', 'integer')) {
        min.ids <- data.frame(min.id = min.ids)  
    } else if (class(min.ids) != 'data.frame') {
        min.ids <- ReadOutput('target.min.ids', level = 0)
    }
    row.names <- rownames(min.ids)
    full.min.list <- GetMinuteList()
    sub.min.list <- full.min.list[full.min.list$min.id %in% min.ids$min.id, c('site', 'date', 'min', 'min.id')]
    ordered.sub.min.list <- sub.min.list[order(order(min.ids$min.id)),]
    new.df <- cbind(min.ids, ordered.sub.min.list[, 1:3])
    # this should not be necessary: according to the doc
    # it should take the rownames of the first argument. But it isn't
    row.names(new.df) <- row.names
    return(new.df)
}


IsWithinTargetTimes <- function (date, min, site) {
    # determines whether the given date, startmin and site are within
    # the start and end date and min and list of sites to process
    #
    #  Args:
    #    date: String; 
    #    min: Int;
    #    site: String
    #
    #  Returns:
    #    Boolean
    # 
    # Details:
    #   first tests for site, then
    #   constructs date-time strings and compares the strings
    require('stringr')
    
    
    if (!site %in% g.sites) {
        return(FALSE)
    }
    date <- FixDate(date)
    start.date <- FixDate(g.start.date)
    end.date <- FixDate(g.end.date)
    start.date.time <- paste(start.date, MinToTime(g.start.min))
    end.date.time <- paste(end.date, MinToTime(g.end.min))
    date.time <- paste(date, MinToTime(min))
    if (date.time >=  start.date.time && date.time <= end.date.time) {
        return(TRUE)
    } else  {
        return(FALSE)
    }
}

AddMinuteIdCol <- function (data) {
    # given a data frame with the columns "date", "site", and either "min" or "start.sec", 
    # will look up the minute id for each row and add a column to the data frame
    #
    # Args:
    #   data: data.frame
    # 
    # Value:
    #   data.frame
    
    
    cols <- colnames(data)
    date.col <- match('date', cols)
    site.col <- match('site', cols)
    min.col <- match('min', cols)
    sec.col <- match('start.sec', cols)
    ids <- apply(as.matrix(data), 1, function (v) {
        if (is.na(min.col)) {
            min <- floor(as.numeric(v[sec.col]) / 60)
        } else {
            min <- as.numeric(v[min.col])
        } 
        id <- paste0(v[date.col], v[site.col], min)
        return(id)  
    })
    new.data <- cbind(data, ids)
    colnames(new.data) <- c(cols, 'min.id')
    return(new.data) 
}

SetMinute <- function (events, start.sec.col = "start.sec")  {
    # for a list of events which contains the filename 
    # (which has the start time for the file encoded)
    # and the start time of the event, works out the minute 
    # of the day that the event happened in
    
    if (is.character(start.sec.col)) {
        start.sec.col <- which( colnames(events) ==  start.sec.col)     
    }
    min <- apply(events, 1, function (v) {
        sec <- as.numeric(unlist(v[start.sec.col]))
        min <- floor(sec / 60)
        return (min)
    })
    new <- cbind(events, min)
    return (new)
    
}

