% classification using KNN
clc; close all; clear;
% read csv files for loading features

pathMFCC = '.\05-01\syllableFeature\*.csv';
featureFolderName = '.\05-01\syllableFeature\fileName';
MFCCList = dir(pathMFCC);
MFCCList = MFCCList(arrayfun(@(x) ~strcmp(x.name(1), '.'), MFCCList));

NUM = 24;

featureResult = zeros(1, NUM);
for iSpecies = 1 : length(MFCCList)
    
    % read feature
    frogSpecies = MFCCList(iSpecies).name;
    frogPath = strrep(featureFolderName, 'fileName', frogSpecies);
    
    fid = fopen(frogPath);
    out = textscan(fid, '%s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s %s', 'delimiter', ',');
    fclose(fid);
    
    % put features of different frog species together
    len = length(out{1});
    frogFeature = zeros(len - 1, NUM);
    
    for i = 1: NUM
        frogFeature(:, i) = str2double(out{i}(2: len));
    end
    featureResult = vertcat(featureResult, frogFeature);
    
end

% remove the first row
featureResult(1,:) = [];

feature = [featureResult(:, 1), featureResult(:, 5:7)] ;

% normalization
feature = normc(feature);
label = featureResult(:, NUM);

NUM = 24;
speciesNumber = hist(label, 1:NUM);

frogAcc = zeros(10, NUM);
% run the classifier 10 times

functionKNN = {'euclidean',  'cityblock' , 'cosine' , 'correlation' };
accFindex = zeros(10, length(functionKNN));

for ff = 1:length(functionKNN)
    
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
        
        outLabel = knnclassify(testingSample, trainingSample, trainingLabel, 3, functionKNN{1,ff});
        
        cm = confusionmat(outLabel, testingLabel);
        
        [rMat, cMat] = size(cm);
        
        for i = 1:rMat
            frogAcc(iAcc, i) = cm(i, i) / sum(cm(:,i));
        end
        
    end  
    
    accFindex(:, ff) = mean(frogAcc, 2);
    
end
 
plot(accFindex');
xlabel('Selection of function for k-NN classifier');
ylabel('Classification accuracy');
set(gca, 'XTickLabel', {'Euclidean'; ' ' ;'Cityblock' ; ' ' ; 'Cosine' ; ' ' ; 'Correlation' });

export_fig '.\05-01\plot\function_selection' -png

