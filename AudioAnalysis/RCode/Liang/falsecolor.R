
falsecolor <- function(red, green, blue, civil){

  #Constant value for linear transformation of indices
  rMax <- 0.7
  rMin <- 0.4
  gMax <- 0.95
  gMin <- 0.7
  bMax <- 20
  bMin <- 0

  #red for ACI
  r <- as.vector(red)
  missing <- which(r == -1)
  r[which(r<rMin)] <- 0
  r[which(r>rMax)] <- 1
  r[which(r>rMin & r<rMax)] <- (r[which(r>rMin & r<rMax)] - rMin) / (rMax - rMin)
  r[missing] <- 0.5
  # r <- r^2

  #green for temporal entropy
  g <- as.vector(green)
  missing <- which(g == -1)
  g[which(g<gMin)] <- 0
  g[which(g>gMax)] <- 1
  g[which(g>gMin & g<gMax)] <- (g[which(g>gMin & g<gMax)] - gMin) / (gMax - gMin)
  g[missing] <- 0.5
  g <- 1 - g
  # g <- g^2

  #blue for cover spectra
  b <- as.vector(blue)
  missing <- which(b == -1)
  b[which(b<bMin)] <- 0
  b[which(b>bMax)] <- 1
  b[which(b>bMin & b<bMax)] <- (b[which(b>bMin & b<bMax)] - bMin) / (bMax - bMin)
  b[missing] <- 0.5
#   b <- 1 - b
  # b <- b^2

  #generate a palette of RGB values
  rgbPalette <- rgb(r,g,b)

  #set the sunrise and sunset
  civilS <- civil$civilS
  civilE <- civil$civilE
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
  
  missing <- matrix(rep("#808080", 5 * 257), 5, 257)
  rgbPalette <- rbind(rgbPalette, missing)
  
  firstDate <- c(13, 43, 74, 105, 134, 165, 195, 226, 256)
  rgbPalette[ ,firstDate] <- "#FFFFFF"
  
  threeHour <- c(60 * seq(3,21,3) + 1)
  rgbPalette[threeHour, ] <- "#FFFFFF"
#   rgbPalette[721, ] <- "#FFFFFF"

  #draw the palette in pixels
  x <- 1:1440
  y <- 1:257
  mydata<-matrix(1:(1440*257), 1440, 257)
# image(x, y, mydata, col=rgbPalette)
# image(seq(0,24,24/1435), seq(0,8820,8820/256), mydata, col=rgbPalette, xaxp=c(0,24,24),
#       yaxp=c(0,8820,6),xlab='time/hour', ylab='frequency/Hz')

  date<-seq(as.Date("2011/10/20"), as.Date("2012/7/2"), "day")
#   png("test.png",width=1435, height=366)
#   par(fin=c(15.3, 3), pin=c(14.948, 2.677), xaxs='i', yaxs='i')    # 1 inch = 96 pixels
  par(bg='gray', fin=c(15.3, 8), pin=c(14.948, 5.354), xaxs='i', yaxs='i')
  image(seq(0,24,24/1439), date, mydata,col=rgbPalette,xaxp=c(0,24,24), xlab="Hours",
        ylab='Days (from 2011/10/20 to 2012/10/20)', yaxt='n')
  axis.Date(2, at=seq(as.Date("2011/10/20"), as.Date("2012/10/20"), 30), format="%b")
  mtext("Days (from 2011/10/20 to 2012/07/02)", side=2, line=3)
#   dev.off()
}