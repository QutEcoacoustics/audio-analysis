#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# This code plots network diagrams 
# from cluster lists
# 12 January 2016
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Read cluster list -----------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
#rm(list = ls())

k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
rm(k1_value, k2_value)
# remove unneeded values
load(file_name)
# load the cluster list corresponding to k1 and k2 value
cluster_list <- get(file_name_short, envir=globalenv())[,column]

# remove unneeded objects from global environment
rm(hclust_clusters_25000, file_name, file_name_short, column)
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Reconstitute the cluster list by adding in missing minutes ----
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# load missing minute reference list 
load(file="data/datasets/missing_minutes_summary_indices.RData")
# load minutes where there was problems with both microphones
microphone_minutes <- c(184321:188640)
# list of all minutes that have been removed previously
removed_minutes <- c(missing_minutes_summary, microphone_minutes)
rm(microphone_minutes, missing_minutes_summary) 

full_length <- length(cluster_list) + length(removed_minutes)
list <- 1:full_length
list1 <- list[-c(removed_minutes)]
reconstituted_cluster_list <- rep(0, full_length)

reconstituted_cluster_list[removed_minutes] <- NA
reconstituted_cluster_list[list1] <- cluster_list
cluster_list <- reconstituted_cluster_list
rm(reconstituted_cluster_list)

cluster_list_Gympie <- cluster_list[1:(length(cluster_list)/2)]
cluster_list_Woondum <- cluster_list[((length(cluster_list)/2)+1):length(cluster_list)]
rm(list, list1, full_length, removed_minutes)
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a list of dates ---------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
# IMPORTANT:  Ensure the mapping file contains the mapping of 
# files at least to the finish date
finish <-  strptime("20160723", format="%Y%m%d") 
dates <- seq(start, finish, by = "1440 mins")
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}
dates <- date.list
rm(dat,start, finish, i, date.list)
Gym_dates <- NULL
for(i in 1:length(dates)) {
  Gym_dat <- paste("Gym_",dates[i], sep = "")
  Gym_dates <- c(Gym_dates, Gym_dat)
}
Woon_dates <- NULL
for(i in 1:length(dates)) {
  Woon_dat <- paste("Woon_",dates[i], sep = "")
  Woon_dates <- c(Woon_dates, Woon_dat)
}
rm(dates, i, Gym_dat, Woon_dat, cluster_list)

# Gympie matrix
Gym_matrix <- matrix(cluster_list_Gympie, nrow = 398, ncol = 1440,
                     byrow = TRUE)
Gym_matrix <- cbind(Gym_dates,Gym_matrix)
# Woondum matrix
Woon_matrix <- matrix(cluster_list_Woondum, nrow = 398, ncol = 1440,
                      byrow = TRUE)
Woon_matrix <- cbind(Woon_dates, Woon_matrix)

# Transpose the matrices
Gym_matrix <- t(Gym_matrix)
Woon_matrix <- t(Woon_matrix)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# occupancy matrix ------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
Gym_summary <- matrix(data = NA, nrow=398, ncol=60)
for (i in 1:398) {
  t <- table(Gym_matrix[2:length(Gym_matrix[,1]),i])
  for(j in 1:length(names(t))) {
    k <- as.numeric(names(t[j]))
    Gym_summary[i,k] <- as.vector(t[j])
  }
}

Gym_summary <- t(Gym_summary)

clusters <- c("01","02","03","04","05",
              "06","07","08","09",
              as.character(10:60))
Gym_summary <- cbind(clusters, Gym_summary)
colnames(Gym_summary) <- c("cluster", Gym_dates)
#write.csv(Gym_summary, "Gym_summary.csv", row.names = FALSE)

Woon_summary <- matrix(data = NA, nrow=398, ncol=60)
for (i in 1:398) {
  t <- table(Woon_matrix[2:length(Woon_matrix[,1]),i])
  for(j in 1:length(names(t))) {
    k <- as.numeric(names(t[j]))
    Woon_summary[i,k] <- as.vector(t[j])
  }
}

Woon_summary <- t(Woon_summary)
Woon_summary <- cbind(clusters, Woon_summary)
colnames(Woon_summary) <- c("cluster", Woon_dates)
#write.csv(Woon_summary, "Woon_summary.csv", row.names = FALSE)

###############
# set up empty empty matrices for authority and hub scores
auth_score <- matrix(data = NA, nrow=27, ncol=12)
hub_score <- matrix(data = NA, nrow=27, ncol=12)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Complete network diagrams all with the same layout-----------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#remove
#colours <- read.csv("data\\datasets\\Cluster_features_colourblind_version.csv", header = T)
#colours <- colours[,c(1,8)]

# old clusters
#rain <- c(2,10,18,21,38,54,59,60)
#wind <- c(8,9,19,20,24,25,28,30,40,42,45,46,47,51,52,56)
#birds <- c(3,4,11,14,15,33,37,39,43,57,58)
#insects <- c(1,17,22,26,27,29)
#cicada <- c(7,12,16,32,34,44,48)
#quiet <- c(5,6,13,31,35,36,41,50,53,55)
#plane <- c(23, 49)
#na <- 61

# This is the final listing
# 38 removed 17 added 
rain <- c(2,10,17,18,21,54,59,60) 
# 8,28 removed
wind <- c(9,19,20,24,25,30,40,42,45,46,47,51,52,56)
# 4 removed 28 added
birds <- c(3,11,14,15,28,33,37,39,43,57,58)
# 17 removed 4 added
insects <- c(1,4,22,26,27,29)
# 8 added
cicada <- c(7,8,12,16,32,34,44,48)
plane <- c(23,49)
#  38 added
quiet <- c(5,6,13,31,35,36,38,41,50,53,55)
na <- 61

insect_col <- "#F0E442"
rain_col <- "#0072B2"
wind_col <- "#56B4E9"
bird_col <- "#009E73"
cicada_col <- "#E69F00"
quiet_col <- "#999999"
plane_col <- "#CC79A7"
na_col <- "white"

library(plotrix)
library(igraph) #graph_from_data_frame function
library(diagram) # for curvedarrow function
library(grDevices)
# make layout for plot
g <- make_star(41)
conc_circles <- layout_as_star(g)
conc_circles <- rbind(conc_circles, conc_circles[seq(2,41,2),])
conc_circles[42:61,] <- 0.6*conc_circles[seq(2,41,2),]
# move row 1 to end
conc_circles <- rbind(conc_circles[2:61,])
conc_circles <- 5*conc_circles
# rearrange co-ordinates to bring all of the clusters 
# in one class together
# rearrange circles
# size of each circle

# layout with concentric circles
num <- c(length(c(wind, plane, rain)), 
         length(c(birds, quiet)),
         length(c(insects, cicada)))
# g is a igraph for the outer concentric circle
g <- make_star(num[1]+1)
# star layout has a circle with one point in centre
conc_circles <- layout_as_star(g)
# remove the point in the centre
conc_circles <- conc_circles[2:nrow(conc_circles),]
# g is a igraph for the middle concentric circle
h <- make_star(num[2]+1)
conc_circles2 <- layout_as_star(h)
conc_circles2 <- conc_circles2[2:nrow(conc_circles2),]
conc_circles2 <- conc_circles2*0.7          #num[2]/num[1]
# i is a igraph for the innermost concentric circle
i <- make_star(num[3]+1)
conc_circles3 <- layout_as_star(i)
conc_circles3 <- conc_circles3[2:nrow(conc_circles3),]
conc_circles3 <- conc_circles3*0.4     #num[3]/num[1]
conc_circles <- rbind(conc_circles, conc_circles2,
                      conc_circles3)
conc_circles <- 5*conc_circles

list <- c(plane, rain, wind, birds, quiet,
          insects, cicada)
#list <- c(insects, rain, wind, quiet, cicada, 
#          plane, birds)
conc_circles1 <- conc_circles
for(i in 1:length(list)) {
  conc_circles1[list[i],] <- conc_circles[i,]   
}
conc_circles <- conc_circles1

# for the concentric circle layout -------------------
#library(grid)
site <- "GympieNP"
#site <- "WoondumNP"
for(i in 1:5) {    #398) {
  if(site == "GympieNP") {
    png(paste("network_diagrams\\3", Gym_dates[i],"_network.png", sep = ""), 
        height=1200, width=1200)
    par(mar=c(2,0,0,0))
    # A is a vector of the Gympie cluster list for one day
    # B is a vector one minute on from A
    A <- as.numeric(unname(Gym_matrix[2:(length(Gym_matrix[,1])-1), i]))
    B <- as.numeric(unname(Gym_matrix[3:length(Gym_matrix[,1]), i]))
  }
  if(site == "WoondumNP") {
    png(paste("network_diagrams\\3", Woon_dates[i],"_network.png", sep = ""), 
        height=1200, width=1200)
    par(mar=c(2,0,0,0))
    par(mar=c(2,0,0,0))
    A <- as.numeric(unname(Woon_matrix[2:(length(Woon_matrix[,1])-1), i]))
    B <- as.numeric(unname(Woon_matrix[3:length(Woon_matrix[,1]), i]))
  }
  
  if(all(is.na(A)==FALSE)) {
    g1 <- data.frame(A=A,B=B)
    # create an igraph from the edge list g1
    g <- graph_from_data_frame(g1, directed=TRUE, vertices = NULL)
  }
  # list of the number of times one cluster transitions into 
  # every other cluster including itself
  datafr1 <- NULL
  for (j in 1:60) {
    for(k in 1:60) {
      a <- which(as.integer(A)==j & as.integer(B)==k) 
      datafr1 <- c(datafr1, length(a))
    }
  }
  # list of 1 to 60 each 60 times
  datafr2 <- rep(1:60, each=60)
  # list of 1 to 60 repeated 60 times
  datafr3 <- rep(1:60, 60)
  
  # remove from list any with less than 10 transitions
  a <- which(datafr1 < 10)
  datafr1 <- datafr1[-a]
  datafr2 <- datafr2[-a]
  datafr3 <- datafr3[-a]
  # list any transitions from one cluster to the same cluster
  # and save these as a special dataframe and then remove these
  # from the original dataframes
  a <- which(datafr2==datafr3)
  datafr1_equals <- datafr1[a]
  datafr2_equals <- datafr2[a]
  datafr1 <- datafr1[-a]
  datafr2 <- datafr2[-a]
  datafr3 <- datafr3[-a]
  
  # start plot
  plot(0, type="n", ann = FALSE, xlim = c(-5.2, 5.2), 
       ylim = c(-5.2, 5.2), axes = FALSE)
  #title
  if(site=="GympieNP") {
    mtext(side = 1, Gym_dates[i], cex = 2.2)  
  }
  if(site=="WoondumNP") {
    mtext(side = 1, Woon_dates[i], cex = 2.2)  
  }
  text(x = 4, y = -5.2, cex = 1.6,
       substitute(paste(italic("37 & 43 = Morning chorus"))))
  # draw loops for cycling within one cluster
  conc_circles_equals <- conc_circles[datafr2_equals,]
  if(nrow(conc_circles_equals) > 0) {
    for (n in 1:length(datafr2_equals)) {
      from <- conc_circles[datafr2_equals[n], ]
      to <- from
      from[[1]] <- from[[1]] + 0.2
      from[[2]] <- from[[2]] + 0.15
      to[[1]] <- to[[1]] - 0.2
      to[[2]] <- to[[2]] + 0.15
      lwd = datafr1_equals[n]/14
      col <- "black"
      C <- curvedarrow(from = from, to = to,
                       curve = 0.6, arr.pos = 0.5, lcol=col, 
                       segment = c(0.01,0.99), lwd = lwd, 
                       arr.width=0,
                       arr.length=0)
      x <- C[[1]]
      y <- C[[2]] + 0.08
      draw.circle(x = x, y = y, radius = 0.18, col = "white",
                  border = "white")
      text(x=x, y=y, paste(datafr1_equals[n]), cex = 1.5)
    }
  }
  for(l in 1:nrow(conc_circles)) {
    if(l %in% insects) {
      col <- insect_col
    }
    if(l %in% cicada) {
      col <- cicada_col
    }
    if(l %in% plane) {
      col <- plane_col
    }
    if(l %in% wind) {
      col <- wind_col
    }
    if(l %in% birds) {
      col <- bird_col
    }
    if(l %in% quiet) {
      col <- quiet_col
    }
    if(l %in% rain) {
      col <- rain_col
    }
    draw.circle(x =conc_circles[l,1],
                y = conc_circles[l,2],
                radius = 0.25, col = col)
  }
  if(length(datafr1) > 0) {
    for (n in 1:length(datafr1)) {
      from <- datafr2[n]
      to <- datafr3[n]
      lwd = datafr1[n]/14
      col <- "black"
      C <- curvedarrow(from = conc_circles[from, ], to = conc_circles[to, ],
                  curve = 0.25, arr.pos = 0.5, lcol=col, 
                  segment = c(0.035,0.965), lwd = lwd, arr.width=lwd/8,
                  arr.length=lwd/8)
      x <- C[[1]] + 0.23
      y <- C[[2]]
      text(x=x, y=y, paste(datafr1[n]), cex = 1.5)
    }
    morning_chorus <- c(37,43)
    for(p in list) {
      if(!p %in% morning_chorus) {
        text(as.character(p),
             x = conc_circles[p,1],
             y = conc_circles[p,2],
             cex = 2.5)  
      }
      if(p %in% morning_chorus) {
        text(as.character(p),
             x = conc_circles[p,1],
             y = conc_circles[p,2],
             cex = 3)  
      }
    }
    rm(g, g1)
  }
  dev.off()
}

