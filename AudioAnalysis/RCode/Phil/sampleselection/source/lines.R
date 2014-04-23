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

    
    # we now have a list of 'centroids' which are the maximum values in the neighbourhood
    # might be a more efficient way to do this with masks
    diff <- 5
    
    # remove any centroids which are too close to the edge
    centroids <- centroids[centroids$row > diff & 
                               centroids$col > diff & 
                               centroids$row < nrow(m) - diff &  
                               centroids$col < ncol(m) - diff, ]
    empty.line.cols <- rep(NA, nrow(centroids))
    #lines <- data.frame(angle = empty.line.cols, score = empty.line.cols, mean = empty.line.cols, sd = empty.line.cols)
    
    lines <- list()
    
    for (cc in (1:nrow(centroids))) {
        # sub matrix centered on the centroid
        
        if (cc %in% c(199, 200)) {
            inspect = TRUE
        } else {
            inspect = FALSE     
        }
        
        line <- LineWalk(m, as.numeric(centroids[cc,]), 4)
        
        lines[[cc]] <- line
        
        #lines[cc, ] <- unlist(line.angle)
    }
    
    # join lines
    # todo

    return(lines)
    
}

LineWalk <- function (m, center, r) {
    # given a center point on matrix m,
    # creates a "line", which consists of 2 branches. 
    # branch 1 is created by finding the angle from 'center' with the highest score
    # then moving "r" pixels in that direction and finding the angle from 
    # there with the highest score and so on. the range of angle to include for each step is 
    # limited to 90 degrees (configurable) from the previous step, so that it keeps
    # the line moving in the same direction. 
    # branch 2 is created in the same way, but by starting the walk 180 degrees from the angle
    # of the first node of branch 1. 
    
    
    
    
    sub <- m[(center[1]-r):(center[1]+r), (center[2]-r):(center[2]+r)] 
    best.angle <- FindBestLine(sub, recurse.depth = 1)
    
    branch.1 <- LineWalkBranch(m, center, r, best.angle$angle, range = pi/4, resolution = 3, refinements = 2)
    branch.2 <- LineWalkBranch(m, center, r, OppositeAngle(best.angle$angle), range = pi/4, resolution = 3, refinements = 2)
    
    #branch.1$branch <- rep(1, nrow(branch.1))
    #branch.1$joint <- 1:nrow(branch.1)
    #branch.2$branch <- rep(1, nrow(branch.2))
    #branch.2$joint <- 1:nrow(branch.2)
    
    return(list(
        branch.1 = branch.1,
        branch.2 = branch.2,
        center = center      
    ))
    
    
}

OppositeAngle <- function (angle) {
    # maybe there is a way to do this in 1 line with modulus. I couldn't find it
    return((angle + pi) %% (2*pi))
}

LineWalkBranch <- function (m, start.center, r,  angle, range, resolution, refinements = 2) {
    
    score.threshold <- 50
    max.per.branch <- 10
    
    cur.center <- start.center
    empty.line.cols <- rep(NA, max.per.branch)
    branch <- data.frame(row = empty.line.cols, col = empty.line.cols, angle = empty.line.cols,  length = empty.line.cols, score = empty.line.cols, mean = empty.line.cols, sd = empty.line.cols)
    
    for(line.num in 1:max.per.branch) {
        
        sub <- m[(cur.center[1]-r):(cur.center[1]+r), (cur.center[2]-r):(cur.center[2]+r)] 
        line <- FindBestLine(sub, angle, range, recurse.depth = refinements)
        
        if (line$score < score.threshold) {
            break()
        }
        

        # shift the current center to the coordinates given by line length and angle
        
        # negative angle means move down, which means positive change to row
        # therefore reverse the sign on row change
        cur.center <- cur.center + round(c( (0-line$length) * sin(line$angle), line$length * cos(line$angle)))
        
        # the "row" and "col" of the line, are the end point of the line. 
        # the start point is the end point of the previous line, or the start of the branch if it's the first line
        # this is why 'cur.center' is calculated before recording this line
        branch$row[line.num] <- cur.center[1]
        branch$col[line.num] <- cur.center[2]
        branch$angle[line.num] <- line$angle
        branch$length[line.num] <- line$length
        branch$score[line.num] <- line$score
        branch$mean[line.num] <- line$mean
        branch$sd[line.num] <- line$sd
        
        angle <- line$angle
        
    }
    
    branch <- branch[complete.cases(branch), ]
    
    
    return(branch)
    
    
    
}



FindBestLine <- function (m = NA, angle = 0, range = pi, resolution = 4, recurse.depth = 2) {
    # finds angle of the line starting at the center of the matrix m
    # within the range eiher side of angle
    # resolution is the number of divisions of the range to compare (either side of angle)
  
    # radius is 1 less than half the width of the square matrix
    r <- (ncol(m) - 1) / 2  

    angles <- angle + range*(-resolution:resolution)/resolution
    
    # if range is pi or more, this will remove duplicate angles 
    # (eg 0 and 2pi if range is pi)
    angles <- unique(angles %% (2*pi))
    scores <- rep(NA, length(angles)) 
    
    
    for (theta.i in 1:length(angles)) {      
        
        distances <- DistanceToLine(angles[theta.i], r, TRUE)
        weights <- GaussianFunction(a = 1, x = distances, b = 0, c = 1) 
        scores[theta.i] <- sum(m * weights, na.rm = TRUE) 
    } 
    # return the angle, what its score was
    # what the mean score was and what the sd of scores was
    # so that we know how 'good' the best was. 
    # if there was no clear best, then this point might not lie on a line
    best <- which.max(scores)
    best.angle <- angles[best]
    mean <- mean(scores)
    sd <- sd(scores)
    score <- max(scores)
    
    # TODO: variable length
    length <- r
    
    if (recurse.depth > 1) {
        line <- FindBestLine(m = m, angle = best.angle, range = range/(resolution*2), resolution = resolution, recurse.depth = recurse.depth-1)
    } else {
        line <- list(
            angle = best.angle,
            length = length,
            score = score,
            mean = mean,
            sd = sd
        )
    }
    
    return(line)
    
    
}



DistanceToLine <- function (theta, r, one.way = FALSE) {
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
    
    if (theta > 2*pi || theta < 0) {
        stop('theta must be between 0 and 2 pi')
    }
    
    width <- 2*r + 1    
    mx <- matrix(-r:r, nrow = width, ncol = width, byrow = TRUE)
    my <- matrix(r:-r, nrow = width, ncol = width)
    tan.theta <- tan(theta)
    md <- abs(tan.theta*mx - my) / sqrt(tan.theta^2 + 1)
    
    outside.circle <- (mx^2 + my^2) > (r+0.5)^2
    md[outside.circle] <- NA
    
    if (one.way) { 
        tan.theta2 <- tan(theta + (pi/2))        
        
        if (theta > 0 & theta <= pi) {
            # round to some significant figure so that very small numbers end up equal to zero
            remove <- my < round(mx * tan.theta2, 6)  
        } else {
            remove <- my > round(mx * tan.theta2, 6)
        }
        md[remove] <- NA      
    }
    
    return(md)
    
}


FindBestLine.twoDirections <- function (m = NA, r = 4, inspect = FALSE) {
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