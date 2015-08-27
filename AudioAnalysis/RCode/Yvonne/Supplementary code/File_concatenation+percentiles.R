# Concatenating csv files

setwd("C:\\Work\\CSV files\\GympieNP1_new\\all_data")

folder <- "C:\\Work\\CSV files\\GympieNP1_new\\"

myFolders <- list.files(full.names=FALSE, pattern="2015_*", path=folder)
myFolders
length <- length(myFolders)
length

all.indices <- NULL

for (i in 1:length) {
  pathName <- (paste(folder, myFolders[i], sep=""))
  myFile <- list.files(full.names=TRUE, pattern="Towsey_Summary_Indices*", 
                       path=pathName)
  assign(paste("fileContents"), read.csv(myFile, header=T))
  all.indices <- rbind(all.indices, fileContents)
}

write.csv(all.indices,
          file=paste("Towsey_Summary_Indices_", site,
                     sub("*.wav","\\1", myFiles[1]),
                     "to current.csv", 
                     sep = ""))
# plot boxplots
boxplot(all.indices$AvgSignalAmplitude, horizontal = T)

quantiles1 <- NULL
quantiles2 <- NULL
minimum <- NULL
average <- NULL
maximum <- NULL

for (i in 4:20) {
  quant1 <- unname(quantile(all.indices[,i], c(2,5)/100))
  quant2 <- unname(quantile(all.indices[,i], c(95,98)/100))
  quantiles1 <- rbind(quantiles1, quant1)
  quantiles2 <- rbind(quantiles2, quant2)
  minim <- min(all.indices[,i])
  minimum <- c(minimum, minim)
  avg <- mean(all.indices[,i])
  average <- c(average, avg)
  maxim <- max(all.indices[,i])
  maximum <- c(maximum, maxim)
}

colNames <- colnames(all.indices[,4:20])
quantiles <- cbind(minimum, quantiles1, average, quantiles2, maximum, colNames)
colnames(quantiles) <- c("min","2%","5%","mean","95%","98%","max","index")

write.csv(quantiles, row.names = FALSE,
          file=paste("Percentiles_", site,
                     sub("*.wav","\\1", myFiles[1]),
                     "to current.csv", 
                     sep = ""))

View(quantiles)