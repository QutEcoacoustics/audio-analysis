
readLongDuration2011 <- function(startDate, endDate, filepath, rCol, gCol, bCol){
  startDate <- "2011/10/20"
  endDate <- "2012/7/2"
  filepath <- ("C:/Work/myfile/AVAILAE_Results-2013Feb05-184941 - Indicies Analysis of all of availae-SERF-VEG/")

  filefolder <- dir(path = filepath, pattern = "MP3")
  #create all the dates
  dates <- seq(as.Date(startDate), as.Date(endDate), "day")
  dates <- as.character(dates)
  for(cnt in 1:length(dates)){
    dates[cnt] <- sub("([[:digit:]]+).([[:digit:]]+).([[:digit:]]+)", 
                    "\\1\\2\\3", dates[cnt])
  }

  current <- 1
  for(cnt in 1:length(dates)){
    fileDate <- sub(".*_([[:digit:]]+)_.*", "\\1", filefolder[current])
    if(dates[cnt] == fileDate){
      subfolder <- paste(filepath, filefolder[current], '/Towsey.Acoustic/', sep = "")
      filename <- paste(subfolder, dir(subfolder, pattern = "Indices.csv"), sep = "")
      indices <- read.csv(filename)
      indices <- as.matrix(indices)
      if(nrow(indices) < 1440){
        compRow <- 1440 - nrow(indices)
        compCol <- ncol(indices)
        compData <- matrix(rep(-1, compRow * compCol), compRow, compCol)
        indices <- rbind(indices, compData)
      }
    
      #26 variables(columns) in total. Column 8 for activity(?cover), Column 14 for temporal entropy
      #Column 18 for ACI
      if(cnt == 1){
        red <- indices[ , rCol]
        green <- indices[ , gCol]
        blue <- indices[ , bCol]
      }
      else{
        red <- cbind(red, indices[ , rCol])
        green <- cbind(green, indices[ , gCol])
        blue <- cbind(blue, indices[ , bCol])
      }
      current <- current + 1
    }
    else{
      missingValues <- rep(-1, 1440)
      dim(missingValues) <- c(1440, 1)
      red <- cbind(red, missingValues)
      green <- cbind(green, missingValues)
      blue <- cbind(blue, missingValues)
    }
  }
  
  result <- list(red=red, green=green, blue=blue)
  return (result)
}