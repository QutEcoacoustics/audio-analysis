overlapping.frequency <- function(bird1, bird2){
  bird1.lower <- bird1$lower.freq
  bird1.upper <- bird1$upper.freq
  bird2.lower <- bird2$lower.freq
  bird2.upper <- bird2$upper.freq
  overlap <- numeric()
  
  if(bird1.upper<=bird2.lower || bird1.lower>=bird2.upper){
    overlap <- 0
  }
  else if(bird1.lower<=bird2.upper && bird1.lower>=bird2.lower && bird1.upper>=bird2.upper){
    overlap <- bird2.upper - bird1.lower
  }
  else if(bird1.lower<=bird2.lower && bird1.upper>=bird2.upper){
    overlap <- bird2.upper - bird2.lower
  }
  else if(bird1.upper<=bird2.upper && bird1.upper>=bird2.lower && bird1.lower<=bird2.lower){
    overlap <- bird1.upper - bird2.lower
  }
  else if(bird1.upper<=bird2.upper && bird1.lower>=bird2.lower){
    overlap <- bird1.upper - bird1.lower
  }
  else{
    overlap <- NA
  }
  
  return(overlap)
}