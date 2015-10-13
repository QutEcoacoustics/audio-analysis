% use five acoustic events to calculate the mean and standard deviation for
% each frog species.
clc; clear; close all;


paraFolder = 'C:\work\dataset\audio data\MIML\validation set for parameter setting\label for bioacoustic JCU data\*.xlsx';
paraFolderName = 'C:\work\dataset\audio data\MIML\validation set for parameter setting\label for bioacoustic JCU data\fileName';

paraList = dir(paraFolder);
paraList = paraList(arrayfun(@(x) ~strcmp(x.name(1), '.'), paraList));

duration = zeros(8, 2);
lowFreq = zeros(8, 2);
highFreq = zeros(8, 2);
peakFreq = zeros(8, 2);

for index = 1:length(paraList)
    
    paraName = paraList(index);
    paraPath = strrep( paraFolderName, 'fileName', paraName.name );
    
    % read xlsx paper
    [num,txt,raw] = xlsread(paraPath);
    
    start = num(:, 1);
    stop = num(:, 2);
    dur = stop - start;
    lowF = num(:, 3);
    highF = num(:, 4);
    peakF = num(:, 5);
    
    % mean and std
    duration(index, 1) = mean(dur);
    duration(index, 2) = std(dur);
    
    lowFreq(index, 1) = mean(lowF);
    lowFreq(index, 2) = std(lowF);
    
    highFreq(index, 1) = mean(highF);
    highFreq(index, 2) = std(highF);
    
    peakFreq(index, 1) = mean(peakF);
    peakFreq(index, 2) = std(peakF);
        
end


outDuration = [duration(:, 1), duration(:, 2)];
csvwrite('.\durationFilter.csv', outDuration);

outPeakFreq = [peakFreq(:, 1), peakFreq(:, 2)];
csvwrite('.\freqFilter.csv', outPeakFreq);



% plot the mean and std for duration
figure;
subplot(2,2,1)
ax = gca;
barwitherr(duration(:,2) ,duration(:,1));

xlabel('Frog species');
ylabel('Duration (Samples)');
code = {'CTD', 'LNA', 'LFX', 'LRI', 'LRA', 'CNE', 'LTE', 'UMA'};
set(ax,'XTickLabel',code);
rotateXLabels(ax, 90);

% plot the mean and std for low frequecny
subplot(2,2,2)
bx = gca;
barwitherr(lowFreq(:,2) ,lowFreq(:,1)); 

xlabel('Frog species');
ylabel('Low frequency (Hz)');
code = {'CTD', 'LNA', 'LFX', 'LRI', 'LRA', 'CNE', 'LTE', 'UMA'};
set(bx,'XTickLabel',code);
rotateXLabels(bx, 90);

% plot the mean and std for high frequecny
subplot(2,2,3)
cx = gca;
barwitherr(highFreq(:,2) ,highFreq(:,1));

ylabel('High frequency (Hz)');
xlabel('Frog species');
code = {'CTD', 'LNA', 'LFX', 'LRI', 'LRA', 'CNE', 'LTE', 'UMA'};
set(cx,'XTickLabel',code);
rotateXLabels(cx, 90);

% plot the mean and std for peak frequency
subplot(2,2,4)
dx = gca;
barwitherr(peakFreq(:,2) ,peakFreq(:,1));

ylabel('Peak frequency (Hz)');
xlabel('Frog species');
code = {'CTD', 'LNA', 'LFX', 'LRI', 'LRA', 'CNE', 'LTE', 'UMA'};
set(dx,'XTickLabel',code);
rotateXLabels(dx, 90);

colormap(cool);

export_fig '.\08-28\plot\parameter.pdf'

csvwrite('.\08-28\highFreq.csv', highFreq(:,1));
csvwrite('.\08-28\lowFreq.csv', lowFreq(:,1));
csvwrite('.\08-28\peakFreq.csv', peakFreq(:,1));


