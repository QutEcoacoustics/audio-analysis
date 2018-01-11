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
gym_clust$acoustic_state <- gym_clust$clust
gym_clust$value <- gym_clust$count
gym_clust$family <- gym_clust$month
gym_clust$item <- gym_clust$date

woon_clust$acoustic_state <- woon_clust$clust
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
colnames(gympie_clusters) <- c("acoustic_state","value","family","item")

class <- c("rain","birds","insects","cicada","planes","quiet","wind")
class <- rep(class, 398)
gympie_clusters$acoustic_state <- class
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
colnames(woondum_clusters) <- c("acoustic_state","value","family","item")

class <- c("rain","birds","insects","cicada","planes","quiet","wind")
class <- rep(class, 398)
woondum_clusters$acoustic_state <- class
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
                              innerRadius = 0.3, 
                              outerRadius = 1,
                              #guides = c(20, 40, 60, 80), 
                              guides = c((2*8.3333), (4*8.3333), (6*8.3333), 
                                         (8*8.3333), (10*8.3333), (12*8.3333)), 
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
  df <- arrange(df, family, item, acoustic_state)
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
  df <- arrange(df, family, item, acoustic_state)
  
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
                                  ymin = ymin, ymax = ymax, fill = acoustic_state))
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
    ifelse(dfItemLabels$item[i]=="01"|dfItemLabels$item[i]=="5"|dfItemLabels$item[i]=="10"|dfItemLabels$item[i]=="15"|dfItemLabels$item[i]=="20"|dfItemLabels$item[i]=="25",
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
  p <- p + scale_fill_manual(values = c('a birds'="#009E73",
                                        'b cicada'="#E69F00", 
                                        'c insects'= "#F0E442",
                                        'd planes'="#CC79A7", 
                                        'e rain'="#0072B2",
                                        'f wind'="#56B4E9",
                                        'g quiet'="#999999",
                                        'end'="gray30"))
  p <- p + theme(legend.text=element_text(size=1))
  p <- p + theme(legend.position="none")
}
#gympie_clusters365 <- gympie_clusters[1:2555,]
gympie_clusters365 <- gympie_clusters

a <- which(gympie_clusters365$acoustic_state=="wind")
b <- which(substr(gympie_clusters365$dates,9,10)=="01")
c <- intersect(a,b)

insertRow <- function(gympie_clusters365, newrow, r) {
  gympie_clusters365[seq(r+1,nrow(gympie_clusters365)+1),] <- gympie_clusters365[seq(r,nrow(gympie_clusters365)),]
  gympie_clusters365[r,] <- newrow
  gympie_clusters365 <- gympie_clusters365
  gympie_clusters365 <<- gympie_clusters365
}
# insert additional rows to mark the end of the month in black
for(i in 1:length(c)) {
  r <- c[i]-6
  newrow <- c("end", 1440, gympie_clusters365$family[r-1], 
              (as.numeric(gympie_clusters365$item[r-1])+1),
              paste(substr(gympie_clusters365$dates[r-1],1,8),
                    as.character(as.numeric(substr(gympie_clusters365$dates[r-1],9,10)) + 1), 
                    sep = ""))
  insertRow(gympie_clusters365, newrow, r)
  c <- c + 1
}

gympie_clusters365$value <- as.numeric(gympie_clusters365$value)
gympie_clusters365$item <- as.numeric(gympie_clusters365$item)

woondum_clusters365 <- woondum_clusters

a <- which(woondum_clusters365$acoustic_state=="wind")
b <- which(substr(woondum_clusters365$dates,9,10)=="01")
c <- intersect(a,b)

insertRow <- function(woondum_clusters365, newrow, r) {
  woondum_clusters365[seq(r+1,nrow(woondum_clusters365)+1),] <- woondum_clusters365[seq(r,nrow(woondum_clusters365)),]
  woondum_clusters365[r,] <- newrow
  woondum_clusters365 <- woondum_clusters365
  woondum_clusters365 <<- woondum_clusters365
}
# insert additional rows to mark the end of the month in black
for(i in 1:length(c)) {
  r <- c[i]-6
  newrow <- c("end", 1440, woondum_clusters365$family[r-1], 
              (as.numeric(woondum_clusters365$item[r-1])+1),
              paste(substr(woondum_clusters365$dates[r-1],1,8),
                    as.character(as.numeric(substr(woondum_clusters365$dates[r-1],9,10)) + 1), 
                    sep = ""))
  insertRow(woondum_clusters365, newrow, r)
  c <- c + 1
}

woondum_clusters365$value <- as.numeric(woondum_clusters365$value)
woondum_clusters365$item <- as.numeric(woondum_clusters365$item)

a <- which(gympie_clusters365$acoustic_state=="birds")
gympie_clusters365$acoustic_state[a] <- "a birds"
a <- which(woondum_clusters365$acoustic_state=="birds")
woondum_clusters365$acoustic_state[a] <- "a birds"
a <- which(gympie_clusters365$acoustic_state=="cicada")
gympie_clusters365$acoustic_state[a] <- "b cicada"
a <- which(woondum_clusters365$acoustic_state=="cicada")
woondum_clusters365$acoustic_state[a] <- "b cicada"
a <- which(gympie_clusters365$acoustic_state=="insects")
gympie_clusters365$acoustic_state[a] <- "c insects"
a <- which(woondum_clusters365$acoustic_state=="insects")
woondum_clusters365$acoustic_state[a] <- "c insects"
a <- which(gympie_clusters365$acoustic_state=="planes")
gympie_clusters365$acoustic_state[a] <- "d planes"
a <- which(woondum_clusters365$acoustic_state=="planes")
woondum_clusters365$acoustic_state[a] <- "d planes"
a <- which(gympie_clusters365$acoustic_state=="rain")
gympie_clusters365$acoustic_state[a] <- "e rain"
a <- which(woondum_clusters365$acoustic_state=="rain")
woondum_clusters365$acoustic_state[a] <- "e rain"
a <- which(gympie_clusters365$acoustic_state=="wind")
gympie_clusters365$acoustic_state[a] <- "f wind"
a <- which(woondum_clusters365$acoustic_state=="wind")
woondum_clusters365$acoustic_state[a] <- "f wind"
a <- which(gympie_clusters365$acoustic_state=="quiet")
gympie_clusters365$acoustic_state[a] <- "g quiet"
a <- which(woondum_clusters365$acoustic_state=="quiet")
woondum_clusters365$acoustic_state[a] <- "g quiet"

#write.csv(gympie_clusters365, "C://Work2//Projects//Twelve_,month_clustering//Saving_dataset//plots//polar_plot_gym.csv", row.names = F)
#gympie_clusters365 <- read.csv("C://Work2//Projects//Twelve_,month_clustering//Saving_dataset//plots//polar_plot_gym.csv",header=T)
p1 <- polarHistogram365(gympie_clusters365, 
                        familyLabels = TRUE,
                        circleProportion = 0.98,
                        normalised = FALSE)
p1 <- p1 + ggtitle("Gympie NP") + theme(title = element_text(vjust = -6)) + theme(title = element_text(size=20)) 
p1 <- p1 + theme(plot.margin=unit(c(0,-10,0,0),"mm"))
print(p1)
ggsave('C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\plots\\Fig14_gym_unedited.tiff', 
       width = 7.5, height = 7.5, dpi = 300, bg = "transparent")

p1 <- polarHistogram365(woondum_clusters365, 
                        familyLabels = TRUE,
                        circleProportion = 0.98,
                        normalised = FALSE)
p1 <- p1 + ggtitle("Woondum NP") + theme(title = element_text(vjust = -6)) + theme(title = element_text(size=20)) 
p1 <- p1 + theme(plot.margin=unit(c(0,-10,0,0),"mm"))
print(p1)
ggsave("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\plots\\Fig14woon_unedited.tiff", 
       width = 7.5, height = 7.5, dpi = 300, bg = "transparent")

data_example <- data.frame(
  acoustic_state = c("rain", "birds","insects","cicada", 
            "planes", "quiet", "wind",
            "rain", "birds","insects","cicada", 
            "planes", "quiet", "wind",
            "rain", "birds","insects","cicada", 
            "planes", "quiet", "wind",
            "rain", "birds","insects","cicada", 
            "planes", "quiet", "wind",
            "rain", "birds","insects","cicada", 
            "planes", "quiet", "wind"),
  value = c(7,365,132,38,10,526,362,
            5,496,90,11,15,754,69,
            128,420,89,1,9,759,41,
            11,390,147,11,13,778,90,
            5,103,67,98,2,348,817),
  family = c("a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015",
             "a June 2015","a June 2015","a June 2015","a June 2015"),
  item = c(22,22,22,22,22,22,22,
           23,23,23,23,23,23,23,
           24,24,24,24,24,24,24,
           25,25,25,25,25,25,25,
           26,26,26,26,26,26,26),
  dates = c("2015-06-22","2015-06-22","2015-06-22",
            "2015-06-22","2015-06-22","2015-06-22","2015-06-22",
            "2015-06-23","2015-06-23","2015-06-23",
            "2015-06-23","2015-06-23","2015-06-23","2015-06-23",
            "2015-06-24","2015-06-24","2015-06-24",
            "2015-06-24","2015-06-24","2015-06-24","2015-06-24",
            "2015-06-25","2015-06-25","2015-06-25",
            "2015-06-25","2015-06-25","2015-06-25","2015-06-25",
            "2015-06-26","2015-06-26","2015-06-26",
            "2015-06-26","2015-06-26","2015-06-26","2015-06-26"))
p1 <- polarHistogram365(data_example, 
                        familyLabels = TRUE,
                        circleProportion = 0.98,
                        normalised = FALSE)
p1 <- p1 + ggtitle("Gympie NP") + theme(title = element_text(vjust = -6)) + theme(title = element_text(size=20)) 
p1 <- p1 + theme(plot.margin=unit(c(0,-10,0,0),"mm"))
print(p1)
p1
ggsave("sample.tiff", 
       width = 7.5, height = 7.5, dpi = 300, bg = "transparent")
