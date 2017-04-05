% classification using Mel scale WPD with KNN

clc; close all; clear;
% read csv files for loading features

pathMelCC = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-04\original\MelCC';
featureFolderName = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-04\original\MelCC\fileName';
MelCCCCList = dir(pathMelCC);
MelCCCCList = MelCCCCList(arrayfun(@(x) ~strcmp(x.name(1), '.'), MelCCCCList));

featureResult = zeros(1, 13);
for iSpecies = 1 : length(MelCCCCList)
    
    % read feature
    frogSpecies = MelCCCCList(iSpecies).name;
    frogPath = strrep(featureFolderName, 'fileName', frogSpecies);
    
    fid = fopen(frogPath);
    out = textscan(fid, '%s %s %s %s %s %s %s %s %s %s %s %s %s', 'delimiter', ',');
    fclose(fid);
    
    % put features of different frog species together
    len = length(out{1});
    frogFeature = zeros(len - 1, 13);
    
    for i = 1: 13
        frogFeature(:, i) = str2double(out{i}(2: len));
    end    
    featureResult = vertcat(featureResult, frogFeature);
    
end

% remove the first row
featureResult(1,:) = [];

feature = featureResult(:, 1:12);


%-------------------------------------------%
pathSF = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-04\syllableFeature';
featureFolderName = 'C:\Users\Jie\Documents\MATLAB\Tasks\E-science conference\07-04\syllableFeature\fileName';
SFList = dir(pathSF);
SFList = SFList(arrayfun(@(x) ~strcmp(x.name(1), '.'), SFList));

sfResult = zeros(1, 7);
for iSpecies = 1 : length(SFList)
    
    % read feature
    frogSpecies = SFList(iSpecies).name;
    frogPath = strrep(featureFolderName, 'fileName', frogSpecies);
    
    fid = fopen(frogPath);
    out = textscan(fid, '%s %s %s %s %s %s %s', 'delimiter', ',');
    fclose(fid);
    
    % put features of different frog species together
    len = length(out{1});
    frogFeature = zeros(len - 1, 7);
    
    for i = 1: 7
        frogFeature(:, i) = str2double(out{i}(2: len));
    end    
    sfResult = vertcat(sfResult, frogFeature);
    
end

sfResult(1,:) = [];

sfRes =[(sfResult(:,1) - min(sfResult(:,1))) / range(sfResult(:,1)) , ...
    (sfResult(:,5) - min(sfResult(:,5))) / range(sfResult(:,5))] ;

feature = [featureResult(:, 1:12), sfRes];

label = featureResult(:, 13);

speciesNumber = hist(label, 1:24);

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

% disp(meanAcc)
mean(meanAcc)

% speciesRatio = speciesNumber / sum(speciesNumber);
% weightedAcc = sum(speciesRatio .* meanAcc);
% disp(weightedAcc)