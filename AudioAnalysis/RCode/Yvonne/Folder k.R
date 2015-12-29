#The following code was run for this folder
#"C:\Work\CSV files\FourMonths\Hybrid_3_4_7_10_11_15_16_knn_k3k\"

#######################################################
# 2 November 2015
# This code performs the hybrid method on a long-duration dataset 
# (about four months at two sites, 111 days multiplied by 2)
#
# The four month dataset goes from 22 June 2015 to 11 Oct 2015 from two 
# sites GympieNP and Woondum3
# NOTE: Variables on line #75 can be changed
#######################################################
setwd("C:\\Work\\CSV files\\FourMonths\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hierarchical\\")

AcousticDS <- read.csv("final_dataset_22June2015_10 Oct2015.csv", 
                       header = T)
#ds3 <- AcousticDS[,c(2:18)]
ds3 <- AcousticDS[,c(3,4,7,10,11,15,16,25)] # without Mid-frequency cover
#ds3 <- AcousticDS[,c(3,4,7,9,10,11,15,16)] # with Mid-frequency cover
############## replace NA values
site1 <- ds3[1:(length(ds3$BackgroundNoise)/2),]
site2 <- ds3[((length(ds3$BackgroundNoise)/2)+1):(length(ds3$BackgroundNoise)),]
for(i in 1:(ncol(site1)-1)) {  # columns
  for(j in 1:nrow(site1)) {  # rows
    if (is.na(site1[j,i])) {
      average <- mean(c(site1[(j-15),i], site1[(j-12),i], site1[(j-10),i],
                        site1[(j+15),i], site1[(j+12),i], site1[(j+10),i]),
                      na.rm=TRUE)
      site1[j,i] <- average
    }
  }
}
for(i in 1:(ncol(site2)-1)) {  # columns
  for(j in 1:nrow(site2)) {  # rows
    if (is.na(site2[j,i])) {
      average <- mean(c(site2[(j-15),i], site2[(j-12),i], site2[(j-10),i],
                        site2[(j+15),i], site2[(j+12),i], site2[(j+10),i]),
                      na.rm=TRUE)
      site2[j,i] <- average
    }
  }
}
ds6 <- rbind(site1[,1:7], site2[,1:7])
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k")
#setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\kmeans")
#setwd("C:\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\hclust")

normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
#######################################################
# Create ds3.norm_2_98 for kmeans, clara, hclust
# a dataset normalised between 1.5 and 98.5%
#######################################################
ds3.norm_2_98 <- ds6
q1.values <- NULL
q2.values <- NULL
for (i in 1:length(ds6)) {
  q1 <- unname(quantile(ds6[,i], probs = 0.015, na.rm = TRUE))
  q2 <- unname(quantile(ds6[,i], probs = 0.985, na.rm = TRUE))
  q1.values <- c(q1.values, q1)
  q2.values <- c(q2.values, q2)
  ds3.norm_2_98[,i]  <- normalise(ds3.norm_2_98[,i], q1, q2)
}
# from zeros dataset
#q1.values <- c(-44.9683, 4.2939, 0.0000, 0.0191, 0.4107,0.0918, 0.00900)
#q2.values <- c(-28.77737, 29.72374, 2.94170, 0.25440, 0.55250, 0.99470, 
#              0.48310)
# from full dataset
#q1.values <- c(-44.9246, 4.3121, 0.0000, 0.0193, 0.4108, 0.0934, 0.0091)
#q2.values <- c(-28.77737, 29.72393,2.94170, 0.25440, 0.55250, 0.99470, 
#               0.48310)
# with probs = 0.019
# q1.values <- c(-44.9683, 4.2964, 0.0000, 0.0191, 0.4107, 0.0920, 0.0090)

# adjust values greater than 1 or less than 0
for (j in 1:length(ds6)) {
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] > 1 & !is.na(ds3.norm_2_98[i,j]))
      ds3.norm_2_98[i,j] = 1
  }
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] < 0 & !is.na(ds3.norm_2_98[i,j]))
      ds3.norm_2_98[i,j] = 0
  }
}

write.csv(ds3.norm_2_98,"ds3.norm_2_98.csv")
#########################
library(MASS)
########set-up the variables
k1 <- i <- 18500
k2 <- seq(5, 100, 5)
k3 <- 3
csv.name <- paste("hybrid_clust_knn_", i, "_3",sep="")

paste(Sys.time(), " Starting kmeans clustering, centers ", i, sep = "")
set.seed(123)
kmeansObj <- kmeans(ds3.norm_2_98, centers = i, iter.max = 100)
kmeansCenters <- kmeansObj$centers

paste(Sys.time(), "Starting hclust")
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

##### This is from the file names "Twelve_day_testing.R"
#dates2 <- rep(dates, each=6)
###################################################
# Use this to select out 12 days for experiment 2
##################################################
cluster.list.hybrid.exp2 <- read.csv(paste(csv.name, ".csv", sep = ""), 
                                     header = T)[c(54721:59040,100801:103680,106561:108000,
                                                   214561:218880,260641:263520,266401:267840),]

indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2a\\Final DataSet 30_31July_1Aug_31Aug_1_4Sept.csv",
                    header = T)
dates <- unique(indices$rec.date)
site <- c(rep("GympieNP",6), rep("WoondumNP",6))
dates2 <- rep(dates, 2) # this is for the 24 hour files

#############################################
# Use this for the whole 111 days by 2 sites (see above for exp2)
#############################################
#cluster.list.hybrid.exp2 <- read.csv(paste(csv.name, ".csv", sep = ""), 
#                            header = T)
#indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)
#dates <- unique(indices$rec.date)
#site <- c(rep("GympieNP",111*12), rep("WoondumNP",111*12)) # for the two hour files

#dates2 <- rep(dates, each=12, 2) # this is for the 2 hour files

# Adaptation to cluster all 111 days x 2 sites 
#cluster.list.hybrid.exp2 <- cluster.list.hybrid.exp2[complete.cases(cluster.list.hybrid.exp2), ]
#indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_11 Oct2015.csv", header=T)
#site <- c(rep("GympieNP",111), rep("WoondumNP",111))
# Setting NAs to 50 works

day.ref <- which(indices$minute.of.day=="0")
day.ref <- c(day.ref, (length(indices$minute.of.day)+1))
two.am.ref  <- which(indices$minute.of.day == "120")
four.am.ref <- which(indices$minute.of.day == "240")
six.am.ref  <- which(indices$minute.of.day == "360")
eight.am.ref <- which(indices$minute.of.day == "480")
ten.am.ref  <- which(indices$minute.of.day == "600")
midday.ref  <- which(indices$minute.of.day == "720")
two.pm.ref  <- which(indices$minute.of.day == "840")
four.pm.ref <- which(indices$minute.of.day == "960")
six.pm.ref  <- which(indices$minute.of.day == "1080")
eight.pm.ref <- which(indices$minute.of.day == "1200")
ten.pm.ref <- which(indices$minute.of.day == "1320")
two.hour.ref <- c(day.ref, two.am.ref,four.am.ref,six.am.ref,eight.am.ref,ten.am.ref,
                  midday.ref,two.pm.ref,four.pm.ref,six.pm.ref,eight.pm.ref,ten.pm.ref)
two.hour.ref <- sort(two.hour.ref)

four.hour.ref <- c(day.ref, four.am.ref, eight.am.ref, midday.ref,
                   four.pm.ref, eight.pm.ref)
four.hour.ref <- sort(four.hour.ref)
four.hour.ref <- c(four.hour.ref, (length(indices$minute.of.day)+1))

six.am.ref   <- which(indices$minute.of.day == "360")
six.pm.ref   <- which(indices$minute.of.day == "1080")
six.hour.ref <- c(day.ref, six.am.ref, midday.ref, six.pm.ref)
six.hour.ref <- sort(six.hour.ref)
six.hour.ref <- c(six.hour.ref, (length(indices$minute.of.day)+1))

length.ref   <- length(indices$X)

####################################################
# Saving the Hybrid 24 hour files
####################################################
twentyfour_hour_table_1 <- NULL 
twentyfour_hour_table_2 <- NULL 
twentyfour_hour_table_3 <- NULL 
twentyfour_hour_table_4 <- NULL 
twentyfour_hour_table_5 <- NULL 
twentyfour_hour_table_6 <- NULL 
twentyfour_hour_table_7 <- NULL 
twentyfour_hour_table_8 <- NULL 
twentyfour_hour_table_9 <- NULL 
twentyfour_hour_table_10 <- NULL 
twentyfour_hour_table_11 <- NULL 
twentyfour_hour_table_12 <- NULL 
twentyfour_hour_table_13 <- NULL 
twentyfour_hour_table_14 <- NULL 
twentyfour_hour_table_15 <- NULL 
twentyfour_hour_table_16 <- NULL 
twentyfour_hour_table_17 <- NULL 
twentyfour_hour_table_18 <- NULL 
twentyfour_hour_table_19 <- NULL 
twentyfour_hour_table_20 <- NULL 

cluster.list <- cluster.list.hybrid.exp2
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(day.ref)-1)) {
    cluster.ref <- hist(cluster.list[day.ref[j]:(day.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    print(length(cluster.ref$counts))
    
    if (i == 1)  {twentyfour_hour_table_1  <- rbind(twentyfour_hour_table_1, cluster.ref$counts)}
    if (i == 2)  {twentyfour_hour_table_2  <- rbind(twentyfour_hour_table_2, cluster.ref$counts)}
    if (i == 3)  {twentyfour_hour_table_3  <- rbind(twentyfour_hour_table_3, cluster.ref$counts)}  
    if (i == 4)  {twentyfour_hour_table_4  <- rbind(twentyfour_hour_table_4, cluster.ref$counts)}
    if (i == 5)  {twentyfour_hour_table_5  <- rbind(twentyfour_hour_table_5, cluster.ref$counts)}
    if (i == 6)  {twentyfour_hour_table_6  <- rbind(twentyfour_hour_table_6, cluster.ref$counts)}  
    if (i == 7)  {twentyfour_hour_table_7  <- rbind(twentyfour_hour_table_7, cluster.ref$counts)}
    if (i == 8)  {twentyfour_hour_table_8  <- rbind(twentyfour_hour_table_8, cluster.ref$counts)}
    if (i == 9)  {twentyfour_hour_table_9  <- rbind(twentyfour_hour_table_9, cluster.ref$counts)}
    if (i == 10) {twentyfour_hour_table_10 <- rbind(twentyfour_hour_table_10, cluster.ref$counts)}
    if (i == 11)  {twentyfour_hour_table_11  <- rbind(twentyfour_hour_table_11, cluster.ref$counts)}
    if (i == 12)  {twentyfour_hour_table_12  <- rbind(twentyfour_hour_table_12, cluster.ref$counts)}
    if (i == 13)  {twentyfour_hour_table_13  <- rbind(twentyfour_hour_table_13, cluster.ref$counts)}  
    if (i == 14)  {twentyfour_hour_table_14  <- rbind(twentyfour_hour_table_14, cluster.ref$counts)}
    if (i == 15)  {twentyfour_hour_table_15  <- rbind(twentyfour_hour_table_15, cluster.ref$counts)}
    if (i == 16)  {twentyfour_hour_table_16  <- rbind(twentyfour_hour_table_16, cluster.ref$counts)}  
    if (i == 17)  {twentyfour_hour_table_17  <- rbind(twentyfour_hour_table_17, cluster.ref$counts)}
    if (i == 18)  {twentyfour_hour_table_18  <- rbind(twentyfour_hour_table_18, cluster.ref$counts)}
    if (i == 19)  {twentyfour_hour_table_19  <- rbind(twentyfour_hour_table_19, cluster.ref$counts)}
    if (i == 20) {twentyfour_hour_table_20 <- rbind(twentyfour_hour_table_20, cluster.ref$counts)}
  }
}

twentyfour_hour_table_1 <- as.data.frame(twentyfour_hour_table_1)
twentyfour_hour_table_2 <- as.data.frame(twentyfour_hour_table_2)
twentyfour_hour_table_3 <- as.data.frame(twentyfour_hour_table_3)
twentyfour_hour_table_4 <- as.data.frame(twentyfour_hour_table_4)
twentyfour_hour_table_5 <- as.data.frame(twentyfour_hour_table_5)
twentyfour_hour_table_6 <- as.data.frame(twentyfour_hour_table_6)
twentyfour_hour_table_7 <- as.data.frame(twentyfour_hour_table_7)
twentyfour_hour_table_8 <- as.data.frame(twentyfour_hour_table_8)
twentyfour_hour_table_9 <- as.data.frame(twentyfour_hour_table_9)
twentyfour_hour_table_10 <- as.data.frame(twentyfour_hour_table_10)
twentyfour_hour_table_11 <- as.data.frame(twentyfour_hour_table_11)
twentyfour_hour_table_12 <- as.data.frame(twentyfour_hour_table_12)
twentyfour_hour_table_13 <- as.data.frame(twentyfour_hour_table_13)
twentyfour_hour_table_14 <- as.data.frame(twentyfour_hour_table_14)
twentyfour_hour_table_15 <- as.data.frame(twentyfour_hour_table_15)
twentyfour_hour_table_16 <- as.data.frame(twentyfour_hour_table_16)
twentyfour_hour_table_17 <- as.data.frame(twentyfour_hour_table_17)
twentyfour_hour_table_18 <- as.data.frame(twentyfour_hour_table_18)
twentyfour_hour_table_19 <- as.data.frame(twentyfour_hour_table_19)
twentyfour_hour_table_20 <- as.data.frame(twentyfour_hour_table_20)

# Rename the columns
column.names <- NULL
if (i==1) {
  for (k in 1:(length(twentyfour_hour_table_1))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_1) <- column.names
}
if (i==2) {
  for (k in 1:(length(twentyfour_hour_table_2))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_2) <- column.names
}
if (i==3) {
  for (k in 1:(length(twentyfour_hour_table_3))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_3) <- column.names
}

if (i==4) {
  for (k in 1:(length(twentyfour_hour_table_4))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_4) <- column.names
}
if (i==5) {
  for (k in 1:(length(twentyfour_hour_table_5))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_5) <- column.names
}
if (i==6) {
  for (k in 1:(length(twentyfour_hour_table_6))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_6) <- column.names
}
if (i==7) {
  for (k in 1:(length(twentyfour_hour_table_7))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_7) <- column.names
}
if (i==8) {
  for (k in 1:(length(twentyfour_hour_table_8))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_8) <- column.names
}
if (i==9) {
  for (k in 1:(length(twentyfour_hour_table_9))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_9) <- column.names
}
if (i==10) {
  for (k in 1:(length(twentyfour_hour_table_10))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_10) <- column.names
}
if (i==11) {
  for (k in 1:(length(twentyfour_hour_table_11))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_11) <- column.names
}
if (i==12) {
  for (k in 1:(length(twentyfour_hour_table_12))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_12) <- column.names
}
if (i==13) {
  for (k in 1:(length(twentyfour_hour_table_13))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_13) <- column.names
}

if (i==14) {
  for (k in 1:(length(twentyfour_hour_table_14))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_14) <- column.names
}
if (i==15) {
  for (k in 1:(length(twentyfour_hour_table_15))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_15) <- column.names
}
if (i==16) {
  for (k in 1:(length(twentyfour_hour_table_16))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_16) <- column.names
}
if (i==17) {
  for (k in 1:(length(twentyfour_hour_table_17))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_17) <- column.names
}
if (i==18) {
  for (k in 1:(length(twentyfour_hour_table_18))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_18) <- column.names
}
if (i==19) {
  for (k in 1:(length(twentyfour_hour_table_19))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_19) <- column.names
}
if (i==20) {
  for (k in 1:(length(twentyfour_hour_table_20))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_20) <- column.names
}

twentyfour_hour_table_1 <- cbind(twentyfour_hour_table_1,site,as.character(dates2))
twentyfour_hour_table_2 <- cbind(twentyfour_hour_table_2,site,as.character(dates2))
twentyfour_hour_table_3 <- cbind(twentyfour_hour_table_3,site,as.character(dates2))
twentyfour_hour_table_4 <- cbind(twentyfour_hour_table_4,site,as.character(dates2))
twentyfour_hour_table_5 <- cbind(twentyfour_hour_table_5,site,as.character(dates2))
twentyfour_hour_table_6 <- cbind(twentyfour_hour_table_6,site,as.character(dates2))
twentyfour_hour_table_7 <- cbind(twentyfour_hour_table_7,site,as.character(dates2))
twentyfour_hour_table_8 <- cbind(twentyfour_hour_table_8,site,as.character(dates2))
twentyfour_hour_table_9 <- cbind(twentyfour_hour_table_9,site,as.character(dates2))
twentyfour_hour_table_10 <- cbind(twentyfour_hour_table_10,site,as.character(dates2))
twentyfour_hour_table_11 <- cbind(twentyfour_hour_table_11,site,as.character(dates2))
twentyfour_hour_table_12 <- cbind(twentyfour_hour_table_12,site,as.character(dates2))
twentyfour_hour_table_13 <- cbind(twentyfour_hour_table_13,site,as.character(dates2))
twentyfour_hour_table_14 <- cbind(twentyfour_hour_table_14,site,as.character(dates2))
twentyfour_hour_table_15 <- cbind(twentyfour_hour_table_15,site,as.character(dates2))
twentyfour_hour_table_16 <- cbind(twentyfour_hour_table_16,site,as.character(dates2))
twentyfour_hour_table_17 <- cbind(twentyfour_hour_table_17,site,as.character(dates2))
twentyfour_hour_table_18 <- cbind(twentyfour_hour_table_18,site,as.character(dates2))
twentyfour_hour_table_19 <- cbind(twentyfour_hour_table_19,site,as.character(dates2))
twentyfour_hour_table_20 <- cbind(twentyfour_hour_table_20,site,as.character(dates2))

write.csv(twentyfour_hour_table_1, paste(csv.name, "_k5_24hour.csv", sep = ""), 
          row.names = F)
write.csv(twentyfour_hour_table_2, paste(csv.name, "_k10_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_3, paste(csv.name, "_k15_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_4, paste(csv.name, "_k20_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_5, paste(csv.name, "_k25_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_6, paste(csv.name, "_k30_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_7, paste(csv.name, "_k35_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_8, paste(csv.name, "_k40_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_9, paste(csv.name, "_k45_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_10, paste(csv.name, "_k50_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_11, paste(csv.name, "_k55_24hour.csv", sep = ""), 
          row.names = F)
write.csv(twentyfour_hour_table_12, paste(csv.name, "_k60_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_13, paste(csv.name, "_k65_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_14, paste(csv.name, "_k70_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_15, paste(csv.name, "_k75_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_16, paste(csv.name, "_k80_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_17, paste(csv.name, "_k85_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_18, paste(csv.name, "_k90_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_19, paste(csv.name, "_k95_24hour.csv", sep = ""),
          row.names = F)
write.csv(twentyfour_hour_table_20, paste(csv.name, "_k100_24hour.csv", sep = ""),
          row.names = F)

# This is from file named "Clustering_of_24_hour_files.R"
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Mclust\\")
myFiles <- list.files(full.names=TRUE, pattern="_6hour.csv$")
myFilesShort <- list.files(full.names=FALSE, pattern="*_6hour.csv$")

length <- length(myFiles)
length
site <- c(rep("GympieNP",6), rep("WoondumNP",6))
dates <- unique(indices$rec.date)
#dates2 <- rep(dates, each=6)
dates <- rep(dates, 2)
# Read file contents of Summary Indices and collate
numberCol <- NULL
heights <- NULL
for (i in 1:length(myFilesShort)) {
  Name <- myFiles[i]
  assign("fileContents", read.csv(Name))
  numberCol <- ncol(fileContents)
  numberCol <- numberCol-2
  dat <- fileContents[,1:numberCol]
  #c <- cor(dat)
  hc.fit <- hclust(dist(dat), method = "ward.D2")
  #saving row1
  if (hc.fit$merge[1]<0 & hc.fit$merge[12]<0) {
    row1 <- c(abs(hc.fit$merge[1]),hc.fit$height[1],abs(hc.fit$merge[12]))
  }
  #saving row2
  if (hc.fit$merge[2]<0 & hc.fit$merge[13]<0) {
    row2 <- c(abs(hc.fit$merge[2]),hc.fit$height[2],abs(hc.fit$merge[13]))
  }
  if (hc.fit$merge[2]<0 & hc.fit$merge[13]==1) {
    row2 <- c(abs(hc.fit$merge[2]),hc.fit$height[2],row1)
  }
  #saving row3
  if (hc.fit$merge[3]<0 & hc.fit$merge[14]<0) {
    row3 <- c(abs(hc.fit$merge[3]),hc.fit$height[3],abs(hc.fit$merge[14]))
  }
  if (hc.fit$merge[3]<0 & hc.fit$merge[14]==1) {
    row3 <- c(abs(hc.fit$merge[3]),hc.fit$height[3],row1)
  }
  if (hc.fit$merge[3]<0 & hc.fit$merge[14]==2) {
    row3 <- c(abs(hc.fit$merge[3]),hc.fit$height[3],row2)
  }
  if (hc.fit$merge[3]==1 & hc.fit$merge[14]==2) {
    row3 <- c(row1,hc.fit$height[3],row2)
  }
  #saving row4
  #negative-negative
  if (hc.fit$merge[4]<0 & hc.fit$merge[15]<0) {
    row4 <- c(abs(hc.fit$merge[4]),hc.fit$height[4],abs(hc.fit$merge[15]))
  }
  #negative-positive
  if (hc.fit$merge[4]<0 & hc.fit$merge[15]==1) {
    row4 <- c(abs(hc.fit$merge[4]),hc.fit$height[4],row1)
  }
  if (hc.fit$merge[4]<0 & hc.fit$merge[15]==2) {
    row4 <- c(abs(hc.fit$merge[4]),hc.fit$height[4],row2)
  }
  if (hc.fit$merge[4]<0 & hc.fit$merge[15]==3) {
    row4 <- c(abs(hc.fit$merge[4]),hc.fit$height[4],row3)
  }
  #positive-positive
  if (hc.fit$merge[4]==1 & hc.fit$merge[15]==2) {
    row4 <- c(row1,hc.fit$height[4],row2)
  }
  if (hc.fit$merge[4]==1 & hc.fit$merge[15]==3) {
    row4 <- c(row1,hc.fit$height[4],row3)
  }
  if (hc.fit$merge[4]==2 & hc.fit$merge[15]==3) {
    row4 <- c(row2,hc.fit$height[4],row3)
  }
  #saving row5
  # negative-negative
  if (hc.fit$merge[5]<0 & hc.fit$merge[16]<0) {
    row5 <- c(abs(hc.fit$merge[5]),hc.fit$height[5],abs(hc.fit$merge[16]))
  }
  # negative-positive
  if (hc.fit$merge[5]<0 & hc.fit$merge[16]==1) {
    row5 <- c(abs(hc.fit$merge[5]),hc.fit$height[5],row1)
  }
  if (hc.fit$merge[5]<0 & hc.fit$merge[16]==2) {
    row5 <- c(abs(hc.fit$merge[5]),hc.fit$height[5],row2)
  }
  if (hc.fit$merge[5]<0 & hc.fit$merge[16]==3) {
    row5 <- c(abs(hc.fit$merge[5]),hc.fit$height[5],row3)
  }
  if (hc.fit$merge[5]<0 & hc.fit$merge[16]==4) {
    row5 <- c(abs(hc.fit$merge[5]),hc.fit$height[5],row4)
  }
  #positive-positive
  if (hc.fit$merge[5]==1 & hc.fit$merge[16]==2) {
    row5 <- c(row1,hc.fit$height[5],row2)
  }
  if (hc.fit$merge[5]==1 & hc.fit$merge[16]==3) {
    row5 <- c(row1,hc.fit$height[5],row3)
  }
  if (hc.fit$merge[5]==1 & hc.fit$merge[16]==4) {
    row5 <- c(row1,hc.fit$height[5],row4)
  }
  if (hc.fit$merge[5]==2 & hc.fit$merge[16]==3) {
    row5 <- c(row2,hc.fit$height[5],row3)
  }
  if (hc.fit$merge[5]==2 & hc.fit$merge[16]==4) {
    row5 <- c(row2,hc.fit$height[5],row4)
  }
  if (hc.fit$merge[5]==3 & hc.fit$merge[16]==4) {
    row5 <- c(row3,hc.fit$height[5],row4)
  }
  #saving row6
  #negative-negative
  if (hc.fit$merge[6]<0 & hc.fit$merge[17]<0) {
    row6 <- c(abs(hc.fit$merge[6]),hc.fit$height[6],abs(hc.fit$merge[17]))
  }
  #negative-positive
  if (hc.fit$merge[6]<0 & hc.fit$merge[17]==1) {
    row6 <- c(abs(hc.fit$merge[6]),hc.fit$height[6],row1)
  }
  if (hc.fit$merge[6]<0 & hc.fit$merge[17]==2) {
    row6 <- c(abs(hc.fit$merge[6]),hc.fit$height[6],row2)
  }
  if (hc.fit$merge[6] <0 & hc.fit$merge[17]==3) {
    row6 <- c(abs(hc.fit$merge[6]),hc.fit$height[6],row3)
  }
  if (hc.fit$merge[6]<0 & hc.fit$merge[17]==4) {
    row6 <- c(abs(hc.fit$merge[6]),hc.fit$height[6],row4)
  }
  if (hc.fit$merge[6]<0 & hc.fit$merge[17]==5) {
    row6 <- c(abs(hc.fit$merge[6]),hc.fit$height[6],row5)
  }
  #positive-positive
  if (hc.fit$merge[6]==1 & hc.fit$merge[17]==2) {
    row6 <- c(row1,hc.fit$height[6],row2)
  }
  if (hc.fit$merge[6]==1 & hc.fit$merge[17]==3) {
    row6 <- c(row1,hc.fit$height[6],row3)
  }
  if (hc.fit$merge[6]==1 & hc.fit$merge[17]==4) {
    row6 <- c(row1,hc.fit$height[6],row4)
  }
  if (hc.fit$merge[6]==1 & hc.fit$merge[17]==5) {
    row6 <- c(row1,hc.fit$height[6],row5)
  }
  if (hc.fit$merge[6]==2 & hc.fit$merge[17]==3) {
    row6 <- c(row2,hc.fit$height[6],row3)
  }
  if (hc.fit$merge[6]==2 & hc.fit$merge[17]==4) {
    row6 <- c(row2,hc.fit$height[6],row4)
  }
  if (hc.fit$merge[6]==2 & hc.fit$merge[17]==5) {
    row6 <- c(row2,hc.fit$height[6],row5)
  }
  if (hc.fit$merge[6]==3 & hc.fit$merge[17]==4) {
    row6 <- c(row3,hc.fit$height[6],row4)
  }
  if (hc.fit$merge[6]==3 & hc.fit$merge[17]==5) {
    row6 <- c(row3,hc.fit$height[6],row5)
  }
  if (hc.fit$merge[6]==4 & hc.fit$merge[17]==5) {
    row6 <- c(row4,hc.fit$height[6],row5)
  }
  #saving row7
  #negative-negative
  if (hc.fit$merge[7]<0 & hc.fit$merge[18]<0) {
    row7 <- c(abs(hc.fit$merge[7]),hc.fit$height[7],abs(hc.fit$merge[18]))
  }
  #negative-positive
  if (hc.fit$merge[7]<0 & hc.fit$merge[18]==1) {
    row7 <- c(abs(hc.fit$merge[7]),hc.fit$height[7],row1)
  }
  if (hc.fit$merge[7]<0 & hc.fit$merge[18]==2) {
    row7 <- c(abs(hc.fit$merge[7]),hc.fit$height[7],row2)
  }
  if (hc.fit$merge[7] <0 & hc.fit$merge[18]==3) {
    row7 <- c(abs(hc.fit$merge[7]),hc.fit$height[7],row3)
  }
  if (hc.fit$merge[7]<0 & hc.fit$merge[18]==4) {
    row7 <- c(abs(hc.fit$merge[7]),hc.fit$height[7],row4)
  }
  if (hc.fit$merge[7]<0 & hc.fit$merge[18]==5) {
    row7 <- c(abs(hc.fit$merge[7]),hc.fit$height[7],row5)
  }
  if (hc.fit$merge[7]<0 & hc.fit$merge[18]==6) {
    row7 <- c(abs(hc.fit$merge[7]),hc.fit$height[7],row6)
  }
  #postive-positive
  if (hc.fit$merge[7]==1 & hc.fit$merge[18]==2) {
    row7 <- c(row1,hc.fit$height[7],row2)
  }
  if (hc.fit$merge[7]==1 & hc.fit$merge[18]==3) {
    row7 <- c(row1,hc.fit$height[7],row3)
  }
  if (hc.fit$merge[7]==1 & hc.fit$merge[18]==4) {
    row7 <- c(row1,hc.fit$height[7],row4)
  }
  if (hc.fit$merge[7]==1 & hc.fit$merge[18]==5) {
    row7 <- c(row1,hc.fit$height[7],row5)
  }
  if (hc.fit$merge[7]==1 & hc.fit$merge[18]==6) {
    row7 <- c(row1,hc.fit$height[7],row6)
  }
  if (hc.fit$merge[7]==2 & hc.fit$merge[18]==3) {
    row7 <- c(row2,hc.fit$height[7],row3)
  }
  if (hc.fit$merge[7]==2 & hc.fit$merge[18]==4) {
    row7 <- c(row2,hc.fit$height[7],row4)
  }
  if (hc.fit$merge[7]==2 & hc.fit$merge[18]==5) {
    row7 <- c(row2,hc.fit$height[7],row5)
  }
  if (hc.fit$merge[7]==2 & hc.fit$merge[18]==6) {
    row7 <- c(row2,hc.fit$height[7],row6)
  }
  if (hc.fit$merge[7]==3 & hc.fit$merge[18]==4) {
    row7 <- c(row3,hc.fit$height[7],row4)
  }
  if (hc.fit$merge[7]==3 & hc.fit$merge[18]==5) {
    row7 <- c(row3,hc.fit$height[7],row5)
  }
  if (hc.fit$merge[7]==3 & hc.fit$merge[18]==6) {
    row7 <- c(row3,hc.fit$height[7],row6)
  }
  if (hc.fit$merge[7]==4 & hc.fit$merge[18]==5) {
    row7 <- c(row4,hc.fit$height[7],row5)
  }
  if (hc.fit$merge[7]==4 & hc.fit$merge[18]==6) {
    row7 <- c(row4,hc.fit$height[7],row6)
  }
  if (hc.fit$merge[7]==5 & hc.fit$merge[18]==6) {
    row7 <- c(row5,hc.fit$height[7],row6)
  }
  #saving row8
  #negative-negative
  if (hc.fit$merge[8]<0 & hc.fit$merge[19]<0) {
    row8 <- c(abs(hc.fit$merge[8]),hc.fit$height[8],abs(hc.fit$merge[19]))
  }
  #negative-positive
  if (hc.fit$merge[8]<0 & hc.fit$merge[19]==1) {
    row8 <- c(abs(hc.fit$merge[8]),hc.fit$height[8],row1)
  }
  if (hc.fit$merge[8]<0 & hc.fit$merge[19]==2) {
    row8 <- c(abs(hc.fit$merge[8]),hc.fit$height[8],row2)
  }
  if (hc.fit$merge[8] <0 & hc.fit$merge[19]==3) {
    row8 <- c(abs(hc.fit$merge[8]),hc.fit$height[8],row3)
  }
  if (hc.fit$merge[8]<0 & hc.fit$merge[19]==4) {
    row8 <- c(abs(hc.fit$merge[8]),hc.fit$height[8],row4)
  }
  if (hc.fit$merge[8]<0 & hc.fit$merge[19]==5) {
    row8 <- c(abs(hc.fit$merge[8]),hc.fit$height[8],row5)
  }
  if (hc.fit$merge[8]<0 & hc.fit$merge[19]==6) {
    row8 <- c(abs(hc.fit$merge[8]),hc.fit$height[8],row6)
  }
  if (hc.fit$merge[8]<0 & hc.fit$merge[19]==7) {
    row8 <- c(abs(hc.fit$merge[8]),hc.fit$height[8],row7)
  }
  #postive-positive
  if (hc.fit$merge[8]==1 & hc.fit$merge[19]==2) {
    row8 <- c(row1,hc.fit$height[8],row2)
  }
  if (hc.fit$merge[8]==1 & hc.fit$merge[19]==3) {
    row8 <- c(row1,hc.fit$height[8],row3)
  }
  if (hc.fit$merge[8]==1 & hc.fit$merge[19]==4) {
    row8 <- c(row1,hc.fit$height[8],row4)
  }
  if (hc.fit$merge[8]==1 & hc.fit$merge[19]==5) {
    row8 <- c(row1,hc.fit$height[8],row5)
  }
  if (hc.fit$merge[8]==1 & hc.fit$merge[19]==6) {
    row8 <- c(row1,hc.fit$height[8],row6)
  }
  if (hc.fit$merge[8]==1 & hc.fit$merge[19]==7) {
    row8 <- c(row1,hc.fit$height[8],row7)
  }
  if (hc.fit$merge[8]==2 & hc.fit$merge[19]==3) {
    row8 <- c(row2,hc.fit$height[8],row3)
  }
  if (hc.fit$merge[8]==2 & hc.fit$merge[19]==4) {
    row8 <- c(row2,hc.fit$height[8],row4)
  }
  if (hc.fit$merge[8]==2 & hc.fit$merge[19]==5) {
    row8 <- c(row2,hc.fit$height[8],row5)
  }
  if (hc.fit$merge[8]==2 & hc.fit$merge[19]==6) {
    row8 <- c(row2,hc.fit$height[8],row6)
  }
  if (hc.fit$merge[8]==2 & hc.fit$merge[19]==7) {
    row8 <- c(row2,hc.fit$height[8],row7)
  }
  if (hc.fit$merge[8]==3 & hc.fit$merge[19]==4) {
    row8 <- c(row3,hc.fit$height[8],row4)
  }
  if (hc.fit$merge[8]==3 & hc.fit$merge[19]==5) {
    row8 <- c(row3,hc.fit$height[8],row5)
  }
  if (hc.fit$merge[8]==3 & hc.fit$merge[19]==6) {
    row8 <- c(row3,hc.fit$height[8],row6)
  }
  if (hc.fit$merge[8]==3 & hc.fit$merge[19]==7) {
    row8 <- c(row3,hc.fit$height[8],row7)
  }
  if (hc.fit$merge[8]==4 & hc.fit$merge[19]==5) {
    row8 <- c(row4,hc.fit$height[8],row5)
  }
  if (hc.fit$merge[8]==4 & hc.fit$merge[19]==6) {
    row8 <- c(row4,hc.fit$height[8],row6)
  }
  if (hc.fit$merge[8]==4 & hc.fit$merge[19]==7) {
    row8 <- c(row4,hc.fit$height[8],row7)
  }
  if (hc.fit$merge[8]==5 & hc.fit$merge[19]==6) {
    row8 <- c(row5,hc.fit$height[8],row6)
  }
  if (hc.fit$merge[8]==5 & hc.fit$merge[19]==7) {
    row8 <- c(row5,hc.fit$height[8],row7)
  }
  if (hc.fit$merge[8]==6 & hc.fit$merge[19]==7) {
    row8 <- c(row6,hc.fit$height[8],row7)
  }
  #saving row9
  #negative-negative
  if (hc.fit$merge[9]<0 & hc.fit$merge[20]<0) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],abs(hc.fit$merge[20]))
  }
  #negative-positive
  if (hc.fit$merge[9]<0 & hc.fit$merge[20]==1) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],row1)
  }
  if (hc.fit$merge[9]<0 & hc.fit$merge[20]==2) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],row2)
  }
  if (hc.fit$merge[9] <0 & hc.fit$merge[20]==3) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],row3)
  }
  if (hc.fit$merge[9]<0 & hc.fit$merge[20]==4) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],row4)
  }
  if (hc.fit$merge[9]<0 & hc.fit$merge[20]==5) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],row5)
  }
  if (hc.fit$merge[9]<0 & hc.fit$merge[20]==6) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],row6)
  }
  if (hc.fit$merge[9]<0 & hc.fit$merge[20]==7) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],row7)
  }
  if (hc.fit$merge[9]<0 & hc.fit$merge[20]==8) {
    row9 <- c(abs(hc.fit$merge[9]),hc.fit$height[9],row8)
  }
  #positive-positive
  if (hc.fit$merge[9]==1 & hc.fit$merge[20]==2) {
    row9 <- c(row1,hc.fit$height[9],row2)
  }
  if (hc.fit$merge[9]==1 & hc.fit$merge[20]==3) {
    row9 <- c(row1,hc.fit$height[9],row3)
  }
  if (hc.fit$merge[9]==1 & hc.fit$merge[20]==4) {
    row9 <- c(row1,hc.fit$height[9],row4)
  }
  if (hc.fit$merge[9]==1 & hc.fit$merge[20]==5) {
    row9 <- c(row1,hc.fit$height[9],row5)
  }
  if (hc.fit$merge[9]==1 & hc.fit$merge[20]==6) {
    row9 <- c(row1,hc.fit$height[9],row6)
  }
  if (hc.fit$merge[9]==1 & hc.fit$merge[20]==7) {
    row9 <- c(row1,hc.fit$height[9],row7)
  }
  if (hc.fit$merge[9]==1 & hc.fit$merge[20]==8) {
    row9 <- c(row1,hc.fit$height[9],row8)
  }
  if (hc.fit$merge[9]==2 & hc.fit$merge[20]==3) {
    row9 <- c(row2,hc.fit$height[9],row3)
  }
  if (hc.fit$merge[9]==2 & hc.fit$merge[20]==4) {
    row9 <- c(row2,hc.fit$height[9],row4)
  }
  if (hc.fit$merge[9]==2 & hc.fit$merge[20]==5) {
    row9 <- c(row2,hc.fit$height[9],row5)
  }
  if (hc.fit$merge[9]==2 & hc.fit$merge[20]==6) {
    row9 <- c(row2,hc.fit$height[9],row6)
  }
  if (hc.fit$merge[9]==2 & hc.fit$merge[20]==7) {
    row9 <- c(row2,hc.fit$height[9],row7)
  }
  if (hc.fit$merge[9]==2 & hc.fit$merge[20]==8) {
    row9 <- c(row2,hc.fit$height[9],row8)
  }
  if (hc.fit$merge[9]==3 & hc.fit$merge[20]==4) {
    row9 <- c(row3,hc.fit$height[9],row4)
  }
  if (hc.fit$merge[9]==3 & hc.fit$merge[20]==5) {
    row9 <- c(row3,hc.fit$height[9],row5)
  }
  if (hc.fit$merge[9]==3 & hc.fit$merge[20]==6) {
    row9 <- c(row3,hc.fit$height[9],row6)
  }
  if (hc.fit$merge[9]==3 & hc.fit$merge[20]==7) {
    row9 <- c(row3,hc.fit$height[9],row7)
  }
  if (hc.fit$merge[9]==3 & hc.fit$merge[20]==8) {
    row9 <- c(row3,hc.fit$height[9],row8)
  }
  if (hc.fit$merge[9]==4 & hc.fit$merge[20]==5) {
    row9 <- c(row4,hc.fit$height[9],row5)
  }
  if (hc.fit$merge[9]==4 & hc.fit$merge[20]==6) {
    row9 <- c(row4,hc.fit$height[9],row6)
  }
  if (hc.fit$merge[9]==4 & hc.fit$merge[20]==7) {
    row9 <- c(row4,hc.fit$height[9],row7)
  }
  if (hc.fit$merge[9]==4 & hc.fit$merge[20]==8) {
    row9 <- c(row4,hc.fit$height[9],row8)
  }
  if (hc.fit$merge[9]==5 & hc.fit$merge[20]==6) {
    row9 <- c(row5,hc.fit$height[9],row6)
  }
  if (hc.fit$merge[9]==5 & hc.fit$merge[20]==7) {
    row9 <- c(row5,hc.fit$height[9],row7)
  }
  if (hc.fit$merge[9]==5 & hc.fit$merge[20]==8) {
    row9 <- c(row5,hc.fit$height[9],row8)
  }
  if (hc.fit$merge[9]==6 & hc.fit$merge[20]==7) {
    row9 <- c(row6,hc.fit$height[9],row7)
  }
  if (hc.fit$merge[9]==6 & hc.fit$merge[20]==8) {
    row9 <- c(row6,hc.fit$height[9],row8)
  }
  if (hc.fit$merge[9]==7 & hc.fit$merge[20]==8) {
    row9 <- c(row7,hc.fit$height[9],row8)
  }
  #saving row10
  #negative-negative
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]<0) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],abs(hc.fit$merge[21]))
  }
  #negative-postive
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]==1) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row1)
  }
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]==2) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row2)
  }
  if (hc.fit$merge[10] <0 & hc.fit$merge[21]==3) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row3)
  }
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]==4) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row4)
  }
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]==5) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row5)
  }
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]==6) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row6)
  }
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]==7) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row7)
  }
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]==8) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row8)
  }
  if (hc.fit$merge[10]<0 & hc.fit$merge[21]==9) {
    row10 <- c(abs(hc.fit$merge[10]),hc.fit$height[10],row9)
  }
  #positive-positive
  if (hc.fit$merge[10]==1 & hc.fit$merge[21]==2) {
    row10 <- c(row1,hc.fit$height[10],row2)
  }
  if (hc.fit$merge[10]==1 & hc.fit$merge[21]==3) {
    row10 <- c(row1,hc.fit$height[10],row3)
  }
  if (hc.fit$merge[10]==1 & hc.fit$merge[21]==4) {
    row10 <- c(row1,hc.fit$height[10],row4)
  }
  if (hc.fit$merge[10]==1 & hc.fit$merge[21]==5) {
    row10 <- c(row1,hc.fit$height[10],row5)
  }
  if (hc.fit$merge[10]==1 & hc.fit$merge[21]==6) {
    row10 <- c(row1,hc.fit$height[10],row6)
  }
  if (hc.fit$merge[10]==1 & hc.fit$merge[21]==7) {
    row10 <- c(row1,hc.fit$height[10],row7)
  }
  if (hc.fit$merge[10]==1 & hc.fit$merge[21]==8) {
    row10 <- c(row1,hc.fit$height[10],row8)
  }
  if (hc.fit$merge[10]==1 & hc.fit$merge[21]==9) {
    row10 <- c(row1,hc.fit$height[10],row9)
  }
  if (hc.fit$merge[10]==2 & hc.fit$merge[21]==3) {
    row10 <- c(row2,hc.fit$height[10],row3)
  }
  if (hc.fit$merge[10]==2 & hc.fit$merge[21]==4) {
    row10 <- c(row2,hc.fit$height[10],row4)
  }
  if (hc.fit$merge[10]==2 & hc.fit$merge[21]==5) {
    row10 <- c(row2,hc.fit$height[10],row5)
  }
  if (hc.fit$merge[10]==2 & hc.fit$merge[21]==6) {
    row10 <- c(row2,hc.fit$height[10],row6)
  }
  if (hc.fit$merge[10]==2 & hc.fit$merge[21]==7) {
    row10 <- c(row2,hc.fit$height[10],row7)
  }
  if (hc.fit$merge[10]==2 & hc.fit$merge[21]==8) {
    row10 <- c(row2,hc.fit$height[10],row8)
  }
  if (hc.fit$merge[10]==2 & hc.fit$merge[21]==9) {
    row10 <- c(row2,hc.fit$height[10],row9)
  }
  if (hc.fit$merge[10]==3 & hc.fit$merge[21]==4) {
    row10 <- c(row3,hc.fit$height[10],row4)
  }
  if (hc.fit$merge[10]==3 & hc.fit$merge[21]==5) {
    row10 <- c(row3,hc.fit$height[10],row5)
  }
  if (hc.fit$merge[10]==3 & hc.fit$merge[21]==6) {
    row10 <- c(row3,hc.fit$height[10],row6)
  }
  if (hc.fit$merge[10]==3 & hc.fit$merge[21]==7) {
    row10 <- c(row3,hc.fit$height[10],row7)
  }
  if (hc.fit$merge[10]==3 & hc.fit$merge[21]==8) {
    row10 <- c(row3,hc.fit$height[10],row8)
  }
  if (hc.fit$merge[10]==3 & hc.fit$merge[21]==9) {
    row10 <- c(row3,hc.fit$height[10],row9)
  }
  if (hc.fit$merge[10]==4 & hc.fit$merge[21]==5) {
    row10 <- c(row4,hc.fit$height[10],row5)
  }
  if (hc.fit$merge[10]==4 & hc.fit$merge[21]==6) {
    row10 <- c(row4,hc.fit$height[10],row6)
  }
  if (hc.fit$merge[10]==4 & hc.fit$merge[21]==7) {
    row10 <- c(row4,hc.fit$height[10],row7)
  }
  if (hc.fit$merge[10]==4 & hc.fit$merge[21]==8) {
    row10 <- c(row4,hc.fit$height[10],row8)
  }
  if (hc.fit$merge[10]==4 & hc.fit$merge[21]==9) {
    row10 <- c(row4,hc.fit$height[10],row9)
  }
  if (hc.fit$merge[10]==5 & hc.fit$merge[21]==6) {
    row10 <- c(row5,hc.fit$height[10],row6)
  }
  if (hc.fit$merge[10]==5 & hc.fit$merge[21]==7) {
    row10 <- c(row5,hc.fit$height[10],row7)
  }
  if (hc.fit$merge[10]==5 & hc.fit$merge[21]==8) {
    row10 <- c(row5,hc.fit$height[10],row8)
  }
  if (hc.fit$merge[10]==5 & hc.fit$merge[21]==9) {
    row10 <- c(row5,hc.fit$height[10],row9)
  }
  if (hc.fit$merge[10]==6 & hc.fit$merge[21]==7) {
    row10 <- c(row6,hc.fit$height[10],row7)
  }
  if (hc.fit$merge[10]==6 & hc.fit$merge[21]==8) {
    row10 <- c(row6,hc.fit$height[10],row8)
  }
  if (hc.fit$merge[10]==6 & hc.fit$merge[21]==9) {
    row10 <- c(row6,hc.fit$height[10],row9)
  }
  if (hc.fit$merge[10]==7 & hc.fit$merge[21]==8) {
    row10 <- c(row7,hc.fit$height[10],row8)
  }
  if (hc.fit$merge[10]==7 & hc.fit$merge[21]==9) {
    row10 <- c(row7,hc.fit$height[10],row9)
  }
  if (hc.fit$merge[10]==8 & hc.fit$merge[21]==9) {
    row10 <- c(row8,hc.fit$height[10],row9)
  }
  #saving row11
  #negative-positive
  if (hc.fit$merge[11]<0 & hc.fit$merge[22]==1) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row1)
  }
  if (hc.fit$merge[11]<0 & hc.fit$merge[22]==2) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row2)
  }
  if (hc.fit$merge[11] <0 & hc.fit$merge[22]==3) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row3)
  }
  if (hc.fit$merge[11]<0 & hc.fit$merge[22]==4) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row4)
  }
  if (hc.fit$merge[11]<0 & hc.fit$merge[21]==5) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row5)
  }
  if (hc.fit$merge[11]<0 & hc.fit$merge[21]==6) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row6)
  }
  if (hc.fit$merge[11]<0 & hc.fit$merge[22]==7) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row7)
  }
  if (hc.fit$merge[11]<0 & hc.fit$merge[22]==8) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row8)
  }
  if (hc.fit$merge[11]<0 & hc.fit$merge[22]==9) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]<0 & hc.fit$merge[22]==10) {
    row11 <- c(abs(hc.fit$merge[11]),hc.fit$height[11],row10)
  }
  #positive-positive
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==2) {
    row11 <- c(row1,hc.fit$height[11],row2)
  }
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==3) {
    row11 <- c(row1,hc.fit$height[11],row3)
  }
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==4) {
    row11 <- c(row1,hc.fit$height[11],row4)
  }
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==5) {
    row11 <- c(row1,hc.fit$height[11],row5)
  }
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==6) {
    row11 <- c(row1,hc.fit$height[11],row6)
  }
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==7) {
    row11 <- c(row1,hc.fit$height[11],row7)
  }
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==8) {
    row11 <- c(row1,hc.fit$height[11],row8)
  }
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==9) {
    row11 <- c(row1,hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]==1 & hc.fit$merge[22]==10) {
    row11 <- c(row1,hc.fit$height[11],row10)
  }
  if (hc.fit$merge[11]==2 & hc.fit$merge[22]==3) {
    row11 <- c(row2,hc.fit$height[11],row3)
  }
  if (hc.fit$merge[11]==2 & hc.fit$merge[22]==4) {
    row11 <- c(row2,hc.fit$height[11],row4)
  }
  if (hc.fit$merge[11]==2 & hc.fit$merge[22]==5) {
    row11 <- c(row2,hc.fit$height[11],row5)
  }
  if (hc.fit$merge[11]==2 & hc.fit$merge[22]==6) {
    row11 <- c(row2,hc.fit$height[11],row6)
  }
  if (hc.fit$merge[11]==2 & hc.fit$merge[22]==7) {
    row11 <- c(row2,hc.fit$height[11],row7)
  }
  if (hc.fit$merge[11]==2 & hc.fit$merge[22]==8) {
    row11 <- c(row2,hc.fit$height[11],row8)
  }
  if (hc.fit$merge[11]==2 & hc.fit$merge[22]==9) {
    row11 <- c(row2,hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]==2 & hc.fit$merge[22]==10) {
    row11 <- c(row2,hc.fit$height[11],row10)
  }
  if (hc.fit$merge[11]==3 & hc.fit$merge[22]==4) {
    row11 <- c(row3,hc.fit$height[11],row4)
  }
  if (hc.fit$merge[11]==3 & hc.fit$merge[22]==5) {
    row11 <- c(row3,hc.fit$height[11],row5)
  }
  if (hc.fit$merge[11]==3 & hc.fit$merge[22]==6) {
    row11 <- c(row3,hc.fit$height[11],row6)
  }
  if (hc.fit$merge[11]==3 & hc.fit$merge[22]==7) {
    row11 <- c(row3,hc.fit$height[11],row7)
  }
  if (hc.fit$merge[11]==3 & hc.fit$merge[22]==8) {
    row11 <- c(row3,hc.fit$height[11],row8)
  }
  if (hc.fit$merge[11]==3 & hc.fit$merge[22]==9) {
    row11 <- c(row3,hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]==3 & hc.fit$merge[22]==10) {
    row11 <- c(row3,hc.fit$height[11],row10)
  }
  if (hc.fit$merge[11]==4 & hc.fit$merge[22]==5) {
    row11 <- c(row4,hc.fit$height[11],row5)
  }
  if (hc.fit$merge[11]==4 & hc.fit$merge[22]==6) {
    row11 <- c(row4,hc.fit$height[11],row6)
  }
  if (hc.fit$merge[11]==4 & hc.fit$merge[22]==7) {
    row11 <- c(row4,hc.fit$height[11],row7)
  }
  if (hc.fit$merge[11]==4 & hc.fit$merge[22]==8) {
    row11 <- c(row4,hc.fit$height[11],row8)
  }
  if (hc.fit$merge[11]==4 & hc.fit$merge[22]==9) {
    row11 <- c(row4,hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]==4 & hc.fit$merge[22]==10) {
    row11 <- c(row4,hc.fit$height[11],row10)
  }
  if (hc.fit$merge[11]==5 & hc.fit$merge[22]==6) {
    row11 <- c(row5,hc.fit$height[11],row6)
  }
  if (hc.fit$merge[11]==5 & hc.fit$merge[22]==7) {
    row11 <- c(row5,hc.fit$height[11],row7)
  }
  if (hc.fit$merge[11]==5 & hc.fit$merge[22]==8) {
    row11 <- c(row5,hc.fit$height[11],row8)
  }
  if (hc.fit$merge[11]==5 & hc.fit$merge[22]==9) {
    row11 <- c(row5,hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]==5 & hc.fit$merge[22]==10) {
    row11 <- c(row5,hc.fit$height[11],row10)
  }
  if (hc.fit$merge[11]==6 & hc.fit$merge[22]==7) {
    row11 <- c(row6,hc.fit$height[11],row7)
  }
  if (hc.fit$merge[11]==6 & hc.fit$merge[22]==8) {
    row11 <- c(row6,hc.fit$height[11],row8)
  }
  if (hc.fit$merge[11]==6 & hc.fit$merge[22]==9) {
    row11 <- c(row6,hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]==6 & hc.fit$merge[22]==10) {
    row11 <- c(row6,hc.fit$height[11],row10)
  }
  if (hc.fit$merge[11]==7 & hc.fit$merge[22]==8) {
    row11 <- c(row7,hc.fit$height[11],row8)
  }
  if (hc.fit$merge[11]==7 & hc.fit$merge[22]==9) {
    row11 <- c(row7,hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]==7 & hc.fit$merge[22]==10) {
    row11 <- c(row7,hc.fit$height[11],row10)
  }
  if (hc.fit$merge[11]==8 & hc.fit$merge[22]==9) {
    row11 <- c(row8,hc.fit$height[11],row9)
  }
  if (hc.fit$merge[11]==8 & hc.fit$merge[22]==10) {
    row11 <- c(row8,hc.fit$height[11],row10)
  }
  if (hc.fit$merge[11]==9 & hc.fit$merge[22]==10) {
    row11 <- c(row9,hc.fit$height[11],row10)
  }
  all.heights <- c(row11[2],row11[4],row11[6],row11[8],
                   row11[10],row11[12],row11[14],row11[16],
                   row11[18],row11[20],row11[22])
  labels.in.order <- c(row11[1],row11[3],row11[5],row11[7],row11[9],
                  row11[11],row11[13],row11[15],row11[17],
                  row11[19],row11[21],row11[23])
  # Heights between group 1,2,3
  whichA <- which(labels.in.order==1|labels.in.order==2|labels.in.order==3)
  h <- NULL
  for (j in whichA[1]:(whichA[2]-1)) {
  h <- c(h,all.heights[j])  
  }
  heightsA <- max(h)
  h <- NULL
  for (j in whichA[2]:(whichA[3]-1)) {
    h <- c(h,all.heights[j])  
  }
  heightsB <- max(h)
  heights1 <- c(heightsA,heightsB)

  # Heights between group 4,5,6
  whichB <- which(labels.in.order==4|labels.in.order==5|labels.in.order==6)
  h <- NULL
  for (j in whichB[1]:(whichB[2]-1)) {
    h <- c(h,all.heights[j])  
  }
  heightsA <- max(h)
  h <- NULL
  for (j in whichB[2]:(whichB[3]-1)) {
    h <- c(h,all.heights[j])  
  }
  heightsB <- max(h)
  heights2 <- c(heightsA,heightsB)
  
  # Heights between group 7,8,9
  whichC <- which(labels.in.order==7|labels.in.order==8|labels.in.order==9)
  h <- NULL
  for (j in whichC[1]:(whichC[2]-1)) {
    h <- c(h,all.heights[j])  
  }
  heightsA <- max(h)
  h <- NULL
  for (j in whichC[2]:(whichC[3]-1)) {
    h <- c(h,all.heights[j])  
  }
  heightsB <- max(h)
  heights3 <- c(heightsA,heightsB)
  
  # Heights between group 10,11,12
  whichD <- which(labels.in.order==10|labels.in.order==11|labels.in.order==12)
  h <- NULL
  for (j in whichD[1]:(whichD[2]-1)) {
    h <- c(h,all.heights[j])  
  }
  heightsA <- max(h)
  h <- NULL
  for (j in whichD[2]:(whichD[3]-1)) {
    h <- c(h,all.heights[j])  
  }
  heightsB <- max(h)
  heights4 <- c(heightsA,heightsB)
   
  eight.heights.total <- sum(heights1,heights2,heights3,heights4)
  I3D.separation <- (eight.heights.total/2)/max(all.heights)
  png(paste(myFilesShort[i],"Method wardD2.png", sep = ""), width=1110,
      height =1000)
  par(oma=c(7,3,3,3))
  plot(hc.fit, cex=2, main = paste(myFilesShort[i]), sub="", xlab = "hclust(method = ward.D2)",
       xaxt="n", yaxt = "n", cex.lab=1.5, ylab="", cex.main=1.5)
  mtext(side = 2, "Height",cex = 1.5, line = -2)
  mtext(side = 3, paste("I3D Separation =",round(I3D.separation,3),sep = " "),
        cex = 2, line = -4)
  heightss <- hc.fit$height
  axis(side = 4, at=c(round(heightss[2],0),round(heightss[4],0),round(heightss[6],0),
                      round(heightss[8],0),round(heightss[10],0)), 
       las=1, cex.axis=1.5)
  axis(side = 2, at=c(round(heightss[1],0),round(heightss[3],0),round(heightss[5],0),
                      round(heightss[7],0),round(heightss[9],0),round(heightss[11],0)), 
       las=1, cex.axis=1.5)
  mtext(side = 1, line = 5.5, adj=1, cex=1.3, paste("1,2,3", site[1], dates[1], 
                                                    dates[2], dates[3], "4,5,6", site[1], dates[4], 
                                                    dates[5], dates[6], sep = "    ")) 
  mtext(side = 1, line = 7, adj=1, cex=1.3, paste("7,8,9", site[7], dates[1], 
                                                  dates[2], dates[3], "10,11,12", site[7], dates[4], 
                                                  dates[5], dates[6], sep = "    "))
  mtext(side = 1, line = 8.5, adj=1, cex=1.2, expression(italic(Twelve ~days)))# ~from ~2 ~x ~111 ~days ~of ~clustering)))
  mtext(side = 1, line = 10, adj=1, cex=1.2, expression(italic(Indices:~BackgroundNoise ~Snr ~EventsPerSecond ~LowFreqCover ~AcousticComplexity ~EntropyOfPeaksSpectrum ~EntropyOfCoVSpectrum)))
  mtext(side = 3, line = -0.8, cex=1.5, paste("heights: ", round(heightss[11],0),
                                              round(heightss[10],0), round(heightss[9],0), round(heightss[8],0),
                                              round(heightss[7],0), round(heightss[6],0), round(heightss[5],0), 
                                              round(heightss[4],0), round(heightss[3],0), round(heightss[2],0), 
                                              round(heightss[1],0), sep = ", "))
  heights <- rbind(heights, heightss)
  dev.off()
}

