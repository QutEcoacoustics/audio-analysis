# contains functions to do with converting between time, second of the day, date strings etc
# should all be generalisable utility functions
#
# many of these functions probably have smarter ways to do them and might already exist as R functions in some form or another


SetTime <- function (min, sec, decimal.places = 0) {
    # given a minute of the day, and a second of the minute
    # returns a time string eg 13:12:03
    # min and sec can be vectors, and it will return a vector
    
    h <- floor(min / 60)
    m <- min - (h * 60)
    sec <- round(sec, decimal.places)
    
    # just in case it got rounded up to 60 
    m2 <- floor(sec / 60)
    sec <- sec - (m2 * 60)
    m <- m + m2
    
    h <- sprintf('%02d', h)
    m <- sprintf('%02d', m)
    
    if (decimal.places == 0) {
        width <- 2
    } else {
        width <- 3 + decimal.places
    }
    p <- paste0('%0',width,'.',decimal.places,'f')
    s <- sprintf(p, sec)
    time <- paste(h, m, s, sep = ":")
    return(time)
    
    
}

GetDateParts <- function (dates) {
    # given a set of dates, will return a list with 2 elements
    # the part of the date that is the same (eg year, or year and month)
    # and the part of the date that is different as a vector of length length(dates)
    
    # split into a matrix of nrow = length(dates) and a column for year, month and day
    all.dates <- strsplit(dates, "-")
    dates.matrix <- matrix(NA, nrow = length(all.dates), ncol = 3)
    for (r in 1:length(all.dates)) {
        dates.matrix[r,1:3]  <- as.numeric(all.dates[[r]])
    }  
    # find the part that is the same among all dates
    same <- c(FALSE, FALSE, FALSE)
    for (i in 1:3) {
        if (abs(max(dates.matrix[,i]) - min(dates.matrix[,i])) < 0.25) {
            same[i] <- TRUE
        } else {
            break()
        }
    }
    prefix <- paste(dates.matrix[1,same], collapse = "-") 
    sig <- apply(dates.matrix, 1, function (row){ 
        paste(row[!same], collapse = "-")
    })
    return(list(prefix = prefix, dates = sig, selector = !same))
}







