source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species4freq.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/overlapping.frequency.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/calculate.freq.range.R')

species.names <- names(species)
freq.ranges <- numeric()
for(i in 1:length(species.names)){
  #find the frequency information
  species.frequency <- match.species4freq(species.names[i])
  
  #find the minimum and maximum frequency
  threshold <- 0.3
  species.range <- calculate.freq.range(species.frequency, threshold)
  range <- species.range[[2]] - species.range[[1]]
  
  #calculate the overlappling frequency
  freq.ranges <- c(freq.ranges, range)
}