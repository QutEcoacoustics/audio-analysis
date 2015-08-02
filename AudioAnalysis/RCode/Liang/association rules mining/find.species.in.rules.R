source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species.in.arule.R')


rules <- read.csv("C:\\Work\\myfile\\SERF_callcount_20sites_fulllist\\rules.csv")

arules <- as.character(rules[,1])
# missing.index <- which(arules=="")
# arules <- arules[-missing.index]
len <- length(arules)
# overlap <- numeric()
missed.pair <- character()
species <- character()

for(i in 1:len){
  #generate a random pair of species from one-day collection
  species.names <- match.species.in.arule(arules[i])
  if(!any(species == species.names[1])){
    species <- c(species, species.names[1])
  }
  if(!any(species == species.names[2])){
    species <- c(species, species.names[2])
  }
}