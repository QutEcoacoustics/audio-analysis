sort.filename <- function(filename){
  index <- numeric()
  for(i in 1:length(filename)){
    index[i] <- sub(".*_([[:digit:]]+).*", "\\1", filename[i])
  }
  index <- as.numeric(index)
  index.sorted <- sort(index, index.return=TRUE)
  rightOrder <- filename[index.sorted[[2]]]
  return(rightOrder)
}