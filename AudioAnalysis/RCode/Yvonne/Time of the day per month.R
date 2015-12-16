# 30 November 2015
####################################
# This code produced a series of histograms per month as a 
# summary of the frequency of clusters per 2 hour time period
# per month
####################################
setwd("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3k")

dataset <- read.csv("hybrid_clust_knn_17500_3_k30_2hour_full111days.csv",header=T)

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

per.month.per.period <- aggregate(dataset[,sapply(dataset,is.numeric)],
                                  dataset["site.yr.mth"],sum)
per.month.per.period <- per.month.per.period[c(2:length(per.month.per.period),1)]
                                                           
#View(per.month.per.period)

for (i in 1:length(per.month.per.period$V1)) {
    per.month.per.period[i,1:(length(per.month.per.period)-1)] <- 
      per.month.per.period[i,1:(length(per.month.per.period)-1)]/days.per.month[i]
}

length_Gympie <- length(which(substr(per.month.per.period$site.yr.mth, 1,6)=="Gympie"))
length_Woondum <- length(which(substr(per.month.per.period$site.yr.mth, 1,7)=="Woondum"))

names <-c("V1 some insects + wind + birds","V2 wind","V3 fairly quiet",
          "V4 birds (afternoon)","V5 quiet + some insects","V6 rain",
          "V7 insects","V8 birds (midday)",
          "V9 birds (morning)","V10 wind + birds",
          "V11 quiet + planes","V12 planes + some birds",
          "V13 louder planes + birds","V14 birds (morning)",
          "V15 wind","V16 birds + wind","V17 rain",
          "V18 birds + wind","V19 quiet + some insects","V20 quiet + birds + insects",
          "V21 breezes","V22 birds (afternoon)", 
          "V23 birds (morning)", "V24 very quiet", 
          "V25 quiet","V26 birds (mid-morning)",
         "V27 Thunder and kookaburras","V28 quiet + some birds", 
         "V29 drizzle + birds + insects", "V30 insects")


#names <-c("V1 Slight wind","V2 Slight wind + insects","V3 insects",
#          "V4 quiet + insects","V5 light rain","V6 very quiet",
#          "V7 Rain","V8 quiet + some insects",
#          "V9 wind + birds","V10 birds (morning)",
#          "V11 planes","V12 birds (morning)",
#          "V13 quieter planes","V14 wind + insects + birds",
#          "V15 wind + birds","V16 Birds (morning)","V17 Wind",
#          "V18 birds + wind","V19 rain","V20 Mid frequency birds",
#          "V21 quiet + some birds","V22 quiet + some insects + birds", 
#          "V23 birds + wind", "V24 Very quiet", 
#          "V25 Thunder and kookaburras","V26 Birds",
#          "V27 Birds","V28 birds + insects", "V29 birds (morning)",
#          "V30 Wind + birds")

#names <-c("V1 Very quiet birds","V2 Very quiet birds2","V3 Birds!",
#          "V4 birds","V5 ??","V6 Birds!","V7 Insects","V8 Quiet",
#          "V9 Wind","V10 Birds!","V11 Quiet+insects","V12 Rain",
#          "V13 Birds!","V14 Quiet_insects","V15 Wind+birds",
#          "V16 Wind+birds ","V17 Quiet+birds","V18 Quieter birds",
#          "V19 Quiet+birds","V20 Birds!","V21 Drizzle",
#          "V22 Moderate wind+birds", "V23 Rain", 
#          "V24 Lightwind+birds", "V25 Wind+insects",
#          "V26 Planes + motorbikes","V27 Moderate wind+birds",
#          "V28 Mid-afternoon birds", "V29 Very quiet",
#          "V30 Thunder and kookaburras")

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
                                 (2*(length_Gympie/length(dates1))),i], 
          beside=T,
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