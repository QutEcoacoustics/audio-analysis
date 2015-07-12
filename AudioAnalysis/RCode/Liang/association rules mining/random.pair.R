random.pair <- function(species){
#   set.seed(as.numeric(Sys.time()) %% 1000)
  
  birdNames <- names(species)
  sequence <- 1:length(birdNames)
  
  randomPair <- sample(sequence, 2)
  results <- birdNames[randomPair]
  return(results)
}