function [y3,fs,I10,F2,T2]=smoothSpectrogram(S,fs)
window1 = 256; % hamming window using 512 samples
noverlap = round(0.0*window1);
nfft =window1; % yield 257 frequency bins to be consistent with C# libraries
[y3,F1,T1,P] = spectrogram(S,window1,noverlap,nfft,fs);

P2 = P(2:(window1/2+1),:);
F2 = F1(2:(window1/2+1));

% We want the time vector to start from zero, i.e. the beginning of each
% time frame rather than the end. So when we convert to acoustic events
% from pixel co-ordinates to time/freq it is consistent with the F#/C#
% code.
T2 = T1 - T1(1);
% convert amplitude to dB
I10= 10*log10(abs(P2));
end