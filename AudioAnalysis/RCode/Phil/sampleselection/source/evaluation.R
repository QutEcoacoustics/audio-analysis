EvaluateSamples <- function () {
    
    SetOutputPath(level = 2, allow.new = FALSE) # for reading
    SetOutputPath(level = 3) # for writing
    
    ranks <- ReadObject('ranked_samples', level = 2)
    d.names <- dimnames(ranks)
    num.clusters.options <- d.names$num.clusters
    default <- length(num.clusters.options) / 2
    num.clusters.choices <- GetMultiUserchoice(num.clusters.options, 'num clusters', default = default)
    
    ranks <- ranks[,num.clusters.choices,]
    
    if (length(num.clusters.choices) > 1) {
        # more than 2 dimensions to graph, so make 3d plot
        EvaluateSamples3d(ranks)
    } else {
        EvaluateSamples2d.2(ranks)  
    }

}

EvaluateSamples2d <- function (ranks, add.dawn = TRUE, cutoff = NA) {
    # given a list of minutes
    # simulates a species richness survey, noting the 
    # total species found after each minute, and the total
    # number of species found after each minute
    
    #!! target only = false because of this hack
    speciesmins <- GetTags(target.only = FALSE);  # get tags within outer target 
    ranked.min.ids <- ReadOutput('target.min.ids', level = 0)
    
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
        ordered.min.ids <- ranked.min.ids$min.id[order(ranks[i,])]
        
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

EvaluateSamples2d.2 <- function (ranks, add.dawn = TRUE, cutoff = NA) {
    # given a list of minutes
    # simulates a species richness survey, noting the 
    # total species found after each minute, and the total
    # number of species found after each minute
    

    # if 'add dawn' equals true, we are prepending some dawn minutes to ranked minutes which appear outside of dawn
    # we therefore comparing our method with the whole day
    speciesmins <- GetTags(target.only = !add.dawn);  # get tags within outer target 
    
    # these min ids have been ranked
    ranked.min.ids <- ReadOutput('target.min.ids', level = 0)
    
    # hacked in for escience paper
    if (add.dawn) {
        # if add dawn, we are now comparing the whole day
        mins.for.comparison <- GetMinuteList()
        mins.for.comparison <- mins.for.comparison[mins.for.comparison$site == "NW" & mins.for.comparison$date == "2010-10-13",]
    } else {
        mins.for.comparison <- ranked.min.ids
    }

    
    
    # temp for escience paper
    if (add.dawn) {
        dawn <- GetDawnMins()
        if (length(intersect(dawn$min.id, ranked.min.ids$min.id)) > 0) {
            stop('cant add dawn if dawn is already in the target')
        }
    }
    
    species.in.each.sample <- ListSpeciesInEachMinute(speciesmins, mins.for.comparison)
    
    ranked.progressions <- list()
    ranked.count.progressions <- list()
    dn <- dimnames(ranks)
    methods <- dn$ranking.method
    i <- 1
    while (i <= length(methods)) {
        method <- methods[i]
        ordered.min.ids <- ranked.min.ids$min.id[order(ranks[i,])]
        
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
            WriteRichnessResults(ordered.min.ids, ranked.progressions[[method]], method, species.in.each.sample)
            ranked.count.progressions[[method]] <- ranked.progressions[[method]]$count
        }
        
        
        i <- i + 1
    }
    
    optimal <- OptimalSamples(speciesmins, mins = mins.for.comparison$min.id, species.in.each.min = species.in.each.sample)
    
    if (IsDawn(mins.for.comparison$min)) {
        random.at.dawn <- RandomSamples(speciesmins = speciesmins, species.in.each.sample, mins.for.comparison$min.id, dawn.first = TRUE)   
    } else {
        random.at.dawn <- NA
    }
    
    
    random.all <- RandomSamples(speciesmins = speciesmins, species.in.each.sample, mins.for.comparison$min.id, dawn.first = FALSE)
    #from.indices <- IndicesProgression(species.in.each.sample, min.ids)
    
    GraphProgressions(ranked.count.progressions = ranked.count.progressions,
                      optimal = optimal$found.species.count.progression, 
                      random.at.dawn = random.at.dawn, 
                      random.all = random.all)
    
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

OptimalSamples <- function (speciesmins = NA, mins = NA, species.in.each.min = NA) {
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

GraphProgressions <- function (ranked.count.progressions, optimal, random.at.dawn = NA, random.all = NA, from.indices = NA, cutoff = 180) {
   
    rank.names <- names(ranked.count.progressions);
    
    # truncate all output at cuttoff
    if (!is.na(cutoff)) {
        optimal <- Truncate(optimal, cutoff)
        if (!is.na(from.indices)) {
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
    }
    
    #plot them against each other
    legend.names = c()
    
    par(col = 'black')
    heading <- "Species count progressions"
    
    # setup using optimal for the y axis, since it will have all the species
    setup.data <- rep(max(optimal), plot.width)
    setup.data[1] <- 0
    plot(setup.data, main=heading, type = 'n', xlab="After this many minutes", ylab="Number of species found")
    
    
    # plot each of the ranking results
    line.colours <- matrix(NA, ncol = 3, nrow = 0)

    
    # plot random at dawn, with standard deviations
    # colours : http://www.stat.columbia.edu/~tzheng/files/Rcolor.pdf
    if (is.list(random.at.dawn) && length(random.at.dawn$mean) > 0) {
        col <- c(0,1,0)
        line.colours <- rbind(line.colours, col)
        PlotLine(line = random.at.dawn$mean, col.rgb = col, sd = random.at.dawn$sd, lty = 'dashed')
        legend.names <- c(legend.names, "Random at dawn")

    }
    
    # plot random, with standard deviations
    if (is.list(random.all) && length(random.all$mean) > 0) {
        col <- c(0.9,0.6,0.2)
        PlotLine(line = random.all$mean, col.rgb = col, sd = random.all$sd, lty = 'dashed')
        legend.names <- c(legend.names, "Random throughout")
        line.colours <- rbind(line.colours, col)
    }
    
    # plot optimal
    col <- c(0.9,0.6,0.2)
    legend.names <- c(legend.names, "Optimal sampling")
    line.colours <- rbind(line.colours, col)
    PlotLine(optimal, col.rgb = col, lty = 'dashed')
    
    # plot from indices
    # !! if this is activated, colours will be wong
    if (!is.na(from.indices)) {
        col <- c(.3, .6, .6)  # dark pink
        legend.names <- c(legend.names, "indices")
        PlotLine(optimal, col.rgb = col, lty = 'dashed')
        line.colours <- rbind(line.colours, col)
    }
    
    ranking.method.names <- c(
        "Event count only",
        "Number of cluster groups A",
        "Number of cluster groups B"
        )
    
    
    
    ranking.method.colours <- matrix(c(1,0.3,0,
                                       0,0.3,1,
                                       0.7,0,0.7), ncol = 3, byrow = TRUE)
    
    line.colours <- rbind(line.colours, ranking.method.colours)
    
    for (i in 1:length(rank.names)) {
        
        if (is.list(ranked.count.progressions[[i]])) {
            line <- ranked.count.progressions[[i]]$mean
            sd <-  ranked.count.progressions[[i]]$sd
        } else {
            line <- ranked.count.progressions[[i]]
            sd <- NA
        }
        
        PlotLine(line, ranking.method.colours[i, ])

        legend.names = c(legend.names, ranking.method.names[i])
    }
    
    legend.cols <- apply(as.matrix(line.colours), 1, RgbCol)
    
    
    lty <- c(rep('dashed', length(legend.cols) - length(rank.names)), rep('solid', length(rank.names)))
    
    legend("bottomright",  legend = legend.names, 
           col = legend.cols, 
           lty = lty, text.col = "black")
    
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
    points(line, type='l', lty = lty)
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
    
    WriteOutput(output, output.fn, level = 3)
    
    
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





