# Date: 26 June 2015
# R version: 3.2.1
# Reads in the Towsey_summary_indices an saves to csv.  
# This file is used with Plot_Towsey_Summary_Indices.R

######## You may wish to change these ###################### 
setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt")

sourceDir <- "E:\\Data\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\" 
#sourceDir <- "E:\\Data\\Data 22 to 27 March 2015 Woondum Eastern Eucalypt\\"

folder <- "E:\\Work\\Data\\2015Mar26-134159 - Yvonne, Towsey.Indices, ICD=60.0, #14\\Yvonne\\Wet Eucalypt\\"
#folder <- "E:\\Work\\Data\\2015Mar31-134325 - Yvonne, Towsey.Indices, ICD=60, #17\\Yvonne\\Eastern Euclalypt\\"

####################################

myFiles <- list.files(full.names=FALSE, pattern="*.wav", path=sourceDir)
      # obtain a list of the original wave files

length<-length(myFiles)

### GENERATE A LIST OF DATES AND TIMES ##############
source("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\shared\\dateTime_function.R")
dt <- dateTime(myFiles)
dates <- dt[,1]
times <- dt[,2]
  
#dates <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\1', myFiles)
#times <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\2', myFiles)

### SUMMARY INDICES ###########################
all.indices <- NULL
numberRows <- NULL
rec.date <- NULL
rec.time <- NULL

for (i in 1:length) {
  pathName <- (paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
    sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.Indices.csv", 
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
          file=paste("Towsey_Summary_Indices",
          sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
          myFiles[length]),".csv"))

### SPECTRAL INDEX FOR ACI ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
         sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.ACI.csv", 
         sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
    file=paste("Towsey.Acoustic.ACI",
      sub("*.wav", "\\1", myFiles[1]), "to", sub("*.wav","\\1", 
      myFiles[length]), ".csv"))

### SPECTRAL INDEX FOR BACKGROUND ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.BGN.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.BGN",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR COVER ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.CVR.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.CVR",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR DIF ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.DIF.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.DIF",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR ENT ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.ENT.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.ENT",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR EVN ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.EVN.csv", 
              sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.EVN",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR POW ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.POW.csv", 
                   sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.POW",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR SPT ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.SPT.csv", 
                   sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.SPT",
                    sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
                    myFiles[length]),".csv"))

### SPECTRAL INDEX FOR SUM ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.SUM.csv", 
                   sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, minute.of.day,
                     time.since.start)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.SUM",
                sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
                myFiles[length]),".csv"))