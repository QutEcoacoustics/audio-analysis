# plot some examples of the function used for temporal spreading

PlotExamplesOfTemporalSpreading <- function (thresholds = c(30, 20), amounts = c(1, 0.4), width = 100) {
    
    distance.scores <- 0:width
    
    ltys <- 2:6
    lwd <- 2
    
    tds <-  TransformDistScores(distance.scores, threshold = thresholds[1], amount = amounts[1])
    plot(distance.scores, tds, type = 'l', xlab = "distance (mins)", ylab = "weight", lty = ltys[1], lwd = lwd)
    legend.names <- paste('threshold =',thresholds[1],",  amount = ", amounts[1])
    
    for (i in 2:length(thresholds)) {
        tds <-  TransformDistScores(distance.scores, threshold = thresholds[i], amount = amounts[i])
        points(distance.scores, tds, type='l', lty = ltys[i], lwd = lwd)
        legend.names <- c(legend.names, paste('threshold =',thresholds[i],",  amount = ", amounts[i]))
        
    }

    legend("bottomright",  legend = legend.names, 
           lty = ltys[1:length(legend.names)], text.col = "black", lwd = 2)
    
}