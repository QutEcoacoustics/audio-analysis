setwd("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\Yvonne\\Eastern Eucalypt")

myFiles <- list.files(full.names=TRUE,pattern="*.csv")
length<-length(myFiles)


for(i in 1:length) {
        assign(paste("propertiesEE",i, sep=""),read.csv(myFiles[i]))
}

########### PLOTTING ##############
par(mfrow=c(1,1)) # set layout
par(mar=c(4.3, 4.6, 0.7, 2.1)) # set margins
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

###### Q25 GRAPH SET 1#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
#plot(c(0:119),c(propertiesEE1$Q25,propertiesEE2$Q25), 
 #    ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
 #    xlab="11.37 am to 1.37pm 22 March 2015",type="l",
 #    mgp = c(1.5, 0.5, 0),cex=0.9)
#par(new=TRUE)
#plot(c(0:119),c(propertiesEE1$median,propertiesEE2$median), 
 #    ylab="",ylim=c(0,7500),
 #    xlab="",type="l",xaxt='n',yaxt='n',col="red")
#par(new=TRUE)
#plot(c(0:119),c(propertiesEE1$Q75,propertiesEE2$Q75), 
#     ylab="",ylim=c(0,7500),
#     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
#legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
 #      col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
  #     x.intersp=0.8,text.width=c(4.2 ,4.2))
#axis(side=1,at=positions,labels=timeLabels12, mgp=c(1.8,0.5,0))
#mtext(side=4,myFiles[1],line=-1.8, font=1, cex=0.7, 
 #     outer=TRUE,at=0.85)
#abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
#abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)
############################################
plot(c(0:119),c(propertiesEE3$Q25,propertiesEE4$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="1:37 to 3:37 pm on 22 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE3$median,propertiesEE4$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE3$Q75,propertiesEE4$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels1, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[3],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.85)# change back to at+0.515
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2); 
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)
#############################################
plot(c(0:119),c(propertiesEE5$Q25,propertiesEE6$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="3:37 to 5:37 pm on 22 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE5$median,propertiesEE6$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE5$Q75,propertiesEE6$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels2, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[5],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)# at 0.185
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 2#################################
#par(mfrow=c(3,1)) # set layout
#par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
plot(c(0:119),c(propertiesEE7$Q25,propertiesEE8$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="5:34 to 7:34 pm on 22 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE7$median,propertiesEE8$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE7$Q75,propertiesEE8$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels3, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[7],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)# at0.85
abline(v=21)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)
#############################
plot(c(0:119),c(propertiesEE9$Q25,propertiesEE10$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="7:37 to 9:37 pm on 22 March 2015",type="l",
     mgp = c(1.8, 0.5, 0))
par(new=TRUE)
plot(c(0:119),c(propertiesEE9$median,propertiesEE10$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE9$Q75,propertiesEE10$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels4, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[9],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesEE11$Q25,propertiesEE12$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="9:34pm to 11:34  on 22 March 2015",type="l",
     mgp = c(1.8, 0.5, 0))
par(new=TRUE)
plot(c(0:119),c(propertiesEE11$median,propertiesEE12$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE11$Q75,propertiesEE12$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2 ,4.2))
axis(side=1,at=positions,labels=timeLabels5, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[11],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 3#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
plot(c(0:119),c(propertiesEE13$Q25,propertiesEE14$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="11:37 to 1:37 am on 22-23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE13$median,propertiesEE14$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE13$Q75,propertiesEE14$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels6, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[13],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesEE15$Q25,propertiesEE16$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="3:34 to 5:34 am on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE15$median,propertiesEE16$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE15$Q75,propertiesEE16$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels7, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[15],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesEE17$Q25,propertiesEE18$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="5:34 to 7:34 am on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=16)
par(new=TRUE)
plot(c(0:119),c(propertiesEE17$median,propertiesEE18$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE17$Q75,propertiesEE18$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels8, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[17],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 4#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
plot(c(0:119),c(propertiesEE19$Q25,propertiesEE20$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="7:34 to 9:34 am on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE19$median,propertiesEE20$median), 
     ylab="",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE19$Q75,propertiesEE20$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels9, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[19],line=-1.8, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesEE21$Q25,propertiesEE22$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="9:34 to 11:34 am on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE21$median,propertiesEE22$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE21$Q75,propertiesEE22$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels10, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[21],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))

plot(c(0:119),c(propertiesEE23$Q25,propertiesEE24$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="11:34 to 1:34 pm on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE23$median,propertiesEE24$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE23$Q75,propertiesEE24$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels11, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[23],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))

###### Q25 GRAPH SET 5#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
plot(c(0:119),c(propertiesEE25$Q25,propertiesEE26$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="11.37 am to 1.37pm 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE25$median,propertiesEE26$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE25$Q75,propertiesEE26$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels12, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[25],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.85)
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

############################################
plot(c(0:119),c(propertiesEE27$Q25,propertiesEE28$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="1:37 to 3:37 pm on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE27$median,propertiesEE28$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE27$Q75,propertiesEE28$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels1, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[27],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.85) #0.515
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)
############################################
plot(c(0:119),c(propertiesEE29$Q25,propertiesEE30$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="3:37 to 5:37 pm on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE29$median,propertiesEE30$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE29$Q75,propertiesEE30$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels2, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[29],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515) #0.185
abline(h=0,lwd=0.1,lty=2); abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2); abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 6#################################
#par(mfrow=c(3,1)) # set layout
#par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
plot(c(0:119),c(propertiesEE31$Q25,propertiesEE32$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="5:34 to 7:34 pm on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE31$median,propertiesEE32$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE31$Q75,propertiesEE32$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels3, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[31],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)# 0.85
abline(v=21)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)
#################
plot(c(0:119),c(propertiesEE33$Q25,propertiesEE34$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="7:37 to 9:37 pm on 15 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE33$median,propertiesEE34$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE33$Q75,propertiesEE34$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels4, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[33],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)

abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesEE35$Q25,propertiesEE36$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="9:34pm to 11:34  on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE35$median,propertiesEE36$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE35$Q75,propertiesEE36$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels5, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[35],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)

abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 7#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
plot(c(0:119),c(propertiesEE37$Q25,propertiesEE38$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="11:37 to 1:37 am on 16 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE37$median,propertiesEE38$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE37$Q75,propertiesEE38$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels6, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[37],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.85)

abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesEE39$Q25,propertiesEE40$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="3:34 to 5:34 am on 23 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE39$median,propertiesEE40$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE39$Q75,propertiesEE40$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels7, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[39],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)

abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesEE41$Q25,propertiesEE42$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="5:34 to 7:34 am on 24 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
abline(v=16)
par(new=TRUE)
plot(c(0:119),c(propertiesEE41$median,propertiesEE42$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE41$Q75,propertiesEE42$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels8, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[41],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)
abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

###### Q25 GRAPH SET 8#################################
par(mfrow=c(3,1)) # set layout
par(mar=c(1.6, 3.1, 0.4, 1.8)) # set margins
plot(c(0:119),c(propertiesEE43$Q25,propertiesEE44$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='x',
     xlab="7:34 to 9:34 am on 24 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE43$median,propertiesEE44$median), 
     ylab="",ylim=c(0,7500),xaxt='n',
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE43$Q75,propertiesEE44$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
legend("topright",inset=c(-0.0,-0.02),legend = c("Q25","Median","Q75"),          
       col=c(1,2,4),lty = 1,cex=0.9,bty="n",horiz=TRUE,
       x.intersp=0.8,text.width=c(4.2, 4.2))
axis(side=1,at=positions,labels=timeLabels9, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[43],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.85)

abline(h=0,lwd=0.1,lty=2)
abline(h=2000,lwd=0.1,lty=2)
abline(h=4000,lwd=0.1,lty=2)
abline(h=6000,lwd=0.5,lty=2)

plot(c(0:119),c(propertiesEE45$Q25,propertiesEE46$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="9:34 to 11:34 am on 24 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE45$median,propertiesEE46$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE45$Q75,propertiesEE46$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels10, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[45],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.515)


plot(c(0:119),c(propertiesEE47$Q25,propertiesEE48$Q25), 
     ylab="Frequency (Hz)",ylim=c(0,7500),xaxt='n',
     xlab="11:34 to 1:34 pm on 24 March 2015",type="l",
     mgp = c(1.5, 0.5, 0),cex=0.9)
par(new=TRUE)
plot(c(0:119),c(propertiesEE47$median,propertiesEE48$median), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="red")
par(new=TRUE)
plot(c(0:119),c(propertiesEE47$Q75,propertiesEE48$Q75), 
     ylab="",ylim=c(0,7500),
     xlab="",type="l",xaxt='n',yaxt='n',col="blue")
axis(side=1,at=positions,labels=timeLabels11, mgp=c(1.8,0.5,0))
mtext(side=4,myFiles[47],line=-1.80, font=1, cex=0.7, 
      outer=TRUE,at=0.185)

#########################################