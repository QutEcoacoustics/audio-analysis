#setwd("C:\\Work\\CSV files\\Eastern Eucalypt")
setwd("C:\\Work\\CSV files\\Wet Eucalypt")

myFiles <- list.files(full.names=TRUE,pattern="*.csv")
length<-length(myFiles)

for(i in 1:length) {
  assign(paste("propertiesEE",i, sep=""),read.csv(myFiles[c(i)]))
}

spec.properties <- NULL
for(i in 1:length) {
  assign(paste("properties"),read.csv(myFiles[c(i)]))
  spec.properties <- rbind(spec.properties, properties)
}

plot(spec.properties$Q75)
plot(spec.properties$Q25)
plot(spec.properties$temporalEntropy)
plot(spec.properties$mean)
plot(spec.properties$IQR)
plot(spec.properties$acousticCompIndex)
plot(spec.properties$skewness) 
plot(spec.properties$sfm) #spectral flatness
plot(spec.properties$zeroCrossingRate) 
plot(spec.properties$sh) # spectral entropy
plot(spec.properties$sd) # standard deviation

########### PLOTTING VARIABLES ##############
positions<-c(8,68,128,188,248,308,368)
timeLabels1<-c("1:45 pm","2:45","3.45","4.45","5.45","6.45","7.45")
timeLabels2<-c("7:45 pm","8:45","9:45","10:45","11:45","12:45","1:45")
timeLabels3<-c("1:45 am","2:45","3.45","4.45","5.45","6.45","7.45")
timeLabels4<-c("7:45 am","8:45","9:45","10:45","11:45","12:45","1:45")

par(mfrow=c(1,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
pos1 <- 0.85
pos2 <- 0.515
pos3 <- 0.185

plotQuartile <- function (data1, data2, data3, data4, data5, data6, data7, 
                          data8, data9, data10, data11, data12, data13, data14,
                          data15, data16, data17, data18, data19, data20, data21,
                          data22, data23, data24, pos, fileName, timeLab)
{
  plot(c(0:359),c(data1, data2, data3, data4, data5, data6), 
       ylab="Frequency (Hz)", ylim=c(0,7500), xaxt='n',
       xlab="", mgp = c(1.5, 0.5, 0), cex=1.0)
  par(new=TRUE)
  plot(c(0:359),c(data7, data8, data9, data10, data11, data12), 
       ylab="", ylim=c(0,7500), xlab="",xaxt='n', yaxt='n', col="red")
  par(new=TRUE)
  plot(c(0:359), c(data13, data14, data15, data16, data17, data18), 
       ylab="", ylim=c(0,7500), xlab="", xaxt='n', yaxt='n', col="blue")
  par(new=TRUE)
  plot(c(0:359), c(data19, data20, data21, data22, data23, data24), 
       ylab="", ylim=c(0,7500), xlab="", xaxt='n', yaxt='n', col="pink")
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
  "EESixHourPeriod1.png",
  width     = 9.75,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1.5, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE3$Q25,    propertiesEE4$Q25,    propertiesEE5$Q25,    propertiesEE6$Q25,    propertiesEE7$Q25,    propertiesEE8$Q25,
              propertiesEE3$median, propertiesEE4$median, propertiesEE5$median, propertiesEE6$median, propertiesEE7$median, propertiesEE8$median,
              propertiesEE3$Q75,    propertiesEE4$Q75,    propertiesEE5$Q75,    propertiesEE6$Q75,    propertiesEE7$Q75,    propertiesEE8$Q75,
              propertiesEE3$cent,   propertiesEE4$cent,   propertiesEE5$cent,   propertiesEE6$cent,   propertiesEE7$cent,   propertiesEE8$cent,
              pos1, myFiles[3], timeLabels1)
par(new=TRUE)
plotQuartile (propertiesEE27$Q25,    propertiesEE28$Q25,    propertiesEE29$Q25,    propertiesEE30$Q25,    propertiesEE31$Q25,    propertiesEE32$Q25,
              propertiesEE27$median, propertiesEE28$median, propertiesEE29$median, propertiesEE30$median, propertiesEE31$median, propertiesEE32$median,
              propertiesEE27$Q75,    propertiesEE28$Q75,    propertiesEE29$Q75,    propertiesEE30$Q75,    propertiesEE31$Q75,    propertiesEE32$Q75,
              propertiesEE27$cent,   propertiesEE28$cent,   propertiesEE29$cent,   propertiesEE30$cent,   propertiesEE31$cent,   propertiesEE32$cent,
              pos1, myFiles[27], timeLabels1)
par(new=TRUE)
plotQuartile (propertiesEE51$Q25,    propertiesEE52$Q25,    propertiesEE53$Q25,    propertiesEE54$Q25,    propertiesEE55$Q25,    propertiesEE56$Q25,
              propertiesEE51$median, propertiesEE52$median, propertiesEE53$median, propertiesEE54$median, propertiesEE55$median, propertiesEE56$median,
              propertiesEE51$Q75,    propertiesEE52$Q75,    propertiesEE53$Q75,    propertiesEE54$Q75,    propertiesEE55$Q75,    propertiesEE56$Q75,
              propertiesEE51$cent,   propertiesEE52$cent,   propertiesEE53$cent,   propertiesEE54$cent,   propertiesEE55$cent,   propertiesEE56$cent,
              pos1, myFiles[51], timeLabels1)
par(new=TRUE)
plotQuartile (propertiesEE75$Q25,    propertiesEE76$Q25,    propertiesEE77$Q25,    propertiesEE78$Q25,    propertiesEE79$Q25,    propertiesEE80$Q25,
              propertiesEE75$median, propertiesEE76$median, propertiesEE77$median, propertiesEE78$median, propertiesEE79$median, propertiesEE80$median,
              propertiesEE75$Q75,    propertiesEE76$Q75,    propertiesEE77$Q75,    propertiesEE78$Q75,    propertiesEE79$Q75,    propertiesEE80$Q75,
              propertiesEE75$cent,   propertiesEE76$cent,   propertiesEE77$cent,   propertiesEE78$cent,   propertiesEE79$cent,   propertiesEE80$cent,
              pos1, myFiles[75], timeLabels1)
par(new=TRUE)
plotQuartile (propertiesEE99$Q25,    propertiesEE100$Q25,    propertiesEE101$Q25,    propertiesEE102$Q25,    propertiesEE103$Q25,    propertiesEE104$Q25,
              propertiesEE99$median, propertiesEE100$median, propertiesEE101$median, propertiesEE102$median, propertiesEE103$median, propertiesEE104$median,
              propertiesEE99$Q75,    propertiesEE100$Q75,    propertiesEE101$Q75,    propertiesEE102$Q75,    propertiesEE103$Q75,    propertiesEE104$Q75,
              propertiesEE97$cent,   propertiesEE100$cent,   propertiesEE101$cent,   propertiesEE102$cent,   propertiesEE103$cent,   propertiesEE104$cent,
              pos1, myFiles[99], timeLabels1)
dev.off()


png(
  "EESixHourPeriod2.png",
  width     = 9.75,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1.5, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE9$Q25,    propertiesEE10$Q25,    propertiesEE11$Q25,    propertiesEE12$Q25,    propertiesEE13$Q25,    propertiesEE14$Q25,
              propertiesEE9$median, propertiesEE10$median, propertiesEE11$median, propertiesEE12$median, propertiesEE13$median, propertiesEE14$median,
              propertiesEE9$Q75,    propertiesEE10$Q75,    propertiesEE11$Q75,    propertiesEE12$Q75,    propertiesEE13$Q75,    propertiesEE14$Q75,
              propertiesEE9$cent,   propertiesEE10$cent,   propertiesEE11$cent,   propertiesEE12$cent,   propertiesEE13$cent,   propertiesEE14$cent,
              pos1, myFiles[9], timeLabels2)
par(new=TRUE)
plotQuartile (propertiesEE33$Q25,    propertiesEE34$Q25,    propertiesEE35$Q25,    propertiesEE36$Q25,    propertiesEE37$Q25,    propertiesEE38$Q25,
              propertiesEE33$median, propertiesEE34$median, propertiesEE35$median, propertiesEE36$median, propertiesEE37$median, propertiesEE38$median,
              propertiesEE33$Q75,    propertiesEE34$Q75,    propertiesEE35$Q75,    propertiesEE36$Q75,    propertiesEE37$Q75,    propertiesEE38$Q75,
              propertiesEE33$cent,   propertiesEE34$cent,   propertiesEE35$cent,   propertiesEE36$cent,   propertiesEE37$cent,   propertiesEE38$cent,
              pos1, myFiles[33], timeLabels2)
par(new=TRUE)
plotQuartile (propertiesEE57$Q25,    propertiesEE58$Q25,    propertiesEE59$Q25,    propertiesEE60$Q25,    propertiesEE61$Q25,    propertiesEE62$Q25,
              propertiesEE57$median, propertiesEE58$median, propertiesEE59$median, propertiesEE60$median, propertiesEE61$median, propertiesEE62$median,
              propertiesEE57$Q75,    propertiesEE58$Q75,    propertiesEE59$Q75,    propertiesEE60$Q75,    propertiesEE61$Q75,    propertiesEE62$Q75,
              propertiesEE57$cent,   propertiesEE58$cent,   propertiesEE59$cent,   propertiesEE60$cent,   propertiesEE61$cent,   propertiesEE62$cent,
              pos1, myFiles[57], timeLabels2)
par(new=TRUE)
plotQuartile (propertiesEE81$Q25,    propertiesEE82$Q25,    propertiesEE83$Q25,    propertiesEE84$Q25,    propertiesEE85$Q25,    propertiesEE86$Q25,
              propertiesEE81$median, propertiesEE82$median, propertiesEE83$median, propertiesEE84$median, propertiesEE85$median, propertiesEE86$median,
              propertiesEE81$Q75,    propertiesEE82$Q75,    propertiesEE83$Q75,    propertiesEE84$Q75,    propertiesEE85$Q75,    propertiesEE86$Q75,
              propertiesEE81$cent,   propertiesEE82$cent,   propertiesEE83$cent,   propertiesEE84$cent,   propertiesEE85$cent,   propertiesEE86$cent,
              pos1, myFiles[81], timeLabels2)
par(new=TRUE)
plotQuartile (propertiesEE105$Q25,    propertiesEE106$Q25,    propertiesEE107$Q25,    propertiesEE108$Q25,    propertiesEE109$Q25,    propertiesEE110$Q25,
              propertiesEE105$median, propertiesEE106$median, propertiesEE107$median, propertiesEE108$median, propertiesEE109$median, propertiesEE110$median,
              propertiesEE105$Q75,    propertiesEE106$Q75,    propertiesEE107$Q75,    propertiesEE108$Q75,    propertiesEE109$Q75,    propertiesEE110$Q75,
              propertiesEE105$cent,   propertiesEE106$cent,   propertiesEE107$cent,   propertiesEE108$cent,   propertiesEE109$cent,   propertiesEE110$cent,
              pos1, myFiles[105], timeLabels2)
dev.off()


png(
  "EESixHourPeriod3.png",
  width     = 9.75,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1.5, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE15$Q25,    propertiesEE16$Q25,    propertiesEE17$Q25,    propertiesEE18$Q25,    propertiesEE19$Q25,    propertiesEE20$Q25,
              propertiesEE15$median, propertiesEE16$median, propertiesEE17$median, propertiesEE18$median, propertiesEE19$median, propertiesEE20$median,
              propertiesEE15$Q75,    propertiesEE16$Q75,    propertiesEE17$Q75,    propertiesEE18$Q75,    propertiesEE19$Q75,    propertiesEE20$Q75,
              propertiesEE15$cent,   propertiesEE16$cent,   propertiesEE17$cent,   propertiesEE18$cent,   propertiesEE19$cent,   propertiesEE20$cent,
              pos1, myFiles[15], timeLabels3)
par(new=TRUE)
plotQuartile (propertiesEE39$Q25,    propertiesEE40$Q25,    propertiesEE41$Q25,    propertiesEE42$Q25,    propertiesEE43$Q25,    propertiesEE44$Q25,
              propertiesEE39$median, propertiesEE40$median, propertiesEE41$median, propertiesEE42$median, propertiesEE43$median, propertiesEE44$median,
              propertiesEE39$Q75,    propertiesEE40$Q75,    propertiesEE41$Q75,    propertiesEE42$Q75,    propertiesEE43$Q75,    propertiesEE44$Q75,
              propertiesEE39$cent,   propertiesEE40$cent,   propertiesEE41$cent,   propertiesEE42$cent,   propertiesEE43$cent,   propertiesEE44$cent,
              pos1, myFiles[39], timeLabels3)
par(new=TRUE)
plotQuartile (propertiesEE63$Q25,    propertiesEE64$Q25,    propertiesEE65$Q25,    propertiesEE66$Q25,    propertiesEE67$Q25,    propertiesEE68$Q25,
              propertiesEE63$median, propertiesEE64$median, propertiesEE65$median, propertiesEE66$median, propertiesEE67$median, propertiesEE68$median,
              propertiesEE63$Q75,    propertiesEE64$Q75,    propertiesEE65$Q75,    propertiesEE66$Q75,    propertiesEE67$Q75,    propertiesEE68$Q75,
              propertiesEE63$cent,   propertiesEE64$cent,   propertiesEE65$cent,   propertiesEE66$cent,   propertiesEE67$cent,   propertiesEE68$cent,
              pos1, myFiles[63], timeLabels3)
par(new=TRUE)
plotQuartile (propertiesEE87$Q25,    propertiesEE88$Q25,    propertiesEE89$Q25,    propertiesEE90$Q25,    propertiesEE91$Q25,    propertiesEE92$Q25,
              propertiesEE87$median, propertiesEE88$median, propertiesEE89$median, propertiesEE90$median, propertiesEE91$median, propertiesEE92$median,
              propertiesEE87$Q75,    propertiesEE88$Q75,    propertiesEE89$Q75,    propertiesEE90$Q75,    propertiesEE91$Q75,    propertiesEE92$Q75,
              propertiesEE87$cent,   propertiesEE88$cent,   propertiesEE89$cent,   propertiesEE90$cent,   propertiesEE91$cent,   propertiesEE92$cent,
              pos1, myFiles[87], timeLabels3)
par(new=TRUE)
plotQuartile (propertiesEE111$Q25,    propertiesEE112$Q25,    propertiesEE113$Q25,    propertiesEE114Q25,     propertiesEE115$Q25,    propertiesEE116$Q25,
              propertiesEE111$median, propertiesEE112$median, propertiesEE113$median, propertiesEE114$median, propertiesEE115$median, propertiesEE116$median,
              propertiesEE111$Q75,    propertiesEE112$Q75,    propertiesEE113$Q75,    propertiesEE114$Q75,    propertiesEE115$Q75,    propertiesEE116$Q75,
              propertiesEE111$cent,   propertiesEE112$cent,   propertiesEE113$cent,   propertiesEE114$cent,   propertiesEE115$cent,   propertiesEE116$cent,
              pos1, myFiles[111], timeLabels3)
dev.off()

png(
  "EESixHourPeriod4.png",
  width     = 9.75,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1.5, lwd=0.5) # set margins, font size & line width
plotQuartile (propertiesEE21$Q25,    propertiesEE22$Q25,    propertiesEE23$Q25,    propertiesEE24$Q25,    propertiesEE25$Q25,    propertiesEE26$Q25,
              propertiesEE21$median, propertiesEE22$median, propertiesEE23$median, propertiesEE24$median, propertiesEE25$median, propertiesEE26$median,
              propertiesEE21$Q75,    propertiesEE22$Q75,    propertiesEE23$Q75,    propertiesEE24$Q75,    propertiesEE25$Q75,    propertiesEE26$Q75,
              propertiesEE21$cent,   propertiesEE22$cent,   propertiesEE23$cent,   propertiesEE24$cent,   propertiesEE25$cent,   propertiesEE26$cent,
              pos1, myFiles[21], timeLabels4)
par(new=TRUE)
plotQuartile (propertiesEE45$Q25,    propertiesEE46$Q25,    propertiesEE47$Q25,    propertiesEE48$Q25,    propertiesEE49$Q25,    propertiesEE50$Q25,
              propertiesEE45$median, propertiesEE46$median, propertiesEE47$median, propertiesEE48$median, propertiesEE49$median, propertiesEE50$median,
              propertiesEE45$Q75,    propertiesEE46$Q75,    propertiesEE47$Q75,    propertiesEE48$Q75,    propertiesEE49$Q75,    propertiesEE50$Q75,
              propertiesEE45$cent,   propertiesEE46$cent,   propertiesEE47$cent,   propertiesEE48$cent,   propertiesEE49$cent,   propertiesEE50$cent,
              pos1, myFiles[45], timeLabels4)
par(new=TRUE)
plotQuartile (propertiesEE69$Q25,    propertiesEE70$Q25,    propertiesEE71$Q25,    propertiesEE72$Q25,    propertiesEE73$Q25,    propertiesEE74$Q25,
              propertiesEE69$median, propertiesEE70$median, propertiesEE71$median, propertiesEE72$median, propertiesEE73$median, propertiesEE74$median,
              propertiesEE69$Q75,    propertiesEE70$Q75,    propertiesEE71$Q75,    propertiesEE72$Q75,    propertiesEE73$Q75,    propertiesEE74$Q75,
              propertiesEE69$cent,   propertiesEE70$cent,   propertiesEE71$cent,   propertiesEE72$cent,   propertiesEE73$cent,   propertiesEE74$cent,
              pos1, myFiles[69], timeLabels4)

plotQuartile (propertiesEE93$Q25,    propertiesEE94$Q25,    propertiesEE95$Q25,    propertiesEE96$Q25,    propertiesEE97$Q25,    propertiesEE98$Q25,
              propertiesEE93$median, propertiesEE94$median, propertiesEE95$median, propertiesEE96$median, propertiesEE97$median, propertiesEE98$median,
              propertiesEE93$Q75,    propertiesEE94$Q75,    propertiesEE95$Q75,    propertiesEE96$Q75,    propertiesEE97$Q75,    propertiesEE98$Q75,
              propertiesEE93$cent,   propertiesEE94$cent,   propertiesEE95$cent,   propertiesEE96$cent,   propertiesEE97$cent,   propertiesEE98$cent,
              pos1, myFiles[93], timeLabels4)
par(new=TRUE)
plotQuartile (propertiesEE117$Q25,    propertiesEE118$Q25,    propertiesEE119$Q25,    propertiesEE120$Q25,    propertiesEE121$Q25,    propertiesEE122$Q25,
              propertiesEE117$median, propertiesEE118$median, propertiesEE119$median, propertiesEE120$median, propertiesEE121$median, propertiesEE122$median,
              propertiesEE117$Q75,    propertiesEE118$Q75,    propertiesEE119$Q75,    propertiesEE120$Q75,    propertiesEE121$Q75,    propertiesEE122$Q75,
              propertiesEE117$cent,   propertiesEE118$cent,   propertiesEE119$cent,   propertiesEE120$cent,   propertiesEE121$cent,   propertiesEE122$cent,
              pos1, myFiles[117], timeLabels4)
dev.off()