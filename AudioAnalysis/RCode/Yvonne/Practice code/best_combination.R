###########################################################################
# 10 July 2015
# This code will take more than 20 minutes to run - the number of combinations
# are listed below:
# n = 4 or 13 (2380) combinations         # n = 5 or 12 (6188)
# n = 6 or 11 (12376)                     # n = 7 or 10 (19448)
# n = 8 or 9 (24310).
# This code determines the best combination of variables for a set number 
# of variables and prints out the kmeansObj$betweenss/kmeansObj$totss value

### Please set the following values ######################################

n = 8 # number of variables
k = 30 # number of cluster centrers (k value in kmeans)

##########################################################################
#setwd("C:\\Work\\CSV files\\Woondum1\\2015_03_15\\")
#setwd("C:\\Work\\CSV files\\Woondum2\\2015_03_22\\")
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_07_05\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_07_05\\")

#indices <- read.csv("Towsey_Summary_Indices_Woondum1 20150315_133427to20150320_153429.csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum2 20150322_113743to20150327_103745.csv", header=T)
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv",header = T)

date <- "20150622"   #indices$rec.date[1]
site <- indices$site[1]
date <- indices$rec.date[1]

normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

normIndices <- indices
# normalise variable columns
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
for (j in 4:18) {
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

################################################
library(caTools)
combinations <- combs(4:18, n) 

ratio <- NULL
library(stats)
for (i in 1:(length(combinations)/n)) {
    set.seed(1364)
    dataFrame <- normIndices[,c(combinations[i,c(1:n)])]
    print(paste("starting", i, sep = ""))
    kmeansObj <- kmeans(dataFrame, centers=k)
    r <- kmeansObj$betweenss/kmeansObj$totss
    ratio <- c(ratio, r)  
}

c <- cbind(combinations, ratio)
best <- c[(which.max(c[,(n+1)])),1:(n+1)]
a <- paste("Number of combinations", (length(combinations)/n), sep = " ")
b <- paste("The best combination is", best)
write.table(c, file = paste("Best_combinations_", site, date, "_", n, "_", k, ".csv",
              sep=""), sep = ",", col.names = NA, qmethod = "double")