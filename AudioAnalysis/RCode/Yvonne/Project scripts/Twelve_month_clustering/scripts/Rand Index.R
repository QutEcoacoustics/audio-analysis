#rm(list = ls())
library(clv)
data(iris)
iris.data <- iris[,1:4]

pam.mod <- pam(iris.data,3) # create three clusters
v.pred <- as.integer(pam.mod$clustering) # get cluster ids associated to given data objects
v.real <- as.integer(iris$Species) # get also real cluster ids

# prepare set of functions which compare two clusterizations
Rand <- function(clust1,clust2) clv.Rand(std.ext(clust1,clust2))
Jaccard <- function(clust1,clust2) clv.Jaccard(std.ext(clust1,clust2))
Folkes.Mallows <- function(clust1,clust2) clv.Folkes.Mallows(std.ext(clust1,clust2))

# compute indicies
rand2 <- Rand(v.pred,v.real)
jaccard2 <- Jaccard(v.pred,v.real)
folk.mal2 <- Folkes.Mallows(v.pred,v.real)

x <- rbind(matrix(rnorm(150,           sd = 0.1), ncol = 3),
           matrix(rnorm(150, mean = 1, sd = 0.1), ncol = 3),
           matrix(rnorm(150, mean = 2, sd = 0.1), ncol = 3),
           matrix(rnorm(150, mean = 3, sd = 0.1), ncol = 3))
gskmn <- clusGap(x, FUN = kmeans, nstart = 20, K.max = 10, B = 60)
gskmn <- clusGap(x, FUN = kmeans, nstart = 50, K.max = 10, B = 500)
gskmn <- clusGap(data[,2:13], kmeans, nstart=30, K.max = 80, B = 500)
gskmn
plot(gskmn$Tab[,3], type="l")

rm(list = ls())
# gap statistic
library(cluster)

list <- c("12500", "15000", "17500", "20000", "22500", "25000", "27500","30000")
list1 <- c("5", "10", "15", "20", "25", "30", "35", "40", "45", "50",
           "55","60", "65", "70", "75", "80", "85", "90", "95", "100")

i=2
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
load(file)
centers15000 <- as.data.frame(centers15000)
#View(data)
rm(data)
mydist <- function(x) {
  as.dist((1-cor(t(x)))/2)
}
mycluster <- function(x, k) {
  list(cluster=cutree(hclust(mydist(x), 
                             method = "ward.D2"), k=k))
} 
# hclust
#plot(gskmn_25000_nst30_iter_50_kmax_100_B12_hclust, main="Gap Statistic")
paste(Sys.time(), " Starting clusGap", sep = " ")
gskmn_15000_k80_B12_hclust <- clusGap(centers15000, 
                                      FUNcluster = mycluster,
                                      K.max = 80, B = 12)
paste(Sys.time(), " Finishing clusGap", sep = " ")

gap_Stat1 <- gskmn_15000_k80_B12_hclust
write.csv(gap_Stat1$Tab, "Gap Statistic_15000_hclust_K80 B12.csv", row.names=F)

tiff("gap_statistics_hclust_15000_2_k80_B12.tiff", 
     height=800, width=1200, res=300)
par(mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
plot(gskmn_15000_k80_B12_hclust, main="Gap Statistic",
     xlim=c(0,50), xlab="", ylab="")
abline(v=36, lty=2)
mtext(side=1, line=1.1, "k2")
mtext(side=2, line=1.15, "gap statistic")
dev.off()

i=2
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
load(file)
centers15000 <- as.data.frame(centers15000)
#j=12
#file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\hybrid_dataset_centers_",list[i],"_", list1[j], ".csv", sep="")
#data_temp <- read.csv(file, header = T)
#View(data)
#data.selection <- data_temp[,2:13]
#rm(data_temp)
mydist <- function(x) as.dist((1-cor(t(x)))/2)
mycluster <- function(x, k) list(cluster=cutree(hclust(mydist(x), method = "ward.D2"), k=k))
paste(Sys.time(), " Starting clusGap", sep = " ")
gskmn_15000_k80_B12_hclust <- clusGap(centers15000, 
                                      FUNcluster = mycluster,
                                      K.max = 80, B = 12,
                                      verbose = TRUE)
paste(Sys.time(), " Ending clusGap", sep = " ")
write.csv(gskmn_15000_k80_B12_hclust$Tab, "Gap Statistic_20000_hclust_K80 B12.csv", row.names=F)

gap_Stat3 <- gskmn_15000_kmax_80_B12_hclust
write.csv(gap_Stat3$Tab, "Gap Statistic_20000_hclust_K80 B12.csv", row.names=F)

tiff("gap_statistics_hclust_15000_k80_B12.tiff", 
     height=800, width=1200, res=300)
par(mar=c(2,2,1,1), mgp = c(1, 0.4, 0))
plot(gskmn_15000_kmax_80_B12_hclust, main="Gap Statistic",
     xlim=c(0,30), xlab="", ylab="")
abline(v=15, lty=2)
mtext(side=1, line=1.1, "k2")
mtext(side=2, line=1.15, "gap statistic")
dev.off()

plot(gskmn_25000_nst30_iter_50_kmax_100_B12_hclust,
     main="Gap statistic hclust 25000")
abline(v=15)
# kmeans
gskmn_25000_nst30_iter_50_kmax_100_B12_kmeans <- clusGap(data.selection, 
                                                  FUN = kmeans, 
                                                  nstart=30, 
                                                  iter.max = 50, 
                                                  K.max = 80, B = 12)



# Alternatively
set.seed(123)
gap_stat <- clusGap(iris.scaled, FUN = hcut, K.max = 10, B = 50)
# Plot gap statistic
fviz_gap_stat(gap_stat)


i=2
#file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
#load(file)
j=12
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\hybrid_dataset_centers_",list[i],"_", list1[j], ".csv", sep="")
data <- read.csv(file, header = T)
#View(data)
gskmn_15000_nst30_iter_50_kmax_100_B12 <- clusGap(data[,2:13], 
                                                  kmeans, nstart=30, 
                                                  iter.max = 50, 
                                                  K.max = 80, 
                                                  B = 12)
plot(gskmn_15000_nst30_iter_50_kmax_100_B12, main = "Gap statistic")

i=4
#file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
#load(file)
j=12
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\hybrid_dataset_centers_",list[i],"_", list1[j], ".csv", sep="")
data <- read.csv(file, header = T)
#View(data)

gskmn_20000_kmax_80_B12_hclust <- clusGap(data.selection, 
                                   kmeans, nstart=30, 
                                   iter.max = 50, 
                                   K.max = 80, 
                                   B = 12)

tiff("gap_statistics_hclust_20000_k80_B12.tiff", 
     height=1200, width=800, res=300)
plot(gskmn_20000_kmax_80_B12_hclust, main="Gap Statistic",
     xlim=c(0,30), xlab="k2", ylab="gap statistic")
dev.off()




# Cophenetic Distances for a Hierarchical Clustering 
require(graphics)
d1 <- dist(USArrests)
hc <- hclust(d1, "ave")
d2 <- cophenetic(hc)
cor(d1, d2) # 0.7659


i=8
#file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
#load(file)
j=12
file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\hybrid_dataset_centers_",list[i],"_", list1[j], ".csv", sep="")
data <- read.csv(file, header = T)
#View(data)
gskmn_30000_nst30_iter_50_kmax_100_B100 <- clusGap(data[,2:13], 
                                                   kmeans, nstart=30, 
                                                   iter.max = 50, 
                                                   K.max = 80, 
                                                   B = 12)



gap <- data.frame(desc = NA)

for(i in 1:length(list)) {
  print(paste("Starting", list[i]))
  file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data/datasets/kmeans_results/kmeanscenters", list[i],".RData", sep="")
  load(file)
  for(j in 1:length(list1)) {
    file <- paste("C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\hybrid_dataset_centers_",list[i],"_", list1[j], ".csv", sep="")
    data <- read.csv(file, header = T)
    #load(file = "C:\\Work\\Projects\\Twelve_month_clustering\\Saving_dataset\\data\\datasets\\hclust_results\\hclust_clusters25000.Rdata")
    centers <- data.frame(get(paste("centers",list[i],sep="")))
    cluster <- data[,1]
    rm(data)
    a <- distcritmulti(centers, cluster, criterion = "asw")
    sil_width <- a$crit.overall
    overall_sil_width[j,(i+1)] <- a$crit.overall
    print(a$crit.overall)
  }
}
