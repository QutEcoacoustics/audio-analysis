# nimei<-numeric()

#similarity measure
threshold<-0.7

#default frequency components
calltypes.table <- rep(1, ncol(clusters.bases[[1]]))
calltypes <- clusters.bases[[1]]
# calltypes.table <- rep(1, ncol(remain.clusters.bases[[1]]))
# calltypes <- remain.clusters.bases[[1]]

#generate calltype table
for(i in 2:length(clusters.bases)){
# for(i in 2:length(remain.clusters.bases)){
  #create a new row
  if(i==2){
    calltypes.table <- rbind(calltypes.table, rep(0, length(calltypes.table)))
  }else{
    calltypes.table <- rbind(calltypes.table, rep(0, ncol(calltypes.table)))
  }
  
  #read the bases of a minute
  temp.bases <- clusters.bases[[i]]
#   temp.bases <- remain.clusters.bases[[i]]
  #calcualte its components
  len <- ncol(temp.bases)
  
  #search for new calltypes
  for(j in 1:len){
    #get correlation between a new component and existing ones
    temp.correlation <- cor(cbind(temp.bases[,j], calltypes))
    flag <- any(temp.correlation[2:nrow(temp.correlation), 1] >= threshold)
#     if(is.na(flag)){
#       nimei <- rbind(nimei, c(i,j))
#       break      
#     }
    if(flag){
      position <- which.max(temp.correlation[2:nrow(temp.correlation), 1])
      calltypes.table[nrow(calltypes.table), position] <- 1 + calltypes.table[nrow(calltypes.table), position]
    }else{
      zeros <- rep(0, nrow(calltypes.table))
      zeros[length(zeros)] <- 1
      calltypes.table <- cbind(calltypes.table, zeros)
      calltypes <- cbind(calltypes, temp.bases[,j])
    }
  }
}
  
