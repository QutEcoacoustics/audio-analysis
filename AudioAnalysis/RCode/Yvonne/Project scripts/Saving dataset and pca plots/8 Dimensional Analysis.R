# Performing dimensional analysis to determine similarity of clusters
# Author: Yvonne Phillips
# Date:  30 October 2016

# Description: Sammmon Map

# remove all objects in the global environment
rm(list = ls())

library(cluster) # needed for pam function in order to find the medoids

#k1_value <- 25000
#k2_value <- 60
#column <- k2_value/5
#kmeans_centers <- read.csv(paste("hybrid_dataset_centers_",k1_value,
#                                 "_",k2_value,".csv", sep = ""),
#                           header = T)
#num_clus <- unique(kmeans_centers$hybrid.clusters)

#medoids <- NULL
#for(i in 1:length(num_clus)) {
#  a <- which(kmeans_centers$hybrid.clusters==i)
#  points(pam(clust, 1)$medoids, pch = 16, col = "red")
#  medo <- pam(clust,1)$medoids
#  medoids <- rbind(medoids, medo)
#}
#rownames(medoids) <- as.character(as.numeric(num_clus))

#dist_medoids <- dist(medoids)
#a <- hclust(dist_medoids, method = "ward.D2")
#plot(a)

#a <- hclust(dist_medoids, method = "complete")
#plot(a)

#distances <- as.matrix(dist(medoids))

# 60_colours
# Check for col_func in globalEnv otherwise source function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# colour information for each cluster, this function calls a csv
# containing R,G and B columns containing numbers between 0 and 255
col_func(cluster_colours, "colourblind")
colours <- cols[1:(length(cols)-1)]
list_unique_colours <- unique(cluster_colours[,3:5])
# change the same colours to unique colours

list_col <- NULL
unique_colours <- unique(colours)
for(i in 1:length(unique_colours)) {
  a <- which(colours==unique_colours[i])
  if(length(a) > 1) {
    ref <- 0
    for(j in 2:(length(a))) {
      if(cluster_colours[a[j],3] < 230) {
        cluster_colours[a[j],3] <- cluster_colours[a[j],3] + ref + 2  
        ref <- ref + 2  
      }
      if(cluster_colours[a[j],3] >= 230) {
        cluster_colours[a[j],3] <- cluster_colours[a[j],3] - ref - 2  
        ref <- ref + 2  
      }
    }  
  } 
}

library(R.utils)
cols <- NULL
for(i in 1:nrow(cluster_colours)) {
  R_code <- intToHex(cluster_colours$R[i])
  # add padding if necessary
  if(nchar(R_code)==1) {
    R_code <- paste("0", R_code, sep="")
  }
  G_code <- intToHex(cluster_colours$G[i])
  if(nchar(G_code)==1) {
    G_code <- paste("0", G_code, sep="")
  }
  B_code <- intToHex(cluster_colours$B[i])
  if(nchar(B_code)==1) {
    B_code <- paste("0", B_code, sep="")
  }
  col_code <- paste("#",
                    R_code, 
                    G_code,
                    B_code,
                    sep = "")
  cols <- c(cols, col_code)
}
cols <<- cols
colours <- cols[1:(length(cols)-1)] 

# legend colours
Insects <- c(240, 228, 66)
Cicada <- c(230, 159, 0)
Rain <- c(0, 114, 178) 
Quiet <- c(153, 153, 153)
Planes <- c(204, 121, 167)
Wind <- c(86, 180, 233)
Birds <- c(0, 158, 115)
leg_col <- rbind(Insects, Cicada, Rain,
                 Quiet, Planes,
                 Wind, Birds)
colnames(leg_col) <- c("R", "G", "B")
leg_col <- data.frame(leg_col)

cols <- NULL
for(i in 1:nrow(leg_col)) {
  R_code <- intToHex(leg_col$R[i])
  # add padding if necessary
  if(nchar(R_code)==1) {
    R_code <- paste("0", R_code, sep="")
  }
  G_code <- intToHex(leg_col$G[i])
  if(nchar(G_code)==1) {
    G_code <- paste("0", G_code, sep="")
  }
  B_code <- intToHex(leg_col$B[i])
  if(nchar(B_code)==1) {
    B_code <- paste("0", B_code, sep="")
  }
  col_code <- paste("#",
                    R_code, 
                    G_code,
                    B_code,
                    sep = "")
  cols <- c(cols, col_code)
}

leg_col <- data.frame(leg_col)
for(i in 1:length(leg_col[,1])) {
  leg_col$hex[i] <- cols[i]
}

# Two dimensional analysis
# Note cmdscale is PCA
#dist <- cmdscale(distances, k=2)
#x <- dist[, 1]
#y <- -dist[, 2]
#png("plot.png", width = abs(min(x)-max(x))*700, 
#    height = abs(min(y)-max(y))*700)
#plot(x, y, type = "n", axes = T, asp=1,
#     xlim = c(min(x), max(x)), ylim = c(min(y),max(y)))

#text(x,y, rownames(distances), cex = 1,
#     col = colours)
#abline(h = seq(-1.5, 1.5, 0.1), col = "lightgray", lty = 3)
#abline(v = seq(-1.5, 1.5, 0.1), col = "lightgray", lty = 3)
#dev.off()

# One dimensional analysis
#dist <- cmdscale(distances[,1:ncol(distances)], k=2)
#y <- dist[, 1]
#z <- sort(y)

#order <- names(z)
#x <- rep(0,ncol(distances))
#y <- as.numeric(dist[,2])
#plot(y, x, type = 'n',ylim = c(-0.00001,0.00001))
#text(y,x, names(z), cex = 0.6, adj = c(1.5,1.5))
#abline(h = seq(-0.6,0.6,0.1), col = "lightgray", lty = 3)
#abline(v = seq(-1,1,0.1), col = "lightgray", lty = 3)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Sammon's Non-Linear Mapping--------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# plot SAMMON projection
library(MASS)
#clusters.x <- as.matrix(medoids)
#clusters.sam <- sammon(dist(clusters.x), k=12)
# see below on how this is generated
medoids1 <- read.csv("medoids_all_data.csv", header = T)
distances <- as.matrix(dist(medoids1))
clusters.sam <- sammon(distances, k=2)

# size of clusters
clust_sizes <- read.csv("data/2hour_plots_25000_60/Summary_25000_60_annotated.csv", header = T)
clust_sizes <- clust_sizes$Cluster.total
clust_sizes1 <- clust_sizes
#rownames(clust_sizes1) <- as.character(1:61) 
clusters1 <- NULL
clusters1$clusters <- as.numeric(1:length(colours))
clusters1$points1 <- clusters.sam$points[,1]
clusters1$points2 <- clusters.sam$points[,2]
clusters1$size <- clust_sizes[1:60]
clusters1$colours <- NULL
clusters1 <- data.frame(clusters1)
clusters1$clusters <- as.numeric(1:length(clusters1$points1))
clusters1$radius <- sqrt(clusters1$size)

# colours for each class
insects <- "#F0E442"
rain <- "#0072B2"
wind <- "#56B4E9"
birds <- "#009E73"
cicadas <- "#E69F00"
quiet <- "#999999"
planes <- "#CC79A7"

clusters1$colours <- "abcd"
clusters1$border <- "abcd"
#clusters1$colours[1] <- insects
# set the circle and border colours
clusters1[1, 6:7]  <-  c(insects, insects)
clusters1[2, 6:7]  <-  c(rain, birds)
clusters1[3, 6:7]  <-  c(birds, birds)
clusters1[4, 6:7]  <-  c(insects, birds)
clusters1[5, 6:7]  <-  c(quiet, quiet)
clusters1[6, 6:7]  <-  c(quiet, quiet)
clusters1[7, 6:7]  <-  c(cicadas, birds)
clusters1[8, 6:7]  <-  c(cicadas, birds)
clusters1[9, 6:7]  <-  c(wind, wind)
clusters1[10, 6:7]  <-  c(rain, rain)
clusters1[11, 6:7]  <-  c(birds, birds)
clusters1[12, 6:7]  <-  c(cicadas, cicadas)
clusters1[13, 6:7]  <-  c(quiet, quiet)
clusters1[14, 6:7]  <-  c(birds, birds)
clusters1[15, 6:7]  <-  c(birds, birds)
clusters1[16, 6:7]  <-  c(cicadas, cicadas)
clusters1[17, 6:7]  <-  c(rain, insects)
clusters1[18, 6:7]  <-  c(rain, rain)
clusters1[19, 6:7]  <-  c(wind, wind)
clusters1[20, 6:7]  <-  c(wind, wind)
clusters1[21, 6:7]  <-  c(rain, rain)
clusters1[22, 6:7]  <-  c(insects, birds)
clusters1[23, 6:7]  <-  c(planes, planes)
clusters1[24, 6:7]  <-  c(wind, cicadas)
clusters1[25, 6:7]  <-  c(wind, wind)
clusters1[26, 6:7]  <-  c(insects, wind)
clusters1[27, 6:7]  <-  c(insects, insects)
clusters1[28, 6:7]  <-  c(birds, insects)
clusters1[29, 6:7]  <-  c(insects, insects)
clusters1[30, 6:7]  <-  c(wind, quiet)
clusters1[31, 6:7]  <-  c(quiet, quiet)
clusters1[32, 6:7]  <-  c(cicadas, cicadas)
clusters1[33, 6:7]  <-  c(birds, birds)
clusters1[34, 6:7]  <-  c(cicadas, cicadas)
clusters1[35, 6:7]  <-  c(quiet, quiet)
clusters1[36, 6:7]  <-  c(quiet, planes)
clusters1[37, 6:7]  <-  c(birds, birds)
clusters1[38, 6:7]  <-  c(quiet, quiet)
clusters1[39, 6:7]  <-  c(birds, planes)
clusters1[40, 6:7]  <-  c(wind, birds)
clusters1[41, 6:7]  <-  c(quiet, quiet)
clusters1[42, 6:7]  <-  c(wind, wind)
clusters1[43, 6:7]  <-  c(birds, birds)
clusters1[44, 6:7]  <-  c(cicadas, cicadas)
clusters1[45, 6:7]  <-  c(wind, planes)
clusters1[46, 6:7]  <-  c(wind, wind)
clusters1[47, 6:7]  <-  c(wind, wind)
clusters1[48, 6:7]  <-  c(cicadas, cicadas)
clusters1[49, 6:7]  <-  c(planes, planes)
clusters1[50, 6:7]  <-  c(quiet, insects)
clusters1[51, 6:7]  <-  c(wind, wind)
clusters1[52, 6:7]  <-  c(wind, wind) 
clusters1[53, 6:7]  <-  c(quiet, quiet)
clusters1[54, 6:7]  <-  c(rain, birds)
clusters1[55, 6:7]  <-  c(quiet, quiet)
clusters1[56, 6:7]  <-  c(wind, wind)
clusters1[57, 6:7]  <-  c(birds, wind) 
clusters1[58, 6:7]  <-  c(birds, birds)
clusters1[59, 6:7]  <-  c(rain, rain) 
clusters1[60, 6:7]  <-  c(rain, birds) 
clusters1 <- clusters1[order(-clusters1$size),]
leg_col <- as.character(c(rain, birds, cicadas, wind, planes, quiet, insects))
leg_names <- c("rain", "birds", "cicadas", "planes", "quiet","insects")
library(plotrix) # needed for draw.cirle function
max <- 0.0009

png("plots/sammon-map_final.png",width = 1600, height = 1000) 
par(mar=c(5,6,3,2))
plot(clusters1$points1, 
     clusters1$points2, type = "n",
     main = "Sammon map - sixty clusters",
     xlab = "x",ylab = "",
     xlim = c((min(clusters1$points1)-0.05),
              (max(clusters1$points1)+0.45)),
     ylim = c((min(clusters1$points2)-0.11),
              (max(clusters1$points2)+0.13)),
     cex.axis=2, cex.lab=3, las=1, cex.main=3)
mtext(side=2, "y", las=1, cex = 3, line = 3.5)
#abline(h = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
#abline(v = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
for(i in 1:nrow(clusters1)) {
  draw.circle(clusters1$points1[i],
              clusters1$points2[i], 
              radius = max*clusters1$radius[i],
              col = clusters1$colours[i],
              border = clusters1$border[i],
              lwd = 8)
}
# plot the x and y axis to form four quadrants
abline(h = 0, col = "gray50", lwd = 0.4)
abline(v = 0, col = "gray50", lwd = 0.4)
# plot the cluster numbers
text(clusters1$points1, clusters1$points2, 
     labels = as.character(clusters1$clusters), cex = 2.6)
# plot the plot legend
a <-legend("topright", title="Classes", 
       col = leg_col, bty = "n", 
       cex=2.6, leg_names ,y.intersp = 1.2) 
for(j in 1:length(a$text$x)) {
  draw.circle(a$text$x[j]-0.06, a$text$y[j]-0.005, 
              radius = 0.035,
              col = leg_col[j],
              border = "white")
}
# add family to fonts list use windowsFonts() to check current
windowsFonts(A = windowsFont("Times New Roman"))
text(x = 1.4, y = 1.1, "I", cex = 2.4, family="A", font = 2)
text(x = -1.6, y = 1.1, "II", cex = 2.4, family="A", font = 2)
text(x = -1.6, y = -1.05, "III", cex = 2.4, family="A", font = 2)
text(x = 1.4, y = -1.05, "IV", cex = 2.4, family="A", font = 2)
dev.off()

# plotrix is needed for the draw.circle function
library(plotrix)
max <- 0.0009
png("sammon-prac_xx.png", 
    width = 1600, #abs(min(clusters1$points1)
    #  -max(clusters1$points1))*700, 
    height = 1000) #abs(min(clusters1$points2)
#-max(clusters1$points2))*700)
par(mar=c(5,6,3,2))
plot(clusters1$points1, 
     clusters1$points2, type = "n",
     main = "Sammon map - 60 clusters",
     xlab = "x",ylab = "y",
     xlim = c((min(clusters1$points1)-0.05),
              (max(clusters1$points1)+0.45)),
     ylim = c((min(clusters1$points2)-0.1),
              (max(clusters1$points2)+0.05)),
     cex.axis=2, cex.lab=3, las=1, cex.main=3)
abline(h = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
abline(v = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
abline(h = 0, col = "lightgray", lwd = 2)
abline(v = 0, col = "lightgray", lwd = 2)
for(i in 1:nrow(clusters1)) {
  draw.circle(clusters1$points1[i],
              clusters1$points2[i], 
              radius = max*clusters1$radius[i],
              col = clusters1$colours[i],
              border = clusters1$border[i],
              lwd = 6)
}
text(clusters1$points1, clusters1$points2, 
     labels = as.character(clusters1$clusters), cex = 2.6)
a <-legend("topright", title="Classes", 
           col = leg_col$hex, bty = "n", 
           cex=2.6, row.names(leg_col), y.intersp = 1.2) 
for(j in 1:length(a$text$x)) {
  draw.circle(a$text$x[j]-0.06, a$text$y[j]-0.005, 
              radius = 0.035,
              col = as.character(leg_col$hex[j]),
              border = "white")
}
dev.off()

# hierarchical clustering of medoids
dist_medoids <- dist(medoids)
a <- hclust(dist_medoids, method = "ward.D2")
plot(a, hang = -1)

a <- hclust(dist_medoids, method = "complete")
plot(a)

# clustering of medoids from 13 months of data ------------------------------------------------------
# remove all objects in the global environment
rm(list = ls())

# load normalised summary indices this has had the missing minutes
# and microphone problem minutes removed 
# the dataframe is called "indices_norm_summary"
load(file="data/datasets/normalised_summary_indices.RData")

k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- 
  paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
         k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# load the dataset in .RData file
load(file_name)
# load the cluster list 
cluster.list <- get(file_name_short, envir=globalenv())[,column]

data <- cbind(cluster.list, indices_norm_summary)

num_clus <- unique(data$cluster.list)
num_clus <- 1:length(num_clus)

#library(cluster) # needed for pam/clara functions
#medoids <- NULL
#for(i in 1:length(num_clus)) {
#  a <- which(data$cluster.list==i)
#  clust <- data[a,2:ncol(data)]
#  medo <- clara(clust,1)$medoids
#  medoids <- rbind(medoids, medo)
#}
#rownames(medoids) <- as.character(as.numeric(num_clus))

#View(medoids)
#write.csv(medoids, "medoids_all_data.csv", row.names = FALSE)
#medoids1 <- read.csv("medoids_all_data.csv", header = T)

#require(graphics)
#dist_medoids <- dist(medoids)
#a <- hclust(dist_medoids, method = "complete")
#plot(a, hang = -0.1)

#a <- hclust(dist_medoids, method = "complete")
#png("plots/cluster_dendrogram.png", height=600, 
#    width=1200)
#plot(a, cex=1.6, lwd=3)
#dev.off()
