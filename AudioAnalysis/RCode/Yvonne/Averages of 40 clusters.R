setwd("F:\\Work\\CSV files\\FourMonths\\Data_40\\")

norm.dat <- read.csv("C:\\Users\\n0572527\\Desktop\\ds3.norm_2_98.csv", header = T)

##############################
setwd("C:\\Users\\n0572527\\Desktop\\")
clusters <- read.csv("C:\\Users\\n0572527\\Desktop\\hybrid_clust_knn_17500_3.csv",header=T)

#norm.dat <- norm.dat[c(1:(length(norm.dat$X)/2),(length(norm.dat$X)/2+1):(length(norm.dat$X))),]

norm.dat <- cbind(norm.dat[2:length(norm.dat)], clusters[9])
norm.dat1 <- norm.dat[1:(length(norm.dat$Snr)/2),]
norm.dat2 <- norm.dat[(length(norm.dat$Snr)/2)+1:length(norm.dat$Snr),]
setwd("C:\\Users\\n0572527\\Desktop\\Data_40\\")
for (i in unique(clusters$hybrid_k17500k40k3)) {
    Name <- paste("Gympie",i,".csv",sep="")
    write.csv(subset(norm.dat1, hybrid_k17500k40k3==i, row.names=F), Name, row.names=F)
}

for (i in unique(clusters$hybrid_k17500k40k3)) {
  Name <- paste("Woondum",i,".csv",sep="")
  write.csv(subset(norm.dat2, hybrid_k17500k40k3==i, row.names=F), Name, row.names=F)
}

for (i in unique(clusters$hybrid_k17500k40k3)) {
  Name <- paste("All",i,".csv",sep="")
  write.csv(subset(norm.dat, hybrid_k17500k40k3==i, row.names=F), Name, row.names=F)
}

#source("F:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\shared\\sort.Filename.R")

#site1.list <- list.files(full.names=FALSE, pattern="Gympie.*.csv", path="C:\\Users\\n0572527//Desktop")
#site1 <- sort.Filename(site1.list)
#### Calculate the means
#site1.list <- list.files(full.names=FALSE, pattern="Gympie.*.csv", path="C:\\Users\\n0572527//Desktop")
site1.list <- c("Gympie1.csv","Gympie2.csv","Gympie3.csv","Gympie4.csv","Gympie5.csv","Gympie6.csv"
                ,"Gympie7.csv","Gympie8.csv","Gympie9.csv","Gympie10.csv","Gympie11.csv","Gympie12.csv"
                ,"Gympie13.csv","Gympie14.csv","Gympie15.csv","Gympie16.csv","Gympie17.csv"
                ,"Gympie18.csv","Gympie19.csv","Gympie20.csv","Gympie21.csv","Gympie22.csv","Gympie23.csv"
                ,"Gympie24.csv","Gympie25.csv","Gympie26.csv","Gympie27.csv","Gympie28.csv"
                ,"Gympie29.csv","Gympie30.csv","Gympie31.csv","Gympie32.csv"
                ,"Gympie33.csv","Gympie34.csv","Gympie35.csv","Gympie36.csv","Gympie37.csv"
                ,"Gympie38.csv","Gympie39.csv","Gympie40.csv")
#site1.list <- ls(pattern="Gympie*") 
#site2.list <- ls(pattern="Woondum*")

site2.list <- c("Woondum1.csv","Woondum2.csv","Woondum3.csv","Woondum4.csv","Woondum5.csv","Woondum6.csv"
                ,"Woondum7.csv","Woondum8.csv","Woondum9.csv","Woondum10.csv","Woondum11.csv","Woondum12.csv"
                ,"Woondum13.csv","Woondum14.csv","Woondum15.csv","Woondum16.csv","Woondum17.csv"
                ,"Woondum18.csv","Woondum19.csv","Woondum20.csv","Woondum21.csv","Woondum22.csv","Woondum23.csv"
                ,"Woondum24.csv","Woondum25.csv","Woondum26.csv","Woondum27.csv","Woondum28.csv"
                ,"Woondum29.csv","Woondum30.csv","Woondum31.csv","Woondum32.csv"
                ,"Woondum33.csv","Woondum34.csv","Woondum35.csv","Woondum36.csv","Woondum37.csv"
                ,"Woondum38.csv","Woondum39.csv","Woondum40.csv")

all.Gympie.averages <- NULL
path <- "C:\\Users\\n0572527\\Desktop\\Data_40\\"

for (i in site1.list) {
  Name <- (paste(path,i,sep =""))
  averages <- NULL
  for(j in 1:7) {  # 7 is the number of indices
    assign(paste("fileContents"), read.csv(Name))
    index.average <- mean(fileContents[,j], na.rm=TRUE)
    averages <- cbind(averages, index.average)
  }
  all.Gympie.averages <- rbind(all.Gympie.averages, averages)
}


all.Woondum.averages <- NULL
path <- "C:\\Users\\n0572527\\Desktop\\Data_40\\"

for (i in site2.list) {
  Name <- (paste(path,i,sep =""))
  averages <- NULL
  for(j in 1:7) {  # 7 is the number of indices
    assign(paste("fileContents"), read.csv(Name))
    index.average <- mean(fileContents[,j], na.rm=TRUE)
    averages <- cbind(averages, index.average)
  }
  all.Woondum.averages <- rbind(all.Woondum.averages, averages)
}

all.averages <- rbind(all.Gympie.averages, all.Woondum.averages)
site.list <- c("G1","G2","G3","G4","G5","G6","G7","G8","G9","G10",
                "G11","G12","G13","G14","G15","G16","G17","G18","G19","G20",
               "G21","G22","G23","G24","G25","G26","G27","G28","G29","G30",
               "G31","G32","G33","G34","G35","G36","G37","G38","G39","G40",
               "W1","W2","W3","W4","W5","W6","W7","W8","W9","W10",
               "W11","W12","W13","W14","W15","W16","W17","W18","W19","W20",
               "W21","W22","W23","W24","W25","W26","W27","W28","W29","W30",
               "W31","W32","W33","W34","W35","W36","W37","W38","W39","W40")
all.averages <- cbind(site.list, all.averages)

site.list <- c("All1.csv","All2.csv","All3.csv","All4.csv","All5.csv","All6.csv","All7.csv","All8.csv","All9.csv","All10.csv",
               "All11.csv","All12.csv","All13.csv","All14.csv","All15.csv","All16.csv","All17.csv","All18.csv","All19.csv","All20.csv",
               "All21.csv","All22.csv","All23.csv","All24.csv","All25.csv","All26.csv","All27.csv","All28.csv","All29.csv","All30.csv",
               "All31.csv","All32.csv","All33.csv","All34.csv","All35.csv","All36.csv","All37.csv","All38.csv","All39.csv","All40.csv")

all.averages <- NULL
path <- "C:\\Users\\n0572527\\Desktop\\Data_40\\"

for (i in site.list) {
  Name <- (paste(path,i,sep =""))
  averages <- NULL
  for(j in 1:7) {  # 7 is the number of indices
    assign(paste("fileContents"), read.csv(Name))
    index.average <- mean(fileContents[,j], na.rm=TRUE)
    averages <- cbind(averages, index.average)
  }
  all.averages <- rbind(all.averages, averages)
}

a <- hclust(dist(all.Gympie.averages),"ward.D2")
png("hclust GympieNP 17500 40 3.png",width=1500,height=1000)
par(mar=c(6,6,2,2))
plot(a, main = "GympieNP k17500, k40 k3", cex.lab=2, cex.axis=2, 
     cex=1.5, cex.main=2)
dev.off()

a <- hclust(dist(all.Woondum.averages),"ward.D2")
png("hclust WoondumNP 17500 40 3.png",width=1500,height=1000)
par(mar=c(6,6,2,2))
plot(a, main = "WoondumNP k17500, k40 k3", cex.lab=2, cex.axis=2, 
     cex=1.5, cex.main=2)
dev.off()

a <- hclust(dist(all.averages),"ward.D2")
png("hclust both sites 17500 40 3.png",width=1500,height=1000)
par(mar=c(6,6,2,2))
plot(a, main = "Gympie and Woondum k17500, k40 k3", cex.lab=2, cex.axis=2, 
     cex=1.5, cex.main=2)
dev.off()

comparisonOfTwo <- rbind(all.Gympie.averages,all.Woondum.averages)
a <- hclust(dist(comparisonOfTwo),"ward.D2")
png("hclust Comparison of Gympie and Woondum 17500 40 3.png",width=2500,height=1000)
par(mar=c(6,6,2,2))
site.list <- c("G1","G2","G3","G4","G5","G6","G7","G8","G9","G10",
               "G11","G12","G13","G14","G15","G16","G17","G18","G19","G20",
               "G21","G22","G23","G24","G25","G26","G27","G28","G29","G30",
               "G31","G32","G33","G34","G35","G36","G37","G38","G39","G40",
               "W1","W2","W3","W4","W5","W6","W7","W8","W9","W10",
               "W11","W12","W13","W14","W15","W16","W17","W18","W19","W20",
               "W21","W22","W23","W24","W25","W26","W27","W28","W29","W30",
               "W31","W32","W33","W34","W35","W36","W37","W38","W39","W40")
plot(a, main = "Comparison of Gympie and Woondum k17500, k40 k3", cex.lab=2, cex.axis=2, 
     cex=1.5, cex.main=2, labels=site.list)
dev.off()


View(norm.dat)
col.name <- names(norm.dat[,1:7])

colnames(all.averages) <- col.name
cluster <- c(1:40)
all.aver <- cbind(cluster, all.averages)
write.csv(all.aver,"all.averages.csv",row.names=F)

