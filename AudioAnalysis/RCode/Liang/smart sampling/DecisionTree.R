decisionTree <- function(n, m){
  require(rpart)
  
  indices.species<-read.csv('c:/work/myfile/linearRegression.csv')
  training <- indices.species[(1+1440*(n-1)):(1440*n), ]
  test <- indices.species[(1+1440*(m-1)):(1440*m), ]
  
  #fit a model
  temp <- rpart.control(cp=0.001)
  fit <- rpart(speciesNum~.-IndicesCount, data=training, method='anova', control=temp)
#   pfit <- prune(fit, cp=fit$cptable[which.min(fit$cptable[,"xerror"]),"CP"])
  
  oneSE <- fit$cptable[which.min(fit$cptable[,"xerror"]),"xerror"] + 
    fit$cptable[which.min(fit$cptable[,"xerror"]),"xstd"]
  pfit <- prune(fit, cp=fit$cptable[which(fit$cptable[ , "xerror"] < oneSE)[1] - 1,'CP'])
  
  #prediction
  predicted <- predict(pfit, test)
  
  result <- list(predicted = predicted, pfit = pfit)
  return(result)
}