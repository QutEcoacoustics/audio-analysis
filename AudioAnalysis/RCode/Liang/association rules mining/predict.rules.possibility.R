#validate the testing dataset
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species.in.arule.R')

rules <- read.csv("C:\\Work\\myfile\\SERF_callCount_20sites_fulllist\\ranked.rules.chisquare.csv",check.names=FALSE)
testing<-read.csv("C:\\Work\\myfile\\SERF_callCount_20sites_fulllist\\testing.csv",check.names=FALSE)
arules <- as.character(rules[,1])
predictions <- list()

for(i in 1:length(arules)){
  species.names <- match.species.in.arule(arules[i])
  antecedent.index <- which(names(testing) == species.names[1])
  consequent.index <- which(names(testing) == species.names[2])
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