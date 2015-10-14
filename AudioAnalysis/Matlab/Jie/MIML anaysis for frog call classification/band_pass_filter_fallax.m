% apply band pass filter to the waveform
clear; close all; clc; 

lowFreq = csvread('.\08-28\lowFreq.csv');
highFreq = csvread('.\08-28\highFreq.csv');

% read waveform
% audioPath = 'C:\work\dataset\audio data\MIML\validation set for parameter setting\bioacoustic JCU data\Canetoad\Stony_creek_dam_-_Herveys_Range_1078_251248_20140323_003000_10.0__.wav';
audioPath = 'C:\work\dataset\audio data\MIML\validation set for parameter setting\bioacoustic JCU data\Canetoad\Stony_creek_dam_-_Herveys_Range_1078_251248_20140323_000000_10.0__.wav';
% audioPath = 'C:\work\dataset\audio data\MIML\validation set for parameter setting\bioacoustic JCU data\Litoria fallax\Stony_creek_dam_-_Herveys_Range_1078_251210_20140211_213000_10.0__.wav';
[y, fs] = audioread(audioPath);

% design a band pass filter
Hd1 = filter_Jie_butter(lowFreq(4), highFreq(4));
signal = filter(Hd1, y);

winSize = 512; winOver = 0.5;
[spec,F,T] = wav_to_spec(signal, fs, winSize, winOver);

[M, N] = size(spec);
frameSecond = N / max(T);
herzBin = fs / 2 / M;

% apply wiener filter
spec2 = wiener2(spec, [7 7]);
 
spec3 = noise_reduce(spec2, frameSecond);

lowBin = round(M * lowFreq(4) / (fs / 2));
highBin = round(M *  highFreq(4) / (fs / 2));

spec3(1:lowBin, :) = 0;
spec3(highBin:M,:) = 0;

% do AED
[spec4, sep] = otsu(spec3, 2);
spec5 = spec4 - 1;

% get acoustic events
[B, L] = bwboundaries(spec5,'noholes');

AE = [];
numB = length(B);
for bb = 1:numB
    
    thisB = B{bb, 1};
    thisrows = thisB(:,1);
    thiscols = thisB(:,2);
    AE(1,bb) = min(thiscols); %left
    AE(2,bb) = min(thisrows); %bottom
    AE(3,bb) = max(thiscols)-min(thiscols)+1; %width
    AE(4,bb) = max(thisrows)-min(thisrows)+1; % height

end

AE3 = AE;

[mAE, nAE] = size(AE);
area = zeros(1, nAE);
for i = 1:nAE
    area(i) = AE(3, i) * AE(4, i);        
end

% segment large events into small events
[newAE] = separate_large_AEs_areas(AE, L, 3000, spec);

% remove small events
smallAreaThresh = 500;
AE3 = mode_small_area_threshold(AE, smallAreaThresh);

% calculate the mean of peak frequency
[~, nEvent] = size(AE3);
start = zeros(1, nEvent);
stop = zeros(1, nEvent);
lowF = zeros(1, nEvent);
highF = zeros(1, nEvent);

for iEvent = 1:nEvent
    start(iEvent) = AE3(1, iEvent);
    stop(iEvent) = AE3(1, iEvent) +AE3(3, iEvent);
    lowF(iEvent) = AE3(2, iEvent);
    highF(iEvent) = AE3(2, iEvent) + AE3(4, iEvent);
end


AE4 = [];
if (~isempty(AE3))
    [rAE,~] = size(AE3');
    AE4 = zeros(rAE,4);
    AE4(:,1) = T([AE3(1,:)]);
    AE4(:,2) = T([AE3(3,:)]);
    AE4(:,3) = F([AE3(2,:)]);
    AE4(:,4) = F([AE3(2,:) + AE3(4,:) - 1]);
end

show_image(jet, spec, T, F, 1, AE4);


