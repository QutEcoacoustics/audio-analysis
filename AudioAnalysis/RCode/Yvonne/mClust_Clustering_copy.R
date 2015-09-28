# 3 September 2015
# The Mclust function uses an optimal Bayesian Information criteria (BIC)
# A seed does not need to be set.

setwd("C:\\Work\\CSV files\\GympieNP1_new\\mclust_30clusters")
indices <- read.csv("C:\\Work\\CSV files\\GympieNP1_new\\all_data\\Towsey_Summary_Indices_22-06-2015all.csv", header = T)
#indices <- read.csv("C:\\Work\\CSV files\\GympieNP1_new\\all_data\\Towsey_Summary_Indices_Gympie NP1 22-06-2015to current.csv")
list <- which(indices$minute.of.day=="0")
lst1 <- NULL
for (i in 1:length(list)) {
  lst <- list[i+1]-1
  lst1 <- c(lst1, lst)
}
list <- cbind(list, lst1)
colnames(list) <- c("start","end")
write.table(list[,1:2], file="list.csv", 
            row.names = F, sep = ",")

site <- indices$site[1]
startDate <- indices$rec.date[1]
endDate <- indices$rec.date[length(indices$rec.date)]

################ Normalise data ####################
# normalize values using minimum and maximum values
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

normIndices <- indices
# normalise variable columns
normIndices[,1]  <- normalise(indices[,1], -44.97,-29.52)  # BackgroundNoise (-50,-10)
normIndices[,2]  <- normalise(indices[,2],  3.41, 7.78)  # AvSnrofActive Frames (3,10)
normIndices[,3]  <- normalise(indices[,3],  0, 2.7) # EventsPerSecond (0,2)
normIndices[,4] <- normalise(indices[,4], 0.015, 0.17)  # HighFreqCover (0,0.5)
normIndices[,5] <- normalise(indices[,5], 0.014, 0.20)  # MidFreqCover (0,0.5)
normIndices[,6] <- normalise(indices[,6], 0.019,  0.26)  # LowFreqCover (0,0.5)
normIndices[,7] <- normalise(indices[,7], 0.41, 0.51)  # AcousticComplexity (0.4,0.7)
normIndices[,8] <- normalise(indices[,8], 0.12, 1)  # EntropyOfPeaksSpectrum (0,1)
normIndices[,9] <- normalise(indices[,9], 0.0045, 0.52)   # EntropyOfCoVSpectrum (0,0.7)

# adjust values greater than 1 or less than 0
for (j in 1:9) {
  for (i in 1:length(normIndices[,j])) {
    if (normIndices[i,j] > 1) {
      normIndices[i,j] = 1
    }
  }
  for (i in 1:length(normIndices[,j])) {
    if (normIndices[i,j] < 0) {
      normIndices[i,j] = 0
    }
  }
}
##############################################
# Create a list of the number of minutes per day used to plot colours ##########
#counter <- NULL
#list <- 0
#endTime <- length(indices$rec.time)
#mn <-indices[grep("\\<0\\>", indices$minute.of.day),]
#min.per.day <- NULL
#for (k in 1:length(mn$rec.time)) {
#  m <- mn$X[k]
#  list <- c(list, m)
#}
#list <- c(list, endTime)

#for (j in 1:(length(mn$rec.time)+1)) {
#  diff <- list[j+1] - list[j]
#  d <- c(min.per.day, diff)
#  counter <- c(counter, d)
#}

# adjust first and last counter by one
#counter[1] <- counter[1]-1
#counter[length(mn$rec.time)] <- counter[length(mn$rec.time)] + 1

#counter <- NULL

#for (i in 1:length(list)) {
#  count <- list[i+1]-list[i]
#  counter <- c(counter, count)
#}
#counter <- counter[1:length(counter)-1]
######## Create day identification for different colours in plot #############
#number.of.days <- length(unique(indices$rec.date))
#day <- NULL

#if (counter[1]==0) 
#  for (i in 1:(number.of.days+1)) {
#    id <- rep(LETTERS[i], counter[i])
#    day <- c(day, id)
#  }


#if (counter[1] > 0) 
#  for (i in 1:(number.of.days)) {
#    id <- rep(LETTERS[i], counter[i])
#    day <- c(day, id)
#  }

#indices <- cbind(indices, day)

######## Create references for plotting dates and times ################
#timeRef <- indices$minute.of.day[1]
#offset <- 0 - timeRef 

#timePos   <- seq((offset), (nrow(indices)+359), by = 360) # 359 ensures timelabels to end
#timeLabel <- c("00:00","6:00","12:00","18:00")
#timeLabel <- rep(timeLabel, length.out=(length(timePos))) 

#datePos <- c(seq((offset+720), nrow(indices)+1500, by = 1440))
#dateLabel <- unique(substr(indices$rec.date, 1,10))
#dateLabel <- dateLabel[1:length(datePos)]
##########################################################
indicesRef <- c(1:9)          #(5,7,9,10,11,12,13,17,18)
length1 <- 0
length2 <- length(normIndices$BackgroundNoise)
dataFrame <- normIndices[length1:length2, indicesRef]  

dev.off()
#colourBlock <- read.csv("colours.csv", header = T)
#colours <- c("red", "chocolate4", "palegreen", "darkblue",
#             "brown1", "darkgoldenrod3", "cadetblue4", 
#             "darkorchid", "orange" ,"darkseagreen", 
#             "deeppink3", "darkslategrey", "firebrick2", 
#             "gold2", "hotpink2", "blue", "maroon", 
#             "mediumorchid4", "mediumslateblue","mistyrose4",
#             "royalblue", "orange", "palevioletred2", 
#             "sienna", "slateblue", "yellow", "tan2", 
#             "salmon","violetred1","plum")

#write.table(colours, file="colours.csv", row.names = F)

# The next line cannot be run on all 8 weeks because
# In double(ld) : Reached total allocation of 16289Mb: 
# see help(memory.size).  Running on one week takes 
# around 10 minutes

# Week 2 
#start <- list[8]; end <- list[15]-1
# Week 3
#start <- list[15]; end <- list[22]-1
# Week 4
#start <- list[22]; end <- list[29]-1

# Model-Based Clustering
library(mclust)

odd <- seq(from=1, to=length(dataFrame$BackgroundNoise), by=2)
#trainData <- dataFrame[odd,1:9]
#trainClass <- dataFrame$Cluster[odd]
#testData <- dataFrame[-odd,1:9]
#testClass <- iris$Cluster[-odd]

#start <- 1 
#end   <- 60000         
fit <- Mclust(dataFrame[odd,1:9], G=1:100)
summary(fit) 

mclust30list_9_all <- unname(fit$classification)
plot(mclust30list_9_all)
write.table(mclust30list_9_all, 
            file="mclustG1_30_I9_T1_60000minutes.csv", 
            row.names = F, sep = ",")
            
mean <- fit$parameters$mean
write.table(mean, 
            file="mclustG1_30_I9_T50001_60000minutes_mean.csv", 
            row.names = F, sep = ",")

sink('mclustG1_30_I9_T50001_60000minutes_variance.csv')
fit$parameters$variance
sink()

sigma <- fit$parameters$variance$sigma
write.table(mean, 
            file="mclustG1_30_I9_T50001_60000minutes_sigma.csv", 
            row.names = F, sep = ",")

#coordProj(data = dataFrame[start:end,1:9], dimens = c(4,9), what = "density",
#          parameters = fit$parameters, z = fit$z)

png('BIC_plot.png', 
    width = 1500, height = 1200, units = "px") 
plot(fit, what = "BIC")
dev.off()

png('Density_plot.png', 
    width = 1500, height = 1200, units = "px") 
plot(fit, what = "density")
dev.off()

png('Classification_plot.png', 
    width = 1500, height = 1200, units = "px") 
plot(fit, what = "classification")
dev.off()

# Density plot # these take a while these give nice density plots of 
# the normalised data

png('densBGR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densBackgr <- densityMclust(dataFrame$BackgroundNoise)
plot(densBackgr, data = dataFrame$BackgroundNoise, what = "density")
dev.off()

png('densAvSNR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAvSNR <- densityMclust(dataFrame$AvgSnrOfActiveFrames)
plot(densAvSNR, data = dataFrame$AvgSnrOfActiveFrames, what = "density")
dev.off()

png('densAccComp_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAcousticComp <- densityMclust(dataFrame$AcousticComplexity)
plot(densAcousticComp, data = dataFrame$AcousticComplexity, what = "density")
dev.off()

png('densEntCOV_plot.png', 
    width = 1500, height = 1200, units = "px") 
densEntCoV <- densityMclust(dataFrame$EntropyOfCoVSpectrum)
plot(densEntCv, data = dataFrame$EntropyOfCoVSpectrum, what = "density")
dev.off()

png('densLowFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densLowFrCov <- densityMclust(dataFrame$LowFreqCover)
plot(densLowFrCov, data = dataFrame$LowFreqCover, what = "density")
dev.off()

png('densMidFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densMidFrCov <- densityMclust(dataFrame$MidFreqCover)
plot(densMidFrCov, data = dataFrame$MidFreqCover, what = "density")
dev.off()

png('densHighFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densHighFrCov <- densityMclust(dataFrame$HighFreqCover)
plot(densHighFrCov, data = dataFrame$HighFreqCover, what = "density")
dev.off()

png('densEntPS_plot.png', 
    width = 1500, height = 1200, units = "px") 
densEntPs <- densityMclust(dataFrame$EntropyOfPeaksSpectrum)
plot(densBackgr, data = dataFrame$EntropyOfPeaksSpectrum, what = "density")
dev.off()

# 3D plot
densBackgr_HighFreq <- densityMclust(dataFrame[1:4000,c(1,4)])
plot(densBackgr_HighFreq, type = "persp", col = grey(0.8))

#write.table(mclust30list_9_all, file="mclust30listG10_35_9_weeks1_4_1_40000minutes_a.csv", 
#            row.names = F)
#clusters <- read.csv(file="mclust30listG10_35_9_weeks1_4_1_40000minutes_a.csv", header=T)
#write.table(fit$parameters$mean, file="mclust30listG10_35_9_weeks1_4_1_40000minutes_a_mean.csv", 
#            row.names = F, sep = ",")

plot(fit, what = "BIC")
plot(fit, what = "classification")
plot(fit, what = "uncertainty")
plot(fit, what = "density")
summary(fit)
DR <- MclustDR(fit)
summary(DR)
# In the example, the first two directions account for most of 
# the clustering structure
plot(DR, what = "evalues")
# The plots below are excellent!!!!
plot(DR)
sym <- rep(pch[1],31) 
plot(DR, what = "contour", symbols = sym)

plot(DR, what = "boundaries", symbols = sym) # showing uncertainty boundaries
plot(DR, what = "density", dimens = 1, symbols = sym) #density plot in dir2 vs dir1
plot(DR, what = "density", dimens = 2, symbols = sym) # density plot in dir1 vs dir2
plot(DR, what = "density", dimens = 3, symbols = sym) #density plot in dir2 vs dir1
plot(DR, what = "density", dimens = 4, symbols = sym) # density plot in dir1 vs dir2
help(plot.MclustDR)

CLUSTCOMBI <- clustCombi(dataFrame[start:end,1:9], fit)
CLUSTCOMBI
# This gives an entropy plot which which indicates 
# number of clusters
plot(CLUSTCOMBI, dataFrame[start:end,1:9]) 

#plot(fit) # plot results 
#summary(fit) # display the best model
#png(paste("Clusterplot_mclust", indices$rec.data[start],
#          indices$rec.date[end],"5,7,9,11,12,13,17,18.png",
#          sep="_"),
#    width = 400, 
#    height = 85, 
#    units = "mm",
#    res=1200,
#    pointsize = 4)

#par(mar=c(3,5,3,3))
#plot(fit$classification, col=colours[fit$classification],
#     xaxt = 'n', xlab = "", ylab = "Cluster reference", cex.axis=2,
#     cex.lab=1.5, main = paste(site, indices$rec.date[start],
#     indices$rec.date[end], "(mclust)", sep = "_"),
#     cex.main = 2)
#axis(side = 1, at = timePos, labels = timeLabel, mgp = c(1.8, 0.5, 0), 
#     cex.axis = 1.5, mgp = c(1.8, 0.5, 0))
#axis(side = 1, at = datePos, labels = dateLabel, mgp = c(4, 1.8, 0),
#     tick = FALSE, cex.axis=1.5, mgp = c(4, 1.8, 0))
#mtext(paste(indicesRef, collapse = ", ", sep = ""), side=3, 
#      line = -0.1, cex = 1.5)
#for (i in 1:length(list)) {
#  abline(v=list[i], lwd=1.5, lty = 3)
#}
#dev.off()

#fit$parameters$mean
#mclust15List <- unname(fit$classification)
#mclust30list_9 <- unname(fit$classification)
#mclust23list_9 <- unname(fit$classification)
#mclust <- cbind(mclust15list_9, mclust23list_9, mclust30list_9)

dataFrame[1:10000,11] <- unname(fit$classification)

start <- 1
end <- 10000
odd <- seq(from=1, to=nrow(dataFrame[start:end,1:9]), by=2)
trainData <- dataFrame[odd,1:9]
trainClass <- dataFrame$Indices[odd]
testData <- dataFrame[-odd,1:9]
testClass <- iris$Indices[-odd]

