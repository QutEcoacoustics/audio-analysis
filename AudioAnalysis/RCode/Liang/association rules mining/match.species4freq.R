match.species4freq <- function(species.name){
  species.id <- read.csv("C:/Work/myfile/species_freq_profiles/species_freq_profiles/csv/sp.csv")
  reference.freq <- read.csv("C:/Work/myfile/species_freq_profiles/species_freq_profiles/csv/dists.csv")
  
  reference.names <- tolower(as.character(species.id[ , 3]))
  reference.ids <- species.id[ , 2]
  
  species.name <- tolower(species.name)
  targeted.id <- reference.ids[which(reference.names == species.name)]
  
  col.index <- which(names(reference.freq) == paste('X', as.character(targeted.id), sep=''))
  frequency.range <- reference.freq[ , col.index]
  return(frequency.range)
}