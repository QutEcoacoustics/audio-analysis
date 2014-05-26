greedyBird <- function(test){
  
  test <- t(test)
  
  #find the instance farest from other instances
  container <- 0
  for(i in 1:ncol(test)){
    temp <- mean(sqrt(colSums((test - test[ , i])^2) / (length(test[ , i]) - 1)))
    container <- c(container, temp)
  }
  newIndex <- which.max(container) - 1
  sequence <- newIndex
  newMatrix <- as.matrix(test[ , newIndex])
  test <- test[ , -newIndex]
  
  container <- 0
  for(k in 1:ncol(test)){
    temp <- sqrt(sum((newMatrix - test[ , k])^2) / length(test[ , k]) )
    container <- c(container, temp)
  }
  newIndex <- which.max(container) - 1
  sequence <- c(sequence, newIndex)
  newMatrix <- cbind(newMatrix, test[ , newIndex])
  test <- test[ , -newIndex]
  
  #iteration on finding the farest instances from the found instances
  len <- ncol(test)
  for(m in 1:(len-1)){
    for(n in 1:ncol(test)){
      temp <- mean(sqrt(colSums((newMatrix - test[ , n])^2) / length(test[ , n])))
      if(n == 1){
        container <- temp
      }
      else{
        container <- c(container, temp)
      }
    }
    newIndex <- which.max(container)
    sequence <- c(sequence, newIndex)
    newMatrix <- cbind(newMatrix, test[ , newIndex])
    test <- test[ , -newIndex]
  }
  
  result <- newMatrix
  return(result)
}