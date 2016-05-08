#library(rjson)
setwd("C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Resources\\Weather\\")
#setwd("C:\\Work\\Latest Observations\\Tewantin\\")
options(scipen=999)
library(jsonlite)

folder <- "C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Resources\\Weather\\"
TewantinFiles <- list.files(path=folder,recursive = TRUE, full.names = FALSE,
                      pattern = "*_IDQ60801.94570.json")
GympieFiles <- list.files(path=folder,recursive = TRUE, full.names = FALSE,
                            pattern = "*_IDQ60801.94566.json")
# Read in all Gympie weather data
data <- fromJSON(GympieFiles[1])
data <- as.data.frame(data)
for (i in seq_along(GympieFiles)) {
  Name <- GympieFiles[i]
  fileContents <- fromJSON(Name)
  fileContents <- as.data.frame(fileContents)
  data <- rbind(data, fileContents)
  print(i)
}
data <- data[order(data$observations.data.local_date_time_full),]

# Remove all but the unique rows
data <- subset(data,!duplicated(data$observations.data.local_date_time_full))
data <- data[,c(8,9,10,13,14,16,17,18,20,21,22,29,31,32,33,37,38,45,46)]
names(data)

# calculate the rain per half hour from the rain trace (column 16)
rain <- as.numeric(data[1,16])
for (i in 2:length(data[,16])) {
  ra <- as.numeric(data[i,16]) - as.numeric(data[(i-1),16])
  if  (substr((data[i,7]),4,10) == "09:30am") {
    ra <- as.numeric(data[i,16])
  }
  if (ra < 0 & !is.na(ra)) {
    ra <- as.numeric(data[i,16])
  }
  rain <- c(rain, ra)
}
max(rain)
data[,20] <- rain
colnames(data)[20] <- "rain"
# remove data not on the half hour
minutes <- as.data.frame(substr(data[,7],8,8))
min.to.remove <-c("1", "2", "3", "4", "5", "6","7","8","9")
refer <- which(minutes[,] %in% min.to.remove)

gympie.data <- data[-c(refer),]
headings <- c("name", "state", "time_zone","sort_order",
           "wmo", "history_product", "date_time",
           "date_time_full", "lat","lon","apparent_t",
           "gust_kmh", "air_temp","dewpt","press",
           "rain_trace","rel_hum", "wind_dir",            
           "wind_spd_kmh", "rain")
colnames(gympie.data) <- headings

write.csv(gympie.data, "Gympie_weather1.csv", row.names = FALSE)
gympie_weather <- read.csv("Gympie_weather1.csv")

##############################################
# produce a table containing the weather data 
# reconstructed into a form useful for package 
# TraMineR
##############################################
list <- c("0000","0030","0100","0130","0200","0230","0300","0330",
          "0400","0430","0500","0530","0600","0630","0700","0730",
          "0800","0830","0900","0930","1000","1030","1100","1130",
          "1200","1230","1300","1330","1400","1430","1500","1530",
          "1600","1630","1700","1730","1800","1830","1900","1930",
          "2000","2030","2100","2130","2200","2230","2300","2330")
#set ref to determine the time of the first row
for(i in 1:length(list)) {
  if(substr(gympie_weather[1,8],9,12)==list[i]) {
    ref1 <- i
    stop
  }
}
ref2 <- 50-ref1
# generate a list of column names for the gympie_matrix
columnNames <- NULL
for (i in 1:length(list)) {
  lab <- c(paste(list[i], "aprnt_t"), paste(list[i],"gust_kmh"), 
           paste(list[i],"air_temp"), paste(list[i],"dewpt"),
           paste(list[i],"press"), paste(list[i], "rain_trace"),
           paste(list[i], "rel_hum"), paste(list[i],"wind_dir"),
           paste(list[i], "wind_spd_kmh"), paste(list[i], "rain"))
  columnNames <- c(columnNames, lab)
}
columnNames <- c("date", columnNames)

no.of.rows <- length(gympie_weather$name)
gympie_matrix <- matrix(data = NA, nrow=floor(no.of.rows/48), 
                        ncol = 1+10*48)
colnames(gympie_matrix) <- columnNames

# fill matrix with weather data
#seq1 <- seq(ref2, length(gympie_weather$name),48)
seq2 <- seq(2,472,10)
count <- ref2
for(j in 1:length(gympie_matrix[,1])) {
  for(k in 1:length(seq2)) {
    gympie_matrix[j,seq2[k]:(seq2[k]+9)] <- unlist(gympie_weather[count,11:20], use.names = F)
    gympie_matrix[j,(seq2[k]+7)] <- as.character(gympie_weather[count,18])
    count <- count + 1
  }
  gympie_matrix[j,1] <- substr(gympie_weather[(count-1),8],1,8)
}
write.csv(gympie_matrix, "gympie_weather_matrix.csv", row.names = FALSE)

###############################################
# TEWANTIN WEATHER DATA
###############################################
# Read in all Tewantin weather data
data <- fromJSON(TewantinFiles[1])
data <- as.data.frame(data)
for (i in seq_along(TewantinFiles)) {
  Name <- TewantinFiles[i]
  fileContents <- fromJSON(Name)
  fileContents <- as.data.frame(fileContents)
  data <- rbind(data, fileContents)
  print(i)
}
data <- data[order(data$observations.data.local_date_time_full),]

# Remove all but the unique rows
data <- subset(data,!duplicated(data$observations.data.local_date_time_full))
data <- data[,c(8,9,10,13,14,16,17,18,20,21,22,29,31,32,33,37,38,45,46)]
names(data)

# calculate the rain per half hour from the rain trace (column 16)
rain <- as.numeric(data[1,16])
for (i in 2:length(data[,16])) {
  ra <- as.numeric(data[i,16]) - as.numeric(data[(i-1),16])
  if  (substr((data[i,7]),4,10) == "09:30am") {
    ra <- as.numeric(data[i,16])
  }
  if (ra < 0 & !is.na(ra)) {
    ra <- as.numeric(data[i,16])
  }
  rain <- c(rain, ra)
}
max(rain)
data[,20] <- rain
colnames(data)[20] <- "rain"
# remove data not on the half hour
minutes <- as.data.frame(substr(data[,7],8,8))
min.to.remove <-c("1","2","3","4","5","6","7","8","9")
refer <- which(minutes[,] %in% min.to.remove)
tewantin.data <- data
if (length(refer)>0) {
  tewantin.data <- data[-c(refer),]  
}

write.csv(tewantin.data, "Tewantin_weather.csv", row.names = FALSE)
########################################################
# Prepare PCA coefficients and plot diel weather plot
########################################################
normIndices <- NULL
# Calculate the pca coefficients
normIndices.pca <- prcomp(gympie.data[,c(12:15,17,19:20)], scale. = F)
#normIndices.pca <- prcomp(gympie.data[,c(13,14,17,19:20)], scale. = F)

normIndices$PC1 <- normIndices.pca$x[,1]
sum(normIndices$PC1)
normIndices$PC2 <- normIndices.pca$x[,2]
normIndices$PC3 <- normIndices.pca$x[,3]
normIndices$PC4 <- normIndices.pca$x[,4]
normIndices$PC5 <- normIndices.pca$x[,5]
normIndices$PC6 <- normIndices.pca$x[,6]
normIndices$PC7 <- normIndices.pca$x[,7]
plot(normIndices.pca)
biplot(normIndices.pca)
pca.weather.coefficients <- cbind(normIndices$PC1,
                                  normIndices$PC2,
                                  normIndices$PC3)
#weather <- read.csv("Weather.csv", header = TRUE)[,2:4]
#ds6 <- weather
ds6 <- pca.weather.coefficients
##### Normalise the dataset ################ 
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
ds.coef_min_max <- ds6
min.values <- NULL
max.values <- NULL
for (i in 1:3) {
  min <- unname(quantile(ds6[,i], probs = 0.0, na.rm = TRUE))
  max <- unname(quantile(ds6[,i], probs = 1.0, na.rm = TRUE))
  min.values <- c(min.values, min)
  max.values <- c(max.values, max)
  ds.coef_min_max[,i]  <- normalise(ds.coef_min_max[,i], min, max)
}

# reference to the first and last 12:30 am
reference <- which(substr((gympie.data[,7]),4,10) == "12:30am")
days <- length(reference)
if (reference[1] > 1) {
  days <- days + 1
}
ref1 <- (48-(reference[1]-1))*30
ref2 <- (days*1440) - (length(gympie.data[,1])*30 + ref1)
ref1_min <- rep(1, ref1)
ref2_min <- rep(1, ref2)
ds.coef1 <- c(ref1_min, rep(ds.coef_min_max[,1], each=30), ref2_min)
ds.coef2 <- c(ref1_min, rep(ds.coef_min_max[,2], each=30), ref2_min)
ds.coef3 <- c(ref1_min, rep(ds.coef_min_max[,3], each=30), ref2_min)
ds.coef_min_max <- cbind(ds.coef1,ds.coef2,ds.coef3)

library(raster)
r <- g <- b <- raster(ncol=1440, nrow=days)
values(r) <- ds.coef_min_max[,1]
values(g) <- ds.coef_min_max[,2]
values(b) <- ds.coef_min_max[,3]
co <- rep(0,length(red)) # added
red <- round(values(r)*255,0) # added
green <- round(values(g)*255,0) # added
blue <- round(values(b)*255,0) #added
colouring <- cbind(red, green, blue, co) # added
colouring <- as.data.frame(colouring) # added
for (i in 1:length(red)) {
  colouring$co[i] <- rgb(red[i],green[i],blue[i],max=255) 
}

colOutput <- matrix(colouring$co, ncol = 1440, byrow = TRUE)
colours <- unique(colOutput)

rgb = rgb <- stack(r*255, g*255, b*255)
# gympie pca weather plot
aspect = 0.6
png("Gympie_weather_diel1.png",
    width = 1000*aspect, height = 200, units="mm",
    res=80)
# plot RGB
par(mar=c(0,0,0,0))
plotRGB(rgb, asp=0.5)
par(new=TRUE)
x <- 1:1440
y <- rep(days,1440)
par(mar=c(0,0,0,0), oma=c(4,2,3,9), cex=0.8, cex.axis=1.2)
plot(1:1440, y, type="n", xlab="", ylab="", axes=F)
at <- seq(-79, 1892, by = 285)
at[2:length(at)] <- at[2:length(at)]-1 
axis(1, at = at, labels = c("00:00",
                            "04:00","08:00","12:00","16:00","20:00",
                            "24:00"), cex.axis=1.5)
labels <- unique(substr(data[,8],1,8))
first.of.month <- which(substr(labels[1:55],7,8)=="01")
labels <- c(labels[first.of.month])

axis(4, at = (days - first.of.month), labels=labels, cex.axis=1.5, las=2)
#axis(4, at = c(111-0,111-10,111-41,111-72,111-102), 
#     labels=c("22 Jun 2015","1 Jul 2015","1 Aug 2015", "1 Sept 2015", "1 Oct 2015"), 
#     cex.axis=1.5, las=2)
mtext(side=3,line=1,"Gympie Weather data", cex = 1.8)
mtext(side=3, line = -1.5, "Normalised pca coefficients",cex=1.5)
dev.off()

############################################################
#TEWANTIN
############################################################
normIndices <- NULL
# Calculate the pca coefficients
# statement about complete cases
missing <- which (!complete.cases(tewantin.data))
result <- rle(diff(missing))
place <-which(result$lengths>=1 & result$values==1)
result2 <- rle(diff(place))
if (length(missing) > 0) {
  for (i in 1:length(missing)) {
    cat("missing values at", missing[i])
    print("\n")
  }
}
# remove these and replace with white
normIndices.pca <- prcomp(tewantin.data[,c(12:15,17,19)], 
                          scale. = F)
#normIndices.pca <- prcomp(tewantin.data[,c(13,14,17,19:20)], scale. = F)

normIndices$PC1 <- normIndices.pca$x[,1]
sum(normIndices$PC1)
normIndices$PC2 <- normIndices.pca$x[,2]
normIndices$PC3 <- normIndices.pca$x[,3]
normIndices$PC4 <- normIndices.pca$x[,4]
normIndices$PC5 <- normIndices.pca$x[,5]
normIndices$PC6 <- normIndices.pca$x[,6]
normIndices$PC7 <- normIndices.pca$x[,7]
plot(normIndices.pca)
biplot(normIndices.pca)
pca.weather.coefficients <- cbind(normIndices$PC1,
                                  normIndices$PC2,
                                  normIndices$PC3)
#weather <- read.csv("Weather.csv", header = TRUE)[,2:4]
#ds6 <- weather
ds6 <- pca.weather.coefficients
##### Normalise the dataset ################ 
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
ds.coef_min_max <- ds6
min.values <- NULL
max.values <- NULL
for (i in 1:3) {
  min <- unname(quantile(ds6[,i], probs = 0.0, na.rm = TRUE))
  max <- unname(quantile(ds6[,i], probs = 1.0, na.rm = TRUE))
  min.values <- c(min.values, min)
  max.values <- c(max.values, max)
  ds.coef_min_max[,i]  <- normalise(ds.coef_min_max[,i], min, max)
}

# reference to the first and last 12:30 am
reference <- which(substr((tewantin.data[,7]),4,10) == "12:30am")
days <- length(reference)
if (reference[1] > 1) {
  days <- days + 1
}
ref1 <- (48-(reference[1]-1))*30
ref2 <- (days*1440) - (length(tewantin.data[,1])*30 + ref1)
ref1_min <- rep(1, ref1)
ref2_min <- rep(1, ref2)
ds.coef1 <- c(ref1_min, rep(ds.coef_min_max[,1], each=30), ref2_min)
ds.coef2 <- c(ref1_min, rep(ds.coef_min_max[,2], each=30), ref2_min)
ds.coef3 <- c(ref1_min, rep(ds.coef_min_max[,3], each=30), ref2_min)
ds.coef_min_max <- cbind(ds.coef1,ds.coef2,ds.coef3)

library(raster)
r <- g <- b <- raster(ncol=1440, nrow=days)
values(r) <- ds.coef_min_max[,1]
values(g) <- ds.coef_min_max[,2]
values(b) <- ds.coef_min_max[,3]
co <- rep(0,length(red)) # added
red <- round(values(r)*255,0) # added
green <- round(values(g)*255,0) # added
blue <- round(values(b)*255,0) #added
colouring <- cbind(red, green, blue, co) # added
colouring <- as.data.frame(colouring) # added
for (i in 1:length(red)) {
  colouring$co[i] <- rgb(red[i],green[i],blue[i],max=255) 
}

colOutput <- matrix(colouring$co, ncol = 1440, byrow = TRUE)
colours <- unique(colOutput)

rgb = rgb <- stack(r*255, g*255, b*255)
# tewantin pca weather plot
aspect = 0.6
png("Tewantin_weather_diel1.png",
    width = 1000*aspect, height = 200, units="mm",
    res=80)
# plot RGB
par(mar=c(0,0,0,0))
plotRGB(rgb, asp=0.5)
par(new=TRUE)
x <- 1:1440
y <- rep(days,1440)
par(mar=c(0,0,0,0), oma=c(4,2,3,9), cex=0.8, cex.axis=1.2)
plot(1:1440, y, type="n", xlab="", ylab="", axes=F)
at <- seq(-79, 1892, by = 285)
at[2:length(at)] <- at[2:length(at)]-1 
axis(1, at = at, labels = c("00:00",
                            "04:00","08:00","12:00","16:00","20:00",
                            "24:00"), cex.axis=1.5)
labels <- unique(substr(data[,8],1,8))
first.of.month <- which(substr(labels[1:55],7,8)=="01")
labels <- c(labels[first.of.month])

axis(4, at = (days - first.of.month), labels=labels, cex.axis=1.5, las=2)
#axis(4, at = c(111-0,111-10,111-41,111-72,111-102), 
#     labels=c("22 Jun 2015","1 Jul 2015","1 Aug 2015", "1 Sept 2015", "1 Oct 2015"), 
#     cex.axis=1.5, las=2)
mtext(side=3,line=1,"Tewantin Weather data", cex = 1.8)
mtext(side=3, line = -1.5, "Normalised pca coefficients",cex=1.5)
dev.off()
####################################################

plot(gympie.data$observations.data.dewpt, type="l")
plot(gympie.data$observations.data.press, type="l")
par(new=TRUE)
plot(gympie.data$rain, col="red", type="l")
par(new=TRUE)
plot(gympie.data$observations.data.dewpt, col="blue", type="l")
par(new=TRUE)
plot(gympie.data$observations.data.rel_hum, col="orange", type="l")
par(new=TRUE)
plot(gympie.data$observations.data.air_temp, type="l")
