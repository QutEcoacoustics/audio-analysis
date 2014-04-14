ReadIndices <- function (cols.to.use) {
    
    # Only hacked in for 1 specific day. 
    # Don't forget to update this if you want to start using different days/sites
    path <- "/Users/n8933464/Documents/SERF/2014Apr02-190454 - Indices, OCT 2010, SERF/SERF/TaggedRecordings/NW/101013.mp3/Towsey.Acoustic/Acoustic.Indices.csv"
    
    indices <- read.csv(path, header = TRUE, stringsAsFactors=FALSE)
    
    indices <- indices[,colnames(indices) %in% cols.to.use]
    
    if (ncol(indices) != length(cols.to.use)) {
        stop("invalid col name")
    }
    
    return(indices)
    
    
    
    
}

RankMinutesFromIndices <- function () {
    
   which.indices <- c("H.spectral.", "H.spectralVar.", "clusterCount")
   weights <- c(0.5, 0.1, 0.4)
   
   indices <- ReadIndices(which.indices)
   indices[,1:2] <- 1-indices[,1:2]  # entropy indices are negatively correlated with species richness
   
   indices <- scale(indices)
   
   scores <- t(crossprod(weights, t(indices)))
   
   # hack the minute ids
   # assume minutes of the indices start at midnight
   min.id <- 1:nrow(scores)
   
   rank <- order(scores, decreasing = TRUE)
    
   return(data.frame(min.id = min.id, rank = rank))
    
}