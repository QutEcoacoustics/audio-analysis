 
# Function to calculate the moving average window

 filterMovingAverage <- function(signal, width) {
   lengthSignal <- length(signal)
   if(width <= 1){
     #signal<-signal
     return(signal) 
   }
   
     if(lengthSignal <= 3){
       return(signal)
     }
       
  # Create an empty vector for filtred signal     
    filtredSignal <- rep(0, lengthSignal)
    edge <- floor(width/2)  
     
   
  # Filter leading edge(left side)

  for(i in 1 : edge){
   sum <- 0.0
   for(j in 1:(i+edge+1)){     
     sum <- sum+signal[j] 
   }
   
   filtredSignal[i] <- sum/(i+edge+1)
 } 
  #print(filtredSignal)
   
  #Filter the midlle portion
   for(i in (edge+1):(lengthSignal-edge)){
    sum <- 0.0
    for(j in 1 :width){
      sum <- sum+ signal[i-edge+j-1]
    }
    filtredSignal[i] <- sum/width
  }
   #print(filtredSignal)
 
 # Filter trailing edge
   #print(lengthSignal)
   #print(edge)
 for(i in (lengthSignal-edge+1) : lengthSignal){
   sum <- 0.0
   #print(i)
    for(j in i : lengthSignal){
     #print(j)
     sum <- sum + signal[j]
     #print(sum)
     
    }
    filtredSignal[i] <- sum/(lengthSignal-i+1)
    #print(filtredSignal[i])
   
 }
  #print(filtredSignal)
   return(filtredSignal)
 
 }
    
  
 
 
 
 
 
 
 
 