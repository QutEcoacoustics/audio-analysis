count.occurrence <- function(species){
  species.row <- nrow(species)
  species.col <- ncol(species)
  counts <- matrix(rep(0, species.row), species.row, species.col)
  presence.index <- which(species == 1, arr.ind = TRUE)
  counts[presence.index] <- 1
  occurances <- colSums(counts)
  return(occurances)
}