AR.dataPreprocess <- function (filepath){
  species <- read.csv(filepath)
  species <- species[ , 4:ncol(species)]
  
  #make zeros to NA and non-zeros to one
  index1 <- which(species == 0, arr.ind = TRUE)
  species[index1] <- 'no'
  index2 <- which(species > 0, arr.ind = TRUE)
  species[index2] <- 'yes'
  species <- apply(species, 2, as.factor)
  species[index1] <- NA
  
  return(species)
  
  #association rule mining
#   rules <- apriori(species, parameter=list(minlen=2, supp=0.01, conf=0.9, maxlen=10), 
#                    control=list(verbose=FALSE))
}