rm(month, rainfall.gympie, rainfall.tewantin, temperature.max,
   temperature.min, temperature.max.tewantin, temperature.min.tewantin)
setwd("C:\\Work\\Mangalam_data\\")
data <- read.csv("Minute_cluster mapping - all.csv", header = TRUE)
#View(data)
data <- t(data)
library(igraph)
require(igraph)
####################################
# Set up layout with first data set
###################################
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

#source("map.R")
#par(mar=c(0,0,0,0))
#plot(g, layout=layout, vertex.size=map(hub, c(1,4)), 
#     edge.arrow.size=.22)

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

#install.packages('qgraph')
library(qgraph)
require(qgraph)

pdf(paste("plots_star_",site,".pdf",sep = ""))

par(mar=c(20,20,20,20))
nodeNames <- c("day","day","day","day","day","day","night",
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
qgraph(datafr, node.width=0.6, asize=1.6,  
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE, cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE, minimum=0, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour, 
       edge.labels=T,legend=T,
       nodeNames=nodeNames)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6,  
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE,minimum=1, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color=colour,"black", maximum=800, color=colour,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6, 
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE, minimum=2, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6,  
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE, minimum=3, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6, 
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE, minimum=4, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6,  
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE, minimum=5, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6, 
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE, minimum=6, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colours,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6,  
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE, minimum=7, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6, 
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE,minimum=10, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
qgraph(datafr, node.width=0.6,asize=1.6,  
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE,cut=20, edge.label.cex=0.8, 
       directed=TRUE, weighted=TRUE,minimum=20, 
       layout=layout, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour,
       edge.labels=T)
mtext(paste("                                                                                                                                                    ",
            site),side = 1, line = 18, cex = 0.8)
dev.off()

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