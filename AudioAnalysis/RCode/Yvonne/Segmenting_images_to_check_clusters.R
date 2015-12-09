# 30 November 2015
##############################################
# This file produces images that show 600 randomly selected minutes
# from each cluster
# NOTE: This file requires a png file dimensions width=620px and 
# height 668px in the wd folder, these files can be easily 
# generated in paint.
##############################################
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3i")

cluster.list <- read.csv("hybrid_clust_knn_17500_3.csv",header=T)[,7]
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
s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

# Get list of names (full path) of all spectrogram files
path <- "E:\\False colour spectrograms" # path to spectrogram files
spect_file_Gympie <- list.files(full.names=TRUE, path = paste(path,"\\GympieNP",sep="")) 
spect_file_Woondum <- list.files(full.names=TRUE, path = paste(path,"\\WoondumNP",sep="")) 
spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)
length(spect_file_list)

######### cluster 1
# Get list of positions of cluster 1
which1 <- which(cluster.list==1)
# Select a random sample from a cluster
whichV1 <- sample(which1, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V1_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
min.ref.check <- NULL
which.check <- NULL
for(i in 1:600) {
  if(whichV1[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV1[i])/1440)
  }
  else {
    day.ref <- floor((whichV1[i])/1440)+1
  }
  min.ref <- ((whichV1[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
  which.check <- c(which.check,whichV1[i])
  min.ref.check <- c(min.ref.check, min.ref)
}

plotRGB(s)
dev.off()

rm(whichV1,which1)

######## cluster 2
which2 <- which(cluster.list==2)
whichV2 <- sample(which2, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V2_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
min.ref.check <- NULL
which.check <- NULL
for(i in 1:600) {
  if(whichV2[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV2[i])/1440)
  }
  else {
    day.ref <- floor((whichV2[i])/1440)+1
  }
  min.ref <- ((whichV2[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV2,which2)

######## cluster 3
which3 <- which(cluster.list==3)
whichV3 <- sample(which3, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V3.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV3[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV3[i])/1440)
  }
  else {
    day.ref <- floor((whichV3[i])/1440)+1
  }
  min.ref <- ((whichV3[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV3,which3)

######### cluster 4
which4 <- which(cluster.list==4)
whichV4 <- sample(which4, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V4_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV4[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV4[i])/1440)
  }
  else {
    day.ref <- floor((whichV4[i])/1440)+1
  }
  min.ref <- ((whichV4[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV4,which4)

######### cluster 5
which5 <- which(cluster.list==5)
whichV5 <- sample(which5, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V5_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV5[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV5[i])/1440)
  }
  else {
    day.ref <- floor((whichV5[i])/1440)+1
  }
  min.ref <- ((whichV5[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV5,which5)

######### cluster 6
which6 <- which(cluster.list==6)
whichV6 <- sample(which6, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V6_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV6[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV6[i])/1440)
  }
  else {
    day.ref <- floor((whichV6[i])/1440)+1
  }
  min.ref <- ((whichV6[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV6,which6)

######### cluster 7
which7 <- which(cluster.list==7)
whichV7 <- sample(which7, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V7_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV7[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV7[i])/1440)
  }
  else {
    day.ref <- floor((whichV7[i])/1440)+1
  }
  min.ref <- ((whichV7[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV7,which7)

######### cluster 8
which8 <- which(cluster.list==8)
whichV8 <- sample(which8, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V8_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV8[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV8[i])/1440)
  }
  else {
    day.ref <- floor((whichV8[i])/1440)+1
  }
  min.ref <- ((whichV8[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV8,which8)

######### cluster 9
which9 <- which(cluster.list==9)
whichV9 <- sample(which9, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V9_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV9[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV9[i])/1440)
  }
  else {
    day.ref <- floor((whichV9[i])/1440)+1
  }
  min.ref <- ((whichV9[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV9,which9)

######### cluster 10
which10 <- which(cluster.list==10)
whichV10 <- sample(which10, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V10_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV10[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV10[i])/1440)
  }
  else {
    day.ref <- floor((whichV10[i])/1440)+1
  }
  min.ref <- ((whichV10[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV10,which10)

######### cluster 11
which11 <- which(cluster.list==11)
whichV11 <- sample(which11, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V11_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV11[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV11[i])/1440)
  }
  else {
    day.ref <- floor((whichV11[i])/1440)+1
  }
  min.ref <- ((whichV11[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV11,which11)

######### cluster 12
which12 <- which(cluster.list==12)
whichV12 <- sample(which12, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V12_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV12[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV12[i])/1440)
  }
  else {
    day.ref <- floor((whichV12[i])/1440)+1
  }
  min.ref <- ((whichV12[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV12,which12)

######### cluster 13
which13 <- which(cluster.list==13)
whichV13 <- sample(which13, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V13_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV13[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV13[i])/1440)
  }
  else {
    day.ref <- floor((whichV13[i])/1440)+1
  }
  min.ref <- ((whichV13[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV13,which13)

######### cluster 14
which14 <- which(cluster.list==14)
whichV14 <- sample(which14, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V14_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV14[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV14[i])/1440)
  }
  else {
    day.ref <- floor((whichV14[i])/1440)+1
  }
  min.ref <- ((whichV14[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV14,which14)

######### cluster 15
which15 <- which(cluster.list==15)
whichV15 <- sample(which15, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V15_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV15[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV15[i])/1440)
  }
  else {
    day.ref <- floor((whichV15[i])/1440)+1
  }
  min.ref <- ((whichV15[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV15,which15)

######### cluster 16
which16 <- which(cluster.list==16)
whichV16 <- sample(which16, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V16_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV16[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV16[i])/1440)
  }
  else {
    day.ref <- floor((whichV16[i])/1440)+1
  }
  min.ref <- ((whichV16[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV16,which16)

######### cluster 17
which17 <- which(cluster.list==17)
whichV17 <- sample(which17, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V17_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV17[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV17[i])/1440)
  }
  else {
    day.ref <- floor((whichV17[i])/1440)+1
  }
  min.ref <- ((whichV17[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV17,which17)

######### cluster 18
which18 <- which(cluster.list==18)
whichV18 <- sample(which18, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V18_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV18[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV18[i])/1440)
  }
  else {
    day.ref <- floor((whichV18[i])/1440)+1
  }
  min.ref <- ((whichV18[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV18,which18)


######### cluster 19
which19 <- which(cluster.list==19)
whichV19 <- sample(which19, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V19_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV19[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV19[i])/1440)
  }
  else {
    day.ref <- floor((whichV19[i])/1440)+1
  }
  min.ref <- ((whichV19[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV19,which19)

######### cluster 20
which20 <- which(cluster.list==20)
whichV20 <- sample(which20, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V20_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV20[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV20[i])/1440)
  }
  else {
    day.ref <- floor((whichV20[i])/1440)+1
  }
  min.ref <- ((whichV20[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV20,which20)

######### cluster 21
which21 <- which(cluster.list==21)
whichV21 <- sample(which21, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V21_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV21[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV21[i])/1440)
  }
  else {
    day.ref <- floor((whichV21[i])/1440)+1
  }
  min.ref <- ((whichV21[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV21,which21)


######### cluster 22
which22 <- which(cluster.list==22)
whichV22 <- sample(which22, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V22_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV22[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV22[i])/1440)
  }
  else {
    day.ref <- floor((whichV22[i])/1440)+1
  }
  min.ref <- ((whichV22[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV22,which22)

######### cluster 23
which23 <- which(cluster.list==23)
whichV23 <- sample(which23, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V23_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV23[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV23[i])/1440)
  }
  else {
    day.ref <- floor((whichV23[i])/1440)+1
  }
  min.ref <- ((whichV23[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV23,which23)

######### cluster 24
which24 <- which(cluster.list==24)
whichV24 <- sample(which24, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V24_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV24[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV24[i])/1440)
  }
  else {
    day.ref <- floor((whichV24[i])/1440)+1
  }
  min.ref <- ((whichV24[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV24,which24)

######### cluster 25
which25 <- which(cluster.list==25)
whichV25 <- sample(which25, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V25_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV25[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV25[i])/1440)
  }
  else {
    day.ref <- floor((whichV25[i])/1440)+1
  }
  min.ref <- ((whichV25[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV25,which25)


######### cluster 26
which26 <- which(cluster.list==26)
whichV26 <- sample(which26, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V26_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV26[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV26[i])/1440)
  }
  else {
    day.ref <- floor((whichV26[i])/1440)+1
  }
  min.ref <- ((whichV26[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV26,which26)

######### cluster 27
which27 <- which(cluster.list==27)
whichV27 <- sample(which27, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V27_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV27[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV27[i])/1440)
  }
  else {
    day.ref <- floor((whichV27[i])/1440)+1
  }
  min.ref <- ((whichV27[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV27,which27)


######### cluster 28
which28 <- which(cluster.list==28)
whichV28 <- sample(which28, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V28_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV28[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV28[i])/1440)
  }
  else {
    day.ref <- floor((whichV28[i])/1440)+1
  }
  min.ref <- ((whichV28[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV28,which28)

######### cluster 29
which29 <- which(cluster.list==29)
whichV29 <- sample(which29, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V29_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV29[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV29[i])/1440)
  }
  else {
    day.ref <- floor((whichV29[i])/1440)+1
  }
  min.ref <- ((whichV29[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV29,which29)


######### cluster 30
which30 <- which(cluster.list==30)
whichV30 <- sample(which30, 600)

s <- brick(image, package="raster", ncol=620, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V30_k30.png", 
    width = 620, height = 668, 
    units = "px", antialias = "none")

length2 <- 10
for(i in 1:600) {
  if(whichV30[i] %in% c(seq(1440,length(spect_file_list)*1440,1440))){
    day.ref <- floor((whichV30[i])/1440)
  }
  else {
    day.ref <- floor((whichV30[i])/1440)+1
  }
  min.ref <- ((whichV30[i]/1440) - (day.ref-1))*1440
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, 
                                     row=1, 
                                     nrows=668, 
                                     col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()
rm(whichV30,which30)