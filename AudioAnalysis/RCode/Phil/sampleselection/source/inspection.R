# todo: this is completely broken since the multicluster group stuff
InspectClusters <- function (cluster.groups = NA, duration.per.group = 30, max.groups = 50) {
    #  shows individual events of a selected clusters, next to each other
    #  
    #  Args:
    #    cluster.groups: vector of ints; which cluster groups to show
    #    duration.per.group: int; number of seconds of audio to show per group (because some events are longer than others,
    #                              the number of events will be different per group. This way there is a similar horizontal size per row)
    #    max.groups: if cluster.groups is not provided it will be set to either the number of groups or max.groups (whichever is smaller)
    #
    #  Details:
    #    Randomply chooses events from the group 1 by 1 until their total duration (plus the padding on each side), reaches the duration.per.group
    #    It then generates spectrogram of the event plus the padding, and appends them to form a spectrogram of length duration.per.group
    #    each clustergroup will have its own row
    #

    events <- ReadOutput('events') # contains events including bounds
    clustered.events <- ReadOutput('clustered.events')  # contains only group and event id
    
    # get the different clusterings for different number of clusters
    clusterings <- colnames(clustered.events)
    clusterings <- clusterings[clusterings != 'event.id']
    
    if (length(clusterings) > 1) {
        which.k <- GetUserChoice(clusterings, 'which clustering (which size k) for inspection')
        all.groups <- unique(clustered.events[,which.k])
    } else {
        all.groups <- unique(clustered.events)
    }
    
    if (is.numeric(cluster.groups)) {
        # make sure that the cluster groups given are actually real groups
        cluster.groups <- cluster.groups[cluster.groups %in% all.groups]
    }
    
    
    events <- events[events$group %in% cluster.groups  ,]
    
    
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
    
    final.image.path <- OutputFilePath(IntegerVectorAsString(cluster.groups), ext = 'png', level = 3)
    StitchImages(group.spectro.fns, final.image.path)

    
}



InspectClusters.segment <- function (clusters = NULL, num.segments = 5, max.clusters = 5, segment.duration = 1) {
    #  for some given clusters, shows some segment events belonging to each of those clusters
    #  
    #  Args:
    #    cluster.groups: vector of ints; which clusters
    #    num.segments: int; number of segments to show for each cluster
    #    max.groups: if clusters is not provided it will be set to either the number of clusters or max.groups (whichever is smaller)
    #
    #  Details:
    #    Randomply chooses segments from the group 1 by 1 
    #    It then generates spectrogram of the event plus the padding, and appends them to form a spectrogram of length duration.per.group
    #    each clustergroup will have its own row
    #
    
    events <- ReadOutput('segment.events') #
    clustered.events <- ReadOutput('clustered.events')  # contains only group and event id and min id
    
    
    # double check that the event ids match correctly
    if (!all(clustered.events$data$event.id == events$data$event.id)) {
        stop('event id of events and clustered events data don\'t match')
    }
    
    # get the different clusterings for different number of clusters
    clusterings <- colnames(clustered.events$data)
    clusterings <- clusterings[!clusterings %in% c('event.id', 'min.id')]
    
    # combine the event columns to the clustered event data frame
    clustered.events.data <- cbind(clustered.events$data, events$data[,-(which(colnames(events$data) == 'event.id'))])
    

    
    if (length(clusterings) > 1) {
        which.k <- GetUserChoice(clusterings, 'which clustering (which size k) for inspection')
    } else {
        which.k <- 1
    }
    
    group.col <- clusterings[which.k]
    
    # this should really just be 1:num.clusters, but to be safe do it like this
    all.groups <- unique(clustered.events.data[,group.col])
    all.groups <- all.groups[order(all.groups)]
    
    if (is.numeric(clusters)) {
        # make sure that the cluster groups given as a param are actually real groups
        groups.that.exist <- clusters %in% all.groups
        groups.that.dont.exist <- clusters[!groups.that.exist]
        if (length(groups.that.dont.exist) > 0) {
            Report(1, 'Clusters specified by user that don\'t exist. Ignoring:', groups.that.dont.exist)
        }
        clusters <- clusters[groups.that.exist]
    } else {
        # no clusters specified by user, so 
        clusters <- all.groups
    }
    
    # make sure max is not exceeded
    if(length(clusters) > max.clusters) {
        removed <- clusters[(max.clusters + 1):length(clusters)]
        clusters <- clusters[1:max.clusters]
        Report(3, 'Number of clusters to render was greater than max.groups ... Ignoring the following clusters:', paste(removed, collapse = ','))
    }
    
    selected.events <- clustered.events.data[clustered.events.data[,group.col] %in% clusters,]
    

    
    # for each cluster, limit the number of segments shown
    for (group in clusters) {
        subset <- selected.events[group.col] == group
        num.before <- sum(subset)
        if (num.before > num.segments) {
            # select some random segments
            subset[subset][sample(num.before,num.before-num.segments)] <- FALSE
        }
        num.after <- sum(subset)
        Report(4, 'Selecting', num.after, 'spectrograms for group', group, 'out of a total of', num.before)
        selected.events <- selected.events[selected.events[group.col] != group | subset,]
    }
    
    
    
    
    spectro.list <- SaveSpectroImgsForInspection(selected.events, temp.dir)

    


    col.names <- colnames(spectro.list)
    col.names[col.names == group.col] <- 'group'
    colnames(selected.events) <- col.names
    
    html.file <- paste0('inspect.segments.', format(Sys.time(), format="%y%m%d_%H%M%S"), '.html')
    
    MakeHtmlInspector(selected.events, file.name =  html.file, group.col = 'group', template.file = segment.event.inspector.html)
    
}



SaveSpectroImgsForInspection <- function (events, temp.dir, use.parallel = TRUE) {
    # given a list of events/segments with at least the columns:
    #   event.id, file.path, file.sec, segment.duration
    # OR
    #   event.id, site, date, min, start.sec (which will use to figure out the file and offset second)
    # generates a spectrogram for each segment, saves the image to the temp path, 
    # then adds the image path (including filename in a new column)
    # and returns the dataframe
    
    events$spectro.fn <- ''
    

    
    temp.dir <- TempDirectory()
    
    temp.fns <- paste(events$event.id, 'png', sep = '.')
    events$spectro.fn <- file.path(temp.dir, temp.fns)
    nums <- 1:nrow(events)
    
    # there are 2 ways access audio for generating spectrograms: 
    # either give the path of the audio file, or supply the site/date/sec
    # here we choose the appropriate method depending on the columns in the data frame
    
    if (!use.parallel) {
        SetReportMode(socket = FALSE) 
        
    if (all(c('site', 'date', 'min', 'start.sec') %in% colnames(events))) {
        for (i in 1:nrow(events)) { 
            Sp.CreateTargeted(site = events$site[i], 
                              start.date = events$date[i], 
                              start.sec = events$min[i] * 60 + events$start.sec[i], 
                              duration = events$segment.duration[i], 
                              img.path = events$spectro.fn[i],
                              msg = nums[i])
        } 
        
    } else if (all(c('file.path', 'file.sec', 'segment.duration') %in% colnames(events))) {
            for (i in 1:nrow(events)) {
                sp.path <- Sp.CreateFromFile(path = events$file.path[i], 
                                             offset = events$file.sec[i], 
                                             duration = events$segment.duration[i],
                                             filename = events$spectro.fn[i],
                                             msg = nums[i]) 
                
            }

            
        } else {
            stop('wrong columns for SaveSpectroImgsForInspection')
        }
        
        
    } else {
        
        # generate spectrograms in parallel
        require('parallel')
        require('doParallel')
        require('foreach')
        SetReportMode(socket = TRUE)
        cl <- makeCluster(3)
        registerDoParallel(cl)
        
    if (all(c('site', 'date', 'min', 'start.sec') %in% colnames(events))) {
        # method 1: create targeted
        
        res <- foreach(site = events$site, 
                       start.date = events$date,
                       start.sec = events$min * 60 + events$start.sec,
                       duration = events$segment.duration,
                       img.path = events$spectro.fn,
                       num = nums, .combine='c', .export=ls(envir=globalenv())) %dopar% Sp.CreateTargeted(site = site, 
                                                                                                          start.date = start.date, 
                                                                                                          start.sec = start.sec, 
                                                                                                          duration = duration, 
                                                                                                          img.path = img.path,
                                                                                                          msg = num)         
    } else  if (all(c('file.path', 'file.sec', 'segment.duration') %in% colnames(events))) {
            # method 2: create from audio file
            
            res <- foreach(path = events$file.path, 
                           file.sec = events$file.sec,
                           duration = events$segment.duration,
                           img.path = events$spectro.fn,
                           num = nums, .combine='c', .export=ls(envir=globalenv())) %dopar% Sp.CreateFromFile(path = path, 
                                                                                                              offset = file.sec, 
                                                                                                              duration = duration,
                                                                                                              filename = img.path,
                                                                                                              msg = num)  
            
            

        } else {
            stop('wrong columns for SaveSpectroImgsForInspection')
        }
        
    }
    
    return(events$spectro.fn)
    


}

HtmlInspector <- function (spectrograms, template.file, output.fn = NULL, singles = list('title' = 'inspect segments')) {
    
    template.path <- file.path('templates', template.file)
    template <- readChar(template.path, file.info(template.path)$size)
    # replace title with title
    
    
    result <- HtmlInspector.InsertData(spectrograms, template, singles)
    
    if (!is.character(output.fn)) {
        output.fn <- template.file
    }
    
    output.path <- file.path(g.output.parent.dir, 'inspection', output.fn)
    
    fileConn<-file(output.path)
    writeLines(result, fileConn)
    close(fileConn)
    return(output.path)
    
}

HtmlInspector.InsertSingles.old <- function (singles, template) {
    # given a list of single text replacements, inserts them into the template
    for (name in names(singles)) {
        template <- InsertIntoTemplate(singles[[name]], name, template)
    }
    return(template)
}

HtmlInspector.InsertData <- function (spectrograms, template, singles = data.frame()) {
    
    require(stringr)
    
    
    # find first repeater
    open.ex <- "<##startforeach\\{[0-9a-z.]*}##>"
    open.loc <- str_locate(template, open.ex)
    if (is.na(open.loc[1])) {
        return(template)
    }
    open.txt <- str_sub(template, start = open.loc[1], end = open.loc[2])
    rep.name <- str_sub(str_extract(open.txt, "\\{[0-9a-z.]*}"), start = 2, end = -2)
    close.ex <- paste0("<##endforeach\\{",rep.name,"}##>")
    close.loc <- str_locate(template, close.ex)
    if (is.na(close.loc[1])) {
        stop(paste('error in template: missing closing tag for ', open.txt))
    }
    repeater.txt.template <- str_sub(template, start = open.loc[2]+1, end = close.loc[1]-1)
    
    repeater.res <- ""
    if (str_detect(repeater.txt.template, open.ex)) {
        if (rep.name != '') {
            groups <- unique(spectrograms[,rep.name])
            
            repeater.res <- rep(NA, length(groups))
            
            for (g in 1:length(groups)) {
                subset <- spectrograms[spectrograms[,rep.name] == groups[g],]
                sub.singles <- list()
                sub.singles[[rep.name]] <- groups[g]
                repeater.res[g] <- HtmlInspector.InsertData(subset, repeater.txt.template, singles = sub.singles)
            }
            
            repeater.res <- paste(repeater.res, collapse = "\n")
            
        } 
        # nothing happens if it is a repeater with a name and no nested repeater, 
        # maybe need to fix this but I can't think why this would be ever needed
        
        
    } else {
        repeater.res <- HtmlInspector.InsertIntoTemplate(repeater.txt.template, spectrograms)
        repeater.res <- paste(repeater.res, collapse = "\n")
    }
    
    template <- paste0(str_sub(template, 1, open.loc[1]-1), repeater.res, str_sub(template, close.loc[2]+1, str_length(template)))
    
    # recurse back into this function in case there are multiple repeaters (I don't see why there should ever need to be)
    # if it was the last one, then it will just return immediately
    template <- HtmlInspector.InsertData(spectrograms, template)
    
    #lastly, insert singles
    template <- HtmlInspector.InsertIntoTemplate(template, singles)
    
    return(template)

    
    
}

HtmlInspector.InsertIntoTemplate <- function (template, vals) {
    # vals is a data frame where the column names match the template flags
    # or a list where the names match
    
    vals <- as.data.frame(vals)
    
    if (nrow(vals) == 0) {
        return(template)
    }
    
    placeholder.name.ex <- '[0-9a-zA-Z._]+'
    placeholders.ex <- paste0("<##",placeholder.name.ex,"##>") 
    
    placeholders <- unique(unlist(str_extract_all(template, placeholders.ex)))
    placeholder.names <- unlist(str_extract_all(placeholders, placeholder.name.ex))
    
    verify <- placeholder.names %in% colnames(vals)
    if (!all(verify)) {
        Report(4, 'some flags from templage were not in the replacement list')
    }
    placeholders <- placeholders[verify]
    placeholder.names <- placeholder.names[verify]
    
    for(i in 1:length(placeholders)) {
        template <- str_replace_all(template, placeholders[i], vals[,placeholder.names[i]])
    }
    return(template)
    
}


InsertIntoTemplate.old <- function (flag, text, template, delim = "###") {
    # gsub is better?
    split <- paste0(delim, flag, delim)
    split.template <- unlist(strsplit(template, split, fixed = TRUE))
    text <- rep(text, length(split.template))
    text[length(text)] <- ''
    return(paste0(split.template, text, collapse = ''))
}



MakeHtmlInspector.old <- function (spectrograms, title = 'Inspect Segments', file.name = 'inspect.segments.html', group.col = 'group') {
    
    template.file <- 'templates/segment.event.inspector.html'
    template <- readChar(template.file, file.info(template.file)$size)
    # replace title with title

    template <- InsertIntoTemplate('title', title, template)
    groups <- unique(spectrograms[,group.col])
    rows = list()
    for (g in groups) {
        s <- spectrograms[spectrograms[,group.col] == g,]
        paths <- s$spectro.fn
        img.titles <- paste(s$event.id, s$site, s$date, s$min, sep = ' : ')
        img.tags <- paste0('    <img src="', paths, '" title="', img.titles, '" alt="" />', "\n") 
        rows[[g]] <- paste0(img.tags, collapse = '')
    }
    
    # wrap each rows in a div
    rows <- paste0('<div class="cluster" title="cluster:', groups, '">',"\n" ,rows, '</div>')
    
    # merge into a single string
    rows <- paste(rows, collapse = "\n\n")
    
    template <- InsertIntoTemplate('content', rows, template)
    
    output.file <- file.path(g.output.parent.dir, 'inspection', file.name)
   
    fileConn<-file(output.file)
    writeLines(template, fileConn)
    close(fileConn)
    return(file.name)
    
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
    #
    # Value: a matrix parallel to samples, where each cell has the filename of the 
    #        saved spectrogram which corresponds to the min id in the same cell of the samples matrix
    
    
    
    events <- ReadOutput('events')
    groups <- ReadOutput('clusters', level = 2)
    group.col.name <- paste0('group.', num.clusters)
    
    # need to filter events by the selected minutes
    # if we want to use the full range of colours on a smaller number of groups
    rects <- CreateRects(events, groups[, group.col.name])
    # file names for later use in imagemagick command to append together
    sample.spectro.fns <- matrix(NA, nrow = nrow(samples), ncol=ncol(samples))
    
    min.ids <- unique(as.vector(samples))


    
    fns <- SaveMinuteSpectroImages(min.ids, rects, events, temp.dir)
    
    for (i in 1:length(min.ids)) {
        # TODO: test this, it is completely untested. check that everything is working properly
        sample.spectro.fns[which(samples == min.ids[i])] <- fns[i]
    }
    
    return(sample.spectro.fns)
    
}



InspectEvents <- function (min.ids = 405) {
    FilterEvents1(min.ids)
 
    all.events <- ReadOutput('events')
    events <- all.events[all.events$min.id %in% min.ids, ]
    rects <- events[, c('start.sec', 'duration', 'bottom.f', 'top.f')]
    #rects$label.tl <- events$event.id
    rects$rect.color <- rep('#ff0000', nrow(rects))
    rects$rect.color[events$is.good] <- '#00ff00'
    
    output.path <- OutputFilePath(fn = 'event.filter', ext = 'png', level = 1)
    
    mins <- ExpandMinId(min.ids)
      
    spectro <- Sp.CreateTargeted(site = mins$site[1], 
                      start.date = mins$date[1], 
                      start.sec = mins$min[1] * 60 , 
                      duration = 60, 
                      rects = rects,
                      label = mins$min.id[1],
                      img.path = output.path)
    
    Sp.Draw(spectro)
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
    
    rankings <- ReadObject('ranked.samples')
    min.ids <- ReadOutput('target.min.ids')
    d.names <- dimnames(rankings$data)
    num.clusters.choices <- d.names$num.clusters
    num.clusters.choice <- GetUserChoice(num.clusters.choices, 'number of clusters', default = floor(length(num.clusters.choices)/2))
    num.clusters <- num.clusters.choices[num.clusters.choice]
    
    rankings <- rankings$data[,num.clusters.choice,]
    
    
    if(class(samples) == 'logical') {
        ranking.method.choices <- d.names$ranking.method
        ranking.methods <- GetMultiUserchoice(ranking.method.choices, 'ranking method', default = 1)
        
        ordered.samples <- matrix(NA, ncol = length(ranking.methods), nrow = g.num.samples)
        for (i in 1:length(ranking.methods)) {
            ordered.samples[,i] <- min.ids$data$min.id[order(rankings$data[ranking.methods[i], ])[1:g.num.samples]]
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
        StitchImages(sample.spectro.fns[,i], OutputFilePath(output.fn, ext = 'png', level = 3))   
    }
    
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