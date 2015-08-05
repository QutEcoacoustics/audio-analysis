# Plot a 24 hour false colour cluster diagram
# 23 July 2015 
# 
# Get cluster information
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
vec <- read.csv("normIndicesClusters 5,9,11,13,14,15,17,Gympie NP1 ,22-28 June 2015 test .csv", header=T)
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#vec <- read.csv("normIndicesClusters ,Woondum3 ,22-28 June 2015.csv", header=T)
#vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015 5,9,11,13,14,15,17.csv", header = T)
#vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015 5,8,10,13,14,15,17.csv", header = T)

# clusters <- unname(kmeansObj$cluster)
clusters <- vec$unname.kmeansObj.cluster.

clusterlist <- list()

#######################################################
# the clusterOrder comes from Distance Matrix code
clusterOrder <- c("29","10","21","27","2","3","19","28","11",
                  "5","17","26","12","16","15","13","6","30",
                  "25","14","20","7","4","8","23","1","18","22",
                  "24","9") # for 5,7,9,10,11,12,13,14,15,17 with seed 
clusterOrder <- c("24","25","10","26","30","5","9","29",
                  "27","11","12","28","14","16","3","20",
                  "8","2","4","7","18","6","23","19","22",
                  "21","17","13","1","15") # for 5,7,9,10,11,12,13,14,15,17
                  # with seed 1234
clusterOrder <- c("27","8","25","20","16","2","6","30","9","17","1","12",
                  "24","4","26","15","18","14","10","21","5","22","19","23",
                  "29","13","3","28","11","7") # for 5,9, 11,13,14,15,17
clusterOrder <- c("13","22","25","15","3","29","2","1","27","8","12","5",
                  "16","14","21","7","20","11","18","19","10","24","28","17",
                  "9","6","26","30","23","4") # for 5,8,10,13,14,15,17
################################################
# Read in the colourBlock image
library(raster)
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
colourName <- "colourBlock.png"
colourBlock <- brick(colourName, package="raster")
plotRGB(colourBlock)
##### Code for 1st day #########################
# make rasterRBG from a 24 hour spectrogram
# 22 June 2015 Gympie NP
setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150622_000000.wav\\Towsey.Acoustic\\")
library(raster)
b1 <- "20150622_000000__2Maps_full.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
e <- extent(0, 1469, 0, 632)
b2 <- brick(b1, package="raster")
s <- b2
# Make an empty brickRaster with extra columns for white lines
# and three layers
s <- brick(s, nrows=632, ncols=1498, nl=3, package="raster")
plotRGB(sourceImage)
# 22 June 2015 Woondum3
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\Woondum3\\20150622_000000.wav\\Towsey.Acoustic\\")

### Change date here!
png(filename = "Image 22 June 2015b Gympie NP 5,7,9,10,11,12,13,14,15,17.png",
    width = 1460, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  cur.minute.list <- which(clusters == clusterOrder[i])
  length <- sum(cur.minute.list <= 1440)
  if (length > 0) {
    for (j in 1:length) { 
      replacementBlock <- getValuesBlock(sourceImage, row=1, 
                          nrows=632, col=cur.minute.list[j], 
                          ncols=1)
      s[1:632, length2] <- replacementBlock
      s[1:38, length2] <- getValuesBlock(colourBlock, row=1,
                          nrows=38, col=i, ncols = 1)
      length2 <- length2 + 1
    }    
  }
  print(paste("finishing", i, sep = ""))
  length2 <- length2 + 2
}

plotRGB(s)
dev.off()

##### Code for 2nd day  #########################
library(raster)
setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150623_000002.wav\\Towsey.Acoustic\\")
b1 <- "20150623_000002__2Maps_full.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
e <- extent(0, 1469, 0, 632)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=632, ncols=1498, nl=3, package="raster")

#Change date here!
png(filename = "Image 23 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 1460, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  cur.minute.list <- which(clusters == clusterOrder[i])
  cur.minute.list.ref <- which((cur.minute.list > 1440) & 
                                 (cur.minute.list <2881))
  length <- length(cur.minute.list.ref)
    if (length > 0) {
      for (j in seq_along(cur.minute.list.ref)) {
        replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=632, 
                            col=(cur.minute.list[cur.minute.list.ref[j]]-1440), 
                            ncols=1)
        s[1:632, length2] <- replacementBlock
        s[1:38, length2] <- getValuesBlock(colourBlock, row=1,
                                           nrows=38, col=i, ncols = 1)
        length2 <- length2 + 1
      }    
  }
  print(paste("finishing", i, sep = ""))
  length2 <- length2 + 2
}

plotRGB(s)
dev.off()

##### Code for 3rd day #########################
# 24 June 2015 Gympie NP
setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150624_000005.wav\\Towsey.Acoustic\\")
library(raster)
b1 <- "20150624_000000__2Maps_full.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
e <- extent(0, 1469, 0, 632)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=632, ncols=1498, nl=3, package="raster")
plotRGB(sourceImage)
#Change date here!
png(filename = "Image 24 June 2015b Gympie NP 5,7,9,10,11,12,13,14,15,17.png",
    width = 1460, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  cur.minute.list <- which(clusters == clusterOrder[i])
  cur.minute.list.ref <- which((cur.minute.list > 2881) & 
                                 (cur.minute.list < 4320))
  length <- length(cur.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(cur.minute.list.ref)) {
     replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=632, 
                        col=(cur.minute.list[cur.minute.list.ref[j]]-2880), 
                        ncols=1)
      s[1:632, length2] <- replacementBlock
      s[1:38, length2] <- getValuesBlock(colourBlock, row=1,
                                 nrows=38, col=i, ncols = 1)
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
e <- extent(0, 1469, 0, 632)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=632, ncols=1498, nl=3, package="raster")
plotRGB(sourceImage)

png(filename = "Image 25 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 1460, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  cur.minute.list <- which(clusters == clusterOrder[i])
  cur.minute.list.ref <- which((cur.minute.list > 4321) & 
                                 (cur.minute.list < 5760))
  length <- length(cur.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(cur.minute.list.ref)) {
      replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=632, 
                      col=(cur.minute.list[cur.minute.list.ref[j]]-4320), 
                      ncols=1)
      s[1:632, length2] <- replacementBlock
      s[1:38, length2] <- getValuesBlock(colourBlock, row=1,
                                         nrows=38, col=i, ncols = 1)
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
e <- extent(0, 1469, 0, 632)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=632, ncols=1498, nl=3, package="raster")
plotRGB(sourceImage)

png(filename = "Image 26 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 1460, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  cur.minute.list <- which(clusters == clusterOrder[i])
  cur.minute.list.ref <- which((cur.minute.list > 5761) & 
                                 (cur.minute.list < 7200))
  length <- length(cur.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(cur.minute.list.ref)) {
      replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=632, 
                          col=(cur.minute.list[cur.minute.list.ref[j]]-5760), 
                          ncols=1)
      s[1:632, length2] <- replacementBlock
      s[1:38, length2] <- getValuesBlock(colourBlock, row=1,
                                         nrows=38, col=i, ncols = 1)
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
e <- extent(0, 1469, 0, 632)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=632, ncols=1498, nl=3, package="raster")
plotRGB(sourceImage)

png(filename = "Image 26 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 1460, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  cur.minute.list <- which(clusters == clusterOrder[i])
  cur.minute.list.ref <- which((cur.minute.list > 7201) & 
                                 (cur.minute.list < 8640))
  length <- length(cur.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(cur.minute.list.ref)) {
      replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=632, 
                         col=(cur.minute.list[cur.minute.list.ref[j]]-7200), 
                         ncols=1)
      s[1:632, length2] <- replacementBlock
      s[1:38, length2] <- getValuesBlock(colourBlock, row=1,
                                         nrows=38, col=i, ncols = 1)
      length2 <- length2 + 1
    }
  }
  print(paste("finishing", i, sep = ""))
  length2 <- length2 + 2 # put two spaces between clusters
}

plotRGB(s)
dev.off()
##################################################################
