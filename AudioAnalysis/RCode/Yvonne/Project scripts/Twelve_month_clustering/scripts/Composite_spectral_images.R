rm(list = ls())
# Note the SPT file for 20151013 at Woondum is empty, 
# perhaps the concat code needs
# to be re-run for this date and index
cluster_desc <- c("INSECTS", "LIGHT RAIN AND BIRDS", 
                  "BIRDS", "INSECTS AND BIRDS",
                  "FAIRLY QUIET","MOSTLY QUIET", 
                  "CICADAS AND BIRDS AND WIND", "CICADAS AND BIRDS AND WIND", 
                  "MODERATE WIND", "LIGHT TO MODERATE RAIN", 
                  "BIRDS", "CICADAS", 
                  "QUIET", "BIRDS", 
                  "BIRDS", "CICADAS", 
                  "LIGHT RAIN OR INSECTS","MODERATE RAIN", 
                  "MODERATE WIND", "MODERATE WIND",
                  "LIGHT RAIN", "INSECTS AND BIRDS", 
                  "PLANES AND MOTORBIKES AND THUNDER", "WIND AND/OR CICADAS",
                  "WIND", "INSECTS AND WIND",
                  "INSECTS", "BIRDS AND/OR INSECTS OR PLANES",
                  "INSECTS", "BIRDS AND QUIET",
                  "QUIET", "CICADAS",
                  "BIRDS", "CICADAS",
                  "QUIET", "QUIET AND/OR PLANES",
                  "MORNING CHORUS", "QUIET",
                  "BIRDS AND PLANES", "WIND AND/OR BIRDS OR INSECTS",
                  "VERY QUIET", "STRONG WIND",
                  "MORNING CHORUS", "LOUD CICADAS",
                  "WIND AND PLANES", "WIND",
                  "VERY STRONG WIND", "CICADAS",
                  "PLANES (INCLUDING THUNDER)", "QUIET AND INSECTS AND BIRDS",
                  "STRONG WIND", "WIND",
                  "MOSTLY QUIET", "MODERATE RAIN AND BIRDS",
                  "QUIET", "WIND",
                  "BIRDS OR WIND", "LOUD BIRDS",
                  "MODERATE RAIN", "RAIN or/BIRDS")

# generate a sequence of dates
start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20160723", format="%Y%m%d")
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
rm(dat, dates, i, x,start, finish)

load(file="C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\missing_minutes_summary_indices.RData")
# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60
cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)

# Get list of names (full path) of all spectrogram files
path <- "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\concat Output" # path to spectrogram files
index <- "POW"
spect_file_Gympie <- list.files(full.names=TRUE, recursive = T,
                                path = paste(path,"\\GympieNP",sep=""),
                                pattern = paste("*",index, ".csv", sep=""))
#remove the 20160724 which is a partial file
spect_file_Gympie <- spect_file_Gympie[-length(spect_file_Gympie)]
spect_file_Woondum <- list.files(full.names=TRUE, recursive = T,
                                 path = paste(path,"\\Woondum3",sep=""),
                                 pattern = paste("*", index, ".csv", sep="")) 
#remove the 20160724 which is a partial file
spect_file_Woondum <- spect_file_Woondum[-length(spect_file_Woondum)]

spect_file_list <- c(spect_file_Gympie, spect_file_Woondum)
rm(path, spect_file_Gympie, spect_file_Woondum)

length <- length(unique(spect_file_list))
length
if(length < 796) { stop("Less files than expected in 'length': n < 796")}
if(length > 796) { stop("More files than expected in 'length': n > 796")}

statistics <- data.frame(date="20150622",
                         min=1, site="NP",
                         v1=1, V2=1, V3=1, V4=1, V5=1, V6=1,
                         v7=1, V8=1, V9=1, V10=1, V11=1, V12=1,
                         v13=1, V14=1, V15=1, V16=1, V17=1, V18=1,
                         v19=1, V20=1, V21=1, V22=1, V23=1, V24=1,
                         v25=1, V26=1, V27=1, V28=1, V29=1, V30=1,
                         v31=1, V32=1, V33=1, V34=1, V35=1, V36=1,
                         v37=1, V38=1, V39=1, V40=1, V41=1, V42=1,
                         v43=1, V44=1, V45=1, V46=1, V47=1, V48=1,
                         v49=1, V50=1, V51=1, V52=1, V53=1, V54=1,
                         v55=1, V56=1, V57=1, V58=1, V59=1, V60=1,
                         v61=1, V62=1, V63=1, V64=1)

list <- c(1:60)
#list <- 58
for(i in list) {
  clust_num <- i
  # set up statistics dataframe
  statistics <- data.frame(date="20150622",
                           min=1, site="NP",
                           v1=1, V2=1, V3=1, V4=1, V5=1, V6=1,
                           v7=1, V8=1, V9=1, V10=1, V11=1, V12=1,
                           v13=1, V14=1, V15=1, V16=1, V17=1, V18=1,
                           v19=1, V20=1, V21=1, V22=1, V23=1, V24=1,
                           v25=1, V26=1, V27=1, V28=1, V29=1, V30=1,
                           v31=1, V32=1, V33=1, V34=1, V35=1, V36=1,
                           v37=1, V38=1, V39=1, V40=1, V41=1, V42=1,
                           v43=1, V44=1, V45=1, V46=1, V47=1, V48=1,
                           v49=1, V50=1, V51=1, V52=1, V53=1, V54=1,
                           v55=1, V56=1, V57=1, V58=1, V59=1, V60=1,
                           v61=1, V62=1, V63=1, V64=1, v64=1, v65=1,
                           v66=1, v67=1, v68=1, v69=1, v70=1, v71=1,
                           v72=1, v73=1, v74=1, v75=1, v76=1, v77=1,
                           v78=1, v79=1, v80=1, v81=1, v82=1, v83=1,
                           v84=1, v85=1, v86=1, v87=1, v88=1, v89=1,
                           v90=1, v91=1, v92=1, v93=1, v94=1, v95=1,
                           v96=1, v97=1, v98=1, v99=1, v100=1, v101=1,
                           v102=1, v103=1, v104=1, v105=1, v106=1, v107=1,
                           v108=1, v109=1, v110=1, v111=1, v112=1, v113=1,
                           v114=1, v115=1, v116=1, v117=1, v118=1, v119=1,
                           v120=1, v121=1, v122=1, v123=1, v124=1, v125=1,
                           v126=1, v127=1, v128=1, v129=1, v130=1, v131=1,
                           v132=1, v133=1, v134=1, v135=1, v136=1, v137=1,
                           v138=1, v139=1, v140=1, v141=1, v142=1, v143=1,
                           v144=1, v145=1, v146=1, v147=1, v148=1, v149=1,
                           v150=1, v151=1, v152=1, v153=1, v154=1, v155=1,
                           v156=1, v157=1, v158=1, v159=1, v160=1, v161=1,
                           v162=1, v163=1, v164=1, v165=1, v166=1, v167=1,
                           v168=1, v169=1, v170=1, v171=1, v172=1, v173=1,
                           v174=1, v175=1, v176=1, v177=1, v178=1, v179=1,
                           v180=1, v181=1, v182=1, v183=1, v184=1, v185=1,
                           v186=1, v187=1, v188=1, v189=1, v190=1, v191=1,
                           v192=1, v193=1, v194=1, v195=1, v196=1, v197=1,
                           v198=1, v199=1, v200=1, v201=1, v202=1, v203=1,
                           v204=1, v205=1, v206=1, v207=1, v208=1, v209=1,
                           v210=1, v211=1, v212=1, v213=1, v214=1, v215=1,
                           v216=1, v217=1, v218=1, v219=1, v220=1, v221=1,
                           v222=1, v223=1, v224=1, v225=1, v226=1, v227=1,
                           v228=1, v229=1, v230=1, v231=1, v232=1, v233=1,
                           v234=1, v235=1, v236=1, v237=1, v238=1, v239=1,
                           v240=1, v241=1, v242=1, v243=1, v244=1, v245=1,
                           v246=1, v247=1, v248=1, v249=1, v250=1, v251=1,
                           v252=1, v253=1, v254=1, v255=1, v256=1) 
  
  statistics$date <- as.character("20150622")
  statistics$site <- as.character("NP")
  sample_size <- 600
  # Get list of positions of cluster
  a <<- which(cluster_list$cluster_list==clust_num)
  # Select a random sample from a cluster
  b <<- sample(a, sample_size)
  for(i in 1:length(b)) {
    ref <<- b[i]
    ifelse(ref %in% c(seq(1440,length(spect_file_list)*1440, 1440)),
           day.ref <- floor(ref/1440),
           day.ref <- floor(ref/1440)+1)
    min.ref <<- ((ref/1440) - (day.ref-1))*1440
    min.ref <- round(min.ref,0)
    # select the twenty-four hour spectrogram image
    b1 <<- spect_file_list[day.ref]
    # read spectral power csv file
    sp_index <<- read.csv(b1, header = T)
    if(day.ref <= 398) {
      statistics[i,1] <- date.list[day.ref]
    }
    if(day.ref > 398) {
      statistics[i,1] <- date.list[(day.ref-398)]
    }
    statistics[i,2] <- min.ref
    if(day.ref <= 398) {
      statistics[i,3] <- "GympieNP"  
    }
    if(day.ref > 398) {
      statistics[i,3] <- "WoondumNP"  
    }
    # Find 64 averages of each group of four frequency bins
    #seq <- seq(2,(ncol(sp_index)-1), by = 4)
    #for(k in 1:length(seq)) {
    #  av <- mean(c(sp_index[min.ref,seq[k]], sp_index[min.ref,(seq[k]+1)],
    #               sp_index[min.ref,(seq[k]+2)], sp_index[min.ref,(seq[k]+3)]))
    #  statistics[i,(k+3)] <- av
    #}
    
    statistics[i,4:ncol(statistics)] <- sp_index[min.ref,2:257] 
    print(paste("Completed", i))
  }
  write.csv(statistics, paste("cluster_statistics_600_", index, "_clust_full", clust_num,".csv", sep=""), row.names = F)
}

dev.off()
par(mfrow=c(2,3), mar=c(3,3,1,1), oma = c(1,2,1,1),
    cex = 1, cex.lab = 2.6)

if(index=="POW") {
  ylim <- c(0, 2.4)  
}
if(index=="BGN") {
  ylim <- c(-110, -50)  
}
if(index=="ACI") {
  ylim <- c(0.35, 0.75)  
}
if(index=="ENT") {
  ylim <- c(0, 0.6)  
}
if(index=="SPT") {
  ylim <- c(0, 0.15)  
}
if(index=="EVN") {
  ylim <- c(0, 2.3)  
}

library(seewave)
library(sm) # this is for the function 'pause'
index
list <- c(1:60)
for(i in list) {
  stats <- read.csv(paste("cluster_statistics_600_",index, "_clust_full",i,".csv",sep=""), header=T)[,5:260]
  indices <- c("ACI","EVN","ENT","POW","SPT")
  if(index %in% indices) {
    mean <- NULL
    sd <- NULL
    sd.up <- NULL
    sd.down <- NULL
    for(j in 1:ncol(stats)) {
      m <- mean(as.numeric(stats[,j]), na.rm=T)
      mean <- c(mean, m)
      s <- sd(as.numeric(stats[,j]), na.rm=T)
      sd <- c(sd, s)
    }  
  }
  if(index=="BGN") {
    mean <- NULL
    sd <- NULL
    sd.up <- NULL
    sd.down <- NULL
    max <- NULL
    for(j in 1:ncol(stats)) {
      m <- -exp(mean(log(as.numeric(abs(stats[,j])))))
      mean <- c(mean, m)
      s <- sddB(as.numeric(abs(stats[,j])))
      sd <- c(sd, s)
    }
  }
  sd.up <- mean + 2*sd
  sd.dn <- mean - 2*sd
  plot(mean, type="l", ylim=ylim, 
       xlab="", xaxt='n')
  mtext(side=2,line=2, paste("average",index,"+/- 2 s.d."), cex=0.8)
  par(new=T)
  plot(sd.up, type="l", ylim=ylim, 
       xlab="",ylab="",xaxt='n', col="red")
  par(new=T)
  plot(sd.dn, type="l", ylim=ylim, 
       xlab="",ylab="",xaxt='n', col="red")
  mtext(side=3, paste("Cluster ",i,sep=""))
  mtext(side=3, line = -1, paste(cluster_desc[i]), cex=0.6)
  axis(side = 1, at = seq(0, 256,length.out = 12), label = 0:11)
  pause()
}

# use to get six plots (one of each index) for one cluster only
dev.off()

library(seewave)
library(sm) # this is for the function 'pause'

return()
a <- 20
tiff(paste("ENT_ACI_EVN_POW__SPT_BGN_",a,".tiff",sep=""), height=1300, width=2400, res = 300)
list <- c(3,11,14,15,33,37,39,43,58)
list <- 1:60
indices_all <- c("ENT","ACI","EVN","POW","SPT", "BGN")
par(mfrow=c(3,6), mar=c(0,1.6,1.6,1), oma = c(2.2,1,0,0),
    cex = 0.8, cex.lab = 2.6)

list <- list[((a-1)*3+1):(a*3)]
for(t in 1:length(list)) {
  i <- list[t]
  for(k in 1:length(indices_all)) {
    index <- indices_all[k]
    if(index=="POW") {
      ylim <- c(0, 16)  
    }
    if(index=="BGN") {
      ylim <- c(-110, -40)  
    }
    if(index=="ACI") {
      ylim <- c(0.15, 1.4)  
    }
    if(index=="ENT") {
      ylim <- c(0, 1.2)  
    }
    if(index=="SPT") {
      ylim <- c(0, 0.3)  
    }
    if(index=="EVN") {
      ylim <- c(0, 5)  
    }
    stats <- read.csv(paste("cluster_statistics_600_",index, "_clust_full",i,".csv",sep=""), header=T)[,5:260]
    indices <- c("ENT","ACI","EVN","POW","SPT")
    if(index %in% indices) {
      mean <- NULL
      sd <- NULL
      sd.up <- NULL
      sd.down <- NULL
      min <- NULL
      max <- NULL
      for(j in 1:ncol(stats)) {
        m <- mean(as.numeric(stats[,j]), na.rm=T)
        mean <- c(mean, m)
        s <- sd(as.numeric(stats[,j]), na.rm=T)
        sd <- c(sd, s)
        mn <-  min(as.numeric(stats[,j]), na.rm=T)
        min <- c(min, mn)
        mx <-  max(as.numeric((stats[,j])), na.rm=T)
        max <- c(max, mx)
      }  
    }
    if(index=="BGN") {
      mean <- NULL
      sd <- NULL
      sd.up <- NULL
      sd.down <- NULL
      max <- NULL
      min <- NULL
      max <- NULL
      for(j in 1:ncol(stats)) {
        m <- -exp(mean(log(as.numeric(abs(stats[,j])))))
        mean <- c(mean, m)
        s <- sddB(as.numeric(abs(stats[,j])))
        sd <- c(sd, s)
        mn <- -max(as.numeric(abs(stats[,j])))
        min <- c(min, mn)
        mx <- -min(as.numeric(abs(stats[,j])))
        max <- c(max, mx)
      }
    }
    sd.up <- mean + 3*sd
    sd.dn <- mean - 3*sd
    plot(mean, type="l", ylim=ylim, ylab="",
         xlab="", xaxt='n', mgp=c(0,0.5,0))
    if(index=="EVN") { 
      mtext(side=3, line= 0.3, 
            paste("                                   Cluster ",i,sep=""))
    }
    if(t %in% c(3,6,9,12) & index=="EVN") { # t is indices
      mtext(side=1, line=1.1, 
            "                                      Frequency (kHz)")
    }
    mtext(side=2,line=1.3, paste("average",index), cex=0.8)
    par(new=T)
    plot(sd.up, type="l", ylim=ylim, yaxt="n",
         xlab="",ylab="",xaxt='n', col="red")
    #par(new=T)
    #plot(sd.dn, type="l", ylim=ylim, yaxt="n", 
    #     xlab="",ylab="",xaxt='n', col="red")
    mtext(side=3, line = -1, paste(cluster_desc[i]), cex=0.6)
    par(new=T)
    plot(min, type="l", ylim=ylim, yaxt="n",
         xlab="",ylab="",xaxt='n', col="blue")
    par(new=T)
    plot(max, type="l", ylim=ylim, yaxt="n",
         xlab="",ylab="",xaxt='n', col="green")
    if(t %in% c(3,6,9,12)) {
      axis(side = 1, at = seq(0, 256,length.out = 12), 
           label = 0:11, mgp=c(0,0.1,0))
    }
    #pause()
  }
}
dev.off()

# Special cluster 58 plot ---------------------------
stats <- read.csv("cluster_statistics_600_POWall_clust_full58.csv", header = T)

indices <- c("POW")
if(index %in% indices) {
  mean <- NULL
  sd <- NULL
  sd.up <- NULL
  sd.down <- NULL
  min <- NULL
  max <- NULL
  for(j in 1:ncol(stats)) {
    m <- mean(as.numeric(stats[,j]), na.rm=T)
    mean <- c(mean, m)
    s <- sd(as.numeric(stats[,j]), na.rm=T)
    sd <- c(sd, s)
    mn <-  min(as.numeric(stats[,j]), na.rm=T)
    min <- c(min, mn)
    mx <-  max(as.numeric((stats[,j])), na.rm=T)
    max <- c(max, mx)
  }  
}
par(mfrow=c(2,3), mar=c(3,3,1,1), oma = c(1,2,1,1),
    cex = 1, cex.lab = 2.6)

if(index=="POW") {
  ylim <- c(0, 2.4)  
}
ylim <- c(0,5.6)
sd.up <- mean + 2*sd
sd.dn <- mean - 2*sd
ylim <- c(0, 5.6)
xlim <- c(0, 100)
tiff("Cluster_58_Kookaburra_frequency.tiff", height = 1000, width=1200, res=300)
par(mar=c(3,3,1,1), cex=0.9)
plot(mean, type="l", ylim=ylim, xlim=xlim, 
     xaxt='n', las=1)
mtext(side=2, line=1.6,"average Power")
mtext(side=1, line=1.6,"Frequency (kHz)")
axis(side = 1, at = seq(0, 256,length.out = 12), label = 0:11)
abline(v=c(17.8, 35.6)) # fundamental frequency 765, 1530 Hz
dev.off()