#Constant value for linear transformation of indices
r.max<-0.7
r.min<-0.4
g.max<-0.95
g.min<-0.7
b.max<-0.8
b.min<-0.1

#red for ACI
r<-as.vector(long.aci)
r[which(r<r.min & r>0)]<-0
r[which(r>r.max)]<-1
r[which(r>r.min & r<r.max)]<-(r[which(r>r.min & r<r.max)]-r.min)*(1/(r.max-r.min))
r[which(r == -1)]<-0.5
# r<-r^2

#green for temporal entropy
g<-as.vector(long.ten)
g[which(g<g.min & g>0)]<-0
g[which(g>g.max)]<-1
g[which(g>g.min & g<g.max)]<-(g[which(g>g.min & g<g.max)]-g.min)*(1/(g.max-g.min))
g[which(g == -1)]<-0.5
g<-1-g
# g<-g^2

#blue for cover spectra
b<-as.vector(long.cvr)
b[which(b<b.min & b>0)]<-0
b[which(b>b.max)]<-1
b[which(b>b.min & b<b.max)]<-(b[which(b>b.min & b<b.max)]-b.min)*(1/(b.max-b.min))
b[which(b == -1)]<-0.5
# b<-b^2

#generate a palette of RGB values
rgb.palette<-rgb(r,g,b)

#draw the palette in pixels
x<-1:1435
y<-1:257
mydata<-matrix(1:(1435*257), 1435, 257)
# image(x,y,mydata,col=rgb.palette)
#image(seq(0,24,24/1435),seq(0,8820,8820/256),mydata,col=rgb.palette,xaxp=c(0,24,24),yaxp=c(0,8820,6),xlab='time/hour',ylab='frequency/Hz')

date<-seq(as.Date("2011/10/20"), as.Date("2012/7/2"), "day")
# png("test.png",width=1435, height=366)
# par(fin=c(15.3, 3), pin=c(14.948, 2.677), xaxs='i', yaxs='i')    # 1 inch = 96 pixels
# par(fin=c(15.3, 8), pin=c(14.948, 5.354), xaxs='i', yaxs='i')
# image(seq(0,24,24/1434),date,mydata,col=rgb.palette,xaxp=c(0,24,24), xlab="Hours",ylab='Days (from 2011/10/20 to 2012/10/20)',yaxt='n')
# axis.Date(2, at=seq(as.Date("2011/10/20"), as.Date("2012/10/20"), 60), format="%m")
# dev.off()