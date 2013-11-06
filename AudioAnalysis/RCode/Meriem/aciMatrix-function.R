  aci <- function(spectrogram){  
 
  # The apply function applies the given function (row) to each row in the matrix
  aciFrequencies <- apply(spectrogram, 1, function(row){
  
  sumIntensities <- sum(row) # returns the sum of all values present in this row 
                              #row <- amplitudeFile[1:ncol(amplitudeFile)]
                               #sumIntensities <- sum(row) this for printing the sumIntensities
  
  rowLength <- length(row)
  deltatIntensities <- sum(abs(row[1:rowLength-1] - row[2:rowLength]))
  print(deltatIntensities)
  
  # ifelse(tes, yes, no), 'yes' returns values for true elements of test
  #  and 'no' returns values for false elements of test
  
  ifelse(sumIntensities > 0.0, deltatIntensities / sumIntensities, 0.0)
  }
  )
 # The list returns both the mean (aciFrequencies) and (aciFrequencies) itself
 list("averageACI" = mean(aciFrequencies), "aciFrequencies" = aciFrequencies )
  }