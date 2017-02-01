# merge the components in the same cluster
# sub.index<-list()
# for(iter in 1:length(clusters)){
#   maximum<-max(clusters[[iter]])
#   representative<-numeric()
#   for(kk in 1:maximum){
#     representative<-c(representative, Position(function(x) x==kk, clusters[[iter]]))
#   }
#   sub.index[[iter]]<-representative
# }

clusters.bases <- list()
for(iter in 1:length(clusters)){
# for(iter in 1:2){
  maximum <- max(clusters[[iter]])
  if(maximum!=0){
    clusters.w<-numeric()
    for(cnt in 1:maximum){
      sameCluster <- which(clusters[[iter]]==cnt)
      min.w <- bases[[iter]][[1]]
#     clusters.w <- cbind(clusters.w, rowSums(min.w[,sameCluster]))
      clusters.w <- cbind(clusters.w, rowMeans(min.w[,sameCluster]))
    }
    if(any(clusters[[iter]]==0)){
      exclusive <- which(clusters[[iter]]==0)
      clusters.w <- cbind(clusters.w, min.w[,exclusive])
    }
    clusters.bases[[iter]] <- clusters.w
  }else{
    clusters.bases[[iter]] <- bases[[iter]][[1]]
  }
}