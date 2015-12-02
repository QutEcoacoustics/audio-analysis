# 3 November 2015
# This code generates 24 hour histogram data specifically used to 
# pick out the twelve days used in Exp2
# This code also saves 2 hour historgram data for the whole dataset

##### change this!!!!!
#setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3b")
csv.name <- "hybrid_clust_knn_17500_3"

#cluster.list.hybrid.exp2 <- read.csv(paste(csv.name, ".csv", sep = ""), 
#                            header = T)[c(54721:59040,100801:103680,106561:108000,
#                            216001:220320,262081:264960,267841:269280),]
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
cluster.list.hybrid.exp2 <- read.csv(paste(csv.name, ".csv", sep = ""), 
                            header = T)

indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)
dates <- unique(indices$rec.date)
site <- c(rep("GympieNP",111*12), rep("WoondumNP",111*12)) # for the two hour files
dates2 <- rep(dates, each=12, 2) # this is for the 2 hour files
# Adaptation to cluster all 112 days x 2 sites 
#cluster.list.hybrid.exp2 <- read.csv("hybrid_clust_30000na_replaced_with_50.csv", header = T)
#cluster.list.hybrid.exp2 <- cluster.list.hybrid.exp2[complete.cases(cluster.list.hybrid.exp2), ]
indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)
site <- c(rep("GympieNP",112), rep("WoondumNP",112))


day.ref <- which(indices$minute.of.day=="0")
day.ref <- c(day.ref, (length(indices$minute.of.day)+1))
two.am.ref   <- which(indices$minute.of.day == "120")
four.am.ref  <- which(indices$minute.of.day == "240")
six.am.ref   <- which(indices$minute.of.day == "360")
eight.am.ref <- which(indices$minute.of.day == "480")
ten.am.ref <- which(indices$minute.of.day == "600")
midday.ref   <- which(indices$minute.of.day == "720")
two.pm.ref   <- which(indices$minute.of.day == "840")
four.pm.ref  <- which(indices$minute.of.day == "960")
six.pm.ref   <- which(indices$minute.of.day == "1080")
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
twentyfour_hour_table_13 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_14 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_15 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_16 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_17 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_18 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_19 <- NULL #read.csv(text="col1,col2")
twentyfour_hour_table_20 <- NULL #read.csv(text="col1,col2")

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

#write.csv(twentyfour_hour_table_1, paste(csv.name, "_k5_24hour_full111days.csv", sep = ""), 
#          row.names = F)
#write.csv(twentyfour_hour_table_2, paste(csv.name, "_k10_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_3, paste(csv.name, "_k15_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_4, paste(csv.name, "_k20_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_5, paste(csv.name, "_k25_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_6, paste(csv.name, "_k30_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_7, paste(csv.name, "_k35_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_8, paste(csv.name, "_k40_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_9, paste(csv.name, "_k45_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_10, paste(csv.name, "_k50_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_11, paste(csv.name, "_k55_24hour_full111days.csv", sep = ""), 
#         row.names = F)
#write.csv(twentyfour_hour_table_12, paste(csv.name, "_k60_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_13, paste(csv.name, "_k65_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_14, paste(csv.name, "_k70_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_15, paste(csv.name, "_k75_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_16, paste(csv.name, "_k80_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_17, paste(csv.name, "_k85_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_18, paste(csv.name, "_k90_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_19, paste(csv.name, "_k95_24hour_full111days.csv", sep = ""),
#          row.names = F)
#write.csv(twentyfour_hour_table_20, paste(csv.name, "_k100_24hour_full111days.csv", sep = ""),
#          row.names = F)
####################################################
# Saving the Hybrid 2 hour files - used for all 111 days
####################################################
two_hour_table_1 <- NULL #read.csv(text="col1,col2")
two_hour_table_2 <- NULL #read.csv(text="col1,col2")
two_hour_table_3 <- NULL #read.csv(text="col1,col2")
two_hour_table_4 <- NULL #read.csv(text="col1,col2")
two_hour_table_5 <- NULL #read.csv(text="col1,col2")
two_hour_table_6 <- NULL #read.csv(text="col1,col2")
two_hour_table_7 <- NULL #read.csv(text="col1,col2")
two_hour_table_8 <- NULL #read.csv(text="col1,col2")
two_hour_table_9 <- NULL #read.csv(text="col1,col2")
two_hour_table_10 <- NULL #read.csv(text="col1,col2")
two_hour_table_11 <- NULL #read.csv(text="col1,col2")
two_hour_table_12 <- NULL #read.csv(text="col1,col2")
two_hour_table_13 <- NULL #read.csv(text="col1,col2")
two_hour_table_14 <- NULL #read.csv(text="col1,col2")
two_hour_table_15 <- NULL #read.csv(text="col1,col2")
two_hour_table_16 <- NULL #read.csv(text="col1,col2")
two_hour_table_17 <- NULL #read.csv(text="col1,col2")
two_hour_table_18 <- NULL #read.csv(text="col1,col2")
two_hour_table_19 <- NULL #read.csv(text="col1,col2")
two_hour_table_20 <- NULL #read.csv(text="col1,col2")
cluster.list <- cluster.list.hybrid.exp2
for (i in 1:length(cluster.list)) {
  for(j in 1:(length(two.hour.ref)-1)) {
    cluster.ref <- hist(cluster.list[two.hour.ref[j]:(two.hour.ref[j+1]-1),i], 
                        breaks=seq(0.5,(max(cluster.list[,i])+0.5)))
    print(length(cluster.ref$counts))
    
    if (i == 1)  {two_hour_table_1  <- rbind(two_hour_table_1, cluster.ref$counts)}
    if (i == 2)  {two_hour_table_2  <- rbind(two_hour_table_2, cluster.ref$counts)}
    if (i == 3)  {two_hour_table_3  <- rbind(two_hour_table_3, cluster.ref$counts)}  
    if (i == 4)  {two_hour_table_4  <- rbind(two_hour_table_4, cluster.ref$counts)}
    if (i == 5)  {two_hour_table_5  <- rbind(two_hour_table_5, cluster.ref$counts)}
    if (i == 6)  {two_hour_table_6  <- rbind(two_hour_table_6, cluster.ref$counts)}  
    if (i == 7)  {two_hour_table_7  <- rbind(two_hour_table_7, cluster.ref$counts)}
    if (i == 8)  {two_hour_table_8  <- rbind(two_hour_table_8, cluster.ref$counts)}
    if (i == 9)  {two_hour_table_9  <- rbind(two_hour_table_9, cluster.ref$counts)}
    if (i == 10) {two_hour_table_10 <- rbind(two_hour_table_10, cluster.ref$counts)}
    if (i == 11)  {two_hour_table_11  <- rbind(two_hour_table_11, cluster.ref$counts)}
    if (i == 12)  {two_hour_table_12  <- rbind(two_hour_table_12, cluster.ref$counts)}
    if (i == 13)  {two_hour_table_13  <- rbind(two_hour_table_13, cluster.ref$counts)}  
    if (i == 14)  {two_hour_table_14  <- rbind(two_hour_table_14, cluster.ref$counts)}
    if (i == 15)  {two_hour_table_15  <- rbind(two_hour_table_15, cluster.ref$counts)}
    if (i == 16)  {two_hour_table_16  <- rbind(two_hour_table_16, cluster.ref$counts)}  
    if (i == 17)  {two_hour_table_17  <- rbind(two_hour_table_17, cluster.ref$counts)}
    if (i == 18)  {two_hour_table_18  <- rbind(two_hour_table_18, cluster.ref$counts)}
    if (i == 19)  {two_hour_table_19  <- rbind(two_hour_table_19, cluster.ref$counts)}
    if (i == 20) {two_hour_table_20 <- rbind(two_hour_table_20, cluster.ref$counts)}
  }
}

two_hour_table_1 <- as.data.frame(two_hour_table_1)
two_hour_table_2 <- as.data.frame(two_hour_table_2)
two_hour_table_3 <- as.data.frame(two_hour_table_3)
two_hour_table_4 <- as.data.frame(two_hour_table_4)
two_hour_table_5 <- as.data.frame(two_hour_table_5)
two_hour_table_6 <- as.data.frame(two_hour_table_6)
two_hour_table_7 <- as.data.frame(two_hour_table_7)
two_hour_table_8 <- as.data.frame(two_hour_table_8)
two_hour_table_9 <- as.data.frame(two_hour_table_9)
two_hour_table_10 <- as.data.frame(two_hour_table_10)
two_hour_table_11 <- as.data.frame(two_hour_table_11)
two_hour_table_12 <- as.data.frame(two_hour_table_12)
two_hour_table_13 <- as.data.frame(two_hour_table_13)
two_hour_table_14 <- as.data.frame(two_hour_table_14)
two_hour_table_15 <- as.data.frame(two_hour_table_15)
two_hour_table_16 <- as.data.frame(two_hour_table_16)
two_hour_table_17 <- as.data.frame(two_hour_table_17)
two_hour_table_18 <- as.data.frame(two_hour_table_18)
two_hour_table_19 <- as.data.frame(two_hour_table_19)
two_hour_table_20 <- as.data.frame(two_hour_table_20)
# Rename the columns
column.names <- NULL
if (i==1) {
  for (k in 1:(length(two_hour_table_1))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_1) <- column.names
}
if (i==2) {
  for (k in 1:(length(two_hour_table_2))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_2) <- column.names
}
if (i==3) {
  for (k in 1:(length(two_hour_table_3))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_3) <- column.names
}

if (i==4) {
  for (k in 1:(length(two_hour_table_4))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_4) <- column.names
}
if (i==5) {
  for (k in 1:(length(two_hour_table_5))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_5) <- column.names
}
if (i==6) {
  for (k in 1:(length(two_hour_table_6))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_6) <- column.names
}
if (i==7) {
  for (k in 1:(length(two_hour_table_7))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_7) <- column.names
}
if (i==8) {
  for (k in 1:(length(two_hour_table_8))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_8) <- column.names
}
if (i==9) {
  for (k in 1:(length(two_hour_table_9))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_9) <- column.names
}
if (i==10) {
  for (k in 1:(length(two_hour_table_10))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_10) <- column.names
}
if (i==11) {
  for (k in 1:(length(two_hour_table_11))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_11) <- column.names
}
if (i==12) {
  for (k in 1:(length(two_hour_table_12))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_12) <- column.names
}
if (i==13) {
  for (k in 1:(length(two_hour_table_13))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_13) <- column.names
}

if (i==14) {
  for (k in 1:(length(two_hour_table_14))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_14) <- column.names
}
if (i==15) {
  for (k in 1:(length(two_hour_table_15))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_15) <- column.names
}
if (i==16) {
  for (k in 1:(length(two_hour_table_16))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_16) <- column.names
}
if (i==17) {
  for (k in 1:(length(two_hour_table_17))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_17) <- column.names
}
if (i==18) {
  for (k in 1:(length(two_hour_table_18))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_18) <- column.names
}
if (i==19) {
  for (k in 1:(length(two_hour_table_19))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_19) <- column.names
}
if (i==20) {
  for (k in 1:(length(two_hour_table_20))) {
    col.names <- paste("clus_", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(two_hour_table_20) <- column.names
}

two_hour_table_1 <- cbind(two_hour_table_1,site,as.character(dates2))
two_hour_table_2 <- cbind(two_hour_table_2,site,as.character(dates2))
two_hour_table_3 <- cbind(two_hour_table_3,site,as.character(dates2))
two_hour_table_4 <- cbind(two_hour_table_4,site,as.character(dates2))
two_hour_table_5 <- cbind(two_hour_table_5,site,as.character(dates2))
two_hour_table_6 <- cbind(two_hour_table_6,site,as.character(dates2))
two_hour_table_7 <- cbind(two_hour_table_7,site,as.character(dates2))
two_hour_table_8 <- cbind(two_hour_table_8,site,as.character(dates2))
two_hour_table_9 <- cbind(two_hour_table_9,site,as.character(dates2))
two_hour_table_10 <- cbind(two_hour_table_10,site,as.character(dates2))
two_hour_table_11 <- cbind(two_hour_table_11,site,as.character(dates2))
two_hour_table_12 <- cbind(two_hour_table_12,site,as.character(dates2))
two_hour_table_13 <- cbind(two_hour_table_13,site,as.character(dates2))
two_hour_table_14 <- cbind(two_hour_table_14,site,as.character(dates2))
two_hour_table_15 <- cbind(two_hour_table_15,site,as.character(dates2))
two_hour_table_16 <- cbind(two_hour_table_16,site,as.character(dates2))
two_hour_table_17 <- cbind(two_hour_table_17,site,as.character(dates2))
two_hour_table_18 <- cbind(two_hour_table_18,site,as.character(dates2))
two_hour_table_19 <- cbind(two_hour_table_19,site,as.character(dates2))
two_hour_table_20 <- cbind(two_hour_table_20,site,as.character(dates2))


write.csv(two_hour_table_1, paste(csv.name, "_k5_2hour_full111days.csv", sep = ""), 
          row.names = F)
write.csv(two_hour_table_2, paste(csv.name, "_k10_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_3, paste(csv.name, "_k15_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_4, paste(csv.name, "_k20_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_5, paste(csv.name, "_k25_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_6, paste(csv.name, "_k30_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_7, paste(csv.name, "_k35_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_8, paste(csv.name, "_k40_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_9, paste(csv.name, "_k45_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_10, paste(csv.name, "_k50_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_11, paste(csv.name, "_k55_2hour_full111days.csv", sep = ""), 
          row.names = F)
write.csv(two_hour_table_12, paste(csv.name, "_k60_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_13, paste(csv.name, "_k65_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_14, paste(csv.name, "_k70_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_15, paste(csv.name, "_k75_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_16, paste(csv.name, "_k80_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_17, paste(csv.name, "_k85_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_18, paste(csv.name, "_k90_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_19, paste(csv.name, "_k95_2hour_full111days.csv", sep = ""),
          row.names = F)
write.csv(two_hour_table_20, paste(csv.name, "_k100_2hour_full111days.csv", sep = ""),
          row.names = F)