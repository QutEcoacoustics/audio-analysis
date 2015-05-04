function [I1,F1,T1] = wav_to_spec(y, fs, windowSize, windowOverlap)

window = windowSize; % hamming window using 512 samples
noverlap = round(windowOverlap * window); % 50% overlap between frames
nfft = windowSize * 2; % yield 257 frequency bins to be consistent with C# libraries
[~,F,T,P] = spectrogram(y,hann(window),noverlap,nfft,fs);

% P matrix has 257 rows and values of F vector go from 0 to 11025.
% Assuming first row are DC values and first frequencey bin is row 2.
% Chop off DC values so we have a 1-1 correspondence between pixel and
% freq/time co-ordinate systems.
P1 = P(2:(windowSize + 1),:); % amplitude
F1 = F(2:(windowSize + 1));

% We want the time vector to start from zero, i.e. the beginning of each
% time frame rather than the end. So when we convert to acoustic events
% from pixel co-ordinates to time/freq it is consistent with the F#/C#
% code.
T1 = T - T(1);

% convert amplitude to dB
I1 = 20*log10(abs(P1));