setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j")
# results in columns are in wrong order ie files on availae contain columns
# that go from high frequency values to low frequency values
cluster.no <-16
name.as.char <- "Sept_5am_C16_Gym_Woondum"
name.as.char1 <- "Sept_5am_C16_Gympie"
name.as.char2 <- "Sept_5am_C16_Woondum"
Sept_5am_C16_Woon$list_Woondum <- Sept_5am_C16_Woon$list_Woondum + 111*1440
name <- c(Sept_5am_C16_Gym$list_Gympie, Sept_5am_C16_Woon$list_Woondum)
write.csv(name,paste(name.as.char,".csv"),row.names = F)
cluster.list <- name
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

path <- "E:\\Cooloola Results\\"
spect_file_Gympie <- list.files(full.names=TRUE, path = paste(path,"GympieNP","\\ACI\\",sep="")) 
spect_file_Woondum <- list.files(full.names=TRUE, path = paste(path,"Woondum3","\\ACI\\",sep="")) 
spect_file_Gympie_date <- list.files(full.names=FALSE, path = paste(path,"GympieNP","\\ACI\\",sep="")) 
spect_file_Woondum_date <- list.files(full.names=FALSE, path = paste(path,"Woondum3","\\ACI\\",sep="")) 
spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)
length(spect_file_list)
######### cluster 
# Get list of positions of cluster 1
which1 <- name

# remove from which1 list any minute that lies the missing data section
# this includes for now the morning of 25 July 2015 at Woondum
# The list of these is obtained from finding which minute contains
# NA in the indices
which.missing <- which(which1 %in% missing.ref) 
if(length(which.missing > 0)) {
  which1 <- which1[-c(which.missing)]  
}

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
whichV1 <- sample(which1, length(which1))

for(i in 1:length(whichV1)) {  
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

write.csv(redSpectrum1, paste("ACI_cluster", name.as.char,".csv",sep = ""),row.names = F)
write.csv(greenSpectrum1, paste("ENT_cluster", name.as.char,".csv",sep = ""),row.names = F)
write.csv(blueSpectrum1, paste("EVN_cluster", name.as.char,".csv",sep = ""),row.names = F)
write.csv(redSpectrum2, paste("BGN_cluster", name.as.char,".csv",sep = ""),row.names = F)
write.csv(greenSpectrum2, paste("POW_cluster", name.as.char,".csv",sep = ""),row.names = F)

write.csv(redSpect1Gympie, paste("ACI_cluster_Gympie",name.as.char1,".csv",sep = "") ,row.names = F)
write.csv(greenSpect1Gympie, paste("ENT_cluster_Gympie",name.as.char1,".csv",sep = "") ,row.names = F)
write.csv(blueSpect1Gympie, paste("EVN_cluster_Gympie",name.as.char1,".csv",sep = "") ,row.names = F)
write.csv(redSpect2Gympie, paste("BGN_cluster_Gympie",name.as.char1,".csv",sep = "") ,row.names = F)
write.csv(greenSpect2Gympie, paste("POW_cluster_Gympie",name.as.char1,".csv",sep = "") ,row.names = F)

write.csv(redSpect1Woondum, paste("ACI_cluster_Woondum",name.as.char2,".csv",sep = "") ,row.names = F)
write.csv(greenSpect1Woondum, paste("ENT_cluster_Woondum",name.as.char2,".csv",sep = "") ,row.names = F)
write.csv(blueSpect1Woondum, paste("EVN_cluster_Woondum",name.as.char2,".csv",sep = "") ,row.names = F)
write.csv(redSpect2Woondum, paste("BGN_cluster_Woondum",name.as.char2,".csv",sep = "") ,row.names = F)
write.csv(greenSpect2Woondum, paste("POW_cluster_Woondum",name.as.char2,".csv",sep = "") ,row.names = F)


#GYMPIE and WOONDUM
at <- seq(0, 256, 23.15)
labels <- as.character(0:11)

png(paste(name.as.char,".png",sep=""), height=480, width=1200)
par(mfrow=c(1,2), mar=c(5, 2, 4, 1))
plot(colMeans(redSpect1Gympie[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n", ylab = "", yaxt="n", 
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, 
     col="orange", ylim = c(0.39, 0.62)) #ACI drop back to 0.61
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, cex = 1.5, paste("minutes = ", length(redSpect1Gympie$c000000), sep = ""))
mtext(side = 3, line = 1,  cex = 1.5, paste("cluster =",cluster.no," ",name.as.char1, sep = ""))
mtext(side = 3, line = 0,  cex = 1.5, paste("Gympie National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
par(new=T)
plot(colMeans(greenSpect1Gympie[256:3], na.rm = T), type="l",
     xaxt="n", xlab = "",#ylab="TEMP ENT", 
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, ylab = "",yaxt="n",
     col= "cyan", ylim = c(0, 0.37)) #TEMP ENT Drop back to 0.36
par(new=T)
plot(colMeans(blueSpect1Gympie[256:3], na.rm = T), type="l",
     xaxt="n", xlab = "", 
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, ylab = "", yaxt="n",
     col= "blue", ylim = c(0, 2.2)) #EVENTS
par(new=T)
plot(colMeans(redSpect2Gympie[256:3], na.rm = T), type="l",yaxt="n",
     xaxt="n", xlab = "",
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, ylab = "",
     col= "red", ylim = c(-105, -70)) #BACKGROUND
par(new=T)
plot(colMeans(greenSpect2Gympie[256:3], na.rm = T), type="l",yaxt="n",
     xaxt="n", xlab = "",
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, ylab = "",yaxt="n",
     col= "green4", ylim = c(0, 4)) #POWER
legend("topright",legend=c("ACI","Temporal Entropy","Events","Background","Power"),          
       col=c("orange", "cyan", "blue", "red", "green4"),lty = 1, 
       lwd=2, xjust=1,y.intersp=0.8, cex=1.5,
       inset = .02, bty = "n")
#png(paste("Spectra_Woondum_",cluster.no,".png",sep=""), height=600, width=600)
#par(lwd=2)
plot(colMeans(redSpect1Woondum[256:3], na.rm = T), type="l",
     xlab="Frequency (kHz)", xaxt="n", ylab = "", yaxt="n", 
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, 
     col= "orange", ylim = c(0.39, 0.62)) #ACI drop back to 0.61
axis(side = 1, at=at, labels=labels, cex.axis = 1.5)
mtext(side = 3, line = 2, cex = 1.5, paste("minutes = ", length(redSpect1Woondum$c000000), sep = ""))
mtext(side = 3, line = 1,  cex = 1.5, paste("cluster =",cluster.no, " ", name.as.char2, sep = ""))
mtext(side = 3, line = 0,  cex = 1.5, paste("Woondum National Park"))
abline (v=seq(0, 256, 46.3), lty=2, lwd=0.2)
par(new=T)
plot(colMeans(greenSpect1Woondum[256:3], na.rm = T), type="l",
     xaxt="n", xlab = "",#ylab="TEMP ENT", 
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, ylab = "",yaxt="n",
     col= "cyan", ylim = c(0, 0.37)) #TEMP ENT Drop back to 0.36
par(new=T)
plot(colMeans(blueSpect1Woondum[256:3], na.rm = T), type="l",
     xaxt="n", xlab = "", 
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, ylab = "", yaxt="n",
     col= "blue", ylim = c(0, 2.2)) #EVENTS
par(new=T)
plot(colMeans(redSpect2Woondum[256:3], na.rm = T), type="l",yaxt="n",
     xaxt="n", xlab = "",
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, ylab = "",
     col= "red", ylim = c(-105,-70)) #BACKGROUND
par(new=T)
plot(colMeans(greenSpect2Woondum[256:3], na.rm = T), type="l",yaxt="n",
     xaxt="n", xlab = "",
     lwd=2, cex.axis = 1.5, cex.lab = 1.5, ylab = "",yaxt="n",
     col= "green4", ylim = c(0,4)) #POWER
legend("topright",legend=c("ACI","Temporal Entropy","Events","Background","Power"),          
       col=c("orange", "cyan", "blue", "red", "green4"),lty = 1, 
       lwd=2, xjust=1,y.intersp=0.8, cex=1.5,
       inset = .02, bty = "n")
dev.off()

# Use this for a short-cut way of plotting without rerunning the code above
# once the csv files have been generated

redSpect1Gympie <- read.csv(paste("ACI_cluster_Gympie", cluster.no,".csv", sep=""))[,2:257]
greenSpect1Gympie <- read.csv(paste("ENT_cluster_Gympie", cluster.no,".csv", sep=""))[,2:257]
blueSpect1Gympie <- read.csv(paste("EVN_cluster_Gympie", cluster.no,".csv", sep=""))[,2:257]
redSpect2Gympie <- read.csv(paste("BGN_cluster_Gympie", cluster.no,".csv", sep=""))[,2:257]
greenSpect2Gympie <- read.csv(paste("POW_cluster_Gympie", cluster.no,".csv", sep=""))[,2:257]

redSpect1Woondum <- read.csv(paste("ACI_cluster_Woondum", cluster.no,".csv", sep=""))[,2:257]
greenSpect1Woondum <- read.csv(paste("ENT_cluster_Woondum", cluster.no,".csv", sep=""))[,2:257]
blueSpect1Woondum <- read.csv(paste("EVN_cluster_Woondum", cluster.no,".csv", sep=""))[,2:257]
redSpect2Woondum <- read.csv(paste("BGN_cluster_Woondum", cluster.no,".csv", sep=""))[,2:257]
greenSpect2Woondum <- read.csv(paste("POW_cluster_Woondum", cluster.no,".csv", sep=""))[,2:257]