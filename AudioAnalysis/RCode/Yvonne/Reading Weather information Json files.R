# remove all objects in the global environment
rm(list = ls())

#library(rjson)
#setwd("C:\\Work\\Weather Data\\")
#setwd("C:\\Work\\Latest Observations\\Tewantin\\")
options(scipen=999)
library(jsonlite)

folder <- "C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Resources\\Weather\\"
GympieFiles <- list.files(path=folder,recursive = TRUE, full.names = TRUE,
                            pattern = "*_IDQ60801.94566.json")

GympieFiles <- GympieFiles[1:268]
# Read in all Gympie weather data
data <- fromJSON(GympieFiles[1])
data <- as.data.frame(data)

# Gympie Files
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
# remove data not on the half hour
minutes <- as.data.frame(substr(data[,7],8,8))
min.to.remove <- c("1", "2", "3", "4", "5", "6", "7", "8", "9")
refer <- which(minutes[,] %in% min.to.remove)

gympie.data <- data[-c(refer),]
headings <- c("name", "state", "time_zone","sort_order",
              "wmo", "history_product", "date_time",
              "date_time_full", "lat","lon","apparent_t",
              "gust_kmh", "air_temp","dewpt","press",
              "rain_trace","rel_hum", "wind_dir",            
              "wind_spd_kmh")
colnames(gympie.data) <- headings

# calculate the rain per half hour from the rain trace (column 16)
rain <- as.numeric(gympie.data[1,16])
for (i in 2:length(gympie.data[,16])) {
  ra <- as.numeric(gympie.data[i,16]) - as.numeric(gympie.data[(i-1),16])
  if  (substr((gympie.data[i,7]),4,10) == "09:30am") {
    ra <- as.numeric(gympie.data[i,16])
  }
  if (ra < 0 & !is.na(ra)) {
    ra <- as.numeric(gympie.data[i,16])
  }
  rain <- c(rain, ra)
}
rain <- data.frame(rain)
max(rain, na.rm = T)

gympie.data[,20] <- rain

a <- which(gympie.data$date_time_full==20160724003000)
write.csv(gympie.data[1:a,], "data/weather/Gympie_weather1.csv", 
          row.names = FALSE)
gympie_weather <- read.csv("Gympie_weather1.csv")
#gympie_weather_complete_cases <- gympie_weather[complete.cases(gympie_weather),]
#gympie_weather_matrix <- 
#  read.csv("C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Resources\\Weather\\gympie_weather_matrix.csv")
# generate a sequence of dates -----------------------
start <- strptime("20160207", format="%Y%m%d")
finish <- strptime("20160731", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
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

# time sequence
times <- c("000000","003000","010000","013000",
           "020000","023000","030000","033000",
           "040000","043000","050000","053000",
           "060000","063000","070000","073000",
           "080000","083000","090000","093000",
           "100000","103000","110000","113000",
           "120000","123000","130000","133000",
           "140000","143000","150000","153000",
           "160000","163000","170000","173000",
           "180000","183000","190000","193000",
           "200000","203000","210000","213000",
           "220000","223000","230000","233000")
dates1 <- unique(substr(gympie.data$date_time_full,1,8))
date_list <- NULL
for(i in 1:length(dates)) {
  for(j in 1:length(times)) {
    lst <- paste(dates[i],times[j],sep = "")
    date_list <- c(date_list, lst)
  }
}

# determine a list of missing observations within the list of dates ---------------------------------
check_dates <- matrix(nrow = length(date_list),
                      ncol = 2)
check_dates <- data.frame(check_dates)
colnames(check_dates) <- c("full_dates", "check")
check_dates$full_dates <- date_list
check_dates$check <- "NO"
full_dates_list <- unique(gympie.data$date_time_full)
for(i in 1:length(full_dates_list)) {
  a <- which(check_dates$full_dates==full_dates_list[i])
  check_dates$check[a] <- "YES"
}
a <- which(check_dates$check=="NO")
#1:21, 4961:4965, 5832:5834, 6117:6119
missing_obs <- check_dates$full_dates[a]

result <- rle(diff(a))
result <- result$lengths[seq(1, length(result$lengths),2)]
result <- result + 1
result <- c(0, result)
a
cum_sum <- cumsum(result) + 1

# add rows where data is missing
if(a[1] == 1) {
  for(i in 3:length(result)) {
    mydf <- matrix(nrow = result[i], 
                   ncol = ncol(gympie_weather), "NA")
    mydf <- data.frame(mydf)
    colnames(mydf) <- colnames(gympie_weather)
    new_gympie_weather <- rbind(gympie_weather[1:(a[cum_sum[i-1]]-1-result[2]), ], 
                                mydf, 
                                gympie_weather[(a[cum_sum[i-1]]-result[2]):nrow(gympie_weather), ])
    # correct the row names
    row.names(new_gympie_weather) <- 1:nrow(new_gympie_weather)  
    gympie_weather <- new_gympie_weather
    rm(new_gympie_weather)
  }  
}

if(a[1] > 1) {
  for(i in 2:length(result)) {
    mydf <- matrix(nrow = result[i], 
                   ncol = ncol(gympie_weather), "NA")
    mydf <- data.frame(mydf)
    colnames(mydf) <- colnames(gympie_weather)
    new_gympie_weather <- rbind(gympie_weather[1:(a[cum_sum[i-1]]-1-result[2]), ], 
                                mydf, 
                                gympie_weather[(a[cum_sum[i-1]]-result[2]):nrow(gympie_weather), ])
    # correct the row names
    row.names(new_gympie_weather) <- 1:nrow(new_gympie_weather)  
    gympie_weather <- new_gympie_weather
    rm(new_gympie_weather)
  }  
}

# Calculate 1 hour rain sums -----------------------------------
gympie_weather$rain_1hour <- 0
hour1_seq <- seq(2,nrow(gympie_weather),2)
gympie_weather$rain <- as.numeric(gympie_weather$rain)
for(i in 1:length(hour1_seq)) {
  gympie_weather$rain_1hour[hour1_seq[i]] <- sum(gympie_weather$rain[hour1_seq[i]-1],
                                                 gympie_weather$rain[hour1_seq[i]]) 
}

View(gympie_weather)

# Calculate 2 hour rain sums --------------------------------------
gympie_weather$rain_2hours <- 0
hour2_seq <- seq(4,nrow(gympie_weather),4)
gympie_weather$rain <- as.numeric(gympie_weather$rain)
for(i in 1:length(hour2_seq)) {
  gympie_weather$rain_2hours[hour2_seq[i]] <- sum(gympie_weather$rain[hour2_seq[i]-3],
                                                  gympie_weather$rain[hour2_seq[i]-2],
                                                  gympie_weather$rain[hour2_seq[i]-1],
                                                  gympie_weather$rain[hour2_seq[i]]) 
}
View(gympie_weather)
#setwd("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/")
#write.csv(gympie_weather, "data/weather/gympie_rain_protected.csv", row.names = F)

# read gympie_weather file (rain) -----------------------------------
gympie_weather <- read.csv("data/weather/gympie_rain_protected.csv", header = T) 
hour_half_rain <- cbind(gympie_weather$date_time_full,
                        round(gympie_weather$rain,1))
hour1_rain <- cbind(gympie_weather$date_time_full[hour1_seq], 
                    round(gympie_weather$rain_1hour[hour1_seq],1))
hour2_rain <- cbind((gympie_weather$date_time_full[hour2_seq]), 
                    round(gympie_weather$rain_2hour[hour2_seq],1))

hour_half_rain <- data.frame(hour_half_rain)
colnames(hour_half_rain) <- c("date-time","rain")
hour1_rain <- data.frame(hour1_rain)
colnames(hour1_rain) <- c("date-time","rain")
hour2_rain <- data.frame(hour2_rain)
colnames(hour2_rain) <- c("date-time","rain")

hour_half_rain$date <- "date"
hour_half_rain$time <- "time"
hour1_rain$date <- "date"
hour1_rain$time <- "time"
hour2_rain$date <- "date"
hour2_rain$time <- "time"

for(i in 1:nrow(hour_half_rain)) {
  hour_half_rain$date[i] <- paste(substr(hour_half_rain$'date-time'[i],1,4),
                                  substr(hour_half_rain$'date-time'[i],5,6),
                                  substr(hour_half_rain$'date-time'[i],7,8), 
                                  sep = "-")
  hour_half_rain$time[i] <- paste(substr(hour_half_rain$'date-time'[i],9,10),
                                  substr(hour_half_rain$'date-time'[i],11,12),
                                  sep = ":")
}
for(i in 1:nrow(hour1_rain)) {
  hour1_rain$date[i] <- paste(substr(hour1_rain$'date-time'[i],1,4),
                              substr(hour1_rain$'date-time'[i],5,6),
                              substr(hour1_rain$'date-time'[i],7,8), 
                              sep = "-")
  hour1_rain$time[i] <- paste(substr(hour1_rain$'date-time'[i],9,10),
                              substr(hour1_rain$'date-time'[i],11,12),
                              sep = ":")
}
for(i in 1:nrow(hour2_rain)) {
  hour2_rain$date[i] <- paste(substr(hour2_rain$'date-time'[i],1,4),
                              substr(hour2_rain$'date-time'[i],5,6),
                              substr(hour2_rain$'date-time'[i],7,8), 
                              sep = "-")
  hour2_rain$time[i] <- paste(substr(hour2_rain$'date-time'[i],9,10),
                              substr(hour2_rain$'date-time'[i],11,12),
                              sep = ":")
}

write.csv(hour_half_rain, "data/weather/hour_half_rain.csv", row.names = FALSE)
write.csv(hour1_rain, "data/weather/hour1_rain.csv", row.names = FALSE)
write.csv(hour2_rain, "data/weather/hour2_rain.csv", row.names = FALSE)

#a <- which(is.na(hour1_rain$rain=="NA"))
#hour1_rain$`date-time`[a]

rain <- read.csv("data/weather/hour_half_rain.csv", header = T)

 
# remove all objects in the global environment
rm(list = ls())

# load the cluster list with the missing minutes in place -------------------
k1_value <- 25000
k2_value <- 60
column <- k2_value/5

# generate the time sequences
startDate = as.POSIXct("2015-06-22 00:00")
endDate = as.POSIXct("2016-07-24 00:00")
dateSeq30min = substr(seq(from=startDate, to=endDate, by="30 min"),1,16)
dateSeq60min = substr(seq(from=startDate, to=endDate, by="60 min"),1,16)
dateSeq120min = substr(seq(from=startDate, to=endDate, by="120 min"),1,16)

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list (missing minutes)
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

# generate a sequence that can be used for half-hour, 
# one-hour and two-hour sequences
day.ref <- seq(1,(398*2*1440+1),30)

# generate half-hour counts for each cluster + sum
# the sum checks for NAs
# WARNING this takes 21 minutes for a cluster list
# over 1 million minutes
counts <- NULL
for(i in 1:(length(day.ref)-1)) {
  a <- tabulate(cluster_list[day.ref[i]:(day.ref[i+1]-1)], nbins = 60)
  a <- c(a, sum(a))
  counts <- rbind(counts, a)
  rownames(counts) <- 1:nrow(counts)
}
counts <- data.frame(counts)
site <- c("GympieNP", "WoondumNP")
sites <- rep(site, each=(nrow(counts)/2))
counts <- cbind(counts, sites, dateSeq30min)
# save the 30minute vector
write.csv(counts, "data/weather/30minute.csv", row.names = FALSE)
rm(counts)
min30_data <- read.csv("data/weather/30minute.csv", header = T)

# generate one-hour counts for each cluster + sum
# the sum checks for NAs
day.ref <- seq(1,(398*2*1440+1),60)
counts <- NULL
for(i in 1:(length(day.ref)-1)) {
  a <- tabulate(cluster_list[day.ref[i]:(day.ref[i+1]-1)], 
                nbins = 60)
  a <- c(a, sum(a))
  counts <- rbind(counts, a)
  rownames(counts) <- 1:nrow(counts)
}
counts <- data.frame(counts)
sites <- rep(site, each=(nrow(counts)/2))
counts <- cbind(counts, sites, dateSeq60min)
# save the 60minute vector
write.csv(counts, "data/weather/60minute.csv", 
          row.names = FALSE)
rm(counts)
min60_data <- read.csv("data/weather/60minute.csv", header = T)

# generate two-hour counts for each cluster + sum
# the sum checks for NAs
day.ref <- seq(1,(398*2*1440+1), 120)
counts <- NULL
for(i in 1:(length(day.ref)-1)) {
  a <- tabulate(cluster_list[day.ref[i]:(day.ref[i+1]-1)], 
                nbins = 60)
  a <- c(a, sum(a))
  counts <- rbind(counts, a)
  rownames(counts) <- 1:nrow(counts)
}
counts <- data.frame(counts)
sites <- rep(site, each=(nrow(counts)/2))
counts <- cbind(counts, sites, dateSeq120min)
# save the 120minute vector
write.csv(counts, "data/weather/120minute.csv", 
          row.names = FALSE)
rm(counts)
min120_data <- read.csv("data/weather/120minute.csv", header = T)

# correlations between clusters and weather ----------------------
# remove all objects in the global environment
rm(list = ls())

min30_data <- read.csv("data/weather/30minute.csv", header = T)
rain <- read.csv("data/weather/hour_half_rain.csv", header = T)
rain <- rbind(rain, rain)
# the weather data starts at 2016-02-07 10:30 am this means that this records
# the rain that occured in the last half an hour ie 10:00 to 10:30 and ends
# at 2016-07-23 23:30
a <- which(min30_data$dateSeq30min=="2016-02-07 10:00")
b <- which(min30_data$dateSeq30min=="2016-07-23 23:30")

# match the rain and the cluster data
rain_half_hour <- rbind(min30_data[a[1]:b[1],],
                    min30_data[a[2]:b[2],])
# Note: It may look like the times do not match but the rain in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

rain_half_hour <- cbind(rain, rain_half_hour)
a <- which(rain_half_hour$X61 < 29)
missing <- which (!complete.cases(rain_half_hour))
rain_half_hour <- rain_half_hour[-c(missing, a),]
for(i in 1:nrow(rain_half_hour)) {
  rain_half_hour$all_rain[i] <- sum(rain_half_hour$X2[i], 
                                rain_half_hour$X10[i],
                                rain_half_hour$X18[i], 
                                rain_half_hour$X21[i],
                                rain_half_hour$X38[i], 
                                rain_half_hour$X54[i],
                                rain_half_hour$X59[i], 
                                rain_half_hour$X60[i])
}
# find the correlations
a <- which(rain_half_hour$sites=="GympieNP")
gympie <- rain_half_hour[a,]
gympie <- gympie[,-c(3,4)]

a <- which(rain_half_hour$sites=="WoondumNP")
woondum <- rain_half_hour[a,]
woondum <- woondum[,-c(3,4)]
a_gympie <- cor(gympie[,c(2,5:64,length(gympie))], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64,length(woondum))], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/Correlation_matrix_weather_half_hour_gympie.csv")
write.csv(a_woondum, file = "data/weather/Correlation_matrix_weather_half_hour_woondum.csv")

a_gym <- abs(cor(y = gympie[,c(2)], x = gympie[,c(5:64)], use = "complete.obs"))

# Calculate the 1 hour correlation --------------------------------------------
min60_data <- read.csv("data/weather/60minute.csv", header = T)
rain <- read.csv("data/weather/hour1_rain.csv", header = T)
rain <- rain[,c(1,2,5,6)]
rain <- rbind(rain, rain)

a <- which(min60_data$dateSeq60min=="2016-02-07 10:00")
b <- which(min60_data$dateSeq60min=="2016-07-23 23:00")

# match the rain and the cluster data
rain_1hour <- rbind(min60_data[a[1]:b[1],],
                    min60_data[a[2]:b[2],])
# Note: It may look like the times do not match but the rain in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

rain_1hour <- cbind(rain, rain_1hour)
a <- which(rain_1hour$X61 < 58)
missing <- which (!complete.cases(rain_1hour))
rain_1hour <- rain_1hour[-c(missing, a),]
for(i in 1:nrow(rain_1hour)) {
  rain_1hour$all_rain[i] <- sum(rain_1hour$X2[i], 
                                rain_1hour$X10[i],
                                rain_1hour$X18[i], 
                                rain_1hour$X21[i],
                                rain_1hour$X38[i], 
                                rain_1hour$X54[i],
                                rain_1hour$X59[i], 
                                rain_1hour$X60[i])
}
# find the correlations
a <- which(rain_1hour$sites=="GympieNP")
gympie <- rain_1hour[a,]

a <- which(rain_1hour$sites=="WoondumNP")
woondum <- rain_1hour[a,]

a_gympie <- cor(gympie[,c(2,5:64,length(gympie))], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64,length(woondum))], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/Correlation_matrix_weather_1hour_gympie.csv")
write.csv(a_woondum, file = "data/weather/Correlation_matrix_weather_1hour_woondum.csv")

a_gym <- abs(cor(y = gympie[,c(2)], x = gympie[,c(5:64)], use = "complete.obs"))

# Calculate the 2 hour correlation --------------------------------------------
min120_data <- read.csv("data/weather/120minute.csv", header = T)
rain <- read.csv("data/weather/hour2_rain.csv", header = T)
rain <- rbind(rain, rain)

a <- which(min120_data$dateSeq120min=="2016-02-07 10:00")
b <- which(min120_data$dateSeq120min=="2016-07-23 22:00")

# match the rain and the cluster data
rain_2hour <- rbind(min120_data[a[1]:b[1],],
                    min120_data[a[2]:b[2],])
# Note: It may look like the times do not match but the rain in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

rain_2hour <- cbind(rain, rain_2hour)
a <- which(rain_2hour$X61 < 116)
missing <- which (!complete.cases(rain_2hour))
rain_2hour <- rain_2hour[-c(missing, a),]
for(i in 1:nrow(rain_2hour)) {
  rain_2hour$all_rain[i] <- sum(rain_2hour$X2[i], 
                                rain_2hour$X10[i],
                                rain_2hour$X18[i], 
                                rain_2hour$X21[i],
                                rain_2hour$X38[i], 
                                rain_2hour$X54[i],
                                rain_2hour$X59[i], 
                                rain_2hour$X60[i])
}
# find the correlations
a <- which(rain_2hour$sites=="GympieNP")
gympie <- rain_2hour[a,]

a <- which(rain_2hour$sites=="WoondumNP")
woondum <- rain_2hour[a,]

a_gympie <- cor(gympie[,c(2,5:64,length(rain_2hour))], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64,length(rain_2hour))], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/Correlation_matrix_weather_2hour_gympie.csv")
write.csv(a_woondum, file = "data/weather/Correlation_matrix_weather_2hour_woondum.csv")

a_gym <- abs(cor(y = gympie[,c(2)], x = gympie[,c(5:64)], use = "complete.obs"))


#############################################################
# Tewantin Files -----------------------------------------------------
options(scipen=999)
library(jsonlite)

folder <- "C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Resources\\Weather\\"
TewantinFiles <- list.files(path=folder,recursive = TRUE, full.names = TRUE,
                            pattern = "*_IDQ60801.94570.json")

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
# remove data not on the half hour
minutes <- as.data.frame(substr(data[,7],8,8))
min.to.remove <- c("1", "2", "3", "4", "5", "6", "7", "8", "9")
refer <- which(minutes[,] %in% min.to.remove)
ten_minutes <- as.data.frame(substr(data[,7],7,8))
ten_minutes_to_remove <- c("10","20","40","50")
refer1 <- which(ten_minutes[,] %in% ten_minutes_to_remove)

tewantin.data <- data[-c(refer, refer1),]
headings <- c("name", "state", "time_zone","sort_order",
              "wmo", "history_product", "date_time",
              "date_time_full", "lat","lon","apparent_t",
              "gust_kmh", "air_temp","dewpt","press",
              "rain_trace","rel_hum", "wind_dir",            
              "wind_spd_kmh")
colnames(tewantin.data) <- headings

# calculate the rain per half hour from the rain trace (column 16)
rain <- as.numeric(tewantin.data[1,16])
for (i in 2:length(tewantin.data[,16])) {
  ra <- as.numeric(tewantin.data[i,16]) - as.numeric(tewantin.data[(i-1),16])
  if  (substr((tewantin.data[i,7]),4,10) == "09:30am") {
    ra <- as.numeric(tewantin.data[i,16])
  }
  if (ra < 0 & !is.na(ra)) {
    ra <- as.numeric(tewantin.data[i,16])
  }
  rain <- c(rain, ra)
}
rain <- data.frame(rain)
max(rain, na.rm = T)

tewantin.data[,20] <- rain

a <- which(tewantin.data$date_time_full==20160724003000)
write.csv(tewantin.data[1:a,], "data/weather/tewantin/Tewantin_weather1.csv", 
          row.names = FALSE)
tewantin_weather <- read.csv("data/weather/tewantin/Tewantin_weather1.csv")

####################################################################

#tewantin_weather_matrix <- 
#  read.csv("C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Resources\\Weather\\tewantin_weather_matrix.csv")

# generate a sequence of dates -----------------------
start <- strptime("20160207", format="%Y%m%d")
finish <- strptime("20160731", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
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

# time sequence
times <- c("000000","003000","010000","013000",
           "020000","023000","030000","033000",
           "040000","043000","050000","053000",
           "060000","063000","070000","073000",
           "080000","083000","090000","093000",
           "100000","103000","110000","113000",
           "120000","123000","130000","133000",
           "140000","143000","150000","153000",
           "160000","163000","170000","173000",
           "180000","183000","190000","193000",
           "200000","203000","210000","213000",
           "220000","223000","230000","233000")
dates1 <- unique(substr(tewantin.data$date_time_full,1,8))
date_list <- NULL
for(i in 1:length(dates)) {
  for(j in 1:length(times)) {
    lst <- paste(dates[i],times[j],sep = "")
    date_list <- c(date_list, lst)
  }
}

# determine a list of missing observations within the list of dates ---------------------------------
check_dates <- matrix(nrow = length(date_list),
                      ncol = 2)
check_dates <- data.frame(check_dates)
colnames(check_dates) <- c("full_dates", "check")
check_dates$full_dates <- date_list
check_dates$check <- "NO"
full_dates_list <- unique(tewantin.data$date_time_full)
for(i in 1:length(full_dates_list)) {
  a <- which(check_dates$full_dates==full_dates_list[i])
  check_dates$check[a] <- "YES"
}
a <- which(check_dates$check=="NO")
#1:21, 4961:4965, 5832:5834, 6117:6119
missing_obs <- check_dates$full_dates[a]
result <- rle(diff(a))
result <- c(result$lengths[1:2],0,result$lengths[3:length(result$lengths)])
result <- result[seq(1, length(result),2)]  

result <- result + 1
result <- c(0, result)
a
cum_sum <- cumsum(result) + 1

# add rows where data is missing
if(a[1] == 1) {
  for(i in 3:length(result)) {
    mydf <- matrix(nrow = result[i], 
                   ncol = ncol(tewantin_weather), "NA")
    mydf <- data.frame(mydf)
    colnames(mydf) <- colnames(tewantin_weather)
    new_tewantin_weather <- rbind(tewantin_weather[1:(a[cum_sum[i-1]]-1-result[2]), ], 
                                mydf, 
                                tewantin_weather[(a[cum_sum[i-1]]-result[2]):nrow(tewantin_weather), ])
    # correct the row names
    row.names(new_tewantin_weather) <- 1:nrow(new_tewantin_weather)  
    tewantin_weather <- new_tewantin_weather
    rm(new_tewantin_weather)
  }  
}

if(a[1] > 1) {
  for(i in 2:length(result)) {
    mydf <- matrix(nrow = result[i], 
                   ncol = ncol(tewantin_weather), "NA")
    mydf <- data.frame(mydf)
    colnames(mydf) <- colnames(tewantin_weather)
    new_tewantin_weather <- rbind(tewantin_weather[1:(a[cum_sum[i-1]]-1-result[2]), ], 
                                mydf, 
                                tewantin_weather[(a[cum_sum[i-1]]-result[2]):nrow(tewantin_weather), ])
    # correct the row names
    row.names(new_tewantin_weather) <- 1:nrow(new_tewantin_weather)  
    tewantin_weather <- new_tewantin_weather
    rm(new_tewantin_weather)
  }  
}

# Calculate 1 hour rain sums -----------------------------------
tewantin_weather$rain_1hour <- 0
hour1_seq <- seq(2,nrow(tewantin_weather),2)
tewantin_weather$rain <- as.numeric(tewantin_weather$rain)
for(i in 1:length(hour1_seq)) {
  tewantin_weather$rain_1hour[hour1_seq[i]] <- sum(tewantin_weather$rain[hour1_seq[i]-1],
                                                 tewantin_weather$rain[hour1_seq[i]]) 
}

View(tewantin_weather)

# Calculate 2 hour rain sums --------------------------------------
tewantin_weather$rain_2hours <- 0
hour2_seq <- seq(4,nrow(tewantin_weather),4)
tewantin_weather$rain <- as.numeric(tewantin_weather$rain)
for(i in 1:length(hour2_seq)) {
  tewantin_weather$rain_2hours[hour2_seq[i]] <- sum(tewantin_weather$rain[hour2_seq[i]-3],
                                                  tewantin_weather$rain[hour2_seq[i]-2],
                                                  tewantin_weather$rain[hour2_seq[i]-1],
                                                  tewantin_weather$rain[hour2_seq[i]]) 
}
View(tewantin_weather)
#setwd("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/")
#write.csv(tewantin_weather, "data/weather/tewantin_rain_protected.csv", row.names = F)

# read tewantin_weather file -----------------------------------
tewantin_weather <- read.csv("data/weather/tewantin/tewantin_rain_protected.csv", header = T) 
hour_half_rain <- cbind(tewantin_weather$date_time_full,
                        round(tewantin_weather$rain,1))
hour1_rain <- cbind(tewantin_weather$date_time_full[hour1_seq], 
                    round(tewantin_weather$rain_1hour[hour1_seq],1))
hour2_rain <- cbind((tewantin_weather$date_time_full[hour2_seq]), 
                    round(tewantin_weather$rain_2hour[hour2_seq],1))

hour_half_rain <- data.frame(hour_half_rain)
colnames(hour_half_rain) <- c("date-time","rain")
hour1_rain <- data.frame(hour1_rain)
colnames(hour1_rain) <- c("date-time","rain")
hour2_rain <- data.frame(hour2_rain)
colnames(hour2_rain) <- c("date-time","rain")

hour_half_rain$date <- "date"
hour_half_rain$time <- "time"
hour1_rain$date <- "date"
hour1_rain$time <- "time"
hour2_rain$date <- "date"
hour2_rain$time <- "time"

for(i in 1:nrow(hour_half_rain)) {
  hour_half_rain$date[i] <- paste(substr(hour_half_rain$'date-time'[i],1,4),
                                  substr(hour_half_rain$'date-time'[i],5,6),
                                  substr(hour_half_rain$'date-time'[i],7,8), 
                                  sep = "-")
  hour_half_rain$time[i] <- paste(substr(hour_half_rain$'date-time'[i],9,10),
                                  substr(hour_half_rain$'date-time'[i],11,12),
                                  sep = ":")
}
for(i in 1:nrow(hour1_rain)) {
  hour1_rain$date[i] <- paste(substr(hour1_rain$'date-time'[i],1,4),
                              substr(hour1_rain$'date-time'[i],5,6),
                              substr(hour1_rain$'date-time'[i],7,8), 
                              sep = "-")
  hour1_rain$time[i] <- paste(substr(hour1_rain$'date-time'[i],9,10),
                              substr(hour1_rain$'date-time'[i],11,12),
                              sep = ":")
}
for(i in 1:nrow(hour2_rain)) {
  hour2_rain$date[i] <- paste(substr(hour2_rain$'date-time'[i],1,4),
                              substr(hour2_rain$'date-time'[i],5,6),
                              substr(hour2_rain$'date-time'[i],7,8), 
                              sep = "-")
  hour2_rain$time[i] <- paste(substr(hour2_rain$'date-time'[i],9,10),
                              substr(hour2_rain$'date-time'[i],11,12),
                              sep = ":")
}

write.csv(hour_half_rain, "data/weather/tewantin/hour_half_rain.csv", row.names = FALSE)
write.csv(hour1_rain, "data/weather/tewantin/hour1_rain.csv", row.names = FALSE)
write.csv(hour2_rain, "data/weather/tewantin/hour2_rain.csv", row.names = FALSE)

#a <- which(is.na(hour1_rain$rain=="NA"))
#hour1_rain$`date-time`[a]

rain <- read.csv("data/weather/tewantin/hour_half_rain.csv", header = T)


# remove all objects in the global environment
rm(list = ls())

# load the cluster list with the missing minutes in place -------------------
k1_value <- 25000
k2_value <- 60
column <- k2_value/5

# generate the time sequences
startDate = as.POSIXct("2015-06-22 00:00")
endDate = as.POSIXct("2016-07-24 00:00")
dateSeq30min = substr(seq(from=startDate, to=endDate, by="30 min"),1,16)
dateSeq60min = substr(seq(from=startDate, to=endDate, by="60 min"),1,16)
dateSeq120min = substr(seq(from=startDate, to=endDate, by="120 min"),1,16)

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list (missing minutes)
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

# generate a sequence that can be used for half-hour, 
# one-hour and two-hour sequences
day.ref <- seq(1,(398*2*1440+1),30)

min30_data <- read.csv("data/weather/30minute.csv", header = T)

min60_data <- read.csv("data/weather/60minute.csv", header = T)

min120_data <- read.csv("data/weather/120minute.csv", header = T)

# correlations between clusters and weather ----------------------
# remove all objects in the global environment
rm(list = ls())

min30_data <- read.csv("data/weather/30minute.csv", header = T)
rain <- read.csv("data/weather/tewantin/hour_half_rain.csv", header = T)
rain <- rbind(rain, rain)
rain <- rain[1:(nrow(rain)-2),]
# the weather data starts at 2016-02-07 10:30 am this means that this records
# the rain that occured in the last half an hour ie 10:00 to 10:30 and ends
# at 2016-07-23 23:30
a <- which(min30_data$dateSeq30min=="2016-02-07 10:00")
b <- which(min30_data$dateSeq30min=="2016-07-23 23:30")

# match the rain and the cluster data
rain_half_hour <- rbind(min30_data[a[1]:b[1],],
                        min30_data[a[2]:b[2],])
# Note: It may look like the times do not match but the rain in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

rain_half_hour <- cbind(rain, rain_half_hour)
a <- which(rain_half_hour$X61 < 29)
missing <- which (!complete.cases(rain_half_hour))
rain_half_hour <- rain_half_hour[-c(missing, a),]
for(i in 1:nrow(rain_half_hour)) {
  rain_half_hour$all_rain[i] <- sum(rain_half_hour$X2[i], 
                                    rain_half_hour$X10[i],
                                    rain_half_hour$X18[i], 
                                    rain_half_hour$X21[i],
                                    rain_half_hour$X38[i], 
                                    rain_half_hour$X54[i],
                                    rain_half_hour$X59[i], 
                                    rain_half_hour$X60[i])
}
# find the correlations
a <- which(rain_half_hour$sites=="GympieNP")
gympie <- rain_half_hour[a,]

a <- which(rain_half_hour$sites=="WoondumNP")
woondum <- rain_half_hour[a,]

a_gympie <- cor(gympie[,c(2,5:64,length(gympie))], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64,length(woondum))], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/tewantin/Correlation_matrix_Tewantin_weather_half_hour_gympie.csv")
write.csv(a_woondum, file = "data/weather/tewantin/Correlation_matrix_Tewantin_weather_half_hour_woondum.csv")

# Calculate the 1 hour correlation --------------------------------------------
min60_data <- read.csv("data/weather/60minute.csv", header = T)
rain <- read.csv("data/weather/tewantin/hour1_rain.csv", header = T)
rain <- rbind(rain, rain)

a <- which(min60_data$dateSeq60min=="2016-02-07 10:00")
b <- which(min60_data$dateSeq60min=="2016-07-23 23:00")

# match the rain and the cluster data
rain_1hour <- rbind(min60_data[a[1]:b[1],],
                    min60_data[a[2]:b[2],])
# Note: It may look like the times do not match but the rain in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

rain_1hour <- cbind(rain, rain_1hour)
a <- which(rain_1hour$X61 < 58)
missing <- which (!complete.cases(rain_1hour))
rain_1hour <- rain_1hour[-c(missing, a),]
for(i in 1:nrow(rain_1hour)) {
  rain_1hour$all_rain[i] <- sum(rain_1hour$X2[i], 
                                rain_1hour$X10[i],
                                rain_1hour$X18[i], 
                                rain_1hour$X21[i],
                                rain_1hour$X38[i], 
                                rain_1hour$X54[i],
                                rain_1hour$X59[i], 
                                rain_1hour$X60[i])
}
# find the correlations
a <- which(rain_1hour$sites=="GympieNP")
gympie <- rain_1hour[a,]

a <- which(rain_1hour$sites=="WoondumNP")
woondum <- rain_1hour[a,]

a_gympie <- cor(gympie[,c(2,5:64,length(gympie))], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64,length(woondum))], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/tewantin/Correlation_matrix_Tewantin_weather_1hour_gympie.csv")
write.csv(a_woondum, file = "data/weather/tewantin/Correlation_matrix_Tewantin_weather_1hour_woondum.csv")

# Calculate the 2 hour correlation --------------------------------------------
min120_data <- read.csv("data/weather/120minute.csv", header = T)
rain <- read.csv("data/weather/tewantin/hour2_rain.csv", header = T)
rain <- rbind(rain, rain)

a <- which(min120_data$dateSeq120min=="2016-02-07 10:00")
b <- which(min120_data$dateSeq120min=="2016-07-23 22:00")

# match the rain and the cluster data
rain_2hour <- rbind(min120_data[a[1]:b[1],],
                    min120_data[a[2]:b[2],])
# Note: It may look like the times do not match but the rain in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

rain_2hour <- cbind(rain, rain_2hour)
a <- which(rain_2hour$X61 < 116)
missing <- which (!complete.cases(rain_2hour))
rain_2hour <- rain_2hour[-c(missing, a),]
for(i in 1:nrow(rain_2hour)) {
  rain_2hour$all_rain[i] <- sum(rain_2hour$X2[i], 
                                rain_2hour$X10[i],
                                rain_2hour$X18[i], 
                                rain_2hour$X21[i],
                                rain_2hour$X38[i], 
                                rain_2hour$X54[i],
                                rain_2hour$X59[i], 
                                rain_2hour$X60[i])
}
# find the correlations
a <- which(rain_2hour$sites=="GympieNP")
gympie <- rain_2hour[a,]

a <- which(rain_2hour$sites=="WoondumNP")
woondum <- rain_2hour[a,]

a_gympie <- cor(gympie[,c(2,5:64,length(rain_2hour))], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64,length(rain_2hour))], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/tewantin/Correlation_matrix_Tewantin_weather_2hour_gympie.csv")
write.csv(a_woondum, file = "data/weather/tewantin/Correlation_matrix_Tewantin_weather_2hour_woondum.csv")

##
# Correlation with pressure ------------------------------------
# read gympie_weather file (pressure) -----------------------------------
gympie_weather <- read.csv("data/weather/gympie_rain_protected.csv", header = T) 
hour1_seq <- seq(2,nrow(gympie_weather),2)
hour2_seq <- seq(4,nrow(gympie_weather),4)
hour_half_pressure <- cbind(gympie_weather$date_time_full,
                        round(gympie_weather$press,1))
hour1_pressure <- cbind(gympie_weather$date_time_full[hour1_seq], 
                    round(gympie_weather$press[hour1_seq],1))
hour2_pressure <- cbind((gympie_weather$date_time_full[hour2_seq]), 
                    round(gympie_weather$press[hour2_seq],1))
hour1_pressure <- data.frame(hour1_pressure)
hour2_pressure <- data.frame(hour2_pressure)
hour_half_pressure <- data.frame(hour_half_pressure)

colnames(hour_half_pressure) <- c("date-time","pressure")
colnames(hour1_pressure) <- c("date-time","pressure")
colnames(hour2_pressure) <- c("date-time","pressure")

hour_half_pressure$date <- "date"
hour_half_pressure$time <- "time"
hour1_pressure$date <- "date"
hour1_pressure$time <- "time"
hour2_pressure$date <- "date"
hour2_pressure$time <- "time"

for(i in 1:nrow(hour_half_pressure)) {
  hour_half_pressure$date[i] <- paste(substr(hour_half_pressure$'date-time'[i],1,4),
                                  substr(hour_half_pressure$'date-time'[i],5,6),
                                  substr(hour_half_pressure$'date-time'[i],7,8), 
                                  sep = "-")
  hour_half_pressure$time[i] <- paste(substr(hour_half_pressure$'date-time'[i],9,10),
                                  substr(hour_half_pressure$'date-time'[i],11,12),
                                  sep = ":")
}
for(i in 1:nrow(hour1_pressure)) {
  hour1_pressure$date[i] <- paste(substr(hour1_pressure$'date-time'[i],1,4),
                              substr(hour1_pressure$'date-time'[i],5,6),
                              substr(hour1_pressure$'date-time'[i],7,8), 
                              sep = "-")
  hour1_pressure$time[i] <- paste(substr(hour1_pressure$'date-time'[i],9,10),
                              substr(hour1_pressure$'date-time'[i],11,12),
                              sep = ":")
}
for(i in 1:nrow(hour2_pressure)) {
  hour2_pressure$date[i] <- paste(substr(hour2_pressure$'date-time'[i],1,4),
                              substr(hour2_pressure$'date-time'[i],5,6),
                              substr(hour2_pressure$'date-time'[i],7,8), 
                              sep = "-")
  hour2_pressure$time[i] <- paste(substr(hour2_pressure$'date-time'[i],9,10),
                              substr(hour2_pressure$'date-time'[i],11,12),
                              sep = ":")
}

write.csv(hour_half_pressure, "data/weather/hour_half_pressure.csv", row.names = FALSE)
write.csv(hour1_pressure, "data/weather/hour1_pressure.csv", row.names = FALSE)
write.csv(hour2_pressure, "data/weather/hour2_pressure.csv", row.names = FALSE)

# correlations between clusters and gympie_weather (pressure) ----------------------
# remove all objects in the global environment
rm(list = ls())

min30_data <- read.csv("data/weather/30minute.csv", header = T)
pressure <- read.csv("data/weather/hour_half_pressure.csv", header = T)
pressure <- rbind(pressure, pressure)
# the weather data starts at 2016-02-07 10:30 am this means that this records
# the rain that occured in the last half an hour ie 10:00 to 10:30 and ends
# at 2016-07-23 23:30
a <- which(min30_data$dateSeq30min=="2016-02-07 10:00")
b <- which(min30_data$dateSeq30min=="2016-07-23 23:30")

# match the rain and the cluster data
pressure_half_hour <- rbind(min30_data[a[1]:b[1],],
                        min30_data[a[2]:b[2],])
# Note: It may look like the times do not match but the rain in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

pressure_half_hour <- cbind(pressure, pressure_half_hour)
a <- which(pressure_half_hour$X61 < 29)
missing <- which (!complete.cases(pressure_half_hour))
pressure_half_hour <- pressure_half_hour[-c(missing, a),]

# find the correlations
a <- which(pressure_half_hour$sites=="GympieNP")
gympie <- pressure_half_hour[a,]

a <- which(pressure_half_hour$sites=="WoondumNP")
woondum <- pressure_half_hour[a,]

a_gympie <- cor(gympie[,c(2,5:64)], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64)], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/Correlation_matrix_weather_half_hour_gympie_pressure.csv")
write.csv(a_woondum, file = "data/weather/Correlation_matrix_weather_half_hour_woondum_pressure.csv")

a_gym <- abs(cor(y = gympie[,c(2)], x = gympie[,c(5:64)], use = "complete.obs"))

# Calculate the 1 hour correlation --------------------------------------------
min60_data <- read.csv("data/weather/60minute.csv", header = T)
pressure <- read.csv("data/weather/hour1_pressure.csv", header = T)
pressure <- rbind(pressure, pressure)

a <- which(min60_data$dateSeq60min=="2016-02-07 10:00")
b <- which(min60_data$dateSeq60min=="2016-07-23 23:00")

# match the pressure and the cluster data
pressure_1hour <- rbind(min60_data[a[1]:b[1],],
                    min60_data[a[2]:b[2],])
# Note: It may look like the times do not match but the pressure in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

pressure_1hour <- cbind(pressure, pressure_1hour)
a <- which(pressure_1hour$X61 < 58)
missing <- which (!complete.cases(pressure_1hour))
pressure_1hour <- pressure_1hour[-c(missing, a),]
# find the correlations
a <- which(pressure_1hour$sites=="GympieNP")
gympie <- pressure_1hour[a,]

a <- which(pressure_1hour$sites=="WoondumNP")
woondum <- pressure_1hour[a,]

a_gympie <- cor(gympie[,c(2,5:64)], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64)], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/Correlation_matrix_weather_1hour_gympie_pressure.csv")
write.csv(a_woondum, file = "data/weather/Correlation_matrix_weather_1hour_woondum_pressure.csv")

# Calculate the 2 hour correlation --------------------------------------------
min120_data <- read.csv("data/weather/120minute.csv", header = T)
pressure <- read.csv("data/weather/hour2_pressure.csv", header = T)
pressure <- rbind(pressure, pressure)

a <- which(min120_data$dateSeq120min=="2016-02-07 10:00")
b <- which(min120_data$dateSeq120min=="2016-07-23 22:00")

# match the pressure and the cluster data
pressure_2hour <- rbind(min120_data[a[1]:b[1],],
                    min120_data[a[2]:b[2],])
# Note: It may look like the times do not match but the pressure in the
# weather data is for the previous half-hour and the count in the
# cluster data is for the next half-hour

pressure_2hour <- cbind(pressure, pressure_2hour)
a <- which(pressure_2hour$X61 < 116)
missing <- which (!complete.cases(pressure_2hour))
pressure_2hour <- pressure_2hour[-c(missing, a),]
# find the correlations
a <- which(pressure_2hour$sites=="GympieNP")
gympie <- pressure_2hour[a,]

a <- which(pressure_2hour$sites=="WoondumNP")
woondum <- pressure_2hour[a,]

a_gympie <- cor(gympie[,c(2,5:64,length(pressure_2hour))], use = "complete.obs")
a_woondum <- cor(woondum[,c(2,5:64,length(pressure_2hour))], use = "complete.obs")
write.csv(a_gympie, file = "data/weather/Correlation_matrix_weather_2hour_gympie_pressure.csv")
write.csv(a_woondum, file = "data/weather/Correlation_matrix_weather_2hour_woondum_pressure.csv")


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
  if(substr(gympie.data[1,8],9,12)==list[i]) {
    ref1 <- i
    stop
  }
}
#set ref to determine the date of the last row
ref3 <- substr(gympie_weather[length(gympie_weather[,1]),8],1,8)

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

no.of.rows <- length(gympie.data$name)
gympie_matrix <- matrix(data = NA, nrow=floor(no.of.rows/50), 
                        ncol = 1+10*48)
colnames(gympie_matrix) <- columnNames

# fill matrix with weather data
#seq1 <- seq(ref2, length(gympie_weather$name),48)
seq2 <- seq(2,472,10)
count <- ref2
for(j in 1:length(gympie_matrix[,1])) {
  for(k in 1:length(seq2)) {
    gympie_matrix[j,seq2[k]:(seq2[k]+9)] <- unlist(gympie.data[count,11:20], use.names = F)
    gympie_matrix[j,(seq2[k]+7)] <- as.character(gympie.data[count,18])
    count <- count + 1
  }
  ifelse(ref1==0, 
         gympie_matrix[j,1] <- substr(gympie.data[(count-1),8],1,8),
         gympie_matrix[j,1] <- substr(gympie.data[(count-49),8],1,8))
}
write.csv(gympie_matrix, paste("gympie_weather_matrix_", ref3, ".csv"), row.names = FALSE)

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
