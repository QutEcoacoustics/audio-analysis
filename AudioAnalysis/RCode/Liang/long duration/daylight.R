
setDaylight <- function(filepath){
#   filepath <- "c:/work/myfile/sunrise_sunset.csv"
  
  daylight <- read.csv(filepath)
  daylight <- as.matrix(daylight)

  #column 2 is sunrise, column 3 is sunset
  # charSunrise <- daylight[ , 2]
  # charSunset <- daylight[ , 3]
  # astStart <- daylight[ , 5]
  # astEnd <- daylight[ , 6]
  # nautStart <- daylight[ , 7]
  # nautEnd <- daylight[ , 8]
  civilStart <- daylight[ , 6]
  civilEnd <- daylight[ , 7]

  #regular expression to find the 
  for(cnt in 1:length(civilStart)){
    civilStart[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", civilStart[cnt])
    civilEnd[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", civilEnd[cnt])
    #   charSunrise[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", charSunrise[cnt])
    #   charSunset[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", charSunset[cnt])
    #   astStart[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", astStart[cnt])
    #   astEnd[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", astEnd[cnt])
    #   nautStart[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", nautStart[cnt])
    #   nautEnd[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", nautEnd[cnt])
  }

  civilS <- as.numeric(civilStart)
  civilS <- civilS %/% 100 * 60 + civilS %% 100
  civilE <- as.numeric(civilEnd)
  civilE <- civilE %/% 100 * 60 + civilE %% 100
  
  # sunrise <- as.numeric(charSunrise)
  # sunrise <- sunrise %/% 100 * 60 + sunrise %% 100
  # sunset <- as.numeric(charSunset)
  # sunset <- sunset %/% 100 * 60 + sunset %% 100
  # 
  # astS <- as.numeric(astStart)
  # astS <- astS %/% 100 * 60 + astS %% 100
  # astE <- as.numeric(astEnd)
  # astE <- astE %/% 100 * 60 + astE %% 100
  # 
  # nautS <- as.numeric(nautStart)
  # nautS <- nautS %/% 100 * 60 + nautS %% 100
  # nautE <- as.numeric(nautEnd)
  # nautE <- nautE %/% 100 * 60 + nautE %% 100
  
  result <- list(civilS=civilS, civilE=civilE)
  return (result)
}