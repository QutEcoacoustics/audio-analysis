clusters <- read.csv("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/weather/clusters_10minute.csv", header = TRUE)
# set the number of periods per day
periods <- 144
# generate a time sequence
startDate = as.POSIXct("2015-06-22 00:05:00")
endDate = as.POSIXct("2016-07-23 23:55:00")
dateSeq5sec = seq(from=startDate, to=endDate, by="600 sec")
head(dateSeq5sec)
times <- substr(dateSeq5sec, 12, 16)
head(times)
tail(times)
# duplicate times to the length of the data.frame
times <- rep(times, (nrow(clusters)/length(times)) )

# fill the 'family' column with a month description
for(i in 1:nrow(clusters)) {
  if((substr(clusters$dateSeq10min[i],6,7)=="06") &
     (substr(clusters$dateSeq10min[i],1,4)=="2015")) {
    clusters$family[i] <- "a  Jun 15"
  }  
  if((substr(clusters$dateSeq10min[i],6,7)=="06") &
     (substr(clusters$dateSeq10min[i],1,4)=="2016")) {
    clusters$family[i] <- "m  Jun 16"
  } 
  if((substr(clusters$dateSeq10min[i],6,7)=="07") &
     (substr(clusters$dateSeq10min[i],1,4)=="2015")) {
    clusters$family[i] <- "b  Jul 15"
  }  
  if((substr(clusters$dateSeq10min[i],6,7)=="07") &
     (substr(clusters$dateSeq10min[i],1,4)=="2016")) {
    clusters$family[i] <- "n  Jul 16"
  }
  if(substr(clusters$dateSeq10min[i],6,7)=="08") {
    clusters$family[i] <- "c  Aug 15"
  }
  if(substr(clusters$dateSeq10min[i],6,7)=="09") {
    clusters$family[i] <- "d  Sept 15"
  }
  if(substr(clusters$dateSeq10min[i],6,7)=="10") {
    clusters$family[i] <- "e  Oct 15"
  }  
  if(substr(clusters$dateSeq10min[i],6,7)=="11") {
    clusters$family[i] <- "f  Nov 15"
  }
  if(substr(clusters$dateSeq10min[i],6,7)=="12") {
    clusters$family[i] <- "g  Dec 15"
  }
  if(substr(clusters$dateSeq10min[i],6,7)=="01") {
    clusters$family[i] <- "h  Jan 16"
  }
  if(substr(clusters$dateSeq10min[i],6,7)=="02") {
    clusters$family[i] <- "i  Feb 16"
  }  
  if(substr(clusters$dateSeq10min[i],6,7)=="03") {
    clusters$family[i] <- "j  Mar 16"
  }
  if(substr(clusters$dateSeq10min[i],6,7)=="04") {
    clusters$family[i] <- "k  Apr 16"
  }
  if(substr(clusters$dateSeq10min[i],6,7)=="05") {
    clusters$family[i] <- "l  May 16"
  } 
}

# generate a cluster name list
cluster_names <- NULL
for(i in 1:60) {
  cluster_names <- c(cluster_names, paste("cluster",i,sep = ""))
}

# WARNING this code takes about 2 hours to run for 1 million minutes  
data_df <- NULL
for(i in 1:10000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar1_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 10001:20000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar2_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 20001:30000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar3_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 30001:40000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar4_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 40001:50000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar5_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 50001:60000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar6_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 60001:70000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar7_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 70001:80000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar8_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 80001:90000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar9_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 90001:100000) { #nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar10_10_data.csv", row.names = FALSE)

data_df <- NULL
for(i in 100001:nrow(clusters)) {
  a <- matrix(ncol = 4, nrow = 60, "NA") 
  a <- data.frame(a)
  colnames(a) <- c("family", "item", "score", "value")
  a$family <- clusters$family[i] 
  a$score <- cluster_names
  a$item <- times[i]
  a$value <- t(unname(clusters[i,1:60]))
  data_df <- rbind(data_df, a)
  print(i)
}
write.csv(data_df, "polarHistograms/polar11_10_data.csv", row.names = FALSE)
#shell.exec("C:\\Work\\Xeno Canto\\Cicadas\\003CS Double drummer.mp3")

data_df <- NULL
data_df1 <- read.csv("polarHistograms/polar1_10_data.csv", header = T)
data_df <- data_df1
rm(data_df1)
data_df2 <- read.csv("polarHistograms/polar2_10_data.csv", header = T)
data_df <- rbind(data_df, data_df2)
rm(data_df2)
data_df3 <- read.csv("polarHistograms/polar3_10_data.csv", header = T)
data_df <- rbind(data_df, data_df3)
rm(data_df3)
data_df4 <- read.csv("polarHistograms/polar4_10_data.csv", header = T)
data_df <- rbind(data_df, data_df4)
rm(data_df4)
data_df5 <- read.csv("polarHistograms/polar5_10_data.csv", header = T)
data_df <- rbind(data_df, data_df5)
rm(data_df5)
data_df6 <- read.csv("polarHistograms/polar6_10_data.csv", header = T)
data_df <- rbind(data_df, data_df6)
rm(data_df6)
data_df7 <- read.csv("polarHistograms/polar7_10_data.csv", header = T)
data_df <- rbind(data_df, data_df7)
rm(data_df7)
data_df8 <- read.csv("polarHistograms/polar8_10_data.csv", header = T)
data_df <- rbind(data_df, data_df8)
rm(data_df8)
data_df9 <- read.csv("polarHistograms/polar9_10_data.csv", header = T)
data_df <- rbind(data_df, data_df9)
rm(data_df9)
data_df10 <- read.csv("polarHistograms/polar10_10_data.csv", header = T)
data_df <- rbind(data_df, data_df10)
rm(data_df10)
data_df11 <- read.csv("polarHistograms/polar11_10_data.csv", header = T)
data_df <- rbind(data_df, data_df11)
rm(data_df11)

write.csv(data_df, "polarHistograms/polar_data10.csv", row.names = F)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Ten minute Rose plots ------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
#rm(list = ls())
list1 <- c("cluster37", "cluster44", "cluster48")
list1 <- c("cluster11")
list1 <- c("cluster43")

# Colours for each class
insect_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
bird_col <- "#009E73"
cicada_col <- "#E69F00"
quiet_col <- "#999999"
plane_col <- "#CC79A7"
na_col <- "white"

for(t in 1:length(list1)) {
  clust <- list1[t]
  
  if(clust=="cluster37"|clust=="cluster43") {
    col <- bird_col
    if(clust=="cluster37") {
      scale <- c(7, 7)  
    }
    if(clust=="cluster43") {
      scale <- c(3, 3)  
    }
    # The selection August 2015 to December 2015 at Gympie
    selection <- 3:7 
    circleProportion <- 0.5
    months <- c("Aug 2015", "Sept 2015", "Oct 2015",
                "Nov 2015", "Dec 2015")
  }
  if(clust=="cluster11") {
    col <- bird_col
    scale <- c(7, 7)
    # The selection August 2015 to December 2015 at Gympie
    selection <- 1:28 
    circleProportion <- 0.5
    months <- c("June 2015", "July 2015",
                "Aug 2015", "Sept 2015", "Oct 2015",
                "Nov 2015", "Dec 2015", "Jan 2016",
                "Feb 2016", "Mar 2016", "Apr 2016", "May 2016", 
                "June 2016", "Jul 2016")
    months <- rep(months, 2)
  }
  if(clust=="cluster44"|clust=="cluster48") {
    col <- cicada_col
    circleProportion <- 1
    months <- c("Dec 2015", "Jan 2016", "Feb 2016")
    if(clust=="cluster44") {
      scale <- c(7, 7)
      # The selection - December 2015 to February 2015 at Woondum
      selection <- 21:23 
    }
    if(clust=="cluster48") {
      scale <- c(5, 5)
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
                             hjust = hjust), y = 1.02, size = 4, vjust = 0.5, data = dfItemLabels)
    # this code prints the guidelines  
    p <- p + geom_segment(aes(x = xmin, xend = xend, y = y, yend = y),
                          colour = "white", data = guidesDF)
    
    if(normalised)
      guideLabels <- data.frame(x = 0, y = affine(1 - guides/100),
                                label = paste(guides, "% ", sep = ""))
    else
      guideLabels <- data.frame(x = 0, y = affine(1 - guides/maxFamily),
                                label = paste(guides, " ", sep = ""), size = 8)
    
    p <- p + geom_text(aes(x = x, y = y, label = label), data = guideLabels,
                       angle = -alphaStart * 180/pi, hjust = 1, size = 8)
    if (familyLabels) {
      familyLabelsDF <- aggregate(xmin ~ family, data = df,
                                  FUN = function(s) mean(s + binSize))
      familyLabelsDF <- within(familyLabelsDF, {
        x <- xmin
        angle <- xmin * (-360/totalLength) - alphaStart * 180/pi
      })
      p <- p + geom_text(aes(x = x, label = family, angle = angle),
                         data = familyLabelsDF, y = 1.35, size = 4)
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
  
  df <- read.csv("polarHistograms/polar_data10.csv", header = T)
  
  df$item <- as.character(df$item)
  df$family <- as.character(df$family)
  df$score <- as.character(df$score)
  df$value <- as.numeric(df$value)
  
  b <- nrow(df)
  gym_df <- df[1:(b/2),]
  won_df <- df[(b/2+1):b,]
  
  a <- which(gym_df$score==clust)
  gym_df <- gym_df[a,]
  
  a <- which(won_df$score==clust)
  won_df <- won_df[a,]
  
  # June 2015
  a <- which(gym_df$family=="a  Jun 15")
  b <- c(min(a), max(a))
  GympieNP_June2015 <- gym_df[b[1]:b[2],]
  # determine the number of days
  n <- nrow(GympieNP_June2015)/periods
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
  n <- nrow(WoondumNP_June2015)/periods
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
  n <- nrow(GympieNP_July2015)/periods
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
  n <- nrow(WoondumNP_July2015)/periods
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
  n <- nrow(GympieNP_August2015)/periods
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
  n <- nrow(WoondumNP_August2015)/periods
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
  n <- (nrow(GympieNP_September2015)/periods) 
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
  n <- nrow(WoondumNP_September2015)/periods
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
  n <- nrow(GympieNP_October2015)/periods
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
  n <- nrow(WoondumNP_October2015)/periods
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
  n <- nrow(GympieNP_November2015)/periods
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
  n <- nrow(WoondumNP_November2015)/periods
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
  n <- nrow(GympieNP_December2015)/periods
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
  n <- nrow(WoondumNP_December2015)/periods
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
  n <- nrow(GympieNP_January2016)/periods
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
  n <- nrow(WoondumNP_January2016)/periods
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
  n <- nrow(GympieNP_February2016)/periods
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
  n <- nrow(WoondumNP_February2016)/periods
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
  n <- nrow(GympieNP_March2016)/periods
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
  n <- nrow(WoondumNP_March2016)/periods
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
  n <- nrow(GympieNP_April2016)/periods
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
  n <- nrow(WoondumNP_April2016)/periods
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
  n <- nrow(GympieNP_May2016)/periods
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
  n <- nrow(WoondumNP_May2016)/periods
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
  n <- nrow(GympieNP_June2016)/periods
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
  n <- nrow(WoondumNP_June2016)/periods
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
  n <- nrow(GympieNP_July2016)/periods
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
  n <- nrow(WoondumNP_July2016)/periods
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
  # Set the layout matrix to divide page into two frames one
  # for the plot and one for the table
  layout.show(11)
  
  n <- 1:nrow(a37)
  j <- 0
  for(i in a37[n,1]) {
    j <- j + 1
    r <- a37[j,2]
    title <- paste(i)
    subtitle <- paste("Cluster", substr(clust,8,10))
    file_title <- paste("polarHistograms/article/rose_plot_", i,"_10_",clust, ".tiff",sep = "")
    data <- get(i)
    data <- data.frame(data)
    if(clust=="cluster37") {
      list <- c("00:05","00:15","00:25","00:35","00:45","00:55",
                "01:05","01:15","01:25","01:35","01:45","01:55",
                "02:05","02:15","02:25","02:35","02:45","02:55",
                "03:05","03:15","03:25","03:35","03:45","03:55",
                "04:05","04:15","04:25","04:35","04:45","04:55",
                "05:05","05:15","05:25","05:35","05:45","05:55",
                "06:05","06:15","06:25","06:35","06:45","06:55",
                "07:05","07:15","07:25","07:35","07:45","07:55",
                "08:05","08:15","08:25","08:35","08:45","08:55",
                "09:05","09:15","09:25","09:35","09:45","09:55",
                "10:05","10:15","10:25","10:35","10:45","10:55",
                "11:05","11:15","11:25","11:35","11:45","11:55",
                "12:05","12:15","12:25","12:35")
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
    if(clust=="cluster44"|clust=="cluster48"|clust=="cluster11"|clust=="cluster43"|clust=="cluster43") {
      ab <- 1:nrow(data)
    }
    if(clust=="cluster37") {
      z <- polarHistogram(data[ab,], familyLabels = F, normalised = F, 
                          colour = col, circleProportion = 0.5,
                          innerRadius = 0, outerRadius = 1,
                          guides = seq(1, (scale[1]-0.5), 1), 
                          labels = TRUE)
    }
    if(clust=="cluster44"|clust=="cluster48"|clust=="cluster11"|clust=="cluster43") {
      z <- polarHistogram(data[ab,], familyLabels = F, normalised = F, 
                          colour = col, circleProportion = 1,
                          innerRadius = 0, outerRadius = 1,
                          guides = seq(1, (scale[1]-0.5), 1), 
                          labels = TRUE)
    }
    #z <- z + ggtitle(bquote(atop(.(title), atop(italic(.(subtitle)), ""))))
    #z <- z + theme(plot.title = element_text(size=22))
    #z <- z + theme(plot.title = element_text(margin=margin(b = -50, unit = "pt")))
    # add the sun symbol x is angle and y the fraction of the radius
    z <- z + geom_point(data=data.frame(x=c(1)), 
                        aes(x = 3*(sunrise_min[r]*60/1440-0.5), y = 0.95), 
                        shape="\u2600", size=9)
    if(clust=="cluster44"|clust=="cluster48"|clust=="cluster11"|clust=="cluster43") {
      # add the moon symbol 
      z <- z + geom_point(data=data.frame(x=c(1)), 
                          aes(x = 3*(sunset_min[r]*60/1440-2), y = 0.95), 
                          shape="\u263D", size=9)
    } 
    if(clust=="cluster37"|clust=="cluster44"|clust=="cluster11"|clust=="cluster43"){
      # add the month label
      z <- z + annotate("text", x=1.2, y=0.55, 
                        label= months[j], size = 9)
    }
    if(clust=="cluster48") {
      # add the month label
      z <- z + annotate("text", x=1.2, y=0.55, 
                        label= months[j], size = 9)
      
    }
    #z <- z + geom_point(x=150,y=0,shape="\u2600", size=20)
    z <- z + theme(plot.background = element_rect(fill = "transparent", 
                                                  colour = NA))
    if(clust=="cluster37") {
      # plot margin controls the top, right, bottom, and left margins
      z <- z + theme(plot.margin=unit(c(-15,-13, -15,-95),"mm"))  
    }
    if(clust=="cluster44"|clust=="cluster48"|clust=="cluster11"|clust=="cluster43") {
      # plot margin controls the top, right, bottom, and left margins
      z <- z + theme(plot.margin=unit(c(-14,-19, -15,-21),"mm"))  
    }
    #invisible is used to stop print opening a print window
    invisible(print(z))
    if(clust=="cluster37") {
      ggsave(file_title, width = 10.2, height = 19.05, units = "cm", 
             dpi = 300)  
    }
    if(clust=="cluster44"|clust=="cluster48"|clust=="cluster11"
       |clust=="cluster43") {
      ggsave(file_title, width = 18.05, height = 19.05, units = "cm", 
             dpi = 300)  
    }
    dev.off()
  }
}

# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%