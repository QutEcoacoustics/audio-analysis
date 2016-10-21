# Title:  Renaming Concatenation Files 
# Author: Yvonne Phillips
# Date:  16 October 2016

# Description: This code renames files in the folder specified

folder <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput"
myFiles <- list.files(path = folder, recursive = T, full.names = T,
                      pattern = "*.SpectralRibbon.png")
# Use this to check that it is doing what you want
name <- myFiles[1] # store original name 
myFiles <- myFiles[1]

newPath <- NULL
for(i in 1:length(myFiles)) {
  newPath <- paste(substr(myFiles[i], 1,122), "_",
                  substr(myFiles[i], 124, nchar(myFiles[i])), 
                  sep = "")
  file.rename(myFiles[i], newPath)
}
