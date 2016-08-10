source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species4freq.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/overlapping.frequency.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/random.pair.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/calculate.freq.range.R')

overlap <- numeric()
missed.pair <- character()
threshold <- 0.1
set.seed(1)

for(i in 1:1000){
#   generate a random pair of species from one-day collection
  species.names <- random.pair(species)
  
#   #a pair of species
#   species.names <- 'Scarlet Honeyeater'
#   species.names <- c(species.names, 'Rufous Whistler')

  #find the frequency information for each species
  species1.frequency <- match.species4freq(species.names[1])
  species2.frequency <- match.species4freq(species.names[2])
  if(is.character(species1.frequency) || is.character(species2.frequency)){
    missed.pair <- c(missed.pair, species.names)
    next
  }
  else{
    #find the minimum and maximum frequency for each species
    species1.range <- calculate.freq.range(species1.frequency, threshold)
    species2.range <- calculate.freq.range(species2.frequency, threshold)

    #calculate the overlappling frequency
    overlap <- c(overlap, overlapping.frequency(species1.range, species2.range))
  }
}