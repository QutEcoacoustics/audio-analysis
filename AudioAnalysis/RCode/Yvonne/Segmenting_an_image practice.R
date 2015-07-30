# Plot a 24 hour false colour cluster diagram
# 23 July 2015 
# 
# Get cluster information
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015.csv", header=T)
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#vec <- read.csv("normIndicesClusters ,Woondum3 ,22-28 June 2015.csv", header=T)

clusters <- vec$unname.kmeansObj.cluster.

clusterlist <- list()

#######################################################
# make rasterRBG from a 24 hour spectrogram
library(raster)
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150622_000000.wav\\Towsey.Acoustic\\")
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\Woondum3\\20150622_000000.wav\\Towsey.Acoustic\\")
b1 <- "20150622_000000__2Maps_full.png"
b <- brick(b1, package="raster")
sourceImage <- brick(b1, package="raster")
e <- extent(0, 1469, 0, 632)
b2 <- brick(b1, package="raster")
# Make an empty brickRaster with extra columns for white lines
# and three layers
s <- brick(s, nrows=632, ncols=1469, nl=3, package="raster")

png(filename = "Image 22 June 2015.png",
    width = 1440, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1
clusterOrder <- c("24","25","10","26","30","5","9","29",
                  "27","11","12","28","14","16","3","20",
                  "8","2","4","7","18","6","23","19","22",
                  "21","17","13","1","15")

for (i in 1:30) {
  cur.minute.list <- which(clusters == clusterOrder[i])
  length <- sum(cur.minute.list <= 1440)
  if (length > 0) {
    for (j in 1:length) { 
      replacementBlock <- getValuesBlock(sourceImage, row=1, nrows=632, 
                    col=cur.minute.list[j], ncols=1)
      s[1:632, length2] <- replacementBlock
      #s[c(1:30,316:345), length2] <- colours[i,]   # 13 pixels high block of colour
      length2 <- length2 + 1
    }    
  }
  length2 <- length2 + 1
}

plotRGB(s)
dev.off()