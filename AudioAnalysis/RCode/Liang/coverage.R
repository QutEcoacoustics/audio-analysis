# calculate the cover spectrum from noise removed spectrogram amplitude
copy.noise.removed <- noise.removed
copy.noise.removed[which(copy.noise.removed <= 3)] <- 0
copy.noise.removed[which(copy.noise.removed > 3)] <- 1
cover.spectrum <- rowMeans(copy.noise.removed)

rm(copy.noise.removed)