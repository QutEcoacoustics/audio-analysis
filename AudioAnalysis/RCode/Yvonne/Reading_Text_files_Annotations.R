# Date: 20 July 2015
# R version: 3.2.1
# Reads in the Annotation Text files from Audacity and converts these
# to a time in minutes from the start of the recording.  

######## You may wish to change these ###################### 
#setwd("C:\\Work\\CSV files\\Woondum1\\2015_03_15\\")
#setwd("C:\\Work\\CSV files\\Woondum2\\2015_03_22\\")
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")

#F:/Cooloola/2015_06_21/20150622_20150628_GympieNP/Label Track 20150622_000000_GympieNP1.txt
#sourceDir <- "F:\\Cooloola\\2015_03_15\\20150322_20150320_Woondum1\\"
#sourceDir <- "F:\\Cooloola\\2015_03_22\\20150322_20150327_Woondum2\\"
sourceDir <- "F:\\Cooloola\\2015_06_21\\20150622_20150628_GympieNP\\"
#sourceDir <- "F:\\Cooloola\\2015_06_21\\20150622_20150628_Woondum3\\"
#sourceDir <- "F:\\Cooloola\\2015_06_28\\20150628_20150705_GympieNP\\"
#sourceDir <- "F:\\Cooloola\\2015_06_28\\20150628_20150705_Woondum3\\"
#sourceDir <- "F:\\Cooloola\\2015_07_05\\20150705_20150712_GympieNP\\"
#sourceDir <- "F:\\Cooloola\\2015_07_05\\20150705_20150712_Woondum3\\"

#site <- "Woondum1 "
#latitude <- "Latitude"
#longitude <- "Longitude"
#elevation <- "m"

site <- "Woondum2 "
latitude <- "Latitude"
longitude <- "Longitude"
elevation <- "m"

#site <- "Gympie NP1 "
#latitude <- "Latitude 26deg 3min 49.6sec"
#longitude <- "Longitude 152deg 42min 42.3sec"
#elevation <- "225m"

#site <- "Woondum3 "
#latitude <- "Latitude 26deg 16min 41.7sec"
#longitude <- "Longitude 152deg 47min 51.4sec"
#elevation <- "118m"

###################################
# obtain a list of the original text files
myFiles <- list.files(full.names = T, 
              pattern = ".*([[:digit:]]{8})_([[:digit:]]{6})_([[:alnum:]]{0,}).txt$", 
              path = sourceDir)
length <- length(myFiles)

dateTimes <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\1 \\2', myFiles)
site <- sub('.*([[:digit:]]{8})_([[:digit:]]{6})_([[:alnum:]]{0,}).*','\\3', myFiles)
site <- site[1]
# order dateTime
dateTimes <- strptime(dateTimes, "%Y%m%d %H%M%S")
dateTimes <- sort(dateTimes)

require(stats)
all.contents <- NULL
for (i in 1: length) {
  pathName <- myFiles[i]
  assign(paste("text"), read.table(pathName, header = T))
  numDays <- floor(as.numeric(difftime(dateTimes[i], dateTimes[1], units="days")))
  min <- (as.numeric(difftime(dateTimes[i],dateTimes[1]), units="days")-numDays) * 1440 + 
    numDays * 1440
  length2 <- length(text$start)
  for (j in 1:length2) {
  text$start[j] <- text$start[j]/60 + min
  }
  all.contents <- rbind(all.contents, text)
}

write.csv(all.contents,
          file=paste("Textfiles", site,".csv", sep = "_"))