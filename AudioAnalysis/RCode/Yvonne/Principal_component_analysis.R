#  Date: 17 July 2015
#  R version:  3.2.1 
#  This file calculates the Principal Component Analysis and plots the
#  result
#
########## You may wish to change these ###########################
#setwd("C:\\Work\\CSV files\\Woondum1\\2015_03_15\\")
#setwd("C:\\Work\\CSV files\\Woondum2\\2015_03_22\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_07_05\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_07_05\\")

#indices <- read.csv("Towsey_Summary_Indices_Woondum1 20150315_133427to20150320_153429.csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum2 20150322_113743to20150327_103745.csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv",header = T)

# Gympie NP1 22_06_15
#xlim <- c(-0.015, 0.035)
#ylim = c(-0.017,0.0001)
# Gympie NP1 28_06_15
#xlim <- c(-0.015, 0.028)
#ylim = c(-0.008,0.0001)
# Woondum3 21_06_15
#xlim <- c(-0.017, 0.02)
#ylim = c(-0.008,0.01)
# Woondum3 28_06_15
xlim <- c(-0.017, 0.02)
ylim = c(-0.001,0.009)

site <- indices$site[1]
date <- indices$rec.date[1]
################ Normalise data ####################################
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
normIndices[,15] <- normalise(indices[,15], 0, 0.8)     # AvgEntropySpectrum
normIndices[,16] <- normalise(indices[,16], 0, 1)     # VarianceEntropySpectrum
normIndices[,17] <- normalise(indices[,17], 0, 1)     # EntropyPeaks
normIndices[,18] <- normalise(indices[,18], 0, 22)    # SptDensity

# adjust values greater than 1 or less than 0
for (j in 2:18){
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
######### PCA biplot #####################################
file <- paste("Principal Component Analysis_", site, 
              "_", date, ".png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

par(mar =c(2,2,4,2), cex.axis = 2.5)
PCAofIndices<- prcomp(normIndices[ ,4:18])
biplot(PCAofIndices, col=c("transparent", "red"), 
       cex=c(0.08,2.5), ylim = ylim, 
       xlim = xlim)
mtext(side = 3, line = 2, paste("Principal Component Analysis prcomp ",
              site, date, sep = " "), cex = 2.5)
PCAofIndices$center

dev.off()
#################################################