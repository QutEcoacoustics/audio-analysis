# remove all objects in the global environment
rm(list = ls())

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
rm(full_length, list, list1, minute_reference, removed_minutes)

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

dates <- rep(date.list, each = 1440)
dates <- rep(dates, 2)
# Add site and dates columns to dataframe
sites <- c("Gympie NP", "Woondum NP")
sites <- rep(sites, each=length(dates)/2)
cluster_list <- cbind(cluster_list, sites, dates)
cluster_list <- data.frame(cluster_list)
sites <- unique(sites)

# This is the final listing
rain_all <- c(2,10,17,18,21,54,59,60)
wind_all <- c(9,19,20,24,25,30,40,42,45,46,47,51,52,56)
birds_all <- c(3,11,14,15,28,33,37,39,43,57,58)
insects_all <- c(1,4,22,26,27,29)
cicada_all <- c(7,8,12,16,32,34,44,48)
planes_all <- c(49,23)
quiet_all <- c(5,6,13,31,35,36,38,41,50,53,55)
na <- 61

birds <- c(3, 11, 14, 15, 33, 37, 43, 58)
insects <- c(1, 27, 29)
cicada <- c(12, 16, 32, 34, 44, 48)
rain <- c(21, 10, 18,  59)
wind <- c(30, 9, 19, 20, 25, 46, 56, 24, 40, 42, 51, 52, 47)
planes <- planes_all
quiet <- c(5, 6, 13, 31, 35, 38, 41, 53, 55)
all_clusters <- c(birds, insects, cicada)
# find the position of each of the first of each month
a <- which(substr(date.list, 9, 10)=="01")

site <- sites[1]
count_30 <- NULL

if (site==sites[1]) {
  cluster_list_site <- cluster_list[which(cluster_list$sites==site[1]),]
  a <- which(is.na(cluster_list_site$cluster_list))
  cluster_list_site$cluster_list <- as.character(cluster_list_site$cluster_list)
  cluster_list_site$cluster_list[a] <- as.numeric("61")
  # Re-factorize with the as.factor function or simple factor(fixed$Type)
  cluster_list_site$cluster_list <- as.factor(cluster_list_site$cluster_list)
}
if (site==sites[2]) {
  cluster_list_site <- cluster_list[which(cluster_list$sites==site[2]),]
  a <- which(is.na(cluster_list_site$cluster_list))
  cluster_list_site$cluster_list <- as.character(cluster_list_site$cluster_list)
  cluster_list_site$cluster_list[a] <- as.numeric("61")
  # Re-factorize with the as.factor function or simple factor(fixed$Type)
  cluster_list_site$cluster_list <- as.factor(cluster_list_site$cluster_list)
} 
seq_30min <- seq(1, nrow(cluster_list_site), 30)
count_30 <- NULL
for(i in 1:(length(seq_30min)-48)) { # the whole last day is removed
  number_in_all_clusters <- sum(cluster_list_site$cluster_list[seq_30min[i]:(seq_30min[i+1]-1)] %in% all_clusters, na.rm = TRUE)
  count_30 <- c(count_30, number_in_all_clusters)
}

# put count_30 into a matrix where one row is one day
count_30_matrix <- matrix(count_30, nrow = 48, 
                          ncol = (days/2-1), byrow = F)
count_30_matrix <- as.data.frame(count_30_matrix)

par(mar=c(3,3,1,1))
plot(count_30_matrix$V1, type = "l")
plot(count_30_matrix$V2, type = "l")
count_30a <- as.numeric(count_30) 
library(gplots)
periods <- rep(as.factor(1:48), (days/2-1))
data_count <- cbind(count_30a, periods)
data_count <- data.frame(data_count)
plotmeans(data_count$count_30a ~ data_count$periods)

data_count <- cbind(count_30a, periods)
data_count <- data.frame(data_count)
plotmeans(data_count$count_30a[1:240] ~ data_count$periods[1:240])
plotmeans(data_count$count_30a ~ data_count$periods)

plotmeans(data_count$count_30a[1:480] ~ data_count$periods[1:480])
plotmeans(data_count$count_30a[481:960] ~ data_count$periods[481:960])
plotmeans(data_count$count_30a[961:1440] ~ data_count$periods[961:1440])
plotmeans(data_count$count_30a[1441:1920] ~ data_count$periods[1441:1920])
seq_960 <- seq(1, nrow(data_count), 960)
date_ref <- seq(1, nrow(data_count), 20)
ylim <- c(0, 30)
for(i in 1:(length(seq_960)-1)) { #length(nrow(data_count/960)))
  if(i < 10) {
    png(paste("confidence\\",site," 0", i, ".png", sep = ""), height = 600, width = 1500)
  }
  if(i >= 10) {
    png(paste("confidence\\",site," ", i, ".png", sep = ""), height = 600, width = 1500)
  }
  plotmeans(data_count$count_30a[seq_960[i]:(seq_960[i+1]-1)] ~ 
              data_count$periods[seq_960[i]:(seq_960[i+1]-1)], ylim = ylim)
  mtext(paste(site, date.list[date_ref[i]], " to ", date.list[date_ref[(i+1)]-1]), cex = 2)
  dev.off()
}
