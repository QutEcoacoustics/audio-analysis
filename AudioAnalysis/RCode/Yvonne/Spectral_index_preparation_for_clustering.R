# Prepare spectral indices into frequency bands
# The frequency bands are:
# 0-500 Hz To capture planes
# 500-1000 Hz 
# 1000-2000 Hz
# 2000-3000 Hz
# 3000-4000 Hz
# 4000-5000 Hz
# 5000-6000 Hz
# 6000-8000 Hz
# 8000-11000 Hz
# the last two columns #256 and # 257 were removed because there is 
# a problem with these columns in the ACI calculation 
setwd("C:\\Work\\Following_confirmation\\Dataset\\")

folder <- "C:\\Temp\\Yvonne3\\concatOutput\\GympieNP\\"
library(stringr)
site <- str_sub(folder,-9,-2)
folder <- "C:\\Temp\\Yvonne3\\concatOutput\\Woondum3\\"
site <- str_sub(folder,-9,-2)

myFiles_ACI <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*ACI.csv$")
myFiles_BGN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*BGN.csv$")
myFiles_ENT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*ENT.csv$")
myFiles_EVN <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*EVN.csv$")
myFiles_POW <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*POW.csv$")
myFiles_SPT <- list.files(path=folder, recursive=T, full.names=TRUE, 
                          pattern="*SPT.csv$")
bands <- c(13, 25, 48, 71, 94, 117, 140, 187, 255)

ACI_spect_data <- NULL

for(i in 1:(length(myFiles_ACI)-1)) {
  ACI_data <- read.csv(myFiles_ACI[i], header = T)
  spectral_averages <- NULL
  for(j in 1:1440) {
    spect_avg <- c(mean(as.numeric(ACI_data[j, 2:13])), 
                   mean(as.numeric(ACI_data[j, 14:25])),
                   mean(as.numeric(ACI_data[j, 26:48])), 
                   mean(as.numeric(ACI_data[j, 49:71])),
                   mean(as.numeric(ACI_data[j, 72:94])), 
                   mean(as.numeric(ACI_data[j, 95:117])),
                   mean(as.numeric(ACI_data[j, 118:140])), 
                   mean(as.numeric(ACI_data[j, 141:187])),
                   mean(as.numeric(ACI_data[j, 188:255])))
    spectral_averages <- rbind(spectral_averages, spect_avg)
  }
  ACI_spect_data <- rbind(ACI_spect_data, spectral_averages)
}
col_headings <- c("ACI0_500Hz", "ACI500-1000Hz", "ACI1000-2000Hz", 
                  "ACI2000-3000Hz", "ACI3000-4000Hz", "ACI4000-5000Hz", 
                  "ACI5000-6000Hz", "ACI6000-8000Hz", "ACI8000-11000Hz")
colnames(ACI_spect_data) <- col_headings
write.csv(ACI_spect_data, paste("ACI_spect_", site, "_data.csv", sep = ""),
          row.names = F)

BGN_spect_data <- NULL
for(i in 1:(length(myFiles_BGN)-1)) {
  BGN_data <- read.csv(myFiles_BGN[i], header = T)
  spectral_averages <- NULL
  for(j in 1:1440) {
    spect_avg <- c(mean(as.numeric(BGN_data[j, 2:13])), 
                   mean(as.numeric(BGN_data[j, 14:25])),
                   mean(as.numeric(BGN_data[j, 26:48])), 
                   mean(as.numeric(BGN_data[j, 49:71])),
                   mean(as.numeric(BGN_data[j, 72:94])), 
                   mean(as.numeric(BGN_data[j, 95:117])),
                   mean(as.numeric(BGN_data[j, 118:140])), 
                   mean(as.numeric(BGN_data[j, 141:187])),
                   mean(as.numeric(BGN_data[j, 188:255])))
    spectral_averages <- rbind(spectral_averages, spect_avg)
  }
  BGN_spect_data <- rbind(BGN_spect_data, spectral_averages)
}
col_headings <- c("BGN0_500Hz", "BGN500-1000Hz", "BGN1000-2000Hz", 
                  "BGN2000-3000Hz", "BGN3000-4000Hz", "BGN4000-5000Hz", 
                  "BGN5000-6000Hz", "BGN6000-8000Hz", "BGN8000-11000Hz")
colnames(BGN_spect_data) <- col_headings
write.csv(BGN_spect_data, paste("BGN_spect_", site, "_data.csv", sep = ""),
          row.names = F)

ENT_spect_data <- NULL
for(i in 1:(length(myFiles_ENT)-1)) {
  ENT_data <- read.csv(myFiles_ENT[i], header = T)
  spectral_averages <- NULL
  for(j in 1:1440) {
    spect_avg <- c(mean(as.numeric(ENT_data[j, 2:13])), 
                   mean(as.numeric(ENT_data[j, 14:25])),
                   mean(as.numeric(ENT_data[j, 26:48])), 
                   mean(as.numeric(ENT_data[j, 49:71])),
                   mean(as.numeric(ENT_data[j, 72:94])), 
                   mean(as.numeric(ENT_data[j, 95:117])),
                   mean(as.numeric(ENT_data[j, 118:140])), 
                   mean(as.numeric(ENT_data[j, 141:187])),
                   mean(as.numeric(ENT_data[j, 188:255])))
    spectral_averages <- rbind(spectral_averages, spect_avg)
  }
  ENT_spect_data <- rbind(ENT_spect_data, spectral_averages)
}
col_headings <- c("ENT0_500Hz", "ENT500-1000Hz", "ENT1000-2000Hz", 
                  "ENT2000-3000Hz", "ENT3000-4000Hz", "ENT4000-5000Hz", 
                  "ENT5000-6000Hz", "ENT6000-8000Hz", "ENT8000-11000Hz")
colnames(ENT_spect_data) <- col_headings
write.csv(ENT_spect_data, paste("ENT_spect_", site, "_data.csv", sep = ""),
          row.names = F)

EVN_spect_data <- NULL
for(i in 1:(length(myFiles_EVN)-1)) {
  EVN_data <- read.csv(myFiles_EVN[i], header = T)
  spectral_averages <- NULL
  for(j in 1:1440) {
    spect_avg <- c(mean(as.numeric(EVN_data[j, 2:13])), 
                   mean(as.numeric(EVN_data[j, 14:25])),
                   mean(as.numeric(EVN_data[j, 26:48])), 
                   mean(as.numeric(EVN_data[j, 49:71])),
                   mean(as.numeric(EVN_data[j, 72:94])), 
                   mean(as.numeric(EVN_data[j, 95:117])),
                   mean(as.numeric(EVN_data[j, 118:140])), 
                   mean(as.numeric(EVN_data[j, 141:187])),
                   mean(as.numeric(EVN_data[j, 188:255])))
    spectral_averages <- rbind(spectral_averages, spect_avg)
  }
  EVN_spect_data <- rbind(EVN_spect_data, spectral_averages)
}
col_headings <- c("EVN0_500Hz", "EVN500-1000Hz", "EVN1000-2000Hz", 
                  "EVN2000-3000Hz", "EVN3000-4000Hz", "EVN4000-5000Hz", 
                  "EVN5000-6000Hz", "EVN6000-8000Hz", "EVN8000-11000Hz")
colnames(EVN_spect_data) <- col_headings
write.csv(EVN_spect_data, paste("EVN_spect_", site, "_data.csv", sep = ""),
          row.names = F)


POW_spect_data <- NULL
for(i in 1:(length(myFiles_POW)-1)) {
  POW_data <- read.csv(myFiles_POW[i], header = T)
  spectral_averages <- NULL
  for(j in 1:1440) {
    spect_avg <- c(mean(as.numeric(POW_data[j, 2:13])), 
                   mean(as.numeric(POW_data[j, 14:25])),
                   mean(as.numeric(POW_data[j, 26:48])), 
                   mean(as.numeric(POW_data[j, 49:71])),
                   mean(as.numeric(POW_data[j, 72:94])), 
                   mean(as.numeric(POW_data[j, 95:117])),
                   mean(as.numeric(POW_data[j, 118:140])), 
                   mean(as.numeric(POW_data[j, 141:187])),
                   mean(as.numeric(POW_data[j, 188:255])))
    spectral_averages <- rbind(spectral_averages, spect_avg)
  }
  POW_spect_data <- rbind(POW_spect_data, spectral_averages)
}
col_headings <- c("POW0_500Hz", "POW500-1000Hz", "POW1000-2000Hz", 
                  "POW2000-3000Hz", "POW3000-4000Hz", "POW4000-5000Hz", 
                  "POW5000-6000Hz", "POW6000-8000Hz", "POW8000-11000Hz")
colnames(POW_spect_data) <- col_headings
write.csv(POW_spect_data, paste("POW_spect_", site, "_data.csv", sep = ""),
          row.names = F)

SPT_spect_data <- NULL
for(i in 1:(length(myFiles_SPT)-1)) {
  SPT_data <- read.csv(myFiles_SPT[i], header = T)
  spectral_averages <- NULL
  for(j in 1:1440) {
    spect_avg <- c(mean(as.numeric(SPT_data[j, 2:13])), 
                   mean(as.numeric(SPT_data[j, 14:25])),
                   mean(as.numeric(SPT_data[j, 26:48])), 
                   mean(as.numeric(SPT_data[j, 49:71])),
                   mean(as.numeric(SPT_data[j, 72:94])), 
                   mean(as.numeric(SPT_data[j, 95:117])),
                   mean(as.numeric(SPT_data[j, 118:140])), 
                   mean(as.numeric(SPT_data[j, 141:187])),
                   mean(as.numeric(SPT_data[j, 188:255])))
    spectral_averages <- rbind(spectral_averages, spect_avg)
  }
  SPT_spect_data <- rbind(SPT_spect_data, spectral_averages)
}
index <- "SPT"
col_headings <- c(paste(index,"0_500Hz",sep = ""),
                  paste(index,"500-1000Hz",sep = ""),
                  paste(index,"1000-2000Hz", sep = ""),
                  paste(index, "2000-3000Hz", sep = ""),
                  paste(index,"3000-4000Hz", sep = ""),
                  paste(index, "4000-5000Hz", sep = ""),
                  paste(index, "5000-6000Hz", sep = ""),
                  paste(index, "6000-8000Hz", sep = ""),
                  paste(index, "8000-11000Hz", sep = ""))
colnames(SPT_spect_data) <- col_headings
write.csv(SPT_spect_data, paste("SPT_spect_", site, "_data.csv", sep = ""),
          row.names = F)

#########
# concatenate gympie and woondum files
ACI_gympie <- read.csv("ACI_spect_GympieNP_data.csv", header = T)
ACI_woondum <- read.csv("ACI_spect_Woondum3_data.csv", header = T)
ACI_all <- rbind(ACI_gympie, ACI_woondum)
# replace NAs
for(i in 1:ncol(ACI_all)) {  # columns
  for(j in 1:nrow(ACI_all)) {  # rows
    if (is.na(ACI_all[j,i])) {
      average <- mean(c(ACI_all[(j-15),i], ACI_all[(j-12),i], 
                        ACI_all[(j-10),i], ACI_all[(j+15),i], 
                        ACI_all[(j+12),i], ACI_all[(j+10),i]),
                      na.rm=TRUE)
      ACI_all[j,i] <- average
    }
  }
}
index <- "ACI"
col_headings <- c(paste(index,"0Hz",sep = ""),
                  paste(index,"500Hz",sep = ""),
                  paste(index,"1000Hz", sep = ""),
                  paste(index, "2000Hz", sep = ""),
                  paste(index,"3000Hz", sep = ""),
                  paste(index, "4000Hz", sep = ""),
                  paste(index, "5000Hz", sep = ""),
                  paste(index, "6000Hz", sep = ""),
                  paste(index, "8000Hz", sep = ""))
colnames(ACI_all) <- col_headings
write.csv(ACI_all, "ACI_all.csv", row.names = F)

BGN_gympie <- read.csv("BGN_spect_GympieNP_data.csv", header = T)
BGN_woondum <- read.csv("BGN_spect_Woondum3_data.csv", header = T)
BGN_all <- rbind(BGN_gympie, BGN_woondum)
# replace NAs
for(i in 1:ncol(BGN_all)) {  # columns
  for(j in 1:nrow(BGN_all)) {  # rows
    if (is.na(BGN_all[j,i])) {
      average <- mean(c(BGN_all[(j-15),i], BGN_all[(j-12),i], 
                        BGN_all[(j-10),i], BGN_all[(j+15),i], 
                        BGN_all[(j+12),i], BGN_all[(j+10),i]),
                      na.rm=TRUE)
      BGN_all[j,i] <- average
    }
  }
}
index <- "BGN"
col_headings <- c(paste(index,"0Hz",sep = ""),
                  paste(index,"500Hz",sep = ""),
                  paste(index,"1000Hz", sep = ""),
                  paste(index, "2000Hz", sep = ""),
                  paste(index,"3000Hz", sep = ""),
                  paste(index, "4000Hz", sep = ""),
                  paste(index, "5000Hz", sep = ""),
                  paste(index, "6000Hz", sep = ""),
                  paste(index, "8000Hz", sep = ""))
colnames(BGN_all) <- col_headings
write.csv(BGN_all, "BGN_all.csv", row.names = F)

ENT_gympie <- read.csv("ENT_spect_GympieNP_data.csv", header = T)
ENT_woondum <- read.csv("ENT_spect_Woondum3_data.csv", header = T)
ENT_all <- rbind(ENT_gympie, ENT_woondum)
# replace NAs
for(i in 1:ncol(ENT_all)) {  # columns
  for(j in 1:nrow(ENT_all)) {  # rows
    if (is.na(ENT_all[j,i])) {
      average <- mean(c(ENT_all[(j-15),i], ENT_all[(j-12),i], 
                        ENT_all[(j-10),i], ENT_all[(j+15),i], 
                        ENT_all[(j+12),i], ENT_all[(j+10),i]),
                      na.rm=TRUE)
      ENT_all[j,i] <- average
    }
  }
}
index <- "ENT"
col_headings <- c(paste(index,"0Hz",sep = ""),
                  paste(index,"500Hz",sep = ""),
                  paste(index,"1000Hz", sep = ""),
                  paste(index, "2000Hz", sep = ""),
                  paste(index,"3000Hz", sep = ""),
                  paste(index, "4000Hz", sep = ""),
                  paste(index, "5000Hz", sep = ""),
                  paste(index, "6000Hz", sep = ""),
                  paste(index, "8000Hz", sep = ""))
colnames(ENT_all) <- col_headings
write.csv(ENT_all, "ENT_all.csv", row.names = F)

EVN_gympie <- read.csv("EVN_spect_GympieNP_data.csv", header = T)
EVN_woondum <- read.csv("EVN_spect_Woondum3_data.csv", header = T)
EVN_all <- rbind(EVN_gympie, EVN_woondum)
# replace NAs
for(i in 1:ncol(EVN_all)) {  # columns
  for(j in 1:nrow(EVN_all)) {  # rows
    if (is.na(EVN_all[j,i])) {
      average <- mean(c(EVN_all[(j-15),i], EVN_all[(j-12),i], 
                        EVN_all[(j-10),i], EVN_all[(j+15),i], 
                        EVN_all[(j+12),i], EVN_all[(j+10),i]),
                      na.rm=TRUE)
      EVN_all[j,i] <- average
    }
  }
}
index <- "EVN"
col_headings <- c(paste(index,"0Hz",sep = ""),
                  paste(index,"500Hz",sep = ""),
                  paste(index,"1000Hz", sep = ""),
                  paste(index, "2000Hz", sep = ""),
                  paste(index,"3000Hz", sep = ""),
                  paste(index, "4000Hz", sep = ""),
                  paste(index, "5000Hz", sep = ""),
                  paste(index, "6000Hz", sep = ""),
                  paste(index, "8000Hz", sep = ""))
colnames(EVN_all) <- col_headings
write.csv(EVN_all, "EVN_all.csv", row.names = F)

POW_gympie <- read.csv("POW_spect_GympieNP_data.csv", header = T)
POW_woondum <- read.csv("POW_spect_Woondum3_data.csv", header = T)
POW_all <- rbind(POW_gympie, POW_woondum)
# replace NAs
for(i in 1:ncol(POW_all)) {  # columns
  for(j in 1:nrow(POW_all)) {  # rows
    if (is.na(POW_all[j,i])) {
      average <- mean(c(POW_all[(j-15),i], POW_all[(j-12),i], 
                        POW_all[(j-10),i], POW_all[(j+15),i], 
                        POW_all[(j+12),i], POW_all[(j+10),i]),
                      na.rm=TRUE)
      POW_all[j,i] <- average
    }
  }
}
index <- "POW"
col_headings <- c(paste(index,"0Hz",sep = ""),
                  paste(index,"500Hz",sep = ""),
                  paste(index,"1000Hz", sep = ""),
                  paste(index, "2000Hz", sep = ""),
                  paste(index,"3000Hz", sep = ""),
                  paste(index, "4000Hz", sep = ""),
                  paste(index, "5000Hz", sep = ""),
                  paste(index, "6000Hz", sep = ""),
                  paste(index, "8000Hz", sep = ""))
colnames(POW_all) <- col_headings
write.csv(POW_all, "POW_all.csv", row.names = F)

SPT_gympie <- read.csv("SPT_spect_GympieNP_data.csv", header = T)
SPT_woondum <- read.csv("SPT_spect_Woondum3_data.csv", header = T)
SPT_all <- rbind(SPT_gympie, SPT_woondum)
# replace NAs
for(i in 1:ncol(SPT_all)) {  # columns
  for(j in 1:nrow(SPT_all)) {  # rows
    if (is.na(SPT_all[j,i])) {
      average <- mean(c(SPT_all[(j-15),i], SPT_all[(j-12),i], 
                        SPT_all[(j-10),i], SPT_all[(j+15),i], 
                        SPT_all[(j+12),i], SPT_all[(j+10),i]),
                      na.rm=TRUE)
      SPT_all[j,i] <- average
    }
  }
}
index <- "SPT"
col_headings <- c(paste(index,"0Hz",sep = ""),
                  paste(index,"500Hz",sep = ""),
                  paste(index,"1000Hz", sep = ""),
                  paste(index, "2000Hz", sep = ""),
                  paste(index,"3000Hz", sep = ""),
                  paste(index, "4000Hz", sep = ""),
                  paste(index, "5000Hz", sep = ""),
                  paste(index, "6000Hz", sep = ""),
                  paste(index, "8000Hz", sep = ""))
colnames(SPT_all) <- col_headings
write.csv(SPT_all, "SPT_all.csv", row.names = F)

Spect_ACI <- read.csv("ACI_all.csv", header = T)
Spect_BGN <- read.csv("BGN_all.csv", header = T)
Spect_ENT <- read.csv("ENT_all.csv", header = T)
Spect_EVN <- read.csv("EVN_all.csv", header = T)
Spect_POW <- read.csv("POW_all.csv", header = T)
Spect_SPT <- read.csv("SPT_all.csv", header = T)

Spect_Indices <- cbind(Spect_ACI, Spect_BGN,
                       Spect_ENT, Spect_EVN,
                       Spect_POW, Spect_SPT)
a <- cor(Spect_Indices, use = "complete.obs")
write.table(a, file = paste("Correlation_matrix_completeobs_spectral_indices.csv"),
                            sep = ",", col.names = NA, 
            qmethod = "double")