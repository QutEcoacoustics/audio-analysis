
ACI <- function(amp){
  # calculate the ACI of each frequency bin from spectrogram
  spectroDifference <- t(diff(t(amp)))
  ACIprofile <- rowSums(abs(spectroDifference)) / rowSums(amp)

  return(ACIprofile)
}