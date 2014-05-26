randomSampling <- function(nsample){
  source('C:/Work/decision tree/Accumulative Curve.R')
  
  set.seed(as.numeric(Sys.time()) %% 1000)
  sampleMin <- 1:1435
  
  oct13.calls <- read.csv("C:\\Work\\myfile\\SE\\BirdCallCounts_SERF_Oct2010\\SE_2010Oct13_Calls.csv")
  colname <- names(oct13.calls)
  calls <- oct13.calls[ ,colname[4:80]]

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