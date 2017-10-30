library('seqHMM')
library('TraMineR')
library('seqinr')
########### gympie civil dawn
#read in the sequence from the fasta file
fastacd2gympie = readBStringSet('/Volumes/Nifty/QUT/Fastafiles/Gympie_letters_civildawn.txt', format="fasta",
                                nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)
#Create sequences of equal length, therefore remove the last day because it is significantly shorter
#And remove the last one/two minutes of each sequence so they are all equal
fastacd2gympie = fastacd2gympie[1:397]
gympiecd = subseq(fastacd2gympie, 1, 1438)

#Create a state sequence object that can be used to build a HMM
stateseqgympiecd = seqdef(gympiecd,id = names(gympiecd), missing = '-')

#Initialize the HMM with 7 different states representing rain, birds, etc..
init_hmm_gympiecd = build_hmm(observations=stateseqgympiecd, n_states=7)

#Fit the model to the given observations/sequences
hmm_gympiecd = fit_model(init_hmm_gympiecd)

#Calculate the hidden state path sequence for each day
hidpaths_gympiecd = hidden_paths(hmm_gympiecd$model)

#Plot out the hidden state path sequences
jpeg("/Volumes/Nifty/QUT/HMM/hiddenstates/gympiecd.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_gympiecd$model, hidpaths_gympiecd,plots="hidden.paths", type = "I")
dev.off()

#Show the state transition probabilities and emission probabilties in the form of a graph
jpeg("/Volumes/Nifty/QUT/HMM/statetransitiongympiecd.jpeg", 1000, 1000, quality = 100)
plot(hmm_gympiecd$model,vertex.label='names',combine.slices = 0.05, cpal = colorpalette[[50]])
dev.off()

########### woondum civil dawn
fastacd2woondum = readBStringSet('/Volumes/Nifty/QUT/Fastafiles/Woondum_letters_civildawn.txt', format="fasta",
                                nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)
fastacd2woondum = fastacd2woondum[1:397]
woondumcd = subseq(fastacd2woondum, 1, 1438)
stateseqwoondumcd = seqdef(woondumcd,id = names(woondumcd), missing = '-')
init_hmm_woondumcd = build_hmm(observations=stateseqwoondumcd, n_states=7)
hmm_woondumcd = fit_model(init_hmm_woondumcd)
hidpaths_woondumcd = hidden_paths(hmm_woondumcd$model)

jpeg("/Volumes/Nifty/QUT/HMM/hiddenstates/woondumcd.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_woondumcd$model, hidpaths_woondumcd,plots="hidden.paths", type = "I")
dev.off()

jpeg("/Volumes/Nifty/QUT/HMM/statetransitionwoondumcd.jpeg", 1000, 1000, quality = 100)
plot(hmm_woondumcd$model,vertex.label='names',combine.slices = 0.05, cpal = colorpalette[[50]])
dev.off()

########### gympie midnight
fastamn2gympie = readBStringSet('/Volumes/Nifty/QUT/Fastafiles/Gympie_letters_midnight.txt', format="fasta",
                                nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)

########### woondum midnight
fastamn2woondum = readBStringSet('/Volumes/Nifty/QUT/Fastafiles/Woondum_letters_midnight.txt', format="fasta",
                                 nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)
stateseqwoondum = seqdef(fastamn2woondum,id = names(fastamn2woondum), missing = '-')
init_hmm_woondummn = build_hmm(observations=stateseqwoondum, n_states=7)
hmm_woondummn = fit_model(init_hmm_woondummn)
hidpaths_woondummn = hidden_paths(hmm_woondummn$model)

#two channels, one for each location civildawn
stateseqs = list(stateseqgympiecd,stateseqwoondumcd)
init_hmm_bothcd = build_hmm(observations=stateseqs, n_states = 7, channel_names = c("Gympie","Woondum"))
hmm_bothcd = fit_model(init_hmm_bothcd)
hidpaths_bothcd = hidden_paths(hmm_bothcd$model)

jpeg("/Volumes/Nifty/QUT/HMM/hiddenstates/bothcd.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_bothcd$model, hidpaths_bothcd,plots="hidden.paths", type = "I")
dev.off()

jpeg("/Volumes/Nifty/QUT/HMM/bothcdobservationss.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_bothcd$model, hidpaths_bothcd,plots="obs", type = "I", cpal = colorpalette[[50]])
dev.off()

jpeg("/Volumes/Nifty/QUT/HMM/statetransitionbothcd.jpeg", 1000, 1000, quality = 100)
plot(hmm_woondumcd$model,vertex.label='names',combine.slices = 0.05, cpal = colorpalette[[50]])
dev.off()

#datasets of gympie and woondum combined civil dawn
combcd = append(gympiecd, woondumcd)
stateseqscd = seqdef(combcd,id = names(combcd), missing = '-', cpal = colorpalette[[50]])
init_hmm_combcd = build_hmm(observations=stateseqscd, n_states = 7, state_names = list("G","A","V","L","I","F","Y"))
hmm_combcd = fit_model(init_hmm_combcd)
hidpaths_combcd = hidden_paths(hmm_combcd$model)

jpeg("/Volumes/Nifty/QUT/HMM/hiddenstates/combcd.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_combcd$model, hidpaths_combcd,plots="hidden.paths", type = "I")
dev.off()

jpeg("/Volumes/Nifty/QUT/HMM/combcdobservations.jpeg", 2000, 2000, quality = 100)
ssplot(hmm_combcd$model, hidpaths_combcd,plots="obs", type = "I", tlim= 1:397)
dev.off()

jpeg("/Volumes/Nifty/QUT/HMM/statetransitioncombcd.jpeg", 1000, 1000, quality = 100)
plot(hmm_combcd$model,vertex.label='names',combine.slices = 0.05)
dev.off()

####### split hiddenstatepath data on seasons for Gympie
#days since the start of the experiment that correspond with the start and ending date of the season
winter =  c(346:397,1:71)
spring = 72:162
summer = 163:253
autumn = 254:345

#the hidden state sequences for each season in a stslist format 
winter_hidstateseqgympie = hidpaths_combcd[winter,][1:length(winter),]
spring_hidstateseqgympie = hidpaths_combcd[spring,][1:length(spring),]
summer_hidstateseqgympie = hidpaths_combcd[summer,][1:length(summer),]
autumn_hidstateseqgympie = hidpaths_combcd[autumn,][1:length(autumn),]

#creates a list of the hidden state sequences which can be used to write out a fasta file
makeseqlist = function(season,numberdays){
  seqlist = list()
  for (i in seq(1,numberdays,1)){
    seqlist[[i]] = t(season)[,i]
    i = i+ 1
  }
return(seqlist)
}

#Create a list containing the hidden state sequences and write out a fasta file
wintergympiecd = makeseqlist(winter_hidstateseqgympie, length(winter))
write.fasta(wintergympiecd, names = names(combcd)[winter], file.out = "/Volumes/Nifty/QUT/HMM/hiddenstates/fastafiles/wintergympiecd.txt")

springgympiecd = makeseqlist(spring_hidstateseqgympie, length(spring))
write.fasta(springgympiecd, names = names(combcd)[spring], file.out = "/Volumes/Nifty/QUT/HMM/hiddenstates/fastafiles/springgympiecd.txt")

summergympiecd = makeseqlist(summer_hidstateseqgympie, length(summer))
write.fasta(summergympiecd, names = names(combcd)[summer], file.out = "/Volumes/Nifty/QUT/HMM/hiddenstates/fastafiles/summergympiecd.txt")

autumngympiecd = makeseqlist(autumn_hidstateseqgympie, length(autumn))
write.fasta(autumngympiecd, names = names(combcd)[autumn], file.out = "/Volumes/Nifty/QUT/HMM/hiddenstates/fastafiles/autumngympiecd.txt")

test = readBStringSet("/Volumes/Nifty/QUT/HMM/hiddenstates/fastafiles/wintergympiecd.txt", format="fasta",
                      nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)
letterFrequency(test, uniqueLetters(test))