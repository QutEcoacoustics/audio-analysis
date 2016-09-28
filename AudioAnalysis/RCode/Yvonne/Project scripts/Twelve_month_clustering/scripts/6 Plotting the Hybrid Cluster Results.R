# Title:  Plotting the Hybrid Cluster Results
# Author: Yvonne Phillips
# Date:  24 September 2016

# Description:  This code plots various plots including:
# 1.  ID3 separation
# 2.  Composite spectrogram images 
# 3.  2 hour plots

# remove all objects in the global environment
rm(list = ls())
###########################################
# Plot the ID3 values 
###########################################
ID3_values <- read.csv("ID3_values.csv")
#col <- c("red", "blue", "green", "orange", 
#         "black", "purple","magenta")
col <- rep("black",8)
pch = c(15,20,17,18,16,21,22,23)
labels <- as.character(seq(12500, 30000, 2500))
x <- seq(5, 100, 5)
ylimit <- c(1.1, 2.3)
xlimit <- c(40,100)

png("ID3_separation_plot.png", height = 600, width = 600)
par(mar=c(3.5, 3.5, 2.8, 1.5), cex=1.5)
#plot(x,ID3_values[1:20,3], type = "o", 
#     col=col[1], pch=15, ylim=ylim, las=1)
#par(new=T)
#plot(ID3_values[21:40,3], type = "o", 
#     col=col[2], pch=16, ylim=ylim,
#     yaxt="n",xaxt="n")
#par(new=T)
plot(x,ID3_values[41:60,3], type = "o", 
     col=col, pch=pch[2], ylim=ylimit,
     xlab = "k2", ylab = "ID3 separation", las=1, 
     xlim = xlimit, mgp = c(2.1, 0.6, 0))
mtext("ID3 separation (clustering of 24 hour fingerprints)", side = 3, 
      cex = 1.8, line = 1.2)
mtext("12 days out of 796 days", side = 3, 
      cex = 1.4, line = 0.3)
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
#par(new=T)
#plot(x,ID3_values[121:140,3], type = "o", 
#     col=col[6], pch=pch[6], ylim=ylimit,
#     yaxt="n",xaxt="n", xlab = "", 
#     ylab = "", xlim = xlimit)
#par(new=T)
#plot(x,ID3_values[141:160,3], type = "o", 
#     col=col[7], pch=pch[7], ylim=ylimit,
#     yaxt="n",xaxt="n", xlab = "", 
#     ylab = "", xlim = xlimit)
legend("topright", pch = pch[2:5], lty = c(1),
       title="k1 values", col = col[3:6], bty = "n", 
       cex=1, labels[3:6], y.intersp = 0.85) 
dev.off()
rm(ID3_values)

###########################################
# Plot the composite images of each cluster
###########################################
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

# FUNCTION (CLUSTER_IMAGE)
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
  
  png(filename = paste("data/clusterImages_", k1_value,"_",
                       k2_value, "/ClusterImage_Cluster",
                       clust_num,"_", k1_value, "_", k2_value, 
                       ".png", sep = ""), width = 640, height = 668, 
      units = "px", antialias = "none")
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
#for(j in 3:k2_value) {
dev.off()
for(j in 1:k2_value) {
  print(paste("starting", j, Sys.time(), sep = " "))
  cluster_image(j) # call function k2 times
}

#################################################################
# Plotting 2hour_plot - time of day per month
################################################################
rm(list = ls())

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
finish <-  strptime("20160723", format="%Y%m%d")
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

# FUNCTION
plot_2hour_files <- function(clust_num) {
  if(clust_num<10) {
  png(filename = paste("data/2hour_plots_", k1_value, "_",
                       k2_value,"/2hourPlot_Cluster",
                      "0",clust_num,"_", k1_value, "_", k2_value, 
                       ".png", sep = ""), width = 800, height = 668, 
      units = "px")
  }
  if(clust_num >= 10) {
    png(filename = paste("data/2hour_plots_", k1_value, "_",
                         k2_value,"/2hourPlot_Cluster",
                         clust_num,"_", k1_value, "_", k2_value, 
                         ".png", sep = ""), width = 800, height = 668, 
        units = "px")
  }
  # produce a list of when each cluster occured
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
    if(substr(months[i],5,6)=="01") {months[i] <- paste("Jan", substr(months[i],1,4), sep=" ")}
    if(substr(months[i],5,6)=="02") {months[i] <- paste("Feb", substr(months[i],1,4), sep=" ")}
    if(substr(months[i],5,6)=="03") {months[i] <- paste("Mar", substr(months[i],1,4), sep=" ")}  
    if(substr(months[i],5,6)=="04") {months[i] <- paste("Apr", substr(months[i],1,4), sep=" ")}  
    if(substr(months[i],5,6)=="05") {months[i] <- paste("May", substr(months[i],1,4), sep=" ")}
    if(substr(months[i],5,6)=="06") {months[i] <- paste("Jun", substr(months[i],1,4), sep=" ")}
    if(substr(months[i],5,6)=="07") {months[i] <- paste("Jul", substr(months[i],1,4), sep=" ")}  
    if(substr(months[i],5,6)=="08") {months[i] <- paste("Aug", substr(months[i],1,4), sep=" ")}  
    if(substr(months[i],5,6)=="09") {months[i] <- paste("Sept", substr(months[i],1,4), sep=" ")}
    if(substr(months[i],5,6)=="10") {months[i] <- paste("Oct", substr(months[i],1,4), sep=" ")}
    if(substr(months[i],5,6)=="11") {months[i] <- paste("Nov", substr(months[i],1,4), sep=" ")}  
    if(substr(months[i],5,6)=="12") {months[i] <- paste("Dec", substr(months[i],1,4), sep=" ")}  
  }
  
  started <- seq(1,nrow(cluster_reference),12)
  finished <- started + 11
  
  months <- rep(months, 2)
  barplot(cluster_reference$output[1:nrow(cluster_reference)/2],
          las=1)
  num_of_plots <- length(started)
  max <- max(cluster_reference$output)
  if(num_of_plots<=14){
    par(mfrow=c(2,ceiling(num_of_plots/2)), 
        mar=c(3,3,2,1), oma=c(2,5,2.5,2), 
        cex.axis=1.6)
  }
  if(num_of_plots > 14){
    par(mfrow=c(4,ceiling(num_of_plots/4)), mar=c(3,3,2,1), oma=c(2,5,2.5,2), cex.axis=1.6)
    ref1<- ceiling(num_of_plots/4)
    ref2 <- num_of_plots/2
    for(i in 1:ref1) {
      barplot(cluster_reference$output[started[i]:finished[i]],
              ylim=c(0,max))
      mtext(side=3, paste(months[i]), line = 1)  
    }
    for(i in (ref1+1):ref2) { # num_of_plots should be even
      barplot(cluster_reference$output[started[i]:finished[i]],
              ylim=c(0,max))
      mtext(side=3, paste(months[i]), line = 1)  
    }
    for(i in (ref2+1):(ref2+ref1)) {
      barplot(cluster_reference$output[started[i]:finished[i]],
              ylim=c(0,max))
      mtext(side=3, paste(months[i]), line = 1)  
    }
    for(i in (ref1+ref2+1):num_of_plots) {
      barplot(cluster_reference$output[started[i]:finished[i]],
              ylim=c(0,max))
      mtext(side=3, paste(months[i]), line = 1)  
    }
  }
  mtext(side = 3, line=0.5,paste("Cluster ", clust_num,
                                 "  -  (k1 = ",k1_value,", k2 = ", k2_value,")", 
                                 sep=""), 
        cex=1.4, outer = T)
  mtext(side =3, line = 0.2, adj = 0,
        "Gympie NP", cex=1.2, outer = T)
  mtext(side =3, line = -32.5, adj = 0,
        "Woondum NP", cex=1.2, outer = T)
  mtext(side =2, line = 1, adj = 0,
        "            Average number of minutes in 2 hour period", 
        cex=1, outer = T)
  mtext(side =2, line = 1, adj = 1,
        "Average number of minutes in 2 hour period            ", 
        cex=1, outer = T)
  dev.off()
}

for(i in 1:k2_value){
  plot_2hour_files(i)  
}