# 29 July 2015
# Distance Matrix
# 
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
centers <- read.csv("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\Cluster_centers 22-28 June 2015.csv", header = T)
centers <- centers[,2:11] # this removes the leading column of numerals

site <- "GympieNP 22 June 2015"
####################################
as.matrix(dist(centers))

write.table (as.matrix(dist(centers)), 
             file = paste("Distance_matrix_",
             site, ".csv", sep = ""), sep = ",")

View(m)

# Classical multidimensional scaling of a data matrix. 
# Also known as principal coordinates analysis (Gower, 1966).

# Two dimensional analysis
distances <- read.csv("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\Distance_matrix_GympieNP 22 June 2015.csv", header =
T)
dist <- cmdscale(distances[,1:30], k=2)
x <- dist[, 1]
y <- -dist[, 2]
plot(x, y, type = "n", axes = T, ylim=c(-0.3,0.4))
text(x,y, rownames(distances), cex = 0.6)
abline(h = seq(-0.6,0.6,0.1), col = "lightgray", lty = 3)

# One dimensional analysis
distances <- read.csv("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\Distance_matrix_GympieNP 22 June 2015.csv", header =
                        T)
dist <- cmdscale(distances[,1:30], k=1)
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


