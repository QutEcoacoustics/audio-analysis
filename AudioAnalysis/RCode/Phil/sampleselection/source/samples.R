
RankSamples <- function (num.rows.to.use = FALSE) {
    # ranks all the minutes in the target in the order 
    # that should find the most species in the shortest number of minute
    # samples
    #
    # Details:
    # reads the list of events as detected in part 1 of the whole process
    # combines this with the cluster-list
    # denotes which minute each event belongs i
    # selects minutes based on
    # - minutes with the most events
    # - minutes with the most clusters not previously assigned. 
    # first give each minute a 'rank' based on how many events it has
    # seconds give each minute a 'rank' based on how many clusters it has
    # order by the sum of the 2 ranks for final rank.
    require('plyr')
    
    Report(1, 'Selecting samples')
    
    
    events <- ReadOutput('clusters')
    
    # limit the number of events for dev
    if (num.rows.to.use != FALSE && num.rows.to.use < nrow(events)) {
        events <- events[1:num.rows.to.use, ]
    } else {
        num.rows.to.use <- nrow(events)
    }
    
    if (g.num.samples > num.rows.to.use) {
        stop(paste0("Number of samples is more than number of events. ", 
                    "num.samples = ", g.num.samples, 
                    ". num events = ", num.rows.to.use))
    }
    
    # adds a column which denotes which minute of the day the event happened in
    events <- SetMinute(events)
    
    minute.col <- ncol(events)
    group.col <- minute.col - 1
    date.site.cols <- match(c('date', 'site'), colnames(events))
    
    # the number of the columns with the site, date, and minute of the day
    # to identify a unique minute recording
    id.cols <- c(date.site.cols, minute.col)
    
    # number of events in each minute
    # 4 column dataframe: the three id columns and the frequency
    # minutes with zero events are discarded
    Report(5, 'calculating number of events in each minute')
    num.events.per.min <- count(as.data.frame(events[,id.cols]))
    Report(4, nrow(num.events.per.min), 'minutes have at least one event')
    
    # list of unique group-minute pairs 
    # (i.e. remove duplicate groups from the same minute)
    unique.cluster.minutes <- unique(events[, c(id.cols, group.col)])
    num.clusters.per.min <- count(unique.cluster.minutes[,1:length(id.cols)])
    Report(4, nrow(unique.cluster.minutes), 'cluster minutes ')
    # todo: check this part
 #   Report(4, nrow(num.clusters.per.min), '')
    
    
    mins <- cbind(num.events.per.min, 
                  num.clusters.per.min[,ncol(num.clusters.per.min)])
    
    col.names <- c('date', 'site', 'min','num.events','num.clusters')
    colnames(mins) <- col.names
    
    total <- mins$num.events + mins$num.clusters
    mins <- cbind(mins, total)
    
    mins.sorted <- mins[order(mins[,ncol(mins)], decreasing = TRUE),]
 
    # Todo: improve selection method
    Report(2, 'Sorting minutes based on number of unique cluster groups and number of events')
    Report(1, 'Selecting ', g.num.samples, "(number to minutes to select is set in config")
    if (nrow(mins.sorted) < g.num.samples) {
        g.num.samples <- nrow(mins.sorted)
    }
    
    selection <- mins.sorted[1:g.num.samples, ]
    

    WriteOutput(mins.sorted, 'ranked_samples')
    
    return(selection) 
    
}


OptimalSamples <- function (speciesmins = NA, mins = NA, num.samples = NA) {
    # determines the best possible selection of [numsamples] minute samples
    # to find the most species
    # 
    # Args:
    #   speciesmins: dataframe; the list of species in each minute. If not included
    #                            will retreive from database
    #   mins: dataframe; the list of minutes we can select from. If not
    #                    included then will be read from config
    #   num.samples: int; how many samples to select. If not supplied, 
    #                     will be read from config
    #
    # Value:
    #   list: containing
    #       data.frame; A list of minutes. Cols: site, date, min
    #       the progression of species found in each of those minutes
    #       the progression of total species found count after each minute
    
    if (class(speciesmins) != 'data.frame') {
        speciesmins <- GetTags()
    }
    
    if (class(mins) != 'data.frame') {
        mins <- ReadOutput('minlist', false.on.missing = TRUE)
        if (class(mins) != 'data.frame') {
            CreateMinuteList()
            mins <- ReadOutput('minlist')
        }
    }
    
    if (is.na(num.samples)) {
        num.samples <- g.num.samples
    }
    
    species.in.each.min <- vector("list", nrow(mins))
    total.num.species <- length(unique(speciesmins$species.id))
    # maximum number of samples is the number of species
    selected.samples <- rep(NA, total.num.species)
    found.species.count.progression <- rep(NA, total.num.species)
    found.species.progression <- vector("list", total.num.species)
    all.found.species <- numeric()
    
    # create list of the species in each minute
    for (i in 1:nrow(mins)) {
        sp.list <- speciesmins$species.id[speciesmins$site == mins$site[i] & speciesmins$date == mins$date[i] & speciesmins$min == mins$min[i]]
        species.in.each.min[[i]] <- sp.list
    }

    for(sp in 1:length(selected.samples)) {
        # find minute with most species
        max.sp <- 0
        max.sp.i <- -1
        for (m in 1:length(species.in.each.min)) {
            if (length(species.in.each.min[[m]]) > max.sp) {
                max.sp <- length(species.in.each.min[[m]])
                max.sp.i <- m
            } 
        }
        
        if (max.sp == 0) {
            # all species have been included in the selected mins 
            # (or there were no species)
            break()
        }
        
        #record that minute
        selected.samples[sp] <- max.sp.i
        last.found.species <- species.in.each.min[[max.sp.i]]
        all.found.species <- union(all.found.species, last.found.species)
        found.species.progression[[sp]] <- all.found.species
        found.species.count.progression[sp] <- length(all.found.species)
        #remove the already found species from the list
        for (m in 1:length(species.in.each.min)) { 
               sp <- species.in.each.min[[m]]
               species.in.each.min[[m]] <- sp[! sp %in% last.found.species]
        }

    }
    
    selected.samples <- selected.samples[! is.na(selected.samples)]
    found.species.count.progression <- found.species.count.progression[! is.na(found.species.count.progression)]
    selected.sample.mins <- mins[selected.samples,]
    
    plot(found.species.count.progression, type='both')
    
    return(list(
        found.species.progression = found.species.progression
        found.species.count.progression = found.species.count.progression
        selected.mins = selected.samples
        ))
    
    
    
}


CountSpecies <- function (selected.samples, speciesmins) {
    # finds which species were present in the minutes supplied in selected.samples
    # out of a full species list speciesmins
    found.species <- DoSpeciesCount(selected.samples, speciesmins)
    min.list <- ReadOutput('minlist')
    total.species <- DoSpeciesCount(min.list, speciesmins)
    Report(1, 'number of species found = ', length(found.species))   
    if (length(found.species) > 0) {
        Report(2, 'species list:')
        Report(2, found.species)
    }
    if (length(total.species) == 0) {
        percent <- 100
    } else {
        percent <- length(found.species) * 100 / length(total.species)
    }
    Report(1, percent,"% of ", length(total.species)," species present")
}


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


EvaluateSamples <- function (samples = NA) {
    # given a list of minutes
    # 
    
    if(is.na(samples)) {
        samples <- ReadOutput('ranked_samples')
    }
    
    
}


EvaluateSamples.old <- function (samples = NA) {
    # given a list of minutes, finds the number of species that 
    # appear in those minutes. 
    # also finds the number of total species that appear in between
    # the processed dates at the processed sites
    
    Report(1, 'evaluating samples') 
    if(is.na(samples)) {
        samples <- ReadOutput('selected_samples')
    }
    speciesmins <- GetTags();
    CountSpecies(samples, speciesmins)
    Report(3, "Saving spectrograms of samples with events.")

}





SetMinute <- function (events)  {
    # for a list of events which contains the filename 
    # (which has the start time for the file encoded)
    # and the start time of the event, works out the minute 
    # of the day that the event happened in
    
    start.sec.col <- which( colnames(events) == "start.sec" )
    min <- apply(events, 1, function (v) {
        sec <- as.numeric(unlist(v[start.sec.col]))
        min <- floor(sec / 60)
        return (min)
    })
    
    
    new <- cbind(events, min)
    
    return (new)
    
}



# maybe move this somewhere else
InspectSamples <- function (samples = NA) {
    if(is.na(samples)) {
        samples <- ReadOutput('selected_samples')
    }
    events <- ReadOutput('clusters')
    events <- AssignColourToGroup(events)
    events <- AddMinuteIdCol(events)
    samples <- AddMinuteIdCol(samples)
    
    event.col <- as.data.frame(rep(NA, nrow(samples)))
    colnames(event.col) <- c('events')
    samples <- cbind(samples, event.col)
    
    
    w <- 1000
    # todo: fix this so that it the height of each spectrogram
    # is what it actually is, instead of hardcoded 256
    h <- nrow(samples) * 256
    
    temp.dir <- TempDirectory()
    
    # file names for later use in imagemagick command to append together
    im.command.fns <- ""
    
    
    for (i in 1:nrow(samples)) {
        
        #add events which belong in this sample
        min.id <- as.character(samples$min.id[i])
        minute.events <- events[which(events$min.id == min.id),]
        
        
        
        # offset the start sec of the event so that it is 
        # relative to the start of the sample
        minute.events$start.sec <- (minute.events$start.sec - 
                                        (samples$min[i] * 60))
        
        temp.fn <- paste(i, 'png', sep = '.')
        img.path <- file.path(temp.dir, temp.fn)
        im.command.fns <- paste(im.command.fns, img.path)
        
    # TODO: get this to work    
#         wav <- Audio.Targeted(site = as.character(samples$site[i]),
#                               start.date = as.character(samples$date[i]), 
#                               start.sec = as.numeric(samples$min[i] * 60), 
#                               duration = 60,
#                               save = TRUE)

        Sp.CreateTargeted(site = samples$site[i], 
                          start.date = samples$date[i], 
                          start.sec = samples$min[i] * 60, 
                          duration = 60, 
                          img.path = img.path, 
                          rects = minute.events)
        
        
        
    }
    output.file <- OutputPath('InspectSamples', ext = 'png')
    command <- paste("/opt/local/bin/convert", 
                     im.command.fns, "-append", output.file)
    
    err <- try(system(command))  # ImageMagick's 'convert'

    
}

AssignColourToGroup <- function (events) {
    # adds a "color" column to the events with a hex color
    # for each cluster group
    #
    # Args:
    #   events: data.frame
    #
    # Returns: 
    #   data.frame; same as input but with an extra column
    
    groups <- unique(events$group)   
    num.groups <- length(groups)
    colors <- rainbow(num.groups)
    Report(6, 'Cluster group colors', colors)
    event.colors <- events$group
    for (i in 1:num.groups) {
        event.colors[event.colors == groups[i]] <- colors[i]
    }
    event.colors <- as.data.frame(event.colors)
    colnames(event.colors) <- "rect.color"
    events <- cbind(events, event.colors)
    return(events)
    
}

AddMinuteIdCol <- function (data) {
    
    cols <- colnames(data)
    date.col <- match('date', cols)
    site.col <- match('site', cols)
    min.col <- match('min', cols)
    sec.col <- match('start.sec', cols)
    
    ids <- apply(as.matrix(data), 1, function (v) {
        
        
        
        if (is.na(min.col)) {
            min <- floor(as.numeric(v[sec.col]) / 60)
        } else {
            min <- as.numeric(v[min.col])
        }
        
        id <- paste0(v[date.col], v[site.col], min)
        return(id)
        
        
    })
    
    new.data <- cbind(data, ids)
    colnames(new.data) <- c(cols, 'min.id')
    
    return(new.data)
    
    
    
}

