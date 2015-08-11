# 20 July 2015
# Calculates and plots kmeans quantisation error 

#  This file is #1 in the sequence:
#  1. Save_Summary_Indices_ as_csv_file.R
#  2. Plot_Towsey_Summary_Indices.R
#  3. Correlation_Matrix.R
#  4. Principal_Component_Analysis.R
# *5. Quantisation_error.R
#  6. kMeans_Clustering.R
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

#entropy_cov <- indices[,16]/indices[,15] # Entropy of the coefficient of variance
#entropy_cov <- normalise(entropy_cov, 0,25)

#for (i in 1:length(entropy_cov)) {
#  if (entropy_cov[i] > 1) {
#    entropy_cov[i] = 1
#  }
#  if(entropy_cov[i] < 0) {
#    entropy_cov[i] =0
#  }
#}

normIndices <- indices
# normalise variable columns
normIndices[,4]  <- normalise(indices[,4], -50,-10)    # AverageSignalAmplitude
normIndices[,5]  <- normalise(indices[,5], -50,-10)    # BackgroundNoise
normIndices[,6]  <- normalise(indices[,6],  0,  50)    # Snr
normIndices[,7]  <- normalise(indices[,7],  3,  20)    # AvSnrofActive Frames
normIndices[,8]  <- normalise(indices[,8],  0,  1)     # Activity 
normIndices[,9]  <- normalise(indices[,9],  0,  5)     # EventsPerSecond
normIndices[,10] <- normalise(indices[,10], 0,  0.5)   # HighFreqCover
normIndices[,11] <- normalise(indices[,11], 0,  0.5)   # MidFreqCover
normIndices[,12] <- normalise(indices[,12], 0,  0.5)   # LowFreqCover
normIndices[,13] <- normalise(indices[,13], 0.4,0.7)   # AcousticComplexity
normIndices[,14] <- normalise(indices[,14], 0,  0.6)   # TemporalEntropy
normIndices[,15] <- normalise(indices[,15], 0,  0.8)   # EntropyOfAverageSpectrum
normIndices[,16] <- normalise(indices[,16], 0,  1)     # EntropyOfVarianceSpectrum
normIndices[,17] <- normalise(indices[,17], 0,  1)     # EntropyOfPeaksSpectrum
normIndices[,18] <- normalise(indices[,18], 0,  0.7)   # EntropyOfCoVSpectrum
normIndices[,19] <- normalise(indices[,19], -0.8, 1)   # NDSI
normIndices[,20] <- normalise(indices[,20], 0, 22)     # SptDensity

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
#######################################################
png(
  paste("Quantisation error", site, startDate, ".png", 
        sep = "_"),
  width     = 250,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
######################################
#indicesRef <- c(5,9,11,13,14,15,17) 
#length1 <- 0
#length2 <- length(normIndices$X)
#dataFrame <- normIndices[length1:length2, indicesRef]  
##########################################
indicesRef <- c(5,7,9,10,11,13,14,15,17) 
reference <- "5,7,9,10,11,13,14,15,17"
length1 <- 0
length2 <- length(normIndices$X)
dataFrame <- normIndices[length1:length2, indicesRef]  
#########################################

par(cex.axis=1.2, cex = 2, mar=c(3, 3, 1, 0.2), oma=c(1, 1, 2, 1))
set.seed(1234)
length <- length(indices$rec.date)

#dataFrame <- cbind(dataFrame, entropy_cov)

# 21 to 50 centers
errorRatio <- NULL

for (i in 20:50) {
  kmeansObj <- kmeans(dataFrame, centers = i, iter.max = 50)
  #R <- 100 - (kmeansObj$betweenss*100/kmeansObj$totss)
  #errorRatio <- c(errorRatio, R) 
  W <- kmeansObj$tot.withinss
  errorRatio <- c(errorRatio, W)
}

par(mar=c(3,3,0,0))
plot (c(20:50), type = "l", errorRatio)


mtext(side = 3, paste("Quantisation error", site, startDate, 
                      ".png", sep = "_"), outer=TRUE, cex = 5)
mtext(side = 4, paste("Indices ", reference, sep = ","), outer =T,
      cex = 3)

dev.off()

png(
  paste("Relative drop", site, startDate, ".png", 
        sep = "_"),
  width     = 250,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

relativeDrop <- NULL
col1 <- c(21:50)

# The relative drop is calculated in the form of (b-a)/a
# The favourable numbers are the low on the plot
for (i in 1:length(errorRatio)) {
  drop <- ((errorRatio[i+1]) - (errorRatio[i]))/errorRatio[i]
  relativeDrop <- c(relativeDrop, drop)
}
relativeDrop <- cbind(col1, relativeDrop)

par(cex.axis=1.2, cex = 2, mar=c(3,3,1,0.2), oma=c(1,1,3,1))

plot(relativeDrop, xlab = "", cex.axis = 1.2, type ="l")

mtext(side = 3, paste("Relative drop", site, startDate, 
                      ".png", sep = "_"), outer=TRUE, cex = 5)
mtext(side = 4, paste("Indices ", reference, sep = ","), outer =T,
      cex = 3)
abline(h=0)

dev.off()