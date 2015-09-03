# Concatenating csv files aswell as calculation of percentiles
# of each index

setwd("C:\\Work\\CSV files\\GympieNP1_new\\all_data")

folder <- "C:\\Work\\CSV files\\GympieNP1_new\\"

myFolders <- list.files(full.names=FALSE, pattern="2015_*", 
                        path=folder)
myFolders
length <- length(myFolders)
length

all.indices <- NULL

for (i in 1:length) {
  pathName <- (paste(folder, myFolders[i], sep=""))
  myFile <- list.files(full.names=TRUE, pattern="Towsey_Summary_Indices*", 
                       path=pathName)
  assign(paste("fileContents"), read.csv(myFile, header=T))
  all.indices <- rbind(all.indices, fileContents)
}
site <- all.indices$site[1]
dates <- all.indices$rec.date


write.csv(all.indices, row.names = F,
          file=paste("Towsey_Summary_Indices_", site,
                     dates[1],"to current.csv", 
                     sep = ""))

quantiles1 <- NULL
quantiles2 <- NULL
minimum <- NULL
average <- NULL
maximum <- NULL

for (i in 4:20) {
  quant1 <- unname(quantile(all.indices[,i], probs = c(2,5)/100))
  quant2 <- unname(quantile(all.indices[,i], probs = c(95,98)/100))
  quantiles1 <- rbind(quantiles1, quant1)
  quantiles2 <- rbind(quantiles2, quant2)
  minim <- min(all.indices[,i])
  minimum <- c(minimum, minim)
  avg <- mean(all.indices[,i])
  average <- c(average, avg)
  maxim <- max(all.indices[,i])
  maximum <- c(maximum, maxim)
}

colNames <- colnames(all.indices[,4:20])
quantiles <- cbind(minimum, quantiles1, average, quantiles2, maximum, colNames)
colnames(quantiles) <- c("min","2%","5%","mean","95%","98%","max","index")

write.csv(quantiles, row.names = FALSE,
          file=paste("Percentiles_", site,
                     dates[1],
                     "to current.csv", 
                     sep = ""))

View(quantiles)

# save boxplots
png(
  paste("Boxplots_of_Indices_",site,dates[1],
        dates[length(all.indices$rec.date)],
        ".png", sep=""),
  width     = 500,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
require(graphics)
par(mfrow=c(5,4), mar=c(3,1,7,1), oma=c(2,2,2,2),
    cex.main=4, cex.axis=3, mgp = c(3,-1.2, 0))
for(i in 1:17) {
  boxplot(all.indices[,i+3], horizontal = T,
          main = colNames[i], col = "yellow")
  lines (density(all.indices[,i+3]))
  abline(v=(quantiles[i,c(2,6)]), lty=2, col="red")
}
plot(2, axes=F,xlim=c(0,1), ylim=c(0,1), xlab = "",
     ylab = "")
legend("center", c(paste(dates[1]), paste(dates[length(dates)]),
                   paste(length(all.indices$X),"minutes",sep=" "),
                   "2nd & 98th percentiles"),
       pch = c("-","-","-","----"), bty = "n",
       col = c("black","black","black","red"),
       title = paste(site), cex = 4)
dev.off()

#########################################
library(vioplot)
# save violin plots
png(
  paste("Violinplots_of_Indices_",site,dates[1],
        dates[length(all.indices$rec.date)],
        ".png", sep=""),
  width     = 500,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

par(mfrow=c(5,4), mar=c(3,7,3,1), oma=c(5,5,5,5),
    cex.main=4, cex.axis=3)
for(i in 1:17) {
  vioplot(all.indices[,i+3], names = colNames[i], 
          col="gold", horizontal = T)
  abline(v=(quantiles[i,c(2,6)]), lty=2, col="red")
}
plot(2, axes=F,xlim=c(0,1), ylim=c(0,1), xlab = "",
     ylab = "")
legend("center", c(paste(dates[1]), paste(dates[length(dates)]),
                   paste(length(all.indices$X),"minutes",sep=" "),
                   "2nd & 98th percentiles"),
       pch = c("-","-","-","----"),
       col = c("black","black","black","red"),
       title = paste(site), cex = 4, bty="n")
dev.off()

# save density plots
png(
  paste("Densityplots_of_Indices_",site,dates[1],
        dates[length(all.indices$rec.date)],
        ".png", sep=""),
  width     = 500,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
library(lattice)
colNames <- colnames(all.indices[,4:20])
densityplot(~AvgSignalAmplitude|BackgroundNoise|Snr|AvgSnrOfActiveFrames|
              Activity|EventsPerSecond|HighFreqCover|MidFreqCover|LowFreqCover|
              AcousticComplexity|TemporalEntropy|EntropyOfAverageSpectrum|
              EntropyOfVarianceSpectrum|EntropyOfPeaksSpectrum|EntropyOfCoVSpectrum|
              NDSI|SptDensity, main=paste(site,dates,sep = " "),xlab="",
              layout(5,4))
plot(2, axes=F,xlim=c(0,1), ylim=c(0,1), xlab = "",
     ylab = "")
legend("center", c(paste(dates[1]), paste(dates[length(dates)]),
                   paste(length(all.indices$X),"minutes",sep=" "),
                   "2nd & 98th percentiles"),
                   pch = c("-","-","-","----"),
                   col = c("black","black","black","red"),
                  title = paste(site), cex = 4, bty="n")
dev.off()