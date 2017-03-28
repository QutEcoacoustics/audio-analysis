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
pushViewport(viewport(layout = grid.layout(33,30)))
# Series A - five plots
grid.text("Series A", x = unit(0.5, "npc"), y = unit(0.99, "npc"),
          gp=gpar(fontsize=18))
print(a, vp = vplayout(2:11, 1:6))
print(b, vp = vplayout(2:11, 7:12))
print(c, vp = vplayout(2:11, 13:18))
print(d, vp = vplayout(2:11, 19:24))
print(e, vp = vplayout(2:11, 25:30))
# Series B - five plots
grid.text("Series B", x = unit(0.5, "npc"), y = unit(0.65, "npc"),
          gp=gpar(fontsize=18))
print(a, vp = vplayout(13:22, 1:10))
print(b, vp = vplayout(13:22, 11:20))
print(c, vp = vplayout(13:22, 21:30))
# Series C - five plots
grid.text("Series C", x = unit(0.5, "npc"), y = unit(0.32, "npc"),
          gp=gpar(fontsize=18))
print(a, vp = vplayout(24:33, 1:10))
print(b, vp = vplayout(24:33, 11:20))
print(c, vp = vplayout(24:33, 21:30))
popViewport() # brings you to the top viewport for the whole plotting area
dev.off()