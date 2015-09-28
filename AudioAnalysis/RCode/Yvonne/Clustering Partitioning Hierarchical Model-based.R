#######################################################
# Clustering - Partitioning (kmeans & clara), hierarchical 
# (hclust & agnes), model-based (mclust) and affinity
# propogation (apcluster)
#######################################################
setwd("C:\\Work\\CSV files\\DataSet_New")

AcousticDS <- read.csv("Final_DataSet1_3July2015and14_16Sept2015GympieNP_WoondumNP.csv",
                       header=T)

ds3 <- AcousticDS[,c(3,4,6,9,10,11,13,15)]
#ds3 <- AcousticDS[,c(3,4,6,9,10,11,13,15,16)]

rm(AcousticDS)
#######################################################
# function - normalise
#######################################################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

#######################################################
# Create ds3.norm_2_98 for kmeans, clara, hclust
# a dataset normalised between 2 and 98%
#######################################################
ds3.norm_2_98 <- ds3
for (i in 1:8) {
  q1 <- unname(quantile(ds3[,i], probs = 0.02))
  q2 <- unname(quantile(ds3[,i], probs = 0.98))
  ds3.norm_2_98[,i]  <- normalise(ds3.norm_2_98[,i], 
                                  q1, q2)
}
# adjust values greater than 1 or less than 0
for (j in 1:8) {
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] > 1) ds3.norm_2_98[i,j] = 1
  }
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] < 0) ds3.norm_2_98[i,j] = 0
  }
}

#######################################################
# Create ds3.norm for mclust, apcluster
#######################################################
ds3.norm <- ds3
for (i in 1:8) {
  ds3.norm[,i]  <- normalise(ds3.norm[,i], min(ds3[,i]),max(ds3[,i]))
}

#######################################################
# PRINCIPAL COMPONENT ANALYSIS sd3
#######################################################
file <- paste("Principal Component Analysis_ds3norm_.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)

par(mar =c(2,2,4,2), cex.axis = 2.5)
PCAofIndices<- prcomp(ds3.norm)
biplot(PCAofIndices, col=c("grey80","red"), 
       cex=c(0.5,2), ylim = c(-0.03,0.03), 
       xlim = c(-0.018,0.018))
abline(h=0,v=0)
mtext(side = 3, line = 2, 
      paste("Principal Component Analysis prcomp ds3"), 
      cex = 2.5)
rm(PCAofIndices)
dev.off()

#######################################################
# Method 1A:  Partitioning by kmeans
#######################################################
file <- paste("kmeans_plots_ds3norm_2_98.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 400,
  pointsize = 4
)
par(mfrow=c(2,1), mar=c(5,5,4,2), cex.main=2, cex.axis=2,
    cex.lab=2)
# Determining the number of clusters ()
wss <- (nrow(ds3.norm_2_98)*sum(apply(ds3.norm_2_98, 2, var)))
for (i in 2:30) wss[i] <- sum(sum(kmeans(ds3.norm_2_98, 
                          centers=i)$withinss, iter.max = 100))
plot(1:30, wss, type = "b", xlab="Number of clusters", 
     ylab = "within groups sum of squares",
     main = "kmeans Within Groups Sum of Squares")
set.seed(123)
min.size <- NULL
max.size <- NULL
for (i in 2:30) {
  kmeansObj <- kmeans(ds3.norm_2_98, centers = i, iter.max = 100)
  min <- unname(min(table(kmeansObj$cluster)))
  min.size <- c(min.size, min)
  max <- unname(max(table(kmeansObj$cluster)))
  max.size <- c(max.size, max)
}

plot(2:30,c(min.size), type = "l", ylim=c(0,8000), 
     ylab = "Cluster size", xlab = "Number of clusters",
     main = "kmeans Cluster Size Range")
par(new=TRUE)
plot(2:30,c(max.size), type = "l", col = "red", 
     ylim=c(0,8000), xlab = "", ylab = "")
abline(v=6, col="red", lty=2)
abline(v=15, col="red", lty=2)
abline(v=27, col="red", lty=2)
dev.off()
# determine the minimum size of clusters to get more evenly sized 
# clusters
set.seed(123)
kmeansObj <- kmeans(ds3.norm_2_98, centers = 27, iter.max = 100)
kmeansObj$cluster
table(kmeansObj$cluster)
min.cs <- unname(min(table(kmeansObj$cluster)))
min.cs/length(ds3.norm_2_98$BackgroundNoise)*100
max.cs <- unname(max(table(kmeansObj$cluster)))
max.cs/length(ds3.norm_2_98$BackgroundNoise)*100

# Range of cluster size 6.66% to 32.84% with 6 clusters
# Range of cluster size 3.81% to 20.39% with 10 clusters
# Range of cluster size 2.37% to 18.16% with 15 clusters
# Range of cluster size 1.04% to 11.41% with 27 clusters
# Keep in mind that 1% represents 2hours 52 minutes which 
# over 12 days may represent this period of heavy rain depending
# upon the season

#######################################################
# Method 1B:  Partitioning by clara (and auxillary functions
# from pam)
# pam has a feature which identifies observations which lie 
# closest to the medoids (exemplars for each cluster)
#######################################################
library(cluster)
library(fpc)
# Determine the optimum number of clusters with the pamk function
# in fpc package
opt.no <- NULL
avg.width <- NULL
for (i in 2:30) {
  ds3pk <- pamk(ds3.norm_2_98, krange = i, criterion ="multiasw",
                usepam = F)
  nc <- ds3pk$nc # optimum number of clusters
  aw <- ds3pk$pamobject$silinfo$avg.width
  opt.no <- c(opt.no, nc)
  avg.width <- c(avg.width, aw)
}
rm(ds3pk)
plot(opt.no, avg.width, type="l", 
     main = "Average Silhouette Width (ASW) vs Opt Cluster No.",
     ylab = "ASW", xlab = "Number of Clusters")
abline(v=2, lty=2, col="red")
abline(v=5, lty=2, col="red")
abline(v=12, lty=2, col="red")
abline(v=16, lty=2, col="red")
abline(v=28, lty=2, col="red")

# From pam.object help
# Predict the best number of clusters from a range
# Note: pam() changed to clara() for large dataset
x <- ds3.norm_2_98
asw <- numeric(30)
# Note that "k=1" won't work!
for (k in 7:30)
   asw[k] <- clara(x, k) $ silinfo $ avg.width
k.best <- which.max(asw)
cat("silhouette-optimal number of clusters:", k.best, "\n")

# Another method to determine the no. of clusters is 
# the Calinski-Harabasz index - I did not use!!!
#cal.har <- NULL
#for (i in 2:30) {
#  set.seed(1234)
#  km <- kmeans(ds3.norm_2_98, i, iter.max = 100)
#  ch <- round(calinhara(ds3.norm_2_98,km$cluster),digits=2)
#  cal.har <- c(cal.har,ch)
#}

pr4 <- clara(ds3.norm_2_98, 5, sampsize = nrow(ds3.norm_2_98)/100)
str(si <- silhouette(pr4))
rm(pr4)
(ssi <- summary(si))
plot(si) # silhouette plot
plot(si, col = c("red", "green","orange","yellow",
                 "red","green","orange","yellow",
                 "red","green"))# with cluster-wise coloring

op <- par(mfrow= c(3,2), oma= c(0,0, 3, 0),
          mgp= c(1.6,.8,0), mar= .1+c(4,2,2,2))
for(k in c(2,5,12,16))
  plot(silhouette(clara(ds3.norm, k=k)), main = paste("k = ",k), do.n.k=FALSE)
mtext("CLARA(DataSet) as in Kaufman & Rousseeuw, p.101",
      outer = TRUE, font = par("font.main"), cex = par("cex.main")); frame()

# perform clara clustering with optimum clusters
dev.off()
par(mfrow=c(2,1), mar=c(2.2,3,1,1))
ds3.12.clara <- clara(ds3.norm_2_98, 12)
colours <- c("red","orange","yellow","green","blue",
             "darkorange","violet", "magenta","midnightblue",
             "sienna", "red", "orange")
#plot(ds3.12.clara, col.p = colours, which.plots = 1)
plot(ds3.12.clara, col.p=ds3.12.clara$cluster, 
     shade = T, which.plots = 1, labels = 4,
     color = T) # type of PCA
plot(ds3.12.clara$clustering) 
abline(v=c(0,4320,8640, 12970, length(ds3.norm_2_98$Snr)), 
       col="blue", lty=2)
# decision made on 24 clusters
rm(ds3.12.clara)
dev.off()

#######################################################
# Method 2a:  Hierarchical - hclust or agnes
#######################################################
require(graphics)
par(mfrow = c(1,2))
hc.fit <- hclust(dist(ds3.norm), "ward.D")
plot(hc.fit, hang = -1, cex=0.1,
     main = "Cluster Dendrogram - ward.D")

hc.fit <- hclust(dist(ds3.norm), "ward.D2")
plot(hc.fit, hang = -1, cex=0.1,
     main = "Cluster Dendrogram - ward.D2")

#######################################################
# Method 2b:  Hierarchical - agnes
#######################################################
library(cluster)
# WARNING: agnes takes many hours 
ag.d <- dist(ds3.norm_2_98[1:2880,], method = "euclidean")
ag.fit <- agnes(ag.d, method = "ward")
par(mfrow=c(2,2), mar=c(2,2,2,2))
plot(ag.fit, which.plots=2, cex=0.5, hang=-1)
#(cut_height_3 <- cutree(as.hclust(ag.fit), h = 3))
#plot(unname(cut_height_3), main = "Height =3")
(cut_height_4 <- cutree(as.hclust(ag.fit), h = 4))
plot(unname(cut_height_4), main = "Height =4")
(cut_height_5 <- cutree(as.hclust(ag.fit), h = 5))
plot(unname(cut_height_5), main = "Height =5")
(cut_height_7 <- cutree(as.hclust(ag.fit), h = 7))
plot(unname(cut_height_7), main = "Height =7")

# Clustering of full dataset
ag.d <- dist(ds3.norm_2_98, method = "euclidean")
ag.fit <- agnes(ag.d, method = "ward")
(cut_height_0.5 <- cutree(as.hclust(ag.fit), h = 0.5))
(cut_height_1 <- cutree(as.hclust(ag.fit), h = 1))
(cut_height_1.5 <- cutree(as.hclust(ag.fit), h = 1.5))
(cut_height_2 <- cutree(as.hclust(ag.fit), h = 2))
(cut_height_2.5 <- cutree(as.hclust(ag.fit), h = 2.5))
(cut_height_3 <- cutree(as.hclust(ag.fit), h = 3))
(cut_height_3.5 <- cutree(as.hclust(ag.fit), h = 3.5))
(cut_height_4 <- cutree(as.hclust(ag.fit), h = 4))
(cut_height_4.5 <- cutree(as.hclust(ag.fit), h = 4.5))
(cut_height_5 <- cutree(as.hclust(ag.fit), h = 5))
(cut_height_5.5 <- cutree(as.hclust(ag.fit), h = 5.5))
(cut_height_6 <- cutree(as.hclust(ag.fit), h = 6))
(cut_height_6.5 <- cutree(as.hclust(ag.fit), h = 6.5))
(cut_height_7 <- cutree(as.hclust(ag.fit), h = 7))
(cut_height_7.5 <- cutree(as.hclust(ag.fit), h = 7.5))
(cut_height_8 <- cutree(as.hclust(ag.fit), h = 8))
(cut_height_8.5 <- cutree(as.hclust(ag.fit), h = 8.5))
(cut_height_9 <- cutree(as.hclust(ag.fit), h = 9))
(cut_height_9.5 <- cutree(as.hclust(ag.fit), h = 9.5))
(cut_height_10 <- cutree(as.hclust(ag.fit), h = 10))

cut_heights <- cbind(cut_height_0.5, cut_height_1, cut_height_1.5,
                     cut_height_2, cut_height_2.5, cut_height_3,
                     cut_height_3.5, cut_height_4, cut_height_4.5,
                     cut_height_5, cut_height_5.5, cut_height_6,
                     cut_height_6.5, cut_height_7, cut_height_7.5, 
                     cut_height_8, cut_height_8.5, cut_height_9, 
                     cut_height_9.5, cut_height_10)

#######################################################
# Method 3:  Model-based - Mclust
#######################################################
# WARNING: mclust takes many hours 
library(mclust)
mc.fit <- Mclust(ds3.norm, G=1:50) 
# crashed at 1:50, 1:30, 1:20 so tried 1:15 and 
# defining # rows and # col 

summary(mc.fit) 

mclust.list <- unname(mc.fit$classification)
png('Clusterplot_ds3norm_1_50.png', 
    width = 1500, height = 1200, units = "px") 
plot(mclust.list)
dev.off()

write.table(mclust.list, 
            file="mclustlist_ds3norm_1_50.csv", 
            row.names = F, sep = ",")

mean <- mc.fit$parameters$mean
write.table(mean, 
            file="mclust_ds3norm_1_50_mean.csv", 
            row.names = F, sep = ",")

sink('mclust_variance_ds3norm_1_50_.csv')
mc.fit$parameters$variance
sink()

sigma <- mc.fit$parameters$variance$sigma
write.table(sigma, 
            file="mclust_sigma_ds3norm_1_50.csv", 
            row.names = F, sep = ",")

png('mclust_BIC_plot_ds3norm_1_50.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "BIC")
dev.off()

png('mclust_Density_plot_ds3norm_1_50.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "density")
dev.off()

png('mclust_Classification_plot_ds3norm_1_50.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "classification")
dev.off()

# Density plot # these take a while these give nice density plots of 
# the normalised data

png('densBGR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densBackgr <- densityMclust(ds3.norm$BackgroundNoise)
plot(densBackgr, data = ds3.norm$BackgroundNoise, what = "density")
dev.off()

png('densEPS_plot.png', 
    width = 1500, height = 1200, units = "px") 
densEPS <- densityMclust(ds2.norm$EventsPerSecond)
plot(densEPS, data = ds3.norm$EventsPerSecond, what = "density")
dev.off()

png('densAvSNR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAvSNR <- densityMclust(ds2.norm$AvgSnrOfActiveFrames)
plot(densAvSNR, data = ds2.norm$AvgSnrOfActiveFrames, what = "density")
dev.off()

png('densAccComp_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAcousticComp <- densityMclust(ds3.norm$AcousticComplexity)
plot(densAcousticComp, data = ds3.norm$AcousticComplexity, what = "density")
dev.off()

png('densEntCOV_plot.png', 
    width = 1500, height = 1200, units = "px") 
densEntCoV <- densityMclust(ds3.norm$EntropyOfCoVSpectrum)
plot(densEntCoV, data = ds3.norm$EntropyOfCoVSpectrum, what = "density")
dev.off()

png('densLowFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densLowFrCov <- densityMclust(ds3.norm$LowFreqCover)
plot(densLowFrCov, data = ds3.norm$LowFreqCover, what = "density")
dev.off()

png('densMidFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densMidFrCov <- densityMclust(ds3.norm$MidFreqCover)
plot(densMidFrCov, data = ds3.norm$MidFreqCover, what = "density")
dev.off()

png('densHighFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densHighFrCov <- densityMclust(ds3.norm$HighFreqCover)
plot(densHighFrCov, data = ds3.norm$HighFreqCover, what = "density")
dev.off()

png('densEntPS_plot.png', 
    width = 1500, height = 1200, units = "px") 
densEntPs <- densityMclust(ds3.norm$EntropyOfPeaksSpectrum)
plot(densEntPs, data = ds3.norm$EntropyOfPeaksSpectrum, what = "density")
dev.off()

# WARNING: mclust takes many hours 
library(mclust)
mc2.fit <- Mclust(ds2.norm, G=1:30)
summary(mc2.fit) 

mclust.list2 <- unname(mc2.fit$classification)
png('Clusterplot_ds2.png', 
    width = 1500, height = 1200, units = "px") 
plot(mclust.list2)
dev.off()

write.table(mclust.list2, 
            file="mclustlist_ds2.csv", 
            row.names = F, sep = ",")

mean <- mc2.fit$parameters$mean
write.table(mean, 
            file="mclust_ds2_mean.csv", 
            row.names = F, sep = ",")

sink('mclust_ds2_variance.csv')
mc2.fit$parameters$variance
sink()

sigma <- mc2.fit$parameters$variance$sigma
write.table(sigma, 
            file="mclust_ds2_sigma.csv", 
            row.names = F, sep = ",")

png('mclust_ds2_BIC_plot.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc2.fit, what = "BIC")
dev.off()

png('mclust_ds2_Density_plot.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc2.fit, what = "density")
dev.off()

png('mclust_ds2_Classification_plot.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc2.fit, what = "classification")
dev.off()

