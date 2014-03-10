



DoFeatureExtraction <- function (limit = FALSE) {
    # performs feature extraction on the outer target events
    # i.e. the events which fall within the target sites/dates/times, 
    #      but ignoring the percent of target
    # 
    # Details:
    #   reads events one at a time. For each event, generates the 
    #   spectrogram for the audio file that event was in. Before generating
    #   the spectrogram, the spectrogram for the previous event is checked to 
    #   see if it is the same (if the current event belongs to the same file as
    #   the previous event). For this reason, the function will run much more quickly if
    #   the events are sorted by site then date then start second. 
    
    # set this to how many files to process, 
    # false to do them all
    
    library('tuneR')
    events <- GetOuterTargetEvents()
    # make sure it is sorted by filename 
    events <- events[with(events, order(filename)) ,]
    
    already.extracted.features <- GetExistingFeatures()
    
    # features will be calculated and added to the already extracted features
    # therefore, don't calculate for events which are already calculated
    if (class(already.extracted.features) == 'data.frame') {
        event.ids.to.process <- setdiff(events$event.id, already.extracted.features$event.id)
        events <- events[events$event.id %in% event.ids.to.process, ]   
    }
    
    cur.wav.path <- FALSE
    cur.spectro <- FALSE
    features.all <- c()
    
    Report(2, 'Extracting features for events', OutputPath('events'), '...')
    Report(2, nrow(events), 'events in total')
    
    num.events.before.previous.file <- 0
    ptmt <- proc.time();
    ptm <- proc.time()
    ev <- 1;
    while (ev <= nrow(events)) {
        Dot()
        bounds <- as.numeric(c(events$start.sec.in.file[ev], 
                             events$duration[ev],
                             events$top.f[ev],
                             events$bottom.f[ev]))
        #fn <- EventFile(events$site[ev], events$date[ev], events$min[ev], ext = 'wav')
        wav.path <- file.path(g.audio.dir, paste(events$filename[ev], 'wav', sep='.'))
        if (wav.path != cur.wav.path) {
            if (ev > 1) {
                #not starting the first file, so we can report on the previous
                first.ev.in.prev.file <- num.events.before.previous.file + 1
                last.ev.in.prev.file <- ev - 1
                num.events.in.prev.file <-  last.ev.in.prev.file - first.ev.in.prev.file + 1
                timer.msg <- paste('feature extraction for events', 
                                   first.ev.in.prev.file, 'to', last.ev.in.prev.file, 
                                   '(', num.events.in.prev.file, 'events)')
                Timer(ptm, timer.msg, num.events.in.prev.file, 'event')
                num.events.before.previous.file <- ev - 1
                ptm <- proc.time()
            }
            
            Report(3, 'processing features for events in', wav.path)
            Report(3, 'starting with event', ev)
            
            cur.spectro <- Sp.Create(wav.path, draw=FALSE)
            cur.wav.path <- wav.path
          
        }
        features <- as.data.frame(GetFeatures(events[ev,], cur.spectro));
        features$event.id <- events$event.id[ev]
        
        if (ev > 1) {
            features.all <- rbind(features.all, features)
        } else {
            features.all <- features
        }
        ev <- ev + 1
    }
    Timer(ptmt, paste('feature extraction for all',nrow(events),'events'), nrow(events), 'event')
    
    # merge these features with the previously extracted
    if (class(already.extracted.features) == 'data.frame') {
        features.all <- rbind(features.all, already.extracted.features)  
    }
    
    features.all <- OrderBy(features.all, 'event.id')
    WriteOutputCsv(features.all, MasterOutputPath('features'))
    

    
}

GetExistingFeatures <- function () {
    # determines if any feature extraction already completed is still valid. 
    # if not, move it to archived and start feature extraction from scratch
    # if it is still valid (i.e. events have not chagned and feature extraction has not changed)
    # it returns the features already extracted
    require('digest')
    # check if any of these files have changed
    to.check <- c(AllEventsPath(),
                 'features.R')

    hash.name <- 'features'
    # creates a hash of the text content of all the files appended together
    new.content.hash <- HashFileContents(to.check)
    old.content.hash <-  ReadHash(hash.name)
    if (old.content.hash != new.content.hash) {
        WriteHash(hash.name, new.content.hash)
        return(FALSE)
    } else {
        path <- MasterOutputPath('features');
        if (file.exists(path)) {
            return(ReadOutputCsv(path))
        } else {
            return(FALSE)
        }
    } 
}





GetFeatures <- function (event, spectro) {
    # for a particular event, calculates all the features to be used
    #
    # Args:
    #  event: list of the event bounds
    #  spectro: spectrogram of the event
    #
    # Returns:
    #  list
    
    #start.time, duration, bottom.f, top.f
    
    features <- list(
        duration = event$duration,
        mid.f = (event$bottom.f + event$top.f) / 2,
        f.range = event$top.f - event$bottom.f
    )
    
    # gets the sub-matrix of the event from the full matrix
    event_vals <- SliceStft(c(event$start.sec.in.file, event$duration, event$bottom.f, event$top.f), spectro) 

    
    # Feature: Peak frequency oscillation
    
    # get a vector of peak frequency bins (rows numbers) for each frame
    peaks <- apply(event_vals, 2, which.max)
    
    peak.vals <- apply(event_vals, 2, max)
    
    # the average peak frequency bin, allowing for amplitude of peak
    mean.peak.f.bin <- mean(peaks * peak.vals) / sum(peak.vals)
    features$mean.peak.f <- (mean.peak.f.bin * spectro$hz.per.bin) + event$bottom.f
    
    
    #average change in peak frequency from one frame to the next
    features$peak.f.osc <- (VectorFluctuation(peaks) * spectro$hz.per.bin)
    
    features$peak.amp.osc <- VectorFluctuation(peak.vals)
    
    # Feature: pureness of tone
#    pure.tone.score <- mean(GetPureness(event_vals))
    # feature: amplitude modulation
#    db.osc <- ColFluctuation(event_vals)
    
    

    
    return(features)
}


ColumnSDs <- function (m) {
    sds <- apply(m, 2, sd)
    return (list(mean = mean(sds), sd = sd(sds)))
}

#TODO
ColFluctuation <- function (m, relative = TRUE) {
    col.means <- apply(m, 2, mean)
    return (VectorFluctuation(col.means, relative = relative))
}


FindCenterFreq <- function (v, hz.per.bin, low.f) {
    center.bin <- FindCenter(v)
    center.freq <- (center.bin * hz.per.bin) + (low.f - hz.per.bin)
    return (center.freq)
}

CenterFreqs <- function (m, hz.per.bin, low.f) {
    # calculates the center frequencies for each time frame in the matrix
    # and returns the slope of the best fit line.
    #
    # Args:
    #    m: matrix; the event values
    #    hz.per.bin: float; the number of hz per frequency bin
    #    low.f: the frequency of the lowest frequency bin.
    #
    # Returns:
    #    list: with 2 values:
    #       - cfs.slope: 
    #           the slope of the center frequency calculated for each frame
    #       - cfs.mean: 
    #            the center frequency calculated on the mean db value for each 
    #            frequency bin (this is not the same as the mean center  
    #            frequency after calculating for each time frame)
    #
    # Details:
    #   This indicates how much in general the frequency of the event is 
    #   increasing or decreasing from start to finish
    #   note that both a flat line and a line which increases then decreases 
    #   could end up with a similar slope, because it is fitting a straight line
    
    
    cfs <- apply(m, 2, FindCenterFreq, hz.per.bin = hz.per.bin, low.f = low.f)
    x <- 1:length(cfs)
    fit <- lm(cfs ~ x)
    cfs.slope <- as.vector(fit$coefficients[2])
    
    means <- apply(m, 1, mean)
    cfs.mean <- FindCenterFreq(means, hz.per.bin = hz.per.bin, low.f = low.f)
    
    return(list(cfs.slope = cfs.slope, cfs.mean = cfs.mean))
    
    
}

GetPureness <- function (m) {
    # measures how pure the tone is
    #
    # Args:
    #   m: matrix; Spectrogram values
    #
    # Returns:
    #   vector; a score for each time frame
    #
    # the mean distance in db of each frequency band from the peak frequency DB value.
    # divided by the number of frequency bands
    
    scores <- apply(m, 2, 
                      function (col) {
                          max <- rep(max(col), length(col))
                          return(mean(abs(col - max)))
                      })
    
    return(scores)
    
}


GetLineOfBestFit <- function (m) {
    # given a matrix of spectrogram values
    # uses linear model function to produce a line which best fits
    # the sound
    # 
    # Details:
    #   convert the matrix to a list of examples with 2 variables, Hz and time
    #   for each hz/time cell in the matrix the amplitude will be converted
    #   to a number of examples. A higher amplitude will mean more examples 
    
    # 1. normalize to 0 - 100 and round to integers
    m <- round(Normalize(m) * 100)
    
    data <- MatrixToExamples(m);
    
    res <- lm(data[ ,2]~data[ ,1])
    
    
    return(res);
    
}


MatrixToExamples <- function (m) {
    # converts a matrix of integers to a 2 column table
    # the 2 columns are row number and column number of the matrix
    # the integer value in the matrix is the number of times the 
    # row and column number appear in the resulting table
    #
    # Args:
    #   m: matrix
    # 
    # Returns: 
    #   data.frame; 
    #
    # Details:
    #   example:
    #   given the matrix 
    #   1 2 
    #   3 1
    #   it will return
    #   y x
    #   1 1  # row 1 column 1 appears once
    #   1 2  # row 1 column 2 appears twice
    #   1 2 
    #   2 1  # row 2 column 1 appears thrice
    #   2 1 
    #   2 1
    #   2 2  # row 2 column 2 appears once
    

    examples <- mapply(function (r,c, val) {
        mx <- rep(c(r,c), val)  
        return(mx)  
    }, c(row(m)), c(col(m)), c(m), SIMPLIFY = TRUE);
    examples <- matrix(unlist(examples), ncol = 2, byrow = TRUE);
    #colnames(example) <- c('y', 'x');
    return(examples);
}


testGetLineOfBestFit <- function () {
    
    m <- matrix(seq(1,100,1), nrow = 10) + 
        t(matrix((seq(1,100,1) ^ 2) / 100, nrow = 10)) + 
        matrix(rep(seq(1,25,5),20),nrow = 10, byrow = FALSE) + 
        matrix(rep(seq(1,250,10) / 3 ,4),nrow = 10, byrow = FALSE)
    
    bf <- GetLineOfBestFit(m);
    
    return(bf)
    
    
}



