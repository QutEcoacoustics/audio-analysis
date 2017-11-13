trans_matrix = cell2mat(struct2cell(load('/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/tm_wcd_5.mat')));
tr = trans_matrix;

emiss_matrix = cell2mat(struct2cell(load('/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/em_wcd_5.mat')));
e = emiss_matrix;

seqs = fastaread('/Volumes/Nifty/QUT/60clusters/Fastafiles/Gympie_letters_civildawn.txt');
seqs = seqs(1:397,:);

sym='ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}';

hiddenpaths = zeros(397,1438);
for x =1:397
    seq = seqs(x).Sequence;
    seq = strrep(seq,'-','NaN');
    estimatedStates = hmmviterbi(seq,tr,e,'Symbols',sym);
    hiddenpaths(x,:) = estimatedStates(1:1438);
end

save('/Volumes/Nifty/QUT/60clusters/HMM/hiddenstates/hp_wcdgcd_5.mat','hiddenpaths');
