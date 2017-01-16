#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# This code plots network diagrams 
# from cluster lists
# 12 January 2016
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Read cluster list -----------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# remove all objects in the global environment
rm(list = ls())

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
write.csv(Gym_summary, "Gym_summary.csv", row.names = FALSE)

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
write.csv(Woon_summary, "Woon_summary.csv", row.names = FALSE)

###############
# set up empty empty matrices for authority and hub scores
auth_score <- matrix(data = NA, nrow=27, ncol=12)
hub_score <- matrix(data = NA, nrow=27, ncol=12)

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Complete all with the same layout-----------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
colours <- read.csv("data\\datasets\\Cluster_features_colourblind_version.csv", header = T)
colours <- colours[,c(1,8)]

rain <- c(59,18,10,54,2,21,38,60)
wind <- c(42,47,51,56,52,45,8,40,24,19,46,28,9,25,30,20)
birds <- c(43,37,57,3,58,11,33,15,14,39,4)
insects <- c(29,17,1,27,22,26)
cicada <- c(48,44,34,7,12,32,16)
plane <- c(49,23)
quiet <- c(13,5,6,53,36,31,50,35,55,41)
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
#library(grid)
site <- "GympieNP"
site <- "WoondumNP"
for(i in 1:398) {
  if(site == "GympieNP") {
    png(paste("network_diagrams\\", Gym_dates[i],"_network.png", sep = ""), 
        height=900, width=900)
    par(mar=c(2,0,0,0))
    A <- as.numeric(unname(Gym_matrix[2:(length(Gym_matrix[,1])-1), i]))
    B <- as.numeric(unname(Gym_matrix[3:length(Gym_matrix[,1]), i]))
  }
  if(site == "WoondumNP") {
    png(paste("network_diagrams\\", Woon_dates[i],"_network.png", sep = ""), 
        height=900, width=900)
    par(mar=c(2,0,0,0))
    A <- as.numeric(unname(Woon_matrix[2:(length(Woon_matrix[,1])-1), i]))
    B <- as.numeric(unname(Woon_matrix[3:length(Woon_matrix[,1]), i]))
  }
  if(all(is.na(A)==FALSE)) {
    g1 <- data.frame(A=A,B=B)  
    g <- graph_from_data_frame(g1, directed=TRUE, vertices = NULL)
  }
  
  datafr1 <- NULL
  
  for (j in 1:60) {
    for(k in 1:60) {
      a <- which(as.integer(A)==j & as.integer(B)==k) 
      datafr1 <- c(datafr1, length(a))
    }
  }
  
  datafr2 <- rep(1:60, each=60)
  datafr3 <- rep(1:60, 60)
  
  a <- which(datafr1 < 10)
  datafr1 <- datafr1[-a]
  datafr2 <- datafr2[-a]
  datafr3 <- datafr3[-a]
  a <- which(datafr2==datafr3)
  datafr1_equals <- datafr1[a]
  datafr2_equals <- datafr2[a]
  datafr1 <- datafr1[-a]
  datafr2 <- datafr2[-a]
  datafr3 <- datafr3[-a]
  g <- make_star(41)
  conc_circles <- layout_as_star(g)
  conc_circles <- rbind(conc_circles, conc_circles[seq(2,41,2),])
  conc_circles[42:61,] <- 0.5*conc_circles[seq(2,41,2),]
  # move row 1 to end
  conc_circles <- rbind(conc_circles[2:61,])
  conc_circles <- 5*conc_circles
  # start plot
  plot(0, type="n", ann = FALSE, xlim = c(-5.2, 5.2), 
       ylim = c(-5.2,5.2), axes = FALSE)
  #title
  if(site=="GympieNP") {
    mtext(side = 1, Gym_dates[i], cex = 2.2)  
  }
  if(site=="WoondumNP") {
    mtext(side = 1, Woon_dates[i], cex = 2.2)  
  }
  text(x = 4, y = -5.2, cex = 1.6,
       substitute(paste(italic("37 & 43 = Morning chorus"))))
  conc_circles_equals <- conc_circles[datafr2_equals,]
  if(nrow(conc_circles_equals) > 0) {
    for(j in 1:nrow(conc_circles_equals)) {
      col <- "red"
      col <- "black"
      draw.circle(x = conc_circles_equals[j,1], 
                  y = conc_circles_equals[j,2], 
                  radius = (0.25+datafr1_equals[j]/3000), 
                  col = col, border = col)
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
      col <- "red"
      col <- "black"
      C <- curvedarrow(from = conc_circles[from, ], to = conc_circles[to, ],
                  curve = 0.25, arr.pos = 0.5, lcol=col, 
                  segment = c(0.03,0.97), lwd = lwd, arr.width=lwd/8,
                  arr.length=lwd/8)
      x <- C[[1]] + 0.23
      y <- C[[2]]
      text(x=x, y=y, paste(datafr1[n]))
    }
    morning_chorus <- c(37,43)
    for(p in 1:nrow(conc_circles)) {
      if(!p %in% morning_chorus) {
        text(as.character(p),
             x = conc_circles[p,1],
             y = conc_circles[p,2],
             cex = 2)  
      }
      if(p %in% morning_chorus) {
        text(as.character(p),
             x = conc_circles[p,1],
             y = conc_circles[p,2],
             cex = 3.5)  
      }
    }
    rm(g, g1)
  }
  dev.off()
}
