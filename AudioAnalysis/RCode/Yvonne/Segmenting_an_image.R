# Plot a 24 hour false colour cluster diagram
# 23 July 2015 
# Always check the nrows in the images as this can be 317 or 334

# Get cluster information
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\kmeans_30clusters")
#cluster.list <- read.csv(file = paste("Cluster_list_kmeans_22June-16July2015_5,7,9,10,11,12,13,17,18_30", 
#                        site, ".csv", sep = ""), header = T,
#                        col.names = "cluster.list")

#setwd("C:\\Work\\CSV files\\GympieNP1_new\\mclust_30clusters\\Week2_29June_5July2015")
#cluster.list <- read.csv(file="mclust30list_9_week2.csv", header = T)

setwd("C:\\Work\\CSV files\\GympieNP1_new\\mclust_30clusters\\Week1_22_28June2015")
cluster.list <- read.csv(file="mclust30list_9_week1.csv", header = T)

indices <- read.csv("C:\\Work\\CSV files\\GympieNP1_new\\all_data\\Towsey_Summary_Indices_Gympie NP1 22-06-2015to current.csv")
list <- which(indices$minute.of.day=="0")
lst1 <- NULL
for (i in 1:length(list)) {
  lst <- list[i+1]-1
  lst1 <- c(lst1, lst)
}
list <- cbind(list, lst1)
colnames(list) <- c("start","end")

site <- "Gympie NP1 "
##### Find cluster order #########################
#distances <- read.csv(
#  "Distance_matrix_5,7,9,11,12,13,17,20Gympie NP1 .csv", header =T)

# One dimensional analysis
#distances <- read.csv("Distance_matrix_GympieNP 22 June 2015.csv", header =                        T)

#dist <- cmdscale(distances[,1:35], k=1)
#y <- dist[, 1]
#z <- sort(y)

#clusterOrder <- names(z)
#clusterOrder
################################################
# Read in the colourBlock image
library(raster)
colourName <- "colourBlock.png"
colourBlock <- brick(colourName, package="raster")
#plotRGB(colourBlock)
colours <- c("red", "chocolate4", "palegreen", "darkblue",
             "brown1", "darkgoldenrod3", "cadetblue4", 
             "darkorchid", "orange" ,"darkseagreen", 
             "deeppink3", "darkslategrey", "firebrick2", 
             "gold2", "hotpink2", "blue", "maroon", 
             "mediumorchid4", "mediumslateblue","mistyrose4",
             "royalblue", "orange", "palevioletred2", 
             "sienna", "slateblue", "yellow", "tan2", 
             "salmon","violetred1","plum")

##### Code for 1st day #########################
# make rasterRBG from a 24 hour spectrogram
# 22 June 2015 Gympie NP
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150622_000000.wav\\Towsey.Acoustic\\")
library(raster)
#b1 <- "20150622_000000__2Maps_full.png"
image <- "Rasterimage.png"
image1 <- brick(image, package="raster")
image1[[1]] <- 255 
image1[[2]] <- 255
image1[[3]] <- 255
#png(filename = "whiteRaster.png",
#    width = 2000, height = 334, units = "px", pointsize = 12,
#    res = NA, family = "", restoreConsole = TRUE)
plotRGB(image1)
#dev.off()

b1 <- "GympieNP_20150622__ACI-ENT-EVN.png"
#b1 <- "GympieNP_20150624__ACI-ENT-EVN.png"
#b1 <- "GympieNP_20150628__ACI-ENT-EVN.png"
#b1 <- "GympieNP_20150629__ACI-ENT-EVN.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
#e <- extent(0, 1469, 0, 334)
#e <- extent(0, 1650, 0, 334)
b2 <- brick(b1, package="raster",norows=334,ncols=1650)
#s <- b2
s <- image1
# Make an empty brickRaster with extra columns for white lines
# and three layers
#s <- brick(s, nrows=334, ncols=1498, nl=3, package="raster")
#s <- brick(s, nrows=334, ncols=1441, nl=3, package="raster")
plotRGB(sourceImage)
# 22 June 2015 Woondum3
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\Woondum3\\20150622_000000.wav\\Towsey.Acoustic\\")

### Change date here!
dev.off()
png(filename = "ClusterImage_a_mclust_22 June 2015 Gympie NP_5,7,9,10,11,12,13,17,18.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

clusterOrder <- as.character(c(1:30))
total <- NULL
for (i in 1:30) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  length <- sum(current.minute.list <= 1441)
total <- c(total, length)
}
total

length2 <- 10

for (i in 1:30) {
  current.minute.list <- which(cluster.list == i)
  length <- sum(current.minute.list <= 1441)
  if (length > 0) {
    for (j in 1:length) {
      #replacementBlock <- getValuesBlock(sourceImage, row=1, 
      #                    nrows=334, col=current.minute.list[j], 
      #                    ncols=1)
      #s[1:334, length2] <- replacementBlock
      replacementBlock <- getValuesBlock(sourceImage, row=1, 
                          nrows=334, col=current.minute.list[j], 
                          ncols=1)
      s[1:334, length2] <- replacementBlock
      block <- getValuesBlock(colourBlock, row=1,
                          nrows=40, col=i, ncols = 1)
      s[1:40, length2] <- block[,1:3]
      length2 <- length2 + 1
    }    
  }
  print(paste("finishing", i, sep = ""))
  if (length > 0) {
    length2 <- length2 + 3
  }
}

plotRGB(s)
dev.off()

##### Code for 2nd day  #########################
#library(raster)
#Change date here!
for (j in 1:7) {
  b1 <- c("GympieNP_20150622__ACI-ENT-EVN.png",
          "GympieNP_20150623__ACI-ENT-EVN.png",
          "GympieNP_20150624__ACI-ENT-EVN.png",
          "GympieNP_20150625__ACI-ENT-EVN.png",
          "GympieNP_20150626__ACI-ENT-EVN.png",
          "GympieNP_20150627__ACI-ENT-EVN.png",
          "GympieNP_20150628__ACI-ENT-EVN.png")
  
  b <- brick(paste(b1[j]), package="raster")
  sourceImage <- brick(b1, package="raster")
  b2 <- brick(b1, package="raster", norows=334, ncols=1650)#s <- b2
  s <- image1
  plotRGB(sourceImage)
  
  date <- indices$rec.date
  week <- "1"
  dev.off() 
  dev.off()
  dev.off()
  dev.off()
  file <- paste("ClusterImage_a_mclust_", "week_", week, date[j],
                "_GympieNP 5,7,9,10,11,12,13,14,15,17.png", sep = "_")
  png(filename = file, width = 2000, height = 334, units = "px", 
      pointsize = 12, res = NA, family = "", restoreConsole = TRUE)
  length2 <- 1
  clusterOrder <- as.character(c(1:30))
  
  for (i in 1:30) { 
    current.minute.list <- which(cluster.list == clusterOrder[i])
    current.minute.list.ref <- which((current.minute.list >= list$start[j]) & 
                                       (current.minute.list <= list$start[j]))
    length <- length(current.minute.list.ref)
    if (length > 0) {
      for (j in seq_along(current.minute.list.ref)) {
        replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=334, 
                                           col=(current.minute.list[current.minute.list.ref[j]]-(list$start[j]-1)), 
                                           ncols=1) # number row above needs one less than # row 157
        s[1:334, length2] <- replacementBlock
        s[1:40, length2] <- getValuesBlock(colourBlock, row=1,
                                           nrows=40, col=i, ncols = 1)
        length2 <- length2 + 1
      }    
    }
    print(paste("finishing", i, sep = ""))
    length2 <- length2 + 2
  }
}
plotRGB(s)
dev.off()

##### Code for 3rd day #########################
# 24 June 2015 Gympie NP
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150624_000005.wav\\Towsey.Acoustic\\")
#library(raster)
#b1 <- "GympieNP_20150624__ACI-ENT-EVN.png"
#b <- brick(b1, package="raster")
#sourceImage <- brick(b1, package="raster")
#e <- extent(0, 2000, 0, 334)
#b2 <- brick(b1, package="raster")
#s <- b2
#s <- brick(s, nrows=334, ncols=2000, nl=3, package="raster")
#plotRGB(sourceImage)
#Change date here!
png(filename = "ClusterImage_mclust_24 June 2015 Gympie NP 5,7,9,10,11,12,13,14,15,17.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)
clusterOrder <- as.character(1:30)
length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  current.minute.list.ref <- which((current.minute.list > 2882) & 
                                 (current.minute.list < 4321))
  length <- length(current.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(current.minute.list.ref)) {
     replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=334, 
                        col=(current.minute.list[current.minute.list.ref[j]]-2880), 
                        ncols=1)
      s[1:334, length2] <- replacementBlock
      s[1:40, length2] <- getValuesBlock(colourBlock, row=1,
                                 nrows=40, col=i, ncols = 1)
      length2 <- length2 + 1
    }
  }
  print(paste("finishing", i, sep = ""))
  length2 <- length2 + 2 # put two spaces between clusters
}

plotRGB(s)
dev.off()

##### Code for 4th day #########################
#Change date here!
# 25 June 2015 Gympie NP
setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150625_000005.wav\\Towsey.Acoustic\\")
b1 <- "20150625_000005__2Maps_full.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
e <- extent(0, 2000, 0, 334)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=334, ncols=2000, nl=3, package="raster")
plotRGB(sourceImage)

png(filename = "ClusterImage_mclust_25 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  current.minute.list.ref <- which((current.minute.list > 4321) & 
                                 (current.minute.list < 5760))
  length <- length(current.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(current.minute.list.ref)) {
      replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=334, 
                      col=(current.minute.list[current.minute.list.ref[j]]-4320), 
                      ncols=1)
      s[1:334, length2] <- replacementBlock
      s[1:40, length2] <- getValuesBlock(colourBlock, row=1,
                                         nrows=40, col=i, ncols = 1)
      length2 <- length2 + 1
    }
  }
  print(paste("finishing", i, sep = ""))
  length2 <- length2 + 2 # put two spaces between clusters
}

plotRGB(s)
dev.off()
#### Code for the 5th day ##############################################################
#Change date here!
# 26 June 2015 Gympie NP
setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150626_000004.wav\\Towsey.Acoustic\\")
b1 <- "20150626_000004__2Maps_full.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
e <- extent(0, 2000, 0, 334)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=334, ncols=2000, nl=3, package="raster")
plotRGB(sourceImage)

png(filename = "ClusterImage_mclust_26 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

clusterOrder <- as.character(1:30)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  current.minute.list.ref <- which((current.minute.list > 5761) & 
                                 (current.minute.list < 7200))
  length <- length(current.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(current.minute.list.ref)) {
      replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=334, 
                          col=(current.minute.list[current.minute.list.ref[j]]-5760), 
                          ncols=1)
      s[1:334, length2] <- replacementBlock
      s[1:40, length2] <- getValuesBlock(colourBlock, 
                            row=1,nrows=40, col=i, 
                            ncols = 1)
      length2 <- length2 + 1
    }
  }
  print(paste("finishing", i, sep = ""))
  length2 <- length2 + 2 # put two spaces between clusters
}

plotRGB(s)
dev.off()
##### Code for the 6th day #############################################################
#Change date here!
# 27 June 2015 Gympie NP
setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150627_000003.wav\\Towsey.Acoustic\\")
b1 <- "20150627_000003__2Maps_full.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
e <- extent(0, 2000, 0, 334)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=334, ncols=2000, nl=3, package="raster")
plotRGB(sourceImage)

png(filename = "ClusterImage_mclust_26 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  current.minute.list.ref <- which((current.minute.list > 7201) & 
                                 (current.minute.list < 8640))
  length <- length(current.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(current.minute.list.ref)) {
      replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=334, 
                         col=(current.minute.list[current.minute.list.ref[j]]-7200), 
                         ncols=1)
      s[1:334, length2] <- replacementBlock
      s[1:40, length2] <- getValuesBlock(colourBlock, row=1,
                                         nrows=40, col=i, ncols = 1)
      length2 <- length2 + 1
    }
  }
  print(paste("finishing", i, sep = ""))
  length2 <- length2 + 2 # put two spaces between clusters
}

plotRGB(s)
dev.off()
##################################################################