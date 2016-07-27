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

write.csv(myFiles, "FileList_ Channel_Integrity.csv")

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

statistics_Gymp <- matrix(data="NA", ncol = 5, nrow = length(myFiles_Gympie))

for (i in 1:length(myFiles_Gympie)) {
  data <- read.csv(myFiles_Gympie[i])  
  zcr_l_mean <- mean(data[,1])
  zcr_r_mean <- mean(data[,2])
  chanSim_mean <- mean(data[,3])
  chanDiff_mean <- mean(data[,4])
  statistics_Gymp[i,] <- c(paste("Gymp_", substr(shortNames_Gympie[i],1,15), 
                            sep = ""), zcr_l_mean, zcr_r_mean, 
                            chanSim_mean, chanDiff_mean) 
}

statistics_Woon <- matrix(data="NA", ncol = 5, nrow = length(myFiles_Woondum))

for (i in 1:length(myFiles_Woondum)) {
  data <- read.csv(myFiles_Woondum[i])  
  zcr_l_mean <- mean(data[,1])
  zcr_r_mean <- mean(data[,2])
  chanSim_mean <- mean(data[,3])
  chanDiff_mean <- mean(data[,4])
  statistics_Woon[i,] <- c(paste("Woon_", substr(shortNames_Woondum[i],1,15), 
                                 sep = ""), zcr_l_mean, zcr_r_mean, 
                           chanSim_mean, chanDiff_mean) 
}

write(statistics_Gymp, "Channel integrity Gympie.csv")
write(statistics_Woon, "Channel integrity Woondum.csv")
