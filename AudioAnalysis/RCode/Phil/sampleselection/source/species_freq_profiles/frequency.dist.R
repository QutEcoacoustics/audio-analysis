
GetFrequencyDistributions <- function (sp = NULL, bins = 256) {
    # returns a 1-d heatmap of the relative intensities at each frequency for a particular species
    # as each column of a matrix for each species specified
    #
    # Args: 
    #   sp: data.frame; the species to use. If null, all species
    #   bins: int; number of frequency bins
    
    if (is.null(sp)) {
        species.list <- GetSpeciesList()
    } else {
        species.list <- sp
    }
    

    # TODO: remove species not specified
    dists <- bfs <- tfs <- as.data.frame(matrix(NA, nrow = bins, ncol = nrow(species.list)))
    colnames(dists) <- colnames(bfs) <- colnames(tfs) <- species.list$id
    
    save.path <- "~/Documents/papers/species_fdists"
    for (i in 1:nrow(species.list)) { 
        res <- GetFreqDist(sp = species.list$id[i], save.path = save.path, bins = bins)
        dists[, i] <- res$f.dist
        bfs[,i] <- res$bf.totals
        tfs[,i] <- res$tf.totals
        
    }
    
    write.csv(dists, file = "dists.csv", row.names = FALSE)
    write.csv(bfs, file = "bfs.csv", row.names = FALSE)
    write.csv(tfs, file = "tfs.csv", row.names = FALSE)
    
    #return(dists)
    
}






GetFreqDist <- function (sp, save.path, bins = 256, save.every = 200, range = 11025) {
    # returns a 1-d heatmap of the relative intensities at each frequency for a particular species
    #
    # Args: 
    #   sp: int; the species to use
    #   bins: int; number of frequency bins
    #   save.every: int; Save to file after this many annotations. Allows resumption of processing after quitting
     
    start.date.time <- '2010-10-13 00:00:00'
    end.date.time <- '2010-10-18 00:00:00'
    sites <- c('NE', 'NW', 'SE', 'SW')
    fields <- c('site', 'start_date', 'start_time', 'duration', 'start_frequency', 'end_frequency', 'species_id', 'id')
        
    annotations <- ReadTagsFromDb(fields = fields, 
                                  sites = sites, 
                                  species.id = sp, 
                                  start.date.time = start.date.time, 
                                  end.date.time = end.date.time,
                                  no.duplicates = FALSE)
    
    # convert from milliseconds to seconds
    annotations$duration <- annotations$duration / 1000
    
    # convert top and bottom frequency to numeric
    
    annotations$start_frequency <- as.numeric(annotations$start_frequency)
    annotations$end_frequency <- as.numeric(annotations$end_frequency)
    
    
    max.duration <- 20
    annotations <- annotations[annotations$duration <= max.duration,]
    
    annotations$start_sec <- sapply(annotations$start_time, TimeToSec)
    
    species.save.path <- file.path(save.path, paste0(sp,'.sfdobj'))
    
    # either read it from disk, or if it's not been saved, 
    # gets a new one
    sp.res <- ReadSFD(species.save.path, num.bins = bins, num.annotations = nrow(annotations))
    
    total.num.annotations <- nrow(annotations)
    
    
    # determine bin counts for top and bottom frequency
    sp.res$bf.totals <- GetBinCount(annotations$start_frequency / (range / bins))
    sp.res$tf.totals <- GetBinCount(annotations$end_frequency / (range / bins))
    
    
    # remove all annotations from list which already have results
    annotations <- annotations[! annotations$id %in% sp.res$meta$tag.id,]
  
    num.to.do <- nrow(annotations)
    already.completed <- total.num.annotations - num.to.do
    
    Report(3, 'processing species', sp)
    Report(3, already.completed, 'out of', total.num.annotations, 'already completed (', round(100 * already.completed/total.num.annotations), '%)')
    
    
    
    if (num.to.do > 0) {
        for (a in 1:nrow(annotations)) {
            
            res <- GetAnnotationDist(annotations[a,], bins)
            sp.res$row.totals[,a] <- res$row.totals
            sp.res$meta$num.cols[a] <- res$num.cols
            sp.res$meta$tag.id[a] <- annotations$id[a]
            Dot(3)
            if (a %% save.every == 0) {
                SaveSFD(sp.res, path = species.save.path)
                complete <- round(a / nrow(annotations) * 100, 1)
                Report(3, 'species', sp, complete, '% complete')
            }
        }
    }
    

    
    
    # get means for rows
    # add all the row subtotals (the row totals for each annotation)
    # divide by the grand total number of coluns
    
    row.grand.totals <- apply(sp.res$row.totals, 1, sum)
    row.means <- row.grand.totals / sum(sp.res$meta$num.cols)
    
    sp.res$f.dist <- row.means
    
    SaveSFD(sp.res, path = species.save.path)
    
    return(sp.res)
    
    
    
}

SaveSFD <- function (sfd, path) {
    # saves the species frequency distribution object, 
    # which consists of a list that has a matrix of values and a list that consists of
    # a vector of the number of columns for each annotation and the tag.id for each annotation
    #
    # Args:
    #   sfd: the object list
    #   path: where to save it
    save(sfd, file = path)
}

ReadSFD <- function (path, num.annotations, num.bins) {
    # reads the species frequency distribution from a given path
    # if doesn't exist, will create and return an empty one
    # 
    # Args: 
    #   path: string: the location of the file to read
    #   num.annotations: int; if there is no file, this is used for the creation of the empty object
    #   num.bins: int; same
    #
    if (file.exists(path)) {  
        load(path)
        return(sfd) # this is the name of the variable used when saving
    } else {
        empty.sfd <- list(
            # will hold the totals. Sum this at the end to get the grand totals
            row.totals = matrix(0, nrow = num.bins, ncol = num.annotations),
            # will hold the totals for bottom and top frequencies
            bf.totals = rep(0, num.bins),
            tf.totals = rep(0, num.bins),
            # will hold the number of columns for each annotation, sum to get what to divide by when calculating means
            meta = data.frame(num.cols = rep(0, num.annotations), tag.id = rep(0, num.annotations))
        )
        return(empty.sfd)
    }
}

GetBinCount <- function (bin,  max = 256) {
    # uses table function to get the counts,
    # then sets missing values to zero
    #
    # Args:
    #   bin: vector of ints;
    
    bin.count.all <- data.frame(bin = 1:max, count = rep(0, max))
    bin.count <- as.data.frame(table(bin), stringsAsFactors = FALSE)
    bin.count$bin <- as.numeric(bin.count$bin)
    bin.count.all$count[bin.count$bin] <- bin.count$Freq
    return(bin.count.all$count)
    
}



GetAnnotationDist <- function (a, bins) {
    # for a given annotation, gets the spectrogram, does noise removal and sums the values
    # of each row across the columns
    # 
    # Args: 
    #   a: string; the annotation (a row from the database)
    #   bins: int; the number of frequency bins to use (determines the window of the spectrogram)
    
    spec <- Sp.CreateTargeted(site = a$site, 
                              start.date = a$start_date, 
                              start.sec = a$start_sec, 
                              duration = a$duration,
                              frame.width = bins*2)
    
    # remove frequencies outside annotation
    top.bin <- round(a$end_frequency / spec$hz.per.bin)
    bottom.bin <- round(a$start_frequency / spec$hz.per.bin)
    spec$vals[-(bottom.bin:top.bin),] <- 0
    
    # remove bg noise and normalize within annotation
    spec$vals[bottom.bin:top.bin,] <- NoiseRemoval.MS(spec$vals[bottom.bin:top.bin,])
    
    #print(a)
    #image(t(spec$vals))
    
    # sum each row
    row.totals <- apply(spec$vals, 1, sum)
    
    return(list(row.totals = row.totals, num.cols = ncol(spec$vals)))
    
}


NoiseRemoval.MS <- function (m) {
    # median subtraction noise removal
    # subtracts the value of the median + the standard deviation of minimums of rows or columns (whichever is less)
    # sets negative values to zero
    # normalizes to [0,1]
    #
    # Args:
    #   m: matrix of values
    
    m <- Normalize(m)
    
    # threshold is the median value (assumed to be background noise) + one standard deviation in background noise
    # which is estimated as the standard deviation of the minimums of rows or columns (whichever is less)
    threshold <- median(m) + min(sd(apply(m, 1, min)),sd(apply(m, 2, min)))
    m <- m - threshold
    m[m < 0] <- 0
    m <- Normalize(m)
    return(m)  
}





