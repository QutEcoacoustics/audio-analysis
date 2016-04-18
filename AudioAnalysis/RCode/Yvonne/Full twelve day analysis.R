# 14 April 2016

# This code is a compilation of the code to determine the best 
# clustering method using a twelve day dataset.
# The clustering methods are: kmeans, hclust, mclust and hybrid.
# Works with a twelve (12) day dataset from the two sites Gympie NP and 
# Woondum3.  Dates are: [30/07/2015; 31/07/2015; 1/08/2015]; 
#                       [31/08/2015, 1/09/2015]; [4/09/2015].
# The dataset forms four groups split by sites and months (July and August)
# The aim is to compare three major clustering techniques (listed above)
# and to determine which is best for clustering acoustic data
# The code saves each of the results from thes methods into separate 
# folders

# STEP 1:  Save dataset from Availae
# STEP 2:  Correlation Matrix
# STEP 3:  Principal Components Analysis

# STEP 4:  KMEANS on twelve days
# STEP 5:  Histograms of kmeans clusters 
# STEP 6:  Print dendrograms for kmeans

# STEP 7:  HCLUST on twelve days
# STEP 8:  Histograms of hclust clusters
# STEP 9:  Print dendrograms for hclust

# STEP 10:  MCLUST on twelve days
# STEP 11: Histograms of Mclust clusters
# STEP 12: Print dendrograms for Mclust

# STEP 13: Hybrid on twelve days  
# STEP 14: Histograms on Hybrid clusters
# STEP 15: Print dendrograms for Hybrid

################################################
# STEP 1:  Save dataset from Availae
################################################
setwd("C:\\Work\\CSV files\\FourMonths_repeat\\")
#(for mapping file)

folder <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\GympieNP"

# Set sourceDir to where the wave files 
site <- "Gympie NP1 "
latitude <- "Latitude 26deg 3min 49.6sec"
longitude <- "Longitude 152deg 42min 42.3sec"
elevation <- "225m"

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20151010", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list

myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")
# dates <- c("20150730","20150731","20150801","20150831","20150901","20150904")
file.ref <- NULL
for(i in 1:length(dates))
{
  ref <- grep(paste(dates[i]), myFiles)
  file.ref <- c(file.ref,ref)
}
file.ref

all.indices <- NULL
for (i in seq_along(file.ref)) {
  Name <- myFiles[file.ref[i]]
  assign(paste("fileContents"), read.csv(Name))
  all.indices <- rbind(all.indices, fileContents)
  print(i)
}
write.csv(all.indices, "Gympie_dataSet_upto10Oct2015.csv")

# WOONDUM BEFORE 21 sEPT 2015
folder <- "Y:\\Results\\YvonneResults\\Cooloola_ConcatenatedResults\\Woondum3"

for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list

myFiles <- list.files(path=folder, recursive=T, full.names=TRUE, 
                      pattern="*SummaryIndices.csv$")
file.ref <- NULL
for(i in 1:length(dates))
{
  ref <- grep(paste(dates[i]), myFiles)
  file.ref <- c(file.ref,ref)
}
file.ref
all.indices <- NULL
for (i in seq_along(file.ref)) {
  Name <- myFiles[file.ref[i]]
  assign(paste("fileContents"), read.csv(Name))
  all.indices <- rbind(all.indices, fileContents)
  print(i)
}

#write.csv(all.indices, file=paste("dataSet_", paste(dates, collapse="_"),".csv", sep =""))
write.csv(all.indices, "Woondum_dataSet_upto10Oct2015.csv")

# Concatenating files 
file1 <- read.csv("Gympie_dataSet_upto10Oct2015.csv", header = T)
file2 <- read.csv("Woondum_dataSet_upto10Oct2015.csv", header = T)

length1 <- length(file1$X)
length2 <- length(file2$X)
total.length <- length1 + length2
minute.of.day <- rep(0:1439, total.length/1440)
site <- rep(c("GympieNP", "Woondum3"), each = total.length/2, 
            length = total.length)

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20151011", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

dates <- rep(date.list, each = 1440)
##
concat <- rbind(file1, file2)
concat <- cbind(concat, site, dates, minute.of.day)

write.csv(concat, "Gympie_Woondum_dataset_22June2015_10Oct2015.csv", row.names = F)

# select out the twelve days and save file
twelve <- concat[c(54721:59040, 100801:103680, 106561:108000,
             214561:218880, 260641:263520, 266401:267840),]
write.csv(concat, "Gympie_Woondum_twelve_day_dataset.csv", row.names = FALSE)

################################################
# STEP 2:  Correlation Matrix
################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a")
AcousticDS <- read.csv("Gympie_Woondum_dataset_22June2015_10Oct2015.csv", header=T)

#AcousticDS_noNA <- AcousticDS[complete.cases(AcousticDS), ]
a <- round(abs(cor(AcousticDS_noNA[,2:16][,unlist(lapply(AcousticDS_noNA[,2:16], 
                                                         is.numeric))])),2)
write.table(a, file = paste("Correlation_matrix_final.csv",sep=""), 
            col.names = NA, qmethod = "double", sep = ",")

################################################
# STEP 3:  # PRINCIPAL COMPONENT ANALYSIS 
################################################
# Set indices
ds6 <- AcousticDS[,c(3,4,7,10,11,15,16)] # without Mid-frequency cover

normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

# Create ds3.norm_2_98 for kmeans and hclust
# a dataset normalised between 2 and 98%

ds3.norm_2_98 <- ds6
for (i in 1:length(ds6)) {
  q1 <- unname(quantile(ds6[,i], probs = 0.02, na.rm = TRUE))
  q2 <- unname(quantile(ds6[,i], probs = 0.98, na.rm = TRUE))
  ds3.norm_2_98[,i]  <- normalise(ds3.norm_2_98[,i], q1, q2)
}
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

file <- paste("Principal Component Analysis_ds3norm.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 1200,
  pointsize = 4
)
par(mar =c(2,2,4,2), cex.axis = 2.5)
ds3.norm_noNA <- ds3.norm_2_98[complete.cases(ds3.norm_2_98), ]
PCAofIndices<- prcomp(ds3.norm_noNA)
biplot(PCAofIndices, col=c("grey80","red"), 
       cex=c(0.5,1))#, ylim = c(-0.025,0.02), 
#xlim = c(-0.025,0.02))
abline(h=0,v=0)
mtext(side = 3, line = 2, 
      paste("Principal Component Analysis prcomp ds3"), 
      cex = 2.5)
rm(PCAofIndices)
dev.off()

################################################
# STEP 4: Kmeans on twelve days
################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Kmeans\\x")
AcousticDS <- read.csv("Gympie_Woondum_twelve_day_dataset.csv", header = T)
ds6 <- AcousticDS[,c(3,4,7,10,11,15,16)] # without Mid-frequency cover

normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

ds3.norm_2_98 <- ds6
for (i in 1:length(ds6)) {
  q1 <- unname(quantile(ds6[,i], probs = 0.02, na.rm = TRUE))
  q2 <- unname(quantile(ds6[,i], probs = 0.98, na.rm = TRUE))
  ds3.norm_2_98[,i]  <- normalise(ds3.norm_2_98[,i], q1, q2)
}
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

file <- paste("kmeans_plots_2_98.png", sep = "")
png(
  file,
  width     = 600,
  height    = 600,
  units     = "px"
)

ds3.norm_2_98noNA <- ds3.norm_2_98 #[complete.cases(ds3.norm_2_98), ]
ds3.norm_2_98noNA <- ds3.norm_2_98noNA[,1:7] # replace 7 with length(ds3)
par(mfrow=c(3,1), mar=c(5,7,2,11), cex.main=2, 
    cex.axis=2, cex.lab=2)

# Determining the number of clusters ()
wss <- (nrow(ds3.norm_2_98)*sum(apply(ds3.norm_2_98, 2, var)))
wss <- NULL
#for (i in 2:50) {
for (i in seq(1,40,1)) {
  set.seed(123)
  wss[i] <- kmeans(ds3.norm_2_98, centers=i, iter.max = 50)$tot.withinss
}

error.wss <- NULL
for (i in 1:40) {
  error.wss[i] <- (wss[i]-wss[i+1])/wss[i]
}

plot(1:20, wss[1:20], type = "b", xlab="Number of clusters",
     ylab = "within groups sum of squares",
     main = "kmeans Within Groups Sum of Squares - Exp2")

plot(seq(10000,35000,2500), wss, type = "b", xlab="Number of clusters", 
     ylab = "within groups sum of squares",
     main = "kmeans Within Groups Sum of Squares - Exp2")

min.size <- NULL
max.size <- NULL
variance <- NULL
clusters <- NULL
#write.csv(ds3.norm_2_98noNA,"file.csv")

for (i in 2:50) {
  set.seed(123)
  kmeansObj <- kmeans(ds3.norm_2_98noNA, centers = i, iter.max = 100)
  min <- unname(min(table(kmeansObj$cluster)))
  min.size <- c(min.size, min)
  max <- unname(max(table(kmeansObj$cluster)))
  max.size <- c(max.size, max)
  vari <- var(unname(table(kmeansObj$cluster)))
  variance <- c(variance, vari)
  clust <- kmeansObj$cluster
  clusters <- cbind(clusters, clust)
}

# Rename the columns
column.names <- NULL
for (i in 2:(length(variance)+1)) {
  col.names <- paste("clusters_", i, sep = "")
  column.names <- c(column.names,col.names)
}
colnames(clusters) <- column.names

write.csv(clusters, file = "kmeans_clust.csv")
kmean_clust <- read.csv("kmeans_clust.csv", header=T)

plot(1:50,c(min.size), type = "l", ylim=c(0,8000), 
     ylab = "Cluster size", xlab = "Number of clusters",
     main = "kmeans Cluster Size Range")
mtext("Maximum cluster size",side=2,
      col="black",line=5, cex=2)
par(new=TRUE)

## Error plot
png("kmeans error_test.png", width = 700, height=600)
par(mar=c(4.5,4.5,1.5,4.5), cex = 1.3, cex.axis =1.5,
    cex.lab=1.5)
plot(error.wss,type="l",main="kmeans",xlab = "k value",
     ylab = expression(paste(Delta, " ", Sigma, "wss / ", Sigma, 
                             "wss")),cex.axis=1.6,cex.main=1.5, cex=0.8, lwd = 2)
abline(v=8,col="red",lty=2)
abline(v=16,col="red",lty=2)
abline(v=30,col="red",lty=2)
par(new=TRUE)
plot(max.size[1:40],type = "l", lty=3,yaxt="n",ylab = "",
     xlab = "", lwd=2)
axis(4,ylim=c(min(max.size[1:40]),max(max.size[1:40])),lwd=1.8,
     mgp=c(3,0.4,0),tck=-0.01)
legend("topright",bty='n',lty=c(1,3),lwd=2,
       legend=c("wss error","max cluster size"),cex=1.2)
mtext(side=4, line = 2, "maximum cluster size",cex = 2)
dev.off()
#plot(seq(7500,35000,2500),c(max.size), type = "l", col = "red", 
#     ylim=c(0,9000), xlab = "", 
#     ylab = "",las=1, xlim = c(0,35000))

plot(1:50, c(variance), type = "l", col="blue",
     ylab = "", 
     yaxt='n', xaxt='n',xlab = "")

#plot(seq(7500,35000,2500), c(variance), type = "l", col="blue",
#     ylab = "", xlim = c(0,35000),
#     yaxt='n', xaxt='n',xlab = "")
axis(side=4, at = pretty(range(c(variance))),
     col = "blue",col.axis="blue",las=1)
mtext("Variance",side=4,col="blue",line=9, cex=2)
abline(v=10000, lty=2, col="red")
abline(v=20000, lty=2, col="red")
abline(v=30000, lty=2, col="red")
legend('topright', c("Maximum cluster size","Variance"), 
       lty=1, col=c('red', 'blue'),cex=1)
plot(1:50,error.wss[1:50], type="l")
dev.off()
# calculate the variance
# determine the minimum size of clusters to get more evenly sized 
# clusters
set.seed(123)
kmeansObj <- kmeans(ds3.norm_2_98noNA, centers = 17, iter.max = 100)
kmeansObj$cluster
table(kmeansObj$cluster)
min.cs <- unname(min(table(kmeansObj$cluster)))
min.cs/length(ds3.norm_2_98noNA$BackgroundNoise)*100
max.cs <- unname(max(table(kmeansObj$cluster)))
max.cs/length(ds3.norm_2_98noNA$BackgroundNoise)*100

file <- paste("kmeans9_18_28_plots_ds3norm_2_98_Exp2.png", sep = "")
png(
  file,
  width     = 200,
  height    = 200,
  units     = "mm",
  res       = 400,
  pointsize = 4
)
par(mfrow=c(3,1), mar=c(1,3,1,3), oma=c(4,3,3,3), 
    cex.axis=2, cex.main=3)
plot(kmean_clust$clusters_9, xaxt="n", 
     cex.axis=2)
mtext(side = 3,line = 1.2, "kmeans - Experiment 2 (30July_31July_1Aug_31Aug_1Sept_4Sept2015)", cex = 2.2)
mtext(side = 4, line = 2, "clusters_9", cex = 2)
plot(kmean_clust$clusters_18, xaxt="n", cex.axis=2)
mtext(side = 4, line = 2, "clusters_18", cex = 2)
plot(kmean_clust$clusters_28, cex.axis=2)
mtext(side = 4, line = 2, "clusters_28", cex = 2)
dev.off()

################################################
# STEP 5:  Histograms of kmeans clusters  
################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Kmeans\\x")

indices <- read.csv("C:\\Work\\CSV files\\DataSet_Exp2a\\Final DataSet 30_31July_1Aug_31Aug_1_4Sept.csv", 
                    header=T)
setof6 <- c(6,8,10,12,14,16) # set which columns are to be used for kmeans
cluster.lists.kmeans.exp2   <- read.csv("kmeans_clust.csv", header = T)[,setof6] # kmeans

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

# Saving the kmeans 24 hour files
twentyfour_hour_table_1 <- NULL 
twentyfour_hour_table_2 <- NULL 
twentyfour_hour_table_3 <- NULL 
twentyfour_hour_table_4 <- NULL 
twentyfour_hour_table_5 <- NULL 
twentyfour_hour_table_6 <- NULL 

cluster.list <- cluster.lists.kmeans.exp2

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
  }
}
twentyfour_hour_table_1 <- as.data.frame(twentyfour_hour_table_1)
twentyfour_hour_table_2 <- as.data.frame(twentyfour_hour_table_2)
twentyfour_hour_table_3 <- as.data.frame(twentyfour_hour_table_3)
twentyfour_hour_table_4 <- as.data.frame(twentyfour_hour_table_4)
twentyfour_hour_table_5 <- as.data.frame(twentyfour_hour_table_5)
twentyfour_hour_table_6 <- as.data.frame(twentyfour_hour_table_6)

# Rename the columns
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

# Rename the columns
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

################################################
# STEP 6:  Print dendrograms for kmeans
################################################
myFiles <- list.files(full.names=TRUE, pattern="*hybrid_clust_knn_17500_3_k30_24hour.csv$")
myFilesShort <- list.files(full.names=FALSE, pattern="*hybrid_clust_knn_17500_3_k30_24hour.csv$")

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
  plot(hc.fit, cex=2.5, main = paste(myFilesShort[i]), sub="", 
       xlab = "hclust(method = ward.D2)",
       xaxt="n", yaxt = "n", cex.lab=2, ylab="", 
       cex.main=2.5, lwd=3)
  mtext(side = 2, "Height",cex = 2, line = -2)
  mtext(side = 3, paste("I3D Separation"),
        cex = 2.5, line = -5)
  mtext(side = 3, paste("=",round(I3D.separation,3),sep = " "),
        cex = 2.5, line = -7)
  heightss <- hc.fit$height
  axis(side = 4, at=c(round(heightss[2],0),round(heightss[4],0),round(heightss[6],0),
                      round(heightss[8],0),round(heightss[10],0)), 
       lwd=2,las=1, cex.axis=2.4)
  axis(side = 2, at=c(round(heightss[1],0),round(heightss[3],0),round(heightss[5],0),
                      round(heightss[7],0),round(heightss[9],0),round(heightss[11],0)), 
       lwd=2,las=1, cex.axis=2.4)
  mtext(side = 1, line = 5.5, adj=1, cex=1.3, paste("1,2,3", site[1], dates[1], 
                                                    dates[2], dates[3], "4,5,6", site[1], dates[4], 
                                                    dates[5], dates[6], sep = "    ")) 
  mtext(side = 1, line = 7, adj=1, cex=1.3, paste("7,8,9", site[7], dates[1], 
                                                  dates[2], dates[3], "10,11,12", site[7], dates[4], 
                                                  dates[5], dates[6], sep = "    "))
  mtext(side = 1, line = 8.5, adj=1, cex=1.2, expression(italic(Twelve ~days)))# ~from ~2 ~x ~111 ~days ~of ~clustering)))
  mtext(side = 1, line = 10, adj=1, cex=1.2, expression(italic(Indices:~BackgroundNoise ~Snr ~EventsPerSecond ~LowFreqCover ~AcousticComplexity ~EntropyOfPeaksSpectrum ~EntropyOfCoVSpectrum)))
  mtext(side = 3, line = -0.8, cex=2, paste("heights: ", round(heightss[11],0),
                                            round(heightss[10],0), round(heightss[9],0), round(heightss[8],0),
                                            round(heightss[7],0), round(heightss[6],0), round(heightss[5],0), 
                                            round(heightss[4],0), round(heightss[3],0), round(heightss[2],0), 
                                            round(heightss[1],0), sep = ", "))
  heights <- rbind(heights, heightss)
  dev.off()
}

################################################
# STEP 7:  HCLUST on twelve days
################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\hclust")
require(graphics)
dist.hc <- dist(ds3.norm_2_98)
hc.fit.ward <- hclust(dist.hc, "ward.D2")
hc.fit.average <- hclust(dist.hc, "average")

hc.fit.ward.5 <- cutree(hc.fit.ward, k = 5)
hc.fit.ward.10 <- cutree(hc.fit.ward, k = 10)
hc.fit.ward.15 <- cutree(hc.fit.ward, k = 15)
hc.fit.ward.20 <- cutree(hc.fit.ward, k = 20)
hc.fit.ward.25 <- cutree(hc.fit.ward, k = 25)
hc.fit.ward.30 <- cutree(hc.fit.ward, k = 30)

hc.fit.average.5 <- cutree(hc.fit.average, k = 5)
hc.fit.average.10 <- cutree(hc.fit.average, k = 10)
hc.fit.average.15 <- cutree(hc.fit.average, k = 15)
hc.fit.average.20 <- cutree(hc.fit.average, k = 20)
hc.fit.average.25 <- cutree(hc.fit.average, k = 25)
hc.fit.average.30 <- cutree(hc.fit.average, k = 30)

set.hc.fit <- cbind(hc.fit.average.5,hc.fit.average.10, hc.fit.average.15,
                    hc.fit.average.20, hc.fit.average.25, hc.fit.average.30,
                    hc.fit.ward.5,hc.fit.ward.10, hc.fit.ward.15, hc.fit.ward.20, 
                    hc.fit.ward.25, hc.fit.ward.30)

write.csv(set.hc.fit, "hc_fit_set_cutree_k.csv", row.names = F)

################################################
# STEP 8:  Histograms of hclust clusters
################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2ax\\hclust")
cluster.lists.hclust.k.exp2 <- read.csv("hc_fit_set_cutree_k.csv", header = T) # hclust

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

# Saving the hclust_k 24 hour files
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

################################################
# STEP 9:  Print dendrograms for hclust
################################################
myFiles <- list.files(full.names=TRUE, pattern="*hybrid_clust_knn_17500_3_k30_24hour.csv$")
myFilesShort <- list.files(full.names=FALSE, pattern="*hybrid_clust_knn_17500_3_k30_24hour.csv$")

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
  plot(hc.fit, cex=2.5, main = paste(myFilesShort[i]), sub="", 
       xlab = "hclust(method = ward.D2)",
       xaxt="n", yaxt = "n", cex.lab=2, ylab="", 
       cex.main=2.5, lwd=3)
  mtext(side = 2, "Height",cex = 2, line = -2)
  mtext(side = 3, paste("I3D Separation"),
        cex = 2.5, line = -5)
  mtext(side = 3, paste("=",round(I3D.separation,3),sep = " "),
        cex = 2.5, line = -7)
  heightss <- hc.fit$height
  axis(side = 4, at=c(round(heightss[2],0),round(heightss[4],0),round(heightss[6],0),
                      round(heightss[8],0),round(heightss[10],0)), 
       lwd=2,las=1, cex.axis=2.4)
  axis(side = 2, at=c(round(heightss[1],0),round(heightss[3],0),round(heightss[5],0),
                      round(heightss[7],0),round(heightss[9],0),round(heightss[11],0)), 
       lwd=2,las=1, cex.axis=2.4)
  mtext(side = 1, line = 5.5, adj=1, cex=1.3, paste("1,2,3", site[1], dates[1], 
                                                    dates[2], dates[3], "4,5,6", site[1], dates[4], 
                                                    dates[5], dates[6], sep = "    ")) 
  mtext(side = 1, line = 7, adj=1, cex=1.3, paste("7,8,9", site[7], dates[1], 
                                                  dates[2], dates[3], "10,11,12", site[7], dates[4], 
                                                  dates[5], dates[6], sep = "    "))
  mtext(side = 1, line = 8.5, adj=1, cex=1.2, expression(italic(Twelve ~days)))# ~from ~2 ~x ~111 ~days ~of ~clustering)))
  mtext(side = 1, line = 10, adj=1, cex=1.2, expression(italic(Indices:~BackgroundNoise ~Snr ~EventsPerSecond ~LowFreqCover ~AcousticComplexity ~EntropyOfPeaksSpectrum ~EntropyOfCoVSpectrum)))
  mtext(side = 3, line = -0.8, cex=2, paste("heights: ", round(heightss[11],0),
                                            round(heightss[10],0), round(heightss[9],0), round(heightss[8],0),
                                            round(heightss[7],0), round(heightss[6],0), round(heightss[5],0), 
                                            round(heightss[4],0), round(heightss[3],0), round(heightss[2],0), 
                                            round(heightss[1],0), sep = ", "))
  heights <- rbind(heights, heightss)
  dev.off()
}

################################################
# STEP 10:  MCLUST on twelve days
################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Mclust")
# WARNING: mclust takes many hours 
library(mclust)
mc.fit <- Mclust(ds3.norm, G=1:50) 

sink('mc_fit_BIC_ds3norm_1_50_.csv')
mc.fit$BIC
sink()

summary(mc.fit) 
sink('mc_fit_summary_ds3norm_1_50_.csv')
summary(mc.fit)
sink()

mclust.list <- unname(mc.fit$classification)
png('Clusterplot_ds3norm_1_50_days.png', 
    width = 3000, height = 800, units = "px") 
plot(mclust.list)
abline(v=c(0,1441, 2879,4319,5757,7197,10077,
           11518,12959,14400,15841,17282),lty=2,col="red",lwd=2)
abline(v=8637, lwd=3, lty=2)
dev.off()

list <- which(AcousticDS$minute.of.day=="0")
lst1 <- NULL
for (i in 1:length(list)) {
  lst <- list[i+1]-1
  lst1 <- c(lst1, lst)
}
lst1
#[1]  1441  2879  4319  5757  7197  8637 10077 11518 12959 14400
#[11] 15841    NA

write.table(mclust.list, 
            file="mclustlist_ds3norm_1_50.csv", 
            row.names = F, sep = ",")

mean <- mc.fit$parameters$mean
write.table(mean, 
            file="mclust_ds3norm_1_50_mean.csv", 
            row.names = F, sep = ",")

sink('mclust_variance_ds3norm_1_50_.csv')
mc.fit$parameters$variance
sink()

sigma <- mc.fit$parameters$variance$sigma
write.table(sigma, 
            file="mclust_sigma_ds3norm_1_50.csv", 
            row.names = F, sep = ",")

png('mclust_BIC_plot_ds3norm_1_50.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "BIC")
dev.off()

png('mclust_Density_plot_ds3norm_1_50.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "density")
dev.off()

png('mclust_Classification_plot_ds3norm_1_50.png', 
    width = 1500, height = 1200, units = "px") 
plot(mc.fit, what = "classification")
dev.off()

# Density plot # these take a while these give nice density plots of 
# the normalised data

png('densBGR_plot.png', 
    width = 600, height = 600, units = "px") 
densBackgr <- densityMclust(ds3.norm$BackgroundNoise)
plot(densBackgr, data = ds3.norm$BackgroundNoise, what = "density",
     xlab = "Background Noise")
dev.off()

png('densSNR_plot.png', 
    width = 600, height = 600, units = "px") 
densSNR <- densityMclust(ds3.norm$Snr)
plot(densSNR, data = ds3.norm$Snr, what = "density")
dev.off()

png('densEPS_plot.png', 
    width = 600, height = 600, units = "px") 
densEPS <- densityMclust(ds3.norm$EventsPerSecond)
plot(densEPS, data = ds3.norm$EventsPerSecond, what = "density")
dev.off()

png('densAvSNR_plot.png', 
    width = 1500, height = 1200, units = "px") 
densAvSNR <- densityMclust(ds3.norm$AvgSnrOfActiveFrames)
plot(densAvSNR, data = ds3.norm$AvgSnrOfActiveFrames, what = "density")
dev.off()

png('densAccComp_plot.png', 
    width = 600, height = 600, units = "px") 
densAcousticComp <- densityMclust(ds3.norm$AcousticComplexity)
plot(densAcousticComp, data = ds3.norm$AcousticComplexity, what = "density")
dev.off()

png('densEntCOV_plot.png', 
    width = 600, height = 600, units = "px") 
densEntCoV <- densityMclust(ds3.norm$EntropyOfCoVSpectrum)
plot(densEntCoV, data = ds3.norm$EntropyOfCoVSpectrum, what = "density")
dev.off()

png('densLowFrCov_plot.png', 
    width = 600, height = 600, units = "px") 
densLowFrCov <- densityMclust(ds3.norm$LowFreqCover)
plot(densLowFrCov, data = ds3.norm$LowFreqCover, what = "density")
dev.off()

png('densMidFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densMidFrCov <- densityMclust(ds3.norm$MidFreqCover)
plot(densMidFrCov, data = ds3.norm$MidFreqCover, what = "density")
dev.off()

png('densHighFrCov_plot.png', 
    width = 1500, height = 1200, units = "px") 
densHighFrCov <- densityMclust(ds3.norm$HighFreqCover)
plot(densHighFrCov, data = ds3.norm$HighFreqCover, what = "density")
dev.off()

png('densEntPS_plot.png', 
    width = 600, height = 600, units = "px") 
densEntPs <- densityMclust(ds3.norm$EntropyOfPeaksSpectrum)
plot(densEntPs, data = ds3.norm$EntropyOfPeaksSpectrum, what = "density")
dev.off()

################################################
# STEP 11:  Histograms of Mclust clusters
################################################
cluster.lists.mclust.exp2   <- read.csv("mclustlist_ds3norm_1_50.csv", header = T) # mclust
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

################################################
# STEP 12:  Print dendrograms for Mclust
################################################
myFiles <- list.files(full.names=TRUE, pattern="*hybrid_clust_knn_17500_3_k30_24hour.csv$")
myFilesShort <- list.files(full.names=FALSE, pattern="*hybrid_clust_knn_17500_3_k30_24hour.csv$")

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
  plot(hc.fit, cex=2.5, main = paste(myFilesShort[i]), sub="", 
       xlab = "hclust(method = ward.D2)",
       xaxt="n", yaxt = "n", cex.lab=2, ylab="", 
       cex.main=2.5, lwd=3)
  mtext(side = 2, "Height",cex = 2, line = -2)
  mtext(side = 3, paste("I3D Separation"),
        cex = 2.5, line = -5)
  mtext(side = 3, paste("=",round(I3D.separation,3),sep = " "),
        cex = 2.5, line = -7)
  heightss <- hc.fit$height
  axis(side = 4, at=c(round(heightss[2],0),round(heightss[4],0),round(heightss[6],0),
                      round(heightss[8],0),round(heightss[10],0)), 
       lwd=2,las=1, cex.axis=2.4)
  axis(side = 2, at=c(round(heightss[1],0),round(heightss[3],0),round(heightss[5],0),
                      round(heightss[7],0),round(heightss[9],0),round(heightss[11],0)), 
       lwd=2,las=1, cex.axis=2.4)
  mtext(side = 1, line = 5.5, adj=1, cex=1.3, paste("1,2,3", site[1], dates[1], 
                                                    dates[2], dates[3], "4,5,6", site[1], dates[4], 
                                                    dates[5], dates[6], sep = "    ")) 
  mtext(side = 1, line = 7, adj=1, cex=1.3, paste("7,8,9", site[7], dates[1], 
                                                  dates[2], dates[3], "10,11,12", site[7], dates[4], 
                                                  dates[5], dates[6], sep = "    "))
  mtext(side = 1, line = 8.5, adj=1, cex=1.2, expression(italic(Twelve ~days)))# ~from ~2 ~x ~111 ~days ~of ~clustering)))
  mtext(side = 1, line = 10, adj=1, cex=1.2, expression(italic(Indices:~BackgroundNoise ~Snr ~EventsPerSecond ~LowFreqCover ~AcousticComplexity ~EntropyOfPeaksSpectrum ~EntropyOfCoVSpectrum)))
  mtext(side = 3, line = -0.8, cex=2, paste("heights: ", round(heightss[11],0),
                                            round(heightss[10],0), round(heightss[9],0), round(heightss[8],0),
                                            round(heightss[7],0), round(heightss[6],0), round(heightss[5],0), 
                                            round(heightss[4],0), round(heightss[3],0), round(heightss[2],0), 
                                            round(heightss[1],0), sep = ", "))
  heights <- rbind(heights, heightss)
  dev.off()
}

################################################
# STEP 13:  Hybrid on twelve days
################################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hybrid\\")
list = c(5,10,14,15,16,18,20,22,24,25,26,28,30,32,34,35,36,38,40,45,50,55,60)

for (k in list) {
  clusters <- NULL
  for (i in seq(1000, 4500, 500)) {
    set.seed(123)
    kmeansObj <- kmeans(ds3.norm_2_98, centers = i, iter.max = 100)
    kmeansCenters <- kmeansObj$centers
    dist.hc <- dist(kmeansCenters)
    #hc.fit <- hclust(dist.hc, "average")
    hybrid.fit.ward <- hclust(dist.hc, "ward.D2")
    plot(hybrid.fit.ward)
    hybrid.clusters <- cutree(hybrid.fit.ward, k=k)
    # generate the test dataset
    hybrid.dataset <- cbind(hybrid.clusters, kmeansCenters)
    hybrid.dataset <- as.data.frame(hybrid.dataset)
    train <- hybrid.dataset
    table(hybrid.dataset$hybrid.clusters)
    test <- ds3.norm_2_98
    # set up classes
    cl <- factor(unname(hybrid.clusters))
    # perform k-nearest neighbour
    library(class)
    clusts <- knn(train[,-1], test, cl, k = k3, prob = F)
    clusters <- cbind(clusters, clusts)
  }
  
  # Rename the columns
  column.names <- NULL
  for (i in seq(1000, 4500, 500)) {
    col.names <- paste("hybrid_k", i, "k", k, sep = "")
    column.names <- c(column.names,col.names)
  }
  colnames(clusters) <- column.names
  
  # save hybrid cluster lists
  write.csv(clusters, file = paste("hybrid_clust_k",k,".csv",sep=""), row.names = F)
}
################################################
# STEP 14:  Histograms of hybrid clusters
################################################
# standard list
list1 = c(5,10,15,20,25,30,35,40,45,50,55,60)
# list of between values
list2 = c(14,16,18,22,24,26,28,32,34,36,38)

for(z in list1) {  # set the k value for hybrid method 
  cluster.list.hybrid.exp2 <- read.csv(paste("hybrid_clust_k",z,".csv",sep=""), header = T) # hybrid
  
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
}
################################################
# STEP 15:  Print dendrograms for hybrid
################################################
myFiles <- list.files(full.names=TRUE, pattern="*hybrid_clust_knn_17500_3_k30_24hour.csv$")
myFilesShort <- list.files(full.names=FALSE, pattern="*hybrid_clust_knn_17500_3_k30_24hour.csv$")

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
  plot(hc.fit, cex=2.5, main = paste(myFilesShort[i]), sub="", 
       xlab = "hclust(method = ward.D2)",
       xaxt="n", yaxt = "n", cex.lab=2, ylab="", 
       cex.main=2.5, lwd=3)
  mtext(side = 2, "Height",cex = 2, line = -2)
  mtext(side = 3, paste("I3D Separation"),
        cex = 2.5, line = -5)
  mtext(side = 3, paste("=",round(I3D.separation,3),sep = " "),
        cex = 2.5, line = -7)
  heightss <- hc.fit$height
  axis(side = 4, at=c(round(heightss[2],0),round(heightss[4],0),round(heightss[6],0),
                      round(heightss[8],0),round(heightss[10],0)), 
       lwd=2,las=1, cex.axis=2.4)
  axis(side = 2, at=c(round(heightss[1],0),round(heightss[3],0),round(heightss[5],0),
                      round(heightss[7],0),round(heightss[9],0),round(heightss[11],0)), 
       lwd=2,las=1, cex.axis=2.4)
  mtext(side = 1, line = 5.5, adj=1, cex=1.3, paste("1,2,3", site[1], dates[1], 
                                                    dates[2], dates[3], "4,5,6", site[1], dates[4], 
                                                    dates[5], dates[6], sep = "    ")) 
  mtext(side = 1, line = 7, adj=1, cex=1.3, paste("7,8,9", site[7], dates[1], 
                                                  dates[2], dates[3], "10,11,12", site[7], dates[4], 
                                                  dates[5], dates[6], sep = "    "))
  mtext(side = 1, line = 8.5, adj=1, cex=1.2, expression(italic(Twelve ~days)))# ~from ~2 ~x ~111 ~days ~of ~clustering)))
  mtext(side = 1, line = 10, adj=1, cex=1.2, expression(italic(Indices:~BackgroundNoise ~Snr ~EventsPerSecond ~LowFreqCover ~AcousticComplexity ~EntropyOfPeaksSpectrum ~EntropyOfCoVSpectrum)))
  mtext(side = 3, line = -0.8, cex=2, paste("heights: ", round(heightss[11],0),
                                            round(heightss[10],0), round(heightss[9],0), round(heightss[8],0),
                                            round(heightss[7],0), round(heightss[6],0), round(heightss[5],0), 
                                            round(heightss[4],0), round(heightss[3],0), round(heightss[2],0), 
                                            round(heightss[1],0), sep = ", "))
  heights <- rbind(heights, heightss)
  dev.off()
}