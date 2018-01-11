# Prepare dates
rm(list = ls())
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20160723", format="%Y%m%d")
#* will need to be replaced with " later

dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}
dates <- date.list
rm(date.list)
dates <- paste(substr(dates,1,4),
               substr(dates,6,7),
               substr(dates,9,10), sep="")

# Part I
commands <- "Replace * with one set of inverted commas"
for(i in 1:length(dates)) {
  date <- dates[i]
  a <- paste("  </div>")
  b <- paste("    <div class=", "*row*", ">", sep="")
  c <- paste("  <img src=*",dates[i], "/GympieNP", "_",dates[i], "__ACI-ENT-EVN_SpectralRibbon.png*", 
             " style=", "*width:100%* ","onclick=", 
             "*openModal();currentSlide(", i, ")* ", 
             "class=*hover-shadow cursor*>",sep="")
  commands <- c(commands, a,b,c)
}

# Save the output to text file
fileConn <- file("Script_for_html_code_partI.txt")
writeLines(commands, fileConn)
close(fileConn)

# Part II
commands <- "Replace * with one set of inverted commas"
for(i in 1:length(dates)) {
  date <- dates[i]
  a <- paste("<div class=*mySlides*>")
  b <- paste("<div class=*numbertext*>", i, " / ",length(dates),"</div>", sep="")
  c <- paste("<img src=*", dates[i],"/GympieNP_",dates[i],"__ACI-ENT-EVN.png* style=*width:100%*>", sep="")
  d <- paste("</div>")
  commands <- c(commands, a,b,c,d)
}

fileConn <- file("Script_for_html_code_partII.txt")
writeLines(commands, fileConn)
close(fileConn)

# Part III
commands <- "Replace * with one set of inverted commas"
for(i in 1:length(dates)) {
  date <- dates[i]
  a <- paste("<div class=*column*>")
  if(substr(dates[i],5,6)=="01") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","January ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="02") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","February ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="03") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","March ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="04") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","April ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="05") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","May ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="06") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","June ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="07") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","July ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="08") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","August ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="09") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","September ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="10") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","October ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="11") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","November ",substr(dates[i],1,4),"*>", sep="")
  }
  if(substr(dates[i],5,6)=="12") {
    b <- paste("<img class=*demo cursor* src=*img_nature_wide.jpg* style=*width:100%* onclick=*currentSlide(", i, ")* alt=*", substr(dates[i],7,8)," ","December ",substr(dates[i],1,4),"*>", sep="")
  }
  c <- paste("</div>")
  commands <- c(commands, a,b,c)
}

fileConn <- file("Script_for_html_code_partIII.txt")
writeLines(commands, fileConn)
close(fileConn)

