# online.greedyBird <- function(indicesFile, callFile, day = 1){
source('c:/Work/decision tree/naughtyBird.R')
source('c:/Work/decision tree/Accumulative Curve.R')
source('c:/Work/decision tree/rankMin.R')
source('c:/Work/decision tree/deleteMin.R')
############################

#play and verify
for(count in 1:length(allCats)){
# for(count in 1:2){
  if(count == 1){
    calls <- allCalls[allCats[[count]], ]
  }
  else{
    topCategory <- test[ , rankedMin]
    supplement <- test[ , allCats[[count]]]
    len <- ncol(supplement)
    nextMin <- rankedMin
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
      topCategory <- cbind(topCategory, supplement[ , newIndex])
      tempVector <- supplement[ , newIndex]
      supplement <- supplement[ , -newIndex]
      
      temp <- test - tempVector
      temp <- colSums(temp)
      minute <- which(temp == 0)
      nextMin <- c(nextMin, minute)
    }
    nextMin <- deleteMin(nextMin, minThreshold)
    calls <- allCalls[(length(rankedMin)+1):(length(nextMin)), ]
  }

  
  for(i in 1:nrow(calls)){
    if(i == 1 & count == 1){
      currentSpecies <- calls[1, ]
      previousLen <- length(which(currentSpecies > 0))
    }
    else{
      existLogic <- currentSpecies | calls[i, ]
      currentLen <- length(which(existLogic))
      if(currentLen > previousLen){
        changeFlag <- 0
        previousLen <- currentLen
        currentSpecies <- existLogic
      }
      else
        changeFlag <- changeFlag + 1
      
      if(changeFlag == 10){
        changeFlag <- 0
        break
      }
    }
  }
  
  if(count == 1)
    rankedMin <- allCats[[count]][1:i]
  else{
    nextMin <- nextMin[-(1:length(rankedMin))]
    rankedMin <- c(rankedMin, nextMin[1:i])
  }
}