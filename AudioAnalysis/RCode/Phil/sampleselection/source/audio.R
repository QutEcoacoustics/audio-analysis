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





# used in 2 ok
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
    
    start <- as.POSIXlt(start.date) + start.sec
    end <- as.POSIXlt(start + duration)
    file.positions <- DateTimeToFn(site, start, end, ext = 'wav')
    wav.samples <- numeric()
    samp.rate <- NA
    bit <- NA
    for (i in 1:nrow(file.positions)) {   
        w <- readWave(file.positions$fn[i])
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

# removed because it's probably not needed with the new 1-min file setup
# GetFullPath <- function (fn, site, date) {
#     # returns the full path to an audio file, given the filename, site and date
#     audio.dir <- Path('audio')
#     path <- GetAnalysisOutputPath(as.character(site), as.character(date), audio.dir)$path
#     return(file.path(path, as.character(fn)))
# }


GetAudioFileBatch <- function (df) {
    # given the site, date and minute, 
    # returns the audio file
    # 
    # Args:
    #   df: data.frame. Must have the columns site, date and min
    #
    # Value:
    #   character vector
    
    res <- character(nrow(df))
    multiple = FALSE
    for (i in 1:nrow(df)) {
        res[i] <- GetAudioFile(df$site[i], df$date[i], df$min[i])
    }
    return(res)
    
}

GetAudioFile <- function (site, date, mins) {
    # for a given site, date and list of minutes
    # returns all the files needed 
    
    audio.dir <- Path('audio')
    fns <- sapply(mins, function (min) {
        return(AudioFn(site, date, min))
    })
    paths <- file.path(audio.dir, fns)
    return(paths)
}

GetAudioMeta <- function (path, val = NULL) {
    # for a given audio file, 
    # the meta data by reading the wave
    require('tuneR')
    w <- readWave(path)
    meta = list(
        stereo = w@stereo,
        bit = w@bit,
        samp.rate = w@samp.rate,
        duration = length(w@left) / w@samp.rate,
        pcm = w@pcm
    )
    if (is.character(val)) {
        return(meta[[val]])
    } else {
        return(meta)
    }
    return(w)
}




DateTimeToFn <- function (site, start.datetime = NA, end.datetime = NA, start.date = NA, start.min = NA, ext = FALSE) {
    # determines which file the 
    # recording at a given site and datetime (POSIXlt), 
    # and the number of seconds into the recording it is 
    # if the optional end.datetime is included, then all the files between start.datetime and end.datetime are included
    # alternative params are start date and start min
    #
    # Args
    #   site: string
    #   start.datetime: string
    #   end.datetime: string (optional)
    #   start.date: string
    #   start.min: int
    
    
    if (is.na(start.datetime)) {     
        start.datetime <- paste(start.date, MinToTime(start.min))      
    }
    
    start.datetime <- as.POSIXlt(start.datetime) 
    
    if (is.na(end.datetime)) {
        end.datetime <- start.datetime
    } else {
        end.datetime <- as.POSIXlt(end.datetime) 
    }

    target.diff <-  as.double(difftime(end.datetime, 
                                       start.datetime, 
                                       units = 'secs'))
    
    file.length <- 1 # in minutes

    distance.from.start.of.file <- DifferenceFromNearestXmins(start.datetime, file.length)
    num.files <- ceiling((target.diff + distance.from.start.of.file) / (file.length * 60))
    
    target.fns <- rep(NA, num.files)
    target.from.sec <- rep(0, num.files)
    target.to.sec <- rep(file.length * 60, num.files)
    
    target.from.sec[1] <- distance.from.start.of.file
    target.to.sec[num.files] <- (target.diff + distance.from.start.of.file) %% (file.length * 60)
    if (target.to.sec[num.files] == 0) {
        target.to.sec[num.files] <- (file.length * 60)
    }
    
    file.second.offsets <- 1:num.files * (file.length * 60) - (file.length * 60)
    # a vector of posixlt object at the start date time, the start date time plus file length (2nd file), start datetime plus 2 file lengths (3rd filename)
    file.start.datetimes <- file.second.offsets + start.datetime
    
    for (f in 1:length(file.start.datetimes)) {
        start.date <- strftime(file.start.datetimes[f],'%Y-%m-%d')
        start.min <-  as.numeric(strftime(file.start.datetimes[f], '%H')) * 60 + as.numeric(strftime(file.start.datetimes[f], '%M'))
        target.fns[f] <- GetAudioFile(site, start.date, start.min) 
    }
       
    return(data.frame(fn = target.fns, from.sec = target.from.sec, 
                      to.sec = target.to.sec, stringsAsFactors = FALSE))
    
    
}

DifferenceFromNearestXmins <- function (datetime, xmins = 10) {
    # finds how many seconds after the nearest 10 mins (eg 12:10:00, 13:20:00) datetime is
    datetime <- as.POSIXlt(datetime)
    m <- as.integer(strftime(datetime, '%M'))
    s <- as.numeric(strftime(datetime, '%OS3'))
    nearest.m <- floor(m/xmins) * xmins
    diff <- (m - nearest.m) * 60 + s
    return(diff)
}

FnToMeta <- function (filename) {
    # given a filename that adheres to the naming
    # produces an list of metadata about the file
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

MetaToFn <- function (meta, units = 'm', ext = FALSE) {
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
    start = sprintf(paste0("%0",start.str.len,"s"), as.character(round(start)));
    duration = as.character(round(duration));
    
    fn <- paste(meta$site, meta$date, start, duration, sep = '.')
    
    if (is.character(ext)) {
        fn <- paste(fn, ext, sep = '.')
    }
    
    return(fn)
    
            
}

FileNamesInTarget <- function (ext) {
    mins <- ReadOutput('target.min.ids', purpose = 'file names in target')
    
    site.col <- which('site' == colnames(mins))
    date.col <- which('date' == colnames(mins))
    min.col <- which('min' == colnames(mins))
    
    fns <- apply(mins, 1, function (row) {        
        site <- row[site.col]
        date <- row[date.col]
        min <- as.integer(row[min.col])
        fn <- DateTimeToFn(site, start.date = date, start.min = min, ext = ext)
        return(as.character(fn$fn))    
    })
    
    fns <- unique(fns)
    
    return(fns)
}

FixNames <- function () {
    # renames files so that start min is always 4 digits
    path <- g.audio.dir
    files <- list.files(g.audio.dir)
    for (f in files) {
        
        meta <- FnToMeta(f)
        fixed <- MetaToFn(meta, ext = 'wav')
        file.rename(file.path(g.audio.dir, f), file.path(g.audio.dir, fixed))
        
    }
}

ConvertToMins <- function (replace.existing = FALSE) {
    # converts audio from 10-min files to 1-min files
    # and renames them to site_date_min.wav
    # This was a 1-off function to save a new 1-min set of original audio to use as source
    
    require('stringr')


    destination <- '/Volumes/files/qut_data/SERF/serf_audio' 
    sites <- c('NE', 'NW', 'SE', 'SW')
    dates <- c('2010-10-13', '2010-10-14', '2010-10-15', '2010-10-16', '2010-10-17')
    mins <- 0:143
    
    # minimum number length of file
    # last file might be shorter than 1 minute
    min.length.for.file <- 1
    
    for(site in sites) {
        for (date in dates) {
            for (m10 in mins) {

                Dot();
                
                fns <- sapply(0:9, function (m) {
                    fn <- AudioFn(site, date, m10*10 + m) 
                    path <- file.path(destination, fn)
                    return(path)
                })
                
                if (!replace.existing && all(sapply(fns, file.exists))) {
                    # continue to next 10 min file
                    next
                }
                
                
                source <- GetAudioFile(site,date,m10*10)   
                audio <- readWave(source)
                
                for (m in 0:9) {
                    
                    temp <- audio
                    start.sample <- m*60*audio@samp.rate
                    end.sample <- start.sample + 60*audio@samp.rate
                    
                    # the last minute might be slightly short
                    if (end.sample > length(audio@left)) {
                        end.sample <- length(audio@left)
                        if (end.sample - start.sample < min.length.for.file * audio@samp.rate ) {
                            # just in case the 10 minute file terminates early (for example the last file before midnight)
                            break
                        }
                    }
 
                    
                    temp@left <- audio@left[start.sample:end.sample]
                    
                    writeWave(temp, fns[m+1])
                }
            }
        }
    }
}


AudioFn <- function (site, date, min) {
    require('stringr')
    return(paste0(paste(site, date, str_pad(min, 4, 'left', "0"), sep = "_"), '.wav'))
}



