calculate.freq.range <- function(frequency.range, threshold){
  cutoff.amp <- max(frequency.range) * threshold
  available.range <- which(frequency.range >= cutoff.amp, arr.ind=TRUE)
  lower.freq <- available.range[1] * 11025 / 256
  upper.freq <- available.range[length(available.range)] * 11025 / 256
  freq.limitation <- list(lower.freq = lower.freq, upper.freq = upper.freq)
  return(freq.limitation)
}