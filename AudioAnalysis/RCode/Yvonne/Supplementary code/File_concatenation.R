# Concatenating csv files

setwd("C:\\Work\\CSV files\\GympieNP1_new\\all_data")

folder <- "C:\\Work\\CSV files\\GympieNP1_new\\"

myFolders <- list.files(full.names=FALSE, pattern="2015_*", path=folder)
myFolders
length <- length(myFolders)
length

all.indices <- NULL

for (i in 1:length) {
  pathName <- (paste(folder, myFolders[i], sep=""))
  myFile <- list.files(full.names=TRUE, pattern="Towsey_Summary_Indices*", 
                       path=pathName)
  assign(paste("fileContents"), read.csv(myFile, header=T))
  all.indices <- rbind(all.indices, fileContents)
}

write.csv(all.indices,
          file=paste("Towsey_Summary_Indices_current", site,
                     sub("*.wav","\\1", myFiles[1]),"to", 
                     sub("*.wav","\\1", myFiles[length]),".csv", 
                     sep = ""))
# plot boxplots
boxplot(all.indices$AvgSignalAmplitude, horizontal = T)

c<-seq(0,100,1)
quantile(indices$AvgSignalAmplitude, 98/100)

