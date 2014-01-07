ClusterEvents <- function (num.groups = 'auto', num.rows.to.use = FALSE, show.dendrogram = FALSE, method = 'complete', save = TRUE) {
    # clusters events found in g.events.path
    # based on the features found in g.features.path
    # g.features.path must have the same number of rows as g.events.path
    # Args
    #   num.groups: integer (or 'auto'). the number of clusters to use. If 'auto' will use the square root of the number of events
    #   num.rows.to.use: integer; the number of events to use. Set this to reduce the execution time during dev. FALSE will use all the events
    #   show.dendrogram: boolean; whether to plot the results of clustering in a dendrogram
    #   method: string;  which method to calculate distance between clusters.
    #   save: boolean; whether to save the results of the clustering. Will create a csv file which is the same as events.csv but with a new 'groups' column
  
  Report(4, 'reading features');
  event.features <- read.csv(OutputPath('features'), header = TRUE);
  Report(4, 'reading events');
  events <- ReadEvents();

  if (num.rows.to.use != FALSE && num.rows.to.use < nrow(event.features)) {
      Report(4, 'subsetting features');
      event.features <- event.features[1:num.rows.to.use, ];
  } else {
      num.rows.to.use <- nrow(event.features);
  }


  Report(2, 'scaling features (m = ',  num.rows.to.use, ')');
  ptm <- proc.time()
  event.features <- as.matrix(scale(event.features)) # standardize variables
  Report(2, proc.time() - ptm);
  
  Report(2, 'calculating distance matrix (m = ',  num.rows.to.use, ')');
  ptm <- proc.time()
  d <- dist(event.features, method = "euclidean") # distance matrix
  Report(3, proc.time() - ptm);
  
  if (show.dendrogram) {
      labels.for.dendrogram <- apply(events[1:num.rows.to.use, ], 1, function (r) {
        return(paste(r[2], r[3]))
      });
  }
  #get a cluster object
  Report(2, 'clustering ... (method = ',  method, ')');
  ptm <- proc.time()
  fit <- hclust(d, method=method)
  Report(3, proc.time() - ptm);
  
  if (num.groups == 'auto') {
    num.groups <- floor(sqrt(num.rows.to.use));
  }
  groups <- as.matrix(cutree(fit, num.groups));

  output <- cbind(events[1:num.rows.to.use, ], groups);
  col.names <- c(g.events.col.names, 'group');

  if (save) {
    write.table(output, file = OutputPath('clusters'), sep=',' , row.names = FALSE, col.names = col.names)
  }

  
  if (show.dendrogram) {
      # display dendogram
      plot(fit, labels = labels)
    # draw dendogram with red borders around the k clusters
    rect.hclust(fit, k=num.groups, border="red")
  }
}

TestClusterSpeed <- function () {
    # graphs the execution time of the clustering on the event-data
    
    #methods <- c("ward", "single", "complete", "average", "mcquitty", "median", "centroid");
    methods <- c("average");
    
    #set.sizes <- c(32,64,128,256,512,1024,2048,4096,8192);
    #set.sizes <- c(128,512,2048,8192);
    set.sizes <- (1:5) * 5000;
    
    cols <- c("red", "blue", "green", "orange", "black", "purple", "yellow");
    
    times1 <- matrix(rep(0, length(methods)*length(set.sizes)), ncol = length(methods), nrow = length(set.sizes));
    times2 <- times1;
    
    repeat.until <- 5;
    
    for (ss in 1:length(set.sizes)) {
      for (m in 1:length(methods)) {
        total1 <- 0;
        total2 <- 0;
        num.iterations <- 0;
        print (paste('speed test for', methods[m], set.sizes[ss]));
        while(total1 < repeat.until) {
          print (paste('test number', num.iterations+1));
          ptm <- proc.time()
          speed1 <- system.time(ClusterEvents(save = FALSE, num.rows.to.use = set.sizes[ss], method = methods[m]));
          speed2 <- proc.time() - ptm;
          #print(speed1);
          #print(speed2);
          total1 <- total1 + speed1[3];
          total2 <- total2 + speed2[3]
          num.iterations <- num.iterations + 1;
        }
        av1 <- total1 / num.iterations;
        av2 <- total2 / num.iterations;
        times1[ss,m] <- av1
        times2[ss,m] <- av2
      }
    }

    #temp
    #times <- matrix(c(2.147,2.133,2.151,2.174,2.16,2.141), nrow = 3, byrow = FALSE)
    
    times1.comb <- cbind(set.sizes, times1);
    colnames(times1.comb) <- c('m', methods);
    print(times1.comb);
    
    times2.comb <- cbind(set.sizes, times2);
    colnames(times2.comb) <- c('m', methods);
    print(times2.comb);
    filename <- paste(c('clusterspeedtest',methods,min(set.sizes),max(set.sizes),length(set.sizes)), collapse = '.');
    write.csv(times1.comb, paste0(g.output.path, filename, '.csv'));
    
    ymax = max(times1);
    ymin = min(times1);
    xmax = max(set.sizes);
    xmin = min(set.sizes);

    plot(set.sizes,times1[,1], type="l",col=cols[1], xlim = c(xmin, xmax), ylim = c(ymin, ymax), xlab = 'number of samples', ylab = 'execution time (s)');
    if (length(methods) > 1) {
      for (i in 2:length(methods)) {
        lines(set.sizes, times1[,i],col=cols[i]);
      }
    }
    
    legend(
      x = "topleft",
      legend = methods,
      lty= c(1,1),
      lwd=c(1,1),
      col=cols
    
    );
    

    
    
    
}

testing <- function () {
    methods <- c("ward", "mcquitty");
    cols <- c("red", "blue");
    set.sizes <- (1:3) * 100;
    #example test data
    times <- matrix(c(2.147,2.133,2.151,2.174,2.16,2.141), nrow = 3, byrow = FALSE)
    ymax = max(times);
    ymin = min(times);
    xmax = max(set.sizes);
    xmin = min(set.sizes);
    plot(
      set.sizes,
      times[,1],
      type="l",
      col=cols[1],
      xlim = c(xmin, xmax),
      ylim = c(ymin, ymax),
      xlab = 'number of samples',
      ylab = 'execution time (s)');
      
    for (i in 2:length(methods)) {
        lines(set.sizes, times[,i],col=cols[i]);
    }
    
    legend(
      x = "topright",
      legend = methods,
      lty= c(1,1),
      lwd=c(2.5,2.5),
      col=cols
    );
}



DrawClustersOld <- function (num.clusters = 3, use.random = FALSE) {
    
  source('spectrogram.R');
  source('util.R');
  library('tuneR');
  
  events.per.cluster <- 10;

# read events incluing cluster info
    events <- read.csv(g.clusters.path, stringsAsFactors=FALSE);
    
    # the list of events probably contains many from the same audio file.
    # we store the spectogram of the audio file of the current event,
    # and change it when the current event is from a different audio file.
    # this is OK since the events should be in chronological order, meaning that
    # the audio files should be in order. This is inefficient if events from different
    # clusters are from the same audio file, because the spectrogram of that audio file will be
    # generated for each event.
    cur.wav.path <- FALSE;
    cur.spectro <- FALSE;
    
    if (use.random) {
      # random sample of clusters
      all.clusters <- unique(events$groups);
      clusters <- sample(1:length(all.clusters), num.clusters, replace=F);
    } else {
      clusters <- 1:num.clusters;
      clusters <- clusters + 10;
    }
    
    selection = list();

    # for each cluster, get all the events from that cluster
    for (i in 1:num.clusters) {
      selector <- events$groups %in% clusters[i];
      examples <- events[selector, ];
      cluster <- list(cluster.num = clusters[i], examples = examples);
      selection[[i]] <- cluster;
      
    }
    

    # for each event in the cluster, create a mini-spectrogram
    for(s in 1:length(selection)) {
        cluster.events <- selection[[s]]$examples;
        for (ev in 1:nrow(cluster.events)) {
            bounds <- as.numeric(as.vector(cluster.events[ev,6:9]));
            # use the whole frequency range
            bounds[3] <- FALSE;
            bounds[4] <- FALSE;
            g.wav.path = paste(c(g.audio.dir, cluster.events[ev,2], '.wav'), collapse = '');
            if (wav.path != cur.wav.path) {
                cur.spectro <- Sp.create(wav.path, draw=FALSE);
                cur.wav.path <- wav.path;
            }
            event.vals <- SliceStft(bounds, cur.spectro);
            
            #
            
            blank <- matrix(rep(0, nrow(cur.spectro$vals))*3, nrow = nrow(cur.spectro$vals), ncol = 2);
            
            if (!exists("stitched")) {
                stitched <- blank;
            }
            
            stitched <- cbind(stitched, event.vals, blank);
        }
        
        divider <- matrix(rep(1, nrow(cur.spectro$vals)), nrow = nrow(cur.spectro$vals), ncol = 1);
       
        stitched <- cbind(stitched, divider, blank);
        
    }
    

    
    image(t(stitched));
    

}




