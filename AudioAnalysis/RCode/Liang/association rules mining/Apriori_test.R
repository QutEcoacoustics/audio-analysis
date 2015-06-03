library(arules)

species <- read.csv('c:/work/myfile/SE101015_species.csv')

rules <- apriori(species, parameter=list(minlen=2, supp=0.01, conf=0.9, maxlen=10), control=list(verbose=FALSE)))

rules.sorted <- sort(rules, by='lift')

inspect(head(rules.sorted))