% extract features based on the advertisement call
% classification
clc; close all;

% parameter setting
winSize = 128;
winOver = 0.85;
    
fid = fopen('profile.csv');
profile = textscan(fid, '%s %s %s %s %s %s','delimiter',',');
fclose(fid);

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
    disp(index)
    % read segmentation result
    segName = segList(index);
    segPath = strrep( segFolderName, 'fileName', segName.name );
    segResult = csvread(segPath);
    startLoc = segResult(:,1);
    stopLoc = segResult(:,2);
     
    % read audio file
    audioName = audioList(index);
    audioPath = strrep( audioFolderName, 'fileName', audioName.name );
    [y, fs] = audioread(audioPath);
    
    [spec1, F1, T1] = wav_to_spec(y, fs, winSize, winOver);
    [M, N] = size(spec1);
    
    frameSecond = N / max(T1);
    herzBin = fs / 2 / M;
    
    % show_image(jet, spec1, T1, F1, 1);
    
    %----------------------------------------------------------------------------------------%
    
    maxGap = round(str2double(profile{1}(2)) * frameSecond);
    maxGap = max(maxGap, 5);
    maxGapFrame = round(str2double(profile{2}(2)) * frameSecond);
    minDuration = round(str2double(profile{3}(2)) * frameSecond);
    maxDuration = round(str2double(profile{4}(2)) * frameSecond);
    binTolerance = round(str2double(profile{5}(2)) / herzBin);
    minTrackDensity = str2double(profile{6}(2));
    
    segLength = length(startLoc);
    %----------------------------------------------------------------------------------------%
    
    dominantFreq = zeros(1, segLength);
    maxFreq = zeros(1, segLength);
    minFreq = zeros(1, segLength);
    rangeFreq = zeros(1, segLength);
    syllableDur = zeros(1, segLength);
    oscRate = zeros(1, segLength);
    shanEntropy = zeros(1, segLength);
    frequencyModulation = cell(1, segLength);
    energyModulation = cell(1, segLength);
    
    for iSeg = 1: segLength
        % disp(iSeg)
        signal = y(startLoc(iSeg):stopLoc(iSeg));
        
        % signal = awgn(signal, 10); 

        % filterSignal = filter([1 -.95], 1, signal) .* hamming(length(signal));
         
        [spec2, F, T] = wav_to_spec(signal, fs, winSize, winOver);
         
        % show_image(gray, max(spec2(:)) - spec2,T,F,1)
          
        % figure;
        % axes('position', [0 0 1 1]);
        % reveSpec = fliplr(spec2(:,52)');
        % plot(reveSpec);
        % set(gca,'xlim',[0,128])
        % set(gca, 'xtickLabel',[]);
        % set(gca, 'ytickLabel',[]);
         
        [M2, N2] = size(spec2);
         
        spec3 = withoutSubbandModeIntensities(spec2);
        
        %------------------------------------------------------------------------------------------%
        % smooth the spectrogram along the horizontal direction
        spec = zeros(M2, N2);
        for r = 1:M2
            spec(r, :) = smooth(spec2(r,:) , 7);
        end
        
        % remove the low energy of the frame
        freqLow = round(500 / (fs / 2) * winSize);
        freqHigh = round(6000 / (fs / 2) * winSize);
        
        [rValue, rIndex ]= max(spec(freqLow:freqHigh, :));
        rIndex = rIndex + freqLow;
        
        rIndex(rValue < min(rValue) + 10) = 0;
        
        if (sum(rIndex) >0)
            
            [trackMatrix, track] = SPT_detection(spec, rIndex, maxGap, maxGapFrame, minDuration,...
                maxDuration,binTolerance,minTrackDensity);
            
            % plot the peak on the spectrogram
%                         specT = max(T);
%                         imagesc((0:T), (0:fs/2/1000), max(spec2(:)) - spec2);
%                         axis('xy'); colormap(gray); % colorbar;
%                         hold on;
%                         peakMark = rIndex * 22.05 / 128;
%                         plot(1:N2, peakMark, 'Color', 'red','Marker','.', 'LineWidth',3);
%                         timeRes = max(T) / N2;
%                         set(gca, 'XTickLabel', [0; round(timeRes * 200 * 100) / 100; round(timeRes * 400 * 100) / 100; round(timeRes * 600 * 100) / 100 ;...
%                             round(timeRes * 800 * 100) / 100 ; round(timeRes * 1000 * 100) / 100 ]);
%                         xlabel('Time (seconds)');
%                         ylabel('Frequency (kHz)');
%                         export_fig '.\05-12\plot\peak' -png
            % --------------------------------------------%
            if length(track) > 1
                trackIndex = 1;
                maxLen = length(track{1}.array);
                for iTrack = 2:length(track)
                    if length(track{iTrack}.array) > maxLen
                        trackIndex = iTrack;
                    end
                end
                track{1} = track{trackIndex};
            end
            
            % --------------------------------------------%
%                         specT = max(T);
%                         imagesc((0:T), (0:fs/2/1000), max(spec2(:)) - spec2);
%                         axis('xy'); colormap(gray); % colorbar; 
%                         axis tight;
%                         hold on
%                         iTrack = 1;
%                         x = track{iTrack}.startframe : track{iTrack}.endframe;
%                         y = track{iTrack}.arrayp * 22.05 / 128;
%                         plot(x, y, 'color', 'b','Marker','.', 'LineWidth',3);
%                         timeRes = max(T) / N2;
%                         set(gca, 'XTickLabel', [0; round(timeRes * 200 * 100) / 100; round(timeRes * 400 * 100) / 100; round(timeRes * 600 * 100) / 100 ;...
%                             round(timeRes * 800 * 100) / 100 ; round(timeRes * 1000 * 100) / 100 ]);
%                         xlabel('Time (seconds)');
%                         ylabel('Frequency (kHz)');
%                        export_fig '.\05-06\plot\track' -png
            % calculate syllable features based on the track
              
            if (~isempty(track))
                
                df = track{1}.array;
                df(df == 0) = [];
                dominantFreq(iSeg) = mean(df) * herzBin;
                
                maxFreq(iSeg) = max(track{1}.arrayp) * herzBin;
                minFreq(iSeg) = min(track{1}.arrayp) * herzBin;
                rangeFreq(iSeg) = maxFreq(iSeg)  - minFreq(iSeg) ;
                
                % frequency modulation
                Nblock = 8;
                
                stepFreq = floor(length(track{1}.array) / Nblock);
                equalFreq = zeros(8, 1);
                
                for nBlock = 1:Nblock
                    startTrk = (nBlock - 1) * stepFreq + 1;
                    stopTrk = nBlock * stepFreq;
                    
                    tempFreq = track{1}.array(startTrk : stopTrk);
                    equalFreq(nBlock) = mean(tempFreq) * herzBin;
                end
                tempFM = equalFreq -  dominantFreq(iSeg);
                frequencyModulation{iSeg} = (tempFM - min(tempFM)) / range(tempFM);
                
                % syllable duration
                syllableDur(iSeg) = length(signal) / fs;
                
                spec3(spec3 < 0) = 0;
                oscRate(iSeg) = osc_spectrogram(track, spec3, frameSecond);
                
                % shannon entropy
                % signalMir = miraudio(audioPath, 'Extract', startLoc(iSeg), stopLoc(iSeg), 'sp' );              
                % shannonE = mirentropy(signalMir);
                % shanEntropy(iSeg) = mirgetdata(shannonE);
                
                % shannon entropy
                shanEntropy(iSeg) = abs(entropy(signal));
                   
                % energy modulation
                stepEng =  floor(length(signal) / Nblock);
                equalEng = zeros(8,1);
                
                for nBlock = 1:Nblock
                    startTrk = (nBlock - 1) * stepEng + 1;
                    stopTrk = nBlock * stepEng;
                    
                    tempEng = sum(signal(startTrk : stopTrk).^2);
                    equalEng(nBlock) = tempEng;
                end
                
                tempEM = equalEng - sum(equalEng) / 8;
                energyModulation{iSeg} = (tempEM - min(tempEM)) / range(tempEM);
                                
                % ------------------------------------------------------------%
                
            else
                dominantFreq(iSeg) = 0;
                maxFreq(iSeg) = 0;
                minFreq(iSeg) = 0;
                rangeFreq(iSeg) = 0;
                frequencyModulation{iSeg} = zeros(8, 1);
                syllableDur(iSeg) = 0;
                oscRate(iSeg) = 0;
                shanEntropy(iSeg) = 0;
                energyModulation{iSeg} = zeros(8, 1);
            end
        else
            dominantFreq(iSeg) = 0;
            maxFreq(iSeg) = 0;
            minFreq(iSeg) = 0;
            rangeFreq(iSeg) = 0;
            frequencyModulation{iSeg} = zeros(8, 1);
            syllableDur(iSeg) = 0;
            oscRate(iSeg) = 0;
            shanEntropy(iSeg) = 0;
            energyModulation{iSeg} = zeros(8, 1);
        end
        
    end
    
    label = zeros(segLength, 1) + index;
    
    freqModuResult = zeros(segLength, nBlock);
    engModuResult = zeros(segLength, nBlock);
    
    for k = 1:segLength
        freqModuResult(k,:) = frequencyModulation{k};
        engModuResult(k,:) = energyModulation{k};
    end
    
    
    % result
    frogFeature = [dominantFreq', maxFreq', minFreq', rangeFreq', syllableDur', oscRate', shanEntropy'];
    
    frogFeature = horzcat(frogFeature, freqModuResult, engModuResult, label);
    
    % save the result
    names = cell(24, 1);
    names(1:7) = {'dominantFreq', 'maxFrequency', 'minFrequency', 'rangeFrequency' 'syllableDuration', 'oscRate', 'shanEntropy'};
     
    for i = 8:15
        names{i} = 'FrequencyModulation';
    end
    
    for i = 16:23
        names{i} = 'energyModulation';
    end
    
    names{24} = 'label';
    
    resultPath = '.\05-21\syllableFeature\fileName.csv';
    
    frogFeature(sum((frogFeature==0),2)>0,:) = [];
    
    frog = audioName.name(1:(length(audioName.name) - 4));
    resultSavePath = strrep(resultPath, 'fileName', frog);
    
    csvwrite_with_headers(resultSavePath, frogFeature, names);
    
end


