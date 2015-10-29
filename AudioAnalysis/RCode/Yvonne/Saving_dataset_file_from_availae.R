# Date: 28 October 2015
# R version: 3.2.1
# Creates a dataset from the concatenated files on availae for specific 
# dates and saves to csv.  
# 
######## You may wish to change these ###################### 
setwd("C:\\Work\\CSV files\\FourMonths\\")
#(for mapping file)

folder <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\GympieNP"

# Set sourceDir to where the wave files 
site <- "Gympie NP1 "
latitude <- "Latitude 26deg 3min 49.6sec"
longitude <- "Longitude 152deg 42min 42.3sec"
elevation <- "225m"

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20150920", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list

myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")
# dates <- c("20150730","20150731","20150801","20150831","20150901","20150904")
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
  print(i)
}

#### WOONDUM BEFORE 21 sEPT 2015
folder <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\Woondum3"

for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list

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
  print(i)
}



#write.csv(all.indices, file=paste("dataSet_", paste(dates, collapse="_"),".csv", sep =""))
write.csv(all.indices, "dataSet_upto20Sept2015.csv")

#site <- "Woondum3 "
#latitude <- "Latitude 26deg 16min 41.7sec"
#longitude <- "Longitude 152deg 47min 51.4sec"
#elevation <- "118m"

##### FROM 21 SEPTEMBER 2015 ONWARDS

folder <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\GympieNP"

# Set sourceDir to where the wave files 
site <- "Gympie NP1 "
latitude <- "Latitude 26deg 3min 49.6sec"
longitude <- "Longitude 152deg 42min 42.3sec"
elevation <- "225m"

# generate a sequence of dates
start <-  strptime("20150921", format="%Y%m%d")
finish <-  strptime("20151011", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list

myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")
# dates <- c("20150730","20150731","20150801","20150831","20150901","20150904")
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
  print(i)
}

### wOONDUM AFTER 21 sEPT 2105
folder <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\Woondum3"

for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list

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
  print(i)
}

#write.csv(all.indices, file=paste("dataSet_", paste(dates, collapse="_"),".csv", sep =""))
write.csv(all.indices, "dataSet_21Sept_11Oct2015.csv")

#site <- "Woondum3 "
#latitude <- "Latitude 26deg 16min 41.7sec"
#longitude <- "Longitude 152deg 47min 51.4sec"
#elevation <- "118m"

