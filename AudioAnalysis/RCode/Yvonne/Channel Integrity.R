# 15 June 2016 Channel Integrity test
# 
gympie <- read.csv("C:\\Work\\weka channel integrity separate gympie test.csv")
woondum <- read.csv("C:\\Work\\weka channel integrity separate woondum test.csv")
all <- rbind(gympie, woondum)
write.csv(all, "full Weka dataset2.csv")

#gympie_affected <- grep("left_affected", gympie$label)
#gympie_left <- gympie[gympie_affected,] 

#gympie_affected <- grep("right_affected", gympie$label)
#gympie_right <- gympie[gympie_affected,] 

#gympie_affected <- grep("both", gympie$label)
#gympie_both <- gympie[gympie_affected,] 

#gympie_not_affected <- grep("not", gympie$label)
#gympie_not <- gympie[gympie_not_affected,] 

#woondum_affected <- grep("left_affected", woondum$label)
#woondum_left <- woondum[woondum_affected,] 

#woondum_affected <- grep("right_affected", woondum$label)
#woondum_right <- woondum[woondum_affected,] 

#woondum_affected <- grep("both", woondum$label)
#woondum_both <- woondum[woondum_affected,] 

#woondum_not_affected <- grep("not", woondum$label)
#woondum_not <- woondum[woondum_not_affected,] 

#8 June 2016
# This code calculates the mean of the channel integrity values (zero crossing
# rate, Channel similarity and Channel Difference decibels) and produces
# a csv file containing a summary of these averages for each audio file
# It also collates the powspec data (see line 110)


folderList <- c("Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015June28\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015July5\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015July12\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015July19\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015July26\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug2\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug9\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug16\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug23\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Aug30\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Sept6\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Sept13\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Sept20\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Sept27\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Oct04\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Oct11\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Oct18\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Oct25\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Nov1\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Nov8\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Nov15\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Nov22\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Nov29\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Dec6\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Dec13\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Dec20\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2015Dec29\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Jan07\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Jan16\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Jan24\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Jan31\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Feb07\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Feb14\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Feb21\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Feb28\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016March06\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Mar13\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Mar20\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016Mar29\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016April03\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016April10\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016April17\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016April24\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016May01\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016May08\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016May15\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016May22\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016May29\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016June05\\",
                "Y:\\Results\\2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95\\Yvonne\\Cooloola\\2016June12\\")
shortNames_Gympie <- NULL
shortNames_Woondum <- NULL
setwd("C:\\Work\\Statistics\\")

myFiles <- NULL
for(i in 1:length(folderList)) {
  files <- list.files(path=folderList[i], recursive = TRUE, 
          full.names = TRUE, 
          pattern = "*Towsey.ChannelIntegrity.Indices.csv")
  myFiles <- c(myFiles, files)
}
myFiles
write.csv(myFiles, "Channel Integrity filelist.csv")

ref_Gympie <- grep(myFiles, pattern = "Gympie")
myFiles_Gympie <- myFiles[ref_Gympie]
myFiles_Gympie

for(j in 1:length(myFiles_Gympie)) {
  shortName_G <- substr(myFiles_Gympie[j],(nchar(myFiles_Gympie[j])+1)-57,
                 nchar(myFiles_Gympie))
  shortNames_Gympie <- c(shortNames_Gympie, shortName_G)
}
shortNames_Gympie

ref_Woondum <- grep(myFiles, pattern = "Woondum")
myFiles_Woondum <- myFiles[ref_Woondum]
myFiles_Woondum

for(k in 1:length(myFiles_Woondum)) {
  shortName_W <- substr(myFiles_Woondum[k],(nchar(myFiles_Woondum[k])+1)-57,
                        nchar(myFiles_Woondum))
  shortNames_Woondum <- c(shortNames_Woondum, shortName_W)
}
shortNames_Woondum

#statistics_Gymp <- matrix(data="NA", ncol = 8, nrow = length(myFiles_Gympie))
statistics_Gymp <- matrix(data="NA", ncol = 14, nrow = length(myFiles_Gympie))

for (i in 1:length(myFiles_Gympie)) {
  data <- read.csv(myFiles_Gympie[i])  
  zcr_l_mean <- mean(data[,1])
  zcr_l_sd <- sd(data[,1])
  a <- range(data[,1])
  zcr_l_range <- a[2]-a[1]
  a <- NULL
  zcr_r_mean <- mean(data[,2])
  zcr_r_sd <- sd(data[,2])
  a <- range(data[,2])
  zcr_r_range <- a[2]-a[1]
  a <- NULL
  chanSim_mean <- mean(data[,3])
  # I could add in the standard deviation of the Channel Similarity
  chanSim_sd <- sd(data[,3])
  a <- range(data[,3])
  chanSim_range <- a[2]-a[1]
  a <- NULL
  chanDiff_mean <- mean(data[,4])
  # I could add in the standard deviation of the Channel Difference
  chanDiff_sd <- sd(data[,4])
  a <- range(data[,4])
  chanDiff_range <- a[2]-a[1]
  a <- NULL
  chanBias_dB <- mean(data[,5])
  # I could add in the standard deviation of the Channel Bias
  chanBias_dB_sd <- sd(data[,5])
  a <- range(data[,5])
  chanBias_dB_range <- a[2]-a[1]
  a <- NULL
  #statistics_Gymp[i,2:8] <- c(zcr_l_mean, zcr_r_mean, 
  #                            zcr_l_sd, zcr_r_sd,
  #                            chanSim_mean, chanDiff_mean,
  #                            chanBias_dB) 
  statistics_Gymp[i,2:14] <- c(zcr_l_mean, zcr_r_mean, 
                              zcr_l_sd, zcr_r_sd,
                              chanSim_mean, chanSim_sd, chanSim_range,
                              chanDiff_mean, chanDiff_sd, chanDiff_range,
                              chanBias_dB, chanBias_dB_sd, chanBias_dB_range) 
}

for(i in 1:length(shortNames_Gympie)) {
nam <- paste("Gymp_", substr(shortNames_Gympie[i], 1, 15), 
      sep = "")
shortNames_Gympie[i] <- nam
}
statistics_Gymp[,1] <- shortNames_Gympie
#colnames(statistics_Gymp) <- c("Filename", "mean_ZeroCrossingFractionLeft",	
#                               "meanZeroCrossingFractionRight", "standardDevZCR_left",
#                               "standardDevZCR_right","meanChannelSimilarity",
#                               "meanChannelDiffDecibels","meanChannel_Bias")

colnames(statistics_Gymp) <- c("Filename", "mean_ZeroCrossingFractionLeft",	
                               "meanZeroCrossingFractionRight", "standardDevZCR_left",
                               "standardDevZCR_right","meanChannelSimilarity",
                               "standardDevChanSimilarity", "rangeDevChanSimilarity", 
                                "meanChannelDiffDecibels",
                               "standardDevChannelDiffDecibels", "rangeChannelDiffDecibels",
                                "meanChannel_Bias", "standardDevChannel_Bias", 
                               "rangeChannel_Bias")

write.csv(statistics_Gymp, "Channel integrity Gympie with chan_Bias_sd.csv", row.names = FALSE)

#statistics_Woon <- matrix(data="NA", ncol = 8, nrow = length(myFiles_Woondum))
statistics_Woon <- matrix(data="NA", ncol = 14, nrow = length(myFiles_Woondum))

for (i in 1:length(myFiles_Woondum)) {
  data <- read.csv(myFiles_Woondum[i])  
  zcr_l_mean <- mean(data[,1])
  zcr_l_sd <- sd(data[,1])
  a <- range(data[,1])
  zcr_l_range <- a[2]-a[1]
  a <- NULL
  zcr_r_mean <- mean(data[,2])
  zcr_r_sd <- sd(data[,2])
  a <- range(data[,2])
  zcr_r_range <- a[2]-a[1]
  a <- NULL
  chanSim_mean <- mean(data[,3])
  # I could add in the standard deviation of the Channel Similarity
  chanSim_sd <- sd(data[,3])
  a <- range(data[,3])
  chanSim_range <- a[2]-a[1]
  a <- NULL
  chanDiff_mean <- mean(data[,4])
  # I could add in the standard deviation of the Channel Difference
  chanDiff_sd <- sd(data[,4])
  a <- range(data[,4])
  chanDiff_range <- a[2]-a[1]
  a <- NULL
  chanBias_dB <- mean(data[,5])
  # I could add in the standard deviation of the Channel Bias
  chanBias_dB_sd <- sd(data[,5])
  a <- range(data[,5])
  chanBias_dB_range <- a[2]-a[1]
  a <- NULL
  #statistics_Gymp[i,2:8] <- c(zcr_l_mean, zcr_r_mean, 
  #                            zcr_l_sd, zcr_r_sd,
  #                            chanSim_mean, chanDiff_mean,
  #                            chanBias_dB) 
  statistics_Gymp[i,2:14] <- c(zcr_l_mean, zcr_r_mean, 
                               zcr_l_sd, zcr_r_sd,
                               chanSim_mean, chanSim_sd, chanSim_range,
                               chanDiff_mean, chanDiff_sd, chanDiff_range,
                               chanBias_dB, chanBias_dB_sd, chanBias_dB_range) 
}
#colnames(statistics_Woon) <- c("Filename", "mean_ZeroCrossingFractionLeft",	
#                               "meanZeroCrossingFractionRight", "standardDevZCR_left",
#                               "standardDevZCR_right","meanChannelSimilarity",
#                               "meanChannelDiffDecibels","meanChannel_Bias")

colnames(statistics_Woon) <- c("Filename", "mean_ZeroCrossingFractionLeft",	
                               "meanZeroCrossingFractionRight", "standardDevZCR_left",
                               "standardDevZCR_right","meanChannelSimilarity",
                               "standardDevChanSimilarity", "rangeDevChanSimilarity", 
                                "meanChannelDiffDecibels",
                               "standardDevChannelDiffDecibels", "rangeChannelDiffDecibels",
                                "meanChannel_Bias", "standardDevChannel_Bias", 
                               "rangeChannel_Bias")

for(i in 1:length(shortNames_Woondum)) {
  nam <- paste("Woon_", substr(shortNames_Woondum[i], 1, 15), 
               sep = "")
  shortNames_Woondum[i] <- nam
}
statistics_Woon[,1] <- shortNames_Woondum

write.csv(statistics_Woon, "Channel integrity Woondum with Chan_Bias_sd.csv", row.names = FALSE)

gympie <- read.csv("Channel integrity Gympie with Chan_Bias_sd.csv")
woondum <- read.csv("Channel integrity Woondum with Chan_Bias_sd.csv")

####################################################
# collating the powspec information
####################################################
sourceDir <- "D:\\Cooloola\\"
setwd("D:\\Statistics\\")

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


gympie <- read.csv("Channel integrity Gympie_powspec.csv")
woondum <- read.csv("Channel integrity Woondum_powspec.csv")

