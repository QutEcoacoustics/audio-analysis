# Generate links to select random minutes from Ecosounds website
# for Jason Wimmer
# 24 December 2015
#
# Set cluster number
n <- 20

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
for (i in 1:length(list_Gympie)) {
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
#Cluster 9 selection only from September 4-6 am.
#Cluster 22 selection only from September 12-2 am.
#Cluster 4 selection only from September 4-6 pm.

# cluster 9 Gympie 530 to 630am
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\Ecosounds")
cluster9 <- read.csv("cluster9_dataset_Gympie.csv",header = T)
cluster9 <- cluster9[cluster9[,12]>0,,drop=FALSE] 
cluster9$cluster <- rep(9, length(cluster9$site))

clusters_5am <- cluster9[cluster9[,7]==5 & cluster9[,8]>=30,, drop=FALSE]
clusters_6am <- cluster9[cluster9[,7]==6 & cluster9[,8]<=29,, drop=FALSE]
clusters_530am_630am <- rbind(clusters_5am,clusters_6am)

Sept_530am_630am_C9_Gym <- subset(clusters_530am_630am, substr(clusters_530am_630am$date_times,1,6)=="201509")

Sept_530am_630am_C9_Gym <- subset(Sept_530am_630am_C9_Gym, 
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="01"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="02"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="03"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="04"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="05"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="06"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="07"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="08"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="09"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="10"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="11"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="12"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="13"|
                                  substr(Sept_530am_630am_C9_Gym[,6],7,8)=="14")
length_9_Sept_Gym <- length(Sept_530am_630am_C9_Gym$file.ids)
length_9_Sept_Gym

# cluster 9 Woondum 530 to 630am
cluster9 <- read.csv("cluster9_dataset_Woondum.csv",header = T)
cluster9 <- cluster9[cluster9[,12]>0,,drop=FALSE] 
cluster9$cluster <- rep(9, length(cluster9$site))

clusters_5am <- cluster9[cluster9[,7]==5 & cluster9[,8]>=30,, drop=FALSE]
clusters_6am <- cluster9[cluster9[,7]==6 & cluster9[,8]<=29,, drop=FALSE]
clusters_530am_630am <- rbind(clusters_5am,clusters_6am)

Sept_530am_630am_C9_Woon <- subset(clusters_530am_630am, substr(clusters_530am_630am$date_times,1,6)=="201509")

Sept_530am_630am_C9_Woon <- subset(Sept_530am_630am_C9_Woon, 
                                  substr(Sept_530am_630am_C9_Woon[,6],7,8)=="01"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="02"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="03"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="04"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="05"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="06"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="07"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="08"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="09"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="10"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="11"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="12"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="13"|
                                    substr(Sept_530am_630am_C9_Woon[,6],7,8)=="14")
length_9_Sept_Woon <- length(Sept_530am_630am_C9_Woon$file.ids)
length_9_Sept_Woon
#  276

# cluster 22 Gympie 12 noon to 2pm
cluster22 <- read.csv("cluster22_dataset_Gympie.csv",header = T)
cluster22 <- cluster22[cluster22[,12]>0,,drop=FALSE] 
cluster22$cluster <- rep(22, length(cluster22$site))

clusters_1230 <- cluster22[cluster22[,7]==12 & cluster22[,8]>=30,, drop=FALSE]
clusters_130 <- cluster22[cluster22[,7]==13 & cluster22[,8]<=29,, drop=FALSE]
clusters_1230_130 <- rbind(clusters_1230,clusters_130)

Sept_1230_130_C22_Gym <- subset(clusters_1230_130, substr(clusters_1230_130$date_times,1,6)=="201509")
Sept_1230_130_C22_Gym <- subset(Sept_1230_130_C22_Gym, 
                                  substr(Sept_1230_130_C22_Gym[,6],7,8)=="01"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="02"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="03"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="04"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="05"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="06"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="07"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="08"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="09"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="10"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="11"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="12"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="13"|
                                    substr(Sept_1230_130_C22_Gym[,6],7,8)=="14")

length_22_Sept_Gym <- length(Sept_1230_130_C22_Gym$file.ids)
length_22_Sept_Gym
#  173

# cluster 22 Woondum 12noon to 2pm
cluster22 <- read.csv("cluster22_dataset_Woondum.csv",header = T)
cluster22 <- cluster22[cluster22[,12]>0,,drop=FALSE] 
cluster22$cluster <- rep(22, length(cluster22$site))

clusters_1230 <- cluster22[cluster22[,7]==12 & cluster22[,8]>=30,, drop=FALSE]
clusters_130pm <- cluster22[cluster22[,7]==13 & cluster22[,8]<=29,, drop=FALSE]
clusters_1230_130 <- rbind(clusters_1230,clusters_130pm)

Sept_1230_130_C22_Woon <- subset(clusters_1230_130, substr(clusters_1230_130$date_times,1,6)=="201509")
Sept_1230_130_C22_Woon <- subset(Sept_1230_130_C22_Woon, 
                                substr(Sept_1230_130_C22_Woon[,6],7,8)=="01"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="02"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="03"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="04"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="05"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="06"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="07"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="08"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="09"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="10"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="11"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="12"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="13"|
                                  substr(Sept_1230_130_C22_Woon[,6],7,8)=="14")

length_22_Sept_Woon <- length(Sept_1230_130_C22_Woon$file.ids)
length_22_Sept_Woon
#  125

# cluster 4 Gympie 4pm to 6pm
cluster4 <- read.csv("cluster4_dataset_Gympie.csv",header = T)
cluster4 <- cluster4[cluster4[,12]>0,,drop=FALSE] 
cluster4$cluster <- rep(4, length(cluster4$site))

clusters_1630 <- cluster4[cluster4[,7]==16 & cluster4[,8]>=30,, drop=FALSE]
clusters_1730 <- cluster4[cluster4[,7]==17 & cluster4[,8]<=29,, drop=FALSE]
clusters_1630_1730 <- rbind(clusters_1630,clusters_1730)
Sept_1630_1730_C4_Gym <- subset(clusters_1630_1730, substr(clusters_1630_1730$date_times,1,6)=="201509")
Sept_1630_1730_C4_Gym <- subset(Sept_1630_1730_C4_Gym, 
                                 substr(Sept_1630_1730_C4_Gym[,6],7,8)=="01"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="02"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="03"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="04"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="05"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="06"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="07"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="08"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="09"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="10"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="11"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="12"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="13"|
                                   substr(Sept_1630_1730_C4_Gym[,6],7,8)=="14")

length_4_Sept_Gym <- length(Sept_1630_1730_C4_Gym$file.ids)
length_4_Sept_Gym
#  45

# cluster 4 Woondum 4pm to 6pm
cluster4 <- read.csv("cluster4_dataset_Woondum.csv",header = T)
cluster4 <- cluster4[cluster4[,12]>0,,drop=FALSE] 
cluster4$cluster <- rep(4, length(cluster4$site))

clusters_1630 <- cluster4[cluster4[,7]==16 & cluster4[,8]>=30,, drop=FALSE]
clusters_1730 <- cluster4[cluster4[,7]==17 & cluster4[,8]<=29,, drop=FALSE]
clusters_1630_1730 <- rbind(clusters_1630,clusters_1730)
Sept_1630_1730_C4_Woon <- subset(clusters_1630_1730, substr(clusters_1630_1730$date_times,1,6)=="201509")
Sept_1630_1730_C4_Woon <- subset(Sept_1630_1730_C4_Woon, 
                                substr(Sept_1630_1730_C4_Woon[,6],7,8)=="01"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="02"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="03"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="04"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="05"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="06"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="07"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="08"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="09"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="10"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="11"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="12"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="13"|
                                  substr(Sept_1630_1730_C4_Woon[,6],7,8)=="14")

length_4_Sept_Woon <- length(Sept_1630_1730_C4_Woon$file.ids)
length_4_Sept_Woon
#  79

# Select 20 random sample (minutes) from each of these clusters 4, 9 and 22 and each site
# Generate samples
sample9Gym <- Sept_530am_630am_C9_Gym[sample(nrow(Sept_530am_630am_C9_Gym),20),]
sample9Woon <- Sept_530am_630am_C9_Woon[sample(nrow(Sept_530am_630am_C9_Woon),20),]
sample22Gym <- Sept_1230_130_C22_Gym[sample(nrow(Sept_1230_130_C22_Gym),20),]
sample22Woon <- Sept_1230_130_C22_Woon[sample(nrow(Sept_1230_130_C22_Woon),20),]
sample4Gym <- Sept_1630_1730_C4_Gym[sample(nrow(Sept_1630_1730_C4_Gym),20),]
sample4Woon <- Sept_1630_1730_C4_Woon[sample(nrow(Sept_1630_1730_C4_Woon),20),]
#sample26Gym <- Sept_12_1pm_C26_Gym[sample(nrow(Sept_12_1pm_C26_Gym),20),]
#sample26Woon <- Sept_12_1pm_C26_Woon[sample(nrow(Sept_12_1pm_C26_Woon),20),]
#sample23Gym <- Sept_12_1pm_C23_Gym[sample(nrow(Sept_12_1pm_C23_Gym),20),]
#sample23Woon <- Sept_12_1pm_C23_Woon[sample(nrow(Sept_12_1pm_C23_Woon),20),]

names(sample9Gym)[names(sample9Gym) == 'list_Gympie'] <- 'minute'
names(sample9Woon)[names(sample9Woon) == 'list_Woondum'] <- 'minute'
names(sample22Gym)[names(sample22Gym) == 'list_Gympie'] <- 'minute'
names(sample22Woon)[names(sample22Woon) == 'list_Woondum'] <- 'minute'
names(sample4Gym)[names(sample4Gym) == 'list_Gympie'] <- 'minute'
names(sample4Woon)[names(sample4Woon) == 'list_Woondum'] <- 'minute'
#names(sample26Gym)[names(sample26Gym) == 'list_Gympie'] <- 'minute'
#names(sample26Woon)[names(sample26Woon) == 'list_Woondum'] <- 'minute'
#names(sample23Gym)[names(sample23Gym) == 'list_Gympie'] <- 'minute'
#names(sample23Woon)[names(sample23Woon) == 'list_Woondum'] <- 'minute'

concat.samples <- rbind(sample9Gym, sample9Woon, 
                        sample22Gym, sample22Woon,
                        sample4Gym, sample4Woon)
# Generate links
links <- NULL
for (i in 1:length(concat.samples$minute)) {
  lk <- paste("https://www.ecosounds.org/listen/",
              concat.samples$file.ids[i],"?start=",
              concat.samples$seconds.into.rec[i],sep="")
  links <- c(links, lk)
}

concat.samples$links <- links

write.csv(concat.samples, "new2_links.csv",row.names = F)

fileConn<-file("new2_links.txt")
writeLines(paste(links), fileConn)
close(fileConn)

# cluster 26 Gympie 4 to 6am
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\Ecosounds")
cluster26 <- read.csv("cluster26_dataset_Gympie.csv",header = T)
cluster26 <- cluster26[cluster26[,12]>0,,drop=FALSE] 
cluster26$cluster <- rep(26, length(cluster26$site))

clusters_4am <- cluster26[cluster26[,7]==4,, drop=FALSE]
clusters_5am <- cluster26[cluster26[,7]==5,, drop=FALSE]
clusters_4am_5am <- rbind(clusters_4am,clusters_5am)

Sept_4am_5am_C26_Gym <- subset(clusters_4am_5am, substr(clusters_4am_5am$date_times,1,6)=="201509")
length_26_Sept_Gym <- length(Sept_4am_5am_C26_Gym$file.ids)
length_26_Sept_Gym
#  644

# cluster 26 Woondum 4 to 6am
cluster26 <- read.csv("cluster26_dataset_Woondum.csv",header = T)
cluster26 <- cluster26[cluster26[,12]>0,,drop=FALSE] 
cluster26$cluster <- rep(26, length(cluster26$site))

clusters_4am <- cluster26[cluster26[,7]==4,, drop=FALSE]
clusters_5am <- cluster26[cluster26[,7]==5,, drop=FALSE]
clusters_4am_5am <- rbind(clusters_4am,clusters_5am)

Sept_4am_5am_C26_Woon <- subset(clusters_4am_5am, substr(clusters_4am_5am$date_times,1,6)=="201509")
length_26_Sept_Woon <- length(Sept_4am_5am_C26_Woon$file.ids)
length_26_Sept_Woon
#  462


# cluster 23 Woondum 4 to 6am
cluster23 <- read.csv("cluster23_dataset_Woondum.csv",header = T)
cluster23 <- cluster23[cluster23[,12]>0,,drop=FALSE] 
cluster23$cluster <- rep(23, length(cluster23$site))

clusters_4am <- cluster23[cluster23[,7]==4,, drop=FALSE]
clusters_5am <- cluster23[cluster23[,7]==5,, drop=FALSE]
clusters_4am_5am <- rbind(clusters_4am,clusters_5am)

Sept_4am_5am_C23_Woon <- subset(clusters_4am_5am, substr(clusters_4am_5am$date_times,1,6)=="201509")
length_23_Sept_Woon <- length(Sept_4am_5am_C23_Woon$file.ids)
length_23_Sept_Woon
#  462

