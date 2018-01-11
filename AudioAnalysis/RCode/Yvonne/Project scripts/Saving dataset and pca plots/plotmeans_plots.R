# Author:  Yvonne Phillips
# Date: 6 February 2017
# This code takes the 8 one-minute counts from three periods
# around civil-dawn and draws a plot with error bars of 
# 95% confidence intervals

# This code also plots the occurance of bird calls onto 
# a sunrise plot.  Use Shift-Alt-J to find this

Gym_birds <- read.csv("plotmeans_plots\\Gympie_birds_Civil_dawn_final.csv", header = T)
Woon_birds <- read.csv("plotmeans_plots\\Woondum_birds_civil_dawn_final.csv", header = T)
sites <- c("GympieNP", "WoondumNP")


library(gplots)
labels <- c("Jul", "Aug", "Sep", "Oct", "Nov",
            "Dec", "Jan", "Feb", "Mar", "Apr",
            "May", "Jun")


plot_civil_dawn_single <- function(site, bird_name, ylim, 
                            labels, column, list, 
                            adjust, number) {
    n <- 0
    if(site=="GympieNP") {
      plotmeans(Gym_birds[(n*96+1):(96*(n+1)), column] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
                data = Gym_birds, connect = T, 
                n.label = F, minbar = 0, ylim = ylim, 
                xaxt = "n", mgp = c(3, 0.3, 0), ylab = "", xlab = "",
                las=1, lwd = 2.4)
      abline(v=6.5, lty=2)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      mtext(side = 2, line = 1, cex = 2.6,
            "Number of calls per minute +/- 95% C.I.")
    }
    if(site=="WoondumNP") {
      plotmeans(Woon_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Woon_birds$comb[(n*96+1):(96*(n+1))], 
                data = Woon_birds, connect = T,
                mgp = c(3, 0.3, 0), ylab = "", xlab = "",
                n.label = F, minbar = 0, ylim = ylim, 
                xaxt = "n", las=1, lwd = 2.4)  
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
                ylab = "", xlab = "",
                mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4)
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
                ylab = "", xlab = "",
                mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4)
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
                ylab = "", xlab = "",
                mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4) 
      abline(v=6.5, lty=2, lwd = 1)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
    }
    if(site=="WoondumNP") {
      plotmeans(Woon_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Woon_birds$comb[(n*96+1):(96*(n+1))], 
                data = Woon_birds, connect = T, minbar = 0,
                ylim = ylim, xaxt = "n", yaxt = "n",
                ylab = "", xlab = "",
                mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4)  
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
              ylab = "", xlab = "",
              n.label = F, minbar = 0, ylim = ylim, 
              xaxt = "n", mgp = c(3, 0.3, 0),
              las=1, lwd = 2.4)
    abline(v=6.5, lty=2)
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    mtext(side = 2, line = 1, cex = 2.6,
          "Number of calls per minute +/- 95% C.I.")
    par(new=T)
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, 
              ylab = "", xlab = "",
              n.label = F, minbar = 0, ylim = ylim, 
              xaxt = "n", mgp = c(3, 0.3, 0), lty = 2,
              las=1, lwd = 2.4)
  }
  if(site=="WoondumNP") {
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T,
              mgp = c(3, 0.3, 0), ylab = "", xlab = "",
              n.label = F, minbar = 0, ylim = ylim, 
              xaxt = "n", las=1, lwd = 2.4)  
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
              ylab = "", xlab = "",
              n.label = F, minbar = 0, ylim = ylim, 
              xaxt = "n", las=1, lwd = 2.4)
  }
  text(x = 10, y = (ylim[2]-adjust), "Period A (pre-civil dawn)", 
       cex = 1.1)
  n <- 1
  if(site=="GympieNP") {
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, minbar = 0,
              ylim = ylim, xaxt = "n", yaxt = "n",
              ylab = "", xlab = "",
              mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4)
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
              ylab = "", xlab = "",
              ylim = ylim, xaxt = "n", yaxt = "n", lty = 2,
              mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4)
  }
  if(site=="WoondumNP") {
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T, minbar = 0,
              ylab = "", xlab = "",
              ylim = ylim, xaxt = "n", yaxt = "n",
              mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4)
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
              ylab = "", xlab = "",
              ylim = ylim, xaxt = "n", yaxt = "n", lty = 2,
              mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4)
    
  }
  n <- 2
  if(site=="GympieNP") {
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, minbar = 0,
              ylab = "", xlab = "",
              ylim = ylim, xaxt = "n", yaxt = "n",
              mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4) 
    abline(v=6.5, lty=2, lwd = 1)
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    par(new=T)
    plotmeans(Gym_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
              data = Gym_birds, connect = T, minbar = 0,
              ylab = "", xlab = "",
              ylim = ylim, xaxt = "n", yaxt = "n", lty = 2,
              mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4) 
    legend(x = 7.2, y = (ylim[2]-1.3*adjust), lty = c(1,2), 
           legend = c(legend[1], legend[2]), 
           lwd = 2.4, cex = 1.2)
  }
  if(site=="WoondumNP") {
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[1]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T, minbar = 0,
              ylab = "", xlab = "",
              ylim = ylim, xaxt = "n", yaxt = "n",
              mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4)  
    axis(side = 1, at = 1:12, tick = T, labels = labels,
         mgp = c(3, 0.3, 0))
    par(new=T)
    plotmeans(Woon_birds[(n*96+1):(96*(n+1)), number[2]] ~ 
                Woon_birds$comb[(n*96+1):(96*(n+1))], 
              data = Woon_birds, connect = T, minbar = 0,
              ylab = "", xlab = "",
              ylim = ylim, xaxt = "n", yaxt = "n", lty = 2,
              mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4)
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
number <- 18
ylim_max <- 3
legend <- c("Whistled Notes", "Chatter")
for(i in number[1]) {    #4:length(list)) {
  column <- i
  bird_name <- list[i]
  ylim <- c(0, ylim_max)
  tiff(paste("plotmeans_plots\\test",bird_name," ",sites[1],
             "_", sites[2], ".tiff", sep = ""), 
       width=3400, height=1800)
  par(mfrow=c(2,3), mar=c(2,0,0,0), oma = c(1,2,1,1),
      cex = 2.6, cex.lab = 2.6)
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
dev.off()

source("scripts/Bird_lists_func.R")
colours <- c("red","darkgreen","yellow","orange","skyblue1",
             "seashell3","darkorchid1","hotpink","green","mediumblue","cyan")
library(scales)
dev.off()
show_col(colours)
dev.off()
birds <- c(3,11,14,15,33,37,39,43,58)
birds1 <- c(11,15,37,43,58)
dev.off()
bird_cluster_plot(birds, colours, 20)
dev.off()
bird_cluster_plot(birds1, colours, 20)
dev.off()
bird_cluster_plot(c(11,37), colours, 20)
dev.off()

bird_cluster_plot(c(11,37,43), colours, 20)
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
xlim=c(240, 405) # time
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
a <- which(list=="Lewins not sure")
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
data2 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\White-throated Honeyeater.csv", header = T)
label_name <- "White-throated Honeyeater"
dev.off()
list <- NULL
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
# Scarlet Honeyeater -----------------------------------------
rm(list = ls())
source("scripts/Bird_lists_func.R")
indices_norm_summary <- cluster_list[,4:15]
data2 <- read.csv("C:\\Work\\Kaleidoscope\\20150621\\GympieNP\\Scarlet Honeyeater.csv", header = T)
label_name <- "Scarlet Honeyeater"
n <- 5
descriptor <- "good"
descriptor1 <- "mod"
dev.off()
list <- NULL
list <- unique(data2$MANUAL.ID)
list <- sort(list)
a <- which(list=="-")
list <- list[-a]
a <- which(list=="")
list <- list[-a]
list

a <- grep("Grey Fantail", list, ignore.case = T)
if(length(a) > 0) {
  list <- list[-a]
}
a <- grep("Rufous Fantail", list, ignore.case = T)
if(length(a) > 0) {
  list <- list[-a]
}
a <- grep("White-throated Honeyeater", list, ignore.case = T)
if(length(a) > 0) {
  list <- list[-a]
}
a <- grep("distant", list, ignore.case = T)
if(length(a) > 0) {
  list <- list[-a]
}

#a <- grep("good", list, ignore.case = T)
#list <- list[a]

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

# check that all files have the rows in a consistent order
list <- 1:17
check <- NULL
for(j in 1:(length(list)-1)) {
  file_a <- get(paste("file", list[j],sep = ""))
  file_b <- get(paste("file", list[j+1],sep = ""))
  for(i in 1:nrow(file_a)) {
    ifelse(file_a[i,4] == file_b[i,4], eq_yes <- "YES", eq_no <- "NO")
    #print(i)
  }
  print(paste("Finished checking file", j))
  c1 <- which(a=="NO")
  check <- c(check, length(c1))
  if(length(c1) > 0) break
}
check
if(sum(check) > 0) {
  print("WARNING: files do not have consisent order of rows")
}


all_data <- cbind(file1, 
                  file2[,length(file2)], file3[,length(file3)],
                  file4[,length(file4)], file5[,length(file5)],
                  file6[,length(file6)], file7[,length(file7)],
                  file8[,length(file8)], file9[,length(file9)],
                  file10[,length(file10)], file11[,length(file11)],
                  file12[,length(file12)], file13[,length(file13)],
                  file14[,length(file10)], file15[,length(file11)],
                  file16[,length(file12)], file17[,length(file13)])

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

for(i in 1:nrow(all_data)) {
  all_data$MANUAL_ID[i] <- paste(all_data$`Scarlet Honeyeater`[i],
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
  print(i)  
}

write.csv(all_data, "all_data_added.csv", row.names = F)

all_data <- read.csv("all_data_added_protected.csv", header = T)[,c(1:21,39)]

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

dataset <- NULL
for(i in 1:nrow(all_data)) {
  dataset$date[i] <- substr(all_data$IN.FILE[i],1,8)
  dataset$minute[i] <- all_data$minute[i]
}
uniq_dates  <- unique(dates)
dataset$cluster <- NULL
dataset <- data.frame(dataset)
for(k in 1:nrow(dataset)) {
  a <- which(uniq_dates==dataset$date[k])
  min_ref <- (a-1)*1440 + dataset$minute[k]
  dataset$cluster[k] <- Gym_cluster_list$cluster_list[min_ref]
}

all_data$cluster <- dataset$cluster
list1 <- 22:28
list <- NULL
for(i in 1:length(list1)) {
  lst <- as.character(unique(all_data[,list1[i]]))
  list <- c(list, lst)
}
a <- which(list=="-")
if(length(a) > 0) {
  list <- list[-a]  
}
a <- which(list=="")
if(length(a) > 0) {
  list <- list[-a]  
}
a <- which(list=="Rufous Fantail")
if(length(a) > 0) {
  list <- list[-a]  
}
a <- which(list=="Up note")
if(length(a) > 0) {
  list <- list[-a]  
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