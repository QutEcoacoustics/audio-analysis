clusters_four_months <- read.csv("C:\\Work\\CSV files\\FourMonths\\Hybrid_3_4_7_10_11_15_16_knn_k3j\\four_month_cluster_list_with_na.csv", header=T)

clusters_four_months$site <- as.character(clusters_four_months$site)
a <- which(clusters_four_months$site=="Gympie ")
gympie_four_month_clusters <- clusters_four_months[a,]

a <- which(clusters_four_months$site=="Woondum")
woondum_four_month_clusters <- clusters_four_months[a,]

stats <- matrix(nrow = 30, ncol = 4)
for(i in 1:30) {
  a <- which(gympie_four_month_clusters$cluster_list_na==i)
  b <- length(a)
  c <- (length(a)*100)/159791
  d <- which(woondum_four_month_clusters$cluster_list_na==i)
  e <- length(d)
  f <- (length(d)*100)/159798
  stats[i,1:4] <- c(b,c,e,f)
}

write.csv(stats,"four_month_stats.csv")
