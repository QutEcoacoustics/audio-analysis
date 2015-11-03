# 3 November 2015
# This code generates 24 hour histogram data specifically used to 
# pick out the twelve days used in Exp2

setwd("C:\\Work\\CSV files\\FourMonths")

#setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16")
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_9_10_11_15_16")


cluster.list.hybrid.exp2 <- read.csv("hybrid_clust_35000.csv", header = T)[
  c(54721:59040,100801:103680,106561:108000,
    216001:220320,262081:264960,267841:269280),]

indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2\\Final DataSet 30_31July_1Aug_31Aug_1_4Sept.csv", 
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

dates <- unique(indices$rec.date)
#dates2 <- rep(dates, each=6)
dates2 <- rep(dates, 2)
length.ref <- length(indices$X)


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

write.csv(twentyfour_hour_table_1, "hybrid_k35000_k15_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_2, "hybrid_k35000_k20_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_3, "hybrid_k35000_k25_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_4, "hybrid_k35000_k30_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_5, "hybrid_k35000_k35_24hour.csv", 
          row.names = F)
write.csv(twentyfour_hour_table_6, "hybrid_k35000_k40_24hour.csv", 
          row.names = F)
