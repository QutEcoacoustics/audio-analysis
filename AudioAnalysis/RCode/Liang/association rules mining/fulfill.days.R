source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/names.latin2english.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/find.allspecies.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/AR.dataPreprocess.R')
source('C:/Work/GitHub/audio-analysis/AudioAnalysis/RCode/Liang/association rules mining/extract.daytime.R')

all.species.names <- find.allspecies()
species.names <- names.latin2english(all.species.names)

folder <- "C:\\Work\\myfile\\SERF_callCount_20sites"
pattern <- "hitmaps*"
filename <- list.files(folder, pattern, no..=TRUE)

for(j in 1:length(filename)){
species <- AR.dataPreprocess(paste(folder, "\\", filename[j], sep=""))
names(species) <- names.latin2english(names(species))
species <- extract.daytime(species, 271, 1110)
species.perDay <- names(species)

presence <- matrix(NA, nrow(species), length(species.names))
presence <- apply(presence, 2, factor)
presence <- data.frame(presence)
names(presence) <- species.names
for(i in 1:length(species.names)){
  if(any(species.names[i] == species.perDay)){
    index <- which(species.names[i] == species.perDay)
    presence[ , i] <- species[ , index]
  }
}

write.table(presence, file=paste('c:/work/myfile/SERF_callCount_20sites_fulllist/', filename[j], sep=''), sep=',',row.names=FALSE)
}