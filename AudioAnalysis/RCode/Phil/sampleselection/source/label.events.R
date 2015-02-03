# match events to labels in the database
# not all events will match to labels, because only some vocalizations have been annotated


LabelEvents <- function (events = NULL) {
    # given a dataframe of events, assigns species id to them from the database if possible
    
    require('plyr')
    
    if (is.null(events)) {
        events <- ReadOutput('events')
    }
    
    
    start.time <- events$data$min * 60 + events$data$start.sec
    end.time <- start.time + events$data$duration
    events$data$start.time <- start.time
    events$data$end.time <- end.time
    quote <- "'"
    
    start.date.time <- paste(quote, events$data$date, " ",start.time, quote) 
    end.date.time <- paste(quote, events$data$date, " ",start.time, quote) 
    
    fields <- c('species_id', 'start_frequency', 'end_frequency')
    
    
    
    ReadTagsFromDb()
    
    
    
    species.id <- adply(events$data, 1, FindLabel)
    
    events$data$species.id <- species.id
    
#     WriteOutput(events, 'events')
    
    
    
}


FindLabel <- function (event) {
    # given an event with the following columns
    # date, min, sec, top.f, bottom.f,
    # checks if 
    # it is contained within an annotation, or nearly within an annotation
    # if so, returns the species id, if not returns -
    
    Dot()
    
    tags <- FindTag(event$site, event$date, event$start.time, event$end.time, event$bottom.f, event$top.f)
    

    
    if (nrow(tags) == 1) {
        return(tags$species.id)
    } else {
        return(-1)
    }
    
    
    
}