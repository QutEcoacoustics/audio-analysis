setwd("C:\\Work\\CSV files\\Wet Eucalypt")

myFiles <- list.files(full.names=TRUE,pattern="*.csv")
length<-length(myFiles)


for(i in 1:length) {
        assign(paste("propertiesWE",i, sep=""),read.csv(myFiles[i]))
}

########### PLOTTING ##############
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
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

###### Q25 GRAPH SET 1#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE1$Q25,propertiesWE2$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',xlab="",
     type="l", mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE1$median,propertiesWE2$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE1$Q75,propertiesWE2$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
par(new=TRUE)
plot(c(0:119),c(propertiesWE1$cent,propertiesWE2$cent), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="pink")
axis(side=1,at=positions,labels=timeLabels1)
mtext(side=4,myFiles[1],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

############################################
plot(c(0:119),c(propertiesWE3$Q25,propertiesWE4$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l", mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE3$median,propertiesWE4$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE3$Q75,propertiesWE4$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
par(new=TRUE)
plot(c(0:119),c(propertiesWE3$cent,propertiesWE4$cent), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="pink")
axis(side=1,at=positions,labels=timeLabels2)
mtext(side=4,myFiles[3],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
#############################################
plot(c(0:119),c(propertiesWE5$Q25,propertiesWE6$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',xlab="",
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=32)
par(new=TRUE)
plot(c(0:119),c(propertiesWE5$median,propertiesWE6$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE5$Q75,propertiesWE6$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
par(new=TRUE)
plot(c(0:119),c(propertiesWE5$cent,propertiesWE6$cent), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="pink")
axis(side=1,at=positions,labels=timeLabels3)
mtext(side=4,myFiles[5],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2,4.2))

###### Q25 GRAPH SET 2#################################
par(mfrow=c(3,1)) # set layout

plot(c(0:119),c(propertiesWE7$Q25,propertiesWE8$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE7$median,propertiesWE8$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE7$Q75,propertiesWE8$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels4)
mtext(side=4,myFiles[7],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE9$Q25,propertiesWE10$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE9$median,propertiesWE10$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE9$Q75,propertiesWE10$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels5)
mtext(side=4,myFiles[9],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE11$Q25,propertiesWE12$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE11$median,propertiesWE12$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE11$Q75,propertiesWE12$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels6)
mtext(side=4,myFiles[11],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 3#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE13$Q25,propertiesWE14$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE13$median,propertiesWE14$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE13$Q75,propertiesWE14$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels7)
mtext(side=4,myFiles[13],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE15$Q25,propertiesWE16$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE15$median,propertiesWE16$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE15$Q75,propertiesWE16$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels8)
mtext(side=4,myFiles[15],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE17$Q25,propertiesWE18$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=16)
par(new=TRUE)
plot(c(0:119),c(propertiesWE17$median,propertiesWE18$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE17$Q75,propertiesWE18$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels9)
mtext(side=4,myFiles[17],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 4#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE19$Q25,propertiesWE20$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE19$median,propertiesWE20$median), 
     ylab="",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE19$Q75,propertiesWE20$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels10)
mtext(side=4,myFiles[19],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

plot(c(0:119),c(propertiesWE21$Q25,propertiesWE22$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE21$median,propertiesWE22$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE21$Q75,propertiesWE22$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels11)
mtext(side=4,myFiles[21],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

plot(c(0:119),c(propertiesWE23$Q25,propertiesWE24$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE23$median,propertiesWE24$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE23$Q75,propertiesWE24$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels12)
mtext(side=4,myFiles[23],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

###### Q25 GRAPH SET 5#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE25$Q25,propertiesWE26$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',xlab="",
     type="l", mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE25$median,propertiesWE26$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE25$Q75,propertiesWE26$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels1)
mtext(side=4,myFiles[25],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

############################################
plot(c(0:119),c(propertiesWE27$Q25,propertiesWE28$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l", mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE27$median,propertiesWE28$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE27$Q75,propertiesWE28$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels2)
mtext(side=4,myFiles[27],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

#############################################
plot(c(0:119),c(propertiesWE29$Q25,propertiesWE30$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',xlab="",
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=31)
par(new=TRUE)
plot(c(0:119),c(propertiesWE29$median,propertiesWE30$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE29$Q75,propertiesWE30$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels3)
mtext(side=4,myFiles[29],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

###### Q25 GRAPH SET 6#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE31$Q25,propertiesWE32$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE31$median,propertiesWE32$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE31$Q75,propertiesWE32$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels4)
mtext(side=4,myFiles[31],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE33$Q25,propertiesWE34$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE33$median,propertiesWE34$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE33$Q75,propertiesWE34$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels5)
mtext(side=4,myFiles[33],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE35$Q25,propertiesWE36$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE35$median,propertiesWE36$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE35$Q75,propertiesWE36$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels6)
mtext(side=4,myFiles[35],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 7#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE37$Q25,propertiesWE38$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE37$median,propertiesWE38$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE37$Q75,propertiesWE38$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels7)
mtext(side=4,myFiles[37],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE39$Q25,propertiesWE40$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE39$median,propertiesWE40$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE39$Q75,propertiesWE40$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels8)
mtext(side=4,myFiles[39],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE41$Q25,propertiesWE42$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=16)
par(new=TRUE)
plot(c(0:119),c(propertiesWE41$median,propertiesWE42$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE41$Q75,propertiesWE42$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels9)
mtext(side=4,myFiles[41],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 8#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE43$Q25,propertiesWE44$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE43$median,propertiesWE44$median), 
     ylab="",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE43$Q75,propertiesWE44$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels10)
mtext(side=4,myFiles[43],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

plot(c(0:119),c(propertiesWE45$Q25,propertiesWE46$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE45$median,propertiesWE46$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE45$Q75,propertiesWE46$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels11)
mtext(side=4,myFiles[45],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

plot(c(0:119),c(propertiesWE47$Q25,propertiesWE48$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE47$median,propertiesWE48$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE47$Q75,propertiesWE48$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels12)
mtext(side=4,myFiles[47],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

###### Q25 GRAPH SET 9#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE49$Q25, propertiesWE50$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',xlab="",
     type="l", mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE49$median,propertiesWE50$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE49$Q75,propertiesWE50$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
par(new=TRUE)
plot(c(0:119),c(propertiesWE49$cent,propertiesWE50$cent), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="pink")
axis(side=1,at=positions,labels=timeLabels1)
mtext(side=4,myFiles[1],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

############################################
plot(c(0:119),c(propertiesWE51$Q25,propertiesWE52$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l", mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE51$median,propertiesWE52$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE51$Q75,propertiesWE52$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
par(new=TRUE)
plot(c(0:119),c(propertiesWE51$cent,propertiesWE52$cent), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="pink")
axis(side=1,at=positions,labels=timeLabels2)
mtext(side=4,myFiles[3],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
#############################################
plot(c(0:119),c(propertiesWE53$Q25,propertiesWE54$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',xlab="",
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=32)
par(new=TRUE)
plot(c(0:119),c(propertiesWE53$median,propertiesWE54$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE53$Q75,propertiesWE54$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
par(new=TRUE)
plot(c(0:119),c(propertiesWE53$cent,propertiesWE54$cent), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="pink")
axis(side=1,at=positions,labels=timeLabels3)
mtext(side=4,myFiles[5],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2,4.2))

###### Q25 GRAPH SET 10#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE55$Q25,propertiesWE56$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE55$median,propertiesWE56$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE55$Q75,propertiesWE56$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels4)
mtext(side=4,myFiles[7],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE57$Q25,propertiesWE58$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE57$median,propertiesWE58$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE57$Q75,propertiesWE58$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels5)
mtext(side=4,myFiles[9],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE59$Q25,propertiesWE60$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE59$median,propertiesWE60$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE59$Q75,propertiesWE60$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels6)
mtext(side=4,myFiles[11],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 11#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE61$Q25,propertiesWE62$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE61$median,propertiesWE62$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE61$Q75,propertiesWE62$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels7)
mtext(side=4,myFiles[13],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE63$Q25,propertiesWE64$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE63$median,propertiesWE64$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE63$Q75,propertiesWE64$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels8)
mtext(side=4,myFiles[15],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE65$Q25,propertiesWE66$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=16)
par(new=TRUE)
plot(c(0:119),c(propertiesWE65$median,propertiesWE66$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE65$Q75,propertiesWE66$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels9)
mtext(side=4,myFiles[17],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 12#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE67$Q25,propertiesWE68$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE67$median,propertiesWE68$median), 
     ylab="",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE67$Q75,propertiesWE68$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels10)
mtext(side=4,myFiles[19],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

plot(c(0:119),c(propertiesWE69$Q25,propertiesWE70$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE69$median,propertiesWE70$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE69$Q75,propertiesWE70$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels11)
mtext(side=4,myFiles[21],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

plot(c(0:119),c(propertiesWE71$Q25,propertiesWE72$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE71$median,propertiesWE72$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE71$Q75,propertiesWE72$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels12)
mtext(side=4,myFiles[23],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

###### Q25 GRAPH SET 13#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE73$Q25,propertiesWE74$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',xlab="",
     type="l", mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE73$median,propertiesWE74$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE73$Q75,propertiesWE74$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels1)
mtext(side=4,myFiles[25],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

############################################
plot(c(0:119),c(propertiesWE75$Q25,propertiesWE76$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l", mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE75$median,propertiesWE76$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE75$Q75,propertiesWE76$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels2)
mtext(side=4,myFiles[27],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

#############################################
plot(c(0:119),c(propertiesWE77$Q25,propertiesWE78$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',xlab="",
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=31)
par(new=TRUE)
plot(c(0:119),c(propertiesWE77$median,propertiesWE78$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE77$Q75,propertiesWE78$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels3)
mtext(side=4,myFiles[29],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

###### Q25 GRAPH SET 14#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE79$Q25,propertiesWE80$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE79$median,propertiesWE80$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE79$Q75,propertiesWE80$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels4)
mtext(side=4,myFiles[31],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE81$Q25,propertiesWE82$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE81$median,propertiesWE82$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE81$Q75,propertiesWE81$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels5)
mtext(side=4,myFiles[33],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE83$Q25,propertiesWE84$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE83$median,propertiesWE84$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE83$Q75,propertiesWE84$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels6)
mtext(side=4,myFiles[35],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 15#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE85$Q25,propertiesWE86$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     type="l",mgp = c(1.5, 0.5, 0),cex=0.9,xlab="")
par(new=TRUE)
plot(c(0:119),c(propertiesWE85$median,propertiesWE86$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE85$Q75,propertiesWE86$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels7)
mtext(side=4,myFiles[37],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE87$Q25,propertiesWE88$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE87$median,propertiesWE88$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE87$Q75,propertiesWE88$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels8)
mtext(side=4,myFiles[39],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesWE89$Q25,propertiesWE90$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=16)
par(new=TRUE)
plot(c(0:119),c(propertiesWE89$median,propertiesWE90$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE89$Q75,propertiesWE90$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels9)
mtext(side=4,myFiles[41],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 16#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1), cex=1) # set margins and font size
plot(c(0:119),c(propertiesWE91$Q25,propertiesWE92$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE91$median,propertiesWE92$median), 
     ylab="",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE91$Q75,propertiesWE92$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels10)
mtext(side=4,myFiles[43],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

plot(c(0:119),c(propertiesWE93$Q25,propertiesWE94$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE93$median,propertiesWE94$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE93$Q75,propertiesWE94$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels11)
mtext(side=4,myFiles[45],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

plot(c(0:119),c(propertiesWE95$Q25,propertiesWE96$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesWE95$median,propertiesWE96$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesWE95$Q75,propertiesWE96$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels12)
mtext(side=4,myFiles[47],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))

#########################################
