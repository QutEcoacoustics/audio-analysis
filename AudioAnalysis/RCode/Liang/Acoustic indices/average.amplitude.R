
aveAmplitude <- function(noiseRemovedAmp){
  # calculate the average amplitude
  aveSpectrum <- rowMeans(noiseRemovedAmp)
  return (aveSpectrum)
}
