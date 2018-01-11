#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Rain and insect correlation ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# choose the day numbers
n <- 102:193

moon_phases <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/all_ecosound_links_Gym_final.csv", header=T)[,c(19)]
unique(moon_phases)
table(moon_phases)
a <- which(moon_phases=="FirstQuarter")
b <- which(moon_phases=="FullMoon")
c <- which(moon_phases=="LastQuarter")
d <- which(moon_phases=="NewMoon")

e <- sort(c(a,b,c,d))
f <- moon_phases[e]
for(i in 1:(length(e)-1)) {
  av <- floor(mean(c(e[i], e[i+1])))
  moon_phases[e[i]:av] <- f[i]
  moon_phases[av:e[i+1]] <- f[i+1]
}
unique(moon_phases)
table(moon_phases)
moon_phases[1:e[1]] <- f[i]
length(moon_phases) - e[length(e)]
table(moon_phases)

if(length(e[length(e)]:length(moon_phases)) <  ((e[2]-e[1])/2)) {
  moon_phases[e[length(e)]:length(moon_phases)] <- f[length(e)]
}
unique(moon_phases)
table(moon_phases)

# read file containing summary of each 30 minute segments
df <- read.csv("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/polarHistograms/polar_data.csv", header = T)

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
gym_matrix <- data.frame(gym_matrix)
woon_matrix <- data.frame(woon_matrix)

moon_phase2 <- NULL
for(i in 1:398) {
  a <- which(moon_phases[(((i-1)*1440)+1):(i*1440)]=="FirstQuarter")
  b <- which(moon_phases[(((i-1)*1440)+1):(i*1440)]=="FullMoon")
  c <- which(moon_phases[(((i-1)*1440)+1):(i*1440)]=="LastQuarter")
  d <- which(moon_phases[(((i-1)*1440)+1):(i*1440)]=="NewMoon")
  
  if((length(a)>length(b)) & (length(a)>length(c)) & (length(a)>length(d))) {
    moon_phase2 <- c(moon_phase2, "FirstQuarter")
  }
  if((length(b)>length(a)) & (length(b)>length(c)) & (length(b)>length(d))) {
    moon_phase2 <- c(moon_phase2, "FullMoon")
  }
  if(length(c)>length(a) & length(c)>length(b) & length(c)>length(d)) {
    moon_phase2 <- c(moon_phase2, "LastQuarter")
  }
  if(length(d)>length(a) & length(d)>length(b) & length(d)>length(c)) {
    moon_phase2 <- c(moon_phase2, "NewMoon")
  }
}

gym_matrix$moon_phase <- moon_phase2
woon_matrix$moon_phase <- moon_phase2

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

month1 <- substr(dates[n[1]], 6,7)
if(month1=="01"){
  month1 <- " January "
}
if(month1=="02"){
  month1 <- " February "
}
if(month1=="03"){
  month1 <- " March "
}
if(month1=="04"){
  month1 <- " April "
}
if(month1=="05"){
  month1 <- " May "
}
if(month1=="06"){
  month1 <- " June "
}
if(month1=="07"){
  month1 <- " July "
}
if(month1=="08"){
  month1 <- " August "
}
if(month1=="09"){
  month1 <- " September "
}
if(month1=="10"){
  month1 <- " October "
}
if(month1=="11"){
  month1 <- " November "
}
if(month1=="12"){
  month1 <- " December "
}

month2 <- substr(dates[max(n)], 6,7)
if(month2=="01"){
  month2 <- " January "
}
if(month2=="02"){
  month2 <- " February "
}
if(month2=="03"){
  month2 <- " March "
}
if(month2=="04"){
  month2 <- " April "
}
if(month2=="05"){
  month2 <- " May "
}
if(month2=="06"){
  month2 <- " June "
}
if(month2=="07"){
  month2 <- " July "
}
if(month2=="08"){
  month2 <- " August "
}
if(month2=="09"){
  month2 <- " September "
}
if(month2=="10"){
  month2 <- " October "
}
if(month2=="11"){
  month2 <- " November "
}
if(month2=="12"){
  month2 <- " December "
}

# choose the rain clusters
#clusters 10, 18, 21 and 59 for the Gympie National Park site and an addition of cluster 54 for the Woondum National Park site
rain_clusters <- c(10, 18, 21, 59)

rain_clusters <- c(10, 18, 21, 59)
x <- cbind(gym_matrix[n, rain_clusters])
#clusters 1, 22 26 and 27, 29 for the Gympie National Park site and 

# choose the insect clusters
insect_clusters <- c(1,22,26,27,29)
y <- cbind(gym_matrix[n,insect_clusters])

cor(rowSums(x), rowSums(y))
gym_x <- rowSums(x)
gym_y <- rowSums(y)

max_x <- max(gym_x)
max_y <- max(gym_y)

site <- "Gympie"
if(site=="Gympie") {
  woon_x <- gym_x
  woon_y <- gym_y 
}
#par(mar=c(1,3,3,0))
tiff(paste("C:/Work2/Thesis/Images and plots/Cross_correlation_insect",site,
           "_",substr(dates[n[1]],9,10), 
           month1, substr(dates[n[1]],1,4), "to",
           substr(dates[n[length(n)]],9,10), month2, 
           substr(dates[n[length(n)]],1,4),".tiff",sep=""), 
     width = 1713, height = 1170, units = 'px', res = 300)

par(mar=c(0.1, 3.4, 2.71, 0),  #, mfcol=c(2,1) ,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(2,2,2),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3))
layout(m)
layout.show(3)

# space between sets not columns
gap <- 20
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
if(max(rowSums(y)) >= 300){
  axis(side = 2, at = (seq(0, max(rowSums(y)),100)+gap), 
       seq(0, max(rowSums(y)), 100), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(y)) < 300){
  axis(side = 2, at = (seq(0, max(rowSums(y)),50)+gap), 
       seq(0, max(rowSums(y)), 50), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(x)) >= 300) {
  axis(side = 2, at = -(seq(0, max(rowSums(x)),100)+gap), 
       seq(0, max(rowSums(x)), 100), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(x)) < 300) {
  axis(side = 2, at = -(seq(0, max(rowSums(x)),50)+gap), 
       seq(0, max(rowSums(x)), 50), line = -1.4, cex=1.8, las=1)
}

date.ref <- a[which(a >= min(n))]
date.ref <- date.ref[1:3]

for(i in 1:length(date.ref)) {
  text(x = ((date.ref[i]-min(n))*(width+space)-0.4), 
       y = (max(rowSums(y)) - 10), cex= 1,  
       paste(date.list[date.ref[i]]), pos = 4)
}
abline(v=((a-min(n))*(width+space)))
#par(font=2, mar=c(2, 3, 3, 0), mfcol=c(2,1),
#            cex = 0.6, cex.axis = 1, cex.main = 1)
mtext(side = 3, paste(site, " - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Orthoptera clusters (", 
                      toString(insect_clusters),")"),
      outer = F, cex = 0.8, line=1.8)
mtext(side=3, paste("Days from ", substr(dates[n[1]],9,10), 
                    month1, substr(dates[n[1]],1,4), "to",
                    substr(dates[n[length(n)]],9,10), month2, 
                    substr(dates[n[length(n)]],1,4)),
      line=0.8, cex=0.6)

#par(font=1)
mtext(side = 2, "Minutes per day", line = 1.2, cex=0.8)
mtext(side = 3, "a.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -1.8)
# Perform cross correlation on both the Woondum data
#png(paste("plots\\cross-corr_woondum_insects",toString(insect_clusters),"_rain",
#          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
#    height = 450, width = 600)
#par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
#    cex.axis = 1.2)
# empty plot to fill the plot 2 space
plot(c(0, 1440), c(1440, 0), 
     type = "n",axes=FALSE, frame.plot=FALSE,
     xlab="", ylab="")

ylim <- c(-0.2, 0.58)
par(mar=c(3.8, 4.8, 0, 1), oma= c(0,0,0.5,0),
    cex = 0.6, cex.axis = 1, cex.main = 1)
par(new=T)
c <- ccf(woon_y, woon_x, main = "", bty = "n",
         xlab = "", ylab = "", ylim = ylim, plot = F)
plot(c[0:10], main = "", bty = "n", las=1,
     xlab = "", ylab = "", ylim=ylim)
mtext(side=3, cex=0.8,
      "Cross-correlation between rain and orthopthera acoustic states")
mtext(side = 1, "Lag (days)", line = 2.5, cex=0.8)
mtext(side = 2, "Cross-correlation", line = 2.5, cex=0.8)
mtext(side = 3, "b.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -18)
dev.off()


cor(rowSums(x), rowSums(y))


# Woondum
#clusters 10, 18, 21 and 59 for the Gympie National Park site and an addition of cluster 54 for the Woondum National Park site
rain_clusters <- c(10, 18, 21, 54, 59)
#clusters 1, 26, 27 and 29 for the Woondum National Park site.
insect_clusters <- c(1, 26, 27, 29)

month1 <- substr(dates[n[1]], 6,7)
if(month1=="01"){
  month1 <- " January "
}
if(month1=="02"){
  month1 <- " February "
}
if(month1=="03"){
  month1 <- " March "
}
if(month1=="04"){
  month1 <- " April "
}
if(month1=="05"){
  month1 <- " May "
}
if(month1=="06"){
  month1 <- " June "
}
if(month1=="07"){
  month1 <- " July "
}
if(month1=="08"){
  month1 <- " August "
}
if(month1=="09"){
  month1 <- " September "
}
if(month1=="10"){
  month1 <- " October "
}
if(month1=="11"){
  month1 <- " November "
}
if(month1=="12"){
  month1 <- " December "
}

month2 <- substr(dates[max(n)], 6,7)
if(month2=="01"){
  month2 <- " January "
}
if(month2=="02"){
  month2 <- " February "
}
if(month2=="03"){
  month2 <- " March "
}
if(month2=="04"){
  month2 <- " April "
}
if(month2=="05"){
  month2 <- " May "
}
if(month2=="06"){
  month2 <- " June "
}
if(month2=="07"){
  month2 <- " July "
}
if(month2=="08"){
  month2 <- " August "
}
if(month2=="09"){
  month2 <- " September "
}
if(month2=="10"){
  month2 <- " October "
}
if(month2=="11"){
  month2 <- " November "
}
if(month2=="12"){
  month2 <- " December "
}

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

for(i in 1:length(date.list)) {
  if(date.list[i]=="2015-07-01") {
    date.list[i] <- paste(substr(date.list[i],10,10),
                          "Jul",
                          substr(date.list[i],1,4), sep=" ")
  }
  if(date.list[i]=="2015-08-01") {
    date.list[i] <- paste(substr(date.list[i],10,10),
                          "Aug",
                          substr(date.list[i],1,4), sep=" ")
  }
  if(date.list[i]=="2015-09-01") {
    date.list[i] <- paste(substr(date.list[i],10,10),
                          "Sept",
                          substr(date.list[i],1,4), sep=" ")
  }
}

date.list
site <- "Woondum"
#par(mar=c(1,3,3,0))
tiff(paste("C:/Work2/Thesis/Images and plots/Cross_correlation_insect",site,
           "_",substr(dates[n[1]],9,10), 
           month1, substr(dates[n[1]],1,4), "to",
           substr(dates[n[length(n)]],9,10), month2, 
           substr(dates[n[length(n)]],1,4),".tiff",sep=""), 
     width = 1713, height = 1170, units = 'px', res = 300)

par(mar=c(0.1, 3.4, 2.71, 0),  #, mfcol=c(2,1) ,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(2,2,2),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3))
layout(m)
layout.show(3)

# space between sets not columns
gap <- 20
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
if(max(rowSums(y)) >= 300){
  axis(side = 2, at = (seq(0, max(rowSums(y)),100)+gap), 
       seq(0, max(rowSums(y)), 100), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(y)) < 300){
  axis(side = 2, at = (seq(0, max(rowSums(y)),50)+gap), 
       seq(0, max(rowSums(y)), 50), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(x)) >= 300) {
  axis(side = 2, at = -(seq(0, max(rowSums(x)),100)+gap), 
       seq(0, max(rowSums(x)), 100), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(x)) < 300) {
  axis(side = 2, at = -(seq(0, max(rowSums(x)),50)+gap), 
       seq(0, max(rowSums(x)), 50), line = -1.4, cex=1.8, las=1)
}

date.ref <- a[which(a >= min(n))]
date.ref <- date.ref[1:3]

for(i in 1:length(date.ref)) {
  text(x = ((date.ref[i]-min(n))*(width+space)-0.4), 
       y = (max(rowSums(y)) - 10), cex= 1,  
       paste(date.list[date.ref[i]]), pos = 4)
}
abline(v=((a-min(n))*(width+space)))
#par(font=2, mar=c(2, 3, 3, 0), mfcol=c(2,1),
#            cex = 0.6, cex.axis = 1, cex.main = 1)
mtext(side = 3, paste(site, " - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Orthoptera clusters (", 
                      toString(insect_clusters),")"),
      outer = F, cex = 0.8, line=1.8)
mtext(side=3, paste("Days from ", substr(dates[n[1]],9,10), 
                    month1, substr(dates[n[1]],1,4), "to",
                    substr(dates[n[length(n)]],9,10), month2, 
                    substr(dates[n[length(n)]],1,4)),
      line=0.8, cex=0.6)

#par(font=1)
mtext(side = 2, "Minutes per day", line = 1.2, cex=0.8)
mtext(side = 3, "a.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -1.8)
# Perform cross correlation on both the Woondum data
#png(paste("plots\\cross-corr_woondum_insects",toString(insect_clusters),"_rain",
#          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
#    height = 450, width = 600)
#par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
#    cex.axis = 1.2)
# empty plot to fill the plot 2 space
plot(c(0, 1440), c(1440, 0), 
     type = "n",axes=FALSE, frame.plot=FALSE,
     xlab="", ylab="")

ylim <- c(-0.2, 0.58)
par(mar=c(3.8, 4.8, 0, 1), oma= c(0,0,0.5,0),
    cex = 0.6, cex.axis = 1, cex.main = 1)
par(new=T)
c <- ccf(woon_y, woon_x, main = "", bty = "n",
         xlab = "", ylab = "", ylim = ylim, plot = F)
plot(c[0:10], main = "", bty = "n", las=1,
     xlab = "", ylab = "", ylim=ylim)
mtext(side=3, cex=0.8,
      "Cross-correlation between rain and orthopthera acoustic states")
mtext(side = 1, "Lag (days)", line = 2.5, cex=0.8)
mtext(side = 2, "Cross-correlation", line = 2.5, cex=0.8)
mtext(side = 3, "b.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -18)
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Rain and cicada correlation ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# choose the day numbers
n <- 163:254

# read file containing summary of 30 minute segments
df <- read.csv("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/polarHistograms/polar_data.csv", header = T)

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

month1 <- substr(dates[n[1]], 6,7)
if(month1=="01"){
  month1 <- " January "
}
if(month1=="02"){
  month1 <- " February "
}
if(month1=="03"){
  month1 <- " March "
}
if(month1=="04"){
  month1 <- " April "
}
if(month1=="05"){
  month1 <- " May "
}
if(month1=="06"){
  month1 <- " June "
}
if(month1=="07"){
  month1 <- " July "
}
if(month1=="08"){
  month1 <- " August "
}
if(month1=="09"){
  month1 <- " September "
}
if(month1=="10"){
  month1 <- " October "
}
if(month1=="11"){
  month1 <- " November "
}
if(month1=="12"){
  month1 <- " December "
}

month2 <- substr(dates[max(n)], 6,7)
if(month2=="01"){
  month2 <- " January "
}
if(month2=="02"){
  month2 <- " February "
}
if(month2=="03"){
  month2 <- " March "
}
if(month2=="04"){
  month2 <- " April "
}
if(month2=="05"){
  month2 <- " May "
}
if(month2=="06"){
  month2 <- " June "
}
if(month2=="07"){
  month2 <- " July "
}
if(month2=="08"){
  month2 <- " August "
}
if(month2=="09"){
  month2 <- " September "
}
if(month2=="10"){
  month2 <- " October "
}
if(month2=="11"){
  month2 <- " November "
}
if(month2=="12"){
  month2 <- " December "
}

# choose the rain clusters
#clusters 10, 18, 21 and 59 for the Gympie National Park site and an addition of cluster 54 for the Woondum National Park site
rain_clusters <- c(10, 18, 21, 59)
x <- cbind(gym_matrix[n, rain_clusters])
#clusters 1, 22 26 and 27, 29 for the Gympie National Park site and 

# choose the insect clusters
#cicada clusters were 12, 32, 34 and 48 for both sites 
cicada_clusters <- c(12,32,34,48,44)
#cicada_clusters <- 44
y <- cbind(gym_matrix[n,cicada_clusters])

cor(rowSums(x), rowSums(y))
gym_x <- rowSums(x)
gym_y <- rowSums(y)

max_x <- max(gym_x)
max_y <- max(gym_y)

site <- "Gympie"
if(site=="Gympie") {
  woon_x <- gym_x
  woon_y <- gym_y 
}
#par(mar=c(1,3,3,0))
tiff(paste("C:/Work2/Thesis/Images and plots/Cross_correlation_cicada",site,
           "_",substr(dates[n[1]],9,10), 
           month1, substr(dates[n[1]],1,4), "to",
           substr(dates[n[length(n)]],9,10), month2, 
           substr(dates[n[length(n)]],1,4),".tiff",sep=""), 
     width = 1713, height = 1170, units = 'px', res = 300)

par(mar=c(0.1, 3.4, 2.71, 0),  #, mfcol=c(2,1) ,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(2,2,2),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3))
layout(m)
layout.show(3)

# space between sets not columns
gap <- 20
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
  rect(ref, gap, ref+width, woon_y[i]+gap, col = cicada_col)
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
if(max(rowSums(y)) >= 300){
  axis(side = 2, at = (seq(0, max(rowSums(y)),100)+gap), 
       seq(0, max(rowSums(y)), 100), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(y)) < 300){
  axis(side = 2, at = (seq(0, max(rowSums(y)),50)+gap), 
       seq(0, max(rowSums(y)), 50), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(x)) >= 300) {
  axis(side = 2, at = -(seq(0, max(rowSums(x)),100)+gap), 
       seq(0, max(rowSums(x)), 100), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(x)) < 300) {
  axis(side = 2, at = -(seq(0, max(rowSums(x)),50)+gap), 
       seq(0, max(rowSums(x)), 50), line = -1.4, cex=1.8, las=1)
}

date.ref <- a[which(a >= min(n))]
date.ref <- date.ref[1:3]

for(i in 1:length(date.ref)) {
  text(x = ((date.ref[i]-min(n))*(width+space)-0.4), 
       y = (max(rowSums(y)) - 10), cex= 1,  
       paste(date.list[date.ref[i]]), pos = 4)
}
abline(v=((a-min(n))*(width+space)))
#par(font=2, mar=c(2, 3, 3, 0), mfcol=c(2,1),
#            cex = 0.6, cex.axis = 1, cex.main = 1)
mtext(side = 3, paste(site, " - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Cicada clusters (", 
                      toString(cicada_clusters),")"),
      outer = F, cex = 0.8, line=1.8)
mtext(side=3, paste("Days from ", substr(dates[n[1]],9,10), 
                    month1, substr(dates[n[1]],1,4), "to",
                    substr(dates[n[length(n)]],9,10), month2, 
                    substr(dates[n[length(n)]],1,4)),
      line=0.8, cex=0.6)

#par(font=1)
mtext(side = 2, "Minutes per day", line = 1.2, cex=0.8)
mtext(side = 3, "a.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -1.8)
# Perform cross correlation on both the Woondum data
#png(paste("plots\\cross-corr_woondum_insects",toString(cicada_clusters),"_rain",
#          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
#    height = 450, width = 600)
#par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
#    cex.axis = 1.2)
# empty plot to fill the plot 2 space
plot(c(0, 1440), c(1440, 0), 
     type = "n",axes=FALSE, frame.plot=FALSE,
     xlab="", ylab="")

ylim <- c(-0.2, 0.58)
par(mar=c(3.8, 4.8, 0, 1), oma= c(0,0,0.5,0),
    cex = 0.6, cex.axis = 1, cex.main = 1)
par(new=T)
c <- ccf(woon_y, woon_x, main = "", bty = "n",
         xlab = "", ylab = "", ylim = ylim, plot = F)
plot(c[0:10], main = "", bty = "n", las=1,
     xlab = "", ylab = "", ylim=ylim)
mtext(side=3, cex=0.8,
      "Cross-correlation between rain and cicada acoustic states")
mtext(side = 1, "Lag (days)", line = 2.5, cex=0.8)
mtext(side = 2, "Cross-correlation", line = 2.5, cex=0.8)
mtext(side = 3, "b.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -18)
dev.off()


cor(rowSums(x), rowSums(y))


# Woondum
#clusters 10, 18, 21 and 59 for the Gympie National Park site and an addition of cluster 54 for the Woondum National Park site
rain_clusters <- c(10, 18, 21, 54, 59)
#cicada clusters were 12, 32, 34 and 48 for both sites cicada_clusters <- c(1, 26, 27, 29)

month1 <- substr(dates[n[1]], 6,7)
if(month1=="01"){
  month1 <- " January "
}
if(month1=="02"){
  month1 <- " February "
}
if(month1=="03"){
  month1 <- " March "
}
if(month1=="04"){
  month1 <- " April "
}
if(month1=="05"){
  month1 <- " May "
}
if(month1=="06"){
  month1 <- " June "
}
if(month1=="07"){
  month1 <- " July "
}
if(month1=="08"){
  month1 <- " August "
}
if(month1=="09"){
  month1 <- " September "
}
if(month1=="10"){
  month1 <- " October "
}
if(month1=="11"){
  month1 <- " November "
}
if(month1=="12"){
  month1 <- " December "
}

month2 <- substr(dates[max(n)], 6,7)
if(month2=="01"){
  month2 <- " January "
}
if(month2=="02"){
  month2 <- " February "
}
if(month2=="03"){
  month2 <- " March "
}
if(month2=="04"){
  month2 <- " April "
}
if(month2=="05"){
  month2 <- " May "
}
if(month2=="06"){
  month2 <- " June "
}
if(month2=="07"){
  month2 <- " July "
}
if(month2=="08"){
  month2 <- " August "
}
if(month2=="09"){
  month2 <- " September "
}
if(month2=="10"){
  month2 <- " October "
}
if(month2=="11"){
  month2 <- " November "
}
if(month2=="12"){
  month2 <- " December "
}

# choose the rain clusters
x <- cbind(woon_matrix[n, rain_clusters])
y <- cbind(woon_matrix[n, cicada_clusters])

cor(rowSums(x), rowSums(y))
woon_x <- rowSums(x)
woon_y <- rowSums(y)

max_x <- max(woon_x)
max_y <- max(woon_y)

# Plot an empty plot with no axes or frame
#png(paste("plots\\woondum_Insect",toString(cicada_clusters),"_rain",
#          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
#    height = 675, width = 1500)
m <- rbind(c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(2,2,2),
           c(2,2,2))
layout(m)
layout.show(2)

for(i in 1:length(date.list)) {
  if(date.list[i]=="2015-07-01") {
    date.list[i] <- paste(substr(date.list[i],10,10),
                          "Jul",
                          substr(date.list[i],1,4), sep=" ")
  }
  if(date.list[i]=="2015-08-01") {
    date.list[i] <- paste(substr(date.list[i],10,10),
                          "Aug",
                          substr(date.list[i],1,4), sep=" ")
  }
  if(date.list[i]=="2015-09-01") {
    date.list[i] <- paste(substr(date.list[i],10,10),
                          "Sept",
                          substr(date.list[i],1,4), sep=" ")
  }
}

date.list
site <- "Woondum"
#par(mar=c(1,3,3,0))
tiff(paste("C:/Work2/Thesis/Images and plots/Cross_correlation_cicada",site,
           "_",substr(dates[n[1]],9,10), 
           month1, substr(dates[n[1]],1,4), "to",
           substr(dates[n[length(n)]],9,10), month2, 
           substr(dates[n[length(n)]],1,4),".tiff",sep=""), 
     width = 1713, height = 1170, units = 'px', res = 300)

par(mar=c(0.1, 3.4, 2.71, 0),  #, mfcol=c(2,1) ,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(1,1,1),
           c(2,2,2),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3),
           c(3,3,3))
layout(m)
layout.show(3)

# space between sets not columns
gap <- 20
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
  rect(ref, gap, ref+width, woon_y[i]+gap, col = cicada_col)
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
if(max(rowSums(y)) >= 300){
  axis(side = 2, at = (seq(0, max(rowSums(y)),100)+gap), 
       seq(0, max(rowSums(y)), 100), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(y)) < 300){
  axis(side = 2, at = (seq(0, max(rowSums(y)),50)+gap), 
       seq(0, max(rowSums(y)), 50), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(x)) >= 300) {
  axis(side = 2, at = -(seq(0, max(rowSums(x)),100)+gap), 
       seq(0, max(rowSums(x)), 100), line = -1.4, cex=1.8, las=1)
}
if(max(rowSums(x)) < 300) {
  axis(side = 2, at = -(seq(0, max(rowSums(x)),50)+gap), 
       seq(0, max(rowSums(x)), 50), line = -1.4, cex=1.8, las=1)
}

date.ref <- a[which(a >= min(n))]
date.ref <- date.ref[1:3]

for(i in 1:length(date.ref)) {
  text(x = ((date.ref[i]-min(n))*(width+space)-0.4), 
       y = (max(rowSums(y)) - 10), cex= 1,  
       paste(date.list[date.ref[i]]), pos = 4)
}
abline(v=((a-min(n))*(width+space)))
#par(font=2, mar=c(2, 3, 3, 0), mfcol=c(2,1),
#            cex = 0.6, cex.axis = 1, cex.main = 1)
mtext(side = 3, paste(site, " - Rain clusters (", 
                      toString(rain_clusters),
                      ") and Cicada clusters (", 
                      toString(cicada_clusters),")"),
      outer = F, cex = 0.8, line=1.8)
mtext(side=3, paste("Days from ", substr(dates[n[1]],9,10), 
                    month1, substr(dates[n[1]],1,4), "to",
                    substr(dates[n[length(n)]],9,10), month2, 
                    substr(dates[n[length(n)]],1,4)),
      line=0.8, cex=0.6)

#par(font=1)
mtext(side = 2, "Minutes per day", line = 1.2, cex=0.8)
mtext(side = 3, "a.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -1.8)
# Perform cross correlation on both the Woondum data
#png(paste("plots\\cross-corr_woondum_insects",toString(cicada_clusters),"_rain",
#          toString(rain_clusters), "_n_", min(n), "_", max(n), ".png", sep = ""), 
#    height = 450, width = 600)
#par(mar=c(3.8, 3.8, 0, 1), oma=c(0,0,2,0), cex = 1.2, 
#    cex.axis = 1.2)
# empty plot to fill the plot 2 space
plot(c(0, 1440), c(1440, 0), 
     type = "n",axes=FALSE, frame.plot=FALSE,
     xlab="", ylab="")

ylim <- c(-0.2, 0.58)
par(mar=c(3.8, 4.8, 0, 1), oma= c(0,0,0.5,0),
    cex = 0.6, cex.axis = 1, cex.main = 1)
par(new=T)
c <- ccf(woon_y, woon_x, main = "", bty = "n",
         xlab = "", ylab = "", ylim = ylim, plot = F)
plot(c[0:10], main = "", bty = "n", las=1,
     xlab = "", ylab = "", ylim=ylim)
mtext(side=3, cex=0.8,
      "Cross-correlation between rain and cicada acoustic states")
mtext(side = 1, "Lag (days)", line = 2.5, cex=0.8)
mtext(side = 2, "Cross-correlation", line = 2.5, cex=0.8)
mtext(side = 3, "b.", cex = 1.2, adj = 0.005, outer = TRUE,
      line = -18)
dev.off()

acf(gym_matrix[1:398,39], lag.max = 20)

seq <- rep(c("b Monday", "c Tuesday", "d Wednesday", "e Thursday",
             "f Friday", "g Saturday", "a Sunday"), 60)
seq <- seq[1:nrow(gym_matrix)]
gym_matrix <- data.frame(gym_matrix)
gym_matrix$day <- NULL
gym_matrix$day <- seq
woon_matrix <- data.frame(woon_matrix)
woon_matrix$day <- NULL
woon_matrix$day <- seq

#Rob J. Hyndman
library(gplots)
par(mfrow=c(1,7), mar=c(1,3,1,1))
dev.off()
ylim <- c(0,80)
plot(gym_matrix$X60, type = "l")
par(mar=c(0.1, 3.4, 2.71, 0),  #, mfcol=c(2,1) ,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1,1),
           c(2,2,3,3))
layout(m)
layout.show(3)

p <- plotmeans(X39~day, data=gym_matrix[1:398,], mgp = c(2.5,0.6,0),
               xlab="Days of the week", xaxt = "n", las=1, 
               ylab = "Average of Cluster minutes")#, yaxt ="n", ylab = "")
a <- which(substr(dates, 9,10)=="01")

dev.off()
tiff("C:/Work2/Thesis/Images and plots/cluster39_Gympie.tiff", width=1713, height=1400, res=300)
par(mar=c(2, 3.2, 2.6, 1), oma=c(1,0,0,0), mgp=c(2.2,0.62,0), las=1,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1,1),
           c(2,2,2,2),
           c(3,3,4,4))
n <- 1:398
ylim <- c(0,45)
layout(m)
layout.show(4)
plot(gym_matrix$X39[n], type = "l", xlab="", ylab="", xaxt="n")
abline(v=a)
mtext(side = 1, line=1.8, "Time (days)", cex=0.9)
mtext(side=2, line=2.1, las=0,"Number of cluster minutes", cex=0.7)
mtext(side=3, line=0.1, las=0,"The number of daily minutes in cluster 39", 
      cex=1)
mtext(side=3,"a.", adj=0)
axis(side=1, at=a, cex=0.9,
     labels = c("Jul15","Aug15","Sep15","Oct15",
     "Nov15","Dec15","Jan16","Feb16","Mar16",
     "Apr16","May16","Jun16", "Jul16"))
p <- plotmeans(X39~day, data=gym_matrix[n,], mgp = c(2.5,0.6,0),
               xlab="", las=1, ylim=ylim, xaxt = "n", 
               ylab = "")
axis(side = 1, at = 1:7, mgp=c(1,0.4,0), 
     label = c("Sunday", "Monday", "Tuesday",
               "Wednesday", "Thursday", "Friday",
               "Saturday"), mgp = c(2.5, 0.7, 0))
mtext(side=3,"b.", adj=0)
mtext(side=3, line=0.1, las=0,"The averages number of minutes each day in cluster 39", 
      cex=1)
mtext(side=2, line=2.1, las=0,"Average of cluster minutes", cex=0.7)
mtext(side=1, line=1.8, las=0,"Days of the week", cex=0.9)
acf(gym_matrix$X39[n], lag.max = 30, 
    xlab="", main="")
mtext(side=3,"c.", adj=0)
mtext(side=3, line=0.1, las=1,"Auto-cross correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer = F)
pacf(gym_matrix$X39[n], lag.max = 30, 
     xlab="", main="")
mtext(side=3,"d.", adj=0)
mtext(side=3, line=0.1, las=0,"Partial cross-correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer=F)
dev.off()
a <- which(substr(dates, 9,10)=="01")

dev.off()
tiff("C:/Work2/Thesis/Images and plots/cluster39_Woondum.tiff", width=1713, height=1400, res=300)
par(mar=c(2, 3.2, 2.6, 1), oma=c(1,0,0,0), mgp=c(2.2,0.62,0), las=1,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1,1),
           c(2,2,2,2),
           c(3,3,4,4))
n <- 1:200
layout(m)
layout.show(4)
plot(woon_matrix$X39[n], type = "l", xlab="", ylab="", xaxt="n")
abline(v=a)
mtext(side = 1, line=1.8, "Time (days)", cex=0.9)
mtext(side=2, line=2.1, las=0,"Number of cluster minutes", cex=0.7)
mtext(side=3, line=0.1, las=0,"The number of daily minutes in cluster 39", 
      cex=1)
axis(side=1, at=a, cex=0.9,
     labels = c("Jul15","Aug15","Sep15","Oct15",
                "Nov15","Dec15","Jan16","Feb16","Mar16",
                "Apr16","May16","Jun16", "Jul16"))
p <- plotmeans(X39~day, data=woon_matrix[1:398,], mgp = c(2.5,0.6,0),
               xlab="", xaxt = "n", las=1, ylim=c(0,10),
               ylab = "")
axis(side = 1, at = 1:7, mgp=c(1,0.4,0), 
     label = c("Sunday", "Monday", "Tuesday",
               "Wednesday", "Thursday", "Friday",
               "Saturday"), mgp = c(2.5, 0.7, 0))
mtext(side=3, line=0.1, las=0,"The averages number of minutes each day in cluster 39", 
      cex=1)
mtext(side=2, line=2.1, las=0,"Average of cluster minutes", cex=0.7)
mtext(side=1, line=1.8, las=0,"Days of the week", cex=0.9)
acf(woon_matrix$X39[n], lag.max = 30, 
    xlab="", main="")
mtext(side=3, line=0.1, las=1,"Auto-cross correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer = F)
pacf(woon_matrix$X39[n], lag.max = 30, 
     xlab="", main="")
mtext(side=3, line=0.1, las=0,"Partial cross-correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer=F)
dev.off()

dev.off()
tiff("C:/Work2/Thesis/Images and plots/cluster60_Gympie.tiff", width=1713, height=1400, res=300)
par(mar=c(2, 3.2, 2.6, 1), oma=c(1,0,0,0), mgp=c(2.2,0.62,0), las=1,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1,1),
           c(2,2,2,2),
           c(3,3,4,4))
layout(m)
layout.show(4)
plot(gym_matrix$X60[1:398], type = "l", xlab="", ylab="", xaxt="n")
abline(v=a)
mtext(side = 1, line=1.8, "Time (days)", cex=0.9)
mtext(side=2, line=2.1, las=0,"Number of cluster minutes", cex=0.7)
mtext(side=3, line=0.1, las=0,"The number of daily minutes in cluster 60", 
      cex=1)
mtext(side=3,"a.", adj=0)
axis(side=1, at=a, cex=0.9,
     labels = c("Jul15","Aug15","Sep15","Oct15",
                "Nov15","Dec15","Jan16","Feb16","Mar16",
                "Apr16","May16","Jun16", "Jul16"))
p <- plotmeans(X60~day, data=gym_matrix[1:398,], mgp = c(2.5,0.6,0),
               xlab="", xaxt = "n", las=1, ylim=c(0,2.1),
               ylab = "")
axis(side = 1, at = 1:7, mgp=c(1,0.4,0), 
     label = c("Sunday", "Monday", "Tuesday",
               "Wednesday", "Thursday", "Friday",
               "Saturday"), mgp = c(2.5, 0.7, 0))
mtext(side=3,"b.", adj=0)
mtext(side=3, line=0.1, las=0,"The averages number of minutes each day in cluster 60", 
      cex=1)
mtext(side=2, line=2.1, las=0,"Average of cluster minutes", cex=0.7)
mtext(side=1, line=1.8, las=0,"Days of the week", cex=0.9)
acf(gym_matrix$X60[1:398], lag.max = 30, 
    xlab="", main="")
mtext(side=3,"c.", adj=0)
mtext(side=3, line=0.1, las=1,"Auto-cross correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer = F)
pacf(gym_matrix$X60[1:398], lag.max = 30, 
     xlab="", main="")
mtext(side=3,"d.", adj=0)
mtext(side=3, line=0.1, las=0,"Partial cross-correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer=F)
dev.off()

dev.off()
tiff("C:/Work2/Thesis/Images and plots/cluster23_Gympie.tiff", width=1713, height=1400, res=300)
par(mar=c(2, 3.2, 2.6, 1), oma=c(1,0,0,0), mgp=c(2.2,0.62,0), las=1,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1,1),
           c(2,2,2,2),
           c(3,3,4,4))
n <- c(1:222,224:398)
layout(m)
layout.show(4)
plot(gym_matrix$X23[n], type = "l", xlab="", ylab="", xaxt="n")
abline(v=a)
mtext(side = 1, line=1.8, "Time (days)", cex=0.9)
mtext(side=2, line=2.1, las=0,"Number of cluster minutes", cex=0.7)
mtext(side=3, line=0.1, las=0,"The number of daily minutes in cluster 23", 
      cex=1)
mtext(side=3,"a.", adj=0)

axis(side=1, at=a, cex=0.9,
     labels = c("Jul15","Aug15","Sep15","Oct15",
                "Nov15","Dec15","Jan16","Feb16","Mar16",
                "Apr16","May16","Jun16", "Jul16"))
p <- plotmeans(X23~day, data=gym_matrix[c(n),], mgp = c(2.5,0.6,0),
               xlab="", xaxt = "n", las=1, ylim=c(0,10),
               ylab = "")
axis(side = 1, at = 1:7, mgp=c(1,0.4,0), 
     label = c("Sunday", "Monday", "Tuesday",
               "Wednesday", "Thursday", "Friday",
               "Saturday"), mgp = c(2.5, 0.7, 0))
mtext(side=3,"b.", adj=0)
mtext(side=3, line=0.1, las=0,"The averages number of minutes each day in cluster 23", 
      cex=1)
mtext(side=2, line=2.1, las=0,"Average of cluster minutes", cex=0.7)
mtext(side=1, line=1.8, las=0,"Days of the week", cex=0.9)
acf(gym_matrix$X23[n], lag.max = 30, 
    xlab="", main="")
mtext(side=3,"c.", adj=0)
mtext(side=3, line=0.1, las=1,"Auto-cross correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer = F)
pacf(gym_matrix$X23[n], lag.max = 30, 
     xlab="", main="")
mtext(side=3,"d.", adj=0)
mtext(side=3, line=0.1, las=0,"Partial cross-correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer=F)
dev.off()

dev.off()
tiff("C:/Work2/Thesis/Images and plots/cluster49_Gympie.tiff", width=1713, height=1400, res=300)
par(mar=c(2, 3.2, 2.6, 1), oma=c(1,0,0,0), mgp=c(2.2,0.62,0), las=1,
    cex = 0.6, cex.axis = 1, cex.main = 1)
m <- rbind(c(1,1,1,1),
           c(2,2,2,2),
           c(3,3,4,4))
n <- c(1:127, 129,139, 141:398)
layout(m)
layout.show(4)
plot(gym_matrix$X49[n], type = "l", xlab="", ylab="", xaxt="n")

abline(v=a)
mtext(side = 1, line=1.8, "Time (days)", cex=0.9)
mtext(side=2, line=2.1, las=0,"Number of cluster minutes", cex=0.7)
mtext(side=3, line=0.1, las=0,"The number of daily minutes in cluster 49", 
      cex=1)
mtext(side=3,"a.", adj=0)
axis(side=1, at=a, cex=0.9,
     labels = c("Jul15","Aug15","Sep15","Oct15",
                "Nov15","Dec15","Jan16","Feb16","Mar16",
                "Apr16","May16","Jun16", "Jul16"))
p <- plotmeans(X49~day, data=gym_matrix[n,], mgp = c(2.5,0.6,0),
               xlab="", xaxt = "n", las=1, ylim=c(0,10),
               ylab = "")
axis(side = 1, at = 1:7, mgp=c(1,0.4,0), 
     label = c("Sunday", "Monday", "Tuesday",
               "Wednesday", "Thursday", "Friday",
               "Saturday"), mgp = c(2.5, 0.7, 0))
mtext(side=3,"b.", adj=0)
mtext(side=3, line=0.1, las=0,"The averages number of minutes each day in cluster 49", 
      cex=1)
mtext(side=2, line=2.1, las=0,"Average of cluster minutes", cex=0.7)
mtext(side=1, line=1.8, las=0,"Days of the week", cex=0.9)
acf(gym_matrix$X49[n], lag.max = 30, 
    main="", xlab="")
mtext(side=3,"c.", adj=0)
mtext(side=3, line=0.1, las=1,"Auto-cross correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer = F)
pacf(gym_matrix$X49[n], lag.max = 30, 
     main="", xlab="")
mtext(side=3,"d.", adj=0)
mtext(side=3, line=0.1, las=0,"Partial cross-correlation", cex=1)
mtext(side=1, line=1.6, "Lag (days)", cex=0.8, outer=F)
dev.off()



p <- plotmeans(X60~day, data=woon_matrix[1:398,], mgp = c(2.5,0.6,0),
               xlab="Days of the week", xaxt = "n", las=1, 
               ylab = "Average of Cluster minutes")#, yaxt ="n", ylab = "")
axis(side = 1, at = 1:7, 
     label = c("Sunday", "Monday", "Tuesday",
               "Wednesday", "Thursday", "Friday",
               "Saturday"), mgp = c(2.5, 0.7, 0))





p <- plotmeans(X39~moon_phase, data=gym_matrix[1:126,], ylim=ylim)


one_hour_gym <- NULL
seq <- seq(0,length(df$score[1:((nrow(df)/2)-120)]), 120)

for(i in seq) {
  for(j in 1:60) {
    k <- i + j
    sum <- df$value[k] + df$value[k+60] 
    one_hour_gym <- c(one_hour_gym, sum)
    print(k)
  }
}
length(one_hour_gym)

write.csv(one_hour_gym, "one_hour_gympie.csv", row.names = F)
one_hour <- read.csv("one_hour_gympie.csv", header=T)
clusters <- df$score[1:60]
clusters <- rep(clusters, 24*398)
length(clusters)
one_hour <- data.frame(one_hour)
one_hour$cluster <- NULL
one_hour$cluster <- clusters

n="1"
a <- which(one_hour$cluster==paste("cluster",n,sep=""))
acf(one_hour$x[a], lag.max = 40, main=paste("cluster ",n,sep=""))
n <- "32"

#clusters 10, 18, 21 and 59 for the Gympie National Park site and an addition of cluster 54 for the Woondum National Park site
rain_clusters_gym <- c(10, 18, 21, 59)
rain_clusters_woon <- c(10, 18, 21, 54, 59)

#cicada clusters were 12, 32, 34 and 48 for both sites 
cicada_clusters <- c(12,32,34,48,44)
plane_clusters <- c(23, 49)
quiet_clusters <- c(13, 31, 35, 38, 41, 55)
bird_clusters <- c(3,11,14,15,33,37,43,58)
insect_clusters <- c(1,(22),26,27,29)
library(gplots)
p <- plotmeans(X53~moon_phase2, data=gym_matrix[1:398,], 
               mgp = c(2.5,0.6,0), las = 1,
               xlab="Days of the week", #xaxt = "n", las=1, 
               ylab = "Average of Cluster minutes")

bird_clusters <- c(3,11,14,15,33,37,43,58)
p <- plotmeans(gym_matrix[1:398,58]~gym_matrix[,61], data=gym_matrix[1:398,], 
               mgp = c(2.5,0.6,0), xlab="Days of the week", las=1, 
               ylab = "Average of Cluster minutes", ylim=c(0,12))
axis(side = 1, at = 1:4, 
     label = c("First Quarter", "Full Moon", "Last Quarter",
               "New Moon"), mgp = c(2.5, 0.7, 0))
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Kruskal-Wallis test - Gympie
par(mfcol=c(2,2), mar=c(3, 3.5, 1, 1), mgp=c(2, 0.4, 0))

n <- 41
n1 <- 1:398         #1 august [72] to 1 november [] 2015
i=1
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
site <- "Gympie NP"
gym_temp <- NULL
gym_temp$value <- gym_matrix[,n[i]]
gym_temp$moon_phases <- gym_matrix$moon_phase
gym_temp$day <- gym_matrix$day

a <- which(gym_temp$day[n1]=="a Sunday")
b <- which(gym_temp$day[n1]=="b Monday")
c <- which(gym_temp$day[n1]=="c Tuesday")
d <- which(gym_temp$day[n1]=="d Wednesday")
e <- which(gym_temp$day[n1]=="e Thursday")
g <- which(gym_temp$day[n1]=="f Friday")
h <- which(gym_temp$day[n1]=="g Saturday")
A <- gym_temp$value[a]
B <- gym_temp$value[b]
C <- gym_temp$value[c]
D <- gym_temp$value[d]
E <- gym_temp$value[e]
G <- gym_temp$value[g]
H <- gym_temp$value[h]

x <- c(A, B, C, D, E, G, H)
j <- factor(rep(1:7, c(length(a), length(b), length(c), length(d),
                       length(e), length(g), length(h))),
            labels = c("Sunday",
                       "Monday",
                       "Tuesday",
                       "Wednesday",
                       "Thursday",
                       "Friday",
                       "Saturday"))
kruskal.test(x, j)

label <- toString(n)
k <- kruskal.test(gym_temp$value, as.factor(gym_temp$moon_phases))
k
k1 <- k[1]
k2 <- k[2]
k3 <- k[3]

if(length(n)==1){
  p <- plotmeans(value~moon_phases, data=gym_temp, 
                 xlab="Phases of the moon", las=1, 
                 ylab = "Average of Cluster minutes",
                 main=paste("Cluster ", label, " ", site))
  legend(x=0.3, y=30, paste("k = ", round(unname(k1$statistic), 3), ", ",
                            "df = ", k2, ", p = ", k3), bty = "n")
}
if(length(n)>1){
  p <- plotmeans(value~moon_phases, data=gym_temp, 
                 xlab="Phases of the moon", las=1, 
                 ylab = "Average of Cluster minutes",
                 main=paste("Clusters ", label))
  legend(x=0.3, y=30, paste("k = ", round(unname(k1$statistic), 3), ", ",
                            "df = ", k2, ", p = ", k3), bty = "n")
}
k <- kruskal.test(gym_temp$value, as.factor(gym_temp$day))
k1 <- k[1]
k2 <- k[2]
k3 <- k[3]

if(length(n) == 1) {
  p <- plotmeans(value~day, data=gym_temp, 
                 xlab="Days of the week", las=1, 
                 ylab = "Average of Cluster minutes")
  legend(x=0.1, y=30, paste("k = ", round(unname(k1$statistic), 3), ", ",
                          "df = ", k2, ", p = ", k3), bty = "n")
}
if(length(n) > 1) {
  p <- plotmeans(value~day, data=gym_temp, 
                 xlab="Days of the week", las=1, 
                 ylab = "Average of Cluster minutes")
  legend(x=0.1, y=30, paste("k = ", round(unname(k1$statistic), 3), ", ",
                            "df = ", k2, ", p = ", k3), bty = "n")
}
acf(gym_temp$value, las=1, lag.max = 36)
kruskal.test(gym_temp$value, j)
a <- which(gym_temp$moon_phases[n1]=="FirstQuarter")
b <- which(gym_temp$moon_phases[n1]=="FullMoon")
c <- which(gym_temp$moon_phases[n1]=="LastQuarter")
d <- which(gym_temp$moon_phases[n1]=="NewMoon")
A <- gym_temp$value[a]
B <- gym_temp$value[b]
C <- gym_temp$value[c]
D <- gym_temp$value[d]

x <- c(A, B, C, D)
j <- factor(rep(1:4, c(length(a), length(b), 
                       length(c), length(d) )),
            labels = c("First quarter",
                       "Full moon",
                       "Last quarter",
                       "New moon"))
kruskal.test(x, j)

kruskal.test(gym_temp$value, as.factor(gym_temp$moon_phases))

# 
tiff(paste("C:/Work2/Thesis/Images and plots/Phases_of_the_moon_",site,
           ".tiff",sep=""), width = 1713, height = 1170, units = 'px', 
     res = 300)
par(mfcol=c(2,2), mar=c(2.4, 2.6, 1, 1), mgp=c(2, 0.4, 0))
library(gplots)

p <- plotmeans(X6~moon_phase2, data=gym_matrix[1:398,], ylab="", 
               mgp = c(1.25,0.5,0), las = 1, ylim=c(0,13.7),
               xlab="Phases of the moon", xaxt = "n", las=1)
axis(side = 1, at = 1:4, cex.axis=1, 
     label = c("FQ", "FM", "LQ", "NM"), mgp = c(1.4, 0.5, 0))
mtext(side = 2, line= 1.7, text = "Average of cluster minutes", cex=0.8)
mtext("Cluster 6")
p <- plotmeans(X38~moon_phase2, data=gym_matrix[1:398,], 
               mgp = c(1.25,0.5,0), las = 1, ylim=c(0,23), ylab="",
               xlab="Phases of the moon", xaxt = "n", las=1)
axis(side = 1, at = 1:4, cex.axis=1, 
     label = c("FQ", "FM", "LQ", "NM"), mgp = c(1.4, 0.5, 0))
mtext(side = 2, line= 1.7, text = "Average of cluster minutes", cex=0.8)
mtext("Cluster 38")
p <- plotmeans(X13~moon_phase2, data=gym_matrix[1:398,], 
               mgp = c(1.25,0.5,0), las = 1, ylim=c(0,130), ylab="",
               xlab="Phases of the moon", xaxt = "n", las=1)
axis(side = 1, at = 1:4, cex.axis=1, 
     label = c("FQ", "FM", "LQ", "NM"), mgp = c(1.4, 0.5, 0))
mtext("Cluster 13")
mtext(side = 2, line= 1.7, text = "Average of cluster minutes", cex=0.8)
p <- plotmeans(X41~moon_phase2, data=gym_matrix[1:398,], 
               mgp = c(1.25,0.5,0), las = 1, ylab="",
               xlab="Phases of the moon", xaxt = "n", las=1)
axis(side = 1, at = 1:4, cex.axis=1, 
     label = c("FQ", "FM", "LQ", "NM"), mgp = c(1.4, 0.5, 0))
mtext("Cluster 41")
mtext(side = 2, line= 1.7, text = "Average of cluster minutes", cex=0.8)
dev.off()



#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
site <- "Woondum NP"

woon_temp$value[1:nrow(woon_matrix)] <- 0
for(i in 1:length(n)) {
  woon_temp$value <- woon_temp$value[1:nrow(woon_matrix)] + woon_matrix[,n[i]]
}
woon_temp$moon_phases <- woon_matrix$moon_phase
woon_temp$day <- woon_matrix$day
woon_temp[1:days, ]
a <- which(woon_temp$day[n1]=="a Sunday")
b <- which(woon_temp$day[n1]=="b Monday")
c <- which(woon_temp$day[n1]=="c Tuesday")
d <- which(woon_temp$day[n1]=="d Wednesday")
e <- which(woon_temp$day[n1]=="e Thursday")
g <- which(woon_temp$day[n1]=="f Friday")
h <- which(woon_temp$day[n1]=="g Saturday")
A <- woon_temp$value[a]
B <- woon_temp$value[b]
C <- woon_temp$value[c]
D <- woon_temp$value[d]
E <- woon_temp$value[e]
G <- woon_temp$value[g]
H <- woon_temp$value[h]

x <- c(A, B, C, D, E, G, H)
j <- factor(rep(1:7, c(length(a), length(b), length(c), length(d),
                       length(e), length(g), length(h))),
            labels = c("Sunday",
                       "Monday",
                       "Tuesday",
                       "Wednesday",
                       "Thursday",
                       "Friday",
                       "Saturday"))
kruskal.test(x, j)

label <- toString(n)
k <- kruskal.test(woon_temp$value, as.factor(woon_temp$moon_phases))
k
k1 <- k[1]
k2 <- k[2]
k3 <- k[3]

if(length(n)==1){
  p <- plotmeans(value~moon_phases, data=woon_temp[n1], 
                 xlab="Phases of the moon", las=1, 
                 ylab = "Average of Cluster minutes",
                 main=paste("Cluster ", label, " ", site))
  legend(x=0.3, y=8, paste("k = ", round(unname(k1$statistic), 3), ", ",
                            "df = ", k2, ", p = ", k3), bty = "n")
}
if(length(n)>1){
  p <- plotmeans(value~moon_phases, data=woon_temp, 
                 xlab="Phases of the moon", las=1, 
                 ylab = "Average of Cluster minutes",
                 main=paste("Clusters ", label))
  legend(x=0.3, y=8, paste("k = ", round(unname(k1$statistic), 3), ", ",
                            "df = ", k2, ", p = ", k3), bty = "n")
}
k <- kruskal.test(woon_temp$value, as.factor(woon_temp$day))
k1 <- k[1]
k2 <- k[2]
k3 <- k[3]

if(length(n) == 1) {
  p <- plotmeans(value~day, data=woon_temp, 
                 xlab="Days of the week", las=1, 
                 ylab = "Average of Cluster minutes")
  legend(x=0.1, y=30, paste("k = ", round(unname(k1$statistic), 3), ", ",
                            "df = ", k2, ", p = ", k3), bty = "n")
}
if(length(n) > 1) {
  p <- plotmeans(value~day, data=woon_temp, 
                 xlab="Days of the week", las=1, 
                 ylab = "Average of Cluster minutes")
  legend(x=0.1, y=30, paste("k = ", round(unname(k1$statistic), 3), ", ",
                            "df = ", k2, ", p = ", k3), bty = "n")
}
acf(woon_temp$value, las=1, lag.max = 36)
kruskal.test(woon_temp$value, j)
a <- which(woon_temp$moon_phases[n1]=="FirstQuarter")
b <- which(woon_temp$moon_phases[n1]=="FullMoon")
c <- which(woon_temp$moon_phases[n1]=="LastQuarter")
d <- which(woon_temp$moon_phases[n1]=="NewMoon")
A <- woon_temp$value[a]
B <- woon_temp$value[b]
C <- woon_temp$value[c]
D <- woon_temp$value[d]

x <- c(A, B, C, D)
j <- factor(rep(1:4, c(length(a), length(b), 
                       length(c), length(d) )),
            labels = c("First quarter",
                       "Full moon",
                       "Last quarter",
                       "New moon"))
kruskal.test(x, j)

kruskal.test(gym_temp$value, as.factor(gym_temp$moon_phases))
kruskal.test(gym_temp$value, as.factor(gym_temp$day))
kruskal.test(woon_temp$value, as.factor(woon_temp$moon_phases))
kruskal.test(woon_temp$value, as.factor(woon_temp$day))

# Kruskal test
#Gympie
n1 <- 1:398
statistics <- NULL
statistics <- as.data.frame(matrix(NA,nrow=60,ncol=3))
statistics$k <- 1
statistics$df <- 1
statistics$p <- 1
for(i in 1:60) {
  n <- i
  site <- "Gympie NP"
  gym_temp <- NULL
  gym_temp$value <- gym_matrix[,n]
  gym_temp$moon_phases <- gym_matrix$moon_phase
  gym_temp$day <- gym_matrix$day
  
  a <- which(gym_temp$day[n1]=="a Sunday")
  b <- which(gym_temp$day[n1]=="b Monday")
  c <- which(gym_temp$day[n1]=="c Tuesday")
  d <- which(gym_temp$day[n1]=="d Wednesday")
  e <- which(gym_temp$day[n1]=="e Thursday")
  g <- which(gym_temp$day[n1]=="f Friday")
  h <- which(gym_temp$day[n1]=="g Saturday")
  A <- gym_temp$value[a]
  B <- gym_temp$value[b]
  C <- gym_temp$value[c]
  D <- gym_temp$value[d]
  E <- gym_temp$value[e]
  G <- gym_temp$value[g]
  H <- gym_temp$value[h]
  
  x <- c(A, B, C, D, E, G, H)
  j <- factor(rep(1:7, c(length(a), length(b), length(c), length(d),
                         length(e), length(g), length(h))),
              labels = c("Sunday",
                         "Monday",
                         "Tuesday",
                         "Wednesday",
                         "Thursday",
                         "Friday",
                         "Saturday"))
  kruskal.test(x, j)
  
  label <- toString(n)
  k <- kruskal.test(gym_temp$value, as.factor(gym_temp$moon_phases))
  k1 <- k[1]
  k2 <- k[2]
  k3 <- k[3]
 statistics$k[i] <- as.vector(k1[[1]])
 statistics$df[i] <- as.vector(k2[[1]])
 statistics$p[i] <- as.vector(k3[[1]])
}
View(statistics)
cluster <- ts(gym_temp$value)
tsdisplay(cluster, lag.max = 30)

# Kruskal test
#Woondum
n1 <- 1:(398-(30+23))
statistics <- NULL
statistics <- as.data.frame(matrix(NA,nrow=60))
statistics$k <- 1
statistics$df <- 1
statistics$p <- 1
for(i in 1:60) {
  n <- i
  site <- "Woondum NP"
  woon_temp <- NULL
  woon_temp$value <- woon_matrix[n1, n]
  woon_temp$moon_phases <- woon_matrix$moon_phase[n1]
  woon_temp$day <- woon_matrix$day[n1]
  
  a <- which(woon_temp$day[n1]=="a Sunday")
  b <- which(woon_temp$day[n1]=="b Monday")
  c <- which(woon_temp$day[n1]=="c Tuesday")
  d <- which(woon_temp$day[n1]=="d Wednesday")
  e <- which(woon_temp$day[n1]=="e Thursday")
  g <- which(woon_temp$day[n1]=="f Friday")
  h <- which(woon_temp$day[n1]=="g Saturday")
  A <- woon_temp$value[a]
  B <- woon_temp$value[b]
  C <- woon_temp$value[c]
  D <- woon_temp$value[d]
  E <- woon_temp$value[e]
  G <- woon_temp$value[g]
  H <- woon_temp$value[h]
  
  x <- c(A, B, C, D, E, G, H)
  j <- factor(rep(1:7, c(length(a), length(b), length(c), length(d),
                         length(e), length(g), length(h))),
              labels = c("Sunday",
                         "Monday",
                         "Tuesday",
                         "Wednesday",
                         "Thursday",
                         "Friday",
                         "Saturday"))
  kruskal.test(x, j)
  
  label <- toString(n)
  k <- kruskal.test(woon_temp$value, as.factor(woon_temp$moon_phases[n1]))
  k1 <- k[1]
  k2 <- k[2]
  k3 <- k[3]
  statistics$k[i] <- as.vector(k1[[1]])
  statistics$df[i] <- as.vector(k2[[1]])
  statistics$p[i] <- as.vector(k3[[1]])
}
View(statistics)

par(mfcol=c(1,2), mar=c(3, 3.5, 1, 1), mgp=c(2, 0.4, 0))
n <- 29
site <- "Woondum NP"
woon_temp <- NULL
woon_temp$value <- woon_matrix[n1, n]
woon_temp$moon_phases <- woon_matrix$moon_phase[n1]
woon_temp$day <- woon_matrix$day[n1]

a <- which(woon_temp$day[n1]=="a Sunday")
b <- which(woon_temp$day[n1]=="b Monday")
c <- which(woon_temp$day[n1]=="c Tuesday")
d <- which(woon_temp$day[n1]=="d Wednesday")
e <- which(woon_temp$day[n1]=="e Thursday")
g <- which(woon_temp$day[n1]=="f Friday")
h <- which(woon_temp$day[n1]=="g Saturday")
A <- woon_temp$value[a]
B <- woon_temp$value[b]
C <- woon_temp$value[c]
D <- woon_temp$value[d]
E <- woon_temp$value[e]
G <- woon_temp$value[g]
H <- woon_temp$value[h]

x <- c(A, B, C, D, E, G, H)
j <- factor(rep(1:7, c(length(a), length(b), length(c), length(d),
                       length(e), length(g), length(h))),
            labels = c("Sunday",
                       "Monday",
                       "Tuesday",
                       "Wednesday",
                       "Thursday",
                       "Friday",
                       "Saturday"))
kruskal.test(x, j)

label <- toString(n)
library(gplots)
p <- plotmeans(value~moon_phases, data=woon_temp, 
               xlab="Phases of the moon", las=1, 
               ylab = "Average of Cluster minutes",
               main=paste("Cluster ", label, " ", site))
acf(woon_temp$value[n1], lag.max = 32)

library(forecast)
citation(package = "forecast")
cluster <- ts(woon_temp$value)
tsdisplay(cluster, lag.max = 30)
