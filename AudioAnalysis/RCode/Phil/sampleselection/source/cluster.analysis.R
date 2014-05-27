

ClusterQuality <- function () {
    events <- ApplyGroupToEvents() 
    cluster.mins <- events$data[c('min.id', 'group')]
    unique.cluster.mins <- unique(cluster.mins)
    speciesmins <- GetTags();
    
    
    
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