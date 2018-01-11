#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Plot 11 and 12 Cluster diel plot for Gympie and Woondum  -------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# set the start date in "YYYY-MM-DD" format
start_date <- "2015-06-22"

k1_value <- 25000
k2_value <- 60

# load cluster list
cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",k1_value,
                               "_",k2_value, ".csv", sep = ""))
gym <- cluster_list$cluster_list[1:(nrow(cluster_list)/2)]
woon <- cluster_list$cluster_list[((nrow(cluster_list)/2)+1):nrow(cluster_list)]

# Generate a date sequence & locate the first of each month
days <- floor(nrow(cluster_list)/(2*1440))
start <- as.POSIXct(start_date)
interval <- 1440
end <- start + as.difftime(days, units="days")
dates <- seq(from=start, by=interval*60, to=end)
first_of_month <- which(substr(dates, 9, 10)=="01")

# Prepare civil dawn, civil dusk and sunrise and sunset times
#civil_dawn <- read.csv("data/Sunrise_Sunset_Solar Noon_protected.csv", header=T)
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2016 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2016.csv")
civil_dawn <- rbind(civil_dawn_2015, civil_dawn_2016)
# set the start date in "YYYY-MM-DD" format
start_date <- "2015-06-22"
start <- as.POSIXct(start_date)

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
index <- "Final"  #"SELECTED_Practice" # or "ALL"
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
# version is either 'ordinary' or 'colourblind'
col_func(cluster_colours, version = "colourblind")

# Generate and save the cluster diel plots
for (k in 1:2) {
  ref <- c(0, days*1440)
  # generate a date sequence and locate the first of the month
  days <- nrow(cluster_list)/(2*1440)
  start <- as.POSIXct(paste(start_date))
  interval <- 1440
  end <- start + as.difftime(days, units="days")
  dates <- seq(from=start, by=interval*60, to=end)
  if(k==1) {
    tiff("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/plots/Fig11.tiff", 
         width = 1713, height = 1300, units = 'px', res = 300)
  }
  if(k==2) {
    tiff("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/plots/Fig12.tiff", 
         width = 1713, height = 1300, units = 'px', res = 300)
  }
  #par(mar=c(0.9, 3.9, 0.9, 3.9), mgp = c(3,0.8,0),
  #    cex = 0.6, cex.axis = 1.2, cex.main = 1)
  par(mar=c(0.9, 3.1, 0.9, 3.1), mgp = c(3,0.8,0),
      cex = 0.6, cex.axis = 1.2, cex.main = 1)
  
  # Plot an empty plot with no axes or frame
  plot(c(0,1440), c(398,1), type = "n", axes=FALSE, 
       frame.plot=FALSE,
       xlab="", ylab="") #, asp = 398/1440)
  # Create the heading
  mtext(side=3, line = -1, cex = 0.8,
        paste("Cluster diel plot - ", site[k]," ", format(dates[1], "%d %B %Y")," - ", 
              format(dates[length(dates)-1], "%d %B %Y"), 
              sep=""))
  # Create the sub-heading
  #mtext(side=3, line = -1.5, 
  #      paste(type, " Indices: ", 
  #            paste(indices_names, collapse = ", "), 
  #            sep = ""))
  
  # draw coloured polygons row by row
  ref <- ref[k]
  # set the rows starting at the top of the plot
  for(j in days:1) {
    #if j==first
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
          lwd=1, lty = 3)
  }
  # draw vertical lines
  at <- seq(0,1440, 240) + 1
  for(i in 1:length(at)) {
    lines(c(at[i], at[i]), c(1,days), lwd=1, lty=3)
  }
  # label the x axis
  axis(1, tick = FALSE, at = at, 
       labels = c("12 am","4 am",
                  "8 am","12","4 pm",
                  "8 pm","12 pm"), line = -1.4)
  # plot the left axes
  axis(side = 2, at = first_of_each_month, tick = FALSE, 
       labels=format(dates[first_of_month],"%b %Y"), 
       las=1, line = -2.4, cex.axis=1, hadj=1.2)
  #axis(side = 2, at = c(days), tick = FALSE, 
  #     labels=format(dates[1],"%d %b %Y"), 
  #     las=1, line = -2.5)
  # plot the left axes
  axis(side = 4, at = first_of_each_month, tick = FALSE, 
       labels=format(dates[first_of_month],"%b %Y"), 
       las=1, line = -2.4, cex.axis=1, hadj=-0.16)
  #axis(side = 4, at = c(days), tick = FALSE, 
  #     labels=format(dates[1],"%d %b %Y"), 
  #     las=1, line = -2.5)
  
  at <- seq(0, 1440, 240)
  
  # draw dotted line to show civil-dawn
  for(i in length(civ_dawn):1) {
    lines(c(civ_dawn), c(length(civ_dawn):1),  
          lwd=1, lty=2, col="black")
  }
  # draw dotted line to show civil-dusk
  for(i in length(civ_dusk):1) {
    lines(c(civ_dusk), c(length(civ_dusk):1),  
          lwd=1, lty=2, col="black")
  }
  # draw dotted line to show sunrise
  for(i in length(sunrise):1) {
    lines(c(sunrise), c(length(sunrise):1),  
          lwd=1, lty=2, col="black")
  }
  # draw dotted line to show sunset
  for(i in length(sunset):1) {
    lines(c(sunset), c(length(sunset):1),  
          lwd=1, lty=2, col="black")
  }
  dev.off()
}
