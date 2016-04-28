# 27 April 2016
# calculates the correlation between clusters at the 
# two sites

setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j")
clusters_30 <- read.csv("hybrid_clust_knn_17500_3_k30_2hour_full111days.csv",header=T)
cl <- clusters_30[,1:30]
View(cl)
seq(1, length(clusters_30$V1),12)
d <- rbind(cl[1:111,12], cl[112:222,12])
d <- t(d)
e <- cor(d)
e


# 14 November 2015
# Work out which clusters are correlated in each of the 2 hour "histogram" files that came 
# from the "Twelve_day_testing.R" file

#setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3f")
#PCA type analysis
library(psych)

clusters_10 <- read.csv("hybrid_clust_knn_17500_3_k10_2hour_full111days.csv",header=T)
cor_10 <- cor(clusters_10[,1:10])
#write.csv(cor_10, "cor_hybrid_clust_knn_17500_3_k10.csv")
ic.out <- iclust(clusters_10[,1:10])
ic.out7 <- iclust(clusters_10[,1:10],nclusters = 6)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust") 

clusters_20 <- read.csv("hybrid_clust_knn_17500_3_k20_2hour_full111days.csv",header=T)
cor_20 <- cor(clusters_20[,1:20])
#View(cor_20)
#write.csv(cor_20, "cor_hybrid_clust_knn_17500_3_k20.csv")
ic.out <- iclust(clusters_20[,1:10])
ic.out7 <- iclust(clusters_20[,1:10],nclusters = 6)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust") 

clusters_30 <- read.csv("hybrid_clust_knn_17500_3_k30_2hour_full111days.csv",header=T)

cor_30 <- cor(clusters_30[,1:30])
View(cor_30)
write.csv(cor_30, "cor_hybrid_clust_knn_17500_3_k30.csv")
ic.out <- iclust(clusters_30[,1:30])
ic.out7 <- iclust(clusters_30[,1:30],nclusters = 6)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust") 

clusters_40 <- read.csv("hybrid_clust_knn_17500_3_k40_2hour_full111days.csv",header=T)
cor_40 <- cor(clusters_40[,1:40])
View(cor_40)
#write.csv(cor_40, "cor_hybrid_clust_knn_17500_3_k40.csv")
ic.out <- iclust(clusters_40[,1:40])
ic.out8 <- iclust(clusters_40[,1:40],nclusters = 8)
fa.diagram(ic.out8$pattern,Phi=ic.out8$Phi,main="Pattern taken from iclust") 
pdf("iclust_hybrid_clust_knn_17500_3_k40_2hour.pdf", width=4,
    height =7)
ic.out <- iclust(clusters_40[,1:40])
ic.out8 <- iclust(clusters_40[,1:40],nclusters = 8)
fa.diagram(ic.out8$pattern,Phi=ic.out8$Phi,main="Pattern taken from iclust", cex.lab = 100) 
dev.off()

order <- ic.out$sorted$sorted$item
clusters_40_inorder <- NULL
for (i in order) {
  clusters_40_inorder <- cbind(clusters_40_inorder, clusters_40[,i])
}
# Rename the columns
column.names <- NULL
for (k in order) {
  col.names <- paste("V", k, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(clusters_40_inorder) <- column.names
write.csv(clusters_40_inorder, "clusters_40_inorder.csv")



clusters_50 <- read.csv("hybrid_clust_knn_17500_3_k50_2hour_full111days.csv",header=T)
cor_50 <- cor(clusters_50[,1:50])
#View(cor_50)
#write.csv(cor_50, "cor_hybrid_clust_knn_17500_3_k50.csv")
ic.out <- iclust(clusters_50[,1:50])
ic.out7 <- iclust(clusters_50[,1:10],nclusters = 6)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust") 

clusters_60 <- read.csv("hybrid_clust_knn_17500_3_k60_2hour_full111days.csv",header=T)
cor_60 <- cor(clusters_60[,1:60])
#View(cor_60)
#write.csv(cor_60, "cor_hybrid_clust_knn_17500_3_k60.csv")
ic.out <- iclust(clusters_60[,1:60])
ic.out7 <- iclust(clusters_60[,1:30],nclusters = 6)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust", cex.lab = 100) 

pdf("iclust_hybrid_clust_knn_17500_3_k60_2hour.pdf", width=4,
    height =7)
ic.out <- iclust(clusters_60[,1:60])
ic.out7 <- iclust(clusters_60[,1:60],nclusters = 7)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust", cex.lab = 100) 
dev.off()

order <- ic.out$sorted$sorted$item
clusters_60_inorder <- NULL
for (i in order) {
  clusters_60_inorder <- cbind(clusters_60_inorder, clusters_60[,i])
}
# Rename the columns
column.names <- NULL
for (k in order) {
  col.names <- paste("V", k, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(clusters_60_inorder) <- column.names
write.csv(clusters_60_inorder, "clusters_60_inorder.csv")



clusters_70 <- read.csv("hybrid_clust_knn_17500_3_k70_2hour_full111days.csv",header=T)
cor_70 <- cor(clusters_70[,1:70])
View(cor_70)
write.csv(cor_70, "cor_hybrid_clust_knn_17500_3_k70.csv")

clusters_80 <- read.csv("hybrid_clust_knn_17500_3_k80_2hour_full111days.csv",header=T)
cor_80 <- cor(clusters_80[,1:80])
View(cor_80)
write.csv(cor_80, "cor_hybrid_clust_knn_17500_3_k80.csv")

clusters_90 <- read.csv("hybrid_clust_knn_17500_3_k90_2hour_full111days.csv",header=T)
cor_90 <- cor(clusters_90[,1:90])
View(cor_90)
write.csv(cor_90, "cor_hybrid_clust_knn_17500_3_k90.csv")

clusters_100 <- read.csv("hybrid_clust_knn_17500_3_k100_2hour_full111days.csv",header=T)
cor_100 <- cor(clusters_100[,1:100])
View(cor_100)
write.csv(cor_100, "cor_hybrid_clust_knn_17500_3_k100.csv")



