sort.Filename <- function(filenames){
  index <- numeric()
  for(i in 1:length(filenames)){
    index[i] <- sub(".*_([[:digit:]]+).*", "\\1", filenames[i])
  }
  index <- as.numeric(index)
  index.sorted <- sort(index, index.return=TRUE)
  rightOrder <- filenames[index.sorted[[2]]]
  return(rightOrder)
}