##############################################################################
#  A generic file to read in the Towsey_summary_indices and                  #
#  saves them as csv files                                                   #
##############################################################################
setwd("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\Yvonne")

sourceDir <- "D:\\Data\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\" 

myFiles <- list.files(full.names=FALSE, pattern="*.wav", path=sourceDir)
      # obtain a list of the original wave files

length<-length(myFiles)

folder <- "D:\\Work\\Data\\2015Mar26-134159 - Yvonne, Towsey.Indices, ICD=60.0, #14\\Yvonne\\Wet Eucalypt\\"

### GENERATE A LIST OF DATES AND TIMES ##############
dates <- subsstr(myFiles, 1, 8)
times <- substr(myFiles, 10, 15)
### SUMMARY INDICES ###########################
all.summary.indices<-NULL

for (i in 1:length) {
pathName <- (paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
  sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.Indices.csv", 
  sep =""))
assign(paste("fileContents"), read.csv(pathName))
numberRows <- nrow(fileContents)
dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                      (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                      (substr(times[i],3,4)), sep="")
dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
#dateOfRecord <- dates[i]
#timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

###################################
write.csv(all.summary.indices,
          file=paste("Towsey_Summary_Indices",
          sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
          myFiles[length]),".csv"))

### SPECTRAL INDEX FOR ACI ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
         sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.ACI.csv", 
         sep =""))
  
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
    file=paste("Towsey.Acoustic.ACI",
      sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
      myFiles[length]),".csv"))

### SPECTRAL INDEX FOR BACKGROUND ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.BGN.csv", 
              sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
          file=paste("Towsey.Acoustic.BGN",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR COVER ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.CVR.csv", 
              sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
          file=paste("Towsey.Acoustic.CVR",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR DIF ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.DIF.csv", 
              sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
          file=paste("Towsey.Acoustic.DIF",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR ENT ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.ENT.csv", 
              sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
          file=paste("Towsey.Acoustic.ENT",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR EVN ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.EVN.csv", 
              sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
          file=paste("Towsey.Acoustic.EVN",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR POW ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.POW.csv", 
                   sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
          file=paste("Towsey.Acoustic.POW",
              sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR SPT ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.SPT.csv", 
                   sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
          file=paste("Towsey.Acoustic.SPT",
                     sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
                                                              myFiles[length]),".csv"))

### SPECTRAL INDEX FOR SUM ###########################
all.summary.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), "_Towsey.Acoustic.SUM.csv", 
                   sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1, 4)), "-", (substr(dates[i],5,6)), "-",
                             (substr(dates[i],7,8)), " ", (substr(times[i],1,2)), ":", 
                             (substr(times[i],3,4)), sep="")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), len=numberRows, by="min"))
  #dateOfRecord <- dates[i]
  #timeOfRecord <- paste((substr(times[i],1,2)),":",(substr(times[i],3,4)),sep="")
  fileContents <- cbind(fileContents,dateTimeOfRecordSequence)
  all.summary.indices <- rbind(all.summary.indices, fileContents)
}

rm(fileContents)
#all.summary.indices
#View(all.summary.indices)

write.csv(all.summary.indices,
          file=paste("Towsey.Acoustic.SUM",
                sub("*.wav","\\1", myFiles[1]),"to", sub("*.wav","\\1", 
                myFiles[length]),".csv"))