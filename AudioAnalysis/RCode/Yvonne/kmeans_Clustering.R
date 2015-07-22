# 9 July 2015
# kmeans clustering using variables determined from the correlation matrix 
# and principal component analysis

#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
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

normIndices <- indices
# normalise variable columns
normIndices[,2]  <- normalise(indices[,2],  0, 2)     # HighAmplitudeIndex
normIndices[,3]  <- normalise(indices[,3],  0, 1)     # ClippingIndex
normIndices[,4]  <- normalise(indices[,4], -50, -10)  # AverageSignalAmplitude
normIndices[,5]  <- normalise(indices[,5], -50, -10)  # BackgroundNoise
normIndices[,6]  <- normalise(indices[,6],  0, 50)    # Snr
normIndices[,7]  <- normalise(indices[,7],  3, 20)    # AvSnrofActive Frames
normIndices[,8]  <- normalise(indices[,8],  0, 1)     # Activity 
normIndices[,9]  <- normalise(indices[,9],  0, 5)     # EventsPerSecond
normIndices[,10] <- normalise(indices[,10], 0, 0.5)   # HighFreqCover
normIndices[,11] <- normalise(indices[,11], 0, 0.5)   # MidFreqCover
normIndices[,12] <- normalise(indices[,12], 0, 0.5)   # LowFreqCover
normIndices[,13] <- normalise(indices[,13], 0.4, 0.7) # AcousticComplexity
normIndices[,14] <- normalise(indices[,14], 0, 0.6)   # TemporalEntropy
normIndices[,15] <- normalise(indices[,15], 0, 0.8)   # AvgEntropySpectrum
normIndices[,16] <- normalise(indices[,16], 0, 1)     # VarianceEntropySpectrum
normIndices[,17] <- normalise(indices[,17], 0, 1)     # EntropyPeaks
normIndices[,18] <- normalise(indices[,18], 0, 22)    # SptDensity

# adjust values greater than 1 or less than 0
for (j in 2:18) {
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
png(
  "Cluster plot_rain.png",
  width     = 400,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

indicesRef <- c(5,7,9,10,11,12,13,14,17) 
#0.8878094 for k = 30, Gympie NP 22 to 28 June 2015
set.seed(1234)
length1 <- 0
length2 <- length(normIndices$X)

length <- length(indices$rec.date)
dataFrame <- normIndices[length1:length2, indicesRef]  # best eleven variables
kmeansObj <- kmeans(dataFrame, centers = 30, iter.max = 20)
normIndices <- cbind(normIndices, unname(kmeansObj$cluster))
plot(dataFrame, col=kmeansObj$cluster)
r <- (kmeansObj$betweenss*100/kmeansObj$totss)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices[length1:length2,],vector)
plot(normIndicesVector$vector[length1:length2],col=normIndicesVector$vector[length1:length2], xaxt = 'n',
     xlab = " ", ylab = "Cluster reference", 
     main = paste(site, startDate, "to", endDate, sep = " "),
     cex.main = 0.8)
axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
     cex.axis = 0.7)
axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
     tick = FALSE, cex.axis = 0.8)

mtext(paste(site, "_", round(r, 3), "%", sep = " "), side=4)
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
  if (textLabels$ref[l] == "8") {
    abline(v = textLabels$start[l], lwd=0.2, lty = 3, 
           col = "blue")
    #mtext(side = 1, line = -15+m, paste(textLabels$des[l]), 
     #     at = textLabels$min[l]+1, col = "blue", cex = 0.5)
  }
  if (textLabels$ref[l] == "1") {
    abline(v = textLabels$start[l], lwd=0.2, lty = 3, 
           col = "red")
    mtext(side = 1, line = -15+m, paste(textLabels$des[l]), 
         at = textLabels$min[l]+1, col = "blue", cex = 0.5)
  }
  if (textLabels$ref[l] == "3") {
    abline(v = textLabels$start[l], lwd=0.2, lty = 3, 
           col = "magenta")
    mtext(side = 1, line = -15+m, paste(textLabels$des[l]), 
          at = textLabels$min[l]+1, col = "blue", cex = 0.5)
  }
}
dev.off()




  if (textLabels$ref[l] == 2){
    abline(v = textLabels$min[l]-offset, lwd=0.2, lty = 3, 
           col = "red")
    mtext(side = 1, line = -15+m, paste(textLabels$des[m]), 
          at = textLabels$min[m]+1, col = "red", cex = 0.5)
  }
  if (textLabels$ref[l] == 3){
    abline(v = textLabels$min[l]-offset, lwd=0.2, lty = 3, 
           col = "green")
    mtext(side = 1, line = -15+m, paste(textLabels$des[m]), 
          at = textLabels$min[m]+1, col = "green", cex = 0.5)
  }
  if (textLabels$ref[l] == 4) {
    abline(v = textLabels$min[l]-offset, lwd=0.2, lty = 3, 
           col = "black")
    mtext(side = 1, line = -15+m, paste(textLabels$des[m]), 
          at = textLabels$min[m]+1, col = "black", cex = 0.5)
  }
  #abline(v= textLabels$endMin[l]-offset, lwd= 1.4, lty =3, col="green")
  mtext(side = 3, "blue = light wind; red = bird calls; green = crows; black = plane")
}
for (m in 1:10) {
  mtext(side = 1, line = -15+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
   #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 11:20) {
  mtext(side = 1, line = -35+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5 )
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 21:30) {
  mtext(side = 1, line = -36+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 31:40) {
  mtext(side = 1, line = -56+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 41:50) {
  mtext(side = 1, line = -56+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 51:60) {
  mtext(side = 1, line = -76+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 61:70) {
  mtext(side = 1, line = -76+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 71:80) {
  mtext(side = 1, line = -96+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 81:90) {
  mtext(side = 1, line = -96+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
for (m in 91:100) {
  mtext(side = 1, line = -106+m, paste(textLabels$des[m]), 
        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
  #     at = textLabels$endMin[m]+1, col = "green")
}
#for (m in 101:110) {
#  mtext(side = 1, line = -116+m, paste(textLabels$des[m]), 
#        at = textLabels$min[m]+1, col = "blue", cex = 0.5)
#  #mtext(side = 1, line = -39+m, paste(textLabels$description[m]), 
#  #     at = textLabels$endMin[m]+1, col = "green")
#}

dev.off()

mtext(side = 1, line = -2, paste(textLabels$des[1]), 
      at = textLabels$startMin[1]+1, col = "red")
mtext(side = 1, line = -2, paste(textLabels$description[2]), 
      at = textLabels$startMin[2]+1, col = "red")
mtext(side = 1, line = -2, paste(textLabels$description[3]), 
      at = textLabels$startMin[3]+1, col = "red")
mtext(side = 1, line = -2, paste(textLabels$description[4]), 
      at = textLabels$startMin[4]+1, col = "red")
mtext(side = 1, line = -2, paste(textLabels$description[5]), 
      at = textLabels$startMin[5]+1, col = "red")
#mtext(side = 1, line = -2, paste(textLabels$description[1]), 
 #     at = textLabels$endMin[1]+1, col = "green")
#mtext(side = 1, line = -2, paste(textLabels$description[2]), 
 #     at = textLabels$endMin[2]+1, col = "green")
#mtext(side = 1, line = -2, paste(textLabels$description[3]), 
#      at = textLabels$endMin[3]+1, col = "green" )
#mtext(side = 1, line = -2, paste(textLabels$description[4]), 
#      at = textLabels$endMin[4]+1, col = "green")
#mtext(side = 1, line = -2, paste(textLabels$description[5]), 
#      at = textLabels$endMin[5]+1, col = "green")

indicesRef <- c(6,7,10,11,13,14,17,18) 
#0.905686243 for k = 30, Gympie NP 22 to 28 June 2015
dataFrame <- normIndices[1:3600, indicesRef]  # best ten variables
kmeansObj <- kmeans(dataFrame, centers = 30, iter.max = 20)
plot(dataFrame, col=kmeansObj$cluster)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices[1:3600,],vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector, xaxt = 'n',
     xlab = " ", ylab = "Cluster reference", 
     main = paste(site, startDate, "to", endDate, sep = " "),
     cex.main = 0.8)
axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
     cex.axis = 0.7)
axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
     tick = FALSE, cex.axis = 0.8)
r <- (kmeansObj$betweenss*100/kmeansObj$totss)
mtext(paste(site, "normIndices ", indicesRef, r, "%", sep = " "), side=3)
offset <- indices$minute.of.day[1]

abline(v = 1440-offset, lwd=1.5, lty = 3)
abline(v = 2880-offset, lwd=1.5, lty = 3)
abline(v = 4320-offset, lwd=1.5, lty = 3)
abline(v = 5760-offset, lwd=1.5, lty = 3)
abline(v = 7200-offset, lwd=1.5, lty = 3)
abline(v = 8640-offset, lwd=1.5, lty = 3)
abline(v = 720-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 2160-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 3600-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 6480-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 7920-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 9360-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
##########################################
indicesRef <- c(7,10,11,13,14,17,18) 
#0.919849777396517"
# for k = 30, Gympie NP 22 to 28 June 2015
set.seed(123)
dataFrame <- normIndices[1:3600, indicesRef]  # best nine variables
kmeansObj <- kmeans(dataFrame, centers = 30, iter.max = 20)
plot(dataFrame, col=kmeansObj$cluster)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices[1:3600,],vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector, xaxt = 'n',
     xlab = " ", ylab = "Cluster reference", 
     main = paste(site, startDate, "to", endDate, sep = " "),
     cex.main = 0.8)
axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
     cex.axis = 0.7)
axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
     tick = FALSE, cex.axis = 0.8)
r <- (kmeansObj$betweenss*100/kmeansObj$totss)
mtext(paste(site, "_", round(r,3), "%", sep = " "), side=4)
offset <- indices$minute.of.day[1]

abline(v = 1440-offset, lwd=1.5, lty = 3)
abline(v = 2880-offset, lwd=1.5, lty = 3)
abline(v = 4320-offset, lwd=1.5, lty = 3)
abline(v = 5760-offset, lwd=1.5, lty = 3)
abline(v = 7200-offset, lwd=1.5, lty = 3)
abline(v = 8640-offset, lwd=1.5, lty = 3)
abline(v = 720-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 2160-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 3600-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 6480-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 7920-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 9360-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
#############################################
indicesRef <- c(7,10,11,13,14,17,18) 
#0.919849777396517"
# for k = 30, Gympie NP 26 to 28 June 2015
set.seed(123)
dataFrame <- normIndices[4000:length, indicesRef]  # best nine variables
kmeansObj <- kmeans(dataFrame, centers = 30, iter.max = 20)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices[4000:length,],vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector, xaxt = 'n',
     xlab = " ", ylab = "Cluster reference", 
     main = paste(site, startDate, "to", endDate, sep = " "),
     cex.main = 0.8)
axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
     cex.axis = 0.7)
axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
     tick = FALSE, cex.axis = 0.8)
r <- (kmeansObj$betweenss*100/kmeansObj$totss)
mtext(paste(site, "_", round(r,3), "%", sep = " "), side=4)
offset <- indices$minute.of.day[1]

abline(v = 1440-offset, lwd=1.5, lty = 3)
abline(v = 2880-offset, lwd=1.5, lty = 3)
abline(v = 4320-offset, lwd=1.5, lty = 3)
abline(v = 5760-offset, lwd=1.5, lty = 3)
abline(v = 7200-offset, lwd=1.5, lty = 3)
abline(v = 8640-offset, lwd=1.5, lty = 3)
abline(v = 720-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 2160-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 3600-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 6480-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 7920-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 9360-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
#######################################
indicesRef <- c(7,10,11,13,14,17,18) 
#0.919849777396517"
# for k = 30, Gympie NP 22 to 28 June 2015
dataFrame <- normIndices[indicesRef]  # best nine variables
kmeansObj <- kmeans(dataFrame, centers = 30, iter.max = 20)
plot(normIndicesVector$vector,col=normIndicesVector$vector, xaxt = 'n',
     xlab = " ", ylab = "Cluster reference", 
     main = paste(site, startDate, "to", endDate, sep = " "),
     cex.main = 0.8)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector, xaxt = 'n',
     xlab = " ", ylab = "Cluster reference", 
     main = paste(site, startDate, "to", endDate, sep = " "),
     cex.main = 0.8)
axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
     cex.axis = 0.7)
axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
     tick = FALSE, cex.axis = 0.8)
r <- (kmeansObj$betweenss*100/kmeansObj$totss)
mtext(paste(site, "_", round(r,3), "%", sep = " "), side=4)
offset <- indices$minute.of.day[1]

abline(v = 1440-offset, lwd=1.5, lty = 3)
abline(v = 2880-offset, lwd=1.5, lty = 3)
abline(v = 4320-offset, lwd=1.5, lty = 3)
abline(v = 5760-offset, lwd=1.5, lty = 3)
abline(v = 7200-offset, lwd=1.5, lty = 3)
abline(v = 8640-offset, lwd=1.5, lty = 3)
abline(v = 720-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 2160-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 3600-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 6480-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 7920-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 9360-offset, lwd=1.5, lty = 3, col = "grey")
abline(v = 5040-offset, lwd=1.5, lty = 3, col = "grey")

# k-medoids clustering
library(fpc)
#pamk.result <- pamk(normIndices[c(720:1440,2160:2880,
 #                 3600:4320,5040:5760),c(16:17)])
pamk.result <- pamk(normIndices[c(0:3600),c(6,8,10)], k=10)

# number of clusters
dev.off()
pamk.result$nc
clusters <- unname(pamk.result$pamobject$clustering)
plot(clusters, col=clusters)
abline(v=0, lwd=1.5, lty = 3)
abline(v=720, lwd=1.5, lty = 3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2160, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)

# check clustering against species
#table(pamk.result$pamobject$clustering, iris$Species)
layout(matrix(c(1,2),1,2))
plot(pamk.result$pamobject)
layout(matrix(1)) # change back to one plot per page
plot(pamk.result$pamobject)
##################################################

#normIndices[,4:18] <- scale(normIndices[,4:18])

#dataFrame <- rbind(indices,indices1)

set.seed(1234)
par(mar=c(0,0,0,0))
x <- rnorm(12, mean = rep(1:3, each=4), sd=0.2)
y <- rnorm(12, mean = rep(1,2,1, each=4), sd=0.2)
plot(x,y, col="blue",pch =19, cex=2)
text(x+0.04, y+0.04, labels = as.character(1:12))
dataFrame <- data.frame(x,y)
points(kmeansObj$centers, col=1:3, pch=3,cex=3,lwd=3)
kmeansObj <- kmeans(dataFrame, centers = 5)
names(kmeansObj)
kmeansObj$cluster
plot(dataFrame, col = kmeansObj$cluster)

library(stats)
par(mar=c(4,4,1,1))
dataFrame <- normIndices[,c(3,6,7,10,11,13,14,17,18)]
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj$cluster

library(stats)
par(mar=c(4,4,1,1))
dataFrame <- normIndices[,c(5,6,7,9,11,12,17,15,13)]
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj$cluster
par(mfrow=c(3,3))
par(mar = c(4, 4, 1, 1), oma = c(2, 2, 2, 2))
#par(tcl = -0.25)
#par(mgp = c(2, 0.6, 0))
#plot(dataFrame, col=kmeansObj$cluster)

#5,6,7,9,11,12,17,15,13
plot(normIndices[c(5,5)],col=kmeansObj$cluster)
plot(normIndices[c(5,6)],col=kmeansObj$cluster)
plot(normIndices[c(5,7)],col=kmeansObj$cluster)
plot(normIndices[c(5,9)],col=kmeansObj$cluster)
plot(normIndices[c(5,11)],col=kmeansObj$cluster)
plot(normIndices[c(5,12)],col=kmeansObj$cluster)
plot(normIndices[c(5,17)],col=kmeansObj$cluster)
plot(normIndices[c(5,15)],col=kmeansObj$cluster)
plot(normIndices[c(5,13)],col=kmeansObj$cluster)
#plot(normIndices[c(5,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

plot(normIndices[c(6,3)],col=kmeansObj$cluster)
plot(normIndices[c(6,5)],col=kmeansObj$cluster)
plot(normIndices[c(6,6)],col=kmeansObj$cluster)
plot(normIndices[c(6,7)],col=kmeansObj$cluster)
plot(normIndices[c(6,10)],col=kmeansObj$cluster)
plot(normIndices[c(6,11)],col=kmeansObj$cluster)
plot(normIndices[c(6,13)],col=kmeansObj$cluster)
plot(normIndices[c(6,14)],col=kmeansObj$cluster)
plot(normIndices[c(6,17)],col=kmeansObj$cluster)
plot(normIndices[c(6,18)],col=kmeansObj$cluster)
#plot(normIndices[c(6,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

plot(normIndices[c(7,3)],col=kmeansObj$cluster)
plot(normIndices[c(7,6)],col=kmeansObj$cluster)
plot(normIndices[c(7,7)],col=kmeansObj$cluster)
plot(normIndices[c(7,10)],col=kmeansObj$cluster)
plot(normIndices[c(7,11)],col=kmeansObj$cluster)
plot(normIndices[c(7,13)],col=kmeansObj$cluster)
plot(normIndices[c(7,14)],col=kmeansObj$cluster)
plot(normIndices[c(7,17)],col=kmeansObj$cluster)
plot(normIndices[c(7,18)],col=kmeansObj$cluster)
#plot(normIndices[c(7,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

plot(normIndices[c(10,10)],col=kmeansObj$cluster)
plot(normIndices[c(10,3)],col=kmeansObj$cluster)
plot(normIndices[c(10,6)],col=kmeansObj$cluster)
plot(normIndices[c(10,7)],col=kmeansObj$cluster)
plot(normIndices[c(10,11)],col=kmeansObj$cluster)
plot(normIndices[c(10,13)],col=kmeansObj$cluster)
plot(normIndices[c(10,14)],col=kmeansObj$cluster)
plot(normIndices[c(10,17)],col=kmeansObj$cluster)
plot(normIndices[c(10,18)],col=kmeansObj$cluster)
#plot(normIndices[c(9,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

plot(normIndices[c(11,3)],col=kmeansObj$cluster)
plot(normIndices[c(11,6)],col=kmeansObj$cluster)
plot(normIndices[c(11,7)],col=kmeansObj$cluster)
plot(normIndices[c(11,10)],col=kmeansObj$cluster)
plot(normIndices[c(11,11)],col=kmeansObj$cluster)
plot(normIndices[c(11,13)],col=kmeansObj$cluster)
plot(normIndices[c(11,14)],col=kmeansObj$cluster)
plot(normIndices[c(11,17)],col=kmeansObj$cluster)
plot(normIndices[c(11,18)],col=kmeansObj$cluster)
#plot(normIndices[c(11,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

plot(normIndices[c(13,3)],col=kmeansObj$cluster)
plot(normIndices[c(13,6)],col=kmeansObj$cluster)
plot(normIndices[c(13,7)],col=kmeansObj$cluster)
plot(normIndices[c(13,10)],col=kmeansObj$cluster)
plot(normIndices[c(13,11)],col=kmeansObj$cluster)
plot(normIndices[c(13,13)],col=kmeansObj$cluster)
plot(normIndices[c(13,14)],col=kmeansObj$cluster)
plot(normIndices[c(13,17)],col=kmeansObj$cluster)
plot(normIndices[c(13,18)],col=kmeansObj$cluster)
#plot(normIndices[c(12,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

plot(normIndices[c(14,3)],col=kmeansObj$cluster)
plot(normIndices[c(14,6)],col=kmeansObj$cluster)
plot(normIndices[c(14,7)],col=kmeansObj$cluster)
plot(normIndices[c(14,10)],col=kmeansObj$cluster)
plot(normIndices[c(14,11)],col=kmeansObj$cluster)
plot(normIndices[c(14,13)],col=kmeansObj$cluster)
plot(normIndices[c(14,14)],col=kmeansObj$cluster)
plot(normIndices[c(14,17)],col=kmeansObj$cluster)
plot(normIndices[c(14,18)],col=kmeansObj$cluster)
#plot(normIndices[c(14,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

plot(normIndices[c(17,3)],col=kmeansObj$cluster)
plot(normIndices[c(17,6)],col=kmeansObj$cluster)
plot(normIndices[c(17,7)],col=kmeansObj$cluster)
plot(normIndices[c(17,10)],col=kmeansObj$cluster)
plot(normIndices[c(17,11)],col=kmeansObj$cluster)
plot(normIndices[c(17,13)],col=kmeansObj$cluster)
plot(normIndices[c(17,14)],col=kmeansObj$cluster)
plot(normIndices[c(17,17)],col=kmeansObj$cluster)
plot(normIndices[c(17,18)],col=kmeansObj$cluster)
#plot(normIndices[c(17,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

plot(normIndices[c(18,3)],col=kmeansObj$cluster)
plot(normIndices[c(18,6)],col=kmeansObj$cluster)
plot(normIndices[c(18,7)],col=kmeansObj$cluster)
plot(normIndices[c(18,10)],col=kmeansObj$cluster)
plot(normIndices[c(18,11)],col=kmeansObj$cluster)
plot(normIndices[c(18,13)],col=kmeansObj$cluster)
plot(normIndices[c(18,14)],col=kmeansObj$cluster)
plot(normIndices[c(18,17)],col=kmeansObj$cluster)
plot(normIndices[c(18,18)],col=kmeansObj$cluster)
#plot(normIndices[c(18,6)], axes = FALSE, type = "n", xlab="",ylab="")
kmeansObj

dataFrame <- normIndices[,c(4:18)]
kmeansObj <- kmeans(dataFrame, centers=20)

dataFrame <- normIndices[,c(4,5,8,11,13:17)] # nine variables
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #81.5%

dataFrame <- normIndices[,c(5,8,11,13:17)] # nine variables minus 4
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #82.4%

dataFrame <- normIndices[,c(4,8,11,13:17)] # nine variables -5
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #82.7%

dataFrame <- normIndices[,c(4,5,11,13:17)] # nine variables -8
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #83.4%

dataFrame <- normIndices[,c(4,5,8,13:17)] # nine variables - 11
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #82.8%

dataFrame <- normIndices[,c(4,5,8,11,14:17)] # nine variables -13
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #82.3%

dataFrame <- normIndices[,c(4,5,8,11,13,15:17)] # nine variables -14
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #82.9%

dataFrame <- normIndices[,c(4,5,8,11,13:14,16:17)] # nine variables -15
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #81.3%

dataFrame <- normIndices[,c(4,5,8,11,13:15,17)] # nine variables -16
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #84%

dataFrame <- normIndices[,c(4,5,8,11,13:16)] # nine variables -17
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #78%

dataFrame <- normIndices[,c(4,5,11,13:15,17)] # nine variables -8,-16
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #86.3%

dataFrame <- normIndices[,c(4,5,8,11,13:17,6)] # nine variables +6
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #79.9%

dataFrame <- normIndices[,c(4,5,8,11,13:17,7)] # nine variables +7
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #81%

dataFrame <- normIndices[,c(4,5,8,11,13:17,9)] # nine variables +9
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #79.8%

dataFrame <- normIndices[,c(4,5,8,11,13:17,10)] # nine variables +10
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #81.0%

dataFrame <- normIndices[,c(4,5,8,11,13:17,12)] # nine variables +12
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #79.5%

dataFrame <- normIndices[,c(4,5,10,11,13:17,18)] # nine variables +18
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #82.4%

dataFrame <- normIndices[,c(5,11,13:15,17,7,10)] # nine variables-8,-16,+7,+10,-4
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #86.3%

dataFrame <- normIndices[,c(5,11,13:15,17,10,7,18)] # nine variables -8,-16,+10
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #86.2%

dataFrame <- normIndices[,c(5,10,11,13:15,16,17,18)]
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #83.7%
###
dataFrame <- normIndices[,c(5,11,13:15,17,18,10,16,7)] # best ten variables 
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #83.2%

dataFrame <- normIndices[,c(5,11,13:15,17,18,7)] # eight variables 
kmeansObj <- kmeans(dataFrame, centers=20)
kmeansObj #86.3%

plot(normIndices[c(4,4)],col=kmeansObj$cluster) # AvgSignalAmp
plot(normIndices[c(4,5)],col=kmeansObj$cluster)
plot(normIndices[c(4,6)],col=kmeansObj$cluster)
plot(normIndices[c(4,7)],col=kmeansObj$cluster)
plot(normIndices[c(4,8)],col=kmeansObj$cluster)
plot(normIndices[c(4,9)],col=kmeansObj$cluster)
plot(normIndices[c(4,10)],col=kmeansObj$cluster)
plot(normIndices[c(4,11)],col=kmeansObj$cluster)
plot(normIndices[c(4,12)],col=kmeansObj$cluster)
plot(normIndices[c(4,13)],col=kmeansObj$cluster)
plot(normIndices[c(4,14)],col=kmeansObj$cluster)
plot(normIndices[c(4,15)],col=kmeansObj$cluster)
plot(normIndices[c(4,16)],col=kmeansObj$cluster)
plot(normIndices[c(4,17)],col=kmeansObj$cluster)
plot(normIndices[c(4,18)],col=kmeansObj$cluster)
kmeansObj

plot(normIndices[c(5,4)],col=kmeansObj$cluster)
plot(normIndices[c(5,5)],col=kmeansObj$cluster) # BackgroundNoise
plot(normIndices[c(5,6)],col=kmeansObj$cluster)
plot(normIndices[c(5,7)],col=kmeansObj$cluster)
plot(normIndices[c(5,8)],col=kmeansObj$cluster)
plot(normIndices[c(5,9)],col=kmeansObj$cluster)
plot(normIndices[c(5,10)],col=kmeansObj$cluster)
plot(normIndices[c(5,11)],col=kmeansObj$cluster)
plot(normIndices[c(5,12)],col=kmeansObj$cluster)
plot(normIndices[c(5,13)],col=kmeansObj$cluster)
plot(normIndices[c(5,14)],col=kmeansObj$cluster)
plot(normIndices[c(5,15)],col=kmeansObj$cluster)
plot(normIndices[c(5,16)],col=kmeansObj$cluster)
plot(normIndices[c(5,17)],col=kmeansObj$cluster)
plot(normIndices[c(5,18)],col=kmeansObj$cluster)

plot(normIndices[c(6,4)],col=kmeansObj$cluster)
plot(normIndices[c(6,5)],col=kmeansObj$cluster)
plot(normIndices[c(6,6)],col=kmeansObj$cluster)
plot(normIndices[c(6,7)],col=kmeansObj$cluster)
plot(normIndices[c(6,8)],col=kmeansObj$cluster)
plot(normIndices[c(6,9)],col=kmeansObj$cluster)
plot(normIndices[c(6,10)],col=kmeansObj$cluster)
plot(normIndices[c(6,11)],col=kmeansObj$cluster)
plot(normIndices[c(6,12)],col=kmeansObj$cluster)
plot(normIndices[c(6,13)],col=kmeansObj$cluster)
plot(normIndices[c(6,14)],col=kmeansObj$cluster)
plot(normIndices[c(6,15)],col=kmeansObj$cluster)
plot(normIndices[c(6,16)],col=kmeansObj$cluster)
plot(normIndices[c(6,17)],col=kmeansObj$cluster)
plot(normIndices[c(6,18)],col=kmeansObj$cluster)

plot(normIndices[c(7,4)],col=kmeansObj$cluster)
plot(normIndices[c(7,5)],col=kmeansObj$cluster)
plot(normIndices[c(7,6)],col=kmeansObj$cluster)
plot(normIndices[c(7,7)],col=kmeansObj$cluster)
plot(normIndices[c(7,8)],col=kmeansObj$cluster)
plot(normIndices[c(7,9)],col=kmeansObj$cluster)
plot(normIndices[c(7,10)],col=kmeansObj$cluster)
plot(normIndices[c(7,11)],col=kmeansObj$cluster)
plot(normIndices[c(7,12)],col=kmeansObj$cluster)
plot(normIndices[c(7,13)],col=kmeansObj$cluster)
plot(normIndices[c(7,14)],col=kmeansObj$cluster)
plot(normIndices[c(7,15)],col=kmeansObj$cluster)
plot(normIndices[c(7,16)],col=kmeansObj$cluster)
plot(normIndices[c(7,17)],col=kmeansObj$cluster)
plot(normIndices[c(7,18)],col=kmeansObj$cluster)

plot(normIndices[c(8,4)],col=kmeansObj$cluster)
plot(normIndices[c(8,5)],col=kmeansObj$cluster)
plot(normIndices[c(8,6)],col=kmeansObj$cluster)
plot(normIndices[c(8,7)],col=kmeansObj$cluster)
plot(normIndices[c(8,8)],col=kmeansObj$cluster) # Activity
plot(normIndices[c(8,9)],col=kmeansObj$cluster)
plot(normIndices[c(8,10)],col=kmeansObj$cluster)
plot(normIndices[c(8,11)],col=kmeansObj$cluster)
plot(normIndices[c(8,12)],col=kmeansObj$cluster)
plot(normIndices[c(8,13)],col=kmeansObj$cluster)
plot(normIndices[c(8,14)],col=kmeansObj$cluster)
plot(normIndices[c(8,15)],col=kmeansObj$cluster)
plot(normIndices[c(8,16)],col=kmeansObj$cluster)
plot(normIndices[c(8,17)],col=kmeansObj$cluster)
plot(normIndices[c(8,18)],col=kmeansObj$cluster)

plot(normIndices[c(9,4)],col=kmeansObj$cluster)
plot(normIndices[c(9,5)],col=kmeansObj$cluster)
plot(normIndices[c(9,6)],col=kmeansObj$cluster)
plot(normIndices[c(9,7)],col=kmeansObj$cluster)
plot(normIndices[c(9,8)],col=kmeansObj$cluster)
plot(normIndices[c(9,9)],col=kmeansObj$cluster)
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
indicesRef <- c(5:9,11,12,15,17)
dataFrame <- normIndices[,indicesRef]
kmeansObj <- kmeans(dataFrame, centers=14)
#kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext(paste(site, "normIndices", indicesRef,"81.0%"), side=3)
abline(v = 1440, lwd=1.5, lty = 3)
abline(v = 2880, lwd=1.5, lty = 3)
abline(v = 0, lwd=1.5, lty = 3)
abline(v = 4320, lwd=1.5, lty = 3)
abline(v = 5760, lwd=1.5, lty = 3)
abline(v = 7200, lwd=1.5, lty = 3)
abline(v = 8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[,c(4,6:8,11:12,14,15,17)]
kmeansObj <- kmeans(dataFrame, centers=14)
#kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext("normIndices[,c(4,6:8,11:12,14,15,17)], 81.8%",side=3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)
abline(v=0, lwd=1.5, lty = 3)
abline(v=4320, lwd=1.5, lty = 3)
abline(v=5760, lwd=1.5, lty = 3)
abline(v=7200, lwd=1.5, lty = 3)
abline(v=8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[c(1:3600),c(5:12,17)]
kmeansObj <- kmeans(dataFrame, centers=14)
kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
#set.seed(12345)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices[c(1:3600),c(5:12,17)],
                           vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext("normIndices[,c(5:12,17)], 80%",side=3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)
abline(v=0, lwd=1.5, lty = 3)
abline(v=4320, lwd=1.5, lty = 3)
abline(v=5760, lwd=1.5, lty = 3)
abline(v=7200, lwd=1.5, lty = 3)
abline(v=8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[1:3200,c(5:13)]
kmeansObj <- kmeans(dataFrame, centers=14)
kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
#set.seed(12345)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices[1:3200,],vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext("normIndices[,c(5:13)], 76%",side=3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)
abline(v=0, lwd=1.5, lty = 3)
abline(v=4320, lwd=1.5, lty = 3)
abline(v=5760, lwd=1.5, lty = 3)
abline(v=7200, lwd=1.5, lty = 3)
abline(v=8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[,c(6,13,17,15,16,5)]
kmeansObj <- kmeans(dataFrame, centers=14)
kmeansObj$cluster
#plot(dataFrame, col=kmeansObj$cluster)
#set.seed(12345)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext("normIndices[,c(5:11,16,17)], 79.6%",side=3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)
abline(v=0, lwd=1.5, lty = 3)
abline(v=4320, lwd=1.5, lty = 3)
abline(v=5760, lwd=1.5, lty = 3)
abline(v=7200, lwd=1.5, lty = 3)
abline(v=8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[,c(5:10,14,16,17)]
kmeansObj <- kmeans(dataFrame, centers=14)
kmeansObj$cluster
#plot(dataFrame, col=kmeansObj$cluster)
#set.seed(12345)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext("normIndices[,c(5:10,14,16,17)], 80%",side=3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)
abline(v=0, lwd=1.5, lty = 3)
abline(v=4320, lwd=1.5, lty = 3)
abline(v=5760, lwd=1.5, lty = 3)
abline(v=7200, lwd=1.5, lty = 3)
abline(v=8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[,c(5:10,13,16,17)]
kmeansObj <- kmeans(dataFrame, centers=14)
kmeansObj$cluster
#plot(dataFrame, col=kmeansObj$cluster)
#set.seed(12345)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext("normIndices[,c(5:10,13,16,17)], 80.4%",side=3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)
abline(v=0, lwd=1.5, lty = 3)
abline(v=4320, lwd=1.5, lty = 3)
abline(v=5760, lwd=1.5, lty = 3)
abline(v=7200, lwd=1.5, lty = 3)
abline(v=8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[,c(5:10,12,16,17)]
kmeansObj <- kmeans(dataFrame, centers=14)
kmeansObj$cluster
#plot(dataFrame, col=kmeansObj$cluster)
#set.seed(12345)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext("normIndices[,c(5:10,12,16,17)], 95%",side=3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)
abline(v=0, lwd=1.5, lty = 3)
abline(v=4320, lwd=1.5, lty = 3)
abline(v=5760, lwd=1.5, lty = 3)
abline(v=7200, lwd=1.5, lty = 3)
abline(v=8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[,c(5:9,11,12,16,17)]
kmeansObj <- kmeans(dataFrame, centers=14)
kmeansObj$cluster
#plot(dataFrame, col=kmeansObj$cluster)
#set.seed(12345)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
mtext("normIndices[,c(5:9,11,12,16,17)], 95%",side=3)
abline(v=1440, lwd=1.5, lty = 3)
abline(v=2880, lwd=1.5, lty = 3)
abline(v=0, lwd=1.5, lty = 3)
abline(v=4320, lwd=1.5, lty = 3)
abline(v=5760, lwd=1.5, lty = 3)
abline(v=7200, lwd=1.5, lty = 3)
abline(v=8640, lwd=1.5, lty = 3)
kmeansObj

dataFrame <- normIndices[,c(4,5,6,7,9,18,11,17,15)]
kmeansObj <- kmeans(dataFrame, centers=16)
#kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
kmeansObj

dataFrame <- normIndices[,c(4,5,6,7,9,18,11,17,15)]
kmeansObj <- kmeans(dataFrame, centers=16)
kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
kmeansObj
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)

dataFrame <- normIndices[,c(4,5,6,7,9,18,11,17,15)]
kmeansObj <- kmeans(dataFrame, centers=16)
#kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
kmeansObj

dataFrame <- normIndices[,c(4,5,6,7,9,18,11,17,15)]
kmeansObj <- kmeans(dataFrame, centers=16)
#kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
kmeansObj
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)

dataFrame <- normIndices[,c(4,5,6,7,9,18,11,17,15)]
kmeansObj <- kmeans(dataFrame, centers=20)
#kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
#kmeansObj
vector <- kmeansObj$cluster
normIndicesVector <- cbind(normIndices,vector)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
## cluster centers "fitted" to each obs.:
fitted.x <- fitted(kmeansObj);
head(fitted.x)
resid.x <- normIndices[,c(5,6,10,12:17)] - fitted(kmeansObj)
# sum of squares
ss <- function(x) sum(scale(normIndices[,4:18], scale = FALSE)^2)
cbind(kmeansObj[c("betweenss", "tot.withinss", "totss")])
(kmeansObj <- kmeans(normIndices[,4:18], 5, nstart = 25))
plot(kmeansObj, col = cl$cluster)
points(cl$centers, col = 1:5, pch = 8)

#, c(ss(fitted.x), ss(resid.x))) # , ss(normIndices)))

dataFrame <- normIndices[,c(4,5,6,7,9,18,11,17,15)]
kmeansObj <- kmeans(dataFrame, centers=20)
#kmeansObj$cluster
plot(dataFrame, col=kmeansObj$cluster)
kmeansObj
vector <- kmeansObj$cluster
png(
  "Clusterplot.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
plot(normIndicesVector$vector,col=normIndicesVector$vector)
dev.off()

library(MASS)
write.matrix(normIndices, file="normIndicesClusters.csv", sep=",")
write.matrix(normIndicesVector, file="normIndicesClusters.csv", sep=",")
plot(normIndicesVector$vector,col=normIndicesVector$vector)

vec<- read.csv("normIndicesClusters.csv", header=T)
table(vec$vector)