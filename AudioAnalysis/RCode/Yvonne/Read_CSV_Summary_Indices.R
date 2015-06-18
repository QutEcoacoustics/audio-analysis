##############################################################################
#  A generic file to read in the Towsey summary indices into one data frame
#  called allfile.contents
##############################################################################
setwd("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\Yvonne")

sourceDir <- "D:\\Data\\Data 15 to 20 March 2015 Woondum - Wet Eucalypt\\"

myFiles <- list.files(full.names=FALSE,pattern="*.wav",path=sourceDir)

length<-length(myFiles)

folder <- "D:\\Work\\Data\\2015Mar26-134159 - Yvonne, Towsey.Indices, ICD=60.0, #14\\Yvonne\\Wet Eucalypt\\"

all.file.contents<-NULL
for (i in 1:length){
pathName<-(paste(folder, myFiles[i], "\\Towsey.Acoustic\\", sub("*.wav","\\1",myFiles[i]), "_Towsey.Acoustic.Indices.csv", sep =""))
#fileContents<- read.csv(pathName)
  assign(paste("fileContents"),read.csv(pathName))
  all.file.contents<-rbind(all.file.contents,fileContents)
}
rm(fileContents)
all.file.contents
View(all.file.contents)