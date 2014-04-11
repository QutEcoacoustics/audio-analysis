# This file contains functions to do with creating 
# wave files by stitching together or subsetting
# other wave files. It relies on the naming convention
# for the wave files, which is:
#
# site.date.start.duration.wav
# NW.2010_10_13.460.10.wav
# is a 10 min file from NW site on oct 13 2010, starting at 7:40 am
# the 'units' is either 'm', 's', or 'ms'. If ommited, then it 
# is assumed to be 'm'. m = mins, s = seconds, ms = milliseconds. 
#
# source files (i.e. those not created as part of 
# the analysis process)  must be in whole mins. 



Audio.Targeted <- function (site, start.date, start.sec, duration, save = FALSE) {
    # use extractWave from TuneR to take a subset of a wave 
    #
    # Args:
    #   site: string; 
    #   start.date: string or date object
    #   start.sec: float; the number of seconds from the start of start.date
    #   duration: float; the number of seconds for the spectrogram
    #
    # Details:
    # this is the process:
    # 1. identify the file start
    # 2. identify whether the target spans more than 1 file
    # 3. idenify the sample number for the start and end
    
    require('tuneR')
    
    start <- as.POSIXlt(start.date, tz = "GMT") + start.sec
    end <- as.POSIXlt(start + duration, tz = "GMT")
    file.positions <- DateTimeToFn(site, start, end)
    wav.samples <- numeric()
    samp.rate <- NA
    bit <- NA
    for (i in 1:nrow(file.positions)) {   
        w <- readWave(file.path(g.audio.dir, file.positions$fn[i]))
        if (is.na(samp.rate) && is.na(bit)) {
            samp.rate <- w@samp.rate
            bit <- w@bit
        } else if (samp.rate != w@samp.rate || bit != w@bit ) {
            stop(paste('requested segment spans multiple files of',
                       'different sample rates and/or bit resolutions'))    
        }
        from.sample <- file.positions$from.sec[i] * samp.rate
        to.sample <- file.positions$to.sec[i] * samp.rate
        sub.wave <- extractWave(w, from = from.sample, to = to.sample)
        wav.samples <- c(wav.samples, sub.wave@left)
     
    }
    w <- Wave(left = wav.samples, right = numeric(0), 
              samp.rate = samp.rate, bit = bit)
    
    return(w)
    
} 


DateTimeToFn <- function (site, start.datetime, end.datetime) {
    # determines which file the 
    # recording at a given site and datetime (POSIXlt), 
    # and the number of seconds into the recording it is   
    start.datetime <- as.POSIXct(start.datetime, tz = "GMT")  
    end.datetime <- as.POSIXct(end.datetime, tz = "GMT") 
    target.diff <-  as.double(difftime(start.datetime, 
                                       end.datetime, 
                                       units = 'secs'))
    cur.start.datetime <- start.datetime
    files <- list.files(g.audio.dir)
    target.fns <- character()
    target.from.sec <- numeric()
    target.to.sec <- numeric()
    
    for (i in 1:length(files)) {
        fn <- unlist(strsplit(files[i], '.', fixed = TRUE))    
        fn.site <- fn[1]
        if (site != fn.site) {
            next()
        }
        fn.date <- as.Date(fn[2], format = "%Y_%m_%d")
        fn.min <- as.numeric(fn[3])
        fn.duration <- as.numeric(fn[4]) * 60
        fn.datetime <- as.POSIXct(paste0(format(fn.date, "%Y-%m-%d"), 
                                         " ", 
                                         MinToTime(fn.min)), 
                                  tz = "GMT")
        
        #cur start datetime minus file start datetime in seconds
        diff <- as.double(difftime(cur.start.datetime, 
                                   fn.datetime, units = 'secs'))
        if (diff < fn.duration && diff >= 0) {
            
            file.start.at <- diff
            file.end.at <- as.double(difftime(end.datetime, 
                                              cur.start.datetime, 
                                              units = 'secs')) + file.start.at
            if (file.end.at > fn.duration) {
                # if the end datetime is not within the same 
                # file as the start datetime set the end point for 
                # this file to the end of the file
                file.end.at <- fn.duration
                cur.start.datetime <- cur.start.datetime + (file.end.at - file.start.at)
                brk <- FALSE
            } else {
                brk <- TRUE
            }
            
            target.fns <- c(target.fns, files[i])
            target.from.sec <- c(target.from.sec, file.start.at)
            target.to.sec <- c(target.to.sec, file.end.at)
            
            if (brk) {
                break()
            }
            
            
        } 
    }
    
    return(data.frame(fn = target.fns, from.sec = target.from.sec, 
                      to.sec = target.to.sec))
    
    
}


FnToMeta <- function (filename) {
    # given a filename that adheres to the naming
    # products an list of metadata about the file
    #
    # Args:
    #   filename: String
    #
    # Returns:
    #   list
    #
    # Details:
    #   output will have the following items
    #   site: string
    #   start.date: string
    #   start.min: mins from the start of start.date
    #   start.sec: seconds from start of start.date
    #   duration.mins: duration in minutes 
    #   duration.secs: duration in seconds
      


    filename.parts <- unlist(strsplit(filename, '.', fixed=TRUE))
    site <- filename.parts[1]
    date <- FixDate(filename.parts[2])
    file.start <- as.numeric(filename.parts[3])
    file.duration <- as.numeric(filename.parts[4])
    
    if (length(filename.parts) > 4) {
        units <- filename.parts[5]
        if (units != "s" && units != "ms") {
            units <- "m";
        }
    }
    
    if (units == 'm') {
        start.min = as.numeric(file.start)
        start.sec = as.numeric(file.start) * 60
        duration.mins = file.duration
        duration.secs = file.duration * 60
    } else if (units == 's') {
        start.min = as.numeric(file.start) / 60
        start.sec = as.numeric(file.start)
        duration.mins = file.duration / 60
        duration.secs = file.duration
    } else if (units == 'ms') {
        start.min = as.numeric(file.start) / 60000
        start.sec = as.numeric(file.start) / 1000
        duration.mins = file.duration / 60000
        duration.secs = file.duration / 1000
    }
    
    meta <- list(
        site = site,
        date = date,
        start.min = start.min,
        start.sec = start.sec,
        duration.mins =  duration.mins,
        duration.secs = duration.secs
        )
    
    class(meta) = "audio.meta"
    
    return(meta);
    
}

MetaToFn <- function (meta, units = 'm') {
    # reverse of MetaToFn (see that function for explanation)   
    #
    # Args:
    #   meta: list
    #
    # Returns:
    #   string
    #
    # Details:
    #   meta should be a list with the following
    #   site: string
    #   start.date: string
    #   start.min: mins from the start of start.date
    #   start.sec: seconds from start of start.date
    #   duration.mins: duration in minutes 
    #   duration.secs: duration in seconds
    #
    #   only one of start.min or start.sec is required
    #   only one of duration.mins or duration.secs is requried
    
    # fill in any blanks
    if (is.null(meta$duration.mins)) {
        meta$duration.mins = meta$duration.secs / 60
    } else if (is.null(meta$duration.secs)) {
        meta$duration.secs = meta$duration.mins * 60 
    }
    
    if (is.null(meta$start.min)) {
        meta$start.min = meta$start.sec / 60
    } else if (is.null(meta$start.sec)) {
        meta$start.sec = meta$start.min * 60 
    }

    # determine the units that should be used.
    
    units <- match(units, c('m','s','ms'));
    if (is.na(units)) {
        stop("Invalid units argument. must be 'm', 's' or 'ms'");
    }
    if (is.wholenumber(meta$start.min) && 
            is.wholenumber(meta$duration.mins) && 
            units <= 1) {
        duration <- meta$duration.mins
        start <- meta$start.min
        units <- 'm'
        start.str.len <- 4 # maximum val is 1440
    } else if (is.wholenumber(meta$start.sec) && 
                is.wholenumber(meta$duration.secs) && 
                units <= 1) {
            duration = meta$duration.secs
            start <- meta$start.sec
            units <- 's'
            start.str.len <- 5  # max value 86400
    } else {
        duration = round(meta$duration.secs * 1000)
        start = round(meta$start.sec * 1000)
        units <- 'ms'
        start.str.len <- 8 # max is 86400000
    }
    
    # convert to strings and pad start so that files are listed in order
    start = sprintf(paste0("%",start.str.len,"s"), as.character(round(start)));
    duration = as.character(round(duration));
    
    fn <- paste(meta$site, meta$date, meta$start)
    
            
}

