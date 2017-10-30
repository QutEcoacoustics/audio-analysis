require(ggplot2)
require(ggseqlogo)
library('Biostrings')
l
subset = function(fasta, start, stop){
  subsetseq = c()
  for (seq in fasta){
    subsetseq = c(subsetseq, substr(seq,start,stop))
  }
  return(subsetseq)
}

fasta = readBStringSet('/Volumes/Nifty/QUT/Scripts/Gympie_letters_midnight.txt', format="fasta",
               nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)
fasta1 = readAAStringSet('/Volumes/Nifty/QUT/Scripts/Gympie_letters_midnight.txt', format="fasta",
                        nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)
fasta2 = read.fasta('/Volumes/Nifty/QUT/Fastafiles/Gympie_letters_midnight.txt',as.string='TRUE',seqonly='TRUE')

ggplot() + geom_logo(fasta2,seq_type="other",col_scheme = 'taylor',namespace="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",font="akrobat_bold") + theme_logo()

fastacd = read.fasta('/Volumes/Nifty/QUT/Scripts/Gympie_letters_civildawn.txt',as.string='TRUE',seqonly='TRUE')
fastacdvec = read.fasta('/Volumes/Nifty/QUT/Fastafiles/Gympie_letters_civildawn.txt')
fastacd2 = readBStringSet('/Volumes/Nifty/QUT/Fastafiles/Gympie_letters_civildawn.txt', format="fasta",
                          nrec=-1L, skip=0L, seek.first.rec=FALSE, use.names=TRUE)

ggplot() + geom_logo(fastacd700,method = 'probability',seq_type="other",col_scheme = 'taylor',namespace="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",font="akrobat_bold") + theme_logo()
january = fastacd[1:31]
january100 = subset(january,1,100)
ggplot() + geom_logo(january,method = 'probability',seq_type="other",col_scheme = 'taylor',namespace="BCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",font="akrobat_bold") + theme_logo()

#Calculate the frequency of each letter per sequence gympie
freq = letterFrequency(fasta, letters = uniqueLetters(fasta))
freqtotal = sort(colSums(freq),decreasing=TRUE)
freqtotalperc = (freqtotal*100)/sum(freqtotal)
barplot(freqtotalperc,names.arg = names(freqtotal),las='2', col = colorpalette[[50]])
