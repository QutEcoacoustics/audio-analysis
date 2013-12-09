runAnalysis <- function(){
  filename <- dir(path = "c:/work/myfile/IndexConbination/Towsey.Acoustic", pattern = "wav")
  file.len <- length(filename)
  for(cnt in 1:file.len){
    file.path <- paste("c:/work/myfile/IndexConbination/Towsey.Acoustic/", filename[cnt], 
                     sep = "")
    source("C:/Work/Source/AudioAnalysis/RCode/Liang/spectrogram.R")
    source("C:/Work/Source/AudioAnalysis/RCode/Liang/acoustic.complexity.index.R")
    source("c:/Work/Source/AudioAnalysis/RCode/Liang/spectro.entropy.R")
    if(cnt != 1)
      ACI <- rbind(ACI,ACI.profile)
    else
      ACI <- ACI.profile
    if(cnt != 1)
      entropy <- rbind(entropy,spectro.entropy)
    else
      entropy <- spectro.entropy
    #   source("C:/Work/Source/AudioAnalysis/RCode/Liang/normalisation.R"
    #   source("C:/Work/Source/AudioAnalysis/RCode/Liang/noise.removal.R")
    #   source("C:/Work/Source/AudioAnalysis/RCode/Liang/coverage.R")
    #   source("C:/Work/Source/AudioAnalysis/RCode/Liang/average.amplitude.R")
    #   if(cnt != 1)
    #     cover <- rbind(cover, cover.spectrum)
    #   else
    #     cover <- cover.spectrum
    #   if(cnt != 1)
    #     avg <- rbind(avg, average.spectrum)
    #   else
    #     avg <- average.spectrum
  }
  
  # write.csv(cover,file='c:/Work/myfile/IndexConbination/cover.csv',row.names=FALSE)
  # write.csv(avg,file='c:/Work/myfile/IndexConbination/avg.csv',row.names=FALSE)
  write.csv(entropy,file='c:/Work/myfile/IndexConbination/entropy.csv',row.names=FALSE)
  write.csv(ACI,file='c:/Work/myfile/IndexConbination/ACI.csv',row.names=FALSE)
}