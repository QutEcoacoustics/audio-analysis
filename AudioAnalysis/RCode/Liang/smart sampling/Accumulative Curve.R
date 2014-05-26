drawAC <- function(calls, samples){
  
  currentSpecies <- calls[1, ]
  previousLen <- length(which(currentSpecies > 0))
  accumulation <- c(0, previousLen)
  
  for(i in 2:samples){
    existLogic <- currentSpecies | calls[i, ]
    currentLen <- length(which(existLogic))
    if(currentLen > previousLen){ 
      previousLen <- currentLen
      currentSpecies <- existLogic
    }
    accumulation <- c(accumulation, currentLen)
  }
  
  result<-accumulation / 62 * 100
  return(result)
}