library('seqHMM')
library('TraMineR')
library('seqinr')
require('R.oo')
require('igraph')
require('mhsmm')
require('R.matlab')

colsammap = c("yellow", "blue", "green", "yellow", "grey", "grey", "orange","orange","lightblue","blue","green","orange","grey","green","green","orange","blue","blue","lightblue","red","blue","yellow","pink","lightblue","lightblue","yellow","yellow","green","yellow","lightblue","grey","orange","green","orange","grey","grey","green","grey","green","lightblue","grey","lightblue","red","orange","lightblue","lightblue","lightblue","orange","pink","grey","lightblue","lightblue","grey","blue","grey","lightblue","green","green","blue","green")
########### gympie civil dawn
#read in the sequence from the fasta file
gcd = readBStringSet('/Volumes/Nifty/QUT/60clusters/Fastafiles/Gympie_letters_civildawn.txt', format="fasta",
                                nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)

#Create sequences of equal length, therefore remove the last day because it is significantly shorter
#And remove the last one/two minutes of each sequence so they are all equal
gcd = gcd[1:397]
gcd = subseq(gcd,1,1438)

#Create a state sequence object that can be used to build a HMM
stateseqgcd = seqdef(gcd, id = names(gcd), missing = '-', cpal = colorpalette[[60]],
                     labels = c("01","02","03","04","05","06","07","08","09",seq(10,60)))
#Initialize the HMM with 7 different states representing rain, birds, etc..
init_hmm_gcd = build_hmm(observations=stateseqgcd, n_states=7)

#Fit the model to the given observations/sequences
hmm_gcd = fit_model(init_hmm_gcd)

#Calculate the hidden state path sequence for each day
hidpaths_gcd = hidden_paths(hmm_gcd$model)


#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgcd.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd$model, hidpaths_gcd,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/obsgcd2.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd$model, plots="obs", type = "I", legend.prop = 0.3, cex.legend = 2)
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/statetransitiongcd.jpeg", 1000, 1000, quality = 100)
plot(hmm_gcd$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.01,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T,
     edge.arrow.size = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))
dev.off()

###### 4 hiddenstates
init_hmm_gcd_4 = build_hmm(observations=stateseqgcd, n_states=4)

#Fit the model to the given observations/sequences
hmm_gcd_4 = fit_model(init_hmm_gcd_4)

#Calculate the hidden state path sequence for each day
hidpaths_gcd_4 = hidden_paths(hmm_gcd_4$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgcd_4.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd_4$model, hidpaths_gcd_4,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_gcd_4$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.01,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T,
     edge.arrow.size = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))

###### 5 hiddenstates
init_hmm_gcd_5 = build_hmm(observations=stateseqgcd, n_states=5)

#Fit the model to the given observations/sequences
hmm_gcd_5 = fit_model(init_hmm_gcd_5)

#Calculate the hidden state path sequence for each day
hidpaths_gcd_5 = hidden_paths(hmm_gcd_5$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgcd_5.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd_5$model, hidpaths_gcd_5,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_gcd_5$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.01,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T,
     edge.arrow.size = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))

em_gcd_5 = hmm_gcd_5$model$emission_probs
colnames(em_gcd_5) = c("01","02","03","04","05","06","07","08","09",seq(10,60))
barplot(em_gcd_5, cex.names = 0.5, las =2, col = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231'))
barplot(sort(em_gcd_5[2,], decreasing = T),names.arg=names(sort(em_gcd_5[2,], decreasing = T)),
        cex.names = 0.5,
        ylim = c(0,1), las =2)
barplot(sort(em_gcd_5[3,], decreasing = T),names.arg=names(sort(em_gcd_5[3,], decreasing = T)),
        cex.names = 0.5,
        ylim = c(0,1), las =2)
barplot(sort(em_gcd_5[4,], decreasing = T),names.arg=names(sort(em_gcd_5[4,], decreasing = T)),
        cex.names = 0.5,
        ylim = c(0,1), las =2)
barplot(sort(em_gcd_5[5,], decreasing = T),names.arg=names(sort(em_gcd_5[5,], decreasing = T)),
        cex.names = 0.5,
        ylim = c(0,1), las =2)

###### 6 hiddenstates
init_hmm_gcd_6 = build_hmm(observations=stateseqgcd, n_states=6)

#Fit the model to the given observations/sequences
hmm_gcd_6 = fit_model(init_hmm_gcd_6)

#Calculate the hidden state path sequence for each day
hidpaths_gcd_6 = hidden_paths(hmm_gcd_6$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgcd_6.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd_6$model, hidpaths_gcd_6,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_gcd_6$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.01,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T,
     edge.arrow.size = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))

###### 20 hiddenstates
init_hmm_gcd_20 = build_hmm(observations=stateseqgcd, n_states=20)

#Fit the model to the given observations/sequences
hmm_gcd_20 = fit_model(init_hmm_gcd_20)

#Calculate the hidden state path sequence for each day
hidpaths_gcd_20 = hidden_paths(hmm_gcd_20$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgcd_20.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd_20$model, hidpaths_gcd_20,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0','#911eb4','#d2f53c','#fabebe','#e6beff','#aa6e28','#fffac8','#800000','#aaffc3','#808000','#ffd8b1','#000080','#808080','#FFFFFF','#000000'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_gcd_20$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.01,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T, edge.width = 0.0,
     edge.arrow.size = 0.5,
     loops = F, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))

em_gcd_20 = hmm_gcd_20$model$emission_probs
colnames(em_gcd_20) = c("01","02","03","04","05","06","07","08","09",seq(10,60))
barplot(em_gcd_20, cex.names = 0.5, las =2, col = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0','#911eb4','#d2f53c','#fabebe','#e6beff','#aa6e28','#fffac8','#800000','#aaffc3','#808000','#ffd8b1','#000080','#808080','#FFFFFF','#000000') )
###########woondum civil dawn
#read in the sequence from the fasta file
wcd = readBStringSet('/Volumes/Nifty/QUT/60clusters/Fastafiles/Woondum_letters_civildawn.txt', format="fasta",
                     nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)

#Create sequences of equal length, therefore remove the last day because it is significantly shorter
#And remove the last one/two minutes of each sequence so they are all equal
wcd = wcd[1:397]
wcd = subseq(wcd,1,1438)

#Create a state sequence object that can be used to build a HMM
stateseqwcd = seqdef(wcd, id = names(wcd), missing = '-', cpal = colorpalette[[60]],
                     labels = c("01","02","03","04","05","06","07","08","09",seq(10,60)))
#Initialize the HMM with 7 different states representing rain, birds, etc..
init_hmm_wcd = build_hmm(observations=stateseqwcd, n_states=7)

#Fit the model to the given observations/sequences
hmm_wcd = fit_model(init_hmm_wcd)

#Calculate the hidden state path sequence for each day
hidpaths_wcd = hidden_paths(hmm_wcd$model)

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidwcd.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd$model, hidpaths_wcd,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/obswcd.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd$model, plots="obs", type = "I", legend.prop = 0.3, cex.legend = 2)
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/statetransitionwcd.jpeg", 1000, 1000, quality = 100)
plot(hmm_wcd$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.05,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T,
     edge.arrow.size = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))
dev.off()

###### 3 hiddenstates
init_hmm_wcd_3 = build_hmm(observations=stateseqwcd, n_states=3)

#Fit the model to the given observations/sequences
hmm_wcd_3 = fit_model(init_hmm_wcd_3)

#Calculate the hidden state path sequence for each day
hidpaths_wcd_3 = hidden_paths(hmm_wcd_3$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidwcd_3.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd_3$model, hidpaths_wcd_3,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_wcd_3$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.01,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T,
     edge.arrow.size = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))


###### 4 hiddenstates
init_hmm_wcd_4 = build_hmm(observations=stateseqwcd, n_states=4)

#Fit the model to the given observations/sequences
hmm_wcd_4 = fit_model(init_hmm_wcd_4)

#Calculate the hidden state path sequence for each day
hidpaths_wcd_4 = hidden_paths(hmm_wcd_4$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidwcd_4.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd_4$model, hidpaths_wcd_4,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_wcd_4$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.01,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T,
     edge.arrow.size = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))

###### 5 hiddenstates
init_hmm_wcd_5 = build_hmm(observations=stateseqwcd, n_states=5)

#Fit the model to the given observations/sequences
hmm_wcd_5 = fit_model(init_hmm_wcd_5)

#Calculate the hidden state path sequence for each day
hidpaths_wcd_5 = hidden_paths(hmm_wcd_5$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidwcd_5.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd_5$model, hidpaths_wcd_5,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_wcd_5$model,
     layout = layout_nicely, pie = T,
     cpal = colsammap, combine.slices = 0.01,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 0,
     trim = 0.005, label.signif = 2,
     edge.curved = T,
     edge.arrow.size = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=T, legend.prop = 0.1, cex.legend = 0.4,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)),
     xlim = c(-0.5, 0.5))


######fit model on wcd with emission and state probabilties from gcd in matlab
tm_gcd_5 = hmm_gcd_5$model$transition_probs
writeMat(con="/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/em_gcd_5.mat", x=as.matrix(em_gcd_5))
writeMat(con="/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/tm_gcd_5.mat", x=as.matrix(tm_gcd_5))
