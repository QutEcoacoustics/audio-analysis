# 29 July 2015 
#
# Minimising the error in kmeans clustering - determining which 
# set.seed gives the lowest error

setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")

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
normIndices[,2]  <- normalise(indices[,2],0,2)  # HighAmplitudeIndex (0,2)
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
for (j in 4:20){
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
indicesRef <- c(5,7,9,10,11,13,14,15,17,18)
length1 <- 0
length2 <- length(normIndices$X)
dataFrame <- normIndices[length1:length2, indicesRef]  
#############################################

#indicesRef <- c(5,7,9,10,11,12,13,14,15,17) 
#dataFrame <- normIndices[1:length(normIndices$X),indicesRef]

errorRatio <- NULL
errorList <- NULL
withinSS <- NULL

for (i in 1:2000) {
  set.seed(i)
  kmeansObj <- kmeans(dataFrame, centers = 35, iter.max = 500, 
                      nstart = 1)
  R <- 100 - (kmeansObj$betweenss*100/kmeansObj$totss)
  errorRatio <- c(errorRatio,R)
  with <- kmeansObj$tot.withinss
  withinSS <- c(withinSS, with)
  print(paste(i))
}

ref <- 1:6000
errorList <- cbind(ref, errorRatio, withinSS)
min(errorRatio)

write.csv(errorList,(file=paste("Minimum_error_indices_5,7,9,10,11,13,14,15,17,18", 
                      site, ".csv", sep = "")))