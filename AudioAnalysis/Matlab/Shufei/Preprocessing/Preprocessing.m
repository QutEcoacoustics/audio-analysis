%--Preprocessing aiming to protrude the acoustic components out of background noise--%
function [y,fs,I1,F2,T2] = Preprocessing(PathToInputFile)%output vailiable has to be changed accordingly 
[y,fs,nbits, opts]=wavread(PathToInputFile);% Reading the raw data
% Second step is to frame and window it with the sample rate 11kHz(Fs=22kHz according to Naiquist Theory)
window = 512; % hamming window using 512 samples
overlap = round(0.5*window); % 50% overlap between frames
fft=256*2;
[S,F,T,P]=spectrogram(y,window,overlap,fft,fs);
P2 = P(2:257,:);
F2 = F(2:257);

% We want the time vector to start from zero, i.e. the beginning of each
% time frame rather than the end. So when we convert to acoustic events
% from pixel co-ordinates to time/freq it is consistent with the F#/C#
% code.
T2 = T - T(1);

% convert amplitude to dB
I1 = 10*log10(abs(P2));
showImage(I1,T,F,1);