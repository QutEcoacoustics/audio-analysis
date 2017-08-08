# use the order values from hclust to find the consecutive days.
Gym_data <- read.csv("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\Gympie_norm_indices.csv", header=T)

dates <- unique(Gym_data$dates)
num_seq <- seq(1, 1440, by = 20)
columns <- 4:15
statistics <- data.frame(V1=NA)  

for(i in 1:length(dates)) {
  date <- dates[i]
  a <- which(Gym_data$dates==date)
  Gym_data_temp <- Gym_data[a,]
  ref <- 0
  for(j in num_seq) {
    for(k in columns) {
    stats <- mean(Gym_data_temp[(j:(j+20)), k], na.rm=T)  
    ref <- ref + 1
    statistics[ref, i] <- stats
    }
  }
  print(i)
}
statistics <- t(statistics)
dates <- paste(substr(dates,7,8),
                substr(dates,5,6),
                substr(dates,1,4), sep = "-")
rownames(statistics) <- as.character(dates)

require(graphics)

library(stats)
hc <- hclust(dist(statistics[(1:80),]), method = "ward.D2")
plot(hc)

tiff("Dendrogram_Gympie_80days.tiff", height=600, width=1600)
par(cex.main=3, mar=c(0,2,2,0), cex.lab=2, cex.axis=2)
plot(hc, main = "Gympie_day_distances_first_eightly_days", 
     cex=1.5, mgp=c(0.5,-1,-2))
dev.off()

Woon_data <- read.csv("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\Woondum_norm_indices.csv", header=T)

dates <- unique(Woon_data$dates)
num_seq <- seq(1, 1440, by = 20)
columns <- 4:15
statistics <- data.frame(V1=NA)  

for(i in 1:length(dates)) {
  date <- dates[i]
  a <- which(Woon_data$dates==date)
  Woon_data_temp <- Woon_data[a,]
  ref <- 0
  for(j in num_seq) {
    for(k in columns) {
      stats <- mean(Woon_data_temp[(j:(j+20)), k], na.rm=T)  
      ref <- ref + 1
      statistics[ref, i] <- stats
    }
  }
  print(i)
}
statistics <- t(statistics)
dates <- paste(substr(dates,7,8),
               substr(dates,5,6),
               substr(dates,1,4), sep = "-")
rownames(statistics) <- as.character(dates)

require(graphics)

library(stats)
hc <- hclust(dist(statistics[(1:80),]), method = "ward.D2")
plot(hc)

tiff("Dendrogram_Woondum_80days.tiff", height=600, width=1600)
par(cex.main=3, mar=c(0,2,2,0), cex.lab=2, cex.axis=2)
plot(hc, main = "Woondum_day_distances_first_eightly_days", 
     cex=1.5, mgp=c(0.5,-1,-2))
dev.off()
