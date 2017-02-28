# Cluster Volatility
# Note: This code was written to cover only the first 56 weeks (392 days)
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
n <- 2
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
#rain <- c(2,10,18,21,38,54,59,60)
#wind <- c(8,9,19,20,24,25,28,30,40,42,45,46,47,51,52,56)
#birds <- c(3,4,11,14,15,33,37,39,43,57,58)
#insects <- c(1,17,22,26,27,29)
#cicada <- c(7,12,16,32,34,44,48)
#plane <- c(23,49)
#quiet <- c(5,6,13,31,35,36,41,50,53,55)
#na <- 61

rain <- c(2,10,17,18,21,54,59,60)
wind <- c(9,19,20,24,25,30,40,42,45,46,47,51,52,56)
birds <- c(3,11,14,15,28,33,37,39,43,57,58)
insects <- c(1,4,22,26,27,29)
cicada <- c(7,8,12,16,32,34,44,48)
planes <- c(49,23)
quiet <- c(5,6,13,31,35,36,38,41,50,53,55)
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
number_birds <- NULL
#for(j in 1:(length(end)-1)) {
for (i in 16:(length(clusterList)-(n+1))) {
  ##image_num <- j
  # find the moving count of unique clusters from 1 to 16, 2 to 17 etc
  series <- clusterList[(i-n):(i+n)]
  # find the number of the unique clusters and subtract the clusters 
  # occupying one minute only
  ##uniqClust <- unique(series)
  ##uniqClust_length <- length(table(series)) - length(which(table(series) < 0)) # this was set to 2
  ##uniqueClust <- c(uniqueClust, uniqClust_length)
  ##num_birds <- table(clusterList[(i-n):(i+n)] %in% birds)
  ##ifelse(length(num_birds)==2, (number_birds <- c(number_birds, num_birds[[2]])), (number_birds <- c(number_birds, 0)))
  # find the number of unique bird clusters and substract the clusters
  # occupying one minute only
  # 1. birds
  ##a <- which(uniqClust %in% birds)
  ##b <- which(series %in% birds)
  ##uniqClust_birds <- length(a) - length(which(table(series[b]) < 2))
  ##uniqueClust_birds <- c(uniqueClust_birds, uniqClust_birds)
  # 2. insects
  ##a <- which(uniqClust %in% insects)
  ##b <- which(series %in% insects)
  ##uniqClust_insects <- length(a) - length(which(table(series[b]) < 2))
  ##uniqueClust_insects <- c(uniqueClust_insects, uniqClust_insects)
  # 3. cicada
  ##a <- which(uniqClust %in% cicada)
  ##b <- which(series %in% cicada)
  ##uniqClust_cicada <- length(a) - length(which(table(series[b]) < 2))
  ##uniqueClust_cicada <- c(uniqueClust_cicada, uniqClust_cicada)
  # 4. rain
  ##a <- which(uniqClust %in% rain)
  ##b <- which(series %in% rain)
  ##uniqClust_rain <- length(a) - length(which(table(series[b]) < 2))
  ##uniqueClust_rain <- c(uniqueClust_rain, uniqClust_rain)
  # 5. quiet
  ##a <- which(uniqClust %in% quiet)
  ##b <- which(series %in% quiet)
  ##uniqClust_quiet <- length(a) - length(which(table(series[b]) < 2))
  ##uniqueClust_quiet <- c(uniqueClust_quiet, uniqClust_quiet)
  # 7. wind
  ##a <- which(uniqClust %in% wind)
  ##b <- which(series %in% wind)
  ##uniqClust_wind <- length(a) - length(which(table(series[b]) < 2))
  ##uniqueClust_wind <- c(uniqueClust_wind, uniqClust_wind)
  # counting transitions
  aseq <- sequence(rle(series)$lengths)
  # count the number of non-transitions
  #counter <- length(which(aseq==1)) - 1
  # count the number of transitions
  counter <- length(which(aseq == 1)) - 1 
  count <- c(count, counter)
}

for(i in 2:398) {
  if(i < 10) {
    png(paste("transitions\\transitions_image0", i, ".png", sep = ""),
        height = 600, width = 1200)
  }
  if(i >= 10) {
    png(paste("transitions\\transitions_image", i, ".png", sep = ""),
        height = 600, width = 1200)
  }
  plot(count[(1440*(i-1)+1-n):((1440*i)-n)], type = "l", xaxt = "n",
       main = paste(Gym_dates[i]), ylim = c(0,2*n))
  abline(v=c(0, 120, 240, 360, 480, 600, 720, 840, 960, 1080, 1200, 1320, 1440))
  at=c(0, 120, 240, 360, 480, 600, 720, 840, 960, 1080, 1200, 1320, 1440)
  labels <- c("0","2","4","6","8","10","12","14","16","18","20","22","24")
  axis(side = 1, at = at, labels = labels)
  dev.off()
}

rm(aseq, uniqClust, uniqClust_birds, uniqueClust_insects, uniqClust_cicada, uniqClust_rain, uniqueClust_wind,
   uniqClust_quiet)
# Collate data
data <- as.data.frame(matrix(0, ncol = 9, nrow = 564465))

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
start <- seq(1, 573121, 10080)
start[1] <- n + 1 
end <- c(seq(10080, 573120, 10080), (573120-n))

# determine the number of each class in 2n + 1 period
number_birds <- NULL
number_insects <- NULL
number_cicada <- NULL
number_rain <- NULL
number_wind <- NULL
number_quiet <- NULL
number_planes <- NULL
Sys.time()
for(j in 1:(length(end)-1)) {
  for (i in start[j]:end[j]) {  #(length(clusterList)-(n+1))) {
    num_birds <- table(clusterList[(i-n):(i+n)] %in% birds)
    if(length(num_birds)==2) {
      number_birds <- c(number_birds, num_birds[[2]])
    }
    if(length(num_birds)==1) {
      if(all(names(num_birds)=="TRUE")) {
        number_birds <- c(number_birds, num_birds[[1]])
      }
      if(all(names(num_birds)=="FALSE")) {
        number_birds <- c(number_birds, 0)
      }
    }
    
    num_insects <- table(clusterList[(i-n):(i+n)] %in% insects)
    if(length(num_insects)==2) {
      number_insects <- c(number_insects, num_insects[[2]])
    }
    if(length(num_insects)==1) {
      if(all(names(num_insects)=="TRUE")) {
        number_insects <- c(number_insects, num_insects[[1]])
      }
      if(all(names(num_insects)=="FALSE")) {
        number_insects <- c(number_insects, 0)
      }
    }
    
    num_cicada <- table(clusterList[(i-n):(i+n)] %in% cicada)
    if(length(num_cicada)==2) {
      number_cicada <- c(number_cicada, num_cicada[[2]])
    }
    if(length(num_cicada)==1) {
      if(all(names(num_cicada)=="TRUE")) {
        number_cicada <- c(number_cicada, num_cicada[[1]])
      }
      if(all(names(num_cicada)=="FALSE")) {
        number_cicada <- c(number_cicada, 0)
      }
    }
    
    num_rain <- table(clusterList[(i-n):(i+n)] %in% rain)
    if(length(num_rain)==2) {
      number_rain <- c(number_rain, num_rain[[2]])
    }
    if(length(num_rain)==1) {
      if(all(names(num_rain)=="TRUE")) {
        number_rain <- c(number_rain, num_rain[[1]])
      }
      if(all(names(num_rain)=="FALSE")) {
        no = number_rain <- c(number_rain, 0)
      }
    }
    
    num_wind <- table(clusterList[(i-n):(i+n)] %in% wind)
    if(length(num_wind)==2) {
      number_wind <- c(number_wind, num_wind[[2]])
    }
    if(length(num_wind)==1) {
      if(all(names(num_wind)=="TRUE")) {
        number_wind <- c(number_wind, num_wind[[1]])
      }
      if(all(names(num_wind)=="FALSE")) {
        number_wind <- c(number_wind, 0)
      }
    }
    
    num_quiet <- table(clusterList[(i-n):(i+n)] %in% quiet)
    if(length(num_quiet)==2) {
      number_quiet <- c(number_quiet, num_quiet[[2]])
    }
    if(length(num_quiet)==1) {
      if(all(names(num_quiet)=="TRUE")) {
        number_quiet <- c(number_quiet, num_quiet[[1]])
      }
      if(all(names(num_quiet)=="FALSE")) {
        number_quiet <- c(number_quiet, 0)
      }
    }
    
    num_plane <- table(clusterList[(i-n):(i+n)] %in% plane)
    if(length(num_plane)==2) {
      number_planes <- c(number_planes, num_plane[[2]]) 
    }
    if(length(num_plane)==1) {
      if(all(names(num_plane)=="TRUE")) {
        number_planes <- c(number_planes, num_plane[[1]])
      }
      if(all(names(num_plane)=="FALSE")) {
        number_planes <- c(number_planes, 0)
      }
    }
  }
  if(j==1) {
    print(Sys.time())  
  }
  print(j)
}
Sys.time()

plot(number_wind[1:10080], type = "l")
plot(number_rain[1:10080], type = "l")
plot(number_birds[1:10080], type = "l")
plot(number_planes[1:10080], type = "l")
plot(number_quiet[1:10080], type = "l")
plot(number_insects[1:10080], type = "l")

# Collate data
if(site=="GympieNP") {
  data <- as.data.frame(matrix(0, ncol = 7, nrow = 564465))  
}

if(site=="WoondumNP") {
  data <- as.data.frame(matrix(0, ncol = 7, nrow = 573120))  
}

list <- c("number_birds","number_insects","number_cicada",
          "number_wind", "number_planes","number_rain",
          "number_quiet")

for(i in 1:length(list)) {
  data[1:length(number_birds),i] <- get(list[i])
}

colnames(data) <- list

write.csv(data, paste("volatility\\",site,"_numbers_n_",n,".csv",sep = ""), 
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

total <- NULL
for(i in 1:10080) {
sum <- sum(c(Gympie_num$number_wind[i],
Gympie_num$number_rain[i],
Gympie_num$number_birds[i],
Gympie_num$number_planes[i],
Gympie_num$number_quiet[i],
Gympie_num$number_insects[i],
Gympie_num$number_cicada[i]))
total <- c(total, sum)
}
plot(total)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a list of dates (For table) ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
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

months <- c("06","07","08","09","10","11","12",
            "01","02","03","04","05","06","07")
min_1 <- c("00","01","02","03","04","05","06","07","08","09",
         as.character(10:29))
min_2 <- as.character(30:59)
year <- c(rep("2015",7), rep("2016",7))

# produce an empty matrix to contain the counts for each
# month per half-hour
hour_matrix <- as.data.frame(matrix(0, ncol = 48, nrow = 14))
for(i in 1:length(months)) {
  # first half hour
  b <- which(bird_100$month==months[i] & bird_100$year==year[i] & 
               bird_100$minute %in% min_1) 
  bird_100_month <- bird_100[b,]
  c <- table(bird_100_month$hour)
  for(j in 1:length(c)) {
    d <- as.integer(names(c[j]))
    hour_matrix[i,(d*2)] <- c[[j]]  
  }
  # second half hour
  b <- which(bird_100$month==months[i] & bird_100$year==year[i] & 
               bird_100$minute %in% min_2) 
  bird_100_month <- bird_100[b,]
  c <- table(bird_100_month$hour)
  for(j in 1:length(c)) {
    d <- as.integer(names(c[j]))
    hour_matrix[i, (d*2+1)] <- c[[j]]  
  }
}
hour_matrix <- as.data.frame(hour_matrix)


mgp = c(3, 0.4, 0)
ylim <- c(0, 800)
par(mfrow = c(4,1), mar=c(1.5,2,0,0), mgp = c(3, 0.4, 0))
barplot(as.integer(hour_matrix[1,]), ylim = ylim)
axis(side = 1, at = seq(0,24*2.41,2.41), labels = as.character(0:24))
barplot(as.integer(hour_matrix[2,]), ylim = ylim)
axis(side = 1, at = seq(0,24*2.41,2.41), labels = as.character(0:24))
barplot(as.integer(hour_matrix[3,]), ylim = ylim)
axis(side = 1, at = seq(0,24*2.41,2.41), labels = as.character(0:24))
barplot(as.integer(hour_matrix[4,]), ylim = ylim)
axis(side = 1, at = seq(0,24*2.41,2.41), labels = as.character(0:24))

barplot(as.integer(hour_matrix[5,]), ylim = ylim)
axis(side = 1, at = seq(0,24*2.41,2.41), labels = as.character(0:24))
barplot(as.integer(hour_matrix[6,]), ylim = ylim)
axis(side = 1, at = seq(0,24*2.41,2.41), labels = as.character(0:24))
barplot(as.integer(hour_matrix[7,]), ylim = ylim)
axis(side = 1, at = seq(0,24*2.41,2.41), labels = as.character(0:24))
barplot(as.integer(hour_matrix[8,]), ylim = ylim)
axis(side = 1, at = seq(0,24*2.41,2.41), labels = as.character(0:24))


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

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# read count data ----------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())
# choose n and site
n <- 15
site <- "GympieNP"
if(site=="GympieNP") {
  Gympie_num <- read.csv(paste("volatility\\GympieNP_numbers_n_",n,".csv",sep = ""), 
                          header = T)  
}
if(site=="WoondumNP") {
  Woondum_num <- read.csv(paste("volatility\\WoondumNP_numbers_n_",n,".csv",sep = ""), header = T)  
}
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
png("volatility\\image.png", width = 1200, height = 1200)
par(mfrow=c(7,1), mar=c(2,2,0.1,1))
plot(Gympie_num$number_wind[10081:20080], type = "l")
mtext("Wind", line = -1.2, side = 3)
plot(Gympie_num$number_rain[10081:20080], type = "l")
mtext("Rain", line = -1.2, side = 3)
plot(Gympie_num$number_birds[10081:20080], type = "l")
mtext("Birds", line = -1.2, side = 3)
plot(Gympie_num$number_planes[10081:20080], type = "l")
mtext("Planes", line = -1.2, side = 3)
plot(Gympie_num$number_quiet[10081:20080], type = "l")
mtext("Quiet", line = -1.2, side = 3)
plot(Gympie_num$number_insects[10081:20080], type = "l")
mtext("Insects", line = -1.2, side = 3)
plot(Gympie_num$number_cicada[10081:20080], type = "l")
mtext("Cicada", line = -1.2, side = 3)
dev.off()

total <- NULL
for(i in 1:10080) {
  sum <- sum(c(Gympie_num$number_wind[i],
               Gympie_num$number_rain[i],
               Gympie_num$number_birds[i],
               Gympie_num$number_planes[i],
               Gympie_num$number_quiet[i],
               Gympie_num$number_insects[i],
               Gympie_num$number_cicada[i]))
  total <- c(total, sum)
}

sum <- sum(c(Gympie_num$number_wind[10081:20080],Gympie_num$number_rain[10081:20080]))

plot(Gympie_num$number_insects[1:10080], type = "l")
mtext("Insects", line = -1.2, side = 3)



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
