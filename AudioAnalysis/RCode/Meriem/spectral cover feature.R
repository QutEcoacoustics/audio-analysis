
   # Load the require pachages
   library(tuneR)
   library(seewave)
   library(zoo)
   library(nnet)
   library(GLDEX)
   library(grDevices)
   library(graphics)
   library(fftw)
   
   soundFile0 <- ("DM420036_min599 No Rain-Birds.wav_split-01.wav")
   soundFile1 <- ("7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_min287 No Rain-Birds.wav_split-03.wav")
   soundFile2 <- ("7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_min611 No Rain-Birds.wav_split-11.wav")
   soundFile3 <- ("RAIN_Groote Golf Course_20110406-090000 Heavy Rain-No Birds.wav_split-12.wav")
   soundFile4 <- ("DM420036_min1031_Heavy Rain-Birds.wav_split-05.wav")
   soundFile5 <- ("7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_min1100_Cicadas.wav_split-15.wav")
   soundFile6 <- ("7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_min611 No Rain-Birds.wav_split-11.wav")
   soundFile <- readWave(soundFile3)
   
   #streoToMono <- Wave(left=soundFile1, samp.rate=22050, bit=16)
   
   # Get the amplitude of Spectrogram for each file
   #spectrogramToFile_1 <- spectro(soundFile, palette = rev.gray.colors.2, plot = TRUE, main = "original signal")
  # amplitude <- spectrogramToFile_1$amp
  # print(dim(amplitude))
  # print(amplitude[100,100])
   #print(amplitude[50,10])
   
  
   
   #spectrogramToFile <- stft.ext(file="DM420036_min599 No Rain-Birds.wav_split-01.wav", ovlp=0, mean=FALSE, dB= TRUE)  
   #spectrogramToFile <- stft.ext(file="7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_min287 No Rain-Birds.wav_split-03.wav", dB=TRUE, mean=FALSE)
   
   # take one channel rather than two
   #spectrogramToFile <- stft.ext(file=("RAIN_Groote Golf Course_20110406-090000 Heavy Rain-No Birds.wav_split-12.wav"),verbose=TRUE, ovlp=0, mean=FALSE, dB= TRUE)
   
   
   # band-stop
   #e<-fir(soundFile,f=22050, from=11025, to=9000)
   
   spectrogramToFile <- spectro(soundFile, palette = rev.gray.colors.2, plot = TRUE, dB=NULL, main = "original signal")
   #print(dim(spectrogramToFile))  
   
   #Get the intensity values(amplitude) from the spectrogram
   time <- spectrogramToFile[1]
   frequency <- spectrogramToFile[2]
   amplitudeFile <- spectrogramToFile$amp 
   #print(dim(amplitudeFile))
   samplingFrequency <- soundFile@samp.rate
   
    
   
   
   
    # Transpose of a matrix
    transposeMatrix2 <- t(amplitudeFile)[ncol(amplitudeFile):1 , ]   
    ## flip a matrix vertically
   flippingMatrix2 <- apply( transposeMatrix2, 2, rev)
    x11()
    image(flippingMatrix2 ,axes = FALSE, xaxs='i', yaxs='i', col = grey(seq(1,0, , length = 256)), 
          main = "signal before noise removal") 
    
    # Call noise spectrum function
    output <- getNoiseSpectrum(amplitudeFile) 
   
   # Substract bg noise from every value in the bin #
   numberRows <- nrow(amplitudeFile)
   numberColumns <- ncol(amplitudeFile)
  
    # Create an empty matrix
   signalAfterNoiseRemoval <- matrix(0, nrow= numberRows, ncol=numberColumns)
   
   for (i in 1 : numberRows){
     
     for (j in 1 : numberColumns){
       
       signalAfterNoiseRemoval[i, j] <- amplitudeFile[i, j] - output$smoothedBgNoise[i]
       #print(signalAfterNoiseRemoval[i,j])
       
       # Set the negative numbers in the matrix to zero
       if(signalAfterNoiseRemoval[i, j] < 0){
         signalAfterNoiseRemoval[i, j] <- 0.0
                 
       } 
     }       
   }
   #print(signalAfterNoiseRemoval)
   #print(dim(signalAfterNoiseRemoval))
   
   # use the output of signal after noise removal
   mySignal <- signalAfterNoiseRemoval
    
   # set up the display of spectrogram
   # Transpose of a matrix
   transposeMatrix <- t(mySignal)[ncol(mySignal):1 , ]
  
   ## flip a matrix vertically
   flippingMatrix <- apply( transposeMatrix, 2, rev)
   x11()
   image(flippingMatrix ,axes = FALSE, col = grey(seq(1,0, length = 256)), main = "signal after noise removal")
   
   # plot.figure <- par(mfrow=c(2,1))
    
   #
   # Spectral Cover: this indice calculates the fraction of spectrogram cells in each frequency bin,
   #where the spectral amplitude exceeds a threshold theta=0.015
   #
   nRows <- nrow(mySignal)
   #print(numberRows)
   nColumns <- ncol(mySignal)
   #print(numberColumns)
   theta= 0.015
   spectralCover <- rep (0.0,  nRows)
   
   for (ii in 1 : nRows){
     for (jj in 1 :   nColumns){
       if (mySignal[ii, jj] > theta){
         spectralCover[ii] <-   spectralCover[ii] + 1 #length=256#
       }  
     }
   }
   
   #print(spectralCover)
   
   
   # Calculate the percentage of the celles where the spectral amplitude exceeds  Theta
   spectralCoverArray <- rep(0.0, nRows)
   for (n in 1 : nRows){   
       spectralCoverArray[n] <- (spectralCover[n] /  nRows) # * 100
   }
   #print(spectralCoverArray) #length=256
   
   # Normalization of spectral cover array
   # print(dim(mySignal))
    #print(ncol(mySignal))
    #normalizedSpectralCover <- rep(0.0, length(spectralCoverArray))
   
    #highestValue <- max(spectralCoverArray)
    #print( highestValue )
   # Calculate the number of frames
    soundDuration <- (1/samplingFrequency)*length(soundFile)
    windowLength <- 512.0
    timeStep <-  windowLength / samplingFrequency
    numberOfFrames <- soundDuration/ timeStep
   print(numberOfFrames)
    for (n in 1 : length(spectralCoverArray)){
      
        normalizedSpectralCover[n] <- spectralCoverArray[n] / numberOfFrames
        #print(normalizedSpectralCover[n])
        
         #print( normalizedSpectralCover[n])
    }
    
   
    
   # Plot of Frequency versus % Cover spectrum 
    # Generate the y-axis
    NyquistFrequency <- samplingFrequency/2
    binWidth1 <- NyquistFrequency/numberRows
    Frequency <- seq(0, binWidth1*255, by=binWidth1) 
    x11()
   plot(spectralCoverArray, Frequency, type="l", main="  Cover for spectrogram")
   
   
   
   
  
  
 
  









