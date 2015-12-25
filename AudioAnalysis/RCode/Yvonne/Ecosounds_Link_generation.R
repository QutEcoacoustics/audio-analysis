# Generate links to select random minutes from Ecosounds website
# for Jason Wimmer
# 24 December 2015
#
setwd("C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Yvonne\\")
mapping <- read.csv("audio_recordings_from_site_1192_GympieNP.csv", header = T)[,c(1,5,6,21)]
cluster.list <- read.csv("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\hybrid_clust_17500_30.csv")

indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)
dates <- as.Date(indices$rec.date, format = "%d/%m/%Y")
dates <- unique(dates)
dates <- rep(dates, 2)
# select out 40 random minutes from clusters x, y and z
list_9 <- which(cluster.list==9)
set.seed(123)
random_9 <-list_9 #sample(list_9, 60)
a <- NULL
date_times <- NULL
hour <- NULL
minute <- NULL
seconds <- NULL
site <- NULL
file.id <- mapping$id[1]
file.ids <- NULL
site.ids <- NULL
site.id <- mapping$site_id[1]
duration <- NULL 
dur <- mapping$duration_seconds[1]
  
for (i in 1:length(random_9)) {
  # Find date
  day.ref <- floor(random_9[i]/1440) + 1
  if(day.ref < 112) {
    ste <- "GympieNP"
    date1 <- dates[day.ref]
    hour1 <- floor((random_9[i]/1440 - (day.ref-1))*24)
    hour1 <- as.integer(hour1)
    if (hour1<10) {
      hour1 <- paste("0",as.integer(hour1),sep = "")
    }
  }
  if(day.ref > 111) {
    ste <- "WoondumNP"
    date1 <- dates[day.ref]
    hour1 <- floor((random_9[i]/1440 - (day.ref-1))*24)
    hour1 <- as.integer(hour1)
    if (hour1<10) {
      hour1 <- paste("0",as.integer(hour1),sep = "")
    }
  }
  minute1 <- ((((random_9[i]/1440) - (day.ref-1))*24)-as.integer(hour1))*60
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
  file.ref <- which((substr(mapping$original_file_name,1,8)
                     ==substr(date_time,1,8)) & 
                      (substr(mapping$original_file_name,10,15) < 
                         substr(date_time,10,15)))
  time.ref <- NULL
  if(length(file.ref)>1) {
    for(j in file.ref) {
      time.rf <- substr(mapping$original_file_name[j],10,13)
      time.ref <- c(time.ref, time.rf)
      ref <- which(time.ref==max(time.ref))
    }
    file.ref <- file.ref[ref]
  }
  file.id <- mapping$id[file.ref]
  site.id <- mapping$site_id[file.ref]
  dur <- mapping$duration_seconds[file.ref]
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
  lst <- as.character(mapping$original_file_name[i])
  list <- c(list,lst)
}

file.ref <- a
orig.files <- list

# determine the number of seconds since the start of the recording
sec <- NULL
for (i in 1:length(list_9)) {
  rec.start.hour <- substr(mapping$original_file_name[file.ref[i]],10,11)
  rec.start.min <- substr(mapping$original_file_name[file.ref[i]],12,13)
  rec.start.sec <- substr(mapping$original_file_name[file.ref[i]],14,15)
  total.rec.start.sec <- (as.integer(rec.start.hour)*3600 +
                          as.integer(rec.start.min)*60 + 
                          as.integer(rec.start.sec)) 
  actual.sec.since.midnight <- as.integer(hour[i])*3600 + as.integer(minute[i])*60
  diff.sec <- actual.sec.since.midnight-total.rec.start.sec
  sec <- c(sec, diff.sec)
}

seconds.into.rec <- sec
sec.remainder <- duration - seconds.into.rec 

dataset <- cbind(list_9,file.ref,file.ids, site.ids,site,date_times,hour,
                 minute,orig.files, seconds.into.rec, duration,sec.remainder)
write.csv(dataset, "cluster9_dataset.csv", row.names=F)

