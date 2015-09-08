# 8 September 2015
# 

setwd("C:\\Work\\CSV files\\GympieNP1_new\\kmeans_30clusters")
cluster.4counts <- read.csv("Cluster_4hourcount_kmeans.csv", header = T)

site <- "GympieNP"

normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

normcluster.4counts <- cluster.4counts
# normalise variable columns
for(i in 1:30) {
  normcluster.4counts[,i]  <- normalise(cluster.4counts[,i],  
                                        min(cluster.4counts[,i]),
                                        max(cluster.4counts[,i]))
}
cluster.4counts <- normcluster.4counts
cluster.4counts.pca <- prcomp(cluster.4counts[,1:30], scale. = F)
cluster.4counts$PC1 <- cluster.4counts.pca$x[,1]
sum(cluster.4counts$PC1)
cluster.4counts$PC2 <- cluster.4counts.pca$x[,2]
cluster.4counts$PC3 <- cluster.4counts.pca$x[,3]
cluster.4counts$PC4 <- cluster.4counts.pca$x[,4]
cluster.4counts$PC5 <- cluster.4counts.pca$x[,5]
cluster.4counts$PC6 <- cluster.4counts.pca$x[,6]
cluster.4counts$PC7 <- cluster.4counts.pca$x[,7]
cluster.4counts$PC8 <- cluster.4counts.pca$x[,8]
cluster.4counts$PC9 <- cluster.4counts.pca$x[,9]
plot(cluster.4counts.pca)
biplot(cluster.4counts.pca, cex=0.5)

#Assign colours to time periods
#cluster.4counts <- within(cluster.4counts, levels(time.ref) <- c("red","orange","yellow","green","blue","violet"))

#### Plotting PC1 & PC2 Principal Component Plots with base plotting system
png('pca_plot PC1_PC2_4hour_kmeans_norm0-1.png', 
    width = 1500, height = 1200, units = "px") 
PrinComp_X_axis <- "PC1"
PrinComp_Y_axis <- "PC2"
first <- 1  # change this and values in plot function below!!! to match PC# 
second <- 2  # change this!!! to match PC#
start <- 1
finish <- 332
arrowScale <- 1.3 # increase/decrease this to adjust arrow length
summ <- summary(cluster.4counts.pca)
rotate <- unname(summ$rotation)
labels1 <- names(cluster.4counts[1:length(summ$center)])

mainHeader <- paste (site,cluster.4counts$date[start],
                     cluster.4counts$date[finish],
                     PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
par(mar=c(6,6,4,4))
plot(cluster.4counts$PC1[start:finish],cluster.4counts$PC2[start:finish],  # Change these!!!!! 
     col=as.character(cluster.4counts$timeRef[start:finish]), 
     cex=3, type='p', pch=19, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
     hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels1)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.05, 
       rotate[i,second]*arrowScale*1.05, 
       paste(labels1[i]), cex=2.6)
}
abline (v=0, h=0, lty=2)
clust <- c("12-4am","4-8am","8-12pm","12-4pm","4-8pm","8-12am")
colours <- c("red","orange","yellow","green","blue","violet")
legend('bottomright', clust, pch=19, col=colours, bty='n', 
      cex=3, title = "Time periods")
dev.off()

#### Plotting PC1 & PC3 Principal Component Plots with base plotting system
png('pca_plot PC1_PC3_4hour_kmeans_norm0-1.png', 
    width = 1500, height = 1200, units = "px") 
PrinComp_X_axis <- "PC1"
PrinComp_Y_axis <- "PC3"
first <- 1  # change this and values in plot function below!!! to match PC# 
second <- 3  # change this!!! to match PC#
start <- 1
finish <- 332
arrowScale <- 1.4 # increase/decrease this to adjust arrow length
summ <- summary(cluster.4counts.pca)
rotate <- unname(summ$rotation)
labels1 <- names(cluster.4counts[1:length(summ$center)])

mainHeader <- paste (site,cluster.4counts$date[start],
                     cluster.4counts$date[finish],
                     PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
par(mar=c(6,6,4,4))
plot(cluster.4counts$PC1[start:finish],cluster.4counts$PC3[start:finish],  # Change these!!!!! 
     col=as.character(cluster.4counts$timeRef[start:finish]), 
     cex=3, type='p', pch=19, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels1)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.05, 
       rotate[i,second]*arrowScale*1.05, 
       paste(labels1[i]), cex=2.6)
}
abline (v=0, h=0, lty=2)
clust <- c("12-4am","4-8am","8-12pm","12-4pm","4-8pm","8-12am")
colours <- c("red","orange","yellow","green","blue","violet")
legend('bottomright', clust, pch=19, col=colours, bty='n', 
       cex=3, title = "Time periods")
dev.off()
#################
#### Plotting PC2 & PC3 Principal Component Plots with base plotting system
png('pca_plot PC2_PC3_4hour_kmeans.png', 
    width = 1500, height = 1200, units = "px") 
PrinComp_X_axis <- "PC2"
PrinComp_Y_axis <- "PC3"
first <- 2  # change this and values in plot function below!!! to match PC# 
second <- 3  # change this!!! to match PC#
start <- 1
finish <- 332
arrowScale <- 10 # increase/decrease this to adjust arrow length
summ <- summary(cluster.4counts.pca)
rotate <- unname(summ$rotation)
labels1 <- names(cluster.4counts[1:length(summ$center)])

mainHeader <- paste (site,cluster.4counts$date[start],
                     cluster.4counts$date[finish],
                     PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
par(mar=c(6,6,4,4))
plot(cluster.4counts$PC2[start:finish],cluster.4counts$PC3[start:finish],  # Change these!!!!! 
     col=as.character(cluster.4counts$timeRef[start:finish]), 
     cex=3, type='p', pch=19, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels1)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.05, 
       rotate[i,second]*arrowScale*1.05, 
       paste(labels1[i]), cex=2.6)
}
abline (v=0, h=0, lty=2)
clust <- c("12-4am","4-8am","8-12pm","12-4pm","4-8pm","8-12am")
colours <- c("red","orange","yellow","green","blue","violet")
legend('topleft', clust, pch=19, col=colours, bty='n', 
       cex=3, title = "Time periods")
dev.off()
#################
library(rgl) # using rgl package
#colourSet1 <- c("#FF0000FF","#FF4000FF","#FF8000FF",
#                "#FFBF00FF","#FFFF00FF","#BFFF00FF")
#colourSet2 <- c("#80FF00FF","#40FF00FF","#00FF00FF",
#                "#00FF40FF","#00FF80FF","#00FFBFFF")
#colourSet3 <- c("#00FFFFFF","#00BFFFFF","#0080FFFF",
#                "#0040FFFF","#0000FFFF","#4000FFFF")
#colourSet4 <- c("#8000FFFF","#BF00FFFF","#FF00FFFF",
#                "#FF00BFFF","#FF0080FF","#FF0040FF")

#normIndices <- within(normIndices, levels(fourhour.class) <- 
#                        c(colourSet1,colourSet2, colourSet3,colourSet4))

#day <- c(0, 1440, 2880, 4320, 5760, 7200, 8640, 10080)
#offset <- c(0,360,720,1080)
start <-  1         #day[5] + offset[4] + 1   
finish <- length(normIndices$PC1) #day[7]-1
start
finish
plot3d(normIndices$PC1[start:finish], normIndices$PC2[start:finish], 
       normIndices$PC3[start:finish], 
       col=adjustcolor(normIndices$cluster.list, alpha.f = 0.1))
spheres3d(normIndices$PC1[start:finish], normIndices$PC2[start:finish], 
          normIndices$PC3[start:finish], 
          col=adjustcolor(normIndices$cluster.list, alpha.f = 0.1),
          radius = 0.015)
xyzCoords <- data.frame(x1= numeric(10),  y1= integer(10), 
                        z1 = numeric(10), x2= numeric(10), 
                        y2= integer(10),  z2 = numeric(10))
for (i in 1:8) {
  xyzCoords$x2[i] <- rotate[i,1]
  xyzCoords$y2[i] <- rotate[i,2]
  xyzCoords$z2[i] <- rotate[i,3]
}
# xyz co-ordinates for segments
xyzCoords <- data.frame(x1= numeric(10),  y1= integer(10), 
                        z1 = numeric(10), x2= numeric(10), 
                        y2= integer(10),  z2 = numeric(10))
for (i in 1:9) {
  xyzCoords$x2[i] <- rotate[i,1]*0.8
  xyzCoords$y2[i] <- rotate[i,2]*0.8
  xyzCoords$z2[i] <- rotate[i,3]*0.8
}
segments3d(x=as.vector(t(xyzCoords[1:10,c(1,4)])),
           y=as.vector(t(xyzCoords[1:10,c(2,5)])),
           z=as.vector(t(xyzCoords[1:10,c(3,6)])), 
           lwd=2, col= "midnightblue")
###################
dates <- unique(cluster.4counts$date)

png('pca_PC1_PC2_daily_cycle.png', 
    width = 1500, height = 1200, units = "px") 
par(mfrow=c(5,4), mar=c(0,0,0,0))
plot(cluster.4counts$PC1[1:6],cluster.4counts$PC2[1:6], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[1]), cex=0.8)
plot(cluster.4counts$PC1[6:12],cluster.4counts$PC2[6:12], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[2]), cex=0.8)
plot(cluster.4counts$PC1[12:18],cluster.4counts$PC2[12:18], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[3]), cex=0.8)
plot(cluster.4counts$PC1[18:24],cluster.4counts$PC2[18:24], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[4]), cex=0.8)
plot(cluster.4counts$PC1[24:30],cluster.4counts$PC2[24:30], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[5]), cex=0.8)
plot(cluster.4counts$PC1[30:36],cluster.4counts$PC2[30:36], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[6]), cex=0.8)
plot(cluster.4counts$PC1[36:42],cluster.4counts$PC2[36:42], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[7]), cex=0.8)
plot(cluster.4counts$PC1[42:48],cluster.4counts$PC2[42:48], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[8]), cex=0.8)
plot(cluster.4counts$PC1[48:54],cluster.4counts$PC2[48:54], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[9]), cex=0.8)
plot(cluster.4counts$PC1[54:60],cluster.4counts$PC2[54:60], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[10]), cex=0.8)
plot(cluster.4counts$PC1[60:66],cluster.4counts$PC2[60:66], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[11]), cex=0.8)
plot(cluster.4counts$PC1[66:72],cluster.4counts$PC2[66:72], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[12]), cex=0.8)
plot(cluster.4counts$PC1[72:78],cluster.4counts$PC2[72:78], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[13]), cex=0.8)
plot(cluster.4counts$PC1[78:84],cluster.4counts$PC2[78:84], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[14]), cex=0.8)
plot(cluster.4counts$PC1[84:90],cluster.4counts$PC2[84:90], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[15]), cex=0.8)
plot(cluster.4counts$PC1[90:96],cluster.4counts$PC2[90:96], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[16]), cex=0.8)
plot(cluster.4counts$PC1[96:102],cluster.4counts$PC2[96:102], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[17]), cex=0.8)
plot(cluster.4counts$PC1[102:108],cluster.4counts$PC2[102:108], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[18]), cex=0.8)
plot(cluster.4counts$PC1[108:114],cluster.4counts$PC2[108:114], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[19]), cex=0.8)
plot(cluster.4counts$PC1[114:120],cluster.4counts$PC2[114:120], type="l",
     xlim = c(-5,5), ylim = c(-4,4))
mtext(side = 3,line=-1, paste(dates[20]), cex=0.8)
dev.off()