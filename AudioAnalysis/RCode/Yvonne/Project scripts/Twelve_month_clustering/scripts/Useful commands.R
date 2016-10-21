# Generate a new directoryif (!file.exists("./GeneratedData")) {
dir.create("./GeneratedData")
output.file <- "./GeneratedData/VoteDensityHighRes.csv"
# checking if directory does not exist
if (!file.exists("./GeneratedData")) 
# check if function is in global environment if not then source
# the function
if(!exists("col_func", mode="function")) source("scripts/col_func.R")