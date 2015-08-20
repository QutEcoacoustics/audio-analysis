#  Date: 17 July 2015
#  R version:  3.2.1 
#  This file calculates the Principal Component Analysis and plots the
#  result
#
#  This file is #3 in the sequence:
#  1. Save_Summary_Indices_ as_csv_file.R
#  2. Plot_Towsey_Summary_Indices.R
#  3. Correlation_Matrix.R
# *4. Principal_Component_Analysis.R
#  5. kMeans_Clustering.R
#  6. Quantisation_error.R
#  7. Distance_matrix.R
#  8. Minimising_error.R
#  9. Segmenting_image.R

########## You may wish to change these ###########################
#setwd("C:\\Work\\CSV files\\Woondum1\\2015_03_15\\")
#setwd("C:\\Work\\CSV files\\Woondum2\\2015_03_22\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_28\\")
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_06_28\\")
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_07_05\\")
#setwd("C:\\Work\\CSV files\\Woondum3\\2015_07_05\\")

#indices <- read.csv("Towsey_Summary_Indices_Woondum1 20150315_133427to20150320_153429.csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum2 20150322_113743to20150327_103745.csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622_000000to20150628_064559.csv", header = T)
indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150622_000000to20150628_133139.csv", header = T)
#indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150628_105043to20150705_064555.csv",header = T)
#indices <- read.csv("Towsey_Summary_Indices_Woondum3 20150628_140435to20150705_064558.csv",header = T)

# Gympie NP1 22_06_15
#xlim <- c(-0.012, 0.033)
#ylim = c(-0.005,0.02)
# Gympie NP1 28_06_15
#xlim <- c(-0.025, 0.035)
#ylim = c(-0.02,0.003)
# Woondum3 21_06_15
#xlim <- c(-0.017, 0.02)
#ylim = c(-0.008,0.01)
# Woondum3 28_06_15
#xlim <- c(-0.017, 0.02)
#ylim = c(-0.001,0.009)

site <- indices$site[1]
date <- paste(indices$rec.date[1], 
        indices$rec.date[length(indices$rec.date)],
        sep = "_")

#xlim <- c(-0.025, 0.035)
#ylim <- c(-0.02,0.003)
xlim <- c(-0.035,0.035)
ylim <- c(-0.035,0.035)
################ Normalise data ####################################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

#entropy_cov <- indices[,16]/indices[,15] # Entropy of the coefficient of variance
#entropy_cov <- normalise(entropy_cov, 0,25)

#for (i in 1:length(entropy_cov)) {
#  if (entropy_cov[i] > 1) {
#    entropy_cov[i] = 1
#  }
#  if(entropy_cov[i] < 0) {
#    entropy_cov[i] =0
#  }
#}

# Pre-processing transformation of Temporal Entropy
# to correct the long tail 
indices[,14] <- sqrt(indices[,14])

normIndices <- indices

# normalise variable columns
normIndices[,4]  <- normalise(indices[,4], -50,-10)   # AverageSignalAmplitude
normIndices[,5]  <- normalise(indices[,5], -50,-10)   # BackgroundNoise
normIndices[,6]  <- normalise(indices[,6],  0, 50)    # Snr
normIndices[,7]  <- normalise(indices[,7],  3, 10)    # AvSnrofActive Frames
normIndices[,8]  <- normalise(indices[,8],  0, 1)     # Activity 
normIndices[,9]  <- normalise(indices[,9],  0, 2)     # EventsPerSecond
normIndices[,10] <- normalise(indices[,10], 0, 0.5)   # HighFreqCover
normIndices[,11] <- normalise(indices[,11], 0, 0.5)   # MidFreqCover
normIndices[,12] <- normalise(indices[,12], 0, 0.5)   # LowFreqCover
normIndices[,13] <- normalise(indices[,13], 0.4,0.7)  # AcousticComplexity
normIndices[,14] <- normalise(indices[,14], 0, sqrt(0.3))   # TemporalEntropy
normIndices[,15] <- normalise(indices[,15], 0, 0.7)   # EntropyOfAverageSpectrum
normIndices[,16] <- normalise(indices[,16], 0, 1)     # EntropyOfVarianceSpectrum
normIndices[,17] <- normalise(indices[,17], 0, 1)     # EntropyOfPeaksSpectrum
normIndices[,18] <- normalise(indices[,18], 0, 0.7)   # EntropyOfCoVSpectrum
normIndices[,19] <- normalise(indices[,19], -0.8, 1)  # NDSI
normIndices[,20] <- normalise(indices[,20], 0, 15)    # SptDensity

# adjust values greater than 1 or less than 0
for (j in 4:20) {
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

# Select which indices to consider
#normIndices <- cbind(normIndices[,c(5,7,9,10,11,12,13,14,15,17)], entropy_cov)
######### PCA biplot #####################################
#file <- paste("Principal Component Analysis_adj_ranges", site, 
#              "_", date, ".png", sep = "")
#png(
#  file,
#  width     = 200,
#  height    = 200,
#  units     = "mm",
#  res       = 1200,
#  pointsize = 4
#)

#par(mar =c(2,2,4,2), cex.axis = 2.5)
#PCAofIndices<- prcomp(normIndices)
#biplot(PCAofIndices, col=c("pink","blue"), 
#       cex=c(0.5,0.9), ylim = ylim, 
#       xlim = xlim)
#abline(h=0,v=0)
#mtext(side = 3, line = 2, paste("Principal Component Analysis prcomp ",
#              site, date, sep = " "), cex = 2.5)

#dev.off()
#### Preparing the dataframe ###############################
#normIndices.pca <- prcomp(normIndices[,1:17], 
#                          scale. = F)
normIndices <- normIndices[,c(5,7,9,10,11,13,14,15,17,18,37,38,39,40)]
normIndices.pca <- prcomp(normIndices[,1:10], scale. = F)

normIndices$PC1 <- normIndices.pca$x[,1]
sum(normIndices$PC1)
normIndices$PC2 <- normIndices.pca$x[,2]
normIndices$PC3 <- normIndices.pca$x[,3]
normIndices$PC4 <- normIndices.pca$x[,4]
normIndices$PC5 <- normIndices.pca$x[,5]
normIndices$PC6 <- normIndices.pca$x[,6]
normIndices$PC7 <- normIndices.pca$x[,7]
normIndices$PC8 <- normIndices.pca$x[,8]
normIndices$PC9 <- normIndices.pca$x[,9]
normIndices$PC10 <- normIndices.pca$x[,10]
plot(normIndices.pca)
biplot(normIndices.pca)

# assign colours to time-periods
normIndices <- within(normIndices, levels(fourhour.class) <- c("red","orange","yellow","green","blue","violet"))
normIndices <- within(normIndices, levels(day.night) <- c("midnightblue","orange","yellow","green","blue"))
normIndices <- within(normIndices, levels(nautical.twilight) <- c("midnightblue","orange"))
normIndices <- within(normIndices, levels(hour.class) <- 
              c("#FF0000FF","#FF4000FF","#FF8000FF","#FFBF00FF","#FFFF00FF",
                "#BFFF00FF","#80FF00FF","#40FF00FF","#00FF00FF","#00FF40FF",
                "#00FF80FF","#00FFBFFF","#00FFFFFF","#00BFFFFF","#0080FFFF",
                "#0040FFFF","#0000FFFF","#4000FFFF","#8000FFFF","#BF00FFFF",
                "#FF00FFFF","#FF00BFFF","#FF0080FF","#FF0040FF"))
                      
#fooPlot <- function(x, main, ...) {
#  if(missing(main))
#    main <- deparse(substitute(x))
#  plot(x, main = main, ...)
#}

#  set.seed(42)
#dat <- data.frame(x = rnorm(1:10), y = rnorm(1:10))
#fooPlot(dat, col = "red")

#### Plotting PC1 & PC2 Principal Component Plots with base plotting system
# change file name when necessary
png('pca_plot PC1_PC2_selected_indices.png', width=1500, 
    height=1200, units="px") 
PrinComp_X_axis <- "PC1"
PrinComp_Y_axis <- "PC2"
first <- 1  # change this and values in plot function below!!! to match PC# 
second <- 2  # change this!!! to match PC#
arrowScale <- 1.5 # increase/decrease this to adjust arrow length
summ <- summary(normIndices.pca)
rotate <- unname(summ$rotation)
labels <- names(normIndices[1:length(summ$center)])

mainHeader <- paste (site, date, PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
par(mar=c(6,6,4,4))
plot(normIndices$PC1,normIndices$PC2,  # Change these!!!!! 
     col=as.character(normIndices$hour.class), 
     cex=1.2, type='p', pch=19, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.1, 
       rotate[i,second]*arrowScale*1.1, 
       paste(labels[i]), cex=1.6)
}
abline (v=0, h=0, lty=2)
legend('topright', hours, pch=19, col=c('red','orange','yellow',
        'green','blue','violet'), bty='n', cex=2)
dev.off()

#### Plotting PC1 & PC3 Principal Component Plots with base plotting system
# change file name when necessary
png('pca_plot PC1_PC3_selected_indices.png', width=1500, height=1200, units="px") 
PrinComp_X_axis <- "PC1"
PrinComp_Y_axis <- "PC3"
first <- 1  # change this and values in plot function below!!! to match PC# 
second <- 3  # change this!!! to match PC#
arrowScale <- 0.7 # increase/decrease this to adjust arrow length
summ <- summary(normIndices.pca)
rotate <- unname(summ$rotation)
labels <- names(normIndices[1:length(summ$center)])

mainHeader <- paste (site, date, PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
par(mar=c(6,6,4,4))
plot(normIndices$PC1,normIndices$PC3,  # Change these!!!!! 
     col=as.character(normIndices$hour.class), 
     cex=1.2, type='p', pch=19, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.1, 
       rotate[i,second]*arrowScale*1.1, 
       paste(labels[i]), cex=1.6)
}
abline (v=0, h=0, lty=2)
legend('topright', hours, pch=19, 
       col=c('red','orange','yellow',
       'green','blue','violet'), bty='n', 
       cex=2)
dev.off()

#### Plotting PC2 & PC3 Principal Component Plots with base plotting system
# change file name when necessary
png('pca_plot PC2_PC3_selected_indices.png', width=1500, 
    height=1200, units="px") 
PrinComp_X_axis <- "PC2"
PrinComp_Y_axis <- "PC3"
first <- 2  # change this and values in plot function below!!! to match PC# 
second <- 3  # change this!!! to match PC#
arrowScale <- 0.7 # increase/decrease this to adjust arrow length
summ <- summary(normIndices.pca)
rotate <- unname(summ$rotation)
labels <- names(normIndices[1:length(summ$center)])

mainHeader <- paste (site, date, PrinComp_X_axis, 
                     PrinComp_Y_axis, sep=" ")
normIndices <- within(normIndices, levels(hour.class) 
            <- c("red","orange","yellow","green","blue",
                 "violet"))
#normIndices <- within(normIndices, levels(day.night) 
#                      <- c("red","orange","yellow","green","blue",
#                           "violet"))
par(mar=c(6,6,4,4))
plot(normIndices$PC2,normIndices$PC3,  # Change these!!!!! 
     col=as.character(normIndices$hour.class), 
     cex=1.2, type='p', pch=19, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)",
                sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.1, 
       rotate[i,second]*arrowScale*1.1, 
       paste(labels[i]), cex=1.6)
}
abline (v=0, h=0, lty=2)
legend('topright', hours, pch=19, 
       col=c('red','orange','yellow',
       'green','blue','violet'), bty='n', cex=2)
dev.off()
####### PCA plot in ggplot ################
file <- paste("Principal Component Analysis_adj_ranges_ggbiplot", site, 
              "_", date, ".png", sep = "")

library(ggbiplot)
normIndices.pca <- prcomp(normIndices[,1:17], 
                          scale. = F)

# Initiate initial plot 
#p <- NULL

#p <- print(ggbiplot(normIndices.pca, obs.scale = 1, 
#           var.scale = 1, groups = normIndices$hour.class,
#           ellipse = TRUE, circle = F), size = 0.3)
#png('pca_biplot.png', width=1500,height=1500,units="px")  

g <- ggbiplot(normIndices.pca, obs.scale = 1, var.scale = 1,
              groups = normIndices$hour.class, 
              ellipse = TRUE, circle = TRUE,
              varname.size=20,
              labels=normIndices$hour.class,
              labels.size = 10)
g <- g + scale_color_manual(values = c("red", "orange", "yellow", 
                                       "green", "blue", "violet"))
g <- g + theme_classic()
g <- g + theme(legend.direction = 'horizontal',
               legend.position = 'top')
g <- g + theme(axis.text=element_text(size=40),
        axis.title=element_text(size=40,face="bold"))
g <- g + geom_hline(yintercept = 0)
g <- g + geom_vline(xintercept = 0)
g <- g +  theme(legend.text = element_text(size=50))
print(g)

# Open png device
png('pca_biplot.png', width=3000, height=3000,
    units="px")  

print(g)    # Print to png device         
dev.off()

####### 3d plot #################################
library(rgl) # using rgl package
colourSet1 <- c("#FF0000FF","#FF4000FF","#FF8000FF",
                "#FFBF00FF","#FFFF00FF","#BFFF00FF")
colourSet2 <- c("#80FF00FF","#40FF00FF","#00FF00FF",
                "#00FF40FF","#00FF80FF","#00FFBFFF")
colourSet3 <- c("#00FFFFFF","#00BFFFFF","#0080FFFF",
                "#0040FFFF","#0000FFFF","#4000FFFF")
colourSet4 <- c("#8000FFFF","#BF00FFFF","#FF00FFFF",
                "#FF00BFFF","#FF0080FF","#FF0040FF")

normIndices <- within(normIndices, levels(hour.class) <- 
                        c(colourSet1,colourSet2, colourSet3,colourSet4))

day <- c(0, 1440, 2880, 4320, 5760, 7200, 8640, 10080)
offset <- c(0,360,720,1080)
start <-  3600         #day[5] + offset[4] + 1   
finish <- 3660                #length(normIndices$PC1) #day[7]-1
start
finish
plot3d(normIndices$PC1[start:finish], normIndices$PC2[start:finish], 
       normIndices$PC3[start:finish], 
       col=adjustcolor(normIndices$hour.class, alpha.f = 0.1))
spheres3d(normIndices$PC1[start:finish], normIndices$PC2[start:finish], 
        normIndices$PC3[start:finish], 
          col=adjustcolor(normIndices$hour.class, alpha.f = 0.1),
          radius = 0.015)
xyzCoords <- data.frame(x1= numeric(10),  y1= integer(10), 
                      z1 = numeric(10), x2= numeric(10), 
                      y2= integer(10),  z2 = numeric(10))
for (i in 1:10) {
  xyzCoords$x2[i] <- rotate[i,1]
  xyzCoords$y2[i] <- rotate[i,2]
  xyzCoords$z2[i] <- rotate[i,3]
}
# xyz co-ordinates for segments
xyzCoords <- data.frame(x1= numeric(10),  y1= integer(10), 
                      z1 = numeric(10), x2= numeric(10), 
                      y2= integer(10),  z2 = numeric(10))
for (i in 1:10) {
  xyzCoords$x2[i] <- rotate[i,1]*0.8
  xyzCoords$y2[i] <- rotate[i,2]*0.8
  xyzCoords$z2[i] <- rotate[i,3]*0.8
}
segments3d(x=as.vector(t(xyzCoords[1:10,c(1,4)])),
           y=as.vector(t(xyzCoords[1:10,c(2,5)])),
           z=as.vector(t(xyzCoords[1:10,c(3,6)])), 
           lwd=2, col= "midnightblue")

#library(car) # using car package
#scatter3d(normIndices$PC1, normIndices$PC2, normIndices$PC3, 
#          point.col=normIndices$hour.class, surface = F)

# Acoustic flux
flux <- NULL

for (i in 1:length(normIndices$BackgroundNoise))  {
distance <- sqrt((normIndices[i,1] - normIndices[(i+1),1])^2 +
                 (normIndices[i,2] - normIndices[(i+1),2])^2 +
                 (normIndices[i,3] - normIndices[(i+1),3])^2 +
                 (normIndices[i,4] - normIndices[(i+1),4])^2 +
                 (normIndices[i,5] - normIndices[(i+1),5])^2 +
                 (normIndices[i,6] - normIndices[(i+1),6])^2 +
                 (normIndices[i,7] - normIndices[(i+1),7])^2 +
                 (normIndices[i,8] - normIndices[(i+1),8])^2 +
                 (normIndices[i,9] - normIndices[(i+1),9])^2 +
                 (normIndices[i,10] - normIndices[(i+1),10])^2)
flux <- c(flux, distance)
}
###################
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
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
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")

# Smoothing using a moving average with 2 sides
png('Flux_Time_series_GympieNP_ 22June2015_5,7,9,10,11,13,14,15,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[1:1440],type = "l",xaxt='n')
mtext(side=3,"GympieNP_22June2015_5,7,9,10,11,13,14,15,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1,cex=1.4)
# Trend lines using linear filters
flux.5 <- filter(flux[1:1440],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[1:1440],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[1:1440],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[1:1440],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[1:1440],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min", line=-1,cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1,cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1,cex=1.4)
plot(flux.20, xaxt='n')
mtext(side=4,"20 min",line=-1,cex=1.4)
plot(flux.30, xaxt='n')
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
###########################
png('Flux_Time_series_GympieNP_ 23June2015_5,7,9,10,11,13,14,15,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[1441:2880],type = "l", xaxt="n")
mtext(side=4,"1 min",line=-1, cex=1.4)
mtext(side=3,"GympieNP_23June2015_5,7,9,10,11,13,14,15,17,18",
      cex = 2)

flux.5 <- filter(flux[1441:2880],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[1441:2880],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[1441:2880],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[1441:2880],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[1441:2880],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt='n')
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30,xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
##############################
png('Flux_Time_series_GympieNP_ 24June2015_5,7,9,10,11,13,14,15,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[2881:4320],type = "l", xaxt="n")
mtext(side=4,"1 min",line=-1, cex=1.4)
mtext(side=3,"GympieNP_24June2015_5,7,9,10,11,13,14,15,17,18",
      cex = 2)

flux.5 <- filter(flux[2881:4320],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[2881:4320],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[2881:4320],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[2881:4320],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[2881:4320],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
#################################
png('Flux_Time_series_GympieNP_ 25June2015_5,7,9,10,11,13,14,15,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[4321:5760],type = "l", xaxt="n")
mtext(side=3,"GympieNP_25June2015_5,7,9,10,11,13,14,15,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1, cex=1.4)
flux.5 <- filter(flux[4321:5760],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[4321:5760],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[4321:5760],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[4321:5760],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[4321:5760],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
################################
png('Flux_Time_series_GympieNP_ 26June2015_5,7,9,10,11,13,14,15,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[5761:7200],type = "l", xaxt="n")
mtext(side=3,"GympieNP_26June2015_5,7,9,10,11,13,14,15,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1, cex=1.4)
flux.5 <- filter(flux[5761:7200],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[5761:7200],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[5761:7200],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[5761:7200],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[5761:7200],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
###############################
png('Flux_Time_series_GympieNP_ 27June2015_5,7,9,10,11,13,14,15,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[7201:8640],type = "l", xaxt="n")
mtext(side=3,"GympieNP_27June2015_5,7,9,10,11,13,14,15,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1, cex=1.4)
flux.5 <- filter(flux[7201:8640], filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[7201:8640], filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[7201:8640], filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[7201:8640], filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[7201:8640], filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
#################################
png('Flux_Time_series_GympieNP_ 28June2015_5,7,9,10,11,13,14,15,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[8641:10080],type = "l", xaxt="n")
mtext(side=3,"GympieNP_28June2015_5,7,9,10,11,13,14,15,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1, cex=1.4)
flux.5 <- filter(flux[8641:10080],  filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[8641:10080], filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[8641:10080], filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[8641:10080], filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[8641:10080], filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
######################
#An alternative way to smooth is using a modified Daniel
#plot(kernapply(flux[7201:8640], 
#               kernel("modified.daniell",7)),
#               type="l")
#plot(kernapply(flux[7201:8640], 
#               kernel("modified.daniell",3)),
#              type="l")

# Determine full day acoustic flux vectors
length <- length(normIndices$BackgroundNoise)

flux.5.24 <- filter(flux[1:length], filter=rep(1/5,5)) # 5 minute
flux.10.24 <- filter(flux[1:length], filter=rep(1/10,10)) # 10 minute
flux.15.24 <- filter(flux[1:length], filter=rep(1/15,15)) # 20 minute
flux.20.24 <- filter(flux[1:length], filter=rep(1/20,20)) # half-hourly
flux.30.24 <- filter(flux[1:length], filter=rep(1/30,30)) # hourly
flux.data <- cbind(flux, flux.5.24, flux.10.24, flux.15.24,
                   flux.20.24, flux.30.24)
file <- paste("Acoustic_flux_time_series_", site, "_", 
             "22 to 28 June 2015", ".csv", sep="")
write.table(flux.data, file=file, sep = ",", qmethod = "double",
            row.names = F)