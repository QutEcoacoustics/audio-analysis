# 30 June 2015
# The function uses regular expressions to generate a list of dates
# and times from the names of the files containing the date_time generated
# by the SM2

dateTime <- function(file){

dates <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\1', myFiles)
times <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\2', myFiles)
datesAndTime <- cbind(dates, times)
return(datesAndTime)
}

# The function is run with 
# source("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\shared\\dateTime_function.R")
# dt <- dateTime(myFiles)
# The dates and times can be extracted using
# dates <- dt[,1]
# times <- dt[,2]