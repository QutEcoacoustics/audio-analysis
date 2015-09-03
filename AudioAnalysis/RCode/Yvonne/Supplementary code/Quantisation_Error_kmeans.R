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

setwd("C:\\Work\\CSV files\\GympieNP1_new\\all_data")
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")
indices <- read.csv("C:\\Work\\CSV files\\GympieNP1_new\\all_data\\Towsey_Summary_Indices_Gympie NP1 22-06-2015to current.csv",
                    header = T)

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
#normIndices[,2]  <- normalise(indices[,2],  0,  2)    # HighAmplitudeIndex (0,2)
#normIndices[,3]  <- normalise(indices[,3],  0,  1)    # ClippingIndex (0,1)
#normIndices[,4]  <- normalise(indices[,4], -44.34849276,-27.1750784)   # AverageSignalAmplitude (-50,-10)
normIndices[,5]  <- normalise(indices[,5], -45.06046874,-29.52071375)  # BackgroundNoise (-50,-10)
#normIndices[,6]  <- normalise(indices[,6],  4.281792124, 25.57295061)  # Snr (0,50)
normIndices[,7]  <- normalise(indices[,7],  3.407526438, 7.653004384)  # AvSnrofActive Frames (3,10)
#normIndices[,8]  <- normalise(indices[,8],  0.006581494, 0.453348819)  # Activity (0,1)
normIndices[,9]  <- normalise(indices[,9],  0, 2.691666667)     # EventsPerSecond (0,2)
normIndices[,10] <- normalise(indices[,10], 0.015519804, 0.167782223)  # HighFreqCover (0,0.5)
normIndices[,11] <- normalise(indices[,11], 0.013522414, 0.197555718)  # MidFreqCover (0,0.5)
normIndices[,12] <- normalise(indices[,12], 0.01984127,  0.259381856)  # LowFreqCover (0,0.5)
normIndices[,13] <- normalise(indices[,13], 0.410954108, 0.501671845)  # AcousticComplexity (0.4,0.7)
#normIndices[,14] <- normalise(indices[,14], 0.004326753, sqrt(0.155612175))  # TemporalEntropy (0,sqrt(0.3))
#normIndices[,15] <- normalise(indices[,15], 0.02130969, 0.769678735)   # EntropyOfAverageSpectrum (0,0.7)
#normIndices[,16] <- normalise(indices[,16], 0.098730903, 0.82144857)   # EntropyOfVarianceSpectrum (0,1)
normIndices[,17] <- normalise(indices[,17], 0.119538801, 0.998670805)  # EntropyOfPeaksSpectrum (0,1)
normIndices[,18] <- normalise(indices[,18], 0.004470594, 0.530948096)   # EntropyOfCoVSpectrum (0,0.7)
#normIndices[,19] <- normalise(indices[,19], 0.043940755, 0.931257154)  # NDSI (-0.8,1)
#normIndices[,20] <- normalise(indices[,20], 1.852187379, 11.79845141)    # SptDensity (0,15)

# adjust values greater than 1 or less than 0
for (j in c(5,7,9,11,12,13,17,18)) {
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
indicesRef <- c(5,7,9,11,12,13,17,18) 
reference <- "5,7,9,11,12,13,17,18"
length1 <- 0
length2 <- length(normIndices$X)
dataFrame <- normIndices[length1:length2, indicesRef]
rm(normIndices)
#########################################
par(cex.axis=1.2, cex = 2, mar=c(3, 3, 1, 0.2), oma=c(1, 1, 2, 1))
set.seed(100)
length <- length(indices$rec.date)
rm(indices)
# 10 to 30 centers
errorRatio <- NULL

for (i in 10:30) {
  kmeansObj <- kmeans(dataFrame, centers = i, iter.max = 200)
  W <- kmeansObj$tot.withinss
  errorRatio <- c(errorRatio, W)
}

png(
  paste("QuantisationError_RelativeDrop_5,7,9,11,12,13,17,18_seed_100", site, 
        startDate, ".png", sep = "_"),
  width     = 250,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

relativeDrop <- NULL
col1 <- c(10:30)

# The relative drop is calculated in the form of (b-a)/a
# The favourable numbers are the low on the plot
for (i in 1:length(errorRatio)) {
  drop <- ((errorRatio[i+1]) - (errorRatio[i]))/errorRatio[i]
  relativeDrop <- c(relativeDrop, drop)
}
relativeDrop <- cbind(col1, relativeDrop)

par(cex.axis = 1.2, cex = 2, mar=c(3,3,1,3), oma=c(1,1,3,2))

plot (c(10:30), type = "l", errorRatio, axes = T)
par(new=TRUE)
plot(relativeDrop, xlab = "", cex.axis = 1.2, type ="l", 
     lty=2, col="red", axes = F)
axis(4, col = "red", col.axis = "red", lwd = 0.5)

mtext(side = 3, paste(site, "seed100", startDate, 
                      ".png", sep = "_"), outer=TRUE, cex = 3)
mtext(side = 4, paste("Indices ", reference, sep = ","), outer =T,
      cex = 3)
abline(h=0)
colours <- c(1,2)
annotation <- c("quantisation error","relative drop")
legend('topright', annotation ,lty = c(1,2), col=colours, bty='n', 
       cex=1.4)
dev.off()