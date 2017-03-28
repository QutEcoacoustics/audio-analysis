# Author:  Yvonne Phillips
# Date: December 2016
# Description:

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# association plot of rain and insect clusters------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
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

# Plot an empty plot with no axes or frame
png(paste("plots\\gympie_Insect",toString(insect_clusters),"_rain",
          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
    height = 675, width = 1500)
par(mar=c(1,3,3,0))
# space between sets not columns
gap <- 10
# width and spacing of columns
width <- 2
space <- 0.8

plot(x = c(0,((width+space)*(length(n)-1))), type = "n",
     y = c(-(max_x+12),(max_y+12)),
     frame.plot = FALSE, axes = FALSE) 
ref <- 0
for(i in 1:length(gym_x)) {
  rect(ref, -gap, ref+width, -(gym_x[i]+gap), col = rain_col)
  ref <- ref + (width+space)
}
ref <- 0
for(i in 1:length(gym_x)) {
  rect(ref, gap, ref+width, gym_y[i]+gap, col = insect_col)
  ref <- ref + (width+space)
}
axis(side = 2, at = (seq(0, max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), cex.axis= 2.4, 
     line = -3.2)
axis(side = 2, at = -(seq(0,max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), cex.axis= 2.4, 
     line = -3.2)
date.ref <- a[which(a > min(n))]
for(i in 1:length(date.ref)) {
  text(x = ((date.ref[i]-min(n))*(width+space)-1), 
       y = (max(rowSums(y)) - 10),  
       paste(date.list[date.ref[i]]),
       cex = 2.2, pos = 4)
}
abline(v=((a-min(n))*(width+space)))
par(font=2)
mtext(side = 3, paste("GympieNP - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Insect clusters (", 
                      toString(insect_clusters),")"),
      outer = F, cex = 3)
par(font=1)
mtext(side = 2, "Minutes per day", 
      cex = 2.4, line = 0.9)
dev.off()

# Perform cross correlation on Gympie data
ylim <- c(-0.2, 0.58)
png(paste("plots\\cross-corr_gympie_insects",toString(insect_clusters),"_rain",
          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
    height = 450, width = 600)

par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
    cex.axis =1.2)
ccf(gym_y, gym_x, main = "",
    xlab = "", ylab = "", ylim = ylim)
mtext(side = 3, line = 1, paste("Gympie", 
                                dates[min(n)], "to", dates[max(n)]), cex = 1.25)
mtext(side = 3, line = 0, paste("clusters",
                                toString(rain_clusters), "and", 
                                toString(insect_clusters)))
mtext(side = 3, line = -1, paste("n = ", length(n), sep = ""))
mtext(side = 1, "Lag", line = 2.8, cex = 1.2)
mtext(side = 2, "cross-correlation", line = 2.8, cex = 1.2)
dev.off()

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
png(paste("plots\\woondum_Insect",toString(insect_clusters),"_rain",
          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
    height = 675, width = 1500)
par(mar=c(1,3,3,0))
# space between sets not columns
gap <- 10
# width and spacing of columns
width <- 2
space <- 0.8

plot(x = c(0,((width+space)*(length(n)-1))), type = "n",
     y = c(-(max_x+12),(max_y+12)),
     frame.plot = FALSE, axes = FALSE) 
ref <- 0
for(i in 1:length(woon_x)) {
  rect(ref, -gap, ref+width, -(woon_x[i]+gap), col = rain_col)
  ref <- ref + (width+space)
}
ref <- 0
for(i in 1:length(woon_x)) {
  rect(ref, gap, ref+width, woon_y[i]+gap, col = insect_col)
  ref <- ref + (width+space)
}
axis(side = 2, at = (seq(0, max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), cex.axis= 2.4, 
     line = -3.2)
axis(side = 2, at = -(seq(0,max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), cex.axis= 2.4, 
     line = -3.2)
date.ref <- a[which(a > min(n))]
for(i in 1:length(date.ref)) {
  text(x = ((date.ref[i]-min(n))*(width+space)-1), 
       y = (max(rowSums(y)) - 10),  
       paste(date.list[date.ref[i]]),
       cex = 2.2, pos = 4)
}
abline(v=((a-min(n))*(width+space)))
par(font=2)
mtext(side = 3, paste("WoondumNP - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Insect clusters (", 
                      toString(insect_clusters),")"),
      outer = F, cex = 3)
par(font=1)
mtext(side = 2, "Minutes per day", 
      cex = 2.4, line = 0.9)
dev.off()

# Perform cross correlation on both the Woondum data
png(paste("plots\\cross-corr_woondum_insects",toString(insect_clusters),"_rain",
          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
    height = 450, width = 600)
par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
    cex.axis = 1.2)
ccf(woon_y, woon_x, main = "",
    xlab = "", ylab = "", ylim = ylim)
mtext(side = 3, line = 1, paste("Woondum", 
                                dates[min(n)], "to", dates[max(n)]), cex = 1.25)
mtext(side = 3, line = 0, paste("clusters",
                                toString(rain_clusters), "and", 
                                toString(insect_clusters)))
mtext(side = 1, "Lag", line = 2.8, cex = 1.2)
mtext(side = 2, "cross-correlation (CI = 95%)", line = 2.8, cex = 1.2)
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# association plot of rain and bird clusters------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
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
b <- max(n)- a + 1

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# choose the day numbers
n <- 1:128
# choose the rain clusters
rain_clusters <- c(10, 18, 59)
# choose the bird clusters
bird_clusters <- c(43)

# cluster 10 is light to moderate rain
# cluster 18 is moderate rain
# cluster 21 is light rain
# cluster 59 is heavy rain
x <- cbind(gym_matrix[n, rain_clusters])

# cluster 37, 43, 11
y <- cbind(gym_matrix[n, bird_clusters])

cor(rowSums(x), rowSums(y))
gym_x <- rowSums(x)
gym_y <- rowSums(y)

max_x <- max(gym_x)
max_y <- max(gym_y)

# Plot an empty plot with no axes or frame
png(paste("plots\\gympie_birds",toString(bird_clusters),"_rain",
          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
    height = 675, width = 1500)
par(mar=c(1,3,3,0))
# space between sets not columns
gap <- 10
# width and spacing of columns
width <- 2
space <- 0.8

plot(x = c(0,((width+space)*(length(n)-1))), type = "n",
     y = c(-(max_x+12),(max_y+12)),
     frame.plot = FALSE, axes = FALSE) 
ref <- 0
for(i in 1:length(gym_x)) {
  rect(ref, -gap, ref+width, -(gym_x[i]+gap), col = rain_col)
  ref <- ref + (width+space)
}
ref <- 0
for(i in 1:length(gym_x)) {
  rect(ref, gap, ref+width, gym_y[i]+gap, col = bird_col)
  ref <- ref + (width+space)
}
axis(side = 2, at = (seq(0, max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), cex.axis= 2.4, 
     line = -3.2)
axis(side = 2, at = -(seq(0,max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), cex.axis= 2.4, 
     line = -3.2)
date.ref <- a[which(a > min(n))]
for(i in 1:length(date.ref)) {
  text(x = ((date.ref[i]-min(n))*(width+space)-1), 
       y = (max(rowSums(y)) - 10),  
       paste(date.list[date.ref[i]]),
       cex = 2.2, pos = 4)
}
abline(v=(a-(min(n))*(width+space)))
par(font=2)
mtext(side = 3, paste("GympieNP - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Bird clusters (", 
                      toString(bird_clusters),")"),
      outer = F, cex = 3)
par(font=1)
mtext(side = 2, "Minutes per day", 
      cex = 2.4, line = 0.9)
dev.off()

# Perform cross correlation on Gympie data
ylim <- c(-0.2, 0.58)
png(paste("plots\\cross-corr_gympie_birds",toString(bird_clusters),"_rain",
          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
    height = 450, width = 600)
par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
    cex.axis =1.2)
ccf(gym_y, gym_x, main = "",
    xlab = "", ylab = "", ylim = ylim)
mtext(side = 3, line = 1, paste("Gympie", 
                                dates[min(n)], "to", dates[max(n)]), cex = 1.25)
mtext(side = 3, line = 0, paste("clusters",
                                toString(rain_clusters), "and", 
                                toString(bird_clusters)))
mtext(side = 1, "Lag", line = 2.8, cex = 1.2)
mtext(side = 2, "cross-correlation (CI = 95%)", line = 2.8, cex = 1.2)
dev.off()

# Woondum
# choose the rain clusters
x <- cbind(woon_matrix[n, rain_clusters])
y <- cbind(woon_matrix[n, bird_clusters])

cor(rowSums(x), rowSums(y))
woon_x <- rowSums(x)
woon_y <- rowSums(y)

max_x <- max(woon_x)
max_y <- max(woon_y)

# Plot an empty plot with no axes or frame
png(paste("plots\\woondum_birds",toString(bird_clusters),"_rain",
          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
    height = 675, width = 1500)
par(mar=c(1,3,3,0))
# space between sets not columns
gap <- 10
# width and spacing of columns
width <- 2
space <- 0.8

plot(x = c(0,((width+space)*(length(n)-1))), type = "n",
     y = c(-(max_x+12),(max_y+12)),
     frame.plot = FALSE, axes = FALSE) 
ref <- 0
for(i in 1:length(woon_x)) {
  rect(ref, -gap, ref+width, -(woon_x[i]+gap), col = rain_col)
  ref <- ref + (width+space)
}
ref <- 0
for(i in 1:length(woon_x)) {
  rect(ref, gap, ref+width, woon_y[i]+gap, col = bird_col)
  ref <- ref + (width+space)
}
axis(side = 2, at = (seq(0, max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), cex.axis= 2.4, 
     line = -3.2)
axis(side = 2, at = -(seq(0,max(rowSums(x)),50)+gap), 
     seq(0, max(rowSums(x)), 50), cex.axis= 2.4, 
     line = -3.2)
for(i in 1:length(a)) {
  text(x = (a[i]*(width+space)-1), 
       y = (max(rowSums(y)) - 10),  
       paste(date.list[a[i]]),
       cex = 2.2, pos = 4) 
}
abline(v=((a-1)*(width+space)))
par(font=2)
mtext(side = 3, paste("WoondumNP - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Bird clusters (", 
                      toString(bird_clusters),")"),
      outer = F, cex = 3)
par(font=1)
mtext(side = 2, "Minutes per day", 
      cex = 2.4, line = 0.9)
dev.off()

# Perform cross correlation on both the Woondum data
png(paste("plots\\cross-corr_woondum_birds",toString(bird_clusters),"_rain",
  toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
  height = 450, width = 600)
par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
    cex.axis =1.2)
ccf(woon_y, woon_x, main = "",
    xlab = "", ylab = "", ylim = ylim)
mtext(side = 3, line = 1, paste("Woondum", 
                                dates[min(n)], "to", dates[max(n)]), cex = 1.25)
mtext(side = 3, line = 0, paste("clusters",
                                toString(rain_clusters), "and", 
                                toString(bird_clusters)))
mtext(side = 1, "Lag", line = 2.8, cex = 1.2)
mtext(side = 2, "cross-correlation", line = 2.8, cex = 1.2)
dev.off()
