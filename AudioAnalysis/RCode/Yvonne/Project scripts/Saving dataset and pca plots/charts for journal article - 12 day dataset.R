#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 1 -----------------------------
# was produced in illustrator and photoshop
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 3 -----------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#png(filename = "Journal_article_plot1.png",
#    width = 1200, height = 600, units = "px")
tiff("Figures for plos article/Fig3.tiff", width = 2250, height = 1125, units = 'px', res = 300)
x <- c(6,8,10,12,14,16,18,20,22,24,26,28,30)
y <- c(2.036, 1.255, 1.306, 1.291, 
       1.302, 1.347, 1.933, 1.92 ,
       1.916, 1.931, 1.935, 1.983, 2.022)
xmin <- 5
xmax <- 35
ymin <- 1.2
ymax <- 2.2
xlim <- c(xmin, xmax)
ylim <- c(ymin, ymax)
par(mar=c(3, 3, 2.4, 0.4), mfcol=c(1,2) ,
    cex = 0.6, cex.axis = 1, cex.main = 1.2)
plot(x, y, type = "b", xlab = "k", pch = 20,
     ylab = "ID3 distance",
     main = "Intra-three-day distance
     k-means and hclust ", mgp =c(2,0.8,0),
     xlim = xlim, ylim = ylim)
#%%%%%%%%%%%%%%%%%%%%%%%%
I3D.average <- c(1.527, 1.408, 1.523, 1.585, 1.580, 1.513)
I3D.wardD2 <- c(1.766, 1.360, 1.381, 1.763, 1.738, 1.701)
x <- c(5, 10, 15, 20, 25, 30)
par(new=TRUE)
plot(x,I3D.average, type = "b", pch=0,
     xlab = "", ylab = "", axes = 0,
     xlim = xlim, ylim = ylim)
par(new=TRUE, xpd=TRUE)
plot(x,I3D.wardD2,type = "b", pch=5,
     xlim = xlim, ylim = ylim, axes = 0, mgp = c(2,0.8,0),
     yaxt="n", ylab = "", xlab = "", xaxt ="n")
legend(x=25, y=2.23, pch = c(20,0,5), 
       c("k-means", "hclust (average)","hclust (ward.D2)"),
       bty = "n", cex = 1.2)
text(0.6, 2.3, "a.", adj = c(0,0), cex = 1.2)
#dev.off()
#$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
x <- c(5,10,15,20,22,24,26,28,30)#,35,40,45)
hybrid1000 <- c(1.915,1.292,1.252,1.735,1.521,1.519,1.471,1.482,1.484)#,1.494,1.592,1.601)
hybrid1500<- c(1.789,1.292,1.305,1.334,1.419,1.416,1.478,1.414,1.402)#,1.512,1.878,2.034)
hybrid2000 <- c(1.802,1.279,1.314,1.696,1.450,1.450,1.429,1.449,1.456)#,1.477,1.476,1.654)
hybrid2500<- c(1.858,1.354,1.320,1.357,1.384,1.363,1.349,1.482,1.493)#,1.508,1.568,1.737)
hybrid3000 <- c(1.677,1.299,1.305,1.310,1.406,1.369,1.375,1.385,1.389)#,1.382,1.482,1.708)
hybrid3500<- c(1.681,1.367,1.37,1.354,1.682,1.69,1.706,2.012,2.024)#,1.681,1.824,2.184)
#png("hybrid 12 day_2percent.png",height = 600,width = 600)
#par(new=TRUE)
plot(x, hybrid2500, type = "b", pch=17,
     xlim = xlim, ylim = ylim, 
     ylab = "ID3 distance", xlab = "k2", mgp =c(2,0.8,0),
     main = "Intra-three-day distance
     hybrid method")
par(new=TRUE)
plot(x,hybrid3000, type = "b", pch = 19, axes = 0,
     yaxt = "n", ylab = "", xlab = "",
     xlim = xlim, ylim = ylim)
par(new=TRUE)
plot(x, hybrid3500, type = "b", pch=15, axes = 0,
     yaxt = "n", ylab = "", xlab = "", xlim = xlim, ylim = ylim)

legend(x=30, y=2.23, pch = c(17, 19, 15), title = "k1", #, 18), 
       c("2500", "3000", "3500"),
       bty = "n", cex = 1.2)
text(0.6, 2.3, "b.", adj = c(0,0), cex = 1.2)
dev.off()

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# plot 4 ------------------------------
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

ID3_values <- read.csv("ID3_values.csv")
col <- rep("black", 8)
pch = c(15,20,17,18,16,21,22,23)
labels <- as.character(seq(12500, 30000, 2500))
x <- seq(5, 100, 5)
ylimit <- c(1.1, 2.6)#c(1.1, 2.3)
xlimit <- c(40, 100)

tiff("Figures for plos article/Fig4.tiff", width = 1250, height = 1250, units = 'px', res = 300)
par(mar=c(3, 3, 2.4, 0.4), 
    cex = 0.6, cex.axis = 1, cex.main = 1)

plot(x,ID3_values[41:60,3], type = "o", 
     col=col, pch=pch[2], ylim=ylimit,
     xlab = "k2", ylab = "ID3 distance", las=1, 
     xlim = xlimit, mgp = c(2.1, 0.6, 0))
mtext("ID3 distance (clustering of 24 hour fingerprints)", side = 3, 
      line = 1.2, cex = 0.8)
mtext("12 days out of 796 days", side = 3, 
      line = 0.3, cex = 0.6)
par(new=T)
plot(x,ID3_values[61:80,3], type = "o", 
     col=col[3], pch=pch[3], ylim=ylimit,
     yaxt="n",xaxt="n", xlab = "", 
     ylab = "", xlim = xlimit)
par(new=T)
plot(x,ID3_values[81:100,3], type = "o", 
     col=col[4], pch=pch[4], ylim=ylimit,
     yaxt="n",xaxt="n", xlab = "", 
     ylab = "", xlim = xlimit)
par(new=T)
plot(x,ID3_values[101:120,3], type = "o", 
     col=col[5], pch=pch[5], ylim=ylimit,
     yaxt="n",xaxt="n", xlab = "", 
     ylab = "", xlim = xlimit)
par(new=T)
plot(x,ID3_values[121:140,3], type = "o", 
     col=col[6], pch=pch[6], ylim=ylimit,
     yaxt="n",xaxt="n", xlab = "", 
     ylab = "", xlim = xlimit)
legend("topright", pch = pch[2:6], lty = c(1),
       title="k1 values", col = col[3:7], bty = "n", 
       cex=1.2, labels[3:7], y.intersp = 0.85) 
dev.off()
#rm(ID3_values)

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# plot 6 Sammon map ------------------------------
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

library(cluster) # needed for pam function in order to find the medoids

# plot SAMMON projection
library(MASS)
# see below on how this is generated
medoids1 <- read.csv("medoids_all_data.csv", header = T)
distances <- as.matrix(dist(medoids1))
clusters.sam <- sammon(distances, k=2)

# size of clusters
clust_sizes <- read.csv("data/2hour_plots_25000_60/Summary_25000_60_annotated.csv", header = T)
clust_sizes <- clust_sizes$Cluster.total
clust_sizes1 <- clust_sizes

clusters1 <- NULL
clusters1$clusters <- as.numeric(1:length(colours))
clusters1$points1 <- clusters.sam$points[,1]
clusters1$points2 <- clusters.sam$points[,2]
clusters1$size <- clust_sizes[1:60]
clusters1$colours <- NULL
clusters1 <- data.frame(clusters1)
clusters1$clusters <- as.numeric(1:length(clusters1$points1))
clusters1$radius <- sqrt(clusters1$size)

# colours for each class
insects <- "#F0E442"
rain <- "#0072B2"
wind <- "#56B4E9"
birds <- "#009E73"
cicadas <- "#E69F00"
quiet <- "#999999"
planes <- "#CC79A7"

clusters1$colours <- "abcd"
clusters1$border <- "abcd"
#clusters1$colours[1] <- insects
# set the circle and border colours
clusters1[1, 6:7]  <-  c(insects, insects)
clusters1[2, 6:7]  <-  c(rain, birds)
clusters1[3, 6:7]  <-  c(birds, birds)
clusters1[4, 6:7]  <-  c(insects, birds)
clusters1[5, 6:7]  <-  c(quiet, quiet)
clusters1[6, 6:7]  <-  c(quiet, quiet)
clusters1[7, 6:7]  <-  c(cicadas, birds)
clusters1[8, 6:7]  <-  c(cicadas, birds)
clusters1[9, 6:7]  <-  c(wind, wind)
clusters1[10, 6:7]  <-  c(rain, rain)
clusters1[11, 6:7]  <-  c(birds, birds)
clusters1[12, 6:7]  <-  c(cicadas, cicadas)
clusters1[13, 6:7]  <-  c(quiet, quiet)
clusters1[14, 6:7]  <-  c(birds, birds)
clusters1[15, 6:7]  <-  c(birds, birds)
clusters1[16, 6:7]  <-  c(cicadas, cicadas)
clusters1[17, 6:7]  <-  c(rain, insects)
clusters1[18, 6:7]  <-  c(rain, rain)
clusters1[19, 6:7]  <-  c(wind, wind)
clusters1[20, 6:7]  <-  c(wind, wind)
clusters1[21, 6:7]  <-  c(rain, rain)
clusters1[22, 6:7]  <-  c(insects, birds)
clusters1[23, 6:7]  <-  c(planes, planes)
clusters1[24, 6:7]  <-  c(wind, cicadas)
clusters1[25, 6:7]  <-  c(wind, wind)
clusters1[26, 6:7]  <-  c(insects, wind)
clusters1[27, 6:7]  <-  c(insects, insects)
clusters1[28, 6:7]  <-  c(birds, insects)
clusters1[29, 6:7]  <-  c(insects, insects)
clusters1[30, 6:7]  <-  c(wind, quiet)
clusters1[31, 6:7]  <-  c(quiet, quiet)
clusters1[32, 6:7]  <-  c(cicadas, cicadas)
clusters1[33, 6:7]  <-  c(birds, birds)
clusters1[34, 6:7]  <-  c(cicadas, cicadas)
clusters1[35, 6:7]  <-  c(quiet, quiet)
clusters1[36, 6:7]  <-  c(quiet, planes)
clusters1[37, 6:7]  <-  c(birds, birds)
clusters1[38, 6:7]  <-  c(quiet, quiet)
clusters1[39, 6:7]  <-  c(birds, planes)
clusters1[40, 6:7]  <-  c(wind, birds)
clusters1[41, 6:7]  <-  c(quiet, quiet)
clusters1[42, 6:7]  <-  c(wind, wind)
clusters1[43, 6:7]  <-  c(birds, birds)
clusters1[44, 6:7]  <-  c(cicadas, cicadas)
clusters1[45, 6:7]  <-  c(wind, planes)
clusters1[46, 6:7]  <-  c(wind, wind)
clusters1[47, 6:7]  <-  c(wind, wind)
clusters1[48, 6:7]  <-  c(cicadas, cicadas)
clusters1[49, 6:7]  <-  c(planes, planes)
clusters1[50, 6:7]  <-  c(quiet, insects)
clusters1[51, 6:7]  <-  c(wind, wind)
clusters1[52, 6:7]  <-  c(wind, wind) 
clusters1[53, 6:7]  <-  c(quiet, quiet)
clusters1[54, 6:7]  <-  c(rain, birds)
clusters1[55, 6:7]  <-  c(quiet, quiet)
clusters1[56, 6:7]  <-  c(wind, wind)
clusters1[57, 6:7]  <-  c(birds, wind) 
clusters1[58, 6:7]  <-  c(birds, birds)
clusters1[59, 6:7]  <-  c(rain, rain) 
clusters1[60, 6:7]  <-  c(rain, birds) 
clusters1 <- clusters1[order(-clusters1$size),]
leg_col <- as.character(c(rain, birds, cicadas, wind, planes, quiet, insects))
leg_names <- c("rain", "birds", "cicadas", "wind", "planes", "quiet","insects")
library(plotrix) # needed for draw.cirle function
max <- 0.0009

tiff("Figures for plos article/Fig6.tiff", width = 2250, 
     height = 1500, units = 'px', res = 300)
par(mar=c(1.5, 1.8, 1, 0.4), 
    cex = 1, cex.axis = 1, cex.main = 2.4)
plot(clusters1$points1, 
     clusters1$points2, type = "n",
     main = "Sammon map - sixty clusters",
     xlab = "x",ylab = "", mgp =c(2,0.5,0),
     xlim = c((min(clusters1$points1)-0.05),
              (max(clusters1$points1)+0.45)),
     ylim = c((min(clusters1$points2)-0.11),
              (max(clusters1$points2)+0.13)),
     cex.axis=1, cex.lab=0.6, las=1, cex.main=1,
     bg = "transparent")
mtext(side=2, "y", las=1, cex = 3, line = 3.5)
#abline(h = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
#abline(v = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
for(i in 1:nrow(clusters1)) {
  draw.circle(clusters1$points1[i],
              clusters1$points2[i], 
              radius = max*clusters1$radius[i],
              col = clusters1$colours[i],
              border = clusters1$border[i],
              lwd = 6)
}
# plot the x and y axis to form four quadrants
abline(h = 0, col = "gray50", lwd = 0.4)
abline(v = 0, col = "gray50", lwd = 0.4)
# plot the cluster numbers
text(clusters1$points1, clusters1$points2, 
     labels = as.character(clusters1$clusters), cex = 0.84)
# plot the plot legend
a <-legend("topright", title="Classes", 
           col = leg_col, bty = "n", 
           cex=1.1, leg_names , y.intersp = 1.2) 
for(j in 1:length(a$text$x)) {
  draw.circle(a$text$x[j]-0.06, a$text$y[j]-0.005, 
              radius = 0.035,
              col = leg_col[j],
              border = "white")
}
# add family to fonts list use windowsFonts() to check current
windowsFonts(A = windowsFont("Times New Roman"))
text(x = 1.1, y = 1.1, "I", cex = 1, family="A", font = 2)
text(x = -1.6, y = 1.1, "II", cex = 1, family="A", font = 2)
text(x = -1.6, y = -1.05, "III", cex = 1, family="A", font = 2)
text(x = 1.1, y = -1.05, "IV", cex = 1, family="A", font = 2)
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 7 Two hour plots  --------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
rm(list = ls())
#setwd("C:/Work/Projects/Twelve_month_clustering/Saving_dataset")

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)

site1 <- rep("GympieNP", nrow(cluster_list)/2)
site2 <- rep("WoondumNP", nrow(cluster_list)/2)
site <- c(site1, site2)

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20160723", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)
# duplicate dates 1440 times
dates <- rep(dates, each = 1440)
dates <- rep(dates, 2)
# Add site and dates columns to dataframe
cluster_list <- cbind(cluster_list, site, dates)

# determine the number of days in each month at each site
days_per_month <- NULL
year_month <- unique(substr(cluster_list$dates,1,6))
for(i in 1:length(year_month)) {
  count <- which(substr(cluster_list$dates, 1, 6)==year_month[i])
  count <- length(count)/1440
  days_per_month <- c(days_per_month, count/2)
}
days_per_period <- rep(days_per_month, each =12)
days_per_period <- rep(days_per_period, 2)

# Assign time periods to minute of the day
cluster_list$period <- 0
periods <- seq(0, 1440, 120)
for(i in 1:(length(periods)-1)) {
  a <- which(cluster_list$minute_reference > periods[i]-1 
             & cluster_list$minute_reference < periods[i+1])
  if(i < 10) {cluster_list$period[a] <- paste("0", i, sep = "")}
  if(i >= 10) {cluster_list$period[a] <- paste(i, sep = "")}
}

#cluster_list$site_yrMth_per <- 0
cluster_list$site_yrMth_per <- paste(cluster_list$site, 
                                     substr(cluster_list$dates,1,6),
                                     cluster_list$period, 
                                     sep = "_")

a <- table(cluster_list$cluster_list, cluster_list$site_yrMth_per)

a <- as.data.frame(a)

# replace the NAs with 1000 to track these minutes
NA_ref <- which(is.na(cluster_list$cluster_list)==TRUE)
cluster_list$cluster_list[NA_ref] <- 1000
# Create a 3 column table containing variables and frequencies
a <- table(cluster_list$cluster_list, cluster_list$site_yrMth_per)
a <- as.data.frame(a)

# produce list of when the nas occured
z <- which(a$Var1==1000)
na_reference <- a[z,3]

# colours for each class
insects <- "#F0E442"
rain <- "#0072B2"
wind <- "#56B4E9"
birds <- "#009E73"
cicadas <- "#E69F00"
quiet <- "#999999"
planes <- "#CC79A7"

plot_funct <- function(clust_num, colour) {
  y <- which(a$Var1==clust_num)
  cluster_reference <- a[y,3]
  cluster_reference <- cbind(cluster_reference, 
                             days_per_period,
                             na_reference)
  cluster_reference <- as.data.frame(cluster_reference)
  cluster_reference$output <- cluster_reference$cluster_reference/
    (cluster_reference$days_per_period - (cluster_reference$na_reference/120))
  cluster_reference$output <- round(cluster_reference$output,2)
  
  months <- unique(substr(cluster_list$dates,1,6))
  for(i in 1:length(months)) {
    if(substr(months[i],5,6)=="01") {months[i] <- paste("JAN", substr(months[i],3,4), sep=" ")}
    if(substr(months[i],5,6)=="02") {months[i] <- paste("FEB", substr(months[i],3,4), sep=" ")}
    if(substr(months[i],5,6)=="03") {months[i] <- paste("MAR", substr(months[i],3,4), sep=" ")}  
    if(substr(months[i],5,6)=="04") {months[i] <- paste("APR", substr(months[i],3,4), sep=" ")}  
    if(substr(months[i],5,6)=="05") {months[i] <- paste("MAY", substr(months[i],3,4), sep=" ")}
    if(substr(months[i],5,6)=="06") {months[i] <- paste("JUN", substr(months[i],3,4), sep=" ")}
    if(substr(months[i],5,6)=="07") {months[i] <- paste("JUL", substr(months[i],3,4), sep=" ")}  
    if(substr(months[i],5,6)=="08") {months[i] <- paste("AUG", substr(months[i],3,4), sep=" ")}  
    if(substr(months[i],5,6)=="09") {months[i] <- paste("SEP", substr(months[i],3,4), sep=" ")}
    if(substr(months[i],5,6)=="10") {months[i] <- paste("OCT", substr(months[i],3,4), sep=" ")}
    if(substr(months[i],5,6)=="11") {months[i] <- paste("NOV", substr(months[i],3,4), sep=" ")}  
    if(substr(months[i],5,6)=="12") {months[i] <- paste("DEC", substr(months[i],3,4), sep=" ")}  
  }
  started <- seq(1,nrow(cluster_reference),12)
  finished <- started + 11
  
  months <- rep(months, 2)
  
  num_of_plots <- length(started)
  max <- max(cluster_reference$output)
  for(i in 15:28) { # num_of_plots should be even
    # The y-axis labels are plotted for the 1st and 15th plots
    num1 <- c(1,15)
    if(i %in% num1) {
      barplot(cluster_reference$output[started[i]:finished[i]],
              ylim=c(0, max), col = colour, xlab = "", las=1,
              mgp = c(1, 0.4, 0), tck = - 0.05)
      Axis(side = 1, labels = FALSE, 
           at = seq.int(0.4, 14, length.out = 12),
           cex = 0.4, tck = -0.05, mgp = c(1,0.4,0))
    }
    num2 <- c(2:14,16:28) 
    if(i %in% num2) {
      barplot(cluster_reference$output[started[i]:finished[i]],
              ylim=c(0,max), col = colour, xlab = "", axes=FALSE,
              mgp = c(1,0.5,0), tck = - 0.05)
      Axis(side = 2, labels=FALSE, tck = -0.05, mgp = c(1,0.4,0))
      Axis(side = 1, labels = FALSE, tck = -0.05,
           at = seq.int(0.8, 14, length.out = 12), mgp = c(1,0.4,0))
    }
    mtext(side = 3, paste(months[i]), line = 0.3, cex = 0.7)
    mtext(side = 1, text = c(as.character(seq(0,24,12))), 
          at = c(1,7,13), line=-0.1, cex = 0.5)
  }
}
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# plot 7 ------------------------------
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
tiff("Figures for plos article/Fig7.tiff", width = 2250, 
     height = 1500, units = 'px', res = 300)
#par(mar=c(2, 2.5, 2, 0.4), mfrow = c(4,1),
#    cex = 1, cex.axis = 1, cex.main = 2.4)
par(mfrow=c(4, 14), 
    mar=c(1, 0, 2, 0.1), oma=c(0.1, 2.1, 0, 0), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)

# Start insect image
clust_num <- 29
colour <- insects
plot_funct(clust_num, colour)
mtext(side = 3, line = 1, "INSECTS - Cluster 29                                                                                                                                 ", cex=1.1)

# Start Bird image
clust_num <- 37
colour <- birds
plot_funct(clust_num, colour)
mtext(side = 3, line = 1, "BIRDS - Cluster 37                                                                                                                                 ", cex=1.1)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of minutes in 2 hour period per month")

# Start cicada image
clust_num <- 48
colour <- cicadas
plot_funct(clust_num, colour)
mtext(side = 3, line = 1, "CICADAS - Cluster 48                                                                                                                                 ", cex=1.1)

# Start quiet image
cluster <- 13
colour <- quiet
plot_funct(cluster, colour)
mtext(side = 3, line = 1, "QUIET - Cluster 13                                                                                                                                 ", cex=1.1)

dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 8 Rose plots ------------------------------------------
# remove all objects in the global environment
rm(list = ls())

# Set the variable of cluster number
# choose a cluster, clust is used to label the plots
clust <- "cluster37"
clust <- "cluster44"
clust <- "cluster48"

# Choose a colour from this list
# colours for each class
insect_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
bird_col <- "#009E73"
cicada_col <- "#E69F00"
quiet_col <- "#999999"
plane_col <- "#CC79A7"
na_col <- "white"

if(clust=="cluster37") {
  col <- bird_col
  scale <- c(20, 20)
  selection <- 17:21 # This selects August 2015 to December 2015
  circleProportion <- 0.5
}
if(clust=="cluster44"|clust=="cluster48") {
  col <- cicada_col
  circleProportion <- 1
  if(clust=="cluster44") {
    scale <- c(18, 18)
    selection <- 21:23 # This selects December 2015 to February 2015
  }
  if(clust=="cluster48") {
    scale <- c(12, 12)
    selection <- 21:23
  }
}

# Christophe Ladroue
library(plyr)
library(ggplot2)

polarHistogram <-function (df, family = NULL, 
                           columnNames = NULL, 
                           binSize = 1,
                           spaceItem = 0.2, 
                           spaceFamily = 0, 
                           innerRadius = 0, 
                           outerRadius = 10,
                           guides = c(), 
                           alphaStart = 0, #-0.1, 
                           circleProportion = 0.5,
                           direction = "outwards", 
                           familyLabels = FALSE, 
                           normalised = FALSE,
                           labels=FALSE,
                           units = "cm",
                           colour = NULL)
{
  if (!is.null(columnNames)) {
    namesColumn <- names(columnNames)
    names(namesColumn) <- columnNames
    df <- rename(df, namesColumn)
  }
  
  applyLookup <- function(groups, keys, unassigned = "unassigned") {
    lookup <- rep(names(groups), sapply(groups, length, USE.NAMES = FALSE))
    names(lookup) <- unlist(groups, use.names = FALSE)
    p <- lookup[as.character(keys)]
    p[is.na(p)] <- unassigned
    p
  }
  
  if (!is.null(family))
    df$family <- applyLookup(family, df$item)
  # not useful for date-time data
  #df <- arrange(df, family, item, score)
  if(normalised)
    df <- ddply(df, .(family, item), transform, 
                value = cumsum(value/(sum(value))))
  else {
    maxFamily <- max(plyr::ddply(df,.(family,item), summarise, total = sum(value))$total)
    df <- ddply(df, .(family, item), transform, value = cumsum(value))
    df$value <- df$value/maxFamily
  }
  
  df <- ddply(df, .(family, item), transform, previous = c(0, head(value, length(value) - 1)))
  
  df2 <- ddply(df, .(family, item), summarise, indexItem = 1)
  df2$indexItem <- cumsum(df2$indexItem)
  df3 <- ddply(df, .(family), summarise, indexFamily = 1)
  df3$indexFamily <- cumsum(df3$indexFamily)
  df <- merge(df, df2, by = c("family", "item"))
  df <- merge(df, df3, by = "family")
  df <- arrange(df, family, item, score)
  
  affine <- switch(direction,
                   inwards = function(y) (outerRadius - innerRadius) * y + innerRadius,
                   outwards = function(y) (outerRadius - innerRadius) * (1 - y) + innerRadius,
                   stop(paste("Unknown direction")))
  df <- within(df, {
    xmin <- (indexItem - 1) * binSize + (indexItem - 1) *
      spaceItem + (indexFamily - 1) * (spaceFamily - spaceItem)
    xmax <- xmin + binSize
    ymin <- affine(1 - previous)
    ymax <- affine(1 - value)
  })
  
  if(normalised)
    guidesDF <- data.frame(xmin = rep(df$xmin, length(guides)),
                           y = rep(1 - guides/100, 1, each = nrow(df)))
  else
    guidesDF <- data.frame(xmin = rep(df$xmin, length(guides)),
                           y = rep(1 - guides/maxFamily, 1, each = nrow(df)))
  
  guidesDF <- within(guidesDF, {
    xend <- xmin + binSize
    y <- affine(y)
  })
  
  totalLength <- tail(df$xmin + binSize + spaceFamily, 1)/circleProportion - 0
  
  p <- ggplot(df) + geom_rect(aes(xmin = xmin, xmax = xmax,
                                  ymin = ymin, ymax = ymax, fill = score))
  readableAngle <- function(x) {
    angle <- x * (-360/totalLength) - alphaStart * 180/pi + 90
    angle + ifelse(sign(cos(angle * pi/180)) + sign(sin(angle * pi/180)) == -2, 180, 0)
  }
  
  readableJustification <- function(x) {
    angle <- x * (-360/totalLength) - alphaStart * 180/pi + 90
    ifelse(sign(cos(angle * pi/180)) + sign(sin(angle * pi/180)) == -2, 1, 0)
  }
  
  dfItemLabels <- ddply(df, .(family, item), summarize, xmin = xmin[1])
  dfItemLabels <- within(dfItemLabels, {
    x <- xmin + binSize/2
    angle <- readableAngle(xmin + binSize/2)
    hjust <- readableJustification(xmin + binSize/2)
  })
  
  if(labels)
    p <- p + geom_text(aes(x = x, label = item, angle = angle,
                           hjust = hjust), y = 1.02, size = 6.5, vjust = 0.5, data = dfItemLabels)
# this code prints the guidelines  
    p <- p + geom_segment(aes(x = xmin, xend = xend, y = y, yend = y),
                      colour = "white", data = guidesDF)
  
  if(normalised)
    guideLabels <- data.frame(x = 0, y = affine(1 - guides/100),
                              label = paste(guides, "% ", sep = ""))
  else
    guideLabels <- data.frame(x = 0, y = affine(1 - guides/maxFamily),
                              label = paste(guides, " ", sep = ""))
  
  p <- p + geom_text(aes(x = x, y = y, label = label), data = guideLabels,
                     angle = -alphaStart * 180/pi, hjust = 1, size = 5)
  if (familyLabels) {
    familyLabelsDF <- aggregate(xmin ~ family, data = df,
                                FUN = function(s) mean(s + binSize))
    familyLabelsDF <- within(familyLabelsDF, {
      x <- xmin
      angle <- xmin * (-360/totalLength) - alphaStart * 180/pi
    })
    p <- p + geom_text(aes(x = x, label = family, angle = angle),
                       data = familyLabelsDF, y = 1.35, size = 6.5)
  }
  
  p <- p + theme(panel.background = element_blank(), axis.title.x = element_blank(),
                 axis.title.y = element_blank(), panel.grid.major = element_blank(),
                 panel.grid.minor = element_blank(), axis.text.x = element_blank(),
                 axis.text.y = element_blank(), axis.ticks = element_blank())
  
  p <- p + xlim(0, tail(df$xmin + binSize + spaceFamily, 1)/circleProportion)
  p <- p + ylim(0, outerRadius + 0.2)
  p <- p + coord_polar(start = alphaStart)
  #p <- p + scale_fill_brewer(palette = "Set1", type = "qual")
  p <- p + scale_fill_manual(values = c('cluster0'="white",
                                        'cluster1'=colour, 'cluster2'=colour, 'cluster3'=colour, 'cluster4'=colour,
                                        'cluster5'=colour, 'cluster6'=colour, 'cluster7'=colour, 'cluster8'=colour,
                                        'cluster9'=colour, 'cluster10'=colour,'cluster11'=colour, 'cluster12'=colour,
                                        'cluster13'=colour, 'cluster14'=colour,'cluster15'=colour, 'cluster16'=colour,
                                        'cluster17'=colour, 'cluster18'=colour,'cluster19'=colour, 'cluster20'=colour,
                                        'cluster21'=colour, 'cluster22'=colour,'cluster23'=colour, 'cluster24'=colour,
                                        'cluster25'=colour, 'cluster26'=colour, 'cluster27'=colour, 'cluster28'=colour,
                                        'cluster29'=colour, 'cluster30'=colour, 'cluster31'=colour, 'cluster32'=colour,
                                        'cluster33'=colour, 'cluster34'=colour,'cluster35'=colour, 'cluster36'=colour,
                                        'cluster37'=colour, 'cluster38'=colour,'cluster39'=colour, 'cluster40'=colour,
                                        'cluster41'=colour, 'cluster42'=colour,'cluster43'=colour, 'cluster44'=colour,
                                        'cluster45'=colour, 'cluster46'=colour,'cluster47'=colour, 'cluster48'=colour,
                                        'cluster49'=colour, 'cluster50'=colour, 'cluster51'=colour, 'cluster52'=colour,
                                        'cluster53'=colour, 'cluster54'=colour, 'cluster55'=colour, 'cluster56'=colour,
                                        'cluster57'=colour, 'cluster58'=colour,'cluster59'=colour, 'cluster60'=colour))
  p <- p + theme(legend.text=element_text(size=1))
  p <- p + theme(legend.position="none")
  p <<- p
}

df <- read.csv("polarHistograms/polar_data.csv", header = T)

df$item <- as.character(df$item)
df$family <- as.character(df$family)
df$score <- as.character(df$score)
df$value <- as.numeric(df$value)

# shift the time labels by 15 minutes to correspond to the 
# middle of each time period
times<- unique(df$item)

startDate = as.POSIXct("2013-12-23 00:15:00")
endDate = as.POSIXct("2013-12-23 23:45:00")
dateSeq5sec = seq(from=startDate, to=endDate, by="1800 sec")

times_new <- substr(dateSeq5sec, 12, 16)

for(i in 1:length(times)) {
  a <- which(df$item==times[i]) 
  df$item[a] <- times_new[i]
}

#df$item <- substr(df$item, 1,5)
b <- nrow(df)
gym_df <- df[1:(b/2),]
won_df <- df[(b/2+1):b,]

a <- which(gym_df$score==clust)
gym_df <- gym_df[a,]

a <- which(won_df$score==clust)
won_df <- won_df[a,]

#a <- which(gym_df$value==23)
#m <- max(gym_df$value)

# June 2015
a <- which(gym_df$family=="a  Jun 15")
b <- c(min(a), max(a))
GympieNP_June2015 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_June2015)/48
# normalise the number of minutes
GympieNP_June2015$value <- GympieNP_June2015$value/n
# set the scale of the polar plot by setting an empty
# time slot to the scale set above
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_June2015$item==times_new[i] & GympieNP_June2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
# if a is not NULL take the last value
if(length(a) >= 1) {
  GympieNP_June2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_June2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_June2015$value)
  a <- which(GympieNP_June2015$value==min)
  GympieNP_June2015$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_June2015$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="a  Jun 15")
b <- c(min(a), max(a))
WoondumNP_June2015 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_June2015)/48
# normalise the number of minutes
WoondumNP_June2015$value <- WoondumNP_June2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_June2015$item==times_new[i] & WoondumNP_June2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_June2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_June2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_June2015$value)
  a <- which(WoondumNP_June2015$value==min)
  WoondumNP_June2015$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_June2015$score[a[1]] <- "cluster0"
}

# July 2015
a <- which(gym_df$family=="b  Jul 15")
b <- c(min(a), max(a))
GympieNP_July2015 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_July2015)/48
# normalise the number of minutes
GympieNP_July2015$value <- GympieNP_July2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_July2015$item==times_new[i] & GympieNP_July2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_July2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_July2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_July2015$value)
  a <- which(GympieNP_July2015$value==min)
  GympieNP_July2015$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_July2015$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="b  Jul 15")
b <- c(min(a), max(a))
WoondumNP_July2015 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_July2015)/48
# normalise the number of minutes
WoondumNP_July2015$value <- WoondumNP_July2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_July2015$item==times_new[i] & WoondumNP_July2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_July2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_July2015$score[a[length(a)]] <- "cluster0"
}

if(length(a) < 1) {
  min <- min(WoondumNP_July2015$value)
  a <- which(WoondumNP_July2015$value==min)
  WoondumNP_July2015$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_July2015$score[a[1]] <- "cluster0"
}

# August 2015
a <- which(gym_df$family=="c  Aug 15")
b <- c(min(a), max(a))
GympieNP_August2015 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_August2015)/48
# normalise the number of minutes
GympieNP_August2015$value <- GympieNP_August2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_August2015$item==times_new[i] & GympieNP_August2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_August2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_August2015$score[a[length(a)]] <- "cluster0"
}

if(length(a) < 1) {
  min <- min(GympieNP_August2015$value)
  a <- which(GympieNP_August2015$value==min)
  GympieNP_August2015$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_August2015$score[a[1]] <- "cluster0"
}


a <- which(won_df$family=="c  Aug 15")
b <- c(min(a), max(a))
WoondumNP_August2015 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_August2015)/48
# normalise the number of minutes
WoondumNP_August2015$value <- WoondumNP_August2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_August2015$item==times_new[i] & WoondumNP_August2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_August2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_August2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_August2015$value)
  a <- which(WoondumNP_August2015$value==min)
  WoondumNP_August2015$value[a[1]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_August2015$score[a[1]] <- "cluster0"
}

# September 2015
m_days <- c(28,29,30) # place the missing dates, must be in numeric order
a <- which(gym_df$family=="d  Sept 15")
b <- c(min(a), max(a))
GympieNP_September2015 <- gym_df[b[1]:b[2],]
if(length(m_days >= 1)) {
  for(i in 1:length(m_days)) {
    GympieNP_September2015 <- GympieNP_September2015[-(((m_days[i]-1)*48+1):(m_days[i]*48)),]    
    m_days <- m_days - 1
  }
}
# determine the number of days
n <- (nrow(GympieNP_September2015)/48) 
# normalise the number of minutes
GympieNP_September2015$value <- GympieNP_September2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_September2015$item==times_new[i] & GympieNP_September2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}

if(length(a) >= 1) {
  GympieNP_September2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_September2015$score[a[length(a)]] <- "cluster0"
}

if(length(a) < 1) {
  min <- min(GympieNP_September2015$value)
  a <- which(GympieNP_September2015$value==min)
  GympieNP_September2015$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_September2015$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="d  Sept 15")
b <- c(min(a), max(a))
WoondumNP_September2015 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_September2015)/48
# normalise the number of minutes
WoondumNP_September2015$value <- WoondumNP_September2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_September2015$item==times_new[i] & WoondumNP_September2015$value < 0.04)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_September2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_September2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_September2015$value)
  a <- which(WoondumNP_September2015$value==min)
  WoondumNP_September2015$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_September2015$score[a[1]] <- "cluster0"
}

# October 2015
a <- which(gym_df$family=="e  Oct 15")
b <- c(min(a), max(a))
GympieNP_October2015 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_October2015)/48
# normalise the number of minutes
GympieNP_October2015$value <- GympieNP_October2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_October2015$item==times_new[i] & GympieNP_October2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_October2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_October2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_October2015$value)
  a <- which(GympieNP_October2015$value==min)
  GympieNP_October2015$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_October2015$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="e  Oct 15")
b <- c(min(a), max(a))
WoondumNP_October2015 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_October2015)/48
# normalise the number of minutes
WoondumNP_October2015$value <- WoondumNP_October2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_October2015$item==times_new[i] & WoondumNP_October2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_October2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_October2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_October2015$value)
  a <- which(WoondumNP_October2015$value==min)
  WoondumNP_October2015$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_October2015$score[a[1]] <- "cluster0"
}

# November 2015
a <- which(gym_df$family=="f  Nov 15")
b <- c(min(a), max(a))
GympieNP_November2015 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_November2015)/48
# normalise the number of minutes
GympieNP_November2015$value <- GympieNP_November2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_November2015$item==times_new[i] & GympieNP_November2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_November2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_November2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_November2015$value)
  a <- which(GympieNP_November2015$value==min)
  GympieNP_November2015$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_November2015$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="f  Nov 15")
b <- c(min(a), max(a))
WoondumNP_November2015 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_November2015)/48
# normalise the number of minutes
WoondumNP_November2015$value <- WoondumNP_November2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_November2015$item==times_new[i] & WoondumNP_November2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_November2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_November2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_November2015$value)
  a <- which(WoondumNP_November2015$value==min)
  WoondumNP_November2015$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_November2015$score[a[1]] <- "cluster0"
}

# December 2015
a <- which(gym_df$family=="g  Dec 15")
b <- c(min(a), max(a))
GympieNP_December2015 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_December2015)/48
# normalise the number of minutes
GympieNP_December2015$value <- GympieNP_December2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_December2015$item==times_new[i] & GympieNP_December2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_December2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_December2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_December2015$value)
  a <- which(GympieNP_December2015$value==min)
  GympieNP_December2015$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_December2015$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="g  Dec 15")
b <- c(min(a), max(a))
WoondumNP_December2015 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_December2015)/48
# normalise the number of minutes
WoondumNP_December2015$value <- WoondumNP_December2015$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_December2015$item==times_new[i] & WoondumNP_December2015$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_December2015$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_December2015$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_December2015$value)
  a <- which(WoondumNP_December2015$value==min)
  WoondumNP_December2015$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_December2015$score[a[1]] <- "cluster0"
}

# January 2016
a <- which(gym_df$family=="h  Jan 16")
b <- c(min(a), max(a))
GympieNP_January2016 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_January2016)/48
# normalise the number of minutes
GympieNP_January2016$value <- GympieNP_January2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_January2016$item==times_new[i] & GympieNP_January2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_January2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_January2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_January2016$value)
  a <- which(GympieNP_January2016$value==min)
  GympieNP_January2016$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_January2016$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="h  Jan 16")
b <- c(min(a), max(a))
WoondumNP_January2016 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_January2016)/48
# normalise the number of minutes
WoondumNP_January2016$value <- WoondumNP_January2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_January2016$item==times_new[i] & WoondumNP_January2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_January2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_January2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_January2016$value)
  a <- which(WoondumNP_January2016$value==min)
  WoondumNP_January2016$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_January2016$score[a[1]] <- "cluster0"
}

# February 2016
a <- which(gym_df$family=="i  Feb 16")
b <- c(min(a), max(a))
GympieNP_February2016 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_February2016)/48
# normalise the number of minutes
GympieNP_February2016$value <- GympieNP_February2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_February2016$item==times_new[i] & GympieNP_February2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_February2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_February2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  a <- which(GympieNP_February2016$value < 0.1)
  min <- min(GympieNP_February2016$value)
  a <- which(GympieNP_February2016$value==min)
  GympieNP_February2016$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_February2016$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="i  Feb 16")
b <- c(min(a), max(a))
WoondumNP_February2016 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_February2016)/48
# normalise the number of minutes
WoondumNP_February2016$value <- WoondumNP_February2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_February2016$item==times_new[i] & WoondumNP_February2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_February2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_February2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_February2016$value)
  a <- which(WoondumNP_February2016$value==min)
  WoondumNP_February2016$value[a[1]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_February2016$score[a[1]] <- "cluster0"
}

# March 2016
a <- which(gym_df$family=="j  Mar 16")
b <- c(min(a), max(a))
GympieNP_March2016 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_March2016)/48
# normalise the number of minutes
GympieNP_March2016$value <- GympieNP_March2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_March2016$item==times_new[i] & GympieNP_March2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_March2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_March2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_March2016$value)
  a <- which(GympieNP_March2016$value==min)
  GympieNP_March2016$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_March2016$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="j  Mar 16")
b <- c(min(a), max(a))
WoondumNP_March2016 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_March2016)/48
# normalise the number of minutes
WoondumNP_March2016$value <- WoondumNP_March2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_March2016$item==times_new[i] & WoondumNP_March2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_March2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_March2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_March2016$value)
  a <- which(WoondumNP_March2016$value==min)
  WoondumNP_March2016$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_March2016$score[a[1]] <- "cluster0"
}

# April 2016
a <- which(gym_df$family=="k  Apr 16")
b <- c(min(a), max(a))
GympieNP_April2016 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_April2016)/48
# normalise the number of minutes
GympieNP_April2016$value <- GympieNP_April2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_April2016$item==times_new[i] & GympieNP_April2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_April2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_April2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_April2016$value)
  a <- which(GympieNP_April2016$value==min)
  GympieNP_April2016$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_April2016$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="k  Apr 16")
b <- c(min(a), max(a))
WoondumNP_April2016 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_April2016)/48
# normalise the number of minutes
WoondumNP_April2016$value <- WoondumNP_April2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_April2016$item==times_new[i] & WoondumNP_April2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_April2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_April2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_April2016$value)
  a <- which(WoondumNP_April2016$value==min)
  WoondumNP_April2016$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_April2016$score[a[1]] <- "cluster0"
}

# May 2016
a <- which(gym_df$family=="l  May 16")
b <- c(min(a), max(a))
GympieNP_May2016 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_May2016)/48
# normalise the number of minutes
GympieNP_May2016$value <- GympieNP_May2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_May2016$item==times_new[i] & GympieNP_May2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_May2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_May2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_May2016$value)
  a <- which(GympieNP_May2016$value==min)
  GympieNP_May2016$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_May2016$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="l  May 16")
b <- c(min(a), max(a))
WoondumNP_May2016 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_May2016)/48
# normalise the number of minutes
WoondumNP_May2016$value <- WoondumNP_May2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_May2016$item==times_new[i] & WoondumNP_May2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_May2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_May2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_May2016$value)
  a <- which(WoondumNP_May2016$value==min)
  WoondumNP_May2016$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_May2016$score[a[1]] <- "cluster0"
}

# June 2016
a <- which(gym_df$family=="m  Jun 16")
b <- c(min(a), max(a))
GympieNP_June2016 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_June2016)/48
# normalise the number of minutes
GympieNP_June2016$value <- GympieNP_June2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_June2016$item==times_new[i] & GympieNP_June2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_June2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_June2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_June2016$value)
  a <- which(GympieNP_June2016$value==min)
  GympieNP_June2016$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_June2016$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="m  Jun 16")
b <- c(min(a), max(a))
WoondumNP_June2016 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_June2016)/48
# normalise the number of minutes
WoondumNP_June2016$value <- WoondumNP_June2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_June2016$item==times_new[i] & WoondumNP_June2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_June2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_June2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(WoondumNP_June2016$value)
  a <- which(WoondumNP_June2016$value==min)
  WoondumNP_June2016$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_June2016$score[a[1]] <- "cluster0"
}

# July 2016
a <- which(gym_df$family=="n  Jul 16")
b <- c(min(a), max(a))
GympieNP_July2016 <- gym_df[b[1]:b[2],]
# determine the number of days
n <- nrow(GympieNP_July2016)/48
# normalise the number of minutes
GympieNP_July2016$value <- GympieNP_July2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(GympieNP_July2016$item==times_new[i] & GympieNP_July2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  GympieNP_July2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  GympieNP_July2016$score[a[length(a)]] <- "cluster0"
}
if(length(a) < 1) {
  min <- min(GympieNP_July2016$value)
  a <- which(GympieNP_July2016$value==min)
  GympieNP_July2016$value[a[1]] <- (scale[1] - 0.5)
  GympieNP_July2016$score[a[1]] <- "cluster0"
}

a <- which(won_df$family=="n  Jul 16")
b <- c(min(a), max(a))
WoondumNP_July2016 <- won_df[b[1]:b[2],]
# determine the number of days
n <- nrow(WoondumNP_July2016)/48
# normalise the number of minutes
WoondumNP_July2016$value <- WoondumNP_July2016$value/n
# set the scale of the polar plot
a <- NULL
for(i in 1:length(times_new)) {
  ref <- which(WoondumNP_July2016$item==times_new[i] & WoondumNP_July2016$value< 0.4)
  if(length(ref)==n) {
    a <- c(a,ref[n])  
  }
}
if(length(a) >= 1) {
  WoondumNP_July2016$value[a[length(a)]] <- (scale[1] - 0.5)
  # cluster0 will be white
  WoondumNP_July2016$score[a[length(a)]] <- "cluster0"
}

if(length(a) < 1) {
  min <- min(WoondumNP_July2016$value)
  a <- which(WoondumNP_July2016$value==min)
  WoondumNP_July2016$value[a[1]] <- (scale[1] - 0.5)
  WoondumNP_July2016$score[a[1]] <- "cluster0"
}
a <- c("GympieNP_June2015", "GympieNP_July2015",
       "GympieNP_August2015", "GympieNP_September2015",
       "GympieNP_October2015", "GympieNP_November2015",
       "GympieNP_December2015", "GympieNP_January2016",
       "GympieNP_February2016", "GympieNP_March2016",
       "GympieNP_April2016", "GympieNP_May2016",
       "GympieNP_June2016", "GympieNP_July2016",
       "WoondumNP_June2015", "WoondumNP_July2015",
       "WoondumNP_August2015", "WoondumNP_September2015",
       "WoondumNP_October2015", "WoondumNP_November2015",
       "WoondumNP_December2015", "WoondumNP_January2016",
       "WoondumNP_February2016", "WoondumNP_March2016",
       "WoondumNP_April2016", "WoondumNP_May2016",
       "WoondumNP_June2016", "WoondumNP_July2016")

#a <- "GympieNP_September2015"
#b <- a[c(7:10,21:24)]
#label <- as.character(intToUtf8(0x2600L))
#labels="\u2600";labels
sunrise <- read.csv("data/Sunrise_Sunset_protected.csv", header=T)
sunrise_min <- NULL
sunset_min <- NULL
for(i in 1:nrow(sunrise)) {
  sunrise_min[i:nrow(sunrise)] <- as.numeric(substr(sunrise$Sunrise[i],1,1))*60 +
    as.numeric(substr(sunrise$Sunrise[i],3,4))
  sunset_min[i:nrow(sunrise)] <- (((as.numeric(substr(sunrise$Sunset[i],1,1))*60) + 12*60) +
                                    (as.numeric(substr(sunrise$Sunset[i],3,4))))
}
sunrise_min <- rep(sunrise_min,2)
sunset_min <- rep(sunset_min,2)

a37 <- a[selection]
a37 <- data.frame(a37)
a37[,2] <- NULL
a37[,2] <- selection

n <- 1:nrow(a37)
j <- 0
for(i in a37[n,1]) {
  j <- j + 1
  r <- a37[j,2]
  title <- paste(i)
  subtitle <- paste("Cluster", substr(clust,8,10))
  file_title <- paste("polarHistograms/article/rose_plot_", i,"_",clust, ".tiff",sep = "")
  data <- get(i)
  data <- data.frame(data)
  if(clust=="cluster37") {
    list <- c("00:15","00:45","01:15","01:45","02:15","02:45","03:15","03:45","04:15",
              "04:45","05:15","05:45","06:15","06:45","07:15","07:45","08:15","08:45",
              "09:15","09:45","10:15","10:45","11:15","11:45","12:15","12:30")
    ac <- which(data$item=="00:15")
    data$value[ac[1]] <- (as.numeric(scale[1]) - 0.5)
    data$score[ac[1]] <- "cluster0"
    ab <- NULL
    for(k in 1:length(list)) {
      aa <- which(data$item==list[k])
      ab <- c(ab, aa)
    }
    ab <- sort(ab)
  }
  if(clust=="cluster44"|clust=="cluster48") {
    ab <- 1:nrow(data)
  }
  if(clust=="cluster37") {
    z <- polarHistogram(data[ab,], familyLabels = F, normalised = F, 
                        colour = col, circleProportion = 0.5,
                        innerRadius = 0, outerRadius = 1,
                        guides = seq(2,(scale[1]-1),2), 
                        labels = TRUE)
  }
  if(clust=="cluster44"|clust=="cluster48") {
    z <- polarHistogram(data[ab,], familyLabels = F, normalised = F, 
                        colour = col, circleProportion = 1,
                        innerRadius = 0, outerRadius = 1,
                        guides = seq(2,(scale[1]-1),2), 
                        labels = TRUE)
  }
  #z <- z + ggtitle(bquote(atop(.(title), atop(italic(.(subtitle)), ""))))
  #z <- z + theme(plot.title = element_text(size=22))
  #z <- z + theme(plot.title = element_text(margin=margin(b = -50, unit = "pt")))
  # add the sun symbol x is angle and y the fraction of the radius
  z <- z + geom_point(data=data.frame(x=c(1)), 
                      aes(x = (sunrise_min[r]*60/1440-0.5), y = 0.94), 
                      shape="\u2600", size=10)
  # add the moon symbol 
  z <- z + geom_point(data=data.frame(x=c(1)), 
                      aes(x = (sunset_min[r]*60/1440-2), y = 0.94), 
                      shape="\u263D", size=10)
  #z <- z + geom_point(x=150,y=0,shape="\u2600", size=20)
  z <- z + theme(plot.background = element_rect(fill = "transparent", 
                                                colour = NA))
  z <- z + theme(plot.margin=unit(c(0,-10,0,0),"mm"))
  # invisible is used to stop print opening a print window
  invisible(print(z))
  ggsave(file_title, width = 19.05, height = 19.05, units = "cm", 
         dpi = 300)
  dev.off()
}

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Figure10 ---------------------------------
# remove all objects in the global environment
rm(list = ls())
source("scripts/radarPlot.R")
# load normalised summary indices this has had the missing minutes
# and microphone problem minutes removed 
# the dataframe is called "indices_norm_summary"
load(file="data/datasets/normalised_summary_indices.RData")

k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list 
cluster.list <- get(file_name_short, envir=globalenv())[,column]

data <- cbind(cluster.list, indices_norm_summary)

num_clus <- unique(data$cluster.list)
num_clus <- 1:length(num_clus)

# the clara function is used for large datasets
library(cluster) # needed for pam/clara functions
medoids <- NULL
for(i in 1:length(num_clus)) {
  a <- which(data$cluster.list==i)
  clust <- data[a,2:ncol(data)]
  #plot(clust$BackgroundNoise, clust$Snr)
  #points(pam(clust, 1)$medoids, pch = 16, col = "red")
  medo <- clara(clust,1)$medoids
  medoids <- rbind(medoids, medo)
}
rownames(medoids) <- as.character(as.numeric(num_clus))

dd <- medoids
dd <- data.frame(dd)
dd <- cbind(c(1:60), medoids)
colnames(dd)[] <- c("clust", "BGN","SNR","ACT",
                    "EVN", "HFC", "MFC", "LFC",
                    "ACI", "EAS", "EPS", "ECV",
                    "CLC")

library(fmsb) #Functions for Medical Statistics Book with some Demographic Data

quiet5 <- 5
birds11 <- 11
rain59 <- 59
wind42  <- 42
cicadas48 <- 48
planes49 <- 49

insects_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
birds_col <- "#009E73"
cicadas_col <- "#E69F00"
quiet_col <- "#999999"
planes_col <- "#CC79A7"

colours <- c(quiet_col, birds_col, rain_col,
             wind_col, cicadas_col, planes_col) 
dd <- data.frame(dd)

all <- c("quiet5", "birds11", "rain59", 
         "wind42", "cicadas48", "planes49")

tiff("Figures for plos article/Fig10.tiff", width = 2250, height = 1500, units = 'px', res = 300)
ref <- 1
par(mfrow=c(2,3), xpd=NA, #decrease default margin
    mgp = c(0, 0.2, 0), cex = 0.6, oma = c(0,0.5,1,0)) 
radarPlot(rbind(rep(1,60), rep(0,60), dd[5,-1]), 
           pfcol=colours[ref], seg = 5, vlcex = 1.6, axistype=2,
           centerzero = TRUE, plwd = 1.5, 
           pdensity = 60, x1 = 0.5, y1 = 0.5, x2 = 2, y2 = 0.5)
mtext("a. QUIET", side = 3, cex = 1.1, line = -0.1)
text(x = -0.9, y = 1.26, paste("Cluster 5"), cex = 1.6)
ref <- ref + 1
radarPlot(rbind(rep(1,60), rep(0,60), dd[11,-1]), 
           pfcol=colours[ref], seg = 5, vlcex = 1.6, axistype=2,
           centerzero = TRUE, plwd = 1.5, 
           pdensity = 60, x1 = 0.5, y1 = 0.5, x2 = 2, y2 = 0.5)
text(x = -0.9, y = 1.26, paste("Cluster 11"), cex = 1.6)
mtext("b. BIRDS", side = 3, cex = 1.1, line = -0.1)
ref <- ref + 1
radarPlot(rbind(rep(1,60), rep(0,60), dd[59,-1]), 
           pfcol=colours[ref], seg = 5, vlcex = 1.6, axistype=2,
           centerzero = TRUE, plwd = 1.5, 
           pdensity = 60, x1 = 0.5, y1 = 0.5, x2 = 1, y2 = 0.5)
text(x = -0.9, y = 1.26, paste("Cluster 59"), cex = 1.6)
mtext("c. RAIN", side = 3, cex = 1.1, line = -0.1)
ref <- ref + 1
radarPlot(rbind(rep(1,60), rep(0,60), dd[42,-1]), 
           pfcol=colours[ref], seg = 5, vlcex = 1.6, axistype=2,
           centerzero = TRUE, plwd = 1.5, 
           pdensity = 60, x1 = 0.5, y1 = 0.5, x2 = 2, y2 = 0.5)
text(x = -0.9, y = 1.26, paste("Cluster 42"), cex = 1.6)
mtext("d. WIND", side = 3, cex = 1.1, line = -0.1)
ref <- ref + 1
radarPlot(rbind(rep(1,60), rep(0,60), dd[48,-1]), 
           pfcol=colours[ref], seg = 5, vlcex = 1.6, axistype=2,
           centerzero = TRUE, plwd = 1.5, 
           pdensity = 60, x1 = 0.5, y1 = 0.5, x2 = 2, y2 = 0.5)
text(x = -0.9, y = 1.26, paste("Cluster 48"), cex = 1.6)
mtext("e. CICADAS", side = 3, cex = 1.1, line = -0.1)
ref <- ref + 1
radarPlot(rbind(rep(1,60), rep(0,60), dd[49,-1]), 
           pfcol=colours[ref], seg = 5, vlcex = 1.6, axistype=2,
           centerzero = TRUE, plwd = 1.5, 
           pdensity = 60, x1 = 0.5, y1 = 0.5, x2 = 1, y2 = 0.5)
text(x = -0.9, y = 1.26, paste("Cluster 49"), cex = 1.6)
mtext("f. PLANES", side = 3, cex = 1.1, line = -0.1)
ref <- ref + 1
dev.off()

# Figure11 -----------------------------------------
# Set the layout matrix to divide page into two frames one
# for the plot and one for the table
library(grid)
m <- rbind(c(1,1,1,2,2,2,3,3,3,4,4,4,5,5,5),
           c(6,6,6,7,7,7,8,8,8,9,9,9,10,10,10),
           c(6,6,6,7,7,7,8,8,8,9,9,9,10,10,10),
           c(6,6,6,7,7,7,8,8,8,9,9,9,10,10,10),
           c(6,6,6,7,7,7,8,8,8,9,9,9,10,10,10),
           c(6,6,6,7,7,7,8,8,8,9,9,9,10,10,10),
           c(11,11,11,11,11,12,12,12,12,12,13,13,13,13,13),
           c(14,14,14,14,14,15,15,15,15,15,16,16,16,16,16),
           c(14,14,14,14,14,15,15,15,15,15,16,16,16,16,16),
           c(14,14,14,14,14,15,15,15,15,15,16,16,16,16,16),
           c(14,14,14,14,14,15,15,15,15,15,16,16,16,16,16),
           c(14,14,14,14,14,15,15,15,15,15,16,16,16,16,16),
           c(17,17,17,17,17,18,18,18,18,18,19,19,19,19,19),
           c(20,20,20,20,20,21,21,21,21,21,22,22,22,22,22),
           c(20,20,20,20,20,21,21,21,21,21,22,22,22,22,22),
           c(20,20,20,20,20,21,21,21,21,21,22,22,22,22,22),
           c(20,20,20,20,20,21,21,21,21,21,22,22,22,22,22),
           c(20,20,20,20,20,21,21,21,21,21,22,22,22,22,22))
lay <- layout(m, widths = 1.25, heights = 1.25)
layout.show(22)

vp_top <- viewport(x = 0, y = 0, width = 7.5, height = 8.7, 
                   name = "vp_top", default.units = "in")
vp9 <- viewport(x=0, y=0, width=2.5, height=2.5, just = c("right","top"), 
         name = "vp9", default.units = "in")
vp10 <- viewport(x=2.5, y=0, width=2.5, height=2.5, just = c("right","top"), 
         name = "vp10", default.units = "in")
vp11 <- viewport(x=5, y=0, width=2.5, height=2.5, just = c("right","top"), 
         name = "vp11", default.units = "in")
margin1 <- viewport(x=0, y=2.5, width=7.5, height=0.4, just = c("right","top"), 
         name = "margin1", default.units = "in")
vp6 <- viewport(x=0, y=2.9, width=2.5, height=2.5, just = c("right","top"), 
         name = "vp6", default.units = "in")
vp7 <- viewport(x=2.5, y=2.9, width=2.5, height=2.5, just = c("right","top"), 
         name = "vp7", default.units = "in")
vp8 <- viewport(x=5, y=2.9, width=2.5, height=2.5, just = c("right","top"), 
         name = "vp8", default.units = "in")
margin2 <- viewport(x=0, y=5.4, width=7.5, height=0.4, just = c("right","top"), 
         name = "margin2", default.units = "in")
vp1 <- viewport(x=0, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
         name = "vp1", default.units = "in")
vp2 <- viewport(x=1.5, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
         name = "vp2", default.units = "in")
vp3 <- viewport(x=3, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
         name = "vp3", default.units = "in")
vp4 <- viewport(x=4.5, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
         name = "vp4", default.units = "in")
vp5 <- viewport(x=6, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
         name = "vp5", default.units = "in")
margin3 <- viewport(x=0, y=8.3, width=7.5, height=0.4, just = c("right","top"), 
         name = "margin3", default.units = "in")
splot <- vpTree(vp_top, vpList(vp1, vp2, vp3, vp4, vp5, 
                       vp6, vp7, vp8, vp9, vp10, 
                       vp11, margin1, margin2, margin3))
pushViewport(splot)
seekViewport("vp10")
par(mar=c(0,0,0,0))
plot(1:11, 3:13)
seekViewport("vp5")
par(mar=c(0,0,0,0))
plot(1:11, 3:13)

#########################################################
# Figure 9 Composite images -----------------------------
#########################################################
# remove all objects in the global environment
rm(list = ls())

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list 
cluster_list <- get(file_name_short, envir=globalenv())[,column]

# remove unneeded objects from global environment
rm(hclust_clusters_25000, file_name, file_name_short, column)

# load missing minute reference list 
load(file="data/datasets/missing_minutes_summary_indices.RData")
# load minutes where there was problems with both microphones
microphone_minutes <- c(184321:188640)
# list of all minutes that have been removed previously
removed_minutes <- c(missing_minutes_summary, microphone_minutes)
rm(microphone_minutes, missing_minutes_summary) 

full_length <- length(cluster_list) + length(removed_minutes)
list <- 1:full_length
list1 <- list[-c(removed_minutes)]
reconstituted_cluster_list <- rep(0, full_length)

reconstituted_cluster_list[removed_minutes] <- NA
reconstituted_cluster_list[list1] <- cluster_list
cluster_list <- reconstituted_cluster_list
rm(reconstituted_cluster_list)

days <- length(cluster_list)/(1440)
minute_reference <- rep(0:1439, days)

cluster_list <- cbind(cluster_list, minute_reference)
rm(days, full_length, list, list1, minute_reference, removed_minutes)

write.csv(cluster_list, paste("data/datasets/chosen_cluster_list_",k1_value,
                              "_",k2_value, ".csv", sep = ""), row.names = F)

# remove all objects in the global environment
rm(list = ls())

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",k1_value,
                               "_",k2_value, ".csv", sep = ""))
list <- which(cluster_list$minute_reference=="0")
last1 <- NULL
for (i in 1:(length(list)-1)) {
  last <- list[i+1]-1
  last1 <- c(last1, last)
}
last1 <- c(last1, nrow(cluster_list))
list <- cbind(list, last1)

colnames(list) <- c("start","end")
list <- as.data.frame(list)
list[length(list$start),2] <- length(cluster_list$minute_reference)

library(raster)

# How do you create a new folder and move the raster image 
# into this folder?

image <- paste("data/clusterImages_", k1_value, 
               "_", k2_value, "/Rasterimage1.png", sep = "")
s <- brick(image, package="raster", ncol=615, nrows=668)
#s <- brick(image, package="raster", ncol=1280, nrows=668)
s[[1]] <- 255 
s[[2]] <- 255
s[[3]] <- 255
s <- subset(s,1:3)

# Get list of names (full path) of all spectrogram files
path <- "data/concatOutput" # path to spectrogram files
spect_file_Gympie <- list.files(full.names=TRUE, recursive = T,
                                path = paste(path,"\\GympieNP",sep=""),
                                pattern = "*2Maps.png")
spect_file_Woondum <- list.files(full.names=TRUE, recursive = T,
                                 path = paste(path,"\\Woondum3",sep=""),
                                 pattern = "*2Maps.png") 
spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)
length(spect_file_list)

cluster_image <- function(clust_num) {
  sample_size <- 600
  # Get list of positions of cluster
  which1 <- which(cluster_list$cluster_list==clust_num)
  # Select a random sample from a cluster
  whichV1 <- sample(which1, sample_size)
  # Create a blank raster image
  # Use ncol =615 for 600 minutes with RasterImage1.png
  s <- brick(image, package="raster", ncol=615, nrows=668)
  # Use ncol = 1280 for 1200 minutes with RasterImage.png
  #s <- brick(image, package="raster", ncol=1280, nrows=668)
  s[[1]] <- 255 
  s[[2]] <- 255
  s[[3]] <- 255
  s <- subset(s,1:3)
  tiff(paste("data/clusterImages_", k1_value,"_",
             k2_value, "/ClusterImage_Cluster",
             clust_num,"_", k1_value, "_", k2_value, 
             ".tiff", sep = ""), width = 2250, 
       height = 2350, units = 'px', res = 300)
  # Set the start column from the edge of the image
  length2 <- 10
  min.ref.check <- NULL
  which.check <- NULL
  
  for(i in 1:sample_size) {
    if(whichV1[i] %in% c(seq(1440,length(spect_file_list)*1440, 1440))){
      day.ref <- floor((whichV1[i])/1440)
    }
    else {
      day.ref <- floor((whichV1[i])/1440)+1
    }
    min.ref <- ((whichV1[i]/1440) - (day.ref-1))*1440
    # select the twenty-four hour spectrogram image
    b1 <- spect_file_list[day.ref]
    # read spectrogram image as a raster image (sourceImage)
    b <- brick(b1, package="raster")
    sourceImage <- brick(b1, package="raster")
    current.minute.list <- min.ref
    replacementBlock <- getValuesBlock(sourceImage, 
                                       row=1, 
                                       nrows=668, 
                                       col=current.minute.list, 
                                       ncols=1)
    s[1:668, length2] <- replacementBlock
    length2 <- length2 + 1
    which.check <- c(which.check, whichV1[i])
    min.ref.check <- c(min.ref.check, min.ref)
  }
  plotRGB(s)
  dev.off()
}

# Call the cluster_image function for each of the clusters
dev.off()
clusters <- c(59,42,29,37)
for(j in clusters) {
  print(paste("starting", j, Sys.time(), sep = " "))
  cluster_image(j) # call function k2 times
}
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 11 and 12 Cluster diel plot for Gympie and Woondum  -------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# set the start date in "YYYY-MM-DD" format
start_date <- "2015-06-22"

k1_value <- 25000
k2_value <- 60

# load cluster list
cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",k1_value,
                               "_",k2_value, ".csv", sep = ""))

# Generate a date sequence & locate the first of each month
days <- floor(nrow(cluster_list)/(2*1440))
start <- as.POSIXct(start_date)
interval <- 1440
end <- start + as.difftime(days, units="days")
dates <- seq(from=start, by=interval*60, to=end)
first_of_month <- which(substr(dates, 9, 10)=="01")

# Prepare civil dawn, civil dusk and sunrise and sunset times
#civil_dawn <- read.csv("data/Sunrise_Sunset_Solar Noon_protected.csv", header=T)
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2016 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2016.csv")
civil_dawn <- rbind(civil_dawn_2015, civil_dawn_2016)
# set the start date in "YYYY-MM-DD" format
start_date <- "2015-06-22"
start <- as.POSIXct(start_date)

a <- which(civil_dawn$dates==paste(substr(start, 1,4), substr(start, 6,7),
                                   substr(start, 9,20),sep = "-"))
reference <- a:(a+days-1)
civil_dawn_times <- civil_dawn$CivSunrise[reference]
civil_dusk_times <- civil_dawn$CivSunset[reference]
sunrise_times <- civil_dawn$Sunrise[reference]
sunset_times <- civil_dawn$Sunset[reference]
start <- as.POSIXct(start_date)
# find the minute of civil dawn for each day
civ_dawn <- NULL
for(i in 1:length(civil_dawn_times)) {
  hour <- as.numeric(substr(civil_dawn_times[i], 1,1))
  min <- as.numeric(substr(civil_dawn_times[i], 2,3))
  minute <- hour*60 + min
  civ_dawn <- c(civ_dawn, minute)
}

civ_dusk <- NULL
for(i in 1:length(civil_dusk_times)) {
  hour <- as.numeric(substr(civil_dusk_times[i], 1,2)) 
  min <- as.numeric(substr(civil_dusk_times[i], 3,4))
  minute <- hour*60 + min
  civ_dusk <- c(civ_dusk, minute)
}

sunrise <- NULL
for(i in 1:length(sunrise_times)) {
  hour <- as.numeric(substr(sunrise_times[i], 1,1))
  min <- as.numeric(substr(sunrise_times[i], 2,3))
  minute <- hour*60 + min
  sunrise <- c(sunrise, minute)
}

sunset <- NULL
for(i in 1:length(sunset_times)) {
  hour <- as.numeric(substr(sunset_times[i], 1,2)) 
  min <- as.numeric(substr(sunset_times[i], 3,4))
  minute <- hour*60 + min
  sunset <- c(sunset, minute)
}


# Produce clustering diel plots for both sites 
dev.off()
# load all of the summary indices as "indices_all"
load(file="data/datasets/summary_indices.RData")
# remove redundant indices
remove <- c(1,4,11,13,17:19)
indices_all <- indices_all[,-remove]
indices_all <- indices_all[1,]
rm(remove)

# IMPORTANT:  These are used to name the plots
site <- c("Gympie NP", "Woondum NP")
index <- "Final"  #"SELECTED_Practice" # or "ALL"
type <- "Summary"
indices_names <-colnames(indices_all)
paste("The dataset contains the following indices:"); colnames(indices_all)

indices_names_abb <- NULL
for(i in 1:length(indices_names)) {
  if(indices_names[i] =="AvgSignalAmplitude") {
    indices_names_abb[i] <- "ASA"
  }
  if(indices_names[i] =="BackgroundNoise") {
    indices_names_abb[i] <- "BGN"
  }
  if(indices_names[i] =="Snr") {
    indices_names_abb[i] <- "SNR"
  }
  if(indices_names[i] =="AvgSnrOfActiveFrames" ) {
    indices_names_abb[i] <- "ASF"
  }
  if(indices_names[i] =="Activity") {
    indices_names_abb[i] <- "ACT"
  }
  if(indices_names[i] =="EventsPerSecond") {
    indices_names_abb[i] <- "EVN"
  }
  if(indices_names[i] =="HighFreqCover") {
    indices_names_abb[i] <- "HFC"
  }
  if(indices_names[i] =="MidFreqCover") {
    indices_names_abb[i] <- "MFC"
  }
  if(indices_names[i] =="LowFreqCover") {
    indices_names_abb[i] <- "LFC"
  }
  if(indices_names[i] =="AcousticComplexity") {
    indices_names_abb[i] <- "ACI"
  }
  if(indices_names[i] =="TemporalEntropy") {
    indices_names_abb[i] <- "ENT"
  }
  if(indices_names[i] =="EntropyOfAverageSpectrum") {
    indices_names_abb[i] <- "EAS"
  }
  if(indices_names[i] =="EntropyOfVarianceSpectrum" ) {
    indices_names_abb[i] <- "EVS"
  }
  if(indices_names[i] =="EntropyOfPeaksSpectrum") {
    indices_names_abb[i] <- "EPS"
  }
  if(indices_names[i] =="EntropyOfCoVSpectrum") {
    indices_names_abb[i] <- "ECS"
  }
  if(indices_names[i] =="ClusterCount") {
    indices_names_abb[i] <- "CLC"
  }
  if(indices_names[i] =="ThreeGramCount") {
    indices_names_abb[i] <- "TGC"
  }
  if(indices_names[i] =="NSDI" ) {
    indices_names_abb[i] <- "NSD"
  }
  if(indices_names[i] =="SptDensity" ) {
    indices_names_abb[i] <- "SPD"
  }
}

# Check for col_func in globalEnv otherwise source function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# Generate colour list using col_func
# Note col_func requires a csv file containing customed
# colour information for each cluster 
# version is either 'ordinary' or 'colourblind'
col_func(cluster_colours, version = "ordinary")

# Generate and save the cluster diel plots
for (k in 1:2) {
  ref <- c(0, days*1440)
  # generate a date sequence and locate the first of the month
  days <- nrow(cluster_list)/(2*1440)
  start <- as.POSIXct(paste(start_date))
  interval <- 1440
  end <- start + as.difftime(days, units="days")
  dates <- seq(from=start, by=interval*60, to=end)
  if(k==1) {
    tiff("Figures for plos article/Fig11.tiff", width = 2250, height = 1150, units = 'px', res = 300)
  }
  if(k==2) {
    tiff("Figures for plos article/Fig12.tiff", width = 2250, height = 1150, units = 'px', res = 300)
  }
  #par(mar=c(0.9, 3.9, 0.9, 3.9), mgp = c(3,0.8,0),
  #    cex = 0.6, cex.axis = 1.2, cex.main = 1)
  par(mar=c(0.9, 2.7, 0.9, 2.7), mgp = c(3,0.8,0),
      cex = 0.6, cex.axis = 1.2, cex.main = 1)
  
  # Plot an empty plot with no axes or frame
  plot(c(0,1440), c(398,1), type = "n", axes=FALSE, 
       frame.plot=FALSE,
       xlab="", ylab="") #, asp = 398/1440)
  # Create the heading
  mtext(side=3, line = -1,
        paste("Cluster diel plot - ", site[k]," ", format(dates[1], "%d %B %Y")," - ", 
              format(dates[length(dates)-1], "%d %B %Y"), 
              sep=""))
  # Create the sub-heading
  #mtext(side=3, line = -1.5, 
  #      paste(type, " Indices: ", 
  #            paste(indices_names, collapse = ", "), 
  #            sep = ""))
  
  # draw coloured polygons row by row
  ref <- ref[k]
  # set the rows starting at the top of the plot
  for(j in days:1) {
    # set the column starting on the left
    for(k in 1:1440) {
      ref <- ref + 1
      # draw a square for each minute in each day 
      # using the polygon function mapping the cluster
      # number to a colour
      cluster <- cluster_list$cluster_list[ref]
      if(!is.na(cluster)) {
        col_ref <- cols[cluster]
      }
      if(is.na(cluster)) {
        col_ref <- cols[nrow(cols)]
      }
      polygon(c(k,k,k+1,k+1), c(j,(j-1),(j-1),j),
              col=col_ref,
              border = NA)
    }
  }
  # draw horizontal lines
  first_of_month <- which(substr(dates, 9, 10)=="01")
  first_of_each_month <- days - first_of_month + 1
  for(i in 1:length(first_of_month)) {
    lines(c(1,1441), c(first_of_each_month[i], 
                       first_of_each_month[i]), 
          lwd=1, lty = 3)
  }
  # draw vertical lines
  at <- seq(0,1440, 240) + 1
  for(i in 1:length(at)) {
    lines(c(at[i], at[i]), c(1,days), lwd=1, lty=3)
  }
  # label the x axis
  axis(1, tick = FALSE, at = at, 
       labels = c("12 am","4 am",
                  "8 am","12","4 pm",
                  "8 pm","12 pm"), line = -1.4)
  # plot the left axes
  axis(side = 2, at = first_of_each_month, tick = FALSE, 
       labels=format(dates[first_of_month],"%b %Y"), 
       las=1, line = -2.5)
  #axis(side = 2, at = c(days), tick = FALSE, 
  #     labels=format(dates[1],"%d %b %Y"), 
  #     las=1, line = -2.5)
  # plot the left axes
  axis(side = 4, at = first_of_each_month, tick = FALSE, 
       labels=format(dates[first_of_month],"%b %Y"), 
       las=1, line = -2.5)
  #axis(side = 4, at = c(days), tick = FALSE, 
  #     labels=format(dates[1],"%d %b %Y"), 
  #     las=1, line = -2.5)
  
  at <- seq(0, 1440, 240)
  
  # draw dotted line to show civil-dawn
  for(i in length(civ_dawn):1) {
    lines(c(civ_dawn), c(length(civ_dawn):1),  
          lwd=1.2, lty=2, col="yellow")
  }
  # draw dotted line to show civil-dusk
  for(i in length(civ_dusk):1) {
    lines(c(civ_dusk), c(length(civ_dusk):1),  
          lwd=1.2, lty=2, col="yellow")
  }
  # draw dotted line to show sunrise
  for(i in length(sunrise):1) {
    lines(c(sunrise), c(length(sunrise):1),  
          lwd=1.2, lty=2, col="yellow")
  }
  # draw dotted line to show sunset
  for(i in length(sunset):1) {
    lines(c(sunset), c(length(sunset):1),  
          lwd=1.2, lty=2, col="yellow")
  }
  dev.off()
}
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 14 Polar Histogram 365 days-------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list 
cluster_list <- get(file_name_short, envir=globalenv())[,column]

# remove unneeded objects from global environment
rm(hclust_clusters_25000, file_name, file_name_short, column)

# load missing minute reference list 
load(file="data/datasets/missing_minutes_summary_indices.RData")
# load minutes where there was problems with both microphones
microphone_minutes <- c(184321:188640)
# list of all minutes that have been removed previously
removed_minutes <- c(missing_minutes_summary, microphone_minutes)
rm(microphone_minutes, missing_minutes_summary) 

full_length <- length(cluster_list) + length(removed_minutes)
list <- 1:full_length
list1 <- list[-c(removed_minutes)]
reconstituted_cluster_list <- rep(0, full_length)

reconstituted_cluster_list[removed_minutes] <- NA
reconstituted_cluster_list[list1] <- cluster_list
cluster_list <- reconstituted_cluster_list
rm(reconstituted_cluster_list)

days <- length(cluster_list)/(1440)
minute_reference <- rep(0:1439, days)

cluster_list <- cbind(cluster_list, minute_reference)
rm(days, full_length, list, list1, minute_reference, removed_minutes)

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20160723", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

dates <- rep(date.list, each = 1440)
dates <- rep(dates, 2)
# Add site and dates columns to dataframe
sites <- c("Gympie NP", "Woondum NP")
sites <- rep(sites, each=length(dates)/2)
cluster_list <- cbind(cluster_list, sites, dates)
cluster_list <- data.frame(cluster_list)
site <- unique(sites)

# generates a daily list of the number of each cluster
cluster <- NULL
cluster$clust <- 1:(398*2*60)
cluster$count <- 1:(398*2*60)
for(i in 1:length(site)) {
  for(j in 1:length(date.list)) {
    if(i==1) {
      ref <- j*60 - 59  
    }
    if(i==2) {
      ref <- (j*60 - 59) + 398*60
    }
    a <- which(cluster_list$dates==date.list[j] 
               & cluster_list$sites==site[i])
    b <- table(cluster_list[a,1])
    # reorder the rows to a numeric order
    b <- b[c(1, 12, 23, 34, 45, 56, 
             58:60, 2:11, 13:22, 
             24:33, 35:44, 46:55, 57)]
    cluster$site[ref:(ref+59)] <- site[i]
    cluster$date[ref:(ref+59)] <- date.list[j]
    cluster$clust[ref:(ref+59)] <- names(b)
    cluster$count[ref:(ref+59)] <- unname(b) 
  }  
}

#View(cluster)

cluster <- data.frame(cluster)
length <- length(cluster$clust)/2
gym_clust <- cluster[1:length,]
woon_clust <- cluster[(length+1):(length*2),]
#View(gym_clust)

# rename columns to align to polarHistogram function
gym_clust$score <- gym_clust$clust
gym_clust$value <- gym_clust$count
gym_clust$family <- gym_clust$month
gym_clust$item <- gym_clust$date

woon_clust$score <- woon_clust$clust
woon_clust$value <- woon_clust$count
woon_clust$family <- woon_clust$month
woon_clust$item <- woon_clust$date

# define cluster classes (the are the old list)
#rain <- c(59,18,10,54,2,21,38,60)
#wind <- c(42,47,51,56,52,45,8,40,24,19,46,28,9,25,30,20)
#birds <- c(58,43,57,37,11,3,33,15,14,39,4)
#insects <- c(17,1,27,22,26,29)
#cicada <- c(48,34,44,7,12,32,16)
#planes <- c(49,23)
#quiet <- c(6,53,36,31,50,35,55,41,13,5)

# define cluster classes 
rain <- c(2,10,17,18,21,54,59,60) 
wind <- c(9,19,20,24,25,30,40,42,45,46,47,51,52,56)
birds <- c(3,11,14,15,28,33,37,39,43,57,58)
insects <- c(1,4,22,26,27,29)
cicada <- c(7,8,12,16,32,34,44,48)
planes <- c(49,23)
quiet <- c(5,6,13,31,35,36,38,41,50,53,55)
na <- 61

gympie_clusters <- matrix(ncol = 4, nrow = 398*7, "NA") 
gympie_clusters <- data.frame(gympie_clusters)
colnames(gympie_clusters) <- c("score","value","family","item")

class <- c("rain","birds","insects","cicada","planes","quiet","wind")
class <- rep(class, 398)
gympie_clusters$score <- class
dates1 <- rep(date.list, each=7)
gympie_clusters$dates <- dates1

# fill 'item' column with a single number
gympie_clusters$item <- substr(gympie_clusters$dates,9,10)

# format columns
gympie_clusters$family <- as.character(gympie_clusters$family)
gympie_clusters$item <- as.character(gympie_clusters$item)
gympie_clusters$value <- as.numeric(gympie_clusters$value)

# fill 'family' column with month description
for(i in 1:length(gympie_clusters$item)) {
  if((substr(gympie_clusters$dates[i],6,7)=="06") &
     (substr(gympie_clusters$dates[i],1,4)=="2015")) {
    gympie_clusters$family[i] <- "a  Jun 2015"
  }  
  if((substr(gympie_clusters$dates[i],6,7)=="06") &
     (substr(gympie_clusters$dates[i],1,4)=="2016")) {
    gympie_clusters$family[i] <- "m  Jun 2016"
  } 
  if((substr(gympie_clusters$dates[i],6,7)=="07") &
     (substr(gympie_clusters$dates[i],1,4)=="2015")) {
    gympie_clusters$family[i] <- "b  Jul 2015"
  }  
  if((substr(gympie_clusters$dates[i],6,7)=="07") &
     (substr(gympie_clusters$dates[i],1,4)=="2016")) {
    gympie_clusters$family[i] <- "n  Jul 2016"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="08") {
    gympie_clusters$family[i] <- "c  Aug 2015"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="09") {
    gympie_clusters$family[i] <- "d  Sept 2015"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="10") {
    gympie_clusters$family[i] <- "e  Oct 2015"
  }  
  if(substr(gympie_clusters$dates[i],6,7)=="11") {
    gympie_clusters$family[i] <- "f  Nov 2015"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="12") {
    gympie_clusters$family[i] <- "g  Dec 2015"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="01") {
    gympie_clusters$family[i] <- "h  Jan 2016"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="02") {
    gympie_clusters$family[i] <- "i  Feb 2016"
  }  
  if(substr(gympie_clusters$dates[i],6,7)=="03") {
    gympie_clusters$family[i] <- "j  Mar 2016"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="04") {
    gympie_clusters$family[i] <- "k  Apr 2016"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="05") {
    gympie_clusters$family[i] <- "l  May 2016"
  } 
}

# find the sum of the classes (rain, insects...) per day
ref <- 1
for(i in 1:length(date.list)) {
  b <- (which(gym_clust$date==date.list[i]))
  num <- NULL
  for(k in 1:length(rain)) {
    a <- (which(gym_clust$clust==rain[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    rain_num <- unique(num)
  }
  rain_sum <- sum(gym_clust$count[rain_num])
  if(is.na(rain_sum)==TRUE) {
    rain_sum <- 0
  }
  
  num <- NULL
  for(k in 1:length(insects)) {
    a <- (which(gym_clust$clust==insects[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    insects_num <- unique(num)
  }
  insects_sum <- sum(gym_clust$count[insects_num])
  if(is.na(insects_sum)==TRUE) {
    insects_sum <- 0
  }
  
  num <- NULL
  for(k in 1:length(birds)) {
    a <- (which(gym_clust$clust==birds[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    birds_num <- unique(num)
  }
  birds_sum <- sum(gym_clust$count[birds_num])
  if(is.na(birds_sum)==TRUE) {
    birds_sum <- 0
  }
  
  num <- NULL
  for(k in 1:length(quiet)) {
    a <- (which(gym_clust$clust==quiet[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    quiet_num <- unique(num)
  }
  quiet_sum <- sum(gym_clust$count[quiet_num])
  if(is.na(quiet_sum)==TRUE) {
    quiet_sum <- 0
  }
  
  num <- NULL
  for(k in 1:length(cicada)) {
    a <- (which(gym_clust$clust==cicada[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    cicada_num <- unique(num)
  }
  cicada_sum <- sum(gym_clust$count[cicada_num])
  if(is.na(cicada_sum)==TRUE) {
    cicada_sum <- 0
  }
  
  num <- NULL
  for(k in 1:length(planes)) {
    a <- (which(gym_clust$clust==planes[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    planes_num <- unique(num)
  }
  planes_sum <- sum(gym_clust$count[planes_num])
  if(is.na(planes_sum)==TRUE) {
    planes_sum <- 0
  }
  num <- NULL
  for(k in 1:length(wind)) {
    a <- (which(gym_clust$clust==wind[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    wind_num <- unique(num)
  }
  wind_sum <- sum(gym_clust$count[wind_num])
  if(is.na(wind_sum)==TRUE) {
    wind_sum <- 0
  }
  
  gympie_clusters$value[ref:(ref+6)] <- c(rain_sum,
                                          birds_sum,
                                          insects_sum,
                                          cicada_sum,
                                          planes_sum,
                                          quiet_sum,
                                          wind_sum)
  ref <- ref + 7
}

#View(gympie_clusters)

woondum_clusters <- matrix(ncol = 4, nrow = 398*7, "NA") 
woondum_clusters <- data.frame(woondum_clusters)
colnames(woondum_clusters) <- c("score","value","family","item")

class <- c("rain","birds","insects","cicada","planes","quiet","wind")
class <- rep(class, 398)
woondum_clusters$score <- class
dates1 <- rep(date.list, each=7)
woondum_clusters$dates <- dates1
woondum_clusters$item <- substr(woondum_clusters$dates,9,10)
woondum_clusters$family <- as.character(woondum_clusters$family)
woondum_clusters$item <- as.character(woondum_clusters$item)
woondum_clusters$value <- as.numeric(woondum_clusters$value)
for(i in 1:length(woondum_clusters$item)) {
  if((substr(woondum_clusters$dates[i],6,7)=="06") &
     (substr(woondum_clusters$dates[i],1,4)=="2015")) {
    woondum_clusters$family[i] <- "a  Jun 2015"
  }  
  if((substr(woondum_clusters$dates[i],6,7)=="06") &
     (substr(woondum_clusters$dates[i],1,4)=="2016")) {
    woondum_clusters$family[i] <- "m  Jun 2016"
  } 
  if((substr(woondum_clusters$dates[i],6,7)=="07") &
     (substr(woondum_clusters$dates[i],1,4)=="2015")) {
    woondum_clusters$family[i] <- "b  Jul 2015"
  }  
  if((substr(woondum_clusters$dates[i],6,7)=="07") &
     (substr(woondum_clusters$dates[i],1,4)=="2016")) {
    woondum_clusters$family[i] <- "n  Jul 2016"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="08") {
    woondum_clusters$family[i] <- "c  Aug 2015"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="09") {
    woondum_clusters$family[i] <- "d  Sept 2015"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="10") {
    woondum_clusters$family[i] <- "e  Oct 2015"
  }  
  if(substr(woondum_clusters$dates[i],6,7)=="11") {
    woondum_clusters$family[i] <- "f  Nov 2015"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="12") {
    woondum_clusters$family[i] <- "g  Dec 2015"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="01") {
    woondum_clusters$family[i] <- "h  Jan 2016"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="02") {
    woondum_clusters$family[i] <- "i  Feb 2016"
  }  
  if(substr(woondum_clusters$dates[i],6,7)=="03") {
    woondum_clusters$family[i] <- "j  Mar 2016"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="04") {
    woondum_clusters$family[i] <- "k  Apr 2016"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="05") {
    woondum_clusters$family[i] <- "l  May 2016"
  } 
}

ref <- 1
for(i in 1:length(date.list)) {
  b <- (which(woon_clust$date==date.list[i]))
  num <- NULL
  for(k in 1:length(rain)) {
    a <- (which(woon_clust$clust==rain[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    rain_num <- unique(num)
  }
  rain_sum <- sum(woon_clust$count[rain_num])
  if(is.na(rain_sum)==TRUE) {
    rain_sum <- 0
  }
  
  num <- NULL
  for(k in 1:length(insects)) {
    a <- (which(woon_clust$clust==insects[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    insects_num <- unique(num)
  }
  insects_sum <- sum(woon_clust$count[insects_num])
  if(is.na(insects_sum)==TRUE) {
    insects_sum <- 0
  }
  num <- NULL
  for(k in 1:length(birds)) {
    a <- (which(woon_clust$clust==birds[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    birds_num <- unique(num)
  }
  birds_sum <- sum(woon_clust$count[birds_num])
  if(is.na(birds_sum)==TRUE) {
    birds_sum <- 0
  }
  num <- NULL
  for(k in 1:length(quiet)) {
    a <- (which(woon_clust$clust==quiet[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    quiet_num <- unique(num)
  }
  quiet_sum <- sum(woon_clust$count[quiet_num])
  if(is.na(quiet_sum)==TRUE) {
    quiet_sum <- 0
  }
  num <- NULL
  for(k in 1:length(cicada)) {
    a <- (which(woon_clust$clust==cicada[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    cicada_num <- unique(num)
  }
  cicada_sum <- sum(woon_clust$count[cicada_num])
  if(is.na(cicada_sum)==TRUE) {
    cicada_sum <- 0
  }
  num <- NULL
  for(k in 1:length(planes)) {
    a <- (which(woon_clust$clust==planes[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    planes_num <- unique(num)
  }
  planes_sum <- sum(woon_clust$count[planes_num])
  if(is.na(planes_sum)==TRUE) {
    planes_sum <- 0
  }
  num <- NULL
  for(k in 1:length(wind)) {
    a <- (which(woon_clust$clust==wind[k]))
    a <- intersect(a,b)
    num <- c(num, a)
    wind_num <- unique(num)
  }
  wind_sum <- sum(woon_clust$count[wind_num])
  if(is.na(wind_sum)==TRUE) {
    wind_sum <- 0
  }
  woondum_clusters$value[ref:(ref+6)] <- c(rain_sum,
                                           birds_sum,
                                           insects_sum,
                                           cicada_sum,
                                           planes_sum,
                                           quiet_sum,
                                           wind_sum)
  ref <- ref + 7
}


# Christophe Ladroue
library(plyr)
library(ggplot2)

polarHistogram365 <-function (df, family = NULL, 
                              columnNames = NULL, 
                              binSize = 1,
                              spaceItem = 0.2, 
                              spaceFamily = 0, 
                              innerRadius = 0.2, 
                              outerRadius = 1,
                              guides = c(20, 40, 60, 80), 
                              alphaStart = 0, #-0.3, 
                              circleProportion = 0.98,
                              direction = "outwards", 
                              familyLabels = TRUE, 
                              normalised = FALSE,
                              units = "cm")
{
  if (!is.null(columnNames)) {
    namesColumn <- names(columnNames)
    names(namesColumn) <- columnNames
    df <- rename(df, namesColumn)
  }
  
  applyLookup <- function(groups, keys, unassigned = "unassigned") {
    lookup <- rep(names(groups), sapply(groups, length, USE.NAMES = FALSE))
    names(lookup) <- unlist(groups, use.names = FALSE)
    p <- lookup[as.character(keys)]
    p[is.na(p)] <- unassigned
    p
  }
  
  if (!is.null(family))
    df$family <- applyLookup(family, df$item)
  df <- arrange(df, family, item, score)
  #if(normalised)
  df <- ddply(df, .(family, item), transform, 
              value = cumsum(value/(sum(value))))

  df <- ddply(df, .(family, item), transform, previous = c(0, head(value, length(value) - 1)))
  
  df2 <- ddply(df, .(family, item), summarise, indexItem = 1)
  df2$indexItem <- cumsum(df2$indexItem)
  df3 <- ddply(df, .(family), summarise, indexFamily = 1)
  df3$indexFamily <- cumsum(df3$indexFamily)
  df <- merge(df, df2, by = c("family", "item"))
  df <- merge(df, df3, by = "family")
  df <- arrange(df, family, item, score)
  
  affine <- switch(direction,
                   inwards = function(y) (outerRadius - innerRadius) * y + innerRadius,
                   outwards = function(y) (outerRadius - innerRadius) * (1 - y) + innerRadius,
                   stop(paste("Unknown direction")))
  df <- within(df, {
    xmin <- (indexItem - 1) * binSize + (indexItem - 1) *
      spaceItem + (indexFamily - 1) * (spaceFamily - spaceItem)
    xmax <- xmin + binSize
    ymin <- affine(1 - previous)
    ymax <- affine(1 - value)
  })
  
  #if(normalised)
  guidesDF <- data.frame(xmin = rep(df$xmin, length(guides)),
                         y = rep(1 - guides/100, 1, each = nrow(df)))
  #else
  #  guidesDF <- data.frame(xmin = rep(df$xmin, length(guides)),
  #                         y = rep(1 - guides/maxFamily, 1, each = nrow(df)))
  
  
  guidesDF <- within(guidesDF, {
    xend <- xmin + binSize
    y <- affine(y)
  })
  
  
  totalLength <- tail(df$xmin + binSize + spaceFamily, 1)/circleProportion - 0
  
  p <- ggplot(df) + geom_rect(aes(xmin = xmin, xmax = xmax,
                                  ymin = ymin, ymax = ymax, fill = score))
  readableAngle <- function(x) {
    angle <- x * (-360/totalLength) - alphaStart * 180/pi + 90
    angle + ifelse(sign(cos(angle * pi/180)) + sign(sin(angle * pi/180)) == -2, 180, 0)
  }
  
  readableJustification <- function(x) {
    angle <- x * (-360/totalLength) - alphaStart * 180/pi + 90
    ifelse(sign(cos(angle * pi/180)) + sign(sin(angle * pi/180)) == -2, 1, 0)
  }
  
  dfItemLabels <- ddply(df, .(family, item), summarize, xmin = xmin[1])
  dfItemLabels <- within(dfItemLabels, {
    x <- xmin + binSize/2
    angle <- readableAngle(xmin + binSize/2)
    hjust <- readableJustification(xmin + binSize/2)
  })
  col <- NULL
  for(i in 1:length(dfItemLabels$item)) {
    ifelse(dfItemLabels$item[i]=="01"|dfItemLabels$item[i]=="10"|dfItemLabels$item[i]=="20",
           colour <- "black", colour <- "white")
    col <- c(col, colour)
  }
  
  p <- p + geom_text(aes(x = x, label = item, angle = angle, hjust = hjust), 
                     y = 1.02, size = 1.9, vjust = 0.5, 
                     data = dfItemLabels, 
                     col = col)
  
  p <- p + geom_segment(aes(x = xmin, xend = xend, y = y, yend = y),
                        colour = "white", data = guidesDF)
  
  #if(normalised)
  guideLabels <- data.frame(x = 0, y = affine(1 - guides/100),
                            label = paste(""))
  #else
  #  guideLabels <- data.frame(x = 0, y = affine(1 - guides/maxFamily),
  #                            label = paste(guides, " ", sep = ""))
  
  p <- p + geom_text(aes(x = x, y = y, label = label), data = guideLabels,
                     angle = -alphaStart * 180/pi, hjust = 1, 
                     size = 1.9)
  if (familyLabels) {
    familyLabelsDF <- aggregate(xmin ~ family, data = df,
                                FUN = function(s) mean(s + binSize))
    familyLabelsDF <- within(familyLabelsDF, {
      x <- xmin
      angle <- xmin * (-360/totalLength) - alphaStart * 180/pi
    })
    p <- p + geom_text(aes(x = x, label = family, angle = angle),
                       data = familyLabelsDF, y = 1.1, size = 4)
  }
  
  p <- p + theme(panel.background = element_blank(), axis.title.x = element_blank(),
                 axis.title.y = element_blank(), panel.grid.major = element_blank(),
                 panel.grid.minor = element_blank(), axis.text.x = element_blank(),
                 axis.text.y = element_blank(), axis.ticks = element_blank())
  
  p <- p + xlim(0, tail(df$xmin + binSize + spaceFamily, 1)/circleProportion)
  p <- p + ylim(0, outerRadius + 0.2)
  p <- p + coord_polar(start = alphaStart)
  #p <- p + scale_fill_brewer(palette = "Set1", type = "qual")
  p <- p + scale_fill_manual(values = c('birds'="#009E73",
                                        'cicada'="#E69F00", 
                                        'insects'= "#F0E442",
                                        'planes'="#CC79A7", 
                                        'rain'="#0072B2",
                                        'wind'="lightblue",
                                        'quiet'="#999999"))
  p <- p + theme(legend.text=element_text(size=1))
  p <- p + theme(legend.position="none")
}

gympie_clusters365 <- gympie_clusters[1:2555,]
gympie_clusters365 <- gympie_clusters
p1 <- polarHistogram365(gympie_clusters365, 
                        familyLabels = TRUE,
                        circleProportion = 0.98,
                        normalised = FALSE)
p1 <- p1 + ggtitle("Gympie NP") + theme(title = element_text(vjust = -6)) + theme(title = element_text(size=20)) 
p1 <- p1 + theme(plot.margin=unit(c(0,-10,0,0),"mm"))
print(p1)
ggsave('Figures for plos article/Fig14_gym_unedited.tiff', 
       width = 7.5, height = 7.5, dpi = 300, bg = "transparent")

woondum_clusters365 <- woondum_clusters[1:2555,]
woondum_clusters365 <- woondum_clusters
p1 <- polarHistogram365(woondum_clusters365, 
                        familyLabels = TRUE,
                        circleProportion = 0.98,
                        normalised = FALSE)
p1 <- p1 + ggtitle("Woondum NP") + theme(title = element_text(vjust = -6)) + theme(title = element_text(size=20)) 
p1 <- p1 + theme(plot.margin=unit(c(0,-10,0,0),"mm"))
print(p1)
ggsave("Figures for plos article/Fig14_unedited.tiff", 
       width = 7.5, height = 7.5, dpi = 300, bg = "transparent")

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 15 Rain and insect correlation ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# read file containing summary of 30 minute segments
df <- read.csv("polarHistograms/polar_data.csv", header = T)

# convert from 30 minute to 24 hour summaries
length <- nrow(df)
gym_df <- df[1:(length/2),]
woon_df <- df[(1+length/2):length,]

days <- floor(nrow(gym_df)/(48*60))
clust <- unique(df$score)
ref <- 1440/30 * length(clust)
gym_total <- NULL
for(i in 1:days) {
  gym_df_day <- gym_df[(1+ref*(i-1)):(i*ref),]
  for(j in 1:length(clust)) {
    a <- which(gym_df_day$score==clust[j])
    gym_df_cl <- gym_df_day[a,]
    tot <-sum(gym_df_cl$value)
    gym_total <- c(gym_total, tot)
  }
}

woon_total <- NULL
for(i in 1:days) {
  woon_df_day <- woon_df[(1+ref*(i-1)):(i*ref),]
  for(j in 1:length(clust)) {
    a <- which(woon_df_day$score==clust[j])
    woon_df_cl <- woon_df_day[a,]
    tot <-sum(woon_df_cl$value)
    woon_total <- c(woon_total, tot)
  }
}

gym_matrix <- matrix(gym_total, nrow = days, 
                     ncol = length(clust), byrow = TRUE)
woon_matrix <- matrix(woon_total, nrow = days, 
                      ncol = length(clust), byrow = TRUE)

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20160723", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}
# set all dates to "" except the 1st of each month
a <- which(!substr(date.list, 9, 10)=="01")
date.list[a] <- ""

# colours for each class
insect_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
bird_col <- "#009E73"
cicada_col <- "#E69F00"
quiet_col <- "#999999"
plane_col <- "#CC79A7"
na_col <- "white"

a <- which(substr(date.list, 9, 10)=="01")
# repeat date in the next position
date.list[a+1] <- date.list[a]
blank_date_list <- rep("", length(date.list))
#b <- max(n)- a + 1

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# choose the day numbers
n <- 1:100
# choose the rain clusters
rain_clusters <- c(10, 18, 21, 59)
x <- cbind(gym_matrix[n, rain_clusters])

# choose the insect clusters
insect_clusters <- c(1,27,29)
y <- cbind(gym_matrix[n,insect_clusters])

cor(rowSums(x), rowSums(y))
gym_x <- rowSums(x)
gym_y <- rowSums(y)

max_x <- max(gym_x)
max_y <- max(gym_y)

# Woondum
# choose the rain clusters
x <- cbind(woon_matrix[n, rain_clusters])
y <- cbind(woon_matrix[n, insect_clusters])

cor(rowSums(x), rowSums(y))
woon_x <- rowSums(x)
woon_y <- rowSums(y)

max_x <- max(woon_x)
max_y <- max(woon_y)

# Plot an empty plot with no axes or frame
#png(paste("plots\\woondum_Insect",toString(insect_clusters),"_rain",
#          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
#    height = 675, width = 1500)
m <- rbind(c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(2,2,2),
           c(2,2,2))
layout(m)
layout.show(2)

#par(mar=c(1,3,3,0))
tiff("Figures for plos article/Fig15.tiff", width = 2250, 
     height = 1500, units = 'px', res = 300)

par(mar=c(0, 3.4, 1.5, 0),  #, mfcol=c(2,1) ,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(2,2,2),
           c(2,2,2))
layout(m)
layout.show(2)

# space between sets not columns
gap <- 10
# width and spacing of columns
width <- 3
space <- 0.4
# empty plot
plot(x = c(0,((width+space)*(length(n)-1))), type = "n",
     y = c(-(max_x+12),(max_y+12)), xlab="", ylab="",
     frame.plot = FALSE, axes = FALSE) 
ref <- 0
maxim <- 0
for(i in 1:length(woon_x)) {
  rect(ref, gap, ref+width, woon_y[i]+gap, col = insect_col)
  ref <- ref + (width+space)
  if((woon_y[i]+gap) > maxim) {
    maxim <- woon_y[i]+gap
  }
}
ref <- 0
for(i in 1:length(woon_x)) {
  rect(ref, -gap, ref+width, -(woon_x[i]+gap), col = rain_col)
  ref <- ref + (width+space)
  
}
axis(side = 2, at = (seq(0, max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), line = -1.4, cex=2.2)
axis(side = 2, at = -(seq(0,max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), line = -1.4, cex=2.2)
date.ref <- a[which(a > min(n))]
date.ref <- date.ref[1:3]
for(i in 1:length(date.ref)) {
  text(x = ((date.ref[i]-min(n))*(width+space)-1), 
       y = (max(rowSums(y)) - 10), cex= 1.4,  
       paste(date.list[date.ref[i]]), pos = 4)
}
abline(v=((a-min(n))*(width+space)))
#par(font=2, mar=c(2, 3, 3, 0), mfcol=c(2,1),
#            cex = 0.6, cex.axis = 1, cex.main = 1)
mtext(side = 3, paste("WoondumNP - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Insect clusters (", 
                      toString(insect_clusters),")"),
      outer = F, cex = 1)

#par(font=1)
mtext(side = 2, "Minutes per day", line = 1.2)
mtext(side = 3, "a.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -2)
# Perform cross correlation on both the Woondum data
#png(paste("plots\\cross-corr_woondum_insects",toString(insect_clusters),"_rain",
#          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
#    height = 450, width = 600)
#par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
#    cex.axis = 1.2)
ylim <- c(-0.2, 0.58)
par(mar=c(3.8, 4.8, 0, 1), oma= c(0,0,0.5,0),
    cex = 0.6, cex.axis = 1, cex.main = 1)
par(new=T)
ccf(woon_y, woon_x, main = "", bty = "n",
    xlab = "", ylab = "", ylim = ylim)
abline(h=-0.1)
mtext(side = 1, "Lag (days)", line = 2.5)
mtext(side = 2, "Cross-correlation", line = 2.5)
mtext(side = 3, "b.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -22.8)
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 17 PCA diel plot --------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# set the start date in "YYYY-MM-DD" format
start_date <- "2015-06-22"

k1_value <- 25000
k2_value <- 60

# load cluster list
cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",k1_value,
                               "_",k2_value, ".csv", sep = ""))

# Generate a date sequence & locate the first of each month
days <- floor(nrow(cluster_list)/(2*1440))
start <- as.POSIXct(start_date)
interval <- 1440
end <- start + as.difftime(days, units="days")
dates <- seq(from=start, by=interval*60, to=end)
first_of_month <- which(substr(dates, 9, 10)=="01")

# Prepare civil dawn, civil dusk and sunrise and sunset times
#civil_dawn <- read.csv("data/Sunrise_Sunset_Solar Noon_protected.csv", header=T)
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2016 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2016.csv")
civil_dawn <- rbind(civil_dawn_2015, civil_dawn_2016)
# set the start date in "YYYY-MM-DD" format
start_date <- "2015-06-22"
start <- as.POSIXct(start_date)

a <- which(civil_dawn$dates==paste(substr(start, 1,4), substr(start, 6,7),
                                   substr(start, 9,20),sep = "-"))
reference <- a:(a+days-1)
civil_dawn_times <- civil_dawn$CivSunrise[reference]
civil_dusk_times <- civil_dawn$CivSunset[reference]
sunrise_times <- civil_dawn$Sunrise[reference]
sunset_times <- civil_dawn$Sunset[reference]
start <- as.POSIXct(start_date)
# find the minute of civil dawn for each day
civ_dawn <- NULL
for(i in 1:length(civil_dawn_times)) {
  hour <- as.numeric(substr(civil_dawn_times[i], 1,1))
  min <- as.numeric(substr(civil_dawn_times[i], 2,3))
  minute <- hour*60 + min
  civ_dawn <- c(civ_dawn, minute)
}

civ_dusk <- NULL
for(i in 1:length(civil_dusk_times)) {
  hour <- as.numeric(substr(civil_dusk_times[i], 1,2)) 
  min <- as.numeric(substr(civil_dusk_times[i], 3,4))
  minute <- hour*60 + min
  civ_dusk <- c(civ_dusk, minute)
}

sunrise <- NULL
for(i in 1:length(sunrise_times)) {
  hour <- as.numeric(substr(sunrise_times[i], 1,1))
  min <- as.numeric(substr(sunrise_times[i], 2,3))
  minute <- hour*60 + min
  sunrise <- c(sunrise, minute)
}

sunset <- NULL
for(i in 1:length(sunset_times)) {
  hour <- as.numeric(substr(sunset_times[i], 1,2)) 
  min <- as.numeric(substr(sunset_times[i], 3,4))
  minute <- hour*60 + min
  sunset <- c(sunset, minute)
}

folder <- "C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\"
pca.coefficients <- read.csv(paste(folder, "pca_coefficients.csv",sep = ""), header=T)
ds6 <- pca.coefficients[,2:4]

##### Normalise the dataset ################ 
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
# Create ds3.norm_2_98 for kmeans, clara, hclust
# a dataset normalised between 1.5 and 98.5%
ds.coef_min_max <- ds6
min.values <- NULL
max.values <- NULL
for (i in 1:length(ds6)) {
  min <- unname(quantile(ds6[,i], probs = 0.0, na.rm = TRUE))
  max <- unname(quantile(ds6[,i], probs = 1.0, na.rm = TRUE))
  min.values <- c(min.values, min)
  max.values <- c(max.values, max)
  ds.coef_min_max[,i]  <- normalise(ds.coef_min_max[,i], min, max)
}
library(raster)
#png("GympieNP_diel_pca.png",width = 1700, height = 1000, units="px")
r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),1]
values(g) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),2]
values(b) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),3]

rgb = rgb <-stack(r*255,g*255,b*255)

r <- values(r)*255
g <- values(g)*255
b <- values(b)*255
rgb<- cbind(r,g,b)
rgb <- data.frame(rgb)
rgb$hex <- NULL
for(i in 1:nrow(rgb)) {
  rgb$hex[i] <- rgb(r[i], g[i], b[i], maxColorValue=255)
}

k=1
ref <- c(0, days*1440)
# generate a date sequence and locate the first of the month
days <- nrow(cluster_list)/(2*1440)
start <- as.POSIXct(paste(start_date))
interval <- 1440
end <- start + as.difftime(days, units="days")
dates <- seq(from=start, by=interval*60, to=end)
if(k==1) {
  tiff("Figures for plos article/Fig17.tiff", width = 2250, 
       height = 900, units = 'px', res = 300)
}
if(k==2) {
  tiff("Figures for plos article/Fig17a.tiff", width = 2250, 
       height = 900, units = 'px', res = 300)
}
par(mar=c(0.9, 2.7, 0.9, 2.7), mgp = c(3,0.8,0),
    cex = 0.6, cex.axis = 1.2, cex.main = 1)
# Plot an empty plot with no axes or frame
#plot(c(0,1440), c(111,1), type = "n", axes=FALSE, 
#     frame.plot=FALSE,
#     xlab="", ylab="") #, asp = 398/1440)

# draw coloured polygons row by row
site <- c("GympieNP", "WoondumNP")
days <- 111
#k <- 1 
# set the rows starting at the top of the plot
plot(c(0,1440), c(days,1), type = "n", axes=FALSE, 
     frame.plot=FALSE,
     xlab="", ylab="") #, asp = 398/1440)
# Create the heading
k <- 1
mtext(side=3, line = -0.5,
      paste("PCA diel plot - ", site[k]," ", format(dates[1], "%d %B %Y")," - ", 
            format(dates[length(dates)-1], "%d %B %Y"), 
            sep=""))
ref <- 1
for(j in days:1) {
  # set the column starting on the left
  for(k in 1:1440) {
    # Plot an empty plot with no axes or frame
    # draw a square for each minute in each day 
    # using the polygon function mapping the cluster
    # number to a colour
    polygon(c(k,k,k+1,k+1), c(j,(j-1),(j-1),j),
            col=rgb$hex[ref],
            border = NA)
    ref <- ref + 1
  }
}

# draw horizontal lines
first_of_month <- which(substr(dates, 9, 10)=="01")
first_of_each_month <- days - first_of_month + 1
for(i in 1:length(first_of_month)) {
  lines(c(1,1441), c(first_of_each_month[i], 
                     first_of_each_month[i]), 
        lwd=1.1, lty = 3)
}
# draw vertical lines
at <- seq(0,1440, 240) + 1
for(i in 1:length(at)) {
  lines(c(at[i], at[i]), c(1,days), lwd=1.1, lty=3)
}
# label the x axis
axis(1, tick = FALSE, at = at, 
     labels = c("12 am","4 am",
                "8 am","12","4 pm",
                "8 pm","12 pm"), line = -1.4)
# plot the left axes
axis(side = 2, at = first_of_each_month, tick = FALSE, 
     labels=format(dates[first_of_month],"%b %Y"), 
     las=1, line = -2.5)
#axis(side = 2, at = c(days), tick = FALSE, 
#     labels=format(dates[1],"%d %b %Y"), 
#     las=1, line = -2.5)
# plot the left axes
axis(side = 4, at = first_of_each_month, tick = FALSE, 
     labels=format(dates[first_of_month],"%b %Y"), 
     las=1, line = -2.5)
#axis(side = 4, at = c(days), tick = FALSE, 
#     labels=format(dates[1],"%d %b %Y"), 
#     las=1, line = -2.5)

at <- seq(0, 1440, 240)

# draw dotted line to show civil-dawn
for(i in days:1) {
  lines(c(civ_dawn[1:days]), c(days:1),  
        lwd=1.2, lty=2, col="yellow")
}
# draw dotted line to show civil-dusk
for(i in days:1) {
  lines(c(civ_dusk[1:days]), c(days:1),  
        lwd=1.2, lty=2, col="yellow")
}
# draw dotted line to show sunrise
for(i in days:1) {
  lines(c(sunrise[1:days]), c(days:1),  
        lwd=1.2, lty=2, col="yellow")
}
# draw dotted line to show sunset
for(i in days:1) {
  lines(c(sunset[1:days]), c(days:1),  
        lwd=1.2, lty=2, col="yellow")
}
dev.off()

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# plot 13 Four Dot Matrix plots -----------------
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# choose a colour version to be used in col_func.R 
version <- "colourblind" # choice: "ordinary" or "colourblind"

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)

site1 <- rep("GympieNP", nrow(cluster_list)/2)
site2 <- rep("WoondumNP", nrow(cluster_list)/2)
site <- c(site1, site2)
rm(site1, site2)

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20160723", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
#any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)
# duplicate dates 1440 times
dates <- rep(dates, each = 1440)
dates <- rep(dates, 2)
# Add site and dates columns to dataframe
cluster_list <- cbind(cluster_list, site, dates)

# determine the number of days in each month at each site
days_per_month <- NULL
year_month <- unique(substr(cluster_list$dates,1,6))
for(i in 1:length(year_month)) {
  count <- which(substr(cluster_list$dates, 1, 6)==year_month[i])
  count <- length(count)/1440
  days_per_month <- c(days_per_month, count/2)
}
days_per_period <- rep(days_per_month, each =12)
days_per_period <- rep(days_per_period, 2)

# Check for col_func in globalEnv otherwise source function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# Generate colour list using col_func
# Note col_func requires a csv file containing customed
# colour information for each cluster 

col_func(cluster_colours, version = version)

cluster_list$descpt <- 0
list <- unique(cluster_colours$Feature)
list <- as.character(list[1:(length(list)-1)])

# old cluster classes 
#rain <- c(59,18,10,54,2,21,38,60)
#wind <- c(42,47,51,56,52,45,8,40,24,19,46,28,9,25,30,20)
#birds <- c(43,37,57,3,58,11,33,15,14,39,4)
#insects <- c(29,17,1,27,22,26)
#cicada <- c(48,44,34,7,12,32,16)
#plane <- c(49,23)
#quiet <- c(13,5,6,53,36,31,50,35,55,41)
#na <- 61

# define cluster classes 
rain <- c(2,10,17,18,21,54,59,60) 
wind <- c(9,19,20,24,25,30,40,42,45,46,47,51,52,56)
birds <- c(3,11,14,15,28,33,37,39,43,57,58)
insects <- c(1,4,22,26,27,29)
cicada <- c(7,8,12,16,32,34,44,48)
planes <- c(49,23)
quiet <- c(5,6,13,31,35,36,38,41,50,53,55)
na <- 61

# Add a description column
for(i in 1:(length(unique(cluster_list$cluster_list))-1)) {
  # Obtain a list of clusters corresponding to the 
  # first feature in the list
  a <- which(as.character(cluster_colours$Feature)==list[i])
  for(j in 1:length(a)) {
    a1 <- which(cluster_list$cluster_list==a[j])  
    cluster_list[a1,5] <- as.character(list[i])
  }
}
# colours for each class
insect_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
bird_col <- "#009E73"
cicada_col <- "#E69F00"
quiet_col <- "#999999"
plane_col <- "#CC79A7"
na_col <- "white"

# complete the description column by adding NAs
a <- which(cluster_list$descpt=="0")
cluster_list$descpt[a] <- "NA"
cluster_colours$col <- "0"
if(version=="colourblind") {
  cluster_colours$Feature <- NULL
  for(i in 1:61) {
    if(i %in% rain) {
      cluster_colours$Feature[i] <- "RAIN"
      cluster_colours$col[i] <- rain_col
    }
    if(i %in% wind) {
      cluster_colours$Feature[i] <- "WIND"
      cluster_colours$col[i] <- wind_col
    }
    if(i %in% birds) {
      cluster_colours$Feature[i] <- "BIRD"
      cluster_colours$col[i] <- bird_col
    }
    if(i %in% insects) {
      cluster_colours$Feature[i] <- "INSECT"
      cluster_colours$col[i] <- insect_col
    }
    if(i %in% cicada) {
      cluster_colours$Feature[i] <- "CICADA"
      cluster_colours$col[i] <- cicada_col
    }
    if(i %in% plane) {
      cluster_colours$Feature[i] <- "PLANE"
      cluster_colours$col[i] <- plane_col
    }
    if(i %in% quiet) {
      cluster_colours$Feature[i] <- "QUIET"
      cluster_colours$col[i] <- quiet_col
    }
    if(i %in% na) {
      cluster_colours$Feature[i] <- "NA"
      cluster_colours$col[i] <- na_col
    }
  }
}
if(version=="ordinary") {
  feature_colours <- read.csv("data/datasets/Feature_colours.csv")
}

if(version=="colourblind") {
  feature_colours <- NULL
  feature_colours$Feature <- c("INSECTS", "BIRDS", "WIND",
                               "RAIN", "CICADAS", "QUIET",
                               "PLANES", "NA")
  feature_colours$colour <- c(insect_col, bird_col, wind_col,
                              rain_col, cicada_col, quiet_col,
                              plane_col, na_col)
}

#
cluster_list$col <- NULL
unique_description <- unique(cluster_list$descpt)
# The factors are simplified for the colourblind version because there
# are only seven colours + white used
cluster_list$class <- "0"
cluster_list$col <- "0"
if(version=="colourblind") {
  a <- which(cluster_list$descpt=="BIRDS")
  cluster_list$class[a] <- as.character("BIRDS")
  cluster_list$col[a] <- bird_col
  a <- which(cluster_list$descpt=="NA")
  cluster_list$class[a] <- as.character("NA")
  cluster_list$col[a] <- na_col
  a <- which(cluster_list$descpt=="CICADAS")
  cluster_list$class[a] <- as.character("CICADAS")
  cluster_list$col[a] <- cicada_col
  a <- which(cluster_list$descpt=="QUIET VERY")
  cluster_list$class[a] <- as.character("QUIET")
  cluster_list$col[a] <- quiet_col
  a <- which(cluster_list$descpt=="QUIET/planes")
  cluster_list$class[a] <- as.character("QUIET")
  cluster_list$col[a] <- quiet_col
  a <- which(cluster_list$descpt=="INSECTS")
  cluster_list$class[a] <- as.character("INSECTS")
  cluster_list$col[a] <- insect_col
  a <- which(cluster_list$descpt=="RAIN LIGHT")
  cluster_list$class[a] <- as.character("RAIN")
  cluster_list$col[a] <- rain_col
  a <- which(cluster_list$descpt=="QUIET FAIRLY")
  cluster_list$class[a] <- as.character("QUIET")
  cluster_list$col[a] <- quiet_col
  a <- which(cluster_list$descpt=="QUIET MOSTLY")
  cluster_list$class[a] <- as.character("QUIET")
  cluster_list$col[a] <- quiet_col
  a <- which(cluster_list$descpt=="WIND MODERATE")
  cluster_list$class[a] <- as.character("WIND")
  cluster_list$col[a] <- wind_col
  a <- which(cluster_list$descpt=="RAIN MODERATE")
  cluster_list$class[a] <- as.character("RAIN")
  cluster_list$col[a] <- rain_col
  a <- which(cluster_list$descpt=="QUIET INSECTS")
  cluster_list$class[a] <- as.character("QUIET")
  cluster_list$col[a] <- quiet_col
  a <- which(cluster_list$descpt=="LOW FREQUENCY")
  cluster_list$class[a] <- as.character("PLANES")
  cluster_list$col[a] <- plane_col
  a <- which(cluster_list$descpt=="WIND STRONG")
  cluster_list$class[a] <- as.character("WIND")
  cluster_list$col[a] <- wind_col
  a <- which(cluster_list$descpt=="WIND SLIGHT")
  cluster_list$class[a] <- as.character("WIND")
  cluster_list$col[a] <- wind_col
  a <- which(cluster_list$descpt=="MORNING CHORUS")
  cluster_list$class[a] <- as.character("BIRDS")
  cluster_list$col[a] <- bird_col
  a <- which(cluster_list$descpt=="WIND VERY STRONG")
  cluster_list$class[a] <- as.character("WIND")
  cluster_list$col[a] <- wind_col
  a <- which(cluster_list$descpt=="BIRDS LOUD")
  cluster_list$class[a] <- as.character("BIRDS")
  cluster_list$col[a] <- bird_col
  a <- which(cluster_list$descpt=="RAIN HEAVY")
  cluster_list$class[a] <- as.character("RAIN")
  cluster_list$col[a] <- rain_col
  a <- which(cluster_list$descpt=="PLANES")
  cluster_list$class[a] <- as.character("PLANES")
  cluster_list$col[a] <- plane_col
}
a <- which(cluster_list$site=="GympieNP")
gym_cluster_list <- cluster_list[a,]
a <- which(cluster_list$site=="WoondumNP")
woon_cluster_list <- cluster_list[a,]

# Using dom_agg_cl_gym and feature_colours
# This plot takes 3-4 minutes to generate
shape <- "polygon"
dates1 <- unique(dates)
site <- "WoondumNP"
site <- "GympieNP"
if(site=="GympieNP") {
  file <- "gym_cluster_list"  
}
if(site=="WoondumNP") {
  file <- "woon_cluster_list"  
}

tiff("Figures for plos article/Fig13.tiff", width = 2250, 
     height = 580, units = 'px', res = 300)
par(mar = c(0.65, 0.65, 0, 0), mfrow = c(1,4), 
    cex = 1, cex.axis = 1, cex.main = 2.4,
    oma=c(0.4, 0.15, 0, 0.2))
days <- c(59, 108, 185, 301)
ref2 <- 0
for(l in days) { #length(dates1)) {
  df <- get(file)[(1440*l-1440+1):(l*1440),]
  # Plot an empty plot with no axes or frame
  plot(c(0, 1440), c(1440, 0), 
       type = "n",axes=FALSE, frame.plot=FALSE,
       xlab="", ylab="")
  for(i in 1:length(feature_colours$Feature)) {
    feature <- as.character(feature_colours$Feature[i])
    a <- which(df$class==feature)
    for(j in a) {
      for(k in a) {
        ref <- i
        if(version=="ordinary") {
          R_code <- intToHex(feature_colours[ref,2])
          # add padding if necessary
          if(nchar(R_code)==1) {
            R_code <- paste("0", R_code, sep="")
          }
          G_code <- intToHex(feature_colours[ref,3])
          if(nchar(G_code)==1) {
            G_code <- paste("0", G_code, sep="")
          }
          B_code <- intToHex(feature_colours[ref,4])
          if(nchar(B_code)==1) {
            B_code <- paste("0", B_code, sep="")
          }
          col_code <- paste("#",
                            R_code, 
                            G_code,
                            B_code,
                            sep = "")
        }
        if(version=="colourblind") {
          col_code <- feature_colours$colour[i]
        }
        if(shape=="polygon") {
          polygon(c(k-1,k-1,k,k), c((j-1),j, j,(j-1)),
                  col=col_code, border = NA)  
        }
      }
    }
  }
  # x axis labels
  at <- seq(1, 1441, by = 120)
  axis(1, at = at, line = -0.3, 
       labels = c("0","2","4","6","8","10",
                  "12","14","16","18","20",
                  "22", "24h"), cex.axis = 0.42, 
       las=T, pos=NA, lwd=0.5, tck=-0.02,
       mgp = c(0.2, (-0.22), 0.6))
  x <- c(seq(120, 1440, 120))
  y <- c(seq(121,1441, 120))
  segments(x0 = x, y0 = 0, x1 = x, y1 = 1440,
           lty = 2, lwd = 0.1)
  segments(x0 = 0, y0 = y, x1 = 1441, y1 = y,
           lty = 2, lwd = 0.1)
  # y axis labels
  at <- seq(0, 1440, by = 120)
  axis(2, at = at, line = -0.3, 
       labels = c("0","2","4","6","8","10",
                  "12","14","16","18","20",
                  "22", "24h"), cex.axis = 0.42, 
       las=T, pos=NA, lwd=0.5, tck=-0.02,
       mgp = c(1, 0.2, 0))
  # y axis ticks
  #at <- seq(1, 1441, by = 10)
  #axis(2, line = 1, at = at, tick = TRUE,
  #     labels = FALSE, pos=NA)
  segments(x0 = 1, y0 = 1, x1 = 1440, y1 = 1440,
           lwd=0.02, lty = 1)
  ref2 <- ref2 + 1
  season <- c("a. Winter", "b. Spring", 
              "c. Summer", "d. Autumn")
  mtext(side = 1, line = 0.1, 
        paste(season[ref2], ", ",substr(dates1[l],7,8), "-",
              substr(dates1[l],5,6), "-", substr(dates1[l],1,4),
              sep = ""),
        cex = 0.8)
}
dev.off()

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# plot 16 Winter and summer onto a Sammon map--------------
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# needed for pam function in order to find the medoids
library(cluster) 

# plot SAMMON projection
library(MASS)
# see below on how this is generated
medoids1 <- read.csv("medoids_all_data.csv", header = T)
distances <- as.matrix(dist(medoids1))
clusters.sam <- sammon(distances, k=2)

# size of clusters
clust_sizes <- read.csv("data/2hour_plots_25000_60/Summary_25000_60_annotated.csv", header = T)
clust_sizes <- clust_sizes$Cluster.total
clust_sizes1 <- clust_sizes

clusters1 <- NULL
clusters1$clusters <- as.numeric(1:length(colours))
clusters1$points1 <- clusters.sam$points[,1]
clusters1$points2 <- clusters.sam$points[,2]
clusters1$size <- clust_sizes[1:60]
clusters1$colours <- NULL
clusters1 <- data.frame(clusters1)
clusters1$clusters <- as.numeric(1:length(clusters1$points1))
clusters1$radius <- sqrt(clusters1$size)

# colours for each class
insects <- "#F0E442"
rain <- "#0072B2"
wind <- "#56B4E9"
birds <- "#009E73"
cicadas <- "#E69F00"
quiet <- "#999999"
planes <- "#CC79A7"

clusters1$colours <- "abcd"
clusters1$border <- "abcd"
clusters1$winter1 <- "n"
clusters1$windter2 <- "n"
clusters1$summer1 <- "n"
clusters1$summer2 <- "n"
#clusters1$colours[1] <- insects
# set the circle and border colours
clusters1[1, 6:11]  <-  c(insects, insects,"n","n","n","n")
clusters1[2, 6:11]  <-  c(rain, birds,"n","n","n","n")
clusters1[3, 6:11]  <-  c(birds, birds,"y","y","n","n")
clusters1[4, 6:11]  <-  c(insects, birds,"n","n","n","n")
clusters1[5, 6:11]  <-  c(quiet, quiet,"y","y","n","n")
clusters1[6, 6:11]  <-  c(quiet, quiet,"n","n","n","n")
clusters1[7, 6:11]  <-  c(cicadas, birds,"n","n","y","y")
clusters1[8, 6:11]  <-  c(cicadas, birds,"n","n","y","y")
clusters1[9, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[10, 6:11]  <-  c(rain, rain,"n","n","n","n")
clusters1[11, 6:11]  <-  c(birds, birds,"y","y","n","n")
clusters1[12, 6:11]  <-  c(cicadas, cicadas,"n","n","y","y")
clusters1[13, 6:11]  <-  c(quiet, quiet,"y","y","y","y")
clusters1[14, 6:11]  <-  c(birds, birds,"y","y","n","n")
clusters1[15, 6:11]  <-  c(birds, birds,"n","n","n","n")
clusters1[16, 6:11]  <-  c(cicadas, cicadas,"n","n","n","n")
clusters1[17, 6:11]  <-  c(rain, insects,"n","n","n","n")
clusters1[18, 6:11]  <-  c(rain, rain,"n","n","n","n")
clusters1[19, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[20, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[21, 6:11]  <-  c(rain, rain,"n","n","n","n")
clusters1[22, 6:11]  <-  c(insects, birds,"n","n","y","n")
clusters1[23, 6:11]  <-  c(planes, planes,"n","n","n","n")
clusters1[24, 6:11]  <-  c(wind, cicadas,"n","n","n","n")
clusters1[25, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[26, 6:11]  <-  c(insects, wind,"n","n","n","n")
clusters1[27, 6:11]  <-  c(insects, insects,"n","n","y","y")
clusters1[28, 6:11]  <-  c(birds, insects,"n","n","n","n")
clusters1[29, 6:11]  <-  c(insects, insects,"n","n","y","y")
clusters1[30, 6:11]  <-  c(wind, quiet,"n","n","n","n")
clusters1[31, 6:11]  <-  c(quiet, quiet,"y","y","n","n")
clusters1[32, 6:11]  <-  c(cicadas, cicadas,"n","n","n","n")
clusters1[33, 6:11]  <-  c(birds, birds,"n","n","n","n")
clusters1[34, 6:11]  <-  c(cicadas, cicadas,"n","n","y","y")
clusters1[35, 6:11]  <-  c(quiet, quiet,"y","y","y","n")
clusters1[36, 6:11]  <-  c(quiet, planes,"n","n","n","n")
clusters1[37, 6:11]  <-  c(birds, birds,"y","y","n","n")
clusters1[38, 6:11]  <-  c(quiet, quiet,"n","n","n","n")
clusters1[39, 6:11]  <-  c(birds, planes,"y","y","n","n")
clusters1[40, 6:11]  <-  c(wind, birds,"n","n","n","n")
clusters1[41, 6:11]  <-  c(quiet, quiet,"y","y","n","n")
clusters1[42, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[43, 6:11]  <-  c(birds, birds,"y","y","n","n")
clusters1[44, 6:11]  <-  c(cicadas, cicadas,"n","n","n","n")
clusters1[45, 6:11]  <-  c(wind, planes,"n","n","n","n")
clusters1[46, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[47, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[48, 6:11]  <-  c(cicadas, cicadas,"n","n","y","y")
clusters1[49, 6:11]  <-  c(planes, planes,"n","n","n","n")
clusters1[50, 6:11]  <-  c(quiet, insects,"n","n","n","n")
clusters1[51, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[52, 6:11]  <-  c(wind, wind,"n","n","n","n") 
clusters1[53, 6:11]  <-  c(quiet, quiet,"n","n","n","n")
clusters1[54, 6:11]  <-  c(rain, birds,"n","n","n","n")
clusters1[55, 6:11]  <-  c(quiet, quiet,"n","n","n","n")
clusters1[56, 6:11]  <-  c(wind, wind,"n","n","n","n")
clusters1[57, 6:11]  <-  c(birds, wind,"n","n","n","n")
clusters1[58, 6:11]  <-  c(birds, birds,"n","n","n","n")
clusters1[59, 6:11]  <-  c(rain, rain,"n","n","n","n") 
clusters1[60, 6:11]  <-  c(rain , birds,"n","n","n","n") 
clusters1 <- clusters1[order(-clusters1$size),]
leg_col <- as.character(c(rain, birds, cicadas, wind, planes, quiet, insects))
leg_names <- c("rain", "birds", "cicadas", "wind","planes", "quiet","insects")
library(plotrix) # needed for draw.cirle function
max <- 0.0009

tiff("Figures for plos article/Fig16.tiff", 
     width = 2250, height = 870, units = 'px', res = 300)
par(mar=c(0.4, 0, 0.6, 0), mfrow=c(1,2),
    cex = 1, cex.axis = 1, cex.main = 1.2)
for(i in 1:2) {
    # plot an empty plot
  if(i==1) {
    main <- " a. Winter"  
  }
  if(i==2) {
    main <- " b. Summer"  
  }
  plot(clusters1$points1, 
       clusters1$points2, type = "n",
       main = main, 
       xaxt='n', yaxt='n',
       xlab = "",ylab = "", frame.plot=FALSE,
       xlim = c((min(clusters1$points1)-0.05),
                (max(clusters1$points1)+0.45)),
       ylim = c((min(clusters1$points2)-0.11),
                (max(clusters1$points2)+0.13)),
       cex.axis=1, cex.lab=0.6, las=1, cex.main=1)
  #mtext(side=2, "y", las=1, cex = 3, line = 3.5)
  #abline(h = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
  #abline(v = seq(-10, 10, 0.1), col = "lightgray", lty = 3)
  for(j in 1:nrow(clusters1)) {
    draw.circle(clusters1$points1[j],
                clusters1$points2[j], 
                radius = max*clusters1$radius[j],
                col = clusters1$colours[j],
                border = clusters1$border[j],
                lwd = 4)
  }
  # plot the x and y axis to form four quadrants
  abline(h = 0, col = "gray50", lwd = 0.4)
  abline(v = 0, col = "gray50", lwd = 0.4)
  # plot the plot legend
  leg <-legend("topright", title="Classes", 
               col = leg_col, bty = "n", 
               cex=0.6, leg_names , y.intersp = 1.2) 
  for(j in 1:length(leg$text$x)) {
    draw.circle(leg$text$x[j]-0.06, leg$text$y[j]-0.005, 
                radius = 0.055,
                col = leg_col[j],
                border = "white")
  }  
  # add family to fonts list use windowsFonts() to check current
  windowsFonts(A = windowsFont("Times New Roman"))
  text(x = 1.1, y = 1.1, "I", cex = 0.6, family="A", font = 2)
  text(x = -1.6, y = 1.1, "II", cex = 0.6, family="A", font = 2)
  text(x = -1.6, y = -1.05, "III", cex = 0.6, family="A", font = 2)
  text(x = 1.1, y = -1.05, "IV", cex = 0.6, family="A", font = 2)
  if(i == 1) {
    text(x = 0.45, y = -1.1, "Gym-2015-08-19", 
         cex = 0.6, family="A", font = 4)
    list1 <- c(14,43,37,39,11,31,35,31,13,5,37)
    for(m in 1:length(list1)) {
      a <- which(clusters1$clusters==list1[m])
      list2 <- c(39,37,39,11,3,35,13,13,5,41,11)
      b <- which(clusters1$clusters==list2[m])
      segments(x0 = clusters1$points1[a], 
               y0 = clusters1$points2[a],
               x1 = clusters1$points1[b],
               y1 = clusters1$points2[b],
               lwd = 1.4)
    }
  }
  if(i == 2) {
    text(x = 0.45, y = -1.1, "Gym-2015-12-23", 
         cex = 0.6, family="A", font = 4)
    list1 <- c(48,34,48,22,35,29)
    for(m in 1:length(list1)) {
      a <- which(clusters1$clusters==list1[m])
      list2 <- c(34,12,12,29,13,35)
      b <- which(clusters1$clusters==list2[m])
      segments(x0 = clusters1$points1[a], 
               y0 = clusters1$points2[a],
               x1 = clusters1$points1[b],
               y1 = clusters1$points2[b],
               lwd = 1.6)
    }
  }
  for(j in 1:nrow(clusters1)) {
    if(i==1) {
      if(clusters1$winter1[j]=="y")
        draw.circle(clusters1$points1[j],
                    clusters1$points2[j], 
                    radius = 0.08,
                    col = clusters1$colours[j],
                    border = "black",
                    lwd = 1.6)
      if(clusters1$windter2[j]=="y")
        draw.circle(clusters1$points1[j],
                    clusters1$points2[j]+0.09, 
                    radius = 0.04,
                    col = clusters1$colours[j],
                    border = "black",
                    lwd = 1.2)
    }
    if(i==2) {
      if(clusters1$summer1[j]=="y")
        draw.circle(clusters1$points1[j],
                    clusters1$points2[j], 
                    radius = 0.08,
                    col = clusters1$colours[j],
                    border = "black",
                    lwd = 1.6)
      if(clusters1$summer2[j]=="y")
        draw.circle(clusters1$points1[j],
                  clusters1$points2[j]+0.09, 
                  radius = 0.04,
                  col = clusters1$colours[j],
                  border = "black",
                  lwd = 1.2)
    }
    for(j in 1:nrow(clusters1)) {
    # plot the cluster numbers
    text(clusters1$points1, clusters1$points2, 
         labels = as.character(clusters1$clusters), cex = 0.5)
    }
  }
}
dev.off()
