library(gplots)

# create some data
d <- matrix(rnorm(120),12,10)

# cluster it
hr <- hclust(as.dist(1-cor(t(d), method="pearson")), method="complete")

# define some clusters
mycl <- cutree(hr, h=max(hr$height/1.5))

# get a color palette equal to the number of clusters
clusterCols <- rainbow(length(unique(mycl)))

# create vector of colors for side bar
myClusterSideBar <- clusterCols[mycl]

# choose a color palette for the heat map
myheatcol <- rev(redgreen(75))

# draw the heat map
heatmap.2(d, main="Hierarchical Cluster", Rowv=as.dendrogram(hr), Colv=NA, dendrogram="row", scale="row", col=myheatcol, density.info="none", trace="none", RowSideColors= myClusterSideBar)

# cutree returns a vector of cluster membership
# in the order of the original data rows
# examine it
mycl

# examine the cluster membership by it's order
# in the heatmap
mycl[hr$order]

# grab a cluster
cluster1 <- d[mycl == 1,]

# or simply add the cluster ID to your data
foo <- cbind(d, clusterID=mycl)

# examine the data with cluster ids attached, and ordered like the heat map
foo[hr$order,]

I think the correct code would be: 
  
#1)reorder your d table according to the heatmap 
#d=(d)[row.hc$order,]
#2)grab clusters
#cluster1 <- d[mycl == 1,]