colours <- c("yellow","darkgreen","red","orange","skyblue1",
             "seashell3","darkorchid1","hotpink","green","mediumblue",
             "cyan","slateblue3", "peru", "snow4", "plum3",
             "peachpuff","black")
#library(scales)
#show_col(colours)

pch <- 20
# Generate a cluster list with sites and dates 

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
a <- which(cluster_list$site=="GympieNP")
Gym_cluster_list <- cluster_list[a, ]
a <- which(cluster_list$site=="WoondumNP")
Woon_cluster_list <- cluster_list[a, ]
rm(cluster_list)
# Prepare civil dawn, civil dusk and sunrise and sunset times
#civil_dawn <- read.csv("data/Sunrise_Sunset_Solar Noon_protected.csv", header=T)
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2016 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2016.csv")
civil_dawn <- rbind(civil_dawn_2015, civil_dawn_2016)

# set the start date in "YYYY-MM-DD" format
start_date <- "2015-06-22"
start <- as.POSIXct(start_date)
days <- 398
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
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# This function plots the points for an individual species 
bird_plot <- function(species, col, pch) {
  all <- NULL
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID")
  for(i in (b+1):length(kalscpe_data)) {
    a <- which(substr(kalscpe_data[,i],1,40)==species)
    all <- c(all, a)
  }
  birds <- kalscpe_data[all,]
  for(i in 1:nrow(birds)) {
    a <- which(as.character(substr(birds$IN.FILE[i],1,8))==paste(substr(civil_dawn$dates[reference[1]:nrow(civil_dawn)],1,4),
                                                                 substr(civil_dawn$dates[reference[1]:nrow(civil_dawn)],6,7),
                                                                 substr(civil_dawn$dates[reference[1]:nrow(civil_dawn)],9,10),
                                                                 sep = ""))
    if(length(a) > 0) {
      par(new=T)
      plot(x = birds$OFFSET[i]/60, y = -a, pch=pch, 
           ylim = ylim, col=col,
           xlim = xlim, yaxt="n", xaxt="n", ylab = "", xlab = "")  
    }
  }
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
xlim <- c(310, 402) # time
ylim1 <- c(-length(civ_dawn), -1) # days
ylim <- c(-56, 0)

species_plots <- function(species, col) {
  tiff(paste("morning_chorus_",species,".tiff",sep=""),
       height = 3600, width = 3000)
  par(mar=c(2,3,0.5,1), bty="n", cex=4)
  plot(x = civ_dawn, y = (ylim1[2]:ylim1[1]), type="n", axes=FALSE, ann=FALSE)
  #kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\cluster Gympie first week.csv", header = T)
  kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\20150621.csv", header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  kalscpe_data <- read.csv("C:\\Work\\Outputs\\GympieNP\\GympieNP_13 Sept\\GympieNP 13 Sept.csv", header = T)
  a <- which(kalscpe_data$CHANNEL=="0")
  kalscpe_data <- kalscpe_data[a,]
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150705\\GympieNP\\20150705.csv", header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150712\\GympieNP\\20150712.csv", header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150719\\GympieNP\\20150719.csv", header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150726\\GympieNP\\20150726.csv", header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150802\\GympieNP\\20150802.csv", header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150809\\GympieNP\\20150809.csv", header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150816\\GympieNP\\20150816.csv", header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 20)
  
  legend(x=(xlim[1]-1.5),y=ylim[2]+1,legend=species,
         col=col, pch = 20, bty = "n", cex = 1.2)
  text(x = -10, y = xlim[1]+20, paste(pch = 4, pch = 4, pch = 4, pch = 4))
  par(new=TRUE, cex = 4)
  plot(x = civ_dawn, y = (ylim1[2]:ylim1[1]), 
       type = "l", yaxt="n", xaxt="n", lty = "1F", 
       ylab = "", ylim = ylim, xlim = xlim)
  
  list <- c("01","05","10","15","20","25")
  at <- NULL
  label2 <- NULL
  a_ref <- which(substr(civil_dawn$dates,1,10)==substr(start,1,10))
  for(i in 1:length(list)) {
    a1 <- which(substr(civil_dawn$dates[a_ref:nrow(civil_dawn)],9,10)==list[i])
    at <- c(at, a1)
    b1 <- rep(list[i],length(a1))
    label2 <- c(label2, b1)
  }
  axis(side = 2, at = -at, line = -1.2, labels = label2, mgp=c(4,1,0))
  
  a <- which(substr(civil_dawn$dates[a_ref:nrow(civil_dawn)],9,10)=="01")
  a <- a[1:13]
  a <- c(-29, a)
  abline(h=-a, lty=5, lwd=0.1)
  
  labels <- c("Jun", "Jul", "Aug", "Sep", "Oct", "Nov",
              "Dec", "Jan", "Feb", "Mar", "Apr",
              "May", "Jun", "Jul")
  axis(side = 2, at = -a-14, line = NA, labels = labels, tick = F)
  par(new=T)
  plot(x = civ_dawn-5, y = -1:-length(civ_dawn), type = "l", 
       lty = 5, ylim = ylim, ylab="", yaxt="n",
       xaxt="n", lwd=3, xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+5, y = -1:-length(civ_dawn), type = "l", 
       lty = 5, ylim = ylim, ylab="", xaxt="n", lwd=3,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn-15, y = -1:-length(civ_dawn), 
       type = "l", lty=5, ylim = ylim, ylab="", lwd=3, 
       xaxt="n", yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+15, y = -1:-length(civ_dawn), type = "l", 
       lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn-25, y = -1:-length(civ_dawn), type = "l", 
       lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+25, y = -1:-length(civ_dawn), type = "l", 
       lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
       yaxt="n", xlim = xlim)
  at <- c(  0,  15,  30,  45,  60,  75,  90, 105, 
            120, 135, 150, 165, 180, 195, 210, 225, 
            240, 255, 270, 285, 300, 315, 330, 345, 
            360, 375, 390, 405, 420, 435, 450, 465, 
            480, 495, 510, 525, 540, 555, 570, 585, 
            600)
  at <- c(  0,  30,  60,  90, 
            120, 150, 180, 210, 
            240, 270, 300, 330, 
            360, 390, 420, 450, 
            480, 510, 540, 570, 
            600)
  labels1 <- c("12 am", "12:15 am","12:30 am", "12:45 am",
               "1 am",  "1:15 am",  "1:30 am",  "1:45 am",
               "2 am",  "2:15 am",  "2:30 am",  "2:45 am",
               "3 am",  "3:15 am",  "3:30 am",  "3:45 am",
               "4 am",  "4:15 am",  "4:30 am",  "4:45 am",
               "5 am",  "5:15 am",  "5:30 am",  "5:45 am",
               "6 am",  "6:15 am",  "6:30 am",  "6:45 am", 
               "7 am",  "7:15 am",  "7:30 am",  "7:45 am",
               "8 am",  "8:15 am",  "8:30 am",  "8:45 am",
               "9 am",  "9:15 am",  "9:30 am",  "9:45 am",
               "10 am")
  labels1 <- c("12 am", "12:30 am", 
               "1 am",  "1:30 am",  
               "2 am",  "2:30 am",  
               "3 am",  "3:30 am",  
               "4 am",  "4:30 am",  
               "5 am",  "5:30 am",  
               "6 am",  "6:30 am",  
               "7 am",  "7:30 am",  
               "8 am",  "8:30 am",  
               "9 am",  "9:30 am",  
               "10 am")
  abline(v=at)
  axis(side=1, at = at, labels = labels1, las=1)
  if(ylim[2]==0) {
    text(x = (civ_dawn[-ylim[2]+1] - 20), y = ylim[2]+0.4, "A")
    text(x = civ_dawn[-ylim[2]+1], y = ylim[2]+0.4, "B")
    text(x = civ_dawn[-ylim[2]+1] + 20, y = ylim[2]+0.4, "C")
  }
  
  if(ylim[2] < 0) {
    text(x = (civ_dawn[-ylim[2]] - 20), y = ylim[2]+1.4, "A")
    text(x = civ_dawn[-ylim[2]], y = ylim[2]+1.4, "B")
    text(x = civ_dawn[-ylim[2]] + 20, y = ylim[2]+1.4, "C")
  }
  
  dev.off()  
}

species_plots2 <- function(label, species, leg_label, col, ref) {
  par(mar=c(2,3,0.5,1), bty="n", cex=3)
  plot(x = civ_dawn, y = (ylim1[2]:ylim1[1]), type="n", axes=FALSE, ann=FALSE)
  bird_clust_plot(kalscpe_data)
  bird_plot(species, col, 15)
  legend(x=(xlim[1]-1.5),y=(ylim[2] + 1 - (ref - 1) * 2.1), legend=leg_label,
         col=col, pch = 15, bty = "n", cex = 1.8)
  text(x = -10, y = xlim[1]+20, paste(pch = 4, pch = 4, pch = 4, pch = 4))
  par(new=TRUE)
  #plot(x = civ_dawn, y = (ylim1[2]:ylim1[1]), 
  #     type = "l", yaxt="n", xaxt="n", lty = 3, col = "red",
  #     ylab = "", ylim = ylim, xlim = xlim, lwd = 3.2)
  
  list <- c("01","05","10","15","20","25")
  at <- NULL
  label2 <- NULL
  a_ref <- which(substr(civil_dawn$dates,1,10)==substr(start,1,10))
  for(i in 1:length(list)) {
    a1 <- which(substr(civil_dawn$dates[a_ref:nrow(civil_dawn)],9,10)==list[i])
    at <- c(at, a1)
    b1 <- rep(list[i],length(a1))
    label2 <- c(label2, b1)
  }
  axis(side = 2, at = -at, line = -1.2, labels = label2, mgp=c(4,1,0),
       cex.axis = 1.8, cex.lab = 1.2, las = 1)
  
  a <- which(substr(civil_dawn$dates[a_ref:nrow(civil_dawn)],9,10)=="01")
  a <- a[1:13]
  a <- c(-29, a)
  abline(h=-a, lty=5, lwd=3)
  
  labels <- c("Jun", "Jul", "Aug", "Sep", "Oct", "Nov",
              "Dec", "Jan", "Feb", "Mar", "Apr",
              "May", "Jun", "Jul")
  axis(side = 2, line = 0.65, at = -a-14, labels = labels, 
       tick = F, cex.axis = 1.8, cex.lab = 1.2)
  par(new=TRUE)
  plot(x = civ_dawn-5, y = -1:-length(civ_dawn), type = "l", 
       lty = 3, ylim = ylim, ylab="", yaxt="n",
       xaxt="n", lwd=6, xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+5, y = -1:-length(civ_dawn), type = "l", 
       lty = 3, ylim = ylim, ylab="", xaxt="n", lwd=6,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn-15, y = -1:-length(civ_dawn), 
       type = "l", lty=3, ylim = ylim, ylab="", lwd=6, 
       xaxt="n", yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+15, y = -1:-length(civ_dawn), type = "l", 
       lty=3, ylim = ylim, ylab="", xaxt="n", lwd=6,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn-25, y = -1:-length(civ_dawn), type = "l", 
       lty=3, ylim = ylim, ylab="", xaxt="n", lwd=6,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+25, y = -1:-length(civ_dawn), type = "l", 
       lty=3, ylim = ylim, ylab="", xaxt="n", lwd=6,
       yaxt="n", xlim = xlim)
  at <- c(  0,  15,  30,  45,  60,  75,  90, 105, 
          120, 135, 150, 165, 180, 195, 210, 225, 
          240, 255, 270, 285, 300, 315, 330, 345, 
          360, 375, 390, 405, 420, 435, 450, 465, 
          480, 495, 510, 525, 540, 555, 570, 585, 
          600)
  at <- c(  0,  30,  60,  90, 
            120, 150, 180, 210, 
            240, 270, 300, 330, 
            360, 390, 420, 450, 
            480, 510, 540, 570, 
            600)
  labels1 <- c("12 am", "12:15 am","12:30 am", "12:45 am",
               "1 am",  "1:15 am",  "1:30 am",  "1:45 am",
               "2 am",  "2:15 am",  "2:30 am",  "2:45 am",
               "3 am",  "3:15 am",  "3:30 am",  "3:45 am",
               "4 am",  "4:15 am",  "4:30 am",  "4:45 am",
               "5 am",  "5:15 am",  "5:30 am",  "5:45 am",
               "6 am",  "6:15 am",  "6:30 am",  "6:45 am", 
               "7 am",  "7:15 am",  "7:30 am",  "7:45 am",
               "8 am",  "8:15 am",  "8:30 am",  "8:45 am",
               "9 am",  "9:15 am",  "9:30 am",  "9:45 am",
               "10 am")
  labels1 <- c("12 am", "12:30 am", 
               "1 am",  "1:30 am",  
               "2 am",  "2:30 am",  
               "3 am",  "3:30 am",  
               "4 am",  "4:30 am",  
               "5 am",  "5:30 am",  
               "6 am",  "6:30 am",  
               "7 am",  "7:30 am",  
               "8 am",  "8:30 am",  
               "9 am",  "9:30 am",  
               "10 am")
  abline(v=at, lwd = 3)
  axis(side=1, at = at, labels = labels1,
       cex.axis = 1.8, line = -0.5, cex.lab = 1.2)
  if(ylim[2]==0) {
    text(x = (civ_dawn[-ylim[2]+1] - 20), y = ylim[2]-0.2, "A", cex = 1.4)
    text(x = civ_dawn[-ylim[2]+1], y = ylim[2]-0.2, "B", cex = 1.4)
    text(x = civ_dawn[-ylim[2]+1] + 20, y = ylim[2]-0.2, "C", cex = 1.4)
  }
  
  if(ylim[2] < 0) {
    text(x = (civ_dawn[-ylim[2]] - 20), y = ylim[2]-1.4, "A", cex = 1.4)
    text(x = civ_dawn[-ylim[2]], y = ylim[2]-1.4, "B", cex = 1.4)
    text(x = civ_dawn[-ylim[2]] + 20, y = ylim[2]-1.4, "C", cex = 1.4)
  }
  mtext(side = 3, label, cex = 6, line = -1.2)
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

# This function adds extra columns to the dataframe with individual species
# in each column
bird_clust_plot <- function(kalscpe_data) {
  list1 <- c("Scarlet Honeyeater",
             "Torresian Crow", "Pied Currawong",
             "Laughing Kookaburra", "Australian Magpie",
             "Southern Bookbook", "Lewin's Honeyeater",
             "Spotted Pardalote", "Yellow-tailed Black Cockatoo",
             "Silvereye", "White-throated Nightjar",
             "Russet-tailed Thrush", "Australian King Parrot",
             "Mistletoebird", "Brown Cuckoo-Dove",
             "Powerful Owl","Golden Whistler")
  col_names <- colnames(kalscpe_data)
  
  for(i in 1:length(list1)) {
    a <- NULL
    a <- grep(list1[i], kalscpe_data$MANUAL.ID, ignore.case = T)
    if(length(a) > 0) {
      kalscpe_data[a,(length(kalscpe_data)+1)] <- list1[i]
    }
    if(length(a) == 0) {
      kalscpe_data[,(length(kalscpe_data)+1)] <- ""
    }
  }
  # White-throated Treecreeper
  a <- NULL
  a1 <- NULL
  a2 <- NULL
  a1 <- grep("White-throated Treecreeper", kalscpe_data$MANUAL.ID, ignore.case = T)
  a2 <- grep("WTT", kalscpe_data$MANUAL.ID, ignore.case = T)
  a <- c(a1, a2)
  if(length(a) > 0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- "White-throated Treecreeper"
  }
  if(length(a)==0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- ""
  }
  
  a <- NULL
  a1 <- NULL
  a2 <- NULL
  a1 <- grep("Eastern Yellow Robin", kalscpe_data$MANUAL.ID, ignore.case = T)
  a2 <- grep("EYR", kalscpe_data$MANUAL.ID, ignore.case = T)
  a <- c(a1, a2)
  if(length(a) > 0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- "Eastern Yellow Robin"
  }
  if(length(a)==0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- ""
  }
  
  a1 <- NULL
  a2 <- NULL
  a3 <- NULL
  a4 <- NULL
  a <- NULL
  a1 <- grep("White-throated Honeyeater alarm", kalscpe_data$MANUAL.ID, ignore.case = T)
  a2 <- grep("WTH alarm", kalscpe_data$MANUAL.ID, ignore.case = T)
  a <- c(a1, a2)
  if(length(a) > 0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- "White-throated Honeyeater alarm"
  }
  if(length(a)==0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- ""
  }
  a3 <- grep("White-throated Honeyeater", kalscpe_data$MANUAL.ID, ignore.case = T)
  a3 <- setdiff(a3, a1)
  a4 <- grep("WTH", kalscpe_data$MANUAL.ID, ignore.case = T)
  a4 <- setdiff(a4, a2)
  a <- c(a3, a4)
  if(length(a) > 0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- "White-throated Honeyeater"
  }
  if(length(a)==0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- ""
  }
  a <- NULL
  a1 <- NULL
  a2 <- NULL
  a3 <- NULL
  a1 <- grep(" EW", kalscpe_data$MANUAL.ID, ignore.case = T)
  a2 <- grep("EW ", kalscpe_data$MANUAL.ID, ignore.case = T)
  a3 <- grep("Eastern Whipbird", kalscpe_data$MANUAL.ID, ignore.case = T)
  a <- c(a1, a2, a3)
  if(length(a) > 0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- "Eastern Whipbird"
  }
  if(length(a)==0) {
    kalscpe_data[a,(length(kalscpe_data)+1)] <- ""
  }
  colnames(kalscpe_data) <- c(col_names,list1, "White-throated Treecreeper",
                              "Eastern Yellow Robin",
                              "White-throated Honeyeater alarm",
                              "White-throated Honeyeater",
                              "Eastern Whipbird")
  
  kalscpe_data <<- kalscpe_data
}

bird_cluster_plot <- function(clusts, cols, pch) {
  tiff(paste("morning_chorus_cluster",paste(clusts, collapse = "_",sep=""),".tiff", sep = ""), 
       height = 1800, width = 2600)
  par(mar=c(2,3,0.5,1), bty="n", cex=4)
  plot(x = civ_dawn, y = (ylim1[2]:ylim1[1]), type="n", axes=FALSE, ann=FALSE)
  for(j in 1:length(clusts)) {
    clust <- clusts[j]
    a <- NULL
    a <- which(substr(Gym_cluster_list$cluster_list,1,40)==clust)
    bird_clust <- Gym_cluster_list[a,]
    for(i in 1:nrow(bird_clust)) {
      a <- which(as.character(substr(bird_clust$dates[i],1,8))==
                   paste(substr(civil_dawn$dates[reference[1]:nrow(civil_dawn)], 1, 4),
                         substr(civil_dawn$dates[reference[1]:nrow(civil_dawn)], 6, 7),
                         substr(civil_dawn$dates[reference[1]:nrow(civil_dawn)], 9, 10),
                         sep = ""))
      par(new=T)
      if(length(a) > 0 & a < (-ylim[1]+1)) {
        plot(x = (bird_clust$minute_reference[i] + 1), y = -a, pch=pch, 
             ylim = ylim, col=cols[j], 
             xlim = xlim, yaxt="n", xaxt="n", ylab = "", xlab = "")  
      }
    }
  }
  text(x = -10, y = xlim[1]+20, paste(pch = 4, pch = 4, pch = 4, pch = 4))
  par(new=TRUE)
  plot(x = civ_dawn, y = (ylim1[2]:ylim1[1]), 
       type = "l", yaxt="n", xaxt="n", lty = "1F", 
       ylab = "", ylim = ylim, xlim = xlim, 
       lwd = 3)
  
  list <- c("01","05","10","15","20","25")
  at <- NULL
  label2 <- NULL
  a_ref <- which(substr(civil_dawn$dates,1,10)==substr(start,1,10))
  for(i in 1:length(list)) {
    a1 <- which(substr(civil_dawn$dates[a_ref:nrow(civil_dawn)],9,10)==list[i])
    at <- c(at, a1)
    b1 <- rep(list[i],length(a1))
    label2 <- c(label2, b1)
  }
  axis(side = 2, at = -at, line = -1.2, labels = label2, 
       mgp=c(4,1,0), las=1)
  
  a <- which(substr(civil_dawn$dates[a_ref:nrow(civil_dawn)],9,10)=="01")
  a <- a[1:13]
  a <- c(-29, a)
  abline(h=-a, lty=5, lwd=0.1)
  
  labels <- c("Jun", "Jul", "Aug", "Sep", "Oct", "Nov",
              "Dec", "Jan", "Feb", "Mar", "Apr",
              "May", "Jun", "Jul")
  axis(side = 2, at = -a-14, line = NA, labels = labels, tick = F)
  par(new=T)
  plot(x = civ_dawn-5, y = -1:-length(civ_dawn), type = "l", 
       lty = 5, ylim = ylim, ylab="", yaxt="n",
       xaxt="n", lwd=3, xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+5, y = -1:-length(civ_dawn), type = "l", 
       lty = 5, ylim = ylim, ylab="", xaxt="n", lwd=3,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn-15, y = -1:-length(civ_dawn), 
       type = "l", lty=5, ylim = ylim, ylab="", lwd=3, 
       xaxt="n", yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+15, y = -1:-length(civ_dawn), type = "l", 
       lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn-25, y = -1:-length(civ_dawn), type = "l", 
       lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
       yaxt="n", xlim = xlim)
  par(new=T)
  plot(x = civ_dawn+25, y = -1:-length(civ_dawn), type = "l", 
       lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
       yaxt="n", xlim = xlim)
  at <- c(  0,  15,  30,  45,  60,  75,  90, 105, 
            120, 135, 150, 165, 180, 195, 210, 225, 
            240, 255, 270, 285, 300, 315, 330, 345, 
            360, 375, 390, 405, 420, 435, 450, 465, 
            480, 495, 510, 525, 540, 555, 570, 585, 
            600)
  at <- c(  0,  30,  60,  90, 
            120, 150, 180, 210, 
            240, 270, 300, 330, 
            360, 390, 420, 450, 
            480, 510, 540, 570, 
            600)
  labels1 <- c("12 am", "12:15 am","12:30 am", "12:45 am",
               "1 am",  "1:15 am",  "1:30 am",  "1:45 am",
               "2 am",  "2:15 am",  "2:30 am",  "2:45 am",
               "3 am",  "3:15 am",  "3:30 am",  "3:45 am",
               "4 am",  "4:15 am",  "4:30 am",  "4:45 am",
               "5 am",  "5:15 am",  "5:30 am",  "5:45 am",
               "6 am",  "6:15 am",  "6:30 am",  "6:45 am", 
               "7 am",  "7:15 am",  "7:30 am",  "7:45 am",
               "8 am",  "8:15 am",  "8:30 am",  "8:45 am",
               "9 am",  "9:15 am",  "9:30 am",  "9:45 am",
               "10 am")
  labels1 <- c("12 am", "12:30 am", 
               "1 am",  "1:30 am",  
               "2 am",  "2:30 am",  
               "3 am",  "3:30 am",  
               "4 am",  "4:30 am",  
               "5 am",  "5:30 am",  
               "6 am",  "6:30 am",  
               "7 am",  "7:30 am",  
               "8 am",  "8:30 am",  
               "9 am",  "9:30 am",  
               "10 am")
  abline(v=at)
  axis(side=1, at = at, labels = labels1, las=1, cex = 4)
  #library(plotrix)
  #draw.circle(x = 20, y = 350, radius = 4*1.0, col = "brown", border = "white")
  if(ylim[2]==0) {
    text(x = (civ_dawn[-ylim[2]+1] - 20), y = ylim[2]+0.4, "A")
    text(x = civ_dawn[-ylim[2]+1], y = ylim[2]+0.4, "B")
    text(x = civ_dawn[-ylim[2]+1] + 20, y = ylim[2]+0.4, "C")
  }
  
  if(ylim[2] < 0) {
    text(x = (civ_dawn[-ylim[2]] - 20), y = ylim[2]+1.4, "A")
    text(x = civ_dawn[-ylim[2]], y = ylim[2]+1.4, "B")
    text(x = civ_dawn[-ylim[2]] + 20, y = ylim[2]+1.4, "C")
  }
  
  legend(x=(xlim[1]-1.5),y=ylim[2]+1,legend=
           paste("Cluster ", clusts, sep = ""),
         pch = pch, bty = "n", cex = 1.4, col = cols)
  dev.off()
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Load the SUMMARY indices ---------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# load all unnormalised summary indices as "indices_all"
#load(file="data/datasets/summary_indices.RData")
#col_names <- colnames(indices_all)

# Alternatively load a normalised dataset
# load normalised summary indices this has had the missing minutes
# and microphone problem minutes removed 
# the dataframe is called "indices_norm_summary"
load(file="data/datasets/normalised_summary_indices.RData")

# read cluster list
cluster_list <- read.csv("data/datasets/chosen_cluster_list_25000_60.csv", header = T)
min_ref <- rep(1:1440, 398)
cluster_list$minute_reference <- min_ref
cluster_list$dates <- dates
col_names1 <- colnames(cluster_list)
col_names <- colnames(indices_norm_summary)
col_names <- c(col_names1, col_names)

a <- which(!is.na(cluster_list$cluster_list))
cluster_list[,4:15] <- 0
cluster_list[a,4:15] <- unname(indices_norm_summary[,1:12])

colnames(cluster_list) <- col_names