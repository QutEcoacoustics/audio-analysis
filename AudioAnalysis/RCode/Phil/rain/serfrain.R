save.path <- '/Users/n8933464/Documents/SERF/rain/';

rain.csv <- paste0(save.path, 'datetimes.csv');
intensities.csv <- paste0(save.path, 'intensities.csv');
complete.mins.csv <- paste0(save.path, 'complete.mins.csv');
final.intensities.csv <- paste0(save.path, 'rain.csv');

#these are the colours of each of the rain intensity legend
intensity.key <- c(
   c(0,0,0),
   c(255,255,255),
   c(245,244,245),
   c(180,177,255),
   c(120,111,255),
   c(15,0,255),
   c(36,218,195),
   c(22,152,143),
   c(11,103,102),
   c(255,255,12),
   c(255,202,8),
   c(253,149,6),
   c(252,94,6),
   c(252,0,5),
   c(198,0,3),
   c(119,0,1),
   c(39,0,0));

# pixmap uses values from 0 to 1
intensity.key <- intensity.key/255;


intensity.key <- matrix(intensity.key, ncol = 3, byrow = TRUE);
intensity.val <- c(0,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15);



IntensityMap <- function () {
    intensities <- read.csv(intensities.csv);
    complete.mins <- GetDateTimes(2011, 10, 1, 2012, 6, 2, every.this.many.mins = 1);
    write.csv(complete.mins, complete.mins.csv, row.names = FALSE);
    #adjust for timezone: add 10 hours
    adjust.mins <- 10*60+1;
    complete.mins <- complete.mins[adjust.mins:nrow(complete.mins),];
    vals <- Interpolate(as.vector(intensities[,7]), 6);
    if (length(vals) < nrow(complete.mins)) {
       complete.mins <- complete.mins[1:length(vals), ];
    } else {
       vals <- vals[1:nrow(complete.mins)];
    }
    print(length(vals));
    print(nrow(complete.mins));
    vals <- cbind(complete.mins, vals);
    write.csv(vals, final.intensities.csv, row.names = FALSE);
}

ConvertMinNum <- function (minnum) {
    day <- floor(minnum/1440);
    rest <- minnum - day*1440;
    hour <- floor(rest/60);
    rest <- rest - hour*60;
    min <- rest;
    print(day);
    print(hour);
    print(min)
}

Interpolate <- function (vals, num.steps) {
    i1 <- vals;
    i2 <- i1[-1];
    i1 <- i1[1:(length(i1)-1)];
    step <- (i2 - i1) / num.steps;
    vals <- i1;
    for (i in 1:(num.steps-1)) {
      vals <- cbind(vals, i1+step*i);
    }
    vals <- as.vector(t(vals));
    return(vals);
}

GetRainIntensityFromImages <- function (from.row = 1, to.row = 1000000, every = 1, save.this.often = 50, append = FALSE) {
    
    
    path <- paste0(save.path, 'png/');
    files <- list.files(path);
    datetimes <- read.csv(rain.csv);
    
    
    if (append) {
        # if append is set then we automatically set from.row
        # to be the last row added to the intensities csv
        intensities.file <- read.csv(intensities.csv);
        from.row <- nrow(intensities.file) + 1;
        
    } else {
    
      if (from.row < 1) {
        from.row <- 1
      }
    
    }
    
    if (to.row > nrow(datetimes)) {
        to.row <- nrow(datetimes);
    }
    datetimes <- datetimes[from.row:to.row, ];
    if (every > 1) {
        datetimes <- datetimes[seq(1,nrow(datetimes),every), ];
    }
    
    
    
    library(pixmap);
    total.time.so.far = 0;
    
    save.points <- seq(1, nrow(datetimes), save.this.often);
    
 
    for (sub.from.row in save.points) {
        
      sub.to.row <- sub.from.row+save.this.often-1;
      if (sub.to.row > nrow(datetimes)) {
          sub.to.row <- nrow(datetimes);
      }
        
      print(paste('processing rows', sub.from.row , 'to', sub.to.row, 'of', nrow(datetimes) ,'from selected subset'));
    
      cur.datetimes <- as.matrix(datetimes[sub.from.row:sub.to.row,]);

      ptm <- proc.time();
      intensities <- apply(cur.datetimes, 1, GetRainIntensity);

      

      

    
      intensities <- matrix(intensities, ncol = 1);
      intensities <- cbind(cur.datetimes, intensities);
    
    #print(intensities);
    
      if (sub.from.row == 1 && !append) {
        add <- FALSE;
        col.names <- TRUE;
      } else {
        col.names <- FALSE
        add <- TRUE;
      }
    
      write.table(intensities, intensities.csv, row.names = FALSE, append = add, sep = ",", col.names = col.names);
      
      # reporting
      speed <- proc.time() - ptm;
      total.time.so.far <- total.time.so.far + speed[3];
      
      #reporting about this chunk
      print(paste('time taken for chunk : ', round(speed[3],2), '(', round(speed[3]/nrow(cur.datetimes),2) , ' per row)'));


      #reporting estimate time to go for the whole thing
      average.time.per.row <-  total.time.so.far/sub.to.row;
      estimated.total.time <- average.time.per.row * nrow(datetimes);
      
      print(paste('estimated seconds until completion', round(estimated.total.time - total.time.so.far, 2)))
    
    }
    
    print(paste('complete! time taken: ', round(total.time.so.far,2)));
    


}

GetRainIntensityFromRgb <- function (rgb) {
    diff <- apply(intensity.key, 1, function(v) {
        d <- mean(abs(v - rgb))
        return(d);
    });
    indx <- which.min(diff);
    return(intensity.val[indx]);
}

GetRainIntensity <- function (datetime) {
    
    png.file <- PathToImage(datetime);
    
    
    if (file.exists(png.file) && file.info(png.file)$size > 0) {
        ppm.file <- PathToImage(datetime, 'ppm');
        command <- paste("/opt/local/bin/convert", png.file, ppm.file);
        
        err <- try(system(command)); # ImageMagick's 'convert'
        
        if (class(err) ==  "try-error") {
            return(NA);
        }
        
        # get pixel value
        # TODO: why does this cause a warning?
        
        ppm.img <- try(read.pnm(ppm.file));
        
        if (class(ppm.img) == "try-error") {
            return(NA);
        }

        channels <- getChannels(ppm.img)
        rgb <- channels[183,183,];
        # delete bitmap images (or use > 50GB on a year of observations)
        unlink(ppm.file);
        i.val <- GetRainIntensityFromRgb(rgb);
        
        return(i.val);
    } else {
    

      return(NA)
    
    }
    
 
    
    
}

GetRainDataFromFile <- function () {
    
    datetimes <- read.csv(rain.csv);
    
    for (i in 1:nrow(datetimes)) {
        
        dt <- datetimes[i,];
        status <- SaveImage(dt);
        datetimes[i,6] <- status;
        
        if (i %% 20 == 0) {
            #save the new statuses every 20 rows processed
            if (i %% 40 == 0) {
                # alternate saving to backup
                # because if it gets interrupted while saving it can truncate
                fn <- paste(rain.csv, '.bak');
            } else {
                fn <- rain.csv;
            }
            write.csv(datetimes, fn, row.names = FALSE);
        }
    }
    
}

MakeRainDataFile <- function (start.year, start.month, start.day, end.year, end.month, end.day, save.images = FALSE) {
    #makes a file with 1 row for each minute that rain data should be avilable
    # columns are year, month, day, hour, minute, status (0 = no download attempt, 1= download attempt but doesnt exist, 2 = downloaded)
   datetimes <- GetDateTimes(start.year, start.month, start.day, end.year, end.month, end.day);
   datetimes <- colbind(datetimes, rep.int(0, nrow(datetimes)))
   write.csv(datetimes, rain.csv, row.names = FALSE);
   if (save.images) {
     apply(datetimes, 1, SaveImage);
   }
}

UpdateRainDataStatus <- function (set.for.known.missing = FALSE) {
    
    data <- read.csv(rain.csv);
    
    status <- apply(data, 1, function (datetime) {
        png.file <- PathToImage(datetime);



         #status 2 if file exists or 1 if file has zero size
         if (file.exists(png.file)) {
          if (file.info(png.file)$size > 0) {
            status <- 2;
          } else {
            status <- 1;
          }
        }
        
        
        if (set.for.known.missing) {
        # status 1 for known missing maps
          mfsom.min = 8*1440 + 14*60 + 36;
          mfsom.max = 10*1440 + 2*60 + 0;
          min.from.start.of.month = datetime[3]*1440 + datetime[4]*60 + datetime[5];
          # these days are known to be missing
          if (datetime[1] == 2011 && datetime[2] == 10 && min.from.start.of.month  > mfsom.min &&  min.from.start.of.month < mfsom.max) {
              status <- 1;
          }
        
        }
        
        return(status);
    });
    status <- matrix(status, ncol = 1);
    data <- cbind(data, status);
    write.csv(data, rain.csv, row.names = FALSE)
}

SpotCheckMissingFiles <- function (num.to.check = 60, check.status = 1) {
    data <- read.csv(rain.csv);
    if (check.status != 1 && check.status != 2) {
        stop('invalid check status')
        return();
    }
    data.to.check <- data[data[,6] == check.status,];
    to.check <- sample(1:nrow(data.to.check), num.to.check);
    
    result <- c();
    
    for (i in 1:num.to.check) {
        print(paste(i, '-------'));
        datetime <- data.to.check[to.check[i],]
        print(paste('about to download for', paste(as.character(datetime), collapse = '')));
        status <- SaveImage(datetime, check.status = FALSE);
        
        print(paste('status', status));

        
        result <- c(result, status);
        
    }
    
    return(result)
    
    
}

SaveImage <- function (datetime, check.status = TRUE) {
    
    # if the status says that this has been processed,
    # return
    if (check.status && datetime[6] != 0) {
        return(datetime[6]);
    }
    
    dest.filename <- PathToImage(datetime);
    if (!file.exists(dest.filename)) {
      img.url <- MakeUrl(datetime);
      #z <- tempfile()
      print(paste('downloading file', img.url, 'to', dest.filename));
      err <- try(download.file(url = img.url, destfile = dest.filename, method = 'curl', quiet = TRUE, mode = 'wb', extra='--max-time 10'));
      if (err == 0) {
        print('download ok, returning status 2')
        return(2);
      } else {
          print(paste('curl error',err));
          print('returning status 1');
          return(1);
      }
    } else {
        print('file exists: status 2');
          return(2);
    }
    
    # we shouldn't ever get here
    return(0);

    
    
}

MakeUrl <- function (datetime) {
    return(paste0('http://data1.theweatherchaser.com/radar/IDR663/',datetime[1],'/',sprintf("%02s", datetime[2]),'/',sprintf("%02s", datetime[3]),'/IDR663.T.',DtString(datetime),'.png'));
}

DtString <- function (datetime, separator = '') {
     return(paste(datetime[1], sprintf("%02s", datetime[2]), sprintf("%02s", datetime[3]), sprintf("%02s", datetime[4]), sprintf("%02s", datetime[5]),  sep = separator))
}

PathToImage <- function(datetime, type = 'png') {
    return(paste0(save.path, type, '/',DtString(datetime, '-'), '.', type));
}

GetDateTimes <- function (start.year, start.month, start.day, end.year, end.month, end.day, every.this.many.mins = 6) {
    dates <- GetDates(start.year, start.month, start.day, end.year, end.month, end.day)
    times <- GetTimes(every.this.many.mins);
    datetimes <- c();
    
    #repeat each row of dates by the number of mins in a day
    full.dates <- dates[rep(1:nrow(dates), rep(nrow(times), nrow(dates))),];
    
    #repeat the times for each date
    full.times <- times[rep(1:nrow(times), nrow(dates)),];
    
    return(cbind(full.dates, full.times));

    
}

GetTimes <- function (every.this.many.mins) {
    mins <- seq(0,1439,every.this.many.mins);
    hours <- floor(mins/60);
    mins <-    mins - (hours * 60);
    return(cbind(hours, mins));
}

GetDates <- function (start.year, start.month, start.day, end.year, end.month, end.day) {
    
    
    months <- c(31,28,31,30,31,30,31,31,30,31,30,31);
    months.l <- c(31,29,31,30,31,30,31,31,30,31,30,31);
    
    cur.year <- start.year;
    cur.month <- start.month;
    cur.day <- start.day;
    
    dates <- c();
    
    keepgoing <- TRUE;
    
    while (keepgoing) {
        
        cur.date = c(cur.year,cur.month,cur.day);
        
        dates <- c(dates, cur.date);
        
        if (cur.year %% 400 == 0 || (cur.year %% 4 == 0 && cur.year %% 100 != 0)) {
            days.in.month <- months.l[cur.month];
        } else {
            days.in.month <- months[cur.month];
        }
        
        cur.day <- cur.day + 1;
        if (cur.day > days.in.month) {
            cur.day <- 1;
            cur.month <- cur.month + 1;
            if (cur.month > 12) {
                cur.month <- 1;
                cur.year <- cur.year + 1;
            }
        }
        
        if (cur.year >= end.year && cur.month >= end.month && cur.day > end.day) {
            keepgoing <- FALSE;
        }
        
        
    }
    
    dates <- matrix(dates, ncol = 3, byrow = TRUE);
    

    
    return(dates);
    
    
}

