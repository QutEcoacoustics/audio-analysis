# Title:  Plotting the Hybrid Cluster Results
# Author: Yvonne Phillips
# Date:  24 September 2016

# Use SHIFT, ALT, J to navigate to each of sections or
# functions
# 1.  ID3 separation plots
# 2.  Composite spectrogram images 
# 3.  2 hour plots
# 4.  Yearlong Dot Matrix plots - uses col_func.R 
#     sourced externally
# 5.  Clustering diel plots - uses col_func.R

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# 1. Plot ID3 separation values ------------------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

ID3_values <- read.csv("ID3_values.csv")
#col <- c("red", "blue", "green", "orange", 
#         "black", "purple","magenta")
Jcol <- rep("black",8)
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

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# 2.  Plot Composite Images of each cluster -------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
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
dev.off()
for(j in 1:k2_value) {
  print(paste("starting", j, Sys.time(), sep = " "))
  cluster_image(j) # call function k2 times
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# 3.  Plotting 2hour_plot - time of day per month -------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
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
  if(num_of_plots <= 14){
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

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# 4. Plot yearlong dot matrix plots  -----------------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Description:  Plots the dominant clusters in each two 
#               hour period across days in two ways
# 1. As individual clusters 
# 2. As agglomerated clusters ie grouping all clusters 
# classified as the same feature together

# remove all objects in the global environment
rm(list = ls())

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

# Assign 2 hour periods (120 minutes) 
cluster_list$period <- 0
periods <- seq(0, 1440, 120)
for(i in 1:(length(periods)-1)) {
  a <- which(cluster_list$minute_reference > periods[i]-1 
             & cluster_list$minute_reference < periods[i+1])
  if(i < 10) {cluster_list$period[a] <- paste("0", i, sep = "")}
  if(i >= 10) {cluster_list$period[a] <- paste(i, sep = "")}
}

# Create column containing specific site, date and period info
#cluster_list$site_yrMth_per <- paste(cluster_list$site, 
#                                     substr(cluster_list$dates,1,6),
#                                     cluster_list$period, 
#                                     sep = "_")
# Create column containing specific site, date and period info
cluster_list$site_Date_per <- paste(cluster_list$site, 
                                     substr(cluster_list$dates,1,8),
                                     cluster_list$period, 
                                     sep = "_")

# Aggregate all clusters from the same feature together
# features include 

# Check for col_func in globalEnv otherwise source function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# Generate colour list using col_func
# Note col_func requires a csv file containing customed
# colour information for each cluster 
col_func(cluster_colours)

cluster_list$descpt <- 0
list <- unique(cluster_colours$Feature)
list <- as.character(list[1:(length(list)-1)])

# Add a description column
for(i in 1:(length(unique(cluster_list$cluster_list))-1)) {
  # Obtain a list of clusters corresponding to the first feature
  # in the list
  a <- which(as.character(cluster_colours$Feature)==list[i])
  for(j in 1:length(a)) {
    a1 <- which(cluster_list$cluster_list==a[j])  
    cluster_list[a1,7] <- as.character(list[i])
  }
}
# complete the description column by adding NAs
a <- which(cluster_list$descpt=="0")
cluster_list$descpt[a] <- "NA"

# Find the number of each class 
num_class <- dataFrame(colClasses=c(a="list", 
                                    b="integer", c="numeric",d="numeric", 
                                    e="integer", f="numeric",g="numeric", 
                                    h="numeric"), 
                       nrow=(length(list)+1))
colnames(num_class) <- c("class", "Freq_Gym","Percent_class","Pcent_gym_tot",
                         "Freq_Woon","Percent_class","Pcent_woon_tot",
                         "Percent_total")

number_of_obs <- nrow(cluster_list)
for(i in 1:length(list)) {
  gym <- length(which(cluster_list$descpt[1:(number_of_obs/2)]==list[i]))
  woon <- length(which(cluster_list$descpt[(number_of_obs/2+1):number_of_obs]==list[i]))
  total <- gym + woon
  gym_per <- round(gym/total*100, 6)
  woon_per <- round(woon/total*100, 6)
  gym_per_all <- round(gym/number_of_obs*100, 6)
  woon_per_all <- round(woon/number_of_obs*100, 6)
  percent_total <- gym_per_all + woon_per_all
  num_class[i,] <- c(list[i], gym, gym_per, gym_per_all,
                     woon, woon_per, woon_per_all, percent_total)
}
rm(gym, woon, total, gym_per, woon_per, gym_per_all, woon_per_all)
gym_na <- length(which(cluster_list$descpt[1:(number_of_obs/2)]=="NA"))
woon_na <- length(which(cluster_list$descpt[(number_of_obs/2+1):number_of_obs]=="NA"))
total <- gym_na + woon_na
gym_na_percent <- round((gym_na/total *100),6)
woon_na_percent <- round((woon_na/total *100),6)
gym_na_total <- round(gym_na/number_of_obs*100, 6)
woon_na_total <- round(woon_na/number_of_obs*100, 6)
total_percent <- round(((gym_na_percent + woon_na_percent)/number_of_obs), 10)
num_class[(length(list)+1),] <- c("NA", gym_na, gym_na_percent,
                                  gym_na_total, woon_na, woon_na_percent,
                                  woon_na_total, total_percent)
rm(gym_na,gym_na_percent, gym_na_total,
   woon_na, woon_na_percent, woon_na_total, total_percent)

g <- grep("QUIET", num_class$class)
total_quiet <- sum(as.numeric(num_class$Percent_total[g]))
g <- grep("RAIN", num_class$class)
total_rain <- sum(as.numeric(num_class$Percent_total[g]))
g <- grep("WIND", num_class$class)
total_wind <- sum(as.numeric(num_class$Percent_total[g]))
g <- grep("BIRDS", num_class$class)
h <- grep("MORNING CHORUS", num_class$class)
total_birds <- sum(as.numeric(num_class$Percent_total[c(g,h)]))

g <- grep("INSECTS", num_class$class)
h <- grep("QUIET INSECTS", num_class$class)
total_insects <- sum(as.numeric(num_class$Percent_total[g])) -
  sum(as.numeric(num_class$Percent_total[h]))
g <- grep("CICADAS", num_class$class)
total_cicadas <- sum(as.numeric(num_class$Percent_total[g]))


# not needed for agg
#cluster_list$R <- 0
#cluster_list$G <- 0
#cluster_list$B <- 0
#for(i in 1:length(list)) {
#  a <- which(cluster_colours$Feature==list[i])
#  red <- cluster_colours$R[a[1]]
#  green <- cluster_colours$G[a[1]]
#  blue <- cluster_colours$B[a[1]]
#  a <- which(cluster_list$descpt==list[i])
#  cluster_list$R[a] <- red
#  cluster_list$G[a] <- green
#  cluster_list$B[a] <- blue
#}

# Generate a list of dominant clusters in each 2 hour period from the 
# agglomerated cluster descriptions
a <- table(cluster_list$descpt, cluster_list$site_Date_per)
a <- data.frame(a)
ref1 <- 1
ref2 <- (length(list)+1)
selected_rows <- NULL
i <- 0
for(i in 1:(nrow(a)/(length(list)+1))) {
  b <- which(a$Freq[ref1:ref2]==max(a$Freq[ref1:ref2]))
  
  if(length(b)==1) {
    print(b)
    b <- b + ref1 - 1
    selected_rows <- c(selected_rows, b)
  }
  # Break ties,  Take the one that is greatest over two periods
  if(length(b) >= 2) {
    print(b)
    b <- b + ref1 - 1 
    b <- b[1]
    selected_rows <- c(selected_rows, b)
  }
  ref1 <- ref1 + length(list) + 1
  ref2 <- ref2 + (length(list)+1)
}
dominant_agglomerated_clusters <- a[selected_rows,]
dominant_agglomerated_clusters <- data.frame(dominant_agglomerated_clusters)

feature_colours <- read.csv("data/datasets/Feature_colours.csv")

# Separate cluster_list into sites
nrow_dom_cluster <- nrow(dominant_agglomerated_clusters)
dom_agg_cl_gym <- dominant_agglomerated_clusters[1:(nrow_dom_cluster/2),]
dom_agg_cl_woon <- dominant_agglomerated_clusters[(nrow_dom_cluster/2 + 1):
                                                    nrow_dom_cluster,]
colnames(dom_agg_cl_gym) <- c("feature", "date", "frequency")
colnames(dom_agg_cl_woon) <- c("feature", "date", "frequency")

# Using dom_agg_cl_gym and feature_colours
png("plots/dom_agg_gym_test.png", 
    width = 5000, height = 5000)
par(mar=c(2, 2.5, 2, 2.5))
# Plot an empty plot with no axes or frame
plot(c(0, nrow(dom_agg_cl_gym)), 
     c(nrow(dom_agg_cl_gym), 1), 
     type = "n", axes=FALSE, 
     frame.plot=FALSE,
     xlab="", ylab="")
for(i in 1:length(feature_colours$Feature)) {
  feature <- as.character(feature_colours$Feature[i])
  a <- which(dom_agg_cl_gym$feature==feature)
  for(j in a) {
    for(k in a) {
      ref <- i
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
      #polygon(c(k,k,k+1,k+1), c(j,(j-1),(j-1),j),
      polygon(c(k-1,k-1,k,k), c((j-1),j, j,(j-1)),
              col=col_code, border = NA)
    }
  }
}
# Add axes labels (which in this case will be dates)
at <- seq(12, 4776, by = 12)
#axis(1, line = -5.5, at = at, #line=NA,
#     labels = c("2","4","6","8","10","12","14","16","18","20",
#                "22", "24"), cex.axis=2.1, outer = TRUE,
#     las=T, pos = NA)
#at <- seq(0, 1440, by = 10)
#axis(1, line = -5.5, at = at, tick = TRUE,
#     labels = FALSE, outer=TRUE)
abline (v=seq(0, 4776, 12), h=seq(0,4776,12), lty=2, lwd=0.02, xpd=FALSE)
#at <- seq(0, 1440, by = 120)
#axis(2, line = 0.01, at = at, 
#     labels = c("0","2","4","6","8","10",
#                "12","14","16","18","20",
#                "22", "24"), cex.axis=2.1, las=T, pos=NA)
#at <- seq(0, 1440, by = 10)
#axis(2, line = 0.01, at = at, tick = TRUE,
#     labels = FALSE, pos=NA)
dev.off()

# Using dom_agg_cl_gym and feature_colours
png("plots/dom_agg_woon_test.png", 
    width = 5000, height = 5000)
par(mar=c(2, 2.5, 2, 2.5))
# Plot an empty plot with no axes or frame
plot(c(0, nrow(dom_agg_cl_woon)), 
     c(nrow(dom_agg_cl_woon), 1), 
     type = "n", axes=FALSE, 
     frame.plot=FALSE,
     xlab="", ylab="")
for(i in 1:length(feature_colours$Feature)) {
  feature <- as.character(feature_colours$Feature[i])
  a <- which(dom_agg_cl_woon$feature==feature)
  for(j in a) {
    for(k in a) {
      ref <- i
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
      #polygon(c(k,k,k+1,k+1), c(j,(j-1),(j-1),j),
      polygon(c(k-1,k-1,k,k), c((j-1),j, j,(j-1)),
              col=col_code, border = NA)
    }
  }
}
# Add axes labels (which in this case will be dates)
at <- seq(12, 4776, by = 12)
#axis(1, line = -5.5, at = at, #line=NA,
#     labels = c("2","4","6","8","10","12","14","16","18","20",
#                "22", "24"), cex.axis=2.1, outer = TRUE,
#     las=T, pos = NA)
#at <- seq(0, 1440, by = 10)
#axis(1, line = -5.5, at = at, tick = TRUE,
#     labels = FALSE, outer=TRUE)
abline (v=seq(0, 4776, 12), h=seq(0,4776,12), lty=2, lwd=0.02, xpd=FALSE)
#at <- seq(0, 1440, by = 120)
#axis(2, line = 0.01, at = at, 
#     labels = c("0","2","4","6","8","10",
#                "12","14","16","18","20",
#                "22", "24"), cex.axis=2.1, las=T, pos=NA)
#at <- seq(0, 1440, by = 10)
#axis(2, line = 0.01, at = at, tick = TRUE,
#     labels = FALSE, pos=NA)
dev.off()

# Separate the site lists
cluster_list_Gympie <- cluster_list[1:(nrow(cluster_list)/2),]
cluster_list_Woondum <- cluster_list[(nrow(cluster_list)/2+1):nrow(cluster_list),]

a_Gym <- table(cluster_list_Gympie$cluster_list, cluster_list_Gympie$site_yrMth_per)
a_Gym <- as.data.frame(a_Gym)

a_Woon <- table(cluster_list_Woondum$cluster_list, cluster_list_Woondum$site_yrMth_per)
a_Woon <- as.data.frame(a_Woon)

# replace the NAs with 1000 to track these minutes
#NA_ref <- which(is.na(cluster_list$cluster_list)==TRUE)
#cluster_list$cluster_list[NA_ref] <- 1000
#NA_ref <- which(is.na(cluster_list_Gympie$cluster_list)==TRUE)
#cluster_list_Gympie$cluster_list[NA_ref] <- 1000
#NA_ref <- which(is.na(cluster_list_Woondum$cluster_list)==TRUE)
#cluster_list_Woondum$cluster_list[NA_ref] <- 1000

# Create a csv file containing the numbers of each cluster at each site 
summary <- NULL
gym_tab <- table(cluster_list_Gympie$cluster_list)
max_height_gym <- max(unname(gym_tab))
barplot(gym_tab)

woon_tab <- table(cluster_list_Woondum$cluster_list)
max_height_woon <- max(unname(woon_tab))
barplot(woon_tab)

# Plot the number of each cluster plots
png(paste("data/2hour_plots_", k1_value, "_",
    k2_value, "/cluster_size_Gympie_and_Woondum.png",
    sep = ""), width = 2000, height = 1000)
par(mfrow=c(2,1), mar=c(3,3,2,1))
barplot(gym_tab, ylim = c(0, max(max_height_gym, max_height_woon))) 
title(main = paste("Cluster Size Gympie ", 
                    k1_value, "_",
                    k2_value, sep=""))
barplot(woon_tab) 
title(main = paste("Cluster Size Woondum ", 
                   k1_value, "_",
                   k2_value, sep=""))
dev.off()

percent_gym <- NULL
percent_woon <- NULL
for(i in 1:length(gym_tab)) {
  perc_gym <- round(gym_tab[i]/(gym_tab[i] + woon_tab[i]), 3)*100
  percent_gym <- c(percent_gym, perc_gym)
  perc_woon <- round(woon_tab[i]/(gym_tab[i] + woon_tab[i]), 3)*100
  percent_woon <- c(percent_woon, perc_woon)
}
summary <- cbind(gym_tab, percent_gym, 
                 woon_tab, percent_woon)

name_rows <- row.names(summary)
if(name_rows[length(name_rows)]==1000) {
  name_rows[length(name_rows)] <- "NA"
}
row.names(summary) <- name_rows

summary <- cbind(name_rows, summary)
summary <- round(summary, 0)
colnames(summary) <- c("clusters", "gympie", "gym_percent",
                       "woondum", "woon_percent")
# write csv file containing summary
write.csv(summary, paste("data/2hour_plots_", k1_value, "_",
                         k2_value,"/Summary_",k1_value,"_",
                         k2_value,".csv", sep = ""), row.names = F)
# dominant clusters using all 60 clusters
dominant_clusters <- function(cluster_list) {
  # Create a table containing variables and frequencies
  a <- table(cluster_list$cluster_list, cluster_list$site_Date_per)
  a <- as.data.frame(a)
  
  reference <- NULL
  seq1 <- seq(1, nrow(a), (k2_value+1))
  seq1 <- c(seq1, nrow(a))
  for(i in 1:(nrow(a)/(k2_value+1)))  {
    ref <- which(a[seq1[i]:(seq1[i+1]-1),3]==max(a[seq1[i]:(seq1[i+1]-1),3]))
    reference <- c(reference, ref)
  }
  # produce a dominant cluster list, that is a list of the dominant cluster 
  # for each 2 hour period for each month for the two sites
  dom_clust_list <- a[reference, 1]
  
  # Generate a list of occurances of each cluster in each period
  # on each day
  a <- table(cluster_list$cluster_list, cluster_list$period, cluster_list$dates)
  a <- as.data.frame(a)
  colnames(a) <- c("cluster", "period", "date", "Freq")
  a <<- a
  
  ref2 <- NULL
  length <- NULL
  reference <- NULL
  seq1 <- seq(1, nrow(a), (k2_value+1))
  seq1 <- c(seq1, nrow(a))
  for(i in 1:(nrow(a)/(k2_value+1)))  {
    ref <- which(a[seq1[i]:(seq1[i+1]-1),4]==max(a[seq1[i]:(seq1[i+1]-1),4]))
    if (length(ref)==1) {
      print(ref)
      reference <- c(reference, ref)  
    }
    
    # Break ties,  Take the one that is greatest over two periods
    if(length(ref) >= 2) {
      ref2 <- c(ref2, i)
    
      if(reference[i-1] %in% ref==TRUE) {
        print(ref)
        reference <- c(reference, reference[i-1])
      }
      
      if(reference[i-1] %in% ref==FALSE) {
        print(ref)
        reference <- c(reference, ref[1])
      }
      
    }
  }
  
  # produce a dominant cluster list, that is a list of the dominant cluster 
  # for each 2 hour period for each month for the two sites
  dom_clust_list <<- a[reference, 1]
}

dominant_clusters(cluster_list)
a <- a
dom_clust_list <- dom_clust_list

dominant_clusters(cluster_list_Gympie)
a_Gym <- a
dom_clust_list_Gym <- dom_clust_list

dominant_clusters(cluster_list_Woondum)
a_Woon <- a
dom_clust_list_Woon <- dom_clust_list

png("data/dominant_clusters.png", width = 1000)
plot(dom_clust_list, main=paste("Dominant clusters_in 2 hour periods",
                                k1_value, "_", k2_value))
dev.off()

png("data/dominant_clusters_Gympie_and_Woondum.png", 
    width = 2000, height = 1000)
par(mfrow=c(2,1), mar=c(3,3,2,1))
plot(dom_clust_list_Gym, main=paste("Dominant clusters_Gympie_in 2 hour periods",
                                    k1_value, "_", k2_value))
plot(dom_clust_list_Woon, main=paste("Dominant clusters_Woondum_in 2 hour periods",
                                     k1_value, "_", k2_value))
dev.off()

png("data/dominant_clusters_Gympie.png", width = 1000)
plot(dom_clust_list_Gym, main=paste("Dominant clusters_Gympie_in 2 hour periods",
                                    k1_value, "_", k2_value))
dev.off()

png("data/dominant_clusters_Woondum.png", width = 1000)
plot(dom_clust_list_Woon, main=paste("Dominant clusters_Woondum_in 2 hour periods",
                                     k1_value, "_", k2_value))
dev.off()

############## dominant clusters using all agglomerated clusters
# Check for col_func in globalEnv otherwise source function
#if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# Generate colour list using col_func
# Note col_func requires a csv file containing customed
# colour information for each cluster 
#col_func(cluster_colours)

#cluster_list$descpt <- 0
list <- unique(cluster_colours$Feature)
list <- as.character(list[1:(length(list)-1)])
list <- c(list, "NA")
# sort list into alphabetical listing
alpha_order <- order(list)
list <- list[alpha_order]

dominant_clusters_agglom <- function(cluster_list) {
  # Create a table containing variables and frequencies
  a <- table(cluster_list$descpt, cluster_list$site_Date_per)
  a <- as.data.frame(a)
  a$R <- 0
  a$G <- 0
  a$B <- 0
  for(i in 1:length(list)) {
    b <- which(a$Var1==list[i])
    c <- which(cluster_list$descpt==list[i]) 
      red <- cluster_list$R[c[1]]
      green <- cluster_list$G[c[1]]
      blue <- cluster_list$B[c[1]]
    a$R[b] <- red
    a$G[b] <- green
    a$B[b] <- blue
  }
  
  reference <- NULL
  num_cluster <- length(unique(list))
  seq1 <- seq(1, nrow(a), num_cluster)
  seq1 <- c(seq1, nrow(a))
  
  for(i in 1:(nrow(a)/(num_cluster)))  {
    ref <- which(a[seq1[i]:(seq1[i+1]-1),3]==max(a[seq1[i]:(seq1[i+1]-1),3]))
    reference <- c(reference, ref)
  }
  # produce a dominant cluster list, that is a list of the dominant cluster 
  # for each 2 hour period for each month for the two sites
  dom_clust_list <- a[reference, 1]
  
  ref2 <- NULL
  length <- NULL
  reference <- NULL
  seq1 <- seq(1, nrow(a), num_cluster)
  seq1 <- c(seq1, nrow(a))
  for(i in 1:(nrow(a)/num_cluster))  {
    ref <- which(a[seq1[i]:(seq1[i+1]-1),4]==max(a[seq1[i]:(seq1[i+1]-1),4]))
    if (length(ref)==1) {
      print(ref)
      reference <- c(reference, ref)  
    }
    
    # Break ties,  Take the one that is greatest over two periods
    if(length(ref) >= 2) {
      ref2 <- c(ref2, i)
      if(i > 1 & reference[i-1] %in% ref==TRUE) {
        print(ref)
        reference <- c(reference, reference[i-1])
      }
      
      if(reference[i-1] %in% ref==FALSE) {
        print(ref)
        reference <- c(reference, ref[1])
      }
      
    }
  }
  
  # produce a dominant cluster list, that is a list of the dominant cluster 
  # for each 2 hour period for each month for the two sites
  dom_clust_list <<- a[reference,]
}

dominant_clusters_agglom(cluster_list)
a <- a
dom_clust_list <- dom_clust_list

dominant_clusters_agglom(cluster_list_Gympie)
a_Gym <- a
dom_clust_list_Gym <- dom_clust_list

dominant_clusters_agglom(cluster_list_Woondum)
a_Woon <- a
dom_clust_list_Woon <- dom_clust_list

png("data/dominant_clusters_agglomeration.png", 
    width = 2500)
plot(dom_clust_list, main=paste("Dominant clusters_in 2 hour periods",
                                k1_value, "_", k2_value),
     ylab="Frequency")
dev.off()

png("data/dominant_clusters_Gympie_and_Woondum_agglomeration.png", 
    width = 2500, height = 1000)
par(mfrow=c(2,1), mar=c(3,3,2,1))
max_height <- max(c(max(table(dom_clust_list_Gym),
                        max(table(dom_clust_list_Woon)))))
plot(dom_clust_list_Gym, main=paste("Dominant clusters_Gympie_in 2 hour periods",
                                    k1_value, "_", k2_value),
     ylim = c(0,max_height), ylab="Frequency")
plot(dom_clust_list_Woon, main=paste("Dominant clusters_Woondum_in 2 hour periods",
                                     k1_value, "_", k2_value),
     ylim = c(0,max_height), ylab="Frequency")
dev.off()

png("data/dominant_clusters_Gympie_agglomeration.png", 
    width = 2000)
plot(dom_clust_list_Gym, main=paste("Dominant clusters_Gympie_in 2 hour periods",
                                    k1_value, "_", k2_value),
     ylab="Frequency")
dev.off()

png("data/dominant_clusters_Woondum_agglomeration.png", 
    width = 2000)
plot(dom_clust_list_Woon, main=paste("Dominant clusters_Woondum_in 2 hour periods",
                                     k1_value, "_", k2_value),
     ylab="Frequency")
dev.off()

# dot matrix plots 
library(graphics)
library(fields)
data <- dom_clust_list_Gym
num_periods <- length(dom_clust_list_Gym)  # number of 2 hour segments

num_clus <- length(unique(dom_clust_list))

t <- rep(0,(num_periods*num_periods))
periods <- 12
for (i in 1:num_periods) { 
  for (j in 1:num_periods) {
    if(data[i]==data[j]) { 
      t[((i-1)*periods)+j] <- data[i]
    }
  }
}

# Check for col_func in globalEnv otherwise source function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# Generate colour list using col_func
# Note col_func requires a csv file containing customed
# colour information for each cluster 
col_func(cluster_colours)

# Name the colours
cols <- structure(cols, names=c(1:(length(cols)-1),"1000"))
# Add one colour for image background  
cols <- c('0' = "#F7F7F7",cols)

t <- as.numeric(t)

asratio <- 1
png(paste("colour_dot_plot_test.png"), width=2000, 
    height=2000, units = "px")
score <- matrix(data = t, nrow=min, ncol=min, byrow = T)
image.plot(1:nrow(score), 1:ncol(score), 
           score, col=cols[1:(num_clus+1)], axes=FALSE,
           ann=F, xaxt="n", legend.shrink = 0.9,
           yaxt="n", asp=1, legend.cex=2, pty="s")

#at <- seq(120, 1440, by = 120)
#axis(1, line = -5.5, at = at, #line=NA,
#     labels = c("2","4","6","8","10","12","14","16","18","20",
#                "22", "24"), cex.axis=2.1, outer = TRUE,
#     las=T, pos = NA)
#at <- seq(0, 1440, by = 10)
#axis(1, line = -5.5, at = at, tick = TRUE,
#     labels = FALSE, outer=TRUE)
#abline (v=seq(12,min,12), h=seq(12,min,12), 
#        lty=2, lwd=0.02, xpd=FALSE)
#at <- seq(0, 1440, by = 120)
#axis(2, line = 0.01, at = at, 
#     labels = c("0","2","4","6","8","10",
#                "12","14","16","18","20",
#                "22", "24"), cex.axis=2.1, las=T, pos=NA)
#at <- seq(0, 1440, by = 10)
#axis(2, line = 0.01, at = at, tick = TRUE,
#     labels = FALSE, pos=NA)
#par(usr=c(xmin,xmax,ymin,ymax))
dev.off()

# an alternative plot for both sites ##
dev.off()
days <- floor(length(dom_clust_list)/12)
for (k in 1:2) {
  ref <- c(0, days*12)
  # generate a date sequence and locate the first of the month
  days <- length(coef_min_max[,1])/(2*1440)
  start <- as.POSIXct("2015-06-22")
  interval <- 1440
  end <- start + as.difftime(days, units="days")
  dates <- seq(from=start, by=interval*60, to=end)
  
  png(filename = paste("plots/final",site[k],"_", type,"_", index, ".png", sep = ""),
      width = 2000, height = 1000, units = "px")
  par(mar=c(2, 2.5, 2, 2.5))
  # Plot an empty plot with no axes or frame
  plot(c(0,1440), c(398,1), type = "n", axes=FALSE, 
       frame.plot=FALSE,
       xlab="", ylab="") #, asp = 398/1440)
  # Create the heading
  mtext(side=3, 
        paste(site[k]," ", format(dates[1], "%d %B %Y")," - ", 
              format(dates[length(dates)-1], "%d %B %Y"), sep=""), 
        cex=1.8)
  # Create the sub-heading
  mtext(side=3, line = -1.5, 
        paste(index," ", type, " indices pca coefficients", sep = ""),
        cex=1.4)
  
  # draw coloured polygons row by row
  ref <- ref[k]
  # set the rows starting at the top of the plot
  for(j in days:1) {
    # set the column starting on the left
    for(k in 1:1440) {
      ref <- ref + 1
      # draw a square for each minute in each day 
      # using the polygon function mapping the red, green
      # and blue channels to the normalised pca-coefficients
      polygon(c(k,k,k+1,k+1), c(j,(j-1),(j-1),j),
              col=rgb(coef_min_max_norm[ref,1],
                      coef_min_max_norm[ref,2],
                      coef_min_max_norm[ref,3]),
              border = NA)
    }
  }
  
  # draw horizontal lines
  first_of_month <- which(substr(dates, 9, 10)=="01")
  first_of_each_month <- days - first_of_month + 1
  for(i in 1:length(first_of_month)) {
    lines(c(1,1441), c(first_of_each_month[i], 
                       first_of_each_month[i]), 
          lwd=0.6, lty = 3)
  }
  # draw vertical lines
  at <- seq(0,1440, 120) + 1
  for(i in 1:length(at)) {
    lines(c(at[i], at[i]), c(1,days), lwd=0.5, lty=3)
  }
  # label the x axis
  axis(1, tick = FALSE, at = at, 
       labels = c("12 am","2 am","4 am",
                  "6 am","8 am","10 am",
                  "12","2 pm","4 pm","6 pm",
                  "8 pm","10 pm","12 pm"), 
       cex.axis=1.4, line = -2.6)
  # plot the left axes
  axis(side = 2, at = first_of_each_month, tick = FALSE, 
       labels=format(dates[first_of_month],"%b %Y"), 
       cex.axis=1.3, las=1, line = -5)
  axis(side = 2, at = c(days), tick = FALSE, 
       labels=format(dates[1],"%d %b %Y"), 
       cex.axis=1.3, las=1, line = -5)
  # plot the left axes
  axis(side = 4, at = first_of_each_month, tick = FALSE, 
       labels=format(dates[first_of_month],"%b %Y"), 
       cex.axis=1.3, las=1, line = -5)
  axis(side = 4, at = c(days), tick = FALSE, 
       labels=format(dates[1],"%d %b %Y"), 
       cex.axis=1.3, las=1, line = -5)
  
  at <- seq(0, 1440, 240)
  # add the indices names to the plot
  indices <- colnames(indices_all)
  j <- days - (days - (length(indices)*8))/2
  for(i in 1:length(indices)) {
    text(65, j, indices[i], cex = 1)
    j <- j - 8 
  }
  # draw yellow dotted line to show civil-dawn
  for(i in length(civ_dawn):1) {
    lines(c(civ_dawn+1), c(length(civ_dawn):1),  
          lwd=0.4, lty="16", col="yellow")
  }
  # draw yellow line to show civil-dusk
  for(i in length(civ_dawn):1) {
    lines(c(civ_dusk+1), c(length(civ_dusk):1),  
          lwd=0.4, lty="16", col="yellow")
  }
  # draw yellow line to show sunrise
  for(i in length(sunrise):1) {
    lines(c(sunrise+1), c(length(sunrise):1),  
          lwd=0.4, lty="16", col="yellow")
  }
  # draw yellow line to show sunset
  for(i in length(sunset):1) {
    lines(c(sunset+1), c(length(sunset):1),  
          lwd=0.4, lty="16", col="yellow")
  }
  dev.off()
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# 5. Clustering Diel Plots -----------------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
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
civil_dawn <- read.csv("data/Sunrise_Sunset_Solar Noon_protected.csv", header=T)
a <- which(civil_dawn$Date==paste(substr(start, 9,20), substr(start, 6,7),
                                  substr(start, 1,4), sep = "/"))
reference <- a:(a+days-1)
civil_dawn_times <- civil_dawn$Civil_Sunrise[reference]
civil_dusk_times <- civil_dawn$Civil_Sunset[reference]
sunrise_times <- civil_dawn$Sunrise[reference]
sunset_times <- civil_dawn$Sunset[reference]

civ_dawn <- NULL
for(i in 1:length(civil_dawn_times)) {
  hour <- as.numeric(substr(civil_dawn_times[i], 1,1))
  min <- as.numeric(substr(civil_dawn_times[i], 3,4))
  minute <- hour*60 + min
  civ_dawn <- c(civ_dawn, minute)
}

civ_dusk <- NULL
for(i in 1:length(civil_dusk_times)) {
  hour <- as.numeric(substr(civil_dusk_times[i], 1,1)) + 12
  min <- as.numeric(substr(civil_dusk_times[i], 3,4))
  minute <- hour*60 + min
  civ_dusk <- c(civ_dusk, minute)
}

sunrise <- NULL
for(i in 1:length(sunrise_times)) {
  hour <- as.numeric(substr(sunrise_times[i], 1,1))
  min <- as.numeric(substr(sunrise_times[i], 3,4))
  minute <- hour*60 + min
  sunrise <- c(sunrise, minute)
}

sunset <- NULL
for(i in 1:length(sunset_times)) {
  hour <- as.numeric(substr(sunset_times[i], 1,1)) + 12
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
index <- "SELECTED_Final" # or "ALL"
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
col_func(cluster_colours)

# Generate and save the cluster diel plots
for (k in 1:2) {
  ref <- c(0, days*1440)
  # generate a date sequence and locate the first of the month
  days <- nrow(cluster_list)/(2*1440)
  start <- as.POSIXct(paste(start_date))
  interval <- 1440
  end <- start + as.difftime(days, units="days")
  dates <- seq(from=start, by=interval*60, to=end)
  
  png(filename = paste("plots/Cluster_plot_",site[k],"_", type,"_", index, ".png", sep = ""),
      width = 2000, height = 1000, units = "px")
  par(mar=c(2, 2.5, 2, 2.5))
  # Plot an empty plot with no axes or frame
  plot(c(0,1440), c(398,1), type = "n", axes=FALSE, 
       frame.plot=FALSE,
       xlab="", ylab="") #, asp = 398/1440)
  # Create the heading
  mtext(side=3,
        paste("Cluster plot - ", site[k]," ", format(dates[1], "%d %B %Y")," - ", 
              format(dates[length(dates)-1], "%d %B %Y"), 
              sep=""), 
        cex=1.8)
  # Create the sub-heading
  mtext(side=3, line = -1.5, 
        paste(type, " Indices: ", 
              paste(indices_names, collapse = ", "), 
              sep = ""),
        cex=1.4)
  
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
          lwd=0.6, lty = 3)
  }
  # draw vertical lines
  at <- seq(0,1440, 120) + 1
  for(i in 1:length(at)) {
    lines(c(at[i], at[i]), c(1,days), lwd=0.5, lty=3)
  }
  # label the x axis
  axis(1, tick = FALSE, at = at, 
       labels = c("12 am","2 am","4 am",
                  "6 am","8 am","10 am",
                  "12","2 pm","4 pm","6 pm",
                  "8 pm","10 pm","12 pm"), 
       cex.axis=1.4, line = -2.6)
  # plot the left axes
  axis(side = 2, at = first_of_each_month, tick = FALSE, 
       labels=format(dates[first_of_month],"%b %Y"), 
       cex.axis=1.3, las=1, line = -5)
  axis(side = 2, at = c(days), tick = FALSE, 
       labels=format(dates[1],"%d %b %Y"), 
       cex.axis=1.3, las=1, line = -5)
  # plot the left axes
  axis(side = 4, at = first_of_each_month, tick = FALSE, 
       labels=format(dates[first_of_month],"%b %Y"), 
       cex.axis=1.3, las=1, line = -5)
  axis(side = 4, at = c(days), tick = FALSE, 
       labels=format(dates[1],"%d %b %Y"), 
       cex.axis=1.3, las=1, line = -5)
  
  at <- seq(0, 1440, 240)
  # add the indices names to the plot
  indices <- colnames(indices_all)
  j <- days - (days - (length(indices)*8))/2
  for(i in 1:length(indices)) {
    text(65, j, indices[i], cex = 1)
    j <- j - 8 
  }
  # draw yellow dotted line to show civil-dawn
  for(i in length(civ_dawn):1) {
    lines(c(civ_dawn+1), c(length(civ_dawn):1),  
          lwd=0.4, lty="16", col="yellow")
  }
  # draw yellow line to show civil-dusk
  for(i in length(civ_dawn):1) {
    lines(c(civ_dusk+1), c(length(civ_dusk):1),  
          lwd=0.4, lty="16", col="yellow")
  }
  # draw yellow line to show sunrise
  for(i in length(sunrise):1) {
    lines(c(sunrise+1), c(length(sunrise):1),  
          lwd=0.4, lty="16", col="yellow")
  }
  # draw yellow line to show sunset
  for(i in length(sunset):1) {
    lines(c(sunset+1), c(length(sunset):1),  
          lwd=0.4, lty="16", col="yellow")
  }
  dev.off()
}