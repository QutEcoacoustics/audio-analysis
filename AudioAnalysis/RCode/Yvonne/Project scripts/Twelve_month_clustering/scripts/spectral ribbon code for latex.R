############################################################
# generate a list of dates
############################################################
# remove all objects in the global environment
rm(list = ls())

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <-  strptime("20160723", format="%Y%m%d")
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}

# Convert dates to YYYYMMDD format
for (i in 1:length(dates)) {
  x <- "-"
  date.list[i] <- gsub(x, "",date.list[i])  
}
dates <- date.list
rm(date.list)

# Commands for Gympie ACI-ENT-EVN
commands <- paste("\\begin{document}")
commands <- c(commands, paste(" ",sep = '\n'))

ref <- 0;
for(i in 1:length(dates)) {
  ref <- ref + 1
  b <- NULL
  if(ref==1) {
    b <- paste("\\begin{figure}") 
    first_date <- dates[i]
  }
  j <- "\\vspace*{-0.36cm}"  
  k <- "\\begin{minipage}{.08\\textwidth}"
  l <- paste("\\caption*{", dates[i], "}", sep = "")
  m <- "\\end{minipage}"
  d <- NULL
  e <- NULL
  
  a <- paste("\\includegraphics[width=18.5cm]
{", dates[i], "/GympieNP_", dates[i],"__ACI-ENT-EVN_SpectralRibbon.png}", sep="")
  if(ref==60) {
    last_date <- dates[i]
    c <- "\\newline"
    d <- paste("\\caption{GympieNP ACI-ENT-EVN: ", first_date," to ", last_date, "}")
    e <- paste("\\end{figure}") 
    
    ref <- 0
  }
  commands <- c(commands, b, a, j, k, l, m, c, d, e)
  b <- NULL
  c <- NULL
  d <- NULL
  e <- NULL
}
last_date <- dates[i]
commands <- c(commands,
              paste("\\caption{GympieNP ACI-ENT-EVN: ", first_date," to ", last_date, "}"),
              paste("\\end{figure}"),
              paste(" ",sep = '\n'),
              paste("\\end{document}"))

# Save the commands to text file
fileConn <- file("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP\\GympieNP_tex_commands ACI-ENT-EVN.txt")
writeLines(commands, fileConn)
close(fileConn)

fileConn <- file("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP\\GympieNP_tex_commands ACI-ENT-EVN.txt")
writeLines(commands, fileConn)
close(fileConn)

# remove all objects in the global environment
rm(list = ls())

# This code goes before (latex) the commands generated above
#\documentclass[a4paper]{article} %  
#\usepackage[a4paper, portrait, margin=.15in]{geometry}
#\makeatletter
#\setlength{\@fptop}{0pt}
#\makeatother
#\usepackage{caption}
#\usepackage{graphicx}            
# INSERT COMMANDS

# Commands for Woondum ACI-ENT-EVN
commands <- paste("\\begin{document}")
commands <- c(commands, paste(" ",sep = '\n'))

ref <- 0
for(i in 1:length(dates)) {
  ref <- ref + 1
  b <- NULL
  if(ref==1) {
    b <- paste("\\begin{figure}") 
    first_date <- dates[i]
  }
  j <- "\\vspace*{-0.36cm}"  
  k <- "\\begin{minipage}{.08\\textwidth}"
  l <- paste("\\caption*{", dates[i], "}", sep = "")
  m <- "\\end{minipage}"
  
  
a <- paste("\\includegraphics[width=18.5cm]
{", dates[i], "/Woondum3_", dates[i],"__ACI-ENT-EVN_SpectralRibbon.png}", sep="")
  if(ref==60) {
    last_date <- dates[i]
    c <- "\\newline"
    d <- paste("\\caption{WoondumNP ACI-ENT-EVN: ", first_date," to ", last_date, "}")
    e <- paste("\\end{figure}") 
    
    ref <- 0
  }
  commands <- c(commands, b, a, j, k, l, m, c, d, e)
  b <- NULL
  c <- NULL
  d <- NULL
  e <- NULL
}
last_date <- dates[i]
commands <- c(commands,
              paste("\\caption{WoondumNP ACI-ENT-EVN: ", first_date," to ", last_date, "}"),
              paste("\\end{figure} "),
              paste(" ",sep = '\n'),
              paste("\\end{document}"))

fileConn<-file("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3\\Woondum_tex_commands ACI-ENT-EVN.txt")
writeLines(commands, fileConn)
close(fileConn)

# This code goes before (latex) the commands generated above
#\documentclass[a4paper]{article} %  
#\usepackage[a4paper, portrait, margin=.15in]{geometry}
#\makeatletter
#\setlength{\@fptop}{0pt}
#\makeatother
#\usepackage{caption}
#\usepackage{graphicx}            
# INSERT COMMANDS

# Commands for Gympie BGN-POW-SPT
commands <- paste("\\begin{document}")
commands <- c(commands, paste(" ",sep = '\n'))

ref <- 0
for(i in 1:length(dates)) {
  ref <- ref + 1
  b <- NULL
  if(ref==1) {
    b <- paste("\\begin{figure}") 
    first_date <- dates[i]
  }
  j <- "\\vspace*{-0.36cm}"  
  k <- "\\begin{minipage}{.08\\textwidth}"
  l <- paste("\\caption*{", dates[i], "}", sep = "")
  m <- "\\end{minipage}"
  
  
a <- paste("\\includegraphics[width=18.5cm]
{", dates[i], "/GympieNP_", dates[i],"__BGN-POW-SPT_SpectralRibbon.png}", sep="")
  if(ref==60) {
    last_date <- dates[i]
    c <- "\\newline"
    d <- paste("\\caption{GympieNP BGN-POW-SPT: ", first_date," to ", last_date, "}")
    e <- paste("\\end{figure}") 
    
    ref <- 0
  }
  commands <- c(commands, b, a, j, k, l, m, c, d, e)
  b <- NULL
  c <- NULL
  d <- NULL
  e <- NULL
}
last_date <- dates[i]
commands <- c(commands,
              paste("\\caption{GympieNP BGN-POW-SPT: ", first_date," to ", last_date, "}"),
              paste("\\end{figure} "),
              paste(" ",sep = '\n'),
              paste("\\end{document}"))

fileConn<-file("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\GympieNP\\GympieNP_tex_commands BGN-POW-SPT.txt")
writeLines(commands, fileConn)
close(fileConn)

# This code goes before (latex) the commands generated above
#\documentclass[a4paper]{article} %  
#\usepackage[a4paper, portrait, margin=.15in]{geometry}
#\makeatletter
#\setlength{\@fptop}{0pt}
#\makeatother
#\usepackage{caption}
#\usepackage{graphicx}            
# INSERT COMMANDS

# Commands for Woondum BGN-POW-SPT
commands <- paste("\\begin{document}")
commands <- c(commands, paste(" ",sep = '\n'))

ref <- 0
for(i in 1:length(dates)) {
  ref <- ref + 1
  b <- NULL
  if(ref==1) {
    b <- paste("\\begin{figure}") 
    first_date <- dates[i]
  }
j <- "\\vspace*{-0.36cm}"  
k <- "\\begin{minipage}{.08\\textwidth}"
l <- paste("\\caption*{", dates[i], "}", sep = "")
m <- "\\end{minipage}"

  
a <- paste("\\includegraphics[width=18.5cm]
{", dates[i], "/Woondum3_", dates[i],"__BGN-POW-SPT_SpectralRibbon.png}", sep="")
  if(ref==60) {
    last_date <- dates[i]
    c <- "\\newline"
    d <- paste("\\caption{WoondumNP BGN-POW-SPT: ", first_date," to ", last_date, "}")
    e <- paste("\\end{figure}") 
    
    ref <- 0
  }
  commands <- c(commands, b, a, j, k, l, m, c, d, e)
  b <- NULL
  c <- NULL
  d <- NULL
  e <- NULL
}
last_date <- dates[i]
commands <- c(commands,
              paste("\\caption{WoondumNP BGN-POW-SPT: ", first_date," to ", last_date, "}"),
              paste("\\end{figure} "),
              paste(" ",sep = '\n'),
              paste("\\end{document}"))

fileConn<-file("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concatOutput\\Woondum3\\Woondum_tex_commands BGN-POW-SPT.txt")
writeLines(commands, fileConn)
close(fileConn)

# This code goes before (latex) the commands generated above
#\documentclass[a4paper]{article} %  
#\usepackage[a4paper, portrait, margin=.15in]{geometry}
#\makeatletter
#\setlength{\@fptop}{0pt}
#\makeatother
#\usepackage{caption}
#\usepackage{graphicx}
# INSERT COMMANDS