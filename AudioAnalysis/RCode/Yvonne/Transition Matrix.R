# 24 July 2015
#
# Transition Matrix and transition table
# This follows the kmeans clustering 
#
######## changes these values ########################
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
vec <- read.csv("normIndicesClusters ,Gympie NP1 ,22-28 June 2015.csv", 
                header=T)
timePeriod <- 2880:4320  # range in minutes
site <- "GympieNP Day 3"  # used as a label to save files

#####################################################
clusters <- vec$unname.kmeansObj.cluster.[timePeriod]

m <- matrix(0, 30, 30)

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
    if (m[x,y] > 0) 
     {
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
  trans <- paste((source1[i]), ":  ", paste(as.character(rep(target[i], value[i])), 
                 collapse = " "))
  transitions <- c(transitions, trans)
}

write.table (transitions, file = paste("Transitions_",
            site, ".csv", sep = ""))

write.csv(m, (file=paste("Transition_Matrix_", 
          site, ".csv", sep = "")))
write.csv(value1, (file=paste("Transition_Matrix_table", 
          site, ".csv", sep = "")))