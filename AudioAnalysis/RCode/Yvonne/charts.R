setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k")

####################################
# Plotting comparison between 4, 6 and 24 hour fingerprints using hybrid method
# 12 days (2% normalisation)
# The dendrograms are in two places
# 1. "C:\Work\CSV files\DataSet_Exp2a\hclust\" &
# 2. ""C:\Work\CSV files\DataSet_Exp2a\Hierarchical\"
####################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\hclust\\")
x <- c(5,10,15,20,25,30)
hclust_4hour <- c(2.078, 1.695, 2.022, 2.087, 2.046, 2.011)
hclust_6hour <- c(2.019, 1.634, 1.884, 2.065, 2.033, 1.995)
#hclust_12hour <- c(1.946, 1.931, 1.939, 2.148, 2.158, 2.036)
hclust_24hour <- c(1.766, 1.360, 1.381, 1.763, 1.738, 1.701)

png("Comparison 4 6 24 hr fingerprints_2percent.png", 
    height = 600,width = 600)
par(mar=c(5,5,3,2),cex=1.3, cex.axis=1.5, cex.lab=1.5)
plot(x, hclust_4hour, type = "b",pch=17, ylim = c(1.3,2.3),
     ylab = "I3D distance",xlab = "",
     main = "hclust (ward.D2)
     4, 6 & 24 hour fingerprints - 12 days",
     cex=1.3,cex.axis=1.5)
par(new=TRUE)
plot(x, hclust_6hour,type = "b",pch=19,ylim = c(1.3,2.2),
     yaxt="n",ylab = "",xlab = "k",cex=1.3,cex.axis=1.5)
par(new=TRUE)
plot(x, hclust_24hour,type = "b",pch=15,ylim = c(1.3,2.2),
     yaxt="n",ylab = "",xlab = "k",cex=1.3,cex.axis=1.5)
legend("bottomright",pch = c(17, 19, 15),
       c("4 hour","6 hour","24 hour"),bty = "n",cex=1.5)
dev.off()


####################################
# Plotting kmeans (12 days)
#
# The dendrograms are in the folder:
# "C:\Work\CSV files\DataSet_Exp2a\Kmeans\"
####################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Kmeans\\")
x <- c(6,10,12,14,16,18,20,22,24,26,28,30)
y <- c(2.036, 1.306,
       1.291, 1.302, 1.347, 
       1.933, 1.92 , 1.916, 1.931,
       1.935, 1.983, 2.022)
png("kmeans(12days)_2percent.png",height = 600, width = 600)
par(mar=c(4.5,4.5,3,1.5),cex=1.3, cex.axis=1.5, cex.lab=1.5)
plot(x,y,type = "b",xlab = "k",pch=19,
     ylab = "I3D distance",
     main = "kmeans 
     24 hour fingerprints - 12 days",xlim=c(5,30),
     cex=1.3,cex.axis=1.6, cex.main=1.5)
dev.off()
####################################
# Plotting hclust comparison between average and ward.D2
# methods (12 days)
#
# The dendrograms are found in folder:
# 1. "C:\Work\CSV files\DataSet_Exp2a\hclust\" 
# and are based on the 24 hour fingerprints (wardD2) 
# not average or the 4 or 6 hour fingerprints
####################################
#x <- c(5, 10, 15, 20, 25, 30)
#I3D.average <- c(1.583,1.445,1.647,1.706,1.701,1.716)
#I3D.wardD2 <- c(2.06,1.465,1.508,1.502,1.818,1.966)
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\hclust\\")
x <- c(5, 10, 15, 20, 25, 30)
I3D.average <- c(1.527, 1.408, 1.523, 1.585, 1.580, 1.513)
I3D.wardD2 <- c(1.766, 1.360, 1.381, 1.763, 1.738, 1.701)

png("hclust 12 day Average Vs Wardd2_2percent.png",height = 600, width = 600)
par(mar=c(4.5,4.5,3,1.5),cex=1.3, cex.axis=1.5, cex.lab=1.5)
plot(x,I3D.average,type = "b",pch=15, ylim = c(1.3,1.85),
     ylab = "I3D distance",xlab = "",
     main = "hclust - average and ward.D2 methods 
     24 hour fingerprints - 12 days",
     cex=1.3,cex.axis=1.6,cex.main=1.5)
par(new=TRUE)
plot(x,I3D.wardD2,type = "b",pch=19,ylim = c(1.3,1.85),
     yaxt="n",ylab = "",xlab = "k",cex=1.3,cex.axis=1.5)
legend("bottomright",pch = c(15, 19),
       c("hclust (average)","hclust (ward.D2)"),bty = "n",cex=1.5)
dev.off()

####################################
# Plotting hybrid method (12 days) 2%
# This only compares the 24 hour fingerprints
# The dendrograms are in folder:
#
# "C:\\Work\\CSV files\\DataSet_Exp2a\\Hybrid\\"
####################################
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hybrid\\")
x <- c(5,10,15,20,22,24,26,28,30)#,35,40,45)
hybrid1000 <- c(1.915,1.292,1.252,1.735,1.521,1.519,1.471,1.482,1.484)#,1.494,1.592,1.601)
hybrid1500<- c(1.789,1.292,1.305,1.334,1.419,1.416,1.478,1.414,1.402)#,1.512,1.878,2.034)
hybrid2000 <- c(1.802,1.279,1.314,1.696,1.450,1.450,1.429,1.449,1.456)#,1.477,1.476,1.654)
hybrid2500<- c(1.858,1.354,1.320,1.357,1.384,1.363,1.349,1.482,1.493)#,1.508,1.568,1.737)
hybrid3000 <- c(1.677,1.299,1.305,1.310,1.406,1.369,1.375,1.385,1.389)#,1.382,1.482,1.708)
hybrid3500<- c(1.681,1.367,1.37,1.354,1.682,1.69,1.706,2.012,2.024)#,1.681,1.824,2.184)
png("hybrid 12 day_2percent.png",height = 600,width = 600)
par(mar=c(4.5,4.5,3,1.5),cex=1.3, cex.axis=1.5, cex.lab=1.5)
plot(x,hybrid2500,type = "b",pch=17, ylim = c(1.1,2.1),
     xlim = c(5,30),
     ylab = "I3D distance",xlab = "",
     main = "hybrid  
     24 hour fingerprints - 12 days",
     cex=1.3,cex.axis=1.6, cex.main=1.5)
par(new=TRUE)
plot(x,hybrid3000,type = "b",pch=19,ylim = c(1.1,2.1),
     yaxt="n",ylab = "",xlab = "k",cex=1.3,cex.axis=1.6,
     xlim = c(5,30))
par(new=TRUE)
plot(x,hybrid3500,type = "b",pch=15,ylim = c(1.1,2.1),
     yaxt="n",ylab = "",xlab = "k",cex=1.3,cex.axis=1.6,
     xlim = c(5,30))
#par(new=TRUE)
#plot(x,hybrid3500,type = "b",pch=18,ylim = c(1.1,2.1),
#     yaxt="n",ylab = "",xlab = "k",cex=1.3,cex.axis=1.6)
legend("bottomright",pch = c(17, 19, 15), #, 18), 
       c("hybrid2500", "hybrid3000", "hybrid3500"),
       bty = "n",cex=1.4)
dev.off()

############################################
# Mean rainfall 
# Cooran
###########################################
month <- c("Jan", "Feb", "Mar",
           "Apr","May","Jun","Jul","Aug",
           "Sept", "Oct", "Nov", "Dec")
rainfall <- c(158.7, 197.9, 157.3, 81.1,56.3,
              40.9, 28.1, 75.8, 39.0, 58.2,
              66.7, 119.9)
par(mar=c(5,5,2,1))
barplot(names.arg=month, height=rainfall, ylab = "rainfall (mm)",
     main = "Average monthly rainfall - Cooran", cex.axis = 2,
     cex.main=2,cex.lab=2, xlab = "months", ylim = c(0,250),
     col = "gray20")
# Goomboorian
rainfall <- c(143.0, 233.3, 137.8, 127.3,
              86.6,  68.0,   35.0,  60.3,
              40.3,  56.9,   79.5, 122.2)
barplot(names.arg=month, height=rainfall, ylab = "rainfall (mm)",
        main = "Average monthly rainfall - Goomboorian", cex.axis = 2,
        cex.main=2,cex.lab=2, xlab = "months", ylim = c(0,250),
        col = "gray20")

temperature.min <- c(19.6, 19.6, 18.1, 14.7, 10.8,	
                      8.0,	6.3,  7.1, 10.2, 13.8,	
                     16.5, 18.5)
temperature.max <- c(31.2, 30.4, 29.3, 27.3,
                     24.5, 22.0, 21.9, 23.4,
                     26.1, 28.3, 30.3, 31.3)
par(mar=c(5,5,6,1))
barplot(names.arg=month, height=temperature.max, ylab = "temperature (C)",
        main = "Average minimum and maximum monthly 
        temperatures - Gympie", cex.axis = 2,
        cex.main=2,cex.lab=2, xlab = "months", ylim = c(0,32))
par(new=TRUE)
barplot(names.arg=month, height=temperature.min, ylab = "temperature (C)",
        cex.axis = 2, cex.main=2,cex.lab=2, xlab = "months", ylim = c(0,32),
        col="gray20")

temperature.min.tewantin <- c(21.9, 21.9, 20.7, 18.0,
                              14.7, 12.6, 11.1, 11.6,
                              14.4, 16.9, 18.8, 20.7) 

temperature.max.tewantin <- c(28.5, 28.4, 27.4, 25.8,
                              23.5, 21.5,	21.1,	22.2,
                              24.2, 25.5,	26.5,	28.0)
barplot(names.arg=month, height=temperature.max.tewantin, ylab = "temperature (C)",
        main = "Average minimum and maximum monthly 
        temperatures - Tewantin", cex.axis = 2,
        cex.main=2,cex.lab=2, xlab = "months", ylim = c(0,32))
par(new=TRUE)
barplot(names.arg=month, height=temperature.min.tewantin, ylab = "temperature (C)",
        cex.axis = 2, cex.main=2,cex.lab=2, xlab = "months", ylim = c(0,32),
        col="gray20")
#########################################################
# rose diagrams
#################################################################
library(circular)
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k")
cluster.list <- read.csv("hybrid_clust_17500_30.csv", header = T)
hour.sequence <- rep(rep(seq(0,23.9,0.1), each=6),222)
cluster.list <- cbind(cluster.list, hour.sequence)

dataset <- read.csv("hybrid_clust_knn_17500_3_k30_2hour_full111days.csv",header=T)
dates <- unique(dataset$as.character.dates2.)
dataset$as.character.dates2. <- as.Date(dataset$as.character.dates2., format = "%d/%m/%Y")
dates1 <- unique(substring(dataset$as.character.dates2.,1,7))
site <- unique(dataset$site)
hour.period <- c("12-2am","2-4am","4-6am","6-8am","8-10am","10-12noon",
                 "12-2pm","2-4pm","4-6pm","6-8pm","8-10pm","10-12midnight")
hour.period <- c("01","02","03","04","05","06","07","08","09","10","11","12")
count <- 1
for(i in 1:length(dataset$V1)) {
  if(count == 13) {count <- 1}
  dataset$site.yr.mth[i] <- paste(dataset$site[i], substr(dataset$as.character.dates2.[i],1,7),
                                  hour.period[count],sep="_") 
  count <- count + 1
}
# determining number of days in each month at each site
days.per.month <- NULL
for (i in 1:length(site)) {
  for (j in 1:length(dates1)) {
    ref <- which(substr(dataset$as.character.dates2.,1,7) == dates1[j]
                 & dataset$site==site[i])
    days <- length(ref)/12  
    days.per.month <- c(days.per.month, days)  
  }
}
months <- c("jun","july","aug","sept","oct")
# repeat months for two sites
months <- rep(months,2)
month <- NULL
for (i in 1:length(days.per.month)){
  month.c <- c(rep(months[i],each=(1440*days.per.month[i]))) 
  month <- c(month, month.c)
}
site <- rep(site, each = (length(dataset$V1)*120/2))
cluster.list <- cbind(cluster.list, site,month)

# cluster 1 Gympie NP June 
clus1.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==1 &
                 cluster.list$site=="GympieNP" &
                 cluster.list$month=="jun")
times1 <- cluster.list$hour.sequence[c(clus1.Gy.jun)]
times1 <- sample(times1, floor(length(times1)/days.per.month[1]))
times1c <- circular(times1, type= "angles", units = "hours",
                    template = "clock24")
plot(times1c, pch=16, xlab = "Observation number", 
     ylab = "Cluster 1 times (in hours)",xaxt="n")
plot(times1c, cex=1.5, bin=24, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.2,1.2),xlim = c(-2,2),xaxt="n")
rose.diag(times1c, bins = 12, col="darkgrey",
          cex=1.5, prop = 2, add = F)
lines(density.circular(times1c, bw=100),lwd=2,lty=2, col="red")
rm(times1, times1c,clus1.Gy.jun)

# cluster 1 June for Gympie
clus1.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==1 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="jun")
times1 <- cluster.list$hour.sequence[c(clus1.Gy.jun)]

times1c <- circular(times1, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
plot(times1c, pch=20, xlab = "Observation number", 
     ylab = "Cluster 1 times (in hours)",xaxt="n")
plot(times1c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "")
rose.diag(times1c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times1c, bw=100),lwd=2,lty=2, col="red")
rm(times1, times1c, clus1.Gy.jun)
mtext(line = -1,"Cluster 1 Gympie June 2015")

# cluster 2 June for Gympie wind
clus2.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==2 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="jun")
times2 <- cluster.list$hour.sequence[c(clus2.Gy.jun)]

times2c <- circular(times2, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
plot(times2c, pch=20, xlab = "Observation number", 
     ylab = "Cluster 1 times (in hours)",xaxt="n")
plot(times2c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "")
rose.diag(times2c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times2c, bw=100),lwd=2,lty=2, col="red")
mtext(line = -1,"Cluster 2 Gympie June 2015")

# cluster 3 June for Gympie quiet + some birds
clus3.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==3 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="jun")
times3 <- cluster.list$hour.sequence[c(clus3.Gy.jun)]
times3c <- circular(times3, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
plot(times3c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times3c, bin=12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times3c, bw=100),lwd=2,lty=2, col="red")
mtext(line = 4,"Cluster 3 Gympie June 2015")

# cluster 4 june Gympie birds (afternoon)
clus4.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==4 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="jun")
times4 <- cluster.list$hour.sequence[c(clus4.Gy.jun)]

times4c <- circular(times4, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times4c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 1 times (in hours)",xaxt="n")
plot(times4c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times4c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times4c, bw=100),lwd=2,lty=2, col="red")
mtext(line = -3,"Cluster 4 Gympie June 2015")

# cluster 5 june Gympie quiet + movement
clus5.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==5 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month==months[i])
times5 <- cluster.list$hour.sequence[c(clus5.Gy.jun)]

times5c <- circular(times5, type= "angles", units = "hours",template = "clock24",modulo = "2pi")
#plot(times5c, pch=20, xlab = "Observation number", ylab = "Cluster 5 times (in hours)",xaxt="n")
plot(times5c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-0.6,1.5),xlim = c(-1.5,2),xaxt="n",
     cex.axis=4, cex=1,xaxt="n",xlab = "",pch = 20)
rose.diag(times5c, bins = 12, col="darkgrey",
          cex=0, prop = 1.15, add = T,ylim=c(-0.4,1.3))
lines(density.circular(times5c, bw=100),lwd=2,lty=2, col="red")
#mtext(side = 1, paste("GympieNP V5 ", months[i],sep=""),
#      cex=3, line = -5)
#dev.off()

# cluster 5 june Woondum 
#png(paste("WoondumNP V5 ", months[i],".png",sep=""),
#    width = 2000, height = 2000)
for(i in 1:(length(months)/2)) {
#  png(paste("WoondumNP V5 ", months[i],".png",sep=""),
#      width = 1200, height = 1200)
#  par(mfrow=c(5,2),oma=c(0,0,0,0),mar=c(0,0,0,0),cex=1)
  clus5.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==5 &
                          cluster.list$site=="WoondumNP" &
                          cluster.list$month==months[i])
  times5 <- cluster.list$hour.sequence[c(clus5.Gy.jun)]
  
  times5c <- circular(times5, type= "angles", units = "hours",template = "clock24",modulo = "2pi")
  #plot(times5c, pch=20, xlab = "Observation number", ylab = "Cluster 5 times (in hours)",xaxt="n")
  plot(times5c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
       ylim = c(-0.4,3.4),xlim = c(-2.9,6),xaxt="n",
       cex.axis=6, cex=1,xaxt="n",xlab = "",pch = 20)
  rose.diag(times5c, bin=12, col="darkgrey",
            cex=0, prop = 1.6, add = T)
  lines(density.circular(times5c, bw=100),lwd=5,lty=2, col="red")
  mtext(side = 1, paste("WoondumNP V5 ", months[i],sep=""),
        cex=3, line = -5)
  par(new=FALSE)
}
dev.off()
# cluster 6 june Gympie light rain 
clus6.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==6 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="jun")
times6 <- cluster.list$hour.sequence[c(clus6.Gy.jun)]

times6c <- circular(times6, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times6c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times6c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times6c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times6c, bw=100),lwd=2,lty=2, col="red")
rm(times6,times6c,clus6.Gy.jun)


# cluster 7 june Gympie insects
clus7.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==7 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="oct")
times7 <- cluster.list$hour.sequence[c(clus7.Gy.jun)]

times7c <- circular(times7, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times7c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times7c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times7c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times7c, bw=100),lwd=2,lty=2, col="red")
rm(times7,times7c,clus7.Gy.jun)

# cluster 7 oct Gympie insects
clus7.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==7 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="oct")
times7 <- cluster.list$hour.sequence[c(clus7.Gy.jun)]

times7c <- circular(times7, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times7c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times7c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times7c, bins = 12, col="darkgrey",
          cex=0, prop = 1.1, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times7c, bw=100),lwd=2,lty=2, col="red")
rm(times7,times7c,clus7.Gy.jun)

# cluster 8 june Gympie birds + some wind
clus8.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==8 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="jun")
times8 <- cluster.list$hour.sequence[c(clus8.Gy.jun)]

times8c <- circular(times8, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times8c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times8c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times8c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times8c, bw=100),lwd=2,lty=2, col="red")
rm(times8,times8c,clus8.Gy.jun)

# cluster 9 june Gympie birds (morning)
#png("Cluster9 June Gympie NP.png", height=1000, width = 1000)

# cluster 9 june Gympie
clus9.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==9 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="jun")
times9 <- cluster.list$hour.sequence[c(clus9.Gy.jun)]
times9c <- circular(times9, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
plot(times9c, bin=122, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-0.8,1),xlim = c(-1.5,2.4),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times9c, bins = 12, col="darkgrey",
          cex=0, prop = 1.2, add = T,
          xaxt="n")
lines(density.circular(times9c, bw=100),lwd=2,lty=2, col="red")
mtext(line = -3,"Cluster 9 Gympie June 2015")
rm(times9,times9c,clus9.Gy.jun)
#mtext("June 2015 V9", side = 3, line=-14,cex = 5)
dev.off()

# cluster 9 july Gympie birds (morning)
#png("Cluster9 July Gympie NP.png", height=1000,width = 1000)
clus9.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==9 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="july")
times9 <- cluster.list$hour.sequence[c(clus9.Gy.jun)]
times9 <- times9[seq(times9[1],length(times9),3)]
times9c <- circular(times9, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times9c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times9c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1,1),xlim = c(-0.8,3.2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times9c, bin=12, col="darkgrey",
          cex=0, prop = 1.2, add = T,xaxt="n")
lines(density.circular(times9c, bw=100),lwd=2,lty=2, col="red")
rm(times9,times9c,clus9.Gy.jun)
#mtext("July 2015 V9", side = 3, line=-14,cex = 5)
dev.off()

# cluster 9 aug Gympie birds (morning)
png("Cluster9 August Gympie NP.png", height=1000,width = 1000)
clus9.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==9 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="aug")
times9 <- cluster.list$hour.sequence[c(clus9.Gy.jun)]
times9 <- times9[seq(times9[1],length(times9),3)]
times9c <- circular(times9, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times9c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times9c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1,1),xlim = c(-0.8,3.2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times9c, bin=12, col="darkgrey",
          cex=0, prop = 1.2, add = T,xaxt="n")
lines(density.circular(times9c, bw=100),lwd=2,lty=2, col="red")
rm(times9,times9c,clus9.Gy.jun)
mtext("August 2015 V9", side = 3, line=-14,cex = 5)
dev.off()

# cluster 9 sept Gympie birds (morning)
png("Cluster9 Sept Gympie NP.png", height=1000,width = 1000)
clus9.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==9 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="sept")
times9 <- cluster.list$hour.sequence[c(clus9.Gy.jun)]
times9 <- times9[seq(times9[1],length(times9),3)]
times9c <- circular(times9, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times9c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times9c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1,1),xlim = c(-0.8,3.2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times9c, bin=12, col="darkgrey",
          cex=0, prop = 1.2, add = T,xaxt="n")
lines(density.circular(times9c, bw=100),lwd=2,lty=2, col="red")
rm(times9,times9c,clus9.Gy.jun)
mtext("Sept 2015 V9", side = 3, line=-14,cex = 5)
dev.off()

# cluster 9 oct Gympie birds (morning)
png("Cluster9 Oct Gympie NP.png", height=1000,width = 1000)
clus9.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==9 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="oct")
times9 <- cluster.list$hour.sequence[c(clus9.Gy.jun)]

times9c <- circular(times9, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times9c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times9c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1,1),xlim = c(-0.8,3.2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times9c, bin=12, col="darkgrey",
          cex=0, prop = 1.2, add = T,xaxt="n")
lines(density.circular(times9c, bw=100),lwd=2,lty=2, col="red")
rm(times9,times9c,clus9.Gy.jun)
mtext("Oct 2015 V9", side = 3, line=-14,cex = 5)
dev.off()


# cluster 10 june Gympie birds + wind
clus10.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==10 &
                        cluster.list$site=="GympieNP" &
                        cluster.list$month=="jun")
times10 <- cluster.list$hour.sequence[c(clus10.Gy.jun)]

times10c <- circular(times10, type= "angles", units = "hours",
                    template = "clock24",modulo = "2pi")
#plot(times10c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times10c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times10c, bin=12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times10c, bw=100),lwd=2,lty=2, col="red")
rm(times10,times10c,clus10.Gy.jun)

# cluster 11 june Gympie planes in quiet background
clus11.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==11 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times11 <- cluster.list$hour.sequence[c(clus11.Gy.jun)]
times11c <- circular(times11, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
#plot(times11c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times11c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times11c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times11c, bw=100),lwd=2,lty=2, col="red")
rm(times11,times11c,clus11.Gy.jun)

# cluster 11 july Gympie planes in quiet background
clus11.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==11 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="july")
times11 <- cluster.list$hour.sequence[c(clus11.Gy.jun)]
times11c <- circular(times11, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
#plot(times11c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times11c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-3,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times11c, bins = 12, col="darkgrey",
          cex=0, prop = 1.4, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times11c, bw=100),lwd=2,lty=2, col="red")
rm(times11,times11c,clus11.Gy.jun)

# cluster 12 june Gympie planes
clus12.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==12 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times12 <- cluster.list$hour.sequence[c(clus12.Gy.jun)]
times12c <- circular(times12, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
#plot(times12c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times12c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times12c, bin=12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times12c, bw=100),lwd=2,lty=2, col="red")
rm(times12,times12c,clus12.Gy.jun)

# cluster 13 june Gympie planes
clus13.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==13 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times13 <- cluster.list$hour.sequence[c(clus13.Gy.jun)]
times13c <- circular(times13, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
#plot(times13c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times13c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-2,0.8),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times13c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times13c, bw=100),lwd=2,lty=2, col="red")
rm(times13,times13c,clus13.Gy.jun)


# cluster 14 june Gympie birds (morning)
clus14.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==14 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times14 <- cluster.list$hour.sequence[c(clus14.Gy.jun)]
times14c <- circular(times14, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
#plot(times14c, pch=20, xlab = "Observation number", 
#     ylab = "Cluster 6 distribution (in hours)",xaxt="n")
plot(times14c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times14c, bin=12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times14c, bw=100),lwd=2,lty=2, col="red")
rm(times14,times14c,clus14.Gy.jun)


# cluster 15 june Gympie wind in trees
clus15.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==15 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times15 <- cluster.list$hour.sequence[c(clus15.Gy.jun)]
times15c <- circular(times15, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times15c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times15c, bin=12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times15c, bw=100),lwd=2,lty=2, col="red")
rm(times15,times15c,clus15.Gy.jun)


# cluster 16 june Gympie birds
clus16.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==16 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times16 <- cluster.list$hour.sequence[c(clus16.Gy.jun)]
times16c <- circular(times16, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times16c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times16c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times16c, bw=100),lwd=2,lty=2, col="red")
rm(times16,times16c,clus16.Gy.jun)

# cluster 17 june Gympie consistent rain
clus17.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==17 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times17 <- cluster.list$hour.sequence[c(clus17.Gy.jun)]
times17c <- circular(times17, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times17c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times17c, bins = 12, col="darkgrey",
          cex=0, prop = 1.2, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times17c, bw=100),lwd=2,lty=2, col="red")
rm(times17,times17c,clus17.Gy.jun)

# cluster 18 june Gympie wind in trees + insects
clus18.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==18 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times18 <- cluster.list$hour.sequence[c(clus18.Gy.jun)]
times18c <- circular(times18, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times18c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times18c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times18c, bw=100),lwd=2,lty=2, col="red")
rm(times18,times18c,clus18.Gy.jun)

# cluster 19 june Gympie quiet + some insects
clus19.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==19 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times19 <- cluster.list$hour.sequence[c(clus19.Gy.jun)]
times19c <- circular(times19, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times19c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times19c, bins = 12, col="darkgrey",
          cex=0, prop = 1.4, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times19c, bw=100),lwd=2,lty=2, col="red")
rm(times19,times19c,clus19.Gy.jun)


# cluster 20 june Gympie quiet + birds + insects
clus20.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==20 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times20 <- cluster.list$hour.sequence[c(clus20.Gy.jun)]
times20c <- circular(times20, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times20c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times20c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times20c, bw=100),lwd=2,lty=2, col="red")
rm(times20,times20c,clus20.Gy.jun)


# cluster 21 june Gympie light wind + birds
clus21.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==21 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times21 <- cluster.list$hour.sequence[c(clus21.Gy.jun)]
times21c <- circular(times21, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times21c, bin=144, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times21c, bins = 12, col="darkgrey",
          cex=0, prop = 1.6, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times21c, bw=100),lwd=2,lty=2, col="red")
rm(times21,times21c,clus21.Gy.jun)


# cluster 22 june Gympie birds (afternoon)
clus22.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==22 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times22 <- cluster.list$hour.sequence[c(clus22.Gy.jun)]
times22c <- circular(times22, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times22c, bin=122, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times22c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times22c, bw=100),lwd=2,lty=2, col="red")
mtext(line = -3,"Cluster 22 Gympie June 2015")
rm(times22,times22c,clus22.Gy.jun)


# cluster 23 june Gympie birds(morning)
clus23.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==23 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times23 <- cluster.list$hour.sequence[c(clus23.Gy.jun)]
times23c <- circular(times23, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times23c, bin=123, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times23c, bins = 12, col="darkgrey",
          cex=0, prop = 1.3, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times23c, bw=100),lwd=2,lty=2, col="red")
rm(times23,times23c,clus23.Gy.jun)

# cluster 24 june Gympie quiet some insects
clus24.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==24 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times24 <- cluster.list$hour.sequence[c(clus24.Gy.jun)]
times24c <- circular(times24, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times24c, bin=124, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times24c, bin=12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times24c, bw=100),lwd=2,lty=2, col="red")
rm(times24,times24c,clus24.Gy.jun)


# cluster 25 june Gympie Very quiet
clus25.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==25 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times25 <- cluster.list$hour.sequence[c(clus25.Gy.jun)]
times25c <- circular(times25, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times25c, bin=125, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "")
rose.diag(times25c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times25c, bw=100),lwd=2,lty=2, col="red")
rm(times25,times25c,clus25.Gy.jun)


# cluster 26 june Gympie birds (morning)
clus26.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==26 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times26 <- cluster.list$hour.sequence[c(clus26.Gy.jun)]
times26c <- circular(times26, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times26c, bin=126, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times26c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times26c, bw=100),lwd=2,lty=2, col="red")
rm(times26,times26c,clus26.Gy.jun)


# cluster 27 june Gympie very loud
clus27.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==27 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times27 <- cluster.list$hour.sequence[c(clus27.Gy.jun)]
times27c <- circular(times27, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times27c, bin=127, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times27c, bins = 12, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times27c, bw=100),lwd=2,lty=2, col="red")
rm(times27,times27c,clus27.Gy.jun)


# cluster 28 june Gympie wind + birds
clus28.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==28 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times28 <- cluster.list$hour.sequence[c(clus28.Gy.jun)]
times28c <- circular(times28, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times28c, bin=128, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "")
rose.diag(times28c, bins = 28, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times28c, bw=100),lwd=2,lty=2, col="red")
rm(times28,times28c,clus28.Gy.jun)


# cluster 29 june Gympie very light rain (drizzle)
clus29.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==29 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times29 <- cluster.list$hour.sequence[c(clus29.Gy.jun)]
times29c <- circular(times29, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times29c, bin=129, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "", pch=20)
rose.diag(times29c, bins = 29, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times29c, bw=100),lwd=2,lty=2, col="red")
rm(times29,times29c,clus29.Gy.jun)

# cluster 30 june Gympie insects
clus30.Gy.jun <- which(cluster.list$hybrid_k17500k30k3==30 &
                         cluster.list$site=="GympieNP" &
                         cluster.list$month=="jun")
times30 <- cluster.list$hour.sequence[c(clus30.Gy.jun)]
times30c <- circular(times30, type= "angles", units = "hours",
                     template = "clock24",modulo = "2pi")
plot(times30c, bin=130, stack=TRUE, sep= 0.035, shrink=1,
     ylim = c(-1.5,1.5),xlim = c(-2,2),xaxt="n",cex.axis=0.5,
     xaxt="n",xlab = "")
rose.diag(times30c, bins = 30, col="darkgrey",
          cex=0, prop = 1.5, add = T,ylim=c(-1.5,1.5),
          xaxt="n")
lines(density.circular(times30c, bw=100),lwd=2,lty=2, col="red")
rm(times30,times30c,clus30.Gy.jun)
