# calculate the ACI of each frequency bin from spectrogram
spectro.difference <- t(diff(t(amp)))
ACI.profile <- rowSums(abs(spectro.difference)) / rowSums(amp)

rm(spectro.difference)