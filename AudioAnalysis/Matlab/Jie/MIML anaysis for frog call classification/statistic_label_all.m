% statistic label for all three sites

clc; close all; clear;

labelPath1 = 'C:\Users\Jie\Google Drive\work\my_paper\MIML_AED_SPT_RG for classification\label\1075.xlsx';

labelPath2 = 'C:\Users\Jie\Google Drive\work\my_paper\MIML_AED_SPT_RG for classification\label\1078.xlsx';

labelPath3 = 'C:\Users\Jie\Google Drive\work\my_paper\MIML_AED_SPT_RG for classification\label\1079.xlsx';

code = {'CTD', 'LNA', 'LFX', 'LLA', 'LRI', 'LRA', 'CNE', 'LTE', 'UMA'};

% site 1
[numLabel1, ~, ~] = xlsread(labelPath1);
sumSpecies1 = numLabel1(:,3);

[M1,N1] = size(numLabel1);
labelMatrix1 = numLabel1(:,3:N1);

totalPerSpecies1 = sum(labelMatrix1,1);

% site 2
[numLabel2, ~, ~] = xlsread(labelPath2);
sumSpecies2 = numLabel2(:,3);

[M2,N2] = size(numLabel2);
labelMatrix2 = numLabel2(:,3:N2);

totalPerSpecies2 = sum(labelMatrix2,1);

% site 3
[numLabel3, ~, ~] = xlsread(labelPath3);
sumSpecies3 = numLabel3(:,3);

[M3,N3] = size(numLabel3);
labelMatrix3 = numLabel3(:,3:N3);

totalPerSpecies3 = sum(labelMatrix3,1);


% total
totalSumSpecies = vertcat(sumSpecies1, sumSpecies2, sumSpecies3);

perSpecies = vertcat(totalPerSpecies1, totalPerSpecies2, totalPerSpecies3);
sumPerSpecies = sum(perSpecies, 1);

%--------------%
figure;
subplot(2,1,1);

ax = gca;
bar(sumPerSpecies(2:9),'cyan','EdgeColor',[0,0,0]);
xlabel('Frog species');
ylabel('No. of recordings');
title('No. of recordings that include the particular frog species')

set(ax,'XTickLabel',code);

% disp('The total number of recorings is')
% disp(M);
% plot the number of species in each recording

subplot(2,1,2);
[counts,centers] = hist(totalSumSpecies, 0:max(totalSumSpecies));
bar(centers,counts,'FaceColor',[0 0.5 0.5],'EdgeColor','w');

xlabel('No. of frog species');
ylabel('No. of recordings');
title('No. of recordings that include different number of frog species')
