RankSamples <- function () {
    #r1 <- RankSamples.1()
    
    r1 <- RankSamples1()
    r2 <- RankSamples2()
    r3 <- RankSamples3()
    r1 <- OrderBy(r1, 'min.id')
    r2 <- OrderBy(r2, 'min.id')
    r3 <- OrderBy(r3, 'min.id')
    total.rankings <- data.frame(min.id = r3$min.id, 
                                 r1 = r1$rank, s1 = r1$score, 
                                 r2 = r2$rank, s2 = r2$score, 
                                 r3 = r3$rank, s3 = r3$score)



    # add site,date,min cols to make it easier to examin
    total.rankings <- ExpandMinId(total.rankings)
    
    WriteOutput(total.rankings, 'ranked_samples')
    return(total.rankings)
}

IterateOnSparseMatrix <- function (multipliers = NA,  decay.rate = 1.2) {
    # ranks all the minutes in the target in the order 
    
    # that should find the most species in the shortest number of minute
    # samples
    #
    # Value:
    #   data.frame; cols: min.id, rank, score
    #
    # Details:
    # reads the list of clustered events (i.e. events with a 'group' column)
    # creates a sparse matrix of minutes/groups
    # multiplies each row (minute) by that minute's distance score. 
    # iterates over the rows of the matrix, determining the sum of each row
    # (i.e. the number of events * the distance score for that minute). On iteration n highest
    # scoring unranked minute given the rank n. 
    # before continuing to iteration n+1, the columns (groups) of the highest scoring minute in iteration n
    # are reduced. This reduces the influence these groups have over subsequent rankings, so that 
    # the whole range of cluster groups are included in high ranking mintues. 
    
    require('plyr')
    require('Matrix')
    
    Report(1, 'Ranking samples: method 1')
    events <- ReadOutput('clusters')
    
    # list of unique group-minute pairs 
    # (i.e. remove duplicate groups from the same minute)
    unique.cluster.minutes <- unique(events[, c('min.id', 'group')])
    num.clusters.per.min <- count(unique.cluster.minutes)
    
    #sparseMatrix goes from zero to the max min id. map min id to temporary minute ids
    # so that sparse matrix has the minimum rows needed
    
    unique.min.id <- unique(events$min.id)
    map <- data.frame(min.id = unique.min.id, temp.id = 1:length(unique.min.id))
    mapped.min.id <- map$temp.id[match(events$min.id, map$min.id)]
    cluster.matrix <- as.matrix(sparseMatrix(mapped.min.id, events$group)) * 1
    
    # multipliers <- multipliers[multipliers$min.id == unique.min.id  ,]
    mapped.multipliers <- multipliers$multiplier[match(map$min.id, multipliers$min.id)]
    # multiply each row by the distance score for that minute
    cluster.matrix <- cluster.matrix * mapped.multipliers
    
    # add a column for the min id, 
    empty.col <- rep(-1, nrow(cluster.matrix))
    
    rankings <- data.frame(temp.id = 1:nrow(cluster.matrix), rank = empty.col, score = empty.col)
    
    #initialise empty dataframe for storing the ranked minutes (including scores)
    #ranked.mins <- as.data.frame(matrix(rep(NA, 4*nrow(mins)), ncol = 4))
    
    # repeatedly select the best scoring minute until all minutes have been selected
    
   
    
    for (i in 1:nrow(cluster.matrix)) {
        Dot()
        unranked <- rankings$rank == -1
        if (class(cluster.matrix) == "matrix") {
            scores <- apply(cluster.matrix, 1, sum)
        } else  {
            #down to the last row, so the apply won't work
            scores <- sum(cluster.matrix)
        }
        best <- which.max(scores)
        # the best is the index out of the unranked 
        # need to find which actual minute id (ranked or unranked)
        real.best <- rankings$temp.id[unranked][best]
        rankings$rank[real.best] <- i
        rankings$score[real.best] <- scores[best]
        
        if (class(cluster.matrix) == "matrix") {
            #reduce the value of the found clusters, and 
            # remove the row for the next round
            found.clusters <- which(cluster.matrix[best, ] >0)
            cluster.matrix[,found.clusters] <- cluster.matrix[,found.clusters] / decay.rate
            cluster.matrix <- cluster.matrix[-best,]
        } else {
            break()
        }
        
    }
    
    # re-map back to actual minute ids
    ranked.min.ids <- map$min.id[rankings$temp.id]
    rankings <- data.frame(min.id = ranked.min.ids, score = rankings$score, rank = rankings$rank)
    
    #mins.sorted <- rankings[order(rankings$rank, decreasing = FALSE),]
    
    #append empty minutes
    mins <- ReadOutput('target.min.ids')
    unranked.ids <- setdiff(mins$min.id, rankings$min.id)
    
    if (length(unranked.ids > 0)) {
        unranked.mins <- data.frame(min.id = unranked.ids, rank = (max(rankings$rank)+1):nrow(mins), score = rep(0, length(unranked.ids)))
        rankings <- rbind(rankings, unranked.mins)  
    }
    
    
    return(rankings) 
    
    
    
    
}

RankSamples1 <- function () {
    # use the iterateOnSparseMatrix raking algorithm, 
    # using the distance scores as the multiplier
    
    multiplier <- ReadOutput('distance.scores')
    colnames(multiplier) <- c('min.id', 'multiplier')
    #multiplier$multiplier <- 1
    return(IterateOnSparseMatrix(multiplier))
  
    
}



RankSamples2 <- function () {
    
    events <- ReadOutput('clusters')
    
    Report(5, 'calculating number of events in each minute')
    # count removes duplicates and adds a 'freq' column which is the number 
    # of occurances of that row (i.e. the number of duplicates removed plus 1)
    multipliers <- count(as.data.frame(events$min.id))
    colnames(multipliers) <- c('min.id', 'multiplier')
    #Report(4, nrow(mins), 'minutes have at least one event')
    
    #unique.cluster.minutes <- unique(events[, c('min.id', 'group')])
    #num.clusters.per.min <- count(unique.cluster.minutes, vars = 'min.id')
    
    #initial.weight = mins$num.events
    
    # multipliers <- data.frame(min.id = num.clusters.per.min$min.id, rep(initial.weight, nrow(num.clusters.per.min)))
    
    return(IterateOnSparseMatrix(multipliers))
    
}

RankSamples3 <- function () {
    # ranks samples based purely on v.score
    # which is the internal distance only, influenced by the number and
    # diversity of events in each minute, but not the difference between minutes
    # i.e. similar minutes will can both score high  
    Report(1, 'Ranking samples: method 4')
    mins <- ReadOutput('distance.scores')
    
    mins.ranked <- mins[order(mins$distance.score, decreasing = TRUE),]
    mins.ranked$rank <- 1:nrow(mins)
    return(data.frame(min.id = as.vector(mins.ranked$min.id), rank = (mins.ranked$rank), score = as.vector(mins.ranked$distance.score)))
}

