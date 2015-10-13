% extract feature for comparison

clc; close all;

% parameter setting
winSize = 128;
winOver = 0.85;

% winSize = 512;
% winOver = 0.5;

fs = 44100;

% load segmentation result
segFolder = '.\05-01\segmentation\*.csv';
segFolderName = '.\05-01\segmentation\fileName';
segList = dir(segFolder);
segList = segList(arrayfun(@(x) ~strcmp(x.name(1), '.'), segList));

% load audio data
audioFolder = 'C:\work\dataset\audio data\Bioacoustic\*.wav';
audioFolderName = 'C:\work\dataset\audio data\Bioacoustic\fileName';
audioList = dir(audioFolder);
audioList = audioList(arrayfun(@(x) ~strcmp(x.name(1), '.'), audioList));

for index = 1: length(segList)
    
    segName = segList(index);
    segPath = strrep( segFolderName, 'fileName', segName.name );
    segResult = csvread(segPath);
    startLoc = segResult(:,1);
    stopLoc = segResult(:,2);
    
    % read audio file
    audioName = audioList(index);
    audioPath = strrep( audioFolderName, 'fileName', audioName.name );
        
    [y, fs] = audioread(audioPath);

    segLength = length(startLoc);
    
    shanEntropy = zeros(1, segLength);
    renyEntropy = zeros(1, segLength); 
    spCentroid = zeros(1, segLength);
    sigBandwidth = zeros(1, segLength);
    spFlatness = zeros(1, segLength);
    zcRate = zeros(1, segLength);
    spRolloff = zeros(1, segLength);
    spFlux = zeros(1, segLength);
    avEnergy =  zeros(1, segLength);
    
     for iSeg = 1:segLength
         signal = y(startLoc(iSeg):stopLoc(iSeg));
         
         % add white noise
         signal = awgn(signal, 40);
         
         % shannon entropy
         shanEntropy(iSeg) = abs(entropy(signal));
          
         % reny entropy
         rEntropy = renyi_entropy_lps(signal');
         renyEntropy(iSeg) = rEntropy;
         
         % spectral centroid
        sCentroid = SpectralCentroid(signal, winSize, winSize * winOver, fs);
        spCentroid(iSeg) = mean(sCentroid);
         
         % signal bandwidth
               
         aFFT = abs(fft(signal));
         squareSum1 = aFFT.^2';         
         denominator = sum(squareSum1(1:round(length(aFFT) / 2)));  
         numerator = ((1:round(length(aFFT) / 2)) -  spCentroid(iSeg)) .^2 * aFFT(1:round(length(aFFT) / 2));
         sigBand = sqrt(numerator / denominator);
         sigBandwidth(iSeg) = sigBand;
             
         % spectral flatness
         spectrum = abs(fft(signal));
         sFlatness = geomean(spectrum.^2) / mean(spectrum.^2);
         spFlatness(iSeg) = sFlatness;
         
         % zero crossing rate
         zcrValue = zcr(signal, winSize, round(winSize * winOver));
         zcRate(iSeg) = mean(zcrValue);
         
         % spectral roll off
         sRolloff = SpectralRollOff(signal, winSize, winSize * winOver, 0.8, fs);        
         spRolloff(iSeg) = mean(sRolloff);
          
         % spectral flux
         sFlux = mean(SpectralFlux(signal, winSize, winSize * winOver, fs));         
         spFlux(iSeg) = sFlux;
         
         % average energy
         energy = sum(signal.^2) / length(signal);
         avEnergy(iSeg) = energy;
           
     end
      
     label = zeros(segLength, 1) + index;
     compareFeature = [shanEntropy', renyEntropy', spCentroid', sigBandwidth', spFlatness', zcRate', spRolloff', spFlux', avEnergy', label];
     names = cell(10, 1);
     names{1} = 'shanEntropy';
     names{2} = 'renyEntropy';
     names{3} = 'spCentroid';
     names{4} = 'sigBandwidth';
     names{5} = 'spFlatness';
     names{6} = 'zcRate';
     names{7} = 'spRolloff';
     names{8} = 'spFlux';
     names{9} = 'avEnergy';
     names{10} = 'label';
     
     path = '.\05-01\40\compare\fileName.csv';
     
     saveName = segName.name(1:(length(segName.name) - 4));
     savePath = strrep(path, 'fileName', saveName);
     
     csvwrite_with_headers(savePath, compareFeature, names);
      
end




    