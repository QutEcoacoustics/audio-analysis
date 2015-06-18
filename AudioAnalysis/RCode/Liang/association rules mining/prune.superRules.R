prune.superRules <- function (rules){
  #find redundant rules
  subset.matrix <- is.subset(rules)
  subset.matrix[lower.tri(subset.matrix,diag=TRUE)] <- NA
  redundant <- colSums(subset.matrix, na.rm=TRUE) >= 1
  
  #remove redundant rules
  rules.pruned <- rules[!redundant]
  
  return(rules.pruned)
}