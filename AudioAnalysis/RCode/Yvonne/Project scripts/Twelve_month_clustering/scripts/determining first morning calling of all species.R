rm(list = ls())
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20150816", format="%Y%m%d")
# Prepare civil dawn, civil dusk and sunrise and sunset times
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2015 <- civil_dawn_2015[173:228, ]
civil_sunrise <- as.numeric(substr(civil_dawn_2015$CivSunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise,2,3))
sunrise <- as.numeric(substr(civil_dawn_2015$Sunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$Sunrise,2,3))
# Prepare dates
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

kalscpe_data <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\all_data_added_protected.csv", header=T)
kalscpe_data <- kalscpe_data[,2:39]
kalscpe_data <- read.csv("all_data_added_protected.csv", header=T)

species <- c("EYR","WTT","WTH","KOOK","SC1","SC2","EW")
statistics <- data.frame(date="20150622",
                         V1=0)
statistics$date <- as.character(statistics$date)
statistics <- data.frame(statistics)
for(i in 1:length(dates)) {
  for(j in 1:length(species)) {
    sp <- species[j]
    species_labels <- NULL
    if(sp=="EYR") {
      species_labels <- c("EYR Near", "EYR Mod", "EYR Far")
    }
    if(sp=="WTT") {
      species_labels <- c("WTT trill Near", "WTT trill Mod", "WTT trill Far")
    }
    if(sp=="KOOK") {
      species_labels <- c("Kookaburra")
    }
    if(sp=="WTH") {
      species_labels <- c("WTH Near", "WTH Mod", "WTH Far")
    }
    if(sp=="SC1") {
      species_labels <- c("SC Near", "SC Mod", "SC Far")
    }
    if(sp=="SC2") {
      species_labels <- c("SC Chatter Near", "SC Chatter Mod", "SC Chatter Far")
    }
    if(sp=="EW") {
      species_labels <- c("EW Near", "EW Mod", "EW Far")
    }
    list <- NULL
    a1 <- NULL
    a2 <- NULL
    for(k in 1:length(species_labels)) {
      a1 <- grep(species_labels[k], kalscpe_data$MANUAL_ID, ignore.case = T)
      list <- c(list, a1)
    }
    temp_kalscpe_data <- kalscpe_data[list,]
    a2 <- grep(dates[i], temp_kalscpe_data$IN.FILE)
    if(length(a2)>0) {
      min <- min(temp_kalscpe_data$OFFSET[a2])
      statistics[i,1] <- dates[i]
      statistics[i,(j+1)] <- min
    }
    if(length(a2)==0) {
      statistics[i,1] <- dates[i]
      statistics[i,(j+1)] <- NA
    }
  }
}
colnames(statistics) <- c("dates", species)
#View(statistics)
#plot(statistics$EYR)
#plot(statistics$WTT)
#plot(statistics$WTH)
#plot(statistics$KOOK)
#plot(statistics$SC1)
#plot(statistics$SC2)
#plot(statistics$EW)
statistics$EYR_min <- statistics$EYR/60
statistics$WTT_min <- statistics$WTT/60
statistics$WTH_min <- statistics$WTH/60
statistics$KOOK_min <- statistics$KOOK/60
statistics$SC1_min <- statistics$SC1/60
statistics$SC2_min <- statistics$SC2/60
statistics$EW_min <- statistics$EW/60
statistics$civil_dawn <- civil_sunrise
statistics$sunrise <- sunrise
#statistics$EYR_min_diff <- statistics$EYR_min - statistics$sunrise
#statistics$WTT_min_diff <- statistics$WTT_min - statistics$sunrise
#statistics$WTH_min_diff <- statistics$WTH_min - statistics$sunrise
#statistics$KOOK_min_diff <- statistics$KOOK_min - statistics$sunrise
#statistics$SC1_min_diff <- statistics$SC1_min - statistics$sunrise
#statistics$SC2_min_diff <- statistics$SC2_min - statistics$sunrise
#statistics$EW_min_diff <- statistics$EW_min - statistics$sunrise

statistics$EYR_min_diff <- statistics$EYR_min - statistics$civil_dawn
statistics$WTT_min_diff <- statistics$WTT_min - statistics$civil_dawn
statistics$WTH_min_diff <- statistics$WTH_min - statistics$civil_dawn
statistics$KOOK_min_diff <- statistics$KOOK_min - statistics$civil_dawn
statistics$SC1_min_diff <- statistics$SC1_min - statistics$civil_dawn
statistics$SC2_min_diff <- statistics$SC2_min - statistics$civil_dawn
statistics$EW_min_diff <- statistics$EW_min - statistics$civil_dawn

list2 <- -55:55
par(mfrow=c(7,1), mar=c(0,0,1,0),
    oma=c(2,0,0,0))

boxplot(statistics$EYR_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n",
        border="black")
med <- fivenum(statistics$EYR_min_diff)[3]
text(x=med, y=0.65, as.character(round(med),0), cex=1.4)
legend <- "EYR"
legend(x=(42-nchar(legend)),y=1.5, legend=legend, bty = "n", cex=2)
abline(v=c(-25,-15,-5, 5, 15, 25))

boxplot(statistics$WTH_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$WTH_min_diff)[3]
text(x=med, y=0.65, as.character(round(med,0)), cex=1.4)
legend <- "WTH"
legend(x=(42-nchar(legend)),y=1.5, legend=legend, bty = "n", cex=2)
abline(v=c(-25,-15,-5, 5, 15, 25))

boxplot(statistics$KOOK_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$KOOK_min_diff)[3]
text(x=med, y=0.65, as.character(round(med,0)), cex=1.4)
legend <- "KOOK"
legend(x=(42-nchar(legend)),y=1.5, legend=legend, bty = "n", cex=2)
abline(v=c(-25,-15,-5, 5, 15, 25))

boxplot(statistics$SC1_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$SC1_min_diff)[3]
text(x=med, y=0.65, as.character(round(med,0)), cex=1.4)
legend <- "SC1"
legend(x=(42-nchar(legend)),y=1.5, legend=legend, bty = "n", cex=2)
abline(v=c(-25,-15,-5, 5, 15, 25))

boxplot(statistics$SC2_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$SC2_min_diff)[3]
text(x=med, y=0.65, as.character(round(med,0)), cex=1.4)
legend <- "SC2"
legend(x=(42-nchar(legend)),y=1.5, legend=legend, bty = "n", cex=2)
abline(v=c(-25,-15,-5, 5, 15, 25))

boxplot(statistics$EW_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$EW_min_diff)[3]
text(x=med, y=0.65, as.character(round(med,0)), cex=1.4)
legend <- "EW"
legend(x=(42-nchar(legend)),y=1.5, legend=legend, bty = "n", cex=2)
abline(v=c(-25,-15,-5, 5, 15, 25))

boxplot(statistics$WTT_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)))
med <- fivenum(statistics$WTT_min_diff)[3]
text(x=med, y=0.65, as.character(round(med),0), cex=1.4)
legend <- "WTT"
legend(x=(42-nchar(legend)),y=1.5, legend=legend, bty = "n", cex=2)
abline(v=c(-25,-15,-5, 5, 15, 25))

(EYR_av <- mean(statistics$EYR_min_diff, na.rm=T))
(EYR_sd <- sd(statistics$EYR_min_diff, na.rm=T))
(EYR_earliest_start <- min(statistics$EYR_min_diff, na.rm=T))
(EYR_latest_start <- max(statistics$EYR_min_diff, na.rm=T))
(EYR_count <- length(which(statistics$EYR_min_diff < 0)|which(statistics$EYR_min_diff<0)))

(WTT_av <- mean(statistics$WTT_min_diff, na.rm=T))
(WTT_sd <- sd(statistics$WTT_min_diff, na.rm=T))
(WTT_earliest_start <- min(statistics$WTT_min_diff, na.rm=T))
(WTT_latest_start <- max(statistics$WTT_min_diff, na.rm=T))
(WTT_count <- length(which(statistics$WTT_min_diff < 0))
  + length(which(statistics$WTT_min_diff > 0)))

(WTH_av <- mean(statistics$WTH_min_diff, na.rm=T))
(WTH_sd <- sd(statistics$WTH_min_diff, na.rm=T))
(WTH_earliest_start <- min(statistics$WTH_min_diff, na.rm=T))
(WTH_latest_start <- max(statistics$WTH_min_diff, na.rm=T))
(WTH_count <- length(which(statistics$WTH_min_diff < 0))
  + length(which(statistics$WTH_min_diff > 0)))

(KOOK_av <- mean(statistics$KOOK_min_diff, na.rm=T))
(KOOK_sd <- sd(statistics$KOOK_min_diff, na.rm=T))
(KOOK_earliest_start <- min(statistics$KOOK_min_diff, na.rm=T))
(KOOK_latest_start <- max(statistics$KOOK_min_diff, na.rm=T))
(KOOK_count <- length(which(statistics$KOOK_min_diff < 0))
  + length(which(statistics$KOOK_min_diff > 0)))

(SC1_av <- mean(statistics$SC1_min_diff, na.rm=T))
(SC1_sd <- sd(statistics$SC1_min_diff, na.rm=T))
(SC1_earliest_start <- min(statistics$SC1_min_diff, na.rm=T))
(SC1_latest_start <- max(statistics$SC1_min_diff, na.rm=T))
(SC1_count <- length(which(statistics$SC1_min_diff < 0))
  + length(which(statistics$SC1_min_diff > 0)))

(SC2_av <- mean(statistics$SC2_min_diff, na.rm=T))
(SC2_sd <- sd(statistics$SC2_min_diff, na.rm=T))
(SC2_earliest_start <- min(statistics$SC2_min_diff, na.rm=T))
(SC2_latest_start <- max(statistics$SC2_min_diff, na.rm=T))
(SC2_count <- length(which(statistics$SC2_min_diff < 0))
  + length(which(statistics$SC2_min_diff > 0)))

(EW_av <- mean(statistics$EW_min_diff, na.rm=T))
(EW_sd <- sd(statistics$EW_min_diff, na.rm=T))
(EW_earliest_start <- min(statistics$EW_min_diff, na.rm=T))
(EW_latest_start <- max(statistics$EW_min_diff, na.rm=T))
(EW_count <- length(which(statistics$EW_min_diff < 0))
  + length(which(statistics$EW_min_diff > 0)))

# Table of time of calling before SUNRISE
table <- data.frame(species="species",
                    mean=0,
                    sd=0,
                    earliest=0,
                    latest=0,
                    count=0)
table <- data.frame(table)
table$species <- as.character(table$species)
table[1,] <- c("EYR", round(EYR_av), round(EYR_sd), round(EYR_earliest_start), 
               round(EYR_latest_start), round(EYR_count))
table[2,] <- c("WTH", round(WTH_av), round(WTH_sd), round(WTH_earliest_start), 
               round(WTH_latest_start), round(WTH_count))
table[4,] <- c("SC1", round(SC1_av), round(SC1_sd), round(SC1_earliest_start), 
               round(SC1_latest_start), round(SC1_count))
table[5,] <- c("SC2", round(SC2_av), round(SC2_sd), round(SC2_earliest_start), 
               round(SC2_latest_start), round(SC2_count))
table[7,] <- c("WTT", round(WTT_av), round(WTT_sd), round(WTT_earliest_start), 
               round(WTT_latest_start), round(WTT_count))
table[3,] <- c("KOOK", round(KOOK_av), round(KOOK_sd), round(KOOK_earliest_start), 
               round(KOOK_latest_start), round(KOOK_count))
table[6,] <- c("EW", round(EW_av), round(EW_sd), round(EW_earliest_start), 
               round(EW_latest_start), round(EW_count))
View(table)
# Work out the percentage of the events containing the EYR with different durations
a1 <- NULL
a2 <- NULL
a1 <- grep("EYR", kalscpe_data$MANUAL_ID, ignore.case = T)
temp_kalscpe_data <- kalscpe_data[a1,]

a2 <- which(temp_kalscpe_data$DURATION < 0.72)
a2 <- which(temp_kalscpe_data$DURATION > 3.8)
0.72 #(71.3%)
3.8 #(7.2%)

# Determine the species and the number of species per cluster
data_bird <- read.csv("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\species_each_minute_protected_final_with_species_numbers_sorted_by_num_spec.csv", header=T)
data_bird <- data.frame(data_bird)
#View(data_bird)
a <- which(data_bird$minute_reference < 405)
temp_data_bird <- data_bird[a,]
temp_data_bird$Rain <- as.numeric(temp_data_bird$Rain)

a1 <- which(temp_data_bird$cluster_list==1)
temp_data_bird_1 <- temp_data_bird[a1,]
a2 <- which(temp_data_bird$cluster_list==2)
temp_data_bird_2 <- temp_data_bird[a2,]
a3 <- which(temp_data_bird$cluster_list==3)
temp_data_bird_3 <- temp_data_bird[a3,]
a4 <- which(temp_data_bird$cluster_list==4)
temp_data_bird_4 <- temp_data_bird[a4,]
a5 <- which(temp_data_bird$cluster_list==5)
temp_data_bird_5 <- temp_data_bird[a5,]
a6 <- which(temp_data_bird$cluster_list==6)
temp_data_bird_6 <- temp_data_bird[a6,]
a7 <- which(temp_data_bird$cluster_list==7)
temp_data_bird_7 <- temp_data_bird[a7,]
a8 <- which(temp_data_bird$cluster_list==8)
temp_data_bird_8 <- temp_data_bird[a8,]
a9 <- which(temp_data_bird$cluster_list==9)
temp_data_bird_9 <- temp_data_bird[a9,]
a10 <- which(temp_data_bird$cluster_list==10)
temp_data_bird_10 <- temp_data_bird[a10,]
a11 <- which(temp_data_bird$cluster_list==11)
temp_data_bird_11 <- temp_data_bird[a11,]
a12 <- which(temp_data_bird$cluster_list==12)
temp_data_bird_12 <- temp_data_bird[a12,]
a13 <- which(temp_data_bird$cluster_list==13)
temp_data_bird_13 <- temp_data_bird[a13,]
a14 <- which(temp_data_bird$cluster_list==14)
temp_data_bird_14 <- temp_data_bird[a14,]
a15 <- which(temp_data_bird$cluster_list==15)
temp_data_bird_15 <- temp_data_bird[a15,]
a16 <- which(temp_data_bird$cluster_list==16)
temp_data_bird_16 <- temp_data_bird[a16,]
a17 <- which(temp_data_bird$cluster_list==17)
temp_data_bird_17 <- temp_data_bird[a17,]
a18 <- which(temp_data_bird$cluster_list==18)
temp_data_bird_18 <- temp_data_bird[a18,]
a19 <- which(temp_data_bird$cluster_list==19)
temp_data_bird_19 <- temp_data_bird[a19,]
a20 <- which(temp_data_bird$cluster_list==20)
temp_data_bird_20 <- temp_data_bird[a20,]
a21 <- which(temp_data_bird$cluster_list==21)
temp_data_bird_21 <- temp_data_bird[a21,]
a22 <- which(temp_data_bird$cluster_list==22)
temp_data_bird_22 <- temp_data_bird[a22,]
a23 <- which(temp_data_bird$cluster_list==23)
temp_data_bird_23 <- temp_data_bird[a23,]
a24 <- which(temp_data_bird$cluster_list==24)
temp_data_bird_24 <- temp_data_bird[a24,]
a25 <- which(temp_data_bird$cluster_list==25)
temp_data_bird_25 <- temp_data_bird[a25,]
a26 <- which(temp_data_bird$cluster_list==26)
temp_data_bird_26 <- temp_data_bird[a26,]
a27 <- which(temp_data_bird$cluster_list==27)
temp_data_bird_27 <- temp_data_bird[a27,]
a28 <- which(temp_data_bird$cluster_list==28)
temp_data_bird_28 <- temp_data_bird[a28,]
a29 <- which(temp_data_bird$cluster_list==29)
temp_data_bird_29 <- temp_data_bird[a29,]
a30 <- which(temp_data_bird$cluster_list==30)
temp_data_bird_30 <- temp_data_bird[a30,]
a31 <- which(temp_data_bird$cluster_list==31)
temp_data_bird_31 <- temp_data_bird[a31,]
a32 <- which(temp_data_bird$cluster_list==32)
temp_data_bird_32 <- temp_data_bird[a32,]
a33 <- which(temp_data_bird$cluster_list==33)
temp_data_bird_33 <- temp_data_bird[a33,]
a34 <- which(temp_data_bird$cluster_list==34)
temp_data_bird_34 <- temp_data_bird[a34,]
a35 <- which(temp_data_bird$cluster_list==35)
temp_data_bird_35 <- temp_data_bird[a35,]
a36 <- which(temp_data_bird$cluster_list==36)
temp_data_bird_36 <- temp_data_bird[a36,]
a37 <- which(temp_data_bird$cluster_list==37)
temp_data_bird_37 <- temp_data_bird[a37,]
a38 <- which(temp_data_bird$cluster_list==38)
temp_data_bird_38 <- temp_data_bird[a38,]
a39 <- which(temp_data_bird$cluster_list==39)
temp_data_bird_39 <- temp_data_bird[a39,]
a40 <- which(temp_data_bird$cluster_list==40)
temp_data_bird_40 <- temp_data_bird[a40,]
a41 <- which(temp_data_bird$cluster_list==41)
temp_data_bird_41 <- temp_data_bird[a41,]
a42 <- which(temp_data_bird$cluster_list==42)
temp_data_bird_42 <- temp_data_bird[a42,]
a43 <- which(temp_data_bird$cluster_list==43)
temp_data_bird_43 <- temp_data_bird[a43,]
a44 <- which(temp_data_bird$cluster_list==44)
temp_data_bird_44 <- temp_data_bird[a44,]
a45 <- which(temp_data_bird$cluster_list==45)
temp_data_bird_45 <- temp_data_bird[a45,]
a46 <- which(temp_data_bird$cluster_list==46)
temp_data_bird_46 <- temp_data_bird[a46,]
a47 <- which(temp_data_bird$cluster_list==47)
temp_data_bird_47 <- temp_data_bird[a47,]
a48 <- which(temp_data_bird$cluster_list==48)
temp_data_bird_48 <- temp_data_bird[a48,]
a49 <- which(temp_data_bird$cluster_list==49)
temp_data_bird_49 <- temp_data_bird[a49,]
a50 <- which(temp_data_bird$cluster_list==50)
temp_data_bird_50 <- temp_data_bird[a50,]
a51 <- which(temp_data_bird$cluster_list==51)
temp_data_bird_51 <- temp_data_bird[a51,]
a52 <- which(temp_data_bird$cluster_list==52)
temp_data_bird_52 <- temp_data_bird[a52,]
a53 <- which(temp_data_bird$cluster_list==53)
temp_data_bird_53 <- temp_data_bird[a53,]
a54 <- which(temp_data_bird$cluster_list==54)
temp_data_bird_54 <- temp_data_bird[a54,]
a55 <- which(temp_data_bird$cluster_list==55)
temp_data_bird_55 <- temp_data_bird[a55,]
a56 <- which(temp_data_bird$cluster_list==56)
temp_data_bird_56 <- temp_data_bird[a56,]
a57 <- which(temp_data_bird$cluster_list==57)
temp_data_bird_57 <- temp_data_bird[a57,]
a58 <- which(temp_data_bird$cluster_list==58)
temp_data_bird_58 <- temp_data_bird[a58,]
a59 <- which(temp_data_bird$cluster_list==59)
temp_data_bird_59 <- temp_data_bird[a59,]
a60 <- which(temp_data_bird$cluster_list==60)
temp_data_bird_60 <- temp_data_bird[a60,]

table <- data.frame(species="species",
                    total_calls=0,
                    clust1=0,
                    clust2=0,
                    clust3=0,
                    clust4=0,
                    clust5=0,
                    clust6=0,
                    clust7=0,
                    clust8=0,
                    clust9=0,
                    clust10=0,
                    clust11=0,
                    clust12=0,
                    clust13=0,
                    clust14=0,
                    clust15=0,
                    clust16=0,
                    clust17=0,
                    clust18=0,
                    clust19=0,
                    clust20=0,
                    clust21=0,
                    clust22=0,
                    clust23=0,
                    clust24=0,
                    clust25=0,
                    clust26=0,
                    clust27=0,
                    clust28=0,
                    clust29=0,
                    clust30=0,
                    clust31=0,
                    clust32=0,
                    clust33=0,
                    clust34=0,
                    clust35=0,
                    clust36=0,
                    clust37=0,
                    clust38=0,
                    clust39=0,
                    clust40=0,
                    clust41=0,
                    clust42=0,
                    clust43=0,
                    clust44=0,
                    clust45=0,
                    clust46=0,
                    clust47=0,
                    clust48=0,
                    clust49=0,
                    clust50=0,
                    clust51=0,
                    clust52=0,
                    clust53=0,
                    clust54=0,
                    clust55=0,
                    clust56=0,
                    clust57=0,
                    clust58=0,
                    clust59=0,
                    clust60=0,
                    total=0)
table$species <- as.character(table$species)
table <- data.frame(table)

source("scripts\\EasternYellowRobin.R") # row 1
source("scripts\\LaughingKookaburra.R") # row 2
source("scripts\\ScarletHoneyeater1.R") # row 3
source("scripts\\ScarletHoneyeater2.R") # row 4
source("scripts\\WhiteThroatedHoneyeater.R") # row 5
source("scripts\\WhiteThroatedTreecreeper.R") # row 6
source("scripts\\EasternWhipbird.R") # row 7
source("scripts\\TorresianCrow.R") # row 8
source("scripts\\PiedCurrawong.R") # row 9
source("scripts\\RainbowLorikeet.R") # row 10
source("scripts\\RufousWhistler.R") # row 11
source("scripts\\SouthernBoobook.R") # row 12
source("scripts\\PowerfulOwl.R") # row 13
source("scripts\\WhiteThroatedNightjar.R") # row 14
source("scripts\\Rain.R") # row 15
source("scripts\\AnimalMovement.R") # row 16

source("scripts\\EasternYellowRobin_FAR.R") # row 17
source("scripts\\LaughingKookaburra_FAR.R") # row 18
source("scripts\\ScarletHoneyeater1_FAR.R") # row 19
source("scripts\\ScarletHoneyeater2_FAR.R") # row 20
source("scripts\\WhiteThroatedHoneyeater_FAR.R") # row 21
source("scripts\\WhiteThroatedTreecreeper_FAR.R") # row 22
source("scripts\\EasternWhipbird_FAR.R") # row 23
View(table[,c(1,2,5,13,16,17,35,39,41,45,60)])
View(table)
for(i in 2:62) {
  table[ ,i] <- as.numeric(table[ ,i])
}
str(table)
for(i in 1:nrow(table)) {
  table$total_3_58[i] <- sum(table[i,5],table[i,13],
                             table[i,16],table[i,17],table[i,35],
                             table[i,39],table[i,41],table[i,45],
                             table[i,60])
}
View(table[,c(1,2,5,13,16,17,35,39,41,45,60,64)])

write.csv(table, "data\\Cluster_species_percentages.csv", row.names = F)
N_3 <- which(temp_data_bird$cluster_list==3)
length(N_3)
N_11 <- which(temp_data_bird$cluster_list==11)
length(N_11)
N_14 <- which(temp_data_bird$cluster_list==14)
length(N_14)
N_15 <- which(temp_data_bird$cluster_list==15)
length(N_15)
N_33 <- which(temp_data_bird$cluster_list==33)
length(N_33)

N_37 <- which(temp_data_bird$cluster_list==37)
length(N_37)
N_39 <- which(temp_data_bird$cluster_list==39)
length(N_39)
N_43 <- which(temp_data_bird$cluster_list==43)
length(N_43)
N_58 <- which(temp_data_bird$cluster_list==58)
length(N_58)




ylim1 <- c(0,7)
boxplot(temp_data_bird_58$NumSpec, main="Cluster 58", ylim=ylim1)
par(mfrow=c(1,3))
ylim = c(0,35)
boxplot(temp_data_bird_58$Far_totals, ylim=ylim)
boxplot(temp_data_bird_58$Mod_totals, ylim=ylim)
boxplot(temp_data_bird_58$Near_totals, ylim=ylim)

a43 <- which(temp_data_bird$cluster_list==43)
temp_data_bird_43 <- temp_data_bird[a43,]
boxplot(temp_data_bird_43$NumSpec, main="Cluster 43", ylim=ylim1)
par(mfrow=c(1,3))
boxplot(temp_data_bird_43$Far_totals, ylim=ylim)
boxplot(temp_data_bird_43$Mod_totals, ylim=ylim)
boxplot(temp_data_bird_43$Near_totals, ylim=ylim)

a37 <- which(temp_data_bird$cluster_list==37)
temp_data_bird_37 <- temp_data_bird[a37,]
boxplot(temp_data_bird_37$NumSpec, main="Cluster 37", ylim=ylim1)
boxplot(temp_data_bird_37$Far_totals, ylim=ylim)
boxplot(temp_data_bird_37$Mod_totals, ylim=ylim)
boxplot(temp_data_bird_37$Near_totals, ylim=ylim)

a15 <- which(temp_data_bird$cluster_list==15)
temp_data_bird_15 <- temp_data_bird[a15,]
boxplot(temp_data_bird_15$NumSpec, main="Cluster 15", ylim=ylim1)
boxplot(temp_data_bird_15$Far_totals, ylim=ylim)
boxplot(temp_data_bird_15$Mod_totals, ylim=ylim)
boxplot(temp_data_bird_15$Near_totals, ylim=ylim)

a3 <- which(temp_data_bird$cluster_list==3)
temp_data_bird_3 <- temp_data_bird[a3,]
boxplot(temp_data_bird_3$NumSpec, main="Cluster 3", ylim=ylim1)
boxplot(temp_data_bird_3$Far_totals, ylim=ylim)
boxplot(temp_data_bird_3$Mod_totals, ylim=ylim)
boxplot(temp_data_bird_3$Near_totals, ylim=ylim)

a11 <- which(temp_data_bird$cluster_list==11)
temp_data_bird_11 <- temp_data_bird[a11,]
boxplot(temp_data_bird_11$NumSpec, main="Cluster 11", ylim=ylim1)
boxplot(temp_data_bird_11$Far_totals, ylim=ylim)
boxplot(temp_data_bird_11$Mod_totals, ylim=ylim)
boxplot(temp_data_bird_11$Near_totals, ylim=ylim)
ylim1 <- c(1,6)
plot(table(temp_data_bird_37$NumSpec), xlim=ylim1)
plot(table(temp_data_bird_15$NumSpec), xlim=ylim1)
plot(table(temp_data_bird_43$NumSpec), xlim=ylim1)
plot(table(temp_data_bird_58$NumSpec), xlim=ylim1)
plot(table(temp_data_bird_3$NumSpec), xlim=ylim1)
plot(table(temp_data_bird_11$NumSpec), xlim=ylim1)

a8 <- which(temp_data_bird$cluster_list==8)
temp_data_bird_8 <- temp_data_bird[a8,]

a14 <- which(temp_data_bird$cluster_list==14)
temp_data_bird_14 <- temp_data_bird[a14,]

a39 <- which(temp_data_bird$cluster_list==39)
temp_data_bird_39 <- temp_data_bird[a39,]

a40 <- which(temp_data_bird$cluster_list==40)
temp_data_bird_40 <- temp_data_bird[a40,]

# Normalised sum EYR
sum(temp_data_bird_3$EYR.Near,temp_data_bird_3$EYR.Mod)/ncol(temp_data_bird_3)
sum(temp_data_bird_4$EYR.Near,temp_data_bird_4$EYR.Mod)/ncol(temp_data_bird_4)
sum(temp_data_bird_8$EYR.Near,temp_data_bird_8$EYR.Mod)/ncol(temp_data_bird_8)
sum(temp_data_bird_11$EYR.Near,temp_data_bird_11$EYR.Mod)/ncol(temp_data_bird_11)
sum(temp_data_bird_14$EYR.Near,temp_data_bird_14$EYR.Mod)/ncol(temp_data_bird_14)
sum(temp_data_bird_15$EYR.Near,temp_data_bird_15$EYR.Mod)/ncol(temp_data_bird_15)
sum(temp_data_bird_33$EYR.Near,temp_data_bird_33$EYR.Mod)/ncol(temp_data_bird_33)
sum(temp_data_bird_37$EYR.Near,temp_data_bird_37$EYR.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_39$EYR.Near,temp_data_bird_39$EYR.Mod)/ncol(temp_data_bird_39)
sum(temp_data_bird_40$EYR.Near,temp_data_bird_40$EYR.Mod)/ncol(temp_data_bird_40)
sum(temp_data_bird_43$EYR.Near,temp_data_bird_43$EYR.Mod)/ncol(temp_data_bird_43)
sum(temp_data_bird_58$EYR.Near,temp_data_bird_58$EYR.Mod)/ncol(temp_data_bird_58)

#3 4 8 11 14 15 33 37 39 40 43 58 

full_total <- sum(sum(temp_data_bird_3$EYR.Near, temp_data_bird_3$EYR.Mod),
                  sum(temp_data_bird_4$EYR.Near, temp_data_bird_4$EYR.Mod),
                  sum(temp_data_bird_8$EYR.Near, temp_data_bird_8$EYR.Mod),
                  sum(temp_data_bird_11$EYR.Near, temp_data_bird_11$EYR.Mod),
                  sum(temp_data_bird_14$EYR.Near, temp_data_bird_14$EYR.Mod),
                  sum(temp_data_bird_15$EYR.Near, temp_data_bird_15$EYR.Mod),
                  sum(temp_data_bird_33$EYR.Near, temp_data_bird_33$EYR.Mod),
                  sum(temp_data_bird_37$EYR.Near, temp_data_bird_37$EYR.Mod),
                  sum(temp_data_bird_39$EYR.Near, temp_data_bird_39$EYR.Mod),
                  sum(temp_data_bird_40$EYR.Near, temp_data_bird_40$EYR.Mod),
                  sum(temp_data_bird_43$EYR.Near, temp_data_bird_43$EYR.Mod),
                  sum(temp_data_bird_58$EYR.Near, temp_data_bird_58$EYR.Mod))

total_37_EYR <- sum(temp_data_bird_37$EYR.Near, temp_data_bird_37$EYR.Mod)
(EYR_37 <- total_37_EYR*100/full_total)

total_3_EYR <- sum(temp_data_bird_3$EYR.Near,temp_data_bird_3$EYR.Mod)
(EYR_3 <- total_3_EYR*100/full_total)

total_4_EYR <- sum(temp_data_bird_4$EYR.Near,temp_data_bird_4$EYR.Mod)
(EYR_4 <- total_4_EYR*100/full_total)

total_8_EYR <- sum(temp_data_bird_8$EYR.Near, temp_data_bird_8$EYR.Mod)
(EYR_8 <- total_8_EYR*100/full_total)

total_11_EYR <- sum(temp_data_bird_11$EYR.Near,temp_data_bird_11$EYR.Mod)
(EYR_11 <- total_11_EYR*100/full_total)

total_14_EYR <- sum(temp_data_bird_14$EYR.Near,temp_data_bird_14$EYR.Mod)
(EYR_14 <- total_14_EYR*100/full_total)

total_15_EYR <- sum(temp_data_bird_15$EYR.Near,temp_data_bird_15$EYR.Mod)
(EYR_15 <- total_15_EYR*100/full_total)

total_37_EYR <- sum(temp_data_bird_37$EYR.Near,temp_data_bird_37$EYR.Mod)
(EYR_37 <- total_37_EYR*100/full_total)

total_39_EYR <- sum(temp_data_bird_39$EYR.Near,temp_data_bird_39$EYR.Mod)
(EYR_39 <- total_39_EYR*100/full_total)

total_40_EYR <- sum(temp_data_bird_40$EYR.Near,temp_data_bird_40$EYR.Mod)
(EYR_40 <- total_40_EYR*100/full_total)

total_43_EYR <- sum(temp_data_bird_43$EYR.Near,temp_data_bird_43$EYR.Mod)
(EYR_43 <- total_43_EYR*100/full_total)

total_58_EYR <- sum(temp_data_bird_58$EYR.Near,temp_data_bird_58$EYR.Mod)
(EYR_58 <- total_58_EYR*100/full_total)

sum(temp_data_bird_3$Kookaburra.Loud,temp_data_bird_3$Kookaburra.Mod)/ncol(temp_data_bird_3)
sum(temp_data_bird_8$Kookaburra.Loud,temp_data_bird_8$Kookaburra.Mod)/ncol(temp_data_bird_8)
sum(temp_data_bird_11$Kookaburra.Loud,temp_data_bird_11$Kookaburra.Mod)/ncol(temp_data_bird_11)
sum(temp_data_bird_14$Kookaburra.Loud,temp_data_bird_14$Kookaburra.Mod)/ncol(temp_data_bird_14)
sum(temp_data_bird_15$Kookaburra.Loud,temp_data_bird_15$Kookaburra.Mod)/ncol(temp_data_bird_15)
sum(temp_data_bird_37$Kookaburra.Loud,temp_data_bird_37$Kookaburra.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_39$Kookaburra.Loud,temp_data_bird_39$Kookaburra.Mod)/ncol(temp_data_bird_39)
sum(temp_data_bird_40$Kookaburra.Loud,temp_data_bird_40$Kookaburra.Mod)/ncol(temp_data_bird_40)
sum(temp_data_bird_43$Kookaburra.Loud,temp_data_bird_43$Kookaburra.Mod)/ncol(temp_data_bird_43)
sum(temp_data_bird_58$Kookaburra.Loud,temp_data_bird_58$Kookaburra.Mod)/ncol(temp_data_bird_58)

total_37_KOOK <- sum(temp_data_bird_37$Kookaburra.Loud, temp_data_bird_37$Kookaburra.Mod)
full_total <- sum(sum(temp_data_bird_3$Kookaburra.Loud, temp_data_bird_3$Kookaburra.Mod),
                  sum(temp_data_bird_8$Kookaburra.Loud, temp_data_bird_8$Kookaburra.Mod),
                  sum(temp_data_bird_11$Kookaburra.Loud, temp_data_bird_11$Kookaburra.Mod),
                  sum(temp_data_bird_14$Kookaburra.Loud, temp_data_bird_14$Kookaburra.Mod),
                  sum(temp_data_bird_15$Kookaburra.Loud, temp_data_bird_15$Kookaburra.Mod),
                  sum(temp_data_bird_37$Kookaburra.Loud, temp_data_bird_37$Kookaburra.Mod),
                  sum(temp_data_bird_39$Kookaburra.Loud, temp_data_bird_39$Kookaburra.Mod),
                  sum(temp_data_bird_40$Kookaburra.Loud, temp_data_bird_40$Kookaburra.Mod),
                  sum(temp_data_bird_43$Kookaburra.Loud, temp_data_bird_43$Kookaburra.Mod),
                  sum(temp_data_bird_58$Kookaburra.Loud, temp_data_bird_58$Kookaburra.Mod))

(KOOK_37 <- total_37_KOOK*100/full_total)

total_3_KOOK <- sum(temp_data_bird_3$Kookaburra.Loud, temp_data_bird_3$Kookaburra.Mod)
(KOOK_3 <- total_3_KOOK*100/full_total)

total_8_KOOK <- sum(temp_data_bird_8$Kookaburra.Loud, temp_data_bird_8$Kookaburra.Mod)
(KOOK_8 <- total_8_KOOK*100/full_total)

total_15_KOOK <- sum(temp_data_bird_15$Kookaburra.Loud, temp_data_bird_15$Kookaburra.Mod)
(KOOK_15 <- total_15_KOOK*100/full_total)

total_39_KOOK <- sum(temp_data_bird_39$Kookaburra.Loud, temp_data_bird_39$Kookaburra.Mod)
(KOOK_39 <- total_39_KOOK*100/full_total)

total_40_KOOK <- sum(temp_data_bird_40$Kookaburra.Loud, temp_data_bird_40$Kookaburra.Mod)
(KOOK_40 <- total_40_KOOK*100/full_total)

total_14_KOOK <- sum(temp_data_bird_14$Kookaburra.Loud, temp_data_bird_14$Kookaburra.Mod)
(KOOK_14 <- total_14_KOOK*100/full_total)

total_43_KOOK <- sum(temp_data_bird_43$Kookaburra.Loud, temp_data_bird_43$Kookaburra.Mod)
(KOOK_43 <- total_43_KOOK*100/full_total)

total_11_KOOK <- sum(temp_data_bird_11$Kookaburra.Loud, temp_data_bird_11$Kookaburra.Mod)
(KOOK_11 <- total_11_KOOK*100/full_total)

total_58_KOOK <- sum(temp_data_bird_58$Kookaburra.Loud, temp_data_bird_58$Kookaburra.Mod)
(KOOK_58 <- total_58_KOOK*100/full_total)

sum(temp_data_bird_37$SC1.Near,temp_data_bird_37$SC1.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_15$SC1.Near,temp_data_bird_15$SC1.Mod)/ncol(temp_data_bird_15)
sum(temp_data_bird_43$SC1.Near,temp_data_bird_43$SC1.Mod)/ncol(temp_data_bird_43)
sum(temp_data_bird_11$SC1.Near,temp_data_bird_11$SC1.Mod)/ncol(temp_data_bird_11)
sum(temp_data_bird_58$SC1.Near,temp_data_bird_58$SC1.Mod)/ncol(temp_data_bird_58)

total_37_SC1 <- sum(temp_data_bird_37$SC1.Near, temp_data_bird_37$SC1.Mod)
full_total <- sum(sum(temp_data_bird_37$SC1.Near, temp_data_bird_37$SC1.Mod),
                  sum(temp_data_bird_15$SC1.Near, temp_data_bird_15$SC1.Mod),
                  sum(temp_data_bird_43$SC1.Near, temp_data_bird_43$SC1.Mod),
                  sum(temp_data_bird_11$SC1.Near, temp_data_bird_11$SC1.Mod),
                  sum(temp_data_bird_58$SC1.Near, temp_data_bird_58$SC1.Mod))
(SC1_37 <- total_37_SC1*100/full_total)

total_15_SC1 <- sum(temp_data_bird_15$SC1.Near, temp_data_bird_15$SC1.Mod)
(SC1_15 <- total_15_SC1*100/full_total)

total_43_SC1 <- sum(temp_data_bird_43$SC1.Near, temp_data_bird_43$SC1.Mod)
(SC1_43 <- total_43_SC1*100/full_total)

total_11_SC1 <- sum(temp_data_bird_11$SC1.Near, temp_data_bird_11$SC1.Mod)
(SC1_11 <- total_11_SC1*100/full_total)

total_58_SC1 <- sum(temp_data_bird_58$SC1.Near, temp_data_bird_58$SC1.Mod)
(SC1_58 <- total_58_SC1*100/full_total)

sum(temp_data_bird_37$SC3.Chatter.Near,temp_data_bird_37$SC3.Chatter.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_15$SC3.Chatter.Near,temp_data_bird_15$SC3.Chatter.Mod)/ncol(temp_data_bird_15)
sum(temp_data_bird_43$SC3.Chatter.Near,temp_data_bird_43$SC3.Chatter.Mod)/ncol(temp_data_bird_43)
sum(temp_data_bird_11$SC3.Chatter.Near,temp_data_bird_11$SC3.Chatter.Mod)/ncol(temp_data_bird_11)
sum(temp_data_bird_58$SC3.Chatter.Near,temp_data_bird_58$SC3.Chatter.Mod)/ncol(temp_data_bird_58)

total_37_SC2 <- sum(temp_data_bird_37$SC3.Chatter.Near, temp_data_bird_37$SC3.Chatter.Mod)
full_total <- sum(sum(temp_data_bird_37$SC3.Chatter.Near, temp_data_bird_37$SC3.Chatter.Mod),
                  sum(temp_data_bird_15$SC3.Chatter.Near, temp_data_bird_15$SC3.Chatter.Mod),
                  sum(temp_data_bird_43$SC3.Chatter.Near, temp_data_bird_43$SC3.Chatter.Mod),
                  sum(temp_data_bird_11$SC3.Chatter.Near, temp_data_bird_11$SC3.Chatter.Mod),
                  sum(temp_data_bird_58$SC3.Chatter.Near, temp_data_bird_58$SC3.Chatter.Mod))
(SC2_37 <- total_37_SC2*100/full_total)

total_15_SC2 <- sum(temp_data_bird_15$SC3.Chatter.Near, temp_data_bird_15$SC3.Chatter.Mod)
(SC2_15 <- total_15_SC2*100/full_total)

total_43_SC2 <- sum(temp_data_bird_43$SC3.Chatter.Near, temp_data_bird_43$SC3.Chatter.Mod)
(SC2_43 <- total_43_SC2*100/full_total)

total_11_SC2 <- sum(temp_data_bird_11$SC3.Chatter.Near, temp_data_bird_11$SC3.Chatter.Mod)
(SC2_11 <- total_11_SC2*100/full_total)

total_58_SC2 <- sum(temp_data_bird_58$SC3.Chatter.Near, temp_data_bird_58$SC3.Chatter.Mod)
(SC2_58 <- total_58_SC2*100/full_total)

sum(temp_data_bird_37$WTT.trill.Near,temp_data_bird_37$WTT.trill.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_15$WTT.trill.Near,temp_data_bird_15$WTT.trill.Mod)/ncol(temp_data_bird_15)
sum(temp_data_bird_43$WTT.trill.Near,temp_data_bird_43$WTT.trill.Mod)/ncol(temp_data_bird_43)
sum(temp_data_bird_11$WTT.trill.Near,temp_data_bird_11$WTT.trill.Mod)/ncol(temp_data_bird_11)
sum(temp_data_bird_58$WTT.trill.Near,temp_data_bird_58$WTT.trill.Mod)/ncol(temp_data_bird_58)

total_37_WTT <- sum(temp_data_bird_37$WTT.trill.Near, temp_data_bird_37$WTT.trill.Mod)
full_total <- sum(sum(temp_data_bird_37$WTT.trill.Near, temp_data_bird_37$WTT.trill.Mod),
                  sum(temp_data_bird_15$WTT.trill.Near, temp_data_bird_15$WTT.trill.Mod),
                  sum(temp_data_bird_43$WTT.trill.Near, temp_data_bird_43$WTT.trill.Mod),
                  sum(temp_data_bird_11$WTT.trill.Near, temp_data_bird_11$WTT.trill.Mod),
                  sum(temp_data_bird_58$WTT.trill.Near, temp_data_bird_58$WTT.trill.Mod))
(WTT_37 <- total_37_WTT*100/full_total)

total_15_WTT <- sum(temp_data_bird_15$WTT.trill.Near, temp_data_bird_15$WTT.trill.Mod)
(WTT_37 <- total_15_WTT*100/full_total)

total_43_WTT <- sum(temp_data_bird_43$WTT.trill.Near, temp_data_bird_43$WTT.trill.Mod)
(WTT_43 <- total_43_WTT*100/full_total)

total_11_WTT <- sum(temp_data_bird_11$WTT.trill.Near, temp_data_bird_11$WTT.trill.Mod)
(WTT_11 <- total_11_WTT*100/full_total)

total_58_WTT <- sum(temp_data_bird_58$WTT.trill.Near, temp_data_bird_58$WTT.trill.Mod)
(WTT <- total_58_WTT*100/full_total)

sum(temp_data_bird_3$WTH.Near,temp_data_bird_3$WTH.Mod)/ncol(temp_data_bird_3)
sum(temp_data_bird_8$WTH.Near,temp_data_bird_8$WTH.Mod)/ncol(temp_data_bird_8)
sum(temp_data_bird_11$WTH.Near,temp_data_bird_11$WTH.Mod)/ncol(temp_data_bird_11)
sum(temp_data_bird_14$WTH.Near,temp_data_bird_14$WTH.Mod)/ncol(temp_data_bird_14)
sum(temp_data_bird_15$WTH.Near,temp_data_bird_15$WTH.Mod)/ncol(temp_data_bird_15)
sum(temp_data_bird_37$WTH.Near,temp_data_bird_37$WTH.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_39$WTH.Near,temp_data_bird_39$WTH.Mod)/ncol(temp_data_bird_39)
sum(temp_data_bird_40$WTH.Near,temp_data_bird_40$WTH.Mod)/ncol(temp_data_bird_40)
sum(temp_data_bird_43$WTH.Near,temp_data_bird_43$WTH.Mod)/ncol(temp_data_bird_43)
sum(temp_data_bird_58$WTH.Near,temp_data_bird_58$WTH.Mod)/ncol(temp_data_bird_58)

total_37_WTH <- sum(temp_data_bird_37$WTH.Near, temp_data_bird_37$WTH.Mod)
full_total <- sum(sum(temp_data_bird_3$WTH.Near, temp_data_bird_3$WTH.Mod),
                  sum(temp_data_bird_8$WTH.Near, temp_data_bird_8$WTH.Mod),
                  sum(temp_data_bird_11$WTH.Near, temp_data_bird_11$WTH.Mod),
                  sum(temp_data_bird_14$WTH.Near, temp_data_bird_14$WTH.Mod),
                  sum(temp_data_bird_15$WTH.Near, temp_data_bird_15$WTH.Mod),
                  sum(temp_data_bird_37$WTH.Near, temp_data_bird_37$WTH.Mod),
                  sum(temp_data_bird_39$WTH.Near, temp_data_bird_39$WTH.Mod),
                  sum(temp_data_bird_40$WTH.Near, temp_data_bird_40$WTH.Mod),
                  sum(temp_data_bird_43$WTH.Near, temp_data_bird_43$WTH.Mod),
                  sum(temp_data_bird_58$WTH.Near, temp_data_bird_58$WTH.Mod))
(WTH_37 <- total_37_WTH*100/full_total)

total_3_WTH <- sum(temp_data_bird_3$WTH.Near, temp_data_bird_3$WTH.Mod)
(WTH_3 <- total_3_WTH*100/full_total)

total_8_WTH <- sum(temp_data_bird_8$WTH.Near, temp_data_bird_8$WTH.Mod)
(WTH_8 <- total_8_WTH*100/full_total)

total_14_WTH <- sum(temp_data_bird_14$WTH.Near, temp_data_bird_14$WTH.Mod)
(WTH_14 <- total_14_WTH*100/full_total)

total_15_WTH <- sum(temp_data_bird_15$WTH.Near, temp_data_bird_15$WTH.Mod)
(WTH_15 <- total_15_WTH*100/full_total)

total_43_WTH <- sum(temp_data_bird_43$WTH.Near, temp_data_bird_43$WTH.Mod)
(WTH_43 <- total_43_WTH*100/full_total)

total_39_WTH <- sum(temp_data_bird_39$WTH.Near, temp_data_bird_39$WTH.Mod)
(WTH_39 <- total_39_WTH*100/full_total)

total_11_WTH <- sum(temp_data_bird_11$WTH.Near, temp_data_bird_11$WTH.Mod)
(WTH_11 <- total_11_WTH*100/full_total)

total_58_WTH <- sum(temp_data_bird_58$WTH.Near, temp_data_bird_58$WTH.Mod)
(WTH_58 <- total_58_WTH*100/full_total)

total_40_WTH <- sum(temp_data_bird_40$WTH.Near, temp_data_bird_40$WTH.Mod)
(WTH_40 <- total_40_WTH*100/full_total)

total_43_WTH <- sum(temp_data_bird_43$WTH.Near, temp_data_bird_43$WTH.Mod)
(WTH_43 <- total_43_WTH*100/full_total)

sum(temp_data_bird_37$EW.Near,temp_data_bird_37$EW.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_15$EW.Near,temp_data_bird_15$EW.Mod)/ncol(temp_data_bird_15)
sum(temp_data_bird_43$EW.Near,temp_data_bird_43$EW.Mod)/ncol(temp_data_bird_43)
sum(temp_data_bird_11$EW.Near,temp_data_bird_11$EW.Mod)/ncol(temp_data_bird_11)
sum(temp_data_bird_58$EW.Near,temp_data_bird_58$EW.Mod)/ncol(temp_data_bird_58)

total_37_EW <- sum(temp_data_bird_37$EW.Near, temp_data_bird_37$EW.Mod)
full_total <- sum(sum(temp_data_bird_37$EW.Near, temp_data_bird_37$EW.Mod),
                  sum(temp_data_bird_15$EW.Near, temp_data_bird_15$EW.Mod),
                  sum(temp_data_bird_43$EW.Near, temp_data_bird_43$EW.Mod),
                  sum(temp_data_bird_11$EW.Near,temp_data_bird_11$EW.Mod),
                  sum(temp_data_bird_58$EW.Near,temp_data_bird_58$EW.Mod))
(EW_37 <- total_37_EW*100/full_total)

total_15_EW <- sum(temp_data_bird_15$EW.Near, temp_data_bird_15$EW.Mod)
(EW_15 <- total_15_EW*100/full_total)

total_43_EW <- sum(temp_data_bird_43$EW.Near, temp_data_bird_43$EW.Mod)
(EW_43 <- total_43_EW*100/full_total)

total_11_EW <- sum(temp_data_bird_11$EW.Near, temp_data_bird_11$EW.Mod)
(EW_11 <- total_11_EW*100/full_total)

total_58_EW <- sum(temp_data_bird_58$EW.Near, temp_data_bird_58$EW.Mod)
(EW_58 <- total_58_EW*100/full_total)

sum(temp_data_bird_37$Torresian.Crow.Near,temp_data_bird_37$Torresian.Crow.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_15$Torresian.Crow.Near,temp_data_bird_15$Torresian.Crow.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_43$Torresian.Crow.Near,temp_data_bird_43$Torresian.Crow.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_11$Torresian.Crow.Near,temp_data_bird_11$Torresian.Crow.Mod)/ncol(temp_data_bird_37)
sum(temp_data_bird_58$Torresian.Crow.Near,temp_data_bird_58$Torresian.Crow.Mod)/ncol(temp_data_bird_37)

table[1,] <- c("EYR",
               round(EYR_3),  round(EYR_8), round(EYR_11), 
               round(EYR_14), round(EYR_15), round(EYR_37), 
               round(EYR_39), round(EYR_40), round(EYR_43), 
               round(EYR_58), 
               sum(EYR_3,  EYR_8, EYR_11, EYR_14, 
                   EYR_15, EYR_37, EYR_39, EYR_40,
                   EYR_43, EYR_58))
table[2,] <- c("WTH",
               round(WTH_3),  round(WTH_8), round(WTH_11), 
               round(WTH_14), round(WTH_15), round(WTH_37),
               round(WTH_39), round(WTH_40), round(WTH_43), 
               round(WTH_58),
               sum(WTH_3, WTH_8, WTH_11, WTH_14, WTH_15, 
                   WTH_37, WTH_39, WTH_40, WTH_43, WTH_58))
table[3,] <- c("KOOK",
               round(KOOK_3),  round(KOOK_8), round(KOOK_11), 
               round(KOOK_14), round(KOOK_15), round(KOOK_37),
               round(KOOK_39), round(KOOK_40), round(KOOK_43), 
               round(KOOK_58),
               sum(KOOK_3, KOOK_8, KOOK_11, KOOK_14, KOOK_15, 
                   KOOK_37, KOOK_39, KOOK_40, KOOK_43, KOOK_58))
table[4,] <- c("SC1", round(SC1_3),  round(SC1_8), round(SC1_11), 
               round(SC1_14), round(SC1_15), round(SC1_37),
               round(SC1_39), round(SC1_40), round(SC1_43), 
               round(SC1_58),
               sum(SC1_3, SC1_8, SC1_11, SC1_14, SC1_15, 
                   SC1_37, SC1_39, SC1_40, SC1_43, SC1_58))
table[4,] <- c("SC2",round(SC2_3),  round(SC2_8), round(SC2_11), 
               round(SC2_14), round(SC2_15), round(SC2_37),
               round(SC2_39), round(SC2_40), round(SC2_43), 
               round(SC2_58),
               sum(SC2_3, SC2_8, SC2_11, SC2_14, SC2_15, 
                   SC2_37, SC2_39, SC2_40, SC2_43, SC2_58))
a <- which(data_bird$WTH > 0)
temp <- data_bird[a,]
t <-table(temp$cluster_list)
sum(temp$WTH.Near, temp$WTH.Mod)

# finding which events had more than one species
bird_data <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\all_data_added_protected.csv", header=T)
list <- unique(bird_data$MANUAL_ID)
list <- as.character(list)
list <- sort(list)
list1 <- c("WTT piping Far EW Far",
          "- - WTH Mod - - - WTT trill Far - - - - - - - - - -",     
          "- - WTH Mod EYR - piping - - - - - - - - - - - - -",      
          "- - WTH Mod - - - WTT trill Near - - - - - - - - - -",    
          "- - WTH Far - - - WTT trill Far - - - - - - - - - -",     
          "- - WTH Mod - - Kookaburra Quiet - - - - - - - - - - -",  
          "- - WTH Mod - - - WTT trill Mod - - - - - - - - - -",     
          "- - WTH Mod - Lewins Mod - - - - - - - - - - - -",        
          "- - WTH Mod - Lewins Mod - WTT trill Far - - - - - - - - - -",                 
          "- - - - - - - - - - - WTH Mod - - - WTT trill Far -",     
          "- - WTH Far - Lewins Far - - - - - - - - - - - -",        
          "- - - - - - - - - - WTH Mod - - - - WTT trill Far -",     
          "- - - - - - - - SC Far - - - - - - WTT trill Far -",      
          "SC Near EW Mod - - - - - - - - - - - - - - -",            
          "SC Mod EW Mod - - - - - - - - - - - - - - -",             
          "- - - - - - - - SC Mod EW Far - WTH Mod - - - - -",       
          "SC Far - WTH Mod - - - WTT trill Far - - - - - - - - - -",
          "- - - - - - - - SC Far - - WTH Mod - - - - -",            
          "SC Mod - - - - - WTT trill Far - - - - - - - - - -",      
          "- EW Mod - - - - WTT piping Mod - - - - - - - - - -",     
          "SC Far - - - - - WTT piping Mod - - - - - - - - - -",     
          "SC Mod - - - Lewins Mod - - - - - - - - - - - -",         
          "- - WTH Far EYR - piping - - - - - - - - - - - - -",      
          "SC Far - - EYR - piping - - - - - - - - - - - - -",       
          "- - - - - - - - SC Far - - WTH Far - - - - -",            
          "SC Far - WTH Mod - Lewins Far - - - - - - - - - - - -",   
          "- - WTH Mod - Lewins Far - - - - - - - - - - - -",        
          "- - WTH Far - Lewins Near - - - - - - - - - - - -",       
          "- EW Mod WTH Far - - - - - - - - - - - - - -",            
          "SC Far - - - - - WTT trill Mod - - - - - - - - - -",      
          "SC Far - - - - Kookaburra Quiet - - - - - - - - - - -",   
          "- EW Far - - - - WTT trill Mod - - - - - - - - - -",      
          "- EW Mod - - - - WTT trill Far - - - - - - - - - -",      
          "- EW Far WTH Far - - - WTT trill Mod - - - - - - - - - -",
          "- - - - - - - - SC Far - WTH Far - - - - WTT trill Mod -",
          "SC Far - WTH Far - - - WTT trill Mod - - - - - - - - - -",
          "- - WTH Far - - - WTT trill Mod - - - - - - - - - -",     
          "- - WTH Mod - Lewins Mod - WTT trill Mod - - - - - - - - - -",                 
          "- EW Near - - - - WTT trill Mod - - - - - - - - - -",     
          "SC Mod EW Near  - Lewins Far - - - - - - - - - - - -",    
          "SC Mod - - - - - WTT trill Mod - - - - - - - - - -",      
          "SC Mod EW Mod - - - - WTT trill Mod - - - - - - - - - -", 
          "SC Near - - - - - WTT trill Mod - - - - - - - - - -",     
          "SC Mod - - - Lewins Far - WTT trill Mod - - - - - - - - - -",                  
          "SC Mod - - - - - WTT trill Near - - - - - - - - - -",     
          "SC Single note Near EW Far - - - - - - - - - - - - - - -",
          "SC Near - WTH Far - - - - - - - - - - - - - -",           
          "- - - - - - - - - EW Far - - - Lewins Far - WTT trill Mod -",                  
          "SC Mod EW Far - - - - - - - - - - - - - - -",             
          "- - - - - - - - - EW Far - WTH Far - - - - -",            
          "- - - - - - - - SC Mod - - - - - - WTT trill Mod -",      
          "- - WTH Near - - - WTT trill Far - - - - - - - - - -",    
          "- EW Far - - - - WTT trill Far - - - - - - - - - -",      
          "- - - - - - - - - EW Far - - - Lewins Far - - -",         
          "SC Mod - - - Lewins Far - - - - - - - - - - - -",         
          "SC Near - - - Lewins Mod - - - - - - - - - - - -",        
          "SC Mod - WTH Mod - Lewins Mod - - - - - - - - - - - -",   
           "SC Mod EW Far - - - - WTT trill Mod - - - - - - - - - -", 
          "SC Mod EW Mod - - Lewins Mod - WTT piping Near - - - - - - - - - -",           
          "SC Mod - WTH Far - - - - - - - - - - - - - -",            
          "- - - - - - - - SC Far - WTH Mod - - - - - -",            
          "SC Chatter Far - - - - Kookaburra Quiet - - - - - - - - - - -",                
          "SC Chatter Mod EW Far - - - - - - - - - - - - - - -",     
          "SC Mod - WTH Mod - - - - - - - - - - - - - -",            
          "- - WTH Mod EYR - piping Lewins Far - - - - - - - - - - - -",                  
          "- EW Far WTH Mod - - - - - - - - - - - - - -",            
          "- - WTH Far EYR - piping Lewins Far - - - - - - - - - - - -",                  
          "- - WTH Far EYR - piping - - WTT trill Far - - - - - - - - - -",               
          "SC Far - - - - - WTT trill Far - - - - - - - - - -",      
          "- EW Far WTH Mod - - - WTT trill Mod - - - - - - - - - -",
          "SC Mod EW Mod - - - - WTT trill Far - - - - - - - - - -", 
          "SC Far - WTH Far - - - WTT trill Far - - - - - - - - - -",
          "- EW Far WTH Near - Lewins Far - - - - - - - - - - - -",  
          "SC Single note Mod - - - Lewins Mod - WTT trill Mod - - - - - - - - - -",      
          "SC Near - - - - - WTT trill Far - - - - - - - - - -",     
          "SC Near - - - Lewins Near - - - - - - - - - - - -",       
          "- EW Far WTH Near - - - - - - - - - - - - - -",           
          "- - WTH Near - Lewins Far - - - - - - - - - - - -",       
          "SC Far EW Mod - - - - WTT trill Far - - - - - - - - - -", 
          "- - - - - - - - - - - WTH Near - - - WTT trill Far -",    
          "SC Far EW Mod - - - - - - - - - - - - - - -",             
          "- - - - - - - - SC Near - - - - - - WTT trill Far -",     
          "- EW Mod - - - - WTT trill Mod - - - - - - - - - -",      
          "- EW Mod WTH Far - - - WTT trill Far - - - - - - - - - -",
          "- EW Mod WTH Near - - - WTT trill Far - - - - - - - - - -",                    
          "- - - - - - - - SC Far EW Far - - - - - - -",             
          "SC Far EW Far WTH Mod - - - - - - - - - - - - - -",       
          "- EW Far - - Lewins Far - - - - - - - - - - - -",         
          "- - - - - - - - - - - WTH Far - - - WTT trill Mod -",     
          "- - - - - - - - - EW Far - - - Lewins Mod - - -",         
          "SC Far - - - Lewins Near - - - - - - - - - - - -",        
          "- - - - - - - - - - - - - Lewins Near - WTT trill Near -",
          "SC Far EW Far WTH Mod - - - WTT trill Mod - - - - - - - - - -",                
          "- - - - - - - - - EW Far - - - - - WTT trill Far -",      
          "SC Far - - - - - WTT trill Near - - - - - - - - - -",     
          "SC Near EW Far - - - - - - - - - - - - - - -",            
          "SC Far EW Near - - - - - - - - - - - - - - -",            
          "- EW Near - - Lewins Mod - - - - - - - - - - - -",        
          "- EW Near WTH Far - - - - - - - - - - - - - -",           
          "SC Chatter Far - WTH Mod - - - - - - - - - - - - - -",    
          "SC Chatter Far - - EYR - piping - - - - - - - - - - - - -",                    
          "SC Far - WTH Mod - - - - - - - - - - - - - -",            
          "SC Single note Far - WTH Mod - - - - - - - - - - - - - -",
          "- - - - - - - - - EW Mod - WTH Mod - - - - -",            
          "- - WTH Mod - - - WTT piping Far - - - - - - - - - -",    
          "- - - - - - - - SC Mod - WTH Mod - - - - - -",            
          "SC Mod - WTH Mod - - - WTT trill Mod - - - - - - - - - -",
          "- - WTH Near - - - WTT trill Mod - - - - - - - - - -",    
          "SC Mod - - - - Kookaburra Quiet - - - - - - - - - - -",   
          "- - - - Lewins Far - WTT trill Mod - - - - - - - - - -",  
          "- EW Mod WTH Near - - - WTT trill Mod - - - - - - - - - -",                    
          "- - WTH Near - Lewins Mod - - - - - - - - - - - -",       
          "SC Far EW Mod WTH Near - - - WTT trill Mod - - - - - - - - - -",               
          "SC Mod - WTH Mod - - - WTT trill Far - - - - - - - - - -",
          "SC Far EW Mod - - - - WTT trill Mod - - - - - - - - - -", 
          "SC Far EW Far - - Lewins Far - WTT trill Mod - - - - - - - - - -",             
          "SC Mod - WTH Near - - - WTT trill Mod - - - - - - - - - -",                    
          "SC Near - WTH Near - - - WTT trill Mod - - - - - - - - - -",                   
          "SC Single note Near - - - - - WTT trill Mod - - - - - - - - - -",              
          "SC Near - - - Lewins Mod - WTT trill Far - - - - - - - - - -",                 
          "SC Single note Near - - - - - WTT trill Far - - - - - - - - - -",              
          "SC Single note Mod - - - Lewins Far - - - - - - - - - - - -",                  
          "- - - - - - - - - EW Far WTH Far - - - - - -",            
          "- - - - Lewins Far - WTT trill Far - - - - - - - - - -",  
          "SC Far - - - Lewins Mod - - - - - - - - - - - -",         
          "SC Far EW Near - - - - WTT trill Mod - - - - - - - - - -",
          "- - - - - - - - SC Far - - - - - - WTT trill Mod -",      
          "- - - EYR - piping - - WTT trill Mod - - - - - - - - - -",
          "- - WTH Mod - Lewins Near - - - - - - - - - - - -",       
          "- - - - Lewins Near - WTT trill Mod - - - - - - - - - -", 
          "- - - - Lewins Mod - WTT trill Far - - - - - - - - - -",  
          "- - WTH Far - - - WTT piping Near - - - - - - - - - -",   
          "- - WTH Mod - - - WTT piping Mod - - - - - - - - - -",    
          "- - - - - - - - SC Far EW Far WTH Far - - - - - -",       
          "- - - - - - - - - EW Mod - - - - - WTT trill Far -",      
          "- EW Far WTH Far - - - - - - - - - - - - - -",            
          "- - - - - - - - - - - - - Lewins Far - WTT trill Far -",  
          "- - - - - - - - - - WTH Far - - - - WTT trill Far -",     
          "- - WTH Mod EYR Far - - - - - - - - - - - - -",           
          "SC Far - WTH Mod - - Kookaburra Quiet - - - - - - - - - - -",                  
          "SC Chatter Mod - - - - - WTT trill Far - - - - - - - - - -",                   
          "SC Chatter Mod - WTH Mod - - - - - - - - - - - - - -",    
          "SC Chatter Mod - WTH Mod - - - WTT trill Far - - - - - - - - - -",             
          "SC Chatter Mod - - - Lewins Far - - - - - - - - - - - -", 
          "- - WTH Mod - Lewins Far - WTT trill Far - - - - - - - - - -",                 
          "- - - - - - - - - EW Far - - - - Kookaburra Quiet - -",   
          "SC Far EW Far - - - - - - - - - - - - - - -",             
          "SC Far EW Far - - - - WTT trill Far - - - - - - - - - -", 
          "- EW Far WTH Far - - - WTT trill Far - - - - - - - - - -",
          "SC Far - - - - - WTT piping Far - - - - - - - - - -",     
          "SC Far - - - - - WTT trill fast - - - - - - - - - -",     
          "- - WTH Far EYR Far - - - - - - - - - - - - -",           
          "- - - - - - - - - - WTH Far - EYR Far - - - -",           
          "SC Chatter Near - - - - - WTT trill Far - - - - - - - - - -",                  
          "SC Mod EW Far WTH Mod - - - WTT trill Far - - - - - - - - - -",                
          "SC Far EW Far WTH Far - - - WTT trill Far - - - - - - - - - -",                
          "- EW Far WTH Mod - - - WTT trill Far - - - - - - - - - -",
          "- - - - - - - - - - - WTH Far - - - WTT trill Far -",     
          "SC Chatter Mod - - - - - WTT trill Mod - - - - - - - - - -",                   
          "SC Mod EW Far - - - - WTT trill Far - - - - - - - - - -", 
          "- EW Mod WTH Mod - - - - - - - - - - - - - -",            
          "- - - EYR Far - Kookaburra Quiet - - - - - - - - - - -",  
          "- - WTH Mod EYR Mod - - - - - - - - - - - - -",           
          "- - WTH Far EYR Near - - - - - - - - - - - - -",          
          "- - WTH Mod EYR Near - - - - - - - - - - - - -",          
          "- - WTH Far EYR Mod - - - - - - - - - - - - -",           
          "- - WTH Far - - Kookaburra Quiet - - - - - - - - - - -",  
          "SC Far - WTH Far - Lewins Far - - - - - - - - - - - -",   
          "SC Chatter Mod - WTH Far - - - - - - - - - - - - - -",    
          "SC Far - WTH Far - - - - - - - - - - - - - -",            
          "- -  - Lewins Far - - - - - WTH Near - - Lewins Far - - -",                    
          "SC Mod - WTH Far - - - WTT trill Far - - - - - - - - - -",
          "- - - - - - - - - - - WTH Mod - - - WTT trill Mod -",     
          "SC Far - WTH Far EYR Far - - - - - - - - - - - - -",      
          "- - WTH Mod EYR Far - - WTT trill Far - - - - - - - - - -",                    
          "SC Near - WTH Far - - - WTT trill Far - - - - - - - - - -",                    
          "SC Far - WTH Near - - - - - - - - - - - - - -",           
          "SC Mod - WTH Near - - - - - - - - - - - - - -",           
          "SC Far EW Near - - - - WTT trill Far - - - - - - - - - -",
          "- EW Mod - - - - WTT piping Near - - - - - - - - - -",    
          "- - - - - - - - SC Far - WTH Far - - - - - -",            
          "- - WTH Mod - Lewins Far Kookaburra Quiet - - - - - - - - - - -",              
          "- - WTH Far - - Kookaburra Quiet WTT trill Far - - - - - - - - - -",           
          "SC Single note Far - WTH Far - - - - - - - - - - - - - -",
          "- - - - - - - - SC Far - WTH Mod - - - - WTT trill Far -",
          "- - WTH Far - Lewins Near - WTT trill Far - - - - - - - - - -",                
          "SC Single note Far - - - - Kookaburra Mod - - - - - - - - - - -",              
          "- EW Far WTH Mod - - - WTT piping Near - - - - - - - - - -",                   
          "- EW Mod WTH Far - - - WTT piping Far - - - - - - - - - -",                    
          "- EW Mod WTH Far - Lewins Mod - - - - - - - - - - - -",   
          "- - WTH Far - Lewins Mod - - - - - - - - - - - -",        
          "- - WTH Mod EYR Far - Kookaburra Quiet - - - - - - - - - - -",                 
          "- - WTH Near - - Kookaburra Quiet - - - - - - - - - - -", 
          "SC Mod - - EYR Far - Kookaburra Quiet - - - - - - - - - - -",                  
          "SC Chatter Far - - - - - WTT trill Far - - - - - - - - - -",                   
          "- - - - - - - - - - WTH Mod - - Lewins Far - - -",        
          "- - WTH Mod - - Kookaburra Mod - - - - - - - - - - -",    
          "- - - - - Kookaburra Loud - - - - - - - - - - -",         
          "SC Far - WTH Mod - Lewins Near - - - - - - - - - - - -",  
          "- - - - - - - - - - WTH Far - EYR Mod - - - -",           
          "- - - - - - - - - - WTH Far - EYR Near - - - -",          
          "- - - - - - - - - - WTH Mod - EYR Far - - - -",           
          "- - - - - - - - - - WTH Mod - EYR Mod - - - -",           
          "- - - - - - - SC Chatter Mod - - - - - Lewins Far - - -", 
          "- - - - - - - - SC Far - WTH Far - - - Kookaburra Quiet - -",                  
          "- - - - - - - - SC Far - - - - Lewins Mod - - -",         
          " - - - - - - - - SC Far - WTT trill Mod- - - - - - -",    
          "- - - - - - - - SC Far - - - - - Kookaburra Quiet - -",   
          "- - - - - - - SC Chatter Far - - - - - - Kookaburra Quiet - -",                
          "- - - - - - - - - EW Mod WTH Mod - - - - - -",            
          "- EW Far - - - - WTT trill Near - - - - - - - - - -",     
          "- - WTH Far - - - WTT trill Near - - - - - - - - - -",    
          "- - - - - - - - SC Mod - - WTH Mod - - - - -",            
          "- - - - - - - - - EW Far WTH Mod - - - - - -",            
          "- - - - - - - - SC Far - - - EYR Far - - - -",            
          "- - - - - - - - - - SC added - EYR Far - - - -",          
          "- - - - - - - - - - WTH Mod - - - Kookaburra Quiet - -",  
          "SC Far - - EYR Mod - - - - - - - - - - - - -",            
          "- - - - - - - SC Chatter Mod - - - - EYR Mod - - - -",    
          "- - - - - - - - SC Mod - - - EYR Mod - - - -",            
          "- - - - - - - - SC Mod EW Mod - - - - - - -",             
          "- - - - - - - SC Chatter Far - - WTH Mod - - - - - -",    
          "- - - - - - - - - - WTH Mod - - Lewins Mod Kookaburra Quiet - -",              
          "- - - - - - - - - EW Mod WTH Mod - - Lewins Far - - -",   
          "- - - - - - - - - EW Near - WTH Near - - - - -",          
          "- - - - - - - - SC Far - - WTH Near - - - - -",           
          "- - - - - - - - SC Mod - WTH Far - - - - - -",            
          "- - - - - - - - - - WTH Far - - Lewins Far - - -",        
          "- - - - - - - - SC Mod EW Far - - - - - - -",             
          "SC Far - - - Lewins Far - - - - - - - - - - - -",         
          "- - - - - - - - SC Mod - - - - Lewins Far - - -",         
          "- - - - - - - - - EW Mod WTH Far - - - - - -",            
          "- - - - - - - - SC Far EW Mod - - - - - - -",             
          "- - - - - - - SC Chatter Mod - EW Mod - - - - - - -",     
          "- - - - - - - - - EW Near - WTH Mod - - - - -",           
          "- EW Near WTH Near - - - - - - - - - - - - - -",          
          "- - - - - - - - - EW Near - WTH Far - - - - -",           
          "- - - - - - - - - EW Far - - - - - EW added -",           
          "- - - - - - - - - - - WTH Far EYR Mod - - - -",           
          "- - - - - - - - SC Far - WTH Far - EYR Far - - - -",      
          "- - - - - - - - SC Single note Far - WTH Far - - - - - -",
          " - - - - Lewins Mod - WTT trill Mod - - - - - - - - - - -",                    
          "- - - - - - - SC Chatter Mod - EW Far - - - - - - -",     
          "- - - - - - - - SC Mod EW Far WTH Far - - - - - -",       
          "- - - - - - - - - - - WTH Far - Lewins Far - - -",        
          "- EW Mod WTH Near - - - - - - - - - - - - - -",           
          "- EW Near WTH Mod - - - - - - - - - - - - - -",           
          "- - - - - - - - - EW Far - WTH Mod - - - - -",            
          "- - - - - - - - - EW Far - WTH Mod - - - WTT trill Mod -",
          "- - - - - - - - SC Far EW Mod WTH Far - - - - - -",       
          "- - - EYR Near Lewins Mod - - - - - - - - - - - -",       
          "- - - - - - - - - EW Mod WTH Mod - - - - WTT trill Far -",
          "- - - - - - - SC Chatter Far - - - - - - - WTT trill Far -",                   
          "- - - - - - - SC Chatter Mod - - - - - - - WTT trill Mod -",                   
          "- - - - - - - SC Chatter Mod - - - - - - - WTT trill Far -",                   
          "- - - - - - - SC Chatter Far - EW Far - - - - - WTT trill Mod -",              
          "- - - - - - - SC Chatter Far - EW Mod - - - - - WTT trill Mod -",              
          "- - - - - - - SC Chatter Far - - WTH Mod - - - - WTT trill Far -",             
          "- - - - - - - - - EW Far WTH Mod - - - - WTT trill Far -",
          "- - - - - - - - SC Mod EW Far - - - Lewins Far - - -",    
          "- - - - - - - SC Chatter Mod - EW Far WTH Mod - - - - - -",                    
          "- - - - - - - - - EW Far - WTH Near - - - - -",           
          "SC Near - WTH Mod - - - - - - - - - - - - - -",           
          "SC Mod EW Near - - - - - - - - - - - - - - -",            
          "- EW Near - - - - WTT trill Near - - - - - - - - - -",    
          "- - - - - - - - - EW Near WTH Mod - - - - - -",           
          "- - - - - - - - - EW Near - - - - - WTT trill Far -",     
          "- - - - - - - - - EW Mod - WTH Far - - - - -",            
          "- - - - - - - - - - - WTH Mod EYR Mod - - - -",           
          "- - - - - - - SC Chatter Far - - WTH Mod - EYR Mod - - - -",                   
          "- - - - - - - SC Chatter Far - - - - EYR Mod - - - -",    
          "- - - - - - - SC Chatter Mod - - WTH Mod - - - - - -",    
          "- - - - - - - - SC Far EW Far WTH Mod - - - - - -",       
          "- - - - - - - SC Chatter Far - EW Far WTH Mod - - - - - -",                    
          "- - - - - - - SC Chatter Near - - - - - Lewins Mod - WTT trill Far -",         
          "- - - - - - - - - EW Far WTH Far - - - - WTT trill Far -",
          "- - - - - - - - SC Far EW Far - - - Lewins Mod - - -",    
          "- - - - - - - - - EW Far WTH Far WTH Mod - - - - -",      
          "- - - - - - - - SC Far EW Far - WTH Mod - - - - -",       
          "- - - - - - - - - - WTH Near - - - - WTT trill Far -",    
          "- - - - - - - - SC Far - WTH Mod - - - - WTT trill Mod -",
          "- - - - - - - - - EW Far - - - - - WTT trill Mod -",      
          "- - - - - - - - - - WTH Mod - EYR Near - - - -",          
          "- - - - - - - - - EW Far WTH Far - - - - WTT trill Mod -",
          "- - - - - - - - - EW Far WTH Mod - - - - WTT trill Mod -",
          "- - - - - - - - - - WTH Mod - - - - WTT trill Mod -",     
          "- - - - - - - - - - - - - Lewins Far - WTT trill Mod -",  
          "- - - - - - - - - EW Near - WTH Mod - Lewins Far Kookaburra Quiet - -",        
          "- - - - - - - - SC Single note Mod - - - - - - WTT trill Far -",               
          "- - - - - - - - - EW Mod - - - Lewins Far - - -",         
          "- - - - - - - - - - - WTH Near - Lewins Mod - - -",       
          "- - - - - - - SC Chatter Mod - - WTH Mod - - - - WTT trill Far -",             
          "- - - - - - - - - EW Mod - WTH Mod - - - WTT trill Far -",
          "- EW Mod - - - Kookaburra Quiet - - - - - - - - - - -",   
          "- - - - - - - - - - WTH Far - - - - WTT trill Mod -",     
          "- - - - - - - - - EW Near - WTH Mod - - - WTT trill Far -",                    
          "- EW Far - - - - WTT piping Mod - - - - - - - - - -",     
          "- - - - - - - SC Chatter Far - EW Far - - - - - WTT trill Far -",              
          "- - - EYR Near - Kookaburra Quiet - - - - - - - - - - -", 
          "- - - - - - - - - - WTH Mod - EYR Near - - WTT trill Mod -",                   
          "- - - - - - - - - - WTH Mod - - Lewins Mod - WTT trill Mod -",                 
          "- - - - - - - - SC Far - WTH Mod - - Lewins Far - WTT trill Mod -",            
          "- - - - - - - - SC Far EW Far - - - - - WTT trill Mod -", 
          "- - - - - - - - - - WTH Near - - - - WTT trill Mod -",    
          "- - - - - - - - - EW Mod - - - Lewins Far - WTT trill Mod -",                  
          "- - - - - - - - SC Mod EW Far - WTH Mod - - - WTT trill Far -",                
          "- - - - - - - - - EW Far SC added - - Lewins Far - - -",  
          "- - - - - - - - - EYR Near WTH Far - - - - - -",          
          "- - - - - - - - SC Far EW Mod - - - - Kookaburra Quiet WTT trill Far -",       
          "- - - - - - - - - - WTH Near WTH Mod - Lewins Far - - -", 
          "- - - - - - - - - EW Mod - - - Lewins Mod - - -",         
          "- - - - - - - - - - - WTH Far EYR Far - - - -",           
          "- - - - - - - - SC Far - - - EYR Mod - - - -",            
          "- - - - - - - SC Chatter Far - - - - EYR Far - - - -",    
          "- - - - - - - - SC Mod - - - EYR Far - - - -",            
          "- - - - - - - - SC Far - WTH Far - - - - WTT trill Far -",
          "- - - - - - - - - EW Far WTH Near - - - - WTT trill Far -",                    
          "- - - - - - - - SC Far EW Far - - - - - WTT trill Far -", 
          "- - - - - - - SC Chatter Near - - WTH Near - - - - WTT trill Far -",           
          "- - - - - - - - - EW Far WTH Near - - - - - -",           
          "- - - - - - - SC Chatter Near - EW Far - WTH Near - - - WTT trill Far -",      
          "- - - - - - - - - EW Mod - - - - - WTT trill Mod -",      
          "- - - - - - - - - EW Mod - - - - - WTH Mod -",            
          "- - - - - - - - SC Far - WTH Near - - - - - -",           
          "- - - - - - - - - - - - EYR Far - - WTT trill Far -",     
          "- - WTH Near EYR Far - - - - - - - - - - - - -",          
          "- - - - - - - - - - - WTH Mod EYR Mod Lewins Mod - - -",  
          "- - - EYR Mod - - WTT trill Far - - - - - - - - - -",     
          "- - - - - - - - SC Far - - - - Lewins Far - - -",         
          "- - - - - - - - SC Single note Far EW Far - - - - - - -", 
          "- - - - - - - SC Chatter Far - EW Far - - - - - - -",     
          "SC Chatter Far EW Far - - - - - - - - - - - - - - -",     
          "- EW Far - - - - WTT trill Far - SC Far - - - - - - - -", 
          "- EW Near - - Lewins Far - - - - - - - - - - - -",        
          "SC Chatter Mod EW Mod - - - - - - - - - - - - - - -",     
          "- EW Mod - - Lewins Far - - - - - - - - - - - -",         
          "SC Far - - EYR Near - - - - - - - - - - - - -",           
          "- - - - - - - - SC Single note Mod EW Far - - - - - - -", 
          "SC Chatter Near EW Far - - - - - - - - - - - - - - -",    
          "- - - - - - - - - - - WTH Mod EYR Far - - - -",           
          "- EW Near - - - - WTT trill Far - - - - - - - - - -",     
          "SC Chatter Mod EW Near - - - - - - - - - - - - - - -",    
          "- - - - - - - SC Chatter Far - - WTH Far - - - - - -",    
          "- - - - - - - SC Chatter Far - EW Mod - - - - - - -",     
          "Rufous Fantail EW Far - - - - - - - - - - - - - - -",     
          "- EW Mod - - - - WTT trill Near - - - - - - - - - -",     
          "- EW Far - - - - WTT piping Far - - - - - - - - - -",     
           "- - - - - - - - SC Far - - - EYR Far - Kookaburra Quiet - -",                  
          "- - - - Lewins Far - - - SC Far - - - - - - - -",         
          "- - - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",  
          "- - - - - Kookaburra Quiet WTT trill Mod - - - - - - - - - -",                 
          "SC Far EW Far - - - - WTT trill Mod - - - - - - - - - -", 
          "SC Far EW Far - - Lewins Mod - - - - - - - - - - - -",    
          "SC Chatter Far EW Far - - Lewins Far - - - - - - - - - - - -",                 
          "- - - - - - - - - - - WTH Near - - - WTT trill Mod -",    
          "- - WTH Near - - - WTT trill Near - - - - - - - - - -",   
          "- EW Mod - - Lewins Mod - - - - - - - - - - - -",         
          "- EW Mod - EYR Mod - - - - - - - - - - - - -",            
          "- - WTH Far EYR Near - Kookaburra Quiet - - - - - - - - - - -",                
          "- - WTH Far EYR Near  - - - - - - - - - - - -",           
          "- - WTH Far EYR Far - Kookaburra Quiet - - - - - - - - - - -",                 
          "- - WTH Mod EYR Far Lewins Far - - - - - - - - - - - -",  
          "- - WTH Far EYR Far Lewins Far - - - - - - - - - - - -",  
          "- EW Mod WTH Mod - - Kookaburra Quiet WTT trill Far - - - - - - - - - -",      
          "SC Chatter Near EW Near - - - - WTT trill Far - - - - - - - - - -",            
          "- - - - - - - - - EW Near - - - - - WTT trill Near -",    
          "SC Mod EW Near - - - - WTT trill Near - - - - - - - - - -",                    
          "SC Mod - - - Lewins Mod - WTT trill Near - - - - - - - - - -",                 
          "SC Mod EW Mod - - - - WTT trill Near - - - - - - - - - -",
          "- EW Mod WTH Mod - - - WTT trill Mod - - - - - - - - - -",
          "- - - - Lewins Mod - WTT trill Near - - - - - - - - - -", 
          "- - WTH Near -  - WTT trill Near - - - - - - - - - -",    
          "- EW Mod WTH Mod - - - WTT trill Far - - - - - - - - - -",
          "- - WTH Mod - - Kookaburra Quiet WTT trill Mod - - - - - - - - - -",           
          "- EW Far WTH Mod - - Kookaburra Quiet - - - - - - - - - - -",                  
          "- - - - Lewins Mod - WTT trill Mod - - - - - - - - - -",  
          "- EW Far - - Lewins Mod - - - - - - - - - - - -",         
          "- - - - - - - - SC Far - WTH Mod - EYR Mod - - - -",      
          "SC Far EW Mod - - - Kookaburra Quiet - - - - - - - - - - -",                   
          "- EW Far - - - Kookaburra Quiet - - - - - - - - - - -",   
          "- - - - - - - - SC Far - SC added - - - - - -",           
          "SC Chatter Far EW Far - - - - WTT trill Far - - - - - - - - - -",              
          "SC Chatter Mod - - - - Kookaburra Quiet - - - - - - - - - - -",                
          "- - - - - - - - SC Single note Far - - - - - - WTT trill Far -",               
          "- EW Far - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",                  
          "- EW Mod WTH Mod EYR Mod - - - - - - - - - - - - -",      
          "SC Mod - - - - Kookaburra Cackle - - - - - - - - - - -",  
          "SC Single note Mod EW Mod - - - - - - - - - - - - - - -", 
           "SC Far EW Far WTH Far - - - - - - - - - - - - - -",       
          "- EW Far - - - - - - SC Far - - - - - - - -",             
          "- - - - - - - - - - - - EYR Far - Kookaburra Quiet - -",  
          "- EW Far WTH Far EYR Far - - - - - - - - - - - - -",      
          "- - - - - - - SC Chatter Mod - - - - EYR Far - - - -",    
          "SC Far - - EYR Far - - - - - - - - - - - - -",            
          "- - WTH Far - - Kookaburra Mod - - - - - - - - - - -",    
          "- - - - - Kookaburra Quiet WTT trill Far - - - - - - - - - -",                 
          "- - - - - - - - SC Far - WTH Far - EYR Mod - - - -",      
          "SC Chatter Mod - - EYR Near - - - - - - - - - - - - -",   
          "SC Near - - EYR Near - - - - - - - - - - - - -",          
          "SC Chatter Near - - EYR Near - - - - - - - - - - - - -",  
          "SC Mod - - EYR Near - - - - - - - - - - - - -",           
          "SC Chatter Mod - - EYR Near - Kookaburra Quiet - - - - - - - - - - -",         
          "- - WTH Near EYR Near - - WTT trill Far - - - - - - - - - -",                  
          "- - - EYR Near - - WTT trill Far - - - - - - - - - -",    
          "- - WTH Far EYR Mod Lewins Far - - - - - - - - - - - -",  
          "SC Single note Mod - WTH Mod - Lewins Far - WTT trill Far - - - - - - - - - -",
          "SC Chatter Mod - - - Lewins Mod - - - - - - - - - - - -", 
          "SC Chatter Far - - - - - WTT trill Mod - - - - - - - - - -",                   
          "- EW Far - EYR - piping - - - - - - - - - - - - -",       
          "SC Chatter Mod - - EYR - piping - - WTT trill Mod - - - - - - - - - -",        
          "- - WTH Far - Lewins Far - WTT trill Far - - - - - - - - - -",                 
          "- EW Mod WTH Far - Lewins Far - - - - - - - - - - - -",   
          "- EW Mod WTH Far - - - WTT trill Mod - - - - - - - - - -",
          "- EW Far WTH Mod - Lewins Far - WTT trill Far - - - - - - - - - -",            
          "SC Far - - - Lewins Far - WTT trill Far - - - - - - - - - -",                  
          "- - WTH Far EYR Mod - Kookaburra Quiet - - - - - - - - - - -",                 
          "SC Chatter Mod - - - - - WTT piping Mod - - - - - - - - - -",                  
          "SC Single note Near EW Mod - - - - - - - - - - - - - - -",
          "SC Chatter Far - - - Lewins Mod - - - - - - - - - - - -", 
          "- - WTH Near EYR Mod - - - - - - - - - - - - -",          
          "SC Chatter Far - WTH Mod - Lewins Far - - - - - - - - - - - -",                
          "- EW Far - - - Kookaburra Quiet WTT trill Mod - - - - - - - - - -",            
          "SC Far EW Far - - Lewins Far - - - - - - - - - - - -",    
          "- - WTH Mod EYR Near - Kookaburra Quiet - - - - - - - - - - -",                
          "SC Chatter Far - - EYR Far - - - - - - - - - - - - -",    
          "SC Chatter Far - - EYR Mod - - - - - - - - - - - - -",    
          "SC Chatter Mod - - EYR Mod - - - - - - - - - - - - -",    
          "- EW Far - - Lewins Mod - WTT trill Mod - - - - - - - - - -",                  
          "- EW Mod - - Lewins not sure - - - - - - - - - - - -",    
          "- -  EYR Near - - - - - - WTH Mod - EYR Mod - - - -",     
          "- -  EYR Near - - - - - - WTH Far - EYR Mod - - - -",     
          "- EW Mod - - Lewins Near - WTT trill Mod - - - - - - - - - -",                 
          "- - - - - - - - SC Far - - WTH Near - - - WTT trill Mod -",                    
          "SC Mod EW Near - - - - WTT trill Mod - - - - - - - - - -",
          "SC Far EW Mod - - - - WTT trill Near - - - - - - - - - -",
          "- - - - - - - - - - - WTH Mod - - - WTT trill Near -",    
          "- - - - Lewins Mod Kookaburra Quiet - - - - - - - - - - -",                    
          "SC Near EW Mod - - - - WTT trill Far - - - - - - - - - -",
          "- - - - - - - - - - - WTH Near - - - WTT trill Near -",   
          "- EW Far - - Lewins not sure - - - - - - - - - - - -",    
          "- - WTH Mod EYR Mod - Kookaburra Quiet - - - - - - - - - - -",                 
          "SC Mod - - EYR Far - - - - - - - - - - - - -",            
          "SC Chatter Mod - WTH Far EYR Far - - - - - - - - - - - - -",                   
          "SC Chatter Mod - - EYR Far - - - - - - - - - - - - -",    
          "SC Mod - - EYR Mod - - - - - - - - - - - - -",            
          "SC Chatter Mod - - EYR Far - Kookaburra Quiet - - - - - - - - - - -",          
          "SC Chatter Near - - - Lewins Mod - - - - - - - - - - - -",
          "SC Mod - - EYR Far Lewins Mod - - - - - - - - - - - -",   
          "SC Mod - - EYR Far - - WTT trill fast - - - - - - - - - -",                    
          "SC Chatter Far - WTH Far - - Kookaburra Quiet - - - - - - - - - - -",          
          "SC Chatter Mod EW Far - - - - WTT trill Far - - - - - - - - - -",              
          "SC Chatter Mod - - - Lewins Far - WTT trill Far - - - - - - - - - -",          
          "SC Single note Far - - - - - WTT trill Far - - - - - - - - - -",               
          "SC Far - WTH Mod - Lewins Mod - - - - - - - - - - - -",   
          "SC Chatter Far - - - Lewins not sure - - - - - - - - - - - -",                 
          "SC Chatter Mod - WTH Mod EYR Far - - - - - - - - - - - - -",                   
          "SC Chatter Near - - EYR Far - - - - - - - - - - - - -",   
          "SC Mod EW Far - - Lewins Far - - - - - - - - - - - -",    
          "SC Near EW Far - - - - WTT trill Mod - - - - - - - - - -",
          "SC Chatter Mod EW Far - - - - WTT trill Mod - - - - - - - - - -",              
          "- EW Mod - - Lewins Far - WTT trill Mod - - - - - - - - - -",                  
          "- - - - - - - - SC Far - - - - - - WTT trill Near -",     
          "Rufous Fantail EW Mod - - - - - - - - - - - - - - -",     
          "SC Mod EW Mod - - Lewins Mod - - - - - - - - - - - -",    
          "SC Single note Mod EW Far - - - - - - - - - - - - - - -", 
          "SC Near - - - Lewins Far - - - - - - - - - - - -",        
          "SC Far - - - Lewins Mod Kookaburra Mod - - - - - - - - - - -",                 
          "SC Near EW Mod - - - - WTT trill Mod - - - - - - - - - -",
          "SC Near EW Near - - - - - - - - - - - - - - -",           
          "SC Mod EW Mod - - Lewins Far - - - - - - - - - - - -",    
          "SC Chatter Near - - EYR Mod - - - - - - - - - - - - -",   
          "SC Chatter Mod - - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",          
          "SC Chatter Near - - EYR - piping - - - - - - - - - - - - -",                   
          "SC Near - - EYR Mod - - - - - - - - - - - - -",           
          "SC Mod - - EYR - piping - - - - - - - - - - - - -",       
          "SC Near - - - - Kookaburra Quiet - - - - - - - - - - -",  
          "SC Single note Far EW Far - - - - - - - - - - - - - - -", 
          "SC Single note Far EW Near - - - - - - - - - - - - - - -",
          "SC Near - - - - - WTT piping Near - - - - - - - - - -",   
          "SC Chatter Near - WTH Mod - - - - - - - - - - - - - -",   
          "SC Chatter Mod - WTH Far EYR Mod - - - - - - - - - - - - -",                   
          "SC Chatter Far - - EYR Far Lewins Mod - - - - - - - - - - - -",                
          "SC Chatter Mod EW Far - EYR Far - - - - - - - - - - - - -",                    
          "SC Chatter Mod - - EYR Mod Lewins Far - - - - - - - - - - - -",                
          "SC Mod - WTH Mod - - Kookaburra Quiet - - - - - - - - - - -",                  
          "SC Mod - - - Lewins Far Kookaburra Quiet - - - - - - - - - - -",               
          "SC Chatter Mod EW Mod - - - - WTT trill Mod - - - - - - - - - -",              
          "SC Single note Mod - - - - - WTT trill Mod - - - - - - - - - -",               
          "SC Mod - - - - Kookaburra Mod - - - - - - - - - - -",     
          "SC Far - - - - Kookaburra Quiet WTT trill Mod - - - - - - - - - -",            
          "SC Near EW Near - - - Kookaburra Quiet - - - - - - - - - - -",                 
          "SC Mod EW Near - - - - WTT trill Far - - - - - - - - - -",
          "- EW Near - - - Kookaburra Mod - - - - - - - - - - -",    
          "- EW Near - - - Kookaburra Quiet - - - - - - - - - - -",  
          "SC Chatter Far EW Mod - - - - - - - - - - - - - - -",     
          "SC Far EW Far - - Lewins not sure - - - - - - - - - - - -",                    
          "- - - - - - - - SC Far EW Far - - - Lewins Far - - -",    
          "SC Far - - - Lewins not sure - - - - - - - - - - - -",    
          "SC Chatter Far - - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",          
          "SC Chatter Far - - EYR Far - Kookaburra Quiet - - - - - - - - - - -",          
          "SC Chatter Near - - - - Kookaburra Quiet - - - - - - - - - - -",               
          "SC Mod - - - Lewins Near - - - - - - - - - - - -",        
          "SC Far EW Near WTH Mod - - - - - - - - - - - - - -",      
          "SC Chatter Far - WTH Far EYR Far - - - - - - - - - - - - -",                   
          "SC Mod - - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",                  
          "- - - EYR Mod Lewins Far - - - - - - - - - - - -",        
          "SC Near - WTH Mod EYR Mod - - - - - - - - - - - - -",     
          "SC Chatter Mod - - EYR Mod Lewins Mod - - - - - - - - - - - -",                
          "SC Far - - EYR Mod Lewins Mod Kookaburra Quiet - - - - - - - - - - -",         
          "SC Chatter Mod - WTH Mod EYR Near - - - - - - - - - - - - -",                  
          "SC Mod - - EYR Near Lewins Far - - - - - - - - - - - -",  
          "SC Far - - EYR Near Lewins Far - - - - - - - - - - - -",  
          "SC Chatter Near - - - Lewins Far - - - - - - - - - - - -",
          " - - - Lewins Far - - - - - - - - - - - -",
          "SC Far EW Far - -  - - - - - - - - - - - -",              
          "SC Far EW Far - - - Kookaburra Quiet WTT trill Far - - - - - - - - - -",       
          "SC Mod EW Far - - - Kookaburra Quiet - - - - - - - - - - -",                   
          "SC Mod EW Near - - Lewins Far - - - - - - - - - - - -",   
          "SC Mod EW Far - - Lewins Mod - - - - - - - - - - - -",    
          "SC Mod EW Mod WTH Mod - - - - - - - - - - - - - -",       
          "SC Near - - - Lewins not sure - - - - - - - - - - - -")

list2 <- c("- - - - - - - - SC Mod EW Far - WTH Mod - - - - -",       
           "SC Far - WTH Mod - Lewins Far - - - - - - - - - - - -",   
           "- - - - - - - - SC Far - WTH Far - - - - WTT trill Mod -",
           "SC Far - WTH Far - - - WTT trill Mod - - - - - - - - - -",
           "- - WTH Mod - Lewins Mod - WTT trill Mod - - - - - - - - - -",                 
           "SC Mod EW Near  - Lewins Far - - - - - - - - - - - -",    
           "SC Mod EW Mod - - - - WTT trill Mod - - - - - - - - - -", 
           "SC Mod - - - Lewins Far - WTT trill Mod - - - - - - - - - -",                  
           "- - - - - - - - - EW Far - - - Lewins Far - WTT trill Mod -",                  
           "SC Mod - WTH Mod - Lewins Mod - - - - - - - - - - - -",   
            "SC Mod EW Far - - - - WTT trill Mod - - - - - - - - - -", 
           "SC Mod EW Mod - - Lewins Mod - WTT piping Near - - - - - - - - - -",           
           "- - WTH Mod EYR - piping Lewins Far - - - - - - - - - - - -",                  
           "- - WTH Far EYR - piping - - WTT trill Far - - - - - - - - - -",               
           "- EW Far WTH Mod - - - WTT trill Mod - - - - - - - - - -",
           "SC Mod EW Mod - - - - WTT trill Far - - - - - - - - - -", 
           "SC Far - WTH Far - - - WTT trill Far - - - - - - - - - -",
           "- EW Far WTH Near - Lewins Far - - - - - - - - - - - -",  
           "SC Single note Mod - - - Lewins Mod - WTT trill Mod - - - - - - - - - -",      
           "SC Far EW Mod - - - - WTT trill Far - - - - - - - - - -", 
           "- EW Mod WTH Far - - - WTT trill Far - - - - - - - - - -",
           "- EW Mod WTH Near - - - WTT trill Far - - - - - - - - - -",                    
           "SC Far EW Far WTH Mod - - - - - - - - - - - - - -",       
           "SC Far EW Far WTH Mod - - - WTT trill Mod - - - - - - - - - -",                
           "SC Far EW Mod WTH Near - - - WTT trill Mod - - - - - - - - - -",               
           "SC Mod - WTH Mod - - - WTT trill Far - - - - - - - - - -",
           "SC Far EW Mod - - - - WTT trill Mod - - - - - - - - - -", 
           "SC Far EW Far - - Lewins Far - WTT trill Mod - - - - - - - - - -",             
           "SC Mod - WTH Near - - - WTT trill Mod - - - - - - - - - -",                    
           "SC Near - WTH Near - - - WTT trill Mod - - - - - - - - - -",                   
           "SC Single note Near - - - - - WTT trill Mod - - - - - - - - - -",              
           "SC Near - - - Lewins Mod - WTT trill Far - - - - - - - - - -",                 
           "- - - - - - - - SC Far EW Far WTH Far - - - - - -",       
           "SC Far - WTH Mod - - Kookaburra Quiet - - - - - - - - - - -",                  
           "SC Chatter Mod - WTH Mod - - - WTT trill Far - - - - - - - - - -",             
           "- - WTH Mod - Lewins Far - WTT trill Far - - - - - - - - - -",                 
           "SC Far EW Far - - - - WTT trill Far - - - - - - - - - -", 
           "- EW Far WTH Far - - - WTT trill Far - - - - - - - - - -",
           "SC Mod EW Far WTH Mod - - - WTT trill Far - - - - - - - - - -",                
           "SC Far EW Far WTH Far - - - WTT trill Far - - - - - - - - - -",                
           "- EW Far WTH Mod - - - WTT trill Far - - - - - - - - - -",
           "SC Mod EW Far - - - - WTT trill Far - - - - - - - - - -", 
           "- -  - Lewins Far - - - - - WTH Near - - Lewins Far - - -",                    
           "SC Mod - WTH Far - - - WTT trill Far - - - - - - - - - -",
           "SC Far - WTH Far EYR Far - - - - - - - - - - - - -",      
           "- - WTH Mod EYR Far - - WTT trill Far - - - - - - - - - -",                    
           "SC Near - WTH Far - - - WTT trill Far - - - - - - - - - -",                    
           "SC Far EW Near - - - - WTT trill Far - - - - - - - - - -",
           "- - WTH Mod - Lewins Far Kookaburra Quiet - - - - - - - - - - -",              
           "- - WTH Far - - Kookaburra Quiet WTT trill Far - - - - - - - - - -",           
           "- - - - - - - - SC Far - WTH Mod - - - - WTT trill Far -",
           "- - WTH Far - Lewins Near - WTT trill Far - - - - - - - - - -",                
           "- EW Far WTH Mod - - - WTT piping Near - - - - - - - - - -",                   
           "- EW Mod WTH Far - - - WTT piping Far - - - - - - - - - -",                    
           "- EW Mod WTH Far - Lewins Mod - - - - - - - - - - - -",   
           "- - WTH Mod EYR Far - Kookaburra Quiet - - - - - - - - - - -",                 
           "SC Mod - - EYR Far - Kookaburra Quiet - - - - - - - - - - -",                  
           "SC Far - WTH Mod - Lewins Near - - - - - - - - - - - -",  
           "- - - - - - - - SC Far - WTH Far - - - Kookaburra Quiet - -",                  
           "- - - - - - - - - - WTH Mod - - Lewins Mod Kookaburra Quiet - -",              
           "- - - - - - - - - EW Mod WTH Mod - - Lewins Far - - -",   
           "- - - - - - - - SC Far - WTH Far - EYR Far - - - -",      
           "- - - - - - - - SC Mod EW Far WTH Far - - - - - -",       
           "- - - - - - - - - EW Far - WTH Mod - - - WTT trill Mod -",
           "- - - - - - - - SC Far EW Mod WTH Far - - - - - -",       
           "- - - - - - - SC Chatter Far - EW Far - - - - - WTT trill Mod -",              
           "- - - - - - - SC Chatter Far - EW Mod - - - - - WTT trill Mod -",              
           "- - - - - - - SC Chatter Far - - WTH Mod - - - - WTT trill Far -",             
           "- - - - - - - SC Chatter Mod - EW Far WTH Mod - - - - - -",                    
           "- - - - - - - SC Chatter Far - - WTH Mod - EYR Mod - - - -",                   
           "- - - - - - - - SC Far EW Far WTH Mod - - - - - -",       
           "- - - - - - - SC Chatter Far - EW Far WTH Mod - - - - - -",                    
           "- - - - - - - SC Chatter Near - - - - - Lewins Mod - WTT trill Far -",         
           "- - - - - - - - - EW Far WTH Far - - - - WTT trill Far -",
           "- - - - - - - - SC Far EW Far - - - Lewins Mod - - -",    
           "- - - - - - - - - EW Far WTH Far WTH Mod - - - - -",      
           "- - - - - - - - SC Far EW Far - WTH Mod - - - - -",       
           "- - - - - - - - SC Far - WTH Mod - - - - WTT trill Mod -",
           "- - - - - - - - - EW Far WTH Far - - - - WTT trill Mod -",
           "- - - - - - - - - EW Far WTH Mod - - - - WTT trill Mod -",
           "- - - - - - - - - EW Near - WTH Mod - Lewins Far Kookaburra Quiet - -",        
           "- - - - - - - SC Chatter Mod - - WTH Mod - - - - WTT trill Far -",             
           "- - - - - - - - - EW Mod - WTH Mod - - - WTT trill Far -",
           "- - - - - - - - - EW Near - WTH Mod - - - WTT trill Far -",                    
           "- - - - - - - SC Chatter Far - EW Far - - - - - WTT trill Far -",              
           "- - - - - - - - - - WTH Mod - EYR Near - - WTT trill Mod -",                   
           "- - - - - - - - - - WTH Mod - - Lewins Mod - WTT trill Mod -",                 
           "- - - - - - - - SC Far - WTH Mod - - Lewins Far - WTT trill Mod -",            
           "- - - - - - - - SC Far EW Far - - - - - WTT trill Mod -", 
           "- - - - - - - - - EW Mod - - - Lewins Far - WTT trill Mod -",                  
           "- - - - - - - - SC Mod EW Far - WTH Mod - - - WTT trill Far -",                
           "- - - - - - - - - EW Far SC added - - Lewins Far - - -",  
           "- - - - - - - - SC Far EW Mod - - - - Kookaburra Quiet WTT trill Far -",       
           "- - - - - - - - - - WTH Near WTH Mod - Lewins Far - - -", 
           "- - - - - - - - SC Far - WTH Far - - - - WTT trill Far -",
           "- - - - - - - - - EW Far WTH Near - - - - WTT trill Far -",                    
           "- - - - - - - - SC Far EW Far - - - - - WTT trill Far -", 
           "- - - - - - - SC Chatter Near - - WTH Near - - - - WTT trill Far -",           
           "- - - - - - - SC Chatter Near - EW Far - WTH Near - - - WTT trill Far -",      
           "- - - - - - - - - - - WTH Mod EYR Mod Lewins Mod - - -",  
           "- EW Far - - - - WTT trill Far - SC Far - - - - - - - -", 
           "- - - - - - - - SC Far - - - EYR Far - Kookaburra Quiet - -",                  
           "SC Chatter Far EW Far - - Lewins Far - - - - - - - - - - - -",                 
           "- - WTH Far EYR Near - Kookaburra Quiet - - - - - - - - - - -",                
           "- - WTH Far EYR Far - Kookaburra Quiet - - - - - - - - - - -",                 
           "- - WTH Mod EYR Far Lewins Far - - - - - - - - - - - -",  
           "- - WTH Far EYR Far Lewins Far - - - - - - - - - - - -",  
           "- EW Mod WTH Mod - - Kookaburra Quiet WTT trill Far - - - - - - - - - -",      
           "SC Chatter Near EW Near - - - - WTT trill Far - - - - - - - - - -",            
           "SC Mod EW Near - - - - WTT trill Near - - - - - - - - - -",                    
           "SC Mod - - - Lewins Mod - WTT trill Near - - - - - - - - - -",                 
           "SC Mod EW Mod - - - - WTT trill Near - - - - - - - - - -",
           "- EW Mod WTH Mod - - - WTT trill Mod - - - - - - - - - -",
           "- EW Mod WTH Mod - - - WTT trill Far - - - - - - - - - -",
           "- - WTH Mod - - Kookaburra Quiet WTT trill Mod - - - - - - - - - -",           
           "- EW Far WTH Mod - - Kookaburra Quiet - - - - - - - - - - -",                  
           "- - - - - - - - SC Far - WTH Mod - EYR Mod - - - -",      
           "SC Far EW Mod - - - Kookaburra Quiet - - - - - - - - - - -",                   
           "SC Chatter Far EW Far - - - - WTT trill Far - - - - - - - - - -",              
           "- EW Far - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",                  
           "- EW Mod WTH Mod EYR Mod - - - - - - - - - - - - -",      
           "SC Far EW Far WTH Far - - - - - - - - - - - - - -",       
           "- EW Far WTH Far EYR Far - - - - - - - - - - - - -",      
           "- - - - - - - - SC Far - WTH Far - EYR Mod - - - -",      
           "SC Chatter Mod - - EYR Near - Kookaburra Quiet - - - - - - - - - - -",         
           "- - WTH Near EYR Near - - WTT trill Far - - - - - - - - - -",                  
           "- - WTH Far EYR Mod Lewins Far - - - - - - - - - - - -",  
           "SC Single note Mod - WTH Mod - Lewins Far - WTT trill Far - - - - - - - - - -",
           "SC Chatter Mod - - EYR - piping - - WTT trill Mod - - - - - - - - - -",        
           "- - WTH Far - Lewins Far - WTT trill Far - - - - - - - - - -",                 
           "- EW Far WTH Mod - Lewins Far - WTT trill Far - - - - - - - - - -",            
           "SC Far - - - Lewins Far - WTT trill Far - - - - - - - - - -",                  
           "SC Chatter Far - WTH Mod - Lewins Far - - - - - - - - - - - -",                
           "- EW Far - - - Kookaburra Quiet WTT trill Mod - - - - - - - - - -",            
           "SC Far EW Far - - Lewins Far - - - - - - - - - - - -",    
           "- - WTH Mod EYR Near - Kookaburra Quiet - - - - - - - - - - -",                
           "- EW Far - - Lewins Mod - WTT trill Mod - - - - - - - - - -",                  
           "- EW Mod - - Lewins Near - WTT trill Mod - - - - - - - - - -",                 
           "- - - - - - - - SC Far - - WTH Near - - - WTT trill Mod -",                    
           "- - WTH Mod EYR Mod - Kookaburra Quiet - - - - - - - - - - -",                 
           "SC Chatter Mod - WTH Far EYR Far - - - - - - - - - - - - -",                   
           "SC Chatter Mod - - EYR Far - Kookaburra Quiet - - - - - - - - - - -",          
           "SC Mod - - EYR Far Lewins Mod - - - - - - - - - - - -",   
           "SC Mod - - EYR Far - - WTT trill fast - - - - - - - - - -",                    
           "SC Chatter Far - WTH Far - - Kookaburra Quiet - - - - - - - - - - -",          
           "SC Chatter Mod EW Far - - - - WTT trill Far - - - - - - - - - -",              
           "SC Chatter Mod - - - Lewins Far - WTT trill Far - - - - - - - - - -",          
           "SC Chatter Mod EW Far - - - - WTT trill Mod - - - - - - - - - -",              
           "- EW Mod - - Lewins Far - WTT trill Mod - - - - - - - - - -",                  
           "SC Mod EW Mod - - Lewins Mod - - - - - - - - - - - -",    
           "SC Chatter Mod - - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",          
           "SC Chatter Mod - WTH Far EYR Mod - - - - - - - - - - - - -",                   
           "SC Chatter Mod EW Far - EYR Far - - - - - - - - - - - - -",                    
           "SC Chatter Mod - - EYR Mod Lewins Far - - - - - - - - - - - -",                
           "SC Mod - WTH Mod - - Kookaburra Quiet - - - - - - - - - - -",                  
           "SC Mod - - - Lewins Far Kookaburra Quiet - - - - - - - - - - -",               
           "SC Chatter Mod EW Mod - - - - WTT trill Mod - - - - - - - - - -",              
           "SC Far - - - - Kookaburra Quiet WTT trill Mod - - - - - - - - - -",            
           "SC Near EW Near - - - Kookaburra Quiet - - - - - - - - - - -",                 
           "SC Mod EW Near - - - - WTT trill Far - - - - - - - - - -",
           "SC Far EW Far - - Lewins not sure - - - - - - - - - - - -",                    
           "- - - - - - - - SC Far EW Far - - - Lewins Far - - -",    
           "SC Chatter Far - - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",          
           "SC Chatter Far - - EYR Far - Kookaburra Quiet - - - - - - - - - - -",          
           "SC Far EW Near WTH Mod - - - - - - - - - - - - - -",      
           "SC Chatter Far - WTH Far EYR Far - - - - - - - - - - - - -",                   
           "SC Mod - - EYR Mod - Kookaburra Quiet - - - - - - - - - - -",                  
           "SC Near - WTH Mod EYR Mod - - - - - - - - - - - - -",     
           "SC Chatter Mod - - EYR Mod Lewins Mod - - - - - - - - - - - -",                
           "SC Far - - EYR Mod Lewins Mod Kookaburra Quiet - - - - - - - - - - -",         
           "SC Chatter Mod - WTH Mod EYR Near - - - - - - - - - - - - -",                  
           "SC Mod - - EYR Near Lewins Far - - - - - - - - - - - -",  
           "SC Far - - EYR Near Lewins Far - - - - - - - - - - - -",  
           "SC Far EW Far - - - Kookaburra Quiet WTT trill Far - - - - - - - - - -",       
           "SC Mod EW Near - - Lewins Far - - - - - - - - - - - -",   
           "SC Mod EW Far - - Lewins Mod - - - - - - - - - - - -",    
           "SC Mod EW Mod WTH Mod - - - - - - - - - - - - - -")

length_of_two_species <- NULL
for(i in 1:length(list1)) {
  a <- which(bird_data$MANUAL_ID==list1[i])
  l <- length(a)
  length_of_two_species <- c(length_of_two_species, l)
}
sum(length_of_two_species)

length_of_more_than_two_species <- NULL
for(i in 1:length(list2)) {
  a <- which(bird_data$MANUAL_ID==list2[i])
  l <- length(a)
  length_of_more_than_two_species <- c(length_of_more_than_two_species, l)
}
sum(length_of_more_than_two_species)

length_of_any_num_species <- NULL
for(i in 2:length(list)) {
  a <- which(bird_data$MANUAL_ID==list[i])
  l <- length(a)
  length_of_any_num_species <- c(length_of_any_num_species, l)
}
sum(length_of_any_num_species)

#0.22*8636+0.24*82+0.27*1958+0.29*197+.4*1705+0.52*1877+0.54*61+0.83*479