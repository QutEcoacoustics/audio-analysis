#################################################################
# This code reads in separate 1 minute wave files and calculates 
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
str(allProperties)
View(allProperties)