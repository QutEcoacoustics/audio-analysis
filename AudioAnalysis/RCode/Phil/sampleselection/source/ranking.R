RankSamples <- function (use.lines = TRUE) {
 
    if (use.lines) {
        events <- ReadOutput('line.events') 
    } else {
        events <- ReadOutput('events')
    }

    mins <- ReadOutput('target.min.ids', level = 0)
    ranking.methods <- list()
    ranking.methods[[1]] <- RankSamples1
    ranking.methods[[2]] <- RankSamples2
    ranking.methods[[3]] <- RankSamples3
    ranking.methods[[4]] <- RankSamples4
    ranking.methods[[5]] <- RankSamples5
    ranking.methods[[6]] <- RankSamples6
    
    use.ranking.methods <- c(4,5,6)
    
    # the different number of clusters to perform ranking for
    num.num.clusters <- 5  # this many different numbers of clusters
    num.clusters.start <- 30 # lowest number of clusters
    num.clusters.multiplier <- 2 # each number of clusters is this many times the last
    num.clusters <- round(num.clusters.start * num.clusters.multiplier^(0:(num.num.clusters-1)))
    
    # make sure we are not trying to use more clusters than events
    num.clusters <- num.clusters[num.clusters < nrow(events)]
    
    output <- array(data = NA, dim = c(length(use.ranking.methods), length(num.clusters), nrow(mins)), dimnames = list(ranking.method = 1:length(use.ranking.methods), num.clusters = num.clusters, min.id = mins$min.id))

    groups <- as.data.frame(matrix(rep(NA, length(num.clusters) * nrow(events)), nrow = nrow(events)))
    groups.col.names <- paste0('group.', num.clusters)
    colnames(groups) <- groups.col.names
    groups <- cbind(events, groups)
    
    fit <- ReadObject('clustering', level = 2)
    for (n in 1:length(num.clusters)) {
        group <- cutree(fit, num.clusters[n])
        events$group <- group #temporarily add the group to the events for ranking
        groups[, groups.col.names[n]] <- group  # also add it here so it gets saved
        for (m in 1:length(use.ranking.methods)) {
            
            Report(1, 'Ranking samples:', use.ranking.methods[m], ' num.clusters:', num.clusters[n])
            
            
            
            r <- ranking.methods[[use.ranking.methods[m]]](events = events, min.ids = mins$min.id)   
            r <- r[order(r$min.id), ]
            output[m, n, ] <- r$rank
        }
    }
    SaveObject(output, 'ranked_samples', level = 2)
    WriteOutput(groups, 'clusters', level = 2)
    
}



RankSamples1 <- function (events, min.ids) {
    # use the iterateOnSparseMatrix raking algorithm, 
    # using the distance scores as the multiplier
    
    distance.scores <- ReadOutput('distance.scores', level = 2)
    multiplier <- data.frame(min.id = distance.scores$min.id, multiplier = distance.scores$distance.score)
    #multiplier$multiplier <- 1
    return(IterateOnSparseMatrix(events, multiplier))
    
    
}

RankSamples2 <- function (events, min.ids) {
    # use the iterateOnSparseMatrix raking algorithm, 
    # using the number of events as the multiplier
    
    Report(5, 'calculating number of events in each minute')
    # count removes duplicates and adds a 'freq' column which is the number 
    # of occurances of that row (i.e. the number of duplicates removed plus 1)
    multiplier <- count(as.data.frame(events$min.id))
    colnames(multiplier) <- c('min.id', 'multiplier')
    # multipliers <- data.frame(min.id = num.clusters.per.min$min.id, rep(initial.weight, nrow(num.clusters.per.min)))
    
    return(IterateOnSparseMatrix(events, multiplier))
    
}

RankSamples3 <- function (events, min.ids) {
    # ranks samples based purely on v.score
    # which is the internal distance only, influenced by the number and
    # diversity of events in each minute, but not the difference between minutes
    # i.e. similar minutes will can both score high 
    # input argument is just so that it fits with the conventions of ranking methods
    
    mins <- ReadOutput('distance.scores', level = 2)
    
    mins.ranked <- mins[order(mins$distance.score, decreasing = TRUE),]
    mins.ranked$rank <- 1:nrow(mins)
    
    return(data.frame(min.id = as.vector(mins.ranked$min.id), rank = (mins.ranked$rank), score = as.vector(mins.ranked$distance.score)))
}



RankSamples4 <- function (events, min.ids) {
    # rank samples using only the number of events
    # ignores clustering completely
    
    event.count <- as.data.frame(table(events$min.id))
    colnames(event.count) <- c('min.id', 'count')  
    event.count$min.id <- as.integer(as.character(event.count$min.id))
    missing.mins <- setdiff(min.ids, event.count$min.id)  
    missing.mins <- data.frame(min.id = missing.mins, count = 0)
    event.count <- rbind(event.count, missing.mins)
    event.count <- event.count[order(event.count$count, decreasing = TRUE),]
    o <- order(event.count$min.id)
    rank <- (1:nrow(event.count))[o]
    event.count <- event.count[order(event.count$min.id), ]
    return(data.frame(min.id = min.ids, rank = rank, score = event.count$count))
}


RankSamples5 <- function (events, min.ids) {
    # rank samples using sparse matrix iterator
    # multiplyer is 1 for all minutes. i.e. work only on number of clusters
    # decay rate means that previously found cluster groups are ignored
    
    return(IterateOnSparseMatrix(events, decay.rate = 10))
    
}

RankSamples6 <- function (events, min.ids) {
    # rank samples using sparse matrix iterator
    # multiplyer is 1 for all minutes.  i.e. work only on number of clusters
    # decay rate of 1 means that only the number of clusters per minute is considered
    # not whether they are used before
    
    return(IterateOnSparseMatrix(events, decay.rate = 1))
    
}



RankSamples7 <- function (events, min.ids) {
    
    ranked.by.event.count <- RankSamples4(events, min.ids)
    ordered.by.event.count <- ranked.by.event.count[order(ranked.by.event.count$rank), ]
    
    mins.has.events <- ordered.by.event.count[ordered.by.event.count$score > 0,]
    mins.no.events <- ordered.by.event.count[ordered.by.event.count$score == 0,]
    
    events.per.group.per.min <- EventsPerGroupAllMins(mins.has.events)
    
    block.size <- 5
    look.ahead <- 50
    
    mins.processed <- 1
    
    # initial events per group total is the highest ranked minute
    events.per.group.total <- EventCountByMin(mins.has.events$min.id[1])
    
    processed.min.ids <- c(mins.has.events$min.id[1])
    
    
    while (mins.processed + 1 < nrow(mins.has.events)) {
        if (mins.procesed + 1 + look.ahead > nrow(mins.has.events)) {
            look.ahead.range <- (mins.processed + 1):nrow(mins.has.events)
        } else {
            look.ahead.range <- (mins.processed + 1):(mins.processed + 1 + look.ahead)
        }
        # these are the current 
        check.mins <- mins.has.events[look.ahead.range, ]
        check.mins.events.per.group <- EventCountByMin(check.mins$min.id, events.per.group.per.min)
        
        # each score is the dot product of a minute's event count per cluster group
        # with the total event count per cluster group in the processed min ids
        scores <- crossprod(t(check.mins.events.per.group), events.per.group.total)
        
        
        
     
    }

    
}

EventCountByMin <- function (min.ids, events.per.group.per.min) {
    
    return(events.per.group.per.min$event.counts[events.per.group.per.min$min.ids %in% min.ids, ])
    
    
}






IterateOnSparseMatrix <- function (events, multipliers = NA,  decay.rate = 2.2) {
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
    
    # list of unique group-minute pairs 
    # (i.e. remove duplicate groups from the same minute)
    unique.cluster.minutes <- unique(events[, c('min.id', 'group')])
    #num.clusters.per.min <- count(unique.cluster.minutes$min.id)
    
    #sparseMatrix goes from zero to the max min id. map min id to temporary minute ids
    # so that sparse matrix has the minimum rows needed
    
    unique.min.id <- unique(events$min.id)
    map <- data.frame(min.id = unique.min.id, temp.id = 1:length(unique.min.id))
    mapped.min.id <- map$temp.id[match(events$min.id, map$min.id)]
    cluster.matrix <- as.matrix(sparseMatrix(mapped.min.id, events$group)) * 1
    
    # multipliers <- multipliers[multipliers$min.id == unique.min.id  ,]
    
    if (class(multipliers) != 'logical') {
        mapped.multipliers <- multipliers$multiplier[match(map$min.id, multipliers$min.id)]
    } else {
        mapped.multipliers <- rep(1, nrow(map))
    }
    

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
    mins <- ReadOutput('target.min.ids', level = 0)
    unranked.ids <- setdiff(mins$min.id, rankings$min.id)
    
    if (length(unranked.ids > 0)) {
        unranked.mins <- data.frame(min.id = unranked.ids, rank = (max(rankings$rank)+1):nrow(mins), score = rep(0, length(unranked.ids)))
        rankings <- rbind(rankings, unranked.mins)  
    }
    
    
    return(rankings) 
    
    
    
    
}

