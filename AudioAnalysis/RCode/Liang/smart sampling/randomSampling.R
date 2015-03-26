randomSampling <- function(callCountPerMin, nsample){
  source('C:/Work/decision tree/Accumulative Curve.R')
  
  set.seed(as.numeric(Sys.time()) %% 1000)
  sampleMin <- 1:nrow(callCountPerMin)

  mins <- sample(sampleMin, length(sampleMin))
  randomResult <- callCountPerMin[mins, ]
  Acurve <- as.matrix(drawAC(randomResult, nsample))
  
  #iterate 9 times and calculate the mean
  for(i in 2:1000){
    mins <- sample(sampleMin, length(sampleMin))
    randomResult <- callCountPerMin[mins, ]
    temp <- drawAC(randomResult, nsample)
    Acurve <- cbind(Acurve, temp)
  }
  AcurveMean <- rowMeans(Acurve)
  AcurveStd <- apply(Acurve, 1, sd)
  
  Acurve <- list(AcurveMean = AcurveMean, AcurveStd = AcurveStd)
  return(Acurve)
}