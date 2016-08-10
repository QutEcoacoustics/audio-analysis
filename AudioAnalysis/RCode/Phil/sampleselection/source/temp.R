
testWave <- function (n = 10000) {
    
    w <- matrix(ncol = 5, nrow = n)
    
    # range [-1,1]
    w[,1] <- sin(1:n*3) * seq(1,0,length.out = n)
    w[,2] <- sin(1:n/3)
    w[,3] <- sin(1:n/5) * seq(0,1,length.out = 10000)
    w[,4] <- sin(1:n/9)
    
    w[,5] <- sample(1:200-100, n, replace = TRUE) / 200 
    
    all <- apply(w, 1, sum) / ncol(w)
    
    return(all)
    
}




mySpec <- function (wave, f, wl = 512, wn = "hanning", fftw = FALSE, norm = TRUE, 
          PSD = FALSE, PMF = FALSE, dB = NULL, dBref = NULL, at = NULL, 
          from = NULL, to = NULL, identify = FALSE, col = "black", 
          cex = 1, plot = 1, flab = "Frequency (kHz)", alab = "Amplitude", 
          flim = NULL, alim = NULL, type = "l", ...) 
{
    if (!isTRUE(norm) & PMF) 
        stop("'PMF' can be computed only if 'norm' is TRUE")
    if (!isTRUE(norm) & !is.null(dB)) 
        stop("dB are computed on normalised spectra only, 'norm' should be turned to TRUE")
    if (!is.null(dB) & PMF) 
        stop("PMF cannot be in dB")
    if (!is.null(dB) & PSD) 
        stop("PSD cannot be in dB")
    if (is.null(dB) & !is.null(dBref)) 
        stop("'dB' cannot be NULL  when 'dBref' is not NULL")
    if (is.logical(dB)) 
        stop("'dB' is no more a logical. Please see the documentation: help(spec).")
    if (!is.null(dB) && all(dB != c("max0", "A", "B", "C", "D"))) 
        stop("'dB' has to be one of the following character strings: 'max0', 'A', 'B', 'C' or 'D'")
    if (!is.null(wl) & wl%%2 == 1) 
        stop("'wl' has to be an even number.")
    input <- inputw(wave = wave, f = f)
    wave <- input$w
    f <- input$f
    rm(input)
    if (!is.null(from) | !is.null(to)) {
        if (is.null(from) && !is.null(to)) {
            a <- 1
            b <- round(to * f)
        }
        if (!is.null(from) && is.null(to)) {
            a <- round(from * f)
            b <- length(wave)
        }
        if (!is.null(from) && !is.null(to)) {
            if (from > to) 
                stop("'from' cannot be superior to 'to'")
            if (from == 0) {
                a <- 1
            }
            else a <- round(from * f)
            b <- round(to * f)
        }
        wl <- (b - a) + 1
        wave <- as.matrix(wave[a:b, ])
    }
    if (!is.null(at)) {
        c <- round(at * f)
        wl2 <- wl%/%2
        wave <- as.matrix(wave[(c - wl2):(c + wl2), ])
    }
    n <- nrow(wave)
    W <- ftwindow(n, wn = wn)
    wave <- wave * W
    if (fftw == FALSE) {
        y <- Mod(fft(wave[, 1]))
    }
    else {
        p <- fftw::planFFT(n)
        y <- Mod(fftw::FFT(wave[, 1], plan = p))
    }
    y <- y[1:(n%/%2)]
    if (norm) {
        y <- y/max(y)
    }
    y <- ifelse(y == 0, yes = 1e-06, no = y)
    x <- seq(0, (f/2) - (f/wl), length.out = n%/%2)/1000
    if (PSD) 
        y <- y^2
    if (PMF) 
        y <- y/sum(y)
    if (!is.null(dB)) {
        if (is.null(dBref)) {
            y <- 20 * log10(y)
        }
        else {
            y <- 20 * log10(y/dBref)
        }
        if (dB != "max0") {
            if (dB == "A") 
                y <- dBweight(x * 1000, dBref = y)$A
            if (dB == "B") 
                y <- dBweight(x * 1000, dBref = y)$B
            if (dB == "C") 
                y <- dBweight(x * 1000, dBref = y)$C
            if (dB == "D") 
                y <- dBweight(x * 1000, dBref = y)$D
        }
    }
    if (is.null(alim)) {
        if (is.null(dB)) {
            alim <- c(0, 1.1)
        }
        else {
            alim <- c(min(y, na.rm = TRUE), max(y, na.rm = TRUE) + 
                          20)
        }
        if (PMF | !isTRUE(norm)) 
            alim <- c(0, max(y, na.rm = TRUE))
    }
    if (plot == 1) {
        if (!is.null(dB)) {
            plot(x = x, y = y, xaxs = "i", xlab = flab, xlim = flim, 
                 yaxs = "i", yaxt = "s", ylab = alab, ylim = alim, 
                 col = col, cex = cex, type = type, las = 1, ...)
        }
        else {
            if (isTRUE(norm)) {
                yaxt <- "n"
                ylab <- alab
                if (isTRUE(PMF)) {
                    yaxt = "s"
                }
            }
            else {
                yaxt <- "s"
                ylab <- " "
            }
            plot(x = x, y = y, xaxs = "i", xlab = flab, xlim = flim, 
                 yaxs = "i", yaxt = yaxt, ylab = ylab, ylim = alim, 
                 col = col, cex = cex, type = type, las = 1, ...)
        }
        if (identify) {
            cat("Choose points on the spectrum\n")
            if (.Platform$OS.type == "windows") 
                flush.console()
            id <- identify(x = x, y = y, labels = round(x, 2), 
                           tolerance = 0.15, col = "red")
            id.freq <- x[id]
            id.amp <- y[id]
            coord <- list(freq = id.freq, amp = id.amp)
            return(coord)
        }
    }
    if (plot == 2) {
        if (!is.null(dB)) {
            plot(x = y, y = x, xaxs = "i", xlab = alab, xlim = alim, 
                 yaxs = "i", yaxt = "s", ylab = flab, ylim = flim, 
                 col = col, cex = cex, type = type, las = 1, ...)
        }
        else {
            if (isTRUE(norm)) {
                xaxt <- "n"
                xlab <- alab
                if (isTRUE(PMF)) {
                    xaxt = "s"
                }
            }
            else {
                xaxt <- "s"
                xlab <- " "
            }
            plot(x = y, y = x, xaxs = "i", xaxt = xaxt, xlab = xlab, 
                 xlim = alim, yaxs = "i", ylab = flab, ylim = flim, 
                 col = col, cex = cex, type = type, las = 1, ...)
        }
        if (identify) {
            cat("choose points on the spectrum\n")
            if (.Platform$OS.type == "windows") 
                flush.console()
            id <- identify(x = y, y = x, labels = round(x, 2), 
                           tolerance = 0.15, col = "red")
            id.freq <- x[id]
            id.amp <- y[id]
            coord <- list(freq = id.freq, amp = id.amp)
            return(coord)
        }
    }
    if (plot == 1 | plot == 2) {
        spec <- cbind(x, y)
        invisible(spec)
    }
    else if (plot == FALSE) {
        spec <- cbind(x, y)
        return(spec)
    }
}