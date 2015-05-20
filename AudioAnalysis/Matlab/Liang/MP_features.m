clear;
clc;
iteration = 500;
% category = 'bird';   %choice: 'bird','insect','low activity','rain','wind'
category = {'bird', 'insect', 'low activity', 'rain', 'wind'};
dict = dictread('C:\\Program Files (x86)\\MPTK\\mptk\\reference\\dictionary\\dic_chirp_16384p.xml');
flag = false;

%2nd-order butterworth highpass filter
[B,A] = butter(2, 1000/8820, 'high');

for m = 1:length(category)
    filename = dir(['C:\Work\myfile\MP-feature\raw audio_ver2\' category{m}]);
    len = length(filename);
    
    if flag == false
        MP_freq_mean = zeros(len - 2, length(category));
        MP_freq_std = zeros(len - 2, length(category));
        MP_SRR = zeros(len - 2, length(category));
        MP_chirp_mean = zeros(len - 2, length(category));
        MP_chirp_std = zeros(len - 2, length(category));
        MP_pos_mean = zeros(len - 2, length(category));
        MP_pos_std = zeros(len - 2, length(category));
        flag = true;
    end
    
    for n = 3:len
    % for i = 3
        [signal, sampleRate] = sigread(['C:\Work\myfile\MP-feature\raw audio_ver2\' category{m} '\' filename(n).name]);
        signal = filter(B, A, signal);
        [book, residual] = mpdecomp(signal(:,1),sampleRate,dict,iteration);
        MP_SRR(n - 2, m) = 10 * log10(sum(signal(:,1).^2) / sum(residual.^2));
        MP_chirp_mean(n - 2, m) = mean(book.atom.params.chirp);
        MP_chirp_std(n - 2, m) = std(book.atom.params.chirp);
        MP_pos_mean(n - 2, m) = mean(book.atom.params.pos);
        MP_pos_std(n - 2, m) = std(book.atom.params.pos);
        MP_freq_mean(n - 2, m) = mean(book.atom.params.freq);
        MP_freq_std(n - 2, m) = std(book.atom.params.freq);
    end
end

MP_SRR = reshape(MP_SRR, 150, 1);
MP_chirp_mean =reshape(MP_chirp_mean, 150, 1);
MP_chirp_std = reshape(MP_chirp_std, 150, 1);
MP_pos_mean = reshape(MP_pos_mean, 150, 1);
MP_pos_std = reshape(MP_pos_std, 150, 1);
MP_freq_mean = reshape(MP_freq_mean, 150, 1);
MP_freq_std = reshape(MP_freq_std, 150, 1);
result = [MP_SRR, MP_pos_mean, MP_pos_std, MP_freq_mean, MP_freq_std, MP_chirp_mean, MP_chirp_std];

%csvwrite('c:/work/myfile/MP-feature/mp_rain.csv',result);