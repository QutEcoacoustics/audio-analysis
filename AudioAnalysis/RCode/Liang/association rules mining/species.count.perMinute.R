species.count.perMinute <- function(filepath){
  species <- read.csv(filepath)
  species <- species[ , 4:ncol(species)]
  
  species.count <- rowSums(species)
  return(species.count)
}