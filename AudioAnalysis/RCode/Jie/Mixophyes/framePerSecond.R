framePerSecond <- function(
                          samprate, 
                          windowsize, 
                          Overlap,
                          ...){
  Frameoffset <- (windowsize / samprate) * (1 - Overlap) 
  framepersecond <- 1 / Frameoffset
  return (framepersecond)
}