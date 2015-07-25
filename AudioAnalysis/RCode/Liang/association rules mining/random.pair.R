random.pair <- function(birdNames){
#   set.seed(as.numeric(Sys.time()) %% 1000)
  
  #generate a sequence with the same length as the number of all species
  sequence <- 1:length(birdNames)
  
  #pick up two indexes from the sequence at random, refering to a pair of species
  randomPair <- sample(sequence, 2)
  results <- birdNames[randomPair]
  return(results)
}