setwd("C:\\Users\\n0572527\\Desktop\\")

dataset <- read.csv("hybrid_clust_knn_17500_3_k20_2hour_full111days.csv",header=T)

dates <- unique(dataset$as.character.dates2.)
dataset$as.character.dates2. <- as.Date(dataset$as.character.dates2., format = "%d/%m/%Y")
dates1 <- unique(substring(dataset$as.character.dates2.,1,7))
site <- unique(dataset$site)
hour.period <- c("12-2am","2-4am","4-6am","6-8am","8-10am","10-12noon",
                 "12-2pm","2-4pm","4-6pm","6-8pm","8-10pm","10-12midnight")
hour.period <- c("01","02","03","04","05","06","07","08","09","10","11","12")


count <- 1
for(i in 1:length(dataset$V1)) {
  if(count == 13) {count <- 1}
  dataset$site.yr.mth[i] <- paste(dataset$site[i], substr(dataset$as.character.dates2.[i],1,7),
                                  hour.period[count],sep="_") 
  count <- count + 1
}

# determining number of days in each month at each site
days.per.month <- NULL
for (i in 1:length(site)) {
  for (j in 1:length(dates1)) {
    ref <- which(substr(dataset$as.character.dates2.,1,7) == dates1[j]
                 & dataset$site==site[i])
    days <- length(ref)/12  
    days.per.month <- c(days.per.month, days)  
  }
}
days.per.month <- rep(days.per.month, each=12)

per.month.per.period <- aggregate(dataset[,sapply(dataset,is.numeric)],dataset["site.yr.mth"],sum)
per.month.per.period <- per.month.per.period[c(2:length(per.month.per.period),1)]
                                                           
View(per.month.per.period)

for (i in 1:length(per.month.per.period$V1)) {
    per.month.per.period[i,1:(length(per.month.per.period)-1)] <- 
      per.month.per.period[i,1:(length(per.month.per.period)-1)]/days.per.month[i]
}

length_Gympie <- length(which(substr(per.month.per.period$site.yr.mth, 1,6)=="Gympie"))
length_Woondum <- length(which(substr(per.month.per.period$site.yr.mth, 1,7)=="Woondum"))

names <-c("V1 quiet with some insects ","V2 quiet with insects ","V3 birds ","V4 light rain",
          "V5 birds ","V6 very quiet ","V7 very quiet ","V8 Moderate wind",
          "V9 Moderate rain ","V10 birds ","V11 Wind ","V12 Planes and motorbikes ",
          "V13 birds","V14 Very light rain ","V15 Very very quiet ",
          "V16 Rain ","V17 Loud planes ","V18 ?? ","V19 ??Daytime quietness ",
          "V20 Thunder and kookaburras ")

# Gympie plots
for (i in 1:(length(per.month.per.period)-1)) {
  png(paste("Average minutes per month_Gympie_V",i,".png"), width=1000)
      par(mfrow=c(1,5), mar=c(5,2,1,1), oma=c(2,5,2,2), cex.axis=1.6)
      barplot(per.month.per.period[1:(length_Gympie/length(dates1)),i], beside=T,
              col="red", xlab="hours", ylim=c(0,(max(per.month.per.period[1:length_Gympie,i])+0.2)),
              main=substr(per.month.per.period$site.yr.mth[1],10,16))
      mtext(side=2,"Average number of minutes in two hour period",outer=T,line=0.6)
      axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
      par(new=F)
      barplot(per.month.per.period[(1+length_Gympie/length(dates1)):
                                     (2*(length_Gympie/length(dates1))),i],beside=T,
              col="red", ,xlab="hours", ylim=c(0,(max(per.month.per.period[1:length_Gympie,i])+0.2)),
              main=substr(per.month.per.period$site.yr.mth[13],10,16))
      axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
      par(new=F)
      barplot(per.month.per.period[(1+2*length_Gympie/length(dates1)):
                                     (3*(length_Gympie/length(dates1))),i],
              beside=T, col="red", ,xlab="hours", 
              ylim=c(0,(max(per.month.per.period[1:length_Gympie,i])+0.2)),
              main=substr(per.month.per.period$site.yr.mth[25],10,16))
      axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
      par(new=F)
      barplot(per.month.per.period[(1+3*length_Gympie/length(dates1)):
                                     (4*(length_Gympie/length(dates1))),i],
              beside=T, col="red", xlab="hours",
              ylim=c(0,(max(per.month.per.period[1:length_Gympie,i])+0.2)),
              main=substr(per.month.per.period$site.yr.mth[37],10,16))
      axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
      par(new=F)
      barplot(per.month.per.period[(1+4*length_Gympie/length(dates1)):
                                     (5*(length_Gympie/length(dates1))),i],
              beside=T, col="red", xlab="hours", 
              ylim=c(0,(max(per.month.per.period[1:length_Gympie,i])+0.2)),
              main=substr(per.month.per.period$site.yr.mth[49],10,16))
      axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
      mtext(side=3, paste(substr(per.month.per.period$site.yr.mth[1],1,8),names[i]),
            outer=T,line=0.6)  
      dev.off()
}

# Woondum plots
for (i in 1:(length(per.month.per.period)-1)) {
  png(paste("Average minutes per month_Woondum_V",i,".png"), width=1000)
  par(mfrow=c(1,5), mar=c(5,2,1,1), oma=c(2,5,2,2),cex.axis=1.6)
  barplot(per.month.per.period[(1+(5*length_Woondum/length(dates1))):(6*(length_Woondum/length(dates1))),i], 
          beside=T, col="red", xlab="hours", 
          ylim=c(0,(max(per.month.per.period[(1+length_Woondum):(length(per.month.per.period$V1)),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[61],11,17))
  mtext(side=2,"Average number of minutes in two hour period",outer=T,line=0.6)
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+(6*(length_Woondum/length(dates1)))):
                                 (7*(length_Woondum/length(dates1))),i],
          beside=T, col="red", ,xlab="hours", 
          ylim=c(0,(max(per.month.per.period[(1+length_Woondum):(length(per.month.per.period$V1)),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[73],11,17))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+7*length_Woondum/length(dates1)):
                                 (8*(length_Woondum/length(dates1))),i],
          beside=T, col="red", ,xlab="hours", 
          ylim=c(0,(max(per.month.per.period[(1+length_Woondum):(length(per.month.per.period$V1)),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[85],11,17))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+8*length_Woondum/length(dates1)):
                                 (9*(length_Woondum/length(dates1))),i],
          beside=T, col="red", xlab="hours",
          ylim=c(0,(max(per.month.per.period[(1+length_Woondum):(length(per.month.per.period$V1)),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[97],11,17))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+9*length_Woondum/length(dates1)):
                                 (10*(length_Woondum/length(dates1))),i],
          beside=T, col="red", xlab="hours", 
          ylim=c(0,(max(per.month.per.period[(1+length_Woondum):(length(per.month.per.period$V1)),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[109],11,17))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  mtext(side=3, paste(substr(per.month.per.period$site.yr.mth[(length_Woondum+1)],1,9),names[i]),
        outer=T,line=0.6)  
  dev.off()
}

write.csv(per.month.per.period,"Average_minutes_per_month.csv",row.names=F)

# Gympie and Woondum combination plots
for (i in 1:(length(per.month.per.period)-1)) {
  png(paste("Average minutes per month_Gympie_Woondum_V",i,".png"), width=1200,height=700)
  par(mfrow=c(2,5), mar=c(5,4,5,1), oma=c(2,5,2.5,2),cex.axis=1.6)
  barplot(per.month.per.period[1:(length_Gympie/length(dates1)),i], beside=T,
          col="red", xlab="hours", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[1],10,16))
  mtext(side=2,"Average number of minutes in two hour period",outer=T,line=0.6)
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+length_Gympie/length(dates1)):
                                 (2*(length_Gympie/length(dates1))),i],beside=T,
          col="red", ,xlab="hours", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[13],10,16))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+2*length_Gympie/length(dates1)):
                                 (3*(length_Gympie/length(dates1))),i],
          beside=T, col="red", ,xlab="hours", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[25],10,16))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+3*length_Gympie/length(dates1)):
                                 (4*(length_Gympie/length(dates1))),i],
          beside=T, col="red", xlab="hours",
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[37],10,16))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+4*length_Gympie/length(dates1)):
                                 (5*(length_Gympie/length(dates1))),i],
          beside=T, col="red", xlab="hours", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[49],10,16))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  mtext(side=3, paste(substr(per.month.per.period$site.yr.mth[1],1,8),names[i]),
        outer=T,line=-1.4)
  # Woondum plot
  barplot(per.month.per.period[(1+(5*length_Woondum/length(dates1))):(6*(length_Woondum/length(dates1))),i], 
          beside=T, col="red", xlab="hours", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[61],11,17))
  mtext(side=2,"Average number of minutes in two hour period",outer=T,line=0.6)
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+(6*(length_Woondum/length(dates1)))):
                                 (7*(length_Woondum/length(dates1))),i],
          beside=T, col="red", ,xlab="hours", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[73],11,17))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+7*length_Woondum/length(dates1)):
                                 (8*(length_Woondum/length(dates1))),i],
          beside=T, col="red", ,xlab="hours", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[85],11,17))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+8*length_Woondum/length(dates1)):
                                 (9*(length_Woondum/length(dates1))),i],
          beside=T, col="red", xlab="hours",
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[97],11,17))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  par(new=F)
  barplot(per.month.per.period[(1+9*length_Woondum/length(dates1)):
                                 (10*(length_Woondum/length(dates1))),i],
          beside=T, col="red", xlab="hours", 
          ylim=c(0,(max(per.month.per.period[1:length(per.month.per.period$V1),i])+0.2)),
          main=substr(per.month.per.period$site.yr.mth[109],11,17))
  axis(side=1, at=seq(0.2,16.4,2.39),labels=c(0,4,8,12,16,20,24), tick=F)
  mtext(side=3, line=-36, paste(substr(per.month.per.period$site.yr.mth[(length_Woondum+1)],1,9),names[i]),
        outer=T)  
  dev.off()
}




dev.off()
barplot(per.month.per.period[1:length_Gympie,2],beside=T,col="red",
        main="insects V1")
axis(side=1,at=at,labels=label.month,line=2)
axis(side=1,at=at1,labels=label.hour,cex=0.2)
barplot(per.month.per.period[1:length_Gympie,3],beside=T,col="red",
        main="insects V2")
barplot(per.month.per.period[1:length_Gympie,4],beside=T,col="red",
        main="birds V3")
barplot(per.month.per.period[1:length_Gympie,5],beside=T,col="red",
        main="light rain V4")
barplot(per.month.per.period[1:length_Gympie,6],beside=T,col="red",
        main="birds V5")
barplot(per.month.per.period[1:length_Gympie,7],beside=T,col="red",
        main="very quiet V6")
barplot(per.month.per.period[1:length_Gympie,8],beside=T,col="red",
        main="very quiet V7")
barplot(per.month.per.period[1:length_Gympie,9],beside=T,col="red",
        main="Moderate wind V8 GympieNP")
barplot(per.month.per.period[1:length_Gympie,10],beside=T,col="red",
        main="Moderate rain V9 GympieNP")
barplot(per.month.per.period[1:length_Gympie,11],beside=T,col="red",
        main="birds V10 GympieNP")
barplot(per.month.per.period[1:length_Gympie,12],beside=T,col="red",
        main="Wind V11 GympieNP")
barplot(per.month.per.period[1:length_Gympie,13],beside=T,col="red",
        main="Planes and motorbikes V12 GympieNP")
barplot(per.month.per.period[1:length_Gympie,14],beside=T,col="red",
        main="birds V13 GympieNP")
barplot(per.month.per.period[1:length_Gympie,15],beside=T,col="red",
        main="Very light rain V14 GympieNP")
barplot(per.month.per.period[1:length_Gympie,16],beside=T,col="red",
        main="Very very quiet V15 GympieNP")
barplot(per.month.per.period[1:length_Gympie,17],beside=T,col="red",
        main="Rain V16 GympieNP")
barplot(per.month.per.period[1:length_Gympie,18],beside=T,col="red",
        main="Loud planes V17 GympieNP")
barplot(per.month.per.period[1:length_Gympie,19],beside=T,col="red",
        main="?? V18 GympieNP")
barplot(per.month.per.period[1:length_Gympie,20],beside=T,col="red",
        main="??Daytime quietness V19 GympieNP")
barplot(per.month.per.period[1:length_Gympie,21],beside=T,col="red",
        main="Thunder and kookaburras V20 GympieNP")

barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),2],
        beside=T,col="white", main="insects V1 Woondum NP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),3],
        beside=T,main="insects V2 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),4],
        beside=T,main="birds V3")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),5],
        beside=T,main="light rain V4")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),6],
        beside=T,main="birds V5")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),7],
        beside=T,main="very quiet V6")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),8],
        beside=T,main="very quiet V7")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),9],
        beside=T,main="Moderate wind V8 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),10],
        beside=T,main="Moderate rain V9 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),11],
        beside=T,main="birds V10 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),12],
        beside=T,main="Wind V11 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),13],
        beside=T,main="Planes and motorbikes V12 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),14],
        beside=T,main="birds V13 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),15],
        beside=T,main="Very light rain V14 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),16],
        beside=T,main="Very very quiet V15 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),17],
        beside=T,main="Rain V16 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),18],
        beside=T,main="Loud planes V17 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),19],
        beside=T,main="?? V18 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),20],
        beside=T,main="??Daytime quietness V19 WoondumNP")
barplot(per.month.per.period[(length_Gympie+1):(length_Gympie+length_Woondum),21],
        beside=T,col="red",main="Thunder and kookaburras V20 WoondumNP")





barplot(per.month.per.period[13:48,6]),beside=T,col="red")
barplot(per.month.per.period[13:48,7]),beside=T,col="red")
barplot(per.month.per.period[13:48,8]),beside=T,col="red")
barplot(per.month.per.period[13:48,9]),beside=T,col="red")
barplot(per.month.per.period[13:48,11]),beside=T,col="red")
barplot(per.month.per.period[13:48,12]),beside=T,col="red")


