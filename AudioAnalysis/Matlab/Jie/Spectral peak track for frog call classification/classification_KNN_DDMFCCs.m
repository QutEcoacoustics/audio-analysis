% classification using KNN
clc; close all; clear;
% read csv files for loading features

pathDDMFCC = '.\05-01\DDMFCC';
featureFolderName = '.\05-01\DDMFCC\fileName';
DDMFCCList = dir(pathDDMFCC);
DDMFCCList = DDMFCCList(arrayfun(@(x) ~strcmp(x.name(1), '.'), DDMFCCList));
 
featureResult = zeros(1, 37);
for iSpecies = 1 : length(DDMFCCList)
     
    % read feature
    frogSpecies = DDMFCCList(iSpecies).name;
    frogPath = strrep(featureFolderName, 'fileName', frogSpecies);
     
    fid = fopen(frogPath);
    out = textscan(fid, '%s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s', 'delimiter', ',');
    fclose(fid);
    
    % put features of different frog species together
    len = length(out{1});
    frogFeature = zeros(len - 1, 37);
    
    for i = 1 : 37
        frogFeature(:, i) = str2double(out{i}(2: len));
    end    
    featureResult = vertcat(featureResult, frogFeature);
    
end
    
% remove the first row
featureResult(1,:) = [];
feature = featureResult(:, 1:36);
label = featureResult(:, 37);
 
NUM = 24;
speciesNumber = hist(label, NUM);
frogAcc = zeros(10, NUM);
  
% run the classifier 10 times
for iAcc = 1:10
    cv = cvpartition( label, 'KFold', 10);
    
    index = zeros(size(label));
    nFold = 10;
    
    for k = 1:nFold
        index(cv.test(k)) = k;
    end
     
    testIndex = (index == k);
    trainIndex = ~testIndex;
    
    trainingLabel = label(trainIndex, :);
    trainingSample = feature(trainIndex, :);
    testingLabel = label(testIndex, :);
    testingSample = feature(testIndex, :);
    
    outLabel = knnclassify(testingSample, trainingSample, trainingLabel, 3, 'Cityblock' );
     
    cm = confusionmat(outLabel, testingLabel);
    
    [rMat, cMat] = size(cm);
         
    for i = 1:rMat
        frogAcc(iAcc, i) = cm(i, i) / sum(cm(:,i));        
    end 
       
end

meanAcc = mean(frogAcc, 1);
stdAcc = std(frogAcc, 1);

speciesRatio = speciesNumber / sum(speciesNumber);
weightedAcc = sum(speciesRatio .* meanAcc);
disp(weightedAcc)
mean(stdAcc)
result = [meanAcc; stdAcc];
csvwrite('.\05-21\DDMFCC.csv', result)


