# Title:  Sunrise & Sunset times from Geoscience Australia
# Author: Yvonne Phillips
# Date:   24 September 2017

# The website where the data came from is 
# http://www.ga.gov.au/geodesy/astro/sunrise.jsp

# remove all objects in the global environment
rm(list = ls())

year <- "2016"
site <- "Gympie"
file <- paste("C:\\Work\\Bioacoustics references\\Geoscience Australia  Geodesy - Astronomical Information Gympie sunrise "
              , year,".htm",sep = "")
dat <- read.csv(file = file, header = T)
dat[1:(nrow(dat)-78),] <- dat[79:nrow(dat),]
#View(dat)

date <- rep(1:31, 12)
month <- rep(c("Jan","Feb","Mar","Apr","May","Jun",
      "Jul","Aug","Sep","Oct","Nov","Dec"), each=31)

sequ <- 5
seq <- 5
for(i in 1:6) {
  sequ <- sequ + 5 
  seq <- c(seq, sequ)
  sequ <- sequ + 8
  seq <- c(seq, sequ)
}
seq <- seq[1:(length(seq)-1)]

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a list of dates (General) ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a sequence of dates
start <-  strptime(paste(year,"0101",sep = ""), format="%Y%m%d")
finish <-  strptime(paste(year,"1231",sep = ""), format="%Y%m%d") 
dates <- seq(start, finish, by = "1440 mins")

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Extract sunrise and sunset times
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
lines1 <- c(8:38)
lines2 <- c(41:71)
lines <- NULL
Sunrise_sunset <- NULL
for(i in 1:31) {
  lin <- c(lines1[i],lines2[i])
  lines <- c(lines, lin)
}

for(i in lines) {
  for(j in 1:length(seq)) {
    a <- substr(dat[i,], seq[j], (seq[j]+3))
    Sunrise_sunset <- c(Sunrise_sunset, a)
  }
}
dataframe <- matrix(Sunrise_sunset, ncol = 24, byrow = TRUE)

seq1 <- seq(1, ncol(dataframe), 2)
seq2 <- seq(2, ncol(dataframe), 2)

Sunrise <- NULL
for(i in seq1) {     #1:31) {
  for(j in 1:31) {   #seq1) {
    a <- dataframe[j,i]
    Sunrise <- c(Sunrise, a)
  }
}

Sunset <- NULL
for(i in seq2) {
  for(j in 1:31) {
    a <- dataframe[j,i]
    Sunset <- c(Sunset, a)
  }
}
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Extract civil_sunrise and civil_sunset times
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
lines1 <- c(78:108)
lines2 <- c(111:141)
lines <- NULL
CivSunrise_sunset <- NULL
for(i in 1:31) {
  lin <- c(lines1[i],lines2[i])
  lines <- c(lines, lin)
}

for(i in lines) {
  for(j in 1:length(seq)) {
    a <- substr(dat[i,], seq[j], (seq[j]+3))
    CivSunrise_sunset <- c(CivSunrise_sunset, a)
  }
}
dataframe <- matrix(CivSunrise_sunset, ncol = 24, byrow = TRUE)

seq1 <- seq(1, ncol(dataframe),2)
seq2 <- seq(2, ncol(dataframe), 2)

CivSunrise <- NULL
for(i in seq1) {     
  for(j in 1:31) {   
    a <- dataframe[j,i]
    CivSunrise <- c(CivSunrise, a)
  }
}

CivSunset <- NULL
for(i in seq2) {     
  for(j in 1:31) {   
    a <- dataframe[j,i]
    CivSunset <- c(CivSunset, a)
  }
}
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Extract naut_sunrise and civil_sunset times
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
lines1 <- c(148:178)
lines2 <- c(181:211)
lines <- NULL
NautSunrise_sunset <- NULL
for(i in 1:31) {
  lin <- c(lines1[i],lines2[i])
  lines <- c(lines, lin)
}

for(i in lines) {
  for(j in 1:length(seq)) {
    a <- substr(dat[i,], seq[j], (seq[j]+3))
    NautSunrise_sunset <- c(NautSunrise_sunset, a)
  }
}
dataframe <- matrix(NautSunrise_sunset, ncol = 24, byrow = TRUE)

seq1 <- seq(1, ncol(dataframe),2)
seq2 <- seq(2, ncol(dataframe), 2)

NautSunrise <- NULL
for(i in seq1) {     
  for(j in 1:31) {   
    a <- dataframe[j,i]
    NautSunrise <- c(NautSunrise, a)
  }
}

NautSunset <- NULL
for(i in seq2) {     
  for(j in 1:31) {   
    a <- dataframe[j,i]
    NautSunset <- c(NautSunset, a)
  }
}
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Collate all sunrises and sunsets
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
all_times <- cbind(Sunrise, Sunset, CivSunrise, CivSunset,
                   NautSunrise, NautSunset)
all_times <- data.frame(all_times)
# remove empty lines
a <- which(all_times[,2]=="    "|all_times[,2]=="") 
all_times <- all_times[-a,]
all_times <- cbind(dates, all_times)

write.csv(all_times, paste("data\\Geoscience_Australia_Sunrise_times_", site,"_",year,".csv", sep = ""), row.names = F)

# Calculate difference between civil dawn and sunrise
site <- "Gympie"
year <- "2016"
sun_data <- read.csv(paste("data\\Geoscience_Australia_Sunrise_times_", site,"_",year,".csv", sep = ""))
diff_sunrise <- NULL
for(i in 1:nrow(sun_data)) {
  sunrise_hour <- substr(sun_data$Sunrise[i],1,1)
  sunrise_min <- substr(sun_data$Sunrise[i],2,3)
  sunrise_in_min <- as.numeric(sunrise_hour)*60 + as.numeric(sunrise_min)
  civil_sunrise_hour <- substr(sun_data$CivSunrise[i],1,1)
  civil_sunrise_min <- substr(sun_data$CivSunrise[i],2,3)
  civil_sunrise_in_min <- as.numeric(civil_sunrise_hour)*60 + as.numeric(civil_sunrise_min)
  difference_in_sunrise <- sunrise_in_min - civil_sunrise_in_min 
  diff_sunrise <- c(diff_sunrise, difference_in_sunrise)
}
sun_data <- cbind(sun_data, diff_sunrise)
write.csv(sun_data, paste("data\\Geoscience_Australia_Sunrise_times_", site,"_",year,".csv", sep = ""), row.names = F)
