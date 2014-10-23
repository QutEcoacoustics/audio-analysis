decisionTree <- function(filepath, cp=0.001, approach='anova'){
  require(rpart)
  
  training<-read.csv(filepath)
  
  #fit a model
#   temp <- rpart.control(cp)
  fit <- rpart(callCount~., data=training, method=approach, control=temp)
#   pfit <- prune(fit, cp=fit$cptable[which.min(fit$cptable[,"xerror"]),"CP"])
  
  oneSE <- fit$cptable[which.min(fit$cptable[,"xerror"]),"xerror"] + 
    fit$cptable[which.min(fit$cptable[,"xerror"]),"xstd"]
  pfit <- prune(fit, cp=fit$cptable[which(fit$cptable[ , "xerror"] < oneSE)[1] - 1,'CP'])
  
  result <- list(fit=fit, pfit=pfit)
  return(result)
}