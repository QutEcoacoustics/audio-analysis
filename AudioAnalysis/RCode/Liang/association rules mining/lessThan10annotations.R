#preprocess arules

training<-read.csv("C:\\Work\\myfile\\SERF_callCount_20sites_fulllist\\training.csv", check.names=FALSE)
index <- numeric()

for(i in 1:ncol(training)){
  if(length(which(training[,i]==1)) < 10){
    index <- c(index, i)
  }
}