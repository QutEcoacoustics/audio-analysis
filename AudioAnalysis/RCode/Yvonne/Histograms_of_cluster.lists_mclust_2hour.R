# 7 September 2015
# Histograms of cluster.lists - daily and two-hourly
#
setwd("C:\\Work\\CSV files\\GympieNP1_new\\mclust_30clusters\\")
cluster.list <- read.csv("mclust30list_9.csv", header = T)
indices <- read.csv("C:\\Work\\CSV files\\GympieNP1_new\\all_data\\Towsey_Summary_Indices_Gympie NP1 22-06-2015to current.csv")

day.ref <-      which(indices$minute.of.day == "0")
two.am.ref <-   which(indices$minute.of.day == "120")
four.am.ref <-  which(indices$minute.of.day == "240")
six.am.ref <-   which(indices$minute.of.day == "360")
eight.am.ref <- which(indices$minute.of.day == "480")
ten.am.ref  <-  which(indices$minute.of.day == "600")
midday.ref <-   which(indices$minute.of.day == "720")
two.pm.ref  <-  which(indices$minute.of.day == "840")
four.pm.ref <-  which(indices$minute.of.day == "960")
six.pm.ref  <-  which(indices$minute.of.day == "1080")
eight.pm.ref <- which(indices$minute.of.day == "1200")
ten.pm.ref  <-  which(indices$minute.of.day == "1320")
four.hour.ref <- c(day.ref, four.am.ref,eight.am.ref,midday.ref,
                   four.pm.ref,eight.pm.ref)
four.hour.ref <- sort(four.hour.ref)
two.hour.ref <- c(four.hour.ref, two.am.ref, six.am.ref, ten.am.ref,
                  two.pm.ref, six.pm.ref, ten.pm.ref)
two.hour.ref <- sort(two.hour.ref)
dates <- unique(indices$rec.date)
dates2 <- rep(dates, each=12)
length.ref <- length(indices$X)
cluster.list <- as.data.frame(cluster.list)
#rm(indices)

cluster.ref <- unname(table(cluster.list[day.ref[1]:day.ref[2]-1,]))
### Daily clustering
ref <- LETTERS[1:26] ## Gives a sequence of the letters of the alphabet 
counts <- NULL

for(i in 1:ceiling(length(dates)/12)) { 
  mypath <- file.path("C:\\Work\\CSV files\\GympieNP1_new\\mclust_30clusters",
                      paste("histogram_mclust", ref[i], ".png", sep = ""))
  png(file=mypath,
      width = 200, 
      height = 85, 
      units = "mm",
      res=1200,
      pointsize = 5)
  par(mfrow=c(3,4),mar=c(3,3,3,2))
  for(j in 1:12) {
    ht <- hist(cluster.list[(day.ref[j+12*(i-1)]):(day.ref[j+12*(i-1)+1]-1),], 
               main = paste(dates[j+12*(i-1)]), xlab = "Cluster reference",
               ylim=c(0,400), breaks=seq(0.5,30.5,by=1), xlim = c(0,30),
               freq = T)
    #hist(cluster.list[day.ref[j+6*(i-1)]:day.ref[j+6*(i-1)+1]-1,], 
    #     main = paste(dates[j+6*(i-1)]), xlab = "Cluster reference",
    #     ylim=c(0,900), breaks=seq(0.5,30.5,by=1), xlim = c(0,30))
    counts <- rbind(counts, ht$counts)
  }
  dev.off() 
} 
dev.off()
counts <- as.data.frame(counts)
#View(counts)
counts <- cbind(counts, dates[1:length(dates)-1]) 

write.csv(counts, file="Cluster_dailycount_mclust.csv", row.names = F)

#################
#ref <- LETTERS[1:26] ## Gives a sequence of the letters of the alphabet 
#counts <- NULL

#for(i in 1:ceiling(length(two.hour.ref)/24)) { 
#  mypath <- file.path("C:\\Work\\CSV files\\GympieNP1_new\\mclust_30clusters",
#                      paste("histogram_mclust_fourhour", ref[i], ".png", sep = ""))
#  png(file=mypath,
#      width = 200, 
#      height = 85, 
#      units = "mm",
#      res=1200,
#      pointsize = 5)
#  
#  par(mfrow=c(4,12), mar=c(1,1,2,1), oma=c(3,3,3,3))
#  for(j in 1:48) {
#    ht <- hist(cluster.list[(two.hour.ref[j+48*(i-1)]):(two.hour.ref[j+48*(i-1)+1]-1),], 
#               main = paste(dates2[j+48*(i-1)]), xlab = "Cluster reference",
#               ylim=c(0,100), breaks=seq(0.5,30.5,by=1), xlim = c(0,30),
#               freq = T)
#    counts <- rbind(counts, ht$counts)
#  }
#  dev.off() 
#} 
#dev.off()
#counts <- as.data.frame(counts)

#time.ref <- rep(letters[1:6],ceiling(length(two.hour.ref)/6))
#time.ref <- rep(c("red","orange","yellow","green","blue","mediumpurple1",
#                  "magenta","deepskyblue","sienna","darkgreen","midnightblue","pink"), ceiling(length(two.hour.ref)/12))
#counts <- cbind(counts, time.ref[1:length(counts$V1)], dates2[1:(length(dates2)-7)])
#colnames(counts) <- c("C1","C2","C3","C4","C5","C6","C7","C8","C9","C10","C11","C12",
#                      "C13","C14","C15","C16","C17","C18","C19","C20","C21","C22",
#                      "C23","C24","C25","C26","C27","C28","C29","C30","timeRef","date")
#write.csv(counts, file="Cluster_2hourcount_mclust.csv", row.names = F)
