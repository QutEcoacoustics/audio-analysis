# Plot a 24 hour false colour cluster diagram
# 23 July 2015 
# 
# Get cluster information
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21_35clusters")
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21_40clusters")
site <- "Gympie NP1 "
cluster.list <- read.csv(file = paste("Cluster_list 22-28 June 2015_5,7,9,11,12,13,17,20", 
                         site, ".csv", sep = ""), header = T,
                         col.names = "cluster.list")
##### Find cluster order #########################
distances <- read.csv(
  "Distance_matrix_5,7,9,11,12,13,17,20Gympie NP1 .csv", header =T)

# One dimensional analysis
#distances <- read.csv("Distance_matrix_GympieNP 22 June 2015.csv", header =                        T)

dist <- cmdscale(distances[,1:40], k=1)
y <- dist[, 1]
z <- sort(y)

clusterOrder <- names(z)
clusterOrder
################################################
# Read in the colourBlock image
library(raster)
colourName <- "colourBlock.png"
colourBlock <- brick(colourName, package="raster")
plotRGB(colourBlock)

cols <- c(
  '0' = "#F2F2F2FF", '1' = "#00B917", '2' = "#788231",   '3' = "#FF0000",
  '4' = "#01FFFE",   '5' = "#FE8900", '6' = "#006401",   '7' = "#FFDB66",
  '8' = "#010067",   '9' = "#95003A", '10' = "#007DB5", '11' = "#BE9970",
  '12' = "#774D00", '13' = "#90FB92", '14' = "#0076FF", '15' = "#FF937E",
  '16' = "#6A826C", '17' = "#FF029D", '18' = "#0000FF", '19' = "#7A4782",
  '20' = "#7E2DD2", '21' = "#0E4CA1", '22' = "#FFA6FE", '23' = "#A42400",
  '24' = "#00AE7E", '25' = "#BB8800", '26' = "#BE9970", '27' = "#263400",
  '28' = "#C28C9F", '29' = "#FF74A3", '30' = "#01D0FF", "31" = "#6B6882",
  '32' = "#E56FFE", '33' = "#85A900", '34' = "#968AE8", '35' = "#43002C",
  '36' = "#DEFF74", '37' = "#00FFC6", '38' = "#FFE502", '39' = "#620E00",
  '40' = "#008F9C", '41' = "#98FF52", '42' = "#7544B1", '43' = "#B500FF",
  '44' = "#00FF78", '45' = "#FF6E41", '46' = "#005F39", '47' = "#004754",
  '48' = "#5FAD4E", '49' = "#A75740", '50' = "#A5FFD2", '51' = "#FFB167",
  '52' = "#009BFF", '53' = "#91D0CB")
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
png(filename = "whiteRaster.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)
plotRGB(image1)
dev.off()

b1 <- "GympieNP_20150622__ACI-ENT-EVN.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
#e <- extent(0, 1469, 0, 334)
#e <- extent(0, 1650, 0, 334)
b2 <- brick(b1, package="raster",norows=334, ncols=1650)
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
png(filename = "Image 22 June 2015e Gympie NP_5,7,9,11,12,13,17,20.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

total <- NULL
for (i in 1:40) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  length <- sum(current.minute.list <= 1440)
total <- c(total, length)
}
total

length2 <- 10

for (i in 1:40) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  length <- sum(current.minute.list <= 1440)
  if (length > 0) {
    for (j in 1:length) {
      replacementBlock <- getValuesBlock(sourceImage, row=1, 
                          nrows=334, col=current.minute.list[j], 
                          ncols=1)
      s[1:334, length2] <- replacementBlock
      block <- getValuesBlock(colourBlock, row=1,
                          nrows=40, col=i, ncols = 1)
      # Alternative method using a list of hexadecimal colour strings
      rgb <- col2rgb(unname(cols[i+1]))
      block <- matrix(data="NA", ncol = 3, nrow = 40)
      block1 <- cbind(rgb[1], rgb[2], rgb[3])
      for(k in 1:40) {
        block[k,1:3] <- block1
      }
      for(l in 1:3) {
      block[,l] <- as.integer(block[,l])
      }
      ###### end alternative
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
library(raster)
b1 <- "20150623_000002__2Maps_full.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")

b1 <- "whiteRaster.png"
s <- brick(b1, package="raster")

#Change date here!
png(filename = "Image 23 June 2015 GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:5) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  current.minute.list.ref <- which((current.minute.list > 1440) & 
                              (current.minute.list <2881))
  length <- length(current.minute.list.ref)
    if (length > 0) {
      for (j in seq_along(current.minute.list.ref)) {
        replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=334, 
                            col=(current.minute.list[current.minute.list.ref[j]]-1440), 
                            ncols=1)
        s[1:334, length2] <- replacementBlock
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
e <- extent(0, 2000, 0, 334)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=334, ncols=2000, nl=3, package="raster")
plotRGB(sourceImage)
#Change date here!
png(filename = "Image 24 June 2015b Gympie NP 5,7,9,10,11,12,13,14,15,17.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
#clusterOrder <- order
for (i in 1:30) {
  current.minute.list <- which(cluster.list == clusterOrder[i])
  current.minute.list.ref <- which((current.minute.list > 2881) & 
                                 (current.minute.list < 4320))
  length <- length(current.minute.list.ref)
  if (length > 0) {
    for (j in seq_along(current.minute.list.ref)) {
     replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=334, 
                        col=(current.minute.list[current.minute.list.ref[j]]-2880), 
                        ncols=1)
      s[1:334, length2] <- replacementBlock
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
e <- extent(0, 2000, 0, 334)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=334, ncols=2000, nl=3, package="raster")
plotRGB(sourceImage)

png(filename = "Image 25 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
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
e <- extent(0, 2000, 0, 334)
b2 <- brick(b1, package="raster")
s <- b2
s <- brick(s, nrows=334, ncols=2000, nl=3, package="raster")
plotRGB(sourceImage)

png(filename = "Image 26 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
    width = 2000, height = 334, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

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
      s[1:38, length2] <- getValuesBlock(colourBlock, 
                            row=1,nrows=38, col=i, 
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

png(filename = "Image 26 June 2015b GympieNP 5,7,9,10,11,12,13,14,15,17.png",
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
