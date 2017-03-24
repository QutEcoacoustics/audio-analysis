function [amp] = preprocess(index)

%read the signal and sampling frequency
[signal]=audioread(['C:\Work\myfile\2014Aug15-002542Indices\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000_' ...
    num2str(index) 'min.wav']);

%calculate complex values of spectrogram
s = spectrogram(signal(:,1), 512, 0, 512);

%calculate amplitudes of spectrograms where frequency bins range from 31 to
%246
amp = abs(s(31:246, :));
