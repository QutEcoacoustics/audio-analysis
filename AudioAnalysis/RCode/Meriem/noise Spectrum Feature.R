  

# Define a function for noise removal from spectrogram
getNoiseSpectrum <- function(spectrogram){
  
  # Get noise profile for a spectrogram
  #m1 <- 1
  
  numberRows <- nrow(spectrogram)
  numberColumns <- ncol(spectrogram)
  
  # Create an empty vector of background noise values
  
  numberFrequencyBins <- 256.0
  thresholdVector <- rep(0.0, numberFrequencyBins)
  #n <- 1
  
  
  # Get the histogram for each frequency bin (row)
  noiseModalToSpectrogram <- apply(spectrogram, 1, function(rowBin){
    # print(spectrogram)
    lengthRowBin <- length(rowBin)
     print(lengthRowBin)
    
    # Draw a histogram for each row
    minRow <- min(rowBin)
    maxRow <- max(rowBin)
    
    #print(minRow)
    #print(maxRow)
    
    #numberOfBuckets <- floor(lengthRowBin/7.0)
    
    numberOfBuckets <- 100
    delta <-  maxRow - minRow
    bucketWidth <- delta / numberOfBuckets
    
    # Histogram is calculated by "hist" , for each row, histogram returns a list 
    # the final histogram output is an array 
    # set up the number of bins/buckets, numberOfbuckets=30 
    histogramOutput <- histsu(rowBin, nclass = 100, plot = FALSE)$counts
     #print(histogramOutp
    
    # Sum of elements 
    somme <- sum(histogramOutput)
     #print(somme)
    
    # Draw the histogram
    barplot(histogramOutput, border = "red", col = "red", main = "Histogram of amplitudes",
            xlab = "Histogram bucket",
            ylab = "Number of elements falling into each bucket", plot = FALSE)
    
    # Smooth the histogram
    #smoothHistogram <- rollapply(histogramOutput, width = 3, FUN = mean) 
    smoothHistogram <- filterMovingAverage(histogramOutput, width = 3)
    barplot(smoothHistogram, border="green", col = "grey", main = "Histogram of amplitudes",
            xlab = "Histogram bucket",
            ylab = "Number of elements falling into each bucket", add = FALSE, plot = FALSE)
    #print(smoothHistogram)
    
    # Calculate the mean and the standard deviation
    # Find the bucketA where smoothHistogram is max 
    maxA <- max(smoothHistogram)
    #print(maxA) 
    bucketA <- which.max(smoothHistogram)
    
    # Calculate average noise/total area , sum under curve
    totalArea <- 0.0
    for (k in 1 : bucketA){
      totalArea <- totalArea + smoothHistogram[k] 
    }
    
    #print(totalArea)
    
    # Get the 68% of total area, which means 68% of all data is sitting around the mean
    #left side of the mean (bucketA)
    averageNoise <- 0.68*totalArea
    #print(averageNoiseLevel)
    
    # Calculate the standard deviation on the left side of the mean, oneSD = bucketB
    oneSD <- 0
    sum<- 0.0
    for (k in bucketA : 1){
      if(sum <  averageNoise){
        sum <- sum + smoothHistogram[k]
        oneSD <- k
      }
    }
    #print(oneSD) 
    
    # Get the bucketC/standard deviation on the right side of the mean
    N <- 1.5
    bucketC <- bucketA + (bucketA - oneSD)
    
    # Get the value of bgNoiseLevel
    noiseLevel <- minRow + (bucketC * bucketWidth)
    
    # Get theta which is the noise threshold, N=1.5
    
    thetaValues <- (bucketA + (N * oneSD)) * bucketWidth  #here was (+/- as required)
    
    
    #thresholdVector[n] <<- thetaValues
    #n <<- n+1
    
    return(thetaValues)
    
  })  # "end of apply loop"
  thresholdVector <- noiseModalToSpectrogram
  
  # Generate the x-axis
  NyquistFrequency <- 11025.0
  binWidth1 <- NyquistFrequency/numberRows
  Frequency <- seq(0, binWidth1*255, by=binWidth1)
  
  #par(mfrow=c(2,1))
  x11()
  plot( Frequency, thresholdVector, type="l", xaxs="i",yaxs = "i" ,
       main = "Background noise versus frequency", ylab="Amplitude values")
  
  
  # Smooth background noise using a moving average window size=
  smoothBgNoiseProfile <-  filterMovingAverage(thresholdVector, width = 13)
  size2<-length(smoothBgNoiseProfile)#length=256
  #print(size2)
  #print(smoothBgNoiseProfile)
  
  #Plot the original and smoothed noise in the same graph
  lines(Frequency, smoothBgNoiseProfile, type="l", col="blue")
 
  # The list return thresholdVector before and after smoothing
  
  list("smoothedBgNoise" = smoothBgNoiseProfile, "averageBgNoise" = mean(smoothBgNoiseProfile))
  
  }
  
 