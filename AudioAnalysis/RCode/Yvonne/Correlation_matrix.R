# 3 July 2015
# Calculates the correleation matrix on the normalised indices
# Used in conjunction with Save_Summary_Indices_ as_csv_file.R

#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
setwd("C:\\Work\\CSV files\\2015Jul01-120417\\GympieNP\\")
#setwd("C:\\Work\\CSV files\\2015Jul01-120417\\Woondum3\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)
indices <- read.csv("Towsey_Summary_Indices 20150622_000000 to 20150628_064559 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150622_000000 to 20150628_133139 .csv", header=T)

name <- "Gympie NP 20150622_000000"
#name <- "Woondum3 20150622_000000"

# normalize values using minimum and maximum values
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

normIndices <- indices
# normalise variable columns
normIndices[,2]  <- normalise(indices[,2], 0, 2)      # HighAmplitudeIndex
normIndices[,3]  <- normalise(indices[,3],0,1)        # ClippingIndex
normIndices[,4]  <- normalise(indices[,4],-50,-10)    # AverageSignalAmplitude
normIndices[,5]  <- normalise(indices[,5], -50,-10)   # BackgroundNoise
normIndices[,6]  <- normalise(indices[,6], 0, 50)     # Snr
normIndices[,7]  <- normalise(indices[,7], 0,25)      # AvSnrofActive Frames
normIndices[,8]  <- normalise(indices[,8], 0,1)       # Activity 
normIndices[,9]  <- normalise(indices[,9],0,5)        # EventsPerSecond
normIndices[,10] <- normalise(indices[,10], 0, 0.5)   # HighFreqCover
normIndices[,11] <- normalise(indices[,11], 0,0.5)    # MidFreqCover
normIndices[,12] <- normalise(indices[,12], 0, 0.5)   # LowFreqCover
normIndices[,13] <- normalise(indices[,13], 0.4, 0.7) # AcousticComplexity
normIndices[,14] <- normalise(indices[,14], 0, 0.6)   # TemporalEntropy
normIndices[,15] <- normalise(indices[,15],0, 1)      # AvgEntropySpectrum
normIndices[,16] <- normalise(indices[,16], 0, 1)     # VarianceEntropySpectrum
normIndices[,17] <- normalise(indices[,17], 0,1)      # EntropyPeaks
normIndices[,18] <- normalise(indices[,18], 0, 25)    # SptDensity

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

# save the correlation matrix_name(location and date).csv in csv folder
a <- cor(normIndices[2:18][,unlist(lapply(indices[2:18], is.numeric))])
write.table(a)
write.table(a, file = paste("Correlation_matrix_",name,".csv",sep=""), sep = ",", 
            col.names = NA, qmethod = "double")