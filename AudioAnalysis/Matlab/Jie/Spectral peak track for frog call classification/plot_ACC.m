% read the classification result and plot for six feature sets
%%
clc; clear; close all;
 
audioPath = '.\05-21\*.csv';
featureFolderName = '.\05-21\fileName';
accList = dir(audioPath);
accList = accList(arrayfun(@(x) ~strcmp(x.name(1), '.'), accList));

accMeanResult = zeros(5, 24);
accStdResult = zeros(5, 24);
for iAcc = 1 : length(accList)
    
    frogSpecies = accList(iAcc).name;    
    frogPath = strrep(featureFolderName, 'fileName', frogSpecies);
    acc = csvread(frogPath);
 
    meanAcc = acc(1,:);
    stdAcc = acc(2,:);
 
    accMeanResult(iAcc,:) = meanAcc;
    accStdResult(iAcc,:) = stdAcc;
    
end
 
%  
accMe = accMeanResult';
accSD = accStdResult';

accResult = cell(25, 6);
for i = 2:25
    for j = 2:6
        accResult{i, j} = strcat( num2str(accMe(i-1, j-1)*100,'%5.2f%%'),  '±',  num2str(accSD(i-1, j-1) * 100, '%5.2f%%'));                
    end
end  
 
accFinal = accResult;
accFinal(2:25,5) = accResult(2:25,6);
accFinal(2:25,6) = accResult(2:25,5);
accResult = accFinal;

% add row and column name to the acc result
label = {'ADI','CPA','CSA','LCS', 'LOS', 'LTS','LTE','LCA','LCS','LLA','LNA','LEA','LRA','LTI','LVV','MFS','MFI','NSI','PKN','PSS','PCA','PRI','UFA','ULA'};

accResult(2:25, 1) = label;

name = {'Frog code', 'MFCC', 'Han [21]', 'Huang [18]', 'Xie [22]', 'Syllable Feature + Entropy'};
accResult(1, 1:6) = name;

% cell to CSV
ds = cell2dataset(accResult);
export(ds,'file','.\05-21\acc.csv','delimiter',',')

  
%%
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

frogName = cell(24,1);
for i = 1:length(audioList)
    frog = audioList(i).name(1:(length(audioList(i).name) - 4));
    frogName{i,1} = frog;
end

frogResult = cell(25, 5);

frogResult(2:25, 1) = {'1','2','3','4','5','6','7','8','9','10','11','12','13','14','15','16','17','18','19','20','21','22','23','24'};

label = {'ADI','CPA','CSA','LCS', 'LOS', 'LTS','LTE','LCA','LCS','LLA','LNA','LEA','LRA','LTI','LVV','MFS','MFI','NSI','PKN','PSS','PCA','PRI','UFA','ULA'};
frogResult(2:25, 5) = label;

frogResult(2:25, 2) = frogName;

instance = {'36','32','13','56','62','16','53','76','35','157','74','130','37','71','28','33','27','22','21','27','68','25','39','25'};

frogResult(2:25, 4) = instance;

commonName = {'Pouched frog', 'Eastern Sign-bearing Froglet', 'Common eastern froglet', 'Marbled frog', 'Ornate burrowing frog',...
                            'Spotted grass frog', 'Northern banjo frog', 'Australian green tree frog', 'Red-eyed tree frog', 'Broad-palmed frog',...
                           'Striped rocket frog',' Revealed tree frog','Desert tree frog', ' Southern laughing tree frog', 'Whistling tree frog',...
                           'Great barred frog', 'Fleay_s Barred Frog', 'Painted burrowing frog', 'Mountain frog', 'Sphagnum frog',...
                           'Red-backed toadlet', 'Copper-backed brood frog', 'Dusky toadlet', 'Smooth toadlet'};
frogResult(2:25, 3) = commonName;

frogResult(1, 1:5) = {'No.', 'Scientific name','Common name', 'Instance', 'Code'};

dsFrog = cell2dataset(frogResult);
export(dsFrog,'file','.\05-02\frogTable.csv','delimiter',',')


%%
clc; clear; close all;
meanAcc40 = [ 54.04, 56.19, 85.16, 87.63, 92.14];
sdAcc40 = [14.93, 21.08, 13.92, 11.17, 7.4];

meanAcc30 = [44.78, 48.01, 84.71, 85.76, 88.75];
sdAcc30 = [18.12, 19.17, 14.07, 11.83, 11.43];

meanAcc20 = [32.80, 39.94, 75.18, 80.74, 82.79];
sdAcc20 = [17.12, 18.76, 15.94, 15.34, 14.52];

meanAcc10 = [15.10, 19.61, 59.49, 60.49, 60.94];
sdAcc10 = [9.43, 12.83, 19.51, 19.23, 21.18];

barwitherr(  [sdAcc40' sdAcc30' sdAcc20' sdAcc10'], 1:5, [meanAcc40' meanAcc30' meanAcc20' meanAcc10' ]);


a1 = 'Proposed feature';
a2 = 'Xie [22]';
a3 =  'Huang [18]' ;
a4 = 'Han [21]'     ; 
a5 = 'MFCCs'        ; 

feature = {a5, a4, a3, a2, a1};

set(gca, 'xtick', 1:5, 'XTickLabel', feature);

h_legend = legend('SNR-40', 'SNR-30', 'SNR-20', 'SNR-10');

ylabel(' Classification accuracy (%)');
xlabel('Feature set')
 
export_fig '.\05-21\plot\SNR.png'


%%
clc; clear; close all;
% meanAcc40 = [ 54.04, 56.19, 85.16, 87.63, 92.14];
% sdAcc40 = [14.93, 21.08, 13.92, 11.17, 7.4];
% 
% meanAcc30 = [44.78, 48.01, 84.71, 85.76, 88.75];
% sdAcc30 = [18.12, 19.17, 14.07, 11.83, 11.43];
% 
% meanAcc20 = [32.80, 39.94, 75.18, 80.74, 82.79];
% sdAcc20 = [17.12, 18.76, 15.94, 15.34, 14.52];
% 
% meanAcc10 = [15.10, 19.61, 59.49, 60.49, 60.94];
% sdAcc10 = [9.43, 12.83, 19.51, 19.23, 21.18];

meanMFCCs = [54.04, 44.78, 32.80, 15.10];
sdMFCCs = [14.93, 18.12, 17.12, 9.43];

meanHan = [56.19, 48.01, 39.94, 19.61];
sdHan = [21.08, 19.17, 18.76, 12.83];

meanHuang = [ 85.16,  84.71,75.18, 59.49];
sdHuang = [13.92,14.07, 15.94, 19.51];

meanXie = [ 87.63 , 85.76, 80.74, 60.49 ];
sdXie = [11.17, 11.83, 15.34, 19.23];

meanOur = [92.14, 88.75,  82.79, 60.94]; 
sdOur = [7.4, 11.43, 14.52, 21.18];


barwitherr(  [sdMFCCs' sdHan' sdHuang' sdXie' sdOur'], 1:4, [meanMFCCs' meanHan' meanHuang' meanXie' meanOur' ]);

a4 = 'SNR-40'; 
a3 = 'SNR-30'; 
a2 = 'SNR-20';
a1 = 'SNR-10';

feature = {a4, a3, a2, a1};

set(gca, 'xtick', 1:4, 'XTickLabel', feature);

h_legend = legend('MFCCs', 'Han [21]' , 'Huang [18]' ,'Xie [22]', 'Proposed feature' );

ylabel(' Classification accuracy (%)');
xlabel('Different singal to noise ratio')
 
export_fig '.\05-21\plot\SNR.png'
