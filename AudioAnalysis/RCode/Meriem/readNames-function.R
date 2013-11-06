  readNames<-function(){

  #as.is="File.Name"
  
  FileNames <- read.csv(file="C:/Work/csv files/266TestingAmbiguousFiles23April.csv", header=TRUE, sep=",", colClasses=c("Name"="character"))

  training.FileNames <- FileNames[FileNames$Training...test == 1,]

  test.FileNames <- FileNames[FileNames$Training...test == 0,]
  
  return(list(all = FileNames, test = test.FileNames, training = training.FileNames))

#return(list(all = FileNames))
 }
