% do the wavelet packet decompostion sub-band cepstral
% coefficients based on Mel-scale
fs = 16000;

%----------------------------%
freRes = round((fs / 2 - 100) / 23);
frq = 100: freRes: fs / 2;
[mel, mr] = frq2mel(frq);
frogFrequency = mel;
%--------------------------------%


splitNode = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 15, 16, 17, 18, 19, 20, 31, 32, 33, 34] ;
  
  
%%
% read signal
segFolder = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-04\segmentation\*.csv';
segFolderName = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-04\segmentation\fileName';
segList = dir(segFolder);
segList = segList(arrayfun(@(x) ~strcmp(x.name(1), '.'), segList));

audioFolder = 'C:\work\dataset\audio data\EScience\*.wav';
audioFolderName = 'C:\work\dataset\audio data\EScience\fileName';
audioList = dir(audioFolder);
audioList = audioList(arrayfun(@(x) ~strcmp(x.name(1), '.'), audioList));

for index = 1: length(segList)
% for index = 1:1
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

    MelCC = zeros(12, segLength);
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
        MelCC(:, iSeg) = abs(dctEnergy(2:13)); 
        
    end     
    
    label = zeros(1, segLength) + index;
    MelCCresult = horzcat( MelCC', label' );    
    
    path = '.\07-04\original\MelCC\fileNameMelCC.csv';
    saveName = segName.name(1:(length(segName.name) - 4));    
    savePath = strrep(path, 'fileName', saveName);
    
    melccName = cell(13, 1);
    for i = 1:12
        melccName{i} = 'MelCC';
    end
    
    melccName{13} = 'species';

    csvwrite_with_headers(savePath, MelCCresult, melccName);
  
    % plot(PWSCC);
end
    

 

