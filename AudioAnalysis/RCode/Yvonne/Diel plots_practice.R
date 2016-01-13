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
# gympie pca plot
#png("GympieNP_diel.png",width = 1000, units="px")
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\")
aspect=0.4
png("GympieNP_diel.png",
    width = 1000*aspect, height = 100, units="mm",
    res=80)
r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),1]
values(g) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),2]
values(b) <- ds.coef_min_max[1:(length(ds.coef_min_max$normIndices.PC1)/2),3]
co <- rep(0,length(red)) # added
red <- round(values(r)*255,0) # added
green <- round(values(g)*255,0) # added
blue <- round(values(b)*255,0) #added
colouring <- cbind(red, green, blue, co) # added
colouring <- as.data.frame(colouring) # added
for (i in 1:length(red)) {
  colouring$co[i] <- rgb(red[i],green[i],blue[i],max=255) 
}

colOutput <- matrix(colouring$co, ncol = 1440, byrow = TRUE)
colours <- unique(colOutput)

# similar to
#df = data.frame(red = matrix(mandrill[,,1], ncol=1),  green = matrix(mandrill[,,2], ncol=1),  blue = matrix(mandrill[,,3], ncol=1))

rgb = rgb <-stack(r*255, g*255, b*255)
# plot RGB
par(mar=c(0,0,0,0))
plotRGB(rgb, asp=0.4)
par(new=TRUE)
x <- 1:1440
y <- rep(111,1440)
par(mar=c(0,0,0,0), oma=c(4,2,3,9), cex=0.8, cex.axis=1.2)
plot(1:1440, y, type="n", xlab="", ylab="", axes=F)
at <- seq(1, 1441, by = 240)
at[2:length(at)] <- at[2:length(at)]-1 
axis(1, at = at, labels = c("00:00",
                            "04:00","08:00","12:00","16:00","20:00",
                            "24:00"), cex.axis=1.5)
axis(4, at = c(111-0,111-10,111-41,111-72,111-102), 
     labels=c("22 Jun 2015","1 Jul 2015","1 Aug 2015", "1 Sept 2015", "1 Oct 2015"), 
     cex.axis=1.5, las=2)
mtext(side=3,line=1,"Gympie National Park", cex = 1.8)
mtext(side=3, line = -1.5, "Normalised pca coefficients",cex=1.5)
#rotate(rotate(rotate(x)))
dev.off()
#mtext(side=3, "Gympie NP 22 June 2015 - 10 Oct 2015",cex=2)
#dev.off()

r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- ds.coef_min_max[(((length(ds.coef_min_max$normIndices.PC1)/2)+1):length(ds.coef_min_max$normIndices.PC1)),1]
values(g) <- ds.coef_min_max[(((length(ds.coef_min_max$normIndices.PC1)/2)+1):length(ds.coef_min_max$normIndices.PC1)),2]
values(b) <- ds.coef_min_max[(((length(ds.coef_min_max$normIndices.PC1)/2)+1):length(ds.coef_min_max$normIndices.PC1)),3]
rgb = rgb <-stack(r*255,g*255,b*255)
# plot RGB
#par(oma=c(2,2,2,2))
png("WoondumNP_diel_norm.png",width = 1000, height = 600, units="px")
par(mar=c(0,0,0,0))
plotRGB(rgb, asp=0.4)
#mtext(side=3, "Woondum NP 22 June 2015 - 10 Oct 2015",cex=2)
#mtext(side=3, line = -1.5, "Normalised pca coefficients",cex=1.5)
dev.off()

# setting the working directory to Folder j
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\")
# setting the colours using RcolorBrewer
library(RColorBrewer)
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

output <- matrix(cluster.list1, ncol = 1440, byrow = TRUE)
cols <- c(
  '1' = "#778899",
  '2' = "#778899",
  '3' = "#FFFFB3",  #Set3_2
  '4' = "#252525",  #Greys8
  '5' = "#87CEFA",
  '6' = "#000000",  #Greys9
  '7' = "#87CEFA",  
  '8' = "#252525",  #Greys8
  '9' = "#006837",  #YlGn8
  '10' = "#D9F0A3",  #YlGn3
  '11' = "#F1B6DA",  #PiYG
  '12' = "#F7FCB9",  #YlGn2
  '13' = "#F1B6DA",  #PiYG
  '14' = "#778899",
  '15' = "#778899",
  '16' = "#ADDD8E",  #YlGn4
  '17' = "#CAE1FF",  
  '18' = "#004529", #YlGn9
  '19' = "#000080", 
  '20' = "#78C679",  #YlGn5
  '21' = "#525252",  #Greys7
  '22' = "#252525",  #Greys8
  '23' = "#004529",  #YlGn9
  '24' = "#000000",  #Greys9
  '25' = "#7FFF00",
  '26' = "#41AB5D",  #YlGn6
  '27' = "#238443",  #YlGn7
  '28' = "#78C679",   #YlGn5
  '29' = "#FFFFE5",  #YlGn1
  '30' = "#778899"
)

output1 <- apply(output, 2, rev)
# plot Gympie NP plot
png("GympieNP_diel_Assigned_colours_different_new.png",
    width = 1000*aspect, height = 100, units="mm",
    res=80)
par(mar=c(4,2,3,9), cex=0.8, cex.axis=1.2)
image(1:ncol(output1), 1:nrow(output1), 
      as.matrix(t(output1)), col=cols,
      ylab="Month",xaxt="n", yaxt="n",
      xlab="Time (hours)", cex.lab=1.5)
at <- seq(1, 1441, by = 240)
at[2:length(at)] <- at[2:length(at)]-1 
axis(1, at = at, labels = c("00:00",
                "04:00","08:00","12:00","16:00","20:00",
                "24:00"), cex.axis=1.5)
axis(4, at = c(111-0,111-10,111-41,111-72,111-102), 
     labels=c("22 Jun 2015","1 Jul 2015","1 Aug 2015", "1 Sept 2015", "1 Oct 2015"), 
     cex.axis=1.5, las=2)
mtext(side=3,line=1,"Gympie National Park", cex = 1.8)
#rotate(rotate(rotate(x)))
dev.off()


cluster.list2 <- cluster.list[((length(cluster.list$hybrid_k17500k30k3)/2)+1):
                                length(cluster.list$hybrid_k17500k30k3),]
output <- matrix(cluster.list2, ncol = 1440, byrow = TRUE)
output2 <- apply(output, 2, rev)

# plot Woondum NP plot
png("WoondumNP_diel_Assigned_colours_different_new.png",
    width = 1000*aspect, height = 100, units="mm",
    res=80)
par(mar=c(4,2,3,9), cex=0.8, cex.axis=1.2)
image(1:ncol(output2), 1:nrow(output2), 
      as.matrix(t(output2)), col=cols,
      ylab="Month",xaxt="n", yaxt="n",
      xlab="Time (hours)", cex.lab=1.5)
at <- seq(1, 1441, by = 240)
at[2:length(at)] <- at[2:length(at)]-1 
axis(1, at = at, labels = c("00:00",
                            "04:00","08:00","12:00","16:00","20:00",
                            "24:00"), cex.axis=1.5)
axis(4, at = c(111-0,111-10,111-41,111-72,111-102), 
     labels=c("22 Jun 2015","1 Jul 2015","1 Aug 2015", "1 Sept 2015", "1 Oct 2015"), 
     cex.axis=1.5, las=2)
mtext(side=3,line=1,"Woondum National Park", cex = 1.8)
#rotate(rotate(rotate(x)))
dev.off()



r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- NA
values(g) <- NA
values(b) <- NA
YlGn1 <- col2rgb((YlGn[1]))
YlGn2 <- col2rgb((YlGn[2]))
YlGn3 <- col2rgb((YlGn[3]))
YlGn4 <- col2rgb((YlGn[4]))
YlGn5 <- col2rgb((YlGn[5]))
YlGn6 <- col2rgb((YlGn[6]))
YlGn7 <- col2rgb((YlGn[7]))
YlGn8 <- col2rgb((YlGn[8]))
YlGn9 <- col2rgb((YlGn[9]))
Grey9 <- col2rgb((Greys[9]))
Grey8 <- col2rgb((Greys[8]))
Grey7 <- col2rgb((Greys[7]))
PiYG3 <- col2rgb((PiYG[3]))
#BrBG9 <- col2rgb((BrBG[9]))
#BrBG7 <- col2rgb((BrBG[7]))
#BrBG6 <- col2rgb((BrBG[6]))
BrBG9 <- col2rgb("#000080") # rain
BrBG7 <- col2rgb("#87CEFA") # lighter rain
#Blues5 <- col2rgb((Blues[5])) # wind
#Blues8 <- col2rgb((Blues[8])) # lighter wind
Blues5 <- col2rgb("#CAE1FF")
Blues8 <- col2rgb("#778899")
Set3_2 <- col2rgb((Set3[2]))
#Set3_7 <- col2rgb((Set3[7]))
Set3_7 <- col2rgb("#7FFF00")

for (i in 1:length(cluster.list1)) {
  if (cluster.list1[i]==29) {
    #YlGn1  # birds (morning)
    values(r)[i] <- YlGn1[1]
    values(g)[i] <- YlGn1[2]
    values(b)[i] <- YlGn1[3]
  }
  if (cluster.list1[i]==12) {
    #YlGn2  # birds(morning)
    values(r)[i] <- YlGn2[1]
    values(g)[i] <- YlGn2[2]
    values(b)[i] <- YlGn2[3]
  }
  if (cluster.list1[i]==10) {
    #YlGn3
    values(r)[i] <- YlGn3[1]
    values(g)[i] <- YlGn3[2]
    values(b)[i] <- YlGn3[3]
  }
  #display.brewer.pal(9, "YlGn")
  #display.brewer.pal(9, "YlGnBu")
  if (cluster.list1[i]==16) {
    #YlGn4
    values(r)[i] <- YlGn4[1]
    values(g)[i] <- YlGn4[2]
    values(b)[i] <- YlGn4[3]
  }
  if (cluster.list1[i]==28) {
    #YlGn5
    values(r)[i] <- YlGn5[1]
    values(g)[i] <- YlGn5[2]
    values(b)[i] <- YlGn5[3]
  }
  if (cluster.list1[i]==26) {
    #YlGn6
    values(r)[i] <- YlGn6[1]
    values(g)[i] <- YlGn6[2]
    values(b)[i] <- YlGn6[3]
  }
  if (cluster.list1[i]==27) {
    #YlGn7
    values(r)[i] <- YlGn7[1]
    values(g)[i] <- YlGn7[2]
    values(b)[i] <- YlGn7[3]
  }
  if (cluster.list1[i]==9) {
    #YlGn8
    values(r)[i] <- YlGn8[1]
    values(g)[i] <- YlGn8[2]
    values(b)[i] <- YlGn8[3]
  }
  if (cluster.list1[i]==18) {
    #YlGn9
    values(r)[i] <- YlGn9[1]
    values(g)[i] <- YlGn9[2]
    values(b)[i] <- YlGn9[3]
  }
  if (cluster.list1[i]==20) {
    #YlGn5
    values(r)[i] <- YlGn5[1]
    values(g)[i] <- YlGn5[2]
    values(b)[i] <- YlGn5[3]
  }
  if (cluster.list1[i]==23) {
    #YlGn9
    values(r)[i] <- YlGn9[1]
    values(g)[i] <- YlGn9[2]
    values(b)[i] <- YlGn9[3]
  }
  #display.brewer.pal(9, "Greys")
  if (cluster.list1[i]==6) {
    #Grey9  # Very quiet
    values(r)[i] <- Grey9[1]
    values(g)[i] <- Grey9[2]
    values(b)[i] <- Grey9[3]
  }
  if (cluster.list1[i]==24) {
    #Grey9
    values(r)[i] <- Grey9[1]
    values(g)[i] <- Grey9[2]
    values(b)[i] <- Grey9[3]
  }
  if (cluster.list1[i]==4) {
    #Grey8
    values(r)[i] <- Grey8[1]
    values(g)[i] <- Grey8[2]
    values(b)[i] <- Grey8[3]
  }
  if (cluster.list1[i]==8) {
    #Grey8
    values(r)[i] <- Grey8[1]
    values(g)[i] <- Grey8[2]
    values(b)[i] <- Grey8[3]
  }
  if (cluster.list1[i]==22) {
    #Grey8
    values(r)[i] <- Grey8[1]
    values(g)[i] <- Grey8[2]
    values(b)[i] <- Grey8[3]
  }
  if (cluster.list1[i]==21) {
    #Grey7
    values(r)[i] <- Grey7[1]
    values(g)[i] <- Grey7[2]
    values(b)[i] <- Grey7[3]
  }
  #display.brewer.pal(9, "PiYG")
  if (cluster.list1[i]==13) {  # Planes
    #PiYG3
    values(r)[i] <- PiYG3[1]
    values(g)[i] <- PiYG3[2]
    values(b)[i] <- PiYG3[3]
  }
  if (cluster.list1[i]==11) {
    #PiYG3
    values(r)[i] <- PiYG3[1]
    values(g)[i] <- PiYG3[2]
    values(b)[i] <- PiYG3[3]
  }
  #display.brewer.pal(9, "BrBG")
  if (cluster.list1[i]==19) { # Rain
    #BrBG9
    values(r)[i] <- BrBG9[1]
    values(g)[i] <- BrBG9[2]
    values(b)[i] <- BrBG9[3]
  }
  if (cluster.list1[i]==7) { # Rain
    #BrBG7
    values(r)[i] <- BrBG7[1]
    values(g)[i] <- BrBG7[2]
    values(b)[i] <- BrBG7[3]
  }
  if (cluster.list1[i]==5) { # Rain
    #BrBG7
    values(r)[i] <- BrBG7[1]
    values(g)[i] <- BrBG7[2]
    values(b)[i] <- BrBG7[3]
  }
  #display.brewer.pal(9, "Blues")
  if (cluster.list1[i]==17) { # Wind
    #Blues5
    values(r)[i] <- Blues5[1]
    values(g)[i] <- Blues5[2]
    values(b)[i] <- Blues5[3]
  }
  if (cluster.list1[i]==1) { # Slight wind
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  if (cluster.list1[i]==15) { # Slight wind
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  if (cluster.list1[i]==30) { # Slight wind
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  if (cluster.list1[i]==14) { # Slight wind
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  if (cluster.list1[i]==2) {
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  #display.brewer.pal(12, "Set3")
  if (cluster.list1[i]==3) { # insects
    #Set3_2
    values(r)[i] <- Set3_2[1]
    values(g)[i] <- Set3_2[2]
    values(b)[i] <- Set3_2[3]
  }
  if (cluster.list1[i]==25) { # very loud, kookaburras etc.
    #Set3_7
    values(r)[i] <- Set3_7[1]
    values(g)[i] <- Set3_7[2]
    values(b)[i] <- Set3_7[3]
  }
}
write.csv(rgb,"Gympie_rgb.csv")

rgb <-stack(r,g,b)
aspect <- 0.4
png("GympieNP_diel_Assigned_colours_different_a.png",
    width = 1000*aspect, height = 100, units="mm",
    res=80)
par(mar = rep(0, 4))
plotRGB(rgb, axes=FALSE, asp=aspect)
#mtext(side=3,"Gympie NP 22 June 2015 - 10 Oct 2015",cex=2)
#mtext(side=3, line = -1.5, "Assigned cluster colours",cex=1.5)
dev.off()

# Woondum plot
cluster.list2 <- cluster.list[((length(cluster.list$hybrid_k17500k30k3)/2)+1):
                                length(cluster.list$hybrid_k17500k30k3),]
r <- g <- b <- raster(ncol=1440, nrow=111)
values(r) <- NA
values(g) <- NA
values(b) <- NA
for (i in 1:length(cluster.list2)) {
  if (cluster.list2[i]==29) {
    #YlGn1  # birds (morning)
    values(r)[i] <- YlGn1[1]
    values(g)[i] <- YlGn1[2]
    values(b)[i] <- YlGn1[3]
  }
  if (cluster.list2[i]==12) {
    #YlGn2  # birds(morning)
    values(r)[i] <- YlGn2[1]
    values(g)[i] <- YlGn2[2]
    values(b)[i] <- YlGn2[3]
  }
  if (cluster.list2[i]==10) {
    #YlGn3
    values(r)[i] <- YlGn3[1]
    values(g)[i] <- YlGn3[2]
    values(b)[i] <- YlGn3[3]
  }
  #display.brewer.pal(9, "YlGn")
  #display.brewer.pal(9, "YlGnBu")
  if (cluster.list2[i]==16) {
    #YlGn4
    values(r)[i] <- YlGn4[1]
    values(g)[i] <- YlGn4[2]
    values(b)[i] <- YlGn4[3]
  }
  if (cluster.list2[i]==28) {
    #YlGn5
    values(r)[i] <- YlGn5[1]
    values(g)[i] <- YlGn5[2]
    values(b)[i] <- YlGn5[3]
  }
  if (cluster.list2[i]==26) {
    #YlGn6
    values(r)[i] <- YlGn6[1]
    values(g)[i] <- YlGn6[2]
    values(b)[i] <- YlGn6[3]
  }
  if (cluster.list2[i]==27) {
    #YlGn7
    values(r)[i] <- YlGn7[1]
    values(g)[i] <- YlGn7[2]
    values(b)[i] <- YlGn7[3]
  }
  if (cluster.list2[i]==9) {
    #YlGn8
    values(r)[i] <- YlGn8[1]
    values(g)[i] <- YlGn8[2]
    values(b)[i] <- YlGn8[3]
  }
  if (cluster.list2[i]==18) {
    #YlGn9
    values(r)[i] <- YlGn9[1]
    values(g)[i] <- YlGn9[2]
    values(b)[i] <- YlGn9[3]
  }
  if (cluster.list2[i]==20) {
    #YlGn5
    values(r)[i] <- YlGn5[1]
    values(g)[i] <- YlGn5[2]
    values(b)[i] <- YlGn5[3]
  }
  if (cluster.list2[i]==23) {
    #YlGn9
    values(r)[i] <- YlGn9[1]
    values(g)[i] <- YlGn9[2]
    values(b)[i] <- YlGn9[3]
  }
  #display.brewer.pal(9, "Greys")
  if (cluster.list2[i]==6) {
    #Grey9  # Very quiet
    values(r)[i] <- Grey9[1]
    values(g)[i] <- Grey9[2]
    values(b)[i] <- Grey9[3]
  }
  if (cluster.list2[i]==24) {
    #Grey9
    values(r)[i] <- Grey9[1]
    values(g)[i] <- Grey9[2]
    values(b)[i] <- Grey9[3]
  }
  if (cluster.list2[i]==4) {
    #Grey8
    values(r)[i] <- Grey8[1]
    values(g)[i] <- Grey8[2]
    values(b)[i] <- Grey8[3]
  }
  if (cluster.list2[i]==8) {
    #Grey8
    values(r)[i] <- Grey8[1]
    values(g)[i] <- Grey8[2]
    values(b)[i] <- Grey8[3]
  }
  if (cluster.list2[i]==22) {
    #Grey8
    values(r)[i] <- Grey8[1]
    values(g)[i] <- Grey8[2]
    values(b)[i] <- Grey8[3]
  }
  if (cluster.list2[i]==21) {
    #Grey7
    values(r)[i] <- Grey7[1]
    values(g)[i] <- Grey7[2]
    values(b)[i] <- Grey7[3]
  }
  #display.brewer.pal(9, "PiYG")
  if (cluster.list2[i]==13) {  # Planes
    #PiYG3
    values(r)[i] <- PiYG3[1]
    values(g)[i] <- PiYG3[2]
    values(b)[i] <- PiYG3[3]
  }
  if (cluster.list2[i]==11) {
    #PiYG3
    values(r)[i] <- PiYG3[1]
    values(g)[i] <- PiYG3[2]
    values(b)[i] <- PiYG3[3]
  }
  #display.brewer.pal(9, "BrBG")
  if (cluster.list2[i]==19) { # Rain
    #BrBG9
    values(r)[i] <- BrBG9[1]
    values(g)[i] <- BrBG9[2]
    values(b)[i] <- BrBG9[3]
  }
  if (cluster.list2[i]==7) { # Rain
    #BrBG7
    values(r)[i] <- BrBG7[1]
    values(g)[i] <- BrBG7[2]
    values(b)[i] <- BrBG7[3]
  }
  if (cluster.list2[i]==5) { # Rain
    #BrBG7
    values(r)[i] <- BrBG7[1]
    values(g)[i] <- BrBG7[2]
    values(b)[i] <- BrBG7[3]
  }
  #display.brewer.pal(9, "Blues")
  if (cluster.list2[i]==17) { # Wind
    #Blues5
    values(r)[i] <- Blues5[1]
    values(g)[i] <- Blues5[2]
    values(b)[i] <- Blues5[3]
  }
  if (cluster.list2[i]==1) { # Slight wind
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  if (cluster.list2[i]==15) { # Slight wind
    Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  if (cluster.list2[i]==30) { # Slight wind
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  if (cluster.list2[i]==14) { # Slight wind
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  if (cluster.list2[i]==2) {
    #Blues8
    values(r)[i] <- Blues8[1]
    values(g)[i] <- Blues8[2]
    values(b)[i] <- Blues8[3]
  }
  #display.brewer.pal(12, "Set3")
  if (cluster.list2[i]==3) { # insects
    #Set3_2
    values(r)[i] <- Set3_2[1]
    values(g)[i] <- Set3_2[2]
    values(b)[i] <- Set3_2[3]
  }
  if (cluster.list2[i]==25) { # very loud, kookaburras etc.
    #Set3_7
    values(r)[i] <- Set3_7[1]
    values(g)[i] <- Set3_7[2]
    values(b)[i] <- Set3_7[3]
  }
}
rgb <-stack(r,g,b)
aspect <- 0.4
png("WoondumNP_diel_Assigned_colours_different_a.png",
    width = 1000, units="mm",
    res=80)
par(mar=rep(0,4))
plotRGB(rgb, axes=FALSE, asp=0.25,
        maxpixels=159840)
#mtext(side=3,"Woondum NP 22 June 2015 - 10 Oct 2015",cex=2)
#mtext(side=3, line = -1.5, "Assigned cluster colours",cex=1.5)
dev.off()

aspect <- 0.4
png("WoondumNP_diel_Assigned_colours_different_c.png",
    width = 1000*aspect, height = 100, units="mm",
    res=80)
par(mar = rep(0, 4))
plotRGB(rgb, axes=FALSE, asp=aspect)
#mtext(side=3,"Gympie NP 22 June 2015 - 10 Oct 2015",cex=2)
#mtext(side=3, line = -1.5, "Assigned cluster colours",cex=1.5)
dev.off()

library(colourBrewer)
png("brewer colours.png", width = 1000, height = 1000)
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

