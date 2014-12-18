randomSampling <- function(calls, nsample){
  source('C:/Work/decision tree/Accumulative Curve.R')
  
  set.seed(as.numeric(Sys.time()) %% 1000)
  sampleMin <- 1:nrow(calls)

  mins <- sample(sampleMin, length(sampleMin))
  randomResult <- calls[mins, ]
  Acurve <- as.matrix(drawAC(randomResult, nsample))
  for(i in 2:10){
    mins <- sample(sampleMin, length(sampleMin))
    randomResult <- calls[mins, ]
    temp <- drawAC(randomResult, nsample)
    Acurve <- cbind(Acurve, temp)
  }
  Acurve <- rowMeans(Acurve)
  
  return(Acurve)
}