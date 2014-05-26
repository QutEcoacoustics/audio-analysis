deleteMin <- function(mins){
  minThreshold <- 2
  remainMin <- mins[1]
  for(i in 2:length(mins)){
    judgement <- abs(remainMin - mins[i])
    if(any(judgement <= minThreshold))
      next
    else{
      remainMin <- c(remainMin, mins[i])
    }
  }
  
  return(remainMin)
}