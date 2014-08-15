bestPriorKnowledge <- function(callFile, totalMin, total){
  calls <- read.csv(callFile)
  
  speciesNum <- calls$callCount
  index <- 1:1440
  priorKnow <- cbind(index, speciesNum)
  rm(index,speciesNum)
  
  priorKnowIndex <- sort(priorKnow[,2],decreasing=TRUE,index.return=TRUE)
  priorKnowRanked <- priorKnow[priorKnowIndex[[2]], ]
  priorCalls<-calls[priorKnowRanked[ ,1], ]
  priorCalls<-priorCalls[ , 4:80]
  
  currentSpecies <- priorCalls[1, ]
  previousLen <- length(which(currentSpecies > 0))
  accumulation <- c(0, previousLen)
  
  #each time, find the minute which contains most new species
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
    }
  }

  result<-accumulation / total * 100
  return(result)
}