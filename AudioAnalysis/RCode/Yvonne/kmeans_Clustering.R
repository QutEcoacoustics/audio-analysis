# 9 July 2015
# kmeans clustering using variables determined from the correlation matrix 
# and principal component analysis
# check out kmeansruns {fpc} because this gives a "bestk" value
# for the optimum number of clusters

#  This file is #6 in the sequence:
#  1. Save_Summary_Indices_ as_csv_file.R
#  2. Plot_Towsey_Summary_Indices.R
#  3. Correlation_Matrix.R
#  4. Principal_Component_Analysis.R
#  5. Quantisation_error.R
# *6. kMeans_Clustering.R
#  7. Distance_matrix.R
#  8. Minimising_error.R
#  9. Segmenting_image.R

#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv",header = T)

site <- indices$site[1]
startDate <- indices$rec.date[1]
endDate <- indices$rec.date[length(indices$rec.date)]

################ Normalise data ####################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

# Pre-processing transformation of Temporal Entropy
# to correct the long tail 
indices[,14] <- sqrt(indices[,14])

normIndices <- indices

# normalise variable columns
normIndices[,4]  <- normalise(indices[,4], -50,-10)    # AverageSignalAmplitude
normIndices[,5]  <- normalise(indices[,5], -50,-10)    # BackgroundNoise
normIndices[,6]  <- normalise(indices[,6],  0, 50)    # Snr
normIndices[,7]  <- normalise(indices[,7],  3, 10)    # AvSnrofActive Frames
normIndices[,8]  <- normalise(indices[,8],  0, 1)     # Activity 
normIndices[,9]  <- normalise(indices[,9],  0, 2)     # EventsPerSecond
normIndices[,10] <- normalise(indices[,10], 0, 0.5)   # HighFreqCover
normIndices[,11] <- normalise(indices[,11], 0, 0.5)   # MidFreqCover
normIndices[,12] <- normalise(indices[,12], 0, 0.5)   # LowFreqCover
normIndices[,13] <- normalise(indices[,13], 0.4,0.7)   # AcousticComplexity
normIndices[,14] <- normalise(indices[,14], 0, 0.3)   # TemporalEntropy
normIndices[,15] <- normalise(indices[,15], 0, 0.7)   # EntropyOfAverageSpectrum
normIndices[,16] <- normalise(indices[,16], 0, 1)     # EntropyOfVarianceSpectrum
normIndices[,17] <- normalise(indices[,17], 0, 1)     # EntropyOfPeaksSpectrum
normIndices[,18] <- normalise(indices[,18], 0, 0.7)   # EntropyOfCoefOfVarSpectrum
normIndices[,19] <- normalise(indices[,19], -0.8, 1)   # NDSI
normIndices[,20] <- normalise(indices[,20], 0, 15)     # SptDensity

# adjust values greater than 1 or less than 0
for (j in 4:20) {
  for (i in 1:length(normIndices[,j])) {
    if (normIndices[i,j] > 1) {
      normIndices[i,j] = 1
    }
  }
  for (i in 1:length(normIndices[,j])) {
    if (normIndices[i,j] < 0) {
      normIndices[i,j] = 0
    }
  }
}
##############################################
# Create a list of the number of minutes per day used to plot colours ##########
counter <- NULL
list <- 0
endTime <- length(indices$rec.time)
mn <-indices[grep("\\<0\\>", indices$minute.of.day),]
min.per.day <- NULL
for (k in 1:length(mn$rec.time)) {
  m <- mn$X[k]
  list <- c(list, m)
}
list <- c(list, endTime)

for (j in 1:(length(mn$rec.time)+1)) {
  diff <- list[j+1] - list[j]
  d <- c(min.per.day, diff)
  counter <- c(counter, d)
}

# adjust first and last counter by one
counter[1] <- counter[1]-1
counter[length(mn$rec.time)] <- counter[length(mn$rec.time)]+1

######## Create day identification for different colours in plot #############
number.of.days <- length(unique(indices$rec.date))
day <- NULL

if (counter[1]==0) 
  for (i in 1:(number.of.days+1)) {
    id <- rep(LETTERS[i], counter[i])
    day <- c(day, id)
  }

if (counter[1] > 0) 
  for (i in 1:(number.of.days)) {
    id <- rep(LETTERS[i], counter[i])
    day <- c(day, id)
  }

indices <- cbind(indices, day)

######## Create references for plotting dates and times ################
timeRef <- indices$minute.of.day[1]
offset <- 0 - timeRef 

timePos   <- seq((offset), (nrow(indices)+359), by = 360) # 359 ensures timelabels to end
timeLabel <- c("00:00","6:00","12:00","18:00")
timeLabel <- rep(timeLabel, length.out=(length(timePos))) 

datePos <- c(seq((offset+720), nrow(indices)+1500, by = 1440))
dateLabel <- unique(substr(indices$rec.date, 1,10))
dateLabel <- dateLabel[1:length(datePos)]

##########################################################
library(raster)
colourName <- "colourBlock.png"
colourBlock <- brick(colourName, package="raster")
plotRGB(colourBlock)
colourBlock <- as.data.frame(colourBlock)
colours <- NULL
for(i in 1:30) {
  col <- rgb(colourBlock$colourBlock.1[i],
             colourBlock$colourBlock.2[i],
             colourBlock$colourBlock.3[i],
             max = 255)
  colours <- c(colours, col)
}

png(
  "Cluster 22-28 June 2015 5,7,9,10,11,13,14,15,17,18.png",
  width     = 400,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
#***#***#***#***#***#***#***#
#indicesRef <- c(5,9,11,13,14,15,17)
#set.seed(1234)
#***#***#***#***#***#
#***#***#***#***#***#***#***#
#indicesRef <- c(5,8,10,13,14,15,17)
#set.seed(1234)
#***#***#***#***#***#
#indicesRef <- c(5,6,8,11,12,15,17,18,19,20)
#indicesRef <- c(5,9,11,14,15,17,18,20)
indicesRef <- c(5,7,9,10,11,13,14,15,17,18) 
#0.8878094 for k = 30, Gympie NP 22 to 28 June 2015
set.seed(1092)
#set.seed(1234)
length1 <- 0
length2 <- length(normIndices$X)

length <- length(indices$rec.date)
dataFrame <- normIndices[length1:length2, indicesRef]  # best eleven variables
kmeansObj <- kmeans(dataFrame, centers = 30, iter.max = 20,
                    nstart = 3)
kmeansObj
normIndices <- cbind(normIndices, unname(kmeansObj$cluster))
#plot(dataFrame, col=kmeansObj$cluster)
r <- (kmeansObj$betweenss*100/kmeansObj$totss)
vector <- kmeansObj$cluster[length1:length2]
normIndicesVector <- cbind(normIndices[length1:length2,],vector)

#length1 <- 2881
#length2 <-  5760  #length(normIndices$X)
vector <- kmeansObj$cluster[length1:length2]
normIndicesVector <- cbind(normIndices[length1:length2,],vector)

# The clusterOrder comes from the distance matrix
clusterOrder <- c("29","10","21","27","2","3","19","28","11",
                  "5","17","26","12","16","15","13","6","30",
                  "25","14","20","7","4","8","23","1","18","22",
                  "24","9") # for 5,7,9,10,11,12,13,14,15,17 with seed 

clusterOrder <- c(29,10,21,27,2,3,19,28,11,5,17,26,12,16,15,13,6,30,
                  25,14,20,7,4,8,23,1,18,22,24,9) # for 5,7,9,10,11,12,13,14,15,17 with seed 
colourOrder <- cbind(clusterOrder, colours)
colourOrder <- as.data.frame(colourOrder)
colourOrder$clusterOrder <- as.numeric(as.character(colourOrder$clusterOrder))
colourOrder[ do.call(order, colourOrder), ]

################################################
sort.Filename <- function(filenames) {
  index <- numeric()
  for(i in 1:length(filenames)){
    index[i] <- sub(".*_([[:digit:]]+).*", "\\1", filenames[i])
  }
  index <- as.numeric(index)
  index.sorted <- sort(index, index.return=TRUE)
  rightOrder <- filenames[index.sorted[[2]]]
  return(rightOrder)
}
###############################################
plot(normIndicesVector$vector,
     #col = normIndicesVector$vector, 
     col = colours[normIndicesVector$vector], 
     xaxt = 'n', xlab = " ", ylab = "Cluster reference", 
     main = paste(site, "24 to 25 June 2015", sep = " "),
     cex.main = 2)
axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
     cex.axis = 1.5)
#axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
 #    tick = FALSE, cex.axis = 1.5)
mtext(paste(site, "_", round(r, 3), "%", sep = " "), side=4, 
      cex =1.5)
mtext(paste(indicesRef, collapse = ", ", sep = ""), side=3, 
      line = 0.2, cex = 1.5)
abline(v = 0, lwd=1.5, lty = 3)

#offset <- indices$minute.of.day[1]

abline(v = 1440-offset, lwd=1.5, lty = 3)
abline(v = 2880-offset, lwd=1.5, lty = 3)
abline(v = 4320-offset, lwd=1.5, lty = 3)
abline(v = 5760-offset, lwd=1.5, lty = 3)
abline(v = 7200-offset, lwd=1.5, lty = 3)
abline(v = 8640-offset, lwd=1.5, lty = 3)
#abline(v = 360-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 420-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 480-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 540-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 600-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 660-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 720-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 780-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 840-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 900-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 960-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 1020-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 1080-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 1140-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 1200-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 1260-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 1320-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 1380-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 1440-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 2040-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 2100-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 2160-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 3600-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 6480-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 7920-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 9360-offset, lwd=1.5, lty = 3, col = "grey")
#abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
########################
# Text labels
textLabels <- read.csv("Textfiles_GympieNP1_.csv", 
                       header = TRUE)
#textLabels <- read.csv("Label_text_Gympie NP1_22_06_15.csv", 
#header = TRUE)
for (l in 1:length(textLabels$des)) {
  if (textLabels$des[l] == "lightrain") {
    abline(v = textLabels$start[l], lwd=0.2, lty = 3, 
           col = "blue")
  }
  if(textLabels$des[l] == "rain") {
      abline(v = textLabels$start[l], lwd=0.4, lty = 3, 
      col = "blue")
  }
  if (textLabels$des[l] == "heavyrain") {
  abline(v = textLabels$start[l], lwd=0.6, lty = 3,
  col = "blue")
  }
  if (textLabels$des[l] == "plane") {
    abline(v = textLabels$start[l], lwd=1, lty = 3,
           col = "green")
  }
  if (textLabels$des[l] == "distantplane") {
    abline(v = textLabels$start[l], lwd=0.5, lty = 3,
           col = "green")
  }
  if (textLabels$des[l] == "crows") {
    abline(v = textLabels$start[l], lwd=0.5, lty = 3,
           col = "red")
  }
  if (textLabels$des[l] == "helicopter") {
    abline(v = textLabels$start[l], lwd=0.6, lty = 3,
           col = "magenta")
  }
  if (textLabels$des[l] == "lorikeets") {
    abline(v = textLabels$start[l], lwd=0.6, lty = 3,
           col = "pink")
  }
  if (textLabels$des[l] == "birdscalling") {
    abline(v = textLabels$start[l], lwd=0.4, lty = 3,
           col = "yellow")
  }
  if (textLabels$des[l] == "birdcalls") {
    abline(v = textLabels$start[l], lwd=0.4, lty = 3,
           col = "yellow")
  }
}

dev.off()

# Histogram
par(mfrow = c(3,2), mar = c(3,3,2,1))
hist(normIndicesVector$vector[1:1440],    breaks = 40, ylim= c(0,410), xlim = c(0,31))
hist(normIndicesVector$vector[1441:2880], breaks = 40, ylim= c(0,410), xlim = c(0,31))
hist(normIndicesVector$vector[2881:4320], breaks = 30, ylim= c(0,410), xlim = c(0,31))
hist(normIndicesVector$vector[4321:5760], breaks = 30, ylim= c(0,410), xlim = c(0,31))
hist(normIndicesVector$vector[5761:7200], breaks = 30, ylim= c(0,410), xlim = c(0,31))
hist(normIndicesVector$vector[7201:8640], breaks = 30, ylim= c(0,410), xlim = c(0,31))

#dataFrame <- normIndices[,c(5,7,9,10,11,12,13,14,15,17)]
#kmeansObj <- kmeans(dataFrame, centers=20, iter.max = 30)
#kmeansObj # 87.2% different for different sets of data

plot(normIndices[c(9,4)],col=kmeansObj$cluster)
plot(normIndices[c(9,5)],col=kmeansObj$cluster)
plot(normIndices[c(9,6)],col=kmeansObj$cluster)
plot(normIndices[c(9,7)],col=kmeansObj$cluster)
plot(normIndices[c(9,8)],col=kmeansObj$cluster)
plot(normIndices[c(9,10)],col=kmeansObj$cluster)
plot(normIndices[c(9,11)],col=kmeansObj$cluster)
plot(normIndices[c(9,12)],col=kmeansObj$cluster)
plot(normIndices[c(9,13)],col=kmeansObj$cluster)
plot(normIndices[c(9,14)],col=kmeansObj$cluster)
plot(normIndices[c(9,15)],col=kmeansObj$cluster)
plot(normIndices[c(9,16)],col=kmeansObj$cluster)
plot(normIndices[c(9,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(9,18)],col=kmeansObj$cluster)

plot(normIndices[c(10,4)],col=kmeansObj$cluster)
plot(normIndices[c(10,5)],col=kmeansObj$cluster)
plot(normIndices[c(10,6)],col=kmeansObj$cluster)
plot(normIndices[c(10,7)],col=kmeansObj$cluster)
plot(normIndices[c(10,8)],col=kmeansObj$cluster)
plot(normIndices[c(10,9)],col=kmeansObj$cluster)
plot(normIndices[c(10,10)],col=kmeansObj$cluster)
plot(normIndices[c(10,11)],col=kmeansObj$cluster)
plot(normIndices[c(10,12)],col=kmeansObj$cluster)
plot(normIndices[c(10,13)],col=kmeansObj$cluster)
plot(normIndices[c(10,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(10,15)],col=kmeansObj$cluster)
plot(normIndices[c(10,16)],col=kmeansObj$cluster)
plot(normIndices[c(10,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(10,18)],col=kmeansObj$cluster)

plot(normIndices[c(11,4)],col=kmeansObj$cluster)
plot(normIndices[c(11,5)],col=kmeansObj$cluster)
plot(normIndices[c(11,6)],col=kmeansObj$cluster)
plot(normIndices[c(11,7)],col=kmeansObj$cluster)
plot(normIndices[c(11,8)],col=kmeansObj$cluster)
plot(normIndices[c(11,9)],col=kmeansObj$cluster)
plot(normIndices[c(11,10)],col=kmeansObj$cluster)
plot(normIndices[c(11,11)],col=kmeansObj$cluster) # Mid Freq cover
plot(normIndices[c(11,12)],col=kmeansObj$cluster)
plot(normIndices[c(11,13)],col=kmeansObj$cluster)
plot(normIndices[c(11,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(11,15)],col=kmeansObj$cluster)
plot(normIndices[c(11,16)],col=kmeansObj$cluster)
plot(normIndices[c(11,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(11,18)],col=kmeansObj$cluster)

plot(normIndices[c(12,4)],col=kmeansObj$cluster)
plot(normIndices[c(12,5)],col=kmeansObj$cluster)
plot(normIndices[c(12,6)],col=kmeansObj$cluster)
plot(normIndices[c(12,7)],col=kmeansObj$cluster)
plot(normIndices[c(12,8)],col=kmeansObj$cluster)
plot(normIndices[c(12,9)],col=kmeansObj$cluster)
plot(normIndices[c(12,10)],col=kmeansObj$cluster)
plot(normIndices[c(12,11)],col=kmeansObj$cluster)
plot(normIndices[c(12,12)],col=kmeansObj$cluster)
plot(normIndices[c(12,13)],col=kmeansObj$cluster)
plot(normIndices[c(12,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(12,15)],col=kmeansObj$cluster)
plot(normIndices[c(12,16)],col=kmeansObj$cluster)
plot(normIndices[c(12,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(12,18)],col=kmeansObj$cluster)

plot(normIndices[c(13,4)],col=kmeansObj$cluster)
plot(normIndices[c(13,5)],col=kmeansObj$cluster)
plot(normIndices[c(13,6)],col=kmeansObj$cluster)
plot(normIndices[c(13,7)],col=kmeansObj$cluster)
plot(normIndices[c(13,8)],col=kmeansObj$cluster)
plot(normIndices[c(13,9)],col=kmeansObj$cluster)
plot(normIndices[c(13,10)],col=kmeansObj$cluster)
plot(normIndices[c(13,11)],col=kmeansObj$cluster) # MidFreqCover
plot(normIndices[c(13,12)],col=kmeansObj$cluster)
plot(normIndices[c(13,13)],col=kmeansObj$cluster)# Acoustic Complexity
plot(normIndices[c(13,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(13,15)],col=kmeansObj$cluster)# AverEntropySpec
plot(normIndices[c(13,16)],col=kmeansObj$cluster)# VarianceEntropy
plot(normIndices[c(13,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(13,18)],col=kmeansObj$cluster)

plot(normIndices[c(14,4)],col=kmeansObj$cluster)
plot(normIndices[c(14,5)],col=kmeansObj$cluster)
plot(normIndices[c(14,6)],col=kmeansObj$cluster)
plot(normIndices[c(14,7)],col=kmeansObj$cluster)
plot(normIndices[c(14,8)],col=kmeansObj$cluster)
plot(normIndices[c(14,9)],col=kmeansObj$cluster)
plot(normIndices[c(14,10)],col=kmeansObj$cluster)
plot(normIndices[c(14,11)],col=kmeansObj$cluster)
plot(normIndices[c(14,12)],col=kmeansObj$cluster)
plot(normIndices[c(14,13)],col=kmeansObj$cluster)
plot(normIndices[c(14,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(14,15)],col=kmeansObj$cluster)
plot(normIndices[c(14,16)],col=kmeansObj$cluster)
plot(normIndices[c(14,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(14,18)],col=kmeansObj$cluster)

plot(normIndices[c(15,4)],col=kmeansObj$cluster)
plot(normIndices[c(15,5)],col=kmeansObj$cluster)
plot(normIndices[c(15,6)],col=kmeansObj$cluster)
plot(normIndices[c(15,7)],col=kmeansObj$cluster)
plot(normIndices[c(15,8)],col=kmeansObj$cluster)
plot(normIndices[c(15,9)],col=kmeansObj$cluster)
plot(normIndices[c(15,10)],col=kmeansObj$cluster)
plot(normIndices[c(15,11)],col=kmeansObj$cluster)
plot(normIndices[c(15,12)],col=kmeansObj$cluster)
plot(normIndices[c(15,13)],col=kmeansObj$cluster)
plot(normIndices[c(15,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(15,15)],col=kmeansObj$cluster) # AvgEntropySpectrum
plot(normIndices[c(15,16)],col=kmeansObj$cluster) # Variance
plot(normIndices[c(15,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(15,18)],col=kmeansObj$cluster)

plot(normIndices[c(16,4)],col=kmeansObj$cluster)
plot(normIndices[c(16,5)],col=kmeansObj$cluster)
plot(normIndices[c(16,6)],col=kmeansObj$cluster)
plot(normIndices[c(16,7)],col=kmeansObj$cluster)
plot(normIndices[c(16,8)],col=kmeansObj$cluster)
plot(normIndices[c(16,9)],col=kmeansObj$cluster)
plot(normIndices[c(16,10)],col=kmeansObj$cluster)
plot(normIndices[c(16,11)],col=kmeansObj$cluster)
plot(normIndices[c(16,12)],col=kmeansObj$cluster)
plot(normIndices[c(16,13)],col=kmeansObj$cluster)
plot(normIndices[c(16,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(16,15)],col=kmeansObj$cluster)
plot(normIndices[c(16,16)],col=kmeansObj$cluster) #VarianceEntropy
plot(normIndices[c(16,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(16,18)],col=kmeansObj$cluster)

plot(normIndices[c(17,4)],col=kmeansObj$cluster)
plot(normIndices[c(17,5)],col=kmeansObj$cluster)
plot(normIndices[c(17,6)],col=kmeansObj$cluster)
plot(normIndices[c(17,7)],col=kmeansObj$cluster)
plot(normIndices[c(17,8)],col=kmeansObj$cluster)
plot(normIndices[c(17,9)],col=kmeansObj$cluster)
plot(normIndices[c(17,10)],col=kmeansObj$cluster)
plot(normIndices[c(17,11)],col=kmeansObj$cluster)
plot(normIndices[c(17,12)],col=kmeansObj$cluster)
plot(normIndices[c(17,13)],col=kmeansObj$cluster)
plot(normIndices[c(17,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(17,15)],col=kmeansObj$cluster)
plot(normIndices[c(17,16)],col=kmeansObj$cluster)
plot(normIndices[c(17,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(17,18)],col=kmeansObj$cluster)

plot(normIndices[c(18,4)],col=kmeansObj$cluster)
plot(normIndices[c(18,5)],col=kmeansObj$cluster)
plot(normIndices[c(18,6)],col=kmeansObj$cluster)
plot(normIndices[c(18,7)],col=kmeansObj$cluster)
plot(normIndices[c(18,8)],col=kmeansObj$cluster)
plot(normIndices[c(18,9)],col=kmeansObj$cluster)
plot(normIndices[c(18,10)],col=kmeansObj$cluster)
plot(normIndices[c(18,11)],col=kmeansObj$cluster)
plot(normIndices[c(18,12)],col=kmeansObj$cluster)
plot(normIndices[c(18,13)],col=kmeansObj$cluster)
plot(normIndices[c(18,14)],col=kmeansObj$cluster)# Temporal Entropy
plot(normIndices[c(18,15)],col=kmeansObj$cluster)
plot(normIndices[c(18,16)],col=kmeansObj$cluster)
plot(normIndices[c(18,17)],col=kmeansObj$cluster) # Entropy Peaks
plot(normIndices[c(18,18)],col=kmeansObj$cluster)

############# Saving files ####################
png(file= "Clusterplot 5_7_9_10_11_13_14_15_17_18",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 800,
  pointsize = 4
)

plot(normIndicesVector$vector,col=normIndicesVector$vector)
dev.off()

write.csv(as.matrix(kmeansObj$centers), 
          file = paste("Cluster_centers 22-28 June 2015_5,7,9,10,11,12,13,14,15,17,18", site, 
                       ".csv", sep = ""))

write.csv(unname(kmeansObj$cluster),
          file = paste("Cluster_list 22-28 June 2015_5,7,9,10,11,12,13,14,15,17,18", 
          site, ".csv", sep = ""))

###############################################

write.csv(as.matrix(kmeansObj$centers), 
             file = paste("Cluster_centers 22-28 June 2015_5,9,11,13,14,15,17", site, 
              ".csv", sep = ""))

library(MASS)
write.matrix(normIndices, file=paste("normIndicesClusters 5,9,11,13,14,15,17", 
              site, "22-28 June 2015 test .csv", sep=","), sep=",")

plot(normIndicesVector$vector, col=normIndicesVector$vector)

vec<- read.csv(paste("normIndicesClusters ", site, "22-28 June 2015.csv",
                     sep=","), sep=",", header=T)
table(vec$vector)