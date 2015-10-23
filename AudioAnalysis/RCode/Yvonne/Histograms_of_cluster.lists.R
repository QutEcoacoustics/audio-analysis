####################################################################
# 7 September 2015
# Histograms of cluster.lists - daily and four-hourly
#
####################################################################
#setwd("C:\\Work\\CSV files\\GympieNP1_new\\kmeans_30clusters")
setwd("C:\\Work\\CSV files\\DataSet_Exp2\\24hourFilesA\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hybrid\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2_new_new\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2_new_new\\Hybrid\\")
#cluster.list <- read.csv("Cluster_list_kmeans_22June-16July2015_5,7,9,10,11,12,13,17,18_30Gympie NP1 .csv", header = T)
cluster.lists.kmeans.exp2   <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2\\Partitioning - kmeans\\km_fit_set.csv", header = T)
cluster.lists.hclust.k.exp2 <- read.csv("hc_fit_set_cutree_k.csv", header = T)
cluster.lists.hclust.h.exp2 <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2\\Hierarchical\\hc_fit_set_cutree_h.csv", header = T)
cluster.lists.mclust.exp2   <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2\\Model_based\\mclustlist_ds3norm_1_50.csv", header = T)
cluster.list.hybrid.exp2    <- read.csv("hybrid_clust_k20.csv", header = T)

#indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2\\Final DataSet 30_31July_1Aug_31Aug_1_4Sept.csv", 
#                    header=T)
indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2\\Final DataSet 30_31July_1Aug_31Aug_1_4Sept.csv", 
                    header=T)
indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2_new\\Dataset_30_31July_1Aug2015_4_9_10Sept2015.csv", 
                    header=T)
indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2_new_new\\Dataset_30_31July_1Aug2015_9_10_12Sept2015.csv", 
                    header=T)

day.ref <- which(indices$minute.of.day=="0")
four.am.ref <- which(indices$minute.of.day == "240")
eight.am.ref <- which(indices$minute.of.day=="480")
midday.ref <- which(indices$minute.of.day=="720")
four.pm.ref <- which(indices$minute.of.day=="960")
eight.pm.ref <- which(indices$minute.of.day=="1200")
four.hour.ref <- c(day.ref, four.am.ref, eight.am.ref, midday.ref,
                   four.pm.ref, eight.pm.ref)
four.hour.ref <- sort(four.hour.ref)
four.hour.ref <- c(four.hour.ref, length(indices$minute.of.day))
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

cluster.list <- cluster.lists.kmeans.exp2
#cluster.list <- cluster.list.test
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(unique(dates))*2-1)) {
    #cluster.ref <- table(cluster.list[day.ref[j]:day.ref[j+1]-1,i])
    cluster.ref <- hist(cluster.list[day.ref[j]:(day.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    print(length(cluster.ref$counts))
    if (i == 1) {twentyfour_hour_table_1 <- rbind(twentyfour_hour_table_1, cluster.ref$counts)}
    if (i == 2) {twentyfour_hour_table_2 <- rbind(twentyfour_hour_table_2, cluster.ref$counts)}
    if (i == 3) {twentyfour_hour_table_3 <- rbind(twentyfour_hour_table_3, cluster.ref$counts)}  
  }
  cluster.ref <- hist(cluster.list[day.ref[j+1]:length(cluster.lists.kmeans.exp2$km_clusters_8),i], 
                      breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
  print(length(cluster.ref$counts))
  if (i == 1) {twentyfour_hour_table_1 <- rbind(twentyfour_hour_table_1, unname(cluster.ref$counts))}
  if (i == 2) {twentyfour_hour_table_2 <- rbind(twentyfour_hour_table_2, unname(cluster.ref$counts))}
  if (i == 3) {twentyfour_hour_table_3 <- rbind(twentyfour_hour_table_3, unname(cluster.ref$counts))}  
}
twentyfour_hour_table_1 <- as.data.frame(twentyfour_hour_table_1)
twentyfour_hour_table_2 <- as.data.frame(twentyfour_hour_table_2)
twentyfour_hour_table_3 <- as.data.frame(twentyfour_hour_table_3)
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
site <- c(rep("GympieNP",6), rep("WoondumNP",6))

twentyfour_hour_table_1 <- cbind(twentyfour_hour_table_1,site,dates2)
twentyfour_hour_table_2 <- cbind(twentyfour_hour_table_2,site,dates2)
twentyfour_hour_table_3 <- cbind(twentyfour_hour_table_3,site,dates2)

write.csv(twentyfour_hour_table_1, "kmeans_k8_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_2, "kmeans_k16_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_3, "kmeans_k26_24hour.csv", 
          row.names = F)


#####################################################
# Saving the hclust_k 24 hour files
####################################################
twentyfour_hour_table_1 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_2 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_3 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_4 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_5 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_6 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_7 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_8 <- NULL #read.csv(text="col1,col2")

cluster.list <- cluster.lists.hclust.k.exp2
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(dates)*2-1)) {
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
  }
  cluster.ref <- hist(cluster.list[day.ref[j+1]:length(cluster.lists.hclust.k.exp2$hc.fit.average.10),i], 
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

twentyfour_hour_table_1 <- cbind(twentyfour_hour_table_1,site,dates2)
twentyfour_hour_table_2 <- cbind(twentyfour_hour_table_2,site,dates2)
twentyfour_hour_table_3 <- cbind(twentyfour_hour_table_3,site,dates2)
twentyfour_hour_table_4 <- cbind(twentyfour_hour_table_4,site,dates2)
twentyfour_hour_table_5 <- cbind(twentyfour_hour_table_5,site,dates2)
twentyfour_hour_table_6 <- cbind(twentyfour_hour_table_6,site,dates2)
twentyfour_hour_table_7 <- cbind(twentyfour_hour_table_7,site,dates2)
twentyfour_hour_table_8 <- cbind(twentyfour_hour_table_8,site,dates2)

write.csv(twentyfour_hour_table_1, "hclust_average_k10_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_2, "hclust_average_k15_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_3, "hclust_average_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_4, "hclust_average_k25_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_5, "hclust_wardd2_k10_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_6, "hclust_wardd2_k15_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_7, "hclust_wardd2_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_8, "hclust_wardd2_k25_24hour.csv", 
          row.names = F)

####################################################
# Saving the hclust_k 4 hour files
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
    l <- length(cluster.ref$counts)
  }
  if (i == 1) {
    l <- 10
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
    four_hour_table_1 <- cbind(four_hour_table_1,site,dates2)
    write.csv(four_hour_table_1, "hclust_average_k10_4hour.csv", 
              row.names = F)
  }
  if (i == 2) {
    l <- 15
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
    write.csv(four_hour_table_2, "hclust_average_k15_4hour.csv", 
              row.names = F)
  }
  if (i == 3) {
    l <- 20
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
    write.csv(four_hour_table_3, "hclust_average_k20_4hour.csv", 
              row.names = F)
  }
  if (i == 4) {
    l <- 25
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
    write.csv(four_hour_table_4, "hclust_average_k25_4hour.csv", 
              row.names = F)
  }
  if (i == 5) {
    l <- 10
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
    four_hour_table_5 <- cbind(four_hour_table_5,site,dates2)
    write.csv(four_hour_table_5, "hclust_wardD2_k10_4hour.csv", 
              row.names = F)
  }
  if (i == 6) {
    l <- 15
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
    write.csv(four_hour_table_6, "hclust_wardD2_k15_4hour.csv", 
              row.names = F)
  }
  if (i == 7) {
    l <- 20
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
    write.csv(four_hour_table_7, "hclust_wardD2_k20_4hour.csv", 
              row.names = F)
  }
  if (i == 8) {
    l <- 25
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
    write.csv(four_hour_table_8, "hclust_wardD2_k25_4hour.csv", 
              row.names = F)
  }
}


####################################################
# Saving the mclust 24 hour file
####################################################
twentyfour_hour_table <- NULL #read.csv(text="col1,col2")

cluster.list <- cluster.lists.mclust.exp2
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(dates)*2-1)) {
    cluster.ref <- hist(cluster.list[day.ref[j]:(day.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    print(length(cluster.ref$counts))
    if (i == 1) {twentyfour_hour_table <- rbind(twentyfour_hour_table, 
                                                  cluster.ref$counts)}
  }
  cluster.ref <- hist(cluster.list[day.ref[j+1]:length(cluster.lists.kmeans.exp2$km_clusters_8),i], 
                      breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
  print(length(cluster.ref$counts))
  if (i == 1) {twentyfour_hour_table <- rbind(twentyfour_hour_table, cluster.ref$counts)}
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
twentyfour_hour_table <- cbind(twentyfour_hour_table,site,dates2)

write.csv(twentyfour_hour_table, "mclust_k37_24hour.csv", 
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
  for(j in 1:(length(dates)*2-1)) {
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
  cluster.ref <- hist(cluster.list[day.ref[j+1]:length(cluster.list.hybrid.exp2$hybrid_k1000k20),i], 
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

twentyfour_hour_table_1 <- cbind(twentyfour_hour_table_1,site,dates2)
twentyfour_hour_table_2 <- cbind(twentyfour_hour_table_2,site,dates2)
twentyfour_hour_table_3 <- cbind(twentyfour_hour_table_3,site,dates2)
twentyfour_hour_table_4 <- cbind(twentyfour_hour_table_4,site,dates2)
twentyfour_hour_table_5 <- cbind(twentyfour_hour_table_5,site,dates2)
twentyfour_hour_table_6 <- cbind(twentyfour_hour_table_6,site,dates2)
twentyfour_hour_table_7 <- cbind(twentyfour_hour_table_7,site,dates2)
twentyfour_hour_table_8 <- cbind(twentyfour_hour_table_8,site,dates2)

write.csv(twentyfour_hour_table_1, "hybrid_k1000_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_2, "hybrid_k1500_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_3, "hybrid_k2000_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_4, "hybrid_k2500_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_5, "hybrid_k3000_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_6, "hybrid_k3500_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_7, "hybrid_k4000_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_8, "hybrid_k4500_k20_24hour.csv", 
          row.names = F)

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
  }
}  

four_hour_table_1 <- data.frame(four_hour_table_1[1:120],
                                four_hour_table_1[121:240],
                                four_hour_table_1[241:360],
                                four_hour_table_1[361:480],
                                four_hour_table_1[481:600],
                                four_hour_table_1[601:720],
                                four_hour_table_1[721:840],
                                four_hour_table_1[841:960],
                                four_hour_table_1[961:1080],
                                four_hour_table_1[1081:1200],
                                four_hour_table_1[1201:1320],
                                four_hour_table_1[1321:1440])
four_hour_table_1 <- t(four_hour_table_1)

four_hour_table_2 <- as.data.frame(four_hour_table_2[1:120],
                                   four_hour_table_2[121:240],
                                   four_hour_table_2[241:360],
                                   four_hour_table_2[361:480],
                                   four_hour_table_2[481:600],
                                   four_hour_table_2[601:720],
                                   four_hour_table_2[721:840],
                                   four_hour_table_2[841:960],
                                   four_hour_table_2[961:1080],
                                   four_hour_table_2[1081:1200],
                                   four_hour_table_2[1201:1320],
                                   four_hour_table_2[1321:1440])
four_hour_table_2 <- t(four_hour_table_2)

four_hour_table_3 <- as.data.frame(four_hour_table_3[1:120],
                                   four_hour_table_3[121:240],
                                   four_hour_table_3[241:360],
                                   four_hour_table_3[361:480],
                                   four_hour_table_3[481:600],
                                   four_hour_table_3[601:720],
                                   four_hour_table_3[721:840],
                                   four_hour_table_3[841:960],
                                   four_hour_table_3[961:1080],
                                   four_hour_table_3[1081:1200],
                                   four_hour_table_3[1201:1320],
                                   four_hour_table_3[1321:1440])
four_hour_table_3 <- t(four_hour_table_3)

four_hour_table_4 <- as.data.frame(four_hour_table_4[1:120],
                                   four_hour_table_4[121:240],
                                   four_hour_table_4[241:360],
                                   four_hour_table_4[361:480],
                                   four_hour_table_4[481:600],
                                   four_hour_table_4[601:720],
                                   four_hour_table_4[721:840],
                                   four_hour_table_4[841:960],
                                   four_hour_table_4[961:1080],
                                   four_hour_table_4[1081:1200],
                                   four_hour_table_4[1201:1320],
                                   four_hour_table_4[1321:1440])
four_hour_table_4 <- t(four_hour_table_4)

four_hour_table_5 <- as.data.frame(four_hour_table_5[1:120],
                                   four_hour_table_5[121:240],
                                   four_hour_table_5[241:360],
                                   four_hour_table_5[361:480],
                                   four_hour_table_5[481:600],
                                   four_hour_table_5[601:720],
                                   four_hour_table_5[721:840],
                                   four_hour_table_5[841:960],
                                   four_hour_table_5[961:1080],
                                   four_hour_table_5[1081:1200],
                                   four_hour_table_5[1201:1320],
                                   four_hour_table_5[1321:1440])
four_hour_table_5 <- t(four_hour_table_5)

four_hour_table_6 <- as.data.frame(four_hour_table_6[1:120],
                                   four_hour_table_6[121:240],
                                   four_hour_table_6[241:360],
                                   four_hour_table_6[361:480],
                                   four_hour_table_6[481:600],
                                   four_hour_table_6[601:720],
                                   four_hour_table_6[721:840],
                                   four_hour_table_6[841:960],
                                   four_hour_table_6[961:1080],
                                   four_hour_table_6[1081:1200],
                                   four_hour_table_6[1201:1320],
                                   four_hour_table_6[1321:1440])
four_hour_table_6 <- t(four_hour_table_6)

four_hour_table_7 <- as.data.frame(four_hour_table_7[1:120],
                                   four_hour_table_7[121:240],
                                   four_hour_table_7[241:360],
                                   four_hour_table_7[361:480],
                                   four_hour_table_7[481:600],
                                   four_hour_table_7[601:720],
                                   four_hour_table_7[721:840],
                                   four_hour_table_7[841:960],
                                   four_hour_table_7[961:1080],
                                   four_hour_table_7[1081:1200],
                                   four_hour_table_7[1201:1320],
                                   four_hour_table_7[1321:1440])
four_hour_table_7 <- t(four_hour_table_7)

four_hour_table_8 <- as.data.frame(four_hour_table_8[1:120],
                                   four_hour_table_8[121:240],
                                   four_hour_table_8[241:360],
                                   four_hour_table_8[361:480],
                                   four_hour_table_8[481:600],
                                   four_hour_table_8[601:720],
                                   four_hour_table_8[721:840],
                                   four_hour_table_8[841:960],
                                   four_hour_table_8[961:1080],
                                   four_hour_table_8[1081:1200],
                                   four_hour_table_8[1201:1320],
                                   four_hour_table_8[1321:1440])
four_hour_table_8 <- t(four_hour_table_8)

# Rename the columns
column.names <- NULL
if (i==1) {
  for (k in 1:(length(four_hour_table_1))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(four_hour_table_1) <- column.names
}
if (i==2) {
  for (k in 1:(length(four_hour_table_2))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(four_hour_table_2) <- column.names
}
if (i==3) {
  for (k in 1:(length(four_hour_table_3))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(four_hour_table_3) <- column.names
}

if (i==4) {
  for (k in 1:(length(four_hour_table_4))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(four_hour_table_4) <- column.names
}
if (i==5) {
  for (k in 1:(length(four_hour_table_5))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(four_hour_table_5) <- column.names
}
if (i==6) {
  for (k in 1:(length(four_hour_table_6))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(four_hour_table_6) <- column.names
}
if (i==7) {
  for (k in 1:(length(four_hour_table_7))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(four_hour_table_7) <- column.names
}
if (i==8) {
  for (k in 1:(length(four_hour_table_8))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(four_hour_table_8) <- column.names
}

site <- c(rep("GympieNP",6), rep("WoondumNP",6))

four_hour_table_1 <- cbind(four_hour_table_1,site,dates2)
four_hour_table_2 <- cbind(four_hour_table_2,site,dates2)
four_hour_table_3 <- cbind(four_hour_table_3,site,dates2)
four_hour_table_4 <- cbind(four_hour_table_4,site,dates2)
four_hour_table_5 <- cbind(four_hour_table_5,site,dates2)
four_hour_table_6 <- cbind(four_hour_table_6,site,dates2)
four_hour_table_7 <- cbind(four_hour_table_7,site,dates2)
four_hour_table_8 <- cbind(four_hour_table_8,site,dates2)

write.csv(four_hour_table_1, "hybrid_k1000_k20_4hour.csv", 
          row.names = F)
write.csv(four_hour_table_2, "hybrid_k1500_k20_4hour.csv", 
          row.names = F)
write.csv(four_hour_table_3, "hybrid_k2000_k20_4hour.csv", 
          row.names = F)
write.csv(four_hour_table_4, "hybrid_k2500_k20_4hour.csv", 
          row.names = F)
write.csv(four_hour_table_5, "hybrid_k3000_k20_4hour.csv", 
          row.names = F)
write.csv(four_hour_table_6, "hybrid_k3500_k20_4hour.csv", 
          row.names = F)
write.csv(four_hour_table_7, "hybrid_k4000_k20_4hour.csv", 
          row.names = F)
write.csv(four_hour_table_8, "hybrid_k4500_k20_4hour.csv", 
          row.names = F)

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
