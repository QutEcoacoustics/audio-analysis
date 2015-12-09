calculate.local.maxima <- function(indices, len=20){
  #indices: row is the frequency, column is the minute
  #len: the window length of moving average
  
  sides<-1         # a parameter of filter function
  local.maxima<-numeric()
  indices[1:5, ] <- 0           #remove low frequency noise
  indices[252:256, ] <- 0       #remove high frequency noise

  for(i in 1:ncol(indices)){
    #remove the mean frequency information
    indices[,i]<-indices[,i]-mean(indices[10:240,i])
#     indices[,i]<-indices[,i]-(mean(indices[10:240,i]+3*sd(indices[10:240,i])))
    temp<-which(indices[,i]<0)
    indices[temp,i]<-0

    #use a moving average to smooth the frequency, window length is 'len'
    average<-filter(indices[,i], rep(1,len), sides=sides)/3

    #calculate the lcoal maxima by taking the second order deviation
    deviation<-diff(sign(diff(average)))
    local.maxima<-c(local.maxima, length(which(deviation==-2)))
  }
  return(local.maxima)
}