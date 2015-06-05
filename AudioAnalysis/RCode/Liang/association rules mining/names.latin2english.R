names.latin2english <- function(latin.names){
  latin2english<-read.csv("C:\\Work\\myfile\\speciesNames_latin2english_20sites.csv", as.is=c(1,2))
  english.names <- character()
  for(i in 1:length(latin.names)){
    index <- which(latin2english[,1] == latin.names[i])
    english.names <- c(english.names, latin2english[index, 2])
  }
  return(english.names)
}