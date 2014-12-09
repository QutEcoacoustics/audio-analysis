

SetupArtificialClustering.old <- function (species.mins = NULL) {
    # returns a list of call - minute pairs by randomly assigning 1 or more call(type)s to each species
    #    
    
    if (is.null(species.mins)) {
        species.mins <- GetTags();
        species.mins <- AddMinuteIdCol(species.mins)
    }  
    
    # create dataframe with counts of each species, ordered by species id
    species.ids <- as.data.frame(table(species.mins$species.id))
    colnames(species.ids) <- c('species.id', 'count')
    species.ids <- species.ids[order(species.ids$species.id), ]
    
    # get rid of extra columns in species.mins
    species.mins <- species.mins[, c('species.id', 'min.id')]
    
    # create a list of "calls" call belongs to species, species has many calls
    # arbitrary number of calls to be 2xnumber of species
    call.ids <- 1:(nrow(species.ids)*2)
    
    # assign a species id to each call. 
    # species appearing in fewer than x mins will only be assigned to 1 call, to reduce the chance of a species having more calls than mins it appears in
    x <- 8
    frequent.species <- species.ids[species.ids$count >= x, ]
    rare.species <- species.ids[species.ids$count < x, ]
    call.ids.for.rare.species <- call.ids[1:nrow(rare.species)]
    call.ids.for.frequent.species <- call.ids[(length(call.ids.for.rare.species) + 1):length(call.ids)]
    
    ok <- FALSE
    num <- 0
    while (!ok && num < 50) {
        calls.of.frequent.species <- data.frame(call.id = call.ids.for.frequent.species, 
                                                species.id = sample(frequent.species$species.id, length(call.ids.for.frequent.species), replace = TRUE))
        # ensure all species have at least 1 call
        # select random indices of the list of calls of frequent species 
        indices <- sample(1:length(call.ids.for.frequent.species), nrow(frequent.species), replace = FALSE)
        # assign each species to those random indices
        calls.of.frequent.species$species.id[indices] <- sample(frequent.species$species.id, nrow(frequent.species), replace = FALSE)
        # check that no species has more calls than mins it appears in
        # freaky random sampling may have done this, but unlikely
        call.count <- as.data.frame(table(as.integer(calls.of.frequent.species$species.id)))
        colnames(call.count) <- c('species.id', 'count')
        call.count <- call.count[order(call.count$species.id), ]
        if (all(call.count$count <= frequent.species$count)) {
            ok <- TRUE
        }
        num <- num + 1
    }
    
    calls.of.rare.species <- data.frame(call.id = call.ids.for.rare.species, 
                                        species.id = as.integer(as.character(rare.species$species.id)))
    
    calls <- rbind(calls.of.rare.species, calls.of.frequent.species)
    
    return(list(calls = calls, species.mins = species.mins, species.ids = species.ids))
    
}
SimulatePerfectClustering2.old <- function (species.mins = NULL, events = NULL) {
    # details:
    # rare.species.threshold: species apprearing in less than this many mins will have only 1 call type
    # max.calls: species above the rare.species.threshold will have a random number of call types between 1 and max calls
    
    
    # determine the average number of events that each species has in each minute
    # i.e. each species-minute pair.
    min.ids <- unique(species.mins$min.id)
    num.events <- rep(NA, nrow(species.mins))
    for (min.id in min.ids) { 
        selection <- which(species.mins$min.id == min.id)
        num.species <- length(selection)
        num.events <- length(which(events$min.id == min.id))
        mean.events.per.species <- num.events / num.species
        if (mean.events.per.species < 1) {
            stop(paste("minute id", min.id, "has more species than events! abort, abort, abort")) 
        }
        num.events[selection] <- mean.events.per.species
    }
    species.mins$num.events <- num.events
    
    
    
    
    cur.call.id <- 1
    species.ids <- unique(species.mins$species.id)
    species.mins$call.id <- rep(NA, nrow(species.mins))
    
    rare.species.threshold <- 8
    # number of calls a species has is randomly selected from this list
    # more likely to get a 1 or 2
    num.calls.pool <- c(1,1,1,2,2,2,3,3,4,5)
    
    # for each species, create a list of call.ids then randomly assign to each minute
    
    for (species.id in species.ids) {
        selection <- species.mins$species.id == species.id # the minutes that this species appears in
        num.mins <- length(which(selection)) # how many mins
        if(num.mins < rare.species.threshold) {
            num.calls <- 1
        } else {
            # this species appears in enough minutes
            # select the number of calls this species has at random
            # from num.calls.pool
            num.calls <- sample(num.calls.pool, 1) 
        }
        
        # create a pool of call ids for this species based on number of calls this species has
        # been randomly assigned
        call.id.pool <- (cur.call.id):(cur.call.id + num.calls - 1)
        species.mins$call.id[selection] <- SampleAtLeastOne(call.id.pool, num.mins) 
        cur.call.id <- cur.call.id + num.calls
    }
    
    # for each call id, randomly assign 1 or more group.ids 
    # a call can belong to multiple clusters, because a call 
    # can have multiple syllables that mgiht be detected individually
    # randomly assign multiple clusters to 
    
    # 1. determine the difference beween the desired number of groups and the number of call types
    # 2. figure out how many groups to give to each call type so that the total number of groups is correct
    # 3. ensure that the number of groups for each minute, given the call-minute pairs,  doesn't exceed the number of events
    # 4. If it does, repeat step 3 until it works
    
    group.ids <- SampleAtLeastOne(num.groups, )
    
    
    
    
    # for each event, assign a 'call' label, based on the calls present in the minute 
    # number of calls should be equal to number of species. Number of species *should* be less than number of events
    # only time this would fail would be situation like:
    # 2 species' simultaneous vocalizations make 1 event and there are no other events. 
    
    minute.ids <- unique(events$min.id)
    
    groups <- rep(NA, nrow(events))
    
    for (min.id in minute.ids) {
        
        selection <- events$min.id == min.id
        num.events <- length(which(selection))
        
        call.id.pool <- species.mins$call.ids[species.mins$min.id == min.id] 
        
        # TODO: some calls have multiple clusters 
        # i.e. a call can result in multiple events and each syllable can belong to multiple clusters
        
    }
    
    
}
SimulatePerfectClustering.old <- function (species.mins = NULL, events = NULL) {
    # given a list of species-minute pairs, will create a list of cluster-minute pairs
    # to simulate perfect clustering
    # if events is supplied, for each event will randomly assign one of the 
    #  clusters from the minute that the event appears in to that event
    #  (ensuring each cluster is represented at least once. 
    #  can not have more clusters than events. 
    #
    # Args:
    #   species.mins: data.frame: contains cols species.id and min.id
    #
    # Value:
    #   data.frame: list of group - minute pairs
    #
    # Details: 
    #   if species.mins is ommited, will get them again from the database
    
    
    res <- SetupArtificialClustering(species.mins)
    calls <- res$calls
    species.ids <- res$species.ids
    speceis.mins <- res$species.mins
    
    # each call has 1 or more events. Each event belongs to 1 cluster. So, each call has 1 or more clusters. each cluster belongs to 1 call
    
    cluster.ids <- 1:(nrow(calls) * 2)
    clusters <- data.frame(cluster.id = cluster.ids, call.id = SampleAtLeastOne(calls$call.id, nrow(calls)))
    
    clusters$call.id[sample(1:nrow(clusters), nrow(calls), replace = FALSE)] = sample(calls$call.id, nrow(calls), replace = FALSE)  # ensure all calls have at least 1 cluster
    
    # so, now we have each species with 1 or more calls (average of 2)
    # and each call has 1 or more cluster groups (average of 2)
    # assume that in any particular minute, only one call from each species is present. i.e. a species won't call with 2 different call types in the same minute
    
    
    
    # for each species-minute pair, assign a call.id
    # then add the relevant events to the cluster.minutes df
    species.mins$call.id <- rep(NA, nrow(species.mins))
    for (s.id in as.numeric(as.character(species.ids$species.id))) {   
        species.mins.rows <- which(species.mins$species.id == s.id) 
        species.call.ids <- calls$call.id[calls$species.id == s.id]
        
        if (length(species.call.ids) == 1) {
            species.mins$call.id[species.mins.rows] <- species.call.ids
        } else {
            #if the species has more than one call, randomly assign the calls to the minutes of that species
            # but make sure to include each call id at least once
            
            
            species.mins$call.id[species.mins.rows] <- SampleAtLeastOne(species.call.ids, length(species.mins.rows))
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
ApplyGroupToEvents.old <- function (num.cluster.groups = 240, events = NA, clustering = NA) {
    # given a df of events and a clustering object,  adds a 'group' column to the events df 
    # using the specified number of cluster groups
    #
    # Args:
    #   num.cluster.groups: int
    #   events: data.frame
    #   clustering: clustering.object
    #
    # Value:
    #   data frame: the events data frame with the group number for each event
    #
    # Details: 
    #   if events or clutering is ommited, these will both be obtained by reading output 
    #
    
    if (!is.data.frame(events)) {
        events <- ReadOutput('events')
        fit <- ReadOutput('clustering')
    } 
    group <- cutree(fit$data, num.cluster.groups)
    events$data$group <- group #temporarily add the group to the events for ranking
    return(events)
}
ApplyRandomGroupToEvents.old <- function (num.cluster.groups = 240, events = NA) {
    # given a df of events and a clustering object,  adds a 'group' column to the events df 
    # using the specified number of cluster groups
    #
    # Args:
    #   num.cluster.groups: int
    #   events: data.frame
    #
    # Value:
    #   data frame: the events data frame with the group number for each event
    #
    # Details: 
    #   if events is ommited it will be obtained by reading output 
    #
    
    if (!is.data.frame(events)) {
        events <- ReadOutput('events')
    } 
    group <- SampleAtLeastOne(1:num.cluster.groups, nrow(events$data))
    events$data$group <- group
    return(events)
}


