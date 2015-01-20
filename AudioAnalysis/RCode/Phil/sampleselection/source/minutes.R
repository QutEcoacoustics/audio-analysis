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

CreateTargetMinutes.old <- function () {
    # creates a list of target minute ids, based on
    # the specified start date and time, end date and time, and % of minutes to use (eg, 50% will use every 2nd minute)
    # writes the output to csv   
    study.min.list <- GetMinuteList()
    target.mins <- TargetSubset.old(study.min.list)
    if (g.percent.of.target < 100) { 
        num.to.include <- floor(nrow(target.mins)*g.percent.of.target/100)
        to.include <- GetIncluded(total.num = nrow(target.mins), num.included = num.to.include, offset = 0)
        target.min.ids <- target.mins$min.id[to.include]
    }
    
    # create a new output directory if there is less than 100 % of the target
    # being used, because the random minutes will be different
    params <- list(minute.ranges = g.minute.ranges, start.date = g.start.date, end.date = g.end.date, sites = g.sites)
    params$study <- paste0(g.study.start.date, '-', g.study.end.date, ' ',  g.study.start.min, '-', g.study.end.min, " ", paste(g.study.sites, collapse = ",")) # include the study, because it affects the minute ids
    
    
    
    WriteOutput(target.mins, 'target.min.ids', params = params)
}


CreateTargetMinutesDayByDay <- function () {
    # creates a separate list of target minute ids for each day in the target, 
    targets <- SplitTargetIntoDays(g.target)
    for (t in 1:length(targets)) {
        CreateTargetMinutes(targets[[t]])
    }
}

GetStudyDescription <- function () {
    # creates a deterministic string from the variables that define the study
    # while keeping it as short as possible
    
    sites <- paste(g.study.sites, collapse = ",")
    date.parts <- GetDateParts(c(g.study.start.date, g.study.end.date))
    date.txt <- paste(date.parts$prefix, paste(date.parts$dates, collapse = '-'), sep = " ")
    
    val <- paste(sites, date.txt, sep = ":");
    if (g.study.start.min != 0 || g.study.end.min != 1439) {
        min.txt <- paste(g.study.start.min, g.study.end.min, sep = "-")
        val <- paste(val, min.txt, sep = ":")
    }

    return(val)
    

    
    
    
}


CreateTargetMinutes <- function (target = NULL) {
    # creates a list of target minute ids, based on
    # the target nested list in config
    # writes the output to csv  
    if (is.null(target)) {
        target <- g.target
    }
    
    study.min.list <- GetMinuteList()
    target.mins <- TargetSubset(study.min.list, target)
    if (g.percent.of.target < 100) { 
        num.to.include <- floor(nrow(target.mins)*g.percent.of.target/100)
        to.include <- GetIncluded(total.num = nrow(target.mins), num.included = num.to.include, offset = 0)
        target.min.ids <- target.mins$min.id[to.include]
    }
    
    params <- list(target = GetTargetDescription(target))
    WriteOutput(target.mins, 'target.min.ids', params = params)
    
    
}

TargetSubset <- function (df, target) {
    # returns a subset of the dataframe, includes only rows that 
    # belong within sites, dates and minute ranges (1 or more pairs of start/end minutes of the day)
    # using the target nested list
    # defined in config
    #
    # Args:
    #   df: data.frame; must have the columns site, date, min
    # 
    # Value
    #   data.frame
    
    sites <- names(target)
    
    selection <- rep(FALSE, nrow(df))
    
    for (site in sites) {
        dates <- names(target[[site]])
        for (date in dates) {
            ranges <- target[[site]][[date]]
            start.mins <- ranges[seq(1, length(ranges) - 1, 2)]
            end.mins <- ranges[seq(2, length(ranges), 2)]
            for (i in 1:length(start.mins)) {    
                cur.selection <- df$site == site & df$date == date & df$min >= start.mins[i] & df$min <= end.mins[i]    
                selection[cur.selection] <- TRUE 
            }  
        }
    }
 
    return(df[selection,])
}

 SplitTargetIntoDays <- function (target) {
     # takes the nested target list
     # and creates a bunch of lists with the same structure
     
     targets <- list()
     sites <- names(target)
     for (site in sites) {
         dates <- names(target[[site]])
         for (date in dates) {
             ranges <- target[[site]][[date]]
             l <- list()
             l[[site]] <- list()
             l[[site]][[date]] <- ranges
             targets[[length(targets) + 1]] <- l
         }
     }  
     
     return(targets)
     
 }


    

GetTargetDescription <- function (target) {
    sites <- names(target)
    sites.txt <- sites
    # first get all dates, to if they are all the same year or month
    all.dates <- c()
    for (site in sites) {
        all.dates <- c(all.dates, names(target[[site]]))
    }
    date.parts <- GetDateParts(all.dates)
    
    for (s in 1:length(sites)) {
        site <- sites[s]

        dates <- names(target[[site]])
        dates.txt <- dates
        for (d in 1:length(dates)) {
            date <- dates[d]
            dates.txt[d] <- paste(strsplit(dates.txt[d], "-")[[1]][date.parts$selector], collapse = "-")
            ranges <- target[[site]][[date]]
            if (any(ranges != c(0,1439))) {
                # if the ranges for this date are anything except all day, 
                # specify it, else ommit it since it is all day by default
                dates.txt[d] <- paste0(dates.txt[d], "(", paste(ranges, collapse = ","),")")
            }
        }
        
        sites.txt[s] <- paste(site, paste(dates.txt, collapse = ","), sep = ":")
    }
    d <- paste(sites.txt, collapse = ";")
    d <- paste(date.parts$prefix, d)
    return(d)  
}


GetDateParts <- function (dates) {
    # given a set of dates, will return a list with 2 elements
    # the part of the date that is the same (eg year, or year and month)
    # and the part of the date that is different as a vector of length length(dates)
    
    # split into a matrix of nrow = length(dates) and a column for year, month and day
    all.dates <- strsplit(dates, "-")
    dates.matrix <- matrix(NA, nrow = length(all.dates), ncol = 3)
    for (r in 1:length(all.dates)) {
        dates.matrix[r,1:3]  <- as.numeric(all.dates[[r]])
    }  
    # find the part that is the same among all dates
    same <- c(FALSE, FALSE, FALSE)
    for (i in 1:3) {
        if (abs(max(dates.matrix[,i]) - min(dates.matrix[,i])) < 0.25) {
            same[i] <- TRUE
        } else {
            break()
        }
    }
    prefix <- paste(dates.matrix[1,same], collapse = "-") 
    sig <- apply(dates.matrix, 1, function (row){ 
        paste(row[!same], collapse = "-")
        })
    return(list(prefix = prefix, dates = sig, selector = !same))
}


TargetListToDF <- function () {
    # converts a nested list of sites and dates and ranges to 
    # 
    
    
}





TargetSubset.old <- function (df) {
    # returns a subset of the dataframe, includes only rows that 
    # belong within sites, dates and minute ranges (1 or more pairs of start/end minutes of the day)
    # defined in config
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

AddMinuteIdCol.old <- function (data) {
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

AddMinuteIdCol <- function (data) {
    # given a data frame with the columns "date", "site", and either "min" or "start.sec", 
    # will look up the minute id for each row and add a column to the data frame
    #
    # Args:
    #   data: data.frame
    # 
    # Value:
    #   data.frame
    
    
    min.ids <- GetMinuteList()
    ids <- apply(as.matrix(data), 1, function (v) {
        is.null(v['min'])
        
        if (is.null(v['min'])) {
            min <- floor(as.numeric(v[sec.col]) / 60)
        } else {
            min <- as.numeric(v['min'])
        } 
        
        id <- min.ids$min.id[min.ids$site == v['site'] & min.ids$date == v['date'] & min.ids$min == min]
        
        
        return(id)  
    })
    data$min.id <- ids
    return(data) 
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

CreateTargetMinutesRandom.old <- function () {
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

