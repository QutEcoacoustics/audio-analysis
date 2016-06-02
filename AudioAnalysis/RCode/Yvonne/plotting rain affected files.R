# what if I subtracted the sd from the similarity?
# smooth the data with filter
# data_wfilter <- filter(data_w_28, filter= rep(1/3,3))

sourceDir <- "D:\\Cooloola\\"
setwd(paste("D:\\Statistics\\"))
# Obtain a list of the original wave files
myFiles <- list.files(full.names=TRUE, pattern="*.csv$", 
                      path=sourceDir, recursive=T)
myFiles

myFilesShort <- list.files(full.names=FALSE, pattern="*.csv$", 
                           path=sourceDir, recursive = T)
myFilesShort
library(stats)

statistics <- matrix(data="NA", ncol = 5, nrow = length(myFiles))

for(i in 1:length(myFiles)) {
  name <- substr(myFilesShort[i], 21, 40)
  file <- read.csv(myFiles[i])
  sim_mean <- mean(file[,1])
  sim_sd <- sd(file[,1])
  zcr_l_mean <- mean(file[,2])
  zcr_r_mean <- mean(file[,3])
  statistics[i,] <- c(name, sim_mean, zcr_l_mean, 
                      zcr_r_mean, sim_sd) 
}

name <- statistics[,1]
sim_mean <- statistics[,2]
zcr_l <- statistics[,3]
zcr_r <- statistics[,4]
sim_sd <- statistics[,5]
statistics <- cbind(name, sim_mean, zcr_l, zcr_r, sim_sd)
#(o <- order(name, sim_mean, zcr_l, zcr_r))
#statistics_ordered <-  statistics[o, ]
write.csv(statistics, "statistics.csv")
##############################

gymp <- which(substr(statistics[,1], 1,4) == "Gymp")
statistics_gymp <- statistics[gymp,]

labels <- c(statistics_gymp[,1])
length_gym <- length(statistics_gymp[,1])
#at <- 1:length
#sequence <- seq(1, length, 4)
#at <- at[sequence]

minutes <- NULL
for (i in 1:length(labels)) {
  min <- substr(labels[i],15,18)
  minut <- as.numeric(substr(min, 1, 2))*60 + as.numeric(substr(min, 3,4))
  minutes <- c(minutes, minut)
}
which_midnight <- which(minutes==0)
minutes1 <- minutes
count <- 1440
day <- 1
for(i in 2:length(minutes)-1) {
  if(minutes[i+1] > minutes[i]) {
    minutes1[i] <- count*(day-1) + minutes[i]
  }
  if(minutes[i+1] < minutes[i]) {
    minutes1[i] <- count*(day-1) + minutes[i]
    day <- day + 1
  }
}

at <- minutes1
length_gym
sequence_gym <- seq(1, length_gym, 80)
sequence_gym <- c(sequence_gym, length_gym)

for(i in 1:(length(sequence_gym)-1)) {
  i=i
  start <- sequence_gym[i]
  finish <- sequence_gym[i+1]  
  name <- paste(statistics_gymp[start],"_plot.png", sep = "")
  png(name, height = 1000, width = 2200)
  par(oma=c(20,3,6,2), mar=c(0,1,0,1), mfrow=c(4,1), 
      cex.axis=2)
  x <- minutes1
  plot(x = x[start:finish], y = statistics_gymp[start:finish,2], type = "l",
       xlab = "", ylab = "", xaxt="n", ylim = c(10,70))
  abline(v=minutes1[which_midnight], col="red", lty=2)
  mtext(side=3, line= 1, paste(paste(statistics_gymp[start,1]), " to ", 
                               paste(statistics_gymp[finish,1])), 
        cex = 3)
  abline(h=40, col="red")
  
  x <- minutes1
  plot(x = x[start:finish], y = statistics_gymp[start:finish,5], 
       type = "l", xlab = "", ylab = "", xaxt="n", 
       ylim = c(0,20))
  abline(v=minutes1[which_midnight], col="red", lty=2)
  abline(h=14, col="red")
  
  #par(new=TRUE)
  x <- minutes1
  plot(x = x[start:finish], y = statistics_gymp[start:finish,3], type = "l",
       xlab = "", ylab = "", xaxt="n", lwd=0.2,
       ylim = c(0.07,0.5))
  abline(v=minutes1[which_midnight], col="red", lty=2)
  abline(h=0.18, col="red")
  #par(new=TRUE)
  plot(x = x[start:finish], y = statistics_gymp[start:finish,4], type = "l",
       xlab = "", ylab = "", xaxt="n", lwd=0.2,
       ylim = c(0.07,0.5))
  abline(v=minutes1[which_midnight], col="red", lty=2)
  abline(v=minutes1[which_midnight], col="red", lty=2)
  abline(h=0.18, col="blue")
  axis(side=1, at=minutes1[start:finish], labels= statistics_gymp[start:finish,1], 
       las=2, cex.axis=2, outer = TRUE)
  dev.off()
}

# new code for Woondum
woon <- which(substr(statistics[,1], 1,4) == "Woon")
statistics_woon <- statistics[woon,]

labels <- c(statistics_woon[,1])
length_woon <- length(statistics_woon[,1])
#at <- 1:length
#sequence <- seq(1, length, 4)
#at <- at[sequence]

minutes <- NULL
for (i in 1:length(labels)) {
  min <- substr(labels[i],15,18)
  minut <- as.numeric(substr(min, 1, 2))*60 + as.numeric(substr(min, 3,4))
  minutes <- c(minutes, minut)
}
which_midnight <- which(minutes==0)
minutes1 <- minutes
count <- 1440
day <- 1
for(i in 2:length(minutes)-1) {
  if(minutes[i+1] > minutes[i]) {
    minutes1[i] <- count*(day-1) + minutes[i]
  }
  if(minutes[i+1] < minutes[i]) {
    minutes1[i] <- count*(day-1) + minutes[i]
    day <- day + 1
  }
}

at <- minutes1
length_woon
sequence_woon <- seq(1, length_woon, 80)
sequence_woon <- c(sequence_woon, length_woon)
mean_of_sd <- mean(as.numeric(statistics_woon[start:finish,5]))

for(i in 1:(length(sequence_woon)-1)) {
  i=i
  start <- sequence_woon[i]
  finish <- sequence_woon[i+1]  
  name <- paste(statistics_woon[start],"_plot.png", sep = "")
  png(name, height = 1000, width = 2200)
  par(oma=c(20,3,6,2), mar=c(0,1,0,1), mfrow=c(4,1), 
      cex.axis=2)
  x <- minutes1
  plot(x = x[start:finish], y = statistics_woon[start:finish,2], type = "l",
       xlab = "", ylab = "", xaxt="n", ylim = c(10,70))
  abline(v=minutes1[which_midnight], col="red", lty=2)
  mtext(side=3, line= 1, paste(paste(statistics_woon[start,1]), " to ", 
                               paste(statistics_woon[finish,1])), cex = 3)
  abline(h=40, col="red")
  x <- minutes1
  plot(x = x[start:finish], y = statistics_woon[start:finish,5], 
       type = "l", xlab = "", ylab = "", xaxt="n", 
       ylim = c(0,20))
  abline(v=minutes1[which_midnight], col="red", lty=2)
  
  abline(h=mean_of_sd, col="red")
  
  #par(new=TRUE)
  x <- minutes1
  plot(x = x[start:finish], y = statistics_woon[start:finish,3], type = "l",
       xlab = "", ylab = "", xaxt="n", lwd=0.2,
       ylim = c(0.07,0.5))
  abline(v=minutes1[which_midnight], col="red", lty=2)
  abline(h=0.18, col="red")
  #par(new=TRUE)
  plot(x = x[start:finish], y = statistics_woon[start:finish,4], type = "l",
       xlab = "", ylab = "", xaxt="n", lwd=0.2,
       ylim = c(0.07,0.5))
  abline(v=minutes1[which_midnight], col="red", lty=2)
  abline(v=minutes1[which_midnight], col="red", lty=2)
  abline(h=0.18, col="blue")
  axis(side=1, at=minutes1[start:finish], labels= statistics_woon[start:finish,1], 
       las=2, cex.axis=2, outer = TRUE)
  dev.off()
}

# old code for plots
woon <- which(substr(statistics[,1], 1,4) == "Woon")
statistics_woon <- statistics[woon,]
length <- length(statistics_woon[,1])
labels <- c(statistics_woon[,1])
at <- 1:length
sequence <- seq(1, length, 4)
at <- at[sequence]
labels <- labels[sequence]
png("Woondum_plot.png", height = 1000, width = 2000)
par(oma=c(20,3,6,2), mar=c(0,1,0,1), mfrow=c(3,1), 
    cex.axis=2)
plot(statistics_woon[1:length,2], type = "l",
     xlab = "", ylab = "", xaxt="n")
mtext(side=3, line= 1, paste(paste(statistics_woon[1,1]), " to ", 
                             paste(statistics_woon[length,1])), cex = 3)
abline(h=40, col="red")

#par(new=TRUE)
plot(statistics_woon[1:length,3], type = "l",
     xlab = "", ylab = "", xaxt="n", lwd=0.2,
     ylim = c(0.1,0.3))
abline(h=0.18, col="red")
#par(new=TRUE)
plot(statistics_woon[1:length,4], type = "l",
     xlab = "", ylab = "", xaxt="n", lwd=0.2,
     ylim = c(0.1,0.3))
abline(h=0.18, col="blue")
axis(side=1, at=at[start:finish], labels= labels[1:120], las=2, cex.axis=2,
     outer = TRUE)
dev.off()

data_w_28 <- read.csv("C:\\Work\\Woondum_20150628.csv")
