setwd("C:\\Work\\CSV files\\Wet Eucalypt")

myFiles <- list.files(full.names=TRUE,pattern="*.csv")
length<-length(myFiles)

for(i in 1:length) {
  assign(paste("propertiesWE",i, sep=""),read.csv(myFiles[i]))
}

########### PLOTTING VARIABLES ##############
positions<-c(-4,11,26,41,56,71,86,101,116)
timeLabels1<-c("1:30 pm","1:45","2:00","2:15","2:30","2:45","3:00","3:15","3:30")
timeLabels2<-c("3:30 pm","3:45","4:00","4:15","4:30","4:45","5:00","5:15","5:30")
timeLabels3<-c("5:30 pm","5:45","6:00","6:15","6:30","6:45","7:00","7:15","7:30")
timeLabels4<-c("7:30 pm","7:45","8:00","8:15","8:30","8:45","9:00","9:15","9:30")
timeLabels5<-c("9:30 pm","9:45","10:00","10:15","10:30","10:45","11:00","11:15","11:30")
timeLabels6<-c("11:30 pm","11:45","00:00","00:15","00:30","00:45","1:00","1:15","1:30")
timeLabels7<-c("1:30 am","1:45","2:00","2:15","2:30","2:45","3:00","3:15","3:30")
timeLabels8<-c("3:30 am","3:45","4:00","4:15","4:30","4:45","5:00","5:15","5:30")
timeLabels9<-c("5:30 am","5:45","6:00","6:15","6:30","6:45","7:00","7:15","7:30")
timeLabels10<-c("7:30 am","7:45","8:00","8:15","8:30","8:45","9:00","9:15","9:30")
timeLabels11<-c("9:30 am","9:45","10:00","10:15","10:30","10:45","11:00","11:15","11:30")
timeLabels12<-c("11:30 am","11:45","12:00","12:15","12:30","12:45","1:00","1:15","1:30")

par(mfrow=c(3,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
pos1 <- 0.85
pos2 <- 0.515
pos3 <- 0.185

plotQuartile <- function (data1, data2, data3, data4, data5, data6, data7, 
                          data8, pos, fileName, timeLab)
{
  plot(c(0:119),c(data1,data2), 
       ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
       xlab="",type="l",
       mgp = c(1.5, 0.5, 0),cex=1.0)
  par(new=TRUE)
  plot(c(0:119),c(data3, data4), 
       ylab="",ylim=c(0,7500),
       xlab="",type="l",xaxt='n',yaxt='n',col="red")
  par(new=TRUE)
  plot(c(0:119),c(data5,data6), 
       ylab="",ylim=c(0,7500),
       xlab="",type="l",xaxt='n',yaxt='n',col="blue")
  par(new=TRUE)
  plot(c(0:119),c(data7,data8), 
       ylab="",ylim=c(0,7500),
       xlab="",type="l",xaxt='n',yaxt='n',col="pink")
  legend("topright",inset=c(0.04,0.0),
         legend = c("   Q25  "," Median ","   Q75  ","Centroid"),          
         col=c(1,2,4,"pink"),lty = 1,cex=1.0,bty="n",horiz=TRUE,
         x.intersp=0.8,text.width=c(4.2, 4.2, 4.2))
  axis(side=1,at=positions,labels=timeLab, mgp=c(1.8,0.5,0))
  mtext(side=4, fileName ,line=-1.80, font=1, cex=1, 
        outer=TRUE,at=pos)
  abline(h=c(0,2000,4000,6000), lwd=0.5,lty=2)
}

png(
  "WE1.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesWE1$Q25,propertiesWE2$Q25,
              propertiesWE1$median,propertiesWE2$median,
              propertiesWE1$Q75,propertiesWE2$Q75,
              propertiesWE1$cent,propertiesWE2$cent,pos1, myFiles[1],
              timeLabels1)
plotQuartile (propertiesWE3$Q25,propertiesWE4$Q25,
              propertiesWE3$median,propertiesWE4$median,
              propertiesWE3$Q75,propertiesWE4$Q75,
              propertiesWE3$cent,propertiesWE4$cent,pos2, myFiles[3],
              timeLabels2)
plotQuartile (propertiesWE5$Q25,propertiesWE6$Q25,
              propertiesWE5$median,propertiesWE6$median,
              propertiesWE5$Q75,propertiesWE6$Q75,
              propertiesWE5$cent,propertiesWE6$cent,pos3, myFiles[5],
              timeLabels3)
dev.off()

png(
  "WE2.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesWE7$Q25,propertiesWE8$Q25,
              propertiesWE7$median,propertiesWE8$median,
              propertiesWE7$Q75,propertiesWE8$Q75,
              propertiesWE7$cent,propertiesWE8$cent,pos1, myFiles[7],
              timeLabels4)
plotQuartile (propertiesWE9$Q25,propertiesWE10$Q25,
              propertiesWE9$median,propertiesWE10$median,
              propertiesWE9$Q75,propertiesWE10$Q75,
              propertiesWE9$cent,propertiesWE10$cent,pos2, myFiles[9],
              timeLabels5)
plotQuartile (propertiesWE11$Q25,propertiesWE12$Q25,
              propertiesWE11$median,propertiesWE12$median,
              propertiesWE11$Q75,propertiesWE12$Q75,
              propertiesWE11$cent,propertiesWE12$cent,pos3, myFiles[11],
              timeLabels6)
dev.off()

png(
  "WE3.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesWE13$Q25,propertiesWE14$Q25,
              propertiesWE13$median,propertiesWE14$median,
              propertiesWE13$Q75,propertiesWE14$Q75,
              propertiesWE13$cent,propertiesWE14$cent, pos1, 
              myFiles[13], timeLabels7)
plotQuartile (propertiesWE15$Q25,propertiesWE16$Q25,
              propertiesWE15$median,propertiesWE16$median,
              propertiesWE15$Q75,propertiesWE16$Q75,
              propertiesWE15$cent,propertiesWE16$cent, pos2, 
              myFiles[15], timeLabels8)
plotQuartile (propertiesWE17$Q25, propertiesWE18$Q25,
              propertiesWE17$median, propertiesWE18$median,
              propertiesWE17$Q75,propertiesWE18$Q75,
              propertiesWE17$cent, propertiesWE18$cent, pos3, 
              myFiles[17], timeLabels9)
dev.off()

png(
  "WE4.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesEE19$Q25, propertiesEE20$Q25,
              propertiesEE19$median, propertiesEE20$median,
              propertiesEE19$Q75, propertiesEE20$Q75,
              propertiesEE19$cent, propertiesEE20$cent, pos1, 
              myFiles[19], timeLabels10)
plotQuartile (propertiesEE21$Q25, propertiesEE22$Q25,
              propertiesEE21$median, propertiesEE22$median,
              propertiesEE21$Q75, propertiesEE22$Q75,
              propertiesEE21$cent, propertiesEE22$cent, pos2, 
              myFiles[21], timeLabels11)
plotQuartile (propertiesEE23$Q25, propertiesEE24$Q25,
              propertiesEE23$median, propertiesEE24$median,
              propertiesEE23$Q75, propertiesEE24$Q75,
              propertiesEE23$cent, propertiesEE24$cent, pos3, 
              myFiles[23], timeLabels12)
dev.off()

png(
  "WE5.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesEE25$Q25, propertiesEE26$Q25,
              propertiesEE25$median, propertiesEE26$median,
              propertiesEE25$Q75, propertiesEE26$Q75,
              propertiesEE25$cent, propertiesEE26$cent, pos1, 
              myFiles[25], timeLabels1)
plotQuartile (propertiesEE27$Q25, propertiesEE28$Q25,
              propertiesEE27$median, propertiesEE28$median,
              propertiesEE27$Q75, propertiesEE28$Q75,
              propertiesEE27$cent, propertiesEE28$cent, pos2, 
              myFiles[27], timeLabels2)
plotQuartile (propertiesEE29$Q25, propertiesEE30$Q25,
              propertiesEE29$median, propertiesEE30$median,
              propertiesEE29$Q75, propertiesEE30$Q75,
              propertiesEE29$cent, propertiesEE30$cent, pos3, 
              myFiles[29], timeLabels3)
dev.off()

png(
  "WE6.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesWE31$Q25, propertiesWE32$Q25,
              propertiesWE31$median, propertiesWE32$median,
              propertiesWE31$Q75, propertiesWE32$Q75,
              propertiesWE31$cent, propertiesWE32$cent, pos1, 
              myFiles[31], timeLabels4)
plotQuartile (propertiesWE33$Q25, propertiesWE34$Q25,
              propertiesWE33$median, propertiesWE34$median,
              propertiesWE33$Q75, propertiesWE34$Q75,
              propertiesWE33$cent, propertiesWE34$cent, pos2, 
              myFiles[33], timeLabels5)
plotQuartile (propertiesWE35$Q25, propertiesWE36$Q25,
              propertiesWE35$median, propertiesWE36$median,
              propertiesWE35$Q75, propertiesWE36$Q75,
              propertiesWE35$cent,propertiesWE36$cent,pos3, 
              myFiles[35], timeLabels6)
dev.off()

png(
  "WE7.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesWE37$Q25, propertiesWE38$Q25,
              propertiesWE37$median, propertiesWE38$median,
              propertiesWE37$Q75, propertiesWE38$Q75,
              propertiesWE37$cent, propertiesWE38$cent, pos1, 
              myFiles[37], timeLabels7)
plotQuartile (propertiesWE39$Q25, propertiesWE40$Q25,
              propertiesWE39$median, propertiesWE40$median,
              propertiesWE39$Q75, propertiesWE40$Q75,
              propertiesWE39$cent, propertiesWE40$cent, pos2, 
              myFiles[39], timeLabels8)
plotQuartile (propertiesWE41$Q25, propertiesWE42$Q25,
              propertiesWE41$median, propertiesWE42$median,
              propertiesWE41$Q75, propertiesWE42$Q75,
              propertiesWE41$cent, propertiesWE42$cent, pos3, 
              myFiles[41], timeLabels9)
dev.off()

png(
  "WE8.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesWE43$Q25, propertiesWE44$Q25,
              propertiesWE43$median, propertiesWE44$median,
              propertiesWE43$Q75, propertiesWE44$Q75,
              propertiesWE43$cent, propertiesWE44$cent, pos1, 
              myFiles[43], timeLabels10)
plotQuartile (propertiesWE45$Q25, propertiesWE46$Q25,
              propertiesWE45$median, propertiesWE46$median,
              propertiesWE45$Q75, propertiesWE46$Q75,
              propertiesWE45$cent, propertiesWE46$cent, pos2, 
              myFiles[45], timeLabels11)
plotQuartile (propertiesWE47$Q25, propertiesWE48$Q25,
              propertiesWE47$median, propertiesWE48$median,
              propertiesWE47$Q75, propertiesWE48$Q75,
              propertiesWE47$cent, propertiesWE48$cent, pos3, 
              myFiles[47], timeLabels12)
dev.off()

png(
  "WE9.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesEE49$Q25, propertiesEE50$Q25,
              propertiesEE49$median, propertiesEE50$median,
              propertiesEE49$Q75, propertiesEE50$Q75,
              propertiesEE49$cent, propertiesEE50$cent, pos1, 
              myFiles[49], timeLabels1)
plotQuartile (propertiesEE51$Q25, propertiesEE52$Q25,
              propertiesEE51$median, propertiesEE52$median,
              propertiesEE51$Q75, propertiesEE52$Q75,
              propertiesEE51$cent, propertiesEE52$cent, pos2, 
              myFiles[51], timeLabels2)
plotQuartile (propertiesEE53$Q25, propertiesEE54$Q25,
              propertiesEE53$median, propertiesEE54$median,
              propertiesEE53$Q75, propertiesEE54$Q75,
              propertiesEE53$cent, propertiesEE54$cent, pos3, 
              myFiles[53], timeLabels3)
dev.off()

png(
  "WE10.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesEE55$Q25, propertiesEE56$Q25,
              propertiesEE55$median, propertiesEE56$median,
              propertiesEE55$Q75, propertiesEE56$Q75,
              propertiesEE55$cent, propertiesEE56$cent, pos1, 
              myFiles[55], timeLabels1)
plotQuartile (propertiesEE27$Q25, propertiesEE28$Q25,
              propertiesEE27$median, propertiesEE28$median,
              propertiesEE27$Q75, propertiesEE28$Q75,
              propertiesEE27$cent, propertiesEE28$cent, pos2, 
              myFiles[27], timeLabels2)
plotQuartile (propertiesEE29$Q25, propertiesEE30$Q25,
              propertiesEE29$median, propertiesEE30$median,
              propertiesEE29$Q75, propertiesEE30$Q75,
              propertiesEE29$cent, propertiesEE30$cent, pos3, 
              myFiles[29], timeLabels3)
dev.off()