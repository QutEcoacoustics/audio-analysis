function [y,fs,I1,F,T,fmax,tmax] = wavToSpectrogram(pathToFile)

[y, fs, nbits, opts] = wavread(pathToFile);

%TODO should this be T(end)+T(1)
tmax = length(y)/fs; %length of signal in seconds

window = 512; % hamming window using 512 samples
noverlap = round(0.5*window); % 50% overlap between frames
nfft = 256*2; % yield 512 frequency bins
[S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);

% TODO which fmax?
fmax = F(end);
%fmax = 11025;

% convert amplitude to dB
I1 = 10*log10(abs(P));