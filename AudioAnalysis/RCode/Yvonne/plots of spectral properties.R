##################################################################
#  2 July 2015
#  R version:  3.2.1 
#  This file plots the spectral properites from the Seewave package 
#  and saves a series of five png plots.  The csv file was created
#  in Calculate_Spectral_properties.R
#  
############################################################

setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")

spec.properties <- read.csv("Seewave_spectral_Properties.csv",header = T)

png(
  "Spec.prop.Q25&Q75.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
#par(mfcol = c(2, 1))
par(mar = c(3, 5, 1, 1))
plot(spec.properties$Q25, col="#E69F00", ylim=c(0,8000), 
     ylab='', cex.axis = 2)
par(new=TRUE)
plot(spec.properties$Q75, col="#56B4E9", ylim=c(0,8000), 
     ylab="", cex.axis = 2)
mtext(side=3, line=-4, "Q25 & Q75", 1.8, cex = 2.8)
dev.off()

png(
  "Spec.prop.tempEnt&mean.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
par(mfcol = c(2, 1))
par(mar = c(3, 5, 0, 1))
plot(spec.properties$temEntropy, col="#009E73", 
     ylab='', cex.axis = 2)
mtext(side=3, line=-4, "Temporal Entropy", 1.8, cex = 2.8)
plot(spec.properties$mean, col="#F0E442", 
     ylab='', cex.axis = 2)
mtext(side=3, line=-28, "Mean", 1.8, cex = 2.8)
dev.off()

png(
  "Spec.prop.IQR&sd.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
par(mfcol = c(2, 1))
par(mar = c(3, 5, 0, 1))
plot(spec.properties$IQR, col="#0072B2", 
     ylab='', cex.axis = 2)
mtext(side=3, line=-4, "Interquartile Range", 1.8, cex = 2.8)
plot(spec.properties$sd, col="#D55E00", 
     ylab='', cex.axis = 2)
mtext(side=3, line=-28, "Standard deviation", 1.8, cex = 2.8)
dev.off()

png(
  "Spec.prop.skewness&sfm.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
par(mfcol = c(2, 1))
par(mar = c(3, 5, 0, 1))
plot(spec.properties$skewness, col="#CC79A7", 
     ylab='', cex.axis = 2) 
mtext(side=3, line=-4, "Skewness", 1.8, cex = 2.8)
plot(spec.properties$sfm, col="#D55E00", 
     ylab='', cex.axis = 2) #spectral flatness
mtext(side=3, line=-28, "Spectral flatness", 1.8, cex = 2.8)
dev.off()

png(
  "Spec.prop.ZCR&sh.png",
  width     = 320,
  height    = 85,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
par(mfcol = c(2, 1))
par(mar = c(3, 5, 0, 1))
plot(spec.properties$ZCRate, col="#F0E442", 
     ylab='', cex.axis = 2)
mtext(side=3, line=-4, "Zero Crossing Rate", 1.8, cex = 2.8)
plot(spec.properties$sh, col="#0072B2", 
     ylab='', cex.axis = 2) # spectral entropy
mtext(side=3, line=-28, "Spectral Entropy", 1.8, cex = 2.8)
dev.off()