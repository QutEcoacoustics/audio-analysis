setwd("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\Yvonne")

properties1<-read.csv("output1.csv", header = TRUE)
attach(properties1)
properties2<-read.csv("output2.csv", header = TRUE)
attach(properties2)
properties3<-read.csv("output3.csv", header = TRUE)
attach(properties3)
properties4<-read.csv("output4.csv", header = TRUE)
attach(properties4)
properties5<-read.csv("output5.csv", header = TRUE)
attach(properties5)
properties6<-read.csv("output6.csv", header = TRUE)
attach(properties6)
properties7<-read.csv("output7.csv", header = TRUE)
attach(properties7)
properties8<-read.csv("output8.csv", header = TRUE)
attach(properties8)
properties9<-read.csv("output9.csv", header = TRUE)
attach(properties9)
properties10<-read.csv("output10.csv", header = TRUE)
attach(properties10)
properties11<-read.csv("output11.csv", header = TRUE)
attach(properties11)
properties12<-read.csv("output12.csv", header = TRUE)
attach(properties12)
properties13<-read.csv("output13.csv", header = TRUE)
attach(properties13)
properties14<-read.csv("output14.csv", header = TRUE)
attach(properties14)
properties15<-read.csv("output15.csv", header = TRUE)
attach(properties15)
properties16<-read.csv("output16.csv", header = TRUE)
attach(properties16)
properties17<-read.csv("output17.csv", header = TRUE)
attach(properties17)
properties18<-read.csv("output18.csv", header = TRUE)
attach(properties18)
########### PLOTTING ##############
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
###### Q25 GRAPH SET 1#################################
plot(c(0:119),c(properties1$Q25,properties2$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="1:34 to 3:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties3$Q25,properties4$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="3:34 to 5:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties5$Q25,properties6$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="5:34 to 7:34 pm on 15 March 2015",type="l")
abline(v=32)
###### Q25 GRAPH SET 2#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties7$Q25,properties8$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="7:34 to 9:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties9$Q25,properties10$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="9:34 to 11:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties11$Q25,properties12$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="11:34pm to 1:34 am on 16 March 2015",type="l")
###### Q25 GRAPH SET 3#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties13$Q25,properties14$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="1:34 to 3:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties15$Q25,properties16$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="3:34 to 5:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties17$Q25,properties18$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="5:34 to 7:34 am on 16 March 2015",type="l")
abline(v=16)
#########################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
######  ZCR GRAPH SET 1#################################
plot(c(0:119),c(properties1$zeroCrossingRate,properties2$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="1:34 to 3:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties3$zeroCrossingRate,properties4$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="3:34 to 5:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties5$zeroCrossingRate,properties6$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="5:34 to 7:34 pm on 15 March 2015",type="l")
abline(v=32)
###### ZCR GRAPH SET 2#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties7$zeroCrossingRate,properties8$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="7:34 to 9:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties9$zeroCrossingRate,properties10$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="9:34 to 11:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties11$zeroCrossingRate,properties12$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="11:34pm to 1:34 am on 16 March 2015",type="l")
###### ZCR GRAPH SET 3#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties13$zeroCrossingRate,properties14$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="1:34 to 3:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties15$zeroCrossingRate,properties16$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="3:34 to 5:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties17$zeroCrossingRate,properties18$zeroCrossingRate), 
     ylab="ZCR",ylim=c(0,0.42),
     xlab="5:34 to 7:34 am on 16 March 2015",type="l")
abline(v=16)
#########################################
#########################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
###### ACI GRAPH SET 1#################################
plot(c(0:119),c(properties1$acousticCompIndex,properties2$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="1:34 to 3:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties3$acousticCompIndex,properties4$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="3:34 to 5:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties5$acousticCompIndex,properties6$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="5:34 to 7:34 pm on 15 March 2015",type="l")
abline(v=32)
###### ACI GRAPH SET 2#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties7$acousticCompIndex,properties8$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="7:34 to 9:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties9$acousticCompIndex,properties10$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="9:34 to 11:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties11$acousticCompIndex,properties12$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="11:34pm to 1:34 am on 16 March 2015",type="l")
###### ACI GRAPH SET 3#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties13$acousticCompIndex,properties14$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="1:34 to 3:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties15$acousticCompIndex,properties16$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="3:34 to 5:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties17$acousticCompIndex,properties18$acousticCompIndex), 
     ylab="ACI",ylim=c(140,165),
     xlab="5:34 to 7:34 am on 16 March 2015",type="l")
abline(v=16)
#########################################
#########################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
###### skewness GRAPH SET 1#################################
plot(c(0:119),c(properties1$skewness,properties2$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="1:34 to 3:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties3$skewness,properties4$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="3:34 to 5:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties5$skewness,properties6$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="5:34 to 7:34 pm on 15 March 2015",type="l")
abline(v=32)
###### skewness GRAPH SET 2#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties7$skewness,properties8$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="7:34 to 9:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties9$skewness,properties10$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="9:34 to 11:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties11$skewness,properties12$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="11:34pm to 1:34 am on 16 March 2015",type="l")
###### skewness GRAPH SET 3#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties13$skewness,properties14$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="1:34 to 3:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties15$skewness,properties16$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="3:34 to 5:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties17$skewness,properties18$skewness), 
     ylab="skewness",ylim=c(1,11),
     xlab="5:34 to 7:34 am on 16 March 2015",type="l")
abline(v=16)
#########################################
#########################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
###### MEAN GRAPH SET 1#################################
plot(c(0:119),c(properties1$mean,properties2$mean), 
     ylab="mean",ylim=c(0,5000),
     xlab="1:34 to 3:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties3$mean,properties4$mean), 
     ylab="mean",ylim=c(0,5000),
     xlab="3:34 to 5:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties5$mean,properties6$mean), 
     ylab="mean",ylim=c(0,5000),
     xlab="5:34 to 7:34 pm on 15 March 2015",type="l")
abline(v=32)
###### MEAN GRAPH SET 2#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties7$mean,properties8$mean), 
     ylab="Mean",ylim=c(0,5000),
     xlab="7:34 to 9:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties9$mean,properties10$mean), 
     ylab="mean",ylim=c(0,5000),
     xlab="9:34 to 11:34 pm on 15 March 2015",type="l")
plot(c(0:119),c(properties11$mean,properties12$mean), 
     ylab="mean",ylim=c(0,5000),
     xlab="11:34pm to 1:34 am on 16 March 2015",type="l")
###### MEAN GRAPH SET 3#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties13$mean,properties14$mean), 
     ylab="mean",ylim=c(0,5000),
     xlab="1:34 to 3:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties15$mean,properties16$mean), 
     ylab="mean",ylim=c(0,5000),
     xlab="3:34 to 5:34 am on 16 March 2015",type="l")
plot(c(0:119),c(properties17$mean,properties18$mean), 
     ylab="mean",ylim=c(0,5000),
     xlab="5:34 to 7:34 am on 16 March 2015",type="l")
abline(v=16)
#########################################

par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
########### ACOUSTIC COMPLEXITY INDEX ##############
########### ZERO CROSSING RATE ##############
########### TEMPORAL ENTROPY ##############
plot(c(0:59),properties1$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="1:34 to 2:34 pm on 15 March 2015")
plot(c(0:59),properties2$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="2:34 to 3:34 pm on 15 March 2015")
plot(c(0:59),properties3$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="3:34 to 4:34 pm on 15 March 2015")
plot(c(0:59),properties4$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="4:34 to 5:34 pm on 15 March 2015")
plot(c(0:59),properties5$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="5:34 to 6:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:59),properties6$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="6:34 to 7:34 pm on 15 March 2015")
plot(c(0:59),properties7$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="7:34 to 8:34 pm on 15 March 2015")
plot(c(0:59),properties8$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="8:34 to 9:34 pm on 15 March 2015")
plot(c(0:59),properties9$temporalEntropy, 
     ylab="Temporal entropy",ylim=c(0.9,1),
     xlab="9:34 to 10:34 pm on 15 March 2015")

########### MEAN ##############
########### MEDIAN ##############
plot(c(0:59),properties1$median, ylab="Median",ylim=c(0,6000),
     xlab="1:34 to 2:34 pm on 15 March 2015")
plot(c(0:59),properties2$median, ylab="Median",ylim=c(0,6000),
     xlab="2:34 to 3:34 pm on 15 March 2015")
plot(c(0:59),properties3$median, ylab="Median",ylim=c(0,6000),
     xlab="3:34 to 4:34 pm on 15 March 2015")
plot(c(0:59),properties4$median, ylab="Median",ylim=c(0,6000),
     xlab="4:34 to 5:34 pm on 15 March 2015")
plot(c(0:59),properties5$median, ylab="Median",ylim=c(0,6000),
     xlab="5:34 to 6:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:59),properties6$median, ylab="Median",ylim=c(0,6000),
     xlab="6:34 to 7:34 pm on 15 March 2015")
plot(c(0:59),properties7$median, ylab="Median",ylim=c(0,6000),
     xlab="7:34 to 8:34 pm on 15 March 2015")
plot(c(0:59),properties8$median, ylab="Median",ylim=c(0,6000),
     xlab="8:34 to 9:34 pm on 15 March 2015")
plot(c(0:59),properties9$median, ylab="Median",ylim=c(0,6000),
     xlab="9:34 to 10:34 pm on 15 March 2015")
########### SPECTRAL CENTROID ##############
plot(c(0:59),properties1$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="1:34 to 2:34 pm on 15 March 2015")
plot(c(0:59),properties2$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="2:34 to 3:34 pm on 15 March 2015")
plot(c(0:59),properties3$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="3:34 to 4:34 pm on 15 March 2015")
plot(c(0:59),properties4$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="4:34 to 5:34 pm on 15 March 2015")
plot(c(0:59),properties5$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="5:34 to 6:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:59),properties6$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="6:34 to 7:34 pm on 15 March 2015")
plot(c(0:59),properties7$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="7:34 to 8:34 pm on 15 March 2015")
plot(c(0:59),properties8$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="8:34 to 9:34 pm on 15 March 2015")
plot(c(0:59),properties9$cent, ylab="Spectral centroid",
     ylim=c(0,5000), xlab="9:34 to 10:34 pm on 15 March 2015")

########### SKEWNESS ##############
########### KURTOSIS ##############
plot(c(0:59),properties1$kurtosis, ylab="Kurtosis",
     ylim=c(0,120), xlab="1:34 to 2:34 pm on 15 March 2015")
plot(c(0:59),properties2$kurtosis, ylab="Kurtosis",
     ylim=c(0,120), xlab="2:34 to 3:34 pm on 15 March 2015")
plot(c(0:59),properties3$kurtosis, ylab="Kurtosis",
     ylim=c(0,120),, xlab="3:34 to 4:34 pm on 15 March 2015")
plot(c(0:59),properties4$kurtosis, ylab="Kurtosis",
     ylim=c(0,120), xlab="4:34 to 5:34 pm on 15 March 2015")
plot(c(0:59),properties5$kurtosis, ylab="Kurtosis",
     ylim=c(0,120), xlab="5:34 to 6:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:59),properties6$kurtosis, ylab="Kurtosis",
     ylim=c(0,120), xlab="6:34 to 7:34 pm on 15 March 2015",type='l')
plot(c(0:59),properties7$kurtosis, ylab="Kurtosis",
     ylim=c(0,120), xlab="7:34 to 8:34 pm on 15 March 2015")
plot(c(0:59),properties8$kurtosis, ylab="Kurtosis",
     ylim=c(0,120), xlab="8:34 to 9:34 pm on 15 March 2015")
plot(c(0:59),properties9$kurtosis, ylab="Kurtosis",
     ylim=c(0,120), xlab="9:34 to 10:34 pm on 15 March 2015")
########### SPECTRAL FLATNESS ##############
plot(c(0:59),properties1$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="1:34 to 2:34 pm on 15 March 2015",type="l")
plot(c(0:59),properties2$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="2:34 to 3:34 pm on 15 March 2015")
plot(c(0:59),properties3$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="3:34 to 4:34 pm on 15 March 2015")
plot(c(0:59),properties4$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="4:34 to 5:34 pm on 15 March 2015")
plot(c(0:59),properties5$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="5:34 to 6:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:59),properties6$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="6:34 to 7:34 pm on 15 March 2015")
plot(c(0:59),properties7$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="7:34 to 8:34 pm on 15 March 2015")
plot(c(0:59),properties8$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="8:34 to 9:34 pm on 15 March 2015")
plot(c(0:59),properties9$sfm, ylab="Spectral flatness",
     ylim=c(0,1), xlab="9:34 to 10:34 pm on 15 March 2015")

########### SPECTRAL ENTROPY ##############
plot(c(0:59),properties1$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="1:34 to 2:34 pm on 15 March 2015")
plot(c(0:59),properties2$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="2:34 to 3:34 pm on 15 March 2015")
plot(c(0:59),properties3$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="3:34 to 4:34 pm on 15 March 2015")
plot(c(0:59),properties4$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="4:34 to 5:34 pm on 15 March 2015")
plot(c(0:59),properties5$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="5:34 to 6:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:59),properties6$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="6:34 to 7:34 pm on 15 March 2015")
plot(c(0:59),properties7$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="7:34 to 8:34 pm on 15 March 2015")
plot(c(0:59),properties8$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="8:34 to 9:34 pm on 15 March 2015")
plot(c(0:59),properties9$sh, ylab="Spectral entropy",
     ylim=c(0,1), xlab="9:34 to 10:34 pm on 15 March 2015")

########### Q25 ##############
plot(c(0:59),properties1$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="1:34 to 2:34 pm on 15 March 2015")
plot(c(0:59),properties2$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="2:34 to 3:34 pm on 15 March 2015")
plot(c(0:59),properties3$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="3:34 to 4:34 pm on 15 March 2015")
plot(c(0:59),properties4$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="4:34 to 5:34 pm on 15 March 2015")
plot(c(0:59),properties5$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="5:34 to 6:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:59),properties6$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="6:34 to 7:34 pm on 15 March 2015")
plot(c(0:59),properties7$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="7:34 to 8:34 pm on 15 March 2015")
plot(c(0:59),properties8$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="8:34 to 9:34 pm on 15 March 2015")
plot(c(0:59),properties9$Q25, ylab="25th quartile",
     ylim=c(0,4500), xlab="9:34 to 10:34 pm on 15 March 2015")

########### Q75 ##############
plot(c(0:59),properties1$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="1:34 to 2:34 pm on 15 March 2015")
plot(c(0:59),properties2$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="2:34 to 3:34 pm on 15 March 2015")
plot(c(0:59),properties3$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="3:34 to 4:34 pm on 15 March 2015")
plot(c(0:59),properties4$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="4:34 to 5:34 pm on 15 March 2015")
plot(c(0:59),properties5$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="5:34 to 6:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:59),properties6$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="6:34 to 7:34 pm on 15 March 2015")
plot(c(0:59),properties7$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="7:34 to 8:34 pm on 15 March 2015")
plot(c(0:59),properties8$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="8:34 to 9:34 pm on 15 March 2015")
plot(c(0:59),properties9$Q75, ylab="75th quartile",
     ylim=c(0,8000), xlab="9:34 to 10:34 pm on 15 March 2015")

par(mfrow=c(3,1)) # set layout
par(mar=c(4.1, 4.6, 1.6, 2.1)) # set margins
plot(c(0:119),c(properties1$Q25,properties2$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="1:34 to 3:34 pm on 15 March 2015")
plot(c(0:119),c(properties3$Q25,properties4$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="3:34 to 5:34 pm on 15 March 2015")
plot(c(0:119),c(properties5$Q25,properties6$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="5:34 to 7:34 pm on 15 March 2015")
abline(v=32)
plot(c(0:119),c(properties7$Q25,properties8$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="7:34 to 9:34 pm on 15 March 2015")
plot(c(0:119),c(properties9$Q25,properties10$Q25), 
     ylab="25th quartile",ylim=c(0,4500),
     xlab="7:34 to 9:34 pm on 15 March 2015")