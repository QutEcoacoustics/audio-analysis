library(XML)

months <- c(6,7,8,9,10,11,12,1,2,3,4,5,6,7)
year <- c("2015", "2015", "2015", "2015", "2015", "2015", "2015", 
          "2016", "2016", "2016", "2016", "2016", "2016", "2016")

moon.data <- NULL
for (i in 1:length(months)) {
  moon <- paste("http://www.timeanddate.com/moon/australia/brisbane?month=", 
                months[i], "&year=", year[i],"", sep = "")
  moon.table <- readHTMLTable(moon, header=T, which=1,stringsAsFactors=F)[,1]    
  #library(R.utils)
  moon.table <- dataFrame(colClasses=c(a="numeric", b="numeric", c="numeric",d="numeric", 
                                       e="numeric", f="numeric", g="numeric", 
                                       h="numeric", i="numeric", j="numeric",
                                       k="numeric"),
                          ncol=11, nrow = length(moon.table))
  for(j in 1:10) {
    moon.table[,j] <- readHTMLTable(moon, header=T, which=1,stringsAsFactors=F)[,j]    
  }
  if(nchar(as.character(months[i]))==1) {
    month <- paste("0", months[i], sep = "")
  }
  month <- months[i]
  #colnames(moon.table) <- c("Date","Moonrise1","Moonrise2",
  #                          "Moonset1","Moonset2",
  #                          "Moonrise3","Moonrise4",
  #                         "Meridian Passing", "Meridian Dir",
  #                         "Distance(km)","Illumination")
  colnames(moon.table) <- c("Date","Moonrise1","Moonrise1dir",
                            "Moonset1", "Moonset1dir",
                            "Moonrise2", "Moonrise2dir", 
                            "Meridian Passing", 
                            "Meridian Dir", "Distance(km)",
                            "Illumination")
  moon.table$Date <- paste(moon.table$Date, "-", month, 
                         "-", year[i], sep = "")     
  for(j in 1:9) {
    moon.table$Date[j] <- paste("0", j, "-", month, 
                              "-", year[i], sep = "")     
  }
  # Adjust columns so that they line-up
  a <- which(moon.table$Moonrise1=="-")
  moon.table[a,3:11] <- moon.table[a,2:10]
  a <- which(moon.table$Moonset1=="-")
  moon.table[a,5:11] <- moon.table[a,4:9]
  a <- which(moon.table$Moonrise2=="-")
  moon.table[a,7:11] <- moon.table[a,6:10]
    
  moon.data <- rbind(moon.data, moon.table)
}
start <- as.POSIXct("2015-06-01")
end <- as.POSIXct("2016-07-31")

View(moon.data)
moon.data.copy <- moon.data
moon.data <- moon.data.copy

a <- which(nchar(as.character(moon.data$Moonrise1dir)) == 7)
moon.data$Moonrise1dir[a] <- substr(moon.data$Moonrise1dir[a],4,6)
a <- which(nchar(as.character(moon.data$Moonrise1dir)) == 8)
moon.data$Moonrise1dir[a] <- substr(moon.data$Moonrise1dir[a],4,7)
a <- which(nchar(as.character(moon.data$Moonset1dir)) == 7)
moon.data$Moonset1dir[a] <- substr(moon.data$Moonset1dir[a],4,6)
a <- which(nchar(as.character(moon.data$Moonset1dir)) == 8)
moon.data$Moonset1dir[a] <- substr(moon.data$Moonset1dir[a],4,7)
a <- which(nchar(as.character(moon.data$Moonrise2dir)) == 7)
moon.data$Moonrise2dir[a] <- substr(moon.data$Moonrise2dir[a],4,6)
a <- which(nchar(as.character(moon.data$Moonrise2dir)) == 8)
moon.data$Moonrise2dir[a] <- substr(moon.data$Moonrise2dir[a],4,7)

write.csv(moon.data, paste("data/Moon_phases",start,"_", 
                           end, ".csv", sep = ""), row.names = F)
