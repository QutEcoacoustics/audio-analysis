# Find frequency by Rob Hyndman
find.freq <- function(x)
{
  n <- length(x)
  spec <- spec.ar(c(x),plot=FALSE)
  if(max(spec$spec)>10) # Arbitrary threshold chosen by trial and error.
  {
    period <- round(1/spec$freq[which.max(spec$spec)])
    if(period==Inf) # Find next local maximum
    {
      j <- which(diff(spec$spec)>0)
      if(length(j)>0)
      {
        nextmax <- j[1] + which.max(spec$spec[j[1]:500])
        period <- round(1/spec$freq[nextmax])
      }
      else
        period <- 1
    }
  }
  else
    period <- 1
  return(period)
}
x = cos( (2*pi/40) * (1:1000))+rnorm(1000)
find.freq(x)

###################################
# by Rich 
# http://stats.stackexchange.com/questions/1207/period-detection-of-a-generic-time-series
chisq.pd <- function(x, min.period, max.period, alpha) {
  N <- length(x)
  variances = NULL
  periods = seq(min.period, max.period)
  rowlist = NULL
  for(lc in periods){
    ncol = lc
    nrow = floor(N/ncol)
    rowlist = c(rowlist, nrow)
    x.trunc = x[1:(ncol*nrow)]
    x.reshape = t(array(x.trunc, c(ncol, nrow)))
    variances = c(variances, var(colMeans(x.reshape)))
  }
  Qp = (rowlist * periods * variances) / var(x)
  # degrees of freedom
  df = periods - 1
  pvals = 1-pchisq(Qp, df) #pchisq gives the distribution function
  pass.periods = periods[pvals<alpha]
  pass.pvals = pvals[pvals<alpha]
  #return(cbind(pass.periods, pass.pvals))
  return(cbind(periods[pvals==min(pvals)], pvals[pvals==min(pvals)]))
}

x = cos( (2*pi/37.5) * (1:1000))+rnorm(1000)
chisq.pd(x, 2, 72, .05)
