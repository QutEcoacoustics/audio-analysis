#change WD to ensure the file reading script works

setwd("C:/temp/Actual_Paper_Indices/Fresh_eventstats/L.rothii/ACI")


#Calculate NMDS

#load vegan package and MASS package#

library("vegan")
library("MASS")

#read csv file into R

spc <- read.csv("ACI_Lr_2.2.csv", sep=",", header=T, na.strings="NA")
##Calculate Mets MDS for Spc##
meta <- metaMDS(spc, k=, trymax=100, na.rm=T)
meta
plot(meta, display = "sites")

#Check stressplot via shepards plot and Goodness of fit

#shepards plot
meta
meta$stress
stressplot(meta, main = "Shepard's plot")

#goodness of fit and shepards plot in a single image
par(mfrow=c(1,2))
stressplot(meta, main = "Shepard's plot")
gof <- goodness(meta)
max.gof <- max (gof)

#scaling of goodness of fit, can be scaled up or down
point.size <- 5/max.gof
scores.gof <- scores (meta)
plot(meta, type="n", main="Goodness of fit")
points(scores.gof, pch=21, cex=gof*point.size)
text(scores.gof, row.names(meta), pos=3, cex=0.7)


# fit env var
env <- read.csv("ENV_Lr.2.csv")
envord <- envfit(meta, env, permu =999, na.rm=T)
envord

###Creating plots#####

#Creating the NMDS Plot

plot(meta, display = "sites")

#Fitting the env varibles on NMDS plot

plot(envord, add=T, p.=0.05, col="black", air=0.1)

#Checking its Fit and add the 2d surface
sol.s <- ordisurf(meta~DF, data = env, method = "REML", 
                  select = TRUE)
## look at the fitted model
summary(sol.s)

#Classify the plot based on treatments

#Lr : High=269; Low=48; Mid=109
#Cb : High=115; Low=80; Mid=128
#LB : High=132; Low=140; Mid=65
#Ui : High=122; Low=349; Mid=117
#ALL : Lr=427; Lb=337; Cb=323; Ui=588

#3 level treatments##

treat <- c(rep("Treatment1",269),rep("Treatment2",48), rep("Treatment3",109))
orditorp(meta,display="sites",col=c(rep("green",269),rep("blue",48), rep("red",109)))

#4 level treatments##

treat <- c(rep("Treatment1",427),rep("Treatment2",337), rep("Treatment3",323), rep("Treatment4",588))
orditorp(meta,display="sites",col=c(rep("green",427),rep("blue",337), rep("red",323), rep("yellow",588)))

#6 level treatments##

treat <- c(rep("Treatment1",269),rep("Treatment2",48), rep("Treatment3",109), rep("Treatment4",269), rep("Treatment5",48), rep("Treatment6",109) )
orditorp(meta,display="sites",col=c(rep("green",269),rep("blue",48), rep("red",109),rep("yellow",269),rep("purple",48),rep("pink",109)))


#adding polygons to the plot
ordiplot(meta,type="p")
ordihull(meta,groups=treat,draw="polygon",col="grey90",label=F)
orditorp(meta,display="species",col="black",air=0.01)
