
daylight<-read.csv("c:/work/myfile/sunrise_sunset.csv")
# daylight<-read.csv("c:/work/myfile/sunrise&sunset.csv")
daylight<-as.matrix(daylight)

#column 2 is sunrise, column 3 is sunset
# char.sunrise <- daylight[ , 2]
# char.sunset <- daylight[ , 3]
# ast.start <- daylight[ , 5]
# naut.start <- daylight[ , 7]
civil.start <- daylight[ , 9]
# ast.end <- daylight[ , 6]
# naut.end <- daylight[ , 8]
civil.end <- daylight[ , 10]
# civil.start <- daylight[ , 6]
# civil.end <- daylight[ , 7]

#regular expression to find the 
for(cnt in 1:length(civil.start)){
#   char.sunrise[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", char.sunrise[cnt])
#   char.sunset[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", char.sunset[cnt])
#   ast.start[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", ast.start[cnt])
#   naut.start[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", naut.start[cnt])
  civil.start[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", civil.start[cnt])
#   ast.end[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", ast.end[cnt])
#   naut.end[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", naut.end[cnt])
  civil.end[cnt] <- sub("([[:digit:]]).([[:digit:]]+).*", "\\1\\2", civil.end[cnt])
}
# sunrise <- as.numeric(char.sunrise)
# sunrise <- sunrise %/% 100 * 60 + sunrise %% 100
# 
# sunset <- as.numeric(char.sunset)
# sunset <- sunset %/% 100 * 60 + sunset %% 100
# 
# ast.s <- as.numeric(ast.start)
# ast.s <- ast.s %/% 100 * 60 + ast.s %% 100
# 
# naut.s <- as.numeric(naut.start)
# naut.s <- naut.s %/% 100 * 60 + naut.s %% 100

civil.s <- as.numeric(civil.start)
civil.s <- civil.s %/% 100 * 60 + civil.s %% 100

# ast.e <- as.numeric(ast.end)
# ast.e <- ast.e %/% 100 * 60 + ast.e %% 100
# 
# naut.e <- as.numeric(naut.end)
# naut.e <- naut.e %/% 100 * 60 + naut.e %% 100

civil.e <- as.numeric(civil.end)
civil.e <- civil.e %/% 100 * 60 + civil.e %% 100