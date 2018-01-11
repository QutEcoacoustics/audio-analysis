# Bioacoustic Index
library(soundecology)
library(tuneR)
library(seewave)

sampling_rate <- 22050

sourceDir <- "E:\\Cooloola\\2016_04_17\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_06_21\\GympieNP"
sourceDir <- "D:\\Cooloola\\2015_07_05\\GympieNP"
sourceDir <- "D:\\Cooloola\\2015_07_12\\GympieNP"
sourceDir <- "D:\\Cooloola\\2015_07_19\\GympieNP"
sourceDir <- "D:\\Cooloola\\2015_07_26\\GympieNP"
sourceDir <- "D:\\Cooloola\\2015_08_02\\GympieNP"
sourceDir <- "D:\\Cooloola\\2015_08_09\\GympieNP" #3
sourceDir <- "D:\\Cooloola\\2015_08_16\\GympieNP" #6
sourceDir <- "D:\\Cooloola\\2015_08_23\\GympieNP" #9
sourceDir <- "D:\\Cooloola\\2015_08_30\\GympieNP" #12
sourceDir <- "D:\\Cooloola\\2015_09_06\\GympieNP" #15
sourceDir <- "D:\\Cooloola\\2015_09_13\\GympieNP" #18
sourceDir <- "D:\\Cooloola\\2015_09_20\\GympieNP" #21
sourceDir <- "D:\\Cooloola\\2015_09_27\\GympieNP" #24
sourceDir <- "D:\\Cooloola\\2015_10_04\\GympieNP" #3
sourceDir <- "D:\\Cooloola\\2015_10_11\\GympieNP" #6
sourceDir <- "F:\\Cooloola\\2015_10_25\\GympieNP" #9
sourceDir <- "F:\\Cooloola\\2015_11_01\\GympieNP" #12
sourceDir <- "F:\\Cooloola\\2015_11_08\\GympieNP" #15
sourceDir <- "F:\\Cooloola\\2015_11_15\\GympieNP" #18
sourceDir <- "F:\\Cooloola\\2015_11_22\\GympieNP" #21


#"F:\Cooloola\2015_10_25\GympieNP"
site <- substr(sourceDir, 24, 31)
site

# Obtain a list of the original wave files
myFiles <- list.files(full.names=TRUE, pattern="*.wav$", path=sourceDir)
myFiles
myFilesShort <- list.files(full.names=FALSE, pattern="*.wav$", path=sourceDir)
myFilesShort

setwd(paste(sourceDir))
for (n in 1:length(myFilesShort)) {
  n=n
  lgth <- file.size(myFiles[n])/(sampling_rate*4)
  seqA <- seq(0, lgth, 60)
  bio_index_left <- NULL
  bio_index_right <- NULL
  
  for(i in 2:length(seqA)) {
    i=i
    wave2 <- readWave(myFiles[n], from = seqA[i-1], 
                      to = seqA[i], 
                      units = "seconds")
    # prepare separate left channel objects
    wave_left <- mono(wave2, "left")
    bioindex_L <- unname(bioacoustic_index(wave_left)[1])
    bio_index_left <- c(bio_index_left, bioindex_L)
    # prepare separate right channel objects
    wave_right <- mono(wave2, "right")
    bioindex_R <- unname(bioacoustic_index(wave_right)[1])
    bio_index_right <- c(bio_index_right, bioindex_R)
    print(i)
  }
  print(n)
  combined <- cbind(bio_index_left[1:length(bio_index_right)], 
                    bio_index_right[1:length(bio_index_right)]) 
  # save the statistics
  colnames(combined) <- c("bio_index_left", "bio_index_right")
  file_name <- paste(site, substr(myFilesShort[n], 1,15), "_bio_index.csv", sep="")
  write.csv(x = combined, file = file_name, row.names = F)
  
  dev.off()
  tiff(paste("bio_index",substr(myFilesShort[n], 1,15),".tiff", sep=""), res=300, height=600, width=1200)
  par(mfrow=c(2,1),mar=c(2,2,1,0.2), mgp=c(3,0.2,0), cex=0.6, tcl=0.2)
  combined <- data.frame(combined)
  ylim=c(0,45)
  plot(1:length(combined$bio_index_left),combined$bio_index_left, type="l", 
       main=paste(site, "_", substr(myFilesShort[n], 1,15)),
       col="black", ylim=ylim, xlab="",
       ylab="", las=1, cex=0.6)
  #par(new=TRUE)
  mtext(side=1, "Minute", line=1.2, cex=0.6)
  mtext(side=2, "Bio_index", line=1.3, cex=0.6)
  plot(1:length(combined$bio_index_right),combined$bio_index_right, type="l", 
       main=paste(site, "_", substr(myFilesShort[n], 1,15)),
       col="black", ylim=ylim, xlab="",
       ylab="", las=1, cex=0.6)
  mtext(side=1, "Minute", line=1.2, cex=0.6)
  mtext(side=2, "Bio_index", line=1.3, cex=0.6)
  
  dev.off()
  plot(wave_left, ylim=c(-2000,2000))
  plot(wave_right, ylim=c(-2000,2000))
}