# calculate the entropy of each frequency bin from spectrogram
power.spectro <- amp^2
f.bin.total <- rowSums(power.spectro)
pmf <- power.spectro / f.bin.total
pmf[which(pmf == 0)] <- 1
spectro.entropy <- (-1) * rowSums(log2(pmf)) / log2(ncol(pmf))

rm(power.spectro)