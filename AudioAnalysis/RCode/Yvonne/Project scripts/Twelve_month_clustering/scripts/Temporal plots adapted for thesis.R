#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 7 Two hour plots  Thirteen month dataset --------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
rm(list = ls())
#setwd("C:/Work/Projects/Twelve_month_clustering/Saving_dataset")

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/chosen_cluster_list_",
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
insects_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
birds_col <- "#009E73"
cicadas_col <- "#E69F00"
quiet_col <- "#999999"
planes_col <- "#CC79A7"

numbers <- c(37,44,1,41,49,42)
for(k in 1:length(numbers)) {
  clust_num <- numbers[k]
  y <- which(a$Var1==clust_num)
  cluster_reference <- a[y,3]
  cluster_reference <- cbind(cluster_reference, 
                             days_per_period,
                             na_reference)
  cluster_reference <- as.data.frame(cluster_reference)
  cluster_reference$output <- cluster_reference$cluster_reference/
    (cluster_reference$days_per_period - (cluster_reference$na_reference/120))
  cluster_reference$output <- round(cluster_reference$output,2)
  
  if(clust_num==41){
    label <- ("Cluster 41 - Very Quiet")  
  }
  if(clust_num==42){
    label <- ("Cluster 42 - Strong Wind")  
  }
  if(clust_num==37){
    label <- ("Cluster 37 - Morning Chorus Birds")  
  }
  if(clust_num==37){
    label <- ("Cluster 37 - Morning Chorus Birds")  
  }
  if(clust_num==1){
    label <- ("Cluster 1 - Orthopterans")  
  }
  if(clust_num==49){
    label <- ("Cluster 49 - Planes")  
  }
  if(clust_num==44){
    label <- ("Cluster 44 - Cicadas")  
  }
  
  
  tiff(paste("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset/Temporal_plot_",clust_num,".tiff",sep=""),
       width = 1430, height = 1350, units = 'px', res = 300)
  cex.axis <- 0.6
  par(mfrow=c(2,14), mar=c(1.5,0,2.6,0.2), oma=c(0.6,2,0,0), mgp=c(1,0.3,0))
  opar <- par(lwd = 0.7)
  for(j in seq(1,length(cluster_reference$output),12)) {
    if(j %in% c((0*12+1),(14*12+1))) {
      month <- "Jun15"
    }
    if(j %in% c((1*12+1),(15*12+1))) {
      month <- "Jul15"
    }
    if(j %in% c((2*12+1),(16*12+1))) {
      month <- "Aug15"
    }
    if(j %in% c((3*12+1),(17*12+1))) {
      month <- "Sep15"
    }
    if(j %in% c((4*12+1),(18*12+1))) {
      month <- "Oct15"
    }
    if(j %in% c((5*12+1),(19*12+1))) {
      month <- "Nov15"
    }
    if(j %in% c((6*12+1),(20*12+1))) {
      month <- "Dec15"
    }
    if(j %in% c((7*12+1),(21*12+1))) {
      month <- "Jan16"
    }
    if(j %in% c((8*12+1),(22*12+1))) {
      month <- "Feb16"
    }
    if(j %in% c((9*12+1),(23*12+1))) {
      month <- "Mar16"
    }
    if(j %in% c((10*12+1),(24*12+1))) {
      month <- "Apr16"
    }
    if(j %in% c((11*12+1),(25*12+1))) {
      month <- "May16"
    }
    if(j %in% c((12*12+1),(26*12+1))) {
      month <- "Jun16"
    }
    if(j %in% c((13*12+1),(27*12+1))) {
      month <- "Jul16"
    }
    b <- barplot(cluster_reference$output[j:(j+11)], 
                 las=1, mgp=c(0,0.5,0), ylim=c(0,max(cluster_reference$output)+0.1),
                 yaxt="n", lwd=0.02, col="grey80")
    mtext(side=3, month, cex=0.5)
    abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
    axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
         tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
    axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
    axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
         tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
    axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
         col = NA, col.ticks = 1)
    axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
         col = NA, col.ticks = 1) 
    axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
         tck=-0.07, cex.axis=cex.axis, padj=-1,
         col = NA, col.ticks = 1)
    axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
         col = NA, col.ticks = 1)
    axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
         col = NA, col.ticks = 1)
    segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
    clusters <- c(1:48,50:60)
    if(j %in% c(1,(14*12)+1)&clust_num %in% clusters) {
      axis(side=2, tick = T, las=1, pos = (b[1]-0.5), tck = -0.06,
           lwd=0.02, cex=0.7, ylim=range(pretty(c(0, cluster_reference$output[j:(j+11)]))))
    }
    if(j %in% c(1,(14*12)+1)& clust_num %in% 49) {
      axis(side=2, tick = T, las=1, pos = (b[1]-0.5), tck = -0.06,
           lwd=0.02, cex=0.7, at=c(1,2))
    }
    if(j==(7*12+1)) {
      mtext(side=1, label, outer = T, 
            line=-33.4, cex=0.8)
      mtext(side=1, "Gympie National Park  ", outer = T, 
            line=-32.6, cex=0.6)
    }
    if(j==(22*12+1)) {
      mtext(side=1, label, outer = T, 
            line=-16.8, cex=0.8)
      mtext(side=1, "Wooondum National Park   ", outer = T, 
            line=-16, cex=0.6)
    }
  }
  mtext(side = 2, line = 1.2, outer = T, cex = 0.7,
        "Average number of cluster minutes in 2 hour period")
  mtext(side = 1, line = -0.5, outer = T, cex = 0.7,
        "Time (24 hours)")
  dev.off()
}


#abline(h=seq(2,12,2), lty=c(2,2,2,2,1), lwd=0.001)
for(i in seq(2, 400, 2)) {
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=i, y1=i, lwd=0.01, lty=2)
}
axis(side=2, las=1, pos = b[1]-0.5)
b <-barplot(cluster_reference$output[13:24], 
            las=1, mgp=c(0,0.5,0), ylim=c(0,max(cluster_reference$output)),
            yaxt="n")
for(i in seq(2, 400, 2)) {
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=i, y1=i, lwd=0.01, lty=2)
}
abline(v=c((b[1]-0.5), b[length(b)]+0.5))





plot_funct <- function(clust_num, colour, site) {
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
  if(site=="site1") {
    for(i in 1:14) { # num_of_plots should be even
      # The y-axis labels are plotted for the 1st and 15th plots
      num1 <- c(1,15)
      if(i %in% num1) {
        mp <- barplot(cluster_reference$output[started[i]:finished[i]],
                      ylim=c(0, max), col = colour, xlab = "", las=1,
                      mgp = c(1, 0.4, 0), tck = - 0.05)
        spacing <- mp[2] - mp[1]
        # adjust the midpoints (mp)
        mp <- mp - spacing/2
        mp <- c(mp, mp[length(mp)]+ spacing)
        at <- mp    #seq.int(0.4, 14, length.out = 12)
        Axis(side = 1, labels = FALSE, 
             at = at, cex = 0.4, 
             tck = -0.05, mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE, at = c(at[1], at[7], at[13]),
             cex = 0.4, tck = -0.14, mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE, at = c(at[4], at[10]),
             cex = 0.4, tck = -0.09, mgp = c(1,0.4,0))
      }
      num2 <- c(2:14,16:28) 
      if(i %in% num2) {
        mp <- barplot(cluster_reference$output[started[i]:finished[i]],
                      ylim=c(0,max), col = colour, xlab = "", axes=FALSE,
                      mgp = c(1,0.5,0), tck = - 0.05)
        spacing <- mp[2] - mp[1]
        # adjust the midpoints (mp)
        mp <- mp - spacing/2
        mp <- c(mp, mp[length(mp)]+ spacing)
        at <- mp    #seq.int(0.4, 14, length.out = 12)
        Axis(side = 2, labels=FALSE, tck = -0.05, mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE, tck = -0.05,
             at = at, mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE, tck = -0.14,
             at = c(at[1], at[7], at[13]), mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE, at = c(at[4], at[10]),
             cex = 0.4, tck = -0.09, mgp = c(1,0.4,0))
      }
      mtext(side = 3, paste(months[i]), line = 0.3, cex = 0.7)
      mtext(side = 1, text = c(as.character(seq(0,24,12))), 
            at = c(at[1]+0.7, at[7], at[13]-0.7), line=-0.1, cex = 0.5)
    }  
  }
  if(site=="site2") {
    for(i in 15:28) { # num_of_plots should be even
      # The y-axis labels are plotted for the 1st and 15th plots
      num1 <- c(1,15)
      if(i %in% num1) {
        mp <- barplot(cluster_reference$output[started[i]:finished[i]],
                      ylim=c(0, max), col = colour, xlab = "", las=1,
                      mgp = c(1, 0.4, 0), tck = - 0.05)
        spacing <- mp[2] - mp[1]
        # adjust the midpoints (mp)
        mp <- mp - spacing/2
        mp <- c(mp, mp[length(mp)]+ spacing)
        at <- mp    #seq.int(0.4, 14, length.out = 12)
        Axis(side = 1, labels = FALSE,
             at = at, cex = 0.4, tck = -0.05, mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE,
             at = c(at[1], at[7], at[13]), cex = 0.4, 
             tck = -0.14, mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE, at = c(at[4], at[10]),
             cex = 0.4, tck = -0.09, mgp = c(1,0.4,0))
      }
      num2 <- c(2:14,16:28) 
      if(i %in% num2) {
        mp <- barplot(cluster_reference$output[started[i]:finished[i]],
                      ylim=c(0,max), col = colour, xlab = "", axes=FALSE,
                      mgp = c(1,0.5,0), tck = - 0.05)
        spacing <- mp[2] - mp[1]
        # adjust the midpoints (mp)
        mp <- mp - spacing/2
        mp <- c(mp, mp[length(mp)]+ spacing)
        at <- mp    #seq.int(0.4, 14, length.out = 12)
        Axis(side = 2, labels=FALSE, tck = -0.05, mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE, tck = -0.05,
             at = mp, mgp = c(1, 0.4, 0))
        Axis(side = 1, labels = FALSE, tck = -0.14,
             at = c(at[1], at[7], at[13]), mgp = c(1,0.4,0))
        Axis(side = 1, labels = FALSE, at = c(at[4], at[10]),
             cex = 0.4, tck = -0.09, mgp = c(1,0.4,0))
      }
      mtext(side = 3, paste(months[i]), line = 0.3, cex = 0.7)
      mtext(side = 1, text = c(as.character(seq(0,24,12))), 
            at = c(at[1]+0.7, at[7], at[13]-0.7), line=-0.1, cex = 0.5)
    }
  }
}
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# plot 7 ------------------------------
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
tiff("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\Figures for plos article/Fig7.tiff",
     width = 2025, height = 1350, units = 'px', res = 300)
#par(mar=c(2, 2.5, 2, 0.4), mfrow = c(4,1),
#    cex = 1, cex.axis = 1, cex.main = 2.4)
par(mfrow=c(4, 14), 
    mar=c(1, 0, 2, 0.1), oma=c(0.1, 2.1, 0, 0), xpd = NA,
    cex = 0.9, cex.axis = 0.54, cex.main = 0.9)

# Start insect image
clust_num <- 29
colour <- insects_col
plot_funct(clust_num, colour, "site2")
mtext(side = 3, line = 1, "a. ORTHOPTERA - Cluster 29                                                                                                                                 ", cex=1.1)

# Start Bird image
clust_num <- 37
colour <- birds_col
plot_funct(clust_num, colour, "site2")
mtext(side = 3, line = 1, "b. BIRDS - Cluster 37                                                                                                                                 ", cex=1.1)
mtext(side = 2, line = 1.3, outer = T, cex = 0.8,
      "Average number of cluster minutes in 2 hour period each month")

# Start cicada image
clust_num <- 48
colour <- cicadas_col
plot_funct(clust_num, colour, "site2")
mtext(side = 3, line = 1, "c. CICADAS - Cluster 48                                                                                                                                 ", cex=1.1)

# Start quiet image
cluster <- 13
colour <- quiet_col
plot_funct(cluster, colour, "site2")
mtext(side = 3, line = 1, "d. QUIET - Cluster 13                                                                                                                                 ", cex=1.1)

dev.off()

# Plots for Supplementary S3 
# define cluster classes 
rain <- c(2,10,17,18,21,54,59,60) 
wind <- c(9,19,20,24,25,30,40,42,45,46,47,51,52,56)
birds <- c(3,11,14,15,28,33,37,39,43,57,58)
insects <- c(1,4,22,26,27,29)
cicadas <- c(7,8,12,16,32,34,44,48)
planes <- c(23,49)
quiet <- c(5,6,13,31,35,36,38,41,50,53,55)
list <- c("rain","wind","birds","insects","planes","quiet")

mixtures <- c(2,4,7,8,22,26,30,39,45,54,60)
inconsistent <- c(17,24,28,36,40,50,57)

tiff("Figures for plos article/S4_rain_Gym.tiff", width = 2300, 
     height = (370*length(rain)+20), units = 'px', res = 300)
par(mfrow=c(length(rain), 14), mar=c(1, 0, 2, 0.1), 
    oma=c(1.1, 2.1, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in rain) {
  clust_num <- i
  colour <- rain_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_rain_Woon.tiff", width = 2240, 
     height = (370*length(rain)+20), units = 'px', res = 300)
par(mfrow=c(length(rain), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in rain) {
  clust_num <- i
  colour <- rain_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
dev.off()
#############
# WIND
tiff("Figures for plos article/S4_wind1_Gym.tiff", width = 2300, 
     height = (370*length(wind[1:8])+20), units = 'px', res = 300)
par(mfrow=c(length(wind[1:8]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 2.1, 0, 0), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in wind[1:8]) {
  clust_num <- i
  colour <- wind_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_wind1_Woon.tiff", width = 2250, 
     height = (370*length(wind[1:8])+20), units = 'px', res = 300)
par(mfrow=c(length(wind[1:8]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in wind[1:8]) {
  clust_num <- i
  colour <- wind_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
dev.off()

tiff("Figures for plos article/S4_wind2_Gym.tiff", width = 2300, 
     height = (370*length(wind[9:length(wind)])+20), units = 'px', res = 300)
par(mfrow=c(length(wind[9:length(wind)]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 2.1, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in wind[9:length(wind)]) {
  clust_num <- i
  colour <- wind_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_wind2_Woon.tiff", width = 2250, 
     height = (370*length(wind[9:length(wind)])+20), units = 'px', res = 300)
par(mfrow=c(length(wind[9:length(wind)]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in wind[9:length(wind)]) {
  clust_num <- i
  colour <- wind_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
dev.off()
#############
# PLANES
tiff("Figures for plos article/S4_planes_Gym.tiff", width = 2300, 
     height = (370*length(planes)+20), units = 'px', res = 300)
par(mfrow=c(length(planes), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 2.6, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in planes) {
  clust_num <- i
  colour <- planes_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.9, outer = T, cex = 0.85,
      "   Average number of cluster minutes in")
mtext(side = 2, line = 1.1, outer = T, cex = 0.85,
      " 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1.0,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_planes_Woon.tiff", width = 2250, 
     height = (370*length(planes)+20), units = 'px', res = 300)
par(mfrow=c(length(planes), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in planes) {
  clust_num <- i
  colour <- planes_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
dev.off()
#############
tiff("Figures for plos article/S4_wind2_Gym.tiff", width = 2300, 
     height = (370*length(wind[9:length(wind)])+20), units = 'px', res = 300)
par(mfrow=c(length(wind[9:length(wind)]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 2.1, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in wind[9:length(wind)]) {
  clust_num <- i
  colour <- wind_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_wind2_Woon.tiff", width = 2250, 
     height = (370*length(wind[9:length(wind)])+20), units = 'px', res = 300)
par(mfrow=c(length(wind[9:length(wind)]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in wind[9:length(wind)]) {
  clust_num <- i
  colour <- wind_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
dev.off()
#########
tiff("Figures for plos article/S4_birds1_Gym.tiff", width = 2300, 
     height = (370*length(birds[1:8])+20), units = 'px', res = 300)
par(mfrow=c(length(birds[1:8]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 2.1, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in birds[1:8]) {
  clust_num <- i
  colour <- birds_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_birds1_Woon.tiff", width = 2240, 
     height = (370*length(birds[1:8])+20), units = 'px', res = 300)
par(mfrow=c(length(birds[1:8]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in birds[1:8]) {
  clust_num <- i
  colour <- birds_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
dev.off()

tiff("Figures for plos article/S4_birds2_Gym.tiff", width = 2300, 
     height = (370*length(birds[9:length(birds)])+20), 
     units = 'px', res = 300)
par(mfrow=c(length(birds[9:length(birds)]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 2.6, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in birds[9:length(birds)]) {
  clust_num <- i
  colour <- birds_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.9, outer = T, cex = 0.85,
      "   Average number of cluster minutes in")
mtext(side = 2, line = 1.1, outer = T, cex = 0.85,
      " 2 hour period each month")
#mtext(side = 2, line = 1.3, outer = T,
#      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_birds2_Woon.tiff", width = 2240, 
     height = (370*length(birds[9:length(birds)])+20), units = 'px', res = 300)
par(mfrow=c(length(birds[9:length(birds)]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in birds[9:length(birds)]) {
  clust_num <- i
  colour <- birds_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
dev.off()
##########
# CICADAS
tiff("Figures for plos article/S4_cicadas_Gym.tiff", width = 2300, 
     height = (370*length(cicadas)+20), units = 'px', res = 300)
par(mfrow=c(length(cicadas), 14), mar=c(1, 0, 2, 0.1), 
    oma=c(1.1, 2.1, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in cicadas) {
  clust_num <- i
  colour <- cicadas_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_cicadas_Woon.tiff", width = 2240, 
     height = (370*length(cicadas)+20), units = 'px', res = 300)
par(mfrow=c(length(cicadas), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in cicadas) {
  clust_num <- i
  colour <- cicadas_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
dev.off()
##########
# QUIET
tiff("Figures for plos article/S4_quiet1_Gym.tiff", width = 2300, 
     height = (370*length(quiet[1:8])+20), units = 'px', res = 300)
par(mfrow=c(length(quiet[1:8]), 14), mar=c(1, 0, 2, 0.1), 
    oma=c(1.1, 2.1, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in quiet[1:8]) {
  clust_num <- i
  colour <- quiet_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_quiet1_Woon.tiff", width = 2240, 
     height = (370*length(quiet[1:8])+20), units = 'px', res = 300)
par(mfrow=c(length(quiet[1:8]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in quiet[1:8]) {
  clust_num <- i
  colour <- quiet_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
dev.off()

tiff("Figures for plos article/S4_quiet2_Gym.tiff", width = 2300, 
     height = (370*length(quiet[9:length(quiet)])+20), units = 'px', res = 300)
par(mfrow=c(length(quiet[9:length(quiet)]), 14), mar=c(1, 0, 2, 0.1), 
    oma=c(1.1, 2.6, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in quiet[9:length(quiet)]) {
  clust_num <- i
  colour <- quiet_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.9, outer = T, cex = 0.85,
      "   Average number of cluster minutes")
mtext(side = 2, line = 1.1, outer = T, cex = 0.85,
      " 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_quiet2_Woon.tiff", width = 2240, 
     height = (370*length(quiet[9:length(quiet)])+20), units = 'px', res = 300)
par(mfrow=c(length(quiet[9:length(quiet)]), 14), 
    mar=c(1, 0, 2, 0.1), oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in quiet[9:length(quiet)]) {
  clust_num <- i
  colour <- quiet_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
dev.off()
##########
# INSECTS
tiff("Figures for plos article/S4_insects_Gym.tiff", width = 2300, 
     height = (370*length(insects)+20), units = 'px', res = 300)
par(mfrow=c(length(insects), 14), mar=c(1, 0, 2, 0.1), 
    oma=c(1.1, 2.1, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in insects) {
  clust_num <- i
  colour <- insects_col
  plot_funct(clust_num, colour, "site1")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Gympie NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
mtext(side = 1, outer = T,line = 0.15, cex = 1,
      "(MIX) - Cluster with more than one dominant acoustic class; (IC) - Inconsistent cluster")
dev.off()

tiff("Figures for plos article/S4_insects_Woon.tiff", width = 2250, 
     height = (370*length(insects)+20), units = 'px', res = 300)
par(mfrow=c(length(insects), 14), mar=c(1, 0, 2, 0.1), 
    oma=c(1.1, 1.5, 1, 0.2), xpd = NA,
    cex = 1, cex.axis = 0.6, cex.main = 1)
for(i in insects) {
  clust_num <- i
  colour <- insects_col
  plot_funct(clust_num, colour, "site2")
  if(i %in% mixtures) {
    mtext(side = 3, line = 1, paste("Cluster",i," (MIX)","             ", sep = ""))  
  }
  if(i %in% inconsistent) {
    mtext(side = 3, line = 1, paste("Cluster",i," (IC)","          ", sep = ""))  
  }
  if(!(i %in% mixtures) & !(i %in% inconsistent)) {
    mtext(side = 3, line = 1, paste("Cluster",i,"          ", sep = ""))  
  }
}
mtext(side = 3, "Woondum NP 22 June 2015 - 23 July 2016", outer = T)
mtext(side = 2, line = 1.3, outer = T,
      "       Average number of cluster minutes in 2 hour period each month")
dev.off()
##########

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 7 Two hour plots  Four month dataset --------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
rm(list = ls())
dataset <- read.csv("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\hybrid_clust_knn_17500_3_k30_2hour_full111days.csv",header=T)

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
days.per.month <- rep(days.per.month, each=12)

per.month.per.period <- aggregate(dataset[,sapply(dataset,is.numeric)],
                                  dataset["site.yr.mth"],sum)
per.month.per.period <- per.month.per.period[c(2:length(per.month.per.period),1)]

for (i in 1:length(per.month.per.period$V1)) {
  per.month.per.period[i,1:(length(per.month.per.period)-1)] <- 
    per.month.per.period[i,1:(length(per.month.per.period)-1)]/days.per.month[i]
}

length_Gympie <- length(which(substr(per.month.per.period$site.yr.mth, 1,6)=="Gympie"))
length_Woondum <- length(which(substr(per.month.per.period$site.yr.mth, 1,7)=="Woondum"))

names <-c("V1 Slight wind","V2 Slight wind + insects","V3 insects",
          "V4 quiet + insects","V5 light rain","V6 very quiet",
          "V7 Rain","V8 quiet + some insects",
          "V9 wind + birds","V10 birds (morning)",
          "V11 planes","V12 birds (morning)",
          "V13 quieter planes","V14 wind + insects + birds",
          "V15 wind + birds","V16 Birds (morning)","V17 Wind",
          "V18 birds + wind","V19 rain","V20 Mid frequency birds",
          "V21 quiet + some birds","V22 quiet + some insects + birds", 
          "V23 birds + wind", "V24 Very quiet", 
          "V25 Thunder and kookaburras","V26 Birds",
          "V27 Birds","V28 birds + insects", "V29 birds (morning)",
          "V30 Wind + birds")

# Gympie and Woondum combination plots
for (i in 1:(length(per.month.per.period)-1)) {
  tiff(paste("Average minutes per month_Gympie_Woondum_thesis",i,".tiff"),
      width=590, height=1270, res=300)
  par(mfrow=c(2,5), mar=c(0.5, 0.5, 2, 0.2), oma=c(1.4, 1.6, 0, 0.2),
      cex.axis=0.4, mgp=c(1,0.4,0))
  opar <- par(lwd = 0.4)
  cex.axis <- 0.6
  # June 2015
  b <-barplot(per.month.per.period[1:(length_Gympie/length(dates1)),i], beside=T,
          col="grey80", las=1, yaxt="n", axes = F,
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=2,"Average number of minutes in two hour period",outer=T, 
        line=0.8, cex=0.6)
  mtext(side=1, "Time (24 hours)", line=0.4, outer = T, cex=0.6)
  mtext(side=3, "Gympie National Park", line=-1.2, outer = T, cex=0.6)
  axis(side=2, las=1, cex.axis=1)
  month <- "Jun15"
  mtext(side=3, month, cex=0.5)
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  # July 2015
  par(new=F)
  month <- "Jul15"
  b <- barplot(per.month.per.period[(1+length_Gympie/length(dates1)):
                                 (2*(length_Gympie/length(dates1))),i], 
          beside=T, las=1, xaxt="n", yaxt="n",
          col="grey80", xlab="", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  # August 2015
  par(new=F)
  month <- "Aug15"
  b <- barplot(per.month.per.period[(1+2*length_Gympie/length(dates1)):
                                 (3*(length_Gympie/length(dates1))),i],
          beside=T, col="grey80", las=1, xaxt="n", yaxt="n",
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  # September 2015
  par(new=F)
  month <- "Sep15"
  b <- barplot(per.month.per.period[(1+3*length_Gympie/length(dates1)):
                                 (4*(length_Gympie/length(dates1))),i],
          beside=T, col="grey80", las=1, xaxt="n", yaxt="n",
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  # October 2015
  par(new=F)
  month <- "Oct15"
  b <- barplot(per.month.per.period[(1+4*length_Gympie/length(dates1)):
                                 (5*(length_Gympie/length(dates1))),i],
          beside=T, col="grey80", las=1, xaxt="n", yaxt="n",
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  mtext(side=3, paste(substr(per.month.per.period$site.yr.mth[1],1,8),names[i]),
        outer=T,line=1, cex.axis=0.4, cex=0.6)
  # Woondum plot
  # June 2015
  b <- barplot(per.month.per.period[(1+(5*length_Woondum/length(dates1))):(6*(length_Woondum/length(dates1))),i],
              col="grey80", las=1, xaxt="n", yaxt="n", axes = F,
              ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3,"Woondum National Park",outer=T, line=-16.6, cex=0.6)
  axis(side=2, las=1, cex.axis=1)
  month <- "Jun15"
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  # July 2015
  par(new=F)
  month <- "Jul15"
  b <- barplot(per.month.per.period[(1+(6*length_Woondum/length(dates1))):(7*(length_Woondum/length(dates1))),i],
               beside=T, las=1, xaxt="n", yaxt="n",
               col="grey80", xlab="", 
               ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  # August 2015
  par(new=F)
  month <- "Aug15"
  b <- barplot(per.month.per.period[(1+(7*length_Woondum/length(dates1))):(8*(length_Woondum/length(dates1))),i],
               beside=T, col="grey80", las=1, xaxt="n", yaxt="n",
               ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  # September 2015
  par(new=F)
  month <- "Sep15"
  b <- barplot(per.month.per.period[(1+(8*length_Woondum/length(dates1))):(9*(length_Woondum/length(dates1))),i],
               beside=T, col="grey80", las=1, xaxt="n", yaxt="n",
               ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  # October 2015
  par(new=F)
  month <- "Oct15"
  b <- barplot(per.month.per.period[(1+(9*length_Woondum/length(dates1))):(10*(length_Woondum/length(dates1))),i],
               beside=T, col="grey80", las=1, xaxt="n", yaxt="n",
               ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)))
  mtext(side=3, month, cex=0.5)
  abline(v=c((b[1]-0.5)), lwd=0.01) #, b[length(b)]+0.5))
  axis(side=1, tick = F, at=(b[1]+0.4), labels="0", 
       tck=-0.07, cex.axis=cex.axis, padj=-1, col = NA)
  axis(side=1, tick = T, at=(b[1]-0.5), labels="", tck=-0.1, col = NA, col.ticks = 1)
  axis(side=1, tick = F, at=(b[12]-0.4), labels="24", 
       tck=-0.07, cex.axis=cex.axis, padj=-1,col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[12]+0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[7]-0.5), labels="", tck=-0.1,
       col = NA, col.ticks = 1) 
  axis(side=1, tick = F, at=(b[c(7)]-0.9), labels=c("12"), 
       tck=-0.07, cex.axis=cex.axis, padj=-1,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b[c(4,10)]-0.5), labels=c("",""), tck=-0.06,
       col = NA, col.ticks = 1)
  axis(side=1, tick = T, at=(b-0.5), labels = c(rep("",12)), tck=-0.03,
       col = NA, col.ticks = 1)
  segments(x0 = (b[1]-0.5), x1=(b[length(b)]+0.5), y0=seq(2,200,2), y1=seq(2,200,2), lwd=0.01, lty=2)
  
  mtext(side=3, paste(substr(per.month.per.period$site.yr.mth[1],1,8),names[i]),
        outer=T,line=1, cex.axis=0.4, cex=0.6)
  dev.off()
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot of distribution of clusters - Cicada Clusters at dusk ------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
rm(list = ls())
list4 <- c("20151101", "20151201", "20160101", "20160201","20160301")
list5 <- c("20151130", "20151231", "20160131", "20160229","20160331")
months <- c("November 2015","December 2015","January 2016",
            "February 2016","March 2016")
dev.off()
tiff("Distribution_of_clusters_cicadas_dusk.tiff", 
     height=2000, width=1713, res=300)
par(mfrow=c(5,2), mar=c(2,2,0.1,0.1), oma=c(1, 1, 2, 0),
    mgp=c(1,0.4,0))
for(j in 1:length(list4)) {
  start <-  strptime(paste(list4[j]), format="%Y%m%d")
  finish <- strptime(paste(list5[j]), format="%Y%m%d")
  # Prepare civil dawn, civil dusk and sunrise and sunset times
  civil_dawn_2015 <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
  civil_dawn_2016 <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/Geoscience_Australia_Sunrise_times_Gympie_2016.csv")
  civil_dawn_2015 <- rbind(civil_dawn_2015, civil_dawn_2016)
  a <- which(as.character(civil_dawn_2015$dates)==paste(substr(start,1,10)))
  b <- which(as.character(civil_dawn_2015$dates)==paste(substr(finish,1,10)))
  days <- (b-a)+1
  civil_dawn_2015 <- civil_dawn_2015[a:b, ]
  if(nchar(as.character(civil_dawn_2015$CivSunset[1]))==3) {
    civil_sunset <- as.numeric(substr(civil_dawn_2015$CivSunset,1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunset,2,3))
  }
  if(nchar(as.character(civil_dawn_2015$CivSunset[1]))==4) {
    civil_sunset <- as.numeric(substr(civil_dawn_2015$CivSunset,1,2))*60 + as.numeric(substr(civil_dawn_2015$CivSunset,3,4))
  }
  
  if(nchar(as.character(civil_dawn_2015$Sunset[1]))==3) {
    sunset <- as.numeric(substr(civil_dawn_2015$Sunset,1,1))*60 + as.numeric(substr(civil_dawn_2015$Sunset,2,3))
  }
  if(nchar(as.character(civil_dawn_2015$Sunset[1]))==4) {
    sunset <- as.numeric(substr(civil_dawn_2015$Sunset,1,2))*60 + as.numeric(substr(civil_dawn_2015$Sunset,3,4))
  }
  # Prepare dates
  dates <- seq(start, finish, by = "1440 mins")
  date.list <- NULL
  for (i in 1:length(dates)) {
    dat <- substr(as.character(dates[i]),1,10)
    date.list <- c(date.list, dat)
  }
  dates <- date.list
  rm(date.list, dat)
  
  minute_list <- rep(1:1440, days)
  dates_61 <- rep(dates, each=1440)
  
  # *** Set the cluster set variables
  k1_value <- 25000
  k2_value <- 60
  dt1 = as.POSIXct("2015-06-22 00:00:00")
  dt2 = as.POSIXct(start)
  diffDateTime = dt2 - dt1
  a <- as.numeric(diffDateTime)
  cluster_list <- read.csv(paste("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/chosen_cluster_list_",
                                 k1_value, "_", k2_value, ".csv", sep=""), header = T)
  cluster_list <- cluster_list[(a*1440+1):((a*1440)+(1440*days)),]
  cluster_list$dates <- dates_61
  cluster_list$civ_dusk <- rep(civil_dawn_2015$CivSunset, each=1440)  #**
  # Convert civil dusk times to minutes
  civ_dusk <- NULL
  for(i in 1:days) {
    time <- as.character(civil_dawn_2015$CivSunset[i])
    if(nchar(time)==3) {
      minutes <- as.numeric(substr(time,1,1))*60 + as.numeric(substr(time,2,3))  
    }
    if(nchar(time)==4) {
      minutes <- as.numeric(substr(time,1,2))*60 + as.numeric(substr(time,3,4))  
    }
    civ_dusk <- c(civ_dusk, minutes)
  }
  civ_dusk <- rep(civ_dusk, each=1440)
  cluster_list$civ_dusk_min <- civ_dusk
  # sunset
  cluster_list$sunset_min <- rep(sunset, each=1440)
  
  cluster_list$ref_civ <- 200
  cluster_list$ref_civ <- cluster_list$minute_reference - cluster_list$civ_dusk_min
  cluster_list$minute_reference <- cluster_list$minute_reference + 1
  cluster_list_temp <- cluster_list
  #sunset line
  
  cluster_list_temp$ref_civ2 <- cluster_list_temp$sunset - cluster_list_temp$civ_dusk_min
  mean_sunset <- mean(cluster_list_temp$ref_civ2)
  #a <- which(cluster_list_temp$cluster_list==37)
  #cluster_list_temp37 <- cluster_list_temp[a, ]
  cluster_list1 <- cluster_list_temp
  #layout(matrix(c(1,1,1,1,1,2,2,2,2), 
  #              nrow = 9, ncol = 1, byrow = TRUE))
  #layout.show(2)
  #par(mar=c(0,2,0,1), oma=c(3.8,3.5,3.2,0), 
  #    cex.axis=1.8, cex=0.45)
  pch <- c(15,1,17,0,19,2,3,4,5,6,7,8,9,10)
  
  list2 <- -55:35
  list3 <- c(-45,-35,-25,-15,-5,5,15,25,35)
  
  cbPalette <- c("#000000","#999999", "#56B4E9", 
                 "#D55E00", "#0072B2", 
                 "#CC79A7","#009E73","#E69F00")
  ylim <- c(0, 31)
  
  cluster_list1 <- cluster_list_temp$cluster_list
  
  # cluster 44 
  a <- which(cluster_list_temp$cluster_list==44)
  cluster_list_temp44 <- cluster_list_temp[a, ]
  counts_44 <- NULL
  for(i in list2) {
    a <- which(cluster_list_temp44$ref_civ==i)
    counts_44 <- c(counts_44, length(a))
  }
  x <- 1:length(list2)
  y <- counts_44
  lo <- loess(y~x , span=0.08)
  plot(list2, counts_44, ylim=ylim, xlab="", ylab="", 
       xaxt="n", col=cbPalette[1], yaxt="n", pch = pch[1],
       cex=0.6)
  lines(list2, predict(lo), col=cbPalette[1], lwd=1)
  if(j==1) {
    mtext(line=1, "Gympie National Park", outer = F)
    mtext(side=1, line=-0.1, outer = T,
          "Minutes from civil dusk")
    mtext(side=2, line=-0.12, outer = T,
          "Total number of minutes in each cluster")
  }
  if(j==length(list4)) {
    axis(at=list3, side=1)
  }
  mtext(months[j], cex=0.8)
  abline(v=0,lty=2, col="black")
  axis(at=c(10,20,30,40), side=2, las=1)
  abline(h=c(10,20,30), lwd=0.001, lty=2)
  abline(v=mean_sunset,lty=2, col="black")
  abline(v=list3, lwd=0.01)
  
  # cluster 34
  par(new=TRUE)
  a <- which(cluster_list_temp$cluster_list==34)
  cluster_list_temp34 <- cluster_list_temp[a, ]
  counts_34 <- NULL
  for(i in list2) {
    a <- which(cluster_list_temp34$ref_civ==i)
    counts_34 <- c(counts_34, length(a))
  }
  par(new=TRUE)
  x <- 1:length(list2)
  y <- counts_34
  lo <- loess(y~x, span=0.115)
  plot(list2, counts_34, ylim=ylim,xlab="", ylab="", 
       xaxt="n", col=cbPalette[4], yaxt="n",
       pch = pch[4], cex=0.6)
  lines(list2, predict(lo), col=cbPalette[4], lwd=1)
  
  # legend
  label <- c("cluster 44","cluster 34")
  legend(x=-60, y=(ylim[2]), 
         col = c(cbPalette[1], cbPalette[4]), 
         legend = label, cex = 1.1, bty = "n", 
         horiz = FALSE, xpd=TRUE, pch = pch[c(1,2)],
         x.intersp = 0.9, y.intersp = 0.9, 
         inset=c(-0.15,0), lwd=0.8,
         lty=1, pt.cex = 0.8, pt.lwd = 0.8)
  
  # plot 2
  
  # convert the cluster list temp to the Woondum list
  # *** Set the cluster set variables
  k1_value <- 25000
  k2_value <- 60
  dt1 = as.POSIXct("2015-06-22 00:00:00")
  dt2 = as.POSIXct(start)
  diffDateTime = dt2 - dt1
  a <- as.numeric(diffDateTime)
  cluster_list <- read.csv(paste("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/chosen_cluster_list_",
                                 k1_value, "_", k2_value, ".csv", sep=""), header = T)
  cluster_list <- cluster_list[((a+398)*1440+1):(((a+398)*1440)+(1440*days)),]
  
  cluster_list_temp$cluster_list <- cluster_list
  
  # cluster 44 
  a <- which(cluster_list_temp$cluster_list==44)
  cluster_list_temp44 <- cluster_list_temp[a, ]
  counts_44 <- NULL
  for(i in list2) {
    a <- which(cluster_list_temp44$ref_civ==i)
    counts_44 <- c(counts_44, length(a))
  }
  x <- 1:length(list2)
  y <- counts_44
  lo <- loess(y~x , span=0.08)
  plot(list2, counts_44, ylim=ylim, xlab="", ylab="", 
       xaxt="n", col=cbPalette[1], yaxt="n", pch = pch[1],
       cex=0.6)
  lines(list2, predict(lo), col=cbPalette[1], lwd=1)
  if(j==1) {
    mtext(line=1, "Woondum National Park", outer = F)
  }
  if(j==length(list4)) {
    axis(at=list3, side=1)
  }
  mtext(months[j], cex=0.8)
  abline(v=0,lty=2, col="black")
  axis(at=c(10,20,30,40), side=2, las=1)
  abline(h=c(10,20,30), lwd=0.001, lty=2)
  abline(v=mean_sunset,lty=2, col="black")
  abline(v=list3, lwd=0.01)
  
  # cluster 34
  par(new=TRUE)
  a <- which(cluster_list_temp$cluster_list==34)
  cluster_list_temp34 <- cluster_list_temp[a, ]
  counts_34 <- NULL
  for(i in list2) {
    a <- which(cluster_list_temp34$ref_civ==i)
    counts_34 <- c(counts_34, length(a))
  }
  par(new=TRUE)
  x <- 1:length(list2)
  y <- counts_34
  lo <- loess(y~x, span=0.115)
  plot(list2, counts_34, ylim=ylim,xlab="", ylab="", 
       xaxt="n", col=cbPalette[4], yaxt="n",
       pch = pch[2], cex=0.6)
  lines(list2, predict(lo), col=cbPalette[4], lwd=1)
  
  # legend
  label <- c("cluster 44","cluster 34")
  legend(x=-60, y=(ylim[2]), 
         col = c(cbPalette[1], cbPalette[4]), 
         legend = label, cex = 1.1, bty = "n", 
         horiz = FALSE, xpd=TRUE, pch = pch[c(1,2)],
         x.intersp = 0.9, y.intersp = 0.9, 
         inset=c(-0.15,0), lwd=0.8,
         lty=1, pt.cex = 0.8, pt.lwd = 0.8)
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot of distribution of clusters - Cicada Clusters at dawn ------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
rm(list = ls())
list4 <- c("20151101", "20151201", "20160101", "20160201","20160301")
list5 <- c("20151130", "20151231", "20160131", "20160229","20160331")
months <- c("November 2015","December 2015","January 2016",
            "February 2016","March 2016")
dev.off()
tiff("Distribution_of_clusters_cicadas_dawn.tiff", 
     height=2000, width=1713, res=300)
par(mfrow=c(5,2), mar=c(2,2,0.1,0.1), oma=c(1, 1, 2, 0),
    mgp=c(1,0.4,0))
for(j in 1:length(list4)) {
  start <-  strptime(paste(list4[j]), format="%Y%m%d")
  finish <- strptime(paste(list5[j]), format="%Y%m%d")
  # Prepare civil dawn, civil dusk and sunrise and sunset times
  civil_dawn_2015 <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
  civil_dawn_2016 <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/Geoscience_Australia_Sunrise_times_Gympie_2016.csv")
  civil_dawn_2015 <- rbind(civil_dawn_2015, civil_dawn_2016)
  a <- which(as.character(civil_dawn_2015$dates)==paste(substr(start,1,10)))
  b <- which(as.character(civil_dawn_2015$dates)==paste(substr(finish,1,10)))
  days <- (b-a)+1
  civil_dawn_2015 <- civil_dawn_2015[a:b, ]
  if(nchar(as.character(civil_dawn_2015$CivSunset[1]))==3) {
    civil_sunrise <- as.numeric(substr(civil_dawn_2015$CivSunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise,2,3))
  }
  if(nchar(as.character(civil_dawn_2015$CivSunset[1]))==4) {
    civil_sunrise <- as.numeric(substr(civil_dawn_2015$CivSunrise,1,2))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise,3,4))
  }
  
  if(nchar(as.character(civil_dawn_2015$Sunrise[1]))==3) {
    sunrise <- as.numeric(substr(civil_dawn_2015$Sunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$Sunrise,2,3))
  }
  if(nchar(as.character(civil_dawn_2015$Sunrise[1]))==4) {
    sunrise <- as.numeric(substr(civil_dawn_2015$Sunrise,1,2))*60 + as.numeric(substr(civil_dawn_2015$Sunrise,3,4))
  }
  # Prepare dates
  dates <- seq(start, finish, by = "1440 mins")
  date.list <- NULL
  for (i in 1:length(dates)) {
    dat <- substr(as.character(dates[i]),1,10)
    date.list <- c(date.list, dat)
  }
  dates <- date.list
  rm(date.list, dat)
  
  minute_list <- rep(1:1440, days)
  dates_61 <- rep(dates, each=1440)
  
  # *** Set the cluster set variables
  k1_value <- 25000
  k2_value <- 60
  dt1 = as.POSIXct("2015-06-22 00:00:00")
  dt2 = as.POSIXct(start)
  diffDateTime = dt2 - dt1
  a <- as.numeric(diffDateTime)
  cluster_list <- read.csv(paste("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/chosen_cluster_list_",
                                 k1_value, "_", k2_value, ".csv", sep=""), header = T)
  cluster_list <- cluster_list[(a*1440+1):((a*1440)+(1440*days)),]
  cluster_list$dates <- dates_61
  cluster_list$civ_dawn <- rep(civil_dawn_2015$CivSunrise, each=1440)  #**
  # Convert civil dusk times to minutes
  civ_dawn <- NULL
  for(i in 1:days) {
    time <- as.character(civil_dawn_2015$CivSunrise[i])
    if(nchar(time)==3) {
      minutes <- as.numeric(substr(time,1,1))*60 + as.numeric(substr(time,2,3))  
    }
    if(nchar(time)==4) {
      minutes <- as.numeric(substr(time,1,2))*60 + as.numeric(substr(time,3,4))  
    }
    civ_dawn <- c(civ_dawn, minutes)
  }
  civ_dawn <- rep(civ_dawn, each=1440)
  cluster_list$civ_dawn_min <- civ_dawn
  # sunrise
  cluster_list$sunrise_min <- rep(sunrise, each=1440)
  
  cluster_list$ref_civ <- 200
  cluster_list$ref_civ <- cluster_list$minute_reference - cluster_list$civ_dawn_min
  cluster_list$minute_reference <- cluster_list$minute_reference + 1
  cluster_list_temp <- cluster_list
  #sunrise line
  
  cluster_list_temp$ref_civ2 <- cluster_list_temp$sunrise - cluster_list_temp$civ_dawn_min
  mean_sunrise <- mean(cluster_list_temp$ref_civ2)
  #a <- which(cluster_list_temp$cluster_list==37)
  #cluster_list_temp37 <- cluster_list_temp[a, ]
  cluster_list1 <- cluster_list_temp
  #layout(matrix(c(1,1,1,1,1,2,2,2,2), 
  #              nrow = 9, ncol = 1, byrow = TRUE))
  #layout.show(2)
  #par(mar=c(0,2,0,1), oma=c(3.8,3.5,3.2,0), 
  #    cex.axis=1.8, cex=0.45)
  pch <- c(15,1,17,0,19,2,3,4,5,6,7,8,9,10)
  
  list2 <- -55:35
  list3 <- c(-45,-35,-25,-15,-5,5,15,25,35)
  
  cbPalette <- c("#000000","#999999", "#56B4E9", 
                 "#D55E00", "#0072B2", 
                 "#CC79A7","#009E73","#E69F00")
  ylim <- c(0, 31)
  
  cluster_list1 <- cluster_list_temp$cluster_list
  
  # cluster 44 
  a <- which(cluster_list_temp$cluster_list==44)
  cluster_list_temp44 <- cluster_list_temp[a, ]
  counts_44 <- NULL
  for(i in list2) {
    a <- which(cluster_list_temp44$ref_civ==i)
    counts_44 <- c(counts_44, length(a))
  }
  x <- 1:length(list2)
  y <- counts_44
  lo <- loess(y~x , span=0.08)
  plot(list2, counts_44, ylim=ylim, xlab="", ylab="", 
       xaxt="n", col=cbPalette[1], yaxt="n", pch = pch[1],
       cex=0.6)
  lines(list2, predict(lo), col=cbPalette[1], lwd=1)
  if(j==1) {
    mtext(line=1, "Gympie National Park", outer = F)
    mtext(side=1, line=-0.1, outer = T,
          "Minutes from civil dawn")
    mtext(side=2, line=-0.12, outer = T,
          "Total number of minutes in each cluster")
  }
  if(j==length(list4)) {
    axis(at=list3, side=1)
  }
  mtext(months[j], cex=0.8)
  abline(v=0,lty=2, col="black")
  axis(at=c(10,20,30,40), side=2, las=1)
  abline(h=c(10,20,30), lwd=0.001, lty=2)
  abline(v=mean_sunrise,lty=2, col="black")
  abline(v=list3, lwd=0.01)
  
  # cluster 34
  par(new=TRUE)
  a <- which(cluster_list_temp$cluster_list==34)
  cluster_list_temp34 <- cluster_list_temp[a, ]
  counts_34 <- NULL
  for(i in list2) {
    a <- which(cluster_list_temp34$ref_civ==i)
    counts_34 <- c(counts_34, length(a))
  }
  par(new=TRUE)
  x <- 1:length(list2)
  y <- counts_34
  lo <- loess(y~x, span=0.115)
  plot(list2, counts_34, ylim=ylim,xlab="", ylab="", 
       xaxt="n", col=cbPalette[4], yaxt="n",
       pch = pch[4], cex=0.6)
  lines(list2, predict(lo), col=cbPalette[4], lwd=1)
  
  # legend
  label <- c("cluster 44","cluster 34")
  legend(x=-60, y=(ylim[2]), 
         col = c(cbPalette[1], cbPalette[4]), 
         legend = label, cex = 1.1, bty = "n", 
         horiz = FALSE, xpd=TRUE, pch = pch[c(1,2)],
         x.intersp = 0.9, y.intersp = 0.9, 
         inset=c(-0.15,0), lwd=0.8,
         lty=1, pt.cex = 0.8, pt.lwd = 0.8)
  
  # plot 2
  
  # convert the cluster list temp to the Woondum list
  # *** Set the cluster set variables
  k1_value <- 25000
  k2_value <- 60
  dt1 = as.POSIXct("2015-06-22 00:00:00")
  dt2 = as.POSIXct(start)
  diffDateTime = dt2 - dt1
  a <- as.numeric(diffDateTime)
  cluster_list <- read.csv(paste("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/chosen_cluster_list_",
                                 k1_value, "_", k2_value, ".csv", sep=""), header = T)
  cluster_list <- cluster_list[((a+398)*1440+1):(((a+398)*1440)+(1440*days)),]
  
  cluster_list_temp$cluster_list <- cluster_list
  
  # cluster 44 
  a <- which(cluster_list_temp$cluster_list==44)
  cluster_list_temp44 <- cluster_list_temp[a, ]
  counts_44 <- NULL
  for(i in list2) {
    a <- which(cluster_list_temp44$ref_civ==i)
    counts_44 <- c(counts_44, length(a))
  }
  x <- 1:length(list2)
  y <- counts_44
  lo <- loess(y~x , span=0.08)
  plot(list2, counts_44, ylim=ylim, xlab="", ylab="", 
       xaxt="n", col=cbPalette[1], yaxt="n", pch = pch[1],
       cex=0.6)
  lines(list2, predict(lo), col=cbPalette[1], lwd=1)
  if(j==1) {
    mtext(line=1, "Woondum National Park", outer = F)
  }
  if(j==length(list4)) {
    axis(at=list3, side=1)
  }
  mtext(months[j], cex=0.8)
  abline(v=0,lty=2, col="black")
  axis(at=c(10,20,30,40), side=2, las=1)
  abline(h=c(10,20,30), lwd=0.001, lty=2)
  abline(v=mean_sunrise,lty=2, col="black")
  abline(v=list3, lwd=0.01)
  
  # cluster 34
  par(new=TRUE)
  a <- which(cluster_list_temp$cluster_list==34)
  cluster_list_temp34 <- cluster_list_temp[a, ]
  counts_34 <- NULL
  for(i in list2) {
    a <- which(cluster_list_temp34$ref_civ==i)
    counts_34 <- c(counts_34, length(a))
  }
  par(new=TRUE)
  x <- 1:length(list2)
  y <- counts_34
  lo <- loess(y~x, span=0.115)
  plot(list2, counts_34, ylim=ylim,xlab="", ylab="", 
       xaxt="n", col=cbPalette[4], yaxt="n",
       pch = pch[2], cex=0.6)
  lines(list2, predict(lo), col=cbPalette[4], lwd=1)
  
  # legend
  label <- c("cluster 44","cluster 34")
  legend(x=-60, y=(ylim[2]), 
         col = c(cbPalette[1], cbPalette[4]), 
         legend = label, cex = 1.1, bty = "n", 
         horiz = FALSE, xpd=TRUE, pch = pch[c(1,2)],
         x.intersp = 0.9, y.intersp = 0.9, 
         inset=c(-0.15,0), lwd=0.8,
         lty=1, pt.cex = 0.8, pt.lwd = 0.8)
}
dev.off()

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Species_temporal_distribution ----------------------------
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
civil_dawn_2015 <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
data2 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Scarlet Honeyeater.csv", header = T)
all_data <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/all_data_added_protected.csv", header = T)[,c(1:21,37)]

cbPalette <- c("#000000","#999999", "#56B4E9", 
               "#D55E00", "#0072B2", 
               "#CC79A7","#009E73","#E69F00")
pch <- c(15,1,17,0,19,2,3,4,5,6,7,8,9,10)
pch1 <- pch[2]
pch2 <- pch[3]
pch3 <- pch[1]
colour1 <- cbPalette[2]
colour2 <- cbPalette[4]
colour3 <- cbPalette[1]
lwd1 <- 2
lwd2 <- 2
lwd3 <- 2
lty1 <- 1
lty2 <- 1
lty3 <- 1
pch <- 16
sunrise_mean <- 24.642
# species plot -----------------------------------
dev.off()
tiff("Species_temporal_distribution.tiff", 
     height=2244, width=1713, res=300)
list2 <- -55:35
list3 <- c(-25,-15,-5,5,15,25)

layout(matrix(c(1,1,1,1,
                8,
                2,2,2,2,
                9,
                3,3,3,3,
                10,
                4,4,4,4,
                11,
                5,5,5,5,
                12,
                6,6,6,6,
                13,
                7,7,7,7), nrow = 34, ncol = 1, byrow = TRUE))
layout.show(34)
## show the regions that have been allocated to each plot
par(mar=c(0, 1.3, 0, 0.6), oma = c(4, 4, 4.2, 0), 
    cex.axis = 1.8, cex = 0.45, tcl=-0.4)

# Eastern Yellow Robin----------------------------------
label_name <- "Eastern Yellow Robin"
list <- c("EYR Far", "EYR Mod", "EYR Near")
#list1 <- c("EYR Quiet", "EYR Mod", "EYR Loud")

kalscpe_data <- NULL
kalscpe_data_EYR <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_EYR <- rbind(kalscpe_data_EYR, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_EYR)) {
  dat <- paste(substr(kalscpe_data_EYR$IN.FILE[i],1,4), 
               substr(kalscpe_data_EYR$IN.FILE[i],5,6),
               substr(kalscpe_data_EYR$IN.FILE[i],7,8), sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_EYR$min[i] <- round(floor(kalscpe_data_EYR$OFFSET[i]/60), 0)
  kalscpe_data_EYR$ref_civ[i] <- kalscpe_data_EYR$min[i] - civ_dawn
}

# EYR Far
ylim <- c(0,500)
a <- which(kalscpe_data_EYR$V23=="EYR Far")
kalscpe_data_EYR_temp_far <- kalscpe_data_EYR[a, ]
counts_EYR_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EYR_temp_far$ref_civ==i)
  counts_EYR_Far <- c(counts_EYR_Far, length(a))
}
x <- 1:length(list2)
y <- counts_EYR_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_EYR_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n",  yaxt="n", col=colour1, las=2, 
     pch = pch, lwd = lwd1, cex=0.8)
mtext(side = 3, "a.", cex = 1, adj = 0.005, outer = F,
      line = 0)
lines(list2, predict(lo), col=colour1, 
      lwd=lwd1)

axis(at=c(200,400), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(200,400), lwd=0.001, lty=2)
abline(v=list3)
mtext("Eastern Yellow Robin (EYR)", 
      line=0.1, cex=1)

mtext("Calling rates in relation to civil dawn", 
      line=1.9, cex=1.4)
text(x = -20, y = 0.97*ylim[2]-20, "Pr-C-D", cex = 1.8)
text(x =   0, y = 0.97*ylim[2]-20, "C-D", cex = 1.8)
text(x =  20, y = 0.97*ylim[2]-20, "Po-C-D", cex = 1.8)

label <- c("Far","Mod","Near")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour1, colour2, colour3),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = c(pch, pch, pch),
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(lwd1,lwd2,lwd3),
       lty=c(1,1,1), pt.cex = c(1.2,1.2,1.2), pt.lwd = 0.8,
       seg.len=4)

par(new=TRUE)

# EYR Mod
a <- which(kalscpe_data_EYR$V23=="EYR Mod")
kalscpe_data_EYR_temp_mod <- kalscpe_data_EYR[a, ]
counts_EYR_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EYR_temp_mod$ref_civ==i)
  counts_EYR_Mod <- c(counts_EYR_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_EYR_Mod
lo <- loess(y~x , span=0.09)
plot(list2,counts_EYR_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n",yaxt="n", col=colour2, las=1, 
     pch = pch, lwd = lwd2, cex=0.8)
lines(list2, predict(lo), col=colour2, 
      lwd=lwd2, lty=lty2)
abline(v=list3)
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
# EYR Near
par(new=TRUE)
a <- which(kalscpe_data_EYR$V23=="EYR Near")
kalscpe_data_EYR_temp_near <- kalscpe_data_EYR[a, ]
counts_EYR_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EYR_temp_near$ref_civ==i)
  counts_EYR_Near <- c(counts_EYR_Near, length(a))
}
x <- 1:length(list2)
y <- counts_EYR_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_EYR_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, lwd = lwd3, cex=0.8)
lines(list2, predict(lo), col= colour3, 
      lwd=lwd3, lty = lty3)

#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
abline(v=c(0,sunrise_mean),lty=2, col="black")

# White-throated Honeyeater-------------------------
ylim <- c(0,650)
label_name <- "White-throated Honeyeater"
list <- c("WTH Far", "WTH Mod", "WTH Near")
list1 <- c("WTH Quiet", "WTH Mod", "WTH Loud")

kalscpe_data <- NULL
kalscpe_data_WTH <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_WTH <- rbind(kalscpe_data_WTH, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_WTH)) {
  dat <- paste(substr(kalscpe_data_WTH$IN.FILE[i],1,4), 
               substr(kalscpe_data_WTH$IN.FILE[i],5,6),
               substr(kalscpe_data_WTH$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_WTH$min[i] <- round(floor(kalscpe_data_WTH$OFFSET[i]/60), 0)
  kalscpe_data_WTH$ref_civ[i] <- kalscpe_data_WTH$min[i] - civ_dawn
}

# WTH Far
a <- which(kalscpe_data_WTH$V23=="WTH Far")
kalscpe_data_WTH_temp_far <- kalscpe_data_WTH[a, ]
counts_WTH_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTH_temp_far$ref_civ==i)
  counts_WTH_Far <- c(counts_WTH_Far, length(a))
}
x <- 1:length(list2)
y <- counts_WTH_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTH_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour1, las=1, 
     pch = pch, lwd = lwd1, cex=0.8)
mtext(side = 3, "b.", cex = 1, adj = 0.005, outer = F,
      line = 0)
lines(list2, predict(lo), col=colour1, 
      lwd=lwd1, lty=lty1)
abline(v=list3)
axis(at=c(200,400, 600), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(200,400,600), lwd=0.001, lty=2)
mtext("White-throated Honeyeater (WTH)", 
      line=0.1, cex=1)

# legend
label <- c("Far", "Mod", "Near")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour1, colour2, colour3),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = c(pch, pch, pch),
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(lwd1,lwd2,lwd3),
       lty=c(1,1,1), pt.cex = c(1, 1, 1), pt.lwd = 0.8,
       seg.len=4)

par(new=TRUE)

# WTH Mod
a <- which(kalscpe_data_WTH$V23=="WTH Mod")
kalscpe_data_WTH_temp_mod <- kalscpe_data_WTH[a, ]
counts_WTH_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTH_temp_mod$ref_civ==i)
  counts_WTH_Mod <- c(counts_WTH_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_WTH_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTH_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour2, las=1, 
     pch = pch, lwd = lwd2, cex=0.8)
lines(list2, predict(lo), col=colour2, 
      lwd=lwd2, lty=lty2)
abline(v=list3)

par(new=TRUE)
a <- which(kalscpe_data_WTH$V23=="WTH Near")
kalscpe_data_WTH_temp_near <- kalscpe_data_WTH[a, ]
counts_WTH_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTH_temp_near$ref_civ==i)
  counts_WTH_Near <- c(counts_WTH_Near, length(a))
}
x <- 1:length(list2)
y <- counts_WTH_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTH_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, lwd = lwd3, cex=0.8)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd3, lty=lty3)
abline(v=list3, lty=2, col="black")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
abline(v=c(0,sunrise_mean),lty=2, col="black")

# Laughing Kookaburra----------------------------
ylim <- c(0,53)
ylim <- c(0,12)
label_name <- "Laughing Kookaburra"
list <- c("Kookaburra Quiet","Kookaburra Cackle", "Kookaburra Mod", "Kookaburra Loud")
#list1 <- c("KOOK Quiet","KOOK Mod", "KOOK Loud")

kalscpe_data <- NULL
kalscpe_data_KOOK <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_KOOK <- rbind(kalscpe_data_KOOK, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_KOOK)) {
  dat <- paste(substr(kalscpe_data_KOOK$IN.FILE[i],1,4), 
               substr(kalscpe_data_KOOK$IN.FILE[i],5,6),
               substr(kalscpe_data_KOOK$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_KOOK$min[i] <- round(floor(kalscpe_data_KOOK$OFFSET[i]/60), 0)
  kalscpe_data_KOOK$ref_civ[i] <- kalscpe_data_KOOK$min[i] - civ_dawn
}

# KOOK trill Far
a <- which(kalscpe_data_KOOK$V23=="Kookaburra Quiet")
kalscpe_data_KOOK_temp_far <- kalscpe_data_KOOK[a, ]
counts_KOOK_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_KOOK_temp_far$ref_civ==i)
  counts_KOOK_Far <- c(counts_KOOK_Far, length(a))
}
x <- 1:length(list2)
y <- counts_KOOK_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_KOOK_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour1, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "c.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("Laughing Kookaburra (LKB)", 
      line=0.1, cex=1)

lines(list2, predict(lo), col=colour1, 
      lwd=lwd1, lty=lty1)
abline(v=list3, lty=2, col="black")
axis(at=c(5,10,20,25,30,40), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(5,10,20,25,30,40), lwd=0.001, lty=2)
# legend
label <- c("Far", "Mod", "Near")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour1, colour2, colour3),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = c(pch, pch, pch),
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(lwd1,lwd2,lwd3),
       lty=c(1,1,1), pt.cex = c(1, 1, 1), pt.lwd = 0.8,
       seg.len=4)

par(new=TRUE)

# Kookaburra Mod
a <- which(kalscpe_data_KOOK$V23=="Kookaburra Mod")
kalscpe_data_KOOK_temp_mod <- kalscpe_data_KOOK[a, ]
counts_KOOK_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_KOOK_temp_mod$ref_civ==i)
  counts_KOOK_Mod <- c(counts_KOOK_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_KOOK_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_KOOK_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour2, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour2, 
      lwd=lwd2, lty = lty2)
abline(v=list3)

# Kookaburra Loud
par(new=TRUE)
a <- which(kalscpe_data_KOOK$V23=="Kookaburra Loud")
kalscpe_data_KOOK_temp_near <- kalscpe_data_KOOK[a, ]
counts_KOOK_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_KOOK_temp_near$ref_civ==i)
  counts_KOOK_Near <- c(counts_KOOK_Near, length(a))
}
x <- 1:length(list2)
y <- counts_KOOK_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_KOOK_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n",  yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd3, lty=lty3)
abline(v=list3)
abline(v=c(0,sunrise_mean),lty=2, col="black")

# Scarlet Honeyeater 1--------------------------------------
ylim <- c(0, 150)
label_name <- "Scarlet Honeyeater SC1"
list <- c("SC1 Far", "SC1 Mod", "SC1 Near")
#list1 <- c("SC1 Quiet", "SC1 Mod", "SC1 Loud")
kalscpe_data <- NULL
kalscpe_data_SC1 <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_SC1 <- rbind(kalscpe_data_SC1, kalscpe_data)
}

for(i in 1:nrow(kalscpe_data_SC1)) {
  dat <- paste(substr(kalscpe_data_SC1$IN.FILE[i],1,4), 
               substr(kalscpe_data_SC1$IN.FILE[i],5,6),
               substr(kalscpe_data_SC1$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_SC1$min[i] <- round(floor(kalscpe_data_SC1$OFFSET[i]/60), 0)
  kalscpe_data_SC1$ref_civ[i] <- kalscpe_data_SC1$min[i] - civ_dawn
}

# SC1 Far
a <- which(kalscpe_data_SC1$V23=="SC1 Far")
kalscpe_data_SC1_temp_far <- kalscpe_data_SC1[a, ]
counts_SC1_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC1_temp_far$ref_civ==i)
  counts_SC1_Far <- c(counts_SC1_Far, length(a))
}
x <- 1:length(list2)
y <- counts_SC1_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC1_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour1, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "d.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("Scarlet Honeyeater (SH1)", 
      line=0.1, cex=1)

lines(list2, predict(lo), col=colour1, 
      lwd=lwd1, lty=lty1)
abline(v=list3,lty=2, col="black")
axis(at=c(50,100,150), side=2, las=1,  
     cex.axis=2, hadj = 0.75)
abline(h=c(50,100,150), lwd=0.001, lty=2)

#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
par(new=TRUE)

# SC1 Mod
a <- which(kalscpe_data_SC1$V23=="SC1 Mod")
kalscpe_data_SC1_temp_mod <- kalscpe_data_SC1[a, ]
counts_SC1_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC1_temp_mod$ref_civ==i)
  counts_SC1_Mod <- c(counts_SC1_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_SC1_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC1_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour2, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour2, 
      lwd=lwd2, lty=lty2)
abline(v=list3)
# legend
label <- c("Far", "Mod", "Near")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour1, colour2, colour3),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = c(pch, pch, pch),
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(lwd1,lwd2,lwd3),
       lty=c(1,1,1), pt.cex = c(1, 1, 1), pt.lwd = 0.8,
       seg.len=4)

#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
# SC1 Near
par(new=TRUE)
a <- which(kalscpe_data_SC1$V23=="SC1 Near")
kalscpe_data_SC1_temp_near <- kalscpe_data_SC1[a, ]
counts_SC1_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC1_temp_near$ref_civ==i)
  counts_SC1_Near <- c(counts_SC1_Near, length(a))
}
x <- 1:length(list2)
y <- counts_SC1_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC1_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd3, lty=lty3)
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
mtext(side=2, cex=1.2, line=2.2, outer = T,
      "Total number of calls over 56 days")
abline(v=c(0,sunrise_mean),lty=2, col="black")

# Scarlet Honeyeater 2-------------------
ylim <- c(0,50)
label_name <- "Scarlet Honeyeater SC2"
list <- c("SC3 Chatter Far", "SC3 Chatter Mod", "SC3 Chatter Near")
#list1 <- c("SC3 Chatter Quiet", "SC3 Chatter Mod", "SC3 Chatter Loud")
#colours <- c("grey70", "grey50", "grey0","grey20")
kalscpe_data <- NULL
kalscpe_data_SC2 <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_SC2 <- rbind(kalscpe_data_SC2, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_SC2)) {
  dat <- paste(substr(kalscpe_data_SC2$IN.FILE[i],1,4), 
               substr(kalscpe_data_SC2$IN.FILE[i],5,6),
               substr(kalscpe_data_SC2$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_SC2$min[i] <- round(floor(kalscpe_data_SC2$OFFSET[i]/60), 0)
  kalscpe_data_SC2$ref_civ[i] <- kalscpe_data_SC2$min[i] - civ_dawn
}

# SC2 Far
a <- which(kalscpe_data_SC2$V23=="SC3 Chatter Far")
kalscpe_data_SC2_temp_far <- kalscpe_data_SC2[a, ]
counts_SC2_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC2_temp_far$ref_civ==i)
  counts_SC2_Far <- c(counts_SC2_Far, length(a))
}
x <- 1:length(list2)
y <- counts_SC2_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC2_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour1, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "e.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("Scarlet Honeyeater (SH2)", 
      line=0.1, cex=1)
lines(list2, predict(lo), col=colour1, 
      lwd=lwd1, lty=lty1)
abline(v=list3, lty=2, col="black")
axis(at=c(20,40), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(20,40), lwd=0.001, lty=2)
# legend
label <- c("Far", "Mod", "Near")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour1, colour2, colour3),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = c(pch, pch, pch),
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(lwd1,lwd2,lwd3),
       lty=c(1,1,1), pt.cex = c(1, 1, 1), pt.lwd = 0.8,
       seg.len=4)

#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
par(new=TRUE)

# SC2 Mod
a <- which(kalscpe_data_SC2$V23=="SC3 Chatter Mod")
kalscpe_data_SC2_temp_mod <- kalscpe_data_SC2[a, ]
counts_SC2_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC2_temp_mod$ref_civ==i)
  counts_SC2_Mod <- c(counts_SC2_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_SC2_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC2_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour2, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour2, 
      lwd=lwd2, lty=lty2)
abline(v=list3)
# SC2 Near
par(new=TRUE)
a <- which(kalscpe_data_SC2$V23=="SC3 Chatter Near")
kalscpe_data_SC2_temp_near <- kalscpe_data_SC2[a, ]
counts_SC2_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC2_temp_near$ref_civ==i)
  counts_SC2_Near <- c(counts_SC2_Near, length(a))
}
x <- 1:length(list2)
y <- counts_SC2_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC2_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd3, lty=lty3)
abline(v=list3, lty=2, col="black")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
abline(v=c(0,sunrise_mean),lty=2, col="black")

# White-throated Treecreeper-------------------------------
label_name <- "White-throated Treecreeper"
list <- c("WTT trill Far", "WTT trill Mod", "WTT trill Near")
#list1 <- c("WTT trill Quiet", "WTT trill Mod", "WTT trill Loud")

kalscpe_data <- NULL
kalscpe_data_WTT <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_WTT <- rbind(kalscpe_data_WTT, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_WTT)) {
  dat <- paste(substr(kalscpe_data_WTT$IN.FILE[i],1,4), 
               substr(kalscpe_data_WTT$IN.FILE[i],5,6),
               substr(kalscpe_data_WTT$IN.FILE[i],7,8), sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_WTT$min[i] <- round(floor(kalscpe_data_WTT$OFFSET[i]/60), 0)
  kalscpe_data_WTT$ref_civ[i] <- kalscpe_data_WTT$min[i] - civ_dawn
}

# WTT trill Far
ylim <- c(0,260)
counts_WTT_Far <- NULL
a <- which(kalscpe_data_WTT$V23=="WTT trill Far")
kalscpe_data_WTT_temp_far <- kalscpe_data_WTT[a, ]
counts_WTT_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTT_temp_far$ref_civ==i)
  counts_WTT_Far <- c(counts_WTT_Far, length(a))
}
x <- list2
y <- counts_WTT_Far
lo <- loess(y~x, span=0.09)
plot(list2, counts_WTT_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour1, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "f.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("White-throated Treecreeper (WTT)", 
      line=0.1, cex=1)
axis(at=c(100,200), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(100,200), lwd=0.001, lty=2)
lines(list2, predict(lo), col=colour1, 
      lwd=lwd1, lty=lty1)
abline(v=list3,lty=2, col="black")
# legend
label <- c("Far", "Mod", "Near")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour1, colour2, colour3),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = c(pch, pch, pch),
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(lwd1,lwd2,lwd3),
       lty=c(1,1,1), pt.cex = c(1, 1, 1), pt.lwd = 0.8,
       seg.len=4)

par(new=TRUE)

# WTT trill Mod
a <- which(kalscpe_data_WTT$V23=="WTT trill Mod")
kalscpe_data_WTT_temp_mod <- kalscpe_data_WTT[a, ]
counts_WTT_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTT_temp_mod$ref_civ==i)
  counts_WTT_Mod <- c(counts_WTT_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_WTT_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTT_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour2, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour2, 
      lwd=lwd2, lty=lty2)
abline(v=list3)
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
# WTT trill Near
par(new=TRUE)
a <- which(kalscpe_data_WTT$V23=="WTT trill Near")
kalscpe_data_WTT_temp_near <- kalscpe_data_WTT[a, ]
counts_WTT_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTT_temp_near$ref_civ==i)
  counts_WTT_Near <- c(counts_WTT_Near, length(a))
}
x <- 1:length(list2)
y <- counts_WTT_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTT_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd3, lty=lty3)
abline(v=list3)
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
abline(v=c(0,sunrise_mean),lty=2, col="black")

# Eastern Whipbird---------------------------------
ylim <- c(0,150)
label_name <- "Eastern Whipbird"
list <- c("EW Far", "EW Mod", "EW Near")
list1 <- c("EW Quiet", "EW Mod", "EW Loud")

kalscpe_data <- NULL
kalscpe_data_EW <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_EW <- rbind(kalscpe_data_EW, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_EW)) {
  dat <- paste(substr(kalscpe_data_EW$IN.FILE[i],1,4), 
               substr(kalscpe_data_EW$IN.FILE[i],5,6),
               substr(kalscpe_data_EW$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_EW$min[i] <- round(floor(kalscpe_data_EW$OFFSET[i]/60), 0)
  kalscpe_data_EW$ref_civ[i] <- kalscpe_data_EW$min[i] - civ_dawn
}

# EW Far
a <- which(kalscpe_data_EW$V23=="EW Far")
kalscpe_data_EW_temp_far <- kalscpe_data_EW[a, ]
counts_EW_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EW_temp_far$ref_civ==i)
  counts_EW_Far <- c(counts_EW_Far, length(a))
}
x <- 1:length(list2)
y <- counts_EW_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_EW_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour1, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "g.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("Eastern Whipbird (EWB)", 
      line=0.1, cex=1)
mtext(side=1, line = 2.6, "Minutes from Civil Dawn", cex=1.2)
lines(list2, predict(lo), col=colour1, 
      lwd=lwd1, lty=lty1)
abline(v=list3)
axis(at=c(50,100, 150), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(50,100,150), lwd=0.001, lty=2)
# legend
label <- c("Far", "Mod", "Near")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour1, colour2, colour3),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = c(pch, pch, pch),
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(lwd1,lwd2,lwd3),
       lty=c(1,1,1), pt.cex = c(1, 1, 1), pt.lwd = 0.8,
       seg.len=4)

par(new=TRUE)

# EW Mod
a <- which(kalscpe_data_EW$V23=="EW Mod")
kalscpe_data_EW_temp_mod <- kalscpe_data_EW[a, ]
counts_EW_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EW_temp_mod$ref_civ==i)
  counts_EW_Mod <- c(counts_EW_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_EW_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_EW_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour2, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour2, 
      lwd=lwd2, lty=lty2)
abline(v=list3)

# EW Near
par(new=TRUE)
a <- which(kalscpe_data_EW$V23=="EW Near")
kalscpe_data_EW_temp_near <- kalscpe_data_EW[a, ]
counts_EW_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EW_temp_near$ref_civ==i)
  counts_EW_Near <- c(counts_EW_Near, length(a))
}
x <- 1:length(list2)
y <- counts_EW_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_EW_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd3, lty=lty3)
abline(v=list3)
abline(v=c(0,sunrise_mean),lty=2, col="black")
mtext(side=1,at=list3, line=0.65, cex=1,
      text = c("-25","-15","-5", "+5", "+15", "+25"))
dev.off()

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Species_temporal_distribution - Near and moderate only----------------------------
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
civil_dawn_2015 <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
data2 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Scarlet Honeyeater.csv", header = T)
all_data <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/all_data_added_protected.csv", header = T)[,c(1:21,37)]

cbPalette <- c("#000000","#999999", "#56B4E9", 
               "#D55E00", "#0072B2", 
               "#CC79A7","#009E73","#E69F00")
pch <- c(15,1,17,0,19,2,3,4,5,6,7,8,9,10)
pch1 <- pch[2]
pch2 <- pch[3]
pch3 <- pch[1]
colour1 <- cbPalette[2]
colour2 <- cbPalette[4]
colour3 <- cbPalette[1]
lwd1 <- 2
lwd2 <- 2
lwd3 <- 2
lty1 <- 1
lty2 <- 1
lty3 <- 1
pch <- 16
sunrise_mean <- 24.642
# species plot -----------------------------------
dev.off()
tiff("Species_temporal_distribution_Near_Mod.tiff", 
     height=2244, width=1713, res=300)
list2 <- -55:35
list3 <- c(-25,-15,-5,5,15,25)

layout(matrix(c(1,1,1,1,
                8,
                2,2,2,2,
                9,
                3,3,3,3,
                10,
                4,4,4,4,
                11,
                5,5,5,5,
                12,
                6,6,6,6,
                13,
                7,7,7,7), nrow = 34, ncol = 1, byrow = TRUE))
layout.show(34)
## show the regions that have been allocated to each plot
par(mar=c(0, 1.3, 0, 0.6), oma = c(4, 4, 4.2, 0), 
    cex.axis = 1.8, cex = 0.45, tcl=-0.4)

# Eastern Yellow Robin----------------------------------
label_name <- "Eastern Yellow Robin"
list <- c("EYR Far", "EYR Mod", "EYR Near")
#list1 <- c("EYR Quiet", "EYR Mod", "EYR Loud")

kalscpe_data <- NULL
kalscpe_data_EYR <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_EYR <- rbind(kalscpe_data_EYR, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_EYR)) {
  dat <- paste(substr(kalscpe_data_EYR$IN.FILE[i],1,4), 
               substr(kalscpe_data_EYR$IN.FILE[i],5,6),
               substr(kalscpe_data_EYR$IN.FILE[i],7,8), sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_EYR$min[i] <- round(floor(kalscpe_data_EYR$OFFSET[i]/60), 0)
  kalscpe_data_EYR$ref_civ[i] <- kalscpe_data_EYR$min[i] - civ_dawn
}

# EYR Near and Mod
ylim <- c(0,500)
a <- which(kalscpe_data_EYR$V23=="EYR Near")
b <- which(kalscpe_data_EYR$V23=="EYR Mod")
a <- c(a,b)

kalscpe_data_EYR_temp_far <- kalscpe_data_EYR[a, ]
counts_EYR_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EYR_temp_far$ref_civ==i)
  counts_EYR_Far <- c(counts_EYR_Far, length(a))
}
x <- 1:length(list2)
y <- counts_EYR_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_EYR_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n",  yaxt="n", col=colour3, las=2, 
     pch = pch, lwd = lwd1, cex=0.8)
mtext(side=2, cex=1.2, line=2.2, outer = T,
      "Total number of calls over 56 days")
mtext(side = 3, "a.", cex = 1, adj = 0.005, outer = F,
      line = 0)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd1)
axis(at=c(100,200,300,400), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(100,200,300,400), lwd=0.001, lty=2)
abline(v=list3)
mtext("Eastern Yellow Robin (EYR)", 
      line=0.1, cex=1)

mtext("Calling rates in relation to civil dawn", 
      line=1.9, cex=1.4)
text(x = -20, y = 0.97*ylim[2]-20, "Pr-C-D", cex = 1.8)
text(x =   0, y = 0.97*ylim[2]-20, "C-D", cex = 1.8)
text(x =  20, y = 0.97*ylim[2]-20, "Po-C-D", cex = 1.8)
abline(v=c(0,sunrise_mean),lty=2, col="black")

# White-throated Honeyeater-------------------------
label_name <- "White-throated Honeyeater"
list <- c("WTH Far", "WTH Mod", "WTH Near")
list1 <- c("WTH Quiet", "WTH Mod", "WTH Loud")

kalscpe_data <- NULL
kalscpe_data_WTH <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_WTH <- rbind(kalscpe_data_WTH, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_WTH)) {
  dat <- paste(substr(kalscpe_data_WTH$IN.FILE[i],1,4), 
               substr(kalscpe_data_WTH$IN.FILE[i],5,6),
               substr(kalscpe_data_WTH$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_WTH$min[i] <- round(floor(kalscpe_data_WTH$OFFSET[i]/60), 0)
  kalscpe_data_WTH$ref_civ[i] <- kalscpe_data_WTH$min[i] - civ_dawn
}

# WTH Far
ylim <- c(0,130)
a <- which(kalscpe_data_WTH$V23=="WTH Near")
b <- which(kalscpe_data_WTH$V23=="WTH Mod")
a <- c(a,b)
kalscpe_data_WTH_temp_far <- kalscpe_data_WTH[a, ]
counts_WTH_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTH_temp_far$ref_civ==i)
  counts_WTH_Far <- c(counts_WTH_Far, length(a))
}
x <- 1:length(list2)
y <- counts_WTH_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTH_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, lwd = lwd1, cex=0.8)
mtext(side = 3, "b.", cex = 1, adj = 0.005, outer = F,
      line = 0)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd1, lty=lty1)
abline(v=list3)
axis(at=c(40,80,120,160), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(20,40,60, 80,100,120,140,160), lwd=0.001, lty=2)
mtext("White-throated Honeyeater (WTH)", 
      line=0.1, cex=1)
abline(v=c(0,sunrise_mean),lty=2, col="black")

# Laughing Kookaburra----------------------------
label_name <- "Laughing Kookaburra"
list <- c("Kookaburra Quiet","Kookaburra Cackle", "Kookaburra Mod", "Kookaburra Loud")
#list1 <- c("KOOK Quiet","KOOK Mod", "KOOK Loud")

kalscpe_data <- NULL
kalscpe_data_KOOK <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_KOOK <- rbind(kalscpe_data_KOOK, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_KOOK)) {
  dat <- paste(substr(kalscpe_data_KOOK$IN.FILE[i],1,4), 
               substr(kalscpe_data_KOOK$IN.FILE[i],5,6),
               substr(kalscpe_data_KOOK$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_KOOK$min[i] <- round(floor(kalscpe_data_KOOK$OFFSET[i]/60), 0)
  kalscpe_data_KOOK$ref_civ[i] <- kalscpe_data_KOOK$min[i] - civ_dawn
}

# KOOK trill Far
ylim <- c(0,11)
a <- which(kalscpe_data_KOOK$V23=="Kookaburra Loud")
b <- which(kalscpe_data_KOOK$V23=="Kookaburra Mod")
a <- c(a,b)
kalscpe_data_KOOK_temp_far <- kalscpe_data_KOOK[a, ]
counts_KOOK_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_KOOK_temp_far$ref_civ==i)
  counts_KOOK_Far <- c(counts_KOOK_Far, length(a))
}
x <- 1:length(list2)
y <- counts_KOOK_Far
lo <- loess(y~x , span=0.08)
plot(list2, counts_KOOK_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "c.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("Laughing Kookaburra (LKB)", 
      line=0.1, cex=1)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd1, lty=lty1)
abline(v=list3, col="black")
axis(at=c(5,10,20,25,30,40), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(5,10,20,25,30,40), lwd=0.001, lty=2)
abline(v=c(0,sunrise_mean),lty=2, col="black")
par(new=T)
a <- which(kalscpe_data_KOOK$V23=="Kookaburra Quiet")
kalscpe_data_KOOK_temp_far <- kalscpe_data_KOOK[a, ]
counts_KOOK_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_KOOK_temp_far$ref_civ==i)
  counts_KOOK_Far <- c(counts_KOOK_Far, length(a))
}
x <- 1:length(list2)
y <- counts_KOOK_Far
lo <- loess(y~x , span=0.08)
plot(list2, counts_KOOK_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour1, las=1, 
     pch = pch, cex=0.8)
lines(list2, predict(lo), col=colour1, 
      lwd=lwd1, lty=lty1)
label <- c("Far", "Mod and Near")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour1, colour3),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = c(pch, pch),
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(lwd1,lwd2,lwd3),
       lty=c(1,1), pt.cex = c(1, 1), pt.lwd = 0.8,
       seg.len=4)



# Scarlet Honeyeater 2-------------------
label_name <- "Scarlet Honeyeater SC1"
list <- c("SC3 Chatter Far", "SC3 Chatter Mod", "SC3 Chatter Near")
#list1 <- c("SC3 Chatter Quiet", "SC3 Chatter Mod", "SC3 Chatter Loud")
#colours <- c("grey70", "grey50", "grey0","grey20")
kalscpe_data <- NULL
kalscpe_data_SC2 <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_SC2 <- rbind(kalscpe_data_SC2, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_SC2)) {
  dat <- paste(substr(kalscpe_data_SC2$IN.FILE[i],1,4), 
               substr(kalscpe_data_SC2$IN.FILE[i],5,6),
               substr(kalscpe_data_SC2$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_SC2$min[i] <- round(floor(kalscpe_data_SC2$OFFSET[i]/60), 0)
  kalscpe_data_SC2$ref_civ[i] <- kalscpe_data_SC2$min[i] - civ_dawn
}

# SC2 Far
ylim <- c(0,50)
a <- which(kalscpe_data_SC2$V23=="SC3 Chatter Mod")
b <- which(kalscpe_data_SC2$V23=="SC3 Chatter Near")
a <- c(a,b)
kalscpe_data_SC2_temp_far <- kalscpe_data_SC2[a, ]
counts_SC2_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC2_temp_far$ref_civ==i)
  counts_SC2_Far <- c(counts_SC2_Far, length(a))
}
x <- 1:length(list2)
y <- counts_SC2_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC2_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "e.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("Scarlet Honeyeater (SH1)", 
      line=0.1, cex=1)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd1, lty=lty1)
abline(v=list3, col="black")
axis(at=c(20,40), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(20,40), lwd=0.001, lty=2)
abline(v=c(0,sunrise_mean),lty=2, col="black")

# Scarlet Honeyeater 2--------------------------------------
label_name <- "Scarlet Honeyeater SC2"
list <- c("SC1 Far", "SC1 Mod", "SC1 Near")
#list1 <- c("SC1 Quiet", "SC1 Mod", "SC1 Loud")
kalscpe_data <- NULL
kalscpe_data_SC1 <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_SC1 <- rbind(kalscpe_data_SC1, kalscpe_data)
}

for(i in 1:nrow(kalscpe_data_SC1)) {
  dat <- paste(substr(kalscpe_data_SC1$IN.FILE[i],1,4), 
               substr(kalscpe_data_SC1$IN.FILE[i],5,6),
               substr(kalscpe_data_SC1$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_SC1$min[i] <- round(floor(kalscpe_data_SC1$OFFSET[i]/60), 0)
  kalscpe_data_SC1$ref_civ[i] <- kalscpe_data_SC1$min[i] - civ_dawn
}

# SC1 Far
ylim <- c(0, 72)
a <- which(kalscpe_data_SC1$V23=="SC1 Mod")
b <- which(kalscpe_data_SC1$V23=="SC1 Near")
a <- c(a,b)
kalscpe_data_SC1_temp_far <- kalscpe_data_SC1[a, ]
counts_SC1_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC1_temp_far$ref_civ==i)
  counts_SC1_Far <- c(counts_SC1_Far, length(a))
}
x <- 1:length(list2)
y <- counts_SC1_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC1_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "d.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("Scarlet Honeyeater (SH2)", 
      line=0.1, cex=1)

lines(list2, predict(lo), col=colour3, 
      lwd=lwd1, lty=lty1)
abline(v=list3, col="black")
axis(at=c(20,40,60), side=2, las=1,  
     cex.axis=2, hadj = 0.75)
abline(h=c(20,40, 60), lwd=0.001, lty=2)

abline(v=c(0,sunrise_mean),lty=2, col="black")

# White-throated Treecreeper-------------------------------
label_name <- "White-throated Treecreeper"
list <- c("WTT trill Far", "WTT trill Mod", "WTT trill Near")
#list1 <- c("WTT trill Quiet", "WTT trill Mod", "WTT trill Loud")

kalscpe_data <- NULL
kalscpe_data_WTT <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_WTT <- rbind(kalscpe_data_WTT, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_WTT)) {
  dat <- paste(substr(kalscpe_data_WTT$IN.FILE[i],1,4), 
               substr(kalscpe_data_WTT$IN.FILE[i],5,6),
               substr(kalscpe_data_WTT$IN.FILE[i],7,8), sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_WTT$min[i] <- round(floor(kalscpe_data_WTT$OFFSET[i]/60), 0)
  kalscpe_data_WTT$ref_civ[i] <- kalscpe_data_WTT$min[i] - civ_dawn
}

# WTT trill Near and mod
ylim <- c(0,20)
counts_WTT_Far <- NULL
a <- which(kalscpe_data_WTT$V23=="WTT trill Mod")
a <- which(kalscpe_data_WTT$V23=="WTT trill Near")
a <- c(a,b)
kalscpe_data_WTT_temp_far <- kalscpe_data_WTT[a, ]
counts_WTT_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTT_temp_far$ref_civ==i)
  counts_WTT_Far <- c(counts_WTT_Far, length(a))
}
x <- list2
y <- counts_WTT_Far
lo <- loess(y~x, span=0.09)
plot(list2, counts_WTT_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "f.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("White-throated Treecreeper (WTT)", 
      line=0.1, cex=1)
axis(at=c(10,20), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(10,20), lwd=0.001, lty=2)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd1, lty=lty1)
abline(v=list3, col="black")
abline(v=c(0,sunrise_mean),lty=2, col="black")

# Eastern Whipbird---------------------------------
label_name <- "Eastern Whipbird"
list <- c("EW Far", "EW Mod", "EW Near")
list1 <- c("EW Quiet", "EW Mod", "EW Loud")

kalscpe_data <- NULL
kalscpe_data_EW <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_EW <- rbind(kalscpe_data_EW, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_EW)) {
  dat <- paste(substr(kalscpe_data_EW$IN.FILE[i],1,4), 
               substr(kalscpe_data_EW$IN.FILE[i],5,6),
               substr(kalscpe_data_EW$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_EW$min[i] <- round(floor(kalscpe_data_EW$OFFSET[i]/60), 0)
  kalscpe_data_EW$ref_civ[i] <- kalscpe_data_EW$min[i] - civ_dawn
}

# EW Far
ylim <- c(0,115)
a <- which(kalscpe_data_EW$V23=="EW Mod")
b <- which(kalscpe_data_EW$V23=="EW Near")
a <- c(a,b)
kalscpe_data_EW_temp_far <- kalscpe_data_EW[a, ]
counts_EW_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EW_temp_far$ref_civ==i)
  counts_EW_Far <- c(counts_EW_Far, length(a))
}
x <- 1:length(list2)
y <- counts_EW_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_EW_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col=colour3, las=1, 
     pch = pch, cex=0.8)
mtext(side = 3, "g.", cex = 1, adj = 0.005, outer = F,
      line = 0)
mtext("Eastern Whipbird (EWB)", 
      line=0.1, cex=1)
mtext(side=1, line = 2.6, "Minutes from Civil Dawn", cex=1.2)
lines(list2, predict(lo), col=colour3, 
      lwd=lwd1, lty=lty1)
abline(v=list3)
axis(at=c(40,80, 120), side=2, las=1, 
     cex.axis=2, hadj = 0.75)
abline(h=c(20,40,60, 80, 100, 120), lwd=0.001, lty=2)
abline(v=c(0,sunrise_mean),lty=2, col="black")
mtext(side=1,at=list3, line=0.65, cex=1,
      text = c("-25","-15","-5", "+5", "+15", "+25"))
dev.off()