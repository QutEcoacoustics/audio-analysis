#create all the dates
dates <- seq(as.Date("2011/10/20"), as.Date("2012/7/2"), "day")
dates <- as.character(dates)
for(cnt in 1:length(dates)){
  dates[cnt] <- sub("([[:digit:]]+).([[:digit:]]+).([[:digit:]]+)", 
                    "\\1\\2\\3", dates[cnt])
}

filepath <- ("C:/Work/myfile/AVAILAE_Results-2013Feb05-184941 - Indicies Analysis of all of availae-SERF-VEG/")
filefolder <- dir(path = filepath, pattern = "MP3")

current <- 1
for(cnt in 1:length(dates)){
  file.date <- sub(".*_([[:digit:]]+)_.*", "\\1", filefolder[current])
  if(dates[cnt] == file.date){
    subfolder <- paste(filepath, filefolder[current], '/Towsey.Acoustic/', sep = "")
    filename <- paste(subfolder, dir(subfolder, pattern = "Indices.csv"), sep = "")
    indices <- read.csv(filename)
    indices <- as.matrix(indices)
    if(nrow(indices) < 1435){
      comp.row <- 1435 - nrow(indices)
      comp.col <- ncol(indices)
      comp.data <- matrix(rep(-1, comp.row * comp.col), comp.row, comp.col)
      indices <- rbind(indices, comp.data)
    }
    
    #26 variables(columns) in total. Column 8 for activity(?cover), Column 14 for temporal entropy
    #Column 18 for ACI
    if(cnt == 1){
      long.aci <- indices[ , 18]
      long.ten <- indices[ , 14]
      long.cvr <- indices[ , 8]
    }
    else{
      long.aci <- cbind(long.aci, indices[ , 18])
      long.ten <- cbind(long.ten, indices[ , 14])
      long.cvr <- cbind(long.cvr, indices[ , 8])
    }
    current <- current + 1
  }
  else{
    missing.values <- rep(-1, 1435)
    dim(missing.values) <- c(1435, 1)
    long.aci <- cbind(long.aci, missing.values)
    long.ten <- cbind(long.ten, missing.values)
    long.cvr <- cbind(long.cvr, missing.values)
  }

}