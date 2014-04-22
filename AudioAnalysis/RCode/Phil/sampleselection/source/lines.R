l.test <- function () {
    
    spectro <- Sp.CreateTargeted("NW", "2010-10-13", 21600, 5)
    
    lines <- Lines(spectro$vals)
    
    spectro$lines <- lines
    
    Sp.Draw(spectro)
    
    
}


GetPeaks.1 <- function (m, cell.w = 10, cell.h = 10, overlap = 2) {
    # get peaks using grid method
    # divides the given matrix into a grid, and returns the coordinates of the maximum
    # value from each of the grid cells

    # list of x and y offets
    # staring from half the width from the top and left
    # and ending half the width (plus any leftover which doesn't fit exactly)
    # from the right and bottom 
    x.offsets <- round(seq((cell.w/2), ncol(m) - (cell.w * 1.5), cell.w/overlap))
    y.offsets <- round(seq((cell.h/2), nrow(m) - (cell.h * 1.5), cell.h/overlap))
    
    empty.line.cols <- rep(NA, length(x.offsets) * length(y.offsets))
    #     lines <- data.frame(start.x = line.cols, 
    #                         start.y = line.cols, 
    #                         end.x = line.cols, 
    #                         end.y = line.cols, 
    #                         slope = line.cols, 
    #                         error = line.cols)
    
    blank <- rep(NA, length(x.offsets) * length(y.offsets))
    peaks <- data.frame(row = empty.line.cols, col = empty.line.cols)
    cnum <- 1
    for (x.offset in x.offsets) {
        for (y.offset in y.offsets) {
            sub <- m[y.offset:(y.offset + cell.h - 1), x.offset:(x.offset + cell.w - 1)]
            if (length(sub[sub > 0]) > 10) {
                peaks[cnum, ] <- GetMaxCell(sub) + c(y.offset - 1, x.offset - 1)
                cnum <- cnum + 1
            }
        }
    }
    
    peaks <- peaks[!is.na(peaks[,1]),]
    
    return(peaks)
    
    
}


GetPeaks.2 <- function (m, r = 4) {
    # returns a list of cells (row/column) which are the
    # peak values within a radius of r  
    
    line.offsets <- -r:r   
    x.offsets <- rep(line.offsets, times = length(line.offsets), each = 1)
    y.offsets <- rep(line.offsets, each = length(line.offsets), times = 1)  
    in.circle <- (x.offsets^2 + x.offsets^2) <= r^2
    offsets <- data.frame(x = x.offsets, y = y.offsets)
    offsets <- offsets[in.circle,]
    block <- array(NA, dim = c(nrow(m), ncol(m), nrow(offsets)))
    
    # create a 3d array: 3rd dimension is the passed matrix offset by some value
    for (i in 1:nrow(offsets)) {     
        block[,,i] <- ShiftMatrix(m, offsets$y[i], offsets$x[i]) 
    }
    
    #find the maximum offset, and the value of the maximum offset
    # max.offset <- apply(block, c(1,2), which.max)
    max.offset.val <-  apply(block, c(1,2), max)
    
    # the original offest (no offset) 
    # zero.offset.index <- ceiling(nrow(offsets)/2)  # or which(offsets$x == 0 & offsets$y == 0)
    
    # zero.offset.is.max <- max.offset == zero.offset.index & max.offset.val > 0
    # convert logical to 1s and 0s
    # peaks <- zero.offset.is.max * 1
    peaks.matrix <- m
    
    peaks.matrix[m != max.offset.val ] <- 0
    
    
    w <- which(peaks.matrix > 0)
    
    row <- w %% nrow(m)
    row[row == 0] <-  nrow(m)
    col <- ceiling(w / nrow(m))
    peaks <- data.frame(row = row, col = col)
    return(peaks)
    
}


Simplify <- function (m) {
    
    m <- Blur(m)
    m <- BlurForLines(m)
    

    #m <- BlurForLines(m)
    # reduce dimensions by half to reduce computation
    #r <- 1:(floor(nrow(m)/2)) * 2
    #c <- 1:(floor(ncol(m)/2)) * 2
    #m <- m[r,c]   
    # background removal (all vals < 1 standard deviation above the mean per freq band)
    row.means <- apply(m, 1, mean)
    row.sds <- apply(m, 1, sd)  
    # blur the row thresholds a bit
    row.means <- MovingAverage(row.means, 2, TRUE)
    row.sds <- MovingAverage(row.sds, 2, TRUE)
    row.thresholds <- row.means + row.sds
    # remove values below threshold
    rm.matrix <- matrix(row.means, nrow = nrow(m), ncol = ncol(m))
    rsd.matrix <- matrix(row.sds, nrow = nrow(m), ncol = ncol(m))
    adjust <- TRUE
    if (adjust) {
        # adjust intensities of each frequency band to keep them similar
        m <- m - rm.matrix
        rt.matrix <- rsd.matrix 
    } else {
        rt.matrix <- rm.matrix + rsd.matrix
    }
    below.threshold <- m < rt.matrix
    m[below.threshold] <- NA  
    # reset quantization to integers between 0 and 10
    # m <- round(Normalize(m) * 20)
    m[is.na(m)] <- 0
    
    return(m)
    
}

Lines <- function (m) {
    

    m <- Simplify(m)

    #centroids <- GetPeaks.1(m, 20, 20)
    
    centroids <- GetPeaks.2(m, 2)
    
    

    
    #stop()
    
    # we now have a list of 'centroids' which are the maximum values in the neighbourhood
    # might be a more efficient way to do this with masks
    diff <- 5
    
    # remove any centroids which are too close to the edge
    centroids <- centroids[centroids$row > diff & 
                               centroids$col > diff & 
                               centroids$row < nrow(m) - diff &  
                               centroids$col < ncol(m) - diff, ]
    empty.line.cols <- rep(NA, nrow(centroids))
    lines <- data.frame(angle = empty.line.cols, score = empty.line.cols, mean = empty.line.cols, sd = empty.line.cols)
    for (cc in (1:nrow(centroids))) {
        # sub matrix centered on the centroid
        sub <- m[(centroids[cc,1]-diff):(centroids[cc,1]+diff), (centroids[cc,2]-diff):(centroids[cc,2]+diff)]
        
        if (cc %in% c(199, 200)) {
            inspect = TRUE
        } else {
            inspect = FALSE     
        }
        
        line.angle <- FindBestLine(sub, inspect = inspect)
        lines[cc, ] <- unlist(line.angle)
    }
    
    # join lines
    # todo

    return(cbind(centroids, lines))
    
}

FindBestLine <- function (m = NA, r = 4, inspect = FALSE) {
    # finds angle of the line passing through the centre of m
    # which has the least error
    
    
    if (inspect) {
        inspect = TRUE
    }
    
    # how many angles to check between zero and 180 deg (pi)
    angle.resolution <- 8  # best to use be power of 2 ? not sure
    
    
    r <- (ncol(m) - 1) / 2
    
    scores <- rep(NA, angle.resolution)

    
    for (theta.i in 1:angle.resolution) {
        
        distances <- DistanceToLine(pi*((theta.i-1)/angle.resolution), r)
        weights <- GaussianFunction(a = 1, x = distances, b = 0, c = 1) 
        scores[theta.i] <- sum(m * weights, na.rm = TRUE)
        
    }
    
    
    # return the angle, what its score was
    # what the mean score was and what the sd of scores was
    # so that we know how 'good' the best was. 
    # if there was no clear best, then this point might not lie on a line
    best <- which.max(scores)
    angle <- pi*((best-1)/angle.resolution)
    mean <- mean(scores)
    sd <- sd(scores)
    score <- max(scores)
    
    return(list(
        angle = angle,
        score = score,
        mean = mean,
        sd = sd   
        ))
    
    
}


DistanceToLine <- function (theta, r) {
    # returns a width*width matrix. Each cell holds the distance of
    # that cell from  a line which passes through the center 
    # of the matrix at an angle theta. 
    #
    # Args:
    #   width: integer
    #   theta: number [0,180] angle from horizontal in radians
    # 
    # Value: 
    #   m: matrix
    #
    # Details:
    #   slightly inaccurate (I think because of accuracy that pi is stored)
    #   so zeros will not be exactly zero. 
    
    width <- 2*r + 1    
    mx <- matrix(-r:r, nrow = width, ncol = width, byrow = TRUE)
    my <- matrix(r:-r, nrow = width, ncol = width)
    tan.theta <- tan(theta)
    md <- abs(tan.theta*mx - my) / sqrt(tan.theta^2 + 1)
    
    outside.circle <- (mx^2 + my^2) > (r+0.5)^2
    md[outside.circle] <- NA
    
    return(md)
    
}


Lines1 <- function (spectro) { 

    m <- spectro$vals

    m <- Simplify(m)
    
    #spectro$vals <- m
    #Sp.Draw(spectro)  
    
    cell.w <- 10
    cell.h <- 10
    
    x.offsets <- seq(1, ncol(m) - ncol(m) %% cell.w, cell.w)
    y.offsets <- seq(1, nrow(m) - nrow(m) %% cell.w, cell.w)
    
    line.cols <- rep(NA, length(x.offsets) * length(y.offsets))
    lines <- data.frame(start.x = line.cols, 
                        start.y = line.cols, 
                        end.x = line.cols, 
                        end.y = line.cols, 
                        slope = line.cols, 
                        error = line.cols)
    
    lnum = 1
    
    for (x in x.offsets) {
        for (y in y.offsets) {
            y.offset <- y
            x.offset <- x
            sub <- m[y.offset:(y.offset+cell.h), x.offset:(x.offset+cell.w)]

            if (length(sub[sub > 0]) > 3) {

                image(t(sub))
                
 
                
                lobf <- LineOfBestFit2(sub)       
                
                if(lobf$y.intercept < 0) {
                    x.bottom.intercept <- (0 - lobf$y.intercept) / lobf$slope                   
                    start <- c(x.bottom.intercept, 0)
                } else {
                    start <- c(0, lobf$y.intercept)   
                }
                
                y.right.intercept <- lobf$y.intercept + lobf$slope * cell.w
                
                if (y.right.intercept > cell.h) {
                    x.top.intercept <- (cell.h - lobf$y.intercept) / lobf$slope 
                    end <- c(x.top.intercept, cell.h)
                } else {
                    end <- c(cell.w, y.right.intercept) 
                }
                
                
                
                lines$start.x[lnum] <- start[1] + x.offset 
                lines$end.x[lnum] <- end[1] + x.offset
                
            
                
                # y is a bit hard because we are starting from the top
                lines$start.y[lnum] <-  y.offset + cell.h - start[2]
                lines$end.y[lnum] <-  y.offset + cell.h - end[2]
                
                lines$slope[lnum] <- lobf$slope
                lines$error <- lobf$error
                
    
                

                
 
                
                lnum <- lnum + 1
             
            }
            

        }
    }
    
    spectro$lines <- lines
    
    return(spectro)
    
    
    
}

LineOfBestFit <- function(m) {
    
    data <- MatrixToExamples(m);
    model1 <- lm(data[ ,2]~data[ ,1])
    lm.coefficients <- coefficients(model1)
    mean.squared.error <- mean(residuals(model1)^2)
    return(list(y.intercept = lm.coefficients[1], 
                    slope = lm.coefficients[2],
                    error = mean.squared.error))
    
}

LineOfBestFit2 <- function (m) {
    

    
    angle.res <- 10  # sweep in increments of 10 degrees
    
    
    
    
}

GetMaxCell <- function (m) {
    max.cell <- which(m == max(m), arr.ind = TRUE)
    max.cell <- max.cell[ceiling(nrow(max.cell)/2) ,]  # if there are more than one, use the middle value
    return(max.cell)
}


BlurForLines <- function (m) {  
    
    # mask obtained by doing gaussian blur on 1 pixel in 
    # photoshop and copying resulting pixel values
    # I suppose you could get the same thing somehow with 
    # a gaussian function
    mask <- matrix(c(.01,.02,.03,.02,.01,
                     .02,.05,.09,.05,.02,
                     .03,.09,.12,.09,.03,
                     .02,.05,.09,.05,.02,
                     .01,.02,.03,.02,.01), 5, 5, byrow = TRUE)
    m2 <- Convolve(m, mask)
    return(m2)  
}

# moving average #warning, end values are NA,
# use custom MovingAverage() function to avoid this
ma <- function(x,n=5){filter(x,rep(1/n,n), sides=2)}