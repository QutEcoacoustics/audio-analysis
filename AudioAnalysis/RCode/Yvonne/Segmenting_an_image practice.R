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

#for (i in 1:30) {
#  assign(paste("cluster",i,sep=""), grep(paste("\\<", i,"\\>", sep=""), clusters))
# 
#}

# make rasterRBG from a 24 hour spectrogram
library(raster)
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150622_000000.wav\\Towsey.Acoustic\\")
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\Woondum3\\20150622_000000.wav\\Towsey.Acoustic\\")
b1 <- "20150622_000000__2Maps_full.png"
b <- brick(b1, package="raster")
d <- brick(b1, package="raster")
b2 <- brick(b1, package="raster")

#b2[] <- 1:ncell(b)

length2 <- 1

#for (i in 1:30) {
#  length <- sum(cluster30 <= 1440)
# for (j in seq_along(cluster29[1:length])) {
#    b2[(length2*632+1):((length2+1)*632)] <- c[(cluster29[j]*632):((cluster29[j]+1)*632-1)]
#    length2 <- length2 + 1
#  }  
#}

png(filename = "Image 22 June 2015.png",
    width = 1440, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

length2 <- 1

for (i in 1:30) {
  cur.minute.list <- which(clusters == i)
  length <- sum(cur.minute.list <= 1440)
  if (length > 0) {
    for (j in 1:length) { 
      t <- getValuesBlock(sourceImage, row=1, nrows=632, 
                    col=cur.minute.list[j], ncols=1)
      # write some code below that extracts out exactly the same as t above
      # r <- sourceImage[1:632, 18]
      b2[1:632, length2] <- t
      length2 <- length2 + 1
    }    
  }
}

plotRGB(b2)
dev.off()

t <- getValuesBlock(sourceImage, row=1, nrows=632, col=18, ncols=1)
t <- t[,1:3]
# write some code below that extracts out exactly the same as t above
r <- sourceImage[1:632, 18]

dev.off()

#plotRGB(b2)

