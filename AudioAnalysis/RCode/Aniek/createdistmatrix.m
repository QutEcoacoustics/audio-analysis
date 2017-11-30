seqs = fastaread('/Volumes/Nifty/QUT/HMM/hiddenstates/fastafiles/combcd.txt');
seqsyeargympie = seqs(1:366,:);
seqsyearwoondum = seqs(397:762,:);
load('/Volumes/Nifty/QUT/HMM/hiddenstates/scoring_matrix_matlab.mat');


distmatrixgympie = seqpdist(seqsyeargympie, 'Method', 'p-distance', 'Indels', 'score', 'Scoringmatrix', scoring_matrix , 'SquareForm', true, 'Pairwisealignment', true);
distmatrixwoondum = seqpdist(seqsyearwoondum, 'Method', 'p-distance', 'Indels', 'score', 'Scoringmatrix', scoring_matrix , 'SquareForm', true, 'Pairwisealignment', true);

csvwrite('/Volumes/Nifty/QUT/HMM/hiddenstates/distmatrix365gympiecd',distmatrixgympie)
csvwrite('/Volumes/Nifty/QUT/HMM/hiddenstates/distmatrix365woondumcd',distmatrixwoondum)

