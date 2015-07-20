match.species4freq <- function(species.name){
  #read frequency information, file names: dists.csv, sp.csv 
  species.id <- read.csv("C:/Work/myfile/species_freq_profiles/species_freq_profiles/csv/sp.csv")
  reference.freq <- read.csv("C:/Work/myfile/species_freq_profiles/species_freq_profiles/csv/dists.csv")
  
  #match the species name and the species id
  species.name <- tolower(species.name)
  reference.names <- tolower(as.character(species.id[ , 3]))
  reference.ids <- species.id[ , 2]
  targeted.id <- reference.ids[which(reference.names == species.name)]
  
  #recording missed species
  if(length(targeted.id) == 0){
    return(species.name)
  }
  else{
    #find the corresponding frequency information
    col.index <- which(names(reference.freq) == paste('X', as.character(targeted.id), sep=''))
    frequency.range <- reference.freq[ , col.index]
    return(frequency.range)
  }
}