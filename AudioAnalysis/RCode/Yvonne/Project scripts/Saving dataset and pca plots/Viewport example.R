dev.off()
library(grid)
require(ggplot2) 
require(gridExtra)

# make the data
timeSeries <- data.frame(date = seq(as.Date("2015-01-01"), as.Date("2017-01-01"), by = "mon"), 
                         value = 1:25 + rnorm(25, 0, 10)
)

# make the ggplots
gpLine <- ggplot(timeSeries, aes(x = date, y = value)) +
  geom_line()

gpBar <- ggplot(timeSeries, aes(x = date, y = value)) +
  geom_bar(stat = 'identity')

# ggplot + subtext
grid.arrange(gpBar, sub = textGrob("why is this at the bottom?"))
vplayout <- function(x,y) viewport(layout.pos.row = x, layout.pos.col = y)

tiff("Fig19.tiff", width = 2250, height = 2475, units = 'px', res = 300)
pushViewport(viewport(layout = grid.layout(34,30)))
# Series A - five plots
grid.text("Series A", x = unit(0.5, "inch"), y = unit(0.5, "inch"),
          gp=gpar(fontsize=18))
print(O, vp = vplayout(2:12, 1:6))
print(P, vp = vplayout(2:12, 7:12))
print(Q, vp = vplayout(2:12, 13:18))
print(R, vp = vplayout(2:12, 19:24))
print(S, vp = vplayout(2:12, 25:30))
# Series B - five plots
grid.text("Series B", x = unit(0.5, "inch"), y = unit(0.5, "inch"),
          gp=gpar(fontsize=18))
print(U, vp = vplayout(13:23, 1:10))
print(V, vp = vplayout(13:23, 11:20))
print(W, vp = vplayout(13:23, 21:30))
# Series C - five plots
grid.text("Series C", x = unit(0.5, "inch"), y = unit(0.5, "inch"),
          gp=gpar(fontsize=18))
print(X, vp = vplayout(24:34, 1:10))
print(Y, vp = vplayout(24:34, 11:20))
print(Z, vp = vplayout(24:34, 21:30))
popViewport() # brings you to the top viewport for the whole plotting area
dev.off()

# Example 2
dev.off()
require(grid)
# Move to a new page
grid.newpage()
# Create layout : nrow = 2, ncol = 2
pushViewport(viewport(layout = grid.layout(15, 15, respect = T)))
# A helper function to define a region on the layout
define_region <- function(row, col){
  viewport(layout.pos.row = row, layout.pos.col = col)
} 

# Arrange the plots
print(O, vp=define_region(1:5, 1:3), height=unit(0.01, "npc"))
print(P, vp=define_region(1:5, 4:6), height=unit(0.01, "npc"))
print(Q, vp=define_region(1:5, 7:9), height=unit(0.01, "npc"))
print(R, vp=define_region(1:5, 10:12), height=unit(0.01, "npc"))
print(S, vp=define_region(1:5, 13:15), height=unit(0.01, "npc"))
print(U, vp=define_region(6:10, 1:5), height=unit(0.01, "npc"))
print(V, vp=define_region(6:10, 6:10), height=unit(0.01, "npc"))
print(W, vp=define_region(6:10, 11:15), height=unit(0.01, "npc"))
print(X, vp=define_region(11:15, 1:5), height=unit(0.01, "npc"))
print(Y, vp=define_region(11:15, 6:10), height=unit(0.01, "npc"))
print(Z, vp=define_region(11:15, 11:15), height=unit(0.01, "npc"))
#################################
vp_top <- viewport(x = 0, y = 0, width = 7.5, height = 8.7, 
                   name = "vp_top", default.units = "in")
vp9 <- viewport(x=0, y=0, width=2.5, height=2.5, just = c("right","top"), 
                name = "vp9", default.units = "in")
vp10 <- viewport(x=2.5, y=0, width=2.5, height=2.5, just = c("right","top"), 
                 name = "vp10", default.units = "in")
vp11 <- viewport(x=5, y=0, width=2.5, height=2.5, just = c("right","top"), 
                 name = "vp11", default.units = "in")
margin1 <- viewport(x=0, y=2.5, width=7.5, height=0.4, just = c("right","top"), 
                    name = "margin1", default.units = "in")
vp6 <- viewport(x=0, y=2.9, width=2.5, height=2.5, just = c("right","top"), 
                name = "vp6", default.units = "in")
vp7 <- viewport(x=2.5, y=2.9, width=2.5, height=2.5, just = c("right","top"), 
                name = "vp7", default.units = "in")
vp8 <- viewport(x=5, y=2.9, width=2.5, height=2.5, just = c("right","top"), 
                name = "vp8", default.units = "in")
margin2 <- viewport(x=0, y=5.4, width=7.5, height=0.4, just = c("right","top"), 
                    name = "margin2", default.units = "in")
vp1 <- viewport(x=0, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
                name = "vp1", default.units = "in")
vp2 <- viewport(x=1.5, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
                name = "vp2", default.units = "in")
vp3 <- viewport(x=3, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
                name = "vp3", default.units = "in")
vp4 <- viewport(x=4.5, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
                name = "vp4", default.units = "in")
vp5 <- viewport(x=6, y=5.8, width=1.5, height=2.5, just = c("right","top"), 
                name = "vp5", default.units = "in")
margin3 <- viewport(x=0, y=8.3, width=7.5, height=0.4, just = c("right","top"), 
                    name = "margin3", default.units = "in")
splot <- vpTree(vp_top, vpList(vp1, vp2, vp3, vp4, vp5, 
                               vp6, vp7, vp8, vp9, vp10, 
                               vp11, margin1, margin2, margin3))
pushViewport(splot)
seekViewport("vp9")
par(mar=c(0,0,0,0))
print(U)
seekViewport("vp5")
par(mar=c(0,0,0,0))
print(V)
seekViewport("vp2")
par(mar=c(0,0,0,0))
print(W)
