read.multilabel.prediction<-function(labels, filepath){
  # create connection and read lines
  # read meka multi-label predictions
  # con<-file("C:\\Users\\n8781699\\Downloads\\meka-release-1.7.7\\meka-1.7.7-SNAPSHOT\\prediction_oct15.arff",open="r")
  con<-file(filepath, open='r')
  line<-readLines(con)

  # tidy up
  close(con)

  # labels <- 5
  row.num <- as.numeric(sub('.*N=([[:digit:]]+).*', '\\1', line[1]))
  predicted.annotations <- matrix(0, nrow=row.num, ncol=labels)
  for(i in 2:(row.num + 1)){
  # for(i in 2:5){
    predict <- sub('.*\\[.*\\] \\[(.*)]', '\\1', line[i])
    presence <- as.numeric(unlist(strsplit(predict, split=', ')))
    predicted.annotations[i - 1, presence + 1] <- 1 
  }
}