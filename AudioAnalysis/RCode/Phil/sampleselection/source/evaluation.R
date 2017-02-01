# This file deals with determining species accumulation curve values for ranked samples and for comparison samples
# It doesn't contain anything for 



source('evaluation.plots.R')

Batch1 <- function () {
    # specific set of rankings to compare
    batch <- c(13,14,15,16,17,18,19)
    # subset.rankings <- c(6,10) # clustering decay = 1, with and without temp.disp
    subset.rankings <- c(4,8) # event count only, with and without temp.disp
    return(EvaluateSamples(versions = batch, subset.rankings = subset.rankings))
}

Batch1 <- function () {
    # specific set of rankings to compare
    res <- EvaluateSamples(versions = batch, subset.rankings = NULL)
    PlotProgressionSummary(res)
}





EvaluateSamples <- function (use.last.accessed = FALSE, versions = NULL, subset.rankings = NULL) {
    # Entry point for evaluation. Reads ranking data from disk and evaluates the ranking against ground truth data
    #
    # Args:
    #   use.last.accessed: boolean; whether to prompt for user input when choosing the ranking data version
    #   versions: ints; which versions of ranking to read (makes user input unecessary). This does a batch of many evaluations.
    #   subset.rankings: ranking output might have multiple rankings in a single file. Subset rankings says which ranking methods to use. 
    #
    # Value: nothing
    #
    # Details:
    #   A set of rankings (i.e. a set of minutes ranked from 1 to num minutes) are read from disk. 
    #   The species accumulation curve (SAC) is calculated for that ranking
    #   Comparison species accumulation curves are also calculated for the same set of minutes, e.g. Random at dawn, optimal
    #   The SACs are plotted against each other.
    #   If multiple sets of rankings are supplied, they are each processed separately. Rankings from the same set can be included on the same plot.
    
    if (is.numeric(versions)) {  
        results <- list()
        for (v in versions) {     
            ranks <- ReadOutput('ranked.samples', use.last.accessed = use.last.accessed, version = v)
            ranks <- ChooseNumClusters(ranks)
            ranks <- ChooseRankingsToComapre(ranks, subset.rankings)
            heading <- GetHeading(ranks)
            results[[as.character(v)]] <- EvaluateSamples2d(ranks, heading = heading)  
        }
        return(results)
    } else {
        ranks <- ReadOutput('ranked.samples', use.last.accessed = use.last.accessed)
        
        # user must choose the a single number-of-clusters to plot
        ranks <- ChooseNumClusters(ranks)
        
        # if subset.rankings has been supplied, create a subset of the ranking methods available
        # also creates consistent column names
        ranks <- ChooseRankingsToComapre(ranks, subset.rankings)
        
        heading <- GetHeading(ranks)
        progressions <- EvaluateSamples2d(ranks, heading = heading)  
        # don't return results because we only use that to get averages across many
    }
    
    
}

ChooseNumClusters <- function (ranks) {
    # if there is more than one number of clusters ranked
    # gets the user to choose which one they want
    # then the array is reduced down to 2 dimensions
    
    d.names <- dimnames(ranks$data)      
    num.clusters.options <- d.names$num.clusters
    default <- ceiling(length(num.clusters.options) / 2)  # default is the middle num clust
    
    # todo: HACK! won't work if there is more than one num.clusters chosen
    # change to GetUserChoice (single)
    num.clusters.choice <- GetMultiUserchoice(num.clusters.options, 'num clusters', default = default)
    # reduce the array down to the chosen num clusters
    new.ranks.data <- ranks$data[,,num.clusters.choice] # returns matrix (2d array)
    new.ranks.data <- as.data.frame(new.ranks.data)
    ranks$data <- new.ranks.data

    return(ranks)
}

ChooseRankingsToComapre <- function (ranks, subset.rankings = NULL) {
    # given a ranking output which may contain many rankings
    # on the same data done with different ranking algorithms 
    # creates a subset
    colnames(ranks$data) <- ConsistentRankNames(colnames(ranks$data))
    if (!is.null(subset.rankings)) {
        subset.rankings <- ConsistentRankNames(subset.rankings)
        ranks$data <- ranks$data[,colnames(ranks$data) %in% subset.rankings, drop=FALSE]
        #ranks$data <- ranks$data[,subset.rankings]    
    }
    return(ranks)  
}

EvaluateSamplesEventCountOnly <- function (use.last.accessed = FALSE, versions = NULL) {
    # does evaluation on sampling by event count.
    # Args: 
    #   use.last.accessed: if TRUE, will use the last version of ranked samples used (unless versions is set)
    #   version: numeric vector; if supplied, will do evaulation on each of the specified ranked samples versions

    if (is.numeric(versions)) {        
        results <- list()
        for (v in versions) {
            ranks <- ReadOutput('ranked.samples.ec', use.last.accessed = use.last.accessed, version = v)
            heading <- GetHeading(ranks)
            # plots results and returns SACs
            results[[as.character(v)]] <- EvaluateSamples2d(ranks, heading = heading) 
        }
        
        return(results)
        
        
        
    } else {
        ranks <- ReadOutput('ranked.samples.ec', use.last.accessed = use.last.accessed)
        heading <- GetHeading(ranks)
        EvaluateSamples2d(ranks, heading = heading) 
        
    }


}

GetHeading <- function (ranks) {
    target.min.ids <- GetMeta('target.min.ids', ranks$indirect.dependencies$target.min.ids)
    target <- target.min.ids$params$target
    target <- gsub("[^A-Za-z0-9 -]", '', target)
    heading = target;
    return(heading)
}

EvaluateSamples2d <- function (ranks, add.dawn = 0, heading = '') {
    # given a list of minutes, with their order ranked in a number of ways. 
    # simulates a species richness survey, noting the 
    # total species found after each minute, and the total
    # number of species found after each minute
    #
    # Args
    #   ranks: data.frame; colnames are the names of the ranking method
    #                      values are min.ids in ascending order or ranke (best first)
    #
    #   add.dawn: boolean; How much random at dawn to add to the beginning of the ranked samples. 
    #                      (see details)
    #   
    # Details: 
    #   add.dawn prepends some random minutes at dawn to the ranking. This 

    
    # these min ids have been ranked
    # used to read these separately, but we should just used the dependency target.min.ids used
    # for ranking, right?
    # ranked.min.ids <- ReadOutput('target.min.ids', purpose = "evaluating ranked samples")
    # mins.for.comparison <- ranked.min.ids$data

    # double check to make sure that everything is using the same minutes
    # check that all versions of ranks have the same minute ids (if not the ranking code is buggy)
    if (ncol(ranks$data) > 1) {
        for (i in 2:ncol(ranks$data)) {
            if (!setequal(ranks$data[,i], ranks$data[,1])) {
                stop("rankings have different minute ids from each other")
            }
        }     
    }
    mins.for.comparison <- ranks$data[,1]
    mins.for.comparison <- ExpandMinId(mins.for.comparison)

    species.in.each.sample <- GetSpeciesInEachSample(mins.for.comparison$min.id)
    
    # holds a lot of info about the species found
    ranked.progressions <- list(mins.for.comparison$min.id)

    # holds only the running total of species
    ranked.count.progressions <- list()
    methods <- colnames(ranks$data)

    species.in.each.sample.df <- NULL
    
    i <- 1
    while (i <= length(methods)) {
        method <- methods[i]
        ordered.min.ids <- ranks$data[,methods[i]]
        
        if (add.dawn) {
            iterations <- 100
            amount.of.dawn <- 50
            species.count.progressions <- matrix(NA, nrow = iterations, ncol = length(ordered.min.ids) + amount.of.dawn)
            for (j in 1:iterations) {
                dawn.mins <- GetRandomDawnMins(amount.of.dawn)
                random.plus.ordered.min.ids <- c(dawn.mins$min.id, ordered.min.ids)
                prog <- GetProgression(species.in.each.sample, random.plus.ordered.min.ids)
                species.count.progressions[j, ] <- prog$count  
            }
            ranked.count.progressions[[method]] <- list(mean = apply(species.count.progressions, 2, mean), sd = apply(species.count.progressions, 2, sd))
        } else {
            ranked.progressions[[method]] <- GetProgression(species.in.each.sample, ordered.min.ids)
            # sort the minutes in each species by ranking by the ranking
            
            if (is.data.frame(species.in.each.sample.df)) {
                species.ranked <- species.in.each.sample.df[match(ordered.min.ids, as.numeric(names(species.in.each.sample))),]
                # add a column to show new species
                species.ranked$new.species.ids <- as.character(ranked.progressions[[method]]$new.species)
                # add the table
                ranked.progressions[[method]]$table <- species.ranked 
            }
        

            #WriteRichnessResults(ordered.min.ids, ranked.progressions[[method]], method, species.in.each.sample)
            ranked.count.progressions[[method]] <- ranked.progressions[[method]]$count
        }
        
        i <- i + 1
    }
    
    # save/retrieve the optimal samples by giving the target.min.ids as dependencies
    # which come from the target.min.ids of the ranks
    optimal <- GetOptimalSamples(species.in.each.min = species.in.each.sample, 
                                 use.saved = TRUE, 
                                 target.min.ids.version = ranks$indirect.dependencies$target.min.ids)
    
    if (IsDawn(mins.for.comparison$min)) {
        # if any of the mins are in dawn, do the random at dawn for comparison agains our results
        random.at.dawn <- RandomSamples(speciesmins = speciesmins, species.in.each.sample, mins.for.comparison$min.id, dawn.first = TRUE, num.repetitions = 100)   
    } else {
        random.at.dawn <- NA
    }
    
    
    random.all <- RandomSamples(speciesmins = speciesmins, species.in.each.sample, mins.for.comparison$min.id, dawn.first = FALSE, num.repetitions = 100)
    #from.indices <- IndicesProgression(species.in.each.sample, min.ids)


    comparison.progressions <- list(
        optimal = optimal$found.species.count.progression,
        random.at.dawn = random.at.dawn,
        random.all = random.all
        )
 
    #return(c(ranked.count.progressions, comparison.progressions))
    rankings.string <- paste(names(ranked.count.progressions), collapse = '_')
    fn <- paste(heading,rankings.string,'png', sep = '.')
    heading <- paste("Species accumulation Curve:", heading)

    progressions <- c(ranked.count.progressions, comparison.progressions)
    PlotProgressions(progressions, heading = heading, fn = fn)

    return(progressions)
    
}

GetSpeciesInEachSample <- function (min.ids = NULL) {
    # checks if we have a saved species.in.each.sample for the target mins already
    # and if so, uses it, if not looks in the database etc
    # 
    # Args:
    #   min.ids: data.frame; the minutes that we want to look in
    
    params = list(study = GetStudyDescription())
    # TODO: remove "use last accessed" after building a check for matching params and dependencies into ReadOutput
    species.in.each.sample <- ReadOutput('species.in.each.min', params = params, false.if.missing = TRUE, use.last.accessed = FALSE)
    if (!is.list(species.in.each.sample)) {
        speciesmins <- GetTags(target.only = FALSE, study.only = TRUE);  # get tags from whole study
        speciesmins <- AddMinuteIdCol(speciesmins)
        species.in.each.sample <- ListSpeciesInEachMinute(speciesmins)
        WriteOutput(species.in.each.sample, 'species.in.each.min', params = params)
    } else {
        species.in.each.sample <- species.in.each.sample$data
    }
    
    # todo: ability to add dawn mins
    if (is.null(min.ids)) {
        return(species.in.each.sample)
    } else {
        # we only want the ones with the specified min ids
        list.indexes <- match(min.ids, names(species.in.each.sample))
        return(species.in.each.sample[list.indexes])    
    }

}

ConsistentRankNames <- function (names) {
    # some types of ranking create a table with names like X4, X5 etc, and some with names like 4, 5 etc
    if (!is.na(as.integer(names))) {
        # if coercing to integer works
        names <- paste0("X", names)
    }
    return(names)
    
}

EvaluateSamples3d <- function (ranks) {
    # given a list of minutes
    # simulates a species richness survey several times with different number of cluster groups
    # then outputs the species count progression graph as a 3d surface. 
    # x axis is the minute number of the survey
    # z axis the number of cluster groups used for the ranking algorithms
    # vertical axis is the number of species found in x minutes
    #
    # Args:
    #   ranks: a 3d array: the minute rankings in order of minute id for each ranking algorithm, 
    #                      each run several times based on events being assigned to several different cluster groups



    min.ids <- ReadOutput('target.min.ids', level = 0)
    speciesmins <- GetTags();  # get tags within outer target 
    species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, min.ids)
    
    ranked.progressions <- ranks
    
    dn <- dimnames(ranks)
    methods <- dn$methods
    cluster.nums <- dn$num.clusters
    
    for (m in 1:length(ranks[,1,1])) {
        # for each ranking method
        for (cn in 1:length(ranks[1,,1])) {
            # for each num cluster groups
            
            ordered.min.ids <- min.ids$min.id[order(ranks[m,cn,])]
            progression <- GetProgression(species.in.each.sample, ordered.min.ids)
            #todo: output as a csv
            ranked.progressions[m,cn, ] <- progression$count
            
        }
    }
    
   # optimal <- OptimalSamples(speciesmins, species.in.each.min = species.in.each.sample)
   # random.at.dawn <- RandomSamples(speciesmins = speciesmins, species.in.each.sample, samples$min.id)
    
   for (i in 1:length(ranked.progressions[,1,1])) {
       PlotProgressions3d(ranked.progressions[i,,])   
   }

    
}

ValidateMins <- function (mins = NA) {
    # mins can either be a vector of minute ids, in which case we expand it to 
    # include [site,date,min], a dataframe of [site,date,min], or nothing in which
    # case we use all the minutes from the study   
    if (class(mins) %in% c('integer', 'numeric', 'data.frame')) {
        mins <- ExpandMinId(mins)
    } else if (class(mins) != 'data.frame') {
        mins <- GetMinuteList()
    }  
    #validate if mins has correct cols
    must.have <- c('site', 'date', 'min')
    missing <- setdiff(must.have, colnames(mins))
    if (length(missing) > 0) {
        stop(paste('missing columns from supplied min list: ', missing, collapse = ','))
    }
    return(mins)
}

GetOptimalSamples <- function (mins = NA, species.in.each.min = NA, use.saved = TRUE, target.min.ids.version = NULL) {
    # save/retrieve the optimal samples by giving the target.min.ids as dependencies
    # which come from the target.min.ids of the ranks
    
    dependencies <- list(target.min.ids = target.min.ids.version)
    if (use.saved) {
        optimal <- ReadOutput('optimal.samples', dependencies = dependencies, false.if.missing = TRUE)
    } else {
        optimal <- NULL
    }
    if (!is.list(optimal)) {
        optimal <- OptimalSamples(species.in.each.min = species.in.each.min)
        if (use.saved) {
            WriteOutput(optimal, 'optimal.samples', dependencies = dependencies)
        }
    } else {
        optimal <- optimal$data
    }
    
    return(optimal)
    
}


CalculateOptimalSamples <- function (mins = NA, species.in.each.min = NA) {
    # determines the best possible selection of [numsamples] minute samples
    # to find the most species
    # 
    # Args:
    #   speciesmins: dataframe; the list of species in each minute. If not included
    #                            will retreive from database
    #   mins: dataframe; the list of minutes we can select from. If not
    #                    included then all mins in the 'species in each min' arg
    #   species.in.each.min: list
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
        species.in.each.min <- ListSpeciesInEachMinute(speciesmins, mins = mins) 
    }
    
    # maximum number of samples is the number of species, or number of minutes (whichever is smaller)
    # but finding the total number of species is slow, so just go for number of minutes
    # all.species <- unique(unlist(species.in.each.min)) # this is really slow
    # total.num.species <- length(all.species)
    # initial.length <- min(c(total.num.species, length(species.in.each.min)))
    initial.length <-  length(species.in.each.min)
    selected.samples <- rep(NA, initial.length)
    found.species.count.progression <- rep(NA, initial.length)
    found.species.progression <- vector("list", initial.length)
    all.found.species <- numeric()


    if (class(mins) == 'logical') {
        min.ids <- names(species.in.each.min)
    } else if (class(mins) == 'data.frame') {
        min.ids <- as.character(mins$min.id)
    } else {
        min.ids <- as.character(mins)
    }
    
    
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
       
        for (m in 1:length(min.ids)) {
               m.id <- min.ids[m]
               sp <- species.in.each.min[[m.id]]
               species.in.each.min[[m.id]] <- sp[! sp %in% last.found.species]
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

IndicesProgression <- function (species.in.each.sample, which.mins) {
    # Gets the species progression based on ranking by minute-indices.
    # to serve as a comparison
    
    ranked.mins <- RankMinutesFromIndices()
    ranked.mins <- ranked.mins[ ranked.mins$min.id %in% which.mins$min.id, ] 
    ordered.min.ids <- ranked.mins$min.id[order(ranked.mins$rank)]
    progression <- GetProgression(species.in.each.sample, ordered.min.ids)
    return(progression)
}

ListSpeciesInEachMinute <- function (speciesmins, mins = NA) {
    # given a list of minute ids, and a list of all tags
    # creates a list of species for each minute in mins
    #
    # Args:
    #   speciesmins: list of all the tags, with their site, date and min-of-the-day
    #   mins: vector of minute ids or data frame of minutes, if ommited will use all minutes in speciesmins
    #  
    # Value: list. names of the list are the min.id. values of each element is a vector of species ids
    #
    # Details:
    #  if min.ids is supplied, this will be used for the list of mins
    #  if min.ids is not supplied, mins must be supplied. 
    
    Report(5, 'counting species in each minute')
    
    if (!("min.id" %in% colnames(speciesmins))) {
        AddMinIdCol(speciesmins)
    }
    
    if (is.null(mins)) {
        mins <- unique(as.integer(species.mins$min.id))
    }
    mins <- ValidateMins(mins)
    species.in.each.min <- vector("list")
    
    for (i in 1:nrow(mins)) {
        sp.list <- speciesmins$species.id[speciesmins$min.id == mins$min.id[i]]
        species.in.each.min[[as.character(mins$min.id[i])]] <- sp.list
    }
    
    return(species.in.each.min)
}

GetProgression <- function (species.in.each.sample, ordered.min.list) {
    # returns the count of new species given a list of species vectors
    # 
    # Args: 
    #   species.in.each.sample: list
    #   ordered.min.list: vector; the minute ids in the order they should appear in the progression
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
    
    
    
    
    found.species.count.progression <- rep(NA, length(ordered.min.list))
    found.species.progression <- vector("list", length(ordered.min.list))
    new.species.count.progression <- rep(0, length(ordered.min.list))
    new.species.progression <- vector("list", length(ordered.min.list))
    all.found.species <- numeric()
    
    
    
    if (is.numeric(ordered.min.list)) {
        ordered.min.list <- as.character(ordered.min.list)
    }
    
    
    for (i in 1:length(ordered.min.list)) {
        min.id <- ordered.min.list[i]
        new.species <- setdiff(species.in.each.sample[[min.id]], all.found.species) 
        all.found.species <- c(all.found.species, new.species)
        found.species.count.progression[i] <- length(all.found.species)
        new.species.count.progression[i] <- length(new.species)
        found.species.progression[[i]] <- all.found.species
        new.species.progression[[i]] <- new.species
    }
    
    # this might not be necessary now
    found.species.count.progression <- found.species.count.progression[! is.na(found.species.count.progression)]
    
    return(list(count = found.species.count.progression, 
                new.count = new.species.count.progression, 
                species = found.species.progression, 
                new.species = new.species.progression))
    
    
}

WriteRichnessResults <- function (min.ids, found.species.progression, output.fn, species.in.each.sample) {
    # given a list of samples with rankings, a list of species in each sample, and a species progression
    # writes is all to a csv
    # samples and species.in.each.sample are in order of minute id
    # found.species.progression is already in the order of the ranking
    
    #!! TODO: broken. Must update this to work with new output system
    
    # sort the samples by rank
    
    mins <- ExpandMinId(min.ids)
    sorted.species.in.each.sample <- species.in.each.sample[as.character(min.ids)]
    
    #writes the results of a sample ranking to a csv
    # create dataframe of progression for csv output
    num.species = sapply(sorted.species.in.each.sample, length)
    num.new.species = found.species.progression$new.count
    species = sapply(sorted.species.in.each.sample,  paste, collapse = ', ') 
    new.species = sapply(found.species.progression$new.species, paste, collapse = ', ')
    
    
    output <- data.frame(num.species = num.species, 
                         num.new.species = num.new.species,
                         species = species,
                         new.species = new.species)
    
    output <- cbind(mins, output)
    
    WriteOutput(output, output.fn)
    
    
}



ClustersInOptimal <- function () {
    # given a clustering result, gets the optimal progression based on labelled data
    # and finds what clusters are in each of these optimal minutes
    clustered.events <- ReadOutput('clustered.events')
    
    # should automatically get the relevant target.min.ids based on the ancestor of clustered events
    mins <- ReadOutput('target.min.ids')
    
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

