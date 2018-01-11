########################################################
############# Plot 8 (18 cm)############################
########################################################
# Plot of distribution of clusters - Bird Clusters ------------------------
rm(list = ls())
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20150816", format="%Y%m%d")
# Prepare civil dawn, civil dusk and sunrise and sunset times
civil_dawn_2015 <- read.csv("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2015 <- civil_dawn_2015[173:228, ]
civil_sunrise <- as.numeric(substr(civil_dawn_2015$CivSunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise,2,3))
sunrise <- as.numeric(substr(civil_dawn_2015$Sunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$Sunrise,2,3))
# Prepare dates
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}
dates <- date.list
rm(date.list)

minute_list <- rep(1:1440, 56)
dates_56 <- rep(dates, each=1440)

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("C:/Work2/Projects/Twelve_,month_clustering/Saving_dataset/data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)
cluster_list <- cluster_list[1:(1440*56),]
cluster_list$dates <- dates_56
cluster_list <- cluster_list[,c(3,2,1)]
cluster_list$civ_dawn <- civil_dawn_2015$CivSunrise

# Convert civil dawn times to minutes
civ_dawn <- NULL
for(i in 1:56) {
  time <- as.character(civil_dawn_2015$CivSunrise[i])
  minutes <- as.numeric(substr(time,1,1))*60 + as.numeric(substr(time,2,3))
  civ_dawn <- c(civ_dawn, minutes)
}
civ_dawn <- rep(civ_dawn, each=1440)
cluster_list$civ_dawn_min <- civ_dawn
cluster_list$ref_civ <- 200
list <- c(3,11,14,15,37,39,43,58)
cluster_list$ref_civ <- cluster_list$minute_reference - cluster_list$civ_dawn_min
cluster_list$minute_reference <- cluster_list$minute_reference + 1
a <- which(cluster_list$minute_reference < 600)
cluster_list_temp <- cluster_list[a,]
a <- which(cluster_list_temp$cluster_list==37)
cluster_list_temp37 <- cluster_list_temp[a, ]

dev.off()
tiff("Distribution_of_clusters.tiff", 
     height=1100, width=2400, res=300)
layout(matrix(c(1,1,1,1,1,2,2,2,2), 
              nrow = 9, ncol = 1, byrow = TRUE))
#layout.show(2)
par(mar=c(0,2,0,1), oma=c(3.8,3.5,3.2,0), 
    cex.axis=1.8, cex=0.45)
pch <- c(15,1,17,0,19,2,3,4,5,6,7,8,9,10)

list2 <- -55:35
list3 <- c(-25,-15,-5,5,15,25)

cbPalette <- c("#000000","#999999", "#56B4E9", 
               "#D55E00", "#0072B2", 
               "#CC79A7","#009E73","#E69F00")
ylim <- c(0, 40)

# cluster 3 
a <- which(cluster_list_temp$cluster_list==3)
cluster_list_temp3 <- cluster_list_temp[a, ]
counts_3 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp3$ref_civ==i)
  counts_3 <- c(counts_3, length(a))
}
x <- 1:length(list2)
y <- counts_3
lo <- loess(y~x , span=0.12)
plot(list2, counts_3, ylim=ylim, xlab="", ylab="", 
     xaxt="n", col=cbPalette[6], yaxt="n", pch = pch[1])
lines(list2, predict(lo), col=cbPalette[6], lwd=1.6)
abline(v=list3)

# cluster 11
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==11)
cluster_list_temp11 <- cluster_list_temp[a, ]
counts_11 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp11$ref_civ==i)
  counts_11 <- c(counts_11, length(a))
}
x <- 1:length(list2)
y <- counts_11
lo <- loess(y~x, span=0.09)
plot(list2, counts_11, ylim=ylim, xlab="", ylab="", 
     xaxt="n", col=cbPalette[1], las=1, yaxt="n",
     pch = pch[2])
lines(list2, predict(lo), col=cbPalette[1], lwd=1.6)
abline(v=list3)
mtext(side=2, line=3.6, "Number of mintues                           ")
mtext(line=1.1,"Number of minutes in each cluster in relation to civil dawn", cex=1.2)
axis(at=c(10,20,30,40), side=2, las=1)
abline(v=0,lty=2, col="red")
text(x = -20, y = 0.97*ylim[2], "Pr-C-D", cex = 1.8)
text(x =   0, y = 0.97*ylim[2], "C-D", cex = 1.8)
text(x =  20, y = 0.97*ylim[2], "Po-C-D", cex = 1.8)

# cluster 15
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==15)
cluster_list_temp15 <- cluster_list_temp[a, ]
counts_15 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp15$ref_civ==i)
  counts_15 <- c(counts_15, length(a))
}
par(new=TRUE)
x <- 1:length(list2)
y <- counts_15
lo <- loess(y~x, span=0.12)
plot(list2, counts_15, ylim=ylim,xlab="", ylab="", 
     xaxt="n", col=cbPalette[4], yaxt="n",
     pch = pch[3])
lines(list2, predict(lo), col=cbPalette[4], lwd=1.6)
abline(v=list3)

# cluster 37
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==37)
cluster_list_temp37 <- cluster_list_temp[a, ]
counts_37 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp37$ref_civ==i)
  counts_37 <- c(counts_37, length(a))
}
x <- 1:length(list2)
y <- counts_37
lo <- loess(y~x, span=0.12)
par(new=TRUE)
plot(list2, counts_37, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=cbPalette[5], yaxt="n",
     pch=pch[4])
lines(list2, predict(lo), col=cbPalette[5], lwd=1.6)
abline(v=(0.5*length(list2)+0.5),lty=2, col="red")

# legend
label <- c("cluster 37","cluster 11","cluster 15","cluster  3")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(cbPalette[5], cbPalette[1], 
               cbPalette[4], cbPalette[6], 
               cbPalette[2], cbPalette[3], 
               cbPalette[7], cbPalette[8]),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = pch[c(4,2,3,1)],
       x.intersp = 0.9, y.intersp = 0.9, 
       inset=c(-0.15,0), lwd=1.6,
       lty=1, pt.cex = 1.4, pt.lwd = 0.8)


# plot 2
ylim <- c(0, 8)
colour8 <- cbPalette[8] # bright orange
colour7 <- cbPalette[7] # green
colour3 <- cbPalette[3] # light blue
colour2 <- cbPalette[2] # grey

# cluster 14
a <- which(cluster_list_temp$cluster_list==14)
cluster_list_temp14 <- cluster_list_temp[a, ]
counts_14 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp14$ref_civ==i)
  counts_14 <- c(counts_14, length(a))
}
x <- 1:length(list2)
y <- counts_14
lo <- loess(y~x, span = 0.075)
plot(list2, counts_14, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=colour2, yaxt="n",
     pch = pch[8])
lines(list2, predict(lo), col=colour2, lwd=1.2)
abline(v=list3)
text(x = -20, y = 0.97*ylim[2], "Pr-C-D", cex = 1.8)
text(x =   0, y = 0.97*ylim[2], "C-D", cex = 1.8)
text(x =  20, y = 0.97*ylim[2], "Po-C-D", cex = 1.8)

# cluster 39
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==39)
cluster_list_temp39 <- cluster_list_temp[a, ]
counts_39 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp39$ref_civ==i)
  counts_39 <- c(counts_39, length(a))
}
x <- 1:length(list2)
y <- counts_39
lo <- loess(y~x, span=0.075)
plot(list2, counts_39, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=colour3, yaxt="n",
     pch = pch[6])
lines(list2, predict(lo), col=colour3, lwd=1.2)

# cluster 43
par(new=TRUE)
colour <- cbPalette[7] # green
a <- which(cluster_list_temp$cluster_list==43)
cluster_list_temp43 <- cluster_list_temp[a, ]
counts_43 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp43$ref_civ==i)
  counts_43 <- c(counts_43, length(a))
}
x <- 1:length(list2)
y <- counts_43
lo <- loess(y~x, span=0.075)
plot(list2, counts_43, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=colour7, yaxt="n",
     pch = pch[7])
lines(list2, predict(lo), col=colour7, lwd=1.2)
abline(v=0,lty=2, col="red")

# cluster 58
par(new=TRUE)
colour <- "red"
a <- which(cluster_list_temp$cluster_list==58)
cluster_list_temp58 <- cluster_list_temp[a, ]
counts_58 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp58$ref_civ==i)
  counts_58 <- c(counts_58, length(a))
}
x <- 1:length(list2)
y <- counts_58
lo <- loess(y~x, span=0.075)
plot(list2, counts_58, ylim=ylim,xlab="", ylab="",
     xaxt="n", yaxt="n", col="black", las=1,
     pch = pch[5])
lines(list2, predict(lo), col="black", lwd=2)
abline(v=list3)

axis(side=1, at=list3, 
     labels=c("-25","-15","-5", "+5", "+15", "+25"))
abline(v=0, lty=2, col="red")
mtext(side=1, line = 2.6, "Minutes from Civil Dawn", cex=1)
axis(at=c(2,4,6,8), side=2, las=1, cex.axis=0.64)

label <- c("cluster 14", "cluster 39", 
           "cluster 43", "cluster 58")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour2, colour3,
               colour7, "black"),
       legend = label, cex = 2.2, bty = "n", 
       horiz = FALSE, xpd=TRUE, 
       x.intersp = 0.9, y.intersp = 0.9, 
       inset=c(-0.15,0), lwd=c(1.6,1.6,1.6,2), lty=1,
       pt.cex = 1.4, pt.lwd = 0.8,
       pch = pch[c(8,6,7,5)])

dev.off()

########################################################
############# Plot 8 (13 cm)---------------------------
########################################################
# Plot of distribution of clusters------------------------
rm(list = ls())
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20150816", format="%Y%m%d")
# Prepare civil dawn, civil dusk and sunrise and sunset times
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2015 <- civil_dawn_2015[173:228, ]
civil_sunrise <- as.numeric(substr(civil_dawn_2015$CivSunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise,2,3))
sunrise <- as.numeric(substr(civil_dawn_2015$Sunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$Sunrise,2,3))
# Prepare dates
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}
dates <- date.list
rm(date.list)

minute_list <- rep(1:1440, 56)
dates_56 <- rep(dates, each=1440)

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)
cluster_list <- cluster_list[1:(1440*56),]
cluster_list$dates <- dates_56
cluster_list <- cluster_list[,c(3,2,1)]
cluster_list$civ_dawn <- civil_dawn_2015$CivSunrise

# Convert civil dawn times to minutes
civ_dawn <- NULL
for(i in 1:56) {
  time <- as.character(civil_dawn_2015$CivSunrise[i])
  minutes <- as.numeric(substr(time,1,1))*60 + as.numeric(substr(time,2,3))
  civ_dawn <- c(civ_dawn, minutes)
}
civ_dawn <- rep(civ_dawn, each=1440)
cluster_list$civ_dawn_min <- civ_dawn
cluster_list$ref_civ <- 200
list <- c(3,11,14,15,37,39,43,58)
cluster_list$ref_civ <- cluster_list$minute_reference - cluster_list$civ_dawn_min
cluster_list$minute_reference <- cluster_list$minute_reference + 1
a <- which(cluster_list$minute_reference < 600)
cluster_list_temp <- cluster_list[a,]
a <- which(cluster_list_temp$cluster_list==37)
cluster_list_temp37 <- cluster_list_temp[a, ]

dev.off()
tiff("Distribution_of_clusters_13.tiff", 
     height=800, width=1536, res=300)
layout(matrix(c(1,1,1,1,1,2,2,2,2), 
              nrow = 9, ncol = 1, byrow = TRUE))
#layout.show(2)
par(mar=c(0,0.64*2,0,0.64*1), 
    oma=c(0.64*4,0.64*3.5,0.64*3.2,0),
    cex.axis=1.8, cex=0.45)
pch <- c(15,1,17,0,19,2,3,4,5,6,7,8,9,10)

list2 <- -55:35
list3 <- c(-25,-15,-5,5,15,25)

cbPalette <- c("#000000","#999999", "#56B4E9", 
               "#D55E00", "#0072B2", 
               "#CC79A7","#009E73","#E69F00")
ylim <- c(0, 40)

# cluster 3 
a <- which(cluster_list_temp$cluster_list==3)
cluster_list_temp3 <- cluster_list_temp[a, ]
counts_3 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp3$ref_civ==i)
  counts_3 <- c(counts_3, length(a))
}
x <- 1:length(list2)
y <- counts_3
lo <- loess(y~x , span=0.12)
plot(list2, counts_3, ylim=ylim, xlab="", ylab="", 
     xaxt="n", col=cbPalette[6], yaxt="n", pch = pch[1])
lines(list2, predict(lo), col=cbPalette[6], lwd=(0.64*1.6))
abline(v=list3)

# cluster 11
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==11)
cluster_list_temp11 <- cluster_list_temp[a, ]
counts_11 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp11$ref_civ==i)
  counts_11 <- c(counts_11, length(a))
}
x <- 1:length(list2)
y <- counts_11
lo <- loess(y~x, span=0.09)
plot(list2, counts_11, ylim=ylim, xlab="", ylab="", 
     xaxt="n", col=cbPalette[1], las=1, yaxt="n",
     pch = pch[2])
lines(list2, predict(lo), col=cbPalette[1], lwd=(0.64*1.6))
abline(v=list3)
mtext(side=2, line=2.4, cex=0.64,
      "Number of mintues                           ")
mtext(line=0.6,
      "Number of minutes in each cluster in relation to civil dawn",
  cex=1.2*0.64)
axis(at=c(10,20,30,40), side=2, las=1, cex.axis=(0.64*1/0.45))
abline(v=0,lty=2, col="black")
text(x = -20, y = 0.97*ylim[2], "Pr-C-D", cex = (0.64*1.8))
text(x =   0, y = 0.97*ylim[2], "C-D", cex = (0.64*1.8))
text(x =  20, y = 0.97*ylim[2], "Po-C-D", cex = (0.64*1.8))

# cluster 15
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==15)
cluster_list_temp15 <- cluster_list_temp[a, ]
counts_15 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp15$ref_civ==i)
  counts_15 <- c(counts_15, length(a))
}
par(new=TRUE)
x <- 1:length(list2)
y <- counts_15
lo <- loess(y~x, span=0.12)
plot(list2, counts_15, ylim=ylim,xlab="", ylab="", 
     xaxt="n", col=cbPalette[4], yaxt="n",
     pch = pch[3])
lines(list2, predict(lo), col=cbPalette[4], lwd=(0.64*1.6))
abline(v=list3)

# cluster 37
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==37)
cluster_list_temp37 <- cluster_list_temp[a, ]
counts_37 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp37$ref_civ==i)
  counts_37 <- c(counts_37, length(a))
}
x <- 1:length(list2)
y <- counts_37
lo <- loess(y~x, span=0.12)
par(new=TRUE)
plot(list2, counts_37, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=cbPalette[5], yaxt="n",
     pch=pch[4])
lines(list2, predict(lo), col=cbPalette[5], lwd=(0.64*1.6))
abline(v=(0.5*length(list2)+0.5),lty=2, col="black")

# legend
label <- c("cluster 37","cluster 11","cluster 15","cluster  3")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(cbPalette[5], cbPalette[1], 
               cbPalette[4], cbPalette[6], 
               cbPalette[2], cbPalette[3], 
               cbPalette[7], cbPalette[8]),
       legend = label, cex = 1.8, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = pch[c(4,2,3,1)],
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=(0.64*1.6),
       lty=1, pt.cex = 1.4, pt.lwd = 0.8)


# plot 2
ylim <- c(0, 8)
colour8 <- cbPalette[8] # bright orange
colour7 <- cbPalette[7] # green
colour3 <- cbPalette[3] # light blue
colour2 <- cbPalette[2] # grey

# cluster 14
a <- which(cluster_list_temp$cluster_list==14)
cluster_list_temp14 <- cluster_list_temp[a, ]
counts_14 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp14$ref_civ==i)
  counts_14 <- c(counts_14, length(a))
}
x <- 1:length(list2)
y <- counts_14
lo <- loess(y~x, span = 0.075)
plot(list2, counts_14, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=colour2, yaxt="n",
     pch = pch[8])
lines(list2, predict(lo), col=colour2, lwd=(0.64*1.2))
abline(v=list3)
text(x = -20, y = 0.97*ylim[2], "Pr-C-D", cex = (0.64*1.8))
text(x =   0, y = 0.97*ylim[2], "C-D", cex = (0.64*1.8))
text(x =  20, y = 0.97*ylim[2], "Po-C-D", cex = (0.64*1.8))

# cluster 39
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==39)
cluster_list_temp39 <- cluster_list_temp[a, ]
counts_39 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp39$ref_civ==i)
  counts_39 <- c(counts_39, length(a))
}
x <- 1:length(list2)
y <- counts_39
lo <- loess(y~x, span=0.075)
plot(list2, counts_39, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=colour3, yaxt="n",
     pch = pch[6])
lines(list2, predict(lo), col=colour3, lwd=(0.64*1.2))

# cluster 43
par(new=TRUE)
colour <- cbPalette[7] # green
a <- which(cluster_list_temp$cluster_list==43)
cluster_list_temp43 <- cluster_list_temp[a, ]
counts_43 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp43$ref_civ==i)
  counts_43 <- c(counts_43, length(a))
}
x <- 1:length(list2)
y <- counts_43
lo <- loess(y~x, span=0.075)
plot(list2, counts_43, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=colour7, yaxt="n",
     pch = pch[7])
lines(list2, predict(lo), col=colour7, lwd=(0.64*1.2))
abline(v=0,lty=2, col="red")

# cluster 58
par(new=TRUE)
colour <- "red"
a <- which(cluster_list_temp$cluster_list==58)
cluster_list_temp58 <- cluster_list_temp[a, ]
counts_58 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp58$ref_civ==i)
  counts_58 <- c(counts_58, length(a))
}
x <- 1:length(list2)
y <- counts_58
lo <- loess(y~x, span=0.075)
plot(list2, counts_58, ylim=ylim,xlab="", ylab="",
     xaxt="n", yaxt="n", col="black", las=1,
     pch = pch[5])
lines(list2, predict(lo), col="black", lwd=(0.64*2))
abline(v=list3)

axis(side=1, at=list3, cex.axis=(0.64*1/0.45),
     labels=c("-25","-15","-5", "+5", "+15", "+25"),
     padj=-0.6)
abline(v=0, lty=2, col="black")
mtext(side=1, line = 1.4, "Minutes from Civil Dawn", 
      cex=0.64)
axis(at=c(2,4,6,8), side=2, las=1, cex.axis=(0.64*(1/0.45)))

label <- c("cluster 14", "cluster 39", 
           "cluster 43", "cluster 58")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), 
       col = c(colour2, colour3,
               colour7, "black"),
       legend = label, cex = 1.8, bty = "n", 
       horiz = FALSE, xpd=TRUE, 
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=c(1.6,1.6,1.6,2), lty=1,
       pt.cex = 1.4, pt.lwd = 0.8,
       pch = pch[c(8,6,7,5)])

dev.off()

########################################################
############# Plot 5 (9 cm)-----------------------------
########################################################
rm(list = ls())
statistics <- read.csv("Species_acoustic_indices_statistics_final.csv", 
                       header = T)[1:38,1:10]
statistics <- statistics[3:38,c(1:7,10)]
# reorder the statistics list to match the reordered species below
statistics <- statistics[,c(1,4,6,7,2,3,8,5)]

names <- colnames(statistics[,2:length(statistics)])
names <- c("EYR", "WTH", "LKB", "SH1", "SH2", "WTT", "EWB")

# ALL INDICES SEPARATED (see below for individual plots)
dev.off()
cbbPalette <- c("#000000", "#E69F00", "#56B4E9", 
                "#009E73", "#F0E442", "#0072B2", 
                "#D55E00", "#CC79A7")
list3 <- c("EYR","WTH","LKB","SH1","SH2","WTT","EWB")
list3 <- c("SC1","SC2","EYR","EW","WTH","Kookaburra","WTT")
list3 <- c("EYR","WTH","Kookaburra","SC1","SC2","WTT","EWB")
pch <- c(15,1,17,0,19,2,3,4,5,6,7,8,9,10)
lty <- c(2,1,5,4,3,6)
ref <- 0

a1_list <- c("AV_BGN","AV_SNR","AV_ACT","AV_EVN","AV_HFC","AV_MFC",
             "AV_LFC","AV_ACI","AV_EAS","AV_EPS","AV_ECS","AV_CLC")
a1_list <- c("AV_MFC","AV_BGN","AV_SNR","AV_EPS","AV_ACI")
a1_list <- c("AV_MFC","AV_ACI","AV_BGN")
a11_list <- c("AV_SNR","AV_EPS", "AV_EVN")
a2_list <- c("CI_BGN","CI_SNR","CI_ACT","CI_EVN","CI_HFC","CI_MFC",
             "CI_LFC","CI_ACI","CI_EAS","CI_EPS","CI_ECS","CI_CLC")
a2_list <- c("CI_MFC","CI_BGN","CI_SNR","CI_EPS","CI_ACI")
a2_list <- c("CI_MFC","CI_ACI", "CI_BGN")
a22_list <- c("CI_SNR","CI_EPS", "CI_EVN")

lty <- as.numeric(as.vector(c(1,2,3,4,5,6,7,8,9,10,11,12)))
label_name <- paste(substr(a1_list[1],4,6),
                    substr(a1_list[2],4,6),
                    substr(a1_list[3],4,6),
                    substr(a11_list[1],4,6),
                    substr(a11_list[2],4,6),
                    sep="_")
dev.off()
ref <- 0
figures <- NULL

# Specites_acoustic_IndicesAll_final.tiff
tiff(paste("Species_acoustic_Indices_test2",label_name,"_final_.tiff",sep=""),
     height = 1085, width = 1063, res=300)
par(mfrow=c(2, 1), mar=c(0.8, 1.6, 0, 0), cex.axis=0.6, 
    cex.main=1, cex.lab=0.6, oma=c(0.4, 0, 0.6, 0.1),
    tcl=-0.2)
#plot(x=42, y=1, xlim=c(0,42), ylim=c(0,1), #type="n", 
#     axes=T, xlab="", ylab="", col="white")
ref <- 0
for(i in 1:length(a1_list)) {
  a <- which(statistics[,1]==a1_list[i])
  if(i <= length(a1_list)) {
    BGN <- statistics[a, 2:length(statistics)]
    BGN <- as.numeric(as.vector(BGN))
    a <- which(statistics[,1]==a2_list[i])
    BGN_CI <- statistics[a, 2:length(statistics)]
    BGN_CI <- as.numeric(as.vector(BGN_CI))
    CI.up <- BGN + BGN_CI + 0.00001
    CI.dn <- BGN - BGN_CI + 0.00001
    x <- c(-1, 5, 11, 17, 23, 29, 35)
    x <- as.vector(x) + 2.5
    stat <- cbind(x,BGN)
    #x <- x + ref
    #par(new=T)
    plot(x = x, y = BGN, xaxt='n', ylim=c(0,1),  
         #main='Average of the Normalised Acoustic Index per Species',
         col="black", pch=pch[i], las=1, ylab = "", 
         cex=0.6, xlim = c(0,38), mgp=c(3,0.35,0))
    lines(stat, lty=lty[[i]])
    arrows(x, CI.dn, x, CI.up, code=3, length=0.05, angle=90, 
           col="black") #colour[i])
    ref <- ref + 1  
    if(ref==1) {
      axis(side = 1, at=x, labels=names, 
           cex.axis=0.6, padj=-3)
    }
    figures <- c(figures, BGN)
  }
  mtext(side = 2, line = 1, cex = 0.6,
        'Normalised Index ± 95% C.I.                                                      ')
  mtext(side = 3, line = 0,
        "Average of the Normalised Acoustic Index per Species",
        cex=0.6)
  legend <- substr(a2_list, 4,6)
  par(xpd=FALSE) #x = 33.9, y = 1.06, 
  legend(x=x[6]-2, y=1.04, col = "black", #c(colour[1:5]), 
         legend = c(legend[1], legend[2], legend[3]), 
         cex = 0.6, bty = "n", pch=pch[1:3], lty = c(1,2,3),
         horiz = FALSE, xpd=TRUE, seg.len=3,
         x.intersp = 0.9, y.intersp = 0.9, inset=c(-0.15,0))
  abline(v=c(4,10,16,22,28,34,40))
  abline(h=c(0.2,0.4,0.6,0.8,1.0), lty=2, lwd=0.4)
  abline(h=c(0,0.1,0.3,0.5,0.7,0.9), lty=2, lwd=0.1)
  if(ref %in% c(1:(length(a1_list)-1))) {
    par(new=T)  
  }
}
ref <- 0
for(i in 1:length(list3)) {
  a <- which(statistics[,1]==a11_list[i])
  if(i <= length(a11_list)) {
    BGN <- statistics[a, 2:length(statistics)]
    BGN <- as.numeric(as.vector(BGN))
    a <- which(statistics[,1]==a22_list[i])
    BGN_CI <- statistics[a, 2:length(statistics)]
    BGN_CI <- as.numeric(as.vector(BGN_CI))
    CI.up <- BGN + BGN_CI + 0.00001
    CI.dn <- BGN - BGN_CI + 0.00001
    x <- c(-1, 5, 11, 17, 23, 29, 35)
    x <- as.vector(x) + 2.5
    stat <- cbind(x,BGN)
    #x <- x + ref
    #par(new=T)
    plot(x = x, y = BGN, xaxt='n', ylim=c(0,1),
         col="black", pch=pch[i+3], las=1, ylab = "", 
         cex=0.6, xlim = c(0,38), mgp=c(3,0.35,0))
    lines(stat, lty=lty[[i]])
    arrows(x, CI.dn, x, CI.up, code=3, length=0.05, 
           angle=90, col="black") #colour[i])
    ref <- ref + 1  
    if(ref==1) {
      axis(1, at=x, labels=names,
           cex.axis=0.6, padj=-3)
    }
    figures <- c(figures, BGN)
  }
  #mtext(side = 2, line = 3.9, cex = 2.2,
  #      'Normalised Index ± 95% C.I.')
  mtext(side = 1, line = 0.3, "Species", cex = 0.6)
  legend <- substr(a22_list, 4,6)
  par(xpd=FALSE) #x = 33.9, y = 1.06, 
  legend(x=x[6]-2, y=1.04, col = "black", #c(colour[1:5]), 
         legend = c(legend[1], legend[2], legend[3]), 
         cex = 0.6, bty = "n", pch=pch[4:6], lty = c(1:3),
         horiz = FALSE, xpd=TRUE,
         x.intersp = 0.9, y.intersp = 0.9, 
         inset=c(-0.15,0), seg.len=3)
  abline(v=c(4,10,16,22,28,34,40))
  abline(h=c(0.2,0.4,0.6,0.8,1.0), lty=2, lwd=0.4)
  abline(h=c(0,0.1,0.3,0.5,0.7,0.9), lty=2, lwd=0.1)
  if(ref %in% c(1:(length(a11_list)-1))) {
    par(new=T)  
  }
}
dev.off()
#figures
##########################################################
# Phases of the moon
first_quarter <- c("2015-06-22", "2015-06-23",
                   "2015-06-24", "2015-06-25",
                   "2015-06-26", "2015-06-27",
                   "2015-06-28", "2015-07-21",
                   "2015-07-22", "2015-07-23",
                   "2015-07-24", "2015-07-25",
                   "2015-07-26", "2015-07-27")
full_moon <- c("2015-06-29", "2015-06-30",
               "2015-07-01", "2015-07-02",
               "2015-07-03", "2015-07-04",
               "2015-07-05", "2015-07-28",
               "2015-07-29", "2015-07-30",
               "2015-07-31", "2015-08-01",
               "2015-08-02", "2015-08-03")
last_quarter <- c("2015-07-06", "2015-07-07",
                  "2015-07-08", "2015-07-09",
                  "2015-07-10", "2015-07-11",
                  "2015-07-12", "2015-08-04",
                  "2015-08-05", "2015-08-06",
                  "2015-08-07", "2015-08-08",
                  "2015-08-09", "2015-08-10")
new_moon <- c("2015-07-13", "2015-07-14",
              "2015-07-15", "2015-07-16",
              "2015-07-17", "2015-07-18",
              "2015-07-19", "2015-07-20",
              "2015-08-11", "2015-08-12",
              "2015-08-13", "2015-08-14",
              "2015-08-15", "2015-08-16")
cluster_list$moon_phase <- "phase"
par(mfrow=c(4,1), mar=c(2,3,1,1))

a <- which(cluster_list$dates %in% first_quarter & cluster_list$ref_civ >=-25 & cluster_list$ref_civ < 25)
cluster_list$moon_phase[a] <- "first_quarter"
table(cluster_list$cluster_list[a])
a37 <- which(cluster_list$cluster_list[a]==37)
a15 <- which(cluster_list$cluster_list[a]==15)
a11 <- which(cluster_list$cluster_list[a]==11)
a58 <- which(cluster_list$cluster_list[a]==58)
a3 <- which(cluster_list$cluster_list[a]==3)
sd(cluster_list$ref_civ[a][a37])
mean(cluster_list$ref_civ[a][a37])
median(cluster_list$ref_civ[a][a37])
min(cluster_list$ref_civ[a][a37])
sd(cluster_list$ref_civ[a][a15])
mean(cluster_list$ref_civ[a][a15])
median(cluster_list$ref_civ[a][a15])
min(cluster_list$ref_civ[a][a15])
sd(cluster_list$ref_civ[a][a11])
mean(cluster_list$ref_civ[a][a11])
median(cluster_list$ref_civ[a][a11])
min(cluster_list$ref_civ[a][a11])
sd(cluster_list$ref_civ[a][a58])
mean(cluster_list$ref_civ[a][a58])
median(cluster_list$ref_civ[a][a58])
min(cluster_list$ref_civ[a][a58])
sd(cluster_list$ref_civ[a][a3])
mean(cluster_list$ref_civ[a][a3])
median(cluster_list$ref_civ[a][a3])
min(cluster_list$ref_civ[a][a3])
plot(table(cluster_list$cluster_list[a]), ylim=c(0,250))
abline(h=c(50,100,150,200,250))

a <- which(cluster_list$dates %in% full_moon & cluster_list$ref_civ >=-25 & cluster_list$ref_civ < 25)
cluster_list$moon_phase[a] <- "full_moon"
table(cluster_list$cluster_list[a])
a37 <- which(cluster_list$cluster_list[a]==37)
a15 <- which(cluster_list$cluster_list[a]==15)
a11 <- which(cluster_list$cluster_list[a]==11)
a58 <- which(cluster_list$cluster_list[a]==58)
a3 <- which(cluster_list$cluster_list[a]==3)
sd(cluster_list$ref_civ[a][a37])
mean(cluster_list$ref_civ[a][a37])
median(cluster_list$ref_civ[a][a37])
min(cluster_list$ref_civ[a][a37])
sd(cluster_list$ref_civ[a][a15])
mean(cluster_list$ref_civ[a][a15])
median(cluster_list$ref_civ[a][a15])
min(cluster_list$ref_civ[a][a15])
sd(cluster_list$ref_civ[a][a11])
mean(cluster_list$ref_civ[a][a11])
median(cluster_list$ref_civ[a][a11])
min(cluster_list$ref_civ[a][a11])
sd(cluster_list$ref_civ[a][a58])
mean(cluster_list$ref_civ[a][a58])
median(cluster_list$ref_civ[a][a58])
min(cluster_list$ref_civ[a][a58])
sd(cluster_list$ref_civ[a][a3])
mean(cluster_list$ref_civ[a][a3])
median(cluster_list$ref_civ[a][a3])
min(cluster_list$ref_civ[a][a3])
plot(table(cluster_list$cluster_list[a]), ylim=c(0,250))
abline(h=c(50,100,150,200,250))

a <- which(cluster_list$dates %in% last_quarter & cluster_list$ref_civ >=-25 & cluster_list$ref_civ < 25)
cluster_list$moon_phase[a] <- "last_quarter"
table(cluster_list$cluster_list[a])
a37 <- which(cluster_list$cluster_list[a]==37)
a15 <- which(cluster_list$cluster_list[a]==15)
a11 <- which(cluster_list$cluster_list[a]==11)
a58 <- which(cluster_list$cluster_list[a]==58)
a3 <- which(cluster_list$cluster_list[a]==3)
sd(cluster_list$ref_civ[a][a37])
mean(cluster_list$ref_civ[a][a37])
median(cluster_list$ref_civ[a][a37])
min(cluster_list$ref_civ[a][a37])
sd(cluster_list$ref_civ[a][a15])
mean(cluster_list$ref_civ[a][a15])
median(cluster_list$ref_civ[a][a15])
min(cluster_list$ref_civ[a][a15])
sd(cluster_list$ref_civ[a][a11])
mean(cluster_list$ref_civ[a][a11])
median(cluster_list$ref_civ[a][a11])
min(cluster_list$ref_civ[a][a11])
sd(cluster_list$ref_civ[a][a58])
mean(cluster_list$ref_civ[a][a58])
median(cluster_list$ref_civ[a][a58])
min(cluster_list$ref_civ[a][a58])
sd(cluster_list$ref_civ[a][a3])
mean(cluster_list$ref_civ[a][a3])
median(cluster_list$ref_civ[a][a3])
min(cluster_list$ref_civ[a][a3])
plot(table(cluster_list$cluster_list[a]), ylim=c(0,250))
abline(h=c(50,100,150,200,250))

a <- which(cluster_list$dates %in% new_moon & cluster_list$ref_civ >=-25 & cluster_list$ref_civ < 25)
cluster_list$moon_phase[a] <- "new_moon"
table(cluster_list$cluster_list[a])
a37 <- which(cluster_list$cluster_list[a]==37)
a15 <- which(cluster_list$cluster_list[a]==15)
a11 <- which(cluster_list$cluster_list[a]==11)
a58 <- which(cluster_list$cluster_list[a]==58)
a3 <- which(cluster_list$cluster_list[a]==3)
sd(cluster_list$ref_civ[a][a37])
mean(cluster_list$ref_civ[a][a37])
median(cluster_list$ref_civ[a][a37])
min(cluster_list$ref_civ[a][a37])
sd(cluster_list$ref_civ[a][a15])
mean(cluster_list$ref_civ[a][a15])
median(cluster_list$ref_civ[a][a15])
min(cluster_list$ref_civ[a][a15])
sd(cluster_list$ref_civ[a][a11])
mean(cluster_list$ref_civ[a][a11])
median(cluster_list$ref_civ[a][a11])
min(cluster_list$ref_civ[a][a11])
sd(cluster_list$ref_civ[a][a58])
mean(cluster_list$ref_civ[a][a58])
median(cluster_list$ref_civ[a][a58])
min(cluster_list$ref_civ[a][a58])
sd(cluster_list$ref_civ[a][a3])
mean(cluster_list$ref_civ[a][a3])
median(cluster_list$ref_civ[a][a3])
min(cluster_list$ref_civ[a][a3])
plot(table(cluster_list$cluster_list[a]), ylim=c(0,250))
abline(h=c(50,100,150,200,250))
