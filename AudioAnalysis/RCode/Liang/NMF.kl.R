library(NMF)

source("C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/false-colour/spectrogram.R")

filepath<-"C:\\Work\\myfile\\30s Recording\\bickerton_island_1013_255205_20131211_193531_30_0.wav"

#read signals
sig<-spectrogram(filepath)
amp<-sig$amp
amp<-t(amp)

#remove low frequency (<1000Hz) and high frequency (>8480Hz) noise
# amp<-amp[,31:246]

#remove noise
# temp.amp<-removeNoise(amp)
# amp<-temp.amp[[2]]

#randomise the matrix
row.no<-sample(nrow(amp))
col.no<-sample(ncol(amp))
# random.amp<-amp[row.no, ]
random.amp<-amp[ , col.no]

#initialisation
diff.residual <- 0
diff.random.residual <- 0
r <- 1
residual<-numeric()
random.residual<-numeric()

#calculate non-negative matrix factorisation
while(diff.residual >= diff.random.residual){
  factorisation <- nmf(amp, rank=r, method='snmf/l', seed=12345)
  factorisation.random <- nmf(random.amp, rank=r, method='snmf/l', seed=12345)
  residual[r] <- factorisation@residuals
  random.residual[r] <- factorisation.random@residuals
  if (r>1){
    diff.residual <- residual[r-1] - residual[r]
    diff.random.residual <- random.residual[r-1] - random.residual[r]
  }
  r <- r + 1
}

mydata.h<-basis(factorisation)
mydata.w<-coef(factorisation)