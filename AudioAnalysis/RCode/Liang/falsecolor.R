
falsecolor <- function(longACI, longTEN, longCVR){

  #Constant value for linear transformation of indices
  rMax <- 0.7
  rMin <- 0.4
  gMax <- 0.95
  gMin <- 0.7
  bMax <- 0.8
  bMin <- 0.1

  #red for ACI
  r <- as.vector(longACI)
  r[which(r<rMin & r>0)] <- 0
  r[which(r>rMax)] <- 1
  r[which(r>rMin & r<rMax)] <- (r[which(r>rMin & r<rMax)] - rMin) * (1 / (rMax - rMin))
  r[which(r == -1)] <- 0.5
  # r <- r^2

  #green for temporal entropy
  g <- as.vector(longTEN)
  g[which(g<gMin & g>0)] <- 0
  g[which(g>gMax)] <- 1
  g[which(g>gMin & g<gMax)] <- (g[which(g>gMin & g<gMax)] - gMin) * (1 / (gMax - gMin))
  g[which(g == -1)] <- 0.5
  g <- 1-g
  # g <- g^2

  #blue for cover spectra
  b <- as.vector(longCVR)
  b[which(b<bMin & b>0)] <- 0
  b[which(b>bMax)] <- 1
  b[which(b>bMin & b<bMax)] <- (b[which(b>bMin & b<bMax)] - bMin) * (1 / (bMax - bMin))
  b[which(b == -1)] <- 0.5
  # b <- b^2

  #generate a palette of RGB values
  rgbPalette <- rgb(r,g,b)

  #set the sunrise and sunset
  dim(rgbPalette) <- c(1435, 257)
  for(i in 1:257){
    rgbPalette[civilS[i], i] <- "#FFFFFF"
    rgbPalette[civilE[i]+720, i] <- "#FFFFFF"
    #   rgbPalette[sunrise[i], i] <- "#FFFFFF
    #   rgbPalette[sunset[i]+720, i] <- "#FFFFFF"
    #   rgbPalette[astS[i], i] <- "#FFFFFF"
    #   rgbPalette[astE[i]+720, i] <- "#FFFFFF"
    #   rgbPalette[nautS[i], i] <- "#FFFFFF"
    #   rgbPalette[nautE[i]+720, i] <- "#FFFFFF"
  }

  #draw the palette in pixels
  x <- 1:1435
  y <- 1:257
  mydata<-matrix(1:(1435*257), 1435, 257)
  image(x, y, mydata, col=rgbPalette)
  #image(seq(0,24,24/1435),seq(0,8820,8820/256),mydata,col=rgbPalette,xaxp=c(0,24,24),yaxp=c(0,8820,6),xlab='time/hour',ylab='frequency/Hz')

  # date<-seq(as.Date("2011/10/20"), as.Date("2012/7/2"), "day")
  # png("test.png",width=1435, height=366)
  # # par(fin=c(15.3, 3), pin=c(14.948, 2.677), xaxs='i', yaxs='i')    # 1 inch = 96 pixels
  # par(fin=c(15.3, 8), pin=c(14.948, 5.354), xaxs='i', yaxs='i')
  # image(seq(0,24,24/1434),date,mydata,col=rgbPalette,xaxp=c(0,24,24), xlab="Hours",ylab='Days (from 2011/10/20 to 2012/10/20)',yaxt='n')
  # axis.Date(2, at=seq(as.Date("2011/10/20"), as.Date("2012/10/20"), 30), format="%b")
  # mtext("Days (from 2011/10/20 to 2012/07/02)", side=2, line=3)
  # dev.off()
}