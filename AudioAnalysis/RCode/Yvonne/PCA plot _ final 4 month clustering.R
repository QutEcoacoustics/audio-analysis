#  Date: 18 December 2015
#  R version:  3.2.1 
#  This file calculates the Principal Component Analysis and plots the
#  result
########## You may wish to change these ###########################
#setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\")

cluster.list <- read.csv(file ="C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\hybrid_clust_17500_30.csv", header=T)
indices <- read.csv("C:\\Work\\CSV files\\FourMonths\\final_dataset_22June2015_10 Oct2015.csv", header=T)

ds3 <- indices[,c(3,4,7,10,11,15,16,25)] # without Mid-frequency cover
#ds3 <- indices[,c(4,7,11,15,16,25)] # this is the one in k folder referred to as
# snr_evn_aci_enp_evn
#clust <- as.character(unname(unlist(unique(cluster.list)))) 
site <- "GympieNP&WoondumNP"
date <- paste(indices$rec.date[1], 
              indices$rec.date[length(indices$rec.date)],
              sep = "_")

############## replace NA values
site1 <- ds3[1:(length(ds3[,1])/2),]
site2 <- ds3[((length(ds3[,1])/2)+1):(length(ds3[,1])),]
for(i in 1:(ncol(site1)-1)) {  # columns
  for(j in 1:nrow(site1)) {  # rows
    if (is.na(site1[j,i])) {
      average <- mean(c(site1[(j-15),i], site1[(j-12),i], site1[(j-10),i],
                        site1[(j+15),i], site1[(j+12),i], site1[(j+10),i]),
                      na.rm=TRUE)
      site1[j,i] <- average
    }
  }
}
for(i in 1:(ncol(site2)-1)) {  # columns
  for(j in 1:nrow(site2)) {  # rows
    if (is.na(site2[j,i])) {
      average <- mean(c(site2[(j-15),i], site2[(j-12),i], site2[(j-10),i],
                        site2[(j+15),i], site2[(j+12),i], site2[(j+10),i]),
                      na.rm=TRUE)
      site2[j,i] <- average
    }
  }
}
ds6 <- rbind(site1[,1:7], site2[,1:7])

######### Normalise data #################################
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
#######################################################
# Create ds3.norm_2_98 for kmeans, clara, hclust
# a dataset normalised between 1.5 and 98.5%
#######################################################
ds3.norm_2_98 <- ds6
q1.values <- NULL
q2.values <- NULL
for (i in 1:length(ds6)) {
  q1 <- unname(quantile(ds6[,i], probs = 0.015, na.rm = TRUE))
  q2 <- unname(quantile(ds6[,i], probs = 0.985, na.rm = TRUE))
  q1.values <- c(q1.values, q1)
  q2.values <- c(q2.values, q2)
  ds3.norm_2_98[,i]  <- normalise(ds3.norm_2_98[,i], q1, q2)
}

# adjust values greater than 1 or less than 0
for (j in 1:length(ds6)) {
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] > 1 & !is.na(ds3.norm_2_98[i,j]))
      ds3.norm_2_98[i,j] = 1
  }
  for (i in 1:length(ds3.norm_2_98[,j])) {
    if (ds3.norm_2_98[i,j] < 0 & !is.na(ds3.norm_2_98[i,j]))
      ds3.norm_2_98[i,j] = 0
  }
}

######### PCA biplot #####################################
#file <- paste("Principal Component Analysis_adj_ranges", site, 
#              "_", date, ".png", sep = "")
#png(
#  file,
#  width     = 200,
#  height    = 200,
#  units     = "mm",
#  res       = 1200,
#  pointsize = 4
#)

#par(mar =c(2,2,4,2), cex.axis = 2.5)
#PCAofIndices<- prcomp(normIndices)
#biplot(PCAofIndices, col=c("pink","blue"), 
#       cex=c(0.5,0.9), ylim = ylim, 
#       xlim = xlim)
#abline(h=0,v=0)
#mtext(side = 3, line = 2, paste("Principal Component Analysis prcomp ",
#              site, date, sep = " "), cex = 2.5)

#dev.off()
#### Preparing the dataframe ###############################
#normIndices.pca <- prcomp(normIndices[,1:17], 
#                          scale. = F)
#normIndices <- all.normIndices
#normIndices <- normIndices[c(4:20,37,38)]
#normIndices.pca <- prcomp(normIndices[,1:17], scale. = F)
normIndices <- ds3.norm_2_98

normIndices <- cbind(normIndices, cluster.list)
normIndices$x <- as.factor(normIndices$hybrid_k17500k30k3)
normIndices.pca <- prcomp(normIndices[,1:7], scale. = F)
normIndices$PC1 <- normIndices.pca$x[,1]
sum(normIndices$PC1)
normIndices$PC2 <- normIndices.pca$x[,2]
normIndices$PC3 <- normIndices.pca$x[,3]
normIndices$PC4 <- normIndices.pca$x[,4]
normIndices$PC5 <- normIndices.pca$x[,5]
normIndices$PC6 <- normIndices.pca$x[,6]
normIndices$PC7 <- normIndices.pca$x[,7]
plot(normIndices.pca)
biplot(normIndices.pca)

pca.coef <- cbind(cluster.list,normIndices$PC1,normIndices$PC2,normIndices$PC3,normIndices$PC4,normIndices$PC5,
                  normIndices$PC6,normIndices$PC7)
#write.csv(pca.coef, "pca_coefficients_*", row.names = F)

# assign colours to time-periods
normIndices <- within(normIndices, levels(fourhour.class) <- c("red","orange","yellow","green","blue","violet"))
normIndices <- within(normIndices, levels(hour.class) <- 
                        c("#FF0000FF","#FF4000FF","#FF8000FF","#FFBF00FF","#FFFF00FF",
                          "#BFFF00FF","#80FF00FF","#40FF00FF","#00FF00FF","#00FF40FF",
                          "#00FF80FF","#00FFBFFF","#00FFFFFF","#00BFFFFF","#0080FFFF",
                          "#0040FFFF","#0000FFFF","#4000FFFF","#8000FFFF","#BF00FFFF",
                          "#FF00FFFF","#FF00BFFF","#FF0080FF","#FF0040FF"))
#library(raster)
#colourName <- "colourBlock.png"
#colourBlock <- brick(colourName, package="raster")
#plotRGB(colourBlock)
#colourBlock <- as.data.frame(colourBlock)
#colours <- NULL
#for(i in 1:40) {
#  col <- rgb(colourBlock$colourBlock.1[i],
#             colourBlock$colourBlock.2[i],
#             colourBlock$colourBlock.3[i],
#             max = 255)
#  colours <- c(colours, col)
#}
#colours <- colours[1:30]
colours <- c("red", "chocolate4", "palegreen", "darkblue",
             "brown1", "darkgoldenrod3", "cadetblue4", 
             "darkorchid", "orange" ,"darkseagreen", 
             "deeppink3", "darkslategrey", "firebrick2", 
             "gold2", "hotpink2", "blue", "maroon", 
             "mediumorchid4", "mediumslateblue","mistyrose4",
             "royalblue", "orange", "palevioletred2", 
             "sienna", "slateblue", "yellow", "tan2", 
             "salmon","violetred1","plum")

#write.table(colours, file="colours.csv", row.names = F)

normIndices <- within(normIndices, levels(x) <- colours)

#fooPlot <- function(x, main, ...) {
#  if(missing(main))
#    main <- deparse(substitute(x))
#  plot(x, main = main, ...)
#}

#  set.seed(42)
#dat <- data.frame(x = rnorm(1:10), y = rnorm(1:10))
#fooPlot(dat, col = "red")


#### Plotting PC1 & PC2 Principal Component Plots with base plotting system
png('pca_plot PC1_PC2_2_98_3,4,7,10,11,15,16.png', 
    width = 1800, height = 1200, units = "px") 
PrinComp_X_axis <- "PC1"
PrinComp_Y_axis <- "PC2"
first <- 1  # change this and values in plot function below!!! to match PC# 
second <- 2  # change this!!! to match PC#
start <- 1
finish <- 319680
arrowScale <- 1.3 # increase/decrease this to adjust arrow length
summ <- summary(normIndices.pca)
rotate <- unname(summ$rotation)
labels1 <- names(normIndices[1:length(summ$center)])
labels2 <- c("BGN","SNR","EPS","LFC","ACC","EPS","ECS")
mainHeader <- paste (site,indices$rec.date[start],indices$rec.date[finish],
                     PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
#par(mar=c(6,6,4,4))
par(xpd = T, mar = par()$mar + c(0,6,0,35))
plot(normIndices$PC1[start:finish],normIndices$PC2[start:finish],  # Change these!!!!! 
     col=as.character(normIndices$x[start:finish]), 
     cex=1, type='p', pch=15, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels1)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.05, 
       rotate[i,second]*arrowScale*1.05, 
       paste(labels2[i]), cex=2.6)
}
abline (v=0, h=0, lty=2)
clust <- as.character(1:30)
clust <- c("1.breeze in trees + birds",
           "2.wind +birds",
           "3.light wind + insects",
           "4.birds (afternoon)",
           "5.quiet + movement",
           "6.light rain",
           "7.insects",
           "8.wind + birds",
           "9.birds (morning)",
           "10.birds + wind",
           "11.planes in quiet environment",
           "12.planes",
           "13.planes",
           "14.birds (morning)",
           "15.breeze in trees",
           "16.birds",
           "17.heavy rain",
           "18.breeze in trees + insects",
           "19.quiet + some insects",
           "20.quiet + birds + insects",
           "21.light wind + birds",
           "22.birds (afternoon)",
           "23.birds (morning)",
           "24.quiet + some insects",
           "25.very quiet",
           "26.birds (morning)",
           "27.thunder + kookaburras",
           "28.wind + birds",
           "29.very light rain",
           "30.insects")
legend(0.9,1, clust, pch=15, col=colours, bty='n', 
       cex=2.6, title = "Clusters")
legend('topleft',labels1, col=colours, bty='n', 
       cex=2, title = "Indices")

dev.off()

#### Plotting PC1 & PC3 Principal Component Plots with base plotting system
png('pca_plot PC1_PC3_2_98_3,4,7,10,11,15,16.png', 
    width = 1800, height = 1200, units = "px") 
PrinComp_X_axis <- "PC1"
PrinComp_Y_axis <- "PC3"
first <- 1  # change this and values in plot function below!!! to match PC# 
second <- 3  # change this!!! to match PC#
start <- 1
finish <- 319680
arrowScale <- 0.76 # increase/decrease this to adjust arrow length
summ <- summary(normIndices.pca)
rotate <- unname(summ$rotation)
labels1 <- names(normIndices[1:length(summ$center)])
mainHeader <- paste (site,indices$rec.date[start],indices$rec.date[finish],
                     PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
#par(mar=c(6,6,4,4))
par(xpd = T, mar = par()$mar + c(0,6,0,35))
plot(normIndices$PC1[start:finish],normIndices$PC3[start:finish],  # Change these!!!!! 
     col=as.character(normIndices$x[start:finish]), 
     cex=1, type='p', pch=15, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels1)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.05, 
       rotate[i,second]*arrowScale*1.05, 
       paste(labels2[i]), cex=2.6)
}
abline (v=0, h=0, lty=2)
clust <- as.character(1:30)
clust <- c("1.breeze in trees + birds",
           "2.wind +birds",
           "3.light wind + insects",
           "4.birds (afternoon)",
           "5.quiet + movement",
           "6.light rain",
           "7.insects",
           "8.wind + birds",
           "9.birds (morning)",
           "10.birds + wind",
           "11.planes in quiet environment",
           "12.planes",
           "13.planes",
           "14.birds (morning)",
           "15.breeze in trees",
           "16.birds",
           "17.heavy rain",
           "18.breeze in trees + insects",
           "19.quiet + some insects",
           "20.quiet + birds + insects",
           "21.light wind + birds",
           "22.birds (afternoon)",
           "23.birds (morning)",
           "24.quiet + some insects",
           "25.very quiet",
           "26.birds (morning)",
           "27.thunder + kookaburras",
           "28.wind + birds",
           "29.very light rain",
           "30.insects")
legend(0.9,0.98, clust, pch=15, col=colours, bty='n', 
       cex=2.6, title = "Clusters")
legend('topleft',labels1, col=colours, bty='n', 
       cex=2, title = "Indices")

dev.off()

#### Plotting PC2 & PC3 Principal Component Plots with base plotting system
png('pca_plot PC2_PC3_2_98_5,7,9,10,11,12,13,17,18.png', 
    width = 1800, height = 1200, units = "px") 
PrinComp_X_axis <- "PC2"
PrinComp_Y_axis <- "PC3"
first <- 2  # change this and values in plot function below!!! to match PC# 
second <- 3  # change this!!! to match PC#
start <- 1
finish <- 319680
arrowScale <- 0.76 # increase/decrease this to adjust arrow length
summ <- summary(normIndices.pca)
rotate <- unname(summ$rotation)
labels1 <- names(normIndices[1:length(summ$center)])
mainHeader <- paste (site,indices$rec.date[start],indices$rec.date[finish],
                     PrinComp_X_axis, PrinComp_Y_axis, sep=" ")
#par(mar=c(6,6,4,4))
par(xpd = T, mar = par()$mar + c(0,6,0,35))
plot(normIndices$PC2[start:finish],normIndices$PC3[start:finish],  # Change these!!!!! 
     col=as.character(normIndices$x[start:finish]), 
     cex=1, type='p', pch=15, main=mainHeader, 
     xlab=paste(PrinComp_X_axis," (", 
                round(summ$importance[first*3-1]*100,2),"%)", 
                sep=""),
     ylab=paste(PrinComp_Y_axis," (",  
                round(summ$importance[second*3-1]*100,2),"%)", sep=""),
     cex.lab=2, cex.axis=1.2, cex.main=2)
hours <- c("12 to 4 am","4 to 8 am", "8 to 12 noon",
           "12 noon to 4 pm", "4 to 8 pm", "8 to midnight")
for (i in 1:length(labels1)) {
  arrows(0,0, rotate[i,first]*arrowScale, 
         rotate[i,second]*arrowScale, col=1, lwd=1.6)  
  text(rotate[i,first]*arrowScale*1.05, 
       rotate[i,second]*arrowScale*1.05, 
       paste(labels2[i]), cex=2.6)
}
abline (v=0, h=0, lty=2)
clust <- as.character(1:30)
clust <- c("1.breeze in trees + birds",
           "2.wind +birds",
           "3.light wind + insects",
           "4.birds (afternoon)",
           "5.quiet + movement",
           "6.light rain",
           "7.insects",
           "8.wind + birds",
           "9.birds (morning)",
           "10.birds + wind",
           "11.planes in quiet environment",
           "12.planes",
           "13.planes",
           "14.birds (morning)",
           "15.breeze in trees",
           "16.birds",
           "17.heavy rain",
           "18.breeze in trees + insects",
           "19.quiet + some insects",
           "20.quiet + birds + insects",
           "21.light wind + birds",
           "22.birds (afternoon)",
           "23.birds (morning)",
           "24.quiet + some insects",
           "25.very quiet",
           "26.birds (morning)",
           "27.thunder + kookaburras",
           "28.wind + birds",
           "29.very light rain",
           "30.insects")
legend(0.9,0.98, clust, pch=15, col=colours, bty='n', 
       cex=2.6, title = "Clusters")
legend('topleft',labels1, col=colours, bty='n', 
       cex=2, title = "Indices")

dev.off()

####### PCA plot in ggplot ################
file <- paste("Principal Component Analysis_ggbiplot_5,7,9,10,11,12,13,17,18", site, 
              "_", date, ".png", sep = "")
library(ggbiplot)
normIndices.pca <- prcomp(normIndices[,1:17], 
                          scale. = F)

# Initiate initial plot 
#p <- NULL

#p <- print(ggbiplot(normIndices.pca, obs.scale = 1, 
#           var.scale = 1, groups = normIndices$fourhour.class,
#           ellipse = TRUE, circle = F), size = 0.3)
#png('pca_biplot.png', width=1500,height=1500,units="px")  

g <- ggbiplot(normIndices.pca, obs.scale = 1, var.scale = 1,
              groups = normIndices$fourhour.class, 
              ellipse = TRUE, circle = TRUE,
              varname.size=20,
              labels=normIndices$fourhour.class,
              labels.size = 10)
g <- g + scale_color_manual(values = c("red", "orange", "yellow", 
                                       "green", "blue", "violet"))
g <- g + theme_classic()
g <- g + theme(legend.direction = 'horizontal',
               legend.position = 'top')
g <- g + theme(axis.text=element_text(size=40),
               axis.title=element_text(size=40,face="bold"))
g <- g + geom_hline(yintercept = 0)
g <- g + geom_vline(xintercept = 0)
g <- g +  theme(legend.text = element_text(size=50))
print(g)

# Open png device
png('pca_biplot.png', width=3000, height=3000,
    units="px")  

print(g)    # Print to png device         
dev.off()

####### 3d plot #################################
library(rgl) # using rgl package
#colourSet1 <- c("#FF0000FF","#FF4000FF","#FF8000FF",
#                "#FFBF00FF","#FFFF00FF","#BFFF00FF")
#colourSet2 <- c("#80FF00FF","#40FF00FF","#00FF00FF",
#                "#00FF40FF","#00FF80FF","#00FFBFFF")
#colourSet3 <- c("#00FFFFFF","#00BFFFFF","#0080FFFF",
#                "#0040FFFF","#0000FFFF","#4000FFFF")
#colourSet4 <- c("#8000FFFF","#BF00FFFF","#FF00FFFF",
#                "#FF00BFFF","#FF0080FF","#FF0040FF")

#normIndices <- within(normIndices, levels(fourhour.class) <- 
#                        c(colourSet1,colourSet2, colourSet3,colourSet4))

#day <- c(0, 1440, 2880, 4320, 5760, 7200, 8640, 10080)
#offset <- c(0,360,720,1080)
start <-  1         #day[5] + offset[4] + 1   
finish <- length(normIndices$PC1) #day[7]-1
start
finish
plot3d(normIndices$PC1[start:finish], normIndices$PC2[start:finish], 
       normIndices$PC3[start:finish], 
       col=adjustcolor(normIndices$x, alpha.f = 0.1))
spheres3d(normIndices$PC1[start:finish], normIndices$PC2[start:finish], 
          normIndices$PC3[start:finish], 
          col=adjustcolor(normIndices$x, alpha.f = 0.1),
          radius = 0.015)
xyzCoords <- data.frame(x1= numeric(10),  y1= integer(10), 
                        z1 = numeric(10), x2= numeric(10), 
                        y2= integer(10),  z2 = numeric(10))
for (i in 1:8) {
  xyzCoords$x2[i] <- rotate[i,1]
  xyzCoords$y2[i] <- rotate[i,2]
  xyzCoords$z2[i] <- rotate[i,3]
}
# xyz co-ordinates for segments
xyzCoords <- data.frame(x1= numeric(10),  y1= integer(10), 
                        z1 = numeric(10), x2= numeric(10), 
                        y2= integer(10),  z2 = numeric(10))
for (i in 1:9) {
  xyzCoords$x2[i] <- rotate[i,1]*0.8
  xyzCoords$y2[i] <- rotate[i,2]*0.8
  xyzCoords$z2[i] <- rotate[i,3]*0.8
}
segments3d(x=as.vector(t(xyzCoords[1:10,c(1,4)])),
           y=as.vector(t(xyzCoords[1:10,c(2,5)])),
           z=as.vector(t(xyzCoords[1:10,c(3,6)])), 
           lwd=2, col= "midnightblue")

#library(car) # using car package
#scatter3d(normIndices$PC1, normIndices$PC2, normIndices$PC3, 
#          point.col=normIndices$fourhour.class, surface = F)

# Acoustic flux
flux <- NULL

for (i in 1:length(normIndices$BackgroundNoise))  {
  distance <- sqrt((normIndices[i,1] - normIndices[(i+1),1])^2 +
                     (normIndices[i,2] - normIndices[(i+1),2])^2 +
                     (normIndices[i,3] - normIndices[(i+1),3])^2 +
                     (normIndices[i,4] - normIndices[(i+1),4])^2 +
                     (normIndices[i,5] - normIndices[(i+1),5])^2 +
                     (normIndices[i,6] - normIndices[(i+1),6])^2 +
                     (normIndices[i,7] - normIndices[(i+1),7])^2 +
                     (normIndices[i,8] - normIndices[(i+1),8])^2)
  # +(normIndices[i,9] - normIndices[(i+1),9])^2) 
  # +(normIndices[i,10] - normIndices[(i+1),10])^2)
  flux <- c(flux, distance)
}
###################
######## Create references for plotting dates and times ################
timeRef <- indices$minute.of.day[1]
offset <- 0 - timeRef 

timePos   <- seq((offset), (nrow(indices)+359), by = 360) # 359 ensures timelabels to end
timeLabel <- c("00:00","6:00","12:00","18:00")
timeLabel <- rep(timeLabel, length.out=(length(timePos))) 

datePos <- c(seq((offset+720), nrow(indices)+1500, by = 1440))
dateLabel <- unique(substr(indices$rec.date, 1,10))
dateLabel <- dateLabel[1:length(datePos)]

#################################

# Smoothing using a moving average with 2 sides
png('Flux_Time_series_GympieNP_ 22June2015_2_98_5,7,9,10,11,12,13,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[1:1440],type = "l",xaxt='n')
mtext(side=3,"GympieNP_22June2015_5,7,9,10,11,12,13,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1,cex=1.4)
# Trend lines using linear filters
flux.5 <- filter(flux[1:1440],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[1:1440],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[1:1440],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[1:1440],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[1:1440],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min", line=-1,cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1,cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1,cex=1.4)
plot(flux.20, xaxt='n')
mtext(side=4,"20 min",line=-1,cex=1.4)
plot(flux.30, xaxt='n')
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
###########################
png('Flux_Time_series_GympieNP_ 23June2015__2_98_5,7,9,10,11,12,13,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[1441:2880],type = "l", xaxt="n")
mtext(side=4,"1 min",line=-1, cex=1.4)
mtext(side=3,"GympieNP_23June2015_5,7,9,10,11,12,13,17,18",
      cex = 2)

flux.5 <- filter(flux[1441:2880],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[1441:2880],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[1441:2880],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[1441:2880],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[1441:2880],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt='n')
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30,xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
##############################
png('Flux_Time_series_GympieNP_ 24June2015__2_98_5,7,9,10,11,12,13,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[2881:4320],type = "l", xaxt="n")
mtext(side=4,"1 min",line=-1, cex=1.4)
mtext(side=3,"GympieNP_24June2015_5,7,9,10,11,12,13,17,18",
      cex = 2)

flux.5 <- filter(flux[2881:4320],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[2881:4320],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[2881:4320],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[2881:4320],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[2881:4320],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
#################################
png('Flux_Time_series_GympieNP_ 25June2015__2_98_5,7,9,10,11,12,13,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[4321:5760],type = "l", xaxt="n")
mtext(side=3,"GympieNP_25June2015_5,7,9,10,11,12,13,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1, cex=1.4)
flux.5 <- filter(flux[4321:5760],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[4321:5760],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[4321:5760],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[4321:5760],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[4321:5760],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
################################
png('Flux_Time_series_GympieNP_ 26June2015__2_98_5,7,9,10,11,12,13,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[5761:7200],type = "l", xaxt="n")
mtext(side=3,"GympieNP_26June2015_5,7,9,10,11,12,13,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1, cex=1.4)
flux.5 <- filter(flux[5761:7200],filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[5761:7200],filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[5761:7200],filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[5761:7200],filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[5761:7200],filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
###############################
png('Flux_Time_series_GympieNP_ 27June2015__2_98_5,7,9,10,11,12,13,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[7201:8640],type = "l", xaxt="n")
mtext(side=3,"GympieNP_27June2015_5,7,9,10,11,12,13,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1, cex=1.4)
flux.5 <- filter(flux[7201:8640], filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[7201:8640], filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[7201:8640], filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[7201:8640], filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[7201:8640], filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
#################################
png('Flux_Time_series_GympieNP_ 28June2015__2_98_5,7,9,10,11,12,13,17,18.png', 
    width=2000, height=1200, units="px") 
par(mfrow=c(6,1),cex.axis=2)
par(mar=c(0,0,0,0),oma=c(5,3,4,2))
plot(flux[8641:10080],type = "l", xaxt="n")
mtext(side=3,"GympieNP_28June2015_5,7,9,10,11,12,13,17,18",
      cex = 2)
mtext(side=4,"1 min",line=-1, cex=1.4)
flux.5 <- filter(flux[8641:10080],  filter=rep(1/5,5)) # 5 minute
flux.10 <- filter(flux[8641:10080], filter=rep(1/10,10)) # 10 minute
flux.15 <- filter(flux[8641:10080], filter=rep(1/15,15)) # 20 minute
flux.20 <- filter(flux[8641:10080], filter=rep(1/20,20)) # half-hourly
flux.30 <- filter(flux[8641:10080], filter=rep(1/30,30)) # hourly
lines(flux.10,col="red")
#lines(m.2,col="purple")
#lines(m.3,col="blue")
plot(flux.5, xaxt="n")
mtext(side=4,"5 min",line=-1, cex=1.4)
plot(flux.10, xaxt="n")
mtext(side=4,"10 min",line=-1, cex=1.4)
plot(flux.15, xaxt="n")
mtext(side=4,"15 min",line=-1, cex=1.4)
plot(flux.20, xaxt="n")
mtext(side=4,"20 min",line=-1, cex=1.4)
plot(flux.30, xaxt="n")
mtext(side=4,"30 min",line=-1, cex=1.4)
axis(side = 1, line = 0, at = timePos, labels = timeLabel, 
     mgp = c(1.8, 2, 0), cex.axis = 3)
dev.off()
######################
#An alternative way to smooth is using a modified Daniel
#plot(kernapply(flux[7201:8640], 
#               kernel("modified.daniell",7)),
#               type="l")
#plot(kernapply(flux[7201:8640], 
#               kernel("modified.daniell",3)),
#              type="l")

# Determine full day acoustic flux vectors
length <- length(normIndices$BackgroundNoise)

flux.5.24 <- filter(flux[1:length], filter=rep(1/5,5)) # 5 minute
flux.10.24 <- filter(flux[1:length], filter=rep(1/10,10)) # 10 minute
flux.15.24 <- filter(flux[1:length], filter=rep(1/15,15)) # 20 minute
flux.20.24 <- filter(flux[1:length], filter=rep(1/20,20)) # half-hourly
flux.30.24 <- filter(flux[1:length], filter=rep(1/30,30)) # hourly
flux.data <- cbind(flux, flux.5.24, flux.10.24, flux.15.24,
                   flux.20.24, flux.30.24)
file <- paste("Acoustic_flux_time_series_", site, "_", 
              "22 to 28 June 2015", ".csv", sep="")
write.table(flux.data, file=file, sep = ",", qmethod = "double",
            row.names = F)
