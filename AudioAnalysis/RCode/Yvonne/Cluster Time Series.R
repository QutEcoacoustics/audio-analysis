# 2 August 2015
# Cluster Time Series
# 
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015.csv", header=T)
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#vec <- read.csv("normIndicesClusters ,Woondum3 ,22-28 June 2015.csv", header=T)
vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015 5,9,11,13,14,15,17.csv", header = T)

# clusters <- unname(kmeansObj$cluster)
clusters <- vec$unname.kmeansObj.cluster.
#centers <- kmeansObj$centers
site <- "GympieNP 22-28 June 2015"

indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
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
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\TimeSeriesPlots")
# rename file here
n <- 60
interval <- 2*n + 1
file <- paste("Cluster Time Series_A", site, 
            "_", "interval = ",interval,".png", sep = "")
leadingNAs <- rep(-10, n)
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
# Time interval equals 2*n +1 
for (i in (n+1):(length(clusters)-(n+1))) {
  series <- unique(clusters[(i-n):(i+n)])
  lengthOfSeries <- length(series)
  for(j in seq_along(series)) {
    counter <- which(series==series[j])
    if (counter==1) {
      lengthOfSeries <- lengthOfSeries - 1
    }
  }
  timeSeries <- c(timeSeries, lengthOfSeries)
}
timeSeries <- c(leadingNAs, timeSeries)

# Add some NAs to the beginning and end of the dataset so 
# that the times remain in line with the x axis.
#for(j in seq_along(series)) {
#  print(paste(series[j]),sep="")
#}
par(mfrow=c(7,1))
par(mar=c(0,2,1,1), oma=c(4,1,2,1), cex.axis=3)
plot(timeSeries[1:1440], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)))
mtext(side = 3, paste("Cluster Time Series - GympieNP 22-28 June 2015",
                "Time interval= ",(n*2+1),"min", sep = " "), cex=3)
axis(side = 4, line = -3, at = 5, labels = dateLabel[1], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
plot(timeSeries[1441:2880], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 4, line = -3, at = 5, labels = dateLabel[2], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
plot(timeSeries[2881:4320], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 4, line = -3, at = 5, labels = dateLabel[3], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
plot(timeSeries[4321:5760], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 4, line = -3, at = 5, labels = dateLabel[4], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
plot(timeSeries[5761:7200], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 4, line = -3, at = 5, labels = dateLabel[5], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
plot(timeSeries[7201:8640], type="l", xaxt = 'n',
     ylim = c(0, max(timeSeries)))
axis(side = 4, line = -3, at = 5, labels = dateLabel[6], 
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
plot(timeSeries[8641:10080], type="l", xaxt = 'n',
     ylim = c(0,max(timeSeries)), cex=2)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
axis(side = 4, line=-3, at = 5, labels = dateLabel[7],
     mgp = c(4, 1.8, 0), tick = FALSE, cex=0.7)
abline(v=c(0,60,120,180))
dev.off()