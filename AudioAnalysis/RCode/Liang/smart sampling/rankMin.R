rankMin <- function(rankedIndices, test){
  test <- t(test)
  for(kk in 1:ncol(rankedIndices)){
    #   temp <- sqrt(sum((test - haha[ , kk])^2) / length(haha[ , kk]) )
    temp <- test - rankedIndices[ , kk]
    temp <- colSums(temp)
    sequence <- which(temp == 0)
    if(kk == 1){
      container <- sequence
    }
    else{
      container <- c(container, sequence)
    }
  }
  sequence <- container
  return(sequence)
}
