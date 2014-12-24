
# Instructions:
# 1. Get the species.mins using GetSpeciesMins
# 2. Read the events from previous output using ReadOutput
# EITHER
# 3. Simulate artificial clustering using SimulateClustering
# 4. Calculate B-cubed using BCubed
# OR 
# 3. run EvaluateBCubed, which does the simulation many times (can take a while)

source('artificial.clustering.R')

GetACForVis <- function (fn = NULL, fraction.of.events = 0.4, fraction.of.minutes = 0.06, amount.random = 1, species.mins = NULL) {
    # shortcut function to get the output for visualization, 
    # mainly so that I can remember the steps needed
    if (is.null(species.mins)) {
        species.mins <- GetSpeciesMins()      
    }
    
    if (is.null(fn)) {
        fn = paste(fraction.of.events, fraction.of.minutes, amount.random, sep = "_")
    }

    events <- ReadOutput('events', include.meta = FALSE)
    reduced <- ReduceEventsAndMinutes(events, species.mins, fraction.of.events = fraction.of.events, fraction.of.minutes = fraction.of.minutes)
    
    num.events <- nrow(reduced$events)
    num.clusters <- round(length(unique(reduced$species.mins$species))*2)
    print(paste('num.events',num.events))  
    print(paste('num.clusters',num.clusters))
    
    reduced$events <- SimulateClustering(events = reduced$events, species.mins = reduced$species.mins, num.clusters = num.clusters)
    randomised <- AddRandomGroups(reduced$events, amount.random)
    diff = sum(reduced$events$group != randomised$group)
    print(paste('adding random. diff = ', diff, '/', nrow(reduced$events)))
    OutputForVis(fn, reduced)
    
}





GetSpeciesMins <- function () {
    # creates a list of species-minute pairs
    species.mins <- GetTags();
    species.mins <- AddMinuteIdCol(species.mins) 
    return(species.mins)
}

GetGroupMins <- function (species.mins = NA, use.ideal = FALSE, use.random = FALSE) {
    # creates a list of cluster group - minute pairs
    # Args
    #   use.ideal: boolean; if true it will simulate ideal clustering
    #   use.random: boolean; if true it will simulate very bad quality clusters (randomly assign groups)
    #                        if use.ideal and use random are both false, it will look for actual clustering results 
    #                        in the output csv files 
    if (use.ideal) {
        if (!is.data.frame(species.mins)) {
            stop('species.mins must be supplied if simulating ideal clustering')
        }
        group.mins <- SimulatePerfectClustering(species.mins)
    } else {
        if (use.random) {
            events <- ApplyRandomGroupToEvents()
        } else {
            events <- ApplyGroupToEvents()
        }
        cluster.mins <- events$data[c('min.id', 'group')]
        group.mins <- unique(cluster.mins)  
    }
    return(group.mins)
}





InspectCountEventsByGroupInMins <- function () {
    events <- ApplyGroupToEvents()
    counts <- CountEventsByGroupInMins(c(), events)
    counts <- counts[order(counts$all.events.count),]
    plot(counts$all.events.count)
}

CountEventsByGroupInMins <- function (min.ids, all.events) {
    minute.events <- all.events[all.events$min.id %in% min.ids, ]
    group.ids <- min(all.events$group):max(all.events$group)
    counts <- data.frame(group.id = group.ids, all.events.count = group.ids, minute.events.count = group.ids)
    if (group.ids != 1:max(all.events$group)) {
        # make sure group ids go from 1 to the maximum with no gaps
        # this might be caused by passing only some of the events
        # this would still fail events were excluded that included the only 
        # examples of the group with the highest id
        stop("something is wrong. some cluster groups are missing")
    }
    for (gid in group.ids) {       
        counts$all.events.count[gid] <- sum(all.events$group == gid)
        counts$minute.events.count[gid] <- sum(minute.events$group == gid)
    }
    return(counts)
}

InspectEventsPerGroupAllMins <- function () {
    events <- ApplyGroupToEvents()
    event.count.all.mins <- EventsPerGroupAllMins(events)
}

EventsPerGroupAllMins <- function (events) {
    min.ids <- unique(events$min.id)
    group.ids <- min(events$group):max(events$group)
    count.matrix = matrix(NA, ncol = length(group.ids), nrow = length(min.ids))
    for (i in 1:length(min.ids)) {
        minute.events <- events[events$min.id == min.ids[i], ]
        event.count.vector <- EventsPerGroup(minute.events, group.ids)
        count.matrix[i ,] <- event.count.vector
    }
    
    return(list(
        min.ids <- min.ids,
        event.counts <- count.matrix 
        ))
}

EventsPerGroup <- function (events, group.ids) {    
    count <- rep(NA, length(group.ids))
    
    
    for (i in group.ids) {       
        count[i] <- sum(events$group == group.ids[i])      
    }  
    return(count) 
}

BCubedAll.old <- function (cluster.minutes, species.minutes) {
    # calculate Bcubed precision for all clusters
    clusters <- unique(cluster.minutes$group)
    b.cubed.all <- rep(NA, length(clusters))
    
    jaccard.indexes <- ReadOutput('jaccard.indexes')
    
    for (i in 1:length(clusters)) {
        Dot()
        # the minutes that this cluster is contained in
        min.ids <- cluster.minutes$min.id[cluster.minutes$group == clusters[i]]
        
        b.cubed.all[i] <- BCubed(min.ids, jaccard.indexes$data)
    }
    
    return(b.cubed.all)
}

BCubed.old <- function (min.ids, jaccard.indexes = NA) {
    # preforms a kind of BCubed precision for a particular cluster
    # Args:
    #   min.ids: integer vector; the min.ids that this cluster appears in
    #   species.minutes: data.frame; pairs of species.ids and minute ids
    # Details: 
    #    There may be some minutes that this cluster appears in which are not in the species.minutes
    #    data frame, because there are no species in that minute (in that case the events are non-bird,
    #    but it is still possible)
    
    #pairs <- expand.grid(min.ids, min.ids)
    #pairs <- pairs[pairs[,2]>pairs[,1],] # remove same and reciprocals
    
    pairs <- jaccard.indexes[jaccard.indexes$min.a %in% min.ids & jaccard.indexes$min.b %in% min.ids, ]

    ## the mean of the jaccard indexes (total jaccard indexes divided by number of pairs, not number of minutes)
    bcubed <- mean(pairs$jaccard.index)
    return(bcubed)
}

JaccardIndexAll <- function (species.mins) {
    # creates a jaccard index for ALL minute pairs for which there are species present
    # this could take a while, because for N minutes there are (n*n-1)/2 pairs
    # BUT, if many randomised simluations are done, this will end up being quicker than re-calculating
    min.ids <- unique(species.mins$min.id)
    pairs <- expand.grid(min.a = min.ids, min.b = min.ids)
    pairs <- pairs[pairs[,2]>pairs[,1],] # remove same and reciprocals
    jaccard.index <- apply(pairs, 1, function (pair) {  
        a <- species.mins$species.id[species.mins$min.id == pair[1]]
        b <- species.mins$species.id[species.mins$min.id == pair[2]]
        intersection.length <- length(intersect(a, b))
        union.length <- length(union(a,b))
        return(intersection.length / union.length)   
    })
    pairs <- data.frame(min.a = pairs$min.a, min.b = pairs$min.b, jaccard.index = jaccard.index)
    #WriteOutput(pairs, 'jaccard.indexes', params = c(), dependencies = list())
    return(pairs)
}


EvaluateBCubed <- function (events, species.mins, fraction.of.events = 0.3, fraction.of.minutes = 0.3, iterations = 1) {
    # performs artificial clustering with a variety of known qualities, then gets the b-cubed score for them
    # to the correlation

    reduced <- ReduceEventsAndMinutes(events, species.mins, fraction.of.events, fraction.of.minutes)
    events <- reduced$events
    species.mins <- reduced$species.mins
    
    # get a complete list of species with tVheir weights
    species <- SpeciesWeights(species.mins)
    

    
    amount.random = seq(0,1,0.2)
    
    # scores is a matrix where each row is a different quality of artificial clustering
    # and columns are repetitions to get a mean, since there is a lot of randomness in the artificial clustering
    scores <- matrix(NA, nrow = length(amount.random), ncol = iterations)
    
    for (c in 1:iterations) {
        
        events <- SimulateClustering(events, species.mins)
        
        for (r in 1:length(amount.random)) {
            events.with.random <- AddRandomGroups(events, amount.random[c])
            scores[r, c] <- BCubed(events.with.random, species.mins, species)  
        }    
    }
   
    # row means and sds
    means <- apply(scores, 1, mean)
    sds <- apply(scores, 1, sd)
    
    return(data.frame(amount.random = amount.random, mean.score = means, sd = sd))


    
}

ReduceEventsAndMinutes <- function (events, species.mins, fraction.of.events, fraction.of.minutes) {
    # for testing, to reduce the number of events processed 
    # we randomly select a subset of the minutes
    
    min.ids <- unique(species.mins$min.id)
    num.minutes <- round(length(min.ids) * fraction.of.minutes)
    min.ids <- sample(min.ids, num.minutes, replace = FALSE)
    species.mins <- species.mins[species.mins$min.id %in% min.ids, ]
    events <- events[events$min.id %in% min.ids, ]
    
    # for testing, we can reduce the number of events as well
    num.events <- round(nrow(events) * fraction.of.events)
    events <- events[sample(nrow(events), num.events), ]
    
    return(list(events = events, species.mins = species.mins))
    
}


BCubed <- function (events, species.mins, species) {
    

    # get a list of cluster-species groups (clusters that appear in the same minute as the species)
    # and the number of times they appear (number of events of cluster x in a minute with species y)
    clusters.species <- ClustersSpecies(species.mins, events) 
    
    # set the priority for each species for each cluster
    clusters.species <- ClusterSpeciesPriorities(clusters.species, species)
    
    # for each event, select its priority species
    # i.e. the species in the minute it appears in which has the highest priority
    #  out of all the species its cluster is associated with
    events <- SetEventPriorityLabel(events, clusters.species)
    
    events <- SetEventPriorityLabelWeight(events, species)
    
    events <- EventPrecision(events)
    
    return(sum(events$precision * events$pl.weight) / sum(events$pl.weight))
    
    
}




SpeciesWeights <- function (species.mins) {
    # given a list of species-minute pairs, 
    # calcualtes the weight for each species. 
    # weight is the number of minutes a species occurs in / the total number of minutes that species occur in
    species <- as.data.frame(table(species.mins[, 'species.id'], dnn = c("species.id")))
    species$weight <- 1 - (species$Freq / sum(species$Freq))
    return(species)
}



ClustersSpecies <- function (species.mins, events) {
    # creates a dataframe of species-cluster pairs
    # including the number of events where this pair occurs
    # TODO: figure out a more efficient way to do this.
    #
    # args:
    #   species.mins: data.frame; a data frame of species minute pairs
    #   evemts: data.frame; a data frame of events, including the minute id and the group
    #
    # value:
    #   data.frame: species.id, group, Freq, 
    #      where Freq is the number of events in that cluster 
    #      which are in a minute with the species
    events <- events[,c('min.id','group')]
    clusters.species <- data.frame(group = c(), species.id = c())
    dot.every <- 30
    print(paste("please wait for ", round(nrow(events) / dot.every),"dots to appear"))
    for (i in 1:nrow(events)) {
        if (i %% dot.every == 0) {
            Dot()     
        }

        # get the species for this event's minute
        species <- species.mins$species.id[species.mins$min.id == events$min.id[i]]
        new.rows <- data.frame(group = rep(events$group[i], length(species)), species.id = species)
        clusters.species <- rbind(clusters.species, new.rows)
    }
    
    clusters.species <- as.data.frame(table(clusters.species))
    #remove species-cluster pairs that don't exist
    clusters.species <- clusters.species[clusters.species$Freq > 0,]
    
    return(clusters.species)  
}


ClusterSpeciesPriorities <- function (species.clusters, species) {
    # given a list of species-cluster pairs, and their frequency
    # adds priority column to each row
    # priority is ordered first by Freq (high to low) and then weight (high to low)
    # priority == 1 means that species has the highest priority in the 
    species.clusters.freq$priority <- NA
    groups <- unique(species.clusters$group)
    print(paste('please wait for ', length(groups), 'dots'))
    for (g in groups) {
        Dot()
        selection <- species.clusters$group == g
        species.in.cluster <- species.clusters[selection, ]
        species.weights <- species[species$species.id %in% species.in.cluster$species.id, ]
        species.in.cluster <- species.in.cluster[order(species.in.cluster$species.id),]
        species.weights <- species.weights[order(species.weights$species.id),]
        priority <- (1:sum(selection))[order(species.in.cluster$Freq, species.weights$weight, decreasing = TRUE)]
        species.clusters$priority[selection] <- priority  
    }
    return(species.clusters)
}

SetEventPriorityLabel <- function (events, clusters.species) {
    # given a df of events with group assigned to each event
    # and a list of cluster species pairs with priorities
    # for each event, chooses the label for that cluster with the highest priority
    
    
    labels <- sapply(events$group, function (g, clusters.species) {
        labels <- clusters.species[clusters.species$group == g, ]
        label <- labels$species.id[which.min(labels$priority)]
        return(label) 
    }, clusters.species)
    
    events$priority.label <- labels
    return(events) 
    
}

SetEventPriorityLabelWeight <- function (events, species) {
    events$pl.weight <- NA
    for (i in 1:nrow(species)) {
        events$pl.weight[events$priority.label == species$species.id[i]] <- species$weight[i]
    }
    return(events)
}

EventPrecision <- function (events) {
    # for each cluster:
    #   for each event
    #     sum the number of events of the same priority label
    
    groups <- unique(events$group)
    events$precision <- NA
    
    for (g in groups) { 
        Dot()
        event.selection <- events$group == g
        
        # debug:
        num.events.in.group <- sum(event.selection)
        num.unique.priority.species <- length(unique(events$priority.label[event.selection]))
        
        
        precisions <- sapply(events$priority.label[event.selection], function (this.pl, all.pls) {
            # mean on boolean treats them as 1 or 0
             return(mean(all.pls == this.pl))
        }, events$priority.label[event.selection]) 
        
        if (all(precisions == 1)) {
            print("1") 
        } else {
            print("different") 
        }
        
        events$precision[event.selection] <- precisions
        
    }
    return(events)
}





