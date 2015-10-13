% classification using KNN
clc; close all; clear;
% read csv files for loading features

pathPWSCC = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-09\syllableFeature';
featureFolderName = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-09\syllableFeature\fileName';
PWSCCList = dir(pathPWSCC);
PWSCCList = PWSCCList(arrayfun(@(x) ~strcmp(x.name(1), '.'), PWSCCList));

dim = 7;

featureResult = zeros(1, dim);
for iSpecies = 1 : length(PWSCCList)
     
    % read feature
    frogSpecies = PWSCCList(iSpecies).name;
    frogPath = strrep(featureFolderName, 'fileName', frogSpecies);
    
    fid = fopen(frogPath);
    out = textscan(fid, '%s %s %s %s %s %s %s ', 'delimiter', ',');
    fclose(fid);
    
    % put features of different frog species together
    len = length(out{1});
    frogFeature = zeros(len - 1, dim);
    
    for i = 1: dim
        frogFeature(:, i) = str2double(out{i}(2: len));
    end    
    featureResult = vertcat(featureResult, frogFeature);
    
end

% remove the first row
featureResult(1,:) = [];

feature = [featureResult(:, 1), featureResult(:, 5:6)];
label = featureResult(:, 7);
 
speciesNumber = hist(label, 1:10);

frogAcc = zeros(10, 10);
% run the classifier 10 times
for iAcc = 1: 10
    cv = cvpartition( label, 'KFold', 10);
    % cv = cvpartition(label,'HoldOut', 10) ;
    
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
    
    outLabel = knnclassify(testingSample, trainingSample, trainingLabel, 5);
    
    cm = confusionmat(outLabel, testingLabel);
    [rMat, cMat] = size(cm);
    
    for i = 1:rMat
        frogAcc(iAcc, i) = cm(i, i) / sum(cm(:,i));
        
    end
     
    % acc = mean(outLabel == testingLabel);
end

meanAcc = mean(frogAcc, 1);
stdAcc = std(frogAcc, 1);

disp(mean(meanAcc))


% speciesRatio = speciesNumber / sum(speciesNumber);
% weightedAcc = sum(speciesRatio .* meanAcc);
% disp(weightedAcc)

meanResult = [meanAcc; stdAcc];
csvwrite('.\07-10\classification\SyllableFeature.csv', meanResult);


