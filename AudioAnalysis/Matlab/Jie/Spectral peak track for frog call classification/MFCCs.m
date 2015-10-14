clc; close all;

% parameter setting
winSize = 128;
winOver = 0.85;

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
     
    mfccResult = zeros(segLength, 12);
    dmfccResult = zeros(segLength, 24);
    ddmfccResult = zeros(segLength, 36);
    
    for iSeg = 1:segLength
        signal = y(startLoc(iSeg):stopLoc(iSeg));
        
        % add white noise
        % awgn(signal, 10); 
                
        [spec1, F1, T1] = wav_to_spec(signal, fs, winSize, winOver);
        ceps = JMFCC(spec1, fs, 1);
        MFCC = ceps(1:13, :);
        % calculate the mean of all frames, select the 12 coefficients and
        % do the normalization, Ref: automatic recognition of anuran
        % species based on syllable identification
        meanMFCC = mean(MFCC, 2);
        meanMFCC = meanMFCC(2:13);
        normMFCC = (meanMFCC - min(meanMFCC)) / range(meanMFCC) ;
                    
        dMFCC = ceps(14:26, :);  
        mdMFCC = mean(dMFCC, 2);
        mdMFCC = mdMFCC(2:13,:);
        % nmdMFCC = (mdMFCC - min(mdMFCC)) / range(mdMFCC);
                 
        ddMFCC = ceps(27:39, :); 
        mddMFCC = mean(ddMFCC, 2);
        mddMFCC = mddMFCC(2:13,:);
        % nmddMFCC = (mddMFCC - min(mddMFCC)) / range(mddMFCC);
        
        % save the result
        mfccResult(iSeg, :)= normMFCC';
        dmfccResult(iSeg, :) =  [normMFCC', mdMFCC'];
        ddmfccResult(iSeg, :)= [normMFCC', mdMFCC', mddMFCC'];
                
    end
    
    % add label
    label = zeros(1, segLength) + index;
    
    mfccResult = horzcat( mfccResult, label' );    
    dmfccResult = horzcat( dmfccResult, label' );
    ddmfccResult = horzcat( ddmfccResult, label' );
    
    % save the result to the FILE    
    mfccName = cell(13, 1);
    dmfccName = cell(25, 1);
    ddmfccName = cell(37, 1);
    
    for i = 1:12
        mfccName{i} = 'MFCC';
    end
    
    for i = 1:24
        dmfccName{i} = 'dMFCC';
    end
    
    for i = 1:36
        ddmfccName{i} = 'ddMFCC';
    end

    mfccName{13} = 'species';
    dmfccName{25} = 'species';
    ddmfccName{37} = 'species';
    
    path1 = '.\05-01\MFCC\fileNameMFCC.csv';
    path2 = '.\05-01\DMFCC\fileNameDMFCC.csv';
    path3 = '.\05-01\DDMFCC\fileNameDDMFCC.csv';
    
    saveName = segName.name(1:(length(segName.name) - 4));    
    savePath1 = strrep(path1, 'fileName', saveName);
    savePath2 = strrep(path2, 'fileName', saveName);
    savePath3 = strrep(path3, 'fileName', saveName);
    
    csvwrite_with_headers(savePath1, mfccResult, mfccName);
    csvwrite_with_headers(savePath2, dmfccResult, dmfccName);
    csvwrite_with_headers(savePath3, ddmfccResult, ddmfccName);
     
end





