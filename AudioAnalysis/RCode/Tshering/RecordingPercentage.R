#R code to split time and date into two collumns for d3 calendering

#set the working directory
setwd("C:\\Users\\n8937087\\ownCloud\\Shared\\Ecoacoustics\\Tshering")

#Reading in csv file
myData <- read.csv("audio_recordings_from_site_562.csv", header=TRUE)

#selects columns 4 to 6 into a new dataframe called dataset
dataset <- data.frame(myData[,4:6])

#splits the 4th column into data and time
splitDateTime <- do.call( rbind , strsplit( as.character( dataset$recorded_date ) , " " ) )

#Appending the data and time back to dataset
newDataset <-cbind(splitDateTime, dataset)
head(newDataset)
newDataset$Col2
newDataset <- within(newDataset, PercentageofDayRecording <- signif(newDataset$duration_seconds / (24*60*60),digits = 3))
head(newDataset)
#Reordering the column names in R
finalDataset <- newDataset[c(1,6,5,2)]
head(finalDataset)
colnames(finalDataset)[1] <- "Date"
colnames(finalDataset)[2] <- "PercentRecording"

#write the output to a csv file

write.csv(finalDataset, "RecordingPercent.csv")                     
