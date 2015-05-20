# test foreach
# https://www.google.com.au/webhp?sourceid=chrome-instant&ion=1&espv=2&ie=UTF-8#q=r%20foreach%20loop
# http://cran.r-project.org/web/packages/doParallel/vignettes/gettingstartedParallel.pdf

require('foreach')
library('doParallel')
ParallelTest <- function () {
    
    registerDoParallel(cores=2)
    
    without.par <- system.time({x <- foreach(i=1:5, .combine='c') %do% test_function(i)})[3]
    with.par <- system.time({x <- foreach(i=1:5, .combine='c') %dopar% test_function(i)})[3]
    
    print(paste('without', without.par))
    print(paste('with', with.par))
    
}


test_function <- function (a) {
    #something that uses much cpu
    num <- 1
    for (i in 1:10000000) {
        num <- sqrt(num + sqrt(i))
    }
    return(a + 0.5)
}
