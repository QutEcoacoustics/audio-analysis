# Script by Matt Peeples http://www.mattpeeples.net/kmeans.html 

# initialize all necessary libraries
library(cluster)
library(psych)

# read CSV file - (kmeans_data.csv) - convert to a matrix
#data1 <- read.table(file='kmeans_data.csv', sep=',', header=T, row.names=1)
indicesRef <- c(5,9,11,13,14,15,17) 
length1 <- 0
length2 <- length(normIndices$X)
dataFrame <- normIndices[length1:length2, indicesRef]  

data1 <- dataFrame 
data.p <- as.matrix(data1)

# Ask for user input - convert raw counts to percents?
choose.per <- function(){readline("Covert data to percents? 1=yes, 2=no : ")} 
per <- as.integer(choose.per())

# If user selects yes, convert data from counts to percents
if (per == 1) {
  data.p <- prop.table(data.p,1)*100}

# Ask for user input - Z-score standardize data?
choose.stand <- function(){readline("Z-score standardize data? 1=yes, 2=no : ")} 
stand <- as.integer(choose.stand())

# If user selects yes, Z-score standardize data
kdata <- na.omit(data.p) 
if (stand == 1) {
  kdata <- scale(kdata)}

# Ask for user input - determine the number of cluster solutions to test (must between 2 and the number of rows in the database)
choose.level <- function(){readline("How many clustering solutions to test (> row numbers)? ")} 
n.lev <- as.integer(choose.level())

# Calculate the within groups sum of squared error (SSE) for the number of cluster solutions selected by the user
wss <- rnorm(10)
while (prod(wss==sort(wss,decreasing=T))==0) {
  wss <- (nrow(kdata)-1)*sum(apply(kdata,2,var))
  for (i in 2:n.lev) wss[i] <- sum(kmeans(kdata, iter.max = 100, 
                                          centers=i)$withinss)}

# Calculate the within groups SSE for 250 randomized data sets 
# (based on the original input data)
k.rand <- function(x) {
  km.rand <- matrix(sample(x),dim(x)[1],dim(x)[2])
  rand.wss <- as.matrix(dim(x)[1]-1)*sum(apply(km.rand,2,var))
  for (i in 2:n.lev) rand.wss[i] <- sum(kmeans(km.rand, centers=i)$withinss)
  rand.wss <- as.matrix(rand.wss)
  return(rand.wss)}
rand.mat <- matrix(0,n.lev,250)
k.1 <- function(x) { 
  for (i in 1:250) {
    r.mat <- as.matrix(suppressWarnings(k.rand(kdata)))
    rand.mat[,i] <- r.mat}
  return(rand.mat)}

# Same function as above for data with < 3 column variables
k.2.rand <- function(x){
  rand.mat <- matrix(0,n.lev,250)
  km.rand <- matrix(sample(x),dim(x)[1],dim(x)[2])
  rand.wss <- as.matrix(dim(x)[1]-1)*sum(apply(km.rand,2,var))
  for (i in 2:n.lev) rand.wss[i] <- sum(kmeans(km.rand, centers=i)$withinss)
  rand.wss <- as.matrix(rand.wss)
  return(rand.wss)}
k.2 <- function(x){
  for (i in 1:250) {
    r.1 <- k.2.rand(kdata)
    rand.mat[,i] <- r.1}
  return(rand.mat)}

# Determine if the data data table has > or < 3 variables and call appropriate function above
if (dim(kdata)[2] == 2) { rand.mat <- k.2(kdata) } else { rand.mat <- k.1(kdata) }

# Plot within groups SSE against all tested cluster solutions for actual and randomized data - 1st: Log scale, 2nd: Normal scale
par(ask=TRUE)
xrange <- range(1:n.lev)
yrange <- range(log(rand.mat),log(wss))
plot(xrange,yrange, type='n', xlab='Cluster Solution', ylab='Log of Within Group SSE', main='Cluster Solutions against Log of SSE')
for (i in 1:250) lines(log(rand.mat[,i]),type='l',col='red')
lines(log(wss), type="b", col='blue')
legend('topright',c('Actual Data', '250 Random Runs'), col=c('blue', 'red'), lty=1)
par(ask=TRUE)
yrange <- range(rand.mat,wss)
plot(xrange,yrange, type='n', xlab="Cluster Solution", 
     ylab="Within Groups SSE", 
     main="Cluster Solutions against SSE",
     ylim = c(0,1200))
for (i in 1:250) lines(rand.mat[,i],type='l',col='red')
lines(1:n.lev, wss, type="b", col='blue')
legend('topright',c('Actual Data', '250 Random Runs'), col=c('blue', 'red'), lty=1)

# Calculate the mean and standard deviation of difference between SSE of actual data and SSE of 250 randomized datasets
r.sse <- matrix(0,dim(rand.mat)[1],dim(rand.mat)[2])
wss.1 <- as.matrix(wss)
for (i in 1:dim(r.sse)[2]) {
  r.temp <- abs(rand.mat[,i]-wss.1[,1])
  r.sse[,i] <- r.temp}
r.sse.m <- apply(r.sse,1,mean)
r.sse.sd <- apply(r.sse,1,sd)
r.sse.plus <- r.sse.m + r.sse.sd
r.sse.min <- r.sse.m - r.sse.sd

# Plot differeince between actual SSE mean SSE from 250 randomized datasets - 1st: Log scale, 2nd: Normal scale 
par(ask=TRUE)
xrange <- range(1:n.lev)
yrange <- range(log(r.sse.plus),log(r.sse.min))
plot(xrange,yrange, type='n',xlab='Cluster Solution', ylab='Log of SSE - Random SSE', main='Cluster Solustions against (Log of SSE - Random SSE)')
lines(log(r.sse.m), type="b", col='blue')
lines(log(r.sse.plus), type='l', col='red')
lines(log(r.sse.min), type='l', col='red')
legend('topright',c('SSE - random SSE', 'SD of SSE-random SSE'), col=c('blue', 'red'), lty=1)
par(ask=TRUE)
xrange <- range(1:n.lev)
yrange <- range(r.sse.plus,r.sse.min)
plot(xrange,yrange, type='n',xlab='Cluster Solution', ylab='SSE - Random SSE', main='Cluster Solutions against (SSE - Random SSE)')
lines(r.sse.m, type="b", col='blue')
lines(r.sse.plus, type='l', col='red')
lines(r.sse.min, type='l', col='red')
legend('topright',c('SSE - random SSE', 'SD of SSE-random SSE'), col=c('blue', 'red'), lty=1)

# Ask for user input - Select the appropriate number of clusters
choose.clust <- function(){readline("What clustering solution would you like to use? ")} 
clust.level <- as.integer(choose.clust())

# Apply K-means cluster solutions - append clusters to CSV file
fit <- kmeans(kdata, clust.level)
aggregate(kdata, by=list(fit$cluster), FUN=mean)
clust.out <- fit$cluster
kclust <- as.matrix(clust.out)
kclust.out <- cbind(kclust, data1)
write.table(kclust.out, file="kmeans_out1.csv", sep=",")

# Display Principal Components plot of data with clusters identified
par(ask=TRUE)
clusplot(kdata, fit$cluster, shade=F, labels=2, lines=0, 
         color=T, lty=4, 
         main='Principal Components plot showing K-means clusters')

# Send output to files
kclust.out.p <- prop.table(as.matrix(kclust.out),1)*100
out <- capture.output(describe.by(kclust.out.p,kclust))
cat(out,file='Kmeans_out1.txt', sep='\n', append=F)
pdf(file="kmeans_out1.pdf")
xrange <- range(1:n.lev)
yrange <- range(log(rand.mat),log(wss))
plot(xrange,yrange, type='n', xlab='Cluster Solution', ylab='Log of Within Group SSE', main='Cluster Solutions against Log of SSE')
for (i in 1:250) lines(log(rand.mat[,i]),type='l',col='red')
lines(log(wss), type="b", col='blue')
legend('topright',c('Actual Data', '250 Random Runs'), col=c('blue', 'red'), lty=1)
yrange <- range(rand.mat,wss)
plot(xrange,yrange, type='n', xlab="Cluster Solution", ylab="Within Groups SSE", main="Cluster Solutions against SSE")
for (i in 1:250) lines(rand.mat[,i],type='l',col='red')
lines(1:n.lev, wss, type="b", col='blue')
legend('topright',c('Actual Data', '250 Random Runs'), col=c('blue', 'red'), lty=1)
xrange <- range(1:n.lev)
yrange <- range(log(r.sse.plus),log(r.sse.min))
plot(xrange,yrange, type='n',xlab='Cluster Solution', ylab='Log of SSE - Random SSE', main='Cluster Solustions against (Log of SSE - Random SSE)')
lines(log(r.sse.m), type="b", col='blue')
lines(log(r.sse.plus), type='l', col='red')
lines(log(r.sse.min), type='l', col='red')
legend('topright',c('SSE - random SSE', 'SD of SSE-random SSE'), col=c('blue', 'red'), lty=1)
xrange <- range(1:n.lev)
yrange <- range(r.sse.plus,r.sse.min)
plot(xrange,yrange, type='n',xlab='Cluster Solution', ylab='SSE - Random SSE', main='Cluster Solutions against (SSE - Random SSE)')
lines(r.sse.m, type="b", col='blue')
lines(r.sse.plus, type='l', col='red')
lines(r.sse.min, type='l', col='red')
legend('topright',c('SSE - random SSE', 'SD of SSE-random SSE'), col=c('blue', 'red'), lty=1)
clusplot(kdata, fit$cluster, shade=F, labels=2, lines=0, color=T, lty=4, main='Principal Components plot showing K-means clusters')
dev.off() 

# end of script

output <- read.csv("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\kmeans_out1.csv", header = T)
plot(output$kclust, col=output$kclust)
abline(v=seq(1,9000,1440),lty=3)