
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
  require('plyr');
  
  events <- as.data.frame(read.csv(OutputPath('clusters')));
  
  # limit the number of events for dev
  if (num.rows.to.use != FALSE && num.rows.to.use < nrow(events)) {
    events <- events[1:num.rows.to.use, ];
  } else {
    num.rows.to.use <- nrow(events);
  }
  
  if (g.num.samples > num.rows.to.use) {
    stop(paste0("Number of samples is more than number of events. num.samples = ", g.num.samples, ". num events = ", num.rows.to.use));
  }
  
  # adds a column which denotes which minute of the day the event happened in
  events <- SetMinute(events);
  
  minute.col <- ncol(events);
  group.col <- minute.col - 1;
  date.site.cols <- match(c('date', 'site'), g.events.col.names)
  
  # the number of the columns with the site, date, and minute of the day
  # to identify a unique minute recording
  id.cols <- c(date.site.cols, minute.col);
  
  # number of events in each minute
  # 4 column dataframe: the three id columns and the frequency
  # minutes with zero events are discarded
  Report(5, 'calculating number of events in each minute');
  num.events.per.min <- count(as.data.frame(events[,id.cols]));
  Report(4, nrow(num.events.per.min), 'minutes have at least one event');
  
  # list of unique group-minute pairs (i.e. remove duplicate groups from the same minute)
  unique.cluster.minutes <- unique(events[, c(id.cols, group.col)]);
  num.clusters.per.min <- count(unique.cluster.minutes[,1:length(id.cols)]);
  Report(4, nrow(unique.cluster.minutes), 'cluster - minutes');
  Report(4, nrow(num.clusters.per.min), '');
  
  
  mins <- cbind(num.events.per.min, num.clusters.per.min[,ncol(num.clusters.per.min)]);
  
  col.names <- c('date', 'site', 'min','num.events','num.clusters');
  colnames(mins) <- col.names;
  
  total <- mins$num.events + mins$num.clusters;
  mins <- cbind(mins, total);
  
  mins.sorted <- mins[order(mins[,ncol(mins)], decreasing = TRUE),]
  
  if (nrow(mins.sorted) < g.num.samples) {
      g.num.samples <- nrow(mins.sorted);
  }
  
  selection <- mins.sorted[1:g.num.samples, ];
  
  write.csv(selection, OutputPath('selected_samples'), row.names = FALSE);
  
  return(selection); 
  
}





CountSpecies <- function (selected.samples, speciesmins) {
    found.species <- c();
    for (i in 1:nrow(selected.samples)) {
        cond <- speciesmins$start_date==selected.samples$date[i] & 
                speciesmins$site==selected.samples$site[i] &
                speciesmins$min==selected.samples$min[i];
        hits <- speciesmins[which(cond), ]  
        
        if (nrow(hits) > 0) {
          found.species = c(found.species, as.vector(hits$species_id))
        }
    }
    found.species <- unique(found.species)  
    total.species <- unique(speciesmins$species_id);
    Report(1, 'number of species found = ', length(found.species));   
    if (length(found.species) > 0) {
        Report(2, 'species list:');
        Report(2, found.species);
    }
    if (length(total.species) == 0) {
        percent = 100;
    } else {
        percent = length(found.species)*100/length(total.species);
    }
    Report(1, percent,"% of ", length(total.species)," species present");
}

EvaluateSamples <- function (samples) {
  # given a list of minutes, finds the number of species that 
  # appear in those minutes. 
  # also finds the number of total species that appear in between
  # the processed dates at the processed sites
  
  #speciesmins <- read.csv(g.species.path);
  
  tag.fields <- c('start_date', 'start_time', 'site', 'species_id');  
  tags <- GetTags(tag.fields);
  
  date.col <- match('start_date', colnames(tags));
  time.col <- match('start_time', colnames(tags));
    
  # we have the samples as the minute number within the day (eg 1000 = 4:40pm)
  # need to transforme the date_time of speciesmins to the same format
  
  min.nums <- apply(tags, 1, function (tag, date.col, time.col) {
    sdt <- strptime(paste(tag[date.col], tag[time.col]), format = '%Y-%m-%d %H:%M:%S')
    hour <- format(sdt, format = '%H');
    min <- format(sdt, format = '%M');
    minnum <- as.numeric(hour) * 60 + as.numeric(min) 
    return(minnum)
  }, date.col, time.col);

  tags <- as.data.frame(cbind(tags, min.nums));
  colnames(tags) <- c(tag.fields,'min');
  CountSpecies(samples, tags);
  
  Report(3, "Saving spectrograms of samples with events.");
  InspectSamples();
  
  
}





SetMinute <- function (events)  {
  #  for a list of events which contains the filename (which has the start time for the file encoded)
  #  and the start time of the event, works out the minute of the day that the event happened in

  start.sec.col <- which( colnames(events)=="start.sec" );
  #print(start.sec.col);
  min <- apply(events, 1, function (v) {

    #part <- unlist(strsplit(v[2], '.', fixed=TRUE));
    #part <- as.numeric(part[3]);
    sec <- as.numeric(unlist(v[start.sec.col]));
    min <- floor(sec / 60)
    return (min);
  });
  
  
  new <- cbind(events, min)
  
  return (new);
  
}



# maybe move this somewhere else
InspectSamples <- function (samples = NA) {
    if(is.na(samples)) {
        samples = read.csv(OutputPath('selected_samples'));
    }
    events <- read.csv(OutputPath('clusters'), stringsAsFactors=FALSE);
    events <- AssignColourToGroup(events);
    events <- AddMinuteIdCol(events);
    samples <- AddMinuteIdCol(samples);
    
    event.col <- as.data.frame(rep(NA, nrow(samples)));
    colnames(event.col) <- c('events');
    samples <- cbind(samples, event.col);
    
    
    w = 1000;
    h = length(samples * 256);
    
    #png(OutputPath('samples', ext = 'png'), width = w, height = h) 
    
    temp.dir <- TempDirectory();
    
    # file names for later use in imagemagic command to append together
    im.command.fns <- "";

    
    for (i in 1:length(samples)) {
        
        #add events which belong in this sample
        min.id <- as.character(samples$min.id[i]);
        minute.events <- events[which(events$min.id==min.id),]
        
        # offset the start sec of the event so that it is in relation to the start of the sample
        minute.events$start.sec <- minute.events$start.sec - (samples$min[i] * 60);
        
        temp.fn <- paste(i, 'png', sep = '.');
        img.path <- file.path(temp.dir, temp.fn);
        im.command.fns <- paste(im.command.fns, img.path);
        
        Sp.createTargeted(samples$site[i], samples$date[i], samples$min[i]*60, 60, img.path, minute.events);
        

        
    }
    output.file <- OutputPath('InspectSamples', ext = 'png');
    command <- paste("/opt/local/bin/convert", im.command.fns, "-append", output.file);
    
    err <- try(system(command)); # ImageMagick's 'convert'
    
}

AssignColourToGroup <- function (events) {
    
   
    
    groups <- unique(events$group);
    
    num.groups <- length(groups);
    
    colors <- rainbow(num.groups);
    
    event.colors <- events$group;
    
    for (i in 1:num.groups) {
        event.colors[event.colors == groups[i]] = colors[i];
    }
    
    event.colors <- as.data.frame(event.colors);
    colnames(event.colors) <- "rect.color";
    
    events <- cbind(events, event.colors);
    
    1:1;
    
    return(events);
    
}

AddMinuteIdCol <- function (data) {
    
    cols <- colnames(data);
    date.col <- match('date', cols);
    site.col <- match('site', cols);
    min.col <- match('min', cols);
    sec.col <- match('start.sec', cols);
    
    ids <- apply(as.matrix(data), 1, function (v) {
        

        
        if (is.na(min.col)) {
            min <- floor(as.numeric(v[sec.col]) / 60);
        } else {
            min <- v[min.col];
        }
        
        id <- paste0(v[date.col], v[site.col], min);
        return(id);
        
        
    });
    
    new.data <- cbind(data, ids);
    colnames(new.data) <- c(cols, 'min.id');
    
    return(new.data);
    
    
    
}

