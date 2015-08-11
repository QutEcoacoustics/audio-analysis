#  Date: 26 June 2015
#  R version:  3.2.1 
#  This file plots the indices in the csv file created by the code 

#  This file is #2 in the sequence:
#  1. Save_Summary_Indices_ as_csv_file.R
# *2. Plot_Towsey_Summary_Indices.R
#  3. Correlation_Matrix.R
#  4. Principal_Component_Analysis.R
#  5. kMeans_Clustering.R
#  6. Quantisation_error.R
#  7. Distance_matrix.R
#  8. Minimising_error.R
#  9. Segmenting_image.R

########## You may wish to change these ###########################
#setwd("C:\\Work\\CSV files\\Woondum1\\2015_03_15\\")
#setwd("C:\\Work\\CSV files\\Woondum2\\2015_03_22\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_07_05\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_07_05\\")

#indices <- read.csv("Towsey_Summary_Indices_Woondum1 20150315_133427to20150320_153429.csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum2 20150322_113743to20150327_103745.csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv", header = T)

#Sensor <- read.csv("Sensor file_Woondum1_2015_03_15.csv", header = TRUE)
#Sensor <- read.csv("Sensor file_Woondum2_2015_03_22.csv", header = TRUE)
Sensor <- read.csv(paste(".", "\\", "Sensor file_Gympie NP1_2015_06_21.csv", sep = ""), header = T)
#Sensor <- read.csv(paste(".", "\\", "Sensor file_Woondum3_2015_06_21.csv", sep = ""), header = T)
#Sensor <- read.csv(paste(".", "\\", "Sensor file_Gympie NP1_2015_06_28.csv", sep = ""), header = T)
#Sensor <- read.csv(paste(".", "\\", "Sensor file_Woondum3_2015_06_28.csv", sep = ""), header = T)
#Sensor <- read.csv(paste(".", "\\", "Sensor file_Gympie NP1_2015_07_05.csv", sep = ""), header = T)
#Sensor <- read.csv(paste(".", "\\", "Sensor file_Woondum3_2015_07_05.csv", sep = ""), header = T)

site <- indices$site[1]

###### Create a list of the number of minutes per day used to plot colours ##########
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

########## Plot indices function ###############

plot.indices <- function(file, heading, label) {
  par(mar = c(3.1, 3.6, 3.6, 3.6), cex.axis = 0.7, cex = 3)
  plot(file, xaxt = 'n', xlab = "", type = 'p',
       cex.lab = 0.9, col = indices$day, mgp = c(1.8, 0.5, 0), 
       main = heading, cex = 0.2, ylab = label, cex.main = 1.15)
  axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
       cex.axis = 0.7)
  axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
       tick = FALSE)
  par(new=TRUE) # allows another plot to be printed
  plot(Sensor$Temperature, type = 'l', xaxt = 'n', yaxt = 'n',
       ylab = "", ylim=c(min(Sensor$Temperature), max(Sensor$Temperature)))
  axis(side = 4, ylim=c(min(Sensor$Temperature), max(Sensor$Temperature)),
       line = line, mgp = c(3,0.4,0))
  mtext(side=4, "Temperature (C)", 1.8, cex = 2.8)
  mtext(side = 3, paste(site,"corrected mic", indices$rec.date[1], "to",
       indices$rec.date[length(indices$rec.date)], sep =" "), cex = 2)
  mtext(side = 3, line = -1,"_______ Temperature     ", cex=2, adj=1)
}

######## Print Background Noise plot ###############

png(
  "BackgroundNoise.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$BackgroundNoise
plot.indices(file, "Background Noise", "Background Noise (dB)")

dev.off()

######## Print Average Signal Amplitude plot ###############

png(
  "AverageSignalAmplitude.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$AvgSignalAmplitude
plot.indices(file, "Average Signal Amplitude", "Average Signal Amplitude (dB)")

dev.off()

######## Print Signal to Noise plot ###############

png(
  "SignalToNoise.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$Snr
plot.indices(file, "Signal to Noise ratio", "Signal to noise ratio")

dev.off()

######## Print Average Signal To Noise of Active Frames plot ###############

png(
  "AverageSignalToNoiseOfActiveFrames.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$AvgSnrOfActiveFrames
plot.indices(file, "Average Signal to Noise of Active Frames", "Average Signal to Noise of Active Frames")

dev.off()

######## Print Activity plot ###############

png(
  "Activity.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$Activity
plot.indices(file,"Activity", "Activity")

dev.off()

######## Print Events per Second plot ###############

png(
  "Eventspersecond.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$EventsPerSecond
plot.indices(file,"Events per second", "Events per second")

dev.off()

######## Print High Frequency Cover plot ###############

png(
  "HighFrequencyCover.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$HighFreqCover
plot.indices(file, "High Frequency Cover", "High Frequency Cover")

dev.off()

######## Print Mid Frequency Cover plot ###############

png(
  "MidFrequencyCover.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$MidFreqCover
plot.indices(file,"Mid Frequency Cover", "Mid Frequency Cover")

dev.off()

######## Print Low Frequency Cover plot ###############

png(
  "LowFrequencyCover.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$LowFreqCover
plot.indices(file,"Low Frequency Cover", "Low Frequency Cover")

dev.off()

######## Print Acoustic Complexity plot ###############

png(
  "Acoustic complexity.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$AcousticComplexity
plot.indices(file,"Acoustic Complexity", "Acoustic Complexity")

dev.off()

######## Print Temporal Entropy plot ###############

png(
  "TemporalEntropy.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$TemporalEntropy
plot.indices(file,"Temporal Entropy", "Temporal Entropy")

dev.off()

######## Print Entropy of Average Spectrum plot ###############

png(
  "EntropyOfAverageSpectrum.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$EntropyOfAverageSpectrum
plot.indices(file,"Entropy of Average Spectrum", "Entropy of Average Spectrum")

dev.off()

######## Print Normalised Difference Soundscape Index plot ###############
png(
  "NormalizedDifferenceSoundscapeIndex.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$NDSI
plot.indices(file,"Normalised Difference Soundscape Index", 
             "Normalised Difference Soundscape Index")

dev.off()

######## Print Entropy of Peaks Spectrum plot ###############

png(
  "EntropyOfPeaksSpectrum.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$EntropyOfPeaksSpectrum
plot.indices(file,"Entropy of Peaks Spectrum", "Entropy Peaks")

dev.off()

######## Print Entropy of Variance Spectrum plot ###############

indices$EntropyOfCoVSpectrum

png(
  "EntropyOfVarianceSpectrum.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$EntropyOfVarianceSpectrum
plot.indices(file,"Entropy of Variance Spectrum", "Entropy of Variance Spectrum")

dev.off()

######## Print Entropy of Coefficient of Variance Spectrum plot ###############
png(
  "EntropyOfCoefficientOfVarianceSpectrum.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$EntropyOfCoVSpectrum
plot.indices(file,"Entropy of Coefficient of Variance Spectrum", "Entropy of Coefficient of Variance Spectrum")

dev.off()

######## Print Spectral Density plot ###############

png(
  "SpectralDensity.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$SptDensity
plot.indices(file,"Spectral Density", "Spectral Density")

dev.off()