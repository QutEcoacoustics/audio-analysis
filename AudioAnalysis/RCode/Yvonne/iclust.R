#PCA type analysis
library(psych)
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k")
clusters_30 <- read.csv("hybrid_clust_knn_17500_3_k30_2hour_full111days.csv",header = T)
ic.out <- iclust(clusters_30[,1:30])
ic.out7 <- iclust(clusters_30[,1:30],nclusters = 8)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust")
ic.out7 <- iclust(clusters_30[,1:30],nclusters = 7)
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust")
ic.out10 <- iclust(clusters_30[,1:30],nclusters = 10)
fa.diagram(ic.out10$pattern,Phi=ic.out10$Phi,main="Pattern taken from iclust")
ic.out8 <- iclust(clusters_30[,1:30],nclusters = 8)
fa.diagram(ic.out8$pattern,Phi=ic.out8$Phi,main="Pattern taken from iclust")
pdf("iclust_hybrid_clust_knn_17500_3_k30_2hour.pdf", width=4,
    height =7)
ic.out <- iclust(clusters_30[,1:30])
mtext(side = 3,line=1.5,"ic.out <- iclust(clusters_30[,1:30]")
ic.out8 <- iclust(clusters_30[,1:30],nclusters = 8)
mtext(side = 3,line=1.5,"ic.out8 <- iclust(clusters_30[,1:30],nclusters = 8)")
ic.out7 <- iclust(clusters_30[,1:30],nclusters = 7)
mtext(side = 3,line=1.5,"ic.out7 <- iclust(clusters_30[,1:30],nclusters = 7)")
ic.out9 <- iclust(clusters_30[,1:30],nclusters = 9)
mtext(side = 3,line=1.5,"ic.out9 <- iclust(clusters_30[,1:30],nclusters = 9)")

fa.diagram(ic.out8$pattern,Phi=ic.out8$Phi,main="Pattern taken from iclust", cex.lab = 100)
mtext(side = 3,line=1.5,"fa.diagram(ic.out8$pattern,Phi=ic.out8$Phi)")
fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust", cex.lab = 100)
mtext(side = 3,line=1.5,"fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi)")
fa.diagram(ic.out9$pattern,Phi=ic.out9$Phi,main="Pattern taken from iclust", cex.lab = 100)
mtext(side = 3,line=1.5,"fa.diagram(ic.out9$pattern,Phi=ic.out9$Phi)")
fa.diagram(ICLUST(clusters_30[,1:30],15,title="Two cluster solution of Thurstone"),main="Input from ICLUST")
mtext(side = 3,line=1.5,"fa.diagram(ICLUST(clusters_30[,1:30],15)")
dev.off()

order <- ic.out$sorted$sorted$item
clusters_30_inorder <- NULL
for (i in order) {
  clusters_30_inorder <- cbind(clusters_30_inorder, clusters_30[,i])
}
# Rename the columns
column.names <- NULL
for (k in order) {
  col.names <- paste("V", k, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(clusters_30_inorder) <- column.names
write.csv(clusters_30_inorder, "clusters_40_inorder.csv")
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


test.simple <- fa(item.sim(16),2,rotate="oblimin")
#if(require(Rgraphviz)) {fa.graph(test.simple) } 
fa.diagram(test.simple)
f3 <- fa(Thurstone,3,rotate="cluster")
fa.diagram(f3,cut=.4,digits=2)
f3l <- f3$loadings
fa.diagram(f3l,main="input from a matrix")
Phi <- f3$Phi
fa.diagram(f3l,Phi=Phi,main="Input from a matrix")
fa.diagram(ICLUST(Thurstone,2,title="Two cluster solution of Thurstone"),main="Input from ICLUST")
het.diagram(Thurstone,levels=list(1:4,5:8,3:7))

test.simple <- fa(clusters_30[,1:30])
#if(require(Rgraphviz)) {fa.graph(test.simple) } 
fa.diagram(test.simple)
f3 <- fa(clusters_30[,1:30],3,rotate="none")
fa.diagram(f3,cut=.4,digits=2)
f3l <- f3$loadings
fa.diagram(f3l,main="input from a matrix")
Phi <- f3$Phi
fa.diagram(f3l,Phi=Phi,main="Input from a matrix")
fa.diagram(ICLUST(Thurstone,2,title="Two cluster solution of Thurstone"),main="Input from ICLUST")
het.diagram(Thurstone,levels=list(1:4,5:8,3:7))
