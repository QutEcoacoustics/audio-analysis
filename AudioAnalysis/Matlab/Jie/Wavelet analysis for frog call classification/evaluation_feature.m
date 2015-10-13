% adaptive wavelet packet decomposition
clc; close all;
% read the csv file of syllable features
% select FIVE instance from each frog species

featureFolder = '.\07-04\syllableFeature\*.csv';
featureFolderName = '.\07-04\syllableFeature\fileName';
featureList = dir(featureFolder);
% remove hidden file
featureList = featureList(arrayfun(@(x) ~strcmp(x.name(1), '.'), featureList));

% frogResult = zeros(1, 7);
meanFrequency = zeros(1, 10);
for index = 1: length(featureList)
    disp(index)
    frogFeature = zeros(5, 6);
    featureName = featureList(index);
    featurePath = strrep( featureFolderName, 'fileName', featureName.name );
      
    % open the csv file 
    fid = fopen(featurePath);
    feature = textscan(fid, '%s %s %s %s %s %s %s', 'delimiter',',');
    fclose(fid);
    len = length(feature{1});
    for i = 1:1
        frogFeature(1:5, i) = str2double(feature{i}(3:7));
        temp = str2double(feature{i}(2:len));
         [N, X] = hist(temp, 5);
         [~,loc] = max(N);
         meanFrequency(index) = X(loc);         
    end
    
%     frogResult = horzcat( frogResult, frogFeature);
%     frogResult = [ frogResult; frogFeature ];
    
end
 
% remove the first row
% frogResult(1, :) = [];
% 
% meanFrequency = frogResult(:,1);
% maxFrequency = frogResult(:,2);
% minFrequency = frogResult(:,3);
% rangeFrequency = frogResult(:,4);
% syllableDuration = frogResult(:,5);
% oscillationRate = frogResult(:,6);





