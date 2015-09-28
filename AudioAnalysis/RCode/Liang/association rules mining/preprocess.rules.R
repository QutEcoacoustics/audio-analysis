library(arules)
#preprocess

source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/lessThan10annotations.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/find.frequency.maximum.R')

training <- read.csv("C:\\Work\\myfile\\SERF_callCount_20sites_fulllist\\training.csv",check.names=FALSE)
newTraining<-training[,-index]
max.frequency<-find.frequency.maximum(names(newTraining))
index2 <- which(max.frequency < 6)
newTraining2<-newTraining[ , -index2]

training <- apply(newTraining2, 2, as.factor)
training <- data.frame(training, check.names=FALSE)

rules <- apriori(training, parameter=list(minlen=2, supp=0.001, conf=0.5, maxlen=2), 
                 control=list(verbose=FALSE))

quality(rules) <- round(quality(rules), digits=3)
#     pruned.rules <- prune.superRules(rules)
pruned.rules <- rules
sorted.rules <- sort(pruned.rules, by='lift')
chiSquared <- round(interestMeasure(sorted.rules, method='chiSquared', transactions=training), digits=3)
hyperConfidence <- round(interestMeasure(sorted.rules, method='hyperConfidence', transactions=training), digits=3)
results <- as(sorted.rules, 'data.frame')
results <- cbind(results, chiSquared, hyperConfidence)

#write.table

