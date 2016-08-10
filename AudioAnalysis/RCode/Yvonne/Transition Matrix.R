# 24 July 2015
#
# This code generates three (3) csv files
# 1. A list of all transitions in the form of a stem and leaf plot
# 2. A transition Matrix - a non-symmetrical matrix which shows the 
#    number of time positive transitions from one cluster to another 
#    over a period of time
# 3. A transiton table listing the source, target and value
# It can be used to study associations between the clusters, which can 
# also be studied using the Network code I wrote for Mangalam
#   This file is #10 in the sequence:
#   1. Save_Summary_Indices_ as_csv_file.R
#   2. Plot_Towsey_Summary_Indices.R
#   3. Correlation_Matrix.R
#   4. Principal_Component_Analysis.R
#   5. kMeans_Clustering.R
#   6. Quantisation_error.R
#   7. Distance_matrix.R
#   8. Minimising_error.R
#   9. Segmenting_image.R
# *10. Transition Matrix 

#
######## changes these values ########################
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015.csv", 
                header=T)
timePeriod <- 2880:4320  # range in minutes
site <- "GympieNP Day 3"  # used as a label to save files

#####################################################
clusters <- vec$unname.kmeansObj.cluster.[timePeriod]

# Create an empty matrix
m <- matrix(0, 30, 30)

# Fill the transition matrix
for (i in 1:length(clusters)) {
  x <- clusters[i]
  y <- clusters[i+1]
  m[x,y] <- m[x,y] + 1
}

#View(m)

####################################
#Convert to two column data.frame
value <- NULL
source1 <- NULL
target <- NULL

for (x in 1:30) {
  for (y in 1:30) {
    if (m[x,y] > 0) {
      a <- m[x,y]
      source1 <- c(source1, x)
      target <- c(target, y)
      value <- c(value, a) 
    }
  } 
}

value1 <- cbind(source1, target, value)

transitions <- NULL

a <- sort(value1[,2])

for (i in seq_along(a)) {
  #b <- as.character(rep(target[i], value[i]))
  trans <- paste((source1[i]), ":  ", 
                 paste(as.character(rep(target[i], value[i])), 
                 collapse = " "))
  transitions <- c(transitions, trans)
}

write.table (transitions, file = paste("Transitions_",
            site, ".csv", sep = ""))

write.csv(m, (file=paste("Transition_Matrix_", 
          site, ".csv", sep = "")))
write.csv(value1, (file=paste("Transition_Matrix_table", 
          site, ".csv", sep = "")))