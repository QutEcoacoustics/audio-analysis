# Date: 26 June 2015
# R version: 3.2.1
# Reads in the Towsey_summary_indices an saves to csv.  
# Also generates plot containing an array of
# histrograms of the indices

#  This file is #1 in the sequence:
#  *1. Save_Summary_Indices_ as_csv_file.R
#   2. Plot_Towsey_Summary_Indices.R
#   3. Correlation_Matrix.R
#   4. Principal_Component_Analysis.R
#   5. kMeans_Clustering.R
#   6. Quantisation_error.R
#   7. Distance_matrix.R
#   8. Minimising_error.R
#   9. Segmenting_image.R
#  10. Transition Matrix    
#  11. Cluster Time Series
#  12. Auto-correlation
#  13. Cross-correlation

######## You may wish to change these ###################### 
# Set to where the CSV files are to be saved
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_07_12")

# Set sourceDir to where the wavefiles files are
sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July19\\GympieNP\\"

# Set folder to where the indices files are
folder <- "F:\\Indices\\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48\\Yvonne\\Cooloola\\2015July19\\GympieNP\\"

#site <- "Woondum1 "
#latitude <- "Latitude"
#longitude <- "Longitude"
#elevation <- "m"

#site <- "Woondum2 "
#latitude <- "Latitude"
#longitude <- "Longitude"
#elevation <- "m"

site <- "Gympie NP1 "
latitude <- "Latitude 26deg 3min 49.6sec"
longitude <- "Longitude 152deg 42min 42.3sec"
elevation <- "225m"

#site <- "Woondum3 "
#latitude <- "Latitude 26deg 16min 41.7sec"
#longitude <- "Longitude 152deg 47min 51.4sec"
#elevation <- "118m"

### GENERATE A LIST OF DATES AND TIMES ##############
# Source the code for the dateTime function
source("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\shared\\dateTime_function.R")

# Obtain a list of the original wave files
myFiles <- list.files(full.names=FALSE, pattern="*.wav$", path=sourceDir)

# Call dateTime function and create lists of dates and times
dt <- dateTime(myFiles) 
dtd <- strptime(dt, "%Y%m%d %H%M%S") 
dates <- dt[,1]
times <- dt[,2]

#dates <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\1', myFiles)
#times <- sub('.*([[:digit:]]{8})_([[:digit:]]{6}).*','\\2', myFiles)

date <- dates[1]
###################################
# Create mapping file containing file names and reference info
mapping <- cbind(myFiles, latitude, longitude, elevation)
write.csv(mapping, file=paste("Mapping", site,date, ".csv", sep = "_"))

myFiles <- read.csv(file=paste("Mapping", site, date, ".csv", sep = "_"))[,2]

length <- length(myFiles)

### SUMMARY INDICES ###########################
# Read file contents of Summary Indices and collate
all.indices <- NULL
numberRows <- NULL
rec.date <- NULL
rec.time <- NULL

for (i in 1:length) {
  pathName <- (paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                     sub("*.wav","\\1", myFiles[i]), 
                     "__Towsey.Acoustic.Indices.csv", 
                     sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1,4)), "-", 
                            (substr(dates[i], 5,6)), "-",
                            (substr(dates[i], 7,8)), " ", 
                            (substr(times[i], 10,11)), ":",
                            (substr(times[i], 12,13)), ":", 
                            (substr(times[i], 14,15)), sep = "")
  dateTimeOfRecordSequence <- (seq(as.POSIXct(dateTimeOfRecord), 
                                   len = numberRows, by="min"))
  rec.date1 <- paste(substr(dateTimeOfRecordSequence, 9,10),
                     substr(dateTimeOfRecordSequence, 5,7), "-",
                     substr(dateTimeOfRecordSequence, 1,4), 
                     sep = "")
  rec.date <- c(rec.date, rec.date1)
  rec.time1 <- paste(substr(dateTimeOfRecordSequence, 12,24))
  rec.time <- c(rec.time,rec.time1)
  all.indices <- rbind(all.indices, fileContents)
}

length1 <- length(rec.date)
site1 <- rep(site, length1)
latitude <- rep(latitude, length1)
longitude <- rep(longitude, length1)
elevation <- rep(elevation, length1)
all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation)
rm(fileContents)

######## CREATE MINUTE REFERENCE ########################
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
  reference <- (((dts[i] - ref.date) * (max(minute.of.day)+1)) 
                + (minute.of.day[i]) - minute.of.day[1])
  time.since.start <- c(time.since.start, reference[1])
}

fourhour.class <- NULL
hour.class <- NULL
hour.sequence <-c("00","01","02","03","04","05","06",
                  "07","08","09","10","11","12","13",
                  "14","15","16","17","18","19","20",
                  "21","22","23","24")

for(i in 1:length(rec.time)) {
  for(j in 1:length(hour.sequence)) {
    if(substr(rec.time[i], 1,2)=="00") {
      fourhour.class[i] <- "A"  
      hour.class[i] <- "A"
    }
    if(substr(rec.time[i], 1,2)=="01") {
      fourhour.class[i] <- "A"
      hour.class [i] <- "B"
    }
    if(substr(rec.time[i], 1,2)=="02") {
      fourhour.class[i] <- "A"
      hour.class [i] <- "C"
    }
    if(substr(rec.time[i], 1,2)=="03") {
      fourhour.class[i] <- "A"
      hour.class [i] <- "D"
    }
    if(substr(rec.time[i], 1,2)=="04") {
      fourhour.class[i] <- "B"
      hour.class [i] <- "E"
    }
    if(substr(rec.time[i], 1,2)=="05") {
      fourhour.class[i] <- "B"
      hour.class [i] <- "F"
    }
    if(substr(rec.time[i], 1,2)=="06") {
      fourhour.class[i] <- "B"
      hour.class [i] <- "G"
    }
    if(substr(rec.time[i], 1,2)=="07") {
      fourhour.class[i] <- "B"
      hour.class [i] <- "H"
    }
    if(substr(rec.time[i], 1,2)=="08") {
      fourhour.class[i] <- "C"  
      hour.class[i] <- "I"
    }
    if(substr(rec.time[i], 1,2)=="09") {
      fourhour.class[i] <- "C"
      hour.class [i] <- "J"
    }
    if(substr(rec.time[i], 1,2)=="10") {
      fourhour.class[i] <- "C"
      hour.class [i] <- "K"
    }
    if(substr(rec.time[i], 1,2)=="11") {
      fourhour.class[i] <- "C"
      hour.class [i] <- "L"
    }
    if(substr(rec.time[i], 1,2)=="12") {
      fourhour.class[i] <- "D"
      hour.class [i] <- "M"
    }
    if(substr(rec.time[i], 1,2)=="13") {
      fourhour.class[i] <- "D"
      hour.class [i] <- "N"
    }
    if(substr(rec.time[i], 1,2)=="14") {
      fourhour.class[i] <- "D"
      hour.class [i] <- "O"
    }
    if(substr(rec.time[i], 1,2)=="15") {
      fourhour.class[i] <- "D"
      hour.class [i] <- "P"
    }
    if(substr(rec.time[i], 1,2)=="16") {
      fourhour.class[i] <- "E"  
      hour.class[i] <- "Q"
    }
    if(substr(rec.time[i], 1,2)=="17") {
      fourhour.class[i] <- "E"
      hour.class [i] <- "R"
    }
    if(substr(rec.time[i], 1,2)=="18") {
      fourhour.class[i] <- "E"
      hour.class [i] <- "S"
    }
    if(substr(rec.time[i], 1,2)=="19") {
      fourhour.class[i] <- "E"
      hour.class [i] <- "T"
    }
    if(substr(rec.time[i], 1,2)=="20") {
      fourhour.class[i] <- "F"
      hour.class [i] <- "U"
    }
    if(substr(rec.time[i], 1,2)=="21") {
      fourhour.class[i] <- "F"
      hour.class [i] <- "V"
    }
    if(substr(rec.time[i], 1,2)=="22") {
      fourhour.class[i] <- "F"
      hour.class [i] <- "W"
    }
    if(substr(rec.time[i], 1,2)=="23") {
      fourhour.class[i] <- "F"
      hour.class [i] <- "X"
    }
  }
}

all.indices <- cbind(all.indices, minute.of.day, 
                     time.since.start, fourhour.class,
                     hour.class)

###### SUMMARY INDICES #############################
write.csv(all.indices,
          file=paste("Towsey_Summary_Indices_", site,
          sub("*.wav","\\1", myFiles[1]),"to", 
          sub("*.wav","\\1", myFiles[length]),".csv", 
          sep = ""))

indices <- read.csv(paste("Towsey_Summary_Indices_", site,
                          sub("*.wav","\\1", myFiles[1]),"to", 
                          sub("*.wav","\\1", myFiles[length]),".csv", 
                          sep = ""),header = T)
# Generate histograms of original indices
png(
  "Histograms_of_Indices.png",
  width     = 320,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

par(mfrow=c(4,5)) 
par(mar=c(4,4,4,1))
for(i in 4:20){
  hist(indices[,i],col="red", 
       #main=paste("Index",i,sep = " "),
       main=colnames(indices[i]),
       cex.main=3, cex.axis=3,
       xlab = "", ylab = "")}
dev.off()

### SPECTRAL INDEX FOR ACI ###########################
all.indices <- NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
         sub("*.wav","\\1", myFiles[i]), 
         "__Towsey.Acoustic.ACI.csv", sep =""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)
rm(fileContents)

write.csv(all.indices, file=paste("Towsey.Acoustic.ACI_", site, 
      sub("*.wav", "\\1", myFiles[1]), "to", sub("*.wav","\\1", 
      myFiles[length]), ".csv", sep =""))

### SPECTRAL INDEX FOR BACKGROUND ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), 
              "__Towsey.Acoustic.BGN.csv", sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)

rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.BGN_", site, 
              sub("*.wav","\\1", myFiles[1]),"to", 
              sub("*.wav","\\1", myFiles[length]),
              ".csv", sep =""))

### SPECTRAL INDEX FOR COVER ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), 
              "__Towsey.Acoustic.CVR.csv", sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.CVR_", site, 
              sub("*.wav","\\1", myFiles[1]),"to", 
              sub("*.wav","\\1", myFiles[length]),
              ".csv", sep =""))

### SPECTRAL INDEX FOR DIF ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), 
              "__Towsey.Acoustic.DIF.csv", sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.DIF_", site, 
              sub("*.wav","\\1", myFiles[1]),"to", 
              sub("*.wav","\\1", myFiles[length]),".csv", 
              sep =""))

### SPECTRAL INDEX FOR ENT ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), 
              "__Towsey.Acoustic.ENT.csv", sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.ENT_", site, 
              sub("*.wav","\\1", myFiles[1]),"to", 
              sub("*.wav","\\1", myFiles[length]),".csv", 
              sep =""))

### SPECTRAL INDEX FOR EVN ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
              sub("*.wav","\\1", myFiles[i]), 
              "__Towsey.Acoustic.EVN.csv", sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.EVN_", site, 
              sub("*.wav","\\1", myFiles[1]),"to", 
              sub("*.wav","\\1", myFiles[length]),
              ".csv", sep =""))

### SPECTRAL INDEX FOR POW ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), 
                   "__Towsey.Acoustic.POW.csv", sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.POW_", site, 
              sub("*.wav","\\1", myFiles[1]),"to", 
              sub("*.wav","\\1", myFiles[length]),
              ".csv", sep =""))

### SPECTRAL INDEX FOR SPT ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), 
                   "__Towsey.Acoustic.SPT.csv", sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.SPT_", site, 
                    sub("*.wav","\\1", myFiles[1]),"to", 
                    sub("*.wav","\\1", myFiles[length]),
                    ".csv", sep =""))

### SPECTRAL INDEX FOR SUM ###########################
all.indices<-NULL

for (i in 1:length) {
  pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", 
                   sub("*.wav","\\1", myFiles[i]), 
                   "__Towsey.Acoustic.SUM.csv", 
                   sep = ""))
  assign(paste("fileContents"), read.csv(pathName))
  numberRows <- nrow(fileContents)
  all.indices <- rbind(all.indices, fileContents)
}

all.indices <- cbind(all.indices, rec.date, rec.time, site, 
                     latitude, longitude, elevation, 
                     minute.of.day, time.since.start, fourhour.class,
                     hour.class)
rm(fileContents)

write.csv(all.indices,
          file=paste("Towsey.Acoustic.SUM_", site, 
                sub("*.wav","\\1", myFiles[1]),"to", 
                sub("*.wav","\\1", myFiles[length]),
                ".csv", sep =""))

##################################
