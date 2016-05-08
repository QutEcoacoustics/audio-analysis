# 6 May 2016
# This file evaluates the similarity of the left and right channels (using the simspec
# function in the seewave package) in minute segments and plots the result.  
# If the similarity consistently falls below 20 for a period of time 
# there is most likely a problem with one of the microphones.  It also
# plots the zero crossing rate, a value above 0.5 also indicates a 
# problem with that channel.

library(tuneR)
library(seewave)
#setwd("C:\\Work")

sampling_rate <- 22050

sourceDir <- "F:\\Cooloola\\20151025\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20151101\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20151108\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_10_11\\Gympie NP\\"
sourceDir <- "E:\\Cooloola\\2015_09_13\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\20160501\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\20160501\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\20151220\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20151101\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20151101\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\20151108\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20151108\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\20160501\\Woondum3\\"
#sourceDir <- "E:\\Cooloola\\20160424\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\20160424\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\20160417\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\20160417\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\20160306\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20160306\\WoondumNP\\"
sourceDir <- "E:\\Cooloola\\20160417\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20151101\\GympieNP\\"
setwd(paste(substr(sourceDir,1,31),sep=""))

# Obtain a list of the original wave files
myFiles <- list.files(full.names=TRUE, pattern="*.wav$", path=sourceDir)
myFiles
myFilesShort <- list.files(full.names=FALSE, pattern="*.wav$", path=sourceDir)
myFilesShort

site <- substr(sourceDir, 22, 25)
zro_crs_lf <- NULL
zro_crs_rt <- NULL

for (n in 1:length(myFilesShort)) {
  lgth <- file.size(myFiles[n])/(sampling_rate*4)
  seqA <- seq(0, lgth, 60)
  similarity <- NULL
  for(i in 2:length(seqA)) {  
    wave2 <- readWave(myFiles[n], from = seqA[i-1], to = seqA[i], 
                      units = "seconds")
    #play(wave1)
    # prepare separate left and right channel objects
    wave_l <- mono(wave2, "left")
    wave_r <- mono(wave2, "right")
    
    # generate a frequency spectrum 
    speca <- spec(wave_l, f=sampling_rate, wl=1024, plot = FALSE)
    specb <- spec(wave_r, f=sampling_rate, wl=1024, plot = FALSE)
    
    sim <- simspec(speca, specb)
    similarity <- c(similarity, sim)
    
    z_c_lf <- zcr(wave_l, wl = NULL)
    z_c_rt <- zcr(wave_r, wl = NULL)
    
    zro_crs_lf <- c(zro_crs_lf, z_c_lf)
    zro_crs_rt <- c(zro_crs_rt, z_c_rt)
  }
  write.csv(similarity, paste(site, "_",  
                              substr(myFilesShort[n], 1,15), 
                              "_simspec.csv", sep=""), 
            row.names = F)
  png(paste("file_", site,"_", substr(myFilesShort[n], 1,15), 
            ".png", sep = ""), width=800, height=400)
  par(mar=c(3,4,2,5))
  plot(similarity, type="l", ylim=c(0, 65), axes=FALSE, 
       xlab = "", ylab = "", bty = "n", lwd = 1.5,
       main=paste(site, "_", substr(myFilesShort[n], 1,15)))
  abline(h=20, col="red", lw=0.5)
  axis(2, ylim=c(0,65), col="black", las=1)
  mtext("Similarity", side=2, line=2.5)
  box()
  
  par(new=TRUE)
  com_zro <- c(zro_crs_rt, zro_crs_lf)
  plot(zro_crs_lf, xlab="", ylab="", type="l",
       ylim=c(min(com_zro), max(com_zro)), lwd = 1.5,
       axes=FALSE, col="blue", lty=2)
  mtext("Zero crossing rate", side = 4, line = 3.6)
  axis(4, ylim=c(min(com_zro), max(com_zro)), 
       las=1, line = 0)
  axis(1, at = seqA, seqA)
  mtext("Time (minutes)", side=1, line = 2)
  
  par(new=TRUE)
  plot(zro_crs_rt, xlab="", ylab="", type="l",
       ylim=c(min(com_zro), max(com_zro)), lwd = 1.5,
       axes=FALSE, col="darkgreen", lty=2)
  legend("bottomleft", legend=c("LR Similarity", "ZCR - left", "ZCR - right"),
         text.col = c("black", "blue", "darkgreen"), 
         bg = "white", bty = "n",
         lty = c(1,2,2), col = c("black", "blue", "darkgreen"))
  dev.off()
}
