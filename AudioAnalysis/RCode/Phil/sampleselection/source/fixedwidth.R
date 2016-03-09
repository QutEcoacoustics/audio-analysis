# step 1: call MakeSegmentList to get a list of 1-second segments
# step 2: call ExtractSDF on the segment list to get a list of spectral dynamic features



MakeSegmentList <- function (min.list = NULL, num.per.min = 60) {
    # creates a list of segments of equal length
    #
    # Returns:
    #   data.frame; with cols: site, date, min, event.id, file
    #
    # Details:
    #   assumes files of 10 minute length
    
    # todo: verfify audio exists for full expected duration for each file, and insert NAs for missing audio. 
    
    if (is.null(min.list)) {
        min.list <- ReadOutput('target.min.ids')
    }
    
    
    # get the wave path for every minute
    # because we know that audio files are 10 minutes long, we can shortcut by 
    # getting the audio file for every 10th minute, then repeating the value 10 times
    # every 10th minute of the day
    audio.file.duration <- 10
    min.list.per.duration <- min.list$data[min.list$data$min %% audio.file.duration == 0, ]
    wave.path.per.duration <- audio.durations <- rep(NA, nrow(min.list.per.duration))
    for (i in 1:length(wave.path.per.duration)) {
        wave.path.per.duration[i] <- GetAudioFile(min.list.per.duration$site[i], min.list.per.duration$date[i], min.list.per.duration$min[i])
    }
    min.list$data$wave.path <- wave.path.per.duration[rep(1:length(wave.path.per.duration),each=audio.file.duration)]
    
    segment.list <- min.list$data[rep(1:nrow(min.list$data),each=num.per.min), ]
    segment.list$seg.num <- rep(1:num.per.min, nrow(min.list$data))
    segment.list$start.sec <- (segment.list$seg.num - 1) * (60/num.per.min)
    segment.list$file.sec <- rep(1:(audio.file.duration * num.per.min), length(wave.path.per.duration)) - 1
    segment.list <- AddEventIdColumn(segment.list)
    
    dependencies <- list('target.min.ids' = min.list$version)
    params <- list('num.per.min' = 60)
    
    # check duration of final audio file, since it is probably not exactly the right length
    final.path <- wave.path.per.duration[length(wave.path.per.duration)]
    duration.of.final <- floor(GetAudioMeta(final.path, 'duration'))
    # remove any segments that don't have audio
    include <- rep(TRUE, nrow(segment.list))
    include[segment.list$wave.path == final.path][(duration.of.final+1):sum(segment.list$wave.path == final.path)] <- FALSE
    segment.list <- segment.list[include,]
    
    segment.list.version <- WriteOutput(x = segment.list, name = 'segment.events',params = params, dependencies = dependencies)
    
}




ExtractSDF <- function (num.fbands = 16, max.f = 8000, min.f = 200, num.coefficients = 16, parallel = TRUE) {
    # extracts "spectral dynamic features" of each segemnt in segment list
    # spectral dynamic features are fft coefficients of the spectrogram values in the time domain of each 
    # frequency bin
    #
    # Args:
    #   segment.list: data.frame;
    #   segment.length: numeric. Number of seconds per segment. used to determine the number of frames to include
    #   num.bands: int;
    #   max.f, min.f: int; the spectrogram will be calculated so that frequencies between max.f and min.ft are divided up into num.fbands frequency bands
    #   num.coefficients: how many time domain ceptral coefficients to keep. Only the first coefficients are kept with the high-frequency coefficients discarded. 
    

    segment.events <- ReadOutput('segment.events')
    segment.list <- segment.events$data
    
    segment.length = 60 / segment.list$params$num.per.min
    
    files <- unique(segment.list$wave.path)

    # ensure that each file appears as a single continuous run in the segment list
    # this will make sure we can match segments to results
    each <- nrow(segment.list) / length(files)
    if (!isTRUE(all.equal(segment.list$wave.path, rep(files, each = each)))) {
        stop('problem with the segment list')
    }
    
    # todo: calculate this from wave length and segment length
    num.segments.per.file <- 600
    #todo: set 'outfile' param to get messages
    
    SetReportMode() # reset to console only
    if (parallel) {
        SetReportMode(socket = TRUE)
        cl <- makeCluster(3)
        registerDoParallel(cl)
        res <- foreach(file = files, .combine='rbind', .export=ls(envir=globalenv())) %dopar% ExtractSDFForFile(file, 
                                                                                 segments = segment.list,
                                                                                 num.fbands = num.fbands, 
                                                                                 max.f = max.f, 
                                                                                 min.f = min.f, 
                                                                                 num.coefficients = num.coefficients)  
       
    } else {
        SetReportMode(socket = FALSE)
        res <- foreach(file = files, .combine='rbind') %do% ExtractSDFForFile(file, 
                                                                                 segments = segment.list,
                                                                                 num.fbands = num.fbands, 
                                                                                 max.f = max.f, 
                                                                                 min.f = min.f, 
                                                                                 num.coefficients = num.coefficients)
    }
    SetReportMode() # reset to console only

    # round result to reduce size of csv
    # todo: make this a configuration variable
    # todo: first normalize to make sure we are rounding to significant digits
    #       eg if the range is 0.00001 to 0.00002, then rounding like this is bad
    
    

    
    res[,-1] <- round(res[,-1], 4)

    
    # double check that foreach has put things back in the correct order after doparallel
    (sum(segment.list$event.id == res$event.id) == length(segment.list$event.id))
    
    
    
    # colnames, eg frequency band 3 coefficient 6 will be "b03.c06"
    feature.names <- paste(rep(paste0('b', sprintf("%02s", 1:num.fbands)), each = num.coefficients), rep(paste0('c', sprintf("%02s", 1:num.coefficients)), num.fbands), sep = ".")
    colnames(res) <- c('event.id', feature.names)
    
    #cbind(segment.list[,c('event.id', 'min.id')])
    
    # remove missing audio segments from features and segments
    contains.na <- apply(res, 1, function (x) { 
        any(is.na(x))
    })
    res <- res[!contains.na,]
    segment.list <- segment.list[!contains.na,]
    
    # output
    dependencies <- list(segment.events = segment.events$version)
    params <- list(max.f = max.f, min.f = min.f, num.coefficients = num.coefficients)
    WriteOutput(x = res, name = 'TDCCs', params = params, dependencies = dependencies)
    
    # re-save segments without those with missing audio
    segment.events$data <- segment.list
    WriteStructuredOutput(segment.events)
    
    

    
}


ExtractSDFForFile <- function (path, 
                               segments,
                               num.fbands = 16, 
                               max.f = 8000, 
                               min.f = 400, 
                               num.coefficients = 16,
                               use.cached = TRUE) {
    
    # given the path to an audio file, will create a spectrogram, then split it into the appropriate 
    # segments, then calculate TDCCs 
    #
    # Args:
    #   path: string; the path to the audio file
    #   segments: data.frame; contains column: wave.path, min.id, 
    
    require('digest')
    
    cache.id <- digest(paste(path, num.fbands, max.f, min.f, num.coefficients))
    if (use.cached) {
        retrieved.from.cache <- ReadCache(cache.id)
        if (retrieved.from.cache != FALSE) {
            return(retrieved.from.cache)
        }
    }

    
    cur.segments <- segments[segments$wave.path == path, ] 
    all.files <- unique(segments$wave.path)
    this.file <- match(path, all.files)
    Report(5, "extracting TDCCs for events", cur.segments$event.id[1],"-", cur.segments$event.id[nrow(cur.segments)], 'in file ', this.file, 'of', length(all.files))
    
    # create the spectrogram
    cur.spectro <- Sp.CreateFromFile(path, frame.width = 256)
    
    # Noise Reduction !!!!
    spectro.vals <- DoNoiseReduction(cur.spectro$vals)
    spectro.vals <- ReduceSpectrogram2(cur.spectro$vals, num.bands = num.fbands, min.f, max.f)
    
    
    
    #        cur.spectro$vals <- RemoveNoise(cur.spectro$vals)
    segment.duration <- 1 # seconds
    
    # width of segment should be rounded down to the nearest power of 2 (for fft)
    width.before <- floor(segment.duration * cur.spectro$frames.per.sec)
    width <- RoundToPow2(width.before, floor = TRUE)
    
    Report(5, width.before - width, 'out of', width.before, 'time-frames were discarded to acheive power of 2')
    
    # TODO: update to allow different files to have different sample rates 
    # if files are different sample rates, this will stuff everything up, because the frequency bands will represent different frequencies
    # also, the width will represent a different length of time because of the rounding down
    # currently we know they are all the same
    
    if (num.coefficients > width / 2) {
        stop("specified segment length doesn't allow", num.coefficients, "coefficients")
    }
    
    # hamming  window
    hamming <- 0.54 - 0.46 * cos(2 * pi * c(1:width) / (width - 1))
    
    
    # add the start.sec.in.file
    start.sec.in.file <- (cur.segments$min %% 10) * 60 + cur.segments$start.sec
    
    # add the start.col.in.file
    start.col.in.file <- round(start.sec.in.file * cur.spectro$frames.per.sec + 1)
    
    num.features <- num.fbands * num.coefficients
    # // add one col for event.id
    TDCCs <- matrix(NA, nrow = nrow(cur.segments), ncol = num.features + 1)
    
    
    for (s in 1:nrow(cur.segments)) {
          
        seg <- cur.segments[s, ]
        start.col <- start.col.in.file[s]
        end.col <- start.col + width - 1
        
        if (end.col <= ncol(spectro.vals)) {
            # in case some files are too short. E.g. the original 24 hour recordings finish before midnight
            
            seg.spectro <- spectro.vals[,start.col:end.col]
            TDCCs[s,1:num.features+1] <- ExtractTDCCs(seg.spectro, window = hamming, num.coefficients = num.coefficients)  
        }
        
    }
    
    num.segments.processed <- sum(!is.na(TDCCs[,2]))
    Report(5, 'features extracted for', num.segments.processed, '/', nrow(TDCCs), 'segments')
    
    TDCCs <- as.data.frame(TDCCs)
    
    # 1st col is event .id
    TDCCs[,1] <- cur.segments$event.id
    
    WriteCache(TDCCs, cache.id)
    
    return(TDCCs)
    
    
}





ExtractTDCCs <- function (seg.spectro, window, num.coefficients) {
    # given a spectrogram that has width = window length, 
    # will do a fft on each frequency bin
    # coefficients higher than num.coefficients will be discarded
    
    seg.spectro <- Normalize(seg.spectro)
    
    width <- ncol(seg.spectro)
    
    seg.spectro <- t(seg.spectro) * window
    
    # each column corresponds to a frequency band
    # and consists of the same number of coefficients as the window width
    
    # Mod removes the imaginary part (phase)
    TDCCs <- Mod(mvfft(seg.spectro))      
    
    # remove one of the symetrical halves
    # we now have the number of rows = half the frame width
    # top rows are the low frequencies (as in, frequency with which the amplidude modulates for that frequency band within the window)
    amp <- TDCCs[1:(width / 2), ]
    
    # amplitude modulation frequency of row x = ((width/2) - x + 1) / (width/2) cycles per time frame
    # eg, row 1 = 1 cycle per time frame
    # eg, if width = 512, row 256 = 256 cycles per time frame 
    
    # keep only low-frequency coefficients
    amp <- amp[1:num.coefficients,]  
    
    # return vector coefficient 1 - num.coefficients for bin 1, coefficient 1 - num.coefficients for bin 2 etc
    amp <- as.vector(amp)
    
    return(amp)
    
    
}




RoundToPow2 <- function (x, ceil = FALSE, floor = FALSE) {
    # rounds the number x off to the nearest power of 2
    # if ceil is TRUE, will round up, else if floor is true will round down
    if (ceil) {
        r <- 2^(ceiling(log2(x)))
    } else if (floor) {
        r <- 2^(floor(log2(x))) 
    } else {
        r <- 2^(round(log2(x)))
    }
    return(r)   
}



test1 <- function () {
    
    spectro <- Sp.CreateTargeted('NW', '2010-10-17', 400*60, 10)
    
    # don't use mel scale, stupid
    mel.spectrum <- audspec(pspectrum = spectro$vals, sr = spectro$sample.rate, nfilts = 26, fbtype = 'mel')
    
    return(mel.spectrum)
    
}



# probably useless function
# it was adapded from using mel-wights, but for 
# MakeLinearWeights <- function  (w, h) {
#     m <- matrix(0, nrow = w, ncol = h)
#     slope <- h/w
#     ys <- 1:w * slope
#     for (c in 1:w) {
#         col.vals <- rows     
#     } 
# }
