
amp2dB <- function (amp, sampleRate, bit){

#   windowPower <- sum(hamming ^ 2)
  epsilon <- (1 / 2) ^ (bit - 1)
  minDC <- 10 * log10(epsilon ^ 2 / sampleRate)
  minFrequency <- 10 * log10(epsilon ^ 2 / sampleRate * 2)

  # normalise DC values
  DC <- amp[1, ]
  DC[which(DC <  epsilon)] <- minDC
  DC[which(DC >= epsilon)] <- 10 * log10(DC[which(DC >= epsilon)] ^ 2 / sampleRate)
  amp[1, ] <- DC


  # normalise frequency components
  freqComp <- amp[2 : nrow(amp), ]
  freqComp[which(freqComp <  epsilon)] <- minFrequency
  freqComp[which(freqComp >= epsilon)] <- 10 * log10(freqComp[which(freqComp >= epsilon)] ^ 
                                                 2 / sampleRate * 2)
  amp[2 : nrow(amp), ] <- freqComp

  #   normalise Nyquist values
  #   Nyquist <- amp[nrow(amp), ]
  #   Nyquist[which(Nyquist <  epsilon)] <- minDC
  #   Nyquist[which(Nyquist >= epsilon)] <- 10 * log10(Nyquist[which(Nyquist >= epsilon)] ^
  #                                                  2 / sampleRate)
  #   amp[nrow(amp), ] <- Nyquist
  return(amp)
}