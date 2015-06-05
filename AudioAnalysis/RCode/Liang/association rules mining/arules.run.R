library(arules)
# library(arulesViz)

#search for all relevant files
folder <- "C:\\Work\\myfile\\SERF_callCount_20sites"
pattern <- "hitmaps*"
filename <- list.files(folder, pattern, no..=TRUE)

source("C:\\Work\\GitHub\\audio-analysis\\AudioAnalysis\\RCode\\Liang\\association rules mining\\species.count.perMinute.R")
species.count <- numeric()
for(i in 1:length(filename)){
  count <- species.count.perMinute(paste(folder, "\\", filename[i], sep=""))
  species.count <- cbind(species.count, count)
}