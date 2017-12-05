#rm(list = ls())
library(fpc)
# The Silhouette Width can be calculated in the following ways:
# 1. distcritmulti function in fpc package
# 2. cluster.stats function in fpc package - this one crashes
# 3. silhouette function in cluster package
# 4. intCriteria function in clusterCrit package

# The Dunn Index can be calculated in the following ways:
# 1. dunn function in clValid package
# 2. clv.Dunn function in clv package

#library(clValid)
library(fpc) # needed for distcritmulti function
silhouette <- NULL
  
list <- c("12500", "15000", "17500", "20000", "22500", "25000", "27500","30000")
list1 <- c("5", "10", "15", "20", "25", "30", "35", "40", "45", "50",
           "55","60", "65", "70", "75", "80", "85", "90", "95", "100")

overall_sil_width <- data.frame(desc = NA)

for(i in 1:length(list)) {
  print(paste("Starting", list[i]))
  file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
  load(file)
  for(j in 1:length(list1)) {
    file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\hybrid_dataset_centers_",list[i],"_", list1[j], ".csv", sep="")
    data <- read.csv(file, header = T)
    #load(file = "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\hclust_results\\hclust_clusters25000.Rdata")
    centers <- data.frame(get(paste("centers",list[i],sep="")))
    cluster <- data[,1]
    rm(data)
    a <- distcritmulti(centers, cluster, criterion = "asw")
    sil_width <- a$crit.overall
    overall_sil_width[j,(i+1)] <- a$crit.overall
    print(a$crit.overall)
  }
}
overall_sil_width$desc <- list1
colnames(overall_sil_width) <- c("k1",paste("k2_",list,sep=""))
View(overall_sil_width)
write.csv(overall_sil_width, "silhouette_width_from_distcritmulti.csv", row.names = F)

tiff("silhouette_distcritmulti.tiff", width=1125, height=1012, res = 300)
# plot silhouette width
par(mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
silhouette <- read.csv("silhouette_width_from_distcritmulti.csv", header = T)
legend <- c("k1=12500", "k1=15000","k1=17500", "k1=20000", "k1=22500", "k1=25000", "k1=27500")
ylim <- c(0.04, 0.145)
#plot(silhouette$k2_12500[1:20], type="l", col="orange", ylim=ylim, 
#     xaxt='n', xlab="", ylab="", lty=12, lwd=1.5)
#abline(v=c(4,8,12,16,20), lty=2, lwd=0.2)
#par(new=T)
plot(silhouette$k2_15000[1:20], type="l",  ylim=ylim, #col="maroon",
     xaxt='n', xlab="", ylab="", lty=13, lwd=1.5)
abline(v=c(4,8,12,16,20), lty=2, lwd=0.2)
#par(new=T)
#plot(silhouette$k2_17500[1:20], type="l", col="green", ylim=ylim, 
#     xaxt='n', xlab="", ylab="", lty=14, lwd=1.5)
par(new=T)
plot(silhouette$k2_20000[1:20], type="l", #col="hotpink", 
     ylim=ylim, xaxt='n', xlab="", ylab="", lty=15, 
     lwd=1.5, main="Silhouette index")
mtext(side=2, line=1.1, "Silhouette width")
mtext(side=1, line=1.1, "k2")
#par(new=T)
#plot(silhouette$k2_22500[1:20], type="l", col="red", 
#     ylim=ylim, xaxt='n', xlab="", 
#     ylab="Silhouette width", lty=16, lwd=1.5)
par(new=T)
plot(silhouette$k2_25000[1:20], type="l", ylim=ylim, #col="blue", 
     xaxt='n', xlab="", ylab="", lty=17, lwd=1.5)
#par(new=T)
#plot(silhouette$k2_27500[1:20], type="l", col="darkgreen", ylim=ylim, 
#     xaxt='n', xlab="", ylab="", lty=18, lwd=1.5)
list1 <- c("5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100")
axis(side=1, at=1:20, label=list1)
#legend(x = 8, y = 0.98*ylim[2], col = c("orange", "maroon", "green","hotpink","red","blue","darkgreen"), 
#       legend = c(legend[1], legend[2], legend[3],legend[4], legend[5], legend[6], legend[7]), 
#       lwd = 1.5, cex = 1.1, bty = "n", lty=c(12,13,14,15,16,17,18))
legend(x = 9, y = 1.05*ylim[2], #col = c("maroon", "hotpink","blue"), 
       legend = c(legend[2],legend[4], legend[6]), 
       lwd = 1.5, cex = 1.1, bty = "n", lty=c(13,15,17))
dev.off()
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
rm(list = ls())
an <- as.numeric(list[i])
n <- as.numeric(list1[j]) # number of clusters
i <- 6
j <- 12
list <- c("12500", "15000", "17500", "20000", "22500", "25000", "27500","30000")
list1 <- c("5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100")
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
load(file)
centers <- data.frame(get(paste("centers", list[i],sep="")))
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\hybrid_dataset_centers_",list[i],"_", list1[j], ".csv", sep="")
data <- read.csv(file, header = T)
cluster <- data[,1]
rm(data)

dunn_inx <- NULL
stats <- NULL
print("starting Dist matrix")
Dist <- dist(centers, method = "euclidean")
print("finished Dist matrix")
#print("starting hclust")
#clusterObj <- hclust(Dist, method="ward.D2")
#print("finished hclust")
#cluster <- cutree(clusterObj, nc)
#rm(clusterObj)
#print("finished cutree")
#print("starting dunn index, this takes a while ....")
dunn_inx <<- dunn(Dist, cluster[1:n])

rm(list = ls())
library(clValid)
i <- 6
j <- 12
list <- c("12500", "15000", "17500", "20000", "22500", "25000", "27500","30000")
list1 <- c("5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100")
an <- as.numeric(list[i])
n <- as.numeric(list1[j]) # number of clusters
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
load(file)
centers <- data.frame(get(paste("centers", list[i],sep="")))
paste(Sys.time(), "starting Dist matrix")
Dist <- dist(centers, method = "euclidean")
paste(Sys.time(), "finished Dist matrix")
paste(Sys.time(), " Starting cluster statistics, this will take a while", sep = " ")
stats <<- cluster.stats(Dist, cluster, alt.clustering = NULL,
                        noisecluster=FALSE,
                        silhouette = TRUE, G2 = FALSE, G3 = FALSE,
                        wgap=FALSE, sepindex=FALSE, sepprob=0.1,
                        sepwithnoise=FALSE,
                        compareonly = FALSE,
                        aggregateonly = FALSE)
paste(Sys.time(), " Finished cluster statistics, this will take a while", sep = " ")
write.csv("")
stats$avg.silwidth
#dunn_inx
(stats)
#17500 
# stats
plot(stats$clus.avg.silwidths, type="line")
abline(h=c(0,0.25))

tiff("Silhouette.tiff", width=1600, height=900, res = 300)
legend <- c("k1=20000", "k1=22500", "k1=25000")
silhouette <- read.csv("C:\\Work2\\Silhouette_width.csv", header = T)
plot(silhouette$X20000[1:19], type="l", col="hotpink", 
     ylim=c(0.04,0.14), xaxt='n', xlab="", ylab="", lty=15, 
     lwd=1.5, main="Silhouette index")
par(new=T)

plot(silhouette$X20000[1:19], type="l", col="hotpink", 
     ylim=c(0.04,0.14), xaxt='n', xlab="", ylab="", lty=15, 
     lwd=1.5, main="Silhouette index")
par(new=T)
plot(silhouette$X22500[1:19], type="l", col="red", 
     ylim=c(0.04,0.14), xaxt='n', xlab="", ylab="",
     lty=16, lwd=1.5)
par(new=T)
plot(silhouette$X25000[1:19], type="l", col="blue", ylim=c(0.04,0.14), 
     xaxt='n', xlab="", ylab="", lty=17, lwd=1.5)
list1 <- c("5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100")
axis(side=1, at=1:20, label=list1)
ylim <- 0.14
legend(x = 12, y = 1.05*ylim, col = c("hotpink","red","blue"), 
       legend = c(legend[1], legend[2], legend[3]), 
       lwd = 1.5, cex = 1.4, bty = "n", lty=c(15,16,17))
dev.off()

library(cluster)
library(clValid)
library(clv)
library(clusterCrit)
package <- "clusterCrit"
#package <- "cluster"
dunn <- data.frame(desc = NA)
list <- c("12500", "15000", "17500", "20000", "22500", "25000", "27500","30000")
#list <- c("25000")
s <- NULL
for(i in 1:length(list)) {
  print(paste("Starting", list[i]))
  file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
  load(file)
  list1 <- c("5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100")
  #list1 <- c("60")
    for(j in 1:length(list1)) {
    file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\hybrid_dataset_centers_",list[i],"_", list1[j], ".csv", sep="")
    data <- read.csv(file, header = T)
    centers <- data.frame(get(paste("centers",list[i],sep="")))
    cluster <- data[,1]
    rm(data)
    if(package=="cluster") {
      print("starting Dist matrix")
      Dist <- dist(centers, method = "euclidean")
      s <- silhouette(x=cluster, dist=Dist) # from the cluster package
      m <- mean(s[,3])
      print(m)
    }
    if(package=="clv") {
      ## using clv package
      intraclust <- c("complete") # within cluster
      interclust <- c("single") # between clusters
      cls.scatt <- cls.scatt.data(centers, cluster, dist="euclidean")
      dunn1 <- clv.Dunn(cls.scatt, intraclust, interclust)
      dunn[j,(i+1)] <- dunn1
      print(dunn1)  
    }
    if(package=="clusterCrit") {
      ## using clusterCrit package
      centers <- as.matrix(centers)
      dunn1 <- intCriteria(centers, cluster, "Gamma")
      dunn[j,(i+1)] <- dunn1
      print(dunn1)  
    }
  }
}
dunn$desc <- list1
colnames(dunn) <- c("k1",paste("k2_",list,sep=""))
View(dunn)
dunn <- dunn[,1:6]
#write.csv(dunn, paste("dunn_index_using_clv_Dunn_",interclust, "_",
#                      intraclust, ".csv",sep=""), row.names = F)
#write.csv(dunn, paste("dunn_index_using_clusterCrit.csv",sep=""), row.names = F)
#write.csv(dunn, paste("silhouette_width_using_",package,".csv",sep=""), row.names = F)
write.csv(dunn, paste("Gamma1_",package,".csv",sep=""), row.names = F)
####################################################
package <- "clusterCrit"
package <- "clv"
# Plot the dunn index 
tiff(paste("dunn", package, ".tiff",sep=""), width=1125, height=1012, res = 300)
par(mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
if(package=="clusterCrit") {
  dunn <- read.csv(paste("dunn_index_using_",package,".csv",sep=""), header=T)
  ylim <- c(0.02, 0.042)
}
if(package=="clv") {
  dunn <- read.csv(paste("dunn_index_using_clv_Dunn_single_complete.csv",sep=""), header=T)
  ylim <- c(0.02, 0.042)
}
legend <- c("k1=12500", "k1=15000","k1=17500", "k1=20000", "k1=22500", "k1=25000", "k1=27500")
#plot(dunn$k2_12500[1:20], type="l", col="orange", ylim=ylim, 
#     xaxt='n', xlab="", ylab="", lty=12, lwd=1.5)
#abline(v=c(4,8,12,16,20), lty=2, lwd=0.2)
#par(new=T)
plot(dunn$k2_15000[1:20], type="l", ylim=ylim, #col="maroon", 
     xaxt='n', xlab="", ylab="", lty=13, lwd=1.5)
abline(v=c(4,8,12,16,20), lty=2, lwd=0.2)
#par(new=T)
#plot(dunn$k2_17500[1:20], type="l", col="green", ylim=ylim, 
#     xaxt='n', xlab="", ylab="", lty=14, lwd=1.5)
par(new=T)
plot(dunn$k2_20000[1:20], type="l", #col="hotpink", 
     ylim=ylim, xaxt='n', xlab="", ylab="", lty=15, 
     lwd=1.5, main="Dunn index")
mtext(side=2, line=1.1, "Dunn index")
mtext(side=1, line=1.1, "k2")
#par(new=T)
#plot(dunn$k2_22500[1:20], type="l", col="red", 
#     ylim=ylim, xaxt='n', xlab="", 
#     ylab="Silhouette width", lty=16, lwd=1.5)
par(new=T)
plot(dunn$k2_25000[1:20], type="l", ylim=ylim, #col="blue",  
     xaxt='n', yaxt='n', xlab="", ylab="", lty=17, lwd=1.5)
#par(new=T)
#plot(dunn$k2_27500[1:20], type="l", col="darkgreen", ylim=ylim, 
#     xaxt='n', xlab="", ylab="", lty=18, lwd=1.5)
list1 <- c("5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100")
axis(side=1, at=1:20, label=list1)
axis(side=2)
#legend(x = 8, y = 0.98*ylim[2], col = c("orange", "maroon", "green","hotpink","red","blue","darkgreen"), 
#       legend = c(legend[1], legend[2], legend[3],legend[4], legend[5], legend[6], legend[7]), 
#       lwd = 1.5, cex = 1.1, bty = "n", lty=c(12,13,14,15,16,17,18))
legend(x = 9, y = 1.05*ylim[2], #col = c("maroon", "hotpink","blue"), 
       legend = c(legend[2],legend[4], legend[6]), 
       lwd = 1.5, cex = 1.1, bty = "n", lty=c(13,15,17))
dev.off()

# Dunn and Silhouette width combined
package <- "clv"
tiff(paste("dunn_and_silhouette_clv_distcritmuli.tiff",sep=""), 
     width=2025, height=910, res = 300)
par(mfrow = c(1,2), mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
# Dunn Index plot
if(package=="clusterCrit") {
  dunn <- read.csv(paste("dunn_index_using_",package,".csv",sep=""), header=T)
  ylim <- c(0.02, 0.042)
}
if(package=="clv") {
  dunn <- read.csv(paste("dunn_index_using_clv_Dunn_single_complete.csv",sep=""), header=T)
  ylim <- c(0.02, 0.042)
}
legend <- c("k1=12500", "k1=15000","k1=17500", "k1=20000", "k1=22500", "k1=25000", "k1=27500")
plot(dunn$k2_15000[1:20], type="l", ylim=ylim, #col="maroon", 
     xaxt='n', xlab="", ylab="", lty=13, lwd=1.5)
abline(v=c(4,8,12,16,20), lty=2, lwd=0.2)
#par(new=T)
#plot(dunn$k2_17500[1:20], type="l", col="green", ylim=ylim, 
#     xaxt='n', xlab="", ylab="", lty=14, lwd=1.5)
par(new=T)
plot(dunn$k2_20000[1:20], type="l", #col="hotpink", 
     ylim=ylim, xaxt='n', xlab="", ylab="", lty=15, 
     lwd=1.5, main="Dunn index")
mtext(side=2, line=1.1, "Dunn index")
mtext(side=1, line=1.1, "k2")
#par(new=T)
#plot(dunn$k2_22500[1:20], type="l", col="red", 
#     ylim=ylim, xaxt='n', xlab="", 
#     ylab="Silhouette width", lty=16, lwd=1.5)
par(new=T)
plot(dunn$k2_25000[1:20], type="l", ylim=ylim, #col="blue",  
     xaxt='n', yaxt='n', xlab="", ylab="", lty=17, lwd=1.5)
#par(new=T)
#plot(dunn$k2_27500[1:20], type="l", col="darkgreen", ylim=ylim, 
#     xaxt='n', xlab="", ylab="", lty=18, lwd=1.5)
list1 <- c("5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100")
axis(side=1, at=1:20, label=list1)
axis(side=2)
#legend(x = 8, y = 0.98*ylim[2], col = c("orange", "maroon", "green","hotpink","red","blue","darkgreen"), 
#       legend = c(legend[1], legend[2], legend[3],legend[4], legend[5], legend[6], legend[7]), 
#       lwd = 1.5, cex = 1.1, bty = "n", lty=c(12,13,14,15,16,17,18))
legend(x = 11, y = 1.035*ylim[2], #col = c("maroon", "hotpink","blue"), 
       legend = c(legend[2],legend[4], legend[6]), 
       lwd = 1.5, cex = 0.9, bty = "n", lty=c(13,15,17))
mtext(at = 0.0436, line = 1.3, side = 2, "a.", cex=1.2, las=1)
# Silhouette Index
silhouette <- read.csv("silhouette_width_from_distcritmulti.csv", header = T)
legend <- c("k1=12500", "k1=15000","k1=17500", "k1=20000", "k1=22500", "k1=25000", "k1=27500")
ylim <- c(0.04, 0.145)
plot(silhouette$k2_15000[1:20], type="l",  ylim=ylim, #col="maroon",
     xaxt='n', xlab="", ylab="", lty=13, lwd=1.5)
abline(v=c(4,8,12,16,20), lty=2, lwd=0.2)
par(new=T)
plot(silhouette$k2_20000[1:20], type="l", #col="hotpink", 
     ylim=ylim, xaxt='n', xlab="", ylab="", lty=15, 
     lwd=1.5, main="Silhouette index")
mtext(side=2, line=1.1, "Silhouette width")
mtext(side=1, line=1.1, "k2")
par(new=T)
plot(silhouette$k2_25000[1:20], type="l", ylim=ylim, #col="blue", 
     xaxt='n', xlab="", ylab="", lty=17, lwd=1.5)
list1 <- c("5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100")
axis(side=1, at=1:20, label=list1)
legend(x = 11, y = 1.05*ylim[2], #col = c("maroon", "hotpink","blue"), 
       legend = c(legend[2],legend[4], legend[6]), 
       lwd = 1.5, cex = 0.9, bty = "n", lty=c(13,15,17))
mtext(at = 0.153, line =  1.3, side = 2, "b.", cex=1.2, las=1)
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

legend <- c("k1=12500", "k1=15000","k1=17500", "k1=20000", "k1=22500", "k1=25000", "k1=27500")
plot(dunn$k2_12500[1:20], type="l", col="orange", ylim=ylim, 
     xaxt='n', xlab="", ylab="", lty=14, lwd=1.5,
     main="Dunn index")
list1 <- c("5","10","15","20","25","30","35","40","45","50",
           "55","60","65","70","75","80","85","90","95","100")
abline(v=c(4,8,12,16,20), lty=2, lwd=0.2)
axis(side=1, at=1:20, label=list1)
mtext(side=3,line=0.4, paste("R package ",package, sep=""))
par(new=T)
plot(dunn$k2_15000[1:20], type="l", col="maroon", 
     ylim=ylim, xaxt='n', xlab="k2", ylab="", lty=15, 
     lwd=1.5, main="Dunn index")
par(new=T)
plot(dunn$k2_17500[1:20], type="l", col="green", 
     ylim=ylim, xaxt='n', xlab="k2", ylab="", lty=15, 
     lwd=1.5, main="Dunn index")
par(new=T)
plot(dunn$k2_20000[1:20], type="l", col="hotpink", 
     ylim=ylim, xaxt='n', xlab="k2", ylab="", lty=15, 
     lwd=1.5, main="Dunn index")
par(new=T)
plot(dunn$k2_22500[1:20], type="l", col="red", 
     ylim=ylim, xaxt='n', xlab="", 
     ylab="Dunn index", lty=16, lwd=1.5)
par(new=T)
plot(dunn$k2_25000[1:20], type="l", col="blue", ylim=ylim, 
     xaxt='n', xlab="", ylab="", lty=17, lwd=1.5)
par(new=T)
plot(dunn$k2_27500[1:20], type="l", col="darkgreen", ylim=ylim,
     xaxt='n', xlab="", ylab="", lty=18, lwd=1.5)
legend(x = 8, y = 1.05*ylim[2], col = c("orange", "maroon", "green","hotpink","red","blue","darkgreen"), 
       legend = c(legend[1], legend[2], legend[3],legend[4], legend[5],legend[6], legend[7]), 
       lwd = 1.5, cex = 1.1, bty = "n", lty=c(12,13,14,15,16,17,18))
dev.off()

an <- as.numeric(list[i])
n <- as.numeric(list1[j]) # number of clusters
dunn_inx <- NULL
stats <- NULL
Dist <- dist(centers, method = "euclidean")
s <- silhouette(x=cluster, dist=Dist)
summary(s)
mean(s[,3])
a <- sortSilhouette(s)
a <- data.frame(a)
write.csv(a,"Silhouette_25000_60a.csv", row.names = F)
silh <- read.csv("Silhouette_25000_60.csv",header=T)

silhouette <- read.csv("C:\\Work2\\Silhouette_width.csv", header = T)
plot(silhouette$X25000a[1:19], type="l", col="blue", ylim=c(-0.1,0.3), xaxt='n', xlab="", ylab="")
axis(side=1, at=1:20, label=list1)
par(new=T)
plot(silhouette$X22500a[1:19], type="l", col="darkgreen", ylim=c(-0.1,0.3), xaxt='n', xlab="", ylab="")
par(new=T)
plot(silhouette$X20000a[1:19], type="l", col="red", ylim=c(-0.1,0.3), xaxt='n', xlab="", ylab="")

library(NbClust)
res<-NbClust(centers, distance = "euclidean", min.nc=2, max.nc=100, 
             method = "ward.D", index = "all")
res<-NbClust(diss=Dist, distance = NULL, min.nc=2, max.nc=6, 
             method = "ward.D2", index = "silhouette")

if power =< 10 (=<0.5) leave the green at 0
HSV2RGB(c(0.5,0,0.5))
R     G     B 
127.5   0 127.5

if power => 10 (=>0.5) change the green to 128
HSV2RGB(c(0.75,0,0.25))
R     G     B 
191.25   128  63.75 