
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/false-colour/read.audio.file.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/false-colour/spectrogram.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/Non-negative matrix factorisation/NMF.analysis.R')

load("C:/Users/n8781699/Downloads/Eastern bristle bird/bristle1.RData")
load("C:/Users/n8781699/Downloads/Eastern bristle bird/bristle2.RData")
# load("C:/Users/n8781699/Downloads/Eastern bristle bird/bristle.templates.RData")

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
  
#   haha<-rbind(bristle.templates, results[[2]])
  haha<-rbind(bristle1,bristle2,results[[2]])
  hehe<-cor(t(haha))
  hehe<-round(hehe, digits=2)
  
  threshold <- 0.3
#   if(any(hehe[8:nrow(hehe), 1:7]>=threshold))
  if(any(hehe[(nrow(hehe)-nrow(results[[2]])+1):nrow(hehe), 1:(nrow(hehe)-nrow(results[[2]]))]>=threshold))
    #     print('true')
    tags[i] <- 1
  else
    #     print('false')
    tags[i] <- 0

}


TP.rate <-sum(tags[1:10])/10
FP.rate <- sum(tags[11:20])/10

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