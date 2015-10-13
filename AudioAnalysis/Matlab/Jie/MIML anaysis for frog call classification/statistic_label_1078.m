% calculate the number of frog species in each recording in particular day
% calculate the number of frog species in each recording for all days
% calculate the distribution of frog species
clc; close all; clear;

labelPath = 'C:\Users\Jie\Google Drive\work\my_paper\MIML_AED_SPT_RG for classification\label\1078.xlsx';

code = {'CTD', 'LNA', 'LFX', 'LLA', 'LRI', 'LRA', 'CNE', 'LTE', 'UMA'};

[numLabel, textLabel, rawLabel] = xlsread(labelPath);

sumSpecies = numLabel(:,3);

[M,N] = size(numLabel);
labelMatrix = numLabel(:,3:N);

totalPerSpecies = sum(labelMatrix,1);

% the distribution of each frog species
figure;
subplot(2,1,1);

ax = gca;
bar(totalPerSpecies(2:9),'cyan','EdgeColor',[0,0,0]);
xlabel('Frog species');
ylabel('# of recordings');
title('# of recordings that include the particular frog species')

set(ax,'XTickLabel',code);

disp('The total number of recorings is')
disp(M);
% plot the number of species in each recording

subplot(2,1,2);
[counts,centers] = hist(sumSpecies, 0:max(sumSpecies));
bar(centers,counts,'FaceColor',[0 0.5 0.5],'EdgeColor','w');

xlabel('# of frog species');
ylabel('# of recordings');
title('# of recordings that include different number of frog species')