function [y,fs,I1,F2,T2,window1] = wavToSpectrogram(pathToFile)

[y, fs, nbits, opts] = wavread(pathToFile);

% window1=1024;
%window = 256;
window1 = 512; % hamming window using 512 samples
noverlap = round(0.0*window1);
%noverlap = round(0.5*window); % 50% overlap between frames
%nfft=512*2;
nfft =window1; % yield 257 frequency bins to be consistent with C# libraries
[S,F,T,P] = spectrogram(y,window1,noverlap,nfft,fs);

% P matrix has 257 rows and values of F vector go from 0 to 11025.
% Assuming first row are DC values and first frequencey bin is row 2.
% Chop off DC values so we have a 1-1 correspondence between pixel and
% freq/time co-ordinate systems.
%P2=P(2:513,:);
%F2=F(2:513);
P2 = P(2:(window1/2+1),:);
F2 = F(2:(window1/2+1));

% We want the time vector to start from zero, i.e. the beginning of each
% time frame rather than the end. So when we convert to acoustic events
% from pixel co-ordinates to time/freq it is consistent with the F#/C#
% code.
T2 = T - T(1);

% convert amplitude to dB
I1 = 10*log10(abs(P2));