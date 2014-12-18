bestPriorKnowledge <- function(callCountPerMin){
  #calculate the total count of bird species per minute
  temp <- as.matrix(callCountPerMin)
  temp[which(temp > 0)] <- 1
  speciesNum <- rowSums(temp)
  rm(temp)
  index <- 1:length(speciesNum)
  priorKnow <- cbind(index, speciesNum)
  
  priorKnowIndex <- sort(priorKnow[,2],decreasing=TRUE,index.return=TRUE)
  priorKnowRanked <- priorKnow[priorKnowIndex[[2]], ]
  priorCalls<-callCountPerMin[priorKnowRanked[ ,1], ]
  
  currentSpecies <- priorCalls[1, ]
  previousLen <- length(which(currentSpecies > 0))
  accumulation <- c(0, previousLen)
  
  #each time, find the minute which contains most new species
  totalMin <- nrow(callCountPerMin)
  newOrder <- 1
  for(j in 2:totalMin){
    allLens <- 0
    for(i in 2:totalMin){
      existLogic <- currentSpecies | priorCalls[i, ]
      allLens[i - 1] <- length(which(existLogic))
    }
    currentSpecies <- currentSpecies | priorCalls[which.max(allLens) + 1, ]
    currentLen <- allLens[which.max(allLens)]
    if(accumulation[length(accumulation)] == currentLen){
      break
    }else{
      accumulation <- c(accumulation, currentLen)
      newOrder <- c(newOrder, which.max(allLens) + 1)
    }
  }
  newSequence <- priorKnowRanked[ ,1][newOrder]

  result <- list(accumulation=accumulation, newSequence=newSequence)
  return(result)
}