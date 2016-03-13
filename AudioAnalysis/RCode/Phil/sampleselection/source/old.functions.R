# This file contains functions that have been replaced but might need to be referred to later, so moved out of the way to here



PlotProgressions.old <- function (progressions, 
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


EvaluateSamples2d.old <- function (ranks, cutoff = NA) {
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
    
    PlotProgressions(ranked.progressions = ranked.progressions,
                      optimal = optimal$found.species.count.progression, 
                      random.at.dawn = random.at.dawn, 
                      random.all = random.all)
    
}
