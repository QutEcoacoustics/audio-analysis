priorKnowledge <- function(filepath){
  oct <- read.csv(filepath)
  
  speciesNum <- oct$speciesNum
  index <- 1:1440
  priorKnow <- cbind(index, speciesNum)
  rm(index,speciesNum)
  
  priorKnowIndex <- sort(priorKnow[,2],decreasing=TRUE,index.return=TRUE)
  priorKnowRanked <- priorKnow[priorKnowIndex[[2]], ]
  priorCalls<-calls[priorKnowRanked[ ,1], ]
  Acurve <- drawAC(priorCalls[1:100, ], nrow(priorCalls[1:100, ]))
  
  return(Acurve)
}