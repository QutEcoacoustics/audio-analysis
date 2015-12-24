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
# select out 40 random minutes from clusters x, y and z
list_9 <- which(cluster.list==9)
random_9 <-sample(list_9, 60)
a <- NULL
for (i in 1:length(random_9)) {
  # Find date
  day.ref <- floor(random_9[i]/1440) + 1
  if(day.ref < 112) {
    site <- "GympieNP"
    date <- dates[day.ref]
    hour1 <- floor((random_9[i]/1440 - (day.ref-1))*24)
    if (hour1<12) {
      hour <- paste("0",hour1,sep = "")
    }
  }
  if(day.ref > 111) {
    site <- "WoondumNP"
    date <- dates[day.ref]
    hour1 <- floor((random_9[i]/1440 - (day.ref-1))*24)
    hour1 <- as.integer(hour1)
    if (hour1<12) {
      hour <- paste("0",hour1,sep = "")
    }
  }
  minute <- (((random_9[i]/1440 - (day.ref-1))*24)-hour1)*60
  if(minute <10){
    minute <- paste("0",minute,sep = "")
  }
  date_time <- paste(substr(date,1,4),substr(date,6,7),
                     substr(date,9,10),"_",
                     hour,minute,"00",
                     sep = "")
  file.ref <- which(substr(mapping$original_file_name,1,8)
                    ==substr(date_time,1,8) & 
                      substr(mapping$original_file_name,10,15) < 
                      substr(date_time,10,15))
  a <- c(a,file.ref)
}

list <- NULL
for (i in 1:length(a)) {
  lst <- as.character(mapping$original_file_name[i])
  list <- c(list,lst)
}

