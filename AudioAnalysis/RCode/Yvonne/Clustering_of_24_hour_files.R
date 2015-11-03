## 9 October 2015
# This code takes the 24 hour fingerprints and clusters these using
# hclust into 12 clusters (corresponding to 12 days)
# This code was set up for Experiment 2 (publication)
# The files that this code clusters is generated in the code 
# Histograms_of_cluster.lists.R

setwd("C:\\Work\\CSV files\\DataSet_Exp2\\24hourFilesA\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hybrid\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp3a\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2a\\Kmeans\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2_new_new\\Hybrid\\")
setwd("C:\\Work\\CSV files\\DataSet_Exp2_new_new\\Hierarchical\\")
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16")
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_9_10_11_15_16")

myFiles <- list.files(full.names=TRUE, pattern="*_24hour.csv$")
myFilesShort <- list.files(full.names=FALSE, pattern="*_24hour.csv$")

length <- length(myFiles)
length
site <- c(rep("GympieNP",6), rep("WoondumNP",6))
dates <- unique(indices$rec.date)
#dates2 <- rep(dates, each=6)
dates <- rep(dates, 2)
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
  hc.fit <- hclust(dist(dat), method = "ward.D2")
  png(paste(myFilesShort[i],"Method wardD2.png", sep = ""), width=1000,
      height =1000)
  par(oma=c(7,3,3,3))
  plot(hc.fit, cex=2, main = paste(myFilesShort[i]), sub="", xlab = "hclust(method = ward.D2)",
       xaxt="n", yaxt = "n")
  heightss <- hc.fit$height
  axis(side = 4, at=c(round(heightss[2],0),round(heightss[4],0),round(heightss[6],0),
                      round(heightss[8],0),round(heightss[10],0)), las=1, cex=1.2)
  axis(side = 2, at=c(round(heightss[1],0),round(heightss[3],0),round(heightss[5],0),
                      round(heightss[7],0),round(heightss[9],0),round(heightss[11],0)), 
                      las=1, cex=1.2)
  mtext(side = 1, line = 5.5, adj=1, cex=1.1, paste("1,2,3", site[1], dates[1], 
                                                  dates[2], dates[3], "4,5,6", site[1], dates[4], 
                                                  dates[5], dates[6], sep = "    ")) 
  mtext(side = 1, line = 7, adj=1, cex=1.1, paste("7,8,9", site[7], dates[1], 
                                                  dates[2], dates[3], "10,11,12", site[7], dates[4], 
                                                  dates[5], dates[6], sep = "    "))
  mtext(side = 1, line = 8.5, adj=1, cex=1.1, "**Twelve days from 2 x 121 days of clustering. Indices:  BGN, SNR, EventsPerSec, LowFreqC, AcoComp, EPKS, ECOVS")
  mtext(paste("heights: ",round(heightss[11],0),round(heightss[10],0),round(heightss[9],0),
              round(heightss[8],0),round(heightss[7],0), round(heightss[6],0),
              round(heightss[5],0), round(heightss[4],0),round(heightss[3],0),
              round(heightss[2],0), round(heightss[1],0), sep = ", "))
  heights <- rbind(heights, heightss)
  dev.off()
}

heights <- cbind(myFilesShort, heights)
write.csv(heights, "heights_test_ward_D2.csv")
sort(unique(cophenetic(hc.fit)))
