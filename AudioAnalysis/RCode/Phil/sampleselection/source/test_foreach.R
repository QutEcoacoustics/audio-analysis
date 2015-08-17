# test foreach
# https://www.google.com.au/webhp?sourceid=chrome-instant&ion=1&espv=2&ie=UTF-8#q=r%20foreach%20loop
# http://cran.r-project.org/web/packages/doParallel/vignettes/gettingstartedParallel.pdf

require('foreach')
library('doParallel')
ParallelTest <- function () {
    
    
    test.paths1 <- c("/Volumes/My Passport/Phil#61/Audio/OriginalAudio/TaggedRecordings/NE/4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000.mp3/4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000_80min.wav",
                     "/Volumes/My Passport/Phil#61/Audio/OriginalAudio/TaggedRecordings/NE/4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000.mp3/4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000_90min.wav")
    test.paths2 <- c("/Volumes/My Passport/Phil#61/Audio/OriginalAudio/TaggedRecordings/NE/4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000.mp3/4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000_100min.wav",
                     "/Volumes/My Passport/Phil#61/Audio/OriginalAudio/TaggedRecordings/NE/4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000.mp3/4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000_110min.wav")
    
    
    registerDoParallel(cores=2)
    
    without.par <- system.time({x1 <- foreach(path=test.paths1, id = 1:2, .combine='c') %do% test_function(path, id)})[3]
    with.par <- system.time({x2 <- foreach(path=test.paths2, id = 1:2, .combine='c') %dopar% test_function(path, id)})[3]
    
    print(paste('without', without.par))
    print(paste('with', with.par))
    
    print(x1)
    print(x2)
    
}


test_function <- function (path, id) {
    
    
    cur.spectro <- Sp.CreateFromFile(path, use.cache = FALSE)
    
    
    #something that uses much cpu
    num <- 1
    for (j in 1:5) {
        print(id)
        for (i in 1:1000000) {
            num <- sqrt(num + sqrt(i))
        }  
    }

    return(id)
}
