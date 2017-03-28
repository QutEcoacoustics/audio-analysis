clear;
clc;
index = csvread('C:/Work/myfile/birdIndex.csv');
index = index - 1;
% index = index(1:2);
for i=1:length(index)

%preprocessing
amp = preprocess(index(i));

%run NMF
[W, H] = NMF_decompose(amp);

%save the results
bases{i} = W;
coefficients{i} = H;
end

save('c:/work/myfile/bases.mat', 'bases');
save('c:/work/myfile/coefficients.mat', 'coefficients');