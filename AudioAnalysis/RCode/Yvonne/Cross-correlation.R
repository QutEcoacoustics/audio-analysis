######## Cross-correlation ##########################
# 8 August 2015 
# Cross-correlation compares two time-series, for example
# time series from different days or locations

#  This file is #13 in the sequence:
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
#  12. Auto-correlation
# *13. Cross-correlation

library(forecast)
setwd("C:\\Work\\CSV files\\GympieNP1\\2015_06_21\\TimeSeriesPlots\\")
# Read time-Series from Cluster time series.R
timeSeries <- read.csv("Cluster_time_series_GympieNP 22-28 June 201522-06-2015 to 28-06-2015.csv", header=T)

# Set up a time-series
aMatrix <- matrix(timeSeries[,2], length(timeSeries[,2]), 1)
minutesA <- msts(aMatrix, seasonal.periods=c(60,1440))
m <- ts(matrix(minutesA, length(minutesA), 1), 
        start = c(0,1), frequency=1)

t22June.midnight.ts <- ts(m[1:240])
t23June.midnight.ts <- ts(m[1441:1680])
t24June.midnight.ts <- ts(m[2881:3120])
t25June.midnight.ts <- ts(m[4321:4560])
t26June.midnight.ts <- ts(m[5761:6000])
t27June.midnight.ts <- ts(m[7201:7440])
t28June.midnight.ts <- ts(m[8641:8880])

t22June.6to8 <- ts(m[361:480])
t23June.6to8 <- ts(m[1801:1920])
t24June.6to8 <- ts(m[3241:3360])
t25June.6to8 <- ts(m[4681:4800])
t26June.6to8 <- ts(m[6121:6240])
t27June.6to8 <- ts(m[7560:7680])
t28June.6to8 <- ts(m[9001:9120])

t22June <- m[1:1440]
t23June <- m[1441:2880]
t24June <- m[2881:4320]
t25June <- m[4321:5760]
t26June <- m[5761:7200]
t27June <- m[7201:8640]
plot(t22June, type="l",main="22June2015")
plot(t23June, type="l",main="23June2015")
plot(t24June, type="l",main="24June2015")
plot(t25June, type="l",main="25June2015")
plot(t26June, type="l",main="26June2015")
plot(t27June, type="l",main="27June2015")

# Whole day comparisons
t22.t23 <- ccf(t22June, t23June)
t23.t24 <- ccf(t23June, t24June)
t24.t25 <- ccf(t24June, t25June)
t25.t26 <- ccf(t25June, t26June)
t26.t27 <- ccf(t26June, t27June)
t24.t26 <- ccf(t24June, t26June)
t25.t27 <- ccf(t25June, t27June)

max.hmc <- max(t26.t27$acf) 
t26.t27$lag[which(t26.t27$acf > max.hmc-0.0001 & t26.t27$acf < max.hmc+0.0001)]
# 0.810471 occurs at lag = 2 - overall these days have a very 
# high correlation 
max.hmc <- max(t24.t25$acf) 
t24.t25$lag[which(t24.t25$acf > max.hmc-0.0001 & t24.t25$acf < max.hmc+0.0001)]
# 0.75006976 at lag of 0.

par(mfrow=c(3,2), mar=c(2,2,3,2))
ccf(t22June.midnight.ts, t23June.midnight.ts)
ccf(t23June.midnight.ts,t24June.midnight.ts) 
ccf(t24June.midnight.ts,t25June.midnight.ts)
ccf(t25June.midnight.ts, t26June.midnight.ts)
ccf(t26June.midnight.ts,t27June.midnight.ts) 
ccf(t27June.midnight.ts,t28June.midnight.ts)
