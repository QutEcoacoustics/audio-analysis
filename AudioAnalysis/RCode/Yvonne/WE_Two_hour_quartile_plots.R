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

par(mfrow=c(1,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
pos1 <- 0.85
pos2 <- 0.515
pos3 <- 0.185

plotQuartile <- function (data1, data2, data3, data4, data5, data6, data7, 
                          data8, pos, fileName, timeLab)
{
  plot(c(0:119),c(data1, data2), 
       ylab="Frequency (Hz)", ylim=c(0,7500), xaxt='n',
       xlab="",mgp = c(1.5, 0.5, 0), cex=1.0)
  par(new=TRUE)
  plot(c(0:119),c(data3, data4), 
       ylab="", ylim=c(0,7500),
       xlab="", xaxt='n', yaxt='n', col="red")
  par(new=TRUE)
  plot(c(0:119), c(data5,data6), 
       ylab="", ylim=c(0,7500),
       xlab="", xaxt='n', yaxt='n', col="blue")
  par(new=TRUE)
  plot(c(0:119), c(data7,data8), 
       ylab="", ylim=c(0,7500),
       xlab="", xaxt='n', yaxt='n', col="pink")
  legend("topright",inset=c(0.04,0.0),
         legend = c("Q25", "Median", "Q75", "Centroid"),          
         col=c(1,2,4,"pink"), lty = 1, cex=1.0, bty="n", horiz=TRUE,
         x.intersp=0.8, text.width=c(4.2, 4.2, 4.2))
  axis(side=1,at=positions, labels=timeLab, mgp=c(1.8,0.5,0))
  mtext(side=4, fileName ,line=-1.80, font=1, cex=1, 
        outer=TRUE, at=pos)
  abline(h=c(0,2000,4000,6000), lwd=0.5,lty=2)
}

png(
  "WEperiod1.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE1$Q25, propertiesWE2$Q25,
              propertiesWE1$median, propertiesWE2$median,
              propertiesWE1$Q75, propertiesWE2$Q75,
              propertiesWE1$cent, propertiesWE2$cent,pos1, 
              myFiles[1], timeLabels1)
par(new=TRUE)
plotQuartile (propertiesWE25$Q25, propertiesWE26$Q25,
              propertiesWE25$median, propertiesWE26$median,
              propertiesWE25$Q75, propertiesWE26$Q75,
              propertiesWE25$cent, propertiesWE26$cent, pos1, 
              myFiles[25], timeLabels1)
par(new=TRUE)
plotQuartile (propertiesWE49$Q25, propertiesWE50$Q25,
              propertiesWE49$median, propertiesWE50$median,
              propertiesWE49$Q75, propertiesWE50$Q75,
              propertiesWE49$cent, propertiesWE50$cent, pos1, 
              myFiles[49], timeLabels1)
par(new=TRUE)
plotQuartile (propertiesWE73$Q25, propertiesWE74$Q25,
              propertiesWE73$median, propertiesWE74$median,
              propertiesWE73$Q75, propertiesWE74$Q75,
              propertiesWE73$cent, propertiesWE74$cent, pos1, 
              myFiles[73], timeLabels1)
par(new=TRUE)
plotQuartile (propertiesWE97$Q25, propertiesWE98$Q25,
              propertiesWE97$median, propertiesWE98$median,
              propertiesWE97$Q75, propertiesWE98$Q75,
              propertiesWE97$cent, propertiesWE98$cent, pos1, 
              myFiles[97], timeLabels1)
dev.off()


png(
  "WEperiod2.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width

plotQuartile (propertiesWE3$Q25, propertiesWE4$Q25,
              propertiesWE3$median, propertiesWE4$median,
              propertiesWE3$Q75, propertiesWE4$Q75,
              propertiesWE3$cent, propertiesWE4$cent,pos2, 
              myFiles[3], timeLabels2)
par(new=TRUE)
plotQuartile (propertiesWE27$Q25, propertiesWE28$Q25,
              propertiesWE27$median, propertiesWE28$median,
              propertiesWE27$Q75, propertiesWE28$Q75,
              propertiesWE27$cent, propertiesWE28$cent, pos2, 
              myFiles[27], timeLabels2)
par(new=TRUE)
plotQuartile (propertiesWE51$Q25, propertiesWE52$Q25,
              propertiesWE51$median, propertiesWE52$median,
              propertiesWE51$Q75, propertiesWE52$Q75,
              propertiesWE51$cent, propertiesWE52$cent, pos2, 
              myFiles[51], timeLabels2)
par(new=TRUE)
plotQuartile (propertiesWE75$Q25, propertiesWE76$Q25,
              propertiesWE75$median, propertiesWE76$median,
              propertiesWE75$Q75, propertiesWE76$Q75,
              propertiesWE75$cent, propertiesWE76$cent, pos2, 
              myFiles[75], timeLabels2)
par(new=TRUE)
plotQuartile (propertiesWE99$Q25, propertiesWE100$Q25,
              propertiesWE99$median, propertiesWE100$median,
              propertiesWE99$Q75, propertiesWE100$Q75,
              propertiesWE99$cent, propertiesWE100$cent, pos2, 
              myFiles[99], timeLabels2)
dev.off()

png(
  "WEperiod3.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width

plotQuartile (propertiesWE5$Q25, propertiesWE6$Q25,
              propertiesWE5$median, propertiesWE6$median,
              propertiesWE5$Q75, propertiesWE6$Q75,
              propertiesWE5$cent, propertiesWE6$cent,pos3, 
              myFiles[5], timeLabels3)
par(new=TRUE)
plotQuartile (propertiesWE29$Q25, propertiesWE30$Q25,
              propertiesWE29$median, propertiesWE30$median,
              propertiesWE29$Q75, propertiesWE30$Q75,
              propertiesWE29$cent, propertiesWE30$cent, pos3, 
              myFiles[29], timeLabels3)
par(new=TRUE)
plotQuartile (propertiesWE53$Q25, propertiesWE54$Q25,
              propertiesWE53$median, propertiesWE54$median,
              propertiesWE53$Q75, propertiesWE54$Q75,
              propertiesWE53$cent, propertiesWE54$cent, pos3, 
              myFiles[53], timeLabels3)
par(new=TRUE)
plotQuartile (propertiesWE77$Q25, propertiesWE78$Q25,
              propertiesWE77$median, propertiesWE78$median,
              propertiesWE77$Q75,propertiesWE78$Q75,
              propertiesWE77$cent, propertiesWE78$cent, pos3, 
              myFiles[77], timeLabels3)
par(new=TRUE)
plotQuartile (propertiesWE101$Q25, propertiesWE102$Q25,
              propertiesWE101$median, propertiesWE102$median,
              propertiesWE101$Q75, propertiesWE102$Q75,
              propertiesWE101$cent, propertiesWE102$cent, pos3, 
              myFiles[101], timeLabels3)
dev.off()

png(
  "WEperiod4.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE7$Q25, propertiesWE8$Q25,
              propertiesWE7$median, propertiesWE8$median,
              propertiesWE7$Q75, propertiesWE8$Q75,
              propertiesWE7$cent, propertiesWE8$cent, pos1, 
              myFiles[7], timeLabels4)
par(new=TRUE)
plotQuartile (propertiesWE31$Q25, propertiesWE32$Q25,
              propertiesWE31$median, propertiesWE32$median,
              propertiesWE31$Q75, propertiesWE32$Q75,
              propertiesWE31$cent, propertiesWE32$cent, pos1, 
              myFiles[31], timeLabels4)
par(new=TRUE)
plotQuartile (propertiesWE55$Q25, propertiesWE56$Q25,
              propertiesWE55$median, propertiesWE56$median,
              propertiesWE55$Q75, propertiesWE56$Q75,
              propertiesWE55$cent, propertiesWE56$cent, pos1, 
              myFiles[55], timeLabels4)
par(new=TRUE)
plotQuartile (propertiesWE79$Q25, propertiesWE80$Q25,
              propertiesWE79$median, propertiesWE80$median,
              propertiesWE79$Q75, propertiesWE80$Q75,
              propertiesWE79$cent, propertiesWE80$cent, pos1, 
              myFiles[79], timeLabels4)
par(new=TRUE)
plotQuartile (propertiesWE103$Q25, propertiesWE104$Q25,
              propertiesWE103$median, propertiesWE104$median,
              propertiesWE103$Q75, propertiesWE104$Q75,
              propertiesWE103$cent, propertiesWE104$cent, pos1, 
              myFiles[103], timeLabels4)
dev.off()

png(
  "WEperiod5.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width

plotQuartile (propertiesWE9$Q25, propertiesWE10$Q25,
              propertiesWE9$median, propertiesWE10$median,
              propertiesWE9$Q75, propertiesWE10$Q75,
              propertiesWE9$cent, propertiesWE10$cent,pos2, 
              myFiles[9], timeLabels5)
par(new=TRUE)
plotQuartile (propertiesWE33$Q25, propertiesWE34$Q25,
              propertiesWE33$median, propertiesWE34$median,
              propertiesWE33$Q75, propertiesWE34$Q75,
              propertiesWE33$cent, propertiesWE34$cent, pos2, 
              myFiles[33], timeLabels5)
par(new=TRUE)
plotQuartile (propertiesWE57$Q25, propertiesWE58$Q25,
              propertiesWE57$median, propertiesWE58$median,
              propertiesWE57$Q75, propertiesWE58$Q75,
              propertiesWE57$cent, propertiesWE58$cent, pos2, 
              myFiles[57], timeLabels5)
par(new=TRUE)
plotQuartile (propertiesWE81$Q25, propertiesWE82$Q25,
              propertiesWE81$median, propertiesWE82$median,
              propertiesWE81$Q75, propertiesWE82$Q75,
              propertiesWE81$cent, propertiesWE82$cent, pos2, 
              myFiles[81], timeLabels5)
par(new=TRUE)
plotQuartile (propertiesWE105$Q25, propertiesWE106$Q25,
              propertiesWE105$median, propertiesWE106$median,
              propertiesWE105$Q75, propertiesWE106$Q75,
              propertiesWE105$cent, propertiesWE106$cent, pos2, 
              myFiles[105], timeLabels5)
dev.off()

png(
  "WEperiod6.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE11$Q25, propertiesWE12$Q25,
              propertiesWE11$median, propertiesWE12$median,
              propertiesWE11$Q75, propertiesWE12$Q75,
              propertiesWE11$cent, propertiesWE12$cent,pos3, 
              myFiles[11], timeLabels6)
par(new=TRUE)
plotQuartile (propertiesWE35$Q25, propertiesWE36$Q25,
              propertiesWE35$median, propertiesWE36$median,
              propertiesWE35$Q75, propertiesWE36$Q75,
              propertiesWE35$cent,propertiesWE36$cent,pos3, 
              myFiles[35], timeLabels6)
par(new=TRUE)
plotQuartile (propertiesWE59$Q25, propertiesWE60$Q25,
              propertiesWE59$median, propertiesWE60$median,
              propertiesWE59$Q75, propertiesWE60$Q75,
              propertiesWE59$cent, propertiesWE60$cent, pos3, 
              myFiles[59], timeLabels6)
par(new=TRUE)
plotQuartile (propertiesWE83$Q25, propertiesWE84$Q25,
              propertiesWE83$median, propertiesWE84$median,
              propertiesWE83$Q75, propertiesWE84$Q75,
              propertiesWE83$cent, propertiesWE84$cent, pos3, 
              myFiles[83], timeLabels6)
par(new=TRUE)
plotQuartile (propertiesWE107$Q25, propertiesWE108$Q25,
              propertiesWE107$median, propertiesWE108$median,
              propertiesWE107$Q75, propertiesWE108$Q75,
              propertiesWE107$cent, propertiesWE108$cent, pos3, 
              myFiles[107], timeLabels6)
dev.off()

png(
  "WEperiod7.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE13$Q25, propertiesWE14$Q25,
              propertiesWE13$median, propertiesWE14$median,
              propertiesWE13$Q75, propertiesWE14$Q75,
              propertiesWE13$cent, propertiesWE14$cent, pos1, 
              myFiles[13], timeLabels7)
par(new=TRUE)
plotQuartile (propertiesWE37$Q25, propertiesWE38$Q25,
              propertiesWE37$median, propertiesWE38$median,
              propertiesWE37$Q75, propertiesWE38$Q75,
              propertiesWE37$cent, propertiesWE38$cent, pos1, 
              myFiles[37], timeLabels7)
par(new=TRUE)
plotQuartile (propertiesWE61$Q25, propertiesWE62$Q25,
              propertiesWE61$median, propertiesWE62$median,
              propertiesWE61$Q75, propertiesWE62$Q75,
              propertiesWE61$cent, propertiesWE62$cent, pos1, 
              myFiles[61], timeLabels7)
par(new=TRUE)
plotQuartile (propertiesWE85$Q25, propertiesWE86$Q25,
              propertiesWE85$median, propertiesWE86$median,
              propertiesWE85$Q75, propertiesWE86$Q75,
              propertiesWE85$cent, propertiesWE86$cent, pos1, 
              myFiles[85], timeLabels7)
par(new=TRUE)
plotQuartile (propertiesWE109$Q25, propertiesWE110$Q25,
              propertiesWE109$median, propertiesWE110$median,
              propertiesWE109$Q75, propertiesWE110$Q75,
              propertiesWE109$cent, propertiesWE110$cent, pos1, 
              myFiles[109], timeLabels7)
dev.off()

png(
  "WEperiod8.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE15$Q25, propertiesWE16$Q25,
              propertiesWE15$median, propertiesWE16$median,
              propertiesWE15$Q75, propertiesWE16$Q75,
              propertiesWE15$cent, propertiesWE16$cent, pos2, 
              myFiles[15], timeLabels8)
par(new=TRUE)
plotQuartile (propertiesWE39$Q25, propertiesWE40$Q25,
              propertiesWE39$median, propertiesWE40$median,
              propertiesWE39$Q75, propertiesWE40$Q75,
              propertiesWE39$cent, propertiesWE40$cent, pos2, 
              myFiles[39], timeLabels8)
par(new=TRUE)
plotQuartile (propertiesWE63$Q25, propertiesWE64$Q25,
              propertiesWE63$median, propertiesWE64$median,
              propertiesWE63$Q75, propertiesWE64$Q75,
              propertiesWE63$cent, propertiesWE64$cent, pos2, 
              myFiles[63], timeLabels8)
par(new=TRUE)
plotQuartile (propertiesWE87$Q25, propertiesWE88$Q25,
              propertiesWE87$median, propertiesWE88$median,
              propertiesWE87$Q75, propertiesWE88$Q75,
              propertiesWE87$cent, propertiesWE88$cent, pos2, 
              myFiles[87], timeLabels8)
par(new=TRUE)
plotQuartile (propertiesWE111$Q25, propertiesWE112$Q25,
              propertiesWE111$median, propertiesWE112$median,
              propertiesWE111$Q75, propertiesWE112$Q75,
              propertiesWE111$cent, propertiesWE112$cent, pos2, 
              myFiles[111], timeLabels8)
dev.off()

png(
  "WEperiod9.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE17$Q25, propertiesWE18$Q25,
              propertiesWE17$median, propertiesWE18$median,
              propertiesWE17$Q75,propertiesWE18$Q75,
              propertiesWE17$cent, propertiesWE18$cent, pos3, 
              myFiles[17], timeLabels9)
par(new=TRUE)
plotQuartile (propertiesWE41$Q25, propertiesWE42$Q25,
              propertiesWE41$median, propertiesWE42$median,
              propertiesWE41$Q75, propertiesWE42$Q75,
              propertiesWE41$cent, propertiesWE42$cent, pos3, 
              myFiles[41], timeLabels9)
par(new=TRUE)
plotQuartile (propertiesWE65$Q25, propertiesWE66$Q25,
              propertiesWE65$median, propertiesWE66$median,
              propertiesWE65$Q75, propertiesWE66$Q75,
              propertiesWE65$cent, propertiesWE66$cent, pos3, 
              myFiles[65], timeLabels9)
par(new=TRUE)
plotQuartile (propertiesWE89$Q25, propertiesWE90$Q25,
              propertiesWE89$median, propertiesWE90$median,
              propertiesWE89$Q75, propertiesWE90$Q75,
              propertiesWE89$cent, propertiesWE90$cent, pos3, 
              myFiles[89], timeLabels9)
par(new=TRUE)
plotQuartile (propertiesWE113$Q25, propertiesWE114$Q25,
              propertiesWE113$median, propertiesWE114$median,
              propertiesWE113$Q75, propertiesWE114$Q75,
              propertiesWE113$cent, propertiesWE114$cent, pos3, 
              myFiles[113], timeLabels9)
dev.off()

png(
  "WEperiod10.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE19$Q25, propertiesWE20$Q25,
              propertiesWE19$median, propertiesWE20$median,
              propertiesWE19$Q75, propertiesWE20$Q75,
              propertiesWE19$cent, propertiesWE20$cent, pos1, 
              myFiles[19], timeLabels10)
par(new=TRUE)
plotQuartile (propertiesWE43$Q25, propertiesWE44$Q25,
              propertiesWE43$median, propertiesWE44$median,
              propertiesWE43$Q75, propertiesWE44$Q75,
              propertiesWE43$cent, propertiesWE44$cent, pos1, 
              myFiles[43], timeLabels10)
par(new=TRUE)
plotQuartile (propertiesWE67$Q25, propertiesWE68$Q25,
              propertiesWE67$median, propertiesWE68$median,
              propertiesWE67$Q75, propertiesWE68$Q75,
              propertiesWE67$cent, propertiesWE68$cent, pos1, 
              myFiles[67], timeLabels10)
par(new=TRUE)
plotQuartile (propertiesWE115$Q25, propertiesWE116$Q25,
              propertiesWE115$median, propertiesWE116$median,
              propertiesWE115$Q75, propertiesWE116$Q75,
              propertiesWE115$cent, propertiesWE116$cent, pos1, 
              myFiles[115], timeLabels10)
par(new=TRUE)
plotQuartile (propertiesWE91$Q25, propertiesWE92$Q25,
              propertiesWE91$median, propertiesWE92$median,
              propertiesWE91$Q75, propertiesWE92$Q75,
              propertiesWE91$cent, propertiesWE92$cent, pos1, 
              myFiles[91], timeLabels10)
dev.off()

png(
  "WEperiod11.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE21$Q25, propertiesWE22$Q25,
              propertiesWE21$median, propertiesWE22$median,
              propertiesWE21$Q75, propertiesWE22$Q75,
              propertiesWE21$cent, propertiesWE22$cent, pos2, 
              myFiles[21], timeLabels11)
par(new=TRUE)
plotQuartile (propertiesWE45$Q25, propertiesWE46$Q25,
              propertiesWE45$median, propertiesWE46$median,
              propertiesWE45$Q75, propertiesWE46$Q75,
              propertiesWE45$cent, propertiesWE46$cent, pos2, 
              myFiles[45], timeLabels11)
par(new=TRUE)
plotQuartile (propertiesWE69$Q25, propertiesWE70$Q25,
              propertiesWE69$median, propertiesWE70$median,
              propertiesWE69$Q75, propertiesWE70$Q75,
              propertiesWE69$cent, propertiesWE70$cent, pos2, 
              myFiles[69], timeLabels11)
par(new=TRUE)
plotQuartile (propertiesWE93$Q25, propertiesWE94$Q25,
              propertiesWE93$median, propertiesWE94$median,
              propertiesWE93$Q75, propertiesWE94$Q75,
              propertiesWE93$cent, propertiesWE94$cent, pos2, 
              myFiles[93], timeLabels11)
par(new=TRUE)
plotQuartile (propertiesWE117$Q25, propertiesWE118$Q25,
              propertiesWE117$median, propertiesWE118$median,
              propertiesWE117$Q75, propertiesWE118$Q75,
              propertiesWE117$cent, propertiesWE118$cent, pos2, 
              myFiles[117], timeLabels11)
dev.off()

png(
  "WEperiod12.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesWE23$Q25, propertiesWE24$Q25,
              propertiesWE23$median, propertiesWE24$median,
              propertiesWE23$Q75, propertiesWE24$Q75,
              propertiesWE23$cent, propertiesWE24$cent, pos3, 
              myFiles[23], timeLabels12)
par(new=TRUE)
plotQuartile (propertiesWE47$Q25, propertiesWE48$Q25,
              propertiesWE47$median, propertiesWE48$median,
              propertiesWE47$Q75, propertiesWE48$Q75,
              propertiesWE47$cent, propertiesWE48$cent, pos3, 
              myFiles[47], timeLabels12)
par(new=TRUE)
plotQuartile (propertiesWE71$Q25, propertiesWE72$Q25,
              propertiesWE71$median, propertiesWE72$median,
              propertiesWE71$Q75, propertiesWE72$Q75,
              propertiesWE71$cent, propertiesWE72$cent, pos3, 
              myFiles[71], timeLabels12)
par(new=TRUE)
plotQuartile (propertiesWE95$Q25, propertiesWE96$Q25,
              propertiesWE95$median, propertiesWE96$median,
              propertiesWE95$Q75, propertiesWE96$Q75,
              propertiesWE95$cent,propertiesWE96$cent,pos3, 
              myFiles[95], timeLabels12)
par(new=TRUE)
plotQuartile (propertiesWE119$Q25, propertiesWE120$Q25,
              propertiesWE119$median, propertiesWE120$median,
              propertiesWE119$Q75, propertiesWE120$Q75,
              propertiesWE119$cent, propertiesWE120$cent, pos3, 
              myFiles[119], timeLabels12)
dev.off()