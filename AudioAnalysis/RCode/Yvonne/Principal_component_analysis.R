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
normIndices[,2]  <- normalise(indices[,2],  0, 2)     # HighAmplitudeIndex
normIndices[,3]  <- normalise(indices[,3],  0, 1)     # ClippingIndex
normIndices[,4]  <- normalise(indices[,4], -50,-10)    # AverageSignalAmplitude
normIndices[,5]  <- normalise(indices[,5], -50,-10)    # BackgroundNoise
normIndices[,6]  <- normalise(indices[,6],  0, 50)    # Snr
normIndices[,7]  <- normalise(indices[,7],  3, 10)    # AvSnrofActive Frames
normIndices[,8]  <- normalise(indices[,8],  0, 1)     # Activity 
normIndices[,9]  <- normalise(indices[,9],  0, 2)     # EventsPerSecond
normIndices[,10] <- normalise(indices[,10], 0, 0.5)   # HighFreqCover
normIndices[,11] <- normalise(indices[,11], 0, 0.5)   # MidFreqCover
normIndices[,12] <- normalise(indices[,12], 0, 0.5)   # LowFreqCover
normIndices[,13] <- normalise(indices[,13], 0.4,0.7)   # AcousticComplexity
normIndices[,14] <- normalise(indices[,14], 0, 0.3)   # TemporalEntropy
normIndices[,15] <- normalise(indices[,15], 0, 0.7)   # EntropyOfAverageSpectrum
normIndices[,16] <- normalise(indices[,16], 0, 1)     # EntropyOfVarianceSpectrum
normIndices[,17] <- normalise(indices[,17], 0, 1)     # EntropyOfPeaksSpectrum
normIndices[,18] <- normalise(indices[,18], 0, 0.7)   # EntropyOfCoVSpectrum
normIndices[,19] <- normalise(indices[,19], -0.8, 1)   # NDSI
normIndices[,20] <- normalise(indices[,20], 0, 15)     # SptDensity

# adjust values greater than 1 or less than 0
for (j in 4:20){
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
normIndices <- cbind(normIndices[,c(4:20)], indices[,37])  
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
normIndices.pca <- prcomp(normIndices[,1:17], 
                          scale. = F)
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

#fooPlot <- function(x, main, ...) {
#  if(missing(main))
#    main <- deparse(substitute(x))
#  plot(x, main = main, ...)
#}

#  set.seed(42)
#dat <- data.frame(x = rnorm(1:10), y = rnorm(1:10))
#fooPlot(dat, col = "red")

#### Plotting Principal Component Plots with base plotting system
# change file name when necessary
#png('pca_plot PC2_PC3.png', width=1500, height=1200, units="px") 
PrinComp_X_axis <- "PC2"
PrinComp_Y_axis <- "PC3"
first <- 2  # change this and values in plot function below!!! to match PC# 
second <- 3  # change this!!! to match PC#
arrowScale <- 0.75 # increase/decrease this to adjust arrow length
summ <- summary(normIndices.pca)
rotate <- unname(summ$rotation)
labels <- names(normIndices[1:length(summ$center)])

mainHeader <- paste (site, date, PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
normIndices <- within(normIndices, levels(`indices[, 37]`) <- c("red","orange","yellow","green","blue","violet"))
par(mar=c(6,6,4,4))
plot(normIndices$PC2,normIndices$PC3,  # Change these!!!!! 
     col=as.character(normIndices$`indices[, 37]`), 
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
legend('topright', hours, pch=19, col=c('red','orange','yellow',
        'green','blue','violet'), bty='n', cex=2)
dev.off()

####### An Alternative - Saving a PCA plot produced in ggplot ################
file <- paste("Principal Component Analysis_adj_ranges_ggbiplot", site, 
              "_", date, ".png", sep = "")

library(ggbiplot)
normIndices.pca <- prcomp(normIndices[,1:17], 
                          scale. = F)

# Initiate initial plot 
#p <- NULL

#p <- print(ggbiplot(normIndices.pca, obs.scale = 1, 
#           var.scale = 1, groups = normIndices$`indices[, 37]`,
#           ellipse = TRUE, circle = F), size = 0.3)
#png('pca_biplot.png', width=1500,height=1500,units="px")  

g <- ggbiplot(normIndices.pca, obs.scale = 1, var.scale = 1,
              groups = normIndices$`indices[, 37]`, 
              ellipse = TRUE, circle = TRUE,
              varname.size=20,
              labels=normIndices$`indices[, 37]`,
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

