##################################################################
#  28 June 2015
#  R version:  3.2.1 
#  This file generates regression trees 
#  

setwd("C:\\Work\\CSV files\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\")

indices<-read.csv("Towsey_summary_indices 20150315_133427 to 20150320_153429 .csv", header=T)

########## Regression Tree ############################
png(
  "RegressionTree.png",
  width     = 1200,
  height    = 600,
  units     = "mm",
  res       = 600,
  pointsize = 4
)
par(mar=c(0.5,0.5,0.5,0.5))
rt.a1 <- rpart(indices$AcousticComplexity ~ .,
              data = indices[,c(3:12,14:20)], cp = 0.003)
rt.a2 <- rpart(indices$Activity ~ .,
               data = indices[,c(3:12,14:20)], cp = 0.0015) # lowest error
rt.a3 <- rpart(indices$AvgEntropySpectrum ~ .,
               data = indices[,c(3:12,14:20)], cp = 0.002)

#prettyTree(rt.a1, cex=2.5, branch = 1, compress = T)
prettyTree(rt.a2, cex=4, branch = 1, compress = T)
#prettyTree(rt.a3, cex=2, branch = 1, compress = T)
#plot(rt.a2)
#text(rt.a2, cex=3)
#printcp(rt.a1) 
printcp(rt.a2)
#printcp(rt.a3)

dev.off()