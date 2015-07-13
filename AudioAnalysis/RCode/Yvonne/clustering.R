setwd("C:\\Work\\CSV files\\2015Jul01-120417\\GympieNP\\")
#setwd("C:\\Work\\CSV files\\2015Jul01-120417\\Woondum3\\")

indices <- read.csv("Towsey_Summary_Indices 20150622_000000 to 20150628_064559 .csv",header = T)
#indices <- read.csv("Towsey_Summary_Indices 20150622_000000 to 20150628_133139 .csv", header = T)

model <- m.km <- kmeans(indices[c(5,6,7,9,10,12,13,14,17)], 18)
model$size
model$centers
model$iter
model$ifault
dsc <- scale(indices[,c(5,6,7,9,10,12,13,14,17)])
attr(dsc, "scaled:center") # the mean of each variable
attr(dsc, "scaled:scale") # the standard deviation of each variable

library(ggplot2)
library(reshape2)
dscm <- melt(model$centers)
names(dscm) <- c("Cluster", "Variable", "Value")
dscm$Cluster <- factor(dscm$Cluster)
dscm$Order <- as.vector(sapply(1:length(dscm), rep, 18))
p <- ggplot(subset(dscm, Cluster %in% 1:10),
            aes(x=reorder(Variable, Order),
                y=Value, group=Cluster, colour=Cluster))
p <- p + coord_polar()
p <- p + geom_point()
p <- p + geom_path()
p <- p + labs(x=NULL, y=NULL)
p <- p + theme(axis.ticks.y=element_blank(), axis.text.y = element_blank())
p

nclust <- 9
model <- m.kms <- kmeans(scale(indices[c(5,6,7,9,10,12,13,14,17)], nclust))
dscm <- melt(model$centers)
names(dscm) <- c("Cluster", "Variable", "Value")
dscm$Cluster <- factor(dscm$Cluster)
dscm$Order <- as.vector(sapply(1:18, rep, nclust))
p <- ggplot(dscm,
            aes(x=reorder(Variable, dscm$Order),
                y=Value, group=Cluster, colour=Cluster))
p <- p + coord_polar()
p <- p + geom_point()
p <- p + geom_path()
p <- p + labs(x=NULL, y=NULL)
p <- p + theme(axis.ticks.y=element_blank(), axis.text.y = element_blank())
p

# The function clusterboot() from fpc (Hennig, 2014) provides a 
# convenient tool to identify robust clusters.
# Jaccard similarity values of greater than 0.75 are stable and
# above 0.85 very stable.  Values of 0.6 or below "should not
# be trusted".  Stable clusters does not indicate valid clusters.
library(fpc)
model <- m.kmcb <- clusterboot(indices[,c(5,6,7,9,10,12,13,14,17)],
                     scaling = T,
                     clustermethod=kmeansCBI,
                     bootmethod=c("boot","subset"),
                     B = 50,
                     bscompare = T,
                     runs=10,
                     krange=10,
                     showplots = F,
                     seed=12)
model
str(model)
print(model)
par(mar=c(0,0,0,0))
plot(model)

# Evaluate model quality
model <- kmeans(scale(indices[,c(5,6,7,9,10,12,13,14,17)]),3)
model$totss
model$withinss
model$tot.withinss

# Scree plot
crit <- vector()
nk <- 1:100
t <- c(5,6,7,9,10,12,13,14,17)
for (k in nk)
{
m <- kmeans(scale(indices[,t]), k, iter.max = 20)
crit <- c(crit, sum(m$withinss))
}
crit
plot(crit)

# Principal Component Analysis
summary(pc.cr <- princomp(indices[t], cor = TRUE))
loadings(pc.cr)  # note that blank entries are small but not zero
## The signs of the columns are arbitrary
plot(pc.cr) # shows a screeplot.
biplot(pc.cr, cex=c(0.1,1))


m <- kmeans(scale(ds[numi]), 5)
ic <- intCriteria(as.matrix(ds[numi]), m$cluster, "all")
names(ic)