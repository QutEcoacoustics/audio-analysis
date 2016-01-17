setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j")
# results in columns are in wrong order ie files on availae contain columns
# that go from high frequency values to low frequency values

# set cluster number 
cluster.no = 9
n = 1000 # number of minutes used in the spectrum plots
sample.size = 1400 # Must be smaller or equal to the smallest cluster size
############
cluster.list <- read.csv("hybrid_clust_knn_17500_3.csv",header=T)[,6]
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\ClusterImages\\")
indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)
NA.ref <- which(is.na(indices$BackgroundNoise))
missing.ref <- c(207394:207766)
missing.ref <- c(NA.ref, missing.ref)
list <- which(indices$minute.of.day=="0")

lst1 <- NULL
for (i in 1:length(list)) {
  lst <- list[i+1]-1
  lst1 <- c(lst1, lst)
}
list <- cbind(list, lst1)
colnames(list) <- c("start","end")
list <- as.data.frame(list)
list[length(list$start),2] <- length(indices$X)

# Use if getting values from Results folder on availae
# Get list of names (full path) and dates from all spectrogram files
#path <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\" # path to indices files
#spect_file_Gympie <- list.files(full.names=TRUE, path = paste(path,"GympieNP",sep="")) 
#spect_file_Woondum <- list.files(full.names=TRUE, path = paste(path,"Woondum3",sep="")) 
#spect_file_Gympie_date <- list.files(full.names=FALSE, path = paste(path,"GympieNP",sep="")) 
#spect_file_Woondum_date <- list.files(full.names=FALSE, path = paste(path,"Woondum3",sep="")) 
#spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)
#length(spect_file_list)

path <- "E:\\Cooloola Results\\"
spect_file_Gympie <- list.files(full.names=TRUE, path = paste(path,"GympieNP","\\ACI\\",sep="")) 
spect_file_Woondum <- list.files(full.names=TRUE, path = paste(path,"Woondum3","\\ACI\\",sep="")) 
spect_file_Gympie_date <- list.files(full.names=FALSE, path = paste(path,"GympieNP","\\ACI\\",sep="")) 
spect_file_Woondum_date <- list.files(full.names=FALSE, path = paste(path,"Woondum3","\\ACI\\",sep="")) 
spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)
length(spect_file_list)
######### cluster 
# Get list of positions of cluster 1
which1 <- which(cluster.list==cluster.no)

# remove from which1 list any minute that lies the missing data section
# this includes for now the morning of 25 July 2015 at Woondum
# The list of these is obtained from finding which minute contains
# NA in the indices
which.missing <- which(which1 %in% missing.ref) 
which1 <- which1[-c(which.missing)]

# <- which1[which1 != which1[which.missing[i]]]

redSpectrum1   <- NULL  # ACI
greenSpectrum1 <- NULL  # Temp Entropy
blueSpectrum1  <- NULL  # Events
redSpectrum2   <- NULL  # BGN
greenSpectrum2  <- NULL  # Power
redSpect1Gympie <- NULL
greenSpect1Gympie <- NULL
blueSpect1Gympie  <- NULL
redSpect2Gympie   <- NULL
greenSpect2Gympie <- NULL
redSpect1Woondum  <- NULL
greenSpect1Woondum <- NULL
blueSpect1Woondum  <- NULL
redSpect2Woondum   <- NULL
greenSpect2Woondum <- NULL

# Select a random sample from a cluster
whichV1 <- sample(which1, sample.size)
write.csv(whichV1, paste("which_cluster", cluster.no,".csv",sep = ""), row.names = F)

for(i in 1:n) {  
  # if the minute (whichV1) is the last minute of the day
  # keep the day reference equal to this integer
  if(whichV1[i] %in% c(seq(1440, length(spect_file_list)*1440, 1440))) {
    day.ref <- floor((whichV1[i])/1440)
  }
  # else any other minute of the day increase the integer by one
  # day 1 comes from 0 plus 1
  else {
    day.ref <- floor((whichV1[i])/1440)+1
  }
  min.ref <- round((((whichV1[i]/1440) - (day.ref-1))*1440),0)
  if(day.ref <= 111) { {
    #redSpect1 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__ACI.csv",sep = ""))[min.ref,]
    #greenSpect1 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__ENT.csv",sep = ""))[min.ref,]
    #blueSpect1 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__EVN.csv",sep = ""))[min.ref,]
    #redSpect2 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__BGN.csv",sep = ""))[min.ref,]
    #greenSpect2 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__POW.csv",sep = ""))[min.ref,]
    
    redSpect1 <- read.csv(paste(path,"\\GympieNP\\ACI\\",substr(spect_file_Gympie_date[day.ref],1,17),"__ACI.csv",sep = ""))[min.ref,]
    greenSpect1 <- read.csv(paste(path,"\\GympieNP\\ENT\\",substr(spect_file_Gympie_date[day.ref],1,17),"__ENT.csv",sep = ""))[min.ref,]
    blueSpect1 <- read.csv(paste(path,"\\GympieNP\\EVN\\",substr(spect_file_Gympie_date[day.ref],1,17),"__EVN.csv",sep = ""))[min.ref,]
    redSpect2 <- read.csv(paste(path,"\\GympieNP\\BGN\\",substr(spect_file_Gympie_date[day.ref],1,17),"__BGN.csv",sep = ""))[min.ref,]
    greenSpect2 <- read.csv(paste(path,"\\GympieNP\\POW\\",substr(spect_file_Gympie_date[day.ref],1,17),"__POW.csv",sep = ""))[min.ref,]
  }
    redSpect1Gympie <- rbind(redSpect1Gympie, redSpect1)
    greenSpect1Gympie <- rbind(greenSpect1Gympie, greenSpect1)
    blueSpect1Gympie <- rbind(blueSpect1Gympie, blueSpect1)
    redSpect2Gympie <- rbind(redSpect2Gympie, redSpect2)
    greenSpect2Gympie <- rbind(greenSpect2Gympie, greenSpect2)
  }
  if(day.ref > 111) { {
    #redSpect1 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__ACI.csv",sep = ""))[min.ref,]
    #greenSpect1 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__ENT.csv",sep = ""))[min.ref,]
    #blueSpect1 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__EVN.csv",sep = ""))[min.ref,]
    #redSpect2 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__BGN.csv",sep = ""))[min.ref,]
    #greenSpect2 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__POW.csv",sep = ""))[min.ref,]
    
    redSpect1 <- read.csv(paste(path,"\\Woondum3\\ACI\\",substr(spect_file_Woondum_date[day.ref-111],1,17),"__ACI.csv",sep = ""))[min.ref,]
    greenSpect1 <- read.csv(paste(path,"\\Woondum3\\ENT\\",substr(spect_file_Woondum_date[day.ref-111],1,17),"__ENT.csv",sep = ""))[min.ref,]
    blueSpect1 <- read.csv(paste(path,"\\Woondum3\\EVN\\",substr(spect_file_Woondum_date[day.ref-111],1,17),"__EVN.csv",sep = ""))[min.ref,]
    redSpect2 <- read.csv(paste(path,"\\Woondum3\\BGN\\",substr(spect_file_Woondum_date[day.ref-111],1,17),"__BGN.csv",sep = ""))[min.ref,]
    greenSpect2 <- read.csv(paste(path,"\\Woondum3\\POW\\",substr(spect_file_Woondum_date[day.ref-111],1,17),"__POW.csv",sep = ""))[min.ref,]
  }
    redSpect1Woondum <- rbind(redSpect1Woondum, redSpect1)
    greenSpect1Woondum <- rbind(greenSpect1Woondum, greenSpect1)
    blueSpect1Woondum <- rbind(blueSpect1Woondum, blueSpect1)
    redSpect2Woondum <- rbind(redSpect2Woondum, redSpect2)
    greenSpect2Woondum <- rbind(greenSpect2Woondum, greenSpect2)
  }
  redSpectrum1 <- rbind(redSpectrum1, redSpect1)
  greenSpectrum1 <- rbind(greenSpectrum1, greenSpect1)
  blueSpectrum1 <- rbind(blueSpectrum1, blueSpect1)
  redSpectrum2 <- rbind(redSpectrum2, redSpect2)
  greenSpectrum2 <- rbind(greenSpectrum2, greenSpect2)
}

write.csv(redSpectrum1, paste("ACI_cluster",cluster.no,".csv",sep = ""),row.names = F)
write.csv(greenSpectrum1, paste("ENT_cluster",cluster.no,".csv",sep = ""),row.names = F)
write.csv(blueSpectrum1, paste("EVN_cluster",cluster.no,".csv",sep = ""),row.names = F)
write.csv(redSpectrum2, paste("BGN_cluster",cluster.no,".csv",sep = ""),row.names = F)
write.csv(greenSpectrum2, paste("POW_cluster",cluster.no,".csv",sep = ""),row.names = F)

write.csv(redSpect1Gympie, paste("ACI_cluster_Gympie",cluster.no,".csv",sep = "") ,row.names = F)
write.csv(greenSpect1Gympie, paste("ENT_cluster_Gympie",cluster.no,".csv",sep = "") ,row.names = F)
write.csv(blueSpect1Gympie, paste("EVN_cluster_Gympie",cluster.no,".csv",sep = "") ,row.names = F)
write.csv(redSpect2Gympie, paste("BGN_cluster_Gympie",cluster.no,".csv",sep = "") ,row.names = F)
write.csv(greenSpect2Gympie, paste("POW_cluster_Gympie",cluster.no,".csv",sep = "") ,row.names = F)

write.csv(redSpect1Woondum, paste("ACI_cluster_Woondum",cluster.no,".csv",sep = "") ,row.names = F)
write.csv(greenSpect1Woondum, paste("ENT_cluster_Woondum",cluster.no,".csv",sep = "") ,row.names = F)
write.csv(blueSpect1Woondum, paste("EVN_cluster_Woondum",cluster.no,".csv",sep = "") ,row.names = F)
write.csv(redSpect2Woondum, paste("BGN_cluster_Woondum",cluster.no,".csv",sep = "") ,row.names = F)
write.csv(greenSpect2Woondum, paste("POW_cluster_Woondum",cluster.no,".csv",sep = "") ,row.names = F)

at <- seq(0, 256, 23.15)
labels <- as.character(0:11)
#png(paste("ACIspectrum_cluster", cluster.no, ".png", sep = ""),
#    height = 600, width = 800)
#plot(colMeans(redSpectrum1[256:3], na.rm = T), type="l",
#     xlab="Frequency (kHz)", xaxt="n", ylab="ACI", 
#     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #ACI
#axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
#mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
#mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
#abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
#dev.off()

#png(paste("TEMPENTspectrum_cluster", cluster.no, ".png", sep = ""),
#    height = 600, width = 800)
#plot(colMeans(greenSpectrum1[256:3], na.rm = T), type="l",
#     xlab="Frequency (kHz)", xaxt="n",ylab="TEMP ENT", 
#     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) # TEMPORAL ENTROPY
#axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
#mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
#mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
#abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
#dev.off()

#png(paste("EVENTSspectrum_cluster", cluster.no, ".png", sep = ""),
#    height = 600, width = 800)
#plot(colMeans(blueSpectrum1[256:3], na.rm = T),type="l",
#     xlab="Frequency (kHz)", xaxt="n",ylab="EVENTS", 
#     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #EVENTS
#axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
#mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
#mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
#abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
#dev.off()

#png(paste("BNGspectrum_cluster", cluster.no, ".png", sep = ""),
#    height = 600, width = 800)
#plot(colMeans(redSpectrum2[256:3], na.rm = T), type="l",
#     xlab="Frequency (kHz)", xaxt="n",ylab="BGN", 
#     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5)  #BGN
#axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
#mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
#mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
#abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
#dev.off()

#png(paste("POWspectrum_cluster", cluster.no, ".png", sep = ""),
#    height = 600, width = 800)
#plot(colMeans(greenSpectrum2[256:3], na.rm = T), type="l",
#     xlab="Frequency (kHz)", xaxt="n", ylab="POW", 
#     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) # POW
#axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
#mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
#mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
#abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
#dev.off()

# Separate Gympie and Woondum plots
# ACI plots
png(paste("ACIspectrum_cluster", cluster.no, "Gympie.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(redSpect1Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="ACI", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #ACI
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(redSpect1Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

png(paste("ACIspectrum_cluster", cluster.no, "Woondum.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(redSpect1Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="ACI", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #ACI
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(redSpect1Woondum$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# "TEMP ENT" plots
png(paste("TEMPENTspectrum_cluster", cluster.no, "Gympie.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(greenSpect1Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="TEMP ENT", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #TEMP ENT
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(greenSpect1Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

png(paste("TEMPENTspectrum_cluster", cluster.no, "Woondum.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(greenSpect1Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="TEMP ENT", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #TEMP ENT
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(greenSpect1Woondum$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# EVENTS plots
png(paste("EVENTSspectrum_cluster", cluster.no, "Gympie.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(blueSpect1Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="EVENTS", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #EVENTS
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(blueSpect1Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

png(paste("EVENTSspectrum_cluster", cluster.no, "Woondum.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(blueSpect1Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="EVENTS", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #EVENTS
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(blueSpect1Woondum$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# BACKGROUND plots
png(paste("BNGspectrum_cluster", cluster.no, "Gympie.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(redSpect2Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="BGN", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #BACKGROUND
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(redSpect2Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

png(paste("BNGspectrum_cluster", cluster.no, "Woondum.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(redSpect2Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="BGN", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #BACKGROUND
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(redSpect2Woondum$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# POW plots
png(paste("POWspectrum_cluster", cluster.no, "Gympie.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(greenSpect2Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="POW", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #POWER
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(greenSpect2Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

png(paste("POWspectrum_cluster", cluster.no, "Woondum.png", sep = ""),
    height = 600, width = 800)
plot(colMeans(greenSpect2Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="POW", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #POWER
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(greenSpect2Woondum$Index) , sep = ""))
mtext(side = 3, line = 1, paste("cluster =", cluster.no, sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# Overall, gympie and Woondum ACI
png(paste("ACI_all_",cluster.no,".png",sep=""), height=300, width=1250)
par(mfrow=c(1,3),oma=c(0,1,0,0))
plot(colMeans(redSpectrum1[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n", ylab="ACI", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #ACI
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(redSpect1Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="ACI", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #ACI
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(redSpect1Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(redSpect1Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="ACI", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #ACI
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(redSpect1Woondum$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# Overall, gympie and Woondum TEMP ENT
png(paste("TEMPENT_all_",cluster.no,".png",sep=""), height=300, width=1250)
par(mfrow=c(1,3),oma=c(0,1,0,0))
plot(colMeans(greenSpectrum1[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="TEMP ENT", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) # TEMPORAL ENTROPY
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(greenSpect1Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="TEMP ENT", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #TEMP ENT
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(greenSpect1Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(greenSpect1Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="TEMP ENT", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #TEMP ENT
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(greenSpect1Woondum$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# Overall, gympie and Woondum TEMP ENT
png(paste("EVENTS_all_",cluster.no,".png",sep = ""), height=300, width=1250)
par(mfrow=c(1,3),oma=c(0,1,0,0))
plot(colMeans(blueSpectrum1[256:3], na.rm = T),type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="EVENTS", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #EVENTS
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(blueSpect1Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="EVENTS", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #EVENTS
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(blueSpect1Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(blueSpect1Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="EVENTS", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #EVENTS
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(blueSpect1Woondum$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# Overall, gympie and Woondum BACKGR
png(paste("BGN_all_",cluster.no, ".png",sep = ""), height=300, width=1250)
par(mfrow=c(1,3),oma=c(0,1,0,0))
plot(colMeans(redSpectrum2[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="BGN", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5)  #BGN
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(redSpect2Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="BGN", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #BACKGROUND
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(redSpect2Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(redSpect2Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="BGN", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #BACKGROUND
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(redSpect2Woondum$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()

# Overall, gympie and Woondum POWER
png(paste("POW_all_",cluster.no, ".png",sep = ""), height=300, width=1250)
par(mfrow=c(1,3),oma=c(0,1,0,0))
plot(colMeans(greenSpectrum2[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n", ylab="POW", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) # POW
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 1, paste("minutes = ", n, sep = ""))
mtext(side = 3, line = 0, paste("cluster =",cluster.no,sep = ""))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(greenSpect2Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="POW", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #POWER
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(greenSpect2Gympie$Index), sep = ""))
mtext(side = 3, line = 1, paste("cluster =",cluster.no,sep = ""))
mtext(side = 3, line = 0, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)

plot(colMeans(greenSpect2Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n",ylab="POW", 
     lwd=1.5, cex.axis = 1.5, cex.lab = 1.5) #POWER
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, paste("minutes = ", length(greenSpect2Woondum$Index) , sep = ""))
mtext(side = 3, line = 1, paste("cluster =", cluster.no, sep = ""))
mtext(side = 3, line = 0, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
dev.off()
