GetOptimalSamples <- function (species.in.each.min = NA, use.saved = TRUE, target.min.ids.version = NULL) {
    # save/retrieve the optimal samples by giving the target.min.ids as dependencies
    # which come from the target.min.ids of the ranks
    #
    # Args:
    #   species.in.each.min: list. Species in each minute that we are calculating the optimal sampling for
    #   use.saved: boolean. Whether to attempt to read a saved version of optimal sampling
    #   target.min.ids.version: int. The version of target.min.ids that should be used when reading from saved
    
   
    if (use.saved & !is.null(target.min.ids.version)) {
        dependencies <- list(target.min.ids = target.min.ids.version)
        optimal <- datatrack::ReadDataobject('optimal.samples', dependencies = dependencies, false.if.missing = TRUE)
    } else {
        optimal <- NULL
    }
    if (!is.list(optimal)) {
        optimal <- CalculateOptimalSamples(species.in.each.min = species.in.each.min)
        if (use.saved & !is.null(target.min.ids.version)) {
            datatrack::WriteDataobject(optimal, 'optimal.samples', dependencies = dependencies)
        }
    } else {
        optimal <- optimal$data
    }
    
    return(optimal)
    
}

CalculateOptimalSamples <- function (species.in.each.min = NA, mins = NULL, sample.duration = 1) {
    # determines the best possible selection of [numsamples] minute samples
    # to find the most species
    # 
    # Args:
    #   species.in.each.min: list
    #   mins: dataframe; the list of minutes we can select from. If not
    #                    included then all mins in the 'species in each min' arg
    #   sample.duration: int. how many minutes long should each sample be.
    #
    # Value:
    #   list: containing
    #       data.frame; A list of minutes. Cols: site, date, min
    #       the progression of species found in each of those minutes
    #       the progression of total species found count after each minute
    #
    # Details:
    #   either mins or species.in.each.min must be supplied. If species.in.each.min is not supplied
    #   then the species.in.each.min will be calculated using mins
    
    Report(3, "Calculating optimum sampling")
    
    if (class(species.in.each.min) != 'list') {
        
        if (class(mins) != 'data.frame') {
            stop("You must supply either species.in.each.min or mins") 
        }
        
        # todo: replace this with GetSpeciesInEachMinute
        speciesmins <- GetTags(target.only = FALSE, study.only = TRUE);  # get tags from whole study
        speciesmins <- AddMinuteIdCol(speciesmins)
        species.in.each.min <- ListSpeciesInEachMinute(speciesmins, mins = mins) 
    }
    
    
    
    # make a new list of integer vectors for each sample, which may be longer than 1 min according to the parameter
    # copy the subset of species.in.each.min to a new list, which only has the 1st minute of each sample
    # go through all the minutes 1 by 1 and merge the species id vector in that min to the vector of species ids
    # in the list item for the first minute of the sample. 
    species.in.each.sample <- species.in.each.min[seq.int(1, length(species.in.each.min), sample.duration)]
    map <- rep(1:length(species.in.each.sample), each=sample.duration)
    for (i in 1:length(species.in.each.min)) {
        species.in.each.sample[[map[i]]] <- union(species.in.each.sample[[map[i]]], species.in.each.min[[i]])
    }
    
    # maximum number of samples is the number of species, or number of minutes (whichever is smaller)
    # but finding the total number of species is slow, so just go for number of minutes
    # all.species <- unique(unlist(species.in.each.min)) # this is really slow
    # total.num.species <- length(all.species)
    # initial.length <- min(c(total.num.species, length(species.in.each.min)))
    initial.length <-  length(species.in.each.sample)
    
    selected.samples <- rep(NA, initial.length)
    found.species.count.progression <- rep(NA, initial.length)
    found.species.progression <- vector("list", initial.length)
    all.found.species <- numeric()
    
    #min id of the 1st minute in each sample
    min.ids <- names(species.in.each.sample)

    # this could probably be done faster with a sparsematrix, whatever
    for(i in 1:initial.length) {
        
        # find minute with most species
        max.sp.count <- 0
        max.sp.m.id.index <- (-1)
        for (m in 1:length(min.ids)) {
            m.id <- min.ids[m]
            if (length(species.in.each.min[[m.id]]) > max.sp.count) {
                max.sp.count <- length(species.in.each.min[[m.id]])
                max.sp.m.id.index <- m
            } 
        }
        
        if (max.sp.count == 0) {
            # all species have been included in the selected mins 
            # (or there were no species)
            break()
        }
        
        max.sp.m.id <- min.ids[max.sp.m.id.index]
        
        
        #record that minute
        selected.samples[i] <- max.sp.m.id
        last.found.species <- species.in.each.min[[max.sp.m.id]]
        all.found.species <- union(all.found.species, last.found.species)
        found.species.progression[[i]] <- all.found.species
        found.species.count.progression[i] <- length(all.found.species)
        Report(5, length(all.found.species), " ")
        #remove the already found species from the list
        
        # go through the remaining samples and remove the found species so they don't count towards the next count
        for (m in 1:length(min.ids)) {
            m.id <- min.ids[m]
            sp <- species.in.each.sample[[m.id]]
            species.in.each.sample[[m.id]] <- sp[! sp %in% last.found.species]
        }
        
    }
    
    # initial length was the number of minutes, and optimal should be much less than this
    # so, it will have extra NAs after
    selected.samples <- selected.samples[! is.na(selected.samples)]
    found.species.count.progression <- found.species.count.progression[! is.na(found.species.count.progression)]
    
    return(list(
        found.species.progression = found.species.progression,
        found.species.count.progression = found.species.count.progression,
        selected.mins = selected.samples
    ))
    
}


ClustersInOptimal <- function () {
    # given a clustering result, gets the optimal progression based on labelled data
    # and finds what clusters are in each of these optimal minutes
    
    clustered.events <- datatrack::ReadDataobject('clustered.events')
    
    # should automatically get the relevant target.min.ids based on the ancestor of clustered events
    mins <- datatrack::ReadDataobject('target.min.ids')
    
    optimal <- GetOptimalSamples(mins = mins$data, use.saved = TRUE, target.min.ids.version = mins$version)
    
    # sanity check: make sure all the optimal minute ids are in the set of minutes we are checking
    if (!all(optimal$selected.mins %in% clustered.events$data$min.id)) {
        stop('optimal minutes contain an id that is not supposed to be there')
    }
    
    # create template with templator:
    # 1 row per minute (in order of greedy search) 
    # 1 column per second. 
    # for each second: generate spectrogram with cluster label
    
    
    
    
    
}