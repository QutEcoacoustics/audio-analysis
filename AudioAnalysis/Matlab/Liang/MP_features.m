clear;
clc;
iteration = 1000;
category = 'wind';
filename = dir(['C:\Work\myfile\MP-feature\raw audio\' category]);
len = length(filename);

MP_freq_mean = zeros(len - 2, 1);
MP_freq_std = zeros(len - 2, 1);
MP_amp_mean = zeros(len - 2 , 1);
MP_amp_std = zeros(len - 2, 1);

dict = dictread('C:\\Program Files (x86)\\MPTK\\mptk\\reference\\dictionary\\dic_gabor_one_scale.xml');

%2nd-order butterworth highpass filter
[B,A] = butter(2, 1000/8820, 'high');

for i = 3:len
% for i = 3
    [signal, sampleRate] = sigread(['C:\Work\myfile\MP-feature\raw audio\' category '\' filename(i).name]);
    signal = filter(B, A, signal);
    book = mpdecomp(signal(:,1),sampleRate,dict,iteration);
    MP_freq_mean(i - 2) = mean(book.atom.params.freq);
    MP_freq_std(i - 2) = std(book.atom.params.freq);
    MP_amp_mean(i - 2) = mean(book.atom.params.amp);
    MP_amp_std(i - 2) = std(book.atom.params.amp);
end
result = [MP_amp_mean, MP_amp_std, MP_freq_mean, MP_freq_std];

%csvwrite('c:/work/myfile/mp_rain.csv',rain);