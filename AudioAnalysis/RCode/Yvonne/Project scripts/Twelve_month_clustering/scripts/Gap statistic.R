# Gap statistic
library(cluster) # required for clusGap function
library(stats) # required for cor and hclust functions
library(parallel)
nCores <- detectCores()
#setwd("D:\\Yvonne\\R_data")
source("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\scripts\\Gap_stat_func.R")
list <- c("12500", "15000", "17500", "20000", "22500", 
          "25000", "27500","30000")

# 25000 centers -------------------------------------------
i=6
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
#file <- paste("kmeanscenters", list[i],".RData", sep="")
load(file)
centers25000 <- as.data.frame(centers25000)
#mydist <- function(x) {
  # transform (t) is required to find the correlation
  # between the centers and not the features
  #as.dist((1-cor(t(x)))/2)
  #dist(x)
#}
mycluster <- function(x, k) {
  list(cluster=cutree(hclust(dist(x), 
                             method = "ward.D2"), k=k))
} 
#plot(gskmn_25000_k80_B500_hclust, main="Gap Statistic 25000")
paste(Sys.time(), " Starting clusGap 25000", sep = " ")
gskmn_25000_k80_B500_hclust <- clusGap_par(centers25000, 
                                      FUNcluster = mycluster,
                                      K.max = 80, B = 500,
                                      do_parallel = TRUE,
                                      verbose=TRUE)
paste(Sys.time(), " Finishing clusGap 25000", sep = " ")

gap_Stat1 <- gskmn_25000_k80_B500_hclust
write.csv(gap_Stat1$Tab, "Gap Statistic_25000_hclust_K80_B500.csv", row.names=F)

sink('gskmn_25000_k80_B500_hclust.txt')
gskmn_25000_k80_B500_hclust
sink()

gap_string <- readChar("gskmn_25000_k80_B500_hclust.txt", nchars = 170)
a <- sub(".*: ", "", gap_string)
b <- substr(a, 1,2)
gap <- as.numeric(b)

tiff("gap_statistics_hclust_25000_2_k80_B500.tiff", 
     height=800, width=1200, res=300)
par(mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
plot(gskmn_25000_k80_B500_hclust, main="Gap Statistic 25000",
     xlim=c(0,50), xlab="", ylab="")
abline(v=gap, lty=2)
mtext(side=1, line=1.1, "k2")
mtext(side=2, line=1.15, "gap statistic")
dev.off()

# 15000 centers -------------------------------------------
i=2
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
#file <- paste("kmeanscenters", list[i],".RData", sep="")
load(file)
centers15000 <- as.data.frame(centers15000)

#mydist <- function(x) {
  # transform (t) is required to find the correlation
  # between the centers and not the features
  #as.dist((1-cor(t(x)))/2)
  #dist(x)
#}
mycluster <- function(x, k) {
  list(cluster=cutree(hclust(dist(x), 
                             method = "ward.D2"), k=k))
} 
#plot(gskmn_15000_k80_B500_hclust, main="Gap Statistic 15000")
paste(Sys.time(), " Starting clusGap 15000", sep = " ")
gskmn_15000_k80_B500_hclust <- clusGap(centers15000, 
                                      FUNcluster = mycluster,
                                      K.max = 80, B = 500,
                                      verbose = TRUE)
paste(Sys.time(), " Finishing clusGap 15000", sep = " ")

gap_Stat1 <- gskmn_15000_k80_B500_hclust
write.csv(gap_Stat1$Tab, "Gap Statistic_15000_hclust_K80 B500.csv", row.names=F)

sink('gskmn_15000_k80_B500_hclust.txt')
gskmn_15000_k80_B500_hclust
sink()

gap_string <- readChar("gskmn_15000_k80_B500_hclust.txt", nchars = 170)
a <- sub(".*: ", "", gap_string)
b <- substr(a, 1,2)
gap <- as.numeric(b)

tiff("gap_statistics_hclust_15000_2_k80_B500.tiff", 
     height=800, width=1200, res=300)
par(mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
plot(gskmn_15000_k80_B500_hclust, main="Gap Statistic 15000",
     xlim=c(0,50), xlab="", ylab="")
abline(v=gap, lty=2)
mtext(side=1, line=1.1, "k2")
mtext(side=2, line=1.15, "gap statistic")
dev.off()

sink('gskmn_15000_k80_B500_hclust.txt')
gskmn_15000_k80_B500_hclust
sink()

# 20000 centers -------------------------------------------
i=4
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
#file <- paste("kmeanscenters", list[i],".RData", sep="")
load(file)
centers20000 <- as.data.frame(centers20000)
#View(data)
rm(data)
#mydist <- function(x) {
  # transform (t) is required to find the correlation
  # between the centers and not the features
  #as.dist((1-cor(t(x)))/2)
  #dist(x)
#}
mycluster <- function(x, k) {
  list(cluster=cutree(hclust(dist(x), 
                             method = "ward.D2"), k=k))
} 
# hclust
#plot(gskmn_20000_k80_B500_hclust, main="Gap Statistic 20000")
paste(Sys.time(), " Starting clusGap 20000", sep = " ")
gskmn_20000_k80_B500_hclust <- clusGap(centers20000, 
                                      FUNcluster = mycluster,
                                      K.max = 80, B = 500,
                                      verbose = TRUE)
paste(Sys.time(), " Finishing clusGap 20000", sep = " ")

gap_Stat1 <- gskmn_20000_k80_B500_hclust
write.csv(gap_Stat1$Tab, "Gap Statistic_20000_hclust_K80 B500.csv", row.names=F)

sink('gskmn_20000_k80_B500_hclust.txt')
gskmn_20000_k80_B500_hclust
sink()

gap_string <- readChar("gskmn_20000_k80_B500_hclust.txt", nchars = 170)
a <- sub(".*: ", "", gap_string)
b <- substr(a, 1,2)
gap <- as.numeric(b)

tiff("gap_statistics_hclust_20000_2_k80_B500.tiff", 
     height=800, width=1200, res=300)
par(mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
plot(gskmn_20000_k80_B500_hclust, main="Gap Statistic 20000",
     xlim=c(0,50), xlab="", ylab="")
abline(v=gap, lty=2)
mtext(side=1, line=1.1, "k2")
mtext(side=2, line=1.15, "gap statistic")
dev.off()

# 12500 centers -------------------------------------------
i=1
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
#file <- paste("kmeanscenters", list[i],".RData", sep="")
load(file)
centers12500 <- as.data.frame(centers12500)

mycluster <- function(x, k) {
  list(cluster=cutree(hclust(dist(x), 
                             method = "ward.D2"), k=k))
} 
#plot(gskmn_12500_k80_B500_hclust, main="Gap Statistic 12500")
paste(Sys.time(), " Starting clusGap 12500", sep = " ")
gskmn_12500_k80_B500_hclust <- clusGap(centers12500, 
                                      FUNcluster = mycluster,
                                      K.max = 80, B = 500,
                                      verbose = TRUE)
paste(Sys.time(), " Finishing clusGap 12500", sep = " ")

gap_Stat1 <- gskmn_12500_k80_B500_hclust
write.csv(gap_Stat1$Tab, "Gap Statistic_12500_hclust_K80 B500.csv", row.names=F)

sink('gskmn_12500_k80_B500_hclust.txt')
gskmn_12500_k80_B500_hclust
sink()

gap_string <- readChar("gskmn_12500_k80_B500_hclust.txt", nchars = 170)
a <- sub(".*: ", "", gap_string)
b <- substr(a, 1,2)
gap <- as.numeric(b)

tiff("gap_statistics_hclust_12500_2_k80_B500.tiff", 
     height=800, width=1200, res=300)
par(mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
plot(gskmn_12500_k80_B500_hclust, main="Gap Statistic 12500",
     xlim=c(0,80), xlab="", ylab="")
abline(v=gap, lty=2)
mtext(side=1, line=1.1, "k2")
mtext(side=2, line=1.15, "gap statistic")
dev.off()
