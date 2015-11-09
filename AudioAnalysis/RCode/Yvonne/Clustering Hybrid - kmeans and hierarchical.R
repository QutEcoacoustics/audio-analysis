#######################################################
# 2 November 2015
# This code performs the hybrid method on a long-duration dataset 
# (about four months at two sites, 112 days multiplied by 2)
#
# The four month dataset goes from 22 June 2015 to 11 Oct 2015 from two 
# sites GympieNP and Woondum3
#######################################################
setwd("C:\\Work\\CSV files\\FourMonths\\")

AcousticDS <- read.csv("final_dataset_22June2015_11 Oct2015.csv", header = T)

#ds3 <- AcousticDS[,c(2:18)]
ds3 <- AcousticDS[,c(3,4,7,10,11,15,16)] # without Mid-frequency cover
#ds3 <- AcousticDS[,c(3,4,7,9,10,11,15,16)] # with Mid-frequency cover
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_qda_1")

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
    ds3.norm_2_98[,i]  <- normalise(ds3.norm_2_98[,i], 
                                    q1, q2)  
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
# Alternative 1 Use qda to predict (see Alternative 2 below) 
#######################################################
library(MASS)
k1 <- i <- 34000 
k2 <- seq(10, 100, 10)
clusters <- NULL
ds3.norm_2_98noNA <- ds3.norm_2_98[complete.cases(ds3.norm_2_98), ]
paste(Sys.time(), " Starting kmeans clustering, centers ", i, sep = "")
set.seed(123)
kmeansObj <- kmeans(ds3.norm_2_98noNA, centers = i, iter.max = 100)
kmeansCenters <- kmeansObj$centers
paste(Sys.time(), "Starting hclust")
hybrid.fit.ward <- hclust(dist(kmeansCenters), "ward.D2")
paste(Sys.time(), "Starting cutree function")
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
  library(MASS)
  train.qda <- qda(train[,-1], cl)
  library(stats)
  paste(Sys.time(), "Starting predict k2 =", j, sep = " ")
  pr <- predict(train.qda, test) # gives warnings due to ds3.norm_2_98 
  # containing NA but if ds3.norm_2_98noNA is used for test then the same
  # result but without a clue to the original rows is given.  So it is 
  # best to leave test as ds3.norm_2_98.
  clusts <- as.integer(pr$class)
  rm(pr)
  clusters <- cbind(clusters, clusts)
}

# produce column names 
column.names <- NULL
for (i in i) {
  for (j in k2) {
    col.names <- paste("hybrid_k", i, "k", j, sep = "")
    column.names <- c(column.names,col.names)
  }
}
colnames(clusters) <- column.names

write.csv(clusters, file = paste("hybrid_clust_",i,".csv",sep = ""),
          row.names = F)

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