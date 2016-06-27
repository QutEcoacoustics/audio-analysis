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

#http://stackoverflow.com/questions/29060491/how-to-create-a-time-series-analysis-where-y-axis-are-categorical-variable

sourceDir <- "D:\\Cooloola\\2015_06_28\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\20151220\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20151220\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\20160131\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20160131\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\20160207\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\20160207\\Woondum3\\"

sourceDir <- "F:\\Cooloola\\20160214\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160214\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\20160221\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160221\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\20160228\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160228\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\20160306\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160306\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\20160313\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160313\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\20160320\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160320\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\20160327\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160327\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\20160403\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160403\\Woondum3\\"

sourceDir <- "F:\\Cooloola\\20151220\\GympieNP\\"  # setup
sourceDir <- "F:\\Cooloola\\20151220\\Woondum3\\"  # setup
sourceDir <- "F:\\Cooloola\\20160207\\GympieNP\\"  # setup
sourceDir <- "F:\\Cooloola\\20160306\\GympieNP\\"  # setup
sourceDir <- "F:\\Cooloola\\20160320\\GympieNP\\"  # setup
sourceDir <- "F:\\Cooloola\\20160320\\Woondum3\\"  # setup
sourceDir <- "F:\\Cooloola\\20160327\\GympieNP\\"  # setup
sourceDir <- "F:\\Cooloola\\20160327\\Woondum3\\" # file 10 onwards
sourceDir <- "F:\\Cooloola\\20160403\\Woondum3\\" # file 23 onwards
sourceDir <- "F:\\Cooloola\\20160410\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\20160410\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\20160501\\GympieNP\\"

sourceDir <- "E:\\Cooloola\\20160605\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\20160605\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\20160501\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\20160501\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_07_05\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_07_05\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_07_12\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_07_12\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_07_19\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_07_19\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_07_26\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_07_26\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_08_02\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_08_02\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_08_09\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_08_09\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_08_16\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_08_16\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_08_23\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_08_23\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_08_30\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_08_30\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_09_06\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_09_06\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_09_13\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_09_13\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_09_20\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_09_20\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_09_27\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_09_27\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_10_04\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_10_04\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_10_04\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_10_04\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_10_25\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_10_25\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_11_01\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_11_01\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_11_08\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_11_08\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_11_15\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_11_15\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_11_22\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_11_22\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_11_29\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_11_29\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_12_06\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_12_06\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\2015_12_13\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_12_13\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\2015_12_29\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_12_29\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\2016_01_07\\GympieNP\\" #set
sourceDir <- "E:\\Cooloola\\2016_01_07\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_01_16\\GympieNP\\" #set
sourceDir <- "E:\\Cooloola\\2016_01_16\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_01_24\\GympieNP\\" #set
sourceDir <- "E:\\Cooloola\\2016_01_24\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_01_31\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_01_31\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_02_07\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_02_07\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_02_14\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_02_14\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_02_21\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_02_21\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_02_28\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_02_28\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_03_06\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_03_06\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_03_13\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_03_13\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_03_20\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_03_20\\Woondum3\\" #set
sourceDir <- "E:\\Cooloola\\2016_03_27\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_03_27\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_04_03\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_04_03\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2016_04_10\\GympieNP\\" # set
sourceDir <- "E:\\Cooloola\\2016_04_10\\Woondum3\\" # set
sourceDir <- "D:\\Cooloola\\2016_04_17\\GympieNP\\" # set
sourceDir <- "D:\\Cooloola\\2016_04_17\\Woondum3\\" # set
sourceDir <- "D:\\Cooloola\\2016_04_24\\GympieNP\\" # set
sourceDir <- "D:\\Cooloola\\2016_04_24\\Woondum3\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_01\\GympieNP\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_01\\Woondum3\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_08\\GympieNP\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_08\\Woondum3\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_15\\GympieNP\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_15\\Woondum3\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_22\\GympieNP\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_22\\Woondum3\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_29\\GympieNP\\" # set
sourceDir <- "D:\\Cooloola\\2016_05_29\\Woondum3\\" # set
sourceDir <- "D:\\Cooloola\\2016_06_05\\GympieNP\\" # set
sourceDir <- "D:\\Cooloola\\2016_06_05\\Woondum3\\" # set
sourceDir <- "E:\\Cooloola\\2015_10_04\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_10_04\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_10_11\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_10_11\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\2015_06_21\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_06_21\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\2015_06_28\\GympieNP\\" #
sourceDir <- "E:\\Cooloola\\2015_06_28\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2015_12_20\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_08_02\\Woondum3\\"
sourceDir <- "E:\\Cooloola\\2015_08_16\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_11_22\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2016_06_12\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2015_10_25\\Woondum3\\"
sourceDir <- "F:\\Cooloola\\2016_06_12\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_08_02\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_12_20\\GympieNP\\"
sourceDir <- "F:\\Cooloola\\2015_10_25\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2016_06_19\\GympieNP\\"
sourceDir <- "D:\\Cooloola\\2016_06_19\\Woondum3\\"


setwd(paste(sourceDir))

# Obtain a list of the original wave files
myFiles <- list.files(full.names=TRUE, pattern="*.wav$", path=sourceDir)
myFiles
myFilesShort <- list.files(full.names=FALSE, pattern="*.wav$", path=sourceDir)
myFilesShort

site <- substr(sourceDir, 24, 27)
site
# loop calculates power spectrum statistics and saves to file
for (n in 1:length(myFilesShort)) {
  n=n
  lgth <- file.size(myFiles[n])/(sampling_rate*4)
  if(lgth >= 600) { 
    seqA <- seq(0, lgth, 60)
    seqB <- seq(seqA[10], seqA[length(seqA)], 600)
    seqC <- seqB-60
    
    avg_psp_left <- NULL
    avg_psp_right <- NULL
    med_psp_left <- NULL
    med_psp_right <- NULL
    max_psp_left <- NULL
    max_psp_right <- NULL
    sd_psp_left <- NULL
    sd_psp_right <- NULL
    
    for(i in 2:length(seqC)) {
      i=i
      wave2 <- readWave(myFiles[n], from = seqC[i-1], 
                        to = (seqC[i-1]+60), 
                        units = "seconds")
      
      pspectrum_left <- powspec(wave2@left, sampling_rate)
      pspectrum_right <- powspec(wave2@right, sampling_rate)
      average_pspectrum_l <- mean(pspectrum_left)
      average_pspectrum_r <- mean(pspectrum_right)
      median_pspectrum_l <- median(pspectrum_left)
      median_pspectrum_r <- median(pspectrum_right)
      max_pspectrum_l <- max(pspectrum_left)
      max_pspectrum_r <- max(pspectrum_right)
      sd_pspectrum_l <- sd(pspectrum_left)
      sd_pspectrum_r <- sd(pspectrum_right)
      
      avg_psp_left <- c(avg_psp_left, average_pspectrum_l)
      avg_psp_right <- c(avg_psp_right, average_pspectrum_r)
      med_psp_left <- c(med_psp_left, median_pspectrum_l)
      med_psp_right <- c(med_psp_right, median_pspectrum_r)
      max_psp_left <- c(max_psp_left, max_pspectrum_l)
      max_psp_right <- c(max_psp_right, max_pspectrum_r)
      sd_psp_left <- c(sd_psp_left, sd_pspectrum_l)
      sd_psp_right <- c(sd_psp_right, sd_pspectrum_r)
    }
    
    combined2 <- cbind(avg_psp_left, avg_psp_right, med_psp_left,
                       med_psp_right, max_psp_left, max_psp_right,
                       sd_psp_left, sd_psp_right)
    
    write.csv(combined2, paste(site, substr(myFilesShort[n], 1,15), 
                               "_powspec.csv", sep=""), 
              row.names = F)
  }
}
###########################################
# loop below calculates similarity and zero crossing 
# statistics and saves to file
###########################################
for (n in 1:length(myFilesShort)) {
  n=n
  lgth <- file.size(myFiles[n])/(sampling_rate*4)
  seqA <- seq(0, lgth, 60)
  similarity <- NULL
  zro_crs_left <- NULL
  zro_crs_right <- NULL
  
  for(i in 2:length(seqA)) {
    i=i
    wave2 <- readWave(myFiles[n], from = seqA[i-1], to = seqA[i], 
                      units = "seconds")
    #play(wave1)
    # prepare separate left and right channel objects
    wave_left <- mono(wave2, "left")
    wave_right <- mono(wave2, "right")
    #rm(wave2)
    # generate a frequency spectrum (note: meanspec can be used
    # instead of spec and this could be used in conjunction with
    # cutspec to select out a particular frequency range
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
  
  combined1 <- cbind(similarity, zro_crs_left, zro_crs_right)  
  write.csv(combined1, paste(site, substr(myFilesShort[n], 1,15), 
              "_simspec_cut.csv", sep=""), 
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
  #plot the right channel zero crossing rate
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
