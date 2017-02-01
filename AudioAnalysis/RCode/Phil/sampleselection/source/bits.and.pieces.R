# plot some examples of the function used for temporal spreading

# discrete fourier transform for testing
# not efficient, use stats::fft instead
dft0 <- function (x, use.window = 'hamming') {
    
    N <- length(x)
    n <- 1:N-1
    x <- x * window(N, use.window)
    res <- sapply(n, function (k) { 
        sum(x * (exp(-1i*2*pi*(n)*k/N))) 
        })
    #print(paste('ok? ',all.equal(res,fft(x))))
    
    return(res)
    
}

#dct-II for testing
dct0 <- function (x, use.window = 'hamming') {
    
    N <- length(x)
    n <- 1:N-1
    x <- x * window(N, use.window)
    res <- sapply(n, function (k) {
        sum(x * cos((pi/N)*(n+0.5)*k)) 
    })
    
    return(res)
    
}

window <- function (length, type) {

    n <- 1:length-1
    if (type == 'hamming') {
        
        return(0.54 - 0.46*cos(2*pi*n/(length - 1)))
        
    } 
    
    # default, no window
    return(rep(1,length))
    
    
}


compareDctDft <- function (x, use.window = 'hamming', use.log = FALSE) {
    
    dct.res <- dct.res.2 <- abs(dct0(x, use.window))
    dft.res <- dft.res.2 <- Mod(dft0(x, use.window))
    
    N <- length(x)
    

    odd.nums <- (1:(ceiling(N/2))*2)-1
    even.nums <- (1:(floor(N/2))*2)

    # show only every second dft value
    #dft.res.2 <- rep(NA, N)
    #dft.res.2[odd.nums] <- dft.res[1:length(odd.nums)]
    
    # only show every second dct value, and chop half of dft
    
    N2 <- length(even.nums)
    

    
    dct.res.2 <- dct.res[even.nums]
    dft.res.2 <- dft.res[1:N2]
    
    if (use.log) {
        dct.res.2 <- 20*log10(dct.res.2)
        dft.res.2 <- 20*log10(dft.res.2)
    }
    
    mini <- min(c(dft.res.2,dct.res),na.rm = TRUE)
    maxi <- max(c(dft.res.2,dct.res),na.rm = TRUE)
    
    x.range <-c(0,N2-1)
    y.range <- c(mini, maxi)
        
    
    plot(x.range, y.range, type='n')
    
    x.vals <- x.range[1]:x.range[2]
    
    points(x.vals,dft.res.2, col="red", pch=3)
    points(x.vals,dct.res.2, col="blue", pch=1)
    
    
    
}




PlotExamplesOfTemporalSpreading <- function (thresholds = c(30, 20), amounts = c(1, 0.4), width = 100) {
    
    distance.scores <- 0:width
    
    ltys <- 2:6
    lwd <- 2
    
    tds <-  TransformDistScores(distance.scores, threshold = thresholds[1], amount = amounts[1])
    plot(distance.scores, tds, type = 'l', xlab = "distance (mins)", ylab = "weight", lty = ltys[1], lwd = lwd)
    legend.names <- paste('threshold =',thresholds[1],",  amount = ", amounts[1])
    
    for (i in 2:length(thresholds)) {
        tds <-  TransformDistScores(distance.scores, threshold = thresholds[i], amount = amounts[i])
        points(distance.scores, tds, type='l', lty = ltys[i], lwd = lwd)
        legend.names <- c(legend.names, paste('threshold =',thresholds[i],",  amount = ", amounts[i]))
        
    }

    legend("bottomright",  legend = legend.names, 
           lty = ltys[1:length(legend.names)], text.col = "black", lwd = 2)
    
}


pascalsTriangle <- function (n, last.only = FALSE) {
    
    if (n == 1) {
        return(1)
    }
    
    m <- matrix(0, nrow = n, ncol=n)
    m[,1] <- 1
    
    
    
    cur.row <- 1
    for (r in 2:n) {
        m[r,2:n] <- m[r-1, 1:(n-1)] + m[r-1, 2:n]
    }
    
    if (last.only) {
        return(m[n,])
    } else {
        return(m)
    }
    
}

# m is the minutes with > 0 species
# s is the upper limit given by greedy alg
possibleGroups2 <- function (m,s) {
    
    pt <- pascalsTriangle(m+1, TRUE)
    pt <- pt[-1]  
    pt <- pt[1:s]
    return(sum(pt))
}

# m is the minutes with > 0 species
# s is the upper limit given by greedy alg
possibleGroupGrowth <- function (n) {
    
    pt <- pascalsTriangle(n+1, FALSE)
    pt <- pt[-1,-1]
    
    row.totals <- apply(pt, 1, sum)
    
    
    return(row.totals)
}
