
removeNoise <- function(amp){

  # calculate the noise profile from each frequency bin
  nfreq <- nrow(amp)
  noiseProfile <- rep(0,nfreq)
  for(i in 1:nfreq){
    # calculate the histogram
    minimum <- min(amp[i, ])
    maximum <- max(amp[i, ])
    totalBin <- round(ncol(amp) / 8)
    breaks <- seq(minimum, maximum, (maximum - minimum) / totalBin)
    histo <- hist(amp[i, ], breaks = breaks, plot = FALSE)
    counts <- histo$counts
    mids <- histo$mids
    
    # smooth the histogram (window=5)
    countsLength <- length(counts)
    startTemp <- counts[c(1,2)]
    endTemp <- counts[c(countsLength - 1, countsLength)]
    movAvg <- filter(counts, rep(1 / 5, 5))
    movAvg[c(1, 2)] <- startTemp
    movAvg[c(countsLength - 1, countsLength)] <- endTemp

    # find the maximum count as modal intensity
    maxIndex <- which(movAvg == max(movAvg))
    if (maxIndex > totalBin * 0.95){
      modalIntensity <- mids[round(totalBin * 0.95)]
    }
    else{
      modalIntensity <- mids[maxIndex]
    }

    #   calculate the standard deviation
    #   loop <- 1
    #   if(maxIndex < totalBin * 0.25){
    #     while(sum(movAvg[c((totalBin - loop):totalBin)]) <= 
    #             movAvg[maxIndex] * 0.68)
    #       loop <- loop + 1
    #     standard.deviation <- mids[totalBin - loop + 2]
    #   }else{
    #     while(sum(movAvg[c(1 : loop)]) <= movAvg[maxIndex] * 0.68)
    #       loop <- loop + 1
    #     standard.deviation <- mids[loop - 1] 
    #   }
    #   
    # 
    #   calculate the background noise for each frequency bin
    #   weight <- 0
    #   noiseProfile[i] <- modalIntensity + weight * standard.deviation
    noiseProfile[i] <- modalIntensity
  }


  # smooth the noise profile (window=5)
  noiseProfileLen <- length(noiseProfile)
  startTemp <- noiseProfile[c(1,2)]
  endTemp <- noiseProfile[c(noiseProfileLen - 1, noiseProfileLen)]
  smoothProfile <- filter(noiseProfile, rep(1 / 5, 5))
  smoothProfile[c(1, 2)] <- startTemp
  smoothProfile[c(noiseProfileLen - 1, noiseProfileLen)] <- endTemp
  smoothProfile <- as.vector(smoothProfile)
  

  # calculate the noise removed spectrogram
  noiseRemovedAmp <- amp - smoothProfile
  noiseRemovedAmp[which(noiseRemovedAmp < 0)] <- 0


  ##############################
  # neighbourhood noise removal
  fBin <- 9
  frame <- 3
  inc <- 1
  pixelIndex <- list()
  totalRow <- nrow(noiseRemovedAmp) - fBin
  totalCol <- ncol(noiseRemovedAmp) - frame

  # set up a matrix indices for accelarating computation
  for(i in seq(1, frame, 1)){
    for(j in seq(1, fBin, 1)){
      fBinVector <- seq(j, totalRow + j, 1)
      frameVector <- seq(i, totalCol + i, 1)
      pixelRow <- as.matrix(rep(fBinVector, length(frameVector)))
      pixelCol <- as.matrix(rep(frameVector, each=length(fBinVector)))
      pixelIndex[[inc]] <- cbind(pixelRow, pixelCol)
      inc <- inc + 1
    }
  }

  neighbourhoodRemoved <- matrix(0, totalRow + 1, totalCol + 1)
  for(k in seq(1, frame * fBin, 1))
    neighbourhoodRemoved <- neighbourhoodRemoved + noiseRemovedAmp[pixelIndex[[k]]]
  neighbourhoodRemoved <- neighbourhoodRemoved / (fBin * frame)
  neighbourhoodRemoved[which(neighbourhoodRemoved < 3)] <- 0
  noiseRemovedAmp[pixelIndex[[ceiling(fBin * frame / 2)]]] <- neighbourhoodRemoved

  # draw the spectrogram	
  # duration <- 60
  # x=seq(0, duration, duration / (nframe - 1))
  # y=seq(0, sampleRate / 2, sampleRate / 2 / 256)
  # filled.contour(x,y,t(noiseRemovedAmp),col=gray(seq(1,0,-1/49)),levels=pretty(c(0,50),50))

  result <- list(noiseProfile=noiseProfile, noiseRemovedAmp=noiseRemovedAmp)
  return(result)
}
