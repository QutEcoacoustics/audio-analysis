ridge <- function(amp){
  #setting up the mask matrix
  nMask <- 5
  weight <- -4
  pos <- ceiling(nMask / 2)

  verMask <- matrix(-0.1, nMask, nMask)
  verMask[ , pos] <- verMask[ , pos] * weight
  horMask <- matrix(-0.1, nMask, nMask)
  horMask[pos, ] <- horMask[pos, ] * weight

  diagMask <- diag(nMask) * (weight / 10)
  diagMask[which(diagMask==0)] <- -0.1
  revDiagMask <- diagMask[c(nMask:1), ]
  
  #create an index matrix to 
  fBin <- nMask
  frame <- nMask
  inc <- 1
  pixelIndex <- list()
  totalRow <- nrow(amp) - fBin
  totalCol <- ncol(amp) - frame
  for(i in 1:frame){
    for(j in 1:fBin){
      fBinVector <- seq(j, totalRow + j, 1)
      frameVector <- seq(i, totalCol + i, 1)
      pixelRow <- as.matrix(rep(fBinVector, length(frameVector)))
      pixelCol <- as.matrix(rep(frameVector, each=length(fBinVector)))
      pixelIndex[[inc]] <- cbind(pixelRow, pixelCol)
      inc <- inc + 1
    }
  }

  verResult <- matrix(0, totalRow + 1, totalCol + 1)
  horResult <- matrix(0, totalRow + 1, totalCol + 1)
  for(k in 1:(frame * fBin)){
    verResult <- verResult + amp[pixelIndex[[k]]] * verMask[k]
    horResult <- horResult + amp[pixelIndex[[k]]] * horMask[k]
  } 
  bigger <- verResult > horResult
  verResult[!bigger] <- horResult[!bigger]
  result <- verResult
  rm(verResult, horResult)

  
  diagResult <- matrix(0, totalRow + 1, totalCol + 1)
  revDiagResult <- matrix(0, totalRow + 1, totalCol + 1)
  for(m in 1:(frame * fBin)){
    diagResult <- diagResult + amp[pixelIndex[[m]]] * diagMask[m]
    revDiagResult <- revDiagResult + amp[pixelIndex[[m]]] * revDiagMask[m]
  }
  bigger <- diagResult > revDiagResult
  diagResult[!bigger] <- revDiagResult[!bigger]
  result1 <- diagResult
  rm(diagResult, revDiagResult)
  
  
  bigger <- result > result1
  result[!bigger] <- result1[!bigger]
  
  return(result)
}