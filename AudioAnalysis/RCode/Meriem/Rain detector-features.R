 # Features for rain detection in audio recording

 library('tuneR')
 library('seewave')

 FileNames <- readNames()
 
 N <- length(FileNames$all$Name)

 FileNames$all["temporal.entropy"] <- NA
 FileNames$all["spectral.entropy"] <- NA
 FileNames$all["total.entropy"] <- NA
 FileNames$all["ACIndex"] <- NA

 for (i in 1:N){
 mydata <- FileNames$all[i,] 

 pathTofile <- paste("c:/work/TestingAmbiguousFiles/", mydata[["Name"]], sep="") 
 soundFile <- readWave(pathTofile)
   
 #
 # Temporal entropy
 #
   
 # Calculate the absolute amplitude envelope of a time wave 
 ampEnvelope <- env(soundFile, f = 22050, envt = "abs", ssmooth = 50)
   
 # calculate the Temporal entropy of the time wave th
 temporalEntropy <- th(ampEnvelope)
 mydata$temporal.entropy <- temporalEntropy
  
 #
 # Shannon spectral entropy sH
 #
  
 # Calculate the Frequency spectrum of a time wave 
 freqspectrum <- spec(soundFile, f = 22050)
  
 # calculate the Spectral entropy (Shannon entropy) of the time wave sH
 spectralEntropy <- sh(freqspectrum, alpha = NULL)
 mydata$spectral.entropy <- spectralEntropy
  
 #
 # Total entropy
 #
  
 # calculate of the total entropy of the time wave H
 totalEntropy <- H(soundFile, envt = "abs")
 mydata$total.entropy <- totalEntropy
  
 #
 # Some general values and Settings
 #
  
 # Get the sampling rate
 samplingRate <- soundFile@samp.rate
  
 # Get the Nyquist frequency in Hz
 nyquistFreq <- samplingRate / 2
  
 # Get the frequency band width
 windowSize <- 512
 freqBinWidth <- samplingRate / windowSize
  
 # Get the frequency bin for each class
 freqBinIndex <- nyquistFreq / freqBinWidth
  
 # Get the number of frames
  
 soundDuration <- 5
 nonOverlaping <- 0.5
 
 temporalInterval <- windowSize * (nonOverlaping / samplingRate)
  
 # Number of frames in one seconde
 framePerSecond <- 1 / temporalInterval
  
 # Number of frames in the whole signal
 frameIndex <- soundDuration * framePerSecond
  
 #
 # Calculate the Acoustic Complexity Index ACI
 #
  
 # Get the Spectrogram for each file
 spectrogramToFile <- spectro(soundFile, ovlp = 0.5, f = 22050, palette = rev.gray.colors.1, dB = NULL, plot = FALSE )
 
 #Get the intensity values(amplitude) from the spectrogram
 amplitudeFile <- spectrogramToFile$amp 
  
 # Get the number of rows and columns in each spectrogram in other term get the dimension (amplitudeFile is a matrix)
 soundFileDimension <- dim(spectrogramToFile$amp)
  
 # Call the ACI function and store the result which is an array
 aciResult <- aci(amplitudeFile)

 #mydata$ACI.index <- aciResult$averageACI
    
 mydata$ACIndex <- aciResult$averageACI
      
 # all the features values
 FileNames$all[i,] <- mydata
  
 progress <- (i / N) * 100
 print('Progress: $progress%')
 
 }
 
 # export the results into an csv file
 write.table(FileNames$all, file="C:/Work/csv files/results/output.csv", sep=",", col.names=NA)

  
  

