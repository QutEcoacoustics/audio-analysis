% apply Harma's method to do the segmentation
clc; close all;

audioFolder = 'C:\work\dataset\audio data\EScience\testing data\*.wav';
audioFolderName = 'C:\work\dataset\audio data\EScience\testing data\fileName';
  
audioList = dir(audioFolder);
% remove hidden files
audioList = audioList(arrayfun(@(x) ~strcmp(x.name(1), '.'), audioList));

smoothIndex = [8,9];

% for iAudio = 1:length(audioList) 
for index = 1:length(smoothIndex) 
    iAudio = smoothIndex(index);
     
    frogName = audioList(iAudio); 
    disp(frogName); 
    frogAudioPath = strrep(audioFolderName, 'fileName', frogName.name);
           
    [y, fs] = audioread(frogAudioPath);
     
    % y = y(21376:44396); % L_tasmaniensis 
    % y = y(171709:205438); % N_sudelli
    % y = y(492654:520145); % P.sphagnicolus     
        
    % signal = filter([1 -.95], 1, y);
                       
    [syllable,FS,S,F,T,P] = harmaSyllableSeg(y, fs, kaiser(512), 128, 1024, 20);
                             
    % export_fig '.\05-19\plot\L_tasmaniensis_yes' -png
                    
    % save the syllable to the CSV file    
    numberSyllable = length(syllable);
      
    syllableLength = zeros(1,numberSyllable);
      
    for i = 1:numberSyllable
        syllableLength(i) = length(syllable(i).signal);
    end
    
    for k = 1:length(syllable)
        if syllableLength(k) < 50
            syllableLength(k) = 0;
        end
    end 
       
    syllable(syllableLength == 0) = [];
    
    % remove syllables that RMS shorter than 15% of the largest RMS
    rmsResult = zeros(1, length(syllable));
    for j = 1:length(syllable)
        rmsResult(j) = rms(syllable(j).signal);
    end
    
    syllable(rmsResult <= max(rmsResult) * 0.15) = [];
                  
    start = zeros(1,numberSyllable);
    stop = zeros(1,numberSyllable);
    
    for iSyllable = 1:length(syllable)
         temp = sort(syllable(iSyllable).times);                                       
         start(iSyllable) = round(temp(1) * FS);
         stop(iSyllable) = round(temp(end) * FS);          
    end 
       
    start(start == 0) = [];
    stop(stop == 0) = [];
    
    result = [sort(start);sort(stop)]';
                   
    path = '.\07-22\segmentation\fileName.csv';
    frog = frogName.name;
    frog = frog(1:(length(frog) - 4));
    csvPath = strrep(path, 'fileName', frog);    
    csvwrite(csvPath,result);
      
end 
