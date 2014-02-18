ridgeDetectionS <- function(amp, threshold, maskType, smooth = TRUE, iteration = 3){
  #flip the matrix upsidedown so the smoothing will only remove low frequency part
  amp <- apply(amp, 2, rev)
  
  #setting up the mask matrix
  nMask <- 5
  weight <- -4
  pos <- ceiling(nMask / 2)
  
  mask <- matrix(-0.1, nMask, nMask)
  if(maskType == 'vertical'){
    mask[ , pos] <- mask[ , pos] * weight
  }
  else if(maskType == 'horizontal'){
    mask[pos, ] <- mask[pos, ] * weight
  }
  else if(maskType == 'diagonal'){
    mask <- diag(nMask) * (weight / -10)
    mask[which(mask==0)] <- -0.1
  }
  else if(maskType == 'revDiagonal'){
    mask <- diag(nMask) * (weight / -10)
    mask[which(mask==0)] <- -0.1
    mask <- mask[c(nMask:1), ]
  }
  else{
    print('wrong mask type')
    return()
  }
  
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
  
  ridge <- matrix(0, totalRow + 1, totalCol + 1)
  for(k in 1:(frame * fBin)){
    ridge <- ridge + amp[pixelIndex[[k]]] * mask[k]
  }
  
  #transform ridges into boolean value by setting up the threshold
  ridge[which(ridge<threshold)] <- 0
  ridge[which(ridge>=threshold)] <- 1
  
  # 3*3 box to remove redundancy, iterate 3 times
  if(smooth == TRUE){
    smallBox <- 7
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
  }
  
  ridge <- apply(ridge, 2, rev)
  ridge <- t(ridge)
  
  return(ridge)
}