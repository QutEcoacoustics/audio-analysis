
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/false-colour/read.audio.file.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/false-colour/spectrogram.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/Non-negative matrix factorisation/NMF.analysis.R')

load("C:/Users/n8781699/Downloads/Eastern bristle bird/bristle1.RData")
load("C:/Users/n8781699/Downloads/Eastern bristle bird/bristle2.RData")

# filepath <- "C:\\Users\\n8781699\\Downloads\\cws_aviaries_0m_1186_262852_20150531_124516_30_0.wav"
# filepath <- "C:\\Users\\n8781699\\Downloads\\west_lamington_site_d_1269_336756_20150915_092030_30_0.wav"
filenames <- list.files("C:\\Users\\n8781699\\Downloads\\training recordings_jessie")
tags <- numeric()

for(i in 1:length(filenames)){
  filepath <- paste("C:\\Users\\n8781699\\Downloads\\training recordings_jessie\\",filenames[i],sep="")
  file.info <- read.audio.file(filepath)
  spectra<-spectrogram(file.info[[1]],file.info[[3]])
  
  amp<-spectra[2:dim(spectra)[1],31:246]
  results<-NMF.analysis(amp)
  valid.components<-nrow(results[[2]])
  
  haha<-rbind(bristle1,bristle2,bristle3, bristle4, bristle5, bristle6, bristle7, results[[2]])
  hehe<-cor(t(haha))
  hehe<-round(hehe, digits=2)
  
  threshold <- 0.6
  if(any(hehe[-(1:7),1]>=threshold) | any(hehe[-(1:7),2]>=threshold) | any(hehe[-(1:7),3]>=threshold) | any(hehe[-(1:7),4]>=threshold) | any(hehe[-(1:7),5]>=threshold) | any(hehe[-(1:7),6]>=threshold) | any(hehe[-(1:7),7]>=threshold))
#     print('true')
    tags[i] <- 1
  else
#     print('false')
    tags[i] <- 0
}

# 
# file.info <- read.audio.file(filepath)
# spectra<-spectrogram(file.info[[1]],file.info[[3]])
# 
# amp<-spectra[2:dim(spectra)[1],31:246]
# results<-NMF.analysis(amp)
# 
# haha<-rbind(bristle1,bristle2,results[[2]])
# hehe<-cor(t(haha))
# 
# if(any(hehe[-1,1]>=0.3) | any(hehe[-(1:2),2]>=0.3)){
#   print('true')
# }
# else{
#   print('false')
# }