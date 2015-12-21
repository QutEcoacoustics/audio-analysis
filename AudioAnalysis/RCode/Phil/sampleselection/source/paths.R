

paths <- list(
    audio = c("/Volumes/My Passport/Phil#61/Audio/OriginalAudio/TaggedRecordings",
             "/Volumes/files/qut_data/Phil#61/Audio/OriginalAudio/TaggedRecordings"),
    cache = c('/Volumes/PACKARDBELL/qut_spectrogram_cache',
              '/Volumes/files/qut_data/cache',
              '/Users/n8933464/Documents/sample_selection_output/cache'),
    indices.1.sec = c("/Users/n8933464/Documents/SERF/indices_1_sec")
    )
    


# path to cache




Path <- function (path.name) {  
    # looks for the path in the paths list
    # if muliple paths are supplied for the pathname, will return the first one
    # this is used so that different hard drives can be used (for home and work)
    
    
    first.match <- match(TRUE, file.exists(paths[[path.name]]))
    
    
    if (!is.na(first.match)) {
        path <- paths[[path.name]][first.match]
        return(path)
    } else {
        msg1 <- paste('defined paths for', path.name, "don't exist. ", paste(paths[[path.name]], collapse = ", "))
        stop(msg1)
    }

}


BasePath <- function (full.path, ds = "/") {
    # hack to get around full.names bug
    path <- unlist(strsplit(full.path, ds, fixed = FALSE, perl = FALSE, useBytes = FALSE))
    basepath <- path[[length(path)]]
    return(basepath)
}


FixCacheFns <- function () {
    
    dir <- Path('cache')
    
    files <- list.files(dir, pattern = NULL, all.files = FALSE,
               full.names = FALSE, recursive = FALSE,
               ignore.case = FALSE, include.dirs = FALSE)
    
    for (i in 1:length(files)) {
        old.fn <- files[i]
        new.fn <- FixCacheFn(files[i])
        if (is.character(new.fn)) {
            new.fn <- paste0(new.fn, '.spectro')
            
            old.full.path <- file.path(dir, old.fn)
            new.full.path <- file.path(dir, new.fn)
            
            file.rename(old.full.path, new.full.path)
        }
        Dot()

    }
    
    
    
    
}

FixCacheFn <- function (path) {
    # for a while there was a bug which caused the full file path 
    # to be saved in a strange way
    
    f.split <- strsplit(path, '"')[[1]]
    if (length(f.split) > 8 && f.split[8] == 'Phil#61') {
        # from work hard drive
        return(f.split[20])
    } else if (length(f.split) > 10 && f.split[10] == 'Phil#61') {
        # from home hard drive
        return(f.split[22])  
    } else if (length(f.split) > 10 && f.split[10] == 'SERF') {
        # from old audio on hard drive
        return(f.split[14])
    } else {
        # print(f.split)
        # something else, probably already fixed
        return(FALSE)
    }
    
    
}



GetAnalysisOutputPath <- function (site, date, dir) {
    # audio is in a folder structure like:
    # sitename/UID_YYMMDD-0000.mp3/UID_date-0000_0min.mp3
    
    
    site.dir <- file.path(dir, site)
    day.folders <- list.dirs(site.dir, full.names = FALSE, recursive = FALSE)
    day.folders <- sapply(day.folders, function (folder) {
        date <- unlist(strsplit(unlist(strsplit(folder, c("_")))[2], "-"))[1]
        prefix <- substr(folder,start=1,stop=(nchar(folder)-4))
        return(c(folder, DateFromShortFormat(date), prefix, file.path(site.dir, folder)))
    })
    day.folders <- as.data.frame(t(day.folders), stringsAsFactors = FALSE)
    colnames(day.folders) <- c('folder', 'date', 'prefix', 'path')
    day.folder <- day.folders[day.folders$date == date,]
    
    return(day.folder)
    
}


