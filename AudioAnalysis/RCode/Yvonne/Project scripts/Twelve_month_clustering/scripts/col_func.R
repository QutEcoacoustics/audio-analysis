col_func <- function(cluster_colours) {
  # read the colour reference for each cluster -----------
  cluster_colours <<- read.csv("data/datasets/Cluster_features.csv")
  # Convert the RGB values into hexadecimal (base16) 
  # colours
  library(R.utils)
  cols <- NULL
  for(i in 1:nrow(cluster_colours)) {
    R_code <- intToHex(cluster_colours$R[i])
    if(nchar(R_code)==1) {
      R_code <- paste("0", R_code, sep="")
    }
    G_code <- intToHex(cluster_colours$G[i])
    if(nchar(G_code)==1) {
      G_code <- paste("0", G_code, sep="")
    }
    B_code <- intToHex(cluster_colours$B[i])
    if(nchar(B_code)==1) {
      B_code <- paste("0", B_code, sep="")
    }
    col_code <- paste("#",
                      R_code, 
                      G_code,
                      B_code,
                      sep = "")
    cols <- c(cols, col_code)
  }
  cols <<- cols
}
