data14 <- read.csv("C:\\Work2\\Paper with Susan\\SERF_NE_14th_Oct_2010_0400_to_0900_all_annotations.csv", header=T)
data15 <- read.csv("C:\\Work2\\Paper with Susan\\SERF_NE_15th_Oct_2010_0400_to_0900_all_annotations.csv", header=T)

# Select minutes up to 5:26 am, this is 35 minutes
# after civil dawn
a <- which(substr(data14$StartTime1,1,1) < 6)
b <- which(substr(data15$StartTime1,1,1) < 6)

data14 <- data14[a,]
data15 <- data15[a,]

# Load minutes up to 5:26 am
#View(data14)
data14 <- data14[1:343,]
#View(data15)
data15 <- data15[1:331,]

data14_15 <- rbind(data14,data15)
# rename the Australian Wood Duck1 and Australian Wood Duck2
#so that it does not get confused with the Australasian Figbird4
a <- which(data14_15$Tag=="Australian Wood Duck1")
data14_15$Tag <- as.character(data14_15$Tag)
data14_15$Tag[a] <- "AustWoodDuck1"
a <- which(data14_15$Tag=="Australian Wood Duck2")
data14_15$Tag[a] <- "AustWoodDuck2"
# rename the "White-throated Honeyeater1" and "white-throated honeyeater2"
# and the "White-throated Honeyeater2" so that it does not get confused with the 
#  with the "White-throated Treecreeper1" 
a <- which(data14_15$Tag=="White-throated Honeyeater1")
data14_15$Tag[a] <- "Wht-thrHoneyeater1"
a <- which(data14_15$Tag=="white-throated honeyeater2")
data14_15$Tag[a] <- "Wht-thrHoneyeater2"
a <- which(data14_15$Tag=="White-throated Honeyeater2")
data14_15$Tag[a] <- "Wht-thrHoneyeater2"
a <- which(data14_15$Tag=="Australian Magpie1")
data14_15$Tag[a] <- "AustMagpie1"
a <- which(data14_15$Tag=="Australian Magpie2")
data14_15$Tag[a] <- "AustMagpie2"
a <- which(data14_15$Tag=="Australasian Figbird1")
data14_15$Tag[a] <- "AustFigbird1"
a <- which(data14_15$Tag=="Australasian Figbird2")
data14_15$Tag[a] <- "AustFigbird2"
a <- which(data14_15$Tag=="Australasian Figbird3")
data14_15$Tag[a] <- "AustFigbird3"
a <- which(data14_15$Tag=="Australasian Figbird4")
data14_15$Tag[a] <- "AustFigbird4"

data_sorted <- sort(table(substr(data14_15$Tag,1,9)), decreasing = T)
#plot(sort(table(data14_15$Tag), decreasing=T), type="l", xlim=c(1,100))
#abline(h=c(0,10,20,30,40,50,60))
cols <- c("black","black",   
          "red","black",
          "red","red",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black",
          "black","black")
tiff("Minutes_bird_calling_SERF.tiff", 
    width=900, height=700, res=300)
par(mar=c(1.6,1.6,2,1), mgp=c(3,0.1,0))
plot(data_sorted, xlim=c(1,40), las=1, tck=-0.02,
     xlab="", ylab="", cex.axis=0.7,
     col=cols, pch=20, cex=0.4, cex.lab=1)
mtext("Number of minutes species were calling", cex=0.8, line=1.2)
mtext("14 and 15 October 2011 (3:56 to 5:26 am)", line=0.5, cex=0.5)
mtext("Samford Ecological Research Facility (NE site)",cex=0.4)
mtext(side=1, "Species", line=0.65, cex=0.7)
mtext(side=2, "Number of minutes", line=0.7, cex=0.7)
abline(h=c(30,10),lty=2)
text(x=13, y=58, "Scarlet Honeyeater and Silvereye", cex=0.4)
text(x=10.5, y=50, "Eastern Whipbird",cex=0.4)
text(x=12.5, y=45, "Eastern Yellow Robin", cex=0.4)
text(x=7, y=69, "Lewin's Honeyeater", cex=0.4)
text(x=9, y=63, "White-browed Scrubwren", cex=0.4)
library(graphics)
for(i in 1:length(data_sorted)) {
  segments(x0 = i, y0 = 0, x1 = i, y1=data_sorted[i])
}

dev.off()


segments(x0 = 1, y0 = 0, x1 = 1, y1=data_sorted[1])

unique(substr(data14_15$Tag, 1,9))
data_sorted <- data.frame(data_sorted)
a <- which(data_sorted=="Eastern Y")
a

#library(pracma)
#semilogx(1:length(data_sorted),data_sorted, type = 'b')
#loglog(1:length(data_sorted),data_sorted, type = 'b')
tags <- unique(data14_15$Tag)
a <- which(substr(tags,1,9)=="White-bro") # White-browed Scrubwren
# Most birds in order Silvereye, Scarlet Honeyeater, Lewin's Honeyeater,
# White-browed Scrubwren, Rainbow Lorikeet, Pied Currawong, Eastern Yellow Robin,
# Torresian Crow, Eastern Whipbird, Sacred Kingfisher, Indian peafowl,
# Eastern Koel, White-throated Treecreeper, Australian Figbird, 
