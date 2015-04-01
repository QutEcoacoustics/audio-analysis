

spectral.indices <- c('ACI', 'BGN', 'CVR', 'ENT', 'EVN', 'POW')







CreateFeatureCsv <- function (site, date, discard.bottom = 6, discard.top = 40, num.bands = 42)  {
    # for a particular recording, takes all the different features and condenses them to 1 file, which will be input to clustering
    # rounds precision to 3 sig digits, to save space in the CSV
    # discards a given number of frequency bands from the bottom of the recording
    # keeps only averages of frequency bands, plus the variance. 
    # adds the minuteIDs


    
    dir <- Path('indices.1.sec')
    folder <- GetAnalysisOutputPath(site, date, dir)  
    path <- file.path(folder$path, 'Towsey.Acoustic')
    files <- GetIndexFiles(path, spectral.indices)
        
    # check that the frequency bands divide into the number of frequency bins left after top and bottom bins are discarded
    
    # determined by index calculation method. Hardcoded here for simplicity
    num.bins <- 256 
    
    remainder <- (num.bins - discard.bottom - discard.top) %% num.bands
    if (remainder > 0) {
        stop('invalid parameters')
    }
    

    
    
    for (f in 1:length(files)) {
        
        Report(5, "Reading Index", spectral.indices[f], " : ", files[f])
        csv <- read.csv(files[f])
        Report(5, 'csv contains values for minutes', csv[1,1], 'to', csv[nrow(csv),1])
        # discard bottom 6 bands and top 40 bands, as there are few species vocalizations there. 
        # 256 - 46 = 210
        # take averages to make frequency bands 5 bins wide
        # 210 / 5 = 42
        csv <- ReduceCsv(csv, discard.bottom, discard.top, num.bands)
        colnames(new.csv) <- paste0(spectral.indices[f] + colnames(new.csv))
        if (exists("joined.csv")) {
            joined.csv <- cbind(joined.csv, csv)
        } else {
            joined.csv <- csv
        }
    }
    
    # add min ids
    
    min.ids <- GetMinuteList()
    min.ids <- min.ids[min.ids$date == date & min.ids$site == site,]
    
    joined.csv$min.id <- min.ids$min.id[1:nrow(joined.csv)]
    
    params <- list(site = site, date = date, discard.bottom = 6, discard.top = 40, num.bands = 42)
    
    WriteOutput(x = joined.csv, name = 'indices', params = params)
    
    return(joined.csv)

}

ReduceCsv <- function (csv, discard.bottom = 6, discard.top = 40, num.bands = 42) {
    # for a given dataframe of spectral indices, performs the following column-reductions
    # 1) discards some rows from the bottom (very low frequencies where birds don't call)
    # 2) discards some rows from the top (high frequencies where birds don't call)
    # 3) averaging: groups nearby rows and computes averages so that the total number of 
    #               remaining rows is num.bands
    
    
    # remove the column that has the minute num
    csv <- csv[,2:ncol(csv)]
    
    # descard top and bottom
    csv <- csv[, (discard.bottom+1):(ncol(csv) - discard.top)]
    
    # take averages
    new.csv <- matrix(NA, nrow = nrow(csv), ncol = num.bands)
    
    band.width <- ncol(csv) / num.bands
    
    end.cols <- (1:num.bands)*band.width 
    start.cols <- end.cols - band.width + 1
    
    for (band in 1:num.bands) {
        
        

        Dot()
        new.csv[,band] <- apply(csv[,start.cols[band]:end.cols[band]], 1, mean)
        
    }
    
    new.csv <- as.data.frame(new.csv)
    colnames(new.csv) <- paste0(start + discard.bottom)
    
    
    return(new.csv)
    
    
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
