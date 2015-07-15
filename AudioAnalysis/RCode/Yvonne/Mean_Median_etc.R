#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
#setwd("C:\\Work\\CSV files\\2015Jul01-120417\\GympieNP\\")
setwd("C:\\Work\\CSV files\\2015Jul01-120417\\Woondum3\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150622_000000 to 20150628_064559 .csv",header = T)
indices <- read.csv("Towsey_Summary_Indices 20150622_000000 to 20150628_133139 .csv", header = T)

name <- "Woondum3"
date <- "2015June22"
#plot(indices[,12])
mean        <- numeric()
median      <- numeric()
mode        <- numeric()
standardDev <- numeric()
max         <- numeric()
min         <- numeric()
q25         <- numeric()
q75         <- numeric()
range       <- numeric()
bxPlotStats <- numeric()
all.stats   <- numeric()

rowNames    <-  c("AvgSignalAmplitude",       #4th column
                  "BackgroundNoise",          #5
                  "Snr",                      #6
                  "AvgSnrOfActiveFrames",     #7
                  "Activity",                 #8
                  "EventsPerSecond",          #9
                  "HighFreqCover",           #10
                  "MidFreqCover",            #11
                  "LowFreqCover",            #12
                  "AcousticComplexity",      #13
                  "TemporalEntropy",         #14
                  "AvgEntropySpectrum",      #15
                  "VarianceEntropySpectrum", #16
                  "EntropyPeaks",            #17
                  "SptDensity")              #18

Mode <- function (x){
  ux <- unique(x)
  ux[which.max(tabulate(match(x,ux)))]
}

for (i in 4:18) {
m <- mean(indices[,i])
average <- c(mean, m)
md <- median(indices[,i])
median <- c(median, md)
mo <- Mode(indices[,i])
mode <- c(mode, mo)
std <- sd(indices[,i])
standardDev <- c(standardDev, std)
mx <- max(indices[,i])
max <- c(max, mx)
mn <- min(indices[,i])
min <- c(min, mn)
tw5th <- quantile(indices[,i], 0.25)
tw5th <- unname(tw5th)
q25 <- c(q25, tw5th)
svn5th <- quantile(indices[,i], 0.75)
svn5th <- unname(svn5th)
q75 <- c(q75, svn5th)
rge <- range(indices[,i])
rge1 <- rge[2]-rge[1]
range <- c(range, rge1)
bxpl <- boxplot.stats(indices[,i])
bxpl <- bxpl$stats
dim(bxpl) <- c(1,5)
bxPlotStats <- rbind(bxPlotStats, bxpl)
}

normalisedValues <- array(c(-50,-10,
                            -50,-10,
                              0,50,
                              0,30,
                              0,1,
                              0,5,
                              0,0.5,
                              0,0.5,
                              0,0.5,
                              0.4,0.7,
                              0,0.5,
                              0,1,
                              0,1,
                              0,1,
                              0,2), dim=c(2,15))

normValues <- aperm(normalisedValues)
dim(bxPlotStats) <- c(15,5)

all.stats <- cbind(rowNames, min, q25, median, q75, max, average, 
                   mode, standardDev, range, bxPlotStats, normValues)

#write.table(all.stats, 'all.stats.txt', col.names = NA)

write.table(all.stats, file = paste("Statistics_matrix_", name, "_", date, 
                              ".csv", sep=""), sep = ",", qmethod = "double",
                              col.names = NA)