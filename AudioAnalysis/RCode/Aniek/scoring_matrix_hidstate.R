##
##Sij = log(Pij/Qi*Qj)

#The state transition probabilities
statetransprob = hmm_combcd$model$transition_probs

#Probability of finding state in any sequence
letterFcombcd = letterFrequency(hiddenstatesequencecombcd, uniqueLetters(hiddenstatesequencecombcd))
Fcombcd = colSums(letterFcombcd)
Pcombcd = (Fcombcd)/sum(Fcombcd)
Pcombcd = Pcombcd[c("G","A","V","L","I","F","Y")]

scoring_matrix = data.frame(matrix(ncol = 7, nrow=7))
for (i in 1:7){
  for (j in 1:7){
    scoring_matrix[i,j] =((log(statetransprob[i,j]/(Pcombcd[i] * Pcombcd[j]))))
    print(statetransprob[i,j])
    print(Pcombcd[i])
    print(Pcombcd[j])
  }
}

row.names(scoring_matrix) = c("G","A","V","L","I","F","Y")
colnames(scoring_matrix) = c("G","A","V","L","I","F","Y")

write.table(scoring_matrix, file="/Volumes/Nifty/QUT/HMM/hiddenstates/scoringmatrixmatlab.txt", row.names=TRUE, col.names=TRUE)
