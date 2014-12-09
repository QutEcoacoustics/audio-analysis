

SimulateClustering <- function (events = NULL, species.mins = NULL, num.clusters = 240) {
    # details:
    # rare.species.threshold: species apprearing in less than this many mins will have only 1 call type
    # max.calls: species above the rare.species.threshold will have a random number of call types between 1 and max calls 
    
    # todo: FIRST assign each event to a species, randomly, so that there are different number of events per species within a particular minute
    # maybe assign 10% of events to be other stuff
    
    # first ensure that there are at least 1 event per species in each minute
    # this might not be true if event detection was not sensitive, and a very faint call was annotated
    
    # alternative format for params is to supply both the 
    if (class(events) == 'list') {
        species.mins <- events$species.mins
        events <- events$events
        return.as.list <- TRUE
    } else {
        return.as.list <- FALSE
    }
    
    
    
    events <- AddFakeEvents(species.mins, events)
    
    events <- AssignEventsToSpecies(species.mins, events)
    
    # remove events with no species 
    # maybe we can do something better than this
    
    events <- events[!is.na(events$species.id), ]
    
    # determine the average number of events that each species has in each minute
    # i.e. each species-minute pair.
    min.ids <- unique(species.mins$min.id)
    
    num.events <- rep(NA, nrow(species.mins))
    
    # for each species, determine the total number of events
    species.ids <- unique(species.mins$species.id)
    species.event.counts <- sapply(species.ids, function (species.id, events) {
        return(sum(events$species.id == species.id, na.rm = TRUE))
    }, events)
    
    species <- data.frame(species.id = species.ids, num.events = species.event.counts)
    species <- SetNumClustersPerSpecies(species, num.clusters)
    
    # create species-cluster pairs
    species.clusters <- GetSpeciesClusterPairs(species)
    
    # now we have events matched to species and clusters matched to species
    # so we will now match events to clusters
    
    # for each species, find all the species' events, and randomly assign on of the species' clusters
    # ensuring that all the species' clusters are used
    
    events$group <- NA
    
    for (sp in 1:nrow(species)) {
        event.selection <- events$species.id == species$species.id[sp]
        clusters <- species.clusters$group[species.clusters$species.id == species$species.id[sp]]
        events$group[event.selection] <- SampleAtLeastOne(clusters, sum(event.selection))
    }
    
    if (return.as.list) {
        return(list(events = events, species.mins = species.mins))
    } else {
        return(events) 
    }
    

    
}


AddRandom <- function (events, amount.random) {
    # given a df of events, each with a group
    # assigns a random group to a subset of them
    groups <- unique(events$group)
    new.groups <- c()
    num.random <- round(nrow(events) * amount.random)
    
    # there is a chance that not all groups will be represented
    # as long as num.random is a lot bigger than num groups, this is fairly small
    max <- 20
    for (i in 1:max) {
        rand.event.rows <- sample(nrow(events), num.random, replace = FALSE)
        rand.groups <- sample(groups, num.random, replace = TRUE)      
        events$group[rand.event.rows] <- rand.groups
        new.groups <- unique(events$group)
        if (length(setdiff(groups, new.groups)) == 0) {
            break()
        }
    }
    
    if (length(setdiff(groups, new.groups)) > 0) {
        # the number of groups is too high, so it means that just picking out randomly with replacement
        # is likely to miss some grounds. So, instead we just shuffle the given num.random so that all 
        # the groups are maintained
        print('warning: add random shuffled instead of picking with replacement')
        rand.event.rows <- sample(nrow(events), num.random, replace = FALSE)
        original.groups <- 
        rand.groups <- sample(groups, num.random, replace = TRUE)     
        
    }


    return(events)
}


AssignEventsToSpecies <- function (species.mins, events) {
    # for each event, randomly assigns it one of the species that appear in the minute
    
    min.ids <- unique(species.mins$min.id)
    
    events$species.id <- NA
    
    for (m.id in min.ids) {   
        species.ids <- species.mins$species.id[species.mins$min.id == m.id]
        
        events.selection <- events$min.id == m.id
        num.events <- sum(events.selection)
        prob <- sample(seq(from = 0.3, to = 0.7, by = 0.1), length(species.ids), replace = TRUE)      
        events$species.id[events.selection] <- SampleAtLeastOne(species.ids, num.events, prob)   
    }
    
    # at the end, there will still be some events that are not associated with any species
    # if there were no species labeled in that event's minute
    
    return(events)
    
    
    
}

AddFakeEvents <- function (species.mins, events) {
    # for each minute that has species labels, 
    # ensure that there are at least as many events as species labels
    
    min.ids <- unique(species.mins$min.id)
    min.list <- GetMinuteList()
    for (min.id in min.ids) { 
        num.species <- sum(species.mins$min.id == min.id)
        num.events <- sum(events$min.id == min.id)
        if (num.species > num.events) { 
            # duplicate a row and fake enough events so that each species has enough events
            # find out the site, date, min to fake
            minute <- min.list[ min.list$min.id == min.id ,]
            num.missing <- num.species - num.events
            fake <- events[1:num.missing,]
            fake$site <- minute$site  
            fake$date <- minute$date    
            fake$min <- minute$min
            fake$event.id <- NA
            fake$filename <- NA    
            fake$start.sec  <- NA    
            fake$start.sec.in.file  <- NA
            fake$duration  <- NA
            fake$bottom.f  <- NA	
            fake$top.f  <- NA	
            fake$min.id  <- min.id
            events <- rbind(events, fake)
            print(paste("minute id", min.id, "has more species than events! creating some fake events"))
        }
    }
    # add an event id to each of the new events
    num.fake <- sum(is.na(events$event.id))
    fake.event.id.start <- max(events$event.id, na.rm = TRUE) + 1
    fake.event.id.end <- fake.event.id.start + num.fake - 1
    events$event.id[is.na(events$event.id)] <- fake.event.id.start : fake.event.id.end
    
    return(events)
    
}

SetNumClustersPerSpecies <- function (species, num.clusters) {
    
    attempts <- 3
    for (i in 1:attempts) {
        res <- SetNumClustersPerSpeciesInner(species, num.clusters)
        if (is.data.frame(res)) {
            break()
        } else {
            print("set number clusters per species failed. Too many clusters per species and not enough events")
        }
    }
    
    if (!is.data.frame(res)) {
        stop("unable to Set number of clusters per species")
    }
    
    return(res)
    
    
    
}

SetNumClustersPerSpeciesInner <- function (species, num.clusters) {
    # given a list of species with the number of events each species has
    # and the total number of clusters
    # assigns a number of clusters per species
    # number is random, between 1 and twice the average number (which is num clusters / num species)
    # num.clusters must be > num species, and probably should be several times bigger
    
    av.num.per.species <- num.clusters / nrow(species)
    
    pool <- 1:(round(2*av.num.per.species))
    sd <- av.num.per.species/2
    # probablity for number of clusters is set to be almost gausian around the average number
    # with a slight skew towards fewer cluster 
    prob <- GaussianFunction(1, pool, max(pool-1)/2, sd)
    species$num.clusters <- sample(pool, nrow(species), TRUE, prob)
    
    # very rare species might end up with more clusters than events
    # so in these cases, set the num clusters to the num events
    rare.species <- species$num.events <= species$num.clusters
    species$num.clusters[rare.species] <- species$num.events[rare.species]
    
    # correct it so that the total number of clusters is correct
    # negative value means that we have assigned too many clusters
    deficit <- num.clusters - sum(species$num.clusters)
    
    attempts <- 1
    max.attempts <- 10
    while(deficit != 0 && attempts < max.attempts) {

        
        if (deficit > 0) {
            add <- 1
        } else {
            add <- -1      
        }
        
        # change the number of clusters of some species by 1, 
        # but only for species that have plenty of events, and are not on the max 
        
        # only change species where the new number will be less than the number of events
        condition.1 <- species$num.clusters + add <= species$num.events
        # and only change species where the new number will be less than or equal to the maximum allowed
        # condition.2 <- species$num.clusters + add < max(pool)
        # allow more than max
        to.change <- which(condition.1)
        if (length(to.change) > abs(deficit)) {
            # if this it the case, the number to change will be equal to the deficit, 
            #so we will reduce the deficit to zero on this run
            # if this is not the case, we can't change enough to remove the deficit on this iteration
            to.change <- sample(to.change, abs(deficit)) 
        }
        
     
        species$num.clusters[to.change] <- species$num.clusters[to.change] + add
        deficit <- num.clusters - sum(species$num.clusters)
        attempts <- attempts + 1
        
        
    }
    
    return(species)   
    
}

GetSpeciesClusterPairs <- function (species) {
    # given a df which has the species id and the number of clusters for that species
    # creates a new df which has the each species id repeated for as many clusters as it has, matched with a cluster id
    # there is probably a much simpler way to do this in R
    species.ids <- c()
    for (i in 1:nrow(species)) {
        species.ids <- c(species.ids, rep(species$species.id[i], species$num.clusters[i])) 
    }
    species.clusters <- data.frame(species.id = species.ids, group = 1:length(species.ids))
    return(species.clusters)
}

OutputForVis <- function (fn, events, species.mins, data = NULL,  path = "/Users/n8933464/Google Drive/0qut/papers_by_me/cluster_quality_evaluation/b-cubed/data/", as.json = TRUE, as.txt = FALSE, max.rows = NULL) {
    # outputs the events/groups/mins in the form that the javascript visualisation can read
    #
    # ARGS
    #   fn: string; file name to output to
    #   events: data.frame; list of events with group
    #   species.mins: data.frame; list of species-minute pairs
    #   data: list; a list containing 2 data.frames: events and species.mins. 
    #               This is optional, but if supplied will be used instead of the individual events/species.mins params
    #   path: string; the directory to save the file to
    #   as.json: boolean. There are 2 types of input: json or plain text. Json format specifies minute and event IDs, 
    #                      but with text these are ommited and generated by the javascript as ints in order
    
    if (class(data) == "list") {
        events <- data$events
        species.mins <- data$species.mins
    } else if (class(events) == "list") {
        species.mins <- events$species.mins    
        events <- events$events
    }
    
    if (is.numeric(max.rows) && max.rows < nrow(events)) {
        events <- events[1:max.rows, ]
    } 
    
    
    min.ids <- unique(events$min.id)
    output.txt <- rep(NA, length(min.ids))
    output.json <- vector("list", length(min.ids))
    for (i in 1:length(min.ids)) {
        labels <- species.mins$species.id[species.mins$min.id == min.ids[i]]
        selected.events <- events[events$min.id == min.ids[i], ]
        
        # for plain text
        labels.txt <- paste(labels, collapse = ",")
        event.groups.txt <- paste(selected.events$group, collapse = ",")
        output.txt[i] <- paste(labels.txt, event.groups.txt, sep="|")
        
        
        #for json
        dps <- vector("list", nrow(selected.events))
        for (dp in 1:nrow(selected.events)) {
            dps[[dp]] <- list(id = selected.events$event.id[dp], g = selected.events$group[dp])
        }
        
        output.json[[i]] <- list(
            id = min.ids[i],
            labels = labels,
            dps = dps
            )     
    }
    if (as.json) {
        require('rjson')
        output.json <- toJSON(output.json)
        fileConn<-file(paste0(path, fn, ".json"))
        writeLines(output.json, fileConn)
        close(fileConn)  
    }
    if (as.txt) {
        fileConn<-file(paste0(path, fn, ".txt"))
        writeLines(output.txt, fileConn)
        close(fileConn)      
    }

    
    
}



