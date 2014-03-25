TS <- function (s = NA) {
    if (is.na(s)) {
        s <- Sp.Create('/Users/n8933464/Documents/SERF/NW/test/3.wav')
    }
    s$label = "test label"
    #Sp.Draw(s, 'test.png')
    #Sp.Draw(s)
    return(s)
}



Sp.CreateTargeted <- function (site, start.date, start.sec, 
                               duration, img.path = NA, rects = NA, 
                               audio.source.in.label = TRUE, label = NA) {
    # creates a spectrogram of audio given the start, duration and site 
    #
    # Args:
    #   site: string; 
    #   start.date: string or date object
    #   start.sec: float; the number of seconds from the start of start.date
    #   duration: float; the number of seconds for the spectrogram
    #   img.path: where the spectrogram is saved
    #   rects: table of rectangles to add to the spectrogram
    #   audio.source.in.label: boolean; whether to put the site, date, start and end time in the label
    #
    # Details:
    # this is the process:
    # 1. identify the file start
    # 2. identify whether the target spans more than 1 file
    # 3. idenify the sample number for the start and end
    
    cache.id <- paste(site, start.date, start.sec, duration, 'spectro', sep = '.')
    
    spec <- ReadCache(cache.id)
    
    
    # only cache the spectrogram without the rectangles
    # because the point is to be able to change the rectanles without
    # regenerating the spectrogram
    if (class(spec) != 'spectrogram') {
        
        w <- Audio.Targeted(site, start.date, start.sec, duration)
        # w <- Wave(left = wav.samples, right = numeric(0), 
        #           samp.rate = samp.rate, bit = bit)
        spec <- Sp.Create(w)
        WriteCache(spec, cache.id) 
    } else {
        Report(5, 'using spectrgram retrieved from cache')
    }
        
        
    spec$rects <- rects
    # add the label argument to the default label text (date time site)
        
    if (audio.source.in.label) {
        spec$label <- paste(site, start.date, SecToTime(start.sec), " - ", SecToTime(start.sec + duration)) 
    }
    if (!is.na(label)) {
        spec$label <- paste(spec$label, label, sep = " : ") 
    }
    if (!is.na(img.path)) {
        Sp.Draw(spec, img.path)      
    }
        
     return(spec)
    
}

Sp.Create <- function(wav, frame.width = 512, draw = FALSE, 
                      smooth = TRUE, db = TRUE, filename = FALSE) {
    # performs a stft on a mono wave file (no overlap)
    #  
    # Args:
    #   wav: String; path to the wav file OR a TuneR wave object
    #   frame.width: Int; number of samples per time-frame
    #   draw: boolean; Draw the spectrogram or just return the values
    #   smooth: boolean; Whether to perform smoothing
    #   db: boolean; Whether to convert to db (log). 
    #     Without this, low values will be imperceptible
    #   filename: string or FALSE; the path where to save the image file, 
    #     or FALSE to not save the spectrogram
    #
    # Returns:
    #   spectrogram; (custom object) each column is a time-frame, 
    #     each row is a frequency bin \n
    
    Report('Generating spectrogram')
    ptm <- proc.time()
    #read and shape the original signal\n
    TFRAME <- frame.width
    hamming <- 0.54 - 0.46 * cos(2 * pi * c(1:TFRAME) / (TFRAME - 1))
    # wav can be a path string or a tuneR wav object. 
    # If it's a string then create a tuneR wav object
    if (typeof(wav) == "character") {
        library(tuneR)
        wav <- readWave(wav)
    }
    samp.rate <- wav@samp.rate
    bit <- wav@bit  # resolution of wave eg 16bit
    left <- wav@left  # sample values
    len <- length(left)  # total number of samples
    #trim samples so that TFRAME fits exactly
    sig <- left[c(1:(len - len %% TFRAME))]
    #normalise by the maximum signed value of 16 bit
    sig <- sig / (2 ^ bit / 2)
    #number of frames
    nframe <- length(sig) / TFRAME
    #split into frames. each frame is a column
    # each column contains wave signal data in time domain
    dim(sig) <- c(TFRAME, nframe)
    # perform fft on each of the time frames
    # use Mod to remove imaginary part (phase), leaving only amplitude
    sig <- Mod(mvfft(sig * hamming))
    # smooth the data\n
    if (smooth) {
        first.temp <- sig[1, ]
        sig <- filter(sig, rep(1 / 3, 3))
        sig[1, ] <- first.temp
    } 
    # remove one of the symetrical halves
    amp <- sig[c(1:(TFRAME / 2)), ]
    # clear the unnecessary variables
    rm(wav)
    rm(left)
    rm(sig)
    if (db) {
        amp <- ConvertToDb(amp)
    }
    spectro <- list(
        vals = amp, 
        duration = len / samp.rate, 
        frequency.range = samp.rate / 2, 
        sample.rate = samp.rate, 
        hz.per.bin = (samp.rate / (2 * nrow(amp))), 
        frames.per.sec = (ncol(amp)) / (len / samp.rate))
    
    class(spectro) <- "spectrogram"
    
    Timer(ptm, 'generating spectrogram',  len / samp.rate, 'second of audio')
    
    if (draw) {
        Sp.draw(spectro, filename)
    }
    return(spectro)
}






ConvertToDb <- function (amp, bit.resolution = 16) {
    # converts a matrix of energy levels to DB values
    # 
    # Args:
    #    amp: matrix; the values to convert
    #    bit.resolution: Int; Amplitude resolution of the wave
    #
    # Returns: 
    #     matrix
    #
    # TODO: adjust for extra energy added by hamming window
    
    # replace zero values with the minimum non-zero value
    # possible with the sampled bit resolution
    # this is necessary to avoid log(0) 
    min.val <- 1 / (2 ^ (bit.resolution - 1))
    amp[amp == 0] <- min.val
    amp <- log10(amp ^ 2)
    amp <- amp * 10
    return(amp)
}



Sp.Draw <- function (spectro, img.path = NA) {
    #  draws a spectrogram given a matrix of amplitudes
    #
    #  Args: 
    #    spectro: Spectrogram;
    #    img.path: string: where to save the image. if NA, then 
    #              outputs to screen
    #
    #  Returns:
    #    NULL
    require('grid')
    
    
    amp <- spectro$vals
    width <- ncol(amp)
    height <- nrow(amp)
    if (!is.na(img.path)) {
        png(img.path, width = width, height = height)
    } else {
        
    }
    rast <- Sp.AmpToRaster(amp)

    # create a viewport positioned with cms, so that resizing the device doesn't resize the viewport
    # calculate the cm value of the pixel amount needed (rows, cols)
    grid.newpage()
    devsize.cm <- dev.size(units = "cm")
    devsize.px <- dev.size(units = "px")
    px.per.cm <- devsize.px[1] / devsize.cm[1]
    vp.width.cm <- ncol(spectro$val) / px.per.cm
    vp.height.cm <- nrow(spectro$val) / px.per.cm
    vp <- viewport(x = 0, y = 0, just = c('left', 'bottom'), width = vp.width.cm, height = vp.height.cm, default.units = 'cm')
    pushViewport(vp)
    
    
    grid.raster(image = rast, vp = vp)

    

    Sp.Label(spectro)
    
    if (!is.null(spectro$rects) && nrow(spectro$rects) > 0) {
      for (i in 1:nrow(spectro$rects)) {
        # add rectangles
        rect <- spectro$rects[i, ]
        Sp.Rect(spectro, rect, labels = list(top.left = 'event.id', bottom.right = 'group'))
      }
    }
    

    
    if (!is.na(img.path)) {
        dev.off()
    }
    
    
}

Sp.AmpToRaster <- function (amp) {
    # normalise to {0,1}, reversing value so 
    # current max becomes min, and current min becomes max
    # flip upside down (high freq are low row nums i.e. near top)
    ma <- max(amp)
    mi <- min(amp)  
    #raster renders higher values as white, so lets inverse
    amp <- - (((amp - mi) / (ma - mi)) - 1)
    amp <- apply(amp, 2, rev) 
    rast <- as.raster(amp, max = 1)
    return(rast)
}

Sp.Label <- function (spectro) {
    # prints a label in white on a semi-transparent bg
    
  #  devsize <- dev.size(units = 'px')
  #  top.offset <- devsize[2] - nrow(spectro$val) * 0.5
  #  left.offset <- devsize[1] - ncol(spectro$val) * 0.5
    
    
    

    font.size <- 12
    bg.col <- 'black'
    bg.alpha <- 0.5
    text.col <- 'white'
    text.alpha <- 0.9
    padding <- 2
    char.width <- 6  # depends on font
    
    
    
    
    
    if (!is.null(spectro$label)) {    
        px.v <- 1 / nrow(spectro$val)  # equivalent to 1 px in the vertical 
        px.h <- 1 / ncol(spectro$val)  # 1 px in the horizontal    
        grid.rect(x = 0, 
              y = 1,
              width = (char.width * nchar(spectro$label) + (padding * 2)) * px.h, 
              height = (font.size + padding * 2) * px.v,
              hjust = 0, vjust = 1,
              default.units = "npc", name = NULL,
              gp = gpar(col = bg.col, fill = bg.col, alpha = bg.alpha), 
              draw = TRUE, 
              vp = NULL) 
        text.gp <- gpar(col = text.col, alpha = text.alpha, fontsize=font.size, fontfamily = 'Arial')
        x <- unit(padding * px.h, "npc")
        y <- unit(1 - padding * px.v, "npc")
        grid.text(label = spectro$label, x = x, y = y,
                  gp = text.gp,
                  hjust = 0, vjust = 1
        )
    }
    
    
    
}

Sp.Rect <- function (spectro, rect.borders, labels = list()) {
    
    #  top.pix <- ft * spectro[['hz.per.bin']]
    #  bottom.pix <- fb * spectro[['hz.per.bin']]
    #  left.pix <-  spectro[['frames.per.sec']] / start
    #  right.pix <- left.pix + spectro[['frames.per.sec']] / duration
    
    #    vals = amp, 
    #    duration = len/samp.rate, 
    #    frequency.range = samp.rate/2, 
    #    sample.rate = samp.rate, 
    #    hz.per.bin = (samp.rate/(2 * nrow(amp))), 
    #    frames.per.sec = (ncol(amp))/(len/samp.rate))
    
    x <-  unit(rect.borders$start.sec / spectro$duration, "npc")
    width <- unit(rect.borders$duration / spectro$duration, "npc")
    y <- unit(rect.borders$top.f / spectro$frequency.range, "npc")
    height <- unit(((rect.borders$top.f - rect.borders$bottom.f) / 
                   spectro$frequency.range), "npc")
    
    if (!is.null(rect.borders$rect.color)) {
        rect.col <- as.character(rect.borders$rect.color)
    } else {
        rect.col <- 'green'  # default
    }
    
    fill.alpha <- 0.1
    line.alpha <- 0.9
    text.alpha <- 0.7
    
    if (is.null(labels$top.left)) {
        name <- labels$top.left 
    } else {
        name <- NULL
    }
    
    
    # 2 rectangles, one for fill and one for line
    # to allow the fill and lines to have different alpha 
    grid.rect(x = x, 
              y = y,
              width = width, 
              height = height,
              hjust = 0, vjust = 1,
              default.units = "npc", name = name,
              gp = gpar(col = rect.col, fill = rect.col, alpha = fill.alpha), 
              draw = TRUE, 
              vp = NULL)
    
    grid.rect(x = x, 
              y = y,
              width = width, 
              height = height,
              hjust = 0, vjust = 1,
              default.units = "npc", name = NULL,
              gp = gpar(col = rect.col, fill = NA, alpha = line.alpha), 
              draw = TRUE, 
              vp = NULL) 
    

    text.gp <- gpar(col = rect.col, alpha = text.alpha);
    
    if (!is.null(labels$top.left)) {
        text.txt <- rect.borders[[labels$top.left]]
        grid.text(text.txt, x , y, 
                  gp = text.gp,
                  just = c('left', 'top')
                  )
    }
    
    if (!is.null(labels$bottom.right)) {
        text.txt <- rect.borders[[labels$bottom.right]]
        grid.text(text.txt, x + width , y - height, 
                  gp = text.gp,
                  just = c('right', 'bottom')
        )
    }
    
    
    
    
    
}




TR <- function (threshold = 1) {
    # test ridges
    sp <- TS()
    #amp <- sp$vals[101:120, 51:70]
    amp <- sp$vals
    edges <- amp
    for (i in 1:1) {
        edges <- Sp.Ridge(edges)
    }
    amp <- Normalize(amp)
    edges <- Normalize(edges)
    both <- rbind(amp, edges)
    image(t(both))
    return(both)
    
}
source('../../../liang/Acoustic Indices/ridgeDetectionS.R')
Sp.Ridges <- function (amp, threshold) {
    
    require('biOps')
    
    x <- imagedata(amp)
    #sigma <- threshold
    #edges <- imgCanny(x, sigma)
    m <- matrix(c(1,2,1,2,4,2,1,2,1)/16, 3, 3, byrow = TRUE)
    
    
    
    edges <- imgConvolve(x, m, 32)
    
    
    return(edges)
    
}

Sp.Ridge.1 <- function (amp) { 
    
    
    before.blur <- amp
    w <- ncol(amp)
    h <- nrow(amp)
    amp <- Blur(amp)
    left.delta <- amp[,2:(w-1)] - amp[,1:(w-2)]
    right.delta <- amp[,2:(w-1)] - amp[,3:w]
    ridges <- left.delta * right.delta * (left.delta + right.delta);
    ridges[ridges <= 0] <- 0
    ridges[ridges > 0] <- 1
    border <- rep(0, nrow(ridges))
    ridges <- cbind(border, ridges, border)
    return(ridges)
}

TR2 <- function () {
    
    sp <- TS()
    amp <- sp$vals
    #amp <- sp$vals[101:120, 51:70]
    w <- 11
    h <- 5
    

    
    amp2 <- Normalize(amp)
    
    iteration.widths <- c(13, 11, 9, 7, 5, 3)
    
    for (w in iteration.widths) {
        m <- rep(-1/(w-1), w)
        m[ceiling(w/2)] <- 1  
        m <- matrix(rep(m, h), nrow = h, byrow = TRUE) 
        
        image(t(amp2))
        ridges <- Convolve(amp2, m)
        amp2[ridges < sd(ridges)] <- 0
        amp2 <- Normalize(amp2)

    }
    
    image(t(rbind(Normalize(amp), amp2)))
    
}


Sp.Ridge <- function (amp) {
    amp <- Normalize(amp)
    
    left.v <- c(-0.3, -0.3, -0.4, 1, 0, 0, 0)
    left <- matrix(rep(left.v, 5), nrow = 5, byrow = TRUE)
    right.v <- rev(left.v)
    right <- matrix(rep(right.v, 5), nrow = 5, byrow = TRUE)

    ridge.right <- Convolve(amp, right)
    ridge.left <- Convolve(amp, left)
    
    #standard deviation for each freq
    #sds <- apply(sd, 2, ridge.right)
    
    threshold <- sd(ridge.right)
    
    #m <- matrix(c(-1,-1,-1,0,1,1,1))

    ridge <- ridge.left > threshold & ridge.right > threshold
    
    amp[!ridge] <- 0
    
    return(amp)
    
    

}

Convolve <- function (amp, mask = NA) {
    w <- ncol(amp)
    h <- nrow(amp)
    if (class(mask) != 'matrix') {
        mask <- matrix(c(1,2,1,2,4,2,1,2,1)/16, 3, 3, byrow = TRUE)  
    }
    amp2 <- ExpandMatrix(amp, floor(nrow(mask)/2), floor(ncol(mask)/2))
    total <- matrix(0, nrow = nrow(amp), ncol = ncol(amp))
    for (rr in 1:nrow(mask)) {
        for (cc in 1:ncol(mask)) {
            offset <- amp2[rr:(rr+h-1), cc:(cc+w-1)]
            total <- total + (offset * mask[rr, cc])   
        }
    }
    return(total)
}

ExpandMatrix <- function (m, rr, cc) {
    # copies the edge rows and columns by rr and cc respectively
    if (cc > 0) {
        m <- cbind(matrix(rep(m[,1], cc), ncol = cc), m,  matrix(rep(m[,ncol(m)], cc), ncol = cc) )    
    }
    if (rr > 0) {
        m <- rbind(matrix(rep(m[1,], rr), byrow = TRUE, nrow = rr), m,  matrix(rep(m[nrow(m),], rr), byrow = TRUE, nrow = rr) )
    }
    return(m)
}

ShiftMatrix <- function (m, rr, cc) {
    if (cc < 0) {
        m <- cbind(matrix(rep(m[,1], -cc), ncol = -cc), m)
        m <- m[,-(ncol(m):(ncol(m)+(cc+1)))]
    } else if (cc > 0) {
        m <- cbind(m, matrix(rep(m[,ncol(m)], cc), ncol = cc))
        m <- m[,-(1:cc)]
    }
    if (rr < 0) {
        m <- rbind(matrix(rep(m[1,], -rr), nrow = -rr, byrow = TRUE), m)
        m <- m[-(nrow(m):(nrow(m)+(rr+1))), ]
    } else if (rr > 0) {
        m <- rbind(m, matrix(rep(m[nrow(m),], rr), nrow = rr, byrow = TRUE))
        m <- m[-(1:rr), ]
    }
    
    return(m)
}

