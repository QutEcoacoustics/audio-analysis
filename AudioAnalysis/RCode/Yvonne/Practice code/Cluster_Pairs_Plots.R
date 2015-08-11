# 28 June 2015

setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")
#setwd("C:\\Work\\CSV files\\Data 22 to 27  March 2015 Woondum - Eastern Eucalypt\\")

indices <-read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)
#indices <- read.csv("Towsey_Summary_Indices 20150322_113743 to 20150327_103745 .csv", header=T)

###### Determine number of days and minutes per day in recording ##########

number.of.days <- length(unique(indices$rec.date))
min.per.day <- table(indices$rec.date)
counter <- NULL
for (i in 1:number.of.days) {
  no.min.per.day <- c(min.per.day[[i]])
  counter <- c(counter, no.min.per.day)
}

######## Create day identification for different colours in plot #############

day <- NULL
for (i in 1:number.of.days) {
  id <- rep(paste(LETTERS[i]), counter[i])
  day <- c(day, id)
}

indices <- cbind(indices, day)

counter <- cumsum(counter)
#############################################################################
library (mclust)
par(mfrow=c(1,1))
nf <- layout(matrix(c(1,1,0,2), 2, 2, byrow = TRUE), respect = TRUE)
layout.show(nf)
plot(indices)
plot(indices)
a <- indices[1:counter[1],]
b <- indices[counter[1]:counter[2],]
c <- indices[counter[2]:counter[3],]
d <- indices[counter[3]:counter[4],]
e <- indices[counter[4]:counter[5],]
f <- indices[counter[5]:counter[6],]

clPairs(a[,c(4,5,6)], cl=a[,29], symbols=as.character(1), colors = 1)
clPairs(b[,c(4,5,6)], cl=b[,29], symbols=as.character(2), colors = 2)
clPairs(c[,c(4,5,6)], cl=c[,29], symbols=as.character(3), colors = 3)
clPairs(d[,c(4,5,6)], cl=d[,29], symbols=as.character(4), colors = 4)
clPairs(e[,c(4,5,6)], cl=e[,29], symbols=as.character(5), colors = 5)
clPairs(f[,c(4,5,6)], cl=f[,29], symbols=as.character(6), colors = 6)

clPairs(a[,c(7,8,9)], cl=a[,29], symbols=as.character(1), colors = 1)
clPairs(b[,c(7,8,9)], cl=b[,29], symbols=as.character(2), colors = 2)
clPairs(c[,c(7,8,9)], cl=c[,29], symbols=as.character(3), colors = 3)
clPairs(d[,c(7,8,9)], cl=d[,29], symbols=as.character(4), colors = 4)
clPairs(e[,c(7,8,9)], cl=e[,29], symbols=as.character(5), colors = 5)
clPairs(f[,c(7,8,9)], cl=f[,29], symbols=as.character(6), colors = 6)

clPairs(a[,c(10,11,12)], cl=a[,29], symbols=as.character(1), colors = 1)
clPairs(b[,c(10,11,12)], cl=b[,29], symbols=as.character(2), colors = 2)
clPairs(c[,c(10,11,12)], cl=c[,29], symbols=as.character(3), colors = 3)
clPairs(d[,c(10,11,12)], cl=d[,29], symbols=as.character(4), colors = 4)
clPairs(e[,c(10,11,12)], cl=e[,29], symbols=as.character(5), colors = 5)
clPairs(f[,c(10,11,12)], cl=f[,29], symbols=as.character(6), colors = 6)

clPairs(a[,c(13,14,15)], cl=a[,29], symbols=as.character(1), colors = 1)
clPairs(b[,c(13,14,15)], cl=b[,29], symbols=as.character(2), colors = 2)
clPairs(c[,c(13,14,15)], cl=c[,29], symbols=as.character(3), colors = 3)
clPairs(d[,c(13,14,15)], cl=d[,29], symbols=as.character(4), colors = 4)
clPairs(e[,c(13,14,15)], cl=e[,29], symbols=as.character(5), colors = 5)
clPairs(f[,c(13,14,15)], cl=f[,29], symbols=as.character(6), colors = 6)

clPairs(a[,c(16,17,18)], cl=a[,29], symbols=as.character(1), colors = 1)
clPairs(b[,c(16,17,18)], cl=b[,29], symbols=as.character(2), colors = 2)
clPairs(c[,c(16,17,18)], cl=c[,29], symbols=as.character(3), colors = 3)
clPairs(d[,c(16,17,18)], cl=d[,29], symbols=as.character(4), colors = 4)
clPairs(e[,c(16,17,18)], cl=e[,29], symbols=as.character(5), colors = 5)
clPairs(f[,c(16,17,18)], cl=f[,29], symbols=as.character(6), colors = 6)

clPairs(a[,c(4,5,10,11,15,16,18)], cl=a[,29], symbols=as.character(1), colors = 1)
clPairs(b[,c(4,5,10,11,15,16,18)], cl=b[,29], symbols=as.character(2), colors = 2)
clPairs(c[,c(4,5,10,11,15,16,18)], cl=c[,29], symbols=as.character(3), colors = 3)
clPairs(d[,c(4,5,10,11,15,16,18)], cl=d[,29], symbols=as.character(4), colors = 4)
clPairs(e[,c(4,5,10,11,15,16,18)], cl=e[,29], symbols=as.character(5), colors = 5)
clPairs(f[,c(4,5,10,11,15,16,18)], cl=f[,29], symbols=as.character(6), colors = 6)
