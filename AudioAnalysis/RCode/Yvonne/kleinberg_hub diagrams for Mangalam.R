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
#layout_qgraph <- layout.fruchterman.reingold(g)
#layout_qgraph <- layout.circle(g)
#layout_qgraph <- layout.drl(g)
layout_qgraph <- layout.star(g,21)

# set up empty empty matrices for authority and hub scores
auth_score <- matrix(data = NA, nrow=27, ncol=12)
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

# These four lines convert the numbers to letters for the nodes
c <- rep(c(LETTERS,letters[1]), each=27)
d <- rep(c(LETTERS,letters[1]), 27)
datafr1 <- datafr
datafr1[1:729,1] <- c
datafr1[1:729,2] <- d

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
occupancy <- read.csv("occupancy_matrix.csv", header=T)

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

minimum_list <- c(0:7,9,14,19)

#########################################################
pdf(paste("plots_4_",site,".pdf",sep = ""), height = 8.25,
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
y <- c(LETTERS, letters[1])
row.names(x) <- y
x[,1] <- node_names
x[,2] <- occupancy[,i]
x[,3] <- round(betweenness[,i],1)
x[,4] <- round(closeness[,i],2)
x[,5] <- round(authscore[,i],1)
x[,6] <- round(hubscore[,i],1)

colnames(x) <- c("Time","Occ", "Btnss", "Clnss", "Auth", "Hub")

# plot alternating plots and tables
par(mar=c(1,1,0,0))
qgraph(datafr1, node.width=0.9, asize=1.6,  
       bidirectional=FALSE, nNodes=27, #threshold=10, 
       normalize=TRUE, cut=20, edge.label.cex=1.2, 
       directed=TRUE, weighted=TRUE, minimum=k, 
       layout=layout_qgraph, curve=1.7, fade=FALSE,
       edge.color="black", maximum=800, color=colour, 
       edge.labels=T)
mtext(paste("   ",site),side = 1, line = -1, cex = 1)
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

############################
# dotMatrix plots using seqinr
############################
library(seqinr)
# rearrange columns to align sites
data <- data[,c(1,5,2,6,3,7,4,8,9,10,11,12)]

library(graphics)
site <- NULL
levenshtein_distance <- NULL
smith_waterman <- NULL
library(e1071)
hamming_distance <- NULL
library(Biostrings)

# dotMatrix plots 
#pdf("dot_matrix_plots_gympie.pdf")
for (j in c(1,3,5,7,9,11)) {    #(j in c(1,2,3,4,5)) {
# Use next line for individual plots in conjunct. with line 384
# pdf(paste(data[1,j],data[1,(j+1)],".pdf")) 
a <- as.numeric(unname(data[2:length(data[,1]),j]))
b <- as.numeric(unname(data[2:length(data[,1]),(j+1)]))
a <- c(a, b)
png(paste("same_2day_dot_matrix_plot", data[1,j],".png"), 
    width=3500, height=3500, units="px")

for (i in 1:length(a)) {
  if(a[i]==1) {
    a[i] <- "A"  
  }
  if(a[i]==2) {
    a[i] <- "B"  
  }
  if(a[i]==3) {
    a[i] <- "C"  
  }
  if(a[i]==4) {
    a[i] <- "D"  
  }
  if(a[i]==5) {
    a[i] <- "E"  
  }
  if(a[i]==6) {
    a[i] <- "F"  
  }
  if(a[i]==7) {
    a[i] <- "G"  
  }
  if(a[i]==8) {
    a[i] <- "H"  
  }
  if(a[i]==9) {
    a[i] <- "I"  
  }
  if(a[i]==10) {
    a[i] <- "J"  
  }
  if(a[i]==11) {
    a[i] <- "K"  
  }
  if(a[i]==12) {
    a[i] <- "L"  
  }
  if(a[i]==13) {
    a[i] <- "M"  
  }
  if(a[i]==14) {
    a[i] <- "N"  
  }
  if(a[i]==15) {
    a[i] <- "O"  
  }
  if(a[i]==16) {
    a[i] <- "P"  
  }
  if(a[i]==17) {
    a[i] <- "Q"  
  }
  if(a[i]==18) {
    a[i] <- "R"  
  }
  if(a[i]==19) {
    a[i] <- "S"  
  }
  if(a[i]==20) {
    a[i] <- "T"  
  }
  if(a[i]==21) {
    a[i] <- "U"  
  }
  if(a[i]==22) {
    a[i] <- "V"  
  }
  if(a[i]==23) {
    a[i] <- "W"  
  }
  if(a[i]==24) {
    a[i] <- "X"  
  }
  if(a[i]==25) {
    a[i] <- "Y"  
  }
  if(a[i]==26) {
    a[i] <- "Z"  
  }
  if(a[i]==27) {
    a[i] <- "a"  
  }
  if(a[i]==28) {
    a[i] <- "b"  
  }
  if(a[i]==29) {
    a[i] <- "c"  
  }
  if(a[i]==30) {
    a[i] <- "d"  
  }
}

for (i in 1:length(b)) {
  if(b[i]==1) {
    b[i] <- "A"  
  }
  if(b[i]==2) {
    b[i] <- "B"  
  }
  if(b[i]==3) {
    b[i] <- "C"  
  }
  if(b[i]==4) {
    b[i] <- "D"  
  }
  if(b[i]==5) {
    b[i] <- "E"  
  }
  if(b[i]==6) {
    b[i] <- "F"  
  }
  if(b[i]==7) {
    b[i] <- "G"  
  }
  if(b[i]==8) {
    b[i] <- "H"  
  }
  if(b[i]==9) {
    b[i] <- "I"  
  }
  if(b[i]==10) {
    b[i] <- "J"  
  }
  if(b[i]==11) {
    b[i] <- "K"  
  }
  if(b[i]==12) {
    b[i] <- "L"  
  }
  if(b[i]==13) {
    b[i] <- "M"  
  }
  if(b[i]==14) {
    b[i] <- "N"  
  }
  if(b[i]==15) {
    b[i] <- "O"  
  }
  if(b[i]==16) {
    b[i] <- "P"  
  }
  if(b[i]==17) {
    b[i] <- "Q"  
  }
  if(b[i]==18) {
    b[i] <- "R"  
  }
  if(b[i]==19) {
    b[i] <- "S"  
  }
  if(b[i]==20) {
    b[i] <- "T"  
  }
  if(b[i]==21) {
    b[i] <- "U"  
  }
  if(b[i]==22) {
    b[i] <- "V"  
  }
  if(b[i]==23) {
    b[i] <- "W"  
  }
  if(b[i]==24) {
    b[i] <- "X"  
  }
  if(b[i]==25) {
    b[i] <- "Y"  
  }
  if(b[i]==26) {
    b[i] <- "Z"  
  }
  if(b[i]==27) {
    b[i] <- "a"  
  }
  if(b[i]==28) {
    b[i] <- "b"  
  }
  if(b[i]==29) {
    b[i] <- "c"  
  }
  if(b[i]==30) {
    b[i] <- "d"  
  }
}
par(cex=2, mar=c(2.2,2.2,2.2,2.2), mgp=c(1.2,0.2,0), tck=-0.01)
dotPlot(a, a, wsize = 2,
        xlab = paste(data[1,j],data[1,(j+1)],
                     sep = "   "), xaxt="n",
        ylab = paste(data[1,j],data[1,(j+1)],
                     sep = "   "), yaxt="n",
        col = c("white",c(colors(1))),cex=4)
at <- seq(0, 2870, by = 120)
axis(1, line = 0, at = at, 
     labels = c("0","2","4","6","8","10","12","14","16","18","20",
                "22","24","2","4","6","8","10","12","14","16",
                "18","20","22"), cex.axis=1.2)
axis(2, line = 0, at = at, 
     labels = c("0","2","4","6","8","10","12","14","16","18","20",
                "22", "24","2","4","6","8","10","12","14","16",
                "18","20","22"), cex.axis=1.2)
abline (v=1435, h=1435, lty=2)
dev.off()
#a <- paste(a, collapse = "")
#b <- paste(b, collapse = "")
#s1 <- BString(a)
#s2 <- BString(b)
# Smith-Waterman
#sw <- pairwiseAlignment(s1, s2, type = "local")
#smith_waterman <- c(smith_waterman, sw@score)
#lev <-stringDist(c(a,b))
#levenshtein_distance <- c(levenshtein_distance, lev)
#s <- data[1,j]
#site <- c(site, s)
#ham.dist <- hamming.distance(data[,j],data[,(j+1)])
#hamming_distance <- c(hamming_distance, ham.dist)
}
dev.off()
smith_waterman <- round(smith_waterman, 2)
distances <- cbind(site, smith_waterman,
                   hamming_distance, 
                   levenshtein_distance)
distances
#write.csv(distances, "distances.csv", row.names = FALSE)