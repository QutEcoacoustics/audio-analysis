
coverage <- function(noiseRemovedAmp){

  # calculate the cover spectrum from noise removed spectrogram amplitude
  noiseRemovedAmp[which(noiseRemovedAmp <= 3)] <- 0
  noiseRemovedAmp[which(noiseRemovedAmp > 3)] <- 1
  cover <- rowMeans(noiseRemovedAmp)

  return(cover)
}