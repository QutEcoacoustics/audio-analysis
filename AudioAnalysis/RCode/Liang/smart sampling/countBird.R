countBird <- function(callFile){
  calls <- read.csv(callFile)
  calls <- calls[ ,4:80]
  temp <- calls[1, ]
  for(i in 2:nrow(calls)){
    temp <- temp | calls[i, ]
  }
  total <- length(which(temp))
  return(total)
}