# calculate the average amplitude
average.spectrum <- rowMeans(noise.removed)
# copy.noise.removed <- noise.removed
# copy.noise.removed[which(copy.noise.removed == 0)] <- 1
# average.spectrum <- rowMeans(log2(copy.noise.removed ^ 2))

# rm(copy.noise.removed)