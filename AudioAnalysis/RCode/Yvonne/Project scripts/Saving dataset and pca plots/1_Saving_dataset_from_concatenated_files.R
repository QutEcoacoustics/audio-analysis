# Date: 28 October 2015
# R version: 3.2.1
# Creates a dataset from the concatenated files for specific 
# dates and saves the dataset to csv.  
# Modified:  12 September 2016

############################################
# SAVING SUMMMARY INDICES __ GYMPIE DATASET
#############################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"

# Set sourceDir to where the wave files    
site <- "Gympie NP1 "
latitude <- "Latitude 26deg 3min 49.6sec"
longitude <- "Longitude 152deg 42min 42.3sec"
elevation <- "225m"

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20160723", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)

# Generate a list of files
myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")

# generate a file reference
file.ref <- NULL
for(i in 1:length(dates))
{
  ref <- grep(paste(dates[i]), myFiles)
  file.ref <- c(file.ref,ref)
}

# Create dataframe with all indices
all.indices <- NULL
for (i in seq_along(file.ref)) {
  Name <- myFiles[file.ref[i]]
  assign(paste("fileContents"), read.csv(Name))
  all.indices <- rbind(all.indices, fileContents)
  print(i)
}
# remove columns with little data
all.indices <- all.indices[,-c(1,2,15,17,19,25:27,29)]
write.csv(all.indices, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
          dates[1],"_",dates[length(dates)],
          "_Towsey_Indices",".csv", sep = ""))

##############################################
# SAVING SUMMMARY INDICES __ WOONDUM DATASET
##############################################
site <- "Woondum3 "
latitude <- "Latitude 26deg 16min 41.7sec"
longitude <- "Longitude 152deg 47min 51.4sec"
elevation <- "118m"

# Set folder to where the concatenated files are
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20160723", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list

# Generate a list of files
myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")

# generate a file reference
file.ref <- NULL
for(i in 1:length(dates))
{
  ref <- grep(paste(dates[i]), myFiles)
  file.ref <- c(file.ref,ref)
}

# Create dataframe with all indices
all.indices <- NULL
for (i in seq_along(file.ref)) {
  Name <- myFiles[file.ref[i]]
  assign(paste("fileContents"), read.csv(Name))
  all.indices <- rbind(all.indices, fileContents)
  print(i)
}
all.indices <- all.indices[,-c(1,2,15,17,19,25:27,29)]

write.csv(all.indices, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                dates[1], "_", dates[length(dates)],
                "_Towsey_Indices",".csv", sep = ""))

#############################################
# SAVING SPECTRAL INDICES __ GYMPIE DATASET
#############################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"

listOfSpecIndices <- c("*ACI.csv$",
                       "*BGN.csv$",
                       "*ENT.csv$",
                       "*EVN.csv$",
                       "*POW.csv$",
                       "*SPT.csv$")

# Generate a list of files
#for(j in 1:length(listOfSpecIndices)) {
for(j in 1:1) {
  myFiles <- list.files(path=folder, recursive=T, 
                        full.names=TRUE, 
                        pattern=listOfSpecIndices[j])
  
  # generate a file reference
  file.ref <- NULL
  for(i in 1:length(dates))
  {
    ref <- grep(paste(dates[i]), myFiles)
    file.ref <- c(file.ref,ref)
  }
  
  # Create dataframe with all indices
  all.indices <- NULL
  #for (i in seq_along(file.ref)) {
    for (i in seq_along(file.ref[1:3])) {
    Name <- myFiles[file.ref[i]]
    assign(paste("fileContents"), read.csv(Name))
    all.indices <- rbind(all.indices, fileContents)
    print(i)
  }
  
  write.csv(all.indices, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
           dates[1],"_",dates[length(dates)], "_",
           substr(listOfSpecIndices[j],2,4),".csv", sep = ""),
           row.names = F)
}


#############################################
# SAVING SPECTRAL INDICES __ WOONDUM DATASET
#############################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"

listOfSpecIndices <- c("*ACI.csv$",
                       "*BGN.csv$",
                       "*ENT.csv$",
                       "*EVN.csv$",
                       "*POW.csv$",
                       "*SPT.csv$")

# Generate a list of files
for(j in 1:length(listOfSpecIndices)) {
  myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                        pattern=listOfSpecIndices[j])
  
  # generate a file reference
  file.ref <- NULL
  for(i in 1:length(dates))
  {
    ref <- grep(paste(dates[i]), myFiles)
    file.ref <- c(file.ref,ref)
  }
  
  # Create dataframe with all indices
  all.indices <- NULL
  for (i in seq_along(file.ref)) {
    Name <- myFiles[file.ref[i]]
    assign(paste("fileContents"), read.csv(Name))
    all.indices <- rbind(all.indices, fileContents)
    print(i)
  }
  
  write.csv(all.indices, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
            dates[1],"_",dates[length(dates)], "_",
            substr(listOfSpecIndices[j],2,4),".csv", sep = ""),
            row.names = F)
}
#######################################
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
############################################
# SPECTRAL INDICES __ GYMPIE
# Prepare spectral indices into frequency bands
# In range1 the frequency bands are:
# 0-1000 Hz To capture planes [,2:25]
# 1000-2000 Hz [,26:48]
# 2000-4000 Hz [,49:94]
# 4000-6000 Hz [,95:140]
# 6000-8000 Hz [,141:187]
# 8000-11000 Hz [,188:255]

# In range2 the frequency bands are:
# 0-1000 Hz To capture planes [,2:24]
# 1000-3000 Hz [,25:70]
# 3000-5000 Hz [,71:117]
# 5000-7000 Hz [,118:163]
# 7000-11000 Hz [,164:255]

# the last two columns #256 and # 257 were removed because there is 
# a problem with these columns in the ACI calculation 
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
library(stringr)
site <- str_sub(folder,-8,-3)
site
myFiles_ACI <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ACI.csv$")
ACI_spect_data <- NULL
  
for(i in 1:(length(myFiles_ACI)-1)) {
  ACI_data <- read.csv(myFiles_ACI[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ACI_data[,1], 
                          ACI_0Hz = rowMeans(ACI_data[,c(2:24)]),
                          ACI_1000Hz = rowMeans(ACI_data[,c(25:47)]),
                          ACI_2000Hz = rowMeans(ACI_data[,c(48:93)]),
                          ACI_4000Hz = rowMeans(ACI_data[,c(94:140)]),
                          ACI_6000Hz = rowMeans(ACI_data[,c(141:186)]),
                          ACI_8000Hz = rowMeans(ACI_data[,c(187:255)]))
  ACI_spect_data <- rbind(ACI_spect_data, spect_avg)
  print(i)
}

write.csv(ACI_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
          dates[1],"_",dates[length(dates)],"_ACI_spect_avgs.csv", sep = ""), 
          row.names = F)
##########################################
# ACI SPECT Using range 2
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
ACI_spect_data <- NULL

for(i in 1:(length(myFiles_ACI)-1)) {
  ACI_data <- read.csv(myFiles_ACI[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ACI_data[,1], 
                          ACI_0Hz = rowMeans(ACI_data[,c(2:24)]),
                          ACI_1000Hz = rowMeans(ACI_data[,c(25:70)]),
                          ACI_3000Hz = rowMeans(ACI_data[,c(71:117)]),
                          ACI_5000Hz = rowMeans(ACI_data[,c(118:163)]),
                          ACI_7000Hz = rowMeans(ACI_data[,c(164:255)]))
  ACI_spect_data <- rbind(ACI_spect_data, spect_avg)
  print(i)
}

write.csv(ACI_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_ACI_spect_avgs_range2.csv", sep = ""), 
          row.names = F)

a <- abs(cor(ACI_spect_data[,2:6], use = "complete.obs"))
View(a)
##########################################
# ACI SPECT Using range 3
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_ACI <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ACI.csv$")

ACI_spect_data <- NULL

for(i in 1:(length(myFiles_ACI)-1)) {
  ACI_data <- read.csv(myFiles_ACI[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ACI_data[,1], 
                          ACI_0Hz = rowMeans(ACI_data[,c(2:24)]),
                          ACI_1000Hz = rowMeans(ACI_data[,c(25:47)]),
                          ACI_2000Hz = rowMeans(ACI_data[,c(48:93)]),
                          ACI_4000Hz = rowMeans(ACI_data[,c(94:140)]),
                          ACI_6000Hz = rowMeans(ACI_data[,c(141:255)]))
  ACI_spect_data <- rbind(ACI_spect_data, spect_avg)
  print(i)
}

write.csv(ACI_spect_data, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
          dates[1],"_",dates[length(dates)],
          "_ACI_spect_avgs_range3.csv", sep = ""))

#####################################
# BACKGROUND - averages of logarithmic values
#####################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_BGN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*BGN.csv$")
BGN_spect_data <- NULL

for(i in 1:(length(myFiles_BGN)-1)) {
  BGN_data <- read.csv(myFiles_BGN[i], header = T)
  spectral_averages <- NULL
  BGN_data <- 10^(BGN_data/10)
  BGN_data[,1] <- 1:1440
  spect_avg <- data.frame(ID = BGN_data[,1], 
                          BGN_0Hz = rowMeans(BGN_data[,c(2:24)]),
                          BGN_1000Hz = rowMeans(BGN_data[,c(25:47)]),
                          BGN_2000Hz = rowMeans(BGN_data[,c(48:93)]),
                          BGN_4000Hz = rowMeans(BGN_data[,c(94:140)]),
                          BGN_6000Hz = rowMeans(BGN_data[,c(141:186)]),
                          BGN_8000Hz = rowMeans(BGN_data[,c(187:255)]))
  BGN_spect_data <- rbind(BGN_spect_data, spect_avg)
  print(i)
}

BGN_spect_data[,2:length(BGN_spect_data)] <- 10*log10(abs(BGN_spect_data[,2:length(BGN_spect_data)]))

write.csv(BGN_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
          dates[1],"_",dates[length(dates)],"_BGN_spect_avgs.csv", sep = ""), 
          row.names = F)

##########################################
# BGN SPECT Using range 2
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_BGN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*BGN.csv$")
BGN_spect_data <- NULL

for(i in 1:(length(myFiles_BGN)-1)) {
  BGN_data <- read.csv(myFiles_BGN[i], header = T)
  spectral_averages <- NULL
  BGN_data <- 10^(BGN_data/10)
  BGN_data[,1] <- 1:1440
  spect_avg <- data.frame(ID = BGN_data[,1], 
                          BGN_0Hz = rowMeans(BGN_data[,c(2:24)]),
                          BGN_1000Hz = rowMeans(BGN_data[,c(25:70)]),
                          BGN_3000Hz = rowMeans(BGN_data[,c(71:117)]),
                          BGN_5000Hz = rowMeans(BGN_data[,c(118:163)]),
                          BGN_7000Hz = rowMeans(BGN_data[,c(164:255)]))
  BGN_spect_data <- rbind(BGN_spect_data, spect_avg)
  print(i)
}

BGN_spect_data[,2:length(BGN_spect_data)] <- 10*log10(abs(BGN_spect_data[,2:length(BGN_spect_data)]))

write.csv(BGN_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_BGN_spect_avgs_range2.csv", sep = ""), 
          row.names = F)

##########################################
# BGN SPECT Using range 3
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_BGN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*BGN.csv$")
BGN_spect_data <- NULL

for(i in 1:(length(myFiles_BGN)-1)) {
  BGN_data <- read.csv(myFiles_BGN[i], header = T)
  spectral_averages <- NULL
  BGN_data <- 10^(BGN_data/10)
  BGN_data[,1] <- 1:1440
  spect_avg <- data.frame(ID = BGN_data[,1], 
                          BGN_0Hz = rowMeans(BGN_data[,c(2:24)]),
                          BGN_1000Hz = rowMeans(BGN_data[,c(25:47)]),
                          BGN_2000Hz = rowMeans(BGN_data[,c(48:140)]),
                          BGN_6000Hz = rowMeans(BGN_data[,c(141:255)]))
  BGN_spect_data <- rbind(BGN_spect_data, spect_avg)
  print(i)
}

BGN_spect_data[,2:length(BGN_spect_data)] <- 10*log10(abs(BGN_spect_data[,2:length(BGN_spect_data)]))

write.csv(BGN_spect_data, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
          dates[1], "_", dates[length(dates)],
          "_BGN_spect_avgs_range3.csv", sep = ""))

##################################
# ENTROPY - Gympie
##################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_ENT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ENT.csv$")
ENT_spect_data <- NULL

for(i in 1:(length(myFiles_ENT)-1)) {
  ENT_data <- read.csv(myFiles_ENT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ENT_data[,1], 
                          ENT_0Hz = rowMeans(ENT_data[,c(2:24)]),
                          ENT_1000Hz = rowMeans(ENT_data[,c(25:47)]),
                          ENT_2000Hz = rowMeans(ENT_data[,c(48:93)]),
                          ENT_4000Hz = rowMeans(ENT_data[,c(94:140)]),
                          ENT_6000Hz = rowMeans(ENT_data[,c(141:186)]),
                          ENT_8000Hz = rowMeans(ENT_data[,c(187:255)]))
  ENT_spect_data <- rbind(ENT_spect_data, spect_avg)
  print(i)
}

write.csv(ENT_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_ENT_spect_avgs.csv", sep = ""), 
          row.names = F)

##########################################
# ENT SPECT Using range 2
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_ENT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ENT.csv$")
ENT_spect_data <- NULL

for(i in 1:(length(myFiles_ENT)-1)) {
  ENT_data <- read.csv(myFiles_ENT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ENT_data[,1], 
                          ENT_0Hz = rowMeans(ENT_data[,c(2:24)]),
                          ENT_1000Hz = rowMeans(ENT_data[,c(25:70)]),
                          ENT_3000Hz = rowMeans(ENT_data[,c(71:117)]),
                          ENT_5000Hz = rowMeans(ENT_data[,c(118:163)]),
                          ENT_7000Hz = rowMeans(ENT_data[,c(164:255)]))
  ENT_spect_data <- rbind(ENT_spect_data, spect_avg)
  print(i)
}

write.csv(ENT_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_ENT_spect_avgs_range2.csv", sep = ""), 
          row.names = F)
##################################
# ENTROPY - Gympie - each 1000 Hz
##################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_ENT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ENT.csv$")
ENT_spect_data <- NULL

for(i in 1:(length(myFiles_ENT)-1)) {
  ENT_data <- read.csv(myFiles_ENT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ENT_data[,1], 
                          ENT_0Hz = rowMeans(ENT_data[,c(2:24)]),
                          ENT_1000Hz = rowMeans(ENT_data[,c(25:47)]),
                          ENT_2000Hz = rowMeans(ENT_data[,c(48:70)]),
                          ENT_3000Hz = rowMeans(ENT_data[,c(71:93)]),
                          ENT_4000Hz = rowMeans(ENT_data[,c(94:117)]),
                          ENT_5000Hz = rowMeans(ENT_data[,c(118:140)]),
                          ENT_6000Hz = rowMeans(ENT_data[,c(141:163)]),
                          ENT_7000Hz = rowMeans(ENT_data[,c(164,186)]),
                          ENT_8000Hz = rowMeans(ENT_data[,c(187:255)]))
  ENT_spect_data <- rbind(ENT_spect_data, spect_avg)
  print(i)
}

write.csv(ENT_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_1000Hz_", 
                                dates[1],"_",dates[length(dates)],"_ENT_spect_avgs.csv", sep = ""), 
          row.names = F)

#####################################
# EVENTS
#####################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_EVN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*EVN.csv$")
EVN_spect_data <- NULL

for(i in 1:(length(myFiles_EVN)-1)) {
  EVN_data <- read.csv(myFiles_EVN[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = EVN_data[,1], 
                          EVN_0Hz = rowMeans(EVN_data[,c(2:24)]),
                          EVN_1000Hz = rowMeans(EVN_data[,c(25:47)]),
                          EVN_2000Hz = rowMeans(EVN_data[,c(48:93)]),
                          EVN_4000Hz = rowMeans(EVN_data[,c(94:140)]),
                          EVN_6000Hz = rowMeans(EVN_data[,c(141:186)]),
                          EVN_8000Hz = rowMeans(EVN_data[,c(187:255)]))
  EVN_spect_data <- rbind(EVN_spect_data, spect_avg)
  print(i)
}

write.csv(EVN_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_EVN_spect_avgs.csv", sep = ""), 
          row.names = F)

##########################################
# EVN SPECT Using range 2
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_EVN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*EVN.csv$")
EVN_spect_data <- NULL

for(i in 1:(length(myFiles_EVN)-1)) {
  EVN_data <- read.csv(myFiles_EVN[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = EVN_data[,1], 
                          EVN_0Hz = rowMeans(EVN_data[,c(2:24)]),
                          EVN_1000Hz = rowMeans(EVN_data[,c(25:70)]),
                          EVN_3000Hz = rowMeans(EVN_data[,c(71:117)]),
                          EVN_5000Hz = rowMeans(EVN_data[,c(118:163)]),
                          EVN_7000Hz = rowMeans(EVN_data[,c(164:255)]))
  EVN_spect_data <- rbind(EVN_spect_data, spect_avg)
  print(i)
}

write.csv(EVN_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_EVN_spect_avgs_range2.csv", sep = ""), 
          row.names = F)

#######################################
# POWER
#######################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_POW <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*POW.csv$")
POW_spect_data <- NULL

for(i in 1:(length(myFiles_POW)-1)) {
  POW_data <- read.csv(myFiles_POW[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = POW_data[,1], 
                          POW_0Hz = rowMeans(POW_data[,c(2:24)]),
                          POW_1000Hz = rowMeans(POW_data[,c(25:47)]),
                          POW_2000Hz = rowMeans(POW_data[,c(48:93)]),
                          POW_4000Hz = rowMeans(POW_data[,c(94:140)]),
                          POW_6000Hz = rowMeans(POW_data[,c(141:186)]),
                          POW_8000Hz = rowMeans(POW_data[,c(187:255)]))
  POW_spect_data <- rbind(POW_spect_data, spect_avg)
  print(i)
}

write.csv(POW_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
          dates[1],"_",dates[length(dates)],"_POW_spect_avgs.csv", sep = ""), 
          row.names = F)

##########################################
# POW SPECT Using range 2
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_POW <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*POW.csv$")
POW_spect_data <- NULL

for(i in 1:(length(myFiles_POW)-1)) {
  POW_data <- read.csv(myFiles_POW[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = POW_data[,1], 
                          POW_0Hz = rowMeans(POW_data[,c(2:24)]),
                          POW_1000Hz = rowMeans(POW_data[,c(25:70)]),
                          POW_3000Hz = rowMeans(POW_data[,c(71:117)]),
                          POW_5000Hz = rowMeans(POW_data[,c(118:163)]),
                          POW_7000Hz = rowMeans(POW_data[,c(164:255)]))
  POW_spect_data <- rbind(POW_spect_data, spect_avg)
  print(i)
}

write.csv(POW_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_POW_spect_avgs_range2.csv", sep = ""), 
          row.names = F)

#######################################
# SPECTRAL PEAK TRACK
#######################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_SPT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*SPT.csv$")
SPT_spect_data <- NULL

for(i in 1:(length(myFiles_SPT)-1)) {
  SPT_data <- read.csv(myFiles_SPT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = SPT_data[,1], 
                          SPT_0Hz = rowMeans(SPT_data[,c(2:24)]),
                          SPT_1000Hz = rowMeans(SPT_data[,c(25:47)]),
                          SPT_2000Hz = rowMeans(SPT_data[,c(48:93)]),
                          SPT_4000Hz = rowMeans(SPT_data[,c(94:140)]),
                          SPT_6000Hz = rowMeans(SPT_data[,c(141:186)]),
                          SPT_8000Hz = rowMeans(SPT_data[,c(187:255)]))
  SPT_spect_data <- rbind(SPT_spect_data, spect_avg)
  print(i)
}

write.csv(SPT_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_SPT_spect_avgs.csv", sep = ""), 
          row.names = F)

##########################################
# SPT SPECT Using range 2
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
myFiles_SPT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*SPT.csv$")
SPT_spect_data <- NULL

for(i in 1:(length(myFiles_SPT)-1)) {
  SPT_data <- read.csv(myFiles_SPT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = SPT_data[,1], 
                          SPT_0Hz = rowMeans(SPT_data[,c(2:24)]),
                          SPT_1000Hz = rowMeans(SPT_data[,c(25:70)]),
                          SPT_3000Hz = rowMeans(SPT_data[,c(71:117)]),
                          SPT_5000Hz = rowMeans(SPT_data[,c(118:163)]),
                          SPT_7000Hz = rowMeans(SPT_data[,c(164:255)]))
  SPT_spect_data <- rbind(SPT_spect_data, spect_avg)
  print(i)
}

write.csv(SPT_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_", 
                                dates[1],"_",dates[length(dates)],"_SPT_spect_avgs_range2.csv", sep = ""), 
          row.names = F)


############################################
# SPECTRAL INDICES __ WOONDUM
# Prepare spectral indices into frequency bands
# The frequency bands are:
# 0-1000 Hz To capture planes
# 1000-2000 Hz 
# 2000-4000 Hz
# 4000-6000 Hz
# 6000-8000 Hz
# 8000-11000 Hz

# the last two columns #256 and # 257 were removed because there is 
# a problem with these columns in the ACI calculation 
library(stringr)
site <- str_sub(folder,-8,-2)
site
####################################
# ACI
####################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
ACI_spect_data <- NULL
myFiles_ACI <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ACI.csv$")
for(i in 1:(length(myFiles_ACI)-1)) {
  ACI_data <- read.csv(myFiles_ACI[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ACI_data[,1], 
                          ACI_0Hz = rowMeans(ACI_data[,c(2:24)]),
                          ACI_1000Hz = rowMeans(ACI_data[,c(25:47)]),
                          ACI_2000Hz = rowMeans(ACI_data[,c(48:93)]),
                          ACI_4000Hz = rowMeans(ACI_data[,c(94:140)]),
                          ACI_6000Hz = rowMeans(ACI_data[,c(141:186)]),
                          ACI_8000Hz = rowMeans(ACI_data[,c(187:255)]))
  ACI_spect_data <- rbind(ACI_spect_data, spect_avg)
  print(i)
}

write.csv(ACI_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
          dates[1],"_",dates[length(dates)],"_ACI_spect_avgs.csv", sep = ""), 
          row.names = F)

#################################
#ACI range2
#################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
library(stringr)
site <- str_sub(folder,-8,-2)
site

ACI_spect_data <- NULL
myFiles_ACI <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ACI.csv$")
for(i in 1:(length(myFiles_ACI)-1)) {
  ACI_data <- read.csv(myFiles_ACI[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ACI_data[,1], 
                          ACI_0Hz = rowMeans(ACI_data[,c(2:24)]),
                          ACI_1000Hz = rowMeans(ACI_data[,c(25:70)]),
                          ACI_3000Hz = rowMeans(ACI_data[,c(71:117)]),
                          ACI_5000Hz = rowMeans(ACI_data[,c(118:163)]),
                          ACI_7000Hz = rowMeans(ACI_data[,c(164:255)]))
  ACI_spect_data <- rbind(ACI_spect_data, spect_avg)
  print(i)
}

write.csv(ACI_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                                dates[1],"_",dates[length(dates)],"_ACI_spect_avgs_range2.csv", sep = ""), 
          row.names = F)
##########################################
# ACI SPECT Using range 3
##########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_ACI <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ACI.csv$")
ACI_spect_data <- NULL

for(i in 1:(length(myFiles_ACI)-1)) {
  ACI_data <- read.csv(myFiles_ACI[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ACI_data[,1], 
                          ACI_0Hz = rowMeans(ACI_data[,c(2:24)]),
                          ACI_1000Hz = rowMeans(ACI_data[,c(25:47)]),
                          ACI_2000Hz = rowMeans(ACI_data[,c(48:93)]),
                          ACI_4000Hz = rowMeans(ACI_data[,c(94:140)]),
                          ACI_6000Hz = rowMeans(ACI_data[,c(141:255)]))
  ACI_spect_data <- rbind(ACI_spect_data, spect_avg)
  print(i)
}

write.csv(ACI_spect_data, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                dates[1],"_",dates[length(dates)],
                "_ACI_spect_avgs_range3.csv", sep = ""))

#####################################
# BACKGROUND - averages of logarithmic values
#####################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_BGN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*BGN.csv$")
BGN_spect_data <- NULL

for(i in 1:(length(myFiles_BGN)-1)) {
  BGN_data <- read.csv(myFiles_BGN[i], header = T)
  spectral_averages <- NULL
  BGN_data <- 10^(BGN_data/10)
  BGN_data[,1] <- 1:1440
  spect_avg <- data.frame(ID = BGN_data[,1], 
                          BGN_0Hz = rowMeans(BGN_data[,c(2:24)]),
                          BGN_1000Hz = rowMeans(BGN_data[,c(25:47)]),
                          BGN_2000Hz = rowMeans(BGN_data[,c(48:93)]),
                          BGN_4000Hz = rowMeans(BGN_data[,c(94:140)]),
                          BGN_6000Hz = rowMeans(BGN_data[,c(141:186)]),
                          BGN_8000Hz = rowMeans(BGN_data[,c(187:255)]))
  BGN_spect_data <- rbind(BGN_spect_data, spect_avg)
  print(i)
}

BGN_spect_data[,2:length(BGN_spect_data)] <- 10*log10(abs(BGN_spect_data[,2:length(BGN_spect_data)]))

write.csv(BGN_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
          dates[1],"_",dates[length(dates)],"_BGN_spect_avgs.csv", sep = ""), 
          row.names = F)

#################################
# BACKGROUND - RANGE 2
#################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_BGN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*BGN.csv$")
BGN_spect_data <- NULL

for(i in 1:(length(myFiles_BGN)-1)) {
  BGN_data <- read.csv(myFiles_BGN[i], header = T)
  spectral_averages <- NULL
  BGN_data <- 10^(BGN_data/10)
  BGN_data[,1] <- 1:1440
  spect_avg <- data.frame(ID = BGN_data[,1], 
                          BGN_0Hz = rowMeans(BGN_data[,c(2:24)]),
                          BGN_1000Hz = rowMeans(BGN_data[,c(25:70)]),
                          BGN_3000Hz = rowMeans(BGN_data[,c(71:117)]),
                          BGN_5000Hz = rowMeans(BGN_data[,c(118:163)]),
                          BGN_7000Hz = rowMeans(BGN_data[,c(164:255)]))
  BGN_spect_data <- rbind(BGN_spect_data, spect_avg)
  print(i)
}

BGN_spect_data[,2:length(BGN_spect_data)] <- 10*log10(abs(BGN_spect_data[,2:length(BGN_spect_data)]))

write.csv(BGN_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                                dates[1],"_",dates[length(dates)],"_BGN_spect_avgs_range2.csv", sep = ""), 
          row.names = F)
######################################
# BGN - range 3 (BGN4)
######################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_BGN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*BGN.csv$")
BGN_spect_data <- NULL

for(i in 1:(length(myFiles_BGN)-1)) {
  BGN_data <- read.csv(myFiles_BGN[i], header = T)
  spectral_averages <- NULL
  BGN_data <- 10^(BGN_data/10)
  BGN_data[,1] <- 1:1440
  spect_avg <- data.frame(ID = BGN_data[,1], 
                          BGN_0Hz = rowMeans(BGN_data[,c(2:24)]),
                          BGN_1000Hz = rowMeans(BGN_data[,c(25:47)]),
                          BGN_2000Hz = rowMeans(BGN_data[,c(48:140)]),
                          BGN_6000Hz = rowMeans(BGN_data[,c(141:255)]))
  BGN_spect_data <- rbind(BGN_spect_data, spect_avg)
  print(i)
}

BGN_spect_data[,2:length(BGN_spect_data)] <- 10*log10(abs(BGN_spect_data[,2:length(BGN_spect_data)]))

write.csv(BGN_spect_data, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                dates[1], "_", dates[length(dates)],
                "_BGN_spect_avgs_range3.csv", sep = ""))

######################################
# ENTROPY 
######################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_ENT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ENT.csv$")
ENT_spect_data <- NULL

for(i in 1:(length(myFiles_ENT)-1)) {
  ENT_data <- read.csv(myFiles_ENT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ENT_data[,1], 
                          ENT_0Hz = rowMeans(ENT_data[,c(2:24)]),
                          ENT_1000Hz = rowMeans(ENT_data[,c(25:47)]),
                          ENT_2000Hz = rowMeans(ENT_data[,c(48:93)]),
                          ENT_4000Hz = rowMeans(ENT_data[,c(94:140)]),
                          ENT_6000Hz = rowMeans(ENT_data[,c(141:186)]),
                          ENT_8000Hz = rowMeans(ENT_data[,c(187:255)]))
  ENT_spect_data <- rbind(ENT_spect_data, spect_avg)
  print(i)
}

write.csv(ENT_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                                dates[1],"_",dates[length(dates)],"_ENT_spect_avgs.csv", sep = ""), 
          row.names = F)

#######################################
# ENTROPY - Range 2
########################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_ENT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ENT.csv$")
ENT_spect_data <- NULL

for(i in 1:(length(myFiles_ENT)-1)) {
  ENT_data <- read.csv(myFiles_ENT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ENT_data[,1], 
                          ENT_0Hz = rowMeans(ENT_data[,c(2:24)]),
                          ENT_1000Hz = rowMeans(ENT_data[,c(25:70)]),
                          ENT_3000Hz = rowMeans(ENT_data[,c(71:117)]),
                          ENT_5000Hz = rowMeans(ENT_data[,c(118:163)]),
                          ENT_7000Hz = rowMeans(ENT_data[,c(164:255)]))
  ENT_spect_data <- rbind(ENT_spect_data, spect_avg)
  print(i)
}

write.csv(ENT_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                                dates[1],"_",dates[length(dates)],"_ENT_spect_avgs_range2.csv", sep = ""), 
          row.names = F)

##################################
# ENTROPY - Woondum - each 1000 Hz
##################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_ENT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ENT.csv$")
ENT_spect_data <- NULL

for(i in 1:(length(myFiles_ENT)-1)) {
  ENT_data <- read.csv(myFiles_ENT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = ENT_data[,1], 
                          ENT_0Hz = rowMeans(ENT_data[,c(2:24)]),
                          ENT_1000Hz = rowMeans(ENT_data[,c(25:47)]),
                          ENT_2000Hz = rowMeans(ENT_data[,c(48:70)]),
                          ENT_3000Hz = rowMeans(ENT_data[,c(71:93)]),
                          ENT_4000Hz = rowMeans(ENT_data[,c(94:117)]),
                          ENT_5000Hz = rowMeans(ENT_data[,c(118:140)]),
                          ENT_6000Hz = rowMeans(ENT_data[,c(141:163)]),
                          ENT_7000Hz = rowMeans(ENT_data[,c(164,186)]),
                          ENT_8000Hz = rowMeans(ENT_data[,c(187:255)]))
  ENT_spect_data <- rbind(ENT_spect_data, spect_avg)
  print(i)
}

write.csv(ENT_spect_data, row.names = F, 
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\",
                "Saving_dataset\\data\\datasets\\Woondum_1000Hz_", 
                dates[1], "_", dates[length(dates)],
                "_ENT_spect_avgs.csv", sep = ""))

###################################
# EVENTS
###################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_EVN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*EVN.csv$")
EVN_spect_data <- NULL

for(i in 1:(length(myFiles_EVN)-1)) {
  EVN_data <- read.csv(myFiles_EVN[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = EVN_data[,1], 
                          EVN_0Hz = rowMeans(EVN_data[,c(2:24)]),
                          EVN_1000Hz = rowMeans(EVN_data[,c(25:47)]),
                          EVN_2000Hz = rowMeans(EVN_data[,c(48:93)]),
                          EVN_4000Hz = rowMeans(EVN_data[,c(94:140)]),
                          EVN_6000Hz = rowMeans(EVN_data[,c(141:186)]),
                          EVN_8000Hz = rowMeans(EVN_data[,c(187:255)]))
  EVN_spect_data <- rbind(EVN_spect_data, spect_avg)
  print(i)
}

write.csv(EVN_spect_data, paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
          dates[1],"_",dates[length(dates)],"_EVN_spect_avgs.csv", sep = ""), 
          row.names = F)

#######################################
# EVENTS - range 2
#######################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_EVN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*EVN.csv$")
EVN_spect_data <- NULL

for(i in 1:(length(myFiles_EVN)-1)) {
  EVN_data <- read.csv(myFiles_EVN[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = EVN_data[,1], 
                          EVN_0Hz = rowMeans(EVN_data[,c(2:24)]),
                          EVN_1000Hz = rowMeans(EVN_data[,c(25:70)]),
                          EVN_3000Hz = rowMeans(EVN_data[,c(71:117)]),
                          EVN_5000Hz = rowMeans(EVN_data[,c(118:163)]),
                          EVN_7000Hz = rowMeans(EVN_data[,c(164:255)]))
  EVN_spect_data <- rbind(EVN_spect_data, spect_avg)
  print(i)
}

write.csv(EVN_spect_data, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                dates[1],"_",
                dates[length(dates)],
                "_EVN_spect_avgs_range2.csv", sep = ""))

##################################
# POWER
##################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_POW <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*POW.csv$")
POW_spect_data <- NULL

for(i in 1:(length(myFiles_POW)-1)) {
  POW_data <- read.csv(myFiles_POW[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = POW_data[,1], 
                          POW_0Hz = rowMeans(POW_data[,c(2:24)]),
                          POW_1000Hz = rowMeans(POW_data[,c(25:47)]),
                          POW_2000Hz = rowMeans(POW_data[,c(48:93)]),
                          POW_4000Hz = rowMeans(POW_data[,c(94:140)]),
                          POW_6000Hz = rowMeans(POW_data[,c(141:186)]),
                          POW_8000Hz = rowMeans(POW_data[,c(187:255)]))
  POW_spect_data <- rbind(POW_spect_data, spect_avg)
  print(i)
}

write.csv(POW_spect_data, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                dates[1],"_",
                dates[length(dates)],
                "_POW_spect_avgs.csv", sep = ""))

####################################
# POWER - range 2
####################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_POW <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*POW.csv$")
POW_spect_data <- NULL

for(i in 1:(length(myFiles_POW)-1)) {
  POW_data <- read.csv(myFiles_POW[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = POW_data[,1], 
                          POW_0Hz = rowMeans(POW_data[,c(2:24)]),
                          POW_1000Hz = rowMeans(POW_data[,c(25:70)]),
                          POW_3000Hz = rowMeans(POW_data[,c(71:117)]),
                          POW_5000Hz = rowMeans(POW_data[,c(118:163)]),
                          POW_7000Hz = rowMeans(POW_data[,c(164:255)]))
  POW_spect_data <- rbind(POW_spect_data, spect_avg)
  print(i)
}

write.csv(POW_spect_data, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                dates[1], "_", dates[length(dates)],
                "_POW_spect_avgs_range2.csv", sep = ""))

# SPECTRAL PEAK TRACK
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_SPT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*SPT.csv$")
SPT_spect_data <- NULL

for(i in 1:(length(myFiles_SPT)-1)) {
  SPT_data <- read.csv(myFiles_SPT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = SPT_data[,1], 
                          SPT_0Hz = rowMeans(SPT_data[,c(2:24)]),
                          SPT_1000Hz = rowMeans(SPT_data[,c(25:47)]),
                          SPT_2000Hz = rowMeans(SPT_data[,c(48:93)]),
                          SPT_4000Hz = rowMeans(SPT_data[,c(94:140)]),
                          SPT_6000Hz = rowMeans(SPT_data[,c(141:186)]),
                          SPT_8000Hz = rowMeans(SPT_data[,c(187:255)]))
  SPT_spect_data <- rbind(SPT_spect_data, spect_avg)
  print(i)
}

write.csv(SPT_spect_data, row.names = F, 
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                dates[1], "_", dates[length(dates)],
                "_SPT_spect_avgs.csv", sep = ""))

##############################################
# SPECTRAL PEAK TRACK - Range2
##############################################
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3"
myFiles_SPT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*SPT.csv$")
SPT_spect_data <- NULL

for(i in 1:(length(myFiles_SPT)-1)) {
  SPT_data <- read.csv(myFiles_SPT[i], header = T)
  spectral_averages <- NULL
  spect_avg <- data.frame(ID = SPT_data[,1], 
                          SPT_0Hz = rowMeans(SPT_data[,c(2:24)]),
                          SPT_1000Hz = rowMeans(SPT_data[,c(25:70)]),
                          SPT_3000Hz = rowMeans(SPT_data[,c(71:117)]),
                          SPT_5000Hz = rowMeans(SPT_data[,c(118:163)]),
                          SPT_7000Hz = rowMeans(SPT_data[,c(164:255)]))
  SPT_spect_data <- rbind(SPT_spect_data, spect_avg)
  print(i)
}

write.csv(SPT_spect_data, row.names = F,
          paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_", 
                dates[1], "_", dates[length(dates)],
                "_SPT_spect_avgs_range2.csv", sep = ""))