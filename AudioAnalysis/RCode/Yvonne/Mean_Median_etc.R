# 7 July 2015
# Calculates the min,q25,median,q75,max,mean,mode,standardDev,range
# and states the normalised minimum and maximum values 

#setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")

#indices <- read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv",header = T)
indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv",header = T)

site <- indices$site[1]
date <- indices$rec.date[1]
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
mean <- c(mean, m)
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
}

normalisedValues <- array(c(-50, -10,
                            -50, -10,
                              0, 50,
                              3, 20,
                              0, 1,
                              0, 5,
                              0, 0.5,
                              0, 0.5,
                              0, 0.5,
                              0.4, 0.7,
                              0, 0.6,
                              0, 0.8,
                              0, 1,
                              0, 1,
                              0, 22), dim=c(2,15))

normValues <- aperm(normalisedValues)
dim(bxPlotStats) <- c(15,5)

all.stats <- cbind(rowNames, min, q25, median, q75, max, mean, 
                   mode, standardDev, range, normValues)

write.table(all.stats, file = paste("Statistics_matrix_", site, "_", date, 
                              ".csv", sep=""), sep = ",", qmethod = "double",
                              col.names = NA)