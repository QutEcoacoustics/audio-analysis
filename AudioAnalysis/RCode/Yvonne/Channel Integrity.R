#8 June 2016
# This code calculates the mean of the channel integrity values (zero crossing
# rate, Channel similarity and Channel Difference decibels) and produces
# a csv file containing a summary of these averages for each audio file

folderList <- c("Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015June28",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015July5",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015July12",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015July19",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015July26",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug2",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug9",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug16",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug23",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug30",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Sept6",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Sept13",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Sept20",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Sept27",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Oct04",
                "Y:\\Results\\2016Jun02-111707 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Oct11")
shortNames_Gympie <- NULL
shortNames_Woondum <- NULL

myFiles <- NULL
for(i in 1:length(folderList)) {
  files <- list.files(path=folderList[i],recursive = TRUE, 
          full.names = TRUE, 
          pattern = "*Towsey.ChannelIntegrity.Indices.csv")
  myFiles <- c(myFiles, files)
}

write.csv(myFiles, "Channel Integrity filelist.csv")

ref_Gympie <- grep(myFiles, pattern = "Gympie")
myFiles_Gympie <- myFiles[ref_Gympie]

for(j in 1:length(myFiles_Gympie)) {
  shortName_G <- substr(myFiles_Gympie[j],(nchar(myFiles_Gympie[j])+1)-57,
                 nchar(myFiles_Gympie))
  shortNames_Gympie <- c(shortNames_Gympie, shortName_G)
}

ref_Woondum <- grep(myFiles, pattern = "Woondum")
myFiles_Woondum <- myFiles[ref_Woondum]

for(k in 1:length(myFiles_Woondum)) {
  shortName_W <- substr(myFiles_Woondum[k],(nchar(myFiles_Woondum[k])+1)-57,
                        nchar(myFiles_Woondum))
  shortNames_Woondum <- c(shortNames_Woondum, shortName_W)
}

statistics_Gymp <- matrix(data="NA", ncol = 7, nrow = length(myFiles_Gympie))
statistics_Woon <- matrix(data="NA", ncol = 7, nrow = length(myFiles_Woondum))

for (i in 1:length(myFiles_Gympie)) {
  data <- read.csv(myFiles_Gympie[i])  
  zcr_l_mean <- mean(data[,1])
  zcr_l_sd <- sd(data[,1])
  zcr_r_mean <- mean(data[,2])
  zcr_r_sd <- sd(data[,2])
  chanSim_mean <- mean(data[,3])
  chanDiff_mean <- mean(data[,4])
  statistics_Gymp[i,2:7] <- c(zcr_l_mean, zcr_r_mean, 
                              zcr_l_sd, zcr_r_sd,
                              chanSim_mean, chanDiff_mean) 
}

for(i in 1:length(shortNames_Gympie)) {
nam <- paste("Gymp_", substr(shortNames_Gympie[i], 1, 15), 
      sep = "")
shortNames_Gympie[i] <- nam
}
statistics_Gymp[,1] <- shortNames_Gympie
write.csv(statistics_Gymp, "Channel integrity Gympie1.csv", row.names = FALSE)

statistics_Woon <- matrix(data="NA", ncol = 7, nrow = length(myFiles_Woondum))
for (i in 1:length(myFiles_Woondum)) {
  data <- read.csv(myFiles_Woondum[i])  
  zcr_l_mean <- mean(data[,1])
  zcr_l_sd <- sd(data[,1])
  zcr_r_mean <- mean(data[,2])
  zcr_r_sd <- sd(data[,2])
  chanSim_mean <- mean(data[,3])
  chanDiff_mean <- mean(data[,4])
  statistics_Woon[i,2:7] <- c(zcr_l_mean, zcr_r_mean, 
                              zcr_l_sd, zcr_r_sd,
                              chanSim_mean, chanDiff_mean) 
}
colnames(statistics_Woon) <- c("Filename", "mean_ZeroCrossingFractionLeft",	
                               "meanZeroCrossingFractionRight", 
                               "meanChannelSimilarity",	"meanChannelDiffDecibels")

for(i in 1:length(shortNames_Woondum)) {
  nam <- paste("Woon_", substr(shortNames_Woondum[i], 1, 15), 
               sep = "")
  shortNames_Woondum[i] <- nam
}
statistics_Woon[,1] <- shortNames_Woondum
colnames(statistics_Woon) <- c("Filename", "mean_ZeroCrossingFractionLeft",	
                               "meanZeroCrossingFractionRight", 
                               "meanChannelSimilarity",	"meanChannelDiffDecibels")

write.csv(statistics_Woon, "Channel integrity Woondum1.csv", row.names = FALSE)

gympie <- read.csv("Channel integrity Gympie.csv")
woondum <- read.csv("Channel integrity Woondum.csv")

####################################################
# collating the powspec information
####################################################
sourceDir <- "D:\\Cooloola\\"
setwd("C:\\Work\\")
# Obtain a list of the original wave files
myFiles <- list.files(full.names=TRUE, pattern="*powspec.csv$", 
                      path=sourceDir, recursive=T)
myFiles

myFilesShort <- list.files(full.names=FALSE, pattern="*powspec.csv$", 
                           path=sourceDir, recursive = T)
myFilesShort

shortNames_Gympie <- NULL
shortNames_Woondum <- NULL

ref_Gympie <- grep(myFiles, pattern = "Gympie")
myFiles_Gympie <- myFiles[ref_Gympie]

for(j in 1:length(myFiles_Gympie)) {
  shortName_G <- substr(myFiles_Gympie[j],
                        (nchar(myFiles_Gympie[j])+1)-31,
                        nchar(myFiles_Gympie))
  shortNames_Gympie <- c(shortNames_Gympie, shortName_G)
}

ref_Woondum <- grep(myFiles, pattern = "Woondum")
myFiles_Woondum <- myFiles[ref_Woondum]

for(k in 1:length(myFiles_Woondum)) {
  shortName_W <- substr(myFiles_Woondum[k],
                        (nchar(myFiles_Woondum[k])+1)-31,
                        nchar(myFiles_Woondum))
  shortNames_Woondum <- c(shortNames_Woondum, shortName_W)
}

statistics_Gymp <- matrix(data="NA", ncol = 9, nrow = length(myFiles_Gympie))

for (i in 1:length(myFiles_Gympie)) {
  data <- read.csv(myFiles_Gympie[i])  
  avg_psp_left <- mean(data[,1])
  avg_psp_right <- mean(data[,2])
  med_psp_left <- mean(data[,3])
  med_psp_right <- mean(data[,4])
  max_psp_left <- mean(data[,5])
  max_psp_right <- mean(data[,6])
  sd_psp_left <- mean(data[,7])
  sd_psp_right <- mean(data[,8])
  statistics_Gymp[i,2:9] <- c(avg_psp_left, avg_psp_right,
                              med_psp_left, med_psp_right,
                              max_psp_left, max_psp_right,
                              sd_psp_left, sd_psp_right) 
}

for(i in 1:length(shortNames_Gympie)) {
  nam <- substr(shortNames_Gympie[i], 1, 19)
  shortNames_Gympie[i] <- nam
}
colnames(statistics_Gymp) <- c("names", 
                               "avg_psp_left", "avg_psp_right", 
                               "med_psp_left", "med_psp_right", 
                               "max_psp_left", "max_psp_right", 
                               "sd_psp_left",  "sd_psp_right")
statistics_Gymp[,1] <- shortNames_Gympie
write.csv(statistics_Gymp, "Channel integrity Gympie_powspec.csv", row.names = FALSE)

statistics_Woon <- matrix(data="NA", ncol = 9, nrow = length(myFiles_Woondum))

for (i in 1:length(myFiles_Woondum)) {
  data <- read.csv(myFiles_Woondum[i])  
  avg_psp_left <- mean(data[,1])
  avg_psp_right <- mean(data[,2])
  med_psp_left <- mean(data[,3])
  med_psp_right <- mean(data[,4])
  max_psp_left <- mean(data[,5])
  max_psp_right <- mean(data[,6])
  sd_psp_left <- mean(data[,7])
  sd_psp_right <- mean(data[,8])
  statistics_Woon[i,2:9] <- c(avg_psp_left, avg_psp_right,
                              med_psp_left, med_psp_right,
                              max_psp_left, max_psp_right,
                              sd_psp_left, sd_psp_right) 
}

for(i in 1:length(shortNames_Woondum)) {
  nam <- substr(shortNames_Woondum[i], 1, 19)
  shortNames_Woondum[i] <- nam
}
colnames(statistics_Woon) <- c("names", 
                               "avg_psp_left", "avg_psp_right", 
                               "med_psp_left", "med_psp_right", 
                               "max_psp_left", "max_psp_right", 
                               "sd_psp_left",  "sd_psp_right")
statistics_Woon[,1] <- shortNames_Woondum
write.csv(statistics_Woon, "Channel integrity Woondum_powspec.csv", row.names = FALSE)


gympie <- read.csv("Channel integrity Gympie.csv")
woondum <- read.csv("Channel integrity Woondum.csv")

