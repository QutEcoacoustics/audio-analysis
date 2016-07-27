# overlap.freq.perMin

source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species4freq.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/overlapping.frequency.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/calculate.freq.range.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species.in.arule.R')

rules <- read.csv("C:\\Work\\myfile\\SERF_callcount_20sites_fulllist\\rules.csv")

arules <- as.character(rules[,1])
# missing.index <- which(arules=="")
# arules <- arules[-missing.index]
len <- length(arules)
overlap <- numeric()
missed.pair <- character()
distances <- numeric()

# for(i in 1:len){
for(i in 1:50){
  #generate a random pair of species from one-day collection
  species.names <- match.species.in.arule(arules[i])
  
  #find the frequency information for each species
  species1.frequency <- match.species4freq(species.names[1])
  species2.frequency <- match.species4freq(species.names[2])
  if(is.character(species1.frequency) || is.character(species2.frequency)){
    missed.pair <- c(missed.pair, species.names)
    next
  }
  else{
    #calculate the minimum and maximum frequency for each species
    threshold <- 0.1
    species1.range <- calculate.freq.range(species1.frequency, threshold)
    species2.range <- calculate.freq.range(species2.frequency, threshold)
  
    #calculate the overlappling frequency
    overlap <- c(overlap, overlapping.frequency(species1.range, species2.range))
    
    #calculate the maximum frequency for each species
#     species1.max <- which.max(species1.frequency) * 11025 / 256
#     species2.max <- which.max(species2.frequency) * 11025 / 256
#     
#     #calculate the distances
#     distances <- c(distances, abs(species1.max - species2.max))
#     if(species1.max >= species2.max){
#       distances <- c(distances, log2(species1.max / species2.max))
#     }
#     else{
#       distances <- c(distances, log2(species2.max / species1.max))
#     }
  }
}