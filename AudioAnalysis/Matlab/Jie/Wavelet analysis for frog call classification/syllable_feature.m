% wavelet packet decomposition for frog call classification
clc; clear; close all;

% parameter setting
winSize = 128;
winOver = 0.85;
  
fid = fopen('profile.csv');
profile = textscan(fid, '%s %s %s %s %s %s','delimiter',',');
fclose(fid);

% load segmentation result
segFolder = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-22\segmentation\*.csv';
segFolderName = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-22\segmentation\fileName';
segList = dir(segFolder);
segList = segList(arrayfun(@(x) ~strcmp(x.name(1), '.'), segList));

% load audio data
audioFolder = 'C:\work\dataset\audio data\EScience\testing data\*.wav';
audioFolderName = 'C:\work\dataset\audio data\EScience\testing data\fileName';
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
    maxGap = max(maxGap, 3);
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
     
    for iSeg = 1:segLength
        
        signal = y(startLoc(iSeg):stopLoc(iSeg));
        
        % add white noise
        % signal = awgn(signal, 10); 

        [spec, F, T] = wav_to_spec(signal, fs, winSize, winOver);
        
        [M, N] = size(spec);

        % remove the low energy of the frame
        [rValue, rIndex ]= max(spec);
         
        % rIndex(rValue < min(rValue) + 10) = 0;
        
        [trackMatrix,track] = SPT_detection(spec, rIndex, maxGap, maxGapFrame, minDuration,...
            maxDuration,binTolerance,minTrackDensity);
                       
                    % plot the peak on the spectrogram
%                         specT = max(T);
%                         imagesc((0:T), (0:fs/1000), max(spec(:)) - spec);
%                         axis('xy'); colormap(gray); % colorbar;
%                         hold on; 
%                         peakMark = rIndex * 16 / 128;
%                         plot(1:N, peakMark, 'Color', 'red','Marker','.', 'LineWidth',3);
%                         timeRes = max(T) / N;
%                         set(gca, 'XTickLabel', [0; round(timeRes * 200 * 100) / 100; round(timeRes * 400 * 100) / 100; round(timeRes * 600 * 100) / 100 ;...
%                             round(timeRes * 800 * 100) / 100 ; round(timeRes * 1000 * 100) / 100 ]);
%                         xlabel('Time (seconds)');
%                         ylabel('Frequency (kHz)');
%                         export_fig '.\05-06\plot\peak' -png       
%                              specT = max(T);
%                         imagesc((0:T), (0:fs/1000), max(spec(:)) - spec);
%                         axis('xy'); colormap(gray); % colorbar; 
%                         axis tight;
%                         hold on
%                         iTrack = 1;
%                         x = track{iTrack}.startframe : track{iTrack}.endframe;
%                         y = track{iTrack}.arrayp * 16 / 128;
%                         plot(x, y, 'color', 'b','Marker','.', 'LineWidth',3);
%                         timeRes = max(T) / N;
%                         set(gca, 'XTickLabel', [0; round(timeRes * 200 * 100) / 100; round(timeRes * 400 * 100) / 100; round(timeRes * 600 * 100) / 100 ;...
%                             round(timeRes * 800 * 100) / 100 ; round(timeRes * 1000 * 100) / 100 ]);
%                         xlabel('Time (seconds)');
%                         ylabel('Frequency (kHz)');
%                        export_fig '.\05-06\plot\track' -png
            % calculate syllable features based on the track
    
             
        % calculate syllable features based on the track
         
        if (~isempty(track))
            dominantFreq(iSeg) = mean(track{1}.arrayp) * herzBin;
            maxFreq(iSeg) = max(track{1}.array) * herzBin;
            minFreq(iSeg) = min(track{1}.array) * herzBin;
            rangeFreq(iSeg) = maxFreq(iSeg)  - minFreq(iSeg) ;
            syllableDur(iSeg) = length(track{1}.arrayp) / frameSecond * 1000;
            oscRate(iSeg) = osc_spectrogram(track, spec, frameSecond);
        else 
            dominantFreq(iSeg) = 0;
            maxFreq(iSeg) = 0;
            minFreq(iSeg) = 0;
            rangeFreq(iSeg) = 0;
            syllableDur(iSeg) = 0;
            oscRate(iSeg) = 0;
        end
        
    end
    
    label = zeros(segLength, 1) + index;
     
    % result
    frogFeature = [dominantFreq', maxFreq', minFreq', rangeFreq',  syllableDur', oscRate', label];
    
    % save the result
    names = {'dominantFreq', 'maxFrequency', 'minFrequency', 'rangeFrequency' 'syllableDuration', 'oscRate', 'label'};
    resultPath = '.\07-22\syllableFeature\fileName.csv';
    
    % frogFeature(sum((frogFeature==0),2)>0,:) = [];
        
    frog = audioName.name(1:(length(audioName.name) - 4));
    resultSavePath = strrep(resultPath, 'fileName', frog);
    
    csvwrite_with_headers(resultSavePath, frogFeature, names);
          
end


