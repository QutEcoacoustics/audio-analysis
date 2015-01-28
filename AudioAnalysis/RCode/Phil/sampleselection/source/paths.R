

paths <- list(
    audio = c("/Volumes/My Passport/Phil#61/Audio/OriginalAudio/TaggedRecordings",
                     "/Volumes/files/qut_data/Phil#61/Audio/OriginalAudio/TaggedRecordings"),
    cache = c('/Volumes/files/qut_data/cache')
    )


# path to cache




Path <- function (path.name) {  
    # looks for the path in the paths list
    # if muliple paths are supplied for the pathname, will return the first one
    # this is used so that different hard drives can be used (for home and work)
    path <- paths[[path.name]][match(TRUE, file.exists(paths[[path.name]]))]
    return(path)
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
