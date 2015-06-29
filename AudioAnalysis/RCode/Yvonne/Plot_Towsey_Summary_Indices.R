##################################################################
#  26 June 2015
#  R version:  3.2.1 
#  This file plots the indices in the csv file created by the code 
#  Save_Summary_Indices_ as_csv_file.R

########## You may wish to change these ###########################
#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)

#Sensor <- read.csv("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\Sensorfile\\Sensor file 15_March 2015 to 20_March_2015.csv",header = TRUE)
Sensor <- read.csv("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\Sensorfile\\Sensor file 22_March 2015 to 27_March_2015.csv",header = TRUE)

# Also check the "Set up time and date positons and labels"

###### Determine number of days and minutes per day in recording ##########

number.of.days <- length(unique(indices$rec.date))
min.per.day <- table(indices$rec.date)
counter <- NULL
for (i in 1:number.of.days) {
  no.min.per.day <- c(min.per.day[[i]])
  counter <- c(counter, no.min.per.day)
}

######## Create day identification for different colours in plot #############

day <- NULL
for (i in 1:number.of.days) {
  id <- rep(paste(LETTERS[i]), counter[i])
  day <- c(day, id)
}

indices <- cbind(indices, day)

########### Set up time and date positons and labels #############  

n <- 60 ## Change this figure to match length of the files 
h <- 6  ## Sets the plot hour interval
timePos <- seq(379, nrow(indices), by = ((n-1)*h)) # 59 minutes per hour
timeLabel <- substr(indices$rec.time, 1,5) 
timeLabel <- timeLabel[seq(379 ,nrow(indices), by = ((n-1)*h))]
datePos <- seq(-60, nrow(indices), by = 1440-1440/n) # 59 minutes per hour per day
dateLabel <- unique(substr(indices$rec.date, 1,10))
rm(h,n)

########## Plot indices function ###############

plot.indices <- function(file, heading, label) {
  par(mar=c(3.1, 3.6, 3.6, 3.6), cex.axis = 0.7, cex = 3)
  plot(file, xaxt = 'n', xlab = "", type = 'p',
       cex.lab=0.9, col=indices$day, mgp = c(1.8,0.5,0), 
       main = heading, cex = 0.2, ylab = label, cex.main = 1.15)
  axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8,0.5,0), 
       cex.axis = 0.7)
  axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4,1.8,0),
       tick = FALSE)
  par(new=TRUE) # allows another plot to be printed
  plot(Sensor$Temperature, type = 'l', xaxt = 'n', yaxt = 'n', 
       ylab = "")
  axis(4, ylim=c(min(Sensor$Temperature), max(Sensor$Temperature)),
       lwd=1.8, line = line, mgp = c(3,0.4,0))
  mtext(side=4, "Temperature (C)", 1.8, cex = 2.8)
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

file<-indices$BackgroundNoise
plot.indices(file,"Background Noise", "Background Noise (dB)")

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
plot.indices(file,"Average Signal Amplitude", "Average Signal Amplitude (dB)")

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
plot.indices(file,"Signal to Noise ratio", "Signal to noise ratio")

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
plot.indices(file,"Average Signal to Noise of Active Frames", "Average Signal to Noise of Active Frames")

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

######## Print Average Entropy Spectrum plot ###############

png(
  "AverageEntropySpectrum.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$AvgEntropySpectrum
plot.indices(file,"Average Entropy Spectrum", "Average Entropy Spectrum")

dev.off()

######## Print Variance Entropy Spectrum plot ###############

png(
  "VarianceEntropySpectrum.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$VarianceEntropySpectrum
plot.indices(file,"Variance Entropy Spectrum", "Variance Entropy Spectrum")

dev.off()

######## Print Entropy Peaks plot ###############

png(
  "EntropyPeaks.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$EntropyPeaks
plot.indices(file,"Entropy Peaks", "Entropy Peaks")

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

######## Print Rain Index plot ###############

png(
  "RainIndex.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$RainIndex
plot.indices(file,"Rain Index", "Rain Index")

dev.off()

######## Print Cicada Index plot ###############

png(
  "CicadaIndex.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

file <- indices$CicadaIndex
plot.indices(file,"Cicada Index", "Cicada Index")

dev.off()

####################################################
