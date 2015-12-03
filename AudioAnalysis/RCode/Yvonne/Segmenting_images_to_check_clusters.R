# 30 November 2015
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
#colours <- c("red", "chocolate4", "palegreen", "darkblue",
#             "brown1", "darkgoldenrod3", "cadetblue4", 
#             "darkorchid", "orange" ,"darkseagreen", 
#             "deeppink3", "darkslategrey", "firebrick2", 
#             "gold2", "hotpink2", "blue", "maroon", 
#             "mediumorchid4", "mediumslateblue","mistyrose4",
#             "royalblue", "orange", "palevioletred2", 
#             "sienna", "slateblue", "yellow", "tan2", 
#             "salmon","violetred1","plum","magenta","mauve")

#b1 <- "20150622_000000__2Maps_full.png"
image <- "Rasterimage.png"
#image1 <- brick(image, package="raster", nl=3,
#                crs=NA, ncol=1440)
s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

#png("image.png",width = 1440, height = 668, units = "px")
plotRGB(image1) # white image
dev.off()

# Get list of names (full path) of all spectrogram files
path <- "E:\\False colour spectrograms" # path to spectrogram files
spect_file_Gympie <- list.files(full.names=TRUE, path = paste(path,"\\GympieNP",sep="")) 
spect_file_Woondum <- list.files(full.names=TRUE, path = paste(path,"\\WoondumNP",sep="")) 
spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)
length(spect_file_list)
# Select a random sample from a cluster
# cluster 1
which1 <- which(cluster.list==1)
whichV1 <- sample(which1, 600)

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V1.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

# cluster 2
which2 <- which(cluster.list==2)
whichV2 <- sample(which2, 600)

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V2.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

# cluster 3
which3 <- which(cluster.list==3)
whichV3 <- sample(which3, 600)

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V3.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

# cluster 4
which4 <- which(cluster.list==4)
whichV4 <- sample(which4, 600)

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V4_k30.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V5_k30.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

######### cluster 7
which7 <- which(cluster.list==7)
whichV7 <- sample(which7, 600)

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V7_k30.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V8_k30.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V9_k30.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V10_k30.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V11_k30.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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

s <- brick(image, package="raster", ncol=700, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

png(filename = "ClusterImage_V12_k30.png", 
    width = 700, height = 668, 
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
  #b2 <- brick(b1, package="raster",nrows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
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