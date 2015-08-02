find.frequency.maximum <- function(species.names){
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species4freq.R')

maximum.frequency <- numeric()
missed.species <- character()

for(i in 1:length(species.names)){
  #match the species names to its frequency information
  species.frequency <- match.species4freq(species.names[i])
  if(is.character(species.frequency)){
    missed.species <- c(missed.species, species.frequency)
    next
  }
  else{
    species.maximum.frequency <- which.max(species.frequency) #* 11025 / 256
    maximum.frequency <- c(maximum.frequency, species.maximum.frequency)
  }
}
return(maximum.frequency)
}