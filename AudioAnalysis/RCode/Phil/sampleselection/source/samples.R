
RankSamples <- function () {
    #r1 <- RankSamples.1()
    r3 <- RankSamples.3()
    r4 <- RankSamples.4()
    r3 <- OrderBy(r3, 'min.id')
    r4 <- OrderBy(r4, 'min.id')
    total.rankings <- data.frame(r3.rank = r3$rank, r3.score = r3$score, r4.rank = r4$rank, r4.score = r4$score)
    mins <- ReadOutput('minlist')
    ranked.mins <- cbind(mins, total.rankings)
    WriteOutput(ranked.mins, 'ranked_samples')
    return(ranked.mins)
}


RankSamples.3 <- function () {
    # ranks all the minutes in the target in the order 
    # that should find the most species in the shortest number of minute
    # samples
    #
    # Value:
    #   data.frame; cols: min.id, rank, score
    #
    # Details:
    # reads the list of events as detected in part 1 of the whole process
    # combines this with the cluster-list
    # denotes which minute each event belongs i
    # selects minutes based on
    # - minutes with the most events
    # - minutes with the most clusters not previously assigned. 
    # first give each minute a 'rank' based on how many events it has
    # seconds give each minute a 'rank' based on how many clusters it has
    # order by the sum of the 2 ranks for final rank.
    require('plyr')
    require('Matrix')
    
    Report(1, 'Ranking samples: method 3')
    events <- ReadOutput('clusters')
    
    # number of events in each minute
    # 4 column dataframe: the three id columns and the frequency
    # minutes with zero events are discarded

    Report(5, 'calculating number of events in each minute')
    # count removes duplicates and adds a 'freq' column which is the number 
    # of occurances of that row (i.e. the number of duplicates removed plus 1)
    mins <- count(as.data.frame(events$min.id))
    colnames(mins)[colnames(mins) == 'freq'] = 'num.events'
    Report(4, nrow(mins), 'minutes have at least one event')
    
    # list of unique group-minute pairs 
    # (i.e. remove duplicate groups from the same minute)
    unique.cluster.minutes <- unique(events[, c('min.id', 'group')])
    num.clusters.per.min <- count(unique.cluster.minutes)
    
    
    # this should put the influence of the number of new clusters and
    # the number of events about equal. 
    initial.weight = max(mins$num.events) / max(num.clusters.per.min$freq)
    cluster.matrix <- as.matrix(sparseMatrix(events$min.id, events$group)) * 1
    cluster.matrix <- cluster.matrix * initial.weight
    
    # add a column for the min id, 
    empty.col <- rep(-1, nrow(cluster.matrix))

    rankings <- data.frame(1:nrow(cluster.matrix), empty.col, empty.col)
    colnames(rankings) <- c('min.id', 'rank', 'score')
    
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
        
        #scores <- scores + mins$num.events
        best <- which.max(scores)
        
        # the best is the index out of the unranked 
        # need to find which actual minute id (ranked or unranked)
        real.best <- rankings$min.id[unranked][best]
        rankings$rank[real.best] <- i
        rankings$score[real.best] <- scores[best]
        
        if (class(cluster.matrix) == "matrix") {
            #reduce the value of the found clusters, and 
            # remove the row for the next round
            found.clusters <- which(cluster.matrix[best, ] >0)
            cluster.matrix[,found.clusters] <- cluster.matrix[,found.clusters] / 2
            cluster.matrix <- cluster.matrix[-best,]
        } else {
            break()
        }

    }
    
    #mins.sorted <- rankings[order(rankings$rank, decreasing = FALSE),]
    
    #append empty minutes
    mins <- ReadOutput('minlist')
    mins$min.id <- 1:nrow(mins)
    unranked.ids <- setdiff(mins$min.id, rankings$min.id)
    
    if (length(unranked.ids > 0)) {
        unranked.mins <- data.frame(min.id = unranked.ids, rank = (max(rankings$rank)+1):nrow(mins), score = rep(0, length(unranked.ids)))
        rankings <- rbind(rankings, unranked.mins)  
    }

    
    return(rankings) 
    
    

    
}


RankSamples.4 <- function () {
    # ranks samples based purely on v.score
    # which is the internal distance only, influenced by the number and
    # diversity of events in each minute, but not the difference between minutes
    # i.e. similar minutes will can both score high
    
    Report(1, 'Ranking samples: method 2')
    mins <- ReadOutput('minlist')
    mins$min.id <- 1:nrow(mins)
    mins.ranked <- mins[order(mins$v.score, decreasing = TRUE),]
    mins.ranked$rank <- 1:nrow(mins)

    

    return(data.frame(min.id = as.vector(mins.ranked$min.id), rank = (mins.ranked$rank), score = as.vector(mins.ranked$v.score)))
    
}


RankSamples.2 <- function () {
    # ranks all the minutes in the target in the order 
    # that should find the most species in the shortest number of minute
    # samples
    #
    # Details:
    # reads the list of events as detected in part 1 of the whole process
    # combines this with the cluster-list
    # denotes which minute each event belongs i
    # selects minutes based on
    # - minutes with the most events
    # - minutes with the most clusters not previously assigned. 
    # first give each minute a 'rank' based on how many events it has
    # seconds give each minute a 'rank' based on how many clusters it has
    # order by the sum of the 2 ranks for final rank.
    require('plyr')
    
    Report(1, 'Ranking samples: method 1')
    events <- ReadOutput('clusters')
    
    # number of events in each minute
    # 4 column dataframe: the three id columns and the frequency
    # minutes with zero events are discarded
    id.cols <- c('site','date','min')
    Report(5, 'calculating number of events in each minute')
    # count removes duplicates and adds a 'freq' column which is the number 
    # of occurances of that row (i.e. the number of duplicates removed plus 1)
    mins <- count(as.data.frame(events[,id.cols]))
    colnames(mins)[colnames(mins) == 'freq'] = 'num.events'
    Report(4, nrow(mins), 'minutes have at least one event')
    
    # list of unique group-minute pairs 
    # (i.e. remove duplicate groups from the same minute)
    unique.cluster.minutes <- unique(events[, c(id.cols, 'group')])
    num.clusters.per.min <- count(unique.cluster.minutes[,1:length(id.cols)])
    
    
    
    # this should put the influence of the number of new clusters and
    # the number of events about equal. 
    initial.weight = max(mins$num.events) / max(num.clusters.per.min$freq)
    
    # cluster weight is initialised at 1. Each time the cluster is used in a
    cluster.list <- unique(events$group)
    cluster.weights <- rep(1, length(cluster.list))
    cluster.list <- data.frame(cluster.list, cluster.weights)
    colnames(cluster.list) <- c('group', 'weight')
    
    #initialise empty dataframe for storing the ranked minutes (including scores)
    ranked.mins <- as.data.frame(matrix(rep(NA, 4*nrow(mins)), ncol = 4))
    
    # repeatedly select the best scoring minute until all minutes have been selected
    
    for (i in 1:nrow(mins)) {
        Dot()
        best <- FindBestMin(mins, unique.cluster.minutes, cluster.list)
        best.index <- best$index
        best.min <- mins[best.index, ]
        ranked.mins$site[i] <- best.min$site
        ranked.mins$date[i] <- best.min$date
        ranked.mins$min[i] <- best.min$min
        ranked.mins$score[i] <- best$score
        
        # update scores
        found.clusters <- FilterByMin(unique.cluster.minutes, best.min, 'group')
        indexes <- cluster.list$group == found.clusters
        cluster.list$weight[indexes] <- cluster.list$weight[indexes] / 2
        
        #remove best min from list of mins
        mins <- mins[-best.index,]
        
    }
    
    ranked.mins <- as.data.frame(ranked.mins)
    colnames(ranked.mins) <- c('site', 'date', 'min', 'score')
    
    return(ranked.mins) 

    
# 
#     Report(4, nrow(unique.cluster.minutes), 'cluster minutes ')
#     # todo: check this part
#     #   Report(4, nrow(num.clusters.per.min), '')
#     
#     
#     mins <- cbind(num.events.per.min, 
#                   num.clusters.per.min[,ncol(num.clusters.per.min)])
#     
#     col.names <- c('date', 'site', 'min','num.events','num.clusters')
#     colnames(mins) <- col.names
#     
#     score <- scale(mins$num.events) + scale(mins$num.clusters, center = FALSE)
#     mins <- cbind(mins, score)
#     
#     mins.sorted <- mins[order(mins[,ncol(mins)], decreasing = TRUE),]
#     
#     # Todo: improve selection method
#     Report(2, 'Sorting minutes based on number of unique cluster groups and number of events')
#     if (nrow(mins.sorted) < g.num.samples) {
#         g.num.samples <- nrow(mins.sorted)
#     }
#     
#     
#     return(mins.sorted) 
    
}


FindBestMin <- function (mins, cluster.mins, cluster.weights) {
    # gives a score to each minute
    #
    # Args: 
    #   mins: data.frame (site, date, min, num.events) the list of minutes to give a score to
    #         including the number of events in that min
    #   cluster.mins: list of minute-cluster pairs
    #   cluster.weights: list of cluster group number and their current weight
    #
    # Value:
    #    list containing
    #    best: int. The index of the highest scoring minute
    #    score: numeric: the score of that minute
    #
    # Details:
    #   as minute samples are selected, the weight of the clusters present in those 
    #   minutes goes down, so that future samples are selected which have clusters that
    #   have not appeared before
    
    # give a weight to each cluster.min
    require('plyr')
    
    weight <- sapply(cluster.mins$group, function (group.num) {
        return(cluster.weights$weight[cluster.weights$group == group.num])
    })
    cluster.mins <- cbind(cluster.mins, weight)
    mins.scored <- adply(mins, 1, function (min) {    
        score <- sum(FilterByMin(cluster.mins, min, 'weight'))
        score <- score + min$num.events
        return(score) 
    })

    score <- mins.scored$V1
    return(list(index = which.max(score), score = max(score)))
}

FilterByMin <- function (df, mins, cols = NA) {
    # given a dataframe that contains the minute id cols
    # i.e. site, date, min, returns the rows of df that contain
    # those values for the id cols
    #
    # Args:
    #   df: data.frame. Must have the columns site, date, min
    #   mins: data.frame. must have the colums, site, date, min
    #   cols: character vector. which cols to return from df. NA to return all
    #   simplify: boolean. If true, will return a vector if cols only contains
    #     one column
    #   
    #
    # Value:
    #   data.frame if cols > 1 or NA,
    #   vector if cols == 1
    if (any(is.na(cols))) {
        cols <- colnames(df)
    }
    new.df <- df[df$site == mins$site & df$date == mins$date & df$min == mins$min, cols]
    return(new.df)  
}


RankSamples.1 <- function () {
    # ranks all the minutes in the target in the order 
    # that should find the most species in the shortest number of minute
    # samples
    #
    # Details:
    # reads the list of events as detected in part 1 of the whole process
    # combines this with the cluster-list
    # denotes which minute each event belongs i
    # selects minutes based on
    # - minutes with the most events
    # - minutes with the most clusters not previously assigned. 
    # first give each minute a 'rank' based on how many events it has
    # seconds give each minute a 'rank' based on how many clusters it has
    # order by the sum of the 2 ranks for final rank.
    require('plyr')
    
    Report(1, 'Ranking samples: method 1')
    events <- ReadOutput('clusters')
    
    # adds a column which denotes which minute of the day the event happened in
    events <- SetMinute(events)
    
    minute.col <- ncol(events)
    group.col <- minute.col - 1
    date.site.cols <- match(c('date', 'site'), colnames(events))
    
    # the number of the columns with the site, date, and minute of the day
    # to identify a unique minute recording
    id.cols <- c(date.site.cols, minute.col)
    
    # number of events in each minute
    # 4 column dataframe: the three id columns and the frequency
    # minutes with zero events are discarded
    Report(5, 'calculating number of events in each minute')
    num.events.per.min <- count(as.data.frame(events[,id.cols]))
    Report(4, nrow(num.events.per.min), 'minutes have at least one event')
    
    # list of unique group-minute pairs 
    # (i.e. remove duplicate groups from the same minute)
    unique.cluster.minutes <- unique(events[, c(id.cols, group.col)])
    num.clusters.per.min <- count(unique.cluster.minutes[,1:length(id.cols)])
    Report(4, nrow(unique.cluster.minutes), 'cluster minutes ')
    # todo: check this part
    #   Report(4, nrow(num.clusters.per.min), '')
    
    
    mins <- cbind(num.events.per.min, 
                  num.clusters.per.min[,ncol(num.clusters.per.min)])
    
    col.names <- c('date', 'site', 'min','num.events','num.clusters')
    colnames(mins) <- col.names
    
    score <- scale(mins$num.events) + scale(mins$num.clusters, center = FALSE)
    mins <- cbind(mins, score)
    
    mins.sorted <- mins[order(mins[,ncol(mins)], decreasing = TRUE),]
 
    # Todo: improve selection method
    Report(2, 'Sorting minutes based on number of unique cluster groups and number of events')
    if (nrow(mins.sorted) < g.num.samples) {
        g.num.samples <- nrow(mins.sorted)
    }
    
    
    return(mins.sorted) 
    
}


OptimalSamples <- function (speciesmins = NA, mins = NA, num.samples = NA) {
    # determines the best possible selection of [numsamples] minute samples
    # to find the most species
    # 
    # Args:
    #   speciesmins: dataframe; the list of species in each minute. If not included
    #                            will retreive from database
    #   mins: dataframe; the list of minutes we can select from. If not
    #                    included then will be read from config
    #   num.samples: int; how many samples to select. If not supplied, 
    #                     will be read from config
    #
    # Value:
    #   list: containing
    #       data.frame; A list of minutes. Cols: site, date, min
    #       the progression of species found in each of those minutes
    #       the progression of total species found count after each minute
    
    if (class(speciesmins) != 'data.frame') {
        speciesmins <- GetTags()
    }
    
    if (class(mins) != 'data.frame') {
        mins <- ReadOutput('minlist', false.on.missing = TRUE)
        if (class(mins) != 'data.frame') {
            CreateMinuteList()
            mins <- ReadOutput('minlist')
        }
    }
    
    if (is.na(num.samples)) {
        num.samples <- g.num.samples
    }
    

    total.num.species <- length(unique(speciesmins$species.id))
    # maximum number of samples is the number of species
    selected.samples <- rep(NA, total.num.species)
    found.species.count.progression <- rep(NA, total.num.species)
    found.species.progression <- vector("list", total.num.species)
    all.found.species <- numeric()
    
    # create list of the species in each minute
    species.in.each.min <- ListSpeciesInEachMinute(speciesmins, mins = mins) 

    for(sp in 1:length(selected.samples)) {
        # find minute with most species
        max.sp <- 0
        max.sp.i <- -1
        for (m in 1:length(species.in.each.min)) {
            if (length(species.in.each.min[[m]]) > max.sp) {
                max.sp <- length(species.in.each.min[[m]])
                max.sp.i <- m
            } 
        }
        
        if (max.sp == 0) {
            # all species have been included in the selected mins 
            # (or there were no species)
            break()
        }
        
        #record that minute
        selected.samples[sp] <- max.sp.i
        last.found.species <- species.in.each.min[[max.sp.i]]
        all.found.species <- union(all.found.species, last.found.species)
        found.species.progression[[sp]] <- all.found.species
        found.species.count.progression[sp] <- length(all.found.species)
        #remove the already found species from the list
        for (m in 1:length(species.in.each.min)) { 
               sp <- species.in.each.min[[m]]
               species.in.each.min[[m]] <- sp[! sp %in% last.found.species]
        }

    }
    
    selected.samples <- selected.samples[! is.na(selected.samples)]
    found.species.count.progression <- found.species.count.progression[! is.na(found.species.count.progression)]
    selected.sample.mins <- mins[selected.samples,]
    

    
    return(list(
        found.species.progression = found.species.progression,
        found.species.count.progression = found.species.count.progression,
        selected.mins = selected.samples
        ))
    
    
    
}

RandomSamplesAtDawn <- function (speciesmins = NA, mins = NA, num.repetitions = 20, dawn.from = 315, dawn.to = 495) {
    
    if (class(speciesmins) != 'data.frame') {
        speciesmins <- GetTags()
    }
    
    if (class(mins) != 'data.frame') {
        mins <- ReadOutput('minlist', false.on.missing = TRUE)
        if (class(mins) != 'data.frame') {
            CreateMinuteList()
            mins <- ReadOutput('minlist')
        }
    }

    mins <- mins[mins$min >= dawn.from & mins$min <= dawn.to ,]
    species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, mins = mins)
    repetitions <- matrix(rep(NA, num.repetitions * length(species.in.each.sample)), ncol = num.repetitions)
    # get the progression for random mins many times
    for (i in 1:num.repetitions) {
        # create a jumbled version of the list of species in each min
        sample.order <- sample(1:nrow(mins), nrow(mins), replace = FALSE)
        species.in.each.sample.random <- species.in.each.sample[sample.order]
        found.species.progression <- GetProgression(species.in.each.sample.random)
        repetitions[,i] <- found.species.progression$count  
    }
    #get average progression of counts 
    progression.average <- apply(repetitions, 1, mean)
    progression.average <- round(progression.average)
    return(progression.average)
    
    
    
}


CountSpecies <- function (selected.samples, speciesmins) {
    # finds which species were present in the minutes supplied in selected.samples
    # out of a full species list speciesmins
    found.species <- DoSpeciesCount(selected.samples, speciesmins)
    min.list <- ReadOutput('minlist')
    total.species <- DoSpeciesCount(min.list, speciesmins)
    Report(1, 'number of species found = ', length(found.species))   
    if (length(found.species) > 0) {
        Report(2, 'species list:')
        Report(2, found.species)
    }
    if (length(total.species) == 0) {
        percent <- 100
    } else {
        percent <- length(found.species) * 100 / length(total.species)
    }
    Report(1, percent,"% of ", length(total.species)," species present")
}


DoSpeciesCount <- function (sample.mins, speciesmins) {
    #
    # Args:
    #   sample.mins: data.frame
    #       table with the columns site, date, min
    #    species.mins: data.frame
    #       table with the columns site, date, min
    
    
    found.species <- c()
    for (i in 1:nrow(sample.mins)) {
        cond <- speciesmins$start_date == sample.mins$date[i] & 
            speciesmins$site == sample.mins$site[i] &
            speciesmins$min == sample.mins$min[i]
        rownums <- which(cond)
        hits <- speciesmins[rownums, ]  
        
        if (nrow(hits) > 0) {
            found.species <- c(found.species, as.vector(hits$species_id))
        }
    }
    found.species <- unique(found.species)  
    return(found.species)
}

ListSpeciesInEachMinute <- function (speciesmins, min.ids = NA, mins = NA) {
    # given a list of minute ids, and a list of all tags
    # creates a list of species for each minute in mins
    #
    # Args:
    #   min.ids vector of minute ids
    #   mins: data frame of minutes
    #   speciesmins: list of all the tags
    #  
    # Returns: list
    #
    # Details:
    #  if min.ids is supplied, this will be used for the list of mins
    #  if mins.ids is not supplied, mins must be supplied. 
    

    
    
    if (class(min.ids) == 'integer' || class(min.ids) == 'numeric' ) {
        minlist <- ReadOutput('minlist')
        mins <- minlist[min.ids,]
    } else if (class(mins) != 'data.frame') {
        stop('either min.ids or mins must be supplied as arguments')
    }
    
    
    
    species.in.each.min <- vector("list", nrow(mins))
    for (i in 1:nrow(mins)) {
        sp.list <- speciesmins$species.id[speciesmins$site == mins$site[i] & speciesmins$date == mins$date[i] & speciesmins$min == mins$min[i]]
        species.in.each.min[[i]] <- sp.list
    }
    return(species.in.each.min)
}


EvaluateSamples <- function (samples = NA) {
    # given a list of minutes
    # simulates a species richness survey, noting the 
    # total species found after each minute, and the total
    # number of species found after each minute
    
    if(is.na(samples)) {
        samples <- ReadOutput('ranked_samples')
    }
    
    speciesmins <- GetTags();
    total.num.species <- length(unique(speciesmins$species.id))
    Report(1, 'total species count: ', total.num.species)
    species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, min.ids = 1:nrow(samples))
    
    
    
    found.species.progression.3 <- GetProgression(species.in.each.sample, samples$r3.rank)
    found.species.progression.4 <- GetProgression(species.in.each.sample, samples$r4.rank)
    
    
    
    optimal <- OptimalSamples()
    random.at.dawn <- RandomSamplesAtDawn(speciesmins = speciesmins)
    
    #plot them against each other
    
    par(col = 'black')
    heading <- "Species count progression"
    plot(found.species.progression.3$count, main=heading, type = 'n')
    par(col = 'red')
    points(found.species.progression.3$count, type='l')
    par(col = 'orange')
    points(found.species.progression.4$count, type='l')
    par(col = 'blue')
    points(optimal$found.species.count.progression, type='l')
    par(col = 'green')
    points(random.at.dawn, type='l')
    
    
    # create dataframe of progression for csv output
    num.species = sapply(species.in.each.sample, length)
    num.new.species = found.species.progression$new.count
    species = sapply(species.in.each.sample,  paste, collapse = ', ') 
    new.species = sapply(found.species.progression$new.species, paste, collapse = ', ')
    output <- data.frame(num.species = num.species, 
                         num.new.species = num.new.species,
                         species = species,
                         new.species = new.species)
                         
    output <- cbind(samples, output)
                         
    WriteOutput(output, 'richness_results')
    

    legend("bottomright",  legend = c("smart sampling 3", 
                                      "smart sampling 4", 
                                      "optimal sampling", 
                                      "Random at dawn"), 
           col = c('red', 'blue', 'green'), 
           lty = c(2, 2), text.col = "black")
    
    
}


GetProgression <- function (species.in.each.sample, order = NA) {
    # returns the count of new species given a list
    # of species vectors
    # 
    # Args: 
    #   species.in.each.sample: list
    #   order: vector; the order in which to look at each sample. 
    #                  must be same length as arg 1. Optional
    #
    # Value:
    #   list
    #
    # Details:
    #   example input: list(c(1,2,3), c(3,4,5), c(4,5,7), c(5,7))
    #   output: list containing a list for each sample
    #           each of those lists contains a list or vector corresponding to the 
    #           input list
    #           - count: vector; the total number of species up until each of the samples
    #           - new.count: vector; the number of new species for each sample
    #           - species: list of vectors which contain the total species ids until each sample
    #           - new.species: list of vectors which contain the new species found in each sample
    
    
    
    
    found.species.count.progression <- rep(NA, length(species.in.each.sample))
    found.species.progression <- vector("list", length(species.in.each.sample))
    new.species.count.progression <- rep(0, length(species.in.each.sample))
    new.species.progression <- vector("list", length(species.in.each.sample))
    all.found.species <- numeric()
    
    if (class(order) == 'integer') {
        species.in.each.sample <- species.in.each.sample[order]
    }
    
    
    
    for (i in 1:length(species.in.each.sample)) {
        new.species <- setdiff(species.in.each.sample[[i]], all.found.species) 
        all.found.species <- c(all.found.species, new.species)
        found.species.count.progression[i] <- length(all.found.species)
        new.species.count.progression[i] <- length(new.species)
        found.species.progression[[i]] <- all.found.species
        new.species.progression[[i]] <- new.species
    }

    found.species.count.progression <- found.species.count.progression[! is.na(found.species.count.progression)]
    
    return(list(count = found.species.count.progression, 
                new.count = new.species.count.progression, 
                species = found.species.progression, 
                new.species = new.species.progression))
    
    
}







SetMinute <- function (events)  {
    # for a list of events which contains the filename 
    # (which has the start time for the file encoded)
    # and the start time of the event, works out the minute 
    # of the day that the event happened in
    
    start.sec.col <- which( colnames(events) == "start.sec" )
    min <- apply(events, 1, function (v) {
        sec <- as.numeric(unlist(v[start.sec.col]))
        min <- floor(sec / 60)
        return (min)
    })
    
    
    new <- cbind(events, min)
    
    return (new)
    
}




InspectSamples <- function (samples = NA) {
    # draws the ranked n samples as spectrograms
    # with events marked and colour coded by cluster group
    if(class(samples) == 'integer' || class(samples) == 'numeric') {
        #minute ids have been supplied
        minlist <- ReadOutput('minlist');
        minlist$min.id <- 1:nrow(minlist)
        samples <- minlist[samples,]
    } else {
        if(class(samples) != 'data.frame') {
            samples <- ReadOutput('ranked_samples')
            samples <- samples[1:min(nrow(samples), g.num.samples),]
        }
    }

    events <- ReadOutput('clusters')
    events <- AssignColourToGroup(events)
    #event.col <- as.data.frame(rep(NA, nrow(samples)))
    #colnames(event.col) <- c('events')
    #samples <- cbind(samples, event.col)
    
    w <- 1000
    # todo: fix this so that it the height of each spectrogram
    # is what it actually is, instead of hardcoded 256
    h <- nrow(samples) * 256
    
    temp.dir <- TempDirectory()
    
    # file names for later use in imagemagick command to append together
    im.command.fns <- ""
    
    for (i in 1:nrow(samples)) {
        
        #add events that belong in this sample
        min.id <- as.character(samples$min.id[i])
        minute.events <- events[which(events$min.id == min.id),]
        
        temp.fn <- paste(i, 'png', sep = '.')
        img.path <- file.path(temp.dir, temp.fn)
        im.command.fns <- paste(im.command.fns, img.path)
        
        Report(4, 'inspecting min id ', min.id)
        Report(4, 'num events = ', nrow(minute.events))
        
    # TODO: get this to work    
#         wav <- Audio.Targeted(site = as.character(samples$site[i]),
#                               start.date = as.character(samples$date[i]), 
#                               start.sec = as.numeric(samples$min[i] * 60), 
#                               duration = 60,
#                               save = TRUE)

        Sp.CreateTargeted(site = samples$site[i], 
                          start.date = samples$date[i], 
                          start.sec = samples$min[i] * 60, 
                          duration = 60, 
                          img.path = img.path, 
                          rects = minute.events)
        
        
        
    }

    fn <- paste('InspectSamples', samples$min.id,  collapse = "_")

    output.file <- OutputPath(fn, ext = 'png')
    command <- paste("/opt/local/bin/convert", 
                     im.command.fns, "-append", output.file)
    
    err <- try(system(command))  # ImageMagick's 'convert'

    
}

AssignColourToGroup <- function (events) {
    # adds a "color" column to the events with a hex color
    # for each cluster group
    #
    # Args:
    #   events: data.frame
    #
    # Returns: 
    #   data.frame; same as input but with an extra column
    
    groups <- unique(events$group)   
    num.groups <- length(groups)
    colors <- rainbow(num.groups)
    Report(6, 'Cluster group colors', colors)
    event.colors <- events$group
    for (i in 1:num.groups) {
        event.colors[event.colors == groups[i]] <- colors[i]
    }
    event.colors <- as.data.frame(event.colors)
    colnames(event.colors) <- "rect.color"
    events <- cbind(events, event.colors)
    return(events)
    
}

AddMinuteIdCol <- function (data) {
    
    cols <- colnames(data)
    date.col <- match('date', cols)
    site.col <- match('site', cols)
    min.col <- match('min', cols)
    sec.col <- match('start.sec', cols)
    
    ids <- apply(as.matrix(data), 1, function (v) {
        
        
        
        if (is.na(min.col)) {
            min <- floor(as.numeric(v[sec.col]) / 60)
        } else {
            min <- as.numeric(v[min.col])
        }
        
        id <- paste0(v[date.col], v[site.col], min)
        return(id)
        
        
    })
    
    new.data <- cbind(data, ids)
    colnames(new.data) <- c(cols, 'min.id')
    
    return(new.data)
    
    
    
}

