# Author:  Yvonne Phillips
# Date: 6 February 2017
# This code takes the 8 one-minute counts from three periods
# around civil-dawn and draws a plot with error bars of 
# 95% confidence intervals

Gym_birds <- read.csv("plotmeans_plots\\Gympie_birds_Civil_dawn.csv", header = T)
Woon_birds <- read.csv("plotmeans_plots\\Woondum_birds_civil_dawn.csv", header = T)
sites <- c("GympieNP", "WoondumNP")

library(gplots)
labels <- c("Jul", "Aug", "Sep", "Oct", "Nov",
            "Dec", "Jan", "Feb", "Mar", "Apr",
            "May", "Jun")

plot_civil_dawn <- function(site, bird_name, ylim, 
                            labels, column, list, adjust) {
    n <- 0
    if(site=="GympieNP") {
      plotmeans(Gym_birds[(n*96+1):(96*(n+1)), column] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
                data = Gym_birds, connect = T, 
                n.label = F, minbar = 0, ylim = ylim, 
                xaxt = "n", mgp = c(3, 0.3, 0),
                las=1, lwd = 2.4)
      abline(v=6.5, lty=2)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      mtext(side = 2, line = 1, cex = 2.6,
            "Number of calls +/- 95% C.I.")
    }
    if(site=="WoondumNP") {
      plotmeans(Woon_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Woon_birds$comb[(n*96+1):(96*(n+1))], 
                data = Woon_birds, connect = T,
                mgp = c(3, 0.3, 0), 
                n.label = F, minbar = 0, ylim = ylim, 
                xaxt = "n", las=1, lwd = 2.4)  
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      abline(v=6.5, lty=2, lwd = 1)
      mtext(side = 2, line = 1, cex = 2.6,
            "Number of calls +/- 95% C.I.")
    }
    text(x = 10, y = (ylim[2]-adjust), "Pre-civil dawn", cex = 1.1)
    n <- 1
    if(site=="GympieNP") {
      plotmeans(Gym_birds[(n*96+1):(96*(n+1)), column] ~ 
                Gym_birds$comb[(n*96+1):(96*(n+1))], 
                data = Gym_birds, connect = T, minbar = 0,
                ylim = ylim, xaxt = "n", yaxt = "n",
                mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4)
      abline(v=6.5, lty=2, lwd = 1)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      text(x = 10, y = (ylim[2]-adjust), "Civil dawn", cex = 1.1)
      mtext(side = 3, paste(bird_name, site, sep = " - "), 
            cex = 2.6, line = -1.6)
    }
    if(site=="WoondumNP") {
      plotmeans(Woon_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Woon_birds$comb[(n*96+1):(96*(n+1))], 
                data = Woon_birds, connect = T, minbar = 0,
                ylim = ylim, xaxt = "n", yaxt = "n",
                mgp = c(3, 0.3, 0), n.label = F, lwd = 2.4)
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
      abline(v=6.5, lty=2, lwd = 1)
      text(x = 10, y = (ylim[2]-adjust), "Civil dawn", cex = 1.1)
      mtext(side = 3, paste(bird_name, site, sep = " - "), 
            cex = 2.6, line = -2.6)
    }
    
    
    n <- 2
    if(site=="GympieNP") {
      plotmeans(Gym_birds[(n*96+1):(96*(n+1)), column] ~ 
                  Gym_birds$comb[(n*96+1):(96*(n+1))], 
                data = Gym_birds, connect = T, minbar = 0,
                ylim = ylim, xaxt = "n", yaxt = "n",
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
                mgp = c(3, 0.3, 0),  n.label = F, lwd = 2.4)  
      axis(side = 1, at = 1:12, tick = T, labels = labels,
           mgp = c(3, 0.3, 0))
    }
    text(x = 10, y = (ylim[2]-adjust), "Post-civil dawn", cex = 1.2)
    abline(v=6.5, lty=2, lwd = 1)
}

list <- colnames(Gym_birds)  

#[1] "period_listing"               "month"                       
#[3] "comb"                         "Eastern.Yellow.Robin.song"   
#[5] "Eastern.Yellow.Robin.piping"  "Eastern.Yellow.Robin.3"      
#[7] "White.throated.Honeyeater"    "Eastern.Whipbird"            
#[9] "White.throated.Treecreeper"   "White.throated.Treecreeper.2"
#[11] "White.throated.Treecreeper.3" "Scarlet.Honeyeater"          
#[13] "Scarlet.Honeyeater.2"         "Scarlet.Honeyeater.3"        
#[15] "Lewin.s.Honeyeater"           "Australian.Magpie"           
#[17] "Torresian.Crow"               "Laughing.Kookaburra"         
#[19] "Fantailed.Cuckoo"             "Southern.Boobook"            
#[21] "Leaden.Flycatcher"            "Rufous.Whistler"             
#[23] "Rufous.Whistler.2"            "Rufous.Whistler.3"           
#[25] "Peaceful.Dove"                "Grey.Shrike.Thrush"          
#[27] "Sulfur.Crested.Cockatoo"      "White.throated.Nightjar"     
#[29] "Spectacled.Monarch"           "Brown.Cuckoo.Dove"           
#[31] "Australian.Brush.turkey"      "Pied.Currawong"              
#[33] "Cicadas"                      "Yellow.tailed.Black.Cockatoo"
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

ylim_max <- 22
number <- 8
for(i in  number) {    #4:length(list)) {
  column <- i
  bird_name <- list[i]
  ylim <- c(0, ylim_max)
  png(paste("plotmeans_plots\\",bird_name," ",sites[1],
            "_", sites[2], ".png", sep = ""), 
      width=2800, height=1600)
  par(mfrow=c(2,3), mar=c(0,0,0,0), oma = c(2,2,1,1),
      cex = 2.4)
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
      adjust <- 1
    }
    if(ylim[2] >= 11) {
      adjust <- 1.5
    }
    if(ylim[2] >= 15) {
      adjust <- 2.5
    }
    plot_civil_dawn(site, bird_name, ylim, labels, 
                    column, list, adjust)
  }
  dev.off()
}

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

