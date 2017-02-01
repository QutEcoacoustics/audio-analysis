# Description: Plotting Summary Indices PCA plot
# Author: Yvonne Phillips
# Date: 24 December 2015

# Set cluster number
n <- 24

setwd("C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Yvonne\\")
mapping1 <- read.csv("audio_recordings_from_site_1192_GympieNP.csv", header = T)[,c(1,5,6,21)]
mapping2 <- read.csv("audio_recordings_from_site_1193_Woondum3.csv", header = T)[,c(1,5,6,21)]
mapping1 <- mapping1[order(mapping1[,4]),]
mapping1$row <- c(1:length(mapping1$id))
mapping2 <- mapping2[order(mapping2[,4]),]
mapping2$row <- c(1:length(mapping2$id))

cluster.list <- read.csv("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\hybrid_clust_17500_30.csv",header = T)
cluster.list.Gympie <- cluster.list[1:(length(cluster.list$hybrid_k17500k30k3)/2),]
cluster.list.Woondum <- cluster.list[(length(cluster.list$hybrid_k17500k30k3)/2+1):length(cluster.list$hybrid_k17500k30k3),]

indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)
dates <- as.Date(indices$rec.date, format = "%d/%m/%Y")
dates <- unique(dates)
#dates <- rep(dates, 2)

list_Gympie <- which(cluster.list.Gympie==n)
list_Woondum <- which(cluster.list.Woondum==n)
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\Ecosounds")

# Generate Gympie file
a <- NULL
date_times <- NULL
hour <- NULL
minute <- NULL
seconds <- NULL
site <- NULL
file.ids <- NULL
site.ids <- NULL
duration <- NULL 
file.rf <- NULL

for (i in 1:length(list_Gympie)) {
  # Find date
  day.ref <- floor(list_Gympie[i]/1440) + 1
  if(day.ref < 112) {
    ste <- "GympieNP"
    date1 <- dates[day.ref]
    hour1 <- floor((list_Gympie[i]/1440 - (day.ref-1))*24)
    hour1 <- as.integer(hour1)
  }
  minute1 <- ((((list_Gympie[i]/1440) - (day.ref-1))*24)-as.integer(hour1))*60
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
  if (hour1<10) {
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
  file.ref <- which((substr(mapping1$original_file_name,1,8)
                     ==substr(date_time,1,8)) & 
                      (substr(mapping1$original_file_name,10,15) <=
                         substr(date_time,10,15)))
  }
  if (hour1=="00" & minute1=="00") {
    file.rf1 <- which(((substr(mapping1$original_file_name,1,8))
                       ==substr(date_time,1,8)) &
                        as.numeric(substr(mapping1$original_file_name,10,15)) <=
                        as.numeric("000000"))
    date1 <- dates[day.ref-1]
    date_time <- paste(substr(date1,1,4),substr(date1,6,7),
                       substr(date1,9,10),"_",
                       hour1, minute1,"00",
                       sep = "")
    file.rf2 <- which((substr(mapping1$original_file_name,1,8))
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
  file.id <- mapping1$id[file.ref]
  site.id <- mapping1$site_id[file.ref]
  dur <- mapping1$duration_seconds[file.ref]
  a <- c(a, file.ref)
  file.ids <- c(file.ids, file.id)
  site.ids <- c(site.ids, site.id)
  date_times <- c(date_times, date_time)
  hour <- c(hour, hour1)
  minute <- c(minute, minute1)
  site <- c(site, ste)
  duration <- c(duration, dur)
}

list <- NULL
for (i in a) {
  lst <- as.character(mapping1$original_file_name[i])
  list <- c(list,lst)
}

file.ref <- a
orig.files <- list

# determine the number of seconds since the start of the recording
sec <- NULL
for (i in 1:length(list_Gympie)) {
  rec.start.hour <- substr(mapping1$original_file_name[file.ref[i]],10,11)
  rec.start.min <- substr(mapping1$original_file_name[file.ref[i]],12,13)
  rec.start.sec <- substr(mapping1$original_file_name[file.ref[i]],14,15)
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
for (i in 1:length(list_Gympie)) {
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

dataset <- cbind(list_Gympie,file.ref,file.ids, site.ids,site,date_times,hour,
                 minute,orig.files, seconds.into.rec, duration,sec.remainder, links,
                 hyperlinks)

write.csv(dataset, row.names=F, file=paste("cluster", n, "_dataset_Gympie.csv", sep=""))
####################################

# Generate Woondum File
a <- NULL
date_times <- NULL
hour <- NULL
minute <- NULL
seconds <- NULL
site <- NULL
file.ids <- NULL
site.ids <- NULL
duration <- NULL 

for (i in 1:length(list_Woondum)) {
  # Find date
  day.ref <- floor(list_Woondum[i]/1440) + 1
  if(day.ref < 112) {
    ste <- "WoondumNP"
    date1 <- dates[day.ref]
    hour1 <- floor((list_Woondum[i]/1440 - (day.ref-1))*24)
    hour1 <- as.integer(hour1)
  }
  minute1 <- ((((list_Woondum[i]/1440) - (day.ref-1))*24)-as.integer(hour1))*60
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
  if (hour1<10) {
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
    file.ref <- which((substr(mapping2$original_file_name,1,8)
                       ==substr(date_time,1,8)) & 
                        (substr(mapping2$original_file_name,10,15) <=
                           substr(date_time,10,15)))
  }
  if (hour1=="00" & minute1=="00") {
    file.rf1 <- which(((substr(mapping2$original_file_name,1,8))
                       ==substr(date_time,1,8)) &
                        as.numeric(substr(mapping2$original_file_name,10,15)) <=
                        as.numeric("000000"))
    date1 <- dates[day.ref-1]
    date_time <- paste(substr(date1,1,4),substr(date1,6,7),
                       substr(date1,9,10),"_",
                       hour1, minute1,"00",
                       sep = "")
    file.rf2 <- which((substr(mapping2$original_file_name,1,8))
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
  file.id <- mapping2$id[file.ref]
  site.id <- mapping2$site_id[file.ref]
  dur <- mapping2$duration_seconds[file.ref]
  a <- c(a, file.ref)
  file.ids <- c(file.ids, file.id)
  site.ids <- c(site.ids, site.id)
  date_times <- c(date_times, date_time)
  hour <- c(hour, hour1)
  minute <- c(minute, minute1)
  site <- c(site, ste)
  duration <- c(duration, dur)
}

list <- NULL
for (i in a) {
  lst <- as.character(mapping2$original_file_name[i])
  list <- c(list,lst)
}

file.ref <- a
orig.files <- list

# determine the number of seconds since the start of the recording
sec <- NULL
for (i in 1:length(list_Woondum)) {
  rec.start.hour <- substr(mapping2$original_file_name[file.ref[i]],10,11)
  rec.start.min <- substr(mapping2$original_file_name[file.ref[i]],12,13)
  rec.start.sec <- substr(mapping2$original_file_name[file.ref[i]],14,15)
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
for (i in 1:length(list_Woondum)) {
  lk <- paste("https://www.ecosounds.org/listen/",
              file.ids[i],"?start=",
              seconds.into.rec[i],sep="")
  links <- c(links, lk)
}

hyperlinks <- NULL
for (i in 1:length(list_Woondum)) {
  hyl <- paste("= hyperlink(M",(i+1),")",sep="")
  hyperlinks <- c(hyperlinks, hyl)
}

dataset <- cbind(list_Woondum,file.ref,file.ids, site.ids,site,date_times,hour,
                 minute,orig.files, seconds.into.rec, duration,sec.remainder, links,
                 hyperlinks)

write.csv(dataset, row.names=F, file=paste("cluster", n, "_dataset_Woondum.csv", sep=""))

######################################################################

# generating monthly datasets for certain clusters
#Cluster 16 selection only from September 4-8 am. 5-6
#Cluster 29 selection only from September 4-8 am. 
#Cluster 26 selection only from September 10-2 am.
#Cluster 27 selection only from September 10-2 pm.

# cluster 16 Gympie 4am to 8am
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\Ecosounds")
cluster16 <- read.csv("cluster16_dataset_Gympie.csv",header = T)
cluster16 <- cluster16[cluster16[,12] > 0,,drop=FALSE] 
cluster16$cluster <- rep(16, length(cluster16$site))

clusters_5_7am <- cluster16[cluster16[,7]==5|
                                cluster16[,7]==6
                                ,, drop=FALSE]

Sept_5_7am_C16_Gym <- subset(clusters_5_7am, substr(clusters_5_7am$date_times,1,6)=="201509")

#Sept_4am_8am_C16_Gym <- subset(Sept_4am_8am_C16_Gym, 
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="01"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="02"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="03"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="04"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="05"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="06"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="07"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="08"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="09"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="10"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="11"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="12"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="13"|
#                                  substr(Sept_4am_8am_C16_Gym[,6],7,8)=="14")
length_16_Sept_Gym <- length(Sept_5_7am_C16_Gym$file.ids)
length_16_Sept_Gym
# 4am Sept clus 16 #18
# 5am Sept clus 16 #313
# 6am Sept clus 16 #532
# 7am Sept clus 16 #491
# 8am Sept clus 16 #432

#rm(clusters_5_7am, Sept_5_7am_C16_Gym, length_16_Sept_Gym)

# 1023

# cluster 16 Woondum 4am to 8am
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\Ecosounds")
cluster16 <- read.csv("cluster16_dataset_Woondum.csv",header = T)
cluster16 <- cluster16[cluster16[,12] > 0,,drop=FALSE] 
cluster16$cluster <- rep(16, length(cluster16$site))

clusters_5_7am <- cluster16[cluster16[,7]==5|
                            cluster16[,7]==6
                            ,, drop=FALSE]

Sept_5_7am_C16_Woon <- subset(clusters_5_7am, substr(clusters_5_7am$date_times,1,6)=="201509")

#Sept_4am_8am_C16_Woon <- subset(Sept_4am_8am_C16_Woon, 
#                               substr(Sept_4am_8am_C16_Woon[,6],7,8)=="01"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="02"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="03"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="04"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="05"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="06"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="07"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="08"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="09"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="10"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="11"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="12"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="13"|
#                                 substr(Sept_4am_8am_C16_Woon[,6],7,8)=="14")
length_16_Sept_Woon <- length(Sept_5_7am_C16_Woon$file.ids)
length_16_Sept_Woon
# 4am Sept clus 16 #39
# 5am Sept clus 16 #435  ***
# 6am Sept clus 16 #384  ***
# 7am Sept clus 16 #323
# 8am Sept clus 16 #338

# cluster 29 Gympie 12 to 2pm
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\Ecosounds")
cluster29 <- read.csv("cluster29_dataset_Gympie.csv",header = T)
cluster29 <- cluster29[cluster29[,12] > 0,,drop=FALSE] 
cluster29$cluster <- rep(29, length(cluster29$site))

clusters_5_7am <- cluster29[cluster29[,7]==5|
                                cluster29[,7]==6
                                ,, drop=FALSE]
Sept_5_7am_C29_Gym <- subset(clusters_5_7am, substr(clusters_5_7am$date_times,1,6)=="201509")

#Sept_4am_8am_C29_Gym <- subset(Sept_4am_8am_C29_Gym, 
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="01"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="02"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="03"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="04"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="05"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="06"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="07"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="08"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="09"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="10"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="11"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="12"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="13"|
#                                  substr(Sept_4am_8am_C29_Gym[,6],7,8)=="14")
length_29_Sept_Gym <- length(Sept_5_7am_C29_Gym$file.ids)
length_29_Sept_Gym

# 4am Sept clus 29 #0
# 5am Sept clus 29 #560
# 6am Sept clus 29 #205
# 7am Sept clus 29 #80
# 8am Sept clus 29 #155

# 845

# cluster 29 Woondum 4am to 8am
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\Ecosounds")
cluster29 <- read.csv("cluster29_dataset_Woondum.csv",header = T)
cluster29 <- cluster29[cluster29[,12] > 0,,drop=FALSE] 
cluster29$cluster <- rep(29, length(cluster29$site))

clusters_5_7am <- cluster29[cluster29[,7]==5|
                                cluster29[,7]==6
                                ,, drop=FALSE]

Sept_5_7am_C29_Woon <- subset(clusters_5_7am, substr(clusters_5_7am$date_times,1,6)=="201509")

#Sept_4am_8am_C29_Woon <- subset(Sept_4am_8am_C29_Woon, 
#                               substr(Sept_4am_8am_C29_Woon[,6],7,8)=="01"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="02"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="03"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="04"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="05"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="06"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="07"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="08"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="09"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="10"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="11"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="12"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="13"|
#                                 substr(Sept_4am_8am_C29_Woon[,6],7,8)=="14")
length_29_Sept_Woon <- length(Sept_5_7am_C29_Woon$file.ids)
length_29_Sept_Woon
# 4am Sept clus 29 #1
# 5am Sept clus 29 #285
# 6am Sept clus 29 #106
# 7am Sept clus 29 #105
# 8am Sept clus 29 #94

#497

# cluster 26 midday
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\Ecosounds")
cluster26 <- read.csv("cluster26_dataset_Gympie.csv",header = T)
cluster26 <- cluster26[cluster26[,12] > 0,,drop=FALSE] 
cluster26$cluster <- rep(26, length(cluster26$site))

clusters_11am_1pm <- cluster26[cluster26[,7]==11|
                                cluster26[,7]==12
                               ,, drop=FALSE]

Sept_11am_1pm_C26_Gym <- subset(clusters_11am_1pm, substr(clusters_11am_1pm$date_times,1,6)=="201509")

#Sept_10am_2pm_C26_Gym <- subset(Sept_10am_2pm_C26_Gym, 
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="01"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="02"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="03"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="04"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="05"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="06"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="07"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="08"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="09"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="10"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="11"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="12"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="13"|
#                                  substr(Sept_10am_2pm_C26_Gym[,6],7,8)=="14")
length_26_Sept_Gym <- length(Sept_11am_1pm_C26_Gym$file.ids)
length_26_Sept_Gym
# 914

# cluster 26 Woondum 10am to 2pm
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\Ecosounds")
cluster26 <- read.csv("cluster26_dataset_Woondum.csv",header = T)
cluster26 <- cluster26[cluster26[,12] > 0,,drop=FALSE] 
cluster26$cluster <- rep(26, length(cluster26$site))

clusters_11am_1pm <- cluster26[cluster26[,7]==11|
                               cluster26[,7]==12
                               ,, drop=FALSE]

Sept_11am_1pm_C26_Woon <- subset(clusters_11am_1pm, substr(clusters_11am_1pm$date_times,1,6)=="201509")

#Sept_10am_2pm_C26_Woon <- subset(Sept_10am_2pm_C26_Woon, 
#                               substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="01"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="02"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="03"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="04"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="05"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="06"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="07"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="08"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="09"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="10"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="11"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="12"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="13"|
#                                 substr(Sept_10am_2pm_C26_Woon[,6],7,8)=="14")
length_26_Sept_Woon <- length(Sept_11am_1pm_C26_Woon$file.ids)
length_26_Sept_Woon
# 602

# Select 20 random sample (minutes) from each of these clusters 16,29 and 26 
# at each site
# Generate samples
sample16Gym <- Sept_5_7am_C16_Gym[sample(nrow(Sept_5_7am_C16_Gym),20),]
sample16Woon <- Sept_5_7am_C16_Woon[sample(nrow(Sept_5_7am_C16_Woon),20),]
sample29Gym <- Sept_5_7am_C29_Gym[sample(nrow(Sept_5_7am_C29_Gym),20),]
sample29Woon <- Sept_5_7am_C29_Woon[sample(nrow(Sept_5_7am_C29_Woon),20),]
sample26Gym <- Sept_11am_1pm_C26_Gym[sample(nrow(Sept_11am_1pm_C26_Gym),20),]
sample26Woon <- Sept_11am_1pm_C26_Woon[sample(nrow(Sept_11am_1pm_C26_Woon),20),]

names(sample16Gym)[names(sample16Gym) == 'list_Gympie'] <- 'minute'
names(sample16Woon)[names(sample16Woon) == 'list_Woondum'] <- 'minute'
names(sample29Gym)[names(sample29Gym) == 'list_Gympie'] <- 'minute'
names(sample29Woon)[names(sample29Woon) == 'list_Woondum'] <- 'minute'
names(sample26Gym)[names(sample26Gym) == 'list_Gympie'] <- 'minute'
names(sample26Woon)[names(sample26Woon) == 'list_Woondum'] <- 'minute'
names(sample27Gym)[names(sample27Gym) == 'list_Gympie'] <- 'minute'
names(sample27Woon)[names(sample27Woon) == 'list_Woondum'] <- 'minute'

concat.samples <- rbind(sample16Gym, sample16Woon, 
                        sample29Gym, sample29Woon,
                        sample26Gym, sample26Woon)
# Generate links
links <- NULL
for (i in 1:length(concat.samples$minute)) {
  lk <- paste("https://www.ecosounds.org/listen/",
              concat.samples$file.ids[i],"?start=",
              concat.samples$seconds.into.rec[i],sep="")
  links <- c(links, lk)
}

concat.samples$links <- links

write.csv(concat.samples, "links_20_for.JessC.csv",row.names = F)

fileConn<-file("links_20_for.Jess.C.txt")
writeLines(paste(links), fileConn)
close(fileConn)
