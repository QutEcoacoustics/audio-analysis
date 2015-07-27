extract.daytime <- function(species, start.minute, end.minute){
  species <- species[start.minute:end.minute, ]
  return(species)
}