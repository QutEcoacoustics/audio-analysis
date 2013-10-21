
filename <- dir(path = "c:/work/myfile/output/Towsey.Acoustic", pattern = "wav")
file.len <- length(filename)
for(i in 1:file.len){
  file.path <- paste("c:/work/myfile/output/Towsey.Acoustic/", filename[i], 
                     sep = "")
  source("C:/Work/Source/AudioAnalysis/RCode/Liang/spectrogram.R")
  source("C:/Work/Source/AudioAnalysis/RCode/Liang/acoustic.complexity.index.R")
  if(i != 1)
    ACI <- rbind(ACI,ACI.profile)
  else
    ACI <- ACI.profile
#   source("C:/Work/Source/AudioAnalysis/RCode/Liang/normalization.R")
#   source("C:/Work/Source/AudioAnalysis/RCode/Liang/noise.removal.R")
}


