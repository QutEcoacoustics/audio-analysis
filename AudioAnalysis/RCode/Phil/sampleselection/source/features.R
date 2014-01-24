



DoFeatureExtraction <- function () {
    # performs feature extraction on the events in the events file
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
    limit <- FALSE
    
    library('tuneR')
    
    events <- ReadOutput('events')
    
    if (limit != FALSE && limit < nrow(events)) {
        events <- events[1:limit,]
    }
    
    #events <- as.matrix(events)
    

    
    cur.wav.path <- FALSE
    cur.spectro <- FALSE
    features.all <- c()
    
    Report(2, 'Extracting features for events', OutputPath('events'), '...')
    Report(2, nrow(events), 'events in total')
    
    num.events.before.previous.file <- 0
    ptmt <- proc.time();
    ptm <- proc.time()
    for (ev in 1:nrow(events)) {
        Dot()
        bounds <- as.numeric(c(events$start.sec.in.file[ev], 
                             events$duration[ev],
                             events$top.frequency[ev],
                             events$bottom.frequency[ev]))
        fn <- EventFile(events$site[ev], events$date[ev], events$min[ev], ext = 'wav')
        wav.path <- file.path(g.audio.dir, fn$fn)
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
        features <- as.vector(unlist(GetFeatures(bounds, cur.spectro)))
        features.all <- c(features.all, features)
    }
    Timer(ptmt, paste('feature extraction for all',nrow(events),'events'), nrow(events), 'event')
    features.all <- as.data.frame(matrix(data = features.all, 
                                         nrow = nrow(events), 
                                         byrow = TRUE))
    # have not escaped separator char, but shouldn't 
    # matter with numeric values anyway
    WriteOutput(features.all, 'features')
    
}

GetFeatures <- function (bounds, spectro) {
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
        duration = bounds[2],
        bottom.f = bounds[3],
        top.f = bounds[4],
        mid.f = (bounds[3] + bounds[4]) / 2,
        f.range = bounds[4] - bounds[3]
    )
    
    # gets the sub-matrix of the event from the full matrix
    event_vals <- SliceStft(bounds, spectro) 
    duration <- bounds[2]
    
    # Feature: Peak frequency oscillation
    
    # get a vector of peak frequency bins (rows numbers) for each frame
    peaks <- apply(event_vals, 2, which.max)
    #average change in peak frequency from one frame to the next
    peak.f.osc <- (VectorFluctuation(peaks) * spectro$hz.per.bin)
    
    # Feature: pureness of tone
    
#    pure.tone.score <- mean(GetPureness(event_vals))
    
    # feature: amplitude modulation
    
#    db.osc <- ColFluctuation(event_vals)
    
    

    
    return(c(features, list(
        peak.f.osc = peak.f.osc
        #the average standard deviation of frequency db values across all frames
        # broadband frames will have a low, pure whistle will have a low value
 #       bb.score.mean = mean(bbscores),
        # standard deviation of frequency standard devitions
        # i.e. how much the standard deviation of frequency changes
 #       bb.score.sd  = sd(bbscores),
 #       db.osc  = db.osc,
 #       center.freq.mean = center.freqs$cfs.mean,
 #       center.freq.slope = center.freqs$cfs.slope
    )))
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



