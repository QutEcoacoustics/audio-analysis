
filename <- dir(path = "c:/work/myfile/output/Towsey.Acoustic", pattern = "wav")
file.len <- length(filename)
for(cnt in 1:file.len){
  file.path <- paste("c:/work/myfile/output/Towsey.Acoustic/", filename[cnt], 
                     sep = "")
  source("C:/Work/Source/AudioAnalysis/RCode/Liang/spectrogram.R")
#   source("C:/Work/Source/AudioAnalysis/RCode/Liang/acoustic.complexity.index.R")
#   source("c:/Work/Source/AudioAnalysis/RCode/Liang/spectro.entropy.R")
#   if(cnt != 1)
#     entropy <- rbind(entropy,spectro.entropy)
#   else
#     entropy <- spectro.entropy
  source("C:/Work/Source/AudioAnalysis/RCode/Liang/normalisation.R")
  source("C:/Work/Source/AudioAnalysis/RCode/Liang/noise.removal.R")
  source("C:/Work/Source/AudioAnalysis/RCode/Liang/coverage.R")
  source("C:/Work/Source/AudioAnalysis/RCode/Liang/average.amplitude.R")
  if(cnt != 1)
    cover <- rbind(cover, cover.spectrum)
  else
    cover <- cover.spectrum
  if(cnt != 1)
    avg <- rbind(avg, average.spectrum)
  else
    avg <- average.spectrum
}


