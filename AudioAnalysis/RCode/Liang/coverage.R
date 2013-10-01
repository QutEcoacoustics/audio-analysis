# calculate the cover spectrum from noise removed spectrogram amplitude
copy.noise.removed <- noise.removed
copy.noise.removed[which(copy.noise.removed > 0)] <- 1
cover.spectrum <- rowSums(copy.noise.removed) / ncol(copy.noise.removed)

rm(copy.noise.removed)