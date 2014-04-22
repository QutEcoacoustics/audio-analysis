library('adimpro')

source('ss.R')

f <- "/Users/n8933464/Documents/SERF/NW/test/test.wav"
i <- "/Users/n8933464/Documents/SERF/NW/test/test.wav.png"

spectro <- Sp.Create(f)

Sp.Draw(spectro, img.path = i)

ad <- make.image(spectro$vals)

edg <- edges(ad, type = "Laplacian", ltype=2, abs=FALSE)

edg.img <- extract.image(edg)

image(edg.img)



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
