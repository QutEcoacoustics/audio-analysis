setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j")

cluster.list <- read.csv("hybrid_clust_knn_17500_3.csv",header=T)[,6]
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\ClusterImages\\")
indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)

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

library(raster)
image <- "Rasterimage.png"
s <- brick(image, package="raster", ncol=1280, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

# Get list of names (full path) of all spectrogram files
path <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\" # path to spectrogram files
spect_file_Gympie <- list.files(full.names=TRUE, path = paste(path,"GympieNP",sep="")) 
spect_file_Woondum <- list.files(full.names=TRUE, path = paste(path,"Woondum3",sep="")) 
spect_file_Gympie_date <- list.files(full.names=FALSE, path = paste(path,"GympieNP",sep="")) 
spect_file_Woondum_date <- list.files(full.names=FALSE, path = paste(path,"Woondum3",sep="")) 
spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)
length(spect_file_list)

######### cluster 1
# Get list of positions of cluster 1
which1 <- which(cluster.list==1)
redSpectrum1 <- NULL
greenSpectrum1 <- NULL
blueSpectrum1 <- NULL
redSpectrum2 <- NULL
greenSpectrum2 <- NULL

# Select a random sample from a cluster
whichV1 <- sample(which1, 1200)
for(i in 1:12) {
  if(whichV1[i] %in% c(seq(1440, length(spect_file_list)*1440, 1440))) {
    day.ref <- floor((whichV1[i])/1440)
  }
  #else {
  #  day.ref <- floor((whichV1[i])/1440)+1
  #}
  min.ref <- ((whichV1[i]/1440) - (day.ref-1))*1440
  if(day.ref <= 111) {
    redSpect1 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__ACI.csv",sep = ""))[min.ref,]
    greenSpect1 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__ENT.csv",sep = ""))[min.ref,]
    blueSpect1 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__EVN.csv",sep = ""))[min.ref,]
    redSpect2 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__BGN.csv",sep = ""))[min.ref,]
    greenSpect2 <- read.csv(paste(spect_file_Gympie[day.ref],"\\GympieNP_",spect_file_Gympie_date[day.ref],"__POW.csv",sep = ""))[min.ref,]
  }
  
  if(day.ref > 111) {
    redSpect1 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__ACI.csv",sep = ""))[min.ref,]
    greenSpect1 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__ENT.csv",sep = ""))[min.ref,]
    blueSpect1 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__EVN.csv",sep = ""))[min.ref,]
    redSpect2 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__BGN.csv",sep = ""))[min.ref,]
    greenSpect2 <- read.csv(paste(spect_file_Woondum[day.ref-111],"\\Woondum3_",spect_file_Woondum_date[day.ref-111],"__POW.csv",sep = ""))[min.ref,]
  }
  redSpectrum1 <- rbind(redSpectrum1, redSpect1)
  greenSpectrum1 <- rbind(greenSpectrum1, greenSpect1)
  blueSpectrum1 <- rbind(blueSpectrum1, blueSpect1)
  redSpectrum2 <- rbind(redSpectrum2, redSpect2)
  greenSpectrum2 <- rbind(greenSpectrum2, redSpect2)
}

plot(colMeans(redSpectrum1[4:200], na.rm = T), ylim=c(0.4,0.45), type="l")  #ACI
plot(colMeans(greenSpectrum1), type="l") # TEMPORAL ENTROPY
plot(colMeans(blueSpectrum1),type="l") #EVENTS
plot(colMeans(redSpectrum2), type="l")  #BGN
plot(colMeans(redSpectrum2), type="l") # POW
plot(colMeans(blueSpectrum2),type="l") #EVENTS
