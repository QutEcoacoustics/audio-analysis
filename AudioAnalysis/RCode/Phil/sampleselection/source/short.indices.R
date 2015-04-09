

spectral.indices <- c('ACI', 'BGN', 'CVR', 'ENT', 'EVN', 'POW')



DoClusteringWithIndices <- function (df = NULL) {
    
    if (is.null(df)) {
        res <- ReadOutput('indices')
        df <- res$data
    }
    
    clustering.result <- ClusterSegments(df) 
    res <- CreateEventGroups.kmeans(df, clustering.result)
    
    return(res)
    
}




CreateFeatureCsv <- function (site = "NE", date = "2010-10-13", include.frequencies = c(200, 8000), source.max.frequency = 22050, merge.columns = 1, resolution = 60)  {
    # for a particular recording, takes all the different features and condenses them to 1 file, which will be input to clustering
    # rounds precision to 3 sig digits, to save space in the CSV
    # discards a given number of frequency bands from the bottom of the recording
    # keeps only averages of frequency bands, plus the variance. 
    # adds the minuteIDs
    #
    # Args:
    #   site: string
    #   date: string; yyyy-mm-dd
    #   discard.bottom: int; how many frequency bins (columns) to discard from the bottom of the spectrum
    #   discard.top: int;  how many frequency bins (columns) to discard from the bottom of the spectrum
    #   merge.rows: int; to reduce dimensionality, will take averages of neighbouring columns
    #   resolution: int; how many rows per minute. i.e. 60 / the length of the segments in seconds



    
    dir <- Path('indices.1.sec')
    folder <- GetAnalysisOutputPath(site, date, dir)  
    path <- file.path(folder$path, 'Towsey.Acoustic')
    files <- GetIndexFiles(path, spectral.indices)
        
    # check that the frequency bands divide into the number of frequency bins left after top and bottom bins are discarded
    
    # num.bins is determined by index calculation method. Hardcoded here for simplicity
    num.bins <- 256 

    # calculated the rows to discard
    hz.per.row = source.max.frequency / 256;
    discard.bottom = round(include.frequencies[1] / hz.per.row)
    discard.top = round((source.max.frequency - include.frequencies[2]) / hz.per.row)

    
    for (f in 1:length(files)) {
        
        Report(5, "Reading Index", spectral.indices[f], " : ", files[f])
        csv <- read.csv(files[f])
        Report(5, 'csv contains values for minutes', csv[1,1], 'to', csv[nrow(csv),1])
        # discard bottom 6 bands and top 40 bands, as there are few species vocalizations there. 
        # 256 - 46 = 210
        # take averages to make frequency bands 5 bins wide
        # 210 / 5 = 42
        csv <- ReduceCsv(csv, discard.bottom, discard.top, merge.columns)
        colnames(csv) <- paste0(spectral.indices[f], '.', colnames(csv))
        if (exists("joined.csv")) {
            joined.csv <- cbind(joined.csv, csv)
        } else {
            joined.csv <- csv
        }
    }
    
    # add min ids
    
    joined.csv$min.id <- CreateMinIdColumn(site, date, resolution, nrow(joined.csv), start.min = 0)
    params <- list(site = site, date = date, include.frequencies = include.frequencies)
    if (merge.columns > 1) {
        params$merge.cols = merge.cols
    }
    # don't put source frequency or resolution in params as they are a property of the source file, not the params for csv merging
    WriteOutput(x = joined.csv, name = 'indices', params = params)
    
    return(joined.csv)

}


CreateMinIdColumn <- function (site, date, resolution, num.rows, start.min = 0) {
    # for a csv of indices at from recording taken at a given site, date and start minute
    # with segments of a given resolution and a given number of rows
    # returns the minute id for each row
    #
    # Args:
    #   date: string
    #   site: string
    #   resolution: int; how many rows per minute
    #   num.rows: how many rows. i.e. duration in minutes * resolution
    #   start.minute: the minute of the day of the first minute in the recording. 
    
    
    min.ids <- GetMinuteList()
    min.ids <- min.ids[min.ids$date == date & min.ids$site == site,]
    min.ids <- min.ids$min.id
    
    min.ids <- min.ids[(start.min + 1): length(min.ids)]
    
    # each min.id should be repeated 60 times, since there are 
    min.ids <- rep(min.ids, each = resolution)
    
    # the number of rows may be slightly shorter, if the recording finished before midnight
    min.ids <- min.ids[1:num.rows]   
    
    return(min.ids)
    
}

CreateEventIdColumn <- function (df) {
    
    df$event.id <- NA
    
    # event id is formatted as min.id-event.num.in.min. 
    # to make it sortable, we need to padd with zeros, which means assuming a maximum number
    # of min ids. here, we choose 6 for a maximum number of almost 1 million minutes per study
    minute.padding.length <- 6
    
    min.ids <- unique(df$min.id)
    
    for (min.id in min.ids) {
        Dot()
        selection <- df$min.id == min.id
        # num.events should always be 60 for 1-second indices
        num.events <- sum(selection) 
        # number of digits requires for event number in minute
        event.padding.length <- ceiling(log10(num.events))
        df$event.id[selection] <- paste0(ZeroPad(min.id, minute.padding.length), '-', ZeroPad((1:num.events), event.padding.length))
        
    }
    
    return(df)
    
    
    
}

ZeroPad <- function (int, len) {
    # takes a given integer and returns a string
    # of that integer left padded with zeros to make
    # a total number of characters of lenght len
    #
    # Args:
    #   int: int; the integer to pad
    #   len: int; total number or characters result string
    
    return(sprintf(paste0('%0', len, 'd'), int))
    
}


ReduceCsv <- function (csv, discard.bottom = 6, discard.top = 40, merge.columns = 1) {
    # for a given dataframe of spectral indices, performs the following column-reductions
    # 1) discards some rows from the bottom (very low frequencies where birds don't call)
    # 2) discards some rows from the top (high frequencies where birds don't call)
    # 3) averaging: groups nearby rows and computes averages so that the total number of 
    #               remaining rows is num.bands
    
    
    
    
    
    # remove the column that has the minute num
    csv <- csv[,2:ncol(csv)]
    
    # add to discart.top so that merge.columns will divide exactly into 
    # the remaining number of columns
    discard.top <- discard.top + ((ncol(csv) - discard.bottom - discard.top) %% merge.columns)
    
    # descard top and bottom
    csv <- csv[, (discard.bottom+1):(ncol(csv) - discard.top)]
    

    
    

        

        
        # the number of columns to use per frequency band
        band.width = merge.columns
        
        # we have previously ensured that this is an integer
        num.bands <- ncol(csv) / merge.columns
        
        end.cols <- (1:num.bands)*band.width 
        start.cols <- end.cols - band.width + 1

    if (merge.columns > 1) {
    
        # take averages
        new.csv <- matrix(NA, nrow = nrow(csv), ncol = num.bands)
        

        for (band in 1:num.bands) {
            Dot()
            new.csv[,band] <- apply(csv[,start.cols[band]:end.cols[band]], 1, mean)   
        }
      
        
        
    } else {
        
        new.csv <- csv
        
    }
    

    
    new.csv <- as.data.frame(new.csv)
    colnames(new.csv) <- paste0(start.cols + discard.bottom)
    
    
    return(new.csv)
    
    
}

ClusterSegments <- function (df, num.clusters = 240, normalize = TRUE) {
    # k-means clustering segments based on spectral index values
    # 
    # Args:
    #   df: dataframe. Except for 'min.id' column all columns are features. 
    #   num.clusters: vector of ints;  for each number supplied, will run kmeans clustering 
    
    Report(2, 'scaling features (m = ',  nrow(df), ')')
    ptm <- proc.time() 
    
    # remove the min.id column, so all we are left with is the feature vectors
    remove.cols <- c('min.id', 'event.id')
    features <- df[,!colnames(df) %in% remove.cols]
    features <- as.matrix(scale(features))
    
    Timer(ptm, 'scaling features')  
    
    if (is.null(num.clusters)) {
        num.clusters <- ReadInt('number of clusters for K Means', default = 240)     
    }
    
    kmeans.results <- as.list(rep(NA, length(num.clusters)))
    for (i in 1:length(num.clusters)) {
        kmeans.results[[i]] <- kmeans(features, num.clusters[i], algorithm = "Hartigan-Wong", iter.max = 30)   
    }
    
    return(kmeans.results)
    
    
}


GetIndexFiles <- function (path, indices) {
    # for each of the indices, looks in the path for a file
    # containing that code. Returns the full for each index
    
    files <- sapply(indices, function (x) {
        return(list.files(path = path, pattern = x, all.files = FALSE,
                   full.names = TRUE, recursive = FALSE,
                   ignore.case = FALSE, include.dirs = FALSE, no.. = FALSE))
    })

    
    return(files)
    
    
}
