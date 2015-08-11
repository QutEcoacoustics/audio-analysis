# to make a gml file
#
#my.data <- read.csv("Transition_Matrix_GympieNP Week 1.csv", header = T)
#my.graph <- graph.data.frame(d = my.data, directed = TRUE)
#write.graph(graph = my.graph, file = 'GympieNP Week1.gml', format = 'gml')

library(devtools)
library(arcdiagram)
library(igraph)

setwd <- setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\")
#mis_file <- "lesmiserables.txt"
#mis_file <- "arcdiagram.txt"
#mis_file <- "GympieNP Week1.gml"
mis_file <- "GympieNP Week1 practice practice practice.txt"
mis_graph <- read.graph(mis_file, format = "gml")

#edgelist <- NULL
edgelist <- get.edgelist(mis_graph)
#vlabels <- get.vertex.attribute(mis_graph, "label")
vlabels <- get.vertex.attribute(mis_graph, "name")
#vlabels <- get.vertex.attribute(mis_graph, "name")
#vgroups <- get.vertex.attribute(mis_graph, "group")
vgroups <- vertex_attr(mis_graph, "group")
vfill <- get.vertex.attribute(mis_graph, "fill")
degrees <- degree(mis_graph)
values <- get.edge.attribute(mis_graph, "value")
vborders <- get.vertex.attribute(mis_graph, "border")
vref <- (as.numeric(get.vertex.attribute(mis_graph, "ref"))/20)

#library(reshape)
#library(dplyr) # required for arrange function
#x <- data.frame(vgroups, degrees, vlabels, ind = 1:vcount(mis_graph))
#y <- arrange(x, desc(vgroups), desc(degrees))
#new_ord <- y$ind
#dev.off()

new_order <- c("0","1","2","3","4","5","6") #,"7","8","9","10",
#               "11","12","13","14","15","16","17","18","19",
#               "20","21","22","23","24","25","26","27","28",
#               "29","30")
labs <- new_order
transparentColours <- c("#FF000080","#0000FF80","#0000FF80",
                        "#00FF0080","#FFA50080","#FFA50080",
                        "#FFA50080","#FFFF0080","#19197080",
                        "#19197080","violet")

colours <- c ("red", "blue", "blue", "green", "orange", "yellow",
              "midnightblue", "violet")

a <- unname(node_coords(edgelist))
nodeList <- attr(a,"names") # which appear in a different order, this affects
                # the order of the nodeColours
nodeColours <- unique(colours)
nodeColours <- c("red","blue","orange","yellow","green","midnightblue","violet")
arcplot(edgelist, ordering = new_order, cex.labels = 1, 
        show.nodes = TRUE, cex.nodes = vref[1:29], 
        pch.nodes = 19, lwd.nodes = 2, line = -0.5, 
        col.arcs = transparentColours, 
        lwd.arcs = 0.5 * values, horizontal = T, 
        col.nodes = nodeColours)

#matrix <- cbind(vlabels, vgroups, vfill, degrees, vborders)