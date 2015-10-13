## 9 October 2015
# This code takes the 24 hour fingerprints and clusters these using
# hclust into 12 clusters (corresponding to 12 days)
# This code was set up for Experiment 2 (publication)
# The files that this code clusters is generated in the code 
# Histograms_of_cluster.lists.R

setwd("C:\\Work\\CSV files\\DataSet_Exp2\\24hourFilesA\\")

myFiles <- list.files(full.names=TRUE, pattern="*_24hour.csv$",
                      path = "C:\\Work\\CSV files\\DataSet_Exp2\\24hourFilesA\\")
length <- length(myFiles)
myFilesShort <- list.files(full.names=FALSE, pattern="*24hour.csv$",
                           path = "C:\\Work\\CSV files\\DataSet_Exp2\\24hourFilesA\\")

# Read file contents of Summary Indices and collate
numberCol <- NULL
heights <- NULL
for (i in 1:length(myFilesShort)) {
  Name <- myFiles[i]
  assign("fileContents", read.csv(Name))
  numberCol <- ncol(fileContents)
  numberCol <- numberCol-2
  dat <- fileContents[,1:numberCol]
  #c <- cor(dat)
  hc.fit <- hclust(dist(dat), method = "average")
  png(paste(myFilesShort[i],"Method average.png", sep = ""), width=800,
      height =800)
  par(oma=c(7,3,3,3))
  plot(hc.fit, cex=2, main = paste(myFilesShort[i]), sub="",xlab = "hclust(method = average)",xaxt="n")
  mtext(side = 1, line = 6, adj=1, cex=1.1, "1,2,3 Gympie NP 30/07/2015, 31/07/2015, 1/08/2015  4,5,6 Gympie NP 31/08/2015, 1/09/2015, 4/09/2015")
  mtext(side = 1, line = 8, adj=1, cex=1.1, "7,8,9 Woondum NP 30/07/2015, 31/07/2015, 1/08/2015  10,11,12 Woondum NP 31/08/2015, 1/09/2015, 4/09/2015")
  heightss <- hc.fit$height
  heights <- rbind(heights, heightss)
  dev.off()
}

heights <- cbind(myFilesShort, heights)
write.csv(heights, "heights1.csv")
sort(unique(cophenetic(hc.fit)))
