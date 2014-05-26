source('C:/Work/decision tree/greedyBird.R')
source('C:/Work/decision tree/rankMin.R')
source('C:/Work/decision tree/Accumulative Curve.R')
source('c:/Work/decision tree/deleteMin.R')

indices.species <- read.csv('c:/work/myfile/linearRegression.csv')
oct13 <- indices.species[1:1440,]
rm(indices.species)


# mins <- which(oct13$verFeature>=0.0005083 & oct13$horFeature>=0.001624 & 
#               oct13$H.peakFreq.>=0.7339 & oct13$H.peakFreq.<0.8161)
mins <- which(oct13$verFeature>=0.0005083 & oct13$segCount>=9 & 
                oct13$H.peakFreq.<0.7339)
# mins <- which(oct13$verFeature>=0.0005083 & oct13$horFeature<0.001624 & 
#                 oct13$H.peakFreq.>=0.7339)

verFeature <- oct13$verFeature[mins]
horFeature <- oct13$horFeature[mins]
iH.peakFreq <- 1 - oct13$H.peakFreq.[mins]
speciesCount <- oct13$speciesNum[mins]
index <- 1:length(verFeature)
rm(oct13)

toRank <- cbind(index, mins, speciesCount, verFeature, horFeature, iH.peakFreq)
toRank <- data.frame(toRank)
rm(index)

# rankIndex <- with(toRank, order(verFeature, iH.peakFreq, decreasing=TRUE))
# ranked <- toRank[rankIndex, ]
# 
oct13.calls <- read.csv("C:\\Work\\myfile\\SE\\BirdCallCounts_SERF_Oct2010\\SE_2010Oct13_Calls.csv")
colname <- names(oct13.calls)
calls <- oct13.calls[ ,colname[4:80]]
rm(oct13.calls, colname)
# rankedCalls <- calls[ranked$mins, ]


#there are only 1435 rows of valid data, be careful!
test <- toRank[ , c(4,5,6)]
rankedIndices <- greedyBird(test)
rankedMin <- rankMin(rankedIndices, test)
ranked <- toRank[rankedMin, ]
rankedCalls <- calls[ranked$mins, ]
Acurve <- drawAC(rankedCalls, nrow(rankedCalls))

remainMin<-deleteMin(ranked$mins)
