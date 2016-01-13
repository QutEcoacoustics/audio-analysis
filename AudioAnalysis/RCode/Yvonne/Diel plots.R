# 19 December 2015
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k\\")
pca.coefficients <- read.csv("pca_coefficients.csv", header=T)
ds6 <- pca.coefficients[,2:4]
##### Normalise the dataset ################ 
normalise <- function (x, xmin, xmax) {
  y <- (x - xmin)/(xmax - xmin)
}
#######################################################
# Create ds3.norm_2_98 for kmeans, clara, hclust
# a dataset normalised between 1.5 and 98.5%
#######################################################
ds.coef_min_max <- ds6
min.values <- NULL
max.values <- NULL
for (i in 1:length(ds6)) {
  min <- unname(quantile(ds6[,i], probs = 0.0, na.rm = TRUE))
  max <- unname(quantile(ds6[,i], probs = 1.0, na.rm = TRUE))
  min.values <- c(min.values, min)
  max.values <- c(max.values, max)
  ds.coef_min_max[,i]  <- normalise(ds.coef_min_max[,i], min, max)
}
library(raster)
png("GympieNP_diel.png",width = 1000, height = 600, units="px")
r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),1]
values(g) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),2]
values(b) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),3]
rgb = rgb <-stack(r*255,g*255,b*255)
# plot RGB
par(oma=c(2,2,2,2))
plotRGB(rgb)
mtext(side=3, "Gympie NP 22 June 2015 - 10 Oct 2015",cex=2)
mtext(side=3, line = -1.5, "Normalised pca coefficients",cex=1.5)
dev.off()

png("WoondumNP_diel_norm.png",width = 1000, height = 600, units="px")
r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- ds.coef_min_max[(((length(ds.coef_min_max$normIndices.PC1)/2)+1):length(ds.coef_min_max$normIndices.PC1)),1]
values(g) <- ds.coef_min_max[(((length(ds.coef_min_max$normIndices.PC1)/2)+1):length(ds.coef_min_max$normIndices.PC1)),2]
values(b) <- ds.coef_min_max[(((length(ds.coef_min_max$normIndices.PC1)/2)+1):length(ds.coef_min_max$normIndices.PC1)),3]
rgb = rgb <-stack(r*255,g*255,b*255)
# plot RGB
par(oma=c(2,2,2,2))
plotRGB(rgb)
mtext(side=3, "Woondum NP 22 June 2015 - 10 Oct 2015",cex=2)
mtext(side=3, line = -1.5, "Normalised pca coefficients",cex=1.5)
dev.off()

PuBu <- brewer.pal(9,"PuBu")
YlGn <- brewer.pal(9,"YlGn")
BuPu <- brewer.pal(9, "BuPu")
Greys <- brewer.pal(9,"Greys")
PiYG <- brewer.pal(9,"PiYG")
Oranges <- brewer.pal(9, "Oranges")
Blues <- brewer.pal(9,"Blues")
Set3 <- brewer.pal(12,"Set3")
RdPu <- brewer.pal(9,"RdPu")
YlGnBu <- brewer.pal(9,"YlGnBu")
BrBG <- brewer.pal(9,"BrBG")
OrRd <- brewer.pal(9,"OrRd")
BuGn <- brewer.pal(9,"BuGn")
cluster.list <- read.csv(file ="hybrid_clust_17500_30.csv", header=T)

# Gympie NP plot
cluster.list1 <- cluster.list[1:(length(cluster.list$hybrid_k17500k30k3)/2),]
r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- NA
values(g) <- NA
values(b) <- NA
for (i in 1:length(cluster.list1)) {
  if (cluster.list1[i]==29) {
    a <- col2rgb((YlGnBu[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==6) {
    a <- col2rgb((YlGnBu[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==17) {
    a <- col2rgb((YlGnBu[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==10) {
    a <- col2rgb((YlGn[2]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==16) {
    a <- col2rgb((YlGn[3]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==4) {
    a <- col2rgb((YlGn[4]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==22) {
    a <- col2rgb((YlGn[5]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==23) {
    a <- col2rgb((YlGn[6]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==26) {
    a <- col2rgb((YlGn[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==14) {
    a <- col2rgb((YlGn[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==9) {
    a <- col2rgb((YlGn[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==8) {
    a <- col2rgb((YlGn[3]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==2) {
    a <- col2rgb((Oranges[4]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==1) {
    a <- col2rgb((Oranges[4]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==15) {
    a <- col2rgb((Oranges[5]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==18) {
    a <- col2rgb((Oranges[6]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==21) {
    a <- col2rgb((Oranges[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==7) {
    a <- col2rgb((Set3[2]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==30) {
    a <- col2rgb((Set3[12]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==3) {
    a <- col2rgb((Greys[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==5) {
    a <- col2rgb((Greys[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==19) {
    a <- col2rgb((Greys[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==20) {
    a <- col2rgb((Greys[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==24) {
    a <- col2rgb((Greys[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==25) {
    a <- col2rgb((Greys[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==27) {
    a <- col2rgb((PiYG[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==11) {
    a <- col2rgb((OrRd[6]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==12) {
    a <- col2rgb((OrRd[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==13) {
    a <- col2rgb((OrRd[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list1[i]==28) {
    a <- col2rgb((BuGn[2]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
}
rgb <-stack(r,g,b)
aspect <- 0.4
png("GympieNP_diel_Assigned_colours_different_a.png",
    width = 1000*aspect, height = 100, units="mm",
    res=80)
par(oma=c(2,2,2,2))
par(mar = rep(0, 4))
plotRGB(rgb, axes=FALSE, asp=aspect)
#mtext(side=3,"Gympie NP 22 June 2015 - 10 Oct 2015",cex=2)
#mtext(side=3, line = -1.5, "Assigned cluster colours",cex=1.5)
dev.off()

# Woondum plot
png("WoondumNP_diel_Assigned_colours_different_a.png",
    width = 1000*aspect, height = 100, units = "mm", 
    units="mm", res=80)
cluster.list2 <- cluster.list[((length(cluster.list$hybrid_k17500k30k3)/2)+1):
                                length(cluster.list$hybrid_k17500k30k3),]
r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- NA
values(g) <- NA
values(b) <- NA
for (i in 1:length(cluster.list2)) {
  if (cluster.list2[i]==29) {
    a <- col2rgb((YlGnBu[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==6) {
    a <- col2rgb((YlGnBu[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==17) {
    a <- col2rgb((YlGnBu[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==10) {
    a <- col2rgb((YlGn[2]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==16) {
    a <- col2rgb((YlGn[3]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==4) {
    a <- col2rgb((YlGn[4]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==22) {
    a <- col2rgb((YlGn[5]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==23) {
    a <- col2rgb((YlGn[6]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==26) {
    a <- col2rgb((YlGn[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==14) {
    a <- col2rgb((YlGn[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==9) {
    a <- col2rgb((YlGn[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==8) {
    a <- col2rgb((YlGn[3]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==2) {
    a <- col2rgb((Oranges[4]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==1) {
    a <- col2rgb((Oranges[4]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==15) {
    a <- col2rgb((Oranges[5]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==18) {
    a <- col2rgb((Oranges[6]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==21) {
    a <- col2rgb((Oranges[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==7) {
    a <- col2rgb((Set3[2]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==30) {
    a <- col2rgb((Set3[12]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==3) {
    a <- col2rgb((Greys[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==5) {
    a <- col2rgb((Greys[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==19) {
    a <- col2rgb((Greys[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==20) {
    a <- col2rgb((Greys[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==24) {
    a <- col2rgb((Greys[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==25) {
    a <- col2rgb((Greys[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==27) {
    a <- col2rgb((PiYG[9]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==11) {
    a <- col2rgb((OrRd[6]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==12) {
    a <- col2rgb((OrRd[7]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==13) {
    a <- col2rgb((OrRd[8]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
  if (cluster.list2[i]==28) {
    a <- col2rgb((BuGn[2]))
    values(r)[i] <- a[1]
    values(g)[i] <- a[2]
    values(b)[i] <- a[3]
  }
}
rgb = rgb <-stack(r,g,b)
par(oma=c(2,2,2,2))
plotRGB(rgb)
mtext(side=3,"Woondum NP 22 June 2015 - 10 Oct 2015",cex=2)
mtext(side=3, line = -1.5, "Assigned cluster colours",cex=1.5)
dev.off()

library(colourBrewer)
png("brewer colours.png",width = 1000,height = 1000)
display.brewer.all()
dev.off()
greenPalette <- brewer.pal(9,"Greens")
image(1:9,1,as.matrix(1:9),col=greenPalette,xlab="Greens (sequential)",
      ylab="",xaxt="n",yaxt="n",bty="n")
a <-col2rgb(greenPalette[1])
orangePalette <- brewer.pal(8,"Oranges")
image(1:9,1,as.matrix(1:9),col=orangePalette,xlab="Greens (sequential)",
      ylab="",xaxt="n",yaxt="n",bty="n")
spectralPalette <- brewer.pal(11,"Spectral")
image(1:11,1,as.matrix(1:11),col=spectralPalette,xlab="Greens (sequential)",
      ylab="",xaxt="n",yaxt="n",bty="n")
bluePalette <- brewer.pal(9,"Blues")
image(1:9,1,as.matrix(1:9),col=bluePalette,xlab="Greens (sequential)",
      ylab="",xaxt="n",yaxt="n",bty="n")
BrBGPalette <- brewer.pal(11,"BrBG")
image(1:11,1,as.matrix(1:11),col=BrBGPalette,xlab="Greens (sequential)",
      ylab="",xaxt="n",yaxt="n",bty="n")
greyPalette <- brewer.pal(9,"Greys")
image(1:9,1,as.matrix(1:9),col=greyPalette,xlab="Greens (sequential)",
      ylab="",xaxt="n",yaxt="n",bty="n")
library(raster)

