#################################################################
# This code reads in 1 minute wave files and calculates basic 
# spectral properties using seewave
#################################################################
library(seewave)
library(tuneR)
setwd("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\Yvonne")

sourceDir <- "C:\\Work\\Output"

myFiles <- list.files(path=sourceDir, full.names=TRUE,
                      pattern="*.wav")

source("..\\shared\\sort.Filename.R")#changed to sort.filenames

myFiles <- sort.Filename(myFiles)# changed to sort.filenames

fileCount <- length(myFiles)

# Spectral properties function
getSpectralProperties <- function(file){
                print("starting file")
                wavFile <- readWave(file)               
                meanSp <- meanspec(wavFile, f=22050,plot=FALSE)
                result.prop <- specprop(meanSp)
                return(result.prop)
}

spectralProperties <- sapply(myFiles[1:60], getSpectralProperties,
                             USE.NAMES=FALSE)
spectralProperties <- aperm(spectralProperties) # transpose array

View(spectralProperties)

#Acoustic complexity index function
getACI <- function(file){
        print("starting file")
        wavFile <- readWave(file)               
        result.aci<-ACI(wavFile)
        return(result.aci)
}

acousticCompIndex <- sapply(myFiles[1:60], getACI,
                             USE.NAMES=FALSE)

View(acousticCompIndex)

#Zero crossing rate
getZCR <- function(file){
        print("starting file")
        wavFile <- readWave(file)               
        result.zcr<-zcr(wavFile,plot=F,wl=NULL)
        return(result.zcr)
}

zeroCrossingRate <- sapply(myFiles[1:60], getZCR,
                            USE.NAMES=FALSE)

View(zeroCrossingRate)

#Temporal entropy
getTempEntropy <- function(file){
        print("starting file")
        wavFile <- readWave(file) 
        envb<-env(wavFile,f=22050,plot=FALSE)
        result.th<-th(envb)
        return(result.th)
}

temporalEntropy <- sapply(myFiles[1:60], getTempEntropy,
                           USE.NAMES=FALSE)

View(temporalEntropy)

allProperties<-cbind(acousticCompIndex, zeroCrossingRate, 
                     temporalEntropy,spectralProperties)

allProperties<-data.frame(allProperties)
View(allProperties)

######## PLOTTING ##############################
par(mfcol=c(2,2)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(1:60),acousticCompIndex, ylab="Acoustic complexity index")
plot(c(1:60),zeroCrossingRate, ylab="Zero crossing rate")
plot(c(1:60),temporalEntropy, ylab="Temporal entropy")
plot(c(1:60),sd, ylab="Standard deviation")
###########
plot(c(1:60),median, ylab="median frequency")
plot(c(1:60),sem,ylab="standard error of mean")
plot(c(1:60),IQR, ylab="interquartile range")
plot(c(1:60),cent, ylab="spectral centroid")
###########
plot(c(1:60),skewness)
plot(c(1:60),kurtosis)
plot(c(1:60),sfm, ylab="spectral flatness")
plot(c(1:60),sh, ylab="spectral entropy")
##########
plot(c(1:60),mean, ylab="mean frequency (Hz)")
plot(c(1:60),Q25)
plot(c(1:60),Q75)
plot(c(1:60),IQR, ylab="interquartile range")
###########