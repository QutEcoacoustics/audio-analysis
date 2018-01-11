# convert a list of numbers to a string
rain_clusters <- c(10, 18, 21, 59)
toString(rain_clusters)

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

# convert a dataframe to a vector
SNR <- as.numeric(as.vector(SNR))

# reorder dataframe rows columns
dd[with(dd, order(dd$x)), ]

# Empty plot
plot.new()
plot(1, type="n", xlab="", ylab="", xlim=c(-55, 35), ylim=c(0, 1),
     xaxt = "n", yaxt = "n", bty = "n")

# Saving image as eps
setEPS()
postscript("whatever.eps")
plot(rnorm(100), main="Hey Some Data")
dev.off()
# or
postscript("foo.eps", horizontal = FALSE, onefile = FALSE, 
           paper = "special", height = 10, width = 10, 
           colormodel="rgb")
plot(1:10)
dev.off()

# citing R packages example
citation(package = "fpc")

# Confidence interval for a t distribution
p <- 0.95 # confidence level
data <- c(1,1,1,1,1,1,0.958333333,1,1,1)
ci <- qt((1+p)/2, length(data)-1)*sqrt(var(data)/length(data))
ci
# confidence interval for a normal distibution
# ci <- qnorm((1 + p)/2) * sqrt(var(data)/length(data))

# Saving commands to a text file
fileConn <- file("Script_for_html_code.txt")
writeLines(commands, fileConn)
close(fileConn)

# dates
startDate = as.POSIXct("2013-12-23 9:30:00")
endDate = as.POSIXct("2013-12-23 16:00:00")
dateSeq5sec = seq(from=startDate, to=endDate, by="5 sec")
head(dateSeq5sec)

# or
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20150816", format="%Y%m%d")
# Prepare dates
dates <- seq(start, finish, by = "1440 mins")

# Insert a new row
existingDF <- as.data.frame(matrix(seq(20),nrow=5,ncol=4))
existingDF
r <- 3
newrow <- seq(4)
insertRow <- function(existingDF, newrow, r) {
  existingDF[seq(r+1,nrow(existingDF)+1),] <- existingDF[seq(r,nrow(existingDF)),]
  existingDF[r,] <- newrow
  existingDF
}
insertRow(existingDF, newrow, r)
