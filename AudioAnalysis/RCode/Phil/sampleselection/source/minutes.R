# Contains functions for creating lists of minutes for use in sample selections
# as well as some functions to do with time



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

GetTargetMinutesByDay <- function (site, date, version.only = TRUE) {
    # gets the list of target minutes for the specified site and date
    #
    # Args:
    #   site: string
    #   date: string yyyy-mm-dd
    #   version.only: boolean; if true, returns an int which is the 'target.min.ids' version
    #                          if false, returns the read output including the data.frame of target.min.ids#
    
    target <- list()
    target[site] <- list()
    target[[site]][[date]] <- c(0, 1439)
    
    target.description <- GetTargetDescription(target)
    params <- list('target' = target.description)
    target.minutes <- ReadOutput('target.min.ids', params = params)
    
    if (version.only) {
        version <- target.minutes$version
        return(version)   
    } else {
        return(target.minutes)
    }
    
}





CreateTargetMinutes <- function (target = NULL) {
    # creates a list of target minute ids, based on
    # the target nested list in config
    # writes the output to csv 
    
    if (is.null(target)) {
        target <- g.target
    }
    
    # generates a full list of minute ids for the study
    study.min.list <- GetMinuteList()
    
    target.mins <- TargetSubset(study.min.list, target)
    
    if (g.percent.of.target < 100) { 
        num.to.include <- floor(nrow(target.mins)*g.percent.of.target/100)
        to.include <- GetIncluded(total.num = nrow(target.mins), num.included = num.to.include, offset = 0)
        target.min.ids <- target.mins$min.id[to.include]
    }
    
    params <- list(target = GetTargetDescription(target))
    datatrack::WriteDataobject(target.mins, 'target.min.ids', params = params)
    
}

TargetSubset <- function (df, target) {
    # returns a subset of the dataframe, includes only rows that 
    # belong within sites, dates and minute ranges (1 or more pairs of start/end minutes of the day)
    # using the target nested list
    # defined in config
    #
    # Args:
    #   df: data.frame; must have the columns site, date, min
    #   target: list; which minutes to select. 
    # 
    # Value
    #   data.frame
    #
    # Details 
    #   target parameter is a list where
    #   - The names are sites and the values are lists. 
    #   - The names of these lists are dates and the values are integer vectors. 
    #   - The integer vectors are the start and end minutes. 
    #   For example, the following is NE 2017-10-13 between 123 and 234 and between 345 and 456
    #                     example.target = list('NE' = list(
    #                        '2017-10-13' = [123,234,345,456]
    #                     ))
    
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



GetTargetDescription   <- function (target) {
    # Converts a target from a nested list to a compact textual representation 
    #
    # Args:
    #   target: list; names of top level are sites, values are lists of dates
    #                 names of 2nd level are dates, values are minutes to include 
    #                 eg c(0, 13, 29, 34) will include minutes 0-13 and 29-34
    # 
    
    target.df <- TargetToDf(target)
    
    return(DfToDesc(target.df))
    
    
}

TargetToDf <- function (target) {
    df <- data.frame(site = character(), year = numeric(), month = numeric(), day = numeric(), mins = character())
    sites <- names(target)
    for (site in sites) {
        dates <- names(target[[site]])
        for (date in dates) {
            mins.txt <- paste0("(", paste(target[[site]][[date]], collapse = ","),")")
            date.vals <- unlist(strsplit(date, "-"))
            row <- data.frame(site = site, year = as.numeric(date.vals[1]), month = as.numeric(date.vals[2]), day = as.numeric(date.vals[3]), mins = mins.txt)
            df <- rbind(df, row)
        }
    }
    return(df)
}

DfToDesc <- function (df) {
    # given a data frame of exact minutes and sites with the columns
    # year, month, site, day, mins
    # e.g. year month site day     mins
    #   1  2010    10   NE  12 (0,1439)
    # transforms it into a concise unambiguous string that describes the ranges
    # eg "2010-10NE12,15-17,NW,SE,SW13-17,-11NE14"
    # 
    # Details: 
    #   there is one row of the dataframe for each individual day in the study
    #   and the minutes to include are in the format "(start.range,end.range)"
    #   the returned string will have the years each of which followed by the months 
    #   in that year, each of which followed by the sites in that month, each of which followed by the days in that site
    #   to avoid ambiguity, year is any number is 4 digits, month is preceded by a dash, sites are characters, 
    #   but if they start or end with an integer will be surrounded by square brackets
    #   

    
    # todo: check the colnames are right
    
    df <- data.frame(lapply(df, as.character), stringsAsFactors=FALSE)
    
    # remove minutes 0-1439 because that is all day so default
    df$mins[df$mins == '(0,1439)'] <- ''
    
    # sort correctly
    df <- df[with(df, order(year, month, site, day, mins)), ]
    
    # add a dash before month number
    df$month <- paste0('-', df$month)
    
    # sites that start or end with a number will produce an ambigous string, 
    # so put them in square brackets []
    site.with.num <- grep('^[0-9]|[0-9]$', df$site)
    df$site[site.with.num] <- paste0('[', df$site[site.with.num], ']')
    
    
    cols <- c('year', 'month', 'site', 'day', 'mins')
    
    for (cur.col in 4:1) {
        
        group.by <- cols[cur.col]
        merge <- cols[cur.col:(cur.col+1)]
        df <-GroupBy(df, group.by, merge)
        df <- GroupBy(df, group.by)
    }
    
    
    return(df)
    
    
    
    
}

GroupBy <- function (df, group.by, merge = NULL) {
        
    # the total number of rows will be reduced to the unique combinations of everything to the left
    # of the group.by.col number
    
    if (ncol(df) == 1) {
        return(paste0(as.character(df[,group.by]), collapse=','))
    }
    
    
    keep.cols <- colnames(df)[colnames(df) != group.by]
    u <- unique(df[colnames(df) != group.by])
    grouped = rep(NA, nrow(u))
    for (row in 1:nrow(u)) {
        if (length(keep.cols) > 1) {
            subset <- merge(df, u[row,], by = keep.cols)
            to.group <- subset[,group.by]
        } else {
            to.group <- df[df[keep.cols] == u[row,keep.cols],group.by]
        }
        
        
        grouped[row] <- MakeGroup(to.group)
    }
    u[group.by] <- grouped
    if (!is.null(merge)) {
        merged <- paste0(u[,merge[1]], u[,merge[2]])
        u <- u[!colnames(u) %in% merge]
        u[merge[1]] <- merged
    }
    
    
    
    return(u)
}


MakeGroup <- function (items, sort = TRUE) {
    # if the items are non numerical, pastes them together with separator
    # if they are integers, replaces 3 or more consecutive numbers with the first and last separated by a :
    
    
    if (length(items) == 1) {
        return(items)
    } else if (sort) {
        items <- items[order(items)] 
    }
    
    # check if they are numeric
    if (length(grep('^[0-9]+$', as.character(items))) == length(items)) {
        items <- AbbreviateConsecutive(as.numeric(items))
    }
    
    return(paste0(items, collapse = ','))
}

AbbreviateConsecutive <- function (nums, intra.group.separator = '-', inter.group.separator = NULL) {
    # converts a sequency of numbers from something like this 1,2,3,4,6,7,8,10,11
    # to this "1-4,6-8,10,11" or ["1-4","6-8","10","11"]
    #
    # Args: 
    #   nums: integers
    #   intra.group.separator: string; the string to separate the first and last numbers of a run of consecutive numbers
    #   inter.group.separator: string; optional; if supplied, will return a string with each group of consecutive numbers separated by this
    #                                            if ommited, will return a vector of strings, with each element representing a group of consecutive numbers
    
    if (length(nums) == 1) {
        return(nums)
    }
    groups <- list()
    cur.group <- 1
    groups[[cur.group]] <- nums[1]
    for (i in 2:length(nums)) {
        
        if (nums[i] - nums[i-1] > 1) {
            cur.group <- cur.group + 1
            groups[[cur.group]] <- nums[i]
        } else {
            groups[[cur.group]] <- c(groups[[cur.group]], nums[i])
        }
    }
    grouped <- lapply(groups, function (x) {
        if (length(x) > 2) {
            return(paste0(x[1],intra.group.separator,x[length(x)])) 
        } else if (length(x) == 2) {
            return(paste0(x[1],',',x[length(x)]))
        } else {
            return(x[1])
        }
    })
    
    if (is.null(inter.group.separator)) {
        return(as.character(grouped))
    } else {
        return(paste0(grouped, collapse = inter.group.separator))
    }
    
}



#     
#     # now we have a data frame with all the site year month day mins
#     # to best group, figure out which is 
#     # group by year
#     years <- unique(df$year)
#     for (y in years) {
#         cur.year <- df[df$year == y,]
#         months <- unique(cur.year$month)
#         for (m in months) {
#             cur.month <- cur.year[cur.year$month == m]
#         }
#     }
#     
# DfToDesc.recursive.1 <- function (df) {
#     groups <- unique(df[,1])
#     to.return <- list(vals = )
#     for (g in groups) {
#         selection <- df[df[,1] == g,]
#         if (ncol(df) == 1) {
#             # we are looking at the mins
#             if (selection == '(0,1439)') {
#                 return('')
#             } else {
#                 return(selection)
#             }
#         } else {
#             new.df <- df[,2:(ncol(df))]
#         }
#     }
# }


GetMinlistDescription <- function (min.list) {
    
    return(GetTargetDescription(GetMinlistSummary(min.list)))
    
}

GetMinlistSummary <- function (min.list) {
    # given a dataframe of minutes with the columns site, date, min
    # returns a textual description
    
    sites <- unique(min.list$site)
    summary <- list()
    for (s in sites) {
        summary[[s]] <- list()
        dates <- unique(min.list$date[min.list$site == s])
        for (d in dates) {
            mins <- RemoveConsecutive(min.list$min[min.list$site == s & min.list$date == d])
            summary[[s]][[d]] <- mins
        }
    }
    return(summary)
}

RemoveConsecutive <- function (nums) {
    upper.bounds <- abs(nums[1:(length(nums) - 1)] - nums[2:(length(nums))]) != 1
    lower.bounds <- which(upper.bounds) + 1
    upper.bounds[lower.bounds] <- TRUE
    upper.bounds[1] <- TRUE
    return(nums[upper.bounds])
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
        start.sec.col <- which(colnames(events) ==  start.sec.col)     
    }
    min <- apply(events, 1, function (v) {
        sec <- as.numeric(unlist(v[start.sec.col]))
        min <- floor(sec / 60)
        return (min)
    })
    new.events <- cbind(events, min)
    return (new.events)
    
}

CreateTargetMinutesRandom.old <- function () {
    # randomly selects a subset of the target minutes
    # abandoned because it is non-deterministic and was making 
    # it difficult to save output based on minute selection
    # now use deterministic function CreateTargetMinutes
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

GetTargetDescription.old <- function (target) {
    # Converts a target from a nested list to a compact textual representation 
    #
    # Args:
    #   target: list; names of top level are sites, values are lists of dates
    #                 names of 2nd level are dates, values are minutes to include 
    #                 eg c(0, 13, 29, 34) will include minutes 0-13 and 29-34
    # 
    
    
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

