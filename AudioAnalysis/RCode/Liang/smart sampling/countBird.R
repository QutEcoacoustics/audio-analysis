countBird <- function(calls){
  temp <- calls[1, ]
  for(i in 2:nrow(calls)){
    temp <- temp | calls[i, ]
  }
  total <- length(which(temp))
  return(total)
}