library(XML)

months <- c(6,7,8,9,10,11,12,1,2,3,4,5,6,7)
year <- c("2015", "2015", "2015", "2015", "2015", "2015", "2015", 
          "2016", "2016", "2016", "2016", "2016", "2016", "2016")
solar.data <- NULL
for (i in 1:length(months)) {
  solar <- paste("http://www.timeanddate.com/sun/australia/brisbane?month=", months[i], "&year=", 
  year[i],"", sep = "")
  solar.table <- readHTMLTable(solar, header=T, which=1,stringsAsFactors=F)  
  solar.data <- rbind(solar.data, solar.table)
}

View(solar.data)

# generate a sequence of dates
start <- as.POSIXct("2015-06-01")
end <- as.POSIXct("2016-07-31")
interval <- 1440

dates <- seq(from=start, by=interval*60, to=end)

solar.data$V1 <- dates

headings <- c("Date", "Sunrise", "Sunset", "Daylength", "Daylength_difference",
              "Astron_Sunrise", "Astron_Sunset", "Naut_Sunrise", "Naut_Sunset", 
              "Civil_Sunrise", "Civil_Sunset", "Solar-Noon", "Million_km")
colnames(solar.data) <- headings

write.csv(solar.data, "Sunrise_Sunset_Solar Noon.csv", row.names = F)
