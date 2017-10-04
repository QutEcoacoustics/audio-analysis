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

