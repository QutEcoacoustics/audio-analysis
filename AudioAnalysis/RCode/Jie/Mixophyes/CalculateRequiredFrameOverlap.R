##################################################################################
####                       CALCULATE THE REQUIRED FRAMEOVERLAP
##################################################################################

CalculateRequiredFrameOverlap <- function(
                                          sampeRate,
                                          framewidth,
                                          maxoscifreq,
                                          ...
                                          ){
  optimumframerate <- 3 * maxoscifreq    # to ensure that max oscillation sits in 3/4 along the array of DCT coefficients 
  
  frameoffset <- trunc (sampeRate / optimumframerate)
  
  overlap <- (framewidth - frameoffset) / framewidth
  
  # browser("CalculateRequiredFrameOverlap.R")
  
  return (overlap) 
}