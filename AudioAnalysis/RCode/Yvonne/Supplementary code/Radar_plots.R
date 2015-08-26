setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\2015Jul01-120417\\Woondum3\\")

indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")
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

# Pre-processing transformation of 
# and Temporal Entropy #14 to correct the long
# heavy tail 
indices[,14] <- sqrt(indices[,14])

normIndices <- indices

# normalise variable columns
normIndices[,4]  <- normalise(indices[,4], -50,-10)   # AverageSignalAmplitude
normIndices[,5]  <- normalise(indices[,5], -40,-20)   # BackgroundNoise
normIndices[,6]  <- normalise(indices[,6],  0, 50)    # Snr
normIndices[,7]  <- normalise(indices[,7],  3, 7)     # AvSnrofActive Frames
normIndices[,8]  <- normalise(indices[,8],  0, sqrt(1))     # Activity 
normIndices[,9]  <- normalise(indices[,9],  0, 2)     # EventsPerSecond
normIndices[,10] <- normalise(indices[,10], 0, 0.35)  # HighFreqCover
normIndices[,11] <- normalise(indices[,11], 0, 0.4)   # MidFreqCover
normIndices[,12] <- normalise(indices[,12], 0, 0.5)   # LowFreqCover
normIndices[,13] <- normalise(indices[,13], 0.4,0.55) # AcousticComplexity
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
indices <- normIndices

#model <- m.km <- kmeans(indices[c(4:20)], 30)
model <- m.kms <- kmeans(indices[c(5,7,9,10,11,13,14,15,17,18)], nclust)

model$size
model$centers
model$iter
model$ifault
#dsc <- scale(indices[,c(5,6,7,9,10,12,13,14,17)])
#dsc <- scale(indices[,c(4:18)])
#attr(dsc, "scaled:center") # the mean of each variable
#attr(dsc, "scaled:scale") # the standard deviation of each variable

library(ggplot2)
library(reshape2)
dscm <- melt(model$centers)
names(dscm) <- c("Cluster", "Variable", "Value")
dscm$Cluster <- factor(dscm$Cluster)
#dscm$Order <- as.vector(sapply(1:length(dscm), rep, 18))
dscm$Order <- as.vector(sapply(1:10, rep, 30))

#file <- "Radar_plot.png"
png('Radar_plot_selected_indices.png', width=1500,height=1500,units="px")  
p <- ggplot(subset(dscm, Cluster %in% 1:30),
            aes(x=reorder(Variable, Order),
                y=Value, group=Cluster, colour=Cluster))
p <- p + coord_polar()
p <- p + geom_point()
p <- p + geom_path()
p <- p + labs(x=NULL, y=NULL)
p <- p + theme(axis.ticks.y=element_blank(), 
               axis.text.y = element_blank())
p <- p + theme(axis.text = element_text(size = 20)) # changes axis labels

p
print(p)
dev.off()
# The function clusterboot() from fpc (Hennig, 2014) provides a 
# convenient tool to identify robust clusters.
# Jaccard similarity values of greater than 0.75 are stable and
# above 0.85 very stable.  Values of 0.6 or below "should not
# be trusted".  Stable clusters does not indicate valid clusters.
library(fpc)
model <- m.kmcb <- clusterboot(indices[,c(5,6,7,9,10,12,13,14,17)],
                     scaling = T,
                     clustermethod=kmeansCBI,
                     bootmethod=c("boot","subset"),
                     B = 50,
                     bscompare = T,
                     runs=10,
                     krange=10,
                     showplots = F,
                     seed=12)
model
str(model)
print(model)
par(mar=c(0,0,0,0))
plot(model)

# Evaluate model quality
model <- kmeans(scale(indices[,c(5,6,7,9,10,12,13,14,17)]),3)
model$totss
model$withinss
model$tot.withinss

# Scree plot
crit <- vector()
nk <- 1:45
t <- c(5,6,7,9,10,12,13,14,17)
for (k in nk)
{
m <- kmeans(scale(indices[,t]), k, iter.max = 20)
crit <- c(crit, sum(m$withinss))
}
crit
plot(crit)

# Principal Component Analysis
summary(pc.cr <- princomp(indices[t], cor = TRUE))
loadings(pc.cr)  # note that blank entries are small but not zero
## The signs of the columns are arbitrary
plot(pc.cr) # shows a screeplot.
biplot(pc.cr, cex=c(0.1,1))


m <- kmeans(scale(ds[numi]), 5)
ic <- intCriteria(as.matrix(ds[numi]), m$cluster, "all")
names(ic)