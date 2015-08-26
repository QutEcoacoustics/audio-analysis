# 7 August 2015
# Auto-correlation 

#  This file is #12 in the sequence:
#   1. Save_Summary_Indices_ as_csv_file.R
#   2. Plot_Towsey_Summary_Indices.R
#   3. Correlation_Matrix.R
#   4. Principal_Component_Analysis.R
#   5. kMeans_Clustering.R
#   6. Quantisation_error.R
#   7. Distance_matrix.R
#   8. Minimising_error.R
#   9. Segmenting_image.R
#  10. Transition Matrix    
#  11. Cluster Time Series
# *12. Auto-correlation
#  13. Cross-correlation
 
######## Autocorrelation ##########################

library(forecast)
a <- NULL

setwd("C:\\Work\\CSV files\\GympieNP1_new\\2015_06_21\\")
fluxes <- read.csv("Acoustic_flux_time_series_Gympie NP1 _22 to 28 June 2015.csv", header=T)

for (i in seq(121,length(fluxes$flux),60)) {
  a <- acf(fluxes$flux[i-90:i+90],plot = T, lag.max = 180, type='p')
}




################################
#setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\TimeSeriesPlots\\")
# Read time-Series from Cluster time series.R
#timeSeries <- read.csv("Cluster_time_series_GympieNP 22-28 June 201522-06-2015 to 28-06-2015.csv", header=T)

# Set up a time-series
#aMatrix <- matrix(timeSeries[,2], length(timeSeries[,2]), 1)
#minutesA <- msts(aMatrix, seasonal.periods=c(60,1440))
#m <- ts(matrix(minutesA, length(minutesA), 1), 
#        start = c(0,1), frequency=1)
# Fit a structural model
#par(mfrow=c(2,2))
#(fit <- StructTS(log10(m[300:720]), type = "trend"))
#par(mfrow = c(4, 1)) # to give appropriate aspect ratio for next plot.
#plot(log10(m[300:720]))
#plot(cbind(fitted(fit), resids=resid(fit)), main = "GympieNP 22 June 2015")

#(fit <- StructTS(log10(m[1800:2140]), type = "trend"))
#par(mfrow = c(4, 1)) # to give appropriate aspect ratio for next plot.
#plot(log10(m[1800:2140]))
#plot(cbind(fitted(fit), resids=resid(fit)), main = "GympieNP 23 June 2015")
# Three hour autocorrelation plots
plot(m)
plot(m[16:length(m)], type="l", yax.flip=TRUE, 
     xaxt="n")

lhacf <- acf(m[300:720], lag.max = 120, 
             type = 'correlation',plot=F)
layout(mat=matrix(1:3, 3, 1, byrow=FALSE))

setout <- rbind(c(4),c(1),c(1),c(1),
           c(1),c(1),c(1),c(2),
           c(2),c(2),c(2),c(2),
           c(2),c(3),c(3),c(3),
           c(3),c(3),c(3))
layout(setout)
layout.show(4)
plot(m[300:720], ylab = "Clusters", type="l",
     xlab="Minutes", las = 1)
# Trend lines using linear filters
m.1 <- filter(m[300:720],filter=rep(1/10,10)) # 10 minute
m.2 <- filter(m[300:720],filter=rep(1/30,30)) # half-hourly
m.3 <- filter(m[300:720],filter=rep(1/60,60)) # hourly
lines(m.1,col="red")
lines(m.2,col="purple")
lines(m.3,col="blue")

plot(lhacf$acf, type='l', main='Correlogram for GympieNP',
     xlab='Lag', ylab='ACF')
pacf(m[300:720])
mtext(side=3,line=-3.2,font=2,cex=0.9,outer=TRUE,
      expression(bold(paste("Gympie Cluster time series"))))

# Two hour autocorrelation plots
par(mfrow=c(1,3))
a <- acf(m[1:120],plot = T, lag.max = 3000, type='p')
b <- acf(m[121:240],plot = T, lag.max = 3000, type='p')
c <- acf(m[241:360],plot = T, lag.max = 3000, type='p')
d <- acf(m[361:480],plot = T, lag.max = 3000, type='p')
e <- acf(m[481:600],plot = T, lag.max = 3000, type='p')
f <- acf(m[601:720],plot = T, lag.max = 3000, type='p')

acf(m[721:840],plot = T,lag.max = 3000, type='p')
acf(m[841:960],plot = T,lag.max = 3000, 
    type='p')
acf(m[961:1080],plot = T,lag.max = 3000, 
    type='p',col="orange")
par(new=T)
acf(m[1081:1200],plot = T,lag.max = 3000, 
    type='p',col="green")
par(new=T)
acf(m[1201:1320],plot = T,lag.max = 3000, 
    type='p',col="blue")
par(new=T)
acf(m[1321:1440],plot = T,lag.max = 3000, 
    type='p', col="red")

acf(m[1441:1560],plot = T,lag.max = 3000)
acf(m[1561:1680],plot = T,lag.max = 3000)
acf(m[1681:1800],plot = T,lag.max = 3000)
acf(m[1801:1920],plot = T,lag.max = 3000)
acf(m[1921:2040],plot = T,lag.max = 3000)
acf(m[2041:2160],plot = T,lag.max = 3000)

acf(m[2161:2280],plot = T,lag.max = 3000)
acf(m[2281:2400],plot = T,lag.max = 3000)
acf(m[2401:2520],plot = T,lag.max = 3000)
acf(m[2521:2640],plot = T,lag.max = 3000)
acf(m[2641:2760],plot = T,lag.max = 3000)
acf(m[2761:2880],plot = T,lag.max = 3000)

par(mfrow=c(1,3))
acf(m[1:240],plot = T,lag.max = 3000)
acf(m[241:480],plot = T,lag.max = 3000)
acf(m[481:720],plot = T,lag.max = 3000)

a <-acf(m[721:960],plot = T,lag.max = 3000)
b <- acf(m[961:1200],plot = T,lag.max = 3000)
c <- acf(m[1201:1440],plot = T,lag.max = 3000)
# Day 1
par(mfrow=c(1,3))
acf(m[1:240],plot = T,lag.max = 3000)
acf(m[241:480],plot = T,lag.max = 3000)
acf(m[481:720],plot = T,lag.max = 3000)
acf(m[721:960],plot = T,lag.max = 3000)
acf(m[961:1200],plot = T,lag.max = 3000)
acf(m[1201:1440],plot = T,lag.max = 3000)

# Day 2
par(mfrow=c(1,3))
acf(m[1441:1680],plot = T,lag.max = 3000)
acf(m[1681:1920],plot = T,lag.max = 3000)
acf(m[1921:2160],plot = T,lag.max = 3000)
acf(m[2161:2400],plot = T,lag.max = 3000)
acf(m[2401:2640],plot = T,lag.max = 3000)
acf(m[2641:2880],plot = T,lag.max = 3000)

# Day 3
par(mfrow=c(1,3))
acf(m[2881:3120],plot = T,lag.max = 3000)
acf(m[3121:3360],plot = T,lag.max = 3000)
acf(m[3361:3601],plot = T,lag.max = 3000)
acf(m[3601:3840],plot = T,lag.max = 3000)
acf(m[3841:4080],plot = T,lag.max = 3000)
acf(m[4081:4320],plot = T,lag.max = 3000)

# Day 4
par(mfrow=c(1,3))
acf(m[4321:4560],plot = T,lag.max = 3000)
acf(m[4561:4800],plot = T,lag.max = 3000)
acf(m[4801:5040],plot = T,lag.max = 3000)
acf(m[5041:5280],plot = T,lag.max = 3000)
acf(m[5281:5520],plot = T,lag.max = 3000)
acf(m[5521:5760],plot = T,lag.max = 3000)

# Day 5
par(mfrow=c(1,3))
acf(m[5761:6000],plot = T,lag.max = 300)
acf(m[6001:6240],plot = T,lag.max = 300)
acf(m[6241:6480],plot = T,lag.max = 300)
acf(m[6481:6720],plot = T,lag.max = 300)
acf(m[6721:6960],plot = T,lag.max = 300)
acf(m[6961:7200],plot = T,lag.max = 300)

# Day 6
par(mfrow=c(1,3))
acf(m[7201:7440],plot = T,lag.max = 300)
acf(m[7441:7680],plot = T,lag.max = 300)
acf(m[7681:7920],plot = T,lag.max = 300)
acf(m[7921:8160],plot = T,lag.max = 300)
acf(m[8161:8400],plot = T,lag.max = 300)
acf(m[8401:8640],plot = T,lag.max = 300)

# Day 7
par(mfrow=c(1,3))
acf(m[8641:8880],plot = T,lag.max = 300)
acf(m[8881:9120],plot = T,lag.max = 300)
acf(m[9121:9360],plot = T,lag.max = 300)

# Midnight to 4 am
par(mfrow=c(1,7))
a <- acf(m[1:240],plot = T,lag.max = 3000)
b <- acf(m[1441:1680],plot = T,lag.max = 3000)
c <-acf(m[2881:3120],plot = T,lag.max = 3000)
d <- acf(m[4321:4560],plot = T,lag.max = 3000)
e <- acf(m[5761:6000],plot = T,lag.max = 300)
f <- acf(m[7201:7440],plot = T,lag.max = 300)
g <-acf(m[8641:8880],plot = T,lag.max = 300)
# 4 am to 8 am
par(mfrow=c(1,7))
a <- acf(m[241:480],plot = T,lag.max = 3000)
b <- acf(m[1681:1920],plot = T,lag.max = 3000)
c <- acf(m[3121:3360],plot = T,lag.max = 3000)
d <- acf(m[4561:4800],plot = T,lag.max = 3000)
e <- acf(m[6001:6240],plot = T,lag.max = 300)
f <- acf(m[7441:7680],plot = T,lag.max = 300)
g <- acf(m[8881:9120],plot = T,lag.max = 300)

# 6 am to 8 am
par(mfrow=c(1,7))
a <- acf(m[361:480],  plot = T,lag.max = 3000)
b <- acf(m[1801:1920],plot = T,lag.max = 3000)
c <- acf(m[3241:3360],plot = T,lag.max = 3000)
d <- acf(m[4681:4800],plot = T,lag.max = 3000)
e <- acf(m[6121:6240],plot = T,lag.max = 300)
f <- acf(m[7560:7680],plot = T,lag.max = 300)
g <- acf(m[9001:9120],plot = T,lag.max = 300)

# 6 am to 8 am partial plots
par(mfrow=c(1,7))
a <- acf(m[361:480], type='p', plot = T, lag.max = 3000)
b <- acf(m[1801:1920],   type='p', plot = T,lag.max = 3000)
c <- acf(m[3241:3360],  type='p', plot = F,lag.max = 3000)
d <- acf(m[4681:4800],  type='p', plot = T,lag.max = 3000)
e <- acf(m[6121:6240],  type='p', plot = T,lag.max = 300)
f <- acf(m[7560:7680],  type='p', plot = T,lag.max = 300)
g <- acf(m[9001:9120],  type='p', plot = T,lag.max = 300)

# 8 am to 10 am
a <- acf(m[481:600],plot = T,lag.max = 3000)
b <- acf(m[1921:2040],plot = T,lag.max = 3000)
c <- acf(m[3361:3480],plot = T,lag.max = 3000)
d <- acf(m[4801:4920],plot = T,lag.max = 3000)
e <- acf(m[6241:6360],plot = T,lag.max = 300)
f <- acf(m[7681:7800],plot = T,lag.max = 300)
g <- acf(m[9121:9240],plot = T,lag.max = 300)

# 10 am to 12 noon
a <- acf(m[601:720],plot = T,lag.max = 3000)
b <- acf(m[2041:2160],plot = T,lag.max = 3000)
c <- acf(m[3481:3600],plot = T,lag.max = 3000)
d <- acf(m[4921:5040],plot = T,lag.max = 3000)
e <- acf(m[6361:6480],plot = T,lag.max = 300)
f <- acf(m[7800:7920],plot = T,lag.max = 300)

# 8 am to 12 noon
a <- acf(m[481:720],plot = T,lag.max = 3000)
b <- acf(m[1921:2160],plot = T,lag.max = 3000)
c <- acf(m[3361:3600],plot = T,lag.max = 3000)
d <- acf(m[4801:5040],plot = T,lag.max = 3000)
e <- acf(m[6241:6480],plot = T,lag.max = 300)
f <- acf(m[7681:7920],plot = T,lag.max = 300)
dev.off()
# 12 noon to 4 pm
a <- acf(m[721:960],plot = T,lag.max = 3000)
b <- acf(m[2161:2400],plot = T,lag.max = 3000)
c <- acf(m[3601:3840],plot = T,lag.max = 3000)
d <- acf(m[5041:5280],plot = T,lag.max = 3000)
e <- acf(m[6481:6720],plot = T,lag.max = 300)
f <- acf(m[7921:8160],plot = T,lag.max = 300)

# 4 pm to 8 pm
a <- acf(m[961:1200],plot = T,lag.max = 3000)
b <- acf(m[2401:2640],plot = T,lag.max = 3000)
c <- acf(m[3841:4080],plot = T,lag.max = 3000)
d <- acf(m[5281:5520],plot = T,lag.max = 3000)
e <- acf(m[6721:6960],plot = T,lag.max = 300)
f <- acf(m[8161:8400],plot = T,lag.max = 300)
# 8 pm to Midnight
par(mfrow=c(1,7))
a <- acf(m[1201:1440],plot = T,lag.max = 3000)
b <- acf(m[2641:2880],plot = T,lag.max = 3000)
c <- acf(m[4081:4320],plot = T,lag.max = 3000)
d <- acf(m[5521:5760],plot = T,lag.max = 3000)
e <- acf(m[6961:7200],plot = T,lag.max = 300)
f <- acf(m[8401:8640],plot = T,lag.max = 300)

par(mfrow=c(1,1))
plot(a$acf, type="l",col="black")
par(new=T)
plot(b$acf, type="l",col="red")
par(new=T)
plot(c$acf, type="l",col="orange")
par(new=T)
plot(d$acf, type="l",col="blue")
par(new=T)
plot(e$acf, type="l",col="green")
par(new=T)
plot(f$acf, type="l",col="hotpink")
par(new=T)
plot(g$acf, type="l",col="yellow")

#######

acf(m[1441:1560],plot = T,lag.max = 3000)
acf(m[1561:1680],plot = T,lag.max = 3000)
acf(m[1681:1800],plot = T,lag.max = 3000)
acf(m[1801:1920],plot = T,lag.max = 3000)
acf(m[1921:2040],plot = T,lag.max = 3000)
acf(m[2041:2160],plot = T,lag.max = 3000)

acf(m[2161:2280],plot = T,lag.max = 3000)
acf(m[2281:2400],plot = T,lag.max = 3000)
acf(m[2401:2520],plot = T,lag.max = 3000)
acf(m[2521:2640],plot = T,lag.max = 3000)
acf(m[2641:2760],plot = T,lag.max = 3000)
acf(m[2761:2880],plot = T,lag.max = 3000)

####Autocorrelation example - Lake Huron depths ##############
n = length(LakeHuron)
lhacf = acf(LakeHuron,lag.max=25, type='correlation')
#layout(mat=matrix(1:3, 3, 1, byrow=FALSE))
par(mfrow=c(1,3))
plot(LakeHuron, ylab = "Feet", xlab="Year",las = 1)
title(main = "Lake Huron Water Level Data")
# Plot correlogram
plot(lhacf$acf,type='l',main='Correlogram for Lake Huron Data',xlab='Lag',ylab='ACF')
abline(h=0)
abline(h=1.96/sqrt(n),lty='dotted')
abline(h=-1.96/sqrt(n),lty='dotted')
# Now plot the partial auto-correlation function.
pacf(LakeHuron)
fit = arima(LakeHuron, order = c(2,0,0))
