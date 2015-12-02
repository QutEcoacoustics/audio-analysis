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
image1 <- brick(image, package="raster",nl=3,
                crs=NA,ncol=)
image1[[1]] <- 255 
image1[[2]] <- 255
image1[[3]] <- 255

plotRGB(image1) # white image
dev.off()

path <- "E:\\False colour spectrograms" # path to spectrogram files

spect_file_Gympie <- list.files(full.names=TRUE, path = paste(path,"\\GympieNP",sep="")) 
spect_file_Woondum <- list.files(full.names=TRUE, path = paste(path,"\\WoondumNP",sep="")) 
spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)


# Select a random sample from a cluster
# cluster 1
which1 <- which(cluster.list==1)
whichV1 <- sample(which1, 600)
whichV1 <- 164160:(164160+719)
  
png(filename = "ClusterImage_V1_k30.png", 
    width = 2000, height = 668, 
    units = "px")
s <- image1
length2 <- 10

for(i in 1:720) {
  paste(i)
  day.ref <- floor(whichV1[i]/1440 + 1)
  min.ref <- floor(((whichV1[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster",
                       nrows=668, ncols=1440)
  #b2 <- brick(b1, package="raster",norows=668,ncols=2000)
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

# cluster 2
which2 <- which(cluster.list==2)
whichV2 <- sample(which2, 600)

png(filename = "ClusterImage_V2_k30.png", width = 2000, height = 668, 
    units = "px", antialias=NULL)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV2[i]/1440 + 1)
  min.ref <- floor(((whichV2[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 3
which3 <- which(cluster.list==3)
whichV3 <- sample(which3, 600)

png(filename = "ClusterImage_V3_k30.png", width = 2000, height = 668, 
    units = "px", antialias=NULL)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV3[i]/1440 + 1)
  min.ref <- floor(((whichV3[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 4
which4 <- which(cluster.list==4)
whichV4 <- sample(which4, 600)

png(filename = "ClusterImage_V4_k30.png", width = 2000, height = 668, 
    units = "px", antialias=NULL)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV4[i]/1440 + 1)
  min.ref <- floor(((whichV4[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 5
which5 <- which(cluster.list==5)
whichV5 <- sample(which5, 600)

png(filename = "ClusterImage_V5_k30.png", width = 2000, height = 668, 
    units = "px", antialias=NULL)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV5[i]/1440 + 1)
  min.ref <- floor(((whichV5[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 6
which6 <- which(cluster.list==6)
whichV6 <- sample(which6, 600)

png(filename = "ClusterImage_V6_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV6[i]/1440 + 1)
  min.ref <- floor(((whichV6[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()


# cluster 7
which7 <- which(cluster.list==7)
whichV7 <- sample(which7, 600)

png(filename = "ClusterImage_V7_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV7[i]/1440 + 1)
  min.ref <- floor(((whichV7[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()


# cluster 8
which8 <- which(cluster.list==8)
whichV8 <- sample(which8, 600)

png(filename = "ClusterImage_V8_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV8[i]/1440 + 1)
  min.ref <- floor(((whichV8[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                      nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 9
which9 <- which(cluster.list==9)
whichV9 <- sample(which9, 600)

png(filename = "ClusterImage_V9_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV9[i]/1440 + 1)
  min.ref <- floor(((whichV9[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

#cluster 10
which10 <- which(cluster.list==10)
whichV10 <- sample(which10, 600)

png(filename = "ClusterImage_V10_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV10[i]/1440 + 1)
  min.ref <- floor(((whichV10[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

#cluster 11
which11 <- which(cluster.list==11)
whichV11 <- sample(which11, 600)

png(filename = "ClusterImage_V11_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV11[i]/1440 + 1)
  min.ref <- floor(((whichV11[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

#cluster 12
which12 <- which(cluster.list==12)
whichV12 <- sample(which12, 600)

png(filename = "ClusterImage_V12_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV12[i]/1440 + 1)
  min.ref <- floor(((whichV12[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

#cluster 13
which13 <- which(cluster.list==13)
whichV13 <- sample(which13, 600)

png(filename = "ClusterImage_V13_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV13[i]/1440 + 1)
  min.ref <- floor(((whichV13[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

#cluster 14
which14 <- which(cluster.list==14)
whichV14 <- sample(which14, 600)

png(filename = "ClusterImage_V14_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV14[i]/1440 + 1)
  min.ref <- floor(((whichV14[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

#cluster 15
which15 <- which(cluster.list==15)
whichV15 <- sample(which15, 600)

png(filename = "ClusterImage_V15_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV15[i]/1440 + 1)
  min.ref <- floor(((whichV15[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 16
which16 <- which(cluster.list==16)
whichV16 <- sample(which16, 600)

png(filename = "ClusterImage_V16_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV16[i]/1440 + 1)
  min.ref <- floor(((whichV16[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 17
which17 <- which(cluster.list==17)
whichV17 <- sample(which17, 600)

png(filename = "ClusterImage_V17_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV17[i]/1440 + 1)
  min.ref <- floor(((whichV17[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 18
which18 <- which(cluster.list==18)
whichV18 <- sample(which18, 600)

png(filename = "ClusterImage_V18_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV18[i]/1440 + 1)
  min.ref <- floor(((whichV18[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 19
which19 <- which(cluster.list==19)
whichV19 <- sample(which19, 600)

png(filename = "ClusterImage_V19_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  paste(i)
  day.ref <- floor(whichV19[i]/1440 + 1)
  min.ref <- floor(((whichV19[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                                     nrows=668, col=current.minute.list, 
                                     ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

# cluster 20
which20 <- which(cluster.list==20)
whichV20 <- sample(which20, 600)

png(filename = "ClusterImage_V20_k30.png", width = 2000, height = 668, 
    units = "px", pointsize = 12, res = NA, 
    family = "", restoreConsole = TRUE)
s <- image1
length2 <- 10

for(i in 1:600) {
  day.ref <- floor(whichV20[i]/1440 + 1)
  min.ref <- floor(((whichV20[i]/1440) - (day.ref-1))*1440)
  b1 <- spect_file_list[day.ref]
  b <- brick(b1, package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster",norows=668,ncols=2000)
  #plotRGB(sourceImage)
  #dev.off()
  current.minute.list <- min.ref
  replacementBlock <- getValuesBlock(sourceImage, row=1, 
                         nrows=668, col=current.minute.list, 
                         ncols=1)
  s[1:668, length2] <- replacementBlock
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()

