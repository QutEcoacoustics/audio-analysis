priorKnowledge <- function(filepath){
  source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/smart sampling/Accumulative Curve.R')
  calls <- read.csv(callFile)
  
  speciesNum <- calls$callCount
  index <- 1:1440
  priorKnow <- cbind(index, speciesNum)
  rm(index,speciesNum)
  
  priorKnowIndex <- sort(priorKnow[,2],decreasing=TRUE,index.return=TRUE)
  priorKnowRanked <- priorKnow[priorKnowIndex[[2]], ]
  priorCalls<-calls[priorKnowRanked[ ,1], ]
  Acurve <- drawAC(priorCalls[1:100, ], nrow(priorCalls[1:100, ]), total)
  
  return(Acurve)
}