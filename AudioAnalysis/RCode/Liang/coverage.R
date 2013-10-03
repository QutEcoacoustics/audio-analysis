# calculate the cover spectrum from noise removed spectrogram amplitude
copy.noise.removed <- noise.removed
copy.noise.removed[which(copy.noise.removed > 0.015)] <- 1
copy.noise.removed[which(copy.noise.removed <= 0.015)] <- 0
cover.spectrum <- rowMeans(copy.noise.removed)

rm(copy.noise.removed)