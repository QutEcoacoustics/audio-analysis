# 3 September 2015
# The Mclust function uses an optimal Bayesian Information criteria (BIC)
# A seed does not need to be set.

setwd("C:\\Work\\CSV files\\GympieNP1_new\\mclust_30clusters")
indices <- read.csv("C:\\Work\\CSV files\\GympieNP1_new\\all_data\\Towsey_Summary_Indices_Gympie NP1 22-06-2015to current.csv")
list <- which(indices$minute.of.day=="0")
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
#indices[,14] <- sqrt(indices[,14])

normIndices <- indices
# normalise variable columns
normIndices[,2]  <- normalise(indices[,2],  0,2)  # HighAmplitudeIndex (0,2)
normIndices[,3]  <- normalise(indices[,3],  0,  1)  # ClippingIndex (0,1)
normIndices[,4]  <- normalise(indices[,4], -44.34849276,-27.1750784)   # AverageSignalAmplitude (-50,-10)
normIndices[,5]  <- normalise(indices[,5], -45.06046874,-29.52071375)  # BackgroundNoise (-50,-10)
normIndices[,6]  <- normalise(indices[,6],  4.281792124, 25.57295061)  # Snr (0,50)
normIndices[,7]  <- normalise(indices[,7],  3.407526438, 7.653004384)  # AvSnrofActive Frames (3,10)
normIndices[,8]  <- normalise(indices[,8],  0.006581494, 0.453348819)  # Activity (0,1)
normIndices[,9]  <- normalise(indices[,9],  0, 2.691666667) # EventsPerSecond (0,2)
normIndices[,10] <- normalise(indices[,10], 0.015519804, 0.167782223)  # HighFreqCover (0,0.5)
normIndices[,11] <- normalise(indices[,11], 0.013522414, 0.197555718)  # MidFreqCover (0,0.5)
normIndices[,12] <- normalise(indices[,12], 0.01984127,  0.259381856)  # LowFreqCover (0,0.5)
normIndices[,13] <- normalise(indices[,13], 0.410954108, 0.501671845)  # AcousticComplexity (0.4,0.7)
normIndices[,14] <- normalise(indices[,14], 0.004326753, sqrt(0.155612175))  # TemporalEntropy (0,sqrt(0.3))
normIndices[,15] <- normalise(indices[,15], 0.02130969, 0.769678735)   # EntropyOfAverageSpectrum (0,0.7)
normIndices[,16] <- normalise(indices[,16], 0.098730903, 0.82144857)   # EntropyOfVarianceSpectrum (0,1)
normIndices[,17] <- normalise(indices[,17], 0.119538801, 0.998670805)  # EntropyOfPeaksSpectrum (0,1)
normIndices[,18] <- normalise(indices[,18], 0.004470594, 0.530948096)   # EntropyOfCoVSpectrum (0,0.7)
normIndices[,19] <- normalise(indices[,19], 0.043940755, 0.931257154)  # NDSI (-0.8,1)
normIndices[,20] <- normalise(indices[,20], 1.852187379, 11.79845141)    # SptDensity (0,15)

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
indicesRef <- c(5,7,9,10,11,12,13,17,18)
length1 <- 0
length2 <- length(normIndices$X)
dataFrame <- normIndices[length1:length2, indicesRef]  

dev.off()
#colourBlock <- read.csv("colours.csv", header = T)
colours <- c("red", "chocolate4", "palegreen", "darkblue",
             "brown1", "darkgoldenrod3", "cadetblue4", 
             "darkorchid", "orange" ,"darkseagreen", 
             "deeppink3", "darkslategrey", "firebrick2", 
             "gold2", "hotpink2", "blue", "maroon", 
             "mediumorchid4", "mediumslateblue","mistyrose4",
             "royalblue", "orange", "palevioletred2", 
             "sienna", "slateblue", "yellow", "tan2", 
             "salmon","violetred1","plum")

write.table(colours, file="colours.csv", row.names = F)

# Model-Based Clustering
library(mclust)
# The next line cannot be run on all 8 weeks because
# In double(ld) : Reached total allocation of 16289Mb: 
# see help(memory.size).  Running on one week takes 
# around 10 minutes
start <- 1
end <- 10076
fit <- Mclust(dataFrame[start:end,1:9], G=30)
mclust30list_9 <- unname(fit$classification)
write.table(mclust30list_9, file="mclust30list_9.csv", 
            row.names = F)
clusters <- read.csv(file="mclust30list_9.csv", header=T)

#plot(fit) # plot results 
#summary(fit) # display the best model
png(paste("Clusterplot_mclust", indices$rec.data[start],
          indices$rec.date[end],"5,7,9,11,12,13,17,18.png",
          sep="_"),
    width = 400, 
    height = 85, 
    units = "mm",
    res=1200,
    pointsize = 4)

#par(mar=c(3,5,3,3))
plot(fit$classification, col=colours[fit$classification],
     xaxt = 'n', xlab = "", ylab = "Cluster reference", cex.axis=2,
     cex.lab=1.5, main = paste(site, indices$rec.date[start],
     indices$rec.date[end], "(mclust)", sep = "_"),
     cex.main = 2)
axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
     cex.axis = 1.5, mgp = c(1.8, 0.5, 0))
axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
     tick = FALSE, cex.axis=1.5, mgp = c(4, 1.8, 0))
mtext(paste(indicesRef, collapse = ", ", sep = ""), side=3, 
      line = -0.1, cex = 1.5)
for (i in 1:length(list)) {
  abline(v=list[i], lwd=1.5, lty = 3)
}
dev.off()

#fit$parameters$mean
#mclust15List <- unname(fit$classification)
#mclust30list_9 <- unname(fit$classification)
#mclust23list_9 <- unname(fit$classification)
#mclust <- cbind(mclust15list_9, mclust23list_9, mclust30list_9)

