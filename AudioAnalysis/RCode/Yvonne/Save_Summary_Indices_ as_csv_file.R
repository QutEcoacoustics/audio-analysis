# Date: 26 June 2015
# R version: 3.2.1
# Reads in the Towsey_summary_indices an saves to csv.  
# This file is used with Plot_Towsey_Summary_Indices.R

######## You may wish to change these ###################### 
#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt")
#setwd("C:\\Work\\CSV files\\2015Jul01-120417\\GympieNP")
#setwd("C:\\Work\\CSV files\\2015Jul01-120417\\Woondum3")
#setwd("C:\\Work\\CSV files\\2015Jul01-120417\\Woondum3")
setwd("C:\\Work\\CSV files\\GympieNP\\2015_06_28\\")

#sourceDir <- "F:\\Cooloola\\2015_03_15\\20150322_20150320_Woondum1\\"
#sourceDir <- "F:\\Cooloola\2015_03_22\\20150322_20150327_Woondum2\\"
#sourceDir <- "F:\\Cooloola\\2015_06_21\\20150622_20150628_GympieNP\\"
#sourceDir <- "F:\\Cooloola\2015_06_21\\20150622_20150628_Woondum3\\"
#sourceDir <- "F:\\Cooloola\\2015_06_28\\20150628_20150705_GympieNP\\"
#sourceDir <- "F:\\Cooloola\\2015_06_28\\20150628_20150705_Woondum3\\"
#sourceDir <- "F:\\Cooloola\\2015_07_05\\20150705_20150712_GympieNP\\"
#sourceDir <- "F:\\Cooloola\\2015_07_05\\20150705_20150712_Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_06_28\\20150628_20150705_GympieNP\\"

#folder <- "F:\\Work\\Data\\2015Mar26-134159 - Yvonne, Towsey.Indices, ICD=60.0, #14\\Yvonne\\Wet Eucalypt\\"
#folder <- "F:\\Work\\Data\\2015Mar31-134325 - Yvonne, Towsey.Indices, ICD=60, #17\\Yvonne\\Eastern Euclalypt\\"
#folder <- "F:\\Work\\Data\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\"
#folder <- "F:\\Work\\Data\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\Woondum3\\"
folder <- "F:\\Indices\\2015Jul10-163333 - Yvonne, Indices, ICD=60.0, #40\\Yvonne\\Cooloola\\2015July5\\GympieNP\\"

site <- "Gympie NP "
#site <- "Woondum3" 

### GENERATE A LIST OF DATES AND TIMES ##############
source("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\shared\\dateTime_function.R")
dt <- dateTime(myFiles)
dates <- dt[,1]
times <- dt[,2]

#dates <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\1', myFiles)
#times <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\2', myFiles)

date <- dates[1]
###################################
myFiles <- list.files(full.names=FALSE, pattern="*.wav", path=sourceDir)
# obtain a list of the original wave files

write.csv(myFiles, file=paste(site, "_", date, "_", "mapping.csv", sep = ""))

myFiles <- read.csv(file=paste(site, "_", date, "_", "mapping.csv", sep = ""))[,2]

length <- length(myFiles)

### SUMMARY INDICES ###########################
all.indices <- NULL
numberRows <- NULL
rec.date <- NULL
rec.time <- NULL

for (i in 1:length) {
  pathName <- (paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                     sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.Indices.csv", 
                     sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1,4)), "-", (substr(dates[i], 5,6)), "-",
                             (substr(dates[i], 7,8)), " ", (substr(times[i], 1,2)), ":", 
                             (substr(times[i], 3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  rec.date1 <- paste(substr(dateTimeOfRecordSequence, 9,10),
                     substr(dateTimeOfRecordSequence, 5,7), "-",
                     substr(dateTimeOfRecordSequence, 1,4), sep = "")
  rec.date <- c(rec.date, rec.date1)
  rec.time1 <- paste(substr(dateTimeOfRecordSequence, 12,24))
  rec.time <- c(rec.time,rec.time1)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time)
rm(fileContents)

######## CREATE MINUTE REFERENCE ########################
# Midnight == 0
# Midday == 720

library(chron)
dts <- dates (c(as.character(all.indices$rec.date)),
              format = c(dates = "d-m-y"))
ref.date <- dates (c(as.character(all.indices$rec.date[1])),
              format = c(dates = "d-m-y"))
hms <- times(c(as.character(all.indices$rec.time)))
hrs <- hours(hms)
m <- minutes(hms)
d <- days(dts)
a <- length(dts)
time.since.start <- NULL 

# generate the minutes of the day reference
minute.of.day <- hrs*60 + m

# generate an absolute minutes reference from start
# allowing data with missing sections to be plotted
for (i in 1:a) {
  reference <- (((dts[i] - ref.date) * (max(minute.of.day)+1)) + (minute.of.day[i]) - minute.of.day[1])
  time.since.start <- c(time.since.start, reference[1])
}
  
all.indices <- cbind(all.indices, minute.of.day, time.since.start)

###### SUMMARY INDICES #############################
write.csv(all.indices,
          file=paste("Towsey_Summary_Indices_", location,
          sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
          myFiles[length]),".csv"))

### SPECTRAL INDEX FOR ACI ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
         sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.ACI.csv", 
         sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices, file=paste("Towsey.Acoustic.ACI", location, 
      sub("*.wav", "\\1", myFiles[1]), "to", sub("*.wav","\\1", 
      myFiles[length]), ".csv", sep =""))

### SPECTRAL INDEX FOR BACKGROUND ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.BGN.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.BGN", location, 
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv", sep =""))

### SPECTRAL INDEX FOR COVER ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.CVR.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.CVR", location, 
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv", sep =""))

### SPECTRAL INDEX FOR DIF ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.DIF.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.DIF", location, 
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv", sep =""))

### SPECTRAL INDEX FOR ENT ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.ENT.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.ENT", location, 
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv", sep =""))

### SPECTRAL INDEX FOR EVN ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.EVN.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.EVN", location, 
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv", sep =""))

### SPECTRAL INDEX FOR POW ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.POW.csv", 
                   sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.POW", location, 
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv", sep =""))

### SPECTRAL INDEX FOR SPT ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.SPT.csv", 
                   sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.SPT", location, 
                    sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
                    myFiles[length]),".csv", sep =""))

### SPECTRAL INDEX FOR SUM ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "__Towsey.Acoustic.SUM.csv", 
                   sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.SUM", location, 
                sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
                myFiles[length]),".csv", sep =""))