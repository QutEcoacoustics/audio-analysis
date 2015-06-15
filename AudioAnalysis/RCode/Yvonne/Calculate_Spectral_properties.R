#################################################################
# This code reads one minute wave files and calculates the 
# spectral properties available in seewave
#################################################################
library(seewave)
library(tuneR)
setwd("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\Yvonne")

sourceDir <- "C:\\Work\\Output\\Eastern Eucalypt\\Output66EE"

myFiles <- list.files(path=sourceDir, full.names=TRUE,
                      pattern="*.wav")

source("..\\shared\\sort.Filename.R")

myFiles <- sort.Filename(myFiles)

fileCount <- length(myFiles)

######## TEMPORAL ENTROPY ##############################
getTempEntropy <- function(file){
        print("starting file")
        wavFile <- readWave(file) 
        envb<-env(wavFile,f=22050,plot=FALSE)
        result.th<-th(envb)
        return(result.th)
}

temporalEntropy <- sapply(myFiles[1:fileCount], getTempEntropy,
                          USE.NAMES=FALSE)

#View(temporalEntropy)

######## SPECTRAL PROPERTIES ##############################
getSpectralProperties <- function(file){
                print("starting file")
                wavFile <- readWave(file)               
                meanSp <- meanspec(wavFile, f=22050, plot=FALSE)
                result.prop <- specprop(meanSp)
                return(result.prop)
}

spectralProperties <- sapply(myFiles[1:fileCount], getSpectralProperties,
                             USE.NAMES=FALSE)
spectralProperties <- aperm(spectralProperties) # transpose array

#View(spectralProperties)

######## ACOUSTIC COMPLEXITY INDEX ##############################
getACI <- function(file){
        print("starting file")
        wavFile <- readWave(file)               
        result.aci<-ACI(wavFile)
        return(result.aci)
}

acousticCompIndex <- sapply(myFiles[1:fileCount], getACI,
                             USE.NAMES=FALSE)
#View(acousticCompIndex)

######## ZERO CROSSING RATE ##############################
getZCR <- function(file){
        print("starting file")
        wavFile <- readWave(file)               
        result.zcr<-zcr(wavFile,plot=F,wl=NULL)
        return(result.zcr)
}

zeroCrossingRate <- sapply(myFiles[1:fileCount], getZCR,
                            USE.NAMES=FALSE)

print ("finished calculating the zero crossing rate")

#View(zeroCrossingRate)

######## ALL PROPERTIES ##############################
allProperties<-cbind(acousticCompIndex, zeroCrossingRate, 
                     temporalEntropy,spectralProperties)

allProperties<-data.frame(allProperties)
View(allProperties)

######## WRITE MATRIX ##############################
library(MASS)
write.matrix(allProperties,file="output66EE.csv",sep=",")

######## PLOTTING ##############################
par(mfcol=c(2,2)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
attach(allProperties)
#plot(c(1:fileCount),acousticCompIndex, ylab="Acoustic complexity index")
#plot(c(1:fileCount),zeroCrossingRate, ylab="Zero crossing rate")
#plot(c(1:fileCount),temporalEntropy, ylab="Temporal entropy")
#plot(c(1:fileCount),sd, ylab="Standard deviation")
###########
#plot(c(1:fileCount),median, ylab="median frequency")
#plot(c(1:fileCount),sem,ylab="standard error of mean")
#plot(c(1:fileCount),IQR, ylab="interquartile range")
#plot(c(1:fileCount),cent, ylab="spectral centroid")
###########
#plot(c(1:fileCount),skewness)
#plot(c(1:fileCount),kurtosis)
#plot(c(1:fileCount),sfm, ylab="spectral flatness")
#plot(c(1:fileCount),sh, ylab="spectral entropy")
##########
plot(c(1:fileCount),mean, ylab="mean frequency (Hz)")
plot(c(1:fileCount),Q25)
plot(c(1:fileCount),Q75)
plot(c(1:fileCount),IQR, ylab="interquartile range")
########### END OF CODE ##############