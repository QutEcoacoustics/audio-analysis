AR.dataPreprocess <- function (filepath){
  species <- read.csv(filepath)
  species <- species[ , 4:ncol(species)]
  
  #convert non-zeros to 1
  index <- which(species > 0, arr.ind = TRUE)
  species[index] <- 1
  species <- apply(species, 2, factor)
  species <- data.frame(species)
  
  #convert zeros to missing values
  index <- which(species == 0, arr.ind = TRUE)
  species[index] <- NA
  
  return(species)
  
  #association rule mining
#   rules <- apriori(species, parameter=list(minlen=2, supp=0.01, conf=0.9, maxlen=10), 
#                    control=list(verbose=FALSE))
}