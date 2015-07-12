#################################################################
# This code reads one minute wave files, calculates the 
# spectral properties available in seewave and saves as a csv file
#################################################################
library(seewave)
library(tuneR)

#setwd("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\Yvonne")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")
setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")

folder <- "C:\\Work\\Output\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\"

folder.names <- read.csv("mapping.csv")[ ,2]

length <- length(folder.names)
source("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\shared\\sort.Filename.R")

######## TEMPORAL ENTROPY FUNCTION ##############################
getTempEntropy <- function(file){
  print("starting file")
  wavFile <- readWave(file) 
  envb<-env(wavFile,f=22050,plot=FALSE)
  result.th<-th(envb)
  return(result.th)
}

######## SPECTRAL PROPERTIES  FUNCTION ##############################
getSpectralProperties <- function(file){
  print("starting file")
  wavFile <- readWave(file)               
  meanSp <- meanspec(wavFile, f=22050, plot=FALSE)
  result.prop <- specprop(meanSp)
  return(result.prop)
}

######## ACOUSTIC COMPLEXITY INDEX  FUNCTION ##############################
getACI <- function(file){
  print("starting file")
  wavFile <- readWave(file, from=0, to=59, units="seconds")               
  result.aci<-ACI(wavFile)
  return(result.aci)
}
######## ZERO CROSSING RATE  FUNCTION ##############################
getZCR <- function(file){
  print("starting file")
  wavFile <- readWave(file)               
  result.zcr<-zcr(wavFile,plot=F,wl=NULL)
  return(result.zcr)
}
#######################################################
acoustCompIndex <- NULL
temEntropy <- NULL
ZCRate <- NULL
spectProperties <- NULL
fileNames <- NULL
###################################################
for (i in 1:2) {
  pathName <- paste(folder, folder.names[i],"\\", sep="")
  print(pathName)
  folder.contents <- list.files(full.names = FALSE, pattern = "*.wav", 
        path = pathName)
  folder.contents <- sort.Filename(folder.contents)
  length2 <- length(folder.contents)
  fileNames <- c(fileNames, folder.contents[1:(length2-1)])
  print(folder.contents[1])
  temporalEntropy <- sapply(paste(pathName, folder.contents[1:(length2-1)], sep=""), 
        getTempEntropy, USE.NAMES = F)
  temEntropy <- c(temEntropy, temporalEntropy)
  acousticCompIndex <- sapply(paste(pathName, folder.contents[1:(length2-1)],sep=""), getACI, 
        USE.NAMES = F)
  acoustCompIndex <- c(acoustCompIndex, acousticCompIndex)
  zeroCrossingRate <- sapply(paste(pathName, folder.contents[1:(length2-1)],sep=""), getACI, 
        USE.NAMES = F)
  ZCRate <- c(ZCRate, zeroCrossingRate )
  spectralProperties <- sapply(paste(pathName, folder.contents[1:(length2-1)],sep=""), getSpectralProperties, 
        USE.NAMES = F)
  spectralProperties <- aperm(spectralProperties) # transpose array
  spectProperties <-rbind(spectProperties, spectralProperties)
}

######## ALL PROPERTIES ##############################
allProperties<-cbind(temEntropy, acoustCompIndex, ZCRate, spectProperties, fileNames) 
  
allProperties<-data.frame(allProperties)
View(allProperties)

######## WRITE MATRIX ##############################
library(MASS)
write.matrix(allProperties,file="TEST.csv",sep=",")
