read.audio.file <- function(filepath){
  library(tuneR)

  # origin <- readWave('cabin_EarlyMorning4_CatBirds20091101-000000_0min.wav')
  if(grepl("wav$",filepath))
    origin <- readWave(filepath)
  else if(grepl("[[:alpha:]]+3$",filepath))
    origin <- readMP3(filepath)
  else
    print("Error")
  
  # Use left channel audio signal (if mono, use left only)
  sampRate <- origin@samp.rate
  bit <- origin@bit
  left <- origin@left
  
  results <- list(signal=left, sampleRate=sampRate, bit=bit)
  return(results)
}