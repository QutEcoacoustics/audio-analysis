clc; clear; close all;

target1075 = xlsread('C:\work\dataset\audio data\ICASSP-MIML\label\1075.xlsx');
target1075(target1075 == 0) = -1;

target1078 = xlsread('C:\work\dataset\audio data\ICASSP-MIML\label\1078.xlsx');
target1078(target1078 == 0) = -1;

target1079 = xlsread('C:\work\dataset\audio data\ICASSP-MIML\label\1079.xlsx');
target1079(target1079 == 0) = -1;

% combine target
target = vertcat(target1075, target1078, target1079)';
target = target(2:9,:);

label = target(1,:);

% obtain bags for shapes

csv1075Folder = '.\09-10\1075\shape\*.csv';
csv1075FolderName = '.\09-10\1075\shape\fileName';
csv1075List = dir(csv1075Folder);
csv1075List = csv1075List(arrayfun(@(x) ~strcmp(x.name(1), '.'), csv1075List));

len1075 = length(csv1075List);
bag1075 = cell(len1075, 1);

for indexA = 1:len1075
    disp(indexA);
    csv1075Name = csv1075List(indexA);
    csvPath = strrep( csv1075FolderName, 'fileName', csv1075Name.name );
    
    shapeFeature = csvread(csvPath);
    bag1075(indexA, 1) = num2cell(shapeFeature, [1 2]);
    
end

csv1078Folder = '.\09-10\1078\shape\*.csv';
csv1078FolderName = '.\09-10\1078\shape\fileName';
csv1078List = dir(csv1078Folder);
csv1078List = csv1078List(arrayfun(@(x) ~strcmp(x.name(1), '.'), csv1078List));

len1078 = length(csv1078List);
bag1078 = cell(len1078, 1);

for indexA = 1:len1078
    disp(indexA);
    csv1078Name = csv1078List(indexA);
    csvPath = strrep( csv1078FolderName, 'fileName', csv1078Name.name );
    
    shapeFeature = csvread(csvPath);
    bag1078(indexA, 1) = num2cell(shapeFeature, [1 2]);
    
end


csv1079Folder = '.\09-10\1079\shape\*.csv';
csv1079FolderName = '.\09-10\1079\shape\fileName';
csv1079List = dir(csv1079Folder);
csv1079List = csv1079List(arrayfun(@(x) ~strcmp(x.name(1), '.'), csv1079List));

len1079 = length(csv1079List);
bag1079 = cell(len1079, 1);

for indexA = 1:len1079
    disp(indexA);
    csv1079Name = csv1079List(indexA);
    csvPath = strrep( csv1079FolderName, 'fileName', csv1079Name.name );
    
    shapeFeature = csvread(csvPath);
    bag1079(indexA, 1) = num2cell(shapeFeature, [1 2]);
    
end

bag = [bag1075; bag1078; bag1079];

% 5 fold cross validation
cv = cvpartition( label, 'KFold', 5);
index = zeros(size(label));
nFold = 5;

for k = 1:nFold
    index(cv.test(k)) = k;
end

testIndex = (index == k);
trainIndex = ~testIndex;


train_bags = bag(trainIndex);
train_target = target(:, trainIndex);

test_bags = bag(testIndex);
test_target = target(:, testIndex);  


ratio=0.2; %parameter "k" is set to be 20% of the number of training bags

%Suppose Gaussian kernel SVM are used:

svm.type='RBF';
svm.para=0.6;%the value of "gamma"
cost=1;% the value of "C"

%call MLMLSVM
[HammingLoss,RankingLoss,OneError,Coverage,Average_Precision,Outputs,Pre_Labels]=MIMLSVM(train_bags,train_target,test_bags,test_target,ratio,svm,cost);

disp(HammingLoss);
disp(RankingLoss);
disp(OneError);
disp(Coverage);
disp(Average_Precision);


