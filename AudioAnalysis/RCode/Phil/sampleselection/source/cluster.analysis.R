

ClusterQuality <- function (version = 3, use.ideal = FALSE) {
    events <- ApplyGroupToEvents() 


    species.mins <- GetTags();
    species.mins <- AddMinuteIdCol(species.mins)
    
    if (use.ideal) {
        group.mins <- SimulatePerfectClustering(species.mins)
    } else {
        cluster.mins <- events$data[c('min.id', 'group')]
        group.mins <- unique(cluster.mins)   
    }
    
    
    groups <- unique(group.mins$group)
    species <- unique(species.mins$species.id)
    groups <- groups[order(groups)]
    species <- species[order(species)]
    
    m <- matrix(NA, nrow = length(species), ncol = length(groups))

    for (s in 1:length(species)) {
        Dot()
        for (g in 1:length(groups)) {
            if (version == 1) {
                m[s,g] <- MatchSpeciesGroup(species.id = species[s], group = groups[g], species.mins, group.mins, TRUE)
            } else if (version == 2) {
                m[s,g] <- MatchSpeciesGroup(species.id = species[s], group = groups[g], species.mins, group.mins, FALSE)
            } else if (version == 3) {
                m[s,g] <- MatchSpeciesGroup2(species.id = species[s], group = groups[g], species.mins, group.mins)
            } else if (version == 4) {
                m[s,g] <- MatchSpeciesGroup3(species.id = species[s], group = groups[g], species.mins, group.mins)
            }
        }
    }
    
    return(m)
    
}


SimulatePerfectClustering <- function (species.mins = NULL) {
    
    if (is.null(species.mins)) {
        species.mins <- GetTags();
        species.mins <- AddMinuteIdCol(species.mins)
    }
    species.ids <- unique(species.mins$species.id)
    species.ids <- species.ids[order(species.ids)]
    species.mins <- species.mins[, c('species.id', 'min.id')]
    
    
    # create a list of "calls" call belongs to species, species has many calls
    call.ids <- 1:(length(species.ids)*2)
    calls <- data.frame(call.id <- call.ids, species.id = sample(species.ids, length(call.ids), replace = TRUE))
    calls$species.id[1:length(species.ids)] <- species.ids  # ensure all species have at least 1 call
    
    
    # each call has 1 or more events. Each event belongs to 1 cluster. So, each call has 1 or more clusters. each cluster belongs to 1 call
    
    cluster.ids <- 1:(length(call.ids) * 2)
    clusters <- data.frame(cluster.id = cluster.ids, call.id = sample(call.ids, length(call.ids), replace = TRUE))
    clusters$call.id[1:length(call.ids)] = call.ids  # ensure all calls have at least 1 cluster
    
    # so, now we have each species with a number 1 or more calls (average of 2)
    # and each call has 1 or more cluster groups (average of 2)
    # assume that in any particular minute, only one call from each species is present. i.e. a species won't call with 2 different call types in the same minute
    

    
    # for each species-minute pair, assign a call.id
    # then add the relevant events to the cluster.minues df
    species.mins$call.id <- rep(NA, nrow(species.mins))
    for (s.id in species.ids) {   
        species.mins.rows <- which(species.mins$species.id == s.id) 
        species.call.ids <- calls$call.id[calls$species.id == s.id]
        
        if (length(species.call.ids) == 1) {
            species.mins$call.id[species.mins.rows] <- species.call.ids
        } else {
            species.mins$call.id[species.mins.rows] <- sample(species.call.ids, length(species.mins.rows), replace = TRUE)
        }                                  
    }
    
    # create a list of cluster-minute pairs
    cluster.minutes <- data.frame(min.id = integer(), group = integer())
    
    for (i in 1:nrow(calls)) {
        this.species.mins <- species.mins[species.mins$call.id == calls$call.id[i], ]
        this.clusters <- clusters[clusters$call.id == calls$call.id[i], ]
        this.cluster.minutes <- data.frame(min.id = rep(this.species.mins$min.id, times = nrow(this.clusters)),  group = rep(this.clusters$cluster.id, times = 1, each = nrow(this.species.mins)))
        cluster.minutes <- rbind(cluster.minutes, this.cluster.minutes)
    }


    
    return(cluster.minutes)

    
}

MatchSpeciesGroup3 <- function (species.id, group, species.mins, group.mins) {
    # finds the number of minutes containing both particular species and a particular group
    # divided by the number of minutes containing the species 
    
    mins.with.species <- species.mins$min.id[species.mins$species.id == species.id]
    mins.with.group <- group.mins$min.id[group.mins$group == group] 
    mins.with.both <- intersect(mins.with.species, mins.with.group)
    
    #score <-  (chance.of.both.if.random ) /  ((length(mins.with.both) / total.num.mins) + )
    
    score <- length(mins.with.both)^2 / (length(mins.with.species) * length(mins.with.group))
    
    return(score)
    
}

MatchSpeciesGroup2 <- function (species.id, group, species.mins, group.mins) {
    # finds the number of minutes containing both particular species and a particular group
    # divided by the number of minutes containing the species 
    
    total.num.mins <- length(unique(c(species.mins$min.id, group.mins$min.id)))

    mins.with.species <- species.mins$min.id[species.mins$species.id == species.id]
    mins.with.group <- group.mins$min.id[group.mins$group == group] 
    mins.with.both <- intersect(mins.with.species, mins.with.group)
    
    chance.of.both.if.random <- (length(mins.with.species)/total.num.mins) * (length(mins.with.group)/total.num.mins)
    
    #score <-  (chance.of.both.if.random ) /  ((length(mins.with.both) / total.num.mins) + )
    
    score <- (length(mins.with.both) / total.num.mins) / chance.of.both.if.random
    
    
    return(score)
    
}


MatchSpeciesGroup <- function (species.id, group, species.mins, group.mins, over.species = TRUE) {
    # finds the number of minutes containing both particular species and a particular group
    # divided by the number of minutes containing the species
    
    mins.with.species <- species.mins$min.id[species.mins$species.id == species.id]
    mins.with.group <- group.mins$min.id[group.mins$group == group] 
    mins.with.both <- intersect(mins.with.species, mins.with.group)
    if (over.species) {
        return(length(mins.with.both)/length(mins.with.species))  
    } else {
        return(length(mins.with.both)/length(mins.with.group)) 
    }

}




ApplyGroupToEvents <- function (num.cluster.groups = 240, events = NA, clustering = NA) {  
    if (!is.data.frame(events)) {
        events <- ReadOutput('events')
        fit <- ReadOutput('clustering')
    } 
    group <- cutree(fit$data, num.cluster.groups)
    events$data$group <- group #temporarily add the group to the events for ranking
    return(events)
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