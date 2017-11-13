circlecircumference = 2*pi
angle = 360/365
dist_matrix = data.frame(matrix(ncol = 365, nrow = 365))
for (i in seq(1,365)){
  for(j in seq(1,365)){
    print(i)
    print(j)
    if((j-i) > 180){
      dist_matrix[i,j] = abs((2*pi) * ((360-(j-i))/360))
    }
    else{
      dist_matrix[i,j] = abs((2*pi) * ((j-i)/360))
    }
  }  
}

corrplot(t(apply(dist_matrix, 1, function(x)(x-min(x))/(max(x)-min(x)))), method = "color",
         type = "lower")

dist_matrix_gympie = read.csv('/Volumes/Nifty/QUT/HMM/hiddenstates/distmatrix365gympiecd.csv', header = F)
corrplot(as.matrix(dist_matrix_gympie), method = "color",is.corr = F)

dist_matrix_woondum = read.csv('/Volumes/Nifty/QUT/HMM/hiddenstates/distmatrix365woondumcd.csv', header = F)
corrplot(as.matrix(dist_matrix_woondum), method = "color",is.corr = F)

dist_matrix_all = read.csv('/Volumes/Nifty/QUT/HMM/hiddenstates/distmatrixall.csv', header = F)
corrplot(as.matrix(dist_matrix_all), method = "color",is.corr = F)

colnames(dist_matrix_all) = rep("",794)
colnames(dist_matrix_all)[c(1,398,399,794)] = c("22-06","23-07","22-06","23-07")

col1 <-rainbow(100, s = 1, v = 1, start = 0, end = 0.9, alpha = 1)
col4 <- colorRampPalette(c("#7F0000","red","#FF7F00","yellow","#7FFF7F",
                           "cyan", "#007FFF", "blue","#00007F"))
CairoPNG(filename = "/Volumes/Nifty/QUT/HMM/hiddenstates/distmatrixall2.png", width = 6000, height = 5000,
pointsize = 12, bg = "white",  res = NA)
corrplot(as.matrix(dist_matrix_all), method = "color", col = col2(200), type = "lower", 
title = "Distances between hidden state sequences", diag = T, is.corr=F,
mar=c(0,0,3,0), tl.cex = 1)
dev.off()
