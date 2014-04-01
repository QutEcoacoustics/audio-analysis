
InspectClusters <- function (cluster.groups = NA, duration.per.group = 30, max.groups = 50) {
    #for each of the given cluster groups, will append randomly select duration.per.group
    # worth of events from that cluster group and append the spectrograms of each event
    # side by side, with the event box shown. 
    # all the cluster groups will be appended one under the other. 
    
    events <- ReadOutput('clusters')
    
    if (is.numeric(cluster.groups)) {
        events <- events[events$group %in% cluster.groups  ,]
    }
    # just incase the argument contains non-existent groups
    cluster.groups <- unique(events$group)  
    # make sure max is not exceeded
    if(length(cluster.groups) > max.groups) {
        removed <- cluster.groups[(max.groups + 1):length(cluster.groups)]
        cluster.groups <- cluster.groups[1:max.groups]
        Report(3, 'Number of groups to render was greater than max.groups ... Ignoring the following groups:', paste(removed, collapse = ','))
    }
    # incase max was exceeded, the cluster groups will have changed
    if (is.numeric(cluster.groups)) {
        events <- events[events$group %in% cluster.groups  ,]
    }
    
    # colours for a particular group will be different depending on which
    # groups the user chooses to render
    events <- AssignColourToGroup(events)
    
    CreateRects
    
    # remove events so that the duration per group is at most duration.per.group
    padding = 0.2  # padding each side of the event
    
    overLimit <- function (events) {
        # determines whether there are more events of the 
        ol <- (sum(events$duration) + 2*padding*nrow(events)) > duration.per.group
        return(ol)
    }
    
    temp.dir <- TempDirectory()
    group.spectro.fns <- rep(NA, length(cluster.groups))
    
    Report(3, "about to generate spectrograms for ", nrow(events), 'from', length(cluster.groups), 'cluster groups')
    
    for (group in cluster.groups) {
        group.events <- events[which(events$group == group), ]
        num.before <- nrow(group.events)
        while(overLimit(group.events)) {
            # remove a random event from the group until the total number is less than the group duration
            random.row <- sample(1:nrow(group.events), 1)
            group.events = group.events[-random.row, ] 
        }
        num.after <- nrow(group.events)
        Report(4, 'Generating', num.after, 'spectrograms for group', group, 'out of a total of', num.before)
        this.groups.event.spectro.fns <- rep(NA, nrow(group.events))
        for (e in 1:nrow(group.events)) {
            temp.fn <- paste(group.events$event.id[e], 'png', sep = '.')
            img.path <- file.path(temp.dir, temp.fn)
            this.groups.event.spectro.fns[e] <- img.path 
            # the duration, top and bottom frequency and rect color
            # can all be taken directly from the event. The start.sec of the rect
            # needs to be from the start of the spectrogram, which in this case is the padding
            rect.borders <- group.events[e,]
            rect.borders$start.sec <- padding
            Sp.CreateTargeted(site = group.events$site[e], 
                              start.date = group.events$date[e], 
                              start.sec = group.events$min[e] * 60 + group.events$start.sec[e], 
                              duration = group.events$duration[e] + 2*padding, 
                              img.path = img.path, 
                              rects = rect.borders)
            
        }
        group.spectro.fns[group] <- file.path(temp.dir,paste0('g', group, '.png'))
        StitchImages(this.groups.event.spectro.fns, group.spectro.fns[group], vertical = FALSE)
        
        # todo: label for group number
        
    }
    
    final.image.path <- OutputPath(IntegerVectorAsString(cluster.groups), ext = 'png')
    StitchImages(group.spectro.fns, final.image.path)

    
}

IntegerVectorAsString <- function (ints, inner.sep = "-", outer.sep = ",") {
    # how much each index is different from the last
    delta <- ints[-1] - ints[-(length(ints))]
    deltaNot1 <- which(delta != 1)
    deltaNot1 <- c(0, deltaNot1, length(ints))
    name.parts <- rep(NA, length(deltaNot1)-1)
    for (i in 1:length(deltaNot1)-1) {       
        from <- ints[deltaNot1[i]+1]
        to <- ints[deltaNot1[i+1]]
        name.parts[i] <- paste0(from, inner.sep, to) 
    }
    if (!is.null(outer.sep)) {
        return(paste0(name.parts, collapse = outer.sep))    
    } else {
        return(name.parts)  
    }
}





CreateSampleSpectrograms <- function (samples, num.clusters, temp.dir) {
    # for a given set of minutes, creates minute spectrograms for each of them,
    # including events color coded by group
    # 
    # Args:
    #   samples: matrix. minute ids of the samples. columns can be used to separate
    #                    different ranking methods
    
    
    
    events <- ReadOutput('events')
    groups <- ReadOutput('clusters')
    group.col.name <- paste0('group.', num.clusters)
    
    # need to filter events by the selected minutes
    # if we want to use the full range of colours on a smaller number of groups
    rects <- CreateRects(events, groups[, group.col.name])
    # file names for later use in imagemagick command to append together
    sample.spectro.fns <- matrix(NA, nrow = nrow(samples), ncol=ncol(samples))
    
    mins.ids <- unique(as.vector(samples))


    
    fns <- SaveMinuteSpectroImages(min.ids, rects, events, temp.dir)
    
    for (i in 1:nrow(mins)) {
        # TODO: test this, it is completely untested. check that everything is working properly
        sample.spectro.fns[which(samples == mins.ids[i])] <- fns[i]
    }
    
    return(sample.spectro.fns)
    
}



InspectFeatures <- function () {
    
    min.ids <- c(405, 640)
    features <- ReadMasterOutput('rating.features')
    all.events <- ReadMasterOutput('events')
    events <- all.events[all.events$min.id %in% min.ids, ]
    features <- features[features$event.id %in% events$event.id, ]
    #check <- sum((events$event.id == features$event.id) * 1) == length(events$event.id)
    rects <- events[, c('start.sec', 'duration', 'bottom.f', 'top.f')]
    rects$label.tl <- features[,1]
    rects$label.tr <- features[,2]
    rects$label.br <- features[,3]
    rects$label.bl <- features[,4]
    rects$rect.color <- rep('#00ffff', nrow(rects))
    temp.dir <- TempDirectory()
    fns <- SaveMinuteSpectroImages(min.ids, rects, events, temp.dir)
    

    StitchImages(fns, OutputPath('InspectFeatures', ext = 'png'))   
    
    return(TRUE)    
}

SaveMinuteSpectroImages <- function (min.ids, rects, events, temp.dir) {
    
    fns <- rep(NA, length(min.ids))
    
    mins <- ExpandMinId(min.ids)

    for (i in 1:nrow(mins)) {
        #add events that belong in this sample
        min.id <- as.character(mins$min.id[i])
        which.events <- which(events$min.id == min.id)
        minute.events <- events[which.events, ]
        minute.rects <- rects[which.events, ]
        temp.fn <- paste(min.id, 'png', sep = '.')
        img.path <- file.path(temp.dir, temp.fn)

        
        Report(4, 'inspecting min id ', min.id)
        Report(4, 'num events = ', nrow(minute.events))
        
        # TODO: get this to work    
        #         wav <- Audio.Targeted(site = as.character(samples$site[i]),
        #                               start.date = as.character(samples$date[i]), 
        #                               start.sec = as.numeric(samples$min[i] * 60), 
        #                               duration = 60,
        #                               save = TRUE)
        
        Sp.CreateTargeted(site = mins$site[i], 
                          start.date = mins$date[i], 
                          start.sec = mins$min[i] * 60, 
                          duration = 60, 
                          img.path = img.path, 
                          rects = minute.rects,
                          label = mins$min.id[i])
        
        
        fns[i] <- img.path
        
    }
    
    return(fns)
    
    
}



InspectSamples <- function (samples = NA, output.fns = NA) {
    # draws the ranked n samples as spectrograms
    # with events marked and colour coded by cluster group
    
    rankings <- ReadObject('ranked_samples')
    min.ids <- ReadOutput('target.min.ids')
    d.names <- dimnames(rankings)
    num.clusters.choices <- d.names$num.clusters
    num.clusters.choice <- GetUserChoice(num.clusters.choices, 'number of clusters')
    num.clusters <- num.clusters.choices[num.clusters.choice]
    
    rankings <- rankings[,num.clusters.choice,]
    
    
    if(class(samples) == 'logical') {
        ranking.method.choices <- d.names$ranking.method
        ranking.methods <- GetMultiUserchoice(ranking.method.choices)
        
        ordered.samples <- matrix(NA, ncol = length(ranking.methods), nrow = g.num.samples)
        for (i in 1:length(ranking.methods)) {
            ordered.samples[,i] <- min.ids$min.id[order(rankings[ranking.methods[i], ])[1:g.num.samples]]
        } 
    } else {
        ordered.samples <- as.matrix(samples)
    }
    temp.dir <- TempDirectory()
    sample.spectro.fns <- CreateSampleSpectrograms(ordered.samples, num.clusters, temp.dir)
    
    if (class(output.fns) == 'logical') {
        output.fns <- 1:ncol(ordered.samples)
    }
    # if samples are chosen using rankings
    for (i in 1:ncol(ordered.samples)) {    
        output.fn <- paste('InspectSamples', output.fns[i],  collapse = "_", sep='.')
        StitchImages(sample.spectro.fns[,i], OutputPath(output.fn, ext = 'png'))   
    }
    
}

StitchImages <- function (image.paths, output.fn, vertical = TRUE) {      
        fns <- paste(image.paths, collapse = " ")
        command <- paste("/opt/local/bin/convert", 
                         fns, "-append", output.fn)
        Report(5, 'doing image magic command', command)
        err <- try(system(command))  # ImageMagick's 'convert'
        Report(5, err)
}



GetRankCols <- function (data.frame) {
    #returns the column names of data.frame which are names of ranking columns
    # i.e. the character "r" followed by an integer
    l <-  length(colnames(data.frame))
    return(intersect(paste0(rep('r',l), 1:l), colnames(data.frame)))  
}

CreateRects <- function (events, group) {
    # adds a "color" column to the events with a hex color
    # for each cluster group
    #
    # Args:
    #   events: data.frame
    #
    # Returns: 
    #   data.frame; same as input but with an extra column
    
    rects <- events[, c('start.sec', 'duration', 'bottom.f', 'top.f')]
    rects$label.br <- group
    rects$label.tl <- events$event.id
    rects$rect.color <- GetClusterGroupColors(group)
    return(rects)
    
}

GetClusterGroupColors <- function (groups) {
    unique.groups <- unique(groups)
    num.unique.groups <- length(unique.groups)
    unique.colors <- rainbow(num.unique.groups)
    group.colors <- rep(NA, length(groups))
    for (i in 1:num.unique.groups) {
        group.colors[groups == unique.groups[i]] <- unique.colors[i]
    } 
    return(group.colors)
}