#################################################
# This code plots network diagrams and calculates centrality measures
# from cluster lists
# 8 April 2016
#################################################
setwd("C:\\Work\\Mangalam_data\\")
data <- read.csv("Minute_cluster mapping - all.csv", header = TRUE)
#View(data)

# transpose data table
data <- t(data)

# packages needed for plots
library(igraph)
library(qgraph)

# packages needed to plot table
library(gridBase)
library(grid)
library(gridExtra)

####################################
# Set up layout with first data set
###################################
i=1
site <- data[1,i]
site
A <- unname(data[2:(length(data[,1])-1),i])
B <- unname(data[3:length(data[,1]), i])
g1 <- data.frame(A=A,B=B)

g <- graph_from_data_frame(g1, directed=TRUE, vertices = NULL)
#layout <- layout.fruchterman.reingold(g)
#layout <- layout.circle(g)
#layout <- layout.drl(g)
#layout <- layout.star(g,21)

# set up empty empty matrices for authority and hub scores
auth_score <- matrix(data=NA,nrow=27,ncol=12)
hub_score <- matrix(data = NA, nrow=27, ncol=12)

#####################################
# Complete all with the same layout
####################################
for(i in 1:12) {
site <- data[1,i]
site
A <- as.numeric(unname(data[2:(length(data[,1])-1),i]))
B <- as.numeric(unname(data[3:length(data[,1]), i]))
g1 <- data.frame(A=A,B=B)

g <- graph_from_data_frame(g1, directed=TRUE, vertices = NULL)
layout <- layout.fruchterman.reingold(g)
#plot(g, layout=layout, vertex.size=20, edge.arrow.size=.01)
#plot(g, layout=layout, edge.arrow.size=.08,
#     vertex.label=V(g)$name)

datafr <- NULL

A <- as.numeric(A)
B <- as.numeric(B)
for (j in 1:max(A)) {
  for(k in 1:max(B)) {
    a <- which(as.integer(A)==j & as.integer(B)==k) 
    datafr <- rbind(datafr, c(j,k, length(a)))
  }
}

#source("http://michael.hahsler.net/SMU/ScientificCompR/code/map.R")
auth <- authority_score(g)$vector
hub <- hub.score(g)$vector
for (l in 1:27) {
  a_score <- which(names(auth)==as.character(l))
  h_score <- which(names(hub)==as.character(l))
  if(length(a_score)>0) {
    auth_score[l,i] <- auth[a_score]
  }
  if(length(h_score)>0) {
    hub_score[l,i] <- hub[h_score] 
  }
}

betweenness <- read.csv("betweenness_across_sites.csv", header = T) 
closeness <- read.csv("closeness_across_sites.csv", header = T)
hubscore <- read.csv("hub_scores.csv", header = T)
authscore <- read.csv("authority_scores.csv", header = T)

node_names <- c("day","day","day","day","day","day","night",
                "night","night","day","night","night","night",
                "night","day","day","day","day","night","night",
                "day","day","day","day","night","day","day")

colour <- c("lightyellow2", "lightyellow2","lightyellow2",
            "lightyellow2","lightyellow2", "lightyellow2",
            "grey70","grey70","grey70","lightyellow2",
            "grey70","grey70","grey70","grey70","lightyellow2",
            "lightyellow2","lightyellow2","lightyellow2",
            "grey70","grey70","lightyellow2","lightyellow2",
            "lightyellow2","lightyellow2","grey70","lightyellow2",
            "lightyellow2")

minimum_list <- c(0:7,10,20)

#########################################################
pdf(paste("plots_3_",site,".pdf",sep = ""),height = 8.25,
    width = 11.67)

for (k in minimum_list) {

# Set the layout matrix to divide page into two frames one
# for the plot and one for the table
m <- rbind(c(1,1,2),
           c(1,1,2),
           c(1,1,2))
layout(m)
#layout.show(2)

# collect required statistics  
x <- data.frame(1:27)
x[,1] <- node_names
x[,2] <- round(betweenness[,i],1)
x[,3] <- round(closeness[,i],2)
x[,4] <- round(authscore[,i],1)
x[,5] <- round(hubscore[,i],1)
colnames(x) <- c("Time", "Btnss", "Clnss", "Auth", "Hub")

# plot alternating plots and tables
par(mar=c(1,1,0,0))
qgraph(datafr, node.width=0.9, asize=1.6,  
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE, cut=20, edge.label.cex=1.2, 
       directed=TRUE, weighted=TRUE, minimum=k, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour, 
       edge.labels=T)
mtext(paste("   ",
            site," 2015"),side = 1, line = -1, cex = 1)
# plot table in second frame 
frame()
# Navigate to the next viewport only needs to be done once
if (k==minimum_list[1]) {
  vps <- baseViewports()
  pushViewport(vps$inner, vps$figure, vps$plot)
}
# Plot table
grid.table(x)
}
dev.off()
###############################################################
# Transition matrix
transmat <- matrix(datafr[,3],nrow=27)
#View(transmat)
#write.csv(transmat, paste("transition_matrix_",site,
#                          ".csv",sep=""), row.names = TRUE)

# clustcoef_auto 
czhang <- clustZhang(datafr)
conela <- clustOnnela(datafr)

# Centrality
#Q <- qgraph(datafr)
C <- centrality_auto(datafr)
central <- C$node.centrality # Col# 1(Betweenness) and Col#2 (closeness)
edge_bet <- C$edge.betweenness.centrality
betweenness <- central[,1]
closeness <- central[,2]
#stat1 <-cbind(auth, hub)
#stat1 <-stat1[ order(as.numeric(row.names(stat1))),]
#auth <- stat1[,1]
#hub <- stat1[,2]
stats <- cbind(betweenness,closeness)
#write.csv(stats, paste("stats_",site,sep = "",".csv"))
}
colnames(auth_score) <- colnms
colnames(hub_score) <- colnms
write.csv(auth_score, "authority_scores.csv", row.names = FALSE)
write.csv(hub_score, "hub_scores.csv", row.names = FALSE)
############################
# occupancy matrix
############################
occupancy <- NULL
for (i in 1:12) {
  A <- unname(data[2:(length(data[,1])-1),i])
  a <- tabulate(as.numeric(A), nbins = 27)
  occupancy <- cbind(occupancy, a)
}
colnms <- NULL
for (i in 1:12) {
  colnms <- c(colnms, data[1,i])
}
colnames(occupancy) <- colnms
write.csv(occupancy, "occupancy_matrix.csv",row.names = FALSE)
