# MASS package
paste("Venables, W. N. & Ripley, B. D. (2002)", 
      "Modern Applied Statistics with S.",
      "Fourth Edition. Springer, New York. ISBN 0-387-95457-0")

# cluster package
desc <- read.delim("C:\\Work\\Library\\cluster\\DESCRIPTION")
vers <- desc[1,]
year <- substr(desc[2,],7,10)
textVersion = paste(
  "Maechler, M., Rousseeuw, P., Struyf, A., Hubert, M., Hornik, K.(",
  year, ").  cluster: Cluster Analysis Basics and Extensions. ",
  vers, ".", sep="")
textVersion

# plotrix package
citEntry(entry="Article",
         year = 2006,
         title = "Plotrix: a package in the red light district of R",
         journal = "R-News",
         volume = "6",
         number = "4",
         pages = "8-12",
         author = personList(as.person("Lemon J")),
         textVersion = "Lemon, J. (2006) Plotrix: a package in the red light
  district of R. R-News, 6(4): 8-12."
)

# stats package
#Package: stats
#Version: 3.2.0
#Priority: base
#Title: The R Stats Package
#Author: R Core Team and contributors worldwide
#Maintainer: R Core Team <R-core@r-project.org>
#  Description: R statistical functions.
#License: Part of R 3.2.0
#Built: R 3.2.0; x86_64-w64-mingw32; 2015-04-17 11:43:10 UTC; windows
paste("R Core Team, R: A Language and Environment for Statistical Computing in R Foundation for Statistical Computing, ed. Vienna, Austria, 2016.")
