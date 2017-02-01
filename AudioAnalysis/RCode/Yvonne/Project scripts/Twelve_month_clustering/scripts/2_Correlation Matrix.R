# 3 July 2015
# Calculates the correleation matrix on the normalised indices
# Modified 14 September 2016

# remove all objects in global environment
rm(list = ls())

##############################################
# Read Summary Indices
##############################################
gympie_file <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_Towsey_Indices.csv"
woondum_file <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_Towsey_Indices.csv"
gympie_indices <- read.csv(gympie_file, header = T)[,-20]
woondum_indices <- read.csv(woondum_file, header = T)[,-20]
indices_all <- rbind(gympie_indices, woondum_indices)

##############################################
# Correlation matrix (Summary Indices) of thirteen months (398.5 days) at two sites
##############################################
a <- abs(cor(indices_all[,1:19], use = "complete.obs"))
write.csv(a, file = "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\Correlation matrices\\Correlation_matrix.csv")
rm(gympie_indices, woondum_indices, indices_all, a)

##################################################
# Read and save Spectral Indices as a RData file
##################################################
gympie_ACI <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_ACI_spect_avgs.csv")
woondum_ACI <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_ACI_spect_avgs.csv")
ACI_spect <- rbind(gympie_ACI, woondum_ACI)
rm(gympie_ACI, woondum_ACI)

#gympie_ACI <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_ACI_spect_avgs_range3.csv")
#woondum_ACI <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_ACI_spect_avgs_range3.csv")
#ACI_spect <- rbind(gympie_ACI, woondum_ACI)
#rm(gympie_ACI, woondum_ACI)

gympie_BGN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_BGN_spect_avgs.csv")
woondum_BGN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_BGN_spect_avgs.csv")
BGN_spect <- rbind(gympie_BGN, woondum_BGN)
rm(gympie_BGN, woondum_BGN)

#gympie_BGN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_BGN_spect_avgs_range3.csv")
#woondum_BGN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_BGN_spect_avgs_range3.csv")
#BGN_spect <- rbind(gympie_BGN, woondum_BGN)
#rm(gympie_BGN, woondum_BGN)

gympie_EVN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_EVN_spect_avgs.csv")
woondum_EVN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_EVN_spect_avgs.csv")
EVN_spect <- rbind(gympie_EVN, woondum_EVN)
rm(gympie_EVN, woondum_EVN)

gympie_ENT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_ENT_spect_avgs.csv")
woondum_ENT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_ENT_spect_avgs.csv")
ENT_spect <- rbind(gympie_ENT, woondum_ENT)
rm(gympie_ENT, woondum_ENT)

gympie_POW <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_POW_spect_avgs.csv")
woondum_POW <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_POW_spect_avgs.csv")
POW_spect <- rbind(gympie_POW, woondum_POW)
rm(gympie_POW, woondum_POW)

gympie_SPT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_SPT_spect_avgs.csv")
woondum_SPT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_SPT_spect_avgs.csv")
SPT_spect <- rbind(gympie_SPT, woondum_SPT)
rm(gympie_SPT, woondum_SPT)

indices_all_spect <- cbind(ACI_spect, BGN_spect, ENT_spect, 
                           EVN_spect, POW_spect, SPT_spect)

save(indices_all_spect, file="data/datasets/spectral_indices.RData")

#rm(list = ls())
#load(file="data/datasets/spectral_indices.RData")

#View(indices_all_spect)

# range 2 ##################################
gympie_ACI <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_ACI_spect_avgs_range2.csv")
woondum_ACI <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_ACI_spect_avgs_range2.csv")
ACI_spect <- rbind(gympie_ACI, woondum_ACI)
rm(gympie_ACI, woondum_ACI)

gympie_BGN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_BGN_spect_avgs_range2.csv")
woondum_BGN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_BGN_spect_avgs_range2.csv")
BGN_spect <- rbind(gympie_BGN, woondum_BGN)
rm(gympie_BGN, woondum_BGN)

gympie_EVN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_EVN_spect_avgs_range2.csv")
woondum_EVN <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_EVN_spect_avgs_range2.csv")
EVN_spect <- rbind(gympie_EVN, woondum_EVN)
rm(gympie_EVN, woondum_EVN)

gympie_ENT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_ENT_spect_avgs_range2.csv")
woondum_ENT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_ENT_spect_avgs_range2.csv")
ENT_spect <- rbind(gympie_ENT, woondum_ENT)
rm(gympie_ENT, woondum_ENT)

gympie_POW <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_POW_spect_avgs_range2.csv")
woondum_POW <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_POW_spect_avgs_range2.csv")
POW_spect <- rbind(gympie_POW, woondum_POW)
rm(gympie_POW, woondum_POW)

gympie_SPT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_20150622_20160723_SPT_spect_avgs_range2.csv")
woondum_SPT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_20150622_20160723_SPT_spect_avgs_range2.csv")
SPT_spect <- rbind(gympie_SPT, woondum_SPT)
rm(gympie_SPT, woondum_SPT)

indices_all_spect_range2 <- cbind(ACI_spect, BGN_spect, ENT_spect, 
                           EVN_spect, POW_spect, SPT_spect)

save(indices_all_spect_range2, file="data/datasets/spect_data_range2.RData")

#############################################
# Load the spectal indices
#############################################
# load all of the spectral indices as "indices_all_spect" object
load(file="data/datasets/spect_data.RData")
##############################################
# Correlation matrix (Summary Indices) of thirteen months (398.5 days) at two sites
##############################################
a <- abs(cor(indices_all_spect[,-seq(1,42,7)], use = "complete.obs"))
write.csv(a, file = "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\Correlation matrices\\Correlation_matrix_spectral_indices.csv")
# rm(indices_all_spect, a)

#############################################
# Load the spectal indices - range2
#############################################
# load all of the spectral indices as "indices_all_spect" object
load(file="data/datasets/spect_data_range2.RData")
##############################################
# Correlation matrix (Summary Indices) of thirteen months (398.5 days) at two sites
##############################################
a <- abs(cor(indices_all_spect_range2[,-seq(1,36,6)], use = "complete.obs"))
write.csv(a, file = "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\Correlation matrices\\Correlation_matrix_spectral_indices_range2.csv")
# rm(indices_all_spect, a)

#############################################
# Load the spectal indices BGN4
#############################################
# load all of the spectral indices as "indices_all_spect" object
load(file="data/datasets/spect_data_BGN4.RData")
##############################################
# Correlation matrix (Summary Indices) of thirteen months (398.5 days) at two sites
##############################################
a <- abs(cor(indices_all_spect[,-seq(1,42,7)], use = "complete.obs"))
write.csv(a, file = "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\Correlation matrices\\Correlation_matrix_spectral_indices_BGN4.csv")
# rm(indices_all_spect, a)

#############################################
# Load the spectal indices BGN4_ACI5
#############################################
# load all of the spectral indices as "indices_all_spect" object
load(file="data/datasets/spect_data_BGN4_ACI5.RData")
##############################################
# Correlation matrix (Summary Indices) of thirteen months (398.5 days) at two sites
##############################################
a <- abs(cor(indices_all_spect[,-c(1,7,12,19,26,33)], use = "complete.obs"))

write.csv(a, file = "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\Correlation matrices\\Correlation_matrix_spectral_indices_BGN4_ACI5.csv")
# rm(indices_all_spect, a)

# ENTROPY 1000 Hz
gympie_ENT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Gympie_1000Hz_20150622_20160723_ENT_spect_avgs.csv")
woondum_ENT <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\Woondum_1000Hz_20150622_20160723_ENT_spect_avgs.csv")
ENT_spect <- rbind(gympie_ENT, woondum_ENT)
rm(gympie_ENT, woondum_ENT)

a <- abs(cor(ENT_spect[,-c(1,7,12,19,26,33)], use = "complete.obs"))

write.csv(a, file = "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\Correlation matrices\\Correlation_matrix_ENT_1000_2000_spectral_indices.csv")
# rm(indices_all_spect, a)
