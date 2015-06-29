rm(list = ls())
library(arules)
# library(arulesViz)

source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/names.latin2english.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/AR.dataPreprocess.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/extract.daytime.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/prune.superRules.R')
# source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/count.occurrence.R')

#source('removeRedundantRules.R')
folder <- "C:\\Work\\myfile\\SERF_callCount_20sites"
pattern <- "hitmaps*"
filename <- list.files(folder, pattern, no..=TRUE)
rule.count <- 0

for(i in 1:length(filename)){
  if(i!=1){
    rm(rules)
  }
  species <- AR.dataPreprocess(paste(folder, "\\", filename[i], sep=""))
  names(species) <- names.latin2english(names(species))
  species <- extract.daytime(species, 271, 1110)
#   occurrence <- count.occurrence(species)
  rules <- apriori(species, parameter=list(minlen=2, supp=0.03, conf=0.6, maxlen=10), 
                  control=list(verbose=FALSE))
  if(length(rules)){
    quality(rules) <- round(quality(rules), digits=3)
    pruned.rules <- prune.superRules(rules)
    sorted.rules <- sort(pruned.rules, by='lift')
    chiSquared <- round(interestMeasure(sorted.rules, method='chiSquared', transactions=species), digits=3)
    hyperConfidence <- round(interestMeasure(sorted.rules, method='hyperConfidence', transactions=species), digits=3)
    results <- as(sorted.rules, 'data.frame')
    results <- cbind(results, chiSquared, hyperConfidence)
#     rule.count <- rule.count + length(which(chiSquared>3.84))
    results <- results[order(chiSquared, decreasing=TRUE), ]
    filedate <- sub('.*_([[:alpha:]]+).*_([[:digit:]]+)-([[:digit:]]+)-([[:digit:]]+).*', '\\1\\2\\3\\4', filename[i])
    filepath <- paste('c:/work/myfile/daytime_supp0.03conf0.6_pruned/', filedate, '.csv', sep="")
    write.table(results, file=filepath, sep=',',row.names=FALSE)
  }
}

# inspect(sorted.rules)

# speciesFrequency <- itemFrequency(as(species, 'transactions'), 'absolute')
# speciesFrequency <- sort(speciesFrequency, decreasing=TRUE, index.return=T)
