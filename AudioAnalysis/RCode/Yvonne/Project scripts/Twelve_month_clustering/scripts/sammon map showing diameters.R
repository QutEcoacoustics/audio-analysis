rm(list = ls())
# load the indices_norm_summary dataframe
load("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\data\\datasets\\normalised_summary_indices.RData")

# plot SAMMON projection
library(MASS)
#clusters.x <- as.matrix(medoids)
#clusters.sam <- sammon(dist(clusters.x), k=12)
# see below on how this is generated

medoids1 <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\medoids_all_data.csv", header = T)
distances <- as.matrix(dist(medoids1))
clusters.sam <- sammon(distances, k=2)

# size of clusters

clust_sizes <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\2hour_plots_25000_60\\Summary_25000_60_annotated.csv", header = T)
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

# calculate the distance between the medoids of cluster 5 and 42
med5 <- medoids1[5,]
med42 <- medoids1[42,]
data.fr <- rbind(med5, med42)
dist1 <- dist(data.fr)
# calculate the distance between the points of cluster 5 and 42 in 
# sammon space
point5 <- clusters.sam$points[5,]
point42 <- clusters.sam$points[42,]
data.fr <- rbind(point5, point42)
dist2 <- dist(data.fr)
scale_factor <- dist2/dist1
scale_factor

# colours for each class
insects <- "#F0E442"
rain <- "#0072B2"
wind <- "#56B4E9"
birds <- "#009E73"
cicadas <- "#E69F00"
quiet <- "#999999"
planes <- "#CC79A7"

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
#rm(hclust_clusters_25000)
# load the cluster list 
cluster.list <- get(file_name_short, envir=globalenv())[,column]

data <- cbind(cluster.list, indices_norm_summary)

num_clus <- unique(data$cluster.list)
num_clus <- 1:length(num_clus)

library(cluster) # needed for pam/clara functions
medoids <- NULL
for(i in 1:length(num_clus)) {
  a <- which(data$cluster.list==i)
  clust <- data[a,2:ncol(data)]
  medo <- clara(clust,1)$medoids
  medoids <- rbind(medoids, medo)
}
rownames(medoids) <- as.character(as.numeric(num_clus))

# Alternatively 
medoids1 <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\medoids_all_data.csv", header = T)

# calculate distances from each medoid to each point in the cluster
statistics <- NULL
statistics <- data.frame(statistics)
for(j in 1:60) {
  clust_num <- j
  a <- which(data$cluster.list==clust_num)
  temp_data <- data[a,]
  data_frame <- data.frame(BGN=0,
                           SNR=0,
                           ACT=0,
                           EVN=0,
                           HFC=0,
                           MFC=0,
                           LFC=0,
                           ACI=0,
                           EAS=0,
                           EPS=0,
                           ECS=0,
                           CCL=0)
  # find the distance from each cluster medoid to each point
  distances <- NULL
  data_frame[1, 1:12] <- medoids1[j,]
  for(i in 1:nrow(temp_data)) {
    data_frame[2, 1:12] <- temp_data[i, 2:13]
    d <- dist(data_frame)
    distances <- c(distances, d)
  }
  statistics[j,1] <- mean(distances)
  statistics[j,2] <- sd(distances)
  pvec <- 0.9         #seq(0,1,0.1)
  statistics[j,3] <- quantile(distances,pvec)
}

statistics$mean_1sd <- as.vector(statistics$V1) + as.vector(statistics$V2)
statistics$mean_2sd <- as.vector(statistics$V1) + 2*as.vector(statistics$V2)

plot(statistics$V1) # mean
par(new=T)
plot(statistics$V2, col="red") #standard deviation
#abline(v=c(42,51), col="red")
#abline(v=c(10,18,59,60), col="blue")
abline(v=c(10,20,30,40,50,60), col="darkgrey")

library(MASS)
distances <- as.matrix(dist(medoids1))
clusters.sam <- sammon(distances, k=2)
clust_sammon_pts <- clusters.sam$points
#View(clust_sammon_pts) # Sammon co-ordinates

#write.csv(as.data.frame(as.matrix(dist_sammon)), "distance_matrix_sammon_plot.csv")
#View(as.data.frame(as.matrix(dist_sammon)))

radius <- statistics$V3
clusters1$radius <- radius
# Check for col_func in globalEnv otherwise source function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# colour information for each cluster, this function calls a csv
# containing R,G and B columns containing numbers between 0 and 255
col_func(cluster_colours, "colourblind")
colours <- cols[1:(length(cols)-1)]

clusters1 <- NULL
clusters1$clusters <- as.numeric(1:length(colours))
clusters1$points1 <- clusters.sam$points[,1] #list$`1`
clusters1$points2 <- clusters.sam$points[,2] #list$`2`
clusters1$size <- radius[1:60]
clusters1$colours <- NULL
clusters1 <- data.frame(clusters1)
clusters1$clusters <- as.numeric(1:length(clusters1$points1))
clusters1$radius <- radius

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
leg_names <- c("Rain", "Birds", "Cicadas", "Wind", "Planes", "Quiet","Orthopterans")
library(plotrix) # needed for draw.cirle function
max <- 0.19
# previously titled "Sammon_map_distances_90.tiff"
tiff("Sammon_map_diameters.tiff", width = 2050, 
     height = 1500, units = 'px', res = 300)
par(mar=c(1.5, 1.9, 1, 0.4), 
    cex = 1, cex.axis = 1, cex.main = 2.4)
plot(clusters1$points1, 
     clusters1$points2, type = "n",
     main = "Sammon map of cluster medoids",
     xlab = "x",ylab = "", mgp =c(2,0.5,0),
     xlim = c((min(clusters1$points1)-0.05),
              (max(clusters1$points1)+0.25)),
     ylim = c((min(clusters1$points2)-0.11),
              (max(clusters1$points2)+0.13)),
     cex.axis=1, cex.lab=0.6, las=1, cex.main=1,
     bg = "transparent")
mtext(side=2, "y", las=1, cex = 3, line = 3.5)
#abline(h = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
#abline(v = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
for(i in 1:nrow(clusters1)) {
  draw.circle(clusters1$points1[i],
              clusters1$points2[i], 
              radius = max*clusters1$radius[i],
              col = clusters1$colours[i],
              border = clusters1$border[i],
              lwd = 6)
}
# plot the x and y axis to form four quadrants
abline(h = 0, col = "gray40", lwd = 1)
abline(v = 0, col = "gray40", lwd = 1)
# plot the cluster numbers
text(clusters1$points1, clusters1$points2, 
     labels = as.character(clusters1$clusters), cex = 1)
# plot the plot legend
a <-legend("topright", title="Classes", 
           col = leg_col, bty = "n", 
           cex=1.1, leg_names , y.intersp = 0.85) 
for(j in 1:length(a$text$x)) {
  draw.circle(a$text$x[j]-0.06, a$text$y[j]-0.005, 
              radius = 0.035,
              col = leg_col[j],
              border = "white")
}
# add family to fonts list use windowsFonts() to check current
windowsFonts(A = windowsFont("Times New Roman"))
text(x = 1.75, y = 1.1, "I", cex = 1, family="A", font = 2)
text(x = -1.6, y = 1.1, "II", cex = 1, family="A", font = 2)
text(x = -1.6, y = -1.05, "III", cex = 1, family="A", font = 2)
text(x = 1.75, y = -1.05, "IV", cex = 1, family="A", font = 2)
dev.off()

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Combined Sammon map ------------------------------
# top proportional to the number of minutes in cluster
# bottom proportional to the diameter of the cluster
# also see sammon map showing diameters.R
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())
dev.off()
library(cluster) # needed for pam function in order to find the medoids

# plot SAMMON projection
library(MASS)
# see below on how this is generated
medoids1 <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\medoids_all_data.csv", header = T)
distances <- as.matrix(dist(medoids1))
clusters.sam <- sammon(distances, k=2)

# size of clusters

clust_sizes <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\2hour_plots_25000_60\\Summary_25000_60_annotated.csv", header = T)
clust_sizes <- clust_sizes$Cluster.total
clust_sizes1 <- clust_sizes

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
leg_names <- c("Rain", "Birds", "Cicadas", "Wind", 
               "Planes", "Quiet","Orthopterans")
library(plotrix) # needed for draw.cirle function
max <- 0.0009

tiff("Combined_Sammon_maps.tiff", width = (0.8459259*2025), 
     height = (0.8459259*2362), units = 'px', res = 300)
par(mfrow = c(2,1), mar=c(1.5, 2, 1, 0.4), 
    cex = (0.8459259*1), cex.axis = (0.8459259*1), 
    cex.main = (0.8459259*2.4))
# 1st sammon plot
plot(clusters1$points1, 
     clusters1$points2, type = "n",
     main = "Sammon map of cluster medoids",
     xlab = "x",ylab = "", mgp =c(2,0.5,0),
     xlim = c((min(clusters1$points1)-0.05),
              (max(clusters1$points1)+0.6)),
     ylim = c((min(clusters1$points2)-0.11),
              (max(clusters1$points2)+0.13)),
     cex.axis=1, cex.lab=0.6, las=1, cex.main=1,
     bg = "transparent")
mtext(side=2, "y", las=1, cex = 3, line = 3.5)
#abline(h = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
#abline(v = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
for(i in 1:nrow(clusters1)) {
  draw.circle(clusters1$points1[i],
              clusters1$points2[i], 
              radius = max*clusters1$radius[i],
              col = clusters1$colours[i],
              border = clusters1$border[i],
              lwd = (0.8459259*6))
}
# plot the x and y axis to form four quadrants
abline(h = 0, col = "gray40", lwd = 1)
abline(v = 0, col = "gray40", lwd = 1)
# plot the cluster numbers
text(clusters1$points1, clusters1$points2, 
     labels = as.character(clusters1$clusters), cex = 1)
# plot the plot legend
a <-legend("topright", title="Classes", 
           col = leg_col, bty = "n", 
           cex=1.1, leg_names , y.intersp = 0.85) 
for(j in 1:length(a$text$x)) {
  draw.circle(a$text$x[j]-0.06, a$text$y[j]-0.005, 
              radius = 0.035,
              col = leg_col[j],
              border = "white")
}
# add family to fonts list use windowsFonts() to check current
windowsFonts(A = windowsFont("Times New Roman"))
text(x = 2, y = 1.1, "I", cex = 1, family="A", font = 2)
text(x = -1.6, y = 1.1, "II", cex = 1, family="A", font = 2)
text(x = -1.6, y = -1.05, "III", cex = 1, family="A", font = 2)
text(x = 2, y = -1.05, "IV", cex = 1, family="A", font = 2)
mtext(at = 1.29, line = 0.5, side = 2, "a.", cex=1.2, las=1)

# 2nd sammon plot
load("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\data\\datasets\\normalised_summary_indices.RData")
library(plotrix)
# plot SAMMON projection
library(MASS)
#clusters.x <- as.matrix(medoids)
#clusters.sam <- sammon(dist(clusters.x), k=12)
# see below on how this is generated

medoids1 <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\medoids_all_data.csv", header = T)
distances <- as.matrix(dist(medoids1))
clusters.sam <- sammon(distances, k=2)

# size of clusters

clust_sizes <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\2hour_plots_25000_60\\Summary_25000_60_annotated.csv", header = T)
clust_sizes <- clust_sizes$Cluster.total
clust_sizes1 <- clust_sizes
#rownames(clust_sizes1) <- as.character(1:61) 
clusters2 <- NULL
clusters2$clusters <- as.numeric(1:length(colours))
clusters2$points1 <- clusters.sam$points[,1]
clusters2$points2 <- clusters.sam$points[,2]
clusters2$size <- clust_sizes[1:60]
clusters2$colours <- NULL
clusters2 <- data.frame(clusters2)
clusters2$clusters <- as.numeric(1:length(clusters2$points1))

# calculate the distance between the medoids of cluster 5 and 42
med5 <- medoids1[5,]
med42 <- medoids1[42,]
data.fr <- rbind(med5, med42)
dist1 <- dist(data.fr)
# calculate the distance between the points of cluster 5 and 42 in 
# sammon space
point5 <- clusters.sam$points[5,]
point42 <- clusters.sam$points[42,]
data.fr <- rbind(point5, point42)
dist2 <- dist(data.fr)
scale_factor <- dist2/dist1
scale_factor

# colours for each class
insects <- "#F0E442"
rain <- "#0072B2"
wind <- "#56B4E9"
birds <- "#009E73"
cicadas <- "#E69F00"
quiet <- "#999999"
planes <- "#CC79A7"

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
#rm(hclust_clusters_25000)
# load the cluster list 
cluster.list <- get(file_name_short, envir=globalenv())[,column]

data <- cbind(cluster.list, indices_norm_summary)

num_clus <- unique(data$cluster.list)
num_clus <- 1:length(num_clus)

library(cluster) # needed for pam/clara functions
medoids <- NULL
for(i in 1:length(num_clus)) {
  a <- which(data$cluster.list==i)
  clust <- data[a,2:ncol(data)]
  medo <- clara(clust,1)$medoids
  medoids <- rbind(medoids, medo)
}
rownames(medoids) <- as.character(as.numeric(num_clus))

# Alternatively 
medoids1 <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\medoids_all_data.csv", header = T)

# calculate distances from each medoid to each point in the cluster
statistics <- NULL
statistics <- data.frame(statistics)
for(j in 1:60) {
  clust_num <- j
  a <- which(data$cluster.list==clust_num)
  temp_data <- data[a,]
  data_frame <- data.frame(BGN=0,
                           SNR=0,
                           ACT=0,
                           EVN=0,
                           HFC=0,
                           MFC=0,
                           LFC=0,
                           ACI=0,
                           EAS=0,
                           EPS=0,
                           ECS=0,
                           CCL=0)
  # find the distance from each cluster medoid to each point
  distances <- NULL
  data_frame[1, 1:12] <- medoids1[j,]
  for(i in 1:nrow(temp_data)) {
    data_frame[2, 1:12] <- temp_data[i, 2:13]
    d <- dist(data_frame)
    distances <- c(distances, d)
  }
  statistics[j,1] <- mean(distances)
  statistics[j,2] <- sd(distances)
  pvec <- 0.90          #seq(0,1,0.1)
  statistics[j,3] <- quantile(distances,pvec)
}

statistics$mean_1sd <- as.vector(statistics$V1) + as.vector(statistics$V2)
statistics$mean_2sd <- as.vector(statistics$V1) + 2*as.vector(statistics$V2)

#plot(statistics$V1)
#par(new=T)
#plot(statistics$V2, col="red")
#abline(v=c(10,20,30,40,50,60), col="darkgrey")

library(MASS)
distances <- as.matrix(dist(medoids1))
clusters.sam <- sammon(distances, k=2)
clust_sammon_pts <- clusters.sam$points
#View(clust_sammon_pts) # Sammon co-ordinates

#write.csv(as.data.frame(as.matrix(dist_sammon)), "distance_matrix_sammon_plot.csv")
#View(as.data.frame(as.matrix(dist_sammon)))

radius <- statistics$V3
clusters2$radius <- radius
# Check for col_func in globalEnv otherwise source function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# colour information for each cluster, this function calls a csv
# containing R,G and B columns containing numbers between 0 and 255
col_func(cluster_colours, "colourblind")
colours <- cols[1:(length(cols)-1)]

clusters2 <- NULL
clusters2$clusters <- as.numeric(1:length(colours))
clusters2$points1 <- clusters.sam$points[,1] #list$`1`
clusters2$points2 <- clusters.sam$points[,2] #list$`2`
clusters2$size <- radius[1:60]
clusters2$colours <- NULL
clusters2 <- data.frame(clusters2)
clusters2$clusters <- as.numeric(1:length(clusters2$points1))
clusters2$radius <- radius

# colours for each class
insects <- "#F0E442"
rain <- "#0072B2"
wind <- "#56B4E9"
birds <- "#009E73"
cicadas <- "#E69F00"
quiet <- "#999999"
planes <- "#CC79A7"

clusters2$colours <- "abcd"
clusters2$border <- "abcd"
#clusters2$colours[1] <- insects
# set the circle and border colours
clusters2[1, 6:7]  <-  c(insects, insects)
clusters2[2, 6:7]  <-  c(rain, birds)
clusters2[3, 6:7]  <-  c(birds, birds)
clusters2[4, 6:7]  <-  c(insects, birds)
clusters2[5, 6:7]  <-  c(quiet, quiet)
clusters2[6, 6:7]  <-  c(quiet, quiet)
clusters2[7, 6:7]  <-  c(cicadas, birds)
clusters2[8, 6:7]  <-  c(cicadas, birds)
clusters2[9, 6:7]  <-  c(wind, wind)
clusters2[10, 6:7]  <-  c(rain, rain)
clusters2[11, 6:7]  <-  c(birds, birds)
clusters2[12, 6:7]  <-  c(cicadas, cicadas)
clusters2[13, 6:7]  <-  c(quiet, quiet)
clusters2[14, 6:7]  <-  c(birds, birds)
clusters2[15, 6:7]  <-  c(birds, birds)
clusters2[16, 6:7]  <-  c(cicadas, cicadas)
clusters2[17, 6:7]  <-  c(rain, insects)
clusters2[18, 6:7]  <-  c(rain, rain)
clusters2[19, 6:7]  <-  c(wind, wind)
clusters2[20, 6:7]  <-  c(wind, wind)
clusters2[21, 6:7]  <-  c(rain, rain)
clusters2[22, 6:7]  <-  c(insects, birds)
clusters2[23, 6:7]  <-  c(planes, planes)
clusters2[24, 6:7]  <-  c(wind, cicadas)
clusters2[25, 6:7]  <-  c(wind, wind)
clusters2[26, 6:7]  <-  c(insects, wind)
clusters2[27, 6:7]  <-  c(insects, insects)
clusters2[28, 6:7]  <-  c(birds, insects)
clusters2[29, 6:7]  <-  c(insects, insects)
clusters2[30, 6:7]  <-  c(wind, quiet)
clusters2[31, 6:7]  <-  c(quiet, quiet)
clusters2[32, 6:7]  <-  c(cicadas, cicadas)
clusters2[33, 6:7]  <-  c(birds, birds)
clusters2[34, 6:7]  <-  c(cicadas, cicadas)
clusters2[35, 6:7]  <-  c(quiet, quiet)
clusters2[36, 6:7]  <-  c(quiet, planes)
clusters2[37, 6:7]  <-  c(birds, birds)
clusters2[38, 6:7]  <-  c(quiet, quiet)
clusters2[39, 6:7]  <-  c(birds, planes)
clusters2[40, 6:7]  <-  c(wind, birds)
clusters2[41, 6:7]  <-  c(quiet, quiet)
clusters2[42, 6:7]  <-  c(wind, wind)
clusters2[43, 6:7]  <-  c(birds, birds)
clusters2[44, 6:7]  <-  c(cicadas, cicadas)
clusters2[45, 6:7]  <-  c(wind, planes)
clusters2[46, 6:7]  <-  c(wind, wind)
clusters2[47, 6:7]  <-  c(wind, wind)
clusters2[48, 6:7]  <-  c(cicadas, cicadas)
clusters2[49, 6:7]  <-  c(planes, planes)
clusters2[50, 6:7]  <-  c(quiet, insects)
clusters2[51, 6:7]  <-  c(wind, wind)
clusters2[52, 6:7]  <-  c(wind, wind) 
clusters2[53, 6:7]  <-  c(quiet, quiet)
clusters2[54, 6:7]  <-  c(rain, birds)
clusters2[55, 6:7]  <-  c(quiet, quiet)
clusters2[56, 6:7]  <-  c(wind, wind)
clusters2[57, 6:7]  <-  c(birds, wind) 
clusters2[58, 6:7]  <-  c(birds, birds)
clusters2[59, 6:7]  <-  c(rain, rain) 
clusters2[60, 6:7]  <-  c(rain, birds) 
clusters2 <- clusters2[order(-clusters2$size),]
leg_col <- as.character(c(rain, birds, cicadas, wind, planes, quiet, insects))
leg_names <- c("Rain", "Birds", "Cicadas", "Wind", "Planes", "Quiet", "Orthopterans")
library(plotrix) # needed for draw.cirle function
max <- 0.16

plot(clusters2$points1, 
     clusters2$points2, type = "n",
     main = "Sammon map of cluster medoids",
     xlab = "x",ylab = "", mgp =c(2,0.5,0),
     xlim = c((min(clusters2$points1)-0.05),
              (max(clusters2$points1)+0.6)),
     ylim = c((min(clusters2$points2)-0.11),
              (max(clusters2$points2)+0.13)),
     cex.axis=1, cex.lab=0.6, las=1, cex.main=1,
     bg = "transparent")
mtext(side=2, "y", las=1, cex = 3, line = 3.5)
#abline(h = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
#abline(v = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
for(i in 1:nrow(clusters2)) {
  draw.circle(x = clusters2$points1[i],
              y = clusters2$points2[i], 
              radius = max*clusters2$radius[i],
              col = clusters2$colours[i],
              border = clusters2$border[i],
              lwd = (0.8459259*6))
}
# plot the x and y axis to form four quadrants
abline(h = 0, col = "gray40", lwd = 1)
abline(v = 0, col = "gray40", lwd = 1)
# plot the cluster numbers
text(clusters2$points1, clusters2$points2, 
     labels = as.character(clusters2$clusters), cex = 1)
# plot the plot legend
a <-legend("topright", title="Classes", 
           col = leg_col, bty = "n", 
           cex=1.1, leg_names , y.intersp = 0.85) 
for(j in 1:length(a$text$x)) {
  draw.circle(a$text$x[j]-0.06, a$text$y[j]-0.005, 
              radius = 0.035,
              col = leg_col[j],
              border = "white")
}
# add family to fonts list use windowsFonts() to check current
windowsFonts(A = windowsFont("Times New Roman"))
text(x = 2, y = 1.1, "I", cex = 1, family="A", font = 2)
text(x = -1.6, y = 1.1, "II", cex = 1, family="A", font = 2)
text(x = -1.6, y = -1.05, "III", cex = 1, family="A", font = 2)
text(x = 2, y = -1.05, "IV", cex = 1, family="A", font = 2)

mtext(at = 1.29, line = 0.5, side = 2, "b.", cex=1.2, las=1)

dev.off()
