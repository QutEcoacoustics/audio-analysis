# remove all objects in global environment
rm(list = ls())

# load normalised summary inidces
load(file="data/datasets/normalised_summary_indices.RData")

# load the cluster list
# *** Set the cluster set variables
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
rm(file_name, file_name_short, k1_value, k2_value, column)
rm(hclust_clusters_25000)
indices_norm_summary$cluster_list <- cluster_list

colnames(indices_norm_summary)[] <- c("BGN","SNR","ACT",
                                      "EVN", "HFC", "MFC", "LFC",
                                      "ACI", "EAS", "EPS", "ECV",
                                      "CLC", "cluster_list")

# rearrange columns
indices_norm_summary <- indices_norm_summary[,c(1,2,3,4,7,6,5,8,9,10,11,12,13)]
#colnames(indices_norm_summary)

#colnames(indices_norm_summary) <- c("BGN", "SNR","ACT", "EVN",         
#                                    "HFC", "MFC", "LFC", "ACI",      
#                                    "EVS","EPS",  
#                                    "ECS","CLC","Cluster_list")
#indices_norm_summary <- data.frame(indices_norm_summary)

labels <- c("Cluster 1 - INSECTS",
            "Cluster 2 - LIGHT RAIN AND BIRDS",
            "Cluster 3 - BIRDS",
            "Cluster 4 - INSECTS AND BIRDS",
            "Cluster 5 - FAIRLY QUIET",
            "Cluster 6 - MOSTLY QUIET",
            "Cluster 7 - CICADAS AND BIRDS AND WIND",
            "Cluster 8 - CICADAS AND BIRDS AND WIND",
            "Cluster 9 - MODERATE WIND",
            "Cluster 10 - LIGHT TO MODERATE RAIN",
            "Cluster 11 - BIRDS",
            "Cluster 12 - CICADAS",
            "Cluster 13 - QUIET",
            "Cluster 14 - BIRDS",
            "Cluster 15 - BIRDS",
            "Cluster 16 - CICADAS",
            "Cluster 17 - LIGHT RAIN AND INSECTS",
            "Cluster 18 - MODERATE RAIN",
            "Cluster 19 - MODERATE WIND",
            "Cluster 20 - MODERATE WIND",
            "Cluster 21 - LIGHT RAIN",
            "Cluster 22 - INSECTS AND BIRDS",
            "Cluster 23 - PLANES, MOTORBIKES, THUNDER",
            "Cluster 24 - WIND AND CICADAS",
            "Cluster 25 - WIND",
            "Cluster 26 - INSECTS AND WIND",
            "Cluster 27 - INSECTS",
            "Cluster 28 - BIRDS AND/OR INSECTS OR PLANES",
            "Cluster 29 - INSECTS",
            "Cluster 30 - BIRDS and QUIET",
            "Cluster 31 - QUIET",
            "Cluster 32 - CICADAS",
            "Cluster 33 - BIRDS",
            "Cluster 34 - CICADAS",
            "Cluster 35 - QUIET",
            "Cluster 36 - QUIET AND PLANES ",
            "Cluster 37 - BIRDS - MORNING CHORUS",
            "Cluster 38 - QUIET",
            "Cluster 39 - BIRDS AND PLANES",
            "Cluster 40 - WIND AND BIRDS",
            "Cluster 41 - VERY QUIET",
            "Cluster 42 - STRONG WIND",
            "Cluster 43 - BIRDS - MORNING CHORUS",
            "Cluster 44 - LOUD CICADAS",
            "Cluster 45 - WIND AND PLANES",
            "Cluster 46 - WIND",
            "Cluster 47 - VERY STRONG WIND",
            "Cluster 48 - CICADAS",
            "Cluster 49 - PLANES (INCLUDING THUNDER)",
            "Cluster 50 - QUIET AND INSECTS AND BIRDS",
            "Cluster 51 - STRONG WIND",
            "Cluster 52 - WIND",
            "Cluster 53 - MOSTLY QUIET",
            "Cluster 54 - MODERATE RAIN AND BIRDS",
            "Cluster 55 - QUIET",
            "Cluster 56 - WIND",
            "Cluster 57 - BIRDS OR WIND",
            "Cluster 58 - LOUD BIRDS",
            "Cluster 59 - MODERATE RAIN",
            "Cluster 60 - RAIN AND BIRDS")

# colours for each class
insects_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
birds_col <- "#009E73"
cicadas_col <- "#E69F00"
quiet_col <- "#999999"
planes_col <- "#CC79A7"

# define cluster classes 
birds <- c(3,11,14,15,28,33,37,39,43,57,58)
insects <- c(1,4,22,26,27,29)
cicadas <- c(7,8,12,16,32,34,44,48)
rain <- c(2,10,17,18,21,54,59,60) 
wind <- c(9,19,20,24,25,30,40,42,45,46,47,51,52,56)
quiet <- c(5,6,13,31,35,36,38,41,50,53,55)
planes <- c(49,23)

for(i in 1:60) {
  a <- which(indices_norm_summary$cluster_list==i)
  if(i < 10) {
    tiff(paste("boxplots/image_0",i, ".tiff",sep = ""),
         width = 1200, height = 500)  
  }
  if(i >= 10) {
    tiff(paste("boxplots/image_",i, ".tiff",sep = ""),
         width = 1200, height = 500)  
  }
  par(mar=c(2,2.3,2,1), cex=1.6, cex.lab=6, cex.main=1.6)
  
  if(i %in% insects) {
    col = insects_col
  }
  if(i %in% cicadas) {
    col = cicadas_col
  }
  if(i %in% rain) {
    col = rain_col
  }
  if(i %in% wind) {
    col = wind_col
  }
  if(i %in% quiet) {
    col = quiet_col
  }
  if(i %in% birds) {
    col = birds_col
  }
  if(i %in% planes) {
    col = planes_col
  }
  boxplot(indices_norm_summary[a,1:(length(indices_norm_summary)-1)], 
          main = paste(labels[i]), width = rep(0.5,12), las=1,
          boxwex = 0.2, col = col)
  dev.off()
}

