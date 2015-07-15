source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species4freq.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/overlapping.frequency.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/random.pair.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/find.freq.range.R')

species.names <- names(species)
# maximum.frequency <- numeric()
# for(i in 1:length(species)){
#   species.frequency <- match.species4freq(species.names[i])
#   species.maximum.frequency <- which.max(species.frequency) * 11025 / 256
#   maximum.frequency <- c(maximum.frequency, species.maximum.frequency)
# }

frequency.range <- numeric()
for(i in 1:length(species)){
  species.frequency <- match.species4freq(species.names[i])
  threshold <- 0.2
  species.range <- find.freq.range(species.frequency, threshold)
  species.freq.range <- species.range[[2]] - species.range[[1]]
  frequency.range <- c(frequency.range, species.freq.range)
}