
spectrogram <- function(signal, bit, TFRAME=512){

  #read and shape the original signal
  hamming <- 0.54 - 0.46 * cos(2 * pi * c(1:TFRAME) / (TFRAME - 1))
  
  #segment the signal to an integer number of frames
  len <- length(signal)
  sig <- signal[c(1:(len - len %% TFRAME))]
  
  #normalised by the maximum signed value of 16 bit
  sig <- sig / (2 ^ (bit - 1))
  
  #reshape the signal to a matrix for computational convenience
  nframe <- length(sig) / TFRAME
  dim(sig) <- c(TFRAME, nframe)
  sig <- Mod(mvfft(sig * hamming))

  # smooth the amplitudes value
  temp <- sig[1, ]
  sig <- filter(sig, rep(1 / 3, 3))
  sig[1, ] <- temp
  
  # subset the valid frequency based on Nyquist theory
  amp <- sig[c(1:(TFRAME / 2)), ]
  amp <- t(amp)

  return(amp)
  
  ######################
  # draw the spectrogram
  ######################
  # duration <- 60
  # x <- seq(0, duration, duration / (nframe-1))
  # y <- seq(0, sampRate / 2, sampRate / 2 / 256)
  # filled.contour(x,y,t(amp),col=gray(seq(1,0,-1/19)),levels=pretty(c(0,20),20))
}