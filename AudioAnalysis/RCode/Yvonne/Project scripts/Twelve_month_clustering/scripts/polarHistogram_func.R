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

# Convert dates to YYYYMMDD format
#for (i in 1:length(dates)) {
#  x <- "-"
# date.list[i] <- gsub(x, "", date.list[i])  
#}
#dates <- date.list

# duplicate dates 1440 times
dates <- rep(date.list, each = 1440)
dates <- rep(dates, 2)
# Add site and dates columns to dataframe
sites <- c("Gympie NP", "Woondum NP")
sites <- rep(sites, each=length(dates)/2)
cluster_list <- cbind(cluster_list, sites, dates)
cluster_list <- data.frame(cluster_list)
site <- unique(sites)

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

View(cluster)

#cluster$month <- "NA"
#for(i in 1:length(cluster$clust)) {
#  if(substr(cluster$date[i],6,7)=="06") {
#    cluster$month[i] <- "Jun"
#  }  
#  if(substr(cluster$date[i],6,7)=="07") {
#    cluster$month[i] <- "Jul"
#  }
#  if(substr(cluster$date[i],6,7)=="08") {
#    cluster$month[i] <- "Aug"
#  }
#  if(substr(cluster$date[i],6,7)=="09") {
#    cluster$month[i] <- "Sept"
#  }
#  if(substr(cluster$date[i],6,7)=="10") {
#    cluster$month[i] <- "Oct"
#  }  
#  if(substr(cluster$date[i],6,7)=="11") {
#    cluster$month[i] <- "Nov"
#  }
#  if(substr(cluster$date[i],6,7)=="12") {
#    cluster$month[i] <- "Dec"
#  }
#  if(substr(cluster$date[i],6,7)=="01") {
#    cluster$month[i] <- "Jan"
#  }
#  if(substr(cluster$date[i],6,7)=="02") {
#    cluster$month[i] <- "Feb"
#  }  
#  if(substr(cluster$date[i],6,7)=="03") {
#    cluster$month[i] <- "Mar"
#  }
#  if(substr(cluster$date[i],6,7)=="04") {
#    cluster$month[i] <- "Apr"
#  }
#  if(substr(cluster$date[i],6,7)=="05") {
#    cluster$month[i] <- "May"
#  }
#}
cluster <- data.frame(cluster)
length <- length(cluster$clust)/2
gym_clust <- cluster[1:length,]
woon_clust <- cluster[(length+1):(length*2),]
View(gym_clust)

gym_clust$score <- gym_clust$clust
gym_clust$value <- gym_clust$count
gym_clust$family <- gym_clust$month
gym_clust$item <- gym_clust$date

woon_clust$score <- woon_clust$clust
woon_clust$value <- woon_clust$count
woon_clust$family <- woon_clust$month
woon_clust$item <- woon_clust$date

rain <- c(59,18,10,54,2,21,38,60)
wind <- c(42,47,51,56,52,45,8,40,24,19,46,28,9,25,30,20)
birds <- c(58,43,57,37,11,3,33,15,14,39,4)
insects <- c(17,1,27,22,26,29)
cicada <- c(48,34,44,7,12,32,16)
planes <- c(49,23)
quiet <- c(6,53,36,31,50,35,55,41,13,5)

gympie_clusters <- matrix(ncol = 4, nrow = 398*7, "NA") 
gympie_clusters <- data.frame(gympie_clusters)
colnames(gympie_clusters) <- c("score","value","family","item")

class <- c("rain","birds","insects","cicada","planes","quiet","wind")
class <- rep(class, 398)
gympie_clusters$score <- class
dates1 <- rep(date.list, each=7)
gympie_clusters$dates <- dates1
gympie_clusters$item <- substr(gympie_clusters$dates,9,10)
gympie_clusters$family <- as.character(gympie_clusters$family)
gympie_clusters$item <- as.character(gympie_clusters$item)
gympie_clusters$value <- as.numeric(gympie_clusters$value)
for(i in 1:length(gympie_clusters$item)) {
  if((substr(gympie_clusters$dates[i],6,7)=="06") &
     (substr(gympie_clusters$dates[i],1,4)=="2015")) {
    gympie_clusters$family[i] <- "a  Jun 15"
  }  
  if((substr(gympie_clusters$dates[i],6,7)=="06") &
     (substr(gympie_clusters$dates[i],1,4)=="2016")) {
    gympie_clusters$family[i] <- "m  Jun 16"
  } 
  if((substr(gympie_clusters$dates[i],6,7)=="07") &
     (substr(gympie_clusters$dates[i],1,4)=="2015")) {
    gympie_clusters$family[i] <- "b  Jul 15"
  }  
  if((substr(gympie_clusters$dates[i],6,7)=="07") &
     (substr(gympie_clusters$dates[i],1,4)=="2016")) {
    gympie_clusters$family[i] <- "n  Jun 16"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="08") {
    gympie_clusters$family[i] <- "c  Aug 15"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="09") {
    gympie_clusters$family[i] <- "d  Sept 15"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="10") {
    gympie_clusters$family[i] <- "e  Oct 15"
  }  
  if(substr(gympie_clusters$dates[i],6,7)=="11") {
    gympie_clusters$family[i] <- "f  Nov 15"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="12") {
    gympie_clusters$family[i] <- "g  Dec 15"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="01") {
    gympie_clusters$family[i] <- "h  Jan 16"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="02") {
    gympie_clusters$family[i] <- "i  Feb 16"
  }  
  if(substr(gympie_clusters$dates[i],6,7)=="03") {
    gympie_clusters$family[i] <- "j  Mar 16"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="04") {
    gympie_clusters$family[i] <- "k  Apr 16"
  }
  if(substr(gympie_clusters$dates[i],6,7)=="05") {
    gympie_clusters$family[i] <- "l  May 16"
  } 
}

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

View(gympie_clusters)


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
    woondum_clusters$family[i] <- "a  Jun 15"
  }  
  if((substr(woondum_clusters$dates[i],6,7)=="06") &
     (substr(woondum_clusters$dates[i],1,4)=="2016")) {
    woondum_clusters$family[i] <- "m  Jun 16"
  } 
  if((substr(woondum_clusters$dates[i],6,7)=="07") &
     (substr(woondum_clusters$dates[i],1,4)=="2015")) {
    woondum_clusters$family[i] <- "b  Jul 15"
  }  
  if((substr(woondum_clusters$dates[i],6,7)=="07") &
     (substr(woondum_clusters$dates[i],1,4)=="2016")) {
    woondum_clusters$family[i] <- "n  Jun 16"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="08") {
    woondum_clusters$family[i] <- "c  Aug 15"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="09") {
    woondum_clusters$family[i] <- "d  Sept 15"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="10") {
    woondum_clusters$family[i] <- "e  Oct 15"
  }  
  if(substr(woondum_clusters$dates[i],6,7)=="11") {
    woondum_clusters$family[i] <- "f  Nov 15"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="12") {
    woondum_clusters$family[i] <- "g  Dec 15"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="01") {
    woondum_clusters$family[i] <- "h  Jan 16"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="02") {
    woondum_clusters$family[i] <- "i  Feb 16"
  }  
  if(substr(woondum_clusters$dates[i],6,7)=="03") {
    woondum_clusters$family[i] <- "j  Mar 16"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="04") {
    woondum_clusters$family[i] <- "k  Apr 16"
  }
  if(substr(woondum_clusters$dates[i],6,7)=="05") {
    woondum_clusters$family[i] <- "l  May 16"
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

View(woondum_clusters)

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
source("polarHistogram.R")

polarHistogram <-function (df, family = NULL, 
                           columnNames = NULL, binSize = 1,
                           spaceItem = 0.2, spaceFamily = 0, 
                           innerRadius = 0, 
                           outerRadius = 1,
                           guides = c(40, 60, 80), 
                           alphaStart = -0.3, 
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
                       data = familyLabelsDF, y = 1.22, size = 6.5)
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

# fake data for polarHistogram()
# Christophe Ladroue
library(plyr)
library(ggplot2)
source("polarHistogram.R")

# a little helper that generates random names for families and items.
randomName<-function(n=1,syllables=3){
  vowels<-c("a","e","i","o","u","y")
  consonants<-setdiff(letters,vowels)
  replicate(n,
            paste(
              rbind(sample(consonants,syllables,replace=TRUE),
                    sample(vowels,syllables,replace=TRUE)),
              sep='',collapse='')
  )
}

set.seed(42)

nFamily<-20
nItemPerFamily<-sample(1:6,nFamily,replace=TRUE)
nValues<-3

df<-data.frame(
  family=rep(randomName(nFamily),nItemPerFamily),
  item=randomName(sum(nItemPerFamily),2))

df<-cbind(df,as.data.frame(matrix(runif(nrow(df)*nValues),nrow=nrow(df),ncol=nValues)))

library(reshape)
df<-melt(df,c("family","item"), variable_name="score") # from wide to long
p<-polarHistogram(df, familyLabel=T)
print(p)

gympie_clusters1 <- gympie_clusters[1:420,]
gympie_clusters2 <- gympie_clusters[421:840,]
gympie_clusters3 <- gympie_clusters[841:1260,]
gympie_clusters4 <- gympie_clusters[1261:1680,]
gympie_clusters5 <- gympie_clusters[1681:2100,]
gympie_clusters6 <- gympie_clusters[2101:2520,]
gympie_clusters7 <- gympie_clusters[2520:length(gympie_clusters$score),]

require(gridExtra)
p1 <- polarHistogram(gympie_clusters1, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p1 <- p1 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p1) 
ggsave('polarHistograms/Gympie_polarHistogram_p1.png', 
       width = 9, height = 9, dpi = 400, bg = "transparent")

pdf("polarHistograms/Gympie_polarHistogram_p1.pdf")
print(p1)
dev.off()

p2 <- polarHistogram(gympie_clusters2, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p2 <- p2 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p2)
ggsave('polarHistograms/Gympie_polarHistogram_p2.png', 
       width = 9, height = 9, dpi = 400, bg = "transparent")

pdf("Gympie_polarHistogram_p2.pdf")
print(p2)
dev.off()

p3 <- polarHistogram(gympie_clusters3, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p3 <- p3 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p3)
ggsave('polarHistograms/Gympie_polarHistogram_p3.png', 
       width = 9, height = 9, dpi = 400)

pdf("Gympie_polarHistogram_p3.pdf")
print(p3)
dev.off()

p4 <- polarHistogram(gympie_clusters4, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p4 <- p4 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p4)
ggsave('polarHistograms/Gympie_polarHistogram_p4.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/Gympie_polarHistogram_p4.pdf")
print(p4)
dev.off()

p5 <- polarHistogram(gympie_clusters5, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p5 <- p5 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p5)
ggsave('polarHistograms/Gympie_polarHistogram_p5.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/Gympie_polarHistogram_p5.pdf")
print(p5)
dev.off()

p6 <- polarHistogram(gympie_clusters6, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
p6 <- p6 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p6)
ggsave('polarHistograms/Gympie_polarHistogram_p6.png', 
       width = 9, height = 9, dpi = 400)


pdf("polarHistograms/Gympie_polarHistogram_p6.pdf")
print(p6)
dev.off()

p7 <- polarHistogram(gympie_clusters7, 
                     familyLabels = TRUE,
                     circleProportion 
                     = 0.92*length(gympie_clusters7$score)/420,
                     normalised = FALSE)
p7 <- p7 + ggtitle("Gympie NP") + theme(plot.title = element_text(size=22))
print(p7)
ggsave('polarHistograms/Gympie_polarHistogram_p7.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/Gympie_polarHistogram_p7.pdf")
print(p7)
dev.off()

grid.arrange(p1, p2, p3, p4, p5, p6, ncol=3)


woondum_clusters1 <- woondum_clusters[1:420,]
woondum_clusters2 <- woondum_clusters[421:840,]
woondum_clusters3 <- woondum_clusters[841:1260,]
woondum_clusters4 <- woondum_clusters[1261:1680,]
woondum_clusters5 <- woondum_clusters[1681:2100,]
woondum_clusters6 <- woondum_clusters[2101:2520,]
woondum_clusters7 <- woondum_clusters[2520:length(woondum_clusters$score),]

require(gridExtra)
w1 <- polarHistogram(woondum_clusters1, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w1 <- w1 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w1)
ggsave('polarHistograms/Woondum_polarHistogram_p1.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/woondum_polarHistogram_p1.pdf")
print(w1)
dev.off()

w2 <- polarHistogram(woondum_clusters2, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w2 <- w2 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w2)
ggsave('polarHistograms/Woondum_polarHistogram_p2.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/woondum_polarHistogram_p2.pdf")
print(w2)
dev.off()

w3 <- polarHistogram(woondum_clusters3, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w3 <- w3 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w3)
ggsave('polarHistograms/Woondum_polarHistogram_p3.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/woondum_polarHistogram_p3.pdf")
print(w3)
dev.off()

w4 <- polarHistogram(woondum_clusters4, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w4 <- w4 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w4)
ggsave('polarHistograms/Woondum_polarHistogram_p4.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/woondum_polarHistogram_p4.pdf")
print(w4)
dev.off()

w5 <- polarHistogram(woondum_clusters5, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w5 <- w5 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w5)
ggsave('polarHistograms/Woondum_polarHistogram_p5.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/woondum_polarHistogram_p5.pdf")
print(w5)
dev.off()

w6 <- polarHistogram(woondum_clusters6, 
                     familyLabels = TRUE,
                     circleProportion = 0.92,
                     normalised = FALSE)
w6 <- w6 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w6)
ggsave('polarHistograms/Woondum_polarHistogram_p6.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/woondum_polarHistogram_p6.pdf")
print(w6)
dev.off()

w7 <- polarHistogram(woondum_clusters7, 
                     familyLabels = TRUE,
                     circleProportion 
                     = 0.92*length(woondum_clusters7$score)/420,
                     normalised = FALSE)
w7 <- w7 + ggtitle("Woondum NP") + theme(plot.title = element_text(size=22))
print(w7)
ggsave('polarHistograms/Woondum_polarHistogram_p7.png', 
       width = 9, height = 9, dpi = 400)

pdf("polarHistograms/woondum_polarHistogram_p7.pdf")
print(w7)
dev.off()

grid.arrange(w1, w2, w3, w4, w5, w6, w7, ncol=4)

