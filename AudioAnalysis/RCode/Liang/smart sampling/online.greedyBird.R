# online.greedyBird <- function(indicesFile, callFile, day = 1){
  minThreshold <- 2
  day <- 1
  indicesFile<-"C:\\Work\\myfile\\FiveDayIndices.csv"
  callFile<-"C:\\Work\\myfile\\SE\\BirdCallCounts_SERF_Oct2010\\SE_2010Oct13_Calls.csv"
  
  oct.calls <- read.csv(callFile)
  colname <- names(oct.calls)
  allCalls <- oct.calls[ , colname[4:80]]
  rm(oct.calls, colname)
  
  all.indices <- read.csv(indicesFile)
  oct.indices <- all.indices[(1+(day-1)*1440):(1440*day), ]
  cat1 <- which(oct.indices$verFeature>=0.0005083 & oct.indices$H.peakFreq.>=0.8161 & 
                  oct.indices$horFeature>=0.001624)
  cat2 <- which(oct.indices$verFeature>=0.0005083 & oct.indices$H.peakFreq.>=0.7339 & 
                  oct.indices$horFeature>=0.001624 & oct.indices$H.peakFreq.<0.8161)
  cat3 <- which(oct.indices$verFeature>=0.0005083 & oct.indices$H.peakFreq.>=0.7339 & 
                  oct.indices$horFeature<0.001624)
  cat4 <- which(oct.indices$verFeature>=0.0005083 & oct.indices$H.peakFreq.<0.7339 & 
                  oct.indices$segCount>=9)
  cat5 <- which(oct.indices$verFeature>=0.0005083 & oct.indices$H.peakFreq.<0.7339 & 
                  oct.indices$segCount<9)
  cat6 <- which(oct.indices$verFeature<0.0005083 & oct.indices$verFeature>=0.0003487)
  cat7 <- which(oct.indices$verFeature<0.0003487)
  allCats <- list(cat1,cat2,cat3,cat4,cat5,cat6,cat7)
  rm(cat1,cat2,cat3,cat4,cat5,cat6,cat7,all.indices)
    
  verFeature <- oct.indices$verFeature
  horFeature <- oct.indices$horFeature
  iH.peakFreq <- 1 - oct.indices$H.peakFreq.  
  test <- cbind(verFeature, horFeature, iH.peakFreq)
  test <- t(test)
  
  #find the instance farest from other instances as the first minute
  category <- test[ ,allCats[[1]]]
  container <- 0
  for(i in 1:ncol(category)){
    temp <- mean(sqrt(colSums((category - category[ , i])^2) / (length(category[ , i]) - 1)))
    container <- c(container, temp)
  }
  newIndex <- which.max(container) - 1
  newMatrix <- as.matrix(category[ , newIndex])
  category <- category[ , -newIndex]  
  for(kk in 1:ncol(newMatrix)){
    temp <- test - newMatrix[ , kk]
    temp <- colSums(temp)
    minute <- which(temp == 0)
  }
  mins <- minute
  #################################################################  
  
  
  for(j in 1:(length(allCats)-1)){
#   for(j in 1:3){
    if(j!=1){
      category <- test[ ,allCats[[j]]]
    }
    
    changeFlag <- 0
    ################################
    #find the subsequence minutes
    ################################
    len <- ncol(category)
    for(m in 1:(len-1)){
      for(n in 1:ncol(category)){
        temp <- mean(sqrt(colSums((newMatrix - category[ , n])^2) / length(category[ , n])))
        if(n == 1){
          container <- temp
        }
        else{
          container <- c(container, temp)
        }
      }
      newIndex <- which.max(container)
      tempVector <- category[ , newIndex]
      category <- category[ , -newIndex]
      
      #confirm the minute
      temp <- test - tempVector
      temp <- colSums(temp)
      minute <- which(temp == 0)
      
      #delete neighbourhood minutes
      judgement <- abs(mins - minute)
      if(any(judgement <= minThreshold))
        next
      else{
        newMatrix <- cbind(newMatrix, tempVector)
        mins <- c(mins, minute)
      }
      
      
      #play and verify
      calls <- allCalls[mins, ]
      samples <- nrow(calls)
      currentSpecies <- calls[1, ]
      previousLen <- length(which(currentSpecies > 0))
      accumulation <- c(0, previousLen)
      
      for(i in 2:samples){
        existLogic <- currentSpecies | calls[i, ]
        currentLen <- length(which(existLogic))
        if(currentLen > previousLen){
          changeFlag <- 0
          previousLen <- currentLen
          currentSpecies <- existLogic
        }
        else
          changeFlag <- changeFlag + 1
      }
      if(changeFlag>8){
        changeFlag <- 0
        break
      }
    }
  }
  
#   return(accumulation)
# }