removeNeighbor <- function(minute, neighbor){
  choosen <- minute[1]
  cnt <- 2
  for(i in 2:length(minute)){
    distance <- abs(choosen - minute[i])
    logic <- distance < neighbor
    if(sum(logic)==0){
      choosen[cnt] <- minute[i]
      cnt <- cnt + 1
    }
  }
  return(choosen)
}