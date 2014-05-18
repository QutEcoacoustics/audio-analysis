##################################################################################
####                        CANETOAD RECOGNITION
##################################################################################

setwd("C:/projects/Mixophyes")

##################################################################################
####                        CANETOAD PARAMETER
##################################################################################



#################################################################################
#                 set the parameter for producing spectrogram
#################################################################################


TFRAME <- frameSize <- 1024
hamming <- 0.54-0.46*cos(2 * pi * c(1:frameSize) / (frameSize - 1))


library(tuneR)
library(audio)

sig.a <- load.wave('20 Mixophyes fasciolatus_test.wav')

samp.rate <- sig.a$rate

len <- length(sig.a)


recordingDuration <- len / samp.rate

window.power <- sum(hamming ^ 2)
epsilon <- (1 / 2) ^ (16 - 1)
min.dc <- 10 * log10(epsilon ^ 2 / window.power / samp.rate)
min.freq <- 10 * log10(epsilon ^ 2 / window.power / samp.rate * 2)

# source("CalculateRequiredFrameOverlap.R")  # LOAD FUNCTION
# windowOverlap <- CalculateRequiredFrameOverlap(samp.rate, 
#                                                frameSize, 
#                                                CANETOAD_MAX_OSCI_FREQ)

windowOverlap <- 0.5

overlap <- windowOverlap

#nrow <- frameSize

Shift <- frameSize * (1-overlap)
ncol <- floor((len - frameSize) / Shift + 1)

amp <- matrix(0, (frameSize/2+1), ncol)


for(i in 1:ncol){
  frame.a <- (i-1)*Shift + 1
  frame.b <- frame.a + (frameSize - 1)
  
  sig <- sig.a[frame.a:frame.b]
  
  sig <- sig * hamming
  amp.a <- Mod(fft(sig))
  
 
  
  amp.a <- amp.a[1:(frameSize/2+1)]
  # cat("ss",length(amp.a))
  
  fq.row <- amp.a[1 : length(amp.a)]
  fq.row[which(fq.row <  epsilon)] <- min.freq
  fq.row[which(fq.row >= epsilon)] <- 10 * log10(fq.row[which(fq.row >= epsilon)] ^ 
                                                   2 / window.power / samp.rate)  

#  fq.row <- 10 * log10(fq.row)
  amp[1:(frameSize/2+1),i] <- fq.row
}


#################################################################################

# amp <- floor((amp - min(amp))/(max(amp) - min(amp))*256)
# library(pixmap)
# x <- pixmapGrey(amp, nrow = nrow(amp),ncol = ncol(amp))
# plot(x)

#################################################################################
#                  Smooth data ----amp
#################################################################################
 sig <- amp
 first.temp <- sig[1, ]
 sig <- filter(sig, rep(1 / 3, 3))
 sig[1, ] <- first.temp
 amp <- sig[c(1:(TFRAME / 2 )), ]

##################################################################################
####            Calculate some needed parameters
##################################################################################

recordingDuration <- (len / samp.rate)

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
duration <- recordingDuration
samp.freq <- 22.05
x <- seq(0, duration, duration / (ncol(amp)-1))
y <- seq(0, samp.freq / 2, samp.freq / 2 / 511)
filled.contour(x,y,20-t(abs(amp) / max(abs(amp)) *20 ),col=gray(seq(1,0,-1/19)),levels=pretty(c(0,20),20))

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
source("GetODScores.R")
scores <- GetODScores(hits,
                      CANETOAD_MIN_HZ,
                      CANETOAD_MAX_HZ,
                      FBinWidth)

#source("Data_filterMovingAverage.R")
#scores <- Data_filterMovingAverage(scores,3)

# write.table(scores,file="outfile_scores.xls",sep="\t", col.names = F, row.names = F)

source("GetODFrequency.R")
oscFreq <- GetODFrequency(hits, 
                          CANETOAD_MIN_HZ, 
                          CANETOAD_MAX_HZ, 
                          FBinWidth)
# write.table(oscFreq,file="outfile_oscFreq.xls",sep="\t", col.names = F, row.names = F)

source("convertODScores2Events.R")
events <- convertODScores2Events(scores, 
                                 oscFreq, 
                                 CANETOAD_MIN_HZ, 
                                 CANETOAD_MAX_HZ, 
                                 FramePerSecond, 
                                 FBinWidth, 
                                 CANETOAD_EVENT_THRESHOLD, 
                                 CANETOAD_MIN_DURATION, 
                                 "Canetoad")


##################################################################################
###               SEE THE RESULT OF DETECTION
##################################################################################


duration <- recordingDuration
samp.freq <- 22.05
nframe <- ncol(amp)
x <- seq(0, duration, duration / (nframe-1))
y <- seq(0, samp.freq /2 , samp.freq / 2/ 511)

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
                             
                             
                             minBin <- trunc(CANETOAD_MIN_HZ / FBinWidth )        
                             maxBin <- trunc(CANETOAD_MAX_HZ / FBinWidth )
                             isHit <- FALSE
                             i <- 1
                              
                             
                             while(i <= ncol(hits)){
                              
                               rect.a <- 0
                               rect.b <- 0
                               
                               
                               if(events$Score[i] > 0 ){
                                 rect.a <- i
                                 isHit <- TRUE  
                                 
                                 for(j in i+1:ncol(hits)){
                                   if((isHit == TRUE) & events$Score[j] == 0 ){                                  
                                     rect.b <- j
                                     isHit <- FALSE
                                   }                           
                                 }
                                                          
                               }
                               
                          #     cat("rect.a = ",rect.a,"\n")
                           #    cat("rect.b = ",rect.b, "\n")
                               
                               if(rect.a > 0 & rect.b > 0){
                            #     cat("okok")
                                 rect(recordingDuration / ncol(hits) * (rect.a),
                                      samp.freq / 2 / nrow(hits) * minBin,
                                      recordingDuration / ncol(hits) * (rect.b),
                                      samp.freq / 2 / nrow(hits) * maxBin,                                    
                                      border = "red")                                
                                 
                                 i <- rect.b
                               }
                               
                               i <- i + 1
                               rect.a <- 0
                               rect.b <- 0
                                                              
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

