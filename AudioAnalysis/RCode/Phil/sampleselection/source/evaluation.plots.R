g.plot.path <- '/Users/n8933464/Documents/sample_selection_output/plots'



PlotProgressionSummary <- function (days, fn = NULL, method = 1) {
    # given a list of evaluation results, including random at dawn and some ranking methods 
    # calculates the average improvement over random at dawn
    # 
    # Args
    #   days; list: the evaluation results from many days.
    #   fn: string; where to save it. If ommitted, will output to screen.
    #   method: 1 or 2;  percent over RAD should be relative to itself (1) or relative to the % of total (2)
    #
    # Details:
    #   The format for days is a list of days. Each value (day) is a data frame of species progressions (1 per column),
    #   The columns of each day's data frame must be the same. 
    #   For each ranking method in each day, the difference from random.at.dawn is calculated. 
    #   This difference is averages across days for each ranking method. 
    #   A plot of the average difference is drawn for each ranking method
    
  
    if (!is.null(fn)) {
        fn <- file.path(g.plot.path, fn)
        png(filename = fn,
            width = 1400, height = 700, units = "px", pointsize = 13,
            bg = "white",  res = NA,
            type = c("cairo", "cairo-png", "Xlib", "quartz"))
    }
    
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
            
            if (method == 1) {
                r.a.d <- days[[d]][['random.at.dawn']]$mean
                ranked <- days[[d]][[cur.to.score]]
                percent.improvement <- ((ranked / r.a.d) - 1) * 100  
            } else {
                # method 2
                total.num.species <- max(days[[d]][['optimal']])
                rad.percent <- r.a.d / total.num.species
                ranked.percent <- ranked / total.num.species
                percent.improvement <- (ranked.percent - rad.percent) * 100
            }

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
    par(mar=c(5, 4, 4, 5) + 0.1, cex=1.8, col = 'black')
    colors <- sapply(to.score, GetColorForName)
    line.styles <- sapply(to.score, GetLineStyleName)
    pch <- sapply(to.score, GetPch)
    heading <- "Average improvement over random sampling at dawn"
    plot(setup.data, main=heading, type = 'n', xlab="After this many minutes", ylab="mean % improvement over random at dawn")
    for (i in 1:length(to.score)) {              
        line <- results[[i]]
        sd <- NA  
        PlotLine(line, sd = sd, col.rgb = colors[,i], lty = line.styles[i], pch = pch[i])
    }
    
    legend.names <- sapply(to.score, GetRankingLegendName)
    legend.cols <- apply(colors, 2, RgbCol)
    legend("bottomright",  legend = legend.names, 
           col = legend.cols, 
           pch = pch,
           lty = 'solid', text.col = "black", lwd = 2, cex=1.4)
    
    par(col = 'black')
    points(rep(0, 1440), type='l', lty = 'dotted', lwd = 2)

    
    if (!is.null(fn)) {
        dev.off()   
    }
    
    return(results)
    
    
    
}

PlotProgressions <- function (progressions, 
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
    
    # !! temp ... For some reason saving to file doesn't include the legend
    fn <- NULL
    
    if (!is.null(fn)) {
        png(filename = fn,
            width = 800, height = 600, units = "px", pointsize = 13,
            bg = "white",  res = NA)
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
    par(mar=c(4, 4, 3, 4) - 0.1,    # margin
        cex=1.5)                    #font size
    plot(setup.data, main=heading, type = 'n', xlab="After this many minute samples", ylab="")
    mtext("Number of species found", side=2, line=2.1, cex=1.5)
    
    percent.ticks.at <- (0:5)/5
    axis(4, at=percent.ticks.at*plot.height, labels=round(percent.ticks.at*100))
    mtext("% of total", side=4, line=2.1, cex=1.5)
    par(cex=1.2)
    
    # whether each of the progressions is a comparison curve or the result of 
    # our smart sampling
    is.comparison.curve <- sapply(names(progressions), function (name) {
        to.match <- c('random', 'optimal', 'indices')
        return(length(grep(paste(to.match, collapse = "|"), name)) > 0)
    })
    
    colors <- sapply(names(progressions), GetColorForName)
    line.styles <- sapply(names(progressions), GetLineStyleName)
    pch <- sapply(names(progressions), GetPch)
    
    p.names <- names(progressions)
    for (i in 1:length(p.names)) {              
        if (is.list(progressions[[i]])) {
            line <- progressions[[i]]$mean
            sd <-  progressions[[i]]$sd
        } else {
            line <- progressions[[i]]
            sd <- NA
        }             
        PlotLine(line, sd = sd, col.rgb = colors[,i], lty = line.styles[i], pch = pch[i])
    }
    
    legend.cols <- apply(colors, 2, RgbCol)
    
    legend.names <- p.names
    legend.names[!is.comparison.curve] <- sapply(legend.names[!is.comparison.curve], GetRankingLegendName)
    
    # todo: figure out why this is not working when saving plot
    legend("bottomright",  legend = legend.names, 
           col = legend.cols, 
           pch = pch,
           lty = line.styles, text.col = "black", lwd = 1, cex=1)
    
    if (!is.null(fn)) {
        dev.off()   
    }
    return(true)
    
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
        'c.4' = c(0.8,0.2,0.2),
        'c.5' = c(0,0.2,0.8),
        'c.6' = c(0.25,0.6,0.0),
        'c.7' = c(0.25,0.6,0.2)
    )
    if (name %in% names(colors.1)) {
        return(colors.1[[name]])
    } else {
        return(colors.2[[MapColor(name)]])
    }   
}

GetLineStyleName <- function (name) {
     # http://students.washington.edu/mclarkso/documents/line%20styles%20Ver2.pdf
    styles.1 <- list(
        'optimal' = 'longdash',
        'random.at.dawn' = 'dotted',
        'random.all' = 'dotdash',
        'indices' = 'dashed'
    )
    styles.2 <- list(
        'c.1' = 'solid',
        'c.2' = 'solid',
        'c.3' = 'solid',
        'c.4' = 'solid',
        'c.5' = 'solid',
        'c.6' = 'solid',
        'c.7' = 'solid'
    )
    if (name %in% names(styles.1)) {
        return(styles.1[[name]])
    } else {
        return(styles.2[[MapColor(name)]])
    }   
}

GetPch <- function (name) {
    # the symbol used on the line, so we don't rely only on colours
    # http://www.statmethods.net/advgraphs/parameters.html
    styles.1 <- list(
        'optimal' = NA,
        'random.at.dawn' = NA,
        'random.all' = NA,
        'indices' = NA
    )
    styles.2 <- list(
        'c.1' = 15,
        'c.2' = 16,
        'c.3' = 17,
        'c.4' = 18,
        'c.5' = 19,
        'c.6' = 20,
        'c.7' = 15
    )
    if (name %in% names(styles.1)) {
        return(styles.1[[name]])
    } else {
        return(styles.2[[MapColor(name)]])
    }   
}


MapColor <- function (rank.name) {
    # maps ranking method names to colour names
    # this can change between papers, but keep consistent within a paper
    map <- list('X4' = 'c.1', 'X8' = 'c.2', 'X5' = 'c.3', 'X6' = 'c.4', 'X9' = 'c.5', 'X10' = 'c.6', 'X11' = 'c.7')
    return(map[[rank.name]]) 
}



GetRankingLegendName <- function (ranking.code) {
    codes <- list(
        "X4" = "EC",
        "X8" = "EC.TD",
        "X5" = "Ranked by clusters (δ = 0.1)",
        "X6" = "Ranked by clusters (δ = 1)",
        "X9" = "Ranked by clusters (δ = 0.1) with Temporal Dispersal",
        "X10" = "Ranked by clusters (δ = 1) with Temporal Dispersal",
        "X11" = "Ranked by clusters (δ = 0.6) with Temporal Dispersal"
    )
    
    if (ranking.code %in% names(codes)) {
        return(codes[[ranking.code]])
    } else {
        return(ranking.code)
    }
}




PlotLine <- function (line, col.rgb, sd = NA, sd.col = NA, lty = 'solid', pch = 6) {
    
    if (is.numeric(sd)) {
        poly.y <- c(line + sd, rev(line - sd))
        poly.x <- c(1:length(line), length(line):1)
        fill.col <- rgb(col.rgb[1], col.rgb[2], col.rgb[3], 0.1)
        polygon(poly.x, poly.y,  col = fill.col, border = fill.col)        
    }
    line.col <- rgb(col.rgb[1], col.rgb[2], col.rgb[3], 1)
    par(col = line.col)
    points(line, type='l', lty = lty, lwd = 3)
    if (!is.null(pch)) {
        every <- 10
        points.x <- seq(every,length(line),every)
        points.y <- line[points.x]
        
        points(x = points.x, y = points.y, pch = pch, cex=2)    
    }

}



PlotProgressions3d.1 <- function (progressions, cutoff = 180) {
    
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


PlotProgressions3d <- function (progressions, cutoff = 120) {
    
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


