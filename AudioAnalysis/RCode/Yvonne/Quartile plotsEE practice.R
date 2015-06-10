setwd("C:\\Work\\CSV files\\Eastern Eucalypt")

myFiles <- list.files(full.names=TRUE,pattern="*.csv")
length<-length(myFiles)

for(i in 1:length) {
  assign(paste("propertiesEE",i, sep=""),read.csv(myFiles[i]))
}

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
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=1.0,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLab, mgp=c(1.8,0.5,0))
mtext(side=4, fileName ,line=-1.80, font=1, cex=1.0,
      outer=TRUE,at=pos)
abline(h=c(0,2000,4000,6000), lwd=0.5,lty=2)
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
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesEE3$Q25, propertiesEE4$Q25,
              propertiesEE3$median, propertiesEE4$median,
              propertiesEE3$Q75, propertiesEE4$Q75,
              propertiesEE3$cent, propertiesEE4$cent, pos1, 
              myFiles[3], timeLabels1)

plotQuartile (propertiesEE5$Q25, propertiesEE6$Q25,
              propertiesEE5$median, propertiesEE6$median,
              propertiesEE5$Q75, propertiesEE6$Q75,
              propertiesEE5$cent, propertiesEE6$cent,pos2, myFiles[5],
              timeLabels2)
plotQuartile (propertiesEE7$Q25, propertiesEE8$Q25,
              propertiesEE7$median, propertiesEE8$median,
              propertiesEE7$Q75, propertiesEE8$Q75,
              propertiesEE7$cent, propertiesEE8$cent,pos3, myFiles[7],
              timeLabels3)
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
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesEE9$Q25, propertiesEE10$Q25,
              propertiesEE9$median, propertiesEE10$median,
              propertiesEE9$Q75,propertiesEE10$Q75,
              propertiesEE9$cent, propertiesEE10$cent,pos1, myFiles[9],
              timeLabels4)
plotQuartile (propertiesEE11$Q25, propertiesEE12$Q25,
              propertiesEE11$median, propertiesEE12$median,
              propertiesEE11$Q75, propertiesEE12$Q75,
              propertiesEE11$cent, propertiesEE12$cent,pos2, myFiles[11],
              timeLabels5)
plotQuartile (propertiesEE13$Q25, propertiesEE14$Q25,
              propertiesEE13$median, propertiesEE14$median,
              propertiesEE13$Q75, propertiesEE14$Q75,
              propertiesEE13$cent, propertiesEE14$cent,pos3, myFiles[13],
              timeLabels6)
dev.off()

png(
  "EE3.png",
  width     = 3.25,
  height    = 3.25,
  units     = "in",
  res       = 1200,
  pointsize = 4
)
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
plotQuartile (propertiesEE15$Q25, propertiesEE16$Q25,
              propertiesEE15$median, propertiesEE16$median,
              propertiesEE15$Q75, propertiesEE16$Q75,
              propertiesEE15$cent, propertiesEE16$cent,pos1, myFiles[15],
              timeLabels7)
plotQuartile (propertiesEE17$Q25, propertiesEE18$Q25,
              propertiesEE17$median, propertiesEE18$median,
              propertiesEE17$Q75, propertiesEE18$Q75,
              propertiesEE17$cent, propertiesEE18$cent,pos2, myFiles[17],
              timeLabels8)
plotQuartile (propertiesEE19$Q25, propertiesEE20$Q25,
              propertiesEE19$median, propertiesEE20$median,
              propertiesEE19$Q75, propertiesEE20$Q75,
              propertiesEE19$cent, propertiesEE20$cent, pos3, myFiles[19],
              timeLabels9)
dev.off()
