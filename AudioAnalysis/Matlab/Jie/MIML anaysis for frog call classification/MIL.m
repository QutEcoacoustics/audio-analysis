% multiple instance learning for frog call classification
% each recording is 10 seconds
clc; close all; clear;

audioFolder = 'C:\work\dataset\MIML data - 10s\*.wav';
audioFolderName = 'C:\work\dataset\MIML data - 10s\fileName';
audioList = dir(audioFolder);
% remove hidden files
audioList = audioList(arrayfun(@(x) ~strcmp(x.name(1), '.'), audioList));

% iAudio = 1;
% audioName = audioList(iAudio);
% audioPath = strrep(audioFolderName, 'fileName', audioName.name);

% audioPath = '.\recordings for evaluating AED method\PC4_20100705_070000_0010.wav';
% audioPath = 'C:\work\dataset\MIML data - 10s\recordings to be labelled - example\Black_gin_creek_dam_-_Herveys_Range_1079_250868_20140213_220000_10.0__.wav';
% audioPath = 'C:\work\dataset\MIML data - 10s\recordings to be labelled\Black_gin_creek_dam_-_Herveys_Range_1079_250868_20140213_200000_10.0__.wav';
% audioPath = 'C:\work\dataset\MIML data - 10s\recordings to be labelled\Black_gin_creek_dam_-_Herveys_Range_1079_250868_20140213_220000_10.0__.wav';
% audioPath = 'C:\work\dataset\MIML data - 10s\recordings to be labelled\Black_gin_creek_dam_-_Herveys_Range_1079_250870_20140215_195000_10.0__.wav';
audioPath = '.\recordings for evaluating AED method\Black_gin_creek_dam_-_Herveys_Range_1079_250866_20140211_210000_10.0__.wav';

[y, fs] = audioread(audioPath);

% Gammatone-like spectrogram
% sr = fs; d = y;
% tic; [D,F] = gammatonegram(d,sr,0.025,0.010,64,500,sr/2,0); toc
% %Elapsed time is 3.165083 seconds.
% subplot(212)
% imagesc(20*log10(D));  axis xy
% caxis([-90 -30])
% colorbar
% set(gca,'YTickLabel',round(F(get(gca,'YTick'))));
% ylabel('freq / Hz');
% xlabel('time / 10 ms steps');
% title('Gammatonegram - accurate method')

% spectrogram

winSize = 256; winLap= 0.85;

[spec, F, T] = wav_to_spec(y, fs, winSize, winLap);

[M, N] = size(spec);

frequency = max(F); time = max(T);
resTime = time / N;

% high and low pass filter
% highFreq = 10000; lowFreq = 500;
% highBin = floor(highFreq / (fs / 2) * M); lowBin = floor(lowFreq / (fs / 2) * M);
% spec(1:lowBin,:) = 0; spec(highBin:M,:) = 0;

specClean = noise_reduce(spec, resTime);

% acoustic event detection
% compare 4 methods for segmentation
 
% Method1
%  AE1 = AEDFodor(specClean, T, F);
% Method 2
% AE1 = AEDPotamitis(specClean, T, F);
% Method 3
% AE1 = AEDLasseck(specClean, T, F);
% Jie's AED method
AE1 = AEDJie(specClean, T, F);

% extract dominant spectral peak track

show_image(jet, spec, T, F, 1, AE1);

 

