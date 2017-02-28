# Generate a new directoryif (!file.exists("./GeneratedData")) {
dir.create("./GeneratedData")
output.file <- "./GeneratedData/VoteDensityHighRes.csv"
# checking if directory does not exist
if (!file.exists("./GeneratedData")) 
# check if function is in global environment if not then source
# the function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")

# Saving a file to tiff
tiff(file = "temp.tiff", width = 3200, height = 3200, units = "px", res = 800)
plot(1:10, 1:10)
dev.off()

#This will make a file that is 3200 x 3200 pixels, with an 800
#resolution gives you 3200/800 = 4 inches.

#I would also recommend choosing some sort of compression or you will
#end up with a rather large file.

tiff("Plot3.tiff", width = 10, height = 10, units = 'cm', res = 300)
plot(x, y) # Make plot
dev.off()
