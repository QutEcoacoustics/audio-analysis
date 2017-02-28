# Generate polarHistograms and rose plots
# Author: Yvonne Phillips
# Author of polarHistogram function: Christophe Ladroue
# Downloaded from http://chrisladroue.com/2012/02/polar-histogram-pretty-and-useful/
# and chrisladroue.com/wp-content/uploads/2012/02/polarHistogram.R.zip
# the original code has been adapted by Yvonne Phillips
# Date:  30 October 2016

# Description: Produces two different polarHistograms containing 
# a display of 1. each cluster class is occuring per day,
#              2. each cluster occuring throughout the day per month
# Also produces the plot comparing the occurance of rain and insects

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

#View(woondum_clusters)

##### PolarHistogram function
#' builds many histograms and arranges them around a circle to save space.
#'
#' The data frame is expected to have at least four columns: family, item, score and value.
#' The three first columns are categorical variables, the fourth column contains non-negative values.
#' See the vignette for an example.
#' Each bar represents the proportion of scores for one item. Items can be grouped by families.
#' The resulting graph can be busy and might be better off saved as a pdf, with ggsave("myGraph.pdf").
#'
#' @author Christophe Ladroue
#' @param df a data frame containing the data
#' @param family list for defining families
#' @param columnNames user-defined columns names
#' @param binSize width of the bin. Should probably be left as 1, as other parameters are relative to it.
#' @param spaceItem space between bins
#' @param spaceFamily space between families
#' @param innerRadius radius of inner circle
#' @param outerRadius radius of outer circle. Should probably be left as 1, as other parameters are relative to it.
#' @param guides a vector with percentages to use for the white guide lines
#' @param alphaStart offset from 12 o'clock in radians
#' @param circleProportion proportion of the circle to cover
#' @param direction either of "inwards" or "outwards". Whether the increasing count goes from or to the centre.
#' @param familyLabels logical. Whether to show family names
#' @param normalised logical.
#' @return a ggplot object
#' @export
#' @import ggplot2
#' @importFrom plyr ddply
#' @importFrom plyr arrange
#' @importFrom plyr .
#' @importFrom plyr summarise
#' @importFrom plyr summarize
#' @examples
#' set.seed(42)
#' nFamily <- 20
#' nItemPerFamily <- sample(1:6,nFamily,replace=TRUE)
#' nValues <- 3
#' randomWord <- function(n,nLetters=5)
#'   replicate(n, paste(sample(letters, nLetters, replace = TRUE), sep = '', collapse = ''))
#'
#' df <- data.frame(
#'   family = rep( randomWord(nFamily), times = nValues * nItemPerFamily),
#'   item   = rep( randomWord(sum(nItemPerFamily), 3), each = nValues ),
#'   score  = rep( paste0("V",1:nValues), times = sum(nItemPerFamily)),
#'   value  = runif( sum(nItemPerFamily * nValues)))
#' print(head(df))
#' p <- polarHistogram(df, familyLabels = FALSE)
#' print(p)

# Christophe Ladroue
library(plyr)
library(ggplot2)
#source("polarHistogram.R")
# polarHistogram function --------------------------------------------
polarHistogram <-function (df, family = NULL, 
                           columnNames = NULL, 
                           binSize = 1,
                           spaceItem = 0.2, 
                           spaceFamily = 0, 
                           innerRadius = 0, 
                           outerRadius = 1,
                           guides = c(40, 60, 80), 
                           alphaStart = -0.015, #-0.3, 
                           circleProportion = 0.98,
                           direction = "outwards", 
                           familyLabels = FALSE, 
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
  #else {
  #  maxFamily <- max(plyr::ddply(df,.(family,item), summarise, total = sum(value))$total)
  #  df <- ddply(df, .(family, item), transform, value = cumsum(value))
  #  df$value <- df$value/maxFamily
  #}
  
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
  
  p <- p + geom_text(aes(x = x, label = item, angle = angle,
                           hjust = hjust), y = 1.02, size = 5, vjust = 0.5, data = dfItemLabels)  
  
  p <- p + geom_segment(aes(x = xmin, xend = xend, y = y, yend = y),
                        colour = "white", data = guidesDF)
  
  #if(normalised)
    guideLabels <- data.frame(x = 0, y = affine(1 - guides/100),
                              label = paste(guides, "% ", sep = ""))
  #else
  #  guideLabels <- data.frame(x = 0, y = affine(1 - guides/maxFamily),
  #                            label = paste(guides, " ", sep = ""))
  
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
                       data = familyLabelsDF, y = 1.22, size = 3)
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
  p <- p + theme(legend.text=element_text(size=15))
  p <- p + theme(legend.justification="right", 
                 legend.position=c(1.015,0.5))
}

# polarHistogram365 --------------------------------------------
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
#df <- gympie_clusters365
#family = NULL 
#columnNames = NULL
#binSize = 1
#spaceItem = 0.2
#spaceFamily = 0 
#innerRadius = 0 
#outerRadius = 1
#guides = c(40, 60, 80)
#alphaStart = 0 
#circleProportion = 0.98
#direction = "outwards" 
#familyLabels = TRUE
#normalised = FALSE
#units = "cm"
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
  #else {
  #  maxFamily <- max(plyr::ddply(df,.(family,item), summarise, total = sum(value))$total)
  #  df <- ddply(df, .(family, item), transform, value = cumsum(value))
  #  df$value <- df$value/maxFamily
  #}
  
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
    ifelse(dfItemLabels$item[i]=="01"|dfItemLabels$item[i]=="05"|dfItemLabels$item[i]=="10"|dfItemLabels$item[i]=="15"|dfItemLabels$item[i]=="20"|dfItemLabels$item[i]=="25",
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
  p <- p + theme(legend.text=element_text(size=12))
  p <- p + theme(legend.justification="right", 
                 legend.position=c(1.00001,0.4))
}

# a little helper that generates random names for families and items.
#randomName<-function(n=1,syllables=3){
#  vowels<-c("a","e","i","o","u","y")
#  consonants<-setdiff(letters,vowels)
#  replicate(n,
#            paste(
#              rbind(sample(consonants,syllables,replace=TRUE),
#                    sample(vowels,syllables,replace=TRUE)),
#              sep='',collapse='')
#  )
#}

#set.seed(42)

#nFamily<-20
#nItemPerFamily<-sample(1:6,nFamily,replace=TRUE)
#nValues<-3

#df<-data.frame(
#  family=rep(randomName(nFamily),nItemPerFamily),
#  item=randomName(sum(nItemPerFamily),2))

#df<-cbind(df,as.data.frame(matrix(runif(nrow(df)*nValues),nrow=nrow(df),ncol=nValues)))

#library(reshape)
#df<-melt(df,c("family","item"), variable_name="score") # from wide to long
#p<-polarHistogram(df, familyLabel=T)
#print(p)

gympie_clusters365 <- gympie_clusters[1:2555,]
gympie_clusters365 <- gympie_clusters
p1 <- polarHistogram365(gympie_clusters365, 
                        familyLabels = TRUE,
                        circleProportion = 0.98,
                        normalised = FALSE)
p1 <- p1 + ggtitle("Gympie NP") + theme(title = element_text(vjust = -6)) + theme(title = element_text(size=20)) 
print(p1)
ggsave('polarHistograms/Gympie_polarHistogram_365.png', 
       width = 9, height = 9, dpi = 400, bg = "transparent")

woondum_clusters365 <- woondum_clusters[1:2555,]
woondum_clusters365 <- woondum_clusters
p1 <- polarHistogram365(woondum_clusters365, 
                        familyLabels = TRUE,
                        circleProportion = 0.98,
                        normalised = FALSE)
p1 <- p1 + ggtitle("Woondum NP") + theme(title = element_text(vjust = -6)) + theme(title = element_text(size=20)) 
print(p1)
ggsave('polarHistograms/Woondum_polarHistogram_365.png', 
       width = 9, height = 9, dpi = 400, bg = "transparent")

# This is for plots of a few months at a time
# These must be in whole multiples of the classes
gympie_clusters1 <- gympie_clusters[1:420,] 
gympie_clusters2 <- gympie_clusters[421:840,] 
gympie_clusters3 <- gympie_clusters[841:1260,]
gympie_clusters4 <- gympie_clusters[1261:1680,]
gympie_clusters5 <- gympie_clusters[1681:2100,]
gympie_clusters6 <- gympie_clusters[2101:2520,]
gympie_clusters7 <- gympie_clusters[2521:length(gympie_clusters$score),]

p1 <- polarHistogram(gympie_clusters1, 
                     familyLabels = TRUE,
                     circleProportion = 0.98,
                     normalised = FALSE)
p1 <- p1 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=18))
print(p1) 
ggsave('polarHistograms/Gympie_polarHistogram_p1.png', 
       width = 9, height = 9, dpi = 400, bg = "transparent")

p2 <- polarHistogram(gympie_clusters2, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p2 <- p2 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p2)
ggsave('polarHistograms/Gympie_polarHistogram_p2.png', 
       width = 9, height = 9, dpi = 400, bg = "transparent")

p3 <- polarHistogram(gympie_clusters3, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p3 <- p3 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p3)
ggsave('polarHistograms/Gympie_polarHistogram_p3.png', 
       width = 9, height = 9, dpi = 400)

p4 <- polarHistogram(gympie_clusters4, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p4 <- p4 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p4)
ggsave('polarHistograms/Gympie_polarHistogram_p4.png', 
       width = 9, height = 9, dpi = 400)

p5 <- polarHistogram(gympie_clusters5, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p5 <- p5 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p5)
ggsave('polarHistograms/Gympie_polarHistogram_p5.png', 
       width = 9, height = 9, dpi = 400)

p6 <- polarHistogram(gympie_clusters6, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p6 <- p6 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p6)
ggsave('polarHistograms/Gympie_polarHistogram_p6.png', 
       width = 9, height = 9, dpi = 400)

p7 <- polarHistogram(gympie_clusters7, 
                     familyLabels = TRUE,
                     circleProportion 
                     = 0.92*length(gympie_clusters7$score)/420,
                     normalised = FALSE)
p7 <- p7 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p7)
ggsave('polarHistograms/Gympie_polarHistogram_p7.png', 
       width = 9, height = 9, dpi = 400)

woondum_clusters1 <- woondum_clusters[1:420,]
woondum_clusters2 <- woondum_clusters[421:840,]
woondum_clusters3 <- woondum_clusters[841:1260,]
woondum_clusters4 <- woondum_clusters[1261:1680,]
woondum_clusters5 <- woondum_clusters[1681:2100,]
woondum_clusters6 <- woondum_clusters[2101:2520,]
woondum_clusters7 <- woondum_clusters[2521:length(woondum_clusters$score),]

require(gridExtra)
w1 <- polarHistogram(woondum_clusters1, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w1 <- w1 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w1)
ggsave('polarHistograms/Woondum_polarHistogram_p1.png', 
       width = 9, height = 9, dpi = 400)

w2 <- polarHistogram(woondum_clusters2, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w2 <- w2 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w2)
ggsave('polarHistograms/Woondum_polarHistogram_p2.png', 
       width = 9, height = 9, dpi = 400)

w3 <- polarHistogram(woondum_clusters3, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w3 <- w3 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w3)
ggsave('polarHistograms/Woondum_polarHistogram_p3.png', 
       width = 9, height = 9, dpi = 400)

w4 <- polarHistogram(woondum_clusters4, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w4 <- w4 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w4)
ggsave('polarHistograms/Woondum_polarHistogram_p4.png', 
       width = 9, height = 9, dpi = 400)

w5 <- polarHistogram(woondum_clusters5, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w5 <- w5 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w5)
ggsave('polarHistograms/Woondum_polarHistogram_p5.png', 
       width = 9, height = 9, dpi = 400)

w6 <- polarHistogram(woondum_clusters6, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w6 <- w6 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w6)
ggsave('polarHistograms/Woondum_polarHistogram_p6.png', 
       width = 9, height = 9, dpi = 400)

w7 <- polarHistogram(woondum_clusters7, 
                     familyLabels = TRUE,
                     circleProportion 
                     = 0.92*length(woondum_clusters7$score)/420,
                     normalised = FALSE)
w7 <- w7 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w7)
ggsave('polarHistograms/Woondum_polarHistogram_p7.png', 
       width = 9, height = 9, dpi = 400)

dev.off()

# rose plots -------------------------------------------------

# remove all objects in the global environment
rm(list = ls())

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
                           circleProportion = 1,
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
  #p <- p + theme(legend.text=element_text(size=15))
  #p <- p + theme(legend.justification="right", 
  #               legend.position=c(1.015,0.5))
}

#clusters <- read.csv("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/weather/clusters_30minute.csv", header = TRUE)

# generate a time sequence
#startDate = as.POSIXct("2013-12-23 00:15:00")
#endDate = as.POSIXct("2013-12-23 23:45:00")
#dateSeq5sec = seq(from=startDate, to=endDate, by="1800 sec")
#head(dateSeq5sec)
#times <- substr(dateSeq5sec, 12, 16)

# duplicate times to the length of the data.frame
#times <- rep(times, (nrow(clusters)/length(times)) )

# fill the 'family' column with a month description
#for(i in 1:nrow(clusters)) {
#  if((substr(clusters$dateSeq30min[i],6,7)=="06") &
#     (substr(clusters$dateSeq30min[i],1,4)=="2015")) {
#    clusters$family[i] <- "a  Jun 15"
#  }  
#  if((substr(clusters$dateSeq30min[i],6,7)=="06") &
#     (substr(clusters$dateSeq30min[i],1,4)=="2016")) {
#    clusters$family[i] <- "m  Jun 16"
#  } 
#  if((substr(clusters$dateSeq30min[i],6,7)=="07") &
#     (substr(clusters$dateSeq30min[i],1,4)=="2015")) {
#    clusters$family[i] <- "b  Jul 15"
#  }  
#  if((substr(clusters$dateSeq30min[i],6,7)=="07") &
#     (substr(clusters$dateSeq30min[i],1,4)=="2016")) {
#    clusters$family[i] <- "n  Jul 16"
#  }
#  if(substr(clusters$dateSeq30min[i],6,7)=="08") {
#    clusters$family[i] <- "c  Aug 15"
#  }
#  if(substr(clusters$dateSeq30min[i],6,7)=="09") {
#    clusters$family[i] <- "d  Sept 15"
#  }
#  if(substr(clusters$dateSeq30min[i],6,7)=="10") {
#    clusters$family[i] <- "e  Oct 15"
#  }  
#  if(substr(clusters$dateSeq30min[i],6,7)=="11") {
#    clusters$family[i] <- "f  Nov 15"
#  }
#  if(substr(clusters$dateSeq30min[i],6,7)=="12") {
#    clusters$family[i] <- "g  Dec 15"
#  }
#  if(substr(clusters$dateSeq30min[i],6,7)=="01") {
#    clusters$family[i] <- "h  Jan 16"
#  }
#  if(substr(clusters$dateSeq30min[i],6,7)=="02") {
#    clusters$family[i] <- "i  Feb 16"
#  }  
#  if(substr(clusters$dateSeq30min[i],6,7)=="03") {
#    clusters$family[i] <- "j  Mar 16"
#  }
#  if(substr(clusters$dateSeq30min[i],6,7)=="04") {
#    clusters$family[i] <- "k  Apr 16"
#  }
#  if(substr(clusters$dateSeq30min[i],6,7)=="05") {
#    clusters$family[i] <- "l  May 16"
#  } 
#}

# generate a cluster name list
#cluster_names <- NULL
#for(i in 1:60) {
#  cluster_names <- c(cluster_names, paste("cluster",i,sep = ""))
#}

# WARNING this code takes about 4 hours to run for 1 million minutes  
#data_df <- NULL
#for(i in 1:nrow(clusters)) {
#  a <- matrix(ncol = 4, nrow = 60, "NA") 
#  a <- data.frame(a)
#  colnames(a) <- c("family", "item", "score", "value")
#  a$family <- clusters$family[i] 
#  a$score <- cluster_names
#  a$item <- clusters$item[i] 
#  a$value <- t(unname(clusters[i,1:60]))
#  data_df <- rbind(data_df, a)
#  print(i)
#}
#write.csv(data_df, "polarHistograms/polar_data.csv", row.names = FALSE)

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

# colours for each class
insect_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
bird_col <- "#009E73"
cicada_col <- "#E69F00"
quiet_col <- "#999999"
plane_col <- "#CC79A7"
na_col <- "white"
# choose a cluster, clust is used to label the plots
clust <- "cluster48"
# Choose a colour
col <- cicada_col
# set the scale, the first for Gympie the second Woondum
# set to 7 for cluster 43 ; 20 for cluster 13
# set to 20 for cluster 37
# set to 12 for cluster 48
# the number will not be greater than 30
scale <- c(12, 12) 

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

n <- 1:length(a)
j <- 0
for(i in a[n]) {
  j <- j + 1
  r <- n[j]
  title <- paste(i)
  subtitle <- paste("Cluster", substr(clust,8,10))
  file_title <- paste("polarHistograms/rose_plot_", i,"_tiff",clust, ".tiff",sep = "")
  z <- polarHistogram(get(i), familyLabels = F, normalised = F, 
                      colour = col, 
                      innerRadius = 0, outerRadius = 1,
                      guides = seq(2,(scale[1]-1),2), 
                      labels = TRUE)
  z <- z + ggtitle(bquote(atop(.(title), atop(italic(.(subtitle)), "")))) +
    theme(plot.title = element_text(hjust = 1.2)) 
  z <- z + theme(plot.title = element_text(size=22))
  z <- z + theme(plot.title = element_text(margin=margin(b = -50, unit = "pt")))
  #z <- z + annotate('text', x = 0, y = 0, label = "Value~is~sigma~R^{2}==0.6 ", parse = TRUE,size=20)
  #z <- z + annotate('text', x = 0, y = 0, label = "\u2600", parse = TRUE,size=20)
  # add the sun symbol x is angle and y the fraction of the radius
  z <- z + geom_point(data=data.frame(x=c(1)), 
                      aes(x = ((sunrise_min[r]*58/1440)-0.1), y = 0.94), 
                      shape="\u2600", size=10)
  # add the moon symbol 
  z <- z + geom_point(data=data.frame(x=c(1)), 
                      aes(x = ((sunset_min[r]*58/1440)-0.48), y = 0.94), 
                      shape="\u263D", size=10)
  #z <- z + geom_point(x=150,y=0,shape="\u2600", size=20)
  z <- z + theme(plot.background = element_rect(fill = "transparent", colour = NA))
  # invisible is used to stop print opening a print window
  invisible(print(z))
  ggsave(file_title, width = 7.5, height = 7.5, unit = "in", dpi = 300)
  dev.off()
}

# this code could be used to investigate patterns
b <- barplot(data$value)
axis(side=1,at=b[c(seq(1,nrow(data),48))],
     labels=seq(1,(nrow(data)/48),1))

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# finding values of specific peaks in rose plots
cluster <- "cluster44"
month <- "i  Feb 16"
time <- "18:45"
site <- "WoondumNP"
gym_df <- df[1:(nrow(df)/2),]
woon_df <- df[(nrow(df)/2+1):nrow(df),]

if(site=="GympieNP") {
a <- which(gym_df$score==cluster)
b <- which(gym_df$family==month)
c <- which(gym_df$item==time)
d <- intersect(a,b)
e <- intersect(d,c)
}
if(site=="WoondumNP") {
  a <- which(woon_df$score==cluster)
  b <- which(woon_df$family==month)
  c <- which(woon_df$item==time)
  d <- intersect(a,b)
  e <- intersect(d,c)
  peak_value <- sum(woon_df$value[e])
}

# viewport experimentation
#library(grid)
#
#png("tester.png", height = 1000, width = 1000)
#subvp <-viewport(width=1, height=1,x=0.5,y=0.5)
#t
#print(s, vp=subvp)
#print(r, vp=subvp)
#print(q, vp=subvp)
#print(p, vp=subvp)
#dev.off()

#png("tester_p.png", height = 1000, width = 1000)
#print(p)
#dev.off()
#png("tester_q.png", height = 1000, width = 1000)
#print(q)
#dev.off()
#png("tester_r.png", height = 1000, width = 1000)
#print(r)
#dev.off()
#png("tester_s.png", height = 1000, width = 1000)
#print(s)
#dev.off()
#png("tester_t.png", height = 1000, width = 1000)
#print(t)
#dev.off()

#startDate = as.POSIXct("2013-12-23 00:15:00")
#endDate = as.POSIXct("2013-12-23 23:45:00")
#dateSeq5sec = seq(from=startDate, to=endDate, by="1800 sec")

#times <- substr(dateSeq5sec, 12, 16)
