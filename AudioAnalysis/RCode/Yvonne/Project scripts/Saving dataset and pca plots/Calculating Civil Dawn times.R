# Calculation of civil dawn
####################################################
# Note: DO NOT USE THIS METHOD, go to 
# http://www.ga.gov.au/geodesy/astro/sunrise.jsp
# instead use sunrise_times.R code

###################################################

require(maptools)

## Loading required package: maptools
## Loading required package: sp
## Checking rgeos availability: TRUE

######################################## The example from maptools Location of Helsinki, Finland, in decimal
######################################## degrees, as listed in NOAA's website
# Helsinki, Finland example
hels <- matrix(c(24.97, 60.17), nrow = 1)
# Gympie, Australia example
gym <- matrix(c(152.74, -26.16), nrow = 1)

Hels <- SpatialPoints(hels, proj4string = CRS("+proj=longlat +datum=WGS84"))
Gym <- SpatialPoints(gym, proj4string = CRS("+proj=longlat +datum=WGS84 +ellps=WGS84"))
d041224 <- as.POSIXct("2016-05-27", tz = "Australia/Brisbane")
## Civil dawn
crepuscule(Gym, d041224, solarDep = 6, direction = "dawn", POSIXct.out = TRUE)

