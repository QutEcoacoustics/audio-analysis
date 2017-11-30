###Step 1 - Formating the .csv file to ensure all files in one single .csv###

#change WD to ensure the file reading script works

setwd("C:/temp/Actual_Paper_Indices/Fresh_eventstats/L.bicolor/ACI_ENT")

#Concat all files in a folder, and include the filename, especially for the Freshwater data

files <- list.files()
files

# read the files into a list of data.frames
data.list <- lapply(files, function(.file){
  dat<-read.csv(.file, header = F)
  dat$period<-as.character(.file)
  dat
})


# concatenate into one big data.frame
data.cat <- do.call(rbind, data.list)

#write the concatednated data to csv files

write.csv(data.cat, "freshwaterENT.csv")



#### Step 2 - Edit the concatenated data of interest####

#In this step, make sure the data included in analysis is based on the species and/ or frequency bin of interest. 
#To Do: Write script on how to do this.

##THE FOLLOWING DONE IN EXCEL:

#The name of the file has changed due to editing on excel. The editing has also done the following:
# 1. matched audio files with audio_code and time stamps
# 2. matched the level of intensity with indices files
# 3. All files are now species specific - only time stamps that are species specific
# 4. Subsetted frequency bins, while removing initial numbering from first column and renamed the file including "_2"
#note: orginal file without "_2" has the numbering in tact. this is to prevent any errors due to removing bins or sorting.


##Note for this analysis: 
#FreshwaterACI_2 is a file with appropriate audiocode_time, and no-subsetting of freq bin, and all audio files generated into indices##




##### Step 3 - Start analysing files #####

#load vegan package and MASS package#

library("vegan")
library("MASS")

#read csv file into R

spc_aci <- read.csv("ACI_ENT_Lb_para.2.2.csv", sep=",", header=T, na.strings="NA")
head(spc_aci)

##Calculate Mets MDS for Spc_ACI##
meta <- metaMDS(spc_aci, k=2, trymax=100)
meta

##If wanted##
#Check stressplot

meta
meta$stress
stressplot(meta)

###If wanted###

#Calculate Veg Dist (dissimilarities indices)
data_vegdist<-vegdist(spc_aci, method="euclidean", na.rm=TRUE)
#if wanted: plot the data
plot(data_vegdist)
##Calculate Meta MDS for veg dist##
meta <- metaMDS(data_vegdist, k=2, trymax=100)



#trying to fit env var
env <- read.csv("ENV_Lb_para.2.2.csv")
envord <- envfit(meta, env, permu =999, na.rm=T)
envord
plot(meta, display = "sites")
plot(envord)



##Check: plot theNMDS: phase 1 (use type="point" for points and type="text" for text)

plot(meta, type="p")
plot(meta, type="t")

#Add Polygons to the plot

#Lr : high=269; Low =48; Mid = 109
#Cb : High=115; Low=80; Mid= 128
#LB : High=132; Low=140; Mid=65

treat <- c(rep("Treatment1",132),rep("Treatment2",140), rep("Treatment3",65))
ordiplot(meta,type="p")
ordihull(meta,groups=treat,draw="polygon",col="grey90",label=F)
orditorp(meta,display="species",col="black",air=0.01)
orditorp(meta,display="sites",col=c(rep("green",132),rep("blue",140), rep("red",65)))


#ParralelIndices

treat <- c(rep("Treatment1",132),rep("Treatment2",140), rep("Treatment3",65), rep("Treatment4",132), rep("Treatment5",140), rep("Treatment6",65) )
ordiplot(meta,type="p")
ordihull(meta,groups=treat,draw="polygon",col="grey90",label=F)
orditorp(meta,display="species",col="black",air=0.01)
orditorp(meta,display="sites",col=c(rep("green",132),rep("blue",140), rep("red",65),rep("yellow",132),rep("purple",140),rep("pink",65)))
         
                                    




