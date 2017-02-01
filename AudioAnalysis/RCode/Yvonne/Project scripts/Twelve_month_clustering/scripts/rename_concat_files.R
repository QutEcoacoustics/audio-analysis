# Title:  Renaming Concatenation Files 
# Author: Yvonne Phillips
# Date:  16 October 2016

# Description: Rename fileNames to newNames

folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput"
fileNames <- list.files(path = folder, recursive = T, full.names = T,
                        pattern = "*\\.SpectralRib")

# Replace dot in filename with underscore
newNames <- gsub(".S", "_S", fileNames, fixed=TRUE)

# rename filename to NewNames
file.rename(fileNames, newNames)