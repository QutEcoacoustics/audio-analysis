% do the perceptual wavelet packet decompostion sub-band cepstral
% coefficients (PWSCC)
% adaptive wavelet packet decompostion
fs = 16000;
% get the meanFrequency for each frog species
NUM = 10;
% frogFrequency = zeros(1, NUM);
% for i  = 0 : NUM - 1
%     start = i * 5 + 1;
%     stop = start + 4;
%     frogFrequency(i+1) = mean(meanFrequency(start:stop));
% end  
 
frogFrequency = csvread('.\07-22\centroids.csv');
% frogFrequency = meanFrequency(1:10);
rankFrequency = round(sort(frogFrequency));
gapFrequency = diff(rankFrequency);

% [~, loc] = getNElements(gapFrequency, 5);
% closeFrequency = rankFrequency(loc);
% locB = loc + 1;
% 
% addFreq = zeros(1, 5);
% for k = 1:5
%     addFreq(k) = (rankFrequency(loc(k)) + rankFrequency(locB(k))) / 2;
% end
% 
% rankFrequency = [rankFrequency, addFreq];
% rankFrequency = sort(rankFrequency);

[N, X] = hist(gapFrequency, NUM * 2);
resolution = X(1);

% resolution = min(gapFrequency);

% find the levels for WPD
for j = 1:100
    if (fs / 2 / (2^j)) <= resolution
        level = j;
        break;
    end
end

% adaptively do the WPD
splitNode = 0;

for iLevel = 1:level+1    
    resFrequency = fs / 2 / 2^iLevel;
    
    for iStep = 0:2^iLevel-1
        num = 0;
        lowFreq = iStep * resFrequency;
        highFreq = lowFreq + resFrequency;
        
        for k = 1:length(rankFrequency)
            if rankFrequency(k) > lowFreq && rankFrequency(k) < highFreq
                num = num + 1;               
            end
        end
        
        if num >= 2
            splitNode = [splitNode, 2^iLevel + iStep];
        end
        
    end
end

splitNode(1) = [];
splitNode = splitNode - 1;

 
%%
% read signal
segFolder = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-22\segmentation\*.csv';
segFolderName = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-22\segmentation\fileName';
segList = dir(segFolder);
segList = segList(arrayfun(@(x) ~strcmp(x.name(1), '.'), segList));
 
audioFolder = 'C:\work\dataset\audio data\e-science\testing data\*.wav';
audioFolderName = 'C:\work\dataset\audio data\e-science\testing data\fileName';
audioList = dir(audioFolder);
audioList = audioList(arrayfun(@(x) ~strcmp(x.name(1), '.'), audioList));

for index = 1: length(segList)
% for index = 23:23
    disp(index)
    
    % read segmentation result
    segName = segList(index);
    segPath = strrep( segFolderName, 'fileName', segName.name );
    segResult = csvread(segPath);
    startLoc = segResult(:,1);
    stopLoc = segResult(:,2);
    segLength = length(startLoc);
    
    audioName = audioList(index);
    audioPath = strrep( audioFolderName, 'fileName', audioName.name );
    [y, fs] = audioread(audioPath);
    
    PWSCC = zeros(12, segLength);
    for iSeg = 1:segLength
        
        signal = y(startLoc(iSeg):stopLoc(iSeg));
        
        % signal = awgn(signal, 10);
               
        % add hamming window to the signal
        signal = signal .* hamming(length(signal));
        wpt = wpdec(signal, 1, 'db4');
        
        for iNode = 1:length(splitNode)
            wpt = wpsplt(wpt, splitNode(iNode));
        end
            
        % do the adaptive WPD and the next step is to extract PWSCC
        [tNodePal, tNodeSeq, I, J] = otnodes(wpt);
        wpdCoeff = cell(1, length(tNodePal));
              
        for iCof = 1:length(tNodePal)
            wpdCoeff{iCof} = wpcoef(wpt, tNodePal(iCof));
        end
                    
        % calculate the total energy of each sub-band
        subEnergy = ones(1, length(tNodePal));
        for iSub = 1:length(tNodePal)
            subEnergy(iSub) = sum(cell2mat(wpdCoeff(iSub)).^2);
        end
        
        % do normalization
        norEnergy = subEnergy / length(tNodePal);
        
        % perform DCT on the log energy
        dctEnergy = dct(log10(norEnergy), 13);
        PWSCC(:, iSeg) = abs(dctEnergy(2:13));
        % PWSCC(:, iSeg) = norEnergy;
        
    end
         
    label = zeros(1, segLength) + index;
    PWSCCresult = horzcat( PWSCC', label' );
    
    path = '.\07-22\PWSCC\fileNamePWSCC.csv';
    saveName = segName.name(1:(length(segName.name) - 4));
    savePath = strrep(path, 'fileName', saveName);
    
    pwsccName = cell(13, 1);
    for i = 1:12
        pwsccName{i} = 'PWSCC';
    end
     
    pwsccName{13} = 'species';
    
    csvwrite_with_headers(savePath, PWSCCresult, pwsccName);
    
    % plot(PWSCC);
end




