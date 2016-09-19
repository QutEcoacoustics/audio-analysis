# Date: 28 October 2015
# R version: 3.2.1
# Creates a dataset from the concatenated files on availae for specific 
# dates and saves to csv.  
# See below for the concatenation of the results
######## You may wish to change these ###################### 
# setwd("C:\\Work\\CSV files\\FourMonths\\")
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

#############################
# Concatenating files before and after 21 September 2015
# Note: Do not apply this code until the columns match
# ie. until you have manually deleted certain columns from the
# 2nd file 
setwd("C:\\Work\\CSV files\\FourMonths\\")
file1 <- read.csv("dataSet_upto20Sept2015.csv", header = T)
file2 <- read.csv("dataSet_21Sept_11Oct2015.csv", header = T)

length1 <- length(file1$X)
length2 <- length(file2$X)
total.length <- length1 + length2
file1Gympie <- file1[1:(length1/2),]
file1Woondum <- file1[((length1/2)+1):length1,]
file2Gympie <- file2[1:(length2/2),]
file2Woondum <- file2[((length2/2)+1):length2,]
minute.of.day <- rep(0:1439, total.length/1440)
site <- rep(c("GympieNP", "Woondum3"), each = total.length/2, 
            length = total.length)

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20151011", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

dates <- rep(date.list, each = 1440)
##
concat <- rbind(file1Gympie, file2Gympie, file1Woondum, file2Woondum)
concat <- cbind(concat, site, dates, minute.of.day)

write.csv(concat, "dataset_22June2015_11 Oct2015.csv", row.names = F)

#######################################################
setwd("C:\\Work\\Following_confirmation\\Dataset\\")
folder <- "C:\\Temp\\Yvonne3\\concatOutput\\GympieNP"
myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")

# generate a series of date and times with one minute intervals
start <- as.POSIXct("2015-06-22")
interval <- 1

end <- start + as.difftime(length(myFiles), units="days")

date_time <- seq(from=start, by=interval*60, to=end)

gympie_data <- NULL
for(i in 1:(length(myFiles)-1) {
  concat_data <- read.csv(myFiles[i])[c(3:14,16,18,20:24)]
  gympie_data <- rbind(gympie_data, concat_data)
}
View(gympie_data)
date_time <- date_time[1:length(gympie_data[,1])]
site <- rep("GympieNP", length(gympie_data[,1]))
gympie_data <- cbind(gympie_data, site, date_time)

folder <- "C:\\Temp\\Yvonne3\\concatOutput\\Woondum3"
myFiles_Woon <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")

# generate a series of date and times with one minute intervals
start <- as.POSIXct("2015-06-22")
interval <- 1

end <- start + as.difftime(length(myFiles_Woon), units="days")

date_time <- seq(from=start, by=interval*60, to=end)

woondum_data <- NULL
for(i in 1:(length(myFiles_Woon)-1)) {
  concat_woon_data <- read.csv(myFiles_Woon[i])[c(3:14,16,18,20:24)]
  woondum_data <- rbind(woondum_data, concat_woon_data)
}

#date_time <- date_time[1:length(woondum_data[,1])]
date_time <- date_time[1:length(woondum_data[,1])]
site <- rep("WoondumNP", length(woondum_data[,1]))
woondum_data <- cbind(woondum_data, site, date_time)
View(woondum_data)

write.csv(gympie_data, "gympie_full_dataset.csv",row.names = F)
write.csv(woondum_data, "woondum_full_dataset.csv",row.names = F)

# choose 24 fine days from the gympie np site
gympie_days <- c("2015-07-15", "2015-07-16",
                 "2015-08-17", "2015-08-18",
                 "2015-09-22", "2015-09-23",
                 "2015-10-05", "2015-10-06",
                 "2015-11-12", "2015-11-13",
                 "2015-12-14", "2015-12-15",
                 "2016-01-11", "2016-01-12",
                 "2016-02-25", "2016-02-26",
                 "2016-03-25", "2016-03-26",
                 "2016-04-21", "2016-04-22",
                 "2016-05-18", "2016-05-19",
                 "2016-06-08", "2016-06-10")
reference <- NULL
for (i in 1:length(gympie_days)) {
  a <- grep(gympie_days[i], gympie_data$date_time)
  reference <- c(reference, a)
}

gympie_days_dataset <- gympie_data[reference,]
write.csv(gympie_days_dataset, "gympie_days_dataset.csv",row.names = F)

# choose 24 fine days from the woondum np site
woondum_days <- c("2015-07-30", "2015-07-31",
                 "2015-08-01", "2015-08-04",
                 "2015-09-01", "2015-09-09",
                 "2015-09-22", "2015-10-04",
                 "2015-11-18", "2015-11-19",
                 "2015-12-09", "2015-12-10",
                 "2016-01-11", "2016-01-12",
                 "2016-02-25", "2016-02-26",
                 "2016-03-10", "2016-03-15",
                 "2016-04-06", "2016-04-09",
                 "2016-05-17", "2016-05-18",
                 "2016-06-08", "2016-06-10")
reference_woon <- NULL
for (i in 1:length(woondum_days)) {
  a <- grep(woondum_days[i], woondum_data$date_time)
  reference_woon <- c(reference_woon, a)
}

woondum_days_dataset <- woondum_data[reference_woon,]

write.csv(woondum_days_dataset, "woondum_days_dataset.csv",row.names = F)
