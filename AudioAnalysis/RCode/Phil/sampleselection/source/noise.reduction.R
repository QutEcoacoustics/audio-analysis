

TestNoiseReduction <- function (sp) {
    # tests noise reduction on a spectrogram by creating a copy of it, 
    # doing noise reduction and drawing the images
    Sp.Draw(sp)
    sp.copy <- sp
    sp.copy$vals <- DoNoiseReduction(sp$vals)
    Sp.Draw(sp.copy)

    return(sp.copy)
    
}



DoNoiseReduction <- function (m, quantile = 0.5, blur.amount = 1) {
    # if m is a matrix, do noise reduction. 
    # if m is a spectrogram, call this function with the matrix of vals and return
        
    if (class(m) == "spectrogram") {
        
        m$vals <- DoNoiseReduction(m$vals, quantile, blur.amount)
        return(m)
    }
    
    m <- Normalize(m)
    #np <- GetNoiseProfile.histdy(m)
    if (blur.amount > 0) {
        for (i in 1:blur.amount) {
            m <- Blur(m)
        }
    }
    
    m <- MedianSubtraction(m, quantile)
    return(m)
    
}

MedianSubtraction <- function (m, quantile = 0.5) {
    np <- GetNoiseProfile.quantile(m, quantile.value = quantile)
    np.matrix <- matrix(np, ncol = ncol(m), nrow = nrow(m))
    m2 <- m - np.matrix
    m2[m2 < 0] <- 0
    return(m2)
}


GetNoiseProfile.quantile <- function (m, quantile.value = 0.5) {
    np <- apply(m, 1, quantile, quantile.value)
    return(np)
}

GetNoiseProfile.histdy <- function (m) {
    # finds the level with the steepest drop on the right side of the historgram
    np <- apply(m, 1, GetHistDy)
    return(np) 
}

GetHistDy <- function (v) {
    hist.res <- hist(v, breaks = 100, plot = FALSE)
    counts <- smooth(hist.res$counts)    
    dy <- counts[1:(length(counts)-1)] - counts[2:(length(counts))] 
    res <- hist.res$breaks[which.max(dy)]  
    return(res) 
}


RemoveGrains <- function (m) {
    kernel <- matrix(1, nrow = 3, ncol = 3)
    arr <- constructArray(kernel)
}

ConstructArray <- function(m, kernel) {
    # for a given kernel
    # constructs a 3d array which is made of a number of copies of the matrix m
    # offset and multiplied by the value/position of the cells of kernel 
    
    #empty array
    arr <- array(0, c(nrow(m), ncol(m), length(as.vector(kernel))), c('r','c','o'))
    
    # add NA padding to outside of m
    h.padding <- floor(ncol(kernel)/2)
    v.padding <- floor(nrow(kernel)/2)
    
    m.padded <- PadMatrix(m, h.padding, v.padding)
    o.depth <- 0
    for (cc in 1:ncol(kernel)) {
        for (rr in 1:nrow(kernel)) {
            o.depth <- o.depth + 1
            arr[,,o.depth] <- m.padded[rr:(rr+nrow(m)-1),cc:(cc+ncol(m)-1)] * kernel[rr,cc]
      
        }
    }
    
    return(arr)
    
    
}


PadMatrix <- function (m, numr, numc, val = NA) {
    # adds rows
    if (numc > 0) {
        e.col <- matrix(val, ncol = numc, nrow = nrow(m))
        m <- cbind(e.col, m,  e.col)    
    }
    if (numr > 0) {
        e.row <- matrix(val, nrow = numr, ncol = ncol(m))
        m <- rbind(e.row, m,  e.row)    
    }
    return(m)
}


RemoveNoise <- function (spectro) {
    # aggressive noise removal
    # normalise
    # this is not very good. I don't think it was finished
    spectro <- Normalize(spectro)
    # for each row, calculate the median of it and its neighbouring rows
    med <- rep(NA, nrow(spectro))
    s1 <- rbind(spectro[1,], spectro) # pad before the first row
    s1 <- rbind(s1, s1[nrow(s1),]) # padd after the last row
    for (i in 1:(length(med))) {    
        med[i] <- median(s1[i:(i+2),], na.rm = TRUE)
    }
    med <- matrix(med, nrow = nrow(spectro), ncol = ncol(spectro))
    rem <- spectro < med
    spectro[rem] <- 0
    return(spectro)
}



