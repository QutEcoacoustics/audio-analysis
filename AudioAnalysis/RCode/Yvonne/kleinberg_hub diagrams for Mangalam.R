#################################################
# This code plots network diagrams and calculates centrality measures
# from cluster lists
# 8 April 2016
#################################################
setwd("C:\\Work\\Mangalam_data\\")
data <- read.csv("Minute_cluster mapping - all.csv", header = TRUE)
# transpose data table
data <- t(data)

# rearrange columns to align sites
data <- data[,c(1,5,2,6,3,7,4,8,9,10,11,12)]
ref <- unlist(which(data[27,]=="3"))
for(c in 1:length(ref)) {
  data[27, ref] <- " 3"
}
ref <- unlist(which(data[27,]=="9"))
for(c in 1:length(ref)) {
  data[27,ref] <- " 9"
}
ref <- unlist(which(data[27,]=="4"))
for(c in 1:length(ref)) {
  data[27,ref] <- " 4"
}

#View(data)
############################
# occupancy matrix
############################
summary <- matrix(data = NA, nrow=12, ncol=27)
for (i in 1:12) {
  t <- table(data[2:length(data[,1]),i])
  for(j in 1:length(names(t))) {
    k <- as.numeric(names(t[j]))
   summary[i,k] <- as.vector(t[j])
  }
}

summary <- t(summary)
clusters <- c(LETTERS, letters[1])
summary <- cbind(clusters, summary)

colnames(summary) <- c("cluster",data[1,1:12])
write.csv(summary, "summary.csv", row.names = FALSE)
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
i=2
site <- data[1,i]
site
# Caution must be taken here to choose a day that has a complete
# list of clusters ie. Day 2 because this sets the layout for the
# number of points in the network graph and this must equal the
# maximum number of clusters
A <- unname(data[2:(length(data[,1])-1),i])
B <- unname(data[3:length(data[,1]), i])
g1 <- data.frame(A=A,B=B)

g <- graph_from_data_frame(g1, directed=TRUE, vertices = NULL)
#layout_qgraph <- layout.fruchterman.reingold(g)
#layout_qgraph <- layout.circle(g)
#layout_qgraph <- layout.drl(g)
layout_qgraph <- layout.star(g, 21)

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
  pdf(paste("plots_5_",site,".pdf",sep = ""), height = 8.25,
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
    x[,2] <- summary[,(i+1)]
    x[,3] <- round(betweenness[,i],1)
    x[,4] <- round(closeness[,i],2)
    x[,5] <- round(authscore[,i],1)
    x[,6] <- round(hubscore[,i],1)
    colnames(x) <- c("Time","Occ","Btnss","Clnss","Auth","Hub")
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
# dotMatrix plots using seqinr
############################
library(seqinr)

library(graphics)
site <- NULL
levenshtein_distance <- NULL
smith_waterman <- NULL
library(e1071)
hamming_distance <- NULL
library(Biostrings)
library(utils)
z <-drop(attr(adist("kitten", "sitting", counts = TRUE), "counts"))
z[[1]]
z[[2]]
z[[3]]

data(yeastSEQCHR1)
yeast1 <- DNAString(yeastSEQCHR1)
PpiI <- "GAACNNNNNCTC"  # a restriction enzyme pattern
matchPattern {Biostrings}
x <- DNAString("AAGCGCGATATG")
m1 <- matchPattern("GCNNNAT", x)
m1
m1@ranges@start
m1@ranges@width
m1@subject

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
                "18","20","22"), cex.axis=1.2, las=T)
axis(2, line = 0, at = at, 
     labels = c("0","2","4","6","8","10","12","14","16","18","20",
                "22", "24","2","4","6","8","10","12","14","16",
                "18","20","22"), cex.axis=1.2, las=T)
abline (v=1440, h=1440, lty=2, lwd=2)
abline (v=seq(1,2880,20), h=seq(1,2800,20), lty=2)

dev.off()
}
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

dev.off()
smith_waterman <- round(smith_waterman, 2)
distances <- cbind(site, smith_waterman,
                   hamming_distance, 
                   levenshtein_distance)
distances
#write.csv(distances, "distances.csv", row.names = FALSE)

##############################
# Gympie data
##############################
gym_clust <- read.csv("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\hybrid_clust_knn_17500_3.csv", header=T)
gym_clust_30 <- gym_clust[,6]
gym_clust_60 <- gym_clust[,12]
# transform data into consecutive days
g_c_30 <- data.frame(matrix(NA, nrow = 1441, ncol = (length(gym_clust_30)/1440)))
g_c_60 <- data.frame(matrix(NA, nrow = 1441, ncol = (length(gym_clust_60)/1440)))

column.names <- NULL
for (i in 1:((length(g_c_30)/2))) {
  col.names <- paste("day_", i, sep = "")
  column.names <- c(column.names,col.names)
}

g_c_30[1,] <- column.names
g_c_30[2:1441,] <- data.frame(matrix(unlist(gym_clust_30), nrow=1440, byrow=FALSE),stringsAsFactors=FALSE) 

g_c_60[1,] <- column.names
g_c_60[2:1441,] <- data.frame(matrix(unlist(gym_clust_60), nrow=1440, byrow=FALSE),stringsAsFactors=FALSE) 
#g_c_30 <- g_c_60
setwd("C:\\Work\\CSV files\\FourMonths_repeat\\Dot_matrix_plot\\")

############################
# generate a sequence of dates
############################
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20151010", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
dates1 <- date.list
date1_gym <- date.list
date1_woon <- date.list
for (i in 1:length(dates1)) {
  date1_gym[i] <- paste("gym", dates1[i],sep = "_")
  date1_woon[i] <- paste("woon", dates1[i], sep = "-")
}
date1 <- c(date1_gym, date1_woon)

for (i in 1:length(dates)) {
dates[i] <- paste(substr(dates[i],7,8), substr(dates[i],5,6),
              substr(dates[i],1,4), sep = "-")
}
date <- dates
date <- rep(date, 2)
g_c_30[1,] <- date1
g_c_60[1,] <- date1
############################
# dotMatrix plots using seqinr
############################
library(seqinr)

library(graphics)
site <- NULL
levenshtein_distance <- NULL
smith_waterman <- NULL
library(e1071)
hamming_distance <- NULL
library(Biostrings)

# dotMatrix plots 
#pdf("dot_matrix_plots_gympie.pdf")
for (j in c(198:198)) {    
  # Use next line for individual plots in conjunct. with line 384
  # pdf(paste(data[1,j],data[1,(j+1)],".pdf")) 
  a <- as.numeric(g_c_30[2:length(g_c_30[,1]),j])
  b <- as.numeric(g_c_30[2:length(g_c_30[,1]),(j+1)])
  a <- c(a, b)
  png(paste("same_day_30_dot_matrix_plot", date1[j],".png"), 
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
    if(a[i]==31) {
      a[i] <- "e"  
    }
    if(a[i]==32) {
      a[i] <- "f"  
    }
    if(a[i]==33) {
      a[i] <- "g"  
    }
    if(a[i]==34) {
      a[i] <- "h"  
    }
    if(a[i]==35) {
      a[i] <- "i"  
    }
    if(a[i]==36) {
      a[i] <- "j"  
    }
    if(a[i]==37) {
      a[i] <- "k"  
    }
    if(a[i]==38) {
      a[i] <- "l"  
    }
    if(a[i]==39) {
      a[i] <- "m"  
    }
    if(a[i]==40) {
      a[i] <- "n"  
    }
    if(a[i]==41) {
      a[i] <- "o"  
    }
    if(a[i]==42) {
      a[i] <- "p"  
    }
    if(a[i]==43) {
      a[i] <- "q"  
    }
    if(a[i]==44) {
      a[i] <- "r"  
    }
    if(a[i]==45) {
      a[i] <- "s"  
    }
    if(a[i]==46) {
      a[i] <- "t"  
    }
    if(a[i]==47) {
      a[i] <- "u"  
    }
    if(a[i]==48) {
      a[i] <- "v"  
    }
    if(a[i]==49) {
      a[i] <- "w"  
    }
    if(a[i]==50) {
      a[i] <- "x"  
    }
    if(a[i]==51) {
      a[i] <- "y"  
    }
    if(a[i]==52) {
      a[i] <- "z"  
    }
    if(a[i]==53) {
      a[i] <- ""  
    }
    if(a[i]==54) {
      a[i] <- "1"  
    }
    if(a[i]==55) {
      a[i] <- "2"   
    }
    if(a[i]==56) {
      a[i] <- "3"  
    }
    if(a[i]==57) {
      a[i] <- "4"
    }
    if(a[i]==58) {
      a[i] <- "5"
    }
    if(a[i]==59) {
      a[i] <- "6"
    }
    if(a[i]==60) {
      a[i] <- "7"
    }
    if(a[i]==61) {
      a[i] <- "8"
    }
  }
  
  par(cex=3.5, mar=c(2.2,2.2,2.2,2.2), mgp=c(1.2,0.2,0), tck=-0.01)
  dotPlot(a, a, wsize = 1, 
          xlab = paste(date[j], date[(j+1)],
                       sep = "                                     "), xaxt="n",
          ylab = paste(date[j], date[(j+1)],
                       sep = "                                     "), yaxt="n",
          col = c("white",c(colors(1))), cex=24)
  at <- seq(0, 2880, by = 120)
  axis(1, line = 0, at = at, 
       labels = c("0","2","4","6","8","10","12","14","16","18","20",
                  "22","24","2","4","6","8","10","12","14","16",
                  "18","20","22","24"), cex.axis=1.2, las=T)
  axis(2, line = 0, at = at, 
       labels = c("0","2","4","6","8","10","12","14","16","18","20",
                  "22", "24","2","4","6","8","10","12","14","16",
                  "18","20","22","24"), cex.axis=1.2, las=T)
  abline (v=1440, h=1440, lty=2, lwd=2)
  abline (v=seq(0,2880,20), h=seq(0,2880,20), lty=2)
  dev.off()
}

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
dev.off()
smith_waterman <- round(smith_waterman, 2)
distances <- cbind(site, smith_waterman,
                   hamming_distance, 
                   levenshtein_distance)
distances
#write.csv(distances, "distances.csv", row.names = FALSE)

a <- gym_clust_30
# This will take a minute 
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
  if(a[i]==31) {
    a[i] <- "e"  
  }
  if(a[i]==32) {
    a[i] <- "f"  
  }
  if(a[i]==33) {
    a[i] <- "g"  
  }
  if(a[i]==34) {
    a[i] <- "h"  
  }
  if(a[i]==35) {
    a[i] <- "i"  
  }
  if(a[i]==36) {
    a[i] <- "j"  
  }
  if(a[i]==37) {
    a[i] <- "k"  
  }
  if(a[i]==38) {
    a[i] <- "l"  
  }
  if(a[i]==39) {
    a[i] <- "m"  
  }
  if(a[i]==40) {
    a[i] <- "n"  
  }
  if(a[i]==41) {
    a[i] <- "o"  
  }
  if(a[i]==42) {
    a[i] <- "p"  
  }
  if(a[i]==43) {
    a[i] <- "q"  
  }
  if(a[i]==44) {
    a[i] <- "r"  
  }
  if(a[i]==45) {
    a[i] <- "s"  
  }
  if(a[i]==46) {
    a[i] <- "t"  
  }
  if(a[i]==47) {
    a[i] <- "u"  
  }
  if(a[i]==48) {
    a[i] <- "v"  
  }
  if(a[i]==49) {
    a[i] <- "w"  
  }
  if(a[i]==50) {
    a[i] <- "x"  
  }
  if(a[i]==51) {
    a[i] <- "y"  
  }
  if(a[i]==52) {
    a[i] <- "z"  
  }
  if(a[i]==53) {
    a[i] <- ""  
  }
  if(a[i]==54) {
    a[i] <- "1"  
  }
  if(a[i]==55) {
    a[i] <- "2"   
  }
  if(a[i]==56) {
    a[i] <- "3"  
  }
  if(a[i]==57) {
    a[i] <- "4"
  }
  if(a[i]==58) {
    a[i] <- "5"
  }
  if(a[i]==59) {
    a[i] <- "6"
  }
  if(a[i]==60) {
    a[i] <- "7"
  }
  if(a[i]==61) {
    a[i] <- "8"
  }
}

n <- 5 # time period is 2n (5)
d <- 10 # number of minutes between comparison (10) if d >= 2n there is no overlap
#hamming_distance <- NULL
distance <- NULL
for (i in (1+n):(length(a)-n)) {
  s1 <- (a[(i-n):(i+n)])
  s2 <- (a[(i-n+d):(i+n+d)])
  b <- as.character(pmatch(s1,s2)) # could also use which(s1 %in% s2)/length(s1)
  dist <- length(which(is.na(b))) 
  distance <- c(distance, dist)
  #length(which(is.na(a)))
  #ham.dist <- hamming.distance(a[(i-n):(i+n)], a[(i-n+d):(i+n+d)])  
  #hamming_distance <- c(hamming_distance, ham.dist)
}
#hamming_distance1 <- c(rep(0, n),hamming_distance)
distance1 <- c(rep(10, n), distance)
# detect runs
run <- rle(distance1)
ref <- cumsum(run$lengths)
run1 <- cbind(run$lengths, run$values, ref)
runsub <- subset(run1, c(run1[,2]==0))
runsub[,1] <- runsub[,1]+10
run_data <- data.frame[1:length(gym_clust_30)]
run_data <- matrix(data = 0, nrow=length(gym_clust_30), ncol=2)
run_data[,2] <- 1:length(gym_clust_30)
reference <- runsub[,3]
reference1 <- runsub[,1]
for (i in 1:length(reference)) {
  run_data[reference[i]:(reference[i]+reference1[i]-1),1] <- 1
}
  
plot(distance1[3000:3600], type="l",
     main=paste("n =",n,"d =",d, sep = " "))
plot(distance1[2881:4320], type="l",
     main=paste("n =",n,"d =",d, sep = " "))
plot(distance1[4321:5760], type="l",
     main=paste("n =",n,"d =",d, sep = " "))
plot(distance1[5761:7200], type="l",
     main=paste("n =",n,"d =",d, sep = " "))
plot(distance1[7201:8540], type="l",
     main=paste("n =",n,"d =",d, sep = " "))
date[7200/1440]
plot(distance1[8541:10080], type="l",
     main=paste("n =",n,"d =",d, sep = " "))
plot(distance1[10080:11520], type="l",
     main=paste("n =",n,"d =",d, sep = " "))

#################################
# ten minute
#################################
n <- 2 # period in minutes
unique_clus <- NULL
unit.numbers <- matrix(data = NA, nrow=length(g_c_30$X1), 
                       ncol=length(g_c_30))
unique_clusters <- matrix(data = NA, nrow=length(g_c_30$X1), 
                          ncol=length(g_c_30))
for (i in 1:length(g_c_30)) {
  for (j in 2:(length(g_c_30$X1)-n)) {
    uniq <- paste(sort(as.numeric(unique(g_c_30[j:(j+(n-1)),i]))),collapse = "_")
    unique_clusters[j,i] <- uniq
    unit.num <- length(unique(g_c_30[j:(j+(n-1)),i]))
    unit.numbers[j,i] <- unit.num
  }
}
unique_clusters[1,] <- date1
unit.numbers[1,] <- date1
write.csv(unique_clusters[,1:111], paste("gym_unique_clusters_",
          n,".csv"), row.names = FALSE)
write.csv(unique_clusters[,1:111], paste("woon_unique_clusters_",
          n, ".csv"),row.names = FALSE)
write.csv(unit.numbers[,1:111], paste("gympie_",n,"min.csv"), 
          row.names = FALSE)
write.csv(unit.numbers[,112:222], paste("woondum_", n,"min.csv"), 
          row.names = FALSE)
write.csv(g_c_30, "gympie_30clusters.csv", row.names = FALSE)

####################################
# coloured dot matrix plots
###################################
setwd("C:\\Work\\CSV files\\FourMonths_repeat\\Dot_matrix_plot\\")
data <- g_c_30[2:1441,]

library(raster)
for(day.ref in 1:1) {
data <- g_c_30[2:1441,day.ref]
r <- raster(xmn = 0, xmx = 1440, ymn = 0, ymx =1440, 
            nrows =1440, ncols = 1440, crs=NA)
t <- rep(0,(1440*1440))
for (i in 1:1440) { 
  for (j in 1:1440) {  
    if(data[i]==data[j]) { 
      t[((i-1)*1440)+j] <- data[i]
    }
  }
}

for (i in 1:1440) { 
  for (j in 1:1440) { 
    if(data[i]==data[j]) { 
      t[abs(((i-1)*1440+j)-1439*1440)+2*j] <- data[i]
    }
  }
}
#t <- t[length(t):1]
t <- as.numeric(t)
r[] <- t
plot(r, xaxt="n", yaxt="n", axes=FALSE, 
     box=FALSE, legend=FALSE, frame.plot=F,
     useRaster=F, main=date1[day.ref],pty = "s")
}
dev.off()
library(grid)
redGradient <- matrix(hcl(0, 80, seq(50, 80, 10)),
                      nrow=4, ncol=5)
t <- matrix(data = 0.95, nrow=27, ncol=12)

grid.raster(t)

grid.raster in grid package may be the answer

###################################
# colour dot matrix plots
##################################
#library(raster)
library(fields)
setwd("C:\\Work\\CSV files\\FourMonths_repeat\\Dot_matrix_plot\\")
xmin <- 0
xmax <- 1440
ymin <- 0
ymax <- 1440
asratio = (ymax-ymin)/(xmax-xmin)
cols <- c(
  '0' = "#F2F2F2FF",
  '1' = "#00B917",
  '2' = "#788231",
  '3' = "#FF0000",
  '4' = "#01FFFE",
  '5' = "#FE8900",
  '6' = "#006401",
  '7' = "#FFDB66",
  '8' = "#010067",
  '9' = "#95003A",
  '10' = "#007DB5",
  '11' = "#BE9970",
  '12' = "#774D00",
  '13' = "#90FB92",
  '14' = "#0076FF",
  '15' = "#FF937E",
  '16' = "#6A826C",
  '17' = "#FF029D",
  '18' = "#0000FF",
  '19' = "#7A4782",
  '20' = "#7E2DD2",
  '21' = "#0E4CA1",
  '22' = "#FFA6FE",
  '23' = "#A42400",
  '24' = "#00AE7E",
  '25' = "#BB8800",
  '26' = "#BE9970",
  '27' = "#263400",
  '28' = "#C28C9F",
  '29' = "#FF74A3",
  '30' = "#01D0FF",
  "31" = "#6B6882",
  '32' = "#E56FFE",
  '33' = "#85A900",
  '34' = "#968AE8",
  '35' = "#43002C",
  '36' = "#DEFF74",
  '37' = "#00FFC6",
  '38' = "#FFE502",
  '39' = "#620E00",
  '40' = "#008F9C",
  '41' = "#98FF52",
  '42' = "#7544B1",
  '43' = "#B500FF",
  '44' = "#00FF78",
  '45' = "#FF6E41",
  '46' = "#005F39",
  '47' = "#004754",
  '48' = "#5FAD4E",
  '49' = "#A75740",
  '50' = "#A5FFD2",
  '51' = "#FFB167",
  '52' = "#009BFF")

#,,,,
#,,,,
#,,,,"#91D0CB"
#,,,,
#,,,,
#,,,,
#,,
for(day.ref in 1:222) {
  data <- g_c_30[2:1441,day.ref]
  #r <- raster(xmn = 0, xmx = 1440, ymn = 0, ymx =1440, 
  #            nrows =1440, ncols = 1440, crs=NA)
  t <- rep(0,(1440*1440))
  for (i in 1:1440) { 
    for (j in 1:1440) {  
      if(data[i]==data[j]) { 
        t[((i-1)*1440)+j] <- data[i]
      }
    }
  }
  
#  for (i in 1:1440) { 
#    for (j in 1:1440) { 
#      if(data[i]==data[j]) { 
#        t[abs(((i-1)*1440+j)-1439*1440)+2*j] <- data[i]
#      }
#    }
#  }
  #t <- t[length(t):1]
  library(graphics)
  t <- as.numeric(t)
  t1 <- matrix(t, nrow = 1440, byrow = T)
  #r[] <- t
  png(paste("colour_dot_plot", date1[day.ref],".png"),
      width=1658, height=ceiling(1658*asratio), 
      units = "px")
  #colours <- c("#F2F2F2FF","#00FF00","#001544","#FF0000",
  #             "#01FFFE","#FE8900","#006401","#FFDB66",
  #             "#010067","#95003A","#007DB5","#9E008E",
  #             "#774D00","#90FB92","#0076FF","#FF937E",
  #             "#6A826C","#FF029D","#0000FF","#7A4782",
  #             "#7E2DD2","#85A900","#FF0056","#A42400",
  #             "#00AE7E","#683D3B", "#004754","#263400",
  #             "#C28C9F","#00B917","#FFA6FE")
               
  par(cex=1, oma=c(3,3,3,3))
  score <- matrix(data = t, nrow=1440, ncol=1440)
  score1 <- matrix(data = t, nrow=1440, ncol=1440, byrow = T)
  image.plot(1:nrow(score1), 1:ncol(score1), 
        score1, col=cols[1:31], axes=FALSE,
        ann=F, xaxt="n",legend.shrink = 0.5,
        yaxt="n", asp=1)
  #legend(1445, 980, legend = 1:30, fill = cols[2:31], bty = "n")
  title(paste(date1[day.ref]), cex.main=3)
  #plot basemap
  
  #plot(r,xlim=c(xmin,xmax), 
  #     ylim=c(ymin,ymax), asp=1, xaxs="i", 
  #     yaxs="i", box=FALSE,
  #     main=paste(date1[day.ref],date[day.ref], sep = "  "), 
  #     xaxt="n", yaxt="n", frame.plot=F, useRaster=F, 
  #     las=T, axes=FALSE, cex.main=3,
  #     col=colours)
         #c("#F2F2F2FF", "#E3A931", "#9976DF", "#57CB40",
        #     "#DC6BDF", "#AFC632", "#6080DB", "#7CCFED", 
        #     "#C73C59", "#D83C42","#51C587", "#D84195",
        #     "#68AA41", "#B151A7", "#ACA437", "#CA93D7",
        #     "#5D7725", "#E23572", "#3A8556", "#44BAC5",
        #     "#72A2DA", "#DA7F2F", "#407697", "#E67456", 
        #      "#7C6CA5", "#966E22", "#DE7AB0", "#A85739",
        #     "#945684", "#E57383", '#FFB6C1'))
  at <- seq(120, 1440, by = 120)
  axis(1, line = -5.5, at = at, #line=NA,
       labels = c("2","4","6","8","10","12","14","16","18","20",
       "22", "24"), cex.axis=2.1, outer = TRUE,
       las=T)
  at <- seq(0, 1440, by = 10)
  axis(1, line = -5.5, at = at, tick = TRUE,
       labels = FALSE, outer=TRUE)
  abline (v=seq(120,1440,120), h=seq(120,1440,120), 
          lty=2, lwd=0.1, xpd=FALSE)
  at <- seq(0, 1440, by = 120)
  axis(2, line = 0, at = at, 
       labels = c("0","2","4","6","8","10",
                  "12","14","16","18","20",
       "22", "24"), cex.axis=2.1, las=T, pos=NA)
  at <- seq(0, 1440, by = 10)
  axis(2, line = 0, at = at, tick = TRUE,
       labels = FALSE, pos=NA)
  par(usr=c(xmin,xmax,ymin,ymax))
  
  dev.off()
}
library(munsell)
plot_hex(cols) #this plots the colours
##############################################
#library(raster)
colours <- c("#F2F2F2FF","#00FF00","#001544","#FF0000",
             "#01FFFE","#FE8900","#006401","#FFDB66",
             "#010067","#95003A","#007DB5","#9E008E",
             "#774D00","#90FB92","#0076FF","#FF937E",
             "#6A826C","#FF029D","#0000FF","#7A4782",
             "#7E2DD2","#85A900","#FF0056","#A42400",
             "#00AE7E","#683D3B", "#004754","#263400")

xmin <- 0
xmax <- 1441
ymin <- 0
ymax <- 1441
asratio = (ymax-ymin)/(xmax-xmin)

data1 <- data
for(day.ref in 12:12) {
  data <- data1[2:1436, day.ref]
  r <- raster(xmn = 0, xmx = 1435, ymn = 0, ymx =1435, 
              nrows =1435, ncols = 1435, crs=NA)
  t <- rep(0,(1435*1435))
  for (i in 1:1435) { 
    for (j in 1:1435) {  
      if(data[i]==data[j]) { 
        t[((i-1)*1435)+j] <- data[i]
      }
    }
  }
  
  #  for (i in 1:1440) { 
  #    for (j in 1:1440) { 
  #      if(data[i]==data[j]) { 
  #        t[abs(((i-1)*1440+j)-1439*1440)+2*j] <- data[i]
  #      }
  #    }
  #  }
  #t <- t[length(t):1]
  t <- as.numeric(t)
  r[] <- t
  png(paste("colour4_dot_matrix_plot", data1[1,day.ref],".png"),
      width=1480, height=ceiling(1480*asratio))
  
  #plot basemap
  par(cex=1, oma=c(3,3,3,3))
  score <- matrix(data = t, nrow=1440, ncol=1440)
  score1 <- matrix(data = t, nrow=1440, ncol=1440, byrow = T)
  r <- raster(score)
  plot(r,xlim=c(xmin,xmax), ylim=c(ymin,ymax),
         asp=1,xaxs="i",yaxs="i", box=FALSE,
       main=paste(data1[1,day.ref], sep = "  "), 
       xaxt="n", yaxt="n", frame.plot=F,
       useRaster=T, las=T, axes=FALSE, cex.main=3,
       col=colours[1:28], at <- seq(120, 1440, by = 120))
  labels <- c("2","4","6","8","10","12",
              "14","16","18","20","22","24")
  axis(1, line = -129, at = at, labels = labels, 
       cex.axis=2.1, outer = TRUE, las=T)
  at <- seq(0, 1440, by = 10)
  axis(1, line = -129, at = at, tick = TRUE,
       labels = FALSE, outer=TRUE)
  abline (v=seq(0,1440,120), h=seq(0,1440,120), 
          lty=2, lwd=0.2)
  at <- seq(0, 1440, by = 120)
  labels <- c("24","22","20","18","16","14",
              "12","10","8","6","4","2", "0")
  axis(2, line = 0, at = at, labels = labels, 
       cex.axis=2.1, las=T)
  at <- seq(0, 1440, by = 10)
  axis(2, line = 0, at = at, tick = TRUE,
       labels = FALSE)
  par(usr=c(xmin,xmax,ymin,ymax))
  
  dev.off()
}

########### Model ############
#library(raster)
r <- raster(xmn = 0, xmx = 20, ymn = 0, ymx = 20, 
            nrows = 10, ncols =10, crs=NA)
r[] <- 1:100
plot(r,col=rainbow(255))
#######################
test <- c(rep(0,9), 6, rep(0,9),6,rep(0,9),6)
test <- rep(test, 30)
r <- raster(xmn = 0, xmx = 30, ymn = 0, ymx = 30, 
            nrows = 30, ncols =30)
r[] <- test
plot(r)
