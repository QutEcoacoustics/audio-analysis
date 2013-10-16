window.power <- sum(hamming ^ 2)
epsilon <- (1 / 2) ^ (16 - 1)
min.dc <- 10 * log10(epsilon ^ 2 / window.power / samp.rate)
min.freq <- 10 * log10(epsilon ^ 2 / window.power / samp.rate * 2)

# normalise DC values
dc.row <- amp[1, ]
dc.row[which(dc.row <  epsilon)] <- min.dc
dc.row[which(dc.row >= epsilon)] <- 10 * log10(dc.row[which(dc.row >= epsilon)] ^ 
                                                 2 / window.power / samp.rate)
amp[1, ] <- dc.row
rm(dc.row)

# normalise Nyquist values
ny.row <- amp[nrow(amp), ]
ny.row[which(ny.row <  epsilon)] <- min.dc
ny.row[which(ny.row >= epsilon)] <- 10 * log10(ny.row[which(ny.row >= epsilon)] ^ 
                                                 2 / window.power / samp.rate)
amp[nrow(amp), ] <- ny.row
rm(ny.row)

# normalise frequency components
fq.row <- amp[c(2 : (nrow(amp) - 1)), ]
fq.row[which(fq.row <  epsilon)] <- min.freq
fq.row[which(fq.row >= epsilon)] <- 10 * log10(fq.row[which(fq.row >= epsilon)] ^ 
                                                 2 / window.power / samp.rate)
amp[c(2 : (nrow(amp) - 1)), ] <- fq.row
rm(fq.row)