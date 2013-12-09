
temporalEntropy <- function(amp){

  # calculate the entropy of each frequency bin from spectrogram
  powerSpectro <- amp ^ 2
  frequencyBin <- rowSums(powerSpectro)
  pmf <- powerDpectro / frequencyBin
  pmf[which(pmf == 0)] <- 1
  entropyProfile <- (-1) * rowSums(pmf * log2(pmf)) / log2(ncol(pmf))

  return(entropyProfile)
}