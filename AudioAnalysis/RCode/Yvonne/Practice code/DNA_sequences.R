library(seqinr)
#leprae <- read.fasta(file = "Q9CD83.fasta")
leprae <- read.fasta(file = "http://www.uniprot.org/uniprot/Q9CD83.fasta")
one <- leprae
two <- u
#ulcerans <- read.fasta(file = "A0PQ23.fasta")
ulcerans <- read.fasta(file = "http://www.uniprot.org/uniprot/A0PQ23.fasta")
lepraeseq <- leprae[[1]]
ulceransseq <- ulcerans[[1]]
lepraeseq[1:210]
ulceransseq[1:210]
dotPlot(lepraeseq[1:200], ulceransseq[1:200])

detach("package:IRanges", unload=TRUE)

c <- chartr(old = c("1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19","20","21","22","23","24","25","26","27"), 
            new = c("A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P",
                    "Q","R","S","T","U","V","W","X","Y","Z","a"),a)
head(a)
data(BLOSUM50)