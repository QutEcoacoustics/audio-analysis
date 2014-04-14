ridgeCnts <- function(filepath, ridgeDirection='North'){
  
  # parse the file name, sort them from min 0 to 1435
  filename <- dir(path = filepath, pattern = "min.Event")
  fileOrder <- rep(0, length(filename))
  for(i in 1:length(filename)){
    fileOrder[i] <- sub(".*_([[:digit:]]+).*", "\\1", filename[i])
  }
  fileOrder <- as.numeric(fileOrder)
  newOrder <- sort(fileOrder, index.return=TRUE)
  filename <- filename[newOrder[[2]]]
  
  for(cnt in 1:length(filename)){
    file <- paste(filepath, '/', filename[cnt], sep='')
    ridgeInfo <- read.csv(file)
    #     ridgeInfo <- ridgeInfo[ , 1:4]  # col1 is frame, col2 is Fbin, col3 is magnitude, col4 is direction
    #     frame <- as.numeric(ridgeInfo[ ,2])
    totalFrame <- as.numeric(ridgeInfo[1 ,6])
    totalBin <- as.numeric(ridgeInfo[1 ,1])
    fBin <- as.numeric(ridgeInfo[ ,3])
    orientation <- as.character(ridgeInfo[ , 5])
    #     magnitude <- as.numeric(ridgeInfo[ ,4])
    #     direction <- ridgeInfo[ ,5]

    features <- rep(0, totalBin + 1)
    
    for(i in 1:nrow(ridgeInfo)){
      if(ridgeDirection == orientation[i]){
        features[fBin[i]] <- features[fBin[i]] + 1
      }
    }
    
    ridgeIndex <- features / totalFrame
    if(cnt == 1){
      indices <- ridgeIndex
    }
    else{
      indices <- cbind(indices, ridgeIndex)
    }
    
  }
  
  indices <- t(indices)
  indices[ , 1] <- 0:(nrow(indices)-1)
  
  cNames <- 0:(totalBin - 1)
  cNames <- sprintf('%06d', cNames)
  cNames <- paste('h', cNames, sep='')
  cNames <- c('Index', cNames)
  
  saveFile <- sub(".*_([[:digit:]]+).*", "\\1", filepath)
  write.table(indices, paste("c:/work/myfile/SE/summary/ridgeCntS/", saveFile, '_', ridgeDirection, ".csv",sep=''), 
              row.names=FALSE, col.names=cNames, sep=",")
#   return(indices)
}