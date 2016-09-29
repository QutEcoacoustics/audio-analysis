# Title: PCA Summary and Spectral Indices plot
# Author: Yvonne Phillips
# Date:  14 July 2016
# Modified: 15 September 2016

# Description:  This code plots pca plots of acoustic data by first 
#   replacing missing minutes, normalising and calculating principal
#   components. The PCA components are mapped to the red, green and
#   blue channels. The input data required is data containing 
#   twenty-four hour (ie. 1440 minutes) data, with NAs where there 
#   are missing minutes. Civil-dawn, Sunrise, Sunset and Civil-dusk 
#   are plotted onto the plots as yellow lines.

# IMPORTANT:  
# 1. Set the start date below to the first day of recording
#    in format (YYYY-MM-DD)
# 2. Use lines 20 to 55 for Summary Indices and lines 57 to 99 for 
#    spectral indices

#############################################
# Load the SUMMARY indices
#############################################
start_date <- "2015-06-22"
# remove all objects in the global environment
rm(list = ls())

# load all of the summary indices as "indices_all"
load(file="data/datasets/summary_indices.RData")
# remove redundant indices
remove <- c(1,4,11,13,17:19)
indices_all <- indices_all[,-remove]
rm(remove)

# IMPORTANT:  These are used to name the plots
site <- c("Gympie NP", "Woondum NP")
index <- "SELECTED_Final" # or "ALL"
type <- "Summary"
paste("The dataset contains the following indices:"); colnames(indices_all)

# Generate a list of the missing minutes in summary indices
#missing_minutes_summary <- which(is.na(indices_all[,1]))
#save(missing_minutes_summary, file = "data/datasets/missing_minutes_summary_indices.RData")
load(file="data/datasets/missing_minutes_summary_indices.RData")

# List of summary indices columns:
#[1] "AvgSignalAmplitude"         [2]  "BackgroundNoise"          
#[3] "Snr"                        [4]   "AvgSnrOfActiveFrames"     
#[5] "Activity"                   [6]   "EventsPerSecond"          
#[7] "HighFreqCover"              [8]   "MidFreqCover"             
#[9] "LowFreqCover"               [10]  "AcousticComplexity"       
#[11] "TemporalEntropy"           [12]  "EntropyOfAverageSpectrum" 
#[13] "EntropyOfVarianceSpectrum" [14]  "EntropyOfPeaksSpectrum"   
#[15] "EntropyOfCoVSpectrum"      [16]  "ClusterCount"             
#[17] "ThreeGramCount"            [18]  "NDSI"                     
#[19] "SptDensity" 

#############################################
# Load the SPECTRAL indices
#############################################
# remove all objects in the global environment
rm(list = ls())

# load the spectral indices as "indices_all_spect"
load(file="data/datasets/spectral_indices.RData")

# remove all "ID" columns
a <- which(colnames(indices_all_spect)=="ID")
indices_all <- indices_all_spect[,-a]

# remove selected columns (see lists below)
remove <- c(5,9,12,19,31:36) # This is for the final26
indices_all <- indices_all[,-remove]
rm(remove)

# Generate a list of the missing minutes in spectral indices
#missing_minutes_spectral <- which(is.na(indices_all[,1]))
#save(missing_minutes_spectral, file = "data/datasets/missing_minutes_spectral_indices.RData")
#load(file="data/datasets/spectral_indices.RData")

# Important: These variables are used for naming files
site <- c("Gympie NP", "Woondum NP")
index <- "SELECTED_Final"
type <- "Spectral"

paste("The dataset contains the following indices:"); colnames(indices_all)

length(indices_all[,1])

# List of spectral indices columns:
# [1] "ACI_0Hz"     [2] "ACI_1000Hz"   [3] "ACI_2000Hz"   [4] "ACI_4000Hz"
# [5] "ACI_6000Hz"  [6] "ACI_8000Hz"   [7] "BGN_0Hz"      [8] "BGN_1000Hz"
# [9] "BGN_2000Hz" [10] "BGN_4000Hz"  [11] "BGN_6000Hz"  [12] "BGN_8000Hz"
#[13] "ENT_0Hz"    [14] "ENT_1000Hz"  [15] "ENT_2000Hz"  [16] "ENT_4000Hz"
#[17] "ENT_6000Hz" [18] "ENT_8000Hz"  [19] "EVN_0Hz"     [20] "EVN_1000Hz"
#[21] "EVN_2000Hz" [22] "EVN_4000Hz"  [23] "EVN_6000Hz"  [24] "EVN_8000Hz"
#[25] "POW_0Hz"    [26] "POW_1000Hz"  [27] "POW_2000Hz"  [28] "POW_4000Hz"
#[29] "POW_6000Hz" [30] "POW_8000Hz"  [31] "SPT_0Hz"     [32] "SPT_1000Hz"
#[33] "SPT_2000Hz" [34] "SPT_4000Hz"  [35] "SPT_6000Hz"  [36] "SPT_8000Hz"

##########################################################
# replace the NA values 
##########################################################
for(i in 1:ncol(indices_all)) {  # columns
  a <- which(is.na(indices_all[,i]))
  for(j in a) { 
    average <- mean(c(indices_all[(j-15),i], indices_all[(j-12),i], 
                      indices_all[(j-10),i], indices_all[(j+15),i], 
                      indices_all[(j+12),i], indices_all[(j+10),i]),
                    na.rm=TRUE)
    indices_all[j,i] <- average
  }
}

######### Normalise data #################################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

###########################################################
# Create a normalised dataset between 1.5 and 98.5% bounds 
###########################################################
indices_norm <- indices_all

# normalise values between 1.5 and 98.5 percentile bounds
q1.values <- NULL
q2.values <- NULL
for (i in 1:ncol(indices_all)) {
  q1 <- unname(quantile(indices_all[,i], probs = 0.015, na.rm = TRUE))
  q2 <- unname(quantile(indices_all[,i], probs = 0.985, na.rm = TRUE))
  q1.values <- c(q1.values, q1)
  q2.values <- c(q2.values, q2)
  indices_norm[,i]  <- normalise(indices_all[,i], q1, q2)
}
rm(q1, q2, i)

# adjust values greater than 1 or less than 0 to 1 and 0 respectively
for (j in 1:ncol(indices_norm)) {
  a <- which(indices_norm[,j] > 1)
  indices_norm[a,j] = 1
  a <- which(indices_norm[,j] < 0)
  indices_norm[a,j] = 0
}
##################################
# preform pca analysis
indices_pca <- prcomp(indices_norm, scale. = F)
indices_pca$PC1 <- indices_pca$x[,1]
indices_pca$PC2 <- indices_pca$x[,2]
indices_pca$PC3 <- indices_pca$x[,3]
indices_pca$PC4 <- indices_pca$x[,4]
indices_pca$PC5 <- indices_pca$x[,5]
indices_pca$PC6 <- indices_pca$x[,6]
indices_pca$PC7 <- indices_pca$x[,7]

pca_coef <- cbind(indices_pca$PC1, indices_pca$PC2,
                  indices_pca$PC3, indices_pca$PC4,
                  indices_pca$PC5, indices_pca$PC6,
                  indices_pca$PC7)

coef_min_max <- pca_coef[,1:3]

# Scale the PCA coefficients between 0 and 1 so they can be 
# mapped to red, green and blue channels.  These are multiplied 
# by 255 in order be used on the hexidecimal colour scale.
coef_min_max_norm <- coef_min_max
min.values <- NULL
max.values <- NULL
for (i in 1:3) {
  min <- unname(quantile(pca_coef[,i], probs = 0.0, na.rm = TRUE))
  max <- unname(quantile(pca_coef[,i], probs = 1.0, na.rm = TRUE))
  min.values <- c(min.values, min)
  max.values <- c(max.values, max)
  coef_min_max_norm[,i]  <- normalise(coef_min_max[,i], min, max)
}

# generate a date sequence and locate the first of the month
days <- length(coef_min_max[,1])/(2*1440)
start <- as.POSIXct(start_date)
interval <- 1440
end <- start + as.difftime(days, units="days")
dates <- seq(from=start, by=interval*60, to=end)
first_of_month <- which(substr(dates, 9, 10)=="01")

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

##########################################
# produce pca diel plots for both sites ##
##########################################
dev.off()
for (k in 1:2) {
  ref <- c(0, days*1440)
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

############################################
############################################
# An alternative way to plot
# problems with this method includes significant distortion
############################################
# load the required package
library(raster)

png(filename = "plots/ENT 1000_Gympie_pca_rbg.png", 
    width = 2000, height = 1200, units="px") 
# a more steamlined image can be achieved with height=1002 and asp = 1
r <- g <- b <- raster(ncol=1440, 
                      nrow=length(coef_min_max_norm[,1])/(1440*2))
values(r) <- coef_min_max_norm[1:(length(coef_min_max_norm[,1])/2),1] * 255
values(g) <- coef_min_max_norm[1:(length(coef_min_max_norm[,1])/2),3] * 255
values(b) <- coef_min_max_norm[1:(length(coef_min_max_norm[,1])/2),2] * 255
rgb = rgb <- stack(r, g, b)
rgb_extent <- extent(-180, 180, -90, 90)
extent(rgb) <- rgb_extent
# plot RGB
par(oma=c(2,8,2,8))
plotRGB(rgb, asp = 1.2) #((days*sqrt(2*sqrt(2)))/1440))

at <- seq(-180, 181, by = 60)
axis(1, line = -2, at = at, labels = c("0",
                                       "4 am","8 am","12","4 pm","8 pm",
                                       "24 hours"), cex.axis=1.5)
abline(v=at)
days <- length(coef_min_max[,1])/(2*1440)

at_position <- NULL
for(i in 1:length(first_of_month)) {
  at <- -1*((first_of_month[i])*180/days-90)
  at_position <- c(at_position, at)
}

axis(4, at = c(90, at_position),
     labels=format(dates[c(1,first_of_month)],"%d %b %y"),
     cex.axis=1.5, las=1.5)
axis(2, at = c(90, at_position),
     labels=format(dates[c(1,first_of_month)],"%d %b %y"),
     cex.axis=1.5, las=1.5)
abline(h=at_position)
mtext(side=3, 
      paste("Gympie ", format(dates[1], "%d %B %Y")," - ", 
            format(dates[length(dates)-1], "%d %B %Y"), sep=""), 
      cex=2)
mtext(side=3, line = -1.5, "ENT spectral indices pca coefficients",cex=1.5)

indices <- colnames(indices_all)
j <- 40
for(i in 1:length(indices)) {
  text(-160, j, indices[i], cex = 1.2)
  j <- j - 4.2 
}

dev.off()