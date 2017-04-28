
ICA.analysis <- function(amplitudes.matrix){
	library(fastICA)
	set.seed(10)
	
	# randomise the spectrogram matrix by columns
	row.no <- sample(nrow(amplitudes.matrix))
	random.amp <- amplitudes.matrix[row.no, ]
	
	# initialise the variables
	diff.residual <- 0
	diff.random.residual <- 0
	n.comp <- 1
	residual <- numeric()
	random.residual <- numeric()
	
	# self-determination of ICA components. The iterations stop when the residuals of original
	# amplitude matrix is smaller than that of its randomised counterpart
	while(diff.residual >= diff.random.residual){
		decomposed.matrix <- fastICA(amp, n.comp, method="C")
		decomposed.random.matrix <- fastICA(random.amp, n.comp, method="C",maxit=200)
		
		residual[n.comp] <- sum((amp - decomposed.matrix$S %*% decomposed.matrix$A)^2)
		random.residual[n.comp] <- sum((random.amp - decomposed.random.matrix$S %*% decomposed.random.matrix$A)^2)
		if(n.comp>1){
			diff.residual <- residual[n.comp-1] - residual[n.comp]
			diff.random.residual <- random.residual[n.comp-1] - random.residual[n.comp]
		}
		n.comp <- n.comp + 1
	}
	
	return(decomposed.matrix)
}