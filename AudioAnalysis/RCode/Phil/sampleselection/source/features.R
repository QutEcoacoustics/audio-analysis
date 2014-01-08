

Read.Events <- function () {
    return(read.csv(OutputPath('events'), header = FALSE, 
                    col.names = g.events.col.names))
}


DoFeatureExtraction <- function () {
    # performs feature extraction the events listed in multiple text files
    
    # set this to how many files to process, 
    # false to do them all
    limit <- FALSE
    
    library('tuneR')
    
    events <- read.csv(OutputPath('events'), header = TRUE)
    
    if (limit != FALSE && limit < nrow(events)) {
        events <- events[1:limit,]
    }
    
    events <- as.matrix(events)
    
    cur.wav.path <- FALSE
    cur.spectro <- FALSE
    features.all <- c()
    
    Report(2, 'Extracting features for events', OutputPath('events'), '...')
    Report(2, nrow(events), 'events in total')
    for (ev in 1:nrow(events)) {
        bounds <- as.numeric(as.vector(events[ev,5:8]))
        wav.path <- file.path(g.audio.dir, paste0(events[ev,1], '.wav'))
        if (wav.path != cur.wav.path) {
            cur.spectro <- Sp.create(wav.path, draw=FALSE)
            cur.wav.path <- wav.path
            Report(3, 'processing features for events in', wav.path)
            Report(3, 'starting with event', ev)
        }
        features <- as.vector(unlist(GetFeatures(bounds, cur.spectro)))
        features.all <- c(features.all, features)
    }
    features.all <- as.data.frame(matrix(data = features.all, 
                                         nrow = nrow(events), 
                                         byrow = TRUE))
    # have not escaped separator char, but shouldn't 
    # matter with numeric values anyway
    write.table(features.all, file = OutputPath('features'), append = FALSE, 
                col.names = TRUE, row.names = FALSE, sep = ",", quote = FALSE)
    
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
        top.f = bounds[4]
    )
    
    # gets the sub-matrix of the event from the full matrix
    event_vals <- SliceStft(bounds, spectro)
    duration <- ncol(event_vals) / spectro$frames.per.sec
    # get a vector of peak frequency bins (rows numbers) for each frame
    peaks <- apply(event_vals, 2, which.max)
    #average change in peak frequency from one frame to the next
    peak.f.osc <- (VectorFluctuation(peaks) * spectro$hz.per.bin)
    bbscores <- BbScores(event_vals)
    db.osc <- ColFluctuation(event_vals)
    center.freqs <- CenterFreqs(event_vals, 
                                spectro$hz.per.bin, 
                                features$bottom.f)
    
    return(c(features, list(
        peak.f.osc = peak.f.osc,
        #the average standard deviation of frequency db values across all frames
        # broadband frames will have a low, pure whistle will have a low value
 #       bb.score.mean = mean(bbscores),
        # standard deviation of frequency standard devitions
        # i.e. how much the standard deviation of frequency changes
 #       bb.score.sd  = sd(bbscores),
 #       db.osc  = db.osc,
        center.freq.mean = center.freqs$cfs.mean,
        center.freq.slope = center.freqs$cfs.slope
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

BbScores <- function (m) {
    # measures how broadband each frame of the matrix is
    #
    # sums the distance in db of each frequency band from the peak value.
    
    bbscores <- apply(m, 2, 
                      function (col) {
                          max <- rep(max(col), length(col))
                          return(mean(abs(col - max)))
                      })
    
    return(bbscores)
    
    
    
}





