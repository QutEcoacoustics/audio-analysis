# Equal numbers in both circles
dev.off()
g <- make_star(31)
h <- layout_as_star(g)
h <- rbind(h, h[2:31,])
h[32:61,] <- 0.5*h[2:31,]
# move row 1 to end
h <- rbind(h[2:61,], h[1,])
plot(h)

# fourty in outer and 20 in inner circle
dev.off()
g <- make_star(41)
conc_circles <- layout_as_star(g)
plot(conc_circles)
conc_circles <- rbind(conc_circles, conc_circles[seq(2,41,2),])
plot(conc_circles)
conc_circles[42:61,] <- 0.5*conc_circles[seq(2,41,2),]
plot(conc_circles)
# move row 1 to end
h <- rbind(conc_circles[2:61,], conc_circles[1,])
plot(conc_circles)
