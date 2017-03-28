# Title:  Plotting the Hybrid Cluster Results
# Author: Yvonne Phillips
# Date:  24 December 2015
# Date modified:  27 September 2016

# Description:  This code creates a csv file containing links to 
# the Ecosounds webpage for access to particular minutes within 
# a particular cluster, it could be adapted to include all minutes 
# on a certain day and time.

# remove all objects in global environment
rm(list = ls())

##############################################
# Read Ecosounds mapping file 
##############################################
# Load the mapping file containing id, site.ids, duration and original file name
map_all <- read.csv("data/mapping_ecosounds/all_recordings_from_sites_1192_and_1193.csv",
                    header = T)[c(1,5,6,17,21)]
# remove any rows containing a deleter_id ie. files no longer referenced on Ecosounds
a <- which(!is.na(map_all$deleter_id))
map_all <- map_all[-a,]
map_all <- map_all[,-4]

gym <- which(map_all$site_id=="1192")
map_gym <- map_all[gym,]

woon <- which(map_all$site_id=="1193")
map_woon <- map_all[woon,]

map_gym <- map_gym[order(map_gym$original_file_name),]
map_woon <- map_woon[order(map_woon$original_file_name),]

##############################################
# Read cluster list
##############################################
k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list corresponding to k1 and k2 value
cluster_list <- get(file_name_short, envir=globalenv())[,column]

# remove unneeded objects from global environment
rm(hclust_clusters_25000, file_name, file_name_short, column)

############################################################
# Reconstitute the cluster list by adding in missing minutes
############################################################
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

############################################################
# generate a list of dates
############################################################
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

############################################################
# FUNCTION - generate a csv of cluster links for each cluster
############################################################
Ecosounds_links <- function(clust_num, site, map) {
  file_name_short <- as.character(paste("cluster_list_", site, sep = ""))
  cluster_list <- get(file_name_short, envir=globalenv())
  list <- which(cluster_list==clust_num)
  a <- NULL
  date_times <- NULL
  hour <- NULL
  minute <- NULL
  seconds <- NULL
  file.ids <- NULL
  site.ids <- NULL
  length_list <- length(list)
  site_des <- rep(site, length_list)
  duration <- NULL 
  file.rf <- NULL

  for (i in 1:length_list) {
    # Find date
    day.ref <- floor(list[i]/1440) + 1
    ste <- paste(site, "NP", sep = "")  
    date1 <- dates[day.ref]
    hour1 <- floor((list[i]/1440 - (day.ref-1))*24)
    hour1 <- as.integer(hour1)
    #}
    minute1 <- ((((list[i]/1440) - (day.ref-1))*24)-as.integer(hour1))*60
    # correct a slight rounding problem to adjust hours
    # and minutes
    if(minute1==60) {
      hour1 <- as.integer(hour1) + 1
      minute1 <- 0
    }
    # subtract one from the minutes to get the beginning
    # of the minute that matches the cluster list
    minute1 <- round(minute1,0) - 1
    # correct hours and minutes caused by going back one minute 
    if(minute1==-1 & hour1 > 0) {
      hour1 <- hour1 - 1
      minute1 <- "59" 
    }
    if(minute1==-1 & hour1==0) {
      hour1 <- 23
      minute1 <- 59
      date1 <- dates[day.ref-1]
    }
    if (hour1 < 10) {
      hour1 <- paste("0",as.integer(hour1),sep = "")
    }
    if(minute1 < 10) {
      minute1 <- paste("0",round(minute1,0),sep = "")
    }
    
    date_time <- paste(substr(date1,1,4),substr(date1,6,7),
                       substr(date1,9,10),"_",
                       hour1, minute1,"00",
                       sep = "")
    if (hour1!="00"|minute1!="00") {
      file.ref <- which((substr(map$original_file_name,1,8)
                         ==substr(date_time,1,8)) & 
                          (substr(map$original_file_name,10,15) <=
                             substr(date_time,10,15)))
    }
    if (hour1=="00" & minute1=="00") {
      file.rf1 <- which(((substr(map$original_file_name,1,8))
                         ==substr(date_time,1,8)) &
                          as.numeric(substr(map$original_file_name,10,15)) <=
                          as.numeric("000000"))
      date1 <- dates[day.ref-1]
      date_time <- paste(substr(date1,1,4),substr(date1,6,7),
                         substr(date1,9,10),"_",
                         hour1, minute1,"00",
                         sep = "")
      file.rf2 <- which((substr(map$original_file_name,1,8))
                        ==substr(date_time,1,8))
      file.rf <- c(file.rf, file.rf1, file.rf2)
      file.ref <- file.rf
      date1 <- dates[day.ref]
      date_time <- paste(substr(date1,1,4),substr(date1,6,7),
                         substr(date1,9,10),"_",
                         hour1, minute1,"00",
                         sep = "")
    }
    if(length(file.ref) > 1) {
      file.ref <- max(file.ref)
    }
    file.id <- map$id[file.ref]
    site.id <- map$site_id[file.ref]
    dur <- map$duration_seconds[file.ref]
    a <- c(a, file.ref)
    file.ids <- c(file.ids, file.id)
    site.ids <- c(site.ids, site.id)
    date_times <- c(date_times, date_time)
    hour <- c(hour, hour1)
    minute <- c(minute, minute1)
    duration <- c(duration, dur)
  }
  
  list <- NULL
  for (i in a) {
    lst <- as.character(map$original_file_name[i])
    list <- c(list, lst)
  }
  
  file.ref <- a
  orig.files <- list
  
  # determine the number of seconds since the start of the recording
  sec <- NULL
  for (i in 1:length(list)) {
    rec.start.hour <- substr(map$original_file_name[file.ref[i]],10,11)
    rec.start.min <- substr(map$original_file_name[file.ref[i]],12,13)
    rec.start.sec <- substr(map$original_file_name[file.ref[i]],14,15)
    total.rec.start.sec <- (as.integer(rec.start.hour)*3600 +
                              as.integer(rec.start.min)*60 + 
                              as.integer(rec.start.sec)) 
    actual.sec.since.midnight <- as.integer(hour[i])*3600 + as.integer(minute[i])*60
    diff.sec <- actual.sec.since.midnight-total.rec.start.sec
    sec <- c(sec, diff.sec)
  }
  
  seconds.into.rec <- sec
  sec.remainder <- duration - seconds.into.rec 
  
  links <- NULL
  for (i in 1:length(list)) {
    lk <- paste("https://www.ecosounds.org/listen/",
                file.ids[i],"?start=",
                seconds.into.rec[i],sep="")
    links <- c(links, lk)
  }
  
  hyperlinks <- NULL
  for (i in 1:length(links)) {
    hyl <- paste("= hyperlink(M",(i+1),")",sep="")
    hyperlinks <- c(hyperlinks, hyl)
  }
  
  cluster_num <- rep(clust_num, length(list))
  dataset<- NULL
  dataset <- cbind(list, file.ref, file.ids, site.ids, site_des, 
                   date_times, hour, minute, orig.files, 
                   seconds.into.rec, duration, sec.remainder, 
                   links, cluster_num)
  # remove any minutes where seconds remaining is less than 60
  too_short <- which(as.numeric(dataset[,12]) <= 60)
  dataset <- dataset[-c(too_short),]
  if(clust_num < 10) {
    write.csv(dataset, file = 
                paste("data/Ecosounds_links/", site, 
                      "NP/Ecosounds_Links_Cluster", 
                      "0",clust_num, "_",k1_value, "_", 
                      k2_value,"_",site,".csv",sep=""), 
              row.names = F)
  }
  if(clust_num>=10) {
    write.csv(dataset, file = 
                paste("data/Ecosounds_links/", site, 
                      "NP/Ecosounds_Links_Cluster", 
                      clust_num, "_",k1_value, "_", 
                      k2_value,"_",site,".csv",sep=""), 
              row.names = F)
  }
}

###################################################
# Call the Ecosounds_links function
###################################################
# Call the Ecosounds_links Function for the Gympie data
# to save a csv file for each cluster containing all minutes
for(i in 1:k2_value) {
  Ecosounds_links(i, "Gympie", map_gym) # eg. (i, "Gympie", map_gym)
}

# Call the Ecosounds_links Function for the Woondum data
# to save a csv file for each cluster containing all minutes
for(i in 1:k2_value) {
  Ecosounds_links(i, "Woondum", map_woon) # eg. (i, "Woondum", map_woon)
}

#######################################################
# FUNCTION Random selection of 50 minutes from each cluster from each site
#######################################################
Random_Selection <- function(clust_num, site, sample_num) {
  if(clust_num < 10) {
    links <- read.csv(paste("data/Ecosounds_links/", site, 
                            "NP/Ecosounds_links_Cluster0",
                            clust_num, "_", k1_value, "_", 
                            k2_value,"_", site, ".csv", sep = ""), 
                      header = T)
  }
  if(clust_num >= 10) {
    links <- read.csv(paste("data/Ecosounds_links/", site, 
                             "NP/Ecosounds_links_Cluster",
                            clust_num, "_", k1_value, "_", 
                            k2_value, "_", site, ".csv", sep = ""), 
                      header = T)
  }
  link_sample <<- links[sample(nrow(links), sample_num), ]
}

###################################################
# Call the Random_Selection function
###################################################
link_samples <- NULL
k1_value <- 25000
k2_value <- 60
for(i in 1:60) {
  sample_num <- 50
  print(paste("starting", i, sep = " "))
  Random_Selection(i, "Gympie", 50)
  link_samples <- rbind(link_samples, link_sample)
  print(paste("starting", i, sep = " "))
  Random_Selection(i, "Woondum", 50)
  link_samples <- rbind(link_samples, link_sample)
}

write.csv(link_samples, "data/Ecosounds_links/Minute_samples_from_each_cluster_&_site.csv", row.names = F)


#############################################
# DO NOT DELETE THE INFORMATION BELOW
#############################################
gympie_dates <- c("2015-07-15","2015-07-16","2015-08-17","2015-08-18",
                  "2015-09-22","2015-09-23","2015-10-05","2015-10-06",
                  "2015-11-12","2015-11-13","2015-12-14","2015-12-15",
                  "2016-01-11","2016-01-12","2016-02-25","2016-02-26",
                  "2016-03-25","2016-03-26","2016-04-21","2016-04-22",
                  "2016-05-18","2016-05-19","2016-06-08","2016-06-10")
woondum_dates <- c("2015-07-30","2015-07-31","2015-08-01","2015-08-04",
                   "2015-09-09","2015-09-22","2015-10-04","2015-10-05",
                   "2015-11-18","2015-11-19","2015-12-09","2015-12-10",
                   "2016-01-11","2016-01-12","2016-02-25","2016-02-26",
                   "2016-03-10","2016-03-15","2016-04-06","2016-04-09",
                   "2016-05-12","2016-05-13","2016-06-08","2016-06-10")