DistScores <- function (from.min.ids, to.closest.in.min.ids) {
    # for each of from.min.ids, finds the distance in time to the closest out of the mins in to.closest.min.ids
    # currently, distance is measured by min.id which only works if min.ids are consecutive.  eg, 1-day recording of 1440 mins 
    # will work for multiple days, as long as min ids are consecutive from midnight to midnight (because the transition is during a low activity time)
    dist.scores <- sapply(from.min.ids, function (min.id) { 
        return(min(abs(to.closest.in.min.ids - min.id), na.rm = TRUE)) 
    }) 
    return(dist.scores) 
}


TransformDistScores <- function (dist.scores, threshold = 30, amount = 1) {
    # given a list of distances scores, transforms to a value that 
    # can be used as a weight for score based on something else
    # 
    # Args: 
    #   threshold: int; up to this point, the distance makes a difference. Over this point, 
    #                   the weight will be equal to the weight of 
    #                   a distance score of 60
    #   amount: float [0,1] how much the distance score should affect the weight 
    
    
    dist.scores[dist.scores > threshold] <- threshold # above threshold value is equal to threshold
    dist.scores <- ((threshold^2-((dist.scores-threshold)^2))^0.5)
    dist.scores <- dist.scores / threshold # convert to [0:1]  (maybe use max instead of threshold? but don't think so)
    dist.scores <- dist.scores * amount + (1-amount)
    return(dist.scores)
    
}


PlotFunction <- function ( threshold = 30, amount = 1, range = 50) {
    # for illustration purposes, plots the temporal dispersal score transform function
    
    orig <- 0:range
    transformed <- TransformDistScores(orig, threshold = threshold, amount = amount)
    print(transformed)
    
    par(col = 'black')
    heading <- ""
    plot.height = 1.2
    setup.data <- transformed 
    setup.data[c(3,4)] <- c(0,plot.height)

    par(mar=c(4, 4, 2, 2) + 0.1,    # margin
        cex=1.1)                    #font size
    plot(x = orig, y = setup.data, main=heading, type = 'n', xlab="Distance to nearest ranked sample (minutes)", ylab="score multiplier")
    
    points(x = orig, y = transformed, type='l', lty = 'dashed', lwd = 3)
    
    
}


