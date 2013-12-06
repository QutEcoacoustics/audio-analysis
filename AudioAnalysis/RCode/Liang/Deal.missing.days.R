
dim(rgb.palette) <- c(1435, 257)
for(i in 1:257){
#   rgb.palette[sunrise[i], i] <- "#FFFFFF"
#   rgb.palette[sunset[i]+720, i] <- "#FFFFFF"
#   rgb.palette[ast.s[i], i] <- "#FFFFFF"
#   rgb.palette[naut.s[i], i] <- "#FFFFFF"
  rgb.palette[civil.s[i], i] <- "#FFFFFF"
#   rgb.palette[ast.e[i]+720, i] <- "#FFFFFF"
#   rgb.palette[naut.e[i]+720, i] <- "#FFFFFF"
  rgb.palette[civil.e[i]+720, i] <- "#FFFFFF"
}
