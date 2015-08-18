# 2 August 2015
# Cluster Time Series
# Creates a plot of the number of the number of clusters in a time
# interval.  The time interval is set via "n" where the interval 
# equals 2n + 1.  

#  This file is #11 in the sequence:
#   1. Save_Summary_Indices_ as_csv_file.R
#   2. Plot_Towsey_Summary_Indices.R
#   3. Correlation_Matrix.R
#   4. Principal_Component_Analysis.R
#   5. kMeans_Clustering.R
#   6. Quantisation_error.R
#   7. Distance_matrix.R
#   8. Minimising_error.R
#   9. Segmenting_image.R
#  10. Transition Matrix    
# *11. Cluster Time Series

setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015.csv", header=T)
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#vec <- read.csv("normIndicesClusters ,Woondum3 ,22-28 June 2015.csv", header=T)
#vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015 5,9,11,13,14,15,17.csv", header = T)
vec <- read.csv("Cluster_list 22-28 June 2015_5,7,9,10,11,12,13,14,15,17,18Gympie NP1 .csv", header=T)
vec <- vec[,2]

# clusters <- unname(kmeansObj$cluster)
clusters <- vec$unname.kmeansObj.cluster.
#centers <- kmeansObj$centers

indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
date <- paste(indices$rec.date[1], indices$rec.date[length(indices$rec.date)], sep = " to ")
site <- indices$site[1]
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
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\TimeSeriesPlots")
n <- 7
interval <- 2*n + 1

# rename file here
file <- paste("Cluster Time Series_B", site, 
            "_", "interval = ",interval,".png", sep = "")
shift <- rep(0, n)
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

timeSeries <- NULL
# change n value above
# Time interval equals 2*n + 1 
for (i in (n+1):(length(clusters)-(n+1))) {
  series <- unique(clusters[(i-n):(i+n)])
  clusterSet <- clusters[(i-n):(i+n)]
  lengthOfSeries <- length(series)
  for(j in seq_along(series)) {
        counter <- length(which(clusterSet==series[j]))
  }
  if (counter==1) {
      lengthOfSeries <- lengthOfSeries - 1
  }
  timeSeries <- c(timeSeries, lengthOfSeries)
}
timeSeries <- c(shift, timeSeries)

write.csv(timeSeries, file = paste("Cluster_time_series_", 
          site, date, ".csv", sep = ""))

## filtering
#par(mfrow=c(2,2))
#plot(timeSeries[360:720], 
#       type="n", ylim=c(0, max(timeSeries)))
#m.1 <- filter(timeSeries[360:720],
#       filter=rep(1/60,60)) # 30 minute
#lines(m.1,col="red")
#plot(timeSeries[1800:2160], 
#     type="n", ylim=c(0, max(timeSeries)))
#m.1 <- filter(timeSeries[1800:2160],
#              filter=rep(1/60,60)) # 30 minute
#lines(m.1,col="red")
#plot(timeSeries[3240:3600], 
#     type="n", ylim=c(0, max(timeSeries)))
#m.1 <- filter(timeSeries[3240:3600],
#              filter=rep(1/60,60)) # 30 minute
#lines(m.1,col="red")
#plot(timeSeries[4680:5040], 
#     type="n", ylim=c(0, max(timeSeries)))
#m.1 <- filter(timeSeries[4680:5040],
#              filter=rep(1/60,60)) # 30 minute
#lines(m.1,col="red")

# Add some zeros to the beginning and end of the dataset so 
# that the times remain in line with the x axis.

par(mfrow=c(7,1))
par(mar=c(0,2,0,1), oma=c(4,1,3,1), cex.axis=3)
plot(timeSeries[1:1440], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)))
mtext(side = 3, paste("Cluster Time Series - GympieNP 22-28 June 2015",
    "Time interval= ",(n*2+1),"min", sep = " "), cex=3)
axis(side = 4, line = -3, at = 5, labels = dateLabel[1], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=(seq(0,1440,60)), lty=3)
plot(timeSeries[1441:2880], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 4, line = -3, at = 5, labels = dateLabel[2], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=(seq(0,1440,60)), lty=3)
plot(timeSeries[2881:4320], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 4, line = -3, at = 5, labels = dateLabel[3], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=(seq(0,1440,60)), lty=3)
plot(timeSeries[4321:5760], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 4, line = -3, at = 5, labels = dateLabel[4], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=(seq(0,1440,60)), lty=3)
plot(timeSeries[5761:7200], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 4, line = -3, at = 5, labels = dateLabel[5], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=(seq(0,1440,60)), lty=3)
plot(timeSeries[7201:8640], type="l", xaxt = 'n',
     ylim = c(0, max(timeSeries)))
axis(side = 4, line = -3, at = 5, labels = dateLabel[6], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=(seq(0,1440,60)), lty=3)
plot(timeSeries[8641:10080], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
axis(side = 4, line=-3, at = 5, labels = dateLabel[7],
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=(seq(0,1440,60)), lty=3)
dev.off()