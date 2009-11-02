function [y,fs,S,F,T,P,fmax,tmax] = wavToSpectrogram(pathToFile)

[y, fs, nbits, opts] = wavread(pathToFile);

%TODO should this be T(end)+T(1)
tmax = length(y)/fs; %length of signal in seconds
%fmax = 11025;

window = 512; % hamming window using 512 samples
noverlap = round(0.5*window); % 50% overlap between frames
nfft = 256*2; % yield 512 frequency bins
[S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);

fmax = F(end);