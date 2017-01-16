# Cluster Volatility

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Read cluster list -----------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# choose site
site <- "GympieNP"
#site <- "WoondumNP"

# Set value (n) for the number of minutes where the time interval 
# will be set to (2n + 1)
n <- 10

k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
rm(k1_value, k2_value)
# remove unneeded values
load(file_name)
# load the cluster list corresponding to k1 and k2 value
cluster_list <- get(file_name_short, envir=globalenv())[,column]

# remove unneeded objects from global environment
rm(hclust_clusters_25000, file_name, file_name_short, column)
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Reconstitute the cluster list by adding in missing minutes ----
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
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

cluster_list_Gympie <- cluster_list[1:(length(cluster_list)/2)]
cluster_list_Woondum <- cluster_list[((length(cluster_list)/2)+1):length(cluster_list)]
rm(list, list1, full_length, removed_minutes)

if(site=="GympieNP") {
  clusterList <- cluster_list_Gympie
}
if(site=="WoondumNP") {
  clusterList <- cluster_list_Woondum
}

rm(cluster_list_Gympie, cluster_list_Woondum)
rm(cluster_list)
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a list of dates (General) ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
# IMPORTANT:  Ensure the mapping file contains the mapping of 
# files at least to the finish date
finish <-  strptime("20160723", format="%Y%m%d") 
dates <- seq(start, finish, by = "1440 mins")
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}
dates <- date.list
rm(dat,start, finish, i, date.list)
Gym_dates <- NULL
for(i in 1:length(dates)) {
  Gym_dat <- paste("Gym_",dates[i], sep = "")
  Gym_dates <- c(Gym_dates, Gym_dat)
}
Woon_dates <- NULL
for(i in 1:length(dates)) {
  Woon_dat <- paste("Woon_",dates[i], sep = "")
  Woon_dates <- c(Woon_dates, Woon_dat)
}
rm(dates, i, Gym_dat, Woon_dat, cluster_list)
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Set interval
interval <- 2*n + 1
 
# change n value above
# Time interval equals 2*n + 1 
######## Create references for plotting dates and times ################
timeRef <- 0 #<-indices$minute.of.day[1]
offset <- 0 - timeRef 

timePos   <- seq(0, length(clusterList), by = 360)
timeLabel <- c("00:00","6:00","12:00","18:00")
timeLabel <- rep(timeLabel, length.out=(length(timePos))) 

datePos <- c(seq((offset+720), length(clusterList), by = 1440))
dateLabel <- Gym_dates
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# colour management
rain <- c(59,18,10,54,2,21,38,60)
wind <- c(42,47,51,56,52,45,8,40,24,19,46,28,9,25,30,20)
birds <- c(43,37,57,3,58,11,33,15,14,39,4)
insects <- c(29,17,1,27,22,26)
cicada <- c(48,44,34,7,12,32,16)
plane <- c(49,23)
quiet <- c(13,5,6,53,36,31,50,35,55,41)
na <- 61

insect_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
bird_col <- "#009E73"
cicada_col <- "#E69F00"
quiet_col <- "#999999"
plane_col <- "#CC79A7"
na_col <- "white"

#normalisedCount_dataframe <- NULL
#normalisedCount_bird_dataframe <- NULL
#normalisedCount_insect_dataframe <- NULL
#normalisedCount_cicada_dataframe <- NULL
#normalisedCount_rain_dataframe <- NULL
#normalisedCount_quiet_dataframe <- NULL
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
start <- seq(1, 573121, 10080)
start[1] <- n + 1 
end <- c(seq(10080, 573120, 10080), (573120-n))

count <- NULL
uniqueClust <- NULL
uniqueClust_birds <- NULL
uniqueClust_insects <- NULL
uniqueClust_cicada <- NULL
uniqueClust_rain <- NULL
uniqueClust_quiet <- NULL
uniqueClust_wind <- NULL
for(j in 1:1) { #(length(end)-1)) {
  for (i in start[j]:end[j]) {  #(length(clusterList)-(n+1))) {
    image_num <- j
    # find the moving count of unique clusters from 1 to 16, 2 to 17 etc
    series <- clusterList[(i-n):(i+n)]
    # find the number of the unique clusters and subtract the clusters 
    # occupying one minute only
    uniqClust <- unique(series)
    uniqClust_length <- length(table(series)) - length(which(table(series) < 2))
    uniqueClust <- c(uniqueClust, uniqClust_length)
    # find the number of unique bird clusters and substract the clusters
    # occupying one minute only
    # 1. birds
    a <- which(uniqClust %in% birds)
    b <- which(series %in% birds)
    uniqClust_birds <- length(a) - length(which(table(series[b]) < 2))
    uniqueClust_birds <- c(uniqueClust_birds, uniqClust_birds)
    # 2. insects
    a <- which(uniqClust %in% insects)
    b <- which(series %in% insects)
    uniqClust_insects <- length(a) - length(which(table(series[b]) < 2))
    uniqueClust_insects <- c(uniqueClust_insects, uniqClust_insects)
    # 3. cicada
    a <- which(uniqClust %in% cicada)
    b <- which(series %in% cicada)
    uniqClust_cicada <- length(a) - length(which(table(series[b]) < 2))
    uniqueClust_cicada <- c(uniqueClust_cicada, uniqClust_cicada)
    # 4. rain
    a <- which(uniqClust %in% rain)
    b <- which(series %in% rain)
    uniqClust_rain <- length(a) - length(which(table(series[b]) < 2))
    uniqueClust_rain <- c(uniqueClust_rain, uniqClust_rain)
    # 5. quiet
    a <- which(uniqClust %in% quiet)
    b <- which(series %in% quiet)
    uniqClust_quiet <- length(a) - length(which(table(series[b]) < 2))
    uniqueClust_quiet <- c(uniqueClust_quiet, uniqClust_quiet)
    # 7. wind
    a <- which(uniqClust %in% wind)
    b <- which(series %in% wind)
    uniqClust_wind <- length(a) - length(which(table(series[b]) < 2))
    uniqueClust_wind <- c(uniqueClust_wind, uniqClust_wind)
    # counting transitions
    aseq <- sequence(rle(series)$lengths)
    # count the number of non-transitions
    #counter <- length(which(aseq==1)) - 1
    # count the number of transitions
    counter <- length(which(aseq > 1))
    count <- c(count, counter)
  }
  Sys.time()
  print(j)
}
rm(aseq, uniqClust, uniqClust_birds, uniqueClust_insects, uniqClust_cicada, uniqClust_rain, uniqueClust_wind,
   uniqClust_quiet)
# Collate data
data <- as.data.frame(matrix(0, ncol = 8, nrow = 564465))

list <- c("count", "uniqueClust", "uniqueClust_birds",
              "uniqueClust_insects", "uniqueClust_cicada",
              "uniqueClust_rain", "uniqueClust_quiet",
              "uniqueClust_wind")

for(i in 5:8) { #length(list)) {
data[1:length(count),i] <- get(list[i])
}

colnames(data) <- c("transition_count", list[2:8])

write.csv(data, paste("volatility\\",site,"_volatility_n_",n,".csv",sep = ""), 
          row.names = F)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# read volatility data---------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())
# choose n and site
n <- 15
site <- "GympieNP"
if(site=="GympieNP") {
  Gympie_data <- read.csv(paste("volatility\\GympieNP_volatility_n_",n,".csv",sep = ""), 
  header = T)  
}
if(site=="WoondumNP") {
  Woondum_data <- read.csv(paste("volatility\\WoondumNP_volatility_n_",n,".csv",sep = ""), header = T)  
}
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a list of dates (For table) ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
# IMPORTANT:  Ensure the mapping file contains the mapping of 
# files at least to the finish date
finish <-  strptime("20160723", format="%Y%m%d") 
dates_min <- seq(start, finish, by = "1 mins")

rm(start, finish)
if(site=="GympieNP") {
  Gym_dates_min <- NULL
  Gym_dates_min <- paste("Gym ", substr(dates_min,1,16), sep = "")
  Gympie_data$time <- Gym_dates_min[(n+1):(n+nrow(Gympie_data))]
}
if(site=="WoondumNP") {
  Woon_dates_min <- NULL
  Woon_dates_min <- paste("Woon ", substr(dates_min, 1, 16), sep = "")
  Woondum_data$time <- Woon_dates_min[(n+1):(n+nrow(Woondum_data))]
}

bird_ratio <- Gympie_data$uniqueClust_birds/Gympie_data$uniqueClust

a <- which(bird_ratio==1)
bird_100 <- Gympie_data[a,]
bird_100$date <- substr(bird_100$time, 5,15)
bird_100$hour <- substr(bird_100$time, 16,17)
bird_100$minute <- substr(bird_100$time, 19,20)
bird_100$month <- substr(bird_100$time, 10,11)
bird_100$year <- substr(bird_100$time, 5,8)
plot(table(bird_100$hour))
sum(table(bird_100$hour))
table(bird_100$uniqueClust_birds)
# distribution of hours for each month
# June
b <- which(bird_100$month=="06" & bird_100$year=="2015")
bird_100_June <- bird_100[b,]
c <- table(bird_100_June$hour)
hour_matrix <- as.data.frame(matrix(0, ncol = 24, nrow = 13))
for(i in length(c)) {
  d <- as.integer(names(c[i]))
  hour_matrix[1,d] <- c[[1]]
}

hist(tabulate(as.integer(bird_100_June$hour)))

b <- which(bird_100$month=="07" & bird_100$year=="2015")
bird_100_July <- bird_100[b,]
plot(table(bird_100_July$hour))

b <- which(bird_100$month=="08" & bird_100$year=="2015")
bird_100_August <- bird_100[b,]
plot(table(bird_100_August$hour))
b <- which(bird_100$month=="09" & bird_100$year=="2015")
bird_100_Sept <- bird_100[b,]
plot(tabulate(bird_100_Sept$hour))
b <- which(bird_100$month=="10" & bird_100$year=="2015")
bird_100_Oct <- bird_100[b,]
plot(tabulate(bird_100_Oct$hour))
b <- which(bird_100$month=="11" & bird_100$year=="2015")
bird_100_Nov <- bird_100[b,]
plot(tabulate(bird_100_Nov$hour))
b <- which(bird_100$month=="12" & bird_100$year=="2015")
bird_100_Dec <- bird_100[b,]
plot(tabulate(bird_100_Dec$hour))
b <- which(bird_100$month=="01" & bird_100$year=="2016")
bird_100_Jan <- bird_100[b,]
plot(tabulate(bird_100_Jan$hour))
b <- which(bird_100$month=="02" & bird_100$year=="2016")
bird_100_Feb <- bird_100[b,]
plot(tabulate(bird_100_Feb$hour))
b <- which(bird_100$month=="03" & bird_100$year=="2016")
bird_100_Mar <- bird_100[b,]
plot(tabulate(bird_100_Mar$hour))






a <- which(bird_ratio > 0.8)
bird_80 <- Gympie_data[a,]
bird_80$date <- substr(bird_80$time, 5,15)
bird_80$hour <- substr(bird_80$time, 16,17)
bird_80$minute <- substr(bird_80$time, 19,20)
plot(table(bird_80$hour))
sum(table(bird_80$hour))
table(bird_80$uniqueClust)

a <- which(bird_ratio > 0.6)
bird_60 <- Gympie_data[a,]
bird_60$date <- substr(bird_60$time, 5,15)
bird_60$hour <- substr(bird_60$time, 16,17)
bird_60$minute <- substr(bird_60$time, 19,20)
plot(table(bird_60$hour))
sum(table(bird_60$hour))
table(bird_60$uniqueClust)
table(bird_60$uniqueClust_birds)
plot(table(bird_60$uniqueClust))



# Find normalised counts
normalisedCount <- NULL
for (i in 1:length(count)) {
  normalisedCount[i] <- count[i]*(uniqueClust[i])
}
normalisedCount_bird <- NULL
for (i in 1:length(count)) {
  normalisedCount_bird[i] <- (count[i]*uniqueClust_birds[i])/uniqueClust[i]
}
normalisedCount_insects <- NULL
for (i in 1:length(count)) {
  normalisedCount_insects[i] <- count[i]*uniqueClust_insects[i]/uniqueClust[i]
}
if(image_num==1) {
  normalisedCount_dataframe[((image_num-1)*1440+1+n):(image_num*7*1440)] <- normalisedCount
  normalisedCount_bird_dataframe[((image_num-1)*1440+1+n):(image_num*7*1440)] <- normalisedCount_bird
  normalisedCount_insect_dataframe[((image_num-1)*1440+1+n):(image_num*7*1440)] <- normalisedCount_insects
}
if(image_num > 1) {
  normalisedCount_dataframe[((image_num-1)*7*1440+1):(image_num*7*1440)] <- normalisedCount
  normalisedCount_bird_dataframe[((image_num-1)*7*1440+1):(image_num*7*1440)] <- normalisedCount_bird
  normalisedCount_insect_dataframe[((image_num-1)*7*1440+1):(image_num*7*1440)] <- normalisedCount_insects
}
