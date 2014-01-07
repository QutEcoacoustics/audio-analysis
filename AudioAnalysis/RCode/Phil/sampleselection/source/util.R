# utility functions which are not specific to any particular step 
# (or which might be used by other steps in the future)



#TODO: check fix this so that it makes sense given that the bottom frequency is the top row
SliceStft <- function (bounds, spectro, get.bounds = FALSE) {
    # extracts the stft values within a given bounding box from a longer stft matrix
    #
    # Args:
    #   bounds: vector; contains 4 numeric values:
    #           start.time, duration, bottom.f, top.f
    #     - start.time: number of seconds from the start of the stft matrix to begin the slice
    #     - duration: number of seconds to include in the slice
    #     - bottom.f: lower frequency bound in Hz
    #     - top.f: upper frequency bount in Hz
    #    spectro: spectrogram; what to take the slice from
    #    get.bounds: boolean; whether to return a matrix of values (false) or just the row and column numbers
    #
    # Returns:
    #    Mixed: either matrix of values or list of bounds [left.col, right.col, top.row, bottom.row]
    
    
    #print(spectro);
    
    start.time <- bounds[1];
    duration <- bounds[2];
    bottom.f <- bounds[3];
    top.f <- bounds[4];
    
    
    # set bottom and top frequncy to FALSE to use the whole frequency range
    if (bottom.f == FALSE) {
        bottom.f <- 0;
    }
    if (top.f == FALSE) {
        top.f <- spectro$frequency.range;
    }

    
    
    left.col <- TimeToColNum(start.time, spectro$frames.per.sec);
    right.col <- TimeToColNum(start.time + duration, spectro$frames.per.sec);
    top.row <- FrequencyToRowNum(top.f, spectro$hz.per.bin);
    bottom.row <- FrequencyToRowNum(bottom.f, spectro$hz.per.bin);
    if (get.bounds) {
        return(list(left.col = left.col, right.col = right.col, top.row = top.row, bottom.row = bottom.row));
    } else {
        # slice the part out and return the sub-matrix
        sub <- spectro$vals[bottom.row:top.row, left.col:right.col];
        #write.table(spectro$vals,file="big.csv", sep=",");
        #write.table(sub,file="small.csv", sep=",");
        return(sub);	
    }
}

VectorFluctuation <- function (v, relative = TRUE) {
  # return the sum of the different between consecutive values in a vector
  #
  # eg. [1, 5, 2, 9] would return 14 (4 + 3 + 7)
  # Args
  #    v: vector
  
  v1 <- v[1:(length(v)-1)]
  v2 <- v[2:(length(v))]
  diff <- abs(v1-v2);
  fluctuation <- sum(diff);
  
  if (relative) {
    fluctuation <- mean(diff); 
  } else {
    fluctuation <- sum(diff);
  }
  
  return(fluctuation);
  
  
}

FindCenter <- function (v) {
  # finds the index of a vector so that the values on each side add up to the same
  # returns a float, which shows the proportion of the central value that belongs on each side
  #
  # Args:
  #    v: vector of numbers
  # 
  # Returns:
  #    float;
  #
  #  TODO: explain this better with examples
  
  
  v <- NormalizeVector(v);

  
  len <- length(v);
  l <- 1;
  r <- len;
  total.l <- 0;
  half.total <- sum(v) / 2;
  
  for (i in (1:len)) {
    new <- total.l + v[i];
    if (new > half.total) {
        center.point <- i;
        fraction <- (half.total - total.l) / v[i];
        return (center.point + fraction - 0.5);
    }
    total.l <- new;
  }
  
  return(FALSE);
  
}

NormalizeVector <- function (v) {
  
  #changes the values of a vector so that they lie between zero and 1 inclusive
  # the lowest value will become zero and the largest value will become one, 
  # with the other values remaining the same relative distance from the max and min
  # if all the values are the same in v, the returned vector will be all zeros
  #
  # Args:
  #    v: vector (numeric)
  # 
  # Returns:
  #    vector (numeric)
  
  
  # make the minimum value zero
  v <- v - min(v);

  #make the maximum value 1, keeping the min value zero
  max.val <- max(v);
  if (max.val != 0) {
    v <- v / max.val;
  }
  
  return(v);
  
  
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
  
  
  row.num <- round(f/hz.per.bin);
  return(row.num);
}

TimeToColNum <- function (t, frames.per.second) {


  #  returns out the column number of an stft matrix at a given number of seconds from the start
  col.num <- round(frames.per.second * t);
  return(col.num);
}

# temp to fix the stuffup i made with the filenames
RenameMatlabOutput <- function () {
  
  dir <- 'matlaboutput/';
  rem <- 'matlaboutput';
  files <- list.files(path = dir, full.names = FALSE);

  for (i in 1:length(files)) {
    sub <- substring(files[i], 1, nchar(rem));
    if (sub == rem) {
      new <- gsub(rem, "", files[i]);
      old.path <- paste0(dir, files[i]);
      new.path <- paste0(dir, new);
      file.rename(old.path, new.path);
    }
    
  }
  
  
  
  
}

MinToTime <- function (min, midnight.is.1st.min = FALSE) {
  #given a minute number, returns a string which is the time of day
  # eg 322 returns 5:21 because 5:21am is the 322min minute of the day
  # eg 1 returns 00:00:00 because that is the 1st minute of the day
  
  if (midnight.is.1st.min) {
    min = min-1;
  }
  h <- floor(min/60);
  m <- min - h*60;
  return(paste(sprintf("%02d", h), sprintf("%02d", m), '00', sep = ':'));
}

TimeToMin <- function (time = NULL, hour = NULL, min = NULL, sec = 0, midnight.is.1st.min = FALSE) {
  # given either a time string in the form hh:mm:ss or hh:mm, or the hour and minute
  # returns the minute number of the day
  # Args:
  #   time: string; eg 14:03:22 or 04:40
  #   hour: int;
  #   min: int;
  #   sec: int
  #   midnight.is.first.min: boolean; If true, a value between 00:00:00 and 00:00:59 will return 1
  #                                   and the maximum value in the day will be 1439 
  #                                   (i.e. minute number 1440 is minute number 1 of the next day)
  
  if (!is.null(time)) {
    parts <- unlist(strsplit(time, ':', fixed=TRUE));
    hour <- as.integer(parts[1]);
    min <- as.integer(parts[2]);
    sec <- 0;
  }
  
  min <- hour*60 + min + round(s/60);
  if (midnight.is.1st.min) {
    min = min+1;
  }
  return(min);
  
}

FixDate <- function (date) {
    require('stringr');
    #takes a date string which may be written in a different format
    # and reformats it to be consistent
    return(str_replace_all(date, "[^[:digit:]]", '-'));
}

DateTimeToFn <- function (site, start.datetime, end.datetime) {
    # determines which file the 
    # recording at a given site and datetime (POSIXlt), 
    # and the number of seconds into the recording it is   
    start.datetime <- as.POSIXct(start.datetime, tz = "GMT");  
    end.datetime <- as.POSIXct(end.datetime, tz = "GMT"); 
    target.diff <-  as.double(difftime(start.datetime, end.datetime, units = 'secs'));
    cur.start.datetime <- start.datetime;
    files <- list.files(g.audio.dir);
    target.fns = character();
    target.from.sec = numeric();
    target.to.sec = numeric();

    for (i in 1:length(files)) {
        fn <- unlist(strsplit(files[i], '.', fixed = TRUE));    
        fn.site <- fn[1];
        if (site != fn.site) {
            next();
        }
        fn.date <- as.Date(fn[2], format = "%Y_%m_%d");
        fn.min <- as.numeric(fn[3])
        fn.duration <- as.numeric(fn[4]) * 60;
        fn.datetime <- as.POSIXct(paste0(format(fn.date, "%Y-%m-%d"), " ", MinToTime(fn.min)), tz = "GMT");
        
        #cur start datetime minus file start datetime in seconds
        diff <- as.double(difftime(cur.start.datetime, fn.datetime, units = 'secs'))
        if (diff < fn.duration && diff >= 0) {
            
            file.start.at <- diff;
            file.end.at <- as.double(difftime(end.datetime, cur.start.datetime, units = 'secs')) + file.start.at;
            if (file.end.at > fn.duration) {
                # if the end datetime is not within the same file as the start datetime
                # set the end point for this file to the end of the file
                file.end.at = fn.duration;
                cur.start.datetime <- cur.start.datetime + (file.end.at - file.start.at);
                brk = FALSE;
            } else {
                brk = TRUE;
            }
            
            target.fns = c(target.fns, files[i]);
            target.from.sec = c(target.from.sec, file.start.at);
            target.to.sec = c(target.to.sec, file.end.at);
            
            if (brk) {
                break();
            }
            
            
        } 
    }
    
    print(1)
    
    return(data.frame(fn = target.fns, from.sec = target.from.sec, to.sec = target.to.sec));
    
    
}