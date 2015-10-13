% extract AED feature including syllable duration, dominant frequency and
% frequency bandwidth
clc; clear; close all;
% audioPath = 'C:\work\dataset\audio data\Applied acoustics\1075\Kiyomi_dam_-_Herveys_Range_1075_251109_20140224_000000_10.0__.wav';
% audioPath = 'C:\work\dataset\audio data\Applied acoustics\1078\Stony_creek_dam_-_Herveys_Range_1078_251210_20140211_201000_10.0__.wav';
% audioPath = '.\recordings for evaluating AED method\Stony_creek_dam_-_Herveys_Range_1078_251211_20140212_225000_10.0__.wav';
% audioPath = 'C:\Users\Jie\Documents\MATLAB\Tasks\MIML classification\recordings for evaluating AED method\PC5_20090705_050000_0010.wav';

% audioPath = 'C:\work\dataset\audio data\MIML\1078\Stony_creek_dam_-_Herveys_Range_1078_251211_20140212_222000_10.0__.wav';
% audioPath = 'C:\work\dataset\audio data\MIML\1078\Stony_creek_dam_-_Herveys_Range_1078_251248_20140323_010000_10.0__.wav';
% audioPath = 'C:\work\dataset\audio data\MIML\1078\Stony_creek_dam_-_Herveys_Range_1078_251210_20140211_204000_10.0__.wav';
audioPath = 'C:\work\dataset\audio data\MIML\1075\Kiyomi_dam_-_Herveys_Range_1075_251109_20140224_000000_10.0__.wav';
% audioPath = 'C:\work\dataset\audio data\MIML\1075\Kiyomi_dam_-_Herveys_Range_1075_251110_20140224_201000_10.0__.wav';

audioFolder = 'C:\work\dataset\audio data\ICASSP-MIML\1075\*.wav';
audioFolderName = 'C:\work\dataset\audio data\ICASSP-MIML\1075\fileName';
audioList = dir(audioFolder);
audioList = audioList(arrayfun(@(x) ~strcmp(x.name(1), '.'), audioList));
    
winSize = 512; winOver = 0.5;

for indexAudio = 1: length(audioList)
    disp(indexAudio);
    audioName = audioList(indexAudio);
    audioPath = strrep( audioFolderName, 'fileName', audioName.name );
       
    
    [y, fs] = audioread(audioPath);    
    [spec1, F, T] = wav_to_spec(y, fs, winSize, winOver);
    [M, N] = size(spec1);
    frameSecond = N / max(T);
    herzBin = fs / 2 / M;
       
    % wiener filter
    spec2 = wiener2(spec1, [5 5]);
    
    % gaussian filtering
    G = fspecial('gaussian', [15 15], 0.9);
    spec2 = imfilter(spec2, G, 'same');
    
    % noise reduction
    spec3 = noise_reduce(spec2, frameSecond);
        
    % adaptive thresholding (otsu thresholding)
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
    
    AE1 = AE;
    
    [mAE, nAE] = size(AE1);
    area = zeros(1, nAE);
    for i = 1:nAE
        area(i) = AE1(3, i) * AE(4, i);
    end
    
    % segment large events into small events
    [newAE] = separate_large_AEs_areas(AE1, L, 1000, spec1);
    % newAE = AE1;
    
    % remove small events
    
    AE2 = newAE;
    % Use spectral peak track to remove those events that are not frogs with
    % defined parameters.
    % find the averaged peak frequency for each acoustic event
    [~, numAE] = size(AE2);
    
    parameter = zeros(2, numAE);
    
    highF = 5000; lowF = 300;
    
    filterAE = [];
    tempAE = [];
    for iAE = 1:numAE
        % disp(iAE);
        start = AE2(1, iAE);
        stop = AE2(1, iAE) + AE2(3, iAE)-1;
        lowBin = AE2(2,iAE);
        highBin = AE2(2, iAE) + AE2(4, iAE)-1;
        
        partSpec = spec1( lowBin:highBin, start:stop );
        
        [~, loc] = max(partSpec);
        
        meanBin = mean(loc);
        
        meanFreq = round((meanBin + lowBin) * herzBin);
        % disp(meanFreq);
        % duration = AE3(3, iAE) / frameSecond * 1000;
        
        if( meanFreq < 300 || meanFreq > 5000)
            AE2(:,iAE) = 0;
        end
        
        if( meanFreq > 300 && meanFreq < 800)
            tempAE(:,iAE) = AE2(:,iAE);
        end
        
        % parameter(1, iAE) = meanFreq;
        % parameter(2, iAE) = AE(3, iAE) / frameSecond * 1000;
        % imagesc(partSpec); axis xy;
        
    end
    
    smallAreaThresh = 300;
    AE3 = mode_small_area_threshold(AE2, smallAreaThresh);
    
    AE4 = [AE3, tempAE];
    
    [~, index] = find(sum(AE4, 1) == 0);
    
    AE4(:, index) = [];
    
    % remove particular small events
    smallAreaThresh = 10;
    AE4 = mode_small_area_threshold(AE4, smallAreaThresh);
        
    % use the combinantion of dominant frequency and syllable duration for
    % acosutic events filtering
    freqFilter = csvread('.\profile\freqFilter.csv');
    durationFilter = csvread('.\profile\durationFilter.csv'); % samples
    
    freqFilter = round(freqFilter);
    % change to mille seconds
    durationFilter = durationFilter / fs * 1000;
    
    %---------------------------------------------------------------------------------%
    %--- 'CTD', 'LNA', 'LFX', 'LRI', 'LRA', 'CNE', 'LTE', 'UMA'
    %---------------------------------------------------------------------------------%
    % since the duration of canetoad has a large variation, therefore, we do
    % not use this filter technique for canetoad
    
    CTD_Filter = [freqFilter(1,1), durationFilter(1,1)];
    LNA_Filter = [freqFilter(2,1), durationFilter(2,1)];
    LFX_Filter = [freqFilter(3,1), durationFilter(3,1)];
    LRI_Filter = [freqFilter(4,1), durationFilter(4,1)];
    LRA_Filter = [freqFilter(5,1), durationFilter(5,1)];
    CNE_Filter = [freqFilter(6,1), durationFilter(6,1)];
    LTE_Filter = [freqFilter(7,1), durationFilter(7,1)];
    UMA_Filter = [freqFilter(8,1), durationFilter(8,1)];
    
    [~, numAE2] = size(AE4);
    
    AE5 = [];
    index = 1;
    for iAE = 1:numAE2
        % disp(iAE);
        start = AE4(1, iAE);
        stop = AE4(1, iAE) + AE4(3, iAE)-1;
        lowBin = AE4(2,iAE);
        highBin = AE4(2, iAE) + AE4(4, iAE)-1;
        
        partSpec = spec1( lowBin:highBin, start:stop );
        
        [~, loc] = max(partSpec);
        
        meanBin = mean(loc);
        
        meanFreq = round((meanBin + lowBin) * herzBin);
        % disp(meanFreq);
        % change frames to mille seconds
        duration = AE4(3, iAE) / frameSecond * 1000;
        
        door = 0;
        if (door == 0 && meanFreq < LNA_Filter(1) +300 && meanFreq > LNA_Filter(1) -300)
            AE5(index,:) = AE4(:, iAE);
            door = 1;
        end
        
        if (door == 0 && meanFreq < LFX_Filter(1) +300 && meanFreq > LFX_Filter(1) -300)
            AE5(index,:) = AE4(:, iAE);
            door = 1;
        end
        
        if (door == 0 && meanFreq < LRI_Filter(1) +500 && meanFreq > LRI_Filter(1) -500)
            AE5(index,:) = AE3(:, iAE);
            door = 1;
        end
        
        if (door == 0 && meanFreq < LRA_Filter(1) +300 && meanFreq > LRA_Filter(1) -300)
            AE5(index,:) = AE4(:, iAE);
            door = 1;
        end
        
        if (door == 0 && meanFreq < CNE_Filter(1) +300 && meanFreq > CNE_Filter(1) -300)
            AE5(index,:) = AE4(:, iAE);
            door = 1;
        end
        
        if (door == 0 && meanFreq < LTE_Filter(1) +300 && meanFreq > LTE_Filter(1) -300)
            AE5(index,:) = AE4(:, iAE);
            door = 1;
        end
        
        if (door == 0 && meanFreq < UMA_Filter(1) +300 && meanFreq > UMA_Filter(1) -300)
            AE5(index,:) = AE4(:, iAE);
            door = 1;
        end
        
        index = index + 1;
        
    end
    
    AE6 = AE5';
    % AE6 = AE4;
    [~, index] = find(sum(AE6, 1) == 0);
    
    AE6(:, index) = [];
    % remove some events that averaged dominant frequency and syllable duration
    % is quite different from the template (except canetoad)
    
    % do region growing
    [~, numAE6] = size(AE6);
    result = zeros(M, N);
    
    % feature list
    minFreq = zeros(1, numAE6);
    maxFreq = zeros(1, numAE6);
    bandwidth = zeros(1, numAE6);
    durationFeature = zeros(1, numAE6);
    areaFeature = zeros(1, numAE6);
    perimeter = zeros(1, numAE6);
    nonCompact = zeros(1, numAE6);
    rectangularity = zeros(1, numAE6);
    
    freqGini = zeros(1, numAE6);
    timeGini = zeros(1, numAE6);
    
    % frequency statistics
    freqMean = zeros(1, numAE6);
    freqVar = zeros(1, numAE6);
    freqSkew = zeros(1, numAE6);
    freqKurto = zeros(1, numAE6);
    
    % time statistics
    timeMean = zeros(1, numAE6);
    timeVar = zeros(1, numAE6);
    timeSkew = zeros(1, numAE6);
    timeKurto = zeros(1, numAE6);
    
    freqMax = zeros(1, numAE6);
    timeMax = zeros(1, numAE6);
    
    maskMean = zeros(1, numAE6);
    maskStd = zeros(1, numAE6);
    
   for jAE = 1:numAE6
        
        start = AE6(1, jAE);
        stop = AE6(1, jAE) + AE6(3, jAE)-1;
        lowBin = AE6(2,jAE);
        highBin = AE6(2, jAE) + AE6(4, jAE)-1;
        
        tempSpec = spec3( lowBin:highBin, start:stop );
        
        % step1: find the maximum value
        [xLoc, yLoc] = find(max(tempSpec(:)) == tempSpec);
              
        % step2: do region growing
        label = regiongrowing_jie(spec3(lowBin:highBin, start:stop), xLoc(1), yLoc(1), 5);
        
        result(lowBin:highBin, start:stop) = label;
        
        % extract features based on the shape
        rowSum = sum(label, 2);
        lenRow = length(rowSum);
        
        minBin = 0;
        if rowSum(1) > 0
            minBin = 1;
        else
            for i = 1:lenRow-1
                if rowSum(i) == 0 && rowSum(i+1) > 0
                    minBin = i+1;
                    break;
                end
            end
        end
        maxBin = 0;
        if rowSum(lenRow) ~=0;
            maxBin = lenRow;
        else
            for i = 1:lenRow-1
                if rowSum(i) > 0 && rowSum(i+1) == 0
                    maxBin = i;
                    break;
                end
            end
        end
        
        % min frequency (frequency bins)
        minFreq(jAE) = minBin + lowBin;
        % max frequency
        maxFreq(jAE) = maxBin + lowBin;
        % bandwidth
        bandwidth(jAE) = maxBin - minBin + 1;
        
        % duration (frames)
        colSum = sum(label, 1);
        lenCol = length(colSum);
        
        minFrame = 0;
        if colSum(1) > 0
            minFrame = 1;
        else
            for i = 1:lenCol-1
                if colSum(i) == 0 && colSum(i+1) > 0
                    minFrame = i+1;
                    break;
                end
            end
        end
        maxFrame = 0;
        if colSum(lenCol) ~=0;
            maxFrame = lenCol;
        else
            for i = 1:lenRow-1
                if colSum(i) > 0 && colSum(i+1) == 0
                    maxFrame = i;
                    break;
                end
            end
        end
        
        durationFeature(jAE) = maxFrame - minFrame;
        
        % area (pixels)
        areaFeature(jAE) = sum(label(:));
        
        % perimeter
        tempPeri= regionprops(label, 'Perimeter');
        perimeter(jAE) = tempPeri.Perimeter;
        % non-compactness
        nonCompact(jAE) = perimeter(jAE).^2 / areaFeature(jAE);
        
        % rectangularity
        rectangularity(jAE) = areaFeature(jAE) / (durationFeature(jAE) * bandwidth(jAE));
        
        %--------------------------------------------------------------------------------------------------------------------------------%
        % specFinal = result .* spec1;
        % extract features based on the content
        conSpec  = label.* spec3( lowBin:highBin, start:stop );
        proFreq = sum(conSpec, 2);
        proFreq = proFreq / sum(proFreq);
        
        proTime = sum(conSpec, 1);
        proTime = proTime / sum(proTime);
        
        freqGini(jAE) = 1 - sum(proFreq.^2);
        timeGini(jAE) = 1 - sum(proTime.^2);
        
        % frequency statistics
        freqMean(jAE) = mean(proFreq);
        freqVar(jAE) = var(proFreq);
        freqSkew(jAE) = skewness(proFreq);
        freqKurto(jAE) = kurtosis(proFreq);
        
        % time statistics
        timeMean(jAE) = mean(proTime);
        timeVar(jAE) = var(proTime);
        timeSkew(jAE) = skewness(proTime);
        timeKurto(jAE) = kurtosis(proTime);
        
        freqMax(jAE) = max(proFreq);
        timeMax(jAE) = max(proTime);
        
        maskMean(jAE) = mean(conSpec(conSpec~=0));
        maskStd(jAE) = std(conSpec(conSpec~=0));
        
    end
    
    shapeBlob = vertcat(minFreq, maxFreq, bandwidth, durationFeature, areaFeature, perimeter, nonCompact, rectangularity)';
        
    shapeBlob(isinf(shapeBlob)) = 0; shapeBlob(isnan(shapeBlob)) = 0;
    
    resultPathShape = '.\09-10\1075\shape\fileNameShape.csv';
    frog = audioName.name(1:(length(audioName.name) - 4));
    resultSavePathShape = strrep(resultPathShape, 'fileName', frog);   
%     csvwrite(resultSavePathShape, shapeBlob);
    
    contentBlob = vertcat(freqGini, timeGini, freqMean, freqVar, freqSkew, freqKurto, timeMean, timeVar, ...
        timeSkew, timeKurto, freqMax, timeMax, maskMean, maskStd)';
    
    
     contentBlob(isinf(contentBlob)) = 0; contentBlob(isnan(contentBlob)) = 0;

    resultPathContent = '.\09-10\1075\content\fileNameShape.csv';
    frog = audioName.name(1:(length(audioName.name) - 4));
    resultSavePathContent = strrep(resultPathContent, 'fileName', frog);   
%     csvwrite(resultSavePathContent, contentBlob);
    
    
    allBlob = horzcat(shapeBlob, contentBlob);    
    resultPathAll = '.\09-10\1075\all\fileNameAll.csv';
    frog = audioName.name(1:(length(audioName.name) - 4));
    resultSavePathAll = strrep(resultPathAll, 'fileName', frog);
    csvwrite(resultSavePathAll, allBlob);

    
end


% write feature to csv file
% one bag represent one recording

AE3 = AE6;
%
AE4 = [];
if (~isempty(AE3))
    [rAE,~] = size(AE3');
    AE4 = zeros(rAE,4);
    AE4(:,1) = T([AE3(1,:)]);
    AE4(:,2) = T([AE3(3,:)]);
    AE4(:,3) = F([AE3(2,:)]);
    AE4(:,4) = F([AE3(2,:) + AE3(4,:) - 1]);
end

show_image(jet, spec1, T, F, 1, AE4);

% spec6 = spec5 .* spec1;
% imagesc(spec6); axis xy;

% do a region growing based on the track and calculate the dominant
% frequency and high/low frequency (frequency bandwidth).

export_fig '.\09-23\AEfinal.pdf'

% imagesc(T,F,specFinal);
% axis xy; axis tight; colormap(jet); view(0,90);
% % title('Spectrogram','FontSize',15)
% ylabel('Frequency (Hz)','FontSize',15)
% xlabel('Time (s)','FontSize',15)

% export_fig '.\09-13\acousticEvents.png'
