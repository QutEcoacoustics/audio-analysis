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
                               duration, img.path = NA, rects = NULL, 
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

Sp.CreateFromFile <- function (path, draw = FALSE) {

    
    split <- strsplit(pangram, .Platform$file.sep)
    cache.id <- paste0(split[length(split)], '.spectro')
    
    spectro <- ReadCache(cache.id)
    if (class(spectro) != 'spectrogram') {
        spectro <- Sp.Create(path, draw = draw)
        WriteCache(spectro, cache.id) 
    } else {
        Report(5, 'using spectrgram retrieved from cache')
    }
    return(spectro)
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



Sp.Draw <- function (spectro, img.path = NA, scale = 2) {
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
        png(img.path, width = width*scale, height = height*scale)
    } else {
        
    }
    rast <- Sp.AmpToRaster(amp)

    # create a viewport positioned with cms, so that resizing the device doesn't resize the viewport
    # calculate the cm value of the pixel amount needed (rows, cols)
    grid.newpage()
    devsize.cm <- dev.size(units = "cm")
    devsize.px <- dev.size(units = "px")
    px.per.cm <- (devsize.px[1] / devsize.cm[1]) / scale
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
        Sp.Rect(spectro, rect)
      }
    }
    
    Sp.DrawLines(spectro)

    
    if (!is.na(img.path)) {
        dev.off()
    }
    
    
}


Sp.DrawLines.1 <- function (spectro) {  
    if(!is.null(spectro$lines) && nrow(spectro$lines) > 0) {       
        pallete.size <- 20    
        palette(rainbow(pallete.size))     
        # only show the top scoring half
        # spectro$lines <- spectro$lines[spectro$lines$color > 1 ,]
        for (i in 1:nrow(spectro$lines)) {
            # add lines           
            spectro$lines$color <- ceiling(Normalize(spectro$lines$score) * 10)          
            line <- spectro$lines[i, ]
            Sp.AddLine(spectro, line)          
        }
    }   
}

Sp.DrawLines <- function (spectro) {  
    if(!is.null(spectro$lines) && length(spectro$lines) > 0) {       
        palette.size <- 20
        max.score <- 200   # set this to something which will easily be bigger than the highest socre        
        palette(rainbow(palette.size))
        color.converter <- palette.size / max.score
        # only show the top scoring half
        # spectro$lines <- spectro$lines[spectro$lines$color > 1 ,]
        branches <- c('branch.1', 'branch.2')
        branch.cols <- c('red', 'green')
        for (i in 1:length(spectro$lines)) {           
            center <- spectro$lines[[i]]$center           
            for (b in 1:2) {
                
                b.name <- branches[b]
                col <- branch.cols[b]
                
                Sp.Drawbranch(spectro, center, spectro$lines[[i]][[b.name]], color.converter, col)
            }     
        }
    }   
}


Sp.Drawbranch <- function (spectro, start, branch, color.converter, color) {
    
    if (nrow(branch) < 1) {
        return()
    }
    
    from.row <- start[1]
    from.col <- start[2]
    
    for (i in 1:nrow(branch)) {
        
        #color <- round(branch$score[i] * color.converter)
        Sp.AddLine(spectro, 
                   from.row = from.row, 
                   from.col = from.col, 
                   to.row = branch$row[i],
                   to.col = branch$col[i],
                   color = color
        )
        from.row <- branch$row[i]
        from.col <- branch$col[i]
        
    }
    
}

Sp.AddLine <- function (spectro, from.col, to.col, from.row, to.row, color) {    
    # convert units 
    x.start <- from.col / ncol(spectro$vals)
    y.start <- from.row / nrow(spectro$vals)
    x.end <- to.col / ncol(spectro$vals)
    y.end <- to.row / nrow(spectro$vals)
    grid.lines(x = unit(c(x.start, x.end), "npc"),
               y = unit(c(y.start, y.end), "npc"),
               default.units = "npc",
               arrow = NULL, name = NULL,
               gp=gpar(col = color), draw = TRUE, vp = NULL)
}



Sp.AddLine.1 <- function (spectro, line) {
    
    r <- 4
    
    # find start and end of the line
    x.start <- line$col + cos(line$angle) * r
    x.end <- line$col - cos(line$angle) * r
    
    # be careful with the y values. Remember, row zero goes to the bottom of the spectrogram
    # but the angle is measured with row zero at the TOP
    y.start <- line$row - sin(line$angle) * r
    y.end <- line$row + sin(line$angle) * r
    
    
    # convert units 
    x.start <- x.start / ncol(spectro$vals)
    y.start <- y.start / nrow(spectro$vals)
    x.end <- x.end / ncol(spectro$vals)
    y.end <- y.end / nrow(spectro$vals)
    
    
    #quality <- round(line$val)
    
    # red = low, green = high
    
    
    grid.lines(x = unit(c(x.start, x.end), "npc"),
               y = unit(c(y.start, y.end), "npc"),
               default.units = "npc",
               arrow = NULL, name = NULL,
               gp=gpar(col = line$color), draw = TRUE, vp = NULL)
    
    
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

Sp.Rect <- function (spectro, rect) {
    
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
    
    x <-  unit(rect$start.sec / spectro$duration, "npc")
    width <- unit(rect$duration / spectro$duration, "npc")
    y <- unit(rect$top.f / spectro$frequency.range, "npc")
    height <- unit(((rect$top.f - rect$bottom.f) / 
                   spectro$frequency.range), "npc")
    
    if (!is.null(rect$rect.color)) {
        rect.col <- as.character(rect$rect.color)
    } else {
        rect.col <- 'green'  # default
    }
    
    fill.alpha <- 0.1
    line.alpha <- 0.9
    text.alpha <- 0.7
    
    if (!is.null(rect$label.tl)) {
        name <- rect$label.tl 
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
    
    if (!is.null(rect$label.tl)) {
        text.txt <- rect$label.tl
        grid.text(text.txt, x , y, 
                  gp = text.gp,
                  just = c('left', 'top')
                  )
    }
    
    if (!is.null(rect$label.br)) {
        text.txt <- rect$label.br
        grid.text(text.txt, x + width , y - height, 
                  gp = text.gp,
                  just = c('right', 'bottom')
        )
    }
    
    
    
    
    
}

DrawLongSpectrogram <- function (site, date, 
                                 start.sec, duration, 
                                 output.path,  
                                 segment.size = 600, 
                                 vertical = FALSE, 
                                 jpeg.quality = 80,
                                 scale = 0.5) {
    # creates a long spectrogram by creating several small ones then
    # stitching together the images with image magik
    
    # make sure duration is a multiple of segment size
    extra <- duration %% segment.size
    duration <- duration - extra
    
    start.secs <- seq(start.sec, start.sec + duration, segment.size)
    
    fn.root <- paste0('tmp', as.character(as.integer(Sys.time())))
    
    temp.dir <- TempDirectory()
    
    fns <- paste0(fn.root, 1:length(start.secs), '.png')
    
    paths <- file.path(temp.dir, fns)
    
    for (ss in 1:length(start.secs)) {       
        spectro <- Sp.CreateTargeted(site, date, start.secs[ss], segment.size)
        Sp.Draw(spectro, img.path = paths[ss])   
    }
    
    StitchImages(image.paths = paths, 
                 output.fn = output.path, 
                 vertical = vertical, 
                 jpeg.quality = jpeg.quality,
                 resize = (scale * 100))
    
}


StitchImages <- function (image.paths, output.fn, vertical = TRUE, jpeg.quality = NA, resize = 100) {      
    fns <- paste(image.paths, collapse = " ")
    if (vertical) {
        append <- "-append"
    } else {
        append <- "+append"
    }
    

    
    command <- paste("/opt/local/bin/convert", 
                     fns, append)
    
    if (!is.na(jpeg.quality)) {
        command <- paste(command, '-quality', jpeg.quality)
    }
    
    if (!is.na(resize) && is.numeric(resize) && resize > 0 && resize != 100) {
        command <- paste0(command, ' -resize ', resize, "%")
    }
    
    command <- paste(command, output.fn)
    
    Report(5, 'doing image magic command', command)
    err <- try(system(command))  # ImageMagick's 'convert'
    Report(5, err)
}


