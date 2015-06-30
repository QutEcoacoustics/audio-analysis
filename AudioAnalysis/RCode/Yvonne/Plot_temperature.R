setwd("D:\\Data\\")

sensorList <-read.csv("Sensor-A .txt", header = FALSE)

dateList <- substr(sensorList$V1, 1,11)
timeList <- substr(sensorList$V1, 13,20)
Temperature <- substr(sensorList$V1, 25,28)
plot(Temperature,type = "l")

