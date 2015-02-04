# match events to labels in the database
# not all events will match to labels, because only some vocalizations have been annotated


LabelEvents <- function (all.events = NULL) {
    # given a dataframe of events, assigns species id to them from the database if possible
    
    #todo: filter events
    
    require('plyr')
    
    if (is.null(events)) {
        events <- ReadOutput('events')
    }
    
    # for debugging, to speed things up, limit the number of events
    limit <- 2000
    if (nrow(events$data) > limit) {
        events$data <- events$data[sample.int(nrow(events$data), limit),]
    }
    
    # get the start datetime and enddatetime to search all tags between those
    # then search the dataframe of tags for ones that match the exact start datetime and end datetime
    # including miliseconds

    
    end.sec <- events$data$start.sec + events$data$duration 
    end.min <- events$data$min + floor(end.sec / 60)
    end.sec <- end.sec - floor(end.sec / 60) * 60
    
    end.time <- SetTime(end.min, end.sec)
    start.time <- SetTime(events$data$min, events$data$start.sec)
    

    
    quote <- "'"  
    start.date.time <- paste0(quote, events$data$date, " ",start.time, quote) 
    end.date.time <- paste0(quote, events$data$date, " ",start.time, quote) 
    
    fields <- c('id', 'species_id', 'site', 'start_date_time_char', 'end_date_time_char', 'start_frequency', 'end_frequency')
    
    sites <- unique(events$data$site)
    
    tags <- ReadTagsFromDb(fields, sites,min(start.date.time), max(end.date.time))
    
    
    end.time.exact <- SetTime(end.min, end.sec, decimal.places = 3)
    start.time.exact <- SetTime(events$data$min, events$data$start.sec, decimal.places = 3)
    
    start.date.time.exact <- paste(events$data$date, end.time.exact) 
    end.date.time.exact <- paste(events$data$date, end.time.exact) 
    
    # new data frame of event bounds matching the events data frame
    event.bounds <- data.frame(start.date.time = start.date.time.exact, 
                               end.date.time = end.date.time.exact, 
                               bottom.f = events$data$bottom.f, 
                               top.f = events$data$top.f,
                               site = events$data$site, stringsAsFactors = FALSE)
    
    
    
    #species.id <- adply(event.bounds, 1, FindLabel, tags)
    

    
    species.id <- tag.id <- rep(-1, nrow(event.bounds))

    confirmed <- rep(FALSE, length(species.id))
    
    for (r in 1:nrow(event.bounds)) {
        annotation <- FindAnnotation(event.bounds[r,], tags)
        if (!is.null(annotation)) {
            species.id[r] <- annotation$species.id
            tag.id[r] <- annotation$id
        }

        
    }
    
    events$data$species.id <- species.id
    events$data$tag.id <- tag.id
    events$data$confirmed <- confirmed
    
    events$data <- events$data[species.id != -1, ]
    
    
    WriteOutput(events$data, 'labeled.events', dependencies = list(all.events = all.events$version))
    
    return(events$data)
    
    
    
    
}


FindAnnotation <- function (event.bounds, tags) {
    # given an event with the following columns
    # date, min, sec, top.f, bottom.f,
    # checks if 
    # it is contained within an annotation, or nearly within an annotation
    # if so, returns the species id, if not returns -
    
    Dot()
    
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