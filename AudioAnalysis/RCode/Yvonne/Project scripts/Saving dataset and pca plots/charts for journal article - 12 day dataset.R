# Plot 1
png(filename = "plots/Journal_article_plot1.png",
    width = 1200, height = 600, units = "px")
x <- c(6,8,10,12,14,16,18,20,22,24,26,28,30)
y <- c(2.036, 1.255,
       1.306, 1.291, 1.302, 1.347, 
       1.933, 1.92 , 1.916, 1.931,
       1.935, 1.983, 2.022)
xmin <- 5
xmax <- 35
ymin <- 1.2
ymax <- 2.2
xlim <- c(xmin, xmax)
ylim <- c(ymin, ymax)
par(mar=c(4.3, 4.3, 3, 0.6), cex = 1.3, 
    cex.axis = 1.5, cex.lab = 1.5,
    mfcol=c(1,2))
plot(x, y, type = "b", xlab = "k", pch = 20,
     ylab = "ID3 distance",
     main = "Intra-three-day distance
     k-means and hclust ",
     #24 hour signatures - 12 days", 
     xlim = xlim, ylim = ylim,
     cex = 1.3, cex.axis = 1.6, cex.main = 1.6)
#$$$$$$$$$$$$$$$$$$$$$$$$
I3D.average <- c(1.527, 1.408, 1.523, 1.585, 1.580, 1.513)
I3D.wardD2 <- c(1.766, 1.360, 1.381, 1.763, 1.738, 1.701)
x <- c(5,10,15,20,25,30)
par(new=TRUE)
plot(x,I3D.average,type = "b",pch=0,
     #ylab = "ID3 distance",
     xlab = "", ylab = "", axes = 0,
     #main = "hclust - average and ward.D2 methods 
     #24 hour fingerprints - 12 days",
     xlim = xlim, ylim = ylim,
     cex=1.3, cex.axis=1.6, cex.main=1.5)
par(new=TRUE, xpd=TRUE)
plot(x,I3D.wardD2,type = "b", pch=5,
     xlim = xlim, ylim = ylim, axes = 0,
     yaxt="n", ylab = "", xlab = "k", xaxt ="n",
     cex=1.3, cex.axis = 1.5)
legend(x=25, y=2.23, pch = c(20,0,5), 
       c("k-means", "hclust (average)","hclust (ward.D2)"),
       bty = "n", cex=1.6)

#$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
# Plot 2
x <- c(5,10,15,20,22,24,26,28,30)#,35,40,45)
hybrid1000 <- c(1.915,1.292,1.252,1.735,1.521,1.519,1.471,1.482,1.484)#,1.494,1.592,1.601)
hybrid1500<- c(1.789,1.292,1.305,1.334,1.419,1.416,1.478,1.414,1.402)#,1.512,1.878,2.034)
hybrid2000 <- c(1.802,1.279,1.314,1.696,1.450,1.450,1.429,1.449,1.456)#,1.477,1.476,1.654)
hybrid2500<- c(1.858,1.354,1.320,1.357,1.384,1.363,1.349,1.482,1.493)#,1.508,1.568,1.737)
hybrid3000 <- c(1.677,1.299,1.305,1.310,1.406,1.369,1.375,1.385,1.389)#,1.382,1.482,1.708)
hybrid3500<- c(1.681,1.367,1.37,1.354,1.682,1.69,1.706,2.012,2.024)#,1.681,1.824,2.184)
#png("hybrid 12 day_2percent.png",height = 600,width = 600)
#par(new=TRUE)
plot(x, hybrid2500, type = "b", pch=17,
     xlim = xlim, ylim = ylim, 
     ylab = "ID3 distance", xlab = "",
     main = "Intra-three-day distance
     hybrid method",
     #main = "hybrid  
     #24 hour fingerprints - 12 days",
     cex = 1.3, cex.axis = 1.6, cex.main = 1.5)
par(new=TRUE)
plot(x,hybrid3000, type = "b", pch = 19, axes = 0,
     yaxt = "n", ylab = "", xlab = "k2", cex=1.3, cex.axis=1.6,
     xlim = xlim, ylim = ylim)
par(new=TRUE)
plot(x, hybrid3500, type = "b", pch=15, axes = 0,
     yaxt = "n", ylab = "", xlab = "", cex = 1.3, cex.axis = 1.6,
     xlim = xlim, ylim = ylim)
#par(new=TRUE)
#plot(x,hybrid3500,type = "b",pch=18,ylim = c(1.1,2.1),
#     yaxt="n",ylab = "",xlab = "k",cex=1.3,cex.axis=1.6)
legend(x=30, y=2.23, pch = c(17, 19, 15), title = "k1", #, 18), 
       c("2500", "3000", "3500"),
       bty = "n", cex = 1.6)

dev.off()
