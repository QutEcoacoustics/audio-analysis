# Channel Integrity
# use Shift Alt J to find the Channel Decibel difference plot for Gympie
# NOTE : See line 88 for data to avoid lines 4 to 87
rm(list = ls())
folder <- "D:/Channel Integrity/2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95/"
pattern <- "+1000__Towsey.ChannelIntegrity.Indices.csv"
# Obtain a list of the original wave files
myFiles <- list.files(full.names=FALSE, pattern=pattern, 
                      path=folder, recursive = TRUE)
myFiles

gympie <- grep("Gympie", myFiles)
gympie_files <- myFiles[gympie]
gympie_files
for(i in 1:length(gympie_files)) {
  gympie_files[i] <- paste(folder, gympie_files[i], sep="")
}

gym_statistics <- NULL
for(i in 1:length(gympie_files)) {
  stat <- read.csv(gympie_files[i])
  date_file <- substr(gympie_files[i],161,175)
  stat <- cbind(date_file, stat)
  gym_statistics <- rbind(gym_statistics, stat)
  print(i)
}
for(i in 1:nrow(gym_statistics)) {
  if(as.numeric(substr(gym_statistics$date_file[i],14,15) > 31)) {
    gym_statistics$miniute[i] <- ((as.numeric(substr(gym_statistics$date_file[i],10,11))*60) +
                                    as.numeric(substr(gym_statistics$date_file[i],12,13)) +
                                    gym_statistics$RankOrder[i]+1)  
  }
  if(as.numeric(substr(gym_statistics$date_file[i],14,15) <= 31)) {
    gym_statistics$miniute[i] <- ((as.numeric(substr(gym_statistics$date_file[i],10,11))*60) +
                                    as.numeric(substr(gym_statistics$date_file[i],12,13)) +
                                    gym_statistics$RankOrder[i])  
  }
  print(i)
}
View(gym_statistics)

# generate a sequence of dates
start <-  strptime("20150705", format="%Y%m%d")
finish <- strptime("20160611", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)

statistics_plus <- NULL
statistics_plus$date <- rep(dates, each = 1440)
statistics_plus$minute <- rep(0:1439, length(dates))
statistics_plus$ZeroCrossingFractionLeft <- rep(-10, length(rep(dates, each = 1440)))
statistics_plus$ZeroCrossingFractionRight <- rep(-10, length(rep(dates, each = 1440)))
statistics_plus$ChannelSimilarity <- rep(-10, length(rep(dates, each = 1440)))
statistics_plus$ChannelDiffDecibels <- rep(-10, length(rep(dates, each = 1440)))
statistics_plus$ChannelBiasDecibels <- rep(-10, length(rep(dates, each = 1440)))
gym_statistics$date_file <- as.character(gym_statistics$date_file)

for(i in 1:nrow(gym_statistics)) {
  a <- -10
  a <- which((statistics_plus$date==substr(gym_statistics$date_file[i],1,8)) &
               (statistics_plus$minute==gym_statistics$miniute[i]))
  a <- as.numeric(a)
  if(length(a) > 0) {
    statistics_plus$ZeroCrossingFractionLeft[a] <- gym_statistics$ZeroCrossingFractionLeft[i]
    statistics_plus$ZeroCrossingFractionRight[a] <- gym_statistics$ZeroCrossingFractionRight[i]
    statistics_plus$ChannelSimilarity[a] <- gym_statistics$ChannelSimilarity[i]
    statistics_plus$ChannelDiffDecibels[a] <- gym_statistics$ChannelDiffDecibels[i]
    statistics_plus$ChannelBiasDecibels[a] <- gym_statistics$ChannelBiasDecibels[i]
  }
  print(i)
}

write.csv(statistics_plus, "Gympie_channel_integrity.csv", row.names = F)

integrity <- read.csv("C:\\Work2\\Channel integrity\\Gympie_channel_integrity.csv", header=T)

date <- "20151110"
date2 <- "10 November 2015"
a <- which(integrity$date==date)
range <- c(1:405)
range <- c(812:1217)

tiff(paste("zero_crossing_and_decibel_difference_",
           date,"_", range[1],"_",range[length(range)],
           ".tiff",sep=""),
     height=2000, width=1713, res=300)
par(mfrow=c(2,1), mgp=c(1.5, 0.6, 0), 
    mar=c(2.7, 2.8, 1.3, 0.5))
plot(integrity$ZeroCrossingFractionLeft[a[range]], 
     ylim=c(0,0.4), type="l", ylab="",
     xlab="Time", main="Zero crossing rate",
     col="#0072B2",xaxt="n", las=1)
mtext(side=2, "Zero crossing rate", line=1.84)
mtext(side=3, date2, adj=0.02, line=-1, cex=0.8)
at <- c(1,180,360,480, 720, 900, 1080, 1260,1440)
at <- c(1,180,360,480, 720, 900, 1080, 1260,1440)
at <- seq(60,1440,60)
b <- which(range %in% at)
#at <- at+range[1]
abline(v=b, lty=2)
labels <- c("00","01","02","03","04","05","06",
            "07","08","09","10","11","12",
            "13","14","15","16","17","18",
            "19","20","21","22","23")
labels <- c("1 pm","2 pm","3 pm","4 pm","5 pm","6 pm","7 pm","8 pm")
axis(side=1,at=b, labels=labels)
par(new=TRUE)
plot(integrity$ZeroCrossingFractionRight[a[range]], 
     ylim=c(0,0.4), type="l", ylab="",
     xlab="", main="", 
     col="#D55E00", xaxt="n", yaxt="n")
legend(x=250, y=0.08, lty = c(1,1), 
       c("left channel", "right channel"), 
       col=c("#0072B2","#D55E00"),
       bty = "n", cex = 1.2,
       y.intersp=0.8)
mtext("A.", adj=c(-0.08), cex=1.2, padj=-0.4)
plot(integrity$ChannelDiffDecibels[a[range]], 
     ylim=c(0,8), type="l", ylab="",
     xlab="Time", main="Decibel difference",
     xaxt="n", las=1)
mtext(side=2, "Decibel difference (dB)", line=1.84)
mtext(side=3, date2, adj=0.02, line=-1, cex=0.8)
axis(side=1,at=b, labels=labels)
abline(v=b, lty=2)
#mtext("b.", adj=-0.08, cex=1.4)
mtext("B.", adj=c(-0.08), cex=1.2, padj=-0.4)
dev.off()

date <- "20150824"
a <- which(integrity$date==date)
range <- c(812:1217)
tiff(paste("zero_crossing_and_decibel_difference_",
           date,"_", range[1],"_",range[length(range)],
           ".tiff",sep=""),
     height=2000, width=1713, res=300)
par(mfrow=c(2,1), mgp=c(1.5, 0.6, 0), 
    mar=c(2.7, 2.8, 1.3, 0.5))
plot(integrity$ZeroCrossingFractionLeft[a[range]], 
     ylim=c(0,0.4), type="l", ylab="",
     xlab="Time", main="Zero crossing rate",
     col="#0072B2",xaxt="n", las=1)
mtext(side=2, "Zero crossing rate", line=1.84)
at <- c(1,180,360,480, 720, 900, 1080, 1260,1440)
b <- which(range %in% at)
#at <- at+range[1]
abline(v=b, lty=2)
labels <- c("00","03", "06","09", "12","15", "18","21", "24")
labels <- c("3 pm","6 pm")
axis(side=1,at=b, labels=labels)
par(new=TRUE)
plot(integrity$ZeroCrossingFractionRight[a[range]], 
     ylim=c(0,0.4), type="l", ylab="",
     xlab="", main="", 
     col="#D55E00", xaxt="n", yaxt="n")
legend(x=250, y=0.08, lty = c(1,1), 
       c("left channel", "right channel"), 
       col=c("#0072B2","#D55E00"),
       bty = "n", cex = 1.2,
       y.intersp=0.8)
mtext("A.", adj=c(-0.08), cex=1.2, padj=-0.4)
plot(integrity$ChannelDiffDecibels[a[range]], 
     ylim=c(0,8), type="l", ylab="",
     xlab="Time", main="Decibel difference",
     xaxt="n", las=1)
mtext(side=2, "Decibel difference (dB)", line=1.84)
axis(side=1,at=b, labels=labels)
abline(v=b, lty=2)
#mtext("b.", adj=-0.08, cex=1.4)
mtext("B.", adj=c(-0.08), cex=1.2, padj=-0.4)
dev.off()


plot(integrity$ChannelDiffDecibels[a[361:720]], 
     ylim=c(0,9), type="l", ylab="Decibel difference (dB)",
     xlab="Time (minutes", main="Decibel difference")
plot(integrity$ChannelDiffDecibels[a[711:1080]], 
     ylim=c(0,9), type="l", ylab="Decibel difference (dB)",
     xlab="Time (minutes", main="Decibel difference")
plot(integrity$ChannelDiffDecibels[a[1081:1440]], 
     ylim=c(0,9), type="l", ylab="Decibel difference (dB)",
     xlab="Time (minutes", main="Decibel difference")


# Two tone-plot of Zero-CrossingFractionLeft
# The palette with grey:
#cbPalette <- c("#999999", "#E69F00", "#56B4E9", 
#              "#009E73", "#F0E442", "#0072B2", 
#               "#D55E00", "#CC79A7")

# The palette with black:
cbbPalette <- c("#000000", "#E69F00", "#56B4E9", 
                "#009E73", "#F0E442", "#0072B2", 
                "#D55E00", "#CC79A7")
library(scales)
show_col(cbbPalette)
library(gplots)
hex(RGB(1, 114, 178))

x <- c("0 114 178", "86 180 233",
       "0 158 115", "240 228 66", 
       "230 159 0", "213 94 0")
sapply(strsplit(x, " "), function(x)
  rgb(x[1], x[2], x[3], maxColorValue=255))
#show_col(cbbPalette)
statistics_plus <- read.csv("C:\\Work2\\Channel integrity\\Gympie_channel_integrity_final.csv", header = T)

#smooth the zero-crossing-rate over 120 minutes
statistics_plus$smooth_zcr_left <- 0
n <- 120
statistics_plus$smooth_zcr_left <- filter(statistics_plus$ZeroCrossingFractionLeft, rep(1, n))/n
statistics_plus$smooth_zcr_right <- 0
n <- 120
statistics_plus$smooth_zcr_right <- filter(statistics_plus$ZeroCrossingFractionRight, rep(1, n))/n
# difference between the smoothed zero-crossing rates
statistics_plus$diff_smooth_zcr <- 0
statistics_plus$diff_smooth_zcr <- abs(statistics_plus$smooth_zcr_left - statistics_plus$smooth_zcr_right)
statistics_plus_5July_31Jan <- statistics_plus[1:309600,]

affected_files <- read.csv("C:\\Work\\Statistics\\full Weka dataset.csv", header = T)
affected_files <- affected_files[1:847,c(1,25)]

# add another column to statistics_plus_5July_31Jan
affected_files$date <- substr(affected_files$Filename,6,13)
affected_files$minute <- as.numeric(substr(affected_files$Filename,15,16))*60 +
  as.numeric(substr(affected_files$Filename,17,18))
for(i in 1:(length(affected_files$Filename)-1)) {
  label <- as.character(affected_files$labelD[i])
  date <- affected_files$date[i]
  minute1 <- affected_files$minute[i]
  minute2 <- affected_files$minute[i+1]
  if(minute2<=2) minute2<- 1440
  a <- which(statistics_plus_5July_31Jan$date==date)
  b <- which(statistics_plus_5July_31Jan$minute>=minute1)
  b <- intersect(a,b)
  c <- which(statistics_plus_5July_31Jan$minute < minute2)
  c <- intersect(b,c)
  statistics_plus_5July_31Jan$label[c] <- label 
  print(i)
}
a <- which(statistics_plus_5July_31Jan$ZeroCrossingFractionLeft>0) 
statistics_plus_5July_31Jan_comp <- statistics_plus_5July_31Jan[a,]
#smooth the zero-crossing-rate over 120 minutes
statistics_plus_5July_31Jan_comp$smooth_zcr_left <- 0
n <- 120
statistics_plus_5July_31Jan_comp$smooth_zcr_left <- filter(statistics_plus_5July_31Jan_comp$ZeroCrossingFractionLeft, rep(1, n))/n
statistics_plus_5July_31Jan_comp$smooth_zcr_right <- 0
n <- 120
statistics_plus_5July_31Jan_comp$smooth_zcr_right <- filter(statistics_plus_5July_31Jan_comp$ZeroCrossingFractionRight, rep(1, n))/n
# difference between the smoothed zero-crossing rates
statistics_plus_5July_31Jan_comp$diff_smooth_zcr <- 0
statistics_plus_5July_31Jan_comp$diff_smooth_zcr <- abs(statistics_plus_5July_31Jan_comp$smooth_zcr_left - statistics_plus_5July_31Jan_comp$smooth_zcr_right)

write.csv(statistics_plus_5July_31Jan_comp, row.names = F,
          "C:\\Work2\\Channel integrity\\Gympie_channel_integrity_5July_31Jan.csv")


affected_files <- read.csv("C:\\Work\\Statistics\\full Weka dataset.csv", header = T)
affected_files <- affected_files[1:847,c(1,25)]
affected_files$date <- substr(affected_files$Filename,6,13)
affected_files$minute <- as.numeric(substr(affected_files$Filename,15,16))*60 +
  as.numeric(substr(affected_files$Filename,17,18))

weka_data <- read.csv("C:\\Work2\\Channel integrity\\Weka_3_var_Gympie_channel_integrity_5July_31Jan.csv", header=T)
weka_data$start <- TRUE
# mark the files where the affected files commenced
for(i in 1:(nrow(affected_files)-1)) {
  label <- as.character(affected_files$labelD[i])
  date <- affected_files$date[i]
  minute1 <- affected_files$minute[i]
  minute2 <- affected_files$minute[i+1]
  if(minute2<=2) minute2<- 1440
  a <- which(weka_data$date==date)
  b <- which(weka_data$minute>=minute1)
  b <- intersect(a,b)
  c <- which(weka_data$minute < minute2)
  c <- intersect(b,c)
  if(!affected_files$labelD[i]==affected_files$label[(i+1)])
  weka_data$start[c] <- "start"
  print(i)
}

for(i in 1:(nrow(affected_files)-1)) {
  date <- affected_files$date[(i+1)]
  minute1 <- affected_files$minute[(i+1)]
  a <- which(weka_data$date==date)
  b <- which(weka_data$minute>=minute1)
  b <- intersect(a,b)
  if(affected_files$labelD[i]=="not" & affected_files$label[(i+1)] %in% c("left_affected","both","not_working_left"))
    weka_data$start[b] <- "start"
  print(i)
}
a <- which(weka_data$label=="not")
weka_data$start[a] <- TRUE

write.csv(weka_data, row.names = F,
      "C:\\Work2\\Channel integrity\\Weka_3_with_starts_Gympie_channel_integrity_5July_31Jan.csv")

Weka_data <- read.csv("C:\\Work2\\Channel integrity\\Weka_3_with_starts_Gympie_channel_integrity_5July_31Jan.csv",
         header=T)
a <- which(Weka_data$start=="start")
Weka_data_1 <- Weka_data[-a,]
write.csv(Weka_data_1, row.names = F,
          "C:\\Work2\\Channel integrity\\Weka_test_Gympie_channel_integrity_5July_31Jan.csv")

weka_data <- read.csv("C:\\Work2\\Channel integrity\\Gympie_channel_integrity.csv")

weka_data$zcr_diff <- abs(weka_data$ZeroCrossingFractionLeft - weka_data$ZeroCrossingFractionRight)
a <- which(weka_data$start==TRUE)
weka_data <- weka_data[a,]
write.csv(weka_data, "Weka_test_data.csv", row.names = F)
a <- which(weka_data$label=="both")
weka_data$label[a] <- as.factor("left_affected")
write.csv(weka_data, "Weka_merged_test_data.csv", row.names = F)

weka_data <- read.csv("C:\\Work2\\Channel integrity\\Weka_test_data.csv", header=T)
#View(statistics_plus)
statistics_plus$abs_minute <- 1:nrow(statistics_plus)
a <- which(statistics_plus$ZeroCrossingFractionLeft==-10)
statistics_plus$ZeroCrossingFractionLeft[a] <- NA
statistics_plus$ZeroCrossingFractionRight[a] <- NA
statistics_plus$ChannelSimilarity[a] <- NA
statistics_plus$ChannelDiffDecibels[a] <- NA
statistics_plus$ChannelBiasDecibels[a] <- NA
statistics_plus$ZCFL_col <- NA # Zero crossing rate left colour
statistics_plus$ZCFR_col <- NA # Zero crossing rate right colour
statistics_plus$CS_col <- NA # Channel similarity colour
statistics_plus$CDD_col <- NA # Channel decibel difference colour
statistics_plus$CBD_norm_col <- NA 

# normalise the ChannelBiasDecibels between -6 and 6 
# and then scale between 0 and 1
statistics_plus$CBD_norm <- statistics_plus$ChannelBiasDecibels
a <- which(statistics_plus$CBD_norm > 6)
statistics_plus$CBD_norm[a] <- 6
a <- which(statistics_plus$CBD_norm < -6)
statistics_plus$CBD_norm[a] <- -6
# scale between 0 and 1.2
statistics_plus$CBD_norm <- (statistics_plus$CBD_norm+6)/10

max_zero_crossing <- 0.8
max_channel_similarity <- 1
max_channel_diff_decibels <- 7.5
max_cBD_norm <- 1.2

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_zero_crossing
A <- max/5 # interval
a <- which(statistics_plus$ZeroCrossingFractionLeft < 1*A)
statistics_plus$ZCFL_col[a] <- 1
b <- which(statistics_plus$ZeroCrossingFractionLeft < 2*A)
c <- setdiff(b,a) 
statistics_plus$ZCFL_col[c] <- 2
c <- which(statistics_plus$ZeroCrossingFractionLeft < 3*A)
d <- setdiff(c,b) 
statistics_plus$ZCFL_col[d] <- 3
d <- which(statistics_plus$ZeroCrossingFractionLeft < 4*A)
e <- setdiff(d,c) 
statistics_plus$ZCFL_col[e] <- 4
e <- which(statistics_plus$ZeroCrossingFractionLeft < 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFL_col[f] <- 5
e <- which(statistics_plus$ZeroCrossingFractionLeft >= 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFL_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_zero_crossing
A <- max/5 # interval
a <- which(statistics_plus$ZeroCrossingFractionRight < 1*A)
statistics_plus$ZCFR_col[a] <- 1
b <- which(statistics_plus$ZeroCrossingFractionRight < 2*A)
c <- setdiff(b,a) 
statistics_plus$ZCFR_col[c] <- 2
c <- which(statistics_plus$ZeroCrossingFractionRight < 3*A)
d <- setdiff(c,b) 
statistics_plus$ZCFR_col[d] <- 3
d <- which(statistics_plus$ZeroCrossingFractionRight < 4*A)
e <- setdiff(d,c) 
statistics_plus$ZCFR_col[e] <- 4
e <- which(statistics_plus$ZeroCrossingFractionRight < 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFR_col[f] <- 5
e <- which(statistics_plus$ZeroCrossingFractionRight >= 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFR_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_channel_similarity
A <- max/5 # interval
a <- which(statistics_plus$ChannelSimilarity < 1*A)
statistics_plus$CS_col[a] <- 1
b <- which(statistics_plus$ChannelSimilarity < 2*A)
c <- setdiff(b,a) 
statistics_plus$CS_col[c] <- 2
c <- which(statistics_plus$ChannelSimilarity < 3*A)
d <- setdiff(c,b) 
statistics_plus$CS_col[d] <- 3
d <- which(statistics_plus$ChannelSimilarity < 4*A)
e <- setdiff(d,c) 
statistics_plus$CS_col[e] <- 4
e <- which(statistics_plus$ChannelSimilarity < 5*A)
f <- setdiff(e,d) 
statistics_plus$CS_col[f] <- 5
e <- which(statistics_plus$ChannelSimilarity >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CS_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_channel_diff_decibels
A <- max/5 # interval
a <- which(statistics_plus$ChannelDiffDecibels < 1*A)
statistics_plus$CDD_col[a] <- 1
b <- which(statistics_plus$ChannelDiffDecibels < 2*A)
c <- setdiff(b,a) 
statistics_plus$CDD_col[c] <- 2
c <- which(statistics_plus$ChannelDiffDecibels < 3*A)
d <- setdiff(c,b) 
statistics_plus$CDD_col[d] <- 3
d <- which(statistics_plus$ChannelDiffDecibels < 4*A)
e <- setdiff(d,c) 
statistics_plus$CDD_col[e] <- 4
e <- which(statistics_plus$ChannelDiffDecibels < 5*A)
f <- setdiff(e,d) 
statistics_plus$CDD_col[f] <- 5
e <- which(statistics_plus$ChannelDiffDecibels >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CDD_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_cBD_norm
A <- max/5 # interval
a <- which(statistics_plus$CBD_norm < 1*A)
statistics_plus$CBD_norm_col[a] <- 1
b <- which(statistics_plus$CBD_norm < 2*A)
c <- setdiff(b,a) 
statistics_plus$CBD_norm_col[c] <- 2
c <- which(statistics_plus$CBD_norm < 3*A)
d <- setdiff(c,b) 
statistics_plus$CBD_norm_col[d] <- 3
d <- which(statistics_plus$CBD_norm < 4*A)
e <- setdiff(d,c) 
statistics_plus$CBD_norm_col[e] <- 4
e <- which(statistics_plus$CBD_norm < 5*A)
f <- setdiff(e,d) 
statistics_plus$CBD_norm_col[f] <- 5
e <- which(statistics_plus$CBD_norm >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CBD_norm_col[f] <- 5

#View(statistics_plus)
write.csv(statistics_plus, "Gympie_channel_integrity_final.csv", row.names = F)

folder <- 
# Gympie plot ---------------
# Add columns containing the heights for the overplotting
# heights of zero-crossing_left
max_zero_crossing <- 0.8
max_channel_similarity <- 1
max_channel_difference <- 1
max_channel_diff_decibels <- 8
max_cBD_norm <- 1.2
B <- 0 
h <- 2
statistics_plus$ZCL_height <- NA
max <- max_zero_crossing
A <- max/5
for(i in 1:length(statistics_plus$ZeroCrossingFractionLeft)) {
  value <- statistics_plus$ZeroCrossingFractionLeft[i]
  height <- (value - (B+A*(statistics_plus$ZCFL_col[i]-1)))*h/A
  statistics_plus$ZCL_height[i] <- height
  print(i)
}

statistics_plus$ZCR_height <- NA
max <- max_zero_crossing
A <- max/5
for(i in 1:length(statistics_plus$ZeroCrossingFractionRight)) {
  value <- statistics_plus$ZeroCrossingFractionRight[i]
  height <- (value - (B+A*(statistics_plus$ZCFR_col[i]-1)))*h/A
  statistics_plus$ZCR_height[i] <- height
  print(i)
}

statistics_plus$CS_height <- NA
max <- max_channel_similarity
A <- max/5
for(i in 1:length(statistics_plus$ChannelSimilarity)) {
  value <- statistics_plus$ChannelSimilarity[i]
  height <- (value - (B+A*(statistics_plus$CS_col[i]-1)))*h/A
  statistics_plus$CS_height[i] <- height
  print(i)
}

# normalise the Channel Decibel difference to 0 and 7.5
max(statistics_plus$ChannelDiffDecibels, na.rm=T)
statistics_plus$CDD_norm <- NA
statistics_plus$CDD_norm <- statistics_plus$ChannelDiffDecibels
a <- which(statistics_plus$ChannelDiffDecibels > 8)
statistics_plus$CDD_norm[a] <- 8
  
statistics_plus$CDD_height <- NA
max <- max_channel_diff_decibels
A <- max/5
for(i in 1:length(statistics_plus$ChannelDiffDecibels)) {
  value <- statistics_plus$CDD_norm[i]
  height <- (value - (B+A*(statistics_plus$CDD_col[i]-1)))*h/A
  statistics_plus$CDD_height[i] <- height
  print(i)
}

statistics_plus$CBD_norm_height <- NA
max <- max_cBD_norm
A <- max/5
for(i in 1:length(statistics_plus$ChannelBiasDecibels)) {
  value <- statistics_plus$CBD_norm[i]
  height <- (value - (B+A*(statistics_plus$CBD_norm_col[i]-1)))*h/A
  statistics_plus$CBD_norm_height[i] <- height
  print(i)
}

# calculate the channel difference from the channel similarity
statistics_plus$channel_diff <- 1 - statistics_plus$ChannelSimilarity
statistics_plus$CD_col <- NA
B <- 0   # lower threshold
h <- 2 # height of line
max <- max_channel_difference
A <- max/5 # interval
a <- which(statistics_plus$channel_diff < 1*A)
statistics_plus$CD_col[a] <- 1
b <- which(statistics_plus$channel_diff < 2*A)
c <- setdiff(b,a) 
statistics_plus$CD_col[c] <- 2
c <- which(statistics_plus$channel_diff < 3*A)
d <- setdiff(c,b) 
statistics_plus$CD_col[d] <- 3
d <- which(statistics_plus$channel_diff < 4*A)
e <- setdiff(d,c) 
statistics_plus$CD_col[e] <- 4
e <- which(statistics_plus$channel_diff < 5*A)
f <- setdiff(e,d) 
statistics_plus$CD_col[f] <- 5
e <- which(statistics_plus$channel_diff >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CD_col[f] <- 5

statistics_plus$CD_height <- NA
max <- max_channel_difference
A <- max/5
for(i in 1:length(statistics_plus$channel_diff)) {
  value <- statistics_plus$channel_diff[i]
  height <- (value - (B+A*(statistics_plus$CD_col[i]-1)))*h/A
  statistics_plus$CD_height[i] <- height
  print(i)
}

# gradient of zero-crossing rate
for(i in 1:(nrow(statistics_plus)-1)) {
  statistics_plus$zcrL_grad[i] <- abs(statistics_plus$ZeroCrossingFractionLeft[i+1] -
                                        statistics_plus$ZeroCrossingFractionLeft[i])
  print(i)
}
sL <- summary(statistics_plus$zcrL_grad, rm.na=T)
max(statistics_plus$zcrL_grad, na.rm=T)
bL <- boxplot(statistics_plus$zcrL_grad)
bL$stats
p <- c(5,10,15,20,25, 30,35, 40,45, 50,55, 60,65, 70,75 ,80,85, 90,95)/100
q <- quantile(statistics_plus$zcrL_grad, p, na.rm=T)
q75L <- unname(q[15])

for(i in 1:(nrow(statistics_plus)-1)) {
  statistics_plus$zcrR_grad[i] <- abs(statistics_plus$ZeroCrossingFractionRight[i+1] -
                                        statistics_plus$ZeroCrossingFractionRight[i])
  print(i)
}

# determine the colours for the ZCRL_gradient
# normalise the values to the 3rd quartile q75L
a <- which(statistics_plus$zcrL_grad > q75L)
statistics_plus$zcrL_grad_norm <- statistics_plus$zcrL_grad 
statistics_plus$zcrL_grad_norm[a] <- q75L 
statistics_plus$zcrL_grad_norm_col <- NA
statistics_plus <- data.frame(statistics_plus)

B <- 0   # lower threshold
h <- 2 # height of line
max <- unname(q75L)
A <- max/5 # interval
a <- which(statistics_plus$zcrL_grad_norm < 1*A)
statistics_plus$zcrL_grad_norm_col[a] <- 1
b <- which(statistics_plus$zcrL_grad_norm < 2*A)
c <- setdiff(b,a) 
statistics_plus$zcrL_grad_norm_col[c] <- 2
c <- which(statistics_plus$zcrL_grad_norm < 3*A)
d <- setdiff(c,b) 
statistics_plus$zcrL_grad_norm_col[d] <- 3
d <- which(statistics_plus$zcrL_grad_norm < 4*A)
e <- setdiff(d,c) 
statistics_plus$zcrL_grad_norm_col[e] <- 4
e <- which(statistics_plus$zcrL_grad_norm < 5*A)
f <- setdiff(e,d) 
statistics_plus$zcrL_grad_norm_col[f] <- 5
e <- which(statistics_plus$zcrL_grad_norm >= 5*A)
f <- setdiff(e,d) 
statistics_plus$zcrL_grad_norm_col[f] <- 5

statistics_plus$zcr_grad_norm_height <- NA
max <- q75L
A <- max/5
for(i in 1:length(statistics_plus$zcrL_grad_norm)) {
  value <- statistics_plus$zcrL_grad_norm[i]
  height <- (value - (B+A*(statistics_plus$zcrL_grad_norm_col[i]-1)))*h/A
  statistics_plus$zcr_grad_norm_height[i] <- height
  print(i)
}

sR <- summary(statistics_plus$zcrR_grad, rm.na=T)
bR <- boxplot(statistics_plus$zcrR_grad)
bR$stats[4] # hinge
sR[5]
p <- c(5,10,15,20,25, 30,35, 40,45, 50,55, 60,65, 70,75 ,80,85, 90,95)/100
q <- quantile(statistics_plus$zcrR_grad, p, na.rm=T)
q75R <- unname(q[15])

# determine the colours for the ZCRR_gradient
# normalise the values to the 3rd quartile q75R
a <- which(statistics_plus$zcrR_grad > q75R)
statistics_plus$zcrR_grad_norm <- statistics_plus$zcrR_grad 
statistics_plus$zcrR_grad_norm[a] <- q75R 
statistics_plus$zcrR_grad_norm_col <- NA
statistics_plus <- data.frame(statistics_plus)

B <- 0 # lower threshold
h <- 2 # height of line
max <- unname(q75R)
A <- max/5 # interval
a <- which(statistics_plus$zcrR_grad_norm < 1*A)
statistics_plus$zcrR_grad_norm_col[a] <- 1
b <- which(statistics_plus$zcrR_grad_norm < 2*A)
c <- setdiff(b,a) 
statistics_plus$zcrR_grad_norm_col[c] <- 2
c <- which(statistics_plus$zcrR_grad_norm < 3*A)
d <- setdiff(c,b) 
statistics_plus$zcrR_grad_norm_col[d] <- 3
d <- which(statistics_plus$zcrR_grad_norm < 4*A)
e <- setdiff(d,c) 
statistics_plus$zcrR_grad_norm_col[e] <- 4
e <- which(statistics_plus$zcrR_grad_norm < 5*A)
f <- setdiff(e,d) 
statistics_plus$zcrR_grad_norm_col[f] <- 5
e <- which(statistics_plus$zcrR_grad_norm >= 5*A)
f <- setdiff(e,d) 
statistics_plus$zcrR_grad_norm_col[f] <- 5

statistics_plus$zcrR_grad_norm_height <- NA
max <- q75R
A <- max/5
for(i in 1:length(statistics_plus$zcrR_grad_norm)) {
  value <- statistics_plus$zcrR_grad_norm[i]
  height <- (value - (B+A*(statistics_plus$zcrR_grad_norm_col[i]-1)))*h/A
  statistics_plus$zcrR_grad_norm_height[i] <- height
  print(i)
}

#write.csv(statistics_plus, "C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\data\\Gympie_channel_integrity_final.csv"
#, row.names = F)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Gympie -----------------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Warning: the first 7000 lines are empty in this file
statistics_plus <- read.csv("C:\\Work2\\Channel integrity\\Gympie_channel_integrity_final.csv", header=T)
a <- which(statistics_plus$ChannelDiffDecibels <= 1.5)
statistics_plus$CDD_col[a] <- 2
a <- which(statistics_plus$ChannelDiffDecibels > 1.5 & statistics_plus$ChannelDiffDecibels <=3.0)
statistics_plus$CDD_col[a] <- 3
a <- which(statistics_plus$ChannelDiffDecibels > 3.0 & statistics_plus$ChannelDiffDecibels <= 4.5)
statistics_plus$CDD_col[a] <- 4
a <- which(statistics_plus$ChannelDiffDecibels > 4.5 & statistics_plus$ChannelDiffDecibels <= 6.0)
statistics_plus$CDD_col[a] <- 5
a <- which(statistics_plus$ChannelDiffDecibels > 6.0)
statistics_plus$CDD_col[a] <- 6

max_channel_diff_decibels <- 7.5
a <- which(statistics_plus$ChannelDiffDecibels > max_channel_diff_decibels)
statistics_plus$CDD_norm[a] <- max_channel_diff_decibels

statistics_plus$CDD_height <- NA
max <- max_channel_diff_decibels
A <- max/5
B <- 0   # lower threshold
h <- 2 # height of line
value <- statistics_plus$CDD_norm
height <- (value - (B+A*(statistics_plus$CDD_col-2)))*h/A
statistics_plus$CDD_height <- height


#Empty plot
colour0 <- "#0072B2"
colour1 <- "#56B4E9"
colour2 <- "#009E73"
colour3 <- "#F0E442"
colour4 <- "#E69F00"
colour5 <- "#D55E00"
colours <- c(colour0,colour1,colour2,colour3,colour4,colour5)
#library(scales)
#show_col(c(colour0,colour1,colour2,colour3,colour4,colour5))
# generate a sequence of dates
start <-  strptime("20150705", format="%Y%m%d")
finish <- strptime("20160611", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)

dates
first_of_month <- dates[which(substr(dates,7,8)=="01")]

first_minute_of_month <- NULL
first_date <- as.numeric(substr(statistics_plus$date[1],7,8))
for(i in 1:length(first_of_month)) {
  a <- which(statistics_plus$date==first_of_month[i])
  a <- a[1]
  first_minute_of_month <- c(first_minute_of_month, a)
}
list <- c(1, first_minute_of_month, length(statistics_plus$date))
start <-   152640            #164161
end <- 152640+44640          #172800

# reduce to first 7 months
list <- list[1:8]

dev.off()
tiff("GympieNP_channel_integrity_Channel_decibel_difference_test2.tiff",
     height=600, width=1713, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.4, 0, 0.32, 0), mgp=c(1, 0.1, 0),
    oma=c(1.2, 0, 1.2, 0.8))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(0, 2.2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Channel Decibel Difference", cex=0.5)
    mtext(side = 3, line=0.68, "Gympie National Park", cex=0.7)
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -2.2, las=1, cex=0.5,
            paste("July ",substr(statistics_plus$date[start], 
                                 1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(0, 2.2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3, 
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Sep ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=0.5, line=-2.3,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  for(j in start:end) {
    segments(x0 = (j-1300), 
             y0 = 0, 
             x1 = (j-1300), 
             y1 = 2, 
             col = colours[statistics_plus$CDD_col[j]-1])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = (j-1300), 
             y0 = 0, 
             x1 = (j-1300), 
             y1 = statistics_plus$CDD_height[j], 
             col = colours[statistics_plus$CDD_col[j]])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = (a-1300), outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1, 0.08, 0), cex.axis=0.5)
      #abline(v=(a-1300))
      for(k in 1:length(a)) {
        segments(x0 = (a[k]-1300),x1 = (a[k]-1300), y0 = 0, y1 = 2)  
      }
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = (a-1300), outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1, 0.08, 0), cex.axis=0.5)
      #abline(v=(a-1300))
      for(k in 1:length(a)) {
        segments(x0 = (a[k]-1300),x1 = (a[k]-1300), y0 = 0, y1 = 2)
      }
    }
  }
  if(i==length(list)-1) {
    mtext(side=1, line=0.9, "Days of the month", cex=0.7)  
  }
  for(j in start:end) {
    if(cluster_list$rain[j] > 0 & cluster_list$persist_rain[j]=="yes") {
      segments(x0 = (j-1300), 
               y0 = 2.1,   #0 
               x1 = (j-1300), 
               y1 = 2.2, #cluster_list$rain[j], 
               col = "#0072B2")
    }
    print(j)
  }
  print(i)
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

k1_value <- 25000
k2_value <- 60


cluster_list <- read.csv(paste("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)

cluster_list$rain <- 0
rain <- c(2,10,17,18,21,54,59)
a <- which(cluster_list$cluster_list==2)
cluster_list$rain[a] <- 0
a <- which(cluster_list$cluster_list==10)
cluster_list$rain[a] <- 2
a <- which(cluster_list$cluster_list==17)
cluster_list$rain[a] <- 0
a <- which(cluster_list$cluster_list==18)
cluster_list$rain[a] <- 2
a <- which(cluster_list$cluster_list==21)
cluster_list$rain[a] <- 0
a <- which(cluster_list$cluster_list==54)
cluster_list$rain[a] <- 2
a <- which(cluster_list$cluster_list==59)
cluster_list$rain[a] <- 2

cluster_list$date <- "date"
cluster_list$minute <- as.integer(1)
cluster_list$date[12961:512641] <- statistics_plus$date
cluster_list$minute[12961:512641] <- statistics_plus$minute
cluster_list <- cluster_list[12961:512641,]
row.names(cluster_list) <- 1:nrow(cluster_list)

cluster_list$persist_rain <- "no"
for(i in 1:(length(cluster_list$cluster_list)-1)) {
  a <- which(cluster_list$cluster_list[i:(i+5)] %in% rain)
  a <- length(a)
  if(a > 3) {
    cluster_list$persist_rain[i] <- "yes"
  }
  print(i)
}

dev.off()
tiff("GympieNP_rain_plot_test.tiff",
     height=900, width=1713, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.7, 0, 0.32, 0), mgp=c(1,0.1,0),
    oma=c(1.2, 0, 1.2, 1.1))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(0,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Rain", cex=0.5)
    mtext(side = 3, line=0.68, "Gympie National Park", cex=0.7)
    if(substr(cluster_list$date[start],5,6)=="07") {
      mtext(side = 4, line = -2.2, las=1, cex=0.6,
            paste("July ",substr(cluster_list$date[start], 
                                 1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(0,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(cluster_list$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3, 
            paste("July ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Aug ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Sep ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Oct ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Nov ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Dec ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Jan ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Feb ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Mar ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Apr ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("May ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
    if(substr(cluster_list$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=0.6, line=-2.3,
            paste("Jun ", substr(cluster_list$date[start], 1,4), sep=""))  
    }
  } 
  
  #for(j in start:end) {
  #  segments(x0 = (j-1300), 
  #           y0 = 0, 
  #           x1 = (j-1300), 
  #           y1 = 2, 
  #           col = "white")
  #  print(j)
  #}
  for(j in start:end) {
    if(cluster_list$rain[j] > 0 & cluster_list$persist_rain[j]=="yes") {
      segments(x0 = (j-1300), 
               y0 = 0, 
               x1 = (j-1300), 
               y1 = cluster_list$rain[j], 
               col = "#D55E00")
    }
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(cluster_list$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(cluster_list$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = (a-1300), outer = F,
           labels = substr(cluster_list$date[a],7,8),
           mgp=c(1,0.3,0), cex.axis=0.7, lwd=0.7)
      abline(v=(a-1300), lwd=0.7)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(cluster_list$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(cluster_list$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = (a-1300), outer = F,
           labels = substr(cluster_list$date[a],7,8),
           mgp=c(1,0.3,0), cex.axis=0.7, lwd=0.7)
      abline(v=(a-1300), lwd=0.7)
    }
  }
  if(i==length(list)-1) {
    mtext(side=1, line=0.9, "Days of the month", cex=0.7)  
  }
  print(i)
}
dev.off()
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


dev.off()
tiff("GympieNP_channel_integrity_Channel_bias.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.6, 0, 0.2, 0), 
    oma=c(0, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Channel bias", cex=0.7)
    mtext(side = 3, line=1, "Gympie National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sep ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$CBD_norm_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$CBD_norm_height[j], 
             col = colours[statistics_plus$CBD_norm_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

dev.off()
tiff("GympieNP_channel_integrity_zero_crossing_rate_left.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.8, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Zero Crossing Rate Left", cex=0.7)
    mtext(side = 3, line=1, "Gympie National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$ZCFL_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$ZCL_height[j], 
             col = colours[statistics_plus$ZCFL_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

dev.off()
tiff("GympieNP_channel_integrity_zero_crossing_rate_right.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.8, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Zero Crossing Rate Right", cex=0.7)
    mtext(side = 3, line=1, "Gympie National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$ZCFR_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$ZCR_height[j], 
             col = colours[statistics_plus$ZCFR_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

dev.off()
tiff("GympieNP_channel_integrity_ZCL_gradient.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.8, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Zero Crossing Rate Gradient Left", cex=0.7)
    mtext(side = 3, line=1, "Gympie National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$zcrL_grad_norm_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$zcr_grad_norm_height[j], 
             col = colours[statistics_plus$zcrL_grad_norm_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

dev.off()
tiff("GympieNP_channel_integrity_ZCR_gradient.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.8, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Zero Crossing Rate Gradient Right", cex=0.7)
    mtext(side = 3, line=1, "Gympie National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$zcrR_grad_norm_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$zcrR_grad_norm_height[j], 
             col = colours[statistics_plus$zcrR_grad_norm_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Woondum -----------------------------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
folder <- "D:/Channel Integrity/2016Jun13-143647 - Yvonne, Towsey.ChannelIntegrity, #95/"
pattern <- "+1000__Towsey.ChannelIntegrity.Indices.csv"
# Obtain a list of the original wave files
myFiles <- list.files(full.names=FALSE, pattern=pattern, 
                      path=folder, recursive = TRUE)
myFiles

woondum <- grep("Woondum", myFiles)
woondum_files <- myFiles[woondum]
woondum_files
for(i in 1:length(woondum_files)) {
  woondum_files[i] <- paste(folder, woondum_files[i], sep="")
}

woon_statistics <- NULL
for(i in 1:length(woondum_files)) {
  stat <- read.csv(woondum_files[i])
  date_file <- substr(woondum_files[i],161,175)
  stat <- cbind(date_file, stat)
  woon_statistics <- rbind(woon_statistics, stat)
}
for(i in 1:nrow(woon_statistics)) {
  if(as.numeric(substr(woon_statistics$date_file[i],14,15) > 31)) {
    woon_statistics$miniute[i] <- ((as.numeric(substr(woon_statistics$date_file[i],10,11))*60) +
                                     as.numeric(substr(woon_statistics$date_file[i],12,13)) +
                                     woon_statistics$RankOrder[i]+1)  
  }
  if(as.numeric(substr(woon_statistics$date_file[i],14,15) <= 31)) {
    woon_statistics$miniute[i] <- ((as.numeric(substr(woon_statistics$date_file[i],10,11))*60) +
                                     as.numeric(substr(woon_statistics$date_file[i],12,13)) +
                                     woon_statistics$RankOrder[i])  
  }
  print(i)
}
View(woon_statistics)
woon <- woon_statistics

# generate a sequence of dates
start <-  strptime("20150705", format="%Y%m%d")
finish <- strptime("20160611", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)

statistics_plus <- NULL
statistics_plus$date <- rep(dates, each = 1440)
statistics_plus$minute <- rep(0:1439, length(dates))
statistics_plus$ZeroCrossingFractionLeft <- rep(-10, length(rep(dates, each = 1440)))
statistics_plus$ZeroCrossingFractionRight <- rep(-10, length(rep(dates, each = 1440)))
statistics_plus$ChannelSimilarity <- rep(-10, length(rep(dates, each = 1440)))
statistics_plus$ChannelDiffDecibels <- rep(-10, length(rep(dates, each = 1440)))
statistics_plus$ChannelBiasDecibels <- rep(-10, length(rep(dates, each = 1440)))
woon_statistics$date_file <- as.character(woon_statistics$date_file)

for(i in 1:nrow(woon_statistics)) {
  a <- -10
  a <- which((statistics_plus$date==substr(woon_statistics$date_file[i],1,8)) &
               (statistics_plus$minute==woon_statistics$miniute[i]))
  a <- as.numeric(a)
  if(length(a) > 0) {
    statistics_plus$ZeroCrossingFractionLeft[a] <- woon_statistics$ZeroCrossingFractionLeft[i]
    statistics_plus$ZeroCrossingFractionRight[a] <- woon_statistics$ZeroCrossingFractionRight[i]
    statistics_plus$ChannelSimilarity[a] <- woon_statistics$ChannelSimilarity[i]
    statistics_plus$ChannelDiffDecibels[a] <- woon_statistics$ChannelDiffDecibels[i]
    statistics_plus$ChannelBiasDecibels[a] <- woon_statistics$ChannelBiasDecibels[i]
  }
  print(i)
}

# Two tone-plot of Zero-CrossingFractionLeft
colour0 <- "#0072B2"
colour1 <- "#56B4E9"
colour2 <- "#009E73"
colour3 <- "#F0E442"
colour4 <- "#E69F00"
colour5 <- "#D55E00"
colours <- c(colour0,colour1,colour2,colour3,colour4,colour5)
library(scales)
show_col(c(colour0,colour1,colour2,colour3,colour4,colour5))
# The palette with grey:
#cbPalette <- c("#999999", "#E69F00", "#56B4E9", 
#              "#009E73", "#F0E442", "#0072B2", 
#               "#D55E00", "#CC79A7")

# The palette with black:
#cbbPalette <- c("#000000", "#E69F00", "#56B4E9", 
#                "#009E73", "#F0E442", "#0072B2", 
#                "#D55E00", "#CC79A7")
library(scales)
#show_col(cbPalette)
#show_col(cbbPalette)

View(statistics_plus)
statistics_plus <- data.frame(statistics_plus)
statistics_plus$abs_minute <- 1:nrow(statistics_plus)
a <- which(statistics_plus$ZeroCrossingFractionLeft==-10)
statistics_plus$ZeroCrossingFractionLeft[a] <- NA
statistics_plus$ZeroCrossingFractionRight[a] <- NA
statistics_plus$ChannelSimilarity[a] <- NA
statistics_plus$ChannelDiffDecibels[a] <- NA
statistics_plus$ChannelBiasDecibels[a] <- NA
statistics_plus$ZCFL_col <- NA
statistics_plus$ZCFR_col <- NA
statistics_plus$CS_col <- NA
statistics_plus$CDD_col <- NA
statistics_plus$CBD_norm_col <- NA

# normalise the ChannelBiasDecibels between -6 and 6 
# and then scale between 0 and 1
statistics_plus$CBD_norm <- statistics_plus$ChannelBiasDecibels
a <- which(statistics_plus$CBD_norm > 6)
statistics_plus$CBD_norm[a] <- 6
a <- which(statistics_plus$CBD_norm < -6)
statistics_plus$CBD_norm[a] <- -6
# scale between 0 and 1.2
statistics_plus$CBD_norm <- (statistics_plus$CBD_norm+6)/10

max_zero_crossing <- 0.8
max_channel_similarity <- 1
max_channel_diff_decibels <- 8
max_cBD_norm <- 1.2

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_zero_crossing
A <- max/5 # interval
a <- which(statistics_plus$ZeroCrossingFractionLeft < 1*A)
statistics_plus$ZCFL_col[a] <- 1
b <- which(statistics_plus$ZeroCrossingFractionLeft < 2*A)
c <- setdiff(b,a) 
statistics_plus$ZCFL_col[c] <- 2
c <- which(statistics_plus$ZeroCrossingFractionLeft < 3*A)
d <- setdiff(c,b) 
statistics_plus$ZCFL_col[d] <- 3
d <- which(statistics_plus$ZeroCrossingFractionLeft < 4*A)
e <- setdiff(d,c) 
statistics_plus$ZCFL_col[e] <- 4
e <- which(statistics_plus$ZeroCrossingFractionLeft < 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFL_col[f] <- 5
e <- which(statistics_plus$ZeroCrossingFractionLeft >= 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFL_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_zero_crossing
A <- max/5 # interval
a <- which(statistics_plus$ZeroCrossingFractionRight < 1*A)
statistics_plus$ZCFR_col[a] <- 1
b <- which(statistics_plus$ZeroCrossingFractionRight < 2*A)
c <- setdiff(b,a) 
statistics_plus$ZCFR_col[c] <- 2
c <- which(statistics_plus$ZeroCrossingFractionRight < 3*A)
d <- setdiff(c,b) 
statistics_plus$ZCFR_col[d] <- 3
d <- which(statistics_plus$ZeroCrossingFractionRight < 4*A)
e <- setdiff(d,c) 
statistics_plus$ZCFR_col[e] <- 4
e <- which(statistics_plus$ZeroCrossingFractionRight < 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFR_col[f] <- 5
e <- which(statistics_plus$ZeroCrossingFractionRight >= 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFR_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_channel_similarity
A <- max/5 # interval
a <- which(statistics_plus$ChannelSimilarity < 1*A)
statistics_plus$CS_col[a] <- 1
b <- which(statistics_plus$ChannelSimilarity < 2*A)
c <- setdiff(b,a) 
statistics_plus$CS_col[c] <- 2
c <- which(statistics_plus$ChannelSimilarity < 3*A)
d <- setdiff(c,b) 
statistics_plus$CS_col[d] <- 3
d <- which(statistics_plus$ChannelSimilarity < 4*A)
e <- setdiff(d,c) 
statistics_plus$CS_col[e] <- 4
e <- which(statistics_plus$ChannelSimilarity < 5*A)
f <- setdiff(e,d) 
statistics_plus$CS_col[f] <- 5
e <- which(statistics_plus$ChannelSimilarity >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CS_col[f] <- 5

B <- 0   # lower threshold
h <- 2   # height of line
max <- max_channel_diff_decibels
A <- max/5 # interval
a <- which(statistics_plus$ChannelDiffDecibels < 1*A)
statistics_plus$CDD_col[a] <- 1
b <- which(statistics_plus$ChannelDiffDecibels < 2*A)
c <- setdiff(b, a) 
statistics_plus$CDD_col[c] <- 2
c <- which(statistics_plus$ChannelDiffDecibels < 3*A)
d <- setdiff(c, b) 
statistics_plus$CDD_col[d] <- 3
d <- which(statistics_plus$ChannelDiffDecibels < 4*A)
e <- setdiff(d, c) 
statistics_plus$CDD_col[e] <- 4
e <- which(statistics_plus$ChannelDiffDecibels < 5*A)
f <- setdiff(e, d) 
statistics_plus$CDD_col[f] <- 5
e <- which(statistics_plus$ChannelDiffDecibels >= 5*A)
f <- setdiff(e, d) 
statistics_plus$CDD_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_cBD_norm
A <- max/5 # interval
a <- which(statistics_plus$CBD_norm < 1*A)
statistics_plus$CBD_norm_col[a] <- 1
b <- which(statistics_plus$CBD_norm < 2*A)
c <- setdiff(b,a) 
statistics_plus$CBD_norm_col[c] <- 2
c <- which(statistics_plus$CBD_norm < 3*A)
d <- setdiff(c,b) 
statistics_plus$CBD_norm_col[d] <- 3
d <- which(statistics_plus$CBD_norm < 4*A)
e <- setdiff(d,c) 
statistics_plus$CBD_norm_col[e] <- 4
e <- which(statistics_plus$CBD_norm < 5*A)
f <- setdiff(e,d) 
statistics_plus$CBD_norm_col[f] <- 5
e <- which(statistics_plus$CBD_norm >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CBD_norm_col[f] <- 5


#View(statistics_plus)
write.csv(statistics_plus, "C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\data\\Woondum_channel_integrity_final.csv", row.names = F)

statistics_plus <- read.csv("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\data\\Woondum_channel_integrity_final.csv", header=T)
#data <- data.frame(data)
statistics_plus <- data.frame(statistics_plus)

View(statistics_plus)
statistics_plus$abs_minute <- 1:nrow(statistics_plus)
a <- which(statistics_plus$ZeroCrossingFractionLeft==-10)
statistics_plus$ZeroCrossingFractionLeft[a] <- NA
statistics_plus$ZeroCrossingFractionRight[a] <- NA
statistics_plus$ChannelSimilarity[a] <- NA
statistics_plus$ChannelDiffDecibels[a] <- NA
statistics_plus$ChannelBiasDecibels[a] <- NA
statistics_plus$ZCFL_col <- NA
statistics_plus$ZCFR_col <- NA
statistics_plus$CS_col <- NA
statistics_plus$CDD_col <- NA
statistics_plus$CBD_norm_col <- NA

# normalise the ChannelBiasDecibels between -6 and 6 
# and then scale between 0 and 1
statistics_plus$CBD_norm <- statistics_plus$ChannelBiasDecibels
a <- which(statistics_plus$CBD_norm > 6)
statistics_plus$CBD_norm[a] <- 6
a <- which(statistics_plus$CBD_norm < -6)
statistics_plus$CBD_norm[a] <- -6
# scale between 0 and 1.2
statistics_plus$CBD_norm <- (statistics_plus$CBD_norm+6)/10

max_zero_crossing <- 0.8
max_channel_similarity <- 1
max_channel_diff_decibels <- 8
max_cBD_norm <- 1.2

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_zero_crossing
A <- max/5 # interval
a <- which(statistics_plus$ZeroCrossingFractionLeft < 1*A)
statistics_plus$ZCFL_col[a] <- 1
b <- which(statistics_plus$ZeroCrossingFractionLeft < 2*A)
c <- setdiff(b,a) 
statistics_plus$ZCFL_col[c] <- 2
c <- which(statistics_plus$ZeroCrossingFractionLeft < 3*A)
d <- setdiff(c,b) 
statistics_plus$ZCFL_col[d] <- 3
d <- which(statistics_plus$ZeroCrossingFractionLeft < 4*A)
e <- setdiff(d,c) 
statistics_plus$ZCFL_col[e] <- 4
e <- which(statistics_plus$ZeroCrossingFractionLeft < 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFL_col[f] <- 5
e <- which(statistics_plus$ZeroCrossingFractionLeft >= 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFL_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_zero_crossing
A <- max/5 # interval
a <- which(statistics_plus$ZeroCrossingFractionRight < 1*A)
statistics_plus$ZCFR_col[a] <- 1
b <- which(statistics_plus$ZeroCrossingFractionRight < 2*A)
c <- setdiff(b,a) 
statistics_plus$ZCFR_col[c] <- 2
c <- which(statistics_plus$ZeroCrossingFractionRight < 3*A)
d <- setdiff(c,b) 
statistics_plus$ZCFR_col[d] <- 3
d <- which(statistics_plus$ZeroCrossingFractionRight < 4*A)
e <- setdiff(d,c) 
statistics_plus$ZCFR_col[e] <- 4
e <- which(statistics_plus$ZeroCrossingFractionRight < 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFR_col[f] <- 5
e <- which(statistics_plus$ZeroCrossingFractionRight >= 5*A)
f <- setdiff(e,d) 
statistics_plus$ZCFR_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_channel_similarity
A <- max/5 # interval
a <- which(statistics_plus$ChannelSimilarity < 1*A)
statistics_plus$CS_col[a] <- 1
b <- which(statistics_plus$ChannelSimilarity < 2*A)
c <- setdiff(b,a) 
statistics_plus$CS_col[c] <- 2
c <- which(statistics_plus$ChannelSimilarity < 3*A)
d <- setdiff(c,b) 
statistics_plus$CS_col[d] <- 3
d <- which(statistics_plus$ChannelSimilarity < 4*A)
e <- setdiff(d,c) 
statistics_plus$CS_col[e] <- 4
e <- which(statistics_plus$ChannelSimilarity < 5*A)
f <- setdiff(e,d) 
statistics_plus$CS_col[f] <- 5
e <- which(statistics_plus$ChannelSimilarity >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CS_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_channel_diff_decibels
A <- max/5 # interval
a <- which(statistics_plus$ChannelDiffDecibels < 1*A)
statistics_plus$CDD_col[a] <- 1
b <- which(statistics_plus$ChannelDiffDecibels < 2*A)
c <- setdiff(b,a) 
statistics_plus$CDD_col[c] <- 2
c <- which(statistics_plus$ChannelDiffDecibels < 3*A)
d <- setdiff(c,b) 
statistics_plus$CDD_col[d] <- 3
d <- which(statistics_plus$ChannelDiffDecibels < 4*A)
e <- setdiff(d,c) 
statistics_plus$CDD_col[e] <- 4
e <- which(statistics_plus$ChannelDiffDecibels < 5*A)
f <- setdiff(e,d) 
statistics_plus$CDD_col[f] <- 5
e <- which(statistics_plus$ChannelDiffDecibels >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CDD_col[f] <- 5

B <- 0   # lower threshold
h <- 2 # height of line
max <- max_cBD_norm
A <- max/5 # interval
a <- which(statistics_plus$CBD_norm < 1*A)
statistics_plus$CBD_norm_col[a] <- 1
b <- which(statistics_plus$CBD_norm < 2*A)
c <- setdiff(b,a) 
statistics_plus$CBD_norm_col[c] <- 2
c <- which(statistics_plus$CBD_norm < 3*A)
d <- setdiff(c,b) 
statistics_plus$CBD_norm_col[d] <- 3
d <- which(statistics_plus$CBD_norm < 4*A)
e <- setdiff(d,c) 
statistics_plus$CBD_norm_col[e] <- 4
e <- which(statistics_plus$CBD_norm < 5*A)
f <- setdiff(e,d) 
statistics_plus$CBD_norm_col[f] <- 5
e <- which(statistics_plus$CBD_norm >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CBD_norm_col[f] <- 5

#View(statistics_plus)
write.csv(statistics_plus, "C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\data\\Woondum_channel_integrity_final.csv", row.names = F)

statistics_plus <- read.csv("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\data\\Woondum_channel_integrity_final.csv", header = T)

# Add columns containing the heights for the overplotting
# heights of zero-crossing_left
max_zero_crossing <- 0.8
max_channel_similarity <- 1
max_channel_difference <- 1
max_channel_diff_decibels <- 8
max_cBD_norm <- 1.2
B <- 0 
h <- 2
statistics_plus$ZCL_height <- NA
max <- max_zero_crossing
A <- max/5
for(i in 1:length(statistics_plus$ZeroCrossingFractionLeft)) {
  value <- statistics_plus$ZeroCrossingFractionLeft[i]
  height <- (value - (B+A*(statistics_plus$ZCFL_col[i]-1)))*h/A
  statistics_plus$ZCL_height[i] <- height
  print(i)
}

statistics_plus$ZCR_height <- NA
max <- max_zero_crossing
A <- max/5
for(i in 1:length(statistics_plus$ZeroCrossingFractionRight)) {
  value <- statistics_plus$ZeroCrossingFractionRight[i]
  height <- (value - (B+A*(statistics_plus$ZCFR_col[i]-1)))*h/A
  statistics_plus$ZCR_height[i] <- height
  print(i)
}

statistics_plus$CS_height <- NA
max <- max_channel_similarity
A <- max/5
for(i in 1:length(statistics_plus$ChannelSimilarity)) {
  value <- statistics_plus$ChannelSimilarity[i]
  height <- (value - (B+A*(statistics_plus$CS_col[i]-1)))*h/A
  statistics_plus$CS_height[i] <- height
  print(i)
}

# normalise the Channel Decibel difference to 0 and 8
max(statistics_plus$ChannelDiffDecibels, na.rm=T)
statistics_plus$CDD_norm <- NA
statistics_plus$CDD_norm <- statistics_plus$ChannelDiffDecibels
a <- which(statistics_plus$ChannelDiffDecibels > 8)
statistics_plus$CDD_norm[a] <- 8

statistics_plus$CDD_height <- NA
max <- max_channel_diff_decibels
A <- max/5
for(i in 1:length(statistics_plus$ChannelDiffDecibels)) {
  value <- statistics_plus$CDD_norm[i]
  height <- (value - (B+A*(statistics_plus$CDD_col[i]-1)))*h/A
  statistics_plus$CDD_height[i] <- height
  print(i)
}

statistics_plus$CBD_norm_height <- NA
max <- max_cBD_norm
A <- max/5
for(i in 1:length(statistics_plus$ChannelBiasDecibels)) {
  value <- statistics_plus$CBD_norm[i]
  height <- (value - (B+A*(statistics_plus$CBD_norm_col[i]-1)))*h/A
  statistics_plus$CBD_norm_height[i] <- height
  print(i)
}

# calculate the channel difference from the channel similarity
statistics_plus$channel_diff <- 1 - statistics_plus$ChannelSimilarity
statistics_plus$CD_col <- NA
B <- 0   # lower threshold
h <- 2 # height of line
max <- max_channel_difference
A <- max/5 # interval
a <- which(statistics_plus$channel_diff < 1*A)
statistics_plus$CD_col[a] <- 1
b <- which(statistics_plus$channel_diff < 2*A)
c <- setdiff(b,a) 
statistics_plus$CD_col[c] <- 2
c <- which(statistics_plus$channel_diff < 3*A)
d <- setdiff(c,b) 
statistics_plus$CD_col[d] <- 3
d <- which(statistics_plus$channel_diff < 4*A)
e <- setdiff(d,c) 
statistics_plus$CD_col[e] <- 4
e <- which(statistics_plus$channel_diff < 5*A)
f <- setdiff(e,d) 
statistics_plus$CD_col[f] <- 5
e <- which(statistics_plus$channel_diff >= 5*A)
f <- setdiff(e,d) 
statistics_plus$CD_col[f] <- 5

statistics_plus$CD_height <- NA
max <- max_channel_difference
A <- max/5
for(i in 1:length(statistics_plus$channel_diff)) {
  value <- statistics_plus$channel_diff[i]
  height <- (value - (B+A*(statistics_plus$CD_col[i]-1)))*h/A
  statistics_plus$CD_height[i] <- height
  print(i)
}

# gradient of zero-crossing rate
for(i in 1:(nrow(statistics_plus)-1)) {
  statistics_plus$zcrL_grad[i] <- abs(statistics_plus$ZeroCrossingFractionLeft[i+1] -
                                        statistics_plus$ZeroCrossingFractionLeft[i])
  print(i)
}
sL <- summary(statistics_plus$zcrL_grad, rm.na=T)
max(statistics_plus$zcrL_grad, na.rm=T)
bL <- boxplot(statistics_plus$zcrL_grad)
bL$stats
p <- c(5,10,15,20,25, 30,35, 40,45, 50,55, 60,65, 70,75 ,80,85, 90,95)/100
q <- quantile(statistics_plus$zcrL_grad, p, na.rm=T)
q75L <- unname(q[15])

for(i in 1:(nrow(statistics_plus)-1)) {
  statistics_plus$zcrR_grad[i] <- abs(statistics_plus$ZeroCrossingFractionRight[i+1] -
                                        statistics_plus$ZeroCrossingFractionRight[i])
  print(i)
}

# determine the colours for the ZCRL_gradient
# normalise the values to the 3rd quartile q75L
a <- which(statistics_plus$zcrL_grad > q75L)
statistics_plus$zcrL_grad_norm <- statistics_plus$zcrL_grad 
statistics_plus$zcrL_grad_norm[a] <- q75L 
statistics_plus$zcrL_grad_norm_col <- NA
statistics_plus <- data.frame(statistics_plus)

B <- 0   # lower threshold
h <- 2 # height of line
max <- unname(q75L)
A <- max/5 # interval
a <- which(statistics_plus$zcrL_grad_norm < 1*A)
statistics_plus$zcrL_grad_norm_col[a] <- 1
b <- which(statistics_plus$zcrL_grad_norm < 2*A)
c <- setdiff(b,a) 
statistics_plus$zcrL_grad_norm_col[c] <- 2
c <- which(statistics_plus$zcrL_grad_norm < 3*A)
d <- setdiff(c,b) 
statistics_plus$zcrL_grad_norm_col[d] <- 3
d <- which(statistics_plus$zcrL_grad_norm < 4*A)
e <- setdiff(d,c) 
statistics_plus$zcrL_grad_norm_col[e] <- 4
e <- which(statistics_plus$zcrL_grad_norm < 5*A)
f <- setdiff(e,d) 
statistics_plus$zcrL_grad_norm_col[f] <- 5
e <- which(statistics_plus$zcrL_grad_norm >= 5*A)
f <- setdiff(e,d) 
statistics_plus$zcrL_grad_norm_col[f] <- 5

statistics_plus$zcr_grad_norm_height <- NA
max <- q75L
A <- max/5
for(i in 1:length(statistics_plus$zcrL_grad_norm)) {
  value <- statistics_plus$zcrL_grad_norm[i]
  height <- (value - (B+A*(statistics_plus$zcrL_grad_norm_col[i]-1)))*h/A
  statistics_plus$zcr_grad_norm_height[i] <- height
  print(i)
}

sR <- summary(statistics_plus$zcrR_grad, rm.na=T)
bR <- boxplot(statistics_plus$zcrR_grad)
bR$stats[4] # hinge
sR[5]
p <- c(5,10,15,20,25, 30,35, 40,45, 50,55, 60,65, 70,75 ,80,85, 90,95)/100
q <- quantile(statistics_plus$zcrR_grad, p, na.rm=T)
q75R <- unname(q[15])

# determine the colours for the ZCRR_gradient
# normalise the values to the 3rd quartile q75R
a <- which(statistics_plus$zcrR_grad > q75R)
statistics_plus$zcrR_grad_norm <- statistics_plus$zcrR_grad 
statistics_plus$zcrR_grad_norm[a] <- q75R 
statistics_plus$zcrR_grad_norm_col <- NA
statistics_plus <- data.frame(statistics_plus)

B <- 0   # lower threshold
h <- 2 # height of line
max <- unname(q75R)
A <- max/5 # interval
a <- which(statistics_plus$zcrR_grad_norm < 1*A)
statistics_plus$zcrR_grad_norm_col[a] <- 1
b <- which(statistics_plus$zcrR_grad_norm < 2*A)
c <- setdiff(b,a) 
statistics_plus$zcrR_grad_norm_col[c] <- 2
c <- which(statistics_plus$zcrR_grad_norm < 3*A)
d <- setdiff(c,b) 
statistics_plus$zcrR_grad_norm_col[d] <- 3
d <- which(statistics_plus$zcrR_grad_norm < 4*A)
e <- setdiff(d,c) 
statistics_plus$zcrR_grad_norm_col[e] <- 4
e <- which(statistics_plus$zcrR_grad_norm < 5*A)
f <- setdiff(e,d) 
statistics_plus$zcrR_grad_norm_col[f] <- 5
e <- which(statistics_plus$zcrR_grad_norm >= 5*A)
f <- setdiff(e,d) 
statistics_plus$zcrR_grad_norm_col[f] <- 5

statistics_plus$zcrR_grad_norm_height <- NA
max <- q75R
A <- max/5
for(i in 1:length(statistics_plus$zcrR_grad_norm)) {
  value <- statistics_plus$zcrR_grad_norm[i]
  height <- (value - (B+A*(statistics_plus$zcrR_grad_norm_col[i]-1)))*h/A
  statistics_plus$zcrR_grad_norm_height[i] <- height
  print(i)
}

write.csv(statistics_plus, "C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\data\\Woondum_channel_integrity_final.csv"
          , row.names = F)

#Empty plot
dev.off()
colour0 <- "#0072B2"
colour1 <- "#56B4E9"
colour2 <- "#009E73"
colour3 <- "#F0E442"
colour4 <- "#E69F00"
colour5 <- "#D55E00"
colours <- c(colour0,colour1,colour2,colour3,colour4,colour5)
library(scales)
show_col(c(colour0,colour1,colour2,colour3,colour4,colour5))
dates
# generate a sequence of dates
start <-  strptime("20150705", format="%Y%m%d")
finish <- strptime("20160611", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)

first_of_month <- dates[which(substr(dates,7,8)=="01")]

first_minute_of_month <- NULL
first_date <- as.numeric(substr(statistics_plus$date[1],7,8))
for(i in 1:length(first_of_month)) {
  a <- which(statistics_plus$date==first_of_month[i])
  a <- a[1]
  first_minute_of_month <- c(first_minute_of_month, a)
}
list <- c(1, first_minute_of_month, length(statistics_plus$date))
start <-   152640            #164161
end <- 152640+44640          #172800
# reduce to the first 7 months
list <- list[1:8]

dev.off()
tiff("WoondumNP_channel_integrity_Channel_decibel_difference.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.8, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Channel Decibel Difference", cex=0.7)
    mtext(side = 3, line=1, "Woondum National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$CDD_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$CDD_height[j], 
             col = colours[statistics_plus$CDD_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

dev.off()
tiff("WoondumNP_channel_integrity_zero_crossing_rate_left.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.8, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Zero Crossing Rate Left", cex=0.7)
    mtext(side = 3, line=1, "Woondum National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$ZCFL_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$ZCL_height[j], 
             col = colours[statistics_plus$ZCFL_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()


dev.off()
tiff("WoondumNP_channel_integrity_ZCL_gradient.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.8, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Zero Crossing Rate Gradient Left", cex=0.7)
    mtext(side = 3, line=1, "Woondum National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$zcrL_grad_norm_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$zcr_grad_norm_height[j], 
             col = colours[statistics_plus$zcrL_grad_norm_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

dev.off()
tiff("WoondumNP_channel_integrity_ZCR_gradient.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(0.8, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Zero Crossing Rate Gradient Right", cex=0.7)
    mtext(side = 3, line=1, "Woondum National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$zcrR_grad_norm_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$zcrR_grad_norm_height[j], 
             col = colours[statistics_plus$zcrR_grad_norm_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

dev.off()
tiff("WoondumNP_channel_integrity_Channel_bias.tiff",
     height=1200, width=1800, res=300)
par(mfrow=c((length(list)-1),1), 
    mar=c(1, 0, 0.2, 0), 
    oma=c(0.5, 0, 2.2, 2.2))
for(i in 1:(length(list)-1)) {
  start <-   list[i]
  end <- list[i+1]-1
  if(i==(length(list)-1)) {
    end <- list[i+1]
  }
  if(i==1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="") 
    mtext(side = 3, line=0, "Channel bias", cex=0.7)
    mtext(side = 3, line=1, "Woondum National Park")
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, line = -1.6, las=1, cex=1,
            paste("July ", 
                  substr(statistics_plus$date[start], 
                         1,4), sep=""))
    }
  }
  if(i > 1) {
    plot(c(start,(start+31*1440)), c(1,2), type = "n", 
         axes=FALSE, 
         frame.plot=FALSE,
         xlab="", ylab="",
         main="") 
    if(substr(statistics_plus$date[start],5,6)=="07") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("July ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="08") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Aug ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="09") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Sept ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="10") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Oct ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="11") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Nov ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="12") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Dec ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="01") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jan ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="02") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Feb ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="03") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Mar ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="04") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Apr ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="05") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("May ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
    if(substr(statistics_plus$date[start],5,6)=="06") {
      mtext(side = 4, las=1, cex=1, line=-1.6,
            paste("Jun ", substr(statistics_plus$date[start], 1,4), sep=""))  
    }
  } 
  
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = 2, 
             col = colours[statistics_plus$CBD_norm_col[j]])
    print(j)
  }
  for(j in start:end) {
    segments(x0 = j, 
             y0 = 0, 
             x1 = j, 
             y1 = statistics_plus$CBD_norm_height[j], 
             col = colours[statistics_plus$CBD_norm_col[j]+1])
    print(j)
  }
  if(i < (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:(end+1)])
    all_dates <- uniq
    
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
  if(i == (length(list)-1)) {
    uniq <- unique(statistics_plus$date[start:end])
    all_dates <- uniq
    for(j in 1:length(all_dates)) {
      a <- which(statistics_plus$date==all_dates[j])
      a <- a[1]
      axis(side = 1, line=-0.2, at = a, outer = F,
           labels = substr(statistics_plus$date[a],7,8),
           mgp=c(1,0.3,0))
      abline(v=a)
    }
  }
}
dev.off()

# use ccf to find the lag between the rain and the number of minutes with a 
# decibel difference > 4.53 dB
integrity <- read.csv("C:\\Work2\\Channel integrity\\Gympie_channel_integrity.csv", header=T)
# remove the first day and all dates after the 6th January 2016
integrity <- integrity[1441:nrow(integrity),]
a <- which(integrity$date=="20160107")
integrity <- integrity[c(-(a[1]:length(integrity$date))),]

start <-  strptime("20150706", format="%Y%m%d")
finish <- strptime("20160106", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)

k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)

cluster_list$rain <- 0
rain <- c(2,10,17,18,21,54,59)
a <- which(cluster_list$cluster_list==2)
cluster_list$rain[a] <- 0
a <- which(cluster_list$cluster_list==10)
cluster_list$rain[a] <- 2
a <- which(cluster_list$cluster_list==17)
cluster_list$rain[a] <- 0
a <- which(cluster_list$cluster_list==18)
cluster_list$rain[a] <- 2
a <- which(cluster_list$cluster_list==21)
cluster_list$rain[a] <- 0
a <- which(cluster_list$cluster_list==54)
cluster_list$rain[a] <- 2
a <- which(cluster_list$cluster_list==59)
cluster_list$rain[a] <- 2
# reduce cluster list to 20150706 to 20160106
cluster_list <- cluster_list[(14*1440+1):((14*1440+1)+(length(dates)*1440)-1),]

count_of_rain <- NULL
for(i in 1:length(dates)) {
  cluster_list_temp <- cluster_list[((i-1)*1440+1):(i*1440),]
  a <- which(cluster_list_temp$rain==2)
  count_of_rain <- c(count_of_rain, length(a))
}
count_of_4_54_dB <- NULL
for(i in 1:length(dates)) {
  a <- which(integrity$date==dates[i])
  integrity_temp <- integrity[a,]
  a <- which(integrity_temp$ChannelDiffDecibels >= 4.53)
  count_of_4_54_dB <- c(count_of_4_54_dB, length(a))
  print(i)
}
# remove the 28, 29 and 30 October 2015 because of microphone failure
a <- which(dates=="20151028")
count_of_4_54_dB <- count_of_4_54_dB[1:114:118:length(count_of_4_54_dB)]
count_of_rain <- count_of_rain[1:114:118:length(count_of_rain)]
  
c <- ccf(x = count_of_4_54_dB, y = count_of_rain, plot = F)
c <- ccf(x = count_of_4_54_dB, y = count_of_rain, plot = T)

r_db <- ccf(x = count_of_4_54_dB, y = count_of_rain, 
            plot = T, xlim=c(-10,10), xlab="")
mtext("Lag(days)", side=1, line=1.4, cex=1.7)
mtext(side=3,line=0, cex=1.7,
      "Cross-correlation between rain and a decibel difference > 4.53 dB")

max.lmc <- max(r_db$acf)
lm$lag[which(r_db$acf > max.lmc-0.01 & r_db$acf < max.lmc+0.10)]
