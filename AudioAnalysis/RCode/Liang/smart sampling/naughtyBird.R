naughtyBird <- function(topCategory, supplement){
  topCategory <- t(topCategory)
  supplement <- t(supplement)
  #iteration on finding the farest instances from the found instances
  len <- ncol(supplement)
  for(m in 1:(len-1)){
    for(n in 1:ncol(supplement)){
      temp <- mean(sqrt(colSums((topCategory - supplement[ , n])^2) / length(supplement[ , n])))
      if(n == 1){
        container <- temp
      }
      else{
        container <- c(container, temp)
      }
    }
    newIndex <- which.max(container)
#     sequence <- c(sequence, newIndex)
    topCategory <- cbind(topCategory, supplement[ , newIndex])
    supplement <- supplement[ , -newIndex]
  }
  
  result <- topCategory
  return(result)
}