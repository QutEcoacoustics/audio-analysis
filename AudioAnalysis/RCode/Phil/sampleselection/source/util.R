# utility functions which are not specific to any particular step 
# (or which might be used by other steps in the future)



# TODO: check fix this so that it makes sense given that the bottom 
#      frequency is the top row
SliceStft <- function (bounds, spectro, get.bounds = FALSE) {
    # Extracts the stft values within a given bounding box from 
    # a longer stft matrix
    #
    # Args:
    #   bounds: vector; contains 4 numeric values:
    #           start.time, duration, bottom.f, top.f
    #           - start.time: number of seconds from the start of the 
    #                         stft matrix to begin the slice
    #           - duration: number of seconds to include in the slice
    #           - bottom.f: lower frequency bound in Hz
    #           - top.f: upper frequency bount in Hz
    #   spectro: spectrogram; what to take the slice from
    #   get.bounds: boolean; whether to return a matrix of values (false) 
    #                        or just the row and column numbers
    #
    # Returns:
    #   Mixed: either matrix of values or list of 
    #          bounds [left.col, right.col, top.row, bottom.row]

    
    start.time <- bounds[1]
    duration <- bounds[2]
    bottom.f <- bounds[3]
    top.f <- bounds[4]
    
    
    # set bottom and top frequncy to FALSE to use the whole frequency range
    if (bottom.f == FALSE) {
        bottom.f <- 0
    }
    if (top.f == FALSE) {
        top.f <- spectro$frequency.range
    }
    
    
    
    left.col <- TimeToColNum(start.time, spectro$frames.per.sec)
    right.col <- TimeToColNum(start.time + duration, spectro$frames.per.sec)
    top.row <- FrequencyToRowNum(top.f, spectro$hz.per.bin)
    bottom.row <- FrequencyToRowNum(bottom.f, spectro$hz.per.bin)
    if (get.bounds) {
        return(list(left.col = left.col, 
                    right.col = right.col, 
                    top.row = top.row, 
                    bottom.row = bottom.row))
    } else {
        # slice the part out and return the sub-matrix
        sub <- spectro$vals[bottom.row:top.row, left.col:right.col]

        return(sub)	
    }
}

VectorFluctuation <- function (v, relative = TRUE) {
    # Returns the sum of the difference between consecutive values in a vector
    #
    # Args
    #    v: vector
    #    relative: boolean; whether to return the total or the mean. 
    #
    # Returns: 
    #   number;
    #
    # Details:
    #   eg. [1, 5, 2, 9] would return 14 (4 + 3 + 7)
    
    v1 <- v[1:(length(v) - 1)]
    v2 <- v[2:(length(v))]
    diff <- abs(v1 - v2)
    fluctuation <- sum(diff)
    
    if (relative) {
        fluctuation <- mean(diff) 
    } else {
        fluctuation <- sum(diff)
    }
    
    return(fluctuation)  
}

# this whole function is crap
FindCenter <- function (v) {
    # Finds the index of a vector so that the values on each side 
    # add up to the same. Returns a float, which shows the proportion of 
    # the central value that belongs on each side
    #
    # Args:
    #   v: vector of numbers
    # 
    # Returns:
    #   float;
    #
    # Details:
    #   examples:
    #   v = c(1,1,1,1,1) returns 3. Same total (2) both sides of index 3
    #   v = c(1,1,1,1,1,1) returns 3.5, 3.5 is halfway between 3 and 4. 
    #   There is the same total above and below halfway between index 3 and 4
    #   v = c(c(1,2,3,4,5,6,7,8)) returns 6. Same total (15) both sides of index 6
    #   v = c(1,2,3,4,5,6,7) returns 5.3. below index 5 adds to 10, but below index 6
    #   adds up to 15, which is more than half the total. The centre point is 
    #   between 5 and 6. 
    
    # special case, all the values are the same
    if (min(v) == max(v)) {
        return((length(v) + 1) / 2);
    }
    
    
    # make the minimum value zero
    # this is necessary to avoid problems with negative values
    v <- v - min(v);
    
    
    len <- length(v)
    l <- 1
    r <- len
    total.l <- 0
    half.total <- sum(v) / 2
    
    for (i in (1:len)) {
        new <- total.l + v[i]
        if (new > half.total) {
            center.point <- i
            fraction <- (half.total - total.l) / v[i]
            return (center.point + fraction - 0.5)
        }
        total.l <- new
    }
    
    return(FALSE)
    
}

Normalize <- function (v) {
    # Changes the values of a vector so that they lie 
    # between zero and 1 inclusive
    #
    # Args:
    #    v: vector or matrix (numeric)
    # 
    # Returns:
    #    vector (numeric)
    # 
    # Details:
    #   The lowest value will become zero and the largest value  
    #   will become one, with the other values remaining the same 
    #   relative distance from the max and min. If all the values 
    #  are the same in v, the returned vector will be all zeros
    #
    # TODO:
    #   Check whether there is a better built in way to do this
    
    
    # make the minimum value zero
    v <- v - min(v)
    
    #make the maximum value 1, keeping the min value zero
    max.val <- max(v)
    if (max.val != 0) {
        v <- v / max.val
    } else {
        v <- 0.5
    }
    
    return(v)
    
    
}

FrequencyToRowNum <- function (f, hz.per.bin) {
    #  works out the row number of a stft matrix of a given frequency
    #  assumes top of matrix (row 1) = low frequency
    #
    #  Args: 
    #      f: int; frequency in Hz
    #      hz.per.bin: int; the number of Hz per frequency bin
    #
    #  Returns: 
    #      int; the row number
    
    
    row.num <- round(f / hz.per.bin)
    return(row.num)
}

TimeToColNum <- function (t, frames.per.second) {
    # returns out the column number of an stft matrix at 
    # a given number of seconds from the start
    # 
    # Args:
    #   t: float; the number of seconds
    #   frames.per.second: float; the number of time frames 
    #                             for each second of audio
    #
    # Returns:
    #   Int
    
    col.num <- round(frames.per.second * t)
    return(col.num)
}

# temp to fix the stuffup i made with the filenames
RenameMatlabOutput <- function () {
    
    dir <- 'matlaboutput/'
    rem <- 'matlaboutput'
    files <- list.files(path = dir, full.names = FALSE)
    
    for (i in 1:length(files)) {
        sub <- substring(files[i], 1, nchar(rem))
        if (sub == rem) {
            new <- gsub(rem, "", files[i])
            old.path <- paste0(dir, files[i])
            new.path <- paste0(dir, new)
            file.rename(old.path, new.path)
        }
        
    }
}

SecToTime <- function (sec, decimals = 2,  midnight.is.1st.min = FALSE) {
    #given a second number, returns a string which is the time of day
    # eg 322 returns 5:21 because 5:21am is the 322min minute of the day
    # eg 1 returns 00:00:00 because that is the 1st minute of the day
    sec <- as.numeric(sec)
    if (midnight.is.1st.min) {
        min <- min - 60
    }
    h <- floor(sec / 3600)
    m <- floor(sec / 60) - h * 60
    s1 <- round(sec - (h * 3600 + m * 60), digits = decimals)
    s <- floor(s1)
    cs <- round((s1 - s)*100)
    h <- sprintf("%02d", h)
    m <- sprintf("%02d", m)
    s <- sprintf("%02s", s)
    cs <- sprintf("%02s", cs)
    return(paste(h, m, paste(s, cs, sep = "."), sep = ':'))
}

 

MinToTime <- function (min, midnight.is.1st.min = FALSE) {
    #given a minute number, returns a string which is the time of day
    # eg 322 returns 5:21 because 5:21am is the 322min minute of the day
    # eg 1 returns 00:00:00 because that is the 1st minute of the day
    
    if (midnight.is.1st.min) {
        min <- min - 1
    }
    h <- floor(min / 60)
    m <- min - h * 60
    h <- sprintf("%02d", h)
    m <- sprintf("%02d", m)
    return(paste(h, m, '00', sep = ':'))
}

TimeToMin <- function (time = NULL, hour = NULL, min = NULL, 
                       sec = 0, midnight.is.1st.min = FALSE) {
    # given either a time string in the form hh:mm:ss or hh:mm, 
    # or the hour and minute, finds the minute number of the day
    #
    # Args:
    #   time: string; eg 14:03:22 or 04:40
    #   hour: int;
    #   min: int;
    #   sec: int
    #   midnight.is.first.min: boolean; 
    #     If true, a value between 00:00:00 and 00:00:59 will return 1
    #     and the maximum value in the day will be 1439 
    #     (i.e. minute number 1440 is minute number 1 of the next day)
    #
    # Returns:
    #   Int;
    
    if (!is.null(time)) {
        parts <- unlist(strsplit(time, ':', fixed=TRUE))
        hour <- as.integer(parts[1])
        min <- as.integer(parts[2])
        sec <- 0
    }
    
    min <- hour * 60 + min + round(s / 60)
    if (midnight.is.1st.min) {
        min <- min + 1
    }
    return(min)
    
}

FixDate <- function (date) {
    require('stringr')
    #takes a date string which may be written in a different format
    # and reformats it to be consistent
    return(str_replace_all(date, "[^[:digit:]]", '-'))
}

is.wholenumber <- function(x, tol = .Machine$double.eps^0.5) {
    # check if a number is a whole number
    # copied from the help file for is.integer
    abs(x - round(x)) < tol
} 
