# Load the NMF package before using this function
# library(NMF)

NMF.analysis <- function(amplitudes.matrix){
  
# randomise the spectrogram matrix (by rows)
# row.no<-sample(nrow(amplitudes.matrix))
# random.amp<-amplitudes.matrix[row.no, ]

# randomise the spectrogram matrix (by columns)
  col.no<-sample(ncol(amplitudes.matrix))
  random.amp<-amplitudes.matrix[ , col.no]
  
# initialise the variables
  diff.residual <- 0
  diff.random.residual <- 0
  r <- 1
  residual <- numeric()
  random.residual <- numeric()
  
# Self-determination of factorisation rank r. The iterations stop when the residuals of the original
# amplitude matrix is smaller than that of its randomised counterpart
  while(diff.residual >= diff.random.residual){
#   The nmf algorithm is called 'alternating least square' --- Kim et al 2007
    factorisation <- nmf(amplitudes.matrix, rank=r, method='snmf/l', seed=12345)
    factorisation.random <- nmf(random.amp, rank=r, method='snmf/l', seed=12345)
    residual[r] <- factorisation@residuals
    random.residual[r] <- factorisation.random@residuals
    if (r>1){
      diff.residual <- residual[r-1] - residual[r]
      diff.random.residual <- random.residual[r-1] - random.residual[r]
    }
    r <- r + 1
  }
  
# Variable 'factorisation' contains the final results. mydata.h is the spectral information; 
# mydata.w is the temporal information
  mydata.h<-basis(factorisation)
  mydata.w<-coef(factorisation)

  results <- list(spectral.profiles=mydata.h, temporal.coefficients=mydata.w)
  return(results)
}

