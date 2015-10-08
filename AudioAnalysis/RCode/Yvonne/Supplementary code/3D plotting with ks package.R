# Plotting 3D density plot
# kde can handle up to 6D data
## positive data example
library(MASS)
library(ks)
x <- iris[,1:3]
H.pi <- Hpi(x, pilot="samse")
fhat <- kde(x, H=H.pi, compute.cont=T)  
plot(fhat, drawpoints=TRUE)

x <- indices[3590:3650,c(4,6,10,11)]
H.pi <- Hpi(x, pilot="samse")
fhat <- kde(x, H=H.pi, compute.cont=F)  
plot(fhat, drawpoints=TRUE)
