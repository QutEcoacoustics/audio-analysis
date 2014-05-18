ridgeDetection <- function(amp, threshold, smooth = TRUE, iteration = 3){
  #flip the matrix upsidedown so the smoothing will only remove low frequency part
  amp <- apply(amp, 2, rev)
  
  #setting up the mask matrix
  nMask <- 5
  weight <- -4
  pos <- ceiling(nMask / 2)

  verMask <- matrix(-0.1, nMask, nMask)
  verMask[ , pos] <- verMask[ , pos] * weight
  horMask <- matrix(-0.1, nMask, nMask)
  horMask[pos, ] <- horMask[pos, ] * weight

  diagMask <- diag(nMask) * (weight / -10)
  diagMask[which(diagMask==0)] <- -0.1
  revDiagMask <- diagMask[c(nMask:1), ]
  
  #create a matrix index to accelerate calculation
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

  #vertical and horizontal masks
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

  #two diagnal masks
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
  ridge <- result
  rm(amp, result, result1)
  
  #transform ridges into boolean value by setting up the threshold
  ridge[which(ridge<threshold)] <- 0
  ridge[which(ridge>=threshold)] <- 1
  
  # 3*3 box to remove redundancy, iterate 3 times
  if(smooth == TRUE){
    smallBox <- 3
    fBin <- smallBox
    frame <- smallBox
    for(ite in 1:iteration){
      inc <- 1
      pixelIndex <- list()
      totalRow <- nrow(ridge) - fBin
      totalCol <- ncol(ridge) - frame
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
      result <- matrix(0, totalRow + 1, totalCol + 1)
      for(k in 1:(frame * fBin)){
        result <- result + ridge[pixelIndex[[k]]]
      }
      #threshold for 3 by 3 neighborhood filter, if value <= filter, value = 0
      filter <- 2
      zeros <- (result <= filter)
      ridge <- ridge[-((nrow(ridge)-1):nrow(ridge)), ]
      ridge <- ridge[ , -((ncol(ridge)-1):ncol(ridge))]
      ridge[zeros] <- 0
    }
    #complement the low frequency part so the matrix size is consistant
    complement<- matrix(0, (nMask - 1) + (smallBox - 1) * iteration, ncol(ridge))
    ridge <- rbind(ridge, complement)
  }

  ridge <- apply(ridge, 2, rev)
  ridge <- t(ridge)
  return(ridge)
}