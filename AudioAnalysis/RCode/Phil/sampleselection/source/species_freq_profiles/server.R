library(shiny)

species.profiles <- read.csv(file.path(GetPathToCsv(),'dists.csv'))
species.tfs <- read.csv(file.path(GetPathToCsv(), 'tfs.csv'))
species.bfs <- read.csv(file.path(GetPathToCsv(), 'bfs.csv'))
species <- GetSpeciesListFromFile()

species$num.annotations <- as.numeric(apply(species.tfs, 2, sum))

GetColors <- function (n, a = 1) {
    return(rainbow(n, alpha = a))
}

shinyServer(function(input, output) {
    
    # Expression that generates a histogram. The expression is
    # wrapped in a call to renderPlot to indicate that:
    #
    #  1) It is "reactive" and therefore should re-execute automatically
    #     when inputs change
    #  2) Its output type is a plot
    
    
        output$distPlot <- renderPlot({       
            # draw the plot with the specified species
            if (length(input$sp.id) >= 1) {
                PlotProfiles(input$sp.id, GetColors(length(input$sp.id)))
            }
        })
        
        
        
        output$hms <- renderPlot({
            if (length(input$sp.id) >= 1) {
                PlotHms(input$sp.id)
            }
        })
    
    
})

PlotHms <- function (sp.ids) {
    
    by <- 1/length(sp.ids)
    v.pos <- seq(0,1,by)
    
    colors <- GetColors(length(sp.ids), 0.7)
    
    
    for (i in 1:length(sp.ids)) {
        par(fig=c(0,1,v.pos[i],v.pos[i+1]), new=TRUE)
        par(mar = c(0,6,0,2), new=TRUE)
        PlotBoundary(sp.ids[i], colors[i]) 
    }  
}


PlotProfiles <- function (sp.ids, colors) {
    # Plots the profiles for all the specified species
    #
    # Args:
    #   sp.ids: vector of strings or ints
    
    if (length(sp.ids) > 0) {
        labels1 <- species$name[species$id %in% sp.ids]   
        labels2 <- species$num.annotations[species$id %in% sp.ids]
        labels <- paste0(labels1, ' (', labels2, ')')
        
        selected.cols <- paste0('X',sp.ids)
        cols    <- as.data.frame(species.profiles[, selected.cols])  # profile  
        colnames(cols) <- sp.ids
        PlotSpectrum(levels = cols, labels = labels, colors = colors) 
    }
}


PlotSpectrum <- function (levels, labels, colors, range = 11050) {
    # Plots some spectrums
    # 
    # Args:
    #   levels: matrix; each column is a spectrum to plot. Each row is a frequency bin
    #   labels: vector of strings; length of labels must equal ncol(levels)
    #   range: the frequency range in hz
    
    nbins <- nrow(levels)
    xlab <- 1:nbins * (range/nbins)
    # set up empty plot
    empty.x <- c(0,range)
    empty.y <- c(0,1)
    plot(empty.x, empty.y, type = "n", xlab = "Frequency (Hz)", ylab = "Level") 
    
    for (s in 1:ncol(levels)) {
        #level <- levels[,s] * 1/sum(levels[,s])  # normalize so total adds to 1
        # normalize so max is 1
        level <- Normalize(levels[,s]) 
        lines(xlab, level, type = "l", lwd = 5, xlab = "Frequency (Hz)", ylab = "Level", col = colors[s])    
    }
    legend("topright",  legend = labels, col = colors, text.col = "black", lwd = 2)
}


PlotBoundary <- function (sp.id, col) {
    # draws a heatmap (1d) of the frequencies where the annotation boundaries lie
    # 
    # Args:
    #   sp.id: int; the species id
    #   col: string; The color to use. Will generate a gradient from black to that color
    height <- 3
    v <- species.tfs[,paste0('X',sp.id)] + species.bfs[,paste0('X',sp.id)] 
    m <- matrix(rep(v, height), ncol = height)
    palette <- colorRampPalette(c('black', col))(12)    
    image(m, col = palette, useRaster = TRUE,  xaxt='n',  yaxt='n')
}


