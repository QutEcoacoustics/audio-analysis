#######################################################
# 2 November 2015
# This code performs the hybrid method on a long-duration dataset 
# (about four months ats two sites, 111 days multiplied by 2)
#
# The four month dataset goes from 22 June 2015 to 10 Oct 2015 from two 
# sites GympieNP and Woondum3
#######################################################
setwd("C:\\Work\\CSV files\\FourMonths\\")

AcousticDS <- read.csv("final_dataset_22June2015_10 Oct2015.csv", header = T)
#ds3 <- AcousticDS[,c(2:18)]
ds3 <- AcousticDS[,c(3,4,7,10,11,15,16,25)] # without Mid-frequency cover #25 is minute.of.day
#ds3 <- AcousticDS[,c(3,4,7,9,10,11,15,16)] # with Mid-frequency cover

############## replace NA values
site1 <- ds3[1:(length(ds3$BackgroundNoise)/2),]
site2 <- ds3[((length(ds3$BackgroundNoise)/2)+1):(length(ds3$BackgroundNoise)),]

for(i in 1:(ncol(site1)-1)) {  # columns
  for(j in 1:nrow(site1)) {  # rows 
    if (is.na(site1[j,i])) {
      average <- mean(c(site1[(j-30),i], site1[(j-25),i], site1[(j-20),i],
                        site1[(j-15),i], site1[(j-10),i], site1[(j-5),i],
                        site1[(j+30),i], site1[(j+25),i], site1[(j+20),i],
                        site1[(j+15),i], site1[(j+10),i], site1[(j+5),i]),
                        na.rm=TRUE)
      site1[j,i] <- average
    }
  }
}
# checked to here
for(i in 1:(ncol(site2)-1)) {  # columns
  for(j in 1:nrow(site2)) {  # rows
    if (is.na(site2[j,i])) {
      average <- mean(c(site2[(j-30),i], site2[(j-25),i], site2[(j-20),i],
                        site2[(j-15),i], site2[(j-10),i], site2[(j-5),i],
                        site2[(j+30),i], site2[(j+25),i], site2[(j+20),i],
                        site2[(j+15),i], site2[(j+10),i], site2[(j+5),i]),
                      na.rm=TRUE)
      site2[j,i] <- average   
    }
  }
}

ds3 <- rbind(site1[,1:7], site2[,1:7])
#################################
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3d")
#Hybrid_3_4_7_10_11_15_16_knn_k_1
# PCA type analysis
#library(psych)
#ic.out <- iclust(AcousticDS[,4:10])
#ic.out7 <- iclust(AcousticDS[,3:18],nclusters = 7)
#fa.diagram(ic.out7$pattern,Phi=ic.out7$Phi,main="Pattern taken from iclust") 

#######################################################
# function - normalise
#######################################################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
#######################################################
# Create ds3.norm_2_98 for kmeans, clara, hclust
# a dataset normalised between 2 and 98%
#######################################################
ds3.norm_2_98 <- ds3

for (i in 1:length(ds3)) {
    q1 <- unname(quantile(ds3[,i], probs = 0.02, na.rm = TRUE))
    q2 <- unname(quantile(ds3[,i], probs = 0.98, na.rm = TRUE))
    ds3.norm_2_98[,i]  <- normalise(ds3.norm_2_98[,i], q1, q2)  
}
# adjust values greater than 1 or less than 0
for (j in 1:length(ds3)) {
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] > 1 & !is.na(ds3.norm_2_98[i,j]))
      ds3.norm_2_98[i,j] = 1
  }
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] < 0 & !is.na(ds3.norm_2_98[i,j]))
      ds3.norm_2_98[i,j] = 0
  }
}

#######################################################
# Method:  Hybrid method using kmeans followed by hclust
# Alternative 1 Use knn to predict (see Alternative 2 below) 
#######################################################
library(MASS)
k1 <- i <- 17500 
k2 <- seq(5,100,5)
k3 <- 3
#ds3.norm_2_98noNA <- ds3.norm_2_98[complete.cases(ds3.norm_2_98), ]
paste(Sys.time(), " Starting kmeans clustering, centers ", i, sep = "")
set.seed(123)
kmeansObj <- kmeans(ds3.norm_2_98, centers = i, iter.max = 100)
kmeansCenters <- kmeansObj$centers
#################################
paste(Sys.time(), "Starting hclust")
# generate a dendrogram from the kmeanCenters
hybrid.fit.ward <- hclust(dist(kmeansCenters), "ward.D2")
paste(Sys.time(), "Starting cutree function")

clusters <- NULL
for (j in k2) {
  hybrid.clusters <- cutree(hybrid.fit.ward, k=j)
  # generate the test dataset
  hybrid.dataset <- cbind(hybrid.clusters, kmeansCenters)
  hybrid.dataset <- as.data.frame(hybrid.dataset)
  write.csv(hybrid.dataset, paste("hybrid_dataset_centers_", i, "_", j, 
                                  ".csv",sep=""), row.names = FALSE)
  train <- hybrid.dataset
  test <- ds3.norm_2_98
  # set up class labels
  cl <- factor(unname(hybrid.clusters))
  # perform linear discriminant analysis to perform unsupervised
  # prediction
  #################### qda method #############
  #library(MASS)
  #train.qda <- qda(train[,-1], cl)
  #library(stats)
  #paste(Sys.time(), "Starting predict k2 =", j, sep = " ")
  #pr <- predict(train.qda, test) # gives warnings due to ds3.norm_2_98 
  # containing NA but if ds3.norm_2_98noNA is used for test then the same
  # result but without a clue to the original rows is given.  So it is 
  # best to leave test as ds3.norm_2_98.
  #clusts <- as.integer(pr$class)
  #rm(pr)
  #clusters <- cbind(clusters, clusts)
  ################## end qda method
  ################# start knn method
  library(class)
  clusts <- knn(train[,-1], test, cl, k = k3, prob = F)
  clusters <- cbind(clusters, clusts)
  #row.names(clusters) <- row.names(test)
  ############### end knn method
}
# produce column names 

column.names <- NULL
for (i in i) {
  for (j in k2) {
    col.names <- paste("hybrid_k", i, "k", j,"k",k3, sep = "")
    column.names <- c(column.names,col.names)
  }
}
colnames(clusters) <- column.names
#value.ref <- which(!is.na(ds3$BackgroundNoise))
#clusters <- cbind(value.ref, clusters)

write.csv(clusters, file = paste("hybrid_clust_knn_",i,"_",k3,".csv",sep = ""),
          row.names = F)

###### Replace the NA values ####################
# do not use this it will take one hour, it is not necessary now that the 
# NA values have been replaced.
#cluster.lists <- data.frame(matrix(nrow = length(ds3.norm_2_98$BackgroundNoise), ncol = 10))
#paste(Sys.time(), " Starting replacement of NA values", i, sep = "")
#count <- 0
#for (j in value.ref) {
#    count <- count + 1
#    cluster.lists[j,1:10] <- clusters[count,1:10]
#}
#paste(Sys.time(), " End of replacement of NA values", i, sep = "")

#######################################################
# Method:  Hybrid method using kmeans followed by hclust
# Alternative 2 Use randomForest to predict and crashes when
# 20,000 clusters are attempted
#######################################################
#library(MASS)
#clusters.rf <- NULL
#ds3.norm_2_98noNA <- ds3.norm_2_98[complete.cases(ds3.norm_2_98), ]
#for (i in seq(25000, 25000, 5000)) {
#  paste(Sys.time(), " Starting kmeans clustering, centers ", i, sep = "")
#  set.seed(123)
#  kmeansObj <- kmeans(ds3.norm_2_98noNA, centers = i, iter.max = 100)
#  kmeansCenters <- kmeansObj$centers
#  paste(Sys.time(), "Saving kmeans centers")
#  ###### !!!!!!!!!!!!!!!!!! I need to keep a copy of the centers
#  paste(Sys.time(), "Starting hclust")
#  hybrid.fit.ward <- hclust(dist(kmeansCenters), "ward.D2")
#  paste(Sys.time(), "Starting cutree function")
#  for (j in seq(10, 40, 5)) {
#    hybrid.clusters <- cutree(hybrid.fit.ward, k=j)
#    # generate the test dataset
#    hybrid.dataset <- cbind(hybrid.clusters, kmeansCenters)
#    hybrid.dataset <- as.data.frame(hybrid.dataset)
#    train <- hybrid.dataset
#   table(hybrid.dataset$hybrid.clusters)
#    test <- ds3.norm_2_98
#    #test <- test[complete.cases(test), ]
#    # set up classes
#    cl <- factor(unname(hybrid.clusters))
#    # use randomForest to predict classes 
#    ###########################
#    ###### RANDOM FOREST
#    ###########################
#    paste(Sys.time(), "Starting randomForest")
#    library(randomForest)
#    # crashed at 25000 so randomForest is more memory intensive
#    # apply randomForest algorithm to training data
#    train.rf <- randomForest(train[,-1], cl, proximity = T)
#    train.rf
#    test.pred <- predict(train.rf, test)
#    clust.rf <- as.integer(unname(test.pred))
#    clusters.rf <- cbind(clusters.rf, clust.rf)
#  }
#}

# produce column names 
#column.names <- NULL
#k1 <- seq(25000,25000,5000)
#k2 <- seq(10, 40, 5)
#for (i in k1) {
#  for (j in k2) {
#    col.names <- paste("hybrid_k", i, "k", j,"_rf", sep = "")
#    column.names <- c(column.names,col.names)
#  }
#}
#colnames(clusters.rf) <- column.names

#write.csv(clusters.rf, file = "hybrid_clust_rf_25000.csv", row.names = F)