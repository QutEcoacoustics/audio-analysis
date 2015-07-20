# 20 July 2015
# Calculates and plots kmeans quantisation error 

#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv",header = T)

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
#######################################################
png(
  paste("Quantisation error", site, startDate, ".png", 
        sep = "_"),
  width     = 400,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

par(mfrow = c(1,3), cex.axis=1.2, cex = 2,
    mar=c(3,3,1,0.2), oma=c(1,1,3,1))

# 21 to 40 centers
errorRatio <- NULL

for (i in 21:40) {
  kmeansObj <- kmeans(dataFrame, centers = i, iter.max = 20)
  R <- 100 - (kmeansObj$betweenss*100/kmeansObj$totss)
  errorRatio <- c(errorRatio, R) 
}

plot (c(21:40), errorRatio, ylim= c(13,19))

# 41 to 60 centers
errorRatio <- NULL

for (i in 41:60) {
  kmeansObj <- kmeans(dataFrame, centers = i, iter.max = 20)
  R <- 100 - (kmeansObj$betweenss*100/kmeansObj$totss)
  errorRatio <- c(errorRatio, R) 
}

plot (c(41:60), errorRatio, ylim= c(8,14))

# 61 to 80 centers
errorRatio <- NULL

for (i in 61:80) {
  kmeansObj <- kmeans(dataFrame, centers = i, iter.max = 50)
  R <- 100 - (kmeansObj$betweenss*100/kmeansObj$totss)
  errorRatio <- c(errorRatio, R) 
}

plot (c(61:80), errorRatio, ylim= c(8,14))
mtext(side = 3, paste("Quantisation error", site, startDate, 
                      ".png", sep = "_"), outer=TRUE, cex = 5)

dev.off()