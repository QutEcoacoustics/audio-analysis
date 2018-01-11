# Title:  Hybrid Clustering
# Author: Yvonne Phillips
# Date:  19 September 2016

# Description:  This code applies a Hybrid method to cluster a large acoustic
#   dataset.  The hybrid method involves three steps:
#   1. Partition the dataset using k-means into a large number of clusters
#   2. Apply hierarchical clustering to centroids from step 1 to reduce to
#      a number of clusters less than 100
#   3. Assign all observations to the nearest centroid using knn (k-nearest-
#      neighbour).
#   The final number of clusters in step 2 is determined evaluating the 
#   within-group similarity of sets of three days within a twelve day dataset.
#   The twelve day dataset consists of three consecutive or near consecutive 
#   days that show high similarity in the twenty-four hour spectrograms.  The
#   clustering should maintain the similarity of the three days which should be
#   distinct from another three day set.  
# Note:  the code that generated the normalised .Rdata files 
# see the end of code
load(file="data/datasets/kmeans_results/kmeansclusters25000.RData")

hybrid_clusters <- read.csv("hybrid_dataset_centers_25000_60.csv", header = T)[,1]
clusters25000$clust <- 0
for(i in 1:length(unique(hybrid_clusters))) {
  a <- which(hybrid_clusters==i)
  for(j in 1:length(a)) {
    b <- which(clusters25000$cluster==a[j])
    clusters25000$clust[b] <- i
  }
}
new_cluster_list25000_60 <- clusters25000$clust
new_cluster_list25000_60 <- data.frame(new_cluster_list25000_60)
write.csv(new_cluster_list25000_60, "new_cluster_list.csv", row.names = F)

k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list 
cluster_list <- get(file_name_short, envir=globalenv())[,column]

new_cluster_list25000_60$old_list <- cluster_list
write.csv(cluster_list, "cluster_list.csv", row.names = F)
diff <- NULL
for(i in 1:length(cluster_list)) {
  diff[i] <- new_cluster_list25000_60[i] - as.numeric(cluster_list)[i]  
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Load the SUMMARY indices ---------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

# load normalised summary indices this has had the missing minutes
# and microphone problem minutes removed 
# the dataframe is called "indices_norm_summary"
load(file="data/datasets/normalised_summary_indices.RData")
colnames(indices_norm_summary)
length(indices_norm_summary[,1])
# load normalised spectral indices
# load(file="data/datasets/normalised_spectral_indices.RData")

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Hybrid Method -------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#Step 1 Partitioning using kmeans

#library(MASS)
########set-up the variables
k1 <- i <- c(12500, 15000, 17500, 20000, 22500, 25000, 27500, 30000, 40000)
k2 <- seq(5, 100, 5)

k1 <- 25000
k2 <- 60

paste(Sys.time(), " Starting kmeans clustering", sep = " ")
for (k in 1:1) {
  set.seed(123)
  kmeansObj <- kmeans(indices_norm_summary, centers = k1[k], iter.max = 100)
  paste(Sys.time(), " End of kmeans clustering", k1[k], sep = " ")
  list <- c("clusters","centers","totss","withinss","totwithinss",
            "betweenss", "size", "iter", "ifault")
  for(i in 1:length(list)){
    list[i] <- paste(list[i],k1[k],sep = "")
  }
  for (i in 1:length(list)) {
    assign(paste(list[i],sep = ""), kmeansObj[i])
    save(list = list[i], file = paste("data/datasets/kmeans_results/kmeans", list[i],".Rdata",sep=""))
  }
  for (i in 1:length(list)) {
    rm(list = list[i]) 
  }
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Step 2:  Hierarchical clustering of centers ---------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
paste(Sys.time(), "Starting hclust")
folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\kmeans_results"

kmeansCenters <- kmeansObj$centers
kmeansCenters <- as.hclust(centers25000)
# Hierarchically cluster the centers from kmeans
hybrid.fit.ward <- hclust(dist(kmeansCenters), method = "ward.D2")

paste(Sys.time(), "Starting cutree function")

clusters <- NULL
for (j in k2) {
  # cut the dendrogram into k2 clusters
  hybrid.clusters <- cutree(hybrid.fit.ward, k=j)
  # generate the test dataset
  hybrid.dataset <- cbind(hybrid.clusters, kmeansCenters)
  hybrid.dataset <- as.data.frame(hybrid.dataset)
  write.csv(hybrid.dataset, paste("hybrid_dataset_centers_", k1[k], "_", j,
                                  ".csv",sep=""), row.names = FALSE)
  train <- hybrid.dataset
  test <- indices_norm_summary
  # set up class labels
  cl <- factor(unname(hybrid.clusters))
  library(class)
  # set the k value for the knn function 
  k3 <- sqrt(floor(nrow(train)))
  is.even <- function(x) x %% 2 == 0
  if(is.even(k3)=="TRUE") {
    k3 <- k3 - 1
  }
  clusts <- knn(train[,-1], test, cl, k = k3, prob = F)
  clusters <- cbind(clusters, clusts)
}
colnames(clusters) <- c("clust5", "clust10","clust15","clust20",
                        "clust25","clust30","clust35","clust40",
                        "clust45","clust50","clust55","clust60",
                        "clust65","clust70","clust75","clust80",
                        "clust85","clust90","clust95","clust100")
assign(paste("hclust_clusters_", k1[k], sep = ""), clusters)

save(list = paste("hclust_clusters_", k1[k], sep = ""), 
     file = paste("data/datasets/hclust_results/hclust_clusters", 
                  k1[k],".Rdata",sep=""))
paste(Sys.time(), " Finishing hclust clustering", sep = " ")

# Instead of knn the centers should have been used to assign the 
# whole dataset


################################################################
################################################################




clusters <- NULL
for (j in k2) {
  hybrid.clusters <- cutree(hybrid.fit.ward, k=j)
  # generate the test dataset
  hybrid.dataset <- cbind(hybrid.clusters, centers10000)
  hybrid.dataset <- as.data.frame(hybrid.dataset)
  write.csv(hybrid.dataset, paste("hybrid_dataset_centers_", i, "_", j,
                                  ".csv",sep=""), row.names = FALSE)
  train <- hybrid.dataset
  test <- ds3.norm_2_98
  # set up class labels
  cl <- factor(unname(hybrid.clusters))
  library(class)
  clusts <- knn(train[,-1], test, cl, k = k3, prob = F)
  clusters <- cbind(clusters, clusts)
  ############### end knn method
}
# produce column names
column.names <- NULL
for (i in i) {
  for (j in k2) {
    col.names <- paste("hybrid_k", i, "k", j,"k",k3, sep = "")
    column.names <- c(column.names,col.names)
  }
}
colnames(clusters) <- column.names
#value.ref <- which(!is.na(ds3$BackgroundNoise))
#clusters <- cbind(value.ref, clusters)
write.csv(clusters, file = paste("hybrid_clust_knn_",i,"_",k3,".csv",sep = ""),
          row.names = F)









##############################################
# Normalise the selected summary indices
#############################################
# The code that follows shows how the normalised summary indices file
# were saved
# load all of the summary indices as "indices_all"

load(file="data/datasets/summary_indices.RData")
# remove redundant indices
remove <- c(1,4,11,13,17:19)
indices_all <- indices_all[,-remove]
rm(remove)

# IMPORTANT:  These are used to name the plots
site <- c("Gympie NP", "Woondum NP")
index <- "SELECTED_Final" # or "ALL"
type <- "Summary"
paste("The dataset contains the following indices:"); colnames(indices_all)

# Generate a list of the missing minutes in summary indices
#missing_minutes_summary <- which(is.na(indices_all[,1]))
#save(missing_minutes_summary, file = "data/datasets/missing_minutes_summary_indices.RData")
# load missing_minutes_summary
load(file="data/datasets/missing_minutes_summary_indices.RData")
# There were 3 days where both microphones were not functioning correctly
# following rain.  These days were the 28, 29 and 30 October 2015
microphone_minutes <- c(184321:188640)
remove_minutes <- c(missing_minutes_summary, microphone_minutes)
# remove NA values
indices_all <- indices_all[-c(remove_minutes),]

# List of summary indices columns:
#[1] "AvgSignalAmplitude"         [2]  "BackgroundNoise"          
#[3] "Snr"                        [4]   "AvgSnrOfActiveFrames"     
#[5] "Activity"                   [6]   "EventsPerSecond"          
#[7] "HighFreqCover"              [8]   "MidFreqCover"             
#[9] "LowFreqCover"               [10]  "AcousticComplexity"       
#[11] "TemporalEntropy"           [12]  "EntropyOfAverageSpectrum" 
#[13] "EntropyOfVarianceSpectrum" [14]  "EntropyOfPeaksSpectrum"   
#[15] "EntropyOfCoVSpectrum"      [16]  "ClusterCount"             
#[17] "ThreeGramCount"            [18]  "NDSI"                     
#[19] "SptDensity" 

#############################################
# Normalise the selected Spectral indices
#############################################
# remove all objects in the global environment
rm(list = ls())

# load the spectral indices as "indices_all_spect"
load(file="data/datasets/spectral_indices.RData")

# remove all "ID" columns
a <- which(colnames(indices_all_spect)=="ID")
indices_all <- indices_all_spect[,-a]

# remove selected columns (see lists below)
remove <- c(5,9,12,19,31:36) # This is for the final26

indices_all <- indices_all[,-remove]
rm(remove)

# Generate a list of the missing minutes in spectral indices
#missing_minutes_spectral <- which(is.na(indices_all[,1]))
#save(missing_minutes_spectral, file = "data/datasets/missing_minutes_spectral_indices.RData")
#load(file="data/datasets/spectral_indices.RData")

# Important: These variables are used for naming files
site <- c("Gympie NP", "Woondum NP")
index <- "SELECTED_Final"
type <- "Spectral"

paste("The dataset contains the following indices:"); colnames(indices_all)

length(indices_all[,1])

# load missing_minutes_summary (the same as spectral)
load(file="data/datasets/missing_minutes_summary_indices.RData")

# remove NA values
microphone_minutes <- c(184321:188640)
remove_minutes <- c(missing_minutes_summary, microphone_minutes)
# remove NA values
indices_all <- indices_all[-c(remove_minutes),]

# List of spectral indices columns:
# [1] "ACI_0Hz"     [2] "ACI_1000Hz"   [3] "ACI_2000Hz"   [4] "ACI_4000Hz"
# [5] "ACI_6000Hz"  [6] "ACI_8000Hz"   [7] "BGN_0Hz"      [8] "BGN_1000Hz"
# [9] "BGN_2000Hz" [10] "BGN_4000Hz"  [11] "BGN_6000Hz"  [12] "BGN_8000Hz"
#[13] "ENT_0Hz"    [14] "ENT_1000Hz"  [15] "ENT_2000Hz"  [16] "ENT_4000Hz"
#[17] "ENT_6000Hz" [18] "ENT_8000Hz"  [19] "EVN_0Hz"     [20] "EVN_1000Hz"
#[21] "EVN_2000Hz" [22] "EVN_4000Hz"  [23] "EVN_6000Hz"  [24] "EVN_8000Hz"
#[25] "POW_0Hz"    [26] "POW_1000Hz"  [27] "POW_2000Hz"  [28] "POW_4000Hz"
#[29] "POW_6000Hz" [30] "POW_8000Hz"  [31] "SPT_0Hz"     [32] "SPT_1000Hz"
#[33] "SPT_2000Hz" [34] "SPT_4000Hz"  [35] "SPT_6000Hz"  [36] "SPT_8000Hz"

######### Normalise data #################################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

###########################################################
# Create a normalised dataset between 1.5 and 98.5% bounds 
###########################################################
indices_norm <- indices_all

# normalise values between 1.5 and 98.5 percentile bounds
q1.values <- NULL
q2.values <- NULL
for (i in 1:ncol(indices_all)) {
  q1 <- unname(quantile(indices_all[,i], probs = 0.015, na.rm = TRUE))
  q2 <- unname(quantile(indices_all[,i], probs = 0.985, na.rm = TRUE))
  q1.values <- c(q1.values, q1)
  q2.values <- c(q2.values, q2)
  indices_norm[,i]  <- normalise(indices_all[,i], q1, q2)
}
rm(q1, q2, i)

# adjust values greater than 1 or less than 0 to 1 and 0 respectively
for (j in 1:ncol(indices_norm)) {
  a <- which(indices_norm[,j] > 1)
  indices_norm[a,j] = 1
  a <- which(indices_norm[,j] < 0)
  indices_norm[a,j] = 0
}

#indices_norm_summary <- indices_norm
# save normalised summary indices
#save(indices_norm_summary, file = "data/datasets/normalised_summary_indices.RData")
# load normalised summary indices
#load(file="data/datasets/normalised_summary_indices.RData")

#indices_norm_spectral <- indices_norm
# save normalised spectral indices
#save(indices_norm_spectral, file = "data/datasets/normalised_spectral_indices.RData")
# load normalised spectral indices
#load(file="data/datasets/normalised_spectral_indices.RData")

###################################################