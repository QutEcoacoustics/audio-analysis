# match events to labels in the database
# not all events will match to labels, because only some vocalizations have been annotated



LabelEvents.2 <- function (events = NULL) {
    
    if (is.null(events)) {
        events <- ReadOutput('all.events')
    }
    # for debugging, to speed things up, limit the number of events
    if (!is.null(limit) && nrow(events$data) > limit) {
        events$data <- events$data[sample.int(nrow(events$data), limit),]
    }   
    
    event.time.bounds <- EventTimeBounds(events$data)
    fields <- c('id', 'species_id', 'site', 'start_date_time_char', 'end_date_time_char', 'start_frequency', 'end_frequency', 'start_date', 'start_time')
    
    sites <- unique(events$data$site)
    tags <- ReadTagsFromDb(fields, sites, min(event.time.bounds$start.date.time), max(event.time.bounds$end.date.time))
    # add a 'min' column based on the 'time' column
    tags$min <- TimeToMin(tags$start_time)
    
    
    species.ids <- unique(tags$species_id)
    cur.species.count <- 0
    cur.species.id <- 1 
    

    
    
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
                f2 <- tag$start_date_time_char < min.events$start.date.time
                f3 <- tag$end_date_time_char > min.events$end.date.time
                f4 <- tag$start_frequency > min.events$bottom.f
                f5 <- tag$end_frequency > min.events$top.f
                
                matching <- f2 & f3 & f4 & f5
                
                min.species.ids[matching] <- tag$species.id
                min.tag.ids[matching] <- tag$id
                
            }
            
            
        }  # if there are any tags in the min
        
        species.ids[min.selection] <- min.species.ids
        tag.ids[min.selection] <- min.tag.ids
        
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