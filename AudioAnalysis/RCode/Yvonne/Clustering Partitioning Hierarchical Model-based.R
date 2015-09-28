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

dev.off()

#######################################################
# Method 1A:  Partitioning by kmeans
#######################################################
# Determining the number of clusters ()
wss <- (nrow(ds3.norm_2_98)*sum(apply(ds3.norm_2_98, 2, var)))
for (i in 2:30) wss[i] <- sum(sum(kmeans(ds3.norm_2_98, centers=i)$withinss))
plot(1:30, wss, type = "b", xlab="Number of clusters", 
     ylab = "within groups sum of squares")
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
     ylab = "Cluster size", xlab = "Cluster number",
     main = "kmeans Cluster Size Range")
par(new=TRUE)
plot(2:30,c(max.size), type = "l", col = "red", 
     ylim=c(0,8000), xlab = "", ylab = "")
abline(v=6, col="red", lty=2)
abline(v=15, col="red", lty=2)
abline(v=27, col="red", lty=2)

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
# use pamk to predict number of clusters
#ds1pk <- pamk(ds1.norm, krange = 10:100, criterion ="multiasw",
#              usepam = F)
#ds1pk$nc # optimum number of clusters
#ds1pk$pamobject$silinfo$avg.width
#ds1pk$pamobject$silinfo$clus.avg.widths
#ds1pk$pamobject$i.med
h# [1] 10 when tested between 10:100 for ds1.norm
# $avg.width [1] 0.303939
# $clus.avg.widths
# [1] 0.34850246 0.14569529 0.05002397 0.48073611 
# [4] 0.22814917 0.73292138 0.60045972 0.32395219 
# [7] 0.26941398 0.24524659
opt.no <- NULL
avg.width <- NULL
for (i in 15:40) {
  ds1pk <- pamk(ds1.norm, krange = i, criterion ="multiasw",
                usepam = F)
  nc <- ds1pk$nc # optimum number of clusters
  aw <- ds1pk$pamobject$silinfo$avg.width
  opt.no <- c(opt.no, nc)
  avg.width <- c(avg.width, aw)
}

opt.no <- NULL
avg.width <- NULL
for (i in 2:24) {
  ds3pk <- pamk(ds3.norm, krange = i, criterion ="multiasw",
                usepam = F)
  nc <- ds3pk$nc # optimum number of clusters
  aw <- ds3pk$pamobject$silinfo$avg.width
  opt.no <- c(opt.no, nc)
  avg.width <- c(avg.width, aw)
}

# note value was higher for 10 clusters
plot(opt.no, avg.width, type="l", main = "Average width vs Opt Cluster No.")
abline(v=2, lty=2, col="red")
abline(v=4, lty=2, col="red")
abline(v=10, lty=2, col="red")
abline(v=13, lty=2, col="red")
abline(v=17, lty=2, col="red")

#code from pam.object help
# note change of pam to clara due to large
# dataset
x <- ds3.norm
asw <- numeric(20)
# Note that "k=1" won't work!
for (k in 3:20)
   asw[k] <- clara(x, k) $ silinfo $ avg.width
k.best <- which.max(asw)
cat("silhouette-optimal number of clusters:", k.best, "\n")
 

# Another method to determine the no. of clusters is 
# the Calinski-Harabasz index
cal.har <- NULL
for (i in 2:20) {
  set.seed(1234)
  km <- kmeans(ds3,i)
  ch <- round(calinhara(ds3,km$cluster),digits=2)
  cal.har <- c(cal.har,ch)
}

plot(2:20, cal.har, type = "l")
abline(v=7, lty=2, col="red")

ds3pk <- pamk(ds3.norm, krange = 10:50, criterion ="multiasw",
              usepam = F)
ds3pk$nc # optimum number of clusters
ds3pk$pamobject$silinfo$avg.width
ds3pk$pamobject$silinfo$clus.avg.widths
plot(ds3pk$pamobject, nmax.lab = 100)

pr4 <- clara(ds3.norm, 5, sampsize = nrow(ds3.norm)/100)
str(si <- silhouette(pr4, full = T))
(ssi <- summary(si))
plot(si) # silhouette plot
plot(si, col = c("red", "green","orange","yellow",
                 "red","green","orange","yellow",
                 "red","green"))# with cluster-wise coloring

op <- par(mfrow= c(3,2), oma= c(0,0, 3, 0),
          mgp= c(1.6,.8,0), mar= .1+c(4,2,2,2))
for(k in c(2,4,7,12))
  plot(silhouette(clara(ds3.norm, k=k)), main = paste("k = ",k), do.n.k=FALSE)
mtext("CLARA(DataSet) as in Kaufman & Rousseeuw, p.101",
      outer = TRUE, font = par("font.main"), cex = par("cex.main")); frame()
#dev.off()
# [1] 10 when tested between 10:100 for ds1.norm
# $avg.width [1] 0.303939
#[1] 0.34850246 0.14569529 0.05002397 0.48073611 0.22814917
#[6] 0.73292138 0.60045972 0.32395219 0.26941398 0.24524659

# [1]  when tested between 20:100 for ds1.norm
# $avg.width [1] 22
# $clus.avg.widths [1] 0.1919076
#[1]  0.298682756  0.007886176 -0.030908805  0.019389666  0.343382397
#[6]  0.287502927  0.000000000  0.224976961  0.498949608  0.232327207
#[11]  0.123952792  0.258290524  0.314491017  0.000000000  0.479141721
#[16]  0.100742293  0.297011855  0.000000000  0.018075021  0.429723882
#[21]  0.237040180  0.000000000

# perform clara clustering with optimum clusters
ds3.10.clara <- clara(ds3.norm, 10)
colours <- c("red","orange","yellow","green","blue",
             "darkorange","violet", "magenta","midnightblue",
             "sienna")
plot(ds3.10.clara, col.p = colours, which.plots = 1)
plot(ds3.10.clara, col.p=ds3.10.clara$cluster, 
     shade = T, which.plots = 1, labels = 4,
     color = T) # type of PCA
plot(ds3.10.clara$clustering) 
# decision made on 24 clusters

#######################################################
# Method 2:  Hierarchical either hclust or agnes
# pam has a feature which identifies observations which lie 
# closest to the medoids (exemplars for each cluster)
#######################################################
hc <- hclust(dist(USArrests), "ward.D")
plot(hc)
plot(hc, hang = -1)

hc.fit <- hclust(dist(ds3.norm), "complete")
plot(hc.fit, hang = -1, cex=0.1)
# I tried "ave" it works
# ward.D did not work - requires too much memory
# complete worked faster than ave
# single crashed
# median appeared confused
# mcquitty looked very similar to another
# ward.D2 - fatal error when plotting

library(cluster)
# WARNING: agnes takes many hours 
ag.d <- dist(ds3.norm[1:4000,], method = "euclidean")
ag.fit <- agnes(ag.d)
plot(ag.fit, which.plots=2, cex=0.5, hang=-1)

#rm(ag.d)

# Method 3:  Model-based mclust
# WARNING: mclust takes many hours 
library(mclust)
mc.fit <- Mclust(ds1.norm[,1:9], G=50:70) 
# crashed at 1:50, 1:30, 1:20 so tried 1:15 and 
# defining # rows and # col 

summary(mc.fit) 

mclust.list <- unname(mc.fit$classification)
png('Clusterplot_ds1_50_70.png', 
    width = 1500, height = 1200, units = "px") 
plot(mclust.list)
dev.off()

write.table(mclust.list, 
            file="mclustlist_ds1_50_70.csv", 
            row.names = F, sep = ",")

mean <- mc.fit$parameters$mean
write.table(mean, 
            file="mclust_ds1_50_70_mean.csv", 
            row.names = F, sep = ",")

sink('mclust_variance_ds1_50_70_.csv')
mc.fit$parameters$variance
sink()

sigma <- mc.fit$parameters$variance$sigma
write.table(sigma, 
            file="mclust_sigma_ds1_50_70.csv", 
            row.names = F, sep = ",")

png('mclust_BIC_plot_ds1_50_70.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "BIC")
dev.off()

png('mclust_Density_plot_ds1_50_70.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "density")
dev.off()

png('mclust_Classification_plot_ds1_50_70.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "classification")
dev.off()

# Density plot # these take a while these give nice density plots of 
# the normalised data

png('densBGR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densBackgr <- densityMclust(ds1.norm$BackgroundNoise)
plot(densBackgr, data = ds1.norm$BackgroundNoise, what = "density")
dev.off()

png('densEPS_plot.png', 
    width = 1500, height = 1200, units = "px") 
densEPS <- densityMclust(ds2.norm$EventsPerSecond)
plot(densEPS, data = ds1.norm$EventsPerSecond, what = "density")
dev.off()

png('densAvSNR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAvSNR <- densityMclust(ds2.norm$AvgSnrOfActiveFrames)
plot(densAvSNR, data = ds2.norm$AvgSnrOfActiveFrames, what = "density")
dev.off()

png('densAccComp_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAcousticComp <- densityMclust(ds1.norm$AcousticComplexity)
plot(densAcousticComp, data = ds1.norm$AcousticComplexity, what = "density")
dev.off()

png('densEntCOV_plot.png', 
    width = 1500, height = 1200, units = "px") 
densEntCoV <- densityMclust(ds1.norm$EntropyOfCoVSpectrum)
plot(densEntCoV, data = ds1.norm$EntropyOfCoVSpectrum, what = "density")
dev.off()

png('densLowFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densLowFrCov <- densityMclust(ds1.norm$LowFreqCover)
plot(densLowFrCov, data = ds1.norm$LowFreqCover, what = "density")
dev.off()

png('densMidFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densMidFrCov <- densityMclust(ds1.norm$MidFreqCover)
plot(densMidFrCov, data = ds1.norm$MidFreqCover, what = "density")
dev.off()

png('densHighFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densHighFrCov <- densityMclust(ds1.norm$HighFreqCover)
plot(densHighFrCov, data = ds1.norm$HighFreqCover, what = "density")
dev.off()

png('densEntPS_plot.png', 
    width = 1500, height = 1200, units = "px") 
densEntPs <- densityMclust(ds1.norm$EntropyOfPeaksSpectrum)
plot(densEntPs, data = ds1.norm$EntropyOfPeaksSpectrum, what = "density")
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

