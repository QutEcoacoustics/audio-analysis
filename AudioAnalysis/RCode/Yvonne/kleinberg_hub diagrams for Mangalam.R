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

# packages needed for plot
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
layout <- layout.star(g,21)

#####################################
# Complete all with the same layout
####################################
for(i in 1:12) {
site <- data[1,i]
site
A <- unname(data[2:(length(data[,1])-1),i])
B <- unname(data[3:length(data[,1]), i])
g1 <- data.frame(A=A,B=B)

g <- graph_from_data_frame(g1, directed=TRUE, vertices = NULL)

#plot(g, layout=layout, vertex.size=20, edge.arrow.size=.01)
#plot(g, layout=layout, edge.arrow.size=.08,
#     vertex.label=V(g)$name)

#source("http://michael.hahsler.net/SMU/ScientificCompR/code/map.R")
#auth <- authority_score(g)$vector
#hub <- hub.score(g)$vector

df <- NULL
datafr <- NULL

A <- as.numeric(A)
B <- as.numeric(B)
for (j in 1:max(A)) {
  for(k in 1:max(B)) {
    a <- which(as.integer(A)==j & as.integer(B)==k) 
    df <- c(j, k, length(a))
    datafr <- rbind(datafr, df)
  }
}

betweenness <- read.csv("betweenness_across_sites.csv", header = T) 
closeness <- read.csv("closeness_across_sites.csv", header = T)

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
pdf(paste("plots_star2_",site,".pdf",sep = ""),height = 8.25,
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
x[,3] <- round(closeness[,i],3)
colnames(x) <- c("Time of day", "Betweenness", "Closeness")

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