library(arules)
library(arulesViz)

#search for all relevant files
folder <- "C:\\Work\\myfile\\SERF_callCount_20sites"
pattern <- "hitmaps*"
filename <- list.files(folder, pattern, no..=TRUE)

