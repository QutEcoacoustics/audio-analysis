# Cluster Volatility
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
site <- "GympieNP"
date <- "22-28 June 2015"
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv", header=T)

clusterList <- read.csv("Cluster_list 22-28 June 2015_5,7,9,10,11,12,13,14,15,17,18Gympie NP1 .csv", header=T)
clusterList <- clusterList[,2]

# Set time period and interval
n <- 15
interval <- 2*n + 1
 
# change n value above
# Time interval equals 2*n + 1 
######## Create references for plotting dates and times ################
timeRef <- indices$minute.of.day[1]
offset <- 0 - timeRef 

timePos   <- seq((offset), (nrow(indices)+359), by = 360) # 359 ensures timelabels to end
timeLabel <- c("00:00","6:00","12:00","18:00")
timeLabel <- rep(timeLabel, length.out=(length(timePos))) 

datePos <- c(seq((offset+720), nrow(indices)+1500, by = 1440))
dateLabel <- unique(substr(indices$rec.date, 1,10))
dateLabel <- dateLabel[1:length(datePos)]

#################################
counter <- 0
count <- NULL
uniqueClust <- NULL
  
for (i in (n+1):(length(clusterList)-(n+1))) {
  uniqClust <- length(unique(clusterList[(i-n):(i+n)]))
  for (j in 1:(2*n)) {
    series <- clusterList[(i-n):(i+n)]
    if (series[j]==series[j+1]) {
      counter <- counter + 1 
    }
  }
  count <- c(count, counter)
  uniqueClust <- c(uniqueClust, uniqClust)
}  

for(i in length(count):2) {
  count[i] <- (count[i] - count[i-1]) 
}

for(i in length(count):2) {
  count[i] <- 2*n - count[i]
}
# Find normalised counts
normalisedCount <- NULL

for (i in 1:length(count)) {
  normalisedCount[i] <- count[i]*(uniqueClust[i])
}

shift <- rep(count[1], n)
count <- c(shift, count)
dev.off()
par(mfcol=c(6,1))
par(mar=c(2,5,2,2))
plot(count[1:1440],type="l", xaxt='n',
     ylim=c(min(count[(2*n+1):length(count)]), max(count)))
axis(side = 4, line = -3, at = 5, labels = dateLabel[1], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.5)
mtext(paste(site, date, 2*n+1,"minutes", sep = "_"))
plot(count[1440:2880],type="l", xaxt='n', 
     ylim=c(min(count[(2*n+1):length(count)]), max(count)))
plot(count[2880:4320], type="l", xaxt='n',
     ylim=c(min(count[(2*n+1):length(count)]), max(count)))
plot(count[4320:5760], type="l", xaxt='n',
     ylim=c(min(count[(2*n+1):length(count)]), max(count)))
plot(count[5760:7200], type="l", xaxt='n',
     ylim=c(min(count[(2*n+1):length(count)]), max(count)))
plot(count[7200:8640], type="l", xaxt='n',
     ylim=c(min(count[(2*n+1):length(count)]), max(count)))
###############################################################
# Plot normalised count time-series
shift <- rep(normalisedCount[1], n)
normalisedCount <- c(shift, normalisedCount)
dev.off()
par(mfrow=c(7,1))
par(mar=c(0,2,0,1), oma=c(4,1,3,1), cex.axis=1)
plot(normalisedCount[1:1440],type="l", xaxt='n',
     ylim=c(min(normalisedCount[(2*n+1):length(normalisedCount)]), max(normalisedCount)))
mtext(paste(site, date, 2*n+1,"minutes", sep = "_"))
axis(side = 4, line = -3, at = 5, labels = dateLabel[1], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=(seq(0,1440,360)), lty=3)
plot(normalisedCount[1440:2880],type="l", xaxt='n', 
     ylim=c(min(normalisedCount[(2*n+1):length(normalisedCount)]), max(normalisedCount)))
abline(v=(seq(0,1440,360)), lty=3)
plot(normalisedCount[2880:4320], type="l", xaxt='n', 
     ylim=c(min(normalisedCount[(2*n+1):length(normalisedCount)]), max(normalisedCount)))
abline(v=(seq(0,1440,360)), lty=3)
plot(normalisedCount[4320:5760], type="l", xaxt='n',
     ylim=c(min(normalisedCount[(2*n+1):length(normalisedCount)]), max(normalisedCount)))
abline(v=(seq(0,1440,360)), lty=3)
plot(normalisedCount[5760:7200], type="l", xaxt='n',
     ylim=c(min(normalisedCount[(2*n+1):length(normalisedCount)]), max(normalisedCount)))
abline(v=(seq(0,1440,360)), lty=3)
plot(normalisedCount[7200:8640], type="l", xaxt='n',
     ylim=c(min(normalisedCount[(2*n+1):length(normalisedCount)]), max(normalisedCount)))
abline(v=(seq(0,1440,360)), lty=3)
plot(normalisedCount[8640:10080], type="l", xaxt='n',
     ylim=c(min(normalisedCount[(2*n+1):length(normalisedCount)]), max(normalisedCount)))
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
abline(v=(seq(0,1440,360)), lty=3)


#write.csv(timeSeries, file = paste("Time_series_stability", 
#                                   site, date, ".csv", sep = ""))
