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

#setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
setwd("C:\\Work\\CSV files\\GympieNP1_new\\kmeans_30clusters")
indices <- read.csv("C:\\Work\\CSV files\\GympieNP1_new\\all_data\\Towsey_Summary_Indices_Gympie NP1 22-06-2015to current.csv", header = T)

site <- indices$site[1]
startDate <- indices$rec.date[1]
endDate <- indices$rec.date[length(indices$rec.date)]

################ Normalise data ####################
# normalize values using minimum and maximum values
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
# Pre-processing of Temporal Entropy
# to correct the long tail 
indices[,14] <- sqrt(indices[,14])

normIndices <- indices
# normalise variable columns
normIndices[,2]  <- normalise(indices[,2],  0,  2)    # HighAmplitudeIndex (0,2)
normIndices[,3]  <- normalise(indices[,3],  0,  1)    # ClippingIndex (0,1)
normIndices[,4]  <- normalise(indices[,4], -44.34849276,-27.1750784)   # AverageSignalAmplitude (-50,-10)
normIndices[,5]  <- normalise(indices[,5], -45.06046874,-29.52071375)  # BackgroundNoise (-50,-10)
normIndices[,6]  <- normalise(indices[,6],  4.281792124, 25.57295061)  # Snr (0,50)
normIndices[,7]  <- normalise(indices[,7],  3.407526438, 7.653004384)  # AvSnrofActive Frames (3,10)
normIndices[,8]  <- normalise(indices[,8],  0.006581494, 0.453348819)  # Activity (0,1)
normIndices[,9]  <- normalise(indices[,9],  0, 2.691666667)  # EventsPerSecond (0,2)
normIndices[,10] <- normalise(indices[,10], 0.015519804, 0.167782223)  # HighFreqCover (0,0.5)
normIndices[,11] <- normalise(indices[,11], 0.013522414, 0.197555718)  # MidFreqCover (0,0.5)
normIndices[,12] <- normalise(indices[,12], 0.01984127,  0.259381856)  # LowFreqCover (0,0.5)
normIndices[,13] <- normalise(indices[,13], 0.410954108, 0.501671845)  # AcousticComplexity (0.4,0.7)
normIndices[,14] <- normalise(indices[,14], 0.004326753, sqrt(0.155612175))  # TemporalEntropy (0,sqrt(0.3))
normIndices[,15] <- normalise(indices[,15], 0.02130969, 0.769678735)   # EntropyOfAverageSpectrum (0,0.7)
normIndices[,16] <- normalise(indices[,16], 0.098730903, 0.82144857)   # EntropyOfVarianceSpectrum (0,1)
normIndices[,17] <- normalise(indices[,17], 0.119538801, 0.998670805)  # EntropyOfPeaksSpectrum (0,1)
normIndices[,18] <- normalise(indices[,18], 0.004470594, 0.530948096)  # EntropyOfCoVSpectrum (0,0.7)
normIndices[,19] <- normalise(indices[,19], 0.043940755, 0.931257154)  # NDSI (-0.8,1)
normIndices[,20] <- normalise(indices[,20], 1.852187379, 11.79845141)  # SptDensity (0,15)

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
counter[length(mn$rec.time)] <- counter[length(mn$rec.time)] + 1

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
for(i in 1:35) {
  col <- rgb(colourBlock$colourBlock.1[i],
             colourBlock$colourBlock.2[i],
             colourBlock$colourBlock.3[i],
             max = 255)
  colours <- c(colours, col)
}

indicesRef <- c(5,7,9,10,11,12,13,17,18)

set.seed(1653)    # for 30 clusters and 5,7,9,10,11,12,13,17,18

length1 <- 0
length2 <- length(normIndices$X)

length <- length(indices$rec.date)
dataFrame <- normIndices[length1:length2, indicesRef]  # best eleven variables
kmeansObj <- kmeans(dataFrame, centers = 30, iter.max = 100)
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

##### Find cluster order #########################
centers <- kmeansObj$centers

write.table (as.matrix(dist(centers)), 
             file = paste("Distance_matrix_5,7,9,11,12,13,17,20",
                          site, ".csv", sep = ""), sep = ",")
distances <- read.csv(file = paste("Distance_matrix_5,7,9,11,12,13,17,20",
                                   site, ".csv", sep = ""), header=T)
  
# One dimensional analysis
#distances <- read.csv("Distance_matrix_GympieNP 22 June 2015.csv", header =                        T)

dist <- cmdscale(distances[,1:35], k=1)
y <- dist[, 1]
z <- sort(y)

clusterOrder <- names(z)

colourOrder <- cbind(clusterOrder, colours)
colourOrder <- as.data.frame(colourOrder)
colourOrder$clusterOrder <- as.numeric(as.character(colourOrder$clusterOrder))
colourOrder[ do.call(order, colourOrder), ]

################################################
#sort.Filename <- function(filenames) {
#  index <- numeric()
#  for(i in 1:length(filenames)){
#    index[i] <- sub(".*_([[:digit:]]+).*", "\\1", filenames[i])
#  }
#  index <- as.numeric(index)
#  index.sorted <- sort(index, index.return=TRUE)
#  rightOrder <- filenames[index.sorted[[2]]]
#  return(rightOrder)
#}
###############################################
png(
  "kmeans Cluster 22-28 June 2015 5,7,9,11,12,13,17,18.png",
  width     = 400,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

plot(normIndicesVector$vector,
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
dev.off()
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

setwd("C:\\Work\\CSV files\\GympieNP1_new\\all_data\\")

list <- which(normIndicesVector$minute.of.day=="0")
dates <- unique(normIndices$rec.date)
# Histogram
par(mfrow = c(3,2), mar = c(1,2,1,0),oma=c(3,2,0,2))
for(i in 1:6) {
  hist(normIndicesVector$vector[list[i]:list[i+1]], breaks = 36, 
       ylim= c(0,410), xlim = c(1,36), main="")
  mtext(side = 3,paste(dates[i]), line=-2)
}

list <- which(normIndicesVector$minute.of.day=="0")
dates <- unique(normIndices$rec.date)

# Histogram
par(mfrow = c(4,2), mar = c(1,2,1,0),oma=c(3,2,0,2))
for(i in 1:7) {
  hist(normIndicesVector$vector[list[i]:list[i+1]], breaks = 36, ylim= c(0,410), xlim = c(1,36), main="")
  mtext(side = 3,paste(dates[i]), line=-2)
}
seq(1,1000,7)

par(mfrow = c(4,2), mar = c(1,2,1,0),oma=c(3,2,0,2))
for(i in 8:14) {
  hist(normIndicesVector$vector[list[i]:list[i+1]], breaks = 36, ylim= c(0,410), xlim = c(1,36), main="")
  mtext(side = 3,paste(dates[i]), line=-2)
}

hist(normIndicesVector$vector[list[1]:list[2]-1],  breaks = 36, 
     ylim= c(0,410), xlim = c(1,36),main="")
mtext(side = 3,"22 June 2015", line=-2)
hist(normIndicesVector$vector[list[2]:list[3]-1], breaks = 40, 
     ylim= c(0,410), xlim = c(1,36),main="")
mtext(side = 3,"23 June 2015", line = -2)
hist(normIndicesVector$vector[2882:4320], breaks = 30, 
     ylim= c(0,410), xlim = c(1,36), main="")
mtext(side = 3,"24 June 2015", line=-2)
hist(normIndicesVector$vector[4321:5760], breaks = 30, 
     ylim= c(0,410), xlim = c(1,36), main="")
mtext(side=3,"25 June 2015",line = -2)
hist(normIndicesVector$vector[5761:7200], breaks = 30, 
     ylim= c(0,410), xlim = c(1,36), main = "")
mtext(side = 3,"26 June 2015", line=-2)
hist(normIndicesVector$vector[7201:8640], breaks = 30, 
     ylim= c(0,410), xlim = c(1,36), main="")
mtext(side = 3,"27 June 2015", line=-2)
############# Saving files ####################
write.csv(as.matrix(kmeansObj$centers), 
          file = paste("Cluster_centers 22-28 June 2015_5,7,9,10,11,12,13,17,18", site, 
                       ".csv", sep = ""))
write.csv(unname(kmeansObj$cluster),
          file = paste("Cluster_list_kmeans_22June-16July2015_5,7,9,10,11,12,13,17,18_30", 
          site, ".csv", sep = ""), row.names=FALSE)
cluster.list <- read.csv(file = paste("Cluster_list_kmeans_22June-16July2015_5,7,9,10,11,12,13,17,18_30", 
                                      site, ".csv", sep = ""), header = T,
                         col.names = "cluster.list")
###############################################
library(MASS)
write.matrix(normIndices, file=paste("normIndicesClusters 5,7,9,10,11,12,13,17,18", 
              site, "22-28 June 2015 test .csv", sep=","), sep=",")

plot(normIndicesVector$vector, col=normIndicesVector$vector)

vec<- read.csv(paste("normIndicesClusters ", site, "22-28 June 2015.csv",
                     sep=","), sep=",", header=T)
table(vec$vector)

#indices <- cbind(indices, cluster.list)
#write.csv(indices, row.names=FALSE,
#          file = paste("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000a.csv"))
