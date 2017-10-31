##Create dotplot in R
#Load data
library(ggplot2)
library(dotplot)
gympie_seasonal = read.fasta('/Volumes/Nifty/QUT/HMM/hiddenstates/fastafiles/gympie/consensus_gympie_seasonal.txt', as.string=F,seqtype='AA')

woondum_seasonal = read.fasta('/Volumes/Nifty/QUT/HMM/hiddenstates/fastafiles/woondum/consensus_woondum_seasonal.txt', as.string=F,seqtype='AA')

dotPlot(woondum_seasonal$`Woondum|Spring|civildawn`,gympie_seasonal$`Gympie|Spring|civildawn`)
dotPlot(woondum_seasonal$`Woondum|Summer|civildawn`,gympie_seasonal$`Gympie|Summer|civildawn`)
dotPlot(woondum_seasonal$`Woondum|Autumn|civildawn`,gympie_seasonal$`Gympie|Autumn|civildawn`)