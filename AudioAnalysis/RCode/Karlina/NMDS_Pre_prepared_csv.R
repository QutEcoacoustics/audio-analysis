#change WD to ensure the file reading script works

setwd("C:/temp/Actual_Paper_Indices/Fresh_eventstats/L.rothii/ACI")


#Calculate NMDS

#load vegan package and MASS package#

library("vegan")
library("MASS")

#read csv file into R

spc <- read.csv("ACI_Lr_2.2.csv", sep=",", header=T, na.strings="NA")
##Calculate Mets MDS for Spc##
meta <- metaMDS(spc, k=2, trymax=100, na.rm=T)
meta

#Check stressplot via shepards plot and Goodness of fit

#shepards plot
meta
meta$stress
stressplot(meta)
gof <- goodness(meta)
gof
max.gof <- max (gof)
point.size <- 5/max.gof


# fit env var
env <- read.csv("ENV_Lb.2.2.csv")
envord <- envfit(meta, env, permu =999, na.rm=T)
envord
plot(meta, display = "sites")
plot(envord, col="black", air=0.1)

#Add Polygons to the plot

#Lr : High=269; Low=48; Mid=109
#Cb : High=115; Low=80; Mid=128
#LB : High=132; Low=140; Mid=65
#Ui : High=122; Low=349; Mid=117
#ALL : Lr=427; Lb=337; Cb=323; Ui=588

##For 3 level treatments##

treat <- c(rep("Treatment1",132),rep("Treatment2",140), rep("Treatment3",65))
ordiplot(meta,type="p")
ordihull(meta,groups=treat,draw="polygon",col="grey90",label=F)
orditorp(meta,display="species",col="black",air=0.01)
orditorp(meta,display="sites",col=c(rep("green",122),rep("blue",349), rep("red",117)))

##For 4 level treatments##

treat <- c(rep("Treatment1",427),rep("Treatment2",337), rep("Treatment3",323), rep("Treatment4",588))
ordiplot(meta,type="p")
ordihull(meta,groups=treat,draw="polygon",col="grey90",label=F)
orditorp(meta,display="species",col="black",air=0.01)
orditorp(meta,display="sites",col=c(rep("green",427),rep("blue",337), rep("red",323), rep("yellow",588)))


##For 6 level treatments##

treat <- c(rep("Treatment1",269),rep("Treatment2",48), rep("Treatment3",109), rep("Treatment4",269), rep("Treatment5",48), rep("Treatment6",109) )
ordiplot(meta,type="p")
ordihull(meta,groups=treat,draw="polygon",col="grey90",label=F)
orditorp(meta,display="species",col="black",air=0.01)
orditorp(meta,display="sites",col=c(rep("green",269),rep("blue",48), rep("red",109),rep("yellow",269),rep("purple",48),rep("pink",109)))
