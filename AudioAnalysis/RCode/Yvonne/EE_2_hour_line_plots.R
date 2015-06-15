setwd("C:\\Work\\CSV files\\Eastern Eucalypt")

myFiles <- list.files(full.names=TRUE,pattern="*.csv")
length<-length(myFiles)

for(i in 1:length) {
  assign(paste("propertiesEE",i, sep=""),read.csv(myFiles[i]))
}

###############################
# library(seewave)
# library(tuneR)
# setwd("C:\\Work\\Output\\Eastern Eucalypt\\Output01EE")
# sampsound<-readWave ("20150322_113743_0min.wav")
# acoustat(sampsound, f=22050, fraction=50)
# result<-acoustat(sampsound, f=22050, fraction=50)
# result

########### PLOTTING VARIABLES ##############
positions<-c(8,23,38,53,68,83,98,113)
timeLabels1<-c("1:45 pm","2:00","2:15","2:30","2:45","3:00","3:15","3:30")
timeLabels2<-c("3:45 pm","4:00","4:15","4:30","4:45","5:00","5:15","5:30")
timeLabels3<-c("5:45 pm","6:00","6:15","6:30","6:45","7:00","7:15","7:30")
timeLabels4<-c("7:45 pm","8:00","8:15","8:30","8:45","9:00","9:15","9:30")
timeLabels5<-c("9:45 pm","10:00","10:15","10:30","10:45","11:00","11:15","11:30")
timeLabels6<-c("11:45 pm","00:00","00:15","00:30","00:45","1:00","1:15","1:30")
timeLabels7<-c("1:45 am","2:00","2:15","2:30","2:45","3:00","3:15","3:30")
timeLabels8<-c("3:45 am","4:00","4:15","4:30","4:45","5:00","5:15","5:30")
timeLabels9<-c("5:45 am","6:00","6:15","6:30","6:45","7:00","7:15","7:30")
timeLabels10<-c("7:45 am","8:00","8:15","8:30","8:45","9:00","9:15","9:30")
timeLabels11<-c("9:45 am","10:00","10:15","10:30","10:45","11:00","11:15","11:30")
timeLabels12<-c("11:45 am","12:00","12:15","12:30","12:45","1:00","1:15","1:30")

par(mfrow=c(3,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
pos1 <- 0.85 # filename position 1
pos2 <- 0.515
pos3 <- 0.185

plotQuartile <- function (data1, data2, data3, data4, data5, data6, data7, 
                          data8, pos, fileName, timeLab)
{
plot(c(0:119),c(data1,data2), 
     ylab="Frequency (Hz)", ylim=c(0,7500), xaxt='n',
     xlab="", type="l",
     mgp = c(1.5, 0.5, 0))
par(new=TRUE)
plot(c(0:119), c(data3, data4), 
     ylab="", ylim=c(0,7500),
     xlab="", type="l", xaxt='n', yaxt='n', col="red")
par(new=TRUE)
plot(c(0:119),c(data5, data6), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l", xaxt='n', yaxt='n', col="blue")
par(new=TRUE)
plot(c(0:119),c(data7, data8), 
     ylab="", ylim=c(0, 7500),
     xlab="", type="l", xaxt='n', yaxt='n', col="pink")
legend("topright", inset=c(0.04,0.0),
       legend = c("Q25", "Median", "Q75", "Centroid"),          
       col=c(1,2,4,"pink"),lty = 1,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2, 4.2))
axis(side=1, at=positions, labels=timeLab, mgp=c(1.8,0.5,0))
mtext(side=4, fileName ,line=-1.80, font=1, cex=1.0,
      outer=TRUE, at=pos)
abline(h=c(0,2000,4000,6000), lwd=0.4, lty=2)
}

png(
  "EE1.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE3$Q25,    propertiesEE4$Q25,
              propertiesEE3$median, propertiesEE4$median,
              propertiesEE3$Q75,    propertiesEE4$Q75,
              propertiesEE3$cent,   propertiesEE4$cent, pos1, 
              myFiles[3], timeLabels1)
plotQuartile (propertiesEE5$Q25,    propertiesEE6$Q25,
              propertiesEE5$median, propertiesEE6$median,
              propertiesEE5$Q75,    propertiesEE6$Q75,
              propertiesEE5$cent,   propertiesEE6$cent, pos2,
              myFiles[5], timeLabels2)
plotQuartile (propertiesEE7$Q25,    propertiesEE8$Q25,
              propertiesEE7$median, propertiesEE8$median,
              propertiesEE7$Q75,    propertiesEE8$Q75,
              propertiesEE7$cent,   propertiesEE8$cent, pos3, 
              myFiles[7], timeLabels3)
dev.off()

png(
  "EE2.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE9$Q25,    propertiesEE10$Q25,
              propertiesEE9$median, propertiesEE10$median,
              propertiesEE9$Q75,    propertiesEE10$Q75,
              propertiesEE9$cent,   propertiesEE10$cent, pos1, 
              myFiles[9], timeLabels4)

plotQuartile (propertiesEE11$Q25,    propertiesEE12$Q25,
              propertiesEE11$median, propertiesEE12$median,
              propertiesEE11$Q75,    propertiesEE12$Q75,
              propertiesEE11$cent,   propertiesEE12$cent, pos2, 
              myFiles[11], timeLabels5)

plotQuartile (propertiesEE13$Q25,    propertiesEE14$Q25,
              propertiesEE13$median, propertiesEE14$median,
              propertiesEE13$Q75,    propertiesEE14$Q75,
              propertiesEE13$cent,   propertiesEE14$cent, pos3, 
              myFiles[13], timeLabels6)
dev.off()

png(
  "EE3.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
);
par(mfrow=c(3,1)); # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5); # set margins, font size & line width
plotQuartile (propertiesEE15$Q25,    propertiesEE16$Q25,
              propertiesEE15$median, propertiesEE16$median,
              propertiesEE15$Q75,    propertiesEE16$Q75,
              propertiesEE15$cent,   propertiesEE16$cent, pos1, 
              myFiles[15], timeLabels7)

plotQuartile (propertiesEE17$Q25,    propertiesEE18$Q25,
              propertiesEE17$median, propertiesEE18$median,
              propertiesEE17$Q75,    propertiesEE18$Q75,
              propertiesEE17$cent,   propertiesEE18$cent, pos2, 
              myFiles[17], timeLabels8)

plotQuartile (propertiesEE19$Q25,    propertiesEE20$Q25,
              propertiesEE19$median, propertiesEE20$median,
              propertiesEE19$Q75,    propertiesEE20$Q75,
              propertiesEE19$cent,   propertiesEE20$cent, pos3, 
              myFiles[19], timeLabels9)
dev.off()

png(
  "EE4.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE21$Q25,    propertiesEE22$Q25,
              propertiesEE21$median, propertiesEE22$median,
              propertiesEE21$Q75,    propertiesEE22$Q75,
              propertiesEE21$cent,   propertiesEE22$cent, pos1, 
              myFiles[21], timeLabels10)

plotQuartile (propertiesEE23$Q25,    propertiesEE24$Q25,
              propertiesEE23$median, propertiesEE24$median,
              propertiesEE23$Q75,    propertiesEE24$Q75,
              propertiesEE23$cent,   propertiesEE24$cent, pos2, 
              myFiles[23], timeLabels11)

plotQuartile (propertiesEE25$Q25,    propertiesEE26$Q25,
              propertiesEE25$median, propertiesEE26$median,
              propertiesEE25$Q75,    propertiesEE26$Q75,
              propertiesEE25$cent,   propertiesEE26$cent, pos3, 
              myFiles[25], timeLabels12)
dev.off()

png(
  "EE5.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE27$Q25,    propertiesEE28$Q25,
              propertiesEE27$median, propertiesEE28$median,
              propertiesEE27$Q75,    propertiesEE28$Q75,
              propertiesEE27$cent,   propertiesEE28$cent, pos1, 
              myFiles[27], timeLabels1)

plotQuartile (propertiesEE29$Q25,    propertiesEE30$Q25,
              propertiesEE29$median, propertiesEE30$median,
              propertiesEE29$Q75,    propertiesEE30$Q75,
              propertiesEE29$cent,   propertiesEE30$cent, pos2, 
              myFiles[29], timeLabels2)

plotQuartile (propertiesEE31$Q25,    propertiesEE32$Q25,
              propertiesEE31$median, propertiesEE32$median,
              propertiesEE31$Q75,    propertiesEE32$Q75,
              propertiesEE31$cent,   propertiesEE32$cent, pos3, 
              myFiles[31], timeLabels3)

dev.off()

png(
  "EE6.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE33$Q25,    propertiesEE34$Q25,
              propertiesEE33$median, propertiesEE34$median,
              propertiesEE33$Q75,    propertiesEE34$Q75,
              propertiesEE33$cent,   propertiesEE34$cent, pos1, 
              myFiles[33], timeLabels4)

plotQuartile (propertiesEE35$Q25,    propertiesEE36$Q25,
              propertiesEE35$median, propertiesEE36$median,
              propertiesEE35$Q75,    propertiesEE36$Q75,
              propertiesEE35$cent,   propertiesEE36$cent, pos2, 
              myFiles[35], timeLabels5)

plotQuartile (propertiesEE37$Q25,    propertiesEE38$Q25,
              propertiesEE37$median, propertiesEE38$median,
              propertiesEE37$Q75,    propertiesEE38$Q75,
              propertiesEE37$cent,   propertiesEE38$cent, pos3, 
              myFiles[37], timeLabels6)

dev.off()

png(
  "EE7.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE39$Q25,    propertiesEE40$Q25,
              propertiesEE39$median, propertiesEE40$median,
              propertiesEE39$Q75,    propertiesEE40$Q75,
              propertiesEE39$cent,   propertiesEE40$cent, pos1, 
              myFiles[39], timeLabels7)

plotQuartile (propertiesEE41$Q25,    propertiesEE42$Q25,
              propertiesEE41$median, propertiesEE42$median,
              propertiesEE41$Q75,    propertiesEE42$Q75,
              propertiesEE41$cent,   propertiesEE42$cent, pos2, 
              myFiles[41], timeLabels8)

plotQuartile (propertiesEE43$Q25,    propertiesEE44$Q25,
              propertiesEE43$median, propertiesEE44$median,
              propertiesEE43$Q75,    propertiesEE44$Q75,
              propertiesEE43$cent,   propertiesEE44$cent, pos3, 
              myFiles[43], timeLabels9)
dev.off()

png(
  "EE8.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE45$Q25,    propertiesEE46$Q25,
              propertiesEE45$median, propertiesEE46$median,
              propertiesEE45$Q75,    propertiesEE46$Q75,
              propertiesEE45$cent,   propertiesEE46$cent, pos1, 
              myFiles[45], timeLabels10)

plotQuartile (propertiesEE47$Q25,    propertiesEE48$Q25,
              propertiesEE47$median, propertiesEE48$median,
              propertiesEE47$Q75,    propertiesEE48$Q75,
              propertiesEE47$cent,   propertiesEE48$cent, pos2, 
              myFiles[47], timeLabels11)

plotQuartile (propertiesEE49$Q25,    propertiesEE50$Q25,
              propertiesEE49$median, propertiesEE50$median,
              propertiesEE49$Q75,    propertiesEE50$Q75,
              propertiesEE49$cent,   propertiesEE50$cent, pos3, 
              myFiles[49], timeLabels12)

dev.off()

png(
  "EE9.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE51$Q25,    propertiesEE52$Q25,
              propertiesEE51$median, propertiesEE52$median,
              propertiesEE51$Q75,    propertiesEE52$Q75,
              propertiesEE51$cent,   propertiesEE52$cent, pos1, 
              myFiles[51], timeLabels1)

plotQuartile (propertiesEE53$Q25,    propertiesEE54$Q25,
              propertiesEE53$median, propertiesEE54$median,
              propertiesEE53$Q75,    propertiesEE54$Q75,
              propertiesEE53$cent,   propertiesEE54$cent, pos2, 
              myFiles[53], timeLabels2)

plotQuartile (propertiesEE55$Q25,    propertiesEE56$Q25,
              propertiesEE55$median, propertiesEE56$median,
              propertiesEE55$Q75,    propertiesEE56$Q75,
              propertiesEE55$cent,   propertiesEE56$cent, pos3, 
              myFiles[55], timeLabels3)

dev.off()

png(
  "EE10.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE57$Q25,    propertiesEE58$Q25,
              propertiesEE57$median, propertiesEE58$median,
              propertiesEE57$Q75,    propertiesEE58$Q75,
              propertiesEE57$cent,   propertiesEE58$cent, pos1, 
              myFiles[57], timeLabels4)

plotQuartile (propertiesEE59$Q25,    propertiesEE60$Q25,
              propertiesEE59$median, propertiesEE60$median,
              propertiesEE59$Q75,    propertiesEE60$Q75,
              propertiesEE59$cent,   propertiesEE60$cent, pos2, 
              myFiles[59], timeLabels5)

plotQuartile (propertiesEE61$Q25,    propertiesEE62$Q25,
              propertiesEE61$median, propertiesEE62$median,
              propertiesEE61$Q75,    propertiesEE62$Q75,
              propertiesEE61$cent,   propertiesEE62$cent, pos3, 
              myFiles[61], timeLabels6)

dev.off()

png(
  "EE11.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE63$Q25,    propertiesEE64$Q25,
              propertiesEE63$median, propertiesEE64$median,
              propertiesEE63$Q75,    propertiesEE64$Q75,
              propertiesEE63$cent,   propertiesEE64$cent, pos1, 
              myFiles[63], timeLabels7)

plotQuartile (propertiesEE65$Q25,    propertiesEE66$Q25,
              propertiesEE65$median, propertiesEE66$median,
              propertiesEE65$Q75,    propertiesEE66$Q75,
              propertiesEE65$cent,   propertiesEE66$cent, pos2, 
              myFiles[65], timeLabels8)

plotQuartile (propertiesEE67$Q25,    propertiesEE68$Q25,
              propertiesEE67$median, propertiesEE68$median,
              propertiesEE67$Q75,    propertiesEE68$Q75,
              propertiesEE67$cent,   propertiesEE68$cent, pos3, 
              myFiles[67], timeLabels9)

dev.off()

png(
  "EE12.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE69$Q25,    propertiesEE70$Q25,
              propertiesEE69$median, propertiesEE70$median,
              propertiesEE69$Q75,    propertiesEE70$Q75,
              propertiesEE69$cent,   propertiesEE70$cent, pos1, 
              myFiles[69], timeLabels10)

plotQuartile (propertiesEE71$Q25,    propertiesEE72$Q25,
              propertiesEE71$median, propertiesEE72$median,
              propertiesEE71$Q75,    propertiesEE72$Q75,
              propertiesEE71$cent,   propertiesEE72$cent, pos2, 
              myFiles[71], timeLabels11)

plotQuartile (propertiesEE73$Q25,    propertiesEE74$Q25,
              propertiesEE73$median, propertiesEE74$median,
              propertiesEE73$Q75,    propertiesEE74$Q75,
              propertiesEE73$cent,   propertiesEE74$cent, pos3, 
              myFiles[73], timeLabels12)

dev.off()

png(
  "EE13.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE75$Q25,    propertiesEE76$Q25,
              propertiesEE75$median, propertiesEE76$median,
              propertiesEE75$Q75,    propertiesEE76$Q75,
              propertiesEE75$cent,   propertiesEE76$cent, pos1, 
              myFiles[75], timeLabels1)

plotQuartile (propertiesEE77$Q25,    propertiesEE78$Q25,
              propertiesEE77$median, propertiesEE78$median,
              propertiesEE77$Q75,    propertiesEE78$Q75,
              propertiesEE77$cent,   propertiesEE78$cent, pos2, 
              myFiles[77], timeLabels2)

plotQuartile (propertiesEE79$Q25,    propertiesEE80$Q25,
              propertiesEE79$median, propertiesEE80$median,
              propertiesEE79$Q75,    propertiesEE80$Q75,
              propertiesEE79$cent,   propertiesEE80$cent, pos3, 
              myFiles[79], timeLabels3)

dev.off()

png(
  "EE14.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE81$Q25,    propertiesEE82$Q25,
              propertiesEE81$median, propertiesEE82$median,
              propertiesEE81$Q75,    propertiesEE82$Q75,
              propertiesEE81$cent,   propertiesEE82$cent, pos1, 
              myFiles[81], timeLabels4)

plotQuartile (propertiesEE83$Q25,    propertiesEE84$Q25,
              propertiesEE83$median, propertiesEE84$median,
              propertiesEE83$Q75,    propertiesEE84$Q75,
              propertiesEE83$cent,   propertiesEE84$cent, pos2, 
              myFiles[83], timeLabels5)

plotQuartile (propertiesEE85$Q25,    propertiesEE86$Q25,
              propertiesEE85$median, propertiesEE86$median,
              propertiesEE85$Q75,    propertiesEE86$Q75,
              propertiesEE85$cent,   propertiesEE86$cent, pos3, 
              myFiles[85], timeLabels6)

dev.off()

png(
  "EE15.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE87$Q25,    propertiesEE88$Q25,
              propertiesEE87$median, propertiesEE88$median,
              propertiesEE87$Q75,    propertiesEE88$Q75,
              propertiesEE87$cent,   propertiesEE88$cent, pos1, 
              myFiles[87], timeLabels7)

plotQuartile (propertiesEE89$Q25,    propertiesEE90$Q25,
              propertiesEE89$median, propertiesEE90$median,
              propertiesEE89$Q75,    propertiesEE90$Q75,
              propertiesEE89$cent,   propertiesEE90$cent, pos2, 
              myFiles[89], timeLabels8)

plotQuartile (propertiesEE91$Q25,    propertiesEE92$Q25,
              propertiesEE91$median, propertiesEE92$median,
              propertiesEE91$Q75,    propertiesEE92$Q75,
              propertiesEE91$cent,   propertiesEE92$cent, pos3, 
              myFiles[91], timeLabels9)

dev.off()

png(
  "EE16.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE93$Q25,    propertiesEE94$Q25,
              propertiesEE93$median, propertiesEE94$median,
              propertiesEE93$Q75,    propertiesEE94$Q75,
              propertiesEE93$cent,   propertiesEE94$cent, pos1, 
              myFiles[93], timeLabels10)

plotQuartile (propertiesEE95$Q25,    propertiesEE96$Q25,
              propertiesEE95$median, propertiesEE96$median,
              propertiesEE95$Q75,    propertiesEE96$Q75,
              propertiesEE95$cent,   propertiesEE96$cent, pos2, 
              myFiles[95], timeLabels11)

plotQuartile (propertiesEE97$Q25,    propertiesEE98$Q25,
              propertiesEE97$median, propertiesEE98$median,
              propertiesEE97$Q75,    propertiesEE98$Q75,
              propertiesEE97$cent,   propertiesEE98$cent, pos3, 
              myFiles[97], timeLabels12)

dev.off()

png(
  "EE17.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE99$Q25,    propertiesEE100$Q25,
              propertiesEE99$median, propertiesEE100$median,
              propertiesEE99$Q75,    propertiesEE100$Q75,
              propertiesEE99$cent,   propertiesEE100$cent, pos1, 
              myFiles[99], timeLabels1)

plotQuartile (propertiesEE101$Q25,    propertiesEE102$Q25,
              propertiesEE101$median, propertiesEE102$median,
              propertiesEE101$Q75,    propertiesEE102$Q75,
              propertiesEE101$cent,   propertiesEE102$cent, pos2, 
              myFiles[101], timeLabels2)

plotQuartile (propertiesEE103$Q25,    propertiesEE104$Q25,
              propertiesEE103$median, propertiesEE104$median,
              propertiesEE103$Q75,    propertiesEE104$Q75,
              propertiesEE103$cent,   propertiesEE104$cent, pos3, 
              myFiles[103], timeLabels3)

dev.off()

png(
  "EE18.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE105$Q25,    propertiesEE106$Q25,
              propertiesEE105$median, propertiesEE106$median,
              propertiesEE105$Q75,    propertiesEE106$Q75,
              propertiesEE105$cent,   propertiesEE106$cent, pos1, 
              myFiles[105], timeLabels4)

plotQuartile (propertiesEE107$Q25,    propertiesEE108$Q25,
              propertiesEE107$median, propertiesEE108$median,
              propertiesEE107$Q75,    propertiesEE108$Q75,
              propertiesEE107$cent,   propertiesEE108$cent, pos2, 
              myFiles[107], timeLabels5)

plotQuartile (propertiesEE109$Q25,    propertiesEE110$Q25,
              propertiesEE109$median, propertiesEE110$median,
              propertiesEE109$Q75,    propertiesEE110$Q75,
              propertiesEE109$cent,   propertiesEE110$cent, pos3, 
              myFiles[109], timeLabels6)

dev.off()

png(
  "EE19.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE111$Q25,    propertiesEE112$Q25,
              propertiesEE111$median, propertiesEE112$median,
              propertiesEE111$Q75,    propertiesEE112$Q75,
              propertiesEE111$cent,   propertiesEE112$cent, pos1, 
              myFiles[111], timeLabels7)

plotQuartile (propertiesEE113$Q25,    propertiesEE114$Q25,
              propertiesEE113$median, propertiesEE114$median,
              propertiesEE113$Q75,    propertiesEE114$Q75,
              propertiesEE113$cent,   propertiesEE114$cent, pos2, 
              myFiles[113], timeLabels8)

plotQuartile (propertiesEE115$Q25,    propertiesEE116$Q25,
              propertiesEE115$median, propertiesEE116$median,
              propertiesEE115$Q75,    propertiesEE116$Q75,
              propertiesEE115$cent,   propertiesEE116$cent, pos3, 
              myFiles[115], timeLabels9)

dev.off()

png(
  "EE20.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE117$Q25,    propertiesEE118$Q25,
              propertiesEE117$median, propertiesEE118$median,
              propertiesEE117$Q75,    propertiesEE118$Q75,
              propertiesEE117$cent,   propertiesEE118$cent, pos1, 
              myFiles[117], timeLabels10)

plotQuartile (propertiesEE119$Q25,    propertiesEE120$Q25,
              propertiesEE119$median, propertiesEE120$median,
              propertiesEE119$Q75,    propertiesEE120$Q75,
              propertiesEE119$cent,   propertiesEE120$cent, pos2, 
              myFiles[119], timeLabels11)

plotQuartile (propertiesEE121$Q25,    propertiesEE122$Q25,
              propertiesEE121$median, propertiesEE122$median,
              propertiesEE121$Q75,    propertiesEE122$Q75,
              propertiesEE121$cent,   propertiesEE122$cent, pos3, 
              myFiles[121], timeLabels12)

dev.off()