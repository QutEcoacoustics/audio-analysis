ClusterEvents <- function (num.groups = 'auto', 
                           num.rows.to.use = FALSE, 
                           save.dendrogram = FALSE, 
                           method = 'complete', 
                           save = TRUE) {
    # clusters events found in g.events.path
    # based on the features found in g.features.path
    # g.features.path must have the same number of rows as g.events.path
    #
    # Args:
    #   num.groups: integer (or 'auto'). The number of clusters to use. 
    #     If 'auto' will use the square root of the number of events
    #   num.rows.to.use: integer; The number of events to use. 
    #     Set this to reduce the execution time during dev. 
    #     FALSE will use all the events
    #   show.dendrogram: boolean; 
    #     Whether to plot the results of clustering in a dendrogram
    #   method: string;  
    #     Which method to calculate distance between clusters.
    #   save: boolean; Whether to save the clustering results. 
    #     Will create a csv file which is the same as 
    #     events.csv but with a new 'groups' column
    
    

    vals <- GetEventsAndFeatures()
    event.features <- vals$event.features
    events <- vals$events
    
    if (num.rows.to.use != FALSE && num.rows.to.use < nrow(event.features)) {
        Report(4, 'subsetting features')
        event.features <- event.features[1:num.rows.to.use, ]
        events <- num.rows.to.use
    } else {
        num.rows.to.use <- nrow(event.features)
    }
    
    
    Report(2, 'scaling features (m = ',  num.rows.to.use, ')')
    ptm <- proc.time()
    event.features <- as.matrix(scale(event.features))  # standardize variables
    Timer(ptm, 'scaling features')
    
    Report(2, 'calculating distance matrix (m = ',  num.rows.to.use, 'n = ', ncol(event.features),')')
    
    ptm <- proc.time()
    d <- dist(event.features, method = "euclidean")  # distance matrix
    Timer(ptm, 'distance matrix')
    
    
    if (save.dendrogram) { 
        labels.for.dendrogram <- EventLabels(events[1:num.rows.to.use, ])
    }
    #get a cluster object
    Report(2, 'clustering ... (method = ',  method, ')')
    ptm <- proc.time()
    fit <- hclust(d, method=method)
    Timer(ptm, 'clustering')
    
    if (num.groups == 'auto') {
        num.groups <- floor(sqrt(num.rows.to.use))
    }
    groups <- as.matrix(cutree(fit, num.groups))
    Report(2, 'num cluster groups = ',  num.groups)
    
    events$group <- groups

    
    if (save) {
        WriteOutput(events, 'clusters')
    }
    
    
    if (save.dendrogram) {
        library('pvclust')
        # display dendogram
        img.path <- OutputPath('cluster_dendrogram', ext = 'png');
        Report(5, 'saving dendrogram')
        png(img.path, width = 30000, height = 20000)
        Dot()
        plot(fit, labels = labels.for.dendrogram)
        # draw dendogram with red borders around the k clusters
        rect.hclust(fit, k=num.groups, border="red")
        #pvrect(fit, alpha=.95)
        dev.off()
    }
}


GetEventsAndFeatures <- function () {
    event.features <-  ReadOutput('features')
    events <- ReadOutput('events')
    
    #todo: ensure that both are sorted by event id
    events <- events[with(events, order(event.id)) ,]
    event.features <- event.features[with(event.features, order(event.id)) ,]
    
    # remove event.id.column from features table
    drop.cols <- names(event.features) %in% c('event.id')
    event.features <- event.features[!drop.cols]
    
    return (list(events = events, event.features = event.features))
    
}


InternalMinuteDistances <- function () {
    vals <- GetEventsAndFeatures()
    mins <- ReadOutput('minlist')
    vals$event.features <- as.matrix(scale(vals$event.features))  # standardize variables
    dist.scores <- sapply(1:nrow(mins), InternalMinuteDistance, vals$event.features, vals$events);
    mins$v.score <- dist.scores
    WriteOutput(mins, 'minlist')
}


InternalMinuteDistance <- function (min.id, features, events) {
    # get the relevant event ids for this min
    rows <- events$min.id == min.id
    #event.ids <- events$event.id[]
    # get the relevant feature vectors
    features <- features[rows, ]
    d <- dist(features, method = "euclidean")  # distance matrix
    return(sum(d))
}










TestClusterSpeed <- function () {
    # graphs the execution time of the clustering on the event-data
    
    #methods <- c("ward", "single", "complete", 
    #             "average", "mcquitty", "median", "centroid")
    methods <- c("average")
    
    #set.sizes <- c(32,64,128,256,512,1024,2048,4096,8192)
    #set.sizes <- c(128,512,2048,8192)
    set.sizes <- (1:5) * 5000
    
    cols <- c("red", "blue", "green", "orange", "black", "purple", "yellow")
    
    times1 <- matrix(rep(0, length(methods) * length(set.sizes)), 
                     ncol = length(methods), nrow = length(set.sizes))
    times2 <- times1
    
    repeat.until <- 5
    
    for (ss in 1:length(set.sizes)) {
        for (m in 1:length(methods)) {
            total1 <- 0
            total2 <- 0
            num.iterations <- 0
            print (paste('speed test for', methods[m], set.sizes[ss]))
            while(total1 < repeat.until) {
                print (paste('test number', num.iterations + 1))
                ptm <- proc.time()
                speed1 <- system.time(
                    ClusterEvents(save = FALSE, 
                                  num.rows.to.use = set.sizes[ss], 
                                  method = methods[m]))
                speed2 <- proc.time() - ptm
                #print(speed1)
                #print(speed2)
                total1 <- total1 + speed1[3]
                total2 <- total2 + speed2[3]
                num.iterations <- num.iterations + 1
            }
            av1 <- total1 / num.iterations
            av2 <- total2 / num.iterations
            times1[ss,m] <- av1
            times2[ss,m] <- av2
        }
    }
    
    #temp
    #times <- matrix(c(2.147,2.133,2.151,2.174,2.16,2.141), 
    #                nrow = 3, byrow = FALSE)
    
    times1.comb <- cbind(set.sizes, times1)
    colnames(times1.comb) <- c('m', methods)
    print(times1.comb)
    
    times2.comb <- cbind(set.sizes, times2)
    colnames(times2.comb) <- c('m', methods)
    print(times2.comb)
    filename <- paste(c('clusterspeedtest',
                        methods,
                        min(set.sizes),
                        max(set.sizes),
                        length(set.sizes)), collapse = '.')
    write.csv(times1.comb, paste0(g.output.path, filename, '.csv'))
    
    ymax <- max(times1)
    ymin <- min(times1)
    xmax <- max(set.sizes)
    xmin <- min(set.sizes)
    
    plot(set.sizes,times1[,1], type="l",col=cols[1], 
         xlim = c(xmin, xmax), ylim = c(ymin, ymax), 
         xlab = 'number of samples', ylab = 'execution time (s)')
    if (length(methods) > 1) {
        for (i in 2:length(methods)) {
            lines(set.sizes, times1[,i],col=cols[i])
        }
    }
    
    legend(
        x = "topleft",
        legend = methods,
        lty= c(1,1),
        lwd=c(1,1),
        col=cols
        
    )
    
    
    
    
    
}

testing <- function () {
    methods <- c("ward", "mcquitty")
    cols <- c("red", "blue")
    set.sizes <- (1:3) * 100
    #example test data
    times <- matrix(c(2.147,2.133,2.151,2.174,2.16,2.141), 
                    nrow = 3, byrow = FALSE)
    ymax <- max(times)
    ymin <- min(times)
    xmax <- max(set.sizes)
    xmin <- min(set.sizes)
    plot(set.sizes,
         times[ , 1],
         type="l",
         col=cols[1],
         xlim = c(xmin, xmax),
         ylim = c(ymin, ymax),
         xlab = 'number of samples',
         ylab = 'execution time (s)')
    
    for (i in 2:length(methods)) {
        lines(set.sizes, times[ , i],col=cols[i])
    }
    
    legend(
        x = "topright",
        legend = methods,
        lty= c(1,1),
        lwd=c(2.5,2.5),
        col=cols
    )
}







