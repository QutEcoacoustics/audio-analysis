# 6 May 2016
# This file evaluates the similarity of the left and right channels (using the simspec
# function in the seewave package) in minute segments and plots the result.  
# If the similarity consistently falls below 20 for a period of time 
# there is most likely a problem with one of the microphones.  It also
# plots the zero crossing rate, a high flat line value indicates a 
# problem with that channel.

library(tuneR)
library(seewave)
#setwd("C:\\Work")

sampling_rate <- 22050

sourceDir <- "E:\\Cooloola\\20151025\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20160131\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\20160131\\GympieNP\\"
sourceDir <- "G:\\Data\\"
sourceDir <- "D:\\Cooloola\\20160207\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20160306\\WoondumNP\\"
sourceDir <- "D:\\Cooloola\\20151229\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\20151220\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20151220\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\20151229\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20151229\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\2015_09_27\\GympieNP\\"
setwd(paste(substr(sourceDir,1,31),sep=""))
setwd(paste(sourceDir))
# Obtain a list of the original wave files
myFiles <- list.files(full.names=TRUE, pattern="*.wav$", path=sourceDir)
myFiles
myFilesShort <- list.files(full.names=FALSE, pattern="*.wav$", path=sourceDir)
myFilesShort

site <- substr(sourceDir, 22, 25)
site <- "Gymp"
for (n in 1:length(myFilesShort)) {
  lgth <- file.size(myFiles[n])/(sampling_rate*4)
  seqA <- seq(0, lgth, 60)
  similarity <- NULL
  zro_crs_left <- NULL
  zro_crs_right <- NULL
  
  for(i in 2:length(seqA)) {  
    wave2 <- readWave(myFiles[n], from = seqA[i-1], to = seqA[i], 
                      units = "seconds")
    #play(wave1)
    # prepare separate left and right channel objects
    wave_left <- mono(wave2, "left")
    wave_right <- mono(wave2, "right")
    
    # generate a frequency spectrum 
    spec_left <- spec(wave=wave_left, f=sampling_rate, wl=512, 
                  plot = FALSE)
    spec_right <- spec(wave=wave_right, f=sampling_rate, wl=512, 
                  plot = FALSE)
    
    sim <- simspec(spec_left, spec_right)
    similarity <- c(similarity, sim)
    
    # zero crossing rate
    z_c_left <- zcr(wave_left, wl = NULL)
    z_c_right <- zcr(wave_right, wl = NULL)
    
    zro_crs_left <- c(zro_crs_left, z_c_left)
    zro_crs_right <- c(zro_crs_right, z_c_right)
  }
  combined <- cbind(similarity, zro_crs_left, zro_crs_right)
  
  write.csv(combined, paste(site, "_",  
              substr(myFilesShort[n], 1,15), 
              "_simspec.csv", sep=""), 
              row.names = F)

  png(paste("file_", site,"_", substr(myFilesShort[n], 1,15), 
            ".png", sep = ""), width=1000, height=500)
  par(mar=c(3,4,2,5))
  plot(similarity, type="l", ylim=c(0, 65), axes=FALSE, 
       xlab = "", ylab = "", bty = "n", lwd = 1.3,
       main=paste(site, "_", substr(myFilesShort[n], 1,15)))
  abline(h = 20, col = "red", lw = 0.5)
  axis(2, ylim=c(0,65), col="black", las=1)
  mtext("Similarity (%)", side=2, line=2.5)
  box()
  
  # plot the left channel zero crossing rate
  par(new=TRUE)
  plot(zro_crs_left, xlab="", ylab="", type="o",
       ylim=c(0, 0.65), lwd = 1.3, pch=16, cex=0.2,
       axes=FALSE, col="blue", lty=1)
  mtext("Zero crossing rate", side = 4, line = 3.6)
  axis(4, ylim=c(0, 0.65), las=1, line = 0)
  
  par(new=TRUE)
  # plot the right channel zero crossing rate
  plot(zro_crs_right, xlab="", ylab="", type="o",
       ylim=c(0, 0.65), lwd = 1.3, pch= 16, cex=0.2,
       axes=FALSE, col="darkgreen", lty=1)
  
  # plot the time axis
  axis(1, at = pretty(1:floor(lgth/60),20), 
       pretty(1:floor(lgth/60),20))
  mtext("Time (minutes)", side=1, line = 2)
  
  legend("bottomleft", legend=c("LR Similarity", "ZCR - left", "ZCR - right"),
         text.col = c("black", "blue", "darkgreen"), 
         bg = "white", bty = "n",
         lty = c(1,1,1), col = c("black", "blue", "darkgreen"))
  dev.off()
}
