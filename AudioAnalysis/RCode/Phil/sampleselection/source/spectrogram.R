TS <- function (s = NA) {
    # if (is.na(s)) {
    #     s <- Sp.Create('/Users/n8933464/Documents/SERF/NW/test/test.wav')
    # }
    
    Sp.draw(s, '../output/test/s2.png')
}



Sp.CreateTargeted <- function (site, start.date, start.sec, 
                               duration, img.path = NA, rects = NA) {
    # creates a spectrogram of audio given the start, duration and site 
    #
    # Args:
    #   site: string; 
    #   start.date: string or date object
    #   start.sec: float; the number of seconds from the start of start.date
    #   duration: float; the number of seconds for the spectrogram
    #   img.path: where the spectrogram is saved
    #   rects: table of rectangles to add to the spectrogram
    #
    # Details:
    # this is the process:
    # 1. identify the file start
    # 2. identify whether the target spans more than 1 file
    # 3. idenify the sample number for the start and end
    

    
    w <- Audio.Targeted(site, start.date, start.sec, duration)
   # w <- Wave(left = wav.samples, right = numeric(0), 
   #           samp.rate = samp.rate, bit = bit)
    spec <- Sp.Create(w)
    spec$rects <- rects
    spec$label <- paste(site, start.date, SecToTime(start.sec), " - ", SecToTime(start.sec + duration))
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
    # wav can be a string or a tuneR wav object. 
    # If it's a string then create a tuneR wav object
    if (typeof(wav) == "character") {
        library(tuneR)
        wav <- readWave(wav)
    }
    samp.rate <- wav@samp.rate
    bit <- wav@bit  # resolution of wave eg 16bit\n
    left <- wav@left  # sample values\n
    len <- length(left)  # total number of samples\n
    #trim samples so that TFRAME fits exactly\n
    sig <- left[c(1:(len - len %% TFRAME))]
    #normalise by the maximum signed value of 16 bit\n
    sig <- sig / (2 ^ bit / 2)
    #number of frames\n
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
    }
    rast <- Sp.AmpToRaster(amp)
    grid.raster(image = rast)
    if (!is.null(spectro$rects) && nrow(spectro$rects) > 0) {
      for (i in 1:nrow(spectro$rects)) {
        # add rectangles
        rect <- spectro$rects[i, ]
        Sp.Rect(spectro, rect, labels = list(top.left = 'event.id', bottom.right = 'group'))
      }
    }
    
    if (!is.null(spectro$label) && nrow(spectro$rects) > 0) {
        text.gp <- gpar(col = 'white', alpha = 1);
        text.txt <- spectro$label
        grid.text(text.txt, 0 , 0, 
                  gp = text.gp,
                  just = c('left', 'top')
        )
    
    
    }
    
    if (!is.na(img.path)) {
        dev.off()
    }
    
    
}

Sp.AmpToRaster <- function (amp) {
    ma <- max(amp)
    mi <- min(amp)  
    #raster renders higher values as white, so lets inverse
    amp <- - (((amp - mi) / (ma - mi)) - 1)
    amp <- apply(amp, 2, rev) 
    rast <- as.raster(amp, max = 1)
    return(rast)
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


    

    
    # 2 rectangles, one for fill and one for line
    # to allow the fill and lines to have different alpha 
    grid.rect(x = x, 
              y = y,
              width = width, 
              height = height,
              hjust = 0, vjust = 1,
              default.units = "npc", name = NULL,
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





