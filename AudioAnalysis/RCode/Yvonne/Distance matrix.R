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