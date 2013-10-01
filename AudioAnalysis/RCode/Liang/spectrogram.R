

library(tuneR)
library(seewave)

#read and shape the original signal
#mono signal (left channel)
TFRAME <- 512
hamming <- 0.54-0.46*cos(2 * pi * c(1:TFRAME) / (TFRAME - 1))

origin <- readMP3('1010130000.mp3')
left <- origin@left
len <- length(left)
sig <- left[c(1:(len - len %% TFRAME))]
segment <- length(sig) / TFRAME
dim(sig) <- c(TFRAME, segment)
sig <- abs(mvfft(sig * hamming))
amp <- sig[c(1:(TFRAME/2)), ]

# clear the unnecessary variables
rm(origin)
rm(left)
rm(sig)

######################
# draw the spectrogram
######################
# duration <- 60
# samp.freq <- 44.1
# x <- seq(0, duration, duration / (segment-1))
# y <- seq(0, samp.freq / 2, samp.freq / 2 / 255)
# filled.contour(x,y,log2(t(amp)),col=gray(seq(1,0,-1/19)),levels=pretty(c(0,20),20))
