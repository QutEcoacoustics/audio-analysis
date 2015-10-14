% apply band pass filter to the waveform

lowFreq = csvread('.\08-28\lowFreq.csv');
highFreq = csvread('.\08-28\highFreq.csv');




% read waveform

audioPath = 'C:\work\dataset\audio data\MIML\validation set for parameter setting\bioacoustic JCU data\Canetoad\Stony_creek_dam_-_Herveys_Range_1078_251248_20140323_000000_10.0__.wav';
[y, fs] = audioread(audioPath);

% design a band pass filter
Hd1 = filter_Jie_butter(lowFreq(1), highFreq(1));

signal = filter(Hd1, y);

winSize = 512; winOver = 0.85;
[spec,F,T] = wav_to_spec(signal, fs, winSize, winOver);

[M, N] = size(spec);
frameSecond = N / max(T);
herzBin = fs / 2 / M;

% apply wiener filter
spec2 = wiener2(spec, [7 7]);
 
spec3 = noise_reduce(spec2, frameSecond);

imagesc(spec3);
axis xy;



