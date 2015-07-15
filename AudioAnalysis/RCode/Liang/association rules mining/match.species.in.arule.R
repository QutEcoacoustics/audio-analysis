match.species.in.arule <- function(arule){
  antecedent <- sub(".*\\{(.*)=1\\} =>.*",'\\1',arule)
  consequent <- sub(".*\\{(.*)=1\\}",'\\1',arule)
  species.names <- list(antecedent=antecedent, consequent=consequent)
  return(species.names)
}