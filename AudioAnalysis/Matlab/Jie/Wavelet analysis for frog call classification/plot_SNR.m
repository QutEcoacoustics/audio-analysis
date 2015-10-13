% plot classification accuracy of different SNR
clc; clear; close all;

meanPWSCC = [90.67, 93.83, 93.96  95.93, 96.26];
sdPWSCC = [12.31, 8.16, 6.40,7.65, 6.32];

% meanMWSCC = [70.21, 77.23, 80.20, 93.51];
% sdMWSCC = [6,6,5,3];

meanMFCC = [46.96, 59.38, 77.00, 85.11, 90.69];
sdMFCC = [21.23, 18.66, 15.48, 13.29, 8.77];

errorbar(1:5, meanPWSCC, sdPWSCC, 'linewidth', 2);
hold on;
% errorbar(1:4, meanMWSCC, sdMWSCC,  'linewidth', 2);
% hold on;
errorbar(1:5, meanMFCC, sdMFCC, 'linewidth', 2);

a4 = 'SNR-10'; 
a3 = 'SNR-20'; 
a2 = 'SNR-30';
a1 = 'SNR-40';
a0 = 'SNR-50';

feature = {a4, a3, a2, a1, a0};

set(gca, 'xtick', 1:5, 'XTickLabel', feature);

h_legend = legend('PWSCC',  'MFCCs');

ylabel(' Classification accuracy (%)');

export_fig '.\07-15\SNR.pdf'

