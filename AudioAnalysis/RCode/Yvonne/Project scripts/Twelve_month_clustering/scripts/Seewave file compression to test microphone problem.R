# see spectral difference.R for information on how to use seewave
# to cutfiles
# for example``
# prepare separate left and right channel objects
library(tuneR)
library(seewave)
sampling_rate <- 22050

sourceDir <- "D:\\Cooloola\\2016_01_31\\Woondum3\\"
sourceDir <- "D:\\Cooloola\\2015_06_28\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2015_11_01\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2016_01_07\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2016_01_16\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2016_01_24\\GympieNP\\"
sourceDir <- "E:\\Cooloola\\2016_01_31\\GympieNP\\"
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
  wave_size_left <- NULL
  flac_size_left <- NULL
  wave_size_right <- NULL
  flac_size_right <- NULL
  
  for(i in 2:length(seqA)) {
    i=i
    wave2 <- readWave(myFiles[n], from = seqA[i-1], 
                      to = seqA[i], 
                      units = "seconds")
    # prepare separate left channel objects
    wave_left <- mono(wave2, "left")
    savewav(wave_left, f=22050)
    wave_size_lf <- file.info("wave_left.wav")$size
    wave_size_lf
    wave_size_left <- c(wave_size_left, wave_size_lf)
    wav2flac("wave_left.wav", overwrite=TRUE)
    flac_size_lf <- file.info("wave_left.flac")$size
    flac_size_lf
    flac_size_left <- c(flac_size_left, flac_size_lf)
    unlink("wave_left.wav")
    unlink("wave_left.flac")
    
    # prepare separate right channel objects
    wave_right <- mono(wave2, "right")
    savewav(wave_right, f=22050)
    wave_size_rg <- file.info("wave_right.wav")$size
    wave_size_rg
    wave_size_right <- c(wave_size_right, wave_size_rg)
    wav2flac("wave_right.wav", overwrite=TRUE)
    flac_size_rg <- file.info("wave_right.flac")$size
    flac_size_rg
    flac_size_right <- c(flac_size_right, flac_size_rg)
    unlink("wave_right.wav")
    unlink("wave_right.flac")
    print(i)
  }
  print(n)
  combined <- cbind(wave_size_left, wave_size_right, 
                    flac_size_left, flac_size_right) 
  colnames(combined) <- c("wave_left", "wave_right",
                          "flac_left", "flac_right")
  combined <- data.frame(combined)
  # save the statistics
  write.csv(combined, paste(site, substr(myFilesShort[n], 1,15), 
                             "_wave_flac_size.csv", sep=""), 
            row.names = F)
  ylim = c((1.2E6/1E6), 2.4)
  tiff(paste("wav_flac",substr(myFilesShort[n], 1,15),".tiff", sep=""), res=300, height=600, width=1200)
  par(mar=c(2.2,2.2,2.2,0.2), mgp=c(3,0.2,0), cex=0.6, tcl=0.2)
  plot((combined$wave_left/1E6), type="l", 
       main=paste(site, "_", substr(myFilesShort[n], 1,15)),
       col="black", ylim=ylim, xlab="",
       ylab="", las=1, cex=0.6)
  mtext(side=1, "Minute", line=1.2, cex=0.6)
  mtext(side=2, "File size (MB)", line=1.3, cex=0.6)
  par(new=T)
  plot((combined$wave_right/1E6), type="l", col="black", 
       ylim=ylim, ylab="", xlab="", las=1, yaxt="n", xaxt="n")
  par(new=T)
  plot((combined$flac_left/1E6), type="l", col="blue", ylim=ylim,
       ylab="", xlab="", las=1, yaxt="n", xaxt="n")
  par(new=T)
  plot((combined$flac_right/1E6), type="l", col="red", ylim=ylim,
       ylab="", xlab="", las=1, yaxt="n", xaxt="n")
  text(x=45, y=1.65, paste("File size ", 
                          round(mean(combined$wave_left)/1E6, 2), "MB", sep=""),
       cex=0.6)
  legend("bottomleft", legend=c("FLAC - left", "FLAC - right"),
         text.col = c("black", "black"), adj = c(0,0.2),
         bg = "white", bty = "n", y.intersp =0.8, cex=0.6,
         lty = c(1,1,1), col = c("blue", "red"))
  dev.off()
}

#####################
file <- "D:\\Cooloola\\2016_01_31\\Woondum3\\Woondum20160130_201742_wave_flac_size.csv"
data_flac <- read.csv(file, header = T)
ylim <- c(0.5, 1)
plot((data_flac$flac_left/data_flac$wave_left),
     col="blue", type="l", ylim = ylim,
     ylab="Flac compression", xlab="Minute",
     main=paste(substr("D:\\Cooloola\\2016_01_31\\Woondum3\\Woondum20160130_201742_wave_flac_size.csv",24,30),
                substr("D:\\Cooloola\\2016_01_31\\Woondum3\\Woondum20160130_201742_wave_flac_size.csv",40,54)))
par(new=T)
plot((data_flac$flac_right/data_flac$wave_right), 
     col="red", type="l", ylim = ylim,
     ylab="", xlab="")
legend("bottomleft", legend=c("FLAC - left", "FLAC - right"),
       text.col = c("black", "black"), adj = c(0,0.2),
       bg = "white", bty = "n", y.intersp =0.8, cex=0.6,
       lty = c(1,1), col = c("blue", "red"))
# %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
library(tuneR)
library(seewave)
sampling_rate <- 22050

sourceDir <- "E:\\Cooloola\\2015_11_01\\GympieNP\\"
site <- substr(sourceDir, 24, 31)
site

# Obtain a list of the original wave files
myFiles <- list.files(full.names=TRUE, pattern="*.wav$", path=sourceDir)
myFiles
myFilesShort <- list.files(full.names=FALSE, pattern="*.wav$", path=sourceDir)
myFilesShort
length <- file.info(myFiles[16])
wav_20151029_064553 <- readWave(myFiles[16], from = 0, 
                                to = 300, 
                                units = "seconds")
wav_left <- mono(wav_20151029_064553, "left")
wav_right <- mono(wav_20151029_064553, "right")
duration <- duration(wav_20151029_064553, f = sampling_rate)
duration
par(mfrow=c(2,1), mar=c(2,2,1,1))
spec(wav_left, col="black", mgp=c(3,0.1,0))
spec(wav_right, col="black", mgp=c(3,0.1,0))

spectro(wav_20151029_064553, dB=NULL, norm=FALSE, scale=FALSE)
spectro(wav_left, dB=NULL, norm=FALSE, scale=FALSE)

layout(matrix(c(1,2),nc=2),widths=c(3,1))
par(mar=c(5,4,3,0.5))
spectro(wav_left,f=22050,dB=NULL, norm=FALSE, scale=FALSE)
par(mar=c(5,1,3,0.5))
spec(wav_left,f=22050,col="red",plot=2,flab="",yaxt="n")

layout(matrix(c(1,2),nc=2),widths=c(3,1))
par(mar=c(5,4,3,0.5))
spectro(wav_right,f=22050,dB=NULL, norm=TRUE, scale=FALSE)
par(mar=c(5,1,3,0.5))
spec(wav_right,f=22050,col="red",plot=2,flab="",yaxt="n")

par(mfrow=c(2,1), mar=c(2,2,1,1))
spec(wav_left, col="black", mgp=c(3,0.1,0))
spec(wav_right, col="black", mgp=c(3,0.1,0))

wav_20151029_064553 <- readWave(myFiles[16], from = 3000, 
                                to = 3300, 
                                units = "seconds")
wav_left <- mono(wav_20151029_064553, "left")
wav_right <- mono(wav_20151029_064553, "right")
par(mfrow=c(2,1), mar=c(2,2,1,1))
spec(wav_left, col="black", mgp=c(3,0.1,0))
spec(wav_right, col="black", mgp=c(3,0.1,0))

wav_20151029_064553 <- readWave(myFiles[16], from = 6000, 
                                to = 6300, 
                                units = "seconds")
wav_left <- mono(wav_20151029_064553, "left")
wav_right <- mono(wav_20151029_064553, "right")
par(mfrow=c(2,1), mar=c(2,2,1,1))
spec(wav_left, col="black", mgp=c(3,0.1,0))
spec(wav_right, col="black", mgp=c(3,0.1,0))

wav_20151029_064553 <- readWave(myFiles[16], from = 12000, 
                                to = 12300, 
                                units = "seconds")
wav_left <- mono(wav_20151029_064553, "left")
wav_right <- mono(wav_20151029_064553, "right")
par(mfrow=c(2,1), mar=c(2,2,1,1))
a <- spec(wav_left, col="black", mgp=c(3,0.1,0),
          norm=FALSE, scaled=FALSE, PMF=FALSE)
a <- data.frame(a)
max(a$y)
b <- spec(wav_right, col="black", mgp=c(3,0.1,0),
          norm=FALSE, scaled=FALSE, PMF=FALSE)
b <- data.frame(b)
max(b$y)
par(mfrow=c(2,1), mar=c(2,2,1,1))
spec(wav_left, col="black", mgp=c(3,0.1,0),
     norm=FALSE, scaled=FALSE, PMF=FALSE, alim=c(0,30E6))
spec(wav_right, col="black", mgp=c(3,0.1,0),
     norm=FALSE, scaled=FALSE, PMF=FALSE, alim=c(0,30E6))