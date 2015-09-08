library(ade4)

#read the dataset
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/lessThan10annotations.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/find.frequency.maximum.R')

training <- read.csv("C:\\Work\\myfile\\SERF_callCount_20sites_fulllist\\training.csv",check.names=FALSE)
newTraining<-training[,-index]
max.frequency<-find.frequency.maximum(names(newTraining))
index2 <- which(max.frequency < 6)
newTraining2<-newTraining[ , -index2]

training <- apply(newTraining2, 2, as.factor)
training <- data.frame(training, check.names=FALSE)

rm(newTraining, i, index, index2, max.frequency)

#replace NA with '0'
training <- as.matrix(training)
training[is.na(training)] <- 0

#transform the matrix
training <- apply(training, 2, as.numeric)
training <- t(training)

#calculate the Jaccard matrix
distance.matrix <- dist.binary(training, 1)

#calculate the principle coordinate analysis
pco.result <- dudi.pco(distance.matrix)

#scatter(pco.result)