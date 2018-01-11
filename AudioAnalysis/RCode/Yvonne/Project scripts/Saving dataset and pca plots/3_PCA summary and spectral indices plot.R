# Title: PCA Summary and Spectral Indices plot
# Author: Yvonne Phillips
# Date:  14 July 2016
# Modified: 15 September 2016
# Modified: 24 October 2016 Added PCA biplot

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

# Use SHIFT, ALT, J to navigate to each of sections or
# functions
# 1.  PCA plots
# 2.  PCA biplots (3 plots) 

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# 1. Plot PCA plot ------------------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
## DO NOT LOAD ######ONLY for SPECTRAL INDICES
# Load the SPECTRAL indices
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
############### END OF SPECTRAL INDICES ONLY

# remove all objects in the global environment
rm(list = ls())

# Set the first day of recording
actual_start_date <- "2015-06-22"
start_date <- "2015-09-01" # where the plot will start
actual_end_date <- "2016-07-23"
end_date <- "2016-02-01"
interval <- 1440
start <- as.POSIXct(actual_start_date)
end <- as.POSIXct(actual_end_date)
dates <- seq(from=start, 
             by=interval*60, to=end)
total_days <- length(dates)
rm(start, end)
first_of_month <- which(substr(dates, 9, 10)=="01")
d <- which(substr(dates,1,10)==start_date)
e <- which(substr(dates,1,10)==end_date)

start <- as.POSIXct(start_date)
end <- as.POSIXct(end_date)
#end <- start + as.difftime(days, units="days")
dates <- seq(from=start, by=interval*60, to=end)

# load all of the summary indices as "indices_all"
load(file="C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/summary_indices.RData")
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
load(file="C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/missing_minutes_summary_indices.RData")

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


# replace the NA values with average values
# WARNING: This is only used for this plot no others
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

# Normalise data function
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

# Create a normalised dataset between 1.5 and 98.5% bounds 
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
# select a subset in relation to the start and end date
coef_min_max <- coef_min_max[c(((d-1)*1440):(e*1440-1),
                               (((d+total_days)-1)*1440):((e+total_days)*1440-1)),]

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

# Prepare civil dawn, civil dusk and sunrise and sunset times
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2016 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2016.csv")
civil_dawn_2015_2016 <- rbind(civil_dawn_2015, civil_dawn_2016)
rm(civil_dawn_2015, civil_dawn_2016)
civil_dawn_2015_2016$civil_dawn_times <- paste(substr(civil_dawn_2015_2016$CivSunrise,1,1),
                                          ":",
                                          substr(civil_dawn_2015_2016$CivSunrise,2,3), sep="")
civil_dawn_2015_2016$civil_dusk_times <- paste(substr(civil_dawn_2015_2016$CivSunset,1,2),
                                          ":",
                                          substr(civil_dawn_2015_2016$CivSunset,3,4), sep="")
civil_dawn_2015_2016$sunrise <- paste(substr(civil_dawn_2015_2016$Sunrise,1,1),
                                 ":",
                                 substr(civil_dawn_2015_2016$Sunrise,2,3), sep="")
civil_dawn_2015_2016$sunset <- paste(substr(civil_dawn_2015_2016$Sunset,1,2),
                                 ":",
                                 substr(civil_dawn_2015_2016$Sunset,3,4), sep="")


#civil_dawn <- read.csv("data/Sunrise_Sunset_Solar Noon_protected.csv", header=T)
a <- which(civil_dawn_2015_2016$dates==substr(start,1,10))
reference <- a:(a+days-1)
civil_dawn_times <- civil_dawn_2015_2016$civil_dawn_times[reference]
civil_dusk_times <- civil_dawn_2015_2016$civil_dusk_times[reference]
sunrise_times <- civil_dawn_2015_2016$sunrise[reference]
sunset_times <- civil_dawn_2015_2016$sunset[reference]

civ_dawn <- NULL
for(i in 1:length(civil_dawn_times)) {
  hour <- as.numeric(substr(civil_dawn_times[i], 1,1))
  min <- as.numeric(substr(civil_dawn_times[i], 3,4))
  minute <- hour*60 + min
  civ_dawn <- c(civ_dawn, minute)
}

civ_dusk <- NULL
for(i in 1:length(civil_dusk_times)) {
  hour <- as.numeric(substr(civil_dusk_times[i], 1,2))
  min <- as.numeric(substr(civil_dusk_times[i], 4,5))
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
  hour <- as.numeric(substr(sunset_times[i], 1,2))
  min <- as.numeric(substr(sunset_times[i], 4,5))
  minute <- hour*60 + min
  sunset <- c(sunset, minute)
}
style <- "false_colour"
style <- "colour_blind"
# these came from http://colorbrewer2.org/
#type=diverging&scheme=BrBG&n=11
colour_blind_colours <- c("#543005","#8c510a","#bf812d",
                          "#dfc27d","#f6e8c3",#"#f5f5f5",
                          "#c7eae5","#80cdc1","#35978f",
                          "#01665e","#003c30")
#http://colorbrewer2.org/#type=sequential&scheme=YlOrBr&n=9
yellow_scheme <- c('#ffffe5','#fff7bc','#fee391','#fec44f','#fe9929','#ec7014','#cc4c02','#993404','#662506')
#http://colorbrewer2.org/#type=sequential&scheme=Blues&n=9
blue_scheme <- c('#f7fbff','#deebf7','#c6dbef','#9ecae1','#6baed6','#4292c6','#2171b5','#08519c','#08306b')
#http://colorbrewer2.org/#type=sequential&scheme=Greens&n=9
green_scheme <- c('#f7fcf5','#e5f5e0','#c7e9c0','#a1d99b','#74c476','#41ab5d','#238b45','#006d2c','#00441b')
#http://colorbrewer2.org/#type=sequential&scheme=Greys&n=9  
grey_scheme <- c('#f0f0f0','#d9d9d9','#bdbdbd','#969696','#737373','#525252','#252525','#000000')
#http://colorbrewer2.org/#type=sequential&scheme=Oranges&n=9
orange_scheme <- c('#fff5eb','#fee6ce','#fdd0a2','#fdae6b','#fd8d3c','#f16913','#d94801','#a63603','#7f2704')
#http://colorbrewer2.org/#type=sequential&scheme=Purples&n=9
purple_scheme <- c('#fcfbfd','#efedf5','#dadaeb','#bcbddc','#9e9ac8','#807dba','#6a51a3','#54278f','#3f007d')
#http://colorbrewer2.org/#type=sequential&scheme=Reds&n=9
red_scheme <- c('#fff5f0','#fee0d2','#fcbba1','#fc9272','#fb6a4a','#ef3b2c','#cb181d','#a50f15','#67000d') 
#http://colorbrewer2.org/#type=sequential&scheme=PuRd&n=9
pink_scheme <- c('#f7f4f9','#e7e1ef','#d4b9da','#c994c7','#df65b0','#e7298a','#ce1256','#980043','#67001f')
#http://colorbrewer2.org/#type=diverging&scheme=BrBG&n=9
mixed_scheme <- c('#8c510a','#bf812d','#dfc27d','#f6e8c3','#c7eae5','#80cdc1','#35978f','#01665e')
library(scales)
show_col(mixed_scheme)

#http://colorbrewer2.org/#type=diverging&scheme=RdYlBu&n=11
colour_blind_colours2 <- c("#a50026","#ffffbf",
                           "#f46d43","#fdae61",
                           "#fee090","#d73027",
                           "#313695","#abd9e9",
                           "#74add1","#4575b4",
                           "#e0f3f8")
  
library(scales)
show_col(colour_blind_colours2)
#clusters <- kmeans(coef_min_max_norm[,1:3], centers = 11, iter.max=100)  
clusters <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\clusters_pca.csv")
#write.csv(clusters$cluster, "clusters_pca.csv", row.names = F)
coef_min_max_norm <- data.frame(coef_min_max_norm)
coef_min_max_norm$col <- colour_blind_colours2[clusters$cluster]
# produce pca diel plots for both sites ##
dev.off()
for (k in 1:2) {
  ref <- c(0, days*1440)
  # generate a date sequence and locate the first of the month
  days <- length(coef_min_max[,1])/(2*1440)
  #start <- as.POSIXct("2015-06-22")
  #start <- as.POSIXct(start)
  interval <- 1440
  end <- start + as.difftime(days, units="days")
  dates <- seq(from=start, by=interval*60, to=end)
  tiff(filename = paste("plots/PCA_plot_test",site[k],"_", 
                        type,"_", index, start_date,"_", 
                        end_date, style,".tif", sep = ""),
       width = 2600, height = 1400, units = "px",
       res = 300)
  #png(filename = paste("plots/PCA_plot_test",site[k],"_", type,"_", index, ".png", sep = ""),
  #    width = 2000, height = 1000, units = "px")
  par(mar=c(1.1, 3.4, 1, 3.4))
  # Plot an empty plot with no axes or frame
  plot(c(0,1440), c(days,1), type = "n", axes=FALSE, 
       frame.plot=FALSE,
       xlab="", ylab="") #, asp = 398/1440)
  # Create the heading
  mtext(side=3, line = 0.3,
        paste(site[k]," ", format(dates[1], "%d %B %Y")," - ", 
              format(dates[length(dates)-1], "%d %B %Y"), sep=""), 
        cex=0.7)
  # Create the sub-heading
  mtext(side=3, line = -0.3, 
        paste(index," ", type, " indices PCA coefficients", sep = ""),
        cex=0.6)
  
  # draw coloured polygons row by row
  ref <- ref[k]
  # set the rows starting at the top of the plot
  for(j in days:1) {
    # set the column starting on the left
    for(k in 1:1440) {
      ref <- ref + 1
      # draw a square for each minute in each day 
      # using the polygon function mapping the red, green
      # and blue channels to the normalised pca-coefficients
      if(style=="false_colour") {
        polygon(c(k,k,k+1,k+1), c(j,(j-1),(j-1),j),
                col=rgb(coef_min_max_norm[ref,1],
                        coef_min_max_norm[ref,2],
                        coef_min_max_norm[ref,3]),
                border = NA)  
      }
      if(style=="colour_blind") {
        polygon(c(k,k,k+1,k+1), c(j,(j-1),(j-1),j),
                col=coef_min_max_norm[ref,4],
                border = NA)  
      }
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
  at <- seq(0,1440, 120) + 1
  for(i in 1:length(at)) {
    lines(c(at[i], at[i]), c(1,days), lwd=1, lty=3)
  }
  # label the x axis
  axis(1, tick = FALSE, at = at, 
       labels = c("12 am","2","4",
                  "6","8","10", "12",
                  "2","4","6",
                  "8","10","12 pm"), 
       cex.axis=0.85, line = -1.6)
  mtext(side=1, line=0.16, "Time (24 hours)", cex=0.9)
  # plot the left axes
  axis(side = 2, at = first_of_each_month+2, tick = FALSE, 
       labels=format(dates[first_of_month],"%d %b %Y"), 
       cex.axis=0.9, las=1, line = -2.3, hadj=1.05)
  #if(!dates[1]==start_date) {
  #axis(side = 2, at = c(days), tick = FALSE, 
  #     labels=format(dates[1],"%d %b %Y"), 
  #     cex.axis=0.8, las=1, line = -1)
  #}
  # plot the left axes
  axis(side = 4, at = first_of_each_month+2, tick = FALSE, 
       labels=format(dates[first_of_month],"%d %b %Y"), 
       cex.axis=0.9, las=1, line = -2.3, hadj=-0.01)
  #if(!dates[1]==start_date) {
  #  axis(side = 4, at = c(days), tick = FALSE, 
  #       labels=format(dates[1],"%d %b %Y"), 
  #       cex.axis=0.8, las=1, line = -3)
  #}
  
  at <- seq(0, 1440, 240)
  # add the indices names to the plot
  #indices <- colnames(indices_all)
  #j <- days - (days - (length(indices)*8))/2
  #for(i in 1:length(indices)) {
  #  text(x = 140, j, indices[i], cex = 0.6)
  #  j <- j - 8 
  #}
  # draw YELLOW dotted line to show civil-dawn
  for(i in length(civ_dawn):1) {
    lines(c(civ_dawn+1), c(length(civ_dawn):1),  
          lwd=1.2, lty=2, col="black")
  }
  # draw YELLOW line to show civil-dusk
  for(i in length(civ_dawn):1) {
    lines(c(civ_dusk+1), c(length(civ_dusk):1),  
          lwd=1.2, lty=2, col="black")
  }
  # draw YELLOW line to show sunrise
  for(i in length(sunrise):1) {
    lines(c(sunrise+1), c(length(sunrise):1),  
          lwd=1.2, lty=2, col="black")
  }
  # draw YELLOW line to show sunset
  for(i in length(sunset):1) {
    lines(c(sunset+1), c(length(sunset):1),  
          lwd=1.2, lty=2, col="black")
  }
  dev.off()
}
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#An alternative way to plot
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

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# 2. Plot PCA biplot ------------------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# NOTE:  This code cannot be run until afer clustering 
# remove all objects in the global environment
rm(list = ls())

# Set the first day of recording
actual_start_date <- "2015-06-22"
site <- c("Gympie NP", "Woondum NP")

k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list 
cluster.list <- get(file_name_short, envir=globalenv())[,column]

# remove unneeded objects from global environment
rm(hclust_clusters_25000, file_name, file_name_short, column)

# load normalised summary indices this has had the missing minutes
# and microphone problem minutes removed
load(file="data/datasets/normalised_summary_indices.RData")
colnames(indices_norm_summary)
length(indices_norm_summary[,1])

indices_norm_summary <- cbind(indices_norm_summary, 
                              cluster.list)

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

indices <- indices_norm_summary

# preform pca analysis
indices_pca <- prcomp(indices[,1:12], scale. = F)
summary(indices_pca)
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
colnames(pca_coef) <- c("PC1", "PC2", "PC3", "PC4",
                        "PC5", "PC6","PC7")

coef_min_max <- pca_coef[,1:3]

coef_min_max_norm <- coef_min_max
#plot(coef_min_max_norm)
library(stats)

coef_min_max_norm <- cbind(indices_norm_summary, 
                           coef_min_max_norm)
rm(indices_norm_summary)

coef_min_max_norm <- as.data.frame(coef_min_max_norm)
minutes1_30 <- NULL
for(i in 1:30) {
  a <- which(coef_min_max_norm$cluster.list==i)
  minutes1_30 <- c(minutes1_30, a)
}

coef_min_max_norm <- as.data.frame(coef_min_max_norm)
minutes31_60 <- NULL
for(i in 31:60) {
  a <- which(coef_min_max_norm$cluster.list==i)
  minutes31_60 <- c(minutes31_60, a)
}

normIndices1_30 <- coef_min_max_norm[minutes1_30,]
normIndices31_60 <- coef_min_max_norm[minutes31_60,]
normIndices1_60 <- coef_min_max_norm

normIndices1_30$cluster.list <- as.character(normIndices1_30$cluster.list)
normIndices31_60$cluster.list <- as.character(normIndices31_60$cluster.list)
normIndices1_60$cluster.list <- as.character(normIndices1_60$cluster.list)

colours <- c("red", "chocolate4", "palegreen", "darkblue",
             "brown1", "darkgoldenrod3", "cadetblue4", 
             "darkorchid", "orange" ,"darkseagreen", 
             "deeppink3", "darkslategrey", "firebrick2", 
             "gold2", "hotpink2", "blue", "maroon", 
             "mediumorchid4", "mediumslateblue","mistyrose4",
             "royalblue", "turquoise", "palevioletred2", 
             "sienna", "slateblue", "yellow", "tan2", 
             "salmon","violetred1","plum")
# 60_colours
# Check for col_func in globalEnv otherwise source function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# colour information for each cluster, this function calls a csv
# containing R,G and B columns containing numbers between 0 and 255
col_func(cluster_colours, version = "colourblind")
colours <- cols[1:(length(cols)-1)]
list_unique_colours <- unique(cluster_colours[,3:5])
# change the same colours to unique colours

list_col <- NULL
unique_colours <- unique(colours)
for(i in 1:length(unique_colours)) {
  a <- which(colours==unique_colours[i])
  if(length(a) > 1) {
    ref <- 0
    for(j in 2:(length(a))) {
      if(cluster_colours[a[j],3] < 230) {
        cluster_colours[a[j],3] <- cluster_colours[a[j],3] + ref + 2  
        ref <- ref + 2  
      }
      if(cluster_colours[a[j],3] >= 230) {
        cluster_colours[a[j],3] <- cluster_colours[a[j],3] - ref - 2  
        ref <- ref + 2  
      }
    }  
  } 
}

library(R.utils)
cols <- NULL
for(i in 1:nrow(cluster_colours)) {
  R_code <- intToHex(cluster_colours$R[i])
  # add padding if necessary
  if(nchar(R_code)==1) {
    R_code <- paste("0", R_code, sep="")
  }
  G_code <- intToHex(cluster_colours$G[i])
  if(nchar(G_code)==1) {
    G_code <- paste("0", G_code, sep="")
  }
  B_code <- intToHex(cluster_colours$B[i])
  if(nchar(B_code)==1) {
    B_code <- paste("0", B_code, sep="")
  }
  col_code <- paste("#",
                    R_code, 
                    G_code,
                    B_code,
                    sep = "")
  cols <- c(cols, col_code)
}
cols <<- cols
colours <- cols[1:(length(cols)-1)] 

normIndices1_30 <- within(normIndices1_30, levels(cluster.list) <- colours)
normIndices31_60 <- within(normIndices31_60, levels(cluster.list) <- colours)
normIndices1_60 <- within(normIndices1_60, levels(cluster.list) <- colours)

#### Plotting PC1 & PC2 Principal Component Plots with base plotting system
# changes these and the values in the plot function
PrinComp_X_axis <- "PC2"
PrinComp_Y_axis <- "PC3"
first <- as.numeric(substr(PrinComp_X_axis, 3,3))  # change this and values in plot function below!!! to match PC# 
second <- as.numeric(substr(PrinComp_Y_axis, 3,3))
xlim <- c(-2,1.3)
ylim <- c(-1.35,1)

png(paste("data/plots/31_60_colourblind", PrinComp_X_axis, 
          PrinComp_Y_axis,".png", sep = ""), 
    width = 2200, height = 1400, units = "px") 

start <- 1
#finish <- length(normIndices1_60[,1])
finish <- length(normIndices31_60[,1])
colours <- rep(colours,2)
arrowScale <- 2.2 # increase/decrease this to adjust arrow length
summ <- summary(indices_pca)
rotate <- unname(summ$rotation)
#labels1 <- names(normIndices1_60[1:length(summ$center)])
labels1 <- names(normIndices31_60[1:length(summ$center)])
labels2 <-c("BGN", "SNR", "ACTV", "EPS", "HFC", "MFC",
            "LFC", "ACI", "EAS", "EPSP", "ECS", "CLC")
mainHeader <- paste (site,indices$rec.date[start],indices$rec.date[finish],
                     PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
par(mar=c(6,6,4,4))
plot(normIndices31_60$PC2[start:finish], 
     normIndices31_60$PC3[start:finish],  # Change these!!!!! 
     col=colours[as.numeric(as.character(normIndices31_60$cluster.list[start:finish]))], 
     cex=1, type='p', pch=15, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", 
                sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2,
     xlim = xlim, ylim = ylim)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels1)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.05, 
       rotate[i,second]*arrowScale*1.05, 
       paste(labels2[i]), cex=2.6)
}
abline (v=0, h=0, lty=2)
clust <- as.character(1:60)
#legend('topright', clust[1:30], pch=15, 
#       col=colours[1:30], bty='n', 
#       cex=2, title = "Clusters")
#legend('topright', clust[31:60], pch=15, 
#       col=colours[31:60], bty='n', 
#       cex=2)
#legend(x=xlim[2]-0.18, y= ylim[2],
#       clust[1:30], pch=15, 
#       col=colours[1:30], bty='n', 
#       cex=2)
legend(x=xlim[2]-0.05, y= ylim[2],
       clust[31:60], pch=15, 
       col=colours[31:60], bty='n', 
       cex=2)
text(x=xlim[2]-0.01, y= ylim[2]+0.03,
     "Clusters", cex = 2)
legend('topleft',labels, col=colours, bty='n', 
       cex=2, title = "Indices")

dev.off()



####### 3d plot #################################
library(rgl) # using rgl package
start <-  1         #day[5] + offset[4] + 1   
finish <- floor(length(normIndices31_60$PC1)) #day[7]-1)
start
finish
asp1 <- 2*(max(normIndices31_60$PC1)-min(normIndices31_60$PC1))
asp2 <- 2*(max(normIndices31_60$PC2)-min(normIndices31_60$PC2))
asp3 <- 2*(max(normIndices31_60$PC3)-min(normIndices31_60$PC3))

normIndices <- normIndices1_60
plot3d(normIndices$PC1[start:finish], 
       normIndices$PC2[start:finish], 
       normIndices$PC3[start:finish], 
       aspect = c(asp1, asp2, asp3),
       xlab = "", ylab = "", zlab = "",
       col=colours[as.numeric(
         as.character(
           normIndices$cluster.list[start:finish]))])#,
       #alpha.f = 0.1)

#play3d(spin3d(axis = c(0, 0, 1), rpm = 2), duration = 60)

M <- par3d("userMatrix")
if (!rgl.useNULL())
  play3d( par3dinterp(time = (0:2)*5, userMatrix = list(M,
                             rotate3d(M, pi/2, 1, 0, 0),
                             rotate3d(M, pi/2, 0, 1, 0) ) ), 
          duration = 30)
## Not run: 
movie3d( spin3d(), duration = 50, convert=TRUE )





if (!rgl.useNULL())
  play3d(spin3d(axis = c(1, 0, 0), rpm = 30), duration = 2)
#col=adjustcolor(normIndices$cluster.list, alpha.f = 0.1))
spheres3d(normIndices$PC1[start:finish], 
          normIndices31_60$PC2[start:finish], 
          normIndices31_60$PC3[start:finish], 
          aspect = c(asp1, asp2, asp3),
          col=colours[as.numeric(
            as.character(
              normIndices31_60$cluster.list[start:finish]))],
          radius = 0.005)
M <- par3d("userMatrix")
if (!rgl.useNULL())
  play3d( par3dinterp(time = (0:2)*0.75, userMatrix = list(M,
                                                           rotate3d(M, pi/2, 1, 0, 0),
                                                           rotate3d(M, pi/2, 0, 1, 0) ) ), 
          duration = 3 )
## Not run: 
movie3d( spin3d(), duration = 5, convert=TRUE )

       convert=T)

#col=adjustcolor(normIndices$cluster.list, alpha.f = 0.1),

xyzCoords <- data.frame(x1= numeric(10),  y1= integer(10), 
                        z1 = numeric(10), x2= numeric(10), 
                        y2= integer(10),  z2 = numeric(10))
for (i in 1:8) {
  xyzCoords$x2[i] <- rotate[i,1]
  xyzCoords$y2[i] <- rotate[i,2]
  xyzCoords$z2[i] <- rotate[i,3]
}
# xyz co-ordinates for segments
xyzCoords <- data.frame(x1= numeric(10),  y1= integer(10), 
                        z1 = numeric(10), x2= numeric(10), 
                        y2= integer(10),  z2 = numeric(10))
for (i in 1:9) {
  xyzCoords$x2[i] <- rotate[i,1]*0.8
  xyzCoords$y2[i] <- rotate[i,2]*0.8
  xyzCoords$z2[i] <- rotate[i,3]*0.8
}
segments3d(x=as.vector(t(xyzCoords[1:10,c(1,4)])),
           y=as.vector(t(xyzCoords[1:10,c(2,5)])),
           z=as.vector(t(xyzCoords[1:10,c(3,6)])), 
           lwd=2, col= "midnightblue")

library(scatterplot3d)
angle1 <- NULL
for(i in 0:100) {
  ang <- cos(i*0.03125*pi)
  angle1 <- c(angle1, ang)
}
angle2 <- NULL
for(i in 0:100) {
  ang <- sin(i*0.03125*pi)
  angle2 <- c(angle2, ang)
}

order <- c(1:32)

for(i in 1:18) {
  if(i < 10) {
    png(paste("data\\plots\\scatterplot3d\\scatterplot3d_5_0",i,".png",sep=""),
        width = 2200, height = 1400, units = "px")
  }
  if(i >= 10) {
    png(paste("data\\plots\\scatterplot3d\\scatterplot3d_5_",i,".png",sep=""),
        width = 2200, height = 1400, units = "px")
  }
  scatterplot3d(normIndices31_60$PC1[start:finish], 
                -1*angle2[i]*normIndices31_60$PC2[start:finish], 
                -1*angle1[i]*normIndices31_60$PC3[start:finish],
                xlim = c(-2.5,1),
                ylim = c(-1.5,1),
                zlim = c(-1.5,1),
                color = colours[as.numeric(
                  as.character(normIndices31_60$cluster.list[start:finish]))])
  dev.off()
}






#clust <- as.character(unname(unlist(unique(cluster.list)))) 

######### Normalise data #################################
# normalize values using minimum and maximum values
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
# Pre-processing of Temporal Entropy
# to correct the long tail 
indices[,14] <- sqrt(indices[,14])

normIndices <- indices
# normalise variable columns
normIndices[,2]  <- normalise(indices[,2],  0,  2)    # HighAmplitudeIndex (0,2)
normIndices[,3]  <- normalise(indices[,3],  0,  1)    # ClippingIndex (0,1)
normIndices[,4]  <- normalise(indices[,4], -44.34849276,-27.1750784)   # AverageSignalAmplitude (-50,-10)
normIndices[,5]  <- normalise(indices[,5], -45.06046874,-29.52071375)  # BackgroundNoise (-50,-10)
normIndices[,6]  <- normalise(indices[,6],  4.281792124, 25.57295061)  # Snr (0,50)
normIndices[,7]  <- normalise(indices[,7],  3.407526438, 7.653004384)  # AvSnrofActive Frames (3,10)
normIndices[,8]  <- normalise(indices[,8],  0.006581494, 0.453348819)  # Activity (0,1)
normIndices[,9]  <- normalise(indices[,9],  0, 2.691666667)     # EventsPerSecond (0,2)
normIndices[,10] <- normalise(indices[,10], 0.015519804, 0.167782223)  # HighFreqCover (0,0.5)
normIndices[,11] <- normalise(indices[,11], 0.013522414, 0.197555718)  # MidFreqCover (0,0.5)
normIndices[,12] <- normalise(indices[,12], 0.01984127,  0.259381856)  # LowFreqCover (0,0.5)
normIndices[,13] <- normalise(indices[,13], 0.410954108, 0.501671845)  # AcousticComplexity (0.4,0.7)
normIndices[,14] <- normalise(indices[,14], 0.004326753, sqrt(0.155612175))  # TemporalEntropy (0,sqrt(0.3))
normIndices[,15] <- normalise(indices[,15], 0.02130969, 0.769678735)   # EntropyOfAverageSpectrum (0,0.7)
normIndices[,16] <- normalise(indices[,16], 0.098730903, 0.82144857)   # EntropyOfVarianceSpectrum (0,1)
normIndices[,17] <- normalise(indices[,17], 0.119538801, 0.998670805)  # EntropyOfPeaksSpectrum (0,1)
normIndices[,18] <- normalise(indices[,18], 0.004470594, 0.530948096)   # EntropyOfCoVSpectrum (0,0.7)
normIndices[,19] <- normalise(indices[,19], 0.043940755, 0.931257154)  # NDSI (-0.8,1)
normIndices[,20] <- normalise(indices[,20], 1.852187379, 11.79845141)    # SptDensity (0,15)

# adjust values greater than 1 or less than 0
for (j in 4:20) {
  for (i in 1:length(normIndices[,j])) {
    if (normIndices[i,j] > 1) {
      normIndices[i,j] = 1
    }
  }
  for (i in 1:length(normIndices[,j])) {
    if (normIndices[i,j] < 0) {
      normIndices[i,j] = 0
    }
  }
}

normIndices <- normIndices[,c(5,7,9,10,11,12,13,17,18,37,38)]
normIndices <- cbind(normIndices, cluster.list)
normIndices$cluster.list <- as.factor(normIndices$cluster.list)

normIndices.pca <- prcomp(normIndices[,1:9], scale. = F)
normIndices$PC1 <- normIndices.pca$x[,1]
sum(normIndices$PC1)
normIndices$PC2 <- normIndices.pca$x[,2]
normIndices$PC3 <- normIndices.pca$x[,3]
normIndices$PC4 <- normIndices.pca$x[,4]
normIndices$PC5 <- normIndices.pca$x[,5]
normIndices$PC6 <- normIndices.pca$x[,6]
normIndices$PC7 <- normIndices.pca$x[,7]
normIndices$PC8 <- normIndices.pca$x[,8]
normIndices$PC9 <- normIndices.pca$x[,9]
plot(normIndices.pca)
biplot(normIndices.pca)

# assign colours to time-periods
normIndices <- within(normIndices, levels(fourhour.class) <- c("red","orange","yellow","green","blue","violet"))
normIndices <- within(normIndices, levels(hour.class) <- 
                        c("#FF0000FF","#FF4000FF","#FF8000FF","#FFBF00FF","#FFFF00FF",
                          "#BFFF00FF","#80FF00FF","#40FF00FF","#00FF00FF","#00FF40FF",
                          "#00FF80FF","#00FFBFFF","#00FFFFFF","#00BFFFFF","#0080FFFF",
                          "#0040FFFF","#0000FFFF","#4000FFFF","#8000FFFF","#BF00FFFF",
                          "#FF00FFFF","#FF00BFFF","#FF0080FF","#FF0040FF"))
#library(raster)
#colourName <- "colourBlock.png"
#colourBlock <- brick(colourName, package="raster")
#plotRGB(colourBlock)
#colourBlock <- as.data.frame(colourBlock)
#colours <- NULL
#for(i in 1:40) {
#  col <- rgb(colourBlock$colourBlock.1[i],
#             colourBlock$colourBlock.2[i],
#             colourBlock$colourBlock.3[i],
#             max = 255)
#  colours <- c(colours, col)
#}
#colours <- colours[1:30]
colours <- c("red", "chocolate4", "palegreen", "darkblue",
             "brown1", "darkgoldenrod3", "cadetblue4", 
             "darkorchid", "orange" ,"darkseagreen", 
             "deeppink3", "darkslategrey", "firebrick2", 
             "gold2", "hotpink2", "blue", "maroon", 
             "mediumorchid4", "mediumslateblue","mistyrose4",
             "royalblue", "orange", "palevioletred2", 
             "sienna", "slateblue", "yellow", "tan2", 
             "salmon","violetred1","plum")

#write.table(colours, file="colours.csv", row.names = F)

normIndices <- within(normIndices, levels(cluster.list) <- colours)

#fooPlot <- function(x, main, ...) {
#  if(missing(main))
#    main <- deparse(substitute(x))
#  plot(x, main = main, ...)
#}

#  set.seed(42)
#dat <- data.frame(x = rnorm(1:10), y = rnorm(1:10))
#fooPlot(dat, col = "red")

#### Plotting PC1 & PC2 Principal Component Plots with base plotting system
#png('pca_plot PC1_PC2_2_98_5,7,9,10,11,12,13,17,18.png', 
#    width = 1500, height = 1200, units = "px") 
PrinComp_X_axis <- "PC1"
PrinComp_Y_axis <- "PC2"
first <- 1  # change this and values in plot function below!!! to match PC# 
second <- 2  # change this!!! to match PC#
start <- 1
finish <- 10076
arrowScale <- 1.2 # increase/decrease this to adjust arrow length
summ <- summary(normIndices.pca)
rotate <- unname(summ$rotation)
labels1 <- names(normIndices[1:length(summ$center)])
labels2 <- c("BGN","ASF","EPS","HFC","MFC","LFC","ACC",
             "ENPS","ECVS")
mainHeader <- paste (site,indices$rec.date[start],indices$rec.date[finish],
                     PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
par(mar=c(6,6,4,4))
plot(normIndices$PC1[start:finish],normIndices$PC2[start:finish],  # Change these!!!!! 
     col=as.character(normIndices$cluster.list[start:finish]), 
     cex=1, type='p', pch=15, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.05, 
       rotate[i,second]*arrowScale*1.05, 
       paste(labels2[i]), cex=2.6)
}
abline (v=0, h=0, lty=2)
clust <- as.character(1:30)
legend('topright', clust, pch=15, col=colours, bty='n', 
       cex=2, title = "Clusters")
legend('topleft',labels, col=colours, bty='n', 
       cex=2, title = "Indices")

dev.off()