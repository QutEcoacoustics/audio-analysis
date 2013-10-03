library(tuneR)

#read and shape the original signal
TFRAME <- 512
hamming <- 0.54-0.46*cos(2 * pi * c(1:TFRAME) / (TFRAME - 1))

#mono signal (left channel)
origin <- readWave('cabin_EarlyMorning4_CatBirds20091101-000000_0min.wav')
left <- origin@left
len <- length(left)
sig <- left[c(1:(len - len %% TFRAME))]
sig <- sig / (2 ^ 16 / 2)
segment <- length(sig) / TFRAME
dim(sig) <- c(TFRAME, segment)
sig <- abs(mvfft(sig * hamming))

# smooth the data
first.temp <- sig[1, ]
sig <- filter(sig, rep(1 / 3, 3))
sig[1, ] <- first.temp
amp <- sig[c(1:(TFRAME / 2)), ]

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
