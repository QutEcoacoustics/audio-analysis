EvaluateSamples <- function () {
    
    ranks <- ReadObject('ranked_samples')
    d.names <- dimnames(ranks)
    num.clusters.options <- d.names$num.clusters
    default <- length(num.clusters.options) / 2
    num.clusters.choices <- GetMultiUserchoice(num.clusters.options, 'num clusters', default = default)
    
    ranks <- ranks[,num.clusters.choices,]
    
    if (length(num.clusters.choices) > 1) {
        # more than 2 dimensions to graph, so make 3d plot
        EvaluateSamples3d(ranks)
    } else {
        EvaluateSamples2d(ranks)  
    }
    
    
}

EvaluateSamples2d <- function (ranks, cutoff = NA) {
    # given a list of minutes
    # simulates a species richness survey, noting the 
    # total species found after each minute, and the total
    # number of species found after each minute
    
    
    speciesmins <- GetTags();  # get tags within outer target 
    min.ids <- ReadOutput('target.min.ids')
    species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, min.ids)
    
    ranked.progressions <- list()
    dn <- dimnames(ranks)
    methods <- dn$ranking.method
    i <- 1
    while (i <= length(methods)) {
        method <- methods[i]
        ordered.min.ids <- min.ids$min.id[order(ranks[i,])]
        ranked.progressions[[method]] <- GetProgression(species.in.each.sample, ordered.min.ids)
        WriteRichnessResults(ordered.min.ids, ranked.progressions[[method]], method, species.in.each.sample)
        i <- i + 1
    }

    optimal <- OptimalSamples(speciesmins, species.in.each.min = species.in.each.sample)
    random.at.dawn <- RandomSamples(speciesmins = speciesmins, species.in.each.sample, min.ids$min.id, dawn.first = FALSE)
    
    GraphProgressions(optimal$found.species.count.progression, random.at.dawn, ranked.progressions)
    
}

EvaluateSamples3d <- function (ranks) {
    

    min.ids <- ReadOutput('target.min.ids')
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

OptimalSamples <- function (speciesmins = NA, mins = NA, species.in.each.min = NA) {
    # determines the best possible selection of [numsamples] minute samples
    # to find the most species
    # 
    # Args:
    #   speciesmins: dataframe; the list of species in each minute. If not included
    #                            will retreive from database
    #   mins: dataframe; the list of minutes we can select from. If not
    #                    included then will be read from config
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
    
    if (class(speciesmins) != 'data.frame') {
        speciesmins <- GetTags()
    }
    
    
    if (class(species.in.each.min) != 'list') {
        species.in.each.min <- ListSpeciesInEachMinute(speciesmins, mins = mins) 
    }


    total.num.species <- length(unique(speciesmins$species.id))
    # maximum number of samples is the number of species, or number of minutes
    initial.length <- min(c(total.num.species, length(species.in.each.min)))
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
    
    # optimal should complete well before the initialised length
    selected.samples <- selected.samples[! is.na(selected.samples)]
    found.species.count.progression <- found.species.count.progression[! is.na(found.species.count.progression)]
    
    return(list(
        found.species.progression = found.species.progression,
        found.species.count.progression = found.species.count.progression,
        selected.mins = selected.samples
        ))
    
}

RandomSamples <- function (speciesmins = NA, species.in.each.sample
                                 = NA, mins = NA, num.repetitions = 100, dawn.first = TRUE, dawn.from = 315, dawn.to = 495) {
    # repeatedly performs a sample selection from random selection of dawn minutes
    # 
    # Value:
    #   list: mean: the mean species count progression (count only)
    #   list: sd: the standard deviation minute by minute
    
    Report(3, 'performing random sampling at dawn (RSAD)')
    
    if (class(speciesmins) != 'data.frame') {
        speciesmins <- GetTags()
    }
    
    mins <- ValidateMins(mins)
    which.dawn <- mins$min >= dawn.from & mins$min <= dawn.to
    mins.dawn <- mins[which.dawn, ]
    mins.notdawn <- mins[!which.dawn, ]
    
    min.ids.dawn <- mins.dawn$min.id
    min.ids.notdawn <- mins.notdawn$min.id
    
    Report(4, length(min.ids.dawn), 'of the target are at dawn')
    
    #species.in.each.min is optional. 
    if (class(species.in.each.sample) != 'list') {
        species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, mins = mins$min.id) 
    }

    repetitions <- matrix(rep(NA, num.repetitions * nrow(mins)), ncol = num.repetitions)
    
    Report(4, 'performing', num.repetitions, 'repetitions of RSAD')
    
    # get the progression for random mins many times
    for (i in 1:num.repetitions) {
        
        if (i %% 10) {
            Dot()
        }
        
        if (dawn.first) {
            # create a jumbled version of the list of species in each min
            # putting the dawn part always at the start
            sample.order.dawn <- sample(1:length(min.ids.dawn), length(min.ids.dawn), replace = FALSE)
            sample.order.notdawn <- sample(1:length(min.ids.notdawn), length(min.ids.notdawn), replace = FALSE)
            jumbled.min.ids <- c(min.ids.dawn[sample.order.dawn], min.ids.notdawn[sample.order.notdawn])
        } else {
            sample.order <- sample(1:nrow(mins), nrow(mins), replace = FALSE)
            jumbled.min.ids <- mins$min.id[sample.order]
        }
        found.species.progression <- GetProgression(species.in.each.sample, jumbled.min.ids)
        repetitions[,i] <- found.species.progression$count  
    }
    #get average progression of counts 
    progression.average <- apply(repetitions, 1, mean)
    #progression.average <- round(progression.average)
    progression.sd <- apply(repetitions, 1, sd)
    
    Report(5, "RSAD complete")
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
    #  if min.ids is not supplied, mins must be supplied. 
    
    Report(5, 'counting species in each minute')
    mins <- ValidateMins(mins)
    species.in.each.min <- vector("list")
    for (i in 1:nrow(mins)) {
        sp.list <- speciesmins$species.id[speciesmins$site == mins$site[i] & speciesmins$date == mins$date[i] & speciesmins$min == mins$min[i]]
        species.in.each.min[[as.character(mins$min.id[i])]] <- sp.list
    }
    return(species.in.each.min)
}

GraphProgressions <- function (optimal, random.at.dawn, ranked.progressions, cutoff = 180) {
   
    rank.names <- names(ranked.progressions);
    
    # truncate all output at cuttoff
    if (!is.na(cutoff)) {
        optimal <- Truncate(optimal, cutoff)
        random.at.dawn$mean  <- Truncate(random.at.dawn$mean, cutoff)
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
    
    # setup using optimal for the y axis, since it will have all the species
    setup.data <- rep(max(optimal), length(ranked.progressions[[rank.names[1]]]$count))
    setup.data[1] <- 0
    plot(setup.data, main=heading, type = 'n', xlab="after this many minutes", ylab="number of species found")
    
    
    # plot random at dawn, with standard deviations
    if (length(random.at.dawn$mean) > 0) {
        par(col = 'green')
        poly.y <- c(random.at.dawn$mean + random.at.dawn$sd, rev(random.at.dawn$mean - random.at.dawn$sd))
        poly.x <- c(1:length(random.at.dawn$mean), length(random.at.dawn$mean):1)
        
        polygon(poly.x, poly.y,  col = 'honeydew1', border = 'honeydew2')
        points(random.at.dawn$mean, type='l')
    }
    
    # plot optimal
    par(col = 'blue')
    points(optimal, type='l')
    
    
    # plot each of the ranking results
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
    # returns the count of new species given a list
    # of species vectors
    # 
    # Args: 
    #   species.in.each.sample: list
    #   order: vector; the minute ids in the order they should appear in the progression
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

