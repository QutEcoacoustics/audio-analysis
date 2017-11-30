#Calculate the centroids of the acoustic indices
# woondum = excel file /Volumes/Nifty/QUT/Yvonne_AcousticData_WoondumAndGympie/Cluster50NormalisedIndexFeatures_Gympie2015June22.csv

#create empty date frame where the centroids values will be stored
centroids = data.frame(matrix(ncol = 12, nrow =50))
#For different subsets of data based on cluster_list the mean of every column will be calculated
#The mean of every column for every subset is stored in centroids
for (col in seq(3,ncol(woondum))){
  centroids[,col-2] = aggregate(woondum[[col]] ~ cluster_list, mean, data=woondum)[2]
}

#Ignore the first two columns in woondum; cluster_list & minute_reference
colnames(centroids) = colnames(woondum)[3:14]

#create an empty data frame to store the medoids
medoids = data.frame(matrix(ncol = 12, nrow =50))

#For every subset
for (i in seq(1,50)){
  #create a subset of the data based on cluster number
  subset = woondum[which(woondum$cluster_list==i),]
  smallest_distance = 1000
  
  #Iterate through every row in the subset
  for (row in seq(1,nrow(subset))){
    #Calculate the distance between the current row in the subset and the centroid of the given subset
    distance = (dist(rbind(subset[row,3:14],centroids[i,])))
    print(distance)
    print(row)
    
    #Save the row that has the smallest distance as the medoid for that subset/cluster
    if (distance<smallest_distance){
      smallest_distance = distance
      medoids[i,] = subset[row,3:14]
    }
  }
}

distmatrix = dist(medoids)
hc = hclust(distmatrix)
