

ValidateMins <- function (mins = NA) {
    # mins can either be a vector of minute ids, in which case we expand it to 
    # include [site,date,min], a dataframe of [site,date,min], or nothing in which
    # case we use all the minutes from the study
    
    if (class(mins) == 'integer' || class(mins) == 'numeric') {
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
    
    mins <- ValidateMins(mins)

    total.num.species <- length(unique(speciesmins$species.id))
    # maximum number of samples is the number of species
    selected.samples <- rep(NA, total.num.species)
    found.species.count.progression <- rep(NA, total.num.species)
    found.species.progression <- vector("list", total.num.species)
    all.found.species <- numeric()
    
    # create list of the species in each minute
    species.in.each.min <- ListSpeciesInEachMinute(speciesmins, mins = mins) 

    # this could probably be done faster with a sparsematrix, whatever
    
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

RandomSamplesAtDawn <- function (speciesmins = NA, mins = NA, num.repetitions = 100, dawn.from = 315, dawn.to = 495) {
    
    if (class(speciesmins) != 'data.frame') {
        speciesmins <- GetTags()
    }
    
    mins <- ValidateMins(mins)

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
    #progression.average <- round(progression.average)
    progression.sd <- apply(repetitions, 1, sd)
    return(list(mean = progression.average, sd = progression.sd))
    
    
    
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

ListSpeciesInEachMinute <- function (speciesmins, mins = NA) {
    # given a list of minute ids, and a list of all tags
    # creates a list of species for each minute in mins
    #
    # Args:
    #   speciesmins: list of all the tags
    #   min.ids: vector of minute ids or data frame of minutes
    #  
    # Returns: list
    #
    # Details:
    #  if min.ids is supplied, this will be used for the list of mins
    #  if mins.ids is not supplied, mins must be supplied. 
    
    mins <- ValidateMins(mins)
    species.in.each.min <- vector("list", nrow(mins))
    for (i in 1:nrow(mins)) {
        sp.list <- speciesmins$species.id[speciesmins$site == mins$site[i] & speciesmins$date == mins$date[i] & speciesmins$min == mins$min[i]]
        species.in.each.min[[mins$min.id[i]]] <- sp.list
    }
    return(species.in.each.min)
}

EvaluateSamples <- function (samples = NA, cutoff = NA) {
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
    species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, samples$min.id)
    
    ranked.progressions <- list()
    r.cols <- GetRankCols(samples)
    i <- 1
    while (i <= length(r.cols)) {
            ranked.progressions[[r.cols[i]]] <- GetProgression(species.in.each.sample, order(samples[, r.cols[i]]))
            i <- i + 1
    }
    
    
    target.min.ids <- ReadOutput('target.min.ids')
    optimal <- OptimalSamples(speciesmins, target.min.ids)
    random.at.dawn <- RandomSamplesAtDawn(speciesmins = speciesmins, mins <- target.min.ids)
    
    GraphProgressions(optimal, random.at.dawn, ranked.progressions)
    
    for (i in 1:length(r.cols)) {
        WriteRichnessResults(r.cols[i], samples, species.in.each.sample, ranked.progressions[[r.cols[i]]], r.cols[i])
    }
    
}

GraphProgressions <- function (optimal, random.at.dawn, ranked.progressions, cutoff = 180) {
   
    rank.names <- names(ranked.progressions);
    
    # truncate all output at cuttoff
    if (!is.na(cutoff)) {
        optimal <- Truncate(optimal, cutoff)
        random.at.dawn$mean  <- Truncate(random.at.dawn$mean, cutoff)
        random.at.dawn$sd  <- Truncate(random.at.dawn$sd, cutoff)
        random.at.dawn$sd  <- Truncate(random.at.dawn$sd, cutoff)
        for (i in 1:length(rank.names)) {
            ranked.progressions[[rank.names[i]]]$count <- Truncate(ranked.progressions[[rank.names[i]]]$count, cutoff)
        }
    }
    
    
    
    #plot them against each other
    legend.names = c("optimal sampling", "random at dawn")
    legend.cols = c('blue', 'green')
    
    par(col = 'black')
    heading <- "Species count progression"
    setup.data <- rep(max(optimal$found.species.count.progression), length(ranked.progressions[['r1']]$count))
    setup.data[1] <- 0
    plot(setup.data, main=heading, type = 'n', xlab="after this many minutes", ylab="number of species found")
    
    par(col = 'green')
    poly.y <- c(random.at.dawn$mean + random.at.dawn$sd, rev(random.at.dawn$mean - random.at.dawn$sd))
    poly.x <- c(1:length(random.at.dawn$mean), length(random.at.dawn$mean):1)
    
    polygon(poly.x, poly.y,  col = 'honeydew1', border = 'honeydew2')
    points(random.at.dawn$mean, type='l')
    
    
    par(col = 'blue')
    points(optimal$found.species.count.progression, type='l')
    
    ranked.colours <- c('red', 'orange', 'darkmagenta', 'cyan', 'magenta')

    for (i in 1:length(rank.names)) {
        par(col = ranked.colours[i])
        points(ranked.progressions[[rank.names[i]]]$count, type='l')
        legend.names = c(legend.names, rank.names[i])
        legend.cols = c(legend.cols, ranked.colours[i])
    }
    
    
    
    
    legend("bottomright",  legend = legend.names, 
           col = legend.cols, 
           lty = c(2, 2), text.col = "black")
    
}

WriteRichnessResults <- function (ranking.col, samples, species.in.each.sample, found.species.progression, output.fn) {
    # given a list of samples with rankings, a list of species in each sample, and a species progression
    # writes is all to a csv
    # samples and species.in.each.sample are in order of minute id
    # found.species.progression is already in the order of the ranking
    
    # sort the samples by rank
    
    sort.order <- order(samples[[ranking.col]])
    samples <- samples[sort.order, ]
    species.in.each.sample <- species.in.each.sample[sort.order]
    
    #writes the results of a sample ranking to a csv
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
    
    WriteOutput(output, output.fn)
    
    
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
    rankings = 'all.supplied'
    if(class(samples) == 'integer' || class(samples) == 'numeric') {
        #minute ids have been supplied
        minlist <- ReadOutput('minlist');
        minlist$min.id <- 1:nrow(minlist)
        samples <- minlist[samples,]
    } else {
        if(class(samples) != 'data.frame') {
            samples <- ReadOutput('ranked_samples')
            rankings <- GetRankCols(samples)
            subset <- rep(FALSE, nrow(samples))
            for (i in 1:length(rankings)) {
                subset <- subset | samples[[rankings[i]]] <= g.num.samples
            }
            
            samples <- samples[subset,]
            
        }
    }

    events <- ReadOutput('clusters')
    events <- AssignColourToGroup(events)
    #event.col <- as.data.frame(rep(NA, nrow(samples)))
    #colnames(event.col) <- c('events')
    #samples <- cbind(samples, event.col)
    
#    w <- 1000
    # todo: fix this so that it the height of each spectrogram
    # is what it actually is, instead of hardcoded 256
#    h <- nrow(samples) * 256
    
    temp.dir <- TempDirectory()
    
    # file names for later use in imagemagick command to append together
    sample.spectro.fns <- c()
    
    for (i in 1:nrow(samples)) {
        
        #add events that belong in this sample
        min.id <- as.character(samples$min.id[i])
        minute.events <- events[which(events$min.id == min.id),]
        
        temp.fn <- paste(min.id, 'png', sep = '.')
        img.path <- file.path(temp.dir, temp.fn)
        sample.spectro.fns <- c(sample.spectro.fns, img.path)
        
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


    samples$spectro.fn <- sample.spectro.fns
    # for each ranking, create a 
    for (i in 1:length(rankings)) {
        if (rankings[i] %in% colnames(samples)) {
            output.fn <- paste('InspectSamples', rankings[i],  collapse = "_", sep='.')
            fns <- samples$spectro.fn[samples[[rankings[i]]] <= g.num.samples]
        } else {
            output.fn <- paste('InspectSamples', samples$min.id,  collapse = "_", sep='.') 
            fns <- samples$spectro.fn 
        }
        output.file <- OutputPath(output.fn, ext = 'png')
        fns <- paste(fns, collapse = " ")
        command <- paste("/opt/local/bin/convert", 
                         fns, "-append", output.file)
        err <- try(system(command))  # ImageMagick's 'convert'
        
    }
    
}

GetRankCols <- function (data.frame) {
    #returns the column names of data.frame which are names of ranking columns
    # i.e. the character "r" followed by an integer
    l <-  length(colnames(data.frame))
    return(intersect(paste0(rep('r',l), 1:l), colnames(data.frame)))  
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

