ridgeCntFrequency <- function(filepath, ridgeDirection='East'){
  
  # parse the file name, sort them from min 0 to 1435
  filename <- dir(path = filepath, pattern = "min.Event")
  fileOrder <- rep(0, length(filename))
  for(i in 1:length(filename)){
    fileOrder[i] <- sub(".*_([[:digit:]]+).*", "\\1", filename[i])
  }
  fileOrder <- as.numeric(fileOrder)
  newOrder <- sort(fileOrder, index.return=TRUE)
  filename <- filename[newOrder[[2]]]
  indices <- numeric()
  
  for(cnt in 1:length(filename)){
    file <- paste(filepath, '/', filename[cnt], sep='')
    ridgeInfo <- read.csv(file)
    
    totalFreqBin<- ridgeInfo[1, 1]
    orientation <- as.character(ridgeInfo[ , 5])
    freqIndex <- ridgeInfo[ , 3]
    
    features <- rep(0, totalFreqBin)
    
    for(i in 1:nrow(ridgeInfo)){
      if(ridgeDirection == orientation[i]){
        features[freqIndex[i]] <- features[freqIndex[i]] + 1
      }
    }
    
  indices <- cbind(indices, features)
  }
  return(indices)
}

# maximum <- max(features)
# minimum <- min(features)
# separation <- seq(minimum, maximum, (maximum - minimum)/19)
# histogram <- hist(features, breaks=separation)


# ridgeInfo<-read.csv("C:\\Work\\myfile\\2014Aug15-135419Ridges\\SERF\\TaggedRecordings\\SE\\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\\Dong.RidgeDetection\\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000_415min.Events.csv")
# ggtitle('16th October')+theme(axis.text=element_text(size=10),axis.title=element_text(size=12, face='bold'))