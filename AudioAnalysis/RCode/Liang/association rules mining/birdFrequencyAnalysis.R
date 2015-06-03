birdFrequencyAnalysis <- function(annotation){
  speciesNum <- ncol(annotation)
  #remove non-bird minutes
  birdCountPerMin <- rowSums(annotation)
  non.bird <- which(birdCountPerMin == 0)
  birdMin <- annotation[-non.bird, ]
  
  #single bird absolute and relative support
  singleBirdSupport <- colSums(birdMin)
  singleBirdFrequency <- singleBirdSupport / nrow(birdMin)
  
  #independent joint frequency
  indepJointFrequency <- numeric()
  for(i in 1:speciesNum){
    jointFrequency <- singleBirdFrequency * singleBirdFrequency[i]
    indepJointFrequency <- cbind(indepJointFrequency, jointFrequency)
  }
  
  #actual joint frequency
  actualJointFrequency <- numeric()
  for(i in 1:speciesNum){
    temp <- rep(birdMin[,i], speciesNum)
    dim(temp) <- c(nrow(birdMin), speciesNum)
    logic <- birdMin & temp
    doubleBirdSupport <- colSums(logic)
    doubleBirdFrequency <- doubleBirdSupport / nrow(birdMin)
    actualJointFrequency <- cbind(actualJointFrequency, doubleBirdFrequency)
  }
  jointFrequency <- list(indepJointFrequency = indepJointFrequency, actualJointFrequency = actualJointFrequency)
  return(jointFrequency)
}