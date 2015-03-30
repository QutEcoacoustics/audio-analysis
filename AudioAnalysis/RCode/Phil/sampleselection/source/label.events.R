# match events to labels in the database
# not all events will match to labels, because only some vocalizations have been annotated



LabelEvents.2 <- function (events = NULL, reference.tags = FALSE, tag.ids = NULL) {
    
    if (is.null(events)) {
        events <- ReadOutput('all.events')
        events$data <- EventTimeBounds(events$data)
    }
    
    dependencies <- list(all.events = events$version)
    events <- events$data
    
    events.selection <- EventsSelection(events)
    events <- events[events.selection,]

    fields <- c('id', 'species_id', 'site', 'start_date_time_char', 'end_date_time_char', 'start_frequency', 'end_frequency', 'start_date', 'start_time') 
    tags <- ReadTagsFromDb(fields, unique(events$site), min(events$start.date.time), max(events$end.date.time), reference.tags = reference.tags)
    # remove tags for audio that we don't have
    tags <- RemoveTagsForMissingAudio(events, tags)
    
    if (!is.null(tag.ids)) {
        tags <- tags[tags$id %in% tag.ids,]
    }
    
    
    event.labels <- ReadOutput('event.labels', dependencies = dependencies, false.if.missing = TRUE)

    if (!is.list(event.labels)) {
        event.labels <- data.frame(event.id = integer(), species.id = integer(), tag.id = integer())
        WriteOutput(event.labels, 'event.labels', dependencies = dependencies)
    } else {
        event.labels <- event.labels$data
    }
    
    Report(4, sum(event.labels$species.id > 0)," / ", nrow(event.labels), "labeled events have a species id")
    
    save.every <- 50
    next.save <- nrow(event.labels) + save.every
    
    
    # add a 'min' column based on the 'time' column
    tags$min <- TimeToMin(tags$start_time)
    # extend the bounds
    tags <- ExtendTagBounds(tags)
    
    
    ######
    # TEMP to fix 0 tag ids in labels
    ######
    fix.zeros <- FALSE
    if (fix.zeros) {
        zeros <- which(event.labels$tag.id == 0)
        # indexes of rows neighbourning a zero
        neig <- c(zeros - 1, zeros + 1)
        
        neig <- neig[neig > 0 & neig < nrow(event.labels)]
        neig <- sort(unique(neig))
        
        tag.ids.to.fix <- event.labels$tag.id[neig]
        tag.ids.to.fix <- unique(tag.ids.to.fix[tag.ids.to.fix > 0])
        tags <- tags[tags$id %in% tag.ids.to.fix,]
        include.tags <- rep(TRUE, nrow(tags))
    } else {
        # ignore annotations that have already been checked
        include.tags <- !tags$id %in% event.labels$tag.id
    }
    

    


    
    
    enough = FALSE
    event.ids.before <- event.labels$event.id
    use.tag.id <- FALSE
    while (!enough) {
        
        if (use.tag.id > 0) {
            # use this tag id, whether or not it's already done
            # (it will be already done, because this will be true after an undo)
            cur.tag <- tags[tags$id == use.tag.id,]
        } else {
            
            if (sum(include.tags) < 1) {
                # we have finished !!
                Report(5, "All tags have been processed, well done!")
                break()   
            }
            
            Report(5, sum(include.tags), "tags left to process!") 
            
            # species ids for annotations which are still available for checking
            species.ids <- unique(tags$species_id[include.tags])
            
            # choose an annotation for a random species 
            sp.id <- SampleFromVector(species.ids, 1)
            species.tags <- tags[tags$species_id == sp.id & include.tags,]
            cur.tag <- species.tags[sample.int(nrow(species.tags),1),]
            
        }

        nearby.event.selection <- GetNearbyEvents(events, cur.tag)
        nearby.events <- events[nearby.event.selection,]  
        use.tag.id <- FALSE
        
        
        
        if (any(nearby.events$event.id %in% event.labels$event.id)) {
            # due to a bug, events that were not positively labeled
            # did not have the tag id recorded, so they could be in the list
            
            Report(5, 'updating tag id in event labels from 0')
            
            event.labels$tag.id[event.labels$event.id %in% nearby.events$event.id] <- cur.tag$id
            
            # this one's done
            include.tags[tags$id == cur.tag$id] <- FALSE
            
        } else if (nrow(nearby.events) > 0) {
            cur.event.labels <- LabelEventsForTag(cur.tag, nearby.events)
            
            if (is.data.frame(cur.event.labels)) {
                event.labels <- rbind(event.labels, cur.event.labels)
            }  else {               
                # if Q was pressed in the labling user input
                choices <- c('stop', 'Go back and fix last one')
                choice <- GetUserChoice(choices)
                
                if (choice == 2) {     
                    last.annotation.id <- event.labels$tag.id[nrow(event.labels)]
                    Report(5, 'Removing labels for events near annotation ', last.annotation.id)
                    to.remove <- LastBlock(event.labels$tag.id)
                    event.labels <- event.labels[-to.remove,]
                    use.tag.id <- last.annotation.id
                } else {
                    enough <- TRUE
                }
                
            }
            
        } else {
            # This shouldn't really happen, since every annotation should have an overlapping event
            Report(5, "0 events found for tag ", cur.tag$id)
            # this one's done
            include.tags[tags$id == cur.tag$id] <- FALSE
        }
        
        
        if (next.save < nrow(event.labels)) {
            next.save <- nrow(event.labels) + save.every
            WriteOutput(event.labels, 'event.labels', dependencies = dependencies)
        }
        
        
    }
    
    new.event.ids <- setdiff(event.labels$event.id, event.ids.before)
    
    
    new.labels <- event.labels[event.labels$event.id %in% new.event.ids,]
    
    ones <- sum(new.labels$species.id == -1)
    twos <- sum(new.labels$species.id == -2)
    
   if (length(new.event.ids) > 0 || fix.zeros) {
       WriteOutput(event.labels, 'event.labels', dependencies = dependencies)   
   }
    
    

    
    
}

EventsSelection <- function (events) {
    
    f1 <- events$bottom.f < events$top.f
    
    return(f1)
    
    
}

LabelEventsForTag <- function (tag, events) {
    
    margin <- 2
    tag.start.datetime <- as.POSIXlt(tag$start_date_time_char)
    tag.end.datetime <- as.POSIXlt(tag$end_date_time_char)
    tag.duration <- difftime(tag.end.datetime, 
                             tag.start.datetime, 
                             units = 'secs')
    
    date <- strftime(tag.start.datetime, '%Y-%m-%d')    

    spectro.start <- tag.start.datetime - margin
    spectro.duration <- tag.duration + margin*2
    spectro.start.sec <- as.numeric(difftime(spectro.start, as.POSIXlt(date), units = 'secs')) # number of seconds from start of day
    

    
    events.start.datetime <- as.POSIXlt(events$start.date.time.exact)
    events.start.offset <- as.numeric(difftime(events.start.datetime, spectro.start, units = 'secs'))
    
    event.todo <- 'orange'
    event.done <- 'red'
    event.col.selected <- 'green'
    tag.col <- 'white'
    
    # events rects
    rects <- data.frame(start.sec = events.start.offset, duration = events$duration, top.f = as.numeric(events$top.f), bottom.f = as.numeric(events$bottom.f), rect.color = event.todo, stringsAsFactors = FALSE)
    tag.rect <- data.frame(start.sec = margin, duration = tag.duration, top.f = as.numeric(tag$end_frequency), bottom.f = as.numeric(tag$start_frequency), rect.color = tag.col, stringsAsFactors = FALSE)
    
    
    rects <- rbind(rects, tag.rect)
    
    spectro <- Sp.CreateTargeted(site = tag$site, start.date = date, start.sec = spectro.start.sec, duration = spectro.duration, rects = rects)
    
    species.ids <- tag.ids <- rep(0, nrow(events))
    
    codes.msg <- c('same species as annotation', 'bird but unsure about species', 'not bird', 'all are from annotation')
    codes.char <- c('1', '2', '3', '4')
    msg <- paste(paste0(codes.char, ") ", codes.msg), collapse = " ")
    
    # -1 bird but unsure about species
    # -2 not bird
    # NA not checked

    spectro$rects <- rects
    Sp.Draw(spectro, scale = 2)
    
    print(msg)

    cur.e <- 1
    # while there are still unprocessed events
    while(any(species.ids == 0)) {
        spectro$rects$rect.color[cur.e] <- event.col.selected   
        Sp.Rect(spectro, rect.num = cur.e, fill.alpha = 0)

        valid <- FALSE
        while(!valid) {
            valid <- TRUE
            input <- readline(paste("label event?  : "))
            if (input == '4') {
                # all from this one until the end
                species.ids[cur.e:length(species.ids)] <- tag$species_id
            } else if (input == '1') {
                species.ids[cur.e] <- tag$species_id
            } else if (input == '2') {
                species.ids[cur.e] <- -1
            } else if (input == '3') {
                species.ids[cur.e] <- -2 
            } else if (input == 'Q') {
                return(FALSE)
            } else {
                valid <- FALSE
            }
        }
        
        spectro$rects$rect.color[cur.e] <- event.done
        Sp.Rect(spectro, rect.num = cur.e, fill.alpha = 0)
        cur.e <- cur.e + 1
        
    }
    

    
    
    return(data.frame(event.id = events$event.id, species.id = species.ids, tag.id = as.integer(tag$id)))
    
    
    
    
    
}



GetNearbyEvents <- function (events, tag, min.time.overlap = 0.5, min.frequency.overlap = 0.2) {
    # given a df of events and a tag from the database, 
    # finds any events that are amount.inside.tag overlapping with the tag
    # eg if amount.inside.tag == 0.5 then it will find events that are more then half inside the tag
    
    #shortcuts
    t.l <- tag$start.date.time.extended
    t.r <- tag$end.date.time.extended
    t.t <- tag$top.f.extended
    t.b <- tag$bottom.f.extended
    e.l <- events$start.date.time.exact
    e.r <- events$end.date.time.exact
    e.t <- events$top.f
    e.b <- events$bottom.f
     
    # find all the events which fall within this annotation
    # filters
    f1 <- events$site == tag$site
    
    if (min.time.overlap * min.frequency.overlap >= 1) {
        
        # this could be done the same as the other cases,
        # but it might be faster so I separated it out
        f2 <- t.l < e.l
        f3 <- t.r > e.r
        f4 <- t.b < e.b
        f5 <- t.t > e.t
        
        matching <- f1 & f2 & f3 & f4 & f5    
        
    } else {
        
        time.overlap <- as.numeric(RangeIntersection(e.l, e.r, t.l, t.r))
        time.overlap <- time.overlap / events$duration
        frequency.overlap <- RangeIntersection(e.b, e.t, t.b, t.t)
        frequency.overlap <- frequency.overlap / (events$top.f - events$bottom.f)
        overlap <- frequency.overlap * time.overlap
        matching <- frequency.overlap >= min.frequency.overlap & time.overlap >= min.time.overlap  & f1
        
    }
    
    return(matching)
    
}


RangeIntersection <- function (from.1, to.1, from.2, to.2) {
    # give 2 ranges, (from.1, to.1) and (from.2, to.2)
    # gives the size of the intersection of the ranges
    # works on vectors as well. length(from) must equal 
    # length(to) and length(1) must equal length(2) or
    # one of them must be length 1

    l.max <- pmax(from.1, from.2)
    r.min <- pmin(to.1, to.2)
    intersection <- r.min - l.max
    intersection[intersection < 0] <- 0
    return(intersection)  
    
}

test.RangeIntersection <- function () { 
    from.2 <- 5
    to.2 <- 10
    from.1 <- c(1,1,7,11)
    to.1 <- c(8,12,14,15)
    expected <- c(3, 5, 3, 0)
    res <- RangeIntersection(from.1, to.1, from.2, to.2)
    error <- abs(res - expected)
    print(error)
}




RemoveTagsForMissingAudio <- function (events, tags) {
    # given a list of events and tags
    # removes the tags for days/sites that have no events
    
    event.counts <- as.data.frame(table(events[,c('site', 'date')]), stringsAsFactors = FALSE)
    
    
    tag.sites <- unique(tags$site)
    tag.dates <- unique(tags$start_date)
    
    include <- rep(FALSE, nrow(tags))
    
    for (s in tag.sites) {
        for (d in tag.dates) {
            event.count.selection <- event.counts$site == s & event.counts$date == d
            # if one site or date is completely missing from events, then the event count
            # won't have it in the count table. The site or the date is in the events but no
            # events from that site and date, the count will be present and zero
            if (sum(event.count.selection) > 0 && event.counts$Freq[event.count.selection] > 0) {     
                include[tags$site == s & tags$start_date == d] <- TRUE
            }
        }
    }
    
    tags <- tags[include,]
    
    return(tags)
    
    
}


ExtendTagBounds <- function (tags, time.extension = 0.5, frequency.extension = 50) {
    # when matching events which fit within an annotation, 
    # extend the annotation a bit to capture ones which overlap a little
    
    tags$start.date.time.extended <- ExtendDateTime(tags$start_date_time_char, -time.extension)
    tags$end.date.time.extended <- ExtendDateTime(tags$end_date_time_char, time.extension)
    tags$bottom.f.extended <- as.numeric(tags$start_frequency) - frequency.extension
    tags$top.f.extended <- as.numeric(tags$end_frequency) + frequency.extension
    
    return(tags)
    
    
}

ExtendDateTimeTxt <- function (date.times, change) {
    # given date.times in the form eg 2010-10-13 12:13:14.123
    # adds the given number of seconds
    # minutes are not changed. if the new number of seconds goes above 60 it will be set to 60.000
    # if it goes below 0 it will be set to 00.000
    
    seconds <- substr(date.times, 18, 23)
    new.seconds <- as.numeric(seconds) + change
    new.seconds[new.seconds > 60] <- 60
    new.seconds[new.seconds < 0] <- 0
    new.seconds <- sprintf('%06.3f', new.seconds)
    substr(date.times, 18, 23) <- new.seconds
    return(date.times)
}

ExtendDateTime <- function (date.times, change) {
    # given date.times in the form eg 2010-10-13 12:13:14.123
    # adds the given number of seconds
    # minutes are not changed. if the new number of seconds goes above 60 it will be set to 60.000
    # if it goes below 0 it will be set to 00.000
    date.times <- as.POSIXlt(date.times)
    date.times <- date.times + change
    return(date.times)
}





ReadEventsForLabeling <- function () {
    events <- ReadOutput('all.events')
    events$data <- EventTimeBounds(events$data)
    return(events)
}



LabelEvents <- function (events, dependencies, limit = NULL) {
    # given a dataframe of events, assigns species id to them from the database if possible
     
    # for debugging, to speed things up, limit the number of events
    if (!is.null(limit) && nrow(events) > limit) {
        events <- events[sample.int(nrow(events), limit),]
    }  
    
    fields <- c('id', 'species_id', 'site', 'start_date_time_char', 'end_date_time_char', 'start_frequency', 'end_frequency', 'start_date', 'start_time')

    sites <- unique(events$site)
    tags <- ReadTagsFromDb(fields, sites, min(events$start.date.time), max(events$end.date.time))
    # add a 'min' column based on the 'time' column
    tags$min <- TimeToMin(tags$start_time)   

    species.id <- tag.id <- rep(-1, nrow(events))
    result <- data.frame(species.id = species.id, tag.id = tag.id)
    
    # split the data hierarchically to speed things up  

    # debug
  #  events <- events[events$site == 'NW' & events$date == '2010-10-13', ]
    
    
    sites <- unique(events$site)
    # for each site
    for (s in 1:length(sites)) {
        site.selection <- events$site == sites[s]
        site.tags <- tags[tags$site == sites[s], ]
        dates <- unique(events$date[site.selection])
        # for each date in that site
        for (d in 1:length(dates)) {  
            site.date.selection <- site.selection & events$date == dates[d]        
            
            # for each min in that site and date
            Report(5, 'labeling events for', sites[s], dates[d])

            
            result[site.date.selection,] <- LabelEventsSubset(events[site.date.selection,], site.tags[site.tags$start_date == dates[d], ])
            
            
            Report(5, sum(species.id[site.date.selection] > -1), 'events have been labeled for this site/day', nl.before = TRUE)
            
        } # for each date
    } # for each site
    
    

    
    events$species.id <- species.id
    events$tag.id <- tag.id
    events$confirmed <- confirmed
    
    # only save those events which were given a species id
    events <- events[species.id != -1, ]
    
  #  WriteOutput(events, 'labeled.events', dependencies = list(all.events = events$version))
    
    return(events)
    
    
    
    
}


LabelEventsSubset <- function (events, tags) {
    
    mins <- unique(events$min)
    species.ids <- tag.ids <-  rep(-1, nrow(events))
    Report(5, 'please wait for', length(mins), 'dots (one for each minute)')
    for (m in 1:length(mins)) {
        
        min.selection <- events$min == mins[m]
        min.events <- events[min.selection, ]  
        min.tags <- tags[tags$min == mins[m], ]  
        min.species.ids <- min.tag.ids <- rep(-1, nrow(min.events))
        
        if (mins[m] %% 10 == 0) {
            Report(5, mins[m], nl.after = FALSE)
        }
        Dot()
        
        if (nrow(min.tags) > 0) {
            
            for (t in 1:nrow(min.tags)) {
                
                tag <- tags[t, ]
                
                # find all the events which fall within this annotation
                # filters
                f2 <- tag$start_date_time_char < min.events$start.date.time.exact
                f3 <- tag$end_date_time_char > min.events$end.date.time.exact
                f4 <- tag$start_frequency > min.events$bottom.f
                f5 <- tag$end_frequency > min.events$top.f
                
                matching <- f2 & f3 & f4 & f5
                
                min.species.ids[matching] <- tag$species_id
                min.tag.ids[matching] <- tag$id
                
            }
            
            species.ids[min.selection] <- min.species.ids
            tag.ids[min.selection] <- min.tag.ids 
            
        }  # if there are any tags in the min
        

        
    } # for each min
    
    return(data.frame(species.ids = species.ids, tag.ids = tag.ids))
    
    
}





EventTimeBounds <- function (events, append = TRUE) {
    # adds some extra columns needed for comparison with database
    # start.date.time,
    # end.date.time
    # start.date.time.exact
    # end.date.time.exact
    
    end.sec <- events$start.sec + events$duration 
    end.min <- events$min + floor(end.sec / 60)
    end.sec <- end.sec - floor(end.sec / 60) * 60
    
    end.time <- SetTime(end.min, end.sec)
    start.time <- SetTime(events$min, events$start.sec)

    start.date.time <- paste0(events$date, " ", start.time) 
    end.date.time <- paste0(events$date, " ", end.time) 
    
    end.time.exact <- SetTime(end.min, end.sec, decimal.places = 3)
    start.time.exact <- SetTime(events$min, events$start.sec, decimal.places = 3)
    
    start.date.time.exact <- paste(events$date, start.time.exact) 
    end.date.time.exact <- paste(events$date, end.time.exact) 
    
    start.date.time.exact <- as.POSIXlt(start.date.time.exact)
    end.date.time.exact <- as.POSIXlt(end.date.time.exact)
    
    # new data frame of event bounds matching the events data frame
    event.bounds <- data.frame(start.date.time = start.date.time, 
                               end.date.time = end.date.time, 
                               start.date.time.exact = start.date.time.exact,
                               end.date.time.exact = end.date.time.exact, stringsAsFactors = FALSE)
    
    if (append) {
        return(cbind(events, event.bounds))
    } else {
        return(event.bounds)
    }
    

    
}


FindAnnotation <- function (event.bounds, tags) {
    # given an event with the following columns
    # date, min, sec, top.f, bottom.f,
    # checks if 
    # it is contained within an annotation, or nearly within an annotation
    # if so, returns the species id, if not returns -
    

    
    # filters
    f1 <- tags$site == event.bounds$site
    f2 <- tags$start_date_time_char < event.bounds$start.date.time
    f3 <- tags$end_date_time_char > event.bounds$end.date.time
    f4 <- tags$start_frequency > event.bounds$bottom.f
    f5 <- tags$end_frequency > event.bounds$top.f
    
    matching.tags <- tags[f1 & f2 & f3 & f4 & f5, ]
    

    
    if (nrow(matching.tags) == 1) {
        return(matching.tags)
    } else {
        return(NULL)
    }
    
    
    
}