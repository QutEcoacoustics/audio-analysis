# Microphone test

# remove all objects in the global environment
rm(list = ls())

indices <- c("ACI", "BGN", "ENT", "EVN")

for(j in 1:length(indices)) {
  # New microphone dry test - one file missing
  folder <- "E:\\Microphone test\\New_microphone_test"
  Files <- list.files(path=folder,recursive = TRUE, full.names = TRUE,
                      pattern = paste("*", indices[j], ".csv", sep = ""))
  short_name <- list.files(path=folder,recursive = TRUE, full.names = FALSE,
                           pattern = paste("*", indices[j], ".csv", sep = ""))
  
  ACI_new_dry <- NULL
  for(i in 1:length(Files)) {
    datetime <- substr(short_name[i],5,19)
    datetime <- paste(substr(datetime,1,4),"-",
                      substr(datetime,5,6),"-",
                      substr(datetime,7,8)," ",
                      substr(datetime,10,11), ":",
                      substr(datetime,12,13), ":",
                      substr(datetime,14,15), " AEST", sep = "")
    #time <- as.numeric(time)
    a <- read.csv(Files[i])
    length <- nrow(a)
    startDate = as.POSIXct(datetime)
    a$time <- startDate + 60 * 0:(length-1) # makes a sequence in minutes
    ACI_new_dry <- rbind(ACI_new_dry, a)
  }
  write.csv(ACI_new_dry, paste("microphone test/", indices[j], "_new_dry.csv", sep = ""),
            row.names = FALSE)
  
  # Old microphone dry test 
  folder <- "E:\\Microphone test\\Old_microphone_test"
  Files <- list.files(path=folder,recursive = TRUE, full.names = TRUE,
                      pattern = paste("*", indices[j], ".csv", sep = ""))
  short_name <- list.files(path=folder,recursive = TRUE, full.names = FALSE,
                           pattern = paste("*", indices[j], ".csv", sep = ""))
  ACI_old_dry <- NULL
  for(i in 1:length(Files)) {
    datetime <- substr(short_name[i],5,19)
    datetime <- paste(substr(datetime,1,4),"-",
                      substr(datetime,5,6),"-",
                      substr(datetime,7,8)," ",
                      substr(datetime,10,11), ":",
                      substr(datetime,12,13), ":",
                      substr(datetime,14,15), " AEST", sep = "")
    #time <- as.numeric(time)
    a <- read.csv(Files[i])
    length <- nrow(a)
    startDate = as.POSIXct(datetime)
    a$time <- startDate + 60 * 0:(length-1) # makes a sequence in minutes
    ACI_old_dry <- rbind(ACI_old_dry, a)
  }
  
  write.csv(ACI_old_dry, paste("microphone test/", indices[j], 
                               "_old_dry.csv", sep = ""), row.names = FALSE)
  
  # SM4 microphone dry test 
  folder <- "E:\\Microphone test\\SM4_microphone_test"
  Files <- list.files(path=folder,recursive = TRUE, full.names = TRUE,
                      pattern = paste("*", indices[j], ".csv", sep = ""))
  short_name <- list.files(path=folder,recursive = TRUE, full.names = FALSE,
                           pattern = paste("*", indices[j], ".csv", sep = ""))
  ACI_SM4_dry <- NULL
  for(i in 1:length(Files)) {
    datetime <- substr(short_name[i],5,19)
    datetime <- paste(substr(datetime,1,4),"-",
                      substr(datetime,5,6),"-",
                      substr(datetime,7,8)," ",
                      substr(datetime,10,11), ":",
                      substr(datetime,12,13), ":",
                      substr(datetime,14,15), " AEST", sep = "")
    #time <- as.numeric(time)
    a <- read.csv(Files[i])
    length <- nrow(a)
    startDate = as.POSIXct(datetime)
    a$time <- startDate + 60 * 0:(length-1) # makes a sequence in minutes
    ACI_SM4_dry <- rbind(ACI_SM4_dry, a)
  }
  write.csv(ACI_SM4_dry, paste("microphone test/", indices[j], 
                               "_SM4_dry.csv", sep = ""), row.names = FALSE)
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# choosing periods within the data
# Four periods are set up to analyse
# P1. 4 pm to 6 pm on 7 August 2016
# P2. 10 pm to 12 am on 7 August 2016
# P3. 4 am to 6 am on 8th August 2016
# P4. 10 am to 12 pm on 9th August 2016
# The gap of one day exists because one file failed the analysis

# remove all objects in the global environment
rm(list = ls())

indices <- c("ACI", "BGN", "ENT", "EVN")

for(i in 1:length(indices)) {
  # Period 1 on 7 August 2016 new
  ACI_new_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_new_dry.csv",sep = ""), header = T)
  a <- which(substr(ACI_new_dry$time,1,10)=="2016-08-07" 
             & (substr(ACI_new_dry$time, 12,13)=="16"
                | substr(ACI_new_dry$time, 12,13)=="17"))
  
  per1_ACI_new_dry <- ACI_new_dry[a,]
  
  ACI_old_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_old_dry.csv",sep = ""), header = T)
  # period 1 on 7 August 2016 old
  a <- which(substr(ACI_old_dry$time,1,10)=="2016-08-07" &
               (substr(ACI_old_dry$time, 12,13)=="16"
                | substr(ACI_old_dry$time, 12,13)=="17"))
  
  per1_ACI_old_dry <- ACI_old_dry[a,]
  
  ACI_sm4_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_SM4_dry.csv",sep = ""), header = T)
  # period 1 on 7 August 2016 sm4
  a <- which(substr(ACI_sm4_dry$time,1,10)=="2016-08-07" &
               (substr(ACI_sm4_dry$time, 12,13)=="16"
                | substr(ACI_sm4_dry$time, 12,13)=="17"))
  
  per1_ACI_sm4_dry <- ACI_sm4_dry[a,]
  
  write.csv(per1_ACI_old_dry, paste("microphone test\\per1_", indices[i],
                                    "_old_dry.csv", sep = ""), 
            row.names = F)
  write.csv(per1_ACI_new_dry, paste("microphone test\\per1_", indices[i],
                                    "_new_dry.csv", sep = ""), 
            row.names = F)
  write.csv(per1_ACI_sm4_dry, paste("microphone test\\per1_", indices[i],
                                    "_sm4_dry.csv", sep = ""), 
            row.names = F)
  
  # Period 2 on 7 August 2016 new
  ACI_new_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_new_dry.csv",sep = ""), header = T)
  
  a <- which(substr(ACI_new_dry$time,1,10)=="2016-08-07" 
             & (substr(ACI_new_dry$time, 12,13)=="22"
                | substr(ACI_new_dry$time, 12,13)=="23"))
  per2_ACI_new_dry <- ACI_new_dry[a,]
  
  # Period 2 on 7 August 2016 old
  ACI_old_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_old_dry.csv",sep = ""), header = T)
  a <- which(substr(ACI_old_dry$time,1,10)=="2016-08-07" 
             & (substr(ACI_old_dry$time, 12,13)=="22"
                | substr(ACI_old_dry$time, 12,13)=="23"))
  a <- a - 1
  a <- c(a, (max(a)+1))
  per2_ACI_old_dry <- ACI_old_dry[a,]
  
  ACI_sm4_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_SM4_dry.csv",sep = ""), header = T)
  
  # period 2 on 7 August 2016 sm4
  a <- which(substr(ACI_sm4_dry$time,1,10)=="2016-08-07" 
             & (substr(ACI_sm4_dry$time, 12,13)=="22"
                | substr(ACI_sm4_dry$time, 12,13)=="23"))
  a <- c(a, (max(a)+1))
  per2_ACI_sm4_dry <- ACI_sm4_dry[a,]
  
  write.csv(per2_ACI_old_dry, paste("microphone test\\per2_", indices[i],
                                    "_old_dry.csv", sep = ""), 
            row.names = F)
  write.csv(per2_ACI_new_dry, paste("microphone test\\per2_", indices[i],
                                    "_new_dry.csv", sep = ""), 
            row.names = F)
  write.csv(per2_ACI_sm4_dry, paste("microphone test\\per2_", indices[i],
                                    "_sm4_dry.csv", sep = ""), 
            row.names = F)
  
  # Period 3 on 8 August 2016 new
  ACI_new_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_new_dry.csv",sep = ""), header = T)
  
  a <- which(substr(ACI_new_dry$time,1,10)=="2016-08-08" 
             & (substr(ACI_new_dry$time, 12,13)=="04"
                | substr(ACI_new_dry$time, 12,13)=="05"))
  per3_ACI_new_dry <- ACI_new_dry[a,]
  
  # Period 3 on 8 August 2016 old
  ACI_old_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_old_dry.csv",sep = ""), header = T)
  
  a <- which(substr(ACI_old_dry$time,1,10)=="2016-08-08" 
             & (substr(ACI_old_dry$time, 12,13)=="04"
                | substr(ACI_old_dry$time, 12,13)=="05"))
  per3_ACI_old_dry <- ACI_old_dry[a,]
  
  # Period 3 on 8 August 2016 sm4
  ACI_sm4_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_SM4_dry.csv",sep = ""), header = T)
  
  a <- which(substr(ACI_sm4_dry$time,1,10)=="2016-08-08" 
             & (substr(ACI_sm4_dry$time, 12,13)=="04"
                | substr(ACI_sm4_dry$time, 12,13)=="05"))
  per3_ACI_sm4_dry <- ACI_sm4_dry[a,]
  
  write.csv(per3_ACI_old_dry, paste("microphone test\\per3_", indices[i],
                                    "_old_dry.csv", sep = ""), 
            row.names = F)
  write.csv(per3_ACI_new_dry, paste("microphone test\\per3_", indices[i],
                                    "_new_dry.csv", sep = ""), 
            row.names = F)
  write.csv(per3_ACI_sm4_dry, paste("microphone test\\per3_", indices[i],
                                    "_sm4_dry.csv", sep = ""), 
            row.names = F)
  
  # Period 4 on 9 August 2016 new
  ACI_new_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_new_dry.csv",sep = ""), header = T)
  
  a <- which(substr(ACI_new_dry$time,1,10)=="2016-08-09" 
             & (substr(ACI_new_dry$time, 12,13)=="10"
                | substr(ACI_new_dry$time, 12,13)=="11"))
  per4_ACI_new_dry <- ACI_new_dry[a,]
  
  # Period 4 on 9 August 2016 old
  ACI_old_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_old_dry.csv",sep = ""), header = T)
  
  a <- which(substr(ACI_old_dry$time,1,10)=="2016-08-09" 
             & (substr(ACI_old_dry$time, 12,13)=="10"
                | substr(ACI_old_dry$time, 12,13)=="11"))
  per4_ACI_old_dry <- ACI_old_dry[a,]
  
  # Period 4 on 9 August 2016 sm4
  ACI_sm4_dry <- read.csv(paste("microphone test\\", indices[i], 
                                "_SM4_dry.csv",sep = ""), header = T)
  
  a <- which(substr(ACI_sm4_dry$time,1,10)=="2016-08-09" 
             & (substr(ACI_sm4_dry$time, 12,13)=="10"
                | substr(ACI_sm4_dry$time, 12,13)=="11"))
  per4_ACI_sm4_dry <- ACI_sm4_dry[a,]
  
  write.csv(per4_ACI_old_dry, paste("microphone test\\per4_", indices[i],
                                    "_old_dry.csv", sep = ""), 
            row.names = F)
  write.csv(per4_ACI_new_dry, paste("microphone test\\per4_", indices[i],
                                    "_new_dry.csv", sep = ""), 
            row.names = F)
  write.csv(per4_ACI_sm4_dry, paste("microphone test\\per4_", indices[i],
                                    "_sm4_dry.csv", sep = ""), 
            row.names = F)
  
}

#folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP"
#library(stringr)
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
             pattern = "per[1-4]{1}.*\\.csv")
my_files_BGN <- list.files(path=folder,recursive = TRUE, full.names = T,
                           pattern = "per[1-4]{1}_BGN.*\\.csv")
my_files <- setdiff(my_files, my_files_BGN)
for(i in 1:length(my_files)) {
  data <- read.csv(my_files[i], header = TRUE) 
  spect_data <- NULL
  spect_avg <- data.frame(ID = data[,1], 
                          A_0Hz = rowMeans(data[,c(2:47)]),
                          A_2000Hz = rowMeans(data[,c(48:93)]),
                          A_4000Hz = rowMeans(data[,c(94:140)]),
                          A_6000Hz = rowMeans(data[,c(141:186)]),
                          A_8000Hz = rowMeans(data[,c(187:233)]))
  spect_data <- rbind(spect_data, spect_avg)
  data <- cbind(data, spect_data[,2:6])
  write.csv(data, file=paste(my_files[i], sep = ""), row.names = F)
}

# remove all objects in the global environment
rm(list = ls())

# Background Noise is treaded differently because it is logarithmic
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files_BGN <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[1-4]{1}_BGN.*\\.csv")

for(i in 1:length(my_files_BGN)) {
  BGN_data <- read.csv(my_files_BGN[i], header = T)
  spectral_averages <- NULL
  BGN_data2 <- BGN_data[,1:(length(BGN_data)-1)]
  BGN_data2[,1:(length(BGN_data)-1)] <- 10^(BGN_data[,1:(length(BGN_data)-1)]/10)
  BGN_data2$Index <- BGN_data[,1]
  spect_avg <- data.frame(A_0Hz = rowMeans(BGN_data2[,c(2:47)]),
                          A_2000Hz = rowMeans(BGN_data2[,c(48:93)]),
                          A_4000Hz = rowMeans(BGN_data2[,c(94:140)]),
                          A_6000Hz = rowMeans(BGN_data2[,c(141:186)]),
                          A_8000Hz = rowMeans(BGN_data2[,c(187:233)]))
  spect_data <- 10*log10(abs(spect_avg[,1:length(spect_avg)]))
  print(i)
  BGN_data <- cbind(BGN_data, spect_data)
  write.csv(BGN_data, file=paste(my_files_BGN[i], sep = ""), row.names = F)
  rm(BGN_data2)
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Spectral Indices Boxplot graphs -----------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# period 1 EVN
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[1]{1}_EVN.*\\.csv")
i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period1_EVN <- rbind(old, new, sm4)

png("microphone test\\period1_EVN.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.13, 3)
boxplot(period1_EVN$A_0Hz~period1_EVN$mic,ylim=ylim, 
        main="P1. EVN 0-2 kHz")
boxplot(period1_EVN$A_2000Hz~period1_EVN$mic,ylim=ylim, 
        main="P1. EVN 2-4 kHz", cex.axis=1.8, yaxt = "n")
boxplot(period1_EVN$A_4000Hz~period1_EVN$mic, ylim=ylim, 
        main="P1. EVN 4-6 kHz", cex.axis=1.8, yaxt = "n")
mtext(side = 3, "4pm to 6pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-08", line = -2.2, cex = 1.4)
boxplot(period1_EVN$A_6000Hz~period1_EVN$mic, ylim=ylim, 
        main="P1. EVN 6-8 kHz", cex.axis=1.8, yaxt = "n")
boxplot(period1_EVN$A_8000Hz~period1_EVN$mic, ylim=ylim, 
        main="P1. EVN 8-10 kHz", yaxt = "n")
dev.off()

# period 1 ACI
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[1]{1}_ACI.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period1_ACI <- rbind(old, new, sm4)

png("microphone test\\period1_ACI.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.393, 0.622)
boxplot(period1_ACI$A_0Hz~period1_ACI$mic,ylim=ylim,
        main="P1. ACI 0-2 kHz")
boxplot(period1_ACI$A_2000Hz~period1_ACI$mic,ylim=ylim,
        main="P1. ACI 2-4 kHz", yaxt = "n")
boxplot(period1_ACI$A_4000Hz~period1_ACI$mic, ylim=ylim,
        main="P1. ACI 4-6 kHz", yaxt = "n")
mtext(side = 3, "4pm to 6pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-08", line = -2.2, cex = 1.4)
boxplot(period1_ACI$A_6000Hz~period1_ACI$mic, ylim=ylim, 
        main="P1. ACI 6-8 kHz", yaxt = "n")
boxplot(period1_ACI$A_8000Hz~period1_ACI$mic, ylim=ylim, 
        main="P1. ACI 8-10 kHz", yaxt = "n")
dev.off()

# period 1 ENT
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[1]{1}_ENT.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period1_ENT <- rbind(old, new, sm4)

png("microphone test\\period1_ENT.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.0315, 0.655)
boxplot(period1_ENT$A_0Hz~period1_ENT$mic, ylim=ylim,
        main="P1. ENT 0-2 kHz")
boxplot(period1_ENT$A_2000Hz~period1_ENT$mic, ylim=ylim, 
        main="P1. ENT 2-4 kHz", yaxt = "n")
boxplot(period1_ENT$A_4000Hz~period1_ENT$mic, ylim=ylim, 
        main="P1. ENT 4-6 kHz", yaxt = "n")
mtext(side = 3, "4pm to 6pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-08", line = -2.2, cex = 1.4)
boxplot(period1_ENT$A_6000Hz~period1_ENT$mic, ylim=ylim, 
        main="P1. ENT 6-8 kHz", yaxt = "n")
boxplot(period1_ENT$A_8000Hz~period1_ENT$mic, ylim=ylim, 
        main="P1. ENT 8-10 kHz", yaxt = "n")
dev.off()

# period 1 BGN
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[1]{1}_BGN.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period1_BGN <- rbind(old, new, sm4)

png("microphone test\\period1_BGN.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(-112,-70)
boxplot(period1_BGN$A_0Hz~period1_BGN$mic, ylim=ylim, 
        main="P1. BGN 0-2 kHz")
boxplot(period1_BGN$A_2000Hz~period1_BGN$mic, ylim=ylim, 
        main="P1. BGN 2-4 kHz", yaxt = "n")
boxplot(period1_BGN$A_4000Hz~period1_BGN$mic, ylim=ylim,
        main="P1. BGN 4-6 kHz", yaxt = "n")
mtext(side = 3, "4pm to 6pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-08", line = -2.2, cex = 1.4)
boxplot(period1_BGN$A_6000Hz~period1_BGN$mic, ylim=ylim,
        main="P1. BGN 6-8 kHz", yaxt = "n")
boxplot(period1_BGN$A_8000Hz~period1_BGN$mic, ylim=ylim, 
        main="P1. BGN 8-10 kHz", yaxt = "n")
dev.off()
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# period 2 EVN
#rm(period1_BGN, period1_ENT, period1_ACI, period1_EVN)
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[2]{1}_EVN.*\\.csv")
i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period2_EVN <- rbind(old, new, sm4)

png("microphone test\\period2_EVN.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.13, 3)
boxplot(period2_EVN$A_0Hz~period2_EVN$mic, 
        main="P2. EVN 0-2 kHz", ylim=ylim)
boxplot(period2_EVN$A_2000Hz~period2_EVN$mic, yaxt = "n",
        main="P2. EVN 2-4 kHz", ylim=ylim)
boxplot(period2_EVN$A_4000Hz~period2_EVN$mic, yaxt = "n", 
        main="P2. EVN 4-6 kHz", ylim=ylim)
mtext(side = 3, "10pm to 12pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-07", line = -2.2, cex = 1.4)
boxplot(period2_EVN$A_6000Hz~period2_EVN$mic, yaxt = "n",
        main="P2. EVN 6-8 kHz", ylim=ylim)
boxplot(period2_EVN$A_8000Hz~period2_EVN$mic, yaxt = "n",
        main="P2. EVN 8-10 kHz", ylim=ylim)
dev.off()

# period 2 ACI
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[2]{1}_ACI.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period2_ACI <- rbind(old, new, sm4)

png("microphone test\\period2_ACI.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.393, 0.622)
boxplot(period2_ACI$A_0Hz~period2_ACI$mic, ylim=ylim, 
        main="P2. ACI 0-2 kHz")
boxplot(period2_ACI$A_2000Hz~period2_ACI$mic, ylim=ylim, 
        main="P2. ACI 2-4 kHz", yaxt = "n")
boxplot(period2_ACI$A_4000Hz~period2_ACI$mic, ylim=ylim, 
        main="P2. ACI 4-6 kHz", yaxt = "n")
mtext(side = 3, "10pm to 12pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-07", line = -2.2, cex = 1.4)
boxplot(period2_ACI$A_6000Hz~period2_ACI$mic, ylim=ylim, 
        main="P2. ACI 6-8 kHz", yaxt = "n")
boxplot(period2_ACI$A_8000Hz~period2_ACI$mic, ylim=ylim, 
        main="P2. ACI 8-10 kHz", yaxt = "n")
dev.off()

# period 2 ENT
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[2]{1}_ENT.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period2_ENT <- rbind(old, new, sm4)

png("microphone test\\period2_ENT.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.0315, 0.655)
boxplot(period2_ENT$A_0Hz~period2_ENT$mic, ylim=ylim, 
        main="P2. ENT 0-2 kHz")
boxplot(period2_ENT$A_2000Hz~period2_ENT$mic, ylim=ylim, 
        main="P2. ENT 2-4 kHz", yaxt = "n")
boxplot(period2_ENT$A_4000Hz~period2_ENT$mic, ylim=ylim, 
        main="P2. ENT 4-6 kHz", yaxt = "n")
mtext(side = 3, "10pm to 12pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-07", line = -2.2, cex = 1.4)
boxplot(period2_ENT$A_6000Hz~period2_ENT$mic, ylim=ylim, 
        main="P2. ENT 6-8 kHz", yaxt = "n")
boxplot(period2_ENT$A_8000Hz~period2_ENT$mic, ylim=ylim, 
        main="P2. ENT 8-10 kHz", yaxt = "n")
dev.off()

# period 2 BGN
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[2]{1}_BGN.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period2_BGN <- rbind(old, new, sm4)

png("microphone test\\period2_BGN.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(-112,-70)
boxplot(period2_BGN$A_0Hz~period2_BGN$mic, ylim=ylim, 
        main="P2. BGN 0-2 kHz")
boxplot(period2_BGN$A_2000Hz~period2_BGN$mic, ylim=ylim,
        main="P2. BGN 2-4 kHz", yaxt = "n")
boxplot(period2_BGN$A_4000Hz~period2_BGN$mic, ylim=ylim, 
        main="P2. BGN 4-6 kHz", yaxt = "n")
mtext(side = 3, "10pm to 12pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-07", line = -2.2, cex = 1.4)
boxplot(period2_BGN$A_6000Hz~period2_BGN$mic, ylim=ylim, 
        main="P2. BGN 6-8 kHz", yaxt = "n")
boxplot(period2_BGN$A_8000Hz~period2_BGN$mic, ylim=ylim, 
        main="P2. BGN 8-10 kHz", yaxt = "n")
dev.off()
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#period 3 EVN
#rm(period2_BGN, period2_ENT, period2_ACI, period2_EVN)

folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[3]{1}_EVN.*\\.csv")
i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period3_EVN <- rbind(old, new, sm4)

png("microphone test\\period3_EVN.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.13, 3)
boxplot(period3_EVN$A_0Hz~period3_EVN$mic, 
        main="P3. EVN 0-2 kHz", ylim=ylim)
boxplot(period3_EVN$A_2000Hz~period3_EVN$mic, 
        main="P3. EVN 2-4 kHz", ylim=ylim, yaxt = "n")
boxplot(period3_EVN$A_4000Hz~period3_EVN$mic, 
        main="P3. EVN 4-6 kHz", ylim=ylim, yaxt = "n")
mtext(side = 3, "4am to 6am", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-08", line = -2.2, cex = 1.4)
boxplot(period3_EVN$A_6000Hz~period3_EVN$mic, 
        main="P3. EVN 6-8 kHz", ylim=ylim, yaxt = "n")
boxplot(period3_EVN$A_8000Hz~period3_EVN$mic, 
        main="P3. EVN 8-10 kHz", ylim=ylim, yaxt = "n")
dev.off()

# period 3 ACI
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[3]{1}_ACI.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period3_ACI <- rbind(old, new, sm4)

png("microphone test\\period3_ACI.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.393, 0.622)
boxplot(period3_ACI$A_0Hz~period3_ACI$mic, ylim=ylim, 
        main="P3. ACI 0-2 kHz")
boxplot(period3_ACI$A_2000Hz~period3_ACI$mic, ylim=ylim, 
        main="P3. ACI 2-4 kHz", yaxt = "n")
boxplot(period3_ACI$A_4000Hz~period3_ACI$mic, ylim=ylim, 
        main="P3. ACI 4-6 kHz", yaxt = "n")
mtext(side = 3, "4am to 6am", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-08", line = -2.2, cex = 1.4)
boxplot(period3_ACI$A_6000Hz~period3_ACI$mic, ylim=ylim, 
        main="P3. ACI 6-8 kHz", yaxt = "n")
boxplot(period3_ACI$A_8000Hz~period3_ACI$mic, ylim=ylim, 
        main="P3. ACI 8-10 kHz", yaxt = "n")
dev.off()

# period 3 ENT
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[3]{1}_ENT.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period3_ENT <- rbind(old, new, sm4)

png("microphone test\\period3_ENT.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.0315, 0.655)
boxplot(period3_ENT$A_0Hz~period3_ENT$mic, ylim=ylim, 
        main="P3. ENT 0-2 kHz")
boxplot(period3_ENT$A_2000Hz~period3_ENT$mic, ylim=ylim, 
        main="P3. ENT 2-4 kHz", yaxt = "n")
boxplot(period3_ENT$A_4000Hz~period3_ENT$mic, ylim=ylim, 
        main="P3. ENT 4-6 kHz", yaxt = "n")
mtext(side = 3, "4am to 6am", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-08", line = -2.2, cex = 1.4)
boxplot(period3_ENT$A_6000Hz~period3_ENT$mic, ylim=ylim, 
        main="P3. ENT 6-8 kHz", yaxt = "n")
boxplot(period3_ENT$A_8000Hz~period3_ENT$mic, ylim=ylim, 
        main="P3. ENT 8-10 kHz", yaxt = "n")
dev.off()

# period 3 BGN
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[3]{1}_BGN.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period3_BGN <- rbind(old, new, sm4)

png("microphone test\\period3_BGN.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(-112,-70)
boxplot(period3_BGN$A_0Hz~period3_BGN$mic, ylim=ylim, 
        main="P3. BGN 0-2 kHz")
boxplot(period3_BGN$A_2000Hz~period3_BGN$mic, ylim=ylim, 
        main="P3. BGN 2-4 kHz", yaxt = "n")
boxplot(period3_BGN$A_4000Hz~period3_BGN$mic, ylim=ylim, 
        main="P3. BGN 4-6 kHz", yaxt = "n")
mtext(side = 3, "4am to 6am", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-08", line = -2.2, cex = 1.4)
boxplot(period3_BGN$A_6000Hz~period3_BGN$mic, ylim=ylim,  
        main="P3. BGN 6-8 kHz", yaxt = "n")
boxplot(period3_BGN$A_8000Hz~period3_BGN$mic, ylim=ylim,
        main="P3. BGN 8-10 kHz", yaxt = "n")
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# period 4 EVN
#rm(period3_BGN, period3_ENT, period3_ACI, period3_EVN)

folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[4]{1}_EVN.*\\.csv")
i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period4_EVN <- rbind(old, new, sm4)

png("microphone test\\period4_EVN.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.13, 3)
boxplot(period4_EVN$A_0Hz~period4_EVN$mic, 
        main="P4. EVN 0-2 kHz", ylim=ylim)
boxplot(period4_EVN$A_2000Hz~period4_EVN$mic, ylim=ylim,
        main="P4. EVN 2-4 kHz", yaxt ="n")
boxplot(period4_EVN$A_4000Hz~period4_EVN$mic, ylim=ylim,
        main="P4. EVN 4-6 kHz", yaxt ="n")
mtext(side = 3, "10am to 12pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-09", line = -2.2, cex = 1.4)
boxplot(period4_EVN$A_6000Hz~period4_EVN$mic, ylim=ylim,
        main="P4. EVN 6-8 kHz", yaxt = "n")
boxplot(period4_EVN$A_8000Hz~period4_EVN$mic, ylim=ylim,
        main="P4. EVN 8-10 kHz", yaxt = "n")
dev.off()

# period 4 ACI
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[4]{1}_ACI.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period4_ACI <- rbind(old, new, sm4)

png("microphone test\\period4_ACI.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.393, 0.622)
boxplot(period4_ACI$A_0Hz~period4_ACI$mic, ylim=ylim, 
        main="P4. ACI 0-2 kHz")
boxplot(period4_ACI$A_2000Hz~period4_ACI$mic, ylim=ylim, 
        main="P4. ACI 2-4 kHz", yaxt = "n")
boxplot(period4_ACI$A_4000Hz~period4_ACI$mic, ylim=ylim,
        main="P4. ACI 4-6 kHz", yaxt = "n")
mtext(side = 3, "10am to 12pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-09", line = -2.2, cex = 1.4)
boxplot(period4_ACI$A_6000Hz~period4_ACI$mic, ylim=ylim,
        main="P4. ACI 6-8 kHz", yaxt = "n")
boxplot(period4_ACI$A_8000Hz~period4_ACI$mic, ylim=ylim, 
        main="P4. ACI 8-10 kHz", yaxt = "n")
dev.off()

# period 4 ENT
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[4]{1}_ENT.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period4_ENT <- rbind(old, new, sm4)

png("microphone test\\period4_ENT.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(0.0315, 0.655)
boxplot(period4_ENT$A_0Hz~period4_ENT$mic, ylim=ylim, 
        main="P4. ENT 0-2 kHz", border=c(1,1,1))
boxplot(period4_ENT$A_2000Hz~period4_ENT$mic, ylim=ylim, 
        main="P4. ENT 2-4 kHz", border=c(1,1,1), yaxt = "n")
boxplot(period4_ENT$A_4000Hz~period4_ENT$mic, ylim=ylim, 
        main="P4. ENT 4-6 kHz", border=c(1,1,1), yaxt = "n")
mtext(side = 3, "10am to 12pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-09", line = -2.2, cex = 1.4)
boxplot(period4_ENT$A_6000Hz~period4_ENT$mic, ylim=ylim, 
        main="P4. ENT 6-8 kHz", border=c(1,1,1), yaxt = "n")
boxplot(period4_ENT$A_8000Hz~period4_ENT$mic, ylim=ylim, 
        main="P4. ENT 8-10 kHz", border=c(1,1,1), yaxt = "n")
dev.off()

# period 4 BGN
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\microphone test"
my_files <- list.files(path=folder,recursive = TRUE, full.names = T,
                       pattern = "per[4]{1}_BGN.*\\.csv")

i=1
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
new <- data[,((length-4):length)]
rows <- nrow(data)
new <- cbind(rep("new",rows), new)
colnames(new)[1]  <- "mic"

i=2
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
old <- data[,((length-4):length)]
rows <- nrow(data)
old <- cbind(rep("old",rows), old)
colnames(old)[1]  <- "mic"

i=3
data <- read.csv(my_files[i], header = TRUE) 
length <- length(data)
sm4 <- data[,((length-4):length)]
rows <- nrow(data)
sm4 <- cbind(rep("sm4",rows), sm4)
colnames(sm4)[1]  <- "mic"

period4_BGN <- rbind(old, new, sm4)

png("microphone test\\period4_BGN.png", width = 800,
    height = 800)
par(mfrow=c(1,5), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(-112,-70)
boxplot(period4_BGN$A_0Hz~period4_BGN$mic, ylim=ylim,
        main="P4. BGN 0-2 kHz")
boxplot(period4_BGN$A_2000Hz~period4_BGN$mic, ylim=ylim, 
        main="P4. BGN 2-4 kHz", yaxt = "n")
boxplot(period4_BGN$A_4000Hz~period4_BGN$mic, ylim=ylim, 
        main="P4. BGN 4-6 kHz", yaxt = "n")
mtext(side = 3, "10am to 12pm", line = -4.2, cex = 1.4)
mtext(side = 3, "2016-08-09", line = -2.2, cex = 1.4)
boxplot(period4_BGN$A_6000Hz~period4_BGN$mic, ylim=ylim, 
        main="P4. BGN 6-8 kHz", yaxt = "n")
boxplot(period4_BGN$A_8000Hz~period4_BGN$mic, ylim=ylim,
        main="P4. BGN 8-10 kHz", yaxt = "n")
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Statistical analysis ---------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Shapiro test indicates the data is not normally distributed
# eg. shapiro.test(period4_ACI$A_2000Hz)
# p-value = 1.822e-13
# Therefore will need to use non parametric tests
# Mann Whitney test (also known as a Wilcox test). 
# Mann-Whitney requires that the variances are equal
#  if both x and y are given and paired is FALSE, a 
# Wilcoxon rank sum test (equivalent to the Mann-Whitney test
# By default (if exact is not specified), an exact p-value is 
# computed if the samples contain less than 50 finite values 
# and there are no ties. Otherwise, a normal approximation is used.
old <- 1:120
new <- 121:240
sm4 <- 241:360
# Wilcoxon signed rank test
wilcox.test(x = period1_ACI$A_0Hz[old],
            y = period1_ACI$A_0Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 2610, p-value = 0.007272 
wilcox.test(x = period1_ACI$A_0Hz[new],
            y = period1_ACI$A_0Hz[sm4],
            exact = TRUE, paired = TRUE) 
# V = 1647, p-value = 7.865e-08 (99.5%)
wilcox.test(x = period1_ACI$A_0Hz[old],
            y = period1_ACI$A_0Hz[sm4],
            exact = TRUE, paired = TRUE) 
# V = 1540, p-value = 1.29e-08 (99.5%)

wilcox.test(x = period1_ACI$A_2000Hz[old],
            y = period1_ACI$A_2000Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 174, p-value < 2.2e-16 (100%)
wilcox.test(x = period1_ACI$A_2000Hz[new],
            y = period1_ACI$A_2000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 556, p-value < 2.2e-16 (100%)
wilcox.test(x = period1_ACI$A_2000Hz[old],
            y = period1_ACI$A_2000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 21, p-value < 2.2e-16 (100%)

wilcox.test(x = period1_ACI$A_4000Hz[old],
            y = period1_ACI$A_4000Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 137, p-value < 2.2e-16 (100%)
wilcox.test(x = period1_ACI$A_4000Hz[new],
            y = period1_ACI$A_4000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 220, p-value < 2.2e-16 (100%)
wilcox.test(x = period1_ACI$A_4000Hz[old],
            y = period1_ACI$A_4000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 46, p-value < 2.2e-16 (100%)

wilcox.test(x = period1_ACI$A_6000Hz[old],
            y = period1_ACI$A_6000Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 629, p-value < 2.2e-16 (100%)
wilcox.test(x = period1_ACI$A_6000Hz[new],
            y = period1_ACI$A_6000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 139, p-value < 2.2e-16 (100%)
wilcox.test(x = period1_ACI$A_6000Hz[old],
            y = period1_ACI$A_6000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 117, p-value < 2.2e-16 (100%)

wilcox.test(x = period1_ACI$A_8000Hz[old],
            y = period1_ACI$A_8000Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 3366, p-value = 0.4915 
wilcox.test(x = period1_ACI$A_8000Hz[new],
            y = period1_ACI$A_8000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 299, p-value < 2.2e-16 (100%)
wilcox.test(x = period1_ACI$A_8000Hz[old],
            y = period1_ACI$A_8000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 305, p-value < 2.2e-16 (100%)

# Period 2
a <- wilcox.test(x = period2_ACI$A_0Hz[old],
            y = period2_ACI$A_0Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 3428, p-value = 0.599 
a$statistic #3428
a$p.value
a$method # "Wilcoxon signed rank test"
a$data.name #"period2_ACI$A_0Hz[old] and period2_ACI$A_0Hz[new]"

wilcox.test(x = period2_ACI$A_0Hz[new],
            y = period2_ACI$A_0Hz[sm4],
            exact = TRUE, paired = TRUE) 
# V = 6908, p-value < 2.2e-16 (100%)
wilcox.test(x = period2_ACI$A_0Hz[old],
            y = period2_ACI$A_0Hz[sm4],
            exact = TRUE, paired = TRUE) 
# V = 6886, p-value < 2.2e-16 (100%)

wilcox.test(x = period2_ACI$A_2000Hz[old],
            y = period2_ACI$A_2000Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 2648, p-value = 0.009813 (99%) * sum ranks > 2000
wilcox.test(x = period2_ACI$A_2000Hz[new],
            y = period2_ACI$A_2000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 556, p-value < 2.2e-16 (99.5%)
wilcox.test(x = period2_ACI$A_2000Hz[old],
            y = period2_ACI$A_2000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 21, p-value < 2.2e-16 (99.5%)

wilcox.test(x = period2_ACI$A_4000Hz[old],
            y = period2_ACI$A_4000Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 137, p-value < 2.2e-16 (99.5%)
wilcox.test(x = period2_ACI$A_4000Hz[new],
            y = period2_ACI$A_4000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 220, p-value < 2.2e-16 (99.5%)
wilcox.test(x = period2_ACI$A_4000Hz[old],
            y = period2_ACI$A_4000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 46, p-value < 2.2e-16 (99.5%)

wilcox.test(x = period2_ACI$A_6000Hz[old],
            y = period2_ACI$A_6000Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 629, p-value < 2.2e-16 (100%)
wilcox.test(x = period2_ACI$A_6000Hz[new],
            y = period2_ACI$A_6000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 139, p-value < 2.2e-16 (100%)
wilcox.test(x = period2_ACI$A_6000Hz[old],
            y = period2_ACI$A_6000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 117, p-value < 2.2e-16 (100%)

wilcox.test(x = period2_ACI$A_8000Hz[old],
            y = period2_ACI$A_8000Hz[new],
            exact = TRUE, paired = TRUE) 
#V = 3366, p-value = 0.4915 
wilcox.test(x = period2_ACI$A_8000Hz[new],
            y = period2_ACI$A_8000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 299, p-value < 2.2e-16 (99.5%)
wilcox.test(x = period2_ACI$A_8000Hz[old],
            y = period2_ACI$A_8000Hz[sm4],
            exact = TRUE, paired = TRUE) 
#V = 305, p-value < 2.2e-16 (99.5%)





all_periods_ACI <- rbind(period1_ACI, period2_ACI,
                         period3_ACI, period4_ACI)
bartlett.test(all_periods_ACI$A_0Hz~all_periods_ACI$mic) 

ACI <- rbind(period1_ACI[,c(1:6)],period2_ACI[,c(1:6)], period3_ACI[,c(1:6)], period4_ACI[,c(1:6)])
kruskal.test(ACI$A_0Hz~ACI$mic)
kruskal.test(ACI$A_2000Hz~ACI$mic)
kruskal.test(ACI$A_4000Hz~ACI$mic)
kruskal.test(ACI$A_6000Hz~ACI$mic)
kruskal.test(ACI$A_8000Hz~ACI$mic)

ACI <- rbind(period1_ACI[,c(1:6)],period2_ACI[,c(1:6)], period3_ACI[,c(1:6)], period4_ACI[,c(1:6)])
a <- which(ACI$mic=="old")
b <- which(ACI$mic=="new")
c <- which(ACI$mic=="sm4")
old <- ACI[a,]
new <- ACI[b,]
sm4 <- ACI[c,]
cor(old$A_0Hz,new$A_0Hz) #0.9561218
cor(old$A_0Hz,sm4$A_0Hz) #0.8444629
cor(new$A_0Hz,sm4$A_0Hz) #0.8767721
cor(old$A_2000Hz,new$A_2000Hz) #0.977489
cor(old$A_2000Hz,sm4$A_2000Hz) #0.8888036
cor(new$A_2000Hz,sm4$A_2000Hz) #0.9202489
cor(old$A_4000Hz,new$A_4000Hz) #0.9752195
cor(old$A_4000Hz,sm4$A_4000Hz) #0.8802141
cor(new$A_4000Hz,sm4$A_4000Hz) #0.9169267
cor(old$A_6000Hz,new$A_6000Hz) #0.9708282
cor(old$A_6000Hz,sm4$A_6000Hz) #0.7222259
cor(new$A_6000Hz,sm4$A_6000Hz) #0.7834202
cor(old$A_8000Hz,new$A_8000Hz) #0.7767836
cor(old$A_8000Hz,sm4$A_8000Hz) #0.5163818
cor(new$A_8000Hz,sm4$A_8000Hz) #0.5839773

ENT <- rbind(period1_ENT[,c(1:6)],period2_ENT[,c(1:6)], 
             period3_ENT[,c(1:6)], period4_ENT[,c(1:6)])
a <- which(ENT$mic=="old")
b <- which(ENT$mic=="new")
c <- which(ENT$mic=="sm4")
old <- ENT[a,]
new <- ENT[b,]
sm4 <- ENT[c,]
cor(old$A_0Hz,new$A_0Hz) #0.909994
cor(old$A_0Hz,sm4$A_0Hz) #0.6994123 
cor(new$A_0Hz,sm4$A_0Hz) #0.7701269
cor(old$A_2000Hz,new$A_2000Hz) #0.9737906
cor(old$A_2000Hz,sm4$A_2000Hz) #0.8615619
cor(new$A_2000Hz,sm4$A_2000Hz) #0.8979953
cor(old$A_4000Hz,new$A_4000Hz) #0.9738178
cor(old$A_4000Hz,sm4$A_4000Hz) #0.8787589
cor(new$A_4000Hz,sm4$A_4000Hz) #0.9254128
cor(old$A_6000Hz,new$A_6000Hz) #0.9787446
cor(old$A_6000Hz,sm4$A_6000Hz) #0.7309445
cor(new$A_6000Hz,sm4$A_6000Hz) #0.7851487
cor(old$A_8000Hz,new$A_8000Hz) #0.946094
cor(old$A_8000Hz,sm4$A_8000Hz) #0.4998672
cor(new$A_8000Hz,sm4$A_8000Hz) #0.5592104

EVN <- rbind(period1_EVN[,c(1:6)],period2_EVN[,c(1:6)], 
             period3_EVN[,c(1:6)], period4_EVN[,c(1:6)])
a <- which(ENT$mic=="old")
b <- which(ENT$mic=="new")
c <- which(ENT$mic=="sm4")
old <- EVN[a,]
new <- EVN[b,]
sm4 <- EVN[c,]
cor(old$A_0Hz,new$A_0Hz) #0.8543414
cor(old$A_0Hz,sm4$A_0Hz) #0.6978734
cor(new$A_0Hz,sm4$A_0Hz) #0.7992393
cor(old$A_2000Hz,new$A_2000Hz) #0.9773257
cor(old$A_2000Hz,sm4$A_2000Hz) #0.8870622
cor(new$A_2000Hz,sm4$A_2000Hz) #0.9308992
cor(old$A_4000Hz,new$A_4000Hz) #0.9761898
cor(old$A_4000Hz,sm4$A_4000Hz) #0.8660848
cor(new$A_4000Hz,sm4$A_4000Hz) #0.9301057
cor(old$A_6000Hz,new$A_6000Hz) #0.9588525
cor(old$A_6000Hz,sm4$A_6000Hz) #0.6649649
cor(new$A_6000Hz,sm4$A_6000Hz) #0.7565393
cor(old$A_8000Hz,new$A_8000Hz) #0.6544006
cor(old$A_8000Hz,sm4$A_8000Hz) #0.4798119
cor(new$A_8000Hz,sm4$A_8000Hz) #0.5782928

BGN <- rbind(period1_BGN[,c(1:6)],period2_BGN[,c(1:6)], 
             period3_BGN[,c(1:6)], period4_BGN[,c(1:6)])
a <- which(BGN$mic=="old")
b <- which(BGN$mic=="new")
c <- which(BGN$mic=="sm4")
old <- BGN[a,]
new <- BGN[b,]
sm4 <- BGN[c,]
cor(old$A_0Hz,new$A_0Hz) #0.799255
cor(old$A_0Hz,sm4$A_0Hz) #0.5676251
cor(new$A_0Hz,sm4$A_0Hz) #0.8315151
cor(old$A_2000Hz,new$A_2000Hz) #0.981531
cor(old$A_2000Hz,sm4$A_2000Hz) #0.8206495
cor(new$A_2000Hz,sm4$A_2000Hz) #0.8773363
cor(old$A_4000Hz,new$A_4000Hz) #0.9930025
cor(old$A_4000Hz,sm4$A_4000Hz) #0.8741131
cor(new$A_4000Hz,sm4$A_4000Hz) #0.913808
cor(old$A_6000Hz,new$A_6000Hz) #0.9761549
cor(old$A_6000Hz,sm4$A_6000Hz) #0.8002743
cor(new$A_6000Hz,sm4$A_6000Hz) #0.8477996
cor(old$A_8000Hz,new$A_8000Hz) #0.9113477
cor(old$A_8000Hz,sm4$A_8000Hz) #0.8441799
cor(new$A_8000Hz,sm4$A_8000Hz) #0.8908817

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Summary Indices ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
folder <- "E:\\Microphone test\\Old_microphone_test"
Files <- list.files(path=folder,recursive = TRUE, full.names = TRUE,
                    pattern = "*Towsey.Acoustic.Indices.csv")
short_name <- list.files(path=folder,recursive = TRUE, full.names = FALSE,
                         pattern = "*Towsey.Acoustic.Indices.csv")

old_dry <- NULL
for(i in 1:length(Files)) {
  datetime <- substr(short_name[i],5,19)
  datetime <- paste(substr(datetime,1,4),"-",
                    substr(datetime,5,6),"-",
                    substr(datetime,7,8)," ",
                    substr(datetime,10,11), ":",
                    substr(datetime,12,13), ":",
                    substr(datetime,14,15), " AEST", sep = "")
  #time <- as.numeric(time)
  a <- read.csv(Files[i])
  length <- nrow(a)
  startDate = as.POSIXct(datetime)
  a$time <- startDate + 60 * 0:(length-1) # makes a sequence in minutes
  old_dry <- rbind(old_dry, a)
}
old_dry <- old_dry[,-c(1:3,6,13,15:17,19,22:25)]
# save summary indices for old microphone
write.csv(old_dry, "microphone test/summary_old_dry.csv", row.names = FALSE)

# new microphone
folder <- "E:\\Microphone test\\New_microphone_test"
Files <- list.files(path=folder,recursive = TRUE, full.names = TRUE,
                    pattern = "*Towsey.Acoustic.Indices.csv")
short_name <- list.files(path=folder,recursive = TRUE, full.names = FALSE,
                    pattern = "*Towsey.Acoustic.Indices.csv")
new_dry <- NULL
for(i in 1:length(Files)) {
  datetime <- substr(short_name[i],5,19)
  datetime <- paste(substr(datetime,1,4),"-",
                    substr(datetime,5,6),"-",
                    substr(datetime,7,8)," ",
                    substr(datetime,10,11), ":",
                    substr(datetime,12,13), ":",
                    substr(datetime,14,15), " AEST", sep = "")
  #time <- as.numeric(time)
  a <- read.csv(Files[i])
  length <- nrow(a)
  startDate = as.POSIXct(datetime)
  a$time <- startDate + 60 * 0:(length-1) # makes a sequence in minutes
  new_dry <- rbind(new_dry, a)
}
new_dry <- new_dry[,-c(1:3,6,13,15:17,19,22:25)]
# save summary indices for new microphone
write.csv(new_dry, "microphone test/summary_new_dry.csv", row.names = FALSE)

# sm4
folder <- "E:\\Microphone test\\SM4_microphone_test"
Files <- list.files(path=folder,recursive = TRUE, full.names = TRUE,
                    pattern = "*Towsey.Acoustic.Indices.csv")
short_name <- list.files(path=folder,recursive = TRUE, full.names = FALSE,
                         pattern = "*Towsey.Acoustic.Indices.csv")

sm4_dry <- NULL
for(i in 1:length(Files)) {
  datetime <- substr(short_name[i],5,19)
  datetime <- paste(substr(datetime,1,4),"-",
                    substr(datetime,5,6),"-",
                    substr(datetime,7,8)," ",
                    substr(datetime,10,11), ":",
                    substr(datetime,12,13), ":",
                    substr(datetime,14,15), " AEST", sep = "")
  #time <- as.numeric(time)
  a <- read.csv(Files[i])
  length <- nrow(a)
  startDate = as.POSIXct(datetime)
  a$time <- startDate + 60 * 0:(length-1) # makes a sequence in minutes
  sm4_dry <- rbind(sm4_dry, a)
}
sm4_dry <- sm4_dry[,-c(1:3,6,13,15:17,19,22:25)]
# save summary indices for old microphone
write.csv(sm4_dry, "microphone test/summary_sm4_dry.csv", row.names = FALSE)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# gathering summary indices of four periods  ----------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Four periods
# 8 August 2016 6:00 to 6:15 am or first 15 minutes after 6 am
# 9 August 2016 6:00 to 6:15 am or first 15 minutes after 6 am
# 9 August 2016 12:00 to 12:15 pm or first 15 minutes after 6 am
# 10 August 2016 6:00 to 6:15 am or first 15 minutes after 6 am
#
# Period 1 old
old_dry <- read.csv("microphone test\\summary_old_dry.csv", header = T)
# period 1 on 7 August 2016
ref <- NULL
a <- which(substr(old_dry$time,1,10)=="2016-08-08" 
           & (substr(old_dry$time, 12,13)=="06"))
              
ref <- c(ref, a[1:15])
#per1_old_dry_summary <- old_dry[a,]
#write.csv(per1_old_dry_summary, "microphone test\\period1_summary_old_dry.csv", row.names = F)

# Period 2 old
old_dry <- read.csv("microphone test\\summary_old_dry.csv", header = T)
# period 2 on 7 August 2016
a <- which(substr(old_dry$time,1,10)=="2016-08-09" &
             (substr(old_dry$time, 12,13)=="06"))
ref <- c(ref, a[1:15])
#per2_old_dry_summary <- old_dry[a,]
#write.csv(per2_old_dry_summary, "microphone test\\period2_summary_old_dry.csv", row.names = F)

# Period 3 old
old_dry <- read.csv("microphone test\\summary_old_dry.csv", header = T)
# period 3 on 8 August 2016
a <- which(substr(old_dry$time,1,10)=="2016-08-09" 
           & (substr(old_dry$time, 12,13)=="12"))
ref <- c(ref, a[1:15])
#per3_old_dry_summary <- old_dry[a,]
#write.csv(per3_old_dry_summary, "microphone test\\period3_summary_old_dry.csv", row.names = F)

# Period 4 old
old_dry <- read.csv("microphone test\\summary_old_dry.csv", header = T)
# period 4 on 8 August 2016
a <- which(substr(old_dry$time,1,10)=="2016-08-10" 
           & (substr(old_dry$time, 12,13)=="06"))
ref <- c(ref,a[1:15])
#per4_old_dry_summary <- old_dry[a,]
#write.csv(per4_old_dry_summary, "microphone test\\period4_summary_old_dry.csv", row.names = F)
all_periods_old_dry_summary <- old_dry[ref,]
period <- rep(c("period1", "period2", "period3", "period4"), each = 15)
all_periods_old_dry_summary$period <- period
write.csv(all_periods_old_dry_summary, "microphone test\\all_periods_old_dry_summary.csv", row.names = F)

# Period 1 new
new_dry <- read.csv("microphone test\\summary_new_dry.csv", header = T)
# period 1 on 8 August 2016
ref <- NULL
a <- which(substr(new_dry$time,1,10)=="2016-08-08" 
           & (substr(new_dry$time, 12,13)=="06"))

ref <- c(ref, a[1:15])
#per1_new_dry_summary <- new_dry[a,]
#write.csv(per1_new_dry_summary, "microphone test\\period1_summary_new_dry.csv", row.names = F)

# Period 2 new
new_dry <- read.csv("microphone test\\summary_new_dry.csv", header = T)
# period 2 on 9 August 2016
a <- which(substr(new_dry$time,1,10)=="2016-08-09" &
             (substr(new_dry$time, 12,13)=="06"))
# adjustment required to align times
ref <- c(ref,a[1:15])
#per2_new_dry_summary <- new_dry[a,]
#write.csv(per2_new_dry_summary, "microphone test\\period2_summary_new_dry.csv", row.names = F)

# Period 3 new
new_dry <- read.csv("microphone test\\summary_new_dry.csv", header = T)
# period 3 on 9 August 2016
a <- which(substr(new_dry$time,1,10)=="2016-08-09" 
           & (substr(new_dry$time, 12,13)=="12"))
ref <- c(ref,a[1:15])
#per3_new_dry_summary <- new_dry[a,]
#write.csv(per3_new_dry_summary, "microphone test\\period3_summary_new_dry.csv", row.names = F)

# Period 4 new
new_dry <- read.csv("microphone test\\summary_new_dry.csv", header = T)
# period 4 on 10 August 2016
a <- which(substr(new_dry$time,1,10)=="2016-08-10" 
           & (substr(new_dry$time, 12,13)=="06"))
ref <- c(ref,a[1:15])
#per4_new_dry_summary <- new_dry[a,]
#write.csv(per4_new_dry_summary, "microphone test\\period4_summary_new_dry.csv", row.names = F)
all_periods_new_dry_summary <- new_dry[ref,]
period <- rep(c("period1", "period2", "period3", "period4"), each = 15)
all_periods_new_dry_summary$period <- period
write.csv(all_periods_new_dry_summary, "microphone test\\all_periods_new_dry_summary.csv", row.names = F)

#%%%%%%%
# Period 1 sm4
sm4_dry <- read.csv("microphone test\\summary_sm4_dry.csv", header = T)
# period 1 on 8 August 2016
ref <- NULL
a <- which(substr(sm4_dry$time,1,10)=="2016-08-08" 
           & (substr(sm4_dry$time, 12,13)=="06"))
ref <- c(ref,a[1:15])
#per1_sm4_dry_summary <- sm4_dry[a,]
#write.csv(per1_sm4_dry_summary, "microphone test\\period1_summary_sm4_dry.csv", row.names = F)

# Period 2 sm4
sm4_dry <- read.csv("microphone test\\summary_sm4_dry.csv", header = T)
# period 2 on 9 August 2016
a <- which(substr(sm4_dry$time,1,10)=="2016-08-09" &
             (substr(sm4_dry$time, 12,13)=="06"))
# adjustment required to align times
ref <- c(ref,a[1:15])
#per2_sm4_dry_summary <- sm4_dry[a,]
#write.csv(per2_sm4_dry_summary, "microphone test\\period2_summary_sm4_dry.csv", row.names = F)

# Period 3 sm4
sm4_dry <- read.csv("microphone test\\summary_sm4_dry.csv", header = T)
# period 3 on 9 August 2016
a <- which(substr(sm4_dry$time,1,10)=="2016-08-09" 
           & (substr(sm4_dry$time, 12,13)=="12"))
ref <- c(ref,a[1:15])
#per3_sm4_dry_summary <- sm4_dry[a,]
#write.csv(per3_sm4_dry_summary, "microphone test\\period3_summary_sm4_dry.csv", row.names = F)

# Period 4 sm4
sm4_dry <- read.csv("microphone test\\summary_sm4_dry.csv", header = T)
# period 4 on 10 August 2016
a <- which(substr(sm4_dry$time,1,10)=="2016-08-10" 
           & (substr(sm4_dry$time, 12,13)=="06"))
ref <- c(ref,a[1:15])
#per4_sm4_dry_summary <- sm4_dry[a,]
#write.csv(per4_sm4_dry_summary, "microphone test\\period4_summary_sm4_dry.csv", row.names = F)
all_periods_sm4_dry_summary <- sm4_dry[ref,]
period <- rep(c("period1", "period2", "period3", "period4"), each = 15)
all_periods_sm4_dry_summary$period <- period
write.csv(all_periods_sm4_dry_summary, "microphone test\\all_periods_sm4_dry_summary.csv", row.names = F)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# summary indices boxplots  ----------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

all_periods_sm4_dry_summary <- read.csv("microphone test\\all_periods_sm4_dry_summary.csv", header = T)
all_periods_old_dry_summary <- read.csv("microphone test\\all_periods_old_dry_summary.csv", header = T)
all_periods_new_dry_summary <- read.csv("microphone test\\all_periods_new_dry_summary.csv", header = T)

par(mfrow=c(1,1), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=2)
ylim <- c(-113, 6)

period1 <- rbind(all_periods_old_dry_summary[1:15,],
                  all_periods_new_dry_summary[1:15,],
                  all_periods_sm4_dry_summary[1:15,])
period1$label <- rep(c("a. old", "b. new", "c. sm4"), each = 120)
period1$label <- substr(period1$FileName,1,3)
#period1$label <- as.factor(period1$label)
period1$label <- ordered(period1$label, levels=c("OLD", "NEW", "SM4"))

boxplot(period1$BackgroundNoise~period1$label,
        main="Background Noise - Period 1")
boxplot(period1$Snr~period1$label,
        main="Signal to Noise Ratio - Period 1")
boxplot(period1$Activity~period1$label,
        main="Activity - Period 1")
View(period1)
shapiro.test(period1$Activity[1:15])
shapiro.test(period1$Activity[16:30])
shapiro.test(period1$Activity[31:45])

boxplot(period1$EventsPerSecond~period1$label,
        main="Events per second - Period 1")
boxplot(period1$HighFreqCover~period1$label,
        main="High Frequency Cover - Period 1")
boxplot(period1$MidFreqCover~period1$label,
        main="Mid Frequency Cover - Period 1")
boxplot(period1$LowFreqCover~period1$label,
        main="Low Frequency Cover - Period 1")
boxplot(period1$AcousticComplexity~period1$label,
        main="Acoustic Complexity - Period 1")
boxplot(period1$EntropyOfAverageSpectrum~period1$label,
        main="Entropy of Average Spectrum - Period 1")
boxplot(period1$EntropyOfPeaksSpectrum~period1$label,
        main="Entropy of Peaks Spectrum - Period 1")
boxplot(period1$EntropyOfCoVSpectrum~period1$label,
        main="Entropy Of CoV Spectrum - Period 1")
boxplot(period1$ClusterCount~period1$label,
        main="Cluster Count - Period 1")

period2 <- rbind(all_periods_old_dry_summary[16:30,],
                 all_periods_new_dry_summary[16:30,],
                 all_periods_sm4_dry_summary[16:30,])
period2$label <- substr(period2$FileName, 1, 3)
period2$label <- ordered(period2$label, levels=c("OLD", "NEW", "SM4"))

boxplot(period2$BackgroundNoise~period2$label,
        main="Background Noise - Period 2")
boxplot(period2$Snr~period2$label,
        main="Signal to Noise Ratio - Period 2")
boxplot(period2$Activity~period2$label,
        main="Activity - Period 2")
boxplot(period2$EventsPerSecond~period2$label,
        main="Events per second - Period 2")
boxplot(period2$HighFreqCover~period2$label,
        main="High Frequency Cover - Period 2")
boxplot(period2$MidFreqCover~period2$label,
        main="Mid Frequency Cover - Period 2")
boxplot(period2$LowFreqCover~period2$label,
        main="Low Frequency Cover - Period 2")
boxplot(period2$AcousticComplexity~period2$label,
        main="Acoustic Complexity - Period 2")
boxplot(period2$EntropyOfAverageSpectrum~period2$label,
        main="Entropy of Average Spectrum - Period 2")
boxplot(period2$EntropyOfPeaksSpectrum~period2$label,
        main="Entropy of Peaks Spectrum - Period 2")
boxplot(period2$EntropyOfCoVSpectrum~period2$label,
        main="Entropy Of CoV Spectrum - Period 2")
boxplot(period2$ClusterCount~period2$label,
        main="Cluster Count - Period 2")

period3 <- rbind(all_periods_old_dry_summary[31:45,],
                 all_periods_new_dry_summary[31:45,],
                 all_periods_sm4_dry_summary[31:45,])
period3$label <- substr(period3$FileName, 1, 3)
period3$label <- ordered(period3$label, levels=c("OLD", "NEW", "SM4"))
period3$label <- ordered(period3$label, levels=c("OLD", "NEW", "SM4"))

boxplot(period3$BackgroundNoise~period3$label,
        main="Background Noise - Period 3")
boxplot(period3$Snr~period3$label,
        main="Signal to Noise Ratio - Period 3")
boxplot(period3$Activity~period3$label,
        main="Activity - Period 3")
boxplot(period3$EventsPerSecond~period3$label,
        main="Events per second - Period 3")
boxplot(period3$HighFreqCover~period3$label,
        main="High Frequency Cover - Period 3")
boxplot(period3$MidFreqCover~period3$label,
        main="Mid Frequency Cover - Period 3")
boxplot(period3$LowFreqCover~period3$label,
        main="Low Frequency Cover - Period 3")
boxplot(period3$AcousticComplexity~period3$label,
        main="Acoustic Complexity - Period 3")
boxplot(period3$EntropyOfAverageSpectrum~period3$label,
        main="Entropy of Average Spectrum - Period 3")
boxplot(period3$EntropyOfPeaksSpectrum~period3$label,
        main="Entropy of Peaks Spectrum - Period 3")
boxplot(period3$EntropyOfCoVSpectrum~period3$label,
        main="Entropy Of CoV Spectrum - Period 3")
boxplot(period3$ClusterCount~period3$label,
        main="Cluster Count - Period 3")

period4 <- rbind(all_periods_old_dry_summary[46:60,],
                 all_periods_new_dry_summary[46:60,],
                 all_periods_sm4_dry_summary[46:60,])
period4$label <- substr(period4$FileName, 1, 3)
period4$label <- ordered(period4$label, levels=c("OLD", "NEW", "SM4"))

boxplot(period4$BackgroundNoise~period4$label,
        main="Background Noise - Period 4")
boxplot(period4$Snr~period4$label,
        main="Signal to Noise Ratio - Period 4")
boxplot(period4$Activity~period4$label,
        main="Activity - Period 4")
boxplot(period4$EventsPerSecond~period4$label,
        main="Events per second - Period 4")
boxplot(period4$HighFreqCover~period4$label,
        main="High Frequency Cover - Period 4")
boxplot(period4$MidFreqCover~period4$label,
        main="Mid Frequency Cover - Period 4")
boxplot(period4$LowFreqCover~period4$label,
        main="Low Frequency Cover - Period 4")
boxplot(period4$AcousticComplexity~period4$label,
        main="Acoustic Complexity - Period 4")
boxplot(period4$EntropyOfAverageSpectrum~period4$label,
        main="Entropy of Average Spectrum - Period 4")
boxplot(period4$EntropyOfPeaksSpectrum~period4$label,
        main="Entropy of Peaks Spectrum - Period 4")
boxplot(period4$EntropyOfCoVSpectrum~period4$label,
        main="Entropy Of CoV Spectrum - Period 4")
boxplot(period4$ClusterCount~period4$label,
        main="Cluster Count - Period 4")

par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(-50, -25)
boxplot(period1$BackgroundNoise~period1$label,
        main="P1.BGN", ylim=ylim)
boxplot(period2$BackgroundNoise~period2$label,
        main="P2.BGN", ylim=ylim, yaxt = "n")
boxplot(period3$BackgroundNoise~period3$label,
        main="P3.BGN", ylim=ylim, yaxt = "n")
boxplot(period4$BackgroundNoise~period4$label,
        main="P4.BGN", ylim=ylim, yaxt = "n")

par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(0, 36)
boxplot(period1$Snr~period1$label,
        main="P1.SNR", ylim=ylim)
boxplot(period2$Snr~period2$label,
        main="P2.SNR", ylim=ylim, yaxt = "n")
boxplot(period3$Snr~period3$label,
        main="P3.SNR", ylim=ylim, yaxt = "n")
boxplot(period4$Snr~period4$label,
        main="P4.SNR", ylim=ylim, yaxt = "n")

par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(0, 1)
boxplot(period1$Activity~period1$label,
        main="P1.ACT", ylim=ylim)
boxplot(period2$Activity~period2$label,
        main="P2.ACT", ylim=ylim, yaxt = "n")
boxplot(period3$Activity~period3$label,
        main="P3.ACT", ylim=ylim, yaxt = "n")
boxplot(period4$Activity~period4$label,
        main="P4.ACT", ylim=ylim, yaxt = "n")

par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(0,6.5)
boxplot(period1$EventsPerSecond~period1$label,
        main="P1.EVN", ylim=ylim)
boxplot(period2$EventsPerSecond~period2$label,
        main="P2.EVN", ylim=ylim, yaxt = "n")
boxplot(period3$EventsPerSecond~period3$label,
        main="P3.EVN", ylim=ylim, yaxt = "n")
boxplot(period4$EventsPerSecond~period4$label,
        main="P4.ENV", ylim=ylim, yaxt = "n")

par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(0,0.38)
boxplot(period1$HighFreqCover~period1$label,
        main="P1.HFC", ylim=ylim)
boxplot(period2$HighFreqCover~period2$label,
        main="P2.HFC", ylim=ylim, yaxt = "n")
boxplot(period3$HighFreqCover~period3$label,
        main="P3.HFC", ylim=ylim, yaxt = "n")
boxplot(period4$HighFreqCover~period4$label,
        main="P4.HFC", ylim=ylim, yaxt = "n")

par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(0,0.45)
boxplot(period1$MidFreqCover~period1$label,
        main="P1.MFC", ylim=ylim)
boxplot(period2$MidFreqCover~period2$label,
        main="P2.MFC", ylim=ylim, yaxt = "n")
boxplot(period3$MidFreqCover~period3$label,
        main="P3.MFC", ylim=ylim, yaxt = "n")
boxplot(period4$MidFreqCover~period4$label,
        main="P4.MFC", ylim=ylim, yaxt = "n")

par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(0,0.45)
boxplot(period1$LowFreqCover~period1$label,
        main="P1.LFC", ylim=ylim)
boxplot(period2$LowFreqCover~period2$label,
        main="P2.LFC", ylim=ylim, yaxt = "n")
boxplot(period3$LowFreqCover~period3$label,
        main="P3.LFC", ylim=ylim, yaxt = "n")
boxplot(period4$LowFreqCover~period4$label,
        main="P4.LFC", ylim=ylim, yaxt = "n")

par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(0.36,0.7)
boxplot(period1$AcousticComplexity~period1$label,
        main="P1.ACI", ylim=ylim)
boxplot(period2$AcousticComplexity~period2$label,
        main="P2.ACI", ylim=ylim, yaxt = "n")
boxplot(period3$AcousticComplexity~period3$label,
        main="P3.ACI", ylim=ylim, yaxt = "n")
boxplot(period4$AcousticComplexity~period4$label,
        main="P4.ACI", ylim=ylim, yaxt = "n")


par(mfrow=c(1,4), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(0,1.1)
boxplot(period1$EntropyOfAverageSpectrum~period1$label,
        main="P1.EAS", ylim=ylim)
boxplot(period2$EntropyOfAverageSpectrum~period2$label,
        main="P2.EAS", ylim=ylim, yaxt = "n")
boxplot(period3$EntropyOfAverageSpectrum~period3$label,
        main="P3.EAS", ylim=ylim, yaxt = "n")
boxplot(period4$EntropyOfAverageSpectrum~period4$label,
        main="P4.EAS", ylim=ylim, yaxt = "n")


shapiro.test(period1$MidFreqCover)
shapiro.test((period1$MidFreqCover[1:120]))
shapiro.test((period1$MidFreqCover[121:240]))
shapiro.test((period4$MidFreqCover[1:120]))
shapiro.test((period4$MidFreqCover[241:360])) #W = 0.98405, p-value = 0.1687
shapiro.test((period4$MidFreqCover[121:240])) #W = 0.98889, p-value = 0.4407
shapiro.test((period2$MidFreqCover[241:360]))
shapiro.test((period4$HighFreqCover[1:120]))
shapiro.test((period4$HighFreqCover[241:360])) #W = 0.98405, p-value = 0.1687
shapiro.test((period4$MidFreqCover[1:120]))
shapiro.test((period4$EventsPerSecond[1:120])) # Almost
shapiro.test((period2$Activity[241:360])) # Almost


#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
old <- read.csv("microphone test\\summary_old_dry.csv", header = T)
#old <- read.csv("microphone test\\summary_new_dry.csv", header = T)
#old <- read.csv("microphone test\\summary_sm4_dry.csv", header = T)

hour <- c("14","15","16","17","18","19","20",
          "21","22","23","00","01","02","03",
          "04","05","06","07","08","09","10",
          "11","12","13","14","15","16","17",
          "18","19","20","21","22","23","00",
          "01","02","03","04","05","06","07",
          "08","09","10","11","12","13","14",
          "15","16","17","18","19","20","21",
          "22","23","00","01","02","03","04",
          "05","06","07")
date <- c("2016-08-07", "2016-08-08",
          "2016-08-09", "2016-08-10")

j <- 1
shapiro <- NULL
dat <- NULL
tim <- NULL
ind <- NULL
for(i in 1:length(hour)) {
  a <- which(substr(old$time,1,10)==date[j] 
             & (substr(old$time, 12,13)==hour[i]))
  if(as.numeric(hour[i])==23) {
    j <- j + 1
  }
  for(k in 1:12) {
    if(length(a) > 0) {
      if(sum(!old[a,k])==0) {
        s <- shapiro.test(old[a[1:30],k]) 
        s <- s$p.value
        shapiro <- c(shapiro, s)
        dat <- c(dat, date[j])
        tim <- c(tim, hour[i])
        index <- colnames(old[k])
        ind <- c(ind, index)
      }
      if(sum(old[a,k])==0) {
        s <- rep(0,1)
        shapiro <- c(shapiro, s)
        dats <- rep(date[j],1) 
        dat <- c(dat, dats)
        hours <- rep(hour[i],1)
        tim <- c(tim, hours)
        index <- colnames(old[k])
        ind <- c(ind, index)
      }
    }
  }
}
y <- which(shapiro > 0.05)
y
shapiro[y]

shapiro_test <- cbind(dat, tim, shapiro, ind)
View(shapiro_test[y,])

#%%%%%%%%%%%%%%%%%%%%%%%
png("microphone test\\Microphone test_BGN_2016_08_07_16_00_53_to_17_59_53.png",
    width = 1200, height = 800)
par(mfrow=c(1,3), mar=c(3,3,2,0), cex.axis=2, 
    cex.main=2, oma= c(0,0,0,1))
per1_BGN <- read.csv("microphone test\\per1_BGN_old_dry.csv", header = T)
ylim = c(-112,-23)
plot(per1_BGN$A_0Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_2000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_4000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_4000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_6000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_8000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(summary_old$BackgroundNoise[213:332], type = "l", 
     col="purple", ylim = ylim, lwd = 2,
     main = "BGN SM2 Old microphone")
par(new=FALSE)
per1_BGN <- read.csv("microphone test\\per1_BGN_new_dry.csv", header = T)
plot(per1_BGN$A_0Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_2000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_4000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_4000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_6000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_8000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(summary_new$BackgroundNoise[213:332], type = "l", 
     col="purple", ylim = ylim, lwd = 2,
     main = "BGN SM2 New microphone")
par(new=FALSE)
per1_BGN <- read.csv("microphone test\\per1_BGN_sm4_dry.csv", header = T)
plot(per1_BGN$A_0Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_2000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_4000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_4000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_6000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(per1_BGN$A_8000Hz, type = "l", ylim = ylim)
par(new=TRUE)
plot(summary_sm4$BackgroundNoise[213:332], type = "l", 
     col="purple", ylim = ylim, lwd = 2,
     main = "BGN SM4 New microphone")
dev.off()



summary_old <- read.csv("microphone test\\summary_old_dry.csv",
                        header = T)
summary_new <- read.csv("microphone test\\summary_new_dry.csv",
                        header = T)
summary_sm4 <- read.csv("microphone test\\summary_sm4_dry.csv",
                        header = T)
png("microphone test\\Summary Indices Old New SM4.png",
    width = 1200, height = 800)
par(mfrow=c(1,3), mar=c(2.1,0,2.8,0),oma=c(0,2.8,0,1), cex.main=2,
    cex.axis=1.8)
ylim = c(-52, -21)
boxplot(summary_old$BackgroundNoise,
        main="Summary Indices OLD BGN", 
        ylim=ylim,
        sub = "7 August 2016 1600-1800")
mtext(side = 3, line = -2,  cex = 1.2,
      "7 August 2016 1600-1800")
boxplot(summary_new$BackgroundNoise, 
        main="Summary Indices NEW BGN", 
        ylim=ylim, yaxt = "n",
        sub = "7 August 2016 1600-1800")
mtext(side = 3, line = -2,  cex = 1.2,
      "7 August 2016 1600-1800")
boxplot(summary_sm4$BackgroundNoise,
        main="Summary Indices SM4 BGN", 
        ylim=ylim, yaxt = "n")
mtext(side = 3, line = -2, cex = 1.2,
      "7 August 2016 1600-1800")
dev.off()

par(mfrow=c(1,3), mar=c(3,3,2,0), cex.axis=2, 
    cex.main=2, oma= c(0,0,0,1))
per1_BGN <- read.csv("microphone test\\per1_BGN_old_dry.csv", header = T)
ylim = c(-150,-23) 
for(i in 2:256) {
  plot(per1_BGN[,i], type = "l", ylim = ylim)
  par(new=TRUE)
}
par(new=FALSE)
per1_BGN <- read.csv("microphone test\\per1_BGN_new_dry.csv", header = T)
for(i in 2:256) {
  plot(per1_BGN[,i], type = "l", ylim = ylim)
  par(new=TRUE)
}
par(new=FALSE)
per1_BGN <- read.csv("microphone test\\per1_BGN_sm4_dry.csv", header = T)
for(i in 2:256) {
  plot(per1_BGN[,i], type = "l", ylim = ylim)
  par(new=TRUE)
}
###############

dev.off()
png("microphone test\\per1_EVN_old_dry.png",
    width = 1200, height = 800)
par(mfrow=c(1,3), mar=c(3,3,2,0), cex.axis=2, 
    cex.main=2, oma= c(0,0,0,1))
per1_EVN <- read.csv("microphone test\\per1_EVN_old_dry.csv", header = T)
ylim = c(0,2.8) 
col = c("red", "orange", "yellow", "green","blue")
j <- 0
for(i in 259:263) {
  j <- j + 1
  plot(per1_EVN[,i], type = "l", ylim = ylim, col=col[j])
  par(new=TRUE)
}
par(new=FALSE)
per1_ACI <- read.csv("microphone test\\per1_EVN_new_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  j <- j + 1
  plot(per1_EVN[,i], type = "l", ylim = ylim, col=col[j])
  par(new=TRUE)
}
par(new=FALSE)
per1_EVN <- read.csv("microphone test\\per1_EVN_sm4_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  j <- j +1
  plot(per1_EVN[,i], type = "l", ylim = ylim, col=col[j])
  par(new=TRUE)
}
dev.off()

dev.off()
png("microphone test\\per1_EVN_old_dry.png",
    width = 1200, height = 800)
par(mfrow=c(3,5), mar=c(3,3,2,0), cex.axis=2, 
    cex.main=2, oma= c(0,0,0,1))
per1a_EVN <- read.csv("microphone test\\per1_EVN_old_dry.csv", header = T)
ylim = c(0, 2.8) 
col = c("red", "orange", "yellow", "green","blue")
j <- 0
for(i in 259:263) {
  j <- j + 1
  plot(per1a_EVN[,i], type = "l", ylim = ylim, col=col[j])
  #par(new=TRUE)
}
par(new=FALSE)
per1b_EVN <- read.csv("microphone test\\per1_EVN_new_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  j <- j + 1
  plot(per1b_EVN[,i], type = "l", ylim = ylim, col=col[j])
  #par(new=TRUE)
}
par(new=FALSE)
per1c_EVN <- read.csv("microphone test\\per1_EVN_sm4_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  j <- j + 1
  plot(per1c_EVN[,i], type = "l", ylim = ylim, col=col[j])
  #par(new=TRUE)
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Period 1 ACI
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%
dev.off()
png("microphone test\\per1_ACI_dry.png",
    width = 1200, height = 600)
#col = c("red", "orange", "yellow", "green", "blue")
col = rep("black", 5)
range <- c("0-2000Hz", "2000-4000Hz","4000-6000Hz",
           "6000-8000Hz","8000-10000Hz")
par(mfrow=c(3,5), mar=c(0,0,0,0), cex.axis=2, 
    cex.main=2, oma= c(3, 3, 2.2, 1), cex = 1)
per1a_ACI <- read.csv("microphone test\\per1_ACI_old_dry.csv", 
                      header = T)
ylim = c(0.35, 0.85) 
line <- seq(ylim[1]-0.05, ylim[2], by = 0.1)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1a_ACI[,i], type = "l", ylim = ylim, col=col[j],
         xaxt = "n")
    #par(new=TRUE)
    title(paste(range[j]), line = -1.5, outer = F)
    title(" T1", line = -1.5, adj=0)
    title(" Period 1 ACI", line = 0.7, adj=0, outer = T)
    abline(h=line, lwd=0.1, lty =2)
  }
  if (i > 259) {
    j <- j + 1
    plot(per1a_ACI[,i], type = "l", ylim = ylim, col=col[j],
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)
    title(paste(range[j]), line = -1.5, outer = F)
    title(" T1", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
}
par(new=FALSE)
per1b_ACI <- read.csv("microphone test\\per1_ACI_new_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1b_ACI[,i], type = "l", ylim = ylim, 
         col=col[j], xaxt = "n")
    #par(new=TRUE)  
    title(" T2", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
  if (i > 259) {
    j <- j + 1
    plot(per1b_ACI[,i], type = "l", ylim = ylim, col=col[j], 
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)  
    title(" T2", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
}
par(new=FALSE)
per1c_ACI <- read.csv("microphone test\\per1_ACI_sm4_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1c_ACI[,i], type = "l", ylim = ylim, 
         col=col[j], xaxt = "n")
    #par(new=TRUE)  
    title(" T3", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
    if(i < 263) {
      axis(side = 1, at = c(0, 40, 80))  
    }
    if(i == 263) {
      axis(side = 1, at = c(0, 40, 80, 120))  
    }
  }
  if (i > 259) {
    j <- j + 1
    plot(per1c_ACI[,i], type = "l", ylim = ylim, col=col[j],
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)
    title(" T3", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
    if(i < 263) {
      axis(side = 1, at = c(0, 40, 80))  
    }
    if(i == 263) {
      axis(side = 1, at = c(0, 40, 80, 120))  
    }
  }
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Period 1 EVN
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%
dev.off()
png("microphone test\\per1_EVN_dry.png",
    width = 1200, height = 600)
ylim = c(0, 2.9) 
#col = c("red", "orange", "yellow", "green", "blue")
col = rep("black", 5)
range <- c("0-2000Hz", "2000-4000Hz","4000-6000Hz",
           "6000-8000Hz","8000-10000Hz")
par(mfrow=c(3,5), mar=c(0,0,0,0), cex.axis=2, 
    cex.main=2, oma= c(3, 3, 2.2, 1), cex = 1)
per1a_EVN <- read.csv("microphone test\\per1_EVN_old_dry.csv", 
                      header = T)
line <- seq(ylim[1], ylim[2], by = 0.5)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1a_EVN[,i], type = "l", ylim = ylim, col=col[j],
         xaxt = "n", yaxt = "n")
    axis(side = 2, at = line)
    #par(new=TRUE)
    title(paste(range[j]), line = -1.5, outer = F)
    title(" T1", line = -1.5, adj=0)
    title(" Period 1 EVN", line = 0.7, adj=0, outer = T)
    abline(h=line, lwd=0.1, lty =2)
  }
  if (i > 259) {
    j <- j + 1
    plot(per1a_EVN[,i], type = "l", ylim = ylim, col=col[j],
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)
    title(paste(range[j]), line = -1.5, outer = F)
    title(" T1", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
}
par(new=FALSE)
per1b_EVN <- read.csv("microphone test\\per1_EVN_new_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1b_EVN[,i], type = "l", ylim = ylim, 
         col=col[j], xaxt = "n", yaxt = "n")
    axis(side = 2, at = line)
    #par(new=TRUE)  
    title(" T2", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
  if (i > 259) {
    j <- j + 1
    plot(per1b_EVN[,i], type = "l", ylim = ylim, col=col[j], 
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)  
    title(" T2", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
}
par(new=FALSE)
per1c_EVN <- read.csv("microphone test\\per1_EVN_sm4_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1c_EVN[,i], type = "l", ylim = ylim, 
         col=col[j], xaxt = "n", yaxt = "n")
    axis(side = 2, at = line)
    #par(new=TRUE)  
    title(" T3", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
    if(i < 263) {
      axis(side = 1, at = c(0, 40, 80))  
    }
    if(i == 263) {
      axis(side = 1, at = c(0, 40, 80, 120))  
    }
  }
  if (i > 259) {
    j <- j + 1
    plot(per1c_EVN[,i], type = "l", ylim = ylim, col=col[j],
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)
    title(" T3", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
    if(i < 263) {
      axis(side = 1, at = c(0, 40, 80))  
    }
    if(i == 263) {
      axis(side = 1, at = c(0, 40, 80, 120))  
    }
  }
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%
dev.off()
png("microphone test\\per1_ENT_dry.png",
    width = 1200, height = 600)
ylim = c(0, 0.75) 
#col = c("red", "orange", "yellow", "green", "blue")
col = rep("black", 5)
range <- c("0-2000Hz", "2000-4000Hz","4000-6000Hz",
           "6000-8000Hz","8000-10000Hz")
par(mfrow=c(3,5), mar=c(0,0,0,0), cex.axis=2, 
    cex.main=2, oma= c(3, 3, 2.2, 1), cex = 1)
per1a_ENT <- read.csv("microphone test\\per1_ENT_old_dry.csv", 
                      header = T)
line <- seq(ylim[1], ylim[2], by = 0.5)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1a_ENT[,i], type = "l", ylim = ylim, col=col[j],
         xaxt = "n", yaxt = "n")
    axis(side = 2, at = line)
    #par(new=TRUE)
    title(paste(range[j]), line = -1.5, outer = F)
    title(" T1", line = -1.5, adj=0)
    title(" Period 1 ENT", line = 0.7, adj=0, outer = T)
    abline(h=line, lwd=0.1, lty =2)
  }
  if (i > 259) {
    j <- j + 1
    plot(per1a_ENT[,i], type = "l", ylim = ylim, col=col[j],
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)
    title(paste(range[j]), line = -1.5, outer = F)
    title(" T1", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
}
par(new=FALSE)
per1b_ENT <- read.csv("microphone test\\per1_ENT_new_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1b_ENT[,i], type = "l", ylim = ylim, 
         col=col[j], xaxt = "n", yaxt = "n")
    axis(side = 2, at = line)
    #par(new=TRUE)  
    title(" T2", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
  if (i > 259) {
    j <- j + 1
    plot(per1b_ENT[,i], type = "l", ylim = ylim, col=col[j], 
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)  
    title(" T2", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
}
par(new=FALSE)
per1c_ENT <- read.csv("microphone test\\per1_ENT_sm4_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1c_ENT[,i], type = "l", ylim = ylim, 
         col=col[j], yaxt = "n", xaxt= "n")
    axis(side = 2, at = line)
    #par(new=TRUE)  
    title(" T3", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
    axis(side = 1, at = c(0, 40, 80))
  }
  if (i > 259) {
    j <- j + 1
    plot(per1c_ENT[,i], type = "l", ylim = ylim, col=col[j],
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)
    title(" T3", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
    if(i < 263) {
      axis(side = 1, at = c(0, 40, 80))  
    }
    if(i == 263) {
      axis(side = 1, at = c(0, 40, 80, 120))  
    }
  }
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%
dev.off()
png("microphone test\\per1_BGN_dry.png",
    width = 1200, height = 600)
ylim = c(-112, -60) 
line <- seq(ylim[1]+2, ylim[2], by = 10)
#col = c("red", "orange", "yellow", "green", "blue")
col = rep("black", 5)
range <- c("0-2000Hz", "2000-4000Hz","4000-6000Hz",
           "6000-8000Hz","8000-10000Hz")
par(mfrow=c(3,5), mar=c(0,0,0,0), cex.axis=2, 
    cex.main=2, oma= c(3, 3, 2.2, 1), cex = 1)
per1a_BGN <- read.csv("microphone test\\per1_BGN_old_dry.csv", 
                      header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1a_BGN[,i], type = "l", ylim = ylim, col=col[j],
         xaxt = "n", yaxt = "n")
    axis(side = 2, at = line)
    #par(new=TRUE)
    title(paste(range[j]), line = -1.5, outer = F)
    title(" T1", line = -1.5, adj=0)
    title(" Period 1 BGN", line = 0.7, adj=0, outer = T)
    abline(h=line, lwd=0.1, lty =2)
  }
  if (i > 259) {
    j <- j + 1
    plot(per1a_BGN[,i], type = "l", ylim = ylim, col=col[j],
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)
    title(paste(range[j]), line = -1.5, outer = F)
    title(" T1", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
}
par(new=FALSE)
per1b_BGN <- read.csv("microphone test\\per1_BGN_new_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1b_BGN[,i], type = "l", ylim = ylim, 
         col=col[j], xaxt = "n", yaxt = "n")
    axis(side = 2, at = line)
    #par(new=TRUE)  
    title(" T2", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
  if (i > 259) {
    j <- j + 1
    plot(per1b_BGN[,i], type = "l", ylim = ylim, col=col[j], 
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)  
    title(" T2", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
  }
}
par(new=FALSE)
per1c_BGN <- read.csv("microphone test\\per1_BGN_sm4_dry.csv", header = T)
j <- 0
for(i in 259:263) {
  if(i == 259) {
    j <- j + 1
    plot(per1c_BGN[,i], type = "l", ylim = ylim, 
         col=col[j], yaxt = "n", xaxt= "n")
    axis(side = 2, at = line)
    #par(new=TRUE)  
    title(" T3", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
    axis(side = 1, at = c(0, 40, 80))
  }
  if (i > 259) {
    j <- j + 1
    plot(per1c_BGN[,i], type = "l", ylim = ylim, col=col[j],
         yaxt = "n", xaxt = "n")
    #par(new=TRUE)
    title(" T3", line = -1.5, adj=0)
    abline(h=line, lwd=0.1, lty =2)
    if(i < 263) {
      axis(side = 1, at = c(0, 40, 80))  
    }
    if(i == 263) {
      axis(side = 1, at = c(0, 40, 80, 120))  
    }
  }
}
dev.off()
