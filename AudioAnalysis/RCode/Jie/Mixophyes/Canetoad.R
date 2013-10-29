##################################################################################
####                        CANETOAD RECOGNITION
##################################################################################

setwd("C:/projects/Canetoad")

##################################################################################
####                        CANETOAD PARAMETER
##################################################################################

CANETOAD_MIN_HZ <- 400
CANETOAD_MAX_HZ <- 900
####    duration of DCT in seconds
CANETOAD_DCT_DURATION <- 0.5
####    minimum acceptble value of a DCT coefficients
CANETOAD_DCT_THRESHOLD <- 0.6
CANETOAD_MIN_OSCI_FREQ <- 10
CANETOAD_MAX_OSCI_FREQ <- 15
CANETOAD_MIN_DURATION <- 3.0
CANETOAD_MAX_DURATION <- 60.0
CANETOAD_EVENT_THRESHOLD <-0.4

#################################################################################
#                 set the parameter for producing spectrogram
#################################################################################
TFRAME <- frameSize <- 1024
hamming <- 0.54-0.46*cos(2 * pi * c(1:TFRAME) / (TFRAME - 1))

library(tuneR)
# cane_recording <- readMP3('toad_test.mp3')
cane_recording <- readWave('DM420008_262m_00s__264m_00s - Faint Toad.wav')

#cane_recording <- readWave('CaneToads_rural1_20_MONO.wav')

# cane_recording <- readWave('canetoad2.wav')

samp.rate <- cane_recording@samp.rate
left <- cane_recording@left
len <- length(left)
#sig <- left[c(1:(len - len %% TFRAME))]
#sig <- sig / (2 ^ 16 / 2)                 #normalised by the maximum signed value of 16 bit
#nframe <- length(sig) / TFRAME
# dim(sig) <- c(TFRAME, nframe)
# sig <- Mod(mvfft(sig * hamming))

#write.table(amp,file="outfile.xls",sep="\t", col.names = F, row.names = F)
#################################################################################
#                  Smooth data ----amp
#################################################################################

# first.temp <- sig[1, ]
# sig <- filter(sig, rep(1 / 3, 3))
# sig[1, ] <- first.temp
# amp <- sig[c(1:(TFRAME / 2 )), ]


#################################################################################
#                  Normalise data ----amp
#################################################################################
  
#source("matrix_normalise2UnitLength.R")
#amp1 <- matrix_normalise2UnitLength(amp)

# source("matrix_normalise2UnitFrame.R")
# amp <- matrix_normalise2UnitFrame(amp)

#################################################################################

#amp <- 20*log10(amp)

#################################################################################
#write.table(amp,file="outfile.xls",sep="\t", col.names = F, row.names = F)
#write.table(amp1,file="outfile1.xls",sep="\t", col.names = F, row.names = F)

############################Get the spectrogram data from **.csv#################


amp <- read.csv("C:/projects/amp8.csv")

library(Matrix)
amp <- as.matrix(amp)


 first.temp <- amp[1, ]
 amp <- filter(amp, rep(1 / 3, 3))
 amp[1, ] <- first.temp
 amp <- amp[c(1:(TFRAME / 2 )), ]



 #amp <- 20*log10(amp)


##################################################################################
####            Calculate some needed parameters
##################################################################################

source("CalculateRequiredFrameOverlap.R")  # LOAD FUNCTION
windowOverlap <- CalculateRequiredFrameOverlap(samp.rate, 
                                               frameSize, 
                                               CANETOAD_MAX_OSCI_FREQ)
   

recordingDuration <- (length(left) / samp.rate)

source("fBinWidth.R")
FBinWidth <- fBinWidth(samp.rate, 
                       TFRAME / 2)

source("framePerSecond.R")
FramePerSecond <- framePerSecond(samp.rate, 
                                 frameSize, 
                                 windowOverlap)

#write.table(log2(t(amp1)),file="outfile1.xls",sep="\t", col.names = F, row.names = F)

####################################################################################
#library(seewave)
# duration <- recordingDuration
# samp.freq <- 22.05
# x <- seq(0, duration, duration / (nframe-1))
# y <- seq(0, samp.freq / 2, samp.freq / 2 / 511)
# filled.contour(x,y,t(amp),col=gray(seq(1,0,-1/10)),levels=pretty(c(-115,-10),20))



##################################################################################
####                  USING THE FUNCTION OF R TO CREATE THE SPECTROGRAM
##################################################################################

# library(seewave)
# win.graph(width=4.875, height=2.5,pointsize=8)
# 
# sonogram <- spectro(cane_recording, 
#                     cane_recording@samp.rate, 
#                     wl = 512,
#                     #flim = c(0,8),
#                     palette = rev.gray.colors.1, 
#                     collevels = seq(-260,0,5) )

##################################################################################
####              DETECT THE OSILLATIONS IN A GIVEN FREQ BIN
##################################################################################

source("DetectOscillations.R")
hits <- DetectOscillations(amp, 
                           CANETOAD_MIN_HZ, 
                           CANETOAD_MAX_HZ, 
                           CANETOAD_DCT_DURATION, 
                           CANETOAD_MIN_OSCI_FREQ, 
                           CANETOAD_MAX_OSCI_FREQ,
                           CANETOAD_DCT_THRESHOLD,
                           )

#write.table(hits,file="outfile_hits.xls",sep="\t", col.names = F, row.names = F)

##################################################################################
#a <- hits
#filled.contour(1:ncol(a),1:nrow(a), t(a), col=gray(seq(1,0,-10/19)),levels=pretty(c(20,0),20))




##################################################################################
#if (hits == NULL)
#{
#  cat("WARNING: DCT LENGTH IS TOO SHORT TO DETECT THE MAXOSCILFREQ")
#  out_scores <- NULL
#  out_events <- NULL
#  break
#}
##################################################################################
###            REMOVE SIGLE LINES OF HITS FROM OSCILLATION MATRIX
##################################################################################

source("RemoveIsolatedOscillations.R")
hits <- RemoveIsolatedOscillations(hits)
#write.table(hits,file="outfile_hits_after_remove.xls",sep="\t", col.names = F, row.names = F)

#a <- hits
#filled.contour(1:ncol(a),1:nrow(a), t(a), col=gray(seq(1,0,-10/19)),levels=pretty(c(20,0),20))


##################################################################################
###                        EXTRACT ACOUSTIC EVENTS
##################################################################################
# source("GetODScores.R")
# scores <- GetODScores(hits,
#                       CANETOAD_MIN_HZ,
#                       CANETOAD_MAX_HZ,
#                       FBinWidth)
# 
# source("Data_filterMovingAverage.R")
# scores <- Data_filterMovingAverage(scores,3)
# 
# 
# 
# # write.table(scores,file="outfile_scores.xls",sep="\t", col.names = F, row.names = F)
# 
# source("GetODFrequency.R")
# oscFreq <- GetODFrequency(hits, 
#                           CANETOAD_MIN_HZ, 
#                           CANETOAD_MAX_HZ, 
#                           FBinWidth)
# # write.table(oscFreq,file="outfile_oscFreq.xls",sep="\t", col.names = F, row.names = F)
# 
# 
# 
# 
# source("convertODScores2Events.R")
# events <- convertODScores2Events(scores, 
#                                  oscFreq, 
#                                  CANETOAD_MIN_HZ, 
#                                  CANETOAD_MAX_HZ, 
#                                  FramePerSecond, 
#                                  FBinWidth, 
#                                  CANETOAD_EVENT_THRESHOLD, 
#                                  CANETOAD_MIN_DURATION, 
#                                  "Canetoad")



##################################################################################




##################################################################################
###               SEE THE RESULT OF DETECTION
##################################################################################


duration <- recordingDuration
samp.freq <- 22.05
nframe <- ncol(amp)
x <- seq(0, duration, duration / (nframe-1))
y <- seq(0, samp.freq /2 , samp.freq / 2 / 511)

#temp.hits <- matrix(c(0),nrow(hits),ncol(hits))

##################################################################################
filled.contour(x,y,t(abs(amp) / max(abs(amp)) *20 ),col=gray(seq(1,0,-1/19)),levels=pretty(c(0,20),20),
                xlim=range(x),ylim=range(y),
                plot.title = title(main = "CANETOAD DETECTION",xlab = "Time/s"
                                   ,ylab = "Frequency/KHz"),type="n",
                
                plot.axes = {axis(1); axis(2); 
                              for(i in 1:nrow(hits)){
                                  for(j in 1:ncol(hits)){
                                                                         
                                     if(hits[i,j] != 0){      
                                                                 
                                       points( recordingDuration /  ncol(hits) * j,
                                               samp.freq / 2 / nrow(hits) * i,
                                              cex = 1/ncol(hits)*nrow(hits),col = "blue"
                                              )                      
                     }
                   }
                 }
               }
              )



##################################################################################
# plot.new()
# 
# #I am organizing where the plots appear on the page using the "plt" argument in "par()"
# par(new = "TRUE",              
#     plt = c(0.3,0.8,0.6,0.9),   # using plt instead of mfcol (compare
#     # coordinates in other plots)
#     las = 1,                      # orientation of axis labels
#     cex.axis = 1,                 # size of axis annotation
#     tck = -0.02 )                 # major tick size and direction, < 0 means outside
# 
# source("filled.contour3.R")
# 
# filled.contour3(x,y,log2(t(amp1)),col=gray(seq(1,0,-1/19)),levels=pretty(c(0,20),20),
#                xlim=range(x),ylim=range(y),
#                plot.title = title(main = "CANETOAD DETECTION",xlab = "Time/s"
#                                   ,ylab = "Frequency/KHz"), type="n",
#                
#                plot.axes = {axis(1); axis(2); for(i in 1:nrow(hits)){
#                  for(j in 1:ncol(hits)){
#                    if(hits[i,j] != 0){      
#                      rect(recordingDuration / ncol(hits)*j,
#                           samp.freq / 2 / nrow(hits)*i,
#                           recordingDuration / ncol(hits)*(j+1),
#                           samp.freq / 2 / nrow(hits)*(i+1),border = "blue"        
#                           )                                       
#                       }    
#                     }  
#                   }
#                }
#                
# )
# 
# 
# par(new = "TRUE",
#       plt = c(0.2,0.8,0.1,0.3),
#       las = 1,
#       cex.axis = 1,
#       tck = -0.02)
#     
#   hist(events$Score, cin=0.15,main = "EVENTS DETECTION", xlab = "Bins",ylab = "Scores",
#           space = 0,border = "red" ,axis.lty = 0 ,font.axis = 1)

######################################################################################

