fBinWidth <- function(
                      sampRate, 
                      windowSize,
                      ...
                      ){
   fBinwidth <- (sampRate / 2) / windowSize
   return (fBinwidth)
}