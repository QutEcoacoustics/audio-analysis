#conbime days

folder <- "C:\\Work\\myfile\\SERF_callCount_20sites_fulllist\\training"
pattern <- "hitmaps*"
filename <- list.files(folder, pattern, no..=TRUE)
container <- data.frame()

for(i in 1:length(filename)){
  species <- read.csv(paste(folder, "\\", filename[i], sep=""), check.names=FALSE)
  container <- rbind(container, species)
}

write.table(container, file='c:/work/myfile/SERF_callCount_20sites_fulllist/training.csv', sep=',',row.names=FALSE)