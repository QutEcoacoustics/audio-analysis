
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/match.species4freq.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/calculate.freq.range.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/overlapping.frequency.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/lessThan10annotations.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/find.frequency.maximum.R')
# sp <- read.csv("C:\\Work\\myfile\\species_freq_profiles\\species_freq_profiles\\csv\\sp.csv")
# allNames <- as.character(sp[ , 3])

training <- read.csv("C:\\Work\\myfile\\SERF_callCount_20sites_fulllist\\training.csv", check.names=FALSE)
newTraining<-training[,-index]
max.frequency<-find.frequency.maximum(names(newTraining))
index2 <- which(max.frequency < 6)
newTraining2<-newTraining[ , -index2]

training <- apply(newTraining2, 2, as.factor)
training <- data.frame(training, check.names=FALSE)

allNames <- names(training)
allPairs <- combn(allNames, 2)
threshold <- 0.1
overlap <- numeric()
distances <- numeric()
missed.pair <- character()

for(i in 1:ncol(allPairs)){
# for(i in 1:2){
  #   find the frequency information for each species
  species1.frequency <- match.species4freq(allPairs[1, i])
  species2.frequency <- match.species4freq(allPairs[2, i])
  if(is.character(species1.frequency) || is.character(species2.frequency)){
    missed.pair <- c(missed.pair, allPairs)
    next
  }
  else{
#     #calculate the minimum and maximum frequency for each species
    species1.range <- calculate.freq.range(species1.frequency, threshold)
    species2.range <- calculate.freq.range(species2.frequency, threshold)
    
    #calculate the overlappling frequency
    overlap <- c(overlap, overlapping.frequency(species1.range, species2.range))
    
    #calculate the maximum frequency for each species
#     species1.max <- which.max(species1.frequency) * 11025 / 256
#     species2.max <- which.max(species2.frequency) * 11025 / 256
    
    #calculate the distances
#     distances <- c(distances, abs(species1.max - species2.max))
#     if(species1.max >= species2.max){
#       distances <- c(distances, log2(species1.max / species2.max))
#     }
#     else{
#       distances <- c(distances, log2(species2.max / species1.max))
#     }
  }
}