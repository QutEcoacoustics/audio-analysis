l.test <- function () {  
    duration <- 10
    spectro <- Sp.CreateTargeted("NW", "2010-10-13", 21606, 2)
    #spectro <- Sp.CreateTargeted("NW", "2010-10-13", 1600, duration)
    lines <- Lines(spectro$vals)
    #Sp.Draw(spectro)
    spectro$lines <- lines
    Sp.Draw(spectro) 
}



EmptyFeatureDataFrame <- function (num.rows) {
    return()  
}


LineFeatureExtraction <- function (target.only = FALSE) {  
    # Reads in the 'lines' binary files (lists with meta data and other stuff)
    # and collates them all to a single CSV file for the target. 
    # so that clustering etc can be done
    
    # this is done prior to starting the "SS" entry point
    
    
    if (target.only) {
        fns <- FileNamesInTarget(ext = 'lines')   
    } else {
        fns <- list.files(g.lines.dir)
    }

    line.paths <- file.path(g.lines.dir, fns)
    
    
    feature.col.names <- c('main.frequency', 'av.angle', 'length', 'straight.length')
    event.col.names <- c('total.score', 'mean.score', 'sec', 'date', 'site')    
    
    all.features <- EmptyDataFrame(feature.col.names, 0)
    all.events<- EmptyDataFrame(event.col.names, 0)
    
    
    for (file.num in 1:length(fns)) {
        
        # loads lines for this file into a variable called 'lines'
        load(line.paths[file.num])

        features <- EmptyDataFrame(feature.col.names, length(line.collection$lines))
        events <- EmptyDataFrame(event.col.names, length(line.collection$lines))
        Dot()         
        
        for (l in 1:length(line.collection$lines)) {
            lines <- line.collection$lines[[l]]            
            rows <- c(lines$branch.1$row, lines$center[1], rev(lines$branch.2$row))
            cols <- c(lines$branch.1$col, lines$center[2], rev(lines$branch.2$col))
            scores <- c(lines$branch.1$score, lines$branch.1$score[1], lines$branch.2$score)
            weights <- scores / sum(scores)
            av.row <- sum(rows * weights)         
            features$main.frequency[l] <- av.row         
            angles <- c(lines$branch.1$angle, lines$branch.2$angle)
            scores <- c(lines$branch.1$score, lines$branch.2$score)
            weights <- scores / sum(scores)         
            av.angle <- sum(angles * weights)         
            features$av.angle[l] <- av.angle        
            features$length[l] <- sum(nrow(lines$branch.1), nrow(lines$branch.2))      
            events$total.score[l] <- sum(scores)
            events$mean.score[l] <- events$total.score[l] / features$length[l]         
            features$straight.length[l] <- ((rows[1] - rows[length(rows)])^2 + (cols[1] - cols[length(cols)])^2)^0.5        
            events$date[l] <- line.collection$meta$date
            events$site[l] <- line.collection$meta$site
            events$sec[l] <-  line.collection$meta$start.sec + ColNumToTime(lines$center[2], line.collection$meta$frames.per.sec)
            
        }
        
        all.features <- rbind(all.features, features)
        all.events <- rbind(all.events, events)
        
        
    }
    
    event.id <- 1:nrow(all.features)
    
    

    
    all.features$event.id <- event.id
    all.events$event.id <- event.id
    
    
    
    WriteMasterOutput(all.features, 'line.features')
    WriteMasterOutput(all.events, 'line.events')
    
}

AddMinIdToLineEvents1 <- function (line.events = NULL, save = FALSE) {   
    if (is.null(line.events)) {      
        line.events <- ReadMasterOutput('line.events')
    }  
    all.mins <- GetMinuteList()
     min.ids <- rep(NA, nrow(line.events))    
     for (i in 1:length(min.ids)) {        
         min.ids[i] <- all.mins$min.id[all.mins$site == line.events$site[i] & all.mins$date == line.events$date[i] & all.mins$min == floor(line.events$sec[i] / 60)]
         
         print(paste(i, " / ", nrow(line.events), " " ,round(i/nrow(line.events) * 100), "% "))
         
     }       
    line.events$min.id <- min.ids  
    if (save) {
        WriteMasterOutput(line.events, 'line.events')   
    }
    return(line.events)
    
}

AddMinIdToLineEvents2 <- function (line.events = NULL, save = FALSE) {   
    if (is.null(line.events)) {      
        line.events <- ReadMasterOutput('line.events')
    }  
    all.mins <- GetMinuteList()
    event.min.of.day <- floor(line.events$sec / 60)    
    line.events$min.id <- rep(NA, nrow(line.events))  
    for (i in 1:nrow(all.mins)) {           
        event.rows <- line.events$site == all.mins$site[i] & line.events$date == all.mins$date[i] & event.min.of.day == all.mins$min[i]    
        line.events$min.id[event.rows] <- all.mins$min.id[i]    
        print(paste(i, " / ", nrow(all.mins), " " ,round(i/nrow(all.mins) * 100), "% "))   
    }       
    
    if (save) {
        WriteMasterOutput(line.events, 'line.events')   
    }
    return(line.events)
}

AddMinIdToLineEvents <- function (line.events = NULL, save = FALSE) { 
    Report(4, 'adding min id to line events')
    if (is.null(line.events)) {      
        line.events <- ReadMasterOutput('line.events')
    }  
    all.mins <- GetMinuteList()
    event.min.of.day <- floor(line.events$sec / 60)    
    
    line.events$min <- event.min.of.day
    unique.mins <- unique(line.events[ , c('site', 'date', 'min')])
    
    line.events$min.id <- rep(NA, nrow(line.events))  
    for (i in 1:nrow(unique.mins)) {               
        min.id <- all.mins$min.id[all.mins$site == unique.mins$site[i] & all.mins$date == unique.mins$date[i] & all.mins$min == unique.mins$min[i]]    
        line.events$min.id[line.events$site == unique.mins$site[i] & line.events$date == unique.mins$date[i] & line.events$min == unique.mins$min[i]] <- min.id  
        #print(paste(i, " / ", nrow(unique.mins), " " ,round(i/nrow(unique.mins) * 100), "% "))
        Dot()
    }       
    
    if (save) {
       WriteMasterOutput(line.events, 'line.events')   
    }
    return(line.events)
}

FindLines <- function (overwrite = FALSE) {
    # performs line extraction on each of the minutes in the target 
    # this is SLOW! most of the time is spent on peak detection. 
    
    
        library('tuneR')       
        cur.wav.path <- FALSE
        cur.spectro <- FALSE          
        Report(2, 'Detecting lines.')    
        num.lines.before.previous.file <- 0
        ptmt <- proc.time();
        ptm <- proc.time()


        fns <- FileNamesInTarget(ext = 'wav')
        
        
        for (f in fns) {
            Report(5, 'lines for ', f)    


      
            file.meta <- FnToMeta(f)
            
            lines.path <- file.path(g.lines.dir, MetaToFn(file.meta, ext = 'lines'))
            
            if (!file.exists(lines.path) || overwrite) {
                
                audio.path <- file.path(g.audio.dir, f)  
                spectro <- Sp.CreateFromFile(audio.path)              
                lines <- Lines(spectro$vals)             
                
                file.meta$frames.per.sec <- spectro$frames.per.sec
                file.meta$hz.per.bin <- spectro$hz.per.bin
                
                line.collection <- list(lines = lines,
                                        meta = file.meta)
                
                
                f <- save(line.collection, file = lines.path)              
                
            } else {
                
                Report(3, 'skipping file (already generated). To overwrite files, set "overwrite" to TRUE ', f)
            }
            

        }
        
        Timer(ptmt, paste('line extraction for all',length(fns),'files'), length(fns), 'files')
        
    
        
        
    }


g.centroid.spacing <- 3
g.sweep.radius <- 4
g.walkstep.dist <- 4  # probably best equal or less than g.swwp.radius


Lines <- function (m) {
    m <- Simplify(m)
    centroids <- GetPeaks.2(m, g.centroid.spacing)
    # we now have a list of 'centroids' which are the maximum values in the neighbourhood
    # might be a more efficient way to do this with masks

    # remove any centroids which are too close to the edge
    diff <- g.sweep.radius + 1
    centroids <- centroids[centroids$row > diff & 
                               centroids$col > diff & 
                               centroids$row < nrow(m) - diff &  
                               centroids$col < ncol(m) - diff, ]
    empty.line.cols <- rep(NA, nrow(centroids))
    #lines <- data.frame(angle = empty.line.cols, score = empty.line.cols, mean = empty.line.cols, sd = empty.line.cols)
    lines <- list()
    print(nrow(centroids))
    for (cc in (1:nrow(centroids))) {
        # sub matrix centered on the centroid
        
        if (cc %in% c(18,24,25)) {
            #inspect = TRUE
        } else {
            inspect = FALSE     
        }
        
        line <- LineWalk(m, as.numeric(centroids[cc,]), g.sweep.radius, inspect)
        if (class(line) == 'list') {
            lines[[length(lines) + 1]] <- line
            Dot()

            if (length(lines) %in% c()) {
                print(paste('inspect', cc))
            }
        }
    }
    
    # remove null values where the line wasn't over the threshold
    # https://www.inkling.com/read/r-cookbook-paul-teetor-1st/chapter-5/recipe-5-12
    lines[sapply(lines, is.null)] <- NULL
    
    # join lines
    # todo
    
    return(lines)    
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
    # This is SLOW!  the greater r, the slower it is (becasue the more pixels each on needs to be compared with)
    
    # first, simplify by taking every second row and column
    
    reduce.by <- 2
    
    if (reduce.by > 1) {
        # reduce dimensions by half to reduce computation
        rows <- 1:(floor(nrow(m)/reduce.by)) * reduce.by
        cols <- 1:(floor(ncol(m)/reduce.by)) * reduce.by
        m <- m[rows,cols]      
    }

    
    
    line.offsets <- -r:r   
    x.offsets <- rep(line.offsets, times = length(line.offsets), each = 1)
    y.offsets <- rep(line.offsets, each = length(line.offsets), times = 1)  
    in.circle <- (x.offsets^2 + y.offsets^2) <= r^2
    offsets <- data.frame(x = x.offsets, y = y.offsets)
    offsets <- offsets[in.circle,]
    block <- array(NA, dim = c(nrow(m), ncol(m), nrow(offsets)))
    
    # create a 3d array: 3rd dimension is the passed matrix offset by some value
    for (i in 1:nrow(offsets)) {
        
        shifted <- ShiftMatrix(m, offsets$y[i], offsets$x[i]) 
        block[,,i] <- shifted
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
    
    if (reduce.by > 1) {
        peaks <- peaks * reduce.by 
    }
    
    
    return(peaks)
    
}

Simplify <- function (m) {
    

    
    #m <- Blur(m)
    m <- BlurForLines(m)
    

    #m <- BlurForLines(m)
    # reduce dimensions by half to reduce computation
    #r <- 1:(floor(nrow(m)/2)) * 2
    #c <- 1:(floor(ncol(m)/2)) * 2
    #m <- m[r,c]   
    # background removal (all vals < 1 standard deviation above the mean per freq band)
    # row.means <- apply(m, 1, mean)
    row.av  <- apply(m, 1, median)
    row.sds <- apply(m, 1, sd)  
    # blur the row thresholds a bit
    row.av <- MovingAverage(row.av, 2, TRUE)
    row.sds <- MovingAverage(row.sds, 2, TRUE)

    
    # remove values below threshold
    rm.matrix <- matrix(row.av, nrow = nrow(m), ncol = ncol(m))
    rsd.matrix <- matrix(row.sds, nrow = nrow(m), ncol = ncol(m))
    adjust <- TRUE
    if (adjust) {
        # adjust intensities of each frequency band to keep them similar
        m <- m - rm.matrix
        rt.matrix <- rsd.matrix * .01
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

LineWalk <- function (m, center, r, inspect = FALSE) {
    # given a center point on matrix m,
    # creates a "line", which consists of 2 branches. 
    # branch 1 is created by finding the angle from 'center' with the highest score
    # then moving "r" pixels in that direction and finding the angle from 
    # there with the highest score and so on. the range of angle to include for each step is 
    # limited to 90 degrees (configurable) from the previous step, so that it keeps
    # the line moving in the same direction. 
    # branch 2 is created in the same way, but by starting the walk 180 degrees from the angle
    # of the first node of branch 1. 
    
    if (inspect) {
        
        print('inspect')
        
    }
    
    sub <- m[(center[1]-r):(center[1]+r), (center[2]-r):(center[2]+r)] 
    best.angle <- FindBestLine(sub, recurse.depth = 2, momentum.bias = 0, inspect = inspect)
    
    branch.1 <- LineWalkBranch(m, center, r, best.angle$angle, range = pi/2, resolution = 3, refinements = 2, inspect = inspect)
    branch.2 <- LineWalkBranch(m, center, r, OppositeAngle(best.angle$angle), range = pi/4, resolution = 3, refinements = 2, inspect = inspect)
    
    #branch.1$branch <- rep(1, nrow(branch.1))
    #branch.1$joint <- 1:nrow(branch.1)
    #branch.2$branch <- rep(1, nrow(branch.2))
    #branch.2$joint <- 1:nrow(branch.2)
    
    if (nrow(branch.1) == 0) {
        return(FALSE)
    } else {
        return(list(
            branch.1 = branch.1,
            branch.2 = branch.2,
            center = center      
        ))
    }
    

    
    
}

OppositeAngle <- function (angle) {
    # maybe there is a way to do this in 1 line with modulus. I couldn't find it
    return((angle + pi) %% (2*pi))
}

LineWalkBranch <- function (m, start.center, r,  angle, range, resolution, refinements = 2, inspect = FALSE) {
    
    score.threshold <- 60
    max.per.branch <- 10
    
    cur.center <- start.center
    empty.line.cols <- rep(NA, max.per.branch)
    branch <- data.frame(row = empty.line.cols, col = empty.line.cols, angle = empty.line.cols,  length = empty.line.cols, score = empty.line.cols, mean = empty.line.cols, sd = empty.line.cols)
    
    for(line.num in 1:max.per.branch) {
        
        sub <- m[(cur.center[1]-r):(cur.center[1]+r), (cur.center[2]-r):(cur.center[2]+r)] 
        line <- FindBestLine(sub, angle, range, recurse.depth = refinements, inspect = inspect)
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
        
        if (cur.center[1] > nrow(m) - r || cur.center[1] <= r || cur.center[2] > ncol(m) - r || cur.center[2] <= r ) {
            # the line has walked to the edge of the matrix
            break()
        }
        
        angle <- line$angle
        
    }
    
    branch <- branch[complete.cases(branch), ]
    
    return(branch)
    
    
    
}



FindBestLine <- function (m = NA, angle = 0, range = pi, resolution = 4, recurse.depth = 2, momentum.bias = 0.7, inspect = FALSE) {
    # finds angle of the line starting at the center of the matrix m
    # within the range eiher side of angle
    # resolution is the number of divisions of the range to compare (either side of angle)
    #
    # Args:
    #   m: the matrix to use. The sweep is made around the center of the matrix
    #   angle: the angle in radians to sweep either side of (zero is horizontal facing right)
    #   range: how far to sweep either side of angle
    #   resolution: how many angles to check when doing the sweep on either side
    #   recurse depth: how many times to refine to get the exact angle
    #   momentum bias: float [0,1];  how much to favour angle. This helps to keep going in a straight line
    #                                the score will be multiplied by. zero is no bias, 1 is maximum bias
  
    if (inspect) {
        print('inspect')
    }
    
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
    
    # angle diff is 1 at the opposite angle and zero at the angle
    angle.diff <- abs(angles - angle) / pi
    angle.bias <- 1 - (momentum.bias * angle.diff)
    scores <- scores * angle.bias
    
    
    best <- which.max(scores)
    best.angle <- angles[best]
    mean <- mean(scores)
    sd <- sd(scores)
    score <- max(scores)
    
    # TODO: variable length
    length <- g.walkstep.dist
    
    if (recurse.depth > 1) {
        line <- FindBestLine(m = m, 
                             angle = best.angle, 
                             range = range/(resolution*2), 
                             resolution = resolution, 
                             recurse.depth = recurse.depth-1, 
                             momentum.bias = 0,  # possibly better to pass the angle as a separate 'momentum angle' arg
                             inspect = inspect)
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




GetLinesForclustering <- function (reextract = TRUE) {
    SetOutputPath(level = 1) # for reading  
    
    
    if (reextract || !OutputExists('line.events', level = 1) || !OutputExists('line.features', level = 1)) {
        
        all.events <- ReadMasterOutput('line.events')
        all.feature.rows <- ReadOutputCsv(MasterOutputPath('line.features'))
        
        target.min.ids <- ReadOutput('target.min.ids', level = 0)
        events <- all.events[all.events$min.id %in% target.min.ids$min.id, ]
        event.features <- all.feature.rows[all.feature.rows$event.id %in% events$event.id, ]
        
        #ensure that both are sorted by event id 
        events <- events[with(events, order(event.id)) ,]
        event.features <- event.features[with(event.features, order(event.id)) ,]
        
        score <- events$mean.score * event.features$length^0.5
        limit <- ReadInt('limit the number of events (lines)') 
        score.threshold <- ReadInt(paste0('set the score threshold (mean = ', mean(score), ')'))
        
        event.features <- event.features[score >= score.threshold, ]
        events  <- events[score >= score.threshold, ]
        
        # limit the number
        if (limit < nrow(event.features)) {
            Report(4, 'Number of target events (', nrow(event.features), ") is greater than limit (", limit ,"). Not all the events will be included. ")           
            include <- GetIncluded(nrow(event.features), limit)
            event.features <- event.features[include, ]
            events <- events[include, ]
        }    
        
        WriteOutput(events, 'line.events', level = 1)
        WriteOutput(event.features, 'line.features', level = 1)
        
    } else {
        Report(4, 'Retrieving target line.events and line.features')
        events <- ReadOutput('line.events', level = 1)
        event.features <- ReadOutput('line.features', level = 1)
        
    }
    
    # remove event.id.column from features table
    drop.cols <- names(event.features) %in% c('event.id')
    event.features <- event.features[!drop.cols]
    return (list(events = events, event.features = event.features))
    
}






