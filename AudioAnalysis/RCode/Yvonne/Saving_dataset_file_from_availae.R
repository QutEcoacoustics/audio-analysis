# Date: 28 October 2015
# R version: 3.2.1
# Reads in the Towsey_summary_indices an saves to csv.  
# 
# NOTE:  This code will work well with dates such as 20150426-145603+1000
# if not in this form there are lines indicated (**) in the code that will
# need adjustment

######## You may wish to change these ###################### 
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\")
#(for mapping file)

folder <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\GympieNP"

myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")
dates <- c("20150730","20150731","20150801","20150831","20150901","20150904")
file.ref <- NULL
for(i in 1:length(dates))
{
  ref <- grep(paste(dates[i]), myFiles)
  file.ref <- c(file.ref,ref)
}
file.ref

all.indices <- NULL
for (i in seq_along(file.ref)) {
  Name <- myFiles[file.ref[i]]
    assign(paste("fileContents"), read.csv(Name))
  all.indices <- rbind(all.indices, fileContents)
}

folder <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\Woondum3"

myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")
file.ref <- NULL
for(i in 1:length(dates))
{
  ref <- grep(paste(dates[i]), myFiles)
  file.ref <- c(file.ref,ref)
}
file.ref

for (i in seq_along(file.ref)) {
  Name <- myFiles[file.ref[i]]
  assign(paste("fileContents"), read.csv(Name))
  all.indices <- rbind(all.indices, fileContents)
}

write.csv(all.indices, file=paste("dataSet_", paste(dates, collapse="_"),".csv", sep =""))

# Set sourceDir to where the wave files 
#pathName <- paste(folder,"\\Yvonne\\Cooloola\\2015June28\\GympieNP\\",sep="")
#site <- "Gympie NP1 "
#latitude <- "Latitude 26deg 3min 49.6sec"
#longitude <- "Longitude 152deg 42min 42.3sec"
#elevation <- "225m"

#site <- "Woondum3 "
#latitude <- "Latitude 26deg 16min 41.7sec"
#longitude <- "Longitude 152deg 47min 51.4sec"
#elevation <- "118m"


