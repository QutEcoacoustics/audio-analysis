# clustering of medoids from 13 months of data ------------------------------------------------------
# remove all objects in the global environment
rm(list = ls())

# load normalised summary indices this has had the missing minutes
# and microphone problem minutes removed 
# the dataframe is called "indices_norm_summary"
load(file="data/datasets/normalised_summary_indices.RData")

k1_value <- 25000
k2_value <- 60
column <- k2_value/5

file_name <- paste("C:/Work/Projects/Twelve_month_clustering/Saving_dataset/data/datasets/hclust_results/hclust_clusters",
                   k1_value, ".RData", sep = "")
file_name_short <- paste("hclust_clusters_",k1_value, sep = "")
# remove unneeded values
load(file_name)
# load the cluster list 
cluster.list <- get(file_name_short, envir=globalenv())[,column]

data <- cbind(cluster.list, indices_norm_summary)

num_clus <- unique(data$cluster.list)
num_clus <- 1:length(num_clus)

# the clara function is used for large datasets
library(cluster) # needed for pam/clara functions
medoids <- NULL
for(i in 1:length(num_clus)) {
  a <- which(data$cluster.list==i)
  clust <- data[a,2:ncol(data)]
  #plot(clust$BackgroundNoise, clust$Snr)
  #points(pam(clust, 1)$medoids, pch = 16, col = "red")
  medo <- clara(clust,1)$medoids
  medoids <- rbind(medoids, medo)
}
rownames(medoids) <- as.character(as.numeric(num_clus))

View(medoids)
#write.csv(medoids, "medoids_all_data.csv", row.names = T)

dd <- medoids
dd <- data.frame(dd)
dd <- cbind(c(1:60), medoids)
colnames(dd)[] <- c("clust", "BGN","SNR","ACT",
                     "EVN", "HFC", "MFC", "LFC",
                     "ACI", "EAS", "EPS", "ECV",
                     "CLC")

#dd<-data.frame(
#  clust = 1:4, 
#  BGN = c(0, 0.13, 0.25, 0.38), 
#  SNR = c(0.1, 0.8, 0.63, 1), 
#  ACTV = c(0.12, 0.84, 1, 0.54), 
#  EPS = c(0.23, 0.7, 1, 0.67), 
#  HFC = c(0.33, 0.6, 0.1, 0.9), 
#  MFC = c(1, 0.77, 1, 0.59),
#  LFC = c(0.8, 0.2, 0.3, 0.5),
#  ACI = c(0, 0.13, 0.25, 0.38), 
#  EAS = c(0.1, 0.8, 0.63, 1), 
#  EPST = c(0.12, 0.84, 1, 0.54), 
#  ECS = c(0.23, 0.7, 1, 0.67), 
#  CLC = c(0.33, 0.6, 0.1, 0.9) 
#)


#par(mfrow=c(2,2), mar=c(1, 1, 1, 1)) #decrease default margin
#layout(matrix(1:4, ncol=2)) #draw 4 plots to device
#loop over rows to draw them, add 1 as max and 0 as min for each var
#lapply(1:4, function(i) { 
#  radarchart(rbind(rep(1,12), rep(0,12), dd[i,-1]))
#})

library(fmsb) #Functions for Medical Statistics Book with some Demographic Data

rain <- c(59,18,10,54,2,21,38,60)
wind <- c(42,47,51,56,52,45,8,40,24,19,46,28,9,25,30,20)
birds <- c(43,37,57,11,58,3,33,15,14,39,4)
insects <- c(29,17,1,27,22,26)
cicada <- c(48,44,34,7,12,32,16)
planes <- c(49,23)
quiet <- c(13,5,6,53,36,31,50,35,55,41)
rain1 <- rain[1:4]
rain2 <- rain[5:8]
wind1 <- wind[1:4]
wind2 <- wind[5:8]
wind3 <- wind[9:12]
wind4 <- wind[13:16]
birds1 <- birds[1:4]
birds2 <- birds[5:8]
birds3 <- birds[9:11]
insects1 <- insects[1:4]
insects2 <- insects[5:6]
cicada1 <- cicada[1:4]
cicada2 <- cicada[5:7]
quiet1 <- quiet[1:4]
quiet2 <- quiet[5:8]
quiet3 <- quiet[9:10]

colours <- c("#0072B2", "#0072B2", # rain
             "#56B4E9", "#56B4E9","#56B4E9", "#56B4E9", # wind
             "#009E73", "#009E73","#009E73", # birds
             "#F0E442", "#F0E442", # insects
             "#D55E00", "#D55E00", # cicada
             "#999999", "#999999","#999999", # quiet
             "#CC79A7") #planes
# colourblind pallet
#cbPalette <- c("#999999", "#E69F00", "#56B4E9", 
#               "#009E73", "#F0E442", "#0072B2", 
#               "#D55E00", "#CC79A7")
                # grey, orange, lightblue, 
                #green, yellow, darkblue, 
                #burntorange, purplepink

dd <- data.frame(dd)
all <- c("rain1", "rain2", 
         "wind1", "wind2", "wind3", "wind4",
         "birds1", "birds2", "birds3", 
         "insects1", "insects2",
         "cicada1", "cicada2", 
         "quiet1", "quiet2", "quiet3",
         "planes")

ref <- 1
for(i in all) {
  png(paste("radarPlots/plot_",i,".png",sep = ""), 
      width = 1000, height = 1000)
  par(mfrow=c(2,2), mar=c(2, 2, 2, 2), #xpd=NA, #decrease default margin
      mgp = c(0, 0.5, 0), xpd=NA, oma=c(0,1,0,0)) 
  if(i==all[1]|i==all[2]) {label <- "RAIN"}
  if(i==all[3]|i==all[4]|i==all[5]|i==all[6]) {label <- "WIND"}
  if(i==all[7]|i==all[8]|i==all[9]) {label <- "BIRDS"}
  if(i==all[10]|i==all[11]) {label <- "INSECTS"}
  if(i==all[12]|i==all[13]) {label <- "CICADAS"}
  if(i==all[14]|i==all[15]|i==all[16]) {label <- "QUIET"}
  if(i==all[17]) {label <- "PLANES"}
  lapply(get(i), function(i) {
    radarchart(rbind(rep(1,60), rep(0,60), dd[i,-1]), 
               pfcol=colours[ref], seg = 5, vlcex = 3, axistype=2,
               centerzero = TRUE, plwd = 1.5, cglcol = "black", 
               pdensity = 50, cglwd = 1.2, cglty = 1)
    text(x = -1, y = 1.37, paste("Cluster:",i), cex = 3.2)
    #text(x = -1.1, y = 1.17, paste(label), cex = 3)
  })
  ref <- ref + 1
  dev.off()
}

rain1 <- rain[1:8]
wind1 <- wind[1:8]
wind2 <- wind[9:16]
birds1 <- birds[1:8]
birds2 <- birds[9:11]
insects1 <- insects[1:6]
cicada1 <- cicada[1:7]
quiet1 <- quiet[1:8]
quiet2 <- quiet[9:10]

colours <- c("#0072B2",  # rain
             "#56B4E9", "#56B4E9", # wind
             "#009E73", "#009E73", # birds
             "#F0E442",  # insects
             "#D55E00",  # cicada
             "#999999", "#999999", # quiet
             "#CC79A7") #planes
# colourblind pallet
#cbPalette <- c("#999999", "#E69F00", "#56B4E9", 
#               "#009E73", "#F0E442", "#0072B2", 
#               "#D55E00", "#CC79A7")
# grey, orange, lightblue, 
#green, yellow, darkblue, 
#burntorange, purplepink

dd <- data.frame(dd)
all <- c("rain1",
         "wind1", "wind2", 
         "birds1", "birds2", 
         "insects1", 
         "cicada1", 
         "quiet1", "quiet2",
         "planes")

#all <- all[1]
ref <- 1
for(i in all) {
  png(paste("radarPlots/plot_",i,"4by2.png",sep = ""), 
      width = 1050, height = 500)
  par(mfrow=c(2,4), mar=c(0.5, 1.2, 0.5, 0), xpd=NA, #decrease default margin
      mgp = c(0, 0.2, 0)) 
  lapply(get(i), function(i) {
    radarchart(rbind(rep(1,60), rep(0,60), dd[i,-1]), 
               pfcol=colours[ref], seg = 5, vlcex = 1.8, axistype=2,
               centerzero = TRUE, plwd = 1.5, 
               pdensity = 60)
    text(x = -0.9, y = 1.26, paste("Cluster:",i), cex = 2)
  })
  ref <- ref + 1
  dev.off()
}


wind1 <- wind[1:16]
birds1 <- birds[1:11]
quiet1 <- quiet[1:10]

colours <- c("#56B4E9", # wind 
             "#009E73", # birds
             "#999999") # quiet 
all <- c("wind1", "birds1", "quiet1")

ref <- 1
for(i in all) {
  png(paste("radarPlots/plot_",i,"4by4.png",sep = ""), 
      width = 1050, height = 1000)
  par(mfrow=c(4,4), mar=c(0.5, 1.2, 0.5, 0), xpd=NA, #decrease default margin
      mgp = c(0, 0.2, 0)) 
  lapply(get(i), function(i) {
    radarchart(rbind(rep(1,60), rep(0,60), dd[i,-1]), 
               pfcol=colours[ref], seg = 5, vlcex = 1.8, axistype=2,
               centerzero = TRUE, plwd = 1.5, 
               pdensity = 60)
    text(x = -0.9, y = 1.26, paste("Cluster:",i), cex = 2)
  })
  ref <- ref + 1
  dev.off()
}

###############################
setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
#setwd("C:\\Work\\CSV files\\2015Jul01-120417\\Woondum3\\")

indices <- read.csv("Towsey_Summary_Indices_Gympie NP1 20150622-000000+1000to20150628-064559+1000.csv")
################ Normalise data ####################################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}

#entropy_cov <- indices[,16]/indices[,15] # Entropy of the coefficient of variance
#entropy_cov <- normalise(entropy_cov, 0,25)

#for (i in 1:length(entropy_cov)) {
#  if (entropy_cov[i] > 1) {
#    entropy_cov[i] = 1
#  }
#  if(entropy_cov[i] < 0) {
#    entropy_cov[i] =0
#  }
#}

# Pre-processing transformation of 
# and Temporal Entropy #14 to correct the long
# heavy tail 
indices[,14] <- sqrt(indices[,14])

normIndices <- indices

# normalise variable columns
normIndices[,4]  <- normalise(indices[,4], -50,-10)   # AverageSignalAmplitude
normIndices[,5]  <- normalise(indices[,5], -40,-20)   # BackgroundNoise
normIndices[,6]  <- normalise(indices[,6],  0, 50)    # Snr
normIndices[,7]  <- normalise(indices[,7],  3, 7)     # AvSnrofActive Frames
normIndices[,8]  <- normalise(indices[,8],  0, sqrt(1))     # Activity 
normIndices[,9]  <- normalise(indices[,9],  0, 2)     # EventsPerSecond
normIndices[,10] <- normalise(indices[,10], 0, 0.35)  # HighFreqCover
normIndices[,11] <- normalise(indices[,11], 0, 0.4)   # MidFreqCover
normIndices[,12] <- normalise(indices[,12], 0, 0.5)   # LowFreqCover
normIndices[,13] <- normalise(indices[,13], 0.4,0.55) # AcousticComplexity
normIndices[,14] <- normalise(indices[,14], 0, sqrt(0.3))   # TemporalEntropy
normIndices[,15] <- normalise(indices[,15], 0, 0.7)   # EntropyOfAverageSpectrum
normIndices[,16] <- normalise(indices[,16], 0, 1)     # EntropyOfVarianceSpectrum
normIndices[,17] <- normalise(indices[,17], 0, 1)     # EntropyOfPeaksSpectrum
normIndices[,18] <- normalise(indices[,18], 0, 0.7)   # EntropyOfCoVSpectrum
normIndices[,19] <- normalise(indices[,19], -0.8, 1)  # NDSI
normIndices[,20] <- normalise(indices[,20], 0, 15)    # SptDensity

# adjust values greater than 1 or less than 0
for (j in 4:20) {
  for (i in 1:length(normIndices[,j])) {
    if (normIndices[i,j] > 1) {
      normIndices[i,j] = 1
    }
  }
  for (i in 1:length(normIndices[,j])) {
    if (normIndices[i,j] < 0) {
      normIndices[i,j] = 0
    }
  }
}
indices <- normIndices
nclust <- 30
#model <- m.library(fmsb)km <- kmeans(indices[c(4:20)], 30)
model <- m.kms <- kmeans(indices[c(5,7,9,10,11,13,14,15,17,18)], nclust)

model$size
model$centers
model$iter
model$ifault
#dsc <- scale(indices[,c(5,6,7,9,10,12,13,14,17)])
#dsc <- scale(indices[,c(4:18)])
#attr(dsc, "scaled:center") # the mean of each variable
#attr(dsc, "scaled:scale") # the standard deviation of each variable

library(ggplot2)
library(reshape2)
dscm <- melt(model$centers)
names(dscm) <- c("Cluster", "Variable", "Value")
dscm$Cluster <- factor(dscm$Cluster)
#dscm$Order <- as.vector(sapply(1:length(dscm), rep, 18))
dscm$Order <- as.vector(sapply(1:10, rep, 30))

#file <- "Radar_plot.png"
png('Radar_plot_selected_indices.png', width=1500,height=1500,units="px")  
p <- ggplot(subset(dscm, Cluster %in% 1:30),
            aes(x=reorder(Variable, Order),
                y=Value, group=Cluster, colour=Cluster))
p <- p + coord_polar()
p <- p + geom_point()
p <- p + geom_path()
p <- p + labs(x=NULL, y=NULL)
p <- p + theme(axis.ticks.y=element_blank(), 
               axis.text.y = element_blank())
p <- p + theme(axis.text = element_text(size = 20)) # changes axis labels

p
print(p)
dev.off()
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
nk <- 1:45
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