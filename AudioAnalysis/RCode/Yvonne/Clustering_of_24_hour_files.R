## 9 October 2015
# This code takes the 24 hour fingerprints and clusters these using
# hclust into 12 clusters (corresponding to 12 days) and saves
# the dendrograms
# This code was set up for Experiment 2 (publication)
# The files that this code clusters is generated in the code 
# Histograms_of_cluster.lists.R
#metrics <- read.csv("clustering_metrics.csv", header = T)
myFiles <- list.files(full.names=TRUE, pattern="*_24hour_112days.csv$")
myFilesShort <- list.files(full.names=FALSE, pattern="*_24hour_112days.csv$")

length <- length(myFiles)
length
site <- c(rep("GympieNP",6), rep("WoondumNP",6))
dates <- unique(indices$rec.date)
#dates2 <- rep(dates, each=6)
dates <- rep(dates, 2)
# Read file contents of Summary Indices and collate
numberCol <- NULL
heights <- NULL
for (i in 1:length(myFilesShort)) {
  Name <- myFiles[i]
  assign("fileContents", read.csv(Name))
  numberCol <- ncol(fileContents)
  numberCol <- numberCol-2
  dat <- fileContents[,1:numberCol]
  #c <- cor(dat)
  hc.fit <- hclust(dist(dat), method = "ward.D2")
  png(paste(myFilesShort[i],"Method wardD2.png", sep = ""), width=1100,
      height =1000)
  par(oma=c(7,3,3,3))
  plot(hc.fit, cex=2, main = paste(myFilesShort[i]), sub="", xlab = "hclust(method = ward.D2)",
       xaxt="n", yaxt = "n", cex.lab=1.5, ylab="", cex.main=1.5)
  mtext(side = 2, "Height",cex = 1.5, line = -2)
  heightss <- hc.fit$height
  axis(side = 4, at=c(round(heightss[2],0),round(heightss[4],0),round(heightss[6],0),
                      round(heightss[8],0),round(heightss[10],0)), 
                      las=1, cex.axis=1.5)
  axis(side = 2, at=c(round(heightss[1],0),round(heightss[3],0),round(heightss[5],0),
                      round(heightss[7],0),round(heightss[9],0),round(heightss[11],0)), 
                      las=1, cex.axis=1.5)
  mtext(side = 1, line = 5.5, adj=1, cex=1.3, paste("1,2,3", site[1], dates[1], 
                                                  dates[2], dates[3], "4,5,6", site[1], dates[4], 
                                                  dates[5], dates[6], sep = "    ")) 
  mtext(side = 1, line = 7, adj=1, cex=1.3, paste("7,8,9", site[7], dates[1], 
                                                  dates[2], dates[3], "10,11,12", site[7], dates[4], 
                                                  dates[5], dates[6], sep = "    "))
  mtext(side = 1, line = 8.5, adj=1, cex=1.2, expression(italic(Twelve ~days ~from ~2 ~x ~112 ~days ~of ~clustering)))
  mtext(side = 1, line = 10, adj=1, cex=1.2, expression(italic(Indices:~BackgroundNoise ~Snr ~EventsPerSecond ~LowFreqCover ~AcousticComplexity ~EntropyOfPeaksSpectrum ~EntropyOfCoVSpectrum)))
  mtext(side = 3, line = -0.8, cex=1.5, paste("heights: ", round(heightss[11],0),
        round(heightss[10],0), round(heightss[9],0), round(heightss[8],0),
        round(heightss[7],0), round(heightss[6],0), round(heightss[5],0), 
        round(heightss[4],0), round(heightss[3],0), round(heightss[2],0), 
        round(heightss[1],0), sep = ", "))
  heights <- rbind(heights, heightss)
  dev.off()
}

#heights <- cbind(myFilesShort, heights)
#write.csv(heights, "heights_test_ward_D2.csv")
#sort(unique(cophenetic(hc.fit)))

# Plotting the metrics
png("Clustering Metrics.png", width=1500, height =1000)
x <- c(10000, 15000, 20000, 25000, 30000, 35000)
plot(x,metrics$k15, type="l", ylim=c(1.3,2.1), col="red",
     ylab = "intergroup dissimilarity", xlab = "kmeans k value (k1)", 
     cex.axis=1.5, main="Clustering metrics - 112 days x 2 sites", cex.main=2, cex.lab=1.5)
par(new=TRUE)
plot(x,metrics$k20, type="l", ylim=c(1.3,2.1), col="orange", yaxt='n',
     ylab = "", xlab = "", xaxt='n')
par(new=TRUE)
plot(x,metrics$k25, type="l", ylim=c(1.3,2.1), col="yellow", yaxt='n',
     ylab = "", xlab = "", xaxt='n')
par(new=TRUE)
plot(x,metrics$k30, type="l", ylim=c(1.3,2.1), col="green", yaxt='n',
     ylab = "", xlab = "", xaxt='n')
par(new=TRUE)
plot(x, metrics$k35, type="l", ylim=c(1.3,2.1), col="blue", yaxt='n',
     ylab = "", xlab = "", xaxt='n')
par(new=TRUE)
plot(x, metrics$k40, type="l", ylim=c(1.3,2.1), col="violet", yaxt='n',
     ylab = "", xlab = "", xaxt='n')
k2 <- c("k2_15","k2_20","k2_25","k2_30","k2_35","k2_40")
legend('topright', k2, 
       lty=1, col=c('red', 'orange', 'yellow',' green','blue','violet'), 
       bty='n', cex=2)
dev.off()
