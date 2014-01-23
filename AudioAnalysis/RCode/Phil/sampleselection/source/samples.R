
SelectSamples <- function (num.rows.to.use = FALSE) {
    # selects 1 minute samples from a recording as the 'best' for finding the 
    # most species
    #
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
    
    
    events <- as.data.frame(read.csv(OutputPath('clusters')))
    
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
    
    Report(2, 'Saving selected minute samples to ', OutputPath('selected_samples'));
    write.csv(selection, OutputPath('selected_samples'), row.names = FALSE)
    
    return(selection) 
    
}





CountSpecies <- function (selected.samples, speciesmins) {
    # finds which species were present in the minutes supplied in selected.samples
    # out of a full species list speciesmins
    
    found.species <- DoSpeciesCount(selected.samples, speciesmins)

    min.list <- read.csv(OutputPath('minlist'))
    
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


EvaluateSamples <- function (samples) {
    # given a list of minutes, finds the number of species that 
    # appear in those minutes. 
    # also finds the number of total species that appear in between
    # the processed dates at the processed sites
    
    #speciesmins <- read.csv(g.species.path)
    
    Report(1, 'evaluating samples') 
    tag.fields <- c('start_date', 'start_time', 'site', 'species_id')
    Report(2, 'Checking mySql database of labeled minutes (tags)');
    tags <- GetTags(tag.fields)
    date.col <- match('start_date', colnames(tags))
    time.col <- match('start_time', colnames(tags))
    # we have the samples and selected minutes as the minute number within the day (eg 1000 = 4:40pm)
    # need to transforme the date_time of speciesmins to the same format
    min.nums <- apply(tags, 1, 
                      function (tag, date.col, time.col) {
                          sdt <- strptime(paste(tag[date.col], tag[time.col]), 
                                          format = '%Y-%m-%d %H:%M:%S')
                          hour <- format(sdt, format = '%H')
                          min <- format(sdt, format = '%M')
                          minnum <- as.numeric(hour) * 60 + as.numeric(min) 
                          return(minnum)
                      }, date.col, time.col)
    tags <- as.data.frame(cbind(tags, min.nums))
    colnames(tags) <- c(tag.fields,'min')
    
    
    
    CountSpecies(samples, tags)
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
        samples <- read.csv(OutputPath('selected_samples'))
    }
    events <- read.csv(OutputPath('clusters'), stringsAsFactors=FALSE)
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

