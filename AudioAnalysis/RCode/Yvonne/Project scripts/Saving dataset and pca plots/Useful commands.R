# Generate a new directoryif (!file.exists("./GeneratedData")) {
dir.create("./GeneratedData")
output.file <- "./GeneratedData/VoteDensityHighRes.csv"
# checking if directory does not exist
if (!file.exists("./GeneratedData")) 
# check if function is in global environment if not then source
# the function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# Saving a file to tiff
tiff(file = "temp.tiff", width = 3200, height = 3200, units = "px", 
     res = 800)
plot(1:10, 1:10)
dev.off()

#This will make a file that is 3200 x 3200 pixels, with an 800
#resolution gives you 3200/800 = 4 inches.

#I would also recommend choosing some sort of compression or you will
#end up with a rather large file.

tiff("tiff.plot.tiff", width = 10, height = 10, 
     units = 'cm', res = 300)
plot(x, y) # Make plot
dev.off()

# installing packages example
install.packages("mapdata", repos = "http://cran.case.edu")

# listing all loaded packages
(.packages())

# draw circles also see draw.circle in Plotrix package
symbols(ylim = c(0,4),x = 2,y = 2,circles = 0.1, bg = "blue", inches = F)
symbols(x = 2, y = 3, circles = 0.1, bg = "red", inches = F, add = T)
symbols(x = 2.5,y = 2,circles = 0.1, bg = "green", inches = F, add = T)

#regular expression
# "^abc$"  use to ^ to mark the start and $ to mark the end of a string

# how to convert a factor to a factor
as.numeric(levels(f))[f]

# convert a dataframe to a list
df.list <- as.list(as.data.frame(t(df)))
