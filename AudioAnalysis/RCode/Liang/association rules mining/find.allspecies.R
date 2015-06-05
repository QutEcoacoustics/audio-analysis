#find all unique species names over all hitmaps

#search for all relevant files
folder <- "C:\\Work\\myfile\\SERF_callCount_20sites"
pattern <- "hitmaps*"
filename <- list.files(folder, pattern, no..=TRUE)

#find species names in each file, save as a list
species.names <- list()
for(i in 1:length(filename)){
  species <- read.csv(paste(folder, "\\", filename[i], sep=""))
  species <- species[, 4:ncol(species)]
  species.names[[i]] <- names(species)
}

#find all unique species names from the list, sort them in ascending order by species ID
all.species.names <- species.names[[1]]
for(i in 2:length(filename)){
  for(j in 1:length(species.names[[i]])){
    logic.vector <- all.species.names == species.names[[i]][j]
    if(!any(logic.vector)){
      all.species.names <- c(all.species.names, species.names[[i]][j])
    }
  }
}
all.species.names <- sort(all.species.names)