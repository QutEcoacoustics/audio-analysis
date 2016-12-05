library(R.matlab)
library(dynamicTreeCut)
load("C:/Work/myfile/bird.index.RData")

# read bases (frequency) and coefficients (temporal information)
bases<-readMat('c:/work/myfile/bases.mat')
bases<-bases[[1]]

coef<-readMat('c:/work/myfile/coefficients.mat')
coef<-coef[[1]]

#clustering bases and coefficients
clusters<-list()
for(min.index in 1:length(bird.index)){
  min.w<-bases[[min.index]][[1]]
  min.h<-coef[[min.index]][[1]]
  
#   use 95% quantile to remove noise
  cutoff.w <- quantile(min.w, probs=0.95)
  cutoff.h <- quantile(min.h, probs=0.95)
  
  index.w <- which(min.w < cutoff.w, arr.ind=T)
  index.h <- which(min.h < cutoff.h, arr.ind=T)
  
  min.w[index.w] <- 0
  min.h[index.h] <- 0
  
# clustering
  distance.h<-dist(min.h, method='euclidean')
  fit.h<-hclust(distance.h, method='ward')
#   cut.height<-(max(fit.h[[2]])-min(fit.h[[2]]))*0.5+min(fit.h[[2]])

# prune cluster tree
  distM<-as.matrix(distance.h)
#   pruned<-cutreeHybrid(fit.h,distM, cutHeight=cut.height, minClusterSize=1)
  pruned<-cutreeHybrid(fit.h,distM,minClusterSize=1,deepSplit=3)
# pruned<-cutreeDynamicTree(fit.h, minModuleSize=1)

#cluster labels of bases or coefficients
  labels<-pruned$labels
  clusters[[min.index]]<-labels
}


