# Date: 26 June 2015
# R version: 3.2.1
# Reads in the Towsey_summary_indices an saves to csv.  
# Also generates plot containing an array of
# histrograms of the indices
# NOTE:  This code will work well with dates such as 20150426-145603+1000
# if not in this form there are lines indicated (**) in the code that will
# need adjustment

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
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_08_16")
#date.list <- 	c("2015June28", 
#				  "2015July5","2015July12","2015July19","2015July26",
#				  "2015Aug2","2015Aug9", "2015Aug16","2015Aug23","2015Aug30",
#				  "2015Sept6","2015Sept13","2015Sept20","2015Sept27",
#				  "2015Oct4","2015Oct11","2015Oct18","2015Oct25",
#				  "2015Nov1")
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_28")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_07_05")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_07_12")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_07_19")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_07_26")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_08_02")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_08_09")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_08_16")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_08_23")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_08_30")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_09_06")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_09_13")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_09_20")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_09_27")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_10_04")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_10_11")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_10_18")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_10_25")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_11_01")

#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_06_22")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_06_28")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_07_05")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_07_12")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_07_19")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_07_26")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_08_02")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_08_09")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_08_16")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_08_23")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_08_30")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_09_06")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_09_13")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_09_20")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_09_27")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_10_04")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_10_11")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_10_18")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_10_25")
#setwd("C:\\Work\\CSV files\\Woondum3_new\\2015_11_01")

# Set sourceDir to where the wavefiles files are 
#(for mapping file)
# this date is a week later than the date above
sourceDir <- "Y:\\Yvonne\\Cooloola\\2015June28\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July5\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July12\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July19\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July26\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug2\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug9\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug16\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug23\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug30\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Sept6\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Sept13\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Sept20\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Sept27\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Oct4\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Oct11\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Oct18\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Oct25\\GympieNP\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Nov1\\GympieNP\\"

#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015June28\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July5\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July12\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July19\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015July26\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug2\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug9\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug16\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug23\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Aug30\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Sept6\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Sept13\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Sept20\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Sept27\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Oct4\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Oct11\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Oct18\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Oct25\\Woondum3\\"
#sourceDir <- "Y:\\Yvonne\\Cooloola\\2015Nov1\\Woondum3\\"

# Set folder to where the indices files are
# containing 2015June28 to 2015Aug2
#folder <- "F:\\Indices\\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48"
# containing 2015Aug9
#folder <- "F:\\Indices\\2015Aug20-154235 - Yvonne, Indices, ICD=60.0, #50"
# containing 2015Aug16 
#folder <-  "F:\\Indices\\2015Aug20-180247 - Yvonne, Indics, ICD=60.0, #52"  
# containing 2015Aug23
#folder <- "F:\\Indices\\2015Aug31-110230 - Yvonne, Indices, ICD=60.0, #53" 
# containing 2015June28 to 2015Sept20
folder <- "F:\\Indices\\2015Sep23-154123 - Yvonne, Indices, ICD=60.0, #55, #56, #57"

# Obtain a list of the original wave files
myFiles <- list.files(full.names=FALSE, pattern="*.wav$", path=sourceDir)
myFiles

# Set sourceDir to where the wave files 
pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015June28\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015July5\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015July12\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015July19\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015July26\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug2\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug9\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug16\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug23\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug30\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Sept6\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Sept13\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Sept20\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Sept27\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Oct4\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Oct11\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Oct18\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Oct25\\GympieNP\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Nov1\\GympieNP\\",sep="")

#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015June28\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015July5\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015July12\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015July19\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015July26\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug2\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug9\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug16\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug23\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Aug30\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Sept6\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Sept13\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Sept20\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Sept27\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Oct4\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Oct11\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Oct18\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Oct25\\Woondum3\\",sep="")
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015Nov1\\Woondum3\\",sep="")

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
myFiles
length <- length(myFiles)
length
### SUMMARY INDICES ###########################
# Read file contents of Summary Indices and collate
all.indices <- NULL
numberRows <- NULL
rec.date <- NULL
rec.time <- NULL
for (i in 1:length) {
    Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                   substr(myFiles[i], 1,20),                     
                   #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.Indices.csv", 
                     sep =""))
  assign(paste("fileContents"), read.csv(Name))
  numberRows <- nrow(fileContents)
  dateTimeOfRecord  <- paste((substr(dates[i], 1,4)), "-", 
                            (substr(dates[i], 5,6)), "-",
                            (substr(dates[i], 7,8)), " ", 
                            (substr(times[i], 10,11)), ":",  
                            #(substr(times[i], 1,2)), ":",  # **use with ambiguous dates
                            (substr(times[i], 12,13)), ":",  
                            #(substr(times[i], 3,4)), ":",  # **use with ambiguous dates
                            (substr(times[i], 14,15)), sep = "") 
                            #(substr(times[i], 5,6)), sep="") # **use with ambiguous dates
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
                          sub("*.wav","\\1", myFiles[length(myFiles)]),".csv", 
                          sep = ""),header = T)
# Generate histograms of original indices
png(
  paste("Histograms_of_Indices_",site,dates,".png",sep=""),
  width     = 320,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

par(mfrow=c(4,5), xpd=TRUE) 
par(mar=c(4,3,4,1), oma=c(2,2,0,0))
for(i in 2:20) {
  hist(indices[,i], col="red", 
       #main=paste("Index",i,sep = " "),
       main=colnames(indices[i]),
       cex.main=3, cex.axis=3,
       xlab = "", ylab = "")
  if(max(indices[,i]) >= 0) {
    text((0.7*max(indices[,i])),1000,
         paste("max", signif(max(indices[,i]), digits=3)), 
         cex=4)
  } else {
    text((2*max(indices[,i])),1000,
           paste("max", signif(max(indices[,i]), digits=3)), 
           cex=4)
  }
}
plot(2, axes=F,xlim=c(0,1), ylim=c(0,1), xlab = "",
     ylab = "")
legend("center", c(paste(dates[1]), paste(dates[length(dates)])),
       title = paste(site), cex = 4,
       bty="n",xjust = 0.5)
dev.off()

### SPECTRAL INDEX FOR ACI ###########################
all.indices <- NULL

for (i in 1:length) {
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                 substr(myFiles[i], 1,20),    
                 #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.ACI.csv", sep =""))
  assign(paste("fileContents"), read.csv(Name))
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
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                 substr(myFiles[i], 1,20),
                 #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.BGN.csv", sep = ""))
  assign(paste("fileContents"), read.csv(Name))
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
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                 substr(myFiles[i], 1,20),
                 #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.CVR.csv", sep = ""))
  assign(paste("fileContents"), read.csv(Name))
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
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                 substr(myFiles[i], 1,20),
                 #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.DIF.csv", sep = ""))
  assign(paste("fileContents"), read.csv(Name))
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
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                 substr(myFiles[i], 1,20),
                 #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.ENT.csv", sep = ""))
  assign(paste("fileContents"), read.csv(Name))
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
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                    substr(myFiles[i], 1,20),
                    #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.EVN.csv", sep = ""))
  assign(paste("fileContents"), read.csv(Name))
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
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                 substr(myFiles[i], 1,20),
                 #substr(myFiles[i], 1,15), # **use with ambiguous dates
                    "__Towsey.Acoustic.POW.csv", sep = ""))
  assign(paste("fileContents"), read.csv(Name))
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
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                 substr(myFiles[i], 1,20),
                 #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.SPT.csv", sep = ""))
  assign(paste("fileContents"), read.csv(Name))
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
  Name <- (paste(pathName, myFiles[i],"\\Towsey.Acoustic\\",
                 substr(myFiles[i], 1,20), 
                 #substr(myFiles[i], 1,15), # **use with ambiguous dates
                     "__Towsey.Acoustic.SUM.csv", 
                   sep = ""))
  assign(paste("fileContents"), read.csv(Name))
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
