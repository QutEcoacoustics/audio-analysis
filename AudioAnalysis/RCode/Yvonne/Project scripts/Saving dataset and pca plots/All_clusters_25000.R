# remove all objects in the global environment
rm(list = ls())

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 50
column <- k2_value/5

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_", k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list 
cluster_list <- get(file_name_short, envir=globalenv())[,column]
load(file="data/datasets/normalised_summary_indices.RData")

# remove unneeded objects from global environment
rm(hclust_clusters_25000, file_name, file_name_short, column)

# load missing minute reference list 
load(file="data/datasets/missing_minutes_summary_indices.RData")
# load minutes where there was problems with both microphones
microphone_minutes <- c(184321:188640)
# list of all minutes that have been removed previously
removed_minutes <- c(missing_minutes_summary, microphone_minutes)
rm(microphone_minutes, missing_minutes_summary) 

full_length <- length(cluster_list) + length(removed_minutes)
list <- 1:full_length
list1 <- list[-c(removed_minutes)]
reconstituted_cluster_list <- rep(0, full_length)

reconstituted_cluster_list[removed_minutes] <- NA
reconstituted_cluster_list[list1] <- cluster_list

reconstituted_BGN <- rep(0, full_length)
reconstituted_BGN[removed_minutes] <- NA
reconstituted_BGN[list1] <- indices_norm_summary$BackgroundNoise
BGN <- reconstituted_BGN

reconstituted_SNR <- rep(0, full_length)
reconstituted_SNR[removed_minutes] <- NA
reconstituted_SNR[list1] <- indices_norm_summary$Snr
SNR <- reconstituted_SNR

reconstituted_ACT <- rep(0, full_length)
reconstituted_ACT[removed_minutes] <- NA
reconstituted_ACT[list1] <- indices_norm_summary$Activity
ACT <- reconstituted_ACT

reconstituted_EVN <- rep(0, full_length)
reconstituted_EVN[removed_minutes] <- NA
reconstituted_EVN[list1] <- indices_norm_summary$EventsPerSecond
EVN <- reconstituted_EVN

reconstituted_HFC <- rep(0, full_length)
reconstituted_HFC[removed_minutes] <- NA
reconstituted_HFC[list1] <- indices_norm_summary$HighFreqCover
HFC <- reconstituted_HFC

reconstituted_MFC <- rep(0, full_length)
reconstituted_MFC[removed_minutes] <- NA
reconstituted_MFC[list1] <- indices_norm_summary$MidFreqCover
MFC <- reconstituted_MFC

reconstituted_LFC <- rep(0, full_length)
reconstituted_LFC[removed_minutes] <- NA
reconstituted_LFC[list1] <- indices_norm_summary$LowFreqCover
LFC <- reconstituted_LFC

reconstituted_ACI <- rep(0, full_length)
reconstituted_ACI[removed_minutes] <- NA
reconstituted_ACI[list1] <- indices_norm_summary$AcousticComplexity
ACI <- reconstituted_ACI

reconstituted_EAS <- rep(0, full_length)
reconstituted_EAS[removed_minutes] <- NA
reconstituted_EAS[list1] <- indices_norm_summary$EntropyOfAverageSpectrum
EAS <- reconstituted_EAS

reconstituted_EPS <- rep(0, full_length)
reconstituted_EPS[removed_minutes] <- NA
reconstituted_EPS[list1] <- indices_norm_summary$EntropyOfPeaksSpectrum
EPS <- reconstituted_EPS

reconstituted_ECV <- rep(0, full_length)
reconstituted_ECV[removed_minutes] <- NA
reconstituted_ECV[list1] <- indices_norm_summary$EntropyOfCoVSpectrum
ECV <- reconstituted_ECV

reconstituted_CCC <- rep(0, full_length)
reconstituted_CCC[removed_minutes] <- NA
reconstituted_CCC[list1] <- indices_norm_summary$ClusterCount
CCC <- reconstituted_CCC

cluster_list <- reconstituted_cluster_list
rm(reconstituted_cluster_list)

days <- length(cluster_list)/(1440)
minute_reference <- rep(0:1439, days)

cluster_list <- cbind(cluster_list, minute_reference,
                      BGN,
                      SNR,
                      ACT,
                      EVN,
                      HFC,
                      MFC,
                      LFC,
                      ACI,
                      EAS,
                      EPS,
                      ECV,
                      CCC)

rm(days, full_length, list, list1, minute_reference, removed_minutes)

write.csv(cluster_list, paste("data/datasets/cluster_list_",k1_value,
                              "_",k2_value, ".csv", sep = ""), row.names = F)

write.csv(cluster_list[1:(length(cluster_list[,1])/2),], paste("data/datasets/Gympie_cluster_list_",k1_value,
                              "_",k2_value, ".csv", sep = ""), row.names = F)

write.csv(cluster_list[((length(cluster_list[,1])/2)+1):length(cluster_list[,1]),], paste("data/datasets/Woondum_cluster_list_",k1_value,
                                                              "_",k2_value, ".csv", sep = ""), row.names = F)
