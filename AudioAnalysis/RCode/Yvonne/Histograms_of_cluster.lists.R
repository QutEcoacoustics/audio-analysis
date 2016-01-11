####################################################################
# 7 September 2015
# Histograms of cluster.lists - daily and four-hourly
# Follow this code with 
####################################################################
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\kmeans_30clusters")
setwd("C:\\Work\\CSV files\\DataSet_Exp2\\24hourFilesA\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp3a\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hybrid\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Kmeans\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2_new_new\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2_new_new\\Hybrid\\")
#cluster.list <- read.csv("Cluster_list_kmeans_22June-16July2015_5,7,9,10,11,12,13,17,18_30Gympie NP1 .csv", header = T)
setof6 <- c(6,8,10,12,14,16) # set which columns are to be used
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\kmeans")
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\hybrid12days")
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\hclust")
cluster.lists.kmeans.exp2   <- read.csv("kmeans_clust.csv", header = T)[,setof6] # kmeans
cluster.lists.hclust.k.exp2 <- read.csv("hc_fit_set_cutree_k.csv", header = T) # hclust
cluster.lists.mclust.exp2   <- read.csv("mclustlist_ds3norm_1_50.csv", header = T) # mclust

z <- 5 # set the k value for hybrid method 
cluster.list.hybrid.exp2    <- read.csv(paste("hybrid_clust_k",z,".csv",sep=""), header = T) # hybrid

#indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2\\Final DataSet 30_31July_1Aug_31Aug_1_4Sept.csv", 
#                    header=T)
indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2a\\Final DataSet 30_31July_1Aug_31Aug_1_4Sept.csv", 
                    header=T)

day.ref <- which(indices$minute.of.day=="0")
day.ref <- c(day.ref, (length(indices$minute.of.day)+1))
four.am.ref <- which(indices$minute.of.day == "240")
eight.am.ref <- which(indices$minute.of.day=="480")
midday.ref <- which(indices$minute.of.day=="720")
four.pm.ref <- which(indices$minute.of.day=="960")
eight.pm.ref <- which(indices$minute.of.day=="1200")
four.hour.ref <- c(day.ref, four.am.ref, eight.am.ref, midday.ref,
                   four.pm.ref, eight.pm.ref)
four.hour.ref <- sort(four.hour.ref)
four.hour.ref <- c(four.hour.ref, (length(indices$minute.of.day)+1))

six.am.ref <- which(indices$minute.of.day == "360")
six.pm.ref <- which(indices$minute.of.day=="1080")
six.hour.ref <- c(day.ref, six.am.ref, midday.ref, six.pm.ref)
six.hour.ref <- sort(six.hour.ref)
six.hour.ref <- c(six.hour.ref, (length(indices$minute.of.day)+1))

twelve.hour.ref <- c(midday.ref, day.ref)
twelve.hour.ref <- sort(twelve.hour.ref)
twelve.hour.ref <- c(twelve.hour.ref, length(indices$minute.of.day)+1)

dates <- unique(indices$rec.date)
#dates2 <- rep(dates, each=6)
dates2 <- rep(dates, 2)
length.ref <- length(indices$X)

#####################################################
# Saving the kmeans 24 hour files
####################################################
twentyfour_hour_table_1 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_2 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_3 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_4 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_5 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_6 <- NULL #read.csv(text="col1,col2")

cluster.list <- cluster.lists.kmeans.exp2
#cluster.list <- cluster.list.test
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(day.ref)-1)) {
    #cluster.ref <- table(cluster.list[day.ref[j]:day.ref[j+1]-1,i])
    cluster.ref <- hist(cluster.list[day.ref[j]:(day.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    print(length(cluster.ref$counts))
    if (i == 1) {twentyfour_hour_table_1 <- rbind(twentyfour_hour_table_1, cluster.ref$counts)}
    if (i == 2) {twentyfour_hour_table_2 <- rbind(twentyfour_hour_table_2, cluster.ref$counts)}
    if (i == 3) {twentyfour_hour_table_3 <- rbind(twentyfour_hour_table_3, cluster.ref$counts)}  
    if (i == 4) {twentyfour_hour_table_4 <- rbind(twentyfour_hour_table_4, cluster.ref$counts)}
    if (i == 5) {twentyfour_hour_table_5 <- rbind(twentyfour_hour_table_5, cluster.ref$counts)}
    if (i == 6) {twentyfour_hour_table_6 <- rbind(twentyfour_hour_table_6, cluster.ref$counts)}  
  }
}
twentyfour_hour_table_1 <- as.data.frame(twentyfour_hour_table_1)
twentyfour_hour_table_2 <- as.data.frame(twentyfour_hour_table_2)
twentyfour_hour_table_3 <- as.data.frame(twentyfour_hour_table_3)
twentyfour_hour_table_4 <- as.data.frame(twentyfour_hour_table_4)
twentyfour_hour_table_5 <- as.data.frame(twentyfour_hour_table_5)
twentyfour_hour_table_6 <- as.data.frame(twentyfour_hour_table_6)

column.names <- NULL
if (i==1) {
  for (i in 1:(length(twentyfour_hour_table_1))) {
    col.names <- paste("clus_", i, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_1) <- column.names
}
if (i==2) {
  for (i in 1:(length(twentyfour_hour_table_2))) {
    col.names <- paste("clus_", i, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_2) <- column.names
}
if (i==3) {
  for (i in 1:(length(twentyfour_hour_table_3))) {
    col.names <- paste("clus_", i, sep = "")
   column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_3) <- column.names
}
column.names <- NULL
if (i==4) {
  for (i in 1:(length(twentyfour_hour_table_4))) {
    col.names <- paste("clus_", i, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_4) <- column.names
}
if (i==5) {
  for (i in 1:(length(twentyfour_hour_table_5))) {
    col.names <- paste("clus_", i, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_5) <- column.names
}
if (i==6) {
  for (i in 1:(length(twentyfour_hour_table_6))) {
    col.names <- paste("clus_", i, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(twentyfour_hour_table_6) <- column.names
}

site <- c(rep("GympieNP",6), rep("WoondumNP",6))

twentyfour_hour_table_1 <- cbind(twentyfour_hour_table_1,site,as.character(dates2))
twentyfour_hour_table_2 <- cbind(twentyfour_hour_table_2,site,as.character(dates2))
twentyfour_hour_table_3 <- cbind(twentyfour_hour_table_3,site,as.character(dates2))
twentyfour_hour_table_4 <- cbind(twentyfour_hour_table_4,site,as.character(dates2))
twentyfour_hour_table_5 <- cbind(twentyfour_hour_table_5,site,as.character(dates2))
twentyfour_hour_table_6 <- cbind(twentyfour_hour_table_6,site,as.character(dates2))

write.csv(twentyfour_hour_table_1, paste("kmeans_k",setof6[1],"_24hour.csv",sep = ""), 
          row.names = F)
write.csv(twentyfour_hour_table_2, paste("kmeans_k",setof6[2],"_24hour.csv",sep = ""), 
          row.names = F)
write.csv(twentyfour_hour_table_3, paste("kmeans_k",setof6[3],"_24hour.csv",sep = ""), 
          row.names = F)
write.csv(twentyfour_hour_table_4, paste("kmeans_k",setof6[4],"_24hour.csv",sep = ""), 
          row.names = F)
write.csv(twentyfour_hour_table_5, paste("kmeans_k",setof6[5],"_24hour.csv",sep = ""), 
          row.names = F)
write.csv(twentyfour_hour_table_6, paste("kmeans_k",setof6[6],"_24hour.csv",sep = ""), 
          row.names = F)

#####################################################
# Saving the hclust_k 24 hour files
####################################################
#setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hierarchical\\")
#setwd("C:\\Work\\CSV files\\DataSet_Exp3a\\Hierarchical\\")
twentyfour_hour_table_1 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_2 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_3 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_4 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_5 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_6 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_7 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_8 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_9 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_10 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_11 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_12 <- NULL #read.csv(text="col1,col2")

cluster.list <- cluster.lists.hclust.k.exp2
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(day.ref)-1)) {
    cluster.ref <- hist(cluster.list[day.ref[j]:(day.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    print(length(cluster.ref$counts))
    
    if (i == 1) {twentyfour_hour_table_1 <- rbind(twentyfour_hour_table_1, 
                                                  cluster.ref$counts)}
    if (i == 2) {twentyfour_hour_table_2 <- rbind(twentyfour_hour_table_2, cluster.ref$counts)}
    if (i == 3) {twentyfour_hour_table_3 <- rbind(twentyfour_hour_table_3, cluster.ref$counts)}  
    if (i == 4) {twentyfour_hour_table_4 <- rbind(twentyfour_hour_table_4, cluster.ref$counts)}
    if (i == 5) {twentyfour_hour_table_5 <- rbind(twentyfour_hour_table_5, cluster.ref$counts)}
    if (i == 6) {twentyfour_hour_table_6 <- rbind(twentyfour_hour_table_6, cluster.ref$counts)}  
    if (i == 7) {twentyfour_hour_table_7 <- rbind(twentyfour_hour_table_7, cluster.ref$counts)}
    if (i == 8) {twentyfour_hour_table_8 <- rbind(twentyfour_hour_table_8, cluster.ref$counts)}
    if (i == 9) {twentyfour_hour_table_9 <- rbind(twentyfour_hour_table_9, cluster.ref$counts)}
    if (i == 10) {twentyfour_hour_table_10 <- rbind(twentyfour_hour_table_10, cluster.ref$counts)}  
    if (i == 11) {twentyfour_hour_table_11 <- rbind(twentyfour_hour_table_11, cluster.ref$counts)}
    if (i == 12) {twentyfour_hour_table_12 <- rbind(twentyfour_hour_table_12, cluster.ref$counts)}
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

site <- c(rep("GympieNP",6), rep("WoondumNP",6))

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

write.csv(twentyfour_hour_table_1, "hclust_average_k5_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_2, "hclust_average_k10_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_3, "hclust_average_k15_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_4, "hclust_average_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_5, "hclust_average_k25_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_6, "hclust_average_k30_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_7, "hclust_wardd2_k5_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_8, "hclust_wardd2_k10_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_9, "hclust_wardd2_k15_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_10, "hclust_wardd2_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_11, "hclust_wardd2_k25_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_12, "hclust_wardd2_k30_24hour.csv", 
          row.names = F)

####################################################
# Saving the hclust_k 4 hour files
####################################################
four_hour_table_1 <- NULL 
four_hour_table_2 <- NULL 
four_hour_table_3 <- NULL 
four_hour_table_4 <- NULL 
four_hour_table_5 <- NULL 
four_hour_table_6 <- NULL 
four_hour_table_7 <- NULL 
four_hour_table_8 <- NULL 
four_hour_table_9 <- NULL 
four_hour_table_10 <- NULL
four_hour_table_11 <- NULL 
four_hour_table_12 <- NULL 

n <- 6  # number of time periods in 24 hours
cluster.list <- cluster.lists.hclust.k.exp2
for (i in 1:length(cluster.list)) {
  for (j in 1:(length(four.hour.ref)-1)) {
    cluster.ref <- hist(cluster.list[four.hour.ref[j]:(four.hour.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    cluster.ref$counts
    if (i == 1) {four_hour_table_1 <- append(four_hour_table_1, cluster.ref$counts)}
    if (i == 2) {four_hour_table_2 <- append(four_hour_table_2, cluster.ref$counts)}
    if (i == 3) {four_hour_table_3 <- append(four_hour_table_3, cluster.ref$counts)}  
    if (i == 4) {four_hour_table_4 <- append(four_hour_table_4, cluster.ref$counts)}
    if (i == 5) {four_hour_table_5 <- append(four_hour_table_5, cluster.ref$counts)}
    if (i == 6) {four_hour_table_6 <- append(four_hour_table_6, cluster.ref$counts)}  
    if (i == 7) {four_hour_table_7 <- append(four_hour_table_7, cluster.ref$counts)}
    if (i == 8) {four_hour_table_8 <- append(four_hour_table_8, cluster.ref$counts)}
    if (i == 9) {four_hour_table_9 <- append(four_hour_table_9, cluster.ref$counts)}
    if (i == 10) {four_hour_table_10 <- append(four_hour_table_10, cluster.ref$counts)}  
    if (i == 11) {four_hour_table_11 <- append(four_hour_table_11, cluster.ref$counts)}
    if (i == 12) {four_hour_table_12 <- append(four_hour_table_12, cluster.ref$counts)}
    l <- length(cluster.ref$counts)
  }
  if (i == 1) {
    l <- 5 # number of clusters
    four_hour_table_1 <- data.frame(four_hour_table_1[1:(n*l)],
                                    four_hour_table_1[(n*l+1)   :(2*n*l)],
                                    four_hour_table_1[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_1[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_1[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_1[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_1[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_1[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_1[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_1[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_1[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_1[(11*n*l+1) :(12*n*l)])
    four_hour_table_1 <- t(four_hour_table_1)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_1) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_1 <- cbind(four_hour_table_1,site,as.character(dates2))
    write.csv(four_hour_table_1, "hclust_average_k5_4hour.csv", 
              row.names = F)
  }
  if (i == 2) {
    l <- 10
    four_hour_table_2 <- data.frame(four_hour_table_2[1:(n*l)],
                                    four_hour_table_2[(n*l+1)   :(2*n*l)],
                                    four_hour_table_2[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_2[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_2[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_2[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_2[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_2[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_2[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_2[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_2[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_2[(11*n*l+1) :(12*n*l)])
    four_hour_table_2 <- t(four_hour_table_2)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_2) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_2 <- cbind(four_hour_table_2,site,as.character(dates2))
    write.csv(four_hour_table_2, "hclust_average_k10_4hour.csv", 
              row.names = F)
  }
  if (i == 3) {
    l <- 15
    four_hour_table_3 <- data.frame(four_hour_table_3[1:(n*l)],
                                    four_hour_table_3[(n*l+1)   :(2*n*l)],
                                    four_hour_table_3[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_3[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_3[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_3[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_3[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_3[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_3[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_3[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_3[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_3[(11*n*l+1) :(12*n*l)])
    four_hour_table_3 <- t(four_hour_table_3)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_3) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_3 <- cbind(four_hour_table_3,site,as.character(dates2))
    write.csv(four_hour_table_3, "hclust_average_k15_4hour.csv", 
              row.names = F)
  }
  if (i == 4) {
    l <- 20
    four_hour_table_4 <- data.frame(four_hour_table_4[1:(n*l)],
                                    four_hour_table_4[(n*l+1)   :(2*n*l)],
                                    four_hour_table_4[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_4[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_4[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_4[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_4[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_4[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_4[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_4[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_4[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_4[(11*n*l+1) :(12*n*l)])
    four_hour_table_4 <- t(four_hour_table_4)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_4) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_4 <- cbind(four_hour_table_4,site,as.character(dates2))
    write.csv(four_hour_table_4, "hclust_average_k20_4hour.csv", 
              row.names = F)
  }
  if (i == 5) {
    l <- 25
    four_hour_table_5 <- data.frame(four_hour_table_5[1:(n*l)],
                                    four_hour_table_5[(n*l+1)   :(2*n*l)],
                                    four_hour_table_5[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_5[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_5[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_5[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_5[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_5[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_5[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_5[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_5[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_5[(11*n*l+1) :(12*n*l)])
    four_hour_table_5 <- t(four_hour_table_5)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_5) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_5 <- cbind(four_hour_table_5,site,as.character(dates2))
    write.csv(four_hour_table_5, "hclust_average_k25_4hour.csv", 
              row.names = F)
  }
  if (i == 6) {
    l <- 30
    four_hour_table_6 <- data.frame(four_hour_table_6[1:(n*l)],
                                    four_hour_table_6[(n*l+1)   :(2*n*l)],
                                    four_hour_table_6[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_6[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_6[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_6[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_6[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_6[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_6[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_6[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_6[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_6[(11*n*l+1) :(12*n*l)])
    four_hour_table_6 <- t(four_hour_table_6)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_6) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_6 <- cbind(four_hour_table_6,site,as.character(dates2))
    write.csv(four_hour_table_6, "hclust_average_k30_4hour.csv", 
              row.names = F)
  }
  if (i == 7) {
    l <- 5
    four_hour_table_7 <- data.frame(four_hour_table_7[1:(n*l)],
                                    four_hour_table_7[(n*l+1)   :(2*n*l)],
                                    four_hour_table_7[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_7[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_7[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_7[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_7[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_7[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_7[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_7[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_7[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_7[(11*n*l+1) :(12*n*l)])
    four_hour_table_7 <- t(four_hour_table_7)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_7) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_7 <- cbind(four_hour_table_7,site,as.character(dates2))
    write.csv(four_hour_table_7, "hclust_wardD2_k5_4hour.csv", 
              row.names = F)
  }
  if (i == 8) {
    l <- 10
    four_hour_table_8 <- data.frame(four_hour_table_8[1:(n*l)],
                                    four_hour_table_8[(n*l+1)   :(2*n*l)],
                                    four_hour_table_8[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_8[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_8[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_8[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_8[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_8[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_8[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_8[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_8[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_8[(11*n*l+1) :(12*n*l)])
    four_hour_table_8 <- t(four_hour_table_8)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_8) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_7 <- cbind(four_hour_table_7,site,as.character(dates2))
    write.csv(four_hour_table_8, "hclust_wardD2_k10_4hour.csv", 
              row.names = F)
  }    
    if (i == 9) {
      l <- 15
      four_hour_table_9 <- data.frame(four_hour_table_9[1:(n*l)],
                                      four_hour_table_9[(n*l+1)   :(2*n*l)],
                                      four_hour_table_9[(2*n*l+1) :(3*n*l)],
                                      four_hour_table_9[(3*n*l+1) :(4*n*l)],
                                      four_hour_table_9[(4*n*l+1) :(5*n*l)],
                                      four_hour_table_9[(5*n*l+1) :(6*n*l)],
                                      four_hour_table_9[(6*n*l+1) :(7*n*l)],
                                      four_hour_table_9[(7*n*l+1) :(8*n*l)],
                                      four_hour_table_9[(8*n*l+1) :(9*n*l)],
                                      four_hour_table_9[(9*n*l+1) :(10*n*l)],
                                      four_hour_table_9[(10*n*l+1) :(11*n*l)],
                                      four_hour_table_9[(11*n*l+1) :(12*n*l)])
      four_hour_table_9 <- t(four_hour_table_9)
      column.names <- NULL
      for (k in 1:(l*n)) {
        col.names <- paste("clus_", k, sep = "")
        column.names <- c(column.names,col.names)
      }
      colnames(four_hour_table_9) <- column.names
      site <- c(rep("GympieNP",6), rep("WoondumNP",6))
      four_hour_table_9 <- cbind(four_hour_table_9,site,as.character(dates2))
      write.csv(four_hour_table_9, "hclust_wardD2_k15_4hour.csv", 
                row.names = F)
    }
    if (i == 10) {
      l <- 20
      four_hour_table_10 <- data.frame(four_hour_table_10[1:(n*l)],
                                      four_hour_table_10[(n*l+1)   :(2*n*l)],
                                      four_hour_table_10[(2*n*l+1) :(3*n*l)],
                                      four_hour_table_10[(3*n*l+1) :(4*n*l)],
                                      four_hour_table_10[(4*n*l+1) :(5*n*l)],
                                      four_hour_table_10[(5*n*l+1) :(6*n*l)],
                                      four_hour_table_10[(6*n*l+1) :(7*n*l)],
                                      four_hour_table_10[(7*n*l+1) :(8*n*l)],
                                      four_hour_table_10[(8*n*l+1) :(9*n*l)],
                                      four_hour_table_10[(9*n*l+1) :(10*n*l)],
                                      four_hour_table_10[(10*n*l+1) :(11*n*l)],
                                      four_hour_table_10[(11*n*l+1) :(12*n*l)])
      four_hour_table_10 <- t(four_hour_table_10)
      column.names <- NULL
      for (k in 1:(l*n)) {
        col.names <- paste("clus_", k, sep = "")
        column.names <- c(column.names,col.names)
      }
      colnames(four_hour_table_10) <- column.names
      site <- c(rep("GympieNP",6), rep("WoondumNP",6))
      four_hour_table_10 <- cbind(four_hour_table_10,site,as.character(dates2))
      write.csv(four_hour_table_10, "hclust_wardD2_k20_4hour.csv", 
                row.names = F)
    }
    if (i == 11) {
      l <- 25
      four_hour_table_11 <- data.frame(four_hour_table_11[1:(n*l)],
                                      four_hour_table_11[(n*l+1)   :(2*n*l)],
                                      four_hour_table_11[(2*n*l+1) :(3*n*l)],
                                      four_hour_table_11[(3*n*l+1) :(4*n*l)],
                                      four_hour_table_11[(4*n*l+1) :(5*n*l)],
                                      four_hour_table_11[(5*n*l+1) :(6*n*l)],
                                      four_hour_table_11[(6*n*l+1) :(7*n*l)],
                                      four_hour_table_11[(7*n*l+1) :(8*n*l)],
                                      four_hour_table_11[(8*n*l+1) :(9*n*l)],
                                      four_hour_table_11[(9*n*l+1) :(10*n*l)],
                                      four_hour_table_11[(10*n*l+1) :(11*n*l)],
                                      four_hour_table_11[(11*n*l+1) :(12*n*l)])
      four_hour_table_11 <- t(four_hour_table_11)
      column.names <- NULL
      for (k in 1:(l*n)) {
        col.names <- paste("clus_", k, sep = "")
        column.names <- c(column.names,col.names)
      }
      colnames(four_hour_table_11) <- column.names
      site <- c(rep("GympieNP",6), rep("WoondumNP",6))
      four_hour_table_11 <- cbind(four_hour_table_11,site,as.character(dates2))
      write.csv(four_hour_table_11, "hclust_wardD2_k25_4hour.csv", 
                row.names = F)
    }
    if (i == 12) {
      l <- 30
      four_hour_table_12 <- data.frame(four_hour_table_12[1:(n*l)],
                                      four_hour_table_12[(n*l+1)   :(2*n*l)],
                                      four_hour_table_12[(2*n*l+1) :(3*n*l)],
                                      four_hour_table_12[(3*n*l+1) :(4*n*l)],
                                      four_hour_table_12[(4*n*l+1) :(5*n*l)],
                                      four_hour_table_12[(5*n*l+1) :(6*n*l)],
                                      four_hour_table_12[(6*n*l+1) :(7*n*l)],
                                      four_hour_table_12[(7*n*l+1) :(8*n*l)],
                                      four_hour_table_12[(8*n*l+1) :(9*n*l)],
                                      four_hour_table_12[(9*n*l+1) :(10*n*l)],
                                      four_hour_table_12[(10*n*l+1) :(11*n*l)],
                                      four_hour_table_12[(11*n*l+1) :(12*n*l)])
      four_hour_table_12 <- t(four_hour_table_12)
      column.names <- NULL
      for (k in 1:(l*n)) {
        col.names <- paste("clus_", k, sep = "")
        column.names <- c(column.names,col.names)
      }
    colnames(four_hour_table_12) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_12 <- cbind(four_hour_table_12,site,as.character(dates2))
    write.csv(four_hour_table_12, "hclust_wardD2_k30_4hour.csv", 
              row.names = F)
  }
}

####################################################
# Saving the hclust_k 6 hour files
####################################################
six_hour_table_1 <- NULL 
six_hour_table_2 <- NULL 
six_hour_table_3 <- NULL 
six_hour_table_4 <- NULL 
six_hour_table_5 <- NULL 
six_hour_table_6 <- NULL 
six_hour_table_7 <- NULL 
six_hour_table_8 <- NULL 
six_hour_table_9 <- NULL 
six_hour_table_10 <- NULL
six_hour_table_11 <- NULL
six_hour_table_12 <- NULL

n <- 4 # number of periods in 24 hours
#cluster.list <- cluster.lists.hclust.k.exp2
cluster.list <- cluster.list.hybrid.exp2
for (i in 1:length(cluster.list)) {
  for (j in 1:(length(six.hour.ref)-1)) {
    cluster.ref <- hist(cluster.list[six.hour.ref[j]:(six.hour.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    cluster.ref$counts
    if (i == 1) {six_hour_table_1 <- append(six_hour_table_1, cluster.ref$counts)}
    if (i == 2) {six_hour_table_2 <- append(six_hour_table_2, cluster.ref$counts)}
    if (i == 3) {six_hour_table_3 <- append(six_hour_table_3, cluster.ref$counts)}  
    if (i == 4) {six_hour_table_4 <- append(six_hour_table_4, cluster.ref$counts)}
    if (i == 5) {six_hour_table_5 <- append(six_hour_table_5, cluster.ref$counts)}
    if (i == 6) {six_hour_table_6 <- append(six_hour_table_6, cluster.ref$counts)}  
    if (i == 7) {six_hour_table_7 <- append(six_hour_table_7, cluster.ref$counts)}
    if (i == 8) {six_hour_table_8 <- append(six_hour_table_8, cluster.ref$counts)}
    if (i == 9) {six_hour_table_9 <- append(six_hour_table_9, cluster.ref$counts)}
    if (i == 10) {six_hour_table_10 <- append(six_hour_table_10, cluster.ref$counts)}  
    if (i == 11) {six_hour_table_11 <- append(six_hour_table_11, cluster.ref$counts)}
    if (i == 12) {six_hour_table_12 <- append(six_hour_table_12, cluster.ref$counts)}
    l <- length(cluster.ref$counts)
  }
  if (i == 1) {
    l <- 5 # number of clusters 
    six_hour_table_1 <- data.frame(six_hour_table_1[1:(n*l)],
                                    six_hour_table_1[(n*l+1)   :(2*n*l)],
                                    six_hour_table_1[(2*n*l+1) :(3*n*l)],
                                    six_hour_table_1[(3*n*l+1) :(4*n*l)],
                                    six_hour_table_1[(4*n*l+1) :(5*n*l)],
                                    six_hour_table_1[(5*n*l+1) :(6*n*l)],
                                    six_hour_table_1[(6*n*l+1) :(7*n*l)],
                                    six_hour_table_1[(7*n*l+1) :(8*n*l)],
                                    six_hour_table_1[(8*n*l+1) :(9*n*l)],
                                    six_hour_table_1[(9*n*l+1) :(10*n*l)],
                                    six_hour_table_1[(10*n*l+1) :(11*n*l)],
                                    six_hour_table_1[(11*n*l+1) :(12*n*l)])
    six_hour_table_1 <- t(six_hour_table_1)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_1) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_1 <- cbind(six_hour_table_1,site,as.character(dates2))
    write.csv(six_hour_table_1, "hclust_average_k5_6hour.csv", 
              row.names = F)
  }
  if (i == 2) {
    l <- 10
    six_hour_table_2 <- data.frame(six_hour_table_2[1:(n*l)],
                                    six_hour_table_2[(n*l+1)   :(2*n*l)],
                                    six_hour_table_2[(2*n*l+1) :(3*n*l)],
                                    six_hour_table_2[(3*n*l+1) :(4*n*l)],
                                    six_hour_table_2[(4*n*l+1) :(5*n*l)],
                                    six_hour_table_2[(5*n*l+1) :(6*n*l)],
                                    six_hour_table_2[(6*n*l+1) :(7*n*l)],
                                    six_hour_table_2[(7*n*l+1) :(8*n*l)],
                                    six_hour_table_2[(8*n*l+1) :(9*n*l)],
                                    six_hour_table_2[(9*n*l+1) :(10*n*l)],
                                    six_hour_table_2[(10*n*l+1) :(11*n*l)],
                                    six_hour_table_2[(11*n*l+1) :(12*n*l)])
    six_hour_table_2 <- t(six_hour_table_2)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_2) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_2 <- cbind(six_hour_table_2,site,as.character(dates2))
    write.csv(six_hour_table_2, "hclust_average_k10_6hour.csv", 
              row.names = F)
  }
  if (i == 3) {
    l <- 15
    six_hour_table_3 <- data.frame(six_hour_table_3[1:(n*l)],
                                    six_hour_table_3[(n*l+1)   :(2*n*l)],
                                    six_hour_table_3[(2*n*l+1) :(3*n*l)],
                                    six_hour_table_3[(3*n*l+1) :(4*n*l)],
                                    six_hour_table_3[(4*n*l+1) :(5*n*l)],
                                    six_hour_table_3[(5*n*l+1) :(6*n*l)],
                                    six_hour_table_3[(6*n*l+1) :(7*n*l)],
                                    six_hour_table_3[(7*n*l+1) :(8*n*l)],
                                    six_hour_table_3[(8*n*l+1) :(9*n*l)],
                                    six_hour_table_3[(9*n*l+1) :(10*n*l)],
                                    six_hour_table_3[(10*n*l+1) :(11*n*l)],
                                    six_hour_table_3[(11*n*l+1) :(12*n*l)])
    six_hour_table_3 <- t(six_hour_table_3)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_3) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_3 <- cbind(six_hour_table_3,site,as.character(dates2))
    write.csv(six_hour_table_3, "hclust_average_k15_6hour.csv", 
              row.names = F)
  }
  if (i == 4) {
    l <- 20
    six_hour_table_4 <- data.frame(six_hour_table_4[1:(n*l)],
                                    six_hour_table_4[(n*l+1)   :(2*n*l)],
                                    six_hour_table_4[(2*n*l+1) :(3*n*l)],
                                    six_hour_table_4[(3*n*l+1) :(4*n*l)],
                                    six_hour_table_4[(4*n*l+1) :(5*n*l)],
                                    six_hour_table_4[(5*n*l+1) :(6*n*l)],
                                    six_hour_table_4[(6*n*l+1) :(7*n*l)],
                                    six_hour_table_4[(7*n*l+1) :(8*n*l)],
                                    six_hour_table_4[(8*n*l+1) :(9*n*l)],
                                    six_hour_table_4[(9*n*l+1) :(10*n*l)],
                                    six_hour_table_4[(10*n*l+1) :(11*n*l)],
                                    six_hour_table_4[(11*n*l+1) :(12*n*l)])
    six_hour_table_4 <- t(six_hour_table_4)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_4) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_4 <- cbind(six_hour_table_4,site,as.character(dates2))
    write.csv(six_hour_table_4, "hclust_average_k20_6hour.csv", 
              row.names = F)
  }
  if (i == 5) {
    l <- 25
    six_hour_table_5 <- data.frame(six_hour_table_5[1:(n*l)],
                                    six_hour_table_5[(n*l+1)   :(2*n*l)],
                                    six_hour_table_5[(2*n*l+1) :(3*n*l)],
                                    six_hour_table_5[(3*n*l+1) :(4*n*l)],
                                    six_hour_table_5[(4*n*l+1) :(5*n*l)],
                                    six_hour_table_5[(5*n*l+1) :(6*n*l)],
                                    six_hour_table_5[(6*n*l+1) :(7*n*l)],
                                    six_hour_table_5[(7*n*l+1) :(8*n*l)],
                                    six_hour_table_5[(8*n*l+1) :(9*n*l)],
                                    six_hour_table_5[(9*n*l+1) :(10*n*l)],
                                    six_hour_table_5[(10*n*l+1) :(11*n*l)],
                                    six_hour_table_5[(11*n*l+1) :(12*n*l)])
    six_hour_table_5 <- t(six_hour_table_5)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_5) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_5 <- cbind(six_hour_table_5,site,as.character(dates2))
    write.csv(six_hour_table_5, "hclust_average_k25_6hour.csv", 
              row.names = F)
  }
  if (i == 6) {
    l <- 30
    six_hour_table_6 <- data.frame(six_hour_table_6[1:(n*l)],
                                    six_hour_table_6[(n*l+1)   :(2*n*l)],
                                    six_hour_table_6[(2*n*l+1) :(3*n*l)],
                                    six_hour_table_6[(3*n*l+1) :(4*n*l)],
                                    six_hour_table_6[(4*n*l+1) :(5*n*l)],
                                    six_hour_table_6[(5*n*l+1) :(6*n*l)],
                                    six_hour_table_6[(6*n*l+1) :(7*n*l)],
                                    six_hour_table_6[(7*n*l+1) :(8*n*l)],
                                    six_hour_table_6[(8*n*l+1) :(9*n*l)],
                                    six_hour_table_6[(9*n*l+1) :(10*n*l)],
                                    six_hour_table_6[(10*n*l+1) :(11*n*l)],
                                    six_hour_table_6[(11*n*l+1) :(12*n*l)])
    six_hour_table_6 <- t(six_hour_table_6)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_6) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_6 <- cbind(six_hour_table_6,site,as.character(dates2))
    write.csv(six_hour_table_6, "hclust_average_k30_6hour.csv", 
              row.names = F)
  }
  if (i == 7) {
    l <- 5
    six_hour_table_7 <- data.frame(six_hour_table_7[1:(n*l)],
                                    six_hour_table_7[(n*l+1)   :(2*n*l)],
                                    six_hour_table_7[(2*n*l+1) :(3*n*l)],
                                    six_hour_table_7[(3*n*l+1) :(4*n*l)],
                                    six_hour_table_7[(4*n*l+1) :(5*n*l)],
                                    six_hour_table_7[(5*n*l+1) :(6*n*l)],
                                    six_hour_table_7[(6*n*l+1) :(7*n*l)],
                                    six_hour_table_7[(7*n*l+1) :(8*n*l)],
                                    six_hour_table_7[(8*n*l+1) :(9*n*l)],
                                    six_hour_table_7[(9*n*l+1) :(10*n*l)],
                                    six_hour_table_7[(10*n*l+1) :(11*n*l)],
                                    six_hour_table_7[(11*n*l+1) :(12*n*l)])
    six_hour_table_7 <- t(six_hour_table_7)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_7) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_7 <- cbind(six_hour_table_7,site,as.character(dates2))
    write.csv(six_hour_table_7, "hclust_wardD2_k5_6hour.csv", 
              row.names = F)
  }
  if (i == 8) {
    l <- 10
    six_hour_table_8 <- data.frame(six_hour_table_8[1:(n*l)],
                                    six_hour_table_8[(n*l+1)   :(2*n*l)],
                                    six_hour_table_8[(2*n*l+1) :(3*n*l)],
                                    six_hour_table_8[(3*n*l+1) :(4*n*l)],
                                    six_hour_table_8[(4*n*l+1) :(5*n*l)],
                                    six_hour_table_8[(5*n*l+1) :(6*n*l)],
                                    six_hour_table_8[(6*n*l+1) :(7*n*l)],
                                    six_hour_table_8[(7*n*l+1) :(8*n*l)],
                                    six_hour_table_8[(8*n*l+1) :(9*n*l)],
                                    six_hour_table_8[(9*n*l+1) :(10*n*l)],
                                    six_hour_table_8[(10*n*l+1) :(11*n*l)],
                                    six_hour_table_8[(11*n*l+1) :(12*n*l)])
    six_hour_table_8 <- t(six_hour_table_8)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_8) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_8 <- cbind(six_hour_table_8,site,as.character(dates2))
    write.csv(six_hour_table_8, "hclust_wardD2_k10_6hour.csv", 
              row.names = F)
  }
  if (i == 9) {
    l <- 15
    six_hour_table_9 <- data.frame(six_hour_table_9[1:(n*l)],
                                   six_hour_table_9[(n*l+1)   :(2*n*l)],
                                   six_hour_table_9[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_9[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_9[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_9[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_9[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_9[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_9[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_9[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_9[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_9[(11*n*l+1) :(12*n*l)])
    six_hour_table_9 <- t(six_hour_table_9)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_9) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_9 <- cbind(six_hour_table_9,site,as.character(dates2))
    write.csv(six_hour_table_9, "hclust_wardD2_k15_6hour.csv", 
              row.names = F)
  }
  if (i == 10) {
    l <- 20
    six_hour_table_10 <- data.frame(six_hour_table_10[1:(n*l)],
                                   six_hour_table_10[(n*l+1)   :(2*n*l)],
                                   six_hour_table_10[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_10[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_10[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_10[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_10[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_10[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_10[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_10[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_10[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_10[(11*n*l+1) :(12*n*l)])
    six_hour_table_10 <- t(six_hour_table_10)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_10) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_10 <- cbind(six_hour_table_10,site,as.character(dates2))
    write.csv(six_hour_table_10, "hclust_wardD2_k20_6hour.csv", 
              row.names = F)
  }
  if (i == 11) {
    l <- 25
    six_hour_table_11 <- data.frame(six_hour_table_11[1:(n*l)],
                                   six_hour_table_11[(n*l+1)   :(2*n*l)],
                                   six_hour_table_11[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_11[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_11[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_11[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_11[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_11[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_11[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_11[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_11[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_11[(11*n*l+1) :(12*n*l)])
    six_hour_table_11 <- t(six_hour_table_11)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_11) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_11 <- cbind(six_hour_table_11,site,as.character(dates2))
    write.csv(six_hour_table_11, "hclust_wardD2_k25_6hour.csv", 
              row.names = F)
  }
  if (i == 12) {
    l <- 30
    six_hour_table_12 <- data.frame(six_hour_table_12[1:(n*l)],
                                   six_hour_table_12[(n*l+1)   :(2*n*l)],
                                   six_hour_table_12[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_12[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_12[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_12[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_12[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_12[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_12[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_12[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_12[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_12[(11*n*l+1) :(12*n*l)])
    six_hour_table_12 <- t(six_hour_table_12)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_12) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_12 <- cbind(six_hour_table_12,site,as.character(dates2))
    write.csv(six_hour_table_12, "hclust_wardD2_k30_6hour.csv", 
              row.names = F)
  }
}

####################################################
# Saving the hclust_k 12 hour files
####################################################
twelve_hour_table_1 <- NULL 
twelve_hour_table_2 <- NULL 
twelve_hour_table_3 <- NULL 
twelve_hour_table_4 <- NULL 
twelve_hour_table_5 <- NULL 
twelve_hour_table_6 <- NULL 
twelve_hour_table_7 <- NULL 
twelve_hour_table_8 <- NULL 
twelve_hour_table_9 <- NULL 
twelve_hour_table_10 <- NULL
twelve_hour_table_11 <- NULL
twelve_hour_table_12 <- NULL

n <- 2 # number of periods in 24 hours
#cluster.list <- cluster.lists.hclust.k.exp2
cluster.list <- cluster.lists.hclust.k.exp2
for (i in 1:length(cluster.list)) {
  for (j in 1:(length(twelve.hour.ref)-1)) {
    cluster.ref <- hist(cluster.list[twelve.hour.ref[j]:(twelve.hour.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    cluster.ref$counts
    if (i == 1) {twelve_hour_table_1 <- append(twelve_hour_table_1, cluster.ref$counts)}
    if (i == 2) {twelve_hour_table_2 <- append(twelve_hour_table_2, cluster.ref$counts)}
    if (i == 3) {twelve_hour_table_3 <- append(twelve_hour_table_3, cluster.ref$counts)}  
    if (i == 4) {twelve_hour_table_4 <- append(twelve_hour_table_4, cluster.ref$counts)}
    if (i == 5) {twelve_hour_table_5 <- append(twelve_hour_table_5, cluster.ref$counts)}
    if (i == 6) {twelve_hour_table_6 <- append(twelve_hour_table_6, cluster.ref$counts)}  
    if (i == 7) {twelve_hour_table_7 <- append(twelve_hour_table_7, cluster.ref$counts)}
    if (i == 8) {twelve_hour_table_8 <- append(twelve_hour_table_8, cluster.ref$counts)}
    if (i == 9) {twelve_hour_table_9 <- append(twelve_hour_table_9, cluster.ref$counts)}
    if (i == 10) {twelve_hour_table_10 <- append(twelve_hour_table_10, cluster.ref$counts)}  
    if (i == 11) {twelve_hour_table_11 <- append(twelve_hour_table_11, cluster.ref$counts)}
    if (i == 12) {twelve_hour_table_12 <- append(twelve_hour_table_12, cluster.ref$counts)}
    l <- length(cluster.ref$counts)
  }
  if (i == 1) {
    l <- 5 # number of clusters 
    twelve_hour_table_1 <- data.frame(twelve_hour_table_1[1:(n*l)],
                                   twelve_hour_table_1[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_1[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_1[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_1[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_1[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_1[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_1[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_1[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_1[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_1[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_1[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_1 <- t(twelve_hour_table_1)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_1) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_1 <- cbind(twelve_hour_table_1,site,as.character(dates2))
    write.csv(twelve_hour_table_1, "hclust_average_k5_12hour.csv", 
              row.names = F)
  }
  if (i == 2) {
    l <- 10
    twelve_hour_table_2 <- data.frame(twelve_hour_table_2[1:(n*l)],
                                   twelve_hour_table_2[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_2[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_2[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_2[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_2[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_2[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_2[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_2[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_2[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_2[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_2[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_2 <- t(twelve_hour_table_2)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_2) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_2 <- cbind(twelve_hour_table_2,site,as.character(dates2))
    write.csv(twelve_hour_table_2, "hclust_average_k10_12hour.csv", 
              row.names = F)
  }
  if (i == 3) {
    l <- 15
    twelve_hour_table_3 <- data.frame(twelve_hour_table_3[1:(n*l)],
                                   twelve_hour_table_3[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_3[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_3[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_3[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_3[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_3[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_3[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_3[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_3[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_3[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_3[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_3 <- t(twelve_hour_table_3)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_3) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_3 <- cbind(twelve_hour_table_3,site,as.character(dates2))
    write.csv(twelve_hour_table_3, "hclust_average_k15_12hour.csv", 
              row.names = F)
  }
  if (i == 4) {
    l <- 20
    twelve_hour_table_4 <- data.frame(twelve_hour_table_4[1:(n*l)],
                                   twelve_hour_table_4[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_4[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_4[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_4[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_4[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_4[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_4[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_4[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_4[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_4[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_4[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_4 <- t(twelve_hour_table_4)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_4) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_4 <- cbind(twelve_hour_table_4,site,as.character(dates2))
    write.csv(twelve_hour_table_4, "hclust_average_k20_12hour.csv", 
              row.names = F)
  }
  if (i == 5) {
    l <- 25
    twelve_hour_table_5 <- data.frame(twelve_hour_table_5[1:(n*l)],
                                   twelve_hour_table_5[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_5[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_5[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_5[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_5[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_5[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_5[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_5[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_5[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_5[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_5[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_5 <- t(twelve_hour_table_5)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_5) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_5 <- cbind(twelve_hour_table_5,site,as.character(dates2))
    write.csv(twelve_hour_table_5, "hclust_average_k25_12hour.csv", 
              row.names = F)
  }
  if (i == 6) {
    l <- 30
    twelve_hour_table_6 <- data.frame(twelve_hour_table_6[1:(n*l)],
                                   twelve_hour_table_6[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_6[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_6[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_6[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_6[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_6[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_6[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_6[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_6[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_6[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_6[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_6 <- t(twelve_hour_table_6)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_6) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_6 <- cbind(twelve_hour_table_6,site,as.character(dates2))
    write.csv(twelve_hour_table_6, "hclust_average_k30_12hour.csv", 
              row.names = F)
  }
  if (i == 7) {
    l <- 5
    twelve_hour_table_7 <- data.frame(twelve_hour_table_7[1:(n*l)],
                                   twelve_hour_table_7[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_7[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_7[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_7[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_7[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_7[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_7[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_7[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_7[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_7[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_7[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_7 <- t(twelve_hour_table_7)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_7) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_7 <- cbind(twelve_hour_table_7,site,as.character(dates2))
    write.csv(twelve_hour_table_7, "hclust_wardD2_k5_12hour.csv", 
              row.names = F)
  }
  if (i == 8) {
    l <- 10
    twelve_hour_table_8 <- data.frame(twelve_hour_table_8[1:(n*l)],
                                   twelve_hour_table_8[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_8[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_8[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_8[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_8[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_8[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_8[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_8[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_8[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_8[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_8[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_8 <- t(twelve_hour_table_8)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_8) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_8 <- cbind(twelve_hour_table_8,site,as.character(dates2))
    write.csv(twelve_hour_table_8, "hclust_wardD2_k10_12hour.csv", 
              row.names = F)
  }
  if (i == 9) {
    l <- 15
    twelve_hour_table_9 <- data.frame(twelve_hour_table_9[1:(n*l)],
                                   twelve_hour_table_9[(n*l+1)   :(2*n*l)],
                                   twelve_hour_table_9[(2*n*l+1) :(3*n*l)],
                                   twelve_hour_table_9[(3*n*l+1) :(4*n*l)],
                                   twelve_hour_table_9[(4*n*l+1) :(5*n*l)],
                                   twelve_hour_table_9[(5*n*l+1) :(6*n*l)],
                                   twelve_hour_table_9[(6*n*l+1) :(7*n*l)],
                                   twelve_hour_table_9[(7*n*l+1) :(8*n*l)],
                                   twelve_hour_table_9[(8*n*l+1) :(9*n*l)],
                                   twelve_hour_table_9[(9*n*l+1) :(10*n*l)],
                                   twelve_hour_table_9[(10*n*l+1) :(11*n*l)],
                                   twelve_hour_table_9[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_9 <- t(twelve_hour_table_9)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_9) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_9 <- cbind(twelve_hour_table_9,site,as.character(dates2))
    write.csv(twelve_hour_table_9, "hclust_wardD2_k15_12hour.csv", 
              row.names = F)
  }
  if (i == 10) {
    l <- 20
    twelve_hour_table_10 <- data.frame(twelve_hour_table_10[1:(n*l)],
                                    twelve_hour_table_10[(n*l+1)   :(2*n*l)],
                                    twelve_hour_table_10[(2*n*l+1) :(3*n*l)],
                                    twelve_hour_table_10[(3*n*l+1) :(4*n*l)],
                                    twelve_hour_table_10[(4*n*l+1) :(5*n*l)],
                                    twelve_hour_table_10[(5*n*l+1) :(6*n*l)],
                                    twelve_hour_table_10[(6*n*l+1) :(7*n*l)],
                                    twelve_hour_table_10[(7*n*l+1) :(8*n*l)],
                                    twelve_hour_table_10[(8*n*l+1) :(9*n*l)],
                                    twelve_hour_table_10[(9*n*l+1) :(10*n*l)],
                                    twelve_hour_table_10[(10*n*l+1) :(11*n*l)],
                                    twelve_hour_table_10[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_10 <- t(twelve_hour_table_10)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_10) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_10 <- cbind(twelve_hour_table_10,site,as.character(dates2))
    write.csv(twelve_hour_table_10, "hclust_wardD2_k20_12hour.csv", 
              row.names = F)
  }
  if (i == 11) {
    l <- 25
    twelve_hour_table_11 <- data.frame(twelve_hour_table_11[1:(n*l)],
                                    twelve_hour_table_11[(n*l+1)   :(2*n*l)],
                                    twelve_hour_table_11[(2*n*l+1) :(3*n*l)],
                                    twelve_hour_table_11[(3*n*l+1) :(4*n*l)],
                                    twelve_hour_table_11[(4*n*l+1) :(5*n*l)],
                                    twelve_hour_table_11[(5*n*l+1) :(6*n*l)],
                                    twelve_hour_table_11[(6*n*l+1) :(7*n*l)],
                                    twelve_hour_table_11[(7*n*l+1) :(8*n*l)],
                                    twelve_hour_table_11[(8*n*l+1) :(9*n*l)],
                                    twelve_hour_table_11[(9*n*l+1) :(10*n*l)],
                                    twelve_hour_table_11[(10*n*l+1) :(11*n*l)],
                                    twelve_hour_table_11[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_11 <- t(twelve_hour_table_11)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_11) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_11 <- cbind(twelve_hour_table_11,site,as.character(dates2))
    write.csv(twelve_hour_table_11, "hclust_wardD2_k25_12hour.csv", 
              row.names = F)
  }
  if (i == 12) {
    l <- 30
    twelve_hour_table_12 <- data.frame(twelve_hour_table_12[1:(n*l)],
                                    twelve_hour_table_12[(n*l+1)   :(2*n*l)],
                                    twelve_hour_table_12[(2*n*l+1) :(3*n*l)],
                                    twelve_hour_table_12[(3*n*l+1) :(4*n*l)],
                                    twelve_hour_table_12[(4*n*l+1) :(5*n*l)],
                                    twelve_hour_table_12[(5*n*l+1) :(6*n*l)],
                                    twelve_hour_table_12[(6*n*l+1) :(7*n*l)],
                                    twelve_hour_table_12[(7*n*l+1) :(8*n*l)],
                                    twelve_hour_table_12[(8*n*l+1) :(9*n*l)],
                                    twelve_hour_table_12[(9*n*l+1) :(10*n*l)],
                                    twelve_hour_table_12[(10*n*l+1) :(11*n*l)],
                                    twelve_hour_table_12[(11*n*l+1) :(12*n*l)])
    twelve_hour_table_12 <- t(twelve_hour_table_12)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(twelve_hour_table_12) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    twelve_hour_table_12 <- cbind(twelve_hour_table_12,site,as.character(dates2))
    write.csv(twelve_hour_table_12, "hclust_wardD2_k30_12hour.csv", 
              row.names = F)
  }
}
# now go to "myfiles" in folder k.R file
####################################################
# Saving the mclust 24 hour file
####################################################
twentyfour_hour_table <- NULL #read.csv(text="col1,col2")

cluster.list <- cluster.lists.mclust.exp2
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(day.ref)-1)) {
    cluster.ref <- hist(cluster.list[day.ref[j]:(day.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    print(length(cluster.ref$counts))
    if (i == 1) {twentyfour_hour_table <- rbind(twentyfour_hour_table, 
                                                  cluster.ref$counts)}
  }
}
twentyfour_hour_table <- as.data.frame(twentyfour_hour_table)
# Rename the columns
column.names <- NULL
for (k in 1:(length(twentyfour_hour_table))) {
  col.names <- paste("clus_", k, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(twentyfour_hour_table) <- column.names

site <- c(rep("GympieNP",6), rep("WoondumNP",6))
dates2 <- rep(dates,2)
twentyfour_hour_table <- cbind(twentyfour_hour_table,site,as.character(dates2))

write.csv(twentyfour_hour_table, "mclust_k39_24hour.csv", 
          row.names = F)

####################################################
# Saving the Hybrid 24 hour files
####################################################
twentyfour_hour_table_1 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_2 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_3 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_4 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_5 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_6 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_7 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_8 <- NULL #read.csv(text="col1,col2")

cluster.list <- cluster.list.hybrid.exp2
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(day.ref)-1)) {
    cluster.ref <- hist(cluster.list[day.ref[j]:(day.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    print(length(cluster.ref$counts))
    
    if (i == 1) {twentyfour_hour_table_1 <- rbind(twentyfour_hour_table_1, cluster.ref$counts)}
    if (i == 2) {twentyfour_hour_table_2 <- rbind(twentyfour_hour_table_2, cluster.ref$counts)}
    if (i == 3) {twentyfour_hour_table_3 <- rbind(twentyfour_hour_table_3, cluster.ref$counts)}  
    if (i == 4) {twentyfour_hour_table_4 <- rbind(twentyfour_hour_table_4, cluster.ref$counts)}
    if (i == 5) {twentyfour_hour_table_5 <- rbind(twentyfour_hour_table_5, cluster.ref$counts)}
    if (i == 6) {twentyfour_hour_table_6 <- rbind(twentyfour_hour_table_6, cluster.ref$counts)}  
    if (i == 7) {twentyfour_hour_table_7 <- rbind(twentyfour_hour_table_7, cluster.ref$counts)}
    if (i == 8) {twentyfour_hour_table_8 <- rbind(twentyfour_hour_table_8, cluster.ref$counts)}
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

site <- c(rep("GympieNP",6), rep("WoondumNP",6))

twentyfour_hour_table_1 <- cbind(twentyfour_hour_table_1,site,as.character(dates2))
twentyfour_hour_table_2 <- cbind(twentyfour_hour_table_2,site,as.character(dates2))
twentyfour_hour_table_3 <- cbind(twentyfour_hour_table_3,site,as.character(dates2))
twentyfour_hour_table_4 <- cbind(twentyfour_hour_table_4,site,as.character(dates2))
twentyfour_hour_table_5 <- cbind(twentyfour_hour_table_5,site,as.character(dates2))
twentyfour_hour_table_6 <- cbind(twentyfour_hour_table_6,site,as.character(dates2))
twentyfour_hour_table_7 <- cbind(twentyfour_hour_table_7,site,as.character(dates2))
twentyfour_hour_table_8 <- cbind(twentyfour_hour_table_8,site,as.character(dates2))

write.csv(twentyfour_hour_table_1, paste("hybrid_k1000_k",z,"_24hour.csv",sep=""), 
          row.names = F)
write.csv(twentyfour_hour_table_2, paste("hybrid_k1500_k",z,"_24hour.csv",sep=""),
          row.names = F)
write.csv(twentyfour_hour_table_3, paste("hybrid_k2000_k",z,"_24hour.csv",sep=""),
          row.names = F)
write.csv(twentyfour_hour_table_4, paste("hybrid_k2500_k",z,"_24hour.csv",sep=""),
          row.names = F)
write.csv(twentyfour_hour_table_5, paste("hybrid_k3000_k",z,"_24hour.csv",sep=""),
          row.names = F)
write.csv(twentyfour_hour_table_6, paste("hybrid_k3500_k",z,"_24hour.csv",sep=""),
          row.names = F)
write.csv(twentyfour_hour_table_7, paste("hybrid_k4000_k",z,"_24hour.csv",sep=""),
          row.names = F)
write.csv(twentyfour_hour_table_8, paste("hybrid_k4500_k",z,"_24hour.csv",sep=""),
          row.names = F)
# now go to "myfiles" in folder k.R code
####################################################
# Saving the Hybrid four (4) hour files  
####################################################
four_hour_table_1 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
four_hour_table_2 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
four_hour_table_3 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
four_hour_table_4 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
four_hour_table_5 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
four_hour_table_6 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
four_hour_table_7 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
four_hour_table_8 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))

n <- 6
cluster.list <- cluster.list.hybrid.exp2
for (i in 1:length(cluster.list)) {
  for (j in 1:(length(four.hour.ref)-1)) {
    cluster.ref <- hist(cluster.list[four.hour.ref[j]:(four.hour.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    cluster.ref$counts
    if (i == 1) {four_hour_table_1 <- append(four_hour_table_1, cluster.ref$counts)}
    if (i == 2) {four_hour_table_2 <- append(four_hour_table_2, cluster.ref$counts)}
    if (i == 3) {four_hour_table_3 <- append(four_hour_table_3, cluster.ref$counts)}  
    if (i == 4) {four_hour_table_4 <- append(four_hour_table_4, cluster.ref$counts)}
    if (i == 5) {four_hour_table_5 <- append(four_hour_table_5, cluster.ref$counts)}
    if (i == 6) {four_hour_table_6 <- append(four_hour_table_6, cluster.ref$counts)}  
    if (i == 7) {four_hour_table_7 <- append(four_hour_table_7, cluster.ref$counts)}
    if (i == 8) {four_hour_table_8 <- append(four_hour_table_8, cluster.ref$counts)}
    l <- length(cluster.ref$counts)
  }
  if (i == 1) {
    l <- z
    four_hour_table_1 <- data.frame(four_hour_table_1[1:(n*l)],
                                    four_hour_table_1[(n*l+1)   :(2*n*l)],
                                    four_hour_table_1[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_1[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_1[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_1[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_1[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_1[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_1[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_1[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_1[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_1[(11*n*l+1) :(12*n*l)])
    four_hour_table_1 <- t(four_hour_table_1)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_1) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_1 <- cbind(four_hour_table_1,site,as.character(dates2))
    write.csv(four_hour_table_1, paste("hybrid_k1000_k",z,"_4hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 2) {
    l <- z
    four_hour_table_2 <- data.frame(four_hour_table_2[1:(n*l)],
                                    four_hour_table_2[(n*l+1)   :(2*n*l)],
                                    four_hour_table_2[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_2[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_2[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_2[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_2[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_2[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_2[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_2[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_2[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_2[(11*n*l+1) :(12*n*l)])
    four_hour_table_2 <- t(four_hour_table_2)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_2) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_2 <- cbind(four_hour_table_2,site,as.character(dates2))
    write.csv(four_hour_table_2, paste("hybrid_k1500_k",z,"_4hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 3) {
    l <- z
    four_hour_table_3 <- data.frame(four_hour_table_3[1:(n*l)],
                                    four_hour_table_3[(n*l+1)   :(2*n*l)],
                                    four_hour_table_3[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_3[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_3[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_3[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_3[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_3[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_3[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_3[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_3[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_3[(11*n*l+1) :(12*n*l)])
    four_hour_table_3 <- t(four_hour_table_3)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_3) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_3 <- cbind(four_hour_table_3,site,as.character(dates2))
    write.csv(four_hour_table_3, paste("hybrid_k2000_k",z,"_4hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 4) {
    l <- z
    four_hour_table_4 <- data.frame(four_hour_table_4[1:(n*l)],
                                    four_hour_table_4[(n*l+1)   :(2*n*l)],
                                    four_hour_table_4[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_4[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_4[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_4[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_4[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_4[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_4[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_4[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_4[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_4[(11*n*l+1) :(12*n*l)])
    four_hour_table_4 <- t(four_hour_table_4)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_4) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_4 <- cbind(four_hour_table_4,site,as.character(dates2))
    write.csv(four_hour_table_4, paste("hybrid_k2500_k",z,"_4hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 5) {
    l <- z
    four_hour_table_5 <- data.frame(four_hour_table_5[1:(n*l)],
                                    four_hour_table_5[(n*l+1)   :(2*n*l)],
                                    four_hour_table_5[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_5[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_5[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_5[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_5[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_5[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_5[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_5[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_5[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_5[(11*n*l+1) :(12*n*l)])
    four_hour_table_5 <- t(four_hour_table_5)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_5) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_5 <- cbind(four_hour_table_5,site,as.character(dates2))
    write.csv(four_hour_table_5, paste("hybrid_k3000_k",z,"_4hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 6) {
    l <- z
    four_hour_table_6 <- data.frame(four_hour_table_6[1:(n*l)],
                                    four_hour_table_6[(n*l+1)   :(2*n*l)],
                                    four_hour_table_6[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_6[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_6[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_6[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_6[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_6[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_6[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_6[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_6[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_6[(11*n*l+1) :(12*n*l)])
    four_hour_table_6 <- t(four_hour_table_6)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_6) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_6 <- cbind(four_hour_table_6,site,as.character(dates2))
    write.csv(four_hour_table_6, paste("hybrid_k3500_k",z,"_4hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 7) {
    l <- z
    four_hour_table_7 <- data.frame(four_hour_table_7[1:(n*l)],
                                    four_hour_table_7[(n*l+1)   :(2*n*l)],
                                    four_hour_table_7[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_7[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_7[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_7[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_7[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_7[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_7[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_7[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_7[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_7[(11*n*l+1) :(12*n*l)])
    four_hour_table_7 <- t(four_hour_table_7)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_7) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_7 <- cbind(four_hour_table_7,site,as.character(dates2))
    write.csv(four_hour_table_7, paste("hybrid_k4000_k",z,"_4hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 8) {
    l <- z
    four_hour_table_8 <- data.frame(four_hour_table_8[1:(n*l)],
                                    four_hour_table_8[(n*l+1)   :(2*n*l)],
                                    four_hour_table_8[(2*n*l+1) :(3*n*l)],
                                    four_hour_table_8[(3*n*l+1) :(4*n*l)],
                                    four_hour_table_8[(4*n*l+1) :(5*n*l)],
                                    four_hour_table_8[(5*n*l+1) :(6*n*l)],
                                    four_hour_table_8[(6*n*l+1) :(7*n*l)],
                                    four_hour_table_8[(7*n*l+1) :(8*n*l)],
                                    four_hour_table_8[(8*n*l+1) :(9*n*l)],
                                    four_hour_table_8[(9*n*l+1) :(10*n*l)],
                                    four_hour_table_8[(10*n*l+1) :(11*n*l)],
                                    four_hour_table_8[(11*n*l+1) :(12*n*l)])
    four_hour_table_8 <- t(four_hour_table_8)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(four_hour_table_8) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    four_hour_table_8 <- cbind(four_hour_table_8,site,as.character(dates2))
    write.csv(four_hour_table_8, paste("hybrid_k4500_k",z,"_4hour.csv",sep=""), 
              row.names = F)
  }
}

####################################################
# Saving the hybrid six (6) hour files
####################################################
six_hour_table_1 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
six_hour_table_2 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
six_hour_table_3 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
six_hour_table_4 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
six_hour_table_5 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
six_hour_table_6 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
six_hour_table_7 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))
six_hour_table_8 <- NULL #as.data.frame(matrix(0, ncol = 120, nrow = 12))

n <- 4 # number of periods in 24 hours

cluster.list <- cluster.list.hybrid.exp2
for (i in 1:length(cluster.list)) {
  for (j in 1:(length(six.hour.ref)-1)) {
    cluster.ref <- hist(cluster.list[six.hour.ref[j]:(six.hour.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    cluster.ref$counts
    if (i == 1) {six_hour_table_1 <- append(six_hour_table_1, cluster.ref$counts)}
    if (i == 2) {six_hour_table_2 <- append(six_hour_table_2, cluster.ref$counts)}
    if (i == 3) {six_hour_table_3 <- append(six_hour_table_3, cluster.ref$counts)}  
    if (i == 4) {six_hour_table_4 <- append(six_hour_table_4, cluster.ref$counts)}
    if (i == 5) {six_hour_table_5 <- append(six_hour_table_5, cluster.ref$counts)}
    if (i == 6) {six_hour_table_6 <- append(six_hour_table_6, cluster.ref$counts)}  
    if (i == 7) {six_hour_table_7 <- append(six_hour_table_7, cluster.ref$counts)}
    if (i == 8) {six_hour_table_8 <- append(six_hour_table_8, cluster.ref$counts)}
    l <- length(cluster.ref$counts)
  }
  if (i == 1) {
    l <- z
    six_hour_table_1 <- data.frame(six_hour_table_1[1:(n*l)],
                                   six_hour_table_1[(n*l+1)   :(2*n*l)],
                                   six_hour_table_1[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_1[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_1[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_1[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_1[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_1[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_1[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_1[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_1[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_1[(11*n*l+1) :(12*n*l)])
    six_hour_table_1 <- t(six_hour_table_1)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_1) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_1 <- cbind(six_hour_table_1,site,as.character(dates2))
    write.csv(six_hour_table_1, paste("hybrid_k1000_k",z,"_6hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 2) {
    l <- z
    six_hour_table_2 <- data.frame(six_hour_table_2[1:(n*l)],
                                   six_hour_table_2[(n*l+1)   :(2*n*l)],
                                   six_hour_table_2[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_2[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_2[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_2[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_2[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_2[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_2[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_2[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_2[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_2[(11*n*l+1) :(12*n*l)])
    six_hour_table_2 <- t(six_hour_table_2)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_2) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_2 <- cbind(six_hour_table_2,site,as.character(dates2))
    write.csv(six_hour_table_2, paste("hybrid_k1500_k",z,"_6hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 3) {
    l <- z
    six_hour_table_3 <- data.frame(six_hour_table_3[1:(n*l)],
                                   six_hour_table_3[(n*l+1)   :(2*n*l)],
                                   six_hour_table_3[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_3[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_3[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_3[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_3[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_3[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_3[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_3[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_3[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_3[(11*n*l+1) :(12*n*l)])
    six_hour_table_3 <- t(six_hour_table_3)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_3) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_3 <- cbind(six_hour_table_3,site,as.character(dates2))
    write.csv(six_hour_table_3, paste("hybrid_k2000_k",z,"_6hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 4) {
    l <- z
    six_hour_table_4 <- data.frame(six_hour_table_4[1:(n*l)],
                                   six_hour_table_4[(n*l+1)   :(2*n*l)],
                                   six_hour_table_4[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_4[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_4[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_4[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_4[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_4[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_4[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_4[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_4[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_4[(11*n*l+1) :(12*n*l)])
    six_hour_table_4 <- t(six_hour_table_4)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_4) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_4 <- cbind(six_hour_table_4,site,as.character(dates2))
    write.csv(six_hour_table_4, paste("hybrid_k2500_k",z,"_6hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 5) {
    l <- z
    six_hour_table_5 <- data.frame(six_hour_table_5[1:(n*l)],
                                   six_hour_table_5[(n*l+1)   :(2*n*l)],
                                   six_hour_table_5[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_5[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_5[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_5[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_5[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_5[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_5[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_5[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_5[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_5[(11*n*l+1) :(12*n*l)])
    six_hour_table_5 <- t(six_hour_table_5)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_5) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_5 <- cbind(six_hour_table_5,site,as.character(dates2))
    write.csv(six_hour_table_5, paste("hybrid_k3000_k",z,"_6hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 6) {
    l <- z
    six_hour_table_6 <- data.frame(six_hour_table_6[1:(n*l)],
                                   six_hour_table_6[(n*l+1)   :(2*n*l)],
                                   six_hour_table_6[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_6[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_6[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_6[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_6[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_6[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_6[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_6[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_6[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_6[(11*n*l+1) :(12*n*l)])
    six_hour_table_6 <- t(six_hour_table_6)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_6) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_6 <- cbind(six_hour_table_6,site,as.character(dates2))
    write.csv(six_hour_table_6, paste("hybrid_k3500_k",z,"_6hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 7) {
    l <- z
    six_hour_table_7 <- data.frame(six_hour_table_7[1:(n*l)],
                                   six_hour_table_7[(n*l+1)   :(2*n*l)],
                                   six_hour_table_7[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_7[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_7[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_7[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_7[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_7[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_7[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_7[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_7[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_7[(11*n*l+1) :(12*n*l)])
    six_hour_table_7 <- t(six_hour_table_7)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_7) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_7 <- cbind(six_hour_table_7,site,as.character(dates2))
    write.csv(six_hour_table_7, paste("hybrid_k4000_k",z,"_6hour.csv",sep=""), 
              row.names = F)
  }
  if (i == 8) {
    l <- z
    six_hour_table_8 <- data.frame(six_hour_table_8[1:(n*l)],
                                   six_hour_table_8[(n*l+1)   :(2*n*l)],
                                   six_hour_table_8[(2*n*l+1) :(3*n*l)],
                                   six_hour_table_8[(3*n*l+1) :(4*n*l)],
                                   six_hour_table_8[(4*n*l+1) :(5*n*l)],
                                   six_hour_table_8[(5*n*l+1) :(6*n*l)],
                                   six_hour_table_8[(6*n*l+1) :(7*n*l)],
                                   six_hour_table_8[(7*n*l+1) :(8*n*l)],
                                   six_hour_table_8[(8*n*l+1) :(9*n*l)],
                                   six_hour_table_8[(9*n*l+1) :(10*n*l)],
                                   six_hour_table_8[(10*n*l+1) :(11*n*l)],
                                   six_hour_table_8[(11*n*l+1) :(12*n*l)])
    six_hour_table_8 <- t(six_hour_table_8)
    column.names <- NULL
    for (k in 1:(l*n)) {
      col.names <- paste("clus_", k, sep = "")
      column.names <- c(column.names,col.names)
    }
    colnames(six_hour_table_8) <- column.names
    site <- c(rep("GympieNP",6), rep("WoondumNP",6))
    six_hour_table_8 <- cbind(six_hour_table_8,site,as.character(dates2))
    write.csv(six_hour_table_8, paste("hybrid_k4500_k",z,"_6hour.csv",sep=""), 
              row.names = F)
  }
}


#############################################################
cluster.list <- cluster.lists.kmeans.exp2$km_clusters_26

i=1
j=4
ht <- hist(cluster.list[(day.ref[j+12*(i-1)]):(day.ref[j+12*(i-1)+1]-1)], 
           main = paste(dates[j+12*(i-1)]), xlab = "Cluster reference",
           ylim=c(0,400), breaks=seq(0.5,26.5,by=1), xlim = c(0,26),
           freq = T)
#hist(cluster.list[day.ref[j+6*(i-1)]:day.ref[j+6*(i-1)+1]-1,], 
#     main = paste(dates[j+6*(i-1)]), xlab = "Cluster reference",
#     ylim=c(0,900), breaks=seq(0.5,30.5,by=1), xlim = c(0,30))
counts <- rbind(counts, ht$counts)

### Ordinary histograms from here
cluster.ref <- unname(table(cluster.list[day.ref[1]:day.ref[2]-1,]))

### Daily clustering
ref <- LETTERS[1:26] ## Letter sequence to be used for histogram titles 
counts <- NULL

for(i in 1:ceiling(length(dates)/12)-1) {  # 12 plots per sheet
  i=2
  mypath <- file.path("C:\\Work\\CSV files\\DataSet_Exp2\\24hourfFiles\\a",
                      paste("histogram_kmeans_", ref[i], ".png", sep = ""))
  i=1
  png(file=mypath,
      width = 200, 
      height = 200, 
      units = "mm",
      res=1200,
      pointsize = 5)
  par(mfcol=c(6,2), mar=c(3,3,1,2), oma=c(3,3,3,3), cex.main=2)
  for(j in 1:11) {
    ht <- hist(cluster.list[(day.ref[j]):(day.ref[j+1]),], 
               main = paste(dates[j]), xlab = "Cluster reference",
               ylim=c(0,400), breaks=seq(0.5,26.5,by=1), xlim = c(0,26),
               freq = T)
    #hist(cluster.list[day.ref[j+6*(i-1)]:day.ref[j+6*(i-1)+1]-1,], 
    #     main = paste(dates[j+6*(i-1)]), xlab = "Cluster reference",
    #     ylim=c(0,900), breaks=seq(0.5,30.5,by=1), xlim = c(0,30))
    counts <- rbind(counts, ht$counts)
  }
  ht <- hist(cluster.list[(day.ref[j+1]):(length(cluster.lists.mclust.exp2$x)),], 
             main = paste(dates[j+1]), xlab = "Cluster reference",
             ylim=c(0,400), breaks=seq(0.5,26.5,by=1), xlim = c(0,26),
             freq = T)
  #hist(cluster.list[day.ref[j+6*(i-1)]:day.ref[j+6*(i-1)+1]-1,], 
  #     main = paste(dates[j+6*(i-1)]), xlab = "Cluster reference",
  #     ylim=c(0,900), breaks=seq(0.5,30.5,by=1), xlim = c(0,30))
  counts <- rbind(counts, ht$counts)
  dev.off() 
} 
dev.off()
counts <- as.data.frame(counts)
View(counts)
counts <- cbind(counts, dates[1:length(dates)-1]) 

write.csv(counts, file="Cluster_dailycount_kmeans.csv", row.names = F)

#################
ref <- LETTERS[1:26] ## Gives a sequence of the letters of the alphabet 
counts <- NULL

for(i in 1:ceiling(length(four.hour.ref)/24)) { 
  mypath <- file.path("C:\\Work\\CSV files\\GympieNP1_new\\kmeans_30clusters",
                      paste("histogram_kmeans_fourhour", ref[i], ".png", sep = ""))
  png(file=mypath,
      width = 200, 
      height = 85, 
      units = "mm",
      res=1200,
      pointsize = 5)
  
  par(mfrow=c(4,6), mar=c(2,2,3,2), oma=c(3,3,3,3))
  for(j in 1:24) {
    ht <- hist(cluster.list[(four.hour.ref[j+24*(i-1)]):(four.hour.ref[j+24*(i-1)+1]-1),], 
               main = paste(dates2[j+24*(i-1)]), xlab = "Cluster reference",
               ylim=c(0,100), breaks=seq(0.5,30.5,by=1), xlim = c(0,30),
               freq = T)
    counts <- rbind(counts, ht$counts)
  }
  dev.off() 
} 
dev.off()
counts <- as.data.frame(counts)

#time.ref <- rep(letters[1:6],ceiling(length(four.hour.ref)/6))
time.ref <- rep(c("red","orange","yellow","green","blue","violet"), ceiling(length(four.hour.ref)/6))
counts <- cbind(counts, time.ref[1:length(counts$V1)], dates2[1:(length(dates2)-4)])
colnames(counts) <- c("C1","C2","C3","C4","C5","C6","C7","C8","C9","C10","C11","C12",
                      "C13","C14","C15","C16","C17","C18","C19","C20","C21","C22",
                      "C23","C24","C25","C26","C27","C28","C29","C30","timeRef","date")
write.csv(counts, file="Cluster_4hourcount_kmeans.csv", row.names = F)
