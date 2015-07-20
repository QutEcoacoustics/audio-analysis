source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species4freq.R')


species.names <- names(species)
maximum.frequency <- numeric()
for(i in 1:length(species)){
  species.frequency <- match.species4freq(species.names[i])
  species.maximum.frequency <- which.max(species.frequency) * 11025 / 256
  maximum.frequency <- c(maximum.frequency, species.maximum.frequency)
}
