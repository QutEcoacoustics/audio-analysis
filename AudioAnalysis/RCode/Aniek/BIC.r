#BIC calculation
#BIC = −2 log(Ld) + p log (size dataset)
#Ld = max-likelihood
#p parameters: k + k^2 + km, k = no. of states, m = no. of observations

calcBIC = function(hmm){
  k = hmm$model$n_states
  m = hmm$model$n_symbols
  p = k + k*k + k*m
  BIC = (-2*hmm$logLik) + (p * log(hmm$model$length_of_sequences*hmm$model$n_sequences))
}

BIC_gcd_3 = calcBIC(hmm_gcd_3)
BIC_gcd_4 = calcBIC(hmm_gcd_4)
BIC_gcd_5 = calcBIC(hmm_gcd_5)
BIC_gcd_6 = calcBIC(hmm_gcd_6)
BIC_gcd_7 = calcBIC(hmm_gcd)
BIC_gcd_8 = calcBIC(hmm_gcd_8)
BIC_gcd_9 = calcBIC(hmm_gcd_9)
BIC_gcd_10 = calcBIC(hmm_gcd_10)
BIC_gcd_12 = calcBIC(hmm_gcd_12)
BIC_gcd_15 = calcBIC(hmm_gcd_15)
BIC_gcd_16 = calcBIC(hmm_gcd_16)
BIC_gcd_18 = calcBIC(hmm_gcd_18)
BIC_gcd_20 = calcBIC(hmm_gcd_20)

BIC_gmn_8 = calcBIC(hmm_gmn_8)

BIC_wcd_2 = calcBIC(hmm_wcd_2)
BIC_wcd_3 = calcBIC(hmm_wcd_3)
BIC_wcd_4 = calcBIC(hmm_wcd_4)
BIC_wcd_5 = calcBIC(hmm_wcd_5)
BIC_wcd_6 = calcBIC(hmm_wcd_6)
BIC_wcd_7 = calcBIC(hmm_wcd)
BIC_wcd_8 = calcBIC(hmm_wcd_8)
BIC_wcd_9 = calcBIC(hmm_wcd_9)
BIC_wcd_10 = calcBIC(hmm_wcd_10)
BIC_wcd_12 = calcBIC(hmm_wcd_12)
BIC_wcd_14 = calcBIC(hmm_wcd_14)
BIC_wcd_16 = calcBIC(hmm_wcd_16)
BIC_wcd_18 = calcBIC(hmm_wcd_18)
BIC_wcd_20 = calcBIC(hmm_wcd_20)
BIC_wcd_25 = calcBIC(hmm_wcd_25)

loglik_gcd = c(hmm_gcd_3$logLik,hmm_gcd_4$logLik,hmm_gcd_5$logLik,hmm_gcd_6$logLik,hmm_gcd$logLik,
               hmm_gcd_8$logLik,hmm_gcd_9$logLik,hmm_gcd_10$logLik,hmm_gcd_12$logLik,
               hmm_gcd_15$logLik,hmm_gcd_16$logLik,hmm_gcd_18$logLik,hmm_gcd_20$logLik)
loglik_wcd = c(hmm_wcd_2$logLik,hmm_wcd_3$logLik,hmm_wcd_4$logLik,hmm_wcd_5$logLik,hmm_wcd_6$logLik,hmm_wcd$logLik,
               hmm_wcd_8$logLik,hmm_wcd_9$logLik,hmm_wcd_10$logLik,hmm_wcd_12$logLik,
               hmm_wcd_14$logLik,hmm_wcd_16$logLik,hmm_wcd_18$logLik,hmm_wcd_20$logLik,hmm_wcd_25$logLik)


x_w = c(2,3,4,5,6,7,8,9,10,12,14,16,18,20,25)
y_w = c(BIC_wcd_2, BIC_wcd_3,BIC_wcd_4,BIC_wcd_5,BIC_wcd_6,BIC_wcd_7,BIC_wcd_8,BIC_wcd_9,BIC_wcd_10,BIC_wcd_12,BIC_wcd_14,BIC_wcd_16,BIC_wcd_18,BIC_wcd_20,BIC_wcd_25)
x_g = c(3,4,5,6,7,8,9,10,12,15,16,18,20)
y_g = c(BIC_gcd_3,BIC_gcd_4,BIC_gcd_5,BIC_gcd_6,BIC_gcd_7,BIC_gcd_8,BIC_gcd_9,BIC_gcd_10,BIC_gcd_12,BIC_gcd_15,BIC_gcd_16,BIC_gcd_18,BIC_gcd_20)
plot(c(x_w,x_g),c(y_w,y_g), ylab = "BIC score", xlab = "Number of hidden states", pch = 16, type = 'p', cex = 0.8)
lines(x_w,y_w, col = 'green', lwd = 2.5)
lines(x_g,y_g, col = 'red', lwd = 2.5)
axis(1, at = c(3,4,5,6,7,8,9,10,12,14,16,18,20,25))
legend("topright", legend = c("Gympie","Woondum"), lty = c(1,1), lwd = c(2.5,2.5),
       col = c("red","green"))

par(new = T)

plot(c(x_g,x_w),c(loglik_gcd,loglik_wcd), axes = F, ylab = NA, xlab = NA, pch = 16, cex = 0.8)
axis(side = 4)
mtext(side = 4, line =3, 'Log likelihood')
lines(x_w,loglik_wcd, col = 'darkgreen', lwd=2.5)
lines(x_g,loglik_gcd, col = 'darkred', lwd = 2.5)
axis(1, at = c(3,4,5,6,7,8,9,10,14,16,18,20,25))
legend("topright", legend = c("BIC Gympie","BIC Woondum","Loglik Gympie","Loglik Woondum"), lty = c(1,1), lwd = c(2.5,2.5),
       col = c("red","green","darkred","darkgreen"))
