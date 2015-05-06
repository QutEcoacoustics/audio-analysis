histMinute <- function(features, interval){
  maximum <- max(features)
  minimum <- min(features)
#   separation <- seq(minimum, maximum, (maximum - minimum)/19)
  if(maximum%%interval!=0){
    maximum <- maximum-maximum%%interval+interval
  }

  if(minimum%%interval!=0){
    minimum <- minimum-minimum%%interval
  }

  separation <- seq(minimum, maximum, interval)
  histogram <- hist(features, breaks=separation)

  return(histogram)
}