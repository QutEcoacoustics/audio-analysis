# 30 June 2015
# A function that generates lists of dates and times

dateTime <- function(file){

dates <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\1', myFiles)
times <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\2', myFiles)
datesAndTime <- cbind(dates, times)
return(datesAndTime)
}

# The dates and times can be extracted with
# dt <- dateTime(myFiles)
# dates <- dt[,1]
# times <- dt[,2]