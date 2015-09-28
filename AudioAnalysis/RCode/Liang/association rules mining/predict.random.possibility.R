#possibility of a random pair of species appear concomitantly

source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species.in.arule.R')

testing<-read.csv("C:\\Work\\myfile\\SERF_callCount_20sites_fulllist\\testing.csv",check.names=FALSE)
species.names <- names(testing)

allPairs <- combn(1:length(species.names), 2)
set.seed(1)
sequence <- sample(1:ncol(allPairs), ncol(allPairs))

cnt <- 100
predictions <- list()
for(i in 1:cnt){
  antecedent.index <- allPairs[1, sequence[i]]
  consequent.index <- allPairs[2, sequence[i]]
  ante.presence <- which(testing[ , antecedent.index] == 1)
  temp <- which(testing[ante.presence, consequent.index] == 1)
  conse.presence <- ante.presence[temp]
  if(length(ante.presence) == 0){
    results<-list(antecedent.presence=ante.presence, consequent.presence=conse.presence, proportion=NA)
  }else{
    results<-list(antecedent.presence=ante.presence, consequent.presence=conse.presence, proportion=length(conse.presence)/length(ante.presence))
  }
  predictions[[i]] <- results
}

predicted.values <- numeric()
for(j in 1:length(predictions)){
  predicted.values <- c(predicted.values, predictions[[j]][[3]])
  predicted.values <- round(predicted.values, digits=3)
}