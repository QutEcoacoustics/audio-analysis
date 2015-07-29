# Plot a 24 hour false colour cluster diagram
# 23 July 2015 
# 
# Get cluster information
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015.csv", header=T)
setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
vec <- read.csv("normIndicesClusters ,Woondum3 ,22-28 June 2015.csv", header=T)

clusters <- vec$unname.kmeansObj.cluster.

for (i in 1:30) {
  assign(paste("cluster",i,sep=""), grep(paste("\\<", i,"\\>", sep=""), clusters))
}

# make rasterRBG from a 24 hour spectrogram
library(raster)
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\GympieNP\\20150622_000000.wav\\Towsey.Acoustic\\")
#setwd("F:\\Indices\\2015Jul01-120417 - Yvonne, Indices, ICD=60.0\\Yvonne\\Woondum3\\20150622_000000.wav\\Towsey.Acoustic\\")
b1 <- "20150622_000000__2Maps_full.png"
b <- brick(b1, package="raster")

setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")

# The extents must change to avoid plotting white space over the
# top of the previous plot.  

############ Plotting image #####################
png(filename = "cluster30 22 June 2015.png",
    width = 1440, height = 632, units = "px", pointsize = 12,
    res = NA, family = "", restoreConsole = TRUE)

#initiate plot with full extents
e <- extent(b)
rgbCrop <- c(0, 1, 0, 632) # set the crop extents
rgbRaster_crop <- crop(b, rgbCrop) 
re <- extend(rgbRaster_crop, e)
plotRGB(re)
par(new=T)
i<-0

length <- sum(cluster1 <= 1440)
for (j in seq_along(cluster1[1:length])) {
  # set the crop extents
  rgbCrop <- c(cluster1[j], (cluster1[j]+1), 0, 632) 
  rgbRaster_crop <- crop(b, rgbCrop, snap='near') 
  e <- extent(i, 1440, 0, 632)
  re <- extend(rgbRaster_crop, e)
  plotRGB(re)
  par(new=T)
  i <- cluster1[j+1]
  print(paste("printing ", cluster1[j], sep = ""))
}

mtext("cluster1 Woondum3 22 June 2015_30_clus_10_variables", side = 4, line = -1, 
      outer = T, cex = 2)
dev.off()
################################################
# some practice
#for (j in seq_along(cluster6)) {
#    e <- extent(i, 1440, 0, 632)
#    rgbCrop <- c(cluster6[j], (cluster6[j]+1), 0, 632) # set the crop extents
#    rgbRaster_crop <- crop(b, rgbCrop, snap='near') 
#    re <- extend(rgbRaster_crop, e)
#    plotRGB(re)
#    par(new=T)
#    i <- i - 1
#    print(paste("finished ", cluster6[j], sep = ""))
#}

#plotRGB(b)
#e <- extent(i, 1440, 0, 632)
#rgbCrop <- c(1439, 1440, 0, 632) # set the crop extents
#rgbRaster_crop <- crop(b, rgbCrop) 
#re <- extend(rgbRaster_crop, e)
#plotRGB(re)
#par(new=T)

#for (i in seq_along(cluster6)) {
#  print(paste("hello", cluster6[i], sep = " "))
#}

#par(new=T)
#rgbCrop <- c(349,357,0, 632)
#extent(rgbCrop)
#e <- extent(20, 40, 0, 632)
#re <- extend(b, e)
#plotRGB(re)
#par(new=T)
#e <- extent(-300,1440,0, 632)
#rgbCrop <- c(349,357,0, 632)
#extent(rgbCrop)
#rgbCrop <- c()
#rgbRaster_crop <- crop(b, rgbCrop)
#plotRGB(rgbRaster_crop)
#dev.off()
#e <- extent(300, 500, 0, 632)
#rgbCrop <- c(360,371,0,nrow(b))
#rgbRaster_crop <- crop(b, rgbCrop)
#re <- extend(rgbRaster_crop, e)
#plotRGB(re)
#par(new=T)
##################################
# writeRaster 
#x <- raster("20150622_000000__2Maps.png")
#s <- stack(x, x, x)
#plot(s)
#x <- raster("F:/Indices/2015Jul01-120417 - Yvonne, Indices, ICD=60.0/Yvonne/GympieNP/20150622_000000.wav/Towsey.Acoustic/20150622_000000__2Maps.bmp")
#dims <- c(406, 632)
#class(x)
#image(x)
#summary(x)
#x <- raster("20150622_000000__2Maps.png", band=1)
#y <- raster("20150622_000000__2Maps.png", band=2)
#z <- raster("20150622_000000__2Maps.png", band=3)

#plotRGB(brick("20150622_000000__2Maps.png"))