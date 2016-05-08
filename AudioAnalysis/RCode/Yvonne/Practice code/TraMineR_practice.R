gym_clust <- read.csv("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\hybrid_clust_knn_17500_3.csv", header=T)
gym_clust_30 <- gym_clust[,6]
gym_clust_60 <- gym_clust[,12]
# transform data into consecutive days
g_c_30 <- data.frame(matrix(NA, nrow = 1441, ncol = (length(gym_clust_30)/1440)))
g_c_60 <- data.frame(matrix(NA, nrow = 1441, ncol = (length(gym_clust_60)/1440)))

column.names <- NULL
for (i in 1:((length(g_c_30)/2))) {
  col.names <- paste("day_", i, sep = "")
  column.names <- c(column.names,col.names)
}

g_c_30[1,] <- column.names
g_c_30[2:1441,] <- data.frame(matrix(unlist(gym_clust_30), nrow=1440, byrow=FALSE),stringsAsFactors=FALSE) 

g_c_60[1,] <- column.names
g_c_60[2:1441,] <- data.frame(matrix(unlist(gym_clust_60), nrow=1440, byrow=FALSE),stringsAsFactors=FALSE) 
#g_c_30 <- g_c_60
setwd("C:\\Work\\CSV files\\FourMonths_repeat\\Dot_matrix_plot\\")

############################
# generate a sequence of dates
############################
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20151010", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
dates1 <- date.list
date1_gym <- date.list
date1_woon <- date.list
for (i in 1:length(dates1)) {
  date1_gym[i] <- paste("gym", dates1[i],sep = "_")
  date1_woon[i] <- paste("woon", dates1[i], sep = "-")
}
date1 <- c(date1_gym, date1_woon)

for (i in 1:length(dates)) {
  dates[i] <- paste(substr(dates[i],7,8), substr(dates[i],5,6),
                    substr(dates[i],1,4), sep = "-")
}
date <- dates
date <- rep(date, 2)
g_c_30[1,] <- date1
g_c_60[1,] <- date1

colnames(g_c_30) <- dates1
g_c_30 <- t(g_c_30)

library(TraMineR)
cols <- c(
  '0' = "#F2F2F2FF", '1' = "#00B917", '2' = "#788231",   '3' = "#FF0000",
  '4' = "#01FFFE",   '5' = "#FE8900", '6' = "#006401",   '7' = "#FFDB66",
  '8' = "#010067",   '9' = "#95003A", '10' = "#007DB5", '11' = "#BE9970",
  '12' = "#774D00", '13' = "#90FB92", '14' = "#0076FF", '15' = "#FF937E",
  '16' = "#6A826C", '17' = "#FF029D", '18' = "#0000FF", '19' = "#7A4782",
  '20' = "#7E2DD2", '21' = "#0E4CA1", '22' = "#FFA6FE", '23' = "#A42400",
  '24' = "#00AE7E", '25' = "#BB8800", '26' = "#BE9970", '27' = "#263400",
  '28' = "#C28C9F", '29' = "#FF74A3", '30' = "#01D0FF", "31" = "#6B6882",
  '32' = "#E56FFE", '33' = "#85A900", '34' = "#968AE8", '35' = "#43002C",
  '36' = "#DEFF74", '37' = "#00FFC6", '38' = "#FFE502", '39' = "#620E00",
  '40' = "#008F9C", '41' = "#98FF52", '42' = "#7544B1", '43' = "#B500FF",
  '44' = "#00FF78", '45' = "#FF6E41", '46' = "#005F39", '47' = "#004754",
  '48' = "#5FAD4E", '49' = "#A75740", '50' = "#A5FFD2", '51' = "#FFB167",
  '52' = "#009BFF", '53' = "#91D0CB") 

setwd("C:\\Users\\n0572527\\ownCloud\\Shared\\Ecoacoustics\\Resources\\Weather\\")
options(scipen=999)
weather <- read.csv("gympie_weather_matrix.csv", header = T)
g_c_30_84 <- g_c_30[84:1,]
g_c_30_84 <- cbind(weather, g_c_30_84)
g_c_30_84.lab <- c(as.character(1:30))
gympie.seq <- seqdef(g_c_30_84, 483:1922, labels=g_c_30_84.lab,
                     cpal = cols[2:31])
plot(gympie.seq, tlim=0, space=0, border=NA, cpal = cols[2:31])
seqplot(gympie.seq, type = "mt")
png("tester1.png", height = 3000, width = 3000)
seqdplot(gympie.seq, group = g_c_30_84$X0000.rel_hum < 92, 
         border = NA)
dev.off()
png("tester2.png", height = 3000, width = 3000)
seqdplot(gympie.seq, group = g_c_30_84$X0000.rel_hum < 92 & 
               g_c_30_84$X0000.press < 1014, border = NA)
dev.off()
