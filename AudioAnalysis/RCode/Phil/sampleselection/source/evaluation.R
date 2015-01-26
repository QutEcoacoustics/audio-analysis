EvaluateSamples <- function () {
    
    ranks <- ReadOutput('ranked.samples')
    d.names <- dimnames(ranks$data)
    num.clusters.options <- d.names$num.clusters
    default <- ceiling(length(num.clusters.options) / 2)  # default is the middle num clust
    num.clusters.choices <- GetMultiUserchoice(num.clusters.options, 'num clusters', default = default)
    
    ranks$data <- ranks$data[,num.clusters.choices,]
    
    if (length(num.clusters.choices) > 1) {
        # more than 2 dimensions to graph, so make 3d plot
        EvaluateSamples3d(ranks$data)
    } else {
        # convert to data frame
        
        # TODO!!!! convert to data frame
        
        EvaluateSamples2d.2(ranks$data)  
    }

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
            results[[as.character(v)]] <- EvaluateSamples2d.2(ranks, heading = heading) 
        }
        
        return(results)
        
        
        
    } else {
        ranks <- ReadOutput('ranked.samples.ec', use.last.accessed = use.last.accessed)
        heading <- GetHeading(ranks)
        EvaluateSamples2d.2(ranks, heading = heading) 
        
    }


}


Summary <- function (days) {
    # given a list of evaluation results, including random at dawn and some ranking methods 
    # calculates the average improvement over random at dawn
    
    
    # each of the ranking methods
    ranking.methods <- names(days[[1]])
    
    # each of the non-comparison ranking methods
    to.score <- ranking.methods[!ranking.methods %in% c('optimal', 'random.at.dawn', 'random.all')]
    
    results <- list()
    
    max.val <- -100
    min.val <- 100
    
    for (cur.to.score in to.score) {
        
        # each col is a day
        # each row is a min
        m <- matrix(NA, nrow = length(days[[1]][['random.at.dawn']]$mean), ncol = length(days))      
        
        for (d in 1:length(days)) {   
            r.a.d <- days[[d]][['random.at.dawn']]$mean
            ranked <- days[[d]][[cur.to.score]]
            percent.improvement <- ((ranked / r.a.d) - 1) * 100
            m[,d] <-  percent.improvement
        }
        
        
            
        res.mean <- apply(m, 1, mean)
        if (min(res.mean) < min.val) {
            min.val <- min(res.mean)
        }
        if (max(res.mean) > max.val) {
            max.val <- max(res.mean)
        }
        
        
        results[[cur.to.score]] <- res.mean
    }
    
    cutoff <- 120

    setup.data <- rep(max.val, cutoff)
    setup.data[1] <- min.val
    par(mar=c(5, 4, 4, 5) + 0.1, cex=1.4, col = 'black')
    colors <- sapply(to.score, GetColorForName)
    heading <- "Average improvement over random sampling at dawn"
    plot(setup.data, main=heading, type = 'n', xlab="After this many minutes", ylab="mean % improvement over random at dawn")
    for (i in 1:length(to.score)) {              
        line <- results[[i]]
        sd <- NA  
        PlotLine(line, sd = sd, col.rgb = colors[,i], lty = 'solid')
    }
    
    legend.names <- sapply(to.score, GetRankingLegendName)
    legend.cols <- apply(colors, 2, RgbCol)
    legend("bottomright",  legend = legend.names, 
           col = legend.cols, 
           lty = 'solid', text.col = "black", lwd = 2)
    
    par(col = 'black')
    points(rep(0, 1440), type='l', lty = 'dotted', lwd = 2)

    
    return(results)
    
    
    
}


GetHeading <- function (ranks) {
    target.min.ids <- GetMeta('target.min.ids', ranks$dependencies$target.min.ids)
    target <- target.min.ids$params$target
    target <- gsub("[^A-Za-z0-9 -]", '', target)
    heading = target;
    return(heading)
}


EvaluateSamples2d.1 <- function (ranks, cutoff = NA) {
    # given a list of minutes
    # simulates a species richness survey, noting the 
    # total species found after each minute, and the total
    # number of species found after each minute
    
    # todo: this is broken, use EvaluateSamples2d.2
    
    #!! target only = false because of this hack
    speciesmins <- GetTags(target.only = FALSE);  # get tags within outer target 
    ranked.min.ids <- ReadOutput('target.min.ids')
    
    # hacked in for escience paper
    mins.for.comparison <- GetMinuteList()
    mins.for.comparison <- mins.for.comparison[mins.for.comparison$site == "NW" & mins.for.comparison$date == "2010-10-13",]    
    
    species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, mins.for.comparison)
    
    ranked.progressions <- list()
    dn <- dimnames(ranks)
    methods <- dn$ranking.method
    i <- 1
    while (i <= length(methods)) {
        method <- methods[i]
        ordered.min.ids <- ranked.min.ids$data$min.id[order(ranks[i,])]
        
        if (add.dawn) {
            dawn.mins <- GetRandomDawnMins(40)
            ordered.min.ids <- c(dawn.mins$min.id, ordered.min.ids)
        }

        ranked.progressions[[method]] <- GetProgression(species.in.each.sample, ordered.min.ids)
        WriteRichnessResults(ordered.min.ids, ranked.progressions[[method]], method, species.in.each.sample)
        i <- i + 1
    }

    optimal <- OptimalSamples(speciesmins, species.in.each.min = species.in.each.sample)
    
    if (IsDawn(mins.for.comparison$min)) {
        random.at.dawn <- RandomSamples(speciesmins = speciesmins, species.in.each.sample, mins.for.comparison$min.id, dawn.first = TRUE)   
    } else {
        random.at.dawn <- NA
    }
    
 
    random.all <- RandomSamples(speciesmins = speciesmins, species.in.each.sample, mins.for.comparison$min.id, dawn.first = FALSE)
    #from.indices <- IndicesProgression(species.in.each.sample, min.ids)
    
    GraphProgressions(ranked.progressions = ranked.progressions,
                      optimal = optimal$found.species.count.progression, 
                      random.at.dawn = random.at.dawn, 
                      random.all = random.all)
    
}

EvaluateSamples2d.2 <- function (ranks, add.dawn = FALSE, cutoff = NA, heading = '') {
    # given a list of minutes
    # simulates a species richness survey, noting the 
    # total species found after each minute, and the total
    # number of species found after each minute
    #
    # Args
    #   ranks: data.frame; colnames are the names of the ranking method
    #                      values are min.ids in ascending order or ranke (best first)

    #add.dawn = Confirm("Add Dawn?")
    add.dawn <- FALSE;
    
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
    
    # needs fixing

    
#     # hacked in for escience paper
#     if (add.dawn) {
#         # if add dawn, we are now comparing the whole day
#         mins.for.comparison <- GetMinuteList()
#         mins.for.comparison <- mins.for.comparison[mins.for.comparison$site == "NW" & mins.for.comparison$date == "2010-10-13",]
#     } else {
#         mins.for.comparison <- ranked.min.ids$data
#     }
    

    
#    # temp for escience paper
#    if (add.dawn) {
#        dawn <- GetDawnMins()
#        if (length(intersect(dawn$min.id, ranked.min.ids$data$min.id)) > 0) {
#            stop('cant add dawn if dawn is already in the target')
#        }
#    }

    species.in.each.sample <- SpeciesInEachSampleCached(mins.for.comparison$min.id)
    # holds a lot of info about the species found
    ranked.progressions <- list(mins.for.comparison$min.id)

    # holds only the running total of species
    ranked.count.progressions <- list()
    methods <- names(ranks$data)

    #    species.in.each.sample.df <- data.frame(min.id = names(species.in.each.sample), species = as.character(species.in.each.sample)) #this takes a while
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
    dependencies <- list(target.min.ids = ranks$dependencies$target.min.ids)
    optimal <- ReadOutput('optimal.samples', dependencies = dependencies, false.if.missing = TRUE)
    if (!is.list(optimal)) {
        optimal <- OptimalSamples(species.in.each.min = species.in.each.sample)
        WriteOutput(optimal, 'optimal.samples', dependencies = dependencies)
    } else {
        optimal <- optimal$data
    }
    
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

    fn <- paste0(heading,'.png')
    filepath <- file.path('/Users/n8933464/Documents/sample_selection_output/plots', fn)
    heading <- paste("Species accumulation Curve:", heading)

    progressions <- c(ranked.count.progressions, comparison.progressions)
    GraphProgressions(progressions, heading = heading, fn = filepath)

    return(progressions)
    
}

SpeciesInEachSampleCached <- function (min.ids = NULL) {
    # checks if we have a saved species.in.each.sample for the target mins already
    # and if so, uses it, if not looks in the database etc
    # 
    # Args:
    #   min.ids: data.frame; the minutes that we want to look in
    #   params: list; the params to save the results with
    #   dependencies: list; the dependencies to save the results with
    
    params = list(study = GetStudyDescription())
    # todo: remove "use last accessed" after building a check for matching params and dependencies into ReadOutput
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

SpeciesInEachSampleCached.1 <- function (min.ids, target.min.ids.version, add.dawn) {
    # checks if we have a saved species.in.each.sample for the target mins already
    # and if so, uses it, if not looks in the database etc
    # 
    # Args:
    #   min.ids: data.frame; the minutes that we want to look in
    #   params: list; the params to save the results with
    #   dependencies: list; the dependencies to save the results with
    

    species.in.each.sample <- ReadOutput('species.in.each.min', dependencies = dependencies, false.if.missing = TRUE)
    if (!is.list(species.in.each.sample)) {
        # if 'add dawn' equals true, we are prepending some dawn minutes to ranked minutes which appear outside of dawn
        # we therefore comparing our method with the whole day
        speciesmins <- GetTags(target.only = !add.dawn);  # get tags within outer target 
        speciesmins <- AddMinuteIdCol(speciesmins)
        species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, mins.for.comparison)
        WriteOutput(species.in.each.sample, 'species.in.each.min', dependencies = dependencies)
    } else {
        species.in.each.sample <- species.in.each.sample$data
    }
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
       GraphProgressions3d(ranked.progressions[i,,])   
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

OptimalSamples <- function (mins = NA, species.in.each.min = NA) {
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
    ranked.mins <- RankMinutesFromIndices()
    ranked.mins <- ranked.mins[ ranked.mins$min.id %in% which.mins$min.id, ] 
    ordered.min.ids <- ranked.mins$min.id[order(ranked.mins$rank)]
    progression <- GetProgression(species.in.each.sample, ordered.min.ids)
    return(progression)
}

#todo: check if this function is still used
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

ListSpeciesInEachMinute <- function (speciesmins, mins = NA) {
    # given a list of minute ids, and a list of all tags
    # creates a list of species for each minute in mins
    #
    # Args:
    #   speciesmins: list of all the tags, with their site, date and min-of-the-day
    #   mins: vector of minute ids or data frame of minutes, if ommited will use all minutes in speciesmins
    #  
    # Returns: list. names of the list are the min.id. values of each element is a vector of species ids
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
#     for (i in 1:nrow(mins)) {
#         sp.list <- speciesmins$species.id[speciesmins$site == mins$site[i] & speciesmins$date == mins$date[i] & speciesmins$min == mins$min[i]]
#         species.in.each.min[[as.character(mins$min.id[i])]] <- sp.list
#     }
    for (i in 1:nrow(mins)) {
        sp.list <- speciesmins$species.id[speciesmins$min.id == mins$min.id[i]]
        species.in.each.min[[as.character(mins$min.id[i])]] <- sp.list
    }
    
    return(species.in.each.min)
}



GraphProgressions <- function (progressions, 
                               cutoff = 120, 
                               heading = NULL,
                               fn = NULL) {
    
    # graphs Species Accumulation Curves. A 'curve' is an integer vector, which is the total number of species found
    #
    # Args: 
    #   progressions: list; in the form: name = list(line, legend, sd)
    
    
    #   ranked.count.progressions: list; one memeber of the list for each ranking. 
    #                                    Each memeber is an integer vector representing the total number of species at that sample
    #   optimal: integer vector; 
    #   random.at.dawn: list; 2 integer vectors: the mean and the standard deviation curves
    #   random.all: list; see random.at.dawn
    #   from.indicies; integer.vector; sampling from indicies
    #   cutoff: integer; where to cut the graph off at (after how many samples)
    #   ranked.legend: character vector; what to put for each of the 'ranked.count.progressions'
    #   heading: string; title of the graph
    
    #ranked.count.progressions <- NA
    
    if (!is.null(fn)) {
        png(filename = fn,
            width = 700, height = 700, units = "px", pointsize = 13,
            bg = "white",  res = NA,
            type = c("cairo", "cairo-png", "Xlib", "quartz"))
    }
    
    rank.names <- names(progressions);
    #rank.names <- c()
    
    # truncate all output at cuttoff

    plot.width <- 0
    plot.height <- 0
    #TODO: don't really need to truncate, just define the width for the plot and it will be cropped
        for (i in 1:length(progressions)) {
            if (is.numeric(progressions[[i]])) {
                progressions[[i]] <- Truncate(progressions[[i]], cutoff)
                plot.width <- max(plot.width, length(progressions[[i]]))
                plot.height <- max(plot.height, max(progressions[[i]]))
            } else if (is.list(progressions[[i]])) {
                progressions[[i]]$mean  <- Truncate(progressions[[i]]$mean, cutoff)
                progressions[[i]]$sd  <- Truncate(progressions[[i]]$sd, cutoff)   
                plot.width <- max(plot.width, length(progressions[[i]]$mean))
                plot.height <- max(plot.height, max(progressions[[i]]$mean))
            }  else {
                stop("invalid progressions data type")
            }
        }

    
#     # initialise a dataframe to hold all the info about each plot
#     col.names <- c('legend', 'r', 'g', 'b', 'line.style')
#     plots <- as.data.frame(matrix(NA, nrow = length(progressions), ncol = length(col.names)))
#     colnames(plots) <- col.names
#     nextrow <- function () {
#         return(match(NA, plots$legend))
#     }
    
    par(col = 'black')
    if (is.null(heading)) {
        heading <- "Species accumulation curve"
    }
    
    setup.data <- rep(plot.height, plot.width)
    setup.data[1] <- 0
    par(mar=c(5, 4, 4, 5) + 0.1, cex=1.4)
    plot(setup.data, main=heading, type = 'n', xlab="After this many minutes", ylab="Number of species found")
    
    percent.ticks.at <- (0:6)/6
    axis(4, at=percent.ticks.at*60, labels=round(percent.ticks.at*100))
    mtext("% of total", side=4, line=2.5)
    par(cex=1.2)

    # whether each of the progressions is a comparison curve or the result of 
    # our smart sampling
    is.comparison.curve <- sapply(names(progressions), function (name) {
        to.match <- c('random', 'optimal', 'indices')
        return(length(grep(paste(to.match, collapse = "|"), name)) > 0)
    })
    
    line.styles <- rep('solid', length(progressions))
    line.styles[is.comparison.curve] <- 'dashed'
        

    
    colors <- sapply(names(progressions), GetColorForName)

    
    p.names <- names(progressions)
    for (i in 1:length(p.names)) {              
        if (is.list(progressions[[i]])) {
            line <- progressions[[i]]$mean
            sd <-  progressions[[i]]$sd
        } else {
            line <- progressions[[i]]
            sd <- NA
        }             
        PlotLine(line, sd = sd, col.rgb = colors[,i], lty = line.styles[i])
    }
    
    legend.cols <- apply(colors, 2, RgbCol)

    legend.names <- p.names
    legend.names[!is.comparison.curve] <- sapply(legend.names[!is.comparison.curve], GetRankingLegendName)
    
    legend("bottomright",  legend = legend.names, 
           col = legend.cols, 
           lty = line.styles, text.col = "black", lwd = 2)

    if (!is.null(fn)) {
        dev.off()   
    }

    
}

GetColorForName <- function (name) {
    colors.1 <- list(
        'optimal' = c(0.1,0.1,0.1),
        'random.at.dawn' = c(0.0,0.8,0.0),
        'random.all' = c(0.9,0.6,0.2),
        'indices' = c(.3, .6, .6)
    )
    colors.2 <- list(
        'c.1' = c(1,0.3,0),
        'c.2' = c(0,0.6,0.9),
        'c.3' = c(0.7,0.1,0.9),
        'c.4' = c(0.8,0.2,0.2)
    )
    if (name %in% names(colors.1)) {
        return(colors.1[[name]])
    } else {
        return(colors.2[[MapColor(name)]])
    }   
}

MapColor <- function (rank.name) {
    # maps ranking method names to colour names
    # this can change between papers, but keep consistent within a paper
    map <- list('X4' = 'c.1', 'X8' = 'c.2')
    return(map[[rank.name]]) 
}



GetRankingLegendName <- function (ranking.code) {
    codes <- list(
        "X4" = "Ranked by Event Count",
        "X8" = "Ranked by Event Count with Temporal Dispersal"
        )
    if (ranking.code %in% names(codes)) {
        return(codes[[ranking.code]])
    } else {
        return(ranking.code)
    }
}


GraphProgressions.old <- function (progressions, 
                               cutoff = 150, 
                               ranked.legend = NA, 
                               heading = NA) {
    
    # graphs Species Accumulation Curves. A 'curve' is an integer vector, which is the total number of species found
    #
    # Args: 
    #   progressions: list; in the form: name = list(line, legend, sd)
    
    
    #   ranked.count.progressions: list; one memeber of the list for each ranking. 
    #                                    Each memeber is an integer vector representing the total number of species at that sample
    #   optimal: integer vector; 
    #   random.at.dawn: list; 2 integer vectors: the mean and the standard deviation curves
    #   random.all: list; see random.at.dawn
    #   from.indicies; integer.vector; sampling from indicies
    #   cutoff: integer; where to cut the graph off at (after how many samples)
    #   ranked.legend: character vector; what to put for each of the 'ranked.count.progressions'
    #   heading: string; title of the graph
   
    #ranked.count.progressions <- NA
    
    rank.names <- names(ranked.count.progressions);
    #rank.names <- c()
    
    # truncate all output at cuttoff
    if (is.numeric(cutoff)) {
        optimal <- Truncate(optimal, cutoff)
        if (!is.null(from.indices)) {
            from.indices <- Truncate(from.indices, cutoff)
        }
        
        if (is.list(random.at.dawn)) {
            random.at.dawn$mean  <- Truncate(random.at.dawn$mean, cutoff)
            random.at.dawn$sd  <- Truncate(random.at.dawn$sd, cutoff)         
        } 
        if (is.list(random.all)) {
            random.all$mean  <- Truncate(random.all$mean, cutoff)
            random.all$sd  <- Truncate(random.all$sd, cutoff)
        } 
        
        if (length(rank.names) > 0) {
            for (i in 1:length(rank.names)) {
                if (is.list(ranked.count.progressions[[rank.names[i]]])) {
                    ranked.count.progressions[[rank.names[i]]]$mean <- Truncate(ranked.count.progressions[[rank.names[i]]]$mean, cutoff)
                    ranked.count.progressions[[rank.names[i]]]$sd <- Truncate(ranked.count.progressions[[rank.names[i]]]$sd, cutoff)
                    plot.width <- length(ranked.count.progressions[[rank.names[i]]]$sd)
                } else {
                    ranked.count.progressions[[rank.names[i]]] <- Truncate(ranked.count.progressions[[rank.names[i]]], cutoff)
                    plot.width <- length(ranked.count.progressions[[rank.names[i]]])
                }
                
            }
        } else {
            plot.width <- cutoff
        }
    }
    
    # initialise a dataframe to hold all the info about each plot
    num.plots <- length(ranked.count.progressions) + sum(!c(is.null(optimal), is.null(random.at.dawn), is.null(random.all), is.null(from.indices)))
    col.names <- c('legend', 'r', 'g', 'b', 'line.style')
    plots <- as.data.frame(matrix(NA, nrow = num.plots, ncol = length(col.names)))
    colnames(plots) <- col.names
    nextrow <- function () {
        return(match(NA, plots$legend))
    }
    
    par(col = 'black')
    if (is.null(heading)) {
        heading <- "Species accumulation curve"
    }
    
    # setup using optimal for the y axis, since it will have all the species
    setup.data <- rep(max(optimal), plot.width)
    setup.data[1] <- 0
    par(mar=c(5, 4, 4, 5) + 0.1)
    plot(setup.data, main=heading, type = 'n', xlab="After this many minutes", ylab="Number of species found")
    
    percent.ticks.at <- (0:6)/6
    axis(4, at=percent.ticks.at*60, labels=round(percent.ticks.at*100))
    mtext("% of total", side=4, line=2.5)
    # plot each of the ranking results
    line.colours <- matrix(NA, ncol = 3, nrow = 0)

    
    # plot random at dawn, with standard deviations
    # colours : http://www.stat.columbia.edu/~tzheng/files/Rcolor.pdf
    if (is.list(random.at.dawn) && length(random.at.dawn$mean) > 0) {
        row <- nextrow()
        col <- c(0.0,0.8,0.0)
        plots[row, c('r', 'g', 'b')] <- col
        plots$legend[row] <- "Random sampling at dawn"
        plots$line.style <- 'dashed'
        PlotLine(line = random.at.dawn$mean, col.rgb = col, sd = random.at.dawn$sd, lty = 'dashed')
    }
    
    # plot random, with standard deviations
    if (is.list(random.all) && length(random.all$mean) > 0) {
        row <- nextrow()
        col <- c(0.9,0.6,0.2)
        plots[row, c('r', 'g', 'b')] <- col
        plots$legend[row] <- "Random sampling"
        plots$line.style <- 'dashed'
        PlotLine(line = random.all$mean, col.rgb = col, sd = random.all$sd, lty = 'dashed')
    }
    
    # plot optimal
    if (!is.nul(optimal)) {
        row <- nextrow()
        col <- c(0.1,0.1,0.1)
        plots[row, c('r', 'g', 'b')] <- col
        plots$legend[row] <- "Optimal sampling"
        plots$line.style <- 'dashed'
        PlotLine(optimal, col.rgb = col, lty = 'dashed')  
    }

    
    # plot from indices
    # !! if this is activated, colours will be wong
    if (!is.null(from.indices)) {
        row <- nextrow()
        col <- c(.3, .6, .6)  # dark pink
        plots[row, c('r', 'g', 'b')] <- col
        plots$legend[row] <- "Indices"
        plots$line.style <- 'dashed'
        PlotLine(optimal, col.rgb = col, lty = 'dashed')
    }
    
    #ranking.method.names <- c(
    #    "Samples ranked by event count only",
    #    "Samples ranked by number of clusters (A)",
    #    "Samples ranked by number of clusters (B)"
    #    )
    
    
    #matrix: rows are colours, columns are channels 
    ranking.method.colours <- matrix(c(1,0.3,0,
                                       0,0.6,0.9,
                                       0.7,0.1,0.9,
                                       0.8,0.2,0.2), ncol = 3, byrow = TRUE)
    
    # give the ranking methods a name here to plot them on the graph
    # leave the name blank to ommit from the graph
    
    if (is.null(ranked.legend)) {
        ranked.legend <- c('Smart Sampling by Event Count', 
                             "Smart Sampling by Event count with temporal dispersal")   
    }

    

    if (length(rank.names) > 0) {
        for (i in 1:length(rank.names)) {
            if (ss.method.names[i] != "") {                
                if (is.list(ranked.count.progressions[[i]])) {
                    line <- ranked.count.progressions[[i]]$mean
                    sd <-  ranked.count.progressions[[i]]$sd
                } else {
                    line <- ranked.count.progressions[[i]]
                    sd <- NA
                }             
                PlotLine(line, ranking.method.colours[i, ])           
                #legend.names = c(legend.names, paste('Smart Sampling method', rank.names[i]))
                legend.names = c(legend.names, ss.method.names[i])
                line.colours <- rbind(line.colours,  ranking.method.colours[i, ])            
            }
        }
    }
    
    legend.cols <- apply(as.matrix(line.colours), 1, RgbCol)
    
    
    lty <- c(rep('dashed', length(legend.cols) - length(rank.names) + 1) , rep('solid', length(rank.names)))
    
    legend("bottomright",  legend = legend.names, 
           col = legend.cols, 
           lty = lty, text.col = "black", lwd = 2)
    
}

PlotLine <- function (line, col.rgb, sd = NA, sd.col = NA, lty = 'solid') {

    if (is.numeric(sd)) {
        poly.y <- c(line + sd, rev(line - sd))
        poly.x <- c(1:length(line), length(line):1)
        fill.col <- rgb(col.rgb[1], col.rgb[2], col.rgb[3], 0.1)
        polygon(poly.x, poly.y,  col = fill.col, border = fill.col)        
    }
    line.col <- rgb(col.rgb[1], col.rgb[2], col.rgb[3], 1)
    par(col = line.col)
    points(line, type='l', lty = lty, lwd = 3)
}



GraphProgressions3d.1 <- function (progressions, cutoff = 180) {
    
    num.clusters <- as.numeric(dimnames(progressions)$num.clusters)
    sample.num <- 1:ncol(progressions)
    
    #x <- rep(sample.num, length(num.clusters))
    #z <- rep(num.clusters, times = 1, each = length(sample.num))
    #x <- rep(sample.num, times = 1, each = length(num.clusters))
    #z <- rep(num.clusters, times = length(sample.num), each = 1)
    #y <- as.vector(progressions)
    
    persp(z = t(progressions), 
          xlab = 'sample num', ylab = 'num groups', zlab = 'species found',
          main = 'sample progressions for different number of cluster groups', sub = NULL,
          theta = -35, phi = 25, r = sqrt(3), d = 1,
          scale = TRUE, expand = 1,
          col = "white", border = NULL, ltheta = -135, lphi = 0,
          shade = NA, box = TRUE, axes = TRUE, nticks = 5,
          ticktype = "simple")
    
}

GraphProgressions3d <- function (progressions, cutoff = 120) {
    
    require('rgl')
    
    if (ncol(progressions) > cutoff) {
        progressions <- progressions[,1:cutoff]
    }
    

    
    
    image(progressions)
    View(progressions)
    
    num.clusters <- as.numeric(dimnames(progressions)$num.clusters)
    sample.num <- 1:ncol(progressions)
    
   # x <- rep(sample.num, length(num.clusters))
   # z <- rep(num.clusters, times = 1, each = length(sample.num))
   # x <- rep(sample.num, times = 1, each = length(num.clusters))
   # z <- rep(num.clusters, times = length(sample.num), each = 1)
   # y <- as.vector(t(progressions))

    z <- 1:length(num.clusters) * 4
    x <- sample.num
    y <- t(progressions)

    ylim <- range(y)
    ylen <- ylim[2] - ylim[1] + 1
    colorlut <- terrain.colors(ylen,alpha=0) # height color lookup table
    col <- colorlut[ y-ylim[1]+1 ] # assign colors to heights for each point
    open3d(mouseMode = c('trackball', 'polar', 'xAxis'))
    rgl.surface(x, z, y, color=col, alpha=0.9, back="lines")
   
   
    #axes3d()
    bbox3d(zlen = length(z),
           zlab = as.character(num.clusters),
           zunit="pretty", 
           expand=1.03,
          draw_front=FALSE)  

    rgl.surface(x, z, matrix(0, length(x), length(z)), color=c('black'), back="fill")
   

    
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
    
    
    
    if (class(ordered.min.list) == 'integer') {
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





