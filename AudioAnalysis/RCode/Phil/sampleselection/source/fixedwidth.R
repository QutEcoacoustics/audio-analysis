MakeSegmentList <- function (min.list = NULL, num.per.min = 60) {
    
    if (is.null(min.list)) {
        min.list <- ReadOutput('target.min.ids')
        min.list <- min.list$data
    }
    
    
    min.list.per.10 <- min.list[min.list$min %% 10 == 0, ]
    
    wave.path.per.10 <- rep(NA, nrow(min.list.per.10))
    for (i in 1:length(wave.path.per.10)) {
        wave.path.per.10[i] <- GetAudioFile(min.list.per.10$site[i], min.list.per.10$date[i], min.list.per.10$min[i])
    }  
        
    min.list$wave.path <- wave.path.per.10[rep(1:length(wave.path.per.10),each=10)]
    
    segment.list <- min.list[rep(1:nrow(min.list),each=num.per.min), ]
    
    segment.list$seg.num <- rep(1:num.per.min, nrow(min.list))
    
    segment.list$start.sec <- (segment.list$seg.num - 1) * (60/num.per.min)
    
    return(segment.list)
    
    
}


ExtractSDF <- function (segment.list) {
    
    #for each segment 
        # find the file
        # check if it's spectrogram has been created
        # if not
            # create the spectrogram
        # get the right segment of the spectrogram
        # get transforms
        # add to dataframe
    
    
    
}


ExtractSDF <- function (segment.list, segment.length = 1, num.fbands = 16) {
    
    files <- unique(segment.list$wave.path)
    
    
    for (f in 1:length(files)) {
        
        # create the spectrogram
        cur.spectro <- Sp.CreateFromFile(files[f], frame.width = num.fbands*2)
        
        cur.spectro$vals <- RemoveNoise(cur.spectro$vals)
        
        # width of segment should be rounded down to the nearest power of 2 (for fft)
        width <- RoundToPow2(segment.length * cur.spectro$frames.per.sec, floor = TRUE)
        
        # hamming  window
        hamming <- 0.54 - 0.46 * cos(2 * pi * c(1:width) / (width - 1))
        
        # find the segments for this file
        segs <- segment.list$wave.path == files[f]
        
        # add the segment.width (number of columns)
        seg.width <- cur.spectro$frames.per.sec * segment.length  
        
        # add the start.sec.in.file
        start.sec.in.file <- (segment.list$min[segs] %% 10) * 60 + segment.list$start.sec[segs]
        
        # add the start.col.in.file
        start.col.in.file <- start.sec.in.file * cur.spectro$frames.per.sec + 1
        
        
        
        
        
        for (s in 1:sum(segs)) {
            
            seg <- segment.list[segs, ][s, ]
            
            # make multople of 2
            start.col <- start.col.in.file[s]
            end.col <- start.col + width - 1
            seg.spectro <- cur.spectro$vals[,start.col:end.col]
            seg.spectro <- seg.spectro * hamming
            SDFs <- Mod(mvfft(seg.spectro * hamming))
            
            
            
            
        }
        
        
    }
    
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


RemoveNoise <- function (spectro) {
    # aggressive noise removal
    
    # normalise
    
    spectro <- Normalize(spectro)
    
    
    # for each row, calculate the median of it and it's neighbouring rows
    
    
    
    
    med <- rep(NA, nrow(spectro))
    s1 <- rbind(spectro[1,], spectro)
    s1 <- rbind(s1, s1[nrow(s1),])
    for (i in 1:(length(med))) {    
        med[i] <- median(s1[i:(i+2),], na.rm = TRUE)
    }
    
    med <- matrix(med, nrow = nrow(spectro), ncol = ncol(spectro))
    
    rem <- spectro < med
    spectro[rem] <- 0
    
    return(spectro)
    
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
