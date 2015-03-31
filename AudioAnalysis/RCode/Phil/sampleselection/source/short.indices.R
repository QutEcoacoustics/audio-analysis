

spectral.indices <- c('ACI', 'BGN', 'CVR', 'ENT', 'EVN', 'POW')






CreateFeatureCsv <- function (site, date)  {
    # for a particular recording, takes all the different features and condenses them to 1 file, which will be input to clustering
    # rounds precision to 3 sig digits, to save space in the CSV
    # discards a given number of frequency bands from the bottom of the recording
    # keeps only averages of frequency bands, plus the variance. 
    # adds the minuteIDs
    
    
    dir <- Path('indices.1.sec')
    folder <- GetAnalysisOutputPath(site, date, dir)  
    path <- file.path(folder$path, 'Towsey.Acoustic')
    files <- GetIndexFiles(path, spectral.indices)
        
    return(files)
    
    for (f in files) {
        
        # discard bottom 6 bands and top 40 bands, as there are few species vocalizations there. 
        # 256 - 46 = 210
        
        # take averages to make frequency bands 5 bins wide
        # 210 / 5 = 42
        
        
        
    }
    

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
