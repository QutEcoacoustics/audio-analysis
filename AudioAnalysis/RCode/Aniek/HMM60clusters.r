library('seqHMM')
library('TraMineR')
library('seqinr')
require('R.oo')
require('igraph')
require('mhsmm')
require('R.matlab')

colsammap = c("yellow", "blue", "green", "yellow", "grey", "grey", "orange","orange","lightblue","blue","green","orange","grey","green","green","orange","blue","blue","lightblue","red","blue","yellow","pink","lightblue","lightblue","yellow","yellow","green","yellow","lightblue","grey","orange","green","orange","grey","grey","green","grey","green","lightblue","grey","lightblue","red","orange","lightblue","lightblue","lightblue","orange","pink","grey","lightblue","lightblue","grey","blue","grey","lightblue","green","green","blue","green")
############ Civil dawn
########### Gympie

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


###### 3 hiddenstates
init_hmm_gcd_3 = build_hmm(observations=stateseqgcd, n_states=3)

#Fit the model to the given observations/sequences
hmm_gcd_3 = fit_model(init_hmm_gcd_3)

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
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/1hidgcd_5.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd_5$model, hidpaths_gcd_5,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       legend.prop = 0.3, cex.legend = 3,yaxis = T, ylab = names(gcd))
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

######fit model on gcd with emission and state probabilties from wcd in matlab, see matlab script
tm_wcd_5 = hmm_wcd_5$model$transition_probs
em_wcd_5 = hmm_wcd_5$model$emission_probs
writeMat(con="/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/em_wcd_5.mat", x=as.matrix(em_wcd_5))
writeMat(con="/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/tm_wcd_5.mat", x=as.matrix(tm_wcd_5))

hp_wcdgcd_5 = readMat(con="/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hp_wcdgcd_5.mat")
hp_wcdgcd_5 = hp_wcdgcd_5[[1]]
rownames(hp_wcdgcd_5) = names(gcd)
hp_wcdgcd_5 = seqdef(hp_wcdgcd_5, cpal = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231'))

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidwcdgcd_5.jpeg", 2000, 2000, quality = 100)
ssplot(hp_wcdgcd_5, type = "I",
       legend.prop = 0.3, cex.legend = 3)
dev.off()


######mhmm
###### 5
init_mhmm_gcd_5 = build_mhmm(observations=stateseqgcd, n_states=c(5,5))

#Fit the model to the given observations/sequences
mhmm_gcd_5 = fit_model(init_mhmm_gcd_5)

#Calculate the hidden state path sequence for each day
hidpaths_gcd_5 = hidden_paths(hmm_gcd_5$model)


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

######7 different states representing rain, birds, etc..
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

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/obsgcd.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd$model, plots="obs", type = "I", legend.prop = 0.3, cex.legend = 2,
       yaxis = T)
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


###### 8 hiddenstates
init_hmm_gcd_8 = build_hmm(observations=stateseqgcd, n_states=8)

#Fit the model to the given observations/sequences
hmm_gcd_8 = fit_model(init_hmm_gcd_8)

hidpaths_gcd_8 = hidden_paths(hmm_gcd_8$model)

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/coloredhidgcd_8.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gcd_8$model, hidpaths_gcd_8,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('#FFFF99','green','lightblue','grey','yellow','#32CD32','orange','blue'),
       hidden.states.labels = c('1. Insects',"2. Birds","3. Wind","4. Quiet","5. Insects","6. Birds","7. Cicadas",'8. Rain'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_gcd_8$model,
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

###### 9 hiddenstates
init_hmm_gcd_9 = build_hmm(observations=stateseqgcd, n_states=9)

#Fit the model to the given observations/sequences
hmm_gcd_9 = fit_model(init_hmm_gcd_9)

###### 10 hiddenstates
init_hmm_gcd_10 = build_hmm(observations=stateseqgcd, n_states=10)

#Fit the model to the given observations/sequences
hmm_gcd_10 = fit_model(init_hmm_gcd_10)

###### 12 hiddenstates
init_hmm_gcd_12 = build_hmm(observations=stateseqgcd, n_states=12)

#Fit the model to the given observations/sequences
hmm_gcd_12 = fit_model(init_hmm_gcd_12)

###### 15 hiddenstates
init_hmm_gcd_15 = build_hmm(observations=stateseqgcd, n_states=15)

#Fit the model to the given observations/sequences
hmm_gcd_15 = fit_model(init_hmm_gcd_15)

###### 16 hiddenstates
init_hmm_gcd_16 = build_hmm(observations=stateseqgcd, n_states=16)

#Fit the model to the given observations/sequences
hmm_gcd_16 = fit_model(init_hmm_gcd_16)

###### 18 hiddenstates
init_hmm_gcd_18 = build_hmm(observations=stateseqgcd, n_states=18)

#Fit the model to the given observations/sequences
hmm_gcd_18 = fit_model(init_hmm_gcd_18)

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

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/1sortmds_hidwcd_5.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd_5$model, hidpaths_wcd_5,plots="hidden.paths", type = "I", sortv = "mds.hidden",
       hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
       yaxis = T, legend.prop = 0.3, cex.legend = 3)
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/dist_hidwcd_5.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd_5$model, hidpaths_wcd_5,plots="hidden.paths", type = "d", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0'),
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

######fit model on wcd with emission and state probabilties from gcd in matlab, see matlab script
tm_gcd_5 = hmm_gcd_5$model$transition_probs
writeMat(con="/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/em_gcd_5.mat", x=as.matrix(em_gcd_5))
writeMat(con="/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/tm_gcd_5.mat", x=as.matrix(tm_gcd_5))

hp_wcd_5 = readMat(con="/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hp_wcd_5.mat")
hp_wcd_5 = hp_wcd_5[[1]]
rownames(hp_wcd_5) = names(wcd)
hp_wcd_5 = seqdef(hp_wcd_5, cpal = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231'))

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgcdwcd_5.jpeg", 2000, 2000, quality = 100)
ssplot(hp_wcd_5, type = "I",
       legend.prop = 0.3, cex.legend = 3)
dev.off()

###### 6 hiddenstates
init_hmm_wcd_6 = build_hmm(observations=stateseqwcd, n_states=6)

#Fit the model to the given observations/sequences
hmm_wcd_6 = fit_model(init_hmm_wcd_6)

#7 different states representing rain, birds, etc..
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
ssplot(hmm_wcd$model, plots="obs", type = "I", legend.prop = 0.3, cex.legend = 2,
       yaxis=T, ylab = names(wcd))
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

###### 8 hiddenstates
init_hmm_wcd_8 = build_hmm(observations=stateseqwcd, n_states=8)

#Fit the model to the given observations/sequences
hmm_wcd_8 = fit_model(init_hmm_wcd_8)

hidpaths_wcd_8 = hidden_paths(hmm_wcd_8$model)

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/coloredhidwcd_8.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd_8$model, hidpaths_wcd_8,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('yellow','orange' ,'grey', 'blue' ,'#00FA9A' ,'lightblue' ,'green','lightgrey'),
       hidden.states.labels = c('1. Insects', '2. Cicadas', '3. Quiet','4. Rain','5. Insects/Birds/Wind', '6. Wind', '7. Birds', '8. Quiet'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_wcd_8$model,
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

###### 9 hiddenstates
init_hmm_wcd_9 = build_hmm(observations=stateseqwcd, n_states=9)

#Fit the model to the given observations/sequences
hmm_wcd_9 = fit_model(init_hmm_wcd_9)

###### 10 hiddenstates
init_hmm_wcd_10 = build_hmm(observations=stateseqwcd, n_states=10)

#Fit the model to the given observations/sequences
hmm_wcd_10 = fit_model(init_hmm_wcd_10)

###### 12 hiddenstates
init_hmm_wcd_12 = build_hmm(observations=stateseqwcd, n_states=12)

#Fit the model to the given observations/sequences
hmm_wcd_12 = fit_model(init_hmm_wcd_12)

###### 14 hiddenstates
init_hmm_wcd_14 = build_hmm(observations=stateseqwcd, n_states=14)

#Fit the model to the given observations/sequences
hmm_wcd_14 = fit_model(init_hmm_wcd_14)

###### 16 hiddenstates
init_hmm_wcd_16 = build_hmm(observations=stateseqwcd, n_states=16)

#Fit the model to the given observations/sequences
hmm_wcd_16 = fit_model(init_hmm_wcd_16)

###### 18 hiddenstates
init_hmm_wcd_18 = build_hmm(observations=stateseqwcd, n_states=18)

#Fit the model to the given observations/sequences
hmm_wcd_18 = fit_model(init_hmm_wcd_18)

###### 20 hiddenstates
init_hmm_wcd_20 = build_hmm(observations=stateseqwcd, n_states=20)

#Fit the model to the given observations/sequences
hmm_wcd_20 = fit_model(init_hmm_wcd_20)

#Calculate the hidden state path sequence for each day
hidpaths_wcd_20 = hidden_paths(hmm_wcd_20$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidwcd_20.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd_20$model, hidpaths_wcd_20,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0','#911eb4','#d2f53c','#fabebe','#e6beff','#aa6e28','#fffac8','#800000','#aaffc3','#808000','#ffd8b1','#000080','#808080','#FFFFFF','#000000'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_wcd_20$model,
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

###### 25 hiddenstates
init_hmm_wcd_25 = build_hmm(observations=stateseqwcd, n_states=25)

#Fit the model to the given observations/sequences
hmm_wcd_25 = fit_model(init_hmm_wcd_25)

#Calculate the hidden state path sequence for each day
hidpaths_wcd_25 = hidden_paths(hmm_wcd_25$model)

#######visualize model
jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/1hidwcd_25.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wcd_25$model, hidpaths_wcd_25,plots="hidden.paths", type = "I", hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0','#911eb4','#d2f53c','#fabebe','#e6beff','#aa6e28','#fffac8','#800000','#aaffc3','#808000','#ffd8b1','#000080','#808080','#FFFFFF','#000000', '#914CB1', '#97C161', '#B65A49', '#737F86'),
       legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_wcd_25$model,
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

############# midnight

###########Gympie
gmn = readBStringSet('/Volumes/Nifty/QUT/60clusters/Fastafiles/Gympie_letters_midnight.txt', format="fasta",
                     nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)

stateseqgmn = seqdef(gmn, id = names(gmn), missing = '-', cpal = colsammap,
                     missing.color = "black",
                     labels = c("01","02","03","04","05","06","07","08","09",seq(10,60)))
#######8 hiddenstates
init_hmm_gmn_8 = build_hmm(observations=stateseqgmn, n_states=8)
#Fit the model to the given observations/sequences
set.seed(3995)
hmm_gmn_8 = fit_model(init_hmm_gmn_8)
hidpaths_gmn_8 = hidden_paths(hmm_gmn_8$model)

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgmn_8.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gmn_8$model, hidpaths_gmn_8,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0','#911eb4'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/statetransitions/nicelayout_gmn_8.jpeg", 2000, 2000, quality = 100)
plot(hmm_gmn_8$model,
     layout = matrix(c(0,-1,0,0,1,-1,-1,1,-1,-1,0,1,0,1,0,-1),ncol=2), pie = T,
     xlim = c(-1.2, 1.2), ylim = c(-1.2,1.2), rescale = F,
     combine.slices = F,
     vertex.size = 40, vertex.label = "names", vertex.label.dist = 1.0,
     vertex.label.cex = 1.1, vertex.label.family = 'mono',
     trim = 0.005, label.signif = 2,
     edge.curved = F, edge.arrow.size = 0.6, edge.width = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=F, legend.prop = 0.3, cex.legend = 0.4, ncol.legend = 8,
     cpal = colsammap,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)))
#legend("right", c("01","02","03","04","05","06","07","08","09",seq(10,60)),
 #      cex = 0.5, ncol = 8)
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/coloredhidgmn_8.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gmn_8$model, hidpaths_gmn_8,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('green','orange3' ,'lightblue', 'grey' ,'blue' ,'yellow' ,'orange','lightgreen'),
       hidden.states.labels = c('1. Birds', '2. Cicadas/Wind', '3. Wind','4. Quiet','5. Rain', '6. Insects', '7. Cicadas', '8. Birds'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

em_gmn_8 = hmm_gmn_8$model$emission_probs
colnames(em_gmn_8) = label_no
s_em_gmn_8 = em_gmn_8[ ,order(colnames(em_gmn_8))]
s_em_gmn_8_nor = (s_em_gmn_8/s_labeled$Freq)*100
barplot(s_em_gmn_8, cex.names = 0.5, names.arg = colnames(s_em_gmn_8),las =2,
        col = c('green','orange3' ,'lightblue', 'grey' ,'blue' ,'yellow' ,'orange','lightgreen'))
barplot(s_em_gmn_8_nor, cex.names = 0.5, names.arg = colnames(s_em_gmn_8),las =2,
        col = c('green','orange3' ,'lightblue', 'grey' ,'blue' ,'yellow' ,'orange','lightgreen'))
legend("topright", cex = 0.6, bty="n",
       legend = c('1. Birds', '2. Cicadas/Wind', '3. Wind','4. Quiet','5. Rain', '6. Insects', '7. Cicadas', '8. Birds'),
       fill = c('green','orange3' ,'lightblue', 'grey' ,'blue' ,'yellow' ,'orange','lightgreen'))

#######9 hiddenstates
init_hmm_gmn_9 = build_hmm(observations=stateseqgmn, n_states=9)
#Fit the model to the given observations/sequences
set.seed(3995)
hmm_gmn_9 = fit_model(init_hmm_gmn_9)
hidpaths_gmn_9 = hidden_paths(hmm_gmn_9$model)

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgmn_9.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gmn_9$model, hidpaths_gmn_9,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0','#911eb4','#d2f53c'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()


jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/statetransitions/nicelayout_gmn_9.jpeg", quality = 100)
plot(hmm_gmn_9$model,
     layout = matrix(c(0,1,-1,0,1,-1,1,-1,0,-1,-1,-1,1,1,1,0,0,0),ncol=2), pie = T,
     xlim = c(-1.2, 1.2), ylim = c(-1.2,1.2), rescale = F,
     combine.slices =F,
     vertex.size = 40, vertex.label = "names", vertex.label.dist = 1.0,
     vertex.label.cex = 1.1, vertex.label.family = 'mono',
     trim = 0.005, label.signif = 2,
     edge.curved = F, edge.arrow.size = 0.6, edge.width = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=F, legend.prop = 0.3, cex.legend = 0.4, ncol.legend = 8,
     cpal = colsammap,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)))
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/coloredhidgmn_9.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gmn_9$model, hidpaths_gmn_9,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('lightgrey','darkgrey' ,'blue', 'orange' ,'yellow' ,'green3' ,'lightblue','green','orange3'),
       hidden.states.labels = c('1. Quiet', '2. Quiet', '3. Rain','4. Cicadas','5. Insects', '6. Birds', '7. Wind', '8. Birds', '9. Cicadas/Wind'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

em_gmn_9 = hmm_gmn_9$model$emission_probs
colnames(em_gmn_9) = label_no
s_em_gmn_9 = em_gmn_9[ ,order(colnames(em_gmn_9))]
barplot(s_em_gmn_9, cex.names = 0.5, las =2,
        col = c('lightgrey','darkgrey' ,'blue', 'orange' ,'yellow' ,'green3' ,'lightblue','green','orange3'))
legend("topright", cex = 0.6, bty="n",
       legend = c('1. Quiet', '2. Quiet', '3. Rain','4. Cicadas','5. Insects', '6. Birds', '7. Wind', '8. Birds', '9. Cicadas/Wind'),
       fill = c('lightgrey','darkgrey' ,'blue', 'orange' ,'yellow' ,'green3' ,'lightblue','green','orange3'))

#######10 hiddenstates
init_hmm_gmn_10 = build_hmm(observations=stateseqgmn, n_states=10)
#Fit the model to the given observations/sequences
set.seed(3995)
hmm_gmn_10 = fit_model(init_hmm_gmn_10)
hidpaths_gmn_10 = hidden_paths(hmm_gmn_10$model)

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidgmn_10.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gmn_10$model, hidpaths_gmn_10,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0','#911eb4','#d2f53c', '#fabebe'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/statetransitions/nicelayout_gmn_9.jpeg", quality = 100)
plot(hmm_gmn_10$model,
     layout = matrix(c(-1,-1,1,0,0,1,0,0,-1,-1,-1,0,1,2,0,0,1,-1,1,2),ncol=2), pie = T,
     xlim = c(-1.2, 1.2), ylim = c(-1.2,2.2), rescale = F,
     combine.slices =F,
     vertex.size = 30, vertex.label = "names", vertex.label.dist = 1.0,
     vertex.label.cex = 1.1, vertex.label.family = 'mono',
     trim = 0.005, label.signif = 2,
     edge.curved = F, edge.arrow.size = 0.6, edge.width = 0.5,
     loops = TRUE, edge.loop.angle = -pi/8,
     withlegend=F, legend.prop = 0.3, cex.legend = 0.4, ncol.legend = 8,
     cpal = colsammap,
     ltext = c("01","02","03","04","05","06","07","08","09",seq(10,60)))
dev.off()

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/coloredhidgmn_10.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gmn_10$model, hidpaths_gmn_10,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('green3','grey80' ,'orange3', 'orange' ,'lightblue' ,'pink' ,'yellow','green','grey50','grey70'),
       hidden.states.labels = c('1. Birds', '2. Quiet', '3. Cicadas/Wind','4. Cicadas/insects','5. Wind', '6. Mix', '7. Insects', '8. Birds', '9. Quiet','10. Quiet/rain'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

em_gmn_10 = hmm_gmn_10$model$emission_probs
colnames(em_gmn_10) = label_no
s_em_gmn_10 = em_gmn_10[ ,order(colnames(em_gmn_10))]
barplot(s_em_gmn_10, cex.names = 0.5, las =2,
        col = c('green3','grey80' ,'orange3', 'orange' ,'lightblue' ,'pink' ,'yellow','green','grey50','grey70'))
legend("topright", cex = 0.55, bty="n",
       legend =c('1. Birds', '2. Quiet', '3. Cicadas/Wind','4. Cicadas/insects','5. Wind', '6. Mix', '7. Insects', '8. Birds', '9. Quiet','10. Quiet/rain'),
       fill = c('green3','grey80' ,'orange3', 'orange' ,'lightblue' ,'pink' ,'yellow','green','grey50','grey70'))

###########Woondum
wmn = readBStringSet('/Volumes/Nifty/QUT/60clusters/Fastafiles/Woondum_letters_midnight.txt', format="fasta",
                     nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)

stateseqwmn = seqdef(wmn, id = names(wmn), missing = '-', cpal = colorpalette[[60]],
                     missing.color = "black",
                     labels = c("01","02","03","04","05","06","07","08","09",seq(10,60)))
#######8 hiddenstates
init_hmm_wmn_8 = build_hmm(observations=stateseqwmn, n_states=8)
#Fit the model to the given observations/sequences
hmm_wmn_8 = fit_model(init_hmm_wmn_8)
hidpaths_wmn_8 = hidden_paths(hmm_wmn_8$model)

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hidwmn_8.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wmn_8$model, hidpaths_wmn_8,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('#e6194b','#3cb44b' ,'#ffe119', '#0082c8' ,'#f58231' ,'#f032e6' ,'#46f0f0','#911eb4'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

plot(hmm_wmn_8$model,
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

jpeg("/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/coloredhidwmn_8.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_wmn_8$model, hidpaths_wmn_8,plots="hidden.paths", type = "I", 
       hidden.states.colors = c('lightblue','orange' ,'lightgrey', 'yellow3' ,'yellow' ,'darkgrey' ,'green','blue'),
       hidden.states.labels = c('1. Wind', '2. Cicadas', '3. Quiet','4. Birds/Insects','5. Insects', '6. Quiet', '7. Birds', '8. Rain'),
       yaxis = T,legend.prop = 0.3, cex.legend = 3)
dev.off()

em_wmn_8 = hmm_wmn_8$model$emission_probs
colnames(em_wmn_8) = label_no
s_em_wmn_8 = em_wmn_8[ ,order(colnames(em_wmn_8))]
barplot(s_em_wmn_8, cex.names = 0.5, las =2,
        col =c('lightblue','orange' ,'lightgrey', 'yellow3' ,'yellow' ,'darkgrey' ,'green','blue'))
legend("topright", cex = 0.7, bty="n",
       legend = c('1. Wind', '2. Cicadas', '3. Quiet','4. Birds/Insects','5. Insects', '6. Quiet', '7. Birds', '8. Rain'),
       fill = c('lightblue','orange' ,'lightgrey', 'yellow3' ,'yellow' ,'darkgrey' ,'green','blue'))
