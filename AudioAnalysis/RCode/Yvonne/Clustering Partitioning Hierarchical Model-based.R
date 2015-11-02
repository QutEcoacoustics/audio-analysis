#######################################################
# 2 October 2015
# This code performs two experiments on two different datasets
#
# EXPERIMENT 1 
# Works with a four (4) day dataset from the 22 to 25 June 2015 from
# the Gympie NP site
# The aim of this experiment is to compare three major clustering 
# techniques (PARTITIONING - kmeans, HIERARCHICAL - hclust, agnes and
# MODEL-BASED - mclust) in order to determine which technique is best
# for characterising a rain-event.

# EXPERIMENT 2
# Works with a twelve (12) day dataset from the two sites Gympie NP and 
# Woondum3.  Dates are:  30/07/2015; 31/07/2015; 1/08/2015; 31/08/2015;
# 1/09/2015; 4/09/2015.
# The dataset forms four groups split by sites and months (July and August)
# The aim is to compare three major clustering techniques (listed above)
# and to determine which is best for clustering acoustic data
#
#######################################################
# EXPERIMENT 2 
#######################################################
# Clustering - Partitioning (kmeans & clara), hierarchical (hclust 
# & agnes), model-based (mclust) and affinity propogation (apcluster)
#setwd("C:\\Work\\CSV files\\DataSet_New")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a")
setwd("C:\\Work\\CSV files\\FourMonths")
#setwd("C:\\Work\\CSV files\\DataSet_Exp2_new")
#setwd("C:\\Work\\CSV files\\DataSet_Exp2_new_new")

#AcousticDS <- read.csv("DataSet_Exp2_4_5July_31July_1Aug_31Aug-1Sept_2015.csv", header=T)
AcousticDS <- read.csv("Final DataSet 30_31July_1Aug_31Aug_1_4Sept.csv", header=T)
AcousticDS <- read.csv("final_dataset_22June2015_11 Oct2015.csv", header = T)
#AcousticDS <- read.csv("Dataset_30_31July_1Aug2015_4_9_10Sept2015.csv", header=T)
#AcousticDS <- read.csv("Dataset_30_31July_1Aug2015_9_10_12Sept2015.csv", header = T)
#ds3 <- AcousticDS[,c(2:18)]
ds3 <- AcousticDS[,c(3,4,7,10,11,15,16)]
ds3 <- AcousticDS[,c(3,4,7,9,10,11,15,16)]
#ds3 <- AcousticDS[,c(3,5,7,10,11,15,16)]
#ds3 <- AcousticDS[,c(3,4,7,10,11,13,16)]

# PCA type analysis
library(psych)
ic.out <- iclust(AcousticDS[,4:10])
ic.out7 <- iclust(AcousticDS[,3:18],nclusters = 7)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust") 
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

for (i in 1:length(ds3)) {
    q1 <- unname(quantile(ds3[,i], probs = 0.02, na.rm = TRUE))
    q2 <- unname(quantile(ds3[,i], probs = 0.98, na.rm = TRUE))
    ds3.norm_2_98[,i]  <- normalise(ds3.norm_2_98[,i], 
                                    q1, q2)  
}
# adjust values greater than 1 or less than 0
for (j in 1:length(ds3)) {
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] > 1 & !is.na(ds3.norm_2_98[i,j])) 
      ds3.norm_2_98[i,j] = 1
  }
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] < 0 & !is.na(ds3.norm_2_98[i,j])) 
      ds3.norm_2_98[i,j] = 0
  }
}

# Create ds3.norm for mclust, apcluster where the data is normalised 
# values between zero and one using minimum and maximum values

ds3.norm <- ds3
for (i in 1:length(ds3)) {
    ds3.norm[,i]  <- normalise(ds3.norm[,i], min(ds3[,i], na.rm = TRUE),
                               max(ds3[,i], na.rm = TRUE))
}

#######################################################
# Generate and save the Correlation Matrix 
#######################################################
AcousticDS_noNA <- AcousticDS[complete.cases(AcousticDS), ]
a <- abs(cor(AcousticDS_noNA[,2:18][,unlist(lapply(AcousticDS_noNA[,2:18], 
                                               is.numeric))]))
write.table(a, file = paste("Correlation_matrix_Exp2a.csv",sep=""), 
            col.names = NA, qmethod = "double", sep = ",")

#######################################################
# PRINCIPAL COMPONENT ANALYSIS sd3
#######################################################
file <- paste("Principal Component Analysis_ds3norm.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
par(mar =c(2,2,4,2), cex.axis = 2.5)
ds3.norm_noNA <- ds3.norm[complete.cases(ds3.norm), ]
PCAofIndices<- prcomp(ds3.norm_noNA)
biplot(PCAofIndices, col=c("grey80","red"), 
       cex=c(0.5,1))#, ylim = c(-0.025,0.02), 
       #xlim = c(-0.025,0.02))
abline(h=0,v=0)
mtext(side = 3, line = 2, 
      paste("Principal Component Analysis prcomp ds3"), 
      cex = 2.5)
rm(PCAofIndices)
dev.off()

#######################################################
# Method 1A:  Partitioning by kmeans
#######################################################
file <- paste("kmeans_plots_ds3norm_2_98_Exp2.png", sep = "")

png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 400,
  pointsize = 4
)
ds3.norm_2_98noNA <- ds3.norm_2_98[complete.cases(ds3.norm_2_98), ]
par(mfrow=c(2,1), mar=c(5,7,2,11), cex.main=2, 
    cex.axis=2, cex.lab=2)

# Determining the number of clusters ()
wss <- (nrow(ds3.norm_2_98noNA)*sum(apply(ds3.norm_2_98noNA, 2, var)))
for (i in 2:50) {
  set.seed(123)
  wss[i] <- sum(sum(kmeans(ds3.norm_2_98noNA, 
                    centers=i, iter.max = 100)$withinss))
}

plot(1:50, wss, type = "b", xlab="Number of clusters", 
     ylab = "within groups sum of squares",
     main = "kmeans Within Groups Sum of Squares - Exp2")

min.size <- NULL
max.size <- NULL
variance <- NULL
clusters <- NULL

for (i in 2:50) {
  set.seed(123)
  kmeansObj <- kmeans(ds3.norm_2_98noNA, centers = i, iter.max = 100)
  min <- unname(min(table(kmeansObj$cluster)))
  min.size <- c(min.size, min)
  max <- unname(max(table(kmeansObj$cluster)))
  max.size <- c(max.size, max)
  vari <- var(unname(table(kmeansObj$cluster)))
  variance <- c(variance, vari)
  clust <- kmeansObj$cluster
  clusters <- cbind(clusters, clust)
}

column.names <- NULL
for (i in 2:(length(variance)+1)) {
  col.names <- paste("clusters_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(clusters) <- column.names

write.csv(clusters, file = "kmeans_clust.csv")
kmean_clust <- read.csv("kmeans_clust.csv", header=T)
#plot(2:50,c(min.size), type = "l", ylim=c(0,8000), 
#     ylab = "Cluster size", xlab = "Number of clusters",
#     main = "kmeans Cluster Size Range")
#par(new=TRUE)

plot(2:50,c(max.size), type = "l", col = "red", 
     ylim=c(0,9000), xlab = "", 
     ylab = "",las=1, xlim = c(0,50))
mtext("Maximum cluster size",side=2,
      col="black",line=5, cex=2)
par(new=TRUE)
plot(2:50, c(variance), type = "l", col="blue",
     ylab = "", xlim = c(0,50),
     yaxt='n', xaxt='n',xlab = "")
axis(side=4, at = pretty(range(c(variance))),
     col = "blue",col.axis="blue",las=1)
mtext("Variance",side=4,col="blue",line=9, cex=2)
abline(v=9, lty=2, col="red")
abline(v=18, lty=2, col="red")
abline(v=28, lty=2, col="red")
legend('topright', c("Maximum cluster size","Variance"), 
     lty=1, col=c('red', 'blue'),cex=2)
dev.off()
# calculate the variance

# determine the minimum size of clusters to get more evenly sized 
# clusters
set.seed(123)
kmeansObj <- kmeans(ds3.norm_2_98noNA, centers = 28, iter.max = 100)
kmeansObj$cluster
table(kmeansObj$cluster)
min.cs <- unname(min(table(kmeansObj$cluster)))
min.cs/length(ds3.norm_2_98noNA$BackgroundNoise)*100
max.cs <- unname(max(table(kmeansObj$cluster)))
max.cs/length(ds3.norm_2_98noNA$BackgroundNoise)*100

# Range of cluster size 7.34% to 20.63% with 8 clusters
# Range of cluster size 3.24% to 13.12% with 16 clusters
# Range of cluster size 1.54% to 8.42% with 26 clusters

# Keep in mind that 1% represents 2hours 52 minutes which 
# over 12 days may represent this period of heavy rain depending
# upon the season

file <- paste("kmeans9_18_28_plots_ds3norm_2_98_Exp2.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 400,
  pointsize = 4
)
par(mfrow=c(3,1), mar=c(1,3,1,3), oma=c(4,3,3,3), 
    cex.axis=2, cex.main=3)
plot(kmean_clust$clusters_9, xaxt="n", 
     cex.axis=2)
mtext(side = 3,line = 1.2, "kmeans - Experiment 2 (30July_31July_1Aug_31Aug_1Sept_4Sept2015)", cex = 2.2)
mtext(side = 4, line = 2, "clusters_9", cex = 2)
plot(kmean_clust$clusters_18, xaxt="n", cex.axis=2)
mtext(side = 4, line = 2, "clusters_18", cex = 2)
plot(kmean_clust$clusters_28, cex.axis=2)
mtext(side = 4, line = 2, "clusters_28", cex = 2)
dev.off()

#######################################################
# Method 1B:  Partitioning by Clara (with auxillary functions
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
for (i in 2:50) {
  ds3pk <- pamk(ds3.norm_2_98, krange = i, criterion ="multiasw",
                usepam = F)
  nc <- ds3pk$nc # optimum number of clusters
  aw <- ds3pk$pamobject$silinfo$avg.width
  opt.no <- c(opt.no, nc)
  avg.width <- c(avg.width, aw)
}
rm(ds3pk)
png('ASW_pamk_plot_ds3norm_2_98.png', 
    width = 1500, height = 1200, units = "px") 
plot(opt.no, avg.width, type="l", 
     main = "Average Silhouette Width (ASW) vs Opt Cluster No.",
     ylab = "ASW", xlab = "Number of Clusters")
abline(v=4, lty=2, col="red")
abline(v=8, lty=2, col="red")
abline(v=12, lty=2, col="red")
abline(v=16, lty=2, col="red")

dev.off()
# From pam.object help
# Predict the best number of clusters from a range
# Note: pam() changed to clara() for large dataset
x <- ds3.norm_2_98
asw <- numeric(50)
# Note that "k=1" won't work!
for (k in 2:50)
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

pr4 <- clara(ds3.norm_2_98, 16, sampsize = nrow(ds3.norm_2_98)/100)
str(si <- silhouette(pr4))
rm(pr4)
(ssi <- summary(si))
plot(si) # silhouette plot
plot(si, col = c("red", "green","orange","grey40", #))#,
                 "red","green","orange","yellow",
                 "red","green", "grey20")) #, "pink", "sienna", "magenta"))# with cluster-wise coloring

op <- par(mfrow= c(3,2), oma= c(0,0, 3, 0),
          mgp= c(1.6,.8,0), mar= .1+c(4,2,2,2))
for(k in c(2,4,8,12,16))
  plot(silhouette(clara(ds3.norm_2_98, k=k)), main = paste("k = ",k), do.n.k=FALSE)
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
# Method 2a:  Hierarchical - hclust
#######################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp3a\\Hierarchical\\")
#setwd("C:\\Work\\CSV files\\DataSet_Exp2_new\\Hierarchical\\")

require(graphics)
ds3.norm_2_98 <- cbind(ds3.norm_2_98, AcousticDS$rec.time)
dist.hc <- dist(ds3.norm_2_98)
#dist.hc <- dist(ds3.norm_2_98)
#hc.fit <- hclust(dist.hc, "average")
hc.fit.ward <- hclust(dist.hc, "ward.D2")
hc.fit.average <- hclust(dist.hc, "average")

hc.fit.ward.10 <- cutree(hc.fit.ward, k = 10)
hc.fit.ward.15 <- cutree(hc.fit.ward, k = 15)
hc.fit.ward.20 <- cutree(hc.fit.ward, k = 20)
hc.fit.ward.25 <- cutree(hc.fit.ward, k = 25)

hc.fit.average.10 <- cutree(hc.fit.average, k = 10)
hc.fit.average.15 <- cutree(hc.fit.average, k = 15)
hc.fit.average.20 <- cutree(hc.fit.average, k = 20)
hc.fit.average.25 <- cutree(hc.fit.average, k = 25)

set.hc.fit <- cbind(hc.fit.average.10, hc.fit.average.15,
                hc.fit.average.20, hc.fit.average.25,hc.fit.ward.10, 
                hc.fit.ward.15, hc.fit.ward.20, hc.fit.ward.25)
write.csv(set.hc.fit, "hc_fit_set_cutree_k.csv", row.names = F)

png("hclust_average_cutree_k_ds3norm_2_98.png", width = 1500, 
    height = 1200, units = "px")
par(mfrow=c(4,1), mar=c(3,3,1,1), oma=c(3,2,3,1), cex.main=3,
    cex.axis=2)
plot(hc.fit.average.10, xaxt="n")
mtext("hclust_cutree_average_k_10", side = 3,line = 1, cex = 2)
plot(hc.fit.average.15, xaxt="n")
mtext("hclust_cutree_average_k_15", side = 3,line = 1, cex = 2)
plot(hc.fit.average.20, xaxt="n")
mtext("hclust_cutree_average_k_20", side = 3,line = 1, cex = 2)
plot(hc.fit.average.25)
mtext("hclust_cutree_average_k_25", side = 3,line = 1, cex = 2)
dev.off()

png("hclust_ward_d2_cutree_k_ds3norm_2_98.png", width = 1500, 
    height = 1200, units = "px")
par(mfrow=c(4,1), mar=c(3,3,1,1), oma=c(3,2,3,1), cex.main=3,
    cex.axis=2)
plot(hc.fit.ward.10, xaxt="n")
mtext("hclust_cutree_ward_k_10", side = 3,line = 1, cex = 2)
plot(hc.fit.ward.15, xaxt="n")
mtext("hclust_cutree_ward_k_15", side = 3,line = 1, cex = 2)
plot(hc.fit.ward.20, xaxt="n")
mtext("hclust_cutree_ward_k_20", side = 3,line = 1, cex = 2)
plot(hc.fit.ward.25)
mtext("hclust_cutree_ward_k_25", side = 3,line = 1, cex = 2)
dev.off()

# To plotting a very long narrow dendrogram change width to 200000
rec.time <- AcousticDS$rec.time
png('hclust_ward_D2_dendrogram_average_ds3.exp1.norm_2_98.png', 
    width = 2000, height = 800, units = "px",
    pointsize = 12) 
par(mar=c(0,0,3,0), oma=c(5,5,5,5))
plot(hc.fit.ward, hang = -0.1,
     main = "Cluster Dendrogram - ward.D2",
     cex.main=5, las=2,
     labels = rec.time)
dev.off()

#cut.heights <- NULL
#for (i in seq(1,70,0.5)) {
#  nam <- paste("cut_height_", i, sep="")
#  (nam <- cutree(hc.fit.ward, h = i))
#  cut.heights <- cbind(cut.heights, unname(nam))
#}
#column.names <- NULL
#for (i in seq(1,70,0.5)) {
#  col.names <- paste("cut_heights_", i, sep = "")
#  column.names <- c(column.names,col.names)
#}

#colnames(cut.heights) <- column.names

#write.csv(cut.heights, file = "hclust_ward_D2_clustering_cut_heights.csv")


# Plotting hclust dendograms
#par(mfrow = c(1,2))
#hc.fit.mcquitty <- hclust(dist(ds3.norm_2_98), "mcquitty")
#png('hclust_dendrogram_mcquitty_ds3norm_2_98.png', 
#    width = 1500, height = 1200, units = "px") 
#plot(hc.fit.mcquitty, hang = -0.1, cex=0.1,
#     main = "Cluster Dendrogram - mcquitty")
#dev.off()

#######################################################
# Method 2b:  Hierarchical - agnes
#######################################################
setwd("C:\\Work\\CSV files\\DataSet_New_New_New\\Hierarchical")

library(cluster)
# WARNING: agnes takes four hours to cluster 17000 objects 
ag.d <- dist(ds3.norm_2_98[1:2880,], method = "euclidean")
ag.fit <- agnes(ag.d, method = "ward")

# Plot dendrogram
par(mfrow=c(2,2), mar=c(2,2,2,2))
png('Agnes_plot_ds3norm_2_98.png', 
    width = 1500, height = 1200, units = "px") 
plot(ag.fit, which.plots=2, cex=0.5, hang=-0.1)
dev.off()

#(cut_height_3 <- cutree(as.hclust(ag.fit), h = 3))
#plot(unname(cut_height_3), main = "Height =3")
(cut_height_4 <- cutree(as.hclust(ag.fit), h = 4))
plot(unname(cut_height_4), main = "Height = 4")
(cut_height_5 <- cutree(as.hclust(ag.fit), h = 5))
plot(unname(cut_height_5), main = "Height = 5")
(cut_height_7 <- cutree(as.hclust(ag.fit), h = 7))
plot(unname(cut_height_7), main = "Height = 7")

# Clustering of full dataset
# WARNING: This takes four hours and is demanding on 
# processing memory 7.8 GB
ag.d <- dist(ds3.norm_2_98, method = "euclidean")
ag.fit <- agnes(ag.d, method = "ward")
cut.heights <- NULL
for (i in seq(0.5,20,0.5)) {
  nam <- paste("cut_height_", i, sep="")
  (nam <- cutree(as.hclust(ag.fit), h = i))
  cut.heights <- cbind(cut.heights, unname(nam))
}
column.names <- NULL
for (i in seq(0.5,20,0.5)) {
  col.names <- paste("cut_heights_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(cut.heights) <- column.names

write.csv(cut.heights, file = "Agnes_clustering_cut_heights.csv")

# An example of cuttreeDynamic method ????
library(dynamicTreeCut)
library(stats)
cut <- NULL
for (i in seq(2,60, 1)) {
  cut2 <- cutreeDynamic(as.hclust(ag.fit), 
                        distM = as.matrix(dist(ds3.norm_2_98)),
                        deepSplit=1, cutHeight = i,
                        minClusterSize = 500)
  cut <- cbind(cut, cut2)  
}

column.names <- NULL
for (i in seq(2,60,1)) {
  col.names <- paste("cut_heights_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(cut) <- column.names

write.table(cut, 
            file="agnes_ds3norm_cut_heights.csv", 
            row.names = F, sep = ",")
plot(cut2)

maxCoreScatter <- 0.64
minGap <- (1 - maxCoreScatter) * 3/4
dynamicCut <- NULL
for (i in seq(100,600,20)) {
  dynCut <- cutreeDynamic(as.hclust(ag.fit), minClusterSize=i, 
                              method="hybrid", 
                              distM=as.matrix(dist(ds3.norm_2_98), 
                              method="euclidean"), 
                              deepSplit=0,
                              maxCoreScatter=maxCoreScatter,
                              minGap=minGap,
                              maxAbsCoreScatter=NULL, 
                              minAbsGap=NULL)
  dynamicCut <- cbind(dynamicCut, dynCut)
}

column.names <- NULL
for (i in seq(100,600,20)) {
  col.names <- paste("cut_heights_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(dynamicCut) <- column.names

write.table(dynamicCut, 
            file="agnes_dynamic_mcs64_deepsplit0_ds3norm2_98_cut_heights.csv", 
            row.names = F, sep = ",")
#..cutHeight not given, setting it to 72.4  ===>  99% of the (truncated) height range in dendro.
#..done.
#######################################################
# Method 3:  Model-based - Mclust
#######################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Mclust")
# WARNING: mclust takes many hours 
library(mclust)
mc.fit <- Mclust(ds3.norm, G=1:50) 

sink('mc_fit_BIC_ds3norm_1_50_.csv')
mc.fit$BIC
sink()

summary(mc.fit) 
sink('mc_fit_summary_ds3norm_1_50_.csv')
summary(mc.fit)
sink()

mclust.list <- unname(mc.fit$classification)
png('Clusterplot_ds3norm_1_50_days.png', 
    width = 3000, height = 800, units = "px") 
plot(mclust.list)
abline(v=c(0,1441, 2879,4319,5757,7197,10077,
           11518,12959,14400,15841,17282),lty=2,col="red",lwd=2)
abline(v=8637, lwd=3, lty=2)
dev.off()

list <- which(AcousticDS$minute.of.day=="0")
lst1 <- NULL
for (i in 1:length(list)) {
 lst <- list[i+1]-1
 lst1 <- c(lst1, lst)
}
lst1
#[1]  1441  2879  4319  5757  7197  8637 10077 11518 12959 14400
#[11] 15841    NA

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
    width = 600, height = 600, units = "px") 
densBackgr <- densityMclust(ds3.norm$BackgroundNoise)
plot(densBackgr, data = ds3.norm$BackgroundNoise, what = "density",
     xlab = "Background Noise")
dev.off()

png('densSNR_plot.png', 
    width = 600, height = 600, units = "px") 
densSNR <- densityMclust(ds3.norm$Snr)
plot(densSNR, data = ds3.norm$Snr, what = "density")
dev.off()

png('densEPS_plot.png', 
    width = 600, height = 600, units = "px") 
densEPS <- densityMclust(ds3.norm$EventsPerSecond)
plot(densEPS, data = ds3.norm$EventsPerSecond, what = "density")
dev.off()

png('densAvSNR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAvSNR <- densityMclust(ds3.norm$AvgSnrOfActiveFrames)
plot(densAvSNR, data = ds3.norm$AvgSnrOfActiveFrames, what = "density")
dev.off()

png('densAccComp_plot.png', 
    width = 600, height = 600, units = "px") 
densAcousticComp <- densityMclust(ds3.norm$AcousticComplexity)
plot(densAcousticComp, data = ds3.norm$AcousticComplexity, what = "density")
dev.off()

png('densEntCOV_plot.png', 
    width = 600, height = 600, units = "px") 
densEntCoV <- densityMclust(ds3.norm$EntropyOfCoVSpectrum)
plot(densEntCoV, data = ds3.norm$EntropyOfCoVSpectrum, what = "density")
dev.off()

png('densLowFrCov_plot.png', 
    width = 600, height = 600, units = "px") 
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
    width = 600, height = 600, units = "px") 
densEntPs <- densityMclust(ds3.norm$EntropyOfPeaksSpectrum)
plot(densEntPs, data = ds3.norm$EntropyOfPeaksSpectrum, what = "density")
dev.off()

#######################################################
# Method 4:  Hybrid method using kmeans followed by hclust
# 
#######################################################
# Cluster using kmeans and 1000, 1500... centers
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hybrid\\")
clusters <- NULL
for (i in seq(1000, 4500, 500)) {
  set.seed(123)
  kmeansObj <- kmeans(ds3.norm_2_98, centers = i, iter.max = 100)
  kmeansCenters <- kmeansObj$centers
  dist.hc <- dist(kmeansCenters)
  #hc.fit <- hclust(dist.hc, "average")
  hybrid.fit.ward <- hclust(dist.hc, "ward.D2")
  plot(hybrid.fit.ward)
  hybrid.clusters <- cutree(hybrid.fit.ward, k=25)
  # generate the test dataset
  hybrid.dataset <- cbind(hybrid.clusters, kmeansCenters)
  hybrid.dataset <- as.data.frame(hybrid.dataset)
  train <- hybrid.dataset
  table(hybrid.dataset$hybrid.clusters)
  test <- ds3.norm_2_98
  # set up classes
  cl <- factor(unname(hybrid.clusters))
  # perform linear discriminant analysis
  library(MASS)
  z <- lda(train[,-1], cl)
  pr <- predict(z, test)
  clusts <- as.integer(pr$class)
  clusters <- cbind(clusters, clusts)
}

# produce 24 hour fingerprints from this clusterlist
column.names <- NULL
for (i in seq(1000, 4500, 500)) {
  col.names <- paste("hybrid_k", i, "k25", sep = "")
  column.names <- c(column.names,col.names)
}
colnames(clusters) <- column.names

write.csv(clusters, file = "C:\\Work\\CSV files\\DataSet_Exp2a\\Hybrid\\hybrid_clust_k25.csv", row.names = F)
##################################################################
# EXPERIMENT 1
# The aim of this experiment is to determine the minimum number of
# clusters in each of the methods that sufficiently characterise 
# the rain event at midday on the 24 June 2015 at site Gympie NP
##################################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp1")
Acoustic.exp1 <- read.csv("DataSet 22_25June2015.csv", header = T)

ds.exp1 <- Acoustic.exp1[,c(3,5,7,10,11,14,16)]
#ds.exp1 <- Acoustic.exp1[,c(2:18)]

#######################################################
# function - normalise
#######################################################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

#######################################################
# Create ds.exp1.norm_2_98 for kmeans, clara, hclust
# a dataset normalised between 2 and 98%
#######################################################
ds.exp1.norm_2_98 <- ds.exp1
for (i in 1:length(ds.exp1)) {
  q1 <- unname(quantile(ds.exp1[,i], probs = 0.02))
  q2 <- unname(quantile(ds.exp1[,i], probs = 0.98))
  ds.exp1.norm_2_98[,i]  <- normalise(ds.exp1.norm_2_98[,i], 
                                  q1, q2)
}
# adjust values greater than 1 or less than 0
for (j in 1:length(ds.exp1)) {
  for (i in 1:length(ds.exp1.norm_2_98[,j])) {
    if (ds.exp1.norm_2_98[i,j] > 1) ds.exp1.norm_2_98[i,j] = 1
  }
  for (i in 1:length(ds.exp1.norm_2_98[,j])) {
    if (ds.exp1.norm_2_98[i,j] < 0) ds.exp1.norm_2_98[i,j] = 0
  }
}


# Create ds.exp1.norm for mclust, apcluster
ds.exp1.norm <- ds.exp1
for (i in 1:length(ds.exp1.norm)) {
  ds.exp1.norm[,i]  <- normalise(ds.exp1.norm[,i], min(ds.exp1[,i]),
                                 max(ds.exp1[,i]))
}

#######################################################
# Generate and save the Correlation Matrix 
#######################################################
#a <- cor(Acoustic.exp1[,2:18][,unlist(lapply(Acoustic.exp1[,2:18], 
#        is.numeric))])
#write.table(a, file = paste("Correlation_matrix_Exp1.csv",sep=""), 
#            col.names = NA, qmethod = "double", sep = ",")
#rm(AcousticDS)

#######################################################
# PRINCIPAL COMPONENT ANALYSIS sd3
#######################################################
#file <- paste("Principal Component Analysis_ds_exp1_norm.png", sep = "")
#png(
#  file,
#  width     = 200,
#  height    = 200,
#  units     = "mm",
#  res       = 1200,
#  pointsize = 4
#)

#par(mar =c(2,2,4,2), cex.axis = 2.5)
#PCAofIndices<- prcomp(ds.exp1.norm)
#biplot(PCAofIndices, col=c("grey80","red"), 
#       cex=c(0.5,1))#, ylim = c(-0.025,0.02), 
##xlim = c(-0.025,0.02))
#abline(h=0,v=0)
#mtext(side = 3, line = 2, 
#      paste("Principal Component Analysis prcomp ds_exp1_norm"), 
#      cex = 2.5)
#rm(PCAofIndices)
#dev.off()

# METHOD 1: PARTITIONING - KMEANS
clusters <- NULL
for (i in 2:150) {
  set.seed(123)
  kmeansObj <- kmeans(ds.exp1.norm_2_98, centers = i, iter.max = 100)
  clus <- unname(kmeansObj$cluster)
  clusters <- cbind(clusters, clus)
}
column.names <- NULL
for (i in 2:150) {
  col.names <- paste("clusters_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(clusters) <- column.names

write.csv(clusters, file = "kmeans_clusters.csv")

kmean_clus <- read.csv("kmeans_clusters.csv", header = T)

file <- paste("kmeans_cutree_2_98.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 400,
  pointsize = 4
)

list <- c(10,13,24,32,38,45)

par(mfrow=c(6,1), mar=c(0,0,0,0), oma=c(5,8,5,5), 
    cex=1.2)

for (i in list) {
  plot(kmean_clus[3562:3750,i],xaxt="n",yaxt="n")
  mtext(side=2, line=2, paste(i), las=2, cex=2.5)
  abline(v=33) # 3595 -  11:52:56 AM
  abline(v=41) # 3603 - 12:02:56 PM
  #abline(v=48) # 3610 - 12:07:56 PM
  #abline(v=53) # 3615 - 12:12:56 PM
  abline(v=130) # 3692 - 1:29:56 PM
  abline(v=145) # 3707 - 1:44:48 PM
}
mtext("kmeans", side = 3, line=1, cex=3, outer = T)
dev.off()
###################################################
# METHOD 2: HIERARCHICAL - HCLUST
##################################################
require(graphics)
dist.hc.exp1 <- dist(ds.exp1.norm_2_98)

par(mfrow = c(1,2))
hc.exp1.fit.ward <- hclust(dist(ds.exp1.norm_2_98), "ward.D")
png('hclust_dendrogram_wardD_ds.exp1.norm_2_98.png', 
    width = 1500, height = 1200, units = "px") 
plot(hc.exp1.fit.ward, hang = -0.1, cex=0.1,
     main = "Cluster Dendrogram - ward.D")
dev.off()

library(dynamicTreeCut)
library(stats)
cut <- cutreeDynamic(hc.exp1.fit.ward, method = "tree",
                     minClusterSize = 102)
plot(cut, ylim=c(0,30))

png('hclust_dendrogram_wardD2_ds.exp1.norm_2_98.png', 
    width = 1500, height = 1200, units = "px") 
hc.exp1.fit.ward.d2 <- hclust(dist(ds.exp1.norm_2_98), "ward.D2")
plot(hc.exp1.fit.ward.d2, hang = -0.1, cex=0.1,
     main = "Cluster Dendrogram - ward.D2")
dev.off()

cut.d2 <- cutreeDynamic(hc.fit.ward.d2, method = "tree",
                        minClusterSize = 87)
plot(cut.d2, ylim=c(0,30))

# With deepSplit - ward method
maxCS <- 0.9  # c(0.95, 0.9, 0.85, 0.75, 0.65)
minG <- (1 - maxCoreScatter) * 0.75
deep <-  3      # c(4,3,2,1,0)
dynamicCut <- NULL
for (i in seq(1,100,2)) {
  dynCut <- cutreeDynamic(hc.exp1.fit.ward, minClusterSize=i, 
                          method="hybrid",
                          distM=as.matrix(dist(ds.exp1.norm_2_98), 
                                          method="euclidean"), 
                          deepSplit=deep,
                          maxCoreScatter=maxCS,
                          minGap=minG,
                          maxAbsCoreScatter=NULL, 
                          minAbsGap=NULL)
  dynamicCut <- cbind(dynamicCut, dynCut)
}

column.names <- NULL
for (i in seq(1,100,2)) {
  col.names <- paste("cut_heights_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(dynamicCut) <- column.names

write.table(dynamicCut, 
            file="hclust_ward_dynamic_mcs90_deepsplit3_ds.exp1.norm_2_98_cut_heights.csv", 
            row.names = F, sep = ",")

# With deepSplit ward.d2
maxCS <- 0.95  # c(0.99, 0.95, 0.9, 0.85, 0.75, 0.65)
minG <- (1 - maxCoreScatter) * 0.75
deep <-  4       # c(4,3,2,1,0)
dynamicCut <- NULL
for (i in seq(1,10,2)) {
  dynCut <- cutreeDynamic(hc.exp1.fit.ward.d2, 
                          minClusterSize=i, 
                          method="hybrid",
                          distM=as.matrix(dist(ds.exp1.norm_2_98), 
                          method="euclidean"), 
                          deepSplit=deep,
                          maxCoreScatter=maxCS,
                          minGap=minG,
                          maxAbsCoreScatter=NULL, 
                          minAbsGap=NULL)
  dynamicCut <- cbind(dynamicCut, dynCut)
}

column.names <- NULL
for (i in seq(1,10,2)) {
  col.names <- paste("cut_heights_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(dynamicCut) <- column.names

write.table(dynamicCut, 
            file="hclust_ward_d2_dynamic_mcs90_deepsplit3_ds.exp1.norm_2_98_cut_heights.csv", 
            row.names = F, sep = ",")

# using usual cutree method
cut.heights <- NULL
for (i in seq(1,100,1)) {
  nam <- paste("cut_height_", i, sep="")
  (nam <- cutree(hc.exp1.fit.ward, h = i))
  cut.heights <- cbind(cut.heights, unname(nam))
}
column.names <- NULL
for (i in seq(1,100,1)) {
  col.names <- paste("cut_heights_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(cut.heights) <- column.names

write.csv(cut.heights, file = "hclust_ward_clustering_cut_heights.csv")

cut.heights <- NULL
for (i in seq(1,20,0.5)) {
  nam <- paste("cut_height_", i, sep="")
  (nam <- cutree(hc.exp1.fit.ward.d2, h = i))
  cut.heights <- cbind(cut.heights, unname(nam))
}
column.names <- NULL
for (i in seq(1,20,0.5)) {
  col.names <- paste("cut_heights_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(cut.heights) <- column.names

write.csv(cut.heights, file = "hclust_ward_d2_clustering_cut_heights.csv")

#########
hclust_clus <- read.csv("hclust_ward_clustering_cut_heights.csv", 
                        header = T)
list2 <- NULL
for (i in 2:length(hclust_clus)) {
  lst <- max(hclust_clus[2:length(hclust_clus$X),i])
  list2 <- c(list2, lst)
}

file <- paste("hclust - ward_cutree_2_98.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 400,
  pointsize = 4
)

list <- c(29,26,19,12,9,4)
#list <- c(2:7)
par(mfrow=c(6,1), mar=c(0,0,0,0), oma=c(5,8,5,5),
    cex=1.2)

for (i in list) {
  plot(hclust_clus[3562:3750,i],xaxt="n",yaxt="n")
  mtext(side=2, line=2, paste(list2[i]), las=2, cex = 2.5)
  abline(v=33) # 3595 -  11:52:56 AM
  abline(v=41) # 3603 - 12:02:56 PM
  #abline(v=48) # 3610 - 12:07:56 PM
  #abline(v=53) # 3615 - 12:12:56 PM
  abline(v=130) # 3692 - 1:29:56 PM
  abline(v=145) # 3707 - 1:44:48 PM
}
mtext("hclust - ward method", side = 3, line=1, cex=3, outer = T)
dev.off()

##
hclust_clus <- read.csv("hclust_ward_d2_clustering_cut_heights.csv", 
                        header = T)
list2 <- NULL
for (i in 1:length(hclust_clus)) {
  lst <- max(hclust_clus[2:length(hclust_clus$X),i])
  list2 <- c(list2, lst)
}

file <- paste("hclust - ward.d2_cutree_2_98.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 400,
  pointsize = 4
)

#list <- c(23:20)
list <- c(20,14,10,6,4,3)
par(mfrow=c(6,1), mar=c(0,0,0,0), oma=c(5,8,5,5),
    cex=1.2)

for (i in list) {
  plot(hclust_clus[3562:3750,i],xaxt="n",yaxt="n")
  mtext(side=2, line=2, paste(list2[i]), las=2, cex=2.5)
  abline(v=33) # 3595 -  11:52:56 AM
  abline(v=41) # 3603 - 12:02:56 PM
  #abline(v=48) # 3610 - 12:07:56 PM
  #abline(v=53) # 3615 - 12:12:56 PM
  abline(v=130) # 3692 - 1:29:56 PM
  abline(v=145) # 3707 - 1:44:48 PM
}
mtext("hclust - ward.d2 method", side = 3, line=1, outer = T,
      cex=3)
dev.off()
############################################################
# METHOD 3 EXPERIMENT 1 - Mclust on four days
############################################################
library(mclust)
mc.fit <- Mclust(ds.exp1.norm, G=1:100, modelNames = c("EVI")) 

sink('mc_fit_BIC_ds_exp1_norm_1_100_EVI.csv')
mc.fit$BIC
sink()

summary(mc.fit) 
sink('mc_fit_summary_ds_exp1_norm_1_100_EVI_.csv')
summary(mc.fit)
sink()

mclust.list <- unname(mc.fit$classification)
png('Clusterplot_ds_exp1_norm_1_100_EVI.png', 
    width = 3000, height = 800, units = "px") 
plot(mclust.list)
abline(v=c(0,1441, 2881, 4321,5762),lty=2,col="red",lwd=2)
abline(v=8637, lwd=3, lty=2)
dev.off()

list <- which(Acoustic.exp1$minute.of.day=="0")
lst1 <- NULL
for (i in 1:length(list)) {
  lst <- list[i+1]-1
  lst1 <- c(lst1, lst)
}
lst1
#[1]  1441  2879  4319  5757  7197  8637 10077 11518 12959 14400
#[11] 15841    NA

write.table(mclust.list, 
            file="mclustlist_ds_exp1_norm_1_100_EVI.csv", 
            row.names = F, sep = ",")

mean <- mc.fit$parameters$mean
write.table(mean, 
            file="mclust_ds_exp1_norm_1_100_EVI_mean.csv", 
            row.names = F, sep = ",")

sink('mclust_variance_ds_exp1_norm_1_100_EVI.csv')
mc.fit$parameters$variance
sink()

sigma <- mc.fit$parameters$variance$sigma
write.table(sigma, 
            file="mclust_sigma_ds_exp1_norm_1_100_EVI.csv", 
            row.names = F, sep = ",")

png('mclust_BIC_plot_ds_exp1_norm_1_100_EVI.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "BIC")
dev.off()

png('mclust_Density_plot_ds_exp1_norm_1_100_EVI.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "density")
dev.off()

png('mclust_Classification_plot_ds_exp1_norm_1_100_EVI.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "classification")
dev.off()

# Density plot # these take a while these give nice density plots of 
# the normalised data

png('densBGR_plot.png', 
    width = 600, height = 600, units = "px") 
densBackgr <- densityMclust(ds.exp1.norm$BackgroundNoise)
plot(densBackgr, data = ds.exp1.norm$BackgroundNoise, what = "density",
     xlab = "Background Noise")
dev.off()

png('densSNR_plot.png', 
    width = 600, height = 600, units = "px") 
densSNR <- densityMclust(ds.exp1.norm$Snr)
plot(densSNR, data = ds.exp1.norm$Snr, what = "density")
dev.off()

png('densEPS_plot.png', 
    width = 600, height = 600, units = "px") 
densEPS <- densityMclust(ds.exp1.norm$EventsPerSecond)
plot(densEPS, data = ds.exp1.norm$EventsPerSecond, what = "density")
dev.off()

png('densAvSNR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAvSNR <- densityMclust(ds.exp1.norm$AvgSnrOfActiveFrames)
plot(densAvSNR, data = ds.exp1.norm$AvgSnrOfActiveFrames, what = "density")
dev.off()

png('densAccComp_plot.png', 
    width = 600, height = 600, units = "px") 
densAcousticComp <- densityMclust(ds.exp1.norm$AcousticComplexity)
plot(densAcousticComp, data = ds.exp1.norm$AcousticComplexity, what = "density")
dev.off()

png('densEntCOV_plot.png', 
    width = 600, height = 600, units = "px") 
densEntCoV <- densityMclust(ds.exp1.norm$EntropyOfCoVSpectrum)
plot(densEntCoV, data = ds.exp1.norm$EntropyOfCoVSpectrum, what = "density")
dev.off()

png('densLowFrCov_plot.png', 
    width = 600, height = 600, units = "px") 
densLowFrCov <- densityMclust(ds.exp1.norm$LowFreqCover)
plot(densLowFrCov, data = ds.exp1.norm$LowFreqCover, what = "density")
dev.off()

png('densMidFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densMidFrCov <- densityMclust(ds.exp1.norm$MidFreqCover)
plot(densMidFrCov, data = ds.exp1.norm$MidFreqCover, what = "density")
dev.off()

png('densHighFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densHighFrCov <- densityMclust(ds.exp1.norm$HighFreqCover)
plot(densHighFrCov, data = ds.exp1.norm$HighFreqCover, what = "density")
dev.off()

png('densEntPS_plot.png', 
    width = 600, height = 600, units = "px") 
densEntPs <- densityMclust(ds.exp1.norm$EntropyOfPeaksSpectrum)
plot(densEntPs, data = ds.exp1.norm$EntropyOfPeaksSpectrum, what = "density")
dev.off()

# Extracting the rain section from each of the BIC files
vvv15 <- read.csv("mclustlist_ds_exp1_norm_1_30.csv", header=T)
vev23 <- read.csv("mclustlist_ds_exp1_norm_1_55_VEV.csv", header=T)
evv23 <- read.csv("mclustlist_ds_exp1_norm_1_50_EVV.csv", header=T)
eev29 <- read.csv("mclustlist_ds_exp1_norm_1_80_EEV.csv", header=T)
vve34 <- read.csv("mclustlist_ds_exp1_norm_1_50.csv", header=T)
eve37 <- read.csv("mclustlist_ds_exp1_norm_1_120_EVE.csv", header=T)
vvi42 <- read.csv("mclustlist_ds_exp1_norm_1_70_VVI.csv", header=T)
evi52 <- read.csv("mclustlist_ds_exp1_norm_1_100_EVI.csv", header = T)
vee64 <- read.csv("mclustlist_ds_exp1_norm_1_120_VEE.csv", header = T)
eii74 <- read.csv("mclustlist_ds_exp1_norm_1_160_EII.csv", header = T)
vii75 <- read.csv("mclustlist_ds_exp1_norm_1_160_VII.csv", header = T)
eee76 <- read.csv("mclustlist_ds_exp1_norm_1_160_EEE.csv", header = T) # EEE model takes much longer than other models
vei91 <- read.csv("mclustlist_ds_exp1_norm_1_200_VEI.csv", header = T)
eei92 <- read.csv("mclustlist_ds_exp1_norm_1_160_EEI.csv", header = T)

# Create a database of cluster lists
mclust_clus <- cbind(vvv15,vev23,evv23,eev29,vve34,eve37,
                     vvi42,evi52,vee64,eii74,vii92,eee76,
                     vei91,eei92)
list2 <- c("VVV_15", "VEV_23", "EVV_23", "EEV_29", "VVE_34",
           "EVE_37", "VVI_42", "EVI_52", "VEE_64", "EII_74",
           "VII_75", "VII_76", "VEI_91", "EEI_92")

colnames(mclust_clus) <- list2

# Print plots in pairs
list <- c(17:18)
file <- paste("mclust ds_exp1_norm_",list2[list[1]],"_",list2[list[2]],".png", sep = "")
png(
  file,
  width     = 200,
  height    = 300,
  units     = "mm",
  res       = 400,
  pointsize = 4
)

par(mfrow=c(2,1), mar=c(5,0,0,0), oma=c(5,10,5,5),
    cex=1.2)
timePos <- c(3562-3561, 3592-3561, 3605-3561, 
             3692-3561, 3707-3561, 3750-3561)
timeLabel <- c("11:20am", "11:53", "12:01",
               "13:30","13:45","14:28")
for (i in list) {
  plot(mclust_clus[3562:3750,i],xaxt="n", las=2,
       xlab = "", cex.axis=2)
  mtext(side=2, line=3, paste(list2[i]), cex=2.4)
  abline(v=33) # 3595 -  11:52:56 AM
  abline(v=41) # 3603 - 12:02:56 PM
  #abline(v=48) # 3610 - 12:07:56 PM
  #abline(v=53) # 3615 - 12:12:56 PM
  abline(v=130) # 3692 - 1:29:56 PM
  abline(v=145) # 3707 - 1:44:48 PM
  axis(side = 1, at = timePos, labels = timeLabel, 
       mgp = c(1.8, 0.5, 0), cex.axis = 1.8, line = 1.1,
       lty=0)
mtext("mclust models", side = 3, line=1, outer = T,
      cex=2.4)
}
dev.off()

#[1] "VVV_15" "VEV_23" "EVV_23" "EEV_29" "VVE_34" "EVE_37" "VVI_42" "EVI_52"
#[9] "VEE_64" "EII_74" "VII_75" "VII_76" "VEI_91" "EEI_92"

# Plottting a clusterplot of each of the models
for (i in 1:length(list2)) {
  mclust.list <- mclust_clus[i]
  png(paste("Clusterplot_ds_exp1_norm_", list2[i], ".png", sep=""), 
      width = 3000, height = 1200, units = "px")
  par(oma=c(2,2,2,1.5), mar=c(4,5,2,4))
  plot(as.matrix(mclust.list), ylab="Number of clusters",
       cex.axis=2, cex.lab=2.2, xlab = "Time (minutes)")
  mtext(paste("Mclust - Exp1 ", list2[i], sep = ""), side = 4, cex=2, 
        line = 2, cex.axis=2)
  abline(v=c(0,1441, 2881, 4321,5762),lty=2,col="red",lwd=2)
  abline(v=8637, lwd=3, lty=2)
  dev.off()
}
