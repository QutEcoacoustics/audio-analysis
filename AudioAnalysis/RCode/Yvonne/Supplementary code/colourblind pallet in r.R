library(ggplot2)
library(biovizBase)
library(scales)
library(grDevices)
# The palette with grey:
cbPalette <- c("#999999", "#E69F00", "#56B4E9", "#009E73", 
               "#F0E442", "#0072B2", "#D55E00", "#CC79A7")
# grey 153, 153, 153
# orange 230, 159, 0
# lightblue 86, 180, 233
# green 0, 158, 115
# yellow 240, 228, 66
# darkblue 0, 114, 178
# burntorange 213, 94, 0
# purplepink 204, 121, 167

# grey, orange, lightblue, green, 
#yellow, darkblue, burntorange, purplepink
show_col(cbPalette)
colour_1 <- col2rgb(cbPalette)
colour_1 <- t(colour_1)
colour_1
# The palette with black:
cbbPalette <- c("#000000", "#E69F00", "#56B4E9", "#009E73", "#F0E442", "#0072B2", "#D55E00", "#CC79A7")
show_col(cbbPalette)
col2rgb(cbbPalette)

#Also see 
mypal12 <- c("#FFBF80","#FF8000","#FFFF99","#FFFF33",
  "#B2FF8C","#33FF00","#A6EDFF","#1AB2FF","#CCBFFF",
  "#664CFF","#FF99BF","#E61A33")
show_col(mypal12)
colour_2 <- col2rgb(mypal12)
colour_2 <- t(colour_2)
pallet <- c(cbPalette, cbbPalette, mypal12)
show_col(pallet)
