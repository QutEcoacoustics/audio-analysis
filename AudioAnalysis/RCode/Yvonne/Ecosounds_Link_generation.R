# Generate links to select random minutes from Ecosounds website
# for Jason Wimmer
# 24 December 2015
#
# Set cluster number
n <- 9
  
setwd("C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Yvonne\\")
mapping1 <- read.csv("audio_recordings_from_site_1192_GympieNP.csv", header = T)[,c(1,5,6,21)]
mapping2 <- read.csv("audio_recordings_from_site_1193_Woondum3.csv", header = T)[,c(1,5,6,21)]
mapping1 <- mapping1[order(mapping1[,4]),]
mapping1$row <- c(1:length(mapping1$id))
mapping2 <- mapping2[order(mapping2[,4]),]
mapping2$row <- c(1:length(mapping2$id))

cluster.list <- read.csv("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\hybrid_clust_17500_30.csv",header = T)
cluster.list.Gympie <- cluster.list[1:(length(cluster.list$hybrid_k17500k30k3)/2),]
cluster.list.Woondum <- cluster.list[(length(cluster.list$hybrid_k17500k30k3)/2+1):length(cluster.list$hybrid_k17500k30k3),]

indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)
dates <- as.Date(indices$rec.date, format = "%d/%m/%Y")
dates <- unique(dates)
dates <- rep(dates, 2)

list_Gympie <- which(cluster.list.Gympie==n)
list_Woondum <- which(cluster.list.Woondum==n)

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

for (i in 1:length(list_Gympie)) {
  # Find date
  day.ref <- floor(list_Gympie[i]/1440) + 1
  if(day.ref < 112) {
    ste <- "GympieNP"
    date1 <- dates[day.ref]
    hour1 <- floor((list_Gympie[i]/1440 - (day.ref-1))*24)
    hour1 <- as.integer(hour1)
    if (hour1<10) {
      hour1 <- paste("0",as.integer(hour1),sep = "")
    }
  }
  minute1 <- ((((list_Gympie[i]/1440) - (day.ref-1))*24)-as.integer(hour1))*60
  minute1 <- round(minute1,0)
  if(minute1 < 10) {
    minute1 <- paste("0",round(minute1,0),sep = "")
  }
  if(minute1==60) {
    hour1 <- as.integer(hour1) + 1
    if (hour1<10) {
      hour1 <- paste("0",as.integer(hour1),sep = "")
    }
    minute1 <- "00"
  }
  date_time <- paste(substr(date1,1,4),substr(date1,6,7),
                     substr(date1,9,10),"_",
                     hour1, minute1,"00",
                     sep = "")
  file.ref <- which((substr(mapping1$original_file_name,1,8)
                     ==substr(date_time,1,8)) & 
                      (substr(mapping1$original_file_name,10,15) < 
                         substr(date_time,10,15)))
  if(length(file.ref)>1) {
    file.ref <- max(file.ref)
  }
  file.id <- mapping1$id[file.ref]
  site.id <- mapping1$site_id[file.ref]
  dur <- mapping1$duration_seconds[file.ref]
  a <- c(a,file.ref)
  file.ids <- c(file.ids, file.id)
  site.ids <- c(site.ids, site.id)
  date_times <- c(date_times, date_time)
  hour <- c(hour, hour1)
  minute <- c(minute, minute1)
  site <- c(site,ste)
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

dataset <- cbind(list_Gympie,file.ref,file.ids, site.ids,site,date_times,hour,
                 minute,orig.files, seconds.into.rec, duration,sec.remainder)

write.csv(dataset, row.names=F, file=paste("cluster", n, "_dataset_Gympie.csv", sep=""))

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
    if (hour1<10) {
      hour1 <- paste("0",as.integer(hour1),sep = "")
    }
  }
  minute1 <- ((((list_Woondum[i]/1440) - (day.ref-1))*24)-as.integer(hour1))*60
  minute1 <- round(minute1,0)
  if(minute1 < 10) {
    minute1 <- paste("0",round(minute1,0),sep = "")
  }
  if(minute1==60) {
    hour1 <- as.integer(hour1) + 1
    if (hour1<10) {
      hour1 <- paste("0",as.integer(hour1),sep = "")
    }
    minute1 <- "00"
  }
  date_time <- paste(substr(date1,1,4),substr(date1,6,7),
                     substr(date1,9,10),"_",
                     hour1, minute1,"00",
                     sep = "")
  file.ref <- which((substr(mapping2$original_file_name,1,8)
                     ==substr(date_time,1,8)) & 
                      (substr(mapping2$original_file_name,10,15) < 
                         substr(date_time,10,15)))
  if(length(file.ref)>1) {
    file.ref <- max(file.ref)
  }
  file.id <- mapping2$id[file.ref]
  site.id <- mapping2$site_id[file.ref]
  dur <- mapping2$duration_seconds[file.ref]
  a <- c(a,file.ref)
  file.ids <- c(file.ids, file.id)
  site.ids <- c(site.ids, site.id)
  date_times <- c(date_times, date_time)
  hour <- c(hour, hour1)
  minute <- c(minute, minute1)
  site <- c(site,ste)
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

dataset <- cbind(list_Woondum,file.ref,file.ids, site.ids,site,
                 date_times,hour, minute,orig.files, seconds.into.rec, 
                 duration,sec.remainder)

write.csv(dataset, row.names=F, file=paste("cluster", n, "_dataset_Woondum.csv", sep=""))
