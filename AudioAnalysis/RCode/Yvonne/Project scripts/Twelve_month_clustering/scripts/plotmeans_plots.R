# Author:  Yvonne Phillips
# Date: 6 February 2017
# This code takes the 8 one-minute counts from three periods
# around civil-dawn and draws a plot with error bars of 
# 95% confidence intervals
rm(list = ls())

# This code also plots the occurance of bird calls onto 
# a sunrise plot.  Use Shift-Alt-J to find this

#Gym_birds <- read.csv("plotmeans_plots\\Gympie_birds_Civil_dawn_final.csv", header = T)
Gym_birds <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\plotmeans_plots\\Gympie_birds_Civil_dawn_final.csv", header = T)

#Woon_birds <- read.csv("plotmeans_plots\\Woondum_birds_civil_dawn_final.csv", header = T)
Woon_birds <- read.csv("D:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\plotmeans_plots\\Woondum_birds_Civil_dawn_final.csv", header = T)
sites <- c("GympieNP", "WoondumNP")
# colourblind pallet
cbbPalette <- c("#000000", "#E69F00", "#56B4E9", 
                "#009E73", "#F0E442", "#0072B2", 
                "#D55E00", "#CC79A7") # black, orange, lightblue, 
                # green, yellow, darkblue, burntorange and pink

library(gplots)
labels <- c("Jul15", "Aug", "Sep", "Oct", "Nov",
            "Dec", "Jan16", "Feb", "Mar", "Apr",
            "May", "Jun")


# Average with Confidence intervals (Student distribution) 
month_period <- as.character(substr(unique(Gym_birds$comb),4,11)) # 36 periods (12 months x periods A, B, C)
col_names <- colnames(Gym_birds)
statistics <-  data.frame(desc1=NA,
                          desc2=NA,
                          "Eastern.Yellow.Robin.song"=NA,
                          "Eastern.Yellow.Robin.piping"=NA,
                          "Eastern.Yellow.Robin.3"=NA, 
                          "White.throated.Honeyeater"=NA,  
                          "Eastern.Whipbird"=NA,
                          "White.throated.Treecreeper"=NA,
                          "White.throated.Treecreeper.2"=NA,
                          "White.throated.Treecreeper.3"=NA,
                          "Scarlet.Honeyeater"=NA,
                          "Scarlet.Honeyeater.2"=NA,      
                          "Scarlet.Honeyeater.3"=NA,    
                          "Lewin.s.Honeyeater"=NA,    
                          "Australian.Magpie"=NA,      
                          "Torresian.Crow"=NA,       
                          "Laughing.Kookaburra"=NA,
                          "Fantailed.Cuckoo"=NA,     
                          "Southern.Boobook"=NA,        
                          "Leaden.Flycatcher"=NA,        
                          "Rufous.Whistler"=NA,       
                          "Rufous.Whistler.2"=NA,         
                          "Rufous.Whistler.3"=NA,       
                          "Peaceful.Dove"=NA,       
                          "Grey.Shrike.Thrush"=NA,
                          "Sulfur.Crested.Cockatoo"=NA,
                          "White.throated.Nightjar"=NA,
                          "Spectacled.Monarch"=NA,
                          "Brown.Cuckoo.Dove"=NA,
                          "Australian.Brush.turkey"=NA,
                          "Pied.Currawong"=NA,
                          "Cicadas"=NA,
                          "Yellow.tailed.Black.Cockatoo"=NA,
                          "Channel.billed.Cuckoo"=NA,
                          "Spangled.Drongo"=NA,
                          "Spangled.Drongo.2"=NA,
                          "Australian.King.Parrot"=NA,
                          "Yellow.faced.Honeyeater"=NA,
                          "Variegated.Fairy.wren"=NA,
                          "Russet.tailed.Thrush"=NA,
                          "Brown.Thornbill"=NA,
                          "Olive.backed.Oriole"=NA,
                          "Golden.Whistler"=NA,
                          "Australian.Figbird"=NA,
                          "Australian.Figbird.2"=NA,
                          "Silvereye"=NA,
                          "Mistletoebird"=NA,
                          "Red.browed.Finch"=NA,
                          "White.headed.Pigeon"=NA,
                          "Eastern.Koel"=NA,
                          "Grey.Fantail"=NA,
                          "Grey.Fantail.2"=NA,
                          "Cicadabird"=NA,
                          "Large.billed.Scrubwren"=NA,
                          "Crested.Shrike.tit"=NA,
                          "Rufous.Fantail"=NA,
                          "Little.Lorikeet"=NA,
                          "Little.Shike.thrush"=NA,
                          "White.throated.Gerygone"=NA,
                          "Spotted.Pardalote"=NA,
                          "Rainbow.Lorikeet"=NA,
                          "Rose.Robin"=NA,
                          "Brown.Honeyeater"=NA,
                          "Bassian.Thrush"=NA,
                          "unknown1"=NA,
                          "unknown2"=NA,
                          "Shining.Bronze.Cuckoo"=NA,
                          "Brown.Gerygone"=NA)
statistics[1:(3*length(month_period)),] <- NA
statistics$desc1[1:(3*length(month_period))] <- rep(month_period, each = 3)
statistics$desc2[1:(3*length(month_period))] <- rep(c("av","sd","ci"), 3)
n <- 8
if(n==8) {
  t <- 2.36462425159278 # t value for student distribution and specific to n=8
}

for(i in 1:length(month_period)) {
  a <- grep(month_period[i], Gym_birds$comb)
  Gym_birds_temp <- Gym_birds[a,]
  for(j in 4:69) {
  av <- mean(Gym_birds_temp[,j])
  sd <- sd(Gym_birds_temp[,j])
  ci <- t*(sd/sqrt(n))
  statistics[(3*(i-1)+1),(j-1)] <- av
  statistics[(3*(i-1)+2),(j-1)] <- sd
  statistics[(3*(i-1)+3),(j-1)] <- ci
  }
}
rm(a, av, sd, ci)
label_names <- c("Eastern Yellow Robin",
                 "White-throated Honeyeater",
                 "Laughing Kookaburra",
                 "Scarlet Honeyeater",
                 "White-throated Treecreeper",
                 "Eastern Whipbird")

label_name <- label_names[1]
if(label_name=="Eastern Yellow Robin") {
  ylim <- 20
  x <- 1:12
  column <- 3
}
label_name <- label_names[2]
if(label_name=="White-throated Honeyeater") {
  ylim <- 12
  x <- 1:12
  column <- 6
}

label_name <- label_names[3]
if(label_name=="Laughing Kookaburra") {
  ylim <- 2.5
  x <- 1:12
  column <- 17
}

label_name <- label_names[4]
if(label_name=="Scarlet Honeyeater") {
  label_name <- "Scarlet Honeyeater SC1"
  ylim <- 12.2
  x <- 1:12
  column <- 11
}

label_name <- label_names[4]
if(label_name=="Scarlet Honeyeater") {
  label_name <- "Scarlet Honeyeater SC2"
  ylim <- 12.2
  x <- 1:12
  column <- 12
}

label_name <- label_names[5]
if(label_name=="White-throated Treecreeper") {
  ylim <- 9
  x <- 1:12
  column <- 9
}

label_name <- label_names[6]
if(label_name=="Eastern Whipbird") {
  ylim <- 6
  x <- 1:12
  column <- 7
}

legend <- c("Period A","Period B","Period C")
tiff(paste("test",label_name,".tiff",sep=""),
     height = 550, width = 810)
par(cex=1.6, mar=c(3,3.4,2,1), mgp=c(2,0.8,0), cex=1.6)
# period A
a <- grep("pre", statistics$desc1)
statistics_temp <- statistics[a,]
b <- grep("av", statistics_temp$desc2)
Period_av <- statistics_temp[b,]
b <- grep("ci", statistics_temp$desc2)
Period_ci <- statistics_temp[b,]
Period_ci.up <- Period_av[,3:68] + Period_ci[,3:68]
Period_ci.down <- Period_av[,3:68] - Period_ci[,3:68]

plot(Period_av[,column] ~ x, cex=1.5, xaxt='n',
     xlab='',ylab='', 
     main=label_name, ylim = c(0, ylim),
     pch=15, las=1, col = "#D55E00") #col='red'
axis(1, at=x, labels=labels)
mtext(side = 2, line = 2.2, cex = 1.8,
      "Average number of calls per min ± 95% C.I.")
mtext(side = 1, line = 2.2, cex = 1.8,
      "Months")
arrows(x, Period_ci.down[,(column-2)], x, Period_ci.up[,(column-2)], 
       code=3, length=0.1, angle=90, col="#D55E00") #'red')
for(i in 1:11) {
  segments(x0 = x[i], y0 = Period_av[i,column], 
           x1 = x[i+1], y1 = Period_av[(i+1),column], 
           col = "#D55E00", lwd = 2) #"red")
}
rm(Period_ci, Period_ci.up, Period_ci.down)
# Period B
par(new=T)
a <- grep("civ", statistics$desc1)
statistics_temp <- statistics[a,]
b <- grep("av", statistics_temp$desc2)
Period_av <- statistics_temp[b,]
b <- grep("ci", statistics_temp$desc2)
Period_ci <- statistics_temp[b,]
Period_ci.up <- Period_av[,3:68] + Period_ci[,3:68]
Period_ci.down <- Period_av[,3:68] - Period_ci[,3:68]
plot(Period_av[,column] ~ x, cex=1.5, xaxt='n',
     xlab='',ylab='', 
     main=label_name, ylim = c(0, ylim),
     pch=16, las=1, col = "#0072B2") #col='blue'
axis(1, at=x, labels=labels)
arrows(x, Period_ci.down[,(column-2)], x, Period_ci.up[,(column-2)], code=3, 
       length=0.1, angle=90, col= "#0072B2")  #'blue')
for(i in 1:11) {
  segments(x0 = x[i], y0 = Period_av[i,column], 
           x1 = x[i+1], y1 = Period_av[(i+1),column], 
           col="#0072B2", lwd = 2) #col = "blue")
}
rm(Period_ci, Period_ci.up, Period_ci.down)
# Period C
par(new=T)
a <- NULL
a <- grep("post", statistics$desc1)
statistics_temp <- statistics[a,]
a <- NULL
a <- grep("av", statistics_temp$desc2)
Period_av <- statistics_temp[a,]
a <- NULL
a <- grep("ci", statistics_temp$desc2)
Period_ci <- statistics_temp[a,]
Period_ci.up <- Period_av[,3:68] + Period_ci[,3:68]
Period_ci.down <- Period_av[,3:68] - Period_ci[,3:68]
plot(Period_av[,column] ~ x, cex=1.5, xaxt='n',
     xlab='',ylab='', 
     main=label_name, ylim = c(0, ylim),
     pch=17, las=1, col = "#009E73") #col="green"
axis(1, at=x, labels=labels)
arrows(x, Period_ci.down[,(column-2)], x, Period_ci.up[,(column-2)], code=3, 
       length=0.1, angle=90, col="#009E73")
for(i in 1:11) {
  segments(x0 = x[i], y0 = Period_av[i,column], 
           x1 = x[i+1], y1 = Period_av[(i+1),column], 
           col = "#009E73", lwd = 2)
}
legend(x = 8, y = 1.05*ylim, col = c("#D55E00","#0072B2","#009E73"), 
       legend = c(legend[1], legend[2], legend[3]), 
       lwd = 2, cex = 1.55, bty = "n", pch=c(15,16,17))
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
plot_civil_dawn_single <- function(site, bird_name, ylim, 
                            labels, column, list, 
                            adjust, number) {
    n <- 0
    if(site=="GympieNP") {
      plotmeans(Gym_birds[(n*96+1):(96*(n+1)), column] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
                data = Gym_birds, connect = T, n.label = F, minbar = 0, 
                ylim = ylim, xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
                las=1, lwd = 2.4, ci.label = T, mean.labels = T,
                use.t = F)
      abline(v=6.5, lty=2)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      mtext(side = 2, line = 1, cex = 2.6,
            "Number of calls per minute +/- 95% C.I.")
    }
    if(site=="WoondumNP") {
      plotmeans(Woon_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Woon_birds$comb[(n*96+1):(96*(n+1))], data = Woon_birds, connect = T,
                mgp = c(3, 0.3, 0), ylab = "", xlab = "", n.label = F, 
                minbar = 0, ylim = ylim, xaxt = "n", las=1, lwd = 2.4, 
                ci.label = T, mean.labels = T,
                use.t = F)  
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      abline(v=6.5, lty=2, lwd = 1)
      mtext(side = 2, line = 1, cex = 2.6,
            "Number of calls +/- 95% C.I.")
    }
    text(x = 10, y = (ylim[2]-adjust), "Period A (pre-civil dawn)", 
         cex = 1.1)
    n <- 1
    if(site=="GympieNP") {
      plotmeans(Gym_birds[(n*96+1):(96*(n+1)), column] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
                data = Gym_birds, connect = T, minbar = 0,
                ylim = ylim, xaxt = "n", yaxt = "n", 
                ylab = "", xlab = "", ci.label = T, mean.labels = T,
                mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4,
                use.t = F)
      abline(v=6.5, lty=2, lwd = 1)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      text(x = 10, y = (ylim[2]-adjust), "Period B (civil dawn)", 
           cex = 1.1)
      mtext(side = 3, paste(bird_name, site, sep = " - "), 
            cex = 2.6, line = -1.6)
    }
    if(site=="WoondumNP") {
      plotmeans(Woon_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Woon_birds$comb[(n*96+1):(96*(n+1))], 
                data = Woon_birds, connect = T, minbar = 0,
                ylim = ylim, xaxt = "n", yaxt = "n",
                ylab = "", xlab = "", ci.label = T, mean.labels = T,
                mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4,
                use.t = F)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      abline(v=6.5, lty=2, lwd = 1)
      text(x = 10, y = (ylim[2]-adjust), "Period B (civil dawn)", cex = 1.1)
      mtext(side = 3, paste(bird_name, site, sep = " - "), 
            cex = 2.6, line = -1.5)
    }
    n <- 2
    if(site=="GympieNP") {
      plotmeans(Gym_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Gym_birds$comb[(n*96+1):(96*(n+1))], 
                data = Gym_birds, connect = T, minbar = 0,
                ylim = ylim, xaxt = "n", yaxt = "n",
                ylab = "", xlab = "", ci.label = T, mean.labels = T,
                mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4,
                use.t = F) 
      abline(v=6.5, lty=2, lwd = 1)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
    }
    if(site=="WoondumNP") {
      plotmeans(Woon_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Woon_birds$comb[(n*96+1):(96*(n+1))], 
                data = Woon_birds, connect = T, minbar = 0,
                ylim = ylim, xaxt = "n", yaxt = "n",
                ylab = "", xlab = "", ci.label = T, mean.labels = T,
                mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4,
                use.t = F)  
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
    }
    text(x = 10, y = (ylim[2]-adjust), "Period C (Post-civil dawn)", cex = 1.2)
    abline(v=6.5, lty=2, lwd = 1)
    mtext(side = 4, paste(number,ylim_max,sep = " , "), 
          cex = 2, line = 0)
}

plot_civil_dawn_double <- function(site, bird_name, ylim, 
                                   labels, list, 
                                   adjust, number, legend) {
  n <- 0
  if(site=="GympieNP") {
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, 
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              n.label = F, minbar = 0, ylim = ylim, 
              xaxt = "n", mgp = c(3, 0.3, 0),
              las=1, lwd = 2.4,
              use.t = F)
    abline(v=6.5, lty=2)
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    mtext(side = 2, line = 1, cex = 2.6,
          "Number of calls per minute +/- 95% C.I.")
    par(new=T)
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, 
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              n.label = F, minbar = 0, ylim = ylim, 
              xaxt = "n", mgp = c(3, 0.3, 0), lty = 2,
              las=1, lwd = 2.4,
              use.t = F)
  }
  if(site=="WoondumNP") {
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T, ci.label = T, mean.labels = T,
              mgp = c(3, 0.3, 0), ylab = "", xlab = "",
              n.label = F, minbar = 0, ylim = ylim, 
              xaxt = "n", las=1, lwd = 2.4,
              use.t = F)  
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    abline(v=6.5, lty=2, lwd = 1)
    mtext(side = 2, line = 1, cex = 2.6,
          "Number of calls +/- 95% C.I.")
    par(new=T)
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T,
              mgp = c(3, 0.3, 0), lty = 2,
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              n.label = F, minbar = 0, ylim = ylim, 
              xaxt = "n", las=1, lwd = 2.4,
              use.t = F)
  }
  text(x = 10, y = (ylim[2]-adjust), "Period A (pre-civil dawn)", 
       cex = 1.1)
  n <- 1
  if(site=="GympieNP") {
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, minbar = 0,
              ylim = ylim, xaxt = "n", yaxt = "n",
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4,
              use.t = F)
    abline(v=6.5, lty=2, lwd = 1)
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    text(x = 10, y = (ylim[2]-adjust), "Period B (civil dawn)", 
         cex = 1.1)
    mtext(side = 3, paste(bird_name, site, sep = " - "), 
          cex = 2.6, line = -1.6)
    par(new=T)
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, minbar = 0,
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              ylim = ylim, xaxt = "n", yaxt = "n", lty = 2,
              mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4,
              use.t = F)
  }
  if(site=="WoondumNP") {
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T, minbar = 0,
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              ylim = ylim, xaxt = "n", yaxt = "n",
              mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4,
              use.t = F)
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    abline(v=6.5, lty=2, lwd = 1)
    text(x = 10, y = (ylim[2]-adjust), "Period B (civil dawn)", cex = 1.1)
    mtext(side = 3, paste(bird_name, site, sep = " - "), 
          cex = 2.6, line = -1.5)
    par(new=T)
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T, minbar = 0,
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              ylim = ylim, xaxt = "n", yaxt = "n", lty = 2,
              mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4,
              use.t = F)
    
  }
  n <- 2
  if(site=="GympieNP") {
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, minbar = 0,
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              ylim = ylim, xaxt = "n", yaxt = "n",
              mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4,
              use.t = F) 
    abline(v=6.5, lty=2, lwd = 1)
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    par(new=T)
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, minbar = 0,
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              ylim = ylim, xaxt = "n", yaxt = "n", lty = 2,
              mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4,
              use.t = F) 
    legend(x = 7.2, y = (ylim[2]-1.3*adjust), lty = c(1,2), 
           legend = c(legend[1], legend[2]), 
           lwd = 2.4, cex = 1.2)
  }
  if(site=="WoondumNP") {
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T, minbar = 0,
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              ylim = ylim, xaxt = "n", yaxt = "n",
              mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4,
              use.t = F)  
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    par(new=T)
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T, minbar = 0,
              ylab = "", xlab = "", ci.label = T, mean.labels = T,
              ylim = ylim, xaxt = "n", yaxt = "n", lty = 2,
              mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4,
              use.t = F)
  }
  text(x = 10, y = (ylim[2]-adjust), "Period C (Post-civil dawn)", cex = 1.2)
  abline(v=6.5, lty=2, lwd = 1)
  mtext(side = 4, paste(number,ylim_max,sep = " , "), cex = 2, line = 0)
}

list <- colnames(Gym_birds)  

#[1]  "period_listing"               "month"                       
#[3]  "comb"                         "Eastern.Yellow.Robin.song" (4,20)
#[5]  "Eastern.Yellow.Robin.piping"(5,10)  "Eastern.Yellow.Robin.3"      
#[7]  "White.throated.Honeyeater" (7,11)    "Eastern.Whipbird" (8,6)            
#[9]  "White.throated.Treecreeper"   "White.throated.Treecreeper.2" (10,10)
#[11] "White.throated.Treecreeper.3" "Scarlet.Honeyeater" 12,20
#[13] "Scarlet.Honeyeater.2" 13, 13        "Scarlet.Honeyeater.3"        
#[15] "Lewin.s.Honeyeater" 15,5           "Australian.Magpie"           
#[17] "Torresian.Crow"               "Laughing.Kookaburra"  (18,3)       
#[19] "Fantailed.Cuckoo"             "Southern.Boobook"            
#[21] "Leaden.Flycatcher"            "Rufous.Whistler"             
#[23] "Rufous.Whistler.2"            "Rufous.Whistler.3"           
#[25] "Peaceful.Dove"                "Grey.Shrike.Thrush"          
#[27] "Sulfur.Crested.Cockatoo"      "White.throated.Nightjar"     
#[29] "Spectacled.Monarch"           "Brown.Cuckoo.Dove" (30,20)
#[31] "Australian.Brush.turkey"      "Pied.Currawong"              
#[33] "Cicadas" (33, 14)                "Yellow.tailed.Black.Cockatoo"
#[35] "Channel.billed.Cuckoo"        "Spangled.Drongo"             
#[37] "Spangled.Drongo.2"            "Australian.King.Parrot"      
#[39] "Yellow.faced.Honeyeater"      "Variegated.Fairy.wren"       
#[41] "Russet.tailed.Thrush"         "Brown.Thornbill"             
#[43] "Olive.backed.Oriole"          "Golden.Whistler"             
#[45] "Australian.Figbird"           "Australian.Figbird.2"        
#[47] "Silvereye"                    "Mistletoebird"               
#[49] "Red.browed.Finch"             "White.headed.Pigeon"         
#[51] "Eastern.Koel"                 "Grey.Fantail"                
#[53] "Grey.Fantail.2"               "Cicadabird"                  
#[55] "Large.billed.Scrubwren"       "Crested.Shrike.tit"          
#[57] "Rufous.Fantail"               "Little.Lorikeet"             
#[59] "Little.Shike.thrush"          "White.throated.Gerygone"     
#[61] "Spotted.Pardalote"            "Rainbow.Lorikeet"            
#[63] "Rose.Robin"                   "Brown.Honeyeater"            
#[65] "Bassian.Thrush"               "unknown1"                    
#[67] "unknown2"                     "Shining.Bronze.Cuckoo"       
#[69] "Brown.Gerygone" 


# number is the species number (see above)
number <- c(12,13)
ylim_max <- 20
legend <- c("Whistled Notes", "Chatter")
for(i in number[1]) {    #4:length(list)) {
  column <- i
  bird_name <- list[i]
  ylim <- c(0, ylim_max)
  tiff(paste("D:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\plotmeans_plots\\test",
          bird_name," ",sites[1], "_", sites[2], ".tiff", sep = ""), 
       width=3400, height=1800)
  
  #tiff(paste("plotmeans_plots\\test",bird_name," ",sites[1],
  #           "_", sites[2], ".tiff", sep = ""), 
  #     width=3400, height=1800)
  
  par(mfrow=c(2,3), mar=c(2,0,0,0), oma = c(1,2,1,1),
      cex = 1, cex.lab = 2.6)
  for(j in 1:length(sites)) {
      site <- sites[j]
      if(site=="GympieNP") {
        list <- colnames(Gym_birds)  
      }
      if(site=="WoondumNP") {
        list <- colnames(Woon_birds)  
      }
      adjust <- 0.5
      if(ylim[2] >= 5) {
        adjust <- 0.5
      }
      if(ylim[2] >= 8) {
        adjust <- 1
      }
      if(ylim[2] >= 11) {
        adjust <- 1.5
      }
      if(ylim[2] >= 15) {
        adjust <- 2.5
      }
      if(length(number) == 1) {
        plot_civil_dawn_single(site, bird_name, ylim, labels, 
                        column, list, adjust, number)  
      }
      if(length(number) > 1) {
        plot_civil_dawn_double(site, bird_name, ylim, labels, 
                              list, adjust, number, legend)  
      }
  }
  dev.off()
}

# Fix the month labels in Gym_birds and Woon_birds
Gym_birds$month <- substr(Gym_birds$month, 3,6)
Woon_birds$month <- substr(Woon_birds$month, 3,6)
Gym_birds$month <- factor(Gym_birds$month, levels=levels(Gym_birds$month) <- unique(Gym_birds$month)) 
Woon_birds$month <- factor(Woon_birds$month, levels=levels(Woon_birds$month) <- unique(Woon_birds$month)) 

# boxplots - Eastern Whipbird
par(mfcol=c(2,2), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=1.2, cex.axis=1)
ylim <- c(0,10)
ylab <- "Calls per minute"
b <- boxplot(Gym_birds$Eastern.Whipbird[(96*1+1):(96*2)]~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim, ylab = ylab)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Eastern Whipbird")
boxplot(Woon_birds$Eastern.Whipbird[(96*1+1):(96*2)]~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim, ylab = ylab)
mtext(side = 3, line = -1, "Eastern Whipbird")
mtext(side = 3, line = -1, adj = 1,"Period B")
boxplot(Gym_birds$Eastern.Whipbird[(96*2+1):(96*3)]~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim, ylab = ylab)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Eastern Whipbird")
boxplot(Woon_birds$Eastern.Whipbird[(96*2+1):(96*3)]~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim, ylab = ylab)
mtext(side = 3, line = -1, "Eastern Whipbird")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - Eastern Yellow Robin chop-chop-chop call
par(mfcol=c(2,2), mar=c(2,2,1,1))
ylim <- c(0,28)
boxplot(Gym_birds$Eastern.Yellow.Robin.song[(96*0+1):(96*1)]
        ~Gym_birds$month[(96*0+1):(96*1)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period A")
mtext(side = 3, line = -1, "Eastern Yellow Robin - chop-chop-chop call")
boxplot(Woon_birds$Eastern.Yellow.Robin.song[(96*0+1):(96*1)]
        ~Woon_birds$month[(96*0+1):(96*1)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Eastern Yellow Robin - chop-chop-chop call")
mtext(side = 3, line = -1, adj = 1,"Period A")

boxplot(Gym_birds$Eastern.Yellow.Robin.song[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Eastern Yellow Robin - chop-chop-chop call")
boxplot(Woon_birds$Eastern.Yellow.Robin.song[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Eastern Yellow Robin - chop-chop-chop call")
mtext(side = 3, line = -1, adj = 1,"Period B")

# boxplots - Eastern Yellow Robin piping call
par(mfcol=c(2,2), mar=c(2,2,1,1))
ylim <- c(0,17)
boxplot(Gym_birds$Eastern.Yellow.Robin.piping[(96*0+1):(96*1)]
        ~Gym_birds$month[(96*0+1):(96*1)], 
        main="Gympie", subtitle="Eastern Whipbird", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period A")
mtext(side = 3, line = -1, "Eastern Yellow Robin - piping")
boxplot(Woon_birds$Eastern.Yellow.Robin.piping[(96*0+1):(96*1)]
        ~Woon_birds$month[(96*0+1):(96*1)], 
        main="Woondum", subtitle="Eastern Whipbird", ylim=ylim)
mtext(side = 3, line = -1, "Eastern Yellow Robin - piping")
mtext(side = 3, line = -1, adj = 1,"Period A")

boxplot(Gym_birds$Eastern.Yellow.Robin.piping[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", subtitle="Eastern Whipbird", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Eastern Yellow Robin - piping")
boxplot(Woon_birds$Eastern.Yellow.Robin.piping[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", subtitle="Eastern Whipbird", ylim=ylim)
mtext(side = 3, line = -1, "Eastern Yellow Robin - piping")
mtext(side = 3, line = -1, adj = 1,"Period B")

# boxplots - Brown Cuckoo-dove
par(mfcol=c(2,2), mar=c(2,2,1,1))
ylim <- c(0,28)
boxplot(Gym_birds$Brown.Cuckoo.Dove[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Brown Cuckoo-dove")
boxplot(Woon_birds$Brown.Cuckoo.Dove[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Brown Cuckoo-dove")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$Brown.Cuckoo.Dove[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Brown Cuckoo-dove")
boxplot(Woon_birds$Brown.Cuckoo.Dove[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Brown Cuckoo-dove")
mtext(side = 3, line = -1, adj = 1,"Period C")

# Eastern Yellow Robin
boxplot(Gym_birds$Eastern.Yellow.Robin.piping[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Eastern Yellow Robin - piping")
boxplot(Woon_birds$Eastern.Yellow.Robin.piping[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Eastern Yellow Robin - piping")
mtext(side = 3, line = -1, adj = 1,"Period B")

# boxplots - White-throated Treecreeper 1
par(mfcol=c(2,2), mar=c(2,2,1,1))
ylim <- c(0,5.5)
boxplot(Gym_birds$White.throated.Treecreeper[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "White-throated Treecreeper 1")
boxplot(Woon_birds$White.throated.Treecreeper[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Treecreeper 1")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$White.throated.Treecreeper[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "White-throated Treecreeper 1")
boxplot(Woon_birds$White.throated.Treecreeper[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Treecreeper 1")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - White-throated Treecreeper 2
par(mfcol=c(2,2), mar=c(2,2,1,1))
ylim <- c(0,12)
boxplot(Gym_birds$White.throated.Treecreeper.2[(96*1):(96*2)]~Gym_birds$month[(96*1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "White-throated Treecreeper 2")
boxplot(Woon_birds$White.throated.Treecreeper.2[(96*1):(96*2)]~Woon_birds$month[(96*1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Treecreeper 2")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$White.throated.Treecreeper.2[(96*2):(96*3)]~Gym_birds$month[(96*2):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "White-throated Treecreeper 2")
boxplot(Woon_birds$White.throated.Treecreeper.2[(96*2):(96*3)]~Woon_birds$month[(96*2):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Treecreeper 2")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - White-throated Treecreeper 3
par(mfcol=c(2,2), mar=c(2,2,1,1))
ylim <- c(0,8)
boxplot(Gym_birds$White.throated.Treecreeper.3[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "White-throated Treecreeper 3")
boxplot(Woon_birds$White.throated.Treecreeper.3[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Treecreeper 3")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$White.throated.Treecreeper.3[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "White-throated Treecreeper 3")
boxplot(Woon_birds$White.throated.Treecreeper.3[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Treecreeper 3")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - Scarlet Honeyeater
 par(mfcol=c(2,2), mar=c(2,2,1,1))
ylim <- c(0,11)
boxplot(Gym_birds$Scarlet.Honeyeater[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Scarlet Honeyeater")
boxplot(Woon_birds$Scarlet.Honeyeater[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Scarlet Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$Scarlet.Honeyeater[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Scarlet Honeyeater")
boxplot(Woon_birds$Scarlet.Honeyeater[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Scarlet Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - Scarlet Honeyeater2
par(mfcol=c(2,3), mar=c(2,2,1,1))
ylim <- c(0,13)
boxplot(Gym_birds$Scarlet.Honeyeater.2[(96*0+1):(96*1)]
        ~Gym_birds$month[(96*0+1):(96*1)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period A")
mtext(side = 3, line = -1, "Scarlet Honeyeater2")
boxplot(Woon_birds$Scarlet.Honeyeater.2[(96*0+1):(96*1)]
        ~Woon_birds$month[(96*0+1):(96*1)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Scarlet Honeyeater2")
mtext(side = 3, line = -1, adj = 1,"Period A")

boxplot(Gym_birds$Scarlet.Honeyeater.2[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Scarlet Honeyeater2")
boxplot(Woon_birds$Scarlet.Honeyeater.2[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Scarlet Honeyeater2")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$Scarlet.Honeyeater.2[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Scarlet Honeyeater")
boxplot(Woon_birds$Scarlet.Honeyeater.2[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Scarlet Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - Scarlet Honeyeater
par(mfcol=c(2,2), mar=c(2,2,1,1))
ylim <- c(0,11)
boxplot(Gym_birds$Scarlet.Honeyeater[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Scarlet Honeyeater")
boxplot(Woon_birds$Scarlet.Honeyeater[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Scarlet Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$Scarlet.Honeyeater[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Scarlet Honeyeater")
boxplot(Woon_birds$Scarlet.Honeyeater[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Scarlet Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - Lewin's Honeyeater
par(mfcol=c(2,3), mar=c(2,2,1,1))
ylim <- c(0,7)
boxplot(Gym_birds$Lewin.s.Honeyeater[(96*0+1):(96*1)]
        ~Gym_birds$month[(96*0+1):(96*1)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period A")
mtext(side = 3, line = -1, "Lewin's Honeyeater")
boxplot(Woon_birds$Lewin.s.Honeyeater[(96*0+1):(96*1)]
        ~Woon_birds$month[(96*0+1):(96*1)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Lewin's Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period A")

boxplot(Gym_birds$Lewin.s.Honeyeater[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Lewin's Honeyeater")
boxplot(Woon_birds$Lewin.s.Honeyeater[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Lewin's Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$Lewin.s.Honeyeater[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Lewin's Honeyeater")
boxplot(Woon_birds$Lewin.s.Honeyeater[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Lewin's Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - White-throated Honeyeater
par(mfcol=c(2,3), mar=c(2,2,1,1))
ylim <- c(0,12)
boxplot(Gym_birds$White.throated.Honeyeater[(96*0+1):(96*1)]
        ~Gym_birds$month[(96*0+1):(96*1)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period A")
mtext(side = 3, line = -1, "White-throated Honeyeater")
boxplot(Woon_birds$White.throated.Honeyeater[(96*0+1):(96*1)]
        ~Woon_birds$month[(96*0+1):(96*1)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period A")

boxplot(Gym_birds$White.throated.Honeyeater[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "White-throated Honeyeater")
boxplot(Woon_birds$White.throated.Honeyeater[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$White.throated.Honeyeater[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "White-throated Honeyeater")
boxplot(Woon_birds$White.throated.Honeyeater[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "White-throated Honeyeater")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - Laughing Kookaburra
par(mfcol=c(2,3), mar=c(2,2,1,1))
ylim <- c(0,13.5)
boxplot(Gym_birds$Laughing.Kookaburra[(96*0+1):(96*1)]
        ~Gym_birds$month[(96*0+1):(96*1)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period A")
mtext(side = 3, line = -1, "Laughing Kookaburra")
  boxplot(Woon_birds$Laughing.Kookaburra[(96*0+1):(96*1)]
        ~Woon_birds$month[(96*0+1):(96*1)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Laughing Kookaburra")
mtext(side = 3, line = -1, adj = 1,"Period A")

boxplot(Gym_birds$Laughing.Kookaburra[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Laughing Kookaburra")
boxplot(Woon_birds$Laughing.Kookaburra[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Laughing Kookaburra")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$Laughing.Kookaburra[(96*2+1):(96*3)]~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim = ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Laughing Kookaburra")
boxplot(Woon_birds$Laughing.Kookaburra[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Laughing Kookaburra")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - Cicadas
par(mfcol=c(2,3), mar=c(2,2,1,1))
ylim <- c(0,13.5)
boxplot(Gym_birds$Cicadas[(96*0+1):(96*1)]
        ~Gym_birds$month[(96*0+1):(96*1)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period A")
mtext(side = 3, line = -1, "Cicadas")
boxplot(Woon_birds$Cicadas[(96*0+1):(96*1)]
        ~Woon_birds$month[(96*0+1):(96*1)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Cicadas")
mtext(side = 3, line = -1, adj = 1,"Period A")

boxplot(Gym_birds$Cicadas[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Cicadas")
boxplot(Woon_birds$Cicadas[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Cicadas")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$Cicadas[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim = ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Cicadas")
boxplot(Woon_birds$Cicadas[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Cicadas")
mtext(side = 3, line = -1, adj = 1,"Period C")

# boxplots - Pied Currawong
par(mfcol=c(2,3), mar=c(2,2,1,1))
ylim <- c(0,12)
boxplot(Gym_birds$Pied.Currawong[(96*0+1):(96*1)]
        ~Gym_birds$month[(96*0+1):(96*1)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period A")
mtext(side = 3, line = -1, "Pied Currawong")
boxplot(Woon_birds$Pied.Currawong[(96*0+1):(96*1)]
        ~Woon_birds$month[(96*0+1):(96*1)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Pied Currawong")
mtext(side = 3, line = -1, adj = 1,"Period A")

boxplot(Gym_birds$Pied.Currawong[(96*1+1):(96*2)]
        ~Gym_birds$month[(96*1+1):(96*2)], 
        main="Gympie", ylim=ylim)
mtext(side = 3, line = -1, adj = 1,"Period B")
mtext(side = 3, line = -1, "Pied Currawong")
boxplot(Woon_birds$Pied.Currawong[(96*1+1):(96*2)]
        ~Woon_birds$month[(96*1+1):(96*2)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Pied Currawong")
mtext(side = 3, line = -1, adj = 1,"Period B")

boxplot(Gym_birds$Pied.Currawong[(96*2+1):(96*3)]
        ~Gym_birds$month[(96*2+1):(96*3)], 
        main="Gympie", ylim = ylim)
mtext(side = 3, line = -1, adj = 1,"Period C")
mtext(side = 3, line = -1, "Pied Currawong")
boxplot(Woon_birds$Pied.Currawong[(96*2+1):(96*3)]
        ~Woon_birds$month[(96*2+1):(96*3)], 
        main="Woondum", ylim=ylim)
mtext(side = 3, line = -1, "Pied Currawong")
mtext(side = 3, line = -1, adj = 1,"Period C")

# Weather plot ----------------------------
rain <- read.csv("Weather_2015_2016.csv", header = T)
rain <- rain[3:nrow(rain),]
b <- barplot(as.numeric(levels(rain$Rain))[rain$Rain],
             ylim = c(0,80), ylab = "Rainfall (mm)",
             mgp=c(0.7,-0.3,-1.5))
a <- b[which(substr(rain$Date,1,2)=="1-")]
abline(v=a, lty=2)
average <- NULL
for(i in 1:(length(a)-1)) {
  av <- mean(c(a[i],a[i+1]))
  average <- c(average, av)
}
average <- c(average, (max(average)+30))
months <- c("Jun", "Jul", "Aug", "Sep", "Oct", "Nov",
            "Dec", "Jan", "Feb", "Mar", "Apr",
            "May", "Jun","Jul")
labels1 <- c(rep("2015",7), rep("2016",7))
axis(side=3,line= 0,at=average, labels=months, tick = F)
axis(side=3,line= -1,at=average, labels=labels1, tick = F)

# sample dates
gym_dates <- c()
d <- which(substr(rain$Date,1,9)=="21-Nov-15")
d

#set_the_periods <- function() {
list2 <- c("perA_July", "perA_Aug",
           "perA_Sept", "perA_Oct",
           "perA_Nov", "perA_Dec",
           "perA_Jan", "perA_Feb",
           "perA_Mar", "perA_Apr",
           "perA_May", "perA_Jun")
for(i in 1:length(list2)) {
  assign(list2[i], (1:8 + (i-1)*8))
}
rm(list2)

list2 <- c("perB_July", "perB_Aug",
           "perB_Sept", "perB_Oct",
           "perB_Nov", "perB_Dec",
           "perB_Jan", "perB_Feb",
           "perB_Mar", "perB_Apr",
           "perB_May", "perB_Jun")
for(i in 1:length(list2)) {
  assign(list2[i], (97:104 + (i-1)*8))
}
rm(list2)
list2 <- c("perC_July", "perC_Aug",
           "perC_Sept", "perC_Oct",
           "perC_Nov", "perC_Dec",
           "perC_Jan", "perC_Feb",
           "perC_Mar", "perC_Apr",
           "perC_May", "perC_Jun")
for(i in 1:length(list2)) {
  assign(list2[i], (193:200 + (i-1)*8))
}
#}

# Eastern Yellow Robin - column 4 -----------------------
# Compare Period C from Gympie to Period C from Woondum 
# for twelve months
column <- 4 # Eastern Whipbird
list  <- c("perA_July", "perA_Aug", "perA_Sept", "perA_Oct",
           "perA_Nov", "perA_Dec", "perA_Jan", "perA_Feb",
           "perA_Mar", "perA_Apr", "perA_May", "perA_Jun")

rows  <- c(perA_July, perA_Aug,
           perA_Sept, perA_Oct,
           perA_Nov, perA_Dec,
           perA_Jan, perA_Feb,
           perA_Mar, perA_Apr,
           perA_May, perA_Jun) # Months July-June for Periods C
kw_test <- NULL
for(i in 1:length(list)) {
  Data <- NULL
  rows <- get(list[i]) 
  obs <- rep(1:8, 1)
  counts <- Gym_birds[rows, column]
  counts <- c(counts, Woon_birds[rows, column])
  group <-  Gym_birds$comb[rows]
  group <- c(group, Woon_birds$comb[rows])
  site <- rep("Gym", length(obs))
  site <- c(site, rep("Woon", length(obs)))
  Data <- cbind(obs, counts, group, site)
  Data <- data.frame(Data)
  t <- kruskal.test(counts ~ site, data = Data)
  kw_test <- c(kw_test, t)
} 


# Testing the difference between Period A and Period B
column <- 4
rows <- c(perA_July, perA_Aug,
          perA_Sept, perA_Oct,
          perB_July, perB_Aug,
          perB_Sept, perB_Oct) # Months July-October for Periods A and B
rows1 <- c(perA_July, perA_Aug,
           perA_Sept, perA_Oct)
rows2 <- c(perB_July, perB_Aug,
            perB_Sept, perB_Oct)
obs <- rep(1:8, 4)
counts <- Gym_birds[rows1, column]
counts <- c(counts, Gym_birds[rows2, column])
group <-  Gym_birds$comb[rows]
Data <- cbind(obs, counts, group)

kruskal.test(counts ~ group, data = Data)

# Eastern Yellow Robin song
kruskal.test(Gym_birds[rows,column], Gym_birds$comb[rows])
kruskal.test(Woon_birds[rows,column], Woon_birds$comb[rows])

# Compare Period A from Gympie to Period A from Woondum 
# from July to October
column <- 4 # Eastern Yellow Robin - chop chop call
rows <- c(perA_July, perA_Aug,
          perA_Sept, perA_Oct) # Months July-October for Period A
obs <- rep(1:8, 4)
counts <- Gym_birds[rows, column]
counts <- c(counts, Woon_birds[rows2, column])
group <-  Gym_birds$comb[rows]
Data <- cbind(obs, counts, group)
kruskal.test(counts ~ group, data = Data)

# Compare Period B from Gympie to Period B from Woondum 
# from July to November
column <- 4 # Eastern Yellow Robin - chop chop call
rows <- c(perB_July, perB_Aug,
          perB_Sept, perB_Oct,
          perB_Nov) #Months July-November for Period B
obs <- rep(1:8, 5)
counts <- Gym_birds[rows, column]
counts <- c(counts, Woon_birds[rows, column])
group <-  Gym_birds$comb[rows]
group <- c(group, Woon_birds$comb[rows])
Data <- cbind(obs, counts, group)
kruskal.test(counts ~ group, data = Data)
# or
kruskal.test(rbind(Gym_birds[rows, column], Woon_birds[rows, column]), 
             rbind(Gym_birds$comb[rows], Woon_birds$comb[rows]))

rows <- c(1:32, 97:128)
# Eastern Yellow Robin song
kruskal.test(Gym_birds[rows,column], Gym_birds$comb[rows])
kruskal.test(Woon_birds[rows,column], Woon_birds$comb[rows])

# Eastern Whipbird: Column 8 ---------------------------
column <- 8
rows <- c(perB_July, perB_Aug,
          perB_Sept, perB_Oct,
          perB_Nov, perB_Dec,
          perB_Jan, perB_Feb,
          perB_Mar, perB_Apr,
          perB_May, perB_Jun,
          perC_July, perC_Aug,
          perC_Sept, perC_Oct,
          perC_Nov, perC_Dec,
          perC_Jan, perC_Feb,
          perC_Mar, perC_Apr,
          perC_May, perC_Jun) # Months July-October for Periods A and B
rows1 <- c(perB_July, perB_Aug,
           perB_Sept, perB_Oct,
           perB_Nov, perB_Dec,
           perB_Jan, perB_Feb,
           perB_Mar, perB_Apr,
           perB_May, perB_Jun
)
rows2 <- c(perC_July, perC_Aug,
           perC_Sept, perC_Oct,
           perC_Nov, perC_Dec,
           perC_Jan, perC_Feb,
           perC_Mar, perC_Apr,
           perC_May, perC_Jun
)
Data <- NULL
obs <- rep(1:8, 12)
counts <- Gym_birds[rows1, column]
counts <- c(counts, Gym_birds[rows2, column])
group <-  Gym_birds$comb[rows]
Data <- cbind(obs, counts, group)

kruskal.test(counts ~ group, data = Data)
# Eastern Whipbird
kruskal.test(Gym_birds[rows,column], Gym_birds$comb[rows])
kruskal.test(Woon_birds[rows,column], Woon_birds$comb[rows])

# Compare Period C from Gympie to Period C from Woondum 
# for twelve months
column <- 8 # Eastern Whipbird
list  <- c("perC_July", "perC_Aug",
           "perC_Sept", "perC_Oct",
           "perC_Nov", "perC_Dec",
           "perC_Jan", "perC_Feb",
           "perC_Mar", "perC_Apr",
           "perC_May", "perC_Jun") # Months July-June for Periods C

rows  <- c(perC_July, perC_Aug,
          perC_Sept, perC_Oct,
          perC_Nov, perC_Dec,
          perC_Jan, perC_Feb,
          perC_Mar, perC_Apr,
          perC_May, perC_Jun) # Months July-June for Periods C
kw_test <- NULL
for(i in 1:length(list)) {
  Data <- NULL
  rows <- get(list[i]) 
  obs <- rep(1:8, 1)
  counts <- Gym_birds[rows, column]
  counts <- c(counts, Woon_birds[rows, column])
  group <-  Gym_birds$comb[rows]
  group <- c(group, Woon_birds$comb[rows])
  site <- rep("Gym", length(obs))
  site <- c(site, rep("Woon", length(obs)))
  Data <- cbind(obs, counts, group, site)
  Data <- data.frame(Data)
  t <- kruskal.test(counts ~ site, data = Data)
  kw_test <- c(kw_test, t)
} 

# or
kruskal.test(rbind(Gym_birds[rows, column], Woon_birds[rows, column]), 
             rbind(Gym_birds$comb[rows], Woon_birds$comb[rows]))


# Sunrise Sunset plot ---------------------------------------

# Cluster-Sunrise plots--------------------------------------------
rm(list = ls())

source("scripts/Bird_lists_func.R")
colours <- c("red","darkgreen","yellow","orange","skyblue1",
             "seashell3","darkorchid1","hotpink","green","mediumblue","cyan")
library(scales)
dev.off()
show_col(colours)
dev.off()

birds <- c(3,11,14,15,33,37,39,43,58)
birds1 <- c(11,15,37,43,58)
birds <- c(3,11,14,15,37,39,43,58,55)
colours <- c("red", "darkgreen", "yellow", "orange", "seashell3",
             "skyblue1", "darkorchid1","green", "hotpink",
             "mediumblue", "cyan")

clusts <- birds
cols <- colours
pch <- 20

dev.off()
bird_cluster_plot(clusts = birds, cols = colours, pch = 15)
dev.off()
bird_cluster_plot(birds1, colours, 20)
dev.off()
bird_cluster_plot(c(11,37), colours, 20)
dev.off()

bird_cluster_plot(c(11,37,43), colours, 15)
dev.off()
bird_cluster_plot(c(1:10), colours, 20)
dev.off()
bird_cluster_plot(c(11:20), colours, 20)
dev.off()
bird_cluster_plot(c(21:30), colours, 20)
dev.off()
bird_cluster_plot(c(31:40), colours, 20)
dev.off()
bird_cluster_plot(c(41:50), colours, 20)
dev.off()
bird_cluster_plot(c(51:60), colours, 20)

#bird_clust_plot(c(1:5), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(6:10), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(11:15), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(16:20), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(21:25), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(26:30), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(31:35), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(36:40), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(41:45), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(46:50), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(51:55), c("black","red","green","orange","brown"),20)
#bird_clust_plot(c(56:60), c("black","red","green","orange","brown"),20)


kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\20150621.csv", header = T)
bird_clust_plot(kalscpe_data = kalscpe_data)

kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150705\\GympieNP\\20150705.csv", header = T)
#unique(kalscpe_data$MANUAL.ID)
manual_id(kalscpe_data)


#Start of Sunrise plot ------------------------------------
dev.off()
xlim=c(240, 402) # time
ylim1 <- c(-length(civ_dawn), -1) # days
ylim <- c(-85, 0)

#ylim <- c(-180,-90)
#ylim <- c(-200, -90)
tiff("morning_chorus.tiff", height = 2000, width = 4000)
par(mar=c(2,3,2,1), bty="n", cex=4)
plot(x = civ_dawn, y = (ylim1[2]:ylim1[1]), type="n", axes=FALSE, ann=FALSE)

#kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\cluster Gympie first week.csv", header = T)
kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\20150621.csv", header = T)

col_names <- colnames(kalscpe_data)
b <- which(col_names=="MANUAL.ID.")
col_names[b] <- "MANUAL.ID"
colnames(kalscpe_data) <- col_names
bird_clust_plot(kalscpe_data)
bird_plot("Eastern Whipbird", "blue", 20)
bird_plot(species = "White-throated Honeyeater", col = "darkgreen", pch = 20)
bird_plot("Eastern Yellow Robin", "hotpink", 20)
bird_plot("Scarlet Honeyeater", "#FFD700", 20)
bird_plot("White-throated Treecreeper", "orangered3", 20)
bird_plot("Torresian Crow", "black", 20)
bird_plot("Pied Currawong", "purple", 20)
bird_plot("Laughing Kookaburra", "green", 20)
bird_plot("Australian Magpie","red", 20)
bird_plot("Southern Boobook", "orange", 20)
bird_plot("Spotted Pardalote", "cyan", 20)
bird_plot("Lewin's Honeyeater", "darkorchid4", 20)
bird_plot("Yellow-tailed Black Cockatoo", "yellow", 20)

#kalscpe_data <- read.csv("C:\\Work\\Outputs\\GympieNP\\GympieNP_13 Sept\\GympieNP 13 Sept.csv", header = T)
kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150628\\GympieNP\\20150628.csv", header = T)

a <- which(kalscpe_data$CHANNEL=="0")
kalscpe_data <- kalscpe_data[a,]
bird_clust_plot(kalscpe_data)
bird_plot("Eastern Whipbird", "blue", 20)
bird_plot(species = "White-throated Honeyeater", col = "darkgreen", pch = 20)
bird_plot("Eastern Yellow Robin", "hotpink", 20)
bird_plot("Scarlet Honeyeater", "#FFD700", 20)
bird_plot("White-throated Treecreeper", "orangered3", 20)
bird_plot("Torresian Crow", "black", 20)
bird_plot("Pied Currawong", "purple", 20)
bird_plot("Laughing Kookaburra", "green", 20)
bird_plot("Australian Magpie","red", 20)
bird_plot("Southern Boobook", "orange", 20)
bird_plot("Spotted Pardalote", "cyan", 20)
bird_plot("Lewin's Honeyeater", "darkorchid4", 20)

kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150705\\GympieNP\\20150705.csv", header = T)
bird_clust_plot(kalscpe_data)
bird_plot("Eastern Whipbird", "blue", 20)
bird_plot(species = "White-throated Honeyeater", col = "darkgreen", pch = 20)
bird_plot("Eastern Yellow Robin", "hotpink", 20)
bird_plot("Scarlet Honeyeater", "#FFD700", 20)
bird_plot("White-throated Treecreeper", "orangered3", 20)
bird_plot("Torresian Crow", "black", 20)
bird_plot("Pied Currawong", "purple", 20)
bird_plot("Laughing Kookaburra", "green", 20)
bird_plot("Australian Magpie","red", 20)
bird_plot("Southern Boobook", "orange", 20)
bird_plot("Spotted Pardalote", "cyan", 20)
bird_plot("Lewin's Honeyeater", "darkorchid4", 20)

kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150712\\GympieNP\\20150712.csv", header = T)
col_names <- colnames(kalscpe_data)
b <- which(col_names=="MANUAL.ID.")
col_names[b] <- "MANUAL.ID"
colnames(kalscpe_data) <- col_names
bird_clust_plot(kalscpe_data)
bird_plot("Eastern Whipbird", "blue", 20)
bird_plot(species = "White-throated Honeyeater", col = "darkgreen", pch = 20)
bird_plot("Eastern Yellow Robin", "hotpink", 20)
bird_plot("Scarlet Honeyeater", "#FFD700", 20)
bird_plot("White-throated Treecreeper", "orangered3", 20)
bird_plot("Torresian Crow", "black", 20)
bird_plot("Pied Currawong", "purple", 20)
bird_plot("Laughing Kookaburra", "green", 20)
bird_plot("Australian Magpie","red", 20)
bird_plot("Southern Boobook", "orange", 20)
bird_plot("Spotted Pardalote", "cyan", 20)
bird_plot("Lewin's Honeyeater", "darkorchid4", 20)

kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150719\\GympieNP\\20150719.csv", header = T)
col_names <- colnames(kalscpe_data)
b <- which(col_names=="MANUAL.ID.")
col_names[b] <- "MANUAL.ID"
colnames(kalscpe_data) <- col_names
bird_clust_plot(kalscpe_data)
bird_plot("Eastern Whipbird", "blue", 20)
bird_plot(species = "White-throated Honeyeater", col = "darkgreen", pch = 20)
bird_plot("Eastern Yellow Robin", "hotpink", 20)
bird_plot("Scarlet Honeyeater", "#FFD700", 20)
bird_plot("White-throated Treecreeper", "orangered3", 20)
bird_plot("Torresian Crow", "black", 20)
bird_plot("Pied Currawong", "purple", 20)
bird_plot("Laughing Kookaburra", "green", 20)
bird_plot("Australian Magpie","red", 20)
bird_plot("Southern Boobook", "orange", 20)
bird_plot("Spotted Pardalote", "cyan", 20)
bird_plot("Lewin's Honeyeater", "darkorchid4", 20)

kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150726\\GympieNP\\20150726.csv", header = T)
col_names <- colnames(kalscpe_data)
b <- which(col_names=="MANUAL.ID.")
col_names[b] <- "MANUAL.ID"
colnames(kalscpe_data) <- col_names
bird_clust_plot(kalscpe_data)
bird_plot("Eastern Whipbird", "blue", 20)
bird_plot(species = "White-throated Honeyeater", col = "darkgreen", pch = 20)
bird_plot("Eastern Yellow Robin", "hotpink", 20)
bird_plot("Scarlet Honeyeater", "#FFD700", 20)
bird_plot("White-throated Treecreeper", "orangered3", 20)
bird_plot("Torresian Crow", "black", 20)
bird_plot("Pied Currawong", "purple", 20)
bird_plot("Laughing Kookaburra", "green", 20)
bird_plot("Australian Magpie","red", 20)
bird_plot("Southern Boobook", "orange", 20)
bird_plot("Spotted Pardalote", "cyan", 20)
bird_plot("Lewin's Honeyeater", "darkorchid4", 20)

kalscpe_data <- read.csv("C:\\Work\\Kaleidoscope\\20150802\\GympieNP\\20150802.csv", header = T)
col_names <- colnames(kalscpe_data)
b <- which(col_names=="MANUAL.ID.")
col_names[b] <- "MANUAL.ID"
colnames(kalscpe_data) <- col_names
bird_clust_plot(kalscpe_data)
bird_plot("Eastern Whipbird", "blue", 20)
bird_plot("White-throated Honeyeater", col = "darkgreen", pch = 20)
bird_plot("Eastern Yellow Robin", "hotpink", 20)
bird_plot("Scarlet Honeyeater", "#FFD700", 20)
bird_plot("White-throated Treecreeper", "orangered3", 20)
bird_plot("Torresian Crow", "black", 20)
bird_plot("Pied Currawong", "purple", 20)
bird_plot("Laughing Kookaburra", "green", 20)
bird_plot("Australian Magpie","red", 20)
bird_plot("Southern Boobook", "orange", 20)
bird_plot("Spotted Pardalote", "cyan", 20)
bird_plot("Lewin's Honeyeater", "darkorchid4", 20)

legend(x=(xlim[1]-1.5),y=ylim[2]+1,legend=c("White-throated Honeyeater",
                                            "Eastern Yellow Robin",
                                            "Scarlet Honeyeater",
                                            "White-throated Treecreeper",
                                            "Torresian Crow",
                                            "Eastern Whipbird",
                                            "Pied Currawong",
                                            "Laughing Kookaburra",
                                            "Australian Magpie",
                                            "Spotted Pardalote",
                                            "Lewin's Honeyeater",
                                            "Yellow-tailed Black Cockatoo"),
       col=c("darkgreen","hotpink","#FFD700","orangered3","black",
             "blue","purple","green","red","orange","cyan", "darkorchid4"),
       pch = 20, bty = "n", cex = 1.2)
text(x = -10, y = xlim[1]+20, paste(pch = 4, pch = 4, pch = 4, pch = 4))
par(new=TRUE)
plot(x = civ_dawn, y = (ylim1[2]:ylim1[1]), 
     type = "l", yaxt="n", xaxt="n", lty = "1F", 
     ylab = "", ylim = ylim, xlim = xlim)

list <- c("01","05","10","15","20","25")
at <- NULL
label2 <- NULL
a_ref <- which(substr(civil_dawn$dates,1,10)==substr(start,1,10))
for(i in 1:length(list)) {
  a1 <- which(substr(civil_dawn$dates[a_ref:nrow(civil_dawn)],9,10)==list[i])
  at <- c(at, a1)
  b1 <- rep(list[i],length(a1))
  label2 <- c(label2, b1)
}
axis(side = 2, at = -at, line = -1.2, labels = label2, mgp=c(4,1,0))

a <- which(substr(civil_dawn$dates[a_ref:nrow(civil_dawn)],9,10)=="01")
a <- a[1:13]
a <- c(-29, a)
abline(h=-a, lty=5, lwd=0.1)

labels <- c("Jun", "Jul", "Aug", "Sep", "Oct", "Nov",
            "Dec", "Jan", "Feb", "Mar", "Apr",
            "May", "Jun", "Jul")
axis(side = 2, at = -a-14, line = NA, labels = labels, tick = F)
par(new=T)
plot(x = civ_dawn-5, y = -1:-length(civ_dawn), type = "l", 
     lty = 5, ylim = ylim, ylab="", yaxt="n",
     xaxt="n", lwd=3, xlim = xlim)
par(new=T)
plot(x = civ_dawn+5, y = -1:-length(civ_dawn), type = "l", 
     lty = 5, ylim = ylim, ylab="", xaxt="n", lwd=3,
     yaxt="n", xlim = xlim)
par(new=T)
plot(x = civ_dawn-15, y = -1:-length(civ_dawn), 
     type = "l", lty=5, ylim = ylim, ylab="", lwd=3, 
     xaxt="n", yaxt="n", xlim = xlim)
par(new=T)
plot(x = civ_dawn+15, y = -1:-length(civ_dawn), type = "l", 
     lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
     yaxt="n", xlim = xlim)
par(new=T)
plot(x = civ_dawn-25, y = -1:-length(civ_dawn), type = "l", 
     lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
     yaxt="n", xlim = xlim)
par(new=T)
plot(x = civ_dawn+25, y = -1:-length(civ_dawn), type = "l", 
     lty=5, ylim = ylim, ylab="", xaxt="n", lwd=3,
     yaxt="n", xlim = xlim)
at <- c(60, 120, 180, 240, 300, 360, 420, 480, 540, 600)
labels1 <- c("1 am", "2 am", "3 am",
             "4 am","5 am","6 am","7 am",
             "8 am","9 am","10 am")
abline(v=at)
axis(side=1, at = at, labels = labels1, las=1)
#library(plotrix)
#draw.circle(x = 20, y = 350, radius = 4*1.0, col = "brown", border = "white")
if(ylim[2]==0) {
  text(x = (civ_dawn[-ylim[2]+1] - 20), y = ylim[2]+0.4, "A")
  text(x = civ_dawn[-ylim[2]+1], y = ylim[2]+0.4, "B")
  text(x = civ_dawn[-ylim[2]+1] + 20, y = ylim[2]+0.4, "C")
}

if(ylim[2] < 0) {
  text(x = (civ_dawn[-ylim[2]] - 20), y = ylim[2]+1.4, "A")
  text(x = civ_dawn[-ylim[2]], y = ylim[2]+1.4, "B")
  text(x = civ_dawn[-ylim[2]] + 20, y = ylim[2]+1.4, "C")
}

dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Start species plots ----------------------------------------------

dev.off()
species_plots("Australian Magpie","red")
dev.off()
species_plots("Australian King Parrot", "green")
dev.off()
species_plots("White-throated Nightjar", "green")
dev.off()
species_plots("Southern Boobook", "green")
dev.off()
species_plots("Yellow-tailed Black Cockatoo","darkorchid4")
dev.off()
species_plots("Eastern Yellow Robin","hotpink")
dev.off()
species_plots("Laughing Kookaburra","darkgreen")
dev.off()
species_plots("White-throated Honeyeater","darkgreen")
dev.off()
species_plots("Lewin's Honeyeater","darkorchid4")
dev.off()
species_plots("White-throated Treecreeper","orangered3")
dev.off()
species_plots("Scarlet Honeyeater","#FFD700")
dev.off()
species_plots("Torresian Crow","black")
dev.off()
species_plots("Pied Currawong","purple")
dev.off()
species_plots("Spotted Pardalote","orange")
dev.off()
species_plots("Silvereye", "darkgreen")
dev.off()
species_plots("Russet-tailed Thrush", "green")
dev.off()
species_plots("Eastern Whipbird","blue")
dev.off()
species_plots("Brown Cuckoo-dove","blue")
dev.off()
species_plots("Golden Whistler","blue")
dev.off()
species_plots("Mistletoebird","blue")
dev.off()
species_plots("Powerful Owl","blue")
dev.off()
species_plots("Peaceful Dove","blue")

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Collate bird data
source("scripts/Bird_lists_func.R")

list1 <- c("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\20150621.csv",
           "C:\\Work\\Kaleidoscope\\20150705\\GympieNP\\20150705.csv",
           "C:\\Work\\Kaleidoscope\\20150712\\GympieNP\\20150712.csv",
           "C:\\Work\\Kaleidoscope\\20150719\\GympieNP\\20150719.csv",
           "C:\\Work\\Kaleidoscope\\20150726\\GympieNP\\20150726.csv",
           "C:\\Work\\Kaleidoscope\\20150802\\GympieNP\\20150802.csv",
           "C:\\Work\\Kaleidoscope\\20150809\\GympieNP\\20150809.csv",
           "C:\\Work\\Kaleidoscope\\20150816\\GympieNP\\20150816.csv")
kalscpe_data_all <- NULL
kalscpe_data <- read.csv(list1[1], header = T)
bird_clust_plot(kalscpe_data)
col_names2 <- colnames(kalscpe_data)
col_names2[22] <- "MANUAL.ID"

for(i in 1:length(list1)) {
  kalscpe_data <- NULL
  kalscpe_data <- read.csv(list1[i], header = T)
  col_names <- colnames(kalscpe_data)
  b <- which(col_names2=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  bird_clust_plot(kalscpe_data)
  colnames(kalscpe_data) <- col_names2
  kalscpe_data_all <- rbind(kalscpe_data_all, kalscpe_data)
}

write.csv(kalscpe_data_all, "data\\kalscpe_data.csv", row.names = F)

source("scripts/Bird_lists_func.R")
# Save individual species files into folders
list1 <- c(#"C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\20150621.csv",
  #"C:\\Work\\Kaleidoscope\\20150705\\GympieNP\\20150705.csv",
  #"C:\\Work\\Kaleidoscope\\20150712\\GympieNP\\20150712.csv",
  #"C:\\Work\\Kaleidoscope\\20150719\\GympieNP\\20150719.csv",
  "C:\\Work\\Kaleidoscope\\20150726\\GympieNP\\20150726.csv" #,
  #"C:\\Work\\Kaleidoscope\\20150802\\GympieNP\\20150802.csv",
  #"C:\\Work\\Kaleidoscope\\20150809\\GympieNP\\20150809.csv",
  #"C:\\Work\\Kaleidoscope\\20150816\\GympieNP\\20150816.csv"
)

data <- read.csv("data\\kalscpe_data.csv", header = T)
bird_list <- colnames(data[,23:length(kalscpe_data)])
for(i in 1:length(list1)) {
  kalscpe_data <- read.csv(list1[i])
  meta <- read.csv(paste(substr(list1[i],1,(nchar(list1[i])-12)),"\\meta.csv",sep = ""))
  bird_clust_plot(kalscpe_data)
  for(j in 1:length(bird_list)) {
    species_data <- kalscpe_data[,c(1:22)]
    species_data[22] <- kalscpe_data[,(22+j)]
    write.csv(species_data, 
              paste(substr(list1[i],1,(nchar(list1[i])-4)),"_", bird_list[j],".csv",sep = ""),
              row.names = F)
  }
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Eastern Whipbird ------------------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
#list <- c(3, 11,15,35,37,39,43,58)
#stats <- data.frame(Clust3=NA,
#                    Clust11=NA,
#                    Clust15=NA,
#                    Clust35=NA,
#                    Clust37=NA,
#                    Clust39=NA,
#                    Clust43=NA,
#                    Clust58=NA)
#unique_dates <- unique(Gym_cluster_list$dates)
#for(i in 1:56) {
#  a <- which(Gym_cluster_list$dates==unique_dates[i] & Gym_cluster_list$minute_reference < 406)
#  clust_temp <- Gym_cluster_list$cluster_list[a]
#  for(j in 1:length(list)) {
#    cluster <- list[j]
#  stats[i,j] <- length(which(clust_temp==cluster))
#  }
#}
#for(i in 1:56) {
#  stats$totals[i] <- sum(stats[1:length(stats),i])
#}
#length <- length(stats$Clust3)
#for(i in 1:8) {
#  stats[(length+1), i] <- sum(stats[1:length,i])
#}

data2 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Eastern Whipbird.csv", header = T)
label_name <- "Eastern Whipbird"
dev.off()
list <- unique(data2$MANUAL.ID)
list <- sort(list)
a <- which(list=="-")
list <- list[-a]
a <- which(list=="")
list <- list[-a]
list
colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_monochromatic_",label_name,".tiff",sep=""),
     height = 1900, width = 3000)

for(i in 1:length(list)) {
  label <- list[i]
  a <- which(data2$MANUAL.ID==label)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

a <- grep("EW", data2$MANUAL.ID, ignore.case = T)
call_data <- data2[a,]

for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

# Tabulate dataset
t <- table(dataset$date, dataset$min)
t <- data.frame(t)
col_names <- c("date","minute","freq")
colnames(t) <- col_names
a <- which(t$freq > 0)
t <- t[a,]
t$minute <- as.numeric(levels(t$minute))[t$minute]
col_names <- colnames(indices_norm_summary)

for(i in 1:nrow(t)) {
  a <- which(t$date[i]==uniq_dates)
  minute_ref <- (a-1)*1440 + as.numeric(t$minute[i])
  t$cluster[i] <- Gym_cluster_list$cluster_list[minute_ref]
  t$BackgroundNoise[i] <- indices_norm_summary$BackgroundNoise[minute_ref]
  t$Snr[i] <- indices_norm_summary$Snr[minute_ref]
  t$Activity[i] <- indices_norm_summary$Activity[minute_ref]
  t$EventsPerSecond[i] <- indices_norm_summary$EventsPerSecond[minute_ref]
  t$AcousticComplexity[i] <- indices_norm_summary$AcousticComplexity[minute_ref]
  t$ClusterCount[i] <- indices_norm_summary$ClusterCount[minute_ref]
  t$HighFreqCover[i] <- indices_norm_summary$HighFreqCover[minute_ref]
  t$MidFreqCover[i] <- indices_norm_summary$MidFreqCover[minute_ref]
  t$LowFreqCover[i] <- indices_norm_summary$LowFreqCover[minute_ref]
  t$EntropyOfAverageSpectrum[i] <- indices_norm_summary$EntropyOfAverageSpectrum[minute_ref]
  t$EntropyOfPeaksSpectrum[i] <- indices_norm_summary$EntropyOfPeaksSpectrum[minute_ref]
  t$EntropyOfCoVSpectrum[i] <- indices_norm_summary$EntropyOfCoVSpectrum[minute_ref]
}

# Choose a minimum frequency
n <- 4
a <- which(t$freq >= n)
t <- t[a,]

# Save cluster distribtion histogram
tiff(paste("histogram_",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t2 <- tabulate(t$cluster, nbins = 60)
b <- barplot(t2, ylim = c(0, (max(t2)+0.1*max(t2))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t2 > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t2[a])+0.05*max(t2)), t2[a])
text(b[58], max(t2)+0.08*max(t2), paste("n = ",sum(t2), " minutes", sep = ""))
text(b[31], max(t2)+0.08*max(t2), paste("Minutes with ", n, " or greater calls", sep = ""))
title(main = label_name, line=1)
dev.off()
dev.off()

# Boxplots
tiff(paste(label_name,".tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)
col_names <- colnames(t)
ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(t[(i+4)], 
               main=paste(label_name, " - ", col_names[(i+4)],sep = ""), 
               ylim=ylim, ylab = ylab)
}
dev.off()


#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%555
# Lewin's Honeyeater -----------------------------------------
rm(list=ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Lewin's Honeyeater.csv", header = T)
label_name <- "Lewin's Honeyeater"
dev.off()
list <- unique(data2$MANUAL.ID)
list <- sort(list)
a <- which(list=="-")
list <- list[-a]
a <- which(list=="")
list <- list[-a]

list

tiff(paste("morning_chorus_",label_name,"test.tiff",sep=""),
     height = 1800, width = 3000)

for(i in 1:length(list)) {
  label <- list[i]
  a <- which(data2$MANUAL.ID==label)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()


a <- grep("good", data2$MANUAL.ID, ignore.case = T)
call_data <- data2[a,]

for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

uniq_dates2 <- unique(dataset$date)
freq_table <- NULL
dataset3 <- NULL
ref <- 0
dataset3 <- data.frame(dataset3)
for(i in 1:length(uniq_dates2)) {
  a <- which(dataset$date==uniq_dates2[i])
  dataset2 <- dataset[a,]
  uniq_minutes <- unique(dataset2$minute)
  for(j in 1:length(uniq_minutes)) {
    ref <- ref + 1
    b <- which(dataset2$date==uniq_dates2[i] & dataset2$minute==uniq_minutes[j])
    if(length(b) > 0) {
      cluster <- dataset2$cluster[b[1]]  
    }
    if(length(b) == 0) {
      cluster <- "-"
    }
    frequency <- length(which(dataset2$minute==uniq_minutes[j]))
    dataset3[ref,1] <- as.character(uniq_dates2[i])
    dataset3[ref,2] <- uniq_minutes[j]
    dataset3[ref,3] <- frequency
    dataset3[ref,4] <- cluster
  }
}
col_names <- c("date","minute","freq","cluster")
colnames(dataset3) <- col_names

# Table
table(dataset3$cluster) # loud calls only

n <- 1
a <- which(dataset3$freq > n)
dataset4 <- dataset3[a,]
table(dataset4$cluster)

# Using dataset4 extract minutes that match
dates <- unique(dates)
ref1 <- length(dataset4)
dataset4[,(ref1+1):(ref1+length(indices_norm_summary))] <- 0
col_names1 <- colnames(dataset4)
col_names1 <- col_names1[1:4]
col_names <- colnames(indices_norm_summary)
col_names <- c(col_names1, col_names)
colnames(dataset4) <- col_names
for(i in 1:nrow(dataset4)) {
  a <- which(dataset4$date[i]==dates)                   
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset4[i,(ref1+1):(ref1+length(indices_norm_summary))] <- unname(indices_norm_summary[min_ref,])
}
# Boxplots
tiff(paste(label_name,".tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)

ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(dataset4[i+4], 
               main=paste(label_name, " - ", col_names[i+4],sep = ""), ylim=ylim, ylab = ylab)
}
dev.off()

# Save cluster distribtion histogram
tiff(paste("histogram_",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t <- tabulate(dataset4$cluster, nbins = 60)
b <- barplot(t, ylim = c(0, (max(t)+0.1*max(t))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t[a])+0.05*max(t)), t[a])
text(b[58], max(t)+0.08*max(t), paste("n = ",sum(t), " minutes", sep = ""))
text(b[31], max(t)+0.08*max(t), paste("Minutes with greater than ", n, " calls", sep = ""))
title(main = label_name, line=1)
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Laughing Kookaburra --------------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Laughing Kookaburra.csv", header = T)
label_name <- "Laughing Kookaburra"
dev.off()
list <- NULL
list <- unique(data2$MANUAL.ID)
a <- which(list=="-")
list <- list[-a]
a <- which(list=="")
list <- list[-a]
list
tiff(paste("morning_chorus_",label_name,".tiff",sep=""),
     height = 3600, width = 3000)

for(i in 1:length(list)) {
  label <- list[i]
  a <- which(data2$MANUAL.ID==label)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

a <- grep("loud", data2$MANUAL.ID, ignore.case = T)
a1 <- grep("mod", data2$MANUAL.ID, ignore.case = T)
a2 <- grep("cackle", data2$MANUAL.ID, ignore.case = T)
a <- c(a,a1,a2)
call_data <- data2[a,]

for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

# Tabulate dataset
t <- table(dataset$date, dataset$min)
t <- data.frame(t)
col_names <- c("date","minute","freq")
colnames(t) <- col_names
a <- which(t$freq > 0)
t <- t[a,]
t$minute <- as.numeric(levels(t$minute))[t$minute]
col_names <- colnames(indices_norm_summary)

for(i in 1:nrow(t)) {
  a <- which(t$date[i]==uniq_dates)
  minute_ref <- (a-1)*1440 + as.numeric(t$minute[i])
  t$cluster[i] <- Gym_cluster_list$cluster_list[minute_ref]
  t$BackgroundNoise[i] <- indices_norm_summary$BackgroundNoise[minute_ref]
  t$Snr[i] <- indices_norm_summary$Snr[minute_ref]
  t$Activity[i] <- indices_norm_summary$Activity[minute_ref]
  t$EventsPerSecond[i] <- indices_norm_summary$EventsPerSecond[minute_ref]
  t$AcousticComplexity[i] <- indices_norm_summary$AcousticComplexity[minute_ref]
  t$ClusterCount[i] <- indices_norm_summary$ClusterCount[minute_ref]
  t$HighFreqCover[i] <- indices_norm_summary$HighFreqCover[minute_ref]
  t$MidFreqCover[i] <- indices_norm_summary$MidFreqCover[minute_ref]
  t$LowFreqCover[i] <- indices_norm_summary$LowFreqCover[minute_ref]
  t$EntropyOfAverageSpectrum[i] <- indices_norm_summary$EntropyOfAverageSpectrum[minute_ref]
  t$EntropyOfPeaksSpectrum[i] <- indices_norm_summary$EntropyOfPeaksSpectrum[minute_ref]
  t$EntropyOfCoVSpectrum[i] <- indices_norm_summary$EntropyOfCoVSpectrum[minute_ref]
}

# Choose a minimum frequency
n <- 1
a <- which(t$freq >= n)
t <- t[a,]

# Save cluster distribtion histogram
tiff(paste("histogram_",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t2 <- tabulate(t$cluster, nbins = 60)
b <- barplot(t2, ylim = c(0, (max(t2)+0.1*max(t2))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t2 > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t2[a])+0.05*max(t2)), t2[a])
text(b[58], max(t2)+0.08*max(t2), paste("n = ",sum(t2), " minutes", sep = ""))
text(b[31], max(t2)+0.08*max(t2), paste("Minutes with ", n, " or greater calls", sep = ""))
title(main = label_name, line=1)
dev.off()
dev.off()

# Boxplots
tiff(paste(label_name,".tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)
col_names <- colnames(t)
ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(t[(i+4)], 
               main=paste(label_name, " - ", col_names[(i+4)],sep = ""), 
               ylim=ylim, ylab = ylab)
}
dev.off()

#####################
for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

uniq_dates2 <- unique(dataset$date)
freq_table <- NULL
dataset3 <- NULL
ref <- 0
dataset3 <- data.frame(dataset3)
for(i in 1:length(uniq_dates2)) {
  a <- which(dataset$date==uniq_dates2[i])
  dataset2 <- dataset[a,]
  uniq_minutes <- unique(dataset2$minute)
  for(j in 1:length(uniq_minutes)) {
    ref <- ref + 1
    b <- which(dataset2$date==uniq_dates2[i] & dataset2$minute==uniq_minutes[j])
    if(length(b) > 0) {
      cluster <- dataset2$cluster[b[1]]  
    }
    if(length(b) == 0) {
      cluster <- "-"
    }
    frequency <- length(which(dataset2$minute==uniq_minutes[j]))
    dataset3[ref,1] <- as.character(uniq_dates2[i])
    dataset3[ref,2] <- uniq_minutes[j]
    dataset3[ref,3] <- frequency
    dataset3[ref,4] <- cluster
  }
}
col_names <- c("date","minute","freq","cluster")
colnames(dataset3) <- col_names

# Table
table(dataset3$cluster) # loud calls only

n <- 1
a <- which(dataset3$freq > n)
dataset4 <- dataset3[a,]
table(dataset4$cluster)

# Using dataset4 extract minutes that match
dates <- unique(dates)
ref1 <- length(dataset4)
dataset4[,(ref1+1):(ref1+length(indices_norm_summary))] <- 0
col_names1 <- colnames(dataset4)
col_names1 <- col_names1[1:4]
col_names <- colnames(indices_norm_summary)
col_names <- c(col_names1, col_names)
colnames(dataset4) <- col_names
for(i in 1:nrow(dataset4)) {
  a <- which(dataset4$date[i]==dates)                   
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset4[i,(ref1+1):(ref1+length(indices_norm_summary))] <- unname(indices_norm_summary[min_ref,])
}
# Boxplots
tiff(paste(label_name,".tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)

ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(dataset4[i+4], 
               main=paste(label_name, " - ", col_names[i+4],sep = ""), ylim=ylim, ylab = ylab)
}
dev.off()

# Save cluster distribtion histogram
tiff(paste("histogram_",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t <- tabulate(dataset4$cluster, nbins = 60)
b <- barplot(t, ylim = c(0, (max(t)+0.1*max(t))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t[a])+0.05*max(t)), t[a])
text(b[58], max(t)+0.08*max(t), paste("n = ",sum(t), " minutes", sep = ""))
text(b[31], max(t)+0.08*max(t), paste("Minutes with greater than ", n, " calls", sep = ""))
title(main = label_name, line=1)
dev.off()
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%5
# White-throated Honeyeater ------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("H:\\Work\\Kaleidoscope\\20150621\\GympieNP\\White-throated Honeyeater.csv", header = T)
label_name <- "White-throated Honeyeater"
dev.off()
list <- NULL
list <- unique(data2$MANUAL.ID)
list <- sort(list)
a <- which(list=="-")
list <- list[-a]
a <- which(list=="")
list <- list[-a]

all_data <- read.csv("all_data_added_protected.csv", header = T)[,c(1:21,38)]
list <- c("WTH Far",  "WTH Mod",  "WTH Near")

colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_monochromatic_",label_name,".tiff",sep=""),
     height = 1900, width = 3000)

for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

label2_list <- c("near", "mod") 
for(k in 1:length(label2_list)) {
  label2 <- label2_list[k]
  a <- grep(label2, data2$MANUAL.ID, ignore.case = T)
  call_data <- data2[a,]
  for(i in 1:nrow(call_data)) {
    hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
    minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
    second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
    call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
  }
  
  dataset <- NULL
  for(i in 1:nrow(call_data)) {
    dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
    dataset$minute[i] <- call_data$minute[i]
  }
  uniq_dates  <- unique(dates)
  dataset$cluster <- NULL
  dataset <- data.frame(dataset)
  for(i in 1:nrow(dataset)) {
    a <- which(uniq_dates==dataset$date[i])
    min_ref <- (a-1)*1440 + dataset$minute[i]
    dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
  }
  
  uniq_dates2 <- unique(dataset$date)
  freq_table <- NULL
  dataset3 <- NULL
  ref <- 0
  dataset3 <- data.frame(dataset3)
  for(i in 1:length(uniq_dates2)) {
    a <- which(dataset$date==uniq_dates2[i])
    dataset2 <- dataset[a,]
    uniq_minutes <- unique(dataset2$minute)
    for(j in 1:length(uniq_minutes)) {
      ref <- ref + 1
      b <- which(dataset2$date==uniq_dates2[i] & dataset2$minute==uniq_minutes[j])
      if(length(b) > 0) {
        cluster <- dataset2$cluster[b[1]]  
      }
      if(length(b) == 0) {
        cluster <- "-"
      }
      frequency <- length(which(dataset2$minute==uniq_minutes[j]))
      dataset3[ref,1] <- as.character(uniq_dates2[i])
      dataset3[ref,2] <- uniq_minutes[j]
      dataset3[ref,3] <- frequency
      dataset3[ref,4] <- cluster
    }
  }
  col_names <- c("date","minute","freq","cluster")
  colnames(dataset3) <- col_names
  
  # Table
  table(dataset3$cluster) # loud calls only
  if(k==1) {
    n <- 2
    a <- which(dataset3$freq > n)
  }
  if(k==2) {
    n <- 0
    a <- which(dataset3$freq > n)
  }
  dataset4 <- dataset3[a,]
  table(dataset4$cluster)
  
  # Using dataset4 extract minutes that match
  dates <- unique(dates)
  ref1 <- length(dataset4)
  dataset4[,(ref1+1):(ref1+length(indices_norm_summary))] <- 0
  col_names1 <- colnames(dataset4)
  col_names1 <- col_names1[1:4]
  col_names <- colnames(indices_norm_summary)
  col_names <- c(col_names1, col_names)
  colnames(dataset4) <- col_names
  for(i in 1:nrow(dataset4)) {
    a <- which(dataset4$date[i]==dates)                   
    min_ref <- (a-1)*1440 + dataset$minute[i]
    dataset4[i,(ref1+1):(ref1+length(indices_norm_summary))] <- unname(indices_norm_summary[min_ref,])
  }
  # Boxplots
  tiff(paste(label_name,"_",label2,".tiff", sep = ""), width = 2000, height = 1550)
  par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
      cex.lab=3, cex.axis=3, cex.main=3)
  
  ylim <- c(0,1)
  ylab <- "Normalised index"
  for(i in 1:12) {
    b <- boxplot(dataset4[i+4], 
                 main=paste(label_name, " - ", col_names[i+4],sep = ""), ylim=ylim, ylab = ylab)
  }
  dev.off()
  
  # Save cluster distribtion histogram
  tiff(paste(label_name,"_",label2,".tiff", sep = ""),
       height = 1000, width = 1550)
  par(cex=2, mar=c(4.2,4.2,3,1))
  t <- tabulate(dataset4$cluster, nbins = 60)
  b <- barplot(t, ylim = c(0, (max(t)+0.1*max(t))), col = "white", 
               ylab = "Frequency (number of minutes)",
               xlab = "Cluster number")
  l <- as.character(1:60)
  a <- which(t > 0)
  axis(side = 1, at = b, labels = l)
  text(b[a], (as.numeric(t[a])+0.05*max(t)), t[a])
  text(b[58], max(t)+0.08*max(t), paste("n = ",sum(t), " minutes", sep = ""))
  text(b[31], max(t)+0.08*max(t), paste("Minutes with greater than ", n, " calls", sep = ""))
  title(main = paste(label_name," - ",label2,sep = ""), line=1)
  dev.off()
}
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Eastern Yellow Robin --------------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Eastern Yellow Robin.csv", header = T)
label_name <- "Eastern Yellow Robin"
n <- 10 # minimum number of calls per minute
descriptor <- "Near"
dev.off()
list <- NULL
list <- unique(data2$MANUAL.ID)
list <- sort(list)
a <- which(list=="-")
list <- list[-a]
list

colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_monochromatic_",label_name,".tiff",sep=""),
     height = 1800, width = 3000)

for(i in 1:length(list)) {
  label <- list[i]
  a <- which(data2$MANUAL.ID==label)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label, colours[i],i)
  par(new=T)
}
dev.off()

a <- grep(descriptor, data2$MANUAL.ID, ignore.case = T)
call_data <- data2[a,]

for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

# Tabulate dataset
t <- table(dataset$date, dataset$min)
t <- data.frame(t)
col_names <- c("date","minute","freq")
colnames(t) <- col_names
a <- which(t$freq > 0)
t <- t[a,]
t$minute <- as.numeric(levels(t$minute))[t$minute]
col_names <- colnames(indices_norm_summary)

for(i in 1:nrow(t)) {
  a <- which(t$date[i]==uniq_dates)
  minute_ref <- (a-1)*1440 + as.numeric(t$minute[i])
  t$cluster[i] <- Gym_cluster_list$cluster_list[minute_ref]
  t$BackgroundNoise[i] <- indices_norm_summary$BackgroundNoise[minute_ref]
  t$Snr[i] <- indices_norm_summary$Snr[minute_ref]
  t$Activity[i] <- indices_norm_summary$Activity[minute_ref]
  t$EventsPerSecond[i] <- indices_norm_summary$EventsPerSecond[minute_ref]
  t$AcousticComplexity[i] <- indices_norm_summary$AcousticComplexity[minute_ref]
  t$ClusterCount[i] <- indices_norm_summary$ClusterCount[minute_ref]
  t$HighFreqCover[i] <- indices_norm_summary$HighFreqCover[minute_ref]
  t$MidFreqCover[i] <- indices_norm_summary$MidFreqCover[minute_ref]
  t$LowFreqCover[i] <- indices_norm_summary$LowFreqCover[minute_ref]
  t$EntropyOfAverageSpectrum[i] <- indices_norm_summary$EntropyOfAverageSpectrum[minute_ref]
  t$EntropyOfPeaksSpectrum[i] <- indices_norm_summary$EntropyOfPeaksSpectrum[minute_ref]
  t$EntropyOfCoVSpectrum[i] <- indices_norm_summary$EntropyOfCoVSpectrum[minute_ref]
}

# Select the minimum frequency
a <- which(t$freq >= n)
t <- t[a,]

# Save cluster distribtion histogram
tiff(paste("histogram_",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t2 <- tabulate(t$cluster, nbins = 60)
b <- barplot(t2, ylim = c(0, (max(t2)+0.1*max(t2))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t2 > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t2[a])+0.05*max(t2)), t2[a])
text(b[58], max(t2)+0.08*max(t2), paste("n = ",sum(t2), " minutes", sep = ""))
text(b[31], max(t2)+0.08*max(t2), paste("Minutes with ", n, " or greater calls", sep = ""))
title(main = label_name, line=1)
dev.off()
dev.off()

# Boxplots
tiff(paste(label_name,".tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)
col_names <- colnames(t)
ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(t[(i+4)], 
               main=paste(label_name, " - ", col_names[(i+4)],sep = ""), 
               ylim=ylim, ylab = ylab)
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# White-throated Treecreeper ----------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\White-throated Treecreeper.csv", header = T)
label_name <- "White-throated Treecreeper"
dev.off()
list <- NULL
list <- unique(data2$MANUAL.ID)
list <- sort(list)
a <- which(list=="-")
list <- list[-a]
list
tiff(paste("morning_chorus_",label_name,".tiff",sep=""),
     height = 3600, width = 3000)

for(i in 1:length(list)) {
  label <- list[i]
  a <- which(data2$MANUAL.ID==label)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

label2_list <- c("trill A", "trill B1", "trill B2", "trill E")
for(k in 1:length(label2_list)) {
  label2 <- label2_list[k]
  a <- grep(label2, data2$MANUAL.ID, ignore.case = T)
  call_data <- data2[a,]
  
  for(i in 1:nrow(call_data)) {
    hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
    minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
    second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
    call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
  }
  
  dataset <- NULL
  for(i in 1:nrow(call_data)) {
    dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
    dataset$minute[i] <- call_data$minute[i]
  }
  uniq_dates  <- unique(dates)
  dataset$cluster <- NULL
  dataset <- data.frame(dataset)
  for(i in 1:nrow(dataset)) {
    a <- which(uniq_dates==dataset$date[i])
    min_ref <- (a-1)*1440 + dataset$minute[i]
    dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
  }
  
  uniq_dates2 <- unique(dataset$date)
  freq_table <- NULL
  dataset3 <- NULL
  ref <- 0
  dataset3 <- data.frame(dataset3)
  for(i in 1:length(uniq_dates2)) {
    a <- which(dataset$date==uniq_dates2[i])
    dataset2 <- dataset[a,]
    uniq_minutes <- unique(dataset2$minute)
    for(j in 1:length(uniq_minutes)) {
      ref <- ref + 1
      b <- which(dataset2$date==uniq_dates2[i] & dataset2$minute==uniq_minutes[j])
      if(length(b) > 0) {
        cluster <- dataset2$cluster[b[1]]  
      }
      if(length(b) == 0) {
        cluster <- "-"
      }
      frequency <- length(which(dataset2$minute==uniq_minutes[j]))
      dataset3[ref,1] <- as.character(uniq_dates2[i])
      dataset3[ref,2] <- uniq_minutes[j]
      dataset3[ref,3] <- frequency
      dataset3[ref,4] <- cluster
    }
  }
  col_names <- c("date","minute","freq","cluster")
  colnames(dataset3) <- col_names
  
  # Table
  table(dataset3$cluster) # loud calls only
  
  n <- 1
  a <- which(dataset3$freq > n)
  dataset4 <- dataset3[a,]
  table(dataset4$cluster)
  
  # Using dataset4 extract minutes that match
  dates <- unique(dates)
  ref1 <- length(dataset4)
  dataset4[,(ref1+1):(ref1+length(indices_norm_summary))] <- 0
  col_names1 <- colnames(dataset4)
  col_names1 <- col_names1[1:4]
  col_names <- colnames(indices_norm_summary)
  col_names <- c(col_names1, col_names)
  colnames(dataset4) <- col_names
  for(i in 1:nrow(dataset4)) {
    a <- which(dataset4$date[i]==dates)                   
    min_ref <- (a-1)*1440 + dataset$minute[i]
    dataset4[i,(ref1+1):(ref1+length(indices_norm_summary))] <- unname(indices_norm_summary[min_ref,])
  }
  # Boxplots
  tiff(paste(label_name,"_",label2,".tiff"), width = 2000, height = 1550)
  par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
      cex.lab=3, cex.axis=3, cex.main=3)
  
  ylim <- c(0,1)
  ylab <- "Normalised index"
  for(i in 1:12) {
    b <- boxplot(dataset4[i+4], 
                 main=paste(label_name," ",label2, " - ", col_names[i+4],sep = ""), ylim=ylim, ylab = ylab)
  }
  dev.off()
  
  # Save cluster distribtion histogram
  tiff(paste("histogram_",label_name,"_",label2, ".tiff",sep=""),
       height = 1000, width = 1550)
  par(cex=2, mar=c(4.2,4.2,3,1))
  t <- tabulate(dataset4$cluster, nbins = 60)
  b <- barplot(t, ylim = c(0, (max(t)+0.1*max(t))), col = "white", 
               ylab = "Frequency (number of minutes)",
               xlab = "Cluster number")
  l <- as.character(1:60)
  a <- which(t > 0)
  axis(side = 1, at = b, labels = l)
  text(b[a], (as.numeric(t[a])+0.05*max(t)), t[a])
  text(b[58], max(t)+0.08*max(t), paste("n = ",sum(t), " minutes", sep = ""))
  text(b[31], max(t)+0.08*max(t), paste("Minutes with greater than ", n, " calls", sep = ""))
  title(main = label_name, line=1)
  dev.off()
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Species plots (far, mod, near) red, green, yellow -----------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Scarlet Honeyeater.csv", header = T)
all_data <- read.csv("all_data_added_protected.csv", header = T)[,c(1:21,37)]

# Eastern Whipbird
label_name <- "Eastern Whipbird"
list <- c("EW Far", "EW Mod", "EW Near")
list1 <- c("EW Quiet", "EW Mod", "EW Loud")
cbbPalette <- c("#000000", "#E69F00", "#56B4E9", 
                "#009E73", "#F0E442", "#0072B2", 
                "#D55E00", "#CC79A7") 
                # black, orange, lightblue, 
                # green, yellow, darkblue, 
                # burntorange and pink
#library(scales)
#show_col(cbbPalette, labels = TRUE, borders = NULL)
colours <- c("yellow","darkgreen","red")
#colours <- c("#F0E442","#56B4E9","#D55E00")
# Eastern Yellow Robin
label_name <- "Eastern Yellow Robin"
list <- c("EYR Far", "EYR Mod", "EYR Near")
list1 <- c("EYR Quiet", "EYR Mod", "EYR Loud")

# White-throated Treecreeper
label_name <- "White-throated Treecreeper"
list <- c("WTT trill Far", "WTT trill Mod", "WTT trill Near")
list1 <- c("WTT trill Quiet", "WTT trill Mod", "WTT trill Loud")

# Scarlet Honeyeater 1
label_name <- "Scarlet Honeyeater SC1"
list <- c("SC1 Far", "SC1 Mod", "SC1 Near")
list1 <- c("SC1 Quiet", "SC1 Mod", "SC1 Loud")

# Scarlet Honeyeater 2
label_name <- "Scarlet Honeyeater SC2"
list <- c("SC3 Chatter Far", "SC3 Chatter Mod", "SC3 Chatter Near")
list1 <- c("SC3 Chatter Quiet", "SC3 Chatter Mod", "SC3 Chatter Loud")
#colours <- c("grey70", "grey50", "grey0","grey20")

# White-throated Honeyeater
label_name <- "White-throated Honeyeater"
list <- c("WTH Far", "WTH Mod", "WTH Near")
list1 <- c("WTH Quiet", "WTH Mod", "WTH Loud")

# Laughing Kookaburra
label_name <- "Laughing Kookaburra"
list <- c("Kookaburra Quiet","Kookaburra Cackle", "Kookaburra Mod", "Kookaburra Loud")
list1 <- c("KOOK Quiet","KOOK Mod", "KOOK Loud")

# Torresian Crow
label_name <- "Torresian Crow"
list <- c("Torresian Crow Far","Torresian Crow Mod", "Torresian Crow Near")
list1 <- c("TOR Quiet","TOR Mod", "TOR Near")

tiff(paste("morning_chor_colours_",label_name,".tiff",sep=""),
     height = 1800, width = 2700)
kalscpe_data <- NULL
kalscpe_data_species <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  leg_label <- list1[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label, leg_label, colours[i],i)
  # Next line is only needed for species_histogram plots
  kalscpe_data_species <- rbind(kalscpe_data_species, kalscpe_data)
  par(new=T)
}
dev.off()

#colours <- c("yellow","darkgreen","red")
dev.off()
#colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_colours",label_name,".tiff",sep=""),
     height = 1800, width = 2700)
kalscpe_data <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

# Rufous Whistler
label_name <- "Rufous Whistler"
list <- c("Rufous Whistler Near")
colours <- c("red")
dev.off()
#colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_colours",label_name,".tiff",sep=""),
     height = 1800, width = 2700)
kalscpe_data <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

# Pied Currawong
label_name <- "Pied Currawong"
list <- c("Pied Currawong Far", "Pied Currawong Mod", "Pied Currawong Near")
colours <- c("yellow","darkgreen","red")
dev.off()
#colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_colours",label_name,".tiff",sep=""),
     height = 1800, width = 2700)
kalscpe_data <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

# Yellow-tailed Black Cockatoo
label_name <- "Yellow-tailed Black Cockatoo"
list <- c("Yellow-tailed Black Cockatoo Near")
colours <- c("red")
dev.off()
#colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_colours",label_name,".tiff",sep=""),
     height = 1800, width = 2700)
kalscpe_data <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

# Southern Boobook
label_name <- "Southern Boobook"
list <- c("Southern Boobook")
colours <- c("red")
dev.off()
#colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_colours",label_name,".tiff",sep=""),
     height = 1800, width = 2700)
kalscpe_data <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

# Yellow tailed Black Cockatoo
label_name <- "Yellow tailed Black Cockatoo"
list <- c("Yellow-tailed Black Cockatoo")
colours <- c("red")
dev.off()
#colours <- c("grey70", "grey50", "grey0","grey20")
tiff(paste("morning_chorus_colours",label_name,".tiff",sep=""),
     height = 1800, width = 2700)
kalscpe_data <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label_name, label,colours[i],i)
  par(new=T)
}
dev.off()

a <- grep(descriptor, data2$MANUAL.ID, ignore.case = T)
a1 <- grep(descriptor1, data2$MANUAL.ID, ignore.case = T)
a <- c(a, a1)
call_data <- data2[a,]

for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

# Tabulate dataset
t <- table(dataset$date, dataset$min)
t <- data.frame(t)
col_names <- c("date","minute","freq")
colnames(t) <- col_names
a <- which(t$freq > 0)
t <- t[a,]
t$minute <- as.numeric(levels(t$minute))[t$minute]
col_names <- colnames(indices_norm_summary)

for(i in 1:nrow(t)) {
  a <- which(t$date[i]==uniq_dates)
  minute_ref <- (a-1)*1440 + as.numeric(t$minute[i])
  t$cluster[i] <- Gym_cluster_list$cluster_list[minute_ref]
  t$BackgroundNoise[i] <- indices_norm_summary$BackgroundNoise[minute_ref]
  t$Snr[i] <- indices_norm_summary$Snr[minute_ref]
  t$Activity[i] <- indices_norm_summary$Activity[minute_ref]
  t$EventsPerSecond[i] <- indices_norm_summary$EventsPerSecond[minute_ref]
  t$AcousticComplexity[i] <- indices_norm_summary$AcousticComplexity[minute_ref]
  t$ClusterCount[i] <- indices_norm_summary$ClusterCount[minute_ref]
  t$HighFreqCover[i] <- indices_norm_summary$HighFreqCover[minute_ref]
  t$MidFreqCover[i] <- indices_norm_summary$MidFreqCover[minute_ref]
  t$LowFreqCover[i] <- indices_norm_summary$LowFreqCover[minute_ref]
  t$EntropyOfAverageSpectrum[i] <- indices_norm_summary$EntropyOfAverageSpectrum[minute_ref]
  t$EntropyOfPeaksSpectrum[i] <- indices_norm_summary$EntropyOfPeaksSpectrum[minute_ref]
  t$EntropyOfCoVSpectrum[i] <- indices_norm_summary$EntropyOfCoVSpectrum[minute_ref]
}

# Select the minimum frequency
a <- which(t$freq >= n)
t <- t[a,]

# Save cluster distribtion histogram
tiff(paste("histogram_",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t2 <- tabulate(t$cluster, nbins = 60)
b <- barplot(t2, ylim = c(0, (max(t2)+0.1*max(t2))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t2 > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t2[a])+0.05*max(t2)), t2[a])
text(b[58], max(t2)+0.08*max(t2), paste("n = ",sum(t2), " minutes", sep = ""))
text(b[31], max(t2)+0.08*max(t2), paste("Minutes with ", n, " or greater calls", sep = ""))
title(main = label_name, line=1)
dev.off()
dev.off()

# Boxplots
tiff(paste(label_name,".tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)
col_names <- colnames(t)
ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(t[(i+4)], 
               main=paste(label_name, " - ", col_names[(i+4)],sep = ""), 
               ylim=ylim, ylab = ylab)
}
dev.off()

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
a <- grep("SC and EYR", data2$MANUAL.ID, ignore.case = T)
call_data <- data2[a,]

for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

uniq_dates2 <- unique(dataset$date)
freq_table <- NULL
dataset3 <- NULL
ref <- 0
dataset3 <- data.frame(dataset3)
for(i in 1:length(uniq_dates2)) {
  a <- which(dataset$date==uniq_dates2[i])
  dataset2 <- dataset[a,]
  uniq_minutes <- unique(dataset2$minute)
  for(j in 1:length(uniq_minutes)) {
    ref <- ref + 1
    b <- which(dataset2$date==uniq_dates2[i] & dataset2$minute==uniq_minutes[j])
    if(length(b) > 0) {
      cluster <- dataset2$cluster[b[1]]  
    }
    if(length(b) == 0) {
      cluster <- "-"
    }
    frequency <- length(which(dataset2$minute==uniq_minutes[j]))
    dataset3[ref,1] <- as.character(uniq_dates2[i])
    dataset3[ref,2] <- uniq_minutes[j]
    dataset3[ref,3] <- frequency
    dataset3[ref,4] <- cluster
  }
}
col_names <- c("date","minute","freq","cluster")
colnames(dataset3) <- col_names

# Table
table(dataset3$cluster) # loud calls only

n <- 4
a <- which(dataset3$freq > n)
dataset4 <- dataset3[a,]
table(dataset4$cluster)

# Using dataset4 extract minutes that match
dates <- unique(dates)
ref1 <- length(dataset4)
dataset4[,(ref1+1):(ref1+length(indices_norm_summary))] <- 0
col_names1 <- colnames(dataset4)
col_names1 <- col_names1[1:4]
col_names <- colnames(indices_norm_summary)
col_names <- c(col_names1, col_names)
colnames(dataset4) <- col_names
for(i in 1:nrow(dataset4)) {
  a <- which(dataset4$date[i]==dates)                   
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset4[i,(ref1+1):(ref1+length(indices_norm_summary))] <- unname(indices_norm_summary[min_ref,])
}
# Boxplots
tiff(paste(label_name,"SC_and_EYR.tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)

ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(dataset4[i+4], 
               main=paste(label_name, " - ", col_names[i+4],sep = ""), ylim=ylim, ylab = ylab)
}
dev.off()

# Save cluster distribtion histogram
tiff(paste("histogram_SC_and_EYR",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t <- tabulate(dataset4$cluster, nbins = 60)
b <- barplot(t, ylim = c(0, (max(t)+0.1*max(t))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t[a])+0.05*max(t)), t[a])
text(b[58], max(t)+0.08*max(t), paste("n = ",sum(t), " minutes", sep = ""))
text(b[31], max(t)+0.08*max(t), paste("Minutes with greater than ", n, " calls", sep = ""))
title(main = label_name, line=1)
dev.off()

# White-throated Nightjar ---------------------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\Kaliedoscope\\White-throated Nightjar.csv", header = T)
label_name <- "White-throated Nightjar"
dev.off()
list <- NULL
list <- "White-throated Nightjar"
tiff(paste("morning_chorus_",label_name,".tiff",sep=""),
     height = 3600, width = 3000)

for(i in 1:length(list)) {
  label <- list[i]
  a <- which(data2$MANUAL.ID==label)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label,colours[i],i)
  par(new=T)
}
dev.off()

a <- grep("nightjar", data2$MANUAL.ID, ignore.case = T)
call_data <- data2[a,]

for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

uniq_dates2 <- unique(dataset$date)
freq_table <- NULL
dataset3 <- NULL
ref <- 0
dataset3 <- data.frame(dataset3)
for(i in 1:length(uniq_dates2)) {
  a <- which(dataset$date==uniq_dates2[i])
  dataset2 <- dataset[a,]
  uniq_minutes <- unique(dataset2$minute)
  for(j in 1:length(uniq_minutes)) {
    ref <- ref + 1
    b <- which(dataset2$date==uniq_dates2[i] & dataset2$minute==uniq_minutes[j])
    if(length(b) > 0) {
      cluster <- dataset2$cluster[b[1]]  
    }
    if(length(b) == 0) {
      cluster <- "-"
    }
    frequency <- length(which(dataset2$minute==uniq_minutes[j]))
    dataset3[ref,1] <- as.character(uniq_dates2[i])
    dataset3[ref,2] <- uniq_minutes[j]
    dataset3[ref,3] <- frequency
    dataset3[ref,4] <- cluster
  }
}
col_names <- c("date","minute","freq","cluster")
colnames(dataset3) <- col_names

# Table
table(dataset3$cluster) # loud calls only

n <- 1
a <- which(dataset3$freq > n)
dataset4 <- dataset3[a,]
table(dataset4$cluster)

# Using dataset4 extract minutes that match
dates <- unique(dates)
ref1 <- length(dataset4)
dataset4[,(ref1+1):(ref1+length(indices_norm_summary))] <- 0
col_names1 <- colnames(dataset4)
col_names1 <- col_names1[1:4]
col_names <- colnames(indices_norm_summary)
col_names <- c(col_names1, col_names)
colnames(dataset4) <- col_names
for(i in 1:nrow(dataset4)) {
  a <- which(dataset4$date[i]==dates)                   
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset4[i,(ref1+1):(ref1+length(indices_norm_summary))] <- unname(indices_norm_summary[min_ref,])
}
# Boxplots
tiff(paste(label_name,".tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)

ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(dataset4[i+4], 
               main=paste(label_name, " - ", col_names[i+4],sep = ""), ylim=ylim, ylab = ylab)
}
dev.off()

# Save cluster distribtion histogram
tiff(paste("histogram_",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t <- tabulate(dataset4$cluster, nbins = 60)
b <- barplot(t, ylim = c(0, (max(t)+0.1*max(t))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t[a])+0.05*max(t)), t[a])
text(b[58], max(t)+0.08*max(t), paste("n = ",sum(t), " minutes", sep = ""))
text(b[31], max(t)+0.08*max(t), paste("Minutes with greater than ", n, " calls", sep = ""))
title(main = label_name, line=1)
dev.off()

# Powerful Owl ---------------------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\Kaliedoscope\\Powerful Owl.csv", header = T)
label_name <- "Powerful Owl"
dev.off()
list <- NULL
list <- "Powerful Owl"
tiff(paste("morning_chorus_",label_name,".tiff",sep=""),
     height = 3600, width = 3000)

for(i in 1:length(list)) {
  label <- list[i]
  a <- which(data2$MANUAL.ID==label)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  species_plots2(label,colours[i],i)
  par(new=T)
}
dev.off()

a <- grep("Powerful", data2$MANUAL.ID, ignore.case = T)
call_data <- data2[a,]

for(i in 1:nrow(call_data)) {
  hour <- as.numeric(substr(call_data$IN.FILE[i],10,11))
  minute <- as.numeric(substr(call_data$IN.FILE[i],12,13))
  second <- as.numeric(substr(call_data$IN.FILE[i],14,15))  
  call_data$minute[i] <- 1 + floor(hour*60 + minute + (second + call_data$OFFSET[i])/60)
}

dataset <- NULL
for(i in 1:nrow(call_data)) {
  dataset$date[i] <- substr(call_data$IN.FILE[i],1,8)
  dataset$minute[i] <- call_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(i in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[i])
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset$cluster[i] <- Gym_cluster_list$cluster_list[min_ref]
}

uniq_dates2 <- unique(dataset$date)
freq_table <- NULL
dataset3 <- NULL
ref <- 0
dataset3 <- data.frame(dataset3)
for(i in 1:length(uniq_dates2)) {
  a <- which(dataset$date==uniq_dates2[i])
  dataset2 <- dataset[a,]
  uniq_minutes <- unique(dataset2$minute)
  for(j in 1:length(uniq_minutes)) {
    ref <- ref + 1
    b <- which(dataset2$date==uniq_dates2[i] & dataset2$minute==uniq_minutes[j])
    if(length(b) > 0) {
      cluster <- dataset2$cluster[b[1]]  
    }
    if(length(b) == 0) {
      cluster <- "-"
    }
    frequency <- length(which(dataset2$minute==uniq_minutes[j]))
    dataset3[ref,1] <- as.character(uniq_dates2[i])
    dataset3[ref,2] <- uniq_minutes[j]
    dataset3[ref,3] <- frequency
    dataset3[ref,4] <- cluster
  }
}
col_names <- c("date","minute","freq","cluster")
colnames(dataset3) <- col_names

# Table
table(dataset3$cluster) # loud calls only

n <- 3
a <- which(dataset3$freq > n)
dataset4 <- dataset3[a,]
table(dataset4$cluster)

# Using dataset4 extract minutes that match
dates <- unique(dates)
ref1 <- length(dataset4)
dataset4[,(ref1+1):(ref1+length(indices_norm_summary))] <- 0
col_names1 <- colnames(dataset4)
col_names1 <- col_names1[1:4]
col_names <- colnames(indices_norm_summary)
col_names <- c(col_names1, col_names)
colnames(dataset4) <- col_names
for(i in 1:nrow(dataset4)) {
  a <- which(dataset4$date[i]==dates)                   
  min_ref <- (a-1)*1440 + dataset$minute[i]
  dataset4[i,(ref1+1):(ref1+length(indices_norm_summary))] <- unname(indices_norm_summary[min_ref,])
}
# Boxplots
tiff(paste(label_name,".tiff"), width = 2000, height = 1550)
par(mfcol=c(3,4), mar=c(2,3,1,0), mgp = c(1.8, 0.5, 0),
    cex.lab=3, cex.axis=3, cex.main=3)

ylim <- c(0,1)
ylab <- "Normalised index"
for(i in 1:12) {
  b <- boxplot(dataset4[i+4], 
               main=paste(label_name, " - ", col_names[i+4],sep = ""), ylim=ylim, ylab = ylab)
}
dev.off()

# Save cluster distribtion histogram
tiff(paste("histogram_",label_name,".tiff",sep=""),
     height = 1000, width = 1550)
par(cex=2, mar=c(4.2,4.2,3,1))
t <- tabulate(dataset4$cluster, nbins = 60)
b <- barplot(t, ylim = c(0, (max(t)+0.1*max(t))), col = "white", 
             ylab = "Frequency (number of minutes)",
             xlab = "Cluster number")
l <- as.character(1:60)
a <- which(t > 0)
axis(side = 1, at = b, labels = l)
text(b[a], (as.numeric(t[a])+0.05*max(t)), t[a])
text(b[58], max(t)+0.08*max(t), paste("n = ",sum(t), " minutes", sep = ""))
text(b[31], max(t)+0.08*max(t), paste("Minutes with greater than ", n, " calls", sep = ""))
title(main = label_name, line=1)
dev.off()

# Boxplots of species/call types per cluster -------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")

indices_norm_summary <- cluster_list[,4:15]
file1 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Scarlet Honeyeater.csv", header = T)
file2 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Eastern Whipbird.csv", header = T)
file3 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\White-throated Honeyeater.csv", header = T)
file4 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Eastern Yellow Robin.csv", header = T)
file5 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Lewin's Honeyeater.csv", header = T)
file6 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Laughing Kookaburra.csv", header = T)
file7 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Torresian Crow.csv", header = T)
file8 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\White-throated Treecreeper.csv", header = T)
file9 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\SC Chatter added_protected.csv", header = T)
file10 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\SC added_protected.csv", header = T)
file11 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\EW added_protected.csv", header = T)
file12 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\WTH added_protected.csv", header = T)
file13 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\WTH Alarm added_protected.csv", header = T)
file14 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Lewins added_protected.csv", header = T)
file15 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\WTT piping added_protected.csv", header = T)

# check that all files have the rows in a consistent order
list <- 1:15
check <- NULL
for(j in 1:(length(list)-1)) {
  file_a <- get(paste("file", list[j],sep = ""))
  file_b <- get(paste("file", list[j+1],sep = ""))
  a <- NULL
  for(i in 1:nrow(file_a)) {
    ifelse((abs(file_a[i,4] - file_b[i,4]) < 1), a1 <- "YES", a1 <- "NO")
    a <- c(a, a1)
  }
  b <- which(a=="NO")
  check <- c(check, length(b))
  if(length(b)  > 0) {
    print("WARNING: files do not have consisent order of rows")
    break
  }
  print(paste("Finished checking file", (j+1)))
}
check <- as.numeric(check)
check
if(sum(check) == 0) {
  print("All files are consistent")
}
if(sum(check) >=  1) {
  print("Your files are not consistent")
}

all_data <- cbind(file1, 
                  file2[,length(file2)], file3[,length(file3)],
                  file4[,length(file4)], file5[,length(file5)],
                  file6[,length(file6)], file7[,length(file7)],
                  file8[,length(file8)], file9[,length(file9)],
                  file10[,length(file10)], file11[,length(file11)],
                  file12[,length(file12)], file13[,length(file13)],
                  file14[,length(file14)], file15[,length(file15)])

col_names <- colnames(all_data)
col_names <- c(col_names[1:21],
               "Scarlet Honeyeater",
               "Eastern whipbird",
               "White-throated Honeyeater",
               "Eastern Yellow Robin",
               "Lewin's Honeyeater",
               "Laughing Kookaburra",
               "Torresian Crow",
               "White-throated Treecreeper",
               "SC Chatter added",
               "SC added",
               "EW added",
               "WTH added",
               "WTH Alarm added",
               "Lewins added",
               "WTT piping added")
colnames(all_data) <- col_names

for(i in 1:nrow(all_data)) {
  all_data$MANUAL_ID[i] <- paste(" ", # necessary space to avoid a formula being inserted in csv
                              all_data$`Scarlet Honeyeater`[i],
                              all_data$`Eastern whipbird`[i],
                              all_data$`White-throated Honeyeater`[i],
                              all_data$`Eastern Yellow Robin`[i],
                              all_data$`Lewin's Honeyeater`[i],
                              all_data$`Laughing Kookaburra`[i],
                              all_data$`Torresian Crow`[i],
                              all_data$`White-throated Treecreeper`[i],
                              all_data$`SC Chatter added`[i],
                              all_data$`SC added`[i],
                              all_data$`EW added`[i],
                              all_data$`WTH added`[i],
                              all_data$`WTH Alarm added`[i],
                              all_data$`Lewins added`[i],
                              all_data$`WTT piping added`[i],
                              sep = " ")
  print(i)  
}

#write.csv(all_data, "all_data.csv", row.names = F)

all_data <- read.csv("all_data_added_protected.csv", header = T)[,c(1:21,37)]

for(j in 1:nrow(all_data)) {
  hour <- as.numeric(substr(all_data$IN.FILE[j],10,11))
  minute <- as.numeric(substr(all_data$IN.FILE[j],12,13))
  second <- as.numeric(substr(all_data$IN.FILE[j],14,15))  
  all_data$minute[j] <- 1 + floor(hour*60 + minute + (second + all_data$OFFSET[j] + (all_data$DURATION[j]/3))/60)
}

# June 2015 (9 days)
#a <- which(substr(all_data$IN.FILE,1,6)=="201506")
#hist(all_data$minute[a], breaks = 406, xlim=c(300,406), ylim = c(0,720))
# July 2015 (31 days)
#a <- which(substr(all_data$IN.FILE,1,6)=="201507")
#hist(all_data$minute[a], breaks = 406, xlim=c(300,406), ylim = c(0,720))
# August 2015 (16 days)
#a <- which(substr(all_data$IN.FILE,1,6)=="201508")
#hist(all_data$minute[a], breaks = 406, xlim=c(300,406), ylim = c(0,720))

# add the cluster number for each minute in dataset
dataset <- NULL
for(i in 1:nrow(all_data)) {
  dataset$date[i] <- substr(all_data$IN.FILE[i],1,8)
  dataset$minute[i] <- all_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
# for each entry in the kaleidoscope dataset assign the cluster number 
for(k in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[k])
  min_ref <- (a-1)*1440 + dataset$minute[k]
  dataset$cluster[k] <- Gym_cluster_list$cluster_list[min_ref]
  dataset$BGN[k] <- cluster_list$BackgroundNoise[min_ref]
  dataset$SNR[k] <- cluster_list$Snr[min_ref]
  dataset$ACT[k] <- cluster_list$Activity[min_ref]
  dataset$EventsPerSec[k] <- cluster_list$EventsPerSecond[min_ref]
  dataset$HFC[k] <- cluster_list$HighFreqCover[min_ref]
  dataset$MFC[k] <- cluster_list$MidFreqCover[min_ref]
  dataset$LFC[k] <- cluster_list$LowFreqCover[min_ref]
  dataset$ACI[k] <- cluster_list$AcousticComplexity[min_ref]
  dataset$EAS[k] <- cluster_list$EntropyOfAverageSpectrum[min_ref]
  dataset$EPS[k] <- cluster_list$EntropyOfPeaksSpectrum[min_ref]
  dataset$ECV[k] <- cluster_list$EntropyOfCoVSpectrum[min_ref]
  dataset$CLC[k] <- cluster_list$ClusterCount[min_ref]
}

all_data$cluster <- dataset$cluster
all_data$BGN <- dataset$BGN
all_data$SNR <- dataset$SNR
all_data$ACT <- dataset$ACT
all_data$EventsPerSec <- dataset$EventsPerSec
all_data$HFC <- dataset$HFC
all_data$MFC <- dataset$MFC
all_data$LFC <- dataset$LFC
all_data$ACI <- dataset$ACI
all_data$EAS <- dataset$EAS
all_data$EPS <- dataset$EPS
all_data$ECV <- dataset$ECV
all_data$CLC <- dataset$CLC

list1 <- 22
list <- c("SC1 Far",
          "SC1 Mod",
          "SC1 Near",
          "SC3 Chatter Far",
          "SC3 Chatter Mod",
          "SC3 Chatter Near",
          "EW Far",
          "EW Mod",
          "EW Near",
          "WTH Far",
          "WTH Mod",
          "WTH Near",
          "EYR Far",
          "EYR Mod",
          "EYR Near",
          "Lewins Far",
          "Lewins Mod",
          "Lewins Near",
          "Kookaburra Quiet",
          "Kookaburra Mod",
          "Kookaburra Loud",
          "Kookaburra Cackle",
          "WTT trill Far",
          "WTT trill Mod",
          "WTT trill Near",
          "WTT piping Far",
          "WTT piping Mod",
          "WTT piping Near",
          "Torresian Crow Far",
          "Torresian Crow Mod",
          "Torresian Crow Near",
          "Pied Currawong Far",
          "Pied Currawong Mod",
          "Pied Currawong Near",
          "Rainbow Lorikeet Mod",
          "Rainbow Lorikeet Near",
          "Rufous Whistler Near",
          "Golden Whistler Near",
          "Yellow-tailed Black Cockatoo Near",
          "Southern Boobook Far",
          "Southern Boobook Mod",
          "Powerful Owl",
          "Fairywren",
          "White-throated Nightjar",
          "Animal movement",
          "Rain")  #"WTT trill fast"

statistics <- data.frame(SC1_Far=NA,              #1
                         SC1_Mod=NA,              #2
                         SC1_Near=NA,             #3
                         SC3_Chatter_Far=NA,      #7
                         SC3_Chatter_Mod=NA,      #8
                         SC3_Chatter_Near=NA,     #9
                         EW_Far=NA,               #10
                         EW_Mod=NA,               #11
                         EW_Near=NA,              #12
                         WTH_Far=NA,              #13
                         WTH_Mod=NA,              #14
                         WTH_Near=NA,             #15
                         EYR_Far=NA,              #16
                         EYR_Mod=NA,              #17
                         EYR_Near=NA,             #18
                         Lewins_Far=NA,           #19
                         Lewins_Mod=NA,           #20
                         Lewins_Near=NA,          #21
                         Kookaburra_Quiet=NA,     #22
                         Kookaburra_Mod=NA,       #23
                         Kookaburra_Loud=NA,      #24
                         Kookaburra_Cackle=NA,    #25
                         WTT_trill_Far=NA,        #26
                         WTT_trill_Mod=NA,        #27
                         WTT_trill_Near=NA,       #28
                         WTT_piping_Far=NA,       #29
                         WTT_piping_Mod=NA,       #30
                         WTT_piping_Near=NA,      #31
                         Torresian_Crow_Far=NA,   #32
                         Torresian_Crow_Mod=NA,   #33
                         Torresian_Crow_Near=NA,  #34
                         Pied_Currawong_Far=NA,   #35
                         Pied_Currawong_Mod=NA,   #36
                         Pied_Currawong_Near=NA,  #37
                         Rainbow_Lorikeet_Mod=NA,                 #38
                         Rainbow_Lorikeet_Near=NA,                #39
                         Rufous_Whistler_Near=NA,                 #40
                         Golden_Whistler_Near=NA,                 #41
                         Yellow_tailed_Black_Cockatoo_Near=NA,    #42
                         Southern_Boobook_Far=NA,                 #43
                         Southern_Boobook_Mod=NA,                 #44
                         Powerful_Owl=NA,                         #45
                         Fairywren=NA,                            #46
                         White_throated_Nightjar=NA,              #47
                         Animal_movement=NA,                      #48
                         Rain=NA)                                 #49

all_data <- read.csv("all_data_added_protected.csv", header = T)[,c(1:21,37)]

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
# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)
site1 <- rep("GympieNP", nrow(cluster_list)/2)
site2 <- rep("WoondumNP", nrow(cluster_list)/2)
site <- c(site1, site2)

dates <- date.list
rm(date.list)
# duplicate dates 1440 times
dates <- rep(dates, each = 1440)
dates <- rep(dates, 2)
# Add site and dates columns to dataframe
cluster_list <- cbind(cluster_list, site, dates)
cluster_list <- cluster_list[1:573120,]
col_names <- colnames(cluster_list)

for(i in 5:(length(list)+4)) {
  cluster_list[,i] <- 0  
}

col_names <- c(col_names, list)
colnames(cluster_list) <- col_names
all_data$date <- substr(all_data$IN.FILE, 1, 8)
all_data$min <- (all_data$OFFSET + 3600*as.numeric(substr(all_data$IN.FILE,10,11)) 
                 + 60*as.numeric(substr(all_data$IN.FILE,12,13)) 
                 + as.numeric(substr(all_data$IN.FILE,14,15)))/60
all_data$min1 <- floor(all_data$min)

# This code counts the number of far, mod and near calls for each species in each minute
for(i in 1:length(list)) {
  label <- list[i]
  column <- i + 4
  cluster_list1[ ,column] <- 0
  a <- NULL
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  all_data_temp <- all_data[a,]
  #for(j in 1:nrow(all_data_temp)) {
  for(j in 1:nrow(cluster_list1)) {
    date <- cluster_list1$dates[j]
    minute <- cluster_list1$minute_reference[j]
    a <- NULL
    a <- which(all_data_temp$date==date)
    b <- NULL
    b <- which(all_data_temp$min1==minute)
    # a <- NULL
    #  a <- which(cluster_list$dates == all_data_temp$date[j])
    # b <- NULL
    #  b <- which(cluster_list$minute_reference==all_data_temp$min1[j])
    c <- NULL
    c <- intersect(a, b)
    cluster_list1[j, column] <- length(c)
  }
  print(i) # i is the species list (49)
}

#write.csv(cluster_list, "species_each_minute.csv", row.names = F)

all_data <- cbind(all_data, dataset)
for(i in 1:60) {
  a <- which(all_data$cluster==i)
  data_temp <- NULL
  data_temp <- all_data[a,]
  for(j in 1:length(list)) {
    a <- grep(list[j], data_temp$MANUAL_ID, ignore.case = T)
    a <- length(a)
    statistics[i,j] <- a
  }
}

statistics$cluster <- c(1:60)
statistics <- statistics[,c(length(statistics),1:(length(statistics)-1))]

#write.csv(statistics, "bird_cluster_statistics.csv", row.names = F)

# find the median maximum frequency for each species in the list
medians <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  medians$class[i] <- list[i]
  medians$max_freq_median[i] <- signif(median(all_data$Fmax[a]),2)
}

# find the median duration for each species in the list
duration <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  duration$class[i] <- list[i]
  duration$duration[i] <- signif(median(all_data$DURATION[a]),2)
}

# find the median BGN for each species in the list
BGN <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  BGN$class[i] <- list[i]
  BGN$med_BGN[i] <- signif(median(all_data$BGN[a]),2)
}

# find the median SNR for each species in the list
SNR <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  SNR$class[i] <- list[i]
  SNR$med_SNR[i] <- signif(median(all_data$SNR[a]),2)
}

# find the median ACT for each species in the list
ACT <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  ACT$class[i] <- list[i]
  ACT$med_ACT[i] <- signif(median(all_data$ACT[a]),2)
}

# find the median EventsPerSec for each species in the list
EVN <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  EVN$class[i] <- list[i]
  EVN$med_EVN[i] <- signif(median(all_data$EventsPerSec[a]),2)
}

# find the median HFC for each species in the list
HFC <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  HFC$class[i] <- list[i]
  HFC$med_HFC[i] <- signif(median(all_data$HFC[a]),2)
}

# find the median MFC for each species in the list
MFC <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  MFC$class[i] <- list[i]
  MFC$med_MFC[i] <- signif(median(all_data$MFC[a]),2)
}

# find the median MFC for each species in the list
LFC <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  LFC$class[i] <- list[i]
  LFC$med_LFC[i] <- signif(median(all_data$LFC[a]),2)
}

# find the median ACI for each species in the list
ACI <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  ACI$class[i] <- list[i]
  ACI$med_ACI[i] <- signif(median(all_data$ACI[a]),2)
}

# find the median EAS for each species in the list
EAS <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  EAS$class[i] <- list[i]
  EAS$med_EAS[i] <- signif(median(all_data$EAS[a]),2)
}

# find the median EPS for each species in the list
EPS <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  EPS$class[i] <- list[i]
  EPS$med_EPS[i] <- signif(median(all_data$EPS[a]),2)
}

# find the median EPS for each species in the list
CLC <- NULL
for(i in 1:length(list)) {
  a <- grep(list[i], all_data$MANUAL_ID, ignore.case = T)
  CLC$class[i] <- list[i]
  CLC$med_CLC[i] <- signif(median(all_data$CLC[a]),2)
}

duration$max_freq_median <- medians$max_freq_median
duration$BGN <- BGN$med_BGN
duration$SNR <- SNR$med_SNR
duration$ACT <- ACT$med_ACT
duration$EVN <- EVN$med_EVN
duration$HFC <- HFC$med_HFC
duration$MFC <- MFC$med_MFC
duration$LFC <- LFC$med_LFC
duration$ACI <- ACI$med_ACI
duration$EAS <- EAS$med_EAS
duration$EPS <- EPS$med_EPS
duration$CLC <- CLC$med_CLC
View(duration)

write.csv(duration, "medians_of_acoustic_indices.csv", row.names = F)

ylim_list <- c(12,12,12,10,10,10,
               8,8,8,8,8,8,22,22,22,
               40,40,40,5,5,5,8,8,8,8,
               11,11,11,4,4,4)


# find the number of minutes each species is calling
num2 <- NULL
for(i in 1:length(list)) {
  a <- NULL
  for(j in 22:28) {
    a1 <- which(all_data[,j]==list[i])
    a <- c(a, a1)
  }
  list_n <- all_data[a,]
  list_n <- data.frame(list_n)
  # table t1 gives the frequency for each date and minute
  t2 <- table(list_n$IN.FILE, list_n$minute)
  t2 <- data.frame(t2)
  a <- NULL
  a <- which(t2$Freq > 0)
  nums <- length(a)
  num2 <- c(num2, nums)
}
num2 #number of minutes each species is calling

# find the number of each call of each species
num3 <- NULL
for(i in 1:length(list)) {
  a <- NULL
  for(j in 22:28) {
    a1 <- which(all_data[,j]==list[i])
    a <- c(a, a1)
  }
  list_n <- all_data[a,]
  list_n <- data.frame(list_n)
  nums <- nrow(list_n)
  num3 <- c(num3, nums)
}
num3 #number of each call of each species

#Cluster 15
dev.off()
clust_num <-c(3,11,14,15,33,37,39,43,58)
statistics <- data.frame(cluster = NA, species = NA, l_whisker = NA,
                         l_hinge = NA, median = NA, u_hinge = NA,
                         u_whisker = NA, max = NA, average = NA,
                         n_min = NA, n_clust = NA, percent = NA,
                         percent2 = NA)#
for(n in clust_num) {
  a <- which(all_data$cluster==n)
  cluster_n <- all_data[a,]
  cluster_n <- data.frame(cluster_n)
  # table t1 gives the frequency for each date and minute
  t1 <- table(cluster_n$IN.FILE, cluster_n$minute)
  t1 <- data.frame(t1)
  a <- which(t1$Freq > 0)
  num1 <- length(a)
  columns <- c(22:28)
  tiff(paste("cluster", n,"_boxplots.tiff",sep = ""), height = 2000, width = 2000)
  par(mfrow=c(6,6), mar=c(2,2,1,1), cex=1.6, oma=c(0,0,3,0))
  #for(i in columns) {
  #list  <- unique(cluster_n[,i])
  #a <- which(list=="-")
  #if(length(a) > 0) {
  #  list <- list[-a]
  #}
  #a <- which(list=="")
  #if(length(a) > 0) {
  #  list <- list[-a]
  #}
  for(j in 1:length(list)) {
    a <- NULL
    for(i in 22:28) {
      a1 <- which(cluster_n[,i]==list[j])
      a <- c(a, a1)
    }
    if(length(a) > 0) {
      cluster_n_temp <- cluster_n[a,c(1:21,i,29)]
      cluster_n_temp <- data.frame(cluster_n_temp)
      t <- table(cluster_n_temp$IN.FILE, cluster_n_temp$minute)
      t <- data.frame(t)
      days <- unique(cluster_n_temp$IN.FILE)
      a <- which(t$Freq > 0)
      b <- boxplot(t$Freq[a], main=list[j], ylim = c(0, (ylim_list[j]+0.05*ylim_list[j])) )
      num <- length(a)
      #boxplot(t$Freq, main=list[j])
      text(x = 1, y = (ylim_list[j]), paste("num = ", num, sep = ""), cex = 1.6)  #max(t$Freq)-0.05*max(t$Freq), 
      stat <- c(n, list[j], b$stats[1], b$stats[2], b$stats[3], b$stats[4],
                b$stats[5], max(t$Freq[a]), mean(t$Freq[a]),
                length(t$Freq[a]), num1, (100*length(t$Freq[a])/num1),
                (100*length(t$Freq[a])/num2[j]))
    }
    mtext(side = 3, paste("Cluster ", n, " (", num1, ")", sep = ""),
          outer = T, cex = 5, line = 1)
    if(length(a) == 0) {
      b <- boxplot(0, main=list[j]) #ylim = c(0, (ylim_list[j]+0.05*ylim_list[j])))
      stat <- c(n,list[j],0,0,0,0,0,0,0,0,0,0,0)
    }
    statistics <- rbind(statistics, stat)
  }
  dev.off()
}
statistics$percent <- as.numeric(statistics$percent)
statistics$percent2 <- as.numeric(statistics$percent2)
a <- complete.cases(statistics)
statistics <- statistics[a,]
rownames(statistics) <- 1:nrow(statistics)
View(statistics)
a <- NULL
a <- complete.cases(statistics)
statistics <- statistics[a,]
rownames(statistics) <- 1:nrow(statistics)
View(statistics)

# boxplot of offset statistics for each cluster
tiff("boxplots of durations.tiff", height = 500, width = 2000)
par(mfrow=c(1,9), mar=c(1,0,2,0), 
    cex.axis=1.8, cex.main=1.4, oma=c(0,4,0,1))

for(n in clust_num) {
  a <- which(all_data$cluster==n)
  cluster_n <- all_data[a,]
  cluster_n <- data.frame(cluster_n)
  a <- which((cluster_n$Scarlet.Honeyeater=="-") &
               (cluster_n$Eastern.whipbird=="-") &
               (cluster_n$White.throated.Honeyeater=="-") &
               (cluster_n$Eastern.Yellow.Robin=="-") & 
               (cluster_n$Lewin.s.Honeyeater=="-") &
               (cluster_n$Laughing.Kookaburra=="-") &
               (cluster_n$White.throated.Treecreeper=="-"))
  cluster_n <- cluster_n[-a,]
  a1 <- NULL
  for(i in 1:length(list)) {
    a <- length(which(cluster_n[,22:28]==list[i]))
    a1 <- c(a1, a)
  }
  a1 <- sum(a1)
  t <- table(cluster_n$IN.FILE, cluster_n$minute)
  t <- data.frame(t)
  a <- which(t$Freq > 0)
  num <- length(a)
  if(n==clust_num[1]) {
    b <- boxplot(cluster_n$DURATION, ylim=c(0,8), plot=0)
    boxplot(cluster_n$DURATION, ylim=c(0,8), las=1)
    mtext(side = 3, line = -2, paste("num calls = ", a1, " (", num, ")"), cex = 1.6)
    mtext(side = 3, paste("Cluster", n, sep = ""), cex = 1.6)
    mtext(side = 2, line = 2, paste("Duration (seconds)"), 
          cex = 1.6)
    abline(h=mean(cluster_n$DURATION), col="red")
  }
  if(n %in% clust_num[2:length(clust_num)]) {
    b <- boxplot(cluster_n$DURATION, ylim=c(0,8), plot = 0)
    boxplot(cluster_n$DURATION, ylim=c(0,8),
            ylab=("Duration (seconds)"), las=1)
    mtext(side = 3, line = -2, paste("num calls = ", a1, " (", num, ")"), cex = 1.6)
    mtext(side = 3,paste("Cluster", n, sep = ""), cex = 1.6)
    abline(h=mean(cluster_n$DURATION), col="red")
  }
}
dev.off()
#3 11 14 15 33 37 39 43 58
stats1 <- NULL
stats1$species <- statistics$species[1:length(list)]
stats1$cst3_avg  <- round(as.numeric(statistics$average[(length(list)*0+1):(length(list)*1)]), 1)
stats1$cst11_avg <- round(as.numeric(statistics$average[(length(list)*1+1):(length(list)*2)]), 1)
stats1$cst14_avg <- round(as.numeric(statistics$average[(length(list)*2+1):(length(list)*3)]), 1)
stats1$cst15_avg <- round(as.numeric(statistics$average[(length(list)*3+1):(length(list)*4)]), 1)
stats1$cst33_avg <- round(as.numeric(statistics$average[(length(list)*4+1):(length(list)*5)]), 1)
stats1$cst37_avg <- round(as.numeric(statistics$average[(length(list)*5+1):(length(list)*6)]), 1)
stats1$cst39_avg <- round(as.numeric(statistics$average[(length(list)*6+1):(length(list)*7)]), 1)
stats1$cst43_avg <- round(as.numeric(statistics$average[(length(list)*7+1):(length(list)*8)]), 1)
stats1$cst58_avg <- round(as.numeric(statistics$average[(length(list)*8+1):(length(list)*9)]), 1)

stats1$max3_perc <- round(as.numeric(statistics$max[(length(list)*0+1):(length(list)*1)]), 1)
stats1$max11_perc <- round(as.numeric(statistics$max[(length(list)*1+1):(length(list)*2)]), 1)
stats1$max14_perc <- round(as.numeric(statistics$max[(length(list)*2+1):(length(list)*3)]), 1)
stats1$max15_perc <- round(as.numeric(statistics$max[(length(list)*3+1):(length(list)*4)]), 1)
stats1$max33_perc <- round(as.numeric(statistics$max[(length(list)*4+1):(length(list)*5)]), 1)
stats1$max37_perc <- round(as.numeric(statistics$max[(length(list)*5+1):(length(list)*6)]), 1)
stats1$max39_perc <- round(as.numeric(statistics$max[(length(list)*6+1):(length(list)*7)]), 1)
stats1$max43_perc <- round(as.numeric(statistics$max[(length(list)*7+1):(length(list)*8)]), 1)
stats1$max58_perc <- round(as.numeric(statistics$max[(length(list)*8+1):(length(list)*9)]), 1)

stats1$cst3_perc <- round(as.numeric(statistics$percent[(length(list)*0+1):(length(list)*1)]), 1)
stats1$cst11_perc <- round(as.numeric(statistics$percent[(length(list)*1+1):(length(list)*2)]), 1)
stats1$cst14_perc <- round(as.numeric(statistics$percent[(length(list)*2+1):(length(list)*3)]), 1)
stats1$cst15_perc <- round(as.numeric(statistics$percent[(length(list)*3+1):(length(list)*4)]), 1)
stats1$cst33_perc <- round(as.numeric(statistics$percent[(length(list)*4+1):(length(list)*5)]), 1)
stats1$cst37_perc <- round(as.numeric(statistics$percent[(length(list)*5+1):(length(list)*6)]), 1)
stats1$cst39_perc <- round(as.numeric(statistics$percent[(length(list)*6+1):(length(list)*7)]), 1)
stats1$cst43_perc <- round(as.numeric(statistics$percent[(length(list)*7+1):(length(list)*8)]), 1)
stats1$cst58_perc <- round(as.numeric(statistics$percent[(length(list)*8+1):(length(list)*9)]), 1)

stats1$cst3_perc2 <- round(as.numeric(statistics$percent2[(length(list)*0+1):(length(list)*1)]), 1)
stats1$cst11_perc2 <- round(as.numeric(statistics$percent2[(length(list)*1+1):(length(list)*2)]), 1)
stats1$cst14_perc2 <- round(as.numeric(statistics$percent2[(length(list)*2+1):(length(list)*3)]), 1)
stats1$cst15_perc2 <- round(as.numeric(statistics$percent2[(length(list)*3+1):(length(list)*4)]), 1)
stats1$cst33_perc2 <- round(as.numeric(statistics$percent2[(length(list)*4+1):(length(list)*5)]), 1)
stats1$cst37_perc2 <- round(as.numeric(statistics$percent2[(length(list)*5+1):(length(list)*6)]), 1)
stats1$cst39_perc2 <- round(as.numeric(statistics$percent2[(length(list)*6+1):(length(list)*7)]), 1)
stats1$cst43_perc2 <- round(as.numeric(statistics$percent2[(length(list)*7+1):(length(list)*8)]), 1)
stats1$cst58_perc2 <- round(as.numeric(statistics$percent2[(length(list)*8+1):(length(list)*9)]), 1)
View(stats1)

write.csv(stats1, "bird_statistics_final_1.csv", row.names = F)
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Species class in periods A, B and C ---------------------
# prepare a csv containing the number of each species class are 
# in periods A, B and C
rm(list = ls())

source("scripts/Bird_lists_func.R")
dates <- unique(dates)
dates <- dates[1:56]
dates <- data.frame(dates)

# prepare the civil-dawn times
dates$civil_dawn <- NULL
# Dataframe of date and minute of civil dawn
for(i in 1:nrow(dates)) {
  date <- paste(substr(dates$dates[i],1,4), "-", substr(dates$dates[i], 5,6),
                "-", substr(dates$dates[i], 7,8), sep = "")
  a <- which(civil_dawn$dates==date)
  dates$civil_dawn[i] <- civil_dawn$CivSunrise[a]
}

for(i in 1:nrow(dates)) {
  dates$civil_dawn_min[i] <- as.numeric(substr(dates$civil_dawn[i], 1,1))*60 + as.numeric(substr(dates$civil_dawn[i], 2,3))
  dates$A1[i] <- dates$civil_dawn_min[i] - 24
  dates$A2[i] <- dates$civil_dawn_min[i] - 15
  dates$B1[i] <- dates$civil_dawn_min[i] - 4
  dates$B2[i] <- dates$civil_dawn_min[i] + 5
  dates$C1[i] <- dates$civil_dawn_min[i] + 16
  dates$C2[i] <- dates$civil_dawn_min[i] + 25
}

file1 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Scarlet Honeyeater.csv", header = T)
file2 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Eastern Whipbird.csv", header = T)
file3 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\White-throated Honeyeater.csv", header = T)
file4 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Eastern Yellow Robin.csv", header = T)
file5 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Lewin's Honeyeater.csv", header = T)
file6 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Laughing Kookaburra.csv", header = T)
file7 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\White-throated Treecreeper.csv", header = T)
file8 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\SC Chatter added_protected.csv", header = T)
file9 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\SC added_protected.csv", header = T)
file10 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\EW added_protected.csv", header = T)
file11 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\WTH added_protected.csv", header = T)
file12 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\WTH Alarm added_protected.csv", header = T)
file13 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\EYR added_protected.csv", header = T)
file14 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Lewins added_protected.csv", header = T)
file15 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Kookaburra quiet added_protected.csv", header = T)
file16 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\WTT trill added_protected.csv", header = T)
file17 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\WTT piping added_protected.csv", header = T)


all_data <- cbind(file1, 
                  file2[length(file2)], file3[length(file3)],
                  file4[length(file4)], file5[length(file5)],
                  file6[length(file6)], file7[length(file7)],
                  file8[length(file8)], file9[,length(file9)],
                  file10[length(file10)], file11[length(file11)],
                  file12[length(file12)], file13[length(file13)],
                  file14[length(file14)], file15[length(file15)],
                  file16[length(file16)], file17[length(file17)])

col_names <- colnames(all_data)
col_names <- c(col_names[1:21],
               "Scarlet Honeyeater",
               "Eastern whipbird",
               "White-throated Honeyeater",
               "Eastern Yellow Robin",
               "Lewin's Honeyeater",
               "Laughing Kookaburra",
               "White-throated Treecreeper",
               "SC Chatter added",
               "SC added",
               "EW added",
               "WTH added",
               "WTH Alarm added",
               "EYR added",
               "Lewins added",
               "Kookaburra added",
               "WTT trill added",
               "WTT piping added")
colnames(all_data) <- col_names

for(j in 1:nrow(all_data)) {
  hour <- as.numeric(substr(all_data$IN.FILE[j],10,11))
  minute <- as.numeric(substr(all_data$IN.FILE[j],12,13))
  second <- as.numeric(substr(all_data$IN.FILE[j],14,15))  
  all_data$minute[j] <- 1 + floor(hour*60 + minute + (second + all_data$OFFSET[j])/60)
}

# June 2015 (9 days)
#a <- which(substr(all_data$IN.FILE,1,6)=="201506")
#hist(all_data$minute[a], breaks = 406, xlim=c(300,406), ylim = c(0,720))
# July 2015 (31 days)
#a <- which(substr(all_data$IN.FILE,1,6)=="201507")
#hist(all_data$minute[a], breaks = 406, xlim=c(300,406), ylim = c(0,720))
# August 2015 (16 days)
#a <- which(substr(all_data$IN.FILE,1,6)=="201508")
#hist(all_data$minute[a], breaks = 406, xlim=c(300,406), ylim = c(0,720))
for(i in 1:nrow(all_data)) {
  all_data$all_birds[i] <-  paste(all_data$`Scarlet Honeyeater`[i],
                                  all_data$`Eastern whipbird`[i],
                                  all_data$`White-throated Honeyeater`[i],
                                  all_data$`Eastern Yellow Robin`[i],
                                  all_data$`Lewin's Honeyeater`[i],
                                  all_data$`Laughing Kookaburra`[i],
                                  all_data$`White-throated Treecreeper`[i],
                                  all_data$`SC Chatter added`[i],
                                  all_data$`SC added`[i],
                                  all_data$`EW added`[i],
                                  all_data$`WTH added`[i],
                                  all_data$`WTH Alarm added`[i],
                                  all_data$`EYR added`[i],
                                  all_data$`Lewins added`[i],
                                  all_data$`Kookaburra added`[i],
                                  all_data$`WTT trill added`[i],
                                  all_data$`WTT piping added`[i],
                                  sep = " ") 
}
#write.csv(all_data, "all_data1.csv", row.names = F)

list <- c("EYR added",
          "SC added",
          "EW added",
          "WTH added",
          "WTT trill added",
          "WTT piping added",
          "SC Chatter added",
          "WTH Alarm added",
          "Lewins added",
          "Kookaburra quiet added")

#all_data <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\all_data_protected.csv", header = T)
all_data <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\all_data_added_protected.csv", header = T)
all_data <- all_data[,c(1:21,39)]

#for (i in 1:length(list)) {
#  a <- grep(list[i], all_data$MANUAL.ID, ignore.case = T)
#  species_data <- all_data
#  species_data$MANUAL.ID <- "-"
#  species_data$MANUAL.ID[a] <- list[i]
#  write.csv(species_data, paste(list[i],".csv", sep = ""), row.names = F)
#}

dataset <- NULL
for(i in 1:nrow(all_data)) {
  dataset$date[i] <- substr(all_data$IN.FILE[i],1,8)
  dataset$minute[i] <- all_data$minute[i]
}
uniq_dates  <- unique(dates)
# add cluster to date and minute in dataset
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(k in 1:nrow(dataset)) {
  a <- which(uniq_dates$dates==dataset$date[k])
  min_ref <- (a-1)*1440 + dataset$minute[k]
  dataset$cluster[k] <- Gym_cluster_list$cluster_list[min_ref]
}


list <- c("SC Far",
          "SC Mod",
          "SC Near",
          "SC Single note Far",
          "SC Single note Mod",
          "SC Single note Near",
          "SC Chatter Far",
          "SC Chatter Mod",
          "SC Chatter Near",
          "EW Far",
          "EW Mod",
          "EW Near",
          "WTH Far",
          "WTH Mod",
          "WTH Near",
          "EYR Far",
          "EYR Mod",
          "EYR Near",
          "Lewins Far",
          "Lewins Mod",
          "Lewins Near",
          "Kookaburra Quiet",
          "Kookaburra Mod",
          "Kookaburra Loud",
          "Kookaburra Cackle",
          "WTT trill Far",
          "WTT trill Mod",
          "WTT trill Near",
          "WTT piping Far",
          "WTT piping Mod",
          "WTT piping Near")  #"WTT trill fast"

# Produce an empty dataframe
bird_dataframe <- data.frame(date="19700101", Species_class="",
  A1=0, A2=0, A3=0, A4=0, A5=0, A6=0, A7=0, A8=0, A9=0, A10=0,
                             B1=0, B2=0, B3=0, B4=0, B5=0, B6=0, B7=0, B8=0, B9=0, B10=0,
                             C1=0, C2=0, C3=0, C4=0, C5=0, C6=0, C7=0, C8=0, C9=0, C10=0)
bird_dataframe[,c(1:2)] <- as.character(bird_dataframe[,c(1:2)])
dates <- data.frame(dates)

# Produce a dataframe containing the number of calls of each
# species in each period A, B and C
for(i in 1:nrow(dates)) {
  for(j in 1:length(list)) {
    label <- list[j]
    bird_dataframe[(length(list)*(i-1)+j),1] <- as.character(dates[i,1])
    bird_dataframe[(length(list)*(i-1)+j),2] <- list[j]
    a <- which(substr(all_data$IN.FILE, 1,8)==dates$dates[i])
    all_data_date <- all_data[a,]
    a1 <- NULL
    a <- grep(label, all_data_date$`Scarlet Honeyeater`)
    if(length(a) > 0) {
      a1 <- c(a1, a)  
    }
    a <- grep(label, all_data_date$`Eastern whipbird`)
    if(length(a) > 0) {
      a1 <- c(a1, a)  
    }
    a <- grep(label, all_data_date$`White-throated Honeyeater`)
    if(length(a) > 0) {
      a1 <- c(a1, a)  
    }
    a <- grep(label, all_data_date$`Eastern Yellow Robin`)
    if(length(a) > 0) {
      a1 <- c(a1, a)  
    }
    a <- grep(label, all_data_date$`Lewin's Honeyeater`)
    if(length(a) > 0) {
      a1 <- c(a1, a)  
    }
    a <- grep(label, all_data_date$`Laughing Kookaburra`)
    if(length(a) > 0) {
      a1 <- c(a1, a)  
    }
    a <- grep(label, all_data_date$`White-throated Treecreeper`)
    if(length(a) > 0) {
      a1 <- c(a1, a)  
    }
    all_data_date <- all_data_date[a1,]
    i <- as.numeric(i)
    minutes1 <- dates[i,4]:dates[i,5]
    minutes2 <- dates[i,6]:dates[i,7]
    minutes3 <- dates[i,8]:dates[i,9]
    for(k in minutes1) {
    b <- which(minutes1==k)
    a <- which(all_data_date$minute==k)
      bird_dataframe[(length(list)*(i-1)+j),(b+2)] <- length(a)
    }
    for(k in minutes2) {
      b <- which(minutes2==k)
      a <- which(all_data_date$minute==k)
      bird_dataframe[(length(list)*(i-1)+j),(b+12)] <- length(a)
    }
    for(k in minutes3) {
      b <- which(minutes3==k)
      a <- which(all_data_date$minute==k)
      bird_dataframe[(length(list)*(i-1)+j),(b+22)] <- length(a)
    }
  }
}

write.csv(bird_dataframe, "bird_dataframe_practice.csv", row.names = F)

#period <- "Period A"
#for(k in 1:length(list2)) {
#  month <- list2[k]
#  for(j in 1:length(list)) {
#    birds_period <- NULL
#    for(i in 3:12) {
#      a <- birds_per_period[,c(1:2,i)]
#      a$minute <- i-2
#      a <- a[,c(1,2,4,3)]
#      colnames(a) <- c("date", "Species","minute","Period")
#      birds_period <- rbind(birds_period, a) 
#    }
#    a <- grep(month, birds_period$date)
#    birds_period_temp <- birds_period[a,]
#    #a <- boxplot(birds_periodB$periodB[]~birds_periodB$Species[])
#    a <- grep(list[j], birds_period_temp$Species)
#    birds_period_temp <- birds_period_temp[a,]
#    birds_period_temp[,2] <- as.character(birds_period_temp[,2])
#    tiff(paste(list1[j],period,month,".tiff",sep = "_"),
#         width = 1000, height = 1000)
#    boxplot(birds_period_temp$Period ~ birds_period_temp$Species,
#            cex.main=2, cex.axis=1.4, las=1)
#    mtext(side = 3, line = 1.65, 
#          paste(period, month, sep = " - "), cex = 3)
#    mtext(side = 3, line = 0.3, list1[j], cex = 2)
#    dev.off()
#  }
#}

#period <- "Period B"
#for(k in 1:length(list2)) {
#  month <- list2[k]  
#  for(j in 1:length(list)) {
#    birds_period <- NULL
#    for(i in 13:22) {
#      a <- birds_per_period[,c(1:2,i)]
#      a$minute <- i-2
#      a <- a[,c(1,2,4,3)]
#      colnames(a) <- c("date", "Species","minute","Period")
#      birds_period <- rbind(birds_period, a) 
#    }
#    a <- grep(month, birds_period$date)
#    birds_period_temp <- birds_period[a,]
#    #a <- boxplot(birds_periodB$periodB[]~birds_periodB$Species[])
#    a <- grep(list[j], birds_period_temp$Species)
#    birds_period_temp <- birds_period_temp[a,]
#    birds_period_temp[,2] <- as.character(birds_period_temp[,2])
#    tiff(paste(list1[j],period,month,".tiff",sep = "_"),
#         width = 1000, height = 1000)
#    boxplot(birds_period_temp$Period ~ birds_period_temp$Species,
#            cex.main=2, cex.axis=1.4, las=1)
#    mtext(side = 3, line = 1.65, 
#          paste(period, month, sep = " - "), cex = 3)
#    mtext(side = 3, line = 0.3, list1[j], cex = 2)
#    dev.off()
#  }
#}
#-----------------------------------
#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
# Boxplots for each period for each month ------------------
birds_per_period <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\bird_dataframe_final.csv", header = T)

list <- c("SC1", "SC2", "SC3", "EW ", "WTH", "EYR",
          "Lewins", "Kookaburra", "WTT t", "WTT p")
list1 <- c("Scarlet Honeyeater1", "Scarlet Honeyeater2",
           "Scarlet Honeyeater3", "Eastern Whipbird",
           "White-throated Honeyeater", "Eastern Yellow Robin",
           "Lewin's Honeyeater", "Laughing Kookaburra",
           "White-throated Treecreeper1", "White-throated Treecreeper2")
list2 <- c("201506", "201507", "201508")
list3 <- c(12, 10, 10, 8, 30, 40, 8, 10, 10, 2)
list4 <- c("June 2015", "July 2015", "August 2015")


for(j in 1:length(list)) { # list are the species/classes
  
  for(k in 1:length(list2)) { # list 2 are the months    
  
    # Collate the period A data into long format
    birds_periodA <- NULL
    for(i in 3:12) {
      a <- birds_per_period[,c(1:2,i)]
      a$minute <- i-2
      a <- a[,c(1,2,4,3)]
      colnames(a) <- c("date", "Species","minute","Period")
      birds_periodA <- rbind(birds_periodA, a) 
    }
    
    # Collate the period B data into long format
    birds_periodB <- NULL
    for(i in 13:22) {
      a <- birds_per_period[,c(1:2,i)]
      a$minute <- i-12
      a <- a[,c(1,2,4,3)]
      colnames(a) <- c("date", "Species","minute","Period")
      birds_periodB <- rbind(birds_periodB, a)
    }
    
    # Collate the period C data into long format
    birds_periodC <- NULL
    for(i in 23:32) {
      a <- birds_per_period[,c(1:2,i)]
      a$minute <- i-22
      a <- a[,c(1,2,4,3)]
      colnames(a) <- c("date", "Species","minute","Period")
      birds_periodC <- rbind(birds_periodC, a)
    }
    
    species <- list[j]
    month <- list2[k]
    
    # Select the species and month from the three dataframes
    a <- which(substr(birds_periodA$date,1,6)==month)
    birds_periodA_temp <- birds_periodA[a,]
    a <- grep(species, birds_periodA_temp$Species, 
              ignore.case = F)
    birds_periodA_temp <- birds_periodA_temp[a,]
    birds_periodA_temp <- data.frame(birds_periodA_temp)
    if(nrow(birds_periodA_temp)==0) {
      birds_periodA_temp <- data.frame(date=month, 
                                       Species=species,
                                       minute=0,
                                       Period=0)
      birds_periodA_temp$Species <- species
    }
    birds_periodA_temp$Species <- as.character(birds_periodA_temp$Species)
    
    a <- which(substr(birds_periodB$date,1,6)==month)
    birds_periodB_temp <- birds_periodB[a,]
    a <- grep(species, birds_periodB_temp$Species, 
              ignore.case = F)
    birds_periodB_temp <- birds_periodB_temp[a,]
    birds_periodB_temp <- data.frame(birds_periodB_temp)
    if(nrow(birds_periodB_temp)==0) {
      birds_periodB_temp <- data.frame(date=month, 
                                       Species=species,
                                       minute=0,
                                       Period=0)
      birds_periodB_temp$Species <- species
    }
    birds_periodB_temp$Species <- as.character(birds_periodB_temp$Species)
    
    a <- which(substr(birds_periodC$date,1,6)==month)
    birds_periodC_temp <- birds_periodC[a,]
    a <- grep(species, birds_periodC_temp$Species, 
              ignore.case = F)
    birds_periodC_temp <- birds_periodC_temp[a,]
    birds_periodC_temp <- data.frame(birds_periodC_temp)
    if(nrow(birds_periodC_temp)==0) {
      birds_periodC_temp <- data.frame(date=month, 
                                       Species=species,
                                       minute=0,
                                       Period=0)
      birds_periodC_temp$Species <- species
    }
    birds_periodC_temp$Species <- as.character(birds_periodC_temp$Species)
    
    # plot and save the boxplots
    if(list2[k] ==list2[1]) {
      tiff(paste(list1[j], species,".tiff",sep = "_"),
           width = 2000, height = 1300)
      par(mfrow=c(3,3), mar=c(4,5,1,1), oma=c(2,2,3,1))
    }
    
    boxplot(birds_periodA_temp$Period ~ birds_periodA_temp$Species,
            cex = 2.2, las=1, cex.axis = 2.8, ylim = c(0, list3[j]))
    mtext(side = 2, line = 4, "Calls per minute", cex = 2)
    period <- "Period A"
    if(month==list2[1]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[2]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[3]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    
    boxplot(birds_periodB_temp$Period ~ birds_periodB_temp$Species,
            cex = 2.2, las=1, cex.axis = 2.8, ylim = c(0, list3[j]))
    period <- "Period B"
    if(month==list2[1]) {
    text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
         paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[2]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[3]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    mtext(side = 3, line = 0.3, list1[j], cex = 2.2, outer = T)
    
    boxplot(birds_periodC_temp$Period ~ birds_periodC_temp$Species,
            cex = 2.2, las=1, cex.axis = 2.8, ylim = c(0, list3[j]))
    period <- "Period C"
    if(month==list2[1]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[2]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[3]) {
      text(x = length(unique(birds_periodB_temp$Species)), 
           y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
  }
  dev.off()
}

#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
#Plotmean plots per period for Kaleidoscope data-----------------------------------

birds_per_period <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\bird_dataframe_final.csv", header = T)

list <- c("SC1", "SC2", "SC3", "EW ", "WTH", "EYR",
          "Lewins", "Kookaburra", "WTT t", "WTT p")
list1 <- c("Scarlet Honeyeater1", "Scarlet Honeyeater2",
           "Scarlet Honeyeater3", "Eastern Whipbird",
           "White-throated Honeyeater", "Eastern Yellow Robin",
           "Lewin's Honeyeater", "Laughing Kookaburra",
           "White-throated Treecreeper1", "White-throated Treecreeper2")
list2 <- c("201506", "201507", "201508")
list3 <- c(12, 10, 10, 8, 30, 40, 8, 10, 10, 2)
list4 <- c("June 2015", "July 2015", "August 2015")

for(j in 1:length(list)) { # list are the species/classes
  for(k in 1:length(list2)) { # list 2 are the months    
    # Collate the period A data into long format
    birds_periodA <- NULL
    for(i in 3:12) {
      a <- birds_per_period[,c(1:2,i)]
      a$minute <- i-2
      a <- a[,c(1,2,4,3)]
      colnames(a) <- c("date", "Species","minute","Period")
      birds_periodA <- rbind(birds_periodA, a) 
    }
    
    # Collate the period B data into long format
    birds_periodB <- NULL
    for(i in 13:22) {
      a <- birds_per_period[,c(1:2,i)]
      a$minute <- i-12
      a <- a[,c(1,2,4,3)]
      colnames(a) <- c("date", "Species","minute","Period")
      birds_periodB <- rbind(birds_periodB, a)
    }
    
    # Collate the period C data into long format
    birds_periodC <- NULL
    for(i in 23:32) {
      a <- birds_per_period[,c(1:2,i)]
      a$minute <- i-22
      a <- a[,c(1,2,4,3)]
      colnames(a) <- c("date", "Species","minute","Period")
      birds_periodC <- rbind(birds_periodC, a)
    }
    
    species <- list[j]
    month <- list2[k]
    
    # Select the species and month from the three dataframes
    a <- which(substr(birds_periodA$date,1,6)==month)
    birds_periodA_temp <- birds_periodA[a,]
    a <- grep(species, birds_periodA_temp$Species, 
              ignore.case = F)
    birds_periodA_temp <- birds_periodA_temp[a,]
    birds_periodA_temp <- data.frame(birds_periodA_temp)
    if(nrow(birds_periodA_temp)==0) {
      birds_periodA_temp <- data.frame(date=month, 
                                       Species=species,
                                       minute=0,
                                       Period=0)
      birds_periodA_temp$Species <- species
    }
    birds_periodA_temp$Species <- as.character(birds_periodA_temp$Species)
    
    a <- which(substr(birds_periodB$date,1,6)==month)
    birds_periodB_temp <- birds_periodB[a,]
    a <- grep(species, birds_periodB_temp$Species, 
              ignore.case = F)
    birds_periodB_temp <- birds_periodB_temp[a,]
    birds_periodB_temp <- data.frame(birds_periodB_temp)
    if(nrow(birds_periodB_temp)==0) {
      birds_periodB_temp <- data.frame(date=month, 
                                       Species=species,
                                       minute=0,
                                       Period=0)
      birds_periodB_temp$Species <- species
    }
    birds_periodB_temp$Species <- as.character(birds_periodB_temp$Species)
    
    a <- which(substr(birds_periodC$date,1,6)==month)
    birds_periodC_temp <- birds_periodC[a,]
    a <- grep(species, birds_periodC_temp$Species, 
              ignore.case = F)
    birds_periodC_temp <- birds_periodC_temp[a,]
    birds_periodC_temp <- data.frame(birds_periodC_temp)
    if(nrow(birds_periodC_temp)==0) {
      birds_periodC_temp <- data.frame(date=month, 
                                       Species=species,
                                       minute=0,
                                       Period=0)
      birds_periodC_temp$Species <- species
    }
    birds_periodC_temp$Species <- as.character(birds_periodC_temp$Species)
    
    # plot and save the boxplots
    if(list2[k] ==list2[1]) {
      tiff(paste(list1[j], species,".tiff",sep = "_"),
           width = 2000, height = 1300)
      par(mfrow=c(3,3), mar=c(4,5,1,1), oma=c(2,2,3,1))
    }
    
    boxplot(birds_periodA_temp$Period ~ birds_periodA_temp$Species,
            cex = 2.2, las=1, cex.axis = 2.8, ylim = c(0, list3[j]))
    mtext(side = 2, line = 4, "Calls per minute", cex = 2)
    period <- "Period A"
    if(month==list2[1]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[2]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[3]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    
    boxplot(birds_periodB_temp$Period ~ birds_periodB_temp$Species,
            cex = 2.2, las=1, cex.axis = 2.8, ylim = c(0, list3[j]))
    period <- "Period B"
    if(month==list2[1]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[2]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[3]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    mtext(side = 3, line = 0.3, list1[j], cex = 2.2, outer = T)
    
    boxplot(birds_periodC_temp$Period ~ birds_periodC_temp$Species,
            cex = 2.2, las=1, cex.axis = 2.8, ylim = c(0, list3[j]))
    period <- "Period C"
    if(month==list2[1]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[2]) {
      text(x = length(unique(birds_periodB_temp$Species)), y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
    if(month==list2[3]) {
      text(x = length(unique(birds_periodB_temp$Species)), 
           y = list3[j], 
           paste(period, list4[k], sep = " - "), cex = 2.8)
    }
  }
  dev.off()
}

birds_periodA$month <- "a"
for(i in 1:length(birds_periodA$date)) {
  if(substr(birds_periodA$date[i],5,6)=="06") {
    birds_periodA$month[i] <- "June 2015"  
  }
  if(substr(birds_periodA$date[i],5,6)=="07") {
    birds_periodA$month[i] <- "July 2015"
  }
  if(substr(birds_periodA$date[i],5,6)=="08") {
    birds_periodA$month[i] <- "August 2015"  
  }
}

birds_periodB$month <- "a"
for(i in 1:length(birds_periodB$date)) {
  if(substr(birds_periodB$date[i],5,6)=="06") {
    birds_periodB$month[i] <- "June 2015"  
  }
  if(substr(birds_periodB$date[i],5,6)=="07") {
    birds_periodB$month[i] <- "July 2015"
  }
  if(substr(birds_periodB$date[i],5,6)=="08") {
    birds_periodB$month[i] <- "August 2015"  
  }
}
birds_periodB$Species <- as.character(birds_periodB$Species)

birds_periodC$month <- "a"
for(i in 1:length(birds_periodC$date)) {
  if(substr(birds_periodC$date[i],5,6)=="06") {
    birds_periodC$month[i] <- "June 2015"  
  }
  if(substr(birds_periodC$date[i],5,6)=="07") {
    birds_periodC$month[i] <- "July 2015"
  }
  if(substr(birds_periodC$date[i],5,6)=="08") {
    birds_periodC$month[i] <- "August 2015"  
  }
}
birds_periodC$Species <- as.character(birds_periodC$Species)
  
# Select Species
ylim_max <- c(2.5, 1, 1.8, 2.5, 11, 6, 0.26, 0.4, 2, 0.5)
legend <- c("Far", "Mid", "Near")
#[1] "Kookaburra Cackle" "Kookaburra Loud"   "Kookaburra Mod"   
#[4] "Kookaburra Quiet" 
for(i in 1:length(list)) {
  ylim <- c(0,ylim_max[i])
  tiff(paste("plotmeans_plots\\test", "_",list[i],".tiff", sep = ""), 
       width=3400, height=1200)
  par(mfrow=c(1,3), mar=c(2,0,0,0), oma = c(0,3,1.5,1),
      cex = 2.6, cex.lab = 2.6)
  
  a <- grep(list[i], birds_periodA$Species)
  birds_periodA_temp <- birds_periodA[a,]
  birds_periodA_temp$Species <- as.character(birds_periodA_temp$Species) 
  unique_class <- unique(birds_periodA_temp$Species)
  a <- grep(unique_class[1], birds_periodA_temp$Species)
  birds_periodA_temp1 <- birds_periodA_temp[a,]
  plotmeans(birds_periodA_temp1$Period~birds_periodA_temp1$month,
            connect = T, 
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4, lty = 4)
  abline(v=6.5, lty=2)
  axis(side = 1, at = 1:3, tick = T, labels = list4,
       mgp = c(3, 0.3, 0))
  mtext(side = 2, line = 1.5, cex = 3,
        "Number of calls per minute +/- 95% C.I.")
  par(new=T)
  a <- grep(unique_class[2], birds_periodA_temp$Species)
  birds_periodA_temp1 <- birds_periodA_temp[a,]
  plotmeans(birds_periodA_temp1$Period~birds_periodA_temp1$month,
            connect = T, col = "black", lty = 2,
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4)
  par(new=T)
  a <- grep(unique_class[3], birds_periodA_temp$Species)
  birds_periodA_temp1 <- birds_periodA_temp[a,]
  plotmeans(birds_periodA_temp1$Period~birds_periodA_temp1$month,
            connect = T, col = "black",
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4)
  
  a <- grep(list[i], birds_periodB$Species)
  birds_periodB_temp <- birds_periodB[a,]
  birds_periodB_temp$Species <- as.character(birds_periodB_temp$Species) 
  unique_class <- unique(birds_periodB_temp$Species)
  a <- grep(unique_class[1], birds_periodB_temp$Species)
  birds_periodB_temp1 <- birds_periodB_temp[a,]
  plotmeans(birds_periodB_temp1$Period~birds_periodB_temp1$month,
            connect = T, lty = 4,
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4)
  abline(v=6.5, lty=2)
  axis(side = 1, at = 1:3, tick = T, labels = list4,
       mgp = c(3, 0.3, 0))
  #mtext(side = 2, line = 1, cex = 2.6,
  #      "Number of calls per minute +/- 95% C.I.")
  par(new=T)
  a <- grep(unique_class[2], birds_periodB_temp$Species)
  birds_periodB_temp1 <- birds_periodB_temp[a,]
  plotmeans(birds_periodB_temp1$Period~birds_periodB_temp1$month,
            connect = T, col = "black", lty = 2,
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4)
  par(new=T)
  a <- grep(unique_class[3], birds_periodB_temp$Species)
  birds_periodB_temp1 <- birds_periodB_temp[a,]
  plotmeans(birds_periodB_temp1$Period~birds_periodB_temp1$month,
            connect = T, col = "black",
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4)
  mtext(paste(list1[i]), side = 3, cex = 4)
  
  a <- grep(list[i], birds_periodC$Species)
  birds_periodC_temp <- birds_periodC[a,]
  birds_periodC_temp$Species <- as.character(birds_periodC_temp$Species) 
  unique_class <- unique(birds_periodC_temp$Species)
  a <- grep(unique_class[1], birds_periodC_temp$Species)
  birds_periodC_temp1 <- birds_periodC_temp[a,]
  plotmeans(birds_periodC_temp1$Period~birds_periodC_temp1$month,
            connect = T, lty = 4,
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4)
  abline(v=6.5, lty=2)
  axis(side = 1, at = 1:3, tick = T, labels = list4,
       mgp = c(3, 0.3, 0))
  #mtext(side = 2, line = 1, cex = 2.6,
  #      "Number of calls per minute +/- 95% C.I.")
  par(new=T)
  a <- grep(unique_class[2], birds_periodC_temp$Species)
  birds_periodC_temp1 <- birds_periodC_temp[a,]
  plotmeans(birds_periodC_temp1$Period~birds_periodC_temp1$month,
            connect = T, col = "black", lty = 2,
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4)
  par(new=T)
  a <- grep(unique_class[3], birds_periodC_temp$Species)
  birds_periodC_temp1 <- birds_periodC_temp[a,]
  plotmeans(birds_periodC_temp1$Period~birds_periodC_temp1$month,
            connect = T, col = "black",
            n.label = F, minbar = 0, ylim = ylim, 
            xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
            las=1, lwd = 2.4)
  legend(x = 2.5, y = 1.05*ylim[2], lty = c(4,2,1), 
         legend = c(legend[1], legend[2], legend[3]), 
         lwd = 2.4, cex = 1.8, bty = "n")
  dev.off()
}

#-----------------------------------------
source("scripts/Bird_lists_func.R")
species_data  <- read.csv("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\species_each_minute_protected_saved.csv", header = T)
a <- which(species_data$minute_reference < 405)
species_data_temp <- species_data[a,]

tail(civil_dawn$dates)
civil_dawn$dates <- as.character.Date(civil_dawn$dates)
civil_dawn$dates <- paste(substr(civil_dawn$dates,1,4),
                          substr(civil_dawn$dates,6,7),
                          substr(civil_dawn$dates, 9, 10), sep = "")
a1 <- which(civil_dawn$dates=="20150622")
a2 <- which(civil_dawn$dates=="20150816")
civil_dawn <- civil_dawn[a1:a2,]
for(i in 1:nrow(civil_dawn)) {
  civil_dawn$civil_dawn_min[i] <- as.numeric(substr(civil_dawn$CivSunrise[i],1,1))*60 +
    as.numeric(substr(civil_dawn$CivSunrise[i],2,3))
}

for(i in 1:nrow(civil_dawn)) {
  civil_dawn$Start_A[i] <- civil_dawn$civil_dawn_min[i] - 25
  civil_dawn$Start_B[i] <- civil_dawn$civil_dawn_min[i] - 5
  civil_dawn$Start_C[i] <- civil_dawn$civil_dawn_min[i] + 10
}

list <- c("SC1 Far",
          "SC1 Mod",
          "SC1 Near",
          "SC2 Far",
          "SC2 Mod",
          "SC2 Near",
          "EW Far",
          "EW Mod",
          "EW Near",
          "WTH Far",
          "WTH Mod",
          "WTH Near",
          "EYR Far",
          "EYR Mod",
          "EYR Near",
          "WTT Far",
          "WTT Mod",
          "WTT Near",
          "TRC Far",
          "TRC Mod",
          "TRC Near",
          "PCW Far",
          "PCW Mod",
          "PCW Near",
          "RLT Mod",
          "RLK Near",
          "RFW Near",
          "GDW Near",
          "YTB Near",
          "VFW")  #"WTT trill fast"

cluster <- 40
a <- which(species_data_temp$cluster_list==cluster)
clust_temp <- species_data_temp[a,]

periodA <- as.data.frame(matrix(ncol = length(clust_temp)))
periodB <- as.data.frame(matrix(ncol = length(clust_temp)))
periodC <- as.data.frame(matrix(ncol = length(clust_temp)))
refA <- 0
refB <- 0
refC <- 0
for(i in 1:nrow(clust_temp)) {
  date <- clust_temp$dates[i]
  a <- which(civil_dawn$dates==date)
  startA <- civil_dawn$Start_A[a]
  startB <- civil_dawn$Start_B[a]
  startC <- civil_dawn$Start_C[a]
  if(clust_temp$minute_reference[i] %in% (startA:(startA+9))) {
    refA <- refA + 1
    periodA[refA,1:length(clust_temp)] <- clust_temp[i,]
  }
  if(clust_temp$minute_reference[i] %in% (startB:(startB+9))) {
    refB <- refB + 1
    periodB[refB,(1:length(clust_temp))] <- clust_temp[i,]
  }
  if(clust_temp$minute_reference[i] %in% (startC:(startC+9))) {
    refC <- refC + 1
    periodC[refC,1:length(clust_temp)] <- clust_temp[i,]
  }
}

col_names <- colnames(species_data)
colnames(periodA) <- col_names
colnames(periodB) <- col_names
colnames(periodC) <- col_names
list2 <- c(5:7,11:22,30:32,36:46,50)
tiff(paste("Cluster_Period_plot",cluster,".tiff", sep = "_"),
     height = 780, width = 2300)
par(mfrow=c(3,1), mar=c(3,0,2,2), oma=c(0,4,2,0), cex=1, 
    xaxs="i")
period <- "Period A"
if(!all(is.na(periodA[,list2]))) {
  boxplot(periodA[,list2], frame = F, axes = F) #ylim = c(0,30), frame = F)
  axis(side = 2, line = 0)
  axis(side = 1, line = 1, at = 1:length(list2), labels = list)
  mtext(side = 2, line = 2, "Number of calls", cex = 1.2)
}
if(all(is.na(periodA[,list2]))) {
  periodA[,list2] <- 0
  par(new=T)
  boxplot(periodA[,list2], ylim = c(0,20), 
          frame = F, axes = F)
  mtext(side = 2, side = 2, "Number of calls", cex = 1.2)
}
mtext(side = 3, line = 0.5, paste("     Cluster ", cluster), adj =0, cex=3)
mtext(period, cex = 2)
if(all(is.na(periodA[1]))) {
  mtext(cex = 1.2, paste("Number of minutes = 0", "              "), adj = 1)
  mtext(line = -4, cex = 1.2, side = 3, 
        paste("Average (Mid & Near) calls per minute = 0", 
              "                    "), adj = 1)
}
if(!all(is.na(periodA[1]))) {
  mtext(cex = 1.2, paste("Number of minutes =", nrow(periodA), "                   "), adj = 1)
  mtext(line = -4, cex = 1.2, side = 3, 
        paste("Average (Mid & Near) calls per minute =", 
              round(sum(periodA$Mod_totals, periodA$Near_totals)/nrow(periodA)), 
              "                    "), adj = 1)
}

mtext(side = 3, cex = 1.2, line = -1, paste("Number of Far-distance calls =", sum(periodA$Far_totals), "                   "), adj = 1)
mtext(side = 3, cex = 1.2, line = -2, paste("Number of Mid-distance calls =", sum(periodA$Mod_totals), "                   "), adj = 1)
mtext(side = 3, cex = 1.2, line = -3, paste("Number of Near-distance calls =", sum(periodA$Near_totals), "                   "), adj = 1)

period <- "Period B"
boxplot(periodB[,list2], frame = F, axes = F) #,ylim = c(0,30), frame = F)
axis(side = 2, line = 0)
axis(side = 1, line = 1, at = 1:length(list2), labels = list)
mtext(side = 2, line = 2,"Number of calls", cex = 1.2)
#title(paste("Cluster ", cluster))
mtext(period, cex = 2)
mtext(cex = 1.2, paste("Number of minutes =", nrow(periodB), "                   "), adj = 1)
mtext(line = -1, cex = 1.2, side = 3, paste("Number of Far-distance calls =", sum(periodB$Far_totals), "                   "), adj = 1)
mtext(line = -2, cex = 1.2, side = 3, paste("Number of Mid-distance calls =", sum(periodB$Mod_totals), "                   "), adj = 1)
mtext(line = -3, cex = 1.2, side = 3, paste("Number of Near-distance calls =", sum(periodB$Near_totals), "                   "), adj = 1)
mtext(line = -4, cex = 1.2, side = 3, 
      paste("Average (Mid & Near) calls per minute =", 
            round(sum(periodB$Mod_totals, periodB$Near_totals)/nrow(periodB)), 
            "                    "), adj = 1)
period <- "Period C"
boxplot(periodC[,list2], frame = F, axes = F) #ylim = c(0,30), frame = F)
axis(side = 2, line = 0)
axis(side = 1, line = 1, at = 1:length(list2), labels = list)
mtext(side = 2, line = 2,"Number of calls", cex = 1.2)
#title(paste("Cluster ", cluster))
mtext(period, cex = 2)
mtext(cex = 1.2, paste("Number of minutes =", nrow(periodC), "                   "), adj = 1)
mtext(line = -1, cex = 1.2, side = 3, paste("Number of Far-distance calls =", sum(periodC$Far_totals), "                   "), adj = 1)
mtext(line = -2, cex = 1.2, side = 3, paste("Number of Mid-distance calls =", sum(periodC$Mod_totals), "                    "), adj = 1)
mtext(line = -3, cex = 1.2, side = 3, paste("Number of Near-distance calls =", sum(periodC$Near_totals), "                    "), adj = 1)
mtext(line = -4, cex = 1.2, side = 3, 
      paste("Average (Mid & Near) calls per minute =", 
            round(sum(periodC$Mod_totals, periodC$Near_totals)/nrow(periodC)),
            "                    "), adj = 1)
dev.off()
dev.off()

library(gplots)
list2
par(mfrow=c(1,3), mar=c(2,0,0,0), oma = c(1,3,1,1),
    cex = 1, cex.lab = 2.6)

periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

# EYR
label_name <- "Eastern_Yellow_Robin"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$EYR.Far[a]
mod <- periods$EYR.Mod[a]
near <- periods$EYR.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,40),
            main = "Period A - EYR")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,40), 
            n.label = F, main = "Period A - EYR")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$EYR.Far[a]
mod <- periods$EYR.Mod[a]
near <- periods$EYR.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,40),
            main="Period B - EYR")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,40), 
            n.label = F, main = "Period B - EYR")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$EYR.Far[a]
mod <- periods$EYR.Mod[a]
near <- periods$EYR.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,40), 
            main = "Period C - EYR")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,40), 
            n.label = F, main = "Period C - EYR")
}
dev.off()

# Laughing Kookaburra
label_name <- "Laughing_Kook"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$Kookaburra.Quiet[a]
mod <- periods$Kookaburra.Mod[a]
near <- periods$Kookaburra.Loud[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("a.Quiet","b.Mid","c.Loud"),each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,5),
            main = "Period A - LKB")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,5), 
            n.label = F, main = "Period A - LKB")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$Kookaburra.Quiet[a]
mod <- periods$Kookaburra.Mod[a]
near <- periods$Kookaburra.Loud[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("a.Quiet","b.Mid","c.Loud"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,5),
            main="Period B - LKB")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,5), 
            n.label = F, main = "Period B - LKB")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$Kookaburra.Quiet[a]
mod <- periods$Kookaburra.Mod[a]
near <- periods$Kookaburra.Loud[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("a.Quiet","b.Mid","c.Loud"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,5), 
            main = "Period C - LKB")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,5), 
            n.label = F, main = "Period C - LKB")
}
dev.off()

# White-throated Honeyeater
label_name <- "White_throated_Honeyeater"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$WTH.Far[a]
mod <- periods$WTH.Mod[a]
near <- periods$WTH.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"), each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,28),
            main = "Period A - WTH")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,28), 
            n.label = F, main = "Period A - WTH")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$WTH.Far[a]
mod <- periods$WTH.Mod[a]
near <- periods$WTH.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,28),
            main="Period B - WTH")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,28), 
            n.label = F, main = "Period B - WTH")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$WTH.Far[a]
mod <- periods$WTH.Mod[a]
near <- periods$WTH.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,28), 
            main = "Period C - WTH")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,28), 
            n.label = F, main = "Period C - WTH")
}
dev.off()

# EWB
label_name <- "Eastern_Whipbird"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$EW.Far[a]
mod <- periods$EW.Mod[a]
near <- periods$EW.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"), each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,5),
            main = "Period A - EWB")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,5), 
            n.label = F, main = "Period A - EWB")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$EW.Far[a]
mod <- periods$EW.Mod[a]
near <- periods$EW.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,5),
            main="Period B - EWB")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,5), 
            n.label = F, main = "Period B - EWB")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$EW.Far[a]
mod <- periods$EW.Mod[a]
near <- periods$EW.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,5), 
            main = "Period C - EWB")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,5), 
            n.label = F, main = "Period C - EWB")
}
dev.off()

# TOR
label_name <- "Torresian_Crow"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$Torresian.Crow.Far[a]
mod <- periods$Torresian.Crow.Mod[a]
near <- periods$Torresian.Crow.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"), each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,5),
            main = "Period A - TOR")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,5), 
            n.label = F, main = "Period A - TOR")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$Torresian.Crow.Far[a]
mod <- periods$Torresian.Crow.Mod[a]
near <- periods$Torresian.Crow.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,5),
            main="Period B - TOR")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,5), 
            n.label = F, main = "Period B - TOR")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 2.2)

a <- which(periods$period=="Period C")
far <- periods$Torresian.Crow.Far[a]
mod <- periods$Torresian.Crow.Mod[a]
near <- periods$Torresian.Crow.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,5), 
            main = "Period C - TOR")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,5), 
            n.label = F, main = "Period C - TOR")
}
dev.off()

# WTT
label_name <- "White_throated_Treecreeper"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$WTT.trill.Far[a]
mod <- periods$WTT.trill.Mod[a]
near <- periods$WTT.trill.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"), each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,5),
            main = "Period A - WTT")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,5), 
            n.label = F, main = "Period A - WTT")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$WTT.trill.Far[a]
mod <- periods$WTT.trill.Mod[a]
near <- periods$WTT.trill.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,5),
            main="Period B - WTT")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,5), 
            n.label = F, main = "Period B - WTT")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 2.2)

a <- which(periods$period=="Period C")
far <- periods$WTT.trill.Far[a]
mod <- periods$WTT.trill.Mod[a]
near <- periods$WTT.trill.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,5), 
            main = "Period C - WTT")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,5), 
            n.label = F, main = "Period C - WTT")
}
dev.off()

# Scarlet Honeyeater SC1
label_name <- "Scarlet_Honeyeater1"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$SC1.Far[a]
mod <- periods$SC1.Mod[a]
near <- periods$SC1.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10),
            main = "Period A - SC1")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10), 
            n.label = F, main = "Period A - EYR")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$SC1.Far[a]
mod <- periods$SC1.Mod[a]
near <- periods$SC1.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10),
            main="Period B - SC1")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10), 
            n.label = F, main = "Period B - EYR")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$SC1.Far[a]
mod <- periods$SC1.Mod[a]
near <- periods$SC1.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            main = "Period C - SC1")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            n.label = F, main = "Period C - EYR")
}
dev.off()

# Scarlet Honeyeater SC2
label_name <- "Scarlet_Honeyeater2"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$SC3.Chatter.Far[a]
mod <- periods$SC3.Chatter.Mod[a]
near <- periods$SC3.Chatter.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10),
            main = "Period A - SC2")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10), 
            n.label = F, main = "Period A - EYR")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$SC3.Chatter.Far[a]
mod <- periods$SC3.Chatter.Mod[a]
near <- periods$SC3.Chatter.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10),
            main="Period B - SC2")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10), 
            n.label = F, main = "Period B - EYR")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$SC3.Chatter.Far[a]
mod <- periods$SC3.Chatter.Mod[a]
near <- periods$SC3.Chatter.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            main = "Period C - SC2")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            n.label = F, main = "Period C - EYR")
}
dev.off()

# Rufous Whistler
label_name <- "Rufous_Whistler"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Rufous.Whistler.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10),
            main = "Period A - RFW")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10), 
            n.label = F, main = "Period A - RFW")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Rufous.Whistler.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10),
            main="Period B - RFW")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10), 
            n.label = F, main = "Period B - RFW")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Rufous.Whistler.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            main = "Period C - RFW")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            n.label = F, main = "Period C - RFW")
}
dev.off()

# Yellow-tailed Black Cockatoo
label_name <- "Yellow_tailed_Black_Cockatoo"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Yellow.tailed.Black.Cockatoo.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10),
            main = "Period A - YTB")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10), 
            n.label = F, main = "Period A - YTB")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Rufous.Whistler.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10),
            main="Period B - YTB")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10), 
            n.label = F, main = "Period B - YTB")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Yellow.tailed.Black.Cockatoo.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            main = "Period C - YTB")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            n.label = F, main = "Period C - YTB")
}
dev.off()

# Pied Currawong
label_name <- "Pied Currawong"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$Pied.Currawong.Far[a]
mod <- periods$Pied.Currawong.Mod[a]
near <- periods$Pied.Currawong.Near[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10),
            main = "Period A - PDC")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10), 
            n.label = F, main = "Period A - PDC")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$Pied.Currawong.Far[a]
mod <- periods$Pied.Currawong.Mod[a]
near <- periods$Pied.Currawong.Near[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10),
            main="Period B - PDC")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10), 
            n.label = F, main = "Period B - PDC")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$Pied.Currawong.Far[a]
mod <- periods$Pied.Currawong.Mod[a]
near <- periods$Pied.Currawong.Near[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            main = "Period C - PDC")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            n.label = F, main = "Period C - PDC")
}
dev.off()

# MO Variegated Fairy-Wren
label_name <- "Variegated_Fairy_Wren"
tiff(paste("Period_A_B_C_",cluster,"_",label_name,".tiff",sep=""),
     height = 400, width = 900)
par(mfrow=c(1,3), mar=c(1.4,0,2,0), oma = c(1,3,1,1),
    cex = 1.2, cex.lab = 2.6)
periodA$period <- "Period A"
periodB$period <- "Period B"
periodC$period <- "Period C"
periods <- rbind(periodA, periodB, periodC)

periods_A <- NULL
periods_B <- NULL
periods_C <- NULL
a <- which(periods$period=="Period A")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Fairywren[a]
periods_A$periodA <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             

periods_A$labels <- labels
if(!all(is.na(periods_A$periodA))) {
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10),
            main = "Period A - VFW")
}

if(all(is.na(periods_A$periodA))) {
  periods_A$periodA[1:3] <- 0
  plotmeans(periods_A$periodA~periods_A$labels, ylim=c(0,10), 
            n.label = F, main = "Period A - VFW")
}
mtext("Average number of calls", side = 2, line = 2)

a <- which(periods$period=="Period B")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Fairywren[a]
periods_B$periodB <- c(far,mod, near)
labels <- rep(c("Far","Mid","Near"),each=length(a))                             
periods_B$labels <- labels
if(!all(is.na(periods_B$periodB))) {
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10),
            main="Period B - VFW")
}

if(all(is.na(periods_B$periodB))) {
  periods_B$periodB[1:3] <- 0
  plotmeans(periods_B$periodB~periods_B$labels, ylim=c(0,10), 
            n.label = F, main = "Period B - VFW")
}
mtext(paste("Cluster", cluster), side = 3, line = 1.4, cex = 1.8)

a <- which(periods$period=="Period C")
far <- periods$blank[a]
mod <- periods$blank[a]
near <- periods$Fairywren[a]
periods_C$periodC <- c(far,mod, near)
labels <- rep( c("Far","Mid","Near"), each = length(a))
periods_C$labels <- labels
if(!all(is.na(periods_C$periodC))) {
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            main = "Period C - VFW")
}

if(all(is.na(periods_C$periodC))) {
  periods_C$periodC[1:3] <- 0
  plotmeans(periods_C$periodC~periods_C$labels, ylim=c(0,10), 
            n.label = F, main = "Period C - VFW")
}
dev.off()

# Species _ Acoustic Indices analysis --------------------------------
rm(list = ls())
indices_statistics <- data.frame(desc = NA,
                                 SC1 = NA,
                                 SC2 = NA,
                                 EYR = NA,
                                 EW = NA,
                                 WTH = NA,
                                 KOOK = NA,
                                 TOR = NA,
                                 PDC = NA,
                                 WTT = NA)

indices_statistics$desc[1] <- "Significance_level"
indices_statistics[,2:10] <- 0.05
indices_statistics[2,1] <- "Sample_size"
indices_statistics[2,2:10] <- 20
indices_statistics[3,1] <- "SD_BGN"
indices_statistics[4,1] <- "SD_SNR"
indices_statistics[5,1] <- "SD_ACT"
indices_statistics[6,1] <- "SD_EVN"
indices_statistics[7,1] <- "SD_HFC"
indices_statistics[8,1] <- "SD_MFC"
indices_statistics[9,1] <- "SD_LFC"
indices_statistics[10,1] <- "SD_ACI"
indices_statistics[11,1] <- "SD_EAS"
indices_statistics[12,1] <- "SD_EPS"
indices_statistics[13,1] <- "SD_ECS"
indices_statistics[14,1] <- "SD_CLC"

indices_statistics[15,1] <- "AV_BGN"
indices_statistics[16,1] <- "AV_SNR"
indices_statistics[17,1] <- "AV_ACT"
indices_statistics[18,1] <- "AV_EVN"
indices_statistics[19,1] <- "AV_HFC"
indices_statistics[20,1] <- "AV_MFC"
indices_statistics[21,1] <- "AV_LFC"
indices_statistics[22,1] <- "AV_ACI"
indices_statistics[23,1] <- "AV_EAS"
indices_statistics[24,1] <- "AV_EPS"
indices_statistics[25,1] <- "AV_ECS"
indices_statistics[26,1] <- "AV_CLC"

indices_statistics[27,1] <- "CI_BGN"
indices_statistics[28,1] <- "CI_SNR"
indices_statistics[29,1] <- "CI_ACT"
indices_statistics[30,1] <- "CI_EVN"
indices_statistics[31,1] <- "CI_HFC"
indices_statistics[32,1] <- "CI_MFC"
indices_statistics[33,1] <- "CI_LFC"
indices_statistics[34,1] <- "CI_ACI"
indices_statistics[35,1] <- "CI_EAS"
indices_statistics[36,1] <- "CI_EPS"
indices_statistics[37,1] <- "CI_ECS"
indices_statistics[38,1] <- "CI_CLC"

indices_statistics <- t(indices_statistics)

species_acoustic_indices <- read.csv("species_each_minute_protected_final.csv", header = T)
#a <- which(species_acoustic_indices$minute_reference < 406)

#species_acoustic_indices_405 <- species_acoustic_indices[a,]
#write.csv(species_acoustic_indices_405, "species_each_minute_405_protected.csv", row.names = F)
#a <- which(species_acoustic_indices_405$cluster_list==11)
#species_acoustic_indices_405_temp_11 <- species_acoustic_indices_405[a,]
#mean(species_acoustic_indices_405_temp_11$EYR.Near)
#a <- which(species_acoustic_indices_405_temp_11$EW.Near > 0)
#a <- which(species_acoustic_indices_405_temp_11$WTT.trill.Near > 0)
#species_acoustic_indices_405_temp_11_EYRNear <- species_acoustic_indices_405_temp_11[a,]
#species_acoustic_indices_405_temp_11_EWNear <- species_acoustic_indices_405_temp_11[a,]
#species_acoustic_indices_405_temp_11_WTTNear <- species_acoustic_indices_405_temp_11[a,]
#mean(species_acoustic_indices_405_temp_11_EYRNear$EYR.Near)
#mean(species_acoustic_indices_405_temp_11_EWNear$EW.Near)
#mean(species_acoustic_indices_405_temp_11_WTTNear$WTT.trill.Near)
#boxplot(species_acoustic_indices_405_temp_11_EWNear$EW.Near)
#mtext(side=1, paste("n=", b$n,sep = ""))
list1 <- c("01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
           "11", "12", "13", "14", "15", "16", "17", "18", "19", "20")
list3 <- c("SC1", "SC3", "EYR", "EW", "WTH", "Kookaburra", "Tor", "PDC", "WTT")

for(i in 1:length(list3)) {
  list2 <- list3[i]
  a_list <- NULL
  for(i in 1:length(list2)) {
    for(j in 1:length(list1)) {
      label <- paste(list2[i],list1[j],sep = " ")
      a <- grep(label, species_acoustic_indices$Species) 
      a_list <- c(a_list, a)
    }
  }
  
  standard_deviation <- NULL
  average <- NULL
  species_acoustic_indices_temp <- species_acoustic_indices[a_list,]
  for(i in 60:71) {
    st_dev <- sd(species_acoustic_indices_temp[,i])
    standard_deviation <- c(standard_deviation, st_dev)
    av <- mean(species_acoustic_indices_temp[,i])
    average <- c(average, av)
  }
  
  if(list2 == "SC1") {
    indices_statistics[2, 3:14] <- standard_deviation
    indices_statistics[2, 15:26] <- average
  }
  if(list2 == "SC3") {
    indices_statistics[3, 3:14] <- standard_deviation
    indices_statistics[3, 15:26] <- average
  }
  if(list2 == "EYR") {
    indices_statistics[4, 3:14] <- standard_deviation
    indices_statistics[4, 15:26] <- average
  }
  if(list2 == "EW") {
    indices_statistics[5, 3:14] <- standard_deviation
    indices_statistics[5, 15:26] <- average
  }
  if(list2 == "WTH") {
    indices_statistics[6, 3:14] <- standard_deviation
    indices_statistics[6, 15:26] <- average
  }
  if(list2 == "Kookaburra") {
    indices_statistics[7, 3:14] <- standard_deviation
    indices_statistics[7, 15:26] <- average
  }
  if(list2 == "Tor") {
    indices_statistics[8, 3:14] <- standard_deviation
    indices_statistics[8, 15:26] <- average
  }
  if(list2 == "PDC") {
    indices_statistics[9, 3:14] <- standard_deviation
    indices_statistics[9, 15:26] <- average
  }
  if(list2 == "WTT") {
    indices_statistics[10,  3:14] <- standard_deviation
    indices_statistics[10, 15:26] <- average
  }
}

list2 <- "SC1"
a_list <- NULL
for(i in 1:length(list2)) {
  for(j in 1:length(list1)) {
    label <- paste(list2[i],list1[j],sep = " ")
    a <- grep(label, species_acoustic_indices$Species) 
    a_list <- c(a_list, a)
  }
}

standard_deviation <- NULL
average <- NULL
species_acoustic_indices_temp <- species_acoustic_indices[a_list,]
for(i in 60:71) {
  st_dev <- sd(species_acoustic_indices_temp[,i])
  standard_deviation <- c(standard_deviation, st_dev)
  av <- mean(species_acoustic_indices_temp[,i])
  average <- c(average, av)
}

if(list2 == "SC1") {
  indices_statistics[2, 3:14] <- standard_deviation
  indices_statistics[2, 15:26] <- average
}
if(list2 == "SC3") {
  indices_statistics[3, 3:14] <- standard_deviation
  indices_statistics[3, 15:26] <- average
}
if(list2 == "EYR") {
  indices_statistics[4, 3:14] <- standard_deviation
  indices_statistics[4, 15:26] <- average
}
if(list2 == "EW") {
  indices_statistics[5, 3:14] <- standard_deviation
  indices_statistics[5, 15:26] <- average
}
if(list2 == "WTH") {
  indices_statistics[6, 3:14] <- standard_deviation
  indices_statistics[6, 15:26] <- average
}
if(list2 == "Kookaburra") {
  indices_statistics[7, 3:14] <- standard_deviation
  indices_statistics[7, 15:26] <- average
}
if(list2 == "Tor") {
  indices_statistics[8, 3:14] <- standard_deviation
  indices_statistics[8, 15:26] <- average
}
if(list2 == "PDC") {
  indices_statistics[9, 3:14] <- standard_deviation
  indices_statistics[9, 15:26] <- average
}
if(list2 == "WTT") {
  indices_statistics[10,  3:14] <- standard_deviation
  indices_statistics[10, 15:26] <- average
}

indices_statistics <- t(indices_statistics)

View(indices_statistics)
#write.csv(indices_statistics, "Species_acoustic_indices_statistics.csv", row.names = F)
rm(list = ls())
statistics <- read.csv("Species_acoustic_indices_statistics_final.csv", 
                       header = T)[1:38,1:10]
statistics <- statistics[,c(1:7,10)]

names <- colnames(statistics[,2:length(statistics)])

# ALL INDICES SEPARATED (see below for individual plots)
dev.off()
cbbPalette <- c("#000000", "#E69F00", "#56B4E9", 
                "#009E73", "#F0E442", "#0072B2", 
                "#D55E00", "#CC79A7")
list3 <- c("SC1","SC2","EYR","EW","WTH","Kookaburra","WTT")
pch <- c(16,8,21,25,15,0,17,0,1)
lty <- c(2,1,5,4,3)
ref <- 0

a1_list <- c("AV_BGN","AV_SNR","AV_ACT","AV_EVN","AV_HFC","AV_MFC",
             "AV_LFC","AV_ACI","AV_EAS","AV_EPS","AV_ECS","AV_CLC")
a1_list <- c("AV_MFC","AV_BGN","AV_SNR","AV_EPS","AV_ACI")
a2_list <- c("CI_BGN","CI_SNR","CI_ACT","CI_EVN","CI_HFC","CI_MFC",
             "CI_LFC","CI_ACI","CI_EAS","CI_EPS","CI_ECS","CI_CLC")
a2_list <- c("CI_MFC","CI_BGN","CI_SNR","CI_EPS","CI_ACI")
label_name <- paste(substr(a1_list[1],4,6),
                    substr(a1_list[2],4,6),
                    substr(a1_list[3],4,6),
                    substr(a1_list[4],4,6),
                    substr(a1_list[5],4,6),
                    sep="_")
dev.off()
ref <- 0

tiff(paste("Species_acoustic_Indices",label_name,"_final_.tiff",sep=""),
     height = 750, width = 980)
par(mar=c(5,5,3,8), cex.axis=2, cex.main=2, cex.lab=2, oma=c(0,0,0,0))
#plot(x=42, y=1, xlim=c(0,42), ylim=c(0,1), #type="n", 
#     axes=T, xlab="", ylab="", col="white")
abline(v=1:42)

for(i in 1:length(list3)) {
  a <- which(statistics[,1]==a1_list[i])
  if(i <= length(a1_list)) {
    BGN <- statistics[a, 2:length(statistics)]
    BGN <- as.numeric(as.vector(BGN))
  a <- which(statistics[,1]==a2_list[i])
  BGN_CI <- statistics[a, 2:length(statistics)]
  BGN_CI <- as.numeric(as.vector(BGN_CI))
  CI.up <- BGN + BGN_CI
  CI.dn <- BGN - BGN_CI
  x <- c(-1, 5, 11, 17, 23, 29, 35)
  x <- x + ref
  #par(new=T)
  plot(x = x, y = BGN, xaxt='n', ylim=c(0,1), xlab='Species', 
       main='Average of the Normalised Acoustic Index per Species',
       col="black", pch=pch[i], las=1, ylab = "", cex=2.6, xlim = c(0,38))
  arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col="black") #colour[i])
  
  ref <- ref + 1  
  if(ref==1) {
    axis(1, at=x+2, labels=names)
  }
  par(new=T)
  }
}
mtext(side = 2, line = 3.5, 'Normalised Index ± 95% C.I.', cex = 2)
legend <- substr(a2_list, 4,6)
par(xpd=FALSE) #x = 33.9, y = 1.06, 
legend("topright", col = "black", #c(colour[1:5]), 
       legend = c(legend[1], legend[2], legend[3], legend[4], legend[5]), 
       cex = 2.5, bty = "n", pch=pch[1:5], horiz = FALSE, xpd=TRUE,
       x.intersp = 0.9, y.intersp = 0.7, inset=c(-0.15,0))
abline(v=c(4,10,16,22,28,34,40))
abline(h=c(0.2,0.4,0.6,0.8), lty=2, lwd=0.4)
dev.off()


# Background Noise
dev.off()
label_name <- "BGN"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
x <- 1:(length(names))*2-1
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))

a <- which(statistics[,1]=="AV_BGN")
BGN <- statistics[a, 2:length(statistics)]
BGN <- as.numeric(as.vector(BGN))

a <- which(statistics[,1]=="CI_BGN")
BGN_CI <- statistics[a, 2:length(statistics)]
BGN_CI <- as.numeric(as.vector(BGN_CI))

CI.up <- BGN + BGN_CI
CI.dn <- BGN - BGN_CI

plot(BGN ~ x, cex=1.5, xaxt='n', ylim=c(0,1), xlab='Species', ylab=paste('Normalised ', 
                               label_name," ± 95% C.I.", sep = ""), main='Background Noise',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Signal to Noise
x <- 1:(length(names))*2-1
label_name <- "SNR"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_SNR")
SNR <- statistics[a, 2:length(statistics)]
SNR <- as.numeric(as.vector(SNR))

a <- which(statistics[,1]=="CI_SNR")
SNR_CI <- statistics[a, 2:length(statistics)]
SNR_CI <- as.numeric(as.vector(SNR_CI))

CI.up <- SNR + SNR_CI
CI.dn <- SNR - SNR_CI

plot(SNR ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='Signal to Noise',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Activity
x <- 1:(length(names))*2-1
label_name <- "ACT"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_ACT")
ACT <- statistics[a, 2:length(statistics)]
ACT <- as.numeric(as.vector(ACT))

a <- which(statistics[,1]=="CI_ACT")
ACT_CI <- statistics[a, 2:length(statistics)]
ACT_CI <- as.numeric(as.vector(ACT_CI))

CI.up <- ACT + ACT_CI
CI.dn <- ACT - ACT_CI

plot(ACT ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', label_name," ± 95% C.I.", sep = ""), 
     main='Activity',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Events per second
x <- 1:(length(names))*2-1
label_name <- "EVN"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_EVN")
EVN <- statistics[a, 2:length(statistics)]
EVN <- as.numeric(as.vector(EVN))

a <- which(statistics[,1]=="CI_EVN")
EVN_CI <- statistics[a, 2:length(statistics)]
EVN_CI <- as.numeric(as.vector(EVN_CI))

CI.up <- EVN + EVN_CI
CI.dn <- EVN - EVN_CI

plot(EVN ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='Events per second',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# High Frequency Cover
x <- 1:(length(names))*2-1
label_name <- "HFC"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_HFC")
HFC <- statistics[a, 2:length(statistics)]
HFC <- as.numeric(as.vector(HFC))

a <- which(statistics[,1]=="CI_HFC")
HFC_CI <- statistics[a, 2:length(statistics)]
HFC_CI <- as.numeric(as.vector(HFC_CI))

CI.up <- HFC + HFC_CI
CI.dn <- HFC - HFC_CI

plot(HFC ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='High Frequency Cover',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Mid Frequency Cover
x <- 1:(length(names))*2-1
label_name <- "MFC"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_MFC")
MFC <- statistics[a, 2:length(statistics)]
MFC <- as.numeric(as.vector(MFC))

a <- which(statistics[,1]=="CI_MFC")
MFC_CI <- statistics[a, 2:length(statistics)]
MFC_CI <- as.numeric(as.vector(MFC_CI))

CI.up <- MFC + MFC_CI
CI.dn <- MFC - MFC_CI

plot(MFC ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='Mid Frequency Cover',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Low Frequency Cover
x <- 1:(length(names))*2-1
label_name <- "LFC"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_LFC")
LFC <- statistics[a, 2:length(statistics)]
LFC <- as.numeric(as.vector(LFC))

a <- which(statistics[,1]=="CI_LFC")
LFC_CI <- statistics[a, 2:length(statistics)]
LFC_CI <- as.numeric(as.vector(LFC_CI))

CI.up <- LFC + LFC_CI
CI.dn <- LFC - LFC_CI

plot(LFC ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='Low Frequency Cover',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Acoustic Complexity
x <- 1:(length(names))*2-1
label_name <- "ACI"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_ACI")
ACI <- statistics[a, 2:length(statistics)]
ACI <- as.numeric(as.vector(ACI))

a <- which(statistics[,1]=="CI_ACI")
ACI_CI <- statistics[a, 2:length(statistics)]
ACI_CI <- as.numeric(as.vector(ACI_CI))

CI.up <- ACI + ACI_CI
CI.dn <- ACI - ACI_CI

plot(ACI ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='Acoustic Complexity Index',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Entropy of Average Spectrum
x <- 1:(length(names))*2-1
label_name <- "EAS"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_EAS")
EAS <- statistics[a, 2:length(statistics)]
EAS <- as.numeric(as.vector(EAS))

a <- which(statistics[,1]=="CI_EAS")
EAS_CI <- statistics[a, 2:length(statistics)]
EAS_CI <- as.numeric(as.vector(EAS_CI))

CI.up <- EAS + EAS_CI
CI.dn <- EAS - EAS_CI

plot(EAS ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species',ylab=paste('Normalised ', 
                               label_name," ± 95% C.I.", sep = ""), 
     main="Entropy of Average Spectrum",
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Entropy of Peaks Spectrum
x <- 1:(length(names))*2-1
label_name <- "EPS"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_EPS")
EPS <- statistics[a, 2:length(statistics)]
EPS <- as.numeric(as.vector(EPS))

a <- which(statistics[,1]=="CI_EPS")
EPS_CI <- statistics[a, 2:length(statistics)]
EPS_CI <- as.numeric(as.vector(EPS_CI))

CI.up <- EPS + EPS_CI
CI.dn <- EPS - EPS_CI

plot(EPS ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='Entropy of Peak Spectrum',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Entropy of the spectrum of Coefficient of Variation 
x <- 1:(length(names))*2-1
label_name <- "ECS"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_ECS")
ECS <- statistics[a, 2:length(statistics)]
ECS <- as.numeric(as.vector(ECS))

a <- which(statistics[,1]=="CI_ECS")
ECS_CI <- statistics[a, 2:length(statistics)]
ECS_CI <- as.numeric(as.vector(ECS_CI))

CI.up <- ECS + ECS_CI
CI.dn <- ECS - ECS_CI

plot(ECS ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='Entropy of the spectrum of Coefficient of Variation',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()

# Cluster Count
x <- 1:(length(names))*2-1
label_name <- "CLC"
tiff(paste("Species_acoustic_Indices",label_name,".tiff",sep=""),
     height = 750, width = 850)
par(cex=1.6, mar=c(3,3,2,1), mgp=c(2,0.8,0))
a <- which(statistics[,1]=="AV_CLC")
CLC <- statistics[a, 2:length(statistics)]
CLC <- as.numeric(as.vector(CLC))

a <- which(statistics[,1]=="CI_CLC")
CLC_CI <- statistics[a, 2:length(statistics)]
CLC_CI <- as.numeric(as.vector(CLC_CI))

CI.up <- CLC + CLC_CI
CI.dn <- CLC - CLC_CI

plot(CLC ~ x, cex=1.5, xaxt='n', ylim=c(0,1), 
     xlab='Species', ylab=paste('Normalised ', 
                                label_name," ± 95% C.I.", sep = ""), 
     main='Cluster Count',
     col='blue',pch=16, las=1)
axis(1, at=x, labels=names)
arrows(x, CI.dn, x, CI.up, code=3, length=0.2, angle=90, col='red')
dev.off()
#-------------------
data <- read.csv("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\species_each_minute_405_protected.csv", header=T)
#View(data)
min <- NULL
max <- NULL
data <- data.frame(data)
a <- which(data$count_all_species..mod.and.near.==0)
length(a)
data_temp <- data[a,]
mean(data_temp$AcousticComplexity)
sd(data_temp$AcousticComplexity)
boxplot(data_temp$AcousticComplexity)
minn <- min(data_temp$AcousticComplexity)
maxx <- max(data_temp$AcousticComplexity)
min <- c(min, minn)
max <- c(max, maxx)

a <- which(data$count_all_species..mod.and.near.==1)
length(a)
data_temp <- data[a,]
mean(data_temp$AcousticComplexity)
sd(data_temp$AcousticComplexity)
boxplot(data_temp$AcousticComplexity, ylim=c(0,1))
min(data_temp$AcousticComplexity)
max(data_temp$AcousticComplexity)
minn <- min(data_temp$AcousticComplexity)
maxx <- max(data_temp$AcousticComplexity)
min <- c(min, minn)
max <- c(max, maxx)

a <- which(data$count_all_species..mod.and.near.==2)
length(a)
data_temp <- data[a,]
mean(data_temp$AcousticComplexity)
sd(data_temp$AcousticComplexity)
boxplot(data_temp$AcousticComplexity, ylim=c(0,1))
min(data_temp$AcousticComplexity)
max(data_temp$AcousticComplexity)
minn <- min(data_temp$AcousticComplexity)
maxx <- max(data_temp$AcousticComplexity)
min <- c(min, minn)
max <- c(max, maxx)

a <- which(data$count_all_species..mod.and.near.==3)
length(a)
data_temp <- data[a,]
mean(data_temp$AcousticComplexity)
sd(data_temp$AcousticComplexity)
boxplot(data_temp$AcousticComplexity, ylim=c(0,1))
min(data_temp$AcousticComplexity)
max(data_temp$AcousticComplexity)
minn <- min(data_temp$AcousticComplexity)
maxx <- max(data_temp$AcousticComplexity)
min <- c(min, minn)
max <- c(max, maxx)

a <- which(data$count_all_species..mod.and.near.==4)
length(a)
data_temp <- data[a,]
mean(data_temp$AcousticComplexity)
sd(data_temp$AcousticComplexity)
boxplot(data_temp$AcousticComplexity, ylim=c(0,1))
minn <- min(data_temp$AcousticComplexity)
maxx <- max(data_temp$AcousticComplexity)
min <- c(min, minn)
max <- c(max, maxx)

a <- which(data$count_all_species..mod.and.near.==5)
length(a)
data_temp <- data[a,]
mean(data_temp$AcousticComplexity)
sd(data_temp$AcousticComplexity)
t <- t.test(data_temp$AcousticComplexity)
boxplot(data_temp$AcousticComplexity, ylim=c(0,1))
minn <- min(data_temp$AcousticComplexity)
maxx <- max(data_temp$AcousticComplexity)
min <- c(min, minn)
max <- c(max, maxx)

data <- read.csv("C:\\Work2\\ACI mean sd and confidence interval.csv", header = T)
mean <- as.numeric(as.vector(data[2,2:7]))
ci <- as.numeric(as.vector(data[5,2:7]))
ci.up <- mean+ci
ci.dn <- mean-ci
x <- 1:(length(mean))*2-1 
tiff("ACI_verses_number_of_species.tiff", height = 400, width = 500)
plot(mean ~ x, cex=1.5, xaxt='n', ylim=c(0,1), xlab='Number of Species per minute', 
     ylab="Normalised ACI ± 95% C.I. & Min-Max", main='ACI verses number of species per minute',
     col='blue',pch=16, las=1)
par(cex.lab=1.8, cex=1, cex.axis=1)
names <- c("0", "1","2","3","4","5")
axis(1, at=x, labels=names, cex=1.2)
arrows(x, ci.dn, x, ci.up, code=3, length=0.2, angle=90, col='red')
arrows(x, min, x, max, code=3, length=0.2, angle=90, col='darkgreen')
dev.off()

# Species analysis ---------------------
data_sp <- read.csv("C:\\Work2\\Projects\\Twelve_,month_clustering\\Saving_dataset\\species_each_minute_protected_final_with_species_numbers.csv", header = T)
a <- which(data_sp$minute_reference < 406)
data_sp <- data_sp[a,]

boxplot(plot=0, x=1:7,y=0:1)
a <- which(data_sp$cluster_list==37)
data_sp_37 <- data_sp[a,]
mean <- mean(data_sp_37$AcousticComplexity)
min <- min(data_sp_37$AcousticComplexity)
max <- max(data_sp_37$AcousticComplexity)
plot(ylim=c(0,1), mean)
boxplot(data_sp_37$NumSpec, ylim=c(0,7), main="Cluster 37",
        ylab="Number of species")

a <- which(data_sp$cluster_list==11)
data_sp_11 <- data_sp[a,]
mean(data_sp_11$AcousticComplexity)
min(data_sp_11$AcousticComplexity)
max(data_sp_11$AcousticComplexity)
boxplot(data_sp_11$NumSpec, ylim=c(0,7), main="Cluster 11",
        ylab="Number of species")

a <- which(data_sp$cluster_list==58)
length(a)
data_sp_58 <- data_sp[a,]
mean(data_sp_58$AcousticComplexity)
min(data_sp_58$AcousticComplexity)
max(data_sp_58$AcousticComplexity)
boxplot(data_sp_58$NumSpec, ylim=c(0,7), main="Cluster 58",
        ylab="Number of species")


a <- which(data_sp$cluster_list==43)
length(a)
data_sp_43 <- data_sp[a,]
mean(data_sp_43$AcousticComplexity)
min(data_sp_43$AcousticComplexity)
max(data_sp_43$AcousticComplexity)
boxplot(data_sp_43$NumSpec, ylim=c(0,7), main="Cluster 43",
        ylab="Number of species")


a <- which(data_sp$cluster_list==15)
length(a)
data_sp_15 <- data_sp[a,]
mean(data_sp_15$AcousticComplexity)
min(data_sp_15$AcousticComplexity)
max(data_sp_15$AcousticComplexity)
boxplot(data_sp_15$NumSpec, ylim=c(0,7), main="Cluster 15",
        ylab="Number of species")

a <- which(data_sp$cluster_list==39)
length(a)
data_sp_39 <- data_sp[a,]
mean(data_sp_39$AcousticComplexity)
min(data_sp_29$AcousticComplexity)
max(data_sp_29$AcousticComplexity)
boxplot(data_sp_39$NumSpec, ylim=c(0,7), main="Cluster 39",
        ylab="Number of species")

a <- which(data_sp$cluster_list==39)
length(a)
data_sp_39 <- data_sp[a,]
mean(data_sp_39$AcousticComplexity)
min(data_sp_39$AcousticComplexity)
max(data_sp_39$AcousticComplexity)
boxplot(data_sp_39$NumSpec, ylim=c(0,7), main="Cluster 39",
        ylab="Number of species")

a <- which(data_sp$cluster_list==3)
length(a)
data_sp_3 <- data_sp[a,]
mean(data_sp_3$AcousticComplexity)
min(data_sp_3$AcousticComplexity)
max(data_sp_3$AcousticComplexity)
boxplot(data_sp_3$NumSpec, ylim=c(0,7), main="Cluster 3",
        ylab="Number of species")

a <- which(data_sp$cluster_list==33)
length(a)
data_sp_33 <- data_sp[a,]
mean(data_sp_33$AcousticComplexity)
min(data_sp_33$AcousticComplexity)
max(data_sp_33$AcousticComplexity)
boxplot(data_sp_33$NumSpec, ylim=c(0,7), main="Cluster 33",
        ylab="Number of species")

boxplot(data_sp_37$NumSpec, data_sp_11$NumSpec,
        data_sp_15$NumSpec, data_sp_43$NumSpec,
        data_sp_58$NumSpec, data_sp_39$NumSpec,
        data_sp_3$NumSpec, data_sp_33$NumSpec,
        ylab="Number of Species", cex=1.2)
labels <- c("Clust 37", "Clust 11", 
            "Clust 15", "Clust 43", 
            "Clust 58", "Clust 39",
            "Clust 3", "Clust 33")
axis(side = 1, at = 1:8, labels=labels)

a <- which(data_sp$NumSpec==1)
data_sp_1 <- data_sp[a,]
mean(data_sp_1$AcousticComplexity)
min(data_sp_1$AcousticComplexity)
max(data_sp_1$AcousticComplexity)
a <- which(data_sp_1$Rain2 > 0)
data_sp_1_wo_rain <- data_sp_1[-a,]
mean(data_sp_1_wo_rain$AcousticComplexity)
min(data_sp_1_wo_rain$AcousticComplexity)
max(data_sp_1_wo_rain$AcousticComplexity)

a <- which(data_sp$NumSpec==2)
data_sp_2 <- data_sp[a,]
mean(data_sp_2$AcousticComplexity)
min(data_sp_2$AcousticComplexity)
max(data_sp_2$AcousticComplexity)
a <- which(data_sp_2$Rain2 > 0)
if(length(a) > 0) {
  data_sp_2_wo_rain <- data_sp_2[-a,]  
  mean(data_sp_2_wo_rain$AcousticComplexity)
  min(data_sp_2_wo_rain$AcousticComplexity)
  max(data_sp_2_wo_rain$AcousticComplexity)
}

a <- which(data_sp$NumSpec==3)
data_sp_3 <- data_sp[a,]
mean(data_sp_3$AcousticComplexity)
min(data_sp_3$AcousticComplexity)
max(data_sp_3$AcousticComplexity)
a <- which(data_sp_3$Rain2 > 0)
if(length(a) > 0) {
  data_sp_3_wo_rain <- data_sp_3[-a,]  
  mean(data_sp_3_wo_rain$AcousticComplexity)
  min(data_sp_3_wo_rain$AcousticComplexity)
  max(data_sp_3_wo_rain$AcousticComplexity)
}

a <- which(data_sp$NumSpec==4)
data_sp_4 <- data_sp[a,]
mean(data_sp_4$AcousticComplexity)
min(data_sp_4$AcousticComplexity)
max(data_sp_4$AcousticComplexity)
a <- which(data_sp_4$Rain2 > 0)
if(length(a) > 0) {
  data_sp_4_wo_rain <- data_sp_4[-a,]  
  mean(data_sp_4_wo_rain$AcousticComplexity)
  min(data_sp_4_wo_rain$AcousticComplexity)
  max(data_sp_4_wo_rain$AcousticComplexity)
}

a <- which(data_sp$NumSpec==5)
data_sp_5 <- data_sp[a,]
mean(data_sp_5$AcousticComplexity)
min(data_sp_5$AcousticComplexity)
max(data_sp_5$AcousticComplexity)
a <- which(data_sp_5$Rain2 > 0)
if(length(a) > 0) {
  data_sp_5_wo_rain <- data_sp_5[-a,]  
  mean(data_sp_5_wo_rain$AcousticComplexity)
  min(data_sp_5_wo_rain$AcousticComplexity)
  max(data_sp_5_wo_rain$AcousticComplexity)
}

a <- which(data_sp$NumSpec==6)
data_sp_5 <- data_sp[a,]
mean(data_sp_5$AcousticComplexity)
min(data_sp_5$AcousticComplexity)
max(data_sp_5$AcousticComplexity)
a <- which(data_sp_5$Rain2 > 0)
if(length(a) > 0) {
  data_sp_5_wo_rain <- data_sp_5[-a,]  
  mean(data_sp_5_wo_rain$AcousticComplexity)
  min(data_sp_5_wo_rain$AcousticComplexity)
  max(data_sp_5_wo_rain$AcousticComplexity)
}

# Plot of distribution of clusters------------------------
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
dates <- date.list
rm(date.list)

minute_list <- rep(1:1440, 56)
dates_56 <- rep(dates, each=1440)

# *** Set the cluster set variables
k1_value <- 25000
k2_value <- 60

cluster_list <- read.csv(paste("data/datasets/chosen_cluster_list_",
                               k1_value, "_", k2_value, ".csv", sep=""), header = T)
cluster_list <- cluster_list[1:(1440*56),]
cluster_list$dates <- dates_56
cluster_list <- cluster_list[,c(3,2,1)]
cluster_list$civ_dawn <- civil_dawn_2015$CivSunrise

# Convert civil dawn times to minutes
civ_dawn <- NULL
for(i in 1:56) {
  time <- as.character(civil_dawn_2015$CivSunrise[i])
  minutes <- as.numeric(substr(time,1,1))*60 + as.numeric(substr(time,2,3))
  civ_dawn <- c(civ_dawn, minutes)
}
civ_dawn <- rep(civ_dawn, each=1440)
cluster_list$civ_dawn_min <- civ_dawn
cluster_list$ref_civ <- 200
list <- c(3,11,14,15,37,39,43,58)
cluster_list$ref_civ <- cluster_list$minute_reference - cluster_list$civ_dawn_min
cluster_list$minute_reference <- cluster_list$minute_reference + 1
a <- which(cluster_list$minute_reference < 600)
cluster_list_temp <- cluster_list[a,]
a <- which(cluster_list_temp$cluster_list==37)
cluster_list_temp37 <- cluster_list_temp[a, ]

dev.off()
tiff("Distribution_of_clusters.tiff", 
     height=800, width=2200, res=300)
layout(matrix(c(1,1,1,1,1,1,1,1,1,1,
                2,2,2,2,2,2,2,2,2,2), 
              nrow = 20, ncol = 1, byrow = TRUE))
#layout.show(2)
par(mar=c(0,2,0,1), oma=c(3.8,3.5,0.2,0), cex.axis=1.8, cex=0.45)

list2 <- -55:35
list3 <- c(-25,-15,-5,5,15,25)

cbPalette <- c("#000000","#999999", "#56B4E9", 
               "#D55E00", "#0072B2", 
               "#CC79A7","#009E73","#E69F00")

# cluster 11
ylim <- c(0,35)
a <- which(cluster_list_temp$cluster_list==11)
cluster_list_temp11 <- cluster_list_temp[a, ]
counts_11 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp11$ref_civ==i)
  counts_11 <- c(counts_11, length(a))
}
x <- 1:length(list2)
y <- counts_11
lo <- loess(y~x, span=0.09)
plot(list2, counts_11, ylim=ylim, xlab="", ylab="", 
     xaxt="n", col=cbPalette[1], las=1, yaxt="n")
lines(list2, predict(lo), col=cbPalette[1], lwd=1.6)
abline(v=list3)
mtext(side=2, line=3.6, "Number of mintues                           ")
axis(at=c(10,20,30), side=2, las=1)
# cluster 3
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==3)
cluster_list_temp3 <- cluster_list_temp[a, ]
counts_3 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp3$ref_civ==i)
  counts_3 <- c(counts_3, length(a))
}
x <- 1:length(list2)
y <- counts_3
lo <- loess(y~x , span=0.12)
plot(list2, counts_3, ylim=ylim,xlab="", ylab="", 
     xaxt="n", col=cbPalette[2], yaxt="n")
lines(list2, predict(lo), col=cbPalette[2], lwd=1.6)
abline(v=list3)

# cluster 14
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==14)
cluster_list_temp14 <- cluster_list_temp[a, ]
counts_14 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp14$ref_civ==i)
  counts_14 <- c(counts_14, length(a))
}
x <- 1:length(list2)
y <- counts_14
lo <- loess(y~x, span=0.12)
plot(list2, counts_14, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=cbPalette[3], yaxt="n")
lines(list2, predict(lo), col=cbPalette[3], lwd=1.6)
abline(v=list3)

# cluster 15
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==15)
cluster_list_temp15 <- cluster_list_temp[a, ]
counts_15 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp15$ref_civ==i)
  counts_15 <- c(counts_15, length(a))
}
par(new=TRUE)
x <- 1:length(list2)
y <- counts_15
lo <- loess(y~x, span=0.12)
plot(list2, counts_15, ylim=ylim,xlab="", ylab="", 
     xaxt="n", col=cbPalette[4], yaxt="n")
lines(list2, predict(lo), col=cbPalette[4], lwd=1.6)
abline(v=list3)

# cluster 37
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==37)
cluster_list_temp37 <- cluster_list_temp[a, ]
counts_37 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp37$ref_civ==i)
  counts_37 <- c(counts_37, length(a))
}
x <- 1:length(list2)
y <- counts_37
lo <- loess(y~x, span=0.12)
par(new=TRUE)
plot(list2, counts_37, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=cbPalette[5], yaxt="n")
lines(list2, predict(lo), col=cbPalette[5], lwd=1.6)
abline(v=(0.5*length(list2)+0.5),lty=2, col="red")

# cluster 39
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==39)
cluster_list_temp39 <- cluster_list_temp[a, ]
counts_39 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp39$ref_civ==i)
  counts_39 <- c(counts_39, length(a))
}
x <- 1:length(list2)
y <- counts_39
lo <- loess(y~x, span=0.12)
plot(list2, counts_39, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=cbPalette[6], yaxt="n")
lines(list2, predict(lo), col=cbPalette[6], lwd=1.6)

# cluster 43
par(new=TRUE)
a <- which(cluster_list_temp$cluster_list==43)
cluster_list_temp43 <- cluster_list_temp[a, ]
counts_43 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp43$ref_civ==i)
  counts_43 <- c(counts_43, length(a))
}
x <- 1:length(list2)
y <- counts_43
lo <- loess(y~x, span=0.12)
plot(list2, counts_43, ylim=ylim,xlab="", ylab="",
     xaxt="n", col=cbPalette[7], yaxt="n")
lines(list2, predict(lo), col=cbPalette[7], lwd=1.6)
abline(v=0,lty=2, col="red")

label <- c("cluster  3","cluster 11","cluster 14","cluster 15",
            "cluster 37","cluster 39","cluster 43")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), col = c(cbPalette[1], cbPalette[2],
                                 cbPalette[3], cbPalette[4],
                                 cbPalette[5], cbPalette[6],
                                 cbPalette[7]),
       legend = label, cex = 1.8, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = 1,
       x.intersp = 0.9, y.intersp = 0.8, 
       inset=c(-0.15,0), lwd=1.2, 
       lty=1)

# plot 2
# cluster 58
ylim <- c(0,6)
a <- which(cluster_list_temp$cluster_list==58)
cluster_list_temp58 <- cluster_list_temp[a, ]
counts_58 <- NULL
for(i in list2) {
  a <- which(cluster_list_temp58$ref_civ==i)
  counts_58 <- c(counts_58, length(a))
}
x <- 1:length(list2)
y <- counts_58
lo <- loess(y~x, span=0.12)
plot(list2, counts_58, ylim=ylim,xlab="", ylab="",
     xaxt="n", yaxt="n", col="red", las=1)
lines(list2, predict(lo), col="red", lwd=1.6)
abline(v=list3)
axis(side=1, at=list3, 
     labels=c("-25","-15","-5", "+5", "+15", "+25"))
abline(v=0,lty=2, col="red")
mtext(side=1, line = 2.6, "Minutes from Civil Dawn", cex=1)
axis(at=c(2,4,6), side=2, las=1)

label <- c("cluster 58")
legend(x=-55, y=(ylim[2]+0.05*ylim[2]), col = c("red"),
       legend = label, cex = 1.8, bty = "n", 
       horiz = FALSE, xpd=TRUE, pch = 1,
       x.intersp = 0.9, y.intersp = 1.2, 
       inset=c(-0.15,0), lwd=1.2, lty=1)

dev.off()
#cumulative_total <- as.vector(lapply(seq_along(x), function(i) 
#                           counts_3[i]+counts_11[i]+counts_14[i]+counts_15[i]+counts_37[i]+counts_39[i]+counts_43[i]+counts_58[i]))
#counts_cumul <- NULL
#for(i in 1:length(counts_3)) {
#  count <- counts_3[i]+counts_11[i]+counts_14[i]+counts_15[i]+counts_37[i]+counts_39[i]+counts_43[i]+counts_58[i]
#  counts_cumul <- c(counts_cumul, count)
#}                           
#par(new=T)
#x <- 1:length(list2)
#y <- counts_cumul
#lo <- loess(y~x, span=0.09)
#plot(counts_cumul, ylim=c(0,52),xlab="", ylab="",
#     xaxt="n", col="black")
#lines(predict(lo), col="black", lwd=1.8, lty=2)

#------------------------------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work2\\Kaleidoscope\\20150621\\GympieNP\\Scarlet Honeyeater.csv", header = T)
all_data <- read.csv("all_data_added_protected.csv", header = T)[,c(1:21,37)]

start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20150816", format="%Y%m%d")
# Prepare civil dawn, civil dusk and sunrise and sunset times
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2015 <- civil_dawn_2015[173:228, ]

# Prepare dates
dates <- seq(start, finish, by = "1440 mins")
any(is.na(dates)) #FALSE
date.list <- NULL
for (i in 1:length(dates)) {
  dat <- substr(as.character(dates[i]),1,10)
  date.list <- c(date.list, dat)
}
dates <- date.list
rm(date.list)

start <-  strptime("20150622", format="%Y%m%d")
finish <- strptime("20150816", format="%Y%m%d")
# Prepare civil dawn, civil dusk and sunrise and sunset times
civil_dawn_2015 <- read.csv("data/Geoscience_Australia_Sunrise_times_Gympie_2015.csv")
civil_dawn_2015 <- civil_dawn_2015[173:228, ]
civil_sunrise <- as.numeric(substr(civil_dawn_2015$CivSunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise,2,3))
civsunrise <- as.numeric(substr(civil_dawn_2015$CivSunrise,1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise,2,3))
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

statistics$EYR_min <- statistics$EYR/60
statistics$WTT_min <- statistics$WTT/60
statistics$WTH_min <- statistics$WTH/60
statistics$KOOK_min <- statistics$KOOK/60
statistics$SC1_min <- statistics$SC1/60
statistics$SC2_min <- statistics$SC2/60
statistics$EW_min <- statistics$EW/60
statistics$civil_dawn <- civil_sunrise
statistics$sunrise <- sunrise

statistics$EYR_min_diff <- statistics$EYR_min - statistics$civil_dawn
statistics$WTT_min_diff <- statistics$WTT_min - statistics$civil_dawn
statistics$WTH_min_diff <- statistics$WTH_min - statistics$civil_dawn
statistics$KOOK_min_diff <- statistics$KOOK_min - statistics$civil_dawn
statistics$SC1_min_diff <- statistics$SC1_min - statistics$civil_dawn
statistics$SC2_min_diff <- statistics$SC2_min - statistics$civil_dawn
statistics$EW_min_diff <- statistics$EW_min - statistics$civil_dawn

# start plot-----------------------
dev.off()
tiff("Species_temporal_distribution_boxplot.tiff", 
     height=2400, width=2200, res=300)
#par(mfrow=c(7, 1), mar=c(0, 0.5, 0.5, 0.5), 
#    oma=c(16, 5, 16, 0), cex.axis=2, cex=0.8)
list2 <- -55:35
list3 <- c(-25,-15,-5,5,15,25)

layout(matrix(c(1,1,1,1,2,2,2,2,2,2,
                3,3,3,3,4,4,4,4,4,4,
                5,5,5,5,6,6,6,6,6,6,
                7,7,7,7,8,8,8,8,8,8,
                9,9,9,9,10,10,10,10,10,10,
                11,11,11,11,12,12,12,12,12,12,
                13,13,13,13,14,14,14,14,14,14), nrow = 70, ncol = 1, byrow = TRUE))
layout.show(14)
## show the regions that have been allocated to each plot
par(mar=c(0,2,0,1), oma=c(3.8,3.5,0.2,0), cex.axis=1.8, cex=0.45)

# Eastern Yellow Robin----------------------------------
#par(new=TRUE)
boxplot(statistics$EYR_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n",
        border="black")
med <- fivenum(statistics$EYR_min_diff)[c(1,3,5)]
text(x=med[1], y=0.66, as.character(round(med[1],0)), cex=1.4)
text(x=med[2], y=0.66, as.character(round(med[2],0)), cex=1.4)
text(x=med[3], y=0.66, as.character(round(med[3],0)), cex=1.4)
leg <- as.character.default("EYR")
abline(v=c(-25,-15,-5, 5, 15, 25))
label <- "EYR"
legend(x=(26-nchar(label)), y=1.5,
       legend=label, bty = "n", cex=2.2)

label_name <- "Eastern Yellow Robin"
list <- c("EYR Far", "EYR Mod", "EYR Near")
#list1 <- c("EYR Quiet", "EYR Mod", "EYR Loud")

kalscpe_data <- NULL
kalscpe_data_EYR <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_EYR <- rbind(kalscpe_data_EYR, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_EYR)) {
  dat <- paste(substr(kalscpe_data_EYR$IN.FILE[i],1,4), 
             substr(kalscpe_data_EYR$IN.FILE[i],5,6),
             substr(kalscpe_data_EYR$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_EYR$min[i] <- round(floor(kalscpe_data_EYR$OFFSET[i]/60), 0)
  kalscpe_data_EYR$ref_civ[i] <- kalscpe_data_EYR$min[i] - civ_dawn
}

# EYR Far
ylim <- c(0,420)
a <- which(kalscpe_data_EYR$V23=="EYR Far")
kalscpe_data_EYR_temp_far <- kalscpe_data_EYR[a, ]
counts_EYR_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EYR_temp_far$ref_civ==i)
  counts_EYR_Far <- c(counts_EYR_Far, length(a))
}
x <- 1:length(list2)
y <- counts_EYR_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_EYR_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n",  yaxt="n", col="black", las=2)
lines(list2, predict(lo), col='black', lwd=1.6)
axis(at=c(200,400), side=2, las=1)
abline(v=list3)
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
#mtext(side=3, line=1.1, cex=1.4, "Number of 'calls' over the 56 days in each minute")
par(new=TRUE)

# EYR Mod
a <- which(kalscpe_data_EYR$V23=="EYR Mod")
kalscpe_data_EYR_temp_mod <- kalscpe_data_EYR[a, ]
counts_EYR_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EYR_temp_mod$ref_civ==i)
  counts_EYR_Mod <- c(counts_EYR_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_EYR_Mod
lo <- loess(y~x , span=0.09)
plot(list2,counts_EYR_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n",yaxt="n", col="darkgreen", las=1)
lines(list2, predict(lo), col='darkgreen', lwd=1.6)
abline(v=list3)
abline(v=0,lty=2, col="red")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
# EYR Near
par(new=TRUE)
a <- which(kalscpe_data_EYR$V23=="EYR Near")
kalscpe_data_EYR_temp_near <- kalscpe_data_EYR[a, ]
counts_EYR_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EYR_temp_near$ref_civ==i)
  counts_EYR_Near <- c(counts_EYR_Near, length(a))
}
x <- 1:length(list2)
y <- counts_EYR_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_EYR_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="red", las=1)
lines(list2, predict(lo), col='red', lwd=1.6)
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))

# White-throated Honeyeater-------------------------
boxplot(statistics$WTH_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$WTH_min_diff)[c(1,3,5)]
text(x=med[1], y=0.66, as.character(round(med[1],0)), cex=1.4)
text(x=med[2], y=0.66, as.character(round(med[2],0)), cex=1.4)
text(x=med[3], y=0.66, as.character(round(med[3],0)), cex=1.4)
leg <- as.character.default("WTH")
abline(v=c(-25,-15,-5, 5, 15, 25))
label <- "WTH"
legend(x=(26-nchar(label)), y=1.5,
       legend=label, bty = "n", cex=2.2)

ylim <- c(0,650)
label_name <- "White-throated Honeyeater"
list <- c("WTH Far", "WTH Mod", "WTH Near")
list1 <- c("WTH Quiet", "WTH Mod", "WTH Loud")

kalscpe_data <- NULL
kalscpe_data_WTH <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_WTH <- rbind(kalscpe_data_WTH, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_WTH)) {
  dat <- paste(substr(kalscpe_data_WTH$IN.FILE[i],1,4), 
               substr(kalscpe_data_WTH$IN.FILE[i],5,6),
               substr(kalscpe_data_WTH$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_WTH$min[i] <- round(floor(kalscpe_data_WTH$OFFSET[i]/60), 0)
  kalscpe_data_WTH$ref_civ[i] <- kalscpe_data_WTH$min[i] - civ_dawn
}

# WTH Far
a <- which(kalscpe_data_WTH$V23=="WTH Far")
kalscpe_data_WTH_temp_far <- kalscpe_data_WTH[a, ]
counts_WTH_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTH_temp_far$ref_civ==i)
  counts_WTH_Far <- c(counts_WTH_Far, length(a))
}
x <- 1:length(list2)
y <- counts_WTH_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTH_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="black", las=1)
lines(list2, predict(lo), col='black', lwd=1.6)
abline(v=list3)
abline(v=0,lty=2, col="red")
axis(at=c(200,400, 600), side=2, las=1)

#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
par(new=TRUE)

# WTH Mod
a <- which(kalscpe_data_WTH$V23=="WTH Mod")
kalscpe_data_WTH_temp_mod <- kalscpe_data_WTH[a, ]
counts_WTH_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTH_temp_mod$ref_civ==i)
  counts_WTH_Mod <- c(counts_WTH_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_WTH_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTH_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="darkgreen", las=1)
lines(list2, predict(lo), col='darkgreen', lwd=1.6)
abline(v=list3)
abline(v=0,lty=2, col="red")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
# WTH Near
par(new=TRUE)
a <- which(kalscpe_data_WTH$V23=="WTH Near")
kalscpe_data_WTH_temp_near <- kalscpe_data_WTH[a, ]
counts_WTH_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTH_temp_near$ref_civ==i)
  counts_WTH_Near <- c(counts_WTH_Near, length(a))
}
x <- 1:length(list2)
y <- counts_WTH_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTH_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="red", las=1)
lines(list2, predict(lo), col='red', lwd=1.6)
abline(v=list3)
abline(v=0,lty=2, col="red")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))

# Laughining Kookaburra----------------------------
boxplot(statistics$KOOK_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$KOOK_min_diff)[c(1,3,5)]
text(x=med[1], y=0.66, as.character(round(med[1],0)), cex=1.4)
text(x=med[2], y=0.66, as.character(round(med[2],0)), cex=1.4)
text(x=med[3], y=0.66, as.character(round(med[3],0)), cex=1.4)
label <- as.character.default("KOOK")
abline(v=c(-25,-15,-5, 5, 15, 25))
legend(x=(26-nchar(label)), y=1.5,
       legend=label, bty = "n", cex=2.2)

ylim <- c(0,53)
ylim <- c(0,12)
label_name <- "Laughing Kookaburra"
list <- c("Kookaburra Quiet","Kookaburra Cackle", "Kookaburra Mod", "Kookaburra Loud")
#list1 <- c("KOOK Quiet","KOOK Mod", "KOOK Loud")

kalscpe_data <- NULL
kalscpe_data_KOOK <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_KOOK <- rbind(kalscpe_data_KOOK, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_KOOK)) {
  dat <- paste(substr(kalscpe_data_KOOK$IN.FILE[i],1,4), 
               substr(kalscpe_data_KOOK$IN.FILE[i],5,6),
               substr(kalscpe_data_KOOK$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_KOOK$min[i] <- round(floor(kalscpe_data_KOOK$OFFSET[i]/60), 0)
  kalscpe_data_KOOK$ref_civ[i] <- kalscpe_data_KOOK$min[i] - civ_dawn
}

# KOOK trill Far
a <- which(kalscpe_data_KOOK$V23=="Kookaburra Quiet")
kalscpe_data_KOOK_temp_far <- kalscpe_data_KOOK[a, ]
counts_KOOK_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_KOOK_temp_far$ref_civ==i)
  counts_KOOK_Far <- c(counts_KOOK_Far, length(a))
}
x <- 1:length(list2)
y <- counts_KOOK_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_KOOK_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="black", las=1)
lines(list2, predict(lo), col='black', lwd=1.6)
abline(v=list3)
abline(v=0, lty=2, col="red")
axis(at=c(5,10,20,25,30,40), side=2, las=1)
par(new=TRUE)

# Kookaburra Mod
a <- which(kalscpe_data_KOOK$V23=="Kookaburra Mod")
kalscpe_data_KOOK_temp_mod <- kalscpe_data_KOOK[a, ]
counts_KOOK_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_KOOK_temp_mod$ref_civ==i)
  counts_KOOK_Mod <- c(counts_KOOK_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_KOOK_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_KOOK_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="darkgreen", las=1)
lines(list2, predict(lo), col='darkgreen', lwd=1.6)
abline(v=list3)

# Kookaburra Loud
par(new=TRUE)
a <- which(kalscpe_data_KOOK$V23=="Kookaburra Loud")
kalscpe_data_KOOK_temp_near <- kalscpe_data_KOOK[a, ]
counts_KOOK_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_KOOK_temp_near$ref_civ==i)
  counts_KOOK_Near <- c(counts_KOOK_Near, length(a))
}
x <- 1:length(list2)
y <- counts_KOOK_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_KOOK_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n",  yaxt="n", col="red", las=1)
lines(list2, predict(lo), col='red', lwd=1.6)
abline(v=list3)
# Scarlet Honeyeater 1--------------------------------------
boxplot(statistics$SC1_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$SC1_min_diff)[c(1,3,5)]
text(x=med[1], y=0.66, as.character(round(med[1],0)), cex=1.4)
text(x=med[2], y=0.66, as.character(round(med[2],0)), cex=1.4)
text(x=med[3], y=0.66, as.character(round(med[3],0)), cex=1.4)
leg <- as.character.default("SC1")
abline(v=c(-25,-15,-5, 5, 15, 25))
label <- "SC1"
legend(x=(26-nchar(label)), y=1.5,
       legend=label, bty = "n", cex=2.2)

ylim <- c(0, 150)
label_name <- "Scarlet Honeyeater SC1"
list <- c("SC1 Far", "SC1 Mod", "SC1 Near")
#list1 <- c("SC1 Quiet", "SC1 Mod", "SC1 Loud")

kalscpe_data <- NULL
kalscpe_data_SC1 <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_SC1 <- rbind(kalscpe_data_SC1, kalscpe_data)
}

for(i in 1:nrow(kalscpe_data_SC1)) {
  dat <- paste(substr(kalscpe_data_SC1$IN.FILE[i],1,4), 
               substr(kalscpe_data_SC1$IN.FILE[i],5,6),
               substr(kalscpe_data_SC1$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_SC1$min[i] <- round(floor(kalscpe_data_SC1$OFFSET[i]/60), 0)
  kalscpe_data_SC1$ref_civ[i] <- kalscpe_data_SC1$min[i] - civ_dawn
}

# SC1 Far
a <- which(kalscpe_data_SC1$V23=="SC1 Far")
kalscpe_data_SC1_temp_far <- kalscpe_data_SC1[a, ]
counts_SC1_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC1_temp_far$ref_civ==i)
  counts_SC1_Far <- c(counts_SC1_Far, length(a))
}
x <- 1:length(list2)
y <- counts_SC1_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC1_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="black", las=1)
lines(list2, predict(lo), col='black', lwd=1.6)
abline(v=list3)
abline(v=0,lty=2, col="red")
axis(at=c(50,100, 150), side=2, las=1)

#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
par(new=TRUE)

# SC1 Mod
a <- which(kalscpe_data_SC1$V23=="SC1 Mod")
kalscpe_data_SC1_temp_mod <- kalscpe_data_SC1[a, ]
counts_SC1_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC1_temp_mod$ref_civ==i)
  counts_SC1_Mod <- c(counts_SC1_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_SC1_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC1_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="darkgreen", las=1)
lines(list2, predict(lo), col='darkgreen', lwd=1.6)
abline(v=list3)
abline(v=0,lty=2, col="red")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
# SC1 Near
par(new=TRUE)
a <- which(kalscpe_data_SC1$V23=="SC1 Near")
kalscpe_data_SC1_temp_near <- kalscpe_data_SC1[a, ]
counts_SC1_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC1_temp_near$ref_civ==i)
  counts_SC1_Near <- c(counts_SC1_Near, length(a))
}
x <- 1:length(list2)
y <- counts_SC1_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC1_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="red", las=1)
lines(list2, predict(lo), col='red', lwd=1.6)
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
mtext(side=2, cex=1, line=3.7,
      "Total number of 'calls' over 56 days")

# Scarlet Honeyeater 2-------------------
boxplot(statistics$SC2_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$SC2_min_diff)[c(1,3,5)]
text(x=med[1], y=0.66, as.character(round(med[1],0)), cex=1.4)
text(x=med[2], y=0.66, as.character(round(med[2],0)), cex=1.4)
text(x=med[3], y=0.66, as.character(round(med[3],0)), cex=1.4)
leg <- as.character.default("SC1")
abline(v=c(-25,-15,-5, 5, 15, 25))
label <- "SC2"
legend(x=(26-nchar(label)), y=1.5,
       legend=label, bty = "n", cex=2.2)

ylim <- c(0,50)
label_name <- "Scarlet Honeyeater SC2"
list <- c("SC3 Chatter Far", "SC3 Chatter Mod", "SC3 Chatter Near")
#list1 <- c("SC3 Chatter Quiet", "SC3 Chatter Mod", "SC3 Chatter Loud")
#colours <- c("grey70", "grey50", "grey0","grey20")
kalscpe_data <- NULL
kalscpe_data_SC2 <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_SC2 <- rbind(kalscpe_data_SC2, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_SC2)) {
  dat <- paste(substr(kalscpe_data_SC2$IN.FILE[i],1,4), 
               substr(kalscpe_data_SC2$IN.FILE[i],5,6),
               substr(kalscpe_data_SC2$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_SC2$min[i] <- round(floor(kalscpe_data_SC2$OFFSET[i]/60), 0)
  kalscpe_data_SC2$ref_civ[i] <- kalscpe_data_SC2$min[i] - civ_dawn
}

# SC2 Far
a <- which(kalscpe_data_SC2$V23=="SC3 Chatter Far")
kalscpe_data_SC2_temp_far <- kalscpe_data_SC2[a, ]
counts_SC2_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC2_temp_far$ref_civ==i)
  counts_SC2_Far <- c(counts_SC2_Far, length(a))
}
x <- 1:length(list2)
y <- counts_SC2_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC2_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="black", las=1)
lines(list2, predict(lo), col='black', lwd=1.6)
abline(v=list3)
abline(v=0, lty=2, col="red")
axis(at=c(20,40), side=2, las=1)

#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
par(new=TRUE)

# SC2 Mod
a <- which(kalscpe_data_SC2$V23=="SC3 Chatter Mod")
kalscpe_data_SC2_temp_mod <- kalscpe_data_SC2[a, ]
counts_SC2_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC2_temp_mod$ref_civ==i)
  counts_SC2_Mod <- c(counts_SC2_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_SC2_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC2_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="darkgreen", las=1)
lines(list2, predict(lo), col='darkgreen', lwd=1.6)
abline(v=list3)
# SC2 Near
par(new=TRUE)
a <- which(kalscpe_data_SC2$V23=="SC3 Chatter Near")
kalscpe_data_SC2_temp_near <- kalscpe_data_SC2[a, ]
counts_SC2_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_SC2_temp_near$ref_civ==i)
  counts_SC2_Near <- c(counts_SC2_Near, length(a))
}
x <- 1:length(list2)
y <- counts_SC2_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_SC2_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="red", las=1)
lines(list2, predict(lo), col='red', lwd=1.6)
abline(v=list3)
abline(v=0, lty=2, col="red")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))

# White-throated Treecreeper-------------------------------
boxplot(statistics$WTT_min_diff, horizontal=TRUE, 
        ylim=c(min(list2), max(list2)), xaxt="n")
med <- fivenum(statistics$WTT_min_diff)[c(1,3,5)]
text(x=med[1], y=0.66, as.character(round(med[1],0)), cex=1.4)
text(x=med[2], y=0.66, as.character(round(med[2],0)), cex=1.4)
text(x=med[3], y=0.66, as.character(round(med[3],0)), cex=1.4)
leg <- as.character.default("WTT")
abline(v=list3)
label <- "WTT"
legend(x=(26-nchar(label)), y=1.5,
       legend=label, bty = "n", cex=2.2)

label_name <- "White-throated Treecreeper"
list <- c("WTT trill Far", "WTT trill Mod", "WTT trill Near")
#list1 <- c("WTT trill Quiet", "WTT trill Mod", "WTT trill Loud")

kalscpe_data <- NULL
kalscpe_data_WTT <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_WTT <- rbind(kalscpe_data_WTT, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_WTT)) {
  dat <- paste(substr(kalscpe_data_WTT$IN.FILE[i],1,4), 
               substr(kalscpe_data_WTT$IN.FILE[i],5,6),
               substr(kalscpe_data_WTT$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_WTT$min[i] <- round(floor(kalscpe_data_WTT$OFFSET[i]/60), 0)
  kalscpe_data_WTT$ref_civ[i] <- kalscpe_data_WTT$min[i] - civ_dawn
}

# WTT trill Far
ylim <- c(0,260)
counts_WTT_Far <- NULL
a <- which(kalscpe_data_WTT$V23=="WTT trill Far")
kalscpe_data_WTT_temp_far <- kalscpe_data_WTT[a, ]
counts_WTT_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTT_temp_far$ref_civ==i)
  counts_WTT_Far <- c(counts_WTT_Far, length(a))
}
x <- list2
y <- counts_WTT_Far
lo <- loess(y~x, span=0.09)
plot(list2, counts_WTT_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="black", las=1)
axis(at=c(100,200), side=2, las=1)
lines(list2, predict(lo), col='black', lwd=1.6)
abline(v=list3)
abline(v=0,lty=2, col="red")
par(new=TRUE)

# WTT trill Mod
a <- which(kalscpe_data_WTT$V23=="WTT trill Mod")
kalscpe_data_WTT_temp_mod <- kalscpe_data_WTT[a, ]
counts_WTT_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTT_temp_mod$ref_civ==i)
  counts_WTT_Mod <- c(counts_WTT_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_WTT_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTT_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="darkgreen", las=1)
lines(list2, predict(lo), col='darkgreen', lwd=1.6)
abline(v=list3)
abline(v=0, lty=2, col="red")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))
# WTT trill Near
par(new=TRUE)
a <- which(kalscpe_data_WTT$V23=="WTT trill Near")
kalscpe_data_WTT_temp_near <- kalscpe_data_WTT[a, ]
counts_WTT_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_WTT_temp_near$ref_civ==i)
  counts_WTT_Near <- c(counts_WTT_Near, length(a))
}
x <- 1:length(list2)
y <- counts_WTT_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_WTT_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="red", las=1)
lines(list2, predict(lo), col='red', lwd=1.6)
abline(v=list3)
abline(v=0, lty=2, col="red")
#axis(side=1, at=list3, 
#     labels=c("-25","-15","-5", "+5", "+15", "+25"))

# Eastern Whipbird---------------------------------
boxplot(statistics$EW_min_diff, horizontal=TRUE, 
        ylim=c(min(list2),max(list2)), xaxt="n")
med <- fivenum(statistics$EW_min_diff)[c(1,3,5)]
text(x=med[1], y=0.66, as.character(round(med[1],0)), cex=1.4)
text(x=med[2], y=0.66, as.character(round(med[2],0)), cex=1.4)
text(x=med[3], y=0.66, as.character(round(med[3],0)), cex=1.4)
abline(v=c(-25,-15,-5, 5, 15, 25))
label <- "EW"
legend(x=(26-nchar(label)), y=1.5,
       legend=label, bty = "n", cex=2.2)

ylim <- c(0,150)
label_name <- "Eastern Whipbird"
list <- c("EW Far", "EW Mod", "EW Near")
list1 <- c("EW Quiet", "EW Mod", "EW Loud")

kalscpe_data <- NULL
kalscpe_data_EW <- NULL
for(i in 1:length(list)) {
  label <- list[i]
  a <- grep(label, all_data$MANUAL_ID, ignore.case = T)
  kalscpe_data <- data2[a,]
  kalscpe_data[ ,23] <- label
  col_names <- colnames(kalscpe_data)
  b <- which(col_names=="MANUAL.ID.")
  col_names[b] <- "MANUAL.ID"
  colnames(kalscpe_data) <- col_names
  # Next line is only needed for species_histogram plots
  kalscpe_data_EW <- rbind(kalscpe_data_EW, kalscpe_data)
}
for(i in 1:nrow(kalscpe_data_EW)) {
  dat <- paste(substr(kalscpe_data_EW$IN.FILE[i],1,4), 
               substr(kalscpe_data_EW$IN.FILE[i],5,6),
               substr(kalscpe_data_EW$IN.FILE[i],7,8),sep="-")
  a <- which(civil_dawn_2015$dates==dat)
  civ_dawn <- (as.numeric(substr(civil_dawn_2015$CivSunrise[a],1,1))*60 
               + as.numeric(substr(civil_dawn_2015$CivSunrise[a],2,3)))
  kalscpe_data_EW$min[i] <- round(floor(kalscpe_data_EW$OFFSET[i]/60), 0)
  kalscpe_data_EW$ref_civ[i] <- kalscpe_data_EW$min[i] - civ_dawn
}

# EW Far
a <- which(kalscpe_data_EW$V23=="EW Far")
kalscpe_data_EW_temp_far <- kalscpe_data_EW[a, ]
counts_EW_Far <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EW_temp_far$ref_civ==i)
  counts_EW_Far <- c(counts_EW_Far, length(a))
}
x <- 1:length(list2)
y <- counts_EW_Far
lo <- loess(y~x , span=0.09)
plot(list2, counts_EW_Far, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="black", las=1)
mtext(side=1, line = 2.6, "Minutes from Civil Dawn", cex=1)
lines(list2, predict(lo), col='black', lwd=1.6)
abline(v=list3)
abline(v=0,lty=2, col="red")
axis(at=c(50,100, 150), side=2, las=1)
par(new=TRUE)

# EW Mod
a <- which(kalscpe_data_EW$V23=="EW Mod")
kalscpe_data_EW_temp_mod <- kalscpe_data_EW[a, ]
counts_EW_Mod <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EW_temp_mod$ref_civ==i)
  counts_EW_Mod <- c(counts_EW_Mod, length(a))
}
x <- 1:length(list2)
y <- counts_EW_Mod
lo <- loess(y~x , span=0.09)
plot(list2, counts_EW_Mod, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="darkgreen", las=1)
lines(list2, predict(lo), col='darkgreen', lwd=1.6)
abline(v=list3)

# EW Near
par(new=TRUE)
a <- which(kalscpe_data_EW$V23=="EW Near")
kalscpe_data_EW_temp_near <- kalscpe_data_EW[a, ]
counts_EW_Near <- NULL
for(i in list2) {
  a <- which(kalscpe_data_EW_temp_near$ref_civ==i)
  counts_EW_Near <- c(counts_EW_Near, length(a))
}
x <- 1:length(list2)
y <- counts_EW_Near
lo <- loess(y~x , span=0.09)
plot(list2, counts_EW_Near, ylim=ylim, xlab="", ylab="", 
     xaxt="n", yaxt="n", col="red", las=1)
lines(list2, predict(lo), col='red', lwd=1.6)
abline(v=list3)

mtext(side=1,at=list3, line=1, cex=1,
      text = c("-25","-15","-5", "+5", "+15", "+25"))
dev.off()