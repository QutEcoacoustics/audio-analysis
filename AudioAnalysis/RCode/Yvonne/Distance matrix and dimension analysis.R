# 29 July 2015
# Distance Matrix
#
#  This file is #7 in the sequence:
#  1. Save_Summary_Indices_ as_csv_file.R
#  2. Plot_Towsey_Summary_Indices.R
#  3. Correlation_Matrix.R
#  4. Principal_Component_Analysis.R
#  5. kMeans_Clustering.R
#  6. Quantisation_error.R
# *7. Distance_matrix.R
#  8. Minimising_error.R
#  9. Segmenting_image.R
# 10. 

setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
# The cluster centers have come from kmeans Clustering
centers <- read.csv("Cluster_centers 22-28 June 2015_5,7,9,11,12,13,14,17,20Gympie NP1 .csv", header = T)
centers <- centers[,2:10] # this removes the leading column of numerals

#centers <- read.csv("Cluster_centers 22-28 June 2015_5,9,11,13,14,15,17Gympie NP1 .csv", header = T)
#centers <- centers[,2:8] # this removes the leading column of numerals
#centers <- read.csv("Cluster_centers 22-28 June 2015_5,8,10,13,14,15,17Gympie NP1 .csv", header = T)
#centers <- centers[,2:8] # this removes the leading column of numerals
#centers <- kmeansObj$centers
site <- "GympieNP 22 June 2015"
####################################
as.matrix(dist(centers))
write.table (as.matrix(dist(centers)), 
             file = paste("Distance_matrix_5,7,9,11,12,13,14,17,20",
             site, ".csv", sep = ""), sep = ",")
#write.table (as.matrix(dist(centers)), 
#             file = paste("Distance_matrix_5,8,10,13,14,15,17",
#             site, ".csv", sep = ""), sep = ",")
#write.table (as.matrix(dist(centers)), 
#             file = paste("Distance_matrix_5,9,11,13,14,15,17",
#             site, ".csv", sep = ""), sep = ",")

# Classical multidimensional scaling of a data matrix. 
# Also known as principal coordinates analysis (Gower, 1966).

distances <- read.csv("Distance_matrix_5,7,9,11,12,13,14,17,20GympieNP 22 June 2015.csv", header =T)
#distances <- read.csv(
#  "Distance_matrix_5,9,11,13,14,15,17GympieNP 22 June 2015.csv", header =
#    T)
#distances <- read.csv("Distance_matrix_5,8,10,13,14,15,17GympieNP 22 June 2015.csv", header=T)

# Two dimensional analysis
dist <- cmdscale(distances[,1:35], k=2)
x <- dist[, 1]
y <- -dist[, 2]
plot(x, y, type = "l", axes = T, asp=1)
text(x,y, rownames(distances), cex = 0.6)
abline(h = seq(-0.6,0.6,0.1), col = "lightgray", lty = 3)

# One dimensional analysis
#distances <- read.csv("Distance_matrix_GympieNP 22 June 2015.csv", header =                        T)
                        
dist <- cmdscale(distances[,1:35], k=1)
y <- dist[, 1]
z <- sort(y)

order <- names(z)

x <- c(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0)
y <- as.numeric(b[,2])
plot(y, x, type = 'n')
text(y,x, names(z), cex = 0.6, adj = c(1.5,1.5))
abline(h = seq(-0.6,0.6,0.1), col = "lightgray", lty = 3)

# Sammon's Non-Linear Mapping
library(MASS)
clusters.x <- as.matrix(distances[, -1])
clusters.sam <- sammon(dist(clusters.x))
plot(clusters.sam$points, type = "n")
text(clusters.sam$points, labels = as.character(1:nrow(clusters.x)))

# Generate 10000 random numbers
it <- irnorm(1, count=10000)
random <- NULL

for (i in 1:10000) {
  rand <- abs(as.integer(nextElem(it)*10000))
  random <- c(random, rand)
}
