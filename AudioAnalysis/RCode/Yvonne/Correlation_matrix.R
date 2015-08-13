# 3 July 2015
# Calculates the correleation matrix on the normalised indices

#  This file is #3 in the sequence:
#  1. Save_Summary_Indices_ as_csv_file.R
#  2. Plot_Towsey_Summary_Indices.R
# *3. Correlation_Matrix.R
#  4. Principal_Component_Analysis.R
#  5. Quantisation_error.R
#  6. kMeans_Clustering.R
#  7. Distance_matrix.R
#  8. Minimising_error.R
#  9. Segmenting_image.R
# 10. Transition Matrix 
# 11. Cluster time series

#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv",header = T)
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv", header = T)

site <- indices$site[1]
date <- indices$rec.date[1]

# normalize values using minimum and maximum values
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
# Pre-processing of Temporal Entropy
# to correct the long tail 
indices[,14] <- sqrt(indices[,14])

normIndices <- indices
# normalise variable columns
normIndices[,2]  <- normalise(indices[,2],  0,  2)     # HighAmplitudeIndex
normIndices[,3]  <- normalise(indices[,3],  0,  1)     # ClippingIndex
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
normIndices[,18] <- normalise(indices[,18], 0, 0.7)   # EntropyOfCoVSpectrum
normIndices[,19] <- normalise(indices[,19], -0.8, 1)   # NDSI
normIndices[,20] <- normalise(indices[,20], 0, 15)     # SptDensity

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

# save the correlation matrix_name(location and date).csv in csv folder
a <- cor(normIndices[,4:20][,unlist(lapply(indices[,4:20], is.numeric))])
write.table(a, file = paste("Correlation_matrix_adj_range",site, "_", date,
                            ".csv",sep=""), sep = ",", col.names = NA, 
                            qmethod = "double")