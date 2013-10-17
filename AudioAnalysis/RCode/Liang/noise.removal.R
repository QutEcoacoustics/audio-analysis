# calculate the noise profile from each frequency bin
nfreq <- nrow(amp)
noise.profile <- rep(0,nfreq)
for(i in 1:nfreq){
# calculate the histogram
  minimum <- min(amp[i, ])
  maximum <- max(amp[i, ])
  total.bin <- round(ncol(amp) / 8)
  breaks <- seq(minimum, maximum, (maximum - minimum) / total.bin)
  histo <- hist(amp[i, ], breaks = breaks, plot = FALSE)
  counts <- histo$counts
  mids <- histo$mids
  rm(histo)

# smooth the histogram (window=5)
#   counts.len <- length(counts)
#   start.temp <- counts[c(1,2)]
#   end.temp <- counts[c(counts.len - 1, counts.len)]
#   mov.avg <- filter(counts,rep(1 / 5, 5))
#   mov.avg[c(1,2)] <- start.temp
#   mov.avg[c(counts.len - 1, counts.len)] <- end.temp
  mov.avg<-counts

# find the maximum count as modal intensity
  max.index <- which(mov.avg == max(mov.avg))
  if (max.index > total.bin * 0.95){
    modal.intensity <- mids[round(total.bin * 0.95)]
  }else{
    modal.intensity <- mids[max.index]
  }

# # calculate the standard deviation
#   loop <- 1
#   if(max.index < total.bin * 0.25){
#     while(sum(mov.avg[c((total.bin - loop):total.bin)]) <= 
#             mov.avg[max.index] * 0.68)
#       loop <- loop + 1
#     standard.deviation <- mids[total.bin - loop + 2]
#   }else{
#     while(sum(mov.avg[c(1 : loop)]) <= mov.avg[max.index] * 0.68)
#       loop <- loop + 1
#     standard.deviation <- mids[loop - 1] 
#   }
#   
# 
# # calculate the background noise for each frequency bin
#   weight <- 0
#   noise.profile[i] <- modal.intensity + weight * standard.deviation
  noise.profile[i] <- modal.intensity
}


# smooth the noise profile (window=5)
  noise.profile.len <- length(noise.profile)
  start.temp <- noise.profile[c(1,2)]
  end.temp <- noise.profile[c(noise.profile.len - 1, noise.profile.len)]
  smooth.profile <- filter(noise.profile, rep(1 / 5, 5))
  smooth.profile[c(1,2)] <- start.temp
  smooth.profile[c(noise.profile.len - 1, noise.profile.len)] <- end.temp
  smooth.profile <- as.vector(smooth.profile)
  

# calculate the noise removed spectrogram
  noise.removed <- amp - smooth.profile
  noise.removed[which(noise.removed < 0)] <- 0


##############################
# neighbourhood noise removal
f.bin <- 9
frame <- 3
inc <- 1
pixel.index <- list()
row.n <- nrow(noise.removed) - f.bin
col.n <- ncol(noise.removed) - frame

# set up a matrix indices for accelarating computation
for(i in seq(1, frame, 1)){
  for(j in seq(1, f.bin, 1)){
    f.bin.vector <- seq(j, row.n + j, 1)
    frame.vector <- seq(i, col.n + i, 1)
    pixel.row <- as.matrix(rep(f.bin.vector, length(frame.vector)))
    pixel.col <- as.matrix(rep(frame.vector, each=length(f.bin.vector)))
    pixel.index[[inc]] <- cbind(pixel.row, pixel.col)
    inc <- inc + 1
  }
}

neighbourhood.removed <- matrix(0, row.n + 1, col.n + 1)
for(k in seq(1, frame * f.bin, 1)){
  neighbourhood.removed <- neighbourhood.removed + noise.removed[pixel.index[[k]]]
}
neighbourhood.removed <- neighbourhood.removed / (f.bin * frame)
neighbourhood.removed[which(neighbourhood.removed < 3)] <- 0
noise.removed[pixel.index[[ceiling(f.bin * frame / 2)]]] <- neighbourhood.removed
################################

# clear the unnecessary variables
rm(pixel.col)
rm(pixel.row)
rm(neighbourhood.removed)

###################################
# draw the spectrogram	
# duration <- 60
# x=seq(0, duration, duration / (nframe - 1))
# y=seq(0, samp.rate / 2, samp.rate / 2 / 256)
# filled.contour(x,y,t(noise.removed),col=gray(seq(1,0,-1/49)),levels=pretty(c(0,50),50))


