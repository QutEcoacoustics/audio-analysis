ridgeVector <- function(ridge, compression){
  if(compression == 'horizontal'){
    avRidge <- rowMeans(ridge)
  }else if(compression == 'vertical'){
    avRidge <- colMeans(ridge)
  }else if(compression == 'compact'){
    ridge <- rowMeans(ridge)
    avRidge <- mean(ridge)
  }else{
    print('error in compression method')
    return()
  }
  return(avRidge)
}